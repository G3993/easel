#include "sources/FluidSource3D.h"
#include "app/MIDIManager.h"
#include <GLFW/glfw3.h>
#include <iostream>
#include <string>
#include <algorithm>
#include <cstdlib>
#include <cmath>

// ─────────────────────────────────────────────────────────────────────
// GLSL 330 core. Port of shadertoy mstfzS "Particle cluster grid SPH 3D".
// Adapted from the shadertoy's 2D-tiled-3D addressing to real 3D textures:
//   - particle voxels are RGBA32UI (exact bit-packed; usampler3D + texelFetch)
//   - the 2D-tiling helpers (dim2from3/dim3from2/voxel/pixel/SCALE) are
//     dropped; cell address = (gl_FragCoord.xy, uSlice), trilinear is a
//     native sampler3D LINEAR fetch.
//   - InitGrid is replaced by size3d = vec3(uSize).
// The SPH constants that are worth playing with (dt, rest density, max
// velocity, friction, tension, wall stiffness) are uniforms whose defaults
// reproduce the shadertoy exactly; every extra force is formulated so that
// 0 == original behaviour.
// ─────────────────────────────────────────────────────────────────────

static const char* VS_FULL = R"(#version 330 core
layout(location=0) in vec2 position;
out vec2 vUv;
void main(){ vUv = position*0.5+0.5; gl_Position = vec4(position,0.0,1.0); })";

// Layered sim VS+GS: one instanced draw (instance = Z slice) writes the
// whole 3D volume in a single render pass via gl_Layer — avoids the
// per-slice FBO reattach that murders perf on tile-based / Metal GL.
static const char* VS_LAYER = R"(#version 330 core
layout(location=0) in vec2 position;
flat out int vLayer;
void main(){ vLayer = gl_InstanceID; gl_Position = vec4(position,0.0,1.0); })";

static const char* GS_LAYER = R"(#version 330 core
layout(triangles) in;
layout(triangle_strip, max_vertices=3) out;
flat in int vLayer[];
flat out int gLayer;
void main(){
    for(int i = 0; i < 3; i++){
        gl_Layer = vLayer[0];
        gLayer   = vLayer[0];
        gl_Position = gl_in[i].gl_Position;
        EmitVertex();
    }
    EndPrimitive();
})";

// Shared chunk prepended to every pass (after #version).
static const char* COMMON = R"(
uniform float uSize;
uniform int   uFrame;
uniform float uTime;
uniform float uEnergy;   // audio-driven motion intensity (1 = original)
// Extra force fields — all formulated so 0 == original behaviour, so an
// unbound uniform can never break the sim.
uniform float uGravityAdd;  // gravity strength delta (0 = original 1g)
uniform vec3  uGravityDir;  // gravity direction (0,0,-1 = original)
uniform float uVortex;      // coherent swirl around the vertical axis (0 = off)
uniform float uVortexLift;  // tornado lift along the axis
uniform float uTurbulence;  // chaotic turbulent push (0 = off)
uniform float uTurbScale;   // turbulence noise scale (1 = original)
uniform float uTurbSpeed;   // turbulence animation speed (1 = original)
uniform float uForceAdd;    // SPH inter-particle (cohesion) force delta (0 = original)
uniform float uSphereShape; // container shape: 0 = cube (original), 1 = sphere
uniform float uFill;        // how much of the container is seeded with fluid (0..1)
uniform float uSeedDouble;  // 1 = seed two particles per cell
// SPH constants promoted to uniforms (defaults == the shadertoy #defines).
uniform float uDt;          // 0.7
uniform float uRestDensity; // 1.0
uniform float uMaxVel;      // 2.0
uniform float uViscosity;   // friction multiplier (1)
uniform float uTension;     // surface tension multiplier (1)
uniform float uBounce;      // wall stiffness multiplier (1)
uniform float uDamping;     // extra damping 0..1 (0 = none)
uniform vec3  uWind;        // wind force (gravity units)
uniform float uGust;        // wind oscillation amount
uniform float uAttract;     // point attractor strength (−repel .. +attract)
uniform vec3  uAttractPos;  // normalized 0..1
// Lifetime + emitter.
uniform float uLifeP;       // per-frame probability of an age tick (0 = immortal)
uniform float uEmitRate;    // 0 = emitter off
uniform int   uEmitShape;   // 0 point 1 rain 2 fountain 3 ring
uniform vec3  uEmitPos;     // normalized 0..1
uniform float uEmitRadius;  // normalized
uniform vec3  uEmitVel;     // initial velocity (cells/step)
uniform float uEmitSpread;  // random velocity jitter 0..1
// Orbs as physics objects: xyz = centre (grid units), w = radius;
// mode.x = 0 none / 1 solid / 2 attract / 3 repel, mode.y = strength.
uniform int   uSimOrbCount;
uniform vec4  uSimOrb[4];
uniform vec2  uSimOrbMode[4];

vec3 size3d;

#define PI 3.1415926535
#define TWO_PI 6.28318530718
uniform vec3 uLightDir;
#define light_dir normalize(uLightDir)
#define surface_tension 0.5
#define surface_tension_rad 2.0
#define initial_particle_density 1u
#define dt uDt
#define rest_density uRestDensity
#define gravity 0.01
#define force_k 0.15
#define force_coef_a -4.0
#define force_boundary 5.0
#define max_velocity uMaxVel
#define GD(x, R) (exp(-dot((x)/(R),(x)/(R)))/((R)*(R)))
#define range(i,a,b) for(int i=a;i<=b;i++)
#define MAX_MASS 31u

ivec3 cellClamp(vec3 p){ return clamp(ivec3(p), ivec3(0), ivec3(int(uSize)-1)); }
uvec4 LOAD3D(usampler3D ch, vec3 p){ return texelFetch(ch, cellClamp(p), 0); }
vec4  LOAD3D(sampler3D ch, vec3 p){ return texelFetch(ch, cellClamp(p), 0); }
vec4  trilinear(sampler3D ch, vec3 p){ return texture(ch, (p)/vec3(uSize)); }

struct Particle { uint mass; uint age; vec3 pos; vec3 vel; vec3 force; float density; };

// 5 bits shared exponent, 9 bits per component.
uint packvec3(vec3 v){
    float maxv = max(abs(v.x), max(abs(v.y), abs(v.z)));
    int expo = clamp(int(ceil(log2(maxv))), -15, 15);
    float scale = exp2(-float(expo));
    uvec3 sv = uvec3(round(clamp(v*scale, -1.0, 1.0) * 255.0) + 255.0);
    return uint(expo + 15) | (sv.x << 5) | (sv.y << 14) | (sv.z << 23);
}
vec3 unpackvec3(uint pk){
    int expo = int(pk & 0x1Fu) - 15;
    vec3 sv = vec3((pk >> 5) & 0x1FFu, (pk >> 14) & 0x1FFu, (pk >> 23) & 0x1FFu);
    vec3 v = (sv - 255.0) / 255.0;
    v *= exp2(float(expo));
    return v;
}

// data1: mass0 (5b) | age0 (3b) | mass1 (5b) | age1 (3b) | pos0.x (8b) | pos0.y (8b)
uvec4 packParticles(Particle p0, Particle p1, vec3 pos){
    p0.pos -= pos; p1.pos -= pos;
    uvec3 pos0 = uvec3(clamp(p0.pos, 0.0, 1.0) * 255.0);
    uvec3 pos1 = uvec3(clamp(p1.pos, 0.0, 1.0) * 255.0);
    uint data1 = min(p0.mass, MAX_MASS) | ((p0.age & 7u) << 5)
               | (min(p1.mass, MAX_MASS) << 8) | ((p1.age & 7u) << 13)
               | (pos0.x << 16) | (pos0.y << 24);
    uint data2 = pos0.z | (pos1.x << 8) | (pos1.y << 16) | (pos1.z << 24);
    uint data3 = packvec3(p0.vel);
    uint data4 = packvec3(p1.vel);
    return uvec4(data1, data2, data3, data4);
}
void unpackParticles(uvec4 d, vec3 pos, out Particle p0, out Particle p1){
    p0.mass = d.x & 0x1Fu;
    p0.age  = (d.x >> 5) & 7u;
    p1.mass = (d.x >> 8) & 0x1Fu;
    p1.age  = (d.x >> 13) & 7u;
    uvec3 pos0 = uvec3((d.x >> 16) & 0xFFu, (d.x >> 24) & 0xFFu, d.y & 0xFFu);
    uvec3 pos1 = uvec3((d.y >> 8) & 0xFFu, (d.y >> 16) & 0xFFu, (d.y >> 24) & 0xFFu);
    p0.pos = vec3(pos0) / 255.0 + pos;
    p1.pos = vec3(pos1) / 255.0 + pos;
    p0.vel = unpackvec3(d.z);
    p1.vel = unpackvec3(d.w);
    // Not packed — zero so density/force accumulators start clean.
    p0.force = vec3(0.0); p1.force = vec3(0.0);
    p0.density = 0.0;     p1.density = 0.0;
}

vec2 hash23(vec3 p3){
    p3 = fract(p3 * vec3(.1031, .1030, .0973));
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.xx + p3.yz) * p3.zy);
}
vec3 udir(vec2 rng){
    float phi = 2.*PI*rng.x;
    float ctheta = 2.*rng.y - 1.;
    float stheta = sqrt(1.0 - ctheta*ctheta);
    return vec3(cos(phi)*stheta, sin(phi)*stheta, ctheta);
}

int ClosestCluster(Particle p0, Particle p1, Particle incoming){
    if(float(p0.mass) < 0.01*float(p1.mass) || float(p1.mass) < 0.01*float(p0.mass))
        return p0.mass < p1.mass ? 0 : 1;
    float d0 = length(p0.pos - incoming.pos);
    float d1 = length(p1.pos - incoming.pos);
    return d0 < d1 ? 0 : 1;
}
void BlendParticle(inout Particle p, in Particle incoming){
    uint sum = p.mass + incoming.mass;
    vec2 weight = vec2(p.mass, incoming.mass) / float(sum);
    p.pos = p.pos*weight.x + incoming.pos*weight.y;
    p.vel = p.vel*weight.x + incoming.vel*weight.y;
    p.age = (p.age*p.mass + incoming.age*incoming.mass) / sum;
    p.mass = min(sum, MAX_MASS);
}
void Clusterize(inout Particle p0, inout Particle p1, in Particle incoming, vec3 pos){
    if(!all(equal(pos, floor(incoming.pos)))) return;
    int closest = ClosestCluster(p0, p1, incoming);
    if(closest == 0) BlendParticle(p0, incoming);
    else             BlendParticle(p1, incoming);
}
void SplitParticle(inout Particle p1, inout Particle p2){
    vec3 pos = p1.pos;
    uint newMass = p1.mass;
    p1.mass = newMass/2u;
    p2.mass = newMass - p1.mass;
    vec3 dir = udir(hash23(pos));
    p1.pos = pos - dir*5e-3;
    p2.pos = pos + dir*5e-3;
    p2.vel = p1.vel;
    p2.age = p1.age;
}

void ApplyForce(inout Particle p, in Particle incoming){
    float d = distance(p.pos, incoming.pos);
    vec3 dir = (incoming.pos - p.pos)/max(d, 1e-5);
    vec3 dvel = incoming.vel - p.vel;
    float f = force_coef_a*GD(d, 1.5);
    float irho = float(incoming.mass);
    float rho = 0.5*(p.density + incoming.density);
    float pressure = max(rho / rest_density - 1.0, -0.0);
    float SPH_F = f * pressure;
    float F = surface_tension*uTension*GD(d, surface_tension_rad);
    float Friction = 0.45*uViscosity * dot(dir, dvel) * GD(d, 1.5);
    p.force += (1.0 + uForceAdd) * force_k * dir * (F + SPH_F + Friction) * irho / rest_density;
}

float minv(vec3 a){ return min(min(a.x, a.y), a.z); }
float maxv(vec3 a){ return max(max(a.x, a.y), a.z); }
float distance2border(vec3 p){
    // Box SDF (inside-positive): distance to the nearest cube face.
    vec3 a = vec3(size3d - 1.) - p;
    float boxd = min(minv(p), minv(a)) + 1.;
    // Sphere SDF (inside-positive): distance to the inscribed sphere surface.
    // Blending the two gives a rounded-box continuum at intermediate values.
    vec3  c = size3d*0.5;
    float R = minv(size3d)*0.5 - 1.0;
    float sphd = R - length(p - c);
    return mix(boxd, sphd, uSphereShape);
}
vec4 border_grad(vec3 p){
    const float dx = 0.001;
    const vec3 k = vec3(1,-1,0);
    return (k.xyyx*distance2border(p + k.xyy*dx) +
            k.yyxx*distance2border(p + k.yyx*dx) +
            k.yxyx*distance2border(p + k.yxy*dx) +
            k.xxxx*distance2border(p + k.xxx*dx))/vec4(4.*dx,4.*dx,4.*dx,4.);
}
void IntegrateParticle(inout Particle p, vec3 pos, float time){
    // uEnergy scales the turbulent wobble: calm (small) when audio is quiet,
    // churning (large) when it's loud. uGravityAdd scales the pull along
    // uGravityDir (0 = 1g; -1 = zero-g float; >0 = heavier).
    vec3 gdir = uGravityDir;
    float gl = length(gdir);
    gdir = (gl > 1e-4) ? gdir/gl : vec3(0.0, 0.0, -1.0);
    p.force += gravity*vec3(0.4*sin(0.7*time)*uEnergy, 0.2*cos(0.5*time)*uEnergy, 0.0)
             + gravity*(1.0 + uGravityAdd)*gdir;
    // Wind: a steady push with an optional slow gust oscillation that
    // varies across the volume so the whole body doesn't shove at once.
    if(dot(uWind, uWind) > 0.0){
        float g = 1.0 + uGust*sin(1.3*time + 0.02*p.pos.x + 0.015*p.pos.y);
        p.force += gravity*uWind*g;
    }
    // Point attractor / repulsor (constant magnitude toward/away from a point).
    if(uAttract != 0.0){
        vec3 d = uAttractPos*size3d - p.pos;
        float l = length(d);
        p.force += gravity*2.0*uAttract*(d/max(l, 1e-3));
    }
    // Emissive orbs as physical objects: a solid sphere the fluid flows
    // around, or a point that pulls / pushes the particles.
    float orbAct = 0.0;
    for(int i = 0; i < 4; i++){
        if(i >= uSimOrbCount) break;
        vec3 d = p.pos - uSimOrb[i].xyz;
        float l = length(d);
        float r = uSimOrb[i].w;
        vec3 n = d / max(l, 1e-3);
        int mode = int(uSimOrbMode[i].x + 0.5);
        float s = uSimOrbMode[i].y;
        if(mode == 1){
            float gap = l - r;
            if(gap < 1.5){
                float vn = dot(p.vel, n);
                if(vn < 0.0) p.vel -= n*vn*1.2;          // no sinking into the sphere
            }
            p.force += n*force_boundary*s*exp(-0.4*max(gap, 0.0)*max(gap, 0.0));
        } else if(mode == 2){
            if(l > r) p.force -= n*gravity*2.0*s;
            else      p.force += n*gravity*1.0*s;         // hollow core: orbit the surface
            orbAct += abs(s)*0.5;
        } else if(mode == 3){
            p.force += n*gravity*2.0*s*exp(-l/(size3d.x*0.35));
            orbAct += abs(s)*0.5;
        }
    }
    // Coherent vortex: a tangential swirl around the vertical axis, applied as
    // a BOUNDED VELOCITY NUDGE (not an accumulating force, which used to spin
    // every particle up to max velocity and never let it stop). Self-limiting;
    // motion ceases the moment you set it back to 0.
    if(uVortex != 0.0){
        vec3 r  = p.pos - size3d*0.5;
        vec3 sw = vec3(-r.y, r.x, 0.0);
        float rl = length(sw);
        if(rl > 1e-4){
            vec3 target = (sw/rl) * (0.5 * uVortex);        // tangential stir speed
            // Tornado lift: strongest on the axis, fading out to the walls.
            target.z = uVortexLift * 0.4 * uVortex * (1.0 - clamp(rl/(0.5*size3d.x), 0.0, 1.0));
            p.vel = mix(p.vel, target, clamp(0.06*uVortex, 0.0, 0.4));
        }
    }
    // Chaotic turbulence: a DIVERGENCE-FREE flow (ABC-flow form — each
    // component depends only on the *other* two axes, so div = 0), applied as
    // a BOUNDED VELOCITY NUDGE rather than an accelerating force. Pushing it as
    // a force kept flinging every particle to max velocity ("flips out"); a
    // nudge toward a small target stir-velocity is self-limiting and stable,
    // and pulls energy back out when you dial it down.
    if(uTurbulence != 0.0){
        vec3 q = p.pos*0.35*uTurbScale;
        float t = time*uTurbSpeed;
        vec3 flow = vec3(
            sin(q.z + t)      + cos(q.y + 0.7*t),
            sin(q.x + 1.3*t)  + cos(q.z + t),
            sin(q.y + 0.7*t)  + cos(q.x + 1.3*t));
        vec3 target = flow * (0.12 * uTurbulence);          // gentle stir speed
        p.vel = mix(p.vel, target, clamp(0.08*uTurbulence, 0.0, 0.5));
    }
    // Global velocity damping — the key to recoverability. The original look
    // is essentially preserved at rest (a hair of 0.997), but the moment any
    // extra forcing is engaged (vortex / turbulence / heavier gravity /
    // stronger cohesion / wind / attractor / loud audio) the damping deepens
    // so energy can always bleed off. Without this, combining forces
    // saturates the sim into a permanent max-velocity chaos.
    float activity = uVortex + uTurbulence + abs(uGravityAdd) + abs(uForceAdd)
                   + length(uWind) + abs(uAttract) + orbAct + max(uEnergy - 1.0, 0.0);
    float damp = min(mix(0.985, 1.0, clamp(uEnergy, 0.0, 1.0)),
                     0.997 - 0.03*clamp(activity, 0.0, 1.5));
    p.vel *= clamp(damp, 0.94, 1.0) * (1.0 - 0.06*uDamping);
    vec4 border = border_grad(p.pos);
    vec3 bound = 1.*normalize(border.xyz)*exp(-0.4*border.w*border.w);
    p.force += force_boundary*uBounce*bound*dt;
    p.vel += p.force * dt;
    float v = length(p.vel)/max_velocity;
    p.vel /= (v > 1.) ? v : 1.;
}
)";

// ── Buffer A: advect + clusterize ─────────────────────────────────────
static const char* FS_A = R"(
uniform usampler3D uParticles;
flat in int gLayer;
out uvec4 fragColor;
void main(){
    size3d = vec3(uSize);
    vec3 pos = vec3(floor(gl_FragCoord.xy), float(gLayer));
    Particle p0, p1;
    p0.mass = 0u; p0.age = 0u; p0.pos = vec3(0); p0.vel = vec3(0);
    p1.mass = 0u; p1.age = 0u; p1.pos = vec3(0); p1.vel = vec3(0);
    range(i,-2,2) range(j,-2,2) range(k,-2,2){
        vec3 pos1 = pos + vec3(i,j,k);
        if(!all(lessThanEqual(pos1, size3d)) || !all(greaterThanEqual(pos1, vec3(0.0)))) continue;
        Particle p0_, p1_;
        unpackParticles(LOAD3D(uParticles, pos1), pos1, p0_, p1_);
        if(p0_.mass > 0u){ p0_.pos += p0_.vel*dt; Clusterize(p0, p1, p0_, pos); }
        if(p1_.mass > 0u){ p1_.pos += p1_.vel*dt; Clusterize(p0, p1, p1_, pos); }
    }
    if(p1.mass == 0u && p0.mass > 0u) SplitParticle(p0, p1);
    if(p0.mass == 0u && p1.mass > 0u) SplitParticle(p1, p0);
    fragColor = packParticles(p0, p1, pos);
})";

// ── Buffer B: SPH densities ───────────────────────────────────────────
static const char* FS_B = R"(
uniform usampler3D uParticles;
flat in int gLayer;
out vec4 fragColor;
void AddDensity(inout Particle p, in Particle incoming, float rad){
    if(incoming.mass == 0u) return;
    float d = distance(p.pos, incoming.pos);
    float irho = float(incoming.mass);
    p.density += 0.25*irho*GD(d, rad);
}
void main(){
    size3d = vec3(uSize);
    vec3 pos = vec3(floor(gl_FragCoord.xy), float(gLayer));
    Particle p0, p1, pV;
    unpackParticles(LOAD3D(uParticles, pos), pos, p0, p1);
    pV.mass = 0u; pV.age = 0u; pV.density = 0.0; pV.pos = pos + 0.5;
    range(i,-2,2) range(j,-2,2) range(k,-2,2){
        if(i==0 && j==0 && k==0) continue;
        vec3 pos1 = pos + vec3(i,j,k);
        Particle p0_, p1_;
        unpackParticles(LOAD3D(uParticles, pos1), pos1, p0_, p1_);
        if(p0.mass > 0u){ AddDensity(p0, p0_, 1.5); AddDensity(p0, p1_, 1.5); }
        if(p1.mass > 0u){ AddDensity(p1, p0_, 1.5); AddDensity(p1, p1_, 1.5); }
        AddDensity(pV, p0_, 1.2); AddDensity(pV, p1_, 1.2);
    }
    if(p0.mass > 0u){ AddDensity(p0, p0, 1.5); AddDensity(p0, p1, 1.5); }
    if(p1.mass > 0u){ AddDensity(p1, p0, 1.5); AddDensity(p1, p1, 1.5); }
    AddDensity(pV, p0, 1.2); AddDensity(pV, p1, 1.2);
    fragColor = vec4(p0.density, p1.density, pV.density, 0.0);
})";

// ── Buffer C: forces + integrate + age/emit + seed ────────────────────
static const char* FS_C = R"(
uniform usampler3D uParticles;
uniform sampler3D  uDensity;
flat in int gLayer;
out uvec4 fragColor;
void main(){
    size3d = vec3(uSize);
    vec3 pos = vec3(floor(gl_FragCoord.xy), float(gLayer));
    Particle p0, p1;
    unpackParticles(LOAD3D(uParticles, pos), pos, p0, p1);
    vec2 densities = LOAD3D(uDensity, pos).xy;
    p0.density = densities.x; p1.density = densities.y;
    if(p0.mass + p1.mass > 0u){
        range(i,-2,2) range(j,-2,2) range(k,-2,2){
            if(i==0 && j==0 && k==0) continue;
            vec3 pos1 = pos + vec3(i,j,k);
            Particle p0_, p1_;
            unpackParticles(LOAD3D(uParticles, pos1), pos1, p0_, p1_);
            vec2 d_ = LOAD3D(uDensity, pos1).xy;
            p0_.density = d_.x; p1_.density = d_.y;
            ApplyForce(p0, p0_); ApplyForce(p0, p1_);
            ApplyForce(p1, p0_); ApplyForce(p1, p1_);
        }
        ApplyForce(p0, p1); ApplyForce(p1, p0);
        IntegrateParticle(p0, pos, uTime);
        IntegrateParticle(p1, pos, uTime);
    }
    // Lifetime: each particle has a 3-bit age that ticks stochastically so the
    // EXPECTED lifetime is continuous; at age 7 the next tick kills it.
    if(uLifeP > 0.0){
        vec2 h = hash23(pos + vec3(float(uFrame)*0.37, 1.7, 3.1));
        if(p0.mass > 0u && h.x < uLifeP){ if(p0.age >= 7u) p0.mass = 0u; else p0.age++; }
        if(p1.mass > 0u && h.y < uLifeP){ if(p1.age >= 7u) p1.mass = 0u; else p1.age++; }
    }
    // Emitter: spawn fresh particles into empty slots inside the emitter
    // region (point / ceiling rain / floor fountain / ring).
    if(uEmitRate > 0.0 && uFrame >= 10){
        vec3 np = (pos + 0.5) / size3d;
        bool inEmit = false;
        if(uEmitShape == 0)      inEmit = length(np - uEmitPos) < uEmitRadius;
        else if(uEmitShape == 1) inEmit = np.z > 0.94 && length(np.xy - uEmitPos.xy) < uEmitRadius;
        else if(uEmitShape == 2) inEmit = np.z < 0.06 && length(np.xy - uEmitPos.xy) < uEmitRadius;
        else { float r = length(np.xy - uEmitPos.xy);
               inEmit = abs(r - uEmitRadius) < 0.04 && abs(np.z - uEmitPos.z) < 0.04; }
        if(inEmit){
            vec2 h = hash23(pos*1.7 + vec3(float(uFrame)*0.11, 5.3, 9.1));
            if(h.x < uEmitRate*0.25){
                vec3 jit = udir(hash23(pos + vec3(float(uFrame)))) * uEmitSpread;
                vec3 v = uEmitVel + jit*max(length(uEmitVel), 0.4);
                if(p0.mass == 0u){
                    p0.mass = 1u; p0.age = 0u; p0.vel = v;
                    p0.pos = pos + vec3(h.x, h.y, fract(h.x*7.1));
                } else if(p1.mass == 0u){
                    p1.mass = 1u; p1.age = 0u; p1.vel = v;
                    p1.pos = pos + vec3(h.y, h.x, fract(h.y*7.1));
                }
            }
        }
    }
    if(uFrame < 10){
        bool seed;
        // uFill controls how much fluid is seeded — small blob .. full container.
        if(uSphereShape > 0.5){
            // Ball in the upper part of the sphere; radius grows with uFill.
            float R  = minv(size3d)*0.5;
            vec3  sc = size3d*0.5 + vec3(0.0, 0.0, 0.30*R);
            seed = length(pos - sc) < (0.25 + 0.65*uFill)*R;
        } else {
            // Centred box region; half-extent grows with uFill.
            float h = 0.15 + 0.35*uFill;          // 0.15..0.50 of the grid
            vec3  d = abs(pos - size3d*0.5) / size3d;
            seed = (d.x < h && d.y < h && d.z < h);
        }
        p0.mass = 0u; p1.mass = 0u;
        if(seed){
            p0.mass = initial_particle_density;
            if(uSeedDouble > 0.5) p1.mass = initial_particle_density;
        }
        p0.age = 0u; p1.age = 0u;
        p0.pos = pos + ((uSeedDouble > 0.5) ? vec3(0.25) : vec3(0.0)); p0.vel = vec3(0.0);
        p1.pos = pos + ((uSeedDouble > 0.5) ? vec3(0.75) : vec3(0.0)); p1.vel = vec3(0.0);
    }
    fragColor = packParticles(p0, p1, pos);
})";

// ── Buffer D: shadow march ────────────────────────────────────────────
static const char* FS_D = R"(
uniform sampler3D uDensity;
flat in int gLayer;
out vec4 fragColor;
float Density(vec3 p){ return trilinear(uDensity, p).z; }
vec4 calcNormal(vec3 p, float dx){
    const vec3 k = vec3(1,-1,0);
    return (k.xyyx*Density(p + k.xyy*dx) +
            k.yyxx*Density(p + k.yyx*dx) +
            k.yxyx*Density(p + k.yxy*dx) +
            k.xxxx*Density(p + k.xxx*dx))/vec4(4.*dx,4.*dx,4.*dx,4.);
}
void main(){
    size3d = vec3(uSize);
    vec3 pos = vec3(floor(gl_FragCoord.xy), float(gLayer));
    const float step_size = 1.0;
    const int step_count = 100;
    float td = 0.0;
    vec3 rd = light_dir;
    float optical_density = 0.0;
    vec3 normal = normalize(calcNormal(pos, 0.5).xyz);
    pos += -normal*0.5;
    for(int i = 0; i < step_count; i++){
        vec3 p = pos + rd*td;
        if(!all(lessThanEqual(p, size3d)) || !all(greaterThanEqual(p, vec3(0.0)))) break;
        optical_density += Density(p) * step_size;
        td += step_size;
    }
    fragColor = vec4(0.2*optical_density);
})";

// ── Image: raymarch the particle spheres + the scene ──────────────────
static const char* FS_RENDER = R"(
in vec2 vUv;
uniform usampler3D uParticles;
uniform sampler3D  uDensity;
uniform sampler3D  uShadow;
uniform vec2  uRes;
uniform float uRotSpeed;
uniform float uTilt;
uniform vec3  uDeep;
uniform vec3  uGlow;
uniform float uBright;
// ── color + lighting options (defaults reproduce the original look) ──
uniform float uLightInt;   // diffuse key-light intensity (1 = original)
uniform float uAmbient;    // ambient fill (0.1 = original)
uniform float uSpec;       // reflection/specular strength (1 = original)
uniform float uRough;      // reflection blur (0 = mirror / original)
uniform float uFresnelPow; // grazing reflection power (3 = original)
uniform float uRefract;    // glassy see-through (0 = original)
uniform float uSelfReflect;// blobs reflect each other (0 = off)
uniform vec3  uShallow;    // rim / grazing-edge tint
uniform float uRim;        // rim amount (0 = off / original)
uniform float uSat;        // output saturation (1 = unchanged)
uniform float uVelGlow;    // speed-glow amount (1 = original)
uniform float uFluidAlpha; // fluid pixel opacity
// ── background / scene ──
uniform int   uBgMode;     // 0 gradient 1 solid 2 horizon 3 image 4 stars
uniform vec3  uBgTop;      // background gradient top (also reflection env)
uniform vec3  uBgBottom;   // background gradient bottom
uniform float uBgAlpha;    // background opacity (0 = transparent / composites under)
uniform vec3  uHorizonCol; uniform float uHorizonAmt;
uniform vec3  uSunCol;     uniform float uSunInt; uniform float uSunSize; uniform vec3 uSunDir;
uniform float uFog;
uniform float uVignette;
uniform float uStars;
uniform float uFloorOn;    uniform float uFloorGap;  uniform vec3 uFloorCol;
uniform float uFloorGrid;  uniform vec3  uGridCol;   uniform float uFloorReflect; uniform float uFloorFade;
uniform float uCage;       uniform vec3  uCageCol;   uniform float uCageWidth;
uniform int   uOrbCount;   uniform vec4  uOrbPos[4]; uniform vec4 uOrbCol[4];
uniform float uOrbGlow;    uniform float uOrbLight;  uniform float uOrbSpec;
uniform sampler2D uBgImage; uniform int uBgImageOn; uniform float uBgFlipY;
// ── particles ──
uniform float uSphereScale;// particle sphere size multiplier (1 = original)
uniform float uParticleCube;// 0 = round particles (original), 1 = cube particles
uniform int   uSurfaceMode;// 0 = individual particles (original), 1 = smooth liquid isosurface
uniform float uIsoLevel;   // density threshold for the liquid surface
uniform float uAgeShrink;  uniform vec3 uAgeCol; uniform float uAgeTint;
uniform float uZoom;       // camera zoom (1 = original; >1 zooms in, telephoto)
uniform float uCamDist;    // camera distance multiplier (1 = original framing)
uniform float uCamHeight;  // camera aim point, vertical offset × container size
// ── liquid material ──
uniform float uAbsorb;     // depth colouring: thin = light, thick = deep (Beer-Lambert)
uniform float uSubsurface; // light bleeding through thin parts toward the camera
uniform float uAO;         // crevice occlusion from the density field
uniform float uKeySpec;    // key-light highlight on the liquid
uniform sampler2D uImage;
uniform int   uImageOn;
uniform float uImageMix;
uniform float uFlipY;
layout(location = 0) out vec4 fragColor;
layout(location = 1) out vec4 fragAux;   // x = normalised depth, y = liquid thickness

#define FOV 2.5
#define MAX_DIST 1e10

mat3 getCamera(vec2 a){
    mat3 t = mat3(1,0,0, 0,cos(a.y),-sin(a.y), 0,sin(a.y),cos(a.y));
    mat3 p = mat3(cos(a.x),sin(a.x),0, -sin(a.x),cos(a.x),0, 0,0,1);
    return t*p;
}
vec3 getRay(vec2 a, vec2 pos){
    mat3 cam = getCamera(a);
    float fov = FOV / max(uZoom, 0.05);   // narrower FOV = telephoto zoom-in
    return normalize(transpose(cam)*vec3(fov*pos.x, 1.0, fov*pos.y));
}
struct Ray { vec3 ro; vec3 rd; float td; vec3 normal; vec3 color; };
void iSphere(inout Ray ray, vec4 sphere, vec3 color){
    vec3 ro = ray.ro - sphere.xyz;
    float b = dot(ro, ray.rd);
    float c = dot(ro, ro) - sphere.w*sphere.w;
    float h = b*b - c;
    if(h > 0.){
        h = sqrt(h);
        float d1 = -b-h, d2 = -b+h;
        if(d1 >= 0.0 && d1 <= ray.td){ ray.normal = normalize(ro + ray.rd*d1); ray.color = color; ray.td = d1; }
        else if(d2 >= 0.0 && d2 <= ray.td){ ray.normal = normalize(ro + ray.rd*d2); ray.color = color; ray.td = d2; }
    }
}
// Ray vs axis-aligned cube (slab method) with the entry-face normal, so
// particles can render as cubes instead of spheres. Updates the ray like
// iSphere does (nearest hit only).
void iCube(inout Ray ray, vec3 center, float hs, vec3 color){
    vec3 ro = ray.ro - center;
    vec3 inv = 1.0 / (ray.rd + vec3(1e-8));
    vec3 n = inv * ro;
    vec3 k = abs(inv) * hs;
    vec3 t1 = -n - k, t2 = -n + k;
    float tN = max(max(t1.x, t1.y), t1.z);
    float tF = min(min(t2.x, t2.y), t2.z);
    if(tN > tF || tF < 0.0) return;
    float td = (tN >= 0.0) ? tN : tF;
    if(td >= 0.0 && td <= ray.td){
        ray.td = td;
        ray.color = color;
        // Face normal = the axis whose near-plane (t1) was the max.
        ray.normal = -sign(ray.rd) * step(t1.yzx, t1.xyz) * step(t1.zxy, t1.xyz);
    }
}
vec2 iBox(vec3 ro, vec3 rd, vec3 bs){
    vec3 m = sign(rd)/max(abs(rd), 1e-8);
    vec3 n = m*ro;
    vec3 k = abs(m)*bs;
    vec3 t1 = -n - k, t2 = -n + k;
    float tN = max(max(t1.x,t1.y),t1.z);
    float tF = min(min(t2.x,t2.y),t2.z);
    if(tN > tF || tF <= 0.) return vec2(MAX_DIST);
    return vec2(tN, tF);
}
float hash13(vec3 p3){
    p3 = fract(p3 * .1031);
    p3 += dot(p3, p3.zyx + 31.32);
    return fract((p3.x + p3.y) * p3.z);
}
// Environment, d in env space (d.y = world up). `rough` blurs the sun.
vec3 envBase(vec3 d, float rough){
    d = normalize(d);
    float t = clamp(0.5 + 0.5*d.y, 0.0, 1.0);
    vec3 col = (uBgMode == 1) ? uBgBottom : mix(uBgBottom, uBgTop, t);
    col += uHorizonCol * uHorizonAmt * 0.9 * pow(1.0 - abs(d.y), 22.0);   // thin horizon band
    col *= mix(0.45, 1.0, smoothstep(-0.5, 0.05, d.y));            // ground side darker
    // Bright key light → tight specular highlight that slides across the
    // reflective surface, which is what reads as crisp + 3D.
    vec3 sun = normalize(uSunDir.yzx);
    float s = max(dot(d, sun), 0.0);
    float sz = max(uSunSize / (1.0 + 20.0*rough), 2.0);
    col += uSunCol * pow(s, sz) * 1.6 * uSunInt / (1.0 + 2.0*rough);
    col += vec3(0.50,0.55,0.72) * pow(s, 6.0) * 0.25 * uSunInt;
    if(uBgMode == 4){
        // Sparse star field on a direction grid (fixed in world space).
        vec3 g = floor(d * 90.0);
        float h = hash13(g);
        float thr = 1.0 - 0.06*uStars;
        if(h > thr){
            vec3 c = (g + 0.5 + (vec3(hash13(g+1.0), hash13(g+2.0), hash13(g+3.0)) - 0.5)*0.8) / 90.0;
            float dd = length(d - normalize(c)) * 90.0;
            float tw = 0.6 + 0.4*sin(uTime*3.0 + h*40.0);
            col += vec3(1.0) * (1.0 - smoothstep(0.0, 0.5, dd)) * tw * (1.0 - rough);
        }
    }
    return col;
}
float Density(vec3 p){ return trilinear(uDensity, p).z; }
// 7-tap smoothed density: takes the cell-frequency lumps out of the liquid
// surface and its normals (the raw field is only 1 sample per sim cell).
float DensityS(vec3 p){
    const float o = 0.9;
    float d = 2.0*Density(p);
    d += Density(p + vec3(o,0,0)) + Density(p - vec3(o,0,0));
    d += Density(p + vec3(0,o,0)) + Density(p - vec3(0,o,0));
    d += Density(p + vec3(0,0,o)) + Density(p - vec3(0,0,o));
    return d*0.125;
}
vec3 calcNormalS(vec3 p, float dx){
    const vec3 k = vec3(1,-1,0);
    return normalize(k.xyy*DensityS(p + k.xyy*dx) + k.yyx*DensityS(p + k.yyx*dx) +
                     k.yxy*DensityS(p + k.yxy*dx) + k.xxx*DensityS(p + k.xxx*dx));
}
float Shadow(vec3 p){ return exp(-trilinear(uShadow, p).x) + 0.05; }
vec4 calcNormal(vec3 p, float dx){
    const vec3 k = vec3(1,-1,0);
    return (k.xyyx*Density(p + k.xyy*dx) +
            k.yyxx*Density(p + k.yyx*dx) +
            k.yxyx*Density(p + k.yxy*dx) +
            k.xxxx*Density(p + k.xxx*dx))/vec4(4.*dx,4.*dx,4.*dx,4.);
}
vec3 particleColor(Particle p){
    return uGlow * length(p.vel) * uVelGlow + uAgeCol * uAgeTint * (float(p.age)/7.0);
}
void TraceCell(inout Ray ray, vec3 p){
    Particle p0, p1;
    unpackParticles(LOAD3D(uParticles, p), p, p0, p1);
    float rad = 0.85*uSphereScale;
    float r0 = rad * (1.0 - uAgeShrink*float(p0.age)/8.0);
    float r1 = rad * (1.0 - uAgeShrink*float(p1.age)/8.0);
    if(uParticleCube > 0.5){
        if(p0.mass > 0u) iCube(ray, p0.pos, r0, particleColor(p0));
        if(p1.mass > 0u) iCube(ray, p1.pos, r1, particleColor(p1));
    } else {
        if(p0.mass > 0u) iSphere(ray, vec4(p0.pos, r0), particleColor(p0));
        if(p1.mass > 0u) iSphere(ray, vec4(p1.pos, r1), particleColor(p1));
    }
}
void TraceCells(inout Ray ray, vec3 p){
    vec3 p0 = floor(p);
    vec4 rho = LOAD3D(uDensity, p);
    if(rho.z < 1e-5) return;
    range(i,-1,1) range(j,-1,1) range(k,-1,1){
        TraceCell(ray, p0 + vec3(i,j,k));
    }
}
// Average particle glow colour around a point (for the isosurface mode).
vec3 cellGlow(vec3 p){
    Particle p0, p1;
    vec3 c = floor(p);
    unpackParticles(LOAD3D(uParticles, c), c, p0, p1);
    vec3 g = vec3(0.0); float w = 0.0;
    if(p0.mass > 0u){ g += particleColor(p0)*float(p0.mass); w += float(p0.mass); }
    if(p1.mass > 0u){ g += particleColor(p1)*float(p1.mass); w += float(p1.mass); }
    return (w > 0.0) ? g/w : vec3(0.0);
}
// March the fluid between t0..t1 along (ro, rd). True on a hit.
// Surface mode 0 traces the individual particle spheres (the original look);
// mode 1 marches the trilinear density field to a smooth liquid isosurface
// (metaball-style, no per-sphere aliasing) with a bisection refine.
bool marchFluid(vec3 ro, vec3 rd, float t0, float t1, int steps, float stepSize, out Ray ray){
    ray.ro = ro; ray.rd = rd; ray.td = t1; ray.color = vec3(0.0); ray.normal = vec3(0.0, 0.0, 1.0);
    float td = max(t0, 0.0);
    if(uSurfaceMode == 1){
        float st = stepSize*0.5;
        int n = steps*2;
        for(int i = 0; i < n; i++){
            vec3 p = ro + rd*td;
            // Cheap raw-density test to get near the surface, then the
            // smoothed field decides the actual crossing + a bisection refine.
            if(Density(p) > uIsoLevel*0.45 && DensityS(p) > uIsoLevel){
                float a = max(td - st, max(t0, 0.0)), b = td;
                for(int k = 0; k < 6; k++){
                    float m = 0.5*(a + b);
                    if(DensityS(ro + rd*m) > uIsoLevel) b = m; else a = m;
                }
                ray.td = b;
                vec3 hp = ro + rd*b;
                ray.normal = calcNormalS(hp, 1.1);
                ray.color = cellGlow(hp);
                return true;
            }
            td += st;
            if(td > t1) break;
        }
        return false;
    }
    for(int i = 0; i < steps; i++){
        vec3 p = ro + rd*td;
        TraceCells(ray, p);
        td += stepSize;
        if(td > t1 || ray.td < t1) break;
    }
    return ray.td < t1;
}
// ── Emissive orbs ──
float orbHit(vec3 ro, vec3 rd, out int idx){
    float best = MAX_DIST; idx = -1;
    for(int i = 0; i < 4; i++){
        if(i >= uOrbCount) break;
        vec3 oc = ro - uOrbPos[i].xyz;
        float b = dot(oc, rd);
        float c = dot(oc, oc) - uOrbPos[i].w*uOrbPos[i].w;
        float h = b*b - c;
        if(h > 0.0){
            float t = -b - sqrt(h);
            if(t > 0.0 && t < best){ best = t; idx = i; }
        }
    }
    return best;
}
vec3 orbEmissive(int i, vec3 p, vec3 rd){
    vec3 n = normalize(p - uOrbPos[i].xyz);
    float f = max(dot(n, -rd), 0.0);
    // Limb-darkened sphere with a hot core — the core pushes past the bloom
    // threshold so the orb blooms like a real light instead of a flat disc.
    return uOrbCol[i].rgb * uOrbCol[i].a * (0.45 + 0.9*pow(f, 1.5) + 1.6*pow(f, 8.0));
}
vec3 orbGlowAlong(vec3 ro, vec3 rd, float tMax){
    vec3 g = vec3(0.0);
    if(uOrbGlow <= 0.0) return g;
    for(int i = 0; i < 4; i++){
        if(i >= uOrbCount) break;
        vec3 c = uOrbPos[i].xyz; float r = uOrbPos[i].w;
        float t = clamp(dot(c - ro, rd), 0.0, tMax);
        float d = length(ro + rd*t - c);
        float k = r / (d + 0.5*r);
        g += uOrbCol[i].rgb * uOrbCol[i].a * uOrbGlow * 0.35 * k*k;
    }
    return g;
}
// Light the fluid surface with the orbs as point lights.
void orbLighting(vec3 p, vec3 n, vec3 reflD, vec3 albedo, inout vec3 body, inout vec3 spec){
    for(int i = 0; i < 4; i++){
        if(i >= uOrbCount) break;
        vec3 L = uOrbPos[i].xyz - p;
        float dist = length(L); L /= max(dist, 1e-3);
        float att = uOrbCol[i].a / (1.0 + dist*dist / (size3d.x*size3d.x*0.03));
        body += albedo * uOrbCol[i].rgb * att * max(dot(n, L), 0.0) * uOrbLight * uBright * 0.3;
        spec += uOrbCol[i].rgb * att * pow(max(dot(reflD, L), 0.0), 48.0) * uOrbSpec;
    }
}
// Environment seen from a world point along a world direction: sky + orbs.
vec3 envWorld(vec3 ro, vec3 rd, float rough){
    vec3 col = envBase(rd.yzx, rough);
    int oi; float to = orbHit(ro, rd, oi);
    if(to < MAX_DIST) col = mix(col, orbEmissive(oi, ro + rd*to, rd), 1.0 - rough*0.5);
    return col;
}
float floorZ(){ return -uFloorGap * size3d.z; }
float floorT(vec3 ro, vec3 rd){
    if(uFloorOn < 0.5 || rd.z > -1e-5) return MAX_DIST;
    float t = (floorZ() - ro.z) / rd.z;
    return (t > 0.0) ? t : MAX_DIST;
}
// Simplified fluid shade (used for secondary rays: floor + self reflections).
vec3 shadeFluidSimple(Ray ray){
    vec3 p0 = ray.ro + ray.rd*ray.td;
    vec3 normal = normalize(calcNormal(p0, 0.5).xyz);
    if(any(isnan(normal))) normal = ray.normal;
    normal = -normalize(mix(normal, ray.normal, (uSurfaceMode == 1) ? 1.0 : 0.25));
    float LdotN = max(dot(normal, light_dir), 0.0);
    float shadow = Shadow(p0);
    vec3 refl = envBase(reflect(ray.rd, normal).yzx, uRough);
    float K = 0.75*(1.0 - pow(max(dot(normal, reflect(ray.rd, normal)), 0.0), uFresnelPow));
    return uBright*shadow*uDeep*(LdotN*uLightInt)*(1.0 - K) + shadow*refl*K*uSpec + uAmbient*ray.color;
}
// How much liquid sits behind the hit point along the view ray (grid units,
// normalised by the container size) — drives the depth colouring.
float liquidThickness(vec3 hp, vec3 rd){
    float ts = size3d.x*0.035;
    float thick = 0.0;
    float j = hash13(hp*7.1) - 0.5;          // per-pixel dither: no contour bands
    float inv = 1.0 / max(uIsoLevel*0.8, 0.02);
    for(int i = 1; i <= 12; i++){
        vec3 q = hp + rd*ts*(float(i) + j);
        if(any(lessThan(q, vec3(0.0))) || any(greaterThan(q, size3d))) break;
        thick += clamp(Density(q)*inv, 0.0, 1.0);
    }
    return thick*ts / (size3d.x*0.45);
}
// Crevice darkening: density just outside the surface along the normal is
// ~0 on an open blob and high where blobs meet / fold.
float liquidAO(vec3 p, vec3 n){
    float d1 = DensityS(p + n*2.0) / max(uIsoLevel, 0.02);
    float d2 = DensityS(p + n*4.5) / max(uIsoLevel, 0.02);
    return exp(-uAO*(0.9*d1 + 0.5*d2));
}
// Full fluid shade for the primary hit.
vec3 shadeFluid(Ray ray, vec2 tdBox, out float thickOut){
    vec3 p0 = ray.ro + ray.rd*ray.td;
    vec3 normal = (uSurfaceMode == 1) ? -ray.normal : normalize(calcNormal(p0, 0.5).xyz);
    // a flat density patch has a zero gradient → normalize() returns NaN;
    // one NaN pixel used to smear into a black rectangle through the bloom blur
    if(any(isnan(normal))) normal = -ray.rd;
    // Cubes want their flat face normal to read; spheres keep the smooth
    // density-gradient normal (only lightly biased by the hit normal).
    float nMix = (uSurfaceMode == 1) ? 0.0 : (uParticleCube > 0.5) ? 0.85 : 0.25;
    if(uSurfaceMode != 1) normal = -normalize(mix(normal, ray.normal, nMix));
    float T = liquidThickness(p0, ray.rd);
    thickOut = T;
    float ao = liquidAO(p0, normal);
    vec3 albedo = uDeep;
    if(uImageOn == 1){
        vec2 iuv = clamp(p0.xy / size3d.xy, 0.0, 1.0);
        iuv.y = mix(iuv.y, 1.0 - iuv.y, uFlipY);
        vec3 img = texture(uImage, iuv).rgb;
        albedo = mix(uDeep, img, uImageMix);
    }
    float LdotN = dot(normal, light_dir);
    float shadow = Shadow(p0);
    vec3 refl_d = reflect(ray.rd, normal);
    // Reflection: sky + orbs, optionally the floor, optionally other blobs.
    vec3 refl = envWorld(p0, refl_d, uRough);
    if(uFloorOn > 0.5 && refl_d.z < 0.0){
        float tf = floorT(p0, refl_d);
        if(tf < MAX_DIST){
            vec3 fp = p0 + refl_d*tf;
            float dist = length(fp.xy - size3d.xy*0.5)/size3d.x;
            vec3 fc = uFloorCol * (0.35 + 0.65*max(light_dir.z, 0.0));
            if(uFloorGrid > 0.0){
                vec2 g = abs(fract(fp.xy/(size3d.x*0.125)) - 0.5);
                fc += uGridCol*uFloorGrid*(1.0 - smoothstep(0.0, 0.08, min(g.x, g.y)));
            }
            refl = mix(refl, fc, clamp(1.0 - (dist-1.0)*uFloorFade*0.5, 0.0, 1.0) * (1.0 - uRough));
        }
    }
    if(uSelfReflect > 0.0){
        Ray r2;
        vec2 tb = iBox(p0 - size3d*0.5, refl_d, 0.5*size3d);
        if(tb.y < MAX_DIST && marchFluid(p0 + refl_d*1.2, refl_d, 0.0, tb.y, 40, 2.5, r2))
            refl = mix(refl, shadeFluidSimple(r2), uSelfReflect);
    }
    // Schlick fresnel (F0 of water-ish 0.03) with the exponent as the
    // artistic control: grazing angles go fully reflective.
    float NdotV = max(dot(normal, -ray.rd), 0.0);
    float K = mix(0.03, 1.0, pow(1.0 - NdotV, uFresnelPow));
    K = clamp(K, 0.0, 0.92);
    // rim/fresnel tint toward the shallow colour at grazing angles
    float fres = pow(1.0 - NdotV, 3.0);
    albedo = mix(albedo, uShallow, fres * uRim);
    // Depth colouring (Beer-Lambert): thin edges go light/bright, thick
    // bodies sink to the deep colour. This is what makes it read as liquid
    // instead of painted plastic.
    vec3 thin  = mix(albedo, vec3(1.0), 0.45);
    vec3 thick = albedo*0.8;
    float beer = 1.0 - exp(-T*uAbsorb*2.2);
    vec3 bodyAlbedo = mix(thin, thick, beer);
    // Wrapped diffuse so the unlit side still carries colour.
    float wrap = clamp((LdotN + 0.35)/1.35, 0.0, 1.0);
    // Split body (diffuse + ambient) from the glassy specular reflection
    // so the image can recolor the body without killing the wet highlight.
    vec3 specCol = shadow*refl*K*uSpec;
    // Key-light highlight: a tight lobe that slides over the surface.
    specCol += uSunCol * pow(max(dot(refl_d, light_dir), 0.0), 140.0) * 2.5 * uKeySpec * shadow;
    vec3 bodyCol = uBright*shadow*bodyAlbedo*(wrap*uLightInt)*(1.0 - K)
                 + uAmbient*(ray.color + bodyAlbedo*0.6);
    // Subsurface: light from behind leaks through the thin parts.
    float back = pow(max(dot(ray.rd, light_dir), 0.0), 3.0);
    bodyCol += uSunCol * thin * exp(-T*uAbsorb*1.6) * back * uSubsurface * uLightInt;
    bodyCol *= mix(1.0, ao, 0.9);
    specCol *= mix(1.0, ao, 0.5);
    orbLighting(p0, normal, refl_d, bodyAlbedo, bodyCol, specCol);
    // Glass: see the environment through the surface along the refracted ray.
    if(uRefract > 0.0){
        vec3 rr = refract(ray.rd, normal, 0.75);
        if(dot(rr, rr) < 1e-6) rr = refl_d;
        vec3 glass = envWorld(p0, rr, uRough) * albedo * uBright * 0.55;
        bodyCol = mix(bodyCol, glass, uRefract);
    }
    // The liquid's color should COME FROM the image: recolor the lit body
    // to the image hue at full strength while keeping the sim's luminance,
    // so it still reads as a shaded 3D liquid. uImageMix fades this in.
    if(uImageOn == 1){
        float bl = dot(bodyCol, vec3(0.299, 0.587, 0.114));
        float al = dot(albedo,  vec3(0.299, 0.587, 0.114)) + 1e-4;
        vec3 recolored = albedo * (bl / al);   // image chroma at body luminance
        bodyCol = mix(bodyCol, recolored, uImageMix);
    }
    vec3 col = bodyCol + specCol;
    // Depth fog toward the background.
    if(uFog > 0.0){
        float depth = clamp((ray.td - max(tdBox.x, 0.0)) / (size3d.x*1.2), 0.0, 1.0);
        col = mix(col, envBase(ray.rd.yzx, 0.0), uFog*depth);
    }
    return col;
}
vec3 shadeFloor(vec3 p, vec3 rd){
    vec3 n = vec3(0.0, 0.0, 1.0);
    float diff = 0.35 + 0.65*max(dot(n, light_dir), 0.0);
    // Contact shadow: the fluid's shadow volume sampled where the light ray
    // from this floor point enters the container.
    float sh = 1.0;
    vec2 tb = iBox(p - size3d*0.5, light_dir, 0.5*size3d);
    if(tb.x < MAX_DIST){ vec3 e = p + light_dir*(max(tb.x, 0.0) + 0.5); sh = Shadow(e); }
    vec3 col = uFloorCol * diff * sh;
    // Orbs pool light onto the floor (grounds them in the scene).
    for(int i = 0; i < 4; i++){
        if(i >= uOrbCount) break;
        vec3 L = uOrbPos[i].xyz - p; float dist = length(L); L /= max(dist, 1e-3);
        float att = uOrbCol[i].a / (1.0 + dist*dist / (size3d.x*size3d.x*0.05));
        col += uFloorCol * 6.0 * uOrbCol[i].rgb * att * max(L.z, 0.0) * uOrbLight;
    }
    // Sun sheen: a broad glossy lobe on the floor.
    {
        vec3 rr = reflect(rd, n);
        vec3 sun = normalize(uSunDir);
        col += uSunCol * pow(max(dot(rr, sun), 0.0), 24.0) * 0.12 * uSunInt * uFloorReflect;
    }
    if(uFloorGrid > 0.0){
        vec2 g = abs(fract(p.xy/(size3d.x*0.125)) - 0.5);
        col += uGridCol*uFloorGrid*(1.0 - smoothstep(0.0, 0.08, min(g.x, g.y)));
    }
    if(uFloorReflect > 0.0){
        vec3 rr = reflect(rd, n);
        vec3 refl = envWorld(p, rr, uRough);
        vec2 tb2 = iBox(p - size3d*0.5, rr, 0.5*size3d);
        if(tb2.x < MAX_DIST){
            Ray r2;
            if(marchFluid(p, rr, tb2.x, tb2.y, 60, 2.5, r2)) refl = shadeFluidSimple(r2);
        }
        float fr = pow(1.0 - max(dot(n, -rd), 0.0), 2.0);
        col = mix(col, refl, uFloorReflect*(0.35 + 0.65*fr));
    }
    float dist = length(p.xy - size3d.xy*0.5)/size3d.x;
    // Far floor sinks into the dark ground tone (fading into the horizon
    // colour painted a wide orange band across the whole far floor).
    vec3 ground = mix(uFloorCol, uBgBottom, 0.6)*0.6;
    col = mix(col, ground, clamp((dist - 1.0)*uFloorFade*0.5, 0.0, 1.0));
    return col;
}
// Emissive container edges: how close the box-face hit point is to an edge.
// Width is scaled by the ray distance so the lines keep a constant
// on-screen thickness (a fixed world width made the near edge balloon
// into a wedge when the camera orbits past it).
float cageLine(vec3 hp, float t){
    vec3 e = 0.5*size3d - abs(hp - 0.5*size3d);     // distance to each face
    // smallest = the face we're on (~0); second smallest = distance to the edge
    float a = min(e.x, min(e.y, e.z));
    float c = max(e.x, max(e.y, e.z));
    float b = e.x + e.y + e.z - a - c;
    float w = max(uCageWidth, 0.05) * max(t, 1.0) / size3d.x;
    return 1.0 - smoothstep(0.0, w, b);
}
void main(){
    size3d = vec3(uSize);
    vec2 uv = (vUv*uRes - 0.5*uRes)/max(uRes.x, uRes.y);
    vec2 angles = vec2(uRotSpeed*uTime, uTilt);
    vec3 rd = getRay(angles, uv);
    vec3 crd = getRay(angles, vec2(0.0));
    float d = sqrt(dot(size3d, size3d))*0.5;
    vec3 ro = size3d*0.5 + vec3(0.0, 0.0, uCamHeight*size3d.z) - crd*d*uCamDist;
    vec2 tdBox = iBox(ro - size3d*0.5, rd, 0.5*size3d);

    // Scene candidates along the primary ray.
    int oi; float tO = orbHit(ro, rd, oi);
    float tF = floorT(ro, rd);
    Ray ray; bool fluidHit = false; float tFl = MAX_DIST;
    if(tdBox.x < MAX_DIST){
        fluidHit = marchFluid(ro, rd, tdBox.x, tdBox.y, 100, 2.0, ray);
        if(fluidHit) tFl = ray.td;
    }
    float tNear = min(min(tO, tF), tFl);
    vec3 col; float alpha; float thick = 0.0;
    if(tNear >= MAX_DIST){
        if(uBgMode == 3 && uBgImageOn == 1){
            vec2 buv = vUv; buv.y = mix(buv.y, 1.0 - buv.y, uBgFlipY);
            col = texture(uBgImage, buv).rgb;
        } else col = envBase(rd.yzx, 0.0);
        alpha = uBgAlpha;
    } else if(tNear == tO){
        col = orbEmissive(oi, ro + rd*tO, rd); alpha = 1.0;
    } else if(tNear == tF){
        col = shadeFloor(ro + rd*tF, rd); alpha = 1.0;
    } else {
        col = shadeFluid(ray, tdBox, thick); alpha = uFluidAlpha;
    }
    // Emissive extras: orb halos + container cage lines.
    vec3 glow = orbGlowAlong(ro, rd, tNear);
    if(uCage > 0.0 && tdBox.x < MAX_DIST){
        if(tdBox.x < tNear) glow += uCageCol*uCage*cageLine(ro + rd*tdBox.x, tdBox.x);
        if(!fluidHit && tdBox.y < tNear) glow += uCageCol*uCage*0.6*cageLine(ro + rd*tdBox.y, tdBox.y);
    }
    col += glow;
    alpha = max(alpha, clamp(max(glow.r, max(glow.g, glow.b)), 0.0, 1.0));
    // never let a NaN/inf reach the HDR target: the bloom blur would spread
    // it into a black box
    if(any(isnan(col)) || any(isinf(col))) col = envBase(rd.yzx, 0.0);
    if(isnan(alpha)) alpha = 1.0;
    // Linear HDR out — tonemap, bloom, grain and vignette happen in the post pass.
    fragColor = vec4(col, alpha);
    float depthN = (tNear >= MAX_DIST) ? 1.0 : clamp(tNear / (d*2.6), 0.0, 1.0);
    fragAux = vec4(depthN, thick, 0.0, 1.0);
})";

// ── Post pipeline: bloom (¼ res) + filmic finish ─────────────────────
static const char* FS_BLOOM_DOWN = R"(
in vec2 vUv;
uniform sampler2D uSrc;
uniform vec2  uTexel;      // 1 / src size
uniform float uThreshold;
out vec4 fragColor;
void main(){
    // 4-tap box downsample (each tap is already bilinear = 16 source pixels)
    vec3 c = vec3(0.0);
    for(int i = 0; i < 4; i++){
        vec2 o = vec2((i & 1) == 0 ? -1.0 : 1.0, i < 2 ? -1.0 : 1.0);
        vec3 t = texture(uSrc, vUv + uTexel*o).rgb;
        if(any(isnan(t)) || any(isinf(t))) t = vec3(0.0);   // never spread a bad pixel
        c += t;
    }
    c *= 0.25;
    // Soft-knee threshold: keep only what is brighter than the threshold.
    float l = max(c.r, max(c.g, c.b));
    float knee = uThreshold*0.5 + 1e-4;
    float soft = clamp(l - uThreshold + knee, 0.0, 2.0*knee);
    soft = soft*soft / (4.0*knee);
    float contrib = max(soft, l - uThreshold) / max(l, 1e-4);
    fragColor = vec4(c*contrib, 1.0);
})";
static const char* FS_BLOOM_BLUR = R"(
in vec2 vUv;
uniform sampler2D uSrc;
uniform vec2 uDir;         // texel step along the blur axis
out vec4 fragColor;
void main(){
    // 9-tap gaussian (sigma ≈ 3 taps), run once per axis.
    const float w[5] = float[](0.2270, 0.1945, 0.1216, 0.0540, 0.0162);
    vec3 c = texture(uSrc, vUv).rgb*w[0];
    for(int i = 1; i < 5; i++){
        vec2 o = uDir*float(i)*1.5;
        c += texture(uSrc, vUv + o).rgb*w[i];
        c += texture(uSrc, vUv - o).rgb*w[i];
    }
    fragColor = vec4(c, 1.0);
})";
static const char* FS_POST = R"(
in vec2 vUv;
uniform sampler2D uScene;  // linear HDR scene (alpha = coverage)
uniform sampler2D uAux;    // x = depth, y = thickness
uniform sampler2D uBloom;
uniform vec2  uTexel;
uniform float uTime;
uniform float uExposure;
uniform float uContrast;
uniform float uBloomAmt;
uniform float uSat;
uniform float uVignette;
uniform float uGrain;
uniform float uDof;
uniform float uDofFocus;
out vec4 fragColor;
vec3 aces(vec3 x){
    const float a = 2.51, b = 0.03, c = 2.43, d = 0.59, e = 0.14;
    return clamp((x*(a*x + b)) / (x*(c*x + d) + e), 0.0, 1.0);
}
float hash12(vec2 p){
    vec3 p3 = fract(vec3(p.xyx) * .1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}
void main(){
    vec4 scene = texture(uScene, vUv);
    if(any(isnan(scene)) || any(isinf(scene))) scene = vec4(0.0, 0.0, 0.0, 1.0);
    vec3 col = scene.rgb;
    float alpha = scene.a;
    // Depth of field: circle-of-confusion from the depth buffer, gathered
    // on a 12-tap disc so the far/near side softens without halos.
    if(uDof > 0.0){
        float depth = texture(uAux, vUv).x;
        float coc = clamp(abs(depth - uDofFocus)*uDof*4.0, 0.0, 1.0);
        float radius = coc*9.0;
        if(radius > 0.5){
            vec3 acc = col; float wsum = 1.0;
            for(int i = 0; i < 12; i++){
                float a = float(i)*0.5236 + hash12(vUv*1000.0)*0.5;
                float r = radius*(0.4 + 0.6*fract(float(i)*0.618));
                vec2 o = vec2(cos(a), sin(a))*r*uTexel;
                vec4 s = texture(uScene, vUv + o);
                float sd = texture(uAux, vUv + o).x;
                // don't let sharp foreground bleed onto blurred background
                float w = (abs(sd - uDofFocus) >= abs(depth - uDofFocus)*0.6) ? 1.0 : 0.25;
                acc += s.rgb*w; wsum += w;
            }
            col = acc / wsum;
        }
    }
    col += texture(uBloom, vUv).rgb * uBloomAmt;
    col *= uExposure;
    col = aces(col);
    // S-curve contrast (midpoint-preserving)
    col = mix(col, col*col*(3.0 - 2.0*col), uContrast);
    float luma = dot(col, vec3(0.299, 0.587, 0.114));
    col = mix(vec3(luma), col, uSat);
    vec2 uv = vUv - 0.5;
    if(uVignette > 0.0)
        col *= 1.0 - uVignette*smoothstep(0.3, 0.95, length(uv)*1.5);
    // Fine film grain, luminance-weighted so blacks stay clean.
    if(uGrain > 0.0){
        float g = hash12(vUv/uTexel + fract(uTime*7.31)*vec2(97.0, 61.0)) - 0.5;
        col += g * uGrain * 0.12 * (0.35 + 0.65*luma);
    }
    vec3 bl = texture(uBloom, vUv).rgb;
    alpha = max(alpha, clamp(max(bl.r, max(bl.g, bl.b))*uBloomAmt, 0.0, 1.0));
    fragColor = vec4(max(col, 0.0), alpha);
})";

// ─────────────────────────────────────────────────────────────────────

static GLuint compileStage(GLenum type, const char* src) {
    GLuint s = glCreateShader(type);
    glShaderSource(s, 1, &src, nullptr);
    glCompileShader(s);
    GLint ok = 0; glGetShaderiv(s, GL_COMPILE_STATUS, &ok);
    if (!ok) {
        char log[4096]; glGetShaderInfoLog(s, sizeof(log), nullptr, log);
        std::cerr << "[FluidSource3D] shader compile error:\n" << log << std::endl;
        glDeleteShader(s); return 0;
    }
    return s;
}

GLuint FluidSource3D::compileProgram(const char* vs, const char* fs) {
    GLuint v = compileStage(GL_VERTEX_SHADER, vs);
    GLuint f = compileStage(GL_FRAGMENT_SHADER, fs);
    if (!v || !f) return 0;
    GLuint p = glCreateProgram();
    glAttachShader(p, v); glAttachShader(p, f);
    glLinkProgram(p);
    glDeleteShader(v); glDeleteShader(f);
    GLint ok = 0; glGetProgramiv(p, GL_LINK_STATUS, &ok);
    if (!ok) {
        char log[4096]; glGetProgramInfoLog(p, sizeof(log), nullptr, log);
        std::cerr << "[FluidSource3D] link error:\n" << log << std::endl;
        glDeleteProgram(p); return 0;
    }
    return p;
}

GLuint FluidSource3D::compileProgram(const char* vs, const char* gs, const char* fs) {
    GLuint v = compileStage(GL_VERTEX_SHADER, vs);
    GLuint g = compileStage(GL_GEOMETRY_SHADER, gs);
    GLuint f = compileStage(GL_FRAGMENT_SHADER, fs);
    if (!v || !g || !f) return 0;
    GLuint p = glCreateProgram();
    glAttachShader(p, v); glAttachShader(p, g); glAttachShader(p, f);
    glLinkProgram(p);
    glDeleteShader(v); glDeleteShader(g); glDeleteShader(f);
    GLint ok = 0; glGetProgramiv(p, GL_LINK_STATUS, &ok);
    if (!ok) {
        char log[4096]; glGetProgramInfoLog(p, sizeof(log), nullptr, log);
        std::cerr << "[FluidSource3D] link error (gs):\n" << log << std::endl;
        glDeleteProgram(p); return 0;
    }
    return p;
}

FluidSource3D::Volume FluidSource3D::createVolume(int size, GLint internalFmt,
        GLenum format, GLenum type, GLenum filter) {
    Volume v; v.size = size; v.internalFmt = internalFmt;
    glGenTextures(1, &v.tex);
    glBindTexture(GL_TEXTURE_3D, v.tex);
    glTexImage3D(GL_TEXTURE_3D, 0, internalFmt, size, size, size, 0,
                 format, type, nullptr);
    glTexParameteri(GL_TEXTURE_3D, GL_TEXTURE_MIN_FILTER, filter);
    glTexParameteri(GL_TEXTURE_3D, GL_TEXTURE_MAG_FILTER, filter);
    glTexParameteri(GL_TEXTURE_3D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
    glTexParameteri(GL_TEXTURE_3D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
    glTexParameteri(GL_TEXTURE_3D, GL_TEXTURE_WRAP_R, GL_CLAMP_TO_EDGE);
    glBindTexture(GL_TEXTURE_3D, 0);
    return v;
}

void FluidSource3D::destroyVolume(Volume& v) {
    if (v.tex) glDeleteTextures(1, &v.tex);
    v = Volume{};
}

void FluidSource3D::clearVolume(const Volume& v, bool isInt) {
    glBindFramebuffer(GL_FRAMEBUFFER, m_volFBO);
    for (int z = 0; z < v.size; z++) {
        glFramebufferTextureLayer(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, v.tex, 0, z);
        if (isInt) { GLuint zero[4] = {0,0,0,0}; glClearBufferuiv(GL_COLOR, 0, zero); }
        else       { GLfloat zero[4] = {0,0,0,0}; glClearBufferfv(GL_COLOR, 0, zero); }
    }
    glBindFramebuffer(GL_FRAMEBUFFER, 0);
}

FluidSource3D::FBO FluidSource3D::createFBO(int w, int h, GLint internalFmt,
                                            GLenum type, GLenum filter) {
    FBO f; f.w = w; f.h = h;
    glGenFramebuffers(1, &f.fbo);
    glGenTextures(1, &f.tex);
    glBindTexture(GL_TEXTURE_2D, f.tex);
    glTexImage2D(GL_TEXTURE_2D, 0, internalFmt, w, h, 0, GL_RGBA, type, nullptr);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, filter);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, filter);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
    glBindFramebuffer(GL_FRAMEBUFFER, f.fbo);
    glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, f.tex, 0);
    glClearColor(0, 0, 0, 1); glClear(GL_COLOR_BUFFER_BIT);
    glBindFramebuffer(GL_FRAMEBUFFER, 0);
    return f;
}

void FluidSource3D::destroyFBO(FBO& f) {
    if (f.tex) glDeleteTextures(1, &f.tex);
    if (f.fbo) glDeleteFramebuffers(1, &f.fbo);
    f = FBO{};
}

void FluidSource3D::reallocVolumes() {
    destroyVolume(m_volA); destroyVolume(m_volB);
    destroyVolume(m_volC); destroyVolume(m_volD);
    m_simRes = std::max(8, std::min(128, m_simRes));
    m_size = m_simRes;
    m_volA = createVolume(m_size, GL_RGBA32UI, GL_RGBA_INTEGER, GL_UNSIGNED_INT, GL_NEAREST);
    m_volC = createVolume(m_size, GL_RGBA32UI, GL_RGBA_INTEGER, GL_UNSIGNED_INT, GL_NEAREST);
    m_volB = createVolume(m_size, GL_RGBA16F, GL_RGBA, GL_HALF_FLOAT, GL_LINEAR);
    m_volD = createVolume(m_size, GL_RGBA16F, GL_RGBA, GL_HALF_FLOAT, GL_LINEAR);
    clearVolume(m_volA, true);  clearVolume(m_volC, true);
    clearVolume(m_volB, false); clearVolume(m_volD, false);
    m_frame = 0;   // re-run the seed block at the new resolution
}

void FluidSource3D::reallocOutput() {
    m_renderScale = std::min(2.0f, std::max(0.25f, m_renderScale));
    int nw = std::max(2, (int)std::round(m_zoneW * m_renderScale));
    int nh = std::max(2, (int)std::round(m_zoneH * m_renderScale));
    if (nw == m_output.w && nh == m_output.h) return;
    destroyFBO(m_output);
    m_outW = nw; m_outH = nh;
    m_output = createFBO(m_outW, m_outH, GL_RGBA8, GL_UNSIGNED_BYTE, GL_LINEAR);
    allocPostTargets();
}

// HDR scene target (+ depth/thickness aux attachment) and the ¼-res bloom
// ping-pong pair. Sized from m_outW/m_outH; called after the output FBO.
void FluidSource3D::allocPostTargets() {
    destroyPostTargets();
    m_hdr = createFBO(m_outW, m_outH, GL_RGBA16F, GL_HALF_FLOAT, GL_LINEAR);
    glGenTextures(1, &m_hdrAux);
    glBindTexture(GL_TEXTURE_2D, m_hdrAux);
    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA16F, m_outW, m_outH, 0, GL_RGBA, GL_HALF_FLOAT, nullptr);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
    glBindFramebuffer(GL_FRAMEBUFFER, m_hdr.fbo);
    glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT1, GL_TEXTURE_2D, m_hdrAux, 0);
    const GLenum bufs[2] = { GL_COLOR_ATTACHMENT0, GL_COLOR_ATTACHMENT1 };
    glDrawBuffers(2, bufs);
    glBindFramebuffer(GL_FRAMEBUFFER, 0);
    int bw = std::max(2, m_outW / 4), bh = std::max(2, m_outH / 4);
    m_bloomA = createFBO(bw, bh, GL_RGBA16F, GL_HALF_FLOAT, GL_LINEAR);
    m_bloomB = createFBO(bw, bh, GL_RGBA16F, GL_HALF_FLOAT, GL_LINEAR);
}

void FluidSource3D::destroyPostTargets() {
    destroyFBO(m_hdr);
    if (m_hdrAux) { glDeleteTextures(1, &m_hdrAux); m_hdrAux = 0; }
    destroyFBO(m_bloomA);
    destroyFBO(m_bloomB);
}

bool FluidSource3D::init(int outW, int outH) {
    // Render at the full zone resolution by default (crisp). m_renderScale
    // (0.25..2.0) trades sharpness for speed and can be changed live; >1 is
    // supersampling (the compositor's bilinear downsample averages it).
    m_zoneW = std::max(2, outW);
    m_zoneH = std::max(2, outH);
    m_renderScale = std::min(2.0f, std::max(0.25f, m_renderScale));
    m_outW = std::max(2, (int)std::round(m_zoneW * m_renderScale));
    m_outH = std::max(2, (int)std::round(m_zoneH * m_renderScale));
    m_simRes = std::max(8, std::min(128, m_simRes));
    m_size = m_simRes;

    std::string common = COMMON;
    std::string ver = "#version 330 core\n";
    std::string sA = ver + common + FS_A;
    std::string sB = ver + common + FS_B;
    std::string sC = ver + common + FS_C;
    std::string sD = ver + common + FS_D;
    std::string sR = ver + common + FS_RENDER;
    std::string sBD = ver + FS_BLOOM_DOWN;
    std::string sBB = ver + FS_BLOOM_BLUR;
    std::string sPo = ver + FS_POST;

    // Sim passes use layered rendering (VS+GS route each instance to a Z
    // slice via gl_Layer). The render pass is an ordinary 2D fullscreen draw.
    m_progA = compileProgram(VS_LAYER, GS_LAYER, sA.c_str());
    m_progB = compileProgram(VS_LAYER, GS_LAYER, sB.c_str());
    m_progC = compileProgram(VS_LAYER, GS_LAYER, sC.c_str());
    m_progD = compileProgram(VS_LAYER, GS_LAYER, sD.c_str());
    m_progRender = compileProgram(VS_FULL, sR.c_str());
    m_progBloomDown = compileProgram(VS_FULL, sBD.c_str());
    m_progBloomBlur = compileProgram(VS_FULL, sBB.c_str());
    m_progPost      = compileProgram(VS_FULL, sPo.c_str());
    if (!m_progA || !m_progB || !m_progC || !m_progD || !m_progRender ||
        !m_progBloomDown || !m_progBloomBlur || !m_progPost) {
        std::cerr << "[FluidSource3D] program compile failed" << std::endl;
        return false;
    }

    float verts[] = { -1.f,-1.f,  3.f,-1.f,  -1.f,3.f };
    glGenVertexArrays(1, &m_quadVAO);
    glGenBuffers(1, &m_quadVBO);
    glBindVertexArray(m_quadVAO);
    glBindBuffer(GL_ARRAY_BUFFER, m_quadVBO);
    glBufferData(GL_ARRAY_BUFFER, sizeof(verts), verts, GL_STATIC_DRAW);
    glEnableVertexAttribArray(0);
    glVertexAttribPointer(0, 2, GL_FLOAT, GL_FALSE, 0, (void*)0);
    glBindVertexArray(0);

    glGenFramebuffers(1, &m_volFBO);

    // Particle voxels: exact bit-packed → RGBA32UI + NEAREST.
    m_volA = createVolume(m_size, GL_RGBA32UI, GL_RGBA_INTEGER, GL_UNSIGNED_INT, GL_NEAREST);
    m_volC = createVolume(m_size, GL_RGBA32UI, GL_RGBA_INTEGER, GL_UNSIGNED_INT, GL_NEAREST);
    // Density / shadow: trilinear → RGBA16F + LINEAR.
    m_volB = createVolume(m_size, GL_RGBA16F, GL_RGBA, GL_HALF_FLOAT, GL_LINEAR);
    m_volD = createVolume(m_size, GL_RGBA16F, GL_RGBA, GL_HALF_FLOAT, GL_LINEAR);

    clearVolume(m_volA, true);  clearVolume(m_volC, true);
    clearVolume(m_volB, false); clearVolume(m_volD, false);

    m_output = createFBO(m_outW, m_outH, GL_RGBA8, GL_UNSIGNED_BYTE, GL_LINEAR);
    allocPostTargets();

    m_lastTime = glfwGetTime();
    m_frame = 0;
    m_ready = true;
    std::cout << "[FluidSource3D] init size3d=" << m_size
              << " out=" << m_outW << "x" << m_outH << std::endl;
    return true;
}

void FluidSource3D::bindVolume(GLuint prog, const char* name, const Volume& v, int unit) {
    glActiveTexture(GL_TEXTURE0 + unit);
    glBindTexture(GL_TEXTURE_3D, v.tex);
    glUniform1i(glGetUniformLocation(prog, name), unit);
}

void FluidSource3D::runVolumePass(GLuint prog, const Volume& dst) {
    // Layered attach (all Z slices) + ONE instanced draw — the GS routes
    // instance i to layer i via gl_Layer. Single render pass, no per-slice
    // FBO reattach (the prior approach cost ~N render-pass switches/pass).
    glBindFramebuffer(GL_FRAMEBUFFER, m_volFBO);
    glFramebufferTexture(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, dst.tex, 0);
    glViewport(0, 0, dst.size, dst.size);
    glBindVertexArray(m_quadVAO);
    glUniform1f(glGetUniformLocation(prog, "uSize"), (float)dst.size);
    glDrawArraysInstanced(GL_TRIANGLES, 0, 3, dst.size);
}

static inline float deg2rad(float d) { return d * 3.14159265358979f / 180.0f; }

// Per-frame derived state: light direction from azimuth/elevation, orb
// world positions from their motion mode.
void FluidSource3D::updateDerived(float /*dt*/) {
    {
        float az = deg2rad(m_lightAzimuth), el = deg2rad(m_lightElev);
        m_lightDir[0] = std::cos(el) * std::cos(az);
        m_lightDir[1] = std::cos(el) * std::sin(az);
        m_lightDir[2] = std::sin(el);
    }
    float S = (float)m_size;
    for (int i = 0; i < kMaxOrbs; i++) {
        Orb& o = m_orbs[i];
        float ph = deg2rad(o.phase);
        if (o.motion == 1) {            // orbit around the container's vertical axis
            float a = m_time * o.speed + ph;
            float r = o.orbitRadius * 0.5f * S;
            o.worldPos[0] = 0.5f * S + std::cos(a) * r;
            o.worldPos[1] = 0.5f * S + std::sin(a) * r;
            o.worldPos[2] = o.orbitHeight * S;
        } else if (o.motion == 2) {     // bob up and down at a fixed xy
            float a = m_time * o.speed * 2.0f + ph;
            o.worldPos[0] = o.pos[0] * S;
            o.worldPos[1] = o.pos[1] * S;
            o.worldPos[2] = (o.pos[2] + 0.5f * o.orbitRadius * std::sin(a)) * S;
        } else {
            o.worldPos[0] = o.pos[0] * S;
            o.worldPos[1] = o.pos[1] * S;
            o.worldPos[2] = o.pos[2] * S;
        }
    }
}

void FluidSource3D::setSimUniforms(GLuint prog) {
    auto U = [&](const char* n) { return glGetUniformLocation(prog, n); };
    // Audio-driven motion: calm when quiet, churning when loud. At
    // m_audioIntensity==0 this is exactly 1.0 (original behaviour).
    float hot = 0.25f + (2.5f - 0.25f) * m_audioEnergy;     // calm..intense by loudness
    float energyU = 1.0f + (hot - 1.0f) * m_audioIntensity; // gated by the user amount
    glUniform1f(U("uEnergy"), energyU);
    // Extra force fields. All default to 0 == original. Audio Motion
    // also pumps vortex + turbulence so it churns harder as it gets loud.
    float audioPump = m_audioIntensity * m_audioEnergy;
    glUniform1f(U("uGravityAdd"), m_gravity - 1.0f);
    glUniform3f(U("uGravityDir"), m_gravityTiltX, m_gravityTiltY, -1.0f);
    glUniform1f(U("uForceAdd"),   m_forceScale - 1.0f);
    glUniform1f(U("uVortex"),     m_vortex + audioPump * 0.8f);
    glUniform1f(U("uVortexLift"), m_vortexLift);
    glUniform1f(U("uTurbulence"), m_turbulence + audioPump * 1.2f);
    glUniform1f(U("uTurbScale"),  m_turbScale);
    glUniform1f(U("uTurbSpeed"),  m_turbSpeed);
    glUniform1f(U("uSphereShape"), m_sphereShape);
    glUniform1f(U("uFill"),       m_fillAmount);
    glUniform1f(U("uSeedDouble"), m_doubleDensity ? 1.0f : 0.0f);
    glUniform1f(U("uDt"),         std::max(0.05f, m_simSpeed));
    glUniform1f(U("uRestDensity"), std::max(0.1f, m_restDensity));
    glUniform1f(U("uMaxVel"),     std::max(0.1f, m_maxVel));
    glUniform1f(U("uViscosity"),  m_viscosity);
    glUniform1f(U("uTension"),    m_tension);
    glUniform1f(U("uBounce"),     m_bounce);
    glUniform1f(U("uDamping"),    m_damping);
    {
        float a = deg2rad(m_windAngle);
        glUniform3f(U("uWind"), std::cos(a) * m_wind, std::sin(a) * m_wind, m_updraft);
    }
    glUniform1f(U("uGust"),       m_gust);
    glUniform1f(U("uAttract"),    m_attract);
    glUniform3fv(U("uAttractPos"), 1, m_attractPos);
    // Lifetime → per-frame tick probability: 8 ticks over lifetime seconds
    // at the nominal 60 sim frames/s (× substeps).
    float lifeP = 0.0f;
    if (m_lifetime > 0.01f) {
        float frames = m_lifetime * 60.0f * (float)std::max(1, m_substeps);
        lifeP = std::min(1.0f, 8.0f / std::max(frames, 8.0f));
    }
    glUniform1f(U("uLifeP"), lifeP);
    glUniform1f(U("uEmitRate"), m_emitOn ? m_emitRate : 0.0f);
    glUniform1i(U("uEmitShape"), m_emitShape);
    glUniform3fv(U("uEmitPos"), 1, m_emitPos);
    glUniform1f(U("uEmitRadius"), m_emitRadius);
    {
        float v[3] = {0, 0, 0};
        if (m_emitShape == 1)      v[2] = -m_emitSpeed;   // rain falls
        else if (m_emitShape >= 2) v[2] =  m_emitSpeed;   // fountain / ring rise
        glUniform3fv(U("uEmitVel"), 1, v);
    }
    glUniform1f(U("uEmitSpread"), m_emitSpread);
    {
        float pos[kMaxOrbs * 4], mode[kMaxOrbs * 2]; int n = 0;
        for (int i = 0; i < kMaxOrbs; i++) {
            const Orb& o = m_orbs[i];
            if (!o.enabled || o.physics == 0) continue;
            pos[n*4+0] = o.worldPos[0]; pos[n*4+1] = o.worldPos[1]; pos[n*4+2] = o.worldPos[2];
            pos[n*4+3] = std::max(0.005f, o.size) * (float)m_size;
            mode[n*2+0] = (float)o.physics; mode[n*2+1] = o.force;
            n++;
        }
        glUniform1i(U("uSimOrbCount"), n);
        if (n > 0) { glUniform4fv(U("uSimOrb"), n, pos); glUniform2fv(U("uSimOrbMode"), n, mode); }
    }
}

void FluidSource3D::renderToOutput() {
    GLuint P = m_progRender;
    auto U = [&](const char* n) { return glGetUniformLocation(P, n); };
    glUseProgram(P);
    bindVolume(P, "uParticles", m_volC, 0);
    bindVolume(P, "uDensity",   m_volB, 1);
    bindVolume(P, "uShadow",    m_volD, 2);
    glUniform1f(U("uSize"), (float)m_size);
    glUniform2f(U("uRes"), (float)m_outW, (float)m_outH);
    glUniform1f(U("uTime"), m_time);
    glUniform1f(U("uRotSpeed"), m_autoRotate ? m_rotateSpeed : 0.0f);
    glUniform1f(U("uTilt"), m_tilt);
    glUniform3fv(U("uDeep"), 1, m_deepColor);
    glUniform3fv(U("uGlow"), 1, m_glowColor);
    glUniform1f(U("uBright"), m_brightness);
    // Color + lighting options
    glUniform3fv(U("uLightDir"), 1, m_lightDir);
    glUniform1f(U("uLightInt"), m_lightIntensity);
    glUniform1f(U("uAmbient"), m_ambient);
    glUniform1f(U("uSpec"), m_specular);
    glUniform1f(U("uRough"), m_roughness);
    glUniform1f(U("uFresnelPow"), std::max(0.5f, m_fresnel));
    glUniform1f(U("uRefract"), m_refract);
    glUniform1f(U("uSelfReflect"), m_selfReflect);
    glUniform3fv(U("uShallow"), 1, m_shallowColor);
    glUniform1f(U("uRim"), m_rim);
    glUniform1f(U("uSat"), m_saturation);
    glUniform1f(U("uVelGlow"), m_velGlow);
    glUniform1f(U("uFluidAlpha"), m_fluidAlpha);
    // Scene
    glUniform1i(U("uBgMode"), m_bgMode);
    glUniform3fv(U("uBgTop"), 1, m_bgTop);
    glUniform3fv(U("uBgBottom"), 1, m_bgBottom);
    glUniform1f(U("uBgAlpha"), m_bgAlpha);
    glUniform3fv(U("uHorizonCol"), 1, m_horizonColor);
    glUniform1f(U("uHorizonAmt"), m_horizonAmt);
    glUniform3fv(U("uSunCol"), 1, m_sunColor);
    glUniform1f(U("uSunInt"), m_sunIntensity);
    glUniform1f(U("uSunSize"), m_sunSize);
    {
        float az = deg2rad(m_sunAzimuth), el = deg2rad(m_sunElev);
        glUniform3f(U("uSunDir"), std::cos(el) * std::cos(az), std::cos(el) * std::sin(az), std::sin(el));
    }
    glUniform1f(U("uFog"), m_fog);
    glUniform1f(U("uVignette"), m_vignette);
    glUniform1f(U("uStars"), m_stars);
    glUniform1f(U("uFloorOn"), m_floorOn ? 1.0f : 0.0f);
    glUniform1f(U("uFloorGap"), m_floorGap);
    glUniform3fv(U("uFloorCol"), 1, m_floorColor);
    glUniform1f(U("uFloorGrid"), m_floorGrid);
    glUniform3fv(U("uGridCol"), 1, m_gridColor);
    glUniform1f(U("uFloorReflect"), m_floorReflect);
    glUniform1f(U("uFloorFade"), m_floorFade);
    glUniform1f(U("uCage"), m_cage);
    glUniform3fv(U("uCageCol"), 1, m_cageColor);
    glUniform1f(U("uCageWidth"), m_cageWidth);
    {
        // Pack enabled orbs contiguously.
        float pos[kMaxOrbs * 4], col[kMaxOrbs * 4]; int n = 0;
        for (int i = 0; i < kMaxOrbs; i++) {
            const Orb& o = m_orbs[i];
            if (!o.enabled) continue;
            pos[n*4+0] = o.worldPos[0]; pos[n*4+1] = o.worldPos[1]; pos[n*4+2] = o.worldPos[2];
            pos[n*4+3] = std::max(0.005f, o.size) * (float)m_size;
            col[n*4+0] = o.color[0]; col[n*4+1] = o.color[1]; col[n*4+2] = o.color[2];
            col[n*4+3] = std::max(0.0f, o.intensity);
            n++;
        }
        glUniform1i(U("uOrbCount"), n);
        if (n > 0) {
            glUniform4fv(U("uOrbPos"), n, pos);
            glUniform4fv(U("uOrbCol"), n, col);
        }
        glUniform1f(U("uOrbGlow"), m_orbGlow);
        glUniform1f(U("uOrbLight"), m_orbLight);
        glUniform1f(U("uOrbSpec"), m_orbSpec);
    }
    glUniform1f(U("uSphereScale"), m_sphereScale);
    glUniform1f(U("uParticleCube"), m_particleCube ? 1.0f : 0.0f);
    glUniform1i(U("uSurfaceMode"), m_surfaceMode);
    glUniform1f(U("uIsoLevel"), m_isoLevel);
    glUniform1f(U("uAgeShrink"), m_ageShrink);
    glUniform3fv(U("uAgeCol"), 1, m_ageColor);
    glUniform1f(U("uAgeTint"), m_ageTint);
    glUniform1f(U("uZoom"), m_zoom);
    glUniform1f(U("uCamDist"), m_camDist);
    glUniform1f(U("uCamHeight"), m_camHeight);
    glUniform1f(U("uAbsorb"), m_absorb);
    glUniform1f(U("uSubsurface"), m_subsurface);
    glUniform1f(U("uAO"), m_ao);
    glUniform1f(U("uKeySpec"), m_keySpec);
    // Image injection — colors the surface albedo (mapped to world x/y).
    bool imgOn = (m_imageEnabled && m_image.textureId != 0);
    glActiveTexture(GL_TEXTURE0 + 3);
    glBindTexture(GL_TEXTURE_2D, imgOn ? m_image.textureId : 0);
    glUniform1i(U("uImage"), 3);
    glUniform1i(U("uImageOn"), imgOn ? 1 : 0);
    glUniform1f(U("uImageMix"), m_imageMix);
    glUniform1f(U("uFlipY"), m_image.flippedV ? 1.0f : 0.0f);
    // Backdrop layer (background mode "Image").
    bool bgOn = (m_bgMode == 3 && m_backdrop.textureId != 0);
    glActiveTexture(GL_TEXTURE0 + 4);
    glBindTexture(GL_TEXTURE_2D, bgOn ? m_backdrop.textureId : 0);
    glUniform1i(U("uBgImage"), 4);
    glUniform1i(U("uBgImageOn"), bgOn ? 1 : 0);
    glUniform1f(U("uBgFlipY"), m_backdrop.flippedV ? 1.0f : 0.0f);
    glBindFramebuffer(GL_FRAMEBUFFER, m_hdr.fbo);
    glViewport(0, 0, m_hdr.w, m_hdr.h);
    glBindVertexArray(m_quadVAO);
    glDrawArrays(GL_TRIANGLES, 0, 3);
    renderPost();
}

// Bloom chain + filmic finish: HDR scene → ¼-res bright pass → H blur →
// V blur → composite/tonemap into the RGBA8 output the compositor samples.
void FluidSource3D::renderPost() {
    auto bind2D = [](GLuint prog, const char* name, GLuint tex, int unit) {
        glActiveTexture(GL_TEXTURE0 + unit);
        glBindTexture(GL_TEXTURE_2D, tex);
        glUniform1i(glGetUniformLocation(prog, name), unit);
    };
    glBindVertexArray(m_quadVAO);
    // 1. bright pass + downsample
    {
        GLuint P = m_progBloomDown; glUseProgram(P);
        bind2D(P, "uSrc", m_hdr.tex, 0);
        glUniform2f(glGetUniformLocation(P, "uTexel"), 1.0f / m_hdr.w, 1.0f / m_hdr.h);
        glUniform1f(glGetUniformLocation(P, "uThreshold"), std::max(0.0f, m_bloomThreshold));
        glBindFramebuffer(GL_FRAMEBUFFER, m_bloomA.fbo);
        glViewport(0, 0, m_bloomA.w, m_bloomA.h);
        glDrawArrays(GL_TRIANGLES, 0, 3);
    }
    // 2+3. separable blur, twice (wide soft halo)
    {
        GLuint P = m_progBloomBlur; glUseProgram(P);
        float tx = 1.0f / m_bloomA.w, ty = 1.0f / m_bloomA.h;
        for (int pass = 0; pass < 2; pass++) {
            bind2D(P, "uSrc", m_bloomA.tex, 0);
            glUniform2f(glGetUniformLocation(P, "uDir"), tx * (1.0f + pass), 0.0f);
            glBindFramebuffer(GL_FRAMEBUFFER, m_bloomB.fbo);
            glViewport(0, 0, m_bloomB.w, m_bloomB.h);
            glDrawArrays(GL_TRIANGLES, 0, 3);
            bind2D(P, "uSrc", m_bloomB.tex, 0);
            glUniform2f(glGetUniformLocation(P, "uDir"), 0.0f, ty * (1.0f + pass));
            glBindFramebuffer(GL_FRAMEBUFFER, m_bloomA.fbo);
            glViewport(0, 0, m_bloomA.w, m_bloomA.h);
            glDrawArrays(GL_TRIANGLES, 0, 3);
        }
    }
    // 4. finish
    {
        GLuint P = m_progPost; glUseProgram(P);
        auto U = [&](const char* n) { return glGetUniformLocation(P, n); };
        bind2D(P, "uScene", m_hdr.tex, 0);
        bind2D(P, "uAux",   m_hdrAux, 1);
        bind2D(P, "uBloom", m_bloomA.tex, 2);
        glUniform2f(U("uTexel"), 1.0f / m_hdr.w, 1.0f / m_hdr.h);
        glUniform1f(U("uTime"), m_time);
        glUniform1f(U("uExposure"), m_exposure);
        glUniform1f(U("uContrast"), m_contrast);
        glUniform1f(U("uBloomAmt"), m_bloom);
        glUniform1f(U("uSat"), m_saturation);
        glUniform1f(U("uVignette"), m_vignette);
        glUniform1f(U("uGrain"), m_grain);
        glUniform1f(U("uDof"), m_dof);
        glUniform1f(U("uDofFocus"), m_dofFocus);
        glBindFramebuffer(GL_FRAMEBUFFER, m_output.fbo);
        glViewport(0, 0, m_output.w, m_output.h);
        glDrawArrays(GL_TRIANGLES, 0, 3);
    }
}

// Park the GPU state while this source is reachable only from undo/redo
// snapshots. Called from UndoStack::suspendOrphanedSources on the main-thread
// frame sweep — the GL context is current there, so the deletes are safe.
// Frees the 4 sim volumes (the VRAM hog: at size3d=128 the two RGBA32UI
// particle cubes are ~33 MB each and the two RGBA16F cubes ~17 MB each,
// ~100 MB total) plus the output FBO. Sim state is lost — accepted for
// undo-orphaned sources; revival re-runs the seed block. Programs, the quad
// and the layered-render FBO handle stay resident (trivial).
void FluidSource3D::suspend() {
    if (!m_ready) return;
    destroyVolume(m_volA); destroyVolume(m_volB);
    destroyVolume(m_volC); destroyVolume(m_volD);
    destroyFBO(m_output);
    destroyPostTargets();
    m_ready = false;
    m_suspended = true;
}

void FluidSource3D::update() {
    if (!m_ready) {
        // Lazy revive after suspend(): recreate the volumes at the current
        // m_simRes (reallocVolumes also resets m_frame so the seed block
        // re-runs) and the output at the current zone size × render scale
        // (reallocOutput sees the destroyed FBO's 0×0 dims and reallocs).
        if (!m_suspended || !m_progRender) return;
        m_suspended = false;   // one attempt — no retry storm on failure
        reallocVolumes();
        reallocOutput();
        m_lastTime = glfwGetTime();   // no giant dt on the first revived frame
        m_ready = true;
    }
    // Optional fps diagnostic — set EASEL_F3D_FPS=1 in the environment.
    static const bool s_fpsLog = (std::getenv("EASEL_F3D_FPS") != nullptr);
    if (s_fpsLog) {
        static int fc = 0; static double t0 = glfwGetTime();
        double n = glfwGetTime();
        if (++fc >= 120) {
            std::cerr << "[FluidSource3D] " << (fc / (n - t0)) << " fps  size="
                      << m_size << " out=" << m_outW << "x" << m_outH << std::endl;
            fc = 0; t0 = n;
        }
    }
    GLint prevFBO = 0; glGetIntegerv(GL_FRAMEBUFFER_BINDING, &prevFBO);
    GLint prevVP[4]; glGetIntegerv(GL_VIEWPORT, prevVP);
    GLboolean prevBlend = glIsEnabled(GL_BLEND);
    GLboolean prevDepth = glIsEnabled(GL_DEPTH_TEST);

    double now = glfwGetTime();
    float dt = (float)(now - m_lastTime);
    m_lastTime = now;
    if (dt <= 0.0f) dt = 0.016f;
    m_time += dt;

    glDisable(GL_BLEND);
    glDisable(GL_DEPTH_TEST);

    // Honor live sim-res / render-scale changes from the PropertyPanel.
    if (m_simRes != m_size) reallocVolumes();
    reallocOutput();   // early-returns unless the scaled dims changed
    updateDerived(dt);

    // Switching the container box<->sphere, changing the fill amount or the
    // particle density needs a reseed: the wall force only acts near the
    // surface, so the fluid volume only changes when the seed block re-runs.
    {
        bool wantSphere = (m_sphereShape > 0.5f);
        if (wantSphere != m_seedingSphere) { m_seedingSphere = wantSphere; m_frame = 0; }
        if (std::fabs(m_fillAmount - m_fillPrev) > 0.001f) { m_fillPrev = m_fillAmount; m_frame = 0; }
        if (m_doubleDensity != m_doublePrev) { m_doublePrev = m_doubleDensity; m_frame = 0; }
    }

    int steps = std::max(1, m_substeps);
    for (int s = 0; s < steps; s++) {
        // A: volC(prev) → volA
        glUseProgram(m_progA);
        setSimUniforms(m_progA);
        bindVolume(m_progA, "uParticles", m_volC, 0);
        runVolumePass(m_progA, m_volA);
        // B: volA → volB
        glUseProgram(m_progB);
        bindVolume(m_progB, "uParticles", m_volA, 0);
        runVolumePass(m_progB, m_volB);
        // C: volA + volB → volC
        glUseProgram(m_progC);
        setSimUniforms(m_progC);
        bindVolume(m_progC, "uParticles", m_volA, 0);
        bindVolume(m_progC, "uDensity",   m_volB, 1);
        glUniform1i(glGetUniformLocation(m_progC, "uFrame"), m_frame);
        glUniform1f(glGetUniformLocation(m_progC, "uTime"), m_time);
        runVolumePass(m_progC, m_volC);
        // D: volB → volD
        glUseProgram(m_progD);
        bindVolume(m_progD, "uDensity", m_volB, 0);
        glUniform3fv(glGetUniformLocation(m_progD, "uLightDir"), 1, m_lightDir); // shadow march dir
        runVolumePass(m_progD, m_volD);
        m_frame++;
    }

    renderToOutput();

    glBindFramebuffer(GL_FRAMEBUFFER, prevFBO);
    glViewport(prevVP[0], prevVP[1], prevVP[2], prevVP[3]);
    glBindVertexArray(0);
    glUseProgram(0);
    glActiveTexture(GL_TEXTURE0);
    if (prevBlend) glEnable(GL_BLEND); else glDisable(GL_BLEND);
    if (prevDepth) glEnable(GL_DEPTH_TEST); else glDisable(GL_DEPTH_TEST);
}

// ── Param table ───────────────────────────────────────────────────────
// ONE list of every scalar tunable. ids are stable (project files, OSC,
// bindings). Flags: kBind = offered for audio/MIDI binding + Show page;
// kLook = morphs between the VJ Low/High looks.
std::vector<FluidSource3D::ParamDef> FluidSource3D::params() {
    const unsigned BL = kBind | kLook;
    std::vector<ParamDef> t = {
        // camera / render
        { "brightness",     "Brightness",     &m_brightness,     0.0f,   6.0f,  BL },
        { "rotateSpeed",    "Spin Speed",     &m_rotateSpeed,    0.0f,   2.0f,  BL },
        { "tilt",           "Tilt",           &m_tilt,          -1.57f,  1.57f, BL },
        { "zoom",           "Zoom",           &m_zoom,           0.5f,   4.0f,  BL },
        { "audioIntensity", "Audio Motion",   &m_audioIntensity, 0.0f,   1.0f,  kBind },
        // physics
        { "gravity",        "Gravity",        &m_gravity,        0.0f,   4.0f,  BL },
        { "gravityTiltX",   "Gravity Tilt X", &m_gravityTiltX,  -1.5f,   1.5f,  BL },
        { "gravityTiltY",   "Gravity Tilt Y", &m_gravityTiltY,  -1.5f,   1.5f,  BL },
        { "vortex",         "Vortex",         &m_vortex,         0.0f,   3.0f,  BL },
        { "vortexLift",     "Vortex Lift",    &m_vortexLift,    -1.0f,   1.0f,  BL },
        { "turbulence",     "Turbulence",     &m_turbulence,     0.0f,   2.0f,  BL },
        { "turbScale",      "Turb Scale",     &m_turbScale,      0.25f,  4.0f,  BL },
        { "turbSpeed",      "Turb Speed",     &m_turbSpeed,      0.0f,   4.0f,  BL },
        { "forceScale",     "Cohesion",       &m_forceScale,     0.1f,   4.0f,  BL },
        { "viscosity",      "Viscosity",      &m_viscosity,      0.0f,   4.0f,  BL },
        { "tension",        "Surface Tension",&m_tension,        0.0f,   3.0f,  BL },
        { "restDensity",    "Rest Density",   &m_restDensity,    0.4f,   2.5f,  BL },
        { "maxVel",         "Speed Limit",    &m_maxVel,         0.3f,   4.0f,  BL },
        { "damping",        "Damping",        &m_damping,        0.0f,   1.0f,  BL },
        { "bounce",         "Wall Bounce",    &m_bounce,         0.2f,   3.0f,  BL },
        { "simSpeed",       "Sim Speed",      &m_simSpeed,       0.2f,   1.4f,  BL },
        { "wind",           "Wind",           &m_wind,           0.0f,   3.0f,  BL },
        { "windAngle",      "Wind Angle",     &m_windAngle,      0.0f, 360.0f,  BL },
        { "updraft",        "Updraft",        &m_updraft,       -2.0f,   2.0f,  BL },
        { "gust",           "Gust",           &m_gust,           0.0f,   1.0f,  BL },
        { "attract",        "Attract",        &m_attract,       -2.0f,   2.0f,  BL },
        { "attractX",       "Attract X",      &m_attractPos[0],  0.0f,   1.0f,  BL },
        { "attractY",       "Attract Y",      &m_attractPos[1],  0.0f,   1.0f,  BL },
        { "attractZ",       "Attract Z",      &m_attractPos[2],  0.0f,   1.0f,  BL },
        { "sphereShape",    "Container",      &m_sphereShape,    0.0f,   1.0f,  kBind },
        // particles
        { "sphereScale",    "Blob Size",      &m_sphereScale,    0.05f,  1.5f,  BL },
        { "isoLevel",       "Surface Level",  &m_isoLevel,       0.02f,  1.0f,  BL },
        { "lifetime",       "Lifetime",       &m_lifetime,       0.0f,  20.0f,  BL },
        { "ageShrink",      "Shrink With Age",&m_ageShrink,      0.0f,   1.0f,  BL },
        { "ageTint",        "Age Tint",       &m_ageTint,        0.0f,   1.0f,  BL },
        { "emitRate",       "Emit Rate",      &m_emitRate,       0.0f,   1.0f,  BL },
        { "emitSpeed",      "Emit Speed",     &m_emitSpeed,      0.0f,   2.0f,  BL },
        { "emitSpread",     "Emit Spread",    &m_emitSpread,     0.0f,   1.0f,  BL },
        { "emitRadius",     "Emit Radius",    &m_emitRadius,     0.02f,  0.5f,  BL },
        { "emitX",          "Emit X",         &m_emitPos[0],     0.0f,   1.0f,  BL },
        { "emitY",          "Emit Y",         &m_emitPos[1],     0.0f,   1.0f,  BL },
        { "emitZ",          "Emit Z",         &m_emitPos[2],     0.0f,   1.0f,  BL },
        { "velGlow",        "Speed Glow",     &m_velGlow,        0.0f,   3.0f,  BL },
        { "fluidAlpha",     "Fluid Opacity",  &m_fluidAlpha,     0.0f,   1.0f,  BL },
        // light / material
        { "lightAzimuth",   "Light Angle",    &m_lightAzimuth,   0.0f, 360.0f,  BL },
        { "lightElev",      "Light Height",   &m_lightElev,    -20.0f,  90.0f,  BL },
        { "lightIntensity", "Light Intensity",&m_lightIntensity, 0.0f,   3.0f,  BL },
        { "ambient",        "Ambient",        &m_ambient,        0.0f,   1.0f,  BL },
        { "specular",       "Reflection",     &m_specular,       0.0f,   3.0f,  BL },
        { "roughness",      "Roughness",      &m_roughness,      0.0f,   1.0f,  BL },
        { "fresnel",        "Fresnel",        &m_fresnel,        0.5f,   8.0f,  BL },
        { "refract",        "Glass",          &m_refract,        0.0f,   1.0f,  BL },
        { "selfReflect",    "Self Reflect",   &m_selfReflect,    0.0f,   1.0f,  BL },
        { "rim",            "Rim Amount",     &m_rim,            0.0f,   1.0f,  BL },
        { "saturation",     "Saturation",     &m_saturation,     0.0f,   2.0f,  BL },
        { "imageMix",       "Image Amount",   &m_imageMix,       0.0f,   1.0f,  kBind },
        // scene
        { "bgAlpha",        "BG Opacity",     &m_bgAlpha,        0.0f,   1.0f,  BL },
        { "horizonAmt",     "Horizon",        &m_horizonAmt,     0.0f,   3.0f,  BL },
        { "sunIntensity",   "Sun",            &m_sunIntensity,   0.0f,   4.0f,  BL },
        { "sunSize",        "Sun Sharpness",  &m_sunSize,        4.0f, 200.0f,  BL },
        { "sunAzimuth",     "Sun Angle",      &m_sunAzimuth,     0.0f, 360.0f,  BL },
        { "sunElev",        "Sun Height",     &m_sunElev,      -10.0f,  90.0f,  BL },
        { "fog",            "Fog",            &m_fog,            0.0f,   1.0f,  BL },
        { "vignette",       "Vignette",       &m_vignette,       0.0f,   1.0f,  BL },
        { "stars",          "Stars",          &m_stars,          0.0f,   1.0f,  BL },
        { "floorGap",       "Floor Distance", &m_floorGap,       0.0f,   1.0f,  BL },
        { "floorGrid",      "Floor Grid",     &m_floorGrid,      0.0f,   3.0f,  BL },
        { "floorReflect",   "Floor Mirror",   &m_floorReflect,   0.0f,   1.0f,  BL },
        { "floorFade",      "Floor Fade",     &m_floorFade,      0.0f,   4.0f,  BL },
        { "cage",           "Cage Glow",      &m_cage,           0.0f,   3.0f,  BL },
        { "cageWidth",      "Cage Width",     &m_cageWidth,      0.1f,   3.0f,  BL },
        { "orbGlow",        "Orb Halo",       &m_orbGlow,        0.0f,   3.0f,  BL },
        { "orbLight",       "Orb Light",      &m_orbLight,       0.0f,   3.0f,  BL },
        { "orbSpec",        "Orb Reflect",    &m_orbSpec,        0.0f,   3.0f,  BL },
        // liquid material (depth / occlusion / highlights)
        { "absorb",         "Depth Color",    &m_absorb,         0.0f,   4.0f,  BL },
        { "subsurface",     "Subsurface",     &m_subsurface,     0.0f,   2.0f,  BL },
        { "ao",             "Occlusion",      &m_ao,             0.0f,   3.0f,  BL },
        { "keySpec",        "Highlight",      &m_keySpec,        0.0f,   3.0f,  BL },
        { "camDist",        "Distance",       &m_camDist,        0.6f,   2.2f,  BL },
        { "camHeight",      "Aim Height",     &m_camHeight,     -0.5f,   0.5f,  BL },
        // finish (post)
        { "exposure",       "Exposure",       &m_exposure,       0.2f,   3.0f,  BL },
        { "contrast",       "Contrast",       &m_contrast,       0.0f,   1.0f,  BL },
        { "bloom",          "Bloom",          &m_bloom,          0.0f,   2.0f,  BL },
        { "bloomThreshold", "Bloom Threshold",&m_bloomThreshold, 0.0f,   2.0f,  BL },
        { "grain",          "Grain",          &m_grain,          0.0f,   1.0f,  BL },
        { "dof",            "Depth of Field", &m_dof,            0.0f,   1.0f,  BL },
        { "dofFocus",       "Focus",          &m_dofFocus,       0.0f,   1.0f,  BL },
    };
    static const char* kOrbIds[kMaxOrbs][7] = {
        {"orb1Intensity","orb1Size","orb1Speed","orb1Radius","orb1Height","orb1Phase","orb1Force"},
        {"orb2Intensity","orb2Size","orb2Speed","orb2Radius","orb2Height","orb2Phase","orb2Force"},
        {"orb3Intensity","orb3Size","orb3Speed","orb3Radius","orb3Height","orb3Phase","orb3Force"},
        {"orb4Intensity","orb4Size","orb4Speed","orb4Radius","orb4Height","orb4Phase","orb4Force"},
    };
    static const char* kOrbLbl[kMaxOrbs][7] = {
        {"Orb 1 Intensity","Orb 1 Size","Orb 1 Speed","Orb 1 Radius","Orb 1 Height","Orb 1 Phase","Orb 1 Force"},
        {"Orb 2 Intensity","Orb 2 Size","Orb 2 Speed","Orb 2 Radius","Orb 2 Height","Orb 2 Phase","Orb 2 Force"},
        {"Orb 3 Intensity","Orb 3 Size","Orb 3 Speed","Orb 3 Radius","Orb 3 Height","Orb 3 Phase","Orb 3 Force"},
        {"Orb 4 Intensity","Orb 4 Size","Orb 4 Speed","Orb 4 Radius","Orb 4 Height","Orb 4 Phase","Orb 4 Force"},
    };
    for (int i = 0; i < kMaxOrbs; i++) {
        Orb& o = m_orbs[i];
        t.push_back({ kOrbIds[i][0], kOrbLbl[i][0], &o.intensity,   0.0f,  5.0f,  BL });
        t.push_back({ kOrbIds[i][1], kOrbLbl[i][1], &o.size,        0.01f, 0.4f,  BL });
        t.push_back({ kOrbIds[i][2], kOrbLbl[i][2], &o.speed,      -3.0f,  3.0f,  BL });
        t.push_back({ kOrbIds[i][3], kOrbLbl[i][3], &o.orbitRadius, 0.0f,  1.4f,  BL });
        t.push_back({ kOrbIds[i][4], kOrbLbl[i][4], &o.orbitHeight,-0.3f,  1.3f,  BL });
        t.push_back({ kOrbIds[i][5], kOrbLbl[i][5], &o.phase,       0.0f, 360.0f, kLook });
        t.push_back({ kOrbIds[i][6], kOrbLbl[i][6], &o.force,       0.0f,  3.0f,  BL });
    }
    return t;
}

std::vector<FluidSource3D::ColorDef> FluidSource3D::colors() {
    std::vector<ColorDef> t = {
        { "deepColor",    "Liquid",   m_deepColor },
        { "glowColor",    "Glow",     m_glowColor },
        { "shallowColor", "Rim",      m_shallowColor },
        { "ageColor",     "Age Tint", m_ageColor },
        { "bgTop",        "BG Top",   m_bgTop },
        { "bgBottom",     "BG Bottom",m_bgBottom },
        { "horizonColor", "Horizon",  m_horizonColor },
        { "sunColor",     "Sun",      m_sunColor },
        { "floorColor",   "Floor",    m_floorColor },
        { "gridColor",    "Grid",     m_gridColor },
        { "cageColor",    "Cage",     m_cageColor },
    };
    static const char* kOrbCol[kMaxOrbs] = {"orb1Color","orb2Color","orb3Color","orb4Color"};
    static const char* kOrbLbl[kMaxOrbs] = {"Orb 1","Orb 2","Orb 3","Orb 4"};
    for (int i = 0; i < kMaxOrbs; i++) t.push_back({ kOrbCol[i], kOrbLbl[i], m_orbs[i].color });
    return t;
}

const FluidSource3D::ParamDef* FluidSource3D::findParam(const std::string& id) {
    static thread_local std::vector<ParamDef> cache;
    cache = params();
    for (const auto& d : cache) if (id == d.id) return &d;
    return nullptr;
}

void FluidSource3D::copyConfigFrom(const FluidSource3D& o) {
    auto& src = const_cast<FluidSource3D&>(o);
    auto mine = params(); auto theirs = src.params();
    for (size_t i = 0; i < mine.size() && i < theirs.size(); i++) *mine[i].ptr = *theirs[i].ptr;
    auto mc = colors(); auto tc = src.colors();
    for (size_t i = 0; i < mc.size() && i < tc.size(); i++)
        for (int k = 0; k < 3; k++) mc[i].rgb[k] = tc[i].rgb[k];
    for (int i = 0; i < kMaxOrbs; i++) {
        m_orbs[i].enabled = o.m_orbs[i].enabled; m_orbs[i].motion = o.m_orbs[i].motion;
        m_orbs[i].physics = o.m_orbs[i].physics;
        for (int k = 0; k < 3; k++) m_orbs[i].pos[k] = o.m_orbs[i].pos[k];
    }
    m_autoRotate = o.m_autoRotate; m_particleCube = o.m_particleCube; m_surfaceMode = o.m_surfaceMode;
    m_doubleDensity = o.m_doubleDensity; m_fillAmount = o.m_fillAmount;
    m_emitOn = o.m_emitOn; m_emitShape = o.m_emitShape;
    m_bgMode = o.m_bgMode; m_floorOn = o.m_floorOn;
    m_imageEnabled = o.m_imageEnabled; m_image.sourceLayerId = o.m_image.sourceLayerId;
    m_backdrop.sourceLayerId = o.m_backdrop.sourceLayerId;
    m_simRes = o.m_simRes; m_substeps = o.m_substeps; m_renderScale = o.m_renderScale;
    m_vjMode = o.m_vjMode; m_vjGrab = o.m_vjGrab; m_journeySignal = o.m_journeySignal;
    m_journeyGain = o.m_journeyGain; m_journeyPosManual = o.m_journeyPosManual;
    for (int i = 0; i < 2; i++) { m_look[i] = o.m_look[i]; m_lookSet[i] = o.m_lookSet[i]; }
    m_audioBindings = o.m_audioBindings;
}

// ── VJ looks: snapshot the current look, and crossfade between the two ──
void FluidSource3D::captureLook(int which) {
    if (which < 0 || which > 1) return;
    FluidLook& L = m_look[which];
    L.v.clear();
    for (const auto& d : params()) if (d.flags & kLook) L.v[d.id] = *d.ptr;
    static const char* sfx[3] = {".r", ".g", ".b"};
    for (const auto& c : colors())
        for (int k = 0; k < 3; k++) L.v[std::string(c.id) + sfx[k]] = c.rgb[k];
    m_lookSet[which] = true;
}

void FluidSource3D::loadLook(int which) {
    if (which < 0 || which > 1) return;
    const FluidLook& L = m_look[which];
    for (const auto& d : params()) {
        auto it = L.v.find(d.id);
        if (it != L.v.end()) *d.ptr = it->second;
    }
    static const char* sfx[3] = {".r", ".g", ".b"};
    for (const auto& c : colors())
        for (int k = 0; k < 3; k++) {
            auto it = L.v.find(std::string(c.id) + sfx[k]);
            if (it != L.v.end()) c.rgb[k] = it->second;
        }
}

// Pre-2026-10 projects stored looks as a flat 30-float array in this order.
void FluidSource3D::lookFromArray(int which, const float* in) {
    if (which < 0 || which > 1) return;
    FluidLook& L = m_look[which]; L.v.clear(); int n = 0;
    const char* cols[5] = {"deepColor", "glowColor", "shallowColor", "bgTop", "bgBottom"};
    static const char* sfx[3] = {".r", ".g", ".b"};
    for (int c = 0; c < 5; c++) for (int k = 0; k < 3; k++) L.v[std::string(cols[c]) + sfx[k]] = in[n++];
    const char* ids[15] = {"brightness","lightIntensity","ambient","specular","rim","saturation",
                           "bgAlpha","sphereScale","zoom","gravity","vortex","turbulence",
                           "forceScale","rotateSpeed","tilt"};
    for (int i = 0; i < 15; i++) L.v[ids[i]] = in[n++];
    m_lookSet[which] = true;
}

void FluidSource3D::applyJourney(float level, float energy, float build,
                                 float momentum, float drop, float dt) {
    if (!(dt > 0.0f)) dt = 1.0f/60.0f; if (dt > 0.1f) dt = 0.1f;

    // On a mode switch, set up the live params for the new mode: load the look
    // you're about to edit (so the sliders show it), or ensure both ends exist
    // before performing.
    if (m_vjMode != m_vjModePrev) {
        if (m_vjMode == 0)      { if (m_lookSet[0]) loadLook(0); else captureLook(0); }
        else if (m_vjMode == 1) { if (m_lookSet[1]) loadLook(1); else captureLook(1); }
        else { if (!m_lookSet[0]) captureLook(0); if (!m_lookSet[1]) captureLook(1); }
        m_vjModePrev = m_vjMode;
    }

    // Edit modes (Low/High): the live params ARE the look. Snapshot them every
    // frame so every tweak updates that end — full manual control, no lock-in,
    // and no morph runs.
    if (m_vjMode != 2) { captureLook(m_vjMode); return; }

    // Live: crossfade Low <-> High by the song, or by a hand-grabbed fader.
    if (!m_lookSet[0] || !m_lookSet[1]) return;
    float pos;
    if (m_vjGrab) {
        pos = m_journeyPos = m_journeyPosManual;
    } else {
        float drive = energy;
        if      (m_journeySignal == 1) drive = build;
        else if (m_journeySignal == 2) drive = level;
        else if (m_journeySignal == 3) drive = momentum;
        drive *= m_journeyGain;
        float lead = (momentum - 0.5f) * 2.0f;            // rising energy leads
        float target = drive + 0.25f * std::max(lead, 0.0f);
        if (target < 0.0f) target = 0.0f; if (target > 1.0f) target = 1.0f;
        // Drop snap: jump toward Peak when a drop lands, then decay (~0.5s).
        m_journeyDropEnv = std::max(m_journeyDropEnv * std::exp(-2.0f*dt), drop);
        target = std::max(target, m_journeyDropEnv);
        // Asymmetric glide: rise fast (ride builds/drops), fall slower (linger).
        float rate = (target > m_journeyPos) ? 3.5f : 1.1f;
        m_journeyPos += (target - m_journeyPos) * (1.0f - std::exp(-rate*dt));
        pos = m_journeyPos;
    }
    // Safety: a non-finite or out-of-range position would write garbage into
    // every look param. Clamp hard so the morph can never break the visual.
    if (!std::isfinite(pos)) pos = 0.0f;
    if (pos < 0.0f) pos = 0.0f; else if (pos > 1.0f) pos = 1.0f;
    m_journeyPos = pos;
    const FluidLook& A = m_look[0]; const FluidLook& B = m_look[1];
    auto lerpKey = [&](const std::string& key, float* dst) {
        auto a = A.v.find(key), b = B.v.find(key);
        if (a == A.v.end() || b == B.v.end()) return;
        *dst = a->second + (b->second - a->second) * pos;
    };
    for (const auto& d : params()) if (d.flags & kLook) lerpKey(d.id, d.ptr);
    static const char* sfx[3] = {".r", ".g", ".b"};
    for (const auto& c : colors())
        for (int k = 0; k < 3; k++) lerpKey(std::string(c.id) + sfx[k], &c.rgb[k]);
}

void FluidSource3D::applyAudioBindings(float level, float bass, float mid,
                                       float high, float beat, float dt,
                                       MIDIManager* midi,
                                       float energy, float build, float drop,
                                       float silence, float momentum) {
    // Track overall loudness (attack fast, release slow) for the calm->intense
    // motion drive — independent of explicit per-param bindings.
    float aRate = (level > m_audioEnergy) ? 6.0f : 1.5f;
    m_audioEnergy += (level - m_audioEnergy) * (1.0f - std::exp(-aRate * std::max(dt, 1e-3f)));

    // Journey morph FIRST (sets the base look between Valley/Peak), so any
    // explicit per-param bindings below can still layer accents on top.
    applyJourney(level, energy, build, momentum, drop, dt);

    if (m_audioBindings.empty()) return;
    auto table = params();
    for (auto& [name, b] : m_audioBindings) {
        if (b.signal == AudioSignal::None) continue;
        float raw = 0.0f; bool have = true;
        switch (b.signal) {
            case AudioSignal::Level: raw = level; break;
            case AudioSignal::Bass:  raw = bass;  break;
            case AudioSignal::Mid:   raw = mid;   break;
            case AudioSignal::High:  raw = high;  break;
            case AudioSignal::Beat:  raw = beat;  break;
            case AudioSignal::Energy:   raw = energy;   break;
            case AudioSignal::Build:    raw = build;    break;
            case AudioSignal::Drop:     raw = drop;     break;
            case AudioSignal::Silence:  raw = silence;  break;
            case AudioSignal::Momentum: raw = momentum; break;
            case AudioSignal::MidiCC: {
                if (midi && b.midiCC >= 0) {
                    float v = midi->getCCValue(b.midiChannel, b.midiCC);
                    if (v < 0.0f) have = false; else raw = v;
                } else have = false;
                break;
            }
            default: have = false; break;
        }
        if (!have) continue;
        float mapped = b.follow(raw, dt);
        for (const auto& d : table) {
            if (name != d.id) continue;
            if (mapped < d.lo) mapped = d.lo; if (mapped > d.hi) mapped = d.hi;
            *d.ptr = mapped;
            break;
        }
    }
}

FluidSource3D::~FluidSource3D() {
    if (m_quadVBO) glDeleteBuffers(1, &m_quadVBO);
    if (m_quadVAO) glDeleteVertexArrays(1, &m_quadVAO);
    if (m_volFBO)  glDeleteFramebuffers(1, &m_volFBO);
    destroyVolume(m_volA); destroyVolume(m_volB);
    destroyVolume(m_volC); destroyVolume(m_volD);
    destroyFBO(m_output);
    destroyPostTargets();
    GLuint progs[] = { m_progA, m_progB, m_progC, m_progD, m_progRender,
                       m_progBloomDown, m_progBloomBlur, m_progPost };
    for (GLuint p : progs) if (p) glDeleteProgram(p);
}

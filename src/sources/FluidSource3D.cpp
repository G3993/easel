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
// SPH constants stay as #defines (faithful to the shadertoy); M5 promotes
// the tunable ones to uniforms.
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

vec3 size3d;

#define PI 3.1415926535
#define TWO_PI 6.28318530718
uniform vec3 uLightDir;
#define light_dir normalize(uLightDir)
#define surface_tension 0.5
#define surface_tension_rad 2.0
#define initial_particle_density 1u
#define dt 0.7
#define rest_density 1.0
#define gravity 0.01
#define force_k 0.15
#define force_coef_a -4.0
#define force_boundary 5.0
#define max_velocity 2.0
#define GD(x, R) (exp(-dot((x)/(R),(x)/(R)))/((R)*(R)))
#define range(i,a,b) for(int i=a;i<=b;i++)

ivec3 cellClamp(vec3 p){ return clamp(ivec3(p), ivec3(0), ivec3(int(uSize)-1)); }
uvec4 LOAD3D(usampler3D ch, vec3 p){ return texelFetch(ch, cellClamp(p), 0); }
vec4  LOAD3D(sampler3D ch, vec3 p){ return texelFetch(ch, cellClamp(p), 0); }
vec4  trilinear(sampler3D ch, vec3 p){ return texture(ch, (p)/vec3(uSize)); }

struct Particle { uint mass; vec3 pos; vec3 vel; vec3 force; float density; };

// 5 bits shared exponent, 9 bits per component.
uint packvec3(vec3 v){
    float maxv = max(abs(v.x), max(abs(v.y), abs(v.z)));
    int expo = clamp(int(ceil(log2(maxv))), -15, 15);
    float scale = exp2(-float(expo));
    uvec3 sv = uvec3(round(clamp(v*scale, -1.0, 1.0) * 255.0) + 255.0);
    return uint(expo + 15) | (sv.x << 5) | (sv.y << 14) | (sv.z << 23);
}
vec3 unpackvec3(uint packed){
    int expo = int(packed & 0x1Fu) - 15;
    vec3 sv = vec3((packed >> 5) & 0x1FFu, (packed >> 14) & 0x1FFu, (packed >> 23) & 0x1FFu);
    vec3 v = (sv - 255.0) / 255.0;
    v *= exp2(float(expo));
    return v;
}

uvec4 packParticles(Particle p0, Particle p1, vec3 pos){
    p0.pos -= pos; p1.pos -= pos;
    uvec3 pos0 = uvec3(clamp(p0.pos, 0.0, 1.0) * 255.0);
    uvec3 pos1 = uvec3(clamp(p1.pos, 0.0, 1.0) * 255.0);
    uint data1 = p0.mass | (p1.mass << 8) | (pos0.x << 16) | (pos0.y << 24);
    uint data2 = pos0.z | (pos1.x << 8) | (pos1.y << 16) | (pos1.z << 24);
    uint data3 = packvec3(p0.vel);
    uint data4 = packvec3(p1.vel);
    return uvec4(data1, data2, data3, data4);
}
void unpackParticles(uvec4 d, vec3 pos, out Particle p0, out Particle p1){
    p0.mass = d.x & 0xFFu;
    p1.mass = (d.x >> 8) & 0xFFu;
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
    uint newMass = p.mass + incoming.mass;
    vec2 weight = vec2(p.mass, incoming.mass) / float(newMass);
    p.pos = p.pos*weight.x + incoming.pos*weight.y;
    p.vel = p.vel*weight.x + incoming.vel*weight.y;
    p.mass = newMass;
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
    float F = surface_tension*GD(d, surface_tension_rad);
    float Friction = 0.45 * dot(dir, dvel) * GD(d, 1.5);
    p.force += force_k * dir * (F + SPH_F + Friction) * irho / rest_density;
}

float minv(vec3 a){ return min(min(a.x, a.y), a.z); }
float maxv(vec3 a){ return max(max(a.x, a.y), a.z); }
float distance2border(vec3 p){
    vec3 a = vec3(size3d - 1.) - p;
    return min(minv(p), minv(a)) + 1.;
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
    // churning (large) when it's loud. Downward gravity (-1) is unaffected.
    p.force += gravity*vec3(0.4*sin(0.7*time)*uEnergy, 0.2*cos(0.5*time)*uEnergy, -1.0);
    p.vel *= mix(0.985, 1.0, clamp(uEnergy, 0.0, 1.0));   // extra damping when calm → settles
    vec4 border = border_grad(p.pos);
    vec3 bound = 1.*normalize(border.xyz)*exp(-0.4*border.w*border.w);
    p.force += force_boundary*bound*dt;
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
    p0.mass = 0u; p0.pos = vec3(0); p0.vel = vec3(0);
    p1.mass = 0u; p1.pos = vec3(0); p1.vel = vec3(0);
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
    pV.mass = 0u; pV.density = 0.0; pV.pos = pos + 0.5;
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

// ── Buffer C: forces + integrate + seed ───────────────────────────────
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
    if(uFrame < 10){
        if(pos.x < 0.5*size3d.x && pos.x > 0.0*size3d.x &&
           pos.y < 0.85*size3d.y && pos.y > 0.15*size3d.y &&
           pos.z < 0.85*size3d.z && pos.z > 0.15*size3d.z){
            p0.mass = initial_particle_density; p1.mass = 0u;
        }
        p0.pos = pos; p0.vel = vec3(0.0);
        p1.pos = pos; p1.vel = vec3(0.0);
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

// ── Image: raymarch the particle spheres ──────────────────────────────
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
uniform vec3  uShallow;    // rim / grazing-edge tint
uniform float uRim;        // rim amount (0 = off / original)
uniform float uSat;        // output saturation (1 = unchanged)
uniform vec3  uBgTop;      // background gradient top (also reflection env)
uniform vec3  uBgBottom;   // background gradient bottom
uniform float uBgAlpha;    // background opacity (0 = transparent / composites under)
uniform float uSphereScale;// particle sphere size multiplier (1 = original)
uniform sampler2D uImage;
uniform int   uImageOn;
uniform float uImageMix;
uniform float uFlipY;
out vec4 fragColor;

#define FOV 2.5
#define MAX_DIST 1e10

mat3 getCamera(vec2 a){
    mat3 t = mat3(1,0,0, 0,cos(a.y),-sin(a.y), 0,sin(a.y),cos(a.y));
    mat3 p = mat3(cos(a.x),sin(a.x),0, -sin(a.x),cos(a.x),0, 0,0,1);
    return t*p;
}
vec3 getRay(vec2 a, vec2 pos){
    mat3 cam = getCamera(a);
    return normalize(transpose(cam)*vec3(FOV*pos.x, 1.0, FOV*pos.y));
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
vec3 env(vec3 d){
    d = normalize(d);
    float t = clamp(0.5 + 0.5*d.y, 0.0, 1.0);
    vec3 col = mix(uBgBottom, uBgTop, t);
    col += vec3(0.06,0.04,0.10) * pow(1.0 - abs(d.y), 4.0);   // warm horizon rim
    // Bright key light → tight specular highlight that slides across the
    // reflective surface, which is what reads as crisp + 3D.
    vec3 sun = normalize(vec3(0.6, 0.7, 0.4));
    float s = max(dot(d, sun), 0.0);
    col += vec3(1.0,0.96,0.90) * pow(s, 80.0) * 1.6;
    col += vec3(0.50,0.55,0.72) * pow(s, 6.0) * 0.25;
    return col;
}
float Density(vec3 p){ return trilinear(uDensity, p).z; }
float Shadow(vec3 p){ return exp(-trilinear(uShadow, p).x) + 0.05; }
vec4 calcNormal(vec3 p, float dx){
    const vec3 k = vec3(1,-1,0);
    return (k.xyyx*Density(p + k.xyy*dx) +
            k.yyxx*Density(p + k.yyx*dx) +
            k.yxyx*Density(p + k.yxy*dx) +
            k.xxxx*Density(p + k.xxx*dx))/vec4(4.*dx,4.*dx,4.*dx,4.);
}
void TraceCell(inout Ray ray, vec3 p){
    Particle p0, p1;
    unpackParticles(LOAD3D(uParticles, p), p, p0, p1);
    if(p0.mass > 0u) iSphere(ray, vec4(p0.pos, 0.85*uSphereScale), uGlow * length(p0.vel));
    if(p1.mass > 0u) iSphere(ray, vec4(p1.pos, 0.85*uSphereScale), uGlow * length(p0.vel));
}
void TraceCells(inout Ray ray, vec3 p){
    vec3 p0 = floor(p);
    vec4 rho = LOAD3D(uDensity, p);
    if(rho.z < 1e-5) return;
    range(i,-1,1) range(j,-1,1) range(k,-1,1){
        TraceCell(ray, p0 + vec3(i,j,k));
    }
}
void main(){
    size3d = vec3(uSize);
    vec2 uv = (vUv*uRes - 0.5*uRes)/max(uRes.x, uRes.y);
    vec2 angles = vec2(uRotSpeed*uTime, uTilt);
    vec3 rd = getRay(angles, uv);
    vec3 crd = getRay(angles, vec2(0.0));
    float d = sqrt(dot(size3d, size3d))*0.5;
    vec3 ro = size3d*0.5 - crd*d;
    vec2 tdBox = iBox(ro - size3d*0.5, rd, 0.5*size3d);
    vec3 col = env(rd.yzx);
    float alpha = uBgAlpha;
    if(tdBox.x < MAX_DIST){
        float td = max(tdBox.x, 0.0);
        float step_size = 2.0;
        const int step_count = 100;
        Ray ray; ray.ro = ro; ray.rd = rd; ray.td = tdBox.y; ray.color = vec3(0.0);
        for(int i = 0; i < step_count; i++){
            vec3 p = ro + rd*td;
            TraceCells(ray, p);
            td += step_size;
            if(td > tdBox.y || ray.td < tdBox.y) break;
        }
        if(ray.td < tdBox.y){
            vec3 p0 = ray.ro + ray.rd*ray.td;
            vec3 normal = normalize(calcNormal(p0, 0.5).xyz);
            normal = -normalize(mix(normal, ray.normal, 0.25));
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
            vec3 refl = env(refl_d.yzx);
            float K = 1. - pow(max(dot(normal, refl_d), 0.), 3.);
            K = mix(0.0, K, 0.75);
            // rim/fresnel tint toward the shallow colour at grazing angles
            float fres = pow(1.0 - max(dot(normal, -ray.rd), 0.0), 3.0);
            albedo = mix(albedo, uShallow, fres * uRim);
            // Split body (diffuse + ambient) from the glassy specular reflection
            // so the image can recolor the body without killing the wet highlight.
            vec3 specCol = shadow*refl*K*uSpec;
            vec3 bodyCol = uBright*shadow*albedo*(LdotN*uLightInt)*(1.0 - K)
                         + uAmbient*ray.color;
            // The liquid's color should COME FROM the image: recolor the lit body
            // to the image hue at full strength while keeping the sim's luminance,
            // so it still reads as a shaded 3D liquid. uImageMix fades this in.
            if(uImageOn == 1){
                float bl = dot(bodyCol, vec3(0.299, 0.587, 0.114));
                float al = dot(albedo,  vec3(0.299, 0.587, 0.114)) + 1e-4;
                vec3 recolored = albedo * (bl / al);   // image chroma at body luminance
                bodyCol = mix(bodyCol, recolored, uImageMix);
            }
            col = bodyCol + specCol;
            // output saturation
            float luma = dot(col, vec3(0.299, 0.587, 0.114));
            col = mix(vec3(luma), col, uSat);
            alpha = 1.0;
        }
    }
    fragColor = vec4(col, alpha);
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
    m_simRes = std::max(8, std::min(80, m_simRes));
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
    m_renderScale = std::min(1.0f, std::max(0.25f, m_renderScale));
    int nw = std::max(2, (int)std::round(m_zoneW * m_renderScale));
    int nh = std::max(2, (int)std::round(m_zoneH * m_renderScale));
    if (nw == m_output.w && nh == m_output.h) return;
    destroyFBO(m_output);
    m_outW = nw; m_outH = nh;
    m_output = createFBO(m_outW, m_outH, GL_RGBA8, GL_UNSIGNED_BYTE, GL_LINEAR);
}

bool FluidSource3D::init(int outW, int outH) {
    // Render at the full zone resolution by default (crisp). m_renderScale
    // (0.25..1.0) trades sharpness for speed and can be changed live.
    m_zoneW = std::max(2, outW);
    m_zoneH = std::max(2, outH);
    m_renderScale = std::min(1.0f, std::max(0.25f, m_renderScale));
    m_outW = std::max(2, (int)std::round(m_zoneW * m_renderScale));
    m_outH = std::max(2, (int)std::round(m_zoneH * m_renderScale));
    m_simRes = std::max(8, std::min(80, m_simRes));
    m_size = m_simRes;

    std::string common = COMMON;
    std::string ver = "#version 330 core\n";
    std::string sA = ver + common + FS_A;
    std::string sB = ver + common + FS_B;
    std::string sC = ver + common + FS_C;
    std::string sD = ver + common + FS_D;
    std::string sR = ver + common + FS_RENDER;

    // Sim passes use layered rendering (VS+GS route each instance to a Z
    // slice via gl_Layer). The render pass is an ordinary 2D fullscreen draw.
    m_progA = compileProgram(VS_LAYER, GS_LAYER, sA.c_str());
    m_progB = compileProgram(VS_LAYER, GS_LAYER, sB.c_str());
    m_progC = compileProgram(VS_LAYER, GS_LAYER, sC.c_str());
    m_progD = compileProgram(VS_LAYER, GS_LAYER, sD.c_str());
    m_progRender = compileProgram(VS_FULL, sR.c_str());
    if (!m_progA || !m_progB || !m_progC || !m_progD || !m_progRender) {
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

void FluidSource3D::renderToOutput() {
    glUseProgram(m_progRender);
    bindVolume(m_progRender, "uParticles", m_volC, 0);
    bindVolume(m_progRender, "uDensity",   m_volB, 1);
    bindVolume(m_progRender, "uShadow",    m_volD, 2);
    glUniform1f(glGetUniformLocation(m_progRender, "uSize"), (float)m_size);
    glUniform2f(glGetUniformLocation(m_progRender, "uRes"), (float)m_outW, (float)m_outH);
    glUniform1f(glGetUniformLocation(m_progRender, "uTime"), m_time);
    glUniform1f(glGetUniformLocation(m_progRender, "uRotSpeed"), m_autoRotate ? m_rotateSpeed : 0.0f);
    glUniform1f(glGetUniformLocation(m_progRender, "uTilt"), m_tilt);
    glUniform3fv(glGetUniformLocation(m_progRender, "uDeep"), 1, m_deepColor);
    glUniform3fv(glGetUniformLocation(m_progRender, "uGlow"), 1, m_glowColor);
    glUniform1f(glGetUniformLocation(m_progRender, "uBright"), m_brightness);
    // Color + lighting options
    glUniform3fv(glGetUniformLocation(m_progRender, "uLightDir"), 1, m_lightDir);
    glUniform1f(glGetUniformLocation(m_progRender, "uLightInt"), m_lightIntensity);
    glUniform1f(glGetUniformLocation(m_progRender, "uAmbient"), m_ambient);
    glUniform1f(glGetUniformLocation(m_progRender, "uSpec"), m_specular);
    glUniform3fv(glGetUniformLocation(m_progRender, "uShallow"), 1, m_shallowColor);
    glUniform1f(glGetUniformLocation(m_progRender, "uRim"), m_rim);
    glUniform1f(glGetUniformLocation(m_progRender, "uSat"), m_saturation);
    glUniform3fv(glGetUniformLocation(m_progRender, "uBgTop"), 1, m_bgTop);
    glUniform3fv(glGetUniformLocation(m_progRender, "uBgBottom"), 1, m_bgBottom);
    glUniform1f(glGetUniformLocation(m_progRender, "uBgAlpha"), m_bgAlpha);
    glUniform1f(glGetUniformLocation(m_progRender, "uSphereScale"), m_sphereScale);
    // Image injection — colors the surface albedo (mapped to world x/y).
    bool imgOn = (m_imageEnabled && m_image.textureId != 0);
    glActiveTexture(GL_TEXTURE0 + 3);
    glBindTexture(GL_TEXTURE_2D, imgOn ? m_image.textureId : 0);
    glUniform1i(glGetUniformLocation(m_progRender, "uImage"), 3);
    glUniform1i(glGetUniformLocation(m_progRender, "uImageOn"), imgOn ? 1 : 0);
    glUniform1f(glGetUniformLocation(m_progRender, "uImageMix"), m_imageMix);
    glUniform1f(glGetUniformLocation(m_progRender, "uFlipY"), m_image.flippedV ? 1.0f : 0.0f);
    glBindFramebuffer(GL_FRAMEBUFFER, m_output.fbo);
    glViewport(0, 0, m_output.w, m_output.h);
    glBindVertexArray(m_quadVAO);
    glDrawArrays(GL_TRIANGLES, 0, 3);
}

void FluidSource3D::update() {
    if (!m_ready) return;
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

    int steps = std::max(1, m_substeps);
    for (int s = 0; s < steps; s++) {
        // A: volC(prev) → volA
        glUseProgram(m_progA);
        bindVolume(m_progA, "uParticles", m_volC, 0);
        runVolumePass(m_progA, m_volA);
        // B: volA → volB
        glUseProgram(m_progB);
        bindVolume(m_progB, "uParticles", m_volA, 0);
        runVolumePass(m_progB, m_volB);
        // C: volA + volB → volC
        glUseProgram(m_progC);
        // Audio-driven motion: calm when quiet, churning when loud. At
        // m_audioIntensity==0 this is exactly 1.0 (original behaviour).
        {
            float hot = 0.25f + (2.5f - 0.25f) * m_audioEnergy;     // calm..intense by loudness
            float energyU = 1.0f + (hot - 1.0f) * m_audioIntensity; // gated by the user amount
            glUniform1f(glGetUniformLocation(m_progC, "uEnergy"), energyU);
        }
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

void FluidSource3D::applyAudioBindings(float level, float bass, float mid,
                                       float high, float beat, float dt,
                                       MIDIManager* midi) {
    // Track overall loudness (attack fast, release slow) for the calm->intense
    // motion drive — independent of explicit per-param bindings.
    float aRate = (level > m_audioEnergy) ? 6.0f : 1.5f;
    m_audioEnergy += (level - m_audioEnergy) * (1.0f - std::exp(-aRate * std::max(dt, 1e-3f)));

    if (m_audioBindings.empty()) return;
    for (auto& [name, b] : m_audioBindings) {
        if (b.signal == AudioSignal::None) continue;
        float raw = 0.0f; bool have = true;
        switch (b.signal) {
            case AudioSignal::Level: raw = level; break;
            case AudioSignal::Bass:  raw = bass;  break;
            case AudioSignal::Mid:   raw = mid;   break;
            case AudioSignal::High:  raw = high;  break;
            case AudioSignal::Beat:  raw = beat;  break;
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
        float* dst = nullptr; float lo = 0.0f, hi = 1.0f;
        if      (name == "rotateSpeed") { dst = &m_rotateSpeed; lo = 0.0f; hi = 2.0f; }
        else if (name == "brightness")  { dst = &m_brightness;  lo = 0.0f; hi = 6.0f; }
        else if (name == "tilt")        { dst = &m_tilt;        lo = -1.57f; hi = 1.57f; }
        else if (name == "imageMix")    { dst = &m_imageMix;    lo = 0.0f; hi = 1.0f; }
        else if (name == "sphereScale") { dst = &m_sphereScale; lo = 0.05f; hi = 1.5f; }
        else if (name == "bgAlpha")     { dst = &m_bgAlpha;     lo = 0.0f; hi = 1.0f; }
        else if (name == "lightIntensity"){ dst = &m_lightIntensity; lo = 0.0f; hi = 3.0f; }
        else if (name == "ambient")     { dst = &m_ambient;     lo = 0.0f; hi = 1.0f; }
        else if (name == "specular")    { dst = &m_specular;    lo = 0.0f; hi = 3.0f; }
        else if (name == "rim")         { dst = &m_rim;         lo = 0.0f; hi = 1.0f; }
        else if (name == "saturation")  { dst = &m_saturation;  lo = 0.0f; hi = 2.0f; }
        else if (name == "audioIntensity"){ dst = &m_audioIntensity; lo = 0.0f; hi = 1.0f; }
        if (dst) { if (mapped < lo) mapped = lo; if (mapped > hi) mapped = hi; *dst = mapped; }
    }
}

FluidSource3D::~FluidSource3D() {
    if (m_quadVBO) glDeleteBuffers(1, &m_quadVBO);
    if (m_quadVAO) glDeleteVertexArrays(1, &m_quadVAO);
    if (m_volFBO)  glDeleteFramebuffers(1, &m_volFBO);
    destroyVolume(m_volA); destroyVolume(m_volB);
    destroyVolume(m_volC); destroyVolume(m_volD);
    destroyFBO(m_output);
    GLuint progs[] = { m_progA, m_progB, m_progC, m_progD, m_progRender };
    for (GLuint p : progs) if (p) glDeleteProgram(p);
}

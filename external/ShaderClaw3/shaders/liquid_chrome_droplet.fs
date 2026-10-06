/*{
  "DESCRIPTION": "Liquid Chrome: six raymarched metaball droplets, shaded as chrome/oil-slick liquid, floating and orbiting a central gravity well in a black void. This is a real 2D physics simulation, not decorative motion — a persistent buffer stores each droplet's actual position + velocity and integrates gravity, mutual soft repulsion and damping every frame. Bass transiently widens the metaball merge radius (a droplet 'merge' event), mid drives orbit speed and the oil hue drift, treble sharpens the chrome specular shimmer, and beat kicks an outward velocity impulse into the droplets' own dynamics.",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "3D",
    "Fluid",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "mergeSoftness",
      "LABEL": "Merge Softness",
      "TYPE": "float",
      "DEFAULT": 0.22,
      "MIN": 0.05,
      "MAX": 0.6
    },
    {
      "NAME": "chromeGloss",
      "LABEL": "Chrome Gloss",
      "TYPE": "float",
      "DEFAULT": 1.15,
      "MIN": 0.2,
      "MAX": 3
    },
    {
      "NAME": "texMix",
      "LABEL": "Env Image Mix",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "inputImage",
      "LABEL": "Input Image",
      "TYPE": "image"
    },
    {
      "NAME": "repelRadius",
      "LABEL": "Repel Radius",
      "TYPE": "float",
      "DEFAULT": 0.34,
      "MIN": 0.05,
      "MAX": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "dropletSize",
      "LABEL": "Droplet Size",
      "TYPE": "float",
      "DEFAULT": 0.17,
      "MIN": 0.06,
      "MAX": 0.4,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "gravityStrength",
      "LABEL": "Gravity Well",
      "TYPE": "float",
      "DEFAULT": 0.55,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "repelStrength",
      "LABEL": "Mutual Repel",
      "TYPE": "float",
      "DEFAULT": 0.65,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "orbitSpin",
      "LABEL": "Orbit Drift",
      "TYPE": "float",
      "DEFAULT": 0.42,
      "MIN": 0,
      "MAX": 1.5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "damping",
      "LABEL": "Damping",
      "TYPE": "float",
      "DEFAULT": 0.965,
      "MIN": 0.85,
      "MAX": 0.999,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "bobAmount",
      "LABEL": "Vertical Bob",
      "TYPE": "float",
      "DEFAULT": 0.22,
      "MIN": 0,
      "MAX": 0.6,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "hueBase",
      "LABEL": "Chrome Hue",
      "TYPE": "float",
      "DEFAULT": 0.56,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "hueDriftSpeed",
      "LABEL": "Hue Drift (Mid)",
      "TYPE": "float",
      "DEFAULT": 0.16,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "colorBoost",
      "LABEL": "Color Boost",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "camSpin",
      "LABEL": "Camera Spin",
      "TYPE": "float",
      "DEFAULT": 0.14,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "bgColor",
      "LABEL": "Void Color",
      "TYPE": "color",
      "DEFAULT": [
        0.006,
        0.008,
        0.015,
        1
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Sound Reactivity",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "mergeBoost",
      "LABEL": "Bass Merge Boost",
      "TYPE": "float",
      "DEFAULT": 0.9,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "scatterKick",
      "LABEL": "Beat Scatter Kick",
      "TYPE": "float",
      "DEFAULT": 0.85,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Audio Reactivity"
    }
  ],
  "PASSES": [
    {
      "TARGET": "simBuf",
      "PERSISTENT": true
    },
    {}
  ]
}*/

// ============================================================================
// LIQUID CHROME DROPLET
//   A spiritual sibling to this library's Fluid Sim / 3D SPH Fluid / Fluid
//   Sound lineage, compressed into a compact ISF raymarch piece: instead of a
//   full field simulation, six point-mass droplets carry REAL position +
//   velocity state in a persistent 1-texel-per-droplet buffer (row i, any
//   column). Every frame: gravity well toward the origin, soft mutual
//   repulsion so they never fully overlap, idle tangential spin so the
//   cluster keeps drifting/orbiting even in total silence, and damping.
//   Audio injects energy into that system rather than posing it directly:
//   bass transiently widens the metaball merge radius used by the final
//   render (a "merge" event), mid speeds the idle orbit + the oil hue drift,
//   treble sharpens/brightens the chrome specular, and beat fires an outward
//   velocity kick straight into the sim's own momentum (law 5: impulse in,
//   physics out). The final pass reconstructs each droplet's 3D position
//   (2D sim plane -> XZ, hashed-phase Y bob) and raymarches a metaball union
//   shaded as chrome / oil-slick — real thin-film interference colour that
//   shifts with view angle, reflecting a procedural chrome void or the input
//   image as an environment map.
// ============================================================================

#define DT 0.02
#define POS_RANGE 1.35
#define VEL_RANGE 1.6
#define TWO_PI 6.2831853
#define PI 3.14159265

// ---------------------------------------------------------------------------
// hashing / audio conditioning (house standard snippet)
// ---------------------------------------------------------------------------
float hash11(float p){
    p = fract(p * 0.1031);
    p *= p + 33.33;
    p *= p + p;
    return fract(p);
}

float knee(float x, float lo, float hi){ return smoothstep(lo, hi, x); }
// log-frequency FFT lookup — musical energy lives in the low bins
float fftLog(float t){ return texture2D(audioFFT, vec2(pow(clamp(t, 0.0, 1.0), 2.2) * 0.5, 0.5)).r; }

// ---------------------------------------------------------------------------
// state encode / decode — pos + vel packed into one RGBA texel per droplet
// ---------------------------------------------------------------------------
vec2 encodePos(vec2 p){ return clamp(p / POS_RANGE, -1.0, 1.0) * 0.5 + 0.5; }
vec2 decodePos(vec2 e){ return (e * 2.0 - 1.0) * POS_RANGE; }
vec2 encodeVel(vec2 v){ return clamp(v / VEL_RANGE, -1.0, 1.0) * 0.5 + 0.5; }
vec2 decodeVel(vec2 e){ return (e * 2.0 - 1.0) * VEL_RANGE; }

vec2 initPos(float idx){
    float ang = idx / 6.0 * TWO_PI + hash11(idx * 3.1 + 1.0) * 0.9;
    float rad = 0.42 + 0.34 * hash11(idx * 7.7 + 2.0);
    return vec2(cos(ang), sin(ang)) * rad;
}

// soft gaussian repulsion; returns zero when self==other (guards the divide)
vec2 repel(vec2 self, vec2 other, float k, float rad){
    vec2 d = self - other;
    float d2 = dot(d, d);
    if (d2 < 1.0e-5) return vec2(0.0);
    float fall = exp(-d2 / (rad * rad));
    return normalize(d) * k * fall;
}

// One droplet's physics update. `idx` is a compile-time-constant-per-call-site
// literal (0.0..5.0) used only for per-droplet hash phase — every droplet
// reads the SAME six fixed texels (no dynamic indexing anywhere).
vec4 stepDroplet(float idx, vec2 pi, vec2 vi,
                  vec2 p0, vec2 p1, vec2 p2, vec2 p3, vec2 p4, vec2 p5,
                  float midBoost, float beatKick){
    vec2 force = -pi * gravityStrength;
    force += repel(pi, p0, repelStrength, repelRadius);
    force += repel(pi, p1, repelStrength, repelRadius);
    force += repel(pi, p2, repelStrength, repelRadius);
    force += repel(pi, p3, repelStrength, repelRadius);
    force += repel(pi, p4, repelStrength, repelRadius);
    force += repel(pi, p5, repelStrength, repelRadius);

    // idle tangential spin — the cluster's own dynamics keep it orbiting
    // even with zero audio (sound-off test); mid briskens the orbit.
    vec2 tang = vec2(-pi.y, pi.x);
    float tlen = length(tang) + 1.0e-4;
    float ownSpin = orbitSpin * (0.55 + 0.55 * hash11(idx * 3.7 + 11.0)) * (1.0 + 0.8 * midBoost);
    force += (tang / tlen) * ownSpin;

    vec2 newVel = (vi + force * DT) * clamp(damping, 0.85, 0.999);

    // beat: a scatter impulse injected straight into velocity (law 5) —
    // decays away afterward through the sim's own damping, not a redraw.
    vec2 outward = normalize(pi + vec2(1.0e-4, 0.0));
    newVel += outward * beatKick * (0.5 + 0.5 * hash11(idx * 9.1 + 2.0));
    newVel += (tang / tlen) * beatKick * 0.4;

    vec2 newPos = pi + newVel * DT;
    newPos = clamp(newPos, vec2(-1.3), vec2(1.3));

    return vec4(encodePos(newPos), encodeVel(newVel));
}

// ---------------------------------------------------------------------------
// metaball scene (six spheres, sequential smooth-min union)
// ---------------------------------------------------------------------------
float sdSphere(vec3 p, vec3 c, float r){ return length(p - c) - r; }

float smin(float a, float b, float k){
    float h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0);
    return mix(b, a, h) - k * h * (1.0 - h);
}

float mapScene(vec3 p,
                vec3 c0, vec3 c1, vec3 c2, vec3 c3, vec3 c4, vec3 c5,
                float r0, float r1, float r2, float r3, float r4, float r5,
                float k){
    float d = sdSphere(p, c0, r0);
    d = smin(d, sdSphere(p, c1, r1), k);
    d = smin(d, sdSphere(p, c2, r2), k);
    d = smin(d, sdSphere(p, c3, r3), k);
    d = smin(d, sdSphere(p, c4, r4), k);
    d = smin(d, sdSphere(p, c5, r5), k);
    return d;
}
// convenience macro — expands against the c0..c5 / r0..r5 / kMerge locals
// that screenPass() defines just before using it.
#define MAPSCENE(p) mapScene(p, c0,c1,c2,c3,c4,c5, r0,r1,r2,r3,r4,r5, kMerge)

vec3 dropletCenter(vec2 p, float idx){
    float freq  = 0.75 + 0.5 * hash11(idx * 3.7 + 2.0);
    float phase = hash11(idx * 5.3 + 9.0) * TWO_PI;
    float y = bobAmount * sin(TIME * freq + phase);
    return vec3(p.x, y, p.y);
}
float dropletRadius(float idx){
    return dropletSize * (0.78 + 0.42 * hash11(idx * 2.1 + 4.0));
}

// real thin-film interference colour — reflectance peaks where
// 2*n*t*cos(theta) = (m+1/2)*lambda, evaluated at RGB wavelengths.
// The classic oil-slick / soap-bubble palette, shared lineage with this
// library's oil_slick_iridescence.fs.
vec3 thinFilm(float t_nm, float nIdx, float cosTheta){
    float opt = 2.0 * nIdx * t_nm * cosTheta;
    vec3 c = vec3(
        0.5 + 0.5 * cos(TWO_PI * opt / 620.0),
        0.5 + 0.5 * cos(TWO_PI * opt / 550.0 + 1.04),
        0.5 + 0.5 * cos(TWO_PI * opt / 470.0 + 2.09)
    );
    c.r = mix(c.r, 0.5 + 0.5 * cos(TWO_PI * opt / 720.0), 0.18);
    c.b = mix(c.b, 0.5 + 0.5 * cos(TWO_PI * opt / 430.0), 0.18);
    return c;
}

// ---------------------------------------------------------------------------
// PASS 0 : physics simulation, one droplet's state per texel (rows 0..5)
// ---------------------------------------------------------------------------
void simPass(float midBoost, float beatKick){
    vec2 texel = 1.0 / RENDERSIZE;
    float row = floor(gl_FragCoord.y);
    if (row > 5.5){ gl_FragColor = vec4(0.0); return; }

    if (FRAMEINDEX < 2){
        gl_FragColor = vec4(encodePos(initPos(row)), encodeVel(vec2(0.0)));
        return;
    }

    // fixed literal texel reads — one droplet per row, no dynamic indexing
    vec4 raw0 = texture2D(simBuf, vec2(0.5, 0.5) * texel);
    vec4 raw1 = texture2D(simBuf, vec2(0.5, 1.5) * texel);
    vec4 raw2 = texture2D(simBuf, vec2(0.5, 2.5) * texel);
    vec4 raw3 = texture2D(simBuf, vec2(0.5, 3.5) * texel);
    vec4 raw4 = texture2D(simBuf, vec2(0.5, 4.5) * texel);
    vec4 raw5 = texture2D(simBuf, vec2(0.5, 5.5) * texel);

    vec2 p0 = decodePos(raw0.xy); vec2 v0 = decodeVel(raw0.zw);
    vec2 p1 = decodePos(raw1.xy); vec2 v1 = decodeVel(raw1.zw);
    vec2 p2 = decodePos(raw2.xy); vec2 v2 = decodeVel(raw2.zw);
    vec2 p3 = decodePos(raw3.xy); vec2 v3 = decodeVel(raw3.zw);
    vec2 p4 = decodePos(raw4.xy); vec2 v4 = decodeVel(raw4.zw);
    vec2 p5 = decodePos(raw5.xy); vec2 v5 = decodeVel(raw5.zw);

    vec4 outState;
    if (row < 0.5)      outState = stepDroplet(0.0, p0, v0, p0,p1,p2,p3,p4,p5, midBoost, beatKick);
    else if (row < 1.5) outState = stepDroplet(1.0, p1, v1, p0,p1,p2,p3,p4,p5, midBoost, beatKick);
    else if (row < 2.5) outState = stepDroplet(2.0, p2, v2, p0,p1,p2,p3,p4,p5, midBoost, beatKick);
    else if (row < 3.5) outState = stepDroplet(3.0, p3, v3, p0,p1,p2,p3,p4,p5, midBoost, beatKick);
    else if (row < 4.5) outState = stepDroplet(4.0, p4, v4, p0,p1,p2,p3,p4,p5, midBoost, beatKick);
    else                outState = stepDroplet(5.0, p5, v5, p0,p1,p2,p3,p4,p5, midBoost, beatKick);

    gl_FragColor = outState;
}

// ---------------------------------------------------------------------------
// procedural chrome void (background + reflection environment)
// ---------------------------------------------------------------------------
vec3 voidColor(vec3 rd){
    float up = clamp(rd.y * 0.5 + 0.5, 0.0, 1.0);
    return mix(bgColor.rgb * 0.55, bgColor.rgb * 1.6, up);
}

vec2 envUV(vec3 r){
    float u = atan(r.z, r.x) / TWO_PI + 0.5;
    float v = acos(clamp(r.y, -1.0, 1.0)) / PI;
    return vec2(u, v);
}

vec3 envProc(vec3 r){
    vec3 c = voidColor(r);
    vec3 l1 = normalize(vec3(0.4, 0.85, 0.35));
    float s1 = pow(max(dot(r, l1), 0.0), 40.0);
    c += vec3(1.0, 0.95, 0.85) * s1 * 1.5;
    vec3 l2 = normalize(vec3(-0.6, 0.25, -0.55));
    float s2 = pow(max(dot(r, l2), 0.0), 90.0);
    c += vec3(0.55, 0.7, 1.0) * s2 * 1.1;
    return c;
}

// ---------------------------------------------------------------------------
// PASS 1 (screen) : raymarch the metaball union as chrome / oil-slick liquid
// ---------------------------------------------------------------------------
void screenPass(float bassP, float midP, float highP, float drive){
    vec2 texel = 1.0 / RENDERSIZE;

    vec4 raw0 = texture2D(simBuf, vec2(0.5, 0.5) * texel);
    vec4 raw1 = texture2D(simBuf, vec2(0.5, 1.5) * texel);
    vec4 raw2 = texture2D(simBuf, vec2(0.5, 2.5) * texel);
    vec4 raw3 = texture2D(simBuf, vec2(0.5, 3.5) * texel);
    vec4 raw4 = texture2D(simBuf, vec2(0.5, 4.5) * texel);
    vec4 raw5 = texture2D(simBuf, vec2(0.5, 5.5) * texel);

    vec2 p0 = decodePos(raw0.xy);
    vec2 p1 = decodePos(raw1.xy);
    vec2 p2 = decodePos(raw2.xy);
    vec2 p3 = decodePos(raw3.xy);
    vec2 p4 = decodePos(raw4.xy);
    vec2 p5 = decodePos(raw5.xy);

    // reconstruct 3D positions: sim plane -> XZ, hashed-phase Y bob
    vec3 c0 = dropletCenter(p0, 0.0);
    vec3 c1 = dropletCenter(p1, 1.0);
    vec3 c2 = dropletCenter(p2, 2.0);
    vec3 c3 = dropletCenter(p3, 3.0);
    vec3 c4 = dropletCenter(p4, 4.0);
    vec3 c5 = dropletCenter(p5, 5.0);

    // bass swells the droplets themselves a touch on top of the merge —
    // both read instantly (no simulation lag) so a hit is felt at once.
    float bassSwell = 1.0 + 0.30 * bassP;
    float r0 = dropletRadius(0.0) * bassSwell;
    float r1 = dropletRadius(1.0) * bassSwell;
    float r2 = dropletRadius(2.0) * bassSwell;
    float r3 = dropletRadius(3.0) * bassSwell;
    float r4 = dropletRadius(4.0) * bassSwell;
    float r5 = dropletRadius(5.0) * bassSwell;

    // bass transiently widens the merge radius -> droplets visually fuse
    // into one blob on a hit, then relax back as bassP decays.
    float kMerge = clamp(mergeSoftness * (1.0 + mergeBoost * 1.8 * bassP), 0.02, 1.6);

    // --- camera: slow orbit around the cluster --------------------------
    float ang = TIME * camSpin * 0.35;
    float orbitR = 2.6;
    float camY = 1.05 + 0.35 * sin(TIME * 0.17);
    vec3 ro = vec3(sin(ang) * orbitR, camY, cos(ang) * orbitR);
    vec3 ta = vec3(0.0, 0.0, 0.0);
    vec3 fwd = normalize(ta - ro);
    vec3 rgt = normalize(cross(vec3(0.0, 1.0, 0.0), fwd));
    vec3 upv = cross(fwd, rgt);
    vec2 ndc = (gl_FragCoord.xy - 0.5 * RENDERSIZE) / RENDERSIZE.y;
    float fov = 1.3;
    vec3 rd = normalize(fwd + (ndc.x * rgt + ndc.y * upv) * fov);

    // --- raymarch ---------------------------------------------------------
    float t = 0.05;
    bool hit = false;
    vec3 hitP = vec3(0.0);
    float minD = 1.0e5;
    for (int i = 0; i < 70; i++){
        vec3 pos = ro + rd * t;
        float d = MAPSCENE(pos);
        minD = min(minD, d);
        if (d < 0.001){ hit = true; hitP = pos; break; }
        t += d * 0.9;
        if (t > 9.0) break;
    }

    vec3 col;
    if (hit){
        vec2 e = vec2(0.0015, 0.0);
        vec3 n = normalize(vec3(
            MAPSCENE(hitP + e.xyy) - MAPSCENE(hitP - e.xyy),
            MAPSCENE(hitP + e.yxy) - MAPSCENE(hitP - e.yxy),
            MAPSCENE(hitP + e.yyx) - MAPSCENE(hitP - e.yyx)
        ));
        vec3 viewDir = normalize(ro - hitP);
        float fres = pow(1.0 - clamp(dot(n, viewDir), 0.0, 1.0), 5.0);
        vec3 refl = reflect(-viewDir, n);

        vec3 envCol = envProc(refl);
        if (texMix > 0.001){
            vec3 texCol = texture2D(inputImage, clamp(envUV(refl), 0.0, 1.0)).rgb;
            envCol = mix(envCol, texCol, clamp(texMix, 0.0, 1.0));
        }

        // oil-slick chrome tint: hue rides on view angle (fresnel), slowly
        // drifts with mid energy, and gets an instant bass-driven push so a
        // hit visibly shifts the film colour the moment it lands.
        float huePhase = hueBase + TIME * hueDriftSpeed * (0.25 + 0.9 * midP) + 0.14 * bassP;
        float t_nm = mix(320.0, 760.0, fres) + sin(huePhase * TWO_PI) * 230.0 + 90.0 * bassP;
        vec3 film = thinFilm(t_nm, 1.35, clamp(dot(n, viewDir), 0.25, 1.0));

        col = envCol * mix(vec3(1.0), film * 1.5, 0.62);
        col += film * fres * (0.4 + 0.5 * bassP);

        // treble sharpens + brightens the chrome specular (shimmer)
        vec3 keyDir = normalize(vec3(0.35, 0.8, 0.42));
        vec3 halfv = normalize(keyDir + viewDir);
        float specPow = mix(50.0, 210.0, clamp(highP, 0.0, 1.0));
        float spec = pow(max(dot(n, halfv), 0.0), specPow);
        float specGain = chromeGloss * (1.0 + 1.6 * highP);
        col += vec3(1.0) * spec * specGain;

        // fine high-frequency glints across the surface (log-FFT lookup)
        float sparkH = fftLog(fract(hitP.x * 3.1 + hitP.z * 5.7 + 0.15));
        col += sparkH * highP * 0.35 * vec3(0.9, 0.95, 1.0);
    } else {
        // near-miss glow: a soft chrome halo bleeding into the void
        col = voidColor(rd) + exp(-max(minD, 0.0) * 7.0) * vec3(0.5, 0.6, 0.95) * 0.22;
    }

    // idle floor so silence never reads as a dead frame
    col *= 0.82 + 0.18 * drive;

    // tonemap + gamma
    col = col / (1.0 + col);
    col = pow(max(col, 0.0), vec3(1.0 / 2.2));
    // ---- universal color block (defaults = no-op) ----
    float ucL = dot(col, vec3(0.299, 0.587, 0.114));
    col = mix(vec3(ucL), col, colorBoost);
    gl_FragColor = vec4(col, 1.0);
}

// ---------------------------------------------------------------------------
void main(){
    float bassRaw = clamp(audioBass * audioReact, 0.0, 2.0);
    float midRaw  = clamp(audioMid  * audioReact, 0.0, 2.0);
    float highRaw = clamp(audioHigh * audioReact, 0.0, 2.0);

    float bassP = pow(knee(bassRaw, 0.05, 0.85), 1.6);
    float midP  = pow(knee(midRaw,  0.08, 0.85), 1.3);
    float highP = pow(knee(highRaw, 0.10, 0.90), 1.2);
    float drive = 0.25 + 0.75 * knee(audioEnergy, 0.05, 0.9);

    // beat: an outward velocity impulse fed straight into the sim (law 5)
    float beatKick = scatterKick * audioReact * audioBeatPulse * audioBeatPulse;

    if (PASSINDEX == 0){
        simPass(midP, beatKick);
    } else {
        screenPass(bassP, midP, highP, drive);
    }
}

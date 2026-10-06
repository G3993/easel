/*{
  "DESCRIPTION": "3D Liquid — a self-contained, single-pass raymarched violet metaball fluid in the spirit of Wyatt's 3D SPH shader (shadertoy.com/view/mstfzS). An orbiting liquid blob raymarched with diffuse + environment reflection + fresnel rim. Drop in YOUR OWN image as the background: 'Wrap Environment' wraps it around the scene so the liquid genuinely reflects/refracts your picture, or 'Backdrop' places it flat behind the blob. Knobs: BLOBS, VISCOSITY (merge smoothness), SPEED, SIZE, REFLECT, AUTO SPIN, two liquid colors, BACKGROUND MIX/MODE/SPIN, RIM GLOW, and live SOUND REACTIVITY (bass swells the droplets, mid makes them merge gooier).",
  "CREDIT": "Easel · liquid_3d  (after Wyatt 'SPH 3D', shadertoy.com/view/mstfzS)",
  "CATEGORIES": [
    "Generator",
    "3D",
    "Fluid"
  ],
  "INPUTS": [
    {
      "NAME": "reflAmt",
      "LABEL": "Reflect",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.6
    },
    {
      "NAME": "inputImage",
      "LABEL": "Your Background",
      "TYPE": "image"
    },
    {
      "NAME": "rimGlow",
      "LABEL": "Rim Glow",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.4
    },
    {
      "NAME": "blobs",
      "LABEL": "Blobs",
      "TYPE": "float",
      "MIN": 3,
      "MAX": 16,
      "DEFAULT": 11,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "blobSize",
      "LABEL": "Size",
      "TYPE": "float",
      "MIN": 0.4,
      "MAX": 1.6,
      "DEFAULT": 0.9,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "viscosity",
      "LABEL": "Viscosity",
      "TYPE": "float",
      "MIN": 0.2,
      "MAX": 2.5,
      "DEFAULT": 1.2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "speed",
      "LABEL": "Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 0.7,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "spin",
      "LABEL": "Auto Spin",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.5,
      "DEFAULT": 0.25,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "colA",
      "LABEL": "Deep",
      "TYPE": "color",
      "DEFAULT": [
        0.22,
        0.349,
        1,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "colB",
      "LABEL": "Glow",
      "TYPE": "color",
      "DEFAULT": [
        0.42,
        0.302,
        0.996,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "hueShift",
      "LABEL": "Hue Shift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
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
      "NAME": "bgMix",
      "LABEL": "Background Mix",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Background"
    },
    {
      "NAME": "bgMode",
      "LABEL": "Background Mode",
      "TYPE": "long",
      "VALUES": [
        0,
        1
      ],
      "LABELS": [
        "Backdrop (flat)",
        "Wrap Environment"
      ],
      "DEFAULT": 1,
      "GROUP": "Background"
    },
    {
      "NAME": "bgSpin",
      "LABEL": "Background Spin",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.05,
      "GROUP": "Background"
    },
    {
      "NAME": "bgColor",
      "LABEL": "Background",
      "TYPE": "color",
      "DEFAULT": [
        0,
        0,
        0,
        0
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Sound Reactivity",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

#define FOV 2.5
#define PI  3.14159265359
#define TAU 6.28318530718

// Audio pulse globals — set in main() from the live audio bus, read inside map().
float gAudioSize = 1.0;   // bass swells droplet radius
float gAudioVisc = 1.0;   // mid loosens the merge (gooier)

// ── Camera rig — verbatim from the original (theta/phi rotation) ─────
mat3 getCamera(vec2 a){
    mat3 t = mat3(1.0, 0.0, 0.0,
                  0.0, cos(a.y), -sin(a.y),
                  0.0, sin(a.y),  cos(a.y));
    mat3 p = mat3(cos(a.x), sin(a.x), 0.0,
                 -sin(a.x), cos(a.x), 0.0,
                  0.0, 0.0, 1.0);
    return t * p;
}
// GLSL ES 1.0 has no transpose() — build it manually.
mat3 transpose3(mat3 m){
    return mat3(m[0][0], m[1][0], m[2][0],
                m[0][1], m[1][1], m[2][1],
                m[0][2], m[1][2], m[2][2]);
}
vec3 getRay(vec2 a, vec2 pos){
    mat3 cam = getCamera(a);
    return normalize(transpose3(cam) * vec3(FOV * pos.x, 1.0, FOV * pos.y));
}

float hash(float n){ return fract(sin(n) * 43758.5453); }

// Smooth-union of distance fields → the merging-droplet "liquid" look that
// stands in for SPH particle clustering.
float smin(float a, float b, float k){
    float h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0);
    return mix(b, a, h) - k * h * (1.0 - h);
}

// Metaball fluid field. Up to 16 animated droplets orbiting/bobbing inside
// a ~unit sphere, smooth-unioned by VISCOSITY (with a live audio nudge).
float map(vec3 p){
    float t = TIME * speed;
    float d = 1e9;
    int   n = int(blobs);
    float k = 0.35 * viscosity * gAudioVisc;
    for (int i = 0; i < 16; i++){
        if (i >= n) break;
        float fi = float(i);
        vec3 c = vec3(
            sin(t * (0.50 + 0.30 * hash(fi))       + fi * 1.7),
            sin(t * (0.40 + 0.35 * hash(fi + 9.0))  + fi * 2.3),
            cos(t * (0.45 + 0.25 * hash(fi + 3.0))  + fi * 1.1)
        ) * (1.10 + 0.30 * hash(fi + 5.0));
        float r = blobSize * gAudioSize * (0.55 + 0.40 * hash(fi + 7.0));
        d = smin(d, length(p - c) - r, k);
    }
    return d;
}

vec3 calcNormal(vec3 p){
    vec2 e = vec2(0.012, 0.0);
    return normalize(vec3(
        map(p + e.xyy) - map(p - e.xyy),
        map(p + e.yxy) - map(p - e.yxy),
        map(p + e.yyx) - map(p - e.yyx)));
}

// Procedural environment — the original violet studio sky + key-light hotspot.
vec3 proceduralSky(vec3 d){
    float up  = d.y * 0.5 + 0.5;
    vec3  sky = mix(vec3(0.03, 0.04, 0.09), vec3(0.28, 0.42, 0.85), up);
    float s   = pow(max(dot(d, normalize(vec3(0.5, 0.8, 0.3))), 0.0), 18.0);
    sky += vec3(1.0, 0.95, 0.85) * s * 1.6;       // key-light hotspot
    return sky;
}

// Direction → equirectangular UV (lets a flat image wrap the whole scene).
vec2 dirToEquirect(vec3 d){
    return vec2(atan(d.z, d.x) / TAU + 0.5,
                acos(clamp(d.y, -1.0, 1.0)) / PI);
}

// The environment the liquid lives in AND reflects. When a background image is
// mixed in (bgMix>0) it's sampled equirectangularly so reflections pick it up.
vec3 envSample(vec3 d){
    vec3 sky = proceduralSky(d);
    if (bgMix > 0.0){
        vec2 euv = dirToEquirect(d);
        euv.x += TIME * bgSpin * 0.08;             // slow drift so reflections shimmer
        sky = mix(sky, texture2D(inputImage, fract(euv)).rgb, bgMix);
    }
    return sky;
}

// Flat backdrop: the image sized to "cover" the frame, behind the blob.
vec3 backdrop(vec3 rd, vec2 fragUV, vec2 res){
    vec3 base = proceduralSky(rd);
    if (bgMix > 0.0){
        vec2 isz = max(IMG_SIZE_inputImage, vec2(1.0));
        float sa = res.x / res.y;
        float ia = isz.x / isz.y;
        vec2  sc = (sa > ia) ? vec2(1.0, ia / sa) : vec2(sa / ia, 1.0);
        vec2  iuv = (fragUV - 0.5) / sc + 0.5;
        base = mix(base, texture2D(inputImage, iuv).rgb, bgMix);
    }
    return base;
}

void main(){
    vec2 R  = RENDERSIZE;
    vec2 uv = (gl_FragCoord.xy - 0.5 * R) / max(R.x, R.y);

    // Live audio bus → droplet swell + merge (calm: bass K=0.30, mid K=0.40).
    float bAud = audioBass * audioReact;
    float mAud = audioMid  * audioReact;
    gAudioSize = 1.0 + 0.65 * bAud;
    gAudioVisc = 1.0 + 0.85 * mAud;

    // Auto-orbit camera (the original's no-mouse fallback path).
    vec2 angles = vec2(TIME * spin * 0.5, -0.5);
    vec3 rd  = getRay(angles, uv);
    vec3 crd = getRay(angles, vec2(0.0));
    vec3 ro  = -crd * 4.5;

    // Background — flat backdrop or wrapped environment.
    vec3 col = (bgMode < 0.5)
        ? backdrop(rd, gl_FragCoord.xy / R, R)
        : envSample(rd);

    // Sphere-march the metaball field.
    float t = 0.0; bool hit = false; vec3 p = ro;
    for (int i = 0; i < 90; i++){
        p = ro + rd * t;
        float d = map(p);
        if (d < 0.002){ hit = true; break; }
        t += d;
        if (t > 12.0) break;
    }

    if (hit){
        vec3  nrm  = calcNormal(p);
        vec3  L    = normalize(vec3(0.5, 0.8, 0.3));
        float diff = clamp(dot(nrm, L) * 0.5 + 0.5, 0.0, 1.0);   // half-Lambert
        vec3  refl = reflect(rd, nrm);
        vec3  envR = envSample(refl);                            // reflects your image
        float fres = pow(1.0 - max(dot(nrm, -rd), 0.0), 3.0);
        float grad = clamp(length(p) * 0.32, 0.0, 1.0);
        vec3  albedo = mix(colA.rgb, colB.rgb, grad);
        col  = albedo * (0.25 + 1.60 * diff);
        col  = mix(col, envR, reflAmt * (0.25 + 0.75 * fres));
        col += colB.rgb * fres * rimGlow;                        // fresnel rim glow
    }

    // Audio punch: guarantees a visible reactive lift even when the camera
    // framing keeps the metaball field small on screen this frame.
    col *= 1.0 + 0.45 * bAud + 0.30 * mAud;

    // Tonemap + gamma.
    col = col / (1.0 + col);
    col = pow(max(col, 0.0), vec3(1.0 / 2.2));
    // ---- universal color block (defaults = no-op) ----
    float ucL = dot(col, vec3(0.299, 0.587, 0.114));
    col = mix(vec3(ucL), col, colorBoost);
    if (hueShift > 0.0005) {
        float hA = hueShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        col = clamp(hM * col, 0.0, 1.0);
    }
    col = mix(col, bgColor.rgb, bgColor.a * (1.0 - smoothstep(0.0, 0.35, ucL)));
    gl_FragColor = vec4(col, 1.0);
}

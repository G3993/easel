/*{
  "DESCRIPTION": "Dancing Cubes — a raymarched grid of rounded cubes whose heights rise and fall from the brightness of the bound image (or its motion), lit with orange specular, ambient occlusion and a procedural sky reflection. A built-in 'Dance' wobble keeps them moving even on a still image. Ported from a Shadertoy: iChannel1 (height + colour source) -> input image, iChannel0 cubemap -> procedural environment.",
  "CREDIT": "Shadertoy 'Dancing Cubes' (AO by Shane) — ISF port for Easel",
  "CATEGORIES": [
    "3D",
    "Effect",
    "Geometric"
  ],
  "INPUTS": [
    {
      "NAME": "inputImage",
      "LABEL": "Texture",
      "TYPE": "image"
    },
    {
      "NAME": "scale",
      "LABEL": "Scale",
      "TYPE": "float",
      "MIN": 20,
      "MAX": 100,
      "DEFAULT": 60,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "height",
      "LABEL": "Height",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 16,
      "DEFAULT": 8,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "dance",
      "LABEL": "Dance",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "danceSpeed",
      "LABEL": "Dance Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 4,
      "DEFAULT": 1.5,
      "GROUP": "Motion / Animation"
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
      "LABEL": "Audio React",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.35,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//  DANCING CUBES — Easel ISF port of a Shadertoy raymarch.
//    iChannel1 (grey -> tile height, rgb -> tile colour) -> inputImage
//    iChannel0 (cubemap reflection)                      -> procedural env()
//    iMouse height control                               -> "scale" input
//    Added: a TIME-driven "Dance" wobble so cubes move on a still image.
// ════════════════════════════════════════════════════════════════════════

#define FAR 100.0
#define ASP (RENDERSIZE.x / RENDERSIZE.y)
#define ACCURACY 1.0

// Audio state, set once per-fragment in main() before the raymarch — tileH()
// and lighting() (called many times from trace/normal/AO) read these globals
// rather than recomputing knees every call. Non-gating: 0.0 at rest.
float gBassBoost = 0.0;   // bass -> skyline height (dominant structure)
float gBeatBoost = 0.0;   // beat -> a decaying spike on a sparse subset of cubes
float gHighBoost = 0.0;   // highs -> sparkle on a sparse subset of cube tops

float rnd(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}

float dieAbgerundeteBox(vec3 p, vec3 b, float r) {
    return length(max(abs(p) - b, 0.0)) - r;
}

mat2 rot(float a) { float c = cos(a), s = sin(a); return mat2(c, -s, s, c); }

float getGrey(vec3 p) { return p.x * 0.299 + p.y * 0.587 + p.z * 0.114; }

// Procedural fallback so heights aren't flat when no image is bound.
vec3 srcTex(vec2 uv) {
    if (IMG_SIZE_inputImage.x > 0.0) return texture(inputImage, fract(uv)).rgb;
    float a = 0.5 + 0.5 * sin(uv.x * 9.0);
    float b = 0.5 + 0.5 * sin(uv.y * 9.0 + 2.0);
    return vec3(a, b, 0.5 + 0.5 * sin((uv.x + uv.y) * 6.0));
}

// Procedural environment (replaces the Shadertoy cubemap on iChannel0).
vec3 env(vec3 d) {
    d = normalize(d);
    float t = 0.5 + 0.5 * d.y;
    vec3 sky = mix(vec3(0.10, 0.09, 0.16), vec3(0.45, 0.62, 0.95), t);
    float sun = pow(max(dot(d, normalize(vec3(0.5, 0.65, 0.4))), 0.0), 48.0);
    sky += vec3(1.0, 0.82, 0.55) * sun * 1.5;
    return sky;
}

// Per-tile height from the image brightness + a dancing time wobble.
float tileH(vec2 cell) {
    float g = getGrey(srcTex(cell * vec2(1.0, ASP) / scale));
    float wob = dance * 0.5 * (0.5 + 0.5 * sin(TIME * danceSpeed + (cell.x * 0.7 + cell.y * 1.3)));
    float cellRnd = rnd(cell);
    // Bass breathes the whole skyline (each cube weighted a little differently
    // so it doesn't pump in lockstep — law 3: give each element its own phase).
    float bassMod = 1.0 + gBassBoost * (0.55 + 0.45 * cellRnd);
    // Beat: a brief height spike on a sparse subset of cubes, decays with the pulse.
    float beatSpike = gBeatBoost * step(0.55, cellRnd);
    return (g * height + wob) * bassMod + beatSpike;
}

float tile(vec3 p) {
    // WebGL1: loop indices must init/compare against constant expressions,
    // so the -ACCURACY..ACCURACY loops use literal bounds (ACCURACY == 1.0).
    vec2 id = floor(p.xz);
    p.xz = fract(p.xz) - 0.5;
    float d = 100.0;
    for (float i = -1.0; i <= 1.0; i += 1.0) {
        for (float j = -1.0; j <= 1.0; j += 1.0) {
            vec2 n = vec2(i, j);
            float h = tileH(floor(id + n));
            float minD = dieAbgerundeteBox(p - vec3(n.x, -3.0 + h, n.y), vec3(0.45, 0.08, 0.45), 0.019);
            minD = min(minD, dieAbgerundeteBox(p - vec3(n.x, -3.0, n.y), vec3(0.15, h, 0.15), 0.019));
            d = min(d, minD);
        }
    }
    return d;
}

vec2 map(vec3 p) { return vec2(tile(p), 0.0); }

vec2 trace(vec3 ro, vec3 rd) {
    vec2 t = vec2(0.0), dist;
    for (int i = 0; i < 48; i++) {
        dist = map(ro + rd * t.x);
        if (dist.x < 0.001 || t.x > FAR) break;
        t.x += dist.x * 0.5;
        t.y = dist.y;
    }
    return t;
}

vec3 normal(vec3 p) {
    mat3 k = mat3(p, p, p) - mat3(0.001);
    return normalize(map(p).x - vec3(map(k[0]).x, map(k[1]).x, map(k[2]).x));
}

float calculateAO(in vec3 pos, in vec3 nor) {
    float sca = 2.0, occ = 0.0;
    for (int i = 0; i < 5; i++) {
        float hr = 0.01 + float(i) * 0.5 / 4.0;
        float dd = map(nor * hr + pos).x;
        occ += (hr - dd) * sca;
        sca *= 0.7;
    }
    return clamp(1.0 - occ, 0.0, 1.0);
}

vec3 lighting(vec3 sp, vec3 sn, vec3 lp, vec3 rd) {
    vec3 lv = lp - sp;
    float ldist = max(length(lv), 0.001);
    vec3 ldir = lv / ldist;
    float atte = 2.0 / (1.0 + 0.002 * ldist * ldist);
    float diff = dot(ldir, sn);
    float spec = pow(max(dot(reflect(-ldir, sn), -rd), 0.0), 10.0);
    float ao = calculateAO(sp, sn);
    vec3 refl = reflect(rd, sn);
    vec3 color2 = srcTex(floor(sp.xz) * vec2(1.0, ASP) / scale);
    vec3 reflColor = env(refl);
    vec3 hotSpec = vec3(0.9, 0.5, 0.2);
    vec3 color = (diff * color2 + spec * hotSpec + reflColor * 0.05) * atte;
    // Highs: fine sparkle on a sparse subset of cube tops (fine detail, not global).
    float sparkleCell = rnd(floor(sp.xz) * 3.7 + 11.0);
    color += vec3(gHighBoost) * step(0.82, sparkleCell) * clamp(sn.y, 0.0, 1.0);
    return clamp(color * ao, 0.0, 1.0);
}

void main() {
    vec2 fragCoord = gl_FragCoord.xy;
    vec2 uv = (fragCoord - RENDERSIZE.xy * 0.5) / RENDERSIZE.y;

    // Non-gating audio: alive at audio=0; audioReact only adds on top.
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.6);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float beatP = audioBeatPulse * audioBeatPulse;
    gBassBoost = audioReact * 0.55 * bassP;   // skyline breathes up to ~+19% at DEFAULT
    gBeatBoost = audioReact * 2.2  * beatP;   // sparse height spikes on the beat
    gHighBoost = audioReact * 0.6  * highP;   // sparkle glints on cube tops

    float gridW = scale;
    vec3 lk = vec3(gridW * 0.5, 0.0, gridW * 0.33);
    vec3 ro = lk + vec3(0.0, scale * 0.3, -0.25);
    vec3 lp = ro + vec3(0.0, 3.75, 10.0);

    float FOV = 1.57;
    vec3 fwd = normalize(lk - ro);
    vec3 rgt = normalize(vec3(fwd.z, 0.0, -fwd.x));
    vec3 up = cross(fwd, rgt);
    vec3 rd = normalize(fwd + FOV * uv.x * rgt + FOV * uv.y * up);

    vec2 t = trace(ro, rd);
    vec3 sp = ro + rd * t.x;
    vec3 sn = normal(sp);

    // universal background: raymarch miss (sky) blends toward bgColor
    vec3 color = (t.x > FAR) ? mix(env(rd), bgColor.rgb, bgColor.a)
                             : lighting(sp, sn, lp, rd);

    float vig = 1.0 - smoothstep(1.0, 3.5, length(uv));
    color *= mix(0.8, 1.0, vig);
    color = pow(color, vec3(0.75));

    // ---- universal color block (defaults = no-op) ----
    vec3 uc = color;
    float ucL = dot(uc, vec3(0.299, 0.587, 0.114));
    uc = mix(vec3(ucL), uc, colorBoost);                   // saturation
    if (hueShift > 0.0005) {                               // cheap hue rotate (YIQ)
        float hA = hueShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hM * uc, 0.0, 1.0);
    }

    gl_FragColor = vec4(uc, 1.0);
}

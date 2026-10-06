/*{
  "DESCRIPTION": "RIPPLE — a 3D soundwave. The music's amplitude history is written into a persistent ring buffer and travels outward from the centre as concentric crests over a raymarched heightfield: every beat becomes a ring you can watch cross the plane and die at the horizon. Bass sets crest height, mids wobble the rings, highs sparkle the facets. Neon contour lines, deep-black fog, low orbiting camera. Beautiful in silence (gentle idle rings), alive with sound.",
  "CREDIT": "Easel / ShaderClaw house shader, 2026-09-11",
  "CATEGORIES": ["Generator", "3D", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "amp",         "LABEL": "Wave Height",   "TYPE": "float", "MIN": 0.0,  "MAX": 2.0, "DEFAULT": 1.0,  "GROUP": "Wave" },
    { "NAME": "waveSpeed",   "LABEL": "Travel Speed",  "TYPE": "float", "MIN": 0.1,  "MAX": 3.0, "DEFAULT": 1.0,  "GROUP": "Wave" },
    { "NAME": "ringDensity", "LABEL": "Ring Density",  "TYPE": "float", "MIN": 0.3,  "MAX": 4.0, "DEFAULT": 2.2,  "GROUP": "Wave" },
    { "NAME": "decay",       "LABEL": "Fade Out",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.0, "DEFAULT": 0.45, "GROUP": "Wave" },
    { "NAME": "idle",        "LABEL": "Idle Ripple",   "TYPE": "float", "MIN": 0.0,  "MAX": 1.0, "DEFAULT": 0.35, "GROUP": "Wave" },
    { "NAME": "wobble",      "LABEL": "Ring Wobble",   "TYPE": "float", "MIN": 0.0,  "MAX": 1.0, "DEFAULT": 0.3,  "GROUP": "Motion" },
    { "NAME": "camOrbit",    "LABEL": "Orbit",         "TYPE": "float", "MIN": -1.0, "MAX": 1.0, "DEFAULT": 0.15, "GROUP": "Motion" },
    { "NAME": "camHeight",   "LABEL": "Camera Height", "TYPE": "float", "MIN": 0.4,  "MAX": 4.0, "DEFAULT": 1.5, "GROUP": "Motion" },
    { "NAME": "camDist",     "LABEL": "Camera Dist",   "TYPE": "float", "MIN": 1.5,  "MAX": 8.0, "DEFAULT": 4.2,  "GROUP": "Motion" },
    { "NAME": "camBob",      "LABEL": "Camera Bob",    "TYPE": "float", "MIN": 0.0,  "MAX": 1.0, "DEFAULT": 0.2,  "GROUP": "Motion" },
    { "NAME": "lineAmt",     "LABEL": "Contour Lines", "TYPE": "float", "MIN": 0.0,  "MAX": 1.0, "DEFAULT": 0.55, "GROUP": "Look" },
    { "NAME": "glow",        "LABEL": "Crest Glow",    "TYPE": "float", "MIN": 0.0,  "MAX": 2.0, "DEFAULT": 1.0,  "GROUP": "Look" },
    { "NAME": "paletteShift","LABEL": "Palette",       "TYPE": "float", "MIN": 0.0,  "MAX": 1.0, "DEFAULT": 0.0,  "GROUP": "Look" },
    { "NAME": "brightness",  "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.2,  "MAX": 3.0, "DEFAULT": 1.0,  "GROUP": "Look" },
    { "NAME": "audioReact",  "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0,  "MAX": 1.0, "DEFAULT": 0.7,  "GROUP": "Audio Reactivity" }
  ],
  "PASSES": [
    { "TARGET": "hist", "PERSISTENT": true },
    {}
  ]
}*/

#define PI  3.14159265
#define TAU 6.28318531

// ── house conditioning ──────────────────────────────────────────────────
float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }
float hash11(float p) { p = fract(p * 0.1031); p *= p + 33.33; return fract(p * (p + p)); }

// world radius of the plane the buffer spans (u = r / R)
const float R = 4.0;
// seconds for a ring to cross the plane at speed 1 — the buffer is a
// time-indexed ring: texel = fract(TIME / PERIOD). No scrolling, no
// resampling, so nothing ever diffuses or grows: each texel is written
// once when its moment comes and read back by age.
const float PERIOD = 7.0;
const float SCROLL = 1.0 / PERIOD;

// ── pass 0: the ring buffer. x = 0 is the centre (emission), x = 1 the rim.
//    rgba = level · bass · mid · high, travelling outward and fading.
vec4 emitNow() {
    float lvl   = knee(audioLevel, 0.04, 0.90);
    float bassP = pow(knee(audioBass, 0.05, 0.85), 1.6);
    float midP  = knee(audioMid, 0.05, 0.85);
    float highP = pow(knee(audioHigh, 0.10, 0.90), 1.2);
    float punch = audioBeatPulse * audioBeatPulse;
    vec4 a = vec4(min(lvl + 0.5 * punch, 1.4), bassP, midP, highP);
    // idle rings: a soft pulse train so silence still breathes
    float ph = 0.5 + 0.5 * sin(TIME * 1.9);
    float idl = idle * 0.28 * pow(ph, 6.0);
    vec4 i = vec4(idl, idl * 0.6, 0.08 * idle, 0.0);
    return i + audioReact * a;
}

void passHist() {
    if (FRAMEINDEX < 2) { gl_FragColor = vec4(0.0); return; }   // clean start
    float x    = isf_FragNormCoord.x;
    vec4 prev  = IMG_NORM_PIXEL(hist, vec2(x, 0.5));
    // host buffers can start as NaN/inf garbage — never let it live in the ring
    if (any(notEqual(prev, prev))) prev = vec4(0.0);                 // NaN
    if (any(greaterThan(abs(prev), vec4(1.0e6)))) prev = vec4(0.0);  // inf / garbage
    prev = max(min(prev, vec4(1.5)), vec4(0.0));
    // this frame's slot, plus everything skipped since the last frame
    float slot   = fract(TIME / PERIOD);
    float dtSlot = clamp(TIMEDELTA, 0.0, 0.1) / PERIOD + 1.5 / RENDERSIZE.x;
    float behind = fract(slot - x);              // how far x sits behind the write head
    gl_FragColor = (behind < dtSlot) ? emitNow() : prev;
}

// ── pass 1: the surface ─────────────────────────────────────────────────
float carrierK() { return ringDensity * 28.0; }

float heightAt(vec2 xz, out vec4 h) {
    float r = length(xz);
    float u = r / R;
    // age in buffer turns: a ring at u was emitted u/waveSpeed periods ago
    float ageT = u / max(waveSpeed, 0.05);
    float slot = fract(TIME / PERIOD);
    h = IMG_NORM_PIXEL(hist, vec2(fract(slot - ageT), 0.5));
    h *= (ageT < 0.98) ? 1.0 : 0.0;                     // older than the ring: gone
    h *= exp(-(0.3 + 3.5 * decay) * u);                 // rings die toward the rim
    if (u > 1.0) h *= exp(-(u - 1.0) * 8.0);
    float K = carrierK();
    // the carrier moves with the buffer, so crests travel instead of standing
    float phase = K * (u - waveSpeed * SCROLL * TIME);
    float th = atan(xz.y, xz.x);
    // mids wobble the rings out of round; a slow spin so it's alive not static
    float wob = 1.0 + wobble * (0.25 + 0.75 * h.b) * 0.30 * sin(5.0 * th + u * 14.0 + TIME * 0.7)
                    + wobble * 0.12 * sin(3.0 * th - TIME * 0.4);
    // spiky crests: a raised sine so the rings read as thin ridges, not swells
    float ridge = pow(0.5 + 0.5 * sin(phase), 4.0);
    float crest = h.r * (0.15 + 0.85 * ridge) * wob;
    float body  = 0.25 * h.g * (0.5 + 0.5 * sin(phase * 0.5 + 1.0));
    float fine  = 0.06 * h.a * sin(phase * 3.0 + th * 8.0);
    // the centre dome: newest energy pushes the origin up
    float dome  = 0.10 * h.r * exp(-r * r * 6.0);
    return amp * 0.17 * (crest + body + fine) + amp * dome;
}

float heightOnly(vec2 xz) { vec4 h; return heightAt(xz, h); }

vec3 palette(float t) {
    // neon: electric blue ↔ hot magenta, cyan in the crossover; the crests
    // whiten separately so the plane never reads as a flat primary
    vec3 blue = vec3(0.10, 0.30, 1.00), mag = vec3(1.00, 0.18, 0.80), cyan = vec3(0.15, 0.95, 1.00);
    float s = 0.5 + 0.5 * sin(TAU * t);
    float c = pow(0.5 + 0.5 * sin(TAU * t * 2.0 + 1.57), 3.0);
    return mix(mix(blue, mag, s), cyan, 0.35 * c);
}

void passMain() {
    vec2 uv = (gl_FragCoord.xy - 0.5 * RENDERSIZE) / RENDERSIZE.y;

    float energy = knee(audioEnergy, 0.05, 0.9);
    float drive  = 0.25 + 0.75 * energy;

    // camera: low orbit, a slow bob, always looking at the centre
    float a  = TIME * camOrbit * 0.45;
    float bob = camBob * (0.12 * sin(TIME * 0.31) + 0.06 * sin(TIME * 0.77));
    vec3 ro = vec3(sin(a) * camDist, camHeight + bob, cos(a) * camDist);
    { vec4 h0; float H0 = heightAt(ro.xz, h0); ro.y = max(ro.y, H0 + 0.35); }
    vec3 ta = vec3(0.0, 0.12 + 0.1 * bob, 0.0);
    vec3 fw = normalize(ta - ro);
    vec3 rt = normalize(cross(fw, vec3(0.0, 1.0, 0.0)));
    vec3 up = cross(rt, fw);
    vec3 rd = normalize(uv.x * rt + uv.y * up + 1.35 * fw);

    // heightfield march
    float t = 0.0, hitT = -1.0;
    vec4 hv = vec4(0.0);
    float maxH = amp * 0.6 + 0.3;
    for (int i = 0; i < 160; i++) {
        vec3 p = ro + rd * t;
        if (p.y > maxH && rd.y > 0.0) break;
        float hgt = heightAt(p.xz, hv);
        float d = p.y - hgt;
        if (d < 0.0012 * (1.0 + t)) { hitT = t; break; }
        t += clamp(d * 0.45, 0.004, 0.06);
        if (t > 30.0) break;
    }

    vec3 col = vec3(0.0);
    vec3 bg  = vec3(0.004, 0.005, 0.012);
    if (hitT > 0.0) {
        vec3 p = ro + rd * hitT;
        float eps = 0.003 + 0.0025 * hitT;
        vec3 n = normalize(vec3(
            heightOnly(p.xz - vec2(eps, 0.0)) - heightOnly(p.xz + vec2(eps, 0.0)),
            2.0 * eps,
            heightOnly(p.xz - vec2(0.0, eps)) - heightOnly(p.xz + vec2(0.0, eps))));
        float r = length(p.xz);
        float u = r / R;
        float hgt = p.y;

        // colour by ring age (u) and height, palette slow-shifting with the track
        float pt = u * 1.1 + paletteShift + 0.06 * audioBrightness + hgt * 0.35;
        vec3 base = palette(pt);
        base = mix(base, vec3(0.20, 0.90, 1.00), 0.45 * hv.g);   // bass-heavy rings go electric cyan

        // lights: cold key from above-behind, warm fill low from the camera side
        vec3 keyDir = normalize(vec3(-0.4, 0.9, -0.3));
        vec3 filDir = normalize(vec3(ro.x, 0.6, ro.z));
        float dif = max(dot(n, keyDir), 0.0);
        float fil = max(dot(n, filDir), 0.0) * 0.35;
        vec3 hv2 = normalize(keyDir - rd);
        float spec = pow(max(dot(n, hv2), 0.0), 48.0);
        float fres = pow(1.0 - max(dot(n, -rd), 0.0), 3.0);

        // the floor is near-black; the wave is what glows
        float crest = smoothstep(0.008, 0.13 * amp + 0.015, hgt);
        vec3 surf = base * (0.012 + 0.06 * dif + 0.4 * fil) * (0.12 + 0.88 * crest);
        surf += base * fres * (0.10 + 0.9 * crest);
        surf += base * crest * crest * glow * 0.8;
        surf += vec3(0.75, 0.9, 1.0) * pow(crest, 4.0) * glow * 0.35;  // crests go white-hot
        surf += vec3(1.0) * spec * (0.5 + 1.5 * hv.a);        // highs sparkle the facets
        surf += vec3(0.9, 0.95, 1.0) * spec * spec * 1.5 * hv.a;

        // contour lines: rings at fixed radii + faint spokes, crisp and thin
        float lw = 0.012 + 0.006 * hitT;
        float ring = abs(fract(r * 1.8) - 0.5);
        float lineR = 1.0 - smoothstep(0.0, lw * 1.8, ring - (0.5 - lw * 1.8) + 0.0);
        lineR = 1.0 - smoothstep(0.0, lw * 2.0, abs(fract(r * 1.8 + 0.5) - 0.5) * (1.0 / 1.8));
        float th = atan(p.z, p.x);
        float spoke = 1.0 - smoothstep(0.0, lw * 3.0, abs(fract(th * 24.0 / TAU + 0.5) - 0.5) * r * 0.26);
        float lines = clamp(lineR + 0.35 * spoke, 0.0, 1.0) * lineAmt;
        surf += mix(base, vec3(1.0), 0.35) * lines * (0.30 + 0.9 * crest) * 0.9;

        // fog to black — deep, so the far rim vanishes instead of greying
        float fog = exp(-hitT * (0.19 - 0.04 * energy));
        col = mix(bg, surf, fog);
    } else {
        // sky: nothing but the faintest gradient, the horizon stays black
        col = bg + vec3(0.01, 0.012, 0.02) * pow(max(rd.y, 0.0), 0.6);
    }

    // finish: lift with energy, soft clip, THEN the S-curve (on 0..1 only —
    // a smoothstep on HDR values flips the hottest channel negative)
    col *= brightness * (0.85 + 0.3 * drive);
    col = 1.0 - exp(-col * 1.25);
    col = mix(col, col * col * (3.0 - 2.0 * col), 0.5);
    // vignette
    vec2 q = gl_FragCoord.xy / RENDERSIZE - 0.5;
    col *= 1.0 - 0.55 * dot(q, q) * 1.6;
    gl_FragColor = vec4(col, 1.0);
}

void main() {
    if (PASSINDEX == 0) passHist();
    else                passMain();
}

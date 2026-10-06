/*{
  "DESCRIPTION": "Autonomous — a shader that runs its own AV show. Six embedded scenes (neon tunnel, plasma ocean, warp starfield, kaleido bloom, synthwave grid, aurora curtains) shuffle on a slow hashed schedule. The show clock listens to the music: song intensity accelerates scene changes, buildups widen the transition window, and beat drops slam the current transition through with a glitch-flash cut. Transition style is selectable (crossfade / wavy wipe / zoom punch / glitch slice) or auto-shuffled per change. Universal params: show speed, transition style + length, audio reactivity, hue, brightness. Idle: a calm, endless self-curated show.",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "showSpeed",
      "LABEL": "Show Speed",
      "TYPE": "float",
      "MIN": 0.15,
      "MAX": 2.5,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "transStyle",
      "LABEL": "Transition (0 Auto / 1 Fade / 2 Wipe / 3 Zoom / 4 Glitch)",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 4,
      "DEFAULT": 0,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "transLength",
      "LABEL": "Transition Length",
      "TYPE": "float",
      "MIN": 0.05,
      "MAX": 0.45,
      "DEFAULT": 0.18,
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
      "NAME": "brightness",
      "LABEL": "Brightness",
      "TYPE": "float",
      "MIN": 0.3,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio Reactivity",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.35,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

// AUTONOMOUS — a meta-shader that VJs itself. All analytic, no persistence.
// Show clock: clock = (TIME + audio-accumulated time) * speed / sceneLen, so
// loud passages genuinely reach the next scene sooner while silence drifts
// slowly. Scene k's local time is (clock - k) * sceneLen + hash(k) — continuous
// for a fixed k across the whole transition, so handoffs never jump.
// Transitions live in the last transLength of each cycle; audioBuildup widens
// the window (anticipation), audioDrop + beat pulse slam the blend through
// with a glitch flash. Scene order is a hashed shuffle with no repeats.

#define TAU 6.2831853
#define SLEN 24.0

float hash11(float p) {
    p = fract(p * 0.1031);
    p *= p + 33.33;
    p *= p + p;
    return fract(p);
}
float hash21(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}
float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    vec2 u = f * f * (3.0 - 2.0 * f);
    float a = hash21(i), b = hash21(i + vec2(1.0, 0.0));
    float c = hash21(i + vec2(0.0, 1.0)), d = hash21(i + vec2(1.0, 1.0));
    return mix(mix(a, b, u.x), mix(c, d, u.x), u.y);
}
float fbm(vec2 p) {
    float v = 0.0;
    v += vnoise(p) * 0.55;
    v += vnoise(p * 2.13 + 7.7) * 0.28;
    v += vnoise(p * 4.31 + 3.1) * 0.17;
    return v;
}
vec3 pal(float t) {
    return 0.52 + 0.46 * cos(TAU * (t + vec3(0.0, 0.33, 0.67)));
}

// aud = (bassP, midL, highP, pulse); each scene routes bands per the playbook:
// bass = big/global, mids = detail/turbulence, highs = fine sparkle.

vec3 sceneTunnel(vec2 q, float t, float hue, vec4 aud, float drive) {
    float r = length(q) + 1e-4;
    float a = atan(q.y, q.x);
    float z = 0.30 / r + t * 1.4;
    float zoom = 1.0 + 0.20 * aud.x;                      // bass pushes depth
    float rings = 0.5 + 0.5 * cos(z * 3.1 * zoom);
    rings = pow(rings, 3.0);
    float spokes = 0.5 + 0.5 * cos(a * 12.0 + z * 0.8 + t * 0.3);
    spokes = pow(spokes, 2.0) * (0.5 + 0.5 * aud.y);      // mids feed spokes
    vec3 col = pal(hue + z * 0.035 + a / TAU * 0.15)
             * (rings * 0.85 + spokes * 0.55) * drive;
    col *= smoothstep(0.0, 0.22, r);                       // dark core
    float spark = step(0.995, hash21(floor(vec2(a * 40.0, z * 6.0))));
    col += vec3(1.0) * spark * aud.z * 0.7;                // highs sparkle walls
    return col * exp(-r * 0.55);
}

vec3 scenePlasma(vec2 q, float t, float hue, vec4 aud, float drive) {
    float w = 1.0 + 0.35 * aud.x;                          // bass swells scale
    float v = sin(q.x * 5.2 * w + t * 0.9)
            + sin(q.y * 6.1 * w - t * 0.7)
            + sin((q.x + q.y) * 4.3 + t * 1.15)
            + sin(length(q) * 8.5 - t * 1.5);
    float det = sin(dot(q, vec2(11.0, 9.0)) + t * 1.8) * (0.4 + 0.6 * aud.y);
    vec3 col = pal(hue + 0.5 + v * 0.10 + det * 0.04);
    float lum = 0.45 + 0.28 * sin(v * 1.3 + t * 0.4) + 0.10 * aud.z;
    return col * lum * (0.55 + 0.45 * drive);
}

vec3 sceneStars(vec2 q, float t, float hue, vec4 aud, float drive) {
    vec3 col = vec3(0.0);
    for (int L = 0; L < 3; L++) {
        float fl = float(L);
        float s = fract(t * 0.09 * (1.0 + 0.25 * aud.x) + fl / 3.0);
        float scale = mix(0.6, 7.0, s * s);
        vec2 p2 = q * scale + vec2(hash11(fl * 3.7) * 9.0);
        vec2 cell = floor(p2 * 3.0);
        vec2 f2 = fract(p2 * 3.0) - 0.5;
        float h = hash21(cell + fl * 17.0);
        vec2 off = vec2(hash21(cell + 3.1), hash21(cell + 6.4)) - 0.5;
        float d = length(f2 - off * 0.8);
        float star = exp(-d * d / 0.004) * step(0.55, h);
        float fade = sin(s * 3.14159);                     // in/out with depth
        vec3 sc = pal(hue + 0.62 + h * 0.25);
        col += sc * star * fade * (0.5 + 0.5 * drive + aud.z * 0.8 * step(0.9, h));
    }
    float core = exp(-dot(q, q) * 3.0);
    col += pal(hue + 0.7) * core * 0.14 * (drive + aud.w * 0.8);  // beat glows core
    return col;
}

vec3 sceneKaleido(vec2 q, float t, float hue, vec4 aud, float drive) {
    float a = atan(q.y, q.x);
    float r = length(q);
    float seg = TAU / 6.0;
    a = abs(mod(a, seg) - seg * 0.5);
    vec2 p2 = vec2(cos(a), sin(a)) * r;
    float n = fbm(p2 * (3.2 + 0.8 * aud.y) + vec2(t * 0.12, -t * 0.09));
    float ridg = abs(n - 0.5) * 2.0;
    float petals = pow(1.0 - ridg, 4.0 + 3.0 * aud.x);
    vec3 col = pal(hue + 0.15 + r * 0.35 - n * 0.22) * petals * (0.6 + 0.5 * drive);
    col += vec3(1.0, 0.95, 0.9) * pow(1.0 - ridg, 14.0) * (0.25 + aud.z * 0.9);
    return col * exp(-r * 0.8);
}

vec3 sceneGrid(vec2 q, float t, float hue, vec4 aud, float drive) {
    vec3 col = vec3(0.0);
    float hor = 0.04;
    if (q.y < hor) {
        float z = 1.0 / (hor - q.y + 0.02);
        float gx = q.x * z;
        float travel = z * 0.5 + t * (1.6 + 0.8 * aud.x);   // bass drives speed
        float lx = pow(abs(fract(gx * 0.8) - 0.5) * 2.0, 8.0);
        float lz = pow(abs(fract(travel) - 0.5) * 2.0, 8.0);
        float fade = exp(-(z - 1.0) * 0.10);
        col += pal(hue + 0.85) * (lx + lz) * fade * (0.7 + 0.5 * drive);
        col += pal(hue + 0.85) * 0.05 * fade;
    } else {
        float sunR = 0.20 * (1.0 + 0.18 * aud.x);           // bass pumps the sun
        vec2 sp = q - vec2(0.0, hor + 0.16);
        float sd = length(sp) - sunR;
        float sun = smoothstep(0.012, -0.012, sd);
        sun *= 0.55 + 0.45 * step(0.5, fract(sp.y * 26.0 + t * 0.5)); // banded
        col += mix(pal(hue + 0.02), pal(hue + 0.10), clamp(sp.y * 3.0 + 0.5, 0.0, 1.0)) * sun;
        col += pal(hue + 0.05) * exp(-max(sd, 0.0) * 9.0) * 0.35 * drive;
        vec2 sc = floor(q * 140.0);                         // static star points
        vec2 sf = fract(q * 140.0) - 0.5;
        float st = step(0.997, hash21(sc)) * exp(-dot(sf, sf) * 9.0);
        col += vec3(st) * (0.10 + aud.z * 0.55) * (1.0 - sun);  // highs twinkle
    }
    return col;
}

vec3 sceneAurora(vec2 q, float t, float hue, vec4 aud, float drive) {
    vec3 col = vec3(0.008, 0.01, 0.02);
    for (int L = 0; L < 3; L++) {
        float fl = float(L);
        float n = fbm(vec2(q.x * (1.8 + fl * 0.7) + t * (0.05 + 0.03 * fl),
                           fl * 5.1 + t * 0.02));
        float y0 = (n - 0.5) * 0.9 + 0.05 * sin(t * 0.2 + fl * 2.1);
        float d = q.y - y0;
        float curtain = exp(-abs(d) * (3.2 - aud.x * 1.2)) * (1.0 - fl * 0.22);
        vec3 ac = mix(pal(hue + 0.38), pal(hue + 0.55), clamp(d * 2.0 + 0.5, 0.0, 1.0));
        col += ac * curtain * (0.28 + 0.28 * drive + 0.14 * aud.y);
    }
    float shimmer = vnoise(vec2(q.x * 60.0, q.y * 4.0 - t * 0.4));
    col *= 0.85 + 0.15 * shimmer + aud.z * 0.20 * shimmer;   // highs shimmer
    return col;
}

vec3 renderScene(int id, vec2 q, float t, float hue, vec4 aud, float drive) {
    if (id == 0) return sceneTunnel(q, t, hue, aud, drive);
    if (id == 1) return scenePlasma(q, t, hue, aud, drive);
    if (id == 2) return sceneStars(q, t, hue, aud, drive);
    if (id == 3) return sceneKaleido(q, t, hue, aud, drive);
    if (id == 4) return sceneGrid(q, t, hue, aud, drive);
    return sceneAurora(q, t, hue, aud, drive);
}

int pickScene(float cyc) {
    int a = int(floor(hash11(cyc * 17.31 + 3.7) * 5.9999));
    int prev = int(floor(hash11((cyc - 1.0) * 17.31 + 3.7) * 5.9999));
    if (a == prev) a = int(mod(float(a + 1), 6.0));          // never repeat
    return a;
}

void main() {
    vec2 uv = isf_FragNormCoord.xy;
    float asp = RENDERSIZE.x / RENDERSIZE.y;
    vec2 q = vec2((uv.x - 0.5) * asp, uv.y - 0.5);

    // ---- audio conditioning ----
    float amt   = clamp(audioReact, 0.0, 1.0);
    float bassP = pow(knee(audioBass, 0.05, 0.85), 1.6);
    float midL  = clamp(audioMid, 0.0, 1.0);
    float highP = pow(knee(audioHigh, 0.10, 0.90), 1.2);
    float pulse = clamp(audioBeatPulse, 0.0, 1.0);
    float drive = 0.25 + 0.75 * knee(audioEnergy, 0.05, 0.9);
    float build = knee(audioBuildup, 0.15, 0.9);
    float dropP = knee(audioDrop, 0.10, 0.85) * (0.4 + 0.6 * pulse);
    vec4 aud = vec4(bassP, midL, highP, pulse) * amt;
    float sDrive = mix(0.6, drive, amt);                     // scenes' idle floor

    // beat pump: whole frame breathes very slightly with big hits
    q *= 1.0 - amt * 0.045 * pulse * pulse * drive;

    // ---- show clock: intensity makes the show move faster ----
    float clock = (TIME * 0.72 + amt * 0.90 * audioTime) * showSpeed / SLEN;
    float cyc = floor(clock);
    float f = fract(clock);
    int idA = pickScene(cyc);
    int idB = pickScene(cyc + 1.0);

    // scene-local times: continuous for a fixed k, offsets vary per visit
    float tA = (clock - cyc) * SLEN + hash11(cyc * 5.13) * 60.0;
    float tB = (clock - cyc - 1.0) * SLEN + hash11((cyc + 1.0) * 5.13) * 60.0;
    float hueA = hueShift + hash11(cyc * 2.71) * 0.30;
    float hueB = hueShift + hash11((cyc + 1.0) * 2.71) * 0.30;

    // ---- transition window: buildup widens it, drops slam it through ----
    float tl = transLength * (1.0 + amt * 0.8 * build);
    float base = smoothstep(1.0 - tl, 1.0, f);
    float b = clamp(base + amt * 0.55 * dropP * smoothstep(0.02, 0.25, base), 0.0, 1.0);
    b = b * b * (3.0 - 2.0 * b);

    // style: 0 = auto-shuffle per transition
    float styleF = transStyle < 0.5
        ? 1.0 + floor(hash11(cyc * 9.77 + 1.1) * 3.9999)
        : clamp(floor(transStyle + 0.5), 1.0, 4.0);

    vec2 qA = q, qB = q;
    float glitchZone = b * (1.0 - b) * 4.0;                  // peaks mid-blend
    if (styleF > 2.5 && styleF < 3.5) {                      // zoom punch
        qA = q * (1.0 + 1.6 * b * b);
        qB = q * (2.2 - 1.2 * b);
    } else if (styleF > 3.5) {                               // glitch slice
        float band = floor(uv.y * 22.0);
        float j = (hash11(band * 7.7 + floor(f * 60.0)) - 0.5);
        qA.x += j * 0.55 * glitchZone;
        qB.x -= j * 0.35 * glitchZone;
    }

    vec3 colA = renderScene(idA, qA, tA, hueA, aud, sDrive);
    vec3 colB = renderScene(idB, qB, tB, hueB, aud, sDrive);

    vec3 col;
    if (styleF < 1.5) {                                      // crossfade
        col = mix(colA, colB, b);
    } else if (styleF < 2.5) {                               // wavy wipe
        float dirf = sign(hash11(cyc * 4.4) - 0.5);
        float edge = 0.5 + dirf * (uv.x - 0.5) + 0.10 * sin(uv.y * 9.0 + f * 6.0);
        // map b so the sweep fully clears the wavy edge range [-0.24, 1.24]
        float m = smoothstep(edge - 0.14, edge + 0.14, mix(-0.28, 1.28, b));
        col = mix(colA, colB, m);
    } else if (styleF < 3.5) {                               // zoom punch
        col = mix(colA * (1.0 - b * 0.6), colB, b);
        col += vec3(1.0) * glitchZone * 0.06;                // flash at the punch
    } else {                                                 // glitch slice
        float band = floor(uv.y * 22.0);
        // sharpened blend: transition tails stay pure A/B (no stray lone bands)
        float bq = smoothstep(0.10, 0.90, b);
        // hash remapped into (0.02, 0.98): step(h, 0) fires on an exact-zero
        // hash, which painted a lone full-strength band of scene B at b = 0
        float cut = step(mix(0.02, 0.98, hash11(band * 3.3 + floor(f * 30.0))), bq);
        col = mix(colA, colB, cut);
        col += pal(hueB + band * 0.02) * glitchZone * 0.10;
    }

    // ---- drop event: glitch-flash the whole frame (masks any blend snap) ----
    float dropFx = amt * dropP;
    if (dropFx > 0.01) {
        float band = floor(uv.y * 30.0);
        float j = hash11(band * 11.3 + floor(TIME * 24.0));
        col *= 1.0 + (j - 0.5) * 0.55 * dropFx;
        col += vec3(0.9, 0.95, 1.0) * dropFx * 0.12;
    }

    // ---- unify: one show, one finish ----
    col *= 1.0 + amt * 0.15 * bassP;                         // gentle master swell
    vec2 vd = uv - 0.5;
    col *= 1.0 - 0.42 * dot(vd, vd) * 2.0;
    float grain = hash21(uv * RENDERSIZE.xy + 4.2);
    col += (grain - 0.5) * 0.012;

    col = col * brightness / (1.0 + 0.70 * max(brightness - 1.0, 0.0) * col);
    gl_FragColor = vec4(clamp(col, 0.0, 1.0), 1.0);
}

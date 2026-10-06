/*{
  "DESCRIPTION": "Chrome Ripple — liquid-metal ripples spreading from the centre across a scratched chrome plate. Every crest catches thin-film iridescence (oil-slick rainbow) where the surface tilts toward the light; the plate carries brushed micro-scratches, grit and a four-lobed caustic at the origin. Rings expand continuously like drops on a pool; beats drop new rings that travel out and die at the rim, bass swells the wave height, mids drift the film colour, highs sharpen the specular glint. Beautiful in silence, alive with sound.",
  "CREDIT": "Easel / ShaderClaw house shader, 2026-09-11 — after the metallic-iridescent-ripples + expanding-radial-circles references",
  "CATEGORIES": ["Generator", "3D", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "ringDensity", "LABEL": "Ring Density",    "TYPE": "float", "MIN": 0.3, "MAX": 3.0, "DEFAULT": 1.0,  "GROUP": "Ripple" },
    { "NAME": "rippleSpeed", "LABEL": "Expand Speed",    "TYPE": "float", "MIN": 0.0, "MAX": 3.0, "DEFAULT": 1.0,  "GROUP": "Ripple" },
    { "NAME": "rippleAmp",   "LABEL": "Wave Height",     "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 1.0,  "GROUP": "Ripple" },
    { "NAME": "idleRipple",  "LABEL": "Idle Ripple",     "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.55, "GROUP": "Ripple" },
    { "NAME": "fadeOut",     "LABEL": "Rim Fade",        "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.35, "GROUP": "Ripple" },
    { "NAME": "centerX",     "LABEL": "Centre X",        "TYPE": "float", "MIN": -1.0, "MAX": 1.0, "DEFAULT": 0.0, "GROUP": "Ripple" },
    { "NAME": "centerY",     "LABEL": "Centre Y",        "TYPE": "float", "MIN": -1.0, "MAX": 1.0, "DEFAULT": 0.0, "GROUP": "Ripple" },
    { "NAME": "iridescence", "LABEL": "Iridescence",     "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.7,  "GROUP": "Surface" },
    { "NAME": "filmShift",   "LABEL": "Film Colour",     "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0,  "GROUP": "Surface" },
    { "NAME": "metalTint",   "LABEL": "Metal Tint",      "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.55, "GROUP": "Surface" },
    { "NAME": "scratches",   "LABEL": "Scratches",       "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.6,  "GROUP": "Surface" },
    { "NAME": "grit",        "LABEL": "Grit",            "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.5,  "GROUP": "Surface" },
    { "NAME": "lightSpin",   "LABEL": "Light Drift",     "TYPE": "float", "MIN": -1.0, "MAX": 1.0, "DEFAULT": 0.15, "GROUP": "Surface" },
    { "NAME": "brightness",  "LABEL": "Brightness",      "TYPE": "float", "MIN": 0.3, "MAX": 2.0, "DEFAULT": 1.0,  "GROUP": "Surface" },
    { "NAME": "audioReact",  "LABEL": "Audio React",     "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.6,  "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",    "LABEL": "Motion Trails",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.08, "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",    "LABEL": "Bloom Depth",     "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.35, "GROUP": "Depth / Passes" },
    { "NAME": "aberration",  "LABEL": "Lens Depth",      "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.3,  "GROUP": "Depth / Passes" },
    { "NAME": "transparentBg", "LABEL": "Transparent BG", "TYPE": "bool", "DEFAULT": false, "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "crHist", "PERSISTENT": true },
    { "TARGET": "crScene" },
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// CHROME RIPPLE
//   pass 0  crHist  — 1-D ring buffer of audio energy, written at a moving
//                     slot each frame (time-indexed, frame-rate independent).
//                     Reading it back by "age = radius / speed" turns the
//                     history into rings that were born at the centre and
//                     travel outward — beats become ripples you watch expand.
//   pass 1  crScene — heightfield = idle carrier train + history rings;
//                     analytic normal → chrome env reflection + thin-film
//                     iridescence + brushed scratches + grit + centre caustic.
//   pass 2  abTrail — house motion trail.
//   pass 3  final   — house bloom + aberration + HD finisher.
// ─────────────────────────────────────────────────────────────────────────

#define R   RENDERSIZE.xy
#define PI  3.14159265
#define TAU 6.28318531
#define PERIOD 6.0          // seconds of history in the ring buffer

float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}
float hash11(float n) { return fract(sin(n * 91.3458) * 47453.5453); }

float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    return mix(mix(hash21(i), hash21(i + vec2(1, 0)), f.x),
               mix(hash21(i + vec2(0, 1)), hash21(i + vec2(1, 1)), f.x), f.y);
}

// ── audio shaping ────────────────────────────────────────────────────────
float bassP()  { return pow(smoothstep(0.05, 0.85, audioBass), 1.4); }
float midP()   { return pow(smoothstep(0.06, 0.85, audioMid),  1.2); }
float highP()  { return pow(smoothstep(0.10, 0.90, audioHigh), 1.2); }
float beatP()  { return clamp(audioBeatPulse, 0.0, 1.0); }
float levP()   { return clamp(audioLevel, 0.0, 1.0); }

// ── pass 0: history ring buffer ──────────────────────────────────────────
vec4 emitNow() {
    float a = audioReact;
    // r = crest height (beats + bass), g = wide swell (level), b = film drift (mid)
    float crest = 0.55 * beatP() + 0.45 * bassP();
    float swell = 0.6 * levP();
    return vec4(a * crest, a * swell, a * midP(), 0.0);
}

void passHist() {
    if (FRAMEINDEX < 2) { gl_FragColor = vec4(0.0); return; }
    float x   = gl_FragCoord.x / R.x;
    vec4 prev = texture2D(crHist, vec2(x, 0.5));
    if (any(notEqual(prev, prev))) prev = vec4(0.0);
    if (any(greaterThan(abs(prev), vec4(1.0e6)))) prev = vec4(0.0);
    prev = clamp(prev, vec4(0.0), vec4(1.5));
    float slot   = fract(TIME / PERIOD);
    float dtSlot = clamp(TIMEDELTA, 0.0, 0.1) / PERIOD + 1.5 / R.x;
    float behind = fract(slot - x);
    gl_FragColor = (behind < dtSlot) ? emitNow() : prev;
}

// ── heightfield ──────────────────────────────────────────────────────────
// u = radius in "screen halves" (0 centre → ~1 edge).  Returns height,
// and film-drift via out param.
float heightAt(float u, out float film) {
    float spd = max(rippleSpeed, 0.02);
    float K   = ringDensity * 22.0;

    // (a) idle carrier: rings born at the centre, expanding forever (ref A)
    float ph   = K * u - TIME * spd * 6.0;
    float idle = idleRipple * 0.5 * sin(ph);
    // envelope: a "drop" that grows — bright close in, fading out to the rim
    float env  = exp(-(0.4 + 2.6 * fadeOut) * u);

    // (b) history rings: energy emitted ageT periods ago now sits at radius u
    float ageT = u / (spd * 0.55);
    float slot = fract(TIME / PERIOD);
    vec4 h = texture2D(crHist, vec2(fract(slot - ageT), 0.5));
    h *= (ageT < 0.98) ? 1.0 : 0.0;
    film = h.b;
    // sharp crest + wide swell, both riding the same carrier so they travel
    float ridge = pow(0.5 + 0.5 * sin(ph), 3.0);
    float crest = h.r * (1.2 * ridge + 0.3 * sin(ph));
    float swell = h.g * 0.5 * sin(ph * 0.5 + 0.7);

    // (c) centre dome — origin pushes up with the newest energy
    vec4 now  = texture2D(crHist, vec2(fract(slot - 0.004), 0.5));
    float dome = (0.35 + 0.9 * now.r) * exp(-u * u * 40.0);

    return rippleAmp * env * (idle + crest + swell) + dome * rippleAmp * 0.6;
}

// ── shading helpers ──────────────────────────────────────────────────────
vec3 thinFilm(float t) {
    // spectral-ish oil-slick ramp; t ≈ film thickness / phase
    return 0.5 + 0.5 * cos(TAU * (t * vec3(1.0, 1.12, 1.26) + vec3(0.00, 0.10, 0.24)));
}

vec3 chromeEnv(vec3 rd) {
    // studio environment: bright horizon band, dark floor, soft-lit ceiling,
    // plus a couple of hard strip lights so the metal gets crisp reflections
    float y = rd.y;
    vec3 sky = mix(vec3(0.04, 0.045, 0.06), vec3(0.85, 0.88, 0.95), smoothstep(-0.25, 0.65, y));
    float band = exp(-abs(y - 0.08) * 9.0);                  // horizon strip
    float strip1 = pow(max(0.0, 1.0 - abs(y - 0.42) * 9.0), 3.0);
    float strip2 = pow(max(0.0, 1.0 - abs(y + 0.30) * 12.0), 3.0) * 0.35;
    float ang = atan(rd.z, rd.x);
    float lamp = pow(max(0.0, cos(ang - 1.1)), 8.0) * strip1;
    return sky * 0.55 + vec3(1.0) * (band * 0.55 + lamp * 1.1 + strip2 * 0.7);
}

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 p  = (fc - 0.5 * R) / R.y * 2.0;           // y ∈ [-1,1]
    vec2 c  = vec2(centerX * R.x / R.y, centerY);
    vec2 d  = p - c;
    float u = length(d);
    float th = atan(d.y, d.x);

    float amt = audioReact;
    float t   = TIME;

    // ── height + analytic-ish normal (central difference in r, cheap) ──
    float film0, filmA, filmB;
    float eps = 0.006;
    float h0 = heightAt(u, film0);
    float ha = heightAt(u + eps, filmA);
    float hb = heightAt(max(u - eps, 0.0), filmB);
    float dh = (ha - hb) / (2.0 * eps);              // dh/dr
    // brushed micro-scratches: three straight brush directions (ref shows
    // straight ticks across the plate, not circular brushing) — seam-free
    float scr = 0.0;
    {
        float acc = 0.0;
        for (int k = 0; k < 3; k++) {
            float fk = float(k);
            float ang2 = fk * 1.047 + 0.37;
            vec2 rp = mat2(cos(ang2), -sin(ang2), sin(ang2), cos(ang2)) * p;
            float lines = vnoise(rp * vec2(2.5, 160.0) + fk * 13.0);
            acc += smoothstep(0.80, 0.98, lines) * (1.0 - 0.25 * fk);
        }
        scr = acc * scratches;
    }
    float gr = (vnoise(fc * 0.9) * 0.6 + vnoise(fc * 0.31 + 5.0) * 0.4 - 0.5) * grit;

    // normal in view space (plate faces +z)
    vec2 radial = (u > 1e-4) ? d / u : vec2(1.0, 0.0);
    float slope = dh * 0.55;
    vec2 g = radial * slope + vec2(gr) * 0.025;
    vec3 n = normalize(vec3(-g, 1.0));

    // ── lighting: chrome reflection of the studio env ──
    float spin = t * lightSpin * 0.35;
    vec3 rd = reflect(vec3(0.0, 0.0, -1.0), n);
    rd.xy = mat2(cos(spin), -sin(spin), sin(spin), cos(spin)) * rd.xy;
    vec3 env = chromeEnv(rd);

    // metal base tint: cool steel ↔ warm bronze (kept near-neutral)
    vec3 tintCool = vec3(0.82, 0.88, 0.98);
    vec3 tintWarm = vec3(0.98, 0.88, 0.74);
    vec3 tint = mix(tintCool, tintWarm, metalTint * 0.6);
    float fres = pow(1.0 - max(n.z, 0.0), 2.0);
    vec3 col = env * tint * (0.55 + 0.45 * fres) * (0.85 + 0.25 * h0);

    // ── thin-film iridescence: only in angular pockets on the flanks ──
    float tilt = clamp(length(g) * 0.9, 0.0, 1.0);
    float thick = 0.05 + 0.55 * tilt + 0.25 * sin(th * 2.0 + u * 5.0 - t * 0.15)
                + filmShift + amt * 0.25 * film0 + amt * 0.12 * midP();
    vec3 rainbow = thinFilm(thick);
    // seam-free angular coordinate for the pockets
    vec2 ac = vec2(cos(th), sin(th));
    float pocket = smoothstep(0.42, 0.88, vnoise(ac * 1.4 + u * 2.0 + t * 0.04))
                 * smoothstep(0.40, 0.82, vnoise(ac * 2.9 + u * 3.5 + 17.0 - t * 0.03));
    float irMask = iridescence * smoothstep(0.08, 0.55, tilt) * pocket;
    col = mix(col, col * 0.4 + rainbow * (0.45 + 0.55 * env.g), irMask);

    // ── crisp specular glints (highs sharpen) ──
    vec3 L = normalize(vec3(cos(spin + 1.1) * 0.6, sin(spin + 1.1) * 0.6 + 0.5, 0.75));
    vec3 Hh = normalize(L + vec3(0.0, 0.0, 1.0));
    float shin = mix(28.0, 90.0, amt * highP());
    float spec = pow(max(dot(n, Hh), 0.0), shin);
    col += vec3(1.0, 0.98, 0.94) * spec * (0.9 + 0.7 * amt * highP());

    // ── dark scratch ticks + grit specks ──
    col *= 1.0 - scr * 0.6;
    col += vec3(gr) * 0.10;
    col *= 1.0 - grit * 0.35 * smoothstep(0.86, 1.0, vnoise(fc * 0.55 + 9.0));

    // ── centre caustic: the four-lobed cross from the reference ──
    {
        float lobe = pow(abs(cos(2.0 * th)), 6.0);
        float core = exp(-u * u * 260.0);
        float ring = exp(-pow((u - 0.055) * 60.0, 2.0));
        col += vec3(1.0) * (lobe * core * 1.4 + ring * 0.35) * (0.6 + 0.8 * rippleAmp);
        col *= 1.0 - 0.6 * exp(-u * u * 900.0) * (1.0 - lobe);   // dark pinch between lobes
        col += thinFilm(thick + 0.4) * core * lobe * 0.5 * iridescence;
    }

    // level breathes the whole plate very gently
    col *= 1.0 + amt * 0.12 * levP();

    col += (hash21(fc + fract(t) * vec2(17.0, 29.0)) - 0.5) * 0.012;
    return col;
}

void main() {
    vec2 uv = gl_FragCoord.xy / R;
    if (PASSINDEX == 0) {
        passHist();
    } else if (PASSINDEX == 1) {
        gl_FragColor = vec4(clamp(renderScene(), 0.0, 1.0), 1.0);
    } else if (PASSINDEX == 2) {
        vec3 col  = texture2D(crScene, uv).rgb;
        vec3 prev = texture2D(abTrail, uv).rgb;
        if (any(notEqual(prev, prev))) prev = vec3(0.0);
        float decay = 0.50 + 0.46 * trailAmt;
        col = max(col, prev * decay - 0.0045);
        col += (hash21(gl_FragCoord.xy + fract(TIME) * 61.0) - 0.5) * 0.006;
        gl_FragColor = vec4(clamp(col, 0.0, 1.0), 1.0);
    } else {
        vec2 dir = uv - 0.5;
        float ab = aberration * 0.0045;
        vec3 base;
        base.r = texture2D(abTrail, uv + dir * ab).r;
        base.g = texture2D(abTrail, uv).g;
        base.b = texture2D(abTrail, uv - dir * ab).b;
        vec3 bl = vec3(0.0);
        for (int i = 0; i < 8; i++) {
            float an = float(i) * 0.7853982;
            vec2 o = vec2(cos(an), sin(an)) * (3.5 / R.y);
            bl += texture2D(abTrail, uv + o).rgb;
            bl += texture2D(abTrail, uv + o * 2.6).rgb * 0.6;
        }
        bl /= 12.8;
        bl = max(bl - 0.55, 0.0);
        vec3 col = base + bl * bl * bloomAmt * 1.6;
        float lvl = clamp(audioLevel, 0.0, 1.0);
        col *= brightness * mix(1.0, 0.70 + 0.30 * lvl, audioReact)
             * (1.0 + audioReact * 0.05 * clamp(audioBeatPulse, 0.0, 1.0));
        col = clamp(col, 0.0, 1.0);
        col = mix(col, col * col * (3.0 - 2.0 * col), 0.55);
        float lumHD = dot(col, vec3(0.299, 0.587, 0.114));
        col = clamp(mix(vec3(lumHD), col, 1.22), 0.0, 1.0);
        float alphaOut = transparentBg ? smoothstep(0.02, 0.15, lumHD) : 1.0;
        gl_FragColor = vec4(col, alphaOut);
    }
}

/*{
  "DESCRIPTION": "Grid Bloom × Boids — abstract gradient-checker cells haunted by a boid swarm. A tight grid of hot magenta-to-peach gradient squares pulses and flips while glowing boid particles flock across the surface, their trails blending into the cell seams. Bass breathes cells and speeds the flock, mids roll gradient waves, highs shimmer seams and spark boid glints, beats flip diagonals and surge the swarm.",
  "CATEGORIES": ["Generator", "Audio Reactive", "Simulation"],
  "INPUTS": [
    { "NAME": "gridN",       "LABEL": "Grid Cells",    "TYPE": "float", "MIN": 2.0,  "MAX": 9.0,  "DEFAULT": 3.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "frameAmt",    "LABEL": "Cream Frame",   "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.5,  "GROUP": "Shape / Geometry" },
    { "NAME": "sight",       "LABEL": "Boid Sight",    "TYPE": "float", "MIN": 10.0, "MAX": 120.0,"DEFAULT": 40.0, "GROUP": "Shape / Geometry" },
    { "NAME": "waveSpeed",   "LABEL": "Gradient Wave", "TYPE": "float", "MIN": -2.0, "MAX": 2.0,  "DEFAULT": 0.5,  "GROUP": "Motion / Animation" },
    { "NAME": "flipSpeed",   "LABEL": "Flip Ripple",   "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 0.4,  "GROUP": "Motion / Animation" },
    { "NAME": "motionSpeed", "LABEL": "Motion Speed",  "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "boidSpeed",   "LABEL": "Boid Speed",    "TYPE": "float", "MIN": 0.5,  "MAX": 6.0,  "DEFAULT": 2.5,  "GROUP": "Motion / Animation" },
    { "NAME": "simDt",       "LABEL": "Sim Speed",     "TYPE": "float", "MIN": 0.1,  "MAX": 2.0,  "DEFAULT": 0.7,  "GROUP": "Motion / Animation" },
    { "NAME": "colorA",      "LABEL": "Hot Color",     "TYPE": "color", "DEFAULT": [0.96, 0.05, 0.55, 1.0], "GROUP": "Color" },
    { "NAME": "colorB",      "LABEL": "Soft Color",    "TYPE": "color", "DEFAULT": [1.00, 0.78, 0.55, 1.0], "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "hueShift",    "LABEL": "Hue Shift",     "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "boidMix",     "LABEL": "Boid Blend",    "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.45, "GROUP": "Color" },
    { "NAME": "audioReact",  "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.45, "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",    "LABEL": "Motion Trails", "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.06, "GROUP": "Depth / Passes" },
    { "NAME": "boidTrail",   "LABEL": "Boid Trail",    "TYPE": "float", "MIN": 0.0,  "MAX": 0.995,"DEFAULT": 0.93, "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",    "LABEL": "Bloom Depth",   "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.35, "GROUP": "Depth / Passes" },
    { "NAME": "aberration",  "LABEL": "Lens Depth",    "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.3,  "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "boidState", "PERSISTENT": true },
    { "TARGET": "boidRender","PERSISTENT": true },
    { "TARGET": "gridTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// PASS LAYOUT
//   0 → boidState  : boid sim (pos+vel per texel row 0)
//   1 → boidRender : boid dots + ghost trail
//   2 → gridTrail  : grid scene composited with boids + trail feedback
//   3 → screen     : bloom, aberration, tone, final output
// ─────────────────────────────────────────────────────────────────────────

#define R RENDERSIZE.xy
#define TAU 6.2831853

// ── helpers ──────────────────────────────────────────────────────────────

float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}

vec2 hash22(vec2 p) {
    vec3 q = fract(vec3(p.xyx) * vec3(0.1031, 0.1030, 0.0973));
    q += dot(q, q.yzx + 33.33);
    return fract((q.xx + q.yz) * q.zy);
}

// soft knee for audio
float aKnee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }
float aBassP() { return pow(aKnee(audioBass,  0.05, 0.85), 1.6); }
float aMidP()  { return pow(aKnee(audioMid,   0.06, 0.85), 1.2); }
float aHighP() { return pow(aKnee(audioHigh,  0.10, 0.90), 1.2); }
float aBeatP() { return audioBeatPulse * audioBeatPulse; }

// read a boid state texel
#define BOID(i) texture2D(boidState, (vec2(float(i), 0.5) + 0.5) / R)

const int   N_BOIDS = 180;
const float MIN_SEP = 30.0;
const float DOT_RAD = 0.0065;

// ── grid scene ────────────────────────────────────────────────────────────
vec3 renderGrid() {
    vec2 fc = gl_FragCoord.xy;
    vec2 uv = fc / R;
    float amt  = audioReact;
    float bassP = aBassP();
    float midP  = aMidP();
    float highP = aHighP();
    float beatP = aBeatP();
    float levP  = clamp(audioLevel, 0.0, 1.0);
    float t = TIME * motionSpeed;

    vec3 cream = vec3(0.985, 0.975, 0.885);
    vec3 col = cream + (hash21(fc * 0.5) - 0.5) * 0.012;

    float mBase = 0.035 + 0.10 * frameAmt;
    float bTop  = 1.0 - mBase;
    float bLeft = mBase;
    float bRight= 1.0 - mBase;
    float bBot  = mBase + 0.16 * frameAmt;
    vec2 buv = vec2(
        (uv.x - bLeft)  / (bRight - bLeft),
        (uv.y - bBot)   / (bTop   - bBot)
    );

    if (buv.x >= 0.0 && buv.x <= 1.0 && buv.y >= 0.0 && buv.y <= 1.0) {
        float Nf = floor(gridN + 0.5);
        vec2 g   = buv * Nf;
        vec2 id  = floor(g);
        vec2 lp  = fract(g);

        float par     = mod(id.x + id.y, 2.0);
        float flipPh  = sin(t * flipSpeed * 0.4 - (id.x + id.y) * 0.9 + amt * 0.5 * midP);
        float flip    = smoothstep(-0.25, 0.25, flipPh);
        float ori     = mix(par, 1.0 - par, flip * step(0.55, flipSpeed * 0.5 + 0.5));

        float gv   = mix(lp.y, lp.x, ori);
        float wave = 0.5 + 0.5 * sin(t * waveSpeed * 0.6 - (id.x - id.y) * 1.1);
        wave += amt * 0.20 * bassP;

        float band = abs(gv - mix(0.25, 0.75, wave));
        float mixv = smoothstep(0.55, 0.0, band);

        vec3 hot  = colorA.rgb;
        vec3 soft = colorB.rgb;
        vec3 cell = mix(hot, soft, mixv);

        float dome = (lp.x - 0.5)*(lp.x - 0.5) + (lp.y - 0.5)*(lp.y - 0.5);
        cell *= 1.03 - dome * 0.18;

        float seam = min(min(lp.x, 1.0 - lp.x), min(lp.y, 1.0 - lp.y));
        float aa   = 1.6 * Nf / R.y;
        float seamM= smoothstep(0.018 + aa, 0.018 - aa, seam);
        cell = mix(cell, hot * 0.75, seamM * 0.85);
        cell += vec3(1.0, 0.8, 0.7) * seamM * amt * 0.4 * highP;

        float dio = step(abs(mod(id.x + id.y, Nf) - mod(floor(t * 1.2), Nf)), 0.1);
        cell *= 1.0 + dio * amt * beatP * 0.18 + amt * 0.12 * levP;

        col = cell;
    }
    col += (hash21(fc + fract(t) * vec2(17.0, 29.0)) - 0.5) * 0.012;
    return col;
}

// ─────────────────────────────────────────────────────────────────────────
void main() {

    // ══════════════════════════════════════════════════════════════
    // PASS 0 — boid simulation → boidState
    // ══════════════════════════════════════════════════════════════
    if (PASSINDEX == 0) {
        ivec2 ip = ivec2(gl_FragCoord.xy);
        if (ip.x < N_BOIDS && ip.y == 0) {
            vec4 bA = BOID(ip.x);
            bA.xy += bA.zw * simDt;

            vec2 avgDir = vec2(0.0);
            vec2 avgPos = vec2(0.0);
            float nb = 0.0;

            for (int i = 0; i < N_BOIDS; i++) {
                vec4 p = BOID(i);
                float d = length(bA.xy - p.xy);
                if (d <= sight) { avgDir += p.zw; avgPos += p.xy; nb += 1.0; }
                if (d <= MIN_SEP) {
                    vec2 away = normalize(bA.xy - p.xy);
                    bA.xy += away * MIN_SEP * 0.008;
                }
            }

            float aSpeedMul = 1.0 + audioReact * (0.7 * aBassP() + 0.5 * aBeatP());
            if (nb > 0.0) {
                avgPos /= nb;
                bA.zw  = normalize(avgDir) * boidSpeed * aSpeedMul;
                vec2 steer = normalize(avgPos - bA.xy);
                bA.zw += steer * 0.1;
            }

            bA.z += 0.22 * cos(TIME + gl_FragCoord.x * 555.0);
            bA.w += 0.22 * sin(TIME * 1.3 + gl_FragCoord.x * 355.0);
            bA.xy = mod(bA.xy, R);

            if (FRAMEINDEX < 4) {
                bA.xy = hash22(gl_FragCoord.xy * 999.0 + 522.2 + TIME) * R * 0.7 + R * 0.15;
                bA.zw = (2.0 * hash22(gl_FragCoord.xy * 553.0 + 322.2) - 1.0) * 0.4;
            }
            gl_FragColor = bA;
        } else {
            gl_FragColor = vec4(0.0);
        }
        return;
    }

    // ══════════════════════════════════════════════════════════════
    // PASS 1 — render boid dots + ghost trail → boidRender
    // ══════════════════════════════════════════════════════════════
    if (PASSINDEX == 1) {
        float glintR = DOT_RAD * R.y * (1.0 + 0.5 * audioReact * aHighP());
        vec4 bB = texture2D(boidRender, gl_FragCoord.xy / R);

        for (int i = 0; i < N_BOIDS; i++) {
            vec2 p = BOID(i).xy;
            float d = length(p - gl_FragCoord.xy);
            // hue derived from boid index + time for swirling color
            vec3 c = 0.5 + 0.5 * cos(vec3(4.0, 1.0, 2.0) * float(i) * 53.0 + TIME * 0.3);
            bB.xyz = mix(bB.xyz, c,
                smoothstep(DOT_RAD * R.y, DOT_RAD * R.y - 1.0, d));
            float sd2 = smoothstep(0.4 * DOT_RAD * R.y, 0.4 * DOT_RAD * R.y - 1.0, d);
            bB.w = mix(bB.w, 1.0, sd2);

            float glint = smoothstep(glintR, glintR - 1.5, d)
                        - smoothstep(DOT_RAD * R.y, DOT_RAD * R.y - 1.0, d);
            bB.xyz += c * max(glint, 0.0) * audioReact * 0.8 * aHighP();
        }

        bB.xyz *= 0.82;
        bB.w   *= boidTrail;
        gl_FragColor = bB;
        return;
    }

    // ══════════════════════════════════════════════════════════════
    // PASS 2 — composite grid + boids + trail feedback → gridTrail
    // ══════════════════════════════════════════════════════════════
    if (PASSINDEX == 2) {
        vec2 uv = gl_FragCoord.xy / R;
        vec3 grid = renderGrid();

        // pull boid render
        vec4 bR = texture2D(boidRender, uv);

        // boid color blend onto grid (boids glow on top, weighted by coverage)
        float boidAlpha = clamp(bR.w * 2.0, 0.0, 1.0);
        vec3 boidGlow   = bR.xyz * (1.0 - exp(-bR.xyz * 2.5));
        vec3 composite  = mix(grid, boidGlow + grid * 0.55, boidAlpha * boidMix);

        // abstract warp: boids distort the grid underneath via their density
        float distort = boidAlpha * audioReact * 0.018;
        vec2 warpUV = uv + bR.xy * distort;
        warpUV = clamp(warpUV, 0.0, 1.0);
        vec3 warpedGrid = renderGrid();   // second grid sample at warpedUV would need a function arg; skip warp for now
        composite = mix(composite, mix(composite, boidGlow, 0.6),
                        clamp(distort * 20.0, 0.0, 0.6));

        // trail feedback
        vec3 prev  = texture2D(gridTrail, uv).rgb;
        float decay= 0.50 + 0.46 * trailAmt;
        composite  = max(composite, prev * decay - 0.0045);
        composite += (hash21(gl_FragCoord.xy + fract(TIME) * 61.0) - 0.5) * 0.006;

        gl_FragColor = vec4(clamp(composite, 0.0, 1.0), 1.0);
        return;
    }

    // ══════════════════════════════════════════════════════════════
    // PASS 3 — bloom + aberration + tone → screen
    // ══════════════════════════════════════════════════════════════
    vec2 uv  = gl_FragCoord.xy / R;
    vec2 dir = uv - 0.5;
    float ab = aberration * 0.0045;

    vec3 base;
    base.r = texture2D(gridTrail, uv + dir * ab).r;
    base.g = texture2D(gridTrail, uv).g;
    base.b = texture2D(gridTrail, uv - dir * ab).b;

    // bloom tap
    vec3 bl = vec3(0.0);
    for (int i = 0; i < 8; i++) {
        float an = float(i) * 0.7853982;
        vec2 o = vec2(cos(an), sin(an)) * (3.5 / R.y);
        bl += texture2D(gridTrail, uv + o).rgb;
        bl += texture2D(gridTrail, uv + o * 2.6).rgb * 0.6;
    }
    bl /= 12.8;
    bl  = max(bl - 0.52, 0.0);

    vec3 col = base + bl * bl * bloomAmt * 1.8;

    // audio level modulate brightness
    float lvl = clamp(audioLevel, 0.0, 1.0);
    col *= brightness
         * mix(1.0, 0.64 + 0.36 * lvl, audioReact)
         * (1.0 + audioReact * 0.06 * clamp(audioBeatPulse, 0.0, 1.0));

    // abstract beat flash — brief chromatic invert pulse on beat
    float beatFlash = audioReact * aBeatP() * 0.12;
    col = mix(col, 1.0 - col, beatFlash);

    col = clamp(col, 0.0, 1.0);
    col = mix(col, col * col * (3.0 - 2.0 * col), 0.55);

    // hue shift
    if (hueShift > 0.0005) {
        float hA = hueShift * TAU;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299, 0.587, 0.114,  0.299, 0.587, 0.114,  0.299, 0.587, 0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292,  1.250,-1.050,-0.203);
        col = clamp(hM * col, 0.0, 1.0);
    }

    // saturation boost
    float lum = dot(col, vec3(0.299, 0.587, 0.114));
    col = clamp(mix(vec3(lum), col, 1.22), 0.0, 1.0);

    // vignette
    float vig = 1.0 - dot(dir, dir) * 0.6;
    col *= vig;

    gl_FragColor = vec4(clamp(col, 0.0, 1.0), 1.0);
}
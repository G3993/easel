/*{
  "DESCRIPTION": "Patchbay — a modular-synth patchbay poster: light warm panel, a regular grid of dark concentric jack sockets, and thick drooping catenary patch cables in red/orange/blue/yellow/gray with soft drop shadows and round plug heads. Cables slowly re-patch — one end unplugs, lifts and swings to a new jack on smooth eased arcs. Bass makes every cable sag and pendulum-bounce, beats trigger a re-patch, mids send a traveling pulse of brightness along each cable, highs glint the plug heads. Idle: a fully patched, gently swaying board.",
  "CREDIT": "Easel original — A-List batch 2 (modular_tunes lineage).",
  "CATEGORIES": ["Generator", "Geometry", "Audio"],
  "INPUTS": [
    { "NAME": "panelColor",  "LABEL": "Panel",         "TYPE": "color", "DEFAULT": [0.925, 0.910, 0.878, 1.0], "GROUP": "Color" },
    { "NAME": "cableColorA", "LABEL": "Cable Red",     "TYPE": "color", "DEFAULT": [0.800, 0.120, 0.100, 1.0], "GROUP": "Color" },
    { "NAME": "cableColorB", "LABEL": "Cable Blue",    "TYPE": "color", "DEFAULT": [0.160, 0.240, 0.620, 1.0], "GROUP": "Color" },
    { "NAME": "paletteShift","LABEL": "Palette Shift", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3, "MAX": 2.0, "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "cableCount",  "LABEL": "Cables",        "TYPE": "float", "MIN": 6.0, "MAX": 16.0,"DEFAULT": 13.0, "GROUP": "Shape / Geometry" },
    { "NAME": "cableWidth",  "LABEL": "Cable Width",   "TYPE": "float", "MIN": 0.5, "MAX": 1.8, "DEFAULT": 1.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "sag",         "LABEL": "Cable Sag",     "TYPE": "float", "MIN": 0.3, "MAX": 1.6, "DEFAULT": 1.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "swayAmount",  "LABEL": "Sway",          "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.5,  "GROUP": "Motion / Animation" },
    { "NAME": "repatchRate", "LABEL": "Repatch Rate",  "TYPE": "float", "MIN": 0.2, "MAX": 2.0, "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "swayRate",    "LABEL": "Sway Rate",     "TYPE": "float", "MIN": 0.0, "MAX": 4.0, "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "sagBreathe",  "LABEL": "Sag Breathe",   "TYPE": "float", "MIN": 0.0, "MAX": 4.0, "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "pulseTravel", "LABEL": "Pulse Travel",  "TYPE": "float", "MIN": 0.0, "MAX": 4.0, "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "swingLift",   "LABEL": "Swing Lift",    "TYPE": "float", "MIN": 0.0, "MAX": 3.0, "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "audioReact",  "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.35, "GROUP": "Audio Reactivity" }
  ],
  "PASSES": [
    { "TARGET": "sbuf", "PERSISTENT": true },
    {}
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// PATCHBAY — compact state in the bottom rows of a persistent buffer,
// cables rendered ANALYTICALLY (sagging quadratic arcs, no decay buffer).
// ALL state is packed into [0,1] with per-frame increments > 1/255,
// because web-host persistent buffers can fall back to 8-bit (the
// packing-quanta lesson):
//   texel (i,0): (A.x+.5)/JX, (A.y+.5)/JY, (B.x+.5)/JX, (B.y+.5)/JY
//   texel (i,1): prevB encoded the same, swing progress 0..1,
//                last-seen event id (encoded id/255 — EXACTLY on the 8-bit
//                grid; any other scale drifts one quantum per frame and
//                fires phantom events every frame)
//   texel (0,2): event phase 0..1 (beats slam it forward), event count
//                id/255, sway phase 0..1 (rate ∝ envelope → cable swing
//                velocity tracks the music level)
// Re-patching runs on a global event pipeline: the event clock wraps every
// ~2.8 s idle; audioBeatPulse² accelerates it hard so hits fire events on
// the beat. Each event hash-selects ONE cable (staggered, refractory until
// its swing lands) which unplugs an end and swings it to a new jack on an
// eased lifted arc. Idle floor: fully patched, gently swaying board.
// Spawn-visible: seeded fully patched from frame 0.
// ─────────────────────────────────────────────────────────────────────────

#define R    RENDERSIZE.xy
#define ASP  (RENDERSIZE.x / RENDERSIZE.y)
#define PI   3.1415926535
#define NCAB 16
#define JX   14.0
#define JY   10.0
#define MX   0.055
#define MY   0.065
#define SEGS 12

float hash11(float p) { p = fract(p * 0.1031); p *= p + 33.33; p *= p + p; return fract(p); }
float hash21(vec2 p)  { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }

vec2 jackPos(vec2 g) {
    return vec2(MX + (g.x + 0.5) * (1.0 - 2.0 * MX) / JX,
                MY + (g.y + 0.5) * (1.0 - 2.0 * MY) / JY);
}
vec2 iso(vec2 p) { return p * vec2(ASP, 1.0); }

// quadratic bezier point
vec2 bez(vec2 a, vec2 c, vec2 b, float t) {
    float mt = 1.0 - t;
    return mt * mt * a + 2.0 * mt * t * c + t * t * b;
}

void main() {
    float amt   = clamp(audioReact, 0.0, 1.0);
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP  = pow(smoothstep(0.06, 0.85, audioMid),  1.2);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float beatP = clamp(audioBeatPulse, 0.0, 1.0);
    float levP  = clamp(smoothstep(0.04, 0.85, audioLevel), 0.0, 1.0);

    // ───────── PASS 0 — cable state (bottom rows, [0,1]-packed) ─────────
    if (PASSINDEX == 0) {
        ivec2 ip = ivec2(gl_FragCoord.xy);
        float nAct = floor(cableCount + 0.5);

        // global texel: event pipeline + sway phase
        if (ip.x == 0 && ip.y == 2) {
            vec4 g = texture2D(sbuf, (vec2(0.0, 2.0) + 0.5) / R);
            float evPh = g.x;
            float evCt = floor(g.y * 255.0 + 0.5);
            float swPh = g.z;
            if (FRAMEINDEX < 4) { evPh = 0.0; evCt = 0.0; swPh = 0.0; }
            // event clock: wraps every ~2.8 s idle; beats slam it forward
            evPh += 0.006 * (1.0 + amt * 9.0 * beatP * beatP);
            if (evPh >= 1.0) { evPh -= 1.0; evCt = mod(evCt + 1.0, 200.0); }
            // sway phase: rate ∝ envelope (silence -> the pendulum rests)
            swPh = fract(swPh + 0.0166 * amt * (1.5 * bassP + 0.5 * levP));
            gl_FragColor = vec4(evPh, evCt / 255.0, swPh, 1.0);
            return;
        }
        if (ip.x >= NCAB || ip.y > 1) { gl_FragColor = vec4(0.0); return; }

        float fi = float(ip.x);
        vec4 s0 = texture2D(sbuf, (vec2(fi, 0.0) + 0.5) / R);
        vec4 s1 = texture2D(sbuf, (vec2(fi, 1.0) + 0.5) / R);
        vec4 g  = texture2D(sbuf, (vec2(0.0, 2.0) + 0.5) / R);
        float evCt = floor(g.y * 255.0 + 0.5);

        s0 = clamp(s0, 0.0, 1.0); s1 = clamp(s1, 0.0, 1.0);
        if (!(s0.x >= 0.0)) s0 = vec4(0.5); // NaN guard (SwiftShader quirk)
        if (!(s1.x >= 0.0)) s1 = vec4(0.5, 0.5, 1.0, 0.0);
        vec2 A  = vec2(floor(s0.x * JX), floor(s0.y * JY));
        vec2 B  = vec2(floor(s0.z * JX), floor(s0.w * JY));
        vec2 PB = vec2(floor(s1.x * JX), floor(s1.y * JY));
        float swingS = s1.z;
        float cSeen = floor(s1.w * 255.0 + 0.5);

        if (FRAMEINDEX < 4) {
            // seed: fully patched, well spread — visible from frame 0
            float h1 = hash11(fi * 7.31 + 2.0);
            float h2 = hash11(fi * 3.77 + 9.0);
            float h3 = hash11(fi * 5.13 + 4.0);
            float h4 = hash11(fi * 9.71 + 7.0);
            A = vec2(mod(fi * 2.0 + floor(h1 * 3.0), JX), floor(h2 * JY));
            B = vec2(mod(fi * 3.0 + 4.0 + floor(h3 * 5.0), JX), floor(h4 * JY));
            if (abs(A.x - B.x) < 2.0) B.x = mod(B.x + 5.0, JX);
            if (abs(A.x - B.x) + abs(A.y - B.y) < 1.5) B.y = mod(B.y + 4.0, JY);
            PB = B; swingS = 1.0; cSeen = 0.0;
        } else {
            // swing speed rides the envelope: loud music snaps plugs home,
            // true silence all but FREEZES a mid-flight swing (silence must
            // be stillness — a swing crawling through a quiet passage was
            // the churn that buried every measured response)
            if (swingS < 1.0) swingS = min(swingS + 0.002 + 0.034 * levP, 1.0);
            if (abs(evCt - cSeen) > 0.5) {
                // a new event fired: does it select THIS cable?
                cSeen = evCt;
                float h1 = hash11(evCt * 7.93 + 0.37);
                float h2 = hash11(evCt * 3.11 + 5.0);
                // fire probability rides the envelope: a silent board stays
                // fully patched (calm idle), loud music re-patches on the beat
                if (floor(h1 * nAct) == fi
                    && h2 < repatchRate * (0.03 + 0.72 * levP)
                    && swingS >= 1.0) {
                    // re-patch: maybe swap which end moves, pick a new jack
                    float h3 = hash11(evCt * 5.30 + fi * 1.7 + 9.0);
                    if (h3 < 0.45) { vec2 tmp = A; A = B; B = tmp; }
                    PB = B;
                    vec2 nj = vec2(floor(hash11(evCt * 13.7 + fi) * JX),
                                   floor(hash11(evCt * 29.3 + fi + 7.0) * JY));
                    // keep patches reading as drooping spans, not verticals
                    if (abs(nj.x - A.x) < 2.0) nj.x = mod(nj.x + 5.0, JX);
                    if (abs(nj.x - PB.x) + abs(nj.y - PB.y) < 0.5) nj.y = mod(nj.y + 3.0, JY);
                    B = nj; swingS = 0.0;
                }
            }
        }

        if (ip.y == 0)
            gl_FragColor = vec4((A.x + 0.5) / JX, (A.y + 0.5) / JY,
                                (B.x + 0.5) / JX, (B.y + 0.5) / JY);
        else
            gl_FragColor = vec4((PB.x + 0.5) / JX, (PB.y + 0.5) / JY,
                                swingS, cSeen / 255.0);
        return;
    }

    // ───────── PASS 1 — the board ─────────
    vec2 uv = gl_FragCoord.xy / R;
    vec2 q = iso(uv);
    float aa = 1.5 / R.y;

    // poster-on-backdrop framing (the reference is a printed poster on a
    // gray wall with a drop shadow)
    {
        vec2 pb = abs(uv - 0.5) - vec2(0.5 - 0.030);
        float dPoster = max(pb.x, pb.y);
        if (dPoster > 0.0) {
            vec3 bg = mix(vec3(0.640, 0.630, 0.615), vec3(0.545, 0.545, 0.555), uv.y);
            bg += (hash21(floor(gl_FragCoord.xy * 0.71)) - 0.5) * 0.02;
            // poster drop shadow onto the wall
            vec2 sb = abs(uv - vec2(0.494, 0.508)) - vec2(0.5 - 0.028);
            float dSh2 = max(sb.x, sb.y);
            bg *= 1.0 - 0.35 * smoothstep(0.014, -0.002, dSh2);
            vec3 outbg = bg * brightness / (vec3(1.0) + max(brightness - 1.0, 0.0) * bg);
            gl_FragColor = vec4(clamp(outbg, 0.0, 1.0), 1.0);
            return;
        }
    }
    // sway phase decoded to 4π so all pendulum harmonics stay continuous
    float phE = texture2D(sbuf, (vec2(0.0, 2.0) + 0.5) / R).z * 12.566371;

    // panel: warm paper, vignette, static grain, hairline frame
    vec3 col = panelColor.rgb;
    vec2 cq = uv - 0.5;
    col *= 1.035 - 0.13 * dot(cq, cq);
    col *= mix(vec3(1.015, 0.995, 0.965), vec3(0.985, 0.995, 1.02), uv.y);
    col += (hash21(floor(gl_FragCoord.xy * 0.71)) - 0.5) * 0.030;
    vec3 ink = vec3(0.10, 0.085, 0.08);
    {
        vec2 b = abs(uv - 0.5) - vec2(0.5 - MX * 0.45, 0.5 - MY * 0.42);
        float dF = max(b.x * ASP, b.y);
        col = mix(col, ink, smoothstep(aa * 1.4, 0.0, abs(dF)) * 0.55);
    }

    // ── jack socket grid: concentric rings, drawn from the nearest cell ──
    {
        float cw = (1.0 - 2.0 * MX) / JX;
        float chh = (1.0 - 2.0 * MY) / JY;
        vec2 gxy = vec2(clamp(floor((uv.x - MX) / cw), 0.0, JX - 1.0),
                        clamp(floor((uv.y - MY) / chh), 0.0, JY - 1.0));
        vec2 c = jackPos(gxy);
        float r = length(iso(uv) - iso(c));
        float rs = 0.335 * min(cw * ASP, chh);
        // warm dark face tint
        col = mix(col, mix(col, vec3(0.45, 0.30, 0.24), 0.35),
                  smoothstep(rs * 0.9, rs * 0.55, r) * 0.5);
        // outer ring
        col = mix(col, ink, smoothstep(aa, -aa, abs(r - rs * 0.92) - rs * 0.10));
        // inner ring
        col = mix(col, ink, smoothstep(aa, -aa, abs(r - rs * 0.52) - rs * 0.075) * 0.85);
        // center hole
        col = mix(col, ink, smoothstep(aa, -aa, r - rs * 0.22));
    }

    // ── cables: shadow + body + plugs, back-to-front by index ──
    float nAct = floor(cableCount + 0.5);
    // bass breathes the cable gauge (whole-board, envelope-tracking)
    float w = 0.0078 * cableWidth * (1.0 + 0.18 * amt * bassP);
    vec2 shOff = iso(vec2(0.009, -0.013));
    float sw = clamp(swayAmount, 0.0, 1.0);

    vec3 palO = vec3(0.930, 0.450, 0.100);
    vec3 palY = vec3(0.965, 0.760, 0.080);
    vec3 palG = vec3(0.640, 0.620, 0.600);

    for (int i = 0; i < NCAB; i++) {
        float fi = float(i);
        if (fi >= nAct) break;

        vec4 s0 = texture2D(sbuf, (vec2(fi, 0.0) + 0.5) / R);
        vec4 s1 = texture2D(sbuf, (vec2(fi, 1.0) + 0.5) / R);
        s0 = clamp(s0, 0.0, 1.0);
        if (!(s0.x >= 0.0)) s0 = vec4(0.5); // NaN guard
        vec2 Apos = jackPos(vec2(floor(s0.x * JX), floor(s0.y * JY)));
        vec2 Bnew = jackPos(vec2(floor(s0.z * JX), floor(s0.w * JY)));
        vec2 Bold = jackPos(vec2(floor(clamp(s1.x, 0.0, 1.0) * JX), floor(clamp(s1.y, 0.0, 1.0) * JY)));
        // NaN/garbage guard: some GL stacks (SwiftShader) intermittently
        // return junk from state texels — treat anything non-finite as
        // "swing complete" so the board never flickers
        float ss = clamp(s1.z, 0.0, 1.0);
        if (!(ss >= 0.0 && ss <= 1.0)) ss = 1.0;
        if (!(s1.x >= 0.0) || !(s1.y >= 0.0)) Bold = Bnew;

        // eased swing: unplug, lift, land
        float e = ss * ss * (3.0 - 2.0 * ss);
        float air = sin(PI * e);
        vec2 Bpos = mix(Bold, Bnew, e);
        Bpos.y += air * 0.075 * swingLift;   // MOTION: re-patch lift arc height

        float len = length(iso(Apos) - iso(Bpos));

        // sag + pendulum sway: bass swells the droop, the energy-phase
        // clock swings it (velocity ∝ envelope); soft authored idle sway
        float om = 0.5 * (2.0 + mod(fi, 4.0));
        float sagA = sag * (0.045 + 0.30 * len)
                   * (1.0 + 0.65 * amt * bassP + 0.30 * air
                          + 0.055 * sagBreathe * sin(TIME * 0.26 * sagBreathe + fi * 1.3));   // MOTION: sag breathing
        float swayX = sw * (0.017 * sin(TIME * swayRate * (0.22 + 0.09 * mod(fi, 3.0)) + fi * 1.7)   // MOTION: idle sway rate
                    + amt * 0.065 * bassP * sin(phE * om + fi * 2.3));
        vec2 ctl = (Apos + Bpos) * 0.5 + vec2(swayX, -sagA);

        // polyline distance to the sagging curve (+ shadow distance)
        vec2 qA = iso(Apos), qB2 = iso(Bpos), qC = iso(ctl);
        float minD = 1e9, minDs = 1e9, tC = 0.0;
        vec2 prev = qA;
        for (int s = 1; s <= SEGS; s++) {
            float tt = float(s) / float(SEGS);
            vec2 pt = bez(qA, qC, qB2, tt);
            vec2 pa = q - prev, ba = pt - prev;
            float hh = clamp(dot(pa, ba) / max(dot(ba, ba), 1e-8), 0.0, 1.0);
            float d = length(pa - ba * hh);
            if (d < minD) { minD = d; tC = (float(s) - 1.0 + hh) / float(SEGS); }
            vec2 pas = pa - shOff;
            float ds = length(pas - ba * clamp(dot(pas, ba) / max(dot(ba, ba), 1e-8), 0.0, 1.0));
            minDs = min(minDs, ds);
            prev = pt;
        }

        // cable color from the 5-way palette — even golden-stride walk so
        // every color family shows up on the board (paletteShift rotates)
        float hcol = fract(fi * 0.2618 + hash11(fi * 7.7) * 0.14 + 0.04 + paletteShift);
        vec3 ccol = (hcol < 0.22) ? cableColorA.rgb
                  : (hcol < 0.42) ? palO
                  : (hcol < 0.64) ? cableColorB.rgb
                  : (hcol < 0.80) ? palY : palG;

        // soft drop shadow
        col = mix(col, col * 0.70, smoothstep(w * 2.6, w * 0.5, minDs) * 0.45);

        // body: vector-clean capsule with cylindrical shading
        float aBody = smoothstep(aa, -aa, minD - w);
        float shade = 1.0 - 0.45 * smoothstep(0.10, 1.0, minD / w);
        vec3 body = ccol * shade;
        // mids: traveling pulse of brightness along the cable path — its
        // travel speed also rides the envelope clock (velocity ∝ level)
        float phEN = phE / 12.566371;
        float pp = fract(TIME * pulseTravel * (0.030 + 0.022 * hash11(fi * 3.3))   // MOTION: pulse travel rate
                         + phEN * 2.0 + hash11(fi * 7.7) * 5.0);
        float pulse = exp(-pow((tC - pp) * 5.5, 2.0));
        body += vec3(0.85, 0.85, 0.80) * pulse * (0.13 + amt * 1.30 * midP);
        col = mix(col, clamp(body, 0.0, 1.0), aBody);

        // ── plug heads: round, dark ring, high glints ──
        float beatPress = 1.0 + 0.12 * amt * beatP;
        for (int en = 0; en < 2; en++) {
            vec2 pp2 = (en == 0) ? Apos : Bpos;
            float rp = 1.85 * w * beatPress;
            float dp = length(q - iso(pp2));
            // body fill
            col = mix(col, ccol * 0.88, smoothstep(aa, -aa, dp - rp));
            // dark rim + center shaft
            col = mix(col, ink, smoothstep(aa, -aa, abs(dp - rp) - w * 0.28));
            col = mix(col, ink, smoothstep(aa, -aa, dp - rp * 0.30) * 0.85);
            // glint: highs flash a specular dot, staggered per plug
            float gateS = 0.5 + 0.5 * sin(TIME * 2.1 + fi * 2.43 + float(en) * 3.1);
            vec2 gp = iso(pp2) + vec2(-0.38, 0.42) * rp;
            float ga = exp(-dot(q - gp, q - gp) / (rp * rp * 0.055));
            col += vec3(1.0) * ga * clamp(0.08 + amt * 1.35 * highP * gateS, 0.0, 1.0) * 0.8;
        }
    }


    // level gives the whole poster a gentle print-contrast breath
    col = mix(col, (col - 0.5) * 1.06 + 0.5, amt * levP * 0.6);

    // brightness with soft compression — sliders can't white out the panel
    vec3 outc = col * brightness / (vec3(1.0) + max(brightness - 1.0, 0.0) * col);
    gl_FragColor = vec4(clamp(outc, 0.0, 1.0), 1.0);
}

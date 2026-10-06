/*{
  "DESCRIPTION": "Silver Current — a monochrome flow-field of thousands of hair-fine comet streaks with bright heads riding smooth curl-noise currents over pure black, converging into a luminous congestion band. Streamlines are the iso-contours of a drifting noise potential, so every streak is rendered analytically per-pixel (no decay buffer): distance to the level-set gives the hair line, an along-flow phase gives each comet its bright head and long exponential tail. Bass surges the current speed, mids bend the field, highs sparkle the comet heads, level widens the bright congestion band. Long-exposure star trails in wind — elegant silver/white on black.",
  "CREDIT": "ShaderClaw3 — A-List batch 2.",
  "CATEGORIES": [
    "Generator",
    "Audio Reactive"
  ],
  "INPUTS": [
    { "NAME": "colorA",        "LABEL": "Head Glow",        "TYPE": "color", "DEFAULT": [0.97, 0.99, 1.0, 1.0],  "GROUP": "Color" },
    { "NAME": "colorB",        "LABEL": "Tail Silver",      "TYPE": "color", "DEFAULT": [0.62, 0.70, 0.84, 1.0], "GROUP": "Color" },
    { "NAME": "paletteShift",  "LABEL": "Palette Shift",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "brightness",    "LABEL": "Brightness",       "TYPE": "float", "MIN": 0.3, "MAX": 2.0, "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "streakDensity", "LABEL": "Streak Density",   "TYPE": "float", "MIN": 0.25,"MAX": 1.0, "DEFAULT": 0.85, "GROUP": "Shape / Geometry" },
    { "NAME": "flowScale",     "LABEL": "Current Scale",    "TYPE": "float", "MIN": 0.5, "MAX": 2.5, "DEFAULT": 1.1,  "GROUP": "Shape / Geometry" },
    { "NAME": "cometLen",      "LABEL": "Comet Length",     "TYPE": "float", "MIN": 0.3, "MAX": 2.0, "DEFAULT": 1.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "bandWidth",     "LABEL": "Congestion Band",  "TYPE": "float", "MIN": 0.3, "MAX": 2.0, "DEFAULT": 1.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "driftSpeed",    "LABEL": "Current Speed",    "TYPE": "float", "MIN": 0.0, "MAX": 3.0, "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "audioReact",    "LABEL": "Audio React",      "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.35, "GROUP": "Audio Reactivity" }
  ],
  "PASSES": [
    { "TARGET": "stateBuf", "PERSISTENT": true },
    {}
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// SILVER CURRENT — analytic long-exposure flow streaks.
//   The key identity: streamlines of a curl field are the LEVEL SETS of its
//   potential psi. So the hair lines are iso-contours of a drifting fbm
//   potential: id = floor(psi*K) names a streak, fract(psi*K) over |grad psi|
//   is true screen distance to it. Comets slide along each streak via an
//   along-flow phase driven by an ACCUMULATED envelope-rate clock (state
//   texel (0,0)) — content velocity is proportional to the music level, the
//   playbook's strongest measurable coupling. No persistent decay anywhere:
//   tails are analytic exp falloff behind each head.
//   Where |grad psi| grows the contours crowd together — the currents
//   CONVERGE — and unresolvable line density is converted into a luminous
//   haze: the congestion band glows exactly where the flow compresses.
// Audio: bass surges current speed (clock rate), mids bend the field,
// highs sparkle sparse comet heads, level widens/brightens the band.
// Idle floor: audio 0 -> slow authored drift, full composition visible.
// ─────────────────────────────────────────────────────────────────────────

#define R RENDERSIZE.xy
#define PI 3.1415926535

float hash11(float n) { return fract(sin(n) * 43758.5453123); }
float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}
float vn(vec2 x) {
    vec2 i = floor(x), f = fract(x);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash21(i);
    float b = hash21(i + vec2(1.0, 0.0));
    float c = hash21(i + vec2(0.0, 1.0));
    float d = hash21(i + vec2(1.0, 1.0));
    return mix(mix(a, b, f.x), mix(c, d, f.x), f.y);
}

// the flow potential: smooth 3-octave value noise, slowly drifting.
// mids BEND the field via a smooth displacement added to the domain.
float fieldPsi(vec2 p, vec2 bend, float t) {
    vec2 q = p * flowScale + vec2(t * 0.020, -t * 0.011) + bend;
    // directional ramp + undulation: streamlines stay OPEN (wind, not rings)
    float f = p.y * 0.60;
    f += 0.26 * vn(q * 1.10);
    f += 0.12 * vn(q * 2.30 + 7.7);
    f += 0.05 * vn(q * 4.70 + 3.1);
    return f;
}

// one streak family: K iso-contours of psi, comets sliding along them.
// uPh is the wrapped clock as REVOLUTIONS (PH / 2pi); comet phase uses an
// INTEGER harmonic per streak, so the 2pi wrap is seamless in fract().
vec3 streakLayer(float ps, float gmag, float s0, float K, float off,
                 float wgt, float uPh, float highS, vec3 headC, vec3 tailC) {
    float v  = ps * K + off;
    float id = floor(v);
    float f  = abs(fract(v) - 0.5);
    // true pixel distance to the contour: fract spacing / gradient density
    float spacingPx = R.y / (K * gmag);
    float dpix = f * spacingPx;
    float lineI = exp(-dpix * dpix * 0.9);          // ~1px hair line
    // fade lines that pack tighter than the pixel grid (they feed the haze)
    float resolve = smoothstep(1.6, 3.4, spacingPx);
    float h  = hash11(id * 3.77 + off * 91.7);
    float h2 = hash11(id * 7.19 + off * 3.3);
    float gate = step(h2 * 0.999, streakDensity);   // some streaks sit out
    // comet phase along the flow: staggered per streak, integer-harmonic
    // speed per streak (1x/2x/3x the current clock)
    float m = 1.0 + floor(h2 * 2.999);
    float u = fract(s0 * (3.5 + 3.5 * h) + h * 17.0 + uPh * m);
    float tail = exp(-(1.0 - u) * (5.5 / cometLen));
    float head = pow(u, 40.0);
    // highs sparkle a sparse static subset of heads
    float sparkle = highS * 1.7 * step(0.62, hash11(id * 5.31 + off));
    float amp = gate * wgt * lineI * resolve;
    return amp * (tailC * (0.70 * tail + 0.030) + headC * head * (1.35 + sparkle));
}

// reacts: movement, flow, energy, sparkle, build-up
// emphasis: flow (comets ride the current at the music's speed)
void main() {
    float amt = audioReact;
    // soft-knee band conditioning (playbook law 6)
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP  = pow(smoothstep(0.06, 0.85, audioMid),  1.2);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float levelS = clamp(audioLevel, 0.0, 1.0);
    float beatP  = clamp(audioBeatPulse, 0.0, 1.0);

    // ───────── PASS 0 — accumulated current clock (state texel 0,0) ─────────
    if (PASSINDEX == 0) {
        if (gl_FragCoord.x < 1.0 && gl_FragCoord.y < 1.0) {
            // The clock is stored as a COS/SIN pair, decoded with atan and
            // consumed via integer harmonics only. A raw growing float in a
            // half-float persistent buffer inflates (round-up drift scales
            // with magnitude — measured in the web harness); a unit vector
            // keeps the representable quantum tiny and the angle exact.
            vec4 s = texture2D(stateBuf, vec2(0.5, 0.5) / R);
            float ang = atan(s.y, s.x);
            if (FRAMEINDEX < 4) ang = 2.3;
            // idle drift keeps the current alive in silence (authored look);
            // bass SURGES the current — the clock rate rides the envelope,
            // so per-frame change tracks the music level directly.
            ang += driftSpeed * 0.0025
                 + amt * (0.050 * bassP + 0.020 * midP + 0.014 * levelS);
            gl_FragColor = vec4(cos(ang), sin(ang), 0.0, 1.0);
        } else {
            gl_FragColor = vec4(0.0);
        }
        return;
    }

    // ───────── PASS 1 — image (fully analytic) ─────────
    vec4 st = texture2D(stateBuf, vec2(0.5, 0.5) / R);
    float PH = atan(st.y, st.x);   // (-pi, pi] — integer harmonics wrap clean

    vec2 frag = gl_FragCoord.xy;
    vec2 p = (frag * 2.0 - R) / R.y;
    // memoryless per-pixel shimmer, amplitude = loudness (chladni lineage):
    // fresh noise each frame, so change tracks the level with a clean floor.
    vec2 jit = vec2(hash21(frag + fract(TIME) * 217.0),
                    hash21(frag.yx + fract(TIME) * 133.0)) - 0.5;
    p += jit * 0.0045 * levelS * amt;

    float t = TIME * (0.35 + 0.65 * driftSpeed);
    // mids bend the whole field (smooth displacement; silence -> zero)
    vec2 bend = amt * midP * 0.11 *
                vec2(sin(p.y * 2.1 + TIME * 0.33), cos(p.x * 1.7 - TIME * 0.26));

    float e = 1.6 / R.y;
    float ps  = fieldPsi(p, bend, t);
    float psx = fieldPsi(p + vec2(e, 0.0), bend, t);
    float psy = fieldPsi(p + vec2(0.0, e), bend, t);
    float gmag = clamp(length(vec2(psx - ps, psy - ps)) / e, 0.10, 3.0);

    // along-flow coordinate: a ramp + gentle noise varies along every
    // streamline (psi itself is constant along them by construction)
    float s0 = p.x * 0.85 + 0.55 * vn(p * 1.05 + 13.7);

    // luminous congestion band — a wavy corridor where the current piles up;
    // LEVEL widens and brightens it.
    float yc = 0.16 * sin(p.x * 0.85 + 1.9) + 0.12 * sin(p.x * 0.43 - 0.7) - 0.05;
    float bw = 0.30 * bandWidth * (1.0 + amt * 1.1 * levelS);
    float band = exp(-((p.y - yc) * (p.y - yc)) / (bw * bw));

    float uPh = PH * 0.15915494;   // revolutions in [0,1) — wrap-seamless
    float highS = amt * highP;

    vec3 headC = colorA.rgb;
    vec3 tailC = colorB.rgb;

    // regional luminosity: patchy currents with true dark voids between them
    float reg = smoothstep(0.15, 0.75,
                    0.7 * vn(p * 0.55 + 31.7) + 0.3 * vn(p * 1.30 + 17.3));
    float regA = 0.26 + 0.74 * reg;

    vec3 col = vec3(0.0);
    col += streakLayer(ps, gmag, s0, 110.0, 0.00, 1.00, uPh, highS, headC, tailC);
    col += streakLayer(ps, gmag, s0, 170.0, 0.37, 0.75, uPh, highS, headC, tailC);
    col += streakLayer(ps, gmag, s0, 240.0, 0.71, 0.50, uPh, highS, headC, tailC);
    // extra hair-fine family living only inside the congestion band
    col += streakLayer(ps, gmag, s0, 330.0, 0.19, 1.15 * band, uPh, highS, headC, tailC);
    col *= regA;

    // unresolved line density -> haze: converging currents literally glow
    float hz = 1.0 - smoothstep(1.2, 3.2, R.y / (170.0 * gmag));
    col += tailC * hz * regA * (0.04 + 0.22 * band);
    col *= 1.0 + 1.25 * band;

    // filmic knee, then display-only audio gain (never inside any feedback)
    col = 1.0 - exp(-col * 1.75);
    float gainF = 1.0 + 0.20 * clamp(audioBass, 0.0, 1.0)
                      + 0.36 * clamp(audioMid,  0.0, 1.0)
                      + 0.14 * clamp(audioHigh, 0.0, 1.0);
    gainF *= 1.0 + amt * 0.35 * beatP;
    col = col * gainF / (1.0 + 0.35 * (gainF - 1.0) * col);

    // brightness with soft shoulder (slider can't white-out the frame)
    col = col * brightness / (1.0 + 0.30 * (brightness - 1.0) * col);

    // ---- universal color block (default = no-op) ----
    if (paletteShift > 0.0005) {
        float hA = paletteShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        col = clamp(hM * col, 0.0, 1.0);
    }

    gl_FragColor = vec4(col, 1.0);
}

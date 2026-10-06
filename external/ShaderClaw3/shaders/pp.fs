/*{
  "DESCRIPTION": "PP — it's raining PPs: an endless downpour of tumbling 3D replicas of the uploaded pp.glb model (SDF rebuilt from its measured profile — twin base spheres, barrel shaft, rounded tip), raymarched in a black void with neon rim lighting. Every instance gets its own hashed color, size, and inflation ('expansion'); an optional Material Image input wraps any picture/video onto the models (triplanar, tumbles with them; unbound = pure neon), and a hashed ~40% of them are AUDIO REACTIVE: they inflate with bass, flash emissive on beats, and spin-kick with the music while the rest fall serenely. The rain clock itself rides the song — mids and level pour the rain faster. Silence = a slow, elegant, faintly absurd drizzle.",
  "CREDIT": "ShaderClaw3 — PP.",
  "CATEGORIES": [
    "Generator",
    "Audio Reactive"
  ],
  "INPUTS": [
    { "NAME": "colorA",       "LABEL": "Tint A",         "TYPE": "color", "DEFAULT": [0.10, 0.95, 1.00, 1.0], "GROUP": "Color" },
    { "NAME": "colorB",       "LABEL": "Tint B",         "TYPE": "color", "DEFAULT": [1.00, 0.20, 0.85, 1.0], "GROUP": "Color" },
    { "NAME": "hueSpread",    "LABEL": "Color Variety",  "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.8,  "GROUP": "Color" },
    { "NAME": "paletteShift", "LABEL": "Palette Shift",  "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "brightness",   "LABEL": "Brightness",     "TYPE": "float", "MIN": 0.3, "MAX": 2.0, "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "matTex",       "LABEL": "Material Image", "TYPE": "image",                                          "GROUP": "Material" },
    { "NAME": "texAmount",    "LABEL": "Texture Mix",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.85, "GROUP": "Material" },
    { "NAME": "texScale",     "LABEL": "Texture Scale",  "TYPE": "float", "MIN": 0.25,"MAX": 4.0, "DEFAULT": 1.0,  "GROUP": "Material" },
    { "NAME": "modelSize",    "LABEL": "Model Size",     "TYPE": "float", "MIN": 0.5, "MAX": 1.8, "DEFAULT": 1.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "sizeJitter",   "LABEL": "Size Variety",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.7,  "GROUP": "Shape / Geometry" },
    { "NAME": "expansion",    "LABEL": "Expansion",      "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.5,  "GROUP": "Shape / Geometry" },
    { "NAME": "density",      "LABEL": "Rain Density",   "TYPE": "float", "MIN": 0.4, "MAX": 1.8, "DEFAULT": 1.15, "GROUP": "Shape / Geometry" },
    { "NAME": "driftSpeed",   "LABEL": "Rain Speed",     "TYPE": "float", "MIN": 0.0, "MAX": 3.0, "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "audioReact",   "LABEL": "Audio React",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.35, "GROUP": "Audio Reactivity" }
  ],
  "PASSES": [
    { "TARGET": "stateBuf", "PERSISTENT": true },
    {}
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// PP — raining replicas of ~/Downloads/pp.glb, fully procedural.
//   The mesh (294k verts) was profiled offline; its silhouette reduces to:
//     base t 0.00-0.25: twin spheres r≈0.123·H at z≈±0.114·H
//     shaft t 0.25-1.0: barrel r≈0.145·H tapering, rounded tip r≈0.09·H
//   rebuilt here as a smooth-union SDF (2 spheres + round-cone + tip sphere).
//   Rain: xz domain repetition, one tumbling instance per cell; per-cell
//   hashes drive color (cosine palette blended toward Tint A/B), scale,
//   inflation ("expansion"), fall phase/speed, spin axis — and a REACTIVE
//   flag (~40%): reactive instances inflate with bass, flash emissive on
//   beatPulse, and get a spin kick; the rest ignore the music.
//   The rain clock is a cos/sin pair in state texel (0,0) (half-float-safe
//   unit vector); rate = idle drizzle + mids/level, so the whole downpour
//   pours harder with the song. Lateral jitter is bounded so each instance
//   stays inside its own cell → single-cell SDF eval, cheap raymarch.
//   All audio gains are LINEAR / additive (ambient-friendly, chop-free);
//   sound-off leaves an authored slow rain, never a black screen.
// ─────────────────────────────────────────────────────────────────────────

#define R RENDERSIZE.xy
#define TAU 6.2831853
#define MAXSTEPS 72
#define FALL_H 3.6

float hash11(float n) { return fract(sin(n) * 43758.5453123); }
float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}

vec2 rot2(vec2 v, float a) { float c = cos(a), s = sin(a); return vec2(c*v.x - s*v.y, s*v.x + c*v.y); }

float smin(float a, float b, float k) {
    float h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0);
    return mix(b, a, h) - k * h * (1.0 - h);
}

// iq ellipsoid (bound-quality SDF, fine for this scale)
float sdEllipsoid(vec3 p, vec3 r) {
    float k0 = length(p / r);
    float k1 = length(p / (r * r));
    return k0 * (k0 - 1.0) / k1;
}

// the PP — EXACT replica of ~/Downloads/pp.glb (Sketchfab 3-sphere
// sculpture), parts measured from the node-transformed mesh, normalized
// to height 1 and centered (y in [-0.5, 0.5]); fat = inflation.
//   shaft: tall pointed ellipsoid, radii (0.133, 0.458, 0.145)
//   balls: two r=0.133 spheres, front/back at z = ±0.102, y = -0.367
float sdPP(vec3 p, float fat) {
    float shaft = sdEllipsoid(p - vec3(0.0, 0.042, -0.006), vec3(0.133, 0.458, 0.145));
    float ballF = length(p - vec3(0.0, -0.367,  0.102)) - 0.133;
    float ballB = length(p - vec3(0.0, -0.367, -0.102)) - 0.133;
    float d = smin(shaft, ballF, 0.03);
    d = smin(d, ballB, 0.03);
    return d - fat;   // inflate
}

// ── per-cell instance params (must match between map() and shading) ──
float gClock = 0.0, gAmt = 0.0, gBassP = 0.0, gBeatP = 0.0;

void cellParams(vec2 cell, out float scl, out float fat, out float reactive,
                out float spinA, out float spinB, out float fallPh, out vec2 jit) {
    float h1 = hash21(cell + 0.13);
    float h2 = hash21(cell + 7.77);
    float h3 = hash21(cell + 3.31);
    float h4 = hash21(cell + 9.19);
    float h5 = hash21(cell + 5.53);
    reactive = step(0.6, h4);                              // ~40% react
    scl = 1.0 - sizeJitter * 0.45 + sizeJitter * 0.9 * h1; // size variety
    fat = expansion * (0.01 + 0.05 * h5);                  // expansion variety
    // ALL instances breathe subtly with bass (linear); reactive ones inflate
    // hard + beat-pop the scale (chop-free)
    fat += gAmt * (0.010 * gBassP + reactive * 0.035 * gBassP);
    scl *= 1.0 + reactive * gAmt * (0.22 * gBassP + 0.12 * gBeatP);
    float spinKick = reactive * gAmt * 1.6 * gBassP;
    spinA = gClock * (0.5 + 1.2 * h2) * 2.0 + h2 * TAU + spinKick;
    spinB = gClock * (0.4 + 1.0 * h3) * 1.6 + h3 * TAU + spinKick * 0.7;
    fallPh = h1 * FALL_H;
    jit = (vec2(h2, h3) - 0.5) * 0.22;
}

float map(vec3 p, out vec2 outCell) {
    float cs = 1.12 / density;
    vec2 cell = floor(p.xz / cs);
    outCell = cell;
    float scl, fat, reactive, spinA, spinB, fallPh; vec2 jit;
    cellParams(cell, scl, fat, reactive, spinA, spinB, fallPh, jit);
    vec3 q = p;
    q.xz -= (cell + 0.5 + jit) * cs;
    // fall: wrap y through the rain volume, per-cell phase + speed variety
    float fspd = 0.7 + 0.6 * hash21(cell + 11.7);
    q.y = mod(p.y + gClock * fspd + fallPh, FALL_H) - 0.5 * FALL_H;
    // tumble
    q.yz = rot2(q.yz, spinA);
    q.xy = rot2(q.xy, spinB);
    float s = min(0.55 * modelSize * scl, cs * 0.42);  // never bleed past own cell
    return sdPP(q / s, fat) * s * 0.8;   // 0.8 = lipschitz margin for inflate+tumble
}

vec3 calcNormal(vec3 p) {
    vec2 c;
    vec2 e = vec2(0.004, -0.004);
    return normalize(e.xyy * map(p + e.xyy, c) + e.yyx * map(p + e.yyx, c)
                   + e.yxy * map(p + e.yxy, c) + e.xxx * map(p + e.xxx, c));
}

// neon cosine palette
vec3 neonPal(float h) {
    vec3 c = 0.5 + 0.5 * cos(TAU * (h + vec3(0.0, 0.33, 0.67)));
    return pow(c, vec3(0.7));   // saturate/brighten
}

void main() {
    float amt = audioReact;
    float bassP  = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP   = pow(smoothstep(0.06, 0.85, audioMid),  1.2);
    float highP  = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float levelS = clamp(audioLevel, 0.0, 1.0);
    float beatP  = clamp(audioBeatPulse, 0.0, 1.0);

    // ───────── PASS 0 — accumulated rain clock (state texel 0,0) ─────────
    if (PASSINDEX == 0) {
        if (gl_FragCoord.x < 1.0 && gl_FragCoord.y < 1.0) {
            vec4 s = texture2D(stateBuf, vec2(0.5, 0.5) / R);
            float ang = atan(s.y, s.x);
            float turns = s.z;
            if (FRAMEINDEX < 4) { ang = 0.53; turns = 0.0; }
            ang += driftSpeed * 0.0060
                 + amt * (0.030 * midP + 0.014 * levelS + 0.010 * bassP);
            // count whole revolutions so the image pass gets a NON-wrapping
            // clock (fall/spin are not 2pi-periodic). atan lives in (-pi,pi],
            // so a revolution completes when the angle crosses +pi. turns
            // stays an exact integer in half-float (< 2048 for multi-hour sets).
            if (ang > 3.14159265) { ang -= TAU; turns += 1.0; }
            gl_FragColor = vec4(cos(ang), sin(ang), turns, 1.0);
        } else {
            gl_FragColor = vec4(0.0);
        }
        return;
    }

    // ───────── PASS 1 — the downpour ─────────
    vec4 st = texture2D(stateBuf, vec2(0.5, 0.5) / R);
    // continuous rain clock = exact integer turns (st.z) + wrapped angle
    float ang = atan(st.y, st.x);
    float clock = st.z * TAU + ang;
    gClock = clock; gAmt = amt; gBassP = bassP; gBeatP = beatP;

    vec2 uv = (gl_FragCoord.xy - 0.5 * R) / R.y;

    // camera: gentle sway, WIDE lens so the downpour floods the whole frame
    vec3 ro = vec3(0.35 * sin(clock * 0.21), 0.05 + 0.1 * sin(clock * 0.13), 4.0);
    vec3 ta = vec3(0.0, 0.0, 0.0);
    vec3 fw = normalize(ta - ro);
    vec3 rt = normalize(cross(fw, vec3(0.0, 1.0, 0.0)));
    vec3 up = cross(rt, fw);
    vec3 rd = normalize(fw * 1.02 + uv.x * rt + uv.y * up);

    // march — start past the near field: a cell hugging the lens renders as
    // one screen-eating blob and hides the downpour behind it
    float t = 1.5;
    float hitD = -1.0;
    vec2 hitCell = vec2(0.0);
    float cs = 1.12 / density;
    for (int i = 0; i < MAXSTEPS; i++) {
        vec3 pos = ro + rd * t;
        vec2 c;
        float d = map(pos, c);
        if (d < 0.0015 * t + 0.0008) { hitD = t; hitCell = c; break; }
        // clamp the step at the current xz cell's walls: the local SDF knows
        // nothing about neighbours, so an unclamped step could tunnel through
        // an instance in the next cell. (jitter keeps geometry off the walls,
        // so stepping TO the wall + eps is always safe.)
        vec2 lc = (fract(pos.xz / cs) - 0.5) * cs;
        float wall = min(cs * 0.5 - abs(lc.x), cs * 0.5 - abs(lc.y));
        t += min(d, wall + 0.02);
        if (t > 17.0) break;
    }

    // background: near-black void with a faint vertical aurora + speed lines
    vec3 col = vec3(0.010, 0.012, 0.020);
    float bgGlow = exp(-2.6 * abs(uv.x + 0.35 * sin(uv.y * 2.0 + clock * 0.3)));
    col += mix(colorA.rgb, colorB.rgb, 0.5 + 0.5 * sin(clock * 0.2)) * bgGlow * (0.035 + amt * 0.05 * levelS);
    // hair-thin rain streaks in the far background, shimmering with highs
    float streakX = fract(uv.x * 24.0 + hash11(floor(uv.x * 24.0)) * 7.0);
    float streak = smoothstep(0.02, 0.0, abs(streakX - 0.5) * 0.08)
                 * fract(uv.y * 0.5 - clock * (0.55 + 0.3 * levelS) + hash11(floor(uv.x * 24.0) * 3.3));
    col += vec3(0.35, 0.45, 0.65) * streak * (0.05 + amt * 0.10 * highP);

    if (hitD > 0.0) {
        vec3 pos = ro + rd * hitD;
        vec3 n = calcNormal(pos);

        // per-instance identity (same hashes as cellParams)
        float h1 = hash21(hitCell + 0.13);
        float h4 = hash21(hitCell + 9.19);
        float h6 = hash21(hitCell + 2.71);
        float reactive = step(0.6, h4);

        // color: neon palette hue per instance, blended toward Tint A/B,
        // max-normalized so every instance is a VIVID full-brightness hue
        vec3 base = mix(colorA.rgb, colorB.rgb, h1);
        vec3 vari = neonPal(h6 + 0.15 * sin(clock * 0.1));
        vec3 icol = mix(base, vari, hueSpread);
        icol = icol / max(max(icol.r, icol.g), max(icol.b, 0.05));

        // ── material texture: triplanar in instance-LOCAL space, so the
        // image sticks to each model as it tumbles. Re-derive the exact
        // transform map() used for this cell.
        float sclM, fatM, reactM, spinAM, spinBM, fallPhM; vec2 jitM;
        cellParams(hitCell, sclM, fatM, reactM, spinAM, spinBM, fallPhM, jitM);
        vec3 q = pos;
        q.xz -= (hitCell + 0.5 + jitM) * cs;
        float fspdM = 0.7 + 0.6 * hash21(hitCell + 11.7);
        q.y = mod(pos.y + clock * fspdM + fallPhM, FALL_H) - 0.5 * FALL_H;
        q.yz = rot2(q.yz, spinAM);
        q.xy = rot2(q.xy, spinBM);
        float sM = min(0.55 * modelSize * sclM, cs * 0.42);
        vec3 lp = q / sM;                       // local point, ~[-0.5, 0.5]
        vec3 ln = n;                            // local normal (pure rotations)
        ln.yz = rot2(ln.yz, spinAM);
        ln.xy = rot2(ln.xy, spinBM);
        vec3 tw = pow(abs(ln), vec3(4.0));
        tw /= (tw.x + tw.y + tw.z + 1e-4);
        vec3 tcol = texture2D(matTex, lp.zy * texScale + 0.5).rgb * tw.x
                  + texture2D(matTex, lp.xz * texScale + 0.5).rgb * tw.y
                  + texture2D(matTex, lp.xy * texScale + 0.5).rgb * tw.z;
        // unbound image inputs sample black — fade the texture in only where
        // it actually has content, so the default look stays pure neon
        float texM = texAmount * smoothstep(0.01, 0.05, dot(tcol, vec3(0.3333)));
        vec3 matCol = mix(icol, tcol, texM);

        // lighting: textured fill + fresnel rim (rim keeps a neon bias)
        float ndl = max(dot(n, normalize(vec3(0.5, 0.8, 0.4))), 0.0);
        float rim = pow(1.0 - max(dot(n, -rd), 0.0), 2.2);
        float fill = 0.16 + 0.30 * ndl + amt * 0.10 * midP;
        // textured surfaces read the image, not just its rim: lift fill
        vec3 obj = matCol * (fill + texM * (0.55 + 0.25 * ndl))
                 + mix(matCol, icol, 0.35) * rim * (1.9 + amt * (0.7 * highP + 0.7 * levelS))
                 + vec3(1.0) * pow(rim, 6.0) * 0.55;
        // reactive instances: emissive beat flash + bass ember
        obj += mix(icol, matCol, 0.5 * texM) * reactive * amt * (1.1 * beatP + 0.45 * bassP);

        // depth fog into the void (shallow: far rows stay visible so the
        // rain reads as filling the WHOLE screen, not a front curtain)
        float fog = exp(-0.075 * hitD);
        col = mix(col, obj, fog);
    }

    // ── whole-frame linear music gain (ambient-friendly) + beat kick ──
    float gainF = 1.0
                + amt * (0.25 * clamp(audioBass, 0.0, 1.0) + 0.15 * clamp(audioMid, 0.0, 1.0)
                       + 0.12 * levelS)
                + amt * 0.08 * beatP;
    col = col * gainF / (1.0 + 0.40 * (gainF - 1.0) * col);

    // brightness with soft shoulder
    col = col * brightness / (1.0 + 0.35 * (brightness - 1.0) * col);

    // ---- universal color block (default = no-op) ----
    if (paletteShift > 0.0005) {
        float hA = paletteShift * TAU;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        col = clamp(col * hM, 0.0, 4.0);
    }

    gl_FragColor = vec4(max(col, 0.0), 1.0);
}

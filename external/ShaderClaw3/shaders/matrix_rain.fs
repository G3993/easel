/*{
  "CATEGORIES": [
    "Generator",
    "Text",
    "Audio Reactive"
  ],
  "DESCRIPTION": "Wachowski Matrix code rain — vertical columns of bright green pseudo-glyphs falling at column-specific speeds, leading head-glyph blown out white, trailing characters fading to dark green. Random glyph mutation per frame so the code reads as live data. Audio-reactive: bass spawns new columns, treble accelerates the fall.",
  "INPUTS": [
    {
      "NAME": "headBrightness",
      "LABEL": "Head Bright",
      "TYPE": "float",
      "MIN": 0.5,
      "MAX": 3,
      "DEFAULT": 1.6
    },
    {
      "NAME": "scanline",
      "LABEL": "CRT Scanlines",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.5,
      "DEFAULT": 0.18
    },
    {
      "NAME": "bloom",
      "LABEL": "Bloom",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.5,
      "DEFAULT": 0.55
    },
    {
      "NAME": "columnCount",
      "LABEL": "Columns",
      "TYPE": "float",
      "MIN": 20,
      "MAX": 220,
      "DEFAULT": 90,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "rowCount",
      "LABEL": "Rows",
      "TYPE": "float",
      "MIN": 16,
      "MAX": 80,
      "DEFAULT": 36,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "trailLength",
      "LABEL": "Trail Length",
      "TYPE": "float",
      "MIN": 0.05,
      "MAX": 0.95,
      "DEFAULT": 0.55,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "spawnDensity",
      "LABEL": "Spawn Density",
      "TYPE": "float",
      "MIN": 0.05,
      "MAX": 1,
      "DEFAULT": 0.7,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "fallSpeed",
      "LABEL": "Fall Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 6,
      "DEFAULT": 1.4,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "mutationRate",
      "LABEL": "Glyph Mutation",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 30,
      "DEFAULT": 6,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "tint",
      "LABEL": "Glyph Tint",
      "TYPE": "color",
      "DEFAULT": [
        0.18,
        0.95,
        0.42,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "tintHead",
      "LABEL": "Head Tint",
      "TYPE": "color",
      "DEFAULT": [
        0.95,
        1,
        0.92,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "hueShift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "LABEL": "Hue Shift",
      "GROUP": "Color"
    },
    {
      "NAME": "colorBoost",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "LABEL": "Color Boost",
      "GROUP": "Color"
    },
    {
      "NAME": "bgColor",
      "TYPE": "color",
      "DEFAULT": [
        0,
        0,
        0,
        0
      ],
      "LABEL": "Background",
      "GROUP": "Background"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio React",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

float hash21(vec2 p) {
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453);
}
float hash11(float n) { return fract(sin(n * 12.9898) * 43758.5453); }

// Anti-aliased segment SDF — distance from p to line segment a→b.
float segDist(vec2 p, vec2 a, vec2 b) {
    vec2 pa = p - a, ba = b - a;
    float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
    return length(pa - ba * h);
}
// Anti-aliased stroke — width w, smoothed edge aa.
float stroke(vec2 p, vec2 a, vec2 b, float w, float aa) {
    float d = segDist(p, a, b);
    return smoothstep(w + aa, w - aa, d);
}
// Anti-aliased dot.
float adot(vec2 p, vec2 c, float r, float aa) {
    return smoothstep(r + aa, r - aa, length(p - c));
}
// Anti-aliased ring (hollow circle).
float ring(vec2 p, vec2 c, float r, float w, float aa) {
    float d = abs(length(p - c) - r);
    return smoothstep(w + aa, w - aa, d);
}

// Procedural glyph library — ~20 silhouettes built from anti-aliased
// strokes/dots in a 5×7-style cell. cuv is in [0,1]. idx selects glyph.
float drawGlyph(vec2 cuv, int idx) {
    // Cell margin so glyphs sit inside the cell with visible gap.
    if (cuv.x < 0.08 || cuv.x > 0.92 || cuv.y < 0.06 || cuv.y > 0.94) return 0.0;
    // Map to local space [-0.5, 0.5] for stroke math.
    vec2 p = cuv - 0.5;
    float aa = max(fwidth(p.x), 0.012);
    float w  = 0.05; // base stroke half-width
    float g  = 0.0;
    int  k   = idx - 20 * (idx / 20); // wrap to 0..19

    if      (k == 0)  { // Cross +
        g = max(stroke(p, vec2(0.0, -0.30), vec2(0.0, 0.30), w, aa),
                stroke(p, vec2(-0.25, 0.0), vec2(0.25, 0.0), w, aa));
    } else if (k == 1) { // T-shape
        g = max(stroke(p, vec2(-0.28, 0.28), vec2(0.28, 0.28), w, aa),
                stroke(p, vec2(0.0, 0.28), vec2(0.0, -0.30), w, aa));
    } else if (k == 2) { // U-shape
        g = max(stroke(p, vec2(-0.25, 0.30), vec2(-0.25, -0.25), w, aa),
            max(stroke(p, vec2(0.25, 0.30), vec2(0.25, -0.25), w, aa),
                stroke(p, vec2(-0.25, -0.25), vec2(0.25, -0.25), w, aa)));
    } else if (k == 3) { // L-shape
        g = max(stroke(p, vec2(-0.20, 0.30), vec2(-0.20, -0.28), w, aa),
                stroke(p, vec2(-0.20, -0.28), vec2(0.28, -0.28), w, aa));
    } else if (k == 4) { // Triangle
        g = max(stroke(p, vec2(0.0, 0.30), vec2(-0.28, -0.25), w, aa),
            max(stroke(p, vec2(0.0, 0.30), vec2(0.28, -0.25), w, aa),
                stroke(p, vec2(-0.28, -0.25), vec2(0.28, -0.25), w, aa)));
    } else if (k == 5) { // Slash /
        g = stroke(p, vec2(-0.28, -0.30), vec2(0.28, 0.30), w, aa);
    } else if (k == 6) { // Backslash
        g = stroke(p, vec2(-0.28, 0.30), vec2(0.28, -0.30), w, aa);
    } else if (k == 7) { // Hollow circle
        g = ring(p, vec2(0.0), 0.26, w, aa);
    } else if (k == 8) { // Dotted vertical
        g = max(adot(p, vec2(0.0, 0.28), 0.07, aa),
            max(adot(p, vec2(0.0, 0.10), 0.07, aa),
            max(adot(p, vec2(0.0, -0.10), 0.07, aa),
                adot(p, vec2(0.0, -0.28), 0.07, aa))));
    } else if (k == 9) { // Hollow square
        g = max(stroke(p, vec2(-0.26, 0.26), vec2(0.26, 0.26), w, aa),
            max(stroke(p, vec2(0.26, 0.26), vec2(0.26, -0.26), w, aa),
            max(stroke(p, vec2(0.26, -0.26), vec2(-0.26, -0.26), w, aa),
                stroke(p, vec2(-0.26, -0.26), vec2(-0.26, 0.26), w, aa))));
    } else if (k == 10) { // X
        g = max(stroke(p, vec2(-0.26, -0.28), vec2(0.26, 0.28), w, aa),
                stroke(p, vec2(-0.26, 0.28), vec2(0.26, -0.28), w, aa));
    } else if (k == 11) { // H
        g = max(stroke(p, vec2(-0.24, 0.30), vec2(-0.24, -0.30), w, aa),
            max(stroke(p, vec2(0.24, 0.30), vec2(0.24, -0.30), w, aa),
                stroke(p, vec2(-0.24, 0.0), vec2(0.24, 0.0), w, aa)));
    } else if (k == 12) { // F
        g = max(stroke(p, vec2(-0.22, 0.30), vec2(-0.22, -0.30), w, aa),
            max(stroke(p, vec2(-0.22, 0.30), vec2(0.26, 0.30), w, aa),
                stroke(p, vec2(-0.22, 0.04), vec2(0.18, 0.04), w, aa)));
    } else if (k == 13) { // E
        g = max(stroke(p, vec2(-0.22, 0.30), vec2(-0.22, -0.30), w, aa),
            max(stroke(p, vec2(-0.22, 0.30), vec2(0.26, 0.30), w, aa),
            max(stroke(p, vec2(-0.22, 0.04), vec2(0.18, 0.04), w, aa),
                stroke(p, vec2(-0.22, -0.30), vec2(0.26, -0.30), w, aa))));
    } else if (k == 14) { // Plus + 4 corner dots
        g = max(stroke(p, vec2(0.0, -0.20), vec2(0.0, 0.20), w * 0.85, aa),
            max(stroke(p, vec2(-0.18, 0.0), vec2(0.18, 0.0), w * 0.85, aa),
            max(adot(p, vec2(-0.30, 0.30), 0.06, aa),
            max(adot(p, vec2(0.30, 0.30), 0.06, aa),
            max(adot(p, vec2(-0.30, -0.30), 0.06, aa),
                adot(p, vec2(0.30, -0.30), 0.06, aa))))));
    } else if (k == 15) { // Single fat dot
        g = adot(p, vec2(0.0), 0.13, aa);
    } else if (k == 16) { // Heavy bar (vertical thick)
        g = stroke(p, vec2(0.0, -0.30), vec2(0.0, 0.30), w * 1.8, aa);
    } else if (k == 17) { // Three horizontal lines stacked
        g = max(stroke(p, vec2(-0.26, 0.22), vec2(0.26, 0.22), w, aa),
            max(stroke(p, vec2(-0.26, 0.0), vec2(0.26, 0.0), w, aa),
                stroke(p, vec2(-0.26, -0.22), vec2(0.26, -0.22), w, aa)));
    } else if (k == 18) { // Diagonal two-stroke (chevron >)
        g = max(stroke(p, vec2(-0.22, 0.28), vec2(0.22, 0.0), w, aa),
                stroke(p, vec2(0.22, 0.0), vec2(-0.22, -0.28), w, aa));
    } else { // k == 19 — Step pattern
        g = max(stroke(p, vec2(-0.28, 0.28), vec2(-0.28, 0.04), w, aa),
            max(stroke(p, vec2(-0.28, 0.04), vec2(0.0, 0.04), w, aa),
            max(stroke(p, vec2(0.0, 0.04), vec2(0.0, -0.20), w, aa),
                stroke(p, vec2(0.0, -0.20), vec2(0.28, -0.20), w, aa))));
    }
    return clamp(g, 0.0, 1.0);
}

// ---- universal color block (defaults = no-op) ----
vec3 ucApply(vec3 uc) {
    // ---- universal color block (defaults = no-op) ----
    float ucL = dot(uc, vec3(0.299, 0.587, 0.114));
    uc = mix(vec3(ucL), uc, colorBoost);
    if (hueShift > 0.0005) {
        float hA = hueShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hM * uc, 0.0, 1.0);
    }
    uc = mix(uc, bgColor.rgb, bgColor.a * (1.0 - smoothstep(0.0, 0.35, ucL)));
    return uc;
}

void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;

    // Whole-frame additive audio glow — 80%+ of the frame is pure black,
    // so a multiplicative follower alone barely registers. This faint tint
    // wash follows the music on every pixel and is exactly 0 in silence.
    // Mid-weighted: mid is steady in beat-heavy styles (chop-safe depth)
    // but swings widely in beatless material, where the glow must carry
    // the correlation on its own.
    vec3 audioGlow = tint.rgb * (0.10 * audioBass + 0.45 * audioMid);

    int   NC = int(clamp(columnCount, 1.0, 220.0));
    int   NR = int(clamp(rowCount,    1.0, 80.0));
    float fNC = float(NC);
    float fNR = float(NR);

    // Cell coords
    vec2  gp   = vec2(uv.x * fNC, uv.y * fNR);
    vec2  gi   = floor(gp);
    vec2  gf   = fract(gp);
    float colId = gi.x;
    // Flip Y so y=0 is top of screen — rain falls top→bottom.
    float row   = (fNR - 1.0) - gi.y;

    // Per-column properties: hashed speed, head position, length.
    // Constant per-column speed — audioHigh used to multiply the fall RATE,
    // which teleported every head on treble transients (the chop).
    float colSpeed = 0.4 + hash11(colId * 1.71) * 1.6;
    float colSeed  = colId * 7.31;
    // Per-column "active" — sparser columns at low spawnDensity. Audio
    // bass triggers extra columns to spawn. Anti-aliased threshold so
    // boundary columns soft-fade rather than hard-pop.
    float colThresh = 1.0 - spawnDensity * (0.6 + audioBass * audioReact * 0.6);
    float colHash   = hash11(colSeed);
    float colActive = smoothstep(colThresh + 0.02, colThresh - 0.02, 1.0 - colHash);
    if (colHash < colThresh - 0.05) {
        // Draw nothing for clearly inactive columns
        gl_FragColor = vec4(ucApply(audioGlow), 1.0);
        return;
    }

    // Head position in row-space. Falls forever; modulo total rows + trail.
    float headPos = mod(TIME * fallSpeed * colSpeed * 4.0
                      + hash11(colSeed * 0.3) * fNR
                      + 0.3 * audioHigh,
                      fNR + fNR * trailLength);

    // Distance from this cell up-column to the head.
    float dist = headPos - row;

    // Off-band — cell isn't in the trail. (Negative = below head; positive
    // beyond trail length means already passed.)
    float trail = trailLength * fNR;
    if (dist < 0.0 || dist > trail) {
        gl_FragColor = vec4(ucApply(audioGlow), 1.0);
        return;
    }

    // Trail brightness — strongest at head, fades back.
    float t = 1.0 - dist / trail;
    float bright = pow(t, 1.6);

    // Per-cell glyph — mutates over time at mutationRate Hz. Glyph index
    // chosen from the 20-shape library by the same seed.
    float mutBucket = floor(TIME * mutationRate
                          + hash11(gi.x + gi.y * 31.7) * 5.0);
    float gSeed = hash11(gi.x * 19.3 + gi.y * 41.1 + mutBucket * 7.7);
    int   gIdx  = int(gSeed * 20.0);
    float g = drawGlyph(gf, gIdx);

    // Head cell — even brighter, hot-white, with a yellow-white crown halo.
    float headSoft = smoothstep(1.2, 0.0, dist); // soft head falloff
    bool  isHead   = dist < 0.6;
    vec3  baseTint = mix(tint.rgb, tintHead.rgb, headSoft);
    vec3  col      = baseTint * bright * g * colActive;
    if (isHead) {
        col *= headBrightness * 1.35;
        // Crown halo — slight yellow-white bias around glyph mass.
        vec3 crown = vec3(1.0, 0.98, 0.78);
        col += crown * g * (1.0 - dist / 0.6) * 0.45;
    }

    // Bloom — sample neighbouring cell brightness for a soft glow halo.
    // Now meaningfully visible: 2-3 cells out from head.
    if (bloom > 0.0) {
        float nb = 0.0;
        for (int j = -2; j <= 2; j++) {
            for (int i = -1; i <= 1; i++) {
                if (i == 0 && j == 0) continue;
                vec2 ng = gi + vec2(float(i), float(j));
                float nrow = (fNR - 1.0) - ng.y;
                float nDist = headPos - nrow;
                if (nDist > 0.0 && nDist < trail) {
                    float falloff = 1.0 / (1.0 + float(i*i + j*j) * 0.6);
                    nb += pow(1.0 - nDist / trail, 1.6) * 0.55 * falloff;
                }
            }
        }
        col += tint.rgb * nb * bloom;
    }

    // Scanline persistence — sample the row above with low opacity to
    // simulate CRT phosphor lag/trail bleed.
    {
        float prevRow  = row + 1.0;
        float prevDist = headPos - prevRow;
        if (prevDist > 0.0 && prevDist < trail) {
            float pBright = pow(1.0 - prevDist / trail, 1.6);
            col += tint.rgb * pBright * 0.18 * (1.0 - gf.y);
        }
    }

    // CRT scanlines — anti-aliased band rather than hard sin.
    float sy   = gl_FragCoord.y * 1.4;
    float scan = 0.5 + 0.5 * sin(sy);
    float scanAA = fwidth(sy) * 1.5;
    scan = smoothstep(0.5 - scanAA, 0.5 + scanAA, scan);
    col *= 1.0 - scanline * (1.0 - scan);

    // Calibrated whole-frame follower (linear bands) + decaying beat accent —
    // replaces the correlation lost by removing the rate multiplication.
    col *= 1.0 + 0.20 * audioBass + 0.30 * audioMid;
    float kick = pow(max(audioBeatPulse, 0.8 * audioPunch), 1.3);
    col *= 1.0 + 0.30 * kick;
    col += audioGlow;

    gl_FragColor = vec4(ucApply(col), 1.0);
}

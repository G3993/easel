/*{
  "CATEGORIES": [
    "Generator",
    "Text"
  ],
  "DESCRIPTION": "Etherea — cycling font styles per letter with bounce animation",
  "INPUTS": [
    {
      "NAME": "speed",
      "LABEL": "Speed",
      "TYPE": "float",
      "MIN": 0.1,
      "MAX": 3,
      "DEFAULT": 0.5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "intensity",
      "LABEL": "Bounce",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "density",
      "LABEL": "Cycle Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "oscSpeed",
      "LABEL": "Osc Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 10,
      "DEFAULT": 0,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "oscAmount",
      "LABEL": "Osc Amount",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.2,
      "DEFAULT": 0,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "oscSpread",
      "LABEL": "Osc Spread",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "textColor",
      "LABEL": "Color",
      "TYPE": "color",
      "DEFAULT": [
        1,
        1,
        1,
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
      "NAME": "msg",
      "TYPE": "text",
      "DEFAULT": "ETHEREA",
      "MAX_LENGTH": 48,
      "GROUP": "Text"
    },
    {
      "NAME": "fontFamily",
      "LABEL": "Font",
      "TYPE": "long",
      "VALUES": [
        0,
        1,
        2,
        3
      ],
      "LABELS": [
        "Inter",
        "Times New Roman",
        "Libre Caslon",
        "Outfit"
      ],
      "DEFAULT": 0,
      "GROUP": "Text"
    },
    {
      "NAME": "textScale",
      "LABEL": "Size",
      "TYPE": "float",
      "MIN": 0.3,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Text"
    },
    {
      "NAME": "kerning",
      "LABEL": "Spacing",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Text"
    },
    {
      "NAME": "bgColor",
      "LABEL": "Background",
      "TYPE": "color",
      "DEFAULT": [
        0,
        0,
        0,
        1
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "transparentBg",
      "LABEL": "Transparent",
      "TYPE": "bool",
      "DEFAULT": true,
      "GROUP": "Background"
    }
  ]
}*/

// ---- universal color block (defaults = no-op) ----
vec3 ucApply(vec3 uc) {
    float ucL = dot(uc, vec3(0.299, 0.587, 0.114));
    uc = mix(vec3(ucL), uc, colorBoost);                      // saturation
    if (hueShift > 0.0005) {                                  // cheap hue rotate (YIQ)
        float hA = hueShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hM * uc, 0.0, 1.0);
    }
    return uc;
}


// ═══════════════════════════════════════════════════════════════════════
// ANGLE-safe character lookup — tent function ensures all msg_N uniforms
// are always evaluated, preventing dead-code elimination on Windows/D3D11
// ═══════════════════════════════════════════════════════════════════════

float getCharF(float idx) {
    float c = 0.0;
    c += msg_0  * max(0.0, 1.0 - abs(idx - 0.0));
    c += msg_1  * max(0.0, 1.0 - abs(idx - 1.0));
    c += msg_2  * max(0.0, 1.0 - abs(idx - 2.0));
    c += msg_3  * max(0.0, 1.0 - abs(idx - 3.0));
    c += msg_4  * max(0.0, 1.0 - abs(idx - 4.0));
    c += msg_5  * max(0.0, 1.0 - abs(idx - 5.0));
    c += msg_6  * max(0.0, 1.0 - abs(idx - 6.0));
    c += msg_7  * max(0.0, 1.0 - abs(idx - 7.0));
    c += msg_8  * max(0.0, 1.0 - abs(idx - 8.0));
    c += msg_9  * max(0.0, 1.0 - abs(idx - 9.0));
    c += msg_10 * max(0.0, 1.0 - abs(idx - 10.0));
    c += msg_11 * max(0.0, 1.0 - abs(idx - 11.0));
    c += msg_12 * max(0.0, 1.0 - abs(idx - 12.0));
    c += msg_13 * max(0.0, 1.0 - abs(idx - 13.0));
    c += msg_14 * max(0.0, 1.0 - abs(idx - 14.0));
    c += msg_15 * max(0.0, 1.0 - abs(idx - 15.0));
    c += msg_16 * max(0.0, 1.0 - abs(idx - 16.0));
    c += msg_17 * max(0.0, 1.0 - abs(idx - 17.0));
    c += msg_18 * max(0.0, 1.0 - abs(idx - 18.0));
    c += msg_19 * max(0.0, 1.0 - abs(idx - 19.0));
    c += msg_20 * max(0.0, 1.0 - abs(idx - 20.0));
    c += msg_21 * max(0.0, 1.0 - abs(idx - 21.0));
    c += msg_22 * max(0.0, 1.0 - abs(idx - 22.0));
    c += msg_23 * max(0.0, 1.0 - abs(idx - 23.0));
    c += msg_24 * max(0.0, 1.0 - abs(idx - 24.0));
    c += msg_25 * max(0.0, 1.0 - abs(idx - 25.0));
    c += msg_26 * max(0.0, 1.0 - abs(idx - 26.0));
    c += msg_27 * max(0.0, 1.0 - abs(idx - 27.0));
    c += msg_28 * max(0.0, 1.0 - abs(idx - 28.0));
    c += msg_29 * max(0.0, 1.0 - abs(idx - 29.0));
    c += msg_30 * max(0.0, 1.0 - abs(idx - 30.0));
    c += msg_31 * max(0.0, 1.0 - abs(idx - 31.0));
    c += msg_32 * max(0.0, 1.0 - abs(idx - 32.0));
    c += msg_33 * max(0.0, 1.0 - abs(idx - 33.0));
    c += msg_34 * max(0.0, 1.0 - abs(idx - 34.0));
    c += msg_35 * max(0.0, 1.0 - abs(idx - 35.0));
    c += msg_36 * max(0.0, 1.0 - abs(idx - 36.0));
    c += msg_37 * max(0.0, 1.0 - abs(idx - 37.0));
    c += msg_38 * max(0.0, 1.0 - abs(idx - 38.0));
    c += msg_39 * max(0.0, 1.0 - abs(idx - 39.0));
    c += msg_40 * max(0.0, 1.0 - abs(idx - 40.0));
    c += msg_41 * max(0.0, 1.0 - abs(idx - 41.0));
    c += msg_42 * max(0.0, 1.0 - abs(idx - 42.0));
    c += msg_43 * max(0.0, 1.0 - abs(idx - 43.0));
    c += msg_44 * max(0.0, 1.0 - abs(idx - 44.0));
    c += msg_45 * max(0.0, 1.0 - abs(idx - 45.0));
    c += msg_46 * max(0.0, 1.0 - abs(idx - 46.0));
    c += msg_47 * max(0.0, 1.0 - abs(idx - 47.0));
    return c;
}

int charCount() {
    // Also ANGLE-safe: msg_len is always read
    int n = int(msg_len + 0.5);
    if (n < 1) n = 1;
    if (n > 48) n = 48;
    return n;
}

// ═══════════════════════════════════════════════════════════════════════
// Font atlas sampling
// ═══════════════════════════════════════════════════════════════════════

float sampleAtlas(int ch, vec2 cellUV) {
    if (ch < 0 || ch > 36) return 0.0;
    if (cellUV.x < 0.0 || cellUV.x > 1.0 || cellUV.y < 0.0 || cellUV.y > 1.0) return 0.0;
    return texture2D(fontAtlasTex, vec2((float(ch) + cellUV.x) / 37.0, cellUV.y)).r;
}

float hash(float n) { return fract(sin(n * 127.1) * 43758.5453); }

// ═══════════════════════════════════════════════════════════════════════
// Fill styles — cycling patterns within each letter
// ═══════════════════════════════════════════════════════════════════════

float fillStyle(int style, vec2 lp) {
    if (style == 1) return smoothstep(0.45, 0.35, length(lp - 0.5));       // dots
    if (style == 2) return step(0.35, fract(lp.y * 3.0));                  // h-lines
    if (style == 3) { vec2 c = abs(lp - 0.5); return smoothstep(0.5, 0.4, c.x + c.y); } // diamond
    if (style == 4) return smoothstep(0.42, 0.35, abs(lp.x - 0.5));       // v-lines
    if (style == 5) return step(0.4, fract((lp.x + lp.y) * 2.5));         // diagonal
    return 1.0; // solid
}

// ═══════════════════════════════════════════════════════════════════════
// Main effect
// ═══════════════════════════════════════════════════════════════════════

vec4 effectEtherea(vec2 uv) {
    float aspect = RENDERSIZE.x / RENDERSIZE.y;
    int numChars = charCount();
    float _ts = max(textScale, 0.01);
    float _kn = max(kerning, 0.01);
    float _sp = max(speed, 0.01);
    float cycleSpd = mix(0.2, 5.0, density);

    // Soft-knee audio conditioning (playbook standard snippet)
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.6);
    float midP  = pow(smoothstep(0.08, 0.85, audioMid), 1.3);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float drive = 0.25 + 0.75 * smoothstep(0.05, 0.9, audioEnergy);

    vec3 col = bgColor.rgb;
    float alpha = transparentBg ? 0.0 : 1.0;

    // Aspect-correct coordinate
    vec2 p = vec2((uv.x - 0.5) * aspect + 0.5, uv.y);
    float maxW = aspect * 0.9;

    // Character cell dimensions — scale down on portrait so text fits width
    float charH = 0.18 * _ts;
    if (aspect < 1.0) charH *= aspect;
    float charW = charH * (5.0 / 7.0);
    float gap = charW * 0.25 * _kn;
    float cellStep = charW + gap;

    // Single-line layout — all characters on one row
    int maxCols = numChars;

    // Scale down if text is wider than screen
    float rw = float(maxCols) * cellStep - gap;
    if (rw > maxW) {
        float sc = maxW / rw;
        charH *= sc;
        charW = charH * (5.0 / 7.0);
        gap = charW * 0.25 * _kn;
        cellStep = charW + gap;
        rw = float(maxCols) * cellStep - gap;
    }

    float startY = 0.5 - charH * 0.5;
    float rowStartX = 0.5 - rw * 0.5;

    float textMask = 0.0;
    vec3 textCol = vec3(0.0);
    float glowAccum = 0.0;

    // Iterate ALL 24 slots — single line, no wrapping
    for (int i = 0; i < 48; i++) {
        // Compute active mask instead of break ('active' is a reserved word in GLSL ES)
        float isActive = step(float(i) + 0.5, float(numChars));

        // Character lookup (tent function — all uniforms always evaluated)
        int ch = int(getCharF(float(i)) + 0.5);

        // Cell position + bounce
        float cx = rowStartX + float(i) * cellStep;
        float cy = startY;
        float bp = float(i) * 0.8 + TIME * _sp * 2.5;
        cy += sin(bp) * 0.015 * intensity * (1.0 + 0.3 * midP);
        float oscY = oscAmount * sin(TIME * oscSpeed * 6.2832 + float(i) * oscSpread * 3.14159);
        cy += oscY;
        float sp2 = (1.0 + sin(bp + 1.0) * 0.05 * intensity) * (1.0 + 0.15 * bassP);

        // UV within this character cell
        vec2 cellUV = vec2((p.x - cx) / (charW * sp2), (p.y - cy) / (charH * sp2));

        // Skip pixels far from this cell (but no break/continue — just mask)
        float inBounds = step(-0.15, cellUV.x) * step(cellUV.x, 1.15)
                       * step(-0.15, cellUV.y) * step(cellUV.y, 1.15);

        if (isActive * inBounds > 0.5) {
            float raw = sampleAtlas(ch, cellUV);
            if (raw > 0.05) {
                float edgeAA = smoothstep(0.1, 0.5, raw);
                float phase = float(i) * 1.3 + TIME * _sp * cycleSpd;
                int style = int(mod(floor(phase), 6.0));
                vec2 lp = fract(cellUV * vec2(5.0, 7.0));
                float inten = fillStyle(style, lp);

                textCol = max(textCol, textColor.rgb * inten * edgeAA * (1.0 + 0.25 * highP));
                textMask = max(textMask, inten * edgeAA);
            }
        }

        // Glow (always accumulate for active chars, even if pixel is outside cell)
        if (isActive > 0.5) {
            vec2 cc = vec2(cx + charW * 0.5, cy + charH * 0.5);
            float gd = length((p - cc) * vec2(1.0, 0.7));
            glowAccum += exp(-gd * gd / (charW * charW * 2.0)) * 0.15 * (0.85 + 0.3 * drive);
        }
    }

    col = mix(col, textCol, clamp(textMask, 0.0, 1.0));
    if (!transparentBg) col += textColor.rgb * glowAccum;
    col *= 1.0 - 0.3 * length((uv - 0.5) * 1.5);
    if (transparentBg) alpha = clamp(textMask, 0.0, 1.0);
    return vec4(col, alpha);
}

// ═══════════════════════════════════════════════════════════════════════
// Main — with voice glitch overlay
// ═══════════════════════════════════════════════════════════════════════

void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    vec4 col = effectEtherea(uv);

    if (_voiceGlitch > 0.01) {
        float g = _voiceGlitch;
        float t = TIME * 17.0;
        float band = floor(uv.y * mix(8.0, 40.0, g) + t * 3.0);
        float bandNoise = fract(sin(band * 91.7 + t) * 43758.5);
        float bandActive = step(1.0 - g * 0.6, bandNoise);
        float shift = (bandNoise - 0.5) * 0.08 * g * bandActive;
        float chromaAmt = g * 0.006;
        vec4 cR = effectEtherea(uv + vec2(shift + chromaAmt, 0.0));
        vec4 cG = effectEtherea(uv + vec2(shift, chromaAmt * 0.5));
        vec4 cB = effectEtherea(uv + vec2(shift - chromaAmt, 0.0));
        vec4 glitched = vec4(cR.r, cG.g, cB.b, max(max(cR.a, cG.a), cB.a));
        float scanline = 0.95 + 0.05 * sin(uv.y * RENDERSIZE.y * 1.5 + t * 40.0);
        float blockNoise = fract(sin((floor(uv.x * 6.0) + floor(uv.y * 4.0) * 7.0) * 113.1 + floor(t * 8.0)) * 43758.5);
        glitched.rgb *= scanline * (1.0 - step(1.0 - g * 0.15, blockNoise));
        col = mix(col, glitched, smoothstep(0.0, 0.3, g));
    }

    gl_FragColor = vec4(ucApply(col.rgb), col.a);
}

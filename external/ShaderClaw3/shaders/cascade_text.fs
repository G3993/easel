/*{
  "CATEGORIES": [
    "Generator",
    "Text"
  ],
  "DESCRIPTION": "Screen-filling rows of text with cascading horizontal wave offsets",
  "INPUTS": [
    {
      "NAME": "rowCount",
      "LABEL": "Row Count",
      "TYPE": "float",
      "MIN": 5,
      "MAX": 30,
      "DEFAULT": 14,
      "GROUP": "Shape / Geometry"
    },
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
      "NAME": "waveAmount",
      "LABEL": "Wave Amount",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.4,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "textColor",
      "LABEL": "Text Color",
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
      "NAME": "msg",
      "LABEL": "Message",
      "TYPE": "text",
      "DEFAULT": "ETHEREA",
      "MAX_LENGTH": 12,
      "GROUP": "Text"
    },
    {
      "NAME": "textScale",
      "LABEL": "Text Scale",
      "TYPE": "float",
      "MIN": 0.3,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Text"
    },
    {
      "NAME": "bgColor",
      "LABEL": "Background Color",
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
      "LABEL": "Transparent Background",
      "TYPE": "bool",
      "DEFAULT": true,
      "GROUP": "Background"
    }
  ]
}*/

// ── Font engine ──────────────────────────────────────────────────────
// Atlas-based (replaces legacy hardcoded 5x7 packed-bit charData() bitmap
// with a sample from the shared, high-resolution fontAtlasTex).

float charPixel(int ch, float col, float row) {
    if (ch < 0 || ch > 36) return 0.0;
    vec2 uv = vec2(col / 5.0, row / 7.0);
    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) return 0.0;
    return smoothstep(0.1, 0.55, texture2D(fontAtlasTex, vec2((float(ch) + uv.x) / 37.0, uv.y)).r);
}

int getChar(int slot) {
    if (slot == 0) return int(msg_0); if (slot == 1) return int(msg_1);
    if (slot == 2) return int(msg_2); if (slot == 3) return int(msg_3);
    if (slot == 4) return int(msg_4); if (slot == 5) return int(msg_5);
    if (slot == 6) return int(msg_6); if (slot == 7) return int(msg_7);
    if (slot == 8) return int(msg_8); if (slot == 9) return int(msg_9);
    if (slot == 10) return int(msg_10); return int(msg_11);
}

int charCount() { int n = int(msg_len); return n > 0 ? n : 1; }

// ── Main ─────────────────────────────────────────────────────────────

// Cheap hash + value noise for a secondary detail layer (density fill).
float hash21(vec2 p) { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }
float valueNoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    float a = hash21(i), b = hash21(i + vec2(1.0, 0.0));
    float c = hash21(i + vec2(0.0, 1.0)), d = hash21(i + vec2(1.0, 1.0));
    vec2 u = f * f * (3.0 - 2.0 * f);
    return mix(mix(a, b, u.x), mix(c, d, u.x), u.y);
}

void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    float aspect = RENDERSIZE.x / RENDERSIZE.y;

    // Soft-knee audio conditioning (playbook standard snippet).
    float bassP = pow(clamp(smoothstep(0.05, 0.85, audioBass), 0.0, 1.0), 1.6);
    float highP = pow(clamp(smoothstep(0.10, 0.90, audioHigh), 0.0, 1.0), 1.2);
    float drive = 0.25 + 0.75 * clamp(smoothstep(0.05, 0.9, audioEnergy), 0.0, 1.0);

    // Time-warp clock: scene lives in silence, leans into the music when present.
    float musicTime = TIME * (0.5 + 1.2 * drive * 0.6);

    int numChars = charCount();
    float rows = floor(rowCount);

    // Warp Y coordinate with a sine wave to create bunching/spreading.
    // Bass adds a gentle extra bunch/spread breathing on top of the base wave.
    float warpedY = uv.y + sin(uv.y * 6.2832 * 1.5 + musicTime * speed * 1.5) * waveAmount * (0.06 + 0.02 * bassP);

    // Row height in UV space
    float rowH = 1.0 / rows;

    // Which row is this pixel in?
    float rowIdx = floor(warpedY / rowH);
    rowIdx = clamp(rowIdx, 0.0, rows - 1.0);

    // Local Y within this row [0..1]
    float localY = fract(warpedY / rowH);

    // Character dimensions: each row is rowH tall, char aspect is 5:7
    float charH = rowH;
    float charW = charH * (5.0 / 7.0) * (1.0 / aspect) * textScale;
    float gapW = charW * 0.15;

    // Total width of one copy of the message (in UV x-space)
    float wordW = float(numChars) * (charW + gapW);

    // Cascading wave offset per row (audio time-warp clock drives pace)
    float wavePhase = rowIdx * 0.6 + musicTime * speed * 2.0;
    float xOffset = sin(wavePhase) * waveAmount * wordW * 1.5;

    // Slow base scroll
    xOffset += musicTime * speed * 0.08;

    // X position with offset applied
    float px = uv.x + xOffset;

    // Tile/repeat the text horizontally
    float posInWord = mod(px, wordW);
    if (posInWord < 0.0) posInWord += wordW;

    // Which character slot are we in?
    float charStep = charW + gapW;
    float charSlotF = posInWord / charStep;
    int charSlot = int(floor(charSlotF));

    // Position within the character cell
    float cellLocalX = fract(charSlotF);
    float charFrac = charW / charStep;

    float textHit = 0.0;

    if (cellLocalX < charFrac && charSlot >= 0 && charSlot < numChars) {
        float glyphX = cellLocalX / charFrac;
        float glyphY = localY;

        float gcol = glyphX * 5.0;
        float grow = glyphY * 7.0;

        if (gcol >= 0.0 && gcol < 5.0 && grow >= 0.0 && grow < 7.0) {
            int ch = -1;
            if (charSlot == 0) ch = getChar(0);
            else if (charSlot == 1) ch = getChar(1);
            else if (charSlot == 2) ch = getChar(2);
            else if (charSlot == 3) ch = getChar(3);
            else if (charSlot == 4) ch = getChar(4);
            else if (charSlot == 5) ch = getChar(5);
            else if (charSlot == 6) ch = getChar(6);
            else if (charSlot == 7) ch = getChar(7);
            else if (charSlot == 8) ch = getChar(8);
            else if (charSlot == 9) ch = getChar(9);
            else if (charSlot == 10) ch = getChar(10);
            else if (charSlot == 11) ch = getChar(11);

            if (ch >= 0 && ch <= 25) {
                textHit = charPixel(ch, gcol, grow);
            }
        }
    }

    // Alternating color inversion per row
    bool inverted = mod(rowIdx, 2.0) < 1.0;

    vec3 fg, bg;
    if (inverted) {
        fg = bgColor.rgb;
        bg = textColor.rgb;
    } else {
        fg = textColor.rgb;
        bg = bgColor.rgb;
    }

    // Secondary detail layer: a faint drifting grain fills the negative
    // space so the field isn't a flat, empty background between glyphs.
    // Highs sparsen/brighten it into occasional sparkle; kept subtle so it
    // never competes with the text itself.
    vec2 grainUv = uv * vec2(48.0, 18.0) + vec2(musicTime * 0.15, rowIdx * 3.7);
    float grain = valueNoise(grainUv) * 0.5 + valueNoise(grainUv * 2.13 + 11.0) * 0.5;
    float sparkle = step(0.965, grain + highP * 0.06) * highP;
    float fill = grain * 0.10 + sparkle * 0.5;

    vec3 finalCol = mix(bg, fg, textHit);
    finalCol = mix(finalCol, fg, fill * (1.0 - textHit));
    float alpha = max(textHit, fill * 0.6 * (1.0 - textHit));

    // Beat pulse: glyphs briefly brighten/flash on strong hits, easing back.
    float kick = audioBeatPulse * audioBeatPulse;
    finalCol += fg * kick * 0.35 * textHit;

    if (transparentBg) {
        alpha = max(textHit, fill * 0.5 * (1.0 - textHit));
    }

    // Surprise: every ~17s the cascade momentarily reverses — letters
    // climb upward for ~0.4s. Gravity blink.
    {
        float _ph = fract(TIME / 17.0);
        float _f  = smoothstep(0.0, 0.03, _ph) * smoothstep(0.15, 0.08, _ph);
        finalCol = mix(finalCol, finalCol.gbr * 1.15, _f * 0.55);
    }

    // ---- universal color block (defaults = no-op) ----
    vec3 uc = finalCol;
    float ucL = dot(uc, vec3(0.299, 0.587, 0.114));
    uc = mix(vec3(ucL), uc, colorBoost);                     // saturation
    if (hueShift > 0.0005) {                                  // cheap hue rotate (YIQ)
        float hA = hueShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hM * uc, 0.0, 1.0);
    }
    finalCol = uc;

    gl_FragColor = vec4(finalCol, alpha);
}

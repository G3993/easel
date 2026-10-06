/*{
  "CATEGORIES": [
    "Generator",
    "Text"
  ],
  "DESCRIPTION": "Text waving like a flag — sine wave displacement per letter",
  "INPUTS": [
    {
      "NAME": "msg",
      "TYPE": "text",
      "DEFAULT": "ETHEREA",
      "MAX_LENGTH": 12,
      "LABEL": "Message",
      "GROUP": "Text"
    },
    {
      "NAME": "textScale",
      "TYPE": "float",
      "MIN": 0.3,
      "MAX": 2,
      "DEFAULT": 1,
      "LABEL": "Text Size",
      "GROUP": "Text"
    },
    {
      "NAME": "speed",
      "TYPE": "float",
      "MIN": 0.2,
      "MAX": 5,
      "DEFAULT": 1.5,
      "LABEL": "Speed",
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "amplitude",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.15,
      "DEFAULT": 0.06,
      "LABEL": "Amplitude",
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "frequency",
      "TYPE": "float",
      "MIN": 0.5,
      "MAX": 5,
      "DEFAULT": 2,
      "LABEL": "Frequency",
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "textColor",
      "TYPE": "color",
      "DEFAULT": [
        1,
        0.9,
        0.3,
        1
      ],
      "LABEL": "Text Color",
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
        1
      ],
      "GROUP": "Background",
      "LABEL": "Background"
    },
    {
      "NAME": "transparentBg",
      "TYPE": "bool",
      "DEFAULT": true,
      "GROUP": "Background",
      "LABEL": "Transparent BG"
    }
  ]
}*/

// ---- Font engine ----
// Atlas-based (replaces legacy hardcoded 5x7 packed-bit charData() bitmap
// with a sample from the shared, high-resolution fontAtlasTex).

float charPixel(int ch, float col, float row) {
    if (ch < 0 || ch > 36) return 0.0;
    vec2 uv = vec2(col / 5.0, row / 7.0);
    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) return 0.0;
    return smoothstep(0.1, 0.55, texture2D(fontAtlasTex, vec2((float(ch) + uv.x) / 37.0, uv.y)).r);
}

int getChar(int slot) {
    if (slot == 0) return int(msg_0);
    if (slot == 1) return int(msg_1);
    if (slot == 2) return int(msg_2);
    if (slot == 3) return int(msg_3);
    if (slot == 4) return int(msg_4);
    if (slot == 5) return int(msg_5);
    if (slot == 6) return int(msg_6);
    if (slot == 7) return int(msg_7);
    if (slot == 8) return int(msg_8);
    if (slot == 9) return int(msg_9);
    if (slot == 10) return int(msg_10);
    return int(msg_11);
}

int charCount() {
    int n = int(msg_len);
    return n > 0 ? n : 1;
}

// ---- Sample a single character at a UV position within its cell ----
float sampleChar(int ch, vec2 cellUV) {
    // Space character (26) or out-of-range renders nothing
    if (ch == 26) return 0.0;
    if (ch < 0 || ch > 36) return 0.0;

    // Map cellUV to the 5x7 grid
    float col = cellUV.x * 5.0;
    float row = cellUV.y * 7.0;

    // Bounds check
    if (col < 0.0 || col >= 5.0 || row < 0.0 || row >= 7.0) return 0.0;

    return charPixel(ch, col, row);
}

// ---- Main ----

void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    float aspect = RENDERSIZE.x / RENDERSIZE.y;

    // Aspect-corrected coordinates, centered
    vec2 p = vec2((uv.x - 0.5) * aspect + 0.5, uv.y);

    int numChars = charCount();

    // ---- Soft-knee audio conditioning (playbook standard snippet) ----
    float bassP = pow(clamp(smoothstep(0.05, 0.85, audioBass), 0.0, 1.0), 1.6);
    float midP  = pow(clamp(smoothstep(0.08, 0.85, audioMid),  0.0, 1.0), 1.3);
    float highP = pow(clamp(smoothstep(0.10, 0.90, audioHigh), 0.0, 1.0), 1.2);
    float drive = 0.25 + 0.75 * clamp(smoothstep(0.05, 0.9, audioEnergy), 0.0, 1.0);
    float kick  = audioBeatPulse * audioBeatPulse;
    float musicTime = TIME * (0.85 + 0.30 * drive); // energy paces the wave
    float amp = amplitude * (1.0 + 0.28 * bassP);   // bass swells the wave

    // Character cell dimensions
    float charW = 0.09 * textScale;
    float charH = charW * 1.5;
    float gapW = charW * 0.25;
    float cellStep = charW + gapW;

    // Total width of the text block
    float totalW = float(numChars) * cellStep - gapW;

    // Starting x position (centered)
    float startX = 0.5 - totalW * 0.5;

    // Accumulate text hits
    float mainHit = 0.0;
    float shadowHit = 0.0;

    // Shadow offset in UV space
    vec2 shadowOff = vec2(0.005, -0.005);

    for (int i = 0; i < 12; i++) {
        if (i >= numChars) break;

        int ch = getChar(i);

        // Per-letter wave phase
        float phase = float(i) * frequency + musicTime * speed;

        // Vertical displacement
        float yOff = sin(phase) * amp;

        // Tilt from wave derivative (subtle rotation via skew; mids add ripple)
        float tilt = cos(phase) * amp * 3.0 * (1.0 + 0.30 * midP);

        // Cell origin (bottom-left corner before displacement)
        float cellX = startX + float(i) * cellStep;
        float cellY = 0.5 - charH * 0.5;

        // --- Main text ---
        vec2 mainCellUV = vec2(
            (p.x - cellX) / charW,
            (p.y - (cellY + yOff)) / charH
        );

        // Apply tilt (skew x based on y position within cell)
        mainCellUV.x += (mainCellUV.y - 0.5) * tilt;

        if (mainCellUV.x >= 0.0 && mainCellUV.x <= 1.0 && mainCellUV.y >= 0.0 && mainCellUV.y <= 1.0) {
            float px = sampleChar(ch, mainCellUV);
            mainHit = max(mainHit, px);
        }

        // --- Shadow (offset behind main text) ---
        vec2 shadCellUV = vec2(
            (p.x - shadowOff.x - cellX) / charW,
            (p.y - shadowOff.y - (cellY + yOff)) / charH
        );

        // Same tilt for shadow
        shadCellUV.x += (shadCellUV.y - 0.5) * tilt;

        if (shadCellUV.x >= 0.0 && shadCellUV.x <= 1.0 && shadCellUV.y >= 0.0 && shadCellUV.y <= 1.0) {
            float px = sampleChar(ch, shadCellUV);
            shadowHit = max(shadowHit, px);
        }
    }

    // Compose final color
    vec4 bg;
    if (transparentBg) {
        bg = vec4(0.0, 0.0, 0.0, 0.0);
    } else {
        bg = bgColor;
    }

    vec4 result = bg;

    // Shadow layer: dark color at 30% opacity, behind main text
    if (shadowHit > 0.5) {
        vec4 shadColor = vec4(0.0, 0.0, 0.0, 0.3);
        result = vec4(
            mix(result.rgb, shadColor.rgb, shadColor.a),
            result.a + shadColor.a * (1.0 - result.a)
        );
    }

    // Main text layer on top (highs + beat pulse sparkle the glyphs)
    if (mainHit > 0.5) {
        result = vec4(textColor.rgb * (1.0 + 0.35 * highP + 0.30 * kick), textColor.a);
    }

    // ---- universal color block (defaults = no-op) ----
    vec3 uc = result.rgb;
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
    result.rgb = uc;

    gl_FragColor = result;
}

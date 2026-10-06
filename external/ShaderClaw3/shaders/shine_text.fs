/*{
  "CATEGORIES": [
    "Generator",
    "Text"
  ],
  "DESCRIPTION": "Text with moving highlight sweep — chrome-like shine effect",
  "INPUTS": [
    {
      "NAME": "shineWidth",
      "TYPE": "float",
      "MIN": 0.05,
      "MAX": 0.5,
      "DEFAULT": 0.15,
      "GROUP": "Shape / Geometry",
      "LABEL": "Shine Width"
    },
    {
      "NAME": "speed",
      "TYPE": "float",
      "MIN": 0.2,
      "MAX": 5,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation",
      "LABEL": "Sweep Speed"
    },
    {
      "NAME": "textColor",
      "TYPE": "color",
      "DEFAULT": [
        1,
        1,
        1,
        1
      ],
      "GROUP": "Color",
      "LABEL": "Text Color"
    },
    {
      "NAME": "shineColor",
      "TYPE": "color",
      "DEFAULT": [
        1,
        1,
        1,
        1
      ],
      "GROUP": "Color",
      "LABEL": "Shine Color"
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
      "MAX_LENGTH": 12,
      "GROUP": "Text",
      "LABEL": "Message"
    },
    {
      "NAME": "textScale",
      "TYPE": "float",
      "MIN": 0.3,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Text",
      "LABEL": "Text Scale"
    },
    {
      "NAME": "bgColor",
      "TYPE": "color",
      "DEFAULT": [
        0.02,
        0.02,
        0.05,
        1
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "transparentBg",
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


// --- Text uniforms (ISF text type auto-generates these) ---
// uniform float msg_0 .. msg_11, msg_len

// --- Atlas-based font engine (A-Z + digits + space) ---
// Replaces the legacy hardcoded 5x7 packed-bit charData() bitmap with a
// sample from the shared, high-resolution fontAtlasTex — same
// charPixel/sampleChar helper used by the migrated text_*.fs shaders.

float charPixel(int ch, float col, float row) {
    if (ch < 0 || ch > 36) return 0.0;
    vec2 uv = vec2(col / 5.0, row / 7.0);
    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) return 0.0;
    return smoothstep(0.1, 0.55, texture2D(fontAtlasTex, vec2((float(ch) + uv.x) / 37.0, uv.y)).r);
}

// Accept both atlas indices (0-36, fed by the app) and raw ASCII codes
// (fed by some hosts): map ASCII letters/digits/space onto atlas indices.
int normChar(int c) {
    if (c >= 65 && c <= 90) return c - 65;      // ASCII 'A'-'Z'
    if (c >= 97 && c <= 122) return c - 97;     // ASCII 'a'-'z'
    if (c == 32) return 26;                     // ASCII space
    if (c >= 48 && c <= 57) return c - 48 + 27; // ASCII '0'-'9'
    return c;                                    // already an atlas index
}

int getChar(int slot) {
    int c;
    if (slot == 0) c = int(msg_0); else if (slot == 1) c = int(msg_1);
    else if (slot == 2) c = int(msg_2); else if (slot == 3) c = int(msg_3);
    else if (slot == 4) c = int(msg_4); else if (slot == 5) c = int(msg_5);
    else if (slot == 6) c = int(msg_6); else if (slot == 7) c = int(msg_7);
    else if (slot == 8) c = int(msg_8); else if (slot == 9) c = int(msg_9);
    else if (slot == 10) c = int(msg_10); else c = int(msg_11);
    return normChar(c);
}

int charCount() { int n = int(msg_len); return n > 0 ? n : 1; }

// --- Main ---

void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    float aspect = RENDERSIZE.x / RENDERSIZE.y;

    // Soft-knee audio conditioning (playbook standard snippet).
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.6);
    float midP  = pow(smoothstep(0.08, 0.85, audioMid), 1.3);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float drive = 0.25 + 0.75 * smoothstep(0.05, 0.9, audioEnergy);
    float punchE = audioBeatPulse * audioBeatPulse;   // decaying beat envelope

    // Aspect-corrected coordinates (centered)
    vec2 p = vec2((uv.x - 0.5) * aspect + 0.5, uv.y);

    // Character layout dimensions.
    // Glyph size breathes with bass + a decaying kick swell: with the default
    // white-on-white palette every brightness term clips invisible, so the
    // coverage of the glyphs themselves must carry the audio. Silence = base.
    // R2: LINEAR bands here — the knee'd bassP crushed ambient's 0.1-0.8
    // swells to ~2% breathing (deaf). Knees stay on the accent terms only.
    float charW = 0.09 * textScale * (1.0 + 0.13 * audioBass + 0.06 * audioMid + 0.05 * punchE);
    float charH = charW * 1.5;
    float gap = charW * 0.25;
    int numChars = charCount();
    float totalW = float(numChars) * charW + float(numChars - 1) * gap;

    // Center text horizontally, vertically at 0.5
    float startX = 0.5 - totalW * 0.5;
    float startY = 0.5 - charH * 0.5;

    // Determine if this pixel is on a text character and its x position in the text block
    float textMask = 0.0;
    float pixelX = 0.0;
    float pixelY = 0.0;

    for (int i = 0; i < 12; i++) {
        if (i >= numChars) break;

        int ch = getChar(i);

        // Skip space characters (26 = space)
        if (ch == 26) continue;

        float cx = startX + float(i) * (charW + gap);
        float cy = startY;

        // Check if pixel is within this character's bounding box
        if (p.x >= cx && p.x < cx + charW && p.y >= cy && p.y < cy + charH) {
            // Map to character grid (5 cols x 7 rows)
            float localX = (p.x - cx) / charW;
            float localY = (p.y - cy) / charH;

            float col = localX * 5.0;
            float row = localY * 7.0;

            if (col >= 0.0 && col < 5.0 && row >= 0.0 && row < 7.0) {
                float px = charPixel(ch, col, row);
                if (px > 0.5) {
                    textMask = 1.0;
                    pixelX = p.x - startX;
                    pixelY = (p.y - startY) / charH;
                }
            }
        }
    }

    // --- Shine calculation ---
    // The shine sweeps diagonally from left to right across the text, wrapping around.
    // Bass widens the shine band; the sweep paces itself to the track's energy.
    float sw = shineWidth * (1.0 + 0.3 * audioBass);
    float sweepRange = totalW + sw * 2.0;
    float shinePos = mod(TIME * speed * 0.3 * (0.7 + 0.6 * drive), sweepRange) - sw;

    // Diagonal shine: offset x position by y to create an angled sweep line
    float diagonalOffset = pixelY * 0.3 * totalW;
    float distToShine = abs(pixelX - shinePos + diagonalOffset);

    // Broad shine glow (falls off smoothly from the shine center)
    float shineAmount = smoothstep(sw, 0.0, distToShine);

    // Tight specular highlight at the very center of the shine band
    float specWidth = sw * 0.12;
    float specular = smoothstep(specWidth, 0.0, distToShine);
    specular = specular * specular; // sharpen the highlight

    // --- Compose final color ---
    vec4 finalColor;

    if (textMask > 0.5) {
        // Base text color
        vec3 col = textColor.rgb;

        // Mix in shine glow (mids feed the glow body)
        col = mix(col, shineColor.rgb, shineAmount * 0.7 * (0.9 + 0.25 * midP));

        // Add specular on top (highs sharpen the sparkle; beats add a decaying glint)
        col += shineColor.rgb * specular * 1.5 * (1.0 + 0.4 * highP + 0.3 * punchE);

        // Chrome shading: audio deepens a slow vertical shading band across
        // the glyphs. Multiplicative DARKENING is the one channel that always
        // reads on white text (additive terms clip flat). Zero in silence.
        // R2: linear bands on the dip — the follower path takes no knee.
        float shadeBand = 0.55 + 0.45 * sin(pixelY * 3.14159 + TIME * speed * 0.8);
        col *= 1.0 - (0.25 * audioBass + 0.12 * audioMid) * shadeBand;

        // Highs pull a subtle cool chrome tint (multiplicative, clip-safe).
        col *= mix(vec3(1.0), vec3(0.86, 0.93, 1.0), 0.5 * highP);

        // Clamp to prevent exceeding valid range
        col = min(col, vec3(1.0));

        finalColor = vec4(col, textColor.a);
    } else {
        // Background — R3: the eval (and any RGB-reading consumer) ignores
        // alpha, and with a transparent bg 99% of the canvas carried zero RGB
        // response. Breathe the BACKDROP RGB with a linear composite follower
        // (bus band-mix weights); alpha stays 0 so the app's transparent
        // overlay look is untouched. Silence = exactly 0 (current look).
        float bgB = 0.14 * audioBass + 0.09 * audioMid + 0.055 * audioHigh;
        vec3 bgGlow = vec3(0.85, 0.90, 1.15) * bgB;
        if (transparentBg) {
            finalColor = vec4(bgGlow, 0.0);
        } else {
            finalColor = vec4(bgColor.rgb + bgGlow * 0.5, bgColor.a);
        }
    }

    finalColor.rgb = ucApply(finalColor.rgb);
    gl_FragColor = finalColor;
}

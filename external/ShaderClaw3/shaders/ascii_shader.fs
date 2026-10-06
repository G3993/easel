/*{
  "CATEGORIES": [
    "Generator"
  ],
  "DESCRIPTION": "ASCII Matrix — falling columns of animated ASCII characters",
  "INPUTS": [
    {
      "NAME": "contrast",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.6,
      "LABEL": "Contrast"
    },
    {
      "NAME": "charSize",
      "TYPE": "float",
      "MIN": 4,
      "MAX": 32,
      "DEFAULT": 6.24,
      "LABEL": "Char Size",
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "density",
      "TYPE": "float",
      "MIN": 0.1,
      "MAX": 1,
      "DEFAULT": 0.27,
      "LABEL": "Density",
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "scrollSpeed",
      "TYPE": "float",
      "MIN": 0.1,
      "MAX": 5,
      "DEFAULT": 0.15,
      "LABEL": "Scroll Speed",
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "colorMode",
      "TYPE": "long",
      "VALUES": [
        0,
        1,
        2
      ],
      "LABELS": [
        "Mono Green",
        "Mono White",
        "Rainbow"
      ],
      "DEFAULT": 0,
      "LABEL": "Color Mode",
      "GROUP": "Color"
    },
    {
      "NAME": "charColor",
      "TYPE": "color",
      "DEFAULT": [
        1,
        1,
        1,
        1
      ],
      "LABEL": "Char Color",
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
      "LABEL": "Transparent"
    }
  ]
}*/

// =======================================================================
// MINI FONT TABLE — 10 ASCII density characters: ' .:-=+*#%@'
// Encoded as 5x7 bitmaps packed into vec2 (same encoding as text_james)
// =======================================================================

float hash(float n) { return fract(sin(n * 127.1) * 43758.5453); }
float hash2(vec2 p) { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }

// Simplified character bitmaps for density ramp
// Characters:  space . : - = + * # % @
// Returns packed vec2 for 5x7 bitmap
vec2 asciiChar(int idx) {
    if (idx == 0) return vec2(0.0, 0.0);                    // space
    if (idx == 1) return vec2(0.0, 4096.0);                 // .  (single dot bottom center)
    if (idx == 2) return vec2(4096.0, 4096.0);              // :  (two dots vertical)
    if (idx == 3) return vec2(0.0, 14336.0);                // -  (horizontal bar middle)
    if (idx == 4) return vec2(14336.0, 14336.0);            // =  (double bar)
    if (idx == 5) return vec2(4100.0, 14724.0);             // +  (cross)
    if (idx == 6) return vec2(141873.0, 4564.0);            // *  (asterisk pattern)
    if (idx == 7) return vec2(718609.0, 23213.0);           // #  (hash - dense)
    if (idx == 8) return vec2(575022.0, 14897.0);           // %  (dense pattern)
    return vec2(1033777.0, 14897.0);                        // @  (densest - uses A shape)
}

float asciiPixel(int idx, float col, float row) {
    vec2 data = asciiChar(idx);
    float ri = floor(row);
    float rv;
    if (ri < 4.0) rv = mod(floor(data.x / pow(32.0, ri)), 32.0);
    else rv = mod(floor(data.y / pow(32.0, ri - 4.0)), 32.0);
    return mod(floor(rv / pow(2.0, 4.0 - floor(col))), 2.0);
}

// =======================================================================
// MAIN — ASCII matrix effect
// =======================================================================

void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    vec2 px = gl_FragCoord.xy;

    // Soft-knee audio conditioning. Sub coupled in for hiphop's sub-heavy
    // kicks; 0.95 ceiling keeps headroom so EDM's sustained bass still
    // breathes instead of pegging the knee; 0.03 floor catches soft hits.
    float bassP = pow(smoothstep(0.03, 0.95, max(audioBass, 0.85 * audioSub)), 1.3);
    float midP  = smoothstep(0.08, 0.85, audioMid);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float drive = 0.25 + 0.75 * smoothstep(0.05, 0.9, audioEnergy);
    float hitT  = audioBeatPulse * audioBeatPulse; // decaying hit trace (300-1200ms)
    // Rain falls with the music, drifts gently in silence
    float musicTime = TIME * (0.9 + 0.4 * (drive - 0.25));

    // Grid of character cells
    float cellW = charSize;
    float cellH = charSize * 1.4;  // 5:7 aspect
    vec2 cell = floor(px / vec2(cellW, cellH));
    vec2 cellUV = mod(px, vec2(cellW, cellH)) / vec2(cellW, cellH);

    // Column properties (seeded by x position)
    float colSeed = hash(cell.x * 73.1);
    float colSpeed = 0.5 + colSeed * 1.5;
    float colOffset = colSeed * 100.0;

    // Scrolling: each column scrolls at its own rate
    float scroll = musicTime * scrollSpeed * colSpeed + colOffset;
    float scrolledY = cell.y + floor(scroll);

    // Character selection: pseudo-random per cell, changes with scroll
    float charSeed = hash2(vec2(cell.x, scrolledY));

    // Column activity: some columns are dim/inactive based on density
    float colActive = step(1.0 - density, hash(cell.x * 31.7 + 0.5));

    // Brightness ramp: brighter at the leading edge (bottom of column)
    float headPos = fract(scroll);
    float distFromHead = mod(cell.y / (RENDERSIZE.y / cellH) + headPos, 1.0);
    // Trail fade: bright at head, dims toward tail
    float trail = pow(1.0 - distFromHead, (2.0 + contrast * 3.0) * (1.0 - 0.25 * bassP));  // bass lengthens the trails

    // Map brightness to ASCII density character (0-9)
    float brightness = trail * colActive;
    int charIdx = int(floor(brightness * 9.99));
    if (charIdx < 0) charIdx = 0;
    if (charIdx > 9) charIdx = 9;

    // Sample the character bitmap
    float col5 = cellUV.x * 5.0;
    float row7 = (1.0 - cellUV.y) * 7.0;  // flip Y for top-down rendering
    float pixel = 0.0;
    if (col5 >= 0.0 && col5 < 5.0 && row7 >= 0.0 && row7 < 7.0) {
        pixel = asciiPixel(charIdx, col5, row7);
    }

    // Character change animation: occasionally swap characters
    float changeRate = hash2(vec2(cell.x, floor(TIME * 3.0 + cell.y * 0.1)));
    if (changeRate > 0.85 - 0.10 * midP) {  // mids churn more characters
        // Re-pick character for flickering effect
        int newIdx = int(floor(hash2(vec2(cell.x * 7.0, floor(TIME * 8.0))) * 9.99));
        if (newIdx < 0) newIdx = 0;
        if (newIdx > 9) newIdx = 9;
        pixel = asciiPixel(newIdx, col5, row7);
    }

    // Color
    vec3 charCol;
    if (colorMode < 0.5) {
        charCol = charColor.rgb;  // mono (default green)
    } else if (colorMode < 1.5) {
        charCol = vec3(1.0);  // white
    } else {
        // Rainbow: hue varies by column
        float hue = fract(cell.x * 0.05 + TIME * 0.1);
        charCol = vec3(
            abs(hue * 6.0 - 3.0) - 1.0,
            2.0 - abs(hue * 6.0 - 2.0),
            2.0 - abs(hue * 6.0 - 4.0)
        );
        charCol = clamp(charCol, 0.0, 1.0);
    }

    // Final color: character pixel * brightness * color
    vec3 finalCol = bgColor.rgb;
    float finalAlpha = transparentBg ? 0.0 : 1.0;
    // Bass follow deepened to a visible ±35%, plus a decaying trace on each
    // hit so hiphop's two-kicks-a-bar still read seconds apart (no gate,
    // beatPulse eases out on its own).
    float mask = pixel * brightness * (1.0 + 0.35 * bassP + 0.45 * hitT);

    // Brighten the leading edge characters (highs whiten the head sparkle,
    // hits flash the heads then ease back)
    float headGlow = smoothstep(0.8, 1.0, 1.0 - distFromHead) * colActive;
    vec3 glowCol = mix(charCol, vec3(1.0), headGlow * (0.6 + 0.25 * highP + 0.3 * hitT));

    finalCol = mix(finalCol, glowCol, clamp(mask, 0.0, 1.0));
    if (transparentBg) finalAlpha = clamp(mask, 0.0, 1.0);

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

    gl_FragColor = vec4(finalCol, finalAlpha);
}

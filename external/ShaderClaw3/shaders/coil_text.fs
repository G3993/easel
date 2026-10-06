/*{
  "CATEGORIES": [
    "Generator",
    "Text"
  ],
  "DESCRIPTION": "Text arranged along a spiral/coil path with rotating colored ring bands",
  "INPUTS": [
    {
      "NAME": "preset",
      "LABEL": "Coil Pattern",
      "TYPE": "long",
      "VALUES": [
        0,
        1,
        2,
        3,
        4,
        5,
        6,
        7
      ],
      "LABELS": [
        "Wide",
        "Tight",
        "Star",
        "Hourglass",
        "Lemniscate",
        "Spacer",
        "Dense",
        "Pulse"
      ],
      "DEFAULT": 0,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "rings",
      "LABEL": "Rings",
      "TYPE": "float",
      "MIN": 3,
      "MAX": 15,
      "DEFAULT": 8,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "speed",
      "LABEL": "Speed",
      "TYPE": "float",
      "MIN": 0.1,
      "MAX": 3,
      "DEFAULT": 0.3,
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

const float PI = 3.14159265;
const float TWO_PI = 6.28318530;

// ── Font engine ──────────────────────────────────────────────────────
// Atlas-based (replaces legacy hardcoded 5x7 packed-bit charData() bitmap
// with a sample from the shared, high-resolution fontAtlasTex).

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

// ── Sample a character at a local UV ─────────────────────────────────

float sampleChar(int ch, vec2 localUV) {
    if (ch == 26) return 0.0;
    if (ch < 0 || ch > 36) return 0.0;
    float col = localUV.x * 5.0;
    float row = localUV.y * 7.0;
    if (col < 0.0 || col >= 5.0 || row < 0.0 || row >= 7.0) return 0.0;
    return charPixel(ch, col, row);
}

// ── Main ─────────────────────────────────────────────────────────────

void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    float aspect = RENDERSIZE.x / RENDERSIZE.y;

    int numChars = charCount();
    int presetIdx = int(preset);

    // ── Soft-knee audio conditioning (playbook standard snippet) ─────
    float bassP = pow(clamp(smoothstep(0.05, 0.85, audioBass), 0.0, 1.0), 1.6);
    float midP  = pow(clamp(smoothstep(0.08, 0.85, audioMid),  0.0, 1.0), 1.3);
    float highP = pow(clamp(smoothstep(0.10, 0.90, audioHigh), 0.0, 1.0), 1.2);
    float drive = 0.25 + 0.75 * clamp(smoothstep(0.05, 0.9, audioEnergy), 0.0, 1.0);
    float kick  = audioBeatPulse * audioBeatPulse;

    // ── Preset parameters ────────────────────────────────────────────
    float innerRadius = 0.1;
    float ringGap = 0.06;
    float charSpacing = 1.0;   // multiplier on gap between characters along arc
    bool shapeModulate = false;
    int shapeType = 0;        // 0=none, 1=star, 2=hourglass, 3=lemniscate
    bool doPulse = false;

    if (presetIdx == 0) {
        // Wide: standard spiral, generous spacing
        innerRadius = 0.1;
        ringGap = 0.06;
    } else if (presetIdx == 1) {
        // Tight: tighter ring spacing
        innerRadius = 0.05;
        ringGap = 0.03;
    } else if (presetIdx == 2) {
        // Star: 5-pointed star modulation
        innerRadius = 0.08;
        ringGap = 0.05;
        shapeModulate = true;
        shapeType = 1;
    } else if (presetIdx == 3) {
        // Hourglass: abs(sin(angle)) modulation
        innerRadius = 0.08;
        ringGap = 0.05;
        shapeModulate = true;
        shapeType = 2;
    } else if (presetIdx == 4) {
        // Lemniscate: sqrt(abs(cos(2*angle))) modulation
        innerRadius = 0.08;
        ringGap = 0.05;
        shapeModulate = true;
        shapeType = 3;
    } else if (presetIdx == 5) {
        // Spacer: extra gap between characters
        innerRadius = 0.1;
        ringGap = 0.06;
        charSpacing = 2.0;
    } else if (presetIdx == 6) {
        // Dense: very tight, many rings, small text
        innerRadius = 0.02;
        ringGap = 0.02;
    } else if (presetIdx == 7) {
        // Pulse: ring radius pulsates with time
        innerRadius = 0.1;
        ringGap = 0.06;
        doPulse = true;
    }

    // Apply pulse
    float effectiveRingGap = ringGap;
    if (doPulse) {
        effectiveRingGap *= 1.0 + 0.3 * sin(TIME * 2.0);
    }

    // Scale ring gap by textScale (larger text = wider rings)
    effectiveRingGap *= textScale;
    innerRadius *= textScale;

    // ── Pixel to centered, aspect-corrected coordinates ──────────────
    vec2 center = vec2(0.5 * aspect, 0.5);
    vec2 p = vec2(uv.x * aspect, uv.y) - center;
    // Bass breathes the whole coil outward (smoothed zoom, layout unchanged)
    p /= 1.0 + 0.15 * bassP;

    // ── Polar coordinates ────────────────────────────────────────────
    float radius = length(p);
    float angle = atan(p.y, p.x); // -PI to PI

    // Add time rotation
    angle -= TIME * speed;
    // Keep angle in -PI..PI range
    angle = mod(angle + PI, TWO_PI) - PI;

    // ── Shape modulation ─────────────────────────────────────────────
    // For shape presets, we modulate the effective radius so the rings
    // distort into non-circular shapes
    float effectiveRadius = radius;
    if (shapeModulate) {
        float shapeFactor = 1.0;
        if (shapeType == 1) {
            // Star: modulate by cos(5*angle)
            shapeFactor = 1.0 + 0.3 * cos(5.0 * angle);
        } else if (shapeType == 2) {
            // Hourglass: modulate by abs(sin(angle))
            shapeFactor = 0.5 + 0.5 * abs(sin(angle));
        } else if (shapeType == 3) {
            // Lemniscate: modulate by sqrt(abs(cos(2*angle)))
            shapeFactor = 0.3 + 0.7 * sqrt(abs(cos(2.0 * angle)));
        }
        // Divide radius by shape factor so that the "ring" boundary
        // follows the shape (pixels at the shape boundary all map to same ring)
        effectiveRadius = radius / shapeFactor;
    }

    // ── Determine ring index ─────────────────────────────────────────
    float ringFloat = (effectiveRadius - innerRadius) / effectiveRingGap;
    float ringIdx = floor(ringFloat);
    float ringFrac = ringFloat - ringIdx; // 0..1 position within ring band

    // Clamp to valid ring range
    float maxRings = rings;
    if (ringIdx < 0.0 || ringIdx >= maxRings) {
        // Outside all rings
        if (transparentBg) {
            gl_FragColor = vec4(0.0, 0.0, 0.0, 0.0);
        } else {
            gl_FragColor = bgColor;
        }
        return;
    }

    // ── Character layout along ring arc ──────────────────────────────
    // The ring's center radius (in effective-radius space)
    float ringCenterR = innerRadius + (ringIdx + 0.5) * effectiveRingGap;

    // Character cell size in world units
    float charH = effectiveRingGap * 0.75;  // height = fraction of ring width
    float charW = charH * (5.0 / 7.0);      // aspect ratio of 5x7 font
    float gapW = charW * 0.3 * charSpacing;  // gap between characters

    // Arc length per character cell
    float cellArc = charW + gapW;

    // Total arc length of the ring circumference at this radius
    // Use the actual ring center radius for correct spacing
    float actualRingR = ringCenterR;
    if (shapeModulate) {
        // For shape modulation, the actual circumference varies, but we
        // use the effective radius for consistent character placement
        actualRingR = ringCenterR;
    }
    float circumference = TWO_PI * actualRingR;

    // How many character cells fit around the ring
    float charsAround = circumference / cellArc;

    // Number of text repetitions around the ring (at least 1)
    float textLen = float(numChars);
    float reps = max(1.0, floor(charsAround / textLen));
    float totalCharsAround = reps * textLen;

    // Recalculate cell arc to perfectly tile
    float adjustedCellArc = circumference / totalCharsAround;
    float adjustedCharW = adjustedCellArc * (charW / cellArc);

    // ── Map angle to character position ──────────────────────────────
    // Normalize angle to 0..TWO_PI
    float normAngle = mod(angle + PI, TWO_PI);

    // Add per-ring offset so rings don't all align
    normAngle = mod(normAngle + ringIdx * 0.7, TWO_PI);

    // Arc position in character units
    float arcPos = (normAngle / TWO_PI) * totalCharsAround;
    float charIdx = floor(arcPos);
    float charFrac = arcPos - charIdx;

    // Which character in the text string
    int textIdx = int(mod(charIdx, textLen));

    // ── Transform pixel into character-local rotated space ───────────
    // The character's center angle on the ring
    float charCenterArc = (charIdx + 0.5) / totalCharsAround;
    float charAngle = charCenterArc * TWO_PI - PI;
    // Undo the per-ring offset
    charAngle = charAngle - ringIdx * 0.7;
    // Undo time rotation to get back to world-space angle
    charAngle = charAngle + TIME * speed;

    // Character center position in world space (before shape modulation)
    float ca = cos(charAngle);
    float sa = sin(charAngle);

    // For shape-modulated presets, compute the actual radius at this char angle
    float charActualR = ringCenterR;
    if (shapeModulate) {
        float sf = 1.0;
        // Use original angle (before time rotation) for shape
        // Actually use charAngle which includes time rotation undone
        float shapeAngle = charAngle;
        if (shapeType == 1) {
            sf = 1.0 + 0.3 * cos(5.0 * shapeAngle);
        } else if (shapeType == 2) {
            sf = 0.5 + 0.5 * abs(sin(shapeAngle));
        } else if (shapeType == 3) {
            sf = 0.3 + 0.7 * sqrt(abs(cos(2.0 * shapeAngle)));
        }
        charActualR = ringCenterR * sf;
    }

    vec2 charCenter = vec2(ca, sa) * charActualR;

    // Pixel offset from character center
    vec2 pixelOffset = p - charCenter;

    // Rotate into character-local space
    // Tangent direction = (-sin(charAngle), cos(charAngle)) = text X axis
    // Radial direction = (cos(charAngle), sin(charAngle)) = text Y axis
    vec2 localPos;
    localPos.x = dot(pixelOffset, vec2(-sa, ca));   // along tangent (text x)
    localPos.y = dot(pixelOffset, vec2(ca, sa));     // along radial (text y)

    // ── Map local position to character cell UV ──────────────────────
    // Character cell is adjustedCharW wide, charH tall
    // Energy + mids thicken the glyphs in place (cell centers untouched)
    float swell = 1.0 + 0.04 * (drive - 0.25) + 0.12 * midP;
    vec2 cellUV;
    cellUV.x = (localPos.x / (adjustedCharW * swell)) + 0.5;
    cellUV.y = 1.0 - ((localPos.y / (charH * swell)) + 0.5);

    // ── Sample the character ─────────────────────────────────────────
    float textHit = 0.0;

    if (cellUV.x >= 0.0 && cellUV.x <= 1.0 && cellUV.y >= 0.0 && cellUV.y <= 1.0) {
        // Get the character for this text position
        int ch = -1;
        if (textIdx == 0) ch = getChar(0);
        else if (textIdx == 1) ch = getChar(1);
        else if (textIdx == 2) ch = getChar(2);
        else if (textIdx == 3) ch = getChar(3);
        else if (textIdx == 4) ch = getChar(4);
        else if (textIdx == 5) ch = getChar(5);
        else if (textIdx == 6) ch = getChar(6);
        else if (textIdx == 7) ch = getChar(7);
        else if (textIdx == 8) ch = getChar(8);
        else if (textIdx == 9) ch = getChar(9);
        else if (textIdx == 10) ch = getChar(10);
        else if (textIdx == 11) ch = getChar(11);

        if (ch >= 0 && ch <= 25) {
            textHit = sampleChar(ch, cellUV);
        }
    }

    // ── Ring band coloring (alternating) ─────────────────────────────
    bool inverted = mod(ringIdx, 2.0) < 1.0;

    vec3 fg, bg;
    if (inverted) {
        fg = textColor.rgb;
        bg = bgColor.rgb;
    } else {
        fg = bgColor.rgb;
        bg = textColor.rgb;
    }

    vec3 finalCol = mix(bg, fg, textHit);
    float alpha = 1.0;

    if (transparentBg) {
        // In transparent mode, only show text pixels
        alpha = textHit;
        finalCol = mix(vec3(0.0), textColor.rgb, textHit);
    }

    // Highs + beat pulse sparkle the glyph pixels only
    finalCol *= 1.0 + (0.30 * highP + 0.30 * kick) * textHit;

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

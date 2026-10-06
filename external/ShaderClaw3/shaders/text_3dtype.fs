/*{
  "CATEGORIES": [
    "Generator",
    "Text"
  ],
  "DESCRIPTION": "3D Type — layered depth text with parallax perspective and color gradients",
  "INPUTS": [
    {
      "NAME": "depth",
      "LABEL": "Depth",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.5,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "speed",
      "LABEL": "Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 0.5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "hueSpread",
      "LABEL": "Hue Spread",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 1,
      "GROUP": "Color"
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
      "DEFAULT": " ETHEREA",
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
        0.04,
        0.04,
        0.07,
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


// Atlas-only character sampling (no bitmap charData = no 27-branch dead code)
float sampleAtlas(int ch, vec2 cellUV) {
    if (ch < 0 || ch > 36) return 0.0;
    if (cellUV.x < 0.0 || cellUV.x > 1.0 || cellUV.y < 0.0 || cellUV.y > 1.0) return 0.0;
    return smoothstep(0.1, 0.55, texture2D(fontAtlasTex, vec2((float(ch) + cellUV.x) / 37.0, cellUV.y)).r);
}

int getChar(int slot) {
    if (slot == 0) return int(msg_0);   if (slot == 1) return int(msg_1);
    if (slot == 2) return int(msg_2);   if (slot == 3) return int(msg_3);
    if (slot == 4) return int(msg_4);   if (slot == 5) return int(msg_5);
    if (slot == 6) return int(msg_6);   if (slot == 7) return int(msg_7);
    if (slot == 8) return int(msg_8);   if (slot == 9) return int(msg_9);
    if (slot == 10) return int(msg_10); if (slot == 11) return int(msg_11);
    if (slot == 12) return int(msg_12); if (slot == 13) return int(msg_13);
    if (slot == 14) return int(msg_14); if (slot == 15) return int(msg_15);
    if (slot == 16) return int(msg_16); if (slot == 17) return int(msg_17);
    if (slot == 18) return int(msg_18); if (slot == 19) return int(msg_19);
    if (slot == 20) return int(msg_20); if (slot == 21) return int(msg_21);
    if (slot == 22) return int(msg_22); if (slot == 23) return int(msg_23);
    if (slot == 24) return int(msg_24); if (slot == 25) return int(msg_25);
    if (slot == 26) return int(msg_26); if (slot == 27) return int(msg_27);
    if (slot == 28) return int(msg_28); if (slot == 29) return int(msg_29);
    if (slot == 30) return int(msg_30); if (slot == 31) return int(msg_31);
    if (slot == 32) return int(msg_32); if (slot == 33) return int(msg_33);
    if (slot == 34) return int(msg_34); if (slot == 35) return int(msg_35);
    if (slot == 36) return int(msg_36); if (slot == 37) return int(msg_37);
    if (slot == 38) return int(msg_38); if (slot == 39) return int(msg_39);
    if (slot == 40) return int(msg_40); if (slot == 41) return int(msg_41);
    if (slot == 42) return int(msg_42); if (slot == 43) return int(msg_43);
    if (slot == 44) return int(msg_44); if (slot == 45) return int(msg_45);
    if (slot == 46) return int(msg_46); return int(msg_47);
}

int charCount() {
    int n = int(msg_len);
    if (n <= 0) return 7;
    if (n > 48) return 48;
    return n;
}

vec3 hsv2rgb(vec3 c) {
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

vec3 rgb2hsv(vec3 c) {
    vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));
    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

// Single-line text hit test — all chars on one row, scaled to fit width
float textHit(vec2 uv, float aspect) {
    int numChars = charCount();
    float _ts = textScale > 0.01 ? textScale : 1.0;
    float _kn = kerning > 0.01 ? kerning : 1.0;
    float charH = 0.18 * _ts;
    if (aspect < 1.0) charH *= aspect;
    float charW = charH * (5.0 / 7.0);
    float gap = charW * 0.25 * _kn;
    float cellStep = charW + gap;
    float maxW = aspect * 0.9;

    // All chars on one row — scale down if wider than screen
    float totalW = float(numChars) * cellStep - gap;
    if (totalW > maxW) {
        float sc = maxW / totalW;
        charH *= sc; charW = charH * (5.0 / 7.0); gap = charW * 0.25 * _kn;
        cellStep = charW + gap;
        totalW = float(numChars) * cellStep - gap;
    }

    float startY = 0.5 - charH * 0.5;
    float startX = 0.5 - totalW * 0.5;
    vec2 p = vec2((uv.x - 0.5) * aspect + 0.5, uv.y);

    // Bounding box early-out
    if (p.y < startY - charH * 0.1 || p.y > startY + charH * 1.1) return 0.0;
    if (p.x < startX - cellStep * 0.1 || p.x > startX + totalW + cellStep * 0.1) return 0.0;

    for (int i = 0; i < 48; i++) {
        if (i >= numChars) break;
        int ch = getChar(i);
        if (ch >= 0 && ch <= 36 && ch != 26) {
            float cx = startX + float(i) * cellStep;
            float cy = startY;
            vec2 cellUV = vec2((p.x - cx) / charW, (p.y - cy) / charH);
            float hit = sampleAtlas(ch, cellUV);
            if (hit > 0.5) return 1.0;
        }
    }
    return 0.0;
}

// =======================================================================
// MAIN — 8-layer parallax (down from 25 for fast ANGLE compilation)
// =======================================================================

void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE;
    float aspect = RENDERSIZE.x / RENDERSIZE.y;

    // Soft-knee audio conditioning: low floors so ambient swells and soft
    // jazz accents register; 0.95 ceiling keeps headroom so EDM's constant
    // kicks still breathe instead of pegging the knee.
    float bassP = pow(smoothstep(0.03, 0.95, audioBass), 1.3);
    float midP  = pow(smoothstep(0.04, 0.90, audioMid), 1.2);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float drive = 0.25 + 0.75 * smoothstep(0.05, 0.9, audioEnergy);
    float kickT = audioBeatPulse * audioBeatPulse; // decaying hit trace

    float layerSpacing = depth * 0.008 * (1.0 + 0.3 * bassP);

    float perspX = sin(TIME * speed * 0.3) * 0.25;
    float perspY = cos(TIME * speed * 0.25) * 0.15;
    float breathe = sin(TIME * speed * 0.8) * 0.3 * (0.85 + 0.3 * drive);

    vec3 baseHSV = rgb2hsv(textColor.rgb);
    vec3 finalColor = transparentBg ? vec3(0.0) : bgColor.rgb;
    float finalAlpha = transparentBg ? 0.0 : 1.0;

    // 8 layers back to front (i=7 deepest, i=0 front)
    for (int i = 7; i >= 0; i--) {
        float t = float(i) / 7.0;
        float layerDepth = float(i) * layerSpacing * (1.0 + breathe * 0.2);

        float waveX = sin(TIME * speed * 0.5 + float(i) * 0.4) * 0.003 * (1.0 + 0.3 * midP);
        float waveY = cos(TIME * speed * 0.4 + float(i) * 0.3) * 0.002 * (1.0 + 0.3 * midP);

        vec2 offset = vec2(perspX, perspY) * layerDepth + vec2(waveX, waveY);
        float layerScale = 1.0 + t * 0.06;
        // Continuous zoom breathing: bass swells (ambient) and soft kicks
        // (jazz) visibly scale the whole type; kick trace pops then eases.
        // R2: LINEAR bands, deeper — the knee'd bassP crushed ambient's
        // swells to ~2% zoom. White glyphs clip additive light, so scale
        // is the channel that has to carry the follower.
        float breatheZoom = 1.0 + 0.13 * audioBass + 0.06 * audioMid + 0.05 * kickT;
        vec2 layerUV = (uv - 0.5) / (layerScale * breatheZoom) + 0.5 + offset;

        float hit = textHit(layerUV, aspect);
        if (hit > 0.5) {
            vec3 hsv = baseHSV;
            hsv.x = fract(hsv.x + t * hueSpread);
            hsv.y = min(1.0, hsv.y + t * 0.2);
            hsv.z = min(1.0, hsv.z * (1.0 + 0.3 * highP * (1.0 - t)));
            vec3 layerColor = hsv2rgb(hsv);
            // Mids raise deep-layer opacity (silence = exactly the old look;
            // front layer clamps at 1.0 so only the depth stack breathes).
            // R2: linear audioMid — no knee on the follower path.
            float alpha = min(1.0, (0.3 + 0.7 * (1.0 - t)) * (1.0 + 0.35 * audioMid));
            finalColor = mix(finalColor, layerColor, alpha);
            finalAlpha = max(finalAlpha, alpha);
        }
    }

    // Scanline
    float scanline = 1.0 - 0.03 * step(0.5, fract(gl_FragCoord.y / 3.0));
    finalColor *= scanline;

    gl_FragColor = vec4(ucApply(finalColor), finalAlpha);
}

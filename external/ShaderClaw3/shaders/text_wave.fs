/*{
  "CATEGORIES": [
    "Generator",
    "Text"
  ],
  "DESCRIPTION": "Wave - sine displacement per letter",
  "INPUTS": [
    {
      "NAME": "msg",
      "TYPE": "text",
      "DEFAULT": " ETHEREA",
      "MAX_LENGTH": 48,
      "LABEL": "Message",
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
      "LABEL": "Amplitude",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "density",
      "LABEL": "Frequency",
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

const float PI = 3.14159265;
const float TWO_PI = 6.28318530;

// Atlas-only font engine (no bitmap fallback — faster ANGLE compile)
float charPixel(int ch, float col, float row) {
    if (ch < 0 || ch > 36) return 0.0;
    vec2 uv = vec2(col / 5.0, row / 7.0);
    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) return 0.0;
    return smoothstep(0.1, 0.55, texture2D(fontAtlasTex, vec2((float(ch) + uv.x) / 37.0, uv.y)).r);
}

int getChar(int slot) {
    if (slot == 0)  return int(msg_0);
    if (slot == 1)  return int(msg_1);
    if (slot == 2)  return int(msg_2);
    if (slot == 3)  return int(msg_3);
    if (slot == 4)  return int(msg_4);
    if (slot == 5)  return int(msg_5);
    if (slot == 6)  return int(msg_6);
    if (slot == 7)  return int(msg_7);
    if (slot == 8)  return int(msg_8);
    if (slot == 9)  return int(msg_9);
    if (slot == 10) return int(msg_10);
    if (slot == 11) return int(msg_11);
    if (slot == 12) return int(msg_12);
    if (slot == 13) return int(msg_13);
    if (slot == 14) return int(msg_14);
    if (slot == 15) return int(msg_15);
    if (slot == 16) return int(msg_16);
    if (slot == 17) return int(msg_17);
    if (slot == 18) return int(msg_18);
    if (slot == 19) return int(msg_19);
    if (slot == 20) return int(msg_20);
    if (slot == 21) return int(msg_21);
    if (slot == 22) return int(msg_22);
    if (slot == 23) return int(msg_23);
    if (slot == 24) return int(msg_24);
    if (slot == 25) return int(msg_25);
    if (slot == 26) return int(msg_26);
    if (slot == 27) return int(msg_27);
    if (slot == 28) return int(msg_28);
    if (slot == 29) return int(msg_29);
    if (slot == 30) return int(msg_30);
    if (slot == 31) return int(msg_31);
    if (slot == 32) return int(msg_32);
    if (slot == 33) return int(msg_33);
    if (slot == 34) return int(msg_34);
    if (slot == 35) return int(msg_35);
    if (slot == 36) return int(msg_36);
    if (slot == 37) return int(msg_37);
    if (slot == 38) return int(msg_38);
    if (slot == 39) return int(msg_39);
    if (slot == 40) return int(msg_40);
    if (slot == 41) return int(msg_41);
    if (slot == 42) return int(msg_42);
    if (slot == 43) return int(msg_43);
    if (slot == 44) return int(msg_44);
    if (slot == 45) return int(msg_45);
    if (slot == 46) return int(msg_46);
    return int(msg_47);
}

int charCount() {
    int n = int(msg_len);
    return n > 0 ? n : 1;
}

float sampleChar(int ch, vec2 uv) {
    if (ch < 0 || ch > 36) return 0.0;
    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) return 0.0;
    return texture2D(fontAtlasTex, vec2((float(ch) + uv.x) / 37.0, uv.y)).r;
}

float hash(float n) { return fract(sin(n * 127.1) * 43758.5453); }


// =======================================================================
// EFFECT: WAVE - sine displacement per letter
// =======================================================================

vec4 effectWave(vec2 uv) {
    float aspect = RENDERSIZE.x / RENDERSIZE.y;
    int numChars = charCount();

    // Soft-knee audio conditioning (playbook standard snippet).
    float bassP = pow(clamp(smoothstep(0.05, 0.85, audioBass), 0.0, 1.0), 1.6);
    float midP  = pow(clamp(smoothstep(0.08, 0.85, audioMid),  0.0, 1.0), 1.3);
    float highP = pow(clamp(smoothstep(0.10, 0.90, audioHigh), 0.0, 1.0), 1.2);
    float drive = 0.25 + 0.75 * clamp(smoothstep(0.05, 0.9, audioEnergy), 0.0, 1.0);
    float kick  = audioBeatPulse * audioBeatPulse;
    // Un-powed low-knee follower: ambient's 0.1-0.8 bass swells live here.
    float bassSm = clamp(smoothstep(0.03, 0.92, audioBass), 0.0, 1.0);
    float musicTime = TIME * (0.85 + 0.30 * drive); // energy paces the wave

    float amplitude = mix(0.0, 0.15, intensity) * (1.0 + 0.55 * bassSm); // bass swells amplitude (deep, continuous)
    float frequency = mix(0.5, 5.0, density);

    vec2 p = vec2((uv.x - 0.5) * aspect + 0.5, uv.y);
    // Bass zoom breathing (±10%) — whole line scales with the smoothed envelope.
    p = vec2(0.5, 0.5) + (p - vec2(0.5, 0.5)) / (1.0 + 0.10 * bassSm);

    // Single-line layout — scale down on portrait so text fits width
    float cW = 0.09 * textScale;
    if (aspect < 1.0) cW *= aspect;
    float cH = cW * 1.5;
    float gW = cW * 0.25 * kerning;
    float cellStep = cW + gW;

    // Scale down to fit width if text is wider than screen
    float totalTextW = float(numChars) * cellStep - gW;
    float availW = aspect * 0.9;
    float fitScale = 1.0;
    if (totalTextW > availW) {
        fitScale = availW / totalTextW;
        cW *= fitScale;
        cH *= fitScale;
        gW *= fitScale;
        cellStep = cW + gW;
        totalTextW = float(numChars) * cellStep - gW;
    }

    float totalH = cH;
    float startY = 0.5;
    float rowStartX = 0.5 - totalTextW * 0.5;

    float mainHit = 0.0, shadowHit = 0.0;
    vec2 so = vec2(0.005, -0.005);

    for (int i = 0; i < 48; i++) {
        if (i >= numChars) break;
        int ch = getChar(i);

        float phase = float(i) * frequency + musicTime * speed;
        float yOff = sin(phase) * amplitude;
        float oscY = oscAmount * sin(TIME * oscSpeed * 6.2832 + float(i) * oscSpread * 3.14159);
        yOff += oscY;
        float tilt = cos(phase) * amplitude * 3.0 * (1.0 + 0.30 * midP); // mids add skew detail
        float cellX = rowStartX + float(i) * cellStep;
        float cellY = startY;

        vec2 m = vec2((p.x - cellX) / cW, (p.y - (cellY + yOff)) / cH);
        m.x += (m.y - 0.5) * tilt;
        if (m.x >= 0.0 && m.x <= 1.0 && m.y >= 0.0 && m.y <= 1.0)
            mainHit = max(mainHit, sampleChar(ch, m));

        vec2 s = vec2((p.x - so.x - cellX) / cW, (p.y - so.y - (cellY + yOff)) / cH);
        s.x += (s.y - 0.5) * tilt;
        if (s.x >= 0.0 && s.x <= 1.0 && s.y >= 0.0 && s.y <= 1.0)
            shadowHit = max(shadowHit, sampleChar(ch, s));
    }

    vec4 result = transparentBg ? vec4(0.0) : bgColor;
    if (shadowHit > 0.5)
        result = vec4(mix(result.rgb, vec3(0.0), 0.3), result.a + 0.3*(1.0-result.a));
    if (mainHit > 0.5) result = vec4(textColor.rgb * (1.0 + 0.28 * bassSm + 0.22 * midP + 0.35 * highP + 0.30 * kick), textColor.a); // continuous band glow + beat sparkle
    return result;
}

// =======================================================================
// MAIN
// =======================================================================

void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    vec4 col = effectWave(uv);

    if (_voiceGlitch > 0.01) {
        float g = _voiceGlitch;
        float t = TIME * 17.0;
        float band = floor(uv.y * mix(8.0, 40.0, g) + t * 3.0);
        float bandNoise = fract(sin(band * 91.7 + t) * 43758.5);
        float bandActive = step(1.0 - g * 0.6, bandNoise);
        float shift = (bandNoise - 0.5) * 0.08 * g * bandActive;
        float chromaAmt = g * 0.015;
        vec2 uvR = uv + vec2(shift + chromaAmt, 0.0);
        vec2 uvB = uv + vec2(shift - chromaAmt, 0.0);
        vec2 uvG = uv + vec2(shift, chromaAmt * 0.5);
        vec4 cR = effectWave(uvR);
        vec4 cG = effectWave(uvG);
        vec4 cB = effectWave(uvB);
        vec4 glitched = vec4(cR.r, cG.g, cB.b, max(max(cR.a, cG.a), cB.a));
        float scanline = 0.95 + 0.05 * sin(uv.y * RENDERSIZE.y * 1.5 + t * 40.0);
        float blockX = floor(uv.x * 6.0);
        float blockY = floor(uv.y * 4.0);
        float blockNoise = fract(sin((blockX + blockY * 7.0) * 113.1 + floor(t * 8.0)) * 43758.5);
        float dropout = step(1.0 - g * 0.15, blockNoise);
        glitched.rgb *= scanline;
        glitched.rgb *= 1.0 - dropout;
        col = mix(col, glitched, smoothstep(0.0, 0.3, g));
    }

    // ---- universal color block (defaults = no-op) ----
    vec3 uc = col.rgb;
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
    col.rgb = uc;

    gl_FragColor = col;
}

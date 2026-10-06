/*{
  "CATEGORIES": [
    "Generator",
    "Text"
  ],
  "DESCRIPTION": "Digifade — glitch dissolve with psychedelic plasma background. Rainbow sinusoidal plasma field pulses in full saturation. White text at HDR 2.5+ cuts through. LINEAR HDR out.",
  "INPUTS": [
    {
      "NAME": "preset",
      "LABEL": "Style",
      "TYPE": "long",
      "VALUES": [
        0,
        1
      ],
      "LABELS": [
        "Digifade",
        "Digifade Glitch"
      ],
      "DEFAULT": 0
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
      "LABEL": "Glitch",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "density",
      "LABEL": "Dissolve",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
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
    },
    {
      "NAME": "hdrGlow",
      "LABEL": "HDR Glow",
      "TYPE": "float",
      "MIN": 1,
      "MAX": 4,
      "DEFAULT": 2.5
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

vec3 hsv2rgbDF(vec3 c) {
    vec4 K = vec4(1.0,2.0/3.0,1.0/3.0,3.0);
    vec3 p = abs(fract(c.xxx+K.xyz)*6.0-K.www);
    return c.z*mix(K.xxx, clamp(p-K.xxx,0.0,1.0), c.y);
}

// Psychedelic plasma background — sinusoidal rainbow field
vec3 plasmaBg(vec2 uv) {
    float t = TIME * 0.35;
    vec2 p = uv * 4.0;
    float v = sin(p.x + t)
            + sin(p.y + t * 0.93)
            + sin((p.x + p.y) * 0.7 + t * 1.1)
            + sin(length(p - vec2(2.0, 2.0)) * 1.5 - t * 0.8);
    float hue = fract(v * 0.18 + t * 0.05);
    // Fully saturated, value at 0.7 so it reads dark enough for text contrast
    return hsv2rgbDF(vec3(hue, 1.0, 0.65));
}

// =======================================================================
// EFFECT: DIGIFADE - glitch dissolve
// =======================================================================

vec4 effectDigifade(vec2 uv, int sub, vec3 bgOverride) {
    float aspect = RENDERSIZE.x / RENDERSIZE.y;
    int numChars = charCount();
    float glitchAmount = intensity;
    float sliceCount = mix(5.0, 100.0, density);

    float complexity = 1.0, sweepSpeed = 1.0, vertGlitch = 0.0, maxDisp = 0.3;
    if (sub == 1) { complexity = 2.0; sweepSpeed = 1.3; maxDisp = 0.5; vertGlitch = 0.4; }

    // Soft-knee audio conditioning (playbook standard snippet). Idle floor
    // keeps the sweep alive in silence; energy leans the clock into the beat.
    float bassP = pow(clamp(smoothstep(0.03, 0.85, audioBass), 0.0, 1.0), 1.4);
    // Sub coupling: hiphop's sparse kicks live in the sub band — fold it in
    // so those hits register even when the bass band alone stays quiet.
    float subP  = pow(clamp(smoothstep(0.03, 0.80, audioSub), 0.0, 1.0), 1.3);
    bassP = max(bassP, subP);
    float midP  = clamp(smoothstep(0.06, 0.90, audioMid), 0.0, 1.0);
    float highP = pow(clamp(smoothstep(0.10, 0.90, audioHigh), 0.0, 1.0), 1.2);
    float drive = 0.25 + 0.75 * clamp(smoothstep(0.05, 0.9, audioEnergy), 0.0, 1.0);
    float musicTime = TIME * (0.5 + 1.2 * drive * 0.55);
    maxDisp *= 1.0 + bassP * 0.45 + midP * 0.15; // bass/mids deepen the glitch sweep
    glitchAmount *= 0.7 + 0.6 * drive + 0.25 * midP; // energy+mids drive the dissolve intensity

    float t = musicTime * speed * sweepSpeed;
    vec2 p = vec2((uv.x - 0.5) * aspect + 0.5, uv.y);

    // Single-line layout: all chars on one row, scale to fit width
    float cH = 0.18 * textScale;
    if (aspect < 1.0) cH *= aspect;
    float cW = cH * (5.0/7.0);
    float gW = cW * 0.2;

    // Scale down if text is wider than screen
    float totalTextW = float(numChars) * cW + float(numChars - 1) * gW;
    float maxW = 0.9 * aspect;
    float fitScale = totalTextW > maxW ? maxW / totalTextW : 1.0;
    cH *= fitScale;
    cW *= fitScale;
    gW *= fitScale;

    // Size breathing: the word swells with bass, a touch with mids —
    // a continuous, unclippable response (text brightness is already HDR).
    float sizePulse = 1.0 + 0.10 * bassP + 0.05 * midP;
    cH *= sizePulse;
    cW *= sizePulse;
    gW *= sizePulse;

    float rowW = float(numChars) * cW + float(numChars - 1) * gW;
    float startX = 0.5 - rowW * 0.5;
    float startY = 0.5 - cH * 0.5;

    float si = floor(uv.y * sliceCount);
    float n1 = hash(si + floor(t*2.0));
    float n2 = hash(si*3.7 + floor(t*3.0));

    float textHit = 0.0;

    float sw = sin(t*0.7)*0.5+0.5;
    float ps = smoothstep(sw-0.15, sw+0.1, (p.x-startX)/max(rowW, 0.001));

    float dx = abs(ps*n1*glitchAmount*maxDisp + ps*sin(si*0.3*complexity+t)*glitchAmount*maxDisp*0.3);
    float dy = vertGlitch > 0.01 ? ps*(n2-0.5)*vertGlitch*glitchAmount*0.06 : 0.0;

    vec2 samp = vec2(p.x - dx, p.y - dy);
    float rx = samp.x - startX, ry = samp.y - startY;

    if (rx >= 0.0 && rx <= rowW && ry >= 0.0 && ry <= cH) {
        float cs = cW + gW;
        float csF = rx / cs;
        int slot = int(floor(csF));
        float clx = fract(csF), cf = cW/cs;
        if (clx < cf && slot >= 0 && slot < numChars) {
            float gc = (clx/cf)*5.0, gr = (ry/cH)*7.0;
            if (gc >= 0.0 && gc < 5.0 && gr >= 0.0 && gr < 7.0) {
                int ch = getChar(slot);
                if (ch >= 0 && ch <= 36 && ch != 26) textHit = max(textHit, charPixel(ch, gc, gr));
            }
        }
    }

    // Secondary detail layer: cheap drifting digital dust fills the negative
    // space so the field is never mostly-empty. Highs sparsen/brighten it
    // into occasional sparkle; kept subtle so it never competes with text.
    float dustA = hash(floor(p.x * 90.0) + floor(p.y * 60.0) * 61.0 + floor(t * 0.6) * 911.0);
    float dustB = hash(floor(p.x * 150.0) - floor(p.y * 100.0) * 37.0 - floor(t * 0.4) * 401.0);
    float dust = dustA * 0.5 + dustB * 0.5;
    float sparkle = step(0.94, dust + highP * 0.1) * highP;
    // Dust density follows mids + bass continuously (whole negative space
    // breathes with beat-less material too). Silence ≈ the old 0.18 look.
    float dustFill = dust * (0.15 + 0.14 * midP + 0.10 * bassP) + sparkle * 0.6;
    vec3 dustTint = vec3(0.25, 0.55, 0.8); // cool cyan dust — matches the glitch/chromatic identity

    // Contour rim around glyph edges (edges axis) — reuses the already
    // anti-aliased textHit mask, no extra samples.
    float rim = smoothstep(0.0, 0.2, textHit) - smoothstep(0.2, 0.55, textHit);
    vec3 rimCol = mix(vec3(0.35, 0.85, 1.0), vec3(1.0, 0.35, 0.75), 0.5 + 0.5 * sin(si * 0.7 + t * 1.3));

    // Beat pulse: glyphs briefly brighten on strong hits, easing back (event-only).
    float kick = audioBeatPulse * audioBeatPulse;

    vec3 tCol = textColor.rgb * hdrGlow * (1.0 + kick * 0.3);
    vec3 bgDetailed = bgOverride + dustTint * dustFill;
    vec3 fc = mix(bgDetailed, tCol, textHit) + rimCol * rim * (0.25 + 0.35 * glitchAmount + 0.30 * midP);
    float a = 1.0;
    if (transparentBg) { a = max(textHit, dustFill * 0.35 + rim * 0.4); }
    return vec4(fc, a);
}

// =======================================================================
// MAIN
// =======================================================================

void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    int p = int(preset);
    // Under transparentBg the plasma still peeks through at low amplitude —
    // ambient texture for the negative space; true transparency is carried
    // by alpha (below), so this doesn't affect real overlay compositing.
    vec3 animBg = transparentBg ? mix(bgColor.rgb, plasmaBg(uv), 0.16) : plasmaBg(uv);
    vec4 col = effectDigifade(uv, p, animBg);

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
        vec4 cR = effectDigifade(uvR, p, plasmaBg(uvR));
        vec4 cG = effectDigifade(uvG, p, plasmaBg(uvG));
        vec4 cB = effectDigifade(uvB, p, plasmaBg(uvB));
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

    gl_FragColor = vec4(ucApply(col.rgb), col.a);
}

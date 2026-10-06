/*{
  "CATEGORIES": [
    "Generator",
    "Art Movement",
    "Audio Reactive"
  ],
  "DESCRIPTION": "Pop Art × Bauhaus 3D — Lichtenstein Ben-Day dots and hard outlines fused with Kandinsky Lissajous-orbiting geometric solids. Shapes float in perspective-projected 3D space, casting halo glows. Five mood panels cross-fade. Bass springs shapes outward, treble shimmers the dot field. Five-colour Lichtenstein palette plus Kandinsky primaries.",
  "INPUTS": [
    {
      "NAME": "moodOverride",
      "LABEL": "Mood",
      "TYPE": "long",
      "DEFAULT": -1,
      "VALUES": [
        -1,
        0,
        1,
        2,
        3,
        4
      ],
      "LABELS": [
        "Auto Cycle",
        "Whaam!",
        "Drowning Girl",
        "Crying Girl",
        "Sunrise",
        "Bauhaus"
      ]
    },
    {
      "NAME": "speechBubble",
      "LABEL": "Speech Bubble",
      "TYPE": "bool",
      "DEFAULT": false
    },
    {
      "NAME": "haloStrength",
      "LABEL": "Halo Strength",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.6
    },
    {
      "NAME": "compositionSeed",
      "LABEL": "Seed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 50,
      "DEFAULT": 0
    },
    {
      "NAME": "dotDensity",
      "LABEL": "Ben-Day Density",
      "TYPE": "float",
      "MIN": 30,
      "MAX": 160,
      "DEFAULT": 72,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "dotRadius",
      "LABEL": "Dot Radius",
      "TYPE": "float",
      "MIN": 0.2,
      "MAX": 0.48,
      "DEFAULT": 0.34,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "outlineWeight",
      "LABEL": "Outline Weight",
      "TYPE": "float",
      "MIN": 0.001,
      "MAX": 0.012,
      "DEFAULT": 0.0042,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "shapeCount",
      "LABEL": "Shape Count",
      "TYPE": "float",
      "MIN": 3,
      "MAX": 20,
      "DEFAULT": 11,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "shapeSize",
      "LABEL": "Shape Size",
      "TYPE": "float",
      "MIN": 0.03,
      "MAX": 0.18,
      "DEFAULT": 0.08,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "lineCount",
      "LABEL": "Support Lines",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 14,
      "DEFAULT": 7,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "panelDuration",
      "LABEL": "Panel Seconds",
      "TYPE": "float",
      "MIN": 4,
      "MAX": 20,
      "DEFAULT": 9,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "orbitSpeed",
      "LABEL": "Orbit Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.5,
      "DEFAULT": 0.3,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "orbitRange",
      "LABEL": "Orbit Range",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.45,
      "DEFAULT": 0.2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "wanderAmt",
      "LABEL": "Home Wander",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.4,
      "DEFAULT": 0.08,
      "GROUP": "Motion"
    },
    {
      "NAME": "depthSwing",
      "LABEL": "Depth Swing",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.5,
      "DEFAULT": 0.6,
      "GROUP": "Motion"
    },
    {
      "NAME": "tumbleRate",
      "LABEL": "Shape Tumble",
      "TYPE": "float",
      "MIN": -1,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Motion"
    },
    {
      "NAME": "lineSway",
      "LABEL": "Line Sway",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.5,
      "GROUP": "Motion"
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
      "NAME": "depth3D",
      "LABEL": "3D Depth",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.65,
      "GROUP": "Camera / Layout"
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
      "NAME": "springReact",
      "LABEL": "Bass Spring",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.5,
      "DEFAULT": 0.14,
      "GROUP": "Audio Reactivity"
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

// ═══════════════════════════════════════════════════════════════════════
//  POP ART × BAUHAUS 3D
//  Lichtenstein panels + Kandinsky Lissajous 3D solids, single pass.
// ═══════════════════════════════════════════════════════════════════════

// ─── PALETTE ──────────────────────────────────────────────────────────
const vec3 LL_YELLOW = vec3(0.98, 0.85, 0.10);
const vec3 LL_RED    = vec3(0.92, 0.18, 0.16);
const vec3 LL_CYAN   = vec3(0.10, 0.55, 0.82);
const vec3 LL_WHITE  = vec3(0.96, 0.94, 0.88);
const vec3 LL_BLACK  = vec3(0.04, 0.04, 0.06);
const vec3 K_BLUE    = vec3(0.10, 0.18, 0.70);

// ─── HASH / NOISE ─────────────────────────────────────────────────────
float hash11(float n) { return fract(sin(n * 12.9898) * 43758.5453); }
float hash12(vec2 p)  { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }

// ─── SDF helpers ──────────────────────────────────────────────────────
float sdCircle(vec2 p, float r)     { return length(p) - r; }
float sdRect(vec2 p, vec2 b)        { vec2 d = abs(p) - b; return max(d.x, d.y); }
float sdRoundRect(vec2 p, vec2 b, float r) {
    vec2 d = abs(p) - b + vec2(r);
    return min(max(d.x, d.y), 0.0) + length(max(d, 0.0)) - r;
}
float sdSegment(vec2 p, vec2 a, vec2 b, float w) {
    vec2 pa = p - a, ba = b - a;
    float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
    return length(pa - ba * h) - w;
}
float sdBox2(vec2 p, float r) {
    vec2 d = abs(p) - r;
    return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
}
float sdTriangle2(vec2 p, float r) {
    float k = 1.7320508;
    p.x = abs(p.x) - r;
    p.y = p.y + r / k;
    if (p.x + k * p.y > 0.0) p = vec2(p.x - k * p.y, -k * p.x - p.y) * 0.5;
    p.x -= clamp(p.x, -2.0 * r, 0.0);
    return -length(p) * sign(p.y);
}

// ─── Ben-Day halftone ─────────────────────────────────────────────────
float benDay(vec2 uv, float density, float radius, float aspect, float jitter) {
    vec2 g = vec2(uv.x * aspect, uv.y) * density;
    float row = floor(g.y);
    g.x += 0.5 * mod(row, 2.0);
    vec2 cell = fract(g) - 0.5;
    float r = radius + jitter * 0.05 * sin(row * 1.91 + g.x * 3.0);
    return smoothstep(r, r - 0.04, length(cell));
}

// ─── BLOCK-LETTER GLYPHS ──────────────────────────────────────────────
float bar(vec2 p, vec4 r) {
    return step(r.x, p.x) * step(p.x, r.z) * step(r.y, p.y) * step(p.y, r.w);
}
float gW(vec2 p) {
    return max(max(bar(p, vec4(0.00,0.05,0.20,0.95)), bar(p, vec4(0.80,0.05,1.00,0.95))),
               max(bar(p, vec4(0.32,0.05,0.48,0.55)), bar(p, vec4(0.52,0.05,0.68,0.55))));
}
float gH(vec2 p) {
    return max(max(bar(p,vec4(0.00,0.05,0.20,0.95)),bar(p,vec4(0.80,0.05,1.00,0.95))),
               bar(p,vec4(0.20,0.42,0.80,0.58)));
}
float gA(vec2 p) {
    return max(max(bar(p,vec4(0.00,0.05,0.20,0.85)),bar(p,vec4(0.80,0.05,1.00,0.85))),
               max(bar(p,vec4(0.05,0.85,0.95,1.00)),bar(p,vec4(0.20,0.42,0.80,0.55))));
}
float gM(vec2 p) {
    return max(max(bar(p,vec4(0.00,0.05,0.20,1.00)),bar(p,vec4(0.80,0.05,1.00,1.00))),
               max(max(bar(p,vec4(0.20,0.55,0.40,0.95)),bar(p,vec4(0.60,0.55,0.80,0.95))),
                   bar(p,vec4(0.40,0.45,0.60,0.75))));
}
float gP(vec2 p) {
    return max(max(bar(p,vec4(0.00,0.05,0.20,1.00)),bar(p,vec4(0.20,0.85,0.85,1.00))),
               max(bar(p,vec4(0.20,0.50,0.85,0.65)),bar(p,vec4(0.80,0.50,1.00,1.00))));
}
float gO(vec2 p) {
    return bar(p,vec4(0.00,0.05,1.00,0.95)) * (1.0 - bar(p,vec4(0.22,0.25,0.78,0.75)));
}
float gB(vec2 p) {
    return max(max(max(bar(p,vec4(0.00,0.05,0.20,1.00)),bar(p,vec4(0.20,0.85,0.82,1.00))),
                   max(bar(p,vec4(0.20,0.45,0.82,0.58)),bar(p,vec4(0.20,0.05,0.82,0.20)))),
               max(bar(p,vec4(0.82,0.55,1.00,0.92)),bar(p,vec4(0.82,0.13,1.00,0.50))));
}
float gL(vec2 p) {
    return max(bar(p,vec4(0.00,0.05,0.20,1.00)),bar(p,vec4(0.20,0.05,0.95,0.20)));
}
float gI(vec2 p) { return bar(p,vec4(0.40,0.05,0.60,1.00)); }
float gT(vec2 p) {
    return max(bar(p,vec4(0.00,0.85,1.00,1.00)),bar(p,vec4(0.40,0.05,0.60,0.85)));
}
float gS(vec2 p) {
    return max(max(max(bar(p,vec4(0.00,0.85,1.00,1.00)),bar(p,vec4(0.00,0.55,0.20,0.85))),
                   max(bar(p,vec4(0.00,0.42,1.00,0.58)),bar(p,vec4(0.80,0.20,1.00,0.45)))),
               bar(p,vec4(0.00,0.05,1.00,0.20)));
}
float gE(vec2 p) {
    return max(max(bar(p,vec4(0.00,0.05,0.20,1.00)),bar(p,vec4(0.20,0.85,1.00,1.00))),
               max(bar(p,vec4(0.20,0.42,0.85,0.58)),bar(p,vec4(0.20,0.05,1.00,0.20))));
}
float gR(vec2 p) {
    return max(max(max(bar(p,vec4(0.00,0.05,0.20,1.00)),bar(p,vec4(0.20,0.85,0.82,1.00))),
                   max(bar(p,vec4(0.20,0.45,0.82,0.58)),bar(p,vec4(0.82,0.50,1.00,0.95)))),
               max(bar(p,vec4(0.30,0.05,0.55,0.45)),bar(p,vec4(0.70,0.05,1.00,0.45))));
}
float gV(vec2 p) {
    return max(max(bar(p,vec4(0.00,0.40,0.20,1.00)),bar(p,vec4(0.80,0.40,1.00,1.00))),
               max(bar(p,vec4(0.20,0.05,0.45,0.45)),bar(p,vec4(0.55,0.05,0.80,0.45))));
}
float gExcl(vec2 p) {
    return max(bar(p,vec4(0.40,0.30,0.60,1.00)),bar(p,vec4(0.40,0.00,0.60,0.18)));
}
float gDot(vec2 p)  { return bar(p,vec4(0.40,0.00,0.60,0.18)); }

float renderGlyph(int g, vec2 p) {
    if (p.x < 0.0 || p.x > 1.0 || p.y < 0.0 || p.y > 1.0) return 0.0;
    if (g == 0)  return gW(p);
    if (g == 1)  return gH(p);
    if (g == 2)  return gA(p);
    if (g == 3)  return gM(p);
    if (g == 4)  return gP(p);
    if (g == 5)  return gO(p);
    if (g == 6)  return gB(p);
    if (g == 7)  return gL(p);
    if (g == 8)  return gI(p);
    if (g == 9)  return gT(p);
    if (g == 10) return gS(p);
    if (g == 11) return gE(p);
    if (g == 12) return gR(p);
    if (g == 13) return gV(p);
    if (g == 14) return gExcl(p);
    if (g == 15) return gDot(p);
    return 0.0;
}

int wordGlyph(int word, int i) {
    if (word == 0) { if(i==0)return 0; if(i==1)return 1; if(i==2)return 2; if(i==3)return 2; if(i==4)return 3; return 14; }
    if (word == 1) { if(i==0)return 4; if(i==1)return 5; if(i==2)return 0; return 14; }
    if (word == 2) { if(i==0)return 6; if(i==1)return 7; if(i==2)return 2; if(i==3)return 3; return 14; }
    if (word == 3) { if(i==0)return 5; if(i==1)return 1; return 15; }
    if(i==0)return 8; if(i==1)return 9; if(i==2)return 10; if(i==3)return 5;
    if(i==4)return 13; if(i==5)return 11; if(i==6)return 12; return 14;
}
int wordLength(int word) {
    if (word == 0) return 6;
    if (word == 1) return 4;
    if (word == 2) return 5;
    if (word == 3) return 5;
    return 8;
}
float drawWord(vec2 uv, vec2 origin, float width, float h, float aspect, int word) {
    int n = wordLength(word);
    if (n <= 0) return 0.0;
    float cellW = width / float(n);
    vec2 d = uv - origin;
    d.x *= aspect;
    float fx = d.x / cellW + float(n) * 0.5;
    int idx = int(floor(fx));
    if (idx < 0 || idx >= n) return 0.0;
    vec2 lp = vec2(fract(fx), (d.y + h * 0.5) / h);
    return renderGlyph(wordGlyph(word, idx), lp);
}

// ─── SUBJECTS (Lichtenstein mood panels) ─────────────────────────────
struct Subject { vec3 base; float outline; float dotRegion; vec3 dotColor; };

Subject subjWhaam(vec2 uv, float aspect, float t, float bass) {
    Subject S;
    vec2 p = (uv - vec2(0.55, 0.50)); p.x *= aspect;
    float ang = atan(p.y, p.x); float r = length(p);
    float burst = 0.30 + 0.06*cos(ang*9.0) + 0.02*cos(ang*17.0+t*0.8) + 0.04*bass;
    float inBurst = step(r, burst); float inCore = step(r, burst*0.55);
    float horizon = step(uv.y, 0.32 + 0.04*sin(uv.x*6.0));
    S.base = mix(LL_CYAN, LL_RED, horizon*0.4);
    S.base = mix(S.base, LL_YELLOW, inBurst);
    S.base = mix(S.base, LL_RED, inCore);
    float rim = abs(r - burst);
    S.outline = step(rim, 0.012)*(1.0-inCore);
    S.outline = max(S.outline, step(abs(r - burst*0.55), 0.010));
    S.dotRegion = (1.0-inBurst)*(1.0-horizon);
    S.dotColor = LL_RED;
    return S;
}
Subject subjDrowning(vec2 uv, float aspect, float t, float bass) {
    Subject S;
    float w = 0.50 + 0.10*sin(uv.x*7.0+t*0.5) + 0.05*sin(uv.x*13.0-t*0.3);
    float water = step(uv.y, w);
    float foam = smoothstep(0.012,0.0,abs(uv.y-w))
               + smoothstep(0.008,0.0,abs(uv.y-(w-0.06-0.02*sin(uv.x*11.0))));
    vec2 fp = (uv - vec2(0.62,0.66)); fp.x *= aspect;
    float face = sdCircle(fp*vec2(0.85,1.0), 0.18); float inFace = step(face,0.0);
    float tear = sdCircle((uv - vec2(0.58, 0.55+0.02*sin(t)))*vec2(aspect,2.5), 0.012);
    S.base = mix(LL_WHITE, LL_CYAN, water);
    S.base = mix(S.base, LL_WHITE, foam);
    S.base = mix(S.base, LL_YELLOW, inFace);
    S.base = mix(S.base, LL_CYAN, step(tear,0.0));
    S.outline = step(abs(face),0.008);
    S.outline = max(S.outline, smoothstep(0.004,0.0,abs(uv.y-w))*(1.0-inFace));
    S.dotRegion = water*(1.0-inFace)*(1.0-foam);
    S.dotColor = LL_WHITE;
    S.base = mix(S.base, LL_WHITE, bass*0.05);
    return S;
}
Subject subjCrying(vec2 uv, float aspect, float t, float bass) {
    Subject S;
    float hair = step(uv.y, 0.62 + 0.08*sin(uv.x*4.0+1.2));
    vec2 lp = (uv - vec2(0.50,0.30)); lp.x *= aspect;
    float lips = sdRect(lp*vec2(1.0,3.5), vec2(0.10,0.04)); float inLips = step(lips,0.0);
    vec2 tp = (uv - vec2(0.60, 0.42-0.10*fract(t*0.4))); tp.x *= aspect;
    float tear = sdCircle(tp*vec2(1.0,1.4), 0.018); float inTear = step(tear,0.0);
    vec2 ep = (uv - vec2(0.58,0.46)); ep.x *= aspect;
    float eye = abs(length(ep*vec2(1.0,2.2)) - 0.05);
    S.base = LL_WHITE;
    S.base = mix(S.base, LL_YELLOW, hair);
    S.base = mix(S.base, LL_WHITE, step(uv.y,0.45)*(1.0-hair));
    S.base = mix(S.base, LL_RED, inLips);
    S.base = mix(S.base, LL_CYAN, inTear);
    S.outline = smoothstep(0.006,0.0,abs(uv.y-(0.62+0.08*sin(uv.x*4.0+1.2))));
    S.outline = max(S.outline, step(abs(lips),0.006));
    S.outline = max(S.outline, step(abs(tear),0.006));
    S.outline = max(S.outline, step(eye,0.005));
    S.dotRegion = hair;
    S.dotColor = LL_RED;
    return S;
}
Subject subjSunrise(vec2 uv, float aspect, float t, float bass) {
    Subject S;
    vec2 p = (uv - vec2(0.50,0.10)); p.x *= aspect;
    float ang = atan(p.y, max(p.x,0.0001)); float r = length(p);
    float sun = step(r, 0.18+0.02*bass);
    float ray = step(0.0, sin(ang*11.0+t*0.2));
    float aboveHorizon = step(0.10, uv.y);
    S.base = LL_CYAN;
    S.base = mix(S.base, LL_YELLOW, aboveHorizon*(1.0-ray));
    S.base = mix(S.base, LL_RED, aboveHorizon*ray);
    S.base = mix(S.base, LL_YELLOW, sun);
    S.outline = step(abs(r-0.18),0.008);
    S.outline = max(S.outline, smoothstep(0.005,0.0,abs(uv.y-0.10)));
    S.dotRegion = (1.0-aboveHorizon);
    S.dotColor = LL_RED;
    return S;
}
Subject subjBauhaus(vec2 uv, float aspect, float t, float bass) {
    Subject S;
    float band = floor(uv.x*3.0);
    vec3 field = (band < 0.5) ? LL_YELLOW : ((band < 1.5) ? LL_RED : LL_CYAN);
    S.base = field;
    vec2 cp = (uv - vec2(0.50,0.50)); cp.x *= aspect;
    float disc = sdCircle(cp, 0.16); float inDisc = step(disc,0.0);
    S.base = mix(S.base, LL_BLACK, inDisc);
    S.outline = step(abs(disc),0.006);
    S.dotRegion = (1.0-inDisc);
    S.dotColor = (band < 0.5) ? LL_RED : LL_WHITE;
    S.outline = max(S.outline, step(abs(disc - bass*0.02),0.004));
    return S;
}
Subject subjectByMood(int mood, vec2 uv, float aspect, float t, float bass) {
    if (mood == 0) return subjWhaam   (uv, aspect, t, bass);
    if (mood == 1) return subjDrowning(uv, aspect, t, bass);
    if (mood == 2) return subjCrying  (uv, aspect, t, bass);
    if (mood == 3) return subjSunrise (uv, aspect, t, bass);
    return             subjBauhaus (uv, aspect, t, bass);
}
vec3 renderPanel(int mood, vec2 uv, float aspect, float t,
                 float bass, float treble,
                 float densityV, float radiusV) {
    Subject S = subjectByMood(mood, uv, aspect, t, bass);
    vec3 c = S.base;
    float dots = benDay(uv, densityV, radiusV, aspect, treble);
    c = mix(c, S.dotColor, dots * S.dotRegion);
    c = mix(c, LL_BLACK, clamp(S.outline,0.0,1.0));
    return c;
}

// ─── 3D PERSPECTIVE PROJECTION ───────────────────────────────────────
vec2 project3D(vec3 pos, float cameraZ) {
    float iz = 1.0 / max(cameraZ - pos.z, 0.01);
    return pos.xy * (cameraZ * iz);
}

void main() {
    vec2 uv = isf_FragNormCoord.xy;
    float aspect = RENDERSIZE.x / max(RENDERSIZE.y, 1.0);
    float t = TIME;

    float bass   = clamp(audioBass,  0.0, 1.0) * audioReact;
    float mid    = clamp(audioMid,   0.0, 1.0) * audioReact;
    float treble = clamp(audioHigh,  0.0, 1.0) * audioReact;
    float level  = clamp(audioLevel, 0.0, 1.0) * audioReact;

    // ─── Panel selection ─────────────────────────────────────────────
    const int   N_MOODS           = 5;
    const float TRANSITION_SECS   = 1.5;

    int   moodA, moodB;
    float blend;
    if (moodOverride < -0.5) {
        float dur   = max(panelDuration, 0.5);
        float pT    = t / dur;
        float idxA  = floor(pT);
        moodA  = int(mod(idxA,        float(N_MOODS)));
        moodB  = int(mod(idxA + 1.0,  float(N_MOODS)));
        float local = fract(pT) * dur;
        float startB = max(dur - TRANSITION_SECS, 0.0);
        blend = smoothstep(0.0, 1.0,
                  clamp((local - startB) / max(TRANSITION_SECS, 0.0001), 0.0, 1.0));
    } else {
        moodA = int(moodOverride);
        moodB = moodA;
        blend = 0.0;
    }

    // ─── Render Lichtenstein base panels ─────────────────────────────
    vec3 colA = renderPanel(moodA, uv, aspect, t, bass, treble, dotDensity, dotRadius);
    vec3 col  = colA;
    if (blend > 0.001) {
        vec3 colB = renderPanel(moodB, uv, aspect, t, bass, treble, dotDensity, dotRadius);
        col = mix(colA, colB, blend);
    }

    // ─── KANDINSKY 3D FLOATING SHAPES ────────────────────────────────
    vec2 Psc = vec2((uv.x - 0.5) * aspect, uv.y - 0.5);

    float cameraZ = 2.2;
    float zRange  = depth3D * 0.85;

    int N = int(clamp(shapeCount, 1.0, 20.0));

    vec3 shapeColA = LL_YELLOW;
    vec3 shapeColB = LL_RED;
    vec3 shapeColC = LL_CYAN;
    if (moodA == 1 || moodA == 2) { shapeColC = K_BLUE; }
    if (moodA == 3) { shapeColA = LL_RED; shapeColB = LL_YELLOW; shapeColC = LL_CYAN; }

    // Pass 1 — halo glow (additive, soft)
    float haloField = 0.0;
    vec3  haloCol   = vec3(0.0);
    float haloWt    = 0.0;

    for (int i = 0; i < 20; i++) {
        if (i >= N) break;
        float fi = float(i) + compositionSeed * 1.71;

        float phA  = fi * 2.399;
        float phB  = fi * 1.618;
        float spd  = orbitSpeed * (0.5 + hash11(fi * 3.1) * 1.2);
        float ox   = (hash11(fi * 1.3) - 0.5) * 0.7 * aspect;
        float oy   = (hash11(fi * 2.7) - 0.5) * 0.7;
        float oz   = (hash11(fi * 4.1) - 0.5) * 2.0 * zRange;

        vec3 home = vec3(ox + wanderAmt*sin(t*0.05 + fi),
                         oy + wanderAmt*cos(t*0.04 + fi*1.3),
                         oz + zRange*0.4*sin(t*0.06 + fi*0.77));

        vec3 orbit3 = vec3(
            sin(t * spd + phA) * orbitRange * aspect,
            cos(t * spd * 0.7 + phB * 1.7) * orbitRange,
            sin(t * spd * 0.43 + fi * 1.11) * zRange * depthSwing
        );

        vec3 pos3 = home + orbit3;
        vec2 fromCtr2 = pos3.xy;
        float fcLen = length(fromCtr2);
        if (fcLen > 1e-4) fromCtr2 = fromCtr2 / fcLen;
        pos3.xy += fromCtr2 * springReact * bass;

        vec2 ctr2D = project3D(pos3, cameraZ);

        float zFactor  = clamp((cameraZ - pos3.z) / cameraZ, 0.3, 2.5);
        float sz = shapeSize * (0.7 + hash11(fi * 5.3) * 0.6)
                 * zFactor
                 * (1.0 + level * 0.08);

        float rDist = length(Psc - ctr2D);
        float halo  = exp(-pow(rDist / (sz * 2.2), 2.0));
        haloField += halo;

        int shapeType = int(mod(fi, 3.0));
        vec3 sc = (shapeType == 0) ? shapeColA
                : (shapeType == 1) ? shapeColB : shapeColC;
        haloCol += sc * halo;
        haloWt  += halo;
    }
    if (haloWt > 1e-4) {
        haloCol /= haloWt;
        col = mix(col, haloCol,
                  clamp(haloField * haloStrength * 0.38, 0.0, 0.72));
    }

    // Pass 2 — solid shapes (closest-wins, with outlines + Ben-Day fill)
    float bestSD  = 1e9;
    vec3  bestCol = col;
    float bestSz  = 0.05;
    int   bestType = 0;
    vec3  bestSC   = LL_YELLOW;

    for (int i = 0; i < 20; i++) {
        if (i >= N) break;
        float fi = float(i) + compositionSeed * 1.71;

        float phA = fi * 2.399; float phB = fi * 1.618;
        float spd = orbitSpeed * (0.5 + hash11(fi*3.1)*1.2);
        float ox  = (hash11(fi*1.3)-0.5)*0.7*aspect;
        float oy  = (hash11(fi*2.7)-0.5)*0.7;
        float oz  = (hash11(fi*4.1)-0.5)*2.0*zRange;

        vec3 home = vec3(ox + wanderAmt*sin(t*0.05+fi),
                         oy + wanderAmt*cos(t*0.04+fi*1.3),
                         oz + zRange*0.4*sin(t*0.06+fi*0.77));
        vec3 orbit3 = vec3(
            sin(t*spd+phA)*orbitRange*aspect,
            cos(t*spd*0.7+phB*1.7)*orbitRange,
            sin(t*spd*0.43+fi*1.11)*zRange*depthSwing
        );
        vec3 pos3 = home + orbit3;
        vec2 fromCtr2 = pos3.xy;
        float fcLen = length(fromCtr2);
        if (fcLen > 1e-4) fromCtr2 = fromCtr2/fcLen;
        pos3.xy += fromCtr2 * springReact * bass;

        vec2 ctr2D = project3D(pos3, cameraZ);

        float zFactor = clamp((cameraZ - pos3.z)/cameraZ, 0.3, 2.5);
        float sz = shapeSize*(0.7+hash11(fi*5.3)*0.6)*zFactor*(1.0+level*0.08);

        // Fixed orientation per shape; tumbleRate (0 = off) spins each solid
        // at its own hashed rate — triangles, boxes and checker fills read it
        float rot = hash11(fi*7.7) * 6.2832 + t * tumbleRate * (0.4 + hash11(fi*9.1) * 1.2);
        float ca = cos(-rot), sa = sin(-rot);
        vec2 lp = Psc - ctr2D;
        lp = vec2(ca*lp.x - sa*lp.y, sa*lp.x + ca*lp.y);

        int shapeType = int(mod(fi, 3.0));
        float sd = (shapeType == 0) ? sdTriangle2(lp, sz)
                 : (shapeType == 1) ? sdBox2(lp, sz)
                 : sdCircle(lp, sz);

        if (sd < bestSD) {
            bestSD   = sd;
            bestSz   = sz;
            bestType = shapeType;
            bestSC   = (shapeType == 0) ? shapeColA
                     : (shapeType == 1) ? shapeColB : shapeColC;

            vec2  chk   = floor(lp / max(sz*0.30, 1e-4));
            bool  chkOn = (mod(chk.x+chk.y, 2.0) < 1.0);
            bool  useChk= (mod(fi, 4.0) >= 3.0);
            bestCol = useChk ? (chkOn ? bestSC : LL_BLACK) : bestSC;
        }
    }

    if (bestSD < 0.0) {
        float shapeDots = benDay(uv, dotDensity * 1.1, dotRadius * 0.75, aspect, treble * 0.5);
        vec3 dotCol = mix(bestCol, LL_BLACK, 0.85);
        vec3 filled = mix(bestCol, dotCol, shapeDots * 0.55);
        col = mix(col, filled, 1.0);
    } else if (bestSD < bestSz * 0.12) {
        float ot = 1.0 - smoothstep(0.0, outlineWeight * 2.0, bestSD);
        col = mix(col, LL_BLACK, ot);
    } else {
        float nearGlow = exp(-bestSD / (bestSz * 0.25)) * 0.18;
        col = mix(col, bestSC, nearGlow * haloStrength);
    }

    // ─── Support lines ────────────────────────────────────────────────
    int NL = int(clamp(lineCount, 0.0, 14.0));
    for (int k = 0; k < 14; k++) {
        if (k >= NL) break;
        float fk  = float(k) + compositionSeed * 0.71;
        float ang = hash11(fk*1.7)*6.2832 + sin(t*0.3+fk*1.3)*lineSway;
        vec2 dir  = vec2(cos(ang), sin(ang));
        vec2 pt   = vec2(hash11(fk*3.3), hash11(fk*5.1));
        pt += vec2(sin(t*0.4+fk), cos(t*0.32+fk*1.7))*0.05;
        vec2 d    = uv - pt;
        float perp= abs(d.x*(-dir.y) + d.y*dir.x);
        float lw  = outlineWeight*(0.6 + hash11(fk*7.13)*0.8);
        float lm  = smoothstep(lw, 0.0, perp);
        if (hash11(fk*11.7) > 0.55) {
            float along = d.x*dir.x + d.y*dir.y;
            float dash  = step(0.5, fract(along*26.0 + t*0.3));
            lm *= dash;
        }
        col = mix(col, LL_BLACK, lm*(0.5 + treble*0.4));
    }

    // ─── Bass explosion flash ─────────────────────────────────────────
    {
        vec2 fc = (uv - vec2(0.18, 0.82)); fc.x *= aspect;
        float fa = atan(fc.y, fc.x); float fr = length(fc);
        float flashR = 0.12 + 0.04*cos(fa*10.0);
        float flashAmt = step(fr, flashR) * bass;
        col = mix(col, LL_YELLOW, flashAmt * 0.85);
        col = mix(col, LL_BLACK,  step(abs(fr - flashR), 0.008)*bass);
    }

    // ─── Speech bubble ────────────────────────────────────────────────
    if (speechBubble) {
        int mood = moodA;
        vec2 bC;
        if      (mood == 0) bC = vec2(0.22, 0.78);
        else if (mood == 1) bC = vec2(0.25, 0.30);
        else if (mood == 2) bC = vec2(0.22, 0.82);
        else if (mood == 3) bC = vec2(0.78, 0.78);
        else                bC = vec2(0.50, 0.78);
        vec2 bD = uv - bC; bD.x *= aspect;
        float bSz = 0.13*(1.0+0.08*sin(t*1.6))*(1.0+bass*0.18);
        float bd  = sdRoundRect(bD, vec2(bSz, bSz*0.50), bSz*0.18);
        float inB = step(bd, 0.0);
        vec2 tF = vec2(-bSz*0.3,-bSz*0.45), tT = vec2(-bSz*0.85,-bSz*1.1);
        float tail = sdSegment(bD, tF, tT, bSz*0.06); float inT = step(tail, 0.0);
        col = mix(col, LL_WHITE, max(inB,inT));
        col = mix(col, LL_BLACK, max(step(abs(bd), outlineWeight*1.4),
                                     step(abs(tail),outlineWeight)*(1.0-inB)));
        int word = int(mod(floor(t / max(panelDuration*0.5,0.5)), 5.0));
        if (inB > 0.5) {
            float ink = drawWord(uv, bC, bSz*1.5, bSz*0.55, aspect, word);
            col = mix(col, LL_BLACK, ink);
        }
    }

    // ─── Treble shimmer extra dot pass ────────────────────────────────
    if (treble > 0.05) {
        float d2 = benDay(uv + vec2(0.5/max(dotDensity,1.0)),
                          dotDensity*1.4, dotRadius*0.5, aspect, treble);
        col = mix(col, LL_CYAN, d2*treble*0.10);
    }

    // ─── Malevich black-square Easter egg ────────────────────────────
    {
        float ph = fract(t / 35.0);
        float ef = smoothstep(0.0,0.04,ph)*smoothstep(0.18,0.10,ph);
        float which = floor(fract(t/35.0+0.5)*4.0);
        vec2 origin = vec2(which < 2.0 ? 0.06 : 0.74,
                           (mod(which,2.0)<1.0) ? 0.06 : 0.74);
        vec2 dq = uv - origin;
        float sq = step(0.0,dq.x)*step(dq.x,0.18)*step(0.0,dq.y)*step(dq.y,0.18);
        col = mix(col, LL_BLACK, ef*sq);
    }

    // ---- universal color block (defaults = no-op) ----
    float ucL = dot(col, vec3(0.299, 0.587, 0.114));
    vec3 uc = mix(vec3(ucL), col, colorBoost);
    if (hueShift > 0.0005) {
        float hueA = hueShift * 6.2831853;
        float hueC = cos(hueA), hueS = sin(hueA);
        mat3 hueM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                  + hueC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                  + hueS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hueM * uc, 0.0, 1.0);
    }
    uc = mix(uc, bgColor.rgb, bgColor.a * (1.0 - smoothstep(0.0, 0.35, ucL)));

    gl_FragColor = vec4(uc, 1.0);
}
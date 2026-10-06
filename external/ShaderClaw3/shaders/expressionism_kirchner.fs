/*{
  "CATEGORIES": [
    "Generator",
    "Art Movement",
    "Audio Reactive"
  ],
  "DESCRIPTION": "Abstract Paint Strokes × Pop Art Fusion — Dense HDR expressionist gestural brushstrokes (Kirchner/Brücke/Soutine) fused with Lichtenstein Ben-Day halftone dots, hard pop outlines, and Kandinsky 3D Lissajous-orbiting geometric solids. Woodcut-bone ink linework edges every stroke. Central angular figure silhouette with shadow double. Floating 3D shapes cast halo glows over the paint field. Bass springs shapes outward, treble shimmers dots, audio drives gesture and stroke width. Mood blends expressionist palette with pop-art panel colours. LINEAR HDR out.",
  "INPUTS": [
    {
      "NAME": "mood",
      "LABEL": "Expr. Mood",
      "TYPE": "long",
      "DEFAULT": 2,
      "VALUES": [
        0,
        1,
        2,
        3
      ],
      "LABELS": [
        "Soutine Twist",
        "Schiele Nervous Line",
        "Brücke Storm",
        "War Charcoal"
      ]
    },
    {
      "NAME": "showFigure",
      "LABEL": "Show Figure",
      "TYPE": "bool",
      "DEFAULT": true
    },
    {
      "NAME": "shadowDepth",
      "LABEL": "Shadow Double",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.6
    },
    {
      "NAME": "crisp",
      "LABEL": "Crispness",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2.5,
      "DEFAULT": 1.1
    },
    {
      "NAME": "dotMix",
      "LABEL": "Dot Mix",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.4
    },
    {
      "NAME": "haloStrength",
      "LABEL": "Halo Strength",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.55
    },
    {
      "NAME": "compositionSeed",
      "LABEL": "Shape Seed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 50,
      "DEFAULT": 0
    },
    {
      "NAME": "brushDensity",
      "LABEL": "Brush Density",
      "TYPE": "float",
      "MIN": 12,
      "MAX": 48,
      "DEFAULT": 28,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "figureX",
      "LABEL": "Figure X",
      "TYPE": "float",
      "MIN": -1,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "figureY",
      "LABEL": "Figure Y",
      "TYPE": "float",
      "MIN": -1,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "figureScale",
      "LABEL": "Figure Scale",
      "TYPE": "float",
      "MIN": 0.5,
      "MAX": 1.8,
      "DEFAULT": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "dotDensity",
      "LABEL": "Ben-Day Density",
      "TYPE": "float",
      "MIN": 20,
      "MAX": 140,
      "DEFAULT": 60,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "dotRadius",
      "LABEL": "Dot Radius",
      "TYPE": "float",
      "MIN": 0.15,
      "MAX": 0.48,
      "DEFAULT": 0.3,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "outlineWeight",
      "LABEL": "Pop Outline",
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
      "MIN": 0,
      "MAX": 20,
      "DEFAULT": 9,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "shapeSize",
      "LABEL": "Shape Size",
      "TYPE": "float",
      "MIN": 0.02,
      "MAX": 0.18,
      "DEFAULT": 0.07,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "gestureSpeed",
      "LABEL": "Gesture Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.7,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "orbitSpeed",
      "LABEL": "Orbit Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.5,
      "DEFAULT": 0.28,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "orbitRange",
      "LABEL": "Orbit Range",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.45,
      "DEFAULT": 0.18,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "paletteWarmth",
      "LABEL": "Palette Warmth",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.5,
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
      "NAME": "depth3D",
      "LABEL": "3D Depth",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.6,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "bgColor",
      "LABEL": "Background",
      "TYPE": "color",
      "DEFAULT": [
        0,
        0,
        0,
        0
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio React",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "springReact",
      "LABEL": "Bass Spring",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.5,
      "DEFAULT": 0.14,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

// ═══════════════════════════════════════════════════════════════
//  ABSTRACT PAINT × POP FUSION
//  Expressionist gestural strokes + Ben-Day dots + 3D Kandinsky shapes
// ═══════════════════════════════════════════════════════════════

// ── EXPRESSIONIST PALETTE ──────────────────────────────────────
const vec3 P_BONECREAM = vec3(0.94, 0.88, 0.74);
const vec3 P_RAWUMBER  = vec3(0.30, 0.18, 0.08);
const vec3 P_CHARCOAL  = vec3(0.02, 0.02, 0.03);
const vec3 P_INK       = vec3(0.005,0.005,0.010);
const vec3 P_CADRED    = vec3(0.92, 0.10, 0.06);
const vec3 P_VIRIDIAN  = vec3(0.04, 0.55, 0.34);
const vec3 P_PRUSSIAN  = vec3(0.03, 0.10, 0.65);
const vec3 P_CADORANGE = vec3(0.98, 0.42, 0.06);
const vec3 P_FLESH     = vec3(0.90, 0.42, 0.30);
const vec3 P_BLOODHOT  = vec3(3.20, 0.18, 0.06);

// ── POP / KANDINSKY PALETTE ────────────────────────────────────
const vec3 LL_YELLOW = vec3(0.98, 0.85, 0.10);
const vec3 LL_RED    = vec3(0.92, 0.18, 0.16);
const vec3 LL_CYAN   = vec3(0.10, 0.55, 0.82);
const vec3 LL_WHITE  = vec3(0.96, 0.94, 0.88);
const vec3 LL_BLACK  = vec3(0.04, 0.04, 0.06);
const vec3 K_BLUE    = vec3(0.10, 0.18, 0.70);

// ── HASH / NOISE ───────────────────────────────────────────────
float h11(float n) { return fract(sin(n * 12.9898) * 43758.5453); }
float h21(vec2 p)  { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }

float vnoise(vec2 p) {
    vec2 ip = floor(p), fp = fract(p);
    fp = fp * fp * (3.0 - 2.0 * fp);
    float a = h21(ip), b = h21(ip + vec2(1.0,0.0));
    float c = h21(ip + vec2(0.0,1.0)), d = h21(ip + vec2(1.0,1.0));
    return mix(mix(a,b,fp.x), mix(c,d,fp.x), fp.y);
}

float fbm(vec2 p) {
    float v = 0.0, a = 0.55;
    mat2 R = mat2(0.8,-0.6,0.6,0.8);
    for (int i = 0; i < 5; i++) {
        v += a * vnoise(p);
        p = R * p * 2.07 + vec2(7.3,1.9);
        a *= 0.55;
    }
    return v;
}

vec2 warp(vec2 p, float t, float k) {
    vec2 q = vec2(fbm(p + vec2(0.0, t*0.3)),
                  fbm(p + vec2(5.2,-t*0.27)));
    return p + (q - 0.5) * k;
}

// ── SDF HELPERS ────────────────────────────────────────────────
float sdCircle(vec2 p, float r)     { return length(p) - r; }
float sdBox2(vec2 p, float r)       { vec2 d = abs(p)-r; return length(max(d,0.0))+min(max(d.x,d.y),0.0); }
float sdTriangle2(vec2 p, float r) {
    float k = 1.7320508;
    p.x = abs(p.x) - r;
    p.y = p.y + r/k;
    if (p.x + k*p.y > 0.0) p = vec2(p.x - k*p.y, -k*p.x - p.y)*0.5;
    p.x -= clamp(p.x, -2.0*r, 0.0);
    return -length(p)*sign(p.y);
}

// ── BEN-DAY HALFTONE ───────────────────────────────────────────
float benDay(vec2 uv, float density, float radius, float aspect, float jitter) {
    vec2 g = vec2(uv.x * aspect, uv.y) * density;
    float row = floor(g.y);
    g.x += 0.5 * mod(row, 2.0);
    vec2 cell = fract(g) - 0.5;
    float r = radius + jitter * 0.05 * sin(row*1.91 + g.x*3.0);
    return smoothstep(r, r - 0.04, length(cell));
}

// ── AUDIO BANDS (simulated) ────────────────────────────────────
vec3 audioBands(float t, float a) {
    float bass = 0.5 + 0.5*sin(t*0.31);
    float mid  = 0.5 + 0.5*sin(t*0.83+1.7);
    float trb  = 0.5 + 0.5*sin(t*1.91+3.1);
    return vec3(bass,mid,trb) * (0.25 + 0.75*a);
}

// ── STROKE SDF ─────────────────────────────────────────────────
float strokeSDF(vec2 uv, vec2 c, float ang, float len, float wid, out vec2 local) {
    float ca = cos(ang), sa = sin(ang);
    vec2 d = uv - c;
    local = vec2(ca*d.x + sa*d.y, -sa*d.x + ca*d.y);
    vec2 q = abs(local) - vec2(len,wid);
    return length(max(q,0.0)) + min(max(q.x,q.y),0.0);
}

float brushCoverage(vec2 uv, vec2 c, float ang, float len, float wid,
                    float seed, float warpK, out float sdOut) {
    vec2 wuv = warp(uv*2.0, seed, warpK)*0.5;
    vec2 lp;
    float sd = strokeSDF(wuv, c, ang, len, wid, lp);
    sdOut = sd;
    float body = smoothstep(0.0,-0.004,sd);
    if (body <= 0.0) return 0.0;
    float endTaper = smoothstep(len, len*0.55, abs(lp.x));
    float bristles = sin((lp.y/max(wid,1e-4))*7.0 + lp.x*22.0 + seed*6.28);
    bristles = mix(0.78, 1.0, 0.5+0.5*bristles);
    float grain = fbm(lp*vec2(38.0,70.0)+seed*13.0);
    float erode = smoothstep(0.10, 0.55, grain);
    return body * endTaper * bristles * erode;
}

// ── PAINT STROKE LAYER ─────────────────────────────────────────
vec3 paintStroke(vec3 under, vec2 uv, float idx, float t, int moodI,
                 float density, float warmth, vec3 bands, float audio) {
    float s  = h11(idx*17.13+1.7);
    float s2 = h11(idx*31.7 +3.3);
    float s3 = h11(idx*9.91 +7.1);

    vec2 c = vec2(s,s2)*2.0 - 1.0;
    c.x *= 1.6;
    c += 0.10*vec2(sin(t*0.7+idx), cos(t*0.61+idx*1.3));

    float baseAng = mix(-1.4,1.4,s3);
    if (moodI==1) baseAng *= 1.15;
    if (moodI==0) baseAng += 0.3*sin(t*0.4+idx);
    if (moodI==3) baseAng = mix(baseAng, 0.0, 0.4);

    float len = 0.45 + 0.45*h11(idx*5.7);
    float wid = 0.045 + 0.075*h11(idx*11.3);
    if (moodI==1){ len*=0.85; wid*=0.70; }
    if (moodI==0){ len*=1.15; wid*=1.20; }
    if (moodI==2){ wid*=1.20; }
    if (moodI==3){ wid*=0.95; }
    len *= 0.85 + 0.5*bands.x*audio;
    wid *= 0.9  + 0.4*bands.y*audio;

    float warpK = (moodI==0) ? 0.55 : (moodI==1) ? 0.20 : 0.34;
    float sd;
    float cov = brushCoverage(uv, c, baseAng, len, wid, idx, warpK, sd);
    if (cov <= 0.0 && sd > 0.012) return under;

    vec3 col;
    float pickW = h11(idx*2.71);
    if (moodI==0) {
        if      (pickW<0.32) col=P_CADRED;
        else if (pickW<0.56) col=P_FLESH;
        else if (pickW<0.78) col=P_RAWUMBER;
        else                  col=P_VIRIDIAN;
    } else if (moodI==1) {
        if      (pickW<0.30) col=P_RAWUMBER;
        else if (pickW<0.55) col=P_CADRED;
        else if (pickW<0.78) col=P_PRUSSIAN;
        else                  col=P_CHARCOAL;
    } else if (moodI==2) {
        if      (pickW<0.28) col=P_CADRED;
        else if (pickW<0.50) col=P_VIRIDIAN;
        else if (pickW<0.70) col=P_PRUSSIAN;
        else if (pickW<0.86) col=P_CADORANGE;
        else                  col=P_CHARCOAL;
    } else {
        if      (pickW<0.55) col=P_CHARCOAL;
        else if (pickW<0.80) col=P_RAWUMBER;
        else if (pickW<0.95) col=P_INK;
        else                  col=P_CADRED;
    }

    col = mix(col, col*vec3(1.10,0.92,0.78), warmth*0.5);
    vec3 dragged = mix(col, under, 0.12+0.12*h11(idx*4.1));
    float edge = pow(1.0 - smoothstep(0.0,0.7,cov), 1.5);
    vec3 pigment = mix(col, dragged, edge*0.5);
    vec3 outc = mix(under, pigment, cov);
    float inkBand = smoothstep(0.014,0.000,abs(sd));
    inkBand *= smoothstep(-0.020,-0.002,sd);
    outc = mix(outc, P_INK, inkBand*0.85);
    return outc;
}

// ── HDR RED SLASHES ────────────────────────────────────────────
vec3 redSlash(vec3 under, vec2 uv, float t, int moodI, float audio) {
    int n = (moodI==3) ? 2 : (moodI==1) ? 3 : 4;
    vec3 col = under;
    for (int i = 0; i < 5; i++) {
        if (i >= n) break;
        float fi = float(i);
        vec2 c = vec2(0.7*sin(t*0.23+fi*2.1), 0.6*cos(t*0.31+fi*1.7));
        float ang = mix(-1.2,1.2,h11(fi*7.3+0.13))+0.2*sin(t*0.5+fi);
        float len = 0.55+0.18*sin(t*0.7+fi*2.0);
        float wid = 0.012+0.012*h11(fi*3.7);
        float sd;
        float cov = brushCoverage(uv, c, ang, len, wid, fi*11.0+91.0, 0.25, sd);
        vec3 hot = P_BLOODHOT*(1.0+0.6*audio);
        col = mix(col, hot, cov);
    }
    return col;
}

// ── CENTRAL FIGURE ─────────────────────────────────────────────
vec3 centralFigure(vec3 under, vec2 uv, float t, int moodI, float audio) {
    uv = (uv - vec2(figureX, figureY)) / max(figureScale, 0.01);
    float br   = 0.5+0.5*sin(t*1.3);
    float sway = 0.15*sin(t*0.9)+0.05*sin(t*1.7+1.1);
    float lean = 0.16*sin(t*0.7);
    float bob  = 0.04*sin(t*1.3+1.6);
    if (moodI==1){ sway+=0.035*sin(t*11.0); lean+=0.06*sin(t*9.3); }
    if (moodI==0){ lean+=0.13*sin(t*0.5); }
    if (moodI==2){ sway*=1.4; }
    if (moodI==3){ sway+=0.02*sin(t*17.0); bob+=0.015*sin(t*19.0); }
    float amp = 1.0+0.6*audio;
    sway*=amp; lean*=amp;
    vec2 p = uv;
    p.x -= sway; p.y -= bob;
    vec2 piv = vec2(0.0,-0.30);
    float cl = cos(lean), sl = sin(lean);
    vec2 pr = p - piv;
    p = piv + vec2(cl*pr.x-sl*pr.y, sl*pr.x+cl*pr.y);
    if (moodI==0)
        p += (vec2(fbm(p*3.0+t*0.6), fbm(p*3.0-t*0.5))-0.5)*0.10*amp;
    vec2 hp = p - vec2(0.0,0.18);
    if (moodI==1) hp.x += 0.18*hp.y;
    if (moodI==0) hp.x += 0.10*sin(hp.y*5.0);
    float headR = 0.55+0.05*sin(t*0.5)+0.03*br;
    float headD = abs(hp.x)*1.05+abs(hp.y)*0.85;
    if (hp.y < 0.0) headD = abs(hp.x)*1.25+abs(hp.y*1.4)*0.95;
    float head = smoothstep(headR+0.02, headR-0.02, headD);
    vec2 bp = p - vec2(0.0,-0.62);
    float bodyD = max(abs(bp.x)-(0.55+0.35*smoothstep(0.0,-0.6,bp.y)), abs(bp.y)-(0.40+0.05*br));
    float body = smoothstep(0.02,-0.02,bodyD);
    vec2 np = p - vec2(0.0,-0.30);
    float neckD = max(abs(np.x)-0.18, abs(np.y)-0.14);
    float neck = smoothstep(0.02,-0.02,neckD);
    float figMask = max(head, max(body,neck));
    if (figMask <= 0.0) return under;
    float edgeNoise = fbm(p*8.0+vec2(t*0.1,0.0));
    figMask *= mix(0.85,1.0,smoothstep(0.30,0.70,edgeNoise));
    vec3 figCol = P_INK;
    float gAng = (moodI==1) ? 1.2 : (moodI==3) ? 1.5 : 0.9;
    float gca = cos(gAng), gsa = sin(gAng);
    vec2 gp = p;
    float gv = -gsa*gp.x+gca*gp.y;
    float stripe = sin(gv*28.0+fbm(gp*4.0)*6.0);
    float gouge = smoothstep(0.55,0.85,stripe);
    float eyeY = 0.30;
    vec2 e1 = p - vec2(-0.18,eyeY);
    vec2 e2 = p - vec2( 0.18,eyeY);
    float eye = clamp(smoothstep(0.10,0.06,length(e1*vec2(1.0,1.6)))
                     +smoothstep(0.10,0.06,length(e2*vec2(1.0,1.6))), 0.0,1.0);
    vec2 mp = p - vec2(0.02*sin(t*0.4),-0.02);
    float mouthD = max(abs(mp.x)-0.22, abs(mp.y)-0.025);
    float mouth = smoothstep(0.005,-0.005,mouthD);
    vec3 gougeCol = P_CADRED*1.4;
    vec3 figureFill = mix(figCol, gougeCol, gouge*0.45);
    figureFill = mix(figureFill, P_BLOODHOT*(0.7+0.5*audio), mouth*0.95);
    figureFill = mix(figureFill, P_INK*0.0, eye*0.85);
    return mix(under, figureFill, figMask);
}

// ── SHADOW DOUBLE ──────────────────────────────────────────────
vec3 shadowDouble(vec3 under, vec2 uv, float t, int moodI, float amt) {
    if (amt <= 0.001) return under;
    vec2 off = vec2(figureX,figureY) + vec2(-0.17,0.11)
             + vec2(0.13*sin(t*0.9-0.5)+0.04*sin(t*1.7+0.6), 0.03*sin(t*1.3+1.0))
             + 0.05*vec2(sin(t*0.19+1.3), cos(t*0.17));
    float sc = max(figureScale,0.01)*1.35;
    vec2 p = (uv - off)/sc;
    vec2 hp = p - vec2(0.0,0.18);
    if (moodI==1) hp.x += 0.18*hp.y;
    float headR = 0.56;
    float headD = abs(hp.x)*1.05+abs(hp.y)*0.85;
    if (hp.y < 0.0) headD = abs(hp.x)*1.25+abs(hp.y*1.4)*0.95;
    float head = smoothstep(headR+0.11, headR-0.11, headD);
    vec2 bp = p - vec2(0.0,-0.62);
    float bodyD = max(abs(bp.x)-(0.55+0.35*smoothstep(0.0,-0.6,bp.y)), abs(bp.y)-0.40);
    float body = smoothstep(0.11,-0.11,bodyD);
    vec2 np = p - vec2(0.0,-0.30);
    float neckD = max(abs(np.x)-0.18, abs(np.y)-0.14);
    float neck = smoothstep(0.11,-0.11,neckD);
    float m = max(head, max(body,neck));
    if (m <= 0.0) return under;
    m *= 0.55+0.45*fbm(p*5.0+t*0.05);
    vec3 shadowCol = mix(P_CHARCOAL, P_PRUSSIAN, 0.30)*0.55;
    return mix(under, shadowCol, clamp(m*amt,0.0,0.9));
}

// ── 3D PROJECTION ─────────────────────────────────────────────
vec2 project3D(vec3 pos, float cameraZ) {
    float iz = 1.0 / max(cameraZ - pos.z, 0.01);
    return pos.xy * (cameraZ * iz);
}

void main() {
    vec2 uv = isf_FragNormCoord * 2.0 - 1.0;
    uv.x *= RENDERSIZE.x / RENDERSIZE.y;

    float aspect = RENDERSIZE.x / max(RENDERSIZE.y, 1.0);
    // uv01 for pop/dot functions that expect [0,1]
    vec2 uv01 = isf_FragNormCoord;

    float t = TIME * (0.4 + gestureSpeed);
    int moodI = int(clamp(float(mood), 0.0, 3.0)); // WebGL: long inputs arrive as float
    vec3 bands = audioBands(t, audioReact);

    float bass   = clamp(audioBass,  0.0, 1.0) * audioReact;
    float midA   = clamp(audioMid,   0.0, 1.0) * audioReact;
    float treble = clamp(audioHigh,  0.0, 1.0) * audioReact;
    float level  = clamp(audioLevel, 0.0, 1.0) * audioReact;

    // ── 1) UNDERPAINTING ──────────────────────────────────────────
    vec3 under = mix(P_BONECREAM, P_BONECREAM*0.78, fbm(uv*1.4));
    float stain = fbm(uv*2.7+13.0);
    under = mix(under, P_RAWUMBER, smoothstep(0.55,0.85,stain)*0.55);
    if (moodI==3) under = mix(under, P_CHARCOAL, 0.70+0.20*fbm(uv*1.9));
    if (moodI==1) under *= 1.04;

    // ── 2) DENSE STROKE FIELD ─────────────────────────────────────
    int N = int(clamp(brushDensity, 12.0, 48.0));
    vec3 col = under;
    for (int i = 0; i < 48; i++) {
        if (i >= N) break;
        col = paintStroke(col, uv, float(i)+1.0, t, moodI,
                          float(N), paletteWarmth, bands, audioReact);
    }

    // ── 3) HDR RED SLASHES ────────────────────────────────────────
    col = redSlash(col, uv, t, moodI, audioReact);

    // ── 4) FIGURE + SHADOW ────────────────────────────────────────
    if (showFigure) {
        col = shadowDouble(col, uv, t, moodI, shadowDepth);
        col = centralFigure(col, uv, t, moodI, audioReact);
    }

    // ── 5) BEN-DAY DOT OVERLAY ────────────────────────────────────
    // Choose pop dot colour driven by mood
    vec3 dotColA = (moodI==0) ? LL_YELLOW
                 : (moodI==1) ? LL_CYAN
                 : (moodI==2) ? LL_RED
                 : LL_WHITE;
    vec3 dotColB = (moodI==0) ? LL_RED
                 : (moodI==1) ? LL_RED
                 : (moodI==2) ? LL_YELLOW
                 : LL_RED;

    // Animated two-layer dot pass
    float dotPhase = treble * audioReact;
    float dots1 = benDay(uv01, dotDensity, dotRadius, aspect, dotPhase);
    float dots2 = benDay(uv01 + vec2(0.5/max(dotDensity,1.0), 0.0),
                         dotDensity*1.35, dotRadius*0.55, aspect, dotPhase*0.6);

    // Dots are masked by stroke luminance — appear more in dark areas
    float lum = dot(col, vec3(0.299,0.587,0.114));
    float dotMaskA = clamp(1.0 - lum*1.2, 0.0, 1.0); // dark areas
    float dotMaskB = clamp(lum*1.0,       0.0, 1.0); // light areas

    float effectiveDotMix = dotMix * (0.5 + 0.5*sin(t*0.41 + 1.3));
    col = mix(col, dotColA, dots1 * dotMaskA * effectiveDotMix);
    col = mix(col, dotColB, dots2 * dotMaskB * effectiveDotMix * 0.55);

    // ── 6) KANDINSKY 3D SHAPES ────────────────────────────────────
    vec2 Psc = vec2((uv01.x - 0.5)*aspect, uv01.y - 0.5);

    float cameraZ = 2.2;
    float zRange  = depth3D * 0.85;

    // Shape palette tuned to current mood
    vec3 shapeColA = (moodI==0) ? P_CADORANGE : (moodI==1) ? LL_CYAN    : (moodI==2) ? LL_YELLOW : LL_WHITE;
    vec3 shapeColB = (moodI==0) ? LL_RED      : (moodI==1) ? LL_RED     : (moodI==2) ? LL_RED    : LL_RED;
    vec3 shapeColC = (moodI==0) ? LL_CYAN     : (moodI==1) ? K_BLUE     : (moodI==2) ? LL_CYAN   : K_BLUE;

    int NS = int(clamp(shapeCount, 0.0, 20.0));

    // Halo pass
    float haloField = 0.0;
    vec3  haloCol   = vec3(0.0);
    float haloWt    = 0.0;

    for (int i = 0; i < 20; i++) {
        if (i >= NS) break;
        float fi = float(i) + compositionSeed*1.71;
        float phA = fi*2.399;
        float phB = fi*1.618;
        float spd = orbitSpeed*(0.5+h11(fi*3.1)*1.2);
        float ox  = (h11(fi*1.3)-0.5)*0.7*aspect;
        float oy  = (h11(fi*2.7)-0.5)*0.7;
        float oz  = (h11(fi*4.1)-0.5)*2.0*zRange;
        vec3 home = vec3(ox+0.08*sin(t*0.05+fi),
                         oy+0.08*cos(t*0.04+fi*1.3),
                         oz+zRange*0.4*sin(t*0.06+fi*0.77));
        vec3 orbit3 = vec3(
            sin(t*spd+phA)*orbitRange*aspect,
            cos(t*spd*0.7+phB*1.7)*orbitRange,
            sin(t*spd*0.43+fi*1.11)*zRange*0.6
        );
        vec3 pos3 = home + orbit3;
        vec2 fromCtr2 = pos3.xy;
        float fcLen = length(fromCtr2);
        if (fcLen > 1e-4) fromCtr2 = fromCtr2/fcLen;
        pos3.xy += fromCtr2*springReact*bass*2.2;
        vec2 ctr2D = project3D(pos3, cameraZ);
        float zFactor = clamp((cameraZ-pos3.z)/cameraZ, 0.3, 2.5);
        float sz = shapeSize*(0.7+h11(fi*5.3)*0.6)*zFactor*(1.0+level*0.08);
        float rDist = length(Psc - ctr2D);
        float halo  = exp(-pow(rDist/(sz*2.2), 2.0));
        haloField += halo;
        int stype = int(mod(fi, 3.0));
        vec3 sc = (stype==0) ? shapeColA : (stype==1) ? shapeColB : shapeColC;
        haloCol += sc*halo;
        haloWt  += halo;
    }
    if (haloWt > 1e-4) {
        haloCol /= haloWt;
        col = mix(col, haloCol, clamp(haloField*haloStrength*0.38, 0.0, 0.72));
    }

    // Solid shapes pass
    float bestSD   = 1e9;
    vec3  bestCol2 = col;
    float bestSz   = 0.05;
    vec3  bestSC   = LL_YELLOW;

    for (int i = 0; i < 20; i++) {
        if (i >= NS) break;
        float fi = float(i) + compositionSeed*1.71;
        float phA = fi*2.399; float phB = fi*1.618;
        float spd = orbitSpeed*(0.5+h11(fi*3.1)*1.2);
        float ox  = (h11(fi*1.3)-0.5)*0.7*aspect;
        float oy  = (h11(fi*2.7)-0.5)*0.7;
        float oz  = (h11(fi*4.1)-0.5)*2.0*zRange;
        vec3 home = vec3(ox+0.08*sin(t*0.05+fi),
                         oy+0.08*cos(t*0.04+fi*1.3),
                         oz+zRange*0.4*sin(t*0.06+fi*0.77));
        vec3 orbit3 = vec3(
            sin(t*spd+phA)*orbitRange*aspect,
            cos(t*spd*0.7+phB*1.7)*orbitRange,
            sin(t*spd*0.43+fi*1.11)*zRange*0.6
        );
        vec3 pos3 = home + orbit3;
        vec2 fromCtr2 = pos3.xy;
        float fcLen = length(fromCtr2);
        if (fcLen > 1e-4) fromCtr2 = fromCtr2/fcLen;
        pos3.xy += fromCtr2*springReact*bass*2.2;
        vec2 ctr2D = project3D(pos3, cameraZ);
        float zFactor = clamp((cameraZ-pos3.z)/cameraZ, 0.3, 2.5);
        float sz = shapeSize*(0.7+h11(fi*5.3)*0.6)*zFactor*(1.0+level*0.08);
        float rot = t*midA*1.3 + h11(fi*7.7)*6.2832 + t*orbitSpeed*h11(fi*2.33)*0.8;
        float ca = cos(-rot), sa = sin(-rot);
        vec2 lp = Psc - ctr2D;
        lp = vec2(ca*lp.x - sa*lp.y, sa*lp.x + ca*lp.y);
        int stype = int(mod(fi, 3.0));
        float sd = (stype==0) ? sdTriangle2(lp, sz)
                 : (stype==1) ? sdBox2(lp, sz)
                 : sdCircle(lp, sz);
        if (sd < bestSD) {
            bestSD  = sd;
            bestSz  = sz;
            bestSC  = (stype==0) ? shapeColA : (stype==1) ? shapeColB : shapeColC;
            vec2 chk = floor(lp / max(sz*0.30, 1e-4));
            bool chkOn  = (mod(chk.x+chk.y, 2.0) < 1.0);
            bool useChk = (mod(fi, 4.0) >= 3.0);
            bestCol2 = useChk ? (chkOn ? bestSC : LL_BLACK) : bestSC;
        }
    }

    if (NS > 0) {
        if (bestSD < 0.0) {
            // Inside shape — Ben-Day fill with ink dots
            float shapeDots = benDay(uv01, dotDensity*1.1, dotRadius*0.75, aspect, treble*0.5);
            vec3 dotC = mix(bestCol2, LL_BLACK, 0.85);
            vec3 filled = mix(bestCol2, dotC, shapeDots*0.55);
            col = mix(col, filled, 1.0);
            // Pop outline reinforcement
            float popOut = benDay(uv01, dotDensity*0.5, dotRadius*0.3, aspect, 0.0);
            col = mix(col, P_INK, popOut*0.15);
        } else if (bestSD < bestSz*0.12) {
            float ot = 1.0 - smoothstep(0.0, outlineWeight*2.0, bestSD);
            col = mix(col, LL_BLACK, ot);
        } else {
            float nearGlow = exp(-bestSD/(bestSz*0.25))*0.18;
            col = mix(col, bestSC, nearGlow*haloStrength);
        }
    }

    // ── 7) POP SUPPORT LINES ──────────────────────────────────────
    // Thin Kandinsky scaffolding lines (7 lines, audio-reactive)
    for (int k = 0; k < 7; k++) {
        float fk = float(k) + compositionSeed*0.71;
        float ang = h11(fk*1.7)*6.2832 + sin(t*0.3+fk*1.3)*0.5;
        vec2 dir  = vec2(cos(ang), sin(ang));
        vec2 pt   = vec2(h11(fk*3.3), h11(fk*5.1));
        pt += vec2(sin(t*0.4+fk), cos(t*0.32+fk*1.7))*0.05;
        vec2 d    = uv01 - pt;
        float perp = abs(d.x*(-dir.y)+d.y*dir.x);
        float lw   = outlineWeight*(0.6+h11(fk*7.13)*0.8);
        float lm   = smoothstep(lw, 0.0, perp);
        if (h11(fk*11.7) > 0.55) {
            float along = d.x*dir.x+d.y*dir.y;
            lm *= step(0.5, fract(along*26.0+t*0.3));
        }
        col = mix(col, LL_BLACK, lm*(0.4+treble*0.7));
    }

    // ── 8) CANVAS TOOTH ───────────────────────────────────────────
    float tooth = fbm(uv*90.0);
    float hi = smoothstep(0.78,0.95,tooth);
    col += vec3(0.30,0.26,0.18)*hi*0.7;

    // ── 9) VIGNETTE ───────────────────────────────────────────────
    float r = length(uv*vec2(0.6,0.8));
    col = mix(col, col*0.78+P_RAWUMBER*0.05, smoothstep(0.95,1.55,r));

    // ── 10) CRISPNESS / LOCAL CONTRAST ────────────────────────────
    float l = dot(col, vec3(0.299,0.587,0.114));
    col += (col - vec3(l)) * clamp(crisp, 0.0, 2.5) * 0.16;

    // ── 11) BASS FLASH ────────────────────────────────────────────
    {
        vec2 fc = (uv01 - vec2(0.18,0.82)); fc.x *= aspect;
        float fa = atan(fc.y, fc.x); float fr = length(fc);
        float flashR = 0.10 + 0.04*cos(fa*10.0) + 0.10*bass;
        float flashAmt = step(fr, flashR)*bass;
        col = mix(col, LL_YELLOW, flashAmt*0.70);
        col = mix(col, LL_BLACK,  step(abs(fr-flashR),0.008)*bass);
    }

    // ---- universal color block (defaults = no-op) ----
    vec3 uc = max(col, 0.0);
    float ucL = dot(uc, vec3(0.299, 0.587, 0.114));
    uc = mix(vec3(ucL), uc, colorBoost);                   // saturation
    if (hueShift > 0.0005) {                               // cheap hue rotate (YIQ)
        float hA = hueShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hM * uc, 0.0, 1.0);
    }
    // background = darkest end of the paint field (ink/charcoal depths)
    uc = mix(uc, bgColor.rgb, bgColor.a * (1.0 - smoothstep(0.0, 0.35, ucL)));

    gl_FragColor = vec4(uc, 1.0);
}
/*{
  "DESCRIPTION": "Analog Green — a 1960s vector scope with a life of its own: a warm-black CRT, a barrel-warped phosphor-green grid, and a set of figure slots that keep spawning, morphing and dissolving on their own clocks — Lissajous loops, rose curves, spiral bursts, sweeping waveforms, noisy rings and a live spectrum trace — each drawn by a hot beam dot racing along the curve so the phosphor tail is what you see. A beam-drawn coastline with islands and two crosshair cursors sit quietly underneath. Persistence, beam glow, mains flicker, line jitter, grain and glass vignette. Bass swells the figures, beats re-strike them, highs add jitter.",
  "CREDIT": "Easel / ShaderClaw house shader, 2026-10-06 — after Lu's vector-scope coastline reference",
  "CATEGORIES": ["Generator", "Retro", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "speed",        "LABEL": "Speed",            "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 4.0,  "GROUP": "Speed" },
    { "NAME": "drift",        "LABEL": "Chart Drift",      "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 3.0,  "GROUP": "Speed" },
    { "NAME": "redraw",       "LABEL": "Coast Redraw",     "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 3.0,  "GROUP": "Speed" },
    { "NAME": "cursorSpeed",  "LABEL": "Cursor Speed",     "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 3.0,  "GROUP": "Speed" },
    { "NAME": "figures",      "LABEL": "Figures",          "TYPE": "float", "DEFAULT": 4.0,  "MIN": 0.0,  "MAX": 6.0,  "GROUP": "Figures" },
    { "NAME": "figureLife",   "LABEL": "Figure Life (s)",  "TYPE": "float", "DEFAULT": 6.0,  "MIN": 1.0,  "MAX": 30.0, "GROUP": "Figures" },
    { "NAME": "figureSize",   "LABEL": "Figure Size",      "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.3,  "MAX": 3.0,  "GROUP": "Figures" },
    { "NAME": "figureMotion", "LABEL": "Figure Motion",    "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 3.0,  "GROUP": "Figures" },
    { "NAME": "figureWidth",  "LABEL": "Figure Width",     "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.3,  "MAX": 4.0,  "GROUP": "Figures" },
    { "NAME": "beamDot",      "LABEL": "Beam Dot",         "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 3.0,  "GROUP": "Figures" },
    { "NAME": "chaos",        "LABEL": "Chaos",            "TYPE": "float", "DEFAULT": 0.5,  "MIN": 0.0,  "MAX": 1.0,  "GROUP": "Figures" },
    { "NAME": "spectrum",     "LABEL": "Spectrum Trace",   "TYPE": "float", "DEFAULT": 0.8,  "MIN": 0.0,  "MAX": 2.0,  "GROUP": "Figures" },
    { "NAME": "phosphor",     "LABEL": "Phosphor",         "TYPE": "color", "DEFAULT": [0.36, 1.0, 0.82, 1.0], "GROUP": "Colors" },
    { "NAME": "glassColor",   "LABEL": "Glass",            "TYPE": "color", "DEFAULT": [0.05, 0.035, 0.045, 1.0], "GROUP": "Colors" },
    { "NAME": "brightness",   "LABEL": "Brightness",       "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.2,  "MAX": 3.0,  "GROUP": "Colors" },
    { "NAME": "gridBright",   "LABEL": "Grid Brightness",  "TYPE": "float", "DEFAULT": 0.8,  "MIN": 0.0,  "MAX": 2.0,  "GROUP": "Colors" },
    { "NAME": "traceBright",  "LABEL": "Coast Brightness", "TYPE": "float", "DEFAULT": 0.55,  "MIN": 0.0,  "MAX": 2.0,  "GROUP": "Colors" },
    { "NAME": "columns",      "LABEL": "Grid Columns",     "TYPE": "float", "DEFAULT": 12.0, "MIN": 2.0,  "MAX": 40.0, "GROUP": "Grid" },
    { "NAME": "rows",         "LABEL": "Grid Rows",        "TYPE": "float", "DEFAULT": 8.0,  "MIN": 2.0,  "MAX": 30.0, "GROUP": "Grid" },
    { "NAME": "gridWidth",    "LABEL": "Grid Line Width",  "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.3,  "MAX": 4.0,  "GROUP": "Grid" },
    { "NAME": "ticks",        "LABEL": "Edge Ticks",       "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 1.0,  "GROUP": "Grid" },
    { "NAME": "tilt",         "LABEL": "Tilt",             "TYPE": "float", "DEFAULT": 0.03, "MIN": -0.3, "MAX": 0.3,  "GROUP": "Grid" },
    { "NAME": "keystone",     "LABEL": "Keystone",         "TYPE": "float", "DEFAULT": 0.06, "MIN": -0.4, "MAX": 0.4,  "GROUP": "Grid" },
    { "NAME": "barrel",       "LABEL": "Barrel",           "TYPE": "float", "DEFAULT": 0.10, "MIN": -0.2, "MAX": 0.5,  "GROUP": "Grid" },
    { "NAME": "coastDetail",  "LABEL": "Coast Detail",     "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 3.0,  "GROUP": "Trace" },
    { "NAME": "jagged",       "LABEL": "Jaggedness",       "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 3.0,  "GROUP": "Trace" },
    { "NAME": "traceWidth",   "LABEL": "Trace Width",      "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.3,  "MAX": 4.0,  "GROUP": "Trace" },
    { "NAME": "islands",      "LABEL": "Islands",          "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 2.0,  "GROUP": "Trace" },
    { "NAME": "crosshairs",   "LABEL": "Crosshairs",       "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 1.0,  "GROUP": "Trace" },
    { "NAME": "persistence",  "LABEL": "Persistence",      "TYPE": "float", "DEFAULT": 0.7,  "MIN": 0.0,  "MAX": 1.0,  "GROUP": "Phosphor" },
    { "NAME": "glowAmt",      "LABEL": "Beam Glow",        "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 3.0,  "GROUP": "Phosphor" },
    { "NAME": "flicker",      "LABEL": "Flicker",          "TYPE": "float", "DEFAULT": 0.5,  "MIN": 0.0,  "MAX": 2.0,  "GROUP": "Phosphor" },
    { "NAME": "jitter",       "LABEL": "Line Jitter",      "TYPE": "float", "DEFAULT": 0.5,  "MIN": 0.0,  "MAX": 2.0,  "GROUP": "Phosphor" },
    { "NAME": "grain",        "LABEL": "Grain",            "TYPE": "float", "DEFAULT": 0.5,  "MIN": 0.0,  "MAX": 2.0,  "GROUP": "Phosphor" },
    { "NAME": "vignetteStr",  "LABEL": "Glass Vignette",   "TYPE": "float", "DEFAULT": 0.5,  "MIN": 0.0,  "MAX": 1.0,  "GROUP": "Phosphor" },
    { "NAME": "audioReact",   "LABEL": "Audio React",      "TYPE": "float", "DEFAULT": 0.5,  "MIN": 0.0,  "MAX": 1.0,  "GROUP": "Audio Reactivity" },
    { "NAME": "bassResponse", "LABEL": "Bass → Hum",       "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 3.0,  "GROUP": "Audio Reactivity" },
    { "NAME": "beatResponse", "LABEL": "Beat → Re-strike",    "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 3.0,  "GROUP": "Audio Reactivity" },
    { "NAME": "highResponse", "LABEL": "High → Jitter",    "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 3.0,  "GROUP": "Audio Reactivity" }
  ],
  "PASSES": [
    { "TARGET": "chartBuf" },
    { "TARGET": "phosBuf", "PERSISTENT": true },
    {}
  ]
}*/

#define PI 3.14159265
#define COAST_SEGS 56
#define N_ISLANDS 6

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

// ---- hashes / noise -----------------------------------------------------------
float hash12(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}
float hash11(float n) { return fract(sin(n * 127.1) * 43758.5453); }
float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    vec2 u = f * f * (3.0 - 2.0 * f);
    return mix(mix(hash12(i), hash12(i + vec2(1.0, 0.0)), u.x),
               mix(hash12(i + vec2(0.0, 1.0)), hash12(i + vec2(1.0, 1.0)), u.x), u.y) * 2.0 - 1.0;
}
float fbm(vec2 p) {
    float a = 0.5, r = 0.0;
    for (int i = 0; i < 4; i++) { r += a * vnoise(p); p = p * 2.03 + 17.1; a *= 0.5; }
    return r;
}
float fbm3(vec2 p) {
    float a = 0.5, r = 0.0;
    for (int i = 0; i < 3; i++) { r += a * vnoise(p); p = p * 2.03 + 17.1; a *= 0.5; }
    return r;
}

// ---- geometry helpers ----------------------------------------------------------
float segDist(vec2 p, vec2 a, vec2 b) {
    vec2 pa = p - a, ba = b - a;
    float h = clamp(dot(pa, ba) / max(dot(ba, ba), 1e-8), 0.0, 1.0);
    return length(pa - ba * h);
}
// a crisp phosphor line: hard core + tight falloff, width in uv units
float beamLine(float d, float w) {
    return smoothstep(w, w * 0.35, d);
}

// ---- the chart (drawn in undistorted "scope space", uv 0..1, y up) -----------
float g_t;      // chart clock
float g_strike; // beam strike (beat)

// The coast: a wandering polyline from the upper-left to the lower-right,
// shaped by two low octaves of noise; its fine shore is a domain warp of the
// query point, so the jaggedness is free at any segment count.
vec2 coastPoint(float s, float tm) {
    float x = mix(0.30, 0.95, s) + 0.10 * sin(s * PI) * sin(tm * 0.11 + 2.0);
    float y = mix(0.92, 0.06, pow(s, 0.85));
    vec2 p = vec2(x, y);
    vec2 n = vec2(fbm(vec2(s * 3.1, tm * 0.07)), fbm(vec2(s * 2.7 + 9.0, tm * 0.07 + 4.0)));
    p += n * vec2(0.09, 0.06) * coastDetail;
    // the peninsula dangling off the middle (Baja-like)
    float pen = exp(-pow((s - 0.58) * 9.0, 2.0));
    p += vec2(-0.05, -0.07) * pen * coastDetail;
    return p;
}
// The coast polyline + island centres are the same for every pixel, so a
// tiny lookup pass (chartBuf, 64 normalised slots in an 8-texel band)
// evaluates their noise ONCE per frame; pixels only fetch. (Evaluating the
// 57 noise-shaped points per pixel cost half the frame.)
#define CHART_SLOTS 64.0
vec4 chartSlot(int i) {
    return texture2D(chartBuf, vec2((float(i) + 0.5) / CHART_SLOTS, 2.0 / RENDERSIZE.y));
}
vec2 islandCentre(int i, float tm) {
    float fi = float(i);
    vec2 c = vec2(0.33 + 0.08 * fi + 0.02 * hash11(fi * 3.1), 0.28 + 0.03 * sin(fi * 1.7) + 0.04 * hash11(fi * 5.3));
    c += 0.015 * vec2(fbm(vec2(fi, tm * 0.05)), fbm(vec2(fi + 7.0, tm * 0.05)));
    return c;
}
float coastDist(vec2 q) {
    float d = 1e3;
    vec2 prev = chartSlot(0).xy;
    for (int i = 1; i <= COAST_SEGS; i++) {
        vec2 cur = chartSlot(i).xy;
        d = min(d, segDist(q, prev, cur));
        prev = cur;
    }
    return d;
}
// small islands: noisy closed loops drawn as thin rings
float islandDist(vec2 q) {
    float d = 1e3;
    for (int i = 0; i < N_ISLANDS; i++) {
        float fi = float(i);
        vec4 sl = chartSlot(COAST_SEGS + 1 + i);
        vec2 c = sl.xy; float r = sl.z;
        vec2 rel = q - c;
        float ang = atan(rel.y, rel.x);
        float rr = r * (1.0 + 0.45 * vnoise(vec2(ang * 1.6 + fi * 7.0, fi)));
        d = min(d, abs(length(rel) - rr));
    }
    return d;
}
// ---- scope figures: slots that spawn, morph and dissolve ----------------------
#define FIG_SEGS 32
#define MAX_FIGS 6
float g_bassP, g_highP;

// A figure slot's state for this moment: centre, radius, kind, life envelope.
// Each slot runs its own clock (epoch = floor(time/life + offset)), so slots
// never spawn or die together.
vec2 figPoint(float s, int kind, vec2 c, float r, vec4 h, float age, float tm) {
    float mo = figureMotion;
    if (kind == 0) {            // Lissajous: integer ratios from the hash, phase morphs
        float a = 1.0 + floor(h.x * 4.0), b = 1.0 + floor(h.y * 4.0);
        float ph = h.z * 6.28 + tm * (0.35 + 0.6 * h.w) * mo;
        return c + r * vec2(sin(a * s * 6.28318 + ph), sin(b * s * 6.28318 + tm * 0.2 * mo));
    } else if (kind == 1) {     // rose curve, rotating
        float n = 2.0 + floor(h.x * 5.0);
        float th = s * 6.28318 + tm * 0.4 * mo * (h.y - 0.5) * 2.0;
        float rr = r * cos(n * s * 6.28318 + tm * 0.5 * mo);
        return c + rr * vec2(cos(th), sin(th));
    } else if (kind == 2) {     // spiral burst: unwinds as it lives
        float turns = 2.0 + h.x * 4.0;
        float th = s * 6.28318 * turns + tm * 1.2 * mo * (h.y > 0.5 ? 1.0 : -1.0);
        float rr = r * s * smoothstep(0.0, 0.6, age);
        return c + rr * vec2(cos(th), sin(th));
    } else if (kind == 3) {     // sweeping waveform
        float w = r * 2.4;
        float f1 = 2.0 + h.x * 6.0, f2 = f1 * (1.7 + h.y);
        float y = 0.55 * sin(s * f1 * 6.28318 + tm * 6.0 * mo) + 0.3 * sin(s * f2 * 6.28318 - tm * 4.3 * mo);
        y *= (1.0 + 0.6 * g_bassP);
        return c + vec2((s - 0.5) * w, y * r * 0.55);
    }                           // noisy ring, slowly wobbling
    float th = s * 6.28318 + tm * 0.25 * mo;
    float rr = r * (1.0 + 0.3 * vnoise(vec2(s * 5.0 + h.x * 9.0, tm * 0.6 * mo + h.y * 7.0)));
    return c + rr * vec2(cos(th), sin(th));
}
float drawFigures(vec2 q, float aspect, float px, float tm) {
    float v = 0.0;
    int nFig = int(clamp(figures, 0.0, float(MAX_FIGS)));
    float fw = px * 1.3 * figureWidth;
    for (int k = 0; k < MAX_FIGS; k++) {
        if (k >= nFig) break;
        float fk = float(k);
        float life = figureLife * (0.6 + 0.8 * hash11(fk * 3.7));       // slots differ in life
        float clock = tm / life + hash11(fk * 11.3);
        float epoch = floor(clock), age = fract(clock);
        vec4 h = vec4(hash11(epoch * 17.1 + fk), hash11(epoch * 7.3 + fk * 5.0),
                      hash11(epoch * 3.9 + fk * 9.0), hash11(epoch * 29.7 + fk * 2.0));
        // strike: beats re-arm a slot (brighter, snaps to full size)
        float env = smoothstep(0.0, 0.12, age) * smoothstep(1.0, 0.72, age);
        env = max(env, g_strike * 0.6 * step(0.5, hash11(epoch + fk * 4.4 + floor(tm * 3.0))));
        if (env < 0.01) continue;
        int kind = int(mod(floor(h.x * 11.0 + fk), 5.0));
        vec2 c = vec2(0.15 + 0.7 * hash11(epoch * 5.1 + fk * 13.0) * aspect, 0.18 + 0.64 * hash11(epoch * 9.7 + fk * 3.3));
        c += 0.02 * vec2(sin(tm * 0.3 + fk), cos(tm * 0.27 + fk * 2.0)) * figureMotion;
        float r = (0.05 + 0.12 * hash11(epoch * 13.9 + fk)) * figureSize * (1.0 + 0.35 * g_bassP);
        r *= mix(1.0, 0.5 + hash11(epoch * 2.2 + fk), chaos);
        float tmk = tm * mix(1.0, 0.4 + 1.6 * hash11(epoch * 6.1 + fk), chaos);
        // most pixels are nowhere near this figure: skip its curve entirely
        if (length(q - c) > r * 1.6 + px * 14.0) continue;
        // the curve as a polyline
        float d = 1e3;
        vec2 prev = figPoint(0.0, kind, c, r, h, age, tmk);
        for (int i = 1; i <= FIG_SEGS; i++) {
            vec2 cur = figPoint(float(i) / float(FIG_SEGS), kind, c, r, h, age, tmk);
            d = min(d, segDist(q, prev, cur));
            prev = cur;
        }
        float line = beamLine(d, fw) * env;
        // the beam dot racing along the figure (the phosphor tail does the rest)
        if (beamDot > 0.0) {
            float sp = fract(tm * (0.5 + 0.8 * hash11(fk * 1.9)) * figureMotion);
            vec2 bp = figPoint(sp, kind, c, r, h, age, tmk);
            float bd = length(q - bp);
            line = max(line, smoothstep(px * 3.2, px * 0.8, bd) * 1.6 * beamDot * env);
            line += smoothstep(px * 9.0, 0.0, bd) * 0.25 * beamDot * env;
        }
        v = max(v, line);
    }
    // live spectrum trace along the bottom third, like a scope on the music
    if (spectrum > 0.0) {
        float x = q.x / aspect;
        float yb = 0.16, hgt = 0.14 * spectrum;
        float sv = 0.0;
        // 7-bin smoothed spectrum at this x
        for (int i = -3; i <= 3; i++) sv += audioSpectrum(clamp(x + float(i) * 0.004, 0.0, 1.0) * 0.6);
        sv /= 7.0;
        sv = pow(clamp(sv, 0.0, 1.0), 0.7);
        float idle = 0.08 * (0.5 + 0.5 * sin(x * 31.0 + tm * 2.0)) * (0.5 + 0.5 * sin(x * 7.0 - tm * 1.3));
        float y = yb + hgt * max(sv, idle);
        // distance to the trace: compare against neighbours for a proper line
        float dyl = abs(q.y - y);
        float tr = beamLine(dyl, px * 1.4 * figureWidth) * (0.35 + 0.65 * spectrum);
        v = max(v, tr * step(0.02, x) * step(x, 0.98));
    }
    return v;
}

// The grid is static scenery: it is drawn straight into the screen pass (not
// through the phosphor), so nothing accumulates if its spacing is animated.
float drawGrid(vec2 uv, float aspect, float px) {
    vec2 q = uv; q.x *= aspect;
    float gw = px * 1.1 * gridWidth;
    float cols = max(2.0, floor(columns + 0.5)), rws = max(2.0, floor(rows + 0.5));
    vec2 cell = vec2(aspect / cols, 1.0 / rws);
    vec2 g = abs(fract(q / cell + 0.5) - 0.5) * cell;
    float grid = beamLine(min(g.x, g.y), gw);
    if (ticks > 0.0) {
        float ty = abs(fract(q.y / (cell.y * 0.2) + 0.5) - 0.5) * cell.y * 0.2;
        float tx = abs(fract(q.x / (cell.x * 0.2) + 0.5) - 0.5) * cell.x * 0.2;
        float tk = max(step(q.x, 0.012) * beamLine(ty, gw), step(q.y, 0.012) * beamLine(tx, gw));
        grid = max(grid, tk * ticks);
    }
    return grid * gridBright * 0.75;
}

float drawChart(vec2 uv, float aspect) {
    vec2 q = uv; q.x *= aspect;                       // square units
    float jit = jitter * (0.0006 + 0.0025 * g_strike);
    // line jitter: the beam wobbles a hair, per scanline band
    q += jit * vec2(vnoise(vec2(uv.y * 90.0, g_t * 37.0)), vnoise(vec2(uv.x * 90.0 + 5.0, g_t * 31.0)));
    float px = 1.0 / RENDERSIZE.y;                    // one pixel in uv units

    float v = 0.0;
    // coast + islands, drawn through a fine domain warp for the shore detail
    {
        float tm = g_t * redraw;
        vec2 warp = vec2(fbm3(q * 55.0 + tm * 0.3), fbm3(q * 55.0 + 31.0 - tm * 0.2)) * 0.0032 * jagged;
        vec2 qw = q + warp;
        float tw = px * 1.25 * traceWidth;
        float dc = coastDist(qw);
        float coast = beamLine(dc, tw);
        if (islands > 0.0) coast = max(coast, beamLine(islandDist(qw), tw) * islands);
        v = max(v, coast * traceBright);
    }
    // two crosshair cursors, slowly wandering
    if (crosshairs > 0.0) {
        float tw = px * 1.1;
        float ct = g_t * cursorSpeed * 0.25;
        vec2 c1 = vec2(0.40 + 0.05 * sin(ct * 0.7), 0.33 + 0.04 * cos(ct * 0.9));
        vec2 c2 = vec2(0.31 + 0.05 * sin(ct * 0.5 + 2.0), 0.24 + 0.04 * cos(ct * 0.6 + 1.0));
        float arm = 0.10;
        float x1 = beamLine(segDist(q, c1 - vec2(arm, 0.0), c1 + vec2(arm, 0.0)), tw);
        float y1 = beamLine(segDist(q, c1 - vec2(0.0, arm * 1.4), c1 + vec2(0.0, arm * 1.4)), tw);
        float x2 = beamLine(segDist(q, c2 - vec2(arm, 0.0), c2 + vec2(arm, 0.0)), tw);
        float y2 = beamLine(segDist(q, c2 - vec2(0.0, arm * 1.4), c2 + vec2(0.0, arm * 1.4)), tw);
        v = max(v, max(max(x1, y1), max(x2, y2)) * crosshairs * 0.9);
    }
    // the living part: figures that come and go
    v = max(v, drawFigures(q, aspect, px, g_t * 0.6));
    return v;
}

// ---- pass 0: chart lookup (coast points + island centres) ---------------------
vec4 passChart() {
    vec2 fc = floor(gl_FragCoord.xy);
    float slot = floor(gl_FragCoord.x / RENDERSIZE.x * CHART_SLOTS);
    if (fc.y >= 8.0) return vec4(0.0);
    float tm = g_t * redraw;
    int i = int(slot);
    if (i <= COAST_SEGS) return vec4(coastPoint(float(i) / float(COAST_SEGS), tm), 0.0, 1.0);
    int k = i - (COAST_SEGS + 1);
    if (k >= 0 && k < N_ISLANDS) return vec4(islandCentre(k, tm), 0.006 + 0.010 * hash11(float(k) * 9.7), 1.0);
    return vec4(0.0);
}

// ---- pass 1: phosphor persistence --------------------------------------------
vec4 passPhosphor() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    float aspect = RENDERSIZE.x / RENDERSIZE.y;
    float cur = drawChart(uv, aspect);
    vec4 prev = texture2D(phosBuf, uv);
    if (FRAMEINDEX < 2 || any(notEqual(prev, prev))) prev = vec4(0.0);
    // frame-rate independent decay: persistence 0 = none, 1 = ~2 s tail
    float tail = mix(0.02, 2.0, persistence);
    float decay = exp(-max(TIMEDELTA, 0.001) / tail);
    float p = max(prev.x * decay, cur);
    return vec4(p, cur, 0.0, 1.0);
}

// ---- pass 1: CRT glass ----------------------------------------------------------
vec4 passScreen() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    float ar = clamp(audioReact, 0.0, 1.0);
    float bassP = pow(knee(audioBass, 0.05, 0.85), 1.5) * bassResponse * ar;
    float highP = pow(knee(audioHigh, 0.10, 0.90), 1.2) * highResponse * ar;

    // screen geometry: tilt, keystone, barrel — the chart is a flat drawing
    // behind curved glass
    vec2 c = uv - 0.5;
    float a = tilt + 0.004 * bassP * sin(g_t * 23.0);          // mains hum nudges the raster
    c = mat2(cos(a), -sin(a), sin(a), cos(a)) * c;
    c.x *= 1.0 + keystone * c.y;
    float r2 = dot(c, c);
    c *= 1.0 + barrel * r2;
    vec2 suv = c + 0.5;
    float inside = step(0.0, suv.x) * step(suv.x, 1.0) * step(0.0, suv.y) * step(suv.y, 1.0);
    vec2 texel = 1.0 / RENDERSIZE.xy;

    vec2 ph = texture2D(phosBuf, suv).xy;
    float trace = ph.x;
    float aspect = RENDERSIZE.x / RENDERSIZE.y;
    float grid = drawGrid(suv, aspect, 1.0 / RENDERSIZE.y) * inside;
    // beam glow: 8-tap ring at two radii
    float glow = 0.0;
    for (int i = 0; i < 8; i++) {
        float an = float(i) * 0.7853982;
        vec2 o = vec2(cos(an), sin(an));
        glow += texture2D(phosBuf, suv + o * texel * 2.5).x;
        glow += texture2D(phosBuf, suv + o * texel * 6.0).x * 0.5;
    }
    glow /= 12.0;

    // phosphor colour: the fresh beam is whiter, the persistence tail greener
    vec3 ph0 = phosphor.rgb;
    vec3 hot = mix(ph0, vec3(1.0), 0.35);
    vec3 col = hot * trace + ph0 * glow * glow * 2.2 * glowAmt + ph0 * glow * 0.25 * glowAmt;
    col *= inside;
    col += ph0 * grid * 0.9;

    // glass: warm black with a soft centre lift and a dark vignette
    float vig = 1.0 - vignetteStr * smoothstep(0.25, 0.9, length(uv - 0.5) * 1.5);
    vec3 glass = glassColor.rgb * (0.7 + 0.6 * (1.0 - smoothstep(0.0, 0.8, r2))) * inside;
    col = col * vig + glass * vig;

    // mains flicker + grain
    float fl = 1.0 + flicker * 0.06 * (hash11(floor(TIME * 60.0)) - 0.5) * (1.0 + 2.0 * highP);
    col *= fl;
    col += (hash12(gl_FragCoord.xy + fract(TIME) * vec2(31.0, 17.0)) - 0.5) * 0.045 * grain * inside;

    col *= brightness;
    col = clamp(col, 0.0, 1.0);
    col = mix(col, col * col * (3.0 - 2.0 * col), 0.35);
    return vec4(col, 1.0);
}

void main() {
    g_t = TIME * speed;
    float ar = clamp(audioReact, 0.0, 1.0);
    g_strike = clamp(audioBeatPulse, 0.0, 1.0) * beatResponse * ar;
    g_bassP  = pow(knee(audioBass, 0.05, 0.85), 1.5) * bassResponse * ar;
    g_highP  = pow(knee(audioHigh, 0.10, 0.90), 1.2) * highResponse * ar;
    if      (PASSINDEX == 0) gl_FragColor = passChart();
    else if (PASSINDEX == 1) gl_FragColor = passPhosphor();
    else                     gl_FragColor = passScreen();
}

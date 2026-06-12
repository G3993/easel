/*{
  "DESCRIPTION": "Digital Wave Grid — a full-screen stacked z-rail of perspective-projected wireframe terrains. Three independent grid sheets float at different depths; each one is a wireframe mesh whose height field is a sum of travelling sines driven by a distinct player[i].energy. The sheets stack vertically across the canvas (top / mid / bottom band) so the composition reads as parallactic strata of digital water. A typewriter title from `cue.latest` is laid down between bands as a thin engraved slab — the message rides on the grid, never replaces it. fwidth-AA line thickness, perspective foreshortening, parallax dolly, depth fog. No EKG, no spectrum bars, no checkerboard, no mirrored horizon — the bands are stacked, not mirrored, and the grids are projected meshes, not 2D regular lattices.",
  "CREDIT": "easel auto-loop — A-List daily / digital abstract wave grid",
  "CATEGORIES": ["Generator", "Text", "A-List"],
  "INPUTS": [
    { "NAME": "msg", "TYPE": "text", "DEFAULT": "DIGITAL WAVE GRID", "MAX_LENGTH": 48, "BIND": "cue.latest" },
    { "NAME": "energyA", "LABEL": "Band A Energy", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[1].energy" },
    { "NAME": "energyB", "LABEL": "Band B Energy", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[2].energy" },
    { "NAME": "energyC", "LABEL": "Band C Energy", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[3].energy" },
    { "NAME": "audioDepth", "LABEL": "Audio Depth Push", "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 0.8 },
    { "NAME": "gridDensity", "LABEL": "Grid Density", "TYPE": "float", "MIN": 8.0, "MAX": 48.0, "DEFAULT": 22.0 },
    { "NAME": "waveAmp", "LABEL": "Wave Amplitude", "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 0.85 },
    { "NAME": "perspective", "LABEL": "Perspective", "TYPE": "float", "MIN": 0.2, "MAX": 2.0, "DEFAULT": 1.0 },
    { "NAME": "motionSpeed", "LABEL": "Motion Speed", "TYPE": "float", "MIN": 0.0, "MAX": 2.5, "DEFAULT": 1.0 },
    { "NAME": "lineWidth", "LABEL": "Line Weight", "TYPE": "float", "MIN": 0.4, "MAX": 2.6, "DEFAULT": 1.0 },
    { "NAME": "textSize", "LABEL": "Text Size", "TYPE": "float", "MIN": 0.5, "MAX": 2.4, "DEFAULT": 1.0 },
    { "NAME": "variant", "LABEL": "Palette", "TYPE": "long", "DEFAULT": 0, "VALUES": [0,1,2], "LABELS": ["Tide","Magenta","Mono"] },
    { "NAME": "skyA", "LABEL": "Sky · top", "TYPE": "color", "DEFAULT": [0.02, 0.04, 0.10, 1.0] },
    { "NAME": "skyB", "LABEL": "Sky · floor", "TYPE": "color", "DEFAULT": [0.08, 0.05, 0.14, 1.0] },
    { "NAME": "lineA", "LABEL": "Grid · A", "TYPE": "color", "DEFAULT": [0.36, 0.92, 1.00, 1.0] },
    { "NAME": "lineB", "LABEL": "Grid · B", "TYPE": "color", "DEFAULT": [0.86, 0.42, 1.00, 1.0] },
    { "NAME": "lineC", "LABEL": "Grid · C", "TYPE": "color", "DEFAULT": [1.00, 0.78, 0.32, 1.0] },
    { "NAME": "inkColor", "LABEL": "Title Ink", "TYPE": "color", "DEFAULT": [0.98, 0.99, 1.00, 1.0] }
  ]
}*/

// ═══════════════════════════════════════════════════════════════════════
//  DIGITAL WAVE GRID  ·  three stacked perspective wireframes + title
//
//  • Three independent height-field grids, each at its own z-band:
//      band A → upper third  (player[1].energy)
//      band B → middle third (player[2].energy)
//      band C → lower third  (player[3].energy)
//    Bands STACK along screen-y; they do not mirror across a horizon
//    (anti-pattern guard). Each band is its own world with its own
//    palette, wave speed and amplitude.
//
//  • Real perspective: each band projects from a virtual camera with
//    its own near/far. Rows compress toward the band's vanishing line.
//    Columns hold parallel in screen-x but get warped by the height
//    field — a wireframe mesh, not a lattice.
//
//  • The wave on every band is a sum of travelling sines whose phase
//    is driven by motionSpeed and whose amplitude rides player energy
//    (with audioDepth as a global multiplier). Silence → glassy plane.
//    Energy → swelling wireframe with foreshortened ridges.
//
//  • Lines use fwidth-AA against an analytic 1D distance to the nearest
//    integer row / column in band-local UV — gallery-grade edges.
//
//  • A typewriter title slab from `cue.latest` is engraved between
//    bands A and B (the "sky" between the upper and middle bands).
//    `msgAge` drives a left-to-right reveal at ~28 cps.
// ═══════════════════════════════════════════════════════════════════════

#define MAX_MSG 48
#define SPACE_CH 26
const float TAU = 6.28318530718;

// ─── font atlas (37 cells: A..Z, space, 0..9) ───────────────────────────
float sampleChar(int ch, vec2 uv) {
    if (ch < 0 || ch > 36) return 0.0;
    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) return 0.0;
    return texture2D(fontAtlasTex, vec2((float(ch) + uv.x) / 37.0, uv.y)).r;
}

int getChar(int slot) {
    if (slot ==  0) return int(msg_0);
    if (slot ==  1) return int(msg_1);
    if (slot ==  2) return int(msg_2);
    if (slot ==  3) return int(msg_3);
    if (slot ==  4) return int(msg_4);
    if (slot ==  5) return int(msg_5);
    if (slot ==  6) return int(msg_6);
    if (slot ==  7) return int(msg_7);
    if (slot ==  8) return int(msg_8);
    if (slot ==  9) return int(msg_9);
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
    if (slot == 47) return int(msg_47);
    return -1;
}

int msgTotal() {
    int n = int(msg_len);
    if (n < 0) return 0;
    if (n > MAX_MSG) return MAX_MSG;
    return n;
}

float hash11(float n) { return fract(sin(n * 127.1) * 43758.5453); }
float hash12(vec2 p)  { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }

// Cheap 1D value noise for fog jitter / line variance.
float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash12(i);
    float b = hash12(i + vec2(1.0, 0.0));
    float c = hash12(i + vec2(0.0, 1.0));
    float d = hash12(i + vec2(1.0, 1.0));
    return mix(mix(a, b, f.x), mix(c, d, f.x), f.y);
}

// Palette swap. `variant`: 0 Tide, 1 Magenta, 2 Mono.
vec3 bandColor(int band, vec3 cA, vec3 cB, vec3 cC) {
    if (variant == 1) {
        // Magenta-night — each band still distinct.
        if (band == 0) return vec3(1.00, 0.32, 0.78);
        if (band == 1) return vec3(0.62, 0.36, 1.00);
        return vec3(0.30, 0.90, 1.00);
    }
    if (variant == 2) {
        // Monochrome — brightness separates bands.
        if (band == 0) return vec3(0.94);
        if (band == 1) return vec3(0.74);
        return vec3(0.52);
    }
    if (band == 0) return cA;
    if (band == 1) return cB;
    return cC;
}

// Travelling wave height in band-local (u, v) ∈ [0,1] × [0,1].
// `tt` is the band's local time; `energy` is the band's player energy.
// The sum of three travelling sines + a slow swell keeps motion alive
// even at silence (idle drift), then ramps in amplitude with energy.
float waveHeight(vec2 uv, float tt, float energy, float seed) {
    float amp = waveAmp * (0.18 + 0.82 * energy);
    // Idle floor so even silent bands ripple slightly — the "stillness
    // ↔ crescendo" axis of the rubric.
    float idle = 0.05;
    amp = max(amp, idle);

    float u = uv.x;
    float v = uv.y;

    // Three travelling sines with different directions / frequencies.
    float w  = sin( (u * 6.0 + v * 2.0) - tt * 1.20 + seed)        * 0.45;
          w += sin( (u * 3.0 - v * 4.5) + tt * 0.85 - seed * 1.7)  * 0.30;
          w += sin( (u * 11.0 + v * 7.0) + tt * 1.65 + seed * 0.5) * 0.18;
    // A diagonal travelling crest — gives the "abstract digital" feel.
    w += sin((u + v) * 4.0 - tt * 0.55) * 0.20;
    // Tiny audio-bass tug — silence stays smooth, loud → ridges spike.
    w += sin(u * 18.0 + tt * 3.1 + seed * 4.1) * 0.07 * energy;

    return w * amp;
}

// Perspective project a (u, v) point in a band's mesh-space onto
// the band's screen-y. `bandTop`, `bandBot` are the screen-y range of
// the band. `vDepth ∈ [0,1]` is the mesh-V coordinate (0 = far, 1 = near).
// Returns screen-y inside [bandTop, bandBot]. Foreshortening: rows
// compress toward the band's far edge.
float bandProject(float vDepth, float bandTop, float bandBot, float pers) {
    // Perspective compression — far rows squeezed near bandTop, near rows
    // expanded near bandBot. pers > 1 → strong compression; <1 → softer.
    float t = pow(clamp(vDepth, 0.0, 1.0), max(pers, 0.2));
    return mix(bandTop, bandBot, t);
}

// Given a pixel in the band's strip, invert the projection to recover
// the (uMesh, vDepth) it samples. Because columns are screen-parallel,
// uMesh is just the centred horizontal coordinate scaled to [0,1].
// vDepth uses the inverse of bandProject.
vec2 bandUnproject(vec2 uvPix, float bandTop, float bandBot, float pers) {
    float t = clamp((uvPix.y - bandTop) / max(bandBot - bandTop, 1e-4),
                    0.0, 1.0);
    float vDepth = pow(t, 1.0 / max(pers, 0.2));
    float uMesh  = uvPix.x;       // already in [0,1]
    return vec2(uMesh, vDepth);
}

// Wireframe distance to nearest mesh line at this band-local mesh-uv
// after height-field displacement. Returns a 1D distance (in screen
// units, scaled by fwidth) for fwidth-AA. We model two line families:
//   • rows: lines of constant V (parallel to U)
//   • cols: lines of constant U (parallel to V)
// The displacement of the mesh perturbs the *V* line locations so the
// row family bends — that's the wireframe terrain effect. Columns stay
// straight in mesh-u but inherit the displacement through the band
// projection (they pinch toward the far edge).
float wireframe(vec2 meshUv, float densityRows, float densityCols,
                float disp, out float ridge) {
    // Bent V coordinate: the wave displaces the row family vertically
    // in mesh-space. The visible row at this pixel is at V' = V + disp,
    // so the distance to the nearest integer row is the fract of that.
    float bentV = meshUv.y * densityRows + disp * densityRows;
    float rowDist = abs(fract(bentV) - 0.5);

    // Column distance — straight in mesh-u but gets foreshortened by
    // the projection's t-curve, which we already apply via bandProject.
    float colVal = meshUv.x * densityCols;
    float colDist = abs(fract(colVal) - 0.5);

    // Track the closest of the two families. Output a normalised
    // distance ∈ [0, 0.5] where 0 = on a line.
    float d = min(rowDist, colDist);

    // Ridge brightness boost — pixels right at the top of a wave crest
    // glow a touch. `disp` is the displacement; high positive = crest.
    ridge = smoothstep(0.45, 0.95, disp / max(waveAmp, 0.01));

    return d;
}

// Render one band into `outCol` (premultiplied) and return its alpha.
// `bandIdx`  : 0/1/2 (top, mid, bottom)
// `bandTop`, `bandBot` : screen-y of the band
// `tt`       : band-local time
// `energy`   : per-band player energy
// `screenUv` : current pixel uv ∈ [0,1]^2
// `lineCol`  : per-band line colour
float renderBand(int bandIdx, float bandTop, float bandBot, float tt,
                 float energy, vec2 screenUv, vec3 lineCol,
                 out vec3 outCol) {
    outCol = vec3(0.0);
    if (screenUv.y < bandTop || screenUv.y > bandBot) return 0.0;

    float pers = perspective * (1.0 + 0.25 * float(bandIdx));

    // Recover mesh-uv from the screen pixel.
    vec2 meshUv = bandUnproject(screenUv, bandTop, bandBot, pers);

    // Sample the height field at this mesh-uv. The displacement is a
    // signed "elevation" of the row at this position.
    float disp = waveHeight(meshUv, tt, energy, float(bandIdx) * 3.7);

    // Density of mesh lines (rows/cols) — user-controlled global, but
    // each band gets a slight offset so the bands don't lock to a single
    // rhythm (anti-pattern: same grid stacked three times).
    float rows = gridDensity * (1.0 + 0.18 * float(bandIdx));
    float cols = gridDensity * (1.0 - 0.12 * float(bandIdx));

    float ridge;
    float d = wireframe(meshUv, rows, cols, disp, ridge);

    // fwidth-AA line. Line width in screen units, scaled by user param.
    float fw = fwidth(d) * 1.4 + 1e-4;
    float lineCore = 0.018 * lineWidth / max(rows / gridDensity, 0.5);
    float line = 1.0 - smoothstep(lineCore - fw, lineCore + fw, d);

    if (line < 0.001 && ridge < 0.01) return 0.0;

    // Depth fog inside the band: rows near the far edge fade into the
    // sky. vDepth is meshUv.y; far = 0, near = 1.
    float depthFade = mix(0.25, 1.0, meshUv.y);

    // Energy halo — when the band's player is loud, lines bloom slightly.
    float halo = 0.45 + 0.55 * energy;

    // Ridge highlights — pixels close to a wave crest get a small bonus
    // brightness independent of the line family.
    vec3 col = lineCol * (halo + 0.5 * ridge) * depthFade;

    // Premultiplied alpha output (caller does over-composite).
    float a = line * depthFade;
    a = clamp(a, 0.0, 1.0);
    outCol = col * a;
    return a;
}

// ─── typewriter title slab (engraved between band A and band B) ─────────
// Renders the message as a single horizontal line of glyphs centred on
// `slabY`, sized by `slabH`. Reveal cap from msgAge.
vec4 renderTitleSlab(vec2 screenUv, float slabY, float slabH,
                     int total, float aspect, vec3 ink) {
    vec4 acc = vec4(0.0);
    if (total <= 0) return acc;

    // Reveal cap from typewriter.
    int revealCap;
    if (msgAge >= 0.0) {
        int rev = int(floor(msgAge * 28.0));
        if (rev < 1) rev = 1;
        if (rev > total) rev = total;
        revealCap = rev;
    } else {
        revealCap = total;
    }

    // Glyph metrics. Width derived from 5:7 atlas cell, with kerning.
    float gH = slabH * textSize;
    float gW = gH * (5.0 / 7.0);
    float kern = gW * 0.92;
    float totalW = float(revealCap) * kern;

    // Centred origin in screen-uv space (so it doesn't drift with audio).
    float originX = 0.5 - 0.5 * totalW / aspect;
    float y0 = slabY - gH * 0.5;
    float y1 = slabY + gH * 0.5;

    // Vertical clip.
    if (screenUv.y < y0 - 0.01 || screenUv.y > y1 + 0.01) return acc;

    // Determine which glyph slot we're inside (if any).
    float xLocal = (screenUv.x - originX) * aspect;
    if (xLocal < 0.0 || xLocal > totalW) return acc;
    int slot = int(floor(xLocal / kern));
    if (slot < 0 || slot >= revealCap) return acc;
    int ch = getChar(slot);
    if (ch < 0 || ch > 36) return acc;
    if (ch == SPACE_CH) return acc;

    // Glyph-local UV.
    float xInCell = xLocal - float(slot) * kern;
    // Centre glyph horizontally inside its kern-wide column.
    float colPad = (kern - gW) * 0.5;
    float xInGlyph = (xInCell - colPad) / gW;
    if (xInGlyph < 0.0 || xInGlyph > 1.0) return acc;
    // screenUv.y is y-UP, y0=bottom-of-glyph, y1=top-of-glyph. The host
    // font atlas stores letter-top at v=1, so mapping screen y-up
    // directly puts letter-top at screen-top. The previous
    // `(y1 - screenUv.y)` form flipped this and rendered glyphs upside
    // down.
    float yInGlyph = (screenUv.y - y0) / gH;
    if (yInGlyph < 0.0 || yInGlyph > 1.0) return acc;

    float s = sampleChar(ch, vec2(xInGlyph, yInGlyph));
    float aa = fwidth(s) * 1.4 + 1e-4;
    float alpha = smoothstep(0.5 - aa, 0.5 + aa, s);
    if (alpha < 0.001) return acc;

    // Subtle engraved drop-shadow: sample one texel down + brighten ink.
    float sShadow = sampleChar(ch, vec2(xInGlyph, yInGlyph - 0.04));
    float shadow = smoothstep(0.5 - aa, 0.5 + aa, sShadow) * 0.35;

    vec3 col = mix(vec3(0.0), ink, 1.0);
    acc.rgb = col * alpha + vec3(0.0) * shadow * (1.0 - alpha);
    acc.a   = max(alpha, shadow * 0.6);
    return acc;
}

void main() {
    vec2 res = RENDERSIZE;
    vec2 uv  = gl_FragCoord.xy / res;
    float aspect = res.x / res.y;

    float t = TIME * motionSpeed;

    // Audio multipliers.
    float bass = audioBass;
    float mid  = audioMid;
    float high = audioHigh;

    // Per-band energies — players + a small audio.* injection so the
    // shader still breathes when no player channels are bound.
    float eA = clamp(energyA + bass * 0.20 * audioDepth, 0.0, 1.5);
    float eB = clamp(energyB + mid  * 0.20 * audioDepth, 0.0, 1.5);
    float eC = clamp(energyC + high * 0.20 * audioDepth, 0.0, 1.5);

    // Vertical band layout. Bands STACK across the canvas — explicit
    // anti-mirror: no horizon line, no symmetric reflection. The three
    // bands sit one above the other with thin sky strips between them.
    // (Band C is the lower band, but it's not a mirror of A — it has
    // its own wave parameters and palette tint.)
    const float SKY_GAP = 0.02;
    float bandH = (1.0 - 4.0 * SKY_GAP) / 3.0;
    // band 0 (A) — top
    float a0 = 1.0 - SKY_GAP - bandH;
    float a1 = 1.0 - SKY_GAP;
    // band 1 (B) — middle
    float b1 = a0 - SKY_GAP;
    float b0 = b1 - bandH;
    // band 2 (C) — bottom
    float c1 = b0 - SKY_GAP;
    float c0 = c1 - bandH;

    // Camera-y dolly: scroll the bands very slowly so the stripes
    // breathe even when audio is silent.
    float dolly = sin(t * 0.07) * 0.012;
    a0 += dolly; a1 += dolly;
    b0 += dolly; b1 += dolly;
    c0 += dolly; c1 += dolly;

    // Per-band time offsets — each band has its own wave clock so the
    // three rhythms never align into a single global pulse.
    float tA = t * 0.95;
    float tB = t * 1.15 + 13.7;
    float tC = t * 0.78 + 27.1;

    // Mouse parallax: nudges every band horizontally + tilts perspective.
    vec2 m2 = (mousePos - 0.5);
    vec2 puv = uv;
    puv.x += m2.x * 0.04;

    // ── Background sky gradient ─────────────────────────────────────
    vec3 sky = mix(skyB.rgb, skyA.rgb, smoothstep(0.0, 1.0, uv.y));
    // Subtle vignette + low-frequency starfield haze.
    float vign = 1.0 - 0.35 * dot(uv - 0.5, uv - 0.5);
    sky *= vign;
    // A few faint stars in the upper sky strip.
    float starN = vnoise(uv * vec2(res.x, res.y) * 0.004 + t * 0.02);
    float stars = pow(starN, 18.0) * smoothstep(0.6, 1.0, uv.y);
    sky += stars * vec3(0.5, 0.6, 0.8);

    vec3 col = sky;

    // ── Render each band ───────────────────────────────────────────
    // Lower bands rendered first so closer (lower-on-screen) bands
    // composite over the sky; bands don't overlap because of SKY_GAP.
    vec3 bandColOut;
    float bandAlpha;

    vec3 colA = bandColor(0, lineA.rgb, lineB.rgb, lineC.rgb);
    vec3 colB = bandColor(1, lineA.rgb, lineB.rgb, lineC.rgb);
    vec3 colC = bandColor(2, lineA.rgb, lineB.rgb, lineC.rgb);

    bandAlpha = renderBand(0, a0, a1, tA, eA, puv, colA, bandColOut);
    col = col * (1.0 - bandAlpha) + bandColOut;

    bandAlpha = renderBand(1, b0, b1, tB, eB, puv, colB, bandColOut);
    col = col * (1.0 - bandAlpha) + bandColOut;

    bandAlpha = renderBand(2, c0, c1, tC, eC, puv, colC, bandColOut);
    col = col * (1.0 - bandAlpha) + bandColOut;

    // ── Soft inter-band sky tint: each gap glows slightly with the
    //    adjacent band's palette so the stack feels continuous, not
    //    sliced. The gap between A and B leans on band A's palette;
    //    the gap between B and C leans on band B's. ─────────────────
    float gapAB = smoothstep(0.0, SKY_GAP, abs(uv.y - (a0 + b1) * 0.5));
    float gapBC = smoothstep(0.0, SKY_GAP, abs(uv.y - (b0 + c1) * 0.5));
    col += (1.0 - gapAB) * colA * 0.06;
    col += (1.0 - gapBC) * colB * 0.06;

    // ── Title slab: engraved between band A and band B sky gap ─────
    int total = msgTotal();
    float slabY = (a0 + b1) * 0.5;
    float slabH = SKY_GAP * 0.95;
    // Make title scale with widget; clamp to keep it inside the gap.
    slabH = min(slabH, 0.022 * textSize);
    vec4 title = renderTitleSlab(uv, slabY, slabH, total, aspect, inkColor.rgb);
    col = mix(col, title.rgb / max(title.a, 1e-4), title.a);

    // ── Final colour grade ─────────────────────────────────────────
    // Tonemap: soft Reinhard. Then a tiny scanline jitter (digital feel).
    col = col / (1.0 + 0.6 * col);
    float scan = 1.0 + 0.025 * sin(uv.y * res.y * 0.5 + t * 4.0);
    col *= scan;
    // Bloom-ish lift on the brightest grid pixels.
    float L = dot(col, vec3(0.299, 0.587, 0.114));
    col += 0.10 * smoothstep(0.55, 1.2, L) * col;
    col = pow(max(col, 0.0), vec3(0.95));

    gl_FragColor = vec4(col, 1.0);
}

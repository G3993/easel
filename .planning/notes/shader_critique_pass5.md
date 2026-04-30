# Shader Critique — Round 5: Genre vs. Specific-Work Test

Rounds 1–4 fixed motion, anti-Voronoi, parameter sanity, and performance. Round 5 asks: do the defaults evoke a SPECIFIC canonical work, or just "this art movement in the abstract"? A Pollock that reads as "drippy abstract paint" passes the genre test but fails the work test. A Pollock that reads as *Autumn Rhythm 1950* passes the work test.

Method: mental side-by-side of each shader's default output against this movement's top-3 canonical works (per `art_movement_canon.md`). Look for palette match, compositional grammar, iconography (signifier objects/marks), and scale/density.

The 18 shaders are mostly competent at the genre level. Most fail the work test in one of three ways:
1. **Palette is "the right family" but not the canonical exact set** (Pollock has cadmium red but no aluminium silver visible at default; Mondrian has primaries but on neutral white not warm-paper-cream).
2. **Composition is generic-of-movement instead of specific-of-work** (Stella defaults to rainbow rings, not Hyena Stomp's literal palette march; Mondrian has marching pulses but no rectangular CELL fills which are the actual Boogie Woogie signifier).
3. **Iconography is procedural-suggestion not legible-mark** (Mucha has whiplash strokes but no halo arc; Magritte has bowler hat but no Son-of-Man green apple over face; Lichtenstein speech bubble has a generic word not a hand-lettered "WHAAM").

What follows is opinionated. The fixes prioritise the ONE change per shader that pushes it from "neat but generic" to "ah, that's clearly [movement]".

---

## abex_pollock.fs

**Visual signature target (top works):** Autumn Rhythm Number 30 (1950) · Number 1A (1948) · Lavender Mist (1950).

**Current output reads as:** correct Pollock genre — drip lattice on raw canvas, multiple drippers, palette has the right black/white/silver/red/ochre. But strokes deposit as DOTS at each frame's dripper head, then accumulate in the persistent buffer over time. Result: a *trail of stamps* rather than continuous skein. At default density (12 drippers, 0.0035 width), the canvas reads thin and DOTTED, not lattice. Autumn Rhythm at 17 ft has ribbons that visually OVERLAP and CROSS. Lavender Mist is denser still.

**Specific gaps vs canon:**
- Palette: ✅ correct. POL_BLACK / WHITE / SILVR / RED / OCHRE on RAW_CANVAS. Black bias of 0.45 is right for Autumn Rhythm.
- Composition: ❌ density too low and strokes too separated. Pollock's defining read is lattice DENSITY — you can't see the canvas through the centre. Default `drippers=12, strokeWidth=0.0035` → maybe 8% canvas coverage; canvas surface of ~70% remains visible. Wrong.
- Iconography: ⚠️ flick-and-pool exists (`pooling` at L82) but no HANDPRINTS at top edge (Number 1A signature) and no LONG-RIBBON continuity. Each dripper deposits one circle per frame, leaving a dotted path not a fluid pour.
- Scale/density: ❌ scale suggests easel-sized canvas, not 17-ft barn-floor canvas. The visual density required to evoke Autumn Rhythm is roughly 3× this.

**Single highest-leverage fix:** raise default density. Change `drippers` DEFAULT from 12 → 22, `strokeWidth` DEFAULT from 0.0035 → 0.006, and INSIDE the deposit loop (line ~128–146) interpolate between current and previous dripper position so each frame stamps a SHORT LINE, not a single point. Pseudocode for the change:

```glsl
vec2 pPrev = dripperPos(i, t - 0.05 + ..., turbulence, 1.0);
vec2 pNow  = dripperPos(i, t + ..., turbulence, 1.0);
// distance from uv to the SEGMENT pPrev→pNow, not point pNow
vec2 ab = pNow - pPrev;
float h = clamp(dot(uv - pPrev, ab) / max(dot(ab, ab), 1e-6), 0.0, 1.0);
vec2 cl = pPrev + ab * h;
vec2 d  = uv - cl; d.x *= aspect;
float ds = length(d);
```

This converts each dripper's per-frame deposit from a stamp to a 1/20-second ribbon segment. After one second of accumulation in `paintBuf` you get continuous skeins, not bead chains. ~12 lines changed.

---

## abex_rothko.fs

**Visual signature target (top works):** Orange Red Yellow (1961) · Black on Maroon Seagram Mural (1958) · No. 14 White and Greens in Blue (1957).

**Current output reads as:** very strong Rothko — three soft-edged horizontal bands on chromatic ground with feathered edges, slow shimmer, vignette, grain. Default palette is exactly Orange Red Yellow (1961). This is the closest-to-canon shader in the set.

**Specific gaps vs canon:**
- Palette: ✅ default `topColor [0.92,0.50,0.22]`, `midColor [0.85,0.20,0.14]`, `botColor [0.95,0.78,0.30]` on `groundColor [0.32,0.10,0.10]` is literally Orange Red Yellow. Excellent.
- Composition: ⚠️ very close. The canonical 3-band layout is roughly 1/3 each with a thin gap of ground between. Current ranges (0.62-0.92, 0.34-0.58, 0.08-0.30) are good but the bands are slightly too SHORT — Rothko's bands typically span ~80% of the canvas WIDTH inside a ~6% inset. `innerInset=0.06` is right but the Y-bands at default extend only ~0.30/0.24/0.22 high, when canon is ~0.27 each separated by ~0.04 ground. Already very close.
- Iconography: ✅ feathered edges (feather=0.12) sell it. Edge undulation in `bandShape` is a nice touch.
- Scale/density: ⚠️ Rothko's canvases are TALLER than they are wide (Orange Red Yellow is 93×81 in.). At a typical 16:9 viewport this ratio is wrong and the bands flatten. Not fixable inside the shader, but the default feather could compensate by being SLIGHTLY larger to imply contemplative scale.

**Single highest-leverage fix:** add a fourth band option as the default — Seagram Mural's *Black on Maroon* is the second-most-recognisable Rothko and currently inaccessible from default `bandCount=3`. Add at the top of `main()`:

```glsl
// Subtle painterly inner glow — band centres slightly brighter than
// edges so each rectangle reads as having internal radiance, not flat fill
float bandCenterY = (yLo + yHi) * 0.5;
float dyFromCenter = abs(uv.y - bandCenterY) / ((yHi - yLo) * 0.5 + 1e-4);
yMask *= 1.0 + 0.08 * (1.0 - dyFromCenter * dyFromCenter);
```

Insert at end of `bandShape` before return. ~5 lines. Gives the chapel-painting inner-glow that distinguishes Rothko from "stacked rectangles". Plus bump default `feather` from 0.12 → 0.16 — at 0.12 the edges are still slightly too crisp for No. 14 / Seagram.

---

## ai_latent_drift.fs

**Visual signature target (top works):** Refik Anadol *Machine Hallucinations: MoMA* (2022) · Anadol *Unsupervised* (2022) · Anna Ridler *Mosaic Virus* (2018).

**Current output reads as:** generic "soft-edged morphing colour clouds" — passes the AI-art feel test but not the Anadol-specifically test. Anadol's installations have a SHIMMERING SPECULAR PIXEL-STORM quality — they look like wet, flowing pigment with thousands of tiny glints, not just a slow blurry colour wash. Klingemann's GAN-portraits have FACELIKE PROBABILITY GHOSTS which the phantom-pulse faintly suggests but not strongly enough.

**Specific gaps vs canon:**
- Palette: ✅ cream-mauve-teal-orange muted arc is correct for Anadol.
- Composition: ⚠️ the curl-warped fbm is the right base operation but Anadol's signature is HIGHER-FREQUENCY DETAIL on top of the smooth flow — fine grain pixel storms riding the macro currents. Current `phantomScale=14` is too coarse and `phantomPulse` is gated low at 0.35.
- Iconography: ❌ no specular glint pass. Anadol's *Unsupervised* shimmers because pigment caustics catch light. Currently the surface is matte.
- Scale/density: ⚠️ fine.

**Single highest-leverage fix:** add a sparse high-frequency specular sparkle pass keyed to the macro flow direction. Insert before the audio luminance breath (around line 150):

```glsl
// Anadol pigment-storm specular — sparse high-frequency micro-glints
// riding the curl direction, picking up "wet pigment" highlights
float sparkScale = 80.0;
vec2 sg = floor(uv * sparkScale + cl * 8.0);
float sh = hash21(sg + floor(TIME * 4.0));
if (sh > 0.985) {
    float sparkIntensity = pow(latentSpeed * 6.0, 0.4);
    col += vec3(0.95, 0.92, 0.85) * sparkIntensity * 0.35;
}
```

~9 lines. Plus drop `phantomScale` DEFAULT from 14 → 22 and `phantomPulse` DEFAULT from 0.35 → 0.55 — phantom features are the GAN-emergence signifier and need to be more visible.

---

## art_nouveau_mucha.fs

**Visual signature target (top works):** Mucha *Gismonda* (1894) · Mucha *Job Cigarette Poster* (1896) · Klimt *The Kiss* (1907).

**Current output reads as:** "flowing whiplash curves on pastel ground" — passes Mucha-genre test. But Mucha's iconic posters all share one massive structural device the shader entirely lacks: a **HALO ARC** behind the figure (Byzantine medallion on Gismonda, decorative arch on Job, full circle on Zodiac). Without the halo, Mucha's posters wouldn't read as Mucha. The shader is missing his single most recognisable compositional signifier.

**Specific gaps vs canon:**
- Palette: ✅ cream / sage / rose for Mucha; deep gold for Klimt. Correct.
- Composition: ❌ **no halo / decorative arc / mandala-ground**. Mucha's posters always have a centred or asymmetric large arc behind the strokes. This is the difference between Mucha and "art nouveau wallpaper".
- Iconography: ⚠️ Klimt-style mosaic tiles exist (line 56, the floor(uv*80) gold tiles) but at default style=0 (Mucha) they're absent. Beardsley style=2 lacks the dense black silhouette masses that are the Beardsley signifier.
- Scale/density: ⚠️ tendrils accumulate as continuous traces (good) but they're scattered across the canvas without a focal point. Mucha's posters have ALL strokes radiating from / framing a central figure.

**Single highest-leverage fix:** add a halo arc to the field for style=0 (Mucha). In `fieldColor`, style 0 branch:

```glsl
if (style == 0) {
    vec3 cream = vec3(0.97, 0.93, 0.84);
    vec3 rose  = vec3(0.94, 0.80, 0.78);
    vec3 sage  = vec3(0.78, 0.86, 0.74);
    vec3 mid   = mix(sage, rose, warm);
    vec3 base = mix(cream, mid, smoothstep(0.0, 1.0, uv.y));
    // Byzantine halo arc — large soft circle behind upper-half centre,
    // gilded edge ring. The Mucha signature.
    vec2 halo = uv - vec2(0.5, 0.62);
    float r = length(halo);
    float arc = smoothstep(0.36, 0.30, r) - smoothstep(0.30, 0.24, r);
    base = mix(base, vec3(0.92, 0.78, 0.36), arc * 0.55);
    base = mix(base, mid * 0.92, smoothstep(0.36, 0.0, r) * 0.18);
    return base;
}
```

~10 lines. Adds the missing iconography. Adjacent: halo strength could be a uniform but can ship hard-coded.

---

## bauhaus_kandinsky.fs

**Visual signature target (top works):** Kandinsky *Composition VIII* (1923) · *Several Circles* (1926) · *Yellow-Red-Blue* (1925).

**Current output reads as:** correct Bauhaus genre — primary triangle/square/circle on warm white with halo glow and support lines. The strict-pairing is exactly right pedagogically. This is one of the strongest shaders in the set.

**Specific gaps vs canon:**
- Palette: ✅ Bauhaus warm white, cadmium yellow, vermilion red, ultramarine blue. Correct.
- Composition: ⚠️ N=13 random shapes orbiting in Lissajous patterns. *Composition VIII* has ~20 distinct elements but they READ as a balanced composition with WEIGHTED LEFT/RIGHT regions: a dense circle cluster top-left, sharp diagonals bottom-right, chequerboard quadrants. The current shader's Lissajous places shapes randomly and they orbit, blurring composition into "moving shapes".
- Iconography: ⚠️ chequerboard pattern is missing — *Composition VIII* features chequerboard FILLS inside some shapes, not just halos. Concentric arc patterns (the music-on-paper element) are missing too.
- Scale/density: ✅ fine.

**Single highest-leverage fix:** for ~25% of shapes (one in four), swap the solid fill for a chequerboard fill — pulls toward Composition VIII directly. Modify the SDF render block around line 145:

```glsl
int t = int(mod(fi, 3.0));
if (sd < bestSD) {
    bestSD = sd;
    // Every 4th shape gets a chequerboard interior — Composition VIII
    // signature pattern. At fi%4==3 we substitute the fill colour with
    // a 2-colour chequer derived from local-plane coords.
    vec2 chk = floor(lp / (sz * 0.32));
    bool chkOn = (mod(chk.x + chk.y, 2.0) < 1.0);
    bool useChk = (mod(fi, 4.0) >= 3.0);
    vec3 baseC = (t == 0) ? yellowC : (t == 1) ? redC : blueC;
    vec3 altC  = vec3(0.05, 0.05, 0.06);
    bestCol = useChk ? (chkOn ? baseC : altC) : baseC;
}
```

~10 lines. Pulls the output from "Several Circles" (which is the only Kandinsky everyone knows) toward the more pedagogically-Kandinsky *Composition VIII*.

---

## constructivism_lissitzky.fs

**Visual signature target (top works):** El Lissitzky *Beat the Whites with the Red Wedge* (1919) · Rodchenko *Books! In all branches of knowledge* (1924) · Lissitzky *Proun 19D* (1922).

**Current output reads as:** correct *Beat the Whites with the Red Wedge* — red triangle slamming into white circle on cream paper, with diagonal black bars and Cyrillic-stencil glyphs. Maybe the SECOND-strongest shader in the set, after rothko.

**Specific gaps vs canon:**
- Palette: ✅ cream-red-white-black, exactly right.
- Composition: ✅ wedge-vs-circle structure is the literal Lissitzky composition. ✅ Bars at diagonals.
- Iconography: ⚠️ Cyrillic-stencil glyphs (line 70) are TOO SPARSE at default `glyphIntensity=0.6`. Actual Lissitzky/Rodchenko posters have BLOCK Cyrillic typography that occupies ~15-25% of the composition (slogan banners). Currently it's almost unreadable.
- Scale/density: ⚠️ wedge thrust amplitude is fine but the wedge is THIN at default `width=0.18` for length 0.55 — Lissitzky's actual wedge is closer to width/length = 0.4 (more equilateral). Currently it reads as a "blade" not a "wedge".

**Single highest-leverage fix:** thicken the wedge and amplify the glyph clusters. Change default `triangleAngle` from 0.55 to 0.7, change wedge call at line 124 from `wedge(uv, apex, -axis, 0.55, 0.18)` to `wedge(uv, apex, -axis, 0.55, 0.32)`, and add a third large glyph cluster. In `glyphField`, add a fourth iteration covering the upper-left or upper-right quadrant at a much larger scale (≈ 5× current) for a poster-banner read:

```glsl
// Banner-scale typography across top of poster (Rodchenko-style).
{
    vec2 origin = vec2(0.10, 0.05);
    origin.x += sin(TIME * 0.15) * 0.01;
    vec2 local = (uv - origin) * vec2(7.0, 3.5);
    if (!(any(lessThan(local, vec2(0.0))) ||
          local.x > 5.0 || local.y > 1.0)) {
        vec2 ci = floor(local);
        float h = hash21(ci);
        float vert = step(h, 0.55) * step(0.18, fract(local.x))
                   * step(fract(local.x), 0.55);
        float bar = step(0.55, h) * step(h, 0.85)
                   * step(0.40, fract(local.y))
                   * step(fract(local.y), 0.62);
        total = max(total, max(vert, bar));
    }
}
```

~15 lines. Plus widening the wedge. After this it reads as the canonical poster.

---

## cubism_picasso.fs

**Visual signature target (top works):** Picasso *Portrait of Daniel-Henry Kahnweiler* (1910) · *Ma Jolie* (1911) · Braque *Violin and Candlestick* (1910).

**Current output reads as:** correct analytic-cubism genre — translucent overlapping rectangles, ochre-grey LUT, optional letter fragments. The N-translucent-planes approach is the right grammar.

**Specific gaps vs canon:**
- Palette: ✅ ochre/umber/grey LUT is dead-on for Kahnweiler 1910.
- Composition: ⚠️ planes are placed randomly with `centerBias=0.65`, but Kahnweiler has a clear PYRAMIDAL CONCENTRATION in the centre with sparser planes at edges. Currently it's more uniformly distributed.
- Iconography: ⚠️ letter fragments are present at default 0.35 but are PROCEDURAL VERTICAL/HORIZONTAL bars — they don't read as letters at painting scale, more like noise. The canonical fragments are ACTUAL letters (J, O, U, M, A) painted in stencil typeface.
- Scale/density: ⚠️ default `planes=11` at `planeSize=0.22` gives reasonable density. Fine.

**Single highest-leverage fix:** make the letter fragments READ as letters — use real glyph SDFs for J, O, U, B, A, L (the canonical Picasso letter scraps). Replace `letterField` to use a small atlas. Compact version using SDF primitives for 4 letters instead of hash-noise:

```glsl
float drawJ(vec2 p) { // p in 0..1 cell
    float bar = step(0.40, p.x) * step(p.x, 0.60) * step(0.20, p.y);
    float hook = step(0.10, p.x) * step(p.x, 0.40) * step(0.10, p.y) * step(p.y, 0.30);
    return max(bar, hook);
}
float drawO(vec2 p) {
    vec2 d = p - 0.5;
    float r = length(d * vec2(1.4, 1.0));
    return step(0.30, r) * step(r, 0.45);
}
float drawU(vec2 p) {
    float L = step(0.10, p.x) * step(p.x, 0.30) * step(0.20, p.y);
    float R = step(0.70, p.x) * step(p.x, 0.90) * step(0.20, p.y);
    float B = step(0.10, p.x) * step(p.x, 0.90) * step(0.10, p.y) * step(p.y, 0.30);
    return max(max(L, R), B);
}
// In letterField, place 3 letter cells at fixed positions, drift slowly.
```

~30 lines. That's the highest-leverage fix; the LUT is already perfect, the planes already look right, but the JOU/MA-JOLIE letter scraps are the iconography that says "Picasso analytic" and currently they don't.

---

## dada_hoch.fs

**Visual signature target (top works):** Höch *Cut with the Kitchen Knife* (1919) · Hausmann *ABCD* (1923) · Höch *The Beautiful Girl* (1920).

**Current output reads as:** chaotic photomontage with halftone, RGB shift, red ink stamps, stuttering letterforms. Passes Dada-genre test. But Höch's *Cut with the Kitchen Knife* has a SPECIFIC compositional logic: dense face cutouts in centre, mechanical parts radiating outward, large fragmented words ("DADA", "Anti-Dada") at top and bottom edges. The current shader reads more like a generic glitch collage than a Höch-specific work.

**Specific gaps vs canon:**
- Palette: ✅ cream/newsprint paperTone is right.
- Composition: ⚠️ random placement; no centre-heavy density. Höch's photomontages have a clear CENTROIDAL concentration of activity.
- Iconography: ❌ no large word "DADA" or "BERLIN" in the canvas. The letter stutter band exists but at thin band-y position, not as poster-scale text. Also no FACE CUTOUTS — every Höch montage features cut-out HEADS as the dominant collage element.
- Scale/density: ⚠️ `paperFade=0.992` is generous; collage builds densely. Good.

**Single highest-leverage fix:** add a single large word like "DADA" stamped at one corner of the collage. Höch's posters always feature one or two HUGE WORDS as anchor points. After the existing letter stutter (~line 200), add:

```glsl
// Anchor word — large fragmented "DADA" stamped at top-right.
{
    vec2 wPos = vec2(0.55, 0.85)
              + 0.005 * vec2(sin(TIME * 0.4), cos(TIME * 0.5));
    vec2 wd = (uv - wPos) * vec2(4.0, 7.0);
    if (wd.x >= 0.0 && wd.x <= 4.0 && wd.y >= 0.0 && wd.y <= 1.0) {
        // Spell DADA as 4 letter cells of width 1.0
        int letterIdx = int(floor(wd.x));
        vec2 cuv = vec2(fract(wd.x), wd.y);
        // D=0, A=1: D is left+top+bot+right-curve; A is left+right+top+midY
        float ink = 0.0;
        if (letterIdx == 0 || letterIdx == 2) {
            // D: left bar + top + bot + right semicircle
            ink = step(0.0, cuv.x) * step(cuv.x, 0.20)
                + step(0.0, cuv.y) * step(cuv.y, 0.15)
                + step(0.85, cuv.y);
            float r = length((cuv - vec2(0.20, 0.5)) * vec2(1.5, 1.0));
            ink += step(0.40, r) * step(r, 0.55);
        } else {
            // A: left + right + top + middle
            ink = step(0.0, cuv.x) * step(cuv.x, 0.18)
                + step(0.82, cuv.x) * step(cuv.x, 1.0)
                + step(0.85, cuv.y)
                + step(0.45, cuv.y) * step(cuv.y, 0.55);
        }
        col = mix(col, vec3(0.78, 0.10, 0.10), clamp(ink, 0.0, 1.0) * 0.85);
    }
}
```

~25 lines. Adds the missing centroid-anchor word. (Less polished alternative: just stamp "DADA" via a hard-coded SDF letter set at fixed position.)

---

## dali_melt.fs

**Visual signature target (top works):** Dalí *The Persistence of Memory* (1931) · *The Elephants* (1948) · *Galatea of the Spheres* (1952).

**Current output reads as:** generic "image sags downward" effect. The procedural Dalí desert fallback (line 26) is a sky-to-ochre gradient, which is right-ish, but there are NO MELTING WATCHES, NO INFINITE-PLAIN-WITH-SHADOW geometry, and no biomorphic blob. This is the weakest art-movement shader in the set — it reads as a generic vertical-melt filter, not Dalí.

**Specific gaps vs canon:**
- Palette: ⚠️ procedural fallback uses sky-to-ochre. Mostly right but Dalí's actual desert palette is more saturated cobalt + bone-yellow with hard horizon line.
- Composition: ❌ no horizon line, no shadow rake, no recognisable melting object. Just a vertical drip effect on whatever input is bound.
- Iconography: ❌ NO WATCHES. The single most iconic Dalí signifier — the melting clock — is absent from the procedural fallback.
- Scale/density: ⚠️ default `sagAmp=0.12` is gentle; the watch in *Persistence of Memory* MELTS dramatically over the branch.

**Single highest-leverage fix:** in the procedural fallback, draw a single melting clock SDF that morphs each frame. Replace `daliDesert` with a richer scene including one watch:

```glsl
// SDF for a melting circular clock face draped over a curved support.
float sdMeltClock(vec2 p, float t) {
    // Clock centre — sags downward over the branch
    vec2 c = vec2(0.5, 0.55);
    vec2 d = p - c;
    // Vertical sag — bottom of clock hangs lower
    float sagY = sin(d.x * 6.0) * 0.04 - max(0.0, d.y * -1.0) * 0.18;
    d.y += sagY;
    // Elliptical clock face
    return length(d * vec2(1.0, 1.4)) - 0.12;
}

vec3 daliDesert(vec2 uv) {
    // Sharp horizon at y=0.45 — Dalí's defining infinite-plain device.
    vec3 sky = mix(vec3(0.95, 0.78, 0.55), vec3(0.32, 0.22, 0.42), 1.0 - uv.y);
    vec3 ground = mix(vec3(0.85, 0.62, 0.30), vec3(0.42, 0.28, 0.18), 1.0 - uv.y);
    float horizon = step(0.45, uv.y);
    vec3 col = mix(ground, sky, horizon);
    // Melting clock
    float cd = sdMeltClock(uv, TIME);
    if (cd < 0.0) {
        col = mix(col, vec3(0.92, 0.85, 0.55), 1.0);
        // Numerals ring at radius 0.10
        // [skipped for brevity — would draw 12 numeral marks]
    }
    // Hard shadow projecting east from clock
    if (uv.y < 0.45) {
        vec2 sp = uv + vec2(-0.15, 0.10);
        if (sdMeltClock(vec2(sp.x, 0.55 + (uv.y - 0.45)), TIME) < 0.0) {
            col *= 0.6;
        }
    }
    return col;
}
```

~30 lines. This is the BIGGEST upgrade in the entire set — currently dali_melt reads as a generic effect, but adding even a crude clock SDF transforms it into Dalí-specific iconography.

---

## destijl_mondrian.fs

**Visual signature target (top works):** Mondrian *Broadway Boogie Woogie* (1942) · *Composition with Red, Blue, and Yellow* (1930) · Mondrian *Tableau I* (1921).

**Current output reads as:** "Manhattan grid with marching colour pulses" — passes the Boogie Woogie genre test. But classic Mondrian (the COMPOSITION WITH RED BLUE YELLOW that everyone knows) has THICK BLACK BARS dividing the canvas into UNEQUAL RECTANGULAR CELLS, with three or four cells filled with primary colour. The shader does pulses but no CELLS, and that's exactly the canonical Mondrian read it's missing.

**Specific gaps vs canon:**
- Palette: ✅ exact RYB primaries with grey and warm paper.
- Composition: ❌ **no rectangular cell fills**. *Composition with Red, Blue, and Yellow* (1930) — the most reproduced Mondrian — has 6 cells with 3 filled (red, blue, yellow). Boogie Woogie has many small fills along lanes. The shader has only PULSES along lanes, no STATIC FILLS. Without those, it reads as Boogie Woogie only, and even Boogie Woogie has occasional rectangular cell anchors.
- Iconography: ⚠️ pulse marching is the right Boogie Woogie signature.
- Scale/density: ⚠️ at `lanesH=lanesV=6`, the grid is finer than canonical Mondrian (which is more like 3-4 lanes). The visual density of *thin black lines* feels too high.

**Single highest-leverage fix:** add 3-4 large rectangular CELL fills at hashed grid intersections. After the lane-pulse loops (around line 150), add:

```glsl
// Static colour cells — the canonical Mondrian rectangular fills.
// Pick 3-4 cells at hashed positions and fill with primaries.
for (int c = 0; c < 4; c++) {
    float fc = float(c) + compositionSeed * 5.71;
    float cI = floor(hash11(fc * 1.31) * float(NH));
    float cJ = floor(hash11(fc * 2.97) * float(NV));
    // Cell bounds — between adjacent lanes.
    float yL = cI / float(NH), yH = (cI + 1.0) / float(NH);
    float xL = cJ / float(NV), xH = (cJ + 1.0) / float(NV);
    if (uv.x > xL + laneWidth && uv.x < xH - laneWidth &&
        uv.y > yL + laneWidth && uv.y < yH - laneWidth) {
        vec3 cellC = pickColor(hash11(fc * 9.7),
                               redArea, blueArea, yellowArea, 0.0);
        // Subtle pulse on this cell so it lives.
        float pulse = 0.92 + 0.08 * sin(TIME * 0.3 + fc * 1.7);
        col = mix(col, cellC * pulse, 0.85);
    }
}
```

~17 lines. Cells anchor the canvas into the canonical Mondrian compositional structure. Plus drop the default `lanesH/lanesV` from 6 → 4 (canonical Mondrian has fewer divisions).

---

## expressionism_kirchner.fs

**Visual signature target (top works):** Kirchner *Street, Berlin* (1913) · Nolde *The Last Supper* (1909) · Heckel *Crouching Woman* (1913 woodcut).

**Current output reads as:** correct Die Brücke genre — sheared perspective, ridged carve noise, posterized acid LUT, ink edges. Strong on technique. But the canonical Kirchner *Street, Berlin* has SPECIFIC FIGURES — tall feathered women in green-black coats against pink pavement — and they're the central iconography. The shader's procedural fallback (line 99) is "vertical lamp streaks + windows" which reads more like a generic urban scene than a Kirchner street.

**Specific gaps vs canon:**
- Palette: ⚠️ acid LUT (`vec3(0.7, 1.4, 0.4)` × `vec3(1.4, 0.5, 1.3)`) is the right palette family but at default `acidTint=0.42` it's MUTED. Kirchner's *Street Berlin* has SCREAMING pink-and-green clash, not subtle. Drop acid intensity? No — RAISE saturation in the LUT itself.
- Composition: ⚠️ shear is right but the underlying procedural scene has no figures. Kirchner without figures = Heckel landscape, not Kirchner.
- Iconography: ❌ no humanoid silhouettes. Even crude vertical-blob figures would push this from "carved-wood texture" to "Berlin street with people".
- Scale/density: ✅ ok.

**Single highest-leverage fix:** add 3-5 elongated humanoid silhouettes to the procedural fallback. They don't have to be detailed — *Street Berlin*'s figures are essentially tall green-black VERTICAL BLOBS with masklike triangles for faces. Insert in the fallback (around line 121, before lit-window block):

```glsl
// Elongated Kirchner street figures — tall vertical blobs with
// triangular masklike heads, pink-skinned faces, green-black coats.
for (int f = 0; f < 5; f++) {
    float ff = float(f);
    vec2 fc = vec2(0.18 + ff * 0.16
                  + 0.02 * sin(TIME * 0.3 + ff),
                  0.55);
    vec2 fd = sUV - fc;
    // Coat — tall ellipse
    float coat = length(fd * vec2(8.0, 1.6)) - 0.18;
    if (coat < 0.0) {
        fresh = mix(fresh, vec3(0.18, 0.32, 0.12), 0.95);
    }
    // Face — pink triangle/circle on top
    vec2 hd = fd - vec2(0.0, -0.18);
    float face = length(hd * vec2(8.0, 4.0)) - 0.06;
    if (face < 0.0) {
        fresh = mix(fresh, vec3(0.95, 0.68, 0.55), 0.92);
    }
}
```

~17 lines. Adds the missing figural iconography. After this, even fresh-out-of-box (no input bound) the shader reads as Kirchner *Street*.

---

## fauvism_matisse.fs

**Visual signature target (top works):** Matisse *Open Window Collioure* (1905) · *The Dance I* (1909) · Derain *Charing Cross Bridge* (1906).

**Current output reads as:** living pigment buffer with curl advection and Fauvist palette drops. Genre-correct. But Matisse and Derain's Fauvist works share one specific compositional rule: LARGE FLAT COLOUR REGIONS (the pink wall, the green sea, the magenta water), not scattered small drops. The shader's drop-deposition gives a CONFETTI feel rather than the BLOCK-OF-FLAT-COLOUR Fauvism.

**Specific gaps vs canon:**
- Palette: ✅ FAUVE[6] is exactly the Matisse/Derain unmixed primaries.
- Composition: ❌ small drops at gridN=16 give pointillist confetti, not Matisse blocks. *The Open Window* divides into ~4 large coloured planes (pink wall, green shutters, magenta sea, blue sky); *The Dance* into 3 (vermilion figures, green ground, blue sky).
- Iconography: ⚠️ snap-to-nearest-Fauvist-primary (line 150) is right. But complementary edge boost is subtle.
- Scale/density: ❌ drops are TOO SMALL for Matisse. Default `dropSize=0.028`. Should be 0.12+ for the BLOCK feel.

**Single highest-leverage fix:** raise default `dropSize` from 0.028 → 0.085 and lower default `dropRate` from 0.55 → 0.20. Bigger drops, sparser, gives flat blocks rather than confetti. Plus increase the `paintFade` from 0.988 → 0.995 so the blocks PERSIST and drown the canvas in solid colour rather than dissolve into noise. This is a parameter-default change of THREE values — matches the rules — but is the highest-leverage move.

Plus add a hard horizontal divider on the canvas (the horizon line that anchors *Open Window* and *The Dance*):

```glsl
// Canvas-wide horizon line in the persistent buffer — Matisse's flat
// pictorial divisions. Snap pigment colour to one zone above/below.
if (abs(uv.y - 0.55) < 0.003) {
    deposit = mix(deposit, vec3(0.10, 0.07, 0.06), 0.4);
}
```

~5 lines. The horizon plus large flat blocks reads as Matisse rather than Pollock-coloured.

---

## futurism_boccioni.fs

**Visual signature target (top works):** Boccioni *Dynamism of a Cyclist* (1913) · Balla *Dynamism of a Dog on a Leash* (1912) · Boccioni *Unique Forms of Continuity in Space* (1913).

**Current output reads as:** correct Futurism — frame-feedback motion blur, force lines from a wandering origin, divisionist dabs. But the trail buffer DOMINATES so the visual is mostly soft streaks; Balla's *Dog on a Leash* has DISCRETE PHANTOM COPIES (~20 distinct leg positions) not a smear. Boccioni's *Cyclist* is FACETED, not blurred.

**Specific gaps vs canon:**
- Palette: ⚠️ procedural fallback uses earthy red/sky. OK for Boccioni *City Rises*; Balla's *Speed+Sound* is more cobalt+pink.
- Composition: ⚠️ the persistent trail compounds, which is technically right, but at `trailPersistence=0.96` it smears into an indistinct blur. Balla's chronophotography is DISCRETE — copies separated by gaps, not blended.
- Iconography: ⚠️ phantom copies exist (line 96–110) but they're additive into `newC` — they BLEND together. They should remain DISCRETE.
- Scale/density: ✅ fine.

**Single highest-leverage fix:** make phantom copies discrete instead of blended. Replace the `newC += c * w; wsum += w;` accumulation with `newC = max(newC, c)` so each phantom remains visible:

```glsl
// Each phantom copy is a DISCRETE position (Balla chronophotography)
// not a soft blend — use max-blend so all 20 dog legs are visible.
vec3 newC = vec3(0.0);
int PC = int(clamp(phantomCount, 1.0, 10.0));
for (int i = 0; i < 10; i++) {
    if (i >= PC) break;
    float fi = float(i);
    vec2 off = vp * (fi - float(PC) * 0.5)
             * phantomSpread * 0.18;
    vec2 sUV = uv - off;
    vec3 c = (IMG_SIZE_inputTex.x > 0.0)
           ? texture(inputTex, sUV).rgb
           : fallbackBody(sUV, vel);
    // Per-phantom darkening — closer to the head is brighter.
    float w = 1.0 - fi / float(PC) * 0.6;
    newC = max(newC, c * w);
}
// No wsum normalisation — each phantom contributes its full discrete colour.
```

~12 lines. After this, motion reads as Balla's *Dog on a Leash* discretely-copied legs rather than smeared blur. Bonus: also drop default `trailPersistence` from 0.96 → 0.88 so trails fade FASTER and discrete phantoms are less likely to be overwritten by smear.

---

## glitch_datamosh.fs

**Visual signature target (top works):** Takeshi Murata *Monster Movie* (2005) · *Pink Dot* (2007) · Rosa Menkman *The Collapse of PAL* (2010).

**Current output reads as:** correct datamosh genre — buffer pulled along motion vectors, RGB shift, DCT block corruption, burst rainbow garbage. Strong technical match to Menkman's "broken codec" output.

**Specific gaps vs canon:**
- Palette: ⚠️ on procedural fallback, the stripe/ring composite has full-saturation RGB which is RIGHT for catastrophic decoder failure but doesn't capture Murata's *Pink Dot* signature: a single pulsing pink form on dark ground. Pink Dot is the most-reproduced datamosh image after Monster Movie itself.
- Composition: ⚠️ catastrophic-failure mode is right. But Monster Movie features RECOGNISABLE FACES dissolving — the failure mode is PARASITIC on recognisable input. With no input bound, the procedural fallback loses that read.
- Iconography: ⚠️ no Pink Dot. The most iconic glitch-art image isn't accessible.
- Scale/density: ✅ fine.

**Single highest-leverage fix:** add a procedural Pink Dot fallback option that pulses through the moshBuf — a single biomorphic pink blob centred-ish, smearing along the mosh direction, on dark ground. Replace the existing procedural fallback inner block (line 86-95):

```glsl
// Pink Dot fallback (Murata 2007) — single pulsing pink biomorphic
// blob on dark ground, smeared by the mosh direction itself.
vec2 dC = vec2(0.5, 0.5)
        + 0.10 * vec2(sin(TIME * 0.4), cos(TIME * 0.32));
vec2 dD = uv - dC;
float dR = 0.18 + 0.05 * sin(TIME * 0.7);
// Biomorphic blob — distorted ellipse with internal noise
float blob = length(dD * vec2(1.4, 1.0))
           - dR * (1.0 + 0.15 * sin(atan(dD.y, dD.x) * 3.0 + TIME));
float blobMask = smoothstep(0.02, -0.02, blob);
vec3 dark = vec3(0.04, 0.02, 0.06);
vec3 pink = vec3(0.95, 0.42, 0.78);
fresh = mix(dark, pink, blobMask);
```

~12 lines. Adds Pink Dot iconography. The mosh direction will then SMEAR the dot on every frame which is the actual Murata effect.

---

## memphis_primitives.fs

**Visual signature target (top works):** Sottsass *Carlton Bookcase* (1981, Memphis flag) · du Pasquier *Royal Sofa* (1983) · Memphis Group postcard imagery.

**Current output reads as:** correct Memphis Group genre — flat primary palette, shape grid, polka dots, squiggles. Strong genre fidelity. Memphis Group is mostly fashion+furniture so there's less single-work canon than fine-art movements; the shader's "Memphis as 7-shape grid" is accurate to the patterns.

**Specific gaps vs canon:**
- Palette: ⚠️ exact Sottsass red, yellow, blue, black, paper. ✅ but missing two of the canonical Memphis additions: TURQUOISE/AQUA and HOT PINK. Real Memphis prints lean heavily on those two.
- Composition: ✅ grid layout is right.
- Iconography: ⚠️ has 7 primitives (circle, square, triangle, checker-circle, stripe-square, squiggle, polka). Missing the canonical TERRAZZO speckled fill and the BLACK ZIGZAG that appears across half of Memphis fabric prints.
- Scale/density: ✅ fine.

**Single highest-leverage fix:** add turquoise and hot-pink to the palette and add a zigzag primitive. In `palette()`:

```glsl
vec3 palette(float h) {
    int i = int(floor(h * 7.0));
    if (i == 0) return vec3(0.902, 0.224, 0.275);   // memphis red
    if (i == 1) return vec3(0.957, 0.827, 0.369);   // memphis yellow
    if (i == 2) return vec3(0.227, 0.525, 1.000);   // memphis blue
    if (i == 3) return vec3(0.078, 0.078, 0.094);   // black
    if (i == 4) return vec3(0.20, 0.85, 0.85);      // turquoise
    if (i == 5) return vec3(0.95, 0.30, 0.65);      // hot pink
    return vec3(0.96, 0.94, 0.88);                  // off-white (paper)
}
```

And add a zigzag primitive case:

```glsl
if (kind == 7) {
    float zz = abs(fract(p.x * 4.0) - 0.5) * 0.4 - 0.1;
    float d = abs(p.y - zz) - 0.04;
    return mix(bg, c, smoothstep(0.0, 0.02, -d));
}
```

Bump prim selection from `int(seed * 7.0)` to `int(seed * 8.0)`. ~10 lines.

---

## minimalism_stella.fs

**Visual signature target (top works):** Stella *Hyena Stomp* (1962) · *Marrakech* (1964) · *Die Fahne hoch!* (1959).

**Current output reads as:** concentric ring marching with palette presets. Strong genre + passes "Hyena Stomp" specifically when palette=0. **This is one of the closest-to-canon shaders** — defaults already hit *Hyena Stomp*.

**Specific gaps vs canon:**
- Palette: ✅ four presets cover the main Stella periods.
- Composition: ✅ concentric Chebyshev = square rings = literal Stella geometry.
- Iconography: ⚠️ canvas gap = `vec3(0.94, 0.91, 0.83)` raw canvas is right. But the *Black Paintings* (preset=1) miss something — Stella's actual *Die Fahne hoch!* has the THINNEST possible canvas-coloured pinstripes, only 2-3 pixels wide between black bands. The shader's `gapWidth=0.014` at default is too thick for that read.
- Scale/density: ✅ fine.

**Single highest-leverage fix:** when palette preset = 1 (Black Paintings), force a thinner gap automatically (or add a per-preset `gapWidth` override):

```glsl
// Stella's Black Paintings have hairline canvas-thread gaps; force
// thinner gap for that preset regardless of user setting.
float effectiveGap = gapWidth;
if (palettePreset == 1) effectiveGap = min(effectiveGap, 0.006);
// then use effectiveGap in the smoothstep below
float gap = smoothstep(0.5 - effectiveGap, 0.5, fract(bf));
```

~4 lines. Plus consider raising default `bandCount` from 11 → 14 for Hyena Stomp's denser ring count. Already very close — these are polish fixes.

---

## opart_riley_waves.fs

**Visual signature target (top works):** Bridget Riley *Current* (1964) · *Cataract 3* (1967) · *Fall* (1963).

**Current output reads as:** dense black-and-white parallel waves bending under sin warp. Passes Riley genre. *Current* (1964) is the canonical match — pure black-and-white wavy lines.

**Specific gaps vs canon:**
- Palette: ⚠️ default is pure black-and-white which is correct for *Current* / *Fall*. *Cataract 3* uses red+turquoise complementaries. Currently the shader has an `accentColor` that defaults to red — fine.
- Composition: ✅ frequency 60, warp 0.12 is correct.
- Iconography: ⚠️ Riley's *Movement in Squares* and *Blaze 1* aren't reachable — only the wave variant.
- Scale/density: ✅ default `freq=60` gives near-Nyquist optical vibration.

**Single highest-leverage fix:** add a `period` mode toggle that lets the user switch between WAVE (current) and CONCENTRIC RAYS (Blaze 1). 60-line cap. Add at the end of `main()`, before the final mix:

```glsl
// (Optional addition: a complementary-clash COLOUR mode — Cataract 3.)
// When accentEvery is low, every ~5th band becomes the accent; sub the
// stripe colour to RED on accent and TURQUOISE on white for that throb.
if (accentEvery <= 4.5) {
    vec3 turq = vec3(0.10, 0.85, 0.85);
    col = mix(col, turq, (1.0 - bw) * step(0.5, fract(idx / accentEvery)));
}
```

Or more simply, expose a `palette` long input with values [0=BlackWhite, 1=RedTurquoise, 2=BlazeRays]. ~15 lines. The current shader is already strong; the missing canon is the COLOURED-RILEY (Cataract 3) variant.

---

## opart_vasarely.fs

**Visual signature target (top works):** Vasarely *Vega-Nor* (1969) · *Vega 200* (1968) · *Tlinko* (1955).

**Current output reads as:** correct Vega — concentric rings warped by spherical bulge with two-tone hue rotation. Strong genre + specific-work fidelity.

**Specific gaps vs canon:**
- Palette: ⚠️ default `hueA=0.85, hueB=0.50` (magenta + cyan). Vega-Nor canonical is more saturated emerald+yellow+ochre; Vega 200 is teal+orange. Currently it's reading more like generic op-art.
- Composition: ✅ spherical bulge is the literal Vega operation.
- Iconography: ⚠️ Vasarely's actual Vega series uses POLYCHROME segments — not just two-tone, but each ring assigned to a different hue from a 4-6-colour palette. Currently it's two-tone alternating.
- Scale/density: ✅ fine.

**Single highest-leverage fix:** swap the two-tone band mix for a 4-colour palette indexed by the band number. Replace:

```glsl
vec3 col = mix(colA, colB, band);
```

with:

```glsl
// Polychrome Vega — index each ring by floor(phase / pi) and pick from
// a 4-colour palette so adjacent rings cycle through all four hues.
int ringIdx = int(floor(phase / 3.14159));
vec3 vegaP[4] = vec3[4](
    hsv2rgb(vec3(fract(0.55 + hueShift), saturation, 0.95)),
    hsv2rgb(vec3(fract(0.10 + hueShift), saturation, 0.95)),
    hsv2rgb(vec3(fract(0.85 + hueShift), saturation, 0.95)),
    hsv2rgb(vec3(fract(0.30 + hueShift), saturation, 0.95))
);
vec3 col = vegaP[ringIdx & 3];
// Smooth band edge — soften with adjacent ring colour at boundary
vec3 nextC = vegaP[(ringIdx + 1) & 3];
col = mix(col, nextC, band);
```

~15 lines. Reads as Vega-Nor specifically rather than "two-tone op art".

---

## popart_lichtenstein.fs

**Visual signature target (top works):** Lichtenstein *Whaam!* (1963) · *Drowning Girl* (1963) · *M-Maybe* (1965).

**Current output reads as:** correct Lichtenstein genre — Ben-Day dots, comic primaries, hard outlines, speech bubble with "WHAAM"/"POW"/"ZOK"/"BAM". This is one of the strongest shaders in the set; the speech bubble is exactly the Lichtenstein signifier.

**Specific gaps vs canon:**
- Palette: ✅ LIK_INK / RED / YELLOW / BLUE / FLESH — comic-CMYK exact.
- Composition: ⚠️ on no-input fallback, the procedural test pattern (line 116-126) is a UV gradient with cell tints — reads as a generic mapping image, not a Lichtenstein subject (face/explosion/airplane). Without a video source, the dots and outlines have nothing recognisable to apply to.
- Iconography: ✅ speech bubble + WHAAM word is the icon.
- Scale/density: ⚠️ default `dotDensity=140` is fine but the bubble at default size 0.14 placed at (0.72, 0.78) is small; *Whaam!*'s WHAAM occupies ~30% of the canvas.

**Single highest-leverage fix:** on procedural fallback, replace the UV-grid pattern with a stylised comic-explosion silhouette — a star-burst SDF in red/yellow/blue:

```glsl
// Comic explosion fallback — radial starburst silhouette in red/yellow,
// the WHAAM core graphic. Replaces the generic UV grid.
vec2 cuv = uv - vec2(0.5);
cuv.x *= aspect;
float ang = atan(cuv.y, cuv.x);
float r = length(cuv);
// Spiky starburst — radius modulated by sin(ang * 8) for 8-point burst
float burstR = 0.30 + 0.08 * sin(ang * 8.0 + TIME * 0.3);
float burst = step(r, burstR);
float burstInner = step(r, burstR * 0.6);
raw = mix(vec3(0.97, 0.95, 0.92), LIK_YELLOW, burst);
raw = mix(raw, LIK_RED, burstInner);
// Sky background outside the burst
if (burst < 0.5) raw = mix(LIK_BLUE * 0.7, raw, smoothstep(0.7, 0.3, r));
```

~14 lines. After this even with no video bound the shader reads as *Whaam!* specifically.

---

## surrealism_magritte.fs

**Visual signature target (top works):** Magritte *The Son of Man* (1964) · *Empire of Light* (1953) · *Golconda* (1953).

**Current output reads as:** photoreal sky with cloud, floating SDF object (apple/bowler/rock/pipe), long shadow, Golconda ghost duplicates. Very strong overall — passes specific-work test for Golconda when ghosts are visible. But *The Son of Man* — the most-reproduced Magritte — has a SPECIFIC iconography: a bowler-hat MAN with a green apple FLOATING IN FRONT OF HIS FACE. The shader has bowler OR apple separately, never together.

**Specific gaps vs canon:**
- Palette: ✅ cobalt sky / cream horizon / dim ground are correct.
- Composition: ✅ horizon, hovering object, long shadow are all there.
- Iconography: ⚠️ no SON OF MAN combined-figure mode — bowler man with apple over face. Currently you can pick bowler XOR apple; you can't get the combination. Yet *Son of Man* is more famous than either standalone.
- Scale/density: ✅ fine.

**Single highest-leverage fix:** add a 5th `objectChoice` value: "Son of Man" (combined bowler-hat figure with green apple over face). Add to the LABELS and a new SDF case:

```glsl
float sdSonOfMan(vec2 p, float s) {
    // Tall figure body — rectangle from below horizon up
    float body = max(abs(p.x) - s * 0.45,
                     abs(p.y + s * 0.5) - s * 0.85);
    // Bowler hat on top
    float crown = length(vec2(p.x, max(p.y - s * 0.55, 0.0))) - s * 0.4;
    float brim = max(abs(p.x) - s * 0.55,
                     abs(p.y - s * 0.50) - s * 0.05);
    float hat = min(crown, brim);
    return min(body, hat);
}
// And then a green-apple float overlay (NOT part of the SDF — it's a
// composite layer) drawn ON TOP of the figure in the main fill section.
```

In the main fill: after the body fill, draw a green apple at `objCtr + vec2(0, -objectSize*0.1)` of size `objectSize * 0.4`, in vec3(0.45, 0.78, 0.20). ~25 lines. The Son of Man combination is the single most-recognisable Magritte; adding it transforms this from "Magritte-genre" to "Magritte-iconic".

---

## vaporwave_floral_shoppe.fs

**Visual signature target (top works):** Macintosh Plus *Floral Shoppe* album cover (2011) · 2814 *Birth of a New Day* (2015) · James Ferraro *Far Side Virtual* (2011).

**Current output reads as:** correct vaporwave genre — pink/teal sky, perspective grid, magenta sun with bars, Greek bust silhouette, scanlines, katakana, CRT bloom. Strong specific-work fidelity to *Floral Shoppe* — it's basically a render of that album cover.

**Specific gaps vs canon:**
- Palette: ✅ pink/teal/magenta. Correct.
- Composition: ✅ horizon, sun, grid, bust, scanlines — every element of the *Floral Shoppe* cover is here.
- Iconography: ⚠️ the bust SDF (line 50) is approximated head+neck+shoulders+profile but doesn't really read as a marble bust. The bust looks like a generic blob silhouette.
- Scale/density: ⚠️ default `bustSize=0.26` is fine. Sun bars 6 is right.

**Single highest-leverage fix:** improve the bust SDF profile — currently it reads as a vague blob. Add more detail to the silhouette so it recognisably suggests a head+nose+chin profile. Replace `bustSDF`:

```glsl
float bustSDF(vec2 p, float s) {
    // Head — circle slightly tilted
    float head = length(p - vec2(-s * 0.05, s * 0.18)) - s * 0.30;
    // NOSE bump — small ellipse on the right of the face
    float nose = length((p - vec2(s * 0.22, s * 0.22)) * vec2(2.5, 1.0))
               - s * 0.06;
    head = min(head, nose);
    // CHIN cleft — subtract a small notch below the head
    float chin = length((p - vec2(s * 0.14, s * 0.0)) * vec2(2.0, 1.5))
               - s * 0.08;
    head = max(head, -chin);
    // Neck
    float neck = max(abs(p.x) - s * 0.13,
                     abs(p.y + s * 0.08) - s * 0.18);
    // Shoulders rounded rect
    float sh = length(max(vec2(abs(p.x) - s * 0.4,
                                abs(p.y + s * 0.40) - s * 0.10), 0.0)) - s * 0.16;
    float body = min(min(head, neck), sh);
    // Profile cut for Greek-bust profile
    float profile = p.x - p.y * 0.18 - s * 0.42;
    return max(body, profile);
}
```

~20 lines. Adds a NOSE — the single feature that makes a silhouette read as a face profile rather than a blob. After this the bust looks like David's head specifically.

---

## Top-5 priority fixes (across all 18)

Ranked by leverage — likely to push the user from "neat but generic" to "wow that's clearly [movement]":

1. **`dali_melt.fs` — add a melting clock SDF to the procedural fallback.** Currently the only Surrealism shader without iconography; readable as generic vertical melt. A crude clock with sagging numerals would add the single most recognisable Dalí signifier. (`daliDesert` function, lines 26–31, ~30 lines.)

2. **`art_nouveau_mucha.fs` — add the Byzantine halo arc to the Mucha field.** Mucha posters all share this central decorative arc; without it the shader reads as "art-nouveau wallpaper" rather than "Mucha poster". (`fieldColor` style==0 branch, lines 44–50, ~10 lines.)

3. **`destijl_mondrian.fs` — add 3-4 static rectangular cell fills.** The shader is currently Boogie Woogie-only; adding cells unlocks the canonical *Composition with Red, Blue, and Yellow* read everyone knows. (After lane loops around line 150, ~17 lines.)

4. **`abex_pollock.fs` — interpolate dripper segments instead of stamping points.** Default density currently reads as dotted trails not lattice. Drawing line-segments between consecutive dripper positions transforms output from bead-chains to continuous skeins. (`PASSINDEX==0` deposit loop, lines 128–146, ~12 lines + raise default density.)

5. **`surrealism_magritte.fs` — add a "Son of Man" combined-object choice.** Currently bowler XOR apple but never together; *Son of Man* is the single most-reproduced Magritte and unreachable from any setting. (New `sdSonOfMan` + apple overlay in fill block, ~25 lines.)

Honourable mention: **`cubism_picasso.fs`** letter-fragment SDFs (J/O/U/B/A/L) would fix the "noise that looks like letters" problem — but the LUT and plane composition are already so close to canon that the letter fix is polish, not transformation.

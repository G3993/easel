# Shader Critique — Round 7: Closing the Final Eight

Rounds 5 and 6 landed 10 specific-work fixes covering Mucha, Mondrian,
Pollock, Magritte, Dalí, Picasso, Kirchner, Lichtenstein, Matisse, and
Vasarely. Round 7 finishes the job: the eight remaining shaders, each
with a single-feature push from "movement-genre" to "specific-canonical-
work". Smaller fixes, mostly polish — most of these are already very
close.

---

## constructivism_lissitzky.fs

**Specific work target:** El Lissitzky *Beat the Whites with the Red Wedge* (1919).

**Verdict before fix:** wedge-vs-circle composition is canonically correct
and exact for the poster. But the wedge at default `width=0.18` vs
`length=0.55` reads as a thin BLADE rather than the equilateral WEDGE
that punches the white circle. Cyrillic glyph clusters at default
`glyphIntensity=0.6` are three small clusters at corner positions —
they read as decorative scatter, not the BANNER-SCALE typography that
defines a Constructivist poster. Pass 5's banner-typography suggestion
did NOT land yet.

**Fix (apply this directly):** two changes.

(1) Widen the wedge — line 124 currently calls
`wedge(uv, apex, -axis, 0.55, 0.18)`. Change to
`wedge(uv, apex, -axis, 0.55, 0.32)` (width 0.18 → 0.32). One token.

(2) Add a fourth banner-scale glyph cluster spanning the upper edge.
Insert inside `glyphField` (after the existing for-loop, before
`return total;` at line 90):

```glsl
// Banner-scale typography across top of poster (Rodchenko grammar).
{
    vec2 origin = vec2(0.10, 0.05);
    origin.x += sin(TIME * 0.15) * 0.01;
    vec2 local = (uv - origin) * vec2(7.0, 3.5);
    if (!(any(lessThan(local, vec2(0.0))) ||
          local.x > 5.0 || local.y > 1.0)) {
        vec2 ci = floor(local);
        float h  = hash21(ci);
        float vert = step(h, 0.55) * step(0.18, fract(local.x))
                   * step(fract(local.x), 0.55);
        float bar  = step(0.55, h) * step(h, 0.85)
                   * step(0.40, fract(local.y))
                   * step(fract(local.y), 0.62);
        total = max(total, max(vert, bar));
    }
}
```

~15 lines. After this the poster reads as the canonical Lissitzky.

---

## abex_rothko.fs

**Specific work target:** Rothko *Orange Red Yellow* (1961) / Seagram Murals chapel feel.

**Verdict before fix:** palette is exactly Orange Red Yellow at default
and the band layout is right. Edges feather at 0.12 which is decent but
still slightly crisper than the chapel paintings. No internal inner-glow
pass — every band is a flat smoothstep fill, missing the chapel-painting
"radiance from within" that distinguishes Rothko from "stacked
rectangles". Pass 5's inner-glow suggestion did NOT land yet.

**Fix (apply this directly):** two changes.

(1) Bump default `feather` from 0.12 → 0.16 — the manifest at line 6.

(2) Add a per-band centre brightness glow inside `bandShape` (before
`return yMask * xMask;` at line 56):

```glsl
// Painterly inner glow — band centres slightly brighter than edges
// so each rectangle reads as having internal radiance, not flat fill.
float bandCenterY = (yLo + yHi) * 0.5;
float dyFromCenter = abs(uv.y - bandCenterY) / max((yHi - yLo) * 0.5, 1e-4);
yMask *= 1.0 + 0.08 * (1.0 - dyFromCenter * dyFromCenter);
```

~5 lines. Gives each Rothko slab the chapel-painting interior radiance.

---

## bauhaus_kandinsky.fs

**Specific work target:** Kandinsky *Composition VIII* (1923).

**Verdict before fix:** strict primary-shape pairing on warm white with
halo glow is correct for *Several Circles* / Composition VIII. But every
shape currently fills as solid colour. *Composition VIII*'s signature is
CHEQUERBOARD-FILLED interiors on a subset of shapes — without that the
shader reads as *Several Circles* only. Pass 5's chequerboard suggestion
did NOT land yet.

**Fix (apply this directly):** modify the closer-wins SDF block at
lines 149–152. Replace:

```glsl
if (sd < bestSD) {
    bestSD = sd;
    bestCol = (t == 0) ? yellowC : (t == 1) ? redC : blueC;
}
```

with:

```glsl
if (sd < bestSD) {
    bestSD = sd;
    // Every 4th shape gets a chequerboard interior — Composition VIII
    // signature pattern.
    vec2 chk = floor(lp / max(sz * 0.32, 1e-4));
    bool chkOn  = (mod(chk.x + chk.y, 2.0) < 1.0);
    bool useChk = (mod(fi, 4.0) >= 3.0);
    vec3 baseC  = (t == 0) ? yellowC : (t == 1) ? redC : blueC;
    vec3 altC   = vec3(0.05, 0.05, 0.06);
    bestCol = useChk ? (chkOn ? baseC : altC) : baseC;
}
```

~10 lines. ~25% of shapes now read as chequerboard-filled, pulling the
canvas toward Composition VIII directly without losing Several Circles.

---

## futurism_boccioni.fs

**Specific work target:** Boccioni *Dynamism of a Cyclist* (1913) — fractured cobalt-and-orange wedges, NOT a soft motion smear.

**Verdict before fix:** trail-feedback + force rays + divisionist dabs
read as generic "motion blur with rays". The trail buffer dominates and
phantom copies blend additively into mush. *Dynamism of a Cyclist*'s
defining feature is FACETED OVERLAPPING WEDGES — discrete planar
fractures, not smooth blur. The shader's PASS 0 phantom accumulator
(lines 95–111) blends N copies into a single soft averaged image.

**Fix (apply this directly):** two changes.

(1) Replace the additive phantom accumulator at lines 95–111 with
max-blend so phantoms remain discrete:

```glsl
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
    // Per-phantom darkening so closer-to-head copies are brighter.
    float w = 1.0 - fi / float(PC) * 0.6;
    newC = max(newC, c * w);
}
// No wsum normalisation — discrete copies, not smooth blend.
```

(2) In the procedural `fallbackBody` at lines 44–55, add 6–8 hard wedge
shards radiating from the moving centre so the no-input case reads as
faceted-cyclist rather than soft blob:

```glsl
vec3 fallbackBody(vec2 uv, vec2 vel) {
    vec2 c = vec2(0.5) + vel * sin(TIME * 0.7) * 6.0;
    vec2 d = uv - c;
    float ang = atan(d.y, d.x);
    float r   = length(d);
    // 7-bladed faceted wedge silhouette — Boccioni Cyclist fractured
    // forms in cobalt + orange, not a smooth blob.
    float blade = pow(abs(sin(ang * 3.5 + TIME * 0.4)), 0.6);
    float bladeR = 0.18 + 0.10 * blade;
    float wedge  = smoothstep(bladeR + 0.02, bladeR - 0.02, r);
    vec3 sky    = vec3(0.10, 0.14, 0.30);
    vec3 cobalt = vec3(0.08, 0.30, 0.78);
    vec3 orange = vec3(0.98, 0.45, 0.10);
    vec3 body   = mix(cobalt, orange, blade);
    return mix(sky, body, wedge);
}
```

(3) Drop default `trailPersistence` from 0.96 → 0.88 (line 5) so faceted
wedges aren't immediately re-smeared.

~22 lines + two default tweaks. After this the shader reads as
Boccioni's fractured cyclist instead of generic motion-blur.

---

## dada_hoch.fs

**Specific work target:** Höch *Cut with the Kitchen Knife* (1919) — face cutouts and machine parts, not generic torn rectangles.

**Verdict before fix:** strips, halftone, RGB shift, ink stamps, and a
letter-stutter band sell the photomontage feel. But every patch is a
RECTANGLE (line 75 — `if (abs(q.x) > s || abs(q.y) > s * aR) continue;`).
Höch's signature is FACE CUTOUTS and GEAR/COG mechanical parts in
silhouette — circular/oval cutouts dominate her composition. Currently
the shader reads as "torn newsprint scatter" not "Höch face-and-machine
collage". Pass 5's DADA word stamp is a smaller fix that doesn't
address this root issue.

**Fix (apply this directly):** in the strip loop (line 60), make ~50%
of strips OVAL-CUT (face silhouettes) and ~25% GEAR-CUT (machine
silhouettes) instead of all rectangular. Replace the rectangle test at
line 75 + edge mask at lines 107–112:

```glsl
// Strip kind: 0=face oval, 1=gear cog, 2=newsprint rectangle.
int kind = int(hash11(fi * 27.3) * 3.0);
float inside;
float edgeMask;
if (kind == 0) {
    // Face oval — Höch's cut-out heads.
    vec2 qN = q / vec2(s, s * aR);
    float r = length(qN * vec2(0.85, 1.20));   // slightly tall oval
    if (r > 1.0) continue;
    inside = 1.0;
    edgeMask = smoothstep(1.0, 0.85, r);
} else if (kind == 1) {
    // Gear/cog silhouette — 12-toothed circle for machine parts.
    vec2 qN = q / vec2(s, s * aR);
    float ang = atan(qN.y, qN.x);
    float gearR = 0.85 + 0.10 * step(0.5, fract(ang * 12.0 / 6.2832));
    float r = length(qN);
    if (r > gearR) continue;
    inside = 1.0;
    edgeMask = smoothstep(gearR, gearR - 0.10, r);
} else {
    // Original rectangle path with torn-paper jag.
    if (abs(q.x) > s || abs(q.y) > s * aR) continue;
    float jag = hash21(q * 40.0 + bt) * 0.04;
    float edgeX = (s - abs(q.x)) / s;
    float edgeY = (s * aR - abs(q.y)) / (s * aR);
    edgeMask = smoothstep(0.0, 0.06, min(edgeX, edgeY) - jag);
}

prev = mix(prev, patch, edgeMask);
```

~25 lines (replaces ~10). Now Höch's actual collage grammar — face
ovals + cog silhouettes + occasional newsprint strips — is on screen.
Pass 5's DADA word-stamp can still ship later as a polish addition.

---

## vaporwave_floral_shoppe.fs

**Specific work target:** Macintosh Plus *Floral Shoppe* album cover (2011) — but the album is literally titled *Floral Shoppe* and there are NO ROSES in the shader.

**Verdict before fix:** sky, sun, grid, bust, scanlines, katakana, CRT
bloom — every standard vaporwave element is here and looks correct. Bust
SDF reads as generic blob silhouette. But the canonical
*Floral Shoppe* cover features ROSES on the floor in front of the bust
(the album's namesake). Pass 5's nose-detail fix on the bust SDF is
small polish that misses the bigger absence: no flowers anywhere despite
the album title.

**Fix (apply this directly):** add a roses pass below the horizon, in
front of the grid. Insert after the bust block (after line 159, before
the chromatic aberration block):

```glsl
// Roses on the floor — the *Floral Shoppe* namesake. Three pink-magenta
// rose silhouettes scattered across the foreground floor, each a
// 5-petal rosette SDF over a small green stem dot.
if (uv.y < horizonY - 0.04) {
    for (int rIdx = 0; rIdx < 3; rIdx++) {
        float fr = float(rIdx);
        vec2 rC = vec2(0.18 + fr * 0.32
                      + 0.01 * sin(TIME * 0.3 + fr),
                       horizonY - 0.08 - fr * 0.04);
        vec2 rD = uv - rC; rD.x *= aspect;
        // Scale roses larger when closer to viewer (lower on screen).
        float rS = 0.04 + 0.04 * (horizonY - uv.y);
        float ang = atan(rD.y, rD.x);
        // 5-petal rosette — radius modulated by cos(5*angle).
        float petalR = rS * (0.7 + 0.3 * abs(cos(ang * 2.5 + TIME * 0.2)));
        float rL = length(rD);
        if (rL < petalR) {
            // Pink-magenta gradient from centre out.
            vec3 roseInner = vec3(1.00, 0.55, 0.78);
            vec3 roseOuter = vec3(0.82, 0.18, 0.52);
            vec3 roseC = mix(roseInner, roseOuter, rL / petalR);
            col = mix(col, roseC, smoothstep(petalR, petalR - 0.005, rL));
        }
        // Small green stem dot below.
        vec2 sD = rD - vec2(0.0, -rS * 1.2);
        if (length(sD) < rS * 0.25) {
            col = mix(col, vec3(0.20, 0.55, 0.30), 0.85);
        }
    }
}
```

~28 lines. After this the shader matches the album cover NAME, not just
its sky-and-bust. The bust nose-detail polish from pass 5 is still
worth doing later but this is the larger transformation.

---

## glitch_datamosh.fs

**Specific work target:** Takeshi Murata *Pink Dot* (2007) — single pulsing pink biomorphic blob smeared by motion vectors, the canonical glitch-art image.

**Verdict before fix:** all three glitch grammars (mosh-direction
feedback, RGB split, DCT-block corruption, burst rainbow garbage) work
and read as catastrophic codec failure. But the no-input fallback at
lines 87–95 is "scrolling stripes + ring + flickering blocks" — generic
glitch test pattern, not Murata-specific. *Pink Dot* is the most
reproduced glitch-art image after Monster Movie itself and is currently
unreachable.

**Fix (apply this directly):** the choice in the brief is "Pink Dot
fallback OR scrolling QR-code-like garbage block". Pink Dot wins —
QR-garbage already exists in the burst block (lines 137–142) and adding
more wouldn't be transformative. Replace the procedural fallback at
lines 86–95 (inside the `else` branch of the I-frame stutter and again
in the main fresh path):

```glsl
// Pink Dot fallback (Murata 2007) — single pulsing pink biomorphic
// blob on dark ground. The mosh direction will smear it on every
// frame which IS the canonical Murata effect.
vec2 dC = vec2(0.5, 0.5)
        + 0.10 * vec2(sin(TIME * 0.4), cos(TIME * 0.32));
vec2 dD = uv - dC;
float angle = atan(dD.y, dD.x);
// Biomorphic radius — distorted ellipse with internal noise.
float dR = 0.18 + 0.05 * sin(TIME * 0.7)
                + 0.04 * sin(angle * 5.0 + TIME * 1.3);
float blob = length(dD * vec2(1.4, 1.0)) - dR;
float blobMask = smoothstep(0.02, -0.02, blob);
vec3 dark = vec3(0.04, 0.02, 0.06);
vec3 pink = vec3(0.95, 0.42, 0.78);
fresh = mix(dark, pink, blobMask);
```

~14 lines, replacing ~10. Apply in BOTH places where the no-input
fallback runs (the I-frame reset block at lines 50–62 and the main
`fresh` assignment at lines 81–96 — substitute the entire `else` body
in each). The mosh accumulator will then drag the pink dot along the
mosh direction every frame, which is exactly Murata's process.

---

## ai_latent_drift.fs

**Specific work target:** Refik Anadol *Machine Hallucinations: MoMA* (2022) — wet pigment storm with thousands of micro-glints, NOT a smooth colour wash.

**Verdict before fix:** curl-warped fbm with mauve-teal-orange palette
arc and phantom-feature pulses reads as soft Anadol-genre. Pass 5's two
candidates were (a) Anadol pigment-storm specular sparkle or (b)
Klingemann face-ghost where phantom features briefly form an eye/mouth
shape. The brief asks me to pick the more transformative.

The Klingemann face-ghost is more transformative. The current shader
already has a phantom-pulse pass — adding sparkles enhances the existing
Anadol read. Adding face-features unlocks a SECOND artist (Klingemann)
that's currently inaccessible and gives the latent-drift conceit a
stronger "almost-recognises-something" payoff.

**Fix (apply this directly):** in the phantom-features block at lines
125–132, add a per-frame eye/mouth probe — when the phantom field
exceeds a threshold at a hashed location, draw two small dark "eye"
ovals + one mouth slit that briefly form before dissolving:

```glsl
if (phantomPulse > 0.0) {
    float scaleNow = phantomScale * (0.7 + 0.3 * sin(TIME * 0.10));
    float ph = fbm(P * scaleNow + L2 * 1.7) - 0.5;
    ph *= smoothstep(0.4, 0.0, abs(ph));
    vec3 phCol = latentPalette(t + 0.3, 0.0);
    col += phCol * ph
         * phantomPulse * (0.4 + audioHigh * audioReact * 1.3);

    // Klingemann face-ghost — every ~3s, a phantom face emerges at a
    // hashed centre, holds for ~0.6s, dissolves. Two dark eye dots
    // and a mouth slit, faded by the macro field so they integrate.
    float faceCycle = floor(TIME * 0.33);
    vec2 fcC = vec2(0.30 + 0.40 * hash21(vec2(faceCycle, 1.7)),
                    0.35 + 0.35 * hash21(vec2(faceCycle, 5.3)));
    float faceLife = fract(TIME * 0.33);
    float faceVis  = smoothstep(0.0, 0.15, faceLife)
                   * smoothstep(0.6, 0.45, faceLife);
    vec2 fd = (uv - fcC) * vec2(aspect, 1.0);
    // Two eyes
    float eyeL = length((fd - vec2(-0.04, 0.03)) * vec2(1.0, 1.6)) - 0.018;
    float eyeR = length((fd - vec2( 0.04, 0.03)) * vec2(1.0, 1.6)) - 0.018;
    float eyes = min(eyeL, eyeR);
    // Mouth slit
    float mouth = max(abs(fd.x) - 0.045,
                      abs(fd.y + 0.05) - 0.006);
    float face = min(eyes, mouth);
    float faceMask = smoothstep(0.005, -0.005, face) * faceVis * 0.55;
    col = mix(col, vec3(0.10, 0.08, 0.12), faceMask);
}
```

~22 lines. Phantom faces now emerge from the pigment storm for
~0.6 seconds every 3 seconds at random locations — the Klingemann
*Memories of Passersby* "almost-portrait" payoff. The face fades back
into the macro field rather than persisting.

---

## After round 7

If all 8 fixes above land, the shader set has **18 of 18** shaders
passing the specific-work test:

1. abex_pollock — *Autumn Rhythm* (round 5)
2. abex_rothko — *Orange Red Yellow* + chapel inner-glow (round 7)
3. ai_latent_drift — Anadol pigment storm + Klingemann face-ghost (round 7)
4. art_nouveau_mucha — *Gismonda* halo (round 5)
5. bauhaus_kandinsky — *Composition VIII* via chequerboard fills (round 7)
6. constructivism_lissitzky — *Beat the Whites with the Red Wedge* +
   banner typography + thicker wedge (round 7)
7. cubism_picasso — Kahnweiler letters (round 6)
8. dada_hoch — *Cut with the Kitchen Knife* — face ovals + gear cogs (round 7)
9. dali_melt — *Persistence of Memory* (round 5)
10. destijl_mondrian — *Composition with RYB* cells (round 5)
11. expressionism_kirchner — *Street Berlin* figures (round 6)
12. fauvism_matisse — *Open Window Collioure* blocks (round 6)
13. futurism_boccioni — *Dynamism of a Cyclist* discrete wedges (round 7)
14. glitch_datamosh — Murata *Pink Dot* (round 7)
15. memphis_primitives — Memphis Group genre (canonical-collective)
16. minimalism_stella — *Hyena Stomp* (default)
17. opart_riley_waves — *Current* (default)
18. opart_vasarely — *Vega-Nor* polychrome rings (round 6)
19. popart_lichtenstein — *Whaam!* explosion fallback (round 6)
20. surrealism_magritte — *Son of Man* (round 5)
21. vaporwave_floral_shoppe — Macintosh Plus cover with roses (round 7)

Anything still possibly failing after this round:
- **memphis_primitives** never had a single canonical work to nail —
  Memphis is collective, the shader passes the genre test but there's
  no equivalent of "Whaam!" to reach for. Acceptable.
- **opart_riley_waves** is canonical for *Current* (1964) but
  *Cataract 3*'s red-turquoise palette and *Movement in Squares* aren't
  reachable without a palette mode toggle. Marginal — the default IS
  canonical, alternate works just aren't.

Net: pending the round 7 implementations, the set goes from 10/18 to
18/18 specific-work-passing — the qualitative threshold where users
recognise paintings rather than merely the movements they came from.

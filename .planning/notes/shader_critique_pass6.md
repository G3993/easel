# Shader Critique — Round 6: Verification + Next-Five Iconography Push

Round 5 prescribed five highest-leverage iconography fixes, all of which
landed on disk. Round 6 verifies them and ranks the next five fixes — the
ones that will move the next batch of shaders from "movement-genre" to
"specific-canonical-work".

---

## Round-5 verification

### art_nouveau_mucha.fs — LANDED
At lines 45–58 the style==0 branch now contains a halo arc:
```
vec2 halo  = uv - vec2(0.5, 0.62);
float r    = length(halo);
float arc  = smoothstep(0.36, 0.30, r) - smoothstep(0.30, 0.24, r);
base = mix(base, vec3(0.92, 0.78, 0.36), arc * 0.55);
```
Byzantine medallion ring is in place. Mucha now reads as poster, not
wallpaper. Recommended in pass 5; implemented exactly as sketched.

### destijl_mondrian.fs — LANDED
At lines 119–144 a 4-cell static fill loop is added, using BBW_RED /
BLUE / YELLOW / GREY. Cells drift gently (lines 136–137) so they don't
strobe. Description string was also updated to mention "primary squares".
Lane count default unchanged (still 6) — pass 5 suggested dropping to 4
but that was a side recommendation, not part of the headline fix; the
canonical *Composition with Red Blue and Yellow* read is now reachable.

### abex_pollock.fs — LANDED
Defaults raised: `drippers` 12 → 22 (line 5), `strokeWidth` 0.0035 →
0.006 (line 6). Inside the deposit loop (lines 137–142) the segment
math is in place — pPrev computed at `t - 0.06`, segment-distance via
projection of (uv − pPrev) onto (pNow − pPrev). Per-frame deposits are
now 60-ms ribbons rather than stamps; over ~30 frames this builds the
Autumn-Rhythm lattice. Cleanest pass-5 implementation.

### surrealism_magritte.fs — LANDED
A new `objectChoice = 4` "Son of Man" enum value is exposed (line 5,
DEFAULT 4). `sdSonOfMan` SDF defined at lines 89–119 combining body
rectangle + bowler crown + brim. Branched in main fill at lines 156+
with green-apple overlay tint logic ("Son of Man — apple is green, hat
is black, body is..."). Default object is now Son of Man; the canonical
Magritte is reachable from a fresh viewport.

### dali_melt.fs — LANDED
`sdMeltingClock` SDF added at lines 26–38 with sag-amount-modulated
ellipse + drape tail. `daliDesert` (lines 40–61) now draws ground/sky
with horizon at y = 0.42–0.46, ochre clock fill, hour-mark dots ringing
the body, gold-rim outline. The procedural fallback now reads as
*Persistence of Memory* even with no input bound — biggest single
upgrade in the round.

**All 5 round-5 fixes are present and behave as specified.**

---

## Round-6 priorities

The remaining shaders that fail the work-test most badly. Same standard
as round 5: each fix must move the viewer from "I can tell what
movement this is" to "I can name the painting".

### 1. cubism_picasso.fs — letter fragments don't read as letters

(~30 lines, replace `letterField` ~lines 56–80, also touched in main
near the letter-stamp call.)

The LUT is exact for Kahnweiler 1910 and the planes are right-grammar.
The single failing element is the JOU / MA JOLIE letter scraps —
currently they're hash-driven vertical/horizontal bar noise that reads
as "scribble" not "stencilled letter". Real Picasso analytic paintings
have legible fragments of A, B, J, L, M, O, U, painted in stencil
typeface. This is the iconography Picasso actually drew.

```glsl
// Replace letterField with explicit SDFs for J, O, U, A.
float drawJ(vec2 p) {
    float bar  = step(0.40, p.x) * step(p.x, 0.60) * step(0.20, p.y);
    float hook = step(0.10, p.x) * step(p.x, 0.40)
               * step(0.10, p.y) * step(p.y, 0.30);
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
    float B = step(0.10, p.x) * step(p.x, 0.90)
            * step(0.10, p.y) * step(p.y, 0.30);
    return max(max(L, R), B);
}
float drawA(vec2 p) {
    float L = step(0.10, p.x) * step(p.x, 0.28) * step(0.10, p.y);
    float R = step(0.72, p.x) * step(p.x, 0.90) * step(0.10, p.y);
    float T = step(0.10, p.x) * step(p.x, 0.90) * step(0.85, p.y);
    float M = step(0.20, p.x) * step(p.x, 0.80)
            * step(0.45, p.y) * step(p.y, 0.55);
    return max(max(L, R), max(T, M));
}
float letterField(vec2 uv, float seed) {
    // 4 letter cells at fixed-but-drifting positions.
    float total = 0.0;
    for (int i = 0; i < 4; i++) {
        float fi = float(i);
        vec2 origin = vec2(0.18 + hash11(fi + seed) * 0.6,
                           0.18 + hash11(fi * 3.7 + seed) * 0.6);
        origin += 0.01 * vec2(sin(TIME * 0.2 + fi),
                              cos(TIME * 0.17 + fi));
        vec2 lp = (uv - origin) * vec2(18.0, 14.0);
        if (lp.x < 0.0 || lp.x > 1.0 || lp.y < 0.0 || lp.y > 1.0) continue;
        float ink = (i == 0) ? drawJ(lp)
                  : (i == 1) ? drawO(lp)
                  : (i == 2) ? drawU(lp)
                             : drawA(lp);
        total = max(total, ink);
    }
    return total;
}
```
Highest leverage of any remaining shader because the LUT and plane
composition are already so close to canon — the letter scraps are the
only thing keeping it from "Picasso analytic" recognition.

### 2. expressionism_kirchner.fs — no figures in the procedural fallback

(~17 lines added inside the procedural fallback ~line 122, before the
window block.)

The carve overlay and acid LUT are correct. But *Street, Berlin* (1913)
— Kirchner's defining painting — is a STREET WITH FIGURES: tall feathered
women in green-black coats, masklike pink faces. Without figures the
procedural scene reads as Heckel landscape, not Kirchner street.

```glsl
// Elongated Kirchner street figures — tall green-black coat blobs with
// pink masklike faces, the *Street Berlin* signature.
for (int f = 0; f < 5; f++) {
    float ff = float(f);
    vec2 fc = vec2(0.18 + ff * 0.16
                  + 0.02 * sin(TIME * 0.3 + ff),
                  0.55);
    vec2 fd = sUV - fc;
    float coat = length(fd * vec2(8.0, 1.6)) - 0.18;
    if (coat < 0.0) {
        fresh = mix(fresh, vec3(0.18, 0.32, 0.12), 0.95);
    }
    vec2 hd = fd - vec2(0.0, -0.18);
    float face = length(hd * vec2(8.0, 4.0)) - 0.06;
    if (face < 0.0) {
        fresh = mix(fresh, vec3(0.95, 0.68, 0.55), 0.92);
    }
}
```

Insert before the lit-window block (~line 116). After this the no-input
fallback reads as Berlin street-corner with crowd, not as a deserted
gradient with lamp streaks.

### 3. popart_lichtenstein.fs — fallback is a UV mapping grid, not a comic frame

(~14 lines, replace the mapping-pattern fallback at lines 113–126.)

The Ben-Day dots, comic outlines, and WHAAM speech bubble are the
strongest part of any art-movement shader in the set. But on no-input
fallback the UV gradient with cell tints reads as a generic test image
— the comic effects have nothing recognisable to apply to. *Whaam!*'s
core graphic is a starburst explosion silhouette; even at low fidelity
that's the read.

```glsl
} else {
    // Comic explosion fallback — radial starburst silhouette in
    // red/yellow on cobalt sky, the *Whaam!* core graphic.
    vec2 cuv = (uv - vec2(0.5)) * vec2(aspect, 1.0);
    float ang = atan(cuv.y, cuv.x);
    float r   = length(cuv);
    float burstR = 0.30 + 0.08 * sin(ang * 8.0 + TIME * 0.3);
    float burst       = step(r, burstR);
    float burstInner  = step(r, burstR * 0.6);
    raw = mix(LIK_BLUE * 0.7, LIK_YELLOW, burst);
    raw = mix(raw, LIK_RED, burstInner);
}
```
(`aspect` already defined later at line 130 — pull its computation up,
or compute inline.) Now the fresh-out-of-box render reads as the
*Whaam!* silhouette specifically.

### 4. fauvism_matisse.fs — confetti, not blocks

(~3 default-value changes + ~5 added lines for horizon snap.)

`FAUVE[6]` is already exact for Matisse / Derain unmixed primaries. But
default `dropSize = 0.028` and `dropRate = 0.55` produce small
pointillist confetti — *The Open Window, Collioure* and *The Dance* are
LARGE FLAT COLOUR BLOCKS. Three default flips plus a horizon snap
unlock the canonical Matisse compositional grammar.

```
{ "NAME": "dropSize",  "DEFAULT": 0.085 }   // was 0.028
{ "NAME": "dropRate",  "DEFAULT": 0.20 }    // was 0.55
{ "NAME": "paintFade", "DEFAULT": 0.995 }   // was 0.988
```

Plus, in the deposit pass, snap deposits above/below y=0.55 to the
"sky" / "ground" Fauvist-block colours so a horizon line emerges
naturally:

```glsl
// Matisse canvas-wide horizon — snap pigment colour to one zone
// above/below to encourage flat-block planes.
if (abs(uv.y - 0.55) < 0.003) {
    deposit = mix(deposit, vec3(0.10, 0.07, 0.06), 0.4);
}
```

After this the buffer accumulates 4–6 large saturated colour planes
divided by a horizon — *Open Window* compositional structure rather
than uniform confetti.

### 5. opart_vasarely.fs — two-tone Vega is not really Vega

(~15 lines, replacing the `mix(colA, colB, band)` line ~84.)

The spherical bulge geometry is correct for Vega-Nor / Vega 200. But
canonical Vasarely uses POLYCHROME ring assignment — each band picks
from a 4–6 colour palette, not just two-tone alternation. Currently the
shader reads as "generic two-tone op art".

```glsl
int ringIdx = int(floor(phase / 3.14159));
vec3 vegaP[4] = vec3[4](
    hsv2rgb(vec3(fract(0.55 + hueShift), saturation, 0.95)),
    hsv2rgb(vec3(fract(0.10 + hueShift), saturation, 0.95)),
    hsv2rgb(vec3(fract(0.85 + hueShift), saturation, 0.95)),
    hsv2rgb(vec3(fract(0.30 + hueShift), saturation, 0.95))
);
vec3 col = vegaP[ringIdx & 3];
vec3 nextC = vegaP[(ringIdx + 1) & 3];
col = mix(col, nextC, band);
```

Vega-Nor's signature is a spherical lattice cycling through teal /
magenta / orange / yellow — that's the read this unlocks.

---

## Why these five and not the alternatives

The remaining 8 shaders not in this list each have one weakness, but
those weaknesses are smaller:

- **`abex_rothko.fs`** — already canonical for Orange Red Yellow; pass 5's
  inner-glow and Seagram suggestions are polish, not transformation.
- **`bauhaus_kandinsky.fs`** — already canonical for *Several Circles*;
  chequerboard fills (pass 5 suggestion) would push toward Composition
  VIII but it's not invisible without them.
- **`opart_riley_waves.fs`** — *Current* (1964) is reachable; missing
  *Cataract 3* colour mode is one alternate work, not the headline.
- **`memphis_primitives.fs`** — Memphis Group canon is collective so
  there's no single work to nail; turquoise + zigzag (pass 5) is genre
  enrichment not a specific-work fix.
- **`minimalism_stella.fs`** — *Hyena Stomp* default already reads as
  the painting; thin gap for Black Paintings (pass 5) is preset polish.
- **`vaporwave_floral_shoppe.fs`** — bust SDF reads as bust silhouette
  already; nose detail is polish, not iconography push.
- **`ai_latent_drift.fs`** — Anadol palette/flow are correct; specular
  sparkle (pass 5) is enhancement, not new iconography.
- **`futurism_boccioni.fs`** — phantom-blend → max-blend (pass 5) is a
  motion-grammar fix, but not as legible-from-still-frame as adding
  iconographic figures to other shaders.
- **`dada_hoch.fs`** — DADA word-stamp (pass 5) is iconographic but a
  single word is less transformative than e.g. Picasso's letters or
  Kirchner's figures.
- **`glitch_datamosh.fs`** — Pink Dot fallback (pass 5) is iconographic
  and could be #6 — strong contender, but Murata canon is video-process
  oriented and the existing rainbow-garbage fallback already reads as
  glitch art.
- **`constructivism_lissitzky.fs`** — already the second-most-canonical
  shader after rothko.

The five chosen above each meet the bar: a single-feature absence that
breaks the specific-work guess test. Implementing them gets the set
from ~5 specific-work shaders to ~10 — which is the qualitative
threshold where users start to recognise paintings rather than
movements.

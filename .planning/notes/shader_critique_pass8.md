# Shader Critique — Round 8: From One Painting to a Career Tour

Rounds 5–7 made every shader land ONE specific canonical work at default
settings. Round 8 raises the bar: each artist in the canon doc has 5
works × 3 artists = 15 works per movement, but each shader currently
reaches only one of them. The leverage is making each shader a TOUR of
its movement — multiple canonical works reachable via either an enum
preset switch (UI) or an audio-driven palette walk (no UI).

ISF supports `TYPE: "long"` enum inputs (already used by
`minimalism_stella.fs` `palettePreset` and `surrealism_magritte.fs`
`objectChoice`) so the STYLE_CYCLE pattern is one-line manifest +
constants block per shader.

---

## abex_pollock.fs

**Current state:** *Autumn Rhythm* (1950) at default — black skein
dominant, ochre/red accents on raw canvas.

**Round-8 path:** STYLE_CYCLE.

**Proposed upgrade:** add `pollockWork` enum {Autumn_Rhythm,
Lavender_Mist, Number_1A, Blue_Poles, Convergence}. Each remaps the 5
palette constants + `blackWeight` default.

```glsl
// Replace the const POL_* block with a function:
void pollockPalette(int w, out vec3 c0, out vec3 c1, out vec3 c2,
                    out vec3 c3, out vec3 c4, out vec3 canvas) {
    if (w == 1) { // Lavender Mist 1950 — pink/grey/white veils
        c0=vec3(0.18,0.16,0.18); c1=vec3(0.93,0.91,0.92);
        c2=vec3(0.78,0.66,0.74); c3=vec3(0.55,0.50,0.58);
        c4=vec3(0.88,0.78,0.82); canvas=vec3(0.86,0.82,0.78);
    } else if (w == 2) { // Number 1A 1948 — handprints, ochre+black
        c0=vec3(0.06,0.05,0.05); c1=vec3(0.96,0.94,0.88);
        c2=vec3(0.70,0.55,0.20); c3=vec3(0.40,0.32,0.20);
        c4=vec3(0.85,0.75,0.55); canvas=vec3(0.90,0.86,0.74);
    } else if (w == 3) { // Blue Poles 1952 — cobalt verticals
        c0=vec3(0.05,0.04,0.04); c1=vec3(0.92,0.88,0.78);
        c2=vec3(0.10,0.22,0.62); c3=vec3(0.78,0.18,0.14);
        c4=vec3(0.65,0.55,0.20); canvas=vec3(0.84,0.78,0.66);
    } else if (w == 4) { // Convergence 1952 — white/red/yellow chaos
        c0=vec3(0.05,0.04,0.04); c1=vec3(0.96,0.95,0.92);
        c2=vec3(0.85,0.20,0.18); c3=vec3(0.92,0.78,0.20);
        c4=vec3(0.20,0.30,0.55); canvas=vec3(0.80,0.74,0.62);
    } else { // 0 = Autumn Rhythm (current default)
        c0=POL_BLACK; c1=POL_WHITE; c2=POL_SILVR;
        c3=POL_RED;   c4=POL_OCHRE; canvas=RAW_CANVAS;
    }
}
```

Tours: Autumn Rhythm / Lavender Mist / Number 1A / Blue Poles /
Convergence — Pollock's full drip-period range.

---

## abex_rothko.fs

**Current state:** *Orange Red Yellow* (1961) chapel band glow.

**Round-8 path:** STYLE_CYCLE.

**Proposed upgrade:** add `rothkoWork` enum {Orange_Red_Yellow,
No61_Rust_Blue, White_Center, Seagram_Maroon, Black_on_Maroon}. Just
swaps the 3-band colour triplet.

```glsl
void rothkoBands(int w, out vec3 top, out vec3 mid, out vec3 bot) {
    if      (w == 1) { top=vec3(0.55,0.18,0.18); mid=vec3(0.20,0.22,0.45); bot=vec3(0.40,0.16,0.20); }
    else if (w == 2) { top=vec3(0.92,0.86,0.78); mid=vec3(0.85,0.32,0.20); bot=vec3(0.55,0.18,0.40); }
    else if (w == 3) { top=vec3(0.30,0.05,0.06); mid=vec3(0.18,0.04,0.05); bot=vec3(0.10,0.03,0.04); }
    else if (w == 4) { top=vec3(0.05,0.02,0.03); mid=vec3(0.28,0.05,0.06); bot=vec3(0.10,0.03,0.04); }
    else             { top=vec3(0.95,0.50,0.10); mid=vec3(0.85,0.18,0.12); bot=vec3(0.92,0.78,0.18); }
}
```

Tours: Orange Red Yellow / No.61 Rust+Blue / White Center / Seagram /
Black on Maroon — covers the chapel-period spectrum.

---

## ai_latent_drift.fs

**Current state:** Anadol pigment storm + Klingemann face-ghost.

**Round-8 path:** AUDIO_WALK.

**Proposed upgrade:** the palette already arcs through a hue range over
TIME. On bass kicks, jump the palette index through a 4-position canon
sequence so the storm transitions Anadol → Klingemann → Tyka →
StyleGAN-portrait moods. Tracked by `audioBass` rising-edge.

```glsl
// At top of fragment main:
float canonStep = floor(mod(canonWalkPhase, 4.0));
vec3 PA = mix(vec3(0.55,0.32,0.62), vec3(0.20,0.45,0.55), canonStep/3.0); // Anadol mauve→teal
vec3 PB = mix(vec3(0.92,0.55,0.20), vec3(0.18,0.10,0.30), canonStep/3.0); // sunset→Klingemann night
vec3 PC = mix(vec3(0.85,0.78,0.65), vec3(0.10,0.30,0.55), canonStep/3.0); // bone→cobalt
// The latentPalette() call uses PA/PB/PC as the three corners.
```

Tours: Anadol Machine Hallucinations / Klingemann Memories of Passersby /
Anna Ridler Mosaic Virus / Mario Klingemann Mistaken Identity / Helena
Sarin StyleGAN florals.

`canonWalkPhase` increments on each `beatPulse` rising edge inside the
existing audio-react block.

---

## art_nouveau_mucha.fs

**Current state:** *Gismonda* (1894) halo + lily palette.

**Round-8 path:** STYLE_CYCLE.

**Proposed upgrade:** add `muchaWork` enum {Gismonda, Job, Zodiac,
Four_Seasons_Spring, Reverie}. Swaps halo type (full circle vs zodiac
ring vs no halo + flower wreath) and accent palette.

```glsl
// In halo composition:
if (muchaWork == 1)      haloRingW = 0.0;   // Job: no halo, smoke wreath
else if (muchaWork == 2) haloRingW = 0.18;  // Zodiac: thick ring + 12 marks
else if (muchaWork == 3) flowerRingDensity = 1.4; // Spring wreath
else if (muchaWork == 4) accentHue = 0.78;  // Reverie violet
// Default 0 = Gismonda halo.

// And add zodiac tick marks if muchaWork == 2:
float zAng = atan(d.y, d.x);
float zTick = step(0.92, abs(sin(zAng * 6.0)));
halo += zTick * 0.4 * step(0.16, r) * step(r, 0.18);
```

Tours: Gismonda / Job (cigarette papers) / Zodiac / Four Seasons Spring /
Reverie.

---

## bauhaus_kandinsky.fs

**Current state:** *Composition VIII* (1923) chequerboard fills.

**Round-8 path:** STYLE_CYCLE.

**Proposed upgrade:** add `kandinskyWork` enum {Composition_VIII,
Several_Circles, Yellow_Red_Blue, Composition_X, On_White_II}. Each
toggles shape-mix ratios and background.

```glsl
// At top of main:
float circleBias = 0.33, lineBias = 0.33, bgVal = 0.92;
if      (kandinskyWork == 1) { circleBias = 0.95; lineBias = 0.0;  bgVal = 0.06; } // Several Circles black BG
else if (kandinskyWork == 2) { circleBias = 0.20; lineBias = 0.50; bgVal = 0.92; } // Yellow Red Blue grid
else if (kandinskyWork == 3) { circleBias = 0.10; lineBias = 0.10; bgVal = 0.04; } // Composition X black
else if (kandinskyWork == 4) { circleBias = 0.40; lineBias = 0.45; bgVal = 0.97; } // On White II white BG
// 0 = Composition VIII chequerboards (current default)
```

Tours: Composition VIII / Several Circles (1926) / Yellow Red Blue
(1925) / Composition X (1939) / On White II.

---

## constructivism_lissitzky.fs

**Current state:** *Beat the Whites with the Red Wedge* (1919).

**Round-8 path:** STYLE_CYCLE.

**Proposed upgrade:** add `constructivistWork` enum {Beat_the_Whites,
Proun_99, Rodchenko_Books, Stenberg_Man_Camera, Klutsis_5_Year_Plan}.
Swaps wedge geometry + glyph density + dominant colour.

```glsl
if (constructivistWork == 1) { // Proun 99 — axonometric blocks
    wedgeApex = vec2(0.5,0.5); wedgeAng = 0.6;
    glyphDensity = 0.0; bgCol = vec3(0.88,0.82,0.70);
} else if (constructivistWork == 2) { // Rodchenko Books — full-screen typography
    wedgeWidth = 0.0; glyphDensity = 1.6; redOnWhite = true;
} else if (constructivistWork == 3) { // Stenberg — circular lens overlays
    addLensCircles = true; wedgeAng = 1.4;
} else if (constructivistWork == 4) { // Klutsis — fist + diagonal stripes
    addFistSilhouette = true; bgCol = vec3(0.85,0.20,0.18);
}
```

Tours: Beat the Whites / Proun 99 (Lissitzky 1923) / Rodchenko Books
poster / Stenberg Man with a Movie Camera / Klutsis Five Year Plan.

---

## cubism_picasso.fs

**Current state:** *Portrait of Kahnweiler* (1910) — analytical letters.

**Round-8 path:** STYLE_CYCLE.

**Proposed upgrade:** add `picassoWork` enum {Kahnweiler,
Demoiselles_dAvignon, Three_Musicians, Guernica, Ma_Jolie}. Toggles
palette saturation + facet density + silhouette overlay.

```glsl
if      (picassoWork == 1) { // Demoiselles — pink/ochre + mask
    palBase = vec3(0.85,0.62,0.55); palAcc = vec3(0.55,0.30,0.20);
    addMaskOval = true; analyticGreyMix = 0.0;
}
else if (picassoWork == 2) { // Three Musicians — synthetic flat colour
    palBase = vec3(0.88,0.75,0.18); palAcc = vec3(0.20,0.18,0.55);
    facetCount = 6.0; analyticGreyMix = 0.0;
}
else if (picassoWork == 3) { // Guernica — pure greyscale, scream silhouette
    palBase = vec3(0.85); palAcc = vec3(0.10);
    desaturate = 1.0; addHorseSilhouette = true;
}
else if (picassoWork == 4) { // Ma Jolie — analytic with stencil "MA JOLIE"
    glyphText = 1.0; palBase = vec3(0.55,0.50,0.42);
}
// 0 = Kahnweiler default
```

Tours: Kahnweiler / Demoiselles d'Avignon / Three Musicians / Guernica /
Ma Jolie.

---

## dada_hoch.fs

**Current state:** *Cut with the Kitchen Knife* (1919) — face ovals + cogs.

**Round-8 path:** STYLE_CYCLE.

**Proposed upgrade:** add `dadaWork` enum {Hoch_Cut_Kitchen_Knife,
Schwitters_Merzbau, Duchamp_Fountain, Hausmann_Mechanical_Head,
Ernst_Elephant_Celebes}. Toggles patch-kind weights + dominant
silhouette.

```glsl
if      (dadaWork == 1) { // Schwitters — beige paper scraps, no faces
    kindFaceProb = 0.0; kindGearProb = 0.10; bgWarmth = 0.85;
}
else if (dadaWork == 2) { // Duchamp Fountain — single white urinal silhouette
    addUrinalSilhouette = true; stripCount = 1;
}
else if (dadaWork == 3) { // Hausmann — single mechanical head centre
    kindFaceProb = 0.0; addMechHeadCentre = true;
}
else if (dadaWork == 4) { // Ernst Elephant Celebes — surreal elephant body
    addElephantSilhouette = true; bgCol = vec3(0.20,0.18,0.25);
}
// 0 = Höch Cut with the Kitchen Knife (default)
```

Tours: Höch / Schwitters Merzbau / Duchamp Fountain / Hausmann
Mechanical Head / Ernst Celebes.

---

## dali_melt.fs

**Current state:** *Persistence of Memory* (1931) — soft watches.

**Round-8 path:** STYLE_CYCLE.

**Proposed upgrade:** add `daliWork` enum {Persistence_of_Memory,
Disintegration_of_Persistence, Christ_St_John, Swans_Reflecting_Elephants,
Crucifixion_Hypercubus}. Swaps melt object set + horizon palette.

```glsl
if      (daliWork == 1) { // Disintegration — shattered watches floating in cubes
    watchShatter = 1.0; addCubeGrid = true;
    horizonCol = vec3(0.55,0.62,0.70);
}
else if (daliWork == 2) { // Christ St John — overhead crucifix on dark sky
    addCrucifix = true; horizonCol = vec3(0.04,0.05,0.10);
    watchOpacity = 0.0;
}
else if (daliWork == 3) { // Swans Reflecting Elephants — water mirror line
    mirrorY = 0.5; addSwanSilhouette = true; watchOpacity = 0.0;
}
else if (daliWork == 4) { // Hypercubus — floating cross of 8 cubes
    addHypercubeCross = true; watchOpacity = 0.0;
}
// 0 = Persistence of Memory (default)
```

Tours: Persistence / Disintegration / Christ of St John / Swans
Reflecting Elephants / Crucifixion Hypercubus.

---

## destijl_mondrian.fs

**Current state:** *Composition with Red Yellow Blue* (1930).

**Round-8 path:** STYLE_CYCLE.

**Proposed upgrade:** add `mondrianWork` enum {Composition_RYB,
Broadway_Boogie_Woogie, Victory_Boogie_Woogie, Composition_Grey_Lines,
Pier_and_Ocean}. Toggles cell density + accent strategy.

```glsl
if      (mondrianWork == 1) { // Broadway Boogie — yellow grid + red/blue chips
    cellDensity = 18.0; lineCol = vec3(0.92,0.78,0.18); chipMode = true;
}
else if (mondrianWork == 2) { // Victory BB — diagonal coloured dashes
    cellDensity = 14.0; chipMode = true; rotateGrid = 0.785;
}
else if (mondrianWork == 3) { // Composition Grey Lines — no fills
    primaryFillProb = 0.0; lineCol = vec3(0.45);
}
else if (mondrianWork == 4) { // Pier and Ocean — plus-sign field
    plusSignField = 1.0; lineCol = vec3(0.10,0.18,0.40);
}
// 0 = Composition RYB default
```

Tours: Composition RYB / Broadway Boogie Woogie / Victory Boogie Woogie /
Composition Grey Lines / Pier and Ocean.

---

## expressionism_kirchner.fs

**Current state:** *Street Berlin* (1913) — figures + green-purple acid.

**Round-8 path:** STYLE_CYCLE.

**Proposed upgrade:** add `expressionistWork` enum {Kirchner_Street_Berlin,
Munch_Scream, Nolde_Last_Supper, Marc_Blue_Horses, Schiele_Self_Portrait}.
Swaps palette + dominant figure silhouette.

```glsl
if      (expressionistWork == 1) { // Scream — orange sky swirls + scream head
    skyCol = vec3(0.95,0.45,0.18); addScreamHead = true;
    figureCount = 1.0;
}
else if (expressionistWork == 2) { // Nolde — heavy reds and yellows, halos
    palette = vec3(0.85,0.18,0.12); haloMode = true;
}
else if (expressionistWork == 3) { // Marc Blue Horses — three blue horse silhouettes
    addHorseSilhouettes = 3.0; bgCol = vec3(0.85,0.55,0.18);
}
else if (expressionistWork == 4) { // Schiele — single emaciated figure on bone bg
    figureCount = 1.0; figureGaunt = 1.0; bgCol = vec3(0.92,0.86,0.74);
}
// 0 = Kirchner Street Berlin (default)
```

Tours: Kirchner / Munch Scream / Nolde Last Supper / Marc Blue Horses /
Schiele Self-Portrait.

---

## fauvism_matisse.fs

**Current state:** *Open Window Collioure* (1905) — block colour.

**Round-8 path:** STYLE_CYCLE.

**Proposed upgrade:** add `matisseWork` enum {Open_Window_Collioure,
Woman_with_Hat, Dance, Red_Studio, Goldfish}. Each remaps block palette +
adds optional silhouette.

```glsl
if      (matisseWork == 1) { // Woman with Hat — green stripe down face
    palWarm = vec3(0.95,0.32,0.18); palCool = vec3(0.18,0.55,0.30);
    addFaceOval = true;
}
else if (matisseWork == 2) { // Dance — five orange figures on blue/green
    addDancingFigures = true; bgTop = vec3(0.18,0.30,0.55);
    bgBot = vec3(0.20,0.45,0.30);
}
else if (matisseWork == 3) { // Red Studio — flood entire canvas red
    floodRed = 1.0; palWarm = vec3(0.78,0.16,0.10);
}
else if (matisseWork == 4) { // Goldfish — pink cylinder + orange dots
    addGoldfishBowl = true; palCool = vec3(0.55,0.78,0.60);
}
// 0 = Open Window Collioure (default)
```

Tours: Collioure / Woman with Hat / Dance / Red Studio / Goldfish.

---

## futurism_boccioni.fs

**Current state:** *Dynamism of a Cyclist* (1913) — discrete wedges.

**Round-8 path:** AUDIO_WALK.

**Proposed upgrade:** Boccioni / Balla / Severini share the
faceted-motion grammar — bass kicks rotate the palette + wedge density
through the canon. Subtle, no UI clutter.

```glsl
// At top of main, after audio sampling:
float walk = mod(canonWalkPhase, 4.0);
vec3 hueA, hueB; float wedgeDensityMult;
if      (walk < 1.0) { hueA = vec3(0.10,0.22,0.62); hueB = vec3(0.95,0.45,0.10); wedgeDensityMult = 1.0; } // Boccioni Cyclist
else if (walk < 2.0) { hueA = vec3(0.18,0.16,0.18); hueB = vec3(0.92,0.78,0.40); wedgeDensityMult = 1.6; } // Balla Dog on Leash
else if (walk < 3.0) { hueA = vec3(0.55,0.18,0.40); hueB = vec3(0.20,0.55,0.62); wedgeDensityMult = 0.8; } // Severini Dynamic Hieroglyph
else                 { hueA = vec3(0.85,0.18,0.12); hueB = vec3(0.18,0.18,0.18); wedgeDensityMult = 1.4; } // Boccioni States of Mind
```

Tours: Dynamism of a Cyclist / Balla Dynamism of a Dog on a Leash /
Severini Dynamic Hieroglyphic / Boccioni States of Mind: The Farewells /
Carrà Funeral of the Anarchist Galli.

---

## glitch_datamosh.fs

**Current state:** Murata *Pink Dot* fallback.

**Round-8 path:** AUDIO_WALK.

**Proposed upgrade:** the procedural fallback colour-and-shape rotates
through canonical glitch works on bass. Three modes: Murata Pink Dot,
Cory Arcangel I Shot Andy Warhol pixel-blocks, Rosa Menkman vertical
banded corruption.

```glsl
// In the procedural fallback (both places):
float gWalk = mod(canonWalkPhase, 3.0);
if (gWalk < 1.0) {
    // Pink Dot biomorphic blob (current code)
} else if (gWalk < 2.0) {
    // Arcangel I Shot Andy Warhol — orange Warhol-flower 8-bit shape
    vec2 q = floor(uv * 24.0); vec3 px = (mod(q.x+q.y,2.0) < 1.0)
        ? vec3(0.95,0.55,0.20) : vec3(0.18,0.08,0.30);
    fresh = px;
} else {
    // Menkman — horizontal corruption bands of saturated noise
    float band = step(0.5, fract(uv.y * 18.0 + TIME * 0.4));
    fresh = vec3(band, fract(uv.y * 7.3), 1.0 - band);
}
```

Tours: Murata Pink Dot / Arcangel I Shot Andy Warhol / Menkman A Vernacular
of File Formats / JODI wwwwwwwww.jodi.org / Beflix Dawn Boundary.

---

## memphis_primitives.fs

**Current state:** Memphis Group genre (collective, no single canonical work).

**Round-8 path:** STYLE_CYCLE.

**Proposed upgrade:** add `memphisWork` enum {Sottsass_Carlton,
Sowden_D_Lux, Du_Pasquier_Royal, Bedin_Super_Lamp, Generic_Pattern}. The
shader gets a SINGLE-OBJECT mode that builds a recognisable furniture
silhouette over the pattern field.

```glsl
if      (memphisWork == 1) { // Carlton bookshelf — totem of coloured rectangles
    addCarltonTotem = true; patternOpacity = 0.6;
}
else if (memphisWork == 2) { // D'Lux chair — angular pink+black silhouette
    addDLuxChair = true; bgPattern = squiggle;
}
else if (memphisWork == 3) { // Royal sofa — laminate triangle + dot fabric
    addRoyalSofa = true; bgPattern = triangleDot;
}
else if (memphisWork == 4) { // Super Lamp — blue cone + yellow base
    addSuperLamp = true; bgCol = vec3(0.92);
}
// 0 = Generic pattern (current behaviour)
```

Tours: Sottsass Carlton bookshelf / Sowden D'Lux chair / du Pasquier Royal
sofa / Bedin Super Lamp / generic Memphis squiggle. First time the
shader is recognisable as SPECIFIC FURNITURE rather than vibe.

---

## minimalism_stella.fs

**Current state:** *Hyena Stomp* (default `palettePreset=0`); other 3
presets exist but are vague labels.

**Round-8 path:** STYLE_CYCLE (already exists — tighten preset values).

**Proposed upgrade:** rename and tune the 4 existing palettePreset
values to map to NAMED canonical works. Currently `Black Paintings` is
just dark; `Marrakech` and `Synthetic Late` are loose. Lock to specific
works:

```glsl
// Replace LABELS: ["Hyena Stomp","Black Paintings","Marrakech","Synthetic Late"]
// with        : ["Hyena Stomp","Die Fahne Hoch","Marrakech","Harran II"]
// and the colourFromPreset() function:
if (palettePreset == 1) { // Die Fahne Hoch 1959 — pure black on raw canvas pinstripes
    base = vec3(0.04); accent = vec3(0.88,0.83,0.72); satMult = 0.0;
}
else if (palettePreset == 2) { // Marrakech 1964 — fluorescent zigzags
    base = vec3(0.95,0.32,0.55); accent = vec3(0.18,0.78,0.62); satMult = 1.4;
}
else if (palettePreset == 3) { // Harran II 1967 — pastel protractor curves
    base = vec3(0.92,0.55,0.18); accent = vec3(0.55,0.18,0.62); satMult = 1.1;
}
// 0 = Hyena Stomp (default — already correct)
```

Tours: Hyena Stomp / Die Fahne Hoch / Marrakech / Harran II /
(audio-walk could add Six Mile Bottom).

---

## opart_riley_waves.fs

**Current state:** *Current* (1964) — black-white waves.

**Round-8 path:** STYLE_CYCLE.

**Proposed upgrade:** add `rileyWork` enum {Current, Cataract_3,
Movement_in_Squares, Fall, Late_Morning}. Toggles palette (B&W vs
red-turquoise) + wave geometry (sin vs square chequerboard vs vertical
bars).

```glsl
if      (rileyWork == 1) { // Cataract 3 — red/turquoise waves
    colA = vec3(0.85,0.18,0.18); colB = vec3(0.20,0.78,0.78);
}
else if (rileyWork == 2) { // Movement in Squares — chequerboard compression
    waveMode = 1; colA = vec3(0.04); colB = vec3(0.95);
    squareCompress = 1.0;
}
else if (rileyWork == 3) { // Fall — vertical zigzag bars
    waveMode = 2; vertical = 1.0;
}
else if (rileyWork == 4) { // Late Morning — vertical pastel stripes
    waveMode = 2; vertical = 1.0;
    colA = vec3(0.85,0.62,0.20); colB = vec3(0.30,0.55,0.85);
}
// 0 = Current default
```

Tours: Current / Cataract 3 / Movement in Squares / Fall / Late Morning.
Closes the round-7 marginal gap directly.

---

## opart_vasarely.fs

**Current state:** *Vega-Nor* (1969) — polychrome bulging rings.

**Round-8 path:** STYLE_CYCLE.

**Proposed upgrade:** add `vasarelyWork` enum {Vega_Nor, Zebra,
Tlinko, Alom, Vega_200}. Toggles between sphere-bulge (Vega family),
flat black-and-white stripes (Zebra), tessellated diamonds (Tlinko).

```glsl
if      (vasarelyWork == 1) { // Zebra 1937 — pure B&W curved stripes, no bulge
    bulgeAmount = 0.0; colA = vec3(0.04); colB = vec3(0.95);
    stripeMode = 1;
}
else if (vasarelyWork == 2) { // Tlinko — tessellated coloured diamonds
    bulgeAmount = 0.0; tessMode = 1;
    colA = vec3(0.85,0.32,0.18); colB = vec3(0.18,0.30,0.62);
}
else if (vasarelyWork == 3) { // Alom — bulging hexagonal cells
    bulgeAmount = 0.6; cellShape = 6.0;
}
else if (vasarelyWork == 4) { // Vega 200 — black-and-white sphere bulge
    monoBulge = 1.0;
}
// 0 = Vega-Nor (default polychrome)
```

Tours: Vega-Nor / Zebra / Tlinko / Alom / Vega 200.

---

## popart_lichtenstein.fs

**Current state:** *Whaam!* (1963) — explosion + ben-day dots.

**Round-8 path:** STYLE_CYCLE.

**Proposed upgrade:** add `lichtensteinWork` enum {Whaam,
Drowning_Girl, Crying_Girl, M_Maybe, Brushstrokes}. Swaps comic
silhouette + speech-bubble text.

```glsl
if      (lichtensteinWork == 1) { // Drowning Girl — wave + face
    addWaveCurve = true; addFaceProfile = true;
    bubbleText = "I DON'T CARE!";
}
else if (lichtensteinWork == 2) { // Crying Girl — single eye with tear
    addEyeWithTear = true; bgCol = vec3(0.85,0.55,0.62);
}
else if (lichtensteinWork == 3) { // M-Maybe — face profile + bubble
    addFaceProfile = true; bubbleText = "M-MAYBE";
}
else if (lichtensteinWork == 4) { // Brushstrokes — single thick stroke
    addBrushstroke = true; dotsOpacity = 0.0;
}
// 0 = Whaam! default
```

Tours: Whaam! / Drowning Girl / Crying Girl / M-Maybe (He Promised) /
Brushstrokes.

---

## surrealism_magritte.fs

**Current state:** Already has `objectChoice` enum — Apple/Bowler/Rock/
Pipe/SonOfMan. Default = Son of Man.

**Round-8 path:** STYLE_CYCLE (extension — add SKY/BG variants).

**Proposed upgrade:** the object choice covers 5 motifs, but each is
overlaid on the SAME blue cumulus sky. Add `magritteScene` enum
{Empire_of_Light, Day_Sky, Night_Sky, Castle_of_Pyrenees, Golconda} that
swaps the background environment. Cross-product = 5×5 = 25 hybrid works
reachable.

```glsl
if      (magritteScene == 1) { // Empire of Light — daylight sky over night street
    skyTop = vec3(0.55,0.78,0.92); skyBot = vec3(0.10,0.12,0.20);
    addStreetLamp = true;
}
else if (magritteScene == 2) { // Night Sky — black with pinpoint stars
    skyTop = vec3(0.04,0.05,0.10); addStars = true;
}
else if (magritteScene == 3) { // Castle of Pyrenees — floating boulder over sea
    addFloatingBoulder = true; skyBot = vec3(0.18,0.40,0.55);
}
else if (magritteScene == 4) { // Golconda — falling bowler-hat men
    addRainOfMen = true;
}
// 0 = Day Sky (default cumulus)
```

Tours: Son of Man / Listening Room / Empire of Light / Castle of the
Pyrenees / Golconda — and 4× more from the object axis.

---

## vaporwave_floral_shoppe.fs

**Current state:** *Floral Shoppe* (Macintosh Plus 2011) with roses.

**Round-8 path:** STYLE_CYCLE.

**Proposed upgrade:** add `vaporworkAlbum` enum {Floral_Shoppe,
Eccojams_Vol_1, Replica, New_Dreams_Ltd_Initiation_Tape, Hiraeth}. Each
swaps the central iconography (bust → ECCO dolphin → office mannequin
→ palm trees → ocean) + sky palette.

```glsl
if      (vaporworkAlbum == 1) { // Eccojams — ECCO dolphin silhouette over reef sky
    bustOpacity = 0.0; addDolphinSilhouette = true;
    skyTop = vec3(0.20,0.55,0.78); skyBot = vec3(0.40,0.78,0.92);
}
else if (vaporworkAlbum == 2) { // Replica — corporate office plant + window
    bustOpacity = 0.0; addOfficePlant = true; addVerticalBlinds = true;
}
else if (vaporworkAlbum == 3) { // New Dreams Ltd — palm tree silhouettes
    bustOpacity = 0.0; addPalmSilhouettes = 3.0;
    skyTop = vec3(0.92,0.55,0.40);
}
else if (vaporworkAlbum == 4) { // Hiraeth — ocean horizon + setting sun
    bustOpacity = 0.0; gridOpacity = 0.0; addOceanHorizon = true;
}
// 0 = Floral Shoppe default (bust + roses)
```

Tours: Floral Shoppe / Eccojams Vol.1 / Replica / New Dreams Ltd
Initiation Tape / Hiraeth.

---

## Top-3 priority round-8 fixes

The three shaders where adding a multi-work path most multiplies
recognition value are **abex_pollock.fs** (a single palette+blackWeight
swap unlocks Lavender Mist's pinks, Number 1A's ochres, and Blue Poles'
cobalts — Pollock's whole drip period from a 5-line preset table),
**cubism_picasso.fs** (Kahnweiler currently dominates but Demoiselles,
Three Musicians, and Guernica are wildly different visual languages —
adding the enum reaches three more universally-recognised paintings,
including Guernica which is arguably more famous than the default), and
**surrealism_magritte.fs** (already has the object-axis enum; adding the
5-value scene-axis enum creates a 5×5 cross product reaching 25 distinct
canonical Magritte compositions including Empire of Light and Golconda
without rewriting the object code at all — highest leverage per line).

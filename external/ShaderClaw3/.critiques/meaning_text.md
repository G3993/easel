# meaning_text — critique

**Slug:** `meaning_text`
**File:** `/Users/lu/easel/shaders/meaning_text.fs` (copied to `build/Easel.app/Contents/Resources/shaders/`)
**Reference:** `/Users/lu/Documents/A-List Shaders/meaning_text.jpg` ("meaningful things" beaming-design poster)

## Concept

A poster about **a single word**. The cue text is the hero — an oversized
italic-serif headline is typed glyph by glyph across the page with a wavy
hand-drawn underline that breathes with `audio.bass`. Behind it, three
abstract aura-orbs hover at **three different z-planes**, each bound to
its own `player[i].energy` + its own hue family + its own parallax
response. When a voice arrives, that player's orb swells forward and the
others recede into atmospheric fog. Silence: orbs idle-pulse on
`transport.beat` so the page is never dead, but the stillness reads
intentional.

Anti-pattern hygiene: no bars, no spectrum line, no logo glyph, no
horizon symmetry, no SDF debug grid. The orbs are abstract sculptures,
the text is the cue (typewriter via `msgAge`).

## INPUTS (with BIND)

| Name | Type | BIND | Notes |
|---|---|---|---|
| `msg` | text | `cue.latest` | hero word, max 24 chars, typewriter via `msgAge` |
| `energyA` | float | `player[1].energy` | orb A breath + parallax kick |
| `energyB` | float | `player[2].energy` | orb B breath + parallax kick |
| `energyC` | float | `player[3].energy` | orb C breath + parallax kick |
| `activeA` | float | `player[1].active` | promotes orb A while talking |
| `activeB` | float | `player[2].active` | promotes orb B while talking |
| `bassDrive` | float | `audio.bass` | warm page tint + underline wave amp |
| `beatPhase` | float | `transport.beat` | idle-pulse for silence |
| `palette` | long | — | Aurora / Cool tide / Sunset / Mono ink (style) |
| `paletteShift` | float | — | hue rotation (style) |
| `layoutVariant` | long | — | Triad below / Halo around / Diagonal (style) |
| `motionSpeed` | float | — | overall flow (style) |
| `audioDepth` | float | — | how hard audio pushes energy (style) |
| `breathe` | float | — | idle breathing amp (style) |
| `textScale`, `italicSlant`, `underlineWave`, `fog`, `bloom`, `grain`, `paperColor`, `inkColor` | — | — | typographic + atmospheric style |

Hard floor passes: 3 `player[*]` binds + `cue.latest` + `audio.bass` + `transport.beat`.

## glslang

```
$ /tmp/easel_validate.sh /Users/lu/easel/shaders/meaning_text.fs
EXIT=0
```

Harness assembles Easel's ShaderSource.cpp preamble (TIME, RENDERSIZE,
audio*, msgAge, msg_0..msg_23, msg_len, fontAtlasTex, mediapipe stubs)
+ INPUT uniforms, then `#define gl_FragColor FragColor`. Clean compile
under `#version 330 core`. No warnings.

## Rubric — self-score /25

| Axis | Score | Rationale |
|---|---|---|
| **a. Multi-player separability** | **5** | 3 orbs × 3 distinct `player[i].energy` binds, each with its own hue family from `orbPalette(seed)`, its own z-plane, its own parallax phase. Muting `energyB` visibly stills the middle orb while A/C keep breathing. |
| **b. Depth & dimensionality** | **4** | Genuine 3D: per-orb raymarched SDF in cell-local coords; three discrete z-planes back-to-front composited; energy-driven parallax shift on z; fog blend per zPlane fades back orbs into paper. Text on front plane is 2D but italic-shear gives it a slight axonometric feel. Not quite a single coherent 3D scene → 4 not 5. |
| **c. Intentional motion** | **4** | Distinct silence (idle beat-pulse, small) / mid (orb energy swells) / loud (orb pushed forward, underline wave amp surges, page warms) states. Per-glyph reveal is a real moment (newest glyph lifts off baseline and bolds). Could push surprise stops further → 4 not 5. |
| **d. Abstract not literal** | **5** | The orbs are aura-blobs, not anything depicting "meaning". The word IS the subject but reads as poster typography, not decoration of a literal object. No anti-pattern templates (no bars, no waveform, no logo). |
| **e. Surprise / risk** | **4** | Hand-drawn dotted wavy underline whose amplitude is `audio.bass`-driven is a fresh move. Italic-shear per-glyph + newest-glyph baseline-lift is a typographic micro-animation I haven't seen in the corpus. Triad raymarch with depth-sorted z-planes is meaningful — not a stacked grid. Stops short of 5 because the orb language overlaps `meaningful_forms_text.fs`. |

**Total: 22/25.** Hard floor passed (6 binds). No anti-patterns triggered.

## What to try next

- Drop the orb count to 1 in a follow-up variant where the orb sits
  **inside** the counter of the word's "g" descender — typography +
  sculpture as one form.
- Per-glyph hue assignment from `player[i].pitch` so the colour of each
  letter is the voice that birthed it.
- Variable-width baseline distortion driven by `cue.coach.severity` —
  the word literally trembles when the moment is heavy.

## Caveats

- Atlas indices for the fallback word "meaning" assume `a..z = 0..25`.
  This is the convention `text_clusters.fs` and `meaningful_forms_text.fs`
  use (`SPACE_CH = 26`). If Easel's atlas remapped, the fallback would
  show the wrong word — live cue path is unaffected.
- `MAX_GLYPHS = 24` (matches the `MAX_LENGTH: 24` on `msg`). Longer
  utterances will be truncated by `charCount()`.
- No headless screenshot was rendered in this pass (per task brief: no
  relaunch). The visual claims above are derived from the source +
  reference image, not from a live frame.
- Three-orb fixed N: depth sort is hardcoded for N=3 (bubble sort across
  the small idxOrder array). Adding a 4th orb would need the sort
  generalised.

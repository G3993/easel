# `highlighted_text.fs` — critique

## Concept

The reference image (Stedelijk Museum Amsterdam) lives on one idea: pale-blue
highlighter bands sweeping behind selected words on a paper page, accented by two
solid blue dots at the start/end of the swipe. The shader literalizes the *mark*
(highlight + dot) and uses live caption as the substrate.

The signature move: **each word gets a player-slot color** (round-robin 1→2→3),
so a three-way conversation paints itself in three highlighter colors. Mute one
player, watch every third word lose its mark.

## Rubric self-score: **22 / 25**

### a. Multi-player separability — **5 / 5**
Three independent `player[i].energy` + `player[i].active` binds, each gating its
own subset of words via slot assignment. Visually distinguishable: zeroing
`player[2].energy` makes every word with index `1 mod 3` fade to a quiet rest
state while slots 0 and 2 stay hot. Distinct colors (cyan/yellow/magenta)
amplify the separability. Bass binds to `audio.bass` for a global pulse on top.

### b. Depth & dimensionality — **4 / 5**
Four genuine z-layers: paper (with marbled fibre + wash + vignette), highlight
bands on their own plane (with cast shadow lifting the paper underneath them),
ink text in front of bands, accent dots in front of text with halos. Not
raymarched — capped at 4. Could earn a 5 with a real DoF/parallax.

### c. Intentional motion — **4 / 5**
- Sweep animates per-word with the typewriter `msgAge` and `sweepSpeed`,
  not a global lerp.
- Sweep eases into a *pool* at the right edge (felt-tip pause).
- Edges wobble per frame via fbm; grain streaks the inside of the band.
- Accent dots float independently.
- Silence: paper still drifts, ghost highlight sweeps slow.
A bit short of a "5" because there's no big compositional swell on crescendo —
the visual responds smoothly rather than dramatically.

### d. Abstract not literal — **4 / 5**
It's *about* highlighting, but it isn't a logo, EKG, spectrum bar, or icon.
The text comes from `cue.latest`, so it's the live transcript, not decoration.
The reference image directly maps but the player-color decomposition is the
abstract layer the source can't see.

### e. Surprise / risk — **5 / 5**
New authoring move for the corpus: **swipe direction driven by typewriter
character index**, not by `audio.level`. Per-word player-slot color assignment
is a novel decomposition pattern — previous text shaders (`doubleperson_convo`,
`dotconnector_clusters`) treated the whole sentence as one channel. The
marker-tip pooling and rough-edge fbm wobble are visual moves I haven't seen
in this pack.

## Anti-pattern check
- No EKG line ✓
- No spectrum bars ✓
- No icons ✓
- No checkerboard ✓
- No single-color noise plane (4 composed layers) ✓
- No mirror-symmetry (tilt, word colors, dot positions all asymmetric) ✓
- Rendered text is *captioning*, not decorative logo (cue.latest) ✓

## Hard floor
Passes: `cue.latest`, `player[1..3].energy`, `player[1..3].active`,
`audio.bass` — 8 channel binds.

## INPUTS summary

| NAME | TYPE | BIND |
|---|---|---|
| `msg` | text | `cue.latest` |
| `energyA/B/C` | float | `player[1..3].energy` |
| `activeA/B/C` | float | `player[1..3].active` |
| `bassDrive` | float | `audio.bass` |
| `paperColor` | color | — |
| `highlightA/B/C` | color | — |
| `inkColor`, `accentColor` | color | — |
| `textScale`, `sweepSpeed`, `audioDepth`, `opacity`, `grain`, `tiltAmount`, `lineGap`, `kerning` | float | — |

22 inputs total; 7 of them are channel-bound.

## Caveats

- Caption hard-limited to 48 chars (Easel default). Past that the shader
  silently truncates — fine for cue.latest utterances which are sentence-scale.
- `MAX_WORDS = 12`. Longer captions still render text but extra words won't
  receive their own highlight band — they sit on paper. Acceptable degradation.
- Two passes walk the message (highlight + ink). At 48-char ceiling this is
  cheap; if MAX_LENGTH grows, factor the word-table walk into a shared pass.
- Tilt is per-word deterministic from a hash. Looks lively for short captions;
  on very long ones the tilts may visually collide. The 0.06 default is gentle.
- `audioDepth` only modulates bass; energies are passed straight (intelligence
  layer expects pre-smoothed channels).

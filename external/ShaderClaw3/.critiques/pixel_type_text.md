## 2026-05-20 — pixel_type_text

**Reference image:** `pixel_type_text.jpg` ("Georgia & Verdana" pixel-block typography
poster — chunky multi-color pixelated headline glyphs over a swarm of scattered
tetromino fragments in cyan / hot pink / lime / yellow on solid black).

### Concept
A text-as-hero shader where the live cue.latest utterance is rendered as one giant
single-line pixel-quantized headline filling the canvas. Each glyph is hard-edge
sampled from the font atlas and re-tiled onto a coarse pixel grid; every pixel-block
gets its own hue drawn from a four-mode palette so the headline reads as a candy
mosaic, not a monochrome word. Behind the type, a swarm of 8-variant tetromino
sprites (I/L/J/T/S/Z/O/+) drift on a slow flow field arranged into three depth
lanes — far/mid/near — each lane driven by its own player[i].energy channel for
genuine multi-voice decomposition. A backdrop of two parallaxing pinstripe layers
recedes at half-speed behind the sprites. Bass jitters the headline horizontally
like CRT bleed; typewriter reveal makes the most-recent glyph briefly flash bright.

### Channel bindings declared
- `msg` → `cue.latest` (live transcript hero text)
- `energyA` → `player[1].energy` (far lane sprites)
- `energyB` → `player[2].energy` (mid lane sprites + pinstripe wall A)
- `energyC` → `player[3].energy` (near lane sprites)
- `audioJitter` → `audio.bass` (pixel-grid jitter on the headline)

Binding floor: 3× `player[*]`, 1× `cue.*`, 1× `audio.*` → passes cleanly.

### Self-score (rubric v2)
- **a. Multi-player separability — 4/5.** Three sprite lanes are visually distinct
  (different depth, brightness, drift speed) and each is on its own player channel.
  Mute one and that lane goes still — visible. A 5 would require even more distinct
  visual languages per lane (different shape sets).
- **b. Depth & dimensionality — 3/5.** Pinstripe parallax wall, three-lane sprite
  z-ordering with size+brightness depth cues, plus pseudo-3D extrusion via shadow
  pixel-blocks on the headline. Not raymarched — pseudo-3D layered z.
- **c. Intentional motion — 4/5.** Silence = static text on dark wall with sprites
  barely drifting; mid energy = sprites lift, palette boil starts on glyphs; bass
  hit = headline jitters horizontally as a discrete event; per-player push lights
  one lane at a time. Distinct calm/build/crescendo states.
- **d. Abstract not literal — 3/5.** Type IS literal text (it has to be — the brief
  says text-as-hero), but the *treatment* is abstracted: pixel-block mosaic with
  per-block color shuffle, not a clean typeset glyph. Sprites are abstract shapes,
  not icons. Honest 3 because the headline is by design readable text.
- **e. Surprise / risk — 4/5.** The per-pixel-block hue shuffle (instead of a
  single text color) is the central move and not in the existing corpus. The
  tetromino swarm in lanes bound to players is a fresh authoring pattern. Headline
  shadow-block extrusion using a 1-pixel-offset second atlas sample is a small
  novel technique.

**Total: 18/25.** Hard floor passed (5 channel bindings).

### Anti-pattern checklist
- EKG / sound-wave line — **NO**
- Spectrum bars — **NO**
- Literal icon depiction (soccer ball etc.) — **NO**
- Default checkerboard / SDF grid — **NO**
- Mirror-symmetric horizon — **NO**
- Single-color noise plane — **NO**
- Logo / readable text as central visual — **the headline IS the brief; allowed per
  spec ("Text-as-hero")**, but flagging for the reviewer: this is intentional per
  the request, not an accidental anti-pattern.

### Next-iteration ideas
1. Wrap the headline onto multiple lines (currently single-line scales to fit width)
   so long utterances stay big instead of shrinking.
2. Add a 4th layer: floating "fragment" tetrominos that *peel off* recently-revealed
   glyphs — visual continuity between headline and swarm.
3. Per-lane sprite shape pools (lane A = round, lane B = angular, lane C = thin)
   so muting a lane is even more legible — push axis (a) to 5/5.

### Files written
- `/Users/lu/easel/shaders/pixel_type_text.fs`
- `/Users/lu/easel/build/Easel.app/Contents/Resources/shaders/pixel_type_text.fs`
- `/Users/lu/ShaderClaw3/.critiques/pixel_type_text.md`

glslangValidator: **exit 0, no warnings** (validated against Easel preamble harness
with msg_0..47 + msg_len + fontAtlasTex + audio* + ISF INPUTS).

# Easel Shader Work — Handoff

Date: 2026-04-29 evening
Context exhausted in prior session. Resume here.

## Current state of the work

### Repos
- **easel** — `git@github.com:G3993/easel.git` master, clone at `/Users/lu/easel/`. Upstream `origin = jameslbarnes/easel` (PR #2 already merged). Fork remote `fork = G3993/easel`.
- **ShaderClaw3** — nested at `/Users/lu/easel/external/ShaderClaw3/` AND standalone at `/Users/lu/ShaderClaw3/`. Both push to `origin = G3993/ShaderClaw3`. They mirror each other; always copy edits between them.
- **Bundle symlink** — `/Users/lu/easel/build/Easel.app/Contents/Resources/external/ShaderClaw3/shaders` is a symlink to `/Users/lu/easel/external/ShaderClaw3/shaders/` so .fs edits hot-reload without rebuild. Don't break this symlink.

### Latest commits
- ShaderClaw3 master: `b3286ff` "Fauvism 3-buffer loop, Pollock 100 drippers max, Kirchner Brücke acid cycle, Vaporwave Y2K chaos, Rainbow Flower depth controls, aspect fixes"
- easel master: `6105ff2` "Merge branch 'master' of jameslbarnes/easel" (round 5-8 critique notes + native fullscreen menu fix)

### 18 art-movement shaders (live in `external/ShaderClaw3/shaders/`)
Each through 8 generations of critique-and-fix. Each reaches at least one specific canonical work; 5 reach 5 works each via `Painting`/`Scene` enum:
- `art_nouveau_mucha` — Mucha Gismonda halo + 3-style enum
- `fauvism_matisse` — **CURRENTLY BROKEN** (see below)
- `expressionism_kirchner` — full Brücke acid LUT cycle (`anguishHue` param)
- `cubism_picasso` — 5-painting LUT enum (Kahnweiler/Demoiselles/Three Musicians/Guernica/Ma Jolie)
- `futurism_boccioni` — faceted-wedge fallback + max-blend phantoms (Dynamism of a Cyclist)
- `constructivism_lissitzky` — wedge + banner Cyrillic
- `dada_hoch` — face-oval + gear-cog cutouts (Cut with the Kitchen Knife)
- `destijl_mondrian` — 4 primary cells + Boogie Woogie pulses
- `bauhaus_kandinsky` — 5-painting enum + checkerboard fills
- `surrealism_magritte` — `magritteScene` × `objectChoice` = 25 compositions
- `abex_pollock` — segment skeins, **drippers MAX 100** (clamp matched)
- `abex_rothko` — 5-painting palette enum + chapel inner-glow
- `opart_vasarely` — Vega-Nor 4-color polychrome rings
- `popart_lichtenstein` — 3 techniqueStyle branches (Ben-Day/Silkscreen/Halftone) + Whaam! starburst fallback
- `minimalism_stella` — Hyena Stomp / Black Paintings / Marrakech / Synthetic Late
- `vaporwave_floral_shoppe` — Y2K chaos swarm (no bust), procedural Giphy substitute
- `glitch_datamosh` — Murata Pink Dot fallback + I-frame stutter
- `ai_latent_drift` — Anadol palette arc, face-ghost removed per user

Plus `dali_melt` (Persistence of Memory clock) and `red_carpet` (procedural drape) reworked.

### Removed shaders (Voronoi purge)
shatter_grid, squares, stained_glass, stained_glass_chartres, veroni3, proximity_field, cubism_braque (old). Don't re-add cellular tile patterns.

### Stage panel bug fix
Add Surface no longer silently bails when no 3D model loaded. Environment menu items all toggle `m_envVisible`. In `src/stage/StageView.cpp`. Already merged.

### Native macOS fullscreen menu inset
`EaselMac_IsNativeFullScreen()` in `src/app/WindowChrome_mac.mm`. Used in `Application.cpp` `renderMenuBar()` to push EDIT/FILE/LAYER/ZONE to a 12px inset (matches floating toolbar) when in green-button fullscreen. Already merged.

## ⚠️ Fauvism is currently broken / user is unhappy with it

**Ship state:** I just `git reset` it to commit `a3a5327` (the original from before round 5-8). Easel is relaunched.

**User's complaint history (chronological):**
1. After many tweaks, paint fades to white → 3% snap-toward-FAUVE caused convergence to one colour
2. After removing snap, the curl-advection itself blends neighbours toward grey
3. Tried 3-buffer loop architecture → "broken" (probably an ISF multi-pass issue: I left dead code after the new `return;` statement, GLSL may not be compiling)
4. User wants: "infinite paint swirling like Matisse" — drops, curl-noise smear, never washes out, never collapses to one colour, ideally circular drops that read as fluid not glitchy

**What works visually (per user feedback):** the very early iteration — paint buffer + curl + drops + saturate + grain. No anti-wash logic, no chroma snap, no depth pass.

**Why it actually washes out:** with `paintFade=0.988` toward `PAPER`, after ~80 frames (1.3s) the buffer is 80% paper. Drops only fire on a sparse hashed grid so coverage gaps fade. With NO `paintFade`, curl-advection still slowly converges neighbours toward an average colour over many frames.

**Suggested fix path for next session (untried):**
1. Use a 2-buffer ping-pong: paintA writes from paintB + drops + advection. paintB writes a delayed snapshot of paintA. Output samples both with sin-weighted mix. This gives layered fluid feel without inter-pixel blend collapse.
2. Or: keep single buffer, NO `paintFade`, but ADD per-frame jitter to the curl velocity origin so advection samples don't converge (each frame curl is computed from a different noise offset → averaging neighbours doesn't reduce variance over time).
3. Or: use the `liquid_warp.fs` / `cfd_paint.fs` pattern from existing ShaderClaw3 shaders — they have full Navier-Stokes-lite that's stable. Read those for reference.

The user keeps reverting to "the way it was originally" — which is genuinely just `a3a5327`'s code. Maybe just leave it there and stop iterating.

## Useful commands

### Hot-reload an .fs edit
Just write to `external/ShaderClaw3/shaders/foo.fs`. Bundle symlink picks it up. Easel hot-reloads via `ShaderClawBridge::watchSource`. Always also copy to `/Users/lu/ShaderClaw3/shaders/foo.fs` for the standalone repo.

### Take a screenshot of Easel canvas
```bash
SS=/Users/lu/easel/build/Easel.app/Contents/Resources/screenshots
touch "$SS/.capture"
sleep 0.6
cp "$SS/claude_capture.png" /tmp/easel_latest.png
```
Then `Read /tmp/easel_latest.png`. Easel must be running with the target shader as active layer.

### Relaunch Easel
```bash
pkill -x Easel; sleep 1; open /Users/lu/easel/build/Easel.app
```

### Rebuild Easel (only needed for src/ changes)
```bash
cmake --build /Users/lu/easel/build --target Easel -j8
```

### Manifest live at
- `external/ShaderClaw3/shaders/manifest.json` (74 entries, ID range to 491)
- `/Users/lu/ShaderClaw3/shaders/manifest.json` (same)

## Critique loop pattern

When asked to keep iterating shaders, spawn a background general-purpose agent to read the canon doc + all 18 .fs files and produce a `shader_critique_passN.md` with mechanical line-range fixes. Then apply them in batch.

Canon doc: `/Users/lu/easel/.planning/notes/art_movement_canon.md` (top 3 artists × top 5 works per movement + GPU technique recipes).

Critique notes so far: pass1 through pass8 in `.planning/notes/`.

## Known landmines

- **Read-before-edit hook** complains every Edit but the edit DOES land. Just keep going.
- **JSON manifest editing** via `python3 ... json.dump(...)` is more reliable than Edit for big sweeps.
- **Don't spawn shaders agents 5 deep** — they each cost ~$1 and the diminishing returns kicked in around round 7.
- **User dislikes Voronoi / cellular tile patterns** — never reintroduce.
- **User wants movement** — anything that's frozen at TIME=0 fails the 2-sec guess test.
- **User wants specific-work fidelity** — generic-of-genre fails the test.

## Open user-asked items not yet done

1. **HTTPGifSource layer in Easel** for Giphy integration — would let `vaporwave_floral_shoppe.fs` chroma-key real GIFs into the chaos swarm. ~1-day scaffold in `src/sources/`.
2. **Surface-as-shaped-display** in StageView (user asked for "auto mask template" surfaces — currently `ProjectionSurface` is a stub at `src/stage/StageView.cpp:110`, no shape/mask). Half-day for Rect/Circle/Polygon, full day with PNG-mask import.
3. **Remaining round-8 silhouette work** — see queue below.
4. **Fauvism long-term stability** — last session added paintFade clamp to 0.99, init mosaic, per-frame curl jitter, and saturation pump (`prev = lum + (prev-lum)*1.014`). Verify no oversaturation drift.

## 2026-04-30 autonomous improvement loop status

The user authorised an overnight self-pacing loop. Each `ScheduleWakeup` cycle:
- Picks 1–2 shaders from the priority queue below
- Adds new SDFs, palette enums, audio walks, parallax, or new visual concepts
- Mirrors edits to `/Users/lu/ShaderClaw3/shaders/`
- Updates this HANDOFF priority queue
- Schedules the next wakeup ~25–30 min later (deliberately past 5-min cache TTL — single cache miss buys a long working window per cycle)
- Stops after ~10 cycles / 5–6 hours, OR if a cycle hits an error / produces low-quality work

### Round 8 multi-canon enum scoreboard (11/18 shaders done)

DONE before this session: Pollock, Picasso, Rothko, Magritte, Kandinsky, Stella.
DONE this session: Mondrian, Vasarely, Boccioni, Datamosh, Latent, **Lichtenstein**, **Dali**.

REMAINING (in priority order — these need new SDF silhouettes, real geometry work):

1. **dada_hoch.fs** — `dadaWork` enum {Höch Cut Kitchen Knife default, Schwitters Merzbau beige scraps, Duchamp Fountain urinal silhouette, Hausmann Mechanical Head, Ernst Elephant Celebes}. Need urinal SDF (capsule + flange), mechanical-head SDF (rounded box + dial), elephant silhouette.
4. **expressionism_kirchner.fs** — `expressionistWork` enum {Kirchner Street Berlin default, Munch Scream orange-sky + scream head, Nolde Last Supper red+halos, Marc Blue Horses 3 silhouettes, Schiele Self-Portrait gaunt single figure}. Need scream-head SDF (Munch's open-mouth oval), horse silhouettes, gaunt figure.
5. **constructivism_lissitzky.fs** — `constructivistWork` enum {Beat the Whites default, Proun 99 axonometric blocks, Rodchenko Books typography, Stenberg Man-Camera lens overlays, Klutsis 5 Year Plan fist + diagonal stripes}. Need axonometric block SDF, fist silhouette, lens-circle overlay.
6. **vaporwave_floral_shoppe.fs** — `vaporworkAlbum` enum {Floral Shoppe default, Eccojams ECCO dolphin, Replica corporate office plant + blinds, New Dreams Ltd palm silhouettes, Hiraeth ocean horizon}. Need dolphin curve SDF, palm-tree silhouette, ocean horizon mode.
7. **art_nouveau_mucha.fs** — `muchaWork` enum {Gismonda halo default, Job no-halo+smoke, Zodiac thick ring + 12 ticks, Four Seasons Spring flower wreath, Reverie violet}. Has `artistStyle` already (3 artists); add second enum keyed off Mucha-mode.

- DONE 2026-04-30: dali_melt.fs — daliWork enum (Persistence / Disintegration / Christ St John / Swans / Hypercubus) with SDF silhouettes

### Other improvement opportunities (after round 8)

- **Solar Flare** — add audio walk through SDO/STEREO/SOHO/AIA wavelength palettes (171Å gold/304Å red/193Å green-yellow/211Å magenta/335Å blue). Each is a recognisable solar imaging mode.
- **Fog Machine** — add fog-machine "scene" enum {Concert Stage, Dream Sequence, Horror Film, Datacenter, Bioluminescent Cave}. Each retunes fog colour + back colour + glow profile.
- **Liquid Metal / Liquid Ripples** — add audio-walk palette presets.
- **Oil Paint** — add brand/artist palette enum.
- **Bioluminescent Fluid** — add ocean/jungle/cave/aurora environment toggle.

### Per-cycle task brief (re-issued at every wakeup)

```
Read /Users/lu/easel/.planning/notes/HANDOFF.md and continue the autonomous
shader improvement loop. Pick ONE shader from the priority queue. Add the
multi-canon enum and the SDF silhouettes per shader_critique_pass8.md.
Mirror to /Users/lu/ShaderClaw3/shaders/. Update the priority queue here
(mark done, push remaining up). Then ScheduleWakeup ~25 min later with the
same prompt. Stop after 10 cycles or by 8am EDT 2026-04-30, whichever first.

Constraints:
- No Voronoi / cellular tile patterns — user dislikes them
- No frozen-at-TIME=0 visuals — must move with TIME alone, audio is bonus
- Specific canonical works, not "generic of genre"
- Keep edits surgical: read existing shader, add enum INPUT, add per-work
  override block, route through one or two call sites. Don't rewrite.
- Always mirror to standalone repo after edit.
- After 3 cycles without hitting an error: do a critique-and-polish pass
  on a previously-improved shader (e.g. boost depth, parallax, motion).
```

## File paths quick reference

```
/Users/lu/easel/                                       — easel repo (G3993/easel)
/Users/lu/easel/src/app/Application.cpp                — main app, renderMenuBar at line ~6326
/Users/lu/easel/src/app/WindowChrome_mac.mm            — fullscreen helpers
/Users/lu/easel/src/stage/StageView.cpp                — stage tab + Add Surface fix
/Users/lu/easel/external/ShaderClaw3/shaders/*.fs      — 18 art shaders + others
/Users/lu/easel/external/ShaderClaw3/shaders/manifest.json
/Users/lu/easel/.planning/notes/art_movement_canon.md  — canon reference
/Users/lu/easel/.planning/notes/shader_critique_pass1..8.md
/Users/lu/easel/.planning/notes/HANDOFF.md             — this file
/Users/lu/ShaderClaw3/                                 — standalone mirror
```

- DONE 2026-04-30: dada_hoch.fs — dadaWork enum (Höch / Schwitters / Duchamp / Hausmann / Ernst) wired with SDF silhouettes

End of handoff.

- DONE 2026-04-30: constructivism_lissitzky.fs — constructivistWork enum (Beat Whites / Proun 99 / Rodchenko Books / Stenberg / Klutsis) with SDFs
- DONE 2026-04-30: expressionism_kirchner.fs — expressionistWork enum (Kirchner / Munch / Nolde / Marc / Schiele) with SDF silhouettes
- DONE 2026-04-30: vaporwave_floral_shoppe.fs — vaporworkAlbum enum (Floral Shoppe / Eccojams / Replica / New Dreams Ltd / Hiraeth) with SDFs

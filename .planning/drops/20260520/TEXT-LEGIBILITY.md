# Text Legibility Audit — 37 _text Shaders (drop 20260520)

Render config: 1920x1080 single frame at t=4s (and 4-frame contact sheets in
`<slug>/rendered.png`), msg = "HELLO WORLD CONVERSATION VISUAL TEST" (default
in `render_isf.py`).

## Pre-fix verdicts

| Shader | Verdict | Rationale |
|---|---|---|
| gradient_text | PASS | "HELLO WORLD CONVERSATION VISUAL TEST" upright across all rows. |
| dotconnector_clusters_text | PASS | Caption inside dot-cluster reads upright, left-to-right. |
| pixel_type_text | FLIPPED_Y | Pixelated by design; underlying atlasUV used `1.0 - sampleP.y/cellH`. |
| circle_text | PASS | Letters on cards arranged around ring; rotation is design-intentional, glyphs upright relative to card. |
| grid_text | FLIPPED_Y | Random-rotated letters; underlying gUv used `0.5 - d.y/gH`. |
| spiral_text | NO_TEXT_VISIBLE | Glyphs tangent to spiral path at tiny radius — too small to verify. |
| forms_text | PASS | Caption reads upright on multiple lines. |
| doubleperson_convo_text | PASS | Speech-bubble text upright and readable. |
| connectnumbers_text | FLIPPED_Y | Bottom caption upside-down (each letter Y-flipped). Also caption band was too narrow to show full glyph height. |
| colorpantone_boxes_text | PASS | Per-card caption labels small but upright. |
| data_collector_text | FLIPPED_Y | Both header and caption upside-down. |
| highlighted_text | PASS | Caption upright in colored highlight pills. |
| morphing_organic_text | FLIPPED_Y | Watermark text upside-down. |
| digital_wave_grid_text | FLIPPED_Y | Both top and bottom slabs show upside-down text. |
| gradient_box_lines_text | PASS | Letters arrayed along curves; all upright. |
| poster_type_text | PASS | "HELLO WORLD / CONVERSATION VISUAL / TEST" upright. |
| type_shapes_blobs_text | PASS | Caption upright. |
| gemstones_border_text | FLIPPED_Y | Inner caption upside-down; ALSO rows filled bottom-up (visual order reversed). |
| png3d_rows_text | FLIPPED_Y | Repeated message across 3D rows all upside-down. |
| data_minimal_lines_text | FLIPPED_Y | Single caption line upside-down. |
| form_plus_lines_text | PASS | Top caption upright (uses correct y-down→v-up double-flip). |
| shape_grid_circular_text | FLIPPED_Y | Bottom caption upside-down. |
| images_3dshape_text | NO_TEXT_VISIBLE | Scene dark in this stub (image-dependent); not a flip issue. |
| moving_grid_gradient_text | FLIPPED_Y | Bottom caption upside-down. |
| moving_circular_grid_text | PASS | Tiny corner caption upright (canonical `1.0 - yInRow/effCharH` pattern). |
| meaningful_forms_text | FLIPPED_Y | Two captions upside-down. |
| sphere_images_text | PASS | "HELLO WOR / LD CONVER / SATION VI / SUAL TEST" upright, word-wrapped. |
| abstract_subtle_gradient_text | NO_TEXT_VISIBLE | No text rendered (low-contrast design; not a flip issue). |
| grid_colors_text | FLIPPED_Y | Colored bars contain text strips, each upside-down. |
| images_maximalist_text | PASS | Repeated tile, all upright. |
| maximalist_3dshapes_text | PASS | "HELLO WORLD CONVERSATION VISUAL" upright. |
| meaning_text | FLIPPED_Y | "...RATION VISU..." upside-down. |
| image_grid_colorbg_text | PASS | Left-side caption stack + central HELLO WORLD all upright. |
| imagedrag_text | PASS | "...LLO WORLD CONVERSATION VISUAL TEST" upright. |
| images_grid_lines_text | PASS | Right-side caption block upright. |
| images_with_text | PASS | Caption block upright. |
| shape_grid_text | FLIPPED_Y | Bottom caption upside-down (corner digits also affected but less visible because 0,1,8 are near-symmetric). |

**Summary**: 15 FLIPPED_Y, 3 NO_TEXT_VISIBLE (spiral, images_3dshape, abstract_subtle_gradient — none of which are flip issues), 19 PASS. Zero BACKWARD_X. Zero GARBLED.

## Root cause

All 15 FLIPPED_Y shaders made the same mistake in the font-atlas V mapping.

Canonical correct idiom (text_clusters.fs / dotconnector_clusters_text.fs and
others):

```glsl
float ly = -lp.y;          // flip y-up world → y-down
// or: float ly = boxHalf - localP.y;    // y-down from box top
...
cellLocal.y = 1.0 - yInRow / effCharH;   // back to v-up so screen-top → v=1
```

The double-flip (y-down then `1.0 -`) means the atlas v=1 ("letter top" in
our atlas convention) maps to screen-top. Net effect: glyph upright.

Buggy idiom in the 15 FLIPPED_Y shaders:

```glsl
vec2 lp = p - anchor;      // p.y is y-UP, anchor at the BOTTOM of the cell,
                           // so lp.y grows UP from 0 to cell.y at top.
cuv.y = 1.0 - lp.y / cell.y;   // single flip puts atlas v=1 at SCREEN-BOTTOM
```

That single `1.0 -` flips screen-top → v=0 → letter BOTTOM, so glyphs render
upside down.

The fix is the same for every site: **drop the `1.0 -` (or equivalent
form-change)** so that lp.y is mapped directly to cuv.y. Specifically:

- `1.0 - lp.y / cell.y`  → `lp.y / cell.y` (connectnumbers, gemstones_border, png3d_rows, meaningful_forms, meaning, shape_grid, pixel_type, data_minimal_lines caption, data_collector caption)
- `1.0 - (tp.y+0.5*gH)/gH` → `(tp.y+0.5*gH)/gH` (morphing_organic)
- `(y1 - screenUv.y)/gH` → `(screenUv.y - y0)/gH` (digital_wave_grid, moving_grid_gradient)
- `1.0 - (rowFrac-0.05)/0.85` → `(rowFrac-0.05)/0.85` (data_collector grid)
- `1.0 - ly/baseH` → `ly/baseH` (shape_grid_circular)
- `0.5 - d.y/gH` → `0.5 + d.y/gH` (grid_text)
- `1.0 - tlocal.y/blockH` → `tlocal.y/blockH` (grid_colors)

Two shaders needed additional adjustments:
- `connectnumbers_text`: caption band was sized to `bandH=0.05` half-height,
  but the cell glyph height is `gh ≈ 0.064` so most letters were clipped.
  Resized the band to `bandH = gh*0.55` and centered the cell on the band.
- `gemstones_border_text`: rows filled bottom-up. Added a `usedRows-1-cursorR`
  index reversal so row 0 sits at the top of the caption block.

## Post-fix verdicts (re-rendered after triple-sync)

| Shader | Verdict |
|---|---|
| connectnumbers_text | PASS — "HELLO WORLD CONVERSATION VISUAL TEST" upright across bottom band. |
| data_collector_text | PASS — caption and grid digits upright. |
| morphing_organic_text | PASS — watermark upright (sparse but legible). |
| digital_wave_grid_text | PASS — "HELLO WORLD CONVERSATION VISUAL TEST" upright in slab. |
| gemstones_border_text | PASS — "HELLO WORLD CONVERSATION / VISUAL TEST" top-down, upright. |
| png3d_rows_text | PASS — "HELLO" "WORLD" "TEST" "CONVERSATION" upright in 3D rows. |
| data_minimal_lines_text | PASS — caption upright. |
| shape_grid_circular_text | PASS — bottom caption upright. |
| moving_grid_gradient_text | PASS — caption upright. |
| meaningful_forms_text | PASS — both caption rows upright. |
| meaning_text | PASS — typewriter glyphs upright. |
| shape_grid_text | PASS — caption AND corner digits now upright. |
| grid_text | PASS — each scattered glyph upright. |
| grid_colors_text | PASS — each colored strip shows upright text. |
| pixel_type_text | PASS (with caveat) — glyphs still fragmented by design `pixelSize`; per-pixel-block atlas mapping is now correct. Legibility limited by design intent at default pixelSize=40 (lower values produce clearer letters). |

Outstanding non-flip caveats:
- `spiral_text`: glyphs are tangent to the spiral and very small at this render
  scale. Manual inspection of the V mapping (`across` is the radial projection)
  shows it's consistent with the rotation frame, but couldn't be visually
  verified at HD resolution. No change made.
- `images_3dshape_text` and `abstract_subtle_gradient_text`: these don't render
  text in the stubbed sample (image-input dependent or low-contrast caption);
  not a flip bug.

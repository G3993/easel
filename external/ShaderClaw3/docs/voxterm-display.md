# VoxTerm Display

ShaderClaw can run as a fullscreen VoxTerm transcript display. It watches the
live markdown transcript files that VoxTerm already writes to:

```text
~/Documents/voxterm-transcripts/.live
```

Start ShaderClaw in display mode:

```bash
npm run voxterm
```

That opens:

```text
http://localhost:7778/voxterm
```

The display mode hides the editor panels, keeps the canvas fullscreen, and feeds
new VoxTerm transcript lines into the text layer. It cycles through curated text
shaders with background shaders:

- `text_la_bloom.fs` over `sonoluminescence.fs`
- `text_matrix.fs` over `time_glitch_rgbfs.fs`
- `text_cascade.fs` over `metamorphosis.fs`
- `text_digifade.fs` over `ether.fs`
- `text_coil.fs` over `soph_orb.fs`
- `text_spacy.fs` over `laser_labyrinth.fs`

Every few transcript lines, ShaderClaw advances to the next scene and derives a
fresh palette from the active transcript session.

Useful endpoints:

```text
GET /api/voxterm/status
GET /api/voxterm/events
```

Keyboard controls in the display window:

```text
Space or Tab  next shader scene
F             fullscreen
Escape        pause the transcript stream
```

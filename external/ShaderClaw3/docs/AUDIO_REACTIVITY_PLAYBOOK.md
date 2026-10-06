# Audio Reactivity Playbook (ShaderClaw / Easel house rules)

Distilled from deep research into Milkdrop/Butterchurn internals, Shadertoy exemplars
(iq, kishimisu, Kali), ISF/VDMX practice, Resolume/TouchDesigner/Synesthesia pro
workflow, and MIR literature. This is the reference for authoring and improving
every audio-reactive shader in the library.

## The uniform bus (web = js/audio.js + isf.js; native = ShaderSource.cpp)

Core (always cheap): `audioLevel audioBass audioMid audioHigh` (smoothed,
asymmetric attack/release), `audioFFT` (256×1 texture).
Feature bus (conditional, native parity): `audioSub audioLowMid audioHighMid
audioTreble audioPunch audioBeat audioBeatPulse audioBeatPhase audioBarPhase
audioBPM audioBrightness audioFlux audioOnset audioEnergy audioEnergyVel
audioBuildup audioDrop audioValence audioArousal audioWarmth ...` plus palette
anchors `audioPalShadow/Mid/High/Accent` (vec3) and helpers
`audioSpectrum(f) audioKick() audioHit() audioBreath() audioAlive(rest,drive,amt)
audioPalette(t)`.

## The 7 laws

1. **Structure on beats, texture on levels, color on spectral character.**
   Levels modulate continuous params; beats change structure (seed re-roll,
   palette flip, camera cut, fold parameter jump) then ease back.
2. **Never map raw values to position.** Envelopes for continuous signals;
   decaying events (birth time + finite life 300–1200 ms) for transients.
3. **Frequency → space.** Bass moves big/central/global things; mids drive
   mid-scale detail/turbulence; highs drive fine/peripheral sparkle. Give each
   element its own band + phase lag (hash of element id) so nothing snaps in
   lockstep.
4. **Give the image memory.** Feedback buffers / history textures; one beat
   should stay visible for seconds (rings that travel, trails that advect).
5. **Audio injects energy into a system with its own dynamics.** Impulse in,
   physics out — momentum/inertia is what reads as "physical".
6. **Soft knees + floors, never linear.** `pow(knee(x,0.05,0.9), 0.6..1.6)`;
   idle floor so silence ≠ dead (`drive = 0.25 + 0.75*knee(audioEnergy,...)`).
7. **Sound-off test.** The shader must be beautiful in silence; audio is
   roughly a third of total motion, not all of it. House default smoothing
   0.85; ease reactivity in (intensity ~0.22 start).

## Standard conditioning snippet (paste into shaders)

```glsl
float knee(float x, float lo, float hi){ return smoothstep(lo, hi, x); }
float bassP = pow(knee(audioBass, 0.05, 0.85), 1.6);  // structural weight
float highP = pow(knee(audioHigh, 0.10, 0.90), 1.2);  // sparkle
float drive = 0.25 + 0.75 * knee(audioEnergy, 0.05, 0.9); // never zero
// log-frequency FFT lookup — musical energy lives in the low bins
float fftLog(float t){ return texture2D(audioFFT, vec2(pow(t,2.2)*0.5, 0.5)).r; }
```

## Routing table (default assignments)

| Visual parameter | Feature | Depth |
|---|---|---|
| Scale/zoom/warp amount | audioBass (smoothed) | ±15–25% around base (Milkdrop uses 2%!) |
| Flash/pulse/shockwave | audioBeatPulse² or audioPunch^1.5 | event only |
| Detail/turbulence/noise gain | audioMid or audioFlux | ±30% |
| Sparkle/particles/highlights | audioHigh | ±40%, sparse subset only |
| Hue/palette | audioBrightness (slow) or beat-flip | ±0.08 hue; discrete on downbeat |
| Camera/animation speed | audioEnergy via time-warp clock | `musicTime += dt*(0.5+1.2*energy)` |
| Bloom/saturation/contrast | audioEnergy / audioArousal | ±20% |
| Scene/mode switches | audioBeat on downbeats, audioDrop | discrete, eased 80–150ms |
| Mood/temperature | audioValence/audioWarmth | invisible per-frame, section-level |

## Anti-patterns (reject in review)

Raw FFT → param jitter; everything pumping together; strobe every beat;
level-driven hue cycling; linear mapping; binary gates with no ease;
frozen/black frame in silence; identical response in intro vs drop.

## Golden techniques (implementation sketches live in the research corpus)

1. **Kick shockwave rings** — `age = 1.0 - audioBeatPulse`; gaussian ring at
   `radius = mix(0.05, 1.2, age)`, brightness `audioBeatPulse²`, weight by punch.
2. **Per-band cells** — stable hash-assigned band per cell via `fftLog(h)`,
   per-cell lag, bass adds small global lift, highs gate sparse sparkle.
3. **Spectral waterfall** — persistent 256×128 buffer, newest row = FFT,
   scroll+decay; sample as terrain/rings/tunnel displacement.
4. **Feedback warp (Milkdrop)** — persistent full-res buffer; bass → zoom of
   the *velocity field*, mid → swirl, beat re-rolls rotation direction,
   injections max-blend then decay `mix(0.90, 0.985, energy)`.
5. **Time-warp clock** — accumulate `musicTime` scaled by energy; drive the
   whole scene from it so everything breathes with the track.

## Envelope reference (per-frame at 60fps, host side)

kick/onset: attack instant, release 150–250ms · bass: 10–20ms / 250–500ms ·
mids: 15–30ms / 150–300ms · highs: 5–15ms / 80–200ms · energy/mood: 50ms / 1–3s.
AGC: `rel = band / max(longAvg(0.992/frame), 0.02)` — 1.0 means "average for
this track"; beats trigger at >1.3× attenuated average with 120ms refractory.

/** @jsxImportSource smithers-orchestrator */
// Audio-reactivity control map + mobile parity plan.
//
// Phase 1 fans out three read-only researchers: (a) every audio control on
// Easel desktop, (b) the full signal path from analyzer to shader uniforms /
// param bindings, (c) EaselMobile's current audio-related surface and the
// OSC/SDK endpoints it already speaks. Phase 2 synthesizes one control map.
// Phase 3 designs the mobile control set and writes two docs into the repo:
//   docs/AUDIO_CONTROL_MAP.md
//   docs/MOBILE_AUDIO_REACTIVITY_PLAN.md
import { createSmithers } from "smithers-orchestrator";
import { z } from "zod/v4";
import { agents } from "../agents";

const { Workflow, Task, Sequence, Parallel, smithers, outputs } = createSmithers({
  desktopControls: z.object({
    report: z.string().describe("Markdown inventory of every desktop audio control"),
  }),
  signalPath: z.object({
    report: z.string().describe("Markdown map of how audio reaches shaders"),
  }),
  mobileState: z.object({
    report: z.string().describe("Markdown inventory of EaselMobile's current audio surface + OSC/SDK endpoints"),
  }),
  controlMap: z.object({
    docPath: z.string(),
    summary: z.string(),
  }),
  mobilePlan: z.object({
    docPath: z.string(),
    summary: z.string(),
    missingEndpoints: z.array(z.string()),
  }),
});

export default smithers((ctx) => {
  const desktop = ctx.outputMaybe("desktopControls", { nodeId: "desktop-controls" });
  const signal  = ctx.outputMaybe("signalPath", { nodeId: "signal-path" });
  const mobile  = ctx.outputMaybe("mobileState", { nodeId: "mobile-state" });
  const map     = ctx.outputMaybe("controlMap", { nodeId: "control-map" });
  return (
  <Workflow name="audio-reactivity-map">
    <Sequence>
      <Parallel>
        <Task id="desktop-controls" output={outputs.desktopControls} agent={agents.smart}>
          {`You are auditing the Easel desktop app (C++/ImGui) at /Users/lu/easel.
READ-ONLY: do not modify any files.

Inventory EVERY audio control a user can touch, grouped by surface:
1. The AUDIO panel (src/app/Application.cpp — search "AUDIO", "Levels", "Gain",
   "Gate", "Smoothness", "Attack"): input device picker (System Audio loopback
   vs mic, the m_wantsSystemAudio gate in src/app/AudioAnalyzer_mac.mm),
   per-band gains, gate, attack/release smoothing, BPM/tap tempo if present.
2. The PropertyPanel Audio Reactivity section (src/ui/PropertyPanel.cpp —
   audioPresetRow): Reactivity (intensity), Punch (character), On/Shuffle/Off,
   and the per-param sparkle bind popup (signal source list in the AudioSignal
   enum, output range min/max, smoothing, character), the inline range row and
   the live tick markers.
3. AudioPresetEngine (src/sources/AudioPresetEngine.{h,cpp}): recipe model,
   intensity->range scaling, character->conditioner mapping, on/off/shuffle.
4. Per-zone mics (src/app/OutputZone.h micEnabled/pushToTalkActive) and the
   /easel/zone/mic OSC endpoints.
5. The OSC remote surface for all of the above (search "audiopreset",
   "/easel/" in Application.cpp) — list every audio-related OSC address with
   its arguments.

For each control: name, file:line, value range/default, what it changes, and
how it is persisted. Return ONE markdown report as the "report" field.`}
        </Task>
        <Task id="signal-path" output={outputs.signalPath} agent={agents.smart}>
          {`You are mapping the audio->visual signal path in /Users/lu/easel and
/Users/lu/ShaderClaw3. READ-ONLY: do not modify any files.

Trace, with file:line references:
1. Capture: src/app/AudioAnalyzer_mac.mm (ScreenCaptureKit loopback vs CoreAudio
   mic) -> AudioAnalyzer FFT bands, RMS, beat, stems, temperaments (see
   src/app/AudioAnalyzer.h and the EaselAudio docs in /Users/lu/ShaderClaw3/docs
   if present).
2. Uniform bus: ShaderSource::setAudioFeatures / setAudioState in
   src/sources/ShaderSource.cpp (~line 760) — list EVERY audio uniform name a
   shader can read (audioLevel, audioBass, ..., stems, beat phases, brightness).
3. Per-param bindings: ShaderSource::applyAudioBindings (~line 1008),
   AudioBinding::follow in src/sources/AudioBinding.h (smoothing, character,
   range mapping), and the equivalents in FluidSource::applyAudioBindings and
   FluidSource3D.
4. Where Application.cpp feeds them each frame (~line 1500-1670): which
   signals (level/bass/mid/high/beat/energy/build/drop/silence/momentum) and
   how zone mics substitute.
5. How the ShaderClaw3 shaders consume this (the conditioning house pattern in
   /Users/lu/ShaderClaw3/docs/AUDIO_REACTIVITY_PLAYBOOK.md and an example like
   shaders/cascade_text.fs or shaders/fluid3d_prism.fs).

Return ONE markdown report as the "report" field.`}
        </Task>
        <Task id="mobile-state" output={outputs.mobileState} agent={agents.smart}>
          {`You are auditing EaselMobile's audio-reactivity surface. READ-ONLY.

Repos:
- /Users/lu/easel-mobile (SwiftUI app in EaselMobile/; NOTE: local checkout is
  on 'main' but the active dev branch is scratch/james-merge — inspect that
  branch too via 'git -C /Users/lu/easel-mobile log --oneline scratch/james-merge | head'
  and 'git -C /Users/lu/easel-mobile show scratch/james-merge:<path>' for key files.
  Never check out or modify branches.)
- /Users/lu/easel-agent-sdk (Python SDK server on :8765 the phone talks to).

Find:
1. What audio-reactivity controls mobile has TODAY (search for "Reactivity",
   "Punch", "Shuffle", "audiopreset" in Swift sources — e.g.
   EaselMobile/EaselMobile/Features/**). The controls sheet shows
   Reactivity/Punch/Shuffle/Off currently.
2. How mobile reaches the desktop: which SDK endpoints (/api/... in
   easel-agent-sdk/src/easel_agent/server.py) and which OSC addresses the SDK
   relays (actions.py). List every audio-related action available end-to-end
   from the phone.
3. What is MISSING vs the desktop surface: the 'on' audiopreset command,
   per-param bind editing (signal/range/smoothing), live driven-value feedback
   (does any endpoint report AudioBinding.smoothedValue?), audio input device
   selection, per-band gains/gate, zone mic push-to-talk, the native Fluid /
   Fluid3D sources (are they controllable from mobile at all?).

Return ONE markdown report as the "report" field.`}
        </Task>
      </Parallel>
      <Task id="control-map" output={outputs.controlMap} agent={agents.smart}>
        {`Synthesize these three research reports into ONE document and
WRITE it to /Users/lu/easel/docs/AUDIO_CONTROL_MAP.md (create docs/ if needed).

Structure: 1) Signal path diagram (capture -> analyzer -> features -> uniforms
+ bindings -> shader), 2) Control inventory table (control, surface, range,
default, OSC address if any, file:line), 3) Mobile's current reach, 4) Gaps.

=== DESKTOP CONTROLS ===
${desktop?.report ?? ""}

=== SIGNAL PATH ===
${signal?.report ?? ""}

=== MOBILE STATE ===
${mobile?.report ?? ""}

Return docPath and a 10-line summary.`}
      </Task>
      <Task id="mobile-plan" output={outputs.mobilePlan} agent={agents.smart}>
        {`Design the mobile audio-reactivity control set and WRITE the
plan to /Users/lu/easel/docs/MOBILE_AUDIO_REACTIVITY_PLAN.md.

Inputs: the control map at ${map?.docPath ?? "/Users/lu/easel/docs/AUDIO_CONTROL_MAP.md"}
(read it), plus this context:
- Mobile design language: the existing controls sheet (Reactivity/Punch/
  Shuffle/Off, tall pill sliders, monochrome). Desktop was just restyled to
  match it, including an On/Shuffle/Off row with a lit On state and semantic
  words (Subtle/Medium/Intense, Smooth/Classic/Chopped).
- Desktop just gained: AudioPresetEngine::on() + OSC
  /easel/layer/audiopreset <key> on, live tone ticks on bound sliders,
  /easel/layer/allzones <idx> [solo] to push one look house-wide.

The plan must specify:
1. The mobile control set, tiered: Tier 1 (ship first): On/Shuffle/Off +
   Reactivity + Punch per layer (wire the new 'on' command), an "Everywhere"
   button (allzones solo). Tier 2: per-param bind list with signal picker +
   range editing. Tier 3: live driven-value feedback on sliders (needs a new
   SDK/OSC feedback channel — design it), audio input picker, zone mic PTT.
2. For every control: the exact OSC/SDK endpoint it uses; mark endpoints that
   do not exist yet with a concrete proposal (address, args, which C++ file
   handles it).
3. SwiftUI surface: where each control lives in the existing app structure
   (ShaderParamsSheet / ZonesView), matching the current sheet's design.
4. A milestone list (M1..Mn) with verification steps per milestone.

Return docPath, a 10-line summary, and missingEndpoints (list of OSC/SDK
endpoints that must be built).`}
      </Task>
    </Sequence>
  </Workflow>
  );
});

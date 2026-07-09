#pragma once
#include "app/EaselAudio.h"
#include <cmath>

// Shared audio/MIDI parameter-binding types. Originally defined inside
// ShaderSource.h; factored out so non-shader sources (FluidSource, …) can
// reuse the exact same binding model + follower math behind the "sparkle"
// bind affordance in the PropertyPanel.

// Signal sources for parameter binding
enum class AudioSignal {
    None = 0,
    Level,   // RMS
    Bass,
    Mid,
    High,
    Beat,    // beat decay (0-1 pulse)
    MidiCC,  // MIDI control change (uses midiCC/midiChannel fields)
    // ── "Listening" signals — time-aware musical dynamics, not instantaneous
    // level. Sourced from the AudioAnalyzer's structure layer so the visuals
    // follow the SONG's arc (highs, lows, builds, drops, pauses) instead of
    // just twitching to the current loudness. APPENDED after MidiCC so the
    // serialized int values of the original signals never shift.
    Energy,    // slow arrangement altitude — the high<->low journey
    Build,     // riser / build-up progress — anticipation before a peak
    Drop,      // structural impact impulse — fires when a drop lands
    Silence,   // rises when the track goes quiet (pauses / breakdowns)
    Momentum,  // energy rising(>0.5) vs falling(<0.5), remapped to 0..1
};

// Per-parameter audio/MIDI binding
struct AudioBinding {
    AudioSignal signal = AudioSignal::None;
    float rangeMin = 0.0f;  // output min (maps to param min by default)
    float rangeMax = 1.0f;  // output max (maps to param max by default)
    // 0 = instant (snappy), 1 = very slow (heavy glide). Default 0.85 keeps new
    // bindings calm by default (was 0.7, then 0.55 — both still felt too
    // fast/strobey the moment audio is enabled). Users wanting punch drag it
    // down; the common case is "make it smooth," so start there.
    float smoothing = 0.85f;
    // EaselAudio character: -1 = extra smooth … +1 = spiky/chopped.
    // 0 = NEUTRAL = the exact legacy feel (existing projects load with 0, so
    // nothing changes until the user touches the new Character control).
    float character = 0.0f;
    float smoothedValue = 0.0f; // internal follower state (0-1, pre-range-map)
    bool  hasSmoothed  = false; // false until first sample (avoids 0 ramp-in)
    // MIDI fields (used when signal == MidiCC)
    int midiCC = -1;        // CC number 0-127, -1 = unassigned
    int midiChannel = -1;   // MIDI channel 0-15, -1 = any

    // Shared EaselAudio conditioning block (gate→attack→hold→release→
    // hard-change→character→remap→micro-slew). The legacy smoothing slider
    // maps onto its attack/release taus with the SAME rate curve as the old
    // inline follower, so a binding with character 0 behaves identically.
    easelaudio::Conditioner cond;

    // Frame-rate-independent asymmetric follower (punchy attack, softer
    // release), then map the conditioned 0..1 value onto [rangeMin, rangeMax].
    // `raw` is the 0..1 signal sample; returns the mapped output value (the
    // caller clamps it to the destination parameter's own range).
    float follow(float raw, float dt) {
        if (!(dt > 0.0f)) dt = 1.0f / 60.0f;
        if (dt > 0.1f)    dt = 0.1f;

        // Legacy smoothing-slider → rate mapping (fast ends roughly halved
        // vs the original 28/14 so even a snappy binding glides rather than
        // strobes — the "way too fast" fix). Unchanged math, now expressed
        // as the conditioning block's attack/release taus.
        constexpr float kAttackFast  = 14.0f, kAttackSlow  = 1.5f;
        constexpr float kReleaseFast = 7.0f,  kReleaseSlow = 0.7f;
        float s = smoothing;
        if (s < 0.0f) s = 0.0f; else if (s > 1.0f) s = 1.0f;
        cond.p.setRates(kAttackFast  + (kAttackSlow  - kAttackFast)  * s,
                        kReleaseFast + (kReleaseSlow - kReleaseFast) * s);
        cond.p.character = character;

        if (!hasSmoothed) { cond.reset(); hasSmoothed = true; }
        smoothedValue = cond.process(raw, dt);

        return rangeMin + smoothedValue * (rangeMax - rangeMin);
    }
};

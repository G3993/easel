#pragma once
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
};

// Per-parameter audio/MIDI binding
struct AudioBinding {
    AudioSignal signal = AudioSignal::None;
    float rangeMin = 0.0f;  // output min (maps to param min by default)
    float rangeMax = 1.0f;  // output max (maps to param max by default)
    // 0 = instant (snappy), 1 = very slow (heavy glide). Default 0.55 is a
    // deliberately gentler envelope than the legacy fixed attack-8 / release-3
    // feel, which the user found too aggressive / strobey.
    float smoothing = 0.55f;
    float smoothedValue = 0.0f; // internal follower state
    bool  hasSmoothed  = false; // false until first sample (avoids 0 ramp-in)
    // MIDI fields (used when signal == MidiCC)
    int midiCC = -1;        // CC number 0-127, -1 = unassigned
    int midiChannel = -1;   // MIDI channel 0-15, -1 = any

    // Frame-rate-independent asymmetric follower (punchy attack, softer
    // release), then map the smoothed 0..1 value onto [rangeMin, rangeMax].
    // `raw` is the 0..1 signal sample; returns the mapped output value (the
    // caller clamps it to the destination parameter's own range). Identical
    // math to the original ShaderSource::applyAudioBindings inline follower.
    float follow(float raw, float dt) {
        if (!(dt > 0.0f)) dt = 1.0f / 60.0f;
        if (dt > 0.1f)    dt = 0.1f;
        if (!hasSmoothed) { smoothedValue = raw; hasSmoothed = true; }

        constexpr float kAttackFast  = 28.0f, kAttackSlow  = 2.0f;
        constexpr float kReleaseFast = 14.0f, kReleaseSlow = 0.9f;
        float s = smoothing;
        if (s < 0.0f) s = 0.0f; else if (s > 1.0f) s = 1.0f;
        float attackRate  = kAttackFast  + (kAttackSlow  - kAttackFast)  * s;
        float releaseRate = kReleaseFast + (kReleaseSlow - kReleaseFast) * s;
        float rate  = (raw > smoothedValue) ? attackRate : releaseRate;
        float alpha = 1.0f - std::exp(-rate * dt);
        smoothedValue += (raw - smoothedValue) * alpha;

        return rangeMin + smoothedValue * (rangeMax - rangeMin);
    }
};

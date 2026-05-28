#pragma once
#include <string>

enum class EffectType {
    Blur = 0,
    ColorAdjust,   // brightness, contrast, saturation, hue
    Invert,
    Pixelate,
    Feedback,      // trails / echo
    Glow,          // threshold + blur + additive blend
    Sharpen,       // unsharp mask — restores crispness / fixes fuzzy text
    COUNT
};

inline const char* effectTypeName(EffectType t) {
    switch (t) {
        case EffectType::Blur: return "Blur";
        case EffectType::ColorAdjust: return "Color";
        case EffectType::Invert: return "Invert";
        case EffectType::Pixelate: return "Pixelate";
        case EffectType::Feedback: return "Feedback";
        case EffectType::Glow: return "Glow";
        case EffectType::Sharpen: return "Sharpen";
        default: return "Unknown";
    }
}

// Effects that expose a per-effect mic/audio modulator (band selector +
// amount). These are the "movement / depth / fidelity" effects whose primary
// parameter is meant to pulse with the music. Existing color/utility effects
// stay static.
inline bool effectSupportsAudio(EffectType t) {
    return t == EffectType::Sharpen;
}

struct LayerEffect {
    EffectType type = EffectType::Blur;
    bool enabled = true;

    // Blur
    float blurRadius = 4.0f;    // 0-20

    // Color adjust
    float brightness = 0.0f;    // -1 to 1
    float contrast = 0.0f;      // -1 to 1
    float saturation = 0.0f;    // -1 to 1
    float hueShift = 0.0f;      // 0-360

    // Pixelate
    float pixelSize = 8.0f;     // 1-64

    // Feedback
    float feedbackMix = 0.8f;   // 0-1
    float feedbackZoom = 1.01f;  // 0.95-1.1

    // Glow
    float glowThreshold = 0.6f; // brightness threshold (0-1)
    float glowRadius = 8.0f;    // blur radius
    float glowIntensity = 1.0f; // additive strength (0-3)

    // Sharpen (unsharp mask) — boosts high-frequency detail so low-fi /
    // up-scaled text reads crisp instead of fuzzy.
    float sharpenAmount = 1.0f; // 0-3 high-pass boost
    float sharpenRadius = 1.5f; // 0.5-4 texel radius of the soft reference

    // Per-effect audio modulation (only used when effectSupportsAudio(type)).
    // The effect's primary parameter pulses with the chosen mic/loopback band:
    // modulated = base + audioAmount * band. Reuses CompositeEngine::m_audio.
    int   audioSignal = -1;     // -1=off, 0=bass, 1=mid, 2=treble, 3=beat
    float audioAmount = 0.0f;   // modulation depth (0-1, scaled per effect)
};

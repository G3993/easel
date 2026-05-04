#pragma once
#include <string>
#include <vector>

// V1 voice intent vocabulary. Deterministic — keyword/regex match, no LLM.
// Adding a new intent kind is a one-line case in parse() + a handler.
//
// Vocabulary (V1.0):
//   "play" / "pause" / "stop" / "loop"
//   "go to N seconds" / "skip [forward/back] N"
//   "fade in <name> over N seconds"
//   "fade out <name> over N seconds"
//   "set <name> opacity to N percent"
//   "show <name>" / "hide <name>"
//   "transition to <name> with <transition> [over N seconds]"   (v1.1)
//   "add <shader name>"                                          (v1.1)

namespace easel::voice {

enum class IntentKind {
    Unknown,
    Play, Pause, Stop, ToggleLoop,
    Seek,            // value = absolute seconds
    Skip,            // value = relative seconds (signed)
    FadeIn,          // target = layer name; value = duration seconds
    FadeOut,         // target = layer name; value = duration seconds
    SetOpacity,      // target = layer name; value = 0..1
    Show, Hide,      // target = layer name
    AddShader,       // target = shader name (matched against ShaderClaw library)
    PickTransition,  // target = transition name
};

struct VoiceIntent {
    IntentKind kind = IntentKind::Unknown;
    std::string target;      // layer name / shader name / transition name
    std::string secondary;   // e.g. transition name when kind == PickTransition for a follow-up
    float value = 0.0f;      // seconds, percent, etc.
    std::string raw;         // original transcript (for log/echo)
};

// Returns Unknown intent when no match. Case-insensitive.
VoiceIntent parse(const std::string& transcript);

// Human-readable name of the intent (for UI echo).
const char* intentName(IntentKind k);

// Pretty-print an intent in the form a user would recognize as "what you said".
std::string formatIntent(const VoiceIntent& i);

} // namespace easel::voice

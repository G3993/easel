#include "voice/VoiceCommand.h"

#include <algorithm>
#include <cctype>
#include <regex>
#include <sstream>

namespace easel::voice {

namespace {

std::string toLower(std::string s) {
    std::transform(s.begin(), s.end(), s.begin(),
                   [](unsigned char c) { return (char)std::tolower(c); });
    return s;
}

std::string trim(const std::string& s) {
    size_t a = 0, b = s.size();
    while (a < b && std::isspace((unsigned char)s[a])) a++;
    while (b > a && std::isspace((unsigned char)s[b-1])) b--;
    return s.substr(a, b - a);
}

// Parse a number that may be a digit ("3", "3.5") or a small word ("three").
bool parseNumber(const std::string& tok, float& out) {
    if (tok.empty()) return false;
    if (std::isdigit((unsigned char)tok[0]) ||
        (tok[0] == '.' && tok.size() > 1 && std::isdigit((unsigned char)tok[1]))) {
        try { out = std::stof(tok); return true; } catch (...) { return false; }
    }
    static const struct { const char* w; float v; } words[] = {
        {"zero",0},{"one",1},{"two",2},{"three",3},{"four",4},{"five",5},
        {"six",6},{"seven",7},{"eight",8},{"nine",9},{"ten",10},
        {"eleven",11},{"twelve",12},{"fifteen",15},{"twenty",20},{"thirty",30},
        {"sixty",60},{"half",0.5f},{"a half", 0.5f}
    };
    for (auto& w : words) if (tok == w.w) { out = w.v; return true; }
    return false;
}

// Pull "over N seconds" / "in N seconds" / "for N seconds" and return seconds
// (default 1.5 if absent). Strips the matched phrase from `s` so the remainder
// can be searched for the target name.
float consumeDuration(std::string& s, float defaultSec = 1.5f) {
    std::regex pat(R"((?:over|in|for|across)\s+([0-9.]+|one|two|three|four|five|six|seven|eight|nine|ten|fifteen|twenty)\s*(?:seconds?|secs?|s)?)");
    std::smatch m;
    if (std::regex_search(s, m, pat)) {
        float v = defaultSec;
        if (parseNumber(m[1].str(), v)) {
            s = m.prefix().str() + m.suffix().str();
            return v;
        }
    }
    return defaultSec;
}

// Pull "to N percent" / "to N%" → 0..1
bool consumePercent(std::string& s, float& out) {
    std::regex pat(R"(to\s+([0-9.]+)\s*(?:percent|%))");
    std::smatch m;
    if (std::regex_search(s, m, pat)) {
        try {
            out = std::stof(m[1].str()) / 100.0f;
            s = m.prefix().str() + m.suffix().str();
            return true;
        } catch (...) {}
    }
    return false;
}

// After a leading verb, the "rest" is treated as the target name. Strips
// filler words ("the", "a", "an", "layer", "shader") so "the magritte layer"
// becomes "magritte".
std::string extractTarget(const std::string& s) {
    std::string t = trim(s);
    static const std::vector<std::string> fillers = {
        "the ", "a ", "an ", "layer ", "shader ", "named ", "called "
    };
    bool changed = true;
    while (changed) {
        changed = false;
        for (const auto& f : fillers) {
            if (t.size() >= f.size() &&
                std::equal(f.begin(), f.end(), t.begin())) {
                t = t.substr(f.size());
                changed = true;
                break;
            }
        }
    }
    // Strip trailing fillers too (" layer", " shader")
    static const std::vector<std::string> trail = { " layer", " shader" };
    for (const auto& f : trail) {
        if (t.size() >= f.size() &&
            std::equal(f.rbegin(), f.rend(), t.rbegin())) {
            t = t.substr(0, t.size() - f.size());
        }
    }
    return trim(t);
}

bool startsWith(const std::string& s, const std::string& p) {
    return s.size() >= p.size() && std::equal(p.begin(), p.end(), s.begin());
}

} // anon

const char* intentName(IntentKind k) {
    switch (k) {
        case IntentKind::Play:            return "Play";
        case IntentKind::Pause:           return "Pause";
        case IntentKind::Stop:            return "Stop";
        case IntentKind::ToggleLoop:      return "Toggle loop";
        case IntentKind::Seek:            return "Seek";
        case IntentKind::Skip:            return "Skip";
        case IntentKind::FadeIn:          return "Fade in";
        case IntentKind::FadeOut:         return "Fade out";
        case IntentKind::SetOpacity:      return "Set opacity";
        case IntentKind::Show:            return "Show";
        case IntentKind::Hide:            return "Hide";
        case IntentKind::AddShader:       return "Add shader";
        case IntentKind::PickTransition:  return "Pick transition";
        case IntentKind::Unknown:
        default:                          return "Unknown";
    }
}

std::string formatIntent(const VoiceIntent& i) {
    std::ostringstream os;
    os << intentName(i.kind);
    if (!i.target.empty()) os << " · " << i.target;
    if (!i.secondary.empty()) os << " (" << i.secondary << ")";
    if (i.kind == IntentKind::FadeIn || i.kind == IntentKind::FadeOut)
        os << " · " << i.value << "s";
    if (i.kind == IntentKind::Seek) os << " → " << i.value << "s";
    if (i.kind == IntentKind::Skip) os << " " << (i.value >= 0 ? "+" : "") << i.value << "s";
    if (i.kind == IntentKind::SetOpacity)
        os << " → " << (int)(i.value * 100.0f) << "%";
    return os.str();
}

VoiceIntent parse(const std::string& transcript) {
    VoiceIntent out;
    out.raw = transcript;

    std::string t = toLower(trim(transcript));
    if (t.empty()) return out;

    // Strip trailing punctuation that speech-to-text often appends.
    while (!t.empty() && (t.back() == '.' || t.back() == '!' ||
                          t.back() == '?' || t.back() == ',')) {
        t.pop_back();
    }

    // ── Transport (one-word commands first) ──
    if (t == "play")                      { out.kind = IntentKind::Play;        return out; }
    if (t == "pause")                     { out.kind = IntentKind::Pause;       return out; }
    if (t == "stop")                      { out.kind = IntentKind::Stop;        return out; }
    if (t == "loop" || t == "toggle loop"){ out.kind = IntentKind::ToggleLoop;  return out; }

    // ── Seek ──
    {
        std::regex pat(R"(^(?:go to|jump to|seek to|seek)\s+([0-9.]+)\s*(?:seconds?|secs?|s)?)");
        std::smatch m;
        if (std::regex_search(t, m, pat)) {
            out.kind = IntentKind::Seek;
            try { out.value = std::stof(m[1].str()); } catch (...) {}
            return out;
        }
    }
    {
        std::regex pat(R"(^skip\s+(forward|back|backward|backwards)?\s*([0-9.]+))");
        std::smatch m;
        if (std::regex_search(t, m, pat)) {
            out.kind = IntentKind::Skip;
            float v = 0;
            try { v = std::stof(m[2].str()); } catch (...) {}
            std::string dir = m[1].str();
            if (dir.find("back") != std::string::npos) v = -v;
            out.value = v;
            return out;
        }
    }

    // ── Fade in/out ──
    {
        std::regex pat(R"(^fade\s+(in|out)\s+(.+?)$)");
        std::smatch m;
        if (std::regex_search(t, m, pat)) {
            std::string body = m[2].str();
            float dur = consumeDuration(body, 1.5f);
            out.kind   = (m[1].str() == "in") ? IntentKind::FadeIn : IntentKind::FadeOut;
            out.target = extractTarget(body);
            out.value  = dur;
            return out;
        }
    }

    // ── Set opacity ──
    {
        std::regex pat(R"(^set\s+(.+?)\s+opacity\s+(.+))");
        std::smatch m;
        if (std::regex_search(t, m, pat)) {
            std::string body = m[2].str();
            float pct = 0.5f;
            if (consumePercent(body, pct) || parseNumber(trim(body), pct)) {
                if (pct > 1.0f) pct /= 100.0f;
                out.kind = IntentKind::SetOpacity;
                out.target = extractTarget(m[1].str());
                out.value = pct;
                return out;
            }
        }
    }

    // ── Show / Hide ──
    if (startsWith(t, "show ")) {
        out.kind = IntentKind::Show;
        out.target = extractTarget(t.substr(5));
        return out;
    }
    if (startsWith(t, "hide ")) {
        out.kind = IntentKind::Hide;
        out.target = extractTarget(t.substr(5));
        return out;
    }

    // ── Add (V1.1 — rough recognition; dispatcher decides if it can resolve) ──
    if (startsWith(t, "add ")) {
        out.kind = IntentKind::AddShader;
        out.target = extractTarget(t.substr(4));
        return out;
    }

    // ── Transition to <name> [with <transition>] [over Ns] ──
    {
        std::regex pat(R"(^(?:transition|go|cut)\s+to\s+(.+?)(?:\s+(?:with|using)\s+(.+?))?$)");
        std::smatch m;
        if (std::regex_search(t, m, pat)) {
            out.kind = IntentKind::PickTransition;
            std::string body1 = m[1].str();
            std::string body2 = m[2].str();
            float dur = consumeDuration(body2, 0.0f);
            if (dur > 0.0f) out.value = dur;
            else            out.value = consumeDuration(body1, 1.5f);
            out.target    = extractTarget(body1);
            out.secondary = trim(body2);
            return out;
        }
    }

    return out;
}

} // namespace easel::voice

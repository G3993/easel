#include "speech/EthereaSdkClient.h"
#include "net/SdkHttp.h"

#include <nlohmann/json.hpp>

#include <chrono>
#include <cmath>
#include <cstdlib>
#include <fstream>
#include <iostream>
#include <vector>
#ifndef _WIN32
#include <sys/stat.h>
#endif

using json = nlohmann::json;

namespace {

constexpr double kWriteInterval   = 0.1;   // ~10 sends/s while dragging
constexpr double kPollInterval    = 1.0;   // status read cadence while visible
constexpr double kVisibleGrace    = 1.5;   // panel counts as open this long after a heartbeat
constexpr double kStatusTimeout   = 6.0;
constexpr double kWriteTimeout    = 8.0;
constexpr double kPromptTimeout   = 30.0;  // same as Paseo

double nowSec() {
    using namespace std::chrono;
    return duration<double>(steady_clock::now().time_since_epoch()).count();
}

std::string keyPath() {
#ifdef _WIN32
    const char* home = getenv("USERPROFILE");
#else
    const char* home = getenv("HOME");
#endif
    return std::string(home ? home : ".") + "/.easel/etherea_sdk_key";
}

std::string trim(std::string s) {
    while (!s.empty() && (s.back() == '\n' || s.back() == '\r' || s.back() == ' ' || s.back() == '\t')) s.pop_back();
    size_t i = 0;
    while (i < s.size() && (s[i] == ' ' || s[i] == '\t')) i++;
    return s.substr(i);
}

// Outcome of one /api/actions call: HTTP status + SDK envelope {ok,data,error}.
struct ActionResult {
    SdkHttpResponse http;
    bool parsed = false;
    bool ok = false;
    json data;
    std::string errCode, errMsg;
    bool authFailed() const { return http.status == 401; }
    bool transportFailed() const { return http.status == 0; }
    std::string describe() const {
        if (http.timedOut) return "timed out";
        if (transportFailed()) return http.error.empty() ? "no response" : http.error;
        if (authFailed()) return "SDK key rejected (401)";
        if (http.status == 429) return "locked out after failed keys (429)";
        if (!errCode.empty() || !errMsg.empty())
            return errCode + (errMsg.empty() ? "" : (errCode.empty() ? "" : ": ") + errMsg);
        if (!parsed) return "HTTP " + std::to_string(http.status) + " (not JSON)";
        return "HTTP " + std::to_string(http.status);
    }
};

ActionResult callAction(const std::string& base, const std::string& key, const std::string& action,
                        const json& payload, bool houseAct, double timeout) {
    std::vector<std::pair<std::string, std::string>> headers;
    if (!key.empty()) headers.emplace_back("X-Easel-Agent-Key", key);
    if (houseAct) headers.emplace_back("X-Easel-House-Act", "1");
    ActionResult r;
    r.http = sdkHttpPostJson(base + "/api/actions/" + action, payload.dump(), headers, timeout);
    if (r.http.status == 0) return r;
    json env = json::parse(r.http.body, nullptr, false);
    if (env.is_discarded() || !env.is_object()) return r;
    r.parsed = true;
    r.ok = env.value("ok", false) && r.http.status >= 200 && r.http.status < 300;
    if (env.contains("data")) r.data = env["data"];
    if (env.contains("error")) {
        const json& e = env["error"];
        if (e.is_object()) {
            r.errCode = e.value("code", std::string());
            r.errMsg  = e.value("message", std::string());
        } else if (e.is_string()) {
            r.errMsg = e.get<std::string>();
        }
    }
    return r;
}

bool getNum(const json& o, const char* k, double& out) {
    if (!o.is_object() || !o.contains(k) || !o[k].is_number()) return false;
    out = o[k].get<double>();
    return std::isfinite(out);
}

// Readback from a strength object {target, value, revision, settled}.
void readStrength(const json& st, EthereaSdkClient::Snapshot& s) {
    if (st.is_number()) { s.hasTarget = true; s.target = st.get<double>(); return; }
    if (!st.is_object()) return;
    s.hasTarget = getNum(st, "target", s.target);
    s.hasValue  = getNum(st, "value", s.value);
    double rev;
    if (getNum(st, "revision", rev)) s.revision = (long long)rev;
    if (st.contains("settled") && st["settled"].is_boolean()) {
        s.hasSettled = true;
        s.settled = st["settled"].get<bool>();
    }
}

// Port of Paseo's cueDecision()/cueRefusal(): what did Cue do with the words?
// data.results[].toolResults[] — a successful video.* tool can carry the
// generated prompt in actions[].payload.prompt. Words accepted with no scene
// action are NOT proof the image changed.
void judgeCue(const json& data, std::string& kind, std::string& text, std::string& applied) {
    std::vector<const json*> results;
    if (data.is_object() && data.contains("results") && data["results"].is_array())
        for (const auto& r : data["results"]) results.push_back(&r);
    else if (data.is_object() && data.contains("toolResults"))
        results.push_back(&data);

    std::string refusal, otherTools;
    for (const json* r : results) {
        if (!r->is_object()) continue;
        if (r->contains("toolResults") && (*r)["toolResults"].is_array()) {
            for (const auto& tr : (*r)["toolResults"]) {
                if (!tr.is_object()) continue;
                std::string tool = tr.value("tool", std::string());
                std::string st   = tr.value("status", std::string());
                if (st == "ok" && tool.rfind("video.", 0) == 0) {
                    if (tr.contains("actions") && tr["actions"].is_array())
                        for (const auto& a : tr["actions"]) {
                            if (a.is_object() && a.contains("payload") && a["payload"].is_object()) {
                                const json& p = a["payload"];
                                if (p.contains("prompt") && p["prompt"].is_string() && applied.empty())
                                    applied = p["prompt"].get<std::string>();
                            }
                        }
                    if (applied.empty()) otherTools += (otherTools.empty() ? "" : ", ") + tool;
                } else if (st == "rejected" || st == "error") {
                    std::string why = tr.value("error", std::string());
                    if (why.empty() && tr.contains("data") && tr["data"].is_object())
                        why = tr["data"].value("reason", std::string());
                    if (!refusal.empty()) refusal += "; ";
                    refusal += tool + " " + st + (why.empty() ? "" : (": " + why));
                } else if (!tool.empty()) {
                    otherTools += (otherTools.empty() ? "" : ", ") + tool;
                }
            }
        }
        // Explicit refusal events, if the cue emits them.
        if (r->contains("events") && (*r)["events"].is_array()) {
            for (const auto& ev : (*r)["events"]) {
                if (!ev.is_object()) continue;
                std::string t = ev.value("type", std::string());
                if (t.find("refus") == std::string::npos) continue;
                std::string why;
                if (ev.contains("payload") && ev["payload"].is_object())
                    why = ev["payload"].value("reason", ev["payload"].value("message", std::string()));
                if (!refusal.empty()) refusal += "; ";
                refusal += why.empty() ? t : why;
            }
        }
    }

    if (!applied.empty()) {
        kind = "changed";
        text = "Cue updated the scene.";
    } else if (!refusal.empty()) {
        kind = "refused";
        text = "Cue declined: " + refusal;
    } else if (!otherTools.empty()) {
        kind = "heard";
        text = "Cue heard it but chose no scene change (" + otherTools + ").";
    } else {
        kind = "accepted";
        text = "Words accepted — no scene action yet, so the image may not have changed.";
    }
}

} // namespace

struct EthereaSdkClient::PromptState {
    std::mutex mx;
    bool inFlight = false;
    uint64_t seq = 0;
    std::string kind, text, applied;
    bool clearDraft = false;
};

EthereaSdkClient::EthereaSdkClient() : m_prompt(std::make_shared<PromptState>()) {}
EthereaSdkClient::~EthereaSdkClient() { stop(); }

void EthereaSdkClient::start() {
    if (m_running.exchange(true)) return;
    m_worker = std::thread([this] { workerLoop(); });
}

void EthereaSdkClient::stop() {
    if (!m_running.exchange(false)) return;
    m_cv.notify_all();
    if (m_worker.joinable()) m_worker.join();
    // A prompt thread, if any, owns only the shared PromptState — it is
    // detached and finishes on its own.
}

std::string EthereaSdkClient::loadKey() {
    if (const char* env = getenv("EASEL_AGENT_API_KEY")) {
        std::string k = trim(env);
        if (!k.empty()) return k;
    }
    std::ifstream in(keyPath());
    std::string line;
    if (in.is_open()) std::getline(in, line);
    return trim(line);
}

bool EthereaSdkClient::saveKey(const std::string& key) {
    std::string path = keyPath();
    {
        std::ofstream out(path, std::ios::trunc);
        if (!out.is_open()) return false;
        out << trim(key) << "\n";
    }
#ifndef _WIN32
    chmod(path.c_str(), 0600);
#endif
    return true;
}

void EthereaSdkClient::setConnection(const std::string& baseUrl, const std::string& key) {
    std::lock_guard<std::mutex> lk(m_mx);
    std::string url = trim(baseUrl);
    while (!url.empty() && url.back() == '/') url.pop_back();
    m_baseUrl = url;
    m_key = trim(key);
    m_connGen++;
    // Drop every queued write and the old status: the next read decides.
    m_hasPending = false;
    m_dragging = false;
    m_canceled = true;
    m_holdUntilPollSeq = 0;
    m_needFreshStatus = true;
    Snapshot fresh;
    fresh.baseUrl = m_baseUrl;
    fresh.hasKey = !m_key.empty();
    m_s = fresh;
    // Read only if the panel is open; otherwise its next heartbeat does it.
    // (Status reads are POSTs — with a bad key they count toward the lockout.)
    m_refreshRequested = nowSec() - m_lastHeartbeat <= kVisibleGrace;
    m_cv.notify_all();
}

void EthereaSdkClient::heartbeat() {
    std::lock_guard<std::mutex> lk(m_mx);
    double t = nowSec();
    bool wasHidden = t - m_lastHeartbeat > kVisibleGrace;
    m_lastHeartbeat = t;
    if (wasHidden) { m_refreshRequested = true; m_cv.notify_all(); }
}

void EthereaSdkClient::requestRefresh() {
    std::lock_guard<std::mutex> lk(m_mx);
    m_s.authFailed = false;
    m_refreshRequested = true;
    m_cv.notify_all();
}

bool EthereaSdkClient::beginGesture() {
    std::lock_guard<std::mutex> lk(m_mx);
    bool enabled = m_s.statusLoaded && m_s.reachable && !m_s.authFailed && m_s.supportsStrength
                   && m_s.available && !m_needFreshStatus && !m_s.provider.empty() && !m_inFlight;
    if (!enabled) return false;
    m_gestureId++;
    m_gestureProvider = m_s.provider;   // guards against a model change mid-gesture
    m_dragging = true;
    m_canceled = false;
    m_hasPending = false;
    m_s.writeMsg.clear();
    m_s.writeError = false;
    m_s.localValue = m_s.hasTarget ? m_s.target : 0.0;
    return true;
}

void EthereaSdkClient::setGestureValue(double wire) {
    std::lock_guard<std::mutex> lk(m_mx);
    if (!m_dragging || m_canceled || !std::isfinite(wire)) return;
    if (wire < 0) wire = 0;
    if (wire > 1) wire = 1;
    m_pendingValue = wire;     // replaceable latest value
    m_hasPending = true;
    m_s.localValue = wire;
    m_cv.notify_all();
}

void EthereaSdkClient::endGesture() {
    std::lock_guard<std::mutex> lk(m_mx);
    if (!m_dragging) return;
    m_dragging = false;
    // Any pending value is the release value; the worker delivers it after
    // the in-flight write. If nothing is outstanding, wait for one fresh read.
    if (!m_hasPending && !m_inFlight) {
        m_holdUntilPollSeq = m_pollSeqStarted + 1;
        m_refreshRequested = true;
    }
    m_cv.notify_all();
}

void EthereaSdkClient::cancelGestureLocked(const std::string& why, bool error) {
    m_hasPending = false;
    m_dragging = false;
    m_canceled = true;
    m_holdUntilPollSeq = 0;
    m_needFreshStatus = true;
    m_refreshRequested = true;
    m_s.writeMsg = why;
    m_s.writeError = error;
}

EthereaSdkClient::Snapshot EthereaSdkClient::snapshot() const {
    Snapshot s;
    {
        std::lock_guard<std::mutex> lk(m_mx);
        s = m_s;
        s.statusAgeSec = m_s.statusLoaded ? nowSec() - m_statusTime : -1;
        s.inputEnabled = s.statusLoaded && s.reachable && !s.authFailed && s.supportsStrength
                         && s.available && !m_needFreshStatus && !s.provider.empty();
        s.gestureLocal = m_dragging || m_hasPending || m_inFlight
                         || (m_holdUntilPollSeq != 0 && m_pollSeqDone < m_holdUntilPollSeq);
    }
    {
        std::lock_guard<std::mutex> lk(m_prompt->mx);
        s.promptInFlight = m_prompt->inFlight;
        s.promptSeq = m_prompt->seq;
        s.promptKind = m_prompt->kind;
        s.promptText = m_prompt->text;
        s.appliedPrompt = m_prompt->applied;
        s.clearDraft = m_prompt->clearDraft;
    }
    return s;
}

void EthereaSdkClient::workerLoop() {
    while (m_running.load()) {
        enum { None, Write, Poll } job = None;
        double value = 0;
        std::string provider;
        uint64_t gid = 0;
        {
            std::unique_lock<std::mutex> lk(m_mx);
            m_cv.wait_for(lk, std::chrono::milliseconds(20));
            if (!m_running.load()) break;
            double t = nowSec();
            bool configured = !m_baseUrl.empty() && !m_key.empty();
            bool visible = t - m_lastHeartbeat <= kVisibleGrace;
            if (configured && !m_s.authFailed && m_hasPending && !m_inFlight && !m_canceled
                && t - m_lastSend >= kWriteInterval) {
                job = Write;
                value = m_pendingValue;
                provider = m_gestureProvider;
                gid = m_gestureId;
                m_hasPending = false;
                m_inFlight = true;
                m_lastSend = t;
            } else if (configured && !m_inFlight && !m_s.authFailed
                       && (m_refreshRequested || (visible && t - m_lastPollDone >= kPollInterval))) {
                job = Poll;
                m_refreshRequested = false;
            }
        }
        if (job == Write) doWrite(value, provider, gid);
        else if (job == Poll) doPoll();
    }
}

void EthereaSdkClient::doWrite(double value, const std::string& provider, uint64_t gid) {
    std::string base, key;
    uint64_t gen;
    {
        std::lock_guard<std::mutex> lk(m_mx);
        base = m_baseUrl; key = m_key; gen = m_connGen;
    }
    json payload = {{"provider", provider}, {"value", value}};
    ActionResult r = callAction(base, key, "model.primary.strength.set", payload, /*houseAct=*/true, kWriteTimeout);

    std::lock_guard<std::mutex> lk(m_mx);
    m_inFlight = false;
    if (gen != m_connGen) return;   // connection changed underneath; already cleared
    bool current = (gid == m_gestureId) && !m_canceled;

    if (r.authFailed()) {
        m_s.authFailed = true;      // stop: failed writes count toward the SDK lockout
        cancelGestureLocked("SDK key rejected — fix the key, then Refresh", true);
        return;
    }
    if (r.transportFailed()) {
        m_s.reachable = false;
        m_s.connError = r.describe();
        cancelGestureLocked("Write failed (" + r.describe() + ") — reconnecting", true);
        return;
    }
    if (!r.ok) {
        // primary_changed / primary_busy: the model moved under the gesture.
        // Never replay against another model; refresh first.
        std::string msg = (r.errCode == "primary_changed" || r.errCode == "primary_busy")
            ? "Model changed (" + r.errCode + ") — refreshed, drag again"
            : "Rejected: " + r.describe();
        cancelGestureLocked(msg, true);
        return;
    }
    if (current) {
        double accepted = value;
        if (r.data.is_object()) {
            const json* st = nullptr;
            if (r.data.contains("strength")) st = &r.data["strength"];
            double v;
            if (st && st->is_object() && getNum(*st, "target", v)) accepted = v;
        }
        char buf[64];
        snprintf(buf, sizeof(buf), "Accepted %.3f", accepted);
        m_s.writeMsg = buf;
        m_s.writeError = false;
    }
    if (!m_dragging && !m_hasPending) {
        // Final value delivered — keep showing it until a read that started
        // after this point lands (ignores stale readbacks in between).
        m_holdUntilPollSeq = m_pollSeqStarted + 1;
        m_refreshRequested = true;
    }
    m_cv.notify_all();
}

void EthereaSdkClient::doPoll() {
    std::string base, key;
    uint64_t gen, seq;
    {
        std::lock_guard<std::mutex> lk(m_mx);
        base = m_baseUrl; key = m_key; gen = m_connGen;
        seq = ++m_pollSeqStarted;
    }
    ActionResult r = callAction(base, key, "model.primary.status", json::object(), /*houseAct=*/false, kStatusTimeout);

    // FluxRT keeps its own readback (flux.status); only fetched when selected.
    ActionResult flux;
    bool wantFlux = r.ok && r.data.is_object() && r.data.value("provider", std::string()) == "fluxrt";
    if (wantFlux) flux = callAction(base, key, "flux.status", json::object(), false, kStatusTimeout);

    std::lock_guard<std::mutex> lk(m_mx);
    m_lastPollDone = nowSec();
    if (gen != m_connGen) return;
    m_pollSeqDone = seq;

    if (r.authFailed()) {
        m_s.authFailed = true;
        m_s.reachable = true;
        m_s.connError = r.describe();
        cancelGestureLocked("", false);
        return;
    }
    if (r.transportFailed() || !r.parsed) {
        bool wasReachable = m_s.reachable;
        m_s.reachable = false;
        m_s.connError = r.describe();
        if (wasReachable || m_dragging || m_hasPending) cancelGestureLocked("Disconnected — writes cleared", true);
        m_needFreshStatus = true;
        return;
    }
    m_s.reachable = true;
    if (!r.ok || !r.data.is_object()) {
        m_s.connError = "status: " + r.describe();
        m_s.statusLoaded = false;
        return;
    }
    m_s.connError.clear();

    const json& d = r.data;
    std::string prevProvider = m_s.provider;
    Snapshot s = m_s;   // carry gesture/prompt fields
    s.provider = d.value("provider", std::string());
    s.providerLabel = d.value("label", s.provider);
    const json caps = d.contains("capabilities") && d["capabilities"].is_object() ? d["capabilities"] : json::object();
    s.supportsStrength = caps.value("strength", false);
    s.uiMax = 1.0; s.uiExp = 1.0;
    double v;
    if (getNum(caps, "strengthUiMax", v) && v > 0) s.uiMax = v;
    if (getNum(caps, "strengthUiExponent", v) && v > 0) s.uiExp = v;
    s.mode = caps.value("strengthMode", caps.value("mode", std::string()));

    const json obs = d.contains("observed") && d["observed"].is_object() ? d["observed"] : json::object();
    s.observedStatus = obs.value("status", std::string());
    if (d.contains("available") && d["available"].is_boolean()) s.available = d["available"].get<bool>();
    else if (obs.contains("available") && obs["available"].is_boolean()) s.available = obs["available"].get<bool>();
    else s.available = !obs.empty() && s.observedStatus != "stopped" && s.observedStatus != "unreachable"
                       && s.observedStatus != "error" && s.observedStatus != "failed";
    if (s.mode.empty()) s.mode = obs.value("mode", std::string());

    s.hasTarget = s.hasValue = s.hasSettled = false;
    s.revision = -1;
    if (obs.contains("stats") && obs["stats"].is_object() && obs["stats"].contains("strength"))
        readStrength(obs["stats"]["strength"], s);
    if (wantFlux && flux.ok && flux.data.is_object()) {
        if (!s.hasTarget && flux.data.contains("strength")) readStrength(flux.data["strength"], s);
        double rev;
        if (s.revision < 0 && getNum(flux.data, "textureRev", rev)) s.revision = (long long)rev;
    }

    // Operation history is separate from live observed state.
    s.operationSummary.clear();
    if (d.contains("operation") && d["operation"].is_object() && !d["operation"].empty()) {
        const json& op = d["operation"];
        std::string st = op.value("status", op.value("state", std::string()));
        std::string err;
        if (op.contains("error")) err = op["error"].is_string() ? op["error"].get<std::string>() : op["error"].dump();
        std::string tgt = op.value("provider", op.value("target", std::string()));
        s.operationSummary = (st.empty() ? "?" : st) + (tgt.empty() ? "" : " → " + tgt) + (err.empty() ? "" : " (" + err + ")");
        if (s.operationSummary.size() > 140) s.operationSummary = s.operationSummary.substr(0, 137) + "...";
    }

    s.statusLoaded = true;
    s.authFailed = false;
    m_s = s;
    m_statusTime = nowSec();

    bool gestureLive = m_dragging || m_hasPending || m_inFlight;
    if (gestureLive && !m_gestureProvider.empty() && m_s.provider != m_gestureProvider) {
        cancelGestureLocked("Model changed to " + m_s.provider + " during the drag — not replayed", true);
        m_needFreshStatus = false;   // this read IS the fresh status
        return;
    }
    if (!prevProvider.empty() && prevProvider != m_s.provider && !m_s.writeError)
        m_s.writeMsg = "Model is now " + m_s.provider;
    m_needFreshStatus = false;
    if (m_holdUntilPollSeq != 0 && m_pollSeqDone >= m_holdUntilPollSeq) m_holdUntilPollSeq = 0;
}

bool EthereaSdkClient::sendPrompt(const std::string& text, bool newScene) {
    std::string words = trim(text);
    if (words.empty()) return false;
    std::string base, key;
    {
        std::lock_guard<std::mutex> lk(m_mx);
        base = m_baseUrl; key = m_key;
    }
    if (base.empty() || key.empty()) return false;
    std::shared_ptr<PromptState> ps = m_prompt;
    {
        std::lock_guard<std::mutex> lk(ps->mx);
        if (ps->inFlight) return false;
        ps->inFlight = true;
        ps->kind.clear(); ps->text.clear(); ps->applied.clear();
        ps->clearDraft = false;
    }
    // No sessionId: the SDK resolves the configured current session. The SDK
    // adds "Cut to a new scene:" itself for newScene — never prepend it here.
    json payload = {{"text", words}, {"source", "easel"}, {"isFinal", true}, {"newScene", newScene}};
    std::thread([ps, base, key, payload] {
        ActionResult r = callAction(base, key, "cue.transcript.send", payload, /*houseAct=*/true, kPromptTimeout);
        std::string kind, text, applied;
        bool clear = false;
        if (r.http.timedOut) {
            kind = "timeout";
            text = "No answer in 30 s. It may still have been delivered — watch the output before resending.";
        } else if (r.transportFailed()) {
            kind = "error";
            text = "Not sent: " + r.describe();
        } else if (r.authFailed()) {
            kind = "error";
            text = "SDK key rejected (401).";
        } else if (!r.ok) {
            kind = "refused";
            text = "Rejected: " + r.describe();
        } else {
            judgeCue(r.data, kind, text, applied);
            clear = (kind != "refused");   // delivered; keep the draft only on failure
        }
        std::lock_guard<std::mutex> lk(ps->mx);
        ps->inFlight = false;
        ps->kind = kind; ps->text = text; ps->applied = applied;
        ps->clearDraft = clear;
        ps->seq++;
    }).detach();
    return true;
}

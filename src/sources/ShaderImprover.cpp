#include "sources/ShaderImprover.h"

#include <nlohmann/json.hpp>
#include <fstream>
#include <sstream>
#include <filesystem>
#include <cstdlib>
#include <cstdio>
#include <iostream>
#include <random>

namespace fs = std::filesystem;
using nlohmann::json;

static std::string slurp(const std::string& path) {
    std::ifstream f(path);
    if (!f) return "";
    std::stringstream ss; ss << f.rdbuf();
    return ss.str();
}

// ANTHROPIC_API_KEY from env, else from a .env near the working dir / repo.
static std::string findApiKey() {
    if (const char* k = std::getenv("ANTHROPIC_API_KEY"))
        if (k[0]) return std::string(k);
    const char* candidates[] = { ".env", "../.env", "../../.env",
                                 "/Users/lu/easel/.env" };
    for (const char* c : candidates) {
        std::ifstream f(c);
        if (!f) continue;
        std::string line;
        while (std::getline(f, line)) {
            const std::string pfx = "ANTHROPIC_API_KEY=";
            auto pos = line.find(pfx);
            if (pos != std::string::npos) {
                std::string v = line.substr(pos + pfx.size());
                // strip quotes / whitespace / CR
                while (!v.empty() && (v.back()=='\r'||v.back()=='\n'||v.back()==' '||v.back()=='"'||v.back()=='\''))
                    v.pop_back();
                size_t s = v.find_first_not_of(" \"'");
                if (s != std::string::npos) v = v.substr(s);
                if (!v.empty()) return v;
            }
        }
    }
    return "";
}

std::string ShaderImprover::resultPath() {
    std::lock_guard<std::mutex> lk(m_mtx); return m_resultPath;
}
std::string ShaderImprover::origPath() {
    std::lock_guard<std::mutex> lk(m_mtx); return m_origPath;
}
std::string ShaderImprover::message() {
    std::lock_guard<std::mutex> lk(m_mtx); return m_message;
}
void ShaderImprover::markDone(const std::string& finalPath) {
    { std::lock_guard<std::mutex> lk(m_mtx); m_resultPath = finalPath;
      m_message = "Updated " + fs::path(finalPath).filename().string()
                + " — opened on canvas (backup in ~/.easel/trash)"; }
    m_status.store(Status::Done);
}
void ShaderImprover::markError(const std::string& err) {
    { std::lock_guard<std::mutex> lk(m_mtx); m_message = err; }
    m_status.store(Status::Error);
}
void ShaderImprover::reset() {
    if (m_thread.joinable()) m_thread.join();
    { std::lock_guard<std::mutex> lk(m_mtx); m_resultPath.clear(); m_message.clear(); }
    m_status.store(Status::Idle);
}

void ShaderImprover::request(const std::string& shaderPath,
                             const std::string& instruction,
                             const std::string& combineWithPath,
                             const std::string& shadersDir) {
    if (m_status.load() == Status::Working) return;
    if (m_thread.joinable()) m_thread.join();
    { std::lock_guard<std::mutex> lk(m_mtx); m_resultPath.clear();
      m_origPath = shaderPath; m_message = "Generating…";
      m_shaderPath = shaderPath; m_instruction = instruction;
      m_combinePath = combineWithPath; m_shadersDir = shadersDir;
      m_lastCandidate.clear(); }
    m_attempt = 0;
    m_status.store(Status::Working);
    std::cerr << "[Improve] request: " << shaderPath
              << (combineWithPath.empty() ? "" : (" + " + combineWithPath)) << "\n";
    m_thread = std::thread(&ShaderImprover::generate, this, std::string());
}

bool ShaderImprover::retryWithError(const std::string& compileError) {
    if (m_attempt >= kMaxRetries) return false;
    if (m_thread.joinable()) m_thread.join();   // prior generate() has returned
    m_attempt++;
    { std::lock_guard<std::mutex> lk(m_mtx); m_resultPath.clear();
      m_message = "Fixing compile error (try "
                + std::to_string(m_attempt + 1) + ")…"; }
    std::cerr << "[Improve] retry " << m_attempt << " after compile error\n";
    m_status.store(Status::Working);
    m_thread = std::thread(&ShaderImprover::generate, this, compileError);
    return true;
}

void ShaderImprover::generate(std::string fixError) {
    std::string key = findApiKey();
    if (key.empty()) { markError("No ANTHROPIC_API_KEY (env or easel/.env)"); return; }

    // Snapshot request context (set by request(), stable across retries).
    std::string shaderPath, instruction, combineWithPath, lastCandidate;
    { std::lock_guard<std::mutex> lk(m_mtx);
      shaderPath = m_shaderPath; instruction = m_instruction;
      combineWithPath = m_combinePath; lastCandidate = m_lastCandidate; }

    std::string src = slurp(shaderPath);
    if (src.empty() && fixError.empty()) { markError("Could not read shader source"); return; }
    std::string combineSrc = combineWithPath.empty() ? "" : slurp(combineWithPath);

    std::string sys =
        "You are an expert GLSL author for Easel's ISF dialect (a VJ / "
        "projection-mapping app). Output ONLY the COMPLETE .fs file: a /*{ JSON "
        "header }*/ (DESCRIPTION, INPUTS) then GLSL. No markdown fences, no prose.\n"
        "HARD RULES — the output MUST follow these or it will not compile:\n"
        "1. Write the result to gl_FragColor (vec4). Do NOT declare any `out` "
        "variables.\n"
        "2. Do NOT emit a `#version` line and do NOT use `precision` qualifiers — "
        "Easel injects the version, precision, and ALL uniforms. Never re-declare "
        "a uniform that comes from an INPUT or a builtin.\n"
        "3. MULTI-PASS IS SUPPORTED — preserve it. If a source shader has a "
        "\"PASSES\" array with named TARGET buffers, KEEP those passes. When "
        "COMBINING two shaders, include the UNION of every pass/buffer both need "
        "(rename TARGETs to avoid clashes). Each non-final pass needs a TARGET; "
        "the LAST pass has no TARGET and writes the screen. Read a TARGET buffer "
        "with IMG_PIXEL(bufName, gl_FragCoord.xy) and branch work on PASSINDEX. "
        "Use a single pass only when the effect genuinely needs just one.\n"
        "4. Available builtins/uniforms (already declared): RENDERSIZE (vec2), "
        "TIME, TIMEDELTA (float), FRAMEINDEX (int), isf_FragNormCoord (vec2), "
        "gl_FragCoord, audioLevel, audioBass, audioMid, audioHigh (float), and "
        "IMG_PIXEL/IMG_NORM_PIXEL for any image INPUT.\n"
        "5. Define every function/const BEFORE use. No geometry/compute shaders. "
        "Keep GLSL ES-compatible: float literals (1.0 not 1), explicit vec ctors, "
        "no implicit int↔float.\n"
        "6. Keep existing INPUTS that still make sense; add new ones as needed.\n"
        "Make a bold, premium, modern result — not a timid tweak.";

    std::string user;
    if (fixError.empty()) {
        user  = "INSTRUCTION:\n" + instruction + "\n\n";
        user += "CURRENT SHADER (" + fs::path(shaderPath).filename().string() + "):\n" + src;
        if (!combineSrc.empty())
            user += "\n\nCOMBINE IDEAS FROM THIS SECOND SHADER ("
                  + fs::path(combineWithPath).filename().string() + "):\n" + combineSrc;
    } else {
        user  = "The .fs you produced FAILED to compile in Easel. GLSL error:\n----\n"
              + fixError + "\n----\n\nThe shader you produced was:\n" + lastCandidate
              + "\n\nReturn the COMPLETE corrected .fs that fixes this error while "
                "still honoring the original instruction:\n" + instruction
              + "\nOutput ONLY the file contents.";
    }

    json body = {
        {"model", "claude-sonnet-4-6"},
        {"max_tokens", 24000},   // combined shaders are long — avoid truncation (premature EOF)
        {"system", sys},
        {"messages", json::array({ json{{"role","user"},{"content",user}} })}
    };

    // Write request body + run curl → response file (avoids shell-escaping the
    // large GLSL payload; key passed as a header value).
    std::random_device rd;
    std::string tag = std::to_string(rd());
    fs::path reqF  = fs::temp_directory_path() / ("easel_improve_req_" + tag + ".json");
    fs::path respF = fs::temp_directory_path() / ("easel_improve_resp_" + tag + ".json");
    { std::ofstream o(reqF); o << body.dump(); }

    std::string cmd =
        "curl -s -m 180 https://api.anthropic.com/v1/messages "
        "-H 'content-type: application/json' "
        "-H 'anthropic-version: 2023-06-01' "
        "-H 'x-api-key: " + key + "' "
        "-d @'" + reqF.string() + "' -o '" + respF.string() + "'";
    int rc = std::system(cmd.c_str());
    std::error_code ec; fs::remove(reqF, ec);
    if (rc != 0) { markError("curl failed (network?)"); return; }

    std::string resp = slurp(respF.string());
    fs::remove(respF, ec);
    if (resp.empty()) { markError("Empty API response"); return; }

    std::string text;
    try {
        json r = json::parse(resp);
        if (r.contains("error")) {
            std::string em = r["error"].value("message", std::string("API error"));
            markError("API: " + em); return;
        }
        if (r.contains("content") && r["content"].is_array() && !r["content"].empty())
            text = r["content"][0].value("text", std::string(""));
    } catch (...) { markError("Bad API JSON"); return; }
    if (text.empty()) { markError("No shader in response"); return; }

    // Strip accidental ``` / ```glsl fences if the model added them.
    auto strip = [](std::string s) {
        size_t a = s.find("```");
        if (a != std::string::npos) {
            size_t nl = s.find('\n', a);
            size_t b = s.rfind("```");
            if (nl != std::string::npos && b != std::string::npos && b > nl)
                s = s.substr(nl + 1, b - nl - 1);
        }
        return s;
    };
    text = strip(text);

    // Write the candidate to a TEMP file. The GL thread compile-tests it and,
    // only if it compiles, backs up + overwrites the ORIGINAL .fs (build on
    // top), so a bad generation never corrupts the user's shader. shadersDir
    // is unused now but kept in the signature for callers.
    fs::path outF = fs::temp_directory_path() / ("easel_improve_out_" + tag + ".fs");
    { std::ofstream o(outF); if (!o) { markError("Cannot write candidate"); return; }
      o << text; }

    { std::lock_guard<std::mutex> lk(m_mtx);
      m_lastCandidate = text;                 // kept for compile-error retry
      m_resultPath = outF.string();
      m_message = "Compiling…"; }
    m_status.store(Status::ReadyToCompile);   // GL thread compile-tests + finalizes
}

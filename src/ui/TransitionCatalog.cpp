#include "ui/TransitionCatalog.h"
#include "render/GLTransition.h"

#include <imgui.h>
#include <GLFW/glfw3.h>

#include <algorithm>
#include <cstring>
#include <cctype>
#include <cmath>

namespace {

constexpr int kTileW            = 192;
constexpr int kTileH            = 108;          // 16:9
constexpr int kColumns          = 3;
constexpr int kTilesPerFrame    = 6;            // round-robin budget
constexpr float kLoopSeconds    = 2.6f;
constexpr float kHoldSeconds    = 0.45f;

// Loops 0→1 with a hold at each end, so the user sees both endpoints clearly.
float computeLoopProgress(double t0, double now, int slot) {
    double loop = (double)kLoopSeconds;
    double hold = (double)kHoldSeconds;
    double phase = std::fmod((now - t0) + slot * 0.18, loop);
    double frac  = phase / loop;
    if (frac < hold / loop) return 0.0f;
    if (frac > 1.0 - hold / loop) return 1.0f;
    double inner = (frac - hold / loop) / (1.0 - 2.0 * hold / loop);
    if (inner < 0.0) inner = 0.0; else if (inner > 1.0) inner = 1.0;
    return (float)inner;
}

} // namespace

TransitionCatalog::~TransitionCatalog() { release(); }

void TransitionCatalog::release() {
    for (auto& kv : m_tiles) {
        if (kv.second.tex) glDeleteTextures(1, &kv.second.tex);
        if (kv.second.fbo) glDeleteFramebuffers(1, &kv.second.fbo);
    }
    m_tiles.clear();
    m_visibleOrder.clear();
    m_renderCursor = 0;
}

TransitionCatalog::Tile& TransitionCatalog::ensureTile(const std::string& name,
                                                       int w, int h) {
    Tile& t = m_tiles[name];
    if (t.fbo && t.w == w && t.h == h) return t;
    if (t.tex) glDeleteTextures(1, &t.tex);
    if (t.fbo) glDeleteFramebuffers(1, &t.fbo);

    glGenTextures(1, &t.tex);
    glBindTexture(GL_TEXTURE_2D, t.tex);
    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA8, w, h, 0, GL_RGBA, GL_UNSIGNED_BYTE, nullptr);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);

    glGenFramebuffers(1, &t.fbo);
    glBindFramebuffer(GL_FRAMEBUFFER, t.fbo);
    glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0,
                           GL_TEXTURE_2D, t.tex, 0);
    glBindFramebuffer(GL_FRAMEBUFFER, 0);
    glBindTexture(GL_TEXTURE_2D, 0);

    t.w = w; t.h = h;
    return t;
}

void TransitionCatalog::renderTile(Tile& tile, const std::string& name,
                                   GLuint fromTex, GLuint toTex,
                                   int /*sourceW*/, int /*sourceH*/,
                                   float progress) {
    GLTransition* runner = GLTransitionLibrary::instance().get(name);
    if (!runner || !runner->isValid()) return;

    // Capture previous viewport + framebuffer so we don't disturb caller state.
    GLint prevFbo = 0, prevVp[4] = {0,0,0,0};
    glGetIntegerv(GL_DRAW_FRAMEBUFFER_BINDING, &prevFbo);
    glGetIntegerv(GL_VIEWPORT, prevVp);

    glBindFramebuffer(GL_FRAMEBUFFER, tile.fbo);
    glViewport(0, 0, tile.w, tile.h);
    glDisable(GL_BLEND);
    glClearColor(0.04f, 0.05f, 0.06f, 1.0f);
    glClear(GL_COLOR_BUFFER_BIT);

    // If callers haven't given us textures (no clips on stage), the runner
    // still draws using whatever 0-bound texture returns — usually transparent
    // or solid black. Better than crashing.
    runner->render(fromTex, toTex, progress, tile.w, tile.h);

    glBindFramebuffer(GL_FRAMEBUFFER, prevFbo);
    glViewport(prevVp[0], prevVp[1], prevVp[2], prevVp[3]);
}

std::string TransitionCatalog::drawAndPick(GLuint fromTex, GLuint toTex,
                                           int sourceW, int sourceH,
                                           const std::string& currentlyPicked) {
    if (m_t0 < 0.0) m_t0 = glfwGetTime();
    double now = glfwGetTime();

    // ── Filter row ────────────────────────────────────────────────────────
    ImGui::SetNextItemWidth(-1);
    ImGui::InputTextWithHint("##transitionFilter",
                             "Search transitions...",
                             m_filter, sizeof(m_filter));
    ImGui::Dummy(ImVec2(0, 4));

    auto matches = [&](const std::string& s) -> bool {
        if (m_filter[0] == '\0') return true;
        std::string a = s, b = m_filter;
        std::transform(a.begin(), a.end(), a.begin(),
                       [](unsigned char c) { return (char)std::tolower(c); });
        std::transform(b.begin(), b.end(), b.begin(),
                       [](unsigned char c) { return (char)std::tolower(c); });
        return a.find(b) != std::string::npos;
    };

    auto allNames = GLTransitionLibrary::instance().names();

    std::string picked;

    // ── Grid ──────────────────────────────────────────────────────────────
    // Bounded scroll region. Each tile renders its preview lazily — only
    // kTilesPerFrame fire glDraw calls per frame, distributed by round-robin
    // across the names that survived the filter.
    ImGui::BeginChild("##trCatalogGrid", ImVec2(0, 360), true,
                      ImGuiWindowFlags_AlwaysVerticalScrollbar);

    m_visibleOrder.clear();
    m_visibleOrder.reserve(allNames.size());
    int col = 0;
    int idx = 0;
    for (const auto& n : allNames) {
        if (!matches(n)) continue;
        m_visibleOrder.push_back(n);

        if (col != 0) ImGui::SameLine(0, 6);

        // Per-tile rendering, throttled.
        bool shouldRenderThisFrame = false;
        if (m_visibleOrder.size() <= (size_t)kTilesPerFrame) {
            shouldRenderThisFrame = true;
        } else {
            int slot = (m_renderCursor + idx) % (int)m_visibleOrder.size();
            shouldRenderThisFrame = (slot < kTilesPerFrame);
        }
        Tile& tile = ensureTile(n, kTileW, kTileH);
        if (shouldRenderThisFrame) {
            float p = computeLoopProgress(m_t0, now, idx);
            renderTile(tile, n, fromTex, toTex, sourceW, sourceH, p);
        }

        bool isCurrent = (n == currentlyPicked);
        ImVec2 cursor = ImGui::GetCursorScreenPos();
        ImVec2 size((float)kTileW, (float)kTileH);

        ImGui::PushID(n.c_str());
        // Invisible hit target overlays the image so we get hover/active state.
        ImGui::ImageButton(n.c_str(),
                           (ImTextureID)(intptr_t)tile.tex,
                           size, ImVec2(0, 1), ImVec2(1, 0));
        bool hov = ImGui::IsItemHovered();
        bool clk = ImGui::IsItemClicked();
        ImGui::PopID();

        // Selection ring + label.
        ImDrawList* d = ImGui::GetWindowDrawList();
        if (isCurrent) {
            d->AddRect(cursor, ImVec2(cursor.x + size.x, cursor.y + size.y),
                       IM_COL32(255, 255, 255, 220), 4.0f, 0, 2.0f);
        } else if (hov) {
            d->AddRect(cursor, ImVec2(cursor.x + size.x, cursor.y + size.y),
                       IM_COL32(255, 255, 255, 90), 4.0f, 0, 1.0f);
        }
        // Tiny label band at the bottom — readable even on bright transitions.
        ImU32 labelBg = IM_COL32(0, 0, 0, 160);
        d->AddRectFilled(ImVec2(cursor.x, cursor.y + size.y - 18),
                         ImVec2(cursor.x + size.x, cursor.y + size.y),
                         labelBg);
        ImVec2 ts = ImGui::CalcTextSize(n.c_str());
        ImU32 labelCol = isCurrent ? IM_COL32(255, 255, 255, 255)
                                   : IM_COL32(220, 224, 230, 230);
        d->AddText(ImVec2(cursor.x + 6,
                          cursor.y + size.y - 14 - (14 - ts.y) * 0.5f),
                   labelCol, n.c_str());

        if (clk) picked = n;

        col++;
        if (col >= kColumns) { col = 0; }
        idx++;
    }

    ImGui::EndChild();

    // Advance the round-robin so different tiles render next frame.
    if (!m_visibleOrder.empty()) {
        m_renderCursor = (m_renderCursor + kTilesPerFrame) %
                         (int)m_visibleOrder.size();
    }

    return picked;
}

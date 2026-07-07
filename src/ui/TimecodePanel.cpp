#include "ui/TimecodePanel.h"
#include "ui/UIManager.h"
#include <imgui.h>
#include <imgui_internal.h>
#include <cmath>
#include <cstring>
#include <cstdio>
#include <chrono>
#include <ctime>
#include <algorithm>

// ─── helpers ─────────────────────────────────────────────────────────────────

namespace {

// Returns HH, MM, SS, mmm in Eastern time (UTC-5 = EST; rough, ignores DST)
// Adjust kESTOffsetH to -4 for EDT if desired.
struct HMSms { int h, m, s, ms; };

HMSms eastClock() {
    using namespace std::chrono;
    auto now = system_clock::now();
    auto ms  = duration_cast<milliseconds>(now.time_since_epoch()).count();
    static constexpr int64_t kESTOffsetMs = -5 * 3600 * 1000LL;
    ms += kESTOffsetMs;
    if (ms < 0) ms += 24 * 3600 * 1000LL;
    int64_t dayMs = ms % (24LL * 3600 * 1000);
    if (dayMs < 0) dayMs += 24LL * 3600 * 1000;
    int msi = (int)(dayMs % 1000);
    int sec = (int)(dayMs / 1000);
    int min = sec / 60; sec %= 60;
    int hr  = min / 60; min %= 60; hr %= 24;
    return {hr, min, sec, msi};
}

HMSms toHMSms(double totalSecs) {
    int ms  = (int)(std::fmod(totalSecs, 1.0) * 1000.0);
    int s   = (int)totalSecs % 60;
    int m   = ((int)totalSecs / 60) % 60;
    int h   = (int)totalSecs / 3600;
    return {h, m, s, ms};
}

// Renders a large mono digit block with a subtle label below
void bigClock(const char* fmt, const HMSms& t, ImU32 col) {
    char buf[32];
    snprintf(buf, sizeof(buf), fmt, t.h, t.m, t.s, t.ms);
    ImDrawList* dl = ImGui::GetWindowDrawList();
    ImVec2 pos = ImGui::GetCursorScreenPos();
    float fs = ImGui::GetFontSize() * 1.6f;
    dl->AddText(ImGui::GetFont(), fs, pos, col, buf);
    ImGui::Dummy(ImVec2(ImGui::CalcTextSize(buf).x * 1.6f, fs + 4.0f));
}

void badge(const char* label, ImU32 bg, ImU32 fg) {
    ImVec2 p = ImGui::GetCursorScreenPos();
    ImVec2 sz = ImGui::CalcTextSize(label);
    float padX = 5.0f, padY = 2.0f;
    ImDrawList* dl = ImGui::GetWindowDrawList();
    dl->AddRectFilled(p, ImVec2(p.x + sz.x + padX*2, p.y + sz.y + padY*2),
                      bg, 3.0f);
    dl->AddText(ImVec2(p.x + padX, p.y + padY), fg, label);
    ImGui::Dummy(ImVec2(sz.x + padX*2 + 4, sz.y + padY*2));
}

} // namespace

// ─── public entry point ──────────────────────────────────────────────────────

void TimecodePanel::render(double glfwTime, ProDJLink* djlink) {
    // Advance production timecode
    if (!timecodePaused && m_lastFrameTime > 0.0) {
        double dt = glfwTime - m_lastFrameTime;
        if (dt > 0.0 && dt < 0.5) timecodeSeconds += dt;
    }
    m_lastFrameTime = glfwTime;

    ImGui::PushStyleColor(ImGuiCol_Text, IM_COL32(210, 220, 235, 255));

    // ── Wall Clock ────────────────────────────────────────────────────────
    {
        ImGui::PushStyleColor(ImGuiCol_Text, IM_COL32(140, 150, 165, 255));
        ImGui::Text("WALL CLOCK");
        ImGui::PopStyleColor();
        ImGui::SameLine(ImGui::GetContentRegionAvail().x - ImGui::CalcTextSize("Hide").x - 8);
        if (ImGui::SmallButton(showWallClock ? "Hide##wc" : "Show##wc"))
            showWallClock = !showWallClock;
    }

    if (showWallClock) {
        HMSms t = eastClock();
        ImGui::PushStyleColor(ImGuiCol_Text, IM_COL32(255,255,255,255));
        char buf[32];
        snprintf(buf, sizeof(buf), "%02d:%02d:%02d.%03d", t.h, t.m, t.s, t.ms);
        // Large monospaced clock
        ImDrawList* dl = ImGui::GetWindowDrawList();
        ImVec2 pos = ImGui::GetCursorScreenPos();
        float fs = ImGui::GetFontSize() * 2.0f;
        dl->AddText(ImGui::GetFont(), fs, pos, IM_COL32(255,255,255,255), buf);
        ImGui::Dummy(ImVec2(ImGui::GetContentRegionAvail().x, fs + 2.0f));
        // Sub-label
        ImGui::PushStyleColor(ImGuiCol_Text, IM_COL32(100,108,120,255));
        ImGui::Text("EST (UTC-5)");
        ImGui::PopStyleColor();
        ImGui::PopStyleColor();
    }

    ImGui::Dummy(ImVec2(0, 8));
    ImGui::PushStyleColor(ImGuiCol_Separator, IM_COL32(255,255,255,22));
    ImGui::Separator();
    ImGui::PopStyleColor();
    ImGui::Dummy(ImVec2(0, 6));

    // ── Production Timecode ───────────────────────────────────────────────
    {
        ImGui::PushStyleColor(ImGuiCol_Text, IM_COL32(140, 150, 165, 255));
        ImGui::Text("PRODUCTION TIMECODE");
        ImGui::PopStyleColor();
        ImGui::SameLine(ImGui::GetContentRegionAvail().x - ImGui::CalcTextSize("Hide").x - 8);
        if (ImGui::SmallButton(showTimecode ? "Hide##tc" : "Show##tc"))
            showTimecode = !showTimecode;
    }

    if (showTimecode) {
        HMSms t = toHMSms(timecodeSeconds);

        // Big timecode display
        {
            ImDrawList* dl = ImGui::GetWindowDrawList();
            ImVec2 pos = ImGui::GetCursorScreenPos();
            float fs = ImGui::GetFontSize() * 2.0f;
            char buf[32];
            snprintf(buf, sizeof(buf), "%02d:%02d:%02d.%03d", t.h, t.m, t.s, t.ms);
            ImU32 col = timecodePaused ? IM_COL32(180,185,195,255)
                                       : IM_COL32(120,200,255,255);
            dl->AddText(ImGui::GetFont(), fs, pos, col, buf);
            ImGui::Dummy(ImVec2(ImGui::GetContentRegionAvail().x, fs + 2.0f));
        }

        // Transport controls
        float btnW = 40.0f;
        ImGui::PushStyleColor(ImGuiCol_Button,        IM_COL32(255,255,255,20));
        ImGui::PushStyleColor(ImGuiCol_ButtonHovered, IM_COL32(255,255,255,40));
        ImGui::PushStyleColor(ImGuiCol_ButtonActive,  IM_COL32(255,255,255,60));

        if (ImGui::Button(timecodePaused ? ">" : "||", ImVec2(btnW, 0)))
            timecodePaused = !timecodePaused;
        if (ImGui::IsItemHovered()) ImGui::SetTooltip(timecodePaused ? "Play" : "Pause");

        ImGui::SameLine(0, 4);
        if (ImGui::Button("[]", ImVec2(btnW, 0))) {
            timecodeSeconds = 0.0;
            timecodePaused  = true;
        }
        if (ImGui::IsItemHovered()) ImGui::SetTooltip("Reset to zero");

        ImGui::SameLine(0, 4);
        if (ImGui::Button("Edit", ImVec2(btnW + 10, 0))) {
            m_editing = !m_editing;
            if (m_editing) {
                m_editH  = t.h; m_editM  = t.m;
                m_editS  = t.s; m_editMs = t.ms;
            }
        }
        if (ImGui::IsItemHovered()) ImGui::SetTooltip("Edit timecode value");
        ImGui::PopStyleColor(3);

        // Inline editor
        if (m_editing) {
            ImGui::Dummy(ImVec2(0, 4));
            ImGui::SetNextItemWidth(48);
            ImGui::InputInt("H##tce", &m_editH, 0); m_editH = std::max(0, m_editH);
            ImGui::SameLine(0, 4);
            ImGui::SetNextItemWidth(48);
            ImGui::InputInt("M##tce", &m_editM, 0);
            m_editM = std::max(0, std::min(59, m_editM));
            ImGui::SameLine(0, 4);
            ImGui::SetNextItemWidth(48);
            ImGui::InputInt("S##tce", &m_editS, 0);
            m_editS = std::max(0, std::min(59, m_editS));
            ImGui::SameLine(0, 4);
            ImGui::SetNextItemWidth(56);
            ImGui::InputInt("ms##tce", &m_editMs, 0);
            m_editMs = std::max(0, std::min(999, m_editMs));

            ImGui::Dummy(ImVec2(0, 2));
            ImGui::PushStyleColor(ImGuiCol_Button, IM_COL32(74,140,255,180));
            if (ImGui::Button("Set", ImVec2(60, 0))) {
                timecodeSeconds = m_editH*3600.0 + m_editM*60.0 + m_editS + m_editMs/1000.0;
                m_editing = false;
            }
            ImGui::PopStyleColor();
            ImGui::SameLine(0, 4);
            if (ImGui::Button("Cancel", ImVec2(60, 0))) m_editing = false;
        }
    }

    ImGui::Dummy(ImVec2(0, 8));
    ImGui::PushStyleColor(ImGuiCol_Separator, IM_COL32(255,255,255,22));
    ImGui::Separator();
    ImGui::PopStyleColor();
    ImGui::Dummy(ImVec2(0, 6));

    // ── Pro DJ Link ───────────────────────────────────────────────────────
    renderDJLinkSection(djlink);

    ImGui::PopStyleColor(); // Text
}

// ─── Pro DJ Link section ─────────────────────────────────────────────────────

void TimecodePanel::renderDJLinkSection(ProDJLink* djlink) {
    ImDrawList* dl = ImGui::GetWindowDrawList();

    // Header row
    {
        ImGui::PushStyleColor(ImGuiCol_Text, IM_COL32(140, 150, 165, 255));
        ImGui::Text("PRO DJ LINK");
        ImGui::PopStyleColor();

        ImGui::SameLine();
        if (djlink) {
            bool running = djlink->isRunning();
            if (running) {
                int n = djlink->deviceCount();
                ImGui::PushStyleColor(ImGuiCol_Text,
                    n > 0 ? IM_COL32(80,220,120,255) : IM_COL32(200,200,80,255));
                ImGui::Text(n > 0 ? "(%d device%s)" : "(scanning...)", n, n==1?"":"s");
                ImGui::PopStyleColor();
                ImGui::SameLine(ImGui::GetContentRegionAvail().x - 70);
                ImGui::PushStyleColor(ImGuiCol_Button, IM_COL32(200,60,60,120));
                ImGui::PushStyleColor(ImGuiCol_ButtonHovered, IM_COL32(220,80,80,180));
                if (ImGui::Button("Disconnect", ImVec2(-1, 0))) djlink->stop();
                ImGui::PopStyleColor(2);
            } else {
                ImGui::PushStyleColor(ImGuiCol_Text, IM_COL32(140,140,140,255));
                ImGui::Text("(not connected)");
                ImGui::PopStyleColor();
                ImGui::SameLine(ImGui::GetContentRegionAvail().x - 65);
                ImGui::PushStyleColor(ImGuiCol_Button, IM_COL32(74,140,255,160));
                ImGui::PushStyleColor(ImGuiCol_ButtonHovered, IM_COL32(100,160,255,220));
                if (ImGui::Button("Connect", ImVec2(-1, 0))) djlink->start();
                ImGui::PopStyleColor(2);
            }
        }
    }

    if (!djlink || !djlink->isRunning()) {
        ImGui::Dummy(ImVec2(0, 8));
        ImGui::PushStyleColor(ImGuiCol_Text, IM_COL32(100,108,120,255));
        ImGui::TextWrapped(
            "Connect to a network with Pioneer CDJs or XDJs.\n"
            "Plug into the same switch/router as the DJ gear.\n"
            "Easel will discover players automatically and\n"
            "receive track, BPM, beat and waveform data.");
        ImGui::PopStyleColor();
        return;
    }

    auto statuses = djlink->getPlayerStatuses();

    if (statuses.empty()) {
        ImGui::Dummy(ImVec2(0, 12));
        // Animated scanning indicator
        static double scanT = 0.0;
        scanT += ImGui::GetIO().DeltaTime;
        int dots = (int)(scanT * 2.0) % 4;
        char scanBuf[16] = "Scanning";
        for (int i = 0; i < dots; i++) strcat(scanBuf, ".");
        ImGui::PushStyleColor(ImGuiCol_Text, IM_COL32(160,170,185,255));
        ImGui::Text("%s", scanBuf);
        ImGui::PopStyleColor();
        ImGui::PushStyleColor(ImGuiCol_Text, IM_COL32(100,108,120,255));
        ImGui::TextWrapped("Waiting for CDJ/XDJ status packets on port 50001...");
        ImGui::PopStyleColor();
        return;
    }

    ImGui::Dummy(ImVec2(0, 6));

    // Sort by player number and render each card
    std::vector<std::pair<int,ProDJLink::PlayerStatus>> sorted(statuses.begin(), statuses.end());
    std::sort(sorted.begin(), sorted.end(), [](auto& a, auto& b){return a.first < b.first;});

    for (auto& [num, ps] : sorted) {
        ImGui::PushID(num);
        renderPlayerCard(num, ps);
        ImGui::Dummy(ImVec2(0, 6));
        ImGui::PopID();
    }
}

void TimecodePanel::renderPlayerCard(int slot, const ProDJLink::PlayerStatus& ps) {
    ImDrawList* dl = ImGui::GetWindowDrawList();
    float panelW = ImGui::GetContentRegionAvail().x;

    // Card background
    ImVec2 cardP = ImGui::GetCursorScreenPos();
    float cardH  = 120.0f;
    ImU32 cardBg = ps.playing ? IM_COL32(16, 22, 34, 255)
                               : IM_COL32(14, 16, 22, 255);
    dl->AddRectFilled(cardP, ImVec2(cardP.x + panelW, cardP.y + cardH),
                      cardBg, 6.0f);

    // Accent stripe on left for player color
    static constexpr ImU32 kPlayerColors[4] = {
        IM_COL32(255, 100,  60, 255),  // P1 orange-red
        IM_COL32( 60, 160, 255, 255),  // P2 blue
        IM_COL32(120, 220,  80, 255),  // P3 green
        IM_COL32(220, 100, 255, 255),  // P4 purple
    };
    ImU32 pCol = kPlayerColors[(slot - 1) % 4];
    dl->AddRectFilled(cardP,
                      ImVec2(cardP.x + 4, cardP.y + cardH),
                      pCol, 3.0f);

    // Reserve card area for interaction
    ImGui::Dummy(ImVec2(panelW, cardH));

    // Player number
    {
        char pnum[4]; snprintf(pnum, sizeof(pnum), "P%d", ps.playerNumber);
        float fs = ImGui::GetFontSize() * 1.1f;
        dl->AddText(ImGui::GetFont(), fs, ImVec2(cardP.x + 10, cardP.y + 6), pCol, pnum);
    }

    // ON AIR badge
    if (ps.onAir) {
        ImVec2 bp = ImVec2(cardP.x + 38, cardP.y + 7);
        ImVec2 bsz = ImGui::CalcTextSize("ON AIR");
        dl->AddRectFilled(bp, ImVec2(bp.x + bsz.x + 8, bp.y + bsz.y + 4),
                          IM_COL32(220, 50, 50, 220), 3.0f);
        dl->AddText(ImVec2(bp.x + 4, bp.y + 2), IM_COL32(255,255,255,255), "ON AIR");
    }

    // SYNC / MASTER badges
    float badgeX = cardP.x + panelW - 8;
    if (ps.master) {
        const char* lbl = "MASTER";
        ImVec2 bsz = ImGui::CalcTextSize(lbl);
        badgeX -= bsz.x + 10;
        dl->AddRectFilled(ImVec2(badgeX, cardP.y + 7),
                          ImVec2(badgeX + bsz.x + 8, cardP.y + 7 + bsz.y + 4),
                          IM_COL32(74,140,255,180), 3.0f);
        dl->AddText(ImVec2(badgeX + 4, cardP.y + 9), IM_COL32(255,255,255,255), lbl);
        badgeX -= 4;
    }
    if (ps.syncOn) {
        const char* lbl = "SYNC";
        ImVec2 bsz = ImGui::CalcTextSize(lbl);
        badgeX -= bsz.x + 10;
        dl->AddRectFilled(ImVec2(badgeX, cardP.y + 7),
                          ImVec2(badgeX + bsz.x + 8, cardP.y + 7 + bsz.y + 4),
                          IM_COL32(100,200,100,140), 3.0f);
        dl->AddText(ImVec2(badgeX + 4, cardP.y + 9), IM_COL32(255,255,255,255), lbl);
    }

    // Track title / artist
    {
        float titleY = cardP.y + 26;
        ImU32 titleCol = IM_COL32(230, 235, 245, 255);
        ImU32 artistCol = IM_COL32(130, 140, 160, 255);
        const char* title  = ps.trackTitle.empty()  ? (ps.trackNumber ? "Track Loading..." : "No Track") : ps.trackTitle.c_str();
        const char* artist = ps.artistName.empty()  ? ""                                                  : ps.artistName.c_str();

        // Clip title to card width
        float maxW = panelW - 20;
        dl->PushClipRect(ImVec2(cardP.x + 10, titleY),
                         ImVec2(cardP.x + 10 + maxW, titleY + 30), true);
        dl->AddText(ImVec2(cardP.x + 10, titleY), titleCol, title);
        if (*artist) dl->AddText(ImVec2(cardP.x + 10, titleY + 14), artistCol, artist);
        dl->PopClipRect();
    }

    // BPM display
    {
        float bpmY = cardP.y + 60;
        char bpmBuf[16];
        if (ps.bpm > 0)
            snprintf(bpmBuf, sizeof(bpmBuf), "%.1f", ps.bpm);
        else
            strcpy(bpmBuf, "---.-");
        float fs = ImGui::GetFontSize() * 1.4f;
        ImVec2 bsz = ImGui::CalcTextSize(bpmBuf);
        dl->AddText(ImGui::GetFont(), fs, ImVec2(cardP.x + 10, bpmY),
                    ps.playing ? IM_COL32(255,255,255,255) : IM_COL32(140,150,165,255),
                    bpmBuf);
        dl->AddText(ImVec2(cardP.x + 10 + bsz.x * 1.4f + 4, bpmY + 8),
                    IM_COL32(100,110,125,255), "BPM");
    }

    // Beat grid — 4 dots, current beat lit
    {
        float beatY = cardP.y + 60;
        float dotX  = cardP.x + 100;
        for (int b = 1; b <= 4; b++) {
            bool lit = (b == ps.beatInBar) && ps.playing;
            float r = lit ? 7.0f : 4.5f;
            ImU32 c = lit ? pCol : IM_COL32(60,65,75,255);
            dl->AddCircleFilled(ImVec2(dotX, beatY + 8), r, c);
            if (lit) dl->AddCircle(ImVec2(dotX, beatY + 8), r + 2, ImU32(pCol & 0x00FFFFFF) | 0x60000000);
            dotX += 18.0f;
        }
    }

    // Mini waveform strip showing playhead position
    {
        float wfX  = cardP.x + 10;
        float wfY  = cardP.y + cardH - 22;
        float wfW  = panelW - 20;
        float wfH  = 16.0f;

        // Background track
        dl->AddRectFilled(ImVec2(wfX, wfY), ImVec2(wfX + wfW, wfY + wfH),
                          IM_COL32(30, 35, 45, 255), 2.0f);

        if (ps.waveformLoaded && !ps.waveformHeight.empty()) {
            int cols = (int)ps.waveformHeight.size();
            for (int i = 0; i < cols; i++) {
                float x = wfX + (float)i / cols * wfW;
                float h = (float)ps.waveformHeight[i] / 31.0f * wfH;
                ImU32 c = ps.waveformColor.size() > (size_t)i ? ps.waveformColor[i]
                                                               : IM_COL32(74, 140, 255, 200);
                dl->AddRectFilled(ImVec2(x, wfY + wfH - h),
                                  ImVec2(x + std::max(1.0f, wfW / cols), wfY + wfH), c);
            }
        } else {
            // Placeholder gradient bar
            for (int i = 0; i < (int)wfW; i += 2) {
                float phase = (float)i / wfW;
                float h = wfH * (0.3f + 0.25f * sinf(phase * 30.0f));
                dl->AddRectFilled(ImVec2(wfX + i, wfY + wfH - h),
                                  ImVec2(wfX + i + 2, wfY + wfH),
                                  IM_COL32(50, 60, 80, 200));
            }
        }

        // Playhead
        if (ps.playheadPct >= 0.0f && ps.playheadPct <= 1.0f) {
            float phX = wfX + ps.playheadPct * wfW;
            dl->AddLine(ImVec2(phX, wfY - 1), ImVec2(phX, wfY + wfH + 1),
                        IM_COL32(255,255,255,200), 1.5f);
        }

        // Progress fill (left of playhead) — subtle accent
        if (ps.playheadPct > 0.0f) {
            float phX = wfX + ps.playheadPct * wfW;
            dl->AddRectFilled(ImVec2(wfX, wfY), ImVec2(phX, wfY + wfH),
                              IM_COL32(74,140,255,25), 2.0f);
        }
    }

    // Play state indicator
    {
        float icoX = cardP.x + panelW - 24;
        float icoY = cardP.y + 58;
        if (ps.playing) {
            // Triangle
            dl->AddTriangleFilled(
                ImVec2(icoX, icoY),
                ImVec2(icoX, icoY + 14),
                ImVec2(icoX + 10, icoY + 7),
                IM_COL32(80,220,120,255));
        } else {
            // Pause bars
            dl->AddRectFilled(ImVec2(icoX,     icoY), ImVec2(icoX + 3,  icoY + 14), IM_COL32(150,155,165,255));
            dl->AddRectFilled(ImVec2(icoX + 6, icoY), ImVec2(icoX + 9,  icoY + 14), IM_COL32(150,155,165,255));
        }
    }
}

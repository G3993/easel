#pragma once
#include <cstdint>
#include "app/ProDJLink.h"

// Renders the Timecode inspector tab:
//   - EST wall clock (always-running, millisecond precision, toggleable)
//   - Pausable / editable production timecode (HH:MM:SS.mmm)
//   - Pro DJ Link monitor: device list + per-player status cards with
//     BPM, beat grid, waveform strip, track info, on-air/sync badges.

class TimecodePanel {
public:
    void render(double glfwTime, ProDJLink* djlink);

    // Clock visibility toggles
    bool showWallClock     = true;
    bool showTimecode      = true;

    // Production timecode state
    double timecodeSeconds = 0.0;
    bool   timecodePaused  = true;

private:
    void renderWallClock(double glfwTime);
    void renderProductionTimecode(double glfwTime);
    void renderDJLinkSection(ProDJLink* djlink);
    void renderPlayerCard(int slot, const ProDJLink::PlayerStatus& ps);

    // Editing state for production timecode
    bool   m_editing      = false;
    int    m_editH = 0, m_editM = 0, m_editS = 0, m_editMs = 0;
    double m_lastFrameTime = 0.0;

    // Beat flash state per player (1-4)
    uint32_t m_lastBeatCount[4] = {};
    double   m_beatFlashTime[4] = {};
};

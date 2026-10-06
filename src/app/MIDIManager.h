#pragma once
#include <string>
#include <vector>
#include <mutex>
#include <functional>
#include <unordered_map>
#include <cstdint>
#include <algorithm>
#include <utility>
#include <atomic>

#ifdef _WIN32
#include <windows.h>
#include <mmeapi.h>
#pragma comment(lib, "winmm.lib")
#endif
#ifdef __APPLE__
#include <cstdint>
#endif

// A MIDI CC/Note event
struct MIDIEvent {
    int channel = 0;    // 0-15
    int type = 0;       // 0=CC, 1=NoteOn, 2=NoteOff
    int number = 0;     // CC number or note number
    int value = 0;      // 0-127
};

// A MIDI mapping: CC/note -> target
struct MIDIMapping {
    int channel = -1;   // -1 = any channel
    int type = 0;       // 0=CC, 1=Note
    int number = 0;     // CC or note number

    // Target
    enum class Target {
        LayerOpacity, LayerVisible, LayerPosX, LayerPosY, LayerScale, LayerRotation,
        SceneRecall, BPMSet, BPMTap,
        None // appended: "no target assigned" (e.g. an unassigned knob)
    };
    Target target = Target::LayerOpacity;
    int layerIndex = 0; // for layer targets
    int sceneIndex = 0; // for scene recall
};

class MIDIManager {
public:
    ~MIDIManager();

    // Device management
    // deviceIndex() == kAllDevices means every MIDI source is connected at
    // once (the default): a Launch Control next to an audio interface's MIDI
    // port both work without the user picking one. Hot-plug is handled in
    // handleHotplug() (called from pollEvents on the main thread).
    static constexpr int kAllDevices = -1;
    std::vector<std::string> listDevices();
    bool openDevice(int index);   // kAllDevices = connect every source
    bool openAll() { return openDevice(kAllDevices); }
    void closeDevice();
    bool isOpen() const { return m_open; }
    int deviceIndex() const { return m_deviceIdx; }
    bool isAllDevices() const { return m_open && m_deviceIdx == kAllDevices; }
    // Human label for the current selection ("All devices", the device name,
    // or "None"). Never indexes out of range.
    std::string currentDeviceLabel();
    // Platform read callbacks / notify procs flag a topology change here; the
    // main thread applies it in pollEvents().
    void markSetupChanged() { m_setupChanged = true; }
    void handleHotplug();
    // True when a Novation Launch Control XL 3 ("LCXL3 ...") is among the
    // connected inputs. Its factory CC layout differs from the original XL's
    // (knobs 13-36, faders 5-12), so ShowDeck/Canvas pick their default CC
    // tables from this. Main thread only (reads the hot-plug device list).
    // True when a hands-on control surface is connected. Virtual ports (AVA
    // OS Octagon, IAC) and bare "USB MIDI Device" entries are always present
    // on this Mac, so "MIDI is open" never means a controller is plugged in —
    // the Show page's automatic knob takeover keys off THIS instead.
    bool hasControlSurface() const {
        for (const auto& n : m_lastDeviceNames)
            if (n.find("Launch Control") != std::string::npos || n.find("LCXL") != std::string::npos ||
                n.find("Launchpad") != std::string::npos || n.find("nanoKONTROL") != std::string::npos ||
                n.find("APC") != std::string::npos)
                return true;
        return false;
    }
    bool isLaunchControlXL3() const {
        for (const auto& n : m_lastDeviceNames)
            if (n.find("LCXL3") != std::string::npos || n.find("Launch Control XL 3") != std::string::npos)
                return true;
        return false;
    }

    // Poll events (call from main thread)
    std::vector<MIDIEvent> pollEvents();

    // Get last received CC value (normalized 0-1). Returns -1 if never received.
    // channel = -1 matches any channel.
    float getCCValue(int channel, int ccNum) const;

    // Forget every stored CC value: bindings read "no value yet" until the
    // hardware speaks again. Used by the SHOW page's Shift-reset so restored
    // defaults aren't immediately overwritten by stale knob positions.
    void clearCCValues() {
        std::lock_guard<std::mutex> lock(m_eventMutex);
        m_ccValues.clear();
    }

    // Every CC the controller has actually sent since the device opened, as
    // {channel, ccNum} sorted by (channel, cc). Powers the bind dropdown so a
    // user can pick a knob/fader instead of typing its CC number.
    std::vector<std::pair<int,int>> seenCCs() const {
        std::lock_guard<std::mutex> lock(m_eventMutex);
        std::vector<std::pair<int,int>> out;
        out.reserve(m_ccValues.size());
        for (const auto& kv : m_ccValues)
            out.push_back({ (kv.first >> 8) & 0xFF, kv.first & 0xFF });
        std::sort(out.begin(), out.end());
        return out;
    }

    // Push a parsed MIDI event into the pending queue. Thread-safe; callable from
    // platform read callbacks (e.g. CoreMIDI on macOS).
    void pushEvent(const MIDIEvent& ev);

    // MIDI Learn mode
    void startLearn() {
        std::lock_guard<std::mutex> lock(m_eventMutex);
        m_learning = true;
        m_lastLearnEvent = {};
        m_hasLearnEvent = false;
    }
    void stopLearn() { m_learning = false; }
    bool isLearning() const { return m_learning; }
    MIDIEvent lastLearnEvent() {
        std::lock_guard<std::mutex> lock(m_eventMutex);
        return m_lastLearnEvent;
    }
    // Returns true if a learn event was captured since startLearn()
    bool hasLearnEvent() const {
        std::lock_guard<std::mutex> lock(m_eventMutex);
        return m_hasLearnEvent;
    }

    // Mappings
    void addMapping(const MIDIMapping& mapping);
    void removeMapping(int index);
    std::vector<MIDIMapping>& mappings() { return m_mappings; }
    const std::vector<MIDIMapping>& mappings() const { return m_mappings; }

    // Process mappings against events, returns actions to apply
    struct MappingAction {
        MIDIMapping::Target target;
        int layerIndex;
        int sceneIndex;
        float value; // 0-1 normalized
    };
    std::vector<MappingAction> processEvents(const std::vector<MIDIEvent>& events);

private:
#ifdef _WIN32
    // One handle per opened device — "All devices" (kAllDevices) opens every
    // enumerated input and pushes all of them here, since Win32 has no
    // native "open all" device id (unlike CoreMIDI's virtual "all sources").
    // Single-device mode just ends up with one entry.
    std::vector<HMIDIIN> m_midiIns;
    std::vector<int> m_midiInIds;          // device id of each m_midiIns entry
    // "All devices" mode: ports that failed to open (typically a controller
    // that was just plugged in and whose driver/midisrv endpoint isn't ready
    // yet) are retried on this timer instead of being given up on forever.
    ULONGLONG m_nextRetryMs = 0;
    bool openPort(int id);
    static void CALLBACK midiCallback(HMIDIIN hMidiIn, UINT wMsg, DWORD_PTR dwInstance,
                                       DWORD_PTR dwParam1, DWORD_PTR dwParam2);
#endif
#ifdef __APPLE__
    void* m_macMidiImpl = nullptr;
#endif
    bool m_open = false;
    int m_deviceIdx = -1;
    std::string m_deviceName;              // single-device mode: re-resolve by name after hot-plug
    std::atomic<bool> m_setupChanged{false};
    int m_lastSourceCount = -1;            // fallback hot-plug detection (poll)
    // Windows hot-plug detection: the full device name list as of the last
    // handleHotplug() call. Compared (not just counted) so swapping one
    // controller for another of the same total count still gets noticed.
    std::vector<std::string> m_lastDeviceNames;

    mutable std::mutex m_eventMutex;
    std::vector<MIDIEvent> m_pendingEvents;
    // Last received CC value per (channel, CC#) key. Key = (channel << 8) | ccNum.
    std::unordered_map<int, float> m_ccValues;

    bool m_learning = false;
    bool m_hasLearnEvent = false;
    MIDIEvent m_lastLearnEvent;

    std::vector<MIDIMapping> m_mappings;
};

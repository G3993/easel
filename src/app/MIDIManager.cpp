#include "app/MIDIManager.h"
#include <iostream>
#include <algorithm>

MIDIManager::~MIDIManager() {
    closeDevice();
}

// On macOS, listDevices/openDevice/closeDevice are defined in MIDIManager_mac.mm
#ifndef __APPLE__
std::vector<std::string> MIDIManager::listDevices() {
    std::vector<std::string> devices;
#ifdef _WIN32
    int count = midiInGetNumDevs();
    for (int i = 0; i < count; i++) {
        MIDIINCAPS caps;
        if (midiInGetDevCaps(i, &caps, sizeof(caps)) == MMSYSERR_NOERROR) {
            devices.push_back(caps.szPname);
        }
    }
#endif
    return devices;
}

#ifdef _WIN32
bool MIDIManager::openPort(int id) {
    HMIDIIN h = nullptr;
    MMRESULT result = midiInOpen(&h, id, (DWORD_PTR)midiCallback,
                                  (DWORD_PTR)this, CALLBACK_FUNCTION);
    if (result != MMSYSERR_NOERROR) return false;
    midiInStart(h);
    m_midiIns.push_back(h);
    m_midiInIds.push_back(id);
    return true;
}
#endif

bool MIDIManager::openDevice(int index) {
#ifdef _WIN32
    closeDevice();
    m_lastDeviceNames = listDevices();
    // kAllDevices (-1) is not a real Win32 device id — midiInOpen has no
    // "open everything" concept (unlike CoreMIDI's virtual all-sources
    // input), so passing -1 straight through used to silently do nothing
    // useful: it neither failed loudly nor actually subscribed to any
    // real device, leaving "Connected" showing with zero events ever
    // arriving. Open every enumerated input ourselves instead.
    if (index == kAllDevices) {
        int count = (int)m_lastDeviceNames.size();
        for (int i = 0; i < count; i++) {
            if (!openPort(i))
                std::cerr << "[MIDI] Failed to open device " << i
                          << " (" << m_lastDeviceNames[i] << "), will retry" << std::endl;
        }
        if (m_midiIns.empty()) return false;
        m_open = true;
        m_deviceIdx = kAllDevices;
        m_nextRetryMs = GetTickCount64() + 250;
        std::cout << "[MIDI] Opened all devices (" << m_midiIns.size() << "/"
                  << count << ")" << std::endl;
        return true;
    }
    if (!openPort(index)) {
        std::cerr << "[MIDI] Failed to open device " << index << std::endl;
        return false;
    }
    m_open = true;
    m_deviceIdx = index;
    // Recorded so handleHotplug() can find this same physical device again
    // by name after a topology change moves it to a different index (or
    // it's unplugged and replugged) — Win32 device indices aren't stable
    // across a hot-plug event, but the name is.
    m_deviceName = (index < (int)m_lastDeviceNames.size()) ? m_lastDeviceNames[index] : std::string();
    std::cout << "[MIDI] Opened device " << index << std::endl;
    return true;
#else
    return false;
#endif
}

void MIDIManager::handleHotplug() {
#ifdef _WIN32
    // Win32's midiIn API doesn't reliably notify an open handle when its
    // device is unplugged (behavior varies by driver, and most USB-MIDI
    // class-compliant controllers never fire it) — the handle just goes
    // quiet forever, which is exactly the "loses connection until I
    // restart Easel" bug. Detect the topology change by polling the
    // device name list instead (not just count, so swapping one
    // controller for another of the same total count is still caught).
    // Enumeration is cheap (~0.03ms once winmm is warm) so every frame is fine.
    auto current = listDevices();
    if (m_open && current != m_lastDeviceNames) {
        // Whatever was open is holding stale handle(s) to a topology that
        // no longer matches reality. Reopen right away in the same mode
        // (rather than dropping to "closed" and waiting for Application's
        // ~1s auto-connect poll) so a freshly plugged controller is live
        // immediately.
        bool wasAll = (m_deviceIdx == kAllDevices);
        std::string name = m_deviceName;
        closeDevice();
        int idx = -1;
        if (!wasAll) {
            auto it = std::find(current.begin(), current.end(), name);
            if (it != current.end()) idx = (int)(it - current.begin());
        }
        if (idx >= 0) openDevice(idx);
        else if (!current.empty()) openAll();
    } else if (m_open && m_deviceIdx == kAllDevices &&
               m_midiIns.size() < current.size() && GetTickCount64() >= m_nextRetryMs) {
        // Some ports failed to open. A just-plugged controller often shows
        // up in the device list a moment before Windows (midisrv) will let
        // it be opened; without this retry the virtual/other ports that did
        // open made us look "connected" and the controller never came up.
        for (int i = 0; i < (int)current.size(); i++) {
            if (std::find(m_midiInIds.begin(), m_midiInIds.end(), i) != m_midiInIds.end())
                continue;
            if (openPort(i))
                std::cout << "[MIDI] Opened device " << i << " (" << current[i] << ") on retry" << std::endl;
        }
        m_nextRetryMs = GetTickCount64() + 250;
    }
    m_lastDeviceNames = current;
#endif
    m_setupChanged = false;
}

void MIDIManager::closeDevice() {
#ifdef _WIN32
    for (HMIDIIN h : m_midiIns) {
        midiInStop(h);
        midiInClose(h);
    }
    m_midiIns.clear();
    m_midiInIds.clear();
#endif
    m_open = false;
    m_deviceIdx = -1;
}
#endif // !__APPLE__

#ifdef _WIN32
void CALLBACK MIDIManager::midiCallback(HMIDIIN hMidiIn, UINT wMsg, DWORD_PTR dwInstance,
                                          DWORD_PTR dwParam1, DWORD_PTR dwParam2) {
    if (wMsg != MIM_DATA) return;
    auto* self = (MIDIManager*)dwInstance;

    uint8_t status = dwParam1 & 0xFF;
    uint8_t data1 = (dwParam1 >> 8) & 0xFF;
    uint8_t data2 = (dwParam1 >> 16) & 0xFF;

    MIDIEvent evt;
    evt.channel = status & 0x0F;

    uint8_t msgType = status & 0xF0;
    if (msgType == 0xB0) {
        // Control Change
        evt.type = 0;
        evt.number = data1;
        evt.value = data2;
    } else if (msgType == 0x90) {
        // Note On (velocity 0 = note off)
        evt.type = (data2 > 0) ? 1 : 2;
        evt.number = data1;
        evt.value = data2;
    } else if (msgType == 0x80) {
        // Note Off
        evt.type = 2;
        evt.number = data1;
        evt.value = 0;
    } else {
        return; // ignore other message types
    }

    std::lock_guard<std::mutex> lock(self->m_eventMutex);
    self->m_pendingEvents.push_back(evt);

    // Cache last CC value for parameter binding (normalized 0-1)
    if (evt.type == 0) {
        int key = (evt.channel << 8) | evt.number;
        self->m_ccValues[key] = evt.value / 127.0f;
    }

    if (self->m_learning && evt.type == 0) { // only capture CC events
        self->m_lastLearnEvent = evt;
        self->m_hasLearnEvent = true;
    }
}
#endif

std::vector<MIDIEvent> MIDIManager::pollEvents() {
    handleHotplug(); // main thread: apply any device add/remove first
    std::lock_guard<std::mutex> lock(m_eventMutex);
    auto result = std::move(m_pendingEvents);
    m_pendingEvents.clear();
    return result;
}

std::string MIDIManager::currentDeviceLabel() {
    if (!m_open) return "None";
    if (m_deviceIdx == kAllDevices) return "All devices";
    auto devs = listDevices();
    if (m_deviceIdx >= 0 && m_deviceIdx < (int)devs.size()) return devs[m_deviceIdx];
    return m_deviceName.empty() ? "Connected" : m_deviceName;
}

float MIDIManager::getCCValue(int channel, int ccNum) const {
    std::lock_guard<std::mutex> lock(m_eventMutex);
    if (channel >= 0) {
        auto it = m_ccValues.find((channel << 8) | ccNum);
        return (it != m_ccValues.end()) ? it->second : -1.0f;
    }
    for (int ch = 0; ch < 16; ch++) {
        auto it = m_ccValues.find((ch << 8) | ccNum);
        if (it != m_ccValues.end()) return it->second;
    }
    return -1.0f;
}

void MIDIManager::pushEvent(const MIDIEvent& ev) {
    std::lock_guard<std::mutex> lock(m_eventMutex);
    m_pendingEvents.push_back(ev);
    if (ev.type == 0) {
        m_ccValues[(ev.channel << 8) | ev.number] = ev.value / 127.0f;
    }
    if (m_learning && ev.type == 0) {
        m_lastLearnEvent = ev;
        m_hasLearnEvent = true;
    }
}

void MIDIManager::addMapping(const MIDIMapping& mapping) {
    m_mappings.push_back(mapping);
}

void MIDIManager::removeMapping(int index) {
    if (index >= 0 && index < (int)m_mappings.size()) {
        m_mappings.erase(m_mappings.begin() + index);
    }
}

std::vector<MIDIManager::MappingAction> MIDIManager::processEvents(const std::vector<MIDIEvent>& events) {
    std::vector<MappingAction> actions;
    for (const auto& evt : events) {
        for (const auto& map : m_mappings) {
            // Match channel (-1 = any)
            if (map.channel >= 0 && map.channel != evt.channel) continue;
            // Match type (CC=0, Note=1)
            if (map.type == 0 && evt.type != 0) continue;
            if (map.type == 1 && evt.type != 1 && evt.type != 2) continue;
            // Match number
            if (map.number != evt.number) continue;

            MappingAction action;
            action.target = map.target;
            action.layerIndex = map.layerIndex;
            action.sceneIndex = map.sceneIndex;
            action.value = evt.value / 127.0f;
            actions.push_back(action);
        }
    }
    return actions;
}

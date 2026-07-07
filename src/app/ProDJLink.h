#pragma once
#include <string>
#include <vector>
#include <unordered_map>
#include <atomic>
#include <thread>
#include <mutex>
#include <functional>
#include <cstdint>

// Pro DJ Link network protocol implementation.
// Binds to UDP 50000 (device announcements) and UDP 50001 (CDJ status packets).
// Sends a virtual CDJ announcement so Pioneer players include us in their
// broadcast radius and send full status data.
//
// Protocol reference: https://djl-analysis.deepsymmetry.org/djl-analysis/
// Offset constants cross-checked against james-elliott/beat-link (Java).

class ProDJLink {
public:
    ProDJLink() = default;
    ~ProDJLink() { stop(); }

    bool start();   // bind sockets, start listener thread; returns false on error
    void stop();    // signal thread, close sockets
    void poll();    // call once per frame on main thread — fires callbacks

    enum class SlotType : uint8_t {
        NoTrack = 0, CdDa = 1, UsbA = 2, UsbB = 3, Sd = 4, Rekordbox = 5, Unknown = 0xFF
    };

    struct DeviceInfo {
        uint8_t  playerNumber = 0;
        std::string name;
        uint8_t  ip[4]  = {};
        uint8_t  mac[6] = {};
        uint16_t deviceType = 0;  // 1=CDJ/XDJ, 2=DJM, 3=RB laptop
        double   lastSeen   = 0.0;
        bool     active     = false;
    };

    struct PlayerStatus {
        uint8_t  playerNumber = 0;

        // Transport state
        bool     playing  = false;
        bool     onAir    = false;
        bool     syncOn   = false;
        bool     master   = false;
        bool     looping  = false;
        bool     cueing   = false;

        // Tempo
        float    bpm      = 0.0f;  // effective (pitch-adjusted) BPM
        float    trackBpm = 0.0f;  // native track BPM
        float    pitchPct = 0.0f;  // pitch adjustment in percent

        // Position
        uint8_t  beatInBar  = 1;    // 1-4
        uint32_t beatCount  = 0;    // total beats since start
        float    playheadPct = 0.0f; // 0..1 fractional position in track
        uint32_t playheadMs  = 0;    // milliseconds into track

        // Track identity
        uint32_t trackNumber = 0;
        SlotType slot        = SlotType::NoTrack;
        uint8_t  trackType   = 0;

        // Metadata (fetched via TCP port 12523)
        std::string trackTitle;
        std::string artistName;
        std::string albumName;

        // Simplified waveform: 400 entries, each 0-31 height + color hint
        std::vector<uint8_t>  waveformHeight; // 0..31
        std::vector<uint32_t> waveformColor;  // RGBA
        bool waveformLoaded = false;

        double lastUpdate = 0.0;
    };

    bool isRunning() const { return m_running.load(); }

    // Thread-safe snapshots for reading on the main/render thread
    std::vector<DeviceInfo>               getDevices()        const;
    std::unordered_map<int,PlayerStatus>  getPlayerStatuses() const;
    int                                   deviceCount()       const;

    // Optional callback fired from poll() (main thread) on every status update
    std::function<void(const PlayerStatus&)> onStatusUpdate;

private:
    static constexpr uint16_t kAnnouncePort = 50000;
    static constexpr uint16_t kStatusPort   = 50001;
    static constexpr uint8_t  kMagic[6]     = {0x51,0x73,0x70,0x74,0x1F,0x04};

    // CDJ Status packet type IDs
    static constexpr uint8_t kTypeAnnounce = 0x06;
    static constexpr uint8_t kTypeCDJStatus = 0x29;
    static constexpr uint8_t kTypeMixerStatus = 0x26;
    static constexpr uint8_t kTypeBeat = 0x28;

    // Key byte offsets within CDJ status packet (type 0x29)
    // From djl-analysis + beat-link CDJStatus.java
    static constexpr int kOffPlayerNum   = 0x21;  // 33
    static constexpr int kOffTrackNum    = 0x2C;  // 44 — 4 bytes BE
    static constexpr int kOffSlot        = 0x28;  // 40
    static constexpr int kOffTrackType   = 0x2A;  // 42
    static constexpr int kOffTrackBpm    = 0x5C;  // 92  — 4 bytes BE, /100
    static constexpr int kOffPitch       = 0x60;  // 96  — 4 bytes BE, 0x100000=1.0
    static constexpr int kOffBeatInBar   = 0x55;  // 85
    static constexpr int kOffBeatCount   = 0xA0;  // 160 — 4 bytes BE
    static constexpr int kOffPlayState   = 0x89;  // 137
    static constexpr int kOffFlags       = 0x8B;  // 139
    // Play state flags (kOffPlayState)
    static constexpr uint8_t kFlagPlaying = 0x40;
    static constexpr uint8_t kFlagLooping = 0x02;
    // Flags byte (kOffFlags)
    static constexpr uint8_t kFlagMaster  = 0x20;
    static constexpr uint8_t kFlagSync    = 0x10;
    static constexpr uint8_t kFlagOnAir   = 0x08;

#ifdef _WIN32
    using sock_t = uintptr_t; // SOCKET = UINT_PTR
    static constexpr uintptr_t kInvalidSock = ~(uintptr_t)0; // INVALID_SOCKET
#else
    using sock_t = int;
    static constexpr int kInvalidSock = -1;
#endif

    sock_t m_announceSock = kInvalidSock;
    sock_t m_statusSock   = kInvalidSock;

    std::atomic<bool> m_running{false};
    std::thread       m_thread;
    uint8_t           m_virtualPlayerNum = 5;
    uint32_t          m_announceSeq      = 0;

    mutable std::mutex m_mutex;
    std::vector<DeviceInfo>              m_devices;
    std::unordered_map<int,PlayerStatus> m_statuses;

    // Events queued from listener thread, consumed by poll()
    struct Event { PlayerStatus status; };
    std::vector<Event> m_pendingEvents;
    std::mutex         m_eventMutex;

    void listenerLoop();
    void sendVirtualAnnounce();
    void handleAnnouncePacket(const uint8_t* d, int len);
    void handleStatusPacket(const uint8_t* d, int len);

    static bool   checkMagic(const uint8_t* d, int len);
    static uint32_t u32be(const uint8_t* p);
    static uint16_t u16be(const uint8_t* p);
};

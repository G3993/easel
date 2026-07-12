#include "app/ProDJLink.h"
#include <cstring>
#include <cmath>
#include <chrono>
#include <algorithm>
#include <iostream>

#ifdef _WIN32
#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif
#include <winsock2.h>
#include <ws2tcpip.h>
#pragma comment(lib, "ws2_32.lib")
typedef SOCKET platform_sock_t;
#define INVALID_PLATFORM_SOCK INVALID_SOCKET
static void closeSock(SOCKET s) { closesocket(s); }
#else
#include <sys/socket.h>
#include <sys/select.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <unistd.h>
#include <fcntl.h>
typedef int platform_sock_t;
#define INVALID_PLATFORM_SOCK (-1)
static void closeSock(int s) { close(s); }
#endif

static double nowSec() {
    using namespace std::chrono;
    return duration_cast<duration<double>>(
        steady_clock::now().time_since_epoch()).count();
}

// ─── helpers ────────────────────────────────────────────────────────────────

bool ProDJLink::checkMagic(const uint8_t* d, int len) {
    if (len < 7) return false;
    return memcmp(d, kMagic, 6) == 0;
}
uint32_t ProDJLink::u32be(const uint8_t* p) {
    return ((uint32_t)p[0]<<24)|((uint32_t)p[1]<<16)|((uint32_t)p[2]<<8)|p[3];
}
uint16_t ProDJLink::u16be(const uint8_t* p) {
    return ((uint16_t)p[0]<<8)|p[1];
}

// ─── start / stop ────────────────────────────────────────────────────────────

bool ProDJLink::start() {
#ifdef _WIN32
    WSADATA wsa;
    if (WSAStartup(MAKEWORD(2,2), &wsa) != 0) return false;
#endif

    // Announce socket — bind to port 50000, enable broadcast
    auto mkUDP = [](uint16_t port) -> platform_sock_t {
        platform_sock_t s = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
        if (s == INVALID_PLATFORM_SOCK) return INVALID_PLATFORM_SOCK;

        int yes = 1;
#ifdef _WIN32
        setsockopt(s, SOL_SOCKET, SO_REUSEADDR,  (const char*)&yes, sizeof(yes));
        setsockopt(s, SOL_SOCKET, SO_BROADCAST,  (const char*)&yes, sizeof(yes));
        // Non-blocking
        u_long nb = 1;
        ioctlsocket(s, FIONBIO, &nb);
#else
        setsockopt(s, SOL_SOCKET, SO_REUSEADDR,  &yes, sizeof(yes));
        setsockopt(s, SOL_SOCKET, SO_REUSEPORT,  &yes, sizeof(yes));
        setsockopt(s, SOL_SOCKET, SO_BROADCAST,  &yes, sizeof(yes));
        fcntl(s, F_SETFL, O_NONBLOCK);
#endif
        sockaddr_in addr{};
        addr.sin_family      = AF_INET;
        addr.sin_port        = htons(port);
        addr.sin_addr.s_addr = INADDR_ANY;
        if (bind(s, (sockaddr*)&addr, sizeof(addr)) != 0) {
            closeSock(s);
            return INVALID_PLATFORM_SOCK;
        }
        return s;
    };

    m_announceSock = (sock_t)mkUDP(kAnnouncePort);
    m_statusSock   = (sock_t)mkUDP(kStatusPort);

    if (m_announceSock == kInvalidSock && m_statusSock == kInvalidSock) {
        std::cerr << "[ProDJLink] Failed to bind any sockets\n";
        return false;
    }

    m_running = true;
    m_thread = std::thread(&ProDJLink::listenerLoop, this);
    return true;
}

void ProDJLink::stop() {
    m_running = false;
    if (m_announceSock != kInvalidSock) {
        closeSock((platform_sock_t)m_announceSock);
        m_announceSock = kInvalidSock;
    }
    if (m_statusSock != kInvalidSock) {
        closeSock((platform_sock_t)m_statusSock);
        m_statusSock = kInvalidSock;
    }
    if (m_thread.joinable()) m_thread.join();
#ifdef _WIN32
    WSACleanup();
#endif
}

void ProDJLink::poll() {
    std::vector<Event> events;
    {
        std::lock_guard<std::mutex> lk(m_eventMutex);
        events.swap(m_pendingEvents);
    }
    for (auto& ev : events) {
        if (onStatusUpdate) onStatusUpdate(ev.status);
    }
    // Expire devices not seen for > 8 seconds
    double now = nowSec();
    std::lock_guard<std::mutex> lk(m_mutex);
    for (auto& d : m_devices) {
        if (d.active && (now - d.lastSeen) > 8.0) d.active = false;
    }
}

// ─── thread ──────────────────────────────────────────────────────────────────

void ProDJLink::listenerLoop() {
    uint8_t buf[2048];
    double lastAnnounce = 0.0;

    while (m_running) {
        double now = nowSec();

        // Send virtual CDJ announcement every 1.5 s
        if (now - lastAnnounce > 1.5) {
            sendVirtualAnnounce();
            lastAnnounce = now;
        }

        // select() with 50ms timeout
        fd_set fds;
        FD_ZERO(&fds);
        int maxFd = 0;
        if (m_announceSock != kInvalidSock) {
            FD_SET((platform_sock_t)m_announceSock, &fds);
            if ((int)m_announceSock > maxFd) maxFd = (int)m_announceSock;
        }
        if (m_statusSock != kInvalidSock) {
            FD_SET((platform_sock_t)m_statusSock, &fds);
            if ((int)m_statusSock > maxFd) maxFd = (int)m_statusSock;
        }

        timeval tv{0, 50000}; // 50 ms
        int r = select(maxFd + 1, &fds, nullptr, nullptr, &tv);
        if (r <= 0) continue;

        auto readSock = [&](sock_t s) {
            if (s == kInvalidSock) return;
            if (!FD_ISSET((platform_sock_t)s, &fds)) return;
            sockaddr_in from{};
#ifdef _WIN32
            int fromLen = sizeof(from);
#else
            socklen_t fromLen = sizeof(from);
#endif
            int n = recvfrom((platform_sock_t)s, (char*)buf, sizeof(buf), 0,
                             (sockaddr*)&from, &fromLen);
            if (n <= 0) return;
            if (!checkMagic(buf, n)) return;
            uint8_t type = buf[6];
            if (type == kTypeAnnounce)   handleAnnouncePacket(buf, n);
            else if (type == kTypeCDJStatus) handleStatusPacket(buf, n);
        };

        readSock(m_announceSock);
        readSock(m_statusSock);
    }
}

// ─── virtual announcement ────────────────────────────────────────────────────

void ProDJLink::sendVirtualAnnounce() {
    if (m_announceSock == kInvalidSock) return;

    // Build announcement packet (54 bytes)
    uint8_t pkt[54] = {};
    memcpy(pkt, kMagic, 6);
    pkt[6] = kTypeAnnounce;
    pkt[7] = 0x00;

    // Device name: "Easel" (null-padded to 20 bytes)
    strncpy((char*)pkt + 8, "Easel", 19);
    pkt[28] = 0x01;
    pkt[29] = m_virtualPlayerNum;

    // Packet length = 54 (0x36)
    pkt[30] = 0x00;
    pkt[31] = 0x36;

    // IP — all zeros (we don't need the devices to talk back via unicast for now)
    // Counter
    uint32_t seq = ++m_announceSeq;
    pkt[50] = (seq >> 24) & 0xFF;
    pkt[51] = (seq >> 16) & 0xFF;
    pkt[52] = (seq >>  8) & 0xFF;
    pkt[53] = (seq      ) & 0xFF;

    sockaddr_in dest{};
    dest.sin_family      = AF_INET;
    dest.sin_port        = htons(kAnnouncePort);
    dest.sin_addr.s_addr = INADDR_BROADCAST;
    sendto((platform_sock_t)m_announceSock, (const char*)pkt, sizeof(pkt), 0,
           (sockaddr*)&dest, sizeof(dest));
}

// ─── packet parsers ──────────────────────────────────────────────────────────

void ProDJLink::handleAnnouncePacket(const uint8_t* d, int len) {
    if (len < 54) return;

    DeviceInfo dev;
    dev.lastSeen    = nowSec();
    dev.active      = true;
    dev.playerNumber = d[29]; // byte 29 in announce packet
    // Name: bytes 8..27
    char name[21] = {};
    memcpy(name, d + 8, 20);
    dev.name = name;
    // IP at bytes 36..39
    if (len >= 44) {
        memcpy(dev.ip,  d + 36, 4);
        memcpy(dev.mac, d + 40, 6);
    }
    if (len >= 48) dev.deviceType = u16be(d + 46);

    std::lock_guard<std::mutex> lk(m_mutex);
    for (auto& existing : m_devices) {
        if (existing.playerNumber == dev.playerNumber) {
            existing = dev;
            return;
        }
    }
    m_devices.push_back(dev);
}

void ProDJLink::handleStatusPacket(const uint8_t* d, int len) {
    // CDJ status packets are 0xD4 (212) bytes for modern firmware
    if (len < 0x8C) return;

    PlayerStatus ps;
    ps.lastUpdate   = nowSec();
    ps.playerNumber = (uint8_t)d[kOffPlayerNum];

    // Track info
    if (len > kOffTrackNum + 3)
        ps.trackNumber = u32be(d + kOffTrackNum);
    if (len > kOffSlot)
        ps.slot = (SlotType)d[kOffSlot];
    if (len > kOffTrackType)
        ps.trackType = d[kOffTrackType];

    // BPM — uint32 BE at 0x5C, stored * 100 (e.g. 12800 = 128.00 BPM)
    if (len > kOffTrackBpm + 3) {
        uint32_t raw = u32be(d + kOffTrackBpm);
        if (raw != 0xFFFFFFFFu && raw != 0)
            ps.trackBpm = raw / 100.0f;
    }

    // Pitch — uint32 BE at 0x60, nominal 0x100000 = 0% pitch
    if (len > kOffPitch + 3) {
        uint32_t raw = u32be(d + kOffPitch);
        // Map to percent: (raw - 0x100000) / 0x100000 * 100
        ps.pitchPct = ((float)((int32_t)raw - 0x100000) / 0x100000f) * 100.0f;
    }

    // Effective BPM after pitch adjustment
    ps.bpm = ps.trackBpm * (1.0f + ps.pitchPct / 100.0f);
    if (ps.bpm < 0.0f) ps.bpm = ps.trackBpm;

    // Beat in bar (1-4)
    if (len > kOffBeatInBar)
        ps.beatInBar = d[kOffBeatInBar];
    if (ps.beatInBar < 1 || ps.beatInBar > 4) ps.beatInBar = 1;

    // Beat count
    if (len > kOffBeatCount + 3)
        ps.beatCount = u32be(d + kOffBeatCount);

    // Play state
    if (len > kOffPlayState) {
        uint8_t st = d[kOffPlayState];
        ps.playing = (st & kFlagPlaying) != 0;
        ps.looping = (st & kFlagLooping) != 0;
    }

    // Flags: master/sync/on-air
    if (len > kOffFlags) {
        uint8_t fl = d[kOffFlags];
        ps.master  = (fl & kFlagMaster) != 0;
        ps.syncOn  = (fl & kFlagSync)   != 0;
        ps.onAir   = (fl & kFlagOnAir)  != 0;
    }

    // Preserve metadata and waveform from previous status
    {
        std::lock_guard<std::mutex> lk(m_mutex);
        auto it = m_statuses.find(ps.playerNumber);
        if (it != m_statuses.end()) {
            ps.trackTitle   = it->second.trackTitle;
            ps.artistName   = it->second.artistName;
            ps.albumName    = it->second.albumName;
            ps.waveformHeight = it->second.waveformHeight;
            ps.waveformColor  = it->second.waveformColor;
            ps.waveformLoaded = it->second.waveformLoaded;
        }
        m_statuses[ps.playerNumber] = ps;
    }

    // Queue event for poll()
    std::lock_guard<std::mutex> elk(m_eventMutex);
    // Replace any existing pending event for this player (keep only latest)
    for (auto& ev : m_pendingEvents) {
        if (ev.status.playerNumber == ps.playerNumber) {
            ev.status = ps;
            return;
        }
    }
    m_pendingEvents.push_back({ps});
}

// ─── public accessors ────────────────────────────────────────────────────────

std::vector<ProDJLink::DeviceInfo> ProDJLink::getDevices() const {
    std::lock_guard<std::mutex> lk(m_mutex);
    return m_devices;
}

std::unordered_map<int,ProDJLink::PlayerStatus> ProDJLink::getPlayerStatuses() const {
    std::lock_guard<std::mutex> lk(m_mutex);
    return m_statuses;
}

int ProDJLink::deviceCount() const {
    std::lock_guard<std::mutex> lk(m_mutex);
    int n = 0;
    for (auto& d : m_devices) if (d.active) n++;
    return n;
}

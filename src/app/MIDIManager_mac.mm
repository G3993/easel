#ifdef __APPLE__
#include "app/MIDIManager.h"
#include <CoreMIDI/CoreMIDI.h>
#include <iostream>
#include <string>
#include <vector>

struct MacMIDIImpl {
    MIDIClientRef client = 0;
    MIDIPortRef inputPort = 0;
    MIDIManager* manager = nullptr;
    std::vector<MIDIEndpointRef> connected; // sources currently attached to inputPort
};

static void midiNotifyProc(const MIDINotification* notification, void* refCon) {
    // Delivered on the run loop of the thread that created the client (main).
    // Just flag it — the main thread reconciles sources in handleHotplug().
    auto* impl = static_cast<MacMIDIImpl*>(refCon);
    if (!impl || !impl->manager || !notification) return;
    switch (notification->messageID) {
        case kMIDIMsgSetupChanged:
        case kMIDIMsgObjectAdded:
        case kMIDIMsgObjectRemoved:
            impl->manager->markSetupChanged();
            break;
        default: break;
    }
}

static std::string sourceName(MIDIEndpointRef src, ItemCount i) {
    CFStringRef name = nullptr;
    MIDIObjectGetStringProperty(src, kMIDIPropertyDisplayName, &name);
    if (name) {
        char buf[256];
        CFStringGetCString(name, buf, sizeof(buf), kCFStringEncodingUTF8);
        CFRelease(name);
        return buf;
    }
    return "MIDI Source " + std::to_string(i);
}

static void disconnectAll(MacMIDIImpl* impl) {
    for (MIDIEndpointRef src : impl->connected) MIDIPortDisconnectSource(impl->inputPort, src);
    impl->connected.clear();
}

static void midiReadProc(const MIDIPacketList* pktList, void* readProcRefCon, void* srcConnRefCon) {
    auto* impl = static_cast<MacMIDIImpl*>(readProcRefCon);
    if (!impl || !impl->manager) return;

    const MIDIPacket* packet = &pktList->packet[0];
    for (UInt32 i = 0; i < pktList->numPackets; i++) {
        for (UInt16 j = 0; j < packet->length; ) {
            uint8_t status = packet->data[j];
            if (status < 0x80) { j++; continue; } // skip data bytes

            uint8_t channel = status & 0x0F;
            uint8_t msgType = status & 0xF0;

            MIDIEvent ev;
            ev.channel = channel;

            if (msgType == 0xB0 && j + 2 < packet->length) { // CC
                ev.type = 0;
                ev.number = packet->data[j + 1];
                ev.value = packet->data[j + 2];
                impl->manager->pushEvent(ev);
                j += 3;
            } else if (msgType == 0x90 && j + 2 < packet->length) { // Note On
                ev.type = (packet->data[j + 2] > 0) ? 1 : 2;
                ev.number = packet->data[j + 1];
                ev.value = packet->data[j + 2];
                impl->manager->pushEvent(ev);
                j += 3;
            } else if (msgType == 0x80 && j + 2 < packet->length) { // Note Off
                ev.type = 2;
                ev.number = packet->data[j + 1];
                ev.value = packet->data[j + 2];
                impl->manager->pushEvent(ev);
                j += 3;
            } else {
                j++;
                continue;
            }
        }
        packet = MIDIPacketNext(packet);
    }
}

std::vector<std::string> MIDIManager::listDevices() {
    std::vector<std::string> devices;
    ItemCount sourceCount = MIDIGetNumberOfSources();
    for (ItemCount i = 0; i < sourceCount; i++) devices.push_back(sourceName(MIDIGetSource(i), i));
    return devices;
}

// Lazily create the CoreMIDI client + input port (kept for the manager's
// lifetime so hot-plug reconnects don't churn clients).
static MacMIDIImpl* ensureImpl(MIDIManager* self, void*& slot) {
    if (slot) return static_cast<MacMIDIImpl*>(slot);
    auto* impl = new MacMIDIImpl();
    impl->manager = self;
    OSStatus status = MIDIClientCreate(CFSTR("Easel"), midiNotifyProc, impl, &impl->client);
    if (status != noErr) {
        std::cerr << "[MIDI] Failed to create MIDI client: " << status << std::endl;
        delete impl;
        return nullptr;
    }
    status = MIDIInputPortCreate(impl->client, CFSTR("Easel Input"), midiReadProc, impl, &impl->inputPort);
    if (status != noErr) {
        std::cerr << "[MIDI] Failed to create input port: " << status << std::endl;
        MIDIClientDispose(impl->client);
        delete impl;
        return nullptr;
    }
    slot = impl;
    return impl;
}

static bool connectIndex(MacMIDIImpl* impl, ItemCount i) {
    MIDIEndpointRef src = MIDIGetSource(i);
    if (!src) return false;
    OSStatus status = MIDIPortConnectSource(impl->inputPort, src, nullptr);
    if (status != noErr) {
        std::cerr << "[MIDI] Failed to connect source " << i << ": " << status << std::endl;
        return false;
    }
    impl->connected.push_back(src);
    std::cout << "[MIDI] Connected " << sourceName(src, i) << std::endl;
    return true;
}

bool MIDIManager::openDevice(int index) {
    ItemCount sourceCount = MIDIGetNumberOfSources();
    if (index != kAllDevices && (index < 0 || index >= (int)sourceCount)) return false;

    auto* impl = ensureImpl(this, m_macMidiImpl);
    if (!impl) return false;
    disconnectAll(impl);

    int connected = 0;
    if (index == kAllDevices) {
        for (ItemCount i = 0; i < sourceCount; i++) if (connectIndex(impl, i)) connected++;
        m_deviceName.clear();
    } else {
        if (connectIndex(impl, (ItemCount)index)) connected++;
        m_deviceName = sourceName(MIDIGetSource(index), index);
    }
    m_lastSourceCount = (int)sourceCount;
    m_open = connected > 0;
    m_deviceIdx = m_open ? index : -1;
    if (m_open) {
        std::cout << "[MIDI] Opened " << (index == kAllDevices ? "all devices" : m_deviceName)
                  << " (" << connected << " source" << (connected == 1 ? "" : "s") << ")" << std::endl;
    }
    return m_open;
}

// Main thread. Reconcile connected sources after a device was plugged or
// unplugged. Two triggers: the CoreMIDI notify proc (needs the main run loop,
// which GLFW pumps) and a cheap source-count poll as a fallback.
void MIDIManager::handleHotplug() {
    int count = (int)MIDIGetNumberOfSources();
    bool changed = m_setupChanged.exchange(false) || (m_open && count != m_lastSourceCount);
    if (!changed) return;
    m_lastSourceCount = count;
    if (!m_open) return; // Application's auto-connect handles the closed case

    if (m_deviceIdx == kAllDevices) {
        openDevice(kAllDevices);            // re-attach every current source
        if (!m_open) std::cout << "[MIDI] All devices unplugged" << std::endl;
        return;
    }
    // Single-device mode: find the chosen device by name (indices shift).
    for (int i = 0; i < count; i++) {
        if (sourceName(MIDIGetSource(i), i) == m_deviceName) {
            // Always re-connect: indices shift, and an unplug/replug of the
            // same port yields a new endpoint ref.
            std::string keep = m_deviceName;
            openDevice(i);
            m_deviceName = keep;
            return;
        }
    }
    std::cout << "[MIDI] " << m_deviceName << " unplugged" << std::endl;
    closeDevice(); // auto-connect will pick everything back up
}

void MIDIManager::closeDevice() {
    if (m_macMidiImpl) {
        auto* impl = static_cast<MacMIDIImpl*>(m_macMidiImpl);
        disconnectAll(impl);
        if (impl->client) MIDIClientDispose(impl->client);
        delete impl;
        m_macMidiImpl = nullptr;
    }
    m_open = false;
    m_deviceIdx = -1;
    m_deviceName.clear();
}
#endif

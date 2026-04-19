#include "app/SpoutIO.h"
#include <iostream>

// --- EaselSpoutSender ---

bool EaselSpoutSender::create(const std::string& name, int width, int height) {
#ifdef HAS_SPOUT
    m_sender.SetSenderName(name.c_str());
    // SendTexture auto-creates on first call, but we track state here
    m_name = name;
    m_active = true;
    std::cout << "[Spout] Sender created: " << name << std::endl;
    return true;
#else
    (void)name; (void)width; (void)height;
    std::cout << "[Spout] Not available (Spout2 SDK not linked)" << std::endl;
    return false;
#endif
}

void EaselSpoutSender::send(GLuint texture, int width, int height) {
#ifdef HAS_SPOUT
    if (m_active && texture != 0) {
        m_sender.SendTexture(texture, GL_TEXTURE_2D, (unsigned int)width, (unsigned int)height, false);
    }
#else
    (void)texture; (void)width; (void)height;
#endif
}

void EaselSpoutSender::destroy() {
#ifdef HAS_SPOUT
    if (m_active) {
        m_sender.ReleaseSender();
    }
#endif
    m_active = false;
}

// --- EaselSpoutReceiver ---

bool EaselSpoutReceiver::connect(const std::string& name) {
#ifdef HAS_SPOUT
    if (!name.empty()) {
        m_receiver.SetReceiverName(name.c_str());
    }
    m_senderName = name;
    m_active = true;
    std::cout << "[Spout] Receiver listening" << (name.empty() ? " (any sender)" : ": " + name) << std::endl;
    return true;
#else
    (void)name;
    std::cout << "[Spout] Not available (Spout2 SDK not linked)" << std::endl;
    return false;
#endif
}

GLuint EaselSpoutReceiver::receive(int& width, int& height) {
#ifdef HAS_SPOUT
    if (!m_active) return 0;
    if (m_receiver.ReceiveTexture(m_texture, GL_TEXTURE_2D)) {
        m_width = (int)m_receiver.GetSenderWidth();
        m_height = (int)m_receiver.GetSenderHeight();
        width = m_width;
        height = m_height;
        return m_texture;
    }
    return 0;
#else
    (void)width; (void)height;
    return 0;
#endif
}

void EaselSpoutReceiver::disconnect() {
#ifdef HAS_SPOUT
    if (m_active) {
        m_receiver.ReleaseReceiver();
    }
#endif
    m_active = false;
}

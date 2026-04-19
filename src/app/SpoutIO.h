#pragma once

#include <glad/glad.h>
#include <string>

#ifdef HAS_SPOUT
#include "SpoutSender.h"
#include "SpoutReceiver.h"
#endif

class EaselSpoutSender {
public:
    bool create(const std::string& name, int width, int height);
    void send(GLuint texture, int width, int height);
    void destroy();
    bool isActive() const { return m_active; }
    const std::string& name() const { return m_name; }

private:
    std::string m_name;
    bool m_active = false;
#ifdef HAS_SPOUT
    SpoutSender m_sender;
#endif
};

class EaselSpoutReceiver {
public:
    bool connect(const std::string& name = "");
    GLuint receive(int& width, int& height);
    void disconnect();
    bool isActive() const { return m_active; }
    const std::string& senderName() const { return m_senderName; }

private:
    std::string m_senderName;
    bool m_active = false;
    GLuint m_texture = 0;
    int m_width = 0, m_height = 0;
#ifdef HAS_SPOUT
    SpoutReceiver m_receiver;
#endif
};

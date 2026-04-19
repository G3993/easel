#ifdef HAS_NDI
#include "app/NDIOutput.h"
#include <iostream>
#include <cstring>

NDIOutput::~NDIOutput() {
    destroy();
}

bool NDIOutput::create(const std::string& name) {
    destroy();

    auto& rt = NDIRuntime::instance();
    if (!rt.isAvailable()) return false;

    NDIlib_send_create_t sendCreate = {};
    sendCreate.p_ndi_name = name.c_str();
    sendCreate.clock_video = false;  // Don't block render loop; we pace frames ourselves
    sendCreate.clock_audio = false;

    m_send = rt.api()->send_create(&sendCreate);
    if (!m_send) {
        std::cerr << "[NDI] Failed to create sender" << std::endl;
        return false;
    }

    std::cout << "[NDI] Sender created: " << name << std::endl;
    return true;
}

void NDIOutput::destroy() {
    if (m_send) {
        auto& rt = NDIRuntime::instance();
        if (rt.isAvailable()) {
            rt.api()->send_destroy(m_send);
        }
        m_send = nullptr;
    }
    m_pixelBuffer[0].clear();
    m_lastW = 0;
    m_lastH = 0;
}

bool NDIOutput::hasReceivers() const {
    if (!m_send) return false;
    auto& rt = NDIRuntime::instance();
    return rt.api()->send_get_no_connections(m_send, 0) > 0;
}

void NDIOutput::send(GLuint texture, int w, int h) {
    if (!m_send || w <= 0 || h <= 0 || texture == 0) return;

    // Skip frames when no receivers are connected (avoid expensive readback)
    auto& rt = NDIRuntime::instance();
    if (rt.api()->send_get_no_connections(m_send, 0) == 0) return;

    size_t bytes = (size_t)w * h * 4;

    // Resize buffer if dimensions changed
    if (w != m_lastW || h != m_lastH) {
        m_pixelBuffer[0].resize(bytes);
        m_lastW = w;
        m_lastH = h;
    }

    // Ensure GPU has finished rendering before reading back
    glFlush();
    glFinish();

    glBindTexture(GL_TEXTURE_2D, texture);
    glGetTexImage(GL_TEXTURE_2D, 0, GL_BGRA, GL_UNSIGNED_BYTE, m_pixelBuffer[0].data());
    glBindTexture(GL_TEXTURE_2D, 0);

    // Send synchronously so every frame reaches the receiver
    NDIlib_video_frame_v2_t frame = {};
    frame.xres = w;
    frame.yres = h;
    frame.FourCC = NDIlib_FourCC_video_type_BGRA;
    frame.frame_rate_N = 120000;
    frame.frame_rate_D = 1001;
    frame.picture_aspect_ratio = (float)w / (float)h;
    frame.frame_format_type = NDIlib_frame_format_type_progressive;
    frame.p_data = m_pixelBuffer[0].data();
    frame.line_stride_in_bytes = w * 4;

    rt.api()->send_send_video_v2(m_send, &frame);
}

#endif // HAS_NDI

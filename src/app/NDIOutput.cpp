#ifdef HAS_NDI
#include "app/NDIOutput.h"
#include <iostream>
#include <cstring>
#include <chrono>


NDIOutput::~NDIOutput() {
    destroy();
}

bool NDIOutput::create(const std::string& name) {
    destroy();

    auto& rt = NDIRuntime::instance();
    if (!rt.isAvailable()) return false;

    // The NDI runtime sometimes holds a sender name briefly after a previous
    // process crashed or was killed mid-shutdown; create() then returns null
    // for the duplicate name. Retry with "Name-2", "Name-3", … on failure so
    // a relaunch always finds an open slot.
    for (int suffix = 0; suffix < 8; ++suffix) {
        std::string tryName = (suffix == 0)
            ? name
            : (name + "-" + std::to_string(suffix + 1));

        NDIlib_send_create_t sendCreate = {};
        sendCreate.p_ndi_name = tryName.c_str();
        sendCreate.clock_video = false; // we pace frames ourselves
        sendCreate.clock_audio = false;

        m_send = rt.api()->send_create(&sendCreate);
        if (m_send) {
            m_publishedName = tryName;
            std::cout << "[NDI] Sender created: " << tryName << std::endl;
            return true;
        }
    }
    std::cerr << "[NDI] Failed to create sender (all 8 name slots taken — "
                 "is another Easel still running, or did NDI runtime fail to load?)"
              << std::endl;
    return false;
}

void NDIOutput::destroy() {
    if (m_send) {
        auto& rt = NDIRuntime::instance();
        if (rt.isAvailable()) {
            rt.api()->send_destroy(m_send);
        }
        m_send = nullptr;
    }
    for (int i = 0; i < kReadbackSlots; ++i) {
        if (m_fence[i]) {
            glDeleteSync(m_fence[i]);
            m_fence[i] = nullptr;
        }
    }
    if (m_pbo[0]) {
        glDeleteBuffers(kReadbackSlots, m_pbo);
        for (int i = 0; i < kReadbackSlots; ++i) m_pbo[i] = 0;
    }
    m_pixelBuffer.clear();
    m_pboIndex = 0;
    m_lastW = 0;
    m_lastH = 0;
    m_lastSendAt = {};
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

    // Settings-driven wall-clock throttle (default 30fps; <= 0 = uncapped,
    // live-settable via OSC /easel/ndi/fps).
    const auto now = std::chrono::steady_clock::now();
    if (m_settings.targetFps > 0.0f &&
        m_lastSendAt.time_since_epoch().count() > 0) {
        const double elapsed = std::chrono::duration<double>(now - m_lastSendAt).count();
        if (elapsed < 1.0 / m_settings.targetFps) return;
    }
    size_t bytes = (size_t)w * h * 4;

    if (w != m_lastW || h != m_lastH || !m_pbo[0]) {
        for (int i = 0; i < kReadbackSlots; ++i) {
            if (m_fence[i]) {
                glDeleteSync(m_fence[i]);
                m_fence[i] = nullptr;
            }
        }
        if (m_pbo[0]) glDeleteBuffers(kReadbackSlots, m_pbo);
        glGenBuffers(kReadbackSlots, m_pbo);
        for (int i = 0; i < kReadbackSlots; ++i) {
            glBindBuffer(GL_PIXEL_PACK_BUFFER, m_pbo[i]);
            glBufferData(GL_PIXEL_PACK_BUFFER, bytes, nullptr, GL_STREAM_READ);
        }
        glBindBuffer(GL_PIXEL_PACK_BUFFER, 0);
        m_pixelBuffer.resize(bytes);
        m_pboIndex = 0;
        m_lastW = w;
        m_lastH = h;
    }

    int readPBO = m_pboIndex;
    int mapPBO = (m_pboIndex + 1) % kReadbackSlots;

    if (m_fence[readPBO]) {
        GLenum status = glClientWaitSync(m_fence[readPBO], 0, 0);
        if (status == GL_TIMEOUT_EXPIRED) return;
        glDeleteSync(m_fence[readPBO]);
        m_fence[readPBO] = nullptr;
    }

    m_lastSendAt = now;

    glBindBuffer(GL_PIXEL_PACK_BUFFER, m_pbo[readPBO]);
    glBindTexture(GL_TEXTURE_2D, texture);
    glGetTexImage(GL_TEXTURE_2D, 0, GL_BGRA, GL_UNSIGNED_BYTE, nullptr);
    glBindTexture(GL_TEXTURE_2D, 0);
    m_fence[readPBO] = glFenceSync(GL_SYNC_GPU_COMMANDS_COMPLETE, 0);

    bool hasFrame = false;
    if (m_fence[mapPBO]) {
        GLenum status = glClientWaitSync(m_fence[mapPBO], 0, 0);
        if (status == GL_ALREADY_SIGNALED || status == GL_CONDITION_SATISFIED) {
            glDeleteSync(m_fence[mapPBO]);
            m_fence[mapPBO] = nullptr;

            glBindBuffer(GL_PIXEL_PACK_BUFFER, m_pbo[mapPBO]);
            void* ptr = glMapBufferRange(GL_PIXEL_PACK_BUFFER, 0, bytes, GL_MAP_READ_BIT);
            if (ptr) {
                std::memcpy(m_pixelBuffer.data(), ptr, bytes);
                hasFrame = true;
                glUnmapBuffer(GL_PIXEL_PACK_BUFFER);
            }
        }
    }
    glBindBuffer(GL_PIXEL_PACK_BUFFER, 0);
    m_pboIndex = (m_pboIndex + 1) % kReadbackSlots;

    if (!hasFrame) return;

    NDIlib_video_frame_v2_t frame = {};
    frame.xres = w;
    frame.yres = h;
    frame.FourCC = NDIlib_FourCC_video_type_BGRA;
    // Stamp the actual pacing rather than a fixed 60.
    const float fps = m_settings.targetFps > 0.0f ? m_settings.targetFps : 60.0f;
    frame.frame_rate_N = (int)(fps * 1000.0f);
    frame.frame_rate_D = 1000;
    frame.picture_aspect_ratio = (float)w / (float)h;
    frame.frame_format_type = NDIlib_frame_format_type_progressive;
    frame.p_data = m_pixelBuffer.data();
    frame.line_stride_in_bytes = w * 4;

    rt.api()->send_send_video_v2(m_send, &frame);
}

#endif // HAS_NDI

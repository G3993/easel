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
    if (m_pbo[0]) {
        glDeleteBuffers(2, m_pbo);
        m_pbo[0] = m_pbo[1] = 0;
    }
    m_pixelBuffer[0].clear();
    m_lastW = 0;
    m_lastH = 0;
    m_pboIndex = 0;
    m_pboFilled = 0;
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

    // Crash-on-connect guard. Everything below only runs once a receiver is
    // connected, so a mismatch here is invisible until someone subscribes —
    // exactly the reported failure. The caller passes a layer/zone's logical
    // (w,h), but glGetTexImage below reads the texture's OWN level-0 size into
    // a PBO we size from (w,h). A per-layer source whose backing texture is a
    // different size (video re-alloc, downscaled sim) — or not a GL_TEXTURE_2D
    // at all (rectangle/external/hardware-decode texture) — makes that async
    // DMA overrun the PBO and fault. Read the texture's real dimensions and
    // size everything from those; bail safely if it isn't a sized 2D texture.
    if (!glIsTexture(texture)) return;
    GLint tw = 0, th = 0;
    glBindTexture(GL_TEXTURE_2D, texture);
    glGetTexLevelParameteriv(GL_TEXTURE_2D, 0, GL_TEXTURE_WIDTH,  &tw);
    glGetTexLevelParameteriv(GL_TEXTURE_2D, 0, GL_TEXTURE_HEIGHT, &th);
    glBindTexture(GL_TEXTURE_2D, 0);
    if (tw <= 0 || th <= 0) return;   // not a level-0 2D texture — skip, never read
    w = tw;
    h = th;

    size_t bytes = (size_t)w * h * 4;

    // (Re)allocate the staging buffer + both PBOs on size change / first use.
    // A size change invalidates the previously-filled PBO, so reset the fill
    // counter to re-prime before we map again.
    if (w != m_lastW || h != m_lastH || m_pbo[0] == 0) {
        m_pixelBuffer[0].resize(bytes);
        if (m_pbo[0] == 0) glGenBuffers(2, m_pbo);
        for (int i = 0; i < 2; i++) {
            glBindBuffer(GL_PIXEL_PACK_BUFFER, m_pbo[i]);
            glBufferData(GL_PIXEL_PACK_BUFFER, bytes, nullptr, GL_STREAM_READ);
        }
        glBindBuffer(GL_PIXEL_PACK_BUFFER, 0);
        m_lastW = w;
        m_lastH = h;
        m_pboIndex = 0;
        m_pboFilled = 0;
    }

    const int writeIdx = m_pboIndex;
    const int readIdx  = (m_pboIndex + 1) & 1;

    // Kick off an ASYNCHRONOUS readback of this frame's texture into writeIdx's
    // PBO. With a pack buffer bound, glGetTexImage returns immediately (the DMA
    // proceeds in the background) instead of stalling the CPU on the GPU — no
    // glFinish needed.
    glBindBuffer(GL_PIXEL_PACK_BUFFER, m_pbo[writeIdx]);
    glBindTexture(GL_TEXTURE_2D, texture);
    glGetTexImage(GL_TEXTURE_2D, 0, GL_BGRA, GL_UNSIGNED_BYTE, (void*)0);
    glBindTexture(GL_TEXTURE_2D, 0);
    m_pboFilled++;

    // Map the OTHER PBO — filled on the previous frame, so its DMA is already
    // complete and the map doesn't block. Ship that frame (one frame of
    // latency, zero render-loop stall). Skip until both PBOs are primed.
    if (m_pboFilled >= 2) {
        glBindBuffer(GL_PIXEL_PACK_BUFFER, m_pbo[readIdx]);
        void* mapped = glMapBuffer(GL_PIXEL_PACK_BUFFER, GL_READ_ONLY);
        if (mapped) {
            memcpy(m_pixelBuffer[0].data(), mapped, bytes);
            glUnmapBuffer(GL_PIXEL_PACK_BUFFER);

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
            // Guard the func ptr — older/partially-loaded NDI runtimes can leave
            // this null (the audio path already guards send_send_audio_v2).
            if (rt.api()->send_send_video_v2)
                rt.api()->send_send_video_v2(m_send, &frame);
        }
    }
    glBindBuffer(GL_PIXEL_PACK_BUFFER, 0);
    m_pboIndex = readIdx;  // ping-pong: next frame writes the buffer we just read
}

#endif // HAS_NDI

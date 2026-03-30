#ifdef HAS_NDI
#include "sources/NDISource.h"
#include <iostream>
#include <cstring>
#ifdef _WIN32
#include <winsock2.h>
#pragma comment(lib, "ws2_32.lib")
#else
#include <unistd.h>
#endif

// ── NDIFinder (persistent, accumulates sources over time) ──────────

bool NDIFinder::create() {
    if (m_finder) return true;

    auto& rt = NDIRuntime::instance();
    if (!rt.isAvailable()) return false;

    NDIlib_find_create_t findCreate = {};
    findCreate.show_local_sources = true;

    m_finder = rt.api()->find_create_v2(&findCreate);
    if (!m_finder) {
        std::cout << "[NDI] Failed to create finder" << std::endl;
        return false;
    }
    std::cout << "[NDI] Finder created (persistent)" << std::endl;
    return true;
}

void NDIFinder::destroy() {
    if (m_finder) {
        auto& rt = NDIRuntime::instance();
        if (rt.isAvailable()) {
            rt.api()->find_destroy(m_finder);
        }
        m_finder = nullptr;
    }
}

std::vector<NDISenderInfo> NDIFinder::sources() const {
    std::vector<NDISenderInfo> result;
    if (!m_finder) return result;

    auto& rt = NDIRuntime::instance();
    if (!rt.isAvailable()) return result;

    // Get local machine name for self-filtering
    std::string localHost;
    {
        char buf[256] = {};
        if (gethostname(buf, sizeof(buf)) == 0) {
            localHost = buf;
            // NDI uppercases the hostname
            for (auto& c : localHost) c = (char)toupper((unsigned char)c);
        }
    }
    std::string selfPrefix = localHost + " (Easel)";

    uint32_t count = 0;
    const NDIlib_source_t* sources = rt.api()->find_get_current_sources(m_finder, &count);

    for (uint32_t i = 0; i < count; i++) {
        NDISenderInfo info;
        info.name = sources[i].p_ndi_name ? sources[i].p_ndi_name : "";
        info.url  = sources[i].p_url_address ? sources[i].p_url_address : "";
        // Only filter out THIS machine's Easel output (not remote Easel instances)
        if (!selfPrefix.empty() && info.name == selfPrefix) continue;
        result.push_back(info);
    }
    return result;
}

// ── NDISource ──────────────────────────────────────────────────────

NDISource::~NDISource() {
    disconnect();
}

bool NDISource::connect(const std::string& senderName) {
    disconnect();

    auto& rt = NDIRuntime::instance();
    if (!rt.isAvailable()) return false;

    // Create receiver requesting RGBA format
    NDIlib_recv_create_v3_t recvCreate = {};
    recvCreate.color_format = NDIlib_recv_color_format_RGBX_RGBA;
    recvCreate.bandwidth = NDIlib_recv_bandwidth_highest;
    recvCreate.allow_video_fields = true;
    recvCreate.p_ndi_recv_name = "Easel";

    m_recv = rt.api()->recv_create_v3(&recvCreate);
    if (!m_recv) {
        std::cerr << "[NDI] Failed to create receiver" << std::endl;
        return false;
    }

    // Connect to the named source
    NDIlib_source_t source = {};
    source.p_ndi_name = senderName.c_str();
    rt.api()->recv_connect(m_recv, &source);

    m_senderName = senderName;
    std::cout << "[NDI] Connected to: " << senderName << std::endl;
    return true;
}

void NDISource::disconnect() {
    if (m_recv) {
        auto& rt = NDIRuntime::instance();
        if (rt.isAvailable()) {
            rt.api()->recv_destroy(m_recv);
        }
        m_recv = nullptr;
    }
    m_senderName.clear();
    m_width = 0;
    m_height = 0;
}

void NDISource::update() {
    if (!m_recv) return;

    auto& rt = NDIRuntime::instance();
    if (!rt.isAvailable()) return;

    // Drain the receive queue: audio/metadata frames can starve video if we
    // only call recv_capture_v3 once per update.  Loop until the queue is
    // empty, keeping only the most recent video frame.
    bool gotVideo = false;
    NDIlib_video_frame_v2_t video = {};

    for (int i = 0; i < 16; i++) {  // cap iterations to avoid infinite loop
        NDIlib_video_frame_v2_t v = {};
        NDIlib_audio_frame_v3_t audio = {};
        NDIlib_metadata_frame_t meta = {};

        NDIlib_frame_type_e type = rt.api()->recv_capture_v3(m_recv, &v, &audio, &meta, 0);

        if (type == NDIlib_frame_type_none) break;  // queue empty

        if (type == NDIlib_frame_type_video) {
            // Free previous video frame if we already had one (keep latest)
            if (gotVideo) {
                rt.api()->recv_free_video_v2(m_recv, &video);
            }
            video = v;
            gotVideo = true;
        } else if (type == NDIlib_frame_type_audio) {
            rt.api()->recv_free_audio_v3(m_recv, &audio);
        } else if (type == NDIlib_frame_type_metadata) {
            rt.api()->recv_free_metadata(m_recv, &meta);
        }
    }

    if (gotVideo && video.p_data) {
        if (video.xres != m_width || video.yres != m_height) {
            m_width = video.xres;
            m_height = video.yres;
            m_texture.createEmpty(m_width, m_height);
            m_pixelBuffer.resize(m_width * m_height * 4);
        }

        int stride = video.line_stride_in_bytes;
        if (stride <= 0) stride = m_width * 4;
        int rowBytes = m_width * 4;

        // Upload to GPU -- skip intermediate copy when stride matches
        if (stride == rowBytes) {
            // Direct upload from NDI memory (no CPU copy)
            glBindTexture(GL_TEXTURE_2D, m_texture.id());
            glPixelStorei(GL_UNPACK_ROW_LENGTH, 0);
            glTexSubImage2D(GL_TEXTURE_2D, 0, 0, 0, m_width, m_height,
                            GL_RGBA, GL_UNSIGNED_BYTE, video.p_data);
        } else {
            // Stride mismatch -- use GL_UNPACK_ROW_LENGTH to skip padding
            glBindTexture(GL_TEXTURE_2D, m_texture.id());
            glPixelStorei(GL_UNPACK_ROW_LENGTH, stride / 4);
            glTexSubImage2D(GL_TEXTURE_2D, 0, 0, 0, m_width, m_height,
                            GL_RGBA, GL_UNSIGNED_BYTE, video.p_data);
            glPixelStorei(GL_UNPACK_ROW_LENGTH, 0);
        }

        rt.api()->recv_free_video_v2(m_recv, &video);
    }
}

#endif // HAS_NDI

#pragma once
#ifdef HAS_NDI

#include "sources/NDIRuntime.h"
#include "render/Framebuffer.h"
#include <glad/glad.h>
#include <string>
#include <vector>
#include <chrono>

// Wire-format / pacing settings for an NDI sender. Applied per-frame by the
// app (see Application::ndiOutputSettings()) so changes take effect live and
// every NDIOutput instance (global + per-zone) shares one config.
struct NDIOutputSettings {
    enum class Format {
        UYVY,   // 4:2:2, 16 bpp, NO alpha — default; ~half the wire data of BGRA
        BGRA,   // 32 bpp incl. alpha — legacy/fallback for alpha-sensitive paths
    };
    Format format    = Format::UYVY;
    float  targetFps = 30.0f;   // <= 0 = uncapped (paced only by the render loop)
    int    width     = 0;       // 0 = native source width  (else scale to this)
    int    height    = 0;       // 0 = native source height (else scale to this)
};

class NDIOutput {
public:
    ~NDIOutput();

    bool create(const std::string& name = "Easel");
    void destroy();
    bool isActive() const { return m_send != nullptr; }
    bool hasReceivers() const;
    const std::string& publishedName() const { return m_publishedName; }

    // Apply wire-format / pacing settings. Cheap; call every frame before send().
    void setSettings(const NDIOutputSettings& s) { m_settings = s; }
    const NDIOutputSettings& settings() const { return m_settings; }

    // Read back the warp FBO texture and send it over NDI.
    // Call this after compositeAndWarp() each frame. Honors setSettings():
    // throttles to targetFps, converts to UYVY (GPU) and/or scales as configured.
    void send(GLuint texture, int w, int h);

private:
    NDIlib_send_instance_t m_send = nullptr;
    std::vector<uint8_t> m_pixelBuffer[1];
    int m_lastW = 0, m_lastH = 0;        // dims of the buffer actually read back (PBO sizing)
    // Double-buffered PBO async readback. glGetTexImage into one PBO is a
    // non-blocking DMA; we map the OTHER PBO (filled a frame earlier, so it's
    // ready) to ship it. This replaces the per-frame glFinish + synchronous
    // glGetTexImage that stalled the whole render loop whenever an NDI receiver
    // was connected — the cause of choppy playback while outputting.
    GLuint m_pbo[2] = {0, 0};
    int    m_pboIndex = 0;
    int    m_pboFilled = 0;  // readbacks issued since the last (re)alloc
    std::string m_publishedName;

    NDIOutputSettings m_settings;

    // GPU conversion (BGRA -> UYVY packing and/or scaling). Lazily created the
    // first time a non-trivial format/size is requested; the plain-BGRA,
    // native-resolution path skips this entirely (renders nothing, reads the
    // source texture directly, exactly as before).
    GLuint      m_convProgram = 0;
    GLuint      m_convVAO = 0;
    Framebuffer m_convFBO;
    bool ensureConvProgram();
    // Render `srcTex` into m_convFBO at the packed size, doing UYVY pack (and any
    // scale). packW/packH are the RGBA8 texel dims of the target (UYVY: outW/2).
    void runConvertPass(GLuint srcTex, int packW, int packH, int outW, int outH, bool uyvy);

    // Wall-clock fps throttle.
    std::chrono::steady_clock::time_point m_lastSend;
    bool m_haveLastSend = false;
};

#endif // HAS_NDI

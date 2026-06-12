#pragma once
#ifdef HAS_NDI

#include "sources/NDIRuntime.h"
#include <glad/glad.h>
#include <chrono>
#include <string>
#include <vector>

// Wire pacing settings for an NDI sender. Applied per-frame by the app so
// changes (OSC /easel/ndi/fps) take effect live; every NDIOutput instance
// (global + per-zone) shares one config.
struct NDIOutputSettings {
    enum class Format {
        UYVY,   // 4:2:2, 16 bpp, NO alpha — NOT YET IMPLEMENTED (send() falls back to BGRA)
        BGRA,   // 32 bpp incl. alpha
    };
    Format format    = Format::BGRA;
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

    // Apply wire pacing settings. Cheap; call every frame before send().
    void setSettings(const NDIOutputSettings& s) { m_settings = s; }
    const NDIOutputSettings& settings() const { return m_settings; }

    // Read back the warp FBO texture and send it over NDI.
    // Call this after compositeAndWarp() each frame.
    void send(GLuint texture, int w, int h);

private:
    NDIlib_send_instance_t m_send = nullptr;
    static constexpr int kReadbackSlots = 3;
    GLuint m_pbo[kReadbackSlots] = {0, 0, 0};
    GLsync m_fence[kReadbackSlots] = {nullptr, nullptr, nullptr};
    std::vector<uint8_t> m_pixelBuffer;
    int m_pboIndex = 0;
    int m_lastW = 0, m_lastH = 0;
    std::string m_publishedName;
    std::chrono::steady_clock::time_point m_lastSendAt{};
    NDIOutputSettings m_settings;
};

#endif // HAS_NDI

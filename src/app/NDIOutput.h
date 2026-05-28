#pragma once
#ifdef HAS_NDI

#include "sources/NDIRuntime.h"
#include <glad/glad.h>
#include <string>
#include <vector>

class NDIOutput {
public:
    ~NDIOutput();

    bool create(const std::string& name = "Easel");
    void destroy();
    bool isActive() const { return m_send != nullptr; }
    bool hasReceivers() const;
    const std::string& publishedName() const { return m_publishedName; }

    // Read back the warp FBO texture and send it over NDI.
    // Call this after compositeAndWarp() each frame.
    void send(GLuint texture, int w, int h);

private:
    NDIlib_send_instance_t m_send = nullptr;
    std::vector<uint8_t> m_pixelBuffer[1];
    int m_lastW = 0, m_lastH = 0;
    // Double-buffered PBO async readback. glGetTexImage into one PBO is a
    // non-blocking DMA; we map the OTHER PBO (filled a frame earlier, so it's
    // ready) to ship it. This replaces the per-frame glFinish + synchronous
    // glGetTexImage that stalled the whole render loop whenever an NDI receiver
    // was connected — the cause of choppy playback while outputting.
    GLuint m_pbo[2] = {0, 0};
    int    m_pboIndex = 0;
    int    m_pboFilled = 0;  // readbacks issued since the last (re)alloc
    std::string m_publishedName;
};

#endif // HAS_NDI

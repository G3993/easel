#pragma once
#ifdef HAS_NDI

#include "sources/NDIRuntime.h"
#include <glad/glad.h>
#include <chrono>
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
    static constexpr int kReadbackSlots = 3;
    GLuint m_pbo[kReadbackSlots] = {0, 0, 0};
    GLsync m_fence[kReadbackSlots] = {nullptr, nullptr, nullptr};
    std::vector<uint8_t> m_pixelBuffer;
    int m_pboIndex = 0;
    int m_lastW = 0, m_lastH = 0;
    std::string m_publishedName;
    std::chrono::steady_clock::time_point m_lastSendAt{};
};

#endif // HAS_NDI

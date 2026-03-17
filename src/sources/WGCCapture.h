#pragma once
#ifdef _WIN32

#include <windows.h>
#include <cstdint>
#include <vector>

// Windows Graphics Capture API wrapper (Windows 10 1903+)
// Captures windows reliably even when minimized, occluded, or GPU-accelerated.
class WGCCapture {
public:
    WGCCapture();
    ~WGCCapture();

    // Check if the WGC API is available on this system
    static bool isSupported();

    // Start capturing a window. Returns false if WGC is unavailable or HWND is invalid.
    bool start(HWND hwnd);

    // Stop capturing and release all resources.
    void stop();

    // Poll for a new frame. Returns true if new pixel data is available.
    // Pixels are RGBA, bottom-up (OpenGL-ready).
    bool update();

    const uint8_t* pixels() const { return m_pixels.data(); }
    int width() const { return m_width; }
    int height() const { return m_height; }
    bool isActive() const { return m_active; }

private:
    struct Impl;
    Impl* m_impl = nullptr;

    std::vector<uint8_t> m_pixels;
    int m_width = 0, m_height = 0;
    bool m_active = false;
};

#endif // _WIN32

#pragma once
#ifdef __APPLE__
#include "sources/ContentSource.h"
#include "render/Texture.h"
#include <vector>
#include <string>

struct CaptureMonitorInfo {
    std::string name;
    int width, height;
    int index;
};

class CaptureSource : public ContentSource {
public:
    ~CaptureSource();

    static std::vector<CaptureMonitorInfo> enumerateMonitors();

    bool start(int monitorIndex = 0);
    void stop();

    void update() override;
    GLuint textureId() const override { return m_texture.id(); }
    int width() const override { return m_width; }
    int height() const override { return m_height; }
    std::string typeName() const override { return "Screen Capture"; }
    // ScreenCaptureKit IOSurfaces are top-down and bound zero-copy
    // (CGLTexImageIOSurface2D) with no CPU pass to reorder rows — the
    // compositor must flip at draw, same as VideoSource/NDISource.
    bool isFlippedV() const override { return true; }

private:
    int m_width = 0, m_height = 0;
    Texture m_texture;
    bool m_active = false;
    void* m_impl = nullptr; // Opaque pointer to macOS capture impl
    std::vector<uint8_t> m_pixelBuffer;

    void cleanup();
};
#endif

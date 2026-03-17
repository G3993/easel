#pragma once
#include "sources/ContentSource.h"
#include "render/Texture.h"
#include "sources/WGCCapture.h"

#include <windows.h>
#include <vector>
#include <string>
#include <memory>

struct WindowInfo {
    HWND hwnd;
    std::string title;
    int width, height;
};

class WindowCaptureSource : public ContentSource {
public:
    ~WindowCaptureSource();

    static std::vector<WindowInfo> enumerateWindows();

    bool start(HWND hwnd);
    void stop();

    void update() override;
    GLuint textureId() const override { return m_texture.id(); }
    int width() const override { return m_width; }
    int height() const override { return m_height; }
    std::string typeName() const override { return "Window Capture"; }
    std::string sourcePath() const override { return m_title; }

private:
    HWND m_hwnd = nullptr;
    std::string m_title;
    int m_width = 0, m_height = 0;
    Texture m_texture;
    bool m_active = false;

    // WGC capture (preferred, works for minimized/occluded/GPU-accelerated)
    std::unique_ptr<WGCCapture> m_wgc;
    int m_wgcUpdateCount = 0;
    int m_wgcFrameCount = 0;

    // Chromium toolbar crop (shared by WGC and GDI paths)
    int m_cropTop = 0;
    HDC m_windowDC = nullptr;
    HDC m_memDC = nullptr;
    HBITMAP m_bitmap = nullptr;
    HBITMAP m_oldBitmap = nullptr;
    std::vector<uint8_t> m_pixelBuffer;

    void cleanupGDI();
    bool startGDI(HWND hwnd);
    void updateGDI();
};

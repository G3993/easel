#pragma once
#include "sources/ContentSource.h"
#include "render/Texture.h"

#include <d3d11.h>
#include <dxgi1_2.h>
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

    // Release the D3D11 device/duplication/staging while keeping the monitor
    // index; update() lazily restarts on the next live frame.
    void suspend() override;

    void update() override;
    GLuint textureId() const override { return m_texture.id(); }
    int width() const override { return m_width; }
    int height() const override { return m_height; }
    std::string typeName() const override { return "Screen Capture"; }

private:
    int m_width = 0, m_height = 0;
    Texture m_texture;

    // D3D11 resources
    ID3D11Device* m_device = nullptr;
    ID3D11DeviceContext* m_context = nullptr;
    IDXGIOutputDuplication* m_duplication = nullptr;
    // Double-buffered staging: CopyResource writes one, Map reads the OTHER
    // (last frame's copy, long since completed) so Map never blocks the
    // render thread waiting for the GPU. One frame of added latency.
    ID3D11Texture2D* m_staging[2] = {nullptr, nullptr};
    int m_stagingWrite = 0;
    bool m_stagingPrimed[2] = {false, false};

    std::vector<uint8_t> m_pixelBuffer;
    bool m_active = false;
    int m_monitorIndex = 0;
    bool m_suspended = false;

    void cleanup();
};

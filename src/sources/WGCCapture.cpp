#ifdef _WIN32
#include "sources/WGCCapture.h"
#include <iostream>
#include <fstream>
#include <sstream>

#include <d3d11.h>
#include <dxgi.h>
#include <wrl/client.h>
#include <wrl/wrappers/corewrappers.h>
#include <roapi.h>
#include <inspectable.h>

// ABI WinRT headers (from Windows SDK winrt/ directory)
#include <windows.graphics.capture.h>
#include <windows.graphics.directx.h>
#include <windows.graphics.directx.direct3d11.h>

// Interop headers (from Windows SDK um/ directory)
#include <windows.graphics.capture.interop.h>
#include <windows.graphics.directx.direct3d11.interop.h>

using Microsoft::WRL::ComPtr;
using Microsoft::WRL::Wrappers::HStringReference;

namespace ABI_Cap = ABI::Windows::Graphics::Capture;
namespace ABI_D3D = ABI::Windows::Graphics::DirectX::Direct3D11;
namespace ABI_DX  = ABI::Windows::Graphics::DirectX;
namespace ABI_F   = ABI::Windows::Foundation;

// File-based logging (WIN32 apps have no console)
static void wgcLog(const std::string& msg) {
    static std::ofstream logFile("wgc_debug.log", std::ios::app);
    logFile << msg << std::endl;
    logFile.flush();
}

struct WGCCapture::Impl {
    // D3D11
    ComPtr<ID3D11Device>        device;
    ComPtr<ID3D11DeviceContext>  context;
    ComPtr<ID3D11Texture2D>     stagingTex;
    int stagingW = 0, stagingH = 0;

    // WinRT device wrapper
    ComPtr<IInspectable> winrtDeviceInsp;

    // Capture objects (prevent Release before stop)
    ComPtr<ABI_Cap::IGraphicsCaptureItem>           item;
    ComPtr<ABI_Cap::IDirect3D11CaptureFramePool>    framePool;
    ComPtr<ABI_Cap::IGraphicsCaptureSession>        session;

    bool ensureStaging(int w, int h) {
        if (stagingTex && stagingW == w && stagingH == h) return true;
        stagingTex.Reset();

        D3D11_TEXTURE2D_DESC desc = {};
        desc.Width = w;
        desc.Height = h;
        desc.MipLevels = 1;
        desc.ArraySize = 1;
        desc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
        desc.SampleDesc.Count = 1;
        desc.Usage = D3D11_USAGE_STAGING;
        desc.CPUAccessFlags = D3D11_CPU_ACCESS_READ;

        HRESULT hr = device->CreateTexture2D(&desc, nullptr, &stagingTex);
        if (FAILED(hr)) return false;
        stagingW = w;
        stagingH = h;
        return true;
    }

};

// ─── Static: check API availability ──────────────────────────────────

bool WGCCapture::isSupported() {
    // Ensure WinRT is initialized (required for RoGetActivationFactory)
    HRESULT initHr = RoInitialize(RO_INIT_MULTITHREADED);
    // S_OK = newly initialized, S_FALSE = already initialized, both are fine
    if (FAILED(initHr)) {
        wgcLog("[WGC] RoInitialize failed: 0x" + ([&]{ std::ostringstream s; s << std::hex << initHr; return s.str(); })());
        return false;
    }

    // Try to get the GraphicsCaptureItem activation factory
    ComPtr<IActivationFactory> factory;
    HRESULT hr = RoGetActivationFactory(
        HStringReference(RuntimeClass_Windows_Graphics_Capture_GraphicsCaptureItem).Get(),
        IID_PPV_ARGS(&factory));
    bool supported = SUCCEEDED(hr) && factory;
    wgcLog(std::string("[WGC] isSupported: ") + (supported ? "YES" : "NO") +
           " (hr=0x" + ([&]{ std::ostringstream s; s << std::hex << hr; return s.str(); })() + ")");
    return supported;
}

// ─── Constructor / Destructor ────────────────────────────────────────

WGCCapture::WGCCapture() {}

WGCCapture::~WGCCapture() {
    stop();
}

// ─── Start ───────────────────────────────────────────────────────────

bool WGCCapture::start(HWND hwnd) {
    stop();

    wgcLog("[WGC] start() called for HWND " + std::to_string((uintptr_t)hwnd));

    if (!IsWindow(hwnd)) {
        wgcLog("[WGC] Invalid HWND");
        return false;
    }

    m_impl = new Impl();

    // 1. Create D3D11 device
    D3D_FEATURE_LEVEL featureLevels[] = { D3D_FEATURE_LEVEL_11_0 };
    UINT flags = D3D11_CREATE_DEVICE_BGRA_SUPPORT;
    HRESULT hr = D3D11CreateDevice(
        nullptr, D3D_DRIVER_TYPE_HARDWARE, nullptr, flags,
        featureLevels, 1, D3D11_SDK_VERSION,
        &m_impl->device, nullptr, &m_impl->context);
    if (FAILED(hr)) {
        wgcLog("[WGC] D3D11CreateDevice failed: 0x" + ([&]{ std::ostringstream s; s << std::hex << hr; return s.str(); })());
        stop(); return false;
    }
    wgcLog("[WGC] D3D11 device created");

    // 2. Wrap D3D11 device as WinRT IDirect3DDevice
    ComPtr<IDXGIDevice> dxgiDevice;
    m_impl->device.As(&dxgiDevice);
    hr = CreateDirect3D11DeviceFromDXGIDevice(dxgiDevice.Get(), &m_impl->winrtDeviceInsp);
    if (FAILED(hr)) {
        wgcLog("[WGC] CreateDirect3D11DeviceFromDXGIDevice failed");
        stop(); return false;
    }
    wgcLog("[WGC] WinRT device wrapped");

    // 3. Create GraphicsCaptureItem from HWND
    ComPtr<IActivationFactory> itemFactory;
    hr = RoGetActivationFactory(
        HStringReference(RuntimeClass_Windows_Graphics_Capture_GraphicsCaptureItem).Get(),
        IID_PPV_ARGS(&itemFactory));
    if (FAILED(hr)) {
        wgcLog("[WGC] Failed to get GraphicsCaptureItem factory");
        stop(); return false;
    }

    ComPtr<IGraphicsCaptureItemInterop> interop;
    hr = itemFactory.As(&interop);
    if (FAILED(hr)) {
        wgcLog("[WGC] Failed to get IGraphicsCaptureItemInterop");
        stop(); return false;
    }

    hr = interop->CreateForWindow(hwnd, IID_PPV_ARGS(&m_impl->item));
    if (FAILED(hr)) {
        wgcLog("[WGC] CreateForWindow failed: 0x" + ([&]{ std::ostringstream s; s << std::hex << hr; return s.str(); })());
        stop(); return false;
    }
    wgcLog("[WGC] CaptureItem created for window");

    // 4. Get capture size
    ABI::Windows::Graphics::SizeInt32 itemSize;
    m_impl->item->get_Size(&itemSize);
    wgcLog("[WGC] Item size: " + std::to_string(itemSize.Width) + "x" + std::to_string(itemSize.Height));

    // 5. Create frame pool (free-threaded — no DispatcherQueue needed)
    ComPtr<IActivationFactory> poolFactory;
    hr = RoGetActivationFactory(
        HStringReference(RuntimeClass_Windows_Graphics_Capture_Direct3D11CaptureFramePool).Get(),
        IID_PPV_ARGS(&poolFactory));
    if (FAILED(hr)) {
        wgcLog("[WGC] Failed to get FramePool factory");
        stop(); return false;
    }

    // Try CreateFreeThreaded (Windows 10 2004+), fall back to Create
    ComPtr<ABI_Cap::IDirect3D11CaptureFramePoolStatics2> poolStatics2;
    ComPtr<ABI_Cap::IDirect3D11CaptureFramePoolStatics>  poolStatics;

    ComPtr<ABI_D3D::IDirect3DDevice> winrtDevice;
    m_impl->winrtDeviceInsp.As(&winrtDevice);

    hr = poolFactory.As(&poolStatics2);
    if (SUCCEEDED(hr)) {
        hr = poolStatics2->CreateFreeThreaded(
            winrtDevice.Get(),
            ABI_DX::DirectXPixelFormat_B8G8R8A8UIntNormalized,
            2, itemSize, &m_impl->framePool);
        wgcLog("[WGC] CreateFreeThreaded: hr=0x" + ([&]{ std::ostringstream s; s << std::hex << hr; return s.str(); })());
    }

    if (FAILED(hr) || !m_impl->framePool) {
        wgcLog("[WGC] FreeThreaded failed, trying Create fallback");
        hr = poolFactory.As(&poolStatics);
        if (SUCCEEDED(hr)) {
            hr = poolStatics->Create(
                winrtDevice.Get(),
                ABI_DX::DirectXPixelFormat_B8G8R8A8UIntNormalized,
                2, itemSize, &m_impl->framePool);
        }
        if (FAILED(hr)) {
            wgcLog("[WGC] Failed to create frame pool: 0x" + ([&]{ std::ostringstream s; s << std::hex << hr; return s.str(); })());
            stop(); return false;
        }
    }
    wgcLog("[WGC] Frame pool created");

    // 6. Create capture session
    hr = m_impl->framePool->CreateCaptureSession(m_impl->item.Get(), &m_impl->session);
    if (FAILED(hr)) {
        wgcLog("[WGC] Failed to create capture session");
        stop(); return false;
    }
    wgcLog("[WGC] Capture session created");

    // 7. Disable cursor capture (Windows 10 2004+)
    ComPtr<ABI_Cap::IGraphicsCaptureSession2> session2;
    if (SUCCEEDED(m_impl->session.As(&session2))) {
        session2->put_IsCursorCaptureEnabled(false);
    }

    // 8. Start capture
    hr = m_impl->session->StartCapture();
    if (FAILED(hr)) {
        wgcLog("[WGC] StartCapture failed: 0x" + ([&]{ std::ostringstream s; s << std::hex << hr; return s.str(); })());
        stop(); return false;
    }

    m_active = true;
    wgcLog("[WGC] Capture started (" + std::to_string(itemSize.Width) + "x" + std::to_string(itemSize.Height) + ")");
    return true;
}

// ─── Stop ────────────────────────────────────────────────────────────

void WGCCapture::stop() {
    if (m_impl) {
        // Close session via IClosable
        if (m_impl->session) {
            ComPtr<ABI_F::IClosable> closable;
            if (SUCCEEDED(m_impl->session.As(&closable))) {
                closable->Close();
            }
        }
        // Close frame pool via IClosable
        if (m_impl->framePool) {
            ComPtr<ABI_F::IClosable> closable;
            if (SUCCEEDED(m_impl->framePool.As(&closable))) {
                closable->Close();
            }
        }
        delete m_impl;
        m_impl = nullptr;
    }
    m_active = false;
    m_width = 0;
    m_height = 0;
}

// ─── Update (poll for frames) ────────────────────────────────────────

bool WGCCapture::update() {
    if (!m_active || !m_impl || !m_impl->framePool) return false;

    static int pollCount = 0;
    pollCount++;

    ComPtr<ABI_Cap::IDirect3D11CaptureFrame> frame;
    HRESULT hr = m_impl->framePool->TryGetNextFrame(&frame);
    if (FAILED(hr) || !frame) {
        // Log first few null frames so we know polling is happening
        if (pollCount <= 5) {
            wgcLog("[WGC] update() poll #" + std::to_string(pollCount) + ": no frame (hr=0x" +
                   ([&]{ std::ostringstream s; s << std::hex << hr; return s.str(); })() + ")");
        }
        return false;
    }

    // Get the content size (may differ from pool size if window resized)
    ABI::Windows::Graphics::SizeInt32 contentSize;
    frame->get_ContentSize(&contentSize);

    // Get the D3D11 texture from the frame's surface
    ComPtr<ABI_D3D::IDirect3DSurface> surface;
    frame->get_Surface(&surface);
    if (!surface) {
        wgcLog("[WGC] Frame has no surface!");
        return false;
    }

    // QI for IDirect3DDxgiInterfaceAccess to get the underlying D3D11 texture
    using ::Windows::Graphics::DirectX::Direct3D11::IDirect3DDxgiInterfaceAccess;
    ComPtr<IDirect3DDxgiInterfaceAccess> dxgiAccess;
    hr = surface->QueryInterface(__uuidof(IDirect3DDxgiInterfaceAccess),
                                  reinterpret_cast<void**>(dxgiAccess.GetAddressOf()));
    if (FAILED(hr)) {
        wgcLog("[WGC] QI for IDirect3DDxgiInterfaceAccess failed");
        return false;
    }

    ComPtr<ID3D11Texture2D> frameTex;
    hr = dxgiAccess->GetInterface(__uuidof(ID3D11Texture2D),
                                   reinterpret_cast<void**>(frameTex.GetAddressOf()));
    if (FAILED(hr)) {
        wgcLog("[WGC] GetInterface for ID3D11Texture2D failed");
        return false;
    }

    // Get texture description to know actual size
    D3D11_TEXTURE2D_DESC texDesc;
    frameTex->GetDesc(&texDesc);

    static int frameCount = 0;
    if (frameCount < 5) {
        wgcLog("[WGC] Frame " + std::to_string(frameCount) +
               ": tex=" + std::to_string(texDesc.Width) + "x" + std::to_string(texDesc.Height) +
               " content=" + std::to_string(contentSize.Width) + "x" + std::to_string(contentSize.Height) +
               " fmt=" + std::to_string(texDesc.Format) +
               " usage=" + std::to_string(texDesc.Usage) +
               " bind=" + std::to_string(texDesc.BindFlags) +
               " misc=" + std::to_string(texDesc.MiscFlags));
    }

    // Use content size (actual window content, not texture padding)
    int copyW = contentSize.Width;
    int copyH = contentSize.Height;
    if (copyW <= 0 || copyH <= 0) return false;
    if (copyW > (int)texDesc.Width)  copyW = (int)texDesc.Width;
    if (copyH > (int)texDesc.Height) copyH = (int)texDesc.Height;

    // Handle resize: recreate the pool if content size changed
    if (contentSize.Width != m_impl->stagingW || contentSize.Height != m_impl->stagingH) {
        wgcLog("[WGC] Resize: " + std::to_string(m_impl->stagingW) + "x" + std::to_string(m_impl->stagingH) +
               " -> " + std::to_string(contentSize.Width) + "x" + std::to_string(contentSize.Height));
        ComPtr<ABI_D3D::IDirect3DDevice> winrtDevice;
        m_impl->winrtDeviceInsp.As(&winrtDevice);
        ABI::Windows::Graphics::SizeInt32 newSize = { contentSize.Width, contentSize.Height };
        m_impl->framePool->Recreate(
            winrtDevice.Get(),
            ABI_DX::DirectXPixelFormat_B8G8R8A8UIntNormalized,
            2, newSize);
    }

    // Ensure staging texture
    if (!m_impl->ensureStaging(copyW, copyH)) {
        wgcLog("[WGC] ensureStaging failed for " + std::to_string(copyW) + "x" + std::to_string(copyH));
        return false;
    }

    // Copy content region to staging
    D3D11_BOX srcBox;
    srcBox.left = 0;
    srcBox.top = 0;
    srcBox.right = copyW;
    srcBox.bottom = copyH;
    srcBox.front = 0;
    srcBox.back = 1;
    m_impl->context->CopySubresourceRegion(
        m_impl->stagingTex.Get(), 0, 0, 0, 0,
        frameTex.Get(), 0, &srcBox);
    m_impl->context->Flush();

    // Map staging texture and read pixels
    D3D11_MAPPED_SUBRESOURCE mapped;
    hr = m_impl->context->Map(m_impl->stagingTex.Get(), 0, D3D11_MAP_READ, 0, &mapped);
    if (FAILED(hr)) {
        wgcLog("[WGC] Map failed: 0x" + ([&]{ std::ostringstream s; s << std::hex << hr; return s.str(); })());
        return false;
    }

    // Resize output buffer if needed
    if (m_width != copyW || m_height != copyH) {
        m_width = copyW;
        m_height = copyH;
        m_pixels.resize(m_width * m_height * 4);
    }

    // Copy pixels: BGRA top-down -> RGBA bottom-up (OpenGL-ready)
    int dstStride = m_width * 4;
    for (int y = 0; y < m_height; y++) {
        const uint8_t* srcRow = (const uint8_t*)mapped.pData + y * mapped.RowPitch;
        uint8_t* dstRow = m_pixels.data() + (m_height - 1 - y) * dstStride;
        for (int x = 0; x < m_width; x++) {
            dstRow[x * 4 + 0] = srcRow[x * 4 + 2]; // R = B
            dstRow[x * 4 + 1] = srcRow[x * 4 + 1]; // G = G
            dstRow[x * 4 + 2] = srcRow[x * 4 + 0]; // B = R
            dstRow[x * 4 + 3] = 255;                // A
        }
    }

    m_impl->context->Unmap(m_impl->stagingTex.Get(), 0);

    // Debug: check if pixels are non-zero
    if (frameCount < 5) {
        uint64_t sum = 0;
        for (int i = 0; i < m_width * m_height * 4; i += 97) {
            sum += m_pixels[i];
        }
        // Also sample a few specific pixels for detailed diagnosis
        uint8_t r0 = m_pixels[0], g0 = m_pixels[1], b0 = m_pixels[2], a0 = m_pixels[3];
        int midIdx = (m_height/2 * m_width + m_width/2) * 4;
        uint8_t rM = m_pixels[midIdx], gM = m_pixels[midIdx+1], bM = m_pixels[midIdx+2];
        wgcLog("[WGC] Frame " + std::to_string(frameCount) +
               ": " + std::to_string(m_width) + "x" + std::to_string(m_height) +
               " pixelSum=" + std::to_string(sum) +
               " pitch=" + std::to_string(mapped.RowPitch) +
               " corner=(" + std::to_string(r0) + "," + std::to_string(g0) + "," + std::to_string(b0) + "," + std::to_string(a0) + ")" +
               " center=(" + std::to_string(rM) + "," + std::to_string(gM) + "," + std::to_string(bM) + ")");
        frameCount++;
    }

    return true;
}

#endif // _WIN32

#include "sources/WindowCaptureSource.h"
#include <iostream>
#include <fstream>

// Recursively find a descendant window by class name
static HWND findDescendantByClass(HWND parent, const char* className) {
    HWND child = FindWindowExA(parent, nullptr, className, nullptr);
    if (child) return child;
    // Search children recursively
    HWND next = nullptr;
    while ((next = FindWindowExA(parent, next, nullptr, nullptr)) != nullptr) {
        HWND found = findDescendantByClass(next, className);
        if (found) return found;
    }
    return nullptr;
}

// Shared log file with WGCCapture
static void winCapLog(const std::string& msg) {
    static std::ofstream logFile;
    if (!logFile.is_open()) logFile.open("wgc_debug.log", std::ios::app);
    logFile << msg << std::endl;
    logFile.flush();
}

WindowCaptureSource::~WindowCaptureSource() {
    stop();
}

static BOOL CALLBACK enumWindowsProc(HWND hwnd, LPARAM lParam) {
    auto* windows = (std::vector<WindowInfo>*)lParam;

    if (!IsWindowVisible(hwnd)) return TRUE;

    // Skip windows with no title
    int len = GetWindowTextLengthA(hwnd);
    if (len == 0) return TRUE;

    // Skip tiny windows
    RECT rect;
    GetClientRect(hwnd, &rect);
    int w = rect.right - rect.left;
    int h = rect.bottom - rect.top;
    if (w < 50 || h < 50) return TRUE;

    // Skip our own windows (Easel)
    char title[256];
    GetWindowTextA(hwnd, title, sizeof(title));
    std::string titleStr(title);
    if (titleStr == "Easel" || titleStr == "Easel Projector") return TRUE;

    // Skip taskbar, shell, etc.
    LONG exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
    if (exStyle & WS_EX_TOOLWINDOW) return TRUE;

    WindowInfo info;
    info.hwnd = hwnd;
    info.title = titleStr;
    info.width = w;
    info.height = h;
    windows->push_back(info);

    return TRUE;
}

std::vector<WindowInfo> WindowCaptureSource::enumerateWindows() {
    std::vector<WindowInfo> result;
    EnumWindows(enumWindowsProc, (LPARAM)&result);
    return result;
}

// ─── Start ───────────────────────────────────────────────────────────

bool WindowCaptureSource::start(HWND hwnd) {
    stop();

    if (!IsWindow(hwnd)) {
        std::cerr << "[WinCapture] Invalid window handle" << std::endl;
        return false;
    }

    m_hwnd = hwnd;

    // Get window title
    char title[256];
    GetWindowTextA(hwnd, title, sizeof(title));
    m_title = title;

    // Try WGC first (works for minimized, occluded, GPU-accelerated windows)
    winCapLog("[WinCapture] Checking WGC support...");
    if (WGCCapture::isSupported()) {
        winCapLog("[WinCapture] WGC supported, attempting start...");
        m_wgc = std::make_unique<WGCCapture>();
        if (m_wgc->start(hwnd)) {
            // Get initial size from client rect
            RECT clientRect;
            GetClientRect(hwnd, &clientRect);
            m_width = clientRect.right - clientRect.left;
            m_height = clientRect.bottom - clientRect.top;
            if (m_width <= 0 || m_height <= 0) {
                m_width = 1; m_height = 1;
            }
            m_texture.createEmpty(m_width, m_height);
            m_active = true;
            winCapLog("[WinCapture] Using WGC for: " + m_title + " (" + std::to_string(m_width) + "x" + std::to_string(m_height) + ")");
            return true;
        }
        winCapLog("[WinCapture] WGC start failed, falling back to GDI");
        m_wgc.reset();
    } else {
        winCapLog("[WinCapture] WGC not supported, using GDI");
    }

    // Fall back to GDI
    winCapLog("[WinCapture] Starting GDI capture for: " + m_title);
    return startGDI(hwnd);
}

// ─── Stop ────────────────────────────────────────────────────────────

void WindowCaptureSource::stop() {
    m_active = false;
    m_wgc.reset();
    cleanupGDI();
}

// ─── Update ──────────────────────────────────────────────────────────

void WindowCaptureSource::update() {
    if (!m_active || !m_hwnd) return;

    // Check if window still exists
    if (!IsWindow(m_hwnd)) {
        std::cerr << "[WinCapture] Window was closed" << std::endl;
        m_active = false;
        return;
    }

    if (m_wgc) {
        // WGC path
        m_wgcUpdateCount++;
        if (m_wgc->update()) {
            m_wgcFrameCount++;
            int w = m_wgc->width();
            int h = m_wgc->height();

            // Detect Chromium content child to auto-crop toolbar (tabs + address bar)
            // WGC captures the full window visual; the content area starts below the toolbar.
            // Pixels are stored bottom-up (OpenGL), so toolbar rows are at the END of the buffer.
            int wgcCropTop = 0;
            if (m_wgcFrameCount == 1 && m_hwnd) {
                HWND contentChild = findDescendantByClass(m_hwnd, "Chrome_RenderWidgetHostHWND");
                if (contentChild && IsWindowVisible(contentChild)) {
                    RECT childRect, parentRect;
                    GetWindowRect(contentChild, &childRect);
                    GetWindowRect(m_hwnd, &parentRect);
                    int parentH = parentRect.bottom - parentRect.top;
                    if (parentH > 0) {
                        int logicalCrop = childRect.top - parentRect.top;
                        // Convert to physical pixels using WGC/logical ratio
                        wgcCropTop = (int)((float)logicalCrop / (float)parentH * (float)h + 0.5f);
                        if (wgcCropTop < 0 || wgcCropTop >= h / 2) wgcCropTop = 0;
                        if (wgcCropTop > 0) m_cropTop = wgcCropTop;
                    }
                }
                winCapLog("[WinCapture] WGC first frame: " + std::to_string(w) + "x" + std::to_string(h) +
                          " cropTop=" + std::to_string(m_cropTop));
            }

            // Apply crop: skip toolbar rows from the top of the captured image.
            // Pixels are bottom-up, so content rows are at the start of the buffer.
            int effectiveH = h - m_cropTop;
            if (effectiveH <= 0) effectiveH = h;

            if (w > 0 && effectiveH > 0) {
                if (w != m_width || effectiveH != m_height) {
                    m_width = w;
                    m_height = effectiveH;
                    m_texture.createEmpty(m_width, m_height);
                }
                m_texture.updateData(m_wgc->pixels(), m_width, m_height);
            }
        } else if (m_wgcUpdateCount <= 5) {
            winCapLog("[WinCapture] WGC update returned false (poll " + std::to_string(m_wgcUpdateCount) + ")");
        }
    } else {
        // GDI fallback
        updateGDI();
    }
}

// ─── GDI fallback ────────────────────────────────────────────────────

bool WindowCaptureSource::startGDI(HWND hwnd) {
    RECT rect;
    GetClientRect(hwnd, &rect);
    m_width = rect.right - rect.left;
    m_height = rect.bottom - rect.top;

    if (m_width <= 0 || m_height <= 0) {
        std::cerr << "[WinCapture] Window has no client area" << std::endl;
        return false;
    }

    m_windowDC = GetDC(hwnd);
    if (!m_windowDC) {
        std::cerr << "[WinCapture] Failed to get window DC" << std::endl;
        cleanupGDI();
        return false;
    }

    m_memDC = CreateCompatibleDC(m_windowDC);
    m_bitmap = CreateCompatibleBitmap(m_windowDC, m_width, m_height);
    m_oldBitmap = (HBITMAP)SelectObject(m_memDC, m_bitmap);

    // Detect Chromium content child to auto-crop toolbar
    m_cropTop = 0;
    HWND contentChild = findDescendantByClass(hwnd, "Chrome_RenderWidgetHostHWND");
    if (contentChild && IsWindowVisible(contentChild)) {
        RECT childRect, parentRect;
        GetWindowRect(contentChild, &childRect);
        GetWindowRect(hwnd, &parentRect);
        int crop = childRect.top - parentRect.top;
        if (crop > 0 && crop < m_height / 2)
            m_cropTop = crop;
    }

    m_pixelBuffer.resize(m_width * m_height * 4);
    m_texture.createEmpty(m_width, m_height - m_cropTop);

    m_active = true;
    std::cout << "[WinCapture] Using GDI for: " << m_title
              << " (" << m_width << "x" << (m_height - m_cropTop) << ")" << std::endl;
    return true;
}

void WindowCaptureSource::updateGDI() {
    // Recheck crop (toolbar can change, e.g. bookmarks bar toggle)
    int newCrop = 0;
    HWND contentChild = findDescendantByClass(m_hwnd, "Chrome_RenderWidgetHostHWND");
    if (contentChild && IsWindowVisible(contentChild)) {
        RECT childRect, parentRect;
        GetWindowRect(contentChild, &childRect);
        GetWindowRect(m_hwnd, &parentRect);
        int crop = childRect.top - parentRect.top;
        if (crop > 0 && crop < m_height / 2)
            newCrop = crop;
    }

    // Check for resize or crop change
    RECT clientRect;
    GetClientRect(m_hwnd, &clientRect);
    int newW = clientRect.right - clientRect.left;
    int newH = clientRect.bottom - clientRect.top;

    if (newW != m_width || newH != m_height || newCrop != m_cropTop) {
        m_width = newW;
        m_height = newH;
        m_cropTop = newCrop;
        if (m_width <= 0 || m_height <= 0) return;

        SelectObject(m_memDC, m_oldBitmap);
        DeleteObject(m_bitmap);
        m_bitmap = CreateCompatibleBitmap(m_windowDC, m_width, m_height);
        m_oldBitmap = (HBITMAP)SelectObject(m_memDC, m_bitmap);

        m_pixelBuffer.resize(m_width * m_height * 4);
        m_texture.createEmpty(m_width, m_height - m_cropTop);
    }

    // Capture client area only (works even when occluded)
    if (!PrintWindow(m_hwnd, m_memDC, PW_RENDERFULLCONTENT | PW_CLIENTONLY)) {
        // Fallback to BitBlt from client DC
        BitBlt(m_memDC, 0, 0, m_width, m_height, m_windowDC, 0, 0, SRCCOPY);
    }

    // Read pixels from bitmap
    BITMAPINFOHEADER bi = {};
    bi.biSize = sizeof(bi);
    bi.biWidth = m_width;
    bi.biHeight = m_height; // positive = bottom-up (matches OpenGL)
    bi.biPlanes = 1;
    bi.biBitCount = 32;
    bi.biCompression = BI_RGB;

    GetDIBits(m_memDC, m_bitmap, 0, m_height, m_pixelBuffer.data(),
              (BITMAPINFO*)&bi, DIB_RGB_COLORS);

    // BGRA to RGBA conversion
    int outH = m_height - m_cropTop;
    uint8_t* px = m_pixelBuffer.data();
    int total = m_width * outH;
    for (int i = 0; i < total; i++) {
        uint8_t tmp = px[i * 4 + 0];
        px[i * 4 + 0] = px[i * 4 + 2];
        px[i * 4 + 2] = tmp;
        px[i * 4 + 3] = 255;
    }

    m_texture.updateData(m_pixelBuffer.data(), m_width, outH);
}

void WindowCaptureSource::cleanupGDI() {
    if (m_memDC) {
        if (m_oldBitmap) SelectObject(m_memDC, m_oldBitmap);
        DeleteDC(m_memDC);
        m_memDC = nullptr;
        m_oldBitmap = nullptr;
    }
    if (m_bitmap) {
        DeleteObject(m_bitmap);
        m_bitmap = nullptr;
    }
    if (m_windowDC && m_hwnd) {
        ReleaseDC(m_hwnd, m_windowDC);
        m_windowDC = nullptr;
    }
}

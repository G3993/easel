#ifdef HAS_NDI

// Force static declaration so NDI functions are plain extern "C" (not dllimport).
// We load the NDI DLL manually at runtime via LoadLibrary/GetProcAddress.
#define PROCESSINGNDILIB_STATIC

#include "sources/NDIRuntime.h"
#include <iostream>

#ifdef _WIN32
#include <windows.h>
#endif

NDIRuntime& NDIRuntime::instance() {
    static NDIRuntime s;
    return s;
}

// Manually load NDI runtime DLL and resolve NDIlib_v6_load
static const NDIlib_v6* loadNDIRuntime() {
#ifdef _WIN32
    // Try the environment variable first (NDI 6 runtime)
    const char* redistFolder = getenv("NDI_RUNTIME_DIR_V6");
    if (!redistFolder) redistFolder = getenv("NDI_RUNTIME_DIR_V5");

    std::string dllPath;
    if (redistFolder) {
        dllPath = std::string(redistFolder) + "\\Processing.NDI.Lib.x64.dll";
    }

    HMODULE hNDI = nullptr;
    if (!dllPath.empty()) {
        hNDI = LoadLibraryA(dllPath.c_str());
    }
    if (!hNDI) {
        // Fallback: try system PATH
        hNDI = LoadLibraryA("Processing.NDI.Lib.x64.dll");
    }
    if (!hNDI) {
        return nullptr;
    }

    // Resolve NDIlib_v6_load (or fall back to NDIlib_v5_load)
    typedef const NDIlib_v6* (*NDIlib_v6_load_fn)(void);
    auto loadFn = (NDIlib_v6_load_fn)GetProcAddress(hNDI, "NDIlib_v6_load");
    if (!loadFn) {
        loadFn = (NDIlib_v6_load_fn)GetProcAddress(hNDI, "NDIlib_v5_load");
    }
    if (!loadFn) {
        FreeLibrary(hNDI);
        return nullptr;
    }

    return loadFn();
#else
    return nullptr;
#endif
}

bool NDIRuntime::init() {
    if (m_initialized) return m_loaded;
    m_initialized = true;

    m_pApi = loadNDIRuntime();
    if (!m_pApi) {
        std::cout << "[NDI] Runtime not found (NDI Tools not installed?)" << std::endl;
        return false;
    }

    if (!m_pApi->initialize()) {
        std::cerr << "[NDI] Failed to initialize" << std::endl;
        m_pApi = nullptr;
        return false;
    }

    m_loaded = true;
    std::cout << "[NDI] Runtime loaded successfully" << std::endl;
    return true;
}

void NDIRuntime::shutdown() {
    if (m_loaded && m_pApi && m_pApi->destroy) {
        m_pApi->destroy();
    }
    m_pApi = nullptr;
    m_loaded = false;
    m_initialized = false;
}

#endif // HAS_NDI

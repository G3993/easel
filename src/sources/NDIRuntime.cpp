#ifdef HAS_NDI

#include "sources/NDIRuntime.h"
#include <iostream>

#ifdef _WIN32
#include <windows.h>
#else
#include <dlfcn.h>
#ifdef __APPLE__
#include <mach-o/dyld.h>
#endif
#endif

NDIRuntime& NDIRuntime::instance() {
    static NDIRuntime s;
    return s;
}

<<<<<<< Updated upstream
=======
// Manually load NDI runtime DLL and get the v6 API struct
static const NDIlib_v6* loadNDIRuntime() {
#ifdef _WIN32
    // Try to find NDI runtime DLL
    const char* paths[] = {
        "Processing.NDI.Lib.x64.dll",
        "C:\\Program Files\\NDI\\NDI 6 Runtime\\Processing.NDI.Lib.x64.dll",
        "C:\\Program Files\\NDI\\NDI 5 Runtime\\Processing.NDI.Lib.x64.dll",
    };

    // Also check NDI_RUNTIME_DIR environment variable
    HMODULE hNDI = nullptr;
    char envBuf[512] = {};
    if (GetEnvironmentVariableA("NDI_RUNTIME_DIR_V6", envBuf, sizeof(envBuf)) > 0 ||
        GetEnvironmentVariableA("NDI_RUNTIME_DIR_V5", envBuf, sizeof(envBuf)) > 0) {
        std::string dllPath = std::string(envBuf) + "\\Processing.NDI.Lib.x64.dll";
        hNDI = LoadLibraryA(dllPath.c_str());
    }

    if (!hNDI) {
        for (const char* path : paths) {
            hNDI = LoadLibraryA(path);
            if (hNDI) break;
        }
    }

    if (!hNDI) {
        std::cout << "[NDI] Could not load Processing.NDI.Lib.x64.dll" << std::endl;
        return nullptr;
    }

    // Get the load function
    typedef const NDIlib_v6* (*NDIlib_v6_load_fn)(void);
    auto loadFn = (NDIlib_v6_load_fn)GetProcAddress(hNDI, "NDIlib_v6_load");
    if (!loadFn) {
        // Try v5
        typedef const NDIlib_v6* (*NDIlib_v5_load_fn)(void);
        loadFn = (NDIlib_v6_load_fn)GetProcAddress(hNDI, "NDIlib_v5_load");
    }

    if (!loadFn) {
        std::cout << "[NDI] Could not find NDIlib_v6_load in DLL" << std::endl;
        return nullptr;
    }

    return loadFn();
#else
    return nullptr;
#endif
}

>>>>>>> Stashed changes
bool NDIRuntime::init() {
    if (m_initialized) return m_loaded;
    m_initialized = true;

<<<<<<< Updated upstream
    // Dynamically load the NDI library and find the v6_3 load function
    typedef const NDIlib_v6_3* (*NDIlib_v6_3_load_fn)(void);
    NDIlib_v6_3_load_fn loadFn = nullptr;

#ifdef _WIN32
    // Try loading NDI DLL from runtime directory
    const char* runtimeDir = getenv("NDI_RUNTIME_DIR_V6");
    std::string dllPath;
    if (runtimeDir) {
        dllPath = std::string(runtimeDir) + "\\Processing.NDI.Lib.x64.dll";
    }
    HMODULE hLib = nullptr;
    if (!dllPath.empty()) hLib = LoadLibraryA(dllPath.c_str());
    if (!hLib) hLib = LoadLibraryA("Processing.NDI.Lib.x64.dll");
    if (hLib) {
        loadFn = (NDIlib_v6_3_load_fn)GetProcAddress(hLib, "NDIlib_v6_3_load");
        if (!loadFn) loadFn = (NDIlib_v6_3_load_fn)GetProcAddress(hLib, "NDIlib_v5_load");
    }
#else
    // macOS/Linux: try standard library paths and common NDI install locations
    void* hLib = dlopen("libndi.dylib", RTLD_LAZY);
    if (!hLib) hLib = dlopen("/usr/local/lib/libndi.dylib", RTLD_LAZY);
    if (!hLib) hLib = dlopen("/Library/NDI SDK for Apple/lib/macOS/libndi.dylib", RTLD_LAZY);
#ifdef __APPLE__
    if (!hLib) {
        // Try next to the executable
        char exePath[1024] = {};
        uint32_t size = sizeof(exePath);
        if (_NSGetExecutablePath(exePath, &size) == 0) {
            std::string dir(exePath);
            auto pos = dir.rfind('/');
            if (pos != std::string::npos) {
                dir = dir.substr(0, pos) + "/libndi.dylib";
                hLib = dlopen(dir.c_str(), RTLD_LAZY);
            }
        }
    }
#endif
    if (hLib) {
        loadFn = (NDIlib_v6_3_load_fn)dlsym(hLib, "NDIlib_v6_3_load");
        if (!loadFn) loadFn = (NDIlib_v6_3_load_fn)dlsym(hLib, "NDIlib_v5_load");
    }
#endif

    if (!loadFn) {
=======
    m_pApi = loadNDIRuntime();
    if (!m_pApi) {
>>>>>>> Stashed changes
        std::cout << "[NDI] Runtime not found (NDI Tools not installed?)" << std::endl;
        return false;
    }

<<<<<<< Updated upstream
    m_api = loadFn();
    if (!m_api) {
        std::cout << "[NDI] Failed to get API function table" << std::endl;
        return false;
    }

    if (!m_api->initialize()) {
        std::cerr << "[NDI] Failed to initialize" << std::endl;
        m_api = nullptr;
=======
    if (!m_pApi->initialize()) {
        std::cerr << "[NDI] Failed to initialize" << std::endl;
        m_pApi = nullptr;
>>>>>>> Stashed changes
        return false;
    }

    m_loaded = true;
    std::cout << "[NDI] Runtime loaded successfully (v" << (m_api->version ? m_api->version() : "?") << ")" << std::endl;
    return true;
}

void NDIRuntime::shutdown() {
<<<<<<< Updated upstream
    if (m_loaded && m_api && m_api->destroy) {
        m_api->destroy();
    }
    m_loaded = false;
    m_api = nullptr;
=======
    if (m_loaded && m_pApi) {
        m_pApi->destroy();
    }
    m_loaded = false;
    m_pApi = nullptr;
>>>>>>> Stashed changes
}

#endif // HAS_NDI

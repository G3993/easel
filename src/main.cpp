#include "app/Application.h"

#ifdef _WIN32
#include <windows.h>
#include <string>

// Request high-performance GPU on dual-GPU systems (NVIDIA Optimus / AMD Switchable)
extern "C" {
    __declspec(dllexport) unsigned long NvOptimusEnablement = 1;
    __declspec(dllexport) int AmdPowerXpressRequestHighPerformance = 1;
}
// Use WinMain to avoid console window, but also support console
int WINAPI WinMain(HINSTANCE, HINSTANCE, LPSTR, int) {
    // Attach to parent console if launched from terminal
    AttachConsole(ATTACH_PARENT_PROCESS);

    // Run relative to the exe so "shaders/", "assets/", default.easel and the
    // bundled ShaderClaw3 library resolve wherever the folder was unzipped
    // (portable install: no installer, no internet, no fixed path).
    {
        char exePath[MAX_PATH];
        DWORD n = GetModuleFileNameA(nullptr, exePath, MAX_PATH);
        if (n > 0 && n < MAX_PATH) {
            std::string dir(exePath, n);
            size_t slash = dir.find_last_of("\\/");
            if (slash != std::string::npos) SetCurrentDirectoryA(dir.substr(0, slash).c_str());
        }
    }

    Application app;
    if (!app.init()) return 1;
    app.run();
    app.shutdown();
    return 0;
}
#else

#ifdef __APPLE__
// Defined in main_mac.mm — chdirs to the .app's Resources/ if running from a
// bundle so relative loads like "shaders/foo.frag" still find their files.
void setBundleWorkingDir();
#endif

int main() {
#ifdef __APPLE__
    setBundleWorkingDir();
#endif
    Application app;
    if (!app.init()) return 1;
    app.run();
    app.shutdown();
    return 0;
}
#endif

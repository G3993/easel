#ifdef _WIN32
#include "app/CameraEnumerator.h"

#include <windows.h>
#include <mfapi.h>
#include <mfidl.h>

#pragma comment(lib, "mf.lib")
#pragma comment(lib, "mfplat.lib")
#pragma comment(lib, "mfuuid.lib")
#pragma comment(lib, "ole32.lib")

namespace {
std::string wideToUtf8(const WCHAR* w) {
    if (!w) return std::string();
    int len = WideCharToMultiByte(CP_UTF8, 0, w, -1, nullptr, 0, nullptr, nullptr);
    if (len <= 0) return std::string();
    std::string out(len - 1, '\0'); // len includes the null terminator
    WideCharToMultiByte(CP_UTF8, 0, w, -1, out.data(), len, nullptr, nullptr);
    return out;
}
} // namespace

// Enumerates video capture devices via Media Foundation, in the same order
// MFEnumDeviceSources returns them. WebcamSource::open() explicitly requests
// cv::CAP_MSMF (rather than cv::CAP_ANY) on Windows specifically so its
// index order matches this list 1:1 — without that, OpenCV's chosen backend
// for CAP_ANY isn't guaranteed to agree with MF's enumeration order, which
// would silently point "Camera 2" at the wrong physical device.
std::vector<CameraDeviceInfo> enumerateCameraDevices() {
    std::vector<CameraDeviceInfo> out;

    HRESULT hrCo = CoInitializeEx(nullptr, COINIT_MULTITHREADED);
    bool comInited = SUCCEEDED(hrCo) || hrCo == RPC_E_CHANGED_MODE;

    if (FAILED(MFStartup(MF_VERSION, MFSTARTUP_LITE))) {
        if (comInited) CoUninitialize();
        return out;
    }

    IMFAttributes* attrs = nullptr;
    HRESULT hr = MFCreateAttributes(&attrs, 1);
    if (SUCCEEDED(hr)) {
        hr = attrs->SetGUID(MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE,
                             MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_GUID);
    }

    IMFActivate** devices = nullptr;
    UINT32 count = 0;
    if (SUCCEEDED(hr)) {
        hr = MFEnumDeviceSources(attrs, &devices, &count);
    }

    if (SUCCEEDED(hr)) {
        for (UINT32 i = 0; i < count; i++) {
            WCHAR* name = nullptr;
            UINT32 nameLen = 0;
            if (SUCCEEDED(devices[i]->GetAllocatedString(
                    MF_DEVSOURCE_ATTRIBUTE_FRIENDLY_NAME, &name, &nameLen))) {
                CameraDeviceInfo info;
                info.name = wideToUtf8(name);
                CoTaskMemFree(name);

                WCHAR* symLink = nullptr;
                UINT32 symLen = 0;
                if (SUCCEEDED(devices[i]->GetAllocatedString(
                        MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK,
                        &symLink, &symLen))) {
                    info.uniqueId = wideToUtf8(symLink);
                    CoTaskMemFree(symLink);
                }

                out.push_back(std::move(info));
            }
            devices[i]->Release();
        }
        CoTaskMemFree(devices);
    }

    if (attrs) attrs->Release();
    MFShutdown();
    if (comInited) CoUninitialize();
    return out;
}

#endif // _WIN32

#include "app/AudioInputDevices.h"

#ifdef _WIN32
#include <windows.h>
#include <mmdeviceapi.h>
#include <functiondiscoverykeys_devpkey.h>

static std::string wideToUtf8(const wchar_t* w) {
    if (!w) return {};
    int n = WideCharToMultiByte(CP_UTF8, 0, w, -1, nullptr, 0, nullptr, nullptr);
    if (n <= 1) return {};
    std::string s((size_t)n - 1, '\0');
    WideCharToMultiByte(CP_UTF8, 0, w, -1, s.data(), n, nullptr, nullptr);
    return s;
}

std::vector<AudioInputDevice> enumerateAudioInputDevices() {
    std::vector<AudioInputDevice> result;
    HRESULT initHr = CoInitializeEx(nullptr, COINIT_MULTITHREADED);

    IMMDeviceEnumerator* enumerator = nullptr;
    if (SUCCEEDED(CoCreateInstance(__uuidof(MMDeviceEnumerator), nullptr, CLSCTX_ALL,
                                   __uuidof(IMMDeviceEnumerator), (void**)&enumerator)) && enumerator) {
        IMMDeviceCollection* collection = nullptr;
        if (SUCCEEDED(enumerator->EnumAudioEndpoints(eCapture, DEVICE_STATE_ACTIVE, &collection)) && collection) {
            UINT count = 0;
            collection->GetCount(&count);
            for (UINT i = 0; i < count; i++) {
                IMMDevice* dev = nullptr;
                if (FAILED(collection->Item(i, &dev)) || !dev) continue;
                AudioInputDevice info;
                LPWSTR wid = nullptr;
                if (SUCCEEDED(dev->GetId(&wid))) {
                    info.id = wideToUtf8(wid);
                    CoTaskMemFree(wid);
                }
                IPropertyStore* props = nullptr;
                if (SUCCEEDED(dev->OpenPropertyStore(STGM_READ, &props)) && props) {
                    PROPVARIANT pv;
                    PropVariantInit(&pv);
                    if (SUCCEEDED(props->GetValue(PKEY_Device_FriendlyName, &pv)) && pv.vt == VT_LPWSTR)
                        info.name = wideToUtf8(pv.pwszVal);
                    PropVariantClear(&pv);
                    props->Release();
                }
                if (info.name.empty()) info.name = "Microphone " + std::to_string(i + 1);
                if (!info.id.empty()) result.push_back(std::move(info));
                dev->Release();
            }
            collection->Release();
        }
        enumerator->Release();
    }
    if (SUCCEEDED(initHr)) CoUninitialize();
    return result;
}
#else
std::vector<AudioInputDevice> enumerateAudioInputDevices() { return {}; }
#endif

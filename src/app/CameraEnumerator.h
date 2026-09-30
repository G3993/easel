#pragma once

#include <string>
#include <vector>

struct CameraDeviceInfo {
    std::string name;     // human-readable device name (e.g. "MacBook Pro Camera", "Game Capture HD60 X")
    std::string uniqueId; // AVFoundation uniqueID — stable across replugs
};

#if defined(__APPLE__) || defined(_WIN32)
// Devices in the exact order OpenCV's platform capture backend maps to
// cv::VideoCapture indices: AVFoundation's devicesWithMediaType: order on
// Mac (cap_avfoundation_mac.mm), Media Foundation's MFEnumDeviceSources
// order on Windows (see CameraEnumerator_win.cpp — WebcamSource explicitly
// opens with cv::CAP_MSMF so this stays in sync with what actually opens).
// Index i in this list == addWebcam(i).
std::vector<CameraDeviceInfo> enumerateCameraDevices();
#endif

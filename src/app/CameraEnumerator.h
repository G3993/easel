#pragma once

#include <string>
#include <vector>

struct CameraDeviceInfo {
    std::string name;     // human-readable device name (e.g. "MacBook Pro Camera", "Game Capture HD60 X")
    std::string uniqueId; // AVFoundation uniqueID — stable across replugs
};

#ifdef __APPLE__
// Devices in the exact order OpenCV's AVFoundation backend maps to
// cv::VideoCapture indices: AVMediaTypeVideo devices first, then
// AVMediaTypeMuxed (cap_avfoundation_mac.mm uses devicesWithMediaType:).
// Index i in this list == addWebcam(i).
std::vector<CameraDeviceInfo> enumerateCameraDevices();
#endif

#include "app/CameraEnumerator.h"

#import <AVFoundation/AVFoundation.h>

std::vector<CameraDeviceInfo> enumerateCameraDevices() {
    std::vector<CameraDeviceInfo> out;
    @autoreleasepool {
        // Deliberately the deprecated API: OpenCV 4.13's AVFoundation backend
        // indexes cameras via devicesWithMediaType: (video, then muxed), and
        // this list must match its ordering 1:1.
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wdeprecated-declarations"
        NSArray<AVCaptureDevice*>* devices =
            [[AVCaptureDevice devicesWithMediaType:AVMediaTypeVideo]
                arrayByAddingObjectsFromArray:[AVCaptureDevice devicesWithMediaType:AVMediaTypeMuxed]];
#pragma clang diagnostic pop
        for (AVCaptureDevice* d in devices) {
            CameraDeviceInfo info;
            info.name = d.localizedName ? std::string(d.localizedName.UTF8String) : "Camera";
            info.uniqueId = d.uniqueID ? std::string(d.uniqueID.UTF8String) : "";
            out.push_back(std::move(info));
        }
    }
    return out;
}

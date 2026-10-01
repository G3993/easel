#pragma once
#include <string>
#include <vector>

// Microphone (capture endpoint) enumeration that does NOT depend on FFmpeg.
// VideoRecorder::enumerateAudioDevices() only exists in FFmpeg builds, which
// left builds without FFmpeg with no way to pick a mic. Windows (WASAPI) only;
// returns an empty list elsewhere.
struct AudioInputDevice {
    std::string name;
    std::string id;   // WASAPI endpoint ID, as AudioAnalyzer::setDeviceId expects
};

std::vector<AudioInputDevice> enumerateAudioInputDevices();

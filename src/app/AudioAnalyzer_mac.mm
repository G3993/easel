#ifdef __APPLE__
#include "app/AudioAnalyzer.h"
#include <ScreenCaptureKit/ScreenCaptureKit.h>
#include <CoreMedia/CoreMedia.h>
#include <AudioToolbox/AudioToolbox.h>
#include <CoreAudio/CoreAudio.h>
#import  <AVFoundation/AVFoundation.h>   // AVCaptureDevice — microphone TCC grant
#import  <CoreAudio/CATapDescription.h>    // Core Audio process tap (macOS 14.2+)
#import  <CoreAudio/AudioHardwareTapping.h>
#include <unistd.h>
#include <iostream>
#include <mutex>
#include <vector>

// ─── ScreenCaptureKit audio delegate (for system audio loopback) ──

@interface EaselAudioCaptureDelegate : NSObject <SCStreamOutput>
@property (nonatomic) std::mutex* bufferMutex;
@property (nonatomic) std::vector<float>* sampleBuffer;
@end

@implementation EaselAudioCaptureDelegate
- (void)stream:(SCStream *)stream didOutputSampleBuffer:(CMSampleBufferRef)sampleBuffer ofType:(SCStreamOutputType)type {
    if (type != SCStreamOutputTypeAudio) return;

    CMBlockBufferRef blockBuffer = CMSampleBufferGetDataBuffer(sampleBuffer);
    if (!blockBuffer) return;

    CMFormatDescriptionRef formatDesc = CMSampleBufferGetFormatDescription(sampleBuffer);
    if (!formatDesc) return;

    const AudioStreamBasicDescription* asbd = CMAudioFormatDescriptionGetStreamBasicDescription(formatDesc);
    if (!asbd) return;

    size_t totalBytes = 0;
    char* dataPointer = nullptr;
    OSStatus status = CMBlockBufferGetDataPointer(blockBuffer, 0, nullptr, &totalBytes, &dataPointer);
    if (status != noErr || !dataPointer || totalBytes == 0) return;

    int channels = (int)asbd->mChannelsPerFrame;
    if (channels < 1) channels = 1;

    const float* floatData = (const float*)dataPointer;
    int totalSamples = (int)(totalBytes / sizeof(float));
    int frames = totalSamples / channels;

    std::lock_guard<std::mutex> lock(*self.bufferMutex);
    for (int i = 0; i < frames; i++) {
        float mono = 0.0f;
        for (int ch = 0; ch < channels; ch++) {
            mono += floatData[i * channels + ch];
        }
        mono /= channels;
        self.sampleBuffer->push_back(mono);
    }
}
@end

// ─── Internal state ──────────────────────────────────────────────

struct MacAudioState {
    // ScreenCaptureKit (system audio loopback)
    SCStream* stream = nil;
    EaselAudioCaptureDelegate* delegate = nil;
    dispatch_queue_t queue = nil;

    // CoreAudio AudioUnit (microphone input)
    AudioComponentInstance audioUnit = nullptr;

    // Core Audio process tap (system audio loopback, preferred backend)
    AudioObjectID tapID = kAudioObjectUnknown;
    AudioObjectID aggID = kAudioObjectUnknown;
    AudioDeviceIOProcID tapProc = nullptr;

    // Shared buffer
    std::mutex bufferMutex;
    std::vector<float> sampleBuffer;

    bool isInputDevice = false;
};

// ─── CoreAudio input callback ───────────────────────────────────

static OSStatus audioInputCallback(void* inRefCon,
                                    AudioUnitRenderActionFlags* ioActionFlags,
                                    const AudioTimeStamp* inTimeStamp,
                                    UInt32 inBusNumber,
                                    UInt32 inNumberFrames,
                                    AudioBufferList* ioData) {
    auto* state = (MacAudioState*)inRefCon;
    if (!state || !state->audioUnit) return noErr;

    // Allocate buffer for input
    AudioBufferList bufferList;
    bufferList.mNumberBuffers = 1;
    bufferList.mBuffers[0].mNumberChannels = 1; // mono
    bufferList.mBuffers[0].mDataByteSize = inNumberFrames * sizeof(float);
    std::vector<float> tempBuf(inNumberFrames);
    bufferList.mBuffers[0].mData = tempBuf.data();

    OSStatus status = AudioUnitRender(state->audioUnit, ioActionFlags, inTimeStamp,
                                       inBusNumber, inNumberFrames, &bufferList);
    // A silently failing render is indistinguishable from a silent room at
    // the feature level (the AGC just winds up on nothing), so log the
    // first failure and each status change — not every callback.
    {
        static OSStatus sLastLogged = noErr;
        if (status != sLastLogged) {
            fprintf(stderr, "[AudioAnalyzer] mic AudioUnitRender status changed: %d\n",
                    (int)status);
            sLastLogged = status;
        }
    }
    if (status != noErr) return status;

    std::lock_guard<std::mutex> lock(state->bufferMutex);
    float* samples = (float*)bufferList.mBuffers[0].mData;
    for (UInt32 i = 0; i < inNumberFrames; i++) {
        state->sampleBuffer.push_back(samples[i]);
    }

    return noErr;
}

// ─── Core Audio process tap (system audio loopback) ─────────────
//
// Why not ScreenCaptureKit: ANY SCStream — even a 2x2 audio-only one — makes
// the Window Server draw the screen-capture privacy dot at the top-right of
// EVERY display, at a window level nothing can cover. On stage that dot sits
// on the projector image. A Core Audio process tap (macOS 14.2+) delivers the
// same system mix without touching the screen-capture path, so no dot. It is
// gated by the "System Audio Recording Only" privacy grant, which macOS
// prompts for on first use (NSAudioCaptureUsageDescription in Info.plist).
// SCK remains as a fallback for when the tap can't be created.

static OSStatus systemTapIOProc(AudioObjectID, const AudioTimeStamp*,
                                const AudioBufferList* inInputData, const AudioTimeStamp*,
                                AudioBufferList*, const AudioTimeStamp*, void* clientData) {
    auto* state = (MacAudioState*)clientData;
    if (!state || !inInputData || inInputData->mNumberBuffers == 0) return noErr;

    const AudioBuffer& b0 = inInputData->mBuffers[0];
    const float* data = (const float*)b0.mData;
    int ch = (int)b0.mNumberChannels;
    if (ch < 1) ch = 1;
    int frames = (int)(b0.mDataByteSize / sizeof(float)) / ch;
    if (!data || frames <= 0) return noErr;

    std::lock_guard<std::mutex> lock(state->bufferMutex);
    if (ch >= 2 || inInputData->mNumberBuffers == 1) {
        // interleaved (the stereo tap's normal layout) → mono mix, same as
        // the SCK delegate produced
        for (int i = 0; i < frames; i++) {
            float mono = 0.0f;
            for (int c = 0; c < ch; c++) mono += data[i * ch + c];
            state->sampleBuffer.push_back(mono / ch);
        }
    } else {
        // planar: one mono buffer per channel → average the buffers
        int nb = (int)inInputData->mNumberBuffers;
        for (int i = 0; i < frames; i++) {
            float mono = 0.0f; int cnt = 0;
            for (int bi = 0; bi < nb; bi++) {
                const AudioBuffer& b = inInputData->mBuffers[bi];
                const float* d = (const float*)b.mData;
                if (d && (int)(b.mDataByteSize / sizeof(float)) > i) { mono += d[i]; cnt++; }
            }
            state->sampleBuffer.push_back(cnt ? mono / cnt : 0.0f);
        }
    }
    return noErr;
}

static void stopSystemAudioTap(MacAudioState* state) {
    if (!state) return;
    if (state->aggID == kAudioObjectUnknown && state->tapID == kAudioObjectUnknown) return;
    OSStatus s1 = noErr, s2 = noErr, s3 = noErr, s4 = noErr;
    if (state->aggID != kAudioObjectUnknown) {
        if (state->tapProc) {
            s1 = AudioDeviceStop(state->aggID, state->tapProc);
            s2 = AudioDeviceDestroyIOProcID(state->aggID, state->tapProc);
            state->tapProc = nullptr;
        }
        s3 = AudioHardwareDestroyAggregateDevice(state->aggID);
        state->aggID = kAudioObjectUnknown;
    }
    if (state->tapID != kAudioObjectUnknown) {
        if (@available(macOS 14.2, *)) s4 = AudioHardwareDestroyProcessTap(state->tapID);
        state->tapID = kAudioObjectUnknown;
    }
    std::cout << "[AudioAnalyzer] system audio tap stopped (stop=" << (int)s1
              << " destroyProc=" << (int)s2 << " destroyAgg=" << (int)s3
              << " destroyTap=" << (int)s4 << ")" << std::endl;
}

// Returns true with *outRate set when the tap is live and delivering.
static bool startSystemAudioTap(MacAudioState* state, double* outRate) {
    if (@available(macOS 14.2, *)) {
        @autoreleasepool {
            CATapDescription* desc =
                [[CATapDescription alloc] initStereoGlobalTapButExcludeProcesses:@[]];
            desc.name = @"EaselSystemTap";
            desc.privateTap = YES;
            desc.muteBehavior = CATapUnmuted;   // listen only — never alter playback

            AudioObjectID tapID = kAudioObjectUnknown;
            OSStatus err = AudioHardwareCreateProcessTap(desc, &tapID);
            if (err != noErr || tapID == kAudioObjectUnknown) {
                std::cerr << "[AudioAnalyzer] process tap create failed (" << (int)err
                          << ") — check System Settings > Privacy & Security > "
                             "Screen & System Audio Recording" << std::endl;
                return false;
            }
            state->tapID = tapID;

            NSDictionary* aggDesc = @{
                @(kAudioAggregateDeviceNameKey): @"Easel Tap Aggregate",
                @(kAudioAggregateDeviceUIDKey):
                    [NSString stringWithFormat:@"com.etherealmachine.easel.tapagg.%d", getpid()],
                @(kAudioAggregateDeviceIsPrivateKey): @YES,
                @(kAudioAggregateDeviceTapAutoStartKey): @YES,
                @(kAudioAggregateDeviceTapListKey): @[@{
                    @(kAudioSubTapUIDKey): desc.UUID.UUIDString,
                    @(kAudioSubTapDriftCompensationKey): @YES,
                }],
            };
            AudioObjectID aggID = kAudioObjectUnknown;
            err = AudioHardwareCreateAggregateDevice((__bridge CFDictionaryRef)aggDesc, &aggID);
            if (err != noErr || aggID == kAudioObjectUnknown) {
                std::cerr << "[AudioAnalyzer] tap aggregate device failed (" << (int)err << ")" << std::endl;
                stopSystemAudioTap(state);
                return false;
            }
            state->aggID = aggID;

            Float64 rate = 48000;
            {
                AudioObjectPropertyAddress addr = {kAudioDevicePropertyNominalSampleRate,
                                                   kAudioObjectPropertyScopeGlobal,
                                                   kAudioObjectPropertyElementMain};
                UInt32 sz = sizeof(rate);
                if (AudioObjectGetPropertyData(aggID, &addr, 0, nullptr, &sz, &rate) != noErr || rate <= 0)
                    rate = 48000;
            }

            AudioDeviceIOProcID procID = nullptr;
            err = AudioDeviceCreateIOProcID(aggID, systemTapIOProc, state, &procID);
            if (err != noErr || !procID) {
                std::cerr << "[AudioAnalyzer] tap IOProc create failed (" << (int)err << ")" << std::endl;
                stopSystemAudioTap(state);
                return false;
            }
            state->tapProc = procID;

            err = AudioDeviceStart(aggID, procID);
            if (err != noErr) {
                std::cerr << "[AudioAnalyzer] tap device start failed (" << (int)err << ")" << std::endl;
                stopSystemAudioTap(state);
                return false;
            }
            if (outRate) *outRate = rate;
            return true;
        }
    }
    return false;
}

// ─── Helper: find AudioDeviceID from UID string ─────────────────

static AudioDeviceID findDeviceByUID(const std::string& uid) {
    if (uid.empty()) return kAudioObjectUnknown;

    CFStringRef uidStr = CFStringCreateWithCString(nullptr, uid.c_str(), kCFStringEncodingUTF8);
    if (!uidStr) return kAudioObjectUnknown;

    AudioObjectPropertyAddress prop = {
        kAudioHardwarePropertyTranslateUIDToDevice,
        kAudioObjectPropertyScopeGlobal,
        kAudioObjectPropertyElementMain
    };
    AudioDeviceID deviceId = kAudioObjectUnknown;
    UInt32 size = sizeof(AudioDeviceID);
    AudioObjectGetPropertyData(kAudioObjectSystemObject, &prop, sizeof(CFStringRef), &uidStr, &size, &deviceId);
    CFRelease(uidStr);
    return deviceId;
}

// ─── initCapture: CoreAudio for mic, ScreenCaptureKit for system ─

void AudioAnalyzer::initCapture() {
    cleanupCapture();

    auto* state = new MacAudioState();
    m_macAudioImpl = state;

    // If a specific capture device is selected, use CoreAudio AudioUnit
    if (m_requestedIsCapture && !m_deviceId.empty()) {
        // Microphone capture is gated by the macOS microphone TCC grant.
        // Unlike the ScreenCaptureKit system-audio path below, CoreAudio's HAL
        // input unit does NOT auto-prompt — it just hands back digital silence
        // when the grant is missing, so every band reads 0 and nothing reacts.
        // Request the grant explicitly (mirrors VisionTracker's camera request)
        // and wait for the user's decision BEFORE opening the unit, so capture
        // works on the very first grant rather than needing a re-select.
        AVAuthorizationStatus micStatus =
            [AVCaptureDevice authorizationStatusForMediaType:AVMediaTypeAudio];
        if (micStatus == AVAuthorizationStatusNotDetermined) {
            dispatch_semaphore_t permSem = dispatch_semaphore_create(0);
            [AVCaptureDevice requestAccessForMediaType:AVMediaTypeAudio
                                     completionHandler:^(BOOL granted) {
                (void)granted;
                dispatch_semaphore_signal(permSem);
            }];
            // Bounded wait — an ignored dialog must not hang the render thread
            // forever. The wait returns the instant the user answers; 30s only
            // caps a fully-ignored prompt (re-selecting the device retries).
            dispatch_semaphore_wait(permSem,
                dispatch_time(DISPATCH_TIME_NOW, 30 * NSEC_PER_SEC));
            micStatus = [AVCaptureDevice authorizationStatusForMediaType:AVMediaTypeAudio];
        }
        if (micStatus != AVAuthorizationStatusAuthorized) {
            std::cerr << "[AudioAnalyzer] Microphone permission not granted — enable it "
                         "in System Settings > Privacy & Security > Microphone, then "
                         "re-select the mic." << std::endl;
            cleanupCapture();
            return;
        }

        AudioDeviceID deviceId = findDeviceByUID(m_deviceId);
        if (deviceId == kAudioObjectUnknown) {
            std::cerr << "[AudioAnalyzer] Device not found: " << m_deviceId << std::endl;
            cleanupCapture();
            return;
        }

        state->isInputDevice = true;

        // Create HAL audio unit for input
        AudioComponentDescription desc = {};
        desc.componentType = kAudioUnitType_Output;
        desc.componentSubType = kAudioUnitSubType_HALOutput;
        desc.componentManufacturer = kAudioUnitManufacturer_Apple;

        AudioComponent comp = AudioComponentFindNext(nullptr, &desc);
        if (!comp) {
            std::cerr << "[AudioAnalyzer] Failed to find HAL audio component" << std::endl;
            cleanupCapture();
            return;
        }

        OSStatus status = AudioComponentInstanceNew(comp, &state->audioUnit);
        if (status != noErr) {
            std::cerr << "[AudioAnalyzer] Failed to create audio unit: " << status << std::endl;
            cleanupCapture();
            return;
        }

        // Enable input, disable output
        UInt32 enableIO = 1;
        AudioUnitSetProperty(state->audioUnit, kAudioOutputUnitProperty_EnableIO,
                             kAudioUnitScope_Input, 1, &enableIO, sizeof(enableIO));
        enableIO = 0;
        AudioUnitSetProperty(state->audioUnit, kAudioOutputUnitProperty_EnableIO,
                             kAudioUnitScope_Output, 0, &enableIO, sizeof(enableIO));

        // Set the input device
        AudioUnitSetProperty(state->audioUnit, kAudioOutputUnitProperty_CurrentDevice,
                             kAudioUnitScope_Global, 0, &deviceId, sizeof(deviceId));

        // Client format must run at the device's nominal rate — AUHAL does
        // no sample-rate conversion on the input side, so forcing 48k on a
        // 44.1k device (the built-in mic's default) makes every
        // AudioUnitRender fail with kAudioUnitErr_CannotDoInCurrentContext
        // and the mic delivers pure silence.
        Float64 deviceRate = 48000;
        {
            AudioObjectPropertyAddress rateProp = {
                kAudioDevicePropertyNominalSampleRate,
                kAudioObjectPropertyScopeGlobal,
                kAudioObjectPropertyElementMain
            };
            UInt32 rateSize = sizeof(deviceRate);
            if (AudioObjectGetPropertyData(deviceId, &rateProp, 0, nullptr,
                                           &rateSize, &deviceRate) != noErr ||
                deviceRate <= 0) {
                deviceRate = 48000;
            }
        }

        AudioStreamBasicDescription fmt = {};
        fmt.mSampleRate = deviceRate;
        fmt.mFormatID = kAudioFormatLinearPCM;
        fmt.mFormatFlags = kAudioFormatFlagIsFloat | kAudioFormatFlagIsPacked;
        fmt.mBitsPerChannel = 32;
        fmt.mChannelsPerFrame = 1;
        fmt.mFramesPerPacket = 1;
        fmt.mBytesPerFrame = sizeof(float);
        fmt.mBytesPerPacket = sizeof(float);

        AudioUnitSetProperty(state->audioUnit, kAudioUnitProperty_StreamFormat,
                             kAudioUnitScope_Output, 1, &fmt, sizeof(fmt));

        // Set input callback
        AURenderCallbackStruct callbackStruct = {};
        callbackStruct.inputProc = audioInputCallback;
        callbackStruct.inputProcRefCon = state;
        AudioUnitSetProperty(state->audioUnit, kAudioOutputUnitProperty_SetInputCallback,
                             kAudioUnitScope_Global, 0, &callbackStruct, sizeof(callbackStruct));

        status = AudioUnitInitialize(state->audioUnit);
        if (status != noErr) {
            std::cerr << "[AudioAnalyzer] Failed to initialize audio unit: " << status << std::endl;
            cleanupCapture();
            return;
        }

        status = AudioOutputUnitStart(state->audioUnit);
        if (status != noErr) {
            std::cerr << "[AudioAnalyzer] Failed to start audio unit: " << status << std::endl;
            cleanupCapture();
            return;
        }

        m_initialized = true;
        m_sampleRate = (int)deviceRate;
        m_channels = 1;
        std::cout << "[AudioAnalyzer] CoreAudio input capture started (device: " << m_deviceId
                  << ", " << (int)deviceRate << "Hz)" << std::endl;
        return;
    }

    // Default: ScreenCaptureKit system audio loopback. Gated behind an
    // explicit opt-in so the macOS "Screen Recording" TCC prompt only fires
    // when the user actually needs system-audio capture (picks System Audio
    // in the dropdown, starts recording, etc.) — otherwise self-signed dev
    // builds would prompt on every launch because the cdhash changes.
    if (!m_wantsSystemAudio) {
        cleanupCapture();
        return;
    }

    // Preferred: Core Audio process tap — no screen-capture privacy dot on
    // the projectors (see systemTapIOProc above).
    {
        double tapRate = 48000;
        if (startSystemAudioTap(state, &tapRate)) {
            m_initialized = true;
            m_sampleRate = (int)tapRate;
            m_channels = 2;
            std::cout << "[AudioAnalyzer] system audio via Core Audio process tap ("
                      << m_sampleRate << "Hz stereo -> mono)" << std::endl;
            return;
        }
        std::cerr << "[AudioAnalyzer] process tap unavailable — falling back to "
                     "ScreenCaptureKit (macOS will show the screen-capture dot)" << std::endl;
    }

    dispatch_semaphore_t sem = dispatch_semaphore_create(0);
    __block bool success = false;

    [SCShareableContent getShareableContentWithCompletionHandler:^(SCShareableContent* content, NSError* error) {
        if (error || !content) {
            std::cerr << "[AudioAnalyzer] Failed to get shareable content: "
                      << (error ? error.localizedDescription.UTF8String : "unknown") << std::endl;
            dispatch_semaphore_signal(sem);
            return;
        }

        SCDisplay* display = content.displays.firstObject;
        if (!display) {
            std::cerr << "[AudioAnalyzer] No displays found for audio capture" << std::endl;
            dispatch_semaphore_signal(sem);
            return;
        }

        SCStreamConfiguration* config = [[SCStreamConfiguration alloc] init];
        config.capturesAudio = YES;
        config.excludesCurrentProcessAudio = NO;
        config.sampleRate = 48000;
        config.channelCount = 2;
        config.width = 2;
        config.height = 2;
        config.minimumFrameInterval = CMTimeMake(1, 1);

        SCContentFilter* filter = [[SCContentFilter alloc] initWithDisplay:display excludingWindows:@[]];

        state->stream = [[SCStream alloc] initWithFilter:filter configuration:config delegate:nil];
        state->delegate = [[EaselAudioCaptureDelegate alloc] init];
        state->delegate.bufferMutex = &state->bufferMutex;
        state->delegate.sampleBuffer = &state->sampleBuffer;

        state->queue = dispatch_queue_create("com.easel.audiocapture", DISPATCH_QUEUE_SERIAL);

        NSError* addError = nil;
        [state->stream addStreamOutput:state->delegate type:SCStreamOutputTypeAudio sampleHandlerQueue:state->queue error:&addError];
        if (addError) {
            std::cerr << "[AudioAnalyzer] Failed to add audio output: " << addError.localizedDescription.UTF8String << std::endl;
            dispatch_semaphore_signal(sem);
            return;
        }

        [state->stream startCaptureWithCompletionHandler:^(NSError* startError) {
            if (startError) {
                std::cerr << "[AudioAnalyzer] Failed to start audio capture: " << startError.localizedDescription.UTF8String << std::endl;
            } else {
                success = true;
                std::cout << "[AudioAnalyzer] macOS system audio capture started (48kHz stereo -> mono)" << std::endl;
            }
            dispatch_semaphore_signal(sem);
        }];
    }];

    dispatch_semaphore_wait(sem, dispatch_time(DISPATCH_TIME_NOW, 5 * NSEC_PER_SEC));

    if (success) {
        m_initialized = true;
        m_sampleRate = 48000;
        m_channels = 2;
    } else {
        std::cerr << "[AudioAnalyzer] macOS audio capture failed — FFT will not update" << std::endl;
        std::cerr << "[AudioAnalyzer] Grant Screen Recording permission in System Settings > Privacy & Security" << std::endl;
        cleanupCapture();
    }
}

void AudioAnalyzer::cleanupCapture() {
    if (m_macAudioImpl) {
        auto* state = (MacAudioState*)m_macAudioImpl;
        // Tap teardown is synchronous: after AudioDeviceDestroyIOProcID the
        // IOProc can no longer run, so deleting `state` below is safe.
        stopSystemAudioTap(state);
        if (state->audioUnit) {
            AudioOutputUnitStop(state->audioUnit);
            AudioUnitUninitialize(state->audioUnit);
            AudioComponentInstanceDispose(state->audioUnit);
        }
        if (state->stream) {
            dispatch_semaphore_t sem = dispatch_semaphore_create(0);
            [state->stream stopCaptureWithCompletionHandler:^(NSError* error) {
                dispatch_semaphore_signal(sem);
            }];
            // Bounded wait so a wedged SCK stop can't hang the caller — but
            // on timeout SCK may STILL be delivering into state->queue, and
            // the delegate holds raw pointers to state->bufferMutex /
            // sampleBuffer. Deleting now would be a use-after-free inside
            // the capture callback, so intentionally leak the state instead:
            // one small struct per wedged stop (documented leak > UAF).
            // Only delete when the completion handler actually fired.
            if (dispatch_semaphore_wait(sem, dispatch_time(DISPATCH_TIME_NOW, 2 * NSEC_PER_SEC)) != 0) {
                std::cerr << "[AudioAnalyzer] SCK stop timed out — leaking capture state to avoid a use-after-free" << std::endl;
                state = nullptr;
            }
        }
        delete state;   // no-op when parked above
        m_macAudioImpl = nullptr;
    }
    m_initialized = false;
    m_samplesAccumulated = 0;
}

void AudioAnalyzer::drainPackets() {
    if (!m_macAudioImpl) return;
    auto* state = (MacAudioState*)m_macAudioImpl;

    std::vector<float> samples;
    {
        std::lock_guard<std::mutex> lock(state->bufferMutex);
        samples.swap(state->sampleBuffer);
    }

    for (float s : samples) {
        m_ringBuf[m_ringPos] = s;
        m_ringPos = (m_ringPos + 1) % kFFTSize;
        m_samplesAccumulated++;
    }
}
#endif

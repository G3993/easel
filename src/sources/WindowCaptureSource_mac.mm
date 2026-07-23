#ifdef __APPLE__
#include "sources/WindowCaptureSource_mac.h"

#import <ScreenCaptureKit/ScreenCaptureKit.h>
#import <CoreGraphics/CoreGraphics.h>
#import <CoreFoundation/CoreFoundation.h>
#import <AppKit/AppKit.h>
#include <OpenGL/OpenGL.h>
#include <OpenGL/CGLIOSurface.h>
#include <IOSurface/IOSurface.h>
#include <unistd.h>
#include <algorithm>
#include <iostream>
#include <memory>
#include <mutex>

// ScreenCaptureKit window capture. The desktop-independent window filter
// keeps delivering frames while the window is fully covered by other
// windows, parked on another Space, or behind Easel's own fullscreen
// output — and because the WindowServer keeps compositing a captured
// window, apps like Chrome/Meet/Zoom keep painting video instead of
// throttling to a frozen frame the way they do under the old
// CGWindowListCreateImage polling path. Dock-minimized windows are the
// one case that still stops producing frames (the app stops drawing).

// ─── SCStreamOutput delegate ────────────────────────────────────────

@interface EaselWindowCaptureDelegate : NSObject <SCStreamOutput>
@property (nonatomic) std::mutex* bufferMutex;
@property (nonatomic) IOSurfaceRef pendingSurface;
@property (nonatomic) bool* hasNewFrame;
@end

@implementation EaselWindowCaptureDelegate
- (void)dealloc {
    if (_pendingSurface) {
        IOSurfaceDecrementUseCount(_pendingSurface);
        CFRelease(_pendingSurface);
    }
}

- (void)stream:(SCStream *)stream didOutputSampleBuffer:(CMSampleBufferRef)sampleBuffer ofType:(SCStreamOutputType)type {
    if (type != SCStreamOutputTypeScreen) return;

    CVImageBufferRef imageBuffer = CMSampleBufferGetImageBuffer(sampleBuffer);
    if (!imageBuffer) return;

    IOSurfaceRef surface = CVPixelBufferGetIOSurface(imageBuffer);
    if (!surface) return;

    std::lock_guard<std::mutex> lock(*self.bufferMutex);
    CFRetain(surface);
    IOSurfaceIncrementUseCount(surface);
    if (self.pendingSurface) {
        IOSurfaceDecrementUseCount(self.pendingSurface);
        CFRelease(self.pendingSurface);
    }
    self.pendingSurface = surface;
    *self.hasNewFrame = true;
}
@end

// ─── Internal state stored via m_impl ───────────────────────────────

struct MacWindowCaptureState {
    SCStream* stream = nil;
    EaselWindowCaptureDelegate* delegate = nil;
    dispatch_queue_t queue = nil;
    std::mutex bufferMutex;
    bool hasNewFrame = false;
    // Point-size the stream was configured for; compared against live
    // window bounds so a user resizing the call window re-configures the
    // stream instead of getting a scaled/letterboxed feed forever.
    int cfgPointW = 0, cfgPointH = 0;
    float pixelScale = 2.0f;
};

// ─── Enumeration ────────────────────────────────────────────────────

// Shared holder so a completion block that fires AFTER a semaphore
// timeout writes into memory that is still alive (and races are excluded
// by the mutex) instead of scribbling on a dead stack frame.
struct EnumResult {
    std::mutex m;
    std::vector<WindowInfo> windows;
    bool done = false;
};

std::vector<WindowInfo> WindowCaptureSource::enumerateWindows() {
    // onScreenWindowsOnly:NO is the point: a Meet/Zoom window that is
    // covered, on another Space, or on a disconnected display must still
    // be listed, or it can never be picked for capture.
    auto shared = std::make_shared<EnumResult>();
    dispatch_semaphore_t sem = dispatch_semaphore_create(0);

    [SCShareableContent getShareableContentExcludingDesktopWindows:YES
                                               onScreenWindowsOnly:NO
                                                 completionHandler:^(SCShareableContent* content, NSError* error) {
        if (content && !error) {
            std::lock_guard<std::mutex> lock(shared->m);
            for (SCWindow* w in content.windows) {
                if (w.windowLayer != 0) continue;                       // skip menus/overlays
                if (w.owningApplication.processID == getpid()) continue; // skip Easel itself
                CGSize sz = w.frame.size;
                if (sz.width < 50 || sz.height < 50) continue;

                std::string ownerName, windowTitle;
                if (w.owningApplication.applicationName)
                    ownerName = w.owningApplication.applicationName.UTF8String;
                if (w.title)
                    windowTitle = w.title.UTF8String;

                std::string name;
                if (!ownerName.empty() && !windowTitle.empty())
                    name = ownerName + " — " + windowTitle;
                else if (!ownerName.empty())
                    name = ownerName;
                else if (!windowTitle.empty())
                    name = windowTitle;
                else
                    name = "Window " + std::to_string((uint32_t)w.windowID);
                if (!w.onScreen) name += "  (off-screen)";

                WindowInfo info;
                info.windowID = (uint32_t)w.windowID;
                info.title = name;
                info.width = (int)sz.width;
                info.height = (int)sz.height;
                info.onScreen = w.onScreen;
                shared->windows.push_back(info);
            }
            shared->done = true;
        }
        dispatch_semaphore_signal(sem);
    }];

    dispatch_semaphore_wait(sem, dispatch_time(DISPATCH_TIME_NOW, 3 * NSEC_PER_SEC));

    {
        std::lock_guard<std::mutex> lock(shared->m);
        if (shared->done) {
            // On-screen windows first — the common pick — then off-screen.
            std::stable_partition(shared->windows.begin(), shared->windows.end(),
                                  [](const WindowInfo& w) { return w.onScreen; });
            return shared->windows;
        }
    }

    // SCK unavailable/denied — legacy CGWindowList enumeration (on-screen only).
    std::vector<WindowInfo> result;
    CFArrayRef windowList = CGWindowListCopyWindowInfo(
        kCGWindowListOptionOnScreenOnly | kCGWindowListExcludeDesktopElements,
        kCGNullWindowID
    );
    if (!windowList) return result;
    CFIndex count = CFArrayGetCount(windowList);
    for (CFIndex i = 0; i < count; i++) {
        CFDictionaryRef windowInfo = (CFDictionaryRef)CFArrayGetValueAtIndex(windowList, i);
        CGWindowID windowID = 0;
        CFNumberRef windowIDRef = (CFNumberRef)CFDictionaryGetValue(windowInfo, kCGWindowNumber);
        if (windowIDRef) CFNumberGetValue(windowIDRef, kCFNumberIntType, &windowID);

        std::string ownerName, windowTitle;
        CFStringRef ownerRef = (CFStringRef)CFDictionaryGetValue(windowInfo, kCGWindowOwnerName);
        char buf[512];
        if (ownerRef && CFStringGetCString(ownerRef, buf, sizeof(buf), kCFStringEncodingUTF8))
            ownerName = buf;
        CFStringRef nameRef = (CFStringRef)CFDictionaryGetValue(windowInfo, kCGWindowName);
        if (nameRef && CFStringGetCString(nameRef, buf, sizeof(buf), kCFStringEncodingUTF8))
            windowTitle = buf;

        std::string name;
        if (!ownerName.empty() && !windowTitle.empty()) name = ownerName + " — " + windowTitle;
        else if (!ownerName.empty()) name = ownerName;
        else if (!windowTitle.empty()) name = windowTitle;
        else name = "Window " + std::to_string(windowID);

        CFDictionaryRef boundsRef = (CFDictionaryRef)CFDictionaryGetValue(windowInfo, kCGWindowBounds);
        CGRect bounds = {};
        if (boundsRef) CGRectMakeWithDictionaryRepresentation(boundsRef, &bounds);
        if (bounds.size.width < 50 || bounds.size.height < 50) continue;

        WindowInfo info;
        info.windowID = windowID;
        info.title = name;
        info.width = (int)bounds.size.width;
        info.height = (int)bounds.size.height;
        result.push_back(info);
    }
    CFRelease(windowList);
    return result;
}

// ─── Capture lifecycle ──────────────────────────────────────────────

WindowCaptureSource::~WindowCaptureSource() {
    stop();
}

static SCStreamConfiguration* makeWindowStreamConfig(int pixelW, int pixelH) {
    SCStreamConfiguration* config = [[SCStreamConfiguration alloc] init];
    config.width = pixelW;
    config.height = pixelH;
    config.pixelFormat = kCVPixelFormatType_32BGRA;
    config.minimumFrameInterval = CMTimeMake(1, 60);
    config.showsCursor = NO;
    config.queueDepth = 5;
    return config;
}

bool WindowCaptureSource::start(uint32_t windowID) {
    stop();
    m_windowID = windowID;

    auto* state = new MacWindowCaptureState();
    m_impl = state;

    struct StartResult {
        std::mutex m;
        bool success = false;
        std::string title;
        int pointW = 0, pointH = 0;
        float scale = 2.0f;
    };
    auto shared = std::make_shared<StartResult>();
    dispatch_semaphore_t sem = dispatch_semaphore_create(0);

    [SCShareableContent getShareableContentExcludingDesktopWindows:YES
                                               onScreenWindowsOnly:NO
                                                 completionHandler:^(SCShareableContent* content, NSError* error) {
        if (error || !content) {
            std::cerr << "[WindowCapture] Failed to get shareable content"
                      << (error ? std::string(": ") + error.localizedDescription.UTF8String : "")
                      << std::endl;
            dispatch_semaphore_signal(sem);
            return;
        }

        SCWindow* target = nil;
        for (SCWindow* w in content.windows) {
            if ((uint32_t)w.windowID == windowID) { target = w; break; }
        }
        if (!target) {
            std::cerr << "[WindowCapture] Window " << windowID << " not found" << std::endl;
            dispatch_semaphore_signal(sem);
            return;
        }

        // Desktop-independent filter: frames keep coming while the window
        // is occluded or on another Space — no need to keep it visible.
        SCContentFilter* filter = [[SCContentFilter alloc] initWithDesktopIndependentWindow:target];

        float scale = 2.0f;
        if (@available(macOS 14.0, *)) {
            scale = (float)filter.pointPixelScale;
        } else {
            NSScreen* screen = NSScreen.mainScreen;
            if (screen) scale = (float)screen.backingScaleFactor;
        }
        int pointW = (int)target.frame.size.width;
        int pointH = (int)target.frame.size.height;
        int pixelW = (int)(pointW * scale);
        int pixelH = (int)(pointH * scale);
        if (pixelW <= 0 || pixelH <= 0) {
            std::cerr << "[WindowCapture] Window " << windowID << " has zero size" << std::endl;
            dispatch_semaphore_signal(sem);
            return;
        }

        state->stream = [[SCStream alloc] initWithFilter:filter
                                           configuration:makeWindowStreamConfig(pixelW, pixelH)
                                                delegate:nil];
        state->delegate = [[EaselWindowCaptureDelegate alloc] init];
        state->delegate.bufferMutex = &state->bufferMutex;
        state->delegate.hasNewFrame = &state->hasNewFrame;
        state->queue = dispatch_queue_create("com.easel.windowcapture", DISPATCH_QUEUE_SERIAL);

        NSError* addError = nil;
        [state->stream addStreamOutput:state->delegate
                                  type:SCStreamOutputTypeScreen
                    sampleHandlerQueue:state->queue
                                 error:&addError];
        if (addError) {
            std::cerr << "[WindowCapture] Failed to add stream output: "
                      << addError.localizedDescription.UTF8String << std::endl;
            dispatch_semaphore_signal(sem);
            return;
        }

        std::string title;
        if (target.owningApplication.applicationName)
            title = target.owningApplication.applicationName.UTF8String;
        if (target.title && target.title.length) {
            if (!title.empty()) title += " — ";
            title += target.title.UTF8String;
        }

        [state->stream startCaptureWithCompletionHandler:^(NSError* startError) {
            if (startError) {
                std::cerr << "[WindowCapture] Failed to start capture: "
                          << startError.localizedDescription.UTF8String << std::endl;
            } else {
                std::lock_guard<std::mutex> lock(shared->m);
                shared->success = true;
                shared->title = title;
                shared->pointW = pointW;
                shared->pointH = pointH;
                shared->scale = scale;
            }
            dispatch_semaphore_signal(sem);
        }];
    }];

    dispatch_semaphore_wait(sem, dispatch_time(DISPATCH_TIME_NOW, 5 * NSEC_PER_SEC));

    bool ok = false;
    {
        std::lock_guard<std::mutex> lock(shared->m);
        if (shared->success) {
            ok = true;
            m_title = shared->title;
            state->cfgPointW = shared->pointW;
            state->cfgPointH = shared->pointH;
            state->pixelScale = shared->scale;
            m_width = (int)(shared->pointW * shared->scale);
            m_height = (int)(shared->pointH * shared->scale);
        }
    }

    if (ok) {
        m_active = true;
        m_boundsPollCountdown = 120;
        std::cout << "[WindowCapture] Started SCK capture of '" << m_title << "' "
                  << m_width << "x" << m_height
                  << " (works occluded / off-display)" << std::endl;
    } else {
        stop();
    }
    return ok;
}

void WindowCaptureSource::stop() {
    m_active = false;
    if (!m_impl) return;
    auto* state = (MacWindowCaptureState*)m_impl;
    if (state->stream) {
        dispatch_semaphore_t sem = dispatch_semaphore_create(0);
        [state->stream stopCaptureWithCompletionHandler:^(NSError* error) {
            dispatch_semaphore_signal(sem);
        }];
        dispatch_semaphore_wait(sem, dispatch_time(DISPATCH_TIME_NOW, 2 * NSEC_PER_SEC));
    }
    delete state;
    m_impl = nullptr;
}

void WindowCaptureSource::update() {
    if (!m_active || !m_impl) return;
    auto* state = (MacWindowCaptureState*)m_impl;

    // Track live window resizes (cheap single-window query, ~every 2s at
    // 60fps): reconfigure the stream so the feed stays 1:1 instead of
    // scaling the new window size into the old buffer.
    if (--m_boundsPollCountdown <= 0) {
        m_boundsPollCountdown = 120;
        CFArrayRef list = CGWindowListCopyWindowInfo(kCGWindowListOptionIncludingWindow, m_windowID);
        if (list && CFArrayGetCount(list) > 0) {
            CFDictionaryRef info = (CFDictionaryRef)CFArrayGetValueAtIndex(list, 0);
            CFDictionaryRef boundsRef = (CFDictionaryRef)CFDictionaryGetValue(info, kCGWindowBounds);
            CGRect bounds = {};
            if (boundsRef && CGRectMakeWithDictionaryRepresentation(boundsRef, &bounds)) {
                int pw = (int)bounds.size.width, ph = (int)bounds.size.height;
                if (pw > 0 && ph > 0 &&
                    (abs(pw - state->cfgPointW) > 1 || abs(ph - state->cfgPointH) > 1)) {
                    state->cfgPointW = pw;
                    state->cfgPointH = ph;
                    int pixelW = (int)(pw * state->pixelScale);
                    int pixelH = (int)(ph * state->pixelScale);
                    [state->stream updateConfiguration:makeWindowStreamConfig(pixelW, pixelH)
                                     completionHandler:^(NSError* error) {
                        if (error)
                            std::cerr << "[WindowCapture] Reconfigure failed: "
                                      << error.localizedDescription.UTF8String << std::endl;
                    }];
                }
            }
        }
        if (list) CFRelease(list);
    }

    IOSurfaceRef surface = nullptr;
    {
        std::lock_guard<std::mutex> lock(state->bufferMutex);
        if (!state->hasNewFrame) return;
        state->hasNewFrame = false;
        surface = state->delegate.pendingSurface;
        if (surface) {
            CFRetain(surface);
            IOSurfaceIncrementUseCount(surface);
        }
    }
    if (!surface) return;

    int sw = (int)IOSurfaceGetWidth(surface);
    int sh = (int)IOSurfaceGetHeight(surface);
    if (sw > 0 && sh > 0) {
        m_width = sw;
        m_height = sh;
        if (!m_texture.id() || m_texture.width() != sw || m_texture.height() != sh) {
            m_texture.createEmpty(sw, sh, GL_RGBA8);
        }
        // Zero-copy: bind the IOSurface directly as the GL texture backing.
        CGLContextObj cgl_ctx = CGLGetCurrentContext();
        glBindTexture(GL_TEXTURE_2D, m_texture.id());
        CGLTexImageIOSurface2D(cgl_ctx, GL_TEXTURE_2D, GL_RGBA8,
                               sw, sh,
                               GL_BGRA, GL_UNSIGNED_INT_8_8_8_8_REV,
                               surface, 0);
    }

    IOSurfaceDecrementUseCount(surface);
    CFRelease(surface);
}

#endif

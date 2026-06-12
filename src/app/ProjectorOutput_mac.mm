#ifdef __APPLE__
#define GLFW_EXPOSE_NATIVE_COCOA
#include <GLFW/glfw3.h>
#include <GLFW/glfw3native.h>
#import <Cocoa/Cocoa.h>

// Disable macOS NATIVE (green-button) fullscreen for a GLFW window.
//
// Crash root cause: when a projector opens or projectors are switched, macOS
// pulls another Easel window OUT of native fullscreen. That exit transition
// (-[_NSExitFullScreenTransitionController ...] → setStyleMask →
// viewDidChangeBackingProperties) reports a transiently-zero content scale,
// and GLFW's _glfwInputWindowContentScale() asserts scale>0 → abort()/SIGABRT.
// It is a hard assert, not a catchable ObjC exception. Marking windows
// FullScreenNone stops them ever entering native fullscreen, so the crashing
// transition never runs. Easel has its own borderless fullscreen, so native
// fullscreen isn't needed.
void disableNativeFullscreen(GLFWwindow* window) {
    NSWindow* nsWindow = (NSWindow*)glfwGetCocoaWindow(window);
    if (!nsWindow) return;
    @try {
        NSWindowCollectionBehavior b = [nsWindow collectionBehavior];
        b &= ~NSWindowCollectionBehaviorFullScreenPrimary;
        b &= ~NSWindowCollectionBehaviorFullScreenAuxiliary;
        b |=  NSWindowCollectionBehaviorFullScreenNone;
        [nsWindow setCollectionBehavior:b];
    } @catch (NSException* e) { NSLog(@"[Easel] disableNativeFullscreen: %@", e.reason); }
}

void makeWindowTrulyBorderless(GLFWwindow* window) {
    NSWindow* nsWindow = (NSWindow*)glfwGetCocoaWindow(window);
    if (!nsWindow) return;

    // Each Cocoa call is wrapped in @try/@catch because AppKit can throw
    // NSExceptions during display reconfiguration (monitor hotplug, fullscreen
    // transition in flight, etc). An ObjC exception that escapes here unwinds
    // through C++ frames and ends in abort(), which we saw in the projector-
    // selection crash. None of these calls are critical — GLFW already made
    // the window borderless via GLFW_DECORATED_FALSE — so swallow and log.
    @try {
        // Skip if already borderless to avoid the redundant transition that
        // triggered the original NSPerformVisuallyAtomicChange exception.
        if ([nsWindow styleMask] != NSWindowStyleMaskBorderless) {
            [nsWindow setStyleMask:NSWindowStyleMaskBorderless];
        }
    } @catch (NSException* e) {
        NSLog(@"[Projector] setStyleMask failed: %@ — continuing", e.reason);
    }

    @try { [nsWindow setLevel:NSScreenSaverWindowLevel]; }
    @catch (NSException* e) { NSLog(@"[Projector] setLevel failed: %@", e.reason); }

    @try { [nsWindow setHasShadow:NO]; } @catch (NSException*) {}

    // FullScreenNone (not Auxiliary): the projector window must never join a
    // native fullscreen transition — that path crashes GLFW (see above).
    @try { [nsWindow setCollectionBehavior:NSWindowCollectionBehaviorFullScreenNone]; }
    @catch (NSException* e) { NSLog(@"[Projector] setCollectionBehavior failed: %@", e.reason); }

    @try { [NSMenu setMenuBarVisible:NO]; } @catch (NSException*) {}
}
#endif

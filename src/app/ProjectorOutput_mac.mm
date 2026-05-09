#ifdef __APPLE__
#define GLFW_EXPOSE_NATIVE_COCOA
#include <GLFW/glfw3.h>
#include <GLFW/glfw3native.h>
#import <Cocoa/Cocoa.h>

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

    @try { [nsWindow setCollectionBehavior:NSWindowCollectionBehaviorFullScreenAuxiliary]; }
    @catch (NSException* e) { NSLog(@"[Projector] setCollectionBehavior failed: %@", e.reason); }

    @try { [NSMenu setMenuBarVisible:NO]; } @catch (NSException*) {}
}
#endif

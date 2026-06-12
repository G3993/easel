#ifdef __APPLE__
#import <Cocoa/Cocoa.h>

// macOS presentation toggle for borderless windowed fullscreen.
//
// Editor fullscreen uses a borderless window that covers the monitor instead
// of true (CGDisplayCapture) fullscreen — true fullscreen stalls the app for
// ~0.5-1s on every toggle while macOS does a display capture + video-mode set.
// A borderless window has no such stall, but unlike true fullscreen it does NOT
// auto-hide the menu bar / Dock, so the top nav row would sit under the menu
// bar. Mirror the true-fullscreen look by auto-hiding both while we're in
// borderless fullscreen, and restore the defaults on exit.
//
// AutoHide (rather than fully Hide) keeps the Apple/Figma feel: the bar slides
// in on a top-edge hover and back out otherwise. macOS requires the Dock option
// to accompany an auto-hidden menu bar, so they're toggled together.
extern "C" void macSetFullscreenPresentation(bool on) {
    @autoreleasepool {
        if (on) {
            [NSApp setPresentationOptions:
                (NSApplicationPresentationAutoHideMenuBar |
                 NSApplicationPresentationAutoHideDock)];
        } else {
            [NSApp setPresentationOptions:NSApplicationPresentationDefault];
        }
    }
}
#endif

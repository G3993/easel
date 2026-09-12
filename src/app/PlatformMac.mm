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
static bool gFsPresentation = false;   // borderless fullscreen is on
static bool gDockPeek = false;         // the Dock is being shown on purpose

static void applyPresentation() {
    if (gFsPresentation && !gDockPeek) {
        [NSApp setPresentationOptions:
            (NSApplicationPresentationAutoHideMenuBar |
             NSApplicationPresentationAutoHideDock)];
    } else {
        [NSApp setPresentationOptions:NSApplicationPresentationDefault];
    }
}

extern "C" void macSetFullscreenPresentation(bool on) {
    @autoreleasepool {
        gFsPresentation = on;
        gDockPeek = false;
        applyPresentation();
    }
}

// Dock peek: macOS only slides an auto-hidden Dock out on a hard edge push,
// which a borderless window makes unreliable. Easel watches the cursor itself
// and, when it rests on the bottom edge, drops the auto-hide so the Dock is
// simply there; it goes back to hidden once the cursor leaves the band.
extern "C" void macFullscreenDockPeek(bool peek) {
    @autoreleasepool {
        if (!gFsPresentation || gDockPeek == peek) return;
        gDockPeek = peek;
        applyPresentation();
    }
}

// ── activation: clicking the Dock icon must always bring Easel forward ──
// GLFW installs its own NSApplicationDelegate and does not implement
// applicationShouldHandleReopen:, so with another GLFW app (AVA OS) in front
// a Dock click could activate Easel without raising its window. This proxy
// sits in front of GLFW's delegate, answers reopen + didBecomeActive, and
// forwards everything else untouched.
@interface EaselAppDelegateProxy : NSObject <NSApplicationDelegate>
@property (nonatomic, strong) id<NSApplicationDelegate> inner;
@property (nonatomic, assign) NSWindow* mainWindow;
@end
@implementation EaselAppDelegateProxy
- (BOOL)respondsToSelector:(SEL)sel {
    return [super respondsToSelector:sel] || [self.inner respondsToSelector:sel];
}
- (id)forwardingTargetForSelector:(SEL)sel {
    if ([self.inner respondsToSelector:sel]) return self.inner;
    return [super forwardingTargetForSelector:sel];
}
- (BOOL)applicationShouldHandleReopen:(NSApplication*)sender hasVisibleWindows:(BOOL)flag {
    NSWindow* w = self.mainWindow;
    if (w) {
        if (w.miniaturized) [w deminiaturize:nil];
        [w makeKeyAndOrderFront:nil];
    }
    [NSApp activateIgnoringOtherApps:YES];
    return NO;
}
- (void)applicationDidBecomeActive:(NSNotification*)note {
    if ([self.inner respondsToSelector:@selector(applicationDidBecomeActive:)])
        [self.inner applicationDidBecomeActive:note];
    applyPresentation();                 // re-assert fullscreen chrome after a switch
    NSWindow* w = self.mainWindow;
    if (w && !w.keyWindow) [w makeKeyAndOrderFront:nil];
}
@end

static EaselAppDelegateProxy* gDelegateProxy = nil;

extern "C" void macInstallActivationHooks(void* nsWindow) {
    @autoreleasepool {
        if (!gDelegateProxy) {
            gDelegateProxy = [[EaselAppDelegateProxy alloc] init];
            gDelegateProxy.inner = NSApp.delegate;
            [NSApp setDelegate:gDelegateProxy];
        }
        gDelegateProxy.mainWindow = (__bridge NSWindow*)nsWindow;
    }
}
#endif

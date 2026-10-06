// Unified title bar for the Easel window — mirrors the Figma / VS Code look
// where app chrome (menus, tabs, actions) sits inline with the traffic-light
// buttons instead of below a separate title bar strip.
//
// The NSWindow style changes below make the title bar transparent and
// extend the content view into it; the caller is then responsible for
// leaving ~78px of empty space on the left of the top row so the red/
// yellow/green buttons remain clickable.

#import <Cocoa/Cocoa.h>
#include <initializer_list>
#define GLFW_EXPOSE_NATIVE_COCOA
#include <GLFW/glfw3.h>
#include <GLFW/glfw3native.h>

// ── Green traffic-light button → Easel's own (borderless) fullscreen ──
// macOS NATIVE fullscreen is deliberately disabled on the editor window
// (it SIGABRTs when a projector / display change force-exits it — see
// ProjectorOutput_mac.mm disableNativeFullscreen). So the green button is
// re-targeted to set a request flag the app drains each frame, where it runs
// the same borderless toggleEditorFullscreen() as F11 / the in-app button.
// Option-clicking the green button still performs the classic zoom/maximize.
static BOOL gZoomFsRequested = NO;

@interface EaselZoomTarget : NSObject
@property(assign) NSWindow* win;
- (void)onZoom:(id)sender;
@end
@implementation EaselZoomTarget
- (void)onZoom:(id)sender {
    if ([NSEvent modifierFlags] & NSEventModifierFlagOption) {
        [self.win zoom:sender];        // preserve option-click maximize
    } else {
        gZoomFsRequested = YES;        // plain click → Easel fullscreen
    }
}
@end
static EaselZoomTarget* gZoomTarget = nil;

// Returns 1 (and clears) if the green button was clicked since the last call.
extern "C" int EaselMac_ConsumeZoomFullscreenRequest() {
    if (gZoomFsRequested) { gZoomFsRequested = NO; return 1; }
    return 0;
}

extern "C" int EaselMac_IsNativeFullScreen(GLFWwindow* window) {
    if (!window) return 0;
    NSWindow* ns = glfwGetCocoaWindow(window);
    if (!ns) return 0;
    return (ns.styleMask & NSWindowStyleMaskFullScreen) ? 1 : 0;
}

extern "C" void EaselMac_ExitNativeFullScreen(GLFWwindow* window) {
    if (!window) return;
    NSWindow* ns = glfwGetCocoaWindow(window);
    if (!ns) return;
    if (!(ns.styleMask & NSWindowStyleMaskFullScreen)) return;
    // Use AppKit's own transition so the FS state stays consistent —
    // glfwSetWindowMonitor against a native-FS NSWindow crashes.
    [ns toggleFullScreen:nil];
}

extern "C" void EaselMac_UnifyTitleBar(GLFWwindow* window) {
    if (!window) return;
    NSWindow* ns = glfwGetCocoaWindow(window);
    if (!ns) return;

    ns.titlebarAppearsTransparent = YES;
    ns.titleVisibility = NSWindowTitleHidden;
    ns.styleMask |= NSWindowStyleMaskFullSizeContentView;
    // Make the green traffic-light button enter NATIVE macOS fullscreen
    // (hides the Dock + menu bar) instead of just zooming/maximizing the
    // window. Without this flag GLFW windows default to "zoom" on the green
    // button. (Option-click still does the old zoom.)
    ns.collectionBehavior |= NSWindowCollectionBehaviorFullScreenPrimary;
    // Suppress AppKit's "+" tab-add button and tab-bar overlay that can
    // peek through the chrome row near the traffic-light cluster as a
    // small dark sliver. Easel never opens window tabs, so disallow.
    if ([ns respondsToSelector:@selector(setTabbingMode:)]) {
        ns.tabbingMode = NSWindowTabbingModeDisallowed;
    }
    // Some AppKit builds add a translucent edge on the title-bar separator
    // between titlebar and content view; clear it so our gray bg is the
    // sole tone in that band.
    if ([ns respondsToSelector:@selector(setTitlebarSeparatorStyle:)]) {
        ns.titlebarSeparatorStyle = NSTitlebarSeparatorStyleNone;
    }
    // Move the standard buttons down so they sit on the same vertical
    // centerline as the ImGui menu row (which starts at y=0 of the content
    // view). Default AppKit position is ~3px from the window top edge; a
    // ~7px offset lines them up with a 28px-tall main menu bar.
    NSButton* close = [ns standardWindowButton:NSWindowCloseButton];
    NSButton* mini  = [ns standardWindowButton:NSWindowMiniaturizeButton];
    NSButton* zoom  = [ns standardWindowButton:NSWindowZoomButton];
    // Re-target the green (zoom) button to Easel's borderless fullscreen. Done
    // here so it is (re)bound every time this runs — including after exiting
    // fullscreen, where toggling GLFW_DECORATED recreates the standard buttons
    // with their default zoom: action.
    if (zoom) {
        if (!gZoomTarget) gZoomTarget = [[EaselZoomTarget alloc] init];
        gZoomTarget.win = ns;
        zoom.target = gZoomTarget;
        zoom.action = @selector(onZoom:);
    }
    for (NSButton* b : {close, mini, zoom}) {
        if (!b) continue;
        NSView* bar = b.superview;
        if (!bar) continue;
        NSRect frame = b.frame;
        // Centre the traffic-light buttons vertically within the title-bar
        // band so they line up with the menu-text baseline in the ImGui
        // main menu row below. The title-bar view uses bottom-origin
        // coordinates, so (bar.height - btn.height) / 2 centres the button.
        frame.origin.y = (bar.frame.size.height - frame.size.height) * 0.5;
        b.frame = frame;
    }
}


// ── Displays: how many are online, and "extend, laptop as main" ─────────
// macOS sometimes brings a freshly plugged projector up MIRRORED with the
// projector as the main display: the editor and every new window land on
// the projector, GLFW sees one monitor and no output can open. Easel calls
// this when it notices (see the hot-plug block in Application::run).
#include <CoreGraphics/CoreGraphics.h>
extern "C" int EaselMac_OnlineDisplayCount(void) {
    uint32_t n = 0; CGDirectDisplayID ids[16];
    CGGetOnlineDisplayList(16, ids, &n);
    return (int)n;
}
// Only the LAPTOP screen being in a mirror set is a problem (the editor
// ends up on the projector). Two projectors mirroring each other while the
// laptop is extended is a legitimate show setup and is left alone.
extern "C" int EaselMac_DisplaysMirrored(void) {
    uint32_t n = 0; CGDirectDisplayID ids[16];
    CGGetOnlineDisplayList(16, ids, &n);
    CGDirectDisplayID builtin = kCGNullDirectDisplay;
    for (uint32_t i = 0; i < n; i++) if (CGDisplayIsBuiltin(ids[i])) builtin = ids[i];
    if (builtin == kCGNullDirectDisplay) return 0;
    if (CGDisplayMirrorsDisplay(builtin) != kCGNullDirectDisplay) return 1;          // laptop mirrors a projector
    for (uint32_t i = 0; i < n; i++) if (CGDisplayMirrorsDisplay(ids[i]) == builtin) return 1;   // a projector mirrors the laptop
    return 0;
}
extern "C" int EaselMac_ExtendDisplays(void) {
    uint32_t n = 0; CGDirectDisplayID ids[16];
    CGGetOnlineDisplayList(16, ids, &n);
    CGDirectDisplayID builtin = kCGNullDirectDisplay;
    for (uint32_t i = 0; i < n; i++) if (CGDisplayIsBuiltin(ids[i])) builtin = ids[i];
    if (builtin == kCGNullDirectDisplay || n < 2) return 0;
    CGDisplayConfigRef cfg = NULL;
    if (CGBeginDisplayConfiguration(&cfg) != kCGErrorSuccess) return 0;
    // take only the laptop out of its mirror set; projector↔projector mirrors stay
    CGConfigureDisplayMirrorOfDisplay(cfg, builtin, kCGNullDirectDisplay);
    for (uint32_t i = 0; i < n; i++) if (CGDisplayMirrorsDisplay(ids[i]) == builtin) CGConfigureDisplayMirrorOfDisplay(cfg, ids[i], kCGNullDirectDisplay);
    CGConfigureDisplayOrigin(cfg, builtin, 0, 0);               // laptop = main
    int32_t x = (int32_t)CGDisplayPixelsWide(builtin);
    for (uint32_t i = 0; i < n; i++) if (ids[i] != builtin && CGDisplayMirrorsDisplay(ids[i]) == kCGNullDirectDisplay) { CGConfigureDisplayOrigin(cfg, ids[i], x, 0); x += (int32_t)CGDisplayPixelsWide(ids[i]); }
    return CGCompleteDisplayConfiguration(cfg, kCGConfigurePermanently) == kCGErrorSuccess ? 1 : 0;
}

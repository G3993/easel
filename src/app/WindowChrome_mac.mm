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

extern "C" int EaselMac_IsNativeFullScreen(GLFWwindow* window) {
    if (!window) return 0;
    NSWindow* ns = glfwGetCocoaWindow(window);
    if (!ns) return 0;
    return (ns.styleMask & NSWindowStyleMaskFullScreen) ? 1 : 0;
}

extern "C" void EaselMac_UnifyTitleBar(GLFWwindow* window) {
    if (!window) return;
    NSWindow* ns = glfwGetCocoaWindow(window);
    if (!ns) return;

    ns.titlebarAppearsTransparent = YES;
    ns.titleVisibility = NSWindowTitleHidden;
    ns.styleMask |= NSWindowStyleMaskFullSizeContentView;
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

#ifdef __APPLE__
#import <Cocoa/Cocoa.h>
#include <string>
#include <iostream>

// Activate Easel so dialogs appear on top, raise the panel above the GLFW
// window level, and log so we can tell from the console whether the dialog
// was ever invoked (silent failures here are the usual "click does nothing"
// symptom: the panel opens behind the app window and the user never sees it).

std::string openFileDialog_mac(const char* filter) {
    @autoreleasepool {
        std::cerr << "[FileDialog] openFileDialog_mac invoked" << std::endl;
        [NSApp activateIgnoringOtherApps:YES];
        NSOpenPanel* panel = [NSOpenPanel openPanel];
        [panel setCanChooseFiles:YES];
        [panel setCanChooseDirectories:NO];
        [panel setAllowsMultipleSelection:NO];
        [panel setLevel:NSModalPanelWindowLevel];

        if ([panel runModal] == NSModalResponseOK) {
            NSURL* url = [[panel URLs] objectAtIndex:0];
            return std::string([[url path] UTF8String]);
        }
    }
    return "";
}

std::string saveFileDialog_mac(const char* filter, const char* defaultExt) {
    @autoreleasepool {
        std::cerr << "[FileDialog] saveFileDialog_mac invoked" << std::endl;
        [NSApp activateIgnoringOtherApps:YES];
        NSSavePanel* panel = [NSSavePanel savePanel];
        if (defaultExt && strlen(defaultExt) > 0) {
            NSString* ext = [NSString stringWithUTF8String:defaultExt];
            [panel setAllowedContentTypes:@[]]; // Allow all, rely on extension
            [panel setNameFieldStringValue:[NSString stringWithFormat:@"untitled.%@", ext]];
        }
        [panel setLevel:NSModalPanelWindowLevel];

        if ([panel runModal] == NSModalResponseOK) {
            NSURL* url = [panel URL];
            return std::string([[url path] UTF8String]);
        }
    }
    return "";
}
#endif

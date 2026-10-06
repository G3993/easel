#include "app/ProjectorOutput.h"
#include <iostream>
#include <cstdio>
#include <cstdlib>

#ifdef __APPLE__
extern void makeWindowTrulyBorderless(GLFWwindow* window);
#endif

// EASEL_PROJECTOR_RECT="x,y,w,h" pins the projector window to a fixed
// rectangle of the desktop instead of requiring a second monitor. This is for
// single-head appliances (e.g. the Jetson): the GPU scans out only the left
// part of an oversized framebuffer to the physical projector, while the right
// part holds the editor and is visible only remotely (NoMachine).
static bool forcedProjectorRect(int& x, int& y, int& w, int& h) {
    const char* env = getenv("EASEL_PROJECTOR_RECT");
    if (!env) return false;
    return sscanf(env, "%d,%d,%d,%d", &x, &y, &w, &h) == 4 && w > 0 && h > 0;
}

ProjectorOutput::~ProjectorOutput() {
    destroy();
}

std::vector<MonitorInfo> ProjectorOutput::enumerateMonitors() {
    std::vector<MonitorInfo> result;
    int count;
    GLFWmonitor** monitors = glfwGetMonitors(&count);

    for (int i = 0; i < count; i++) {
        // During a display reconfiguration (projector plug/switch) GLFW can
        // briefly report a monitor whose video mode is null. Dereferencing it
        // crashed; query defensively and fall back to a sane size so indices
        // stay stable and projector routing recovers next frame.
        const GLFWvidmode* mode = glfwGetVideoMode(monitors[i]);
        const char* nm = glfwGetMonitorName(monitors[i]);
        MonitorInfo info;
        info.monitor = monitors[i];
        // a projector with no EDID name used to list as a blank label — give it one
        info.name = (nm && *nm) ? nm : ("Display " + std::to_string(i + 1));
        info.x = 0; info.y = 0;
        glfwGetMonitorPos(monitors[i], &info.x, &info.y);
        info.width  = mode ? mode->width  : 1920;
        info.height = mode ? mode->height : 1080;
        result.push_back(info);
    }

    return result;
}

std::string ProjectorOutput::monitorKey(const std::vector<MonitorInfo>& mons, int idx) {
    if (idx < 0 || idx >= (int)mons.size()) return "";
    int ord = 0;
    for (int i = 0; i < idx; i++) if (mons[i].name == mons[idx].name) ord++;
    return mons[idx].name + "#" + std::to_string(ord) + " " + std::to_string(mons[idx].width) + "x" + std::to_string(mons[idx].height);
}
int ProjectorOutput::findMonitorByKey(const std::vector<MonitorInfo>& mons, const std::string& key) {
    if (key.empty()) return -1;
    for (int i = 0; i < (int)mons.size(); i++) if (monitorKey(mons, i) == key) return i;
    // same name + ordinal but a different mode (the projector changed resolution): still it
    std::string stem = key.substr(0, key.find(' '));
    for (int i = 0; i < (int)mons.size(); i++) { std::string k = monitorKey(mons, i); if (k.substr(0, k.find(' ')) == stem) return i; }
    // an unnamed projector ("Display N" / "" stem): match on size alone
    std::string sz = key.substr(key.rfind(' ') + 1);
    bool anon = stem.empty() || stem.rfind("Display", 0) == 0 || key[0] == '#';
    if (anon) for (int i = 0; i < (int)mons.size(); i++) { if (mons[i].name.rfind("Display", 0) == 0 && std::to_string(mons[i].width) + "x" + std::to_string(mons[i].height) == sz) return i; }
    // ...and if its resolution changed (720p → 1080p), the only unnamed external is still it
    if (anon) { int found = -1, cnt = 0; for (int i = 0; i < (int)mons.size(); i++) if (mons[i].name.rfind("Display", 0) == 0) { found = i; cnt++; } if (cnt == 1) return found; }
    return -1;
}

int ProjectorOutput::findSecondaryMonitor(GLFWwindow* mainWindow) {
    auto monitors = enumerateMonitors();
    if (monitors.size() < 2) return -1;

    // Figure out which monitor the main window is on
    int mainX, mainY;
    glfwGetWindowPos(mainWindow, &mainX, &mainY);

    int primaryIndex = 0;
    for (int i = 0; i < (int)monitors.size(); i++) {
        const auto& m = monitors[i];
        if (mainX >= m.x && mainX < m.x + m.width &&
            mainY >= m.y && mainY < m.y + m.height) {
            primaryIndex = i;
            break;
        }
    }

    // Return the first monitor that ISN'T the one the main window is on
    for (int i = 0; i < (int)monitors.size(); i++) {
        if (i != primaryIndex) return i;
    }

    return -1;
}

bool ProjectorOutput::create(GLFWwindow* mainWindow, int monitorIndex) {
    destroy();

    int tx, ty, tw, th;
    if (forcedProjectorRect(tx, ty, tw, th)) {
        // Pinned-rect mode: no second monitor required, and the same-monitor
        // safety check doesn't apply (the editor is parked outside the rect).
        m_monitorIndex = monitorIndex < 0 ? 0 : monitorIndex;
    } else {
        auto monitors = enumerateMonitors();
        if (monitorIndex < 0 || monitorIndex >= (int)monitors.size()) {
            std::cerr << "Invalid monitor index " << monitorIndex << std::endl;
            return false;
        }

        // Safety: refuse to open on the same monitor as the main window
        int mainX, mainY;
        glfwGetWindowPos(mainWindow, &mainX, &mainY);
        const auto& target = monitors[monitorIndex];
        if (mainX >= target.x && mainX < target.x + target.width &&
            mainY >= target.y && mainY < target.y + target.height) {
            std::cerr << "Refusing to open projector on the same monitor as the editor" << std::endl;

            // Try to find a different monitor instead
            int alt = findSecondaryMonitor(mainWindow);
            if (alt < 0) {
                std::cerr << "No secondary monitor available" << std::endl;
                return false;
            }
            monitorIndex = alt;
        }

        const auto& mi = monitors[monitorIndex];
        m_monitorIndex = monitorIndex;
        tx = mi.x; ty = mi.y; tw = mi.width; th = mi.height;
    }

    m_mainWindow = mainWindow;
    m_width = tw;
    m_height = th;
    m_targetX = tx; m_targetY = ty;

    glfwWindowHint(GLFW_DECORATED, GLFW_FALSE);
    glfwWindowHint(GLFW_AUTO_ICONIFY, GLFW_FALSE);
    glfwWindowHint(GLFW_FOCUS_ON_SHOW, GLFW_FALSE);
    // NOT floating/always-on-top — that traps users if it opens on the wrong screen

    m_window = glfwCreateWindow(tw, th, "Easel Projector", nullptr, mainWindow);

    // Reset hints immediately
    glfwWindowHint(GLFW_DECORATED, GLFW_TRUE);
    glfwWindowHint(GLFW_FOCUS_ON_SHOW, GLFW_TRUE);

    if (!m_window) {
        std::cerr << "Failed to create projector window" << std::endl;
        return false;
    }

    // Position on the target monitor / pinned rect
    glfwSetWindowPos(m_window, tx, ty);
    glfwSetWindowSize(m_window, tw, th);

#ifdef __APPLE__
    // Remove macOS title bar / border that can appear even with GLFW_DECORATED=FALSE
    makeWindowTrulyBorderless(m_window);
#endif

    // Escape key closes the projector window
    {   // where did AppKit actually put it? (a rect off every screen gets
        // constrained onto the main display — the "projector over the editor" glitch)
        int ax = 0, ay = 0; glfwGetWindowPos(m_window, &ax, &ay);
        int aw = 0, ah = 0; glfwGetWindowSize(m_window, &aw, &ah);
        std::cout << "[Projector] window placed at " << ax << "," << ay << " " << aw << "x" << ah
                  << " (wanted " << tx << "," << ty << " " << tw << "x" << th << ")" << std::endl;
    }
    ensurePlacement();   // AppKit sometimes constrains the first placement onto the main display
    glfwSetWindowUserPointer(m_window, this);
    glfwSetKeyCallback(m_window, [](GLFWwindow* win, int key, int, int action, int) {
        if (key == GLFW_KEY_ESCAPE && action == GLFW_PRESS) {
            auto* self = (ProjectorOutput*)glfwGetWindowUserPointer(win);
            if (self) self->bringEditorBack();
        }
    });

    // Create GL resources in the projector context (VAOs aren't shared)
    glfwMakeContextCurrent(m_window);
    glfwSwapInterval(0);

    m_quad.createQuad();
    if (!m_shader.loadFromFiles("shaders/passthrough.vert", "shaders/passthrough.frag")) {
        std::cerr << "Failed to load projector shader" << std::endl;
        glfwMakeContextCurrent(mainWindow);
        destroy();
        return false;
    }

    glfwMakeContextCurrent(mainWindow);

    std::cout << "Projector opened on monitor " << m_monitorIndex
              << " (" << tw << "x" << th << " at " << tx << "," << ty << ")" << std::endl;

    return true;
}

// The window must sit exactly on its monitor. AppKit constrains a window
// whose frame it does not consider on-screen (first placement at startup,
// display rearrangements) onto the main display — that is the "projector
// over the editor" glitch. Put it back whenever it has drifted.
void ProjectorOutput::ensurePlacement() {
    if (!m_window) return;
    int ax = 0, ay = 0;
    glfwGetWindowPos(m_window, &ax, &ay);
    if (ax != m_targetX || ay != m_targetY) {
        glfwSetWindowPos(m_window, m_targetX, m_targetY);
        glfwSetWindowSize(m_window, m_width, m_height);
        int bx = 0, by = 0; glfwGetWindowPos(m_window, &bx, &by);
        std::cout << "[Projector] re-placed from " << ax << "," << ay << " to " << bx << "," << by
                  << " (target " << m_targetX << "," << m_targetY << ")" << std::endl;
    }
}

void ProjectorOutput::bringEditorBack() {
    ensurePlacement();
    if (m_mainWindow) {
        glfwFocusWindow(m_mainWindow);
        std::cout << "[Projector] Esc: editor brought back (output stays live)" << std::endl;
    }
}

void ProjectorOutput::destroy() {
    if (m_window) {
        // Clean up VAO in the projector's GL context (VAOs are per-context;
        // deleting in the wrong context can destroy unrelated main-context VAOs
        // that happen to share the same numeric ID).
        GLFWwindow* prev = glfwGetCurrentContext();
        glfwMakeContextCurrent(m_window);
        m_quad.destroy();
        if (prev && prev != m_window) glfwMakeContextCurrent(prev);

        glfwDestroyWindow(m_window);
        m_window = nullptr;
        std::cout << "Projector closed" << std::endl;
    }
    m_monitorIndex = -1;
    m_closeRequested = false;
}

void ProjectorOutput::present(GLuint texture) {
    presentCrop(texture, 0.0f, 0.0f, 1.0f, 1.0f);
}

void ProjectorOutput::presentCrop(GLuint texture, float uOffX, float uOffY,
                                  float uScaleX, float uScaleY) {
    if (!m_window || !texture) return;
    if ((m_placeCheck++ % 30) == 0) ensurePlacement();   // twice a second

    // Check if close was requested (e.g. Escape pressed on projector window)
    if (m_closeRequested) {
        destroy();
        return;
    }

    // The composite/warp commands for this texture were queued on the MAIN
    // context; this projector context must not sample it before they
    // complete. Fence + server-side glWaitSync keeps the wait on the GPU —
    // the caller used to glFinish() every frame instead, which drained the
    // whole pipeline on the CPU and serialized CPU and GPU.
    GLsync fence = glFenceSync(GL_SYNC_GPU_COMMANDS_COMPLETE, 0);
    glFlush(); // make the fence reachable from the projector context

    glfwMakeContextCurrent(m_window);
    if (fence) glWaitSync(fence, 0, GL_TIMEOUT_IGNORED);

    int fbW, fbH;
    glfwGetFramebufferSize(m_window, &fbW, &fbH);
    glViewport(0, 0, fbW, fbH);

    glClearColor(0, 0, 0, 1);
    glClear(GL_COLOR_BUFFER_BIT);

    m_shader.use();
    m_shader.setInt("uTexture", 0);
    m_shader.setFloat("uOpacity", 1.0f);
    m_shader.setMat3("uTransform", glm::mat3(1.0f));
    // Slice the source texture (identity 0,0,1,1 for the full-frame present()).
    m_shader.setVec2("uUVOffset", glm::vec2(uOffX, uOffY));
    m_shader.setVec2("uUVScale",  glm::vec2(uScaleX, uScaleY));

    glActiveTexture(GL_TEXTURE0);
    glBindTexture(GL_TEXTURE_2D, texture);

    m_quad.draw();

    glfwSwapBuffers(m_window);
    glfwMakeContextCurrent(m_mainWindow);
    // Safe while the wait is queued: deletion is deferred until no context
    // is using the sync object.
    if (fence) glDeleteSync(fence);
}

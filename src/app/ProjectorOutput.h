#pragma once
#include "render/Mesh.h"
#include "render/ShaderProgram.h"
#include <GLFW/glfw3.h>
#include <string>
#include <vector>

struct MonitorInfo {
    GLFWmonitor* monitor;
    std::string name;
    int x, y, width, height;
};

class ProjectorOutput {
public:
    ~ProjectorOutput();

    static std::vector<MonitorInfo> enumerateMonitors();

    // Find a monitor that isn't the one the main window is on. Returns -1 if none.
    static int findSecondaryMonitor(GLFWwindow* mainWindow);
    // identity that survives hot-plug: name + ordinal among same-named monitors + size
    static std::string monitorKey(const std::vector<MonitorInfo>& mons, int idx);
    static int findMonitorByKey(const std::vector<MonitorInfo>& mons, const std::string& key);

    // Create fullscreen window on specified monitor, sharing context with mainWindow.
    // Will refuse to open on the same monitor as mainWindow.
    bool create(GLFWwindow* mainWindow, int monitorIndex);
    void destroy();

    void present(GLuint texture);
    // Present a horizontal/vertical sub-rect of the source texture. UV space:
    // (uOffX,uOffY) is the bottom-left of the slice, (uScaleX,uScaleY) its size.
    // e.g. left half  = present(t, 0.0, 0.0, 0.5, 1.0)
    //      right half = present(t, 0.5, 0.0, 0.5, 1.0)
    void presentCrop(GLuint texture, float uOffX, float uOffY,
                     float uScaleX, float uScaleY);

    void requestClose() { m_closeRequested = true; }
    // is this window covering a screen point? (used when it has been
    // put on the editor's display by a mirror / relocation)
    bool coversPoint(int px, int py) const { if (!m_window) return false; int x, y, w, h; glfwGetWindowPos(m_window, &x, &y); glfwGetWindowSize(m_window, &w, &h); return px >= x && px < x + w && py >= y && py < y + h; }
    // Esc on this window = bring the editor back (focus it, and put this
    // window back on its monitor if AppKit moved it). Never closes the output.
    void bringEditorBack();
    void ensurePlacement();

    bool isActive() const { return m_window != nullptr; }
    GLFWwindow* window() const { return m_window; }
    int monitorIndex() const { return m_monitorIndex; }
    int projectorWidth() const { return m_width; }
    int projectorHeight() const { return m_height; }
    float aspectRatio() const { return m_height > 0 ? (float)m_width / m_height : 16.0f / 9.0f; }

private:
    GLFWwindow* m_window = nullptr;
    GLFWwindow* m_mainWindow = nullptr;
    int m_monitorIndex = -1;
    int m_width = 0, m_height = 0;
    bool m_closeRequested = false;
    int  m_targetX = 0, m_targetY = 0;   // where this window belongs (its monitor's origin)
    int  m_placeCheck = 0;

    Mesh m_quad;
    ShaderProgram m_shader;
};

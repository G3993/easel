#include "scene/SceneCamera.h"
#include <glm/gtc/matrix_transform.hpp>
#include <cmath>
#include <algorithm>

namespace scene {

glm::vec3 directionFromAngles(float azimuthDeg, float elevationDeg) {
    float az = glm::radians(azimuthDeg), el = glm::radians(elevationDeg);
    return glm::normalize(glm::vec3(std::cos(el) * std::sin(az), std::sin(el), std::cos(el) * std::cos(az)));
}

CameraMatrices buildCamera(const SceneCameraParams& cam, float aspect) {
    CameraMatrices m;
    if (aspect <= 0.0f) aspect = 1.0f;
    float dist = std::max(cam.distance, 0.05f);
    glm::vec3 dir = directionFromAngles(cam.azimuth, cam.elevation);
    m.eye = cam.target + dir * dist;
    m.forward = -dir;
    // Up vector with screen roll: rotate world-up about the view axis.
    glm::vec3 up(0.0f, 1.0f, 0.0f);
    if (std::fabs(cam.elevation) > 89.5f) up = glm::vec3(0.0f, 0.0f, -1.0f);  // looking straight down/up
    if (std::fabs(cam.roll) > 1e-4f) {
        glm::mat4 r = glm::rotate(glm::mat4(1.0f), glm::radians(cam.roll), m.forward);
        up = glm::vec3(r * glm::vec4(up, 0.0f));
    }
    m.view = glm::lookAt(m.eye, cam.target, up);
    // dolly: + zooms in (frame shrinks), - zooms out. exp keeps it symmetric.
    float zoom = std::exp(-cam.dolly);
    m.perspective = cam.perspective;
    if (cam.perspective) {
        // Auto fov frames the canvas (half-height 1) exactly at `distance`,
        // so flipping ortho -> perspective never jumps the frame.
        float fovY = cam.fovDeg > 0.5f ? glm::radians(cam.fovDeg) : 2.0f * std::atan(1.0f / dist);
        fovY = 2.0f * std::atan(std::tan(fovY * 0.5f) * zoom);
        fovY = std::min(std::max(fovY, glm::radians(1.0f)), glm::radians(175.0f));
        float nearP = std::max(0.02f, dist * 0.01f);
        m.proj = glm::perspective(fovY, aspect, nearP, dist + 200.0f);
    } else {
        float h = 1.0f * zoom;
        m.proj = glm::ortho(-aspect * h, aspect * h, -h, h, -100.0f, 100.0f + dist);
    }
    m.viewProj = m.proj * m.view;
    return m;
}

LightMatrices buildLight(const SceneLightParams& light, float aspect, float sceneRadius) {
    LightMatrices l;
    l.dir = directionFromAngles(light.azimuth, light.elevation);
    float r = sceneRadius > 0.0f ? sceneRadius : (std::sqrt(aspect * aspect + 1.0f) * 1.6f);
    glm::vec3 center(0.0f);
    glm::vec3 up(0.0f, 1.0f, 0.0f);
    if (std::fabs(glm::dot(l.dir, up)) > 0.995f) up = glm::vec3(0.0f, 0.0f, -1.0f);
    l.view = glm::lookAt(center + l.dir * (r * 2.0f), center, up);
    l.proj = glm::ortho(-r, r, -r, r, 0.05f, r * 4.0f);
    l.viewProj = l.proj * l.view;
    return l;
}

glm::mat4 cardModel(const glm::mat3& x, float aspect, float zOffset, const glm::vec3& rotDeg) {
    // 3x3 NDC affine -> 4x4 (z untouched)
    glm::mat4 m4(1.0f);
    m4[0][0] = x[0][0]; m4[0][1] = x[0][1];
    m4[1][0] = x[1][0]; m4[1][1] = x[1][1];
    m4[3][0] = x[2][0]; m4[3][1] = x[2][1];
    glm::mat4 A = glm::scale(glm::mat4(1.0f), glm::vec3(aspect, 1.0f, 1.0f));
    glm::mat4 flat = A * m4;
    bool rotated = std::fabs(rotDeg.x) > 1e-5f || std::fabs(rotDeg.y) > 1e-5f || std::fabs(rotDeg.z) > 1e-5f;
    if (!rotated && std::fabs(zOffset) < 1e-6f) return flat;
    glm::vec3 c = glm::vec3(flat * glm::vec4(0.0f, 0.0f, 0.0f, 1.0f));
    glm::mat4 r(1.0f);
    if (rotated) {
        r = glm::rotate(r, glm::radians(rotDeg.z), glm::vec3(0.0f, 0.0f, 1.0f));
        r = glm::rotate(r, glm::radians(rotDeg.y), glm::vec3(0.0f, 1.0f, 0.0f));
        r = glm::rotate(r, glm::radians(rotDeg.x), glm::vec3(1.0f, 0.0f, 0.0f));
    }
    glm::mat4 T  = glm::translate(glm::mat4(1.0f), c + glm::vec3(0.0f, 0.0f, zOffset));
    glm::mat4 Ti = glm::translate(glm::mat4(1.0f), -c);
    return T * r * Ti * flat;
}

glm::vec3 cardCenter(const glm::mat4& model) {
    return glm::vec3(model * glm::vec4(0.0f, 0.0f, 0.0f, 1.0f));
}

} // namespace scene

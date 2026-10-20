#pragma once
// Matrix builders for the native 3D scene. Pure math on SceneParams: no GL.
//
// World space: the zone's canvas lies on the z = 0 plane, centred at the
// origin, 2 units tall and 2*aspect units wide (x = ndc.x * aspect, y =
// ndc.y). Y is up, +z points at the default camera. A card at z = 0 with no
// 3D rotation and the orthographic default camera projects EXACTLY to its
// 2D normalized transform, which is what makes the 3D toggle lossless.
#include "scene/SceneParams.h"
#include <glm/glm.hpp>

namespace scene {

struct CameraMatrices {
    glm::mat4 view{1.0f};
    glm::mat4 proj{1.0f};
    glm::mat4 viewProj{1.0f};
    glm::vec3 eye{0.0f, 0.0f, 3.0f};
    glm::vec3 forward{0.0f, 0.0f, -1.0f};
    bool perspective = false;
};

struct LightMatrices {
    glm::vec3 dir{0.0f, 0.0f, 1.0f};   // direction TOWARD the light
    glm::mat4 view{1.0f};
    glm::mat4 proj{1.0f};
    glm::mat4 viewProj{1.0f};
};

// Direction from azimuth/elevation (degrees), Y-up, az 0 = +z (facing the default camera).
glm::vec3 directionFromAngles(float azimuthDeg, float elevationDeg);

CameraMatrices buildCamera(const SceneCameraParams& cam, float aspect);

// The sun's orthographic shadow frustum, sized to cover the canvas plus a
// margin so cards pushed off-canvas still cast.
LightMatrices buildLight(const SceneLightParams& light, float aspect, float sceneRadius = 0.0f);

// A card's model matrix: the layer's 2D NDC transform (3x3) promoted to
// world space (x scaled by aspect) and then rotated/offset in 3D about the
// card's own centre. rotDeg = Euler (x, y, z) in degrees, zOffset in world
// units (+ toward the default camera).
glm::mat4 cardModel(const glm::mat3& xform2D, float aspect, float zOffset, const glm::vec3& rotDeg);

// The card's centre in world space (for depth sorting).
glm::vec3 cardCenter(const glm::mat4& model);

} // namespace scene

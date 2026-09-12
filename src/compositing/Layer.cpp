#include "compositing/Layer.h"
#include <glm/gtc/matrix_transform.hpp>
#include <cmath>

glm::mat3 Layer::getTransformMatrix(float canvasAspect) const {
    float rad = glm::radians(rotation);
    float c = std::cos(rad);
    float s = std::sin(rad);

    // Anchor -> Flip -> Scale -> Rotate -> Translate
    // Anchor offsets the pivot: translate to anchor, scale/rotate, translate back + position
    glm::mat3 m(1.0f);

    // Scale (with flip applied)
    m[0][0] = scale.x * (flipH ? -1.0f : 1.0f);
    m[1][1] = scale.y * (flipV ? -1.0f : 1.0f);

    // Rotate. Normalized canvas space is ANISOTROPIC on a non-square
    // canvas (an x-unit spans `aspect` times the physical distance of a
    // y-unit), and a rotation applied there SHEARS — rotated content
    // visibly skews and loses its proportions. Conjugating the rotation
    // through the aspect (stretch to physically-square space, rotate,
    // stretch back) keeps the turn true at every angle, while position
    // and scale keep their normalized meaning exactly as before.
    glm::mat3 rot(1.0f);
    rot[0][0] = c;  rot[1][0] = -s;
    rot[0][1] = s;  rot[1][1] = c;
    if (canvasAspect > 0.0f && canvasAspect != 1.0f) {
        glm::mat3 A(1.0f), Ai(1.0f);
        A[0][0]  = canvasAspect;
        Ai[0][0] = 1.0f / canvasAspect;
        rot = Ai * rot * A;
    }
    m = rot * m;

    // Translate: position + anchor compensation
    // The anchor shifts the pivot point: we rotate/scale around (anchor), then place at position
    float ax = anchor.x, ay = anchor.y;
    // After scale+rotate, the anchor point moved to rot * scale * anchor.
    // We want it at position, so offset = position - (rot * scale * anchor) + anchor... no.
    // Actually: translate(-anchor) -> scale -> rotate -> translate(+anchor + position)
    // In the combined matrix, the translation becomes:
    //   tx = position.x + anchor.x - (m[0][0]*anchor.x + m[1][0]*anchor.y)
    //   ty = position.y + anchor.y - (m[0][1]*anchor.x + m[1][1]*anchor.y)
    m[2][0] = position.x + ax - (m[0][0]*ax + m[1][0]*ay);
    m[2][1] = position.y + ay - (m[0][1]*ax + m[1][1]*ay);

    return m;
}

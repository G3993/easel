#pragma once
#include "render/Framebuffer.h"
#include "render/ShaderProgram.h"
#include "render/Mesh.h"
#include "compositing/Layer.h"
#include <vector>
#include <memory>

class CompositeEngine {
public:
    bool init(int width, int height);
    void resize(int width, int height);

    // Composite a list of layers into the result FBO
    void composite(const std::vector<std::shared_ptr<Layer>>& layers);

    void setAudioState(float rms, float time) { m_audioRMS = rms; m_time = time; }
    float time() const { return m_time; }

    GLuint resultTexture() const;
    int width() const { return m_width; }
    int height() const { return m_height; }

private:
    float m_audioRMS = 0;
    float m_time = 0;
    Framebuffer m_fbo[2]; // ping-pong
    int m_current = 0;
    int m_width = 0, m_height = 0;

    ShaderProgram m_compositeShader;
    ShaderProgram m_passthroughShader;
    Mesh m_quad;

    void clear();
};

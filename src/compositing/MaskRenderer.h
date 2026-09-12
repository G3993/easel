#pragma once
#include "compositing/MaskPath.h"
#include "render/Texture.h"
#include "render/ShaderProgram.h"
#include <glad/glad.h>

class MaskRenderer {
public:
    ~MaskRenderer();

    bool init();

    // Render the mask path into the given texture.
    // Creates/resizes the texture as needed. Uses stencil buffer for fill.
    // feather > 0 softens the edge INTO the texture: a separable blur whose
    // radius is `feather` in UV (x divided by `aspect` so the falloff is
    // round on a non-square canvas). The edge then follows the shape.
    void render(const MaskPath& path, int width, int height, Texture& outTexture,
                float feather = 0.0f, float aspect = 1.0f);

private:
    ShaderProgram m_shader;
    ShaderProgram m_blurShader;
    bool   m_blurReady = false;
    Texture m_blurScratch;
    GLuint m_blurFbo = 0;
    void featherPass(Texture& tex, int width, int height, float feather, float aspect);
    GLuint m_fbo = 0;
    GLuint m_rbo = 0;     // depth-stencil renderbuffer
    GLuint m_vao = 0;
    GLuint m_vbo = 0;
    int m_fboWidth = 0, m_fboHeight = 0;

    void ensureFBO(int width, int height, GLuint colorTex);
    void destroy();
};

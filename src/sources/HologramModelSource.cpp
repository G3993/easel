#include "sources/HologramModelSource.h"

#include <glad/glad.h>
#include <GLFW/glfw3.h>
#include <glm/gtc/matrix_transform.hpp>
#include <iostream>

// ─── Embedded GLSL ─────────────────────────────────────────────────────────

// Mesh pass: world-space position to the fragment for flat-normal derivation.
static const char* kMeshVert = R"(#version 330 core
layout(location=0) in vec3 aPos;
layout(location=1) in vec2 aUV;
uniform mat4 uMVP;
uniform mat4 uModel;
out vec3 vWorld;
void main(){
    vWorld = (uModel * vec4(aPos, 1.0)).xyz;
    gl_Position = uMVP * vec4(aPos, 1.0);
}
)";

// Mesh pass fragment. uMode 0 = faint translucent surface (flat-normal fresnel
// rim, writes depth as the wire occluder); uMode 1 = bright wireframe line.
static const char* kMeshFrag = R"(#version 330 core
in vec3 vWorld;
out vec4 FragColor;
uniform vec3  uCamPos;
uniform int   uMode;        // 0 = surface fill, 1 = wireframe
uniform float uFill;        // surface fill amount
uniform float uWireBright;  // wire brightness
const vec3 PAL_PRIMARY = vec3(0.25, 0.85, 1.00);
const vec3 PAL_HIGH    = vec3(0.85, 0.97, 1.00);
void main(){
    if (uMode == 1) {
        FragColor = vec4(PAL_HIGH * uWireBright, 0.92);
        return;
    }
    vec3 n = normalize(cross(dFdx(vWorld), dFdy(vWorld)));
    vec3 V = normalize(uCamPos - vWorld);
    if (dot(n, V) < 0.0) n = -n;
    float fres = pow(1.0 - max(dot(n, V), 0.0), 2.0);
    float lit  = 0.4 + 0.6 * max(dot(n, normalize(vec3(0.3, 0.8, 0.5))), 0.0);
    vec3  col  = PAL_PRIMARY * (0.10 * lit) + PAL_PRIMARY * fres * 0.85;
    float a    = (0.10 + 0.55 * fres) * uFill;
    FragColor  = vec4(col, a);
}
)";

// Fullscreen glitch post — the screen-space degradation half of
// hologram_glitch.fs, applied to the pre-rendered mesh texture (uMesh).
static const char* kGlitchVert = R"(#version 330 core
layout(location=0) in vec2 aPos;
layout(location=1) in vec2 aUV;
out vec2 vUV;
void main(){ vUV = aUV; gl_Position = vec4(aPos, 0.0, 1.0); }
)";

static const char* kGlitchFrag = R"(#version 330 core
in vec2 vUV;
out vec4 FragColor;
uniform sampler2D uMesh;
uniform float iTime;
uniform vec2  iResolution;
uniform float scanSpeed, interference, chromaShift, beamHaze;
uniform float audioBass, audioMid, audioHigh, audioReact;

const vec3 PAL_PRIMARY = vec3(0.25, 0.85, 1.00);
const vec3 PAL_HIGH    = vec3(0.96, 0.98, 1.00);
const vec3 PAL_ERROR   = vec3(0.95, 0.20, 0.75);

float h11(float n){ return fract(sin(n * 12.9898) * 43758.5453); }
float h21(vec2 p){ return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }

vec3 interferenceBand(float ny, float t, float amount){
    float slab = floor(ny * 24.0) + floor(t * 12.0) * 47.0;
    float r1 = h11(slab), r2 = h11(slab + 7.7), r3 = h11(slab + 13.3);
    float trig = step(1.0 - 0.30 * amount, r1);
    return vec3((r2 - 0.5) * 0.18 * amount * trig,
                step(0.85, r2) * trig, step(0.92, r3) * trig);
}
vec4 samp(vec2 uv){
    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) return vec4(0.0);
    return texture(uMesh, uv);
}

void main(){
    vec2 uv = vUV;
    float t = iTime;
    float aR    = clamp(audioReact, 0.0, 2.0);
    float aBass = audioBass * aR, aMid = audioMid * aR, aHigh = audioHigh * aR;

    float scanS  = scanSpeed * (1.0 + 1.5 * aMid);
    float chrAmt = chromaShift * (1.0 + 2.0 * aHigh);
    float intAmt = clamp(interference + 0.4 * aBass, 0.0, 1.0);

    float dropPhase = fract(t * 0.31 + h11(floor(t * 0.31)) * 0.5);
    float dropTrig  = step(0.93, h11(floor(t * 4.0))) * step(0.55, aBass);
    float dropEnv   = exp(-dropPhase * 6.0) * dropTrig;
    dropEnv = max(dropEnv, step(0.997, h11(floor(t * 13.0))) * 0.7);

    float scanY  = fract(t * scanS * 0.35);
    float scanD  = uv.y - scanY;
    float scanFx = exp(-pow(scanD * 180.0, 2.0)) * 1.4
                 + exp(-max(-scanD, 0.0) * 9.0) * 0.18;

    vec3 band = interferenceBand(uv.y, t, intAmt);
    vec2 quake = vec2(h21(vec2(floor(t * 30.0), 0.0)) - 0.5,
                      h21(vec2(0.0, floor(t * 30.0))) - 0.5) * dropEnv * 0.04;
    vec2 sUV = uv + vec2(band.x, 0.0) + quake;

    vec2 caOff = vec2(chrAmt, 0.0);
    vec4 sR = samp(sUV + caOff), sG = samp(sUV), sB = samp(sUV - caOff);
    vec3 holo = vec3(sR.r, sG.g, sB.b);
    float alpha = max(sG.a, max(sR.a, sB.a));

    holo = mix(holo, PAL_HIGH - holo, band.y);
    if (band.z > 0.5) {
        float n = h21(uv * iResolution + floor(t * 24.0));
        holo  = mix(holo, mix(PAL_ERROR, PAL_HIGH, n), 0.85);
        alpha = mix(alpha, 0.9, 0.7);
    }
    if (dropEnv > 0.01) {
        float n = h21(uv * iResolution * 0.5 + floor(t * 30.0));
        vec3 e = mix(vec3(0.0), PAL_ERROR, step(0.6, n));
        e = mix(e, PAL_HIGH, step(0.93, n));
        holo  = mix(holo, e, dropEnv * 0.92);
        alpha = mix(alpha, n, dropEnv * 0.6);
    }

    holo += PAL_PRIMARY * scanFx * (alpha * 0.6 + 0.15);

    vec2 ndc = uv * 2.0 - 1.0; ndc.x *= iResolution.x / iResolution.y;
    float radial = length(ndc.xy * vec2(0.9, 0.55));
    float beam   = exp(-radial * 2.4) * (0.35 + 0.65 * smoothstep(-1.0, 0.4, ndc.y));
    float dust = 0.0;
    for (int i = 0; i < 5; i++) {
        float fi = float(i);
        vec2  c  = vec2(0.5 + 0.42 * (h11(fi * 3.7) - 0.5),
                        fract(h11(fi * 1.3) - t * (0.05 + 0.04 * fi)));
        float r  = 0.004 + 0.010 * h11(fi * 9.1);
        float dm = length((uv - c) * vec2(iResolution.x / iResolution.y, 1.0));
        dust += smoothstep(r, 0.0, dm) * (0.3 + 0.5 * h11(fi * 7.7));
    }
    vec3 hazeCol = PAL_PRIMARY * beam * beamHaze * 0.18 + PAL_HIGH * dust * 0.45;

    vec3 col = mix(vec3(0.0), holo, clamp(alpha, 0.0, 1.0)) + hazeCol;
    float floorGleam = smoothstep(0.0, 0.08, uv.y) * (1.0 - smoothstep(0.0, 0.18, uv.y));
    col += PAL_PRIMARY * floorGleam * 0.12 * (alpha + 0.3);

    col *= 0.92 + 0.08 * sin(uv.y * iResolution.y * 1.7);
    col *= 0.90 + 0.10 * sin(t * 1.3) + 0.05 * sin(t * 7.1);
    float sparkle = step(0.997 - aHigh * 0.012, h21(uv * iResolution + t));
    col += PAL_HIGH * sparkle * 0.6 * (0.3 + aHigh);

    float lum = dot(col, vec3(0.299, 0.587, 0.114));
    col += PAL_PRIMARY * pow(max(lum - 0.6, 0.0), 1.6) * 0.4;

    // Coverage alpha: the model (alpha) is opaque; haze/scan add only a faint
    // transparent glow so the layer composits cleanly over what's below
    // instead of washing it with a full-frame haze.
    float hazeCov = clamp(beam * beamHaze * 0.5 + dust * 0.5, 0.0, 1.0);
    float outA = clamp(alpha + hazeCov * 0.45 + scanFx * 0.12, 0.0, 1.0);
    FragColor = vec4(col, outA);
}
)";

// ─── HologramModelSource ────────────────────────────────────────────────────

bool HologramModelSource::init(int resolutionW, int resolutionH) {
    m_w = resolutionW;
    m_h = resolutionH;

    if (!m_meshFBO.create(m_w, m_h, /*withDepth=*/true) ||
        !m_output.create(m_w, m_h, /*withDepth=*/false)) {
        std::cerr << "[HologramModel] FBO create failed" << std::endl;
        return false;
    }
    m_quad.createQuad();

    if (!m_meshShader.loadFromSource(kMeshVert, kMeshFrag)) {
        std::cerr << "[HologramModel] mesh shader compile failed" << std::endl;
        return false;
    }
    if (!m_glitchShader.loadFromSource(kGlitchVert, kGlitchFrag)) {
        std::cerr << "[HologramModel] glitch shader compile failed" << std::endl;
        return false;
    }

    m_startTime = glfwGetTime();
    m_ready = true;
    return true;
}

bool HologramModelSource::loadModel(const std::string& path) {
    if (path.empty()) return false;
    if (!m_model.loadModel(path)) {
        std::cerr << "[HologramModel] failed to load " << path << std::endl;
        return false;
    }
    m_modelPath = path;
    // ObjMeshWarp::loadModel parks the orbit camera at the bbox:
    //   target = bbox center, distance = bbox extent * 1.5 — recover both to
    //   normalize the model to a unit sphere at the origin.
    m_center = m_model.camera().target;
    m_extent = m_model.camera().distance / 1.5f;
    if (m_extent < 1e-4f) m_extent = 1.0f;
    m_hasModel = true;
    std::cout << "[HologramModel] loaded " << path << std::endl;
    return true;
}

void HologramModelSource::renderMesh(float t, float aspect) {
    m_meshFBO.bind();
    glViewport(0, 0, m_w, m_h);
    glClearColor(0.0f, 0.0f, 0.0f, 0.0f);
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

    if (!m_hasModel) { Framebuffer::unbind(); return; }

    const HologramModelParams& P = m_params;

    // Model: center → unit-scale (× user scale) → spin around Y.
    glm::mat4 toOrigin = glm::translate(glm::mat4(1.0f), -m_center);
    float s = (P.modelScale > 1e-3f ? P.modelScale : 1.0f) / m_extent;
    glm::mat4 unit = glm::scale(glm::mat4(1.0f), glm::vec3(s));
    float yaw  = t * P.rotateSpeed * 1.4f;
    glm::mat4 spin = glm::rotate(glm::mat4(1.0f), yaw, glm::vec3(0, 1, 0));
    spin = glm::rotate(spin, 0.10f * sinf(t * 0.6f), glm::vec3(1, 0, 0)); // gentle bob
    glm::mat4 model = spin * unit * toOrigin;

    glm::vec3 eye(0.0f, 0.12f, 2.4f);
    glm::mat4 view = glm::lookAt(eye, glm::vec3(0.0f), glm::vec3(0, 1, 0));
    glm::mat4 proj = glm::perspective(glm::radians(42.0f), aspect, 0.01f, 100.0f);
    glm::mat4 mvp  = proj * view * model;

    m_meshShader.use();
    m_meshShader.setMat4("uMVP", mvp);
    m_meshShader.setMat4("uModel", model);
    m_meshShader.setVec3("uCamPos", eye);
    m_meshShader.setFloat("uWireBright", P.wireBright);

    glDisable(GL_CULL_FACE);   // model winding unknown; normal-flip handles it

    // Pass 1a — faint translucent surface, writes depth (wire occluder).
    glEnable(GL_DEPTH_TEST);
    glDepthMask(GL_TRUE);
    glEnable(GL_BLEND);
    glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
    m_meshShader.setInt("uMode", 0);
    m_meshShader.setFloat("uFill", P.surfaceFill);
    for (auto& mg : m_model.materials())
        if (mg.mesh.isLoaded()) mg.mesh.draw();

    // Pass 1b — bright wireframe over the surface; only front wires (depth
    // LEQUAL vs the surface), additive so they glow.
    glPolygonMode(GL_FRONT_AND_BACK, GL_LINE);
    glDepthFunc(GL_LEQUAL);
    glDepthMask(GL_FALSE);
    glBlendFunc(GL_ONE, GL_ONE);
    m_meshShader.setInt("uMode", 1);
    for (auto& mg : m_model.materials())
        if (mg.mesh.isLoaded()) mg.mesh.draw();

    // Restore polygon mode / depth func for the rest of the app.
    glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
    glDepthFunc(GL_LESS);
    glDepthMask(GL_TRUE);

    Framebuffer::unbind();
}

// Park the GPU render targets while this source is reachable only from
// undo/redo snapshots. Called from UndoStack::suspendOrphanedSources on the
// main-thread frame sweep — the GL context is current there, so the deletes
// are safe. Frees the mesh pass (color+depth, ~7 MB at 720p) and the output
// (~3.7 MB). Shaders, the quad and the loaded model mesh stay resident —
// re-reading the model from disk on revive would cost far more than the
// VRAM its VBOs hold.
void HologramModelSource::suspend() {
    if (!m_ready) return;
    m_meshFBO.destroy();
    m_output.destroy();
    m_ready = false;
    m_suspended = true;
}

void HologramModelSource::update() {
    if (!m_ready) {
        // Lazy revive after suspend(): recreate the FBOs at the configured
        // resolution (fixed-res source — m_w/m_h are exactly what a fresh
        // init() would use, so nothing can be stale).
        if (!m_suspended) return;
        m_suspended = false;   // one attempt — no retry storm on failure
        if (!m_meshFBO.create(m_w, m_h, /*withDepth=*/true) ||
            !m_output.create(m_w, m_h, /*withDepth=*/false)) return;
        m_ready = true;
    }

    float t = static_cast<float>(glfwGetTime() - m_startTime);
    float aspect = (m_h > 0) ? (float)m_w / (float)m_h : 1.777f;

    // Save GL state (we run inside Easel's compositor loop).
    GLint prevVp[4];
    glGetIntegerv(GL_VIEWPORT, prevVp);
    GLboolean prevDepth = glIsEnabled(GL_DEPTH_TEST);
    GLboolean prevCull  = glIsEnabled(GL_CULL_FACE);
    GLboolean prevBlend = glIsEnabled(GL_BLEND);

    // ── Pass 1: mesh wireframe → m_meshFBO ──
    renderMesh(t, aspect);

    // ── Pass 2: fullscreen glitch → m_output ──
    m_output.bind();
    glViewport(0, 0, m_w, m_h);
    glClearColor(0.0f, 0.0f, 0.0f, 0.0f);
    glClear(GL_COLOR_BUFFER_BIT);
    glDisable(GL_DEPTH_TEST);
    glDisable(GL_BLEND);

    m_glitchShader.use();
    glActiveTexture(GL_TEXTURE0);
    glBindTexture(GL_TEXTURE_2D, m_meshFBO.textureId());
    m_glitchShader.setInt("uMesh", 0);
    m_glitchShader.setFloat("iTime", t);
    m_glitchShader.setVec2("iResolution", glm::vec2((float)m_w, (float)m_h));
    m_glitchShader.setFloat("scanSpeed", m_params.scanSpeed);
    m_glitchShader.setFloat("interference", m_params.interference);
    m_glitchShader.setFloat("chromaShift", m_params.chromaShift);
    m_glitchShader.setFloat("beamHaze", m_params.beamHaze);
    m_glitchShader.setFloat("audioBass", m_audioBass);
    m_glitchShader.setFloat("audioMid", m_audioMid);
    m_glitchShader.setFloat("audioHigh", m_audioHigh);
    m_glitchShader.setFloat("audioReact", m_params.audioReact);
    m_quad.draw();

    Framebuffer::unbind();

    // Restore compositor state.
    glViewport(prevVp[0], prevVp[1], prevVp[2], prevVp[3]);
    if (prevDepth) glEnable(GL_DEPTH_TEST); else glDisable(GL_DEPTH_TEST);
    if (prevCull)  glEnable(GL_CULL_FACE);  else glDisable(GL_CULL_FACE);
    if (prevBlend) glEnable(GL_BLEND);      else glDisable(GL_BLEND);
    glActiveTexture(GL_TEXTURE0);
}

#include "sources/ModelSource.h"
#include "app/MIDIManager.h"
#include <glm/gtc/matrix_transform.hpp>
#include <GLFW/glfw3.h>
#include <iostream>
#include <cmath>

bool ModelSource::init(int previewSize) {
    m_size = previewSize > 64 ? previewSize : 64;
    if (!m_output.create(m_size, m_size, /*withDepth=*/true)) {
        std::cerr << "[Model] preview FBO failed" << std::endl;
        return false;
    }
    m_shadersOk = m_meshShader.loadFromFiles("shaders/scene_mesh.vert", "shaders/scene_mesh.frag")
               && m_meshShadowShader.loadFromFiles("shaders/scene_mesh_shadow.vert", "shaders/scene_mesh_shadow.frag");
    if (!m_shadersOk) std::cerr << "[Model] shader load failed: " << m_meshShader.lastError() << std::endl;
    m_lastTime = glfwGetTime();
    return true;
}

bool ModelSource::loadModel(const std::string& path) {
    if (path.empty()) return false;
    if (!m_model.loadModel(path)) {
        std::cerr << "[Model] failed to load " << path << std::endl;
        return false;
    }
    m_modelPath = path;
    // ObjMeshWarp::loadModel parks its orbit camera at the bbox: target =
    // centre, distance = extent * 1.5 — recover both for the unit box.
    m_center = m_model.camera().target;
    m_extent = m_model.camera().distance / 1.5f;
    if (m_extent < 1e-4f) m_extent = 1.0f;
    m_hasModel = true;
    std::cout << "[Model] loaded " << path << std::endl;
    return true;
}

glm::mat4 ModelSource::normalizeMatrix() const {
    glm::mat4 toOrigin = glm::translate(glm::mat4(1.0f), -m_center);
    glm::mat4 unit = glm::scale(glm::mat4(1.0f), glm::vec3(1.0f / m_extent));
    return unit * toOrigin;
}

glm::mat4 ModelSource::spinMatrix() const {
    glm::mat4 m = glm::rotate(glm::mat4(1.0f), glm::radians(m_spinAngle), glm::vec3(0.0f, 1.0f, 0.0f));
    if (m_bob > 0.0f) {
        m = glm::translate(glm::mat4(1.0f), glm::vec3(0.0f, 0.12f * m_bob * std::sin(m_bobPhase), 0.0f)) * m;
        m = glm::rotate(m, 0.08f * m_bob * std::sin(m_bobPhase * 0.7f), glm::vec3(1.0f, 0.0f, 0.0f));
    }
    return m;
}

void ModelSource::update() {
    double now = glfwGetTime();
    float dt = (float)(now - m_lastTime);
    if (dt < 0.0f || dt > 0.25f) dt = 1.0f / 60.0f;
    m_lastTime = now;
    m_spinAngle = std::fmod(m_spinAngle + m_spin * dt, 360.0f);
    m_bobPhase += dt * 1.3f;
    if (m_suspended) {
        m_output.create(m_size, m_size, true);
        m_suspended = false;
    }
    renderPreview();
}

void ModelSource::suspend() {
    m_output.destroy();
    m_suspended = true;
}

void ModelSource::renderPreview() {
    if (!m_shadersOk) return;
    GLint prevFbo = 0, prevVp[4];
    glGetIntegerv(GL_FRAMEBUFFER_BINDING, &prevFbo);
    glGetIntegerv(GL_VIEWPORT, prevVp);
    GLboolean prevBlend = glIsEnabled(GL_BLEND), prevDepth = glIsEnabled(GL_DEPTH_TEST);

    m_output.bind();
    glClearColor(0.0f, 0.0f, 0.0f, 0.0f);
    glClearDepth(1.0);
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
    if (m_hasModel) {
        glm::mat4 model = spinMatrix() * normalizeMatrix();
        glm::vec3 eye(0.0f, 0.25f, 3.2f);
        glm::mat4 view = glm::lookAt(eye, glm::vec3(0.0f), glm::vec3(0.0f, 1.0f, 0.0f));
        glm::mat4 proj = glm::perspective(glm::radians(40.0f), 1.0f, 0.05f, 100.0f);
        m_meshShader.use();
        m_meshShader.setMat4("uModel", model);
        m_meshShader.setMat4("uViewProj", proj * view);
        m_meshShader.setMat4("uLightVP", glm::mat4(1.0f));
        m_meshShader.setVec3("uCamPos", eye);
        m_meshShader.setVec3("uColor", m_color);
        m_meshShader.setFloat("uOpacity", 1.0f);
        m_meshShader.setFloat("uGloss", m_gloss);
        m_meshShader.setFloat("uMetal", m_metal);
        m_meshShader.setFloat("uEmissive", m_emissive);
        m_meshShader.setFloat("uWire", 0.0f);
        m_meshShader.setBool("uLightOn", false);   // built-in header light
        m_meshShader.setBool("uShadowOn", false);
        m_meshShader.setInt("uPointCount", 0);
        glEnable(GL_DEPTH_TEST);
        glDepthFunc(GL_LEQUAL);
        glDepthMask(GL_TRUE);
        glDisable(GL_CULL_FACE);
        glDisable(GL_BLEND);
        for (auto& mg : m_model.materials())
            if (mg.mesh.isLoaded()) mg.mesh.draw();
        if (m_wire > 0.001f) {
            glPolygonMode(GL_FRONT_AND_BACK, GL_LINE);
            glEnable(GL_BLEND);
            glBlendFunc(GL_ONE, GL_ONE);
            glDepthMask(GL_FALSE);
            m_meshShader.setFloat("uWire", m_wire);
            for (auto& mg : m_model.materials())
                if (mg.mesh.isLoaded()) mg.mesh.draw();
            glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
            glDepthMask(GL_TRUE);
            glDisable(GL_BLEND);
        }
        glDisable(GL_DEPTH_TEST);
    }
    glBindFramebuffer(GL_FRAMEBUFFER, prevFbo);
    glViewport(prevVp[0], prevVp[1], prevVp[2], prevVp[3]);
    if (prevBlend) glEnable(GL_BLEND);
    if (prevDepth) glEnable(GL_DEPTH_TEST);
}

void ModelSource::applyAudioBindings(float level, float bass, float mid, float high, float beat, float dt,
                                     MIDIManager* midi, float energy, float build, float drop,
                                     float silence, float momentum) {
    if (m_audioBindings.empty()) return;
    auto table = params();
    for (auto& [name, b] : m_audioBindings) {
        if (b.signal == AudioSignal::None) continue;
        float raw = 0.0f; bool have = true;
        switch (b.signal) {
            case AudioSignal::Level: raw = level; break;
            case AudioSignal::Bass:  raw = bass;  break;
            case AudioSignal::Mid:   raw = mid;   break;
            case AudioSignal::High:  raw = high;  break;
            case AudioSignal::Beat:  raw = beat;  break;
            case AudioSignal::Energy:   raw = energy;   break;
            case AudioSignal::Build:    raw = build;    break;
            case AudioSignal::Drop:     raw = drop;     break;
            case AudioSignal::Silence:  raw = silence;  break;
            case AudioSignal::Momentum: raw = momentum; break;
            case AudioSignal::MidiCC: {
                if (midi && b.midiCC >= 0) {
                    float v = midi->getCCValue(b.midiChannel, b.midiCC);
                    if (v < 0.0f) have = false; else raw = v;
                } else have = false;
                break;
            }
            default: have = false; break;
        }
        if (!have) continue;
        float mapped = b.follow(raw, dt);
        for (const auto& d : table) {
            if (name != d.id) continue;
            if (mapped < d.lo) mapped = d.lo;
            if (mapped > d.hi) mapped = d.hi;
            *d.ptr = mapped;
        }
    }
}

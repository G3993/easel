#include "compositing/CompositeEngine.h"
#include "sources/ShaderSource.h"
#include "render/GLTransition.h"
#include <glm/glm.hpp>
#include <iostream>
#include <set>
#include <string>
#include <unordered_set>

// Render a gl-transitions.com shader blending source A → nextSource B at
// layer->transitionProgress into the provided scratch FBO. Returns the FBO
// texture on success, 0 on failure (caller falls back to source A).
static GLuint renderLayerGLTransition(const std::shared_ptr<Layer>& layer,
                                      Framebuffer& scratch,
                                      const AudioState& audio) {
    if (!layer->source || !layer->nextSource)      return 0;
    if (layer->glTransitionName.empty())           return 0;

    auto* xition = GLTransitionLibrary::instance().get(layer->glTransitionName);
    if (!xition || !xition->isValid()) return 0;

    GLuint texA = layer->source->textureId();
    GLuint texB = layer->nextSource->textureId();
    if (!texA || !texB) return 0;

    int wA = layer->source->width(),  hA = layer->source->height();
    int wB = layer->nextSource->width(), hB = layer->nextSource->height();
    int w = std::max(wA, wB), h = std::max(hA, hB);
    if (w <= 0 || h <= 0) return 0;

    // Ensure the scratch FBO matches the transition's target size so we don't
    // oversample or waste fill rate.
    if (scratch.width() != w || scratch.height() != h) {
        if (scratch.width() == 0) scratch.create(w, h);
        else                      scratch.resize(w, h);
    }

    // Save current viewport — caller has a composite viewport set; we'll
    // restore after the off-screen render.
    GLint viewport[4];
    glGetIntegerv(GL_VIEWPORT, viewport);

    scratch.bind();
    glViewport(0, 0, w, h);
    glClearColor(0, 0, 0, 0);
    glClear(GL_COLOR_BUFFER_BIT);

    bool audioOn = layer->audioReactive;
    xition->render(texA, texB, layer->transitionProgress, w, h,
                   audioOn ? audio.rms : 0.0f,
                   audioOn ? audio.bass : 0.0f,
                   audioOn ? (audio.lowMid + audio.highMid) * 0.5f : 0.0f,
                   audioOn ? audio.treble : 0.0f,
                   audioOn ? audio.beatDecay : 0.0f);

    Framebuffer::unbind();
    glViewport(viewport[0], viewport[1], viewport[2], viewport[3]);
    return scratch.textureId();
}

// Render a shader-based A→B transition. Uses layer->transitionShaderInst
// (lazy-loaded from transitionShaderPath) with two ISF image inputs "from"/"to"
// and a float input "progress". Returns the shader's output texture, or 0 on
// failure (caller falls back to source A).
static GLuint renderLayerShaderTransition(const std::shared_ptr<Layer>& layer) {
    if (!layer->source || !layer->nextSource) return 0;
    if (layer->transitionShaderPath.empty()) return 0;

    // Lazy load / reload on path change
    if (!layer->transitionShaderInst ||
        layer->transitionShaderInst->sourcePath() != layer->transitionShaderPath) {
        auto inst = std::make_shared<ShaderSource>();
        if (!inst->loadFromFile(layer->transitionShaderPath)) {
            std::cerr << "[Transition] Failed to load shader: " << layer->transitionShaderPath << std::endl;
            return 0;
        }
        layer->transitionShaderInst = inst;
    }
    auto& shader = layer->transitionShaderInst;

    GLuint texA = layer->source->textureId();
    GLuint texB = layer->nextSource->textureId();
    if (!texA || !texB) return 0;

    int wA = layer->source->width(),  hA = layer->source->height();
    int wB = layer->nextSource->width(), hB = layer->nextSource->height();
    int w = std::max(wA, wB), h = std::max(hA, hB);
    if (w <= 0 || h <= 0) return 0;
    shader->setResolution(w, h);

    shader->bindImageInput("from", texA, wA, hA, 0, layer->source->isFlippedV());
    shader->bindImageInput("to",   texB, wB, hB, 0, layer->nextSource->isFlippedV());
    shader->setFloat("progress", layer->transitionProgress);

    shader->update();
    return shader->textureId();
}

bool CompositeEngine::init(int width, int height) {
    m_width = width;
    m_height = height;

    // Phase Q: main composite ping-pong is RGBA16F so layer→layer→transition
    // chain keeps headroom and doesn't band in dark gradients. Final readback
    // is still 8-bit (recorder + NDI expect that on output).
    if (!m_fbo[0].createHalfFloat(width, height) || !m_fbo[1].createHalfFloat(width, height)) {
        std::cerr << "Failed to create composite FBOs" << std::endl;
        return false;
    }

    if (!m_compositeShader.loadFromFiles("shaders/passthrough.vert", "shaders/composite.frag")) {
        std::cerr << "Failed to load composite shader" << std::endl;
        return false;
    }

    if (!m_passthroughShader.loadFromFiles("shaders/passthrough.vert", "shaders/passthrough.frag")) {
        std::cerr << "Failed to load passthrough shader" << std::endl;
        return false;
    }

    if (!m_effectShader.loadFromFiles("shaders/passthrough.vert", "shaders/effect.frag")) {
        std::cerr << "Failed to load effect shader" << std::endl;
        return false;
    }

    if (!m_shadowShader.loadFromFiles("shaders/passthrough.vert", "shaders/shadow.frag")) {
        std::cerr << "Failed to load shadow shader" << std::endl;
        return false;
    }

    m_quad.createQuad();
    return true;
}

void CompositeEngine::resize(int width, int height) {
    if (width == m_width && height == m_height) return;
    m_width = width;
    m_height = height;
    m_fbo[0].resize(width, height);
    m_fbo[1].resize(width, height);
}

void CompositeEngine::clear() {
    for (int i = 0; i < 2; i++) {
        m_fbo[i].bind();
        glClearColor(0, 0, 0, 0);
        glClear(GL_COLOR_BUFFER_BIT);
    }
    Framebuffer::unbind();
    m_current = 0;
}

void CompositeEngine::setAudioUniforms(ShaderProgram& shader, float audioStrength) {
    shader.setFloat("uAudioRMS", m_audio.rms);
    shader.setFloat("uAudioStrength", audioStrength);
    shader.setFloat("uAudioBass", audioStrength > 0 ? m_audio.bass : 0.0f);
    shader.setFloat("uAudioLowMid", audioStrength > 0 ? m_audio.lowMid : 0.0f);
    shader.setFloat("uAudioHighMid", audioStrength > 0 ? m_audio.highMid : 0.0f);
    shader.setFloat("uAudioTreble", audioStrength > 0 ? m_audio.treble : 0.0f);
    shader.setFloat("uAudioBeatDecay", audioStrength > 0 ? m_audio.beatDecay : 0.0f);
    // BPM sync (always available, independent of audio reactivity toggle)
    shader.setFloat("uBeatPhase", m_audio.beatPhase);
    shader.setFloat("uBeatPulse", m_audio.beatPulse);
    shader.setFloat("uBarPhase", m_audio.barPhase);
    shader.setFloat("uBPM", m_audio.bpm);
}

void CompositeEngine::releaseIdleScratch() {
    // ~10s at 60fps. A scratch set whose feature (effects, multi-mask,
    // drop shadow, transitions) stopped being used is returned to the
    // driver — these are full-canvas (often supersampled, 16F) buffers
    // that otherwise stay resident until the zone is deleted.
    constexpr uint64_t kIdleFrames = 600;
    auto idle = [&](int g) {
        return m_scratchUsed[g] != 0 && m_frame - m_scratchUsed[g] > kIdleFrames;
    };
    if (idle(0)) {
        m_effectFBO[0].destroy();
        m_effectFBO[1].destroy();
        m_scratchUsed[0] = 0;
    }
    if (idle(1)) {
        m_maskFBO[0].destroy();
        m_maskFBO[1].destroy();
        m_scratchUsed[1] = 0;
    }
    if (idle(2)) {
        m_shadowFBO[0].destroy();
        m_shadowFBO[1].destroy();
        m_shadowLoRes[0].destroy();
        m_shadowLoRes[1].destroy();
        m_scratchUsed[2] = 0;
    }
    if (idle(3)) {
        m_glTransitionFBO.destroy();
        m_scratchUsed[3] = 0;
    }
    if (idle(4)) {
        m_betweenRowFBO.destroy();
        m_scratchUsed[4] = 0;
    }
}

GLuint CompositeEngine::applyEffects(const std::shared_ptr<Layer>& layer, GLuint srcTex) {
    if (layer->effects.empty()) return srcTex;

    // Check if any effect is enabled
    bool anyEnabled = false;
    for (auto& fx : layer->effects) {
        if (fx.enabled) { anyEnabled = true; break; }
    }
    if (!anyEnabled) return srcTex;

    // Ensure effect FBOs match size
    for (int i = 0; i < 2; i++) {
        if (m_effectFBO[i].width() != m_width || m_effectFBO[i].height() != m_height) {
            // Phase Q: effect ping-pong (blur, glow, etc.) is 16F to match
            // composite chain — multi-tap blurs especially benefit.
            m_effectFBO[i].createHalfFloat(m_width, m_height);
        }
    }
    m_scratchUsed[0] = m_frame;

    GLuint currentTex = srcTex;
    int pingpong = 0;

    // Per-effect mic modulation: an effect's primary param pulses with the
    // chosen audio band (modulated = base + audioAmount * band). Same m_audio
    // the layer audioBindings read, so it follows the active mic/loopback.
    auto audioBand = [&](int s) -> float {
        switch (s) {
            case 0: return m_audio.bass;
            case 1: return (m_audio.lowMid + m_audio.highMid) * 0.5f;
            case 2: return m_audio.treble;
            case 3: return m_audio.beatDecay;
            default: return 0.0f;
        }
    };
    auto fxAudioMod = [&](const LayerEffect& f) -> float {
        if (f.audioSignal < 0 || f.audioAmount <= 0.0f) return 0.0f;
        return f.audioAmount * audioBand(f.audioSignal);
    };

    for (auto& fx : layer->effects) {
        if (!fx.enabled) continue;

        if (fx.type == EffectType::Blur && fx.blurRadius > 0.1f) {
            // Two-pass separable blur
            for (int pass = 0; pass < 2; pass++) {
                m_effectFBO[pingpong].bind();
                glViewport(0, 0, m_width, m_height);
                glClear(GL_COLOR_BUFFER_BIT);
                m_effectShader.use();
                m_effectShader.setInt("uEffectType", 0);
                m_effectShader.setInt("uBlurPass", pass);
                m_effectShader.setFloat("uBlurRadius", fx.blurRadius);
                m_effectShader.setVec2("uResolution", glm::vec2(m_width, m_height));
                m_effectShader.setInt("uTexture", 0);
                m_effectShader.setMat3("uTransform", glm::mat3(1.0f));
                glActiveTexture(GL_TEXTURE0);
                glBindTexture(GL_TEXTURE_2D, currentTex);
                m_quad.draw();
                currentTex = m_effectFBO[pingpong].textureId();
                pingpong = 1 - pingpong;
            }
        } else if (fx.type == EffectType::ColorAdjust) {
            m_effectFBO[pingpong].bind();
            glViewport(0, 0, m_width, m_height);
            glClear(GL_COLOR_BUFFER_BIT);
            m_effectShader.use();
            m_effectShader.setInt("uEffectType", 1);
            m_effectShader.setFloat("uBrightness", fx.brightness);
            m_effectShader.setFloat("uContrast", fx.contrast);
            m_effectShader.setFloat("uSaturation", fx.saturation);
            m_effectShader.setFloat("uHueShift", fx.hueShift);
            m_effectShader.setInt("uTexture", 0);
            m_effectShader.setMat3("uTransform", glm::mat3(1.0f));
            glActiveTexture(GL_TEXTURE0);
            glBindTexture(GL_TEXTURE_2D, currentTex);
            m_quad.draw();
            currentTex = m_effectFBO[pingpong].textureId();
            pingpong = 1 - pingpong;
        } else if (fx.type == EffectType::Invert) {
            m_effectFBO[pingpong].bind();
            glViewport(0, 0, m_width, m_height);
            glClear(GL_COLOR_BUFFER_BIT);
            m_effectShader.use();
            m_effectShader.setInt("uEffectType", 2);
            m_effectShader.setInt("uTexture", 0);
            m_effectShader.setMat3("uTransform", glm::mat3(1.0f));
            glActiveTexture(GL_TEXTURE0);
            glBindTexture(GL_TEXTURE_2D, currentTex);
            m_quad.draw();
            currentTex = m_effectFBO[pingpong].textureId();
            pingpong = 1 - pingpong;
        } else if (fx.type == EffectType::Pixelate && fx.pixelSize > 1.0f) {
            m_effectFBO[pingpong].bind();
            glViewport(0, 0, m_width, m_height);
            glClear(GL_COLOR_BUFFER_BIT);
            m_effectShader.use();
            m_effectShader.setInt("uEffectType", 3);
            m_effectShader.setFloat("uPixelSize", fx.pixelSize);
            m_effectShader.setVec2("uResolution", glm::vec2(m_width, m_height));
            m_effectShader.setInt("uTexture", 0);
            m_effectShader.setMat3("uTransform", glm::mat3(1.0f));
            glActiveTexture(GL_TEXTURE0);
            glBindTexture(GL_TEXTURE_2D, currentTex);
            m_quad.draw();
            currentTex = m_effectFBO[pingpong].textureId();
            pingpong = 1 - pingpong;
        } else if (fx.type == EffectType::Feedback) {
            // Get or create feedback FBO for this layer
            auto& fbFBO = m_feedbackFBOs[layer->id];
            if (fbFBO.width() != m_width || fbFBO.height() != m_height) {
                fbFBO.create(m_width, m_height);
            }
            m_effectFBO[pingpong].bind();
            glViewport(0, 0, m_width, m_height);
            glClear(GL_COLOR_BUFFER_BIT);
            m_effectShader.use();
            m_effectShader.setInt("uEffectType", 4);
            m_effectShader.setFloat("uFeedbackMix", fx.feedbackMix);
            m_effectShader.setFloat("uFeedbackZoom", fx.feedbackZoom);
            m_effectShader.setInt("uTexture", 0);
            m_effectShader.setInt("uFeedback", 1);
            m_effectShader.setMat3("uTransform", glm::mat3(1.0f));
            glActiveTexture(GL_TEXTURE0);
            glBindTexture(GL_TEXTURE_2D, currentTex);
            glActiveTexture(GL_TEXTURE1);
            glBindTexture(GL_TEXTURE_2D, fbFBO.textureId());
            m_quad.draw();
            currentTex = m_effectFBO[pingpong].textureId();
            pingpong = 1 - pingpong;

            // Copy result to feedback FBO for next frame
            fbFBO.bind();
            glViewport(0, 0, m_width, m_height);
            m_passthroughShader.use();
            m_passthroughShader.setInt("uTexture", 0);
            m_passthroughShader.setFloat("uOpacity", 1.0f);
            m_passthroughShader.setMat3("uTransform", glm::mat3(1.0f));
            m_passthroughShader.setFloat("uTileX", 1.0f);
            m_passthroughShader.setFloat("uTileY", 1.0f);
            m_passthroughShader.setInt("uMosaicMode", 0);
            m_passthroughShader.setFloat("uFeather", 0.0f);
            m_passthroughShader.setBool("uHasMask", false);
            m_passthroughShader.setBool("uFlipV", false);
            glActiveTexture(GL_TEXTURE0);
            glBindTexture(GL_TEXTURE_2D, currentTex);
            m_quad.draw();
        } else if (fx.type == EffectType::Glow && fx.glowIntensity > 0.01f) {
            // Glow: threshold → blur → additive combine
            GLuint originalTex = currentTex;

            // Step 1: Extract bright areas (threshold)
            m_effectFBO[pingpong].bind();
            glViewport(0, 0, m_width, m_height);
            glClear(GL_COLOR_BUFFER_BIT);
            m_effectShader.use();
            m_effectShader.setInt("uEffectType", 5); // threshold pass
            m_effectShader.setFloat("uGlowThreshold", fx.glowThreshold);
            m_effectShader.setInt("uTexture", 0);
            m_effectShader.setMat3("uTransform", glm::mat3(1.0f));
            glActiveTexture(GL_TEXTURE0);
            glBindTexture(GL_TEXTURE_2D, currentTex);
            m_quad.draw();
            GLuint threshTex = m_effectFBO[pingpong].textureId();
            pingpong = 1 - pingpong;

            // Step 2: Blur the thresholded texture (multiple passes for soft glow)
            float passR = 2.0f;
            int iterations = std::max(1, (int)std::ceil(fx.glowRadius / passR));
            iterations = std::min(iterations, 12);
            GLuint blurSrc = threshTex;
            for (int it = 0; it < iterations; it++) {
                for (int dir = 0; dir < 2; dir++) {
                    m_effectFBO[pingpong].bind();
                    glClear(GL_COLOR_BUFFER_BIT);
                    m_effectShader.setInt("uEffectType", 0); // blur
                    m_effectShader.setInt("uBlurPass", dir);
                    m_effectShader.setFloat("uBlurRadius", passR);
                    m_effectShader.setVec2("uResolution", glm::vec2(m_width, m_height));
                    m_effectShader.setInt("uTexture", 0);
                    m_effectShader.setMat3("uTransform", glm::mat3(1.0f));
                    glActiveTexture(GL_TEXTURE0);
                    glBindTexture(GL_TEXTURE_2D, blurSrc);
                    m_quad.draw();
                    blurSrc = m_effectFBO[pingpong].textureId();
                    pingpong = 1 - pingpong;
                }
            }

            // Step 3: Additive combine original + blurred glow
            m_effectFBO[pingpong].bind();
            glClear(GL_COLOR_BUFFER_BIT);
            m_effectShader.setInt("uEffectType", 6); // glow combine
            m_effectShader.setFloat("uGlowIntensity", fx.glowIntensity);
            m_effectShader.setInt("uTexture", 0);
            m_effectShader.setInt("uFeedback", 1); // reuse sampler for glow texture
            m_effectShader.setMat3("uTransform", glm::mat3(1.0f));
            glActiveTexture(GL_TEXTURE0);
            glBindTexture(GL_TEXTURE_2D, originalTex);
            glActiveTexture(GL_TEXTURE1);
            glBindTexture(GL_TEXTURE_2D, blurSrc);
            m_quad.draw();
            currentTex = m_effectFBO[pingpong].textureId();
            pingpong = 1 - pingpong;
        } else if (fx.type == EffectType::Sharpen) {
            // Unsharp mask. Amount can pulse with the mic via fxAudioMod
            // (audioAmount adds on top of the base when a band is selected).
            float amount = fx.sharpenAmount + fxAudioMod(fx) * 3.0f;
            if (amount <= 0.001f) continue;  // nothing to do this frame
            m_effectFBO[pingpong].bind();
            glViewport(0, 0, m_width, m_height);
            glClear(GL_COLOR_BUFFER_BIT);
            m_effectShader.use();
            m_effectShader.setInt("uEffectType", 7);
            m_effectShader.setFloat("uSharpenAmount", amount);
            m_effectShader.setFloat("uSharpenRadius", fx.sharpenRadius);
            m_effectShader.setVec2("uResolution", glm::vec2(m_width, m_height));
            m_effectShader.setInt("uTexture", 0);
            m_effectShader.setMat3("uTransform", glm::mat3(1.0f));
            glActiveTexture(GL_TEXTURE0);
            glBindTexture(GL_TEXTURE_2D, currentTex);
            m_quad.draw();
            currentTex = m_effectFBO[pingpong].textureId();
            pingpong = 1 - pingpong;
        }
    }

    Framebuffer::unbind();
    return currentTex;
}

void CompositeEngine::composite(const std::vector<std::shared_ptr<Layer>>& layers) {
    clear();
    m_frame++;
    releaseIdleScratch();

    float dt = m_audio.time - m_lastTime;
    if (dt <= 0 || dt > 0.5f) dt = 1.0f / 60.0f; // clamp to sane range
    m_lastTime = m_audio.time;

    // Prune per-layer GPU caches. Feedback FBOs (full-canvas, one per layer
    // ID) are kept only while their layer still has a Feedback effect — they
    // used to survive disabling/removing the effect until the layer itself
    // was deleted. ISF transition shaders are deliberately NOT pruned by
    // live path (a transition is "live" only during its window, and pruning
    // then meant a driver-compile hitch at every window); they're LRU-capped
    // instead.
    if (!m_feedbackFBOs.empty()) {
        std::unordered_set<uint32_t> feedbackIds;
        for (const auto& l : layers) {
            if (!l) continue;
            for (const auto& fx : l->effects) {
                if (fx.type == EffectType::Feedback) {
                    feedbackIds.insert(l->id);
                    break;
                }
            }
        }
        for (auto it = m_feedbackFBOs.begin(); it != m_feedbackFBOs.end(); ) {
            if (feedbackIds.count(it->first) == 0) it = m_feedbackFBOs.erase(it);
            else ++it;
        }
    }
    constexpr size_t kMaxIsfTransitions = 8;
    while (m_isfTransitions.size() > kMaxIsfTransitions) {
        auto lru = m_isfTransitions.end();
        uint64_t oldest = UINT64_MAX;
        for (auto it = m_isfTransitions.begin(); it != m_isfTransitions.end(); ++it) {
            auto u = m_isfTransitionUsed.find(it->first);
            uint64_t stamp = (u != m_isfTransitionUsed.end()) ? u->second : 0;
            // Never evict an entry rendering this/last frame — with >8
            // simultaneously-active transition paths that would evict and
            // recompile a LIVE transition every frame.
            if (stamp + 1 >= m_frame) continue;
            if (stamp < oldest) { oldest = stamp; lru = it; }
        }
        if (lru == m_isfTransitions.end()) break; // everything hot; exceed cap
        m_isfTransitionUsed.erase(lru->first);
        m_isfTransitions.erase(lru);
    }

    // Solo-aware visibility: if any layer is solo'd, only solo'd layers render.
    bool anySoloed = false;
    for (const auto& l : layers) {
        if (l && l->soloed) { anySoloed = true; break; }
    }

    bool firstLayer = true;
    for (int i = 0; i < (int)layers.size(); i++) {
        const auto& layer = layers[i];

        // Hard skip — double-click on a floating thumbnail toggles visible.
        // Don't render the layer at all when hidden, so transitions and
        // shader transition paths can't sneak it back on canvas either.
        if (layer && !layer->visible) continue;

        // Update transition state (opacity-based fade)
        if (layer->transitionActive && layer->transitionDuration > 0.0f) {
            float step = dt / std::max(0.01f, layer->transitionDuration);
            if (layer->transitionDirection) {
                layer->transitionProgress = std::min(1.0f, layer->transitionProgress + step);
                if (layer->transitionProgress >= 1.0f) layer->transitionActive = false;
            } else {
                layer->transitionProgress = std::max(0.0f, layer->transitionProgress - step);
                if (layer->transitionProgress <= 0.0f) {
                    layer->transitionActive = false;
                    layer->visible = false;
                    layer->transitionProgress = 0.0f;
                }
            }
        }

        // Shader-based A→B transition: advance progress, swap sources on completion
        if (layer->shaderTransitionActive && layer->transitionDuration > 0.0f) {
            float step = dt / std::max(0.01f, layer->transitionDuration);
            layer->transitionProgress = std::min(1.0f, layer->transitionProgress + step);
            if (layer->transitionProgress >= 1.0f) {
                // Promote B → A, clear B
                layer->source = std::move(layer->nextSource);
                layer->nextSource.reset();
                layer->shaderTransitionActive = false;
                layer->transitionProgress = 1.0f;
            }
        }

        // gl-transitions.com A→B transition — same promote-B→A semantics.
        if (layer->glTransitionActive && layer->transitionDuration > 0.0f) {
            float step = dt / std::max(0.01f, layer->transitionDuration);
            layer->transitionProgress = std::min(1.0f, layer->transitionProgress + step);
            if (layer->transitionProgress >= 1.0f) {
                layer->source = std::move(layer->nextSource);
                layer->nextSource.reset();
                layer->glTransitionActive = false;
                layer->transitionProgress = 1.0f;
            }
        }

        // Compute effective opacity (skip for A→B transitions — A+B are fully visible in the blend)
        // layer->visible is the iPad-style hide toggle from the floating
        // thumbnails (double-click). false -> fully transparent, preserving
        // the user's opacity slider value for when they toggle back on.
        float effectiveOpacity = layer->visible ? layer->opacity : 0.0f;
        if (layer->transitionDuration > 0.0f
            && !layer->shaderTransitionActive
            && !layer->glTransitionActive) {
            effectiveOpacity *= layer->transitionProgress;
        }

        // Skip fully transparent or invisible (but allow transitioning-out layers to render)
        if (effectiveOpacity <= 0.0f || !layer->textureId()) continue;
        if (layer->userHidden) continue;
        if (layer->muted) continue;
        if (anySoloed && !layer->soloed) continue;
        if (!layer->visible && !layer->transitionActive) continue;

        // Apply audio bindings to layer properties (temporary modulation)
        float savedOpacity = layer->opacity;
        glm::vec2 savedPos = layer->position;
        glm::vec2 savedScale = layer->scale;
        float savedRot = layer->rotation;
        for (const auto& ab : layer->audioBindings) {
            if (ab.target == Layer::AudioTarget::None) continue;
            float sig = 0;
            switch (ab.signal) {
                case 0: sig = m_audio.bass; break;
                case 1: sig = (m_audio.lowMid + m_audio.highMid) * 0.5f; break;
                case 2: sig = m_audio.treble; break;
                case 3: sig = m_audio.beatDecay; break;
            }
            float mod = sig * ab.strength;
            switch (ab.target) {
                case Layer::AudioTarget::Opacity: effectiveOpacity *= (1.0f - ab.strength + ab.strength * sig); break;
                case Layer::AudioTarget::PositionX: layer->position.x += mod * 0.5f; break;
                case Layer::AudioTarget::PositionY: layer->position.y += mod * 0.5f; break;
                case Layer::AudioTarget::Scale: layer->scale *= (1.0f + mod); break;
                case Layer::AudioTarget::Rotation: layer->rotation += mod * 45.0f; break;
                default: break;
            }
        }

        // If a transition is active, blend source A and B and use the result
        // as the layer texture. gl-transitions (named library) takes precedence
        // over legacy ISF shader transitions.
        GLuint baseTex = layer->textureId();
        if (layer->glTransitionActive && layer->nextSource) {
            m_scratchUsed[3] = m_frame;
            GLuint ttex = renderLayerGLTransition(layer, m_glTransitionFBO, m_audio);
            if (ttex) baseTex = ttex;
        } else if (layer->shaderTransitionActive && layer->nextSource) {
            GLuint ttex = renderLayerShaderTransition(layer);
            if (ttex) baseTex = ttex;
        }

        // Apply per-layer effects chain
        GLuint layerTex = applyEffects(layer, baseTex);

        // Combine per-layer masks into a single mask texture (additive union)
        GLuint layerMaskTex = 0;
        float layerMaskFeather = 0.0f;
        bool layerMaskInvert = false;
        {
            int validMasks = 0;
            GLuint singleMaskTex = 0;
            for (auto& mask : layer->masks) {
                if (mask.texture && mask.texture->id() && mask.path.count() >= 3) {
                    validMasks++;
                    singleMaskTex = mask.texture->id();
                    // Use the first valid mask's feather/invert as the layer mask params
                    if (validMasks == 1) {
                        layerMaskFeather = mask.feather;
                        layerMaskInvert = mask.invert;
                    }
                }
            }
            if (validMasks == 1) {
                layerMaskTex = singleMaskTex;
            } else if (validMasks > 1) {
                // Ensure mask FBOs match canvas size
                for (int fi = 0; fi < 2; fi++) {
                    if (m_maskFBO[fi].width() != m_width || m_maskFBO[fi].height() != m_height)
                        m_maskFBO[fi].create(m_width, m_height);
                }
                m_scratchUsed[1] = m_frame;
                int mpp = 0;
                bool first = true;
                for (auto& mask : layer->masks) {
                    if (!mask.texture || !mask.texture->id() || mask.path.count() < 3) continue;
                    m_maskFBO[mpp].bind();
                    if (first) {
                        glClearColor(0, 0, 0, 0);
                        glClear(GL_COLOR_BUFFER_BIT);
                    }
                    glEnable(GL_BLEND);
                    glBlendFunc(GL_ONE, GL_ONE); // additive
                    m_passthroughShader.use();
                    m_passthroughShader.setInt("uTexture", 0);
                    m_passthroughShader.setFloat("uOpacity", 1.0f);
                    m_passthroughShader.setMat3("uTransform", glm::mat3(1.0f));
                    m_passthroughShader.setBool("uHasMask", false);
                    m_passthroughShader.setBool("uFlipV", false);
                    m_passthroughShader.setFloat("uTileX", 1.0f);
                    m_passthroughShader.setFloat("uTileY", 1.0f);
                    m_passthroughShader.setInt("uMosaicMode", 0);
                    m_passthroughShader.setFloat("uFeather", 0.0f);
                    glActiveTexture(GL_TEXTURE0);
                    glBindTexture(GL_TEXTURE_2D, mask.texture->id());
                    m_quad.draw();
                    glDisable(GL_BLEND);
                    if (first) { first = false; }
                }
                layerMaskTex = m_maskFBO[mpp].textureId();
                Framebuffer::unbind();
            }
        }

        int next = 1 - m_current;
        m_fbo[next].bind();
        glViewport(0, 0, m_width, m_height);
        glClearColor(0, 0, 0, 0);
        glClear(GL_COLOR_BUFFER_BIT);

        float audioStrength = layer->audioReactive ? layer->audioStrength : 0.0f;

        // Build native-size transform: normalized by canvas height so aspect ratio
        // is always preserved.  scale (1,1) = source fills canvas height.
        // Layers look consistent regardless of canvas resolution/aspect.
        // When mosaic tiling is active, fill the canvas so the pattern covers everything.
        bool mosaicFill = (layer->tileX > 1.0f || layer->tileY > 1.0f ||
                           layer->mosaicMode != MosaicMode::Mirror);
        glm::mat3 nativeScale(1.0f);
        if (!mosaicFill) {
            int lw = layer->width(), lh = layer->height();
            if (lw > 0 && lh > 0 && m_width > 0 && m_height > 0) {
                // Height-normalized: Y fills canvas, X preserves aspect
                float srcAspect = (float)lw / lh;
                float canvasAspect = (float)m_width / m_height;
                nativeScale[0][0] = srcAspect / canvasAspect;
                nativeScale[1][1] = 1.0f;
            }
        }
        glm::mat3 layerXform = layer->getTransformMatrix() * nativeScale;

        // --- Drop shadow pre-pass ---
        // Render layer silhouette → blur → composite with offset BEFORE the main layer draw
        if (layer->dropShadowEnabled && layer->dropShadowOpacity > 0.001f) {
            // Ensure shadow FBOs match canvas size
            for (int fi = 0; fi < 2; fi++) {
                if (m_shadowFBO[fi].width() != m_width || m_shadowFBO[fi].height() != m_height)
                    m_shadowFBO[fi].create(m_width, m_height);
            }
            m_scratchUsed[2] = m_frame;

            // Step 1: Render the layer silhouette into m_shadowFBO[0] at the layer's transform
            m_shadowFBO[0].bind();
            glViewport(0, 0, m_width, m_height);
            glClearColor(0, 0, 0, 0);
            glClear(GL_COLOR_BUFFER_BIT);
            m_shadowShader.use();
            m_shadowShader.setMat3("uTransform", layerXform);
            m_shadowShader.setBool("uFlipV", layer->source && layer->source->isFlippedV());
            m_shadowShader.setInt("uTexture", 0);
            m_shadowShader.setVec3("uShadowColor", glm::vec3(layer->dropShadowColorR,
                                                              layer->dropShadowColorG,
                                                              layer->dropShadowColorB));
            m_shadowShader.setFloat("uShadowOpacity", 1.0f); // opacity applied later
            m_shadowShader.setFloat("uShadowSpread", layer->dropShadowSpread);
            glEnable(GL_BLEND);
            glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
            glActiveTexture(GL_TEXTURE0);
            glBindTexture(GL_TEXTURE_2D, layerTex);
            m_quad.draw();
            glDisable(GL_BLEND);

            // Step 2: Smooth separable Gaussian blur.
            //   - For small blurs (<= 12px): iterate small-radius passes at full res.
            //   - For large blurs (> 12px): downsample to 1/4 res (bilinear = cheap smoothing),
            //     blur at low res with scaled radius, then let bilinear filtering upsample.
            float targetBlur = layer->dropShadowBlur;
            GLuint blurredShadow = m_shadowFBO[0].textureId();

            if (targetBlur <= 0.1f) {
                // No blur
            } else if (targetBlur <= 12.0f) {
                // Full-res iterative small-radius blur for crisp small shadows
                float passRadius = 1.5f;
                int iterations = std::max(1, (int)std::ceil(targetBlur / passRadius));
                iterations = std::min(iterations, 8);
                int src = 0;
                for (int it = 0; it < iterations; it++) {
                    for (int dir = 0; dir < 2; dir++) {
                        int dst = 1 - src;
                        m_shadowFBO[dst].bind();
                        glClearColor(0, 0, 0, 0);
                        glClear(GL_COLOR_BUFFER_BIT);
                        m_effectShader.use();
                        m_effectShader.setInt("uEffectType", 0);
                        m_effectShader.setInt("uBlurPass", dir);
                        m_effectShader.setFloat("uBlurRadius", passRadius);
                        m_effectShader.setVec2("uResolution", glm::vec2(m_width, m_height));
                        m_effectShader.setInt("uTexture", 0);
                        m_effectShader.setMat3("uTransform", glm::mat3(1.0f));
                        glActiveTexture(GL_TEXTURE0);
                        glBindTexture(GL_TEXTURE_2D, m_shadowFBO[src].textureId());
                        m_quad.draw();
                        src = dst;
                    }
                }
                blurredShadow = m_shadowFBO[src].textureId();
            } else {
                // Downsample + low-res blur for smooth, soft large shadows
                int lw = std::max(16, m_width / 4);
                int lh = std::max(16, m_height / 4);
                for (int fi = 0; fi < 2; fi++) {
                    if (m_shadowLoRes[fi].width() != lw || m_shadowLoRes[fi].height() != lh)
                        m_shadowLoRes[fi].create(lw, lh);
                }

                // Downsample full-res silhouette into m_shadowLoRes[0]
                m_shadowLoRes[0].bind();
                glViewport(0, 0, lw, lh);
                glClearColor(0, 0, 0, 0);
                glClear(GL_COLOR_BUFFER_BIT);
                m_passthroughShader.use();
                m_passthroughShader.setMat3("uTransform", glm::mat3(1.0f));
                m_passthroughShader.setBool("uFlipV", false);
                m_passthroughShader.setFloat("uOpacity", 1.0f);
                m_passthroughShader.setInt("uTexture", 0);
                m_passthroughShader.setBool("uHasMask", false);
                m_passthroughShader.setFloat("uTileX", 1.0f);
                m_passthroughShader.setFloat("uTileY", 1.0f);
                m_passthroughShader.setInt("uMosaicMode", 0);
                m_passthroughShader.setFloat("uFeather", 0.0f);
                glActiveTexture(GL_TEXTURE0);
                glBindTexture(GL_TEXTURE_2D, m_shadowFBO[0].textureId());
                m_quad.draw();

                // Iterate blur at low res with small per-pass radius for smoothness
                float passRadius = 2.0f;
                float loResBlur = targetBlur * 0.25f; // blur at 1/4 res
                int iterations = std::max(2, (int)std::ceil(loResBlur / passRadius));
                iterations = std::min(iterations, 20);
                int src = 0;
                for (int it = 0; it < iterations; it++) {
                    for (int dir = 0; dir < 2; dir++) {
                        int dst = 1 - src;
                        m_shadowLoRes[dst].bind();
                        glViewport(0, 0, lw, lh);
                        glClearColor(0, 0, 0, 0);
                        glClear(GL_COLOR_BUFFER_BIT);
                        m_effectShader.use();
                        m_effectShader.setInt("uEffectType", 0);
                        m_effectShader.setInt("uBlurPass", dir);
                        m_effectShader.setFloat("uBlurRadius", passRadius);
                        m_effectShader.setVec2("uResolution", glm::vec2(lw, lh));
                        m_effectShader.setInt("uTexture", 0);
                        m_effectShader.setMat3("uTransform", glm::mat3(1.0f));
                        glActiveTexture(GL_TEXTURE0);
                        glBindTexture(GL_TEXTURE_2D, m_shadowLoRes[src].textureId());
                        m_quad.draw();
                        src = dst;
                    }
                }
                blurredShadow = m_shadowLoRes[src].textureId();
                // Restore full-res viewport for subsequent draws
                glViewport(0, 0, m_width, m_height);
            }

            // Step 3: Composite the blurred shadow into m_fbo[next] with offset
            int shadowNext = 1 - m_current;
            m_fbo[shadowNext].bind();
            glViewport(0, 0, m_width, m_height);
            glClearColor(0, 0, 0, 0);
            glClear(GL_COLOR_BUFFER_BIT);

            // Draw the base (accumulated) first
            m_passthroughShader.use();
            m_passthroughShader.setMat3("uTransform", glm::mat3(1.0f));
            m_passthroughShader.setBool("uFlipV", false);
            m_passthroughShader.setFloat("uOpacity", 1.0f);
            m_passthroughShader.setInt("uTexture", 0);
            m_passthroughShader.setBool("uHasMask", false);
            m_passthroughShader.setFloat("uTileX", 1.0f);
            m_passthroughShader.setFloat("uTileY", 1.0f);
            m_passthroughShader.setInt("uMosaicMode", 0);
            m_passthroughShader.setFloat("uFeather", 0.0f);
            glActiveTexture(GL_TEXTURE0);
            glBindTexture(GL_TEXTURE_2D, m_fbo[m_current].textureId());
            m_quad.draw();

            // Now overlay the shadow with offset
            glm::mat3 shadowOffset(1.0f);
            shadowOffset[2][0] = layer->dropShadowOffsetX; // translate X in NDC
            shadowOffset[2][1] = -layer->dropShadowOffsetY; // translate Y (NDC Y up, user offset Y down)
            m_passthroughShader.setMat3("uTransform", shadowOffset);
            m_passthroughShader.setFloat("uOpacity", layer->dropShadowOpacity * effectiveOpacity);
            glEnable(GL_BLEND);
            glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
            glActiveTexture(GL_TEXTURE0);
            glBindTexture(GL_TEXTURE_2D, blurredShadow);
            m_quad.draw();
            glDisable(GL_BLEND);

            // Swap: the shadow composite becomes the new "current" base
            m_current = shadowNext;
            // Force composite path so the layer draws on top of the shadow base
            firstLayer = false;
        }

        if (firstLayer && layer->blendMode == BlendMode::Normal) {
            // First layer: just draw it directly
            m_passthroughShader.use();
            m_passthroughShader.setMat3("uTransform", layerXform);
            m_passthroughShader.setBool("uFlipV", layer->source && layer->source->isFlippedV());
            m_passthroughShader.setFloat("uOpacity", effectiveOpacity);
            m_passthroughShader.setInt("uTexture", 0);
            m_passthroughShader.setFloat("uTileX", layer->tileX);
            m_passthroughShader.setFloat("uTileY", layer->tileY);
            m_passthroughShader.setVec4("uCrop", glm::vec4(
                layer->cropLeft, layer->cropRight, layer->cropTop, layer->cropBottom));
            m_passthroughShader.setInt("uMosaicMode", (int)layer->mosaicMode);
            m_passthroughShader.setFloat("uMosaicDensity", layer->mosaicDensity);
            m_passthroughShader.setFloat("uMosaicSpin", layer->mosaicSpin);
            m_passthroughShader.setFloat("uTime", m_audio.time);
            m_passthroughShader.setFloat("uFeather", layer->feather);
            setAudioUniforms(m_passthroughShader, audioStrength);
            {
                float mt = glm::clamp((m_audio.time - layer->mosaicTransitionStart) / layer->mosaicTransitionDuration, 0.0f, 1.0f);
                m_passthroughShader.setInt("uMosaicModeFrom", (int)layer->mosaicModeFrom);
                m_passthroughShader.setFloat("uMosaicTransition", mt);
            }
            glActiveTexture(GL_TEXTURE0);
            glBindTexture(GL_TEXTURE_2D, layerTex);

            if (layerMaskTex) {
                m_passthroughShader.setBool("uHasMask", true);
                glActiveTexture(GL_TEXTURE2);
                glBindTexture(GL_TEXTURE_2D, layerMaskTex);
                m_passthroughShader.setInt("uMask", 2);
                m_passthroughShader.setFloat("uMaskFeather", layerMaskFeather);
                m_passthroughShader.setBool("uMaskInvert", layerMaskInvert);
            } else {
                m_passthroughShader.setBool("uHasMask", false);
                m_passthroughShader.setFloat("uMaskFeather", 0.0f);
                m_passthroughShader.setBool("uMaskInvert", false);
            }
        } else {
            m_compositeShader.use();
            // Draw a full-screen quad (identity transform) — the fragment
            // shader uses uInvTransform to sample the layer texture at the
            // correct position.
            m_compositeShader.setMat3("uTransform", glm::mat3(1.0f));
            m_compositeShader.setMat3("uInvTransform", glm::inverse(layerXform));
            m_compositeShader.setBool("uFlipV", layer->source && layer->source->isFlippedV());
            m_compositeShader.setInt("uBase", 0);
            m_compositeShader.setInt("uLayer", 1);
            m_compositeShader.setFloat("uOpacity", effectiveOpacity);
            m_compositeShader.setInt("uBlendMode", (int)layer->blendMode);
            m_compositeShader.setFloat("uTileX", layer->tileX);
            m_compositeShader.setFloat("uTileY", layer->tileY);
            m_compositeShader.setVec4("uCrop", glm::vec4(
                layer->cropLeft, layer->cropRight, layer->cropTop, layer->cropBottom));
            m_compositeShader.setInt("uMosaicMode", (int)layer->mosaicMode);
            m_compositeShader.setFloat("uMosaicDensity", layer->mosaicDensity);
            m_compositeShader.setFloat("uMosaicSpin", layer->mosaicSpin);
            m_compositeShader.setFloat("uTime", m_audio.time);
            m_compositeShader.setFloat("uFeather", layer->feather);
            setAudioUniforms(m_compositeShader, audioStrength);
            {
                float mt = glm::clamp((m_audio.time - layer->mosaicTransitionStart) / layer->mosaicTransitionDuration, 0.0f, 1.0f);
                m_compositeShader.setInt("uMosaicModeFrom", (int)layer->mosaicModeFrom);
                m_compositeShader.setFloat("uMosaicTransition", mt);
            }

            // Base (accumulated)
            glActiveTexture(GL_TEXTURE0);
            glBindTexture(GL_TEXTURE_2D, m_fbo[m_current].textureId());

            // Current layer
            glActiveTexture(GL_TEXTURE1);
            glBindTexture(GL_TEXTURE_2D, layerTex);

            if (layerMaskTex) {
                m_compositeShader.setBool("uHasMask", true);
                glActiveTexture(GL_TEXTURE2);
                glBindTexture(GL_TEXTURE_2D, layerMaskTex);
                m_compositeShader.setInt("uMask", 2);
                m_compositeShader.setFloat("uMaskFeather", layerMaskFeather);
                m_compositeShader.setBool("uMaskInvert", layerMaskInvert);
            } else {
                m_compositeShader.setBool("uHasMask", false);
                m_compositeShader.setFloat("uMaskFeather", 0.0f);
                m_compositeShader.setBool("uMaskInvert", false);
            }
        }

        glEnable(GL_BLEND);
        glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
        m_quad.draw();
        glDisable(GL_BLEND);

        // ── Between-row shader blend ────────────────────────────────────
        // If a TimelineTransition has annotated this "to" layer, replace
        // m_fbo[next]'s contents with transition(from, to, progress), where
        // from = m_fbo[m_current] (stack through the previous layer) and
        // to = what we just wrote to m_fbo[next] (accumulated + this layer).
        //
        // ── ISF shader transition (user-picked from ShaderClaw etc.) ────
        // If the transition references an ISF `.fs` shader path, drive the
        // A→B blend through that shader. Contract:
        //   - shader has `from`/`to`/`progress` inputs  → full transition
        //     (the shader owns the blend, we just wire the textures)
        //   - shader is missing any of those → interstitial fallback:
        //     crossfade A → shader → B across the transition window.
        if (layer->betweenRowActive && !layer->betweenRowShaderPath.empty()) {
            const std::string& trPath = layer->betweenRowShaderPath;
            std::shared_ptr<ShaderSource> slot;
            auto slotIt = m_isfTransitions.find(trPath);
            if (slotIt != m_isfTransitions.end()) slot = slotIt->second;
            if (!slot) {
                auto s = std::make_shared<ShaderSource>();
                if (s->loadFromFile(trPath)) {
                    s->setResolution(m_width, m_height);
                    slot = s;
                    m_isfTransitions[trPath] = s;
                } else {
                    std::cerr << "[CompositeEngine] ISF transition failed to load: "
                              << trPath << "\n";
                    // Not cached: retried next frame (as before), and a
                    // broken path doesn't consume an LRU slot.
                }
            } else if (slot->width() != m_width || slot->height() != m_height) {
                slot->setResolution(m_width, m_height);
            }
            if (slot) m_isfTransitionUsed[trPath] = m_frame;

            if (slot) {
                bool hasFrom = false, hasTo = false, hasProgress = false;
                for (const auto& inp : slot->inputs()) {
                    if (inp.name == "from"     && inp.type == "image") hasFrom = true;
                    else if (inp.name == "to"       && inp.type == "image") hasTo = true;
                    else if (inp.name == "progress" && inp.type == "float") hasProgress = true;
                }

                // Scratch "to" — copy m_fbo[next] (which holds A+B stacked) so
                // we can read it while writing back into m_fbo[next].
                if (m_betweenRowFBO.width() != m_width
                    || m_betweenRowFBO.height() != m_height) {
                    if (m_betweenRowFBO.width() == 0)
                        m_betweenRowFBO.create(m_width, m_height);
                    else
                        m_betweenRowFBO.resize(m_width, m_height);
                }
                m_scratchUsed[4] = m_frame;
                GLuint toSrc   = m_fbo[next].textureId();
                GLuint fromSrc = m_fbo[m_current].textureId();

                auto drawPassthrough = [&](GLuint texId, float opacity) {
                    m_passthroughShader.use();
                    m_passthroughShader.setInt("uTexture", 0);
                    m_passthroughShader.setFloat("uOpacity", opacity);
                    m_passthroughShader.setMat3("uTransform", glm::mat3(1.0f));
                    m_passthroughShader.setBool("uHasMask", false);
                    m_passthroughShader.setBool("uFlipV", false);
                    m_passthroughShader.setFloat("uTileX", 1.0f);
                    m_passthroughShader.setFloat("uTileY", 1.0f);
                    m_passthroughShader.setInt("uMosaicMode", 0);
                    m_passthroughShader.setFloat("uFeather", 0.0f);
                    glActiveTexture(GL_TEXTURE0);
                    glBindTexture(GL_TEXTURE_2D, texId);
                    m_quad.draw();
                };

                // Copy A+B stacked result into scratch (our "to" texture).
                m_betweenRowFBO.bind();
                glViewport(0, 0, m_width, m_height);
                glClearColor(0, 0, 0, 0);
                glClear(GL_COLOR_BUFFER_BIT);
                drawPassthrough(toSrc, 1.0f);
                Framebuffer::unbind();

                float p = layer->betweenRowProgress;

                // Keep transition shader audio opt-in through the layer Audio toggle.
                bool audioOn = layer->audioReactive;
                slot->setAudioState(audioOn ? m_audio.rms : 0.0f,
                                    audioOn ? m_audio.bass : 0.0f,
                                    audioOn ? (m_audio.lowMid + m_audio.highMid) * 0.5f : 0.0f,
                                    audioOn ? m_audio.treble : 0.0f,
                                    audioOn ? m_audio.fftTexture : 0);

                if (hasFrom && hasTo && hasProgress) {
                    // (d) Full contract: shader is the transition.
                    slot->bindImageInput("from", fromSrc,
                                         m_width, m_height, 0, false);
                    slot->bindImageInput("to", m_betweenRowFBO.textureId(),
                                         m_width, m_height, 0, false);
                    slot->setFloat("progress", p);
                    slot->update();

                    m_fbo[next].bind();
                    glViewport(0, 0, m_width, m_height);
                    glClearColor(0, 0, 0, 0);
                    glClear(GL_COLOR_BUFFER_BIT);
                    drawPassthrough(slot->textureId(), 1.0f);
                    Framebuffer::unbind();
                } else {
                    // (a) Interstitial fallback: A → shader → B across the
                    // window. Each phase is a plain alpha crossfade, so a
                    // generative shader with no transition contract still
                    // slots in cleanly between the two clips.
                    if (hasProgress) slot->setFloat("progress", p);
                    slot->update();
                    GLuint shaderTex = slot->textureId();

                    float fromW, shaderW, toW;
                    if (p < 0.5f) {
                        float t = p * 2.0f;          // 0..1 in the first half
                        fromW   = 1.0f - t;
                        shaderW = t;
                        toW     = 0.0f;
                    } else {
                        float t = (p - 0.5f) * 2.0f; // 0..1 in the second half
                        fromW   = 0.0f;
                        shaderW = 1.0f - t;
                        toW     = t;
                    }

                    m_fbo[next].bind();
                    glViewport(0, 0, m_width, m_height);
                    glClearColor(0, 0, 0, 0);
                    glClear(GL_COLOR_BUFFER_BIT);
                    glEnable(GL_BLEND);
                    glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
                    if (fromW   > 0.001f) drawPassthrough(fromSrc,   fromW);
                    if (shaderW > 0.001f) drawPassthrough(shaderTex, shaderW);
                    if (toW     > 0.001f) drawPassthrough(m_betweenRowFBO.textureId(), toW);
                    glDisable(GL_BLEND);
                    Framebuffer::unbind();
                }
            }
        } else if (layer->betweenRowActive && !layer->betweenRowGLName.empty()
            && layer->betweenRowShaderPath.empty()) {
            auto* xition = GLTransitionLibrary::instance().get(layer->betweenRowGLName);
            if (xition && xition->isValid()) {
                // Ensure scratch FBO matches the canvas.
                if (m_betweenRowFBO.width() != m_width
                    || m_betweenRowFBO.height() != m_height) {
                    if (m_betweenRowFBO.width() == 0)
                        m_betweenRowFBO.create(m_width, m_height);
                    else
                        m_betweenRowFBO.resize(m_width, m_height);
                }
                m_scratchUsed[4] = m_frame;
                // "to" = the just-rendered m_fbo[next]. Copy it into the
                // scratch so we can read-from-scratch while writing-to-next.
                GLuint toSrc   = m_fbo[next].textureId();
                GLuint fromSrc = m_fbo[m_current].textureId();

                m_betweenRowFBO.bind();
                glViewport(0, 0, m_width, m_height);
                glClearColor(0, 0, 0, 0);
                glClear(GL_COLOR_BUFFER_BIT);
                m_passthroughShader.use();
                m_passthroughShader.setInt("uTexture", 0);
                m_passthroughShader.setFloat("uOpacity", 1.0f);
                m_passthroughShader.setMat3("uTransform", glm::mat3(1.0f));
                m_passthroughShader.setBool("uHasMask", false);
                m_passthroughShader.setBool("uFlipV", false);
                m_passthroughShader.setFloat("uTileX", 1.0f);
                m_passthroughShader.setFloat("uTileY", 1.0f);
                m_passthroughShader.setInt("uMosaicMode", 0);
                m_passthroughShader.setFloat("uFeather", 0.0f);
                glActiveTexture(GL_TEXTURE0);
                glBindTexture(GL_TEXTURE_2D, toSrc);
                m_quad.draw();
                Framebuffer::unbind();

                // Run transition shader with from=stack-through-prev-layer,
                // to=scratch, writing directly into m_fbo[next].
                m_fbo[next].bind();
                glViewport(0, 0, m_width, m_height);
                glClearColor(0, 0, 0, 0);
                glClear(GL_COLOR_BUFFER_BIT);
                bool audioOn = layer->audioReactive;
                xition->render(fromSrc, m_betweenRowFBO.textureId(),
                               layer->betweenRowProgress,
                               m_width, m_height,
                               audioOn ? m_audio.rms : 0.0f,
                               audioOn ? m_audio.bass : 0.0f,
                               audioOn ? (m_audio.lowMid + m_audio.highMid) * 0.5f : 0.0f,
                               audioOn ? m_audio.treble : 0.0f,
                               audioOn ? m_audio.beatDecay : 0.0f);
                Framebuffer::unbind();
            }
        }

        m_current = next;
        firstLayer = false;

        // Restore audio-modulated properties
        layer->opacity = savedOpacity;
        layer->position = savedPos;
        layer->scale = savedScale;
        layer->rotation = savedRot;
    }

    Framebuffer::unbind();

}

GLuint CompositeEngine::resultTexture() const {
    return m_fbo[m_current].textureId();
}

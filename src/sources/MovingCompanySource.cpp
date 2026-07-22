#include "sources/MovingCompanySource.h"

#include <glad/glad.h>
#include <GLFW/glfw3.h>
#include <glm/gtc/matrix_transform.hpp>
#include <filesystem>
#include <iostream>

// ─── Embedded GLSL ─────────────────────────────────────────────────────────

// Fullscreen backdrop: deep-space nebula + warp-speed star streaks radiating
// from the vanishing point. The radial streaks are what sell forward motion.
static const char* kBgVert = R"(#version 330 core
layout(location=0) in vec2 aPos;
layout(location=1) in vec2 aUV;
out vec2 vUV;
void main(){
    vUV = aUV;
    gl_Position = vec4(aPos, 0.0, 1.0);
}
)";

static const char* kBgFrag = R"(#version 330 core
in vec2 vUV;
out vec4 FragColor;
uniform float iTime;
uniform vec2  iResolution;
uniform float uWarpSpeed;
uniform float uWarpIntensity;
uniform float uStarDensity;
uniform float uNebulaAmount;

float hash21(vec2 p){
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}
float vnoise(vec2 p){
    vec2 i = floor(p), f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash21(i),            b = hash21(i + vec2(1,0));
    float c = hash21(i + vec2(0,1)), d = hash21(i + vec2(1,1));
    return mix(mix(a,b,f.x), mix(c,d,f.x), f.y);
}
float fbm(vec2 p){
    float s = 0.0, a = 0.5;
    for(int i = 0; i < 5; i++){ s += a * vnoise(p); p *= 2.02; a *= 0.5; }
    return s;
}

void main(){
    vec2 uv = (gl_FragCoord.xy - 0.5 * iResolution.xy) / iResolution.y;
    float r = length(uv);

    // Deep-space base + drifting nebula.
    vec3 col = vec3(0.012, 0.016, 0.03);
    float neb = fbm(uv * 2.6 + vec2(iTime * 0.015, -iTime * 0.01));
    neb = pow(neb, 2.4);
    col = mix(col, vec3(0.10, 0.05, 0.22), neb * 0.8 * uNebulaAmount);
    col += vec3(0.0, 0.10, 0.18) * fbm(uv * 1.4 - iTime * 0.02) * 0.35 * uNebulaAmount;

    // Warp-speed star streaks: each star travels outward along its own ray.
    const int N = 64;
    for(int i = 0; i < N; i++){
        float fi   = float(i);
        float seed = hash21(vec2(fi, 3.0));
        float ang  = seed * 6.28318;
        vec2  dir  = vec2(cos(ang), sin(ang));
        float spd  = 0.30 + seed * 0.75;
        float ph   = fract(iTime * spd * 0.42 * uWarpSpeed + seed);
        float rad  = ph * 1.5;                  // current head radius
        float along = dot(uv, dir);
        if(along < 0.0) continue;
        float pd   = length(uv - dir * along);  // perpendicular dist to ray
        float line = smoothstep(0.006, 0.0, pd);
        float head = smoothstep(0.06, 0.0, abs(along - rad));
        float tail = clamp((along - (rad - 0.42)) / 0.42, 0.0, 1.0)
                     * step(along, rad);
        float bright = line * (head + tail * 0.65) * ph * ph;
        col += vec3(0.62, 0.76, 1.0) * bright * uWarpIntensity;
    }

    // Sparse static near-field stars for depth.
    float st = hash21(floor(gl_FragCoord.xy / iResolution.y * 220.0));
    float thr = 1.0 - 0.009 * uStarDensity;
    if(st > thr) col += vec3(0.9, 0.95, 1.0) * (st - thr) / max(1.0 - thr, 1e-4);

    // Gentle vignette to seat the jet in the frame.
    col *= 1.0 - 0.35 * smoothstep(0.5, 1.1, r);

    FragColor = vec4(col, 1.0);
}
)";

// Faceted jet: flat per-triangle normals from screen-space derivatives, so
// the F-117's stealth facets read crisply with no need for mesh normals.
static const char* kPlaneVert = R"(#version 330 core
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

static const char* kPlaneFrag = R"(#version 330 core
in vec3 vWorld;
out vec4 FragColor;
uniform vec3  uCamPos;
uniform float iTime;
uniform vec3  uJetColor;
uniform vec3  uRimColor;
uniform float uJetBrightness;
uniform float uRimIntensity;
void main(){
    // Flat facet normal (the whole point of the F-117 silhouette).
    vec3 n = normalize(cross(dFdx(vWorld), dFdy(vWorld)));
    vec3 V = normalize(uCamPos - vWorld);
    if(dot(n, V) < 0.0) n = -n;            // always light the visible face

    vec3 L1 = normalize(vec3(-0.45, 0.85, 0.35));   // cool key (above-left)
    vec3 L2 = normalize(vec3( 0.65, 0.20, -0.55));  // warm-ish fill (behind)
    float d1 = max(dot(n, L1), 0.0);
    float d2 = max(dot(n, L2), 0.0);
    float fres = pow(1.0 - max(dot(n, V), 0.0), 3.0);

    // Matte radar-absorbent body.
    vec3 base = uJetColor;
    vec3 col  = base * (0.22 + uJetBrightness * d1);
    col += base * d2 * 0.30;
    // Rim so the jet pops off the nebula.
    col += uRimColor * fres * uRimIntensity;
    // Faint specular glint sliding across facets over time.
    vec3 H = normalize(L1 + V);
    col += uRimColor * pow(max(dot(n, H), 0.0), 48.0) * 0.35;

    FragColor = vec4(col, 1.0);
}
)";

// ─── MovingCompanySource ───────────────────────────────────────────────────

bool MovingCompanySource::init(int resolutionW, int resolutionH) {
    m_w = resolutionW;
    m_h = resolutionH;

    if (!m_output.create(m_w, m_h, /*withDepth=*/true)) {
        std::cerr << "[MovingCompany] FBO create failed" << std::endl;
        return false;
    }
    m_quad.createQuad();

    if (!m_bgShader.loadFromSource(kBgVert, kBgFrag)) {
        std::cerr << "[MovingCompany] bg shader compile failed" << std::endl;
        return false;
    }
    if (!m_planeShader.loadFromSource(kPlaneVert, kPlaneFrag)) {
        std::cerr << "[MovingCompany] plane shader compile failed" << std::endl;
        return false;
    }

    if (!loadModel()) {
        std::cerr << "[MovingCompany] could not load F-117 GLB" << std::endl;
        return false;
    }

    m_startTime = glfwGetTime();
    m_ready = true;
    return true;
}

bool MovingCompanySource::loadModel() {
    const char* candidates[] = {
        "assets/models/stealth_f117.glb",                  // bundled (Resources CWD)
        "/Users/lu/easel/assets/models/stealth_f117.glb",  // dev repo
        "/Users/lu/Downloads/stealth_f-117a (2).glb",      // original download
    };
    for (const char* p : candidates) {
        if (!std::filesystem::exists(p)) continue;
        if (m_model.loadModel(p)) {
            m_modelPath = p;
            // ObjMeshWarp::loadModel parks the orbit camera at the bbox:
            //   target   = bbox center,  distance = bbox extent * 1.5.
            // Recover both to normalize the jet to a unit sphere at origin.
            m_center = m_model.camera().target;
            m_extent = m_model.camera().distance / 1.5f;
            if (m_extent < 1e-4f) m_extent = 1.0f;
            std::cout << "[MovingCompany] loaded " << p
                      << "  center=(" << m_center.x << "," << m_center.y
                      << "," << m_center.z << ") extent=" << m_extent << std::endl;
            return true;
        }
    }
    return false;
}

// Park the GPU render target while this source is reachable only from
// undo/redo snapshots. Called from UndoStack::suspendOrphanedSources on the
// main-thread frame sweep — the GL context is current there, so the delete
// is safe. Frees the color+depth output (~17 MB at 1080p). Shaders, the
// quad and the F-117 mesh stay resident — reloading the GLB from disk on
// revive would cost far more than the VRAM its VBOs hold.
void MovingCompanySource::suspend() {
    if (!m_ready) return;
    m_output.destroy();
    m_ready = false;
    m_suspended = true;
}

void MovingCompanySource::update() {
    if (!m_ready) {
        // Lazy revive after suspend(): recreate the FBO at the configured
        // resolution (fixed-res source — m_w/m_h are exactly what a fresh
        // init() would use, so nothing can be stale).
        if (!m_suspended) return;
        m_suspended = false;   // one attempt — no retry storm on failure
        if (!m_output.create(m_w, m_h, /*withDepth=*/true)) return;
        m_ready = true;
    }

    float t = static_cast<float>(glfwGetTime() - m_startTime);
    float aspect = (m_h > 0) ? (float)m_w / (float)m_h : 1.777f;
    const MovingCompanyParams& P = m_params;

    // Save GL state we touch (we run inside Easel's compositor loop).
    GLint prevVp[4];
    glGetIntegerv(GL_VIEWPORT, prevVp);
    GLboolean prevDepth = glIsEnabled(GL_DEPTH_TEST);
    GLboolean prevCull  = glIsEnabled(GL_CULL_FACE);
    GLboolean prevBlend = glIsEnabled(GL_BLEND);

    m_output.bind();
    glViewport(0, 0, m_w, m_h);
    glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

    // ── Backdrop: fullscreen, no depth ──
    glDisable(GL_DEPTH_TEST);
    glDisable(GL_BLEND);
    m_bgShader.use();
    m_bgShader.setFloat("iTime", t);
    m_bgShader.setVec2("iResolution", glm::vec2((float)m_w, (float)m_h));
    m_bgShader.setFloat("uWarpSpeed", P.warpSpeed);
    m_bgShader.setFloat("uWarpIntensity", P.warpIntensity);
    m_bgShader.setFloat("uStarDensity", P.starDensity);
    m_bgShader.setFloat("uNebulaAmount", P.nebulaAmount);
    m_quad.draw();

    // ── Jet: normalized to a unit sphere, banking through a chase shot ──
    // Build model matrix inner→outer:
    //   center → unit-scale → base orient → live flight animation.
    glm::mat4 toOrigin = glm::translate(glm::mat4(1.0f), -m_center);
    glm::mat4 unit     = glm::scale(glm::mat4(1.0f), glm::vec3(1.0f / m_extent));

    // Base orientation lines the jet up nose-forward for the camera. Tuned
    // visually for this asset (GLB ships +Y up, nose along -Z).
    glm::mat4 baseOrient = glm::rotate(glm::mat4(1.0f),
                                       glm::radians(P.baseYawDeg), glm::vec3(0, 1, 0));

    // Flight: slow yaw turn + lazy banking roll + gentle pitch bob.
    float yaw   = t * P.yawRate;
    float roll  = P.bankAmount  * sinf(t * 0.55f);
    float pitch = P.pitchAmount * sinf(t * 0.37f + 1.2f);
    glm::mat4 anim(1.0f);
    anim = glm::rotate(anim, yaw,   glm::vec3(0, 1, 0));
    anim = glm::rotate(anim, pitch, glm::vec3(1, 0, 0));
    anim = glm::rotate(anim, roll,  glm::vec3(0, 0, 1));

    glm::mat4 model = anim * baseOrient * unit * toOrigin;

    // Chase camera drifting around the (now origin-centered) jet.
    glm::vec3 eye(P.camDistance * sinf(t * P.orbitSpeed),
                  P.camHeight + 0.22f * sinf(t * 0.23f),
                  P.camDistance * cosf(t * P.orbitSpeed));
    glm::mat4 view = glm::lookAt(eye, glm::vec3(0.0f), glm::vec3(0, 1, 0));
    glm::mat4 proj = glm::perspective(glm::radians(P.fov), aspect, 0.01f, 100.0f);
    glm::mat4 mvp  = proj * view * model;

    glEnable(GL_DEPTH_TEST);
    glDisable(GL_CULL_FACE);     // GLB winding is unknown; normal-flip handles it
    m_planeShader.use();
    m_planeShader.setMat4("uMVP", mvp);
    m_planeShader.setMat4("uModel", model);
    m_planeShader.setVec3("uCamPos", eye);
    m_planeShader.setFloat("iTime", t);
    m_planeShader.setVec3("uJetColor", P.jetColor);
    m_planeShader.setVec3("uRimColor", P.rimColor);
    m_planeShader.setFloat("uJetBrightness", P.jetBrightness);
    m_planeShader.setFloat("uRimIntensity", P.rimIntensity);
    for (auto& mg : m_model.materials()) {
        if (mg.mesh.isLoaded()) mg.mesh.draw();
    }

    Framebuffer::unbind();

    // Restore state for the compositor.
    glViewport(prevVp[0], prevVp[1], prevVp[2], prevVp[3]);
    if (prevDepth) glEnable(GL_DEPTH_TEST); else glDisable(GL_DEPTH_TEST);
    if (prevCull)  glEnable(GL_CULL_FACE);  else glDisable(GL_CULL_FACE);
    if (prevBlend) glEnable(GL_BLEND);      else glDisable(GL_BLEND);
}

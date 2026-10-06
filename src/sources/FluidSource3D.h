#pragma once
#include "sources/ContentSource.h"
#include "sources/AudioBinding.h"
#include <glad/glad.h>
#include <cstdint>
#include <map>
#include <string>
#include <vector>

// Native 3D smoothed-particle-hydrodynamics fluid, ported from Wyatt's
// shadertoy "Particle cluster grid SPH 3D" (shadertoy.com/view/mstfzS).
//
// Each grid cell holds up to 2 particles (mass, age, pos, vel, force,
// density) bit-packed into a single RGBA32UI voxel. The sim runs as 4
// fragment passes per frame over real GL_TEXTURE_3D volumes (NOT the
// shadertoy 2D-tiling hack), using layered rendering (one FBO, a geometry
// shader routes each instance to a Z slice via gl_Layer):
//   A: advect neighbors + clusterize into this cell's 2 particles
//   B: SPH density (gaussian kernel over ±2 neighbors)
//   C: apply forces + integrate; age/kill/emit; seed a block on frame 0..9
//   D: shadow march along the light direction (optical density)
// A final raymarch (Image) traces the particle spheres — plus the scene
// (background, floor, emissive orbs, cage) — into the RGBA8 output texture.
//
// Buffer dependency graph (no ping-pong needed — each pass writes a
// distinct volume, C feeds A next frame by pass ordering):
//   A.in = C(prev)        A.out = volA
//   B.in = volA           B.out = volB
//   C.in = volA + volB    C.out = volC
//   D.in = volB           D.out = volD
//   Image.in = volC + volB + volD
//
// Particle volumes (volA/volC): RGBA32UI + NEAREST (exact bit-packed,
// texelFetch only). Density/shadow (volB/volD): RGBA16F + LINEAR (trilinear).
//
// Every tunable lives in the param table (params()) so the PropertyPanel,
// the Show page, audio/MIDI bindings, the VJ Low/High look morph and the
// project file all see the same list — add a param in ONE place.
class FluidSource3D : public ContentSource {
public:
    FluidSource3D() = default;
    ~FluidSource3D() override;

    bool init(int outW = 1280, int outH = 720);

    // ContentSource
    void update() override;
    // Undo-orphan parking: frees the 4 sim volumes + output FBO; programs
    // stay. Sim state is lost — update() lazily reallocs and re-seeds.
    void suspend() override;
    GLuint textureId() const override { return m_output.tex; }
    int width() const override { return m_outW; }
    int height() const override { return m_outH; }
    int zoneWidth() const  { return m_zoneW; }
    int zoneHeight() const { return m_zoneH; }
    std::string typeName() const override { return "Fluid3D"; }
    std::string sourcePath() const override { return "__fluid3d__"; }

    // Re-run the seed block (clears velocities, restocks the initial blob).
    // Recovers from any chaotic/blown-out state the force fields can reach.
    void reseed() { m_frame = 0; }

    // Copy every user-facing setting (params, colors, modes, orbs, bindings)
    // from another instance — used by Layer > Duplicate so each copy gets its
    // OWN simulation that can be colored / driven / blended independently.
    void copyConfigFrom(const FluidSource3D& o);

    // ── Audio/MIDI-reactive parameter bindings ────────────────────────
    std::map<std::string, AudioBinding>& audioBindings() { return m_audioBindings; }
    const std::map<std::string, AudioBinding>& audioBindings() const { return m_audioBindings; }
    void applyAudioBindings(float level, float bass, float mid, float high,
                            float beat, float dt, class MIDIManager* midi = nullptr,
                            float energy = 0.0f, float build = 0.0f, float drop = 0.0f,
                            float silence = 0.0f, float momentum = 0.5f);

    // ── Image injection ──────────────────────────────────────────────
    // Tint the fluid surface with a bound layer's texture, mapped across
    // the fluid's world x/y extent. Host refreshes textureId each frame
    // from the bound layer (mirrors FluidSource's image pattern).
    struct ImageRef {
        GLuint   textureId     = 0;
        int      width         = 0;
        int      height        = 0;
        uint32_t sourceLayerId = 0;
        bool     flippedV      = false;
    };
    ImageRef&       imageSource()       { return m_image; }
    const ImageRef& imageSource() const { return m_image; }
    // Backdrop: a bound layer shown behind the fluid (background mode "Image").
    ImageRef&       backdropSource()       { return m_backdrop; }
    const ImageRef& backdropSource() const { return m_backdrop; }
    bool  m_imageEnabled = false;
    float m_imageMix     = 1.0f;    // 0 = palette only, 1 = full image albedo (video colors)

    // ── Param table ───────────────────────────────────────────────────
    enum ParamFlags : unsigned {
        kBind = 1u << 0,   // offered for audio/MIDI binding + Show page
        kLook = 1u << 1,   // part of the VJ Low/High look morph
    };
    struct ParamDef {
        const char* id;      // stable key: bindings, project file, OSC
        const char* label;   // UI label
        float*      ptr;
        float       lo, hi;
        unsigned    flags;
    };
    struct ColorDef {
        const char* id;
        const char* label;
        float*      rgb;     // float[3]
    };
    std::vector<ParamDef> params();          // every scalar tunable
    std::vector<ColorDef> colors();          // every RGB tunable
    const ParamDef* findParam(const std::string& id);

    // ── Colors ────────────────────────────────────────────────────────
    float m_deepColor[3]  = {0.10f, 0.32f, 0.98f};    // albedo
    float m_glowColor[3]  = {1.000f, 1.000f, 1.000f}; // velocity glow (white default)
    float m_shallowColor[3]= {0.60f, 0.85f, 1.00f};    // rim / grazing-edge tint
    float m_ageColor[3]   = {1.000f, 0.300f, 0.100f}; // tint as particles age (needs Lifetime)
    float m_bgTop[3]      = {0.09f, 0.10f, 0.18f};    // dark cinematic sky
    float m_bgBottom[3]   = {0.010f, 0.010f, 0.018f};
    float m_horizonColor[3]= {0.85f, 0.42f, 0.20f};   // warm horizon band
    float m_sunColor[3]   = {1.00f, 0.96f, 0.90f};    // env sun highlight
    float m_floorColor[3] = {0.075f, 0.08f, 0.105f};  // dark glossy floor
    float m_gridColor[3]  = {1.00f, 1.00f, 1.00f};
    float m_cageColor[3]  = {1.00f, 1.00f, 1.00f};

    // ── Camera / render ───────────────────────────────────────────────
    float m_brightness    = 1.6f;     // diffuse multiplier (HDR + ACES: 2.5 blew out)
    bool  m_autoRotate    = true;
    float m_rotateSpeed   = 0.2f;     // mstfzS: angles = vec2(0.2*iTime, -0.5)
    float m_tilt          = -0.62f;   // a little more top-down so the floor reads
    float m_zoom          = 1.0f;     // camera zoom (1 = original; >1 zooms in)
    float m_audioIntensity = 0.0f;    // 0 = off; higher = calm when quiet, churns when loud
    float m_audioEnergy    = 0.0f;    // smoothed loudness (updated from audio)

    // ── Lighting / material ───────────────────────────────────────────
    float m_lightAzimuth   = 50.6f;   // degrees around the vertical axis
    float m_lightElev      = 28.5f;   // degrees above the horizon
    float m_lightDir[3]    = {0.820f, 1.000f, 0.702f}; // derived each frame from az/el
    float m_lightIntensity = 1.0f;    // diffuse key intensity
    float m_ambient        = 0.14f;   // ambient fill (shadow floor)
    float m_specular       = 1.0f;    // reflection/specular strength
    float m_roughness      = 0.0f;    // blurs the env reflection (0 = mirror)
    float m_fresnel        = 3.0f;    // grazing-angle reflection power
    float m_refract        = 0.0f;    // glassy see-through look
    float m_selfReflect    = 0.0f;    // blobs reflect each other (extra march)
    float m_rim            = 0.0f;    // rim amount (0 = off)
    float m_saturation     = 1.0f;    // output saturation
    float m_velGlow        = 1.0f;    // emissive glow by particle speed
    float m_fluidAlpha     = 1.0f;    // fluid pixel opacity (layer transparency)
    // ── Liquid material (2026-10 render overhaul) ─────────────────────
    float m_absorb         = 1.5f;    // Beer-Lambert depth colouring (thin = light, thick = deep)
    float m_subsurface     = 0.6f;    // light bleeding through thin parts
    float m_ao             = 1.0f;    // crevice occlusion
    float m_keySpec        = 1.0f;    // key-light highlight lobe
    float m_camDist        = 1.35f;   // camera distance (1 = original framing)
    float m_camHeight      = -0.2f;   // aim below centre: liquid + floor fill the frame
    // ── Finish (post pipeline: HDR → bloom → filmic) ──────────────────
    float m_exposure       = 1.0f;
    float m_contrast       = 0.35f;   // S-curve amount
    float m_bloom          = 0.7f;
    float m_bloomThreshold = 0.9f;
    float m_grain          = 0.10f;
    float m_dof            = 0.0f;    // 0 = off
    float m_dofFocus       = 0.45f;   // normalised depth in focus

    // ── Scene / background ────────────────────────────────────────────
    int   m_bgMode         = 2;       // 0 gradient 1 solid 2 horizon 3 image 4 stars
    float m_bgAlpha        = 1.0f;    // 0 = transparent bg (composites under)
    float m_horizonAmt     = 1.0f;
    float m_sunIntensity   = 1.0f;
    float m_sunSize        = 80.0f;   // highlight sharpness (pow exponent)
    float m_sunAzimuth     = 56.3f;
    float m_sunElev        = 44.2f;
    float m_fog            = 0.0f;
    float m_vignette       = 0.3f;
    float m_stars          = 0.5f;
    bool  m_floorOn        = true;
    float m_floorGap       = 0.05f;   // distance below the container (× grid size)
    float m_floorGrid      = 0.0f;
    float m_floorReflect   = 0.65f;
    float m_floorFade      = 1.0f;
    float m_cage           = 0.0f;    // emissive container edges
    float m_cageWidth      = 0.8f;

    // ── Emissive orbs (extra lights / objects in the scene) ───────────
    static constexpr int kMaxOrbs = 4;
    struct Orb {
        bool  enabled   = false;
        float color[3]  = {1.0f, 0.6f, 0.2f};
        float intensity = 1.5f;
        float size      = 0.08f;    // radius × grid size
        int   motion    = 1;        // 0 still, 1 orbit, 2 bob
        int   physics   = 1;        // 0 none, 1 solid (fluid flows around), 2 attract, 3 repel
        float force     = 1.0f;     // physics strength
        float pos[3]    = {0.5f, 0.5f, 0.6f};   // normalized 0..1 (still)
        float orbitRadius = 0.6f;   // × half grid
        float orbitHeight = 0.6f;   // normalized z
        float speed     = 0.5f;     // rad/s (orbit) / bob rate
        float phase     = 0.0f;     // degrees
        float worldPos[3] = {0,0,0};// computed each frame (grid units)
    };
    Orb   m_orbs[kMaxOrbs];
    float m_orbGlow        = 0.4f;    // halo along the view ray (bloom does most of it now)
    float m_orbLight       = 1.0f;    // orbs light the fluid
    float m_orbSpec        = 1.0f;    // orbs reflect in the fluid

    // ── Physics (0 / 1 == the original shadertoy behaviour) ───────────
    float m_gravity        = 1.0f;    // gravity strength (0 = zero-g, 1 = original)
    float m_gravityTiltX   = 0.0f;    // tilt the gravity vector
    float m_gravityTiltY   = 0.0f;
    float m_vortex         = 0.0f;    // coherent swirl around vertical axis (0 = off)
    float m_vortexLift     = 0.0f;    // tornado lift near the axis
    float m_turbulence     = 0.0f;    // chaotic turbulent churn (0 = off)
    float m_turbScale      = 1.0f;
    float m_turbSpeed      = 1.0f;
    float m_forceScale     = 1.0f;    // SPH cohesion force (1 = original)
    float m_viscosity      = 1.0f;    // inter-particle friction
    float m_tension        = 1.0f;    // surface tension
    float m_restDensity    = 1.0f;    // SPH rest density (lower = puffier)
    float m_maxVel         = 2.0f;    // velocity clamp
    float m_damping        = 0.0f;    // extra velocity damping
    float m_bounce         = 1.0f;    // wall stiffness
    float m_simSpeed       = 0.7f;    // integration step (dt)
    float m_wind           = 0.0f;
    float m_windAngle      = 0.0f;    // degrees
    float m_updraft        = 0.0f;
    float m_gust           = 0.0f;
    float m_attract        = 0.0f;    // point attractor (−repel .. +attract)
    float m_attractPos[3]  = {0.5f, 0.5f, 0.5f};
    float m_sphereShape    = 0.0f;    // container: 0 = cube (original), 1 = sphere
    bool  m_seedingSphere  = false;   // tracks the box<->sphere crossing for reseed
    float m_fillAmount     = 0.5f;    // how much fluid is seeded (0 = little .. 1 = full)
    float m_fillPrev       = -1.0f;   // tracks fill changes to trigger a reseed
    bool  m_doubleDensity  = false;   // seed 2 particles per cell (×2 particles)
    bool  m_doublePrev     = false;

    // ── Particles: size / lifetime / emitter ──────────────────────────
    float m_sphereScale    = 0.15f;   // particle sphere size (lower = smaller blobs)
    bool  m_particleCube   = false;   // render particles as cubes instead of spheres
    int   m_surfaceMode    = 0;       // 0 particles (original), 1 smooth liquid isosurface
    float m_isoLevel       = 0.25f;   // liquid surface density threshold
    float m_lifetime       = 0.0f;    // seconds; 0 = particles live forever
    float m_ageShrink      = 0.0f;    // particles shrink as they age
    float m_ageTint        = 0.0f;    // tint toward m_ageColor as they age
    bool  m_emitOn         = false;
    float m_emitRate       = 0.5f;
    int   m_emitShape      = 0;       // 0 point 1 rain 2 fountain 3 ring
    float m_emitPos[3]     = {0.5f, 0.5f, 0.5f};
    float m_emitRadius     = 0.12f;
    float m_emitSpeed      = 0.8f;
    float m_emitSpread     = 0.5f;

    // ── VJ: crossfade the ENTIRE look between a "Low" (chill) and a "High"
    // (intense) snapshot, driven by the song's structural energy. Modes:
    //   Low  — edit the chill look (live = the Low snap)
    //   High — edit the intense look
    //   Live — the music (or a hand-grabbed fader) rides Low <-> High
    struct FluidLook {
        std::map<std::string, float> v;   // param id → value (kLook params + color comps)
    };
    int       m_vjMode           = 0;         // 0=Low(edit) 1=High(edit) 2=Live(perform)
    int       m_vjModePrev       = -1;        // transient: detect mode switch to load look
    bool      m_vjGrab           = false;     // Live: grab the fader (manual position)
    int       m_journeySignal    = 0;         // 0=Energy 1=Build 2=Level 3=Momentum
    float     m_journeyGain      = 1.5f;      // scales the drive so songs reach High
    float     m_journeyPosManual = 0.0f;      // hand-grabbed fader 0..1 (Live)
    float     m_journeyPos       = 0.0f;      // live smoothed position 0..1
    float     m_journeyDropEnv   = 0.0f;      // drop-snap envelope
    bool      m_lookSet[2]       = {false, false};   // [0]=Low captured, [1]=High
    FluidLook m_look[2];                       // [0]=Low (chill), [1]=High (intense)
    void captureLook(int which);               // live look -> snapshot [which]
    void loadLook(int which);                  // snapshot [which] -> live look
    void applyJourney(float level, float energy, float build,
                      float momentum, float drop, float dt);
    // Legacy flat-array looks (pre-2026-10 projects, 30 floats).
    static constexpr int kLookFloats = 30;
    void lookFromArray(int which, const float* in);

    int   m_simRes        = 64;       // size3d cube edge (volume realloc on change)
    int   m_substeps      = 1;        // sim iterations per frame
    // Render resolution as a fraction of the zone size. 1.0 = full/crisp;
    // lower trades sharpness for speed, 2.0 = 2×2 supersampling (anti-alias).
    // Applied live (output realloc).
    float m_renderScale   = 1.0f;

private:
    struct Volume { GLuint tex = 0; int size = 0; GLint internalFmt = 0; };
    struct FBO    { GLuint fbo = 0, tex = 0; int w = 0, h = 0; };

    bool   m_ready = false;
    bool   m_suspended = false;  // volumes dropped by suspend(); update() revives
    int    m_outW = 1280, m_outH = 720;   // actual render-target dims (scaled)
    int    m_zoneW = 1280, m_zoneH = 720; // full zone res (pre-scale)
    int    m_size = 64;          // active size3d
    int    m_frame = 0;          // iFrame analogue (drives the seed block)
    double m_lastTime = 0.0;
    float  m_time = 0.0f;        // iTime analogue (drives gravity wobble + camera)

    std::map<std::string, AudioBinding> m_audioBindings;
    ImageRef m_image;
    ImageRef m_backdrop;

    Volume m_volA, m_volB, m_volC, m_volD;
    FBO    m_output;             // RGBA8, what the compositor samples
    FBO    m_hdr;                // RGBA16F linear scene (+ aux attachment 1)
    GLuint m_hdrAux = 0;         // RGBA16F: x = depth, y = thickness (DOF)
    FBO    m_bloomA, m_bloomB;   // ¼-res bloom ping-pong
    GLuint m_volFBO = 0;         // layered-render FBO, retargeted per Z slice

    GLuint m_progA = 0, m_progB = 0, m_progC = 0, m_progD = 0, m_progRender = 0;
    GLuint m_progBloomDown = 0, m_progBloomBlur = 0, m_progPost = 0;
    GLuint m_quadVAO = 0, m_quadVBO = 0;

    GLuint compileProgram(const char* vs, const char* fs);
    GLuint compileProgram(const char* vs, const char* gs, const char* fs);
    Volume createVolume(int size, GLint internalFmt, GLenum format,
                        GLenum type, GLenum filter);
    void   destroyVolume(Volume& v);
    void   clearVolume(const Volume& v, bool isInt);
    FBO    createFBO(int w, int h, GLint internalFmt, GLenum type, GLenum filter);
    void   destroyFBO(FBO& f);

    // Run a fragment pass over every Z slice of `dst` (uSlice uniform).
    // Caller binds input samplers + sets non-slice uniforms first.
    void runVolumePass(GLuint prog, const Volume& dst);
    void bindVolume(GLuint prog, const char* name, const Volume& v, int unit);
    void setSimUniforms(GLuint prog);      // physics uniforms shared by passes A + C
    void updateDerived(float dt);          // light dir, orb positions, …
    void renderToOutput();
    void renderPost();                     // bloom + tonemap into m_output
    void allocPostTargets();
    void destroyPostTargets();

    // Live realloc when m_simRes / m_renderScale change (called from update()
    // inside the GL-state-saved region).
    void reallocVolumes();
    void reallocOutput();
};

// ============================================================
// Phase 1 Proof of Concept: ISF Shader Running in Three.js
//
// Renders the metaballs ISF shader via RawShaderMaterial on a
// fullscreen quad, PLUS a 3D object (torus knot) in the same
// Three.js scene — sharing one WebGL context, no pixel readback.
//
// The ISF shader output can also be used as a texture on the
// 3D object, demonstrating the key integration benefit.
// ============================================================

(function(THREE) {

    const INPUTS = [
        // Metaballs ISF params
        { NAME: "ballCount",  TYPE: "float", DEFAULT: 6.0,  MIN: 2.0, MAX: 8.0,  LABEL: "Ball Count" },
        { NAME: "ballSpeed",  TYPE: "float", DEFAULT: 1.1,  MIN: 0.0, MAX: 4.0,  LABEL: "Speed" },
        { NAME: "smoothK",    TYPE: "float", DEFAULT: 0.68, MIN: 0.1, MAX: 1.5,  LABEL: "Blend" },
        { NAME: "keyColor",   TYPE: "color", DEFAULT: [0.4, 0.6, 1.0, 1.0],      LABEL: "Key Light" },
        { NAME: "rimColor",   TYPE: "color", DEFAULT: [0.8, 0.3, 0.6, 1.0],      LABEL: "Rim Light" },
        { NAME: "bgColor",    TYPE: "color", DEFAULT: [0.4, 0.6, 1.0, 1.0],      LABEL: "Background" },
        // 3D object params
        { NAME: "showObject", TYPE: "bool",  DEFAULT: true,                        LABEL: "Show 3D Object" },
        { NAME: "objectScale",TYPE: "float", DEFAULT: 1.0,  MIN: 0.1, MAX: 3.0,  LABEL: "Object Scale" },
        { NAME: "useShaderTex",TYPE: "bool", DEFAULT: false,                       LABEL: "Shader as Texture" },
        { NAME: "objectColor",TYPE: "color", DEFAULT: [1.0, 0.85, 0.6, 1.0],     LABEL: "Object Color" },
        { NAME: "transparentBg", TYPE: "bool", DEFAULT: false }
    ];

    // ---- ISF Fragment Shader (metaballs) adapted for Three.js RawShaderMaterial ----
    // Key changes from raw ISF:
    //   - Explicit precision + uniform declarations (no ISF header injection)
    //   - isf_FragNormCoord comes from vUv varying
    //   - Output to gl_FragColor as usual
    const ISF_FRAG = `
precision highp float;

// ISF standard uniforms
uniform float TIME;
uniform vec2 RENDERSIZE;

// ISF shader inputs (from metaballs.fs)
uniform float ballCount;
uniform float ballSpeed;
uniform float smoothK;
uniform vec4 keyColor;
uniform vec4 rimColor;
uniform vec4 bgColor;

// Audio reactive (injected by ShaderClaw)
uniform float audioLevel;
uniform float audioBass;

// Mouse
uniform vec2 mousePos;

varying vec2 vUv;

#define MAX_STEPS 26
#define EPSILON 0.016
#define MAX_BALLS 8

vec3 gBalls[MAX_BALLS];
int gBallCount;

mat3 rotation_matrix(vec2 angles) {
    float cx = cos(angles.x), sx = sin(angles.x);
    float cy = cos(angles.y), sy = sin(angles.y);
    return mat3(
        cy, sy*sx, sy*cx,
        0.0, cx, -sx,
        -sy, cy*sx, cy*cx
    );
}

vec3 metaball_position(float id) {
    float t = id * 88.0 + TIME * ballSpeed;
    return 0.7 * vec3(
        sin(t*1.2) * cos(t*0.82),
        cos(6.0 + t*0.9) * sin(9.0 + t*1.15),
        sin(12.0 + t*0.7) * cos(22.0 + t*1.33)
    );
}

float smooth_min(float a, float b, float k) {
    float h = clamp(0.5 + 0.5*(b-a)/k, 0.0, 1.0);
    return mix(b, a, h) - k*h*(1.0-h);
}

float scene_distance(vec3 p) {
    float dist = 1e6;
    for(int i = 0; i < MAX_BALLS; i++) {
        if(i >= gBallCount) break;
        dist = smooth_min(dist, length(p - 1.2*gBalls[i]), smoothK);
    }
    return dist - 0.24;
}

vec3 calculate_normal(vec3 p) {
    vec2 k = vec2(1.0, -1.0);
    return normalize(
        k.xyy * scene_distance(p + k.xyy*0.0001) +
        k.yyx * scene_distance(p + k.yyx*0.0001) +
        k.yxy * scene_distance(p + k.yxy*0.0001) +
        k.xxx * scene_distance(p + k.xxx*0.0001)
    );
}

vec3 lighting(vec3 p) {
    vec3 n = calculate_normal(p);
    vec3 key_dir = normalize(vec3(0.4, 0.7, -0.3));
    float key = pow(clamp(dot(n, key_dir), 0.0, 1.0), 2.2);
    vec3 fill_dir = normalize(vec3(-0.2, 0.5, 0.1));
    float fill = pow(clamp(dot(n, fill_dir), 0.0, 1.0), 2.0);
    vec3 view_dir = normalize(vec3(0.0, 0.0, -1.0));
    float rim = pow(1.0 - clamp(dot(n, view_dir), 0.0, 1.0), 3.0);
    return key * keyColor.rgb +
           fill * vec3(0.7, 0.7, 0.9) +
           rim * rimColor.rgb;
}

vec3 trace_rays(vec3 origin, vec3 dir) {
    vec3 p = origin;
    for(int i = 0; i < MAX_STEPS; i++) {
        float dist = scene_distance(p);
        if(dist < EPSILON) {
            return lighting(p);
        }
        p += dist * dir;
    }
    return bgColor.rgb * (0.9 - length(p.xy)*0.4);
}

void main() {
    vec2 auto_rot = vec2(sin(TIME * 0.3) * 0.4, cos(TIME * 0.2) * 0.3);
    mat3 rot = rotation_matrix(auto_rot);

    // Use gl_FragCoord for pixel-perfect ISF compatibility
    vec2 uv = (2.0 * gl_FragCoord.xy - RENDERSIZE.xy) / RENDERSIZE.y;

    gBallCount = int(ballCount + 0.5);
    for(int i = 0; i < MAX_BALLS; i++) {
        gBalls[i] = metaball_position(float(i + 1));
    }

    vec3 ray_origin = vec3(0.0, 0.0, -2.2);
    vec3 ray_dir = rot * normalize(vec3(uv * 0.7, 1.0));

    vec3 color = trace_rays(ray_origin, ray_dir);
    gl_FragColor = vec4(1.25 * color, 1.0);
}
`;

    const ISF_VERT = `
precision highp float;
attribute vec2 position;
varying vec2 vUv;
void main() {
    vUv = position * 0.5 + 0.5;
    gl_Position = vec4(position, 0.0, 1.0);
}
`;

    // ---- Fullscreen quad geometry (matches ISF renderer's triangle trick) ----
    function createFullscreenQuad(material) {
        var geom = new THREE.BufferGeometry();
        // Oversized triangle covers entire screen (same as ISF renderer)
        var verts = new Float32Array([-1, -1,  3, -1,  -1, 3]);
        geom.setAttribute('position', new THREE.BufferAttribute(verts, 2));
        var mesh = new THREE.Mesh(geom, material);
        mesh.frustumCulled = false;
        return mesh;
    }

    function create(renderer, canvas, media) {

        // ============================================================
        // LAYER 1: ISF shader rendered to a render target
        // ============================================================
        var rtWidth = 1920, rtHeight = 1080;
        var isfRenderTarget = new THREE.WebGLRenderTarget(rtWidth, rtHeight, {
            minFilter: THREE.LinearFilter,
            magFilter: THREE.LinearFilter,
            format: THREE.RGBAFormat,
            type: THREE.UnsignedByteType
        });

        var isfUniforms = {
            TIME:       { value: 0.0 },
            RENDERSIZE: { value: new THREE.Vector2(rtWidth, rtHeight) },
            ballCount:  { value: 6.0 },
            ballSpeed:  { value: 1.1 },
            smoothK:    { value: 0.68 },
            keyColor:   { value: new THREE.Vector4(0.4, 0.6, 1.0, 1.0) },
            rimColor:   { value: new THREE.Vector4(0.8, 0.3, 0.6, 1.0) },
            bgColor:    { value: new THREE.Vector4(0.4, 0.6, 1.0, 1.0) },
            audioLevel: { value: 0.0 },
            audioBass:  { value: 0.0 },
            mousePos:   { value: new THREE.Vector2(0.5, 0.5) }
        };

        var isfMaterial = new THREE.RawShaderMaterial({
            vertexShader: ISF_VERT,
            fragmentShader: ISF_FRAG,
            uniforms: isfUniforms,
            depthTest: false,
            depthWrite: false
        });

        var isfQuad = createFullscreenQuad(isfMaterial);
        var isfScene = new THREE.Scene();
        isfScene.add(isfQuad);
        var isfCamera = new THREE.OrthographicCamera(-1, 1, 1, -1, 0, 1);

        // ============================================================
        // LAYER 2: 3D scene with object (uses ISF output as background)
        // ============================================================
        var scene3D = new THREE.Scene();
        scene3D.background = null; // transparent — ISF shows through

        var camera3D = new THREE.PerspectiveCamera(50, rtWidth / rtHeight, 0.1, 100);
        camera3D.position.set(0, 0, 4);
        camera3D.lookAt(0, 0, 0);

        // Torus knot as demo 3D object
        var torusGeom = new THREE.TorusKnotGeometry(0.8, 0.25, 128, 32);
        var torusMat = new THREE.MeshStandardMaterial({
            color: 0xffd699,
            metalness: 0.7,
            roughness: 0.2,
            transparent: true,
            opacity: 0.9
        });
        var torusMesh = new THREE.Mesh(torusGeom, torusMat);
        scene3D.add(torusMesh);

        // Lights for 3D object
        var ambientLight = new THREE.AmbientLight(0x404040, 0.5);
        scene3D.add(ambientLight);
        var pointLight = new THREE.PointLight(0xffffff, 1.5, 50);
        pointLight.position.set(3, 3, 3);
        scene3D.add(pointLight);
        var rimLight = new THREE.PointLight(0x8060ff, 1.0, 50);
        rimLight.position.set(-3, -1, 2);
        scene3D.add(rimLight);

        // ============================================================
        // COMPOSITING: Final fullscreen quad blends ISF + 3D
        // ============================================================
        var compositeRT = new THREE.WebGLRenderTarget(rtWidth, rtHeight, {
            minFilter: THREE.LinearFilter,
            magFilter: THREE.LinearFilter,
            format: THREE.RGBAFormat
        });

        // Render 3D scene to its own target
        var scene3DRT = new THREE.WebGLRenderTarget(rtWidth, rtHeight, {
            minFilter: THREE.LinearFilter,
            magFilter: THREE.LinearFilter,
            format: THREE.RGBAFormat
        });

        var compositeFrag = `
precision highp float;
uniform sampler2D isfLayer;
uniform sampler2D sceneLayer;
varying vec2 vUv;
void main() {
    vec4 bg = texture2D(isfLayer, vUv);
    vec4 fg = texture2D(sceneLayer, vUv);
    // Alpha-over compositing: 3D object over ISF background
    gl_FragColor = vec4(mix(bg.rgb, fg.rgb, fg.a), 1.0);
}
`;
        var compositeUniforms = {
            isfLayer:   { value: isfRenderTarget.texture },
            sceneLayer: { value: scene3DRT.texture }
        };

        var compositeMat = new THREE.RawShaderMaterial({
            vertexShader: ISF_VERT,
            fragmentShader: compositeFrag,
            uniforms: compositeUniforms,
            depthTest: false,
            depthWrite: false
        });

        var compositeQuad = createFullscreenQuad(compositeMat);
        var compositeScene = new THREE.Scene();
        compositeScene.add(compositeQuad);

        // ============================================================
        // OUTPUT: This scene that ShaderClaw's SceneRenderer renders
        // We use a "wrapper" scene with the composite quad
        // ============================================================

        return {
            scene: compositeScene,
            camera: isfCamera, // ortho camera for final composite

            update: function(time, values, mediaList) {
                // Update ISF uniforms from ShaderClaw parameters
                isfUniforms.TIME.value = time;
                isfUniforms.ballCount.value = values.ballCount !== undefined ? values.ballCount : 6.0;
                isfUniforms.ballSpeed.value = values.ballSpeed !== undefined ? values.ballSpeed : 1.1;
                isfUniforms.smoothK.value = values.smoothK !== undefined ? values.smoothK : 0.68;

                if (values.keyColor) {
                    var kc = values.keyColor;
                    isfUniforms.keyColor.value.set(kc[0], kc[1], kc[2], kc[3] !== undefined ? kc[3] : 1.0);
                }
                if (values.rimColor) {
                    var rc = values.rimColor;
                    isfUniforms.rimColor.value.set(rc[0], rc[1], rc[2], rc[3] !== undefined ? rc[3] : 1.0);
                }
                if (values.bgColor) {
                    var bc = values.bgColor;
                    isfUniforms.bgColor.value.set(bc[0], bc[1], bc[2], bc[3] !== undefined ? bc[3] : 1.0);
                }

                // Audio injection
                isfUniforms.audioLevel.value = values._audioLevel || 0;
                isfUniforms.audioBass.value = values._audioBass || 0;

                // Mouse
                if (values._mousePos) {
                    isfUniforms.mousePos.value.set(values._mousePos[0], values._mousePos[1]);
                }

                // 3D object updates
                var showObj = values.showObject !== undefined ? values.showObject : true;
                torusMesh.visible = !!showObj;

                var scale = values.objectScale !== undefined ? values.objectScale : 1.0;
                torusMesh.scale.setScalar(scale);

                // Rotate the torus knot
                torusMesh.rotation.x = time * 0.4;
                torusMesh.rotation.y = time * 0.6;

                // Audio-reactive scale pulse
                var audioPulse = 1.0 + (values._audioBass || 0) * 0.3;
                torusMesh.scale.multiplyScalar(audioPulse);

                // Object color
                if (values.objectColor) {
                    var oc = values.objectColor;
                    torusMat.color.setRGB(oc[0], oc[1], oc[2]);
                }

                // If "Shader as Texture" is enabled, use ISF render target as the object's map
                if (values.useShaderTex) {
                    if (torusMat.map !== isfRenderTarget.texture) {
                        torusMat.map = isfRenderTarget.texture;
                        torusMat.needsUpdate = true;
                    }
                } else {
                    if (torusMat.map !== null) {
                        torusMat.map = null;
                        torusMat.needsUpdate = true;
                    }
                }

                // Move light with mouse
                pointLight.position.x = (isfUniforms.mousePos.value.x - 0.5) * 6;
                pointLight.position.y = (isfUniforms.mousePos.value.y - 0.5) * 6;

                // --- Render passes (all within the SAME Three.js renderer) ---

                // Pass 1: Render ISF shader to render target
                renderer.setRenderTarget(isfRenderTarget);
                renderer.setViewport(0, 0, rtWidth, rtHeight);
                renderer.clear();
                renderer.render(isfScene, isfCamera);

                // Pass 2: Render 3D scene to render target
                renderer.setRenderTarget(scene3DRT);
                renderer.setViewport(0, 0, rtWidth, rtHeight);
                renderer.setClearColor(0x000000, 0);
                renderer.clear();
                renderer.render(scene3D, camera3D);

                // Pass 3: Composite will be rendered by SceneRenderer
                // (it renders this.scene with this.camera to the screen)
                renderer.setRenderTarget(null);
            },

            resize: function(w, h) {
                camera3D.aspect = w / h;
                camera3D.updateProjectionMatrix();
                // Resize render targets to match
                if (w !== isfRenderTarget.width || h !== isfRenderTarget.height) {
                    isfRenderTarget.setSize(w, h);
                    scene3DRT.setSize(w, h);
                    isfUniforms.RENDERSIZE.value.set(w, h);
                }
            },

            dispose: function() {
                isfRenderTarget.dispose();
                scene3DRT.dispose();
                isfMaterial.dispose();
                compositeMat.dispose();
                torusGeom.dispose();
                torusMat.dispose();
            }
        };
    }

    return { INPUTS: INPUTS, create: create };

})

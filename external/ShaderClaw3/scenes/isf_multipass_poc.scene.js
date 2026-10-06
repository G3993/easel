// ============================================================
// Phase 2: Multi-Pass ISF Shader in Three.js
//
// Loads fluid_sim.fs (8 passes, 7 persistent ping-pong buffers)
// via ISFThreeRuntime and composites with a 3D object.
// Proves the hardest case: Navier-Stokes simulation running
// entirely through Three.js WebGLRenderTarget chains.
// ============================================================

(function(THREE) {

    const INPUTS = [
        { NAME: "splatForce",    TYPE: "float", DEFAULT: 6000.0, MIN: 500.0,  MAX: 20000.0, LABEL: "Splat Force" },
        { NAME: "splatRadius",   TYPE: "float", DEFAULT: 0.005,  MIN: 0.001,  MAX: 0.05,    LABEL: "Splat Radius" },
        { NAME: "curlStrength",  TYPE: "float", DEFAULT: 30.0,   MIN: 0.0,    MAX: 80.0,    LABEL: "Curl" },
        { NAME: "velDissipation",TYPE: "float", DEFAULT: 0.2,    MIN: 0.0,    MAX: 2.0,     LABEL: "Vel Dissipation" },
        { NAME: "dyeDissipation",TYPE: "float", DEFAULT: 1.0,    MIN: 0.0,    MAX: 5.0,     LABEL: "Dye Dissipation" },
        { NAME: "pressureDecay", TYPE: "float", DEFAULT: 0.8,    MIN: 0.0,    MAX: 1.0,     LABEL: "Pressure Decay" },
        { NAME: "bloomIntensity",TYPE: "float", DEFAULT: 0.8,    MIN: 0.0,    MAX: 3.0,     LABEL: "Bloom" },
        { NAME: "showObject",    TYPE: "bool",  DEFAULT: true,    LABEL: "Show 3D Object" },
        { NAME: "objectScale",   TYPE: "float", DEFAULT: 1.2,    MIN: 0.1,    MAX: 3.0,     LABEL: "Object Scale" },
        { NAME: "transparentBg", TYPE: "bool",  DEFAULT: false }
    ];

    function create(renderer, canvas, media) {

        // ============================================================
        // ISF Runtime: Load fluid_sim.fs via ISFThreeRuntime
        // ============================================================
        var isfRuntime = new ISFThreeRuntime(renderer);
        var loaded = false;

        // Fetch and load the ISF source asynchronously
        fetch('shaders/fluid_sim.fs')
            .then(function(r) { return r.text(); })
            .then(function(src) {
                loaded = isfRuntime.load(src, 1920, 1080);
                if (!loaded) console.error('[Phase2] Failed to load fluid_sim.fs');
                else console.log('[Phase2] fluid_sim.fs loaded — ' + isfRuntime.passes.length + ' passes');
            })
            .catch(function(e) { console.error('[Phase2] fetch error:', e); });

        // ============================================================
        // 3D Scene: Glass sphere refracting the fluid sim
        // ============================================================
        var scene3D = new THREE.Scene();
        scene3D.background = null;

        var camera3D = new THREE.PerspectiveCamera(50, 1920 / 1080, 0.1, 100);
        camera3D.position.set(0, 0, 3.5);
        camera3D.lookAt(0, 0, 0);

        var sphereGeom = new THREE.IcosahedronGeometry(1.0, 4);
        var sphereMat = new THREE.MeshPhysicalMaterial({
            color: 0xffffff,
            metalness: 0.1,
            roughness: 0.05,
            transparent: true,
            opacity: 0.3,
            transmission: 0.9,
            thickness: 1.5,
            ior: 1.5,
        });
        var sphereMesh = new THREE.Mesh(sphereGeom, sphereMat);
        scene3D.add(sphereMesh);

        var ambientLight = new THREE.AmbientLight(0x404060, 0.8);
        scene3D.add(ambientLight);
        var pointLight = new THREE.PointLight(0xffffff, 2.0, 50);
        pointLight.position.set(3, 3, 3);
        scene3D.add(pointLight);

        // Render target for 3D scene
        var scene3DRT = new THREE.WebGLRenderTarget(1920, 1080, {
            minFilter: THREE.LinearFilter,
            magFilter: THREE.LinearFilter,
            format: THREE.RGBAFormat,
        });

        // ============================================================
        // Composite: ISF background + 3D overlay
        // ============================================================
        var compositeVert = ISFThreeRuntime.VERT_SHADER;
        var compositeFrag = [
            'precision highp float;',
            'uniform sampler2D isfLayer;',
            'uniform sampler2D sceneLayer;',
            'varying vec2 vUv;',
            'void main() {',
            '    vec4 bg = texture2D(isfLayer, vUv);',
            '    vec4 fg = texture2D(sceneLayer, vUv);',
            '    gl_FragColor = vec4(mix(bg.rgb, fg.rgb, fg.a), 1.0);',
            '}',
        ].join('\n');

        var compositeUniforms = {
            isfLayer:   { value: null },
            sceneLayer: { value: scene3DRT.texture },
        };

        var compositeMat = new THREE.RawShaderMaterial({
            vertexShader: compositeVert,
            fragmentShader: compositeFrag,
            uniforms: compositeUniforms,
            depthTest: false,
            depthWrite: false,
        });

        var compositeGeom = new THREE.BufferGeometry();
        compositeGeom.setAttribute('position', new THREE.BufferAttribute(
            new Float32Array([-1, -1, 3, -1, -1, 3]), 2
        ));
        var compositeQuad = new THREE.Mesh(compositeGeom, compositeMat);
        compositeQuad.frustumCulled = false;

        var compositeScene = new THREE.Scene();
        compositeScene.add(compositeQuad);
        var compositeCamera = new THREE.OrthographicCamera(-1, 1, 1, -1, 0, 1);

        return {
            scene: compositeScene,
            camera: compositeCamera,

            update: function(time, values, mediaList) {
                if (!loaded) return;

                // Push ISF uniforms
                isfRuntime.update(time, {
                    splatForce:     values.splatForce    || 6000.0,
                    splatRadius:    values.splatRadius   || 0.005,
                    curlStrength:   values.curlStrength  || 30.0,
                    velDissipation: values.velDissipation|| 0.2,
                    dyeDissipation: values.dyeDissipation|| 1.0,
                    pressureDecay:  values.pressureDecay || 0.8,
                    bloomIntensity: values.bloomIntensity|| 0.8,
                    shading:        true,
                    sunrays:        true,
                    sunraysWeight:  1.0,
                    autoSplats:     true,
                    mousePos:   values._mousePos  || [0.5, 0.5],
                    mouseDelta: [0, 0],
                    mouseDown:  0.0,
                    audioLevel: values._audioLevel || 0,
                    audioBass:  values._audioBass  || 0,
                    audioMid:   values._audioMid   || 0,
                    audioHigh:  values._audioHigh  || 0,
                });

                // Render all ISF passes
                isfRuntime.render();

                // Bind ISF output to composite
                compositeUniforms.isfLayer.value = isfRuntime.getOutputTexture();

                // 3D object
                var showObj = values.showObject !== undefined ? values.showObject : true;
                sphereMesh.visible = !!showObj;
                var scale = values.objectScale || 1.2;
                sphereMesh.scale.setScalar(scale);
                sphereMesh.rotation.y = time * 0.3;
                sphereMesh.rotation.x = Math.sin(time * 0.2) * 0.3;

                // Use fluid sim as environment/background texture on the sphere
                if (isfRuntime.getOutputTexture()) {
                    sphereMat.envMap = isfRuntime.getOutputTexture();
                    sphereMat.envMapIntensity = 1.5;
                    sphereMat.needsUpdate = true;
                }

                // Render 3D scene to target
                renderer.setRenderTarget(scene3DRT);
                renderer.setClearColor(0x000000, 0);
                renderer.clear();
                renderer.render(scene3D, camera3D);
                renderer.setRenderTarget(null);
            },

            resize: function(w, h) {
                camera3D.aspect = w / h;
                camera3D.updateProjectionMatrix();
                scene3DRT.setSize(w, h);
                if (isfRuntime) isfRuntime.resize(w, h);
            },

            dispose: function() {
                if (isfRuntime) isfRuntime.dispose();
                scene3DRT.dispose();
                compositeMat.dispose();
                compositeGeom.dispose();
                sphereGeom.dispose();
                sphereMat.dispose();
            }
        };
    }

    return { INPUTS: INPUTS, create: create };

})

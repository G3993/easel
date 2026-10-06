(function(THREE) {
    // ── VERBATIM copy of Three.js webgl_buffergeometry_instancing_billboards ──
    // Only change: wrapped in ShaderClaw scene format + added parameter controls

    var INPUTS = [
        { NAME: "particleCount", TYPE: "long", DEFAULT: 3, VALUES: [0,1,2,3,4], LABELS: ["10K","25K","50K","75K","100K"] },
        { NAME: "speed", TYPE: "float", DEFAULT: 1.0, MIN: 0.0, MAX: 4.0, LABEL: "Speed" },
        { NAME: "rotX", TYPE: "float", DEFAULT: 0.2, MIN: -2.0, MAX: 2.0, LABEL: "Rotate X" },
        { NAME: "rotY", TYPE: "float", DEFAULT: 0.4, MIN: -2.0, MAX: 2.0, LABEL: "Rotate Y" },
        { NAME: "particleScale", TYPE: "float", DEFAULT: 1.0, MIN: 0.1, MAX: 5.0, LABEL: "Particle Size" },
        { NAME: "spread", TYPE: "float", DEFAULT: 500.0, MIN: 50.0, MAX: 2000.0, LABEL: "Spread" },
        { NAME: "texture", TYPE: "image", LABEL: "Particle Texture" },
        { NAME: "movement", TYPE: "float", DEFAULT: 0.6, MIN: 0.0, MAX: 2.0, LABEL: "Camera Drift" },
        { NAME: "transparentBg", TYPE: "bool", DEFAULT: true },
        { NAME: "bgColor", TYPE: "color", DEFAULT: [0.0, 0.0, 0.0, 1.0], LABEL: "Background" }
    ];

    var COUNTS = [10000, 25000, 50000, 75000, 100000];

    // ── EXACT vertex shader from Three.js example ─────────────────
    var vshader = [
        'precision highp float;',
        'uniform mat4 modelViewMatrix;',
        'uniform mat4 projectionMatrix;',
        'uniform float time;',
        'uniform float particleScale;',
        'attribute vec3 position;',
        'attribute vec2 uv;',
        'attribute vec3 translate;',
        'varying vec2 vUv;',
        'varying float vScale;',
        'void main() {',
        '    vec4 mvPosition = modelViewMatrix * vec4( translate, 1.0 );',
        '    vec3 trTime = vec3(translate.x + time,translate.y + time,translate.z + time);',
        '    float scale =  sin( trTime.x * 2.1 ) + sin( trTime.y * 3.2 ) + sin( trTime.z * 4.3 );',
        '    vScale = scale;',
        '    scale = (scale * 10.0 + 10.0) * particleScale;',
        '    mvPosition.xyz += position * scale;',
        '    vUv = uv;',
        '    gl_Position = projectionMatrix * mvPosition;',
        '}'
    ].join('\n');

    // ── EXACT fragment shader from Three.js example ───────────────
    var fshader = [
        'precision highp float;',
        'uniform sampler2D map;',
        'varying vec2 vUv;',
        'varying float vScale;',
        'vec3 HUEtoRGB(float H){',
        '    H = mod(H,1.0);',
        '    float R = abs(H * 6.0 - 3.0) - 1.0;',
        '    float G = 2.0 - abs(H * 6.0 - 2.0);',
        '    float B = 2.0 - abs(H * 6.0 - 4.0);',
        '    return clamp(vec3(R,G,B),0.0,1.0);',
        '}',
        'vec3 HSLtoRGB(vec3 HSL){',
        '    vec3 RGB = HUEtoRGB(HSL.x);',
        '    float C = (1.0 - abs(2.0 * HSL.z - 1.0)) * HSL.y;',
        '    return (RGB - 0.5) * C + HSL.z;',
        '}',
        'void main() {',
        '    vec4 diffuseColor = texture2D( map, vUv );',
        '    gl_FragColor = vec4( diffuseColor.xyz * HSLtoRGB(vec3(vScale/5.0, 1.0, 0.5)), diffuseColor.w );',
        '    if ( diffuseColor.w < 0.5 ) discard;',
        '}'
    ].join('\n');

    function create(renderer, canvas, media) {
        var scene = new THREE.Scene();
        var _bgColor = new THREE.Color(0x000000);
        scene.background = null;

        // ── Camera — same as Three.js example ───────────────────────
        var camera = new THREE.PerspectiveCamera(50, canvas.width / canvas.height, 1, 5000);
        camera.position.z = 1400;

        // ── Load the ACTUAL circle.png from Three.js CDN ────────────
        var defaultTexture = new THREE.TextureLoader().load(
            'https://threejs.org/examples/textures/sprites/circle.png'
        );

        // ── Material — EXACT same setup as Three.js example ─────────
        var material = new THREE.RawShaderMaterial({
            uniforms: {
                'map': { value: defaultTexture },
                'time': { value: 0.0 },
                'particleScale': { value: 1.0 }
            },
            vertexShader: vshader,
            fragmentShader: fshader,
            depthTest: true,
            depthWrite: true
        });

        // ── Geometry — EXACT same setup as Three.js example ─────────
        var mesh = null;
        var currentCount = -1;

        function buildParticles(count) {
            if (mesh) {
                scene.remove(mesh);
                mesh.geometry.dispose();
            }

            // EXACT: CircleGeometry(1, 6) — same as Three.js example
            var circleGeometry = new THREE.CircleGeometry(1, 6);

            // EXACT: InstancedBufferGeometry from circle
            var geometry = new THREE.InstancedBufferGeometry();
            geometry.index = circleGeometry.index;
            geometry.attributes = circleGeometry.attributes;

            // EXACT: random translate in [-1, 1]
            var translateArray = new Float32Array(count * 3);
            for (var i = 0, i3 = 0, l = count; i < l; i++, i3 += 3) {
                translateArray[i3 + 0] = Math.random() * 2 - 1;
                translateArray[i3 + 1] = Math.random() * 2 - 1;
                translateArray[i3 + 2] = Math.random() * 2 - 1;
            }
            geometry.setAttribute('translate', new THREE.InstancedBufferAttribute(translateArray, 3));

            // EXACT: Mesh (NOT Points), scale 500
            mesh = new THREE.Mesh(geometry, material);
            mesh.scale.set(500, 500, 500);
            scene.add(mesh);

            currentCount = count;
        }

        // 75K — same default as Three.js example
        buildParticles(75000);

        var currentTexId = null;

        return {
            scene: scene,
            camera: camera,

            update: function(time, values, mediaList) {
                var spd = (values.speed != null) ? values.speed : 1.0;
                var t = time * spd;

                // EXACT from Three.js example: material.uniforms['time'].value = time;
                material.uniforms['time'].value = t;
                material.uniforms['particleScale'].value = (values.particleScale != null) ? values.particleScale : 1.0;

                // Particle count
                var countIdx = (values.particleCount != null) ? values.particleCount : 3;
                var targetCount = COUNTS[countIdx] || 75000;
                if (targetCount !== currentCount) buildParticles(targetCount);

                // Spread
                var spreadVal = (values.spread != null) ? values.spread : 500.0;
                if (mesh) mesh.scale.set(spreadVal, spreadVal, spreadVal);

                // EXACT from Three.js example: mesh.rotation
                var rx = (values.rotX != null) ? values.rotX : 0.2;
                var ry = (values.rotY != null) ? values.rotY : 0.4;
                if (mesh) {
                    mesh.rotation.x = t * rx;
                    mesh.rotation.y = t * ry;
                }

                // User texture override
                var texId = values.texture;
                if (texId && mediaList) {
                    var m = mediaList.find(function(e) { return String(e.id) === String(texId); });
                    if (m && m.threeTexture && currentTexId !== texId) {
                        material.uniforms['map'].value = m.threeTexture;
                        currentTexId = texId;
                    }
                } else if (!texId && currentTexId) {
                    material.uniforms['map'].value = defaultTexture;
                    currentTexId = null;
                }

                // Background
                var wantTransparent = values.transparentBg != null ? !!values.transparentBg : true;
                if (wantTransparent) {
                    scene.background = null;
                } else {
                    if (values.bgColor) {
                        _bgColor.setRGB(values.bgColor[0], values.bgColor[1], values.bgColor[2]);
                    }
                    scene.background = _bgColor;
                }

                // Camera drift
                var drift = (values.movement != null) ? values.movement : 0.6;
                if (drift > 0.001) {
                    var ot = time * 0.15 * drift;
                    var angle = ot + 0.3 * Math.sin(ot * 0.7);
                    var dist = 1400 + 300 * Math.sin(ot * 0.4) * drift;
                    var camY = 200 * Math.sin(ot * 0.3) * drift;
                    camera.position.set(Math.sin(angle) * dist, camY, Math.cos(angle) * dist);
                    camera.lookAt(0, 0, 0);
                }
            },

            resize: function(w, h) {
                camera.aspect = w / h;
                camera.updateProjectionMatrix();
            },

            dispose: function() {
                if (mesh) mesh.geometry.dispose();
                material.dispose();
                defaultTexture.dispose();
            }
        };
    }

    return { INPUTS: INPUTS, create: create };
})

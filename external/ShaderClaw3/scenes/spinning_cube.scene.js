(function(THREE) {
    const INPUTS = [
        { NAME: "shape", TYPE: "long", DEFAULT: 0, VALUES: [0,1,2,3,4,5,6], LABELS: ["Cube","Sphere","Torus","Cylinder","Cone","Dodecahedron","Custom"] },
        { NAME: "texture", TYPE: "image" },
        { NAME: "cubeColor", TYPE: "color", DEFAULT: [1.0, 1.0, 1.0, 1.0] },
        { NAME: "reflectivity", LABEL: "Mirror", TYPE: "float", DEFAULT: 0.95, MIN: 0.0, MAX: 1.0 },
        { NAME: "strobeSpeed", LABEL: "Strobe", TYPE: "float", DEFAULT: 2.0, MIN: 0.0, MAX: 10.0 },
        { NAME: "strobeColor1", LABEL: "Strobe 1", TYPE: "color", DEFAULT: [0.91, 0.25, 0.34, 1.0] },
        { NAME: "strobeColor2", LABEL: "Strobe 2", TYPE: "color", DEFAULT: [0.2, 0.5, 1.0, 1.0] },
        { NAME: "strobeColor3", LABEL: "Strobe 3", TYPE: "color", DEFAULT: [0.1, 1.0, 0.5, 1.0] },
        { NAME: "transparentBg", TYPE: "bool", DEFAULT: true },
        { NAME: "bgColor", TYPE: "color", DEFAULT: [0.035, 0.035, 0.059, 1.0] },
        { NAME: "floor", TYPE: "bool", DEFAULT: false },
        { NAME: "speed", TYPE: "float", DEFAULT: 1.0, MIN: 0.0, MAX: 5.0 },
        { NAME: "movement", TYPE: "float", DEFAULT: 0.6, MIN: 0.0, MAX: 2.0 },
        { NAME: "rotX", TYPE: "float", DEFAULT: 0.7, MIN: -3.0, MAX: 3.0 },
        { NAME: "rotY", TYPE: "float", DEFAULT: 1.0, MIN: -3.0, MAX: 3.0 },
        { NAME: "rotZ", TYPE: "float", DEFAULT: 0.0, MIN: -3.0, MAX: 3.0 },
        { NAME: "size", TYPE: "float", DEFAULT: 1.0, MIN: 0.2, MAX: 3.0 }
    ];

    function makeGeometry(shapeId) {
        switch (shapeId) {
            case 1: return new THREE.SphereGeometry(0.6, 64, 64);
            case 2: return new THREE.TorusGeometry(0.45, 0.2, 24, 48);
            case 3: return new THREE.CylinderGeometry(0.5, 0.5, 1, 32);
            case 4: return new THREE.ConeGeometry(0.5, 1, 32);
            case 5: return new THREE.DodecahedronGeometry(0.6);
            default: return new THREE.BoxGeometry(1, 1, 1);
        }
    }

    function create(renderer, canvas, media) {
        var scene = new THREE.Scene();
        var _bgColor = new THREE.Color(0x09090f);
        scene.background = null;

        var camera = new THREE.PerspectiveCamera(60, canvas.width / canvas.height, 0.1, 100);
        camera.position.set(0, 1.2, 3.5);
        camera.lookAt(0, 0, 0);

        // Ambient — dim so strobes dominate
        var ambient = new THREE.AmbientLight(0x202030, 0.3);
        scene.add(ambient);

        // Key light — white
        var keyLight = new THREE.DirectionalLight(0xffffff, 0.5);
        keyLight.position.set(3, 4, 2);
        scene.add(keyLight);

        // 3 colored strobe point lights orbiting the cube
        var strobe1 = new THREE.PointLight(0xe84057, 2.0, 8);
        var strobe2 = new THREE.PointLight(0x3388ff, 2.0, 8);
        var strobe3 = new THREE.PointLight(0x1aff80, 2.0, 8);
        scene.add(strobe1);
        scene.add(strobe2);
        scene.add(strobe3);

        // Pivot group
        var pivot = new THREE.Group();
        scene.add(pivot);

        // Mirror material — high metalness, low roughness
        var currentShape = 0;
        var geometry = makeGeometry(0);
        var material = new THREE.MeshStandardMaterial({
            color: new THREE.Color(1.0, 1.0, 1.0),
            roughness: 0.05,
            metalness: 0.95,
            envMapIntensity: 1.5
        });

        // Generate a simple environment map for reflections
        var envScene = new THREE.Scene();
        var envCam = new THREE.CubeCamera(0.1, 100, new THREE.WebGLCubeRenderTarget(128));

        // Fake environment — colored gradient sphere
        var envGeo = new THREE.SphereGeometry(40, 16, 16);
        var envMat = new THREE.MeshBasicMaterial({ side: THREE.BackSide });
        var envMesh = new THREE.Mesh(envGeo, envMat);
        envScene.add(envMesh);

        // Screen-space texture projection
        var _resolution = new THREE.Vector2(1, 1);
        var _texScale = { value: 1.0 };
        var _screenSpaceChunk = [
            '#ifdef USE_MAP',
            '  vec2 screenUV = gl_FragCoord.xy / screenRes;',
            '  screenUV.y = 1.0 - screenUV.y;',
            '  screenUV = fract(screenUV * texScale);',
            '  vec4 texelColor = texture2D( map, screenUV );',
            '  texelColor = mapTexelToLinear( texelColor );',
            '  diffuseColor *= texelColor;',
            '#endif'
        ].join('\n');
        material.onBeforeCompile = function(shader) {
            shader.uniforms.screenRes = { value: _resolution };
            shader.uniforms.texScale = _texScale;
            shader.fragmentShader = 'uniform vec2 screenRes;\nuniform float texScale;\n' + shader.fragmentShader;
            shader.fragmentShader = shader.fragmentShader.replace('#include <map_fragment>', _screenSpaceChunk);
        };

        var mesh = new THREE.Mesh(geometry, material);
        pivot.add(mesh);

        var customModel = null;

        // Reflective floor
        var floorGeo = new THREE.PlaneGeometry(8, 8);
        var floorMat = new THREE.MeshStandardMaterial({
            color: 0x111118,
            roughness: 0.3,
            metalness: 0.8
        });
        var floorMesh = new THREE.Mesh(floorGeo, floorMat);
        floorMesh.rotation.x = -Math.PI / 2;
        floorMesh.position.y = -1.2;
        scene.add(floorMesh);

        var currentTexId = null;
        var lastVideoTime = -1;
        var stallFrames = 0;

        function patchMaterialForScreenSpace(mat) {
            if (mat._screenPatched) return;
            mat._screenPatched = true;
            mat.onBeforeCompile = function(shader) {
                shader.uniforms.screenRes = { value: _resolution };
                shader.uniforms.texScale = _texScale;
                shader.fragmentShader = 'uniform vec2 screenRes;\nuniform float texScale;\n' + shader.fragmentShader;
                shader.fragmentShader = shader.fragmentShader.replace('#include <map_fragment>', _screenSpaceChunk);
            };
        }

        // Initial env map render
        var _envRendered = false;

        return {
            scene: scene,
            camera: camera,
            update: function(time, values, mediaList) {
                var _sz = renderer.getSize(new THREE.Vector2());
                var _dpr = renderer.getPixelRatio();
                _resolution.set(_sz.x * _dpr, _sz.y * _dpr);

                var spd = (values.speed != null) ? values.speed : 1.0;
                var sz = (values.size != null) ? values.size : 1.0;
                var rx = (values.rotX != null) ? values.rotX : 0.7;
                var ry = (values.rotY != null) ? values.rotY : 1.0;
                var rz = (values.rotZ != null) ? values.rotZ : 0.0;
                var stSpd = (values.strobeSpeed != null) ? values.strobeSpeed : 2.0;
                var refl = (values.reflectivity != null) ? values.reflectivity : 0.95;

                _texScale.value = (values.texScale != null) ? values.texScale : 1.0;

                // Mirror material properties
                material.metalness = refl;
                material.roughness = 0.05 + (1.0 - refl) * 0.5;

                // Strobe lights — orbit and pulse
                var st = time * stSpd;
                var pulse1 = Math.max(0, Math.sin(st * 3.14));
                var pulse2 = Math.max(0, Math.sin(st * 3.14 + 2.09));
                var pulse3 = Math.max(0, Math.sin(st * 3.14 + 4.19));

                // Audio reactivity — bass boosts strobe intensity
                var bassBoost = 1.0 + (values._audioBass || 0) * 3.0;

                strobe1.intensity = pulse1 * 3.0 * bassBoost;
                strobe2.intensity = pulse2 * 3.0 * bassBoost;
                strobe3.intensity = pulse3 * 3.0 * bassBoost;

                // Orbit positions
                var r = 2.5;
                strobe1.position.set(Math.cos(st * 0.7) * r, 1.5 + Math.sin(st) * 0.5, Math.sin(st * 0.7) * r);
                strobe2.position.set(Math.cos(st * 0.7 + 2.09) * r, 0.5 + Math.sin(st + 1) * 0.5, Math.sin(st * 0.7 + 2.09) * r);
                strobe3.position.set(Math.cos(st * 0.7 + 4.19) * r, -0.3 + Math.sin(st + 2) * 0.5, Math.sin(st * 0.7 + 4.19) * r);

                // Strobe colors from inputs
                if (values.strobeColor1) {
                    var c = values.strobeColor1;
                    strobe1.color.setRGB(c[0], c[1], c[2]);
                }
                if (values.strobeColor2) {
                    var c = values.strobeColor2;
                    strobe2.color.setRGB(c[0], c[1], c[2]);
                }
                if (values.strobeColor3) {
                    var c = values.strobeColor3;
                    strobe3.color.setRGB(c[0], c[1], c[2]);
                }

                // Transparent background
                var wantTransparent = values.transparentBg != null ? !!values.transparentBg : true;
                if (wantTransparent) {
                    scene.background = null;
                } else {
                    if (!scene.background) scene.background = _bgColor;
                    if (values.bgColor) {
                        var bg = values.bgColor;
                        _bgColor.setRGB(bg[0], bg[1], bg[2]);
                    }
                    scene.background = _bgColor;
                }

                // Floor
                floorMesh.visible = values.floor != null ? !!values.floor : false;

                // Cube color
                if (values.cubeColor) {
                    var cc = values.cubeColor;
                    material.color.setRGB(cc[0], cc[1], cc[2]);
                }

                // Shape switching
                var shapeId = (values.shape != null) ? values.shape : 0;
                if (shapeId !== currentShape) {
                    if (shapeId === 6) {
                        mesh.visible = false;
                        var modelMedia = mediaList && mediaList.find(function(e) { return e.type === 'model' && e.threeModel; });
                        if (modelMedia && modelMedia.threeModel !== (customModel && customModel._sourceModel)) {
                            if (customModel) pivot.remove(customModel);
                            customModel = modelMedia.threeModel.clone();
                            customModel._sourceModel = modelMedia.threeModel;
                            var box = new THREE.Box3().setFromObject(customModel);
                            var center = box.getCenter(new THREE.Vector3());
                            var extent = box.getSize(new THREE.Vector3()).length();
                            var s = extent > 0 ? 2.0 / extent : 1;
                            customModel.scale.multiplyScalar(s);
                            customModel.position.copy(center).multiplyScalar(-s);
                            pivot.add(customModel);
                        }
                    } else {
                        if (customModel) { pivot.remove(customModel); customModel = null; }
                        mesh.visible = true;
                        geometry.dispose();
                        geometry = makeGeometry(shapeId);
                        mesh.geometry = geometry;
                    }
                    currentShape = shapeId;
                }

                // Custom model check
                if (shapeId === 6 && mediaList) {
                    var modelMedia = mediaList.find(function(e) { return e.type === 'model' && e.threeModel; });
                    if (modelMedia && modelMedia.threeModel !== (customModel && customModel._sourceModel)) {
                        if (customModel) pivot.remove(customModel);
                        customModel = modelMedia.threeModel.clone();
                        customModel._sourceModel = modelMedia.threeModel;
                        var box = new THREE.Box3().setFromObject(customModel);
                        var center = box.getCenter(new THREE.Vector3());
                        var extent = box.getSize(new THREE.Vector3()).length();
                        var s = extent > 0 ? 2.0 / extent : 1;
                        customModel.scale.multiplyScalar(s);
                        customModel.position.copy(center).multiplyScalar(-s);
                        pivot.add(customModel);
                        mesh.visible = false;
                    } else if (!modelMedia && customModel) {
                        pivot.remove(customModel);
                        customModel = null;
                        mesh.visible = true;
                    }
                }

                // Rotation
                pivot.rotation.x = time * spd * rx;
                pivot.rotation.y = time * spd * ry;
                pivot.rotation.z = time * spd * rz;
                pivot.scale.setScalar(sz);

                // Camera orbit
                var orbit = (values.movement != null) ? values.movement : 0.6;
                if (orbit > 0.001) {
                    var ot = time * 0.25 * orbit;
                    camera.position.x = Math.sin(ot) * 3.5;
                    camera.position.z = Math.cos(ot) * 3.5;
                    camera.position.y = 1.0 + Math.sin(ot * 0.7) * 0.5;
                    camera.lookAt(0, 0, 0);
                }

                // Render env map once for reflections
                if (!_envRendered) {
                    // Color the env sphere with strobe-like gradient
                    envMat.color.setRGB(0.05, 0.05, 0.1);
                    mesh.visible = false;
                    envCam.position.set(0, 0, 0);
                    envCam.update(renderer, envScene);
                    material.envMap = envCam.renderTarget.texture;
                    mesh.visible = true;
                    _envRendered = true;
                }

                // Texture handling
                var texId = values.texture;
                if (texId && texId !== currentTexId) {
                    var entry = mediaList && mediaList.find(function(e) { return String(e.id) === String(texId); });
                    if (entry) {
                        if (entry.threeTexture) {
                            material.map = entry.threeTexture;
                            material.needsUpdate = true;
                        }
                        currentTexId = texId;
                    }
                } else if (!texId && currentTexId) {
                    material.map = null;
                    material.needsUpdate = true;
                    currentTexId = null;
                }
                // Video texture refresh
                if (currentTexId && material.map && material.map.image) {
                    var vid = material.map.image;
                    if (vid.currentTime !== undefined) {
                        if (vid.currentTime === lastVideoTime) {
                            stallFrames++;
                            if (stallFrames > 10 && vid.paused && vid.loop) vid.play().catch(function(){});
                        } else {
                            stallFrames = 0;
                            material.map.needsUpdate = true;
                        }
                        lastVideoTime = vid.currentTime;
                    }
                }
            },
            dispose: function() {
                geometry.dispose();
                material.dispose();
                floorGeo.dispose();
                floorMat.dispose();
                envGeo.dispose();
                envMat.dispose();
            }
        };
    }

    return { INPUTS: INPUTS, create: create };
})

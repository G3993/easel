(function(THREE) {
    var INPUTS = [
        { NAME: "texture", LABEL: "Texture", TYPE: "image" },
        { NAME: "cubeColor", LABEL: "Color", TYPE: "color", DEFAULT: [1.0, 1.0, 1.0, 1.0] },
        { NAME: "roughness", LABEL: "Roughness", TYPE: "float", DEFAULT: 0.6, MIN: 0.0, MAX: 1.0 },
        { NAME: "floatSpeed", LABEL: "Float Speed", TYPE: "float", DEFAULT: 1.0, MIN: 0.1, MAX: 3.0 },
        { NAME: "spinSpeed", LABEL: "Spin Speed", TYPE: "float", DEFAULT: 1.0, MIN: 0.0, MAX: 5.0 },
        { NAME: "spread", LABEL: "Spread", TYPE: "float", DEFAULT: 1.0, MIN: 0.5, MAX: 3.0 },
        { NAME: "pumpScale", LABEL: "Pump Scale", TYPE: "float", DEFAULT: 1.2, MIN: 0.0, MAX: 3.0 },
        { NAME: "explode", LABEL: "Explode", TYPE: "float", DEFAULT: 2.0, MIN: 0.0, MAX: 5.0 },
        { NAME: "sensitivity", LABEL: "Sensitivity", TYPE: "float", DEFAULT: 2.2, MIN: 1.0, MAX: 5.0 },
        { NAME: "transparentBg", LABEL: "Transparent", TYPE: "bool", DEFAULT: true },
        { NAME: "bgColor", LABEL: "Background", TYPE: "color", DEFAULT: [0.0, 0.0, 0.0, 1.0] }
    ];

    function create(renderer, canvas, media) {
        var scene = new THREE.Scene();
        var _bgColor = new THREE.Color(0x000000);
        scene.background = null;

        var camera = new THREE.PerspectiveCamera(55, canvas.width / canvas.height, 0.01, 500);
        camera.position.set(0, 6, 32);

        // Lighting
        var ambient = new THREE.AmbientLight(0xffffff, 0.35);
        scene.add(ambient);

        var keyLight = new THREE.DirectionalLight(0xffffff, 1.5);
        keyLight.position.set(12, 20, 10);
        scene.add(keyLight);

        var fillLight = new THREE.DirectionalLight(0xd0d8e8, 0.45);
        fillLight.position.set(-8, 5, -5);
        scene.add(fillLight);

        var rimLight = new THREE.DirectionalLight(0xffffff, 0.2);
        rimLight.position.set(0, -10, -15);
        scene.add(rimLight);

        // Materials
        var matLarge = new THREE.MeshStandardMaterial({ color: 0xe8eaed, roughness: 0.6, metalness: 0.05 });
        var matMedium = new THREE.MeshStandardMaterial({ color: 0xdde0e5, roughness: 0.6, metalness: 0.05 });
        var matTiny = new THREE.MeshStandardMaterial({ color: 0xd5d8de, roughness: 0.7, metalness: 0.0 });
        var allMats = [matLarge, matMedium, matTiny];

        // Cubes
        var cubes = [];
        var rng = function(n) { return Math.random() * n - n / 2; };

        // Large core grid
        var gridPos = [
            [-5.5,4,0],[-2,4.5,0],[1.5,4,0.5],[5,4.2,-0.2],
            [-5.8,1.2,0.3],[-2.2,0.8,-0.2],[1.2,1.0,0.1],[5.2,0.9,-0.1],
            [-5.5,-2,0.2],[-2,-2.2,-0.3],[1.5,-1.8,0.2],[5.0,-2.1,0.0],
            [-5.8,-5,-0.1],[-2.2,-5.2,0.1],[1.2,-4.9,-0.2],[5.1,-5.1,0.1]
        ];
        gridPos.forEach(function(pos) {
            var s = 2.4 + Math.random() * 0.8;
            var mesh = new THREE.Mesh(new THREE.BoxGeometry(s, s, s), matLarge);
            mesh.position.set(pos[0] + rng(0.3), pos[1] + rng(0.3), pos[2] + rng(0.4));
            var bp = mesh.position.clone();
            cubes.push({ mesh: mesh, bp: bp, sp: 0.003 + Math.random() * 0.004, off: Math.random() * Math.PI * 2, amp: 0.08 + Math.random() * 0.12, rs: (Math.random() - 0.5) * 0.002, tier: 0 });
            scene.add(mesh);
        });

        // Medium scattered
        for (var i = 0; i < 60; i++) {
            var a = Math.random() * Math.PI * 2, r = 7 + Math.random() * 10;
            var s = 0.25 + Math.random() * 0.9;
            var mesh = new THREE.Mesh(new THREE.BoxGeometry(s, s, s), matMedium);
            mesh.position.set(Math.cos(a) * r * 0.9, Math.sin(a) * r * 0.85 + rng(4), rng(6));
            mesh.rotation.set(rng(Math.PI), rng(Math.PI), rng(Math.PI));
            var bp = mesh.position.clone();
            cubes.push({ mesh: mesh, bp: bp, sp: 0.005 + Math.random() * 0.008, off: Math.random() * Math.PI * 2, amp: 0.15 + Math.random() * 0.3, rs: (Math.random() - 0.5) * 0.005, tier: 1 });
            scene.add(mesh);
        }

        // Tiny particles
        for (var i = 0; i < 200; i++) {
            var a = Math.random() * Math.PI * 2, el = (Math.random() - 0.5) * Math.PI, r = 9 + Math.random() * 18;
            var s = 0.04 + Math.random() * 0.2;
            var mesh = new THREE.Mesh(new THREE.BoxGeometry(s, s, s), matTiny);
            mesh.position.set(
                Math.cos(a) * r * Math.cos(el),
                Math.sin(el) * r * 0.7 + rng(5),
                Math.sin(a) * r * Math.cos(el) * 0.5 + rng(4)
            );
            mesh.rotation.set(rng(Math.PI), rng(Math.PI), rng(Math.PI));
            var bp = mesh.position.clone();
            cubes.push({ mesh: mesh, bp: bp, sp: 0.006 + Math.random() * 0.01, off: Math.random() * Math.PI * 2, amp: 0.2 + Math.random() * 0.5, rs: (Math.random() - 0.5) * 0.009, tier: 2 });
            scene.add(mesh);
        }

        // Beat detection state
        var beatHistory = new Float32Array(43).fill(0);
        var beatPtr = 0;
        var beatFlash = 0;
        var beatPulse = 0;

        var currentTexId = null;
        var lastVideoTime = -1;
        var stallFrames = 0;
        var orbitAngle = 0;

        return {
            scene: scene,
            camera: camera,
            update: function(time, values, mediaList) {
                // Params
                var sens = (values.sensitivity != null) ? values.sensitivity : 2.2;
                var pump = (values.pumpScale != null) ? values.pumpScale : 1.2;
                var expl = (values.explode != null) ? values.explode : 2.0;
                var rough = (values.roughness != null) ? values.roughness : 0.6;
                var fs = (values.floatSpeed != null) ? values.floatSpeed : 1.0;
                var ss = (values.spinSpeed != null) ? values.spinSpeed : 1.0;
                var sp = (values.spread != null) ? values.spread : 1.0;

                // Update roughness
                allMats.forEach(function(m) { m.roughness = rough; });

                // Color
                if (values.cubeColor) {
                    var cc = values.cubeColor;
                    allMats.forEach(function(m) { m.color.setRGB(cc[0], cc[1], cc[2]); });
                }

                // Background
                var wantTransparent = values.transparentBg != null ? !!values.transparentBg : true;
                if (wantTransparent) {
                    scene.background = null;
                } else {
                    if (values.bgColor) {
                        var bg = values.bgColor;
                        _bgColor.setRGB(bg[0], bg[1], bg[2]);
                    }
                    scene.background = _bgColor;
                }

                // Beat detection from ShaderClaw audio uniforms
                var bass = values._audioBass || 0;
                var mid = values._audioMid || 0;
                var energy = bass * 0.65 + mid * 0.35;

                // Rolling average for onset detection
                beatHistory[beatPtr % beatHistory.length] = bass;
                beatPtr++;
                var avg = 0;
                for (var i = 0; i < beatHistory.length; i++) avg += beatHistory[i];
                avg /= beatHistory.length;

                var threshold = avg * sens;
                var isBeat = bass > threshold && bass > 0.1;
                if (isBeat) beatFlash = 1.0;
                beatFlash = Math.max(0, beatFlash - 0.06);

                // Smooth beat pulse
                var targetPulse = isBeat ? 1.0 : bass * sens * 0.5;
                beatPulse += (targetPulse - beatPulse) * 0.18;

                var pumpMag = beatPulse * pump;
                var explodeMag = beatPulse * expl;

                // Camera orbit
                orbitAngle += 0.003;
                var camDist = 32;
                camera.position.set(
                    Math.sin(orbitAngle) * camDist * 0.3,
                    6 + Math.sin(orbitAngle * 0.7) * 2,
                    Math.cos(orbitAngle) * camDist
                );
                camera.lookAt(0, 0, 0);

                // Animate cubes
                cubes.forEach(function(c) {
                    var wave = Math.sin(time * c.sp * 60 * fs + c.off);
                    var waveS = Math.sin(time * c.sp * 40 * fs + c.off + 1);

                    if (c.tier === 0) {
                        c.mesh.position.y = c.bp.y + wave * c.amp * sp;
                        c.mesh.position.x = c.bp.x + waveS * c.amp * 0.4 * sp;
                        c.mesh.rotation.x += c.rs * ss * (1 + pumpMag * 0.5);
                        c.mesh.rotation.y += c.rs * 0.7 * ss * (1 + pumpMag * 0.5);
                        var scl = 1 + pumpMag * 0.18 * pump;
                        c.mesh.scale.setScalar(scl);
                    } else if (c.tier === 1) {
                        c.mesh.position.y = c.bp.y + wave * c.amp * sp * (1 + explodeMag * 0.2);
                        c.mesh.position.x = c.bp.x + Math.cos(time * c.sp * 35 * fs + c.off) * c.amp * 0.4 * sp;
                        c.mesh.position.z = c.bp.z + Math.sin(time * c.sp * 25 * fs + c.off * 2) * c.amp * 0.3 * sp;
                        c.mesh.rotation.x += c.rs * ss * (1 + pumpMag);
                        c.mesh.rotation.y += c.rs * 1.3 * ss * (1 + pumpMag);
                        c.mesh.rotation.z += c.rs * 0.6 * ss;
                        if (explodeMag > 0.1) {
                            var dir = c.bp.clone().normalize();
                            c.mesh.position.addScaledVector(dir, explodeMag * 0.15);
                        }
                    } else {
                        c.mesh.position.y = c.bp.y + wave * c.amp * sp * (1 + explodeMag * 0.4);
                        c.mesh.position.x = c.bp.x + Math.cos(time * c.sp * 28 * fs + c.off) * c.amp * 0.6 * sp;
                        c.mesh.position.z = c.bp.z + Math.sin(time * c.sp * 32 * fs + c.off * 1.5) * c.amp * 0.4 * sp;
                        c.mesh.rotation.x += c.rs * 2 * ss * (1 + pumpMag * 1.5);
                        c.mesh.rotation.y += c.rs * 1.8 * ss * (1 + pumpMag * 1.5);
                        c.mesh.rotation.z += c.rs * ss;
                        if (explodeMag > 0.3) {
                            var dir = c.bp.clone().normalize();
                            c.mesh.position.addScaledVector(dir, explodeMag * 0.5);
                        }
                    }
                });

                // Texture handling
                var texId = values.texture;
                if (texId && texId !== currentTexId) {
                    var entry = mediaList && mediaList.find(function(e) { return String(e.id) === String(texId); });
                    if (entry && entry.threeTexture) {
                        allMats.forEach(function(m) {
                            m.map = entry.threeTexture;
                            m.needsUpdate = true;
                        });
                        currentTexId = texId;
                    }
                } else if (!texId && currentTexId) {
                    allMats.forEach(function(m) {
                        m.map = null;
                        m.needsUpdate = true;
                    });
                    currentTexId = null;
                }
                // Video texture refresh
                if (currentTexId && matLarge.map && matLarge.map.image) {
                    var vid = matLarge.map.image;
                    if (vid.currentTime !== undefined) {
                        if (vid.currentTime === lastVideoTime) {
                            stallFrames++;
                            if (stallFrames > 10 && vid.paused && vid.loop) vid.play().catch(function(){});
                        } else {
                            stallFrames = 0;
                            allMats.forEach(function(m) { if (m.map) m.map.needsUpdate = true; });
                        }
                        lastVideoTime = vid.currentTime;
                    }
                }
            },
            dispose: function() {
                cubes.forEach(function(c) { c.mesh.geometry.dispose(); });
                allMats.forEach(function(m) { m.dispose(); });
            }
        };
    }

    return { INPUTS: INPUTS, create: create };
})

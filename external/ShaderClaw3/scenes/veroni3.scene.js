(function(THREE) {
    const INPUTS = [
        { NAME: "texture", LABEL: "Image", TYPE: "image" },
        { NAME: "panelColor", LABEL: "Color", TYPE: "color", DEFAULT: [0.2, 0.6, 0.9, 1.0] },
        { NAME: "neonColor", LABEL: "Neon", TYPE: "color", DEFAULT: [0.91, 0.25, 0.34, 1.0] },
        { NAME: "cells", LABEL: "Cells", TYPE: "float", DEFAULT: 1, MIN: 0, MAX: 3 },
        { NAME: "neonStr", LABEL: "Neon Glow", TYPE: "float", DEFAULT: 1.5, MIN: 0.0, MAX: 4.0 },
        { NAME: "glassOpacity", LABEL: "Glass", TYPE: "float", DEFAULT: 0.6, MIN: 0.1, MAX: 1.0 },
        { NAME: "morphAmt", LABEL: "Morph", TYPE: "float", DEFAULT: 0.4, MIN: 0.0, MAX: 1.5 },
        { NAME: "speed", LABEL: "Speed", TYPE: "float", DEFAULT: 0.6, MIN: 0.0, MAX: 3.0 },
        { NAME: "rotX", LABEL: "Rot X", TYPE: "float", DEFAULT: 0.3, MIN: -3.0, MAX: 3.0 },
        { NAME: "rotY", LABEL: "Rot Y", TYPE: "float", DEFAULT: 0.5, MIN: -3.0, MAX: 3.0 },
        { NAME: "rotZ", LABEL: "Rot Z", TYPE: "float", DEFAULT: 0.0, MIN: -3.0, MAX: 3.0 },
        { NAME: "size", LABEL: "Size", TYPE: "float", DEFAULT: 1.0, MIN: 0.2, MAX: 3.0 },
        { NAME: "transparentBg", TYPE: "bool", DEFAULT: true },
        { NAME: "bgColor", TYPE: "color", DEFAULT: [0.02, 0.02, 0.04, 1.0] }
    ];

    var RADIUS = 1.5;

    function create(renderer, canvas, media) {
        var scene = new THREE.Scene();
        var _bgColor = new THREE.Color(0.02, 0.02, 0.04);
        scene.background = null;

        var camera = new THREE.PerspectiveCamera(50, canvas.width / canvas.height, 0.1, 100);
        camera.position.set(0, 0, 4.5);
        camera.lookAt(0, 0, 0);

        // Lights
        scene.add(new THREE.AmbientLight(0x303050, 0.5));
        var keyLight = new THREE.DirectionalLight(0xffffff, 0.8);
        keyLight.position.set(3, 4, 5);
        scene.add(keyLight);
        var fillLight = new THREE.DirectionalLight(0x6688cc, 0.3);
        fillLight.position.set(-3, 1, -2);
        scene.add(fillLight);
        var rimLight = new THREE.PointLight(0xffffff, 0.4, 10);
        rimLight.position.set(0, -2, 3);
        scene.add(rimLight);

        var pivot = new THREE.Group();
        scene.add(pivot);

        // State
        var currentDetail = 1;
        var crystalMesh = null;
        var crystalMat = null;
        var edgeLines = null;
        var edgeLineMat = null;
        var glowLines = null;
        var glowLineMat = null;
        var basePositions = null;       // Float32Array of crystal mesh base positions
        var baseEdgePositions = null;   // Float32Array of edge line base positions
        var edgeVertexMap = null;       // maps each edge vertex index to {faceIdx, baryWeights, vertIndices}

        function buildScene(detail) {
            // Clear old
            for (var i = pivot.children.length - 1; i >= 0; i--) {
                var child = pivot.children[i];
                pivot.remove(child);
                if (child.geometry) child.geometry.dispose();
                if (child.material) child.material.dispose();
            }

            // Create icosahedron and convert to non-indexed for flat shading with per-face colors
            var icoGeo = new THREE.IcosahedronGeometry(RADIUS, detail);
            if (icoGeo.index) {
                icoGeo = icoGeo.toNonIndexed();
            }
            icoGeo.computeVertexNormals();

            // Per-face vertex colors using golden ratio hue distribution
            var posArray = icoGeo.attributes.position.array;
            var vertCount = posArray.length / 3;
            var faceCount = vertCount / 3;
            var colors = new Float32Array(vertCount * 3);

            for (var f = 0; f < faceCount; f++) {
                var hue = (f * 0.618033988749) % 1.0;
                var col = new THREE.Color().setHSL(hue, 0.5, 0.5);
                for (var v = 0; v < 3; v++) {
                    var idx = (f * 3 + v) * 3;
                    colors[idx] = col.r;
                    colors[idx + 1] = col.g;
                    colors[idx + 2] = col.b;
                }
            }
            icoGeo.setAttribute('color', new THREE.Float32BufferAttribute(colors, 3));

            // Store base positions for morphing
            basePositions = new Float32Array(posArray);

            // Crystal mesh material
            crystalMat = new THREE.MeshPhongMaterial({
                vertexColors: true,
                flatShading: true,
                transparent: true,
                opacity: 0.6,
                shininess: 120,
                specular: new THREE.Color(0xffffff),
                side: THREE.DoubleSide,
                reflectivity: 0.5
            });
            crystalMesh = new THREE.Mesh(icoGeo, crystalMat);
            pivot.add(crystalMesh);

            // Edge geometry from the SAME icosahedron — guarantees perfect alignment
            var edgeGeo = new THREE.EdgesGeometry(icoGeo, 1); // thresholdAngle=1 to catch all edges

            // Store base edge positions for morphing
            baseEdgePositions = new Float32Array(edgeGeo.attributes.position.array);

            // Build vertex mapping: for each edge vertex, find which crystal face vertex it matches
            // so we can apply the same morph displacement
            edgeVertexMap = buildEdgeVertexMap(basePositions, baseEdgePositions);

            // Neon edge lines
            edgeLineMat = new THREE.LineBasicMaterial({
                color: 0xff4466,
                linewidth: 1,
                transparent: true,
                opacity: 0.9
            });
            edgeLines = new THREE.LineSegments(edgeGeo, edgeLineMat);
            pivot.add(edgeLines);

            // Glow edge lines (duplicate for bloom effect)
            var glowGeo = edgeGeo.clone();
            glowLineMat = new THREE.LineBasicMaterial({
                color: 0xff4466,
                linewidth: 1,
                transparent: true,
                opacity: 0.35
            });
            glowLines = new THREE.LineSegments(glowGeo, glowLineMat);
            glowLines.scale.setScalar(1.002); // slightly larger for glow halo
            pivot.add(glowLines);
        }

        // Map each edge vertex to the nearest crystal mesh vertex index
        // so morph displacement can be copied over
        function buildEdgeVertexMap(crystalPos, edgePos) {
            var map = [];
            var edgeVertCount = edgePos.length / 3;
            var crystalVertCount = crystalPos.length / 3;

            for (var ei = 0; ei < edgeVertCount; ei++) {
                var ex = edgePos[ei * 3];
                var ey = edgePos[ei * 3 + 1];
                var ez = edgePos[ei * 3 + 2];

                var bestDist = Infinity;
                var bestIdx = 0;

                for (var ci = 0; ci < crystalVertCount; ci++) {
                    var dx = crystalPos[ci * 3] - ex;
                    var dy = crystalPos[ci * 3 + 1] - ey;
                    var dz = crystalPos[ci * 3 + 2] - ez;
                    var d = dx * dx + dy * dy + dz * dz;
                    if (d < bestDist) {
                        bestDist = d;
                        bestIdx = ci;
                        if (d < 1e-10) break; // exact match
                    }
                }
                map.push(bestIdx);
            }
            return map;
        }

        buildScene(currentDetail);

        return {
            scene: scene,
            camera: camera,
            update: function(time, values, mediaList) {
                var spd = values.speed != null ? values.speed : 0.6;
                var sz = values.size != null ? values.size : 1.0;
                var rx = values.rotX != null ? values.rotX : 0.3;
                var ry = values.rotY != null ? values.rotY : 0.5;
                var rz = values.rotZ != null ? values.rotZ : 0.0;
                var morph = values.morphAmt != null ? values.morphAmt : 0.4;
                var neon = values.neonStr != null ? values.neonStr : 1.5;
                var glassOp = values.glassOpacity != null ? values.glassOpacity : 0.6;

                // Transparent bg
                var wantTransparent = values.transparentBg != null ? !!values.transparentBg : true;
                if (wantTransparent) {
                    scene.background = null;
                } else {
                    if (values.bgColor) _bgColor.setRGB(values.bgColor[0], values.bgColor[1], values.bgColor[2]);
                    scene.background = _bgColor;
                }

                // Rebuild if detail level changed
                var newDetail = Math.round(values.cells != null ? values.cells : 1);
                newDetail = Math.max(0, Math.min(3, newDetail));
                if (newDetail !== currentDetail) {
                    currentDetail = newDetail;
                    buildScene(newDetail);
                }

                // Rotation
                pivot.rotation.x = time * spd * rx;
                pivot.rotation.y = time * spd * ry;
                pivot.rotation.z = time * spd * rz;
                pivot.scale.setScalar(sz);

                // Colors from user
                var pc = values.panelColor || [0.2, 0.6, 0.9, 1.0];
                var nc = values.neonColor || [0.91, 0.25, 0.34, 1.0];
                var userCol = new THREE.Color(pc[0], pc[1], pc[2]);
                var neonBase = new THREE.Color(nc[0], nc[1], nc[2]);

                // Update panel vertex colors to blend with user color
                if (crystalMesh) {
                    var colAttr = crystalMesh.geometry.attributes.color;
                    var faceCount = colAttr.count / 3;
                    for (var f = 0; f < faceCount; f++) {
                        var hue = (f * 0.618033988749) % 1.0;
                        var baseCol = new THREE.Color().setHSL(hue, 0.5, 0.45);
                        baseCol.lerp(userCol, 0.6);
                        for (var v = 0; v < 3; v++) {
                            var idx = f * 3 + v;
                            colAttr.setXYZ(idx, baseCol.r, baseCol.g, baseCol.b);
                        }
                    }
                    colAttr.needsUpdate = true;

                    crystalMat.opacity = glassOp;

                    // Texture handling
                    var texMedia = null;
                    if (mediaList) {
                        var selectedId = values.texture;
                        if (selectedId) {
                            for (var mi = 0; mi < mediaList.length; mi++) {
                                if (String(mediaList[mi].id) === String(selectedId) && mediaList[mi].threeTexture) {
                                    texMedia = mediaList[mi];
                                    break;
                                }
                            }
                        }
                        if (!texMedia) {
                            for (var mi = 0; mi < mediaList.length; mi++) {
                                if ((mediaList[mi].type === 'image' || mediaList[mi].type === 'video') && mediaList[mi].threeTexture) {
                                    texMedia = mediaList[mi];
                                    break;
                                }
                            }
                        }
                    }
                    if (texMedia && texMedia.threeTexture) {
                        if (crystalMat.map !== texMedia.threeTexture) {
                            crystalMat.map = texMedia.threeTexture;
                            crystalMat.needsUpdate = true;
                        }
                        if (texMedia.threeTexture.image && texMedia.threeTexture.image.videoWidth) {
                            texMedia.threeTexture.needsUpdate = true;
                        }
                    } else if (crystalMat.map) {
                        crystalMat.map = null;
                        crystalMat.needsUpdate = true;
                    }
                }

                // Morph: displace crystal vertices along radial direction
                if (crystalMesh && basePositions) {
                    var posAttr = crystalMesh.geometry.attributes.position;
                    var t = time * spd;

                    for (var vi = 0; vi < posAttr.count; vi++) {
                        var bx = basePositions[vi * 3];
                        var by = basePositions[vi * 3 + 1];
                        var bz = basePositions[vi * 3 + 2];

                        var len = Math.sqrt(bx * bx + by * by + bz * bz);
                        if (len < 0.001) continue;
                        var nx = bx / len;
                        var ny = by / len;
                        var nz = bz / len;

                        // Face index for per-face phase offset
                        var fi = Math.floor(vi / 3);

                        var wave = Math.sin(bx * 3.0 + t * 1.5 + fi * 0.7)
                                 * Math.cos(by * 2.5 + t * 1.2 + fi * 1.3)
                                 * morph * 0.08;
                        var breathe = Math.sin(t * 0.8 + fi * 0.5) * morph * 0.03;
                        var disp = wave + breathe;

                        posAttr.setXYZ(vi, bx + nx * disp, by + ny * disp, bz + nz * disp);
                    }
                    posAttr.needsUpdate = true;

                    // Morph edge lines to track crystal faces
                    if (edgeLines && baseEdgePositions && edgeVertexMap) {
                        var edgePosAttr = edgeLines.geometry.attributes.position;
                        var crystalPosAttr = crystalMesh.geometry.attributes.position;

                        for (var ei = 0; ei < edgePosAttr.count; ei++) {
                            var ci = edgeVertexMap[ei];
                            edgePosAttr.setXYZ(ei,
                                crystalPosAttr.getX(ci),
                                crystalPosAttr.getY(ci),
                                crystalPosAttr.getZ(ci)
                            );
                        }
                        edgePosAttr.needsUpdate = true;
                    }

                    // Morph glow lines too
                    if (glowLines && baseEdgePositions && edgeVertexMap) {
                        var glowPosAttr = glowLines.geometry.attributes.position;
                        var crystalPosAttr = crystalMesh.geometry.attributes.position;

                        for (var ei = 0; ei < glowPosAttr.count; ei++) {
                            var ci = edgeVertexMap[ei];
                            var cx = crystalPosAttr.getX(ci);
                            var cy = crystalPosAttr.getY(ci);
                            var cz = crystalPosAttr.getZ(ci);
                            // Slight outward push for glow
                            var gl = Math.sqrt(cx * cx + cy * cy + cz * cz);
                            if (gl > 0.001) {
                                var s = 1.002;
                                glowPosAttr.setXYZ(ei, cx * s, cy * s, cz * s);
                            } else {
                                glowPosAttr.setXYZ(ei, cx, cy, cz);
                            }
                        }
                        glowPosAttr.needsUpdate = true;
                    }
                }

                // Neon edge color + glow
                if (edgeLineMat) {
                    edgeLineMat.color.copy(neonBase);
                    edgeLineMat.opacity = Math.min(1.0, neon * 0.6);
                }
                if (glowLineMat) {
                    glowLineMat.color.copy(neonBase);
                    glowLineMat.opacity = Math.min(1.0, neon * 0.25);
                }
            },
            resize: function(w, h) {
                camera.aspect = w / h;
                camera.updateProjectionMatrix();
            },
            dispose: function() {
                for (var i = pivot.children.length - 1; i >= 0; i--) {
                    var child = pivot.children[i];
                    if (child.geometry) child.geometry.dispose();
                    if (child.material) child.material.dispose();
                }
            }
        };
    }

    return { INPUTS: INPUTS, create: create };
})

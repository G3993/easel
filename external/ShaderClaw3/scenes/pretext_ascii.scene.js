(function(THREE) {
    // Pretext Variable Typographic ASCII
    // Renders user text in a grid, displaced by orbiting attractor shapes.
    // Brightness field from particles drives variable font weight.

    var LOREM = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum. ";

    var INPUTS = [
        { NAME: "text", LABEL: "Text", TYPE: "text", DEFAULT: LOREM },
        { NAME: "cols", LABEL: "Columns", TYPE: "float", DEFAULT: 80, MIN: 20, MAX: 160 },
        { NAME: "fontSize", LABEL: "Font Size", TYPE: "float", DEFAULT: 14, MIN: 6, MAX: 40 },
        { NAME: "shapeCount", LABEL: "Shapes", TYPE: "float", DEFAULT: 4, MIN: 1, MAX: 8 },
        { NAME: "shapeSize", LABEL: "Shape Size", TYPE: "float", DEFAULT: 0.15, MIN: 0.02, MAX: 0.5 },
        { NAME: "displaceStrength", LABEL: "Displace", TYPE: "float", DEFAULT: 2.5, MIN: 0.0, MAX: 10.0 },
        { NAME: "orbitSpeed", LABEL: "Orbit Speed", TYPE: "float", DEFAULT: 0.5, MIN: 0.05, MAX: 3.0 },
        { NAME: "particleCount", LABEL: "Particles", TYPE: "float", DEFAULT: 80, MIN: 0, MAX: 300 },
        { NAME: "particleForce", LABEL: "Particle Force", TYPE: "float", DEFAULT: 0.22, MIN: 0.01, MAX: 1.0 },
        { NAME: "fieldDecay", LABEL: "Field Decay", TYPE: "float", DEFAULT: 0.85, MIN: 0.5, MAX: 0.99 },
        { NAME: "fontFamily", LABEL: "Font", TYPE: "text", DEFAULT: "Georgia" },
        { NAME: "fgColor", LABEL: "Text Color", TYPE: "color", DEFAULT: [1.0, 1.0, 1.0, 1.0] },
        { NAME: "bgColor", LABEL: "Background", TYPE: "color", DEFAULT: [0.0, 0.0, 0.0, 1.0] },
        { NAME: "showShapes", LABEL: "Show Shapes", TYPE: "bool", DEFAULT: false },
        { NAME: "transparentBg", LABEL: "Transparent", TYPE: "bool", DEFAULT: true }
    ];

    var FIELD_OVERSAMPLE = 2;

    // === Brightness field helpers ===
    function spriteAlphaAt(d) {
        if (d >= 1) return 0;
        if (d <= 0.35) return 0.45 + (0.15 - 0.45) * (d / 0.35);
        return 0.15 * (1 - (d - 0.35) / 0.65);
    }

    function createFieldStamp(radiusPx, scaleX, scaleY) {
        var frx = radiusPx * scaleX;
        var fry = radiusPx * scaleY;
        var rx = Math.ceil(frx);
        var ry = Math.ceil(fry);
        var sx = rx * 2 + 1;
        var sy = ry * 2 + 1;
        var vals = new Float32Array(sx * sy);
        for (var y = -ry; y <= ry; y++) {
            for (var x = -rx; x <= rx; x++) {
                var nd = Math.sqrt((x / frx) * (x / frx) + (y / fry) * (y / fry));
                vals[(y + ry) * sx + x + rx] = spriteAlphaAt(nd);
            }
        }
        return { radiusX: rx, radiusY: ry, sizeX: sx, sizeY: sy, values: vals };
    }

    function splatStamp(field, fieldCols, fieldRows, cx, cy, stamp) {
        var gcx = Math.round(cx);
        var gcy = Math.round(cy);
        for (var y = -stamp.radiusY; y <= stamp.radiusY; y++) {
            var gy = gcy + y;
            if (gy < 0 || gy >= fieldRows) continue;
            var fro = gy * fieldCols;
            var sro = (y + stamp.radiusY) * stamp.sizeX;
            for (var x = -stamp.radiusX; x <= stamp.radiusX; x++) {
                var gx = gcx + x;
                if (gx < 0 || gx >= fieldCols) continue;
                var sv = stamp.values[sro + x + stamp.radiusX];
                if (sv === 0) continue;
                var fi = fro + gx;
                field[fi] = Math.min(1, field[fi] + sv);
            }
        }
    }

    // === Scene creation ===
    function create(renderer, canvas, media) {
        var scene = new THREE.Scene();
        scene.background = null;

        var camera = new THREE.OrthographicCamera(-0.5, 0.5, 0.5, -0.5, 0.1, 10);
        camera.position.z = 1;

        var textCanvas = document.createElement('canvas');
        textCanvas.width = 1920;
        textCanvas.height = 1080;
        var tCtx = textCanvas.getContext('2d');

        var texture = new THREE.CanvasTexture(textCanvas);
        texture.minFilter = THREE.LinearFilter;
        texture.magFilter = THREE.LinearFilter;

        var planeMat = new THREE.MeshBasicMaterial({
            map: texture,
            transparent: true,
            side: THREE.DoubleSide
        });
        var planeGeom = new THREE.PlaneGeometry(1, 1);
        var plane = new THREE.Mesh(planeGeom, planeMat);
        scene.add(plane);

        // State
        var particles = [];
        var brightnessField = null;
        var fieldCols = 0;
        var fieldRows = 0;
        var particleStamp = null;
        var shapeStamp = null;
        var lastFontFamily = '';
        var lastFontSize = 0;
        var lastCols = 0;

        function initParticles(n, w, h) {
            particles = [];
            for (var i = 0; i < n; i++) {
                var angle = Math.random() * Math.PI * 2;
                var radius = Math.random() * Math.min(w, h) * 0.3 + 20;
                particles.push({
                    x: w / 2 + Math.cos(angle) * radius,
                    y: h / 2 + Math.sin(angle) * radius,
                    vx: (Math.random() - 0.5) * 1.2,
                    vy: (Math.random() - 0.5) * 1.2
                });
            }
        }

        return {
            scene: scene,
            camera: camera,
            update: function(time, values, mediaList) {
                var cols = Math.round(values.cols || 80);
                var fSize = Math.round(values.fontSize || 14);
                var fontFamily = values.fontFamily || 'Georgia';
                var nShapes = Math.round(values.shapeCount || 4);
                var shapeSize = values.shapeSize != null ? values.shapeSize : 0.15;
                var displaceStr = values.displaceStrength != null ? values.displaceStrength : 2.5;
                var oSpeed = values.orbitSpeed != null ? values.orbitSpeed : 0.5;
                var nParticles = Math.round(values.particleCount || 80);
                var pForce = values.particleForce != null ? values.particleForce : 0.22;
                var decay = values.fieldDecay != null ? values.fieldDecay : 0.85;
                var showShapes = !!values.showShapes;
                var wantTransparent = values.transparentBg != null ? !!values.transparentBg : true;

                var userText = values.text || LOREM;
                // Repeat text to fill grid
                var textChars = userText.split('');

                var fg = values.fgColor || [1, 1, 1, 1];
                var bg = values.bgColor || [0, 0, 0, 1];
                var fgCSS = 'rgb(' + Math.round(fg[0]*255) + ',' + Math.round(fg[1]*255) + ',' + Math.round(fg[2]*255) + ')';
                var bgCSS = 'rgb(' + Math.round(bg[0]*255) + ',' + Math.round(bg[1]*255) + ',' + Math.round(bg[2]*255) + ')';

                var cw = textCanvas.width;
                var ch = textCanvas.height;
                var cellW = cw / cols;
                var rows = Math.floor(ch / (fSize * 1.3));
                var cellH = ch / rows;

                // Re-init field if grid changed
                var fCols = cols * FIELD_OVERSAMPLE;
                var fRows = rows * FIELD_OVERSAMPLE;
                if (fCols !== fieldCols || fRows !== fieldRows) {
                    fieldCols = fCols;
                    fieldRows = fRows;
                    brightnessField = new Float32Array(fieldCols * fieldRows);
                    var fsx = fieldCols / cw;
                    var fsy = fieldRows / ch;
                    particleStamp = createFieldStamp(12, fsx, fsy);
                    shapeStamp = createFieldStamp(30, fsx, fsy);
                }

                // Re-init particles if count changed
                if (particles.length !== nParticles) {
                    initParticles(nParticles, cw, ch);
                }

                // === Shape positions (orbiting attractors) ===
                var shapes = [];
                for (var s = 0; s < nShapes; s++) {
                    var phase = (s / nShapes) * Math.PI * 2;
                    var orbitRx = cw * 0.3 * (0.6 + 0.4 * Math.sin(phase * 1.3));
                    var orbitRy = ch * 0.3 * (0.6 + 0.4 * Math.cos(phase * 0.9));
                    var t = time * oSpeed;
                    var sx = cw / 2 + Math.cos(t * (0.7 + s * 0.23) + phase) * orbitRx;
                    var sy = ch / 2 + Math.sin(t * (0.5 + s * 0.19) + phase) * orbitRy;
                    var sr = Math.min(cw, ch) * shapeSize * (0.7 + 0.3 * Math.sin(t + s));
                    shapes.push({ x: sx, y: sy, r: sr });
                }

                // === Update particles (attracted to shapes) ===
                for (var i = 0; i < particles.length; i++) {
                    var p = particles[i];
                    // Find nearest shape
                    var bestDist = Infinity;
                    var bestDx = 0, bestDy = 0;
                    for (var s = 0; s < shapes.length; s++) {
                        var dx = shapes[s].x - p.x;
                        var dy = shapes[s].y - p.y;
                        var d2 = dx * dx + dy * dy;
                        if (d2 < bestDist) {
                            bestDist = d2;
                            bestDx = dx;
                            bestDy = dy;
                        }
                    }
                    var dist = Math.sqrt(bestDist) + 1;
                    p.vx += bestDx / dist * pForce;
                    p.vy += bestDy / dist * pForce;
                    p.vx *= 0.96;
                    p.vy *= 0.96;
                    p.x += p.vx;
                    p.y += p.vy;
                    // Wrap
                    if (p.x < 0) p.x += cw;
                    if (p.x >= cw) p.x -= cw;
                    if (p.y < 0) p.y += ch;
                    if (p.y >= ch) p.y -= ch;
                }

                // === Decay brightness field ===
                for (var i = 0; i < brightnessField.length; i++) {
                    brightnessField[i] *= decay;
                }

                // === Splat particles into field ===
                var fsx = fieldCols / cw;
                var fsy = fieldRows / ch;
                for (var i = 0; i < particles.length; i++) {
                    splatStamp(brightnessField, fieldCols, fieldRows,
                        particles[i].x * fsx, particles[i].y * fsy, particleStamp);
                }
                // Splat shapes (larger stamp)
                for (var s = 0; s < shapes.length; s++) {
                    splatStamp(brightnessField, fieldCols, fieldRows,
                        shapes[s].x * fsx, shapes[s].y * fsy, shapeStamp);
                }

                // === Render text ===
                tCtx.clearRect(0, 0, cw, ch);
                if (!wantTransparent) {
                    tCtx.fillStyle = bgCSS;
                    tCtx.fillRect(0, 0, cw, ch);
                }

                tCtx.textBaseline = 'middle';
                tCtx.fillStyle = fgCSS;

                var charIdx = 0;
                var totalChars = textChars.length;

                for (var row = 0; row < rows; row++) {
                    for (var col = 0; col < cols; col++) {
                        // Get the character from user text (looping)
                        var ch_char = textChars[charIdx % totalChars];
                        charIdx++;

                        // Base position
                        var baseX = col * cellW;
                        var baseY = row * cellH + cellH * 0.5;

                        // Sample brightness from field
                        var fx = (col + 0.5) / cols * fieldCols;
                        var fy = (row + 0.5) / rows * fieldRows;
                        var fix = Math.min(fieldCols - 1, Math.max(0, Math.floor(fx)));
                        var fiy = Math.min(fieldRows - 1, Math.max(0, Math.floor(fy)));
                        var brightness = Math.min(1, brightnessField[fiy * fieldCols + fix]);

                        // === Displacement from shapes ===
                        var dispX = 0, dispY = 0;
                        for (var s = 0; s < shapes.length; s++) {
                            var dx = baseX - shapes[s].x;
                            var dy = baseY - shapes[s].y;
                            var dist = Math.sqrt(dx * dx + dy * dy);
                            var influence = shapes[s].r / (dist + 1);
                            influence = influence * influence; // sharper falloff
                            if (influence > 0.001) {
                                var nx = dx / (dist + 1);
                                var ny = dy / (dist + 1);
                                dispX += nx * influence * displaceStr * cellW;
                                dispY += ny * influence * displaceStr * cellH;
                            }
                        }

                        var drawX = baseX + dispX;
                        var drawY = baseY + dispY;

                        // Weight from brightness: thin at 0, bold at 1
                        var weight = Math.round(300 + brightness * 600);
                        weight = Math.min(900, Math.max(100, weight));

                        // Alpha scales with proximity to shapes and brightness
                        var alpha = 0.25 + brightness * 0.75;
                        // Also boost alpha near shapes
                        var shapeProx = 0;
                        for (var s = 0; s < shapes.length; s++) {
                            var dx2 = drawX - shapes[s].x;
                            var dy2 = drawY - shapes[s].y;
                            var d = Math.sqrt(dx2 * dx2 + dy2 * dy2);
                            shapeProx = Math.max(shapeProx, Math.max(0, 1 - d / (shapes[s].r * 3)));
                        }
                        alpha = Math.min(1, alpha + shapeProx * 0.4);

                        // Scale up characters near shapes
                        var scale = 1.0 + shapeProx * 0.8;
                        var drawSize = Math.round(fSize * scale);

                        tCtx.font = weight + ' ' + drawSize + 'px ' + fontFamily;
                        tCtx.globalAlpha = alpha;
                        tCtx.fillText(ch_char, drawX, drawY);
                    }
                }

                // === Draw shape outlines if enabled ===
                if (showShapes) {
                    tCtx.globalAlpha = 0.3;
                    tCtx.strokeStyle = fgCSS;
                    tCtx.lineWidth = 1.5;
                    for (var s = 0; s < shapes.length; s++) {
                        tCtx.beginPath();
                        tCtx.arc(shapes[s].x, shapes[s].y, shapes[s].r, 0, Math.PI * 2);
                        tCtx.stroke();
                    }
                }

                tCtx.globalAlpha = 1;
                texture.needsUpdate = true;

                // Background
                if (wantTransparent) {
                    scene.background = null;
                } else {
                    if (!scene.background) scene.background = new THREE.Color();
                    scene.background.setRGB(bg[0], bg[1], bg[2]);
                }
            }
        };
    }

    return { create: create, INPUTS: INPUTS };
})(typeof THREE !== 'undefined' ? THREE : null);

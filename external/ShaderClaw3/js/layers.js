// 7-Layer Model + Per-Layer Render Logic
// Manages FBO creation, layer rendering dispatch, and the composition pipeline

import { state, LAYER_IDS, getLayer, emit } from './state.js';
import { buildFragmentShader, VERT_SHADER, parseISF } from './isf.js';
import { getAudioState, updateAudioUniforms } from './audio.js';
import { getFontState } from './media.js';

// Scale FBOs to device — mobile GPUs can't handle 7× 1920x1080 FBOs
const _isMobile = typeof window !== 'undefined' && (window.innerWidth <= 900 || /Mobi|Android|iPhone/i.test(navigator.userAgent));
let FBO_WIDTH = _isMobile ? Math.min(960, window.innerWidth * (window.devicePixelRatio || 1)) : 1920;
let FBO_HEIGHT = _isMobile ? Math.min(540, window.innerHeight * (window.devicePixelRatio || 1)) : 1080;

/**
 * Resize all layer FBOs to new dimensions
 * @param {Renderer} renderer
 * @param {number} w - new width
 * @param {number} h - new height
 */
export function resizeLayerFBOs(renderer, w, h) {
  FBO_WIDTH = w;
  FBO_HEIGHT = h;
  for (const id of LAYER_IDS) {
    const layer = getLayer(id);
    layer.fbo = renderer.createFBO(FBO_WIDTH, FBO_HEIGHT);
    // Re-create multi-pass ping-pong FBOs if present
    if (layer.passes) {
      for (const p of layer.passes) {
        if (p.ppFBO) {
          renderer.destroyPingPongFBO(p.ppFBO);
          let pw = FBO_WIDTH, ph = FBO_HEIGHT;
          // Perf: auto-downscale simulation (TARGET) passes on ultra-wide canvases
          const isSimPass = !!p.target;
          if (isSimPass && FBO_WIDTH > 4096) {
            pw = Math.max(1, Math.round(pw / 2));
            ph = Math.max(1, Math.round(ph / 2));
          }
          p._simDownscaled = isSimPass && FBO_WIDTH > 4096;
          p.width = pw;
          p.height = ph;
          p.ppFBO = renderer.createPingPongFBO(pw, ph);
        }
      }
    }
  }
}

export function getFBOSize() { return { width: FBO_WIDTH, height: FBO_HEIGHT }; }

/**
 * Initialize FBOs for all layers
 * @param {Renderer} renderer
 */
export function initLayerFBOs(renderer) {
  for (const id of LAYER_IDS) {
    const layer = getLayer(id);
    if (!layer.fbo) {
      layer.fbo = renderer.createFBO(FBO_WIDTH, FBO_HEIGHT);
    }
  }
}

/**
 * Recreate FBOs after context loss
 */
export function recreateLayerFBOs(renderer) {
  for (const id of LAYER_IDS) {
    const layer = getLayer(id);
    layer.fbo = renderer.createFBO(FBO_WIDTH, FBO_HEIGHT);
    layer.program = null;
    layer.uniformLocs = {};
    // Recompile from stored source
    if (layer._isfSource) {
      compileToLayer(renderer, id, layer._isfSource);
    }
  }
}

/**
 * Compile ISF source to a specific layer
 * @param {Renderer} renderer
 * @param {string} layerId
 * @param {string} source - ISF shader source
 * @returns {{ ok: boolean, errors: string|null, inputs: Array }}
 */
export function compileToLayer(renderer, layerId, source) {
  const layer = getLayer(layerId);
  if (!layer) return { ok: false, errors: `Unknown layer: ${layerId}` };

  // Store for context-loss recovery
  layer._isfSource = source;

  const { frag, parsed, headerLineCount } = buildFragmentShader(source);
  renderer._headerLines = headerLineCount;

  const result = renderer.compileForLayer(layer, VERT_SHADER, frag);
  if (!result.ok) return result;

  // Store parsed inputs
  layer.inputs = parsed.inputs || [];
  layer._hasBgColor = layer.inputs.some(inp => inp.NAME === 'bgColor');
  if (!layer._bgTexture) layer._bgTexture = null;

  // Reset input values with defaults
  const oldValues = { ...layer.inputValues };
  layer.inputValues = {};
  for (const inp of layer.inputs) {
    const name = inp.NAME;
    // Preserve old values if same param exists
    if (name in oldValues) {
      layer.inputValues[name] = oldValues[name];
    } else if (inp.TYPE === 'float' || inp.TYPE === 'long') {
      layer.inputValues[name] = inp.DEFAULT != null ? inp.DEFAULT : 0;
    } else if (inp.TYPE === 'color') {
      layer.inputValues[name] = inp.DEFAULT ? [...inp.DEFAULT] : [1, 1, 1, 1];
    } else if (inp.TYPE === 'bool') {
      layer.inputValues[name] = !!inp.DEFAULT;
    } else if (inp.TYPE === 'point2D') {
      layer.inputValues[name] = inp.DEFAULT ? [...inp.DEFAULT] : [0, 0];
    } else if (inp.TYPE === 'text') {
      const maxLen = inp.MAX_LENGTH || 12;
      const def = (inp.DEFAULT || '').toUpperCase();
      for (let i = 0; i < maxLen; i++) {
        const ch = def[i];
        layer.inputValues[name + '_' + i] = (!ch || ch === ' ') ? 26 : (function(c) { var code = c.charCodeAt(0); if (code >= 65 && code <= 90) return code - 65; if (code >= 48 && code <= 57) return code - 48 + 27; return 26; })(ch.toUpperCase());
      }
      layer.inputValues[name + '_len'] = def.replace(/\s+$/, '').length;
    }
  }

  // Handle PASSES metadata for multi-pass shaders
  layer.passes = null;
  if (parsed.meta && Array.isArray(parsed.meta.PASSES) && parsed.meta.PASSES.length > 0) {
    layer.passes = parsed.meta.PASSES.map(p => {
      let pw = FBO_WIDTH, ph = FBO_HEIGHT;
      const _parseDim = (v, w, h) => {
        if (typeof v === 'number') return v;
        if (typeof v === 'string') {
          const s = v.replace(/\$WIDTH/g, w).replace(/\$HEIGHT/g, h);
          // Handle simple expressions: number, or number / number
          const divMatch = s.match(/^\s*(\d+(?:\.\d+)?)\s*\/\s*(\d+(?:\.\d+)?)\s*$/);
          if (divMatch) return parseFloat(divMatch[1]) / parseFloat(divMatch[2]);
          const mulMatch = s.match(/^\s*(\d+(?:\.\d+)?)\s*\*\s*(\d+(?:\.\d+)?)\s*$/);
          if (mulMatch) return parseFloat(mulMatch[1]) * parseFloat(mulMatch[2]);
          const num = parseFloat(s);
          if (!isNaN(num)) return num;
        }
        return null;
      };
      try { if (p.WIDTH)  { const v = _parseDim(p.WIDTH,  FBO_WIDTH, FBO_HEIGHT); if (v) pw = v; } } catch(e) {}
      try { if (p.HEIGHT) { const v = _parseDim(p.HEIGHT, FBO_WIDTH, FBO_HEIGHT); if (v) ph = v; } } catch(e) {}
      // Perf: auto-downscale simulation (TARGET) passes on ultra-wide canvases
      // Fluid sim doesn't need pixel-perfect resolution; halving saves ~75% fill cost
      const isSimPass = !!p.TARGET;
      if (isSimPass && FBO_WIDTH > 4096) {
        pw = Math.max(1, Math.round(pw / 2));
        ph = Math.max(1, Math.round(ph / 2));
      }
      const pass = {
        target: p.TARGET || null,
        persistent: !!p.PERSISTENT,
        width: pw,
        height: ph,
        _simDownscaled: isSimPass && FBO_WIDTH > 4096,
        ppFBO: null,
      };
      if (pass.target) {
        pass.ppFBO = renderer.createPingPongFBO(pass.width, pass.height);
      }
      return pass;
    });
  }

  emit('layer:compiled', { layerId });
  return { ok: true, errors: null, inputs: layer.inputs };
}

/**
 * Load shader file and compile to layer
 */
export async function loadShaderToLayer(renderer, layerId, folder, file) {
  const url = `${folder}/${file}`;
  const resp = await fetch(url);
  if (!resp.ok) throw new Error(`Failed to load shader: ${url}`);
  const source = await resp.text();
  return { ...compileToLayer(renderer, layerId, source), source };
}

/**
 * The main composition render loop
 * Call this once per frame from app.js
 *
 * Render order (per spec):
 * 1. Background → FBO[0]
 * 2. Media → FBO[1]
 * 3. 3D (with ISF materials) → FBO[2]
 * 4. AV (with audio uniforms) → FBO[3]
 * 5. Composite FBO[0-3] → tempComposite (for Effects input)
 * 6. Effects (reads tempComposite) → FBO[4]
 * 7. Text → FBO[5]
 * 8. Overlay → FBO[6]
 * 9. Final composite: blend all 7 FBOs
 */
export function renderComposition(renderer, sceneRenderer, mediaPipeMgr, tempCompositeFBO) {
  const gl = renderer.gl;
  const audioState = getAudioState();
  const fontState = getFontState();

  // Update audio analysis
  updateAudioUniforms(gl);

  // Get layers in render order
  const layerOrder = LAYER_IDS;
  const orderedLayers = layerOrder.map(id => getLayer(id));

  // --- Phase 1: Render individual layers to their FBOs ---

  // Background layer (index 0)
  const bgLayer = getLayer('background');
  if (bgLayer.visible && bgLayer.program) {
    renderer.renderLayerToFBO(bgLayer, audioState, mediaPipeMgr, fontState, null);
  }

  // Media layer (index 1)
  const mediaLayer = getLayer('media');
  if (mediaLayer.visible && mediaLayer.program) {
    renderer.renderLayerToFBO(mediaLayer, audioState, mediaPipeMgr, fontState, null);
  }

  // 3D layer (index 2) - rendered by SceneRenderer
  const layer3d = getLayer('3d');
  let sceneTexture = null;
  if (layer3d.visible && sceneRenderer && sceneRenderer.sceneDef) {
    sceneRenderer.render();
    // Upload Three.js canvas to a GL texture for the compositor
    const threeCanvas = sceneRenderer.canvas;
    if (!layer3d._sceneTexture) {
      layer3d._sceneTexture = gl.createTexture();
      gl.bindTexture(gl.TEXTURE_2D, layer3d._sceneTexture);
      gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
      gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
      gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
      gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
    } else {
      gl.bindTexture(gl.TEXTURE_2D, layer3d._sceneTexture);
    }
    // Frame-skip: upload every other frame (visual 30fps, compositor stays 60fps)
    layer3d._frameCount = (layer3d._frameCount || 0) + 1;
    if (layer3d._frameCount % 2 === 0) {
      gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, threeCanvas);
    }
    sceneTexture = layer3d._sceneTexture;
  }

  // AV layer (index 3) - audio visualization
  const avLayer = getLayer('av');
  if (avLayer.visible && avLayer.program) {
    renderer.renderLayerToFBO(avLayer, audioState, mediaPipeMgr, fontState, null);
  }

  // --- Phase 2: Composite layers 0-3 for Effects input ---
  let effectsInputTexture = null;
  const effectsLayer = getLayer('effects');
  if (effectsLayer.visible && effectsLayer.program && tempCompositeFBO) {
    // Render partial composite of first 4 layers
    const partialLayers = orderedLayers.slice(0, 4);
    // Pad to 7 slots with null for the compositor shader
    while (partialLayers.length < 7) partialLayers.push(null);
    renderer.renderPartialComposite(partialLayers, sceneTexture, state.background, tempCompositeFBO);
    effectsInputTexture = tempCompositeFBO.texture;
  }

  // --- Phase 3: Effects layer (index 4) ---
  if (effectsLayer.visible && effectsLayer.program) {
    renderer.renderLayerToFBO(effectsLayer, audioState, mediaPipeMgr, fontState, effectsInputTexture);
  }

  // --- Phase 4: Text layer (index 5) ---
  const textLayer = getLayer('text');
  if (textLayer.visible && textLayer.program) {
    renderer.renderLayerToFBO(textLayer, audioState, mediaPipeMgr, fontState, null);
  }

  // --- Phase 5: Overlay layer (index 6) ---
  const overlayLayer = getLayer('overlay');
  if (overlayLayer.visible && overlayLayer.program) {
    renderer.renderLayerToFBO(overlayLayer, audioState, mediaPipeMgr, fontState, null);
  }

  // Re-upload animated GIF (throttle to 30fps — GIFs rarely exceed this)
  if (overlayLayer.visible && overlayLayer._gifElement && overlayLayer.fbo) {
    overlayLayer._gifFrameCount = (overlayLayer._gifFrameCount || 0) + 1;
    if (overlayLayer._gifFrameCount % 2 === 0) {
      const gl = renderer.gl;
      gl.bindTexture(gl.TEXTURE_2D, overlayLayer.fbo.texture);
      gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, true);
      gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, overlayLayer._gifElement);
      gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, false);
    }
  }

  // Re-upload overlay video every frame
  if (overlayLayer.visible && overlayLayer._videoElement && overlayLayer.fbo) {
    const vid = overlayLayer._videoElement;
    if (vid.readyState >= 2 && !vid.paused) {
      const gl = renderer.gl;
      gl.bindTexture(gl.TEXTURE_2D, overlayLayer.fbo.texture);
      gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, true);
      gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, vid);
      gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, false);
    }
    // Auto-resume if paused unexpectedly
    if (vid.paused && vid.loop && !vid.ended) vid.play().catch(() => {});
  }

  // --- Phase 6: Final composite ---
  renderer.renderCompositor(orderedLayers, sceneTexture, state.background);
}

/**
 * Auto-bind media textures to all layers that have image inputs
 */
export function autoBindTextures() {
  // Pre-filter compatible media once (not per-layer)
  const compatible = state.mediaInputs.filter(m => m.type === 'image' || m.type === 'video' || m.type === 'svg');

  for (const id of LAYER_IDS) {
    const layer = getLayer(id);
    if (!layer.inputs) continue;

    const imageInputs = layer.inputs.filter(inp => inp.TYPE === 'image');
    let imageIdx = 0;

    for (const inp of imageInputs) {
      const mediaId = layer.inputValues[inp.NAME];
      if (mediaId) {
        const media = state.mediaInputs.find(m => String(m.id) === String(mediaId));
        if (media && media.glTexture) {
          layer.textures[inp.NAME] = {
            glTexture: media.glTexture,
            isVideo: media.type === 'video',
            element: media.element,
            flipH: media._webcamFlip || false,
            flipV: media._webcamFlipV || false,
            _isNdi: media._isNdi || false,
          };
        }
      } else {
        // Auto-bind by index
        if (compatible[imageIdx] && compatible[imageIdx].glTexture) {
          layer.textures[inp.NAME] = {
            glTexture: compatible[imageIdx].glTexture,
            isVideo: compatible[imageIdx].type === 'video',
            element: compatible[imageIdx].element,
            flipH: compatible[imageIdx]._webcamFlip || false,
            flipV: compatible[imageIdx]._webcamFlipV || false,
            _isNdi: compatible[imageIdx]._isNdi || false,
          };
          layer.inputValues[inp.NAME] = compatible[imageIdx].id;
        }
        imageIdx++;
      }
    }
  }
}

// ============================================================
// Per-Layer FBO Export — streams individual layers for multi-GPU
// ============================================================

const _exports = {};       // layerId → { canvas, ctx, stream, imageData, pixelBuf }
let _exportsActive = false;

/**
 * Initialize per-layer export streams at a fixed resolution.
 * Each exported layer gets its own offscreen canvas + captureStream.
 * @param {string[]} layerIds - which layers to export (e.g., ['background', 'media', 'av'])
 * @param {number} width - export width (default 832)
 * @param {number} height - export height (default 480)
 */
export function initLayerExports(layerIds, width = 832, height = 480) {
  // Clean up any previous exports
  destroyLayerExports();

  for (const id of layerIds) {
    const layer = getLayer(id);
    if (!layer) continue;

    const canvas = document.createElement('canvas');
    canvas.width = width;
    canvas.height = height;
    const ctx = canvas.getContext('2d', { willReadFrequently: false });

    // captureStream(0) = manual frame push via requestFrame()
    const stream = canvas.captureStream(0);
    const track = stream.getVideoTracks()[0];

    // Pre-allocate pixel buffer and ImageData for readPixels
    const pixelBuf = new Uint8ClampedArray(width * height * 4);
    const imageData = new ImageData(pixelBuf, width, height);

    _exports[id] = { canvas, ctx, stream, track, imageData, pixelBuf, width, height };
  }

  _exportsActive = true;

  // Expose on window for cross-app access (Etherea dashboard reads this)
  window.layerExportStreams = {};
  for (const [id, exp] of Object.entries(_exports)) {
    window.layerExportStreams[id] = exp.stream;
  }

  console.log(`[LayerExport] initialized ${layerIds.length} layer streams at ${width}x${height}`);
}

/**
 * Per-frame: read each exported layer's FBO and push to its stream.
 * Call after renderComposition() in the composition loop.
 * @param {Renderer} renderer - the WebGL renderer (needs .gl)
 */
export function updateLayerExports(renderer) {
  if (!_exportsActive) return;

  const gl = renderer.gl;

  for (const [id, exp] of Object.entries(_exports)) {
    const layer = getLayer(id);
    if (!layer || !layer.fbo) continue;

    // Bind the layer's FBO and read pixels
    gl.bindFramebuffer(gl.FRAMEBUFFER, layer.fbo.fbo);
    gl.readPixels(0, 0, exp.width, exp.height, gl.RGBA, gl.UNSIGNED_BYTE, exp.pixelBuf);

    // WebGL readPixels gives bottom-up rows — flip vertically
    const w4 = exp.width * 4;
    const half = exp.height >> 1;
    for (let y = 0; y < half; y++) {
      const topOff = y * w4;
      const botOff = (exp.height - 1 - y) * w4;
      for (let x = 0; x < w4; x++) {
        const tmp = exp.pixelBuf[topOff + x];
        exp.pixelBuf[topOff + x] = exp.pixelBuf[botOff + x];
        exp.pixelBuf[botOff + x] = tmp;
      }
    }

    // Write to the export canvas
    exp.ctx.putImageData(exp.imageData, 0, 0);

    // Push frame to the MediaStream
    if (exp.track && exp.track.readyState === 'live') {
      exp.track.requestFrame();
    }
  }

  // Unbind FBO — restore to screen
  gl.bindFramebuffer(gl.FRAMEBUFFER, null);
}

/**
 * Get the MediaStream for a specific layer
 * @param {string} layerId
 * @returns {MediaStream|null}
 */
export function getLayerStream(layerId) {
  return _exports[layerId]?.stream || null;
}

/**
 * Check if layer exports are active
 * @returns {boolean}
 */
export function isExportActive() {
  return _exportsActive;
}

/**
 * Tear down all exports
 */
export function destroyLayerExports() {
  for (const [id, exp] of Object.entries(_exports)) {
    if (exp.track) exp.track.stop();
  }
  for (const key of Object.keys(_exports)) delete _exports[key];
  _exportsActive = false;
  if (window.layerExportStreams) delete window.layerExportStreams;
  console.log('[LayerExport] destroyed');
}

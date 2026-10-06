// ============================================================
// Layer Export — Per-layer FBO → MediaStream for multi-GPU streaming
// ============================================================
//
// Usage:
//   LayerExport.init(['background', 'media', 'av'], 832, 480);
//   // Now window.layerExportStreams has { background: MediaStream, ... }
//   // Each stream can be sent via separate WHIP connection to a Scope pod
//   LayerExport.destroy();
//

(function () {
  'use strict';

  const _exports = {};       // layerId → { canvas, ctx, stream, track, imageData, pixelBuf }
  let _active = false;

  /**
   * Initialize per-layer export streams.
   * @param {string[]} layerIds - which layers to export (e.g., ['background', 'media', 'av'])
   * @param {number} width - export resolution width (default 832)
   * @param {number} height - export resolution height (default 480)
   */
  function init(layerIds, width = 832, height = 480) {
    destroy();

    for (const id of layerIds) {
      const canvas = document.createElement('canvas');
      canvas.width = width;
      canvas.height = height;
      canvas.style.display = 'none';
      document.body.appendChild(canvas);

      const ctx = canvas.getContext('2d', { willReadFrequently: false });

      // captureStream(0) = manual frame push via requestFrame()
      const stream = canvas.captureStream(0);
      const track = stream.getVideoTracks()[0];

      // Pre-allocate pixel buffer and ImageData
      const pixelBuf = new Uint8ClampedArray(width * height * 4);
      const imageData = new ImageData(pixelBuf, width, height);

      _exports[id] = { canvas, ctx, stream, track, imageData, pixelBuf, width, height };
    }

    _active = true;

    // Expose streams for cross-app access
    window.layerExportStreams = {};
    for (const [id, exp] of Object.entries(_exports)) {
      window.layerExportStreams[id] = exp.stream;
    }

    // Register the per-frame hook in the composition loop
    window._layerExportUpdate = _update;

    console.log(`[LayerExport] initialized ${layerIds.length} streams at ${width}x${height}: ${layerIds.join(', ')}`);
  }

  /**
   * Per-frame update — called from compositionLoop via window._layerExportUpdate hook.
   * Reads each layer's FBO pixels and pushes to the export stream.
   * @param {WebGLRenderingContext} gl
   * @param {Array} layers - the ShaderClaw layers array
   */
  function _update(gl, layers) {
    if (!_active) return;

    for (const [id, exp] of Object.entries(_exports)) {
      // Find layer by id in the layers array
      const layer = layers.find(l => l.id === id);
      if (!layer || !layer.fbo) continue;

      // Bind layer FBO and read pixels
      gl.bindFramebuffer(gl.FRAMEBUFFER, layer.fbo.fbo);

      // If FBO is larger than export, read only the export region
      const readW = Math.min(exp.width, layer.fbo.width);
      const readH = Math.min(exp.height, layer.fbo.height);

      gl.readPixels(0, 0, readW, readH, gl.RGBA, gl.UNSIGNED_BYTE, exp.pixelBuf);

      // WebGL readPixels gives bottom-up rows — flip vertically
      const w4 = exp.width * 4;
      const half = readH >> 1;
      for (let y = 0; y < half; y++) {
        const topOff = y * w4;
        const botOff = (readH - 1 - y) * w4;
        for (let x = 0; x < w4; x++) {
          const tmp = exp.pixelBuf[topOff + x];
          exp.pixelBuf[topOff + x] = exp.pixelBuf[botOff + x];
          exp.pixelBuf[botOff + x] = tmp;
        }
      }

      // Write to export canvas
      exp.ctx.putImageData(exp.imageData, 0, 0);

      // Push frame to MediaStream
      if (exp.track && exp.track.readyState === 'live') {
        exp.track.requestFrame();
      }
    }

    // Restore framebuffer to screen
    gl.bindFramebuffer(gl.FRAMEBUFFER, null);
  }

  /**
   * Get the MediaStream for a specific layer.
   * @param {string} layerId
   * @returns {MediaStream|null}
   */
  function getStream(layerId) {
    return _exports[layerId]?.stream || null;
  }

  /**
   * Get export status.
   * @returns {{ active: boolean, layers: string[], resolution: string }}
   */
  function status() {
    const ids = Object.keys(_exports);
    const first = _exports[ids[0]];
    return {
      active: _active,
      layers: ids,
      resolution: first ? `${first.width}x${first.height}` : 'none',
    };
  }

  /**
   * Tear down all exports.
   */
  function destroy() {
    for (const [id, exp] of Object.entries(_exports)) {
      if (exp.track) exp.track.stop();
      if (exp.canvas.parentNode) exp.canvas.parentNode.removeChild(exp.canvas);
    }
    for (const key of Object.keys(_exports)) delete _exports[key];
    _active = false;
    delete window.layerExportStreams;
    delete window._layerExportUpdate;
    console.log('[LayerExport] destroyed');
  }

  // Expose API on window
  window.LayerExport = { init, destroy, getStream, status };

})();

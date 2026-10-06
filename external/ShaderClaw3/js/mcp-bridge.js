// ============================================================
// ShaderClaw — NDI State and Helpers (Worker-based capture)
// ============================================================

let ndiReceiveEntry = null;
let ndiReceiveCanvas = null;
let ndiReceiveCtx = null;
let ndiSendingActive = false;
// Auto-start NDI only on localhost (Vercel has no server)
let _ndiAutoStartOnConnect = (typeof location !== 'undefined') && (location.hostname === 'localhost' || location.hostname === '127.0.0.1' || /^192\.168\.|^10\./.test(location.hostname));
let ndiSendAnimId = null;
let ndiSendFrameCount = 0;
let ndiSendWorker = null;
let ndiWsMsgId = 0;
const ndiPending = new Map();
const FRAME_TYPE_NDI_VIDEO = 0x01;
const FRAME_TYPE_CANVAS = 0x02;

function ndiRequest(ws, action, params = {}) {
  return new Promise((resolve, reject) => {
    if (!ws || ws.readyState !== WebSocket.OPEN) {
      return reject(new Error('WebSocket not connected'));
    }
    const id = --ndiWsMsgId;
    const timer = setTimeout(() => {
      ndiPending.delete(id);
      reject(new Error('NDI request timeout'));
    }, 5000);
    ndiPending.set(id, { resolve, reject, timer });
    ws.send(JSON.stringify({ id, action, params }));
  });
}

function handleNdiVideoFrame(data, glRef) {
  if (!ndiReceiveEntry) return;
  const view = new DataView(data);
  const compressed = view.getUint8(1); // 0x01 = JPEG
  const width = view.getUint32(2, true);
  const height = view.getUint32(6, true);

  if (compressed === 0x01) {
    // JPEG compressed — decode via createImageBitmap
    const jpegBlob = new Blob([new Uint8Array(data, 10)], { type: 'image/jpeg' });
    createImageBitmap(jpegBlob, { premultiplyAlpha: 'none' }).then(bitmap => {
      if (!ndiReceiveEntry) return;

      // Resize canvas if needed
      if (!ndiReceiveCanvas || ndiReceiveCanvas.width !== width || ndiReceiveCanvas.height !== height) {
        ndiReceiveCanvas = document.createElement('canvas');
        ndiReceiveCanvas.width = width;
        ndiReceiveCanvas.height = height;
        ndiReceiveCtx = ndiReceiveCanvas.getContext('2d');
        if (ndiReceiveEntry) ndiReceiveEntry.element = ndiReceiveCanvas;
      }

      // Draw bitmap to canvas (needed for GL upload)
      ndiReceiveCtx.drawImage(bitmap, 0, 0);
      bitmap.close();

      // Upload to GL texture (flip Y for correct orientation)
      if (ndiReceiveEntry.glTexture && glRef) {
        glRef.pixelStorei(glRef.UNPACK_FLIP_Y_WEBGL, true);
        glRef.bindTexture(glRef.TEXTURE_2D, ndiReceiveEntry.glTexture);
        if (ndiReceiveEntry._texW === width && ndiReceiveEntry._texH === height) {
          glRef.texSubImage2D(glRef.TEXTURE_2D, 0, 0, 0, glRef.RGBA, glRef.UNSIGNED_BYTE, ndiReceiveCanvas);
        } else {
          glRef.texImage2D(glRef.TEXTURE_2D, 0, glRef.RGBA, glRef.RGBA, glRef.UNSIGNED_BYTE, ndiReceiveCanvas);
          ndiReceiveEntry._texW = width;
          ndiReceiveEntry._texH = height;
        }
        glRef.pixelStorei(glRef.UNPACK_FLIP_Y_WEBGL, false);
      }

      // Three.js texture
      if (ndiReceiveEntry.threeTexture) {
        ndiReceiveEntry.threeTexture.image = ndiReceiveCanvas;
        ndiReceiveEntry.threeTexture.needsUpdate = true;
      }
    }).catch(() => {});
    return;
  }

  // Fallback: raw RGBA (uncompressed — legacy path)
  const pixels = new Uint8Array(data, 10);
  if (!ndiReceiveCanvas || ndiReceiveCanvas.width !== width || ndiReceiveCanvas.height !== height) {
    ndiReceiveCanvas = document.createElement('canvas');
    ndiReceiveCanvas.width = width;
    ndiReceiveCanvas.height = height;
    ndiReceiveCtx = ndiReceiveCanvas.getContext('2d');
    if (ndiReceiveEntry) ndiReceiveEntry.element = ndiReceiveCanvas;
  }

  if (ndiReceiveEntry.glTexture && glRef) {
    glRef.bindTexture(glRef.TEXTURE_2D, ndiReceiveEntry.glTexture);
    if (ndiReceiveEntry._texW === width && ndiReceiveEntry._texH === height) {
      glRef.texSubImage2D(glRef.TEXTURE_2D, 0, 0, 0, width, height, glRef.RGBA, glRef.UNSIGNED_BYTE, pixels);
    } else {
      glRef.texImage2D(glRef.TEXTURE_2D, 0, glRef.RGBA, width, height, 0, glRef.RGBA, glRef.UNSIGNED_BYTE, pixels);
      ndiReceiveEntry._texW = width;
      ndiReceiveEntry._texH = height;
    }
  }

  if (ndiReceiveEntry.threeTexture) {
    const imageData = new ImageData(new Uint8ClampedArray(pixels.buffer, pixels.byteOffset, pixels.byteLength), width, height);
    ndiReceiveCtx.putImageData(imageData, 0, 0);
    ndiReceiveEntry.threeTexture.image = ndiReceiveCanvas;
    ndiReceiveEntry.threeTexture.needsUpdate = true;
  }
}

// NDI capture — zero-worker direct readback pipeline
// For 8000x1800: uses a dedicated downscale canvas to keep pixel throughput manageable

let _ndiCaptureCanvas = null;
let _ndiCaptureCtx = null;
let _ndiMsgBuf = null;
let _ndiOutW = 0, _ndiOutH = 0;

function _ndiEnsureCaptureCanvas(cw, ch) {
  // Target ~2M pixels for NDI output — preserves aspect ratio
  const maxPixels = 1920 * 1080;
  const pixels = cw * ch;
  let outW = cw, outH = ch;
  if (pixels > maxPixels) {
    const scale = Math.sqrt(maxPixels / pixels);
    outW = Math.round(cw * scale) & ~1; // even dims for NDI
    outH = Math.round(ch * scale) & ~1;
  }
  if (_ndiOutW !== outW || _ndiOutH !== outH) {
    _ndiCaptureCanvas = document.createElement('canvas');
    _ndiCaptureCanvas.width = outW;
    _ndiCaptureCanvas.height = outH;
    _ndiCaptureCtx = _ndiCaptureCanvas.getContext('2d', { alpha: false, willReadFrequently: true });
    _ndiOutW = outW;
    _ndiOutH = outH;
    _ndiMsgBuf = null;
  }
}

function startNdiSend(ws, canvasEl) {
  if (ndiSendingActive) return;
  ndiSendingActive = true;
  document.getElementById('ndi-indicator')?.classList.add('active');
  const sendBtn = document.getElementById('ndi-send-btn');
  const statusDot = document.getElementById('ndi-status');
  if (sendBtn) sendBtn.textContent = 'Stop';
  if (statusDot) statusDot.classList.add('active');
  ndiSendFrameCount = 0;

  let busy = false;
  let lastCapture = 0;

  function captureLoop(timestamp) {
    if (!ndiSendingActive) return;
    ndiSendAnimId = requestAnimationFrame(captureLoop);

    // 30fps target — but skip if still processing last frame (backpressure)
    if (timestamp - lastCapture < 33) return;
    if (busy) return;
    if (!ws || ws.readyState !== WebSocket.OPEN) return;
    // Drop frames if WS can't keep up
    if (ws.bufferedAmount > 16 * 1024 * 1024) return;

    lastCapture = timestamp;
    busy = true;

    // Direct capture: canvas → downscale canvas → getImageData → WS
    // No workers, no ImageBitmap, no postMessage overhead
    const cw = canvasEl.width, ch = canvasEl.height;
    _ndiEnsureCaptureCanvas(cw, ch);

    // Blit + downscale in one drawImage call
    _ndiCaptureCtx.drawImage(canvasEl, 0, 0, _ndiOutW, _ndiOutH);

    // Read pixels directly
    const imgData = _ndiCaptureCtx.getImageData(0, 0, _ndiOutW, _ndiOutH);
    const pixelBytes = imgData.data.byteLength;

    // Build binary frame: [0x02][width LE4][height LE4][RGBA pixels]
    const totalBytes = 9 + pixelBytes;
    if (!_ndiMsgBuf || _ndiMsgBuf.byteLength !== totalBytes) {
      _ndiMsgBuf = new ArrayBuffer(totalBytes);
    }
    const header = new DataView(_ndiMsgBuf, 0, 9);
    header.setUint8(0, 0x02);
    header.setUint32(1, _ndiOutW, true);
    header.setUint32(5, _ndiOutH, true);
    new Uint8Array(_ndiMsgBuf, 9).set(imgData.data);

    ws.send(_ndiMsgBuf);
    ndiSendFrameCount++;
    busy = false;
  }
  ndiSendAnimId = requestAnimationFrame(captureLoop);
}

// Pause capture loop without changing UI (for WS disconnect/reconnect)
function pauseNdiSend() {
  if (ndiSendAnimId) { cancelAnimationFrame(ndiSendAnimId); ndiSendAnimId = null; }
}

// Full stop — user-initiated, resets UI
function stopNdiSend() {
  ndiSendingActive = false;
  document.getElementById('ndi-indicator')?.classList.remove('active');
  // Update sidebar button + status dot
  const sendBtn = document.getElementById('ndi-send-btn');
  const statusDot = document.getElementById('ndi-status');
  if (sendBtn) sendBtn.textContent = 'Send';
  if (statusDot) statusDot.classList.remove('active');
  pauseNdiSend();
}

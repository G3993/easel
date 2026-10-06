// ============================================================
// ShaderClaw — Deferred Script Loaders
// ============================================================

// Lazy-load 3D model loaders + MediaPipe after first paint
window._deferredLoaders = false;
function loadDeferredScripts() {
  if (window._deferredLoaders) return Promise.resolve();
  window._deferredLoaders = true;
  const urls = [
    'https://cdn.jsdelivr.net/npm/fflate@0.6.10/umd/index.min.js',
    'https://cdn.jsdelivr.net/npm/three@0.128.0/examples/js/loaders/GLTFLoader.js',
    'https://cdn.jsdelivr.net/npm/three@0.128.0/examples/js/loaders/STLLoader.js',
    'https://cdn.jsdelivr.net/npm/three@0.128.0/examples/js/loaders/OBJLoader.js',
    'https://cdn.jsdelivr.net/npm/three@0.128.0/examples/js/loaders/FBXLoader.js',
    'https://cdn.jsdelivr.net/npm/three@0.128.0/examples/js/loaders/FontLoader.js',
    'https://cdn.jsdelivr.net/npm/three@0.128.0/examples/js/geometries/TextGeometry.js',
  ];
  return new Promise(resolve => {
    let loaded = 0;
    function next() {
      if (loaded >= urls.length) {
        // Shim fflate for FBXLoader
        if (window.fflate && typeof THREE !== 'undefined') THREE.fflate = window.fflate;
        resolve();
        return;
      }
      const s = document.createElement('script');
      s.src = urls[loaded];
      s.onload = s.onerror = () => { loaded++; next(); };
      document.head.appendChild(s);
    }
    next();
  });
}

// Load MediaPipe lazily (only when user enables it)
function loadMediaPipeVision() {
  if (window.MediaPipeVision) return Promise.resolve();
  return import('https://cdn.jsdelivr.net/npm/@mediapipe/tasks-vision@0.10.32/vision_bundle.mjs').then(m => {
    window.MediaPipeVision = { FilesetResolver: m.FilesetResolver, HandLandmarker: m.HandLandmarker, FaceLandmarker: m.FaceLandmarker, PoseLandmarker: m.PoseLandmarker, ImageSegmenter: m.ImageSegmenter };
  });
}

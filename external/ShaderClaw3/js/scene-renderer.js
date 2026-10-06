// ShaderClaw — Three.js Scene Renderer

// ============================================================
// Three.js Scene Renderer
// ============================================================

class SceneRenderer {
  constructor(canvas) {
    this.canvas = canvas;
    this.renderer = null;
    this.sceneDef = null;
    this.playing = false;
    this.animId = null;
    this.startTime = performance.now();
    this.inputValues = {};
    this.inputs = [];
    this.media = []; // { name, type, threeTexture, threeModel }
    this._shaderBg = null; // { isfRenderer, texture } when shader bg active
  }

  load(sceneDef) {
    // Skip Three.js entirely on mobile — avoids stealing the single WebGL context
    const _isMob = window.innerWidth <= 900 || /Mobi|Android|iPhone/i.test(navigator.userAgent);
    if (_isMob) return;

    // Dispose old scene but keep the renderer (avoid creating multiple WebGL contexts)
    this.stop();
    if (this.sceneDef && this.sceneDef.dispose) this.sceneDef.dispose();
    this.sceneDef = null;
    this.inputs = [];
    this.inputValues = {};

    if (!this.renderer) {
      this.renderer = new THREE.WebGLRenderer({ canvas: this.canvas, antialias: true, preserveDrawingBuffer: true, powerPreference: 'high-performance', alpha: true, premultipliedAlpha: false });
      this.renderer.setPixelRatio(window.devicePixelRatio);
      this.renderer.setClearColor(0x000000, 0);
    }
    this.resize();
    this.sceneDef = sceneDef.create(this.renderer, this.canvas, this.media);
    this.inputs = sceneDef.INPUTS || [];
    this.startTime = performance.now();
  }

  render() {
    if (!this.sceneDef || !this.renderer) return;
    if (this.renderer.getContext().isContextLost()) return;
    // Update audio analysis (used by ISF bg shader and scene)
    if (this._isfGL) updateAudioUniforms(this._isfGL);
    // Drive ISF offscreen render and update texture before 3D render
    if (this._shaderBg) {
      this._shaderBg.isfRenderer.render();
      this._shaderBg.texture.needsUpdate = true;
    }
    const elapsed = (performance.now() - this.startTime) / 1000;
    // Inject mousePos so scenes can be interactive
    this.inputValues._mousePos = this._mainRenderer ? [this._mainRenderer.mousePos[0], this._mainRenderer.mousePos[1]] : [0.5, 0.5];
    // Inject audio data so scenes can be audio-reactive
    this.inputValues._audioLevel = audioLevel;
    this.inputValues._audioBass = audioBass;
    this.inputValues._audioMid = audioMid;
    this.inputValues._audioHigh = audioHigh;
    this.inputValues._audioFFTTexture = audioFFTThreeTexture;
    this.sceneDef.update(elapsed, this.inputValues, this.media);
    this.renderer.render(this.sceneDef.scene, this.sceneDef.camera);
  }

  start() {
    if (this.playing && this.animId) return;
    this.playing = true;
    const loop = () => {
      if (!this.playing) return;
      this.render();
      this.animId = requestAnimationFrame(loop);
    };
    loop();
  }

  stop() {
    this.playing = false;
    if (this.animId) cancelAnimationFrame(this.animId);
    this.animId = null;
  }

  togglePlay() {
    if (this.playing) this.stop();
    else this.start();
    return this.playing;
  }

  resize() {
    if (!this.renderer) return;
    const dpr = window.devicePixelRatio || 1;
    const parent = this.canvas.parentElement;
    const w = Math.round((parent ? parent.clientWidth : window.innerWidth) * dpr);
    const h = Math.round((parent ? parent.clientHeight : window.innerHeight) * dpr);
    this.renderer.setSize(w, h, false);
    this.canvas.style.width = (parent ? parent.clientWidth : window.innerWidth) + 'px';
    this.canvas.style.height = (parent ? parent.clientHeight : window.innerHeight) + 'px';
    if (this.sceneDef && this.sceneDef.resize) {
      this.sceneDef.resize(w, h);
    }
  }

  cleanup() {
    this.stop();
    if (this.sceneDef && this.sceneDef.dispose) {
      this.sceneDef.dispose();
    }
    if (this.renderer) {
      this.renderer.forceContextLoss();
      this.renderer.dispose();
      this.renderer = null;
    }
    this.sceneDef = null;
    this.inputs = [];
    this.inputValues = {};
  }

  resetTime() {
    this.startTime = performance.now();
  }
}

// makeDraggable removed — panels are now inline in sidebar

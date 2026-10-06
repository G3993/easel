// ============================================================
// ISF-to-Three.js Transpiler & Runtime
//
// Phase 2: Converts any ISF shader (including multi-pass with
// persistent ping-pong buffers) to run inside Three.js using
// RawShaderMaterial + WebGLRenderTarget chains.
//
// Usage:
//   const runtime = new ISFThreeRuntime(renderer);
//   runtime.load(isfSource);          // parse + compile
//   runtime.update(time, uniforms);   // per-frame uniform push
//   runtime.render();                 // execute all passes
//   runtime.getOutputTexture();       // final pass result
//   runtime.dispose();
// ============================================================

class ISFThreeRuntime {
  constructor(threeRenderer) {
    this.renderer = threeRenderer;
    this.material = null;
    this.quad = null;
    this.scene = null;
    this.camera = new THREE.OrthographicCamera(-1, 1, 1, -1, 0, 1);
    this.passes = [];
    this.uniforms = {};
    this.parsed = null;
    this.outputRT = null;
    this.frameIndex = 0;
    this.startTime = performance.now();
  }

  // Parse ISF source and create Three.js materials + render targets
  load(isfSource, width, height) {
    this.dispose();
    width = width || 1920;
    height = height || 1080;

    // Parse ISF header
    this.parsed = ISFThreeRuntime.parseISF(isfSource);
    if (!this.parsed.meta) {
      console.error('[ISFThreeRuntime] No ISF header found');
      return false;
    }

    // Build the transpiled fragment shader
    const frag = ISFThreeRuntime.buildFragmentShader(this.parsed);
    const vert = ISFThreeRuntime.VERT_SHADER;

    // Create uniforms object
    this.uniforms = {
      TIME:       { value: 0.0 },
      RENDERSIZE: { value: new THREE.Vector2(width, height) },
      PASSINDEX:  { value: 0 },
      FRAMEINDEX: { value: 0 },
      // Mouse
      mousePos:   { value: new THREE.Vector2(0.5, 0.5) },
      mouseDelta: { value: new THREE.Vector2(0, 0) },
      mouseDown:  { value: 0.0 },
      // Audio
      audioLevel: { value: 0.0 },
      audioBass:  { value: 0.0 },
      audioMid:   { value: 0.0 },
      audioHigh:  { value: 0.0 },
    };

    // Add per-input uniforms
    for (const inp of this.parsed.inputs) {
      const t = inp.TYPE;
      if (t === 'float' || t === 'long') {
        this.uniforms[inp.NAME] = { value: inp.DEFAULT !== undefined ? inp.DEFAULT : 0.0 };
      } else if (t === 'color') {
        const d = inp.DEFAULT || [0, 0, 0, 1];
        this.uniforms[inp.NAME] = { value: new THREE.Vector4(d[0], d[1], d[2], d[3]) };
      } else if (t === 'bool') {
        this.uniforms[inp.NAME] = { value: inp.DEFAULT || false };
      } else if (t === 'point2D') {
        const d = inp.DEFAULT || [0, 0];
        this.uniforms[inp.NAME] = { value: new THREE.Vector2(d[0], d[1]) };
      } else if (t === 'image') {
        this.uniforms[inp.NAME] = { value: null };
        this.uniforms['IMG_SIZE_' + inp.NAME] = { value: new THREE.Vector2(width, height) };
      }
    }

    // Setup passes (multi-pass with ping-pong render targets)
    const passesSpec = this.parsed.meta.PASSES || [{}];
    this.passes = [];

    for (let i = 0; i < passesSpec.length; i++) {
      const spec = passesSpec[i];
      const isFinal = !spec.TARGET;

      let pw = width, ph = height;
      if (spec.WIDTH) pw = ISFThreeRuntime._parseDim(spec.WIDTH, width, height);
      if (spec.HEIGHT) ph = ISFThreeRuntime._parseDim(spec.HEIGHT, width, height);

      const pass = {
        target: spec.TARGET || null,
        persistent: !!spec.PERSISTENT,
        width: pw,
        height: ph,
        isFinal: isFinal,
        // Ping-pong: two render targets, swap each frame
        rtA: null,
        rtB: null,
        current: 0, // 0 = write to B read from A, 1 = write to A read from B
      };

      if (!isFinal) {
        const rtOpts = {
          minFilter: THREE.LinearFilter,
          magFilter: THREE.LinearFilter,
          format: THREE.RGBAFormat,
          type: THREE.HalfFloatType, // needed for simulation signed values
          depthBuffer: false,
          stencilBuffer: false,
        };
        pass.rtA = new THREE.WebGLRenderTarget(pw, ph, rtOpts);
        pass.rtB = new THREE.WebGLRenderTarget(pw, ph, rtOpts);
        // Add as uniforms so shader can sample them
        this.uniforms[spec.TARGET] = { value: pass.rtA.texture };
      } else {
        // Final output render target
        this.outputRT = new THREE.WebGLRenderTarget(width, height, {
          minFilter: THREE.LinearFilter,
          magFilter: THREE.LinearFilter,
          format: THREE.RGBAFormat,
          type: THREE.UnsignedByteType,
          depthBuffer: false,
          stencilBuffer: false,
        });
      }

      this.passes.push(pass);
    }

    // Create material
    this.material = new THREE.RawShaderMaterial({
      vertexShader: vert,
      fragmentShader: frag,
      uniforms: this.uniforms,
      depthTest: false,
      depthWrite: false,
      transparent: true,
    });

    // Create fullscreen triangle
    const geom = new THREE.BufferGeometry();
    geom.setAttribute('position', new THREE.BufferAttribute(
      new Float32Array([-1, -1, 3, -1, -1, 3]), 2
    ));
    this.quad = new THREE.Mesh(geom, this.material);
    this.quad.frustumCulled = false;

    this.scene = new THREE.Scene();
    this.scene.add(this.quad);

    this.frameIndex = 0;
    this.startTime = performance.now();
    return true;
  }

  // Update uniforms before rendering
  update(time, values) {
    if (!this.material) return;

    this.uniforms.TIME.value = time !== undefined ? time : (performance.now() - this.startTime) / 1000;
    this.uniforms.FRAMEINDEX.value = this.frameIndex;

    if (values) {
      for (const [key, val] of Object.entries(values)) {
        if (this.uniforms[key]) {
          if (this.uniforms[key].value instanceof THREE.Vector4 && Array.isArray(val)) {
            this.uniforms[key].value.set(val[0], val[1], val[2], val[3] !== undefined ? val[3] : 1.0);
          } else if (this.uniforms[key].value instanceof THREE.Vector2 && Array.isArray(val)) {
            this.uniforms[key].value.set(val[0], val[1]);
          } else {
            this.uniforms[key].value = val;
          }
        }
      }
    }
  }

  // Execute all passes
  render() {
    if (!this.material || !this.renderer) return;

    for (let i = 0; i < this.passes.length; i++) {
      const pass = this.passes[i];

      // Set PASSINDEX and RENDERSIZE for this pass
      this.uniforms.PASSINDEX.value = i;

      if (pass.isFinal) {
        // Final pass → output render target
        this.uniforms.RENDERSIZE.value.set(this.outputRT.width, this.outputRT.height);
        this.renderer.setRenderTarget(this.outputRT);
      } else {
        // Intermediate pass → write to ping-pong target
        const writeRT = pass.current === 0 ? pass.rtB : pass.rtA;
        this.uniforms.RENDERSIZE.value.set(pass.width, pass.height);
        this.renderer.setRenderTarget(writeRT);

        if (!pass.persistent) {
          this.renderer.clear();
        }
      }

      // Bind all TARGET textures (read sides) before rendering
      for (const p of this.passes) {
        if (p.target && this.uniforms[p.target]) {
          const readRT = p.current === 0 ? p.rtA : p.rtB;
          this.uniforms[p.target].value = readRT.texture;
        }
      }

      this.renderer.render(this.scene, this.camera);

      // Swap ping-pong for persistent buffers
      if (!pass.isFinal && pass.persistent) {
        pass.current ^= 1;
      }
    }

    // Restore
    this.renderer.setRenderTarget(null);
    this.frameIndex++;
  }

  // Get the final composited output as a Three.js texture
  getOutputTexture() {
    return this.outputRT ? this.outputRT.texture : null;
  }

  // Get a named buffer's texture (for using intermediate passes)
  getBufferTexture(targetName) {
    for (const pass of this.passes) {
      if (pass.target === targetName) {
        return pass.current === 0 ? pass.rtA.texture : pass.rtB.texture;
      }
    }
    return null;
  }

  resize(width, height) {
    if (this.outputRT) {
      this.outputRT.setSize(width, height);
    }
    // Don't resize intermediate buffers — they have fixed dimensions
    // (e.g., fluid sim uses 256x256 regardless of screen size)
  }

  dispose() {
    for (const pass of this.passes) {
      if (pass.rtA) pass.rtA.dispose();
      if (pass.rtB) pass.rtB.dispose();
    }
    if (this.outputRT) this.outputRT.dispose();
    if (this.material) this.material.dispose();
    if (this.quad && this.quad.geometry) this.quad.geometry.dispose();
    this.passes = [];
    this.uniforms = {};
    this.material = null;
    this.quad = null;
    this.scene = null;
    this.outputRT = null;
    this.parsed = null;
  }

  // ============================================================
  // Static: ISF Parser (same logic as isf.js parseISF)
  // ============================================================
  static parseISF(source) {
    const match = source.match(/\/\*\s*(\{[\s\S]*?\})\s*\*\//);
    if (!match) return { meta: null, glsl: source.trim(), inputs: [] };
    try {
      const meta = JSON.parse(match[1]);
      const glsl = source.slice(source.indexOf(match[0]) + match[0].length).trim();
      return { meta, glsl, inputs: meta.INPUTS || [] };
    } catch (e) {
      return { meta: null, glsl: source.trim(), inputs: [], error: e.message };
    }
  }

  // ============================================================
  // Static: Build Three.js-compatible fragment shader from ISF
  // ============================================================
  static buildFragmentShader(parsed) {
    const lines = ['precision mediump float;', 'precision mediump int;', ''];

    // Varying from vertex shader
    lines.push('varying vec2 vUv;');
    lines.push('');

    // ISF standard uniforms
    lines.push('uniform float TIME;');
    lines.push('uniform vec2 RENDERSIZE;');
    lines.push('uniform int PASSINDEX;');
    lines.push('uniform int FRAMEINDEX;');
    lines.push('');

    // ISF compatibility macros
    lines.push('#define isf_FragNormCoord vUv');
    lines.push('#define IMG_NORM_PIXEL(img, coord) texture2D(img, coord)');
    lines.push('#define IMG_PIXEL(img, coord) texture2D(img, coord / RENDERSIZE)');
    lines.push('#define IMG_THIS_PIXEL(img) texture2D(img, vUv)');
    lines.push('#define IMG_THIS_NORM_PIXEL(img) texture2D(img, vUv)');
    lines.push('#define IMG_NORM_THIS_PIXEL(img) texture2D(img, vUv)');
    lines.push('');

    // Mouse uniforms
    lines.push('uniform vec2 mousePos;');
    lines.push('uniform vec2 mouseDelta;');
    lines.push('uniform float mouseDown;');
    lines.push('');

    // Audio uniforms
    lines.push('uniform float audioLevel;');
    lines.push('uniform float audioBass;');
    lines.push('uniform float audioMid;');
    lines.push('uniform float audioHigh;');
    lines.push('');

    // Per-input uniforms
    for (const inp of parsed.inputs) {
      const t = inp.TYPE;
      if (t === 'float' || t === 'long') lines.push(`uniform float ${inp.NAME};`);
      else if (t === 'color') lines.push(`uniform vec4 ${inp.NAME};`);
      else if (t === 'bool') lines.push(`uniform bool ${inp.NAME};`);
      else if (t === 'point2D') lines.push(`uniform vec2 ${inp.NAME};`);
      else if (t === 'image') {
        lines.push(`uniform sampler2D ${inp.NAME};`);
        lines.push(`uniform vec2 IMG_SIZE_${inp.NAME};`);
      }
    }
    lines.push('');

    // PASSES target samplers
    if (parsed.meta && Array.isArray(parsed.meta.PASSES)) {
      for (const p of parsed.meta.PASSES) {
        if (p.TARGET) lines.push(`uniform sampler2D ${p.TARGET};`);
      }
      lines.push('');
    }

    // Clean and append GLSL body
    let body = parsed.glsl
      .replace(/#version\s+\d+.*/g, '')
      .replace(/#ifdef\s+GL_ES\s*\r?\nprecision\s+\w+\s+float\s*;\s*\r?\n#endif\s*\r?\n?/g, '')
      .replace(/precision\s+(highp|mediump|lowp)\s+float\s*;/g, '');

    lines.push(body);

    return lines.join('\n');
  }

  // ============================================================
  // Static: Vertex shader (identical to ISF convention)
  // ============================================================
  static get VERT_SHADER() {
    return `precision mediump float;
attribute vec2 position;
varying vec2 vUv;
void main() {
    vUv = position * 0.5 + 0.5;
    gl_Position = vec4(position, 0.0, 1.0);
}`;
  }

  // Parse dimension expressions like "256", "$WIDTH/2"
  static _parseDim(val, w, h) {
    if (typeof val === 'number') return val;
    if (typeof val === 'string') {
      const s = val.replace(/\$WIDTH/g, String(w)).replace(/\$HEIGHT/g, String(h));
      try { return Math.round(Function('"use strict"; return (' + s + ')')()) || w; }
      catch (e) { return w; }
    }
    return w;
  }
}

// Export for use in scene files and tests
if (typeof window !== 'undefined') window.ISFThreeRuntime = ISFThreeRuntime;

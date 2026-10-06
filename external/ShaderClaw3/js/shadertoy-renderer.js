// ============================================================
// ShaderClaw — Shadertoy Renderer
// Runs Shadertoy shaders natively (single-pass and multi-buffer)
// Bypasses ISF entirely — no conversion, no wrapper interference
// ============================================================

class ShadertoyRenderer {
  constructor(gl, renderer) {
    this.gl = gl;
    this.renderer = renderer;
    this.active = false;

    // Buffer programs and FBOs
    // Each buffer: { program, uniforms, fbo: { read, write, swap() } }
    this.buffers = {};  // 'A', 'B', 'C', 'D'
    this.imageProgram = null;
    this.imageUniforms = {};

    this._frameCount = 0;
    this._startTime = performance.now();
    this._mouseState = { x: 0, y: 0, z: 0, w: 0 };

    // Noise texture for iChannel fallback
    this._noiseTex = null;
  }

  // ── Vertex shader (shared by all passes) ────────────────────

  static get VERT() {
    return `
      precision highp float;
      attribute vec2 position;
      void main() {
        gl_Position = vec4(position, 0.0, 1.0);
      }
    `;
  }

  // ── Shadertoy preamble (injected before each pass) ──────────

  static preamble(bufferSamplers) {
    // bufferSamplers: { A: 'bufA', B: 'bufB', ... } — maps channel refs to sampler names
    let s = `
precision highp float;
precision highp int;
uniform vec3 iResolution;
uniform float iTime;
uniform float iTimeDelta;
uniform int iFrame;
uniform vec4 iMouse;
uniform vec4 iDate;
uniform float iSampleRate;
uniform vec4 iChannelResolution[4];
uniform float iChannelTime[4];
`;
    // Declare all buffer samplers
    for (const name of Object.values(bufferSamplers)) {
      s += `uniform sampler2D ${name};\n`;
    }
    s += `uniform sampler2D _noiseTex;\n`;

    // texelFetch polyfill for WebGL1
    s += `
vec4 texelFetch(sampler2D s, ivec2 c, int lod) {
  return texture2D(s, (vec2(c) + 0.5) / iResolution.xy);
}
`;
    return s;
  }

  // ── Compile a Shadertoy shader ──────────────────────────────

  /**
   * Compile from raw Shadertoy pass sources
   * @param {Object} passes - { image: string, bufferA?: string, bufferB?: string, ... }
   * @param {Object} channels - per-pass channel bindings:
   *   { image: { 0: 'A', 1: 'noise' }, bufferA: { 0: 'A', 1: 'noise' }, ... }
   *   Values: 'A','B','C','D' for buffers, 'noise' for procedural noise, 'self' for self-feedback
   */
  compile(passes, channels) {
    const gl = this.gl;
    this.destroy();

    // Create noise texture
    this._createNoiseTex();

    // Determine which buffers exist
    const bufferNames = [];
    for (const key of ['bufferA', 'bufferB', 'bufferC', 'bufferD']) {
      if (passes[key]) bufferNames.push(key.slice(6)); // 'A', 'B', 'C', 'D'
    }

    // All sampler names (for uniform declarations)
    const samplerMap = {};
    bufferNames.forEach(letter => { samplerMap[letter] = 'buf' + letter; });

    // Create FBOs for each buffer
    const w = gl.canvas.width || 1920;
    const h = gl.canvas.height || 1080;
    for (const letter of bufferNames) {
      this.buffers[letter] = {
        program: null,
        uniforms: {},
        channelBindings: {},
        fbo: this._createDoubleFBO(w, h)
      };
    }

    // Compile buffer programs
    for (const letter of bufferNames) {
      const passKey = 'buffer' + letter;
      const src = passes[passKey];
      const chMap = channels[passKey] || {};

      // Build iChannel declarations
      let channelDecls = '';
      const bindings = {};
      for (let ch = 0; ch < 4; ch++) {
        const binding = chMap[ch];
        let samplerName;
        if (binding && samplerMap[binding]) {
          samplerName = samplerMap[binding];
        } else if (binding === 'self') {
          samplerName = samplerMap[letter]; // self-feedback
        } else if (binding === 'noise') {
          samplerName = '_noiseTex';
        } else if (!binding) {
          samplerName = '_noiseTex'; // default to noise
        } else {
          samplerName = '_noiseTex';
        }
        channelDecls += `#define iChannel${ch} ${samplerName}\n`;
        bindings[ch] = samplerName;
      }

      const frag = ShadertoyRenderer.preamble(samplerMap) + channelDecls + '\n' +
        src + '\n' +
        'void main() { vec4 c = vec4(0.0); mainImage(c, gl_FragCoord.xy); gl_FragColor = c; }\n';

      const result = this._compileProgram(frag);
      if (!result) {
        console.error('[ShadertoyRenderer] Buffer ' + letter + ' compile failed');
        this.destroy();
        return false;
      }
      this.buffers[letter].program = result.program;
      this.buffers[letter].uniforms = result.uniforms;
      this.buffers[letter].channelBindings = bindings;
    }

    // Compile image program
    {
      const src = passes.image;
      const chMap = channels.image || {};
      let channelDecls = '';
      const bindings = {};
      for (let ch = 0; ch < 4; ch++) {
        const binding = chMap[ch];
        let samplerName;
        if (binding && samplerMap[binding]) {
          samplerName = samplerMap[binding];
        } else if (binding === 'noise') {
          samplerName = '_noiseTex';
        } else if (!binding) {
          samplerName = '_noiseTex';
        } else {
          samplerName = '_noiseTex';
        }
        channelDecls += `#define iChannel${ch} ${samplerName}\n`;
        bindings[ch] = samplerName;
      }

      const frag = ShadertoyRenderer.preamble(samplerMap) + channelDecls + '\n' +
        src + '\n' +
        'void main() { vec4 c = vec4(0.0); mainImage(c, gl_FragCoord.xy); gl_FragColor = c; }\n';

      const result = this._compileProgram(frag);
      if (!result) {
        console.error('[ShadertoyRenderer] Image pass compile failed');
        this.destroy();
        return false;
      }
      this.imageProgram = result.program;
      this.imageUniforms = result.uniforms;
      this._imageChannelBindings = bindings;
    }

    this.active = true;
    this._startTime = performance.now();
    this._frameCount = 0;
    console.log('[ShadertoyRenderer] Compiled OK — buffers:', bufferNames.join(',') || 'none', '+ image');
    return true;
  }

  // ── Per-frame update + render ───────────────────────────────

  update(mousePos, mouseDelta, mouseDown) {
    if (!this.active) return;
    const gl = this.gl;
    const w = gl.canvas.width;
    const h = gl.canvas.height;

    // Update mouse state
    if (mouseDown) {
      this._mouseState.z = mousePos[0] * w;
      this._mouseState.w = mousePos[1] * h;
    }
    this._mouseState.x = mousePos[0] * w;
    this._mouseState.y = mousePos[1] * h;

    const time = (performance.now() - this._startTime) / 1000;

    gl.disable(gl.BLEND);

    // Render each buffer in order
    for (const letter of ['A', 'B', 'C', 'D']) {
      const buf = this.buffers[letter];
      if (!buf || !buf.program) continue;

      gl.useProgram(buf.program);
      this._setCommonUniforms(buf.uniforms, buf.program, time, w, h);
      this._bindChannels(buf.channelBindings, buf.uniforms, letter);

      // Render to write FBO
      const target = buf.fbo.write;
      gl.bindFramebuffer(gl.FRAMEBUFFER, target.fbo);
      gl.viewport(0, 0, target.width, target.height);

      gl.bindBuffer(gl.ARRAY_BUFFER, this.renderer.posBuf);
      gl.enableVertexAttribArray(0);
      gl.vertexAttribPointer(0, 2, gl.FLOAT, false, 0, 0);
      gl.drawArrays(gl.TRIANGLES, 0, 3);

      buf.fbo.swap();
    }

    this._frameCount++;
  }

  renderToFBO(targetFBO) {
    if (!this.active || !this.imageProgram) return;
    const gl = this.gl;
    const w = targetFBO.width;
    const h = targetFBO.height;
    const time = (performance.now() - this._startTime) / 1000;

    gl.disable(gl.BLEND);
    gl.useProgram(this.imageProgram);
    this._setCommonUniforms(this.imageUniforms, this.imageProgram, time, w, h);
    this._bindChannels(this._imageChannelBindings, this.imageUniforms, null);

    gl.bindFramebuffer(gl.FRAMEBUFFER, targetFBO.fbo);
    gl.viewport(0, 0, w, h);

    gl.bindBuffer(gl.ARRAY_BUFFER, this.renderer.posBuf);
    gl.enableVertexAttribArray(0);
    gl.vertexAttribPointer(0, 2, gl.FLOAT, false, 0, 0);
    gl.drawArrays(gl.TRIANGLES, 0, 3);
  }

  // ── Set Shadertoy uniforms ──────────────────────────────────

  _setCommonUniforms(uniforms, program, time, w, h) {
    const gl = this.gl;
    const u = (name) => uniforms[name];

    if (u('iResolution') != null) gl.uniform3f(u('iResolution'), w, h, 1.0);
    if (u('iTime') != null) gl.uniform1f(u('iTime'), time);
    if (u('iTimeDelta') != null) gl.uniform1f(u('iTimeDelta'), 1.0 / 60.0);
    if (u('iFrame') != null) gl.uniform1i(u('iFrame'), this._frameCount);
    if (u('iMouse') != null) {
      gl.uniform4f(u('iMouse'), this._mouseState.x, this._mouseState.y,
        this._mouseState.z, this._mouseState.w);
    }
    if (u('iSampleRate') != null) gl.uniform1f(u('iSampleRate'), 44100.0);
    if (u('iDate') != null) {
      const d = new Date();
      gl.uniform4f(u('iDate'), d.getFullYear(), d.getMonth(), d.getDate(),
        d.getHours() * 3600 + d.getMinutes() * 60 + d.getSeconds());
    }
    if (u('iChannelResolution[0]') != null) {
      for (let i = 0; i < 4; i++) {
        const loc = uniforms['iChannelResolution[' + i + ']'];
        if (loc != null) gl.uniform4f(loc, w, h, 1.0, 1.0);
      }
    }
  }

  _bindChannels(bindings, uniforms, currentBuffer) {
    const gl = this.gl;
    let unit = 0;
    for (let ch = 0; ch < 4; ch++) {
      const samplerName = bindings[ch];
      if (!samplerName) continue;

      const loc = uniforms[samplerName];
      if (loc == null) continue;

      gl.activeTexture(gl.TEXTURE0 + unit);

      if (samplerName === '_noiseTex') {
        gl.bindTexture(gl.TEXTURE_2D, this._noiseTex);
      } else {
        // Buffer reference — bind the read side of that buffer's FBO
        const letter = samplerName.replace('buf', '');
        const buf = this.buffers[letter];
        if (buf && buf.fbo) {
          gl.bindTexture(gl.TEXTURE_2D, buf.fbo.read.texture);
        } else {
          gl.bindTexture(gl.TEXTURE_2D, this._noiseTex);
        }
      }

      gl.uniform1i(loc, unit);
      unit++;
    }
  }

  // ── Shader compilation ──────────────────────────────────────

  _compileProgram(fragSrc) {
    const gl = this.gl;
    const vs = gl.createShader(gl.VERTEX_SHADER);
    gl.shaderSource(vs, ShadertoyRenderer.VERT);
    gl.compileShader(vs);
    if (!gl.getShaderParameter(vs, gl.COMPILE_STATUS)) {
      console.error('[ShadertoyRenderer] Vert error:', gl.getShaderInfoLog(vs));
      gl.deleteShader(vs);
      return null;
    }

    const fs = gl.createShader(gl.FRAGMENT_SHADER);
    gl.shaderSource(fs, fragSrc);
    gl.compileShader(fs);
    if (!gl.getShaderParameter(fs, gl.COMPILE_STATUS)) {
      const log = gl.getShaderInfoLog(fs);
      console.error('[ShadertoyRenderer] Frag error:', log);
      // Show first few lines of error
      window._lastShadertoyError = log;
      gl.deleteShader(vs);
      gl.deleteShader(fs);
      return null;
    }

    const prog = gl.createProgram();
    gl.attachShader(prog, vs);
    gl.attachShader(prog, fs);
    gl.bindAttribLocation(prog, 0, 'position');
    gl.linkProgram(prog);
    gl.deleteShader(vs);
    gl.deleteShader(fs);

    if (!gl.getProgramParameter(prog, gl.LINK_STATUS)) {
      console.error('[ShadertoyRenderer] Link error:', gl.getProgramInfoLog(prog));
      gl.deleteProgram(prog);
      return null;
    }

    // Cache uniforms
    const uniforms = {};
    const count = gl.getProgramParameter(prog, gl.ACTIVE_UNIFORMS);
    for (let i = 0; i < count; i++) {
      const info = gl.getActiveUniform(prog, i);
      uniforms[info.name] = gl.getUniformLocation(prog, info.name);
    }

    return { program: prog, uniforms };
  }

  // ── FBO management ──────────────────────────────────────────

  _createSingleFBO(w, h) {
    const gl = this.gl;
    const fbo = gl.createFramebuffer();
    const tex = gl.createTexture();
    gl.bindTexture(gl.TEXTURE_2D, tex);
    // Use UNSIGNED_BYTE + LINEAR (safe for WebGL1)
    gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, w, h, 0, gl.RGBA, gl.UNSIGNED_BYTE, null);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
    gl.bindFramebuffer(gl.FRAMEBUFFER, fbo);
    gl.framebufferTexture2D(gl.FRAMEBUFFER, gl.COLOR_ATTACHMENT0, gl.TEXTURE_2D, tex, 0);
    gl.bindFramebuffer(gl.FRAMEBUFFER, null);
    return { fbo, texture: tex, width: w, height: h };
  }

  _createDoubleFBO(w, h) {
    return {
      read: this._createSingleFBO(w, h),
      write: this._createSingleFBO(w, h),
      swap() { const tmp = this.read; this.read = this.write; this.write = tmp; }
    };
  }

  _createNoiseTex() {
    const gl = this.gl;
    const size = 256;
    const data = new Uint8Array(size * size * 4);
    for (let i = 0; i < data.length; i++) {
      data[i] = Math.random() * 255;
    }
    this._noiseTex = gl.createTexture();
    gl.bindTexture(gl.TEXTURE_2D, this._noiseTex);
    gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, size, size, 0, gl.RGBA, gl.UNSIGNED_BYTE, data);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.REPEAT);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.REPEAT);
  }

  // ── Cleanup ─────────────────────────────────────────────────

  destroy() {
    const gl = this.gl;
    this.active = false;

    for (const letter in this.buffers) {
      const buf = this.buffers[letter];
      if (buf.program) gl.deleteProgram(buf.program);
      if (buf.fbo) {
        if (buf.fbo.read) { gl.deleteTexture(buf.fbo.read.texture); gl.deleteFramebuffer(buf.fbo.read.fbo); }
        if (buf.fbo.write) { gl.deleteTexture(buf.fbo.write.texture); gl.deleteFramebuffer(buf.fbo.write.fbo); }
      }
    }
    this.buffers = {};

    if (this.imageProgram) { gl.deleteProgram(this.imageProgram); this.imageProgram = null; }
    if (this._noiseTex) { gl.deleteTexture(this._noiseTex); this._noiseTex = null; }
    this.imageUniforms = {};
    this._imageChannelBindings = {};
  }
}

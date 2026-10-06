// ============================================================
// ShaderClaw — Fluid Simulation Renderer
// GPU-accelerated Navier-Stokes solver (WebGL 1)
// Based on Pavel Dobryakov's WebGL-Fluid-Simulation (MIT)
// ============================================================

class FluidRenderer {
  constructor(gl, renderer) {
    this.gl = gl;
    this.renderer = renderer; // parent Renderer (for posBuf, defaultTex)
    this.active = false;

    // Cap dye resolution on mobile to avoid OOM
    const _isMobile = /Mobi|Android|iPhone|iPad/i.test(navigator.userAgent);
    const _defaultDyeRes = _isMobile ? 512 : 2048;

    // Simulation config (exposed as UI controls)
    this.config = {
      SIM_RESOLUTION: _isMobile ? 128 : 256,
      DYE_RESOLUTION: _defaultDyeRes,
      LIFE: 8.45,              // 0 = instant fade, 10 = lives forever
      DENSITY_DISSIPATION: 1.0, // derived from LIFE
      VELOCITY_DISSIPATION: 4.0,
      PRESSURE: 1.0,
      PRESSURE_ITERATIONS: 20,
      CURL: 0,
      SPLAT_RADIUS: 1.0,
      SPLAT_FORCE: 20000,
      COLORFUL: true,
      COLOR_UPDATE_SPEED: 10,
      BACK_COLOR: [0, 0, 0],
      TRANSPARENT: true,
      BLOOM: false,
      SHADING: true,
      MOVEMENT: true,
      MOVE_PATTERN: 0,     // 0=Freeform, 1=Center, 2=Wave, 3=Vortex, 4=Pulse
      MOVE_SPEED: 0.5,
      MOVE_SPREAD: 0.7,
      MOVE_INTENSITY: 0.5,
    };
    // Sync initial dissipation from life
    this.config.DENSITY_DISSIPATION = this._lifeToDissipation(this.config.LIFE);

    // Simulation state
    this._programs = {};
    this._velocity = null;
    this._dye = null;
    this._divergence = null;
    this._curl = null;
    this._pressure = null;
    this._displayFBO = null;
    this._lastTime = 0;
    this._splatQueue = [];
    this._pointerColor = { r: 0.91, g: 0.25, b: 0.34 }; // ShaderClaw red
    this._movementTime = 0;
    // Virtual cursors for movement patterns — track position for continuous delta-based splats
    this._moveCursors = [
      { x: 0.5, y: 0.5, prevX: 0.5, prevY: 0.5, color: null },
      { x: 0.5, y: 0.5, prevX: 0.5, prevY: 0.5, color: null },
      { x: 0.5, y: 0.5, prevX: 0.5, prevY: 0.5, color: null },
    ];
    // Pinch-to-fade state
    this.pinchFading = false; // true while any hand is pinching
    this.pinchOpacity = 1.0;  // current fade level (1 = full, 0 = invisible)
    this._colorTimer = 0;

    // Half-float support (reuse renderer's detection)
    this._halfFloatExt = null;
    this._halfFloatType = null;
    this._supportLinearFloat = false;
    this._manualFiltering = false;
  }

  // ── Initialization ──────────────────────────────────────────

  init() {
    const gl = this.gl;

    // Probe float texture support
    this._halfFloatExt = gl.getExtension('OES_texture_half_float');
    this._halfFloatLinear = gl.getExtension('OES_texture_half_float_linear');
    gl.getExtension('EXT_color_buffer_half_float');

    if (this._halfFloatExt) {
      this._halfFloatType = this._halfFloatExt.HALF_FLOAT_OES;
      this._supportLinearFloat = !!this._halfFloatLinear;
    } else {
      this._halfFloatType = gl.UNSIGNED_BYTE;
      this._supportLinearFloat = true;
    }

    this._manualFiltering = !this._supportLinearFloat;

    // Compile all shader programs
    this._compilePrograms();

    // Create simulation FBOs
    this._initFBOs();

    this.active = true;
    this._lastTime = performance.now();

    console.log('[FluidRenderer] Initialized — halfFloat:', !!this._halfFloatExt,
      'linearFilter:', this._supportLinearFloat,
      'simRes:', this.config.SIM_RESOLUTION,
      'dyeRes:', this.config.DYE_RESOLUTION);
  }

  destroy() {
    this.active = false;
    const gl = this.gl;
    // Destroy FBOs
    this._destroyDoubleFBO(this._velocity);
    this._destroyDoubleFBO(this._dye);
    this._destroyDoubleFBO(this._pressure);
    this._destroySingleFBO(this._divergence);
    this._destroySingleFBO(this._curl);
    this._destroySingleFBO(this._displayFBO);
    // Destroy programs
    for (const key in this._programs) {
      if (this._programs[key] && this._programs[key].program) {
        gl.deleteProgram(this._programs[key].program);
      }
    }
    this._programs = {};
  }

  // ── Shader compilation ──────────────────────────────────────

  _compilePrograms() {
    const gl = this.gl;
    const manualKW = this._manualFiltering ? '#define MANUAL_FILTERING\n' : '';

    // Base vertex shader (pre-computes neighbor UVs)
    const baseVert = `
      precision highp float;
      attribute vec2 position;
      varying vec2 vUv;
      varying vec2 vL, vR, vT, vB;
      uniform vec2 texelSize;
      void main() {
        vUv = position * 0.5 + 0.5;
        vL = vUv - vec2(texelSize.x, 0.0);
        vR = vUv + vec2(texelSize.x, 0.0);
        vT = vUv + vec2(0.0, texelSize.y);
        vB = vUv - vec2(0.0, texelSize.y);
        gl_Position = vec4(position, 0.0, 1.0);
      }
    `;

    // Simple vertex (no neighbor UVs)
    const simpleVert = `
      precision highp float;
      attribute vec2 position;
      varying vec2 vUv;
      void main() {
        vUv = position * 0.5 + 0.5;
        gl_Position = vec4(position, 0.0, 1.0);
      }
    `;

    // ── Fragment shaders ──

    const splatFrag = `
      precision highp float;
      varying vec2 vUv;
      uniform sampler2D uTarget;
      uniform float aspectRatio;
      uniform vec3 color;
      uniform vec2 point;
      uniform float radius;
      uniform float isDye;
      void main() {
        vec2 p = vUv - point.xy;
        p.x *= aspectRatio;
        vec3 splat = exp(-dot(p, p) / radius) * color;
        vec3 base = texture2D(uTarget, vUv).xyz;
        vec3 result;
        if (isDye > 0.5) {
          // Screen blend for dye: colors accumulate richly but never wash to white
          // result = 1 - (1-base) * (1-splat) = base + splat - base*splat
          result = base + splat * (vec3(1.0) - base);
        } else {
          // Velocity: pure additive (needs negative values for physics)
          result = base + splat;
        }
        gl_FragColor = vec4(result, 1.0);
      }
    `;

    const advectionFrag = manualKW + `
      precision highp float;
      varying vec2 vUv;
      uniform sampler2D uVelocity;
      uniform sampler2D uSource;
      uniform vec2 texelSize;
      uniform vec2 dyeTexelSize;
      uniform float dt;
      uniform float dissipation;
      vec4 bilerp(sampler2D sam, vec2 uv, vec2 tsize) {
        vec2 st = uv / tsize - 0.5;
        vec2 iuv = floor(st);
        vec2 fuv = fract(st);
        vec4 a = texture2D(sam, (iuv + vec2(0.5, 0.5)) * tsize);
        vec4 b = texture2D(sam, (iuv + vec2(1.5, 0.5)) * tsize);
        vec4 c = texture2D(sam, (iuv + vec2(0.5, 1.5)) * tsize);
        vec4 d = texture2D(sam, (iuv + vec2(1.5, 1.5)) * tsize);
        return mix(mix(a, b, fuv.x), mix(c, d, fuv.x), fuv.y);
      }
      void main() {
        #ifdef MANUAL_FILTERING
          vec2 coord = vUv - dt * bilerp(uVelocity, vUv, texelSize).xy * texelSize;
          vec4 result = bilerp(uSource, coord, dyeTexelSize);
        #else
          vec2 coord = vUv - dt * texture2D(uVelocity, vUv).xy * texelSize;
          vec4 result = texture2D(uSource, coord);
        #endif
        float decay = 1.0 + dissipation * dt;
        gl_FragColor = result / decay;
      }
    `;

    const divergenceFrag = `
      precision mediump float;
      varying highp vec2 vUv;
      varying highp vec2 vL, vR, vT, vB;
      uniform sampler2D uVelocity;
      void main() {
        float L = texture2D(uVelocity, vL).x;
        float R = texture2D(uVelocity, vR).x;
        float T = texture2D(uVelocity, vT).y;
        float B = texture2D(uVelocity, vB).y;
        vec2 C = texture2D(uVelocity, vUv).xy;
        if (vL.x < 0.0) L = -C.x;
        if (vR.x > 1.0) R = -C.x;
        if (vT.y > 1.0) T = -C.y;
        if (vB.y < 0.0) B = -C.y;
        float div = 0.5 * (R - L + T - B);
        gl_FragColor = vec4(div, 0.0, 0.0, 1.0);
      }
    `;

    const curlFrag = `
      precision mediump float;
      varying highp vec2 vUv;
      varying highp vec2 vL, vR, vT, vB;
      uniform sampler2D uVelocity;
      void main() {
        float L = texture2D(uVelocity, vL).y;
        float R = texture2D(uVelocity, vR).y;
        float T = texture2D(uVelocity, vT).x;
        float B = texture2D(uVelocity, vB).x;
        float vorticity = R - L - T + B;
        gl_FragColor = vec4(0.5 * vorticity, 0.0, 0.0, 1.0);
      }
    `;

    const vorticityFrag = `
      precision highp float;
      varying vec2 vUv;
      varying vec2 vL, vR, vT, vB;
      uniform sampler2D uVelocity;
      uniform sampler2D uCurl;
      uniform float curl;
      uniform float dt;
      void main() {
        float L = texture2D(uCurl, vL).x;
        float R = texture2D(uCurl, vR).x;
        float T = texture2D(uCurl, vT).x;
        float B = texture2D(uCurl, vB).x;
        float C = texture2D(uCurl, vUv).x;
        vec2 force = 0.5 * vec2(abs(T) - abs(B), abs(R) - abs(L));
        force /= length(force) + 0.0001;
        force *= curl * C;
        force.y *= -1.0;
        vec2 velocity = texture2D(uVelocity, vUv).xy;
        velocity += force * dt;
        velocity = min(max(velocity, -1000.0), 1000.0);
        gl_FragColor = vec4(velocity, 0.0, 1.0);
      }
    `;

    const pressureFrag = `
      precision mediump float;
      varying highp vec2 vUv;
      varying highp vec2 vL, vR, vT, vB;
      uniform sampler2D uPressure;
      uniform sampler2D uDivergence;
      void main() {
        float L = texture2D(uPressure, vL).x;
        float R = texture2D(uPressure, vR).x;
        float T = texture2D(uPressure, vT).x;
        float B = texture2D(uPressure, vB).x;
        float divergence = texture2D(uDivergence, vUv).x;
        float pressure = (L + R + B + T - divergence) * 0.25;
        gl_FragColor = vec4(pressure, 0.0, 0.0, 1.0);
      }
    `;

    const gradSubFrag = `
      precision mediump float;
      varying highp vec2 vUv;
      varying highp vec2 vL, vR, vT, vB;
      uniform sampler2D uPressure;
      uniform sampler2D uVelocity;
      void main() {
        float L = texture2D(uPressure, vL).x;
        float R = texture2D(uPressure, vR).x;
        float T = texture2D(uPressure, vT).x;
        float B = texture2D(uPressure, vB).x;
        vec2 velocity = texture2D(uVelocity, vUv).xy;
        velocity.xy -= vec2(R - L, T - B);
        gl_FragColor = vec4(velocity, 0.0, 1.0);
      }
    `;

    const clearFrag = `
      precision mediump float;
      varying highp vec2 vUv;
      uniform sampler2D uTexture;
      uniform float value;
      void main() {
        gl_FragColor = value * texture2D(uTexture, vUv);
      }
    `;

    const displayFrag = `
      precision highp float;
      varying vec2 vUv;
      uniform sampler2D uTexture;
      uniform sampler2D uVelocity;
      uniform vec2 texelSize;
      uniform float shading;

      // Smooth bilinear sample for low-res velocity field
      vec2 sampleVelocity(vec2 uv) {
        vec2 st = uv / texelSize - 0.5;
        vec2 iuv = floor(st);
        vec2 fuv = fract(st);
        vec2 a = texture2D(uVelocity, (iuv + vec2(0.5, 0.5)) * texelSize).xy;
        vec2 b = texture2D(uVelocity, (iuv + vec2(1.5, 0.5)) * texelSize).xy;
        vec2 c = texture2D(uVelocity, (iuv + vec2(0.5, 1.5)) * texelSize).xy;
        vec2 d = texture2D(uVelocity, (iuv + vec2(1.5, 1.5)) * texelSize).xy;
        return mix(mix(a, b, fuv.x), mix(c, d, fuv.x), fuv.y);
      }

      void main() {
        vec4 c = texture2D(uTexture, vUv);
        if (shading > 0.5) {
          // Use smooth bilinear sampling for the low-res velocity field
          vec2 lv = sampleVelocity(vUv - vec2(texelSize.x, 0.0));
          vec2 rv = sampleVelocity(vUv + vec2(texelSize.x, 0.0));
          vec2 tv = sampleVelocity(vUv + vec2(0.0, texelSize.y));
          vec2 bv = sampleVelocity(vUv - vec2(0.0, texelSize.y));
          float vort = rv.y - lv.y - tv.x + bv.x;
          // Soft clamp to prevent harsh artifacts at high curl
          vort = clamp(vort * 0.01, -0.15, 0.15);
          c.rgb += vec3(vort);
        }
        float lum = max(c.r, max(c.g, c.b));
        gl_FragColor = vec4(c.rgb, lum);
      }
    `;

    const copyFrag = `
      precision mediump float;
      varying highp vec2 vUv;
      uniform sampler2D uTexture;
      void main() {
        gl_FragColor = texture2D(uTexture, vUv);
      }
    `;

    // Compile all programs
    this._programs.splat = this._createProgram(simpleVert, splatFrag);
    this._programs.advection = this._createProgram(baseVert, advectionFrag);
    this._programs.divergence = this._createProgram(baseVert, divergenceFrag);
    this._programs.curl = this._createProgram(baseVert, curlFrag);
    this._programs.vorticity = this._createProgram(baseVert, vorticityFrag);
    this._programs.pressure = this._createProgram(baseVert, pressureFrag);
    this._programs.gradientSubtract = this._createProgram(baseVert, gradSubFrag);
    this._programs.clear = this._createProgram(simpleVert, clearFrag);
    this._programs.display = this._createProgram(simpleVert, displayFrag);
    this._programs.copy = this._createProgram(simpleVert, copyFrag);
  }

  _createProgram(vertSrc, fragSrc) {
    const gl = this.gl;
    const vs = gl.createShader(gl.VERTEX_SHADER);
    gl.shaderSource(vs, vertSrc);
    gl.compileShader(vs);
    if (!gl.getShaderParameter(vs, gl.COMPILE_STATUS)) {
      console.error('[FluidRenderer] Vertex shader error:', gl.getShaderInfoLog(vs));
      gl.deleteShader(vs);
      return null;
    }

    const fs = gl.createShader(gl.FRAGMENT_SHADER);
    gl.shaderSource(fs, fragSrc);
    gl.compileShader(fs);
    if (!gl.getShaderParameter(fs, gl.COMPILE_STATUS)) {
      console.error('[FluidRenderer] Fragment shader error:', gl.getShaderInfoLog(fs));
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
      console.error('[FluidRenderer] Link error:', gl.getProgramInfoLog(prog));
      gl.deleteProgram(prog);
      return null;
    }

    // Cache uniform locations
    const uniforms = {};
    const count = gl.getProgramParameter(prog, gl.ACTIVE_UNIFORMS);
    for (let i = 0; i < count; i++) {
      const info = gl.getActiveUniform(prog, i);
      uniforms[info.name] = gl.getUniformLocation(prog, info.name);
    }

    return { program: prog, uniforms };
  }

  // ── FBO management ──────────────────────────────────────────

  _initFBOs() {
    const simRes = this._getResolution(this.config.SIM_RESOLUTION);
    const dyeRes = this._getResolution(this.config.DYE_RESOLUTION);

    // Sim FBOs need half-float for negative values (velocity, pressure)
    this._velocity = this._createDoubleFBO(simRes.width, simRes.height, true);
    this._pressure = this._createDoubleFBO(simRes.width, simRes.height, true);
    this._divergence = this._createSingleFBO(simRes.width, simRes.height, true);
    this._curl = this._createSingleFBO(simRes.width, simRes.height, true);
    // Dye FBO stores 0-1 colors — always use UNSIGNED_BYTE + LINEAR for smooth display
    this._dye = this._createDoubleFBO(dyeRes.width, dyeRes.height, false);
  }

  _getResolution(resolution) {
    let aspectRatio = this.gl.canvas.width / this.gl.canvas.height;
    if (aspectRatio < 1) aspectRatio = 1.0 / aspectRatio;
    const min = Math.round(resolution);
    const max = Math.round(resolution * aspectRatio);
    if (this.gl.canvas.width > this.gl.canvas.height) {
      return { width: max, height: min };
    }
    return { width: min, height: max };
  }

  _createSingleFBO(w, h, needsHalfFloat) {
    const gl = this.gl;
    const fbo = gl.createFramebuffer();
    const tex = gl.createTexture();
    gl.bindTexture(gl.TEXTURE_2D, tex);

    // Dye/display FBOs always use UNSIGNED_BYTE + LINEAR for smooth rendering
    // Sim FBOs (velocity, pressure, etc.) need half-float for negative values
    const type = (needsHalfFloat && this._halfFloatExt) ? this._halfFloatType : gl.UNSIGNED_BYTE;
    const filter = (needsHalfFloat && !this._supportLinearFloat) ? gl.NEAREST : gl.LINEAR;

    gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, w, h, 0, gl.RGBA, type, null);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, filter);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, filter);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);

    gl.bindFramebuffer(gl.FRAMEBUFFER, fbo);
    gl.framebufferTexture2D(gl.FRAMEBUFFER, gl.COLOR_ATTACHMENT0, gl.TEXTURE_2D, tex, 0);

    // Check completeness, fall back to UNSIGNED_BYTE
    if (gl.checkFramebufferStatus(gl.FRAMEBUFFER) !== gl.FRAMEBUFFER_COMPLETE) {
      gl.deleteTexture(tex);
      const tex2 = gl.createTexture();
      gl.bindTexture(gl.TEXTURE_2D, tex2);
      gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, w, h, 0, gl.RGBA, gl.UNSIGNED_BYTE, null);
      gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
      gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
      gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
      gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
      gl.framebufferTexture2D(gl.FRAMEBUFFER, gl.COLOR_ATTACHMENT0, gl.TEXTURE_2D, tex2, 0);
      gl.bindFramebuffer(gl.FRAMEBUFFER, null);
      return { fbo, texture: tex2, width: w, height: h };
    }

    gl.bindFramebuffer(gl.FRAMEBUFFER, null);
    return { fbo, texture: tex, width: w, height: h };
  }

  _createDoubleFBO(w, h, needsHalfFloat) {
    return {
      read: this._createSingleFBO(w, h, needsHalfFloat),
      write: this._createSingleFBO(w, h, needsHalfFloat),
      width: w,
      height: h,
      swap() { const tmp = this.read; this.read = this.write; this.write = tmp; }
    };
  }

  _destroySingleFBO(fbo) {
    if (!fbo) return;
    const gl = this.gl;
    if (fbo.texture) gl.deleteTexture(fbo.texture);
    if (fbo.fbo) gl.deleteFramebuffer(fbo.fbo);
  }

  _destroyDoubleFBO(dfbo) {
    if (!dfbo) return;
    this._destroySingleFBO(dfbo.read);
    this._destroySingleFBO(dfbo.write);
  }

  // ── Blit helper ─────────────────────────────────────────────

  _blit(targetFBO) {
    const gl = this.gl;
    if (targetFBO) {
      gl.bindFramebuffer(gl.FRAMEBUFFER, targetFBO.fbo);
      gl.viewport(0, 0, targetFBO.width, targetFBO.height);
    } else {
      gl.bindFramebuffer(gl.FRAMEBUFFER, null);
      gl.viewport(0, 0, gl.canvas.width, gl.canvas.height);
    }
    gl.bindBuffer(gl.ARRAY_BUFFER, this.renderer.posBuf);
    gl.enableVertexAttribArray(0);
    gl.vertexAttribPointer(0, 2, gl.FLOAT, false, 0, 0);
    gl.drawArrays(gl.TRIANGLES, 0, 3);
  }

  // ── Simulation step ─────────────────────────────────────────

  step(dt) {
    if (!this.active) return;
    const gl = this.gl;
    gl.disable(gl.BLEND);

    // Clamp dt
    dt = Math.min(dt, 0.016667);

    const cfg = this.config;
    const vel = this._velocity;
    const dye = this._dye;
    const pres = this._pressure;
    const div = this._divergence;
    const cur = this._curl;
    const simTexel = [1.0 / vel.width, 1.0 / vel.height];
    const dyeTexel = [1.0 / dye.width, 1.0 / dye.height];

    // 1. Curl
    this._useProgram('curl');
    this._setUniform2f('curl', 'texelSize', simTexel);
    this._bindTexture('curl', 'uVelocity', vel.read.texture, 0);
    this._blit(cur);

    // 2. Vorticity confinement
    this._useProgram('vorticity');
    this._setUniform2f('vorticity', 'texelSize', simTexel);
    this._bindTexture('vorticity', 'uVelocity', vel.read.texture, 0);
    this._bindTexture('vorticity', 'uCurl', cur.texture, 1);
    this._setUniform1f('vorticity', 'curl', cfg.CURL);
    this._setUniform1f('vorticity', 'dt', dt);
    this._blit(vel.write);
    vel.swap();

    // 3. Divergence
    this._useProgram('divergence');
    this._setUniform2f('divergence', 'texelSize', simTexel);
    this._bindTexture('divergence', 'uVelocity', vel.read.texture, 0);
    this._blit(div);

    // 4. Clear pressure (dissipation)
    this._useProgram('clear');
    this._bindTexture('clear', 'uTexture', pres.read.texture, 0);
    this._setUniform1f('clear', 'value', cfg.PRESSURE);
    this._blit(pres.write);
    pres.swap();

    // 5. Pressure solve (Jacobi iterations)
    this._useProgram('pressure');
    this._setUniform2f('pressure', 'texelSize', simTexel);
    this._bindTexture('pressure', 'uDivergence', div.texture, 1);
    for (let i = 0; i < cfg.PRESSURE_ITERATIONS; i++) {
      this._bindTexture('pressure', 'uPressure', pres.read.texture, 0);
      this._blit(pres.write);
      pres.swap();
    }

    // 6. Gradient subtract (projection)
    this._useProgram('gradientSubtract');
    this._setUniform2f('gradientSubtract', 'texelSize', simTexel);
    this._bindTexture('gradientSubtract', 'uPressure', pres.read.texture, 0);
    this._bindTexture('gradientSubtract', 'uVelocity', vel.read.texture, 1);
    this._blit(vel.write);
    vel.swap();

    // 7. Advect velocity
    this._useProgram('advection');
    this._setUniform2f('advection', 'texelSize', simTexel);
    this._setUniform2f('advection', 'dyeTexelSize', simTexel);
    this._bindTexture('advection', 'uVelocity', vel.read.texture, 0);
    this._bindTexture('advection', 'uSource', vel.read.texture, 1);
    this._setUniform1f('advection', 'dt', dt);
    this._setUniform1f('advection', 'dissipation', cfg.VELOCITY_DISSIPATION);
    this._blit(vel.write);
    vel.swap();

    // 8. Advect dye
    this._useProgram('advection');
    this._setUniform2f('advection', 'texelSize', simTexel);
    this._setUniform2f('advection', 'dyeTexelSize', dyeTexel);
    this._bindTexture('advection', 'uVelocity', vel.read.texture, 0);
    this._bindTexture('advection', 'uSource', dye.read.texture, 1);
    this._setUniform1f('advection', 'dt', dt);
    this._setUniform1f('advection', 'dissipation', cfg.DENSITY_DISSIPATION);
    this._blit(dye.write);
    dye.swap();
  }

  // ── Splat injection ─────────────────────────────────────────

  splat(x, y, dx, dy, color) {
    if (!this.active) return;
    const gl = this.gl;
    const cfg = this.config;

    // Splat into velocity
    this._useProgram('splat');
    this._bindTexture('splat', 'uTarget', this._velocity.read.texture, 0);
    this._setUniform1f('splat', 'aspectRatio', gl.canvas.width / gl.canvas.height);
    this._setUniform2f('splat', 'point', [x, y]);
    this._setUniform1f('splat', 'radius', this._correctRadius(cfg.SPLAT_RADIUS / 100.0));
    this._setUniform3f('splat', 'color', [dx, dy, 0]);
    this._setUniform1f('splat', 'isDye', 0.0); // velocity — no clamp
    this._blit(this._velocity.write);
    this._velocity.swap();

    // Splat into dye
    this._bindTexture('splat', 'uTarget', this._dye.read.texture, 0);
    this._setUniform3f('splat', 'color', [color.r, color.g, color.b]);
    this._setUniform1f('splat', 'isDye', 1.0); // dye — clamp to prevent white-out
    this._blit(this._dye.write);
    this._dye.swap();
  }

  _correctRadius(radius) {
    const gl = this.gl;
    const aspectRatio = gl.canvas.width / gl.canvas.height;
    if (aspectRatio > 1) radius *= aspectRatio;
    return radius;
  }

  // Queue a splat (processed each frame)
  addSplat(x, y, dx, dy, color) {
    this._splatQueue.push({ x, y, dx, dy, color: color || this._randomColor() });
  }

  // Multiple random splats (initial burst / user triggered)
  splatRandom(count) {
    for (let i = 0; i < count; i++) {
      const color = this._randomColor();
      color.r *= 10; color.g *= 10; color.b *= 10;
      const x = Math.random();
      const y = Math.random();
      const dx = 1000 * (Math.random() - 0.5);
      const dy = 1000 * (Math.random() - 0.5);
      this.addSplat(x, y, dx, dy, color);
    }
  }

  _randomColor() {
    const c = _hsvToRgb(Math.random(), 1.0, 1.0);
    return { r: c[0] * 0.15, g: c[1] * 0.15, b: c[2] * 0.15 };
  }

  // ── Render output ───────────────────────────────────────────

  /**
   * Render the dye field to a target FBO (layer's FBO for compositing)
   * @param {Object} targetFBO - { fbo, texture, width, height }
   */
  renderToFBO(targetFBO) {
    if (!this.active || !this._dye) return;
    const gl = this.gl;
    gl.disable(gl.BLEND);

    this._useProgram('display');
    this._bindTexture('display', 'uTexture', this._dye.read.texture, 0);
    if (this.config.SHADING) {
      this._bindTexture('display', 'uVelocity', this._velocity.read.texture, 1);
      this._setUniform2f('display', 'texelSize', [1.0 / this._velocity.width, 1.0 / this._velocity.height]);
    }
    this._setUniform1f('display', 'shading', this.config.SHADING ? 1.0 : 0.0);
    this._blit(targetFBO);
  }

  // ── Full frame update (called from compositionLoop) ─────────

  update(mousePos, mouseDelta, mouseDown, audioData, mediaPipe) {
    if (!this.active) return;

    const now = performance.now();
    const dt = Math.min((now - this._lastTime) / 1000, 0.016667);
    this._lastTime = now;

    // Color cycling
    if (this.config.COLORFUL) {
      this._colorTimer += dt * this.config.COLOR_UPDATE_SPEED;
      if (this._colorTimer >= 1) {
        this._colorTimer -= 1;
        this._pointerColor = this._randomColor();
      }
    }

    // Mouse splats
    if (mouseDown || (Math.abs(mouseDelta[0]) > 0.001 || Math.abs(mouseDelta[1]) > 0.001)) {
      const dx = mouseDelta[0] * this.config.SPLAT_FORCE;
      const dy = mouseDelta[1] * this.config.SPLAT_FORCE;
      if (Math.abs(dx) > 0.1 || Math.abs(dy) > 0.1) {
        this.addSplat(mousePos[0], mousePos[1], dx, dy, this._pointerColor);
      }
    }

    // Audio-reactive splats — throttled to avoid flooding from mic noise
    if (audioData) {
      const { level, bass, mid, high } = audioData;
      this._audioSplatCooldown = (this._audioSplatCooldown || 0) - dt;
      // Only react to strong bass hits, max ~4 splats/sec
      if (bass > 0.85 && this._audioSplatCooldown <= 0) {
        this._audioSplatCooldown = 0.25;
        const color = this._randomColor();
        color.r *= 2; color.g *= 2; color.b *= 2;
        this.addSplat(
          0.3 + Math.random() * 0.4, 0.3 + Math.random() * 0.4,
          (Math.random() - 0.5) * 400 * bass, (Math.random() - 0.5) * 400 * bass,
          color
        );
      }
    }

    // MediaPipe hand splats (both hands)
    if (mediaPipe && mediaPipe.active && mediaPipe.handCount > 0) {
      // Hand 1
      const pos = mediaPipe.handPos;
      if (pos) {
        const hx = 1.0 - pos[0];
        const hy = pos[1];
        if (this._lastHandX !== undefined) {
          const hdx = (hx - this._lastHandX) * this.config.SPLAT_FORCE * 0.5;
          const hdy = (hy - this._lastHandY) * this.config.SPLAT_FORCE * 0.5;
          if (Math.abs(hdx) > 0.5 || Math.abs(hdy) > 0.5) {
            const color = { r: 0.3, g: 0.7, b: 1.0 }; // cyan for hand 1
            this.addSplat(hx, hy, hdx, hdy, color);
          }
        }
        this._lastHandX = hx;
        this._lastHandY = hy;
      }
      // Hand 2
      if (mediaPipe.handCount > 1) {
        const pos2 = mediaPipe.handPos2;
        if (pos2) {
          const hx2 = 1.0 - pos2[0];
          const hy2 = pos2[1];
          if (this._lastHand2X !== undefined) {
            const hdx2 = (hx2 - this._lastHand2X) * this.config.SPLAT_FORCE * 0.5;
            const hdy2 = (hy2 - this._lastHand2Y) * this.config.SPLAT_FORCE * 0.5;
            if (Math.abs(hdx2) > 0.5 || Math.abs(hdy2) > 0.5) {
              const color2 = { r: 0.91, g: 0.25, b: 0.34 }; // ShaderClaw red for hand 2
              this.addSplat(hx2, hy2, hdx2, hdy2, color2);
            }
          }
          this._lastHand2X = hx2;
          this._lastHand2Y = hy2;
        }
      }
    }

    // MediaPipe pose splats — skeleton joints drive fluid just like mouse/hand
    if (mediaPipe && mediaPipe.active && mediaPipe.modes && mediaPipe.modes.pose
        && mediaPipe._lastPoseLandmarks && mediaPipe._lastPoseLandmarks.length > 28) {
      const pl = mediaPipe._lastPoseLandmarks;
      // Key joints: nose(0), shoulders(11,12), elbows(13,14), wrists(15,16),
      //             hips(23,24), knees(25,26), ankles(27,28)
      const POSE_JOINTS = [0, 11,12, 13,14, 15,16, 23,24, 25,26, 27,28];
      // Per-joint color palette for visual variety
      const JOINT_COLORS = [
        {r:1.0, g:0.8, b:0.3},  // nose — gold
        {r:0.3, g:0.7, b:1.0},  // L shoulder — cyan
        {r:0.3, g:0.7, b:1.0},  // R shoulder
        {r:0.2, g:1.0, b:0.6},  // L elbow — green
        {r:0.2, g:1.0, b:0.6},  // R elbow
        {r:0.9, g:0.3, b:1.0},  // L wrist — purple
        {r:0.9, g:0.3, b:1.0},  // R wrist
        {r:1.0, g:0.4, b:0.2},  // L hip — orange
        {r:1.0, g:0.4, b:0.2},  // R hip
        {r:0.3, g:0.4, b:1.0},  // L knee — blue
        {r:0.3, g:0.4, b:1.0},  // R knee
        {r:1.0, g:0.2, b:0.4},  // L ankle — red-pink
        {r:1.0, g:0.2, b:0.4},  // R ankle
      ];

      if (!this._lastPosePos) this._lastPosePos = {};

      for (let i = 0; i < POSE_JOINTS.length; i++) {
        const ji = POSE_JOINTS[i];
        const lm = pl[ji];
        if (!lm || (lm.visibility !== undefined && lm.visibility < 0.3)) continue;

        // Mirror X (selfie view), Y is already 1-y from gesture.js
        const jx = 1.0 - lm.x;
        const jy = lm.y;
        const key = 'pose_' + ji;

        if (this._lastPosePos[key] !== undefined) {
          const prevX = this._lastPosePos[key][0];
          const prevY = this._lastPosePos[key][1];
          const jdx = (jx - prevX) * this.config.SPLAT_FORCE * 0.6;
          const jdy = (jy - prevY) * this.config.SPLAT_FORCE * 0.6;
          // Lower threshold than hand — pose tracks slower so deltas are smaller
          if (Math.abs(jdx) > 0.2 || Math.abs(jdy) > 0.2) {
            const color = this.config.COLORFUL ? this._randomColor() : JOINT_COLORS[i];
            this.addSplat(jx, jy, jdx, jdy, color);
          }
        }
        this._lastPosePos[key] = [jx, jy];
      }
    } else if (this._lastPosePos) {
      // Pose turned off — clear tracked positions so we don't get a teleport on re-enable
      this._lastPosePos = null;
    }

    // Pinch-to-fade: any hand pinching triggers opacity decay
    const anyPinch = mediaPipe && mediaPipe.active && (mediaPipe.isPinching || mediaPipe.isPinching2);
    if (anyPinch) {
      this.pinchFading = true;
      this.pinchOpacity = Math.max(0, this.pinchOpacity - dt * 0.8); // fade over ~1.2s
    } else if (this.pinchFading) {
      // Released — ramp back up
      this.pinchOpacity = Math.min(1, this.pinchOpacity + dt * 0.5); // recover over ~2s
      if (this.pinchOpacity >= 1) this.pinchFading = false;
    }

    // Movement patterns — virtual cursors that simulate continuous mouse movement
    // Skip when pose tracking is active: body becomes the sole source of fluid.
    const _poseActive = mediaPipe && mediaPipe.active && mediaPipe.modes && mediaPipe.modes.pose
        && mediaPipe._lastPoseLandmarks && mediaPipe._lastPoseLandmarks.length > 28;
    if (this.config.MOVEMENT && !_poseActive) {
      const t = performance.now() * 0.001 * this.config.MOVE_SPEED;
      const spread = this.config.MOVE_SPREAD * 0.42;
      const forceScale = this.config.MOVE_INTENSITY * this.config.SPLAT_FORCE * 0.8;
      const pat = this.config.MOVE_PATTERN;

      for (let s = 0; s < 3; s++) {
        const cur = this._moveCursors[s];
        const fs = s;

        // Save previous position
        cur.prevX = cur.x;
        cur.prevY = cur.y;

        // Compute new target position based on pattern
        if (pat === 0) {
          // Freeform — smooth Lissajous wandering
          const phase = t * (0.5 + fs * 0.3) + fs * 1.257;
          cur.x = 0.5 + spread * Math.sin(phase) * Math.cos(phase * 0.7 + fs);
          cur.y = 0.5 + spread * Math.cos(phase * 0.8) * Math.sin(phase * 0.5 + fs * 1.5);
        } else if (pat === 1) {
          // Center — orbiting with pulsing radius
          const phase = t * (0.8 + fs * 0.25) + fs * 1.257;
          const r = spread * 0.5 * (0.3 + 0.7 * Math.abs(Math.sin(phase * 0.5)));
          const a = phase * 0.7 + fs * 1.257;
          cur.x = 0.5 + Math.cos(a) * r;
          cur.y = 0.5 + Math.sin(a) * r;
        } else if (pat === 2) {
          // Wave — sweeping horizontal with vertical oscillation
          const wavePhase = t * 0.8 + fs * 0.7;
          cur.x = 0.5 + spread * Math.sin(wavePhase * 0.6 + fs * 0.8);
          cur.y = 0.5 + spread * 0.7 * Math.sin(wavePhase * 1.1 + fs * 1.7);
        } else if (pat === 3) {
          // Vortex — spinning around drifting centers
          const vortexPhase = t * (0.6 + fs * 0.2);
          const r = spread * (0.15 + fs * 0.15);
          const cx = 0.5 + spread * 0.3 * Math.sin(t * 0.3 + fs);
          const cy = 0.5 + spread * 0.3 * Math.cos(t * 0.25 + fs * 1.5);
          cur.x = cx + Math.cos(vortexPhase) * r;
          cur.y = cy + Math.sin(vortexPhase) * r;
        } else {
          // Pulse — expanding rings from center
          const pulse = (t * 2.0 * (0.3 + fs * 0.2) + fs * 0.5) % 1.0;
          const pulseR = pulse * spread * 1.5;
          const a = fs * 2.094 + t * 0.3;
          const cx = 0.5 + spread * 0.4 * Math.sin(t * 0.2 + fs * 2.094);
          const cy = 0.5 + spread * 0.4 * Math.cos(t * 0.15 + fs * 2.094);
          cur.x = cx + Math.cos(a) * pulseR;
          cur.y = cy + Math.sin(a) * pulseR;
        }

        // Compute delta (just like real mouse movement)
        const dx = cur.x - cur.prevX;
        const dy = cur.y - cur.prevY;
        const speed = Math.sqrt(dx * dx + dy * dy);

        // Only splat if cursor is actually moving (prevents stationary blobs)
        if (speed > 0.0005) {
          // Cycle cursor color periodically
          if (!cur.color || Math.random() < 0.02) {
            cur.color = this._randomColor();
          }
          // Scale force by delta — exactly how mouse splats work
          const fx = dx * forceScale;
          const fy = dy * forceScale;
          this.addSplat(cur.x, cur.y, fx, fy, cur.color);
        }
      }
    }

    // Process queued splats
    for (const s of this._splatQueue) {
      this.splat(s.x, s.y, s.dx, s.dy, s.color);
    }
    this._splatQueue.length = 0;

    // Run simulation
    this.step(dt);
  }

  // ── Uniform helpers ─────────────────────────────────────────

  _useProgram(name) {
    const p = this._programs[name];
    if (!p) return;
    this.gl.useProgram(p.program);
    this._activeProgram = name;
  }

  _setUniform1f(prog, name, value) {
    const p = this._programs[prog];
    if (p && p.uniforms[name] !== undefined) {
      this.gl.uniform1f(p.uniforms[name], value);
    }
  }

  _setUniform2f(prog, name, value) {
    const p = this._programs[prog];
    if (p && p.uniforms[name] !== undefined) {
      this.gl.uniform2f(p.uniforms[name], value[0], value[1]);
    }
  }

  _setUniform3f(prog, name, value) {
    const p = this._programs[prog];
    if (p && p.uniforms[name] !== undefined) {
      this.gl.uniform3f(p.uniforms[name], value[0], value[1], value[2]);
    }
  }

  _bindTexture(prog, name, texture, unit) {
    const gl = this.gl;
    const p = this._programs[prog];
    if (!p || p.uniforms[name] === undefined) return;
    gl.activeTexture(gl.TEXTURE0 + unit);
    gl.bindTexture(gl.TEXTURE_2D, texture);
    gl.uniform1i(p.uniforms[name], unit);
  }

  // ── ISF-compatible input descriptors for UI controls ────────

  getInputs() {
    return [
      { NAME: 'life', LABEL: 'Life', TYPE: 'float', DEFAULT: this.config.LIFE, MIN: 0, MAX: 10 },
      { NAME: 'curl', LABEL: 'Curl / Vorticity', TYPE: 'float', DEFAULT: this.config.CURL, MIN: 0, MAX: 50 },
      { NAME: 'splatRadius', LABEL: 'Splat Radius', TYPE: 'float', DEFAULT: this.config.SPLAT_RADIUS, MIN: 0.01, MAX: 1.0 },
      { NAME: 'splatForce', LABEL: 'Splat Force', TYPE: 'float', DEFAULT: this.config.SPLAT_FORCE, MIN: 100, MAX: 20000 },
      { NAME: 'pressure', LABEL: 'Pressure', TYPE: 'float', DEFAULT: this.config.PRESSURE, MIN: 0, MAX: 1 },
      { NAME: 'velocityDissipation', LABEL: 'Velocity Dissipation', TYPE: 'float', DEFAULT: this.config.VELOCITY_DISSIPATION, MIN: 0, MAX: 4 },
      { NAME: 'simResolution', LABEL: 'Sim Resolution', TYPE: 'long', DEFAULT: this.config.SIM_RESOLUTION, MIN: 32, MAX: 256, VALUES: [32, 64, 128, 256] },
      { NAME: 'dyeResolution', LABEL: 'Dye Resolution', TYPE: 'long', DEFAULT: this.config.DYE_RESOLUTION, MIN: 128, MAX: 2048, VALUES: [128, 256, 512, 1024, 2048] },
      { NAME: 'movement', LABEL: 'Movement', TYPE: 'bool', DEFAULT: this.config.MOVEMENT },
      { NAME: 'movePattern', LABEL: 'Move Pattern', TYPE: 'long', DEFAULT: this.config.MOVE_PATTERN, VALUES: [0, 1, 2, 3, 4], LABELS: ['Freeform', 'Center', 'Wave', 'Vortex', 'Pulse'] },
      { NAME: 'moveSpeed', LABEL: 'Move Speed', TYPE: 'float', DEFAULT: this.config.MOVE_SPEED, MIN: 0.1, MAX: 2.0 },
      { NAME: 'moveSpread', LABEL: 'Move Spread', TYPE: 'float', DEFAULT: this.config.MOVE_SPREAD, MIN: 0.0, MAX: 1.0 },
      { NAME: 'moveIntensity', LABEL: 'Move Intensity', TYPE: 'float', DEFAULT: this.config.MOVE_INTENSITY, MIN: 0.0, MAX: 1.0 },
      { NAME: 'shading', LABEL: 'Shading', TYPE: 'bool', DEFAULT: this.config.SHADING },
      { NAME: 'colorful', LABEL: 'Colorful', TYPE: 'bool', DEFAULT: this.config.COLORFUL },
    ];
  }

  // Life → dissipation: life 0 = dissipation 4 (instant fade), life 10 = dissipation 0 (forever)
  _lifeToDissipation(life) {
    return Math.max(0, 4.0 * (1.0 - life / 10.0));
  }

  // Apply a parameter change from UI
  setParam(name, value) {
    // Life slider drives density dissipation
    if (name === 'life') {
      this.config.LIFE = value;
      this.config.DENSITY_DISSIPATION = this._lifeToDissipation(value);
      return;
    }

    const map = {
      simResolution: 'SIM_RESOLUTION',
      dyeResolution: 'DYE_RESOLUTION',
      velocityDissipation: 'VELOCITY_DISSIPATION',
      pressure: 'PRESSURE',
      curl: 'CURL',
      splatRadius: 'SPLAT_RADIUS',
      splatForce: 'SPLAT_FORCE',
      movement: 'MOVEMENT',
      movePattern: 'MOVE_PATTERN',
      moveSpeed: 'MOVE_SPEED',
      moveSpread: 'MOVE_SPREAD',
      moveIntensity: 'MOVE_INTENSITY',
      shading: 'SHADING',
      colorful: 'COLORFUL',
    };
    const key = map[name];
    if (!key) return;

    this.config[key] = value;

    // Reinit FBOs if resolution changed
    if (name === 'simResolution' || name === 'dyeResolution') {
      this._reinitFBOs();
    }
  }

  _reinitFBOs() {
    // Destroy old
    this._destroyDoubleFBO(this._velocity);
    this._destroyDoubleFBO(this._dye);
    this._destroyDoubleFBO(this._pressure);
    this._destroySingleFBO(this._divergence);
    this._destroySingleFBO(this._curl);
    // Create new
    this._initFBOs();
  }
}

// ── HSV → RGB utility ─────────────────────────────────────────

function _hsvToRgb(h, s, v) {
  const i = Math.floor(h * 6);
  const f = h * 6 - i;
  const p = v * (1 - s);
  const q = v * (1 - f * s);
  const t = v * (1 - (1 - f) * s);
  switch (i % 6) {
    case 0: return [v, t, p];
    case 1: return [q, v, p];
    case 2: return [p, v, t];
    case 3: return [p, q, v];
    case 4: return [t, p, v];
    case 5: return [v, p, q];
  }
}

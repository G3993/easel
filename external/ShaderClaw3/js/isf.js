// ============================================================
// ShaderClaw — ISF Parser + Shader Builder
// ============================================================

const BLANK_SHADER = `/*{
  "DESCRIPTION": "Blank canvas — describe your vision in the chat",
  "CATEGORIES": ["Generator"],
  "INPUTS": [
    { "NAME": "color", "TYPE": "color", "DEFAULT": [0.91, 0.25, 0.34, 1.0], "LABEL": "Color" },
    { "NAME": "intensity", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "LABEL": "Intensity" }
  ]
}*/

void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    float t = TIME * 0.5;

    // Gentle breathing glow in the center
    float d = length(uv - 0.5);
    float glow = intensity * smoothstep(0.5, 0.0, d) * (0.5 + 0.5 * sin(t));

    vec3 col = mix(vec3(0.02, 0.02, 0.04), color.rgb, glow * 0.3);
    gl_FragColor = vec4(col, 1.0);
}`;

const DEFAULT_SHADER = `/*{
  "DESCRIPTION": "Particle network — drifting points connected by proximity lines",
  "CATEGORIES": ["Generator"],
  "INPUTS": [
    { "NAME": "particleCount", "TYPE": "float", "DEFAULT": 40.0, "MIN": 10.0, "MAX": 80.0 },
    { "NAME": "speed", "TYPE": "float", "DEFAULT": 0.4, "MIN": 0.0, "MAX": 2.0 },
    { "NAME": "connectDist", "TYPE": "float", "DEFAULT": 0.25, "MIN": 0.05, "MAX": 0.5 },
    { "NAME": "lineWidth", "TYPE": "float", "DEFAULT": 1.0, "MIN": 0.3, "MAX": 3.0 },
    { "NAME": "dotSize", "TYPE": "float", "DEFAULT": 3.0, "MIN": 1.0, "MAX": 8.0 },
    { "NAME": "color1", "TYPE": "color", "DEFAULT": [1.0, 1.0, 1.0, 1.0] },
    { "NAME": "bgColor", "TYPE": "color", "DEFAULT": [0.02, 0.02, 0.05, 1.0] }
  ]
}*/

// Stable hash for particle seeding
vec2 particleHash(float id) {
    return vec2(
        fract(sin(id * 127.1 + 311.7) * 43758.5453),
        fract(sin(id * 269.5 + 183.3) * 28001.8384)
    );
}

// Particle position: drifts with unique velocity, wraps around [0,1]
vec2 particlePos(float id, float t) {
    vec2 seed = particleHash(id);
    vec2 vel = (particleHash(id + 100.0) - 0.5) * 0.5;
    return fract(seed + vel * t);
}

// Distance from point p to line segment a-b
float segDist(vec2 p, vec2 a, vec2 b) {
    vec2 ab = b - a;
    float len2 = dot(ab, ab);
    if (len2 < 0.000001) return length(p - a);
    float t = clamp(dot(p - a, ab) / len2, 0.0, 1.0);
    vec2 proj = a + ab * t;
    return length(p - proj);
}

void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    float aspect = RENDERSIZE.x / RENDERSIZE.y;
    vec2 p = vec2(uv.x * aspect, uv.y);
    float t = TIME * speed;
    float px = 1.0 / RENDERSIZE.y; // pixel size in UV space

    int N = int(particleCount);
    vec3 col = bgColor.rgb;

    // Precompute particle positions (store aspect-corrected)
    // GLSL ES 1.0 doesn't support variable arrays, so we compute on the fly

    // Draw connection lines (additive)
    float lineAccum = 0.0;
    for (int i = 0; i < 80; i++) {
        if (i >= N) break;
        vec2 pi = particlePos(float(i), t);
        pi.x *= aspect;

        for (int j = 0; j < 80; j++) {
            if (j >= N || j <= i) break;
            vec2 pj = particlePos(float(j), t);
            pj.x *= aspect;

            float d = length(pi - pj);
            if (d > connectDist) continue;

            // Quick AABB cull: skip if pixel is far from the segment
            vec2 mn = min(pi, pj) - vec2(connectDist * 0.1);
            vec2 mx = max(pi, pj) + vec2(connectDist * 0.1);
            if (p.x < mn.x || p.x > mx.x || p.y < mn.y || p.y > mx.y) continue;

            float sd = segDist(p, pi, pj);
            float lw = lineWidth * px;
            float alpha = (1.0 - d / connectDist);
            alpha *= smoothstep(lw, lw * 0.3, sd);
            lineAccum += alpha * 0.5;
        }
    }
    col += color1.rgb * min(lineAccum, 1.0);

    // Draw particles (additive glow)
    float dotAccum = 0.0;
    for (int i = 0; i < 80; i++) {
        if (i >= N) break;
        vec2 pi = particlePos(float(i), t);
        pi.x *= aspect;

        float d = length(p - pi);
        float r = dotSize * px;
        dotAccum += smoothstep(r, r * 0.15, d);
    }
    col += color1.rgb * min(dotAccum, 1.5);

    gl_FragColor = vec4(col, 1.0);
}`;

// ============================================================
// ISF Parser
// ============================================================

// --- Auto-detect and convert Shadertoy code to ISF ---
function isShadertoyCode(source) {
  // Detect Shadertoy patterns: mainImage signature or iTime/iResolution without ISF header
  const hasISFHeader = /\/\*\s*\{[\s\S]*?\}\s*\*\//.test(source);
  if (hasISFHeader) return false;
  const hasMainImage = /void\s+mainImage\s*\(/.test(source);
  const hasShadertoyUniforms = /\b(iTime|iResolution|iMouse|iFrame|iChannel\d)\b/.test(source);
  return hasMainImage || hasShadertoyUniforms;
}

function convertShadertoy(source) {
  if (!isShadertoyCode(source)) return source;

  let s = source;

  // Collect which Shadertoy uniforms are used so we know what INPUTS to declare
  const usesIMouse = /\biMouse\b/.test(s);
  const usesIFrame = /\biFrame\b/.test(s);
  const usesIChannel = [];
  for (let i = 0; i < 4; i++) {
    if (new RegExp(`\\biChannel${i}\\b`).test(s)) usesIChannel.push(i);
  }

  // Replace Shadertoy uniforms with ISF equivalents
  s = s.replace(/\biTime\b/g, 'TIME');
  s = s.replace(/\biTimeDelta\b/g, '(1.0/60.0)');
  s = s.replace(/\biResolution\b/g, 'RENDERSIZE3');  // temp placeholder (vec3 → vec2 bridge)
  s = s.replace(/\biFrame\b/g, 'FRAMEINDEX');
  s = s.replace(/\biDate\b/g, 'vec4(0.0)');

  // iMouse: Shadertoy uses pixel coords, ShaderClaw mousePos is normalized 0-1
  if (usesIMouse) {
    s = s.replace(/\biMouse\b/g, '_iMouse');
  }

  // iChannelN → image inputs
  for (const ch of usesIChannel) {
    s = s.replace(new RegExp(`\\biChannel${ch}\\b`, 'g'), `inputImage${ch}`);
    // Also handle iChannelResolution[N]
    s = s.replace(new RegExp(`iChannelResolution\\s*\\[\\s*${ch}\\s*\\]`, 'g'), `vec3(IMG_SIZE_inputImage${ch}, 1.0)`);
  }

  // texture() → texture2D() for GLSL ES 1.0
  s = s.replace(/\btexture\s*\(/g, 'texture2D(');
  // textureLod → texture2D (no LOD in ES 1.0)
  s = s.replace(/\btextureLod\s*\(/g, 'texture2D(');

  // mainImage(out vec4 fragColor, in vec2 fragCoord) → void main()
  const mainImageRe = /void\s+mainImage\s*\(\s*out\s+vec4\s+(\w+)\s*,\s*in\s+vec2\s+(\w+)\s*\)/;
  const match = s.match(mainImageRe);
  if (match) {
    const fragColorName = match[1];
    const fragCoordName = match[2];
    s = s.replace(mainImageRe, 'void main()');
    // Replace the output variable with gl_FragColor
    if (fragColorName !== 'gl_FragColor') {
      s = s.replace(new RegExp(`\\b${fragColorName}\\b`, 'g'), 'gl_FragColor');
    }
    // Replace fragCoord with gl_FragCoord.xy
    if (fragCoordName !== 'gl_FragCoord') {
      s = s.replace(new RegExp(`\\b${fragCoordName}\\b`, 'g'), 'gl_FragCoord.xy');
    }
  }

  // RENDERSIZE3 bridge: Shadertoy iResolution is vec3, ISF RENDERSIZE is vec2
  // Replace .xy usage first (most common), then bare usage
  s = s.replace(/RENDERSIZE3\.xy\b/g, 'RENDERSIZE');
  s = s.replace(/RENDERSIZE3\.x\b/g, 'RENDERSIZE.x');
  s = s.replace(/RENDERSIZE3\.y\b/g, 'RENDERSIZE.y');
  s = s.replace(/RENDERSIZE3\.z\b/g, '1.0');
  s = s.replace(/\bRENDERSIZE3\b/g, 'vec3(RENDERSIZE, 1.0)');

  // Build ISF header with INPUTS
  const inputs = [];
  if (usesIMouse) {
    // We'll provide _iMouse as a vec4 derived from mousePos/mouseDown in a preamble
  }
  for (const ch of usesIChannel) {
    inputs.push(`    { "NAME": "inputImage${ch}", "TYPE": "image", "LABEL": "Image ${ch}" }`);
  }

  let header = `/*{\n  "DESCRIPTION": "Converted from Shadertoy",\n  "CATEGORIES": ["Generator"]`;
  if (inputs.length > 0) {
    header += `,\n  "INPUTS": [\n${inputs.join(',\n')}\n  ]`;
  }
  header += `\n}*/\n\n`;

  // Add iMouse bridge (convert normalized mousePos to pixel coords like Shadertoy expects)
  // Must be injected inside main() since it references uniforms
  if (usesIMouse) {
    s = s.replace(/void\s+main\s*\(\s*(void)?\s*\)\s*\{/,
      'void main() {\n  vec4 _iMouse = vec4(mousePos * RENDERSIZE, mouseDown > 0.5 ? mousePos * RENDERSIZE : vec2(0.0));');
  }

  // Remove any existing precision/version lines (buildFragmentShader adds them)
  s = s.replace(/#version\s+\d+.*/g, '');
  s = s.replace(/precision\s+(highp|mediump|lowp)\s+float\s*;/g, '');

  return header + s;
}

function parseISF(source) {
  const match = source.match(/\/\*\s*(\{[\s\S]*?\})\s*\*\//);
  if (!match) return { meta: null, glsl: source.trim(), inputs: [] };
  try {
    const meta = JSON.parse(match[1]);
    const glsl = source.slice(source.indexOf(match[0]) + match[0].length).trim();
    return { meta, glsl, inputs: meta.INPUTS || [] };
  } catch (e) {
    return { meta: null, glsl: source.trim(), inputs: [] };
  }
}

function isfInputToUniform(input) {
  const t = input.TYPE;
  if (t === 'float') return `uniform float ${input.NAME};`;
  if (t === 'color') {
    if (input.NAME === 'bgColor') {
      return [
        'uniform vec4 _bgColorSolid;',
        'uniform sampler2D _bgTex;',
        'uniform float _bgTexActive;',
        'vec4 _resolvedBgColor;',
        '#define bgColor _resolvedBgColor',
      ].join('\n');
    }
    return `uniform vec4 ${input.NAME};`;
  }
  if (t === 'bool') return `uniform bool ${input.NAME};`;
  if (t === 'point2D') return `uniform vec2 ${input.NAME};`;
  if (t === 'image') return `uniform sampler2D ${input.NAME};\nuniform vec2 IMG_SIZE_${input.NAME};`;
  if (t === 'long') return `uniform float ${input.NAME};`;
  if (t === 'text') {
    // Cap at 48 chars for mobile GPU uniform limits
    const isMobile = typeof window !== 'undefined' && (window.innerWidth <= 900 || /Mobi|Android|iPhone/i.test(navigator.userAgent));
    const maxLen = Math.min(input.MAX_LENGTH || 12, isMobile ? 48 : 64);
    const lines = [];
    for (let i = 0; i < maxLen; i++) lines.push(`uniform float ${input.NAME}_${i};`);
    lines.push(`uniform float ${input.NAME}_len;`);
    return lines.join('\n');
  }
  return `// unknown type: ${t} ${input.NAME}`;
}

function buildFragmentShader(source) {
  const parsed = parseISF(source);

  // Inject universal video input for shaders that don't already have image inputs
  // Skip for multi-pass shaders — video injection corrupts simulation buffers
  const hasImageInput = (parsed.inputs || []).some(inp => inp.TYPE === 'image');
  const hasMultiPass = parsed.meta && Array.isArray(parsed.meta.PASSES) && parsed.meta.PASSES.length > 1;
  if (!hasImageInput && !hasMultiPass) {
    if (!parsed.inputs) parsed.inputs = [];
    parsed.inputs.push(
      { NAME: 'scVideoInput', TYPE: 'image', LABEL: 'Video Input', _synthetic: true },
      { NAME: 'scVideoMix', TYPE: 'float', DEFAULT: 0.0, MIN: 0.0, MAX: 1.0, LABEL: 'Video Mix', _synthetic: true }
    );
  }

  const uniformLines = (parsed.inputs || []).map(isfInputToUniform);

  // Only declare sampler2D uniforms that the shader body actually uses
  // (mobile WebGL limits fragment shaders to ~8 samplers)
  const glslBody = parsed.glsl;
  const conditionalSampler = (name) => glslBody.includes(name) ? `uniform sampler2D ${name};` : '';
  const conditionalUniform = (decl, name) => glslBody.includes(name) ? decl : '';

  // Conditionally declare ALL uniforms — only if the shader body references them
  // This minimizes uniform count for mobile GPUs
  // Helper functions imply their dependency uniforms; inputs already declare
  // their own uniform, so bus names that collide with an INPUT are skipped.
  let _implied = '';
  if (/\baudioSpectrum\s*\(/.test(glslBody)) _implied += ' audioFFT';
  if (/\baudioKick\s*\(/.test(glslBody)) _implied += ' audioBeatPulse';
  if (/\baudioHit\s*\(/.test(glslBody)) _implied += ' audioOnset';
  if (/\baudioBreath\s*\(/.test(glslBody)) _implied += ' audioLevel';
  if (/\baudioPalette\s*\(/.test(glslBody)) _implied += ' audioPalShadow audioPalMid audioPalHigh audioPalAccent audioBeat';
  const _refText = glslBody + _implied;
  const _inputNames = new Set((parsed.inputs || []).map((i) => i.NAME));
  const cond = (decl, name) => (_refText.includes(name) && !_inputNames.has(name)) ? decl : '';

  // GLSL ES 1.0 needs an explicit extension for fwidth/dFdx/dFdy
  const usesDerivatives = /\b(fwidth|dFdx|dFdy)\s*\(/.test(glslBody);
  // Shaders written against desktop GLSL 330 (native Easel) call texture();
  // map the 2-arg form onto texture2D for WebGL1.
  const usesGL3Texture = /\btexture\s*\(/.test(glslBody);

  // Audio Feature Bus — parity with native Easel (ShaderSource.cpp).
  // Declared conditionally so mobile uniform counts stay minimal.
  const AUDIO_BUS_FLOATS = [
    'audioSub', 'audioLowMid', 'audioHighMid', 'audioTreble', 'audioPunch',
    'audioBeat', 'audioBeatPhase', 'audioBeatPulse', 'audioBarPhase', 'audioBPM', 'audioTempo01',
    'audioBrightness', 'audioSpread', 'audioRolloff', 'audioFlatness', 'audioTexture',
    'audioFlux', 'audioOnset', 'audioOnsetRate', 'audioTilt', 'audioZCR',
    'audioValence', 'audioArousal', 'audioTension', 'audioWarmth', 'audioSoftness', 'audioRoughness', 'audioCharm',
    'audioEnergy', 'audioEnergyVel', 'audioEnergyAcc', 'audioBuildup', 'audioBuildupRate', 'audioDrop',
    'audioNovelty', 'audioSectionPhase', 'audioSectionAge', 'audioLayers', 'audioDensity',
    'audioPalTemp', 'audioPalSat', 'audioDominantPitch', 'audioMajorMinor', 'audioHCDF',
    'audioBassHit', 'audioMidHit', 'audioHighHit', 'audioBassTime', 'audioMidTime', 'audioHighTime',
    // EaselAudio schema v1.0 additions (spec/easel_audio_bus.json) — additive.
    // NOTE: float `audioPresence` (mix presence) is intentionally absent: the
    // long-standing GLSL contract already declares vec4 audioPresence below.
    'audioBassPresence', 'audioMidPresence', 'audioHighPresence', 'audioTime',
    'audioBPMConfidence', 'audioPhase2', 'audioPhase4', 'audioPhase8', 'audioPhase16',
    'audioOnBeat', 'audioToggleOnBeat',
    'stemBass', 'stemDrums', 'stemMelody', 'stemAir', 'stemVocal',
    'stemBassHit', 'stemDrumsHit', 'stemMelodyHit', 'stemAirHit', 'stemVocalHit',
    'stemBassPresence', 'stemDrumsPresence', 'stemMelodyPresence', 'stemAirPresence', 'stemVocalPresence',
  ];
  const AUDIO_BUS_VECS = [
    ['vec2', 'audioMood'], ['vec4', 'audioPresence'], ['vec2', 'audioFlow'],
    ['vec3', 'audioPalShadow'], ['vec3', 'audioPalMid'], ['vec3', 'audioPalHigh'], ['vec3', 'audioPalAccent'],
  ];

  // Audio helper functions (native parity) — injected only when referenced
  const audioHelperFns = [];
  if (/\baudioSpectrum\s*\(/.test(glslBody)) audioHelperFns.push('float audioSpectrum(float f){ return texture2D(audioFFT, vec2(clamp(f,0.0,1.0),0.5)).r; }');
  if (/\baudioKick\s*\(/.test(glslBody)) audioHelperFns.push('float audioKick(){ return audioBeatPulse; }');
  if (/\baudioHit\s*\(/.test(glslBody)) audioHelperFns.push('float audioHit(){ return audioOnset; }');
  if (/\baudioBreath\s*\(/.test(glslBody)) audioHelperFns.push('float audioBreath(){ return pow(max(audioLevel,0.0),0.6); }');
  if (/\baudioAlive\s*\(/.test(glslBody)) audioHelperFns.push('float audioAlive(float rest,float drive,float amount){ return rest+drive*amount; }');
  if (/\baudioPalette\s*\(/.test(glslBody)) audioHelperFns.push('vec3 audioPalette(float t){ t=clamp(t,0.0,1.0); vec3 c=(t<0.5)?mix(audioPalShadow,audioPalMid,t*2.0):mix(audioPalMid,audioPalHigh,t*2.0-1.0); return mix(c,audioPalAccent,audioBeat*smoothstep(0.6,1.0,t)*0.6); }');

  const headerParts = [
    usesDerivatives ? '#extension GL_OES_standard_derivatives : enable' : '',
    'precision mediump float;',
    'precision mediump int;',
    usesGL3Texture ? '#define texture(s, c) texture2D(s, c)' : '',
    glslBody.includes('IMG_SIZE(') ? '#define IMG_SIZE(img) RENDERSIZE' : '',
    'uniform float TIME;',
    'uniform float TIMEDELTA;',
    'uniform vec2 RENDERSIZE;',
    cond('uniform int PASSINDEX;', 'PASSINDEX'),
    cond('uniform int FRAMEINDEX;', 'FRAMEINDEX'),
    'varying vec2 isf_FragNormCoord;',
    '#define IMG_NORM_PIXEL(img, coord) texture2D(img, coord)',
    '#define IMG_PIXEL(img, coord) texture2D(img, coord / RENDERSIZE)',
    '#define IMG_THIS_PIXEL(img) texture2D(img, isf_FragNormCoord)',
    '#define IMG_THIS_NORM_PIXEL(img) texture2D(img, isf_FragNormCoord)',
    // Mouse — always available (cheap uniforms, prevents undeclared errors)
    'uniform vec2 mousePos;',
    'uniform vec2 mouseDelta;',
    'uniform float mouseDown;',
    'uniform float pinchHold;',
    'uniform float pinchHold2;',
    'uniform float inputActivity;',
    // Audio
    cond('uniform sampler2D audioFFT;', 'audioFFT'),
    cond('uniform float audioLevel;', 'audioLevel'),
    cond('uniform float audioBass;', 'audioBass'),
    cond('uniform float audioMid;', 'audioMid'),
    cond('uniform float audioHigh;', 'audioHigh'),
    // Audio Feature Bus (native Easel parity) — see AudioFeatures in easel/src
    ...AUDIO_BUS_FLOATS.map((n) => cond(`uniform float ${n};`, n)),
    ...AUDIO_BUS_VECS.map(([t, n]) => cond(`uniform ${t} ${n};`, n)),
    // Voice / cue message age (native parity; -1 = static, >=0 = live utterance)
    cond('uniform float msgAge;', 'msgAge'),
    // Font textures
    cond('uniform sampler2D varFontTex;', 'varFontTex'),
    cond('uniform sampler2D fontAtlasTex;', 'fontAtlasTex'),
    cond('uniform float useFontAtlas;', 'useFontAtlas'),
    // Voice decay
    cond('uniform float _voiceGlitch;', '_voiceGlitch'),
    // MediaPipe
    cond('uniform sampler2D mpHandLandmarks;', 'mpHandLandmarks'),
    cond('uniform sampler2D mpFaceLandmarks;', 'mpFaceLandmarks'),
    cond('uniform sampler2D mpPoseLandmarks;', 'mpPoseLandmarks'),
    cond('uniform sampler2D mpSegMask;', 'mpSegMask'),
    cond('uniform float mpHandCount;', 'mpHandCount'),
    cond('uniform vec3 mpHandPos;', 'mpHandPos'),
    cond('uniform vec3 mpHandPos2;', 'mpHandPos2'),
    cond('uniform float mpPoseActive;', 'mpPoseActive'),
    // Layer compositing
    'uniform float _transparentBg;',
    ...uniformLines,
    // Audio helper functions (native parity) — after uniforms so deps resolve
    ...audioHelperFns,
    ''
  ].filter(Boolean);

  // Inject TARGET sampler declarations from PASSES metadata
  if (parsed.meta && Array.isArray(parsed.meta.PASSES)) {
    for (const pass of parsed.meta.PASSES) {
      if (pass.TARGET) {
        headerParts.push(`uniform sampler2D ${pass.TARGET};`);
      }
    }
    headerParts.push('');
  }

  const header = headerParts.join('\n');

  // Strip #version and redundant #ifdef GL_ES precision blocks (header already provides precision)
  const cleaned = parsed.glsl
    .replace(/#version\s+\d+.*/g, '')
    .replace(/#ifdef\s+GL_ES\s*\r?\nprecision\s+\w+\s+float\s*;\s*\r?\n#endif\s*\r?\n?/g, '');

  // Wrap shader main() to inject bgColor resolution, video blend, and/or transparent background support
  const shaderHandlesTransparency = (parsed.inputs || []).some(inp => inp.NAME === 'transparentBg');
  const hasBgColor = (parsed.inputs || []).some(inp => inp.NAME === 'bgColor');
  const injectVideo = !hasImageInput && !hasMultiPass;
  let body = header + '\n' + cleaned;
  const mainRe = /void\s+main\s*\(\s*(void)?\s*\)/;
  const needsWrap = mainRe.test(body) && (hasBgColor || !shaderHandlesTransparency || injectVideo);
  if (needsWrap) {
    body = body.replace(mainRe, 'void _shaderMain()');
    let wrapper = '\nvoid main() {\n';
    if (hasBgColor) {
      wrapper += '    _resolvedBgColor = _bgTexActive > 0.5 ? texture2D(_bgTex, isf_FragNormCoord) : _bgColorSolid;\n';
    }
    wrapper += '    _shaderMain();\n';
    // Universal video input blend (before spotlight/transparency so it participates in those)
    if (injectVideo) {
      wrapper += '    if (scVideoMix > 0.001) {\n';
      wrapper += '        vec4 _vid = texture2D(scVideoInput, isf_FragNormCoord);\n';
      wrapper += '        gl_FragColor = mix(gl_FragColor, _vid, scVideoMix);\n';
      wrapper += '    }\n';
    }
    if (!shaderHandlesTransparency) {
      wrapper += '    // Global mouse spotlight\n';
      wrapper += '    vec2 _muv = gl_FragCoord.xy / RENDERSIZE.xy;\n';
      wrapper += '    vec2 _md = _muv - mousePos;\n';
      wrapper += '    _md.x *= RENDERSIZE.x / RENDERSIZE.y;\n';
      wrapper += '    float _mr = length(_md);\n';
      wrapper += '    float _mspot = exp(-_mr * _mr * 6.0) * 0.25;\n';
      wrapper += '    gl_FragColor.rgb += gl_FragColor.rgb * _mspot;\n';
      wrapper += '    if (_transparentBg > 0.5) {\n';
      wrapper += '        float _lum = dot(gl_FragColor.rgb, vec3(0.299, 0.587, 0.114));\n';
      wrapper += '        gl_FragColor.a = smoothstep(0.02, 0.15, _lum);\n';
      wrapper += '    }\n';
    }
    wrapper += '}\n';
    body += wrapper;
  }

  return { frag: body, parsed, headerLineCount: headerParts.length };
}

const VERT_SHADER = `
precision mediump float;
attribute vec2 position;
varying vec2 isf_FragNormCoord;
void main() {
    isf_FragNormCoord = position * 0.5 + 0.5;
    gl_Position = vec4(position, 0.0, 1.0);
}`;

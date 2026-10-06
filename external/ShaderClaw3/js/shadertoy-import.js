// ============================================================
// ShaderClaw — Shadertoy Importer
// Converts Shadertoy multi-buffer shaders to ISF multi-pass
// ============================================================

/**
 * Extract shader ID from a Shadertoy URL
 * Supports: shadertoy.com/view/XXXX, shadertoy.com/view/XXXX#, etc.
 */
function parseShadertoyUrl(input) {
  input = input.trim();
  // Direct ID (no URL)
  if (/^[a-zA-Z0-9]{4,10}$/.test(input)) return input;
  // URL patterns
  const m = input.match(/shadertoy\.com\/view\/([a-zA-Z0-9]+)/);
  return m ? m[1] : null;
}

/**
 * Fetch shader data from Shadertoy via server proxy
 * @param {string} shaderId
 * @returns {Promise<Object>} Shadertoy shader JSON
 */
async function fetchShadertoyShader(shaderId) {
  const resp = await fetch('/api/shadertoy/' + shaderId);
  const data = await resp.json();
  if (data.Error) throw new Error(data.Error);
  if (!data.Shader) throw new Error('No shader data returned');
  return data.Shader;
}

/**
 * Convert a Shadertoy shader JSON to ISF source code
 * Maps Buffer A/B/C/D + Image → ISF PASSES with PASSINDEX branching
 *
 * @param {Object} shader - Shadertoy Shader object
 * @returns {{ source: string, title: string, description: string }}
 */
function shadertoyToISF(shader) {
  const info = shader.info || {};
  const passes = shader.renderpass || [];

  // Sort passes: buffers first (in order), then image
  const bufferOrder = ['buffer', 'image'];
  const typeOrder = p => {
    if (p.type === 'buffer') return 0;
    if (p.type === 'image') return 10;
    if (p.type === 'common') return -1;
    return 5;
  };

  const sorted = passes.slice().sort((a, b) => {
    if (a.type === b.type && a.type === 'buffer') {
      // Sort by output ID (97=A, 98=B, etc.)
      return (a.outputs?.[0]?.id || 0) - (b.outputs?.[0]?.id || 0);
    }
    return typeOrder(a) - typeOrder(b);
  });

  // Extract common code (if any)
  const commonPass = sorted.find(p => p.type === 'common');
  const commonCode = commonPass ? commonPass.code : '';

  // Build buffer list (excluding common and sound)
  const renderPasses = sorted.filter(p => p.type === 'buffer' || p.type === 'image');

  if (renderPasses.length === 0) throw new Error('No render passes found');

  // Map Shadertoy output IDs to ISF target names
  // Shadertoy: 97=Buffer A, 98=Buffer B, 99=Buffer C, 100=Buffer D
  const outputIdToTarget = {};
  const inputIdToTarget = {};
  const isfPasses = [];

  renderPasses.forEach((pass, idx) => {
    const isImage = pass.type === 'image';
    const outputId = pass.outputs?.[0]?.id;
    let targetName = null;

    if (!isImage && outputId != null) {
      // Buffer pass
      const letter = String.fromCharCode(65 + (outputId - 97)); // 97→A, 98→B, etc.
      targetName = 'buf' + letter;
      outputIdToTarget[outputId] = targetName;
      inputIdToTarget[outputId] = targetName;
    }

    isfPasses.push({
      targetName,
      pass,
      isImage,
      passIndex: idx
    });
  });

  // Build ISF PASSES metadata
  const passesMeta = isfPasses.map(p => {
    if (p.isImage) return {};
    return { TARGET: p.targetName, PERSISTENT: true };
  });

  // Convert each pass's GLSL
  const passBlocks = isfPasses.map(p => {
    let code = p.pass.code || '';

    // Inject common code
    if (commonCode) {
      code = commonCode + '\n' + code;
    }

    // Map Shadertoy uniforms → ISF
    code = code.replace(/\biResolution\b/g, '_iResolution');
    code = code.replace(/\biTime\b/g, 'TIME');
    code = code.replace(/\biTimeDelta\b/g, '(1.0/60.0)');
    code = code.replace(/\biFrame\b/g, '_iFrame');
    code = code.replace(/\biMouse\b/g, '_iMouse');
    code = code.replace(/\biDate\b/g, 'vec4(0.0)');
    code = code.replace(/\biSampleRate\b/g, '44100.0');
    code = code.replace(/\biChannelResolution\b/g, '_iChanRes');
    code = code.replace(/\biChannelTime\b/g, '_iChanTime');

    // Map iChannel references to ISF target names
    const inputs = p.pass.inputs || [];
    for (let ch = 0; ch < 4; ch++) {
      const input = inputs.find(i => i.channel === ch);
      let samplerName = '_emptyTex';

      if (input) {
        const srcId = input.id;
        if (inputIdToTarget[srcId]) {
          // Points to a buffer
          samplerName = inputIdToTarget[srcId];
        } else if (input.ctype === 'texture' || input.ctype === 'cubemap') {
          // External texture — use placeholder
          samplerName = '_noiseTex';
        } else if (srcId != null && outputIdToTarget[srcId]) {
          samplerName = outputIdToTarget[srcId];
        }
      }

      // Replace iChannel0..3 with the mapped sampler name
      const re = new RegExp('\\btexture\\s*\\(\\s*iChannel' + ch, 'g');
      code = code.replace(re, 'texture2D(' + samplerName);
      const re2 = new RegExp('\\btexture2D\\s*\\(\\s*iChannel' + ch, 'g');
      code = code.replace(re2, 'texture2D(' + samplerName);
      const re3 = new RegExp('\\btexelFetch\\s*\\(\\s*iChannel' + ch, 'g');
      code = code.replace(re3, '_texelFetch(' + samplerName);
      // Direct iChannel references (e.g. as function args)
      const re4 = new RegExp('\\biChannel' + ch + '\\b', 'g');
      code = code.replace(re4, samplerName);
    }

    // Convert mainImage signature → ISF main body
    // void mainImage(out vec4 fragColor, in vec2 fragCoord) → block
    code = code.replace(
      /void\s+mainImage\s*\(\s*out\s+vec4\s+(\w+)\s*,\s*in\s+vec2\s+(\w+)\s*\)/,
      (_, colorVar, coordVar) => {
        return `void _passMain_${p.passIndex}(out vec4 ${colorVar}, vec2 ${coordVar})`;
      }
    );

    return {
      index: p.passIndex,
      code,
      isImage: p.isImage,
      funcName: `_passMain_${p.passIndex}`
    };
  });

  // Collect all unique target names for sampler declarations
  const allTargets = isfPasses.filter(p => p.targetName).map(p => p.targetName);

  // Build the combined ISF shader
  let isfSource = '';

  // ISF header
  isfSource += '/*{\n';
  isfSource += `  "DESCRIPTION": ${JSON.stringify(info.description || info.name || 'Shadertoy import')},\n`;
  isfSource += '  "CATEGORIES": ["Generator", "Imported"],\n';
  isfSource += '  "INPUTS": [\n';
  isfSource += '    { "NAME": "transparentBg", "LABEL": "Transparent", "TYPE": "bool", "DEFAULT": false }\n';
  isfSource += '  ],\n';
  isfSource += '  "PASSES": ' + JSON.stringify(passesMeta) + '\n';
  isfSource += '}*/\n\n';

  // Shadertoy compatibility uniforms
  isfSource += '// Shadertoy compatibility\n';
  isfSource += '#define _iResolution vec3(RENDERSIZE, 1.0)\n';
  isfSource += '#define _iFrame FRAMEINDEX\n';
  isfSource += 'vec4 _iMouse = vec4(mousePos * RENDERSIZE, mouseDown * RENDERSIZE);\n';
  isfSource += 'vec4 _iChanRes[4];\n';
  isfSource += 'float _iChanTime[4];\n';
  isfSource += '\n';

  // Noise placeholder (generates procedural noise when shader expects a noise texture)
  isfSource += '// Procedural noise for missing texture inputs\n';
  isfSource += 'float _hash(vec2 p) { vec3 p3 = fract(vec3(p.xyx) * 0.1031); p3 += dot(p3, p3.yzx + 33.33); return fract((p3.x + p3.y) * p3.z); }\n';
  isfSource += 'vec4 _noiseAt(vec2 uv) { float n = _hash(floor(uv * 256.0)); return vec4(n, _hash(uv * 1.17 + 7.3), _hash(uv * 2.31 + 13.7), _hash(uv * 3.47 + 23.1)); }\n';
  isfSource += '\n';

  // texelFetch polyfill for WebGL1
  isfSource += 'vec4 _texelFetch(sampler2D s, ivec2 c, int lod) { return texture2D(s, (vec2(c) + 0.5) / RENDERSIZE); }\n';
  isfSource += '\n';

  // All pass function definitions
  for (const block of passBlocks) {
    isfSource += '// --- Pass ' + block.index + (block.isImage ? ' (Image)' : '') + ' ---\n';
    isfSource += block.code + '\n\n';
  }

  // Main dispatcher
  isfSource += 'void main() {\n';
  isfSource += '  _iChanRes[0] = vec4(RENDERSIZE, 1.0, 1.0);\n';
  isfSource += '  _iChanRes[1] = vec4(RENDERSIZE, 1.0, 1.0);\n';
  isfSource += '  _iChanRes[2] = vec4(RENDERSIZE, 1.0, 1.0);\n';
  isfSource += '  _iChanRes[3] = vec4(RENDERSIZE, 1.0, 1.0);\n';
  isfSource += '  _iChanTime[0] = TIME; _iChanTime[1] = TIME;\n';
  isfSource += '  _iChanTime[2] = TIME; _iChanTime[3] = TIME;\n';
  isfSource += '  vec4 fragColor = vec4(0.0);\n';
  isfSource += '  vec2 fragCoord = gl_FragCoord.xy;\n';

  for (const block of passBlocks) {
    if (passBlocks.length === 1) {
      isfSource += `  ${block.funcName}(fragColor, fragCoord);\n`;
    } else {
      const cond = block.index === 0 ? 'if' : 'else if';
      isfSource += `  ${cond} (PASSINDEX == ${block.index}) ${block.funcName}(fragColor, fragCoord);\n`;
    }
  }

  isfSource += '  gl_FragColor = fragColor;\n';
  isfSource += '}\n';

  return {
    source: isfSource,
    title: info.name || 'Shadertoy Import',
    description: info.description || ''
  };
}

// Export for use in app.js
window._shadertoyImport = {
  parseShadertoyUrl,
  fetchShadertoyShader,
  shadertoyToISF
};

#!/usr/bin/env node
// ============================================================
// ISF-to-FFGL Converter
// Converts ShaderClaw3 ISF (.fs) shaders to Resolume FFGL plugins
// Supports single-pass and multi-pass (persistent FBO) shaders
// ============================================================

const fs = require('fs');
const path = require('path');

// --------------- ISF Parser ---------------

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

// --------------- GLSL Translation (ISF ES1.0 -> GLSL 410 core) ---------------

function translateISFtoGL410(glslBody) {
  let s = glslBody;
  s = s.replace(/precision\s+(highp|mediump|lowp)\s+(float|int)\s*;/g, '');
  s = s.replace(/\bgl_FragColor\b/g, 'fragColor');
  s = s.replace(/\btexture2D\s*\(/g, 'texture(');
  s = s.replace(/\bisf_FragNormCoord\b/g, 'uv');
  s = s.replace(/\bIMG_NORM_PIXEL\s*\(\s*(\w+)\s*,\s*/g, 'texture($1, ');
  s = s.replace(/\bIMG_PIXEL\s*\(\s*(\w+)\s*,\s*([^)]+)\)/g, 'texture($1, ($2) / RENDERSIZE)');
  s = s.replace(/\bIMG_THIS_PIXEL\s*\(\s*(\w+)\s*\)/g, 'texture($1, uv)');
  s = s.replace(/\bIMG_THIS_NORM_PIXEL\s*\(\s*(\w+)\s*\)/g, 'texture($1, uv)');
  s = s.replace(/#ifdef\s+GL_ES\s*\r?\n[\s\S]*?#endif\s*\r?\n?/g, '');
  s = s.replace(/#version\s+\d+.*/g, '');
  // varying -> in (GLSL 410)
  s = s.replace(/\bvarying\s+/g, 'in ');
  return s;
}

// --------------- ISF Input -> FFGL Param Mapping ---------------

function isfTypeToFFGL(input) {
  switch (input.TYPE) {
    case 'float':
      return { ffglType: 'FF_TYPE_STANDARD', cppType: 'float', uniformType: 'float' };
    case 'color': {
      const d = input.DEFAULT || [1, 1, 1, 1];
      return { ffglType: 'color', cppType: 'float[4]', defaultVal: [d[0] ?? 1, d[1] ?? 1, d[2] ?? 1, d[3] ?? 1], uniformType: 'vec4' };
    }
    case 'bool':
      return { ffglType: 'FF_TYPE_BOOLEAN', cppType: 'float', uniformType: 'float' };
    case 'long':
      return { ffglType: 'FF_TYPE_OPTION', cppType: 'float', uniformType: 'float', values: input.VALUES || [], labels: input.LABELS || [] };
    case 'point2D':
      return { ffglType: 'point2D', cppType: 'float[2]', defaultVal: input.DEFAULT || [0.5, 0.5], uniformType: 'vec2' };
    case 'image':
      return { ffglType: 'image', skip: true };
    case 'text':
      return { ffglType: 'text', skip: true };
    case 'event':
      return { ffglType: 'FF_TYPE_EVENT', cppType: 'float', uniformType: 'float' };
    default:
      return { ffglType: 'FF_TYPE_STANDARD', cppType: 'float', uniformType: 'float' };
  }
}

function safeName(name) {
  return name.replace(/[^a-zA-Z0-9_]/g, '_');
}

function generatePluginID(name) {
  const hash = name.split('').reduce((acc, c) => ((acc << 5) - acc + c.charCodeAt(0)) | 0, 0);
  const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789';
  return 'SC' + chars[Math.abs(hash >> 8) % chars.length] + chars[Math.abs(hash) % chars.length];
}

function toClassName(filename) {
  const base = path.basename(filename, '.fs');
  return 'ISF' + base.split(/[_\s-]+/).map(w => w.charAt(0).toUpperCase() + w.slice(1).toLowerCase()).join('');
}

function toPluginName(filename, meta) {
  if (meta && meta.DESCRIPTION) return meta.DESCRIPTION.substring(0, 48);
  const base = path.basename(filename, '.fs');
  return base.split(/[_\s-]+/).map(w => w.charAt(0).toUpperCase() + w.slice(1)).join(' ');
}

// --------------- Pass Size Expression Parser ---------------

function parseSizeExpr(expr) {
  // Returns a C++ expression string for the buffer dimension
  // Supports: "$WIDTH/3", "$HEIGHT/3", "33", "$WIDTH", integer constants
  if (expr == null) return null;
  const s = String(expr).trim();
  // Pure integer
  if (/^\d+$/.test(s)) return s;
  // $WIDTH or $HEIGHT with optional division
  const m = s.match(/^\$?(WIDTH|HEIGHT)\s*\/\s*(\d+(?:\.\d+)?)$/);
  if (m) {
    const dim = m[1] === 'WIDTH' ? 'currentViewport.width' : 'currentViewport.height';
    return `(${dim} / ${parseInt(m[2])})`;
  }
  // Bare $WIDTH / $HEIGHT
  if (s === '$WIDTH' || s === 'WIDTH') return 'currentViewport.width';
  if (s === '$HEIGHT' || s === 'HEIGHT') return 'currentViewport.height';
  // Fallback: return as-is (might be a number like "16.0")
  if (!isNaN(parseFloat(s))) return String(Math.round(parseFloat(s)));
  return null;
}

// --------------- Hardcoded inputs ---------------

const HARDCODED_INPUTS = {
  mousePos: null, mouseDelta: null, mouseDown: null,
  transparentBg: '0.0f', scVideoInput: null, scVideoMix: '0.0f',
  pinchHold: '0.0f', pinchHold2: '0.0f', inputActivity: '0.0f',
};

// --------------- Code Generation ---------------

function generateFFGLPlugin(isfPath) {
  const source = fs.readFileSync(isfPath, 'utf-8');
  const parsed = parseISF(source);
  if (!parsed.meta) {
    console.warn(`  Skipping ${path.basename(isfPath)} -- no ISF header`);
    return null;
  }

  const filename = path.basename(isfPath);
  const className = toClassName(filename);
  const pluginName = toPluginName(filename, parsed.meta);
  const pluginID = generatePluginID(className);

  // Filter inputs
  const inputs = (parsed.inputs || []).filter(inp => {
    const mapped = isfTypeToFFGL(inp);
    if (mapped.skip) return false;
    if (inp.NAME in HARDCODED_INPUTS) return false;
    return true;
  });

  const usesAudio = /\b(audioLevel|audioBass|audioMid|audioHigh|audioFFT)\b/.test(parsed.glsl);
  const usesMouse = /\bmousePos\b/.test(parsed.glsl);
  const passes = parsed.meta.PASSES || [];
  const isMultiPass = passes.length > 1;

  // Parse pass metadata
  const buffers = []; // { name, persistent, widthExpr, heightExpr }
  for (let i = 0; i < passes.length; i++) {
    const pass = passes[i];
    if (pass.TARGET) {
      const persistent = pass.PERSISTENT === true || pass.persistent === true;
      const wExpr = parseSizeExpr(pass.WIDTH);
      const hExpr = parseSizeExpr(pass.HEIGHT);
      buffers.push({
        name: pass.TARGET,
        safeName: safeName(pass.TARGET),
        persistent,
        widthExpr: wExpr,    // null means use viewport width
        heightExpr: hExpr,   // null means use viewport height
        passIndex: i,
      });
    }
  }

  // Build parameter list
  const params = [];
  let paramIndex = 0;
  for (const inp of inputs) {
    const mapped = isfTypeToFFGL(inp);
    if (mapped.ffglType === 'color') {
      const def = mapped.defaultVal;
      const label = inp.LABEL || inp.NAME;
      params.push({ name: inp.NAME, component: 'r', index: paramIndex++, ffglType: 'FF_TYPE_RED', default: def[0], label: `${label} R` });
      params.push({ name: inp.NAME, component: 'g', index: paramIndex++, ffglType: 'FF_TYPE_GREEN', default: def[1], label: `${label} G` });
      params.push({ name: inp.NAME, component: 'b', index: paramIndex++, ffglType: 'FF_TYPE_BLUE', default: def[2], label: `${label} B` });
      params.push({ name: inp.NAME, component: 'a', index: paramIndex++, ffglType: 'FF_TYPE_ALPHA', default: def[3], label: `${label} A` });
    } else if (mapped.ffglType === 'point2D') {
      const def = mapped.defaultVal;
      const label = inp.LABEL || inp.NAME;
      params.push({ name: inp.NAME, component: 'x', index: paramIndex++, ffglType: 'FF_TYPE_XPOS', default: def[0], label: `${label} X` });
      params.push({ name: inp.NAME, component: 'y', index: paramIndex++, ffglType: 'FF_TYPE_YPOS', default: def[1], label: `${label} Y` });
    } else if (mapped.ffglType === 'FF_TYPE_OPTION') {
      params.push({
        name: inp.NAME, index: paramIndex++, ffglType: 'FF_TYPE_OPTION',
        default: inp.DEFAULT || 0, label: inp.LABEL || inp.NAME,
        optionValues: mapped.values, optionLabels: mapped.labels, isOption: true,
      });
    } else {
      let def = inp.DEFAULT != null ? inp.DEFAULT : 0.5;
      let min = inp.MIN != null ? inp.MIN : 0;
      let max = inp.MAX != null ? inp.MAX : 1;
      let normalizedDefault = (max !== min) ? (def - min) / (max - min) : def;
      normalizedDefault = Math.max(0, Math.min(1, normalizedDefault));
      params.push({
        name: inp.NAME, index: paramIndex++, ffglType: mapped.ffglType,
        default: normalizedDefault, label: inp.LABEL || inp.NAME,
        rangeMin: min, rangeMax: max,
        needsRescale: (min !== 0 || max !== 1),
      });
    }
  }

  // Translate GLSL
  const glsl410 = translateISFtoGL410(parsed.glsl);

  // Build uniform declarations
  const uniformDecls = [];
  const processedUniforms = new Set();

  uniformDecls.push('uniform float TIME;');
  uniformDecls.push('uniform vec2 RENDERSIZE;');
  uniformDecls.push('uniform int FRAMEINDEX;');
  if (isMultiPass) uniformDecls.push('uniform int PASSINDEX;');

  if (usesMouse) {
    uniformDecls.push('uniform vec2 mousePos;');
    uniformDecls.push('uniform float mouseDown;');
  }
  if (usesAudio) {
    uniformDecls.push('uniform float audioLevel;');
    uniformDecls.push('uniform float audioBass;');
    uniformDecls.push('uniform float audioMid;');
    uniformDecls.push('uniform float audioHigh;');
  }

  // Buffer sampler uniforms
  for (const buf of buffers) {
    uniformDecls.push(`uniform sampler2D ${buf.name};`);
  }

  // ISF input uniforms
  for (const inp of parsed.inputs || []) {
    if (processedUniforms.has(inp.NAME)) continue;
    processedUniforms.add(inp.NAME);
    const t = inp.TYPE;
    if (t === 'float' || t === 'bool' || t === 'long' || t === 'event') {
      uniformDecls.push(`uniform float ${inp.NAME};`);
    } else if (t === 'color') {
      uniformDecls.push(`uniform vec4 ${inp.NAME};`);
    } else if (t === 'point2D') {
      uniformDecls.push(`uniform vec2 ${inp.NAME};`);
    } else if (t === 'image') {
      uniformDecls.push(`uniform sampler2D ${inp.NAME};`);
      // Some shaders use IMG_SIZE_<name>
      if (parsed.glsl.includes(`IMG_SIZE_${inp.NAME}`)) {
        uniformDecls.push(`uniform vec2 IMG_SIZE_${inp.NAME};`);
      }
    }
  }

  const fragShader = `#version 410 core
${uniformDecls.join('\n')}

in vec2 uv;
out vec4 fragColor;

${glsl410}
`;

  const vertShader = `#version 410 core
layout( location = 0 ) in vec4 vPosition;
layout( location = 1 ) in vec2 vUV;

out vec2 uv;

void main()
{
\tgl_Position = vPosition;
\tuv = vUV;
}
`;

  // --------------- Collect member/param structures ---------------

  const colorNames = new Set();
  const pointNames = new Set();
  const scalarNames = [];
  for (const p of params) {
    if (p.component && ['r', 'g', 'b', 'a'].includes(p.component)) colorNames.add(p.name);
    else if (p.component && ['x', 'y'].includes(p.component)) pointNames.add(p.name);
    else scalarNames.push(p);
  }

  const memberDecls = [];
  for (const cn of colorNames) {
    memberDecls.push(`\tstruct { float r = 1.0f, g = 1.0f, b = 1.0f, a = 1.0f; } ${safeName(cn)};`);
  }
  for (const pn of pointNames) {
    memberDecls.push(`\tstruct { float x = 0.5f, y = 0.5f; } ${safeName(pn)};`);
  }
  for (const p of scalarNames) {
    memberDecls.push(`\tfloat ${safeName(p.name)} = ${p.default.toFixed(4)}f;`);
  }

  // FBO members for multi-pass
  const fboMembers = [];
  if (isMultiPass) {
    for (const buf of buffers) {
      if (buf.persistent) {
        // Ping-pong: two FBOs
        fboMembers.push(`\tffglex::FFGLFBO ${buf.safeName}FBO[2];`);
        fboMembers.push(`\tint ${buf.safeName}Front = 0;`);
      } else {
        fboMembers.push(`\tffglex::FFGLFBO ${buf.safeName}FBO;`);
      }
    }
    fboMembers.push(`\tbool fbosInitialized = false;`);
  }

  // --------------- Generate .h ---------------

  const headerCode = `#pragma once
#include <FFGLSDK.h>

class ${className} : public CFFGLPlugin
{
public:
\t${className}();

\tFFResult InitGL( const FFGLViewportStruct* vp ) override;
\tFFResult ProcessOpenGL( ProcessOpenGLStruct* pGL ) override;
\tFFResult DeInitGL() override;

\tFFResult SetFloatParameter( unsigned int dwIndex, float value ) override;
\tfloat GetFloatParameter( unsigned int index ) override;

private:
${memberDecls.join('\n')}

\tffglex::FFGLShader shader;
\tffglex::FFGLScreenQuad quad;
\tint frameIndex = 0;
${fboMembers.join('\n')}
};
`;

  // --------------- Generate .cpp ---------------

  const enumEntries = params.map(p => {
    const enumName = `PT_${safeName(p.name).toUpperCase()}${p.component ? '_' + p.component.toUpperCase() : ''}`;
    return `\t${enumName} = ${p.index},`;
  });

  const paramRegistrations = [];
  for (const p of params) {
    const enumName = `PT_${safeName(p.name).toUpperCase()}${p.component ? '_' + p.component.toUpperCase() : ''}`;
    if (p.isOption) {
      paramRegistrations.push(`\tSetOptionParamInfo( ${enumName}, "${p.label}", ${p.optionLabels.length}, ${p.default} );`);
      for (let i = 0; i < p.optionLabels.length; i++) {
        paramRegistrations.push(`\tSetParamElementInfo( ${enumName}, ${i}, "${p.optionLabels[i]}", ${p.optionValues[i]}.0f );`);
      }
    } else {
      paramRegistrations.push(`\tSetParamInfof( ${enumName}, "${p.label}", ${p.ffglType} );`);
    }
  }

  const defaultInits = [];
  for (const cn of colorNames) {
    const colorParams = params.filter(p => p.name === cn);
    for (const cp of colorParams) {
      defaultInits.push(`\t${safeName(cn)}.${cp.component} = ${cp.default.toFixed(4)}f;`);
    }
  }
  for (const pn of pointNames) {
    const ptParams = params.filter(p => p.name === pn);
    for (const pp of ptParams) {
      defaultInits.push(`\t${safeName(pn)}.${pp.component} = ${pp.default.toFixed(4)}f;`);
    }
  }

  const setCases = params.map(p => {
    const enumName = `PT_${safeName(p.name).toUpperCase()}${p.component ? '_' + p.component.toUpperCase() : ''}`;
    const target = p.component ? `${safeName(p.name)}.${p.component}` : safeName(p.name);
    return `\tcase ${enumName}:\n\t\t${target} = value;\n\t\tbreak;`;
  });

  const getCases = params.map(p => {
    const enumName = `PT_${safeName(p.name).toUpperCase()}${p.component ? '_' + p.component.toUpperCase() : ''}`;
    const target = p.component ? `${safeName(p.name)}.${p.component}` : safeName(p.name);
    return `\tcase ${enumName}:\n\t\treturn ${target};`;
  });

  // Uniform setting code (shared across passes)
  const uniformSets = [];
  const processedNames = new Set();
  for (const inp of parsed.inputs || []) {
    if (processedNames.has(inp.NAME)) continue;
    processedNames.add(inp.NAME);

    if (inp.NAME in HARDCODED_INPUTS) {
      const val = HARDCODED_INPUTS[inp.NAME];
      if (val != null && (inp.TYPE === 'bool' || inp.TYPE === 'float')) {
        uniformSets.push(`\tglUniform1f( shader.FindUniform( "${inp.NAME}" ), ${val} );`);
      }
      continue;
    }

    if (inp.TYPE === 'color') {
      const sn = safeName(inp.NAME);
      uniformSets.push(`\tglUniform4f( shader.FindUniform( "${inp.NAME}" ), ${sn}.r, ${sn}.g, ${sn}.b, ${sn}.a );`);
    } else if (inp.TYPE === 'point2D') {
      const sn = safeName(inp.NAME);
      uniformSets.push(`\tglUniform2f( shader.FindUniform( "${inp.NAME}" ), ${sn}.x, ${sn}.y );`);
    } else if (inp.TYPE === 'float') {
      const p = params.find(pp => pp.name === inp.NAME && !pp.component);
      if (p && p.needsRescale) {
        uniformSets.push(`\tglUniform1f( shader.FindUniform( "${inp.NAME}" ), ${p.rangeMin.toFixed(1)}f + ${safeName(inp.NAME)} * ${(p.rangeMax - p.rangeMin).toFixed(4)}f );`);
      } else {
        uniformSets.push(`\tglUniform1f( shader.FindUniform( "${inp.NAME}" ), ${safeName(inp.NAME)} );`);
      }
    } else if (inp.TYPE === 'bool' || inp.TYPE === 'event') {
      uniformSets.push(`\tglUniform1f( shader.FindUniform( "${inp.NAME}" ), ${safeName(inp.NAME)} > 0.5f ? 1.0f : 0.0f );`);
    } else if (inp.TYPE === 'long') {
      uniformSets.push(`\tglUniform1f( shader.FindUniform( "${inp.NAME}" ), ${safeName(inp.NAME)} );`);
    }
  }

  // Generate ProcessOpenGL body
  let processBody;
  if (isMultiPass) {
    processBody = generateMultiPassProcess(className, buffers, passes, uniformSets, usesMouse, usesAudio);
  } else {
    processBody = generateSinglePassProcess(uniformSets, usesMouse, usesAudio);
  }

  // Generate InitGL body
  let initGLBody;
  if (isMultiPass) {
    initGLBody = `\tif( !shader.Compile( vertexShaderCode, fragmentShaderCode ) )
\t{
\t\tDeInitGL();
\t\treturn FF_FAIL;
\t}
\tif( !quad.Initialise() )
\t{
\t\tDeInitGL();
\t\treturn FF_FAIL;
\t}

\treturn CFFGLPlugin::InitGL( vp );`;
  } else {
    initGLBody = `\tif( !shader.Compile( vertexShaderCode, fragmentShaderCode ) )
\t{
\t\tDeInitGL();
\t\treturn FF_FAIL;
\t}
\tif( !quad.Initialise() )
\t{
\t\tDeInitGL();
\t\treturn FF_FAIL;
\t}

\treturn CFFGLPlugin::InitGL( vp );`;
  }

  // Generate DeInitGL body
  let deInitGLBody = `\tshader.FreeGLResources();
\tquad.Release();`;
  if (isMultiPass) {
    for (const buf of buffers) {
      if (buf.persistent) {
        deInitGLBody += `\n\t${buf.safeName}FBO[0].Release();\n\t${buf.safeName}FBO[1].Release();`;
      } else {
        deInitGLBody += `\n\t${buf.safeName}FBO.Release();`;
      }
    }
    deInitGLBody += `\n\tfbosInitialized = false;`;
  }

  const fragSafe = fragShader.includes(')"') ? fragShader.replace(/\)"/g, ') "') : fragShader;
  const vertSafe = vertShader.includes(')"') ? vertShader.replace(/\)"/g, ') "') : vertShader;

  const cppCode = `#include "${className}.h"
#include <math.h>
${isMultiPass ? '#include <ffglex/FFGLFBO.h>\n#include <ffglex/FFGLScopedFBOBinding.h>' : ''}
using namespace ffglex;

enum ParamType : FFUInt32
{
${enumEntries.length > 0 ? enumEntries.join('\n') : '\tPT_DUMMY = 0,'}
};

static CFFGLPluginInfo PluginInfo(
\tPluginFactory< ${className} >,
\t"${pluginID}",
\t"${pluginName.substring(0, 48)}",
\t2,
\t1,
\t1,
\t000,
\tFF_SOURCE,
\t"${(parsed.meta.DESCRIPTION || pluginName).substring(0, 64).replace(/"/g, '\\"')}",
\t"ShaderClaw3 ISF"
);

static const char vertexShaderCode[] = R"(${vertSafe})";

static const char fragmentShaderCode[] = R"(${fragSafe})";

${className}::${className}()
{
\tSetMinInputs( 0 );
\tSetMaxInputs( 0 );

${defaultInits.join('\n')}

${paramRegistrations.join('\n')}

\tFFGLLog::LogToHost( "Created ${pluginName.replace(/"/g, '\\"')}" );
}

FFResult ${className}::InitGL( const FFGLViewportStruct* vp )
{
${initGLBody}
}

FFResult ${className}::ProcessOpenGL( ProcessOpenGLStruct* pGL )
{
${processBody}
}

FFResult ${className}::DeInitGL()
{
${deInitGLBody}

\treturn FF_SUCCESS;
}

FFResult ${className}::SetFloatParameter( unsigned int dwIndex, float value )
{
\tswitch( dwIndex )
\t{
${setCases.join('\n')}
\tdefault:
\t\treturn FF_FAIL;
\t}

\treturn FF_SUCCESS;
}

float ${className}::GetFloatParameter( unsigned int index )
{
\tswitch( index )
\t{
${getCases.join('\n')}
\t}

\treturn 0.0f;
}
`;

  return {
    className, pluginName, pluginID,
    headerCode, cppCode, fragShader, vertShader, filename,
    inputCount: inputs.length, paramCount: params.length,
    usesAudio, usesMouse, isMultiPass,
    passCount: passes.length,
    bufferCount: buffers.length,
  };
}

// --------------- Single-Pass ProcessOpenGL ---------------

function generateSinglePassProcess(uniformSets, usesMouse, usesAudio) {
  let code = `\tframeIndex++;

\tScopedShaderBinding shaderBinding( shader.GetGLID() );

\tglUniform1f( shader.FindUniform( "TIME" ), (float)hostTime );
\tglUniform2f( shader.FindUniform( "RENDERSIZE" ),
\t\t(float)currentViewport.width, (float)currentViewport.height );
\tglUniform1i( shader.FindUniform( "FRAMEINDEX" ), frameIndex );
`;
  if (usesMouse) {
    code += `\n\tglUniform2f( shader.FindUniform( "mousePos" ), 0.5f, 0.5f );
\tglUniform1f( shader.FindUniform( "mouseDown" ), 0.0f );
`;
  }
  if (usesAudio) {
    code += `\n\tglUniform1f( shader.FindUniform( "audioLevel" ), 0.0f );
\tglUniform1f( shader.FindUniform( "audioBass" ), 0.0f );
\tglUniform1f( shader.FindUniform( "audioMid" ), 0.0f );
\tglUniform1f( shader.FindUniform( "audioHigh" ), 0.0f );
`;
  }
  code += `\n${uniformSets.join('\n')}

\tquad.Draw();

\treturn FF_SUCCESS;`;
  return code;
}

// --------------- Multi-Pass ProcessOpenGL ---------------

function generateMultiPassProcess(className, buffers, passes, uniformSets, usesMouse, usesAudio) {
  const numPasses = passes.length;

  // Lazy FBO initialization (needs GL context + viewport dimensions)
  let initCode = `\t// Lazy-init FBOs on first frame (need active GL context)
\tif( !fbosInitialized )
\t{
`;
  for (const buf of buffers) {
    const w = buf.widthExpr || 'currentViewport.width';
    const h = buf.heightExpr || 'currentViewport.height';
    if (buf.persistent) {
      initCode += `\t\t${buf.safeName}FBO[0].Initialise( ${w}, ${h}, GL_RGBA16F );
\t\t${buf.safeName}FBO[1].Initialise( ${w}, ${h}, GL_RGBA16F );
`;
    } else {
      initCode += `\t\t${buf.safeName}FBO.Initialise( ${w}, ${h}, GL_RGBA16F );
`;
    }
  }
  initCode += `\t\tfbosInitialized = true;
\t}
`;

  let code = `\tframeIndex++;

${initCode}
\tScopedShaderBinding shaderBinding( shader.GetGLID() );

\t// Set shared uniforms
\tglUniform1f( shader.FindUniform( "TIME" ), (float)hostTime );
\tglUniform1i( shader.FindUniform( "FRAMEINDEX" ), frameIndex );
`;

  if (usesMouse) {
    code += `\tglUniform2f( shader.FindUniform( "mousePos" ), 0.5f, 0.5f );
\tglUniform1f( shader.FindUniform( "mouseDown" ), 0.0f );
`;
  }
  if (usesAudio) {
    code += `\tglUniform1f( shader.FindUniform( "audioLevel" ), 0.0f );
\tglUniform1f( shader.FindUniform( "audioBass" ), 0.0f );
\tglUniform1f( shader.FindUniform( "audioMid" ), 0.0f );
\tglUniform1f( shader.FindUniform( "audioHigh" ), 0.0f );
`;
  }

  code += `\n${uniformSets.join('\n')}\n`;

  // Generate each pass
  for (let i = 0; i < numPasses; i++) {
    const pass = passes[i];
    const isFinalPass = !pass.TARGET;
    const buf = buffers.find(b => b.name === pass.TARGET);

    code += `\n\t// --- Pass ${i}${isFinalPass ? ' (final - render to screen)' : ` (render to ${pass.TARGET})`} ---
\tglUniform1i( shader.FindUniform( "PASSINDEX" ), ${i} );
`;

    if (!isFinalPass && buf) {
      // Render to FBO
      const fboRef = buf.persistent ? `${buf.safeName}FBO[${buf.safeName}Front]` : `${buf.safeName}FBO`;
      const w = buf.widthExpr || 'currentViewport.width';
      const h = buf.heightExpr || 'currentViewport.height';

      code += `\tglUniform2f( shader.FindUniform( "RENDERSIZE" ), (float)(${w}), (float)(${h}) );
\t{
\t\tScopedFBOBinding fboBinding( ${fboRef}.GetGLID(), ScopedFBOBinding::RB_REVERT );
\t\tglViewport( 0, 0, ${w}, ${h} );
`;

      // Bind all buffer textures as samplers for this pass to read from
      let samplerUnit = 0;
      for (const otherBuf of buffers) {
        // For persistent buffers, read from the BACK buffer (previous frame's output)
        let texRef;
        if (otherBuf.persistent) {
          if (otherBuf.name === pass.TARGET) {
            // Reading from the buffer we're about to write to: use back buffer
            texRef = `${otherBuf.safeName}FBO[1 - ${otherBuf.safeName}Front].GetTextureInfo().Handle`;
          } else {
            // Reading from another buffer: use its front (most recent write)
            texRef = `${otherBuf.safeName}FBO[${otherBuf.safeName}Front].GetTextureInfo().Handle`;
          }
        } else {
          texRef = `${otherBuf.safeName}FBO.GetTextureInfo().Handle`;
        }
        code += `\t\tglActiveTexture( GL_TEXTURE${samplerUnit} );
\t\tglBindTexture( GL_TEXTURE_2D, ${texRef} );
\t\tglUniform1i( shader.FindUniform( "${otherBuf.name}" ), ${samplerUnit} );
`;
        samplerUnit++;
      }

      code += `\t\tquad.Draw();
\t}
`;
      // Flip ping-pong for persistent buffers
      if (buf.persistent) {
        code += `\t${buf.safeName}Front = 1 - ${buf.safeName}Front;
`;
      }
    } else {
      // Final pass: render to screen
      code += `\tglUniform2f( shader.FindUniform( "RENDERSIZE" ),
\t\t(float)currentViewport.width, (float)currentViewport.height );
\tglViewport( 0, 0, currentViewport.width, currentViewport.height );
`;

      // Bind all buffer textures
      let samplerUnit = 0;
      for (const otherBuf of buffers) {
        let texRef;
        if (otherBuf.persistent) {
          texRef = `${otherBuf.safeName}FBO[${otherBuf.safeName}Front].GetTextureInfo().Handle`;
        } else {
          texRef = `${otherBuf.safeName}FBO.GetTextureInfo().Handle`;
        }
        code += `\tglActiveTexture( GL_TEXTURE${samplerUnit} );
\tglBindTexture( GL_TEXTURE_2D, ${texRef} );
\tglUniform1i( shader.FindUniform( "${otherBuf.name}" ), ${samplerUnit} );
`;
        samplerUnit++;
      }

      code += `\tquad.Draw();

\t// Reset texture state
\tglActiveTexture( GL_TEXTURE0 );
`;
    }
  }

  code += `\n\treturn FF_SUCCESS;`;
  return code;
}

// --------------- CMake Generation ---------------

function generateCMakeLists(plugins) {
  let cmake = `cmake_minimum_required(VERSION 3.15)
project(ShaderClaw-FFGL VERSION 1.0 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)

# FFGL SDK location
set(FFGL_SDK_DIR "$ENV{USERPROFILE}/ffgl-sdk" CACHE PATH "Path to FFGL SDK")

# Point find_package(GLEW) at the bundled copy in FFGL SDK deps
set(GLEW_INCLUDE_DIR "\${FFGL_SDK_DIR}/deps/glew-2.1.0/include" CACHE PATH "" FORCE)
set(GLEW_LIBRARY "\${FFGL_SDK_DIR}/deps/glew-2.1.0/lib/Release/x64/glew32s.lib" CACHE FILEPATH "" FORCE)
set(GLEW_STATIC_LIBRARY_RELEASE "\${FFGL_SDK_DIR}/deps/glew-2.1.0/lib/Release/x64/glew32s.lib" CACHE FILEPATH "" FORCE)
set(GLEW_USE_STATIC_LIBS ON CACHE BOOL "" FORCE)
add_compile_definitions(GLEW_STATIC)

# FFGL SDK
set(FFGL_BUILD_EXAMPLE_PLUGINS OFF CACHE BOOL "" FORCE)
add_subdirectory(\${FFGL_SDK_DIR} ffgl-sdk-build)

`;

  for (const plugin of plugins) {
    const target = `ffgl-${plugin.className.toLowerCase()}`;
    cmake += `# ${plugin.pluginName}
add_library(${target} SHARED
\tplugins/${plugin.className}/${plugin.className}.h
\tplugins/${plugin.className}/${plugin.className}.cpp
)
target_link_libraries(${target} PRIVATE ffgl::sdk opengl32)
target_include_directories(${target} PRIVATE \${GLEW_INCLUDE_DIR})
set_target_properties(${target} PROPERTIES
\tOUTPUT_NAME "${plugin.className}"
\tSUFFIX ".dll"
\tPREFIX ""
)

`;
  }

  cmake += `# Install to Resolume plugin directory
set(RESOLUME_PLUGIN_DIR "$ENV{USERPROFILE}/Documents/Resolume Arena/Extra Effects" CACHE PATH "Resolume plugin directory")
install(TARGETS
`;
  for (const plugin of plugins) {
    cmake += `\tffgl-${plugin.className.toLowerCase()}\n`;
  }
  cmake += `\tDESTINATION \${RESOLUME_PLUGIN_DIR}
)
`;

  return cmake;
}

// --------------- Main ---------------

function main() {
  const args = process.argv.slice(2);
  const shadersDir = args[0] || path.resolve(__dirname, '../../shaders');
  const outputDir = args[1] || path.resolve(__dirname, 'output');

  console.log(`ISF-to-FFGL Converter`);
  console.log(`Shaders: ${shadersDir}`);
  console.log(`Output:  ${outputDir}`);
  console.log('');

  const files = fs.readdirSync(shadersDir).filter(f => f.endsWith('.fs'));
  console.log(`Found ${files.length} ISF shaders\n`);

  fs.mkdirSync(path.join(outputDir, 'plugins'), { recursive: true });

  const plugins = [];
  const skipped = [];

  for (const file of files) {
    const isfPath = path.join(shadersDir, file);
    process.stdout.write(`  Converting ${file}...`);

    try {
      const result = generateFFGLPlugin(isfPath);
      if (!result) {
        skipped.push(file);
        continue;
      }

      const pluginDir = path.join(outputDir, 'plugins', result.className);
      fs.mkdirSync(pluginDir, { recursive: true });
      fs.writeFileSync(path.join(pluginDir, `${result.className}.h`), result.headerCode);
      fs.writeFileSync(path.join(pluginDir, `${result.className}.cpp`), result.cppCode);

      plugins.push(result);
      const tags = [];
      if (result.paramCount) tags.push(`${result.paramCount} params`);
      if (result.isMultiPass) tags.push(`${result.passCount} passes`);
      if (result.usesAudio) tags.push('audio');
      if (result.usesMouse) tags.push('mouse');
      console.log(` OK (${tags.join(', ')})`);
    } catch (e) {
      console.log(` FAIL: ${e.message}`);
      skipped.push(file);
    }
  }

  const cmakeContent = generateCMakeLists(plugins);
  fs.writeFileSync(path.join(outputDir, 'CMakeLists.txt'), cmakeContent);

  console.log(`\n========================================`);
  console.log(`Converted: ${plugins.length} shaders`);
  console.log(`Skipped:   ${skipped.length}${skipped.length > 0 ? ' (' + skipped.join(', ') + ')' : ''}`);
  console.log(`Output:    ${outputDir}`);
  console.log(`\nNext steps:`);
  console.log(`  cd ${outputDir}`);
  console.log(`  cmake -B build -G "Visual Studio 17 2022" -A x64`);
  console.log(`  cmake --build build --config Release`);
  console.log(`  cmake --install build`);
}

main();

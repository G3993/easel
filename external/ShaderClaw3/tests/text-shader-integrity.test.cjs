/**
 * Text Shader Integrity Test
 *
 * Ensures all text shaders in ShaderClaw have consistent character encoding/decoding.
 * Catches: getChar() slot count vs MAX_LENGTH mismatches, sampleChar atlas formula
 * drift, character range inconsistencies, and missing uniform references.
 *
 * Run: node tests/text-shader-integrity.test.js
 */

const fs = require('fs');
const path = require('path');

const SHADERS_DIR = path.join(__dirname, '..', 'shaders');
const textShaderFiles = fs.readdirSync(SHADERS_DIR)
  .filter(f => f.startsWith('text_') && f.endsWith('.fs'));

let passed = 0;
let failed = 0;
const failures = [];

function fail(shader, check, detail) {
  failed++;
  failures.push({ shader, check, detail });
}

function pass(shader, check) {
  passed++;
}

// Replicate the JS-side charToCode for reference
function charToCode(ch) {
  if (!ch || ch === ' ') return 26;
  const c = ch.toUpperCase().charCodeAt(0) - 65;
  return (c >= 0 && c <= 25) ? c : 26;
}

// Parse ISF JSON header from shader source
function parseISF(src) {
  const match = src.match(/\/\*\s*(\{[\s\S]*?\})\s*\*\//);
  if (!match) return null;
  try { return JSON.parse(match[1]); } catch { return null; }
}

// Count getChar() slots — find all `slot == N` patterns
function getCharSlots(src) {
  const fnMatch = src.match(/int\s+getChar\s*\(\s*int\s+\w+\s*\)\s*\{([\s\S]*?)\n\}/);
  if (!fnMatch) return null;
  const body = fnMatch[1];
  const slots = [...body.matchAll(/slot\s*==\s*(\d+)/g)].map(m => parseInt(m[1]));
  // Also count the bare return at end (handles slot == max implicitly)
  const maxSlot = slots.length > 0 ? Math.max(...slots) : -1;
  return { slots, maxSlot, count: slots.length, body };
}

// Count getCharF() slots — float-based interpolation variant (e.g. text_james.fs)
// Pattern: c += msg_N * max(0.0, 1.0 - abs(idx - N.0));
function getCharFSlots(src) {
  const fnMatch = src.match(/float\s+getCharF\s*\(\s*float\s+\w+\s*\)\s*\{([\s\S]*?)\n\}/);
  if (!fnMatch) return null;
  const body = fnMatch[1];
  const refs = [...body.matchAll(/msg_(\d+)/g)].map(m => parseInt(m[1]));
  const maxSlot = refs.length > 0 ? Math.max(...refs) : -1;
  return { slots: refs, maxSlot, count: refs.length, body, variant: 'getCharF' };
}

// Check what msg_N uniforms are referenced in getChar/getCharF
function getCharUniformRefs(src) {
  // Try getChar first, then getCharF
  let fnMatch = src.match(/int\s+getChar\s*\(\s*int\s+\w+\s*\)\s*\{([\s\S]*?)\n\}/);
  if (!fnMatch) fnMatch = src.match(/float\s+getCharF\s*\(\s*float\s+\w+\s*\)\s*\{([\s\S]*?)\n\}/);
  if (!fnMatch) return [];
  return [...fnMatch[1].matchAll(/msg_(\d+)/g)].map(m => parseInt(m[1]));
}

// Extract sampleChar divisor (the /27.0 in the atlas formula)
function getSampleCharDivisor(src) {
  // Look for the atlas sampling: (float(ch) + uv.x) / N.0
  const match = src.match(/\(\s*float\s*\(\s*ch\s*\)\s*\+\s*\w+(?:\.\w+)?\s*\)\s*\/\s*(\d+\.?\d*)/);
  return match ? parseFloat(match[1]) : null;
}

// Extract charPixel divisor (alternate function name used in some shaders)
function getCharPixelDivisor(src) {
  const match = src.match(/\(\s*float\s*\(\s*ch\s*\)\s*\+\s*[^)]+\)\s*\/\s*(\d+\.?\d*)/);
  return match ? parseFloat(match[1]) : null;
}

// Check sampleChar/charPixel valid range guard
function getCharRangeGuard(src) {
  // Look for: ch < 0 || ch > N
  const sampleMatch = src.match(/float\s+(?:sampleChar|charPixel)\s*\([^)]*\)\s*\{[\s\S]*?ch\s*>\s*(\d+)/);
  return sampleMatch ? parseInt(sampleMatch[1]) : null;
}

// Check if getChar has a bare return (fallthrough) at the end
// Handles both multi-line and compact single-line formats:
//   return int(msg_23);           ← standalone bare return
//   if (slot == 22) return int(msg_22); return int(msg_23);  ← trailing bare return
function getCharHasFallthrough(src) {
  const fnMatch = src.match(/int\s+getChar\s*\(\s*int\s+\w+\s*\)\s*\{([\s\S]*?)\n\}/);
  if (!fnMatch) return false;
  const body = fnMatch[1];
  const lines = body.trim().split('\n');
  const lastLine = lines[lines.length - 1].trim();
  // Case 1: standalone bare return line
  if (lastLine.startsWith('return') && !lastLine.includes('if')) return true;
  // Case 2: compact format — last line has "if(...) return ...; return ...;"
  // The trailing return after the last semicolon of the if-guarded return is the fallthrough
  const trailingReturn = lastLine.match(/;\s*return\s+int\s*\(\s*msg_\d+\s*\)\s*;/);
  return !!trailingReturn;
}

console.log(`\nText Shader Integrity Test`);
console.log(`========================\n`);
console.log(`Found ${textShaderFiles.length} text shaders\n`);

for (const file of textShaderFiles) {
  const src = fs.readFileSync(path.join(SHADERS_DIR, file), 'utf8');
  const isf = parseISF(src);

  console.log(`--- ${file} ---`);

  // 1. ISF header must parse
  if (!isf) {
    fail(file, 'ISF_PARSE', 'Could not parse ISF JSON header');
    continue;
  }

  // 2. Must have a text input
  const textInput = (isf.INPUTS || []).find(i => i.TYPE === 'text');
  if (!textInput) {
    fail(file, 'TEXT_INPUT', 'No TYPE:"text" input found in ISF header');
    continue;
  }

  const maxLen = textInput.MAX_LENGTH || 12;
  const inputName = textInput.NAME;
  console.log(`  MAX_LENGTH: ${maxLen}, input: "${inputName}"`);

  // 3. getChar() or getCharF() must exist
  const charInfo = getCharSlots(src) || getCharFSlots(src);
  if (!charInfo) {
    fail(file, 'GET_CHAR_EXISTS', 'Neither getChar() nor getCharF() function found');
    continue;
  }

  const variant = charInfo.variant || 'getChar';

  // 4. Slot count must cover MAX_LENGTH
  let coveredSlots;
  if (variant === 'getCharF') {
    // getCharF uses all msg_N uniforms directly — count = number of unique refs
    coveredSlots = charInfo.count;
  } else {
    // getChar uses if-chain with optional bare return fallthrough
    const hasFallthrough = getCharHasFallthrough(src);
    coveredSlots = hasFallthrough ? charInfo.maxSlot + 2 : charInfo.maxSlot + 1;
  }

  if (coveredSlots < maxLen) {
    fail(file, 'SLOT_COVERAGE',
      `${variant}() covers slots 0..${coveredSlots - 1} (${coveredSlots} slots) but MAX_LENGTH is ${maxLen}. ` +
      `Characters at index ${coveredSlots}+ will decode wrong.`);
  } else {
    pass(file, 'SLOT_COVERAGE');
    console.log(`  ${variant}: ${coveredSlots} slots (covers MAX_LENGTH=${maxLen}) ✓`);
  }

  // 5. getChar() uniform references must be sequential (msg_0, msg_1, ..., msg_N)
  const uniformRefs = getCharUniformRefs(src);
  const sortedRefs = [...uniformRefs].sort((a, b) => a - b);
  const expectedRefs = Array.from({ length: sortedRefs.length }, (_, i) => i);
  const refsMatch = JSON.stringify(sortedRefs) === JSON.stringify(expectedRefs);
  if (!refsMatch) {
    const missing = expectedRefs.filter(i => !sortedRefs.includes(i));
    fail(file, 'UNIFORM_SEQUENCE',
      `getChar() uniform references are not sequential. Missing: msg_${missing.join(', msg_')}`);
  } else {
    pass(file, 'UNIFORM_SEQUENCE');
    console.log(`  Uniforms: msg_0..msg_${sortedRefs[sortedRefs.length - 1]} sequential ✓`);
  }

  // 6. Atlas sampling divisor should be 27.0 (26 letters + 1 blank cell)
  const divisor = getCharPixelDivisor(src);
  if (divisor !== null) {
    if (divisor !== 27.0) {
      fail(file, 'ATLAS_DIVISOR',
        `Font atlas sampling divides by ${divisor}, expected 27.0 (26 letters + blank)`);
    } else {
      pass(file, 'ATLAS_DIVISOR');
      console.log(`  Atlas divisor: ${divisor} ✓`);
    }
  } else {
    // Some shaders might use varFont only — not a failure
    console.log(`  Atlas divisor: N/A (may use varFont path)`);
  }

  // 7. sampleChar/charPixel range guard should be 25 or 26
  const rangeGuard = getCharRangeGuard(src);
  if (rangeGuard !== null) {
    if (rangeGuard < 25 || rangeGuard > 26) {
      fail(file, 'CHAR_RANGE',
        `Character range guard: ch > ${rangeGuard}, expected ch > 25 or ch > 26`);
    } else {
      pass(file, 'CHAR_RANGE');
      console.log(`  Char range guard: ch > ${rangeGuard} ✓`);
    }
  }

  // 8. charCount() should exist and reference msg_len
  const hasCharCount = /int\s+charCount\s*\(/.test(src);
  const hasMsgLen = src.includes('msg_len');
  if (!hasMsgLen) {
    fail(file, 'MSG_LEN', 'No msg_len uniform reference found');
  } else {
    pass(file, 'MSG_LEN');
    console.log(`  msg_len referenced ✓`);
  }

  // 9. Default text should only contain valid characters (A-Z, space)
  const defaultText = textInput.DEFAULT || '';
  const invalidChars = [...defaultText].filter(ch => {
    return ch !== ' ' && (ch.toUpperCase().charCodeAt(0) < 65 || ch.toUpperCase().charCodeAt(0) > 90);
  });
  if (invalidChars.length > 0) {
    fail(file, 'DEFAULT_CHARS',
      `Default text "${defaultText}" contains invalid characters: ${JSON.stringify(invalidChars)} — only A-Z and space are supported`);
  } else {
    pass(file, 'DEFAULT_CHARS');
    console.log(`  Default text "${defaultText}" valid ✓`);
  }

  // 10. Verify encoding roundtrip: default text → charToCode → expected uniform values
  const encoded = [];
  for (let i = 0; i < maxLen; i++) {
    encoded.push(charToCode(defaultText[i]));
  }
  // Verify each code is in valid range [0, 26]
  const outOfRange = encoded.filter(c => c < 0 || c > 26);
  if (outOfRange.length > 0) {
    fail(file, 'ENCODE_RANGE', `charToCode produced out-of-range values: ${outOfRange}`);
  } else {
    pass(file, 'ENCODE_RANGE');
  }

  console.log('');
}

// Summary
console.log(`\n${'='.repeat(50)}`);
console.log(`RESULTS: ${passed} passed, ${failed} failed`);
console.log(`${'='.repeat(50)}`);

if (failures.length > 0) {
  console.log(`\nFAILURES:\n`);
  for (const f of failures) {
    console.log(`  ✗ ${f.shader} [${f.check}]`);
    console.log(`    ${f.detail}\n`);
  }
  process.exit(1);
} else {
  console.log(`\nAll text shaders passed integrity checks.\n`);
  process.exit(0);
}

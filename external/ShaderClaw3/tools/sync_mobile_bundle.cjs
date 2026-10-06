#!/usr/bin/env node
// ============================================================
// Sync the EaselMobile ShaderPreview bundle from ShaderClaw3.
//   node tools/sync_mobile_bundle.cjs
// - copies js/isf.js (the real ShaderClaw pipeline) into the bundle
// - copies tools/mobile_shaderpreview.html -> shaderpreview.html
// - copies the current rail shaders from shaders/
// - deletes stale .fs files no longer on the rail
// Keep RAIL in sync with fluxShaders in ShowController.swift.
// ============================================================

const fs = require('fs');
const path = require('path');

const SRC = path.join(__dirname, '..');
const DST = '/Users/lu/easel-mobile-v2/EaselMobile/EaselMobile/Resources/ShaderPreview';

const RAIL = [
  'cfd_paint.fs',        // Fluid
  'apollonian_gasket.fs',// Fractal
  'nebulay.fs',          // Nebula
  'rainbow_flower.fs',   // Candy
  'dancing_cubes.fs',    // Cubes
  'droste_spiral.fs',    // Twist
];

let changed = 0;
function copy(src, dst) {
  const a = fs.readFileSync(src);
  if (fs.existsSync(dst) && fs.readFileSync(dst).equals(a)) return;
  fs.writeFileSync(dst, a);
  console.log('  updated ' + path.basename(dst));
  changed++;
}

copy(path.join(SRC, 'js', 'isf.js'), path.join(DST, 'isf.js'));
copy(path.join(SRC, 'tools', 'mobile_shaderpreview.html'), path.join(DST, 'shaderpreview.html'));
for (const f of RAIL) copy(path.join(SRC, 'shaders', f), path.join(DST, f));

// prune stale shaders
for (const f of fs.readdirSync(DST)) {
  if (f.endsWith('.fs') && !RAIL.includes(f)) {
    fs.unlinkSync(path.join(DST, f));
    console.log('  deleted stale ' + f);
    changed++;
  }
}
console.log(changed ? `sync done (${changed} changes)` : 'bundle already up to date');

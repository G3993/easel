#!/usr/bin/env node
// Mechanically tag the handful of universal/near-universal ISF param names
// with a consistent GROUP across every shader, before the judgment-based
// per-shader content pass runs. Idempotent (skips inputs that already have
// a GROUP). Header-JSON-only edit; verified after with the eval harness.
const fs = require('fs');
const path = require('path');
const DIR = path.join(__dirname, '..', 'shaders');

const NAME_TO_GROUP = {
  hueShift: 'Color',
  colorBoost: 'Color',
  bgColor: 'Background',
  transparentBg: 'Background',
  audioReact: 'Audio Reactivity',
  audioReactivity: 'Audio Reactivity',
};

let filesChanged = 0, paramsTagged = 0;
for (const file of fs.readdirSync(DIR).filter(f => f.endsWith('.fs'))) {
  const fp = path.join(DIR, file);
  const src = fs.readFileSync(fp, 'utf8');
  const m = src.match(/\/\*([\s\S]*?)\*\//);
  if (!m) continue;
  let hdr;
  try { hdr = JSON.parse(m[1]); } catch (e) { console.error('PARSE FAIL', file, e.message); continue; }
  if (!Array.isArray(hdr.INPUTS)) continue;
  let changed = false;
  for (const inp of hdr.INPUTS) {
    if (inp.GROUP) continue;
    const g = NAME_TO_GROUP[inp.NAME];
    if (g) { inp.GROUP = g; changed = true; paramsTagged++; }
  }
  if (changed) {
    const newHeader = '/*' + JSON.stringify(hdr, null, 2) + '*/';
    const newSrc = src.slice(0, m.index) + newHeader + src.slice(m.index + m[0].length);
    fs.writeFileSync(fp, newSrc);
    filesChanged++;
  }
}
console.log('files changed:', filesChanged, ' params tagged:', paramsTagged);

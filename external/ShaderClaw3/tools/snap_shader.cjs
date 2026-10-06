#!/usr/bin/env node
// ============================================================
// Snapshot a shader as PNG(s) at arbitrary resolution via the
// eval renderer (same compile path, same audio bus).
//   node tools/snap_shader.cjs --files a.fs,b.fs [--size 1024]
//     [--audio 0.65] [--outdir /tmp/snaps] [--both]
// --both writes <name>_silent.png and <name>_loud.png
// ============================================================

const http = require('http');
const fs = require('fs');
const path = require('path');
const puppeteer = require('puppeteer-core');

const ROOT = path.join(__dirname, '..');
const CHROME = '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome';

const args = process.argv.slice(2);
function argVal(flag, dflt) { const i = args.indexOf(flag); return i >= 0 ? args[i + 1] : dflt; }
const files = (argVal('--files', '') || '').split(',').map(s => s.trim()).filter(Boolean);
const size = Number(argVal('--size', 1024));
const audio = Number(argVal('--audio', 0.65));
const outdir = argVal('--outdir', '/tmp/snaps');
const both = args.includes('--both');
if (!files.length) { console.error('need --files a.fs,b.fs'); process.exit(1); }
fs.mkdirSync(outdir, { recursive: true });

const MIME = { '.html': 'text/html', '.js': 'application/javascript', '.json': 'application/json', '.fs': 'text/plain', '.png': 'image/png' };
const server = http.createServer((req, res) => {
  const fp = path.join(ROOT, decodeURIComponent(req.url.split('?')[0]));
  if (!fp.startsWith(ROOT)) { res.writeHead(403); res.end(); return; }
  fs.readFile(fp, (err, data) => {
    if (err) { res.writeHead(404); res.end('nf'); return; }
    res.writeHead(200, { 'Content-Type': MIME[path.extname(fp)] || 'application/octet-stream' });
    res.end(data);
  });
});

async function main() {
  await new Promise((r) => server.listen(0, r));
  const port = server.address().port;
  const browser = await puppeteer.launch({
    executablePath: CHROME, headless: 'new',
    args: ['--no-sandbox', '--disable-dev-shm-usage', '--enable-unsafe-swiftshader'],
  });
  const page = await browser.newPage();
  page.on('pageerror', (e) => console.error('  [pageerror]', e.message));
  await page.goto(`http://localhost:${port}/tools/eval_page.html?size=${size}`, { waitUntil: 'load' });
  await page.waitForFunction('window.__harnessReady === true', { timeout: 15000 });

  for (const f of files) {
    const variants = both ? [['silent', 0], ['loud', audio]] : [['snap', audio]];
    for (const [tag, a] of variants) {
      try {
        const res = await Promise.race([
          page.evaluate((file, aud) => window.evalShader(file, { captureFrame: true, audio: aud }), f, a),
          new Promise((_, rej) => setTimeout(() => rej(new Error('TIMEOUT 90s')), 90000)),
        ]);
        if (res && res.thumbnail) {
          const out = path.join(outdir, f.replace('.fs', '') + '_' + tag + '.png');
          fs.writeFileSync(out, Buffer.from(res.thumbnail.split(',')[1], 'base64'));
          console.log('wrote', out, res.ok ? '' : '(shader NOT ok: ' + (res.errors || [])[0] + ')');
        } else {
          console.error('no thumbnail for', f, (res && res.errors || []).join(' | '));
        }
      } catch (e) { console.error('snap failed', f, tag, e.message); }
    }
  }
  await browser.close();
  server.close();
}
main().catch((e) => { console.error(e); process.exit(1); });

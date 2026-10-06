#!/usr/bin/env node
// ============================================================
// Animated shader previews: render N consecutive frames per
// shader via the eval renderer (same compile path + audio bus
// as everything else) and assemble looping GIFs with ffmpeg.
// The easel-agent-sdk serves them at /api/shader-previews/<id>.gif
// for the mobile gallery's hero tiles.
//   node tools/gif_previews.cjs [--size 256] [--frames 14]
//     [--step 0.125] [--fps 8] [--width 220] [--audio 0.65]
//     [--outdir shaders/previews_gif] [--files a.fs,b.fs] [--force]
// Resumable: existing gifs are skipped unless --force.
// ============================================================

const http = require('http');
const fs = require('fs');
const os = require('os');
const path = require('path');
const { execFileSync } = require('child_process');
const puppeteer = require('puppeteer-core');

const ROOT = path.join(__dirname, '..');
// Cross-platform: env overrides first, then the platform's usual spots.
const CHROME = process.env.CHROME_PATH || (process.platform === 'win32'
  ? 'C:/Program Files/Google/Chrome/Application/chrome.exe'
  : '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome');
const FFMPEG = process.env.FFMPEG_PATH || (process.platform === 'win32'
  ? 'ffmpeg' : '/opt/homebrew/bin/ffmpeg');

const args = process.argv.slice(2);
function argVal(flag, dflt) { const i = args.indexOf(flag); return i >= 0 ? args[i + 1] : dflt; }
const size = Number(argVal('--size', 256));
const frames = Number(argVal('--frames', 14));
const step = Number(argVal('--step', 0.125));
const fps = Number(argVal('--fps', 8));
const width = Number(argVal('--width', 220));
const audio = Number(argVal('--audio', 0.65));
const outdir = path.resolve(ROOT, argVal('--outdir', 'shaders/previews_gif'));
const force = args.includes('--force');
let files = (argVal('--files', '') || '').split(',').map(s => s.trim()).filter(Boolean);
if (!files.length) {
  const manifest = JSON.parse(fs.readFileSync(path.join(ROOT, 'shaders/manifest.json'), 'utf8'));
  const entries = Array.isArray(manifest) ? manifest : (manifest.shaders || []);
  files = entries.map(e => (typeof e === 'string' ? e : e.file || (e.id ? e.id + '.fs' : null)))
                 .filter(Boolean);
}
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

  let done = 0, skipped = 0, failed = 0;
  for (const f of files) {
    const id = f.replace(/\.fs$/, '');
    const out = path.join(outdir, id + '.gif');
    if (!force && fs.existsSync(out)) { skipped++; continue; }
    try {
      const res = await Promise.race([
        page.evaluate((file, n, st, aud) =>
          window.evalShader(file, { captureFrames: n, frameStep: st, audio: aud }), f, frames, step, audio),
        new Promise((_, rej) => setTimeout(() => rej(new Error('TIMEOUT 120s')), 120000)),
      ]);
      if (!res || !res.previewFrames || !res.previewFrames.length) {
        console.error(`  [skip] ${id}: no frames (${res && res.error ? res.error : 'unknown'})`);
        failed++;
        continue;
      }
      const tmp = fs.mkdtempSync(path.join(os.tmpdir(), 'gifprev-'));
      res.previewFrames.forEach((dataUrl, i) => {
        fs.writeFileSync(path.join(tmp, `f_${String(i).padStart(2, '0')}.png`),
                         Buffer.from(dataUrl.split(',')[1], 'base64'));
      });
      execFileSync(FFMPEG, [
        '-y', '-loglevel', 'error', '-framerate', String(fps),
        '-i', path.join(tmp, 'f_%02d.png'),
        '-vf', `scale=${width}:-1:flags=lanczos,split[s0][s1];[s0]palettegen[p];[s1][p]paletteuse`,
        '-loop', '0', out,
      ]);
      fs.rmSync(tmp, { recursive: true, force: true });
      done++;
      console.log(`  [ok] ${id} (${done} done)`);
    } catch (e) {
      failed++;
      console.error(`  [fail] ${id}: ${e.message}`);
    }
  }
  console.log(`gifs: ${done} rendered, ${skipped} already present, ${failed} failed -> ${outdir}`);
  await browser.close();
  server.close();
}

main().catch((e) => { console.error(e); process.exit(1); });

#!/usr/bin/env node
// Renders a static PNG thumbnail for every shader using ShaderClaw3's OWN
// proven eval harness pipeline (tools/eval_page.html's window.evalShader,
// captureFrame: true) — the same renderer that already compiles and drives
// all 176 shaders correctly (0 failing in eval_harness.cjs). The app's own
// bundled isf.js library is a different, less complete ISF implementation
// (missing several audio-reactive uniforms like audioBass/audioLevel that
// many shaders reference) and can't compile a large fraction of them — since
// thumbnails are now static (no more live in-app WebGL preview), there's no
// requirement to render through that specific library anymore.
const http = require('http');
const fs = require('fs');
const path = require('path');
const puppeteer = require('puppeteer-core');

const ROOT = path.join(__dirname, '..');
const SHADER_DIR = path.join(ROOT, 'shaders');
const OUT_DIR = process.env.THUMB_OUT ||
  '/Users/lu/easel-mobile-v2/EaselMobile/EaselMobile/Resources/ShaderThumbnails';
const CHROME = '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome';
const ONLY = process.env.THUMB_ONLY ? new Set(process.env.THUMB_ONLY.split(',')) : null;
const RESTART_EVERY = 30;

fs.mkdirSync(OUT_DIR, { recursive: true });

const MIME = {
  '.html': 'text/html', '.js': 'application/javascript', '.json': 'application/json',
  '.css': 'text/css', '.fs': 'text/plain', '.vs': 'text/plain', '.png': 'image/png',
};
const server = http.createServer((req, res) => {
  const urlPath = decodeURIComponent(req.url.split('?')[0]);
  const fp = path.join(ROOT, urlPath);
  if (!fp.startsWith(ROOT)) { res.writeHead(403); res.end(); return; }
  fs.readFile(fp, (err, data) => {
    if (err) { res.writeHead(404); res.end('nf'); return; }
    res.writeHead(200, { 'Content-Type': MIME[path.extname(fp)] || 'application/octet-stream' });
    res.end(data);
  });
});

function withTimeout(promise, ms, label) {
  let timer;
  const timeout = new Promise((_, rej) => {
    timer = setTimeout(() => rej(new Error(`hard-timeout after ${ms}ms: ${label}`)), ms);
  });
  return Promise.race([promise, timeout]).finally(() => clearTimeout(timer));
}

async function main() {
  await new Promise((r) => server.listen(0, r));
  const port = server.address().port;
  let files = fs.readdirSync(SHADER_DIR).filter((f) => f.endsWith('.fs')).sort();
  if (ONLY) files = files.filter((f) => ONLY.has(f.replace(/\.fs$/, '')));
  console.log('shaders to render:', files.length);

  let browser = null;
  let page = null;
  async function boot() {
    if (browser) try { await browser.close(); } catch (e) { /* ignore */ }
    browser = await puppeteer.launch({
      executablePath: CHROME,
      headless: 'new',
      args: ['--no-sandbox', '--disable-dev-shm-usage', '--enable-unsafe-swiftshader'],
    });
    page = await browser.newPage();
    page.on('pageerror', () => {}); // eval_page.html reports errors in its own return value
    await page.goto(`http://127.0.0.1:${port}/tools/eval_page.html`, { waitUntil: 'load' });
    await page.waitForFunction('window.__harnessReady === true', { timeout: 10000 });
  }
  await boot();

  const results = { ok: [], fail: [] };
  let sinceRestart = 0;
  for (let i = 0; i < files.length; i++) {
    const file = files[i];
    const id = file.replace(/\.fs$/, '');
    let res;
    try {
      // audio: 0.65 — audio-reactive shaders ("sound is the brush") render
      // black at the harness's silent default; drive them so their thumbnail
      // shows what they actually look like live. Default sample times give
      // the least-black-frame picker four moments to choose from.
      res = await withTimeout(
        page.evaluate((f) => window.evalShader(f, { captureFrame: true, audio: 0.65 }), file),
        30000, file
      );
    } catch (e) {
      res = { errors: [e.message] };
      await boot();
      sinceRestart = 0;
    }
    if (res && res.thumbnail) {
      const b64 = res.thumbnail.replace(/^data:image\/png;base64,/, '');
      fs.writeFileSync(path.join(OUT_DIR, `${id}.png`), Buffer.from(b64, 'base64'));
      const tf = res.thumbnailFrame || {};
      results.ok.push({ id, blackFrac: tf.blackFrac, time: tf.time });
      console.log(`[${i + 1}/${files.length}] ok   ${id} (t=${tf.time} black=${(tf.blackFrac ?? 0).toFixed(2)})`);
    } else {
      results.fail.push({ id, error: (res && res.errors && res.errors.join('; ')) || 'no thumbnail' });
      console.log(`[${i + 1}/${files.length}] FAIL ${id} — ${results.fail[results.fail.length - 1].error}`);
    }

    sinceRestart++;
    if (sinceRestart >= RESTART_EVERY) { await boot(); sinceRestart = 0; }
  }

  try { await browser.close(); } catch (e) { /* ignore */ }
  server.close();
  console.log('OK:', results.ok.length, 'FAIL:', results.fail.length);
  fs.writeFileSync('/tmp/thumbnail_results.json', JSON.stringify(results, null, 2));
  if (results.fail.length) console.log(JSON.stringify(results.fail, null, 2));
}

main().catch((e) => { console.error(e); process.exit(1); });

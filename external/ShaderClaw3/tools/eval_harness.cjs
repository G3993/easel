#!/usr/bin/env node
// ============================================================
// ShaderClaw eval harness driver
//   node tools/eval_harness.cjs [--only a.fs,b.fs] [--out tools/eval_results.json]
// Serves the repo, opens tools/eval_page.html in headless Chrome,
// runs every manifest shader through the app's real WebGL build
// path, writes per-shader compile/render/score results.
// ============================================================

const http = require('http');
const fs = require('fs');
const path = require('path');
const puppeteer = require('puppeteer-core');

const ROOT = path.join(__dirname, '..');
const CHROME = '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome';
// Port 0 = OS-assigned free port, so parallel harness runs never collide
const PORT = process.env.EVAL_PORT ? Number(process.env.EVAL_PORT) : 0;

const args = process.argv.slice(2);
function argVal(flag, dflt) {
  const i = args.indexOf(flag);
  return i >= 0 ? args[i + 1] : dflt;
}
const only = argVal('--only', null);
const outFile = argVal('--out', path.join(__dirname, 'eval_results.json'));

const MIME = {
  '.html': 'text/html', '.js': 'application/javascript', '.json': 'application/json',
  '.css': 'text/css', '.fs': 'text/plain', '.vs': 'text/plain', '.png': 'image/png',
};

const server = http.createServer((req, res) => {
  let urlPath = decodeURIComponent(req.url.split('?')[0]);
  const fp = path.join(ROOT, urlPath);
  if (!fp.startsWith(ROOT)) { res.writeHead(403); res.end(); return; }
  fs.readFile(fp, (err, data) => {
    if (err) { res.writeHead(404); res.end('nf'); return; }
    res.writeHead(200, { 'Content-Type': MIME[path.extname(fp)] || 'application/octet-stream' });
    res.end(data);
  });
});

async function main() {
  await new Promise((r) => server.listen(PORT, r));
  const port = server.address().port;
  const manifest = JSON.parse(fs.readFileSync(path.join(ROOT, 'shaders', 'manifest.json'), 'utf8'));
  let files = manifest.map((e) => e.file).filter((f) => f && f.endsWith('.fs'));
  if (only) {
    const want = new Set(only.split(',').map((s) => s.trim()));
    files = files.filter((f) => want.has(f));
  }

  let browser = null;
  let page = null;
  async function boot() {
    if (browser) try { await browser.close(); } catch (e) {}
    browser = await puppeteer.launch({
      executablePath: CHROME,
      headless: 'new',
      args: ['--no-sandbox', '--disable-dev-shm-usage', '--enable-unsafe-swiftshader'],
    });
    page = await browser.newPage();
    page.on('pageerror', (e) => console.error('  [pageerror]', e.message));
    await page.goto(`http://localhost:${port}/tools/eval_page.html`, { waitUntil: 'load' });
    await page.waitForFunction('window.__harnessReady === true', { timeout: 10000 });
  }
  await boot();

  const results = [];
  let idx = 0;
  for (const f of files) {
    idx++;
    let res;
    try {
      res = await Promise.race([
        page.evaluate((file) => window.evalShader(file, {}), f),
        new Promise((_, rej) => setTimeout(() => rej(new Error('TIMEOUT 45s')), 45000)),
      ]);
    } catch (e) {
      res = { file: f, compileOk: false, linkOk: false, ok: false, errors: ['HARNESS: ' + e.message] };
      console.error(`  !! ${f}: ${e.message} — rebooting browser`);
      try { await boot(); } catch (e2) { console.error('  reboot failed:', e2.message); break; }
    }
    results.push(res);
    const s = res.ok ? (res.scores ? res.scores.overall.toFixed(1) : '?') : 'FAIL';
    const audio = res.scores ? res.scores.audio.toFixed(1) : '-';
    const st = res.scores && res.scores.audioStyles
      ? ` styles[${Object.values(res.scores.audioStyles).map((v) => v.toFixed(1)).join(',')}] min=${res.scores.audioMin.toFixed(1)} diff=${res.scores.styleDiff.toFixed(1)}${res.choppy ? ' CHOPPY' : ''}`
      : '';
    console.log(`[${idx}/${files.length}] ${res.ok ? 'ok ' : 'BAD'} ${f.padEnd(34)} overall=${s} audio=${audio}${st}${res.errors && res.errors.length ? ' | ' + res.errors[0].slice(0, 120) : ''}`);
  }

  fs.writeFileSync(outFile, JSON.stringify({ generated: new Date().toISOString(), count: results.length, results }, null, 1));
  const bad = results.filter((r) => !r.ok);
  console.log(`\n==== ${results.length} shaders, ${bad.length} failing ====`);
  for (const b of bad) console.log('  FAIL ' + b.file + ' :: ' + (b.errors || []).join(' | ').slice(0, 200));
  await browser.close();
  server.close();
}

main().catch((e) => { console.error(e); process.exit(1); });

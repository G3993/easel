#!/usr/bin/env node
// Relative render-cost benchmark: times a fixed evalShader workload per file
// (same frame counts for every shader) through the real compile path.
//   node tools/bench_shader.cjs --files a.fs,b.fs [--runs 2]
const http = require('http');
const fs = require('fs');
const path = require('path');
const puppeteer = require('puppeteer-core');

const ROOT = path.join(__dirname, '..');
const CHROME = '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome';
const args = process.argv.slice(2);
function argVal(f, d) { const i = args.indexOf(f); return i >= 0 ? args[i + 1] : d; }
const files = (argVal('--files', '') || '').split(',').map(s => s.trim()).filter(Boolean);
const runs = Number(argVal('--runs', 2));

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
  await page.goto(`http://localhost:${port}/tools/eval_page.html`, { waitUntil: 'load' });
  await page.waitForFunction('window.__harnessReady === true', { timeout: 15000 });

  const rows = [];
  for (const f of files) {
    let best = Infinity, ok = true;
    for (let i = 0; i < runs; i++) {
      const t0 = Date.now();
      try {
        const res = await Promise.race([
          page.evaluate((file) => window.evalShader(file, {}), f),
          new Promise((_, rej) => setTimeout(() => rej(new Error('TIMEOUT')), 120000)),
        ]);
        if (!res.ok) ok = false;
      } catch (e) { ok = false; }
      best = Math.min(best, Date.now() - t0);
    }
    rows.push({ file: f, ms: best, ok });
    console.log(f.padEnd(30), (best / 1000).toFixed(1) + 's', ok ? '' : 'NOT-OK');
  }
  rows.sort((a, b) => b.ms - a.ms);
  console.log('\nsorted by cost:');
  for (const r of rows) console.log(' ', r.file.padEnd(30), (r.ms / 1000).toFixed(1) + 's');
  await browser.close();
  server.close();
}
main().catch((e) => { console.error(e); process.exit(1); });

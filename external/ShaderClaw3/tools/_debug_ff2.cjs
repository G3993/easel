const http = require('http');
const fs = require('fs');
const path = require('path');
const puppeteer = require('puppeteer-core');
const ROOT = '/Users/lu/ShaderClaw3';
const CHROME = '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome';
const MIME = { '.html': 'text/html', '.js': 'application/javascript', '.json': 'application/json', '.css': 'text/css', '.fs': 'text/plain', '.vs': 'text/plain', '.png': 'image/png' };
const server = http.createServer((req, res) => {
  let urlPath = decodeURIComponent(req.url.split('?')[0]);
  const fp = path.join(ROOT, urlPath);
  fs.readFile(fp, (err, data) => {
    if (err) { res.writeHead(404); res.end('nf'); return; }
    res.writeHead(200, { 'Content-Type': MIME[path.extname(fp)] || 'application/octet-stream' });
    res.end(data);
  });
});
(async () => {
  await new Promise(r => server.listen(0, r));
  const port = server.address().port;
  const browser = await puppeteer.launch({ executablePath: CHROME, headless: 'new', args: ['--no-sandbox','--disable-dev-shm-usage','--enable-unsafe-swiftshader'] });
  const page = await browser.newPage();
  page.on('pageerror', e => console.error('pageerror', e.message));
  await page.goto(`http://localhost:${port}/tools/eval_page.html`, { waitUntil: 'load' });
  await page.waitForFunction('window.__harnessReady === true', { timeout: 10000 });
  const r = await page.evaluate((file) => window.evalShader(file, {}), 'fireflow.fs');
  console.log('audioResponseRaw', r.audioResponseRaw, 'movement', r.movement, 'scores', r.scores);
  await browser.close();
  server.close();
})();

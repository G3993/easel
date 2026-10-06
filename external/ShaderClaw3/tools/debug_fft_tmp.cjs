const http = require('http');
const fs = require('fs');
const path = require('path');
const puppeteer = require('puppeteer-core');
const ROOT = '/Users/lu/ShaderClaw3';
const CHROME = '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome';
const MIME = { '.html':'text/html','.js':'application/javascript','.json':'application/json','.css':'text/css','.fs':'text/plain','.vs':'text/plain','.png':'image/png' };
const server = http.createServer((req,res)=>{
  let urlPath = decodeURIComponent(req.url.split('?')[0]);
  const fp = path.join(ROOT, urlPath);
  fs.readFile(fp, (err,data)=>{
    if (err) { res.writeHead(404); res.end('nf'); return; }
    res.writeHead(200, {'Content-Type': MIME[path.extname(fp)]||'application/octet-stream'});
    res.end(data);
  });
});
(async () => {
  await new Promise(r=>server.listen(0,r));
  const port = server.address().port;
  const browser = await puppeteer.launch({executablePath: CHROME, headless:'new'});
  const page = await browser.newPage();
  page.on('console', m => console.log('PAGE:', m.text()));
  await page.goto(`http://localhost:${port}/tools/eval_page.html`);
  await page.waitForFunction('window.evalShader');
  for (const f of ['fractal_feedback_tunnel.fs']) {
    const result = await page.evaluate(async (f) => {
      const out = await window.evalShader(f, {});
      return { audioResponseRaw: out.audioResponseRaw, movement: out.movement, scores: out.scores, meanLuma: out.meanLuma, blackFrac: out.blackFrac };
    }, f);
    console.log(f, JSON.stringify(result));
  }
  await browser.close();
  server.close();
})();

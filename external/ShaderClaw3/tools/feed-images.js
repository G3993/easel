#!/usr/bin/env node
// ============================================================
// ShaderClaw — Image Feed Fetcher
// Downloads images for a search term into data/feeds/<name>/
// Usage: node tools/feed-images.js <feed-name> <search-term> [--count 20] [--interval 30]
//
// Sources (in priority order):
// 1. Unsplash API (free, high quality — set UNSPLASH_KEY in .env)
// 2. Pexels API (free — set PEXELS_KEY in .env)
// 3. Lorem Picsum (no key needed, random photos)
//
// For social media feeds (Instagram/X), use their respective APIs
// and pipe results into data/feeds/<name>/ as jpg/png files.
// ============================================================

import { mkdir, writeFile, readdir } from 'fs/promises';
import { join, dirname } from 'path';
import { fileURLToPath } from 'url';
import { existsSync } from 'fs';

const __dirname = dirname(fileURLToPath(import.meta.url));
const ROOT = join(__dirname, '..');

// Load .env
try {
  const envContent = await import('fs').then(fs => fs.readFileSync(join(ROOT, '.env'), 'utf-8'));
  for (const line of envContent.split('\n')) {
    const trimmed = line.trim();
    if (!trimmed || trimmed.startsWith('#')) continue;
    const eq = trimmed.indexOf('=');
    if (eq < 0) continue;
    const key = trimmed.slice(0, eq).trim();
    const val = trimmed.slice(eq + 1).trim().replace(/^["']|["']$/g, '');
    if (!process.env[key]) process.env[key] = val;
  }
} catch {}

const args = process.argv.slice(2);
if (args.length < 2) {
  console.log('Usage: node tools/feed-images.js <feed-name> <search-term> [--count 20] [--interval 30]');
  console.log('');
  console.log('Examples:');
  console.log('  node tools/feed-images.js sunsets "sunset sky"');
  console.log('  node tools/feed-images.js flowers "flower close up" --count 30');
  console.log('  node tools/feed-images.js cities "city skyline night" --interval 10');
  console.log('');
  console.log('Set UNSPLASH_KEY or PEXELS_KEY in .env for better results.');
  process.exit(1);
}

const feedName = args[0].replace(/[^a-zA-Z0-9_-]/g, '');
const searchTerm = args[1];
let count = 20;
let interval = 0; // 0 = one-shot, >0 = seconds between fetches

for (let i = 2; i < args.length; i++) {
  if (args[i] === '--count' && args[i + 1]) count = parseInt(args[i + 1]);
  if (args[i] === '--interval' && args[i + 1]) interval = parseInt(args[i + 1]);
}

const feedDir = join(ROOT, 'data', 'feeds', feedName);
await mkdir(feedDir, { recursive: true });

const UNSPLASH_KEY = process.env.UNSPLASH_KEY || process.env.UNSPLASH_ACCESS_KEY;
const PEXELS_KEY = process.env.PEXELS_KEY;

async function downloadImage(url, filepath) {
  const resp = await fetch(url);
  if (!resp.ok) throw new Error(`HTTP ${resp.status}: ${url}`);
  const buffer = Buffer.from(await resp.arrayBuffer());
  await writeFile(filepath, buffer);
}

async function fetchUnsplash(query, perPage) {
  const url = `https://api.unsplash.com/search/photos?query=${encodeURIComponent(query)}&per_page=${perPage}&client_id=${UNSPLASH_KEY}`;
  const resp = await fetch(url);
  const data = await resp.json();
  return (data.results || []).map(r => ({
    id: r.id,
    url: r.urls.regular, // 1080px wide
    thumb: r.urls.thumb,
    credit: r.user.name,
  }));
}

async function fetchPexels(query, perPage) {
  const url = `https://api.pexels.com/v1/search?query=${encodeURIComponent(query)}&per_page=${perPage}`;
  const resp = await fetch(url, { headers: { Authorization: PEXELS_KEY } });
  const data = await resp.json();
  return (data.photos || []).map(p => ({
    id: String(p.id),
    url: p.src.large, // 940px wide
    credit: p.photographer,
  }));
}

async function fetchPicsum(perPage) {
  const images = [];
  for (let i = 0; i < perPage; i++) {
    const id = Math.floor(Math.random() * 1000);
    images.push({
      id: `picsum_${id}_${Date.now()}_${i}`,
      url: `https://picsum.photos/id/${id}/800/600`,
      credit: 'Lorem Picsum',
    });
  }
  return images;
}

async function fetchBatch() {
  let images = [];
  const existing = await readdir(feedDir).catch(() => []);

  if (UNSPLASH_KEY) {
    console.log(`Fetching from Unsplash: "${searchTerm}" (${count} images)`);
    images = await fetchUnsplash(searchTerm, count);
  } else if (PEXELS_KEY) {
    console.log(`Fetching from Pexels: "${searchTerm}" (${count} images)`);
    images = await fetchPexels(searchTerm, count);
  } else {
    console.log(`No API keys — using Lorem Picsum (${count} random images)`);
    console.log('Set UNSPLASH_KEY or PEXELS_KEY in .env for search-based results.');
    images = await fetchPicsum(count);
  }

  let downloaded = 0;
  for (const img of images) {
    const filename = `${img.id}.jpg`;
    const filepath = join(feedDir, filename);
    if (existing.includes(filename)) {
      console.log(`  skip: ${filename} (exists)`);
      continue;
    }
    try {
      await downloadImage(img.url, filepath);
      downloaded++;
      console.log(`  saved: ${filename} (${img.credit})`);
    } catch (e) {
      console.warn(`  failed: ${filename} — ${e.message}`);
    }
  }
  console.log(`Downloaded ${downloaded} new images to data/feeds/${feedName}/`);
  console.log(`Total: ${existing.length + downloaded} images in feed`);
}

// Run
await fetchBatch();

if (interval > 0) {
  console.log(`\nPolling every ${interval}s for new images...`);
  setInterval(fetchBatch, interval * 1000);
} else {
  console.log('\nDone. Images are now available in ShaderClaw via the Image Feed data source.');
  console.log(`In ShaderClaw: Data Sources → Image Feed → feed name: "${feedName}"`);
}

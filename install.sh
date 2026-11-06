#!/bin/bash
# Easel installer (macOS, Apple Silicon)
#   curl -fsSL https://g3993.github.io/easel/install.sh | bash
#
# 1. Downloads the latest Easel Studio build and installs /Applications/Easel.app
#    (curl avoids the browser quarantine flag, so macOS won't call it "damaged").
# 2. Installs the ShaderClaw3 shader library + editor at ~/ShaderClaw3
#    (Easel uses it live when present; the app also carries a bundled copy).
#    Skip with:  curl -fsSL .../install.sh | bash -s -- --no-shaderclaw
set -e

REPO="G3993/easel"
SC_REPO="G3993/ShaderClaw3"
ZIP_URL="https://github.com/$REPO/releases/latest/download/Easel-mac-arm64.zip"
WITH_SC=1
for a in "$@"; do [ "$a" = "--no-shaderclaw" ] && WITH_SC=0; done

TMP_DIR="$(mktemp -d)"
trap 'rm -rf "$TMP_DIR"' EXIT

if [ "$(uname -s)" != "Darwin" ] || [ "$(uname -m)" != "arm64" ]; then
  echo "This installer is for Apple Silicon Macs. On Windows use install.ps1 (see the download page)." >&2
  exit 1
fi

MACOS_MAJOR="$(sw_vers -productVersion | cut -d. -f1)"
if [ "$MACOS_MAJOR" -lt 14 ]; then
  echo "Easel needs macOS 14 (Sonoma) or newer; this Mac runs $(sw_vers -productVersion)." >&2
  exit 1
fi

echo "==> Downloading Easel (latest)…"
curl -fL --progress-bar "$ZIP_URL" -o "$TMP_DIR/Easel.zip"

echo "==> Installing /Applications/Easel.app"
ditto -x -k "$TMP_DIR/Easel.zip" "$TMP_DIR/extract"
APP="$(find "$TMP_DIR/extract" -maxdepth 2 -name 'Easel.app' -type d | head -1)"
[ -n "$APP" ] || { echo "Easel.app not found in the download" >&2; exit 1; }
if [ -d /Applications/Easel.app ]; then
  osascript -e 'tell application "Easel" to quit' >/dev/null 2>&1 || true
  rm -rf /Applications/Easel.app
fi
mv "$APP" /Applications/
xattr -dr com.apple.quarantine /Applications/Easel.app 2>/dev/null || true

if [ "$WITH_SC" = 1 ]; then
  SC="$HOME/ShaderClaw3"
  echo "==> ShaderClaw3 shader library → $SC"
  if [ -d "$SC/.git" ] && command -v git >/dev/null 2>&1 && git -C "$SC" rev-parse >/dev/null 2>&1; then
    git -C "$SC" pull --ff-only -q || echo "   (left your existing ~/ShaderClaw3 as is — it has local changes)"
  elif [ -e "$SC" ]; then
    echo "   ~/ShaderClaw3 already exists (not a git checkout) — left untouched."
  elif xcode-select -p >/dev/null 2>&1 && command -v git >/dev/null 2>&1; then
    git clone -q --depth 1 "https://github.com/$SC_REPO.git" "$SC"
  else
    curl -fL --progress-bar "https://codeload.github.com/$SC_REPO/tar.gz/refs/heads/master" -o "$TMP_DIR/sc.tgz"
    mkdir -p "$SC" && tar -xzf "$TMP_DIR/sc.tgz" -C "$SC" --strip-components 1
  fi
fi

echo
echo "Done."
echo "  • Easel:  /Applications/Easel.app  (opening now)"
[ "$WITH_SC" = 1 ] && echo "  • Shaders: ~/ShaderClaw3/shaders  (Easel picks these up automatically)"
[ "$WITH_SC" = 1 ] && echo "  • ShaderClaw editor (optional, needs Node 18+):  cd ~/ShaderClaw3 && npm install && npm start  → http://localhost:7778"
echo "  • First launch: allow Screen & System Audio Recording, Microphone and Camera when macOS asks."
open /Applications/Easel.app

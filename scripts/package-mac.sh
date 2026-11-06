#!/bin/bash
# Package a distributable Easel.app (Apple Silicon) from a Release build.
#   scripts/package-mac.sh [build-dir] [out-dir]
# Defaults: build-studio  →  dist/
# Produces dist/Easel-mac-arm64.zip that runs on a Mac with no Homebrew:
#   brew dylibs copied into Contents/Frameworks (@rpath), absolute rpaths
#   removed, shader thumbnails bundled, an EMPTY default project (never the
#   dev machine's zones/NDI), everything ad-hoc signed.
set -euo pipefail
cd "$(dirname "$0")/.."
BUILD="${1:-build-studio}"
OUT="${2:-dist}"
SRC_APP="$BUILD/Easel.app"
[ -d "$SRC_APP" ] || { echo "no $SRC_APP — build it first" >&2; exit 1; }

STAGE="$(mktemp -d)"
trap 'rm -rf "$STAGE"' EXIT
APP="$STAGE/Easel.app"
ditto "$SRC_APP" "$APP"
EXE="$APP/Contents/MacOS/Easel"

echo "==> strip signatures"
codesign --remove-signature "$APP" 2>/dev/null || true
find "$APP/Contents" \( -name '*.dylib' -o -name '*.framework' \) -prune -print0 |
  xargs -0 -I{} codesign --remove-signature {} 2>/dev/null || true

echo "==> bundle dylibs"
python3 scripts/bundle-mac-dylibs.py "$APP"

echo "==> drop absolute rpaths"
for bin in "$EXE" "$APP"/Contents/Frameworks/*.dylib; do
  otool -l "$bin" | awk '/cmd LC_RPATH/{getline; getline; print $2}' | while read -r rp; do
    case "$rp" in /*) install_name_tool -delete_rpath "$rp" "$bin" 2>/dev/null || true ;; esac
  done
done

echo "==> thumbnails + clean default project"
RES="$APP/Contents/Resources"
if [ -d "$HOME/.easel/shader_thumbs" ] && [ -d "$RES/ShaderClaw3" ]; then
  mkdir -p "$RES/ShaderClaw3/thumbs"
  cp "$HOME/.easel/shader_thumbs/"*.png "$RES/ShaderClaw3/thumbs/" 2>/dev/null || true
fi
printf '{"layers":[]}' > "$RES/default.easel"

echo "==> ad-hoc sign"
find "$APP/Contents" -name '*.dylib' -not -path '*.framework/*' -print0 |
  xargs -0 -I{} codesign --force --sign - {} 2>/dev/null
for fw in "$APP"/Contents/Frameworks/*.framework; do
  [ -d "$fw" ] && codesign --force --sign - "$fw" 2>/dev/null
done
codesign --force --sign - "$APP"
codesign --verify --deep --strict "$APP"

echo "==> check: no Homebrew / absolute non-system deps"
BAD=$(for b in "$EXE" "$APP"/Contents/Frameworks/*.dylib; do otool -L "$b" | tail -n +2; done |
      awk '{print $1}' | grep -E '^/(opt|usr/local|Users)/' | sort -u || true)
[ -z "$BAD" ] || { echo "still linked outside the bundle:"; echo "$BAD"; exit 1; }

mkdir -p "$OUT"
rm -f "$OUT/Easel-mac-arm64.zip"
ditto -c -k --sequesterRsrc --keepParent "$APP" "$OUT/Easel-mac-arm64.zip"
ls -lh "$OUT/Easel-mac-arm64.zip"

#!/bin/bash
# Package a distributable Easel.app (Apple Silicon) from a Release build.
#   scripts/package-mac.sh [build-dir] [out-dir]
# Defaults: build-dist  →  dist/
# Produces dist/Easel-mac-arm64.zip that runs on a Mac with no Homebrew:
#   dylibs copied into Contents/Frameworks (@rpath), absolute rpaths
#   removed, shader thumbnails bundled, an EMPTY default project (never the
#   dev machine's zones/NDI), every binary checked against MIN_MACOS.
#
# Env:
#   MIN_MACOS=14.0         fail if any bundled binary needs a newer macOS
#   BUNDLE_LIB_DIRS=...    where to find dylibs (default external/mac-deps/lib)
#   SIGN_ID="Developer ID Application: NAME (TEAMID)"
#                          sign with hardened runtime instead of ad-hoc
#   NOTARY_PROFILE=easel   also notarize + staple (create once with
#                          xcrun notarytool store-credentials easel ...)
set -euo pipefail
cd "$(dirname "$0")/.."
BUILD="${1:-build-dist}"
MIN_MACOS="${MIN_MACOS:-14.0}"
export BUNDLE_LIB_DIRS="${BUNDLE_LIB_DIRS:-$PWD/external/mac-deps/lib}"
export BUNDLE_NO_HOMEBREW="${BUNDLE_NO_HOMEBREW:-1}"
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

echo "==> check: every binary runs on macOS $MIN_MACOS"
TOO_NEW=""
while IFS= read -r -d '' b; do
  file -b "$b" | grep -q Mach-O || continue
  v=$(otool -l "$b" | awk '/LC_BUILD_VERSION/{f=1} f&&/minos/{print $2; exit} /LC_VERSION_MIN_MACOSX/{g=1} g&&/version/{print $2; exit}')
  [ -z "$v" ] && continue
  if [ "$(printf '%s\n%s\n' "$MIN_MACOS" "$v" | sort -V | tail -1)" != "$MIN_MACOS" ]; then
    TOO_NEW="$TOO_NEW\n  $v  ${b#$APP/}"
  fi
done < <(find "$APP/Contents" -type f \( -perm -u+x -o -name '*.dylib' \) -print0)
[ -z "$TOO_NEW" ] || { printf "needs a newer macOS than $MIN_MACOS:$TOO_NEW\n"; exit 1; }

SIGN="${SIGN_ID:--}"
if [ "$SIGN" = "-" ]; then
  echo "==> ad-hoc sign (no SIGN_ID: users will see the unidentified-developer warning)"
  RT=()
else
  echo "==> sign with $SIGN (hardened runtime)"
  RT=(--options runtime --timestamp)
fi
find "$APP/Contents" -name '*.dylib' -not -path '*.framework/*' -print0 |
  xargs -0 -I{} codesign --force "${RT[@]}" --sign "$SIGN" {} 2>/dev/null
for fw in "$APP"/Contents/Frameworks/*.framework; do
  [ -d "$fw" ] && codesign --force "${RT[@]}" --sign "$SIGN" "$fw" 2>/dev/null
done
if [ "$SIGN" = "-" ]; then
  codesign --force --sign - "$APP"
else
  codesign --force "${RT[@]}" --entitlements packaging/mac/Easel.entitlements --sign "$SIGN" "$APP"
fi
codesign --verify --deep --strict "$APP"

echo "==> check: no Homebrew / absolute non-system deps"
BAD=$(for b in "$EXE" "$APP"/Contents/Frameworks/*.dylib; do otool -L "$b" | tail -n +2; done |
      awk '{print $1}' | grep -E '^/(opt|usr/local|Users)/' | sort -u || true)
[ -z "$BAD" ] || { echo "still linked outside the bundle:"; echo "$BAD"; exit 1; }

mkdir -p "$OUT"
rm -f "$OUT/Easel-mac-arm64.zip"
ditto -c -k --sequesterRsrc --keepParent "$APP" "$OUT/Easel-mac-arm64.zip"
if [ -n "${NOTARY_PROFILE:-}" ] && [ "$SIGN" != "-" ]; then
  echo "==> notarize"
  xcrun notarytool submit "$OUT/Easel-mac-arm64.zip" --keychain-profile "$NOTARY_PROFILE" --wait
  xcrun stapler staple "$APP"
  rm -f "$OUT/Easel-mac-arm64.zip"
  ditto -c -k --sequesterRsrc --keepParent "$APP" "$OUT/Easel-mac-arm64.zip"
  spctl --assess --type execute -vv "$APP"
fi
ls -lh "$OUT/Easel-mac-arm64.zip"

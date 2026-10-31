#!/bin/bash
# Fetch Google's libwebrtc for the macOS WHEP source (stasel/WebRTC builds,
# the same engine Chrome uses). CMake picks it up from external/webrtc.
set -e
VER="${1:-155.0.0}"
M="${VER%%.*}"
DIR="$(cd "$(dirname "$0")/.." && pwd)/external/webrtc"
mkdir -p "$DIR"
cd "$DIR"
curl -fL -o "WebRTC-M$M.xcframework.zip" "https://github.com/stasel/WebRTC/releases/download/$VER/WebRTC-M$M.xcframework.zip"
rm -rf WebRTC.xcframework
unzip -q "WebRTC-M$M.xcframework.zip"
echo "WebRTC M$M -> $DIR/WebRTC.xcframework"

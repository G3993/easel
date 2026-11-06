#!/bin/bash
# Build the FFmpeg + OpenCV that ship inside the distributable Mac app,
# targeting macOS 14 (Homebrew bottles only target the newest macOS, so
# bundling Homebrew's copies made the app require macOS 26).
#
#   scripts/build-mac-deps.sh            # → external/mac-deps (≈15 min)
#
# FFmpeg: LGPL, decoders + VideoToolbox/AudioToolbox (H.264/AAC encode for
#         recording and RTMP), no external libraries.
# OpenCV: core/imgproc/calib3d/features2d/flann/imgcodecs/videoio (AVFoundation
#         camera backend), bundled libjpeg/libpng/zlib, nothing from Homebrew.
# Then build Easel against them:
#   cmake -S . -B build-dist -DCMAKE_BUILD_TYPE=Release -DEASEL_FEATURED_PACK=studio \
#         -DEASEL_FFMPEG_DIR=$PWD/external/mac-deps -DOpenCV_DIR=$PWD/external/mac-deps/lib/cmake/opencv4
#   cmake --build build-dist --target Easel -j && scripts/package-mac.sh build-dist
set -euo pipefail
cd "$(dirname "$0")/.."
FFMPEG_VER=8.1.1
OPENCV_VER=4.13.0
MIN=14.0
P="$PWD/external/mac-deps"
WORK="${WORK:-$(mktemp -d)}"
JOBS="$(sysctl -n hw.ncpu)"
export MACOSX_DEPLOYMENT_TARGET=$MIN PKG_CONFIG_PATH= PKG_CONFIG_LIBDIR=/nonexistent

cd "$WORK"
[ -f ffmpeg-$FFMPEG_VER.tar.xz ] || curl -fsSLO https://ffmpeg.org/releases/ffmpeg-$FFMPEG_VER.tar.xz
[ -f opencv-$OPENCV_VER.tar.gz ] || curl -fsSL -o opencv-$OPENCV_VER.tar.gz https://github.com/opencv/opencv/archive/refs/tags/$OPENCV_VER.tar.gz
tar xf ffmpeg-$FFMPEG_VER.tar.xz
tar xzf opencv-$OPENCV_VER.tar.gz

echo "==> FFmpeg $FFMPEG_VER"
(cd ffmpeg-$FFMPEG_VER &&
 ./configure --prefix="$P" --cc=clang --enable-shared --disable-static \
   --disable-programs --disable-doc --disable-debug --disable-autodetect \
   --disable-avdevice --disable-avfilter \
   --enable-videotoolbox --enable-audiotoolbox --enable-zlib --enable-bzlib --disable-iconv \
   --install-name-dir='@rpath' \
   --extra-cflags="-mmacosx-version-min=$MIN" --extra-ldflags="-mmacosx-version-min=$MIN" &&
 make -j"$JOBS" && make install)

echo "==> OpenCV $OPENCV_VER"
cmake -S opencv-$OPENCV_VER -B opencv-build -DCMAKE_BUILD_TYPE=Release \
  -DCMAKE_INSTALL_PREFIX="$P" -DCMAKE_OSX_DEPLOYMENT_TARGET=$MIN -DCMAKE_OSX_ARCHITECTURES=arm64 \
  -DCMAKE_INSTALL_NAME_DIR=@rpath \
  -DCMAKE_IGNORE_PREFIX_PATH=/opt/homebrew -DCMAKE_IGNORE_PATH="/opt/homebrew/lib;/opt/homebrew/include" \
  -DBUILD_LIST=core,imgproc,calib3d,features2d,flann,imgcodecs,videoio -DBUILD_SHARED_LIBS=ON \
  -DBUILD_TESTS=OFF -DBUILD_PERF_TESTS=OFF -DBUILD_EXAMPLES=OFF -DBUILD_opencv_apps=OFF \
  -DBUILD_JAVA=OFF -DBUILD_opencv_python3=OFF -DBUILD_DOCS=OFF \
  -DWITH_FFMPEG=OFF -DWITH_GSTREAMER=OFF -DWITH_AVFOUNDATION=ON \
  -DWITH_OPENEXR=OFF -DWITH_TIFF=OFF -DWITH_WEBP=OFF -DWITH_OPENJPEG=OFF -DWITH_JASPER=OFF \
  -DWITH_AVIF=OFF -DWITH_JPEGXL=OFF -DWITH_IMGCODEC_HDR=OFF -DWITH_IMGCODEC_SUNRASTER=OFF \
  -DWITH_IMGCODEC_PXM=OFF -DWITH_IMGCODEC_PFM=OFF -DWITH_SPNG=OFF \
  -DWITH_PNG=ON -DBUILD_PNG=ON -DWITH_JPEG=ON -DBUILD_JPEG=ON -DBUILD_ZLIB=ON \
  -DWITH_TBB=OFF -DWITH_OPENMP=OFF -DWITH_OPENCL=OFF -DWITH_LAPACK=OFF -DWITH_EIGEN=OFF \
  -DWITH_PROTOBUF=OFF -DWITH_ITT=OFF -DWITH_IPP=OFF -DWITH_QT=OFF -DWITH_VTK=OFF \
  -DWITH_1394=OFF -DWITH_OBSENSOR=OFF -DOPENCV_GENERATE_PKGCONFIG=OFF
cmake --build opencv-build -j"$JOBS"
cmake --install opencv-build

echo "==> done: $P"
ls "$P/lib"

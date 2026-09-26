EASEL — WINDOWS (PORTABLE)
==========================
Real-time visual instrument: shader library, audio reactivity, projection
mapping, screen/window capture, NDI, MIDI. This folder is self-contained.

REQUIREMENTS
- Windows 10/11, 64-bit, a GPU with OpenGL 4.1+ drivers (any modern
  NVIDIA / AMD / Intel with the vendor driver installed).
- No internet needed. No installer. The Visual C++ runtime DLLs are
  included next to Easel.exe (vc_redist.x64.exe is there too, only if
  Windows complains about a missing VCRUNTIME/MSVCP dll).

FIRST RUN
1. Unzip the WHOLE folder anywhere (desktop, USB drive, D:\Easel ...).
   Keep the folder intact: shaders\, assets\, ShaderClaw3\, default.easel
   and the .dll files must stay next to Easel.exe.
2. Double-click Easel.exe. If SmartScreen warns, click "More info" →
   "Run anyway" (the build is unsigned).
3. The ShaderClaw3 shader library (ShaderClaw3\shaders, 250+ shaders) is
   auto-connected on launch. To use a live library instead, put one at
   %USERPROFILE%\ShaderClaw3\shaders — the bundled copy still wins on
   Windows; delete/rename ShaderClaw3\ in this folder to prefer yours.

AUDIO
- System audio reactivity uses WASAPI loopback of the DEFAULT output
  device; microphone input via the Audio tab.
- MIDI controllers (Launch Control XL etc.) are detected on launch and on
  hot-plug.

NDI (optional)
- NDI send/receive needs the NDI Runtime (Processing.NDI.Lib.x64.dll).
  Either install "NDI Runtime" / NDI Tools from ndi.video BEFORE going
  offline, or drop Processing.NDI.Lib.x64.dll next to Easel.exe. Without
  it Easel runs normally with NDI disabled.

NOT IN THIS BUILD
- Camera/projector scanning (needs OpenCV) and Spout output are compiled
  out of the CI build. WHEP/WebRTC input is macOS-only.

FILES
- default.easel    startup project (edit or replace with your own)
- shaders\         Easel's internal GLSL (compositing, warps, transitions)
- ShaderClaw3\     bundled shader library + manifest.json
- assets\          icons, fonts, models, GL transitions

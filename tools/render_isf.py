#!/usr/bin/env python3
"""
render_isf.py — minimum-viable offline ISF renderer for Easel auto-improve loop.

Renders an ISF .fs shader to a 2x2 contact-sheet PNG at multiple TIME values.
Mirrors src/sources/ShaderSource.cpp::translateFragment preamble so what you
see here matches what Easel renders (modulo audio/cue live data and IMG_*
texture content — those are stubbed).

Deps (install into /Users/lu/easel/.venv-eval):
    moderngl>=5.12, Pillow>=11, numpy>=2

CLI:
    render_isf.py <shader.fs> --out <out.png>
        [--res 1280x720] [--times 0,1.5,3.5,7] [--msg "..."]
        [--font-atlas <path>] [--single]

Honest caveats:
- No real audio FFT — `audioLevel/Bass/Mid/High` driven by a synthetic
  envelope per-frame, `audioFFT` is a 256x1 ramp texture.
- IMG_* image inputs are bound to a 1x1 black texture; IMG_SIZE_<name> = vec2(0)
  so shaders that gate on it fall back gracefully.
- Font atlas: procedural 37-cell ramp atlas (A-Z, space, 0-9) so font shaders
  don't go blank. Visually approximate, not the same as Easel's stb_truetype
  atlas — letters won't look identical but composition reads correctly.
- player[i].*/cue.*/data.* binds: the orchestrator script drives these by
  setting per-frame default values (energyA/B/C, bassPull, etc.) through the
  same uniform that BIND would target in Easel. See `inject_default_values`.
"""

from __future__ import annotations
import argparse
import json
import math
import os
import re
import sys
from pathlib import Path
from typing import Any, Optional

import moderngl
import numpy as np
from PIL import Image, ImageDraw, ImageFont

# ─── ISF parsing ──────────────────────────────────────────────────────────

def parse_isf(source: str) -> tuple[str, dict, str]:
    """Returns (body_without_header, isf_dict, original_header_str)."""
    start = source.find("/*")
    if start < 0:
        return source, {}, ""
    end = source.find("*/", start)
    if end < 0:
        return source, {}, ""
    json_start = source.find("{", start, end)
    if json_start < 0:
        return source[end+2:], {}, source[start:end+2]
    # Walk brace depth to find matching close brace within comment
    depth = 0
    json_end = -1
    for i in range(json_start, end):
        c = source[i]
        if c == "{":
            depth += 1
        elif c == "}":
            depth -= 1
            if depth == 0:
                json_end = i
                break
    if json_end < 0:
        return source[end+2:], {}, source[start:end+2]
    try:
        isf = json.loads(source[json_start:json_end+1])
    except Exception as e:
        print(f"[warn] ISF JSON parse failed: {e}", file=sys.stderr)
        isf = {}
    return source[end+2:], isf, source[start:end+2]


# ─── Preamble (mirrors ShaderSource::translateFragment) ───────────────────

def build_preamble(isf_inputs: list, pass_buffers: list, body: str) -> str:
    """Generate the Easel-compatible GL3.3 preamble. Mirrors ShaderSource.cpp."""
    lines = []
    lines.append("#version 330 core")
    lines.append("out vec4 FragColor;")
    lines.append("in vec2 isf_FragNormCoord;")
    lines.append("")
    # ISF builtins
    lines.append("uniform float TIME;")
    lines.append("uniform float TIMEDELTA;")
    lines.append("uniform vec2  RENDERSIZE;")
    lines.append("uniform int   PASSINDEX;")
    lines.append("uniform int   FRAMEINDEX;")
    lines.append("uniform vec2  mousePos;")
    lines.append("uniform vec2  mouseDelta;")
    lines.append("uniform float pinchHold;")
    lines.append("uniform float msgAge;")
    lines.append("uniform float mouseDown;")
    lines.append("")
    # MediaPipe stubs
    lines.append("uniform sampler2D mpPoseLandmarks;")
    lines.append("uniform sampler2D mpFaceLandmarks;")
    lines.append("uniform sampler2D mpHandLandmarks;")
    lines.append("uniform sampler2D mpSegmentation;")
    lines.append("")
    # Image inputs
    image_inputs = [i for i in isf_inputs if i.get("TYPE") == "image"]
    for inp in image_inputs:
        n = inp["NAME"]
        lines.append(f"uniform sampler2D {n};")
        lines.append(f"uniform vec2 IMG_SIZE_{n};")
        lines.append(f"uniform bool _flip_{n};")
    for buf in pass_buffers:
        lines.append(f"uniform sampler2D {buf};")
    # font atlas (if referenced)
    if "fontAtlasTex" in body:
        lines.append("uniform sampler2D fontAtlasTex;")
    # audio FFT + reactivity
    lines.append("uniform sampler2D audioFFT;")
    lines.append("uniform float audioLevel;")
    lines.append("uniform float audioBass;")
    lines.append("uniform float audioMid;")
    lines.append("uniform float audioHigh;")
    if "_voiceGlitch" in body:
        lines.append("uniform float _voiceGlitch;")
    lines.append("uniform float _voiceLevel;")
    lines.append("")
    # IMG_* macros
    lines.append("#define IMG_NORM_PIXEL(img, coord) texture(img, coord)")
    lines.append("#define IMG_THIS_NORM_PIXEL(img) texture(img, isf_FragNormCoord)")
    lines.append("#define IMG_THIS_PIXEL(img) texture(img, gl_FragCoord.xy / RENDERSIZE)")
    for inp in image_inputs:
        n = inp["NAME"]
        lines.append(f"vec2 _isf_img_size_{n}() {{ return IMG_SIZE_{n}; }}")
        lines.append(f"vec4 _isf_img_pixel_{n}(vec2 coord) {{ return texture({n}, coord / max(IMG_SIZE_{n}, vec2(1.0))); }}")
        lines.append(f"vec4 _flippedTex_{n}(vec2 uv) {{ if (_flip_{n}) uv.y = 1.0 - uv.y; return texture({n}, uv); }}")
        lines.append(f"vec4 _flippedTexPixel_{n}(vec2 px) {{ return _flippedTex_{n}(px / max(IMG_SIZE_{n}, vec2(1.0))); }}")
        lines.append(f"vec4 _flippedTexThis_{n}() {{ return _flippedTex_{n}(gl_FragCoord.xy / RENDERSIZE); }}")
        lines.append(f"vec4 _flippedTexThisNorm_{n}() {{ return _flippedTex_{n}(isf_FragNormCoord); }}")
    lines.append("#define IMG_SIZE(img) RENDERSIZE")
    lines.append("#define IMG_PIXEL(img, coord) texture(img, (coord) / RENDERSIZE)")
    lines.append("")
    # User INPUTS
    for inp in isf_inputs:
        t = inp.get("TYPE", "float")
        n = inp["NAME"]
        if t == "float":
            lines.append(f"uniform float {n};")
        elif t == "color":
            lines.append(f"uniform vec4 {n};")
        elif t in ("bool", "event"):
            lines.append(f"uniform bool {n};")
        elif t == "point2D":
            lines.append(f"uniform vec2 {n};")
        elif t == "long":
            lines.append(f"uniform int {n};")
        elif t == "text":
            max_len = inp.get("MAX_LENGTH", 12)
            if max_len <= 0:
                max_len = 12
            lines.append(f"uniform int {n}_len;")
            for i in range(max_len):
                lines.append(f"uniform int {n}_{i};")
    lines.append("")
    lines.append("#define gl_FragColor FragColor")
    lines.append("#define texture2D texture")
    lines.append("")
    return "\n".join(lines) + "\n"


def translate_body(body: str, image_inputs: list) -> str:
    """Apply the same body-rewrites ShaderSource does."""
    # Strip #version
    body = re.sub(r"#version\s+\d+[^\n]*", "// (version stripped)", body)
    # Strip precision qualifiers
    body = re.sub(r"precision\s+(highp|mediump|lowp)\s+\w+\s*;", "// (precision stripped)", body)
    # varying -> in (fragment side)
    body = re.sub(r"\bvarying\b", "in", body)
    # texture2D handled by #define
    # Per-input flipped texture wrappers
    for inp in image_inputs:
        n = inp["NAME"]
        body = re.sub(rf"\btexture\s*\(\s*{n}\s*,", f"_flippedTex_{n}(", body)
        body = re.sub(rf"\bIMG_NORM_PIXEL\s*\(\s*{n}\s*,", f"_flippedTex_{n}(", body)
        body = re.sub(rf"\bIMG_PIXEL\s*\(\s*{n}\s*,", f"_flippedTexPixel_{n}(", body)
        body = re.sub(rf"\bIMG_THIS_PIXEL\s*\(\s*{n}\s*\)", f"_flippedTexThis_{n}()", body)
        body = re.sub(rf"\bIMG_THIS_NORM_PIXEL\s*\(\s*{n}\s*\)", f"_flippedTexThisNorm_{n}()", body)
    return body


# ─── Font atlas ───────────────────────────────────────────────────────────

ATLAS_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ 0123456789"  # 37 cells


def build_font_atlas(cell_w: int = 64, cell_h: int = 96) -> np.ndarray:
    """Procedural 37-cell font atlas. Returns HxW uint8, R channel.
    Layout matches Easel's FontAtlas.cpp: 37 horizontal cells, single row."""
    atlas_w = cell_w * len(ATLAS_CHARS)
    atlas_h = cell_h
    img = Image.new("L", (atlas_w, atlas_h), 0)
    draw = ImageDraw.Draw(img)
    # Pick a font
    font_path = None
    for p in [
        "/System/Library/Fonts/Supplemental/Arial Bold.ttf",
        "/System/Library/Fonts/Supplemental/Arial.ttf",
        "/System/Library/Fonts/Helvetica.ttc",
    ]:
        if os.path.exists(p):
            font_path = p
            break
    pixel_h = int(cell_h * 0.78)
    try:
        font = ImageFont.truetype(font_path, pixel_h) if font_path else ImageFont.load_default()
    except Exception:
        font = ImageFont.load_default()
    for i, ch in enumerate(ATLAS_CHARS):
        if ch == " ":
            continue
        # Center horizontally
        try:
            bbox = draw.textbbox((0, 0), ch, font=font)
            w = bbox[2] - bbox[0]
            x = i * cell_w + (cell_w - w) // 2 - bbox[0]
            y = int(cell_h * 0.10) - bbox[1]
        except Exception:
            x = i * cell_w + cell_w // 4
            y = cell_h // 8
        draw.text((x, y), ch, fill=255, font=font)
    # Easel's FontAtlas writes pixels with dstRow = cellH-1-baselineY-yoff-y, then
    # uploads directly. Effect: in the uploaded texture, baseline area sits at HIGH
    # row indices, which (since moderngl/glTexImage2D treats row 0 as v=0 bottom)
    # places baseline at HIGH v. Shaders sample with v∈[0,1] expecting top of letter
    # at v=1. To match: flipud our PIL render so PIL's "top of letter" ends up at
    # the high-row end of the byte buffer.
    arr = np.array(img, dtype=np.uint8)
    arr = np.flipud(arr).copy()
    return arr


# ─── Default-value bake (handles BIND defaults) ──────────────────────────

# Sentinel BIND prefixes we want to "wake up" so the shader doesn't
# look completely dead. The orchestrator can override per-frame; for
# v1 we drive them with a synthetic envelope.
PLAYER_BIND_RE = re.compile(r"player\[(\d+)\]\.(\w+)")


def default_for_input(inp: dict, time_value: float) -> Any:
    """Pick a value for an ISF input.
    - For player[i].energy/active/etc binds: drive a synthetic per-channel envelope so
      shaders that gate on energy actually show something.
    - For audio.* binds: synthetic audio envelope.
    - Otherwise: use DEFAULT.
    """
    bind = inp.get("BIND", "")
    t = inp.get("TYPE", "float")
    name = inp["NAME"]
    # audio.* drives
    if bind == "audio.bass":
        return 0.45 + 0.4 * (0.5 + 0.5 * math.sin(time_value * 1.7))
    if bind == "audio.mid":
        return 0.35 + 0.3 * (0.5 + 0.5 * math.sin(time_value * 2.3 + 1.0))
    if bind == "audio.high":
        return 0.25 + 0.3 * (0.5 + 0.5 * math.sin(time_value * 3.1 + 2.0))
    if bind == "audio.level":
        return 0.35 + 0.4 * (0.5 + 0.5 * math.sin(time_value * 1.1))
    if bind == "transport.beat":
        return (time_value * 2.0) % 1.0
    m = PLAYER_BIND_RE.match(bind)
    if m:
        idx = int(m.group(1))
        prop = m.group(2)
        if prop in ("energy", "level", "amp"):
            # Stagger players so different ones light up at different TIMEs.
            phase = idx * 1.3
            return 0.15 + 0.7 * max(0.0, math.sin(time_value * 0.9 + phase))
        if prop in ("active", "on", "voicing"):
            return 1.0 if math.sin(time_value * 0.6 + idx * 1.1) > -0.3 else 0.0
        return 0.5
    if bind.startswith("cue.") or bind.startswith("data."):
        # text bindings already handled via msg uniforms
        return None
    # Use ISF DEFAULT.
    if t == "float":
        d = inp.get("DEFAULT", 0.5)
        return float(d) if isinstance(d, (int, float, str)) else 0.5
    if t == "long":
        d = inp.get("DEFAULT", 0)
        return int(d) if isinstance(d, (int, float, str)) else 0
    if t in ("bool", "event"):
        d = inp.get("DEFAULT", False)
        if isinstance(d, bool):
            return d
        if isinstance(d, (int, float)):
            return bool(d)
        return False
    if t == "color":
        d = inp.get("DEFAULT", [1.0, 1.0, 1.0, 1.0])
        if not isinstance(d, list):
            d = [1.0, 1.0, 1.0, 1.0]
        while len(d) < 4:
            d.append(1.0)
        return tuple(float(x) for x in d[:4])
    if t == "point2D":
        d = inp.get("DEFAULT", [0.5, 0.5])
        if not isinstance(d, list):
            d = [0.5, 0.5]
        return (float(d[0]), float(d[1]))
    if t == "text":
        d = inp.get("DEFAULT", "")
        return str(d)
    return None


# ─── Text-input encoding (mirrors ShaderSource setText) ───────────────────

def encode_text(text: str, max_len: int) -> tuple[int, list[int]]:
    if max_len <= 0:
        max_len = 12
    if len(text) > max_len:
        text = text[-max_len:]
    codes = []
    for i in range(max_len):
        ch = 26  # space
        if i < len(text):
            c = text[i]
            if "A" <= c <= "Z":
                ch = ord(c) - ord("A")
            elif "a" <= c <= "z":
                ch = ord(c) - ord("a")
            elif "0" <= c <= "9":
                ch = 27 + (ord(c) - ord("0"))
            else:
                ch = 26
        codes.append(ch)
    return len(text), codes


# ─── Main render ──────────────────────────────────────────────────────────

VERTEX_SRC = """
#version 330 core
layout(location=0) in vec2 aPos;
layout(location=1) in vec2 aTexCoord;
out vec2 isf_FragNormCoord;
void main() {
    isf_FragNormCoord = aTexCoord;
    gl_Position = vec4(aPos, 0.0, 1.0);
}
"""


def safe_set(prog, name, value):
    """Set a uniform if present in the linked program. moderngl raises on
    unknown uniforms via prog[name]; this guards."""
    try:
        prog[name].value = value
    except KeyError:
        pass
    except Exception as e:
        # type mismatches etc — non-fatal
        pass


def render_frame(ctx, prog, fbo, time_value: float, frame_index: int,
                 res: tuple[int, int], inputs: list, msg: str,
                 fft_tex, font_tex, img_tex, vao):
    fbo.use()
    ctx.clear(0.0, 0.0, 0.0, 1.0)
    # Builtins
    safe_set(prog, "TIME", float(time_value))
    safe_set(prog, "TIMEDELTA", 1.0 / 60.0)
    safe_set(prog, "RENDERSIZE", (float(res[0]), float(res[1])))
    safe_set(prog, "PASSINDEX", 0)
    safe_set(prog, "FRAMEINDEX", frame_index)
    safe_set(prog, "mousePos", (res[0] * 0.5, res[1] * 0.5))
    safe_set(prog, "mouseDelta", (0.0, 0.0))
    safe_set(prog, "pinchHold", 0.0)
    safe_set(prog, "mouseDown", 0.0)
    # msgAge: simulate "live, just spoken" — text already typed at time=0
    safe_set(prog, "msgAge", max(0.0, time_value))
    # Synthetic audio
    safe_set(prog, "audioLevel", 0.4 + 0.3 * math.sin(time_value * 1.2))
    safe_set(prog, "audioBass", 0.5 + 0.35 * math.sin(time_value * 0.9))
    safe_set(prog, "audioMid", 0.35 + 0.3 * math.sin(time_value * 2.0 + 1.0))
    safe_set(prog, "audioHigh", 0.25 + 0.25 * math.sin(time_value * 3.3 + 2.0))
    safe_set(prog, "_voiceLevel", 0.3)
    safe_set(prog, "_voiceGlitch", 0.0)
    # Bind textures
    fft_tex.use(location=2)
    safe_set(prog, "audioFFT", 2)
    if font_tex is not None:
        font_tex.use(location=3)
        safe_set(prog, "fontAtlasTex", 3)
    # MediaPipe stubs → unit 7 zero texture
    img_tex.use(location=7)
    for name in ("mpPoseLandmarks", "mpFaceLandmarks", "mpHandLandmarks", "mpSegmentation"):
        safe_set(prog, name, 7)

    # ISF user inputs
    text_inputs = []
    img_unit = 8
    for inp in inputs:
        t = inp.get("TYPE", "float")
        n = inp["NAME"]
        if t == "text":
            text_inputs.append(inp)
            continue
        if t == "image":
            img_tex.use(location=img_unit)
            safe_set(prog, n, img_unit)
            safe_set(prog, f"IMG_SIZE_{n}", (0.0, 0.0))
            safe_set(prog, f"_flip_{n}", False)
            img_unit += 1
            continue
        val = default_for_input(inp, time_value)
        if val is None:
            continue
        if t in ("float",):
            safe_set(prog, n, float(val))
        elif t == "long":
            safe_set(prog, n, int(val))
        elif t in ("bool", "event"):
            safe_set(prog, n, bool(val))
        elif t == "color":
            safe_set(prog, n, tuple(val))
        elif t == "point2D":
            safe_set(prog, n, tuple(val))

    # Text inputs — use msg for any text input
    for inp in text_inputs:
        n = inp["NAME"]
        max_len = inp.get("MAX_LENGTH", 12)
        if max_len <= 0:
            max_len = 12
        length, codes = encode_text(msg.upper(), max_len)
        safe_set(prog, f"{n}_len", length)
        for i, c in enumerate(codes):
            safe_set(prog, f"{n}_{i}", int(c))

    # Draw
    vao.render(moderngl.TRIANGLE_STRIP, vertices=4)

    # Read back
    data = fbo.read(components=3, alignment=1)
    arr = np.frombuffer(data, dtype=np.uint8).reshape(res[1], res[0], 3)
    # FBO is bottom-up
    arr = np.flipud(arr).copy()
    return arr


def assemble_contact_sheet(frames: list[tuple[float, np.ndarray]],
                           single: bool = False) -> Image.Image:
    if single or len(frames) == 1:
        t, a = frames[0]
        img = Image.fromarray(a)
        draw = ImageDraw.Draw(img)
        _label(draw, f"t={t:.2f}s", (12, 12))
        return img
    h, w = frames[0][1].shape[:2]
    n = len(frames)
    cols = 2
    rows = (n + cols - 1) // cols
    sheet = Image.new("RGB", (w * cols, h * rows), (0, 0, 0))
    for i, (t, a) in enumerate(frames):
        sub = Image.fromarray(a)
        x = (i % cols) * w
        y = (i // cols) * h
        sheet.paste(sub, (x, y))
        draw = ImageDraw.Draw(sheet)
        _label(draw, f"t={t:.2f}s", (x + 12, y + 12))
    return sheet


def _label(draw, text, xy):
    try:
        font = ImageFont.truetype("/System/Library/Fonts/Supplemental/Arial Bold.ttf", 20)
    except Exception:
        font = ImageFont.load_default()
    # Outline
    x, y = xy
    for dx, dy in [(-1,0),(1,0),(0,-1),(0,1)]:
        draw.text((x+dx, y+dy), text, fill=(0,0,0), font=font)
    draw.text(xy, text, fill=(255,255,255), font=font)


def parse_res(s: str) -> tuple[int, int]:
    a, b = s.lower().split("x")
    return int(a), int(b)


def parse_times(s: str) -> list[float]:
    return [float(x) for x in s.split(",")]


def main(argv=None):
    ap = argparse.ArgumentParser()
    ap.add_argument("shader")
    ap.add_argument("--out", required=True)
    ap.add_argument("--res", default="1280x720")
    ap.add_argument("--times", default="0,1.5,3.5,7")
    ap.add_argument("--msg", default="HELLO WORLD CONVERSATION VISUAL TEST")
    ap.add_argument("--font-atlas", default="")
    ap.add_argument("--single", action="store_true",
                    help="Single-frame mode (only first TIME).")
    args = ap.parse_args(argv)

    shader_path = Path(args.shader)
    if not shader_path.exists():
        print(f"error: shader not found: {shader_path}", file=sys.stderr)
        return 2
    src = shader_path.read_text()
    body, isf, _header = parse_isf(src)
    inputs = isf.get("INPUTS", []) if isinstance(isf, dict) else []
    pass_buffers = []
    if isinstance(isf, dict) and isinstance(isf.get("PASSES"), list):
        for p in isf["PASSES"]:
            if isinstance(p, dict) and p.get("TARGET"):
                pass_buffers.append(p["TARGET"])

    preamble = build_preamble(inputs, pass_buffers, body)
    body = translate_body(body, [i for i in inputs if i.get("TYPE") == "image"])
    frag_src = preamble + body

    res = parse_res(args.res)
    times = parse_times(args.times)
    if args.single:
        times = times[:1]

    ctx = moderngl.create_standalone_context(require=330)

    # Build program
    log_path = Path(args.out).with_suffix(".log")
    try:
        prog = ctx.program(vertex_shader=VERTEX_SRC, fragment_shader=frag_src)
    except moderngl.Error as e:
        # Write the full source + error log
        log_path.write_text(f"=== compile/link error ===\n{e}\n\n=== fragment source ===\n{frag_src}\n")
        print(f"error: GLSL compile/link failed; see {log_path}", file=sys.stderr)
        return 3

    # Fullscreen triangle strip (-1..1 quad)
    verts = np.array([
        # x,    y,    u,    v
        -1.0, -1.0, 0.0, 0.0,
         1.0, -1.0, 1.0, 0.0,
        -1.0,  1.0, 0.0, 1.0,
         1.0,  1.0, 1.0, 1.0,
    ], dtype="f4")
    vbo = ctx.buffer(verts.tobytes())
    vao = ctx.vertex_array(prog, [(vbo, "2f 2f", "aPos", "aTexCoord")])

    # FBO
    tex = ctx.texture(res, 3)
    fbo = ctx.framebuffer(color_attachments=[tex])

    # Stub textures
    # audio FFT: 256x1 ramp
    fft_data = (np.linspace(0.2, 0.8, 256, dtype="f4") *
                (0.5 + 0.5*np.sin(np.linspace(0, 6*np.pi, 256)))).astype("f4")
    fft_tex = ctx.texture((256, 1), 1, fft_data.tobytes(), dtype="f4")
    fft_tex.repeat_x = False
    fft_tex.repeat_y = False
    # font atlas
    if args.font_atlas and os.path.exists(args.font_atlas):
        atlas_img = Image.open(args.font_atlas).convert("L")
        atlas_arr = np.array(atlas_img, dtype=np.uint8)
        atlas_arr = np.flipud(atlas_arr).copy()
    else:
        atlas_arr = build_font_atlas()
    font_tex = ctx.texture((atlas_arr.shape[1], atlas_arr.shape[0]), 1,
                           atlas_arr.tobytes())
    font_tex.repeat_x = False
    font_tex.repeat_y = False
    font_tex.filter = (moderngl.LINEAR, moderngl.LINEAR)
    # Make R-only texture appear as luminance in .r .g .b
    font_tex.swizzle = "RRRR"
    # 1x1 black image stub for IMG inputs and MediaPipe
    img_tex = ctx.texture((1, 1), 4, b"\x00\x00\x00\x00")

    frames = []
    for i, t in enumerate(times):
        arr = render_frame(ctx, prog, fbo, t, i, res, inputs, args.msg,
                           fft_tex, font_tex, img_tex, vao)
        frames.append((t, arr))

    sheet = assemble_contact_sheet(frames, single=args.single)
    out_path = Path(args.out)
    out_path.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(out_path)
    # Also save the assembled fragment source for debugging
    log_path.write_text(f"=== render ok ===\nshader={shader_path}\nout={out_path}\n"
                        f"res={res} times={times}\n"
                        f"=== fragment source ===\n{frag_src}\n")
    print(f"ok: wrote {out_path} (frames: {len(frames)})")
    return 0


if __name__ == "__main__":
    sys.exit(main())

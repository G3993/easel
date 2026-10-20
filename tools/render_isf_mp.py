#!/usr/bin/env python3
"""
render_isf_mp.py — multipass offline ISF renderer (extends render_isf.py).

Runs EVERY pass of a multipass ISF shader the way Easel does: each named
TARGET gets a half-float ping-pong texture (so a pass may read its own
previous frame — persistent simulations work), sized passes honour their
WIDTH/HEIGHT expressions ($WIDTH, $HEIGHT, $input, floor/ceil/min/max), the
final pass renders at full res. Frames are stepped CONSECUTIVELY (dt 1/60 or
--dt) from t=0 up to each requested time so FRAMEINDEX and persistent buffers
evolve exactly like a live run.

CLI:
    render_isf_mp.py <shader.fs> --out out.png
        [--res 1920x1080] [--times 2,6,12] [--single] [--dt 0.01667]
        [--seek]                 jump straight to each time (no stepping; fast,
                                 breaks persistence)
        [--set name=value ...]   override an input (float, bool 0/1, color r,g,b)
        [--audio 0|1]            synthetic audio on (default) / all zeros
        [--pass N]               show pass N's buffer instead of the final image
Reuses render_isf.py's parser/preamble/translation so compilation matches.
"""
from __future__ import annotations
import argparse, math, os, re, sys
from pathlib import Path

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import render_isf as base  # noqa: E402
import moderngl  # noqa: E402
import numpy as np  # noqa: E402
from PIL import Image, ImageDraw  # noqa: E402


def eval_size(expr, W, H, inputs_vals):
    if expr is None:
        return None
    if isinstance(expr, (int, float)):
        return int(expr)
    s = str(expr)
    s = s.replace("$WIDTH", str(W)).replace("$HEIGHT", str(H))
    def sub(m):
        n = m.group(1)
        return str(float(inputs_vals.get(n, 1.0)))
    s = re.sub(r"\$([A-Za-z_][A-Za-z0-9_]*)", sub, s)
    try:
        v = eval(s, {"__builtins__": {}}, {"floor": math.floor, "ceil": math.ceil, "max": max, "min": min, "round": round})
        return max(1, int(v))
    except Exception as e:
        print(f"[warn] pass size expr '{expr}' failed: {e}", file=sys.stderr)
        return W


def parse_set(items):
    out = {}
    for it in items or []:
        if "=" not in it:
            continue
        k, v = it.split("=", 1)
        if "," in v:
            out[k] = tuple(float(x) for x in v.split(","))
        else:
            try:
                out[k] = float(v)
            except ValueError:
                out[k] = v
    return out


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("shader")
    ap.add_argument("--out", required=True)
    ap.add_argument("--res", default="1280x720")
    ap.add_argument("--times", default="0,1.5,3.5,7")
    ap.add_argument("--single", action="store_true")
    ap.add_argument("--dt", type=float, default=1.0 / 60.0)
    ap.add_argument("--seek", action="store_true")
    ap.add_argument("--set", action="append", default=[])
    ap.add_argument("--audio", type=int, default=1)
    ap.add_argument("--pass", dest="show_pass", type=int, default=-1)
    ap.add_argument("--msg", default="HELLO WORLD")
    args = ap.parse_args()

    shader_path = Path(args.shader)
    src = shader_path.read_text()
    body, isf, _ = base.parse_isf(src)
    inputs = isf.get("INPUTS", []) if isinstance(isf, dict) else []
    passes = isf.get("PASSES", [{}]) if isinstance(isf, dict) else [{}]
    if not passes:
        passes = [{}]
    targets = [p.get("TARGET") for p in passes if isinstance(p, dict) and p.get("TARGET")]
    overrides = parse_set(args.set)

    preamble = base.build_preamble(inputs, targets, body)
    body_t = base.translate_body(body, [i for i in inputs if i.get("TYPE") == "image"])
    frag_src = preamble + body_t
    W, H = base.parse_res(args.res)
    times = base.parse_times(args.times)
    if args.single:
        times = times[:1]

    ctx = moderngl.create_standalone_context(require=330)
    log_path = Path(args.out).with_suffix(".log")
    try:
        prog = ctx.program(vertex_shader=base.VERTEX_SRC, fragment_shader=frag_src)
    except moderngl.Error as e:
        log_path.write_text(f"=== compile/link error ===\n{e}\n\n=== fragment source ===\n{frag_src}\n")
        print(f"error: GLSL compile/link failed; see {log_path}", file=sys.stderr)
        return 3
    verts = np.array([-1, -1, 0, 0, 1, -1, 1, 0, -1, 1, 0, 1, 1, 1, 1, 1], dtype="f4")
    vbo = ctx.buffer(verts.tobytes())
    vao = ctx.vertex_array(prog, [(vbo, "2f 2f", "aPos", "aTexCoord")])

    # input values (defaults + overrides) for size expressions
    vals = {}
    for inp in inputs:
        n = inp["NAME"]
        v = overrides.get(n, inp.get("DEFAULT", 0.5))
        vals[n] = v if isinstance(v, (int, float)) else (1.0 if v is True else 0.0 if v is False else 1.0)

    # pass buffers: ping-pong half-float
    bufs = {}
    for p in passes:
        if not isinstance(p, dict) or not p.get("TARGET"):
            continue
        pw = eval_size(p.get("WIDTH"), W, H, vals) or W
        ph = eval_size(p.get("HEIGHT"), W, H, vals) or H
        pair = []
        for _ in range(2):
            t = ctx.texture((pw, ph), 4, dtype="f2")
            t.filter = (moderngl.LINEAR, moderngl.LINEAR)
            t.repeat_x = False; t.repeat_y = False
            fbo = ctx.framebuffer(color_attachments=[t])
            fbo.use(); ctx.clear(0, 0, 0, 0)
            pair.append((t, fbo))
        bufs[p["TARGET"]] = {"pair": pair, "cur": 0, "size": (pw, ph)}
    out_tex = ctx.texture((W, H), 4, dtype="f2")
    out_fbo = ctx.framebuffer(color_attachments=[out_tex])

    # stub textures
    fft_data = (np.linspace(0.2, 0.8, 256, dtype="f4") * (0.5 + 0.5 * np.sin(np.linspace(0, 6 * np.pi, 256)))).astype("f4")
    fft_tex = ctx.texture((256, 1), 1, fft_data.tobytes(), dtype="f4")
    atlas_arr = base.build_font_atlas()
    font_tex = ctx.texture((atlas_arr.shape[1], atlas_arr.shape[0]), 1, atlas_arr.tobytes())
    font_tex.swizzle = "RRRR"
    img_tex = ctx.texture((1, 1), 4, b"\x00\x00\x00\x00")
    S = base.safe_set

    def set_common(t, frame_index):
        S(prog, "TIME", float(t)); S(prog, "TIMEDELTA", float(args.dt)); S(prog, "FRAMEINDEX", int(frame_index))
        S(prog, "mousePos", (W * 0.5, H * 0.5)); S(prog, "mouseDelta", (0.0, 0.0)); S(prog, "pinchHold", 0.0); S(prog, "mouseDown", 0.0)
        S(prog, "msgAge", max(0.0, t))
        a = float(args.audio)
        S(prog, "audioLevel", a * (0.4 + 0.3 * math.sin(t * 1.2)))
        S(prog, "audioBass", a * (0.5 + 0.35 * math.sin(t * 0.9)))
        S(prog, "audioMid", a * (0.35 + 0.3 * math.sin(t * 2.0 + 1.0)))
        S(prog, "audioHigh", a * (0.25 + 0.25 * math.sin(t * 3.3 + 2.0)))
        S(prog, "audioBeatPulse", a * max(0.0, math.sin(t * 4.0)) ** 6)
        S(prog, "audioBeatPhase", (t * 2.0) % 1.0)
        S(prog, "audioEnergy", a * (0.5 + 0.3 * math.sin(t * 0.5)))
        for k in ("audioBassTime", "audioMidTime", "audioHighTime", "audioTime"):
            S(prog, k, t)
        S(prog, "_voiceLevel", 0.3 * a); S(prog, "_voiceGlitch", 0.0)
        fft_tex.use(location=2); S(prog, "audioFFT", 2)
        font_tex.use(location=3); S(prog, "fontAtlasTex", 3)
        img_tex.use(location=7)
        for name in ("mpPoseLandmarks", "mpFaceLandmarks", "mpHandLandmarks", "mpSegmentation"):
            S(prog, name, 7)
        unit = 8
        text_inputs = []
        for inp in inputs:
            ty = inp.get("TYPE", "float"); n = inp["NAME"]
            if ty == "text":
                text_inputs.append(inp); continue
            if ty == "image":
                img_tex.use(location=unit); S(prog, n, unit); S(prog, f"IMG_SIZE_{n}", (0.0, 0.0)); S(prog, f"_flip_{n}", False); unit += 1; continue
            if n in overrides:
                v = overrides[n]
                if ty == "float": S(prog, n, float(v))
                elif ty == "long": S(prog, n, int(v))
                elif ty in ("bool", "event"): S(prog, n, bool(float(v)))
                elif ty == "color":
                    vv = tuple(v) if isinstance(v, tuple) else (float(v),) * 3
                    S(prog, n, (vv + (1.0,))[:4] if len(vv) < 4 else vv)
                elif ty == "point2D": S(prog, n, tuple(v))
                continue
            val = base.default_for_input(inp, t)
            if val is None:
                continue
            if ty == "float": S(prog, n, float(val))
            elif ty == "long": S(prog, n, int(val))
            elif ty in ("bool", "event"): S(prog, n, bool(val))
            elif ty in ("color", "point2D"): S(prog, n, tuple(val))
        for inp in text_inputs:
            n = inp["NAME"]; max_len = inp.get("MAX_LENGTH", 12) or 12
            length, codes = base.encode_text(args.msg.upper(), max_len)
            S(prog, f"{n}_len", length)
            for i, c in enumerate(codes):
                S(prog, f"{n}_{i}", int(c))
        # pass buffers → units 20+
        for i, (name, b) in enumerate(bufs.items()):
            tex = b["pair"][b["cur"]][0]
            tex.use(location=20 + i); S(prog, name, 20 + i)

    def run_frame(t, frame_index):
        set_common(t, frame_index)
        for pi, p in enumerate(passes):
            tgt = p.get("TARGET") if isinstance(p, dict) else None
            if tgt:
                b = bufs[tgt]
                nxt = 1 - b["cur"]
                fbo = b["pair"][nxt][1]
                size = b["size"]
            else:
                fbo = out_fbo; size = (W, H)
            fbo.use(); ctx.viewport = (0, 0, size[0], size[1])
            S(prog, "PASSINDEX", pi); S(prog, "RENDERSIZE", (float(size[0]), float(size[1])))
            vao.render(moderngl.TRIANGLE_STRIP, vertices=4)
            if tgt:
                b["cur"] = nxt
                tex = b["pair"][nxt][0]
                idx = list(bufs.keys()).index(tgt)
                tex.use(location=20 + idx); S(prog, tgt, 20 + idx)

    def grab(show_pass):
        if show_pass >= 0 and isinstance(passes[show_pass], dict) and passes[show_pass].get("TARGET"):
            b = bufs[passes[show_pass]["TARGET"]]
            tex = b["pair"][b["cur"]][0]; size = b["size"]
        else:
            tex = out_tex; size = (W, H)
        raw = tex.read()
        arr = np.frombuffer(raw, dtype="f2").reshape(size[1], size[0], 4).astype("f4")
        arr = np.clip(arr[:, :, :3], 0.0, 1.0)
        arr = (np.flipud(arr) * 255.0 + 0.5).astype(np.uint8)
        return arr

    frames = []
    t_cur = 0.0; fi = 0
    for t in sorted(times):
        if args.seek:
            run_frame(t, fi); fi += 1
        else:
            while t_cur < t - 1e-6:
                run_frame(t_cur, fi); fi += 1; t_cur += args.dt
            run_frame(t, fi); fi += 1; t_cur = t + args.dt
        frames.append((t, grab(args.show_pass)))

    sheet = base.assemble_contact_sheet(frames, single=args.single)
    out_path = Path(args.out); out_path.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(out_path)
    log_path.write_text(f"=== render ok ===\nshader={shader_path}\nres={(W, H)} times={times} frames_run={fi}\n"
                        f"passes={[(p.get('TARGET'), bufs[p['TARGET']]['size']) if p.get('TARGET') else ('final', (W, H)) for p in passes]}\n")
    print(f"ok: wrote {out_path} (captures: {len(frames)}, frames run: {fi}, passes: {len(passes)})")
    return 0


if __name__ == "__main__":
    sys.exit(main())

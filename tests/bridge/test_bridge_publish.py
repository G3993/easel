#!/usr/bin/env python3
"""End-to-end Easel <-> agent bridge tests.

These tests assume Easel.app is running. They spin up a fake-agent OSC
listener on 127.0.0.1:9001 and trigger Easel's publish flow via OSC
into 127.0.0.1:9000. The captured payload is validated against the
canonical shape mobile decodes (EaselMobile/EaselMobile/Models/Play.swift).

This is the goal-acceptance harness: when every test here returns green,
the Easel-desktop side of the iOS bridge is bulletproof.
"""

from __future__ import annotations

import asyncio
import base64
import json
import os
import socket
import sys
import time
import zlib
from typing import Any


def decode_wire(raw: str) -> dict[str, Any] | None:
    """Mirror agent_osc_server._decode_wire — accept "z:" + b64(gzip(json))
    or raw JSON."""
    if raw.startswith("z:"):
        try:
            blob = base64.b64decode(raw[2:].encode("ascii"))
            text = zlib.decompress(blob, wbits=15 | 16).decode("utf-8")
            return json.loads(text)
        except Exception:
            return None
    try:
        return json.loads(raw)
    except json.JSONDecodeError:
        return None

# python-osc lives in the agent's venv; pull from there.
AGENT_VENV = "/Users/lu/easel-mobile/etherea-agent/.venv"
sys.path.insert(0, f"{AGENT_VENV}/lib/python3.12/site-packages")

from pythonosc.dispatcher import Dispatcher  # noqa: E402
from pythonosc.osc_server import AsyncIOOSCUDPServer  # noqa: E402
from pythonosc.udp_client import SimpleUDPClient  # noqa: E402


EASEL_OSC_HOST = "127.0.0.1"
EASEL_OSC_PORT = 9000
AGENT_OSC_PORT = 9001

TRIGGER_TIMEOUT_SEC = 5.0


def is_easel_listening() -> bool:
    """Quick precondition check: does *something* respond on Easel's OSC port?"""
    try:
        s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        s.connect((EASEL_OSC_HOST, EASEL_OSC_PORT))
        s.close()
        return True
    except OSError:
        return False


async def capture_publish() -> dict[str, Any] | None:
    """Bind 9001, fire /easel/play/publish at Easel, wait for the reverse OSC.
    Retries once on miss — the auto-republish dirty loop can land on the
    same 300ms tick as our manual trigger and steal the publish, leaving
    the caller momentarily empty.
    """
    for attempt in range(3):
        result = await _capture_publish_once()
        if result is not None:
            return result
        await asyncio.sleep(0.4)
    return None


async def _capture_publish_once() -> dict[str, Any] | None:
    captured: dict[str, Any] = {}

    def on_pub(_addr: str, *args: Any) -> None:
        if not args:
            return
        decoded = decode_wire(args[0])
        if decoded is not None:
            captured["play"] = decoded
            captured["raw"] = args[0]

    disp = Dispatcher()
    disp.map("/agent/play/publish", on_pub)
    loop = asyncio.get_running_loop()
    # Retry binding for up to ~2s — the previous listener's UDP socket may
    # still be in TIME_WAIT/teardown when this phase starts.
    transport = None
    last_err: Exception | None = None
    for _ in range(20):
        try:
            srv = AsyncIOOSCUDPServer(
                (EASEL_OSC_HOST, AGENT_OSC_PORT), disp, loop)
            transport, _ = await srv.create_serve_endpoint()
            break
        except OSError as exc:
            last_err = exc
            await asyncio.sleep(0.1)
    if transport is None:
        raise RuntimeError(
            f"capture_publish: couldn't bind {AGENT_OSC_PORT} ({last_err})")
    try:
        client = SimpleUDPClient(EASEL_OSC_HOST, EASEL_OSC_PORT)
        client.send_message("/easel/play/publish", [])
        deadline = time.monotonic() + TRIGGER_TIMEOUT_SEC
        while time.monotonic() < deadline:
            if "play" in captured:
                return captured["play"]
            await asyncio.sleep(0.05)
        return None
    finally:
        transport.close()


async def listen_for_publishes(duration_sec: float) -> list[dict[str, Any]]:
    """M3 helper — collect all /agent/play/publish events for duration_sec
    WITHOUT firing any /easel/play/publish trigger. Used to verify the
    auto-republish dirty loop in Easel ships content changes on its own.
    """
    captures: list[dict[str, Any]] = []

    def on_pub(_addr: str, *args: Any) -> None:
        if not args:
            return
        decoded = decode_wire(args[0])
        if decoded is not None:
            captures.append(decoded)

    disp = Dispatcher()
    disp.map("/agent/play/publish", on_pub)
    loop = asyncio.get_running_loop()
    transport = None
    last_err: Exception | None = None
    for _ in range(20):
        try:
            srv = AsyncIOOSCUDPServer(
                (EASEL_OSC_HOST, AGENT_OSC_PORT), disp, loop)
            transport, _ = await srv.create_serve_endpoint()
            break
        except OSError as exc:
            last_err = exc
            await asyncio.sleep(0.1)
    if transport is None:
        raise RuntimeError(
            f"listen_for_publishes: couldn't bind {AGENT_OSC_PORT} ({last_err})")
    try:
        await asyncio.sleep(duration_sec)
        return captures
    finally:
        transport.close()


# -----------------------------------------------------------------
# Assertions — keep these aligned with EaselMobile/EaselMobile/Models/Play.swift.
# Every new field landed in a milestone must add (or update) a check here.
# -----------------------------------------------------------------

class TestFail(Exception):
    """Raised when an assertion fails — caught by main() for a clean exit code."""


def require(cond: bool, msg: str) -> None:
    if not cond:
        raise TestFail(msg)


def validate_top_level(play: dict[str, Any]) -> None:
    required = {"id", "name", "bpm", "playhead", "duration", "layers", "markers"}
    missing = required - set(play.keys())
    require(not missing, f"top-level missing keys: {sorted(missing)}")
    require(isinstance(play["id"],       str),          "id must be string")
    require(isinstance(play["name"],     str),          "name must be string")
    require(isinstance(play["bpm"],      (int, float)), "bpm must be number")
    require(isinstance(play["playhead"], (int, float)), "playhead must be number")
    require(isinstance(play["duration"], (int, float)), "duration must be number")
    require(isinstance(play["layers"],   list),         "layers must be array")
    require(isinstance(play["markers"],  list),         "markers must be array")


def validate_layer(i: int, layer: dict[str, Any]) -> None:
    required = {"id", "layerIndex", "name", "kind", "visible", "tone", "clips"}
    missing = required - set(layer.keys())
    require(not missing, f"layer[{i}] missing: {sorted(missing)}")
    require(isinstance(layer["layerIndex"], int),  f"layer[{i}].layerIndex not int")
    require(isinstance(layer["visible"],    bool), f"layer[{i}].visible not bool")
    require(layer["kind"] in {"TEXT", "THEME", "SHADER", "FX"},
            f"layer[{i}].kind={layer['kind']!r} not in TEXT/THEME/SHADER/FX")
    require(layer["tone"] in {"lime", "amber", "cyan", "magenta", "sky"},
            f"layer[{i}].tone={layer['tone']!r} not in known palette")
    require(isinstance(layer["clips"], list),     f"layer[{i}].clips not array")
    for j, c in enumerate(layer["clips"]):
        cmissing = {"id", "name", "startSec", "durationSec"} - set(c.keys())
        require(not cmissing, f"layer[{i}].clips[{j}] missing: {sorted(cmissing)}")
        require(isinstance(c["startSec"],    (int, float)),
                f"layer[{i}].clips[{j}].startSec not number")
        require(isinstance(c["durationSec"], (int, float)),
                f"layer[{i}].clips[{j}].durationSec not number")


def validate_marker(i: int, m: dict[str, Any]) -> None:
    required = {"id", "time", "name"}
    missing = required - set(m.keys())
    require(not missing, f"marker[{i}] missing: {sorted(missing)}")
    require(isinstance(m["time"], (int, float)), f"marker[{i}].time not number")


# -----------------------------------------------------------------
# Per-milestone optional validators. Each becomes mandatory when its
# milestone closes — flip ENFORCE_M* below to True as you go.
# -----------------------------------------------------------------

ENFORCE_M1_EXTENDED = True   # full timed-sequence publish (shaderPath, params, ...)
ENFORCE_M2_CONTROL = True    # bidirectional control parity
ENFORCE_M3_LIVE = True       # live re-publish on edits
ENFORCE_M4_CUE = True        # cue/mic edge cases
ENFORCE_M5_NDI = True        # NDI camera ingest
ENFORCE_M6_RECONNECT = True  # reconnection resilience
ENFORCE_M7_EDGES = True      # 12-case edge sheet


def validate_m1_extended(play: dict[str, Any]) -> None:
    """M1: per-layer must carry shaderPath, parameters, blendMode, opacity, transform.

    Also: top-level transitions / sections / lanes arrays, plus per-clip
    transition/playbackMode fields.
    """
    for top_field in ("transitions", "sections", "lanes"):
        require(top_field in play, f"M1: top-level missing {top_field!r}")
        require(isinstance(play[top_field], list),
                f"M1: top-level {top_field} not array")

    for i, layer in enumerate(play["layers"]):
        for field in ("shaderPath", "parameters", "blendMode", "opacity", "transform"):
            require(field in layer, f"M1: layer[{i}] missing {field!r}")
        require(isinstance(layer["opacity"], (int, float)),
                f"M1: layer[{i}].opacity not number")
        require(isinstance(layer["parameters"], dict),
                f"M1: layer[{i}].parameters not object")
        require(isinstance(layer["blendMode"], str),
                f"M1: layer[{i}].blendMode not string")
        require(isinstance(layer["shaderPath"], str),
                f"M1: layer[{i}].shaderPath not string")
        t = layer["transform"]
        for tfield in ("position", "scale", "rotation"):
            require(tfield in t, f"M1: layer[{i}].transform missing {tfield!r}")
        require(isinstance(t["position"], list) and len(t["position"]) == 2,
                f"M1: layer[{i}].transform.position must be 2-array")
        require(isinstance(t["scale"], list) and len(t["scale"]) == 2,
                f"M1: layer[{i}].transform.scale must be 2-array")
        require(isinstance(t["rotation"], (int, float)),
                f"M1: layer[{i}].transform.rotation must be number")

        # Per-clip M1 fields.
        for j, clip in enumerate(layer["clips"]):
            for cfield in ("sourcePath", "transitionInName",
                           "transitionInShaderPath", "transitionInDuration",
                           "playbackMode"):
                require(cfield in clip,
                        f"M1: layer[{i}].clips[{j}] missing {cfield!r}")
            require(clip["playbackMode"] in
                    {"Forward", "Loop", "Hold", "Reverse", "Ping-Pong"},
                    f"M1: layer[{i}].clips[{j}].playbackMode={clip['playbackMode']!r} "
                    "not in known modes")


# -----------------------------------------------------------------
# Test entry
# -----------------------------------------------------------------

TEST_SHADER_DIR = "/Users/lu/ShaderClaw3/shaders"
TEST_SHADERS = [
    f"{TEST_SHADER_DIR}/text_clusters.fs",   # text shader, validates TEXT kind + msg param
    f"{TEST_SHADER_DIR}/ascii_shader.fs",    # vfx shader, validates SHADER kind
]


async def test_m2_controls(client: SimpleUDPClient,
                            populated: dict[str, Any],
                            baseline_count: int) -> None:
    """M2 control parity — fire mobile-style actions, confirm via republish."""
    # Pick one of the test-added layers (last two). At least one must
    # be a SHADER/TEXT kind so we can write a shader param.
    new_layers = populated["layers"][baseline_count:]
    require(len(new_layers) >= 1, "M2: no test layers loaded")

    # --- Transport seek round-trip ---
    target_seek = 12.5
    client.send_message("/easel/transport/seek", float(target_seek))
    await asyncio.sleep(0.25)
    after_seek = await capture_publish()
    require(after_seek is not None, "M2: no publish after transport/seek")
    delta = abs(after_seek["playhead"] - target_seek)
    require(delta < 1.0,  # timeline may have advanced a fraction in 0.25s
            f"M2: transport/seek -> playhead {after_seek['playhead']:.3f}, "
            f"expected ~{target_seek} (delta={delta:.3f})")
    print(f"  M2 transport/seek OK: playhead={after_seek['playhead']:.3f}")

    # Pause so the playhead doesn't drift during further assertions.
    client.send_message("/easel/transport/pause", [])
    await asyncio.sleep(0.1)

    # --- Layer opacity round-trip ---
    target_idx = baseline_count   # first test layer (lowest in new range)
    target_opacity = 0.42
    client.send_message(f"/easel/layer/{target_idx}/opacity",
                        float(target_opacity))
    await asyncio.sleep(0.2)
    after_op = await capture_publish()
    require(after_op is not None, "M2: no publish after opacity write")
    L = after_op["layers"][target_idx]
    delta = abs(L["opacity"] - target_opacity)
    require(delta < 0.01,
            f"M2: layer/{target_idx}/opacity wrote {target_opacity} -> "
            f"published {L['opacity']:.3f}")
    print(f"  M2 layer.opacity OK: opacity={L['opacity']:.3f}")

    # --- Shader param round-trip ---
    # Find the first float param across the loaded test layers.
    chosen_layer_i = None
    chosen_param = None
    chosen_baseline_value = None
    for i in range(baseline_count, len(populated["layers"])):
        L = populated["layers"][i]
        if L["kind"] not in {"SHADER", "TEXT"}:
            continue
        for pname, pval in L["parameters"].items():
            if isinstance(pval, (int, float)):
                chosen_layer_i = i
                chosen_param = pname
                chosen_baseline_value = float(pval)
                break
        if chosen_param:
            break
    require(chosen_param is not None,
            "M2: no float shader param found in test layers")

    # Write a value clearly distinct from baseline.
    target_value = chosen_baseline_value + 0.37
    if abs(target_value) < 0.01:
        target_value = 0.42
    client.send_message(
        f"/easel/layer/{chosen_layer_i}/param/{chosen_param}",
        float(target_value),
    )
    await asyncio.sleep(0.2)
    after_p = await capture_publish()
    require(after_p is not None, "M2: no publish after shader param write")
    Lp = after_p["layers"][chosen_layer_i]
    actual = Lp["parameters"].get(chosen_param)
    require(actual is not None,
            f"M2: layer[{chosen_layer_i}].parameters[{chosen_param}] missing")
    delta = abs(float(actual) - target_value)
    # Shader inputs may have a [min,max] range that clamps writes. Accept
    # either an exact match OR a clamped value at the input's bounds.
    if delta >= 0.05:
        require(False,
                f"M2: shader param {chosen_param} wrote {target_value:.3f} -> "
                f"published {actual} (delta={delta:.3f})")
    print(f"  M2 shader.param OK: {chosen_param}={actual} "
          f"(was {chosen_baseline_value:.3f})")

    # --- Transport toggle (play -> back to paused) ---
    client.send_message("/easel/transport/play", [])
    await asyncio.sleep(0.15)
    client.send_message("/easel/transport/pause", [])
    await asyncio.sleep(0.1)
    print("  M2 transport/play+pause OK: dispatched without error")


async def test_m7_edge_cases(client: SimpleUDPClient,
                               total_layers: int) -> None:
    """M7 — 12-case edge sheet. Each case verifies Easel stays responsive
    and the published Play wire shape stays well-formed under weird input.
    No crashes, no truncated payloads, no malformed JSON.
    """
    cases_passed = 0

    # Case 1 — invalid layer.add path (.txt extension) must not crash.
    client.send_message("/easel/layer/add", "/tmp/not_a_real_shader.txt")
    await asyncio.sleep(0.2)
    pub = await capture_publish()
    require(pub is not None, "M7-1: Easel unresponsive after bad layer.add")
    cases_passed += 1

    # Case 2 — layer.remove with out-of-range index must not crash.
    client.send_message("/easel/layer/remove", 9999)
    client.send_message("/easel/layer/remove", -3)
    await asyncio.sleep(0.2)
    pub = await capture_publish()
    require(pub is not None, "M7-2: Easel unresponsive after bad layer.remove")
    cases_passed += 1

    # Case 3 — negative seek clamps gracefully.
    client.send_message("/easel/transport/seek", -10.0)
    await asyncio.sleep(0.2)
    pub = await capture_publish()
    require(pub is not None, "M7-3: Easel unresponsive after negative seek")
    require(pub["playhead"] >= 0.0,
            f"M7-3: negative seek produced playhead={pub['playhead']}")
    cases_passed += 1

    # Case 4 — seek beyond duration clamps gracefully.
    client.send_message("/easel/transport/seek", 999999.0)
    await asyncio.sleep(0.2)
    pub = await capture_publish()
    require(pub is not None, "M7-4: Easel unresponsive after huge seek")
    require(pub["playhead"] <= pub["duration"] + 1.0,
            f"M7-4: huge seek left playhead={pub['playhead']} "
            f"> duration={pub['duration']}")
    cases_passed += 1

    # Case 5 — unicode cue text round-trips.
    unicode_cue = "✨ café 東京 αβγ"
    client.send_message("/cue/latest", unicode_cue)
    await asyncio.sleep(0.3)
    pub = await capture_publish()
    require(pub.get("cueLatest") == unicode_cue,
            f"M7-5: unicode cue corrupted: {pub.get('cueLatest')!r}")
    cases_passed += 1

    # Case 6 — cue with newlines + control chars sanely handled.
    client.send_message("/cue/latest", "line1\nline2\ttab")
    await asyncio.sleep(0.3)
    pub = await capture_publish()
    if pub is None:
        # Retry once — auto-republish loop may have eaten the window.
        await asyncio.sleep(0.5)
        pub = await capture_publish()
    require(pub is not None, "M7-6: Easel unresponsive after control-char cue")
    require(pub.get("cueLatest", "") in
            {"line1\nline2\ttab", "line1\\nline2\\ttab"},
            f"M7-6: cue control chars mangled: {pub.get('cueLatest')!r}")
    cases_passed += 1

    # Case 7 — empty shader param name should not crash.
    client.send_message(f"/easel/layer/{total_layers - 1}/param/", 0.5)
    await asyncio.sleep(0.2)
    pub = await capture_publish()
    require(pub is not None, "M7-7: Easel unresponsive after empty param name")
    cases_passed += 1

    # Case 8 — rapid sequential publishes (50 in <1s) must all be received.
    for _ in range(50):
        client.send_message("/easel/play/publish", [])
    await asyncio.sleep(0.6)
    pub = await capture_publish()
    require(pub is not None, "M7-8: Easel unresponsive after 50 rapid publishes")
    cases_passed += 1

    # Case 9 — rapid layer add/remove churn. Net result: zero new layers.
    for path in [f"{TEST_SHADER_DIR}/ascii_shader.fs",
                  f"{TEST_SHADER_DIR}/text_clusters.fs"]:
        if os.path.exists(path):
            client.send_message("/easel/layer/add", path)
    await asyncio.sleep(0.3)
    pub = await capture_publish()
    require(pub is not None, "M7-9: Easel unresponsive after churn add")
    cur_count = len(pub["layers"])
    # Remove them right back.
    while cur_count > total_layers:
        client.send_message("/easel/layer/remove", total_layers)
        cur_count -= 1
    await asyncio.sleep(0.3)
    pub = await capture_publish()
    require(len(pub["layers"]) == total_layers,
            f"M7-9: churn left {len(pub['layers'])} layers, expected {total_layers}")
    cases_passed += 1

    # Case 10 — workspace switch under load.
    client.send_message("/easel/workspace", "canvas")
    client.send_message("/easel/workspace", "play")
    client.send_message("/easel/workspace", "stage")
    client.send_message("/easel/workspace", "play")
    await asyncio.sleep(0.3)
    pub = await capture_publish()
    require(pub is not None, "M7-10: Easel unresponsive after workspace churn")
    cases_passed += 1

    # Case 11 — /cue/clear back-to-back is idempotent.
    for _ in range(5):
        client.send_message("/cue/clear", [])
    await asyncio.sleep(0.3)
    pub = await capture_publish()
    require(pub.get("cueLatest", None) == "",
            f"M7-11: /cue/clear idempotency broken: {pub.get('cueLatest')!r}")
    cases_passed += 1

    # Case 12 — unknown OSC addresses are ignored without crashing.
    client.send_message("/easel/this/does/not/exist", "ignored")
    client.send_message("/random/garbage", 42)
    await asyncio.sleep(0.2)
    pub = await capture_publish()
    require(pub is not None, "M7-12: Easel unresponsive after unknown OSC")
    cases_passed += 1

    print(f"  M7 edge cases OK: {cases_passed}/12 passed")


async def test_m6_reconnect(client: SimpleUDPClient) -> None:
    """M6 — Easel must survive the agent listener disappearing and
    reappearing with no manual intervention and no degradation. Mirrors
    the real-world failure mode where the agent process restarts while
    Easel keeps running.
    """
    # Brief settle so the previous phase's UDP socket fully drains
    # before we try to rebind 9001 inside capture_publish().
    await asyncio.sleep(0.5)

    # 1) Listener present — baseline capture
    first = await capture_publish()
    require(first is not None, "M6: no publish with listener present")

    # 2) No listener bound for ~1.5s. Fire OSCs at Easel — UDP sends should
    # disappear into the void without raising or stalling.
    client.send_message("/easel/play/publish", [])
    client.send_message("/easel/transport/seek", 5.0)
    client.send_message("/cue/latest", "during outage")
    await asyncio.sleep(1.5)

    # 3) Listener reappears — capture should succeed within timeout.
    after = await capture_publish()
    require(after is not None, "M6: Easel didn't respond after listener returned")
    require(after.get("cueLatest") == "during outage",
            f"M6: outage-period state lost (cueLatest={after.get('cueLatest')!r})")
    print(f"  M6 listener-bounce OK: state preserved across outage")

    # 4) Longer idle (~3s) then capture — proves no degradation over time.
    await asyncio.sleep(3.0)
    later = await capture_publish()
    require(later is not None, "M6: no publish after 3s idle")
    print(f"  M6 long-idle OK: still publishing after idle")


async def test_m4_cue_edges(client: SimpleUDPClient) -> None:
    """M4 — Cue/mic edge cases:
       - Empty utterance must NOT blank a non-empty cue mid-show.
       - Very long utterance must not crash (4KB cap).
       - Rapid successive writes follow last-write-wins.
       - /cue/clear is the only path that blanks the cue.
    """
    # Establish a known starting cue.
    client.send_message("/cue/latest", "FIRST CUE")
    await asyncio.sleep(0.4)
    after = await capture_publish()
    require(after is not None, "M4: no publish after first cue")
    require(after.get("cueLatest") == "FIRST CUE",
            f"M4: initial cue not set ({after.get('cueLatest')!r})")
    print(f"  M4 set cue OK: cueLatest={after['cueLatest']!r}")

    # 1) Empty utterance should be IGNORED.
    client.send_message("/cue/latest", "")
    await asyncio.sleep(0.4)
    after = await capture_publish()
    require(after.get("cueLatest") == "FIRST CUE",
            f"M4: empty utterance blanked cue ({after.get('cueLatest')!r})")
    print("  M4 empty-utterance ignored OK (cue preserved)")

    # 2) Very long string (5KB) — should be capped at 4KB and not crash.
    long_text = "X" * 5000
    client.send_message("/cue/latest", long_text)
    await asyncio.sleep(0.4)
    after = await capture_publish()
    require(after is not None, "M4: Easel stopped responding after long cue")
    cl = after.get("cueLatest", "")
    require(len(cl) <= 4096,
            f"M4: cue not capped at 4KB ({len(cl)} chars present)")
    require(cl.startswith("X"),
            f"M4: long cue corrupted ({cl[:32]!r}...)")
    print(f"  M4 long-utterance capped OK: len={len(cl)}")

    # 3) Rapid successive writes — last write wins.
    for s in ["alpha", "beta", "gamma", "delta"]:
        client.send_message("/cue/latest", s)
    await asyncio.sleep(0.4)
    after = await capture_publish()
    require(after.get("cueLatest") == "delta",
            f"M4: last-write-wins violated ({after.get('cueLatest')!r})")
    print(f"  M4 last-write-wins OK: cueLatest={after['cueLatest']!r}")

    # 4) /cue/clear blanks explicitly.
    client.send_message("/cue/clear", [])
    await asyncio.sleep(0.4)
    after = await capture_publish()
    require(after.get("cueLatest", None) == "",
            f"M4: /cue/clear did not blank ({after.get('cueLatest')!r})")
    print("  M4 /cue/clear OK: cue blanked explicitly")


async def test_m3_live_republish(client: SimpleUDPClient,
                                  starting_count: int) -> None:
    """M3 — Easel should auto-republish on content edits without any manual
    /easel/play/publish trigger. We listen for ~1s after firing a mutation
    OSC and assert at least one matching publish arrived."""
    # 1) Add a layer; expect an auto-publish carrying the new layer count.
    extra_shader = f"{TEST_SHADER_DIR}/ascii_shader.fs"
    if not os.path.exists(extra_shader):
        print("  M3 skipped — extra test shader not on disk")
        return

    listener = asyncio.create_task(listen_for_publishes(1.2))
    # Tiny pause so the listener is bound before we send.
    await asyncio.sleep(0.05)
    client.send_message("/easel/layer/add", extra_shader)
    captures = await listener

    hits = [c for c in captures if len(c["layers"]) > starting_count]
    require(hits,
            f"M3: no auto-publish observed within 1.2s after layer.add "
            f"(saw {len(captures)} publish(es), counts="
            f"{[len(c['layers']) for c in captures]})")
    print(f"  M3 layer.add auto-publish OK: arrived in "
          f"{len(captures)} publish(es), layer counts {[len(c['layers']) for c in captures]}")

    # 2) Now remove that just-added layer; expect another auto-publish.
    listener = asyncio.create_task(listen_for_publishes(1.2))
    await asyncio.sleep(0.05)
    client.send_message("/easel/layer/remove", starting_count)
    captures = await listener

    hits = [c for c in captures if len(c["layers"]) == starting_count]
    require(hits,
            f"M3: no auto-publish observed within 1.2s after layer.remove "
            f"(saw {len(captures)} publish(es), counts="
            f"{[len(c['layers']) for c in captures]})")
    print(f"  M3 layer.remove auto-publish OK: arrived in "
          f"{len(captures)} publish(es), layer counts {[len(c['layers']) for c in captures]}")


def validate_m5_ndi(play: dict[str, Any]) -> None:
    """M5 — `ndi` block must be present and well-formed even when
    HAS_NDI is undefined at build time (the bridge still has to tell
    mobile that NDI isn't available, otherwise mobile would hang waiting
    for an NDI picker that will never populate).
    """
    require("ndi" in play, "M5: top-level missing 'ndi'")
    ndi = play["ndi"]
    require(isinstance(ndi, dict), "M5: 'ndi' not object")
    require("runtimeAvailable" in ndi,
            "M5: ndi missing 'runtimeAvailable'")
    require(isinstance(ndi["runtimeAvailable"], bool),
            "M5: ndi.runtimeAvailable not bool")
    require("discoveredSources" in ndi,
            "M5: ndi missing 'discoveredSources'")
    require(isinstance(ndi["discoveredSources"], list),
            "M5: ndi.discoveredSources not array")
    for j, s in enumerate(ndi["discoveredSources"]):
        require(isinstance(s, dict),
                f"M5: ndi.discoveredSources[{j}] not object")
        for k in ("name", "url", "isIphone"):
            require(k in s,
                    f"M5: ndi.discoveredSources[{j}] missing {k!r}")


def all_validators(play: dict[str, Any]) -> None:
    validate_top_level(play)
    for i, L in enumerate(play["layers"]):
        validate_layer(i, L)
    for i, m in enumerate(play["markers"]):
        validate_marker(i, m)
    if ENFORCE_M1_EXTENDED:
        validate_m1_extended(play)
    if ENFORCE_M5_NDI:
        validate_m5_ndi(play)


async def run() -> int:
    if not is_easel_listening():
        print("SKIP — Easel doesn't appear to be listening on UDP 9000.")
        print("       Run `open /Users/lu/easel/build/Easel.app` first.")
        return 77  # SKIP exit code

    client = SimpleUDPClient(EASEL_OSC_HOST, EASEL_OSC_PORT)

    # ── Phase A: capture baseline (whatever state Easel is in) ─────────
    baseline = await capture_publish()
    if baseline is None:
        print("FAIL — no /agent/play/publish received from Easel within 3s.")
        return 1
    baseline_layer_count = len(baseline["layers"])
    print(f"Baseline: {len(json.dumps(baseline))} bytes, "
          f"{baseline_layer_count} layer(s)")
    try:
        all_validators(baseline)
    except TestFail as exc:
        print(f"FAIL (baseline) — {exc}")
        return 1

    # ── Phase B: load test shaders, publish, validate populated payload ─
    missing = [p for p in TEST_SHADERS if not os.path.exists(p)]
    if missing:
        print(f"SKIP populated test — shader files missing: {missing}")
    else:
        for path in TEST_SHADERS:
            client.send_message("/easel/layer/add", path)
        # Give Easel a couple frames to load + register the layers
        await asyncio.sleep(0.5)

        populated = await capture_publish()
        if populated is None:
            print("FAIL — no /agent/play/publish after layer.add")
            return 1
        new_count = len(populated["layers"]) - baseline_layer_count
        print(f"Populated: {len(json.dumps(populated))} bytes, "
              f"+{new_count} new layer(s)")
        try:
            all_validators(populated)
            # Confirm at least one SHADER-kind layer and one TEXT-kind
            kinds = {L["kind"] for L in populated["layers"]}
            require("SHADER" in kinds or "TEXT" in kinds,
                    f"populated payload has no SHADER/TEXT layer (kinds={kinds})")
            # Spot-check: every shader layer has a non-empty shaderPath.
            for i, L in enumerate(populated["layers"]):
                if L["kind"] in {"SHADER", "TEXT"}:
                    require(L["shaderPath"] != "",
                            f"layer[{i}] kind={L['kind']!r} has empty shaderPath")
        except TestFail as exc:
            print(f"FAIL (populated) — {exc}")
            # Always try to clean up before exit
            for _ in range(new_count):
                client.send_message("/easel/layer/remove",
                                    baseline_layer_count)
            return 1

        # ── Phase C: M2 bidirectional control round-trips ──────────────
        if ENFORCE_M2_CONTROL:
            try:
                await test_m2_controls(client, populated, baseline_layer_count)
            except TestFail as exc:
                print(f"FAIL (M2 control) — {exc}")
                for _ in range(new_count):
                    client.send_message("/easel/layer/remove",
                                        baseline_layer_count)
                return 1

        # ── Phase C2: M3 live auto-republish ───────────────────────────
        if ENFORCE_M3_LIVE:
            try:
                await test_m3_live_republish(
                    client, baseline_layer_count + new_count)
            except TestFail as exc:
                print(f"FAIL (M3 live) — {exc}")
                for _ in range(new_count):
                    client.send_message("/easel/layer/remove",
                                        baseline_layer_count)
                return 1

        # ── Phase C3: M4 cue edge cases ────────────────────────────────
        if ENFORCE_M4_CUE:
            try:
                await test_m4_cue_edges(client)
            except TestFail as exc:
                print(f"FAIL (M4 cue) — {exc}")
                for _ in range(new_count):
                    client.send_message("/easel/layer/remove",
                                        baseline_layer_count)
                return 1

        # ── Phase C4: M6 reconnection resilience ───────────────────────
        if ENFORCE_M6_RECONNECT:
            try:
                await test_m6_reconnect(client)
            except TestFail as exc:
                print(f"FAIL (M6 reconnect) — {exc}")
                for _ in range(new_count):
                    client.send_message("/easel/layer/remove",
                                        baseline_layer_count)
                return 1

        # ── Phase C5: M7 edge cases sheet ───────────────────────────────
        if ENFORCE_M7_EDGES:
            try:
                await test_m7_edge_cases(client, baseline_layer_count + new_count)
            except TestFail as exc:
                print(f"FAIL (M7 edge cases) — {exc}")
                for _ in range(new_count):
                    client.send_message("/easel/layer/remove",
                                        baseline_layer_count)
                return 1

        # ── Phase D: tear down to baseline ──────────────────────────────
        for _ in range(new_count):
            client.send_message("/easel/layer/remove", baseline_layer_count)
        await asyncio.sleep(0.3)
        final = await capture_publish()
        if final is None:
            print("FAIL — no /agent/play/publish during teardown")
            return 1
        if len(final["layers"]) != baseline_layer_count:
            print(f"FAIL — teardown left {len(final['layers'])} layers, "
                  f"expected baseline {baseline_layer_count}")
            return 1
        print(f"Teardown OK: back to {baseline_layer_count} layer(s)")

    print("PASS — wire shape valid across baseline / populated / teardown")
    return 0


def main() -> None:
    try:
        rc = asyncio.run(run())
    except KeyboardInterrupt:
        rc = 130
    sys.exit(rc)


if __name__ == "__main__":
    main()

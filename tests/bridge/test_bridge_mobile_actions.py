#!/usr/bin/env python3
"""Mobile-style action tests — exercise the agent → Easel path.

The mobile app drives Easel through relay actions (WebSocket → agent → OSC).
These tests skip the WebSocket leg by instantiating an EaselBridge and
calling the agent action handlers directly — that's exactly the work the
agent does after it pulls the payload off the relay. End-effect on Easel
is the same.

Verifies B-agent (new agent action wrappers): layer.add, layer.remove,
shader.param, layer.transform, transport.*, workspace.set.
"""

from __future__ import annotations

import asyncio
import base64
import json
import os
import sys
import time
import zlib

AGENT_DIR = "/Users/lu/easel-mobile/etherea-agent"
AGENT_VENV = f"{AGENT_DIR}/.venv"
sys.path.insert(0, AGENT_DIR)
sys.path.insert(0, f"{AGENT_VENV}/lib/python3.12/site-packages")

from pythonosc.dispatcher import Dispatcher  # noqa: E402
from pythonosc.osc_server import AsyncIOOSCUDPServer  # noqa: E402
from pythonosc.udp_client import SimpleUDPClient  # noqa: E402


EASEL_OSC_HOST = "127.0.0.1"
EASEL_OSC_PORT = 9000
AGENT_OSC_PORT = 9001
TEST_SHADER = "/Users/lu/ShaderClaw3/shaders/text_clusters.fs"
EXTRA_SHADER = "/Users/lu/ShaderClaw3/shaders/ascii_shader.fs"


class TestFail(Exception):
    pass


def require(cond: bool, msg: str) -> None:
    if not cond:
        raise TestFail(msg)


async def capture_publish(timeout_sec: float = 3.0) -> dict | None:
    captured: dict = {}

    def on_pub(_addr, *args):
        if not args:
            return
        raw = args[0]
        if isinstance(raw, str) and raw.startswith("z:"):
            try:
                blob = base64.b64decode(raw[2:].encode("ascii"))
                text = zlib.decompress(blob, wbits=15 | 16).decode("utf-8")
                captured["play"] = json.loads(text)
                return
            except Exception:
                pass
        try:
            captured["play"] = json.loads(raw)
        except (json.JSONDecodeError, TypeError):
            pass

    disp = Dispatcher()
    disp.map("/agent/play/publish", on_pub)
    loop = asyncio.get_running_loop()
    transport = None
    for _ in range(20):
        try:
            srv = AsyncIOOSCUDPServer(
                (EASEL_OSC_HOST, AGENT_OSC_PORT), disp, loop)
            transport, _ = await srv.create_serve_endpoint()
            break
        except OSError:
            await asyncio.sleep(0.1)
    if transport is None:
        raise RuntimeError("could not bind 9001")
    try:
        client = SimpleUDPClient(EASEL_OSC_HOST, EASEL_OSC_PORT)
        client.send_message("/easel/play/publish", [])
        deadline = time.monotonic() + timeout_sec
        while time.monotonic() < deadline:
            if "play" in captured:
                return captured["play"]
            await asyncio.sleep(0.05)
        return None
    finally:
        transport.close()


async def run() -> int:
    # Instantiate the agent ourselves so we can drive _handle_X directly.
    # This is exactly the work the agent does after pulling a relay payload
    # off the WebSocket — same EaselBridge instance, same OSC sends.
    from agent import Agent
    agent = Agent()

    # ── Capture starting state ─────────────────────────────────────────
    baseline = await capture_publish()
    if baseline is None:
        print("SKIP — Easel not responding")
        return 77
    start_count = len(baseline["layers"])
    print(f"Baseline: {start_count} layers")

    # ── B-agent — layer.add ─────────────────────────────────────────────
    if not os.path.exists(TEST_SHADER):
        print(f"SKIP — test shader missing: {TEST_SHADER}")
        return 77
    r = await agent._handle_layer_add({"path": TEST_SHADER})
    require(r.get("added"), f"layer.add response: {r}")
    await asyncio.sleep(0.4)
    after = await capture_publish()
    require(len(after["layers"]) == start_count + 1,
            f"layer.add did not grow stack ({start_count} -> "
            f"{len(after['layers'])})")
    added_idx = start_count  # newly added is at the top
    print(f"  B-agent layer.add OK: {start_count} -> {len(after['layers'])}")

    # ── B-agent — shader.param (writing through the agent path) ─────────
    L = after["layers"][added_idx]
    require(L["kind"] in {"SHADER", "TEXT"},
            f"added layer not SHADER/TEXT kind ({L['kind']})")
    float_params = [(k, v) for k, v in L["parameters"].items()
                     if isinstance(v, (int, float))]
    require(float_params, "added layer has no float params to test")
    param_name, param_baseline = float_params[0]
    new_value = float(param_baseline) + 0.42
    if abs(new_value) < 0.01:
        new_value = 0.42
    r = await agent._handle_shader_param({
        "layerIndex": added_idx,
        "name": param_name,
        "value": new_value,
    })
    require(r.get("set"), f"shader.param response: {r}")
    await asyncio.sleep(0.3)
    after = await capture_publish()
    actual = after["layers"][added_idx]["parameters"][param_name]
    require(abs(float(actual) - new_value) < 0.05,
            f"shader.param wrote {new_value} -> published {actual}")
    print(f"  B-agent shader.param OK: {param_name}={actual}")

    # ── B-agent — layer.transform ──────────────────────────────────────
    r = await agent._handle_layer_transform({
        "layerIndex": added_idx, "field": "opacity", "value": 0.37,
    })
    require(r.get("set"), f"layer.transform response: {r}")
    await asyncio.sleep(0.3)
    after = await capture_publish()
    op = after["layers"][added_idx]["opacity"]
    require(abs(op - 0.37) < 0.01,
            f"layer.transform opacity wrote 0.37 -> published {op}")
    print(f"  B-agent layer.transform OK: opacity={op}")

    # ── B-agent — transport.seek then pause ────────────────────────────
    r = await agent._handle_transport_seek({"time": 8.25})
    require(r.get("sought"), f"transport.seek response: {r}")
    await asyncio.sleep(0.2)
    await agent._handle_transport_pause({})
    after = await capture_publish()
    require(abs(after["playhead"] - 8.25) < 1.0,
            f"transport.seek 8.25 -> playhead {after['playhead']}")
    print(f"  B-agent transport.seek OK: playhead={after['playhead']:.3f}")

    # ── B-agent — workspace.set ────────────────────────────────────────
    r = await agent._handle_workspace_set({"name": "play"})
    require(r.get("set"), f"workspace.set response: {r}")
    print("  B-agent workspace.set OK")

    # ── Validation: bad payloads return error, no crash ────────────────
    r = await agent._handle_layer_add({})  # missing path
    require("error" in r, f"layer.add(missing path) should error: {r}")
    r = await agent._handle_shader_param({"layerIndex": 0})  # missing name
    require("error" in r, f"shader.param(missing name) should error: {r}")
    r = await agent._handle_transport_seek({})  # missing time
    require("error" in r, f"transport.seek(missing time) should error: {r}")
    r = await agent._handle_workspace_set({"name": "garbage"})
    require("error" in r, f"workspace.set(garbage) should error: {r}")
    print("  B-agent input validation OK")

    # ── B-agent — layer.move (reorder) ─────────────────────────────────
    # Add a second test layer so we have two to swap.
    if os.path.exists(EXTRA_SHADER):
        r = await agent._handle_layer_add({"path": EXTRA_SHADER})
        require(r.get("added"), f"layer.add(extra) response: {r}")
        await asyncio.sleep(0.4)
        after = await capture_publish()
        require(len(after["layers"]) == start_count + 2,
                f"two test layers not present (got {len(after['layers'])})")
        names_before = [L["name"] for L in after["layers"]]
        # Move the newly-added (top) layer to the bottom of the test slice.
        top = start_count + 1
        bot = start_count
        r = await agent._handle_layer_move({"from": top, "to": bot})
        require(r.get("moved"), f"layer.move response: {r}")
        await asyncio.sleep(0.4)
        after = await capture_publish()
        names_after = [L["name"] for L in after["layers"]]
        require(names_after[top] == names_before[bot]
                and names_after[bot] == names_before[top],
                f"layer.move didn't swap ({names_before} -> {names_after})")
        print(f"  B-agent layer.move OK: swapped [{bot}] <-> [{top}]")
        # Pop both test layers off the top.
        await agent._handle_layer_remove({"layerIndex": start_count + 1})
        await agent._handle_layer_remove({"layerIndex": start_count})
        await asyncio.sleep(0.4)
    else:
        # Skip path: only one test shader on disk. Tear down the single
        # test layer added earlier and exit.
        r = await agent._handle_layer_remove({"layerIndex": added_idx})
        require(r.get("removed"), f"layer.remove response: {r}")
        await asyncio.sleep(0.3)

    after = await capture_publish()
    require(len(after["layers"]) == start_count,
            f"teardown did not return to {start_count}: "
            f"{len(after['layers'])}")
    print(f"  B-agent teardown OK: back to {start_count}")

    print("PASS — all mobile-style agent actions OK")
    return 0


def main():
    try:
        rc = asyncio.run(run())
    except TestFail as exc:
        print(f"FAIL — {exc}")
        rc = 1
    except KeyboardInterrupt:
        rc = 130
    sys.exit(rc)


if __name__ == "__main__":
    main()

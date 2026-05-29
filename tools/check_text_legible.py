#!/usr/bin/env python3
"""
check_text_legible.py — vision-pass CLI for the text-legibility gate.

Emits a prompt file for an orchestrating vision agent to fill in. The agent
inspects the rendered PNG and writes a small JSON next to it documenting
whether the rendered text is readable in the orientation a human expects.

Two modes:
  --emit-prompt   write check_text_legible.prompt.json next to the PNG and
                  stub check_text_legible.json with {"text_legible": null}
                  so downstream tools have a uniform shape.
  --merge         after the agent has written check_text_legible.agent.json,
                  copy its `text_legible` + `reason` into check_text_legible.json.

Schema (final check_text_legible.json):
    {
      "text_legible": true|false|null,
      "reason": "...",
      "verdict": "PASS"|"BACKWARD_X"|"FLIPPED_Y"|"GARBLED"|"NO_TEXT_VISIBLE"
    }

The eval orchestrator passes `--has-text-input` so this script can skip the
check entirely on shaders without a `msg` text input (it writes
text_legible=true with verdict="N/A" in that case).
"""

from __future__ import annotations
import argparse
import json
import sys
from pathlib import Path


def emit_prompt(png_path: Path, out_dir: Path, has_text_input: bool,
                sample_msg: str) -> None:
    """Write the prompt + stub JSON for the orchestrator."""
    stub_path = out_dir / "check_text_legible.json"
    prompt_path = out_dir / "check_text_legible.prompt.json"

    if not has_text_input:
        # Shaders without a `msg` text input bypass the gate.
        stub_path.write_text(json.dumps({
            "text_legible": True,
            "reason": "shader has no `msg` text input — gate not applicable",
            "verdict": "N/A",
        }, indent=2))
        # Still emit a prompt so the harness shape is uniform.
        prompt_path.write_text(json.dumps({
            "task": "text_legibility (skipped)",
            "skip": True,
            "reason": "no msg text input",
        }, indent=2))
        return

    prompt = {
        "task": "Inspect the rendered PNG and decide if the visible text "
                "is legible in the expected human reading orientation.",
        "rendered_png": str(png_path),
        "sample_msg": sample_msg,
        "checks": [
            "Is text visible at all? If not → verdict=NO_TEXT_VISIBLE, "
            "text_legible=false.",
            "Are letters mirrored along X (reads right-to-left or each "
            "glyph reversed)? → verdict=BACKWARD_X, text_legible=false.",
            "Are letters flipped along Y (upside down)? → verdict=FLIPPED_Y, "
            "text_legible=false.",
            "Are glyphs garbled / unrecognisable (wrong cells, fragmented "
            "beyond recognition)? → verdict=GARBLED, text_legible=false.",
            "Otherwise → verdict=PASS, text_legible=true.",
        ],
        "tolerance": [
            "Some designs rotate glyphs along a path (spiral, circle of "
            "cards). If glyph orientation MATCHES the design's local "
            "frame (e.g. tangent to the curve) and reads correctly when "
            "the head is tilted to follow the curve, that is PASS.",
            "Partial fragmentation by an intentional pixel/glitch effect "
            "is acceptable IF the underlying letter shapes (when "
            "reconstructed mentally) are upright. If you can't tell, mark "
            "NO_TEXT_VISIBLE rather than GARBLED.",
        ],
        "output_to_write": str(out_dir / "check_text_legible.agent.json"),
        "schema_expected": {
            "text_legible": "bool",
            "verdict": "one of: PASS, BACKWARD_X, FLIPPED_Y, GARBLED, NO_TEXT_VISIBLE",
            "reason": "1-2 sentence rationale citing visible glyphs/words",
        },
    }
    prompt_path.write_text(json.dumps(prompt, indent=2))

    # Stub so downstream score merge always finds a file.
    stub_path.write_text(json.dumps({
        "text_legible": None,
        "reason": "awaiting vision agent",
        "verdict": "PENDING",
    }, indent=2))


def merge(out_dir: Path) -> int:
    """Pull the agent's answer into the canonical check_text_legible.json."""
    agent_path = out_dir / "check_text_legible.agent.json"
    final_path = out_dir / "check_text_legible.json"
    if not agent_path.exists():
        print(f"warn: no agent answer at {agent_path}; leaving stub.",
              file=sys.stderr)
        return 0
    try:
        agent = json.loads(agent_path.read_text())
    except Exception as e:
        print(f"error: agent answer not parseable: {e}", file=sys.stderr)
        return 2
    out = {
        "text_legible": bool(agent.get("text_legible")),
        "reason": str(agent.get("reason", "")),
        "verdict": str(agent.get("verdict", "")),
    }
    final_path.write_text(json.dumps(out, indent=2))
    print(f"ok: merged {agent_path} -> {final_path}")
    return 0


def main(argv=None) -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("png", type=Path, help="rendered PNG path")
    ap.add_argument("--out-dir", type=Path, default=None,
                    help="dir to write prompt + stub (defaults to PNG dir)")
    ap.add_argument("--has-text-input", action="store_true",
                    help="set when the shader has a `msg` text input")
    ap.add_argument("--sample-msg", default="HELLO WORLD CONVERSATION VISUAL TEST",
                    help="the message text rendered (for the prompt context)")
    g = ap.add_mutually_exclusive_group()
    g.add_argument("--emit-prompt", action="store_true",
                   help="emit prompt + stub (default mode)")
    g.add_argument("--merge", action="store_true",
                   help="merge agent answer into final JSON")
    args = ap.parse_args(argv)

    out_dir = args.out_dir or args.png.parent
    out_dir.mkdir(parents=True, exist_ok=True)

    if args.merge:
        return merge(out_dir)
    emit_prompt(args.png, out_dir, args.has_text_input, args.sample_msg)
    print(f"ok: wrote {out_dir / 'check_text_legible.prompt.json'} "
          f"+ {out_dir / 'check_text_legible.json'}")
    return 0


if __name__ == "__main__":
    sys.exit(main())

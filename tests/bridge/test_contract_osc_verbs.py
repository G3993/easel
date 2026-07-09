#!/usr/bin/env python3
"""Producer-side OSC contract test — Easel's dispatch vs the shared fixture.

The SDK (easel-agent-sdk) sends OSC verbs to UDP :9000; the shared fixture
contracts/easel-osc-verbs.json records every verb it emits and whether Easel
has a handler (handlerStatus: implemented | missing | ffmpeg-gated). This
script statically scans src/app/Application.cpp's OSC dispatch chain
(Application::update, the `msg.address ==` if/else ladder) and fails when the
code drifts from the fixture:

  * "implemented"  -> the literal address must appear in a dispatch comparison
  * "missing"      -> the literal address must NOT appear (if a handler landed,
                      flip the fixture entry to "implemented")
  * "ffmpeg-gated" -> the dispatch arm must sit inside #ifdef HAS_FFMPEG/#endif

It also pins the saveProject serializer keys the SDK reads back from the
.easel project file (easel-agent-sdk _read_managed_layer / list_layers).

Fixture location: $EASEL_SDK_CONTRACTS_DIR, falling back to the sibling
checkout C:\\Users\\Daisy\\easel-agent-sdk\\contracts. Absent -> clean skip
(exit 0) so this repo never gains a hard dependency on the SDK checkout.

Run: python tests/bridge/test_contract_osc_verbs.py
Exit: 0 pass/skip, 1 fail.
"""

from __future__ import annotations

import json
import os
import re
import sys

DEFAULT_CONTRACTS_DIR = r"C:\Users\Daisy\easel-agent-sdk\contracts"
FIXTURE = "easel-osc-verbs.json"

# The literal JSON keys the SDK's project readback depends on, all written in
# Application::saveProject (easel-agent-sdk reads them via EASEL_PROJECT_PATH).
SERIALIZER_KEYS = [
    "managedKey",
    "sourceType",
    "sourcePath",
    "ndiStreamName",
    "visibleLayerIds",
    "showAllLayers",
    "outputDest",
    "outputMonitor",
]


class TestFail(Exception):
    pass


def repo_root() -> str:
    here = os.path.dirname(os.path.abspath(__file__))
    return os.path.dirname(os.path.dirname(here))  # tests/bridge/ -> repo root


def contracts_dir() -> str | None:
    d = os.environ.get("EASEL_SDK_CONTRACTS_DIR") or DEFAULT_CONTRACTS_DIR
    return d if os.path.isdir(d) else None


def load_source() -> list[str]:
    path = os.path.join(repo_root(), "src", "app", "Application.cpp")
    with open(path, "r", encoding="utf-8", errors="replace") as fh:
        return fh.read().splitlines()


def dispatch_lines(lines: list[str]) -> list[tuple[int, str]]:
    """(1-based line number, text) of every real OSC dispatch comparison —
    lines that compare msg.address against a literal, i.e. the if/else ladder
    in Application::update. Ignores comments/log strings elsewhere."""
    out = []
    for i, text in enumerate(lines, start=1):
        stripped = text.lstrip()
        if stripped.startswith("//"):
            continue
        if "msg.address" in text and '"' in text:
            out.append((i, text))
    return out


def ffmpeg_region_flags(lines: list[str]) -> list[bool]:
    """flags[i] == True when 1-based line i+1 sits inside an active
    `#ifdef HAS_FFMPEG` region (simple preprocessor stack; an #else flips
    the branch off)."""
    flags = []
    stack: list[bool] = []  # True == this level is the HAS_FFMPEG branch
    for text in lines:
        s = text.lstrip()
        if s.startswith("#ifdef"):
            stack.append(s.split()[1] == "HAS_FFMPEG" if len(s.split()) > 1 else False)
        elif s.startswith("#ifndef") or (s.startswith("#if") and not s.startswith("#ifdef")):
            stack.append(False)
        elif s.startswith("#else") or s.startswith("#elif"):
            if stack:
                stack[-1] = False
        elif s.startswith("#endif"):
            if stack:
                stack.pop()
        flags.append(any(stack))
    return flags


def save_project_region(lines: list[str]) -> str:
    """The body of Application::saveProject, by brace matching."""
    start = None
    for i, text in enumerate(lines):
        if "Application::saveProject" in text and "(" in text and ";" not in text:
            start = i
            break
    if start is None:
        raise TestFail("could not locate Application::saveProject in Application.cpp")
    depth = 0
    opened = False
    body: list[str] = []
    for text in lines[start:]:
        body.append(text)
        depth += text.count("{") - text.count("}")
        if "{" in text:
            opened = True
        if opened and depth <= 0:
            break
    return "\n".join(body)


def main() -> int:
    cdir = contracts_dir()
    if cdir is None:
        print("SKIP — contracts dir absent "
              "(set EASEL_SDK_CONTRACTS_DIR or check out easel-agent-sdk)")
        return 0
    fixture_path = os.path.join(cdir, FIXTURE)
    if not os.path.isfile(fixture_path):
        print(f"SKIP — fixture missing: {fixture_path}")
        return 0

    with open(fixture_path, "r", encoding="utf-8") as fh:
        fixture = json.load(fh)
    verbs = fixture["verbs"]

    lines = load_source()
    dlines = dispatch_lines(lines)
    ff_flags = ffmpeg_region_flags(lines)

    def arms_for(address: str) -> list[int]:
        needle = f'"{address}"'
        return [n for n, text in dlines if needle in text]

    failures: list[str] = []
    counts = {"implemented": 0, "missing": 0, "ffmpeg-gated": 0}

    for verb in verbs:
        addr = verb["address"]
        status = verb["handlerStatus"]
        arms = arms_for(addr)
        if status == "implemented":
            counts["implemented"] += 1
            if not arms:
                failures.append(
                    f"implemented verb {addr} has NO dispatch arm in Application.cpp")
        elif status == "missing":
            counts["missing"] += 1
            if arms:
                failures.append(
                    f"'missing' verb {addr} HAS a dispatch arm (line "
                    f"{arms[0]}) — a handler landed; flip its fixture entry "
                    f"to handlerStatus \"implemented\"")
        elif status == "ffmpeg-gated":
            counts["ffmpeg-gated"] += 1
            if not arms:
                failures.append(
                    f"ffmpeg-gated verb {addr} has NO dispatch arm in Application.cpp")
            else:
                outside = [n for n in arms if not ff_flags[n - 1]]
                if outside:
                    failures.append(
                        f"ffmpeg-gated verb {addr} dispatch arm at line "
                        f"{outside[0]} is NOT inside #ifdef HAS_FFMPEG/#endif")
        else:
            failures.append(f"{addr}: unknown handlerStatus {status!r} in fixture")

    n_verbs = len(verbs)
    verb_fail = [f for f in failures]
    print(f"verbs: {n_verbs} checked "
          f"({counts['implemented']} implemented, {counts['missing']} missing, "
          f"{counts['ffmpeg-gated']} ffmpeg-gated) — "
          f"{'OK' if not verb_fail else f'{len(verb_fail)} FAILED'}")

    # ── Serializer keys the SDK reads back ────────────────────────────────
    try:
        region = save_project_region(lines)
    except TestFail as exc:
        failures.append(str(exc))
        region = ""
    missing_keys = [k for k in SERIALIZER_KEYS if f'"{k}"' not in region]
    for k in missing_keys:
        failures.append(
            f"saveProject no longer writes \"{k}\" — the SDK project "
            f"readback (easel-agent-sdk list_layers/_read_managed_layer) reads it")
    print(f"serializer keys: {len(SERIALIZER_KEYS) - len(missing_keys)}/"
          f"{len(SERIALIZER_KEYS)} present in saveProject — "
          f"{'OK' if not missing_keys else 'MISSING: ' + ', '.join(missing_keys)}")

    if failures:
        print(f"\nFAIL — {len(failures)} contract mismatch(es):")
        for f in failures:
            print(f"  - {f}")
        return 1
    print("PASS — Application.cpp matches easel-osc-verbs.json + serializer keys")
    return 0


if __name__ == "__main__":
    sys.exit(main())

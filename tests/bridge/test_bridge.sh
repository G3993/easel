#!/usr/bin/env bash
#
# Goal-closing test harness for the Easel <-> iOS bridge.
#
# Returns 0 when the Easel-desktop side of the bridge is bulletproof:
#   - clean cmake build of Easel
#   - canonical /agent/play/publish payload decodes against Mobile's Play.swift shape
#   - every enabled-milestone validator passes
#
# Skips with exit 77 when Easel isn't running (the bridge can't be tested
# without the desktop process; SKIP keeps CI loops idle-quiet).
#
# Usage:
#     /Users/lu/easel/tests/bridge/test_bridge.sh        # default: full
#     SKIP_BUILD=1 ./test_bridge.sh                       # skip cmake rebuild

set -euo pipefail

cd "$(dirname "$0")/../.."   # -> /Users/lu/easel

PY=/Users/lu/easel-mobile/etherea-agent/.venv/bin/python
TESTS_DIR="$(pwd)/tests/bridge"

echo "[bridge-test] phase 1 — build Easel"
if [[ -z "${SKIP_BUILD:-}" ]]; then
    cmake --build build --target Easel | tail -5
else
    echo "    (skipped via SKIP_BUILD=1)"
fi

echo "[bridge-test] phase 2 — publish wire-shape"
if ! "$PY" "$TESTS_DIR/test_bridge_publish.py"; then
    rc=$?
    if [[ $rc -eq 77 ]]; then
        echo "[bridge-test] SKIP — Easel not running. Launch it and re-run."
        exit 77
    fi
    echo "[bridge-test] FAIL at publish wire-shape"
    exit "$rc"
fi

echo "[bridge-test] phase 3 — mobile-style agent actions"
if ! "$PY" "$TESTS_DIR/test_bridge_mobile_actions.py"; then
    rc=$?
    if [[ $rc -eq 77 ]]; then
        echo "[bridge-test] SKIP — mobile-actions phase couldn't reach Easel."
        exit 77
    fi
    echo "[bridge-test] FAIL at mobile-style agent actions"
    exit "$rc"
fi

echo "[bridge-test] all green"

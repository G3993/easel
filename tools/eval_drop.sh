#!/usr/bin/env bash
# eval_drop.sh — orchestrate end-to-end eval of one shader.
#
# Usage:
#   eval_drop.sh <slug>        # looks up shaders/<slug>.fs and matches reference by name
#   eval_drop.sh /path/to/shader.fs --ref /path/to/reference.jpg
#
# Outputs to /Users/lu/easel/.planning/drops/<YYYYMMDD>/<slug>/:
#   - rendered.png         (contact sheet)
#   - rendered.log         (preamble + compile log)
#   - score.json           (numeric portion; rubric is filled by the agent)
#   - score.prompt.json    (self-describing instructions for the agent's vision pass)

set -euo pipefail

REPO=/Users/lu/easel
PY=$REPO/.venv-eval/bin/python
RENDER=$REPO/tools/render_isf.py
SCORE=$REPO/tools/score_drop.py
SHADERS_DIR=$REPO/shaders
REF_DIR="/Users/lu/Documents/A-List Shaders"
DROPS_ROOT=$REPO/.planning/drops

if [[ $# -lt 1 ]]; then
    echo "usage: eval_drop.sh <slug> [--ref <reference.jpg>]" >&2
    exit 1
fi

ARG="$1"; shift || true
REF_OVERRIDE=""
while [[ $# -gt 0 ]]; do
    case "$1" in
        --ref) REF_OVERRIDE="$2"; shift 2;;
        *) echo "unknown arg: $1" >&2; exit 1;;
    esac
done

# Resolve shader path
if [[ -f "$ARG" ]]; then
    SHADER="$ARG"
    SLUG="$(basename "$ARG" .fs)"
else
    SLUG="$ARG"
    SHADER="$SHADERS_DIR/$SLUG.fs"
fi
if [[ ! -f "$SHADER" ]]; then
    echo "error: shader not found: $SHADER" >&2
    exit 2
fi

# Resolve reference
REF=""
if [[ -n "$REF_OVERRIDE" ]]; then
    REF="$REF_OVERRIDE"
else
    # Best-effort fuzzy match. Try exact slug.{jpg,png,gif}, then case-insensitive,
    # then any file containing slug as substring.
    for ext in jpg jpeg png gif; do
        cand="$REF_DIR/$SLUG.$ext"
        if [[ -f "$cand" ]]; then REF="$cand"; break; fi
    done
    if [[ -z "$REF" ]]; then
        # case-insensitive
        REF=$(find "$REF_DIR" -maxdepth 1 -type f -iname "$SLUG.*" 2>/dev/null | head -1 || true)
    fi
    if [[ -z "$REF" ]]; then
        # substring match
        REF=$(find "$REF_DIR" -maxdepth 1 -type f \( -iname "*.jpg" -o -iname "*.png" -o -iname "*.gif" \) 2>/dev/null | grep -i "$SLUG" | head -1 || true)
    fi
fi

DATE=$(date +%Y%m%d)
OUT_DIR="$DROPS_ROOT/$DATE/$SLUG"
mkdir -p "$OUT_DIR"

RENDERED="$OUT_DIR/rendered.png"
SCORE_JSON="$OUT_DIR/score.json"
TEXT_LEG_PY="$REPO/tools/check_text_legible.py"

echo "[eval_drop] slug=$SLUG"
echo "[eval_drop] shader=$SHADER"
echo "[eval_drop] reference=${REF:-<not found>}"
echo "[eval_drop] out_dir=$OUT_DIR"

# 1) Render contact sheet
"$PY" "$RENDER" "$SHADER" --out "$RENDERED"

# 1.5) Text-legibility gate: only fires if the shader declares a `msg` text
# input (the user-visible utterance hook). Cheap grep over the ISF header is
# good enough — false positives just trigger an unnecessary check.
HAS_TEXT_INPUT=0
if grep -qE '"NAME"\s*:\s*"msg"' "$SHADER"; then
    HAS_TEXT_INPUT=1
fi
if [[ $HAS_TEXT_INPUT -eq 1 ]]; then
    "$PY" "$TEXT_LEG_PY" "$RENDERED" --out-dir "$OUT_DIR" --emit-prompt \
        --has-text-input || true
else
    "$PY" "$TEXT_LEG_PY" "$RENDERED" --out-dir "$OUT_DIR" --emit-prompt || true
fi

# 2) Score (numeric portion; emits score.prompt.json)
if [[ -z "$REF" || ! -f "$REF" ]]; then
    echo "[eval_drop] WARN: no reference image matched for slug=$SLUG; emitting render-only drop." >&2
    # Still write a stub score.json so downstream tools have a uniform shape.
    cat > "$SCORE_JSON" <<JSON
{
  "slug": "$SLUG",
  "rendered": "$RENDERED",
  "reference": null,
  "shader": "$SHADER",
  "numeric": null,
  "axes": null,
  "total": null,
  "anti_patterns": {},
  "rationale": "no reference image — render-only drop",
  "numeric_only": true,
  "render_only": true
}
JSON
else
    "$PY" "$SCORE" --slug "$SLUG" --rendered "$RENDERED" --reference "$REF" \
        --shader "$SHADER" --out "$SCORE_JSON"
fi

echo ""
echo "[eval_drop] === next step for orchestrating agent ==="
echo "  Read these files:"
echo "    rendered: $RENDERED"
echo "    reference: ${REF:-<none>}"
echo "    shader source: $SHADER"
echo "    rubric: $REPO/.planning/auto-improve/RUBRIC.md"
echo "    prompt scaffold: $OUT_DIR/score.prompt.json"
if [[ $HAS_TEXT_INPUT -eq 1 ]]; then
    echo "    text-legibility prompt: $OUT_DIR/check_text_legible.prompt.json"
    echo "  Do the text-legibility vision pass and write:"
    echo "    $OUT_DIR/check_text_legible.agent.json"
    echo "  Then merge it:"
    echo "    $PY $TEXT_LEG_PY $RENDERED --out-dir $OUT_DIR --merge"
fi
echo "  Do the vision pass per RUBRIC.md and write:"
echo "    $OUT_DIR/score.rubric.json"
echo "  Then re-run score_drop.py with the same args to merge rubric into score.json:"
echo "    $PY $SCORE --slug $SLUG --rendered $RENDERED --reference ${REF:-<none>} --shader $SHADER --out $SCORE_JSON"

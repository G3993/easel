#!/usr/bin/env bash
# Picks the next shader to improve based on ~/.easel/shader_ratings.json.
# Lowest stars first; ties broken alphabetically. Skips shaders already at 5★.
# Outputs:
#   - Path to the shader source
#   - Current rating
#   - Critique log path (creates dir on first run)
# Pipe into Claude Code or open manually.

set -euo pipefail

RATINGS="$HOME/.easel/shader_ratings.json"
CRITIQUE_DIR="$HOME/.easel/shader_critiques"
mkdir -p "$CRITIQUE_DIR"

if [ ! -f "$RATINGS" ]; then
    echo "no ratings file at $RATINGS — rate some shaders in Easel first" >&2
    exit 1
fi

# Pick lowest-rated, alphabetical tiebreak
read -r SHADER STARS < <(python3 -c "
import json
d = json.load(open('$RATINGS'))
items = sorted([(v, k) for k, v in d.items() if v < 5])
if not items:
    print('', '')
else:
    print(items[0][1], items[0][0])
")

if [ -z "$SHADER" ]; then
    echo "🎉 every rated shader is at 5★ — nothing to improve"
    exit 0
fi

# Resolve full path — try both standalone and submodule, prefer standalone
for BASE in "$HOME/ShaderClaw3/shaders" "$HOME/easel/external/ShaderClaw3/shaders"; do
    if [ -f "$BASE/$SHADER" ]; then
        SHADER_PATH="$BASE/$SHADER"
        break
    fi
done

if [ -z "${SHADER_PATH:-}" ]; then
    echo "shader $SHADER referenced in ratings but file not found" >&2
    exit 1
fi

LOG="$CRITIQUE_DIR/${SHADER%.fs}.md"

echo "next: $SHADER"
echo "rating: ${STARS}★"
echo "path: $SHADER_PATH"
echo "log: $LOG"

# Worklist summary
python3 -c "
import json
d = json.load(open('$RATINGS'))
buckets = {}
for k, v in d.items(): buckets.setdefault(v, []).append(k)
total_below_5 = sum(len(v) for s, v in buckets.items() if s < 5)
print(f'worklist: {total_below_5} shaders below 5★')
for s in sorted(buckets):
    if s < 5: print(f'  {s}★: {len(buckets[s])}')
"

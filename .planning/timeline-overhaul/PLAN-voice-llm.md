# Voice V1.5 — Local LLM NLU (Qwen 2.5 0.5B via llama.cpp)

## What this does

When the deterministic `VoiceCommand` parser returns `Unknown`, route the transcript through a local LLM that produces structured JSON tool calls. Common commands stay free + instant; natural-language phrasing ("make the magritte layer slowly come in", "kill the top one", "everything to half opacity") routes to the LLM and still works.

## Decisions locked in (2026-05-01)
- **Model**: Qwen 2.5 0.5B-Instruct, Q4_K_M GGUF (~350 MB). User-chosen.
- **Runtime**: llama.cpp with Metal acceleration on Apple Silicon (~50–200 ms per command, fully on-device).
- **Source priority**: deterministic regex parser first; LLM only on `Unknown`.
- **No cloud fallback** in V1.5. Offline-first by design — shows must work without internet.

## Architecture

```
transcript
   │
   ▼
VoiceCommand::parse()  ◄── 95% of commands match here (free, ~1µs)
   │
   ▼ Unknown?
   │
   ▼
LocalNLU::resolve()
   │   • Builds context: layer names on stage, available shaders, available transitions
   │   • System prompt: "Return one JSON tool call: {kind:'fade_in', target:'fauvism', value:3.0}"
   │   • Calls llama.cpp inference with Qwen 2.5 0.5B
   │   • Parses JSON → VoiceIntent
   ▼
Application::handleVoiceIntent()
```

## Files

| New | Purpose |
|---|---|
| `src/voice/LocalNLU.h/.cpp` | Wraps llama.cpp; loads model lazily; exposes `resolve(transcript, ctx) → VoiceIntent` |
| `scripts/fetch_qwen.sh` | First-run download of `qwen2.5-0.5b-instruct-q4_k_m.gguf` into `~/.easel/models/` |
| `cmake/FetchDependencies.cmake` (additions) | `FetchContent_Declare(llama_cpp ...)` with Metal build config |
| `CMakeLists.txt` (additions) | Link llama.cpp; only on `__APPLE__` for V1.5 |

## CMake integration sketch

```cmake
set(LLAMA_BUILD_EXAMPLES OFF CACHE BOOL "" FORCE)
set(LLAMA_BUILD_TESTS    OFF CACHE BOOL "" FORCE)
set(LLAMA_BUILD_SERVER   OFF CACHE BOOL "" FORCE)
set(LLAMA_CURL           OFF CACHE BOOL "" FORCE)
set(LLAMA_METAL          ON  CACHE BOOL "" FORCE)  # Apple-only path
FetchContent_Declare(
    llama_cpp
    GIT_REPOSITORY https://github.com/ggerganov/llama.cpp.git
    GIT_TAG        b4524
    GIT_SHALLOW    TRUE
)
FetchContent_MakeAvailable(llama_cpp)

if(APPLE)
    target_link_libraries(${PROJECT_NAME} PRIVATE llama)
endif()
```

First build adds ~5 min for llama.cpp compile. Subsequent builds: cached.

## Model fetch

`scripts/fetch_qwen.sh`:
```bash
#!/usr/bin/env bash
DEST="$HOME/.easel/models/qwen2.5-0.5b-instruct-q4_k_m.gguf"
mkdir -p "$(dirname "$DEST")"
[ -f "$DEST" ] && { echo "Model already at $DEST"; exit 0; }
URL="https://huggingface.co/Qwen/Qwen2.5-0.5B-Instruct-GGUF/resolve/main/qwen2.5-0.5b-instruct-q4_k_m.gguf"
curl -L -o "$DEST" "$URL"
```

App auto-runs this on first launch if the model is missing. Show a one-time progress UI during download.

## System prompt design

```
You translate VJ voice commands into structured JSON tool calls.
Return ONE line of JSON only — no prose, no markdown.

Available intents and their parameters:
  {"kind":"play"} | {"kind":"pause"} | {"kind":"stop"} | {"kind":"toggle_loop"}
  {"kind":"seek","value":<seconds>}
  {"kind":"skip","value":<signed seconds>}
  {"kind":"fade_in","target":"<layer>","value":<seconds>}
  {"kind":"fade_out","target":"<layer>","value":<seconds>}
  {"kind":"set_opacity","target":"<layer>","value":<0..1>}
  {"kind":"show","target":"<layer>"} | {"kind":"hide","target":"<layer>"}
  {"kind":"add_shader","target":"<shader>"}
  {"kind":"transition","target":"<next layer>","secondary":"<transition>","value":<seconds>}
  {"kind":"unknown"}    ← if you cannot resolve

Layers currently on stage:
  {layers_list}

Available shaders (87 total, top-matched):
  {top_8_shaders_for_query}

Available transitions:
  {transitions_list}

User said: "{transcript}"

JSON:
```

Context lists (`layers_list`, etc.) are injected at runtime by `LocalNLU::resolve(transcript, ctx)`. Top-matched shaders/transitions filtered to the 8 best fuzzy matches against the transcript so the prompt stays short.

## Inference budget

Qwen 2.5 0.5B Q4_K_M:
- Load time: ~600 ms first call, model stays in memory after
- Inference: ~80 tokens/s on M-series Metal, ~30 tokens for our short outputs ≈ **~400 ms per command**
- Memory: ~500 MB resident

For "instant feel" we run inference on a worker thread; current intent stays "unknown" in the UI, then updates when LLM resolves. UI shows a subtle "thinking..." indicator during the gap.

## Validation

Unit tests in `tests/test_local_nlu.cpp`:
- "make the fauvism layer slowly come in" → `fade_in fauvism, value≈4.0`
- "kill the top one" → `hide <topmost layer name>`
- "everything to half opacity" → emit `set_opacity` for each layer at value=0.5 (multi-intent — V2)
- "transition to weather using dispersion" → `transition target=weather secondary=dispersion`

Track unrecognized utterances → grow the prompt examples.

## Build path (when greenlit)

1. Update `cmake/FetchDependencies.cmake` with the snippet above. First build pulls llama.cpp.
2. Add `LLAMA_METAL=ON` and link `llama` in CMakeLists.txt.
3. Write `LocalNLU.h/.cpp`. Lazy-load model in a worker thread on Application::init.
4. Write `scripts/fetch_qwen.sh`. Add a one-time check + UI prompt to download.
5. Write `Application::resolveWithLLM(transcript)` — invokes LocalNLU, parses JSON, dispatches.
6. Wire fallback in `MacSpeechRecognizer::onFinal`: parse first, LLM if Unknown, dispatch.
7. Tests + validation utterances.

Estimated effort: ~1.5 days (model integration + system prompt iteration + tests).

## What this rejects

- Cloud LLM (Anthropic / OpenAI). Offline-first means a flaky stage WiFi can't kill voice control.
- Apple Foundation Models. Requires macOS 26+; we want to ship to macOS 14+.
- Larger Qwen variants (1.5B, 3B). 0.5B is enough for short structured outputs and keeps load + memory tiny.
- Free-form chat back. The LLM emits JSON only; conversation lives outside V1.5.

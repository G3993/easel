// ShaderClaw VoxTerm display mode.
// Opens via /voxterm or ?voxterm=1 and drives curated text shader scenes from
// VoxTerm's live markdown transcript stream.
(function () {
  const params = new URLSearchParams(window.location.search);
  const enabled =
    window.location.pathname.replace(/\/+$/, "") === "/voxterm" ||
    params.has("voxterm") ||
    params.get("display") === "voxterm";

  if (!enabled) return;

  const PRESETS = [
    {
      name: "la-bloom",
      textShader: "text_la_bloom.fs",
      backgroundShader: "sonoluminescence.fs",
      opacity: 0.96,
      backgroundOpacity: 0.42,
      params: { fontFamily: 2, speed: 5.8, fadeTime: 6.5, bloom: 0.72, wobble: 0.38, textScale: 0.24, paperGrain: 0.08, edgeBurn: 0.05, foxing: 0.0 },
    },
    {
      name: "matrix",
      textShader: "text_matrix.fs",
      backgroundShader: "time_glitch_rgbfs.fs",
      opacity: 0.94,
      backgroundOpacity: 0.58,
      params: { speed: 0.72, intensity: 0.68, density: 0.76, textScale: 0.72 },
    },
    {
      name: "cascade",
      textShader: "text_cascade.fs",
      backgroundShader: "metamorphosis.fs",
      opacity: 0.88,
      backgroundOpacity: 0.5,
      params: { fontFamily: 3, speed: 0.54, intensity: 0.62, density: 0.56, textScale: 0.92, oscSpeed: 0.75, oscAmount: 0.035, oscSpread: 0.9 },
    },
    {
      name: "digifade",
      textShader: "text_digifade.fs",
      backgroundShader: "ether.fs",
      opacity: 0.92,
      backgroundOpacity: 0.52,
      params: { preset: 1, fontFamily: 0, speed: 0.78, intensity: 0.74, density: 0.55, textScale: 0.94 },
    },
    {
      name: "coil",
      textShader: "text_coil.fs",
      backgroundShader: "soph_orb.fs",
      opacity: 0.9,
      backgroundOpacity: 0.54,
      params: { preset: 3, fontFamily: 0, speed: 0.46, intensity: 0.58, density: 0.48, textScale: 0.78 },
    },
    {
      name: "spacy",
      textShader: "text_spacy.fs",
      backgroundShader: "laser_labyrinth.fs",
      opacity: 0.9,
      backgroundOpacity: 0.5,
      params: { preset: 3, fontFamily: 3, speed: 0.52, intensity: 0.68, density: 0.64, textScale: 0.82, oscSpeed: 0.52, oscAmount: 0.025 },
    },
  ];

  const PALETTES = [
    [[0.98, 0.95, 0.68, 1], [0.08, 0.02, 0.09, 1], [0.17, 0.68, 1.0, 1]],
    [[0.46, 1.0, 0.72, 1], [0.0, 0.02, 0.01, 1], [0.98, 0.26, 0.48, 1]],
    [[0.85, 0.72, 1.0, 1], [0.02, 0.01, 0.04, 1], [1.0, 0.78, 0.24, 1]],
    [[1.0, 0.32, 0.38, 1], [0.04, 0.01, 0.015, 1], [0.24, 0.88, 1.0, 1]],
    [[0.78, 0.98, 1.0, 1], [0.0, 0.035, 0.055, 1], [1.0, 0.42, 0.86, 1]],
    [[0.12, 0.08, 0.05, 1], [0.9, 0.83, 0.66, 1], [0.52, 0.13, 0.08, 1]],
  ];

  let currentPreset = -1;
  let transcriptCount = 0;
  let latestDisplayText = "WAITING FOR VOXTERM";
  let source = null;
  let loadingPreset = false;
  let pendingPreset = null;
  let lastSessionKey = "";

  function injectDisplayCss() {
    document.body.classList.add("voxterm-display-mode");
    const style = document.createElement("style");
    style.textContent = `
      body.voxterm-display-mode { background: #000; cursor: none; }
      body.voxterm-display-mode .sc3-header,
      body.voxterm-display-mode .sc3-toolbar-strip,
      body.voxterm-display-mode .sc3-properties,
      body.voxterm-display-mode .sc3-sources,
      body.voxterm-display-mode .sc3-chat-bar,
      body.voxterm-display-mode .sc3-chat-history,
      body.voxterm-display-mode .sc3-mobile-inputs,
      body.voxterm-display-mode .sc3-mobile-toggle,
      body.voxterm-display-mode .canvas-controls,
      body.voxterm-display-mode #error-bar,
      body.voxterm-display-mode #debug-overlay {
        display: none !important;
      }
      body.voxterm-display-mode #app,
      body.voxterm-display-mode #main,
      body.voxterm-display-mode #preview {
        position: fixed !important;
        inset: 0 !important;
        width: 100vw !important;
        height: 100vh !important;
        height: 100dvh !important;
        border-radius: 0 !important;
      }
      body.voxterm-display-mode #gl-canvas {
        filter: saturate(1.12) contrast(1.04);
      }
      #voxterm-display-hud {
        position: fixed;
        left: 18px;
        bottom: 16px;
        z-index: 100;
        display: flex;
        align-items: center;
        gap: 8px;
        pointer-events: none;
        opacity: 0.62;
        color: rgba(255,255,255,0.72);
        font: 600 10px/1.1 Inter, system-ui, sans-serif;
        letter-spacing: 0.12em;
        text-transform: uppercase;
        transition: opacity 240ms ease;
      }
      #voxterm-display-hud .dot {
        width: 6px;
        height: 6px;
        border-radius: 999px;
        background: #888;
        box-shadow: 0 0 16px currentColor;
      }
      #voxterm-display-hud.live .dot { background: #56ff9c; color: #56ff9c; }
      #voxterm-display-hud.waiting .dot { background: #ffd166; color: #ffd166; }
      #voxterm-display-hud.error .dot { background: #ff4b66; color: #ff4b66; }
    `;
    document.head.appendChild(style);

    const hud = document.createElement("div");
    hud.id = "voxterm-display-hud";
    hud.className = "waiting";
    hud.innerHTML = '<span class="dot"></span><span class="label">VOXTERM</span>';
    document.body.appendChild(hud);
  }

  function setHud(state, text) {
    const hud = document.getElementById("voxterm-display-hud");
    if (!hud) return;
    hud.className = state || "";
    const label = hud.querySelector(".label");
    if (label) label.textContent = text || "VOXTERM";
  }

  function sleep(ms) {
    return new Promise((resolve) => setTimeout(resolve, ms));
  }

  async function waitFor(fn, timeoutMs = 25000) {
    const started = performance.now();
    while (performance.now() - started < timeoutMs) {
      const value = fn();
      if (value) return value;
      await sleep(80);
    }
    throw new Error("Timed out waiting for ShaderClaw");
  }

  function hashString(value) {
    let h = 2166136261;
    const text = String(value || "");
    for (let i = 0; i < text.length; i++) {
      h ^= text.charCodeAt(i);
      h = Math.imul(h, 16777619);
    }
    return h >>> 0;
  }

  function colorFor(index, role = 0) {
    const palette = PALETTES[Math.abs(index) % PALETTES.length];
    return palette[Math.abs(role) % palette.length].slice();
  }

  function textMaxLength() {
    const layer = window.shaderClaw?.getLayer?.("text");
    const msg = (layer?.inputs || []).find((input) => input.NAME === "msg" && input.TYPE === "text");
    return Math.max(12, Math.min(96, msg?.MAX_LENGTH || 48));
  }

  function phraseForShader(text) {
    const maxLen = textMaxLength();
    const cleaned = String(text || "")
      .normalize("NFKD")
      .replace(/[^\w\s'-]/g, " ")
      .replace(/_/g, " ")
      .replace(/\s+/g, " ")
      .trim();
    if (!cleaned) return "LISTENING";
    const words = cleaned.split(" ");
    let out = "";
    for (let i = words.length - 1; i >= 0; i--) {
      const next = out ? `${words[i]} ${out}` : words[i];
      if (next.length > maxLen) break;
      out = next;
    }
    return (out || cleaned.slice(-maxLen)).toUpperCase();
  }

  function setTextBar(text) {
    latestDisplayText = phraseForShader(text);
    const input = document.getElementById("text-msg-input");
    if (input) {
      input.value = latestDisplayText;
      input.dispatchEvent(new Event("input", { bubbles: true }));
    }
    const transcript = document.getElementById("voice-transcript");
    if (transcript) transcript.value = latestDisplayText;
  }

  function setLayerParams(layerId, values) {
    const api = window.shaderClaw;
    const layer = api?.getLayer?.(layerId);
    if (!layer) return;
    layer.inputValues = layer.inputValues || {};
    for (const [name, value] of Object.entries(values || {})) {
      layer.inputValues[name] = Array.isArray(value) ? value.slice() : value;
      api.updateControlUI?.(name, layer.inputValues[name], layerId);
    }
    if (layerId === "text") {
      const picker = document.querySelector('.sc3-layer-color-picker[data-layer="text"]');
      if (picker && Array.isArray(values.textColor)) {
        picker.value = rgbToHex(values.textColor);
      }
    }
  }

  function rgbToHex(color) {
    const part = (n) => Math.round(Math.max(0, Math.min(1, n)) * 255).toString(16).padStart(2, "0");
    return `#${part(color[0])}${part(color[1])}${part(color[2])}`;
  }

  async function waitForLayerIdle(layerId) {
    await waitFor(() => {
      const layer = window.shaderClaw?.getLayer?.(layerId);
      return layer && !layer._pendingCompile && layer.program;
    }, 20000);
    await sleep(40);
  }

  async function loadPreset(index, reason = "") {
    if (loadingPreset) {
      pendingPreset = index;
      return;
    }
    loadingPreset = true;
    pendingPreset = null;

    try {
      const api = window.shaderClaw;
      const preset = PRESETS[((index % PRESETS.length) + PRESETS.length) % PRESETS.length];
      currentPreset = PRESETS.indexOf(preset);
      setHud("live", preset.name.toUpperCase());

      await api.loadShaderFile("shader", "shaders", preset.backgroundShader);
      await waitForLayerIdle("shader");
      api.setLayerVisibility("shader", true);
      api.setLayerOpacity("shader", preset.backgroundOpacity);

      await api.loadShaderFile("text", "shaders", preset.textShader);
      await waitForLayerIdle("text");
      api.setLayerVisibility("text", true);
      api.setLayerOpacity("text", preset.opacity);

      const seed = hashString(`${lastSessionKey}:${transcriptCount}:${preset.name}:${reason}`);
      setLayerParams("text", {
        ...preset.params,
        transparentBg: true,
        textColor: colorFor(seed, 0),
        bgColor: colorFor(seed, 1),
      });
      setTextBar(latestDisplayText);
      const textSelect = document.querySelector('.layer-shader-select[data-layer="text"]');
      if (textSelect) textSelect.value = preset.textShader;
      const shaderSelect = document.querySelector('.layer-shader-select[data-layer="shader"]');
      if (shaderSelect) shaderSelect.value = preset.backgroundShader;
    } catch (error) {
      setHud("error", "VOXTERM ERROR");
      console.warn("[VoxTerm Display]", error);
    } finally {
      loadingPreset = false;
      if (pendingPreset != null && pendingPreset !== currentPreset) {
        const next = pendingPreset;
        pendingPreset = null;
        loadPreset(next, "queued");
      }
    }
  }

  function pulseLayers(text) {
    const seed = hashString(`${text}:${transcriptCount}:${lastSessionKey}`);
    setLayerParams("text", {
      textColor: colorFor(seed, 0),
      bgColor: colorFor(seed, 1),
    });

    const textLayer = window.shaderClaw?.getLayer?.("text");
    const shaderLayer = window.shaderClaw?.getLayer?.("shader");
    if (textLayer) textLayer._voiceGlitch = 1.0;
    if (shaderLayer) shaderLayer._voiceGlitch = 0.65;
  }

  function onTranscript(segment) {
    transcriptCount += 1;
    setHud("live", segment.speaker ? segment.speaker.toUpperCase().slice(0, 18) : "VOXTERM LIVE");
    setTextBar(segment.text);
    pulseLayers(segment.text);
    if (transcriptCount > 1 && transcriptCount % 4 === 0) {
      loadPreset(currentPreset + 1, "transcript");
    }
  }

  function onSnapshot(snapshot) {
    lastSessionKey = snapshot.fileName || snapshot.file || "voxterm";
    if (snapshot.latest?.text) setTextBar(snapshot.latest.text);
    else if (snapshot.transcript) setTextBar(snapshot.transcript);
    setHud(snapshot.file ? "live" : "waiting", snapshot.file ? "VOXTERM LIVE" : "WAITING");
    if (currentPreset < 0) {
      loadPreset(hashString(lastSessionKey) % PRESETS.length, "snapshot");
    }
  }

  async function connectEvents() {
    const status = await fetch("/api/voxterm/status", { cache: "no-store" }).then((res) => res.json()).catch(() => null);
    if (status) onSnapshot(status);

    source = new EventSource("/api/voxterm/events");
    source.addEventListener("snapshot", (event) => onSnapshot(JSON.parse(event.data)));
    source.addEventListener("transcript", (event) => onTranscript(JSON.parse(event.data)));
    source.addEventListener("waiting", () => setHud("waiting", "WAITING"));
    source.addEventListener("error", () => {
      if (source?.readyState === EventSource.CLOSED) setHud("error", "VOXTERM OFFLINE");
    });
  }

  function startDecayLoop() {
    setInterval(() => {
      for (const id of ["text", "shader"]) {
        const layer = window.shaderClaw?.getLayer?.(id);
        if (!layer || !layer._voiceGlitch) continue;
        layer._voiceGlitch *= 0.88;
        if (layer._voiceGlitch < 0.01) layer._voiceGlitch = 0;
      }
    }, 50);
  }

  function installKeys() {
    window.addEventListener("keydown", (event) => {
      if (event.repeat) return;
      if (event.key === " " || event.key === "Tab") {
        event.preventDefault();
        loadPreset(currentPreset + 1, "key");
      } else if (event.key.toLowerCase() === "f") {
        const root = document.getElementById("preview") || document.documentElement;
        if (!document.fullscreenElement) root.requestFullscreen?.();
        else document.exitFullscreen?.();
      } else if (event.key === "Escape" && source) {
        source.close();
        source = null;
        setHud("waiting", "PAUSED");
      }
    });
  }

  async function init() {
    injectDisplayCss();
    setTextBar(latestDisplayText);
    await waitFor(() => window.shaderClaw?.getLayer?.("text")?.program);
    await loadPreset(Math.floor(Math.random() * PRESETS.length), "initial");
    await connectEvents();
    startDecayLoop();
    installKeys();
    document.title = "VoxTerm Display - ShaderClaw";
  }

  init().catch((error) => {
    setHud("error", "VOXTERM ERROR");
    console.warn("[VoxTerm Display]", error);
  });
})();

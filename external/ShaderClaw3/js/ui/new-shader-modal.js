// New Shader Modal — entry point for creating an interactive shader.
// Captures: prompt + reference image/SVG + selected live inputs + intent.
// Sends an extended `chat` action over WS so server can bake input wiring
// into the generated ISF shader.

const LIVE_INPUTS = [
  { id: 'mouse',  label: 'Mouse',  hint: 'cursor position, click' },
  { id: 'audio',  label: 'Audio',  hint: 'mic/system audio FFT + level' },
  { id: 'touch',  label: 'Touch',  hint: 'multi-touch points' },
  { id: 'camera', label: 'Camera', hint: 'webcam frame as texture' },
  { id: 'voice',  label: 'Voice',  hint: 'voice command + level' },
];

const INPUT_ICONS = {
  mouse:  '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><rect x="6" y="3" width="12" height="18" rx="6"/><line x1="12" y1="7" x2="12" y2="11"/></svg>',
  audio:  '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M12 1a3 3 0 00-3 3v8a3 3 0 006 0V4a3 3 0 00-3-3z"/><path d="M19 10v2a7 7 0 01-14 0v-2"/><line x1="12" y1="19" x2="12" y2="23"/></svg>',
  touch:  '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M9 11V6a3 3 0 016 0v5"/><path d="M9 11l-3 3v3a4 4 0 004 4h4a4 4 0 004-4v-5a3 3 0 00-3-3"/></svg>',
  camera: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M23 7l-7 5 7 5V7z"/><rect x="1" y="5" width="15" height="14" rx="2"/></svg>',
  voice:  '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><path d="M3 18v-6a9 9 0 0118 0v6"/><path d="M21 19a2 2 0 01-2 2h-1v-6h3zM3 19a2 2 0 002 2h1v-6H3z"/></svg>',
};

let modalEl = null;
let state = null;

export function openNewShaderModal() {
  if (modalEl) return; // already open
  state = {
    prompt: '',
    referenceData: null,    // dataURL
    referenceKind: 'image', // 'image' | 'svg'
    selectedInputs: new Set(),
    intent: '',
    count: 1,
  };
  modalEl = build();
  document.body.appendChild(modalEl);
  requestAnimationFrame(() => modalEl.classList.add('is-open'));
  modalEl.querySelector('.nsm-prompt').focus();
}

function close() {
  if (!modalEl) return;
  modalEl.classList.remove('is-open');
  const el = modalEl;
  modalEl = null;
  setTimeout(() => el.remove(), 200);
}

function build() {
  const root = document.createElement('div');
  root.className = 'nsm-overlay';
  root.innerHTML = `
    <div class="nsm-backdrop"></div>
    <div class="nsm-dialog" role="dialog" aria-label="New shader">
      <button class="nsm-close" aria-label="Close">
        <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2"><line x1="6" y1="6" x2="18" y2="18"/><line x1="6" y1="18" x2="18" y2="6"/></svg>
      </button>

      <div class="nsm-card nsm-prompt-card">
        <textarea class="nsm-prompt" placeholder="Soft particles drifting like dust in sunlight, reacting gently to sound…" rows="4"></textarea>
        <button class="nsm-spark" title="Suggest a prompt">
          <svg viewBox="0 0 24 24" width="14" height="14" fill="currentColor"><path d="M12 2l1.8 5.5L19 9l-5.2 1.5L12 16l-1.8-5.5L5 9l5.2-1.5L12 2z"/></svg>
        </button>
      </div>

      <div class="nsm-row">
        <div class="nsm-card nsm-ref-card">
          <div class="nsm-card-title">Add a reference</div>
          <label class="nsm-drop">
            <input type="file" accept="image/*,.svg" hidden>
            <div class="nsm-drop-empty">
              <svg viewBox="0 0 24 24" width="22" height="22" fill="none" stroke="currentColor" stroke-width="1.4"><rect x="3" y="3" width="18" height="18" rx="2"/><circle cx="9" cy="9" r="2"/><path d="M21 15l-5-5-9 9"/></svg>
              <span>Drop or click</span>
            </div>
            <img class="nsm-drop-preview" alt=""/>
          </label>
          <div class="nsm-toggle">
            <button class="nsm-toggle-btn is-active" data-kind="image">Image</button>
            <button class="nsm-toggle-btn" data-kind="svg">SVG</button>
          </div>
        </div>

        <div class="nsm-card nsm-inputs-card">
          <div class="nsm-card-title">Live inputs <span class="nsm-card-sub">interactive sources</span></div>
          <div class="nsm-chips">
            ${LIVE_INPUTS.map(i => `
              <button class="nsm-chip" data-input="${i.id}" title="${i.hint}">
                <span class="nsm-chip-icon">${INPUT_ICONS[i.id]}</span>
                <span class="nsm-chip-label">${i.label}</span>
              </button>
            `).join('')}
          </div>
          <textarea class="nsm-intent" placeholder="how should they react? e.g. audio bass → particle scale, mouse → cluster" rows="3"></textarea>
        </div>
      </div>

      <div class="nsm-actions">
        <div class="nsm-stepper">
          <button class="nsm-step" data-dir="-1" aria-label="Decrease">
            <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2"><line x1="5" y1="12" x2="19" y2="12"/></svg>
          </button>
          <span class="nsm-count">1</span>
          <button class="nsm-step" data-dir="1" aria-label="Increase">
            <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
          </button>
        </div>
        <button class="nsm-create" disabled>
          <span>Create New Shader</span>
          <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="5" y1="12" x2="19" y2="12"/><polyline points="12 5 19 12 12 19"/></svg>
        </button>
      </div>

      <div class="nsm-status">
        <span class="nsm-code-icon">
          <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2"><polyline points="16 18 22 12 16 6"/><polyline points="8 6 2 12 8 18"/></svg>
        </span>
        <span class="nsm-status-dot"></span>
      </div>
    </div>
  `;

  wireEvents(root);
  return root;
}

function wireEvents(root) {
  // Close
  root.querySelector('.nsm-close').addEventListener('click', close);
  root.querySelector('.nsm-backdrop').addEventListener('click', close);
  root.addEventListener('keydown', (e) => { if (e.key === 'Escape') close(); });

  // Prompt
  const prompt = root.querySelector('.nsm-prompt');
  const create = root.querySelector('.nsm-create');
  const refresh = () => {
    state.prompt = prompt.value.trim();
    create.disabled = state.prompt.length < 3;
  };
  prompt.addEventListener('input', refresh);
  prompt.addEventListener('keydown', (e) => {
    if (e.key === 'Enter' && (e.metaKey || e.ctrlKey) && !create.disabled) {
      e.preventDefault();
      submit();
    }
  });

  // Reference upload
  const dropLabel = root.querySelector('.nsm-drop');
  const fileInput = dropLabel.querySelector('input[type=file]');
  const preview = root.querySelector('.nsm-drop-preview');
  const empty = root.querySelector('.nsm-drop-empty');
  const onFile = (file) => {
    if (!file) return;
    const reader = new FileReader();
    reader.onload = () => {
      state.referenceData = reader.result;
      preview.src = reader.result;
      preview.style.display = 'block';
      empty.style.display = 'none';
      // Auto-detect kind
      if (file.type === 'image/svg+xml' || /\.svg$/i.test(file.name)) {
        setKind('svg');
      } else {
        setKind('image');
      }
    };
    reader.readAsDataURL(file);
  };
  fileInput.addEventListener('change', () => onFile(fileInput.files[0]));
  dropLabel.addEventListener('dragover', (e) => { e.preventDefault(); dropLabel.classList.add('is-drag'); });
  dropLabel.addEventListener('dragleave', () => dropLabel.classList.remove('is-drag'));
  dropLabel.addEventListener('drop', (e) => {
    e.preventDefault();
    dropLabel.classList.remove('is-drag');
    onFile(e.dataTransfer.files[0]);
  });

  // Image / SVG toggle
  const toggleBtns = root.querySelectorAll('.nsm-toggle-btn');
  function setKind(kind) {
    state.referenceKind = kind;
    toggleBtns.forEach(b => b.classList.toggle('is-active', b.dataset.kind === kind));
  }
  toggleBtns.forEach(b => b.addEventListener('click', () => setKind(b.dataset.kind)));

  // Live input chips
  root.querySelectorAll('.nsm-chip').forEach(chip => {
    chip.addEventListener('click', () => {
      const id = chip.dataset.input;
      if (state.selectedInputs.has(id)) {
        state.selectedInputs.delete(id);
        chip.classList.remove('is-selected');
      } else {
        state.selectedInputs.add(id);
        chip.classList.add('is-selected');
      }
    });
  });

  // Intent
  root.querySelector('.nsm-intent').addEventListener('input', (e) => {
    state.intent = e.target.value.trim();
  });

  // Stepper
  const countEl = root.querySelector('.nsm-count');
  root.querySelectorAll('.nsm-step').forEach(b => {
    b.addEventListener('click', () => {
      const dir = parseInt(b.dataset.dir, 10);
      state.count = Math.max(1, Math.min(4, state.count + dir));
      countEl.textContent = state.count;
    });
  });

  // Submit
  create.addEventListener('click', submit);
}

function submit() {
  if (!state.prompt) return;
  const inputs = Array.from(state.selectedInputs);

  // Compose a richer prompt that bakes the live-input declaration into the
  // shader generation. The server-side system prompt also receives the
  // structured `liveInputs` array and adds the matching ISF input wiring.
  let composed = state.prompt;
  if (inputs.length) {
    composed += `\n\nThis shader must be interactive via these live inputs: ${inputs.join(', ')}.`;
    if (state.intent) composed += `\nMapping: ${state.intent}`;
  }

  const payload = {
    action: 'chat',
    chatId: 'new-shader-' + Date.now(),
    message: composed,
    liveInputs: inputs,
    intent: state.intent,
  };
  if (state.referenceData) payload.referenceImage = state.referenceData;

  // Find the existing WS the rest of the app uses. js/app.js stores it in
  // module scope as _ndiWs but also forwards chat events to a window event.
  const ws = window._ndiWs || window.shaderClawWS;
  if (ws && ws.readyState === WebSocket.OPEN) {
    ws.send(JSON.stringify(payload));
    close();
  } else {
    // Fallback: dispatch a custom event so the host app can route the send.
    window.dispatchEvent(new CustomEvent('sc3-new-shader-submit', { detail: payload }));
    close();
  }
}

// Auto-bind to any element with [data-action="new-shader"]
document.addEventListener('click', (e) => {
  const t = e.target.closest('[data-action="new-shader"]');
  if (t) {
    e.preventDefault();
    openNewShaderModal();
  }
});

window.openNewShaderModal = openNewShaderModal;

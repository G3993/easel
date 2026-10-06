// ============================================================
// ShaderClaw — Universal Movement Engine
// Drives mousePos/mouseDelta with procedural patterns,
// audio reactivity, and body tracking inputs.
// ============================================================

const MOVE_PATTERNS = [
  { name: 'None',       key: 'none' },
  { name: 'Swirl',      key: 'swirl' },
  { name: 'Pulse',      key: 'pulse' },
  { name: 'Wave',       key: 'wave' },
  { name: 'Wander',     key: 'wander' },
  { name: 'Center Out', key: 'centerOut' },
  { name: 'Orbit',      key: 'orbit' },
  { name: 'Bounce',     key: 'bounce' },
];

// Per-layer movement state
function createMovementState() {
  return {
    pattern: 'none',      // MOVE_PATTERNS key
    speed: 0.5,           // 0-1 temporal speed
    intensity: 0.5,       // 0-1 displacement magnitude
    audioMix: 0.0,        // 0-1 how much audio modulates movement
    audioBand: 'bass',    // which band drives audio modulation (bass/mid/high/level)
    manualMix: 1.0,       // 0-1 how much mouse/hand input passes through
    enabled: false,       // master toggle

    // Internal state (not user-facing)
    _phase: 0,
    _wanderX: 0.5,
    _wanderY: 0.5,
    _wanderVx: 0,
    _wanderVy: 0,
    _bounceX: 0.3,
    _bounceY: 0.7,
    _bounceVx: 0.4,
    _bounceVy: 0.3,
    _prevPatternPos: [0.5, 0.5],
  };
}

// Compute movement for one frame. Returns { pos: [x,y], delta: [dx,dy], active: float }
// pos/delta are in normalized 0-1 space
function updateMovement(state, dt, audioState) {
  if (!state.enabled || state.pattern === 'none') {
    state._prevPatternPos = [0.5, 0.5];
    return null;
  }

  const speed = state.speed * 2.0; // scale to useful range
  state._phase += dt * speed;
  const t = state._phase;
  const intensity = state.intensity;

  // Audio modulation factor
  let audioMod = 0;
  if (state.audioMix > 0 && audioState) {
    const band = state.audioBand;
    if (band === 'bass') audioMod = audioState.bass || 0;
    else if (band === 'mid') audioMod = audioState.mid || 0;
    else if (band === 'high') audioMod = audioState.high || 0;
    else audioMod = audioState.level || 0;
  }
  const audioScale = 1.0 + audioMod * state.audioMix * 2.0;

  let px = 0.5, py = 0.5;

  switch (state.pattern) {
    case 'swirl': {
      const r = 0.25 * intensity * audioScale;
      px = 0.5 + Math.cos(t * 1.3) * r;
      py = 0.5 + Math.sin(t * 1.7) * r;
      break;
    }
    case 'pulse': {
      // Radial pulse from center
      const r = 0.3 * intensity * (0.5 + 0.5 * Math.sin(t * 2.0)) * audioScale;
      const angle = t * 0.7;
      px = 0.5 + Math.cos(angle) * r;
      py = 0.5 + Math.sin(angle) * r;
      break;
    }
    case 'wave': {
      // Horizontal sweep with vertical sine wave
      const sweep = (t * 0.3) % 1.0;
      px = sweep;
      py = 0.5 + Math.sin(t * 3.0) * 0.2 * intensity * audioScale;
      break;
    }
    case 'wander': {
      // Brownian-like smooth wandering
      const drift = 0.3 * intensity * dt * speed;
      state._wanderVx += (Math.random() - 0.5) * drift;
      state._wanderVy += (Math.random() - 0.5) * drift;
      // Damping
      state._wanderVx *= 0.98;
      state._wanderVy *= 0.98;
      // Audio kick
      if (audioMod > 0.5) {
        state._wanderVx += (Math.random() - 0.5) * audioMod * state.audioMix * 0.05;
        state._wanderVy += (Math.random() - 0.5) * audioMod * state.audioMix * 0.05;
      }
      state._wanderX += state._wanderVx;
      state._wanderY += state._wanderVy;
      // Soft bounds
      if (state._wanderX < 0.05) state._wanderVx += 0.01;
      if (state._wanderX > 0.95) state._wanderVx -= 0.01;
      if (state._wanderY < 0.05) state._wanderVy += 0.01;
      if (state._wanderY > 0.95) state._wanderVy -= 0.01;
      state._wanderX = Math.max(0, Math.min(1, state._wanderX));
      state._wanderY = Math.max(0, Math.min(1, state._wanderY));
      px = state._wanderX;
      py = state._wanderY;
      break;
    }
    case 'centerOut': {
      // Expand from center then snap back, driven by audio
      const cycle = (t * 0.5) % 1.0;
      const expand = Math.pow(cycle, 0.5) * intensity * audioScale;
      const angle = t * 2.3 + Math.sin(t * 0.7) * 1.5;
      px = 0.5 + Math.cos(angle) * expand * 0.4;
      py = 0.5 + Math.sin(angle) * expand * 0.4;
      break;
    }
    case 'orbit': {
      // Smooth figure-eight orbit
      const r = 0.3 * intensity * audioScale;
      px = 0.5 + Math.sin(t) * r;
      py = 0.5 + Math.sin(t * 2.0) * r * 0.5;
      break;
    }
    case 'bounce': {
      // Physics-like bouncing point
      state._bounceVy -= 0.8 * dt * speed; // gravity
      state._bounceX += state._bounceVx * dt * speed * intensity;
      state._bounceY += state._bounceVy * dt * speed * intensity;
      // Audio impulse
      if (audioMod > 0.6) {
        state._bounceVy += audioMod * state.audioMix * 0.3;
      }
      // Wall bounce
      if (state._bounceX < 0.05 || state._bounceX > 0.95) {
        state._bounceVx *= -0.9;
        state._bounceX = Math.max(0.05, Math.min(0.95, state._bounceX));
      }
      if (state._bounceY < 0.05) {
        state._bounceVy = Math.abs(state._bounceVy) * 0.85;
        state._bounceY = 0.05;
      }
      if (state._bounceY > 0.95) {
        state._bounceVy *= -0.7;
        state._bounceY = 0.95;
      }
      px = state._bounceX;
      py = state._bounceY;
      break;
    }
  }

  // Compute delta from previous frame
  const dx = px - state._prevPatternPos[0];
  const dy = py - state._prevPatternPos[1];
  state._prevPatternPos = [px, py];

  return { pos: [px, py], delta: [dx, dy], active: 1.0 };
}

// Blend movement engine output with manual input (mouse/hand)
// Returns final { pos, delta, down } to send to shader
function blendMovement(movement, manualPos, manualDelta, manualDown, manualMix) {
  if (!movement) return { pos: manualPos, delta: manualDelta, down: manualDown };

  const mm = manualMix;
  const pm = 1.0 - mm;

  return {
    pos: [
      manualPos[0] * mm + movement.pos[0] * pm,
      manualPos[1] * mm + movement.pos[1] * pm,
    ],
    delta: [
      manualDelta[0] * mm + movement.delta[0] * pm,
      manualDelta[1] * mm + movement.delta[1] * pm,
    ],
    // Movement patterns count as "active" for shaders that check mouseDown
    down: Math.max(manualDown, movement.active * (1.0 - mm)),
  };
}

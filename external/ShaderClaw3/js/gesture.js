// ============================================================
// ShaderClaw — Gesture Processor + MediaPipe Manager
// ============================================================

class GestureProcessor {
  constructor() {
    this.derived = {};
    this.settled = true;
    this._smoothing = 0.15; // EMA factor
    this._active = false;
  }

  update(mp) {
    if (!mp || !mp.active) {
      // Ease out all signals toward 0
      let allZero = true;
      for (const key in this.derived) {
        this.derived[key] *= 0.92;
        if (this.derived[key] > 0.001) allZero = false;
        else this.derived[key] = 0;
      }
      this.settled = allZero;
      this._active = false;
      return;
    }
    this._active = true;
    this.settled = false;
    const a = this._smoothing;

    // Hand signals
    if (mp.modes.hand && mp.handCount > 0) {
      const hl = mp._handLandmarks;
      if (hl && hl.length >= 21) {
        // Pinch distance (thumb tip to index tip)
        const thumb = hl[4], index = hl[8];
        if (thumb && index) {
          const dx = thumb.x - index.x, dy = thumb.y - index.y;
          const pinch = Math.sqrt(dx * dx + dy * dy);
          this._smooth('pinchDist', Math.min(1, pinch * 5));
          this._smooth('pinchHold', pinch < 0.05 ? 1 : 0);
        }
        // Grip strength (avg curl of all fingers)
        const tips = [4, 8, 12, 16, 20];
        const mcps = [2, 5, 9, 13, 17];
        let gripSum = 0;
        for (let i = 0; i < 5; i++) {
          const tip = hl[tips[i]], mcp = hl[mcps[i]];
          if (tip && mcp) {
            const d = Math.sqrt((tip.x - mcp.x) ** 2 + (tip.y - mcp.y) ** 2);
            gripSum += Math.max(0, 1 - d * 8);
          }
        }
        this._smooth('gripStrength', gripSum / 5);
        // Finger spread
        let spread = 0;
        for (let i = 0; i < 4; i++) {
          const a = hl[tips[i]], b = hl[tips[i + 1]];
          if (a && b) spread += Math.sqrt((a.x - b.x) ** 2 + (a.y - b.y) ** 2);
        }
        this._smooth('fingerSpread', Math.min(1, spread * 3));
        // Hand angle
        const wrist = hl[0], middle = hl[9];
        if (wrist && middle) {
          const angle = Math.atan2(middle.y - wrist.y, middle.x - wrist.x);
          this._smooth('handAngle', (angle / Math.PI) * 0.5 + 0.5);
        }
        // Individual finger curls
        const fingerNames = ['thumbCurl', 'indexCurl', 'middleCurl', 'ringCurl', 'pinkyCurl'];
        for (let i = 0; i < 5; i++) {
          const tip = hl[tips[i]], mcp = hl[mcps[i]];
          if (tip && mcp) {
            const d = Math.sqrt((tip.x - mcp.x) ** 2 + (tip.y - mcp.y) ** 2);
            this._smooth(fingerNames[i], Math.max(0, 1 - d * 8));
          }
        }
      }
    }

    // Face signals
    if (mp.modes.face && mp._faceLandmarks && mp._faceLandmarks.length > 300) {
      const fl = mp._faceLandmarks;
      // Head orientation (simplified)
      const nose = fl[1], leftEar = fl[234], rightEar = fl[454], chin = fl[152], forehead = fl[10];
      if (nose && leftEar && rightEar) {
        const yaw = (nose.x - (leftEar.x + rightEar.x) / 2) * 4;
        this._smooth('headYaw', yaw * 0.5 + 0.5);
      }
      if (nose && chin && forehead) {
        const pitch = (nose.y - (forehead.y + chin.y) / 2) * 4;
        this._smooth('headPitch', pitch * 0.5 + 0.5);
      }
      if (leftEar && rightEar) {
        const roll = Math.atan2(rightEar.y - leftEar.y, rightEar.x - leftEar.x);
        this._smooth('headRoll', roll / Math.PI * 0.5 + 0.5);
      }
      // Mouth open
      const upperLip = fl[13], lowerLip = fl[14];
      if (upperLip && lowerLip) {
        this._smooth('mouthOpen', Math.min(1, Math.abs(upperLip.y - lowerLip.y) * 15));
      }
      // Blinks
      const leftEyeTop = fl[159], leftEyeBot = fl[145];
      const rightEyeTop = fl[386], rightEyeBot = fl[374];
      if (leftEyeTop && leftEyeBot) {
        this._smooth('leftBlink', Math.max(0, 1 - Math.abs(leftEyeTop.y - leftEyeBot.y) * 40));
      }
      if (rightEyeTop && rightEyeBot) {
        this._smooth('rightBlink', Math.max(0, 1 - Math.abs(rightEyeTop.y - rightEyeBot.y) * 40));
      }
      // Eyebrow raise
      const leftBrow = fl[70], rightBrow = fl[300];
      if (leftBrow && rightBrow && forehead) {
        const browH = ((leftBrow.y + rightBrow.y) / 2 - forehead.y);
        this._smooth('eyebrowRaise', Math.min(1, Math.max(0, browH * 20 + 0.5)));
      }
    }

    // Pose signals
    if (mp.modes.pose && mp._poseLandmarks && mp._poseLandmarks.length > 24) {
      const pl = mp._poseLandmarks;
      const lShoulder = pl[11], rShoulder = pl[12], lHip = pl[23], rHip = pl[24];
      const lElbow = pl[13], rElbow = pl[14], lWrist = pl[15], rWrist = pl[16];
      // Body lean
      if (lShoulder && rShoulder && lHip && rHip) {
        const shoulderMid = (lShoulder.x + rShoulder.x) / 2;
        const hipMid = (lHip.x + rHip.x) / 2;
        this._smooth('bodyLean', (shoulderMid - hipMid) * 4 + 0.5);
      }
      // Arm angles
      if (lShoulder && lElbow && lWrist) {
        const angle = this._armAngle(lShoulder, lElbow, lWrist);
        this._smooth('leftArmAngle', angle / Math.PI);
      }
      if (rShoulder && rElbow && rWrist) {
        const angle = this._armAngle(rShoulder, rElbow, rWrist);
        this._smooth('rightArmAngle', angle / Math.PI);
      }
      // Shoulder width
      if (lShoulder && rShoulder) {
        const w = Math.sqrt((lShoulder.x - rShoulder.x) ** 2 + (lShoulder.y - rShoulder.y) ** 2);
        this._smooth('shoulderWidth', Math.min(1, w * 3));
      }
    }
  }

  applyToLayer(layer) {
    if (!layer.mpBindings) return;
    for (const b of layer.mpBindings) {
      if (b.source === 'derived' && this.derived[b.signalKey] != null) {
        const raw = this.derived[b.signalKey];
        if (typeof applyBindingValue === 'function') {
          applyBindingValue(layer, b, raw);
        }
      }
    }
  }

  _smooth(key, target) {
    const prev = this.derived[key] || 0;
    this.derived[key] = prev + (target - prev) * this._smoothing;
  }

  _armAngle(shoulder, elbow, wrist) {
    const v1x = shoulder.x - elbow.x, v1y = shoulder.y - elbow.y;
    const v2x = wrist.x - elbow.x, v2y = wrist.y - elbow.y;
    const dot = v1x * v2x + v1y * v2y;
    const mag1 = Math.sqrt(v1x * v1x + v1y * v1y);
    const mag2 = Math.sqrt(v2x * v2x + v2y * v2y);
    return mag1 > 0 && mag2 > 0 ? Math.acos(Math.max(-1, Math.min(1, dot / (mag1 * mag2)))) : 0;
  }
}

class MediaPipeManager {
  constructor(gl) {
    this.gl = gl;
    this.active = false;
    this.modes = { hand: false, face: false, pose: false, segment: false };
    this.handLandmarker = null;
    this.faceLandmarker = null;
    this.poseLandmarker = null;
    this.imageSegmenter = null;
    this.handTex = null;
    this.faceTex = null;
    this.poseTex = null;
    this.segTex = null;
    this.handCount = 0;
    this.handPos = [0, 0, 0];
    this.handPos2 = [0, 0, 0];
    this.isPinching = false;
    this.isPinching2 = false;
    this.pinchPos = [0, 0];
    this._pinchStartPos = null;
    this._pinchAccumX = 0;
    this._pinchAccumY = 0;
    this._lastPinchPos = null;
    this._lastHandLandmarks2 = null;
    this._frameCount = 0;
    this._lastTimestamp = 0;
  }

  async init(modes) {
    // Dispose existing landmarkers before reinit to avoid GPU resource leaks
    if (this.handLandmarker) { this.handLandmarker.close(); this.handLandmarker = null; }
    if (this.faceLandmarker) { this.faceLandmarker.close(); this.faceLandmarker = null; }
    if (this.poseLandmarker) { this.poseLandmarker.close(); this.poseLandmarker = null; }
    if (this.imageSegmenter) { this.imageSegmenter.close(); this.imageSegmenter = null; }
    this.active = false;

    if (!window.MediaPipeVision) await loadMediaPipeVision();
    if (!window.MediaPipeVision) throw new Error('MediaPipe failed to load');
    const { FilesetResolver, HandLandmarker, FaceLandmarker, PoseLandmarker, ImageSegmenter } = window.MediaPipeVision;
    const wasmPath = 'https://cdn.jsdelivr.net/npm/@mediapipe/tasks-vision@0.10.32/wasm';
    const vision = await FilesetResolver.forVisionTasks(wasmPath);

    this.modes = { ...this.modes, ...modes };

    if (this.modes.hand) {
      this.handLandmarker = await HandLandmarker.createFromOptions(vision, {
        baseOptions: { modelAssetPath: 'https://storage.googleapis.com/mediapipe-models/hand_landmarker/hand_landmarker/float16/1/hand_landmarker.task', delegate: 'GPU' },
        runningMode: 'VIDEO', numHands: 2
      });
      this.handTex = this._createDataTex(42, 1);
    }
    if (this.modes.face) {
      this.faceLandmarker = await FaceLandmarker.createFromOptions(vision, {
        baseOptions: { modelAssetPath: 'https://storage.googleapis.com/mediapipe-models/face_landmarker/face_landmarker/float16/1/face_landmarker.task', delegate: 'GPU' },
        runningMode: 'VIDEO', numFaces: 1
      });
      this.faceTex = this._createDataTex(478, 1);
    }
    if (this.modes.pose) {
      this.poseLandmarker = await PoseLandmarker.createFromOptions(vision, {
        baseOptions: { modelAssetPath: 'https://storage.googleapis.com/mediapipe-models/pose_landmarker/pose_landmarker_lite/float16/1/pose_landmarker_lite.task', delegate: 'GPU' },
        runningMode: 'VIDEO'
      });
      this.poseTex = this._createDataTex(33, 1);
    }
    if (this.modes.segment) {
      this.imageSegmenter = await ImageSegmenter.createFromOptions(vision, {
        baseOptions: { modelAssetPath: 'https://storage.googleapis.com/mediapipe-models/image_segmenter/selfie_segmenter/float16/latest/selfie_segmenter.tflite', delegate: 'GPU' },
        runningMode: 'VIDEO', outputCategoryMask: false, outputConfidenceMasks: true
      });
    }
    this.active = true;
  }

  _createDataTex(w, h) {
    const gl = this.gl;
    const tex = gl.createTexture();
    gl.bindTexture(gl.TEXTURE_2D, tex);
    gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, w, h, 0, gl.RGBA, gl.UNSIGNED_BYTE, null);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.NEAREST);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.NEAREST);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
    return tex;
  }

  // Rebuild GL textures after context restore
  reinitTextures() {
    if (this.modes.hand && this.handLandmarker) this.handTex = this._createDataTex(42, 1);
    if (this.modes.face && this.faceLandmarker) this.faceTex = this._createDataTex(478, 1);
    if (this.modes.pose && this.poseLandmarker) this.poseTex = this._createDataTex(33, 1);
    // segTex is created on-demand per frame, no persistent texture to rebuild
  }

  detect(video, timestamp) {
    if (!this.active || !video || video.readyState < 2) return;
    // Throttle to ~30fps (every 2nd frame at 60fps)
    this._frameCount++;
    if (this._frameCount % 2 !== 0) return;
    if (timestamp <= this._lastTimestamp) timestamp = this._lastTimestamp + 1;
    this._lastTimestamp = timestamp;

    const gl = this.gl;

    if (this.handLandmarker) {
      const result = this.handLandmarker.detectForVideo(video, timestamp);
      this.handCount = result.landmarks ? result.landmarks.length : 0;
      if (this.handCount > 0) {
        const lm = result.landmarks[0][9]; // middle finger MCP as hand center
        this.handPos = [lm.x, 1.0 - lm.y, lm.z];

        // Pinch detection: distance between thumb tip (4) and index tip (8)
        const thumb = result.landmarks[0][4];
        const index = result.landmarks[0][8];
        const dx = thumb.x - index.x;
        const dy = thumb.y - index.y;
        const dz = thumb.z - index.z;
        const pinchDist = Math.sqrt(dx * dx + dy * dy + dz * dz);
        const wasPinching = this.isPinching;
        this.isPinching = pinchDist < 0.05; // threshold ~5% of frame
        this.pinchPos = [(thumb.x + index.x) / 2, 1.0 - (thumb.y + index.y) / 2];

        if (this.isPinching && !wasPinching) {
          this._pinchStartPos = [...this.pinchPos];
          this._pinchAccumX = 0;
          this._pinchAccumY = 0;
        }
        if (this.isPinching && this._pinchStartPos) {
          this._pinchAccumX += (this.pinchPos[0] - (this._lastPinchPos || this.pinchPos)[0]) * Math.PI * 4;
          this._pinchAccumY += (this.pinchPos[1] - (this._lastPinchPos || this.pinchPos)[1]) * Math.PI * 4;
        }
        this._lastPinchPos = [...this.pinchPos];

        // Store raw landmarks for binding resolution
        this._lastHandLandmarks = result.landmarks[0].map(p => ({ x: p.x, y: 1.0 - p.y, z: p.z }));
        // Store second hand landmarks for two-hand gesture processing
        if (result.landmarks.length >= 2) {
          const lm2 = result.landmarks[1][9];
          this.handPos2 = [lm2.x, 1.0 - lm2.y, lm2.z];
          this._lastHandLandmarks2 = result.landmarks[1].map(p => ({ x: p.x, y: 1.0 - p.y, z: p.z }));
          // Second hand pinch detection
          const thumb2 = result.landmarks[1][4];
          const index2 = result.landmarks[1][8];
          const dx2 = thumb2.x - index2.x;
          const dy2 = thumb2.y - index2.y;
          const dz2 = thumb2.z - index2.z;
          const pinchDist2 = Math.sqrt(dx2 * dx2 + dy2 * dy2 + dz2 * dz2);
          this.isPinching2 = pinchDist2 < 0.05;
        } else {
          this.handPos2 = [0, 0, 0];
          this._lastHandLandmarks2 = null;
          this.isPinching2 = false;
        }

        // Pack all landmarks into RGBA texture
        const data = new Uint8Array(42 * 4);
        for (let h = 0; h < Math.min(2, result.landmarks.length); h++) {
          for (let i = 0; i < 21; i++) {
            const idx = (h * 21 + i) * 4;
            const p = result.landmarks[h][i];
            data[idx] = Math.round(p.x * 255);
            data[idx + 1] = Math.round((1.0 - p.y) * 255);
            data[idx + 2] = Math.round((p.z + 0.5) * 255);
            data[idx + 3] = 255;
          }
        }
        gl.bindTexture(gl.TEXTURE_2D, this.handTex);
        gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, 42, 1, 0, gl.RGBA, gl.UNSIGNED_BYTE, data);
      } else {
        this.handPos = [0, 0, 0];
        this.handPos2 = [0, 0, 0];
        this.isPinching = false;
        this._lastHandLandmarks2 = null;
      }
    }

    if (this.faceLandmarker) {
      const result = this.faceLandmarker.detectForVideo(video, timestamp);
      if (result.faceLandmarks && result.faceLandmarks.length > 0) {
        this._lastFaceLandmarks = result.faceLandmarks[0].map(p => ({ x: p.x, y: 1.0 - p.y, z: p.z }));
        const data = new Uint8Array(478 * 4);
        for (let i = 0; i < Math.min(478, result.faceLandmarks[0].length); i++) {
          const p = result.faceLandmarks[0][i];
          data[i * 4] = Math.round(p.x * 255);
          data[i * 4 + 1] = Math.round((1.0 - p.y) * 255);
          data[i * 4 + 2] = Math.round((p.z + 0.5) * 255);
          data[i * 4 + 3] = 255;
        }
        gl.bindTexture(gl.TEXTURE_2D, this.faceTex);
        gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, 478, 1, 0, gl.RGBA, gl.UNSIGNED_BYTE, data);
      }
    }

    if (this.poseLandmarker) {
      const result = this.poseLandmarker.detectForVideo(video, timestamp);
      if (result.landmarks && result.landmarks.length > 0) {
        this._lastPoseLandmarks = result.landmarks[0].map(p => ({ x: p.x, y: 1.0 - p.y, z: p.z }));
        const data = new Uint8Array(33 * 4);
        for (let i = 0; i < 33; i++) {
          const p = result.landmarks[0][i];
          data[i * 4] = Math.round(p.x * 255);
          data[i * 4 + 1] = Math.round((1.0 - p.y) * 255);
          data[i * 4 + 2] = Math.round((p.z + 0.5) * 255);
          data[i * 4 + 3] = Math.round((p.visibility || 0) * 255);
        }
        gl.bindTexture(gl.TEXTURE_2D, this.poseTex);
        gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, 33, 1, 0, gl.RGBA, gl.UNSIGNED_BYTE, data);
      }
    }

    if (this.imageSegmenter) {
      this.imageSegmenter.segmentForVideo(video, timestamp, (result) => {
        if (result.confidenceMasks && result.confidenceMasks.length > 0) {
          const mask = result.confidenceMasks[0];
          const w = mask.width, h = mask.height;
          const data = new Uint8Array(w * h * 4);
          const raw = mask.getAsFloat32Array();
          for (let i = 0; i < raw.length; i++) {
            const v = Math.round(raw[i] * 255);
            data[i * 4] = data[i * 4 + 1] = data[i * 4 + 2] = v;
            data[i * 4 + 3] = 255;
          }
          if (!this.segTex) this.segTex = this._createDataTex(w, h);
          gl.bindTexture(gl.TEXTURE_2D, this.segTex);
          gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, w, h, 0, gl.RGBA, gl.UNSIGNED_BYTE, data);
        }
      });
    }
  }

  getLabel() {
    const parts = [];
    if (this.modes.hand) parts.push('hand');
    if (this.modes.face) parts.push('face');
    if (this.modes.pose) parts.push('pose');
    if (this.modes.segment) parts.push('segment');
    return 'MediaPipe (' + parts.join('+') + ')';
  }

  dispose() {
    if (this.handLandmarker) this.handLandmarker.close();
    if (this.faceLandmarker) this.faceLandmarker.close();
    if (this.poseLandmarker) this.poseLandmarker.close();
    if (this.imageSegmenter) this.imageSegmenter.close();
    this.active = false;
  }
}

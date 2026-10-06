/*{
  "DESCRIPTION": "Morphing Organic Text — three voices breathe through a domain-warped FBM field that flows like a slow chemistry. Each player owns a region of the canvas and pushes a reaction-diffusion-feel current of warp; bands intermingle, fuse, split, never repeat. A pseudo-3D normal field and a z-parallax shell of darker tissue give the morph real depth. The live utterance arrives as a hand-typewritten line that sinks into the field, contrast-adapted against the morph beneath it.",
  "CREDIT": "easel A-List",
  "CATEGORIES": [
    "Generator",
    "Text",
    "A-List"
  ],
  "INPUTS": [
    {
      "NAME": "energyA",
      "LABEL": "Player A Energy",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "BIND": "player[1].energy"
    },
    {
      "NAME": "energyB",
      "LABEL": "Player B Energy",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "BIND": "player[2].energy"
    },
    {
      "NAME": "energyC",
      "LABEL": "Player C Energy",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "BIND": "player[3].energy"
    },
    {
      "NAME": "activeA",
      "LABEL": "Player A Active",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "BIND": "player[1].active"
    },
    {
      "NAME": "activeB",
      "LABEL": "Player B Active",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "BIND": "player[2].active"
    },
    {
      "NAME": "fidBloom",
      "LABEL": "Glow",
      "TYPE": "float",
      "DEFAULT": 0.55,
      "MIN": 0,
      "MAX": 1.5
    },
    {
      "NAME": "fidDither",
      "LABEL": "Dither",
      "TYPE": "float",
      "DEFAULT": 0.85,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "fidGamma",
      "LABEL": "Gamma",
      "TYPE": "float",
      "DEFAULT": 0.6,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "fidEdgeGlow",
      "LABEL": "Edge Glow",
      "TYPE": "float",
      "DEFAULT": 0.55,
      "MIN": 0,
      "MAX": 2
    },
    {
      "NAME": "fidVignette",
      "LABEL": "Vignette",
      "TYPE": "float",
      "DEFAULT": 0.45,
      "MIN": 0,
      "MAX": 1.5
    },
    {
      "NAME": "fidGrain",
      "LABEL": "Grain",
      "TYPE": "float",
      "DEFAULT": 0.35,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "octaves",
      "LABEL": "Octaves",
      "TYPE": "long",
      "DEFAULT": 5,
      "VALUES": [
        3,
        4,
        5,
        6
      ],
      "LABELS": [
        "3",
        "4",
        "5",
        "6"
      ],
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "morphSpeed",
      "LABEL": "Morph Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "warpAmp",
      "LABEL": "Warp Amplitude",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2.5,
      "DEFAULT": 1.1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "motionDrift",
      "LABEL": "Drift Speed",
      "TYPE": "float",
      "DEFAULT": 1.3,
      "MIN": 0,
      "MAX": 3,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "motionJitter",
      "LABEL": "Jitter",
      "TYPE": "float",
      "DEFAULT": 0.25,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "motionFlicker",
      "LABEL": "Flicker",
      "TYPE": "float",
      "DEFAULT": 0.15,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "motionSway",
      "LABEL": "Sway",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "motionChaos",
      "LABEL": "Chaos",
      "TYPE": "float",
      "DEFAULT": 0.45,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "palette",
      "LABEL": "Palette",
      "TYPE": "long",
      "DEFAULT": 0,
      "VALUES": [
        0,
        1,
        2,
        3
      ],
      "LABELS": [
        "Lime/Ink",
        "Magenta/Bone",
        "Cyan/Iron",
        "Saffron/Indigo"
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "hueShift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "LABEL": "Hue Shift",
      "GROUP": "Color"
    },
    {
      "NAME": "colorBoost",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "LABEL": "Color Boost",
      "GROUP": "Color"
    },
    {
      "NAME": "zParallax",
      "LABEL": "Z Parallax",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.5,
      "DEFAULT": 0.85,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "msg",
      "TYPE": "text",
      "DEFAULT": "BREATHE INTO THE FIELD",
      "MAX_LENGTH": 48,
      "BIND": "cue.latest",
      "LABEL": "Message",
      "GROUP": "Text"
    },
    {
      "NAME": "textContrast",
      "LABEL": "Text vs Morph",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.5,
      "DEFAULT": 0.9,
      "GROUP": "Text"
    },
    {
      "NAME": "textScale",
      "LABEL": "Text Scale",
      "TYPE": "float",
      "MIN": 0.5,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Text"
    },
    {
      "NAME": "bgColor",
      "TYPE": "color",
      "DEFAULT": [
        0,
        0,
        0,
        0
      ],
      "LABEL": "Background",
      "GROUP": "Background"
    },
    {
      "NAME": "bassDepth",
      "LABEL": "Audio Depth",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.7,
      "BIND": "audio.bass",
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio React",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.35,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

// ── FIDELITY KIT v2 (text-safe cinematic polish) ────────────────────
// Pure RGB math on the final color — no displacement, no chroma shift,
// glyph shapes never touched. Stages: edge glow (dFdx) → headroom bloom
// → vignette → animated grain → soft Reinhard tonemap → dither + sRGB.
vec3 fidApply(vec3 col, vec2 frag) {
    float l = dot(col, vec3(0.299, 0.587, 0.114));
    vec2  lg   = vec2(dFdx(l), dFdy(l));
    float edge = clamp(length(lg) * 7.0, 0.0, 1.0);
    col += col * edge * fidEdgeGlow * 1.50;
    float headroom = smoothstep(0.28, 0.95, l);
    col += col * headroom * fidBloom * 1.80;
    vec2  uvN = frag / RENDERSIZE - 0.5;
    float vig = 1.0 - dot(uvN, uvN) * 1.80 * fidVignette;
    col *= clamp(vig, 0.0, 1.0);
    float g = fract(sin(dot(frag + vec2(TIME * 73.0, TIME * 41.0),
                            vec2(12.9898, 78.233))) * 43758.5453);
    col += (g - 0.5) * fidGrain * 0.045;
    col = col / (1.0 + col * 0.18);
    float n = fract(sin(dot(frag, vec2(12.9898, 78.233))) * 43758.5453);
    col += (n - 0.5) * (1.0 / 255.0) * fidDither;
    col = mix(col, pow(max(col, 0.0), vec3(1.0 / 2.2)), fidGamma);
    return col;
}


// ─── MOTION KIT (shared across recent text shaders) ─────────────────
// Adds life on top of each shader's native animation: a breathing sway +
// continuous drift + coarse reseeding jitter on the working coord, plus a
// brightness flicker. Driven by the motion* uniforms. mkMotion() returns an
// offset to add to a coord; mkFlicker() a brightness multiplier.
float mkHash(vec2 p){ p = fract(p * vec2(127.1, 311.7)); p += dot(p, p + 34.5); return fract(p.x * p.y); }
vec2 mkMotion(vec2 q, float t){
    float ch = 0.4 + motionChaos;
    vec2 sway  = vec2(sin(t * 0.32 + q.y * 1.8), cos(t * 0.27 + q.x * 1.6)) * motionSway  * 0.09;
    vec2 drift = vec2(sin(t * 0.12 * ch), cos(t * 0.10 * ch))            * motionDrift * 0.05;
    // Smooth organic wander — layered incommensurate sines, slow + no stepping.
    float f = 1.0 + 1.2 * motionChaos;
    vec2 jit = vec2(
        sin(t * 0.70 * f + q.y * 3.1) * 0.6 + sin(t * 0.45 * f + q.x * 2.3 + 1.7) * 0.4,
        cos(t * 0.60 * f + q.x * 2.7) * 0.6 + cos(t * 0.50 * f + q.y * 2.9 + 4.2) * 0.4
    ) * motionJitter * 0.05;
    return sway + drift + jit;
}
float mkFlicker(vec2 q, float t){
    // Smooth, slow brightness undulation + soft scanline (no hard strobe).
    float n    = 0.5 + 0.5 * sin(t * 2.0 + q.x * 7.0 + q.y * 5.0);
    float scan = 0.5 + 0.5 * sin(q.y * 180.0 + t * 3.0);
    return 1.0 - motionFlicker * (0.5 * n + 0.30 * scan);
}


// ════════════════════════════════════════════════════════════════════════
//  MORPHING ORGANIC TEXT  ·  A-List
//
//  Hero: a continuously morphing organic field. The field is a multi-
//  octave FBM that gets domain-warped twice (Iñigo Quilez warp), then
//  shaped by a reaction-diffusion-FEEL gain function — sharp ridges
//  between two slowly-moving phases (a fuse-split chemistry).
//
//  Each player energy injects a moving warp source in their slice of
//  the canvas (left / middle / right). Different slices, different
//  drift direction, different octave weighting — muting a player
//  visibly stills that band.
//
//  Depth: a back shell (warped further) parallaxes against the hero
//  field; pseudo-3D normals from the gain gradient produce specular
//  glints. Text is a typewriter line on the lower third, with local
//  contrast adapted to the morph luminance underneath.
// ════════════════════════════════════════════════════════════════════════

#define SPACE_CH 26
#define MAX_WALK 48
const float TAU = 6.28318530718;

// ─── Font atlas ──────────────────────────────────────────────────────
float sampleChar(int ch, vec2 uv) {
    if (ch < 0 || ch > 36) return 0.0;
    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) return 0.0;
    return texture2D(fontAtlasTex, vec2((float(ch) + uv.x) / 37.0, uv.y)).r;
}
int getChar(int slot) {
    if (slot ==  0) return int(msg_0);  if (slot ==  1) return int(msg_1);
    if (slot ==  2) return int(msg_2);  if (slot ==  3) return int(msg_3);
    if (slot ==  4) return int(msg_4);  if (slot ==  5) return int(msg_5);
    if (slot ==  6) return int(msg_6);  if (slot ==  7) return int(msg_7);
    if (slot ==  8) return int(msg_8);  if (slot ==  9) return int(msg_9);
    if (slot == 10) return int(msg_10); if (slot == 11) return int(msg_11);
    if (slot == 12) return int(msg_12); if (slot == 13) return int(msg_13);
    if (slot == 14) return int(msg_14); if (slot == 15) return int(msg_15);
    if (slot == 16) return int(msg_16); if (slot == 17) return int(msg_17);
    if (slot == 18) return int(msg_18); if (slot == 19) return int(msg_19);
    if (slot == 20) return int(msg_20); if (slot == 21) return int(msg_21);
    if (slot == 22) return int(msg_22); if (slot == 23) return int(msg_23);
    if (slot == 24) return int(msg_24); if (slot == 25) return int(msg_25);
    if (slot == 26) return int(msg_26); if (slot == 27) return int(msg_27);
    if (slot == 28) return int(msg_28); if (slot == 29) return int(msg_29);
    if (slot == 30) return int(msg_30); if (slot == 31) return int(msg_31);
    if (slot == 32) return int(msg_32); if (slot == 33) return int(msg_33);
    if (slot == 34) return int(msg_34); if (slot == 35) return int(msg_35);
    if (slot == 36) return int(msg_36); if (slot == 37) return int(msg_37);
    if (slot == 38) return int(msg_38); if (slot == 39) return int(msg_39);
    if (slot == 40) return int(msg_40); if (slot == 41) return int(msg_41);
    if (slot == 42) return int(msg_42); if (slot == 43) return int(msg_43);
    if (slot == 44) return int(msg_44); if (slot == 45) return int(msg_45);
    if (slot == 46) return int(msg_46); if (slot == 47) return int(msg_47);
    return -1;
}
int charCount() {
    int n = int(msg_len);
    if (n <= 0) return 0;
    if (n > 48) return 48;
    return n;
}

// ─── Noise / warp ────────────────────────────────────────────────────
float h21(vec2 p){
    vec3 q = fract(vec3(p.xyx)*vec3(0.1031,0.1030,0.0973));
    q += dot(q, q.yzx + 33.33);
    return fract((q.x + q.y) * q.z);
}
float vnoise(vec2 p){
    vec2 i = floor(p), f = fract(p);
    f = f*f*(3.0-2.0*f);
    float a = h21(i), b = h21(i+vec2(1,0));
    float c = h21(i+vec2(0,1)), d = h21(i+vec2(1,1));
    return mix(mix(a,b,f.x), mix(c,d,f.x), f.y);
}
float fbm(vec2 p, int oct){
    float s = 0.0, a = 0.55;
    // Loop bound is a compile-time constant; we branch on `oct`.
    for (int i = 0; i < 6; i++){
        if (i >= oct) break;
        s += a * vnoise(p);
        // irrational rotation to avoid axial banding
        float c = cos(0.7), si = sin(0.7);
        p = mat2(c, -si, si, c) * p * 1.97 + vec2(11.3, 5.7);
        a *= 0.52;
    }
    return s;
}

// ─── Palette ─────────────────────────────────────────────────────────
vec3 paletteLookup(int idx, float t, float ridge){
    // t = [0,1] morph phase, ridge = sharpness of the gain seam
    if (idx == 1) {
        // magenta / bone
        vec3 a = vec3(0.93, 0.90, 0.86);
        vec3 b = vec3(0.85, 0.18, 0.45);
        vec3 c = vec3(0.08, 0.05, 0.10);
        return mix(mix(a, b, t), c, ridge);
    } else if (idx == 2) {
        // cyan / iron
        vec3 a = vec3(0.78, 0.82, 0.84);
        vec3 b = vec3(0.20, 0.78, 0.86);
        vec3 c = vec3(0.06, 0.08, 0.12);
        return mix(mix(a, b, t), c, ridge);
    } else if (idx == 3) {
        // saffron / indigo
        vec3 a = vec3(0.94, 0.88, 0.74);
        vec3 b = vec3(0.96, 0.62, 0.18);
        vec3 c = vec3(0.10, 0.08, 0.28);
        return mix(mix(a, b, t), c, ridge);
    }
    // 0 — lime / ink (matches reference)
    vec3 a = vec3(0.91, 0.90, 0.87);          // warm bone
    vec3 b = vec3(0.78, 0.93, 0.32);          // lime
    vec3 c = vec3(0.05, 0.06, 0.07);          // ink
    return mix(mix(a, b, t), c, ridge);
}

// Player-driven warp source: a moving low-frequency vector field that
// adds to the domain-warp offsets. Each player has a slice anchor on x,
// a drift axis on y, and a different temporal phase. Multiplied by
// energy*active so muting visibly stills the band.
vec2 playerWarp(vec2 p, vec2 anchor, float drift, float t, float energy){
    if (energy < 1e-3) return vec2(0.0);
    vec2 d = p - anchor;
    float r = length(d) + 0.0001;
    // 2D rotational gust scaled by an inverse-square falloff (capped)
    float fall = 1.0 / (1.0 + 6.0 * r * r);
    float ang = drift + t;
    vec2 swirl = vec2(-d.y, d.x) / r * fall;
    vec2 push  = vec2(cos(ang), sin(ang)) * fall;
    return (swirl * 0.6 + push * 0.8) * energy;
}

// Compute the morph field gain at p, plus the local phase. The gain is
// a smoothstep around two moving plateaus — that's the reaction-feel.
// Returns vec3(gain, phase, ridge) where:
//   gain  ∈ [0,1] — overall coverage
//   phase ∈ [0,1] — slow temporal phase for the palette
//   ridge ∈ [0,1] — sharpness of the seam (peaks where two phases meet)
vec3 morphField(vec2 p, float t, int oct,
                vec2 wA, vec2 wB, vec2 wC, float warp){
    // Domain warp pass 1 — large-scale offset
    vec2 q = vec2(
        fbm(p + vec2(0.0, t * 0.21), oct),
        fbm(p + vec2(5.2, -t * 0.18) + 7.7, oct)
    );
    // Domain warp pass 2 — finer offset using q
    vec2 r = vec2(
        fbm(p + 3.5 * q + vec2(1.7, 9.2) + t * 0.13, oct),
        fbm(p + 3.5 * q + vec2(8.3, 2.8) - t * 0.11, oct)
    );
    // Player perturbation injected into the warp coordinates
    vec2 warped = p + warp * (1.6 * r - 0.8) + wA + wB + wC;
    float n = fbm(warped * 1.05 + vec2(0.0, t * 0.05), oct);
    // Reaction-diffusion-feel gain — sharp seam between two phases
    float seam = smoothstep(0.42, 0.58, n);
    // ridge: largest at the seam transition
    float ridge = 1.0 - smoothstep(0.0, 0.18, abs(n - 0.5));
    return vec3(seam, n, ridge);
}

void main(){
    vec2 res = RENDERSIZE;
    vec2 uv  = (gl_FragCoord.xy - 0.5 * res) / res.y;
    uv += mkMotion(uv, TIME);
    float aspect = res.x / res.y;

    float t   = TIME * 0.45 * max(morphSpeed, 0.0);
    float eA  = clamp(energyA * mix(0.35, 1.0, clamp(activeA, 0.0, 1.0)), 0.0, 1.0);
    float eB  = clamp(energyB * mix(0.35, 1.0, clamp(activeB, 0.0, 1.0)), 0.0, 1.0);
    float eC  = clamp(energyC, 0.0, 1.0);   // player[3] only binds energy here
    float bass = clamp(bassDepth, 0.0, 2.0);
    int   oct = int(clamp(float(octaves), 3.0, 6.0));

    // ─── Native audio bus (soft-kneed, idle-floor safe) ───────────────
    float aReact = clamp(audioReact, 0.0, 2.0);
    float aBassP = pow(smoothstep(0.05, 0.85, audioBass), 1.6);
    float aHighP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float aBeat  = audioBeatPulse * audioBeatPulse;

    // Each player owns a column anchor on x; y drifts so the warp
    // sources don't all live on the same horizon.
    vec2 anchorA = vec2(-0.55 * aspect, sin(t * 0.31) * 0.18);
    vec2 anchorB = vec2( 0.00,           cos(t * 0.27) * 0.16);
    vec2 anchorC = vec2( 0.55 * aspect,  sin(t * 0.23 + 1.7) * 0.20);

    vec2 wA = playerWarp(uv, anchorA, 0.0,  t * 0.6, eA);
    vec2 wB = playerWarp(uv, anchorB, 1.7,  t * 0.7, eB);
    vec2 wC = playerWarp(uv, anchorC, 3.4,  t * 0.5, eC);

    // Hero field (front shell)
    float warp = warpAmp * (0.85 + 0.25 * bass + 2.1 * aReact * aBassP);
    vec3 front = morphField(uv * 1.45, t, oct, wA, wB, wC, warp);

    // ─── z-parallax back shell ───────────────────────────────────────
    // Shifted, dimmer, slower — reads as depth behind the hero field.
    // Parallax offset proportional to a fake camera nudge.
    vec2 camOffset = (mousePos - 0.5) * 0.18 + vec2(sin(t*0.13), cos(t*0.11)) * 0.04;
    vec2 backUv    = uv * 0.95 - camOffset * zParallax;
    vec3 back      = morphField(backUv * 1.05 + vec2(3.1, -2.3),
                                t * 0.7, int(max(float(oct - 1), 3.0)),
                                wA * 0.5, wB * 0.5, wC * 0.5, warp * 0.7);

    // ─── Pseudo-3D normals from gain gradient ────────────────────────
    // Numeric gradient of the front "phase" field → normal → specular.
    float eps = 0.0035;
    vec3 fx0 = morphField(uv * 1.45 + vec2( eps,0.0), t, oct, wA, wB, wC, warp);
    vec3 fx1 = morphField(uv * 1.45 + vec2(-eps,0.0), t, oct, wA, wB, wC, warp);
    vec3 fy0 = morphField(uv * 1.45 + vec2(0.0, eps), t, oct, wA, wB, wC, warp);
    vec3 fy1 = morphField(uv * 1.45 + vec2(0.0,-eps), t, oct, wA, wB, wC, warp);
    vec2 grad = vec2(fx0.y - fx1.y, fy0.y - fy1.y) / (2.0 * eps);
    vec3 nrm  = normalize(vec3(-grad * (0.45 + 0.5 * zParallax), 1.0));
    vec3 L    = normalize(vec3(0.45, 0.65, 0.7));
    float diff = clamp(dot(nrm, L), 0.0, 1.0);
    vec3 H    = normalize(L + vec3(0.0, 0.0, 1.0));
    float spec = pow(clamp(dot(nrm, H), 0.0, 1.0), 28.0);

    int pal = int(clamp(float(palette), 0.0, 3.0));

    // Compose back layer (further phase, dimmer, softer)
    vec3 backCol = paletteLookup(pal, back.y, back.z) * (0.55 + 0.10 * bass);

    // Compose front layer with normal lighting
    vec3 frontCol = paletteLookup(pal, front.y, front.z);
    frontCol = mix(frontCol * 0.55, frontCol * (0.6 + 0.6 * diff), 0.8);
    frontCol += spec * vec3(1.0, 0.97, 0.85) * (0.35 + 0.45 * bass + 0.9 * aReact * aHighP) * (1.0 - front.z);

    // Mask between layers — the front shell occludes the back where the
    // morph "fills"; fwidth-AA over the gain seam keeps it pixel-free.
    float fw = fwidth(front.x);
    // Bass nudges the fuse/split threshold itself — the seam visibly
    // advances or retreats with the beat, a structural (not just tonal) cue.
    float seamShift = 0.10 * aReact * (aBassP - 0.5);
    float coverage = smoothstep(0.35 - fw + seamShift, 0.65 + fw + seamShift, front.x);

    // Player tint glints — each player adds a low-saturation halo near
    // their anchor when energetic. Visibly distinct per-player response.
    float gA = exp(-dot(uv - anchorA, uv - anchorA) * 6.0) * eA;
    float gB = exp(-dot(uv - anchorB, uv - anchorB) * 6.0) * eB;
    float gC = exp(-dot(uv - anchorC, uv - anchorC) * 6.0) * eC;

    vec3 col = mix(backCol, frontCol, coverage);
    col += vec3(0.55, 0.95, 0.30) * gA * 0.18;   // A — lime
    col += vec3(0.95, 0.40, 0.55) * gB * 0.18;   // B — rose
    col += vec3(0.30, 0.70, 0.95) * gC * 0.18;   // C — sky
    // Inter-player fusion ridge: where two halos overlap, brighten.
    col += vec3(1.0) * (gA*gB + gB*gC + gA*gC) * 0.25;

    // Beat accent — decaying event lit along the seam ridge, never a strobe.
    col += vec3(1.0, 0.98, 0.92) * aBeat * aReact * 0.60 * front.z;

    // Whole-field energy breathing — a soft global lift so the field
    // visibly "inhales" with the music, idle floor keeps silence untouched.
    float aGlow = smoothstep(0.10, 0.85, audioLevel);
    col *= 1.0 + 1.25 * aReact * aGlow;

    // Gentle vignette + paper warmth
    col *= 1.0 - 0.22 * dot(uv, uv);

    // ─── Typewriter text ─────────────────────────────────────────────
    // The cue line lives on the lower third, hand-typed left-to-right.
    int total = charCount();
    bool live = msgAge >= 0.0;
    if (total > 0) {
        // Typewriter reveal — chars per second matches Easel's typewriter.
        float charsPerSec = 28.0;
        int reveal = live ? int(floor(max(msgAge, 0.0) * charsPerSec)) : total;
        if (reveal > total) reveal = total;
        if (reveal < 1)     reveal = 1;
        if (!live) reveal = total;

        // Lay glyphs across ~84% of canvas width, one row, lower third.
        float scale = clamp(textScale, 0.5, 2.0);
        float baseH = 0.062 * scale;
        // Auto-shrink so long lines still fit the row width.
        float charsForFit = max(float(reveal), 1.0);
        // each cell is glyph height * 5/7 * kerning(0.95)
        float cellW = baseH * (5.0/7.0) * 0.95;
        float maxLineW = 0.84 * aspect;
        if (charsForFit * cellW > maxLineW) {
            float shrink = maxLineW / (charsForFit * cellW);
            baseH *= shrink;
            cellW *= shrink;
        }
        float lineW = float(reveal) * cellW;
        float baseY = -0.30;   // lower third anchor (uv space, y up)
        float baseX = -0.5 * lineW;
        // Subtle wave-bob in lockstep with the morph beneath the line
        float waveBob = 0.012 * sin(t * 1.3 + uv.x * 4.0);

        vec2 tp = vec2(uv.x - baseX, (uv.y - baseY - waveBob));
        // Top-down rows: invert y for atlas (atlas V=1 at top)
        float glyphH = baseH;
        float glyphW = cellW;
        // Inside text band?
        if (tp.x >= 0.0 && tp.x <= lineW &&
            tp.y <= 0.5 * glyphH && tp.y >= -0.5 * glyphH) {
            int col_i = int(floor(tp.x / glyphW));
            if (col_i >= 0 && col_i < reveal) {
                int ch = getChar(col_i);
                if (ch >= 0 && ch <= 35 && ch != SPACE_CH) {
                    float lx = (tp.x - float(col_i) * glyphW) / glyphW;
                    // tp.y is y-UP world; (tp.y+0.5gH)/gH maps
                    // screen-bottom→0, screen-top→1. The host font atlas
                    // stores letter-top at v=1, so the direct mapping
                    // puts letter-top at screen-top. The previous `1.0 -`
                    // here flipped glyphs upside down.
                    float ly = (tp.y + 0.5 * glyphH) / glyphH;
                    // Glyph cell letterboxed (5:7) horizontally inside cellW
                    float aspectFix = (5.0/7.0) * 0.95;
                    float pad = (1.0 - aspectFix) * 0.5;
                    if (lx >= pad && lx <= 1.0 - pad) {
                        float ux = (lx - pad) / (1.0 - 2.0*pad);
                        float s  = sampleChar(ch, vec2(ux, ly));
                        // fwidth-AA on glyph alpha
                        float sa = smoothstep(0.30, 0.62, s);
                        if (sa > 0.001) {
                            // Local contrast — flip ink color based on
                            // the morph luminance beneath this pixel.
                            float lum = dot(col, vec3(0.299, 0.587, 0.114));
                            vec3 ink = (lum > 0.55)
                                     ? vec3(0.03, 0.04, 0.05)
                                     : vec3(0.97, 0.98, 0.96);
                            // Press the text INTO the field — a slight
                            // dark halo under the glyph reads as inkbleed.
                            float halo = smoothstep(0.45, 0.85, s);
                            col = mix(col, col * 0.78, halo * 0.5 * textContrast);
                            col = mix(col, ink, sa * clamp(textContrast, 0.0, 1.5));
                        }
                    }
                }
            }
        }
    }

    // ─── Anti-banding grain ─────────────────────────────────────────
    float grain = h21(gl_FragCoord.xy + fract(TIME) * 53.0) - 0.5;
    col += grain * 0.018;

    // Mild tonemap
    col = col / (1.0 + 0.45 * col);
    col = pow(max(col, 0.0), vec3(0.95));

    // ---- universal color block (defaults = no-op) ----
    float ucL = dot(col, vec3(0.299, 0.587, 0.114));
    col = mix(vec3(ucL), col, colorBoost);
    if (hueShift > 0.0005) {
        float hA = hueShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        col = clamp(hM * col, 0.0, 1.0);
    }
    col = mix(col, bgColor.rgb, bgColor.a * (1.0 - smoothstep(0.0, 0.35, ucL)));
    col *= mkFlicker(gl_FragCoord.xy / RENDERSIZE - 0.5, TIME);
    gl_FragColor = vec4(fidApply(col, gl_FragCoord.xy), 1.0);
}

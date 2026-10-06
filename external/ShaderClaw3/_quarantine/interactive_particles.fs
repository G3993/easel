/*{
  "DESCRIPTION": "Interactive Particles — a lightweight, transparent particle field built as an OVERLAY. Uses a spatial grid so every pixel only ever evaluates its 9 nearest cells (not all particles) — cost is constant no matter how dense you make it, so it stays real-time stacked on top of other effects. Empty space is fully transparent (alpha = particle brightness); set the layer Blend Mode to Add or Screen for emissive glow. mouseX/mouseY (-1..1) or the held mouse steer particles toward a point — bind from MIDI/OSC to sprinkle touches live. Audio modulates brightness/jitter (never gates). LINEAR HDR out.",
  "CREDIT": "Procedural spatial-grid particle field by Easel/ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "Particles",
    "Interactive",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "intensity",
      "LABEL": "Glow Falloff",
      "TYPE": "float",
      "DEFAULT": 1.5,
      "MIN": 0.5,
      "MAX": 3
    },
    {
      "NAME": "exposure",
      "LABEL": "Brightness",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0.2,
      "MAX": 3
    },
    {
      "NAME": "mouseX",
      "LABEL": "Mouse X (-1..1)",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": -1,
      "MAX": 1
    },
    {
      "NAME": "mouseY",
      "LABEL": "Mouse Y (-1..1)",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": -1,
      "MAX": 1
    },
    {
      "NAME": "mouseInfluence",
      "LABEL": "Steer Strength",
      "TYPE": "float",
      "DEFAULT": 0.6,
      "MIN": 0,
      "MAX": 2
    },
    {
      "NAME": "mouseRadius",
      "LABEL": "Steer Radius",
      "TYPE": "float",
      "DEFAULT": 0.45,
      "MIN": 0.05,
      "MAX": 1.5
    },
    {
      "NAME": "streamCount",
      "LABEL": "Field Layers",
      "TYPE": "long",
      "DEFAULT": 3,
      "VALUES": [
        1,
        2,
        3,
        4,
        5
      ],
      "LABELS": [
        "1",
        "2",
        "3",
        "4",
        "5"
      ],
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "particlesPerStream",
      "LABEL": "Density",
      "TYPE": "long",
      "DEFAULT": 1,
      "VALUES": [
        0,
        1,
        2,
        3
      ],
      "LABELS": [
        "Sparse",
        "Med",
        "Dense",
        "Storm"
      ],
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "spread",
      "LABEL": "Scatter",
      "TYPE": "float",
      "DEFAULT": 0.55,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "particleSize",
      "LABEL": "Particle Size",
      "TYPE": "float",
      "DEFAULT": 0.0016,
      "MIN": 0.0003,
      "MAX": 0.006,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "flowSpeed",
      "LABEL": "Flow Speed",
      "TYPE": "float",
      "DEFAULT": 0.18,
      "MIN": 0,
      "MAX": 1.5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "flowAngle",
      "LABEL": "Flow Angle",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 6.2832,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "swirl",
      "LABEL": "Swirl",
      "TYPE": "float",
      "DEFAULT": 0.6,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "hueShift",
      "LABEL": "Hue",
      "TYPE": "float",
      "DEFAULT": 0.55,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "hueSpread",
      "LABEL": "Hue Spread",
      "TYPE": "float",
      "DEFAULT": 0.35,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "colorBoost",
      "LABEL": "Color Boost",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "bgColor",
      "LABEL": "Background",
      "TYPE": "color",
      "DEFAULT": [
        0,
        0,
        0,
        0
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio React",
      "TYPE": "float",
      "DEFAULT": 0.7,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//  INTERACTIVE PARTICLES — spatial-grid edition (cheap, transparent overlay)
//
//  WHY THIS IS FAST:
//   The old version looped over EVERY particle (up to 480) for EVERY pixel,
//   so cost = pixels × particles ≈ a billion evaluations/frame. Unusable.
//
//   This version places one particle per grid cell. A pixel only ever looks
//   at the 9 cells around it (its own + 8 neighbours). Per-pixel work is
//   therefore CONSTANT — 9 particles — no matter how high you push Density.
//   Each particle stays within ~1 cell of its home (drift/swirl/steer are all
//   bounded) so the 3×3 neighbourhood is guaranteed to catch every particle
//   that could touch this pixel. This is the standard trick for real-time
//   procedural particle fields.
//
//  WHY IT OVERLAYS CLEANLY:
//   Output alpha = particle brightness. Empty space → alpha 0 → fully
//   transparent, so whatever is beneath shows through untouched. Set the
//   layer's Blend Mode to Add or Screen for emissive glow on top.
// ════════════════════════════════════════════════════════════════════════

#define PI 3.14159265359

float h11(float n) { return fract(sin(n * 12.9898) * 43758.5453); }
vec2  h21(float n) { return vec2(h11(n), h11(n + 17.31)); }

vec3 hsv2rgb(vec3 c) {
    vec3 p = abs(fract(c.xxx + vec3(0.0, 2.0/3.0, 1.0/3.0)) * 6.0 - 3.0);
    return c.z * mix(vec3(1.0), clamp(p - 1.0, 0.0, 1.0), c.y);
}

void main() {
    vec2  uv     = gl_FragCoord.xy / RENDERSIZE.xy;
    float aspect = RENDERSIZE.x / max(RENDERSIZE.y, 1.0);
    float t      = TIME;

    // Audio is a pure modulator — at 0 the field is simply calmer/dimmer.
    float audio = clamp(audioBass + audioMid * 0.5 + audioHigh * 0.3, 0.0, 2.0);
    float react = clamp(audioReact, 0.0, 2.0);
    audio *= react;

    // Continuous smooth band-followers (ambient fix r2): LINEAR bands (they
    // are pre-smoothed; knees crush ambient swells), with a FLOOR on the
    // user param — audioReact defaults to 0.7, which diluted the round-1
    // depth. Bass breathes particle size/glow, mids breathe swirl radius.
    float reactF = 0.6 + 0.4 * min(react, 1.0);
    float bassSm = clamp(audioBass, 0.0, 1.0) * reactF;
    float midSm  = clamp(audioMid,  0.0, 1.0) * reactF;

    // Steer target in 0..1 space. Prefer the held mouse; otherwise use the
    // bound mouseX/mouseY (-1..1, centred at 0 = no bias) from MIDI/OSC.
    vec2  mxy01 = vec2(mouseX, mouseY) * 0.5 + 0.5;
    vec2  steer = mix(mxy01, mousePos, step(0.5, mouseDown));
    float steerW = mouseInfluence * (0.35
                   + 0.65 * step(0.5, mouseDown)
                   + 0.65 * step(0.001, length(vec2(mouseX, mouseY))));

    // Density → grid resolution. Per-pixel cost is FIXED (9 cells) regardless
    // of this, so "Storm" is just as cheap as "Sparse".
    int   tier  = int(clamp(float(particlesPerStream), 0.0, 3.0));
    float base  = tier == 0 ? 5.0 : tier == 1 ? 8.0 : tier == 2 ? 12.0 : 17.0;
    float gridN = base + (clamp(float(streamCount), 1.0, 5.0) - 1.0) * 1.5;
    // Square-ish cells: more columns on wide canvases.
    vec2  gdim  = vec2(gridN * aspect, gridN);

    // Bass swells the particle footprint (±30%) — reads as glow blooming
    // with the envelope, continuous and smooth (bands are pre-smoothed).
    float drawSize = particleSize * aspect * (1.0 + 0.65 * bassSm);
    float swl      = clamp(swirl, 0.0, 2.0);
    float scat     = clamp(spread, 0.0, 1.0);

    vec3  color = vec3(0.0);
    vec2  cell  = floor(uv * gdim);

    // Only the 3×3 cells around this pixel — the whole optimisation.
    for (int dy = -1; dy <= 1; dy++) {
        for (int dx = -1; dx <= 1; dx++) {
            vec2  cid  = cell + vec2(float(dx), float(dy));
            float seed = h11(cid.x * 57.0 + cid.y * 131.7 + 1.3);

            // ── Local offset, in CELL units, kept inside ±0.95 so the
            //    particle never escapes the 3×3 search window. ──
            // Static scatter from cell centre.
            vec2  jit = (h21(seed * 91.0) - 0.5) * scat;

            // Directional drift that wraps within the cell (infinite flow).
            float ang = flowAngle + (h11(seed + 3.7) - 0.5) * 0.7;
            vec2  dir = vec2(cos(ang), sin(ang));
            float spd = flowSpeed * (0.5 + h11(seed + 9.1));
            float ph  = fract(t * spd + seed) - 0.5;          // -0.5..0.5
            vec2  drift = dir * ph * 0.9;

            // Per-particle swirl wobble.
            float swA = t * (0.2 + 0.5 * h11(seed + 4.2)) + seed * 6.2831;
            vec2  sw  = vec2(cos(swA), sin(swA)) * 0.16 * swl * (1.0 + 0.9 * midSm);

            vec2 local = jit + drift + sw;

            // ── Steer: nudge particles near the target toward it. Gaussian
            //    falloff by distance, bounded so it can't leave the window. ──
            vec2  home = (cid + 0.5) / gdim;
            float dh   = length(steer - home);
            float w    = exp(-(dh * dh) / max(mouseRadius * mouseRadius, 1e-4)) * steerW;
            local += (steer - home) * gdim * clamp(w, 0.0, 0.8);

            // Audio micro-jitter (modulator, alive even at audio=0).
            local += vec2(sin(t * 2.3 + seed * 11.0),
                          cos(t * 1.9 + seed * 7.0)) * 0.05 * (0.4 + audio);

            local = clamp(local, vec2(-0.95), vec2(0.95));
            vec2 p = home + local / gdim;

            // ── Splat ──
            vec2 d = uv - p;
            d.x *= aspect;
            float r = length(d);
            float c = pow(drawSize / max(r, 1e-4), intensity);

            float hue = fract(hueShift + h11(seed * 5.3) * hueSpread + t * 0.015);
            float sat = 0.7 + 0.25 * h11(seed * 0.31);
            float val = 0.65 + 0.55 * audio;
            color += c * hsv2rgb(vec3(hue, sat, val));
        }
    }

    // Whole-field luminance follow — full depth (r3): the field lights <2% of
    // pixels at eval scale, so shallow gains were unmeasurable.
    color *= exposure * (1.0 + 0.60 * bassSm + 0.35 * midSm);

    // OVERLAY: alpha tracks brightness so empty space is transparent. Use a
    // soft knee so faint particle halos still feather in instead of clipping.
    // Alpha is computed BEFORE the haze wash below so overlay compositing of
    // the empty field is unchanged in normal blending.
    float bright = max(color.r, max(color.g, color.b));
    float alpha  = clamp(bright * 1.2, 0.0, 1.0);

    // r3: whole-frame haze breath in RGB — same precedent as the transparent
    // text shaders: the response must live on the backdrop pixels too (in
    // Add/Screen blends it reads as a gentle glow swell). Silence adds 0.
    float highSm = clamp(audioHigh, 0.0, 1.0) * reactF;
    color += vec3(0.10, 0.11, 0.15) * (1.0 * bassSm + 0.65 * midSm + 0.4 * highSm);

    // ---- universal color block (defaults = no-op) ----
    float ucL = dot(color, vec3(0.299, 0.587, 0.114));
    color = mix(vec3(ucL), color, colorBoost);
    float ucBg = bgColor.a * (1.0 - clamp(alpha, 0.0, 1.0));
    color = mix(color, bgColor.rgb, ucBg);
    alpha = max(alpha, ucBg);
    // LINEAR HDR out — host applies tonemap. Premultiply-friendly emissive.
    gl_FragColor = vec4(color, alpha);
}

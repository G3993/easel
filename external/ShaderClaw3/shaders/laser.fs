/*{
  "CATEGORIES": [
    "Generator"
  ],
  "INPUTS": [
    {
      "NAME": "intensity",
      "LABEL": "Intensity",
      "TYPE": "float",
      "DEFAULT": 0.005,
      "MIN": 0.001,
      "MAX": 0.05
    },
    {
      "NAME": "beamCount",
      "LABEL": "Beam Count",
      "TYPE": "long",
      "VALUES": [
        1,
        2,
        3,
        4,
        6,
        8,
        12
      ],
      "LABELS": [
        "1",
        "2",
        "3",
        "4",
        "6",
        "8",
        "12"
      ],
      "DEFAULT": 4,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "speed",
      "LABEL": "Speed",
      "TYPE": "float",
      "DEFAULT": 0.3,
      "MIN": 0,
      "MAX": 5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "color1",
      "LABEL": "Color A",
      "TYPE": "color",
      "DEFAULT": [
        1,
        1,
        1,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "color2",
      "LABEL": "Color B",
      "TYPE": "color",
      "DEFAULT": [
        1,
        1,
        1,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "color3",
      "LABEL": "Color C",
      "TYPE": "color",
      "DEFAULT": [
        1,
        0,
        0,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "hueShift",
      "LABEL": "Hue Shift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
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
      "NAME": "zoom",
      "LABEL": "Scale",
      "TYPE": "float",
      "DEFAULT": 4,
      "MIN": 0.2,
      "MAX": 4,
      "GROUP": "Camera / Layout"
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
    }
  ]
}*/

// RGB Laser Globe — by @paulofalcao

#ifdef GL_ES
precision highp float;
#endif

float makePoint(float x, float y, float fx, float fy, float sx, float sy, float t) {
   float xx = x * cos(t * fx);
   float yy = y * sin(t * fy);
   // Original beam recipe — preserved.
   float v = 1.0 / (sqrt(length(xx + yy) + length(xx * yy)));
   // Soft AA on the silhouette: feather the beam edge using screen-space
   // derivatives so HDR-bright cores don't pixelate when bloom samples them.
   float aa = fwidth(v) + 1e-4;
   float core = smoothstep(0.0, aa * 2.0, v - 0.15);
   return v * mix(0.85, 1.0, core);
}

// Beam recipe tables — column-per-channel (a/b/c). The original 9-beam
// design is preserved; lower beamCount values clamp the loop early.
const int MAX_BEAMS = 9;

float aFx(int i) {
   if (i == 0) return 3.3; if (i == 1) return 1.9; if (i == 2) return 0.8;
   if (i == 3) return 2.3; if (i == 4) return 0.8; if (i == 5) return 0.3;
   if (i == 6) return 1.4; if (i == 7) return 1.3; return 1.8;
}
float aFy(int i) {
   if (i == 0) return 2.9; if (i == 1) return 2.0; if (i == 2) return 0.7;
   if (i == 3) return 0.1; if (i == 4) return 1.7; if (i == 5) return 1.0;
   if (i == 6) return 1.7; if (i == 7) return 2.1; return 1.7;
}
float bFx(int i) {
   if (i == 0) return 1.2; if (i == 1) return 0.7; if (i == 2) return 1.4;
   if (i == 3) return 2.6; if (i == 4) return 0.7; if (i == 5) return 0.7;
   if (i == 6) return 0.8; if (i == 7) return 1.4; return 0.7;
}
float bFy(int i) {
   if (i == 0) return 1.9; if (i == 1) return 2.7; if (i == 2) return 0.6;
   if (i == 3) return 0.9; if (i == 4) return 1.4; if (i == 5) return 1.7;
   if (i == 6) return 0.5; if (i == 7) return 0.7; return 1.3;
}
float cFx(int i) {
   if (i == 0) return 3.7; if (i == 1) return 1.9; if (i == 2) return 0.8;
   if (i == 3) return 1.2; if (i == 4) return 0.3; if (i == 5) return 0.3;
   if (i == 6) return 1.4; if (i == 7) return 0.2; return 1.3;
}
float cFy(int i) {
   if (i == 0) return 0.3; if (i == 1) return 1.3; if (i == 2) return 0.9;
   if (i == 3) return 1.7; if (i == 4) return 0.6; if (i == 5) return 0.3;
   if (i == 6) return 0.8; if (i == 7) return 0.6; return 0.5;
}
float sxOf(int i) {
   if (i == 0) return 0.3; if (i == 1) return 0.4; if (i == 2) return 0.4;
   if (i == 3) return 0.6; if (i == 4) return 0.5; if (i == 5) return 0.4;
   if (i == 6) return 0.4; if (i == 7) return 0.6; return 0.5;
}
float syOf(int i) {
   if (i == 0) return 0.3; if (i == 1) return 0.4; if (i == 2) return 0.5;
   if (i == 3) return 0.3; if (i == 4) return 0.4; if (i == 5) return 0.4;
   if (i == 6) return 0.5; if (i == 7) return 0.3; return 0.4;
}

vec3 laserCluster(vec2 p, float t, int count) {
   float x = p.x;
   float y = p.y;

   // Clamp the requested beam count to the recipe table size.
   int n = count;
   if (n < 1) n = 1;
   if (n > MAX_BEAMS) n = MAX_BEAMS;

   float a = 0.0;
   float b = 0.0;
   float c = 0.0;

   // Fixed upper bound for GLSL ES; gated by `i < n` for the actual count.
   for (int i = 0; i < MAX_BEAMS; i++) {
      if (i >= n) break;
      a += makePoint(x, y, aFx(i), aFy(i), sxOf(i), syOf(i), t);
      b += makePoint(x, y, bFx(i), bFy(i), sxOf(i), syOf(i), t);
      c += makePoint(x, y, cFx(i), cFy(i), sxOf(i), syOf(i), t);
   }

   return a * color1.rgb + b * color2.rgb + c * color3.rgb;
}

// Map a 0–1 hand/mouse position into p-space
vec2 toCenter(vec2 pos) {
   return (pos * 2.0 - 1.0) * vec2(1.0, RENDERSIZE.y / RENDERSIZE.x) * zoom;
}

float laserKnee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

void main(void) {
   vec2 p = (gl_FragCoord.xy / RENDERSIZE.x) * 2.0 - vec2(1.0, RENDERSIZE.y / RENDERSIZE.x);
   p *= zoom;
   // Audio conditioning: low floor (knee from 0.04) + sub coupling so
   // hiphop's sparse sub-heavy kicks register; audioBeatPulse is a host-side
   // decaying event envelope, so each sparse hit leaves a visible trace.
   float bassP = pow(laserKnee(max(audioBass, audioSub), 0.04, 0.9), 1.3);
   float levelK = laserKnee(audioLevel, 0.04, 0.85);
   float hitT = audioBeatPulse;
   // Bass/hit breathe the globe scale (~±10%, geometric, never clamps).
   p /= 1.0 + 0.10 * bassP + 0.05 * hitT;
   // Was raw audioHigh*2.0 (up to 3x speed on any transient high-freq spike)
   // — soft knee + much smaller depth so it's a subtle lift, not a lurch.
   float highP = pow(laserKnee(audioHigh, 0.10, 0.90), 1.2);
   float t = TIME * speed * (1.0 + highP * 0.35);

   // Smooth sinusoidal drift — ease-in/ease-out parallax feel
   vec2 drift = 0.03 * vec2(sin(t * 0.17), cos(t * 0.23));

   // Primary cluster — follows mouse (Easel doesn't expose MediaPipe
   // hand-tracking; the original ShaderClaw3 build used mpHandCount /
   // mpHandPos2 here, which made GLSL compilation fail under Easel
   // because those uniforms are undeclared).
   int bc = int(beamCount);
   vec3 d = laserCluster(p - toCenter(mousePos) + drift, t, bc);

   // Secondary cluster — slow opposing drift so the globe always has a
   // second light source. Substitutes for the second-hand branch.
   vec2 alt = vec2(1.0 - mousePos.x, mousePos.y);
   d += laserCluster(p - toCenter(alt) - drift, t, bc) * 0.6;

   // Audio non-gating: alive at audio=0. Bass flare + continuous level
   // follow + a decaying per-hit trace (all smooth envelopes, no gates).
   float boost = 1.0 + bassP * 0.55 + levelK * 0.25 + hitT * 0.45;
   d *= intensity * boost;

   // HDR PEAKS for Phase Q v4 bloom — lift beam cores into 1.6–2.5 linear so
   // the post-bloom convolution gets real light-bleed off the brightest beams
   // without blowing past the bloom kernel's headroom. Soft knee preserves the
   // existing look in the lower range, then expands peaks geometrically.
   // Output is LINEAR HDR — no tonemap.
   float luma = max(max(d.r, d.g), d.b);
   float knee = 0.6;
   float over = max(luma - knee, 0.0);
   float hdrGain = 1.0 + over * 2.2;
   d *= hdrGain;

   // ---- universal color block (defaults = no-op) ----
   float ucL = dot(d, vec3(0.299, 0.587, 0.114));
   d = mix(vec3(ucL), d, colorBoost);
   if (hueShift > 0.0005) {
       float hA = hueShift * 6.2831853;
       float hC = cos(hA), hS = sin(hA);
       mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
               + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
               + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
       d = clamp(hM * d, 0.0, 1.0);
   }
   d = mix(d, bgColor.rgb, bgColor.a * (1.0 - smoothstep(0.0, 0.35, ucL)));
   gl_FragColor = vec4(d, 1.0);
}

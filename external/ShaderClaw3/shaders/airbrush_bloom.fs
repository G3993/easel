/*{
  "DESCRIPTION": "Airbrush Bloom — a single centered pinwheel flower of soft-3D capsule petals, each an airbrush gradient (hot pink rim, red core, blue/mint inner glow) on warm paper white. Bass pumps petal thickness, mids advance the spiral twist on an envelope-rate clock, highs sweep a satin sheen along the rims, beats breathe a soft full-bloom pulse. Premium airbrush print finish with static grain.",
  "CREDIT": "ShaderClaw — A-List batch 2",
  "CATEGORIES": ["Generator", "Audio"],
  "INPUTS": [
    { "NAME": "petalCount",  "LABEL": "Petal Count",   "TYPE": "float", "MIN": 8.0,  "MAX": 16.0, "DEFAULT": 14.0, "GROUP": "Shape / Geometry" },
    { "NAME": "petalLength", "LABEL": "Petal Length",  "TYPE": "float", "MIN": 0.45, "MAX": 0.78, "DEFAULT": 0.62, "GROUP": "Shape / Geometry" },
    { "NAME": "petalWidth",  "LABEL": "Petal Width",   "TYPE": "float", "MIN": 0.6,  "MAX": 1.4,  "DEFAULT": 1.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "twist",       "LABEL": "Spiral Twist",  "TYPE": "float", "MIN": 0.0,  "MAX": 1.5,  "DEFAULT": 0.85, "GROUP": "Shape / Geometry" },
    { "NAME": "spinSpeed",   "LABEL": "Spin Speed",    "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "breathe",     "LABEL": "Breathe",       "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Motion / Animation" },
    { "NAME": "rimColor",    "LABEL": "Rim Pink",      "TYPE": "color", "DEFAULT": [1.0, 0.16, 0.52, 1.0],  "GROUP": "Color" },
    { "NAME": "coreColor",   "LABEL": "Core Red",      "TYPE": "color", "DEFAULT": [0.87, 0.03, 0.14, 1.0], "GROUP": "Color" },
    { "NAME": "innerColor",  "LABEL": "Inner Glow",    "TYPE": "color", "DEFAULT": [0.24, 0.38, 0.94, 1.0], "GROUP": "Color" },
    { "NAME": "paletteShift","LABEL": "Palette Shift", "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "audioReact",  "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.35, "GROUP": "Audio Reactivity" }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// AIRBRUSH BLOOM — pinwheel flower of inflated capsule petals, airbrush
// print aesthetic (matched to "3d flower" A-List reference).
// Single-pass, memoryless audio response (a persistent phase accumulator was
// tried and measurably polluted the silence baseline in the harness):
//   bass  → petal thickness + bloom scale PUMP (level-proportional)
//   mids  → spiral twist winds tighter + per-petal rotational sway whose
//           AMPLITUDE is the mid level (content velocity ∝ envelope)
//   highs → satin sheen sweep intensity on the rims
//   beats → soft full-bloom pulse via audioBeatPulse (glides)
// Idle floor: silence = exactly the authored slow spin + breathing.
// ─────────────────────────────────────────────────────────────────────────

#define R   RENDERSIZE.xy
#define TAU 6.2831853

float hash11(float n) { return fract(sin(n) * 43758.5453123); }
float hash21(vec2 p)  { return fract(sin(dot(p, vec2(12.9898, 78.233))) * 43758.5453); }

vec3 hueRot(vec3 c, float a) {
    if (a < 0.0005) return c;
    float hC = cos(a), hS = sin(a);
    mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
            + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
            + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
    return clamp(c * hM, 0.0, 1.0);
}

void main() {
    float amt = audioReact;
    // Soft-knee band conditioning (playbook standard).
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.6);
    float midP  = pow(smoothstep(0.06, 0.85, audioMid),  1.2);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float beatP = clamp(audioBeatPulse, 0.0, 1.0);

    vec2 uv = (gl_FragCoord.xy * 2.0 - R) / R.y;

    // warm paper white, near-flat (reference bg is flat warm gray), static grain
    float grain = hash21(gl_FragCoord.xy) - 0.5;
    vec3 col = vec3(0.912, 0.897, 0.878);
    col *= 1.0 - 0.035 * smoothstep(0.75, 1.45, length(uv));

    // slow authored spin; bass + beats bloom the whole flower softly
    float rot   = TIME * 0.015 * spinSpeed;
    float bloom = 1.0 + amt * (0.05 * bassP + 0.06 * beatP);
    // mids wind the spiral tighter (level-proportional, smoothed by the bus)
    float twistEff = twist + amt * 0.40 * midP;

    float pShift = paletteShift * TAU;
    vec3 rimC  = hueRot(rimColor.rgb,  pShift);
    vec3 coreC = hueRot(coreColor.rgb, pShift);
    vec3 innC  = hueRot(innerColor.rgb, pShift);
    vec3 mintC = hueRot(mix(innerColor.rgb, vec3(0.55, 0.97, 0.78), 0.7), pShift);
    vec3 Ld = normalize(vec3(-0.38, 0.55, 0.74));

    float NP = clamp(petalCount, 8.0, 16.0);
    for (int i = 0; i < 16; i++) {
        if (float(i) >= NP) break;
        float fi = float(i);
        float hk = hash11(fi * 7.7);
        // per-petal rotational sway: sinusoidal carrier whose AMPLITUDE is
        // the mid level — per-frame motion tracks the envelope directly,
        // exactly zero in silence (playbook: content velocity ∝ envelope)
        float a  = rot + fi * TAU / NP
                 + amt * 0.30 * midP * sin(TIME * 1.9 + fi * 0.7);

        // per-petal breathing (idle life) + bass PUMP on thickness
        float br    = 1.0 + 0.024 * breathe * sin(TIME * 0.7 + fi * 0.9);
        float r1    = petalLength * bloom * br;
        float wHalf = 0.088 * petalWidth * bloom
                    * (1.0 + amt * 0.55 * bassP)
                    * (1.0 + 0.036 * breathe * sin(TIME * 0.9 + fi * 1.7));

        // capsule: outer tip radial, inner end rotated ahead → pinwheel swirl
        vec2 p1 = r1 * vec2(cos(a), sin(a));
        vec2 p0 = 0.085 * vec2(cos(a + twistEff * 1.6), sin(a + twistEff * 1.6));

        vec2 ba = p1 - p0;
        float bl = length(ba);
        vec2 bd  = ba / bl;
        vec2 pa  = uv - p0;
        float hp = clamp(dot(pa, bd) / bl, 0.0, 1.0);
        vec2 qv  = pa - bd * (hp * bl);
        float dperp = length(qv);
        float d = dperp - wHalf;
        if (d > 0.06) continue;

        float alpha   = smoothstep(0.010, -0.010, d);
        float haloOut = smoothstep(0.055, 0.0, d) * (1.0 - alpha);

        // ---- airbrush gradient: pink rim → red core → wide blue/mint pools ----
        float rr = clamp(-d / wHalf, 0.0, 1.0);       // 0 edge → 1 spine
        float tL = hp;                                 // 0 inner → 1 tip
        // two wide diffuse pools per petal (inner + outer), red between/at tip
        // very wide feather so pool edges melt (airbrush, not print blocks)
        float bandm    = sin(tL * 10.0 - 1.2 + hk * 0.6);
        float blueMask = smoothstep(-0.65, 0.75, bandm);
        vec3 innVar    = mix(innC, mintC, 0.30 + 0.35 * sin(tL * 5.0 + 1.9 + hk * 2.0));
        vec3 spineCol  = mix(coreC, innVar, blueMask);

        // wide glowing pink rim, soft feather into red, pools span the width
        vec3 pc = mix(clamp(rimC * 1.25 + 0.16, 0.0, 1.0), rimC, smoothstep(0.0, 0.20, rr));
        pc = mix(pc, coreC,    smoothstep(0.14, 0.48, rr));
        pc = mix(pc, spineCol, smoothstep(0.24, 0.95, rr));

        // ---- inflated-capsule 3D shading (cylindrical, strong) ----
        float sgn  = dot(qv, vec2(-bd.y, bd.x)) >= 0.0 ? 1.0 : -1.0;
        float sAbs = clamp(dperp / wHalf, 0.0, 1.0);
        vec2 n2 = vec2(-bd.y, bd.x) * sgn;
        vec3 N3 = normalize(vec3(n2 * sAbs, sqrt(max(1.0 - sAbs * sAbs, 0.0))));
        float lam = max(dot(N3, Ld), 0.0);
        pc *= 0.74 + 0.36 * lam;
        // shadow side sinks toward deep magenta — inflated print look
        pc = mix(pc * vec3(0.82, 0.55, 0.75) + vec3(0.06, 0.0, 0.05), pc,
                 smoothstep(0.0, 0.55, lam));

        // satin sheen sweep along the rim (highs fire it, always a whisper idle)
        float sweep = fract(TIME * 0.13 + hk * 0.35);
        float sheen = exp(-pow(tL - (sweep * 1.3 - 0.15), 2.0) / 0.016)
                    * smoothstep(0.45, 0.95, sAbs)
                    * (0.10 + amt * 1.4 * highP);
        pc += (rimC * 0.5 + 0.5) * sheen;

        pc += grain * 0.03;                            // airbrush tooth

        // contact shadow beneath, then paint the petal over
        col *= 1.0 - 0.15 * haloOut;
        col = mix(col, pc, alpha);
    }

    col += grain * 0.022;
    col = clamp(col, 0.0, 1.0);
    // gamma-style brightness: no slider position can white-out or black-out
    col = pow(col, vec3(1.0 / max(brightness, 0.3)));
    gl_FragColor = vec4(col, 1.0);
}

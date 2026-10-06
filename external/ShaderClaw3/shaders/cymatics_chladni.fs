/*{
  "CATEGORIES": [
    "Generator",
    "Audio Reactive",
    "Generative"
  ],
  "DESCRIPTION": "Cymatics — sand on a vibrating plate. True Chladni eigenmodes of a square plate (sin(mπx)sin(nπy) ± sin(nπx)sin(mπy)) driven by FFT bins: bass swells macro lobes, mids carve crosshatch, treble shatters into fine grids. Three-mode interference produces nodal lines (sand) and antinodes (sand-jumping fizz). Plate disc rotates with bevel and Tyndall haze drift outside.",
  "INPUTS": [
    {
      "NAME": "modeBass",
      "LABEL": "Bass Mode",
      "TYPE": "float",
      "MIN": 1,
      "MAX": 12,
      "DEFAULT": 3
    },
    {
      "NAME": "modeMid",
      "LABEL": "Mid Mode",
      "TYPE": "float",
      "MIN": 1,
      "MAX": 16,
      "DEFAULT": 7
    },
    {
      "NAME": "modeHigh",
      "LABEL": "Treble Mode",
      "TYPE": "float",
      "MIN": 1,
      "MAX": 24,
      "DEFAULT": 13
    },
    {
      "NAME": "sandAccumulation",
      "LABEL": "Sand Bias",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.55
    },
    {
      "NAME": "plateRadius",
      "LABEL": "Plate Radius",
      "TYPE": "float",
      "MIN": 0.3,
      "MAX": 0.55,
      "DEFAULT": 0.45,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "drift",
      "LABEL": "Mode Drift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.18,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "plateRotate",
      "LABEL": "Plate Rotate",
      "TYPE": "float",
      "MIN": -0.5,
      "MAX": 0.5,
      "DEFAULT": 0.04,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "sandColor",
      "LABEL": "Sand Color",
      "TYPE": "color",
      "DEFAULT": [
        0.94,
        0.87,
        0.71,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "drumColor",
      "LABEL": "Drum Color",
      "TYPE": "color",
      "DEFAULT": [
        0.04,
        0.05,
        0.07,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "hazeColor",
      "LABEL": "Haze Color",
      "TYPE": "color",
      "DEFAULT": [
        0.12,
        0.14,
        0.18,
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
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

#define PI 3.14159265359

float hash21(vec2 p) {
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453);
}

vec2 hash22(vec2 p) {
    return fract(sin(vec2(dot(p, vec2(127.1, 311.7)),
                          dot(p, vec2(269.5, 183.3)))) * 43758.5453);
}

// Real square-plate Chladni eigenfunction with antisymmetric pairing:
//   f(x,y) = sin(m·π·x)·sin(n·π·y) − sin(n·π·x)·sin(m·π·y)
// Nodal lines (zeros) are where sand settles. A small phase term lets
// modes precess so the figure breathes rather than freezing.
float chladni(vec2 uv, float m, float n, float phase) {
    float a = sin(m * PI * uv.x + phase * 0.5)
            * sin(n * PI * uv.y - phase * 0.3);
    float b = sin(n * PI * uv.x - phase * 0.6)
            * sin(m * PI * uv.y + phase * 0.4);
    return a - b;
}

// ---- universal color block (defaults = no-op) ----
vec3 ucGrade(vec3 uc) {
    float ucL = dot(uc, vec3(0.299, 0.587, 0.114));
    uc = mix(vec3(ucL), uc, colorBoost);                   // saturation
    if (hueShift > 0.0005) {                               // cheap hue rotate (YIQ)
        float hA = hueShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hM * uc, 0.0, 1.0);
    }
    return uc;
}

void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    float aspect = RENDERSIZE.x / max(RENDERSIZE.y, 1.0);

    // universal background: blend the haze (the scene's background) toward bgColor
    vec3 hzBG = mix(hazeColor.rgb, bgColor.rgb, bgColor.a);

    // ---- Plate-local coords (centred + rotated) ---------------------------
    vec2 pp = (uv - 0.5) * vec2(aspect, 1.0);
    float ang = TIME * plateRotate;
    float ca = cos(ang), sa = sin(ang);
    pp = vec2(ca * pp.x - sa * pp.y, sa * pp.x + ca * pp.y);

    float pr = length(pp);
    float plateMask = smoothstep(plateRadius, plateRadius - 0.012, pr);

    // ---- OUTSIDE THE PLATE: Tyndall haze + drifting motes ----------------
    if (plateMask < 0.001) {
        float vig = smoothstep(1.2, 0.4, length(uv - 0.5));
        vec3 outCol = mix(hzBG * 0.55, hzBG, vig);

        // Brownian drift: cells of motes shift with TIME, audioLevel pushes
        // particles harder so the air "thickens" with the music.
        float driftSpeed = 1.0 + audioLevel * audioReact * 3.0;
        vec2 hg = floor(uv * 240.0 + vec2(TIME * driftSpeed, -TIME * driftSpeed * 0.7));
        vec2 hr = hash22(hg);
        float p = step(0.997, hr.x);
        // Brownian micro-jitter inside each cell
        float jitter = step(0.5, fract(TIME * 2.0 + hr.y * 6.28));
        outCol += sandColor.rgb * p * (0.35 + jitter * 0.2);

        gl_FragColor = vec4(ucGrade(outCol), 1.0);
        return;
    }

    // ---- Re-normalise plate interior into [0,1] for eigenfunctions -------
    vec2 puv = pp / plateRadius * 0.5 + 0.5;
    puv = clamp(puv, 0.0, 1.0);

    // ---- Mode evolution: drift + audio amplification ---------------------
    // Bass → low (m,n) → big lobes. Mids → mid (m,n) → crosshatch.
    // Treble → high (m,n) → fine grids. TIME-only baseline keeps motion
    // alive when audio is silent; audio amplifies excursion.
    float mB = modeBass + drift * 1.6 * sin(TIME * 0.25)
                        + audioBass * audioReact * 2.0;
    float nB = modeBass + 1.0 + drift * 1.4 * cos(TIME * 0.19);
    float mM = modeMid  + drift * 1.8 * sin(TIME * 0.31 + 1.7)
                        + audioMid  * audioReact * 2.5;
    float nM = modeMid  + 2.0 + drift * 1.6 * cos(TIME * 0.27 + 0.9);
    float mH = modeHigh + drift * 2.4 * sin(TIME * 0.43 + 2.9)
                        + audioHigh * audioReact * 3.0;
    float nH = modeHigh + 3.0 + drift * 2.0 * cos(TIME * 0.37 + 2.1);

    // Degenerate guard — when m ≈ n the antisymmetric form collapses.
    if (abs(mB - nB) < 0.5) nB += 1.0;
    if (abs(mM - nM) < 0.5) nM += 1.0;
    if (abs(mH - nH) < 0.5) nH += 1.0;

    // Per-pixel jitter — sand vibrates more with mids
    vec2 jit = (hash22(puv * 800.0) - 0.5) * 0.0035 * (1.0 + audioMid * audioReact * 2.0);
    vec2 quv = puv + jit;

    // ---- Three-mode interference -----------------------------------------
    float aB = audioBass * audioReact;
    float aM = audioMid  * audioReact;
    float aH = audioHigh * audioReact;

    float c = 0.0;
    c += chladni(quv, mB, nB, TIME * 0.6) * (0.55 + aB * 1.1);
    c += chladni(quv, mM, nM, TIME * 0.9) * (0.40 + aM * 0.9);
    c += chladni(quv, mH, nH, TIME * 1.2) * (0.30 + aH * 0.8);
    // 4th gentle blend mode: average of bass+treble for richer interference
    c += chladni(quv, (mB + mH) * 0.5, (nB + nH) * 0.5, TIME * 0.45) * 0.18;
    c /= max(1.0 + (aB + aM + aH) * 0.35, 0.5);

    // Nodal lines = where field crosses zero. Sand piles there.
    float nodalDist = abs(c);
    // Sharper line for the crest, softer halo for accumulated piles.
    float nodal = exp(-nodalDist * 12.0);
    float halo  = exp(-nodalDist * 4.0) - nodal;

    // Anti-node "sand-jumping" zones — high |c|, sand thrown away.
    float antiNode = smoothstep(0.55, 0.95, abs(c));
    // Fizz: granular noise inside antinodes that scintillates with audio
    float fizz = hash21(gl_FragCoord.xy + floor(TIME * 30.0));
    fizz = step(0.85 - audioLevel * audioReact * 0.2, fizz) * antiNode;

    // ---- Sand piling: bias rises with audio ------------------------------
    float sandBias = sandAccumulation * (0.6 + audioLevel * audioReact * 0.6);
    float sandDensity = mix(nodal * 0.7, nodal, sandBias);
    sandDensity += halo * 0.35 * sandBias;

    // ---- Compose plate colour --------------------------------------------
    vec3 plate = drumColor.rgb;

    // 3D-ish plate shading: simulated tilt — far edge darker than near edge
    float tilt = dot(normalize(vec2(pp.x, pp.y) + 1e-5),
                     vec2(cos(TIME * 0.15), sin(TIME * 0.15)));
    plate *= 1.0 + tilt * 0.06;

    // Sand brightness lifts with audioLevel — louder = brighter sand
    float bright = 0.7 + audioLevel * audioReact * 0.9;
    vec3 col = mix(plate, sandColor.rgb * bright, sandDensity);

    // Antinode darkening (sand absent there)
    col *= 1.0 - antiNode * 0.22;
    // Antinode fizz — bright micro-sparks where sand jumps
    col += sandColor.rgb * fizz * 0.45;

    // ---- Plate bevel (rim lighting) --------------------------------------
    float bevelOuter = smoothstep(plateRadius - 0.022, plateRadius - 0.004, pr);
    float bevelInner = smoothstep(plateRadius - 0.004, plateRadius - 0.022, pr);
    col = mix(col, drumColor.rgb * 0.55, bevelOuter * 0.45);
    col += sandColor.rgb * 0.08 * bevelInner * smoothstep(plateRadius - 0.006, plateRadius - 0.002, pr);

    // Vignette into haze near plate rim
    float vig = smoothstep(plateRadius * 1.15, plateRadius * 0.55, pr);
    col = mix(hzBG, col, vig);

    // Sand-grain micro-texture
    float grain = (hash21(gl_FragCoord.xy * 0.5 + TIME * 0.1) - 0.5) * 0.05;
    col += grain * sandColor.rgb * sandDensity;

    // Final composite over haze background
    gl_FragColor = vec4(ucGrade(col * plateMask + hzBG * (1.0 - plateMask)), 1.0);
}

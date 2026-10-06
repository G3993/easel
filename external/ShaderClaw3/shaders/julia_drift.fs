/*{
  "DESCRIPTION": "Julia Drift — a living Julia set whose seed c orbits a smooth path, morphing the fractal forever; smooth-iteration color, breathing zoom, audio-reactive drift/fringe/pulse.",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "Fractal",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "fringeGlow",
      "LABEL": "Fringe Glow",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "inputImage",
      "LABEL": "Your Image",
      "TYPE": "image"
    },
    {
      "NAME": "texMix",
      "LABEL": "Image Amount",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "cRadius",
      "LABEL": "C Radius",
      "TYPE": "float",
      "DEFAULT": 0.74,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "maxI",
      "LABEL": "Max Iterations",
      "TYPE": "float",
      "DEFAULT": 180,
      "MIN": 60,
      "MAX": 256,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "cSpeed",
      "LABEL": "Morph Speed",
      "TYPE": "float",
      "DEFAULT": 0.06,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "paletteFreq",
      "LABEL": "Palette Frequency",
      "TYPE": "float",
      "DEFAULT": 0.045,
      "MIN": 0.005,
      "MAX": 0.2,
      "GROUP": "Color"
    },
    {
      "NAME": "paletteShift",
      "LABEL": "Palette Shift",
      "TYPE": "float",
      "DEFAULT": 0,
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
      "NAME": "zoom",
      "LABEL": "Zoom",
      "TYPE": "float",
      "DEFAULT": 1.4,
      "MIN": 0.4,
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
    },
    {
      "NAME": "audioReact",
      "LABEL": "Sound Reactivity",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

// Curated cosine palette (house style)
vec3 pal(float t){ return 0.5 + 0.5*cos(6.28318*(t + vec3(0.0, 0.33, 0.67))); }

void main() {
    // Live audio bands, soft-kneed: floor at ~0.04 so sparse/soft hits
    // (jazz swing, hiphop sub-kicks via audioSub coupling) register, top
    // headroom at 0.9+ so EDM's sustained bass keeps breathing instead of
    // pegging. audioBeatPulse is a decaying event envelope — soft accents
    // leave a visible fading trace (never a raw-gate snap).
    float bass   = pow(smoothstep(0.04, 0.95, audioBass + 0.5*audioSub), 1.2) * audioReact;
    float mid    = pow(smoothstep(0.04, 0.90, audioMid),  1.1) * audioReact;
    float treble = pow(smoothstep(0.03, 0.88, audioHigh), 1.1) * audioReact;
    float hit    = audioBeatPulse * audioReact;

    // Normalized, aspect-correct coordinates centered on screen
    vec2 uv = (gl_FragCoord.xy - 0.5*RENDERSIZE) / min(RENDERSIZE.x, RENDERSIZE.y);

    // Zoom breathes slowly; bass gently pulses it (positional, K<=0.6)
    float breathe = zoom * (1.0 + 0.06*sin(TIME*0.17));
    breathe *= (1.0 + bass*0.55 + hit*0.10);
    vec2 z = uv * breathe;

    // Seed c drifts along a smooth path — the living morph, sped a touch by mid
    float a = TIME*cSpeed + mid*0.8;
    float r = cRadius * (1.0 + 0.04*sin(TIME*0.23));
    vec2 c = r*vec2(cos(a), sin(a*1.3)) + vec2(-0.4, 0.0);

    // Iterate Julia with continuous (smooth) escape count
    float i = 0.0;
    int cap = int(clamp(maxI, 1.0, 256.0));
    for(int n=0; n<256; n++){
        if(n >= cap) break;
        z = vec2(z.x*z.x - z.y*z.y, 2.0*z.x*z.y) + c;
        if(dot(z,z) > 256.0) break;
        i += 1.0;
    }

    float d2 = dot(z,z);
    vec3 col;

    if(i >= float(cap) - 0.5){
        // Interior: deep near-black with a faint internal shimmer
        float shimmer = 0.012 + 0.010*sin(z.x*9.0 + z.y*7.0 + TIME*0.4);
        col = pal(paletteShift + 0.5) * max(shimmer, 0.0);
    } else {
        // Smooth iteration count (banding-free)
        float sm = i + 1.0 - log(log(max(d2, 1.0001))*0.5/log(2.0)) / log(2.0);

        // Color the exterior with the cosine palette
        col = pal(sm*paletteFreq + paletteShift + TIME*0.02);

        // User image colors the living Julia bands (default texMix 0 = pure palette)
        if (texMix > 0.0) {
            // Sample by the escaped-point direction, scrolled by the smooth iteration value
            vec2 tuv = normalize(z)*0.5 + 0.5;
            tuv = fract(tuv + vec2(sm*paletteFreq, sm*paletteFreq*0.5));
            vec3 img = texture2D(inputImage, tuv).rgb;
            col = mix(col, col*(0.4 + 1.6*img), texMix);
        }

        // Brighten the escape fringe — treble-reactive (additive, K<=0.6)
        float edge = exp(-abs(i - (float(cap)-1.0)) * 0.0);    // baseline 1.0
        float fringe = pow(clamp(1.0 - i/float(cap), 0.0, 1.0), 3.0);
        col += fringe * (fringeGlow * (0.35 + treble*0.6 + hit*0.45)) * pal(sm*paletteFreq + 0.15);

        // Gentle contrast falloff toward the set boundary keeps it deep
        col *= 0.55 + 0.6*clamp(i/float(cap), 0.0, 1.0);
    }

    // Tonemap + gamma (house style)
    col = col / (1.0 + col);
    col = pow(col, vec3(0.4545));

    // r2 jazz fix: whole-frame follower with LINEAR bands at full depth,
    // applied AFTER tonemap/gamma so the compression can't dilute it. The
    // kneed 'bass' above stays on geometry only. Soft jazz accents (0.4-0.5
    // beatPulse) clear a low 0.02 floor, never squared; walking mids ride a
    // linear mid follower. Silence = exactly 1.0.
    float bassL = smoothstep(0.02, 0.95, audioBass + 0.5*audioSub) * audioReact;
    float midL  = smoothstep(0.02, 0.95, audioMid) * audioReact;
    float hitL  = smoothstep(0.02, 0.80, audioBeatPulse) * audioReact;
    col *= 1.0 + 0.22*bassL + 0.14*midL + 0.28*hitL;

    // ---- universal color block (defaults = no-op) ----
    float ucL = dot(col, vec3(0.299, 0.587, 0.114));
    col = mix(vec3(ucL), col, colorBoost);
    col = mix(col, bgColor.rgb, bgColor.a * (1.0 - smoothstep(0.0, 0.35, ucL)));
    gl_FragColor = vec4(col, 1.0);
}

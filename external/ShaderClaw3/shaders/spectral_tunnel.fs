/*{
  "DESCRIPTION": "Spectral Tunnel — flight through a tunnel whose walls are carved from the music's spectral history: every beat leaves a ridge that recedes into the distance. Persistent FFT waterfall buffer sampled as tunnel displacement.",
  "CATEGORIES": [
    "Generator",
    "3D",
    "Audio Reactive"
  ],
  "CREDIT": "Etherea",
  "INPUTS": [
    {
      "NAME": "fogAmt",
      "LABEL": "Fog",
      "TYPE": "float",
      "DEFAULT": 0.75,
      "MIN": 0.2,
      "MAX": 1.5
    },
    {
      "NAME": "inputTex",
      "LABEL": "Texture",
      "TYPE": "image"
    },
    {
      "NAME": "texMix",
      "LABEL": "Texture Mix",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "ridgeAmt",
      "LABEL": "Ridge Depth",
      "TYPE": "float",
      "DEFAULT": 0.85,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "speed",
      "LABEL": "Flight Speed",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "twist",
      "LABEL": "Twist",
      "TYPE": "float",
      "DEFAULT": 0.35,
      "MIN": 0,
      "MAX": 1.5,
      "GROUP": "Motion / Animation"
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
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Audio Reactivity"
    }
  ],
  "PASSES": [
    {
      "TARGET": "histBuf",
      "PERSISTENT": true
    },
    {}
  ]
}*/

// ---- universal color block (defaults = no-op) ----
vec3 ucApply(vec3 uc) {
    float ucL = dot(uc, vec3(0.299, 0.587, 0.114));
    uc = mix(vec3(ucL), uc, colorBoost);                      // saturation
    if (hueShift > 0.0005) {                                  // cheap hue rotate (YIQ)
        float hA = hueShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hM * uc, 0.0, 1.0);
    }
    uc = mix(uc, bgColor.rgb, bgColor.a * (1.0 - smoothstep(0.0, 0.35, dot(uc, vec3(0.299, 0.587, 0.114)))));
    return uc;
}


// ── Spectral Tunnel ──────────────────────────────────────────
// Playbook technique: spectral history waterfall (persistent
// buffer, newest row at the top) sampled as tunnel-wall geometry;
// beats leave ridges that physically recede; camera breathes on
// bass; palette from the audio anchors. Idle: gentle noise ridges.

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }
float hash11(float p) { return fract(sin(p * 127.1) * 43758.5453); }

float fftLog(float t) {
    return texture2D(audioFFT, vec2(pow(clamp(t, 0.0, 1.0), 2.2) * 0.5, 0.5)).r;
}

void main() {
    float amt = clamp(audioReact, 0.0, 2.0);

    if (PASSINDEX == 0) {
        // ---- waterfall: scroll down one texel, write FFT into top row ------
        vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
        float texel = 1.0 / RENDERSIZE.y;
        if (uv.y > 1.0 - texel) {
            // newest row: conditioned log-frequency spectrum + idle shimmer
            float s = pow(fftLog(uv.x), 1.35) * amt;
            float idle = 0.05 + 0.04 * sin(TIME * 0.7 + uv.x * 21.0)
                              * sin(TIME * 0.41 + uv.x * 9.0);
            s = max(s, idle * 0.6);
            // beat rows burn brighter — ridges you can see coming
            s += audioBeatPulse * audioBeatPulse * amt * 0.35 * (1.0 - uv.x * 0.5);
            gl_FragColor = vec4(clamp(s, 0.0, 1.0), 0.0, 0.0, 1.0);
        } else {
            // scroll with a very slow decay so history fades over the ride
            float prev = texture2D(histBuf, vec2(uv.x, uv.y + texel)).r;
            gl_FragColor = vec4(prev * 0.9985, 0.0, 0.0, 1.0);
        }
        return;
    }

    // ---- final pass: ride the tunnel ---------------------------------------
    vec2 res = RENDERSIZE.xy;
    vec2 p = (gl_FragCoord.xy - 0.5 * res) / res.y;

    float t = TIME * speed;
    float bassP = pow(knee(audioBass, 0.05, 0.85), 1.6) * amt;
    float drive = 0.25 + 0.75 * knee(audioEnergy, 0.05, 0.9) * min(amt, 1.0);

    // camera: bass breathes the radius, bar phase sways the center
    vec2 sway = 0.10 * vec2(sin(t * 0.31), cos(t * 0.23))
              + 0.05 * vec2(sin(audioBarPhase * 6.2831), cos(audioBarPhase * 6.2831)) * amt;
    p -= sway;

    float ang = atan(p.y, p.x);
    float r = length(p);
    float twistAng = ang + twist * (1.5 / (r + 0.25)) * 0.3
                   + t * 0.15;

    // depth into the tunnel = history row (near = newest)
    float depth = (0.16 + 0.06 * bassP) / max(r, 0.02);       // bass widens the throat
    float fly = t * (0.35 + 0.65 * drive);                    // energy sets flight speed
    float hy = fract(1.0 - (depth * 0.06 + fly * 0.05));      // history coordinate

    // angle -> frequency (mirrored so it tiles seamlessly)
    float freq = abs(fract(twistAng / 6.2831 + 0.5) * 2.0 - 1.0);
    float hist = texture2D(histBuf, vec2(freq, hy)).r;

    // wall shading: ridges catch light, valleys fall to shadow
    float ridge = pow(hist, 1.2) * ridgeAmt;
    float texelY = 1.0 / RENDERSIZE.y;
    float histAhead = texture2D(histBuf, vec2(freq, fract(hy - texelY * 4.0))).r;
    float slope = clamp((hist - histAhead) * 8.0 + 0.5, 0.0, 1.0);

    float fog = exp(-depth * 0.55 * fogAmt);
    float wall = (0.15 + ridge * (0.6 + 0.4 * slope)) * fog;

    // rings: a faint depth grid so motion reads even in silence
    float rings = 0.06 * (0.5 + 0.5 * sin(depth * 9.0 - fly * 3.0)) * fog * drive;

    // palette from anchors; accent rides the ridges + beat
    float tone = clamp(wall * 1.6 + 0.1 * audioBrightness, 0.0, 1.0);
    vec3 col = (tone < 0.5)
        ? mix(audioPalShadow, audioPalMid, tone * 2.0)
        : mix(audioPalMid, audioPalHigh, tone * 2.0 - 1.0);
    col = mix(col, audioPalAccent, clamp(ridge * slope * 1.2, 0.0, 1.0) * 0.55);
    col += audioPalAccent * audioBeatPulse * audioBeatPulse * amt * 0.12 * fog;
    col += rings * audioPalMid;

    // optional texture wrap on the walls
    if (texMix > 0.001) {
        vec3 tex = texture2D(inputTex, vec2(freq, fract(hy * 4.0))).rgb;
        col = mix(col, col * (0.35 + 1.3 * tex), texMix * fog);
    }

    // center glow — the light at the end of the tunnel
    col += audioPalHigh * exp(-r * 7.0) * (0.25 + 0.45 * drive + 0.5 * bassP);

    // Continuous band-follow (ambient fix r2): whole-tunnel luminance breathes
    // with LINEAR smoothed bands — round 1 used bassP (pow-1.6 knee) which
    // crushed ambient's 0.1-0.8 swells to near-zero variance. No beat gating;
    // silence = exactly 1.0.
    col *= 1.0 + (0.28 * clamp(audioBass, 0.0, 1.0)
                + 0.16 * clamp(audioMid,  0.0, 1.0)) * min(amt, 1.0);

    col = ucApply(col);
    gl_FragColor = vec4(col, 1.0);
}

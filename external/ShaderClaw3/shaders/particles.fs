/*{
  "DESCRIPTION": "3D instanced billboard particles — rainbow HSL cloud orbiting in perspective",
  "CREDIT": "ShaderClaw",
  "CATEGORIES": [
    "Generator",
    "3D",
    "Particles"
  ],
  "INPUTS": [
    {
      "NAME": "brightness",
      "TYPE": "float",
      "DEFAULT": 0.6,
      "MIN": 0.1,
      "MAX": 1,
      "LABEL": "Brightness"
    },
    {
      "NAME": "particleCount",
      "TYPE": "float",
      "DEFAULT": 200,
      "MIN": 50,
      "MAX": 500,
      "LABEL": "Particles",
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "spread",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0.2,
      "MAX": 3,
      "LABEL": "Spread",
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "particleSize",
      "TYPE": "float",
      "DEFAULT": 0.04,
      "MIN": 0.005,
      "MAX": 0.15,
      "LABEL": "Size",
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "speed",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 4,
      "LABEL": "Speed",
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "rotateSpeed",
      "TYPE": "float",
      "DEFAULT": 0.3,
      "MIN": 0,
      "MAX": 2,
      "LABEL": "Camera Spin",
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "pulseAmount",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 1,
      "LABEL": "Pulse",
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "swirl",
      "LABEL": "Cloud Swirl",
      "TYPE": "float",
      "GROUP": "Motion / Animation",
      "DEFAULT": 0.0,
      "MIN": -2.0,
      "MAX": 2.0
    },
    {
      "NAME": "drift",
      "LABEL": "Particle Drift",
      "TYPE": "float",
      "GROUP": "Motion / Animation",
      "DEFAULT": 0.0,
      "MIN": 0.0,
      "MAX": 1.0
    },
    {
      "NAME": "pulseRate",
      "LABEL": "Pulse Rate",
      "TYPE": "float",
      "GROUP": "Motion / Animation",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 3.0
    },
    {
      "NAME": "cloudBreathe",
      "LABEL": "Cloud Breathe",
      "TYPE": "float",
      "GROUP": "Motion / Animation",
      "DEFAULT": 0.0,
      "MIN": 0.0,
      "MAX": 1.0
    },
    {
      "NAME": "hueShift",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1,
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
      "NAME": "fov",
      "TYPE": "float",
      "DEFAULT": 2,
      "MIN": 0.5,
      "MAX": 5,
      "LABEL": "FOV",
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "bgColor",
      "TYPE": "color",
      "DEFAULT": [
        0.02,
        0.02,
        0.04,
        1
      ],
      "LABEL": "Background",
      "GROUP": "Background"
    }
  ]
}*/

// Hash functions for deterministic particle positions
float hash(float n) { return fract(sin(n * 127.1) * 43758.5453); }
vec3 hash3(float n) {
    return fract(sin(vec3(n, n + 1.0, n + 2.0)) * vec3(43758.5453, 22578.1459, 19642.3490));
}

// HSL to RGB
vec3 hue2rgb(float h) {
    h = fract(h);
    float r = abs(h * 6.0 - 3.0) - 1.0;
    float g = 2.0 - abs(h * 6.0 - 2.0);
    float b = 2.0 - abs(h * 6.0 - 4.0);
    return clamp(vec3(r, g, b), 0.0, 1.0);
}

vec3 hsl2rgb(float h, float s, float l) {
    vec3 rgb = hue2rgb(h);
    float c = (1.0 - abs(2.0 * l - 1.0)) * s;
    return (rgb - 0.5) * c + l;
}

// Rotation matrix around Y axis
vec3 rotateY(vec3 p, float a) {
    float c = cos(a), s = sin(a);
    return vec3(c * p.x + s * p.z, p.y, -s * p.x + c * p.z);
}

// Rotation matrix around X axis
vec3 rotateX(vec3 p, float a) {
    float c = cos(a), s = sin(a);
    return vec3(p.x, c * p.y - s * p.z, s * p.y + c * p.z);
}

// Soft circle billboard
float billboard(vec2 uv, vec2 center, float radius) {
    float d = length(uv - center);
    return smoothstep(radius, radius * 0.3, d);
}

void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    float aspect = RENDERSIZE.x / RENDERSIZE.y;
    vec2 p = (uv - 0.5) * vec2(aspect, 1.0);

    // Soft-knee audio conditioning (playbook standard snippet)
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.6);
    float midP  = smoothstep(0.08, 0.85, audioMid);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float drive = 0.25 + 0.75 * smoothstep(0.05, 0.9, audioEnergy);

    // Time-warp clock — the cloud breathes with the track's energy
    float t = TIME * speed * (0.9 + 0.4 * (drive - 0.25));
    int N = int(particleCount);

    vec3 col = bgColor.rgb;

    // Camera orbits around origin
    float camAngleY = t * rotateSpeed * 0.5;
    float camAngleX = sin(t * rotateSpeed * 0.2) * 0.3;

    for (int i = 0; i < 240; i++) {   // mobile cap (was 500)
        if (i >= N) break;
        float fi = float(i);

        // Deterministic 3D position in [-1, 1] cube
        vec3 h3 = hash3(fi * 3.7);
        // MOTION: cloud breathes — spread expands / contracts on a slow cycle
        vec3 pos = (h3 * 2.0 - 1.0) * spread * (1.0 + cloudBreathe * 0.35 * sin(t * 0.4));
        // MOTION: per-particle lissajous drift (each on its own phase)
        pos += drift * 0.30 * vec3(sin(t * 0.70 + h3.x * 6.2832),
                                   cos(t * 0.50 + h3.y * 6.2832),
                                   sin(t * 0.60 + h3.z * 6.2832));
        // MOTION: internal swirl — particles orbit the cloud axis at radius-dependent rates
        pos = rotateY(pos, t * swirl * (0.4 + 0.8 * h3.y));

        // Sine-based pulsing (matching Three.js example)
        vec3 trTime = pos + t * pulseRate;   // MOTION: pulse rate
        float scale = sin(trTime.x * 2.1) + sin(trTime.y * 3.2) + sin(trTime.z * 4.3);
        // Mids deepen the per-particle pulse
        float sizeScale = mix(1.0, (scale * 0.5 + 1.0), pulseAmount * (1.0 + 0.30 * midP));

        // Rotate world by camera angle
        pos = rotateY(pos, camAngleY);
        pos = rotateX(pos, camAngleX);

        // Perspective projection
        float z = pos.z + fov;
        if (z < 0.1) continue;
        vec2 projected = pos.xy / z;

        // Screen-space size with perspective (bass swells the whole cloud)
        float sz = particleSize * sizeScale * (1.0 + 0.22 * bassP) / z;

        // Draw billboard
        float d = billboard(p, projected, sz);

        if (d > 0.0) {
            // HSL coloring based on scale (like Three.js example)
            float hue = scale / 5.0 + hueShift;
            vec3 particleCol = hsl2rgb(hue, 1.0, brightness);
            // Highs light up a sparse subset of particles
            particleCol *= 1.0 + 0.40 * highP * step(0.75, hash(fi * 17.3));

            // Depth fade — farther particles are dimmer
            float depthFade = smoothstep(fov + spread * 2.0, 0.5, z);

            col = mix(col, particleCol, d * depthFade);
        }
    }

    // Surprise: every ~18s a sudden swarm convergence — for ~0.8s all
    // particles bloom toward the screen center as if pulled by gravity.
    // Visualized as a bright radial pulse from middle.
    {
        vec2 _suv = gl_FragCoord.xy / RENDERSIZE;
        float _ph = fract(TIME / 18.0);
        float _f  = smoothstep(0.0, 0.04, _ph) * smoothstep(0.20, 0.10, _ph);
        float _r  = length(_suv - 0.5);
        float _pulse = exp(-_r * 8.0) * exp(-pow(_ph * 5.0 - 0.5, 2.0));
        col += vec3(0.7, 0.85, 1.0) * _pulse * _f * 1.2;
    }

    // ---- universal color block (defaults = no-op; hueShift/bgColor already native) ----
    float ucL = dot(col, vec3(0.299, 0.587, 0.114));
    col = mix(vec3(ucL), col, colorBoost);

    gl_FragColor = vec4(col, 1.0);
}

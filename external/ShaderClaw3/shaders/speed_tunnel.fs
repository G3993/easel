/*{
  "DESCRIPTION": "Speed Tunnel — the hyperspace jump: the screen is cut into radial slices and every slice fires a star trail that stretches from blue to white as you accelerate, layer after layer of them for density, until a lens flare blooms at the aim point and the trails whip past into a whiteout. Loops the jump on its own clock, or hold it mid-stretch for an endless warp. Bass lengthens the streaks, beats flash the flare, highs brighten the trails.",
  "CREDIT": "Simplified from Shadertoy MlKBWw (X-Wing Alliance hyperspace), lens flare after XdfXRX / 4sX3Rs. ShaderClaw port + parametrisation.",
  "CATEGORIES": ["Generator", "3D", "Tunnel", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "speed",        "LABEL": "Speed",            "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 4.0,  "GROUP": "Speed" },
    { "NAME": "loopTime",     "LABEL": "Jump Length (s)",  "TYPE": "float", "DEFAULT": 2.0,  "MIN": 0.5,  "MAX": 12.0, "GROUP": "Speed" },
    { "NAME": "jumpAt",       "LABEL": "Jump Point",       "TYPE": "float", "DEFAULT": 0.75, "MIN": 0.2,  "MAX": 0.95, "GROUP": "Speed" },
    { "NAME": "jumpSpeed",    "LABEL": "Jump Speed",       "TYPE": "float", "DEFAULT": 15.0, "MIN": 0.0,  "MAX": 40.0, "GROUP": "Speed" },
    { "NAME": "hold",         "LABEL": "Hold (endless)",   "TYPE": "float", "DEFAULT": 0.0,  "MIN": 0.0,  "MAX": 1.0,  "GROUP": "Speed" },
    { "NAME": "holdPoint",    "LABEL": "Hold Point",       "TYPE": "float", "DEFAULT": 0.55, "MIN": 0.05, "MAX": 0.95, "GROUP": "Speed" },
    { "NAME": "blueColor",    "LABEL": "Trail Start",      "TYPE": "color", "DEFAULT": [0.3, 0.3, 0.5, 1.0],   "GROUP": "Colors" },
    { "NAME": "whiteColor",   "LABEL": "Trail End",        "TYPE": "color", "DEFAULT": [0.85, 0.85, 0.9, 1.0], "GROUP": "Colors" },
    { "NAME": "flareColor",   "LABEL": "Flare",            "TYPE": "color", "DEFAULT": [0.9, 0.9, 1.4, 1.0],   "GROUP": "Colors" },
    { "NAME": "brightness",   "LABEL": "Brightness",       "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.2,  "MAX": 3.0,  "GROUP": "Colors" },
    { "NAME": "contrast",     "LABEL": "Contrast",         "TYPE": "float", "DEFAULT": 0.45, "MIN": 0.0,  "MAX": 1.0,  "GROUP": "Colors" },
    { "NAME": "slices",       "LABEL": "Slices",           "TYPE": "float", "DEFAULT": 125.0,"MIN": 20.0, "MAX": 300.0,"GROUP": "Trails" },
    { "NAME": "layers",       "LABEL": "Layers",           "TYPE": "float", "DEFAULT": 64.0, "MIN": 8.0,  "MAX": 120.0,"GROUP": "Trails" },
    { "NAME": "thickness",    "LABEL": "Thickness",        "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 4.0,  "GROUP": "Trails" },
    { "NAME": "glow",         "LABEL": "Glow",             "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 3.0,  "GROUP": "Trails" },
    { "NAME": "scatter",      "LABEL": "Scatter",          "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 2.0,  "GROUP": "Trails" },
    { "NAME": "trailSpeed",   "LABEL": "Trail Stretch",    "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 4.0,  "GROUP": "Trails" },
    { "NAME": "flareOn",      "LABEL": "Flare",            "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 2.0,  "GROUP": "Flare" },
    { "NAME": "whiteout",     "LABEL": "Whiteout",         "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 1.0,  "GROUP": "Flare" },
    { "NAME": "aimX",         "LABEL": "Aim X",            "TYPE": "float", "DEFAULT": 0.0,  "MIN": -1.0, "MAX": 1.0,  "GROUP": "Camera" },
    { "NAME": "aimY",         "LABEL": "Aim Y",            "TYPE": "float", "DEFAULT": -0.2, "MIN": -1.0, "MAX": 1.0,  "GROUP": "Camera" },
    { "NAME": "yaw",          "LABEL": "Yaw",              "TYPE": "float", "DEFAULT": 0.0,  "MIN": -1.5, "MAX": 1.5,  "GROUP": "Camera" },
    { "NAME": "pitch",        "LABEL": "Pitch",            "TYPE": "float", "DEFAULT": 0.0,  "MIN": -1.5, "MAX": 1.5,  "GROUP": "Camera" },
    { "NAME": "drift",        "LABEL": "Drift",            "TYPE": "float", "DEFAULT": 0.3,  "MIN": 0.0,  "MAX": 2.0,  "GROUP": "Camera" },
    { "NAME": "spin",         "LABEL": "Spin",             "TYPE": "float", "DEFAULT": 0.0,  "MIN": -2.0, "MAX": 2.0,  "GROUP": "Camera" },
    { "NAME": "audioReact",   "LABEL": "Audio React",      "TYPE": "float", "DEFAULT": 0.5,  "MIN": 0.0,  "MAX": 1.0,  "GROUP": "Audio Reactivity" },
    { "NAME": "bassResponse", "LABEL": "Bass → Stretch",   "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 3.0,  "GROUP": "Audio Reactivity" },
    { "NAME": "beatResponse", "LABEL": "Beat → Flare",     "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 3.0,  "GROUP": "Audio Reactivity" },
    { "NAME": "highResponse", "LABEL": "High → Bright",    "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 3.0,  "GROUP": "Audio Reactivity" }
  ]
}*/

#define TAU 6.28318531
#define MAX_LAYERS 120

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

float rand(vec2 co) { return fract(sin(dot(co.xy, vec2(12.9898, 78.233))) * 43758.5453); }

float sdLine(vec2 p, vec2 a, vec2 b, float ring) {
    vec2 pa = p - a, ba = b - a;
    float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
    return length(pa - ba * h) - ring;
}

// Lens flare after XdfXRX / 4sX3Rs
vec3 lensflare(vec3 uv, vec3 pos, float flareSize, float angOffset) {
    float z = uv.z / length(uv.xy);
    vec2 mainV = uv.xy - pos.xy;
    float dist = length(mainV);
    float numPoints = 2.71, diskSize = 0.2;
    float invSize = 1.0 / max(flareSize, 1e-4);
    float ang = atan(mainV.y, mainV.x) + angOffset;
    float fade = (z < 0.0) ? -z : 1.0;
    float f0 = 1.0 / (dist * invSize + 1.0);
    f0 = f0 + f0 * (0.1 * sin((sin(ang * 2.0 + pos.x) * 4.0 - cos(ang * 3.0 + pos.y)) * numPoints) + diskSize);
    if (z < 0.0) return clamp(mix(vec3(f0), vec3(0.0), 0.75 * fade), 0.0, 1.0);
    return vec3(f0);
}
vec3 cc(vec3 color, float factor, float factor2) {
    float w = color.x + color.y + color.z;
    return mix(color, vec3(w) * factor, w * factor2);
}

void main() {
    float ar = clamp(audioReact, 0.0, 1.0);
    float bassP = pow(knee(audioBass, 0.05, 0.85), 1.5) * bassResponse * ar;
    float highP = pow(knee(audioHigh, 0.10, 0.90), 1.2) * highResponse * ar;
    float beatP = clamp(audioBeatPulse, 0.0, 1.0) * beatResponse * ar;

    float T = TIME * speed;
    // normalised jump time: loops, or parks at holdPoint for an endless warp
    float tLoop = fract(T / max(loopTime, 0.1));
    float t = mix(tLoop, holdPoint, clamp(hold, 0.0, 1.0));

    vec2 fc = gl_FragCoord.xy;
    vec2 p = (2.0 * fc - RENDERSIZE.xy) / min(RENDERSIZE.x, RENDERSIZE.y);
    p += vec2(aimX, aimY);
    float sa = spin * T * 0.25;
    p = mat2(cos(sa), -sin(sa), sin(sa), cos(sa)) * p;

    // camera: slow wander on top of the dialled yaw / pitch
    float ay = yaw + drift * 0.25 * sin(T * 0.21);
    float ax = pitch + drift * 0.18 * sin(T * 0.17 + 1.3);
    mat3 mY = mat3(cos(ay), 0.0, sin(ay), 0.0, 1.0, 0.0, -sin(ay), 0.0, cos(ay));
    mat3 mX = mat3(1.0, 0.0, 0.0, 0.0, cos(ax), sin(ax), 0.0, -sin(ax), cos(ax));
    vec3 v = (mX * mY) * vec3(p, 1.0);

    vec3 color = vec3(0.0);
    float fade = clamp(mix(0.1, 1.1, t * 2.0), 0.0, 2.0);
    float stretch = trailSpeed * (1.0 + 0.8 * bassP);
    float jumpAmt = smoothstep(jumpAt, 1.0, t);
    float maxOff = 0.4 * scatter;
    float nLayers = floor(clamp(layers, 8.0, float(MAX_LAYERS)));
    float angBase = atan(v.y, v.x) / 3.141592 / 2.0;
    float zBase = v.z / length(v.xy);

    for (int k = 0; k < MAX_LAYERS; k++) {
        if (float(k) >= nLayers) break;
        float i = float(k);
        float angle = angBase + 0.13 * i;
        float slice = floor(angle * slices);
        float sliceFract = fract(angle * slices);
        float sliceOffset = maxOff * rand(vec2(slice, 4.0 + i * 25.0)) - (maxOff / 2.0);
        float dist = 10.0 * rand(vec2(slice, 1.0 + i * 10.0)) - 5.0;
        float z = dist * zBase;
        float f = sign(dist); if (f == 0.0) f = 1.0;
        float fspeed = f * (0.1 * rand(vec2(slice, 1.0 + i * 10.0)) + i * 0.01) * stretch;
        float trailStart = 10.0 * rand(vec2(slice, 0.0 + i * 10.0)) - 5.0;
        trailStart -= f * jumpSpeed * jumpAmt;
        float trailEnd = trailStart - t * fspeed;
        float trailX = smoothstep(trailStart, trailEnd, z);
        vec3 trailColor = mix(blueColor.rgb, whiteColor.rgb, trailX);
        float h = sdLine(vec2(sliceFract + sliceOffset, z), vec2(0.5, trailStart), vec2(0.5, trailEnd),
                         mix(0.0, 0.015, t * z) * thickness);
        float threshold = 0.09 * glow;
        h = (h < 0.01 * thickness) ? 1.0 : 0.85 * smoothstep(threshold, 0.0, abs(h));
        trailColor *= fade * h;
        color = max(color, trailColor);
    }
    color *= 1.0 + 0.5 * highP;

    if (flareOn > 0.0) {
        float flareSize = mix(0.0, 0.1, smoothstep(0.35, jumpAt + 0.2, t));
        flareSize += mix(0.0, 20.0, smoothstep(jumpAt + 0.05, 1.0, t));
        flareSize = (flareSize + 0.04 * beatP) * flareOn;
        vec3 flare = flareColor.rgb * lensflare(v, vec3(0.0), flareSize, t);
        color += cc(flare, 0.5, 0.1) * (1.0 + 1.5 * beatP);
    }
    color += whiteout * smoothstep(jumpAt + 0.1, 1.0, t);

    color *= brightness;
    color = clamp(color, 0.0, 1.0);
    color = mix(color, color * color * (3.0 - 2.0 * color), contrast);
    float l = dot(color, vec3(0.299, 0.587, 0.114));
    color = clamp(mix(vec3(l), color, 1.15), 0.0, 1.0);
    gl_FragColor = vec4(color, 1.0);
}

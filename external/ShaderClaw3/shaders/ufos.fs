/*{
  "DESCRIPTION": "UFOs — a swarm of colored point lights whirls on a modulated sphere, rendered by analytically integrating each light's falloff along every view ray (real volumetric glow, no marching). Light count swells and shrinks in waves; motion trails persist. Bass swells the swarm radius and intensity, mids drive the swarm population, highs shift the hues, loudness stretches the trails.",
  "CREDIT": "Analytical light integration by fenix (2023), CC BY-NC-SA 3.0, ShaderClaw audio port",
  "CATEGORIES": [
    "Generator",
    "3D"
  ],
  "INPUTS": [
    {
      "NAME": "speed",
      "LABEL": "Speed",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.1,
      "MAX": 3.0
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio React",
      "TYPE": "float",
      "GROUP": "Audio Reactivity",
      "DEFAULT": 0.5,
      "MIN": 0.0,
      "MAX": 1.0
    },
    {
      "NAME": "trails",
      "LABEL": "Trails",
      "TYPE": "float",
      "DEFAULT": 0.35,
      "MIN": 0.0,
      "MAX": 1.0
    },
    {
      "NAME": "tintColor",
      "LABEL": "Tint",
      "TYPE": "color",
      "GROUP": "Color",
      "DEFAULT": [1.0, 1.0, 1.0, 1.0]
    },
    {
      "NAME": "brightness",
      "LABEL": "Brightness",
      "TYPE": "float",
      "GROUP": "Color",
      "DEFAULT": 1.0,
      "MIN": 0.2,
      "MAX": 3.0
    }
  ],
  "PASSES": [
    {
      "TARGET": "trailBuf",
      "PERSISTENT": true
    },
    {}
  ]
}*/

#define PI 3.1415927
#define NUM_LIGHTS 8
#define MAX_NUM 96

#define SPHERE_RADIUS 1.0
#define LIGHT_INTENSITY 0.11
#define MIN_RADIUS 0.4
#define LOOPING_INTERVAL 10.0
#define BASE_SPEED 0.5

// camera
#define CAM_RO vec3(0.0, 1.0, 4.5)
#define CAM_FL 2.0

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

float hash11(float p) {
    p = fract(p * 0.1031);
    p *= p + 33.33;
    p *= p + p;
    return fract(p);
}

// from iq: https://www.shadertoy.com/view/MsS3Wc
vec3 hsv2rgb_smooth(in vec3 c) {
    vec3 rgb = clamp(abs(mod(c.x*6.0 + vec3(0.0,4.0,2.0), 6.0) - 3.0) - 1.0, 0.0, 1.0);
    rgb = rgb*rgb*(3.0 - 2.0*rgb);
    return c.z * mix(vec3(1.0), rgb, c.y);
}

vec3 tanh3(vec3 x) {
    vec3 e = exp(2.0 * clamp(x, -8.0, 8.0));
    return (e - 1.0) / (e + 1.0);
}

float gBass, gMid, gHigh, gLevel;
float gCenterDist;

vec3 rayDir(vec2 uv) {
    vec3 cf = normalize(vec3(0.0) - CAM_RO);
    vec3 cr = normalize(cross(cf, vec3(0,1,0)));
    vec3 cu = normalize(cross(cr, cf));
    return normalize(uv.x * cr + uv.y * cu + CAM_FL * cf);
}

vec3 lightPos(int i, float t) {
    // spherical coordinates; bass swells the whole swarm
    vec2 pc = vec2(BASE_SPEED * 3.0 * t + float(i) * 1.714,
                   BASE_SPEED * t + float(i) * 1.31);
    gCenterDist = sin(BASE_SPEED * 2.0 * t + float(i) * 3.311 * 1.37137) * 0.5 + 0.5;
    gCenterDist = gCenterDist * (1.0 - MIN_RADIUS) + MIN_RADIUS;
    gCenterDist *= SPHERE_RADIUS * (1.0 + 0.25 * audioReact * gBass);
    return gCenterDist * vec3(sin(pc.y) * sin(pc.x), cos(pc.y), sin(pc.y) * cos(pc.x));
}

vec3 traceLights(vec3 ro, vec3 rd, float t) {
    vec3 lightCol = vec3(0.0);

    // light population waves; mids feed the swarm
    float drive = (-cos(t * BASE_SPEED) * 0.5 + 0.5)
                * mix(1.0, 0.3 + 1.4 * gMid, audioReact);
    float numF = mix(float(NUM_LIGHTS), float(MAX_NUM), clamp(drive, 0.0, 0.85));
    int num = int(ceil(numF));

    for (int i = 0; i < MAX_NUM; i++) {
        if (i >= num) break;
        // marginal light fades in fractionally — no per-frame population pop
        float lw = clamp(numF - float(i), 0.0, 1.0);
        vec3 lp = lightPos(i, t);
        vec3 delta = lp - ro;
        float k = dot(delta, rd);                 // project onto ray
        float h = max(length(ro + k * rd - lp), 1e-4); // ray-to-light distance

        // integrate 1/d^2 along the ray: [0,k] plus [0,inf] past the light
        float integralInfluence = (atan(k / h)) / h;
        integralInfluence += 0.5 * PI / h;

        vec3 lc = hsv2rgb_smooth(vec3(
            fract(hash11(float(i)) + t / LOOPING_INTERVAL
                  + audioReact * 0.15 * gHigh),
            0.8, 1.0));
        lc *= 1.0 - exp(-(gCenterDist / SPHERE_RADIUS));
        lightCol += lc * integralInfluence * lw;
    }
    return lightCol * 2.0 / max(numF, 1.0);
}

vec4 passTrace() {
    float t = TIME * speed;
    vec2 uv = (2.0 * gl_FragCoord.xy - RENDERSIZE.xy) / RENDERSIZE.y;
    vec3 rd = rayDir(uv);

    vec3 lightCol = traceLights(CAM_RO, rd, t);
    lightCol *= LIGHT_INTENSITY * mix(1.0, 0.55 + 0.9 * gLevel + 0.35 * gBass, audioReact);
    lightCol = tanh3(lightCol);

    // trails: loudness stretches persistence
    float trailK = mix(trails, min(trails + 0.35, 1.0), audioReact * gLevel);
    float trailing = mix(0.002, 0.05, trailK);
    vec3 prevCol = texture2D(trailBuf, gl_FragCoord.xy / RENDERSIZE.xy).rgb;
    float dt = clamp(TIMEDELTA, 0.001, 0.1);
    lightCol = mix(lightCol, prevCol, exp(-dt / trailing));

    return vec4(lightCol, 1.0);
}

void main() {
    gBass  = pow(knee(audioBass, 0.05, 0.85), 1.6);
    gMid   = pow(knee(audioMid,  0.08, 0.90), 1.3);
    gHigh  = pow(knee(audioHigh, 0.10, 0.90), 1.2);
    gLevel = knee(audioLevel, 0.05, 0.90);

    if (PASSINDEX == 0) gl_FragColor = passTrace();
    else gl_FragColor = vec4(texture2D(trailBuf, gl_FragCoord.xy / RENDERSIZE.xy).rgb * tintColor.rgb * brightness, 1.0);
}

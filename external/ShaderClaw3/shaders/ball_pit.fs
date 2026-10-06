/*{
  "DESCRIPTION": "Ball Pit — balls tumble inside a rotating checkered box, lit by glowing emissive balls mixed into the pit that cast soft moving shadows. Ball color and the three light colors are user-controllable. Bass kicks the box spin and light intensity, mids agitate the balls, highs flicker the glow.",
  "CREDIT": "Re-authored from 'SSAO Rotating Ball Pit' by fenix (shadertoy cl23Ww), CC BY-NC-SA 3.0 — analytic raytrace port, ShaderClaw audio version",
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
      "NAME": "ballColor",
      "LABEL": "Ball Color",
      "TYPE": "color",
      "DEFAULT": [0.8, 0.8, 0.85, 1.0]
    },
    {
      "NAME": "lightColor1",
      "LABEL": "Light 1",
      "TYPE": "color",
      "DEFAULT": [1.0, 0.5, 0.1, 1.0]
    },
    {
      "NAME": "lightColor2",
      "LABEL": "Light 2",
      "TYPE": "color",
      "DEFAULT": [1.0, 0.3, 1.0, 1.0]
    },
    {
      "NAME": "lightColor3",
      "LABEL": "Light 3",
      "TYPE": "color",
      "DEFAULT": [0.0, 1.0, 0.5, 1.0]
    },
    {
      "NAME": "bgColor",
      "LABEL": "Background",
      "TYPE": "color",
      "GROUP": "Color",
      "DEFAULT": [1.0, 1.0, 1.0, 1.0]
    },
    {
      "NAME": "checkerAmt",
      "LABEL": "Checker",
      "TYPE": "float",
      "GROUP": "Color",
      "DEFAULT": 1.0,
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
  ]
}*/

#define N_BALLS 18
#define BALL_R 0.16
#define BOX_R 1.0
#define CAM_RO vec3(0.0, 0.8, 0.85)

float gBass, gMid, gHigh, gLevel;
vec3 gBall[N_BALLS];
mat3 gRot;

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

vec3 hash3(float n) {
    return fract(sin(vec3(n, n + 17.13, n + 41.71)) * vec3(43758.5453, 22578.1459, 19642.3490));
}

// every 5th ball is an emissive light (i = 0, 5, 10, 15)
bool isLight(int i) { return (i - (i / 5) * 5) == 0; }

vec3 lightTint(int i) {
    int pal = (i / 5) - ((i / 5) / 3) * 3;
    if (pal == 0) return lightColor1.rgb;
    if (pal == 1) return lightColor2.rgb;
    return lightColor3.rgb;
}

mat3 boxRotation(float t) {
    float swing = 0.15 * audioReact * gBass;
    float ax = t * 0.35 + swing;
    float ay = t * 0.27 + 0.7 * swing;
    float sx = sin(ax), cx = cos(ax);
    float sy = sin(ay), cy = cos(ay);
    mat3 rx = mat3(1, 0, 0, 0, cx, -sx, 0, sx, cx);
    mat3 ry = mat3(cy, 0, sy, 0, 1, 0, -sy, 0, cy);
    return rx * ry;
}

void buildScene(float t) {
    gRot = boxRotation(t);
    float agitate = 0.05 * mix(1.0, 0.5 + 1.2 * gMid, audioReact)
                  + 0.03 * audioReact * gLevel;
    for (int i = 0; i < N_BALLS; i++) {
        vec3 h = hash3(float(i) * 12.9898);
        vec3 base = (h * 2.0 - 1.0) * (BOX_R - BALL_R - 0.12);
        vec3 ph = hash3(float(i) * 7.31 + 3.7) * 6.28318;
        vec3 wob = vec3(sin(t * 0.8 + ph.x), sin(t * 0.6 + ph.y), sin(t * 0.7 + ph.z));
        vec3 pBox = base + wob * (0.06 + agitate);
        pBox = clamp(pBox, vec3(-(BOX_R - BALL_R - 0.02)), vec3(BOX_R - BALL_R - 0.02));
        gBall[i] = gRot * pBox;
    }
}

// https://iquilezles.org/articles/spherefunctions/
float sphIntersect(vec3 ro, vec3 rd, vec3 ce, float r) {
    vec3 oc = ro - ce;
    float b = dot(oc, rd);
    float c = dot(oc, oc) - r * r;
    float h = b * b - c;
    if (h < 0.0) return -1.0;
    return -b - sqrt(h);
}

float boxDist2(vec2 a, vec2 b) { return max(abs(a.x - b.x), abs(a.y - b.y)); }

float checker(vec2 p, float aa) {
    vec2 m = mod(p, vec2(2.0));
    float sd = min(boxDist2(vec2(0.5, 1.5), m), boxDist2(vec2(1.5, 0.5), m));
    return smoothstep(-aa, aa, 0.5 - sd) * 0.5 + 0.5;
}

// exit intersection with the rotated inside-out box; returns t, sets normal + uv
float boxExit(vec3 ro, vec3 rd, out vec3 nrm, out vec2 uv) {
    // to box space
    vec3 rob = ro * gRot;   // transpose multiply
    vec3 rdb = rd * gRot;
    vec3 s = sign(rdb + vec3(1e-6));
    vec3 tf = (s * BOX_R - rob) / (rdb + vec3(1e-9) * s);
    float t; vec3 nb; vec2 u;
    if (tf.x < tf.y && tf.x < tf.z) {
        t = tf.x; nb = vec3(-s.x, 0, 0);
        vec3 hp = rob + rdb * t; u = hp.yz;
    } else if (tf.y < tf.z) {
        t = tf.y; nb = vec3(0, -s.y, 0);
        vec3 hp = rob + rdb * t; u = hp.zx;
    } else {
        t = tf.z; nb = vec3(0, 0, -s.z);
        vec3 hp = rob + rdb * t; u = hp.xy;
    }
    nrm = gRot * nb;
    uv = u;
    return t;
}

// soft shadow of the balls along segment p -> light
float shadowTo(vec3 p, vec3 lp, int lightIdx, int selfIdx) {
    vec3 dl = lp - p;
    float dist = length(dl);
    vec3 ld = dl / dist;
    float occ = 1.0;
    for (int k = 0; k < N_BALLS; k++) {
        if (k == lightIdx || k == selfIdx) continue;
        vec3 oc = gBall[k] - p;
        float tp = clamp(dot(oc, ld), 0.0, dist);
        float c = length(oc - ld * tp);
        occ *= smoothstep(BALL_R * 0.5, BALL_R * 1.15, c);
    }
    return occ;
}

vec3 shadeSurface(vec3 p, vec3 n, vec3 albedo, int selfIdx, float t) {
    float ar = audioReact;
    float lightGain = mix(1.0, 0.5 + 0.55 * gLevel + 1.0 * gBass, ar);
    vec3 pixel = albedo * 0.025; // ambient floor so silence isn't black
    for (int j = 0; j < N_BALLS; j++) {
        if (!isLight(j)) continue;
        vec3 lp = gBall[j];
        vec3 dl = lp - p;
        float d2 = dot(dl, dl);
        float ndl = max(dot(n, normalize(dl)), 0.0);
        float occ = shadowTo(p, lp, j, selfIdx);
        float flick = 1.0 + ar * 0.35 * gHigh * (0.5 + 0.5 * sin(TIME * 9.0 + float(j) * 2.4));
        vec3 lc = lightTint(j) * lightGain * flick;
        pixel += lc * (occ * (ndl * ndl * 0.45 + 0.01) + 0.008) / (d2 * 4.0 + 0.15) * albedo;
    }
    return pixel;
}

// https://knarkowicz.wordpress.com/2016/01/06/aces-filmic-tone-mapping-curve/
vec3 ACESFilm(vec3 x) {
    return clamp((x * (2.51 * x + 0.03)) / (x * (2.43 * x + 0.59) + 0.14), 0.0, 1.0);
}

void main() {
    gBass  = pow(knee(audioBass, 0.05, 0.85), 1.6);
    gMid   = pow(knee(audioMid,  0.08, 0.90), 1.3);
    gHigh  = pow(knee(audioHigh, 0.10, 0.90), 1.2);
    gLevel = knee(audioLevel, 0.05, 0.90);

    float t = TIME * speed;
    buildScene(t);

    // camera
    vec3 ro = CAM_RO;
    vec3 fwd = normalize(vec3(0.0, -0.5, 0.0) - ro);
    vec3 left = -normalize(cross(fwd, vec3(0, 1, 0)));
    vec3 up = normalize(cross(left, fwd));
    vec2 sp = (gl_FragCoord.xy - 0.5 * RENDERSIZE.xy) / RENDERSIZE.y;
    vec3 rd = normalize(fwd - sp.x * left + sp.y * up);

    // trace: inside-out box walls vs balls
    vec3 bn; vec2 buv;
    float tHit = boxExit(ro, rd, bn, buv);
    vec3 n = bn;
    int hitBall = -1;

    for (int i = 0; i < N_BALLS; i++) {
        float ts = sphIntersect(ro, rd, gBall[i], BALL_R);
        if (ts > 0.0 && ts < tHit) {
            tHit = ts;
            hitBall = i;
            n = normalize(ro + rd * ts - gBall[i]);
        }
    }

    vec3 p = ro + rd * tHit;
    vec3 col;
    if (hitBall >= 0 && isLight(hitBall)) {
        // emissive ball
        float ar = audioReact;
        float flick = 1.0 + ar * 0.35 * gHigh * (0.5 + 0.5 * sin(TIME * 9.0 + float(hitBall) * 2.4));
        col = lightTint(hitBall) * mix(1.0, 0.5 + 0.55 * gLevel + 1.0 * gBass, ar) * flick * 1.4;
    } else if (hitBall >= 0) {
        vec3 shade = ballColor.rgb * mix(0.7, 1.0, hash3(float(hitBall) * 3.77).x);
        col = shadeSurface(p, n, shade, hitBall, t);
    } else {
        float aa = (1.2 + dot(bn, rd)) * 20.0 / RENDERSIZE.y;
        float ch = checker(buv * 4.25 - 0.5, aa);
        // walls: checker fades into a flat colored background
        vec3 wall = mix(bgColor.rgb, mix(vec3(1.0), vec3(0.5), ch) * bgColor.rgb, checkerAmt);
        col = shadeSurface(p, n, wall, -1, t);
    }

    // vignette (Ippokratis / lsKSWR)
    vec2 vuv = gl_FragCoord.xy / RENDERSIZE.xy;
    vuv *= 1.0 - vuv.yx;
    float vig = sqrt(vuv.x * vuv.y * 5.0);
    col *= vig;

    col *= mix(1.0, 0.75 + 0.7 * gLevel, audioReact);
    col = pow(ACESFilm(col * 2.2 * brightness), vec3(1.0 / 2.2));
    col *= tintColor.rgb;
    gl_FragColor = vec4(col, 1.0);
}

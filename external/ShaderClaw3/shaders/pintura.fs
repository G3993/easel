/*{
  "DESCRIPTION": "Pintura — multiscale paint fluid: Large-Eddy turbulence, vorticity confinement and a multiscale Poisson heightmap churn a canvas of thick liquid pigment, lit with GGX specular and cheap occlusion so it reads as embossed wet paint. Side pumps breathe with the bass, highs glint the specular, loudness deepens the contrast.",
  "CREDIT": "Multiscale fluid by Cornus Ammonis (2019) CC BY-NC-SA 3.0, stride-sampled ShaderClaw port (no mipmaps)",
  "CATEGORIES": [
    "Generator"
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
      "NAME": "paintGain",
      "LABEL": "Paint Gain",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.3,
      "MAX": 3.0
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
      "TARGET": "velBuf",
      "PERSISTENT": true
    },
    {
      "TARGET": "turbBuf",
      "PERSISTENT": true
    },
    {
      "TARGET": "confBuf",
      "PERSISTENT": true
    },
    {
      "TARGET": "poisBuf",
      "PERSISTENT": true
    },
    {}
  ]
}*/

#define SCALES 5
#define ADV_STEPS 3
#define PI 3.1415927

// tuning (from the original Common tab)
#define ADVECTION_SCALE 40.0
#define VELOCITY_CONFINEMENT 0.01
#define VELOCITY_LAPLACIAN 0.02
#define ADVECTION_CONFINEMENT 0.6
#define ADVECTION_VELOCITY -0.05
#define ADVECTION_TURBULENCE 1.0
#define DIVERGENCE_MIN 0.1
#define DAMPING 0.0001
#define TURB_ISOTROPY 0.9
#define CURL_ISOTROPY 0.6
#define CONF_ISOTROPY 0.25
#define POIS_ISOTROPY 0.16
#define PUMP_CYCLE 0.2

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

vec4 enc(vec4 v) { return clamp(v * 2.0 + 0.5, 0.0, 1.0); }
vec4 dec(vec4 e) { return (e - 0.5) * 0.5; }

vec2 normz(vec2 x) { return dot(x, x) < 1e-12 ? vec2(0.0) : normalize(x); }

float hash12(vec2 p) { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453123); }

float softclampf(float a, float b, float x, float k) {
    float mx = log(exp(k * a) + exp(k * x)) / k;
    return -log(exp(-k * b) + exp(-k * mx)) / k;
}
vec4 softclamp4(float a, float b, vec4 x, float k) {
    return vec4(softclampf(a, b, x.x, k), softclampf(a, b, x.y, k),
                softclampf(a, b, x.z, k), softclampf(a, b, x.w, k));
}

// stride-sampled 3x3 neighborhood at scale s (mip replacement)
#define WRAP(u) fract(u)

vec2 velAt(vec2 uv)  { return dec(texture2D(velBuf,  WRAP(uv))).xy; }
vec2 turbAt(vec2 uv) { return dec(texture2D(turbBuf, WRAP(uv))).xy; }
float curlAt(vec2 uv){ return dec(texture2D(turbBuf, WRAP(uv))).w; }
vec2 confAt(vec2 uv) { return dec(texture2D(confBuf, WRAP(uv))).xy; }
float poisAt(vec2 uv){ return dec(texture2D(poisBuf, WRAP(uv))).x; }

float reduce9(mat3 a, mat3 b) {
    mat3 p = matrixCompMult(a, b);
    return p[0][0] + p[0][1] + p[0][2] + p[1][0] + p[1][1] + p[1][2]
         + p[2][0] + p[2][1] + p[2][2];
}

// ---- pass 1: turbulence + multiscale curl ----------------------------------
vec4 passTurb() {
    vec2 texel = 1.0 / RENDERSIZE.xy;
    vec2 uv = gl_FragCoord.xy * texel;

    mat3 turb_xx = (2.0 - TURB_ISOTROPY) * mat3(0.125, 0.25, 0.125, -0.25, -0.5, -0.25, 0.125, 0.25, 0.125);
    mat3 turb_yy = (2.0 - TURB_ISOTROPY) * mat3(0.125, -0.25, 0.125, 0.25, -0.5, 0.25, 0.125, -0.25, 0.125);
    mat3 turb_xy = TURB_ISOTROPY * mat3(0.25, 0.0, -0.25, 0.0, 0.0, 0.0, -0.25, 0.0, 0.25);
    float c0 = CURL_ISOTROPY;
    float nrm = 8.8 / (4.0 + 8.0 * c0);
    mat3 curl_x = mat3(c0, 1.0, c0, 0.0, 0.0, 0.0, -c0, -1.0, -c0);
    mat3 curl_y = mat3(c0, 0.0, -c0, 1.0, 0.0, -1.0, c0, 0.0, -c0);

    vec2 v = vec2(0.0);
    float curl = 0.0, tw = 0.0, cw = 0.0;
    for (int i = 0; i < SCALES; i++) {
        float stride = exp2(float(i));
        vec4 t = stride * vec4(texel, -texel.y, 0);
        vec2 d    = velAt(uv + t.ww); vec2 d_n  = velAt(uv + t.wy); vec2 d_e = velAt(uv + t.xw);
        vec2 d_s  = velAt(uv + t.wz); vec2 d_w  = velAt(uv - t.xw); vec2 d_nw = velAt(uv - t.xz);
        vec2 d_sw = velAt(uv - t.xy); vec2 d_ne = velAt(uv + t.xy); vec2 d_se = velAt(uv + t.xz);
        mat3 mx = mat3(d_nw.x, d_n.x, d_ne.x, d_w.x, d.x, d_e.x, d_sw.x, d_s.x, d_se.x);
        mat3 my = mat3(d_nw.y, d_n.y, d_ne.y, d_w.y, d.y, d_e.y, d_sw.y, d_s.y, d_se.y);
        float curl_w = 1.0 / float(i + 1);
        v += vec2(reduce9(turb_xx, mx) + reduce9(turb_xy, my),
                  reduce9(turb_yy, my) + reduce9(turb_xy, mx));
        curl += curl_w * (reduce9(curl_x, mx) + reduce9(curl_y, my));
        tw += 1.0; cw += curl_w;
    }
    vec2 turb = float(SCALES) * v / tw;
    curl = nrm * curl / cw;
    if (FRAMEINDEX < 2) return enc(vec4(0.0));
    return enc(vec4(turb, 0.0, curl));
}

// ---- pass 2: vorticity confinement ------------------------------------------
vec4 passConf() {
    vec2 texel = 1.0 / RENDERSIZE.xy;
    vec2 uv = gl_FragCoord.xy * texel;
    float k0 = CONF_ISOTROPY, k1 = 1.0 - 2.0 * CONF_ISOTROPY;
    mat3 conf_x = mat3(-k0, -k1, -k0, 0.0, 0.0, 0.0, k0, k1, k0);
    mat3 conf_y = mat3(-k0, 0.0, k0, -k1, 0.0, k1, -k0, 0.0, k0);

    vec2 v = vec2(0.0);
    float wc = 0.0;
    for (int i = 0; i < SCALES; i++) {
        float stride = exp2(float(i));
        vec4 t = stride * vec4(texel, -texel.y, 0);
        float d    = abs(curlAt(uv + t.ww)); float d_n  = abs(curlAt(uv + t.wy));
        float d_e  = abs(curlAt(uv + t.xw)); float d_s  = abs(curlAt(uv + t.wz));
        float d_w  = abs(curlAt(uv - t.xw)); float d_nw = abs(curlAt(uv - t.xz));
        float d_sw = abs(curlAt(uv - t.xy)); float d_ne = abs(curlAt(uv + t.xy));
        float d_se = abs(curlAt(uv + t.xz));
        mat3 mc = mat3(d_nw, d_n, d_ne, d_w, d, d_e, d_sw, d_s, d_se);
        float curl = curlAt(uv);
        vec2 n = normz(vec2(reduce9(conf_x, mc), reduce9(conf_y, mc)));
        v += curl * n;
        wc += 1.0;
    }
    if (FRAMEINDEX < 2) return enc(vec4(0.0));
    return enc(vec4(v / wc, 0.0, 0.0));
}

// ---- pass 3: multiscale Poisson ---------------------------------------------
vec4 passPois() {
    vec2 texel = 1.0 / RENDERSIZE.xy;
    vec2 uv = gl_FragCoord.xy * texel;
    float k0 = POIS_ISOTROPY, k1 = 1.0 - 2.0 * POIS_ISOTROPY;
    mat3 pois_x = mat3(k0, 0.0, -k0, k1, 0.0, -k1, k0, 0.0, -k0);
    mat3 pois_y = mat3(-k0, -k1, -k0, 0.0, 0.0, 0.0, k0, k1, k0);
    mat3 gauss = mat3(0.0625, 0.125, 0.0625, 0.125, 0.25, 0.125, 0.0625, 0.125, 0.0625);

    vec2 v = vec2(0.0);
    float wc = 0.0;
    for (int i = 0; i < SCALES; i++) {
        float stride = exp2(float(i));
        vec4 t = stride * vec4(texel, -texel.y, 0);
        vec2 d    = velAt(uv + t.ww); vec2 d_n  = velAt(uv + t.wy); vec2 d_e = velAt(uv + t.xw);
        vec2 d_s  = velAt(uv + t.wz); vec2 d_w  = velAt(uv - t.xw); vec2 d_nw = velAt(uv - t.xz);
        vec2 d_sw = velAt(uv - t.xy); vec2 d_ne = velAt(uv + t.xy); vec2 d_se = velAt(uv + t.xz);
        float p    = poisAt(uv + t.ww); float p_n  = poisAt(uv + t.wy); float p_e = poisAt(uv + t.xw);
        float p_s  = poisAt(uv + t.wz); float p_w  = poisAt(uv - t.xw); float p_nw = poisAt(uv - t.xz);
        float p_sw = poisAt(uv - t.xy); float p_ne = poisAt(uv + t.xy); float p_se = poisAt(uv + t.xz);
        mat3 mx = mat3(d_nw.x, d_n.x, d_ne.x, d_w.x, d.x, d_e.x, d_sw.x, d_s.x, d_se.x);
        mat3 my = mat3(d_nw.y, d_n.y, d_ne.y, d_w.y, d.y, d_e.y, d_sw.y, d_s.y, d_se.y);
        mat3 mp = mat3(p_nw, p_n, p_ne, p_w, p, p_e, p_sw, p_s, p_se);
        float w = 1.0 / float(i + 1);
        wc += w;
        v += w * vec2(reduce9(pois_x, mx) + reduce9(pois_y, my), reduce9(gauss, mp));
    }
    float p = (v / wc).x + (v / wc).y;
    if (FRAMEINDEX < 2) p = 1e-6 * hash12(gl_FragCoord.xy);
    return enc(vec4(p, 0.0, 0.0, 0.0));
}

// ---- pass 0: velocity / advection update -------------------------------------
vec2 poisGrad(vec2 uv) {
    vec2 texel = 1.0 / RENDERSIZE.xy;
    vec4 t = vec4(texel, -texel.y, 0);
    float d_n  = poisAt(uv + t.wy); float d_e = poisAt(uv + t.xw);
    float d_s  = poisAt(uv + t.wz); float d_w = poisAt(uv - t.xw);
    float d_nw = poisAt(uv - t.xz); float d_sw = poisAt(uv - t.xy);
    float d_ne = poisAt(uv + t.xy); float d_se = poisAt(uv + t.xz);
    return vec2(0.5 * (d_e - d_w) + 0.25 * (d_ne - d_nw + d_se - d_sw),
                0.5 * (d_n - d_s) + 0.25 * (d_ne + d_nw - d_se - d_sw));
}

vec2 velLaplacian(vec2 uv) {
    const float K0 = -20.0 / 6.0, K1 = 4.0 / 6.0, K2 = 1.0 / 6.0;
    vec2 texel = 1.0 / RENDERSIZE.xy;
    vec4 t = vec4(texel, -texel.y, 0);
    vec2 d    = velAt(uv + t.ww); vec2 d_n  = velAt(uv + t.wy); vec2 d_e = velAt(uv + t.xw);
    vec2 d_s  = velAt(uv + t.wz); vec2 d_w  = velAt(uv - t.xw); vec2 d_nw = velAt(uv - t.xz);
    vec2 d_sw = velAt(uv - t.xy); vec2 d_ne = velAt(uv + t.xy); vec2 d_se = velAt(uv + t.xz);
    return K0 * d + K1 * (d_e + d_w + d_n + d_s) + K2 * (d_ne + d_nw + d_se + d_sw);
}

vec4 passVel() {
    vec2 tx = 1.0 / RENDERSIZE.xy;
    vec2 uv = gl_FragCoord.xy * tx;
    float T = TIME * speed;
    float ar = audioReact;
    float bassP = pow(knee(audioBass, 0.05, 0.85), 1.6);

    vec2 turb = turbAt(uv);
    vec2 confine = confAt(uv);
    vec2 vel = velAt(uv);
    vec2 div = vec2(0.0);

    vec2 offset = -ADVECTION_SCALE * (ADVECTION_VELOCITY * vel
                + ADVECTION_TURBULENCE * turb - ADVECTION_CONFINEMENT * confine);
    div = poisGrad(uv + tx * offset);
    vec2 lapl = velLaplacian(uv + tx * offset);
    vec2 delta_v = VELOCITY_LAPLACIAN * lapl + VELOCITY_CONFINEMENT * confine
                 - DAMPING * vel - DIVERGENCE_MIN * div;

    vec2 adv = vec2(0.0);
    for (int i = 0; i < ADV_STEPS; i++) {
        adv += velAt(uv + (float(i + 1) / float(ADV_STEPS)) * tx * offset);
    }
    adv /= float(ADV_STEPS);

    // side pumps breathe with bass (audio scales injection, not the field)
    float pumpScale = 0.02 * mix(1.0, 0.4 + 2.6 * bassP, ar);
    vec2 pq = 2.0 * (uv * 2.0 - 1.0) * vec2(1.0, tx.x / tx.y);
    vec2 pump = vec2(0.0);
    float uvy0 = exp(-50.0 * pq.y * pq.y);
    pump += -15.0 * vec2(max(0.0, cos(PUMP_CYCLE * T)) * pumpScale * exp(-50.0 * uv.x * uv.x) * uvy0, 0.0);
    pump += 15.0 * vec2(max(0.0, cos(PUMP_CYCLE * T + PI)) * pumpScale * exp(-50.0 * (1.0 - uv.x) * (1.0 - uv.x)) * uvy0, 0.0);
    float uvy2 = exp(-50.0 * pq.x * pq.x);
    pump += -15.0 * vec2(0.0, max(0.0, sin(PUMP_CYCLE * T)) * pumpScale * exp(-50.0 * uv.y * uv.y) * uvy2);
    pump += 15.0 * vec2(0.0, max(0.0, sin(PUMP_CYCLE * T + PI)) * pumpScale * exp(-50.0 * (1.0 - uv.y) * (1.0 - uv.y)) * uvy2);

    vec2 outV = adv + delta_v + pump;
    if (FRAMEINDEX < 2) outV = 0.15 * (vec2(hash12(gl_FragCoord.xy), hash12(gl_FragCoord.yx)) - 0.5);
    return enc(vec4(outV, 0.0, 0.0));
}

// ---- final: embossed paint render --------------------------------------------
// GGX from Noby's Goo shader (MIT)
float G1V(float dnv, float k) { return 1.0 / (dnv * (1.0 - k) + k); }
float ggx(vec3 n, vec3 v, vec3 l, float rough, float f0) {
    float alpha = rough * rough;
    vec3 h = normalize(v + l);
    float dnl = clamp(dot(n, l), 0.0, 1.0);
    float dnv = clamp(dot(n, v), 0.0, 1.0);
    float dnh = clamp(dot(n, h), 0.0, 1.0);
    float dlh = clamp(dot(l, h), 0.0, 1.0);
    float asqr = alpha * alpha;
    float den = dnh * dnh * (asqr - 1.0) + 1.0;
    float d = asqr / (PI * den * den);
    float f = f0 + (1.0 - f0) * pow(1.0 - dlh, 5.0);
    float vis = G1V(dnl, alpha) * G1V(dnv, alpha);
    return dnl * d * f * vis;
}

vec2 poisGradS(vec2 uv, float stride) {
    vec2 texel = stride / RENDERSIZE.xy;
    vec4 t = vec4(texel, -texel.y, 0);
    float d_n  = -poisAt(uv + t.wy); float d_e = -poisAt(uv + t.xw);
    float d_s  = -poisAt(uv + t.wz); float d_w = -poisAt(uv - t.xw);
    float d_nw = -poisAt(uv - t.xz); float d_sw = -poisAt(uv - t.xy);
    float d_ne = -poisAt(uv + t.xy); float d_se = -poisAt(uv + t.xz);
    return vec2(0.5 * (d_e - d_w) + 0.25 * (d_ne - d_nw + d_se - d_sw),
                0.5 * (d_n - d_s) + 0.25 * (d_ne + d_nw - d_se - d_sw));
}

vec4 passImage() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    float T = TIME * speed;
    float ar = audioReact;
    float highP  = pow(knee(audioHigh, 0.10, 0.90), 1.2);
    float levelP = knee(audioLevel, 0.05, 0.90);

    vec2 dxy = vec2(0.0);
    float occ = 0.0;
    float d0 = -poisAt(uv);
    for (int m = 1; m <= 6; m++) {
        float fm = float(m);
        dxy += (1.0 / pow(2.0, fm)) * poisGradS(uv, pow(2.0, fm - 1.0));
        float dm = -poisAt(uv);
        occ += softclampf(-2.0, 2.0, (d0 - dm), 1.0) / pow(1.5, fm);
    }
    dxy /= 6.0;
    occ = pow(max(0.0, softclampf(0.2, 0.8, 100.0 * occ + 0.5, 1.0)), 0.5);

    // bump lighting (Shane's technique)
    vec3 sp = vec3(uv - 0.5, 0.0);
    vec3 lightP = vec3(cos(T / 2.0) * 0.5, sin(T / 2.0) * 0.5, -0.5);
    vec3 ld = normalize(lightP - sp);
    vec3 avd = reflect(normalize(vec3(3200.0 * dxy, -1.0)), vec3(0, 1, 0));
    float spec = ggx(avd, vec3(0, 1, 0), ld, 0.1, 0.1);
    spec = (log(1001.0) / 1000.0) * log(1.0 + 1000.0 * spec);
    spec *= 1.0 + ar * 1.5 * highP;

    vec4 diffuse = softclamp4(0.0, 1.0, 6.0 * vec4(velAt(uv) * paintGain, 0.0, 0.0) + 0.5, 2.0);
    vec4 col = diffuse + 4.0 * mix(vec4(spec), 1.5 * diffuse * spec, 0.3);
    float contrast = 4.5 * mix(1.0, 0.8 + 0.5 * levelP, ar);
    col = mix(1.0, occ, 0.7) * softclamp4(0.0, 1.0, contrast * (col - 0.5) + 0.5, 3.0);
    return vec4(col.rgb * tintColor.rgb * brightness, 1.0);
}

void main() {
    if      (PASSINDEX == 0) gl_FragColor = passVel();
    else if (PASSINDEX == 1) gl_FragColor = passTurb();
    else if (PASSINDEX == 2) gl_FragColor = passConf();
    else if (PASSINDEX == 3) gl_FragColor = passPois();
    else                     gl_FragColor = passImage();
}

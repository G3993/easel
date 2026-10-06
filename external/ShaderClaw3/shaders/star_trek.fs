/*{
  "DESCRIPTION": "Star Trek — fly through a warping neon tunnel of 50 fluid-skinned capsules, each one a voice: audio FFT bands trigger per-capsule envelopes that fire comet lights down the tunnel and inject ink into a live fluid sim wrapped around every capsule. Bass and band energy drive the hits; idles gently in silence.",
  "CREDIT": "MIDI capsule-tunnel + compact fluid (XtGcDK) from Shadertoy, transparency march by Shane, ShaderClaw audio port",
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
      "DEFAULT": 0.6,
      "MIN": 0.0,
      "MAX": 1.0
    },
    {
      "NAME": "camHeight",
      "LABEL": "Camera Height",
      "TYPE": "float",
      "DEFAULT": 0.05,
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
      "TARGET": "envBuf",
      "PERSISTENT": true
    },
    {
      "TARGET": "fluidB"
    },
    {
      "TARGET": "fluidC"
    },
    {
      "TARGET": "fluidD",
      "PERSISTENT": true
    },
    {}
  ]
}*/

#define TAU 6.283189
#define PI 3.14159265358

#define GRID vec2(10.0, 5.0)
#define NUM_OBJECTS 50
#define CAP_LENGTH 2.0
#define TUNNEL_RADIUS 1.5
#define CAP_RADIUS 0.05
#define COMET_RADIUS 0.015

#define FLUID_FADE_OUT 0.999
#define LINE_LENGTH 0.04
#define TRAPEZOID vec2(0.4, 0.6)
#define FLUID_STRETCH vec2(8.0, 0.8)

#define TRANSPARENCY_PASSES 32.0

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }
// log-frequency FFT lookup — musical energy lives in the low bins
float fftLog(float t) { return texture2D(audioFFT, vec2(pow(t, 2.2) * 0.5, 0.5)).r; }

vec3 encodeSRGB(vec3 linearRGB) {
    vec3 a = 12.92 * linearRGB;
    vec3 b = 1.055 * pow(max(linearRGB, 0.0), vec3(1.0 / 2.4)) - 0.055;
    vec3 c = step(vec3(0.0031308), linearRGB);
    return mix(a, b, c);
}

float envelopeRamp(float time, float freq) { return 1.0 - fract(freq * time); }

vec3 hash31(float n) {
    return fract(sin(vec3(n, n + 1.0, n + 2.0)) * vec3(43758.5453123, 22578.1459123, 19642.3490423));
}

float trapezoid(float x, vec2 begin_end) {
    x = 1.0 - x;
    return min(smoothstep(0.0, begin_end.x, x), smoothstep(1.0, begin_end.y, x));
}

float ln(vec2 p, vec2 a, vec2 b) {
    return length(p - a - (b - a) * clamp(dot(p - a, b - a) / dot(b - a, b - a), 0.0, 1.0));
}

vec2 path_grid(float id) {
    vec2 sector = 1.0 / GRID;
    vec2 grid_pos = vec2(mod(id, GRID.x), floor(id / GRID.x));
    return grid_pos * sector + sector * 0.5;
}

float sdCapsule(vec3 p, vec3 a, vec3 b, float r) {
    vec3 pa = p - a, ba = b - a;
    float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
    return length(pa - ba * h) - r;
}

mat2 rotate(float angle) {
    return mat2(cos(angle), -sin(angle), sin(angle), cos(angle));
}

vec2 capsule_uv(vec3 p, float cap_length, float rota) {
    p.xz *= rotate(rota);
    float u = fract((atan(p.z, p.x) / 2.0) / PI + 0.5);
    float v = p.y / cap_length;
    return vec2(u, v);
}

mat3 cameraMat(vec3 ro, vec3 ta, float cr) {
    vec3 cw = normalize(ta - ro);
    vec3 cp = vec3(sin(cr), cos(cr), 0.0);
    vec3 cu = normalize(cross(cw, cp));
    vec3 cv = normalize(cross(cu, cw));
    return mat3(cu, cv, cw);
}

float slide(float cur, float tar, float slu, float sld) {
    float del = (tar > cur) ? slu : sld;
    cur += (tar - cur) * del;
    return cur;
}

mat2 radial_repeat(vec2 p, inout float id) {
    float angle = TAU / GRID.x;
    float radial_pos = atan(p.x, p.y) + angle * 0.5;
    float radial_sector = floor(radial_pos / angle);
    p.xy *= rotate(angle * 0.25);
    id = floor(((atan(p.y, p.x) + TAU * 0.5) / TAU) * GRID.x);
    return rotate(angle * radial_sector);
}

float depth_repeat(float z, inout float id) {
    id = mod(floor(abs(z)), GRID.y);
    return mod(z, 1.0) - 0.5;
}

vec3 light_color(int id) { return hash31(float(id) * 0.003 * 1000.0); }

vec2 hash_displace(float id) {
    return vec2(sin(id * 210.656) * 0.2, cos(id * 3020.121454) * 0.3);
}

// https://www.shadertoy.com/view/MscSDB
vec2 tunnel_path(float z) { float s = sin(z / 24.0) * cos(z / 16.0); return vec2(s * 9.0, 0.0); }

vec2 sphDistances(vec3 ro, vec3 rd, vec4 sph) {
    vec3 oc = ro - sph.xyz;
    float b = dot(oc, rd);
    float c = dot(oc, oc) - sph.w * sph.w;
    float h = b * b - c;
    float d = sqrt(max(0.0, sph.w * sph.w - h)) - sph.w;
    return vec2(d, -b - sqrt(max(h, 0.0)));
}

// envelope buffer is full-size; data lives in the bottom pixel row
float fetchEnv(int id) {
    return texture2D(envBuf, vec2((float(id) + 0.5) / RENDERSIZE.x, 0.5 / RENDERSIZE.y)).x;
}

// per-capsule state computed on demand (WebGL1 forbids dynamic array indexing)
vec3 capsulePos(int id) {
    float env = fetchEnv(id);
    float env_motion = 2.0 - pow(env, 1.5) * 1.8;
    vec3 pos = vec3(0.0, env_motion, 0.0) + vec3(0.0, TUNNEL_RADIUS, 0.0);
    pos.xz += hash_displace(float(id));
    return pos;
}

vec3 cometPos(int id, float time) {
    float angle = TAU / GRID.x;
    int id_depth = id / 10;
    int id_rad = id - id_depth * 10;
    vec3 upos = capsulePos(id);
    float z = time - mod(time + float(id_depth), GRID.y) - 0.5;
    upos.xy *= rotate(float(id_rad + 3) * angle);
    upos.z += z;
    upos.xy += tunnel_path(z);
    return upos;
}

// ---- fluid encode/decode: signed velocity+pressure round-trip 8-bit buffers --
#define FL_SCALE 8.0
vec4 flDecode(vec4 e) { return vec4((e.xyz - 0.5) * FL_SCALE, e.w); }
vec4 flEncode(vec4 v) { return vec4(clamp(v.xyz / FL_SCALE + 0.5, 0.0, 1.0), clamp(v.w, 0.0, 1.0)); }

// ============================ pass 0: envelopes ===============================
// x = envelope, y = idle trigger ramp, z = audio band follower
vec4 passEnv() {
    int x = int(gl_FragCoord.x);
    if (gl_FragCoord.x >= float(NUM_OBJECTS)) return vec4(0.0);

    float T = TIME * speed;
    float ar = audioReact;
    vec3 vfd = hash31(float(x));
    float freq = vfd.y * 0.2;
    float dur = 1.0 / (vfd.z * 55.0);

    vec4 prev = texture2D(envBuf, vec2(gl_FragCoord.x / RENDERSIZE.x, 0.5 / RENDERSIZE.y));
    if (FRAMEINDEX < 2) prev = vec4(0.0, 0.5, 0.0, 0.0);
    float prev_env = prev.x, prev_ramp = prev.y, prev_aenv = prev.z;

    // idle machine: hash-timed ramps keep the tunnel alive in silence
    float new_ramp = envelopeRamp(T, freq);
    float idleEnv = prev_env;
    if (new_ramp > prev_ramp) idleEnv = 1.0;
    else idleEnv = slide(idleEnv, 0.0, 1.0, dur);
    // quieter idles the louder / more reactive we are
    float presence = knee(audioLevel, 0.05, 0.9);
    idleEnv *= mix(1.0, 0.35, ar * presence);

    // audio machine: each capsule follows its own FFT band (peak-hold + decay)
    float band = fftLog(vfd.x);
    float aenv = max(prev_aenv * (1.0 - max(dur, 0.02)), pow(band, 1.4));

    float env = max(idleEnv, ar * aenv);
    return vec4(env, new_ramp, aenv, 1.0);
}

// ============================ fluid step (XtGcDK) =============================
vec4 fluidSample(sampler2D buf, vec2 U) {
    U.x = mod(U.x, RENDERSIZE.x - 1.0);
    return flDecode(texture2D(buf, U / RENDERSIZE.xy));
}

// one advection/pressure step; `inject` adds the envelope ink lines (pass B only)
vec4 fluidStep(sampler2D buf, vec2 fragCoord, bool inject) {
    vec2 R = RENDERSIZE.xy;
    vec2 U = fragCoord, A = fragCoord + vec2(1, 0), B = fragCoord + vec2(0, 1),
         C = fragCoord + vec2(-1, 0), D = fragCoord + vec2(0, -1);
    vec4 u = fluidSample(buf, U), a = fluidSample(buf, A), b = fluidSample(buf, B),
         c = fluidSample(buf, C), d = fluidSample(buf, D);
    vec4 p = vec4(0.0);
    vec2 g = vec2(0.0);
    for (int i = 0; i < 2; i++) {
        U -= u.xy; A -= a.xy; B -= b.xy; C -= c.xy; D -= d.xy;
        p += vec4(length(U - A), length(U - B), length(U - C), length(U - D)) - 1.0;
        g += vec2(a.z - c.z, b.z - d.z);
        u = fluidSample(buf, U); a = fluidSample(buf, A); b = fluidSample(buf, B);
        c = fluidSample(buf, C); d = fluidSample(buf, D);
    }
    vec4 Q = u;
    vec4 N = 0.25 * (a + b + c + d);
    Q.z = N.z;
    Q.xy -= g / 10.0 / 2.0;
    Q.z += (p.x + p.y + p.z + p.w) / 10.0;
    Q.z *= FLUID_FADE_OUT;

    if (inject) {
        vec2 sectorPx = R / GRID;
        vec2 frag = fragCoord;
        vec2 sectorIdx = floor(frag / sectorPx);
        if (sectorIdx.x < GRID.x && sectorIdx.y < GRID.y) {
            int id = int(sectorIdx.x + sectorIdx.y * GRID.x);
            float env = fetchEnv(id);
            if (env < 0.005) {
                Q = mix(Q, vec4(0.0), 0.5);
            } else {
                vec2 ahead = path_grid(float(id)) * R;
                vec2 behind = ahead - vec2(0.0, LINE_LENGTH) * R;
                float trapez = 1.0 - trapezoid(env, TRAPEZOID);
                float q = ln(frag, behind, ahead);
                vec2 m = behind - ahead;
                float l = length(m);
                if (env > 0.5 && l > 0.0) {
                    Q.xyw = mix(Q.xyw, vec3(-normalize(m) * min(l, 10.0) / 5.0, 1.0),
                                max(0.0, 4.0 * trapez - q) / 15.0);
                }
            }
        }
    }

    if (FRAMEINDEX < 2) Q = vec4(0.0);
    return flEncode(Q);
}

// ============================ image ==========================================
vec4 get_fluid_texture(float id, vec2 cap_uv) {
    vec2 sector_dim = 1.0 / GRID;
    vec2 grid_pos = vec2(mod(id, GRID.x), floor(id / GRID.x) + sector_dim.y * 0.5) * sector_dim;
    float stretch = 1.0 / FLUID_STRETCH.x;
    cap_uv.x = cap_uv.x * stretch + (1.0 - stretch) * 0.5;
    cap_uv *= sector_dim;
    cap_uv.y /= FLUID_STRETCH.y;
    cap_uv += grid_pos;
    return flDecode(texture2D(fluidD, cap_uv));
}

vec4 map(vec3 p, float time) {
    vec3 q = p;
    q.xy -= tunnel_path(q.z);
    float id_rad = -1.0;
    mat2 rot_mat = radial_repeat(q.xy, id_rad);
    q.xy *= rot_mat;
    float id_depth = 0.0;
    q.z = depth_repeat(q.z, id_depth);
    int id_line = int(id_depth * GRID.x + id_rad);
    id_line = int(clamp(float(id_line), 0.0, float(NUM_OBJECTS - 1)));
    vec3 front = capsulePos(id_line);
    float env = fetchEnv(id_line);
    vec3 back = front + vec3(0.0, CAP_LENGTH, 0.0);
    vec2 cap_uv = capsule_uv(q - front, CAP_LENGTH * 2.0, sin(TIME * 20.1) * (env + 0.1));
    vec4 fluid = get_fluid_texture(float(id_line), cap_uv);
    float bump = CAP_RADIUS * fluid.y * 1.1;
    float radius = CAP_RADIUS + bump;
    float cap = sdCapsule(q, front, back, radius);
    return vec4(cap * 0.7, float(id_line), cap_uv);
}

vec2 comet_distance(vec3 ro, vec3 rd, int id_line, float time) {
    vec3 pos = cometPos(id_line, time);
    vec2 com_distance = vec2(sphDistances(ro, rd, vec4(pos, 0.8)).x,
                             sphDistances(ro, rd, vec4(pos, CAP_LENGTH * 0.5)).x);
    com_distance.y = 1.0 - com_distance.y;
    return com_distance;
}

vec3 comet_lights(vec3 ro, vec3 rd, float time) {
    vec3 res = vec3(0.0);
    for (int id = 0; id < NUM_OBJECTS; id++) {
        vec3 pos = cometPos(id, time);
        float env = fetchEnv(id);
        float light_distance = sphDistances(ro, rd, vec4(pos, COMET_RADIUS)).x;
        float falloff = 0.52 * sqrt(env);
        float light_intensity = falloff / pow(max(abs(light_distance), 1e-4), 0.42);
        light_intensity = pow(light_intensity, 5.4545) * (1.0 - env);
        res += light_color(id) * light_intensity;
    }
    // bass flashes the whole comet field
    res *= 1.0 + 1.2 * audioReact * pow(knee(audioBass, 0.05, 0.85), 1.6);
    return res;
}

vec3 surfNormal(vec3 p, float time) {
    vec2 e = vec2(1.0, -1.0) * 0.5773 * 0.0005;
    return normalize(e.xyy * map(p + e.xyy, time).x +
                     e.yyx * map(p + e.yyx, time).x +
                     e.yxy * map(p + e.yxy, time).x +
                     e.xxx * map(p + e.xxx, time).x);
}

vec3 render(vec3 ro, vec3 rd, float time) {
    vec3 lig_pos = normalize(vec3(2.0, 1.0, 0.0));
    float ray_dist = length(rd);
    float thresh_dist = 0.05125;
    vec3 col = vec3(0.0);
    for (float i = 0.0; i < TRANSPARENCY_PASSES; i += 1.0) {
        if (dot(col, vec3(0.299, 0.587, 0.114)) > 1.0 || ray_dist > 26.0) break;
        vec3 p = ro + rd * ray_dist;
        vec4 obj = map(p, time);
        float hit = obj.x;
        float id = obj.y;
        vec2 cap_uv = obj.zw;
        float accum_dist = (thresh_dist - abs(hit) * 31.0 / 32.0) / thresh_dist;
        if (accum_dist > 0.0) {
            vec3 hp = ro + rd * hit;
            vec3 norm = surfNormal(hp, time) * sign(hit);
            vec3 single_col = max(0.0, dot(norm, lig_pos) * 0.5 + 0.5) * light_color(int(id)) * 2.0;
            vec4 fluid = get_fluid_texture(id, cap_uv + vec2(0.0, 0.1));
            vec2 comet_dist = comet_distance(ro, rd, int(id), time);
            col += single_col * 0.05 * accum_dist; // faint base glow keeps the tunnel readable
            if (comet_dist.y > 0.5 && fluid.w > 0.0) {
                col += max(vec3(0.0),
                    single_col * sin(fluid.w * 1.5 * fluid.x * 10.0 * fluid.y * 5.2 * fluid.z * vec3(1, 2, 3)));
            }
        }
        ray_dist += max(abs(hit) * 0.95, thresh_dist * 0.15);
    }
    col += comet_lights(ro, rd, time) * 1.5;
    return encodeSRGB(col);
}

vec4 passImage() {
    float time = -TIME * speed * 2.0;
    float cam_y = camHeight * 20.0;

    vec2 uv = (2.0 * gl_FragCoord.xy - RENDERSIZE.xy) / RENDERSIZE.y;
    vec3 lookat = vec3(0.0, 0.0, time);
    vec3 ro = vec3(0.0, 0.0, 1.0) + lookat;
    lookat.xy += tunnel_path(lookat.z);
    ro.xy += tunnel_path(ro.z);
    ro.y += cam_y;
    mat3 cam = cameraMat(ro, lookat, 0.0);
    float lens_aperture = 0.5 + 1.2 * cam_y / 15.0;
    vec3 rd = cam * normalize(vec3(uv, lens_aperture));
    vec3 col = render(ro, rd, time);
    col = pow(clamp(col, 0.0, 1.0), vec3(0.45));
    col *= 1.0 + audioReact * 0.35 * knee(audioLevel, 0.05, 0.9);
    col *= tintColor.rgb * brightness;
    return vec4(col, 1.0);
}

void main() {
    if      (PASSINDEX == 0) gl_FragColor = passEnv();
    else if (PASSINDEX == 1) gl_FragColor = fluidStep(fluidD, gl_FragCoord.xy, true);
    else if (PASSINDEX == 2) gl_FragColor = fluidStep(fluidB, gl_FragCoord.xy, false);
    else if (PASSINDEX == 3) gl_FragColor = fluidStep(fluidC, gl_FragCoord.xy, false);
    else                     gl_FragColor = passImage();
}

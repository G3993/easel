/*{
  "DESCRIPTION": "Cloud Sphere — a volumetric procedural cloud raymarched inside a 3D glass fisheye sphere with chromatic dispersion, neon rim, lens flare, audio reactivity, and rich color controls. Bass pulses the cloud density, mids warp the lens, highs energise the neon ring.",
  "CREDIT": "ShaderClaw × Shadertoy cloud fusion",
  "CATEGORIES": [
    "3D",
    "Effect",
    "Atmospheric",
    "Audio"
  ],
  "INPUTS": [
    {
      "NAME": "inputTex",
      "LABEL": "Source (optional)",
      "TYPE": "image"
    },
    {
      "NAME": "intensity",
      "LABEL": "Intensity",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 3
    },
    {
      "NAME": "ior",
      "LABEL": "IOR",
      "TYPE": "float",
      "DEFAULT": 1.45,
      "MIN": 1,
      "MAX": 2.5
    },
    {
      "NAME": "fresnel",
      "LABEL": "Fresnel",
      "TYPE": "float",
      "DEFAULT": 0.4,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "rimSoftness",
      "LABEL": "Rim Softness",
      "TYPE": "float",
      "DEFAULT": 0.008,
      "MIN": 0,
      "MAX": 0.08
    },
    {
      "NAME": "rimGlow",
      "LABEL": "Rim Glow",
      "TYPE": "float",
      "DEFAULT": 0.3,
      "MIN": 0,
      "MAX": 1.5
    },
    {
      "NAME": "neonIntensity",
      "LABEL": "Neon Ring",
      "TYPE": "float",
      "DEFAULT": 1.2,
      "MIN": 0,
      "MAX": 4
    },
    {
      "NAME": "neonBloom",
      "LABEL": "Neon Bloom",
      "TYPE": "float",
      "DEFAULT": 0.1,
      "MIN": 0.01,
      "MAX": 0.6
    },
    {
      "NAME": "flare",
      "LABEL": "Lens Flare",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 2
    },
    {
      "NAME": "shadowStrength",
      "LABEL": "Shadow",
      "TYPE": "float",
      "DEFAULT": 0.45,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "cloudScale",
      "LABEL": "Cloud Scale",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0.1,
      "MAX": 3,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "density",
      "LABEL": "Cloud Density",
      "TYPE": "float",
      "DEFAULT": 28,
      "MIN": 4,
      "MAX": 72,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "plasmaAmt",
      "LABEL": "Plasma Flame",
      "TYPE": "float",
      "DEFAULT": 0.55,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "lensSize",
      "LABEL": "Lens Size",
      "TYPE": "float",
      "DEFAULT": 0.88,
      "MIN": 0.3,
      "MAX": 1.3,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "neonWidth",
      "LABEL": "Neon Width",
      "TYPE": "float",
      "DEFAULT": 0.012,
      "MIN": 0.002,
      "MAX": 0.08,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "shadowOffsetX",
      "LABEL": "Shadow X",
      "TYPE": "float",
      "DEFAULT": 0.06,
      "MIN": -1,
      "MAX": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "shadowOffsetY",
      "LABEL": "Shadow Y",
      "TYPE": "float",
      "DEFAULT": -0.09,
      "MIN": -1,
      "MAX": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "speed",
      "LABEL": "Speed",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 4,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "rotSpeed",
      "LABEL": "Cloud Rotation",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "colorSpeed",
      "LABEL": "Color Drift",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 4,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "spin",
      "LABEL": "Spin Speed",
      "TYPE": "float",
      "DEFAULT": 0.08,
      "MIN": -2,
      "MAX": 2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "neonPulse",
      "LABEL": "Neon Pulse",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "chromatic",
      "LABEL": "Chromatic Split",
      "TYPE": "float",
      "DEFAULT": 0.05,
      "MIN": 0,
      "MAX": 0.25,
      "GROUP": "Color"
    },
    {
      "NAME": "neonColor",
      "LABEL": "Neon Color",
      "TYPE": "color",
      "DEFAULT": [
        0.2,
        0.7,
        1,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "cloudColorA",
      "LABEL": "Cloud Color A",
      "TYPE": "color",
      "DEFAULT": [
        0.9,
        0.95,
        1,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "cloudColorB",
      "LABEL": "Cloud Color B",
      "TYPE": "color",
      "DEFAULT": [
        0.3,
        0.5,
        0.9,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "flareColor",
      "LABEL": "Flare Color",
      "TYPE": "color",
      "DEFAULT": [
        1,
        0.92,
        0.78,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "hueShift",
      "LABEL": "Hue Shift",
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
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Color"
    },
    {
      "NAME": "perspective",
      "LABEL": "Perspective",
      "TYPE": "float",
      "DEFAULT": 0.18,
      "MIN": 0,
      "MAX": 1.5,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "flareAngle",
      "LABEL": "Flare Angle",
      "TYPE": "float",
      "DEFAULT": -0.6,
      "MIN": -3.14159,
      "MAX": 3.14159,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "bgColor",
      "LABEL": "Sky / BG",
      "TYPE": "color",
      "DEFAULT": [
        0.04,
        0.05,
        0.12,
        1
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "transparentBg",
      "LABEL": "Transparent BG",
      "TYPE": "bool",
      "DEFAULT": false,
      "GROUP": "Background"
    },
    {
      "NAME": "reactivity",
      "LABEL": "Audio Reactivity",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 3,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

// ═══════════════════════════════════════════════════════
//  UTILITY
// ═══════════════════════════════════════════════════════

#define PI 3.14159265

float sat(float v){ return clamp(v, 0.0, 1.0); }

vec3 hsv2rgb(vec3 c){
    vec4 K = vec4(1.0, 2.0/3.0, 1.0/3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz)*6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

// ═══════════════════════════════════════════════════════
//  PROCEDURAL 3-D NOISE  (value noise fBm)
// ═══════════════════════════════════════════════════════

float h31(vec3 p){
    p = fract(p * 0.1031);
    p += dot(p, p.zyx + 31.32);
    return fract((p.x + p.y) * p.z);
}
float vnoise3(vec3 p){
    vec3 i = floor(p), f = fract(p);
    f = f*f*(3.0 - 2.0*f);
    float n000 = h31(i+vec3(0,0,0)), n100 = h31(i+vec3(1,0,0));
    float n010 = h31(i+vec3(0,1,0)), n110 = h31(i+vec3(1,1,0));
    float n001 = h31(i+vec3(0,0,1)), n101 = h31(i+vec3(1,0,1));
    float n011 = h31(i+vec3(0,1,1)), n111 = h31(i+vec3(1,1,1));
    return mix(mix(mix(n000,n100,f.x),mix(n010,n110,f.x),f.y),
               mix(mix(n001,n101,f.x),mix(n011,n111,f.x),f.y),f.z);
}

const mat3 NOISE_ROT = mat3(0.00,1.60,1.20,-1.60,0.72,-0.96,-1.20,-0.96,1.28);

float cloudNoise(vec3 p){
    vec3 cp = p * 0.5;
    float v = 0.0, w = 1.0, tw = 0.0;
    for(int i = 0; i < 5; i++){
        v  += (vnoise3(cp)*2.0 - 1.0)*w;
        tw += w; w *= 0.5;
        cp  = NOISE_ROT * cp;
    }
    return v / tw;
}

float cloudDensity(vec3 p){
    float v = cloudNoise(p);
    float core = 1.0 - sat(length(p - vec3(0.5))*2.0);
    core = core*core;
    v = (v + core)*core;
    v = sat(v);
    v = 1.0 - pow(1.0 - v, 2.0);
    v = smoothstep(0.0, 0.2, pow(v, 1.5));
    return v;
}

float sampleCloud(vec3 p){ return cloudDensity(p + vec3(0.5)); }

// ═══════════════════════════════════════════════════════
//  CLOUD ROTATION
// ═══════════════════════════════════════════════════════

mat3 getCloudRot(float centerDist, float t){
    float dal = mix(0.0, 1.5, pow(sat(centerDist), 0.5));
    float aX = pow(sin(t*4.0/3.0 - dal*1.5), 2.0)*1.8763*rotSpeed;
    float aY = pow(sin(t*5.0/3.0 - dal*3.5), 2.0)*3.3154*rotSpeed;
    float aZ = smoothstep(0.0,1.0,pow(sin(t*8.0/3.0 - dal*2.5),2.0))*2.5123*rotSpeed;
    float cx=cos(aX),sx=sin(aX),cy=cos(aY),sy=sin(aY),cz=cos(aZ),sz=sin(aZ);
    mat3 rX = mat3(1,0,0, 0,cx,-sx, 0,sx,cx);
    mat3 rY = mat3(cy,0,sy, 0,1,0, -sy,0,cy);
    mat3 rZ = mat3(cz,-sz,0, sz,cz,0, 0,0,1);
    return rX*rY*rZ;
}

// ═══════════════════════════════════════════════════════
//  BOX / SHADOW HELPERS
// ═══════════════════════════════════════════════════════

vec2 boxIntersect(vec3 ro, vec3 rd, vec3 boxSize){
    vec3 m = 1.0/rd;
    vec3 n = m*ro;
    vec3 k = abs(m)*boxSize;
    vec3 t1 = -n-k, t2 = -n+k;
    float tN = max(max(t1.x,t1.y),t1.z);
    float tF = min(min(t2.x,t2.y),t2.z);
    if(tN > tF || tF < 0.0) return vec2(-1.0);
    return vec2(tN, tF);
}

const vec3 BBOX = vec3(0.375);           // 0.75*0.5
const vec3 SUN  = vec3(0.7,-1.0,-0.4);  // unnormalised; normalised at use
const float SHADOW_STEP0 = 0.0025;
const float SHADOW_SCALAR = 1.15;
const vec3  AMBIENT_D = vec3(0.14,0.13,0.1)*3.0;

float shadowStep(vec3 p, float jitter, float densityScale){
    vec3 sd = -normalize(SUN);
    vec2 box = boxIntersect(p, sd, BBOX);
    if(box.y < 0.0) return 0.0;
    float ss = SHADOW_STEP0;
    float d  = jitter*ss*0.125;
    float alpha = 0.0;
    bool inside = true;
    for(int i = 0; i < 32; i++){
        if(!inside) break;
        d += ss;
        float over = max(0.0, d - box.y);
        float cur  = ss - over;
        alpha += sampleCloud(p - d*sd)*cur*densityScale;
        inside = (over == 0.0);
        ss *= SHADOW_SCALAR;
    }
    return alpha;
}

#define CLOUD_STEPS 56

vec4 renderCloud(vec3 ro, vec3 rd, float randVal, float bassBoost, float t){
    vec2 box = boxIntersect(ro, rd, BBOX);
    if(box.y < 0.0) return vec4(0.0);
    float totDist = box.y - box.x;
    float stepSize = sqrt(dot(BBOX*2.0, BBOX*2.0)) / float(CLOUD_STEPS);
    int steps = int(ceil(totDist / stepSize)) + 1;

    float ds = density * (1.0 + bassBoost*1.8*reactivity);
    float tv = max(0.0, box.x);
    float transmittance = 1.0;
    vec3  col = vec3(0.0);
    float rj = randVal*2.0 - 1.0;
    tv += rj*stepSize*0.5;

    for(int i = 0; i < CLOUD_STEPS + 2; i++){
        if(i >= steps) break;
        tv += stepSize;
        float over = max(0.0, tv - box.y);
        tv = min(tv, box.y);
        float cur = stepSize - over;

        vec3 p = ro + tv*rd;
        p = getCloudRot(length(p), t) * p;

        float tCol = t*colorSpeed;
        vec3 phase  = p.z*2.0 - tCol + vec3(0.0, 2.0, 4.0);
        vec3 shadowDens = 0.5 + 0.5*cos(phase)*0.75;

        // Blend cloud colour palette
        float blend = sat(p.y*0.5 + 0.5);
        shadowDens *= mix(cloudColorB.rgb, cloudColorA.rgb, blend);

        float v = sat(sampleCloud(p)*stepSize*ds);
        if(v > 0.001){
            float s  = shadowStep(p, randVal, ds);
            vec3  st = exp(-s*shadowDens);
            col += st*v*transmittance*0.75;

            float ssa = cur;
            float amb = sampleCloud(p + vec3(0,0,ssa))
                      + sampleCloud(p + vec3(0,0,2.0*ssa))
                      + sampleCloud(p + vec3(0,0,3.0*ssa));
            col += exp(-amb*AMBIENT_D)*v*transmittance*0.25;

            transmittance *= 1.0 - v;
        }
        if(transmittance <= 0.01){ transmittance = 0.0; break; }
    }
    return vec4(col, 1.0 - transmittance);
}

// ═══════════════════════════════════════════════════════
//  SPHERE TRACING / REFRACTION (fisheye lens)
// ═══════════════════════════════════════════════════════

bool sphereHit(vec3 ro, vec3 rd, vec3 c, float r, out float t0, out float t1){
    vec3 oc = ro - c;
    float b = dot(oc, rd);
    float cc = dot(oc,oc) - r*r;
    float h = b*b - cc;
    if(h < 0.0) return false;
    h = sqrt(h);
    t0 = -b - h;
    t1 = -b + h;
    return true;
}

// Returns cloud-space ray origin/direction for a refracted ray through the glass sphere
// (output planeHit is 3-D position on back-plane)
bool traceRefractionRay(vec3 ro, vec3 rd, vec3 sc, float r, float eta,
                         float spinA, out vec3 cloudRo, out vec3 cloudRd){
    float t0, t1;
    if(!sphereHit(ro, rd, sc, r, t0, t1)) return false;

    vec3 p0 = ro + rd*t0;
    vec3 n0 = normalize(p0 - sc);
    vec3 rd1 = refract(rd, n0, 1.0/eta);
    if(dot(rd1,rd1) < 1e-5) return false;

    vec3 oc = p0 - sc;
    float b = dot(oc, rd1);
    float cc2 = dot(oc,oc) - r*r;
    float h = b*b - cc2;
    if(h < 0.0) return false;
    float tExit = -b + sqrt(h);
    vec3 p1 = p0 + rd1*tExit;
    vec3 n1 = normalize(sc - p1);
    vec3 rd2 = refract(rd1, n1, eta);
    if(dot(rd2,rd2) < 1e-5) rd2 = reflect(rd1, n1);

    // Apply spin rotation in the lens plane
    float cs = cos(spinA), sn = sin(spinA);
    mat2 R = mat2(cs,-sn,sn,cs);
    p1.xy = R * p1.xy;
    rd2.xy = R * rd2.xy;

    // Scale for cloudScale
    p1 = p1 / max(cloudScale, 0.001);

    cloudRo = p1;
    cloudRd = rd2;
    return true;
}

// ═══════════════════════════════════════════════════════
//  PLASMA FLAME INTERIOR
//  Fusion of the lens-coupled sine-swirl (the lens distance seeds the
//  advection field) and a warped fbm flame ridge that is advected BY the
//  swirl — one coupled system living inside the refracted sphere.
// ═══════════════════════════════════════════════════════

float fbm3s(vec3 p){
    float v = 0.0, w = 0.5;
    for(int i = 0; i < 4; i++){
        v += (vnoise3(p)*2.0 - 1.0)*w;
        p  = NOISE_ROT * p;
        w *= 0.5;
    }
    return v;
}

vec3 tanh3(vec3 x){
    vec3 e = exp(2.0*clamp(x, -8.0, 8.0));
    return (e - 1.0)/(e + 1.0);
}

float s1f(float v){ return sin(v)*0.5 + 0.5; }

vec3 plasmaFlame(vec2 q, float T2, float bass, float mid, float high){
    // swirl essence: the lens term itself seeds the advection velocity
    float l = abs(0.7 - dot(q, q));
    vec2 v = q * (1.0 - l) / 0.2;
    vec4 o = vec4(0.0);
    for (float i = 1.0; i <= 8.0; i += 1.0) {
        o += (sin(v.xyyx) + 1.0) * abs(v.x - v.y) * 0.2;
        v += cos(v.yx * i + vec2(0.0, i) + T2) / i + 0.7;
    }
    // flame essence: fbm ridge riding the swirl's velocity field
    float n = vnoise3(vec3(q * 1.4 + vec2(T2, 0.0), T2 * 0.5));
    vec2 q2 = mix(q, vec2(n), 0.2) + v * 0.04;
    float dEnv = fbm3s(vec3(q2 * vec2(0.5, 3.0) + vec2(-T2 * 0.5, 0.0), T2 * 0.5)) + 0.5;
    vec3 flame = vec3(s1f(3.0 + dEnv*10.0), s1f(2.0 + dEnv*10.0), s1f(1.0 + dEnv*10.0));
    flame = pow(flame + 0.6, vec3(2.1));
    // audio in tone-mapping only (tanh clamps; domains stay audio-free)
    vec3 swirl = tanh3(exp(q.y * vec3(1.0, -1.0, -2.0)) * exp(-4.0 * l)
                       / max(o.rgb, vec3(1e-3)) * (0.8 + 0.7*bass + 0.25*mid));
    return swirl * mix(vec3(1.0), flame * 1.5, 0.75)
         + flame * max(dEnv, 0.0) * (0.25 + 0.25*high);
}

// ═══════════════════════════════════════════════════════
//  HASH JITTER
// ═══════════════════════════════════════════════════════

float hash12(vec2 src){
    // Float hash (GLSL ES 1.0 friendly replacement for murmur/uint hash)
    return fract(sin(dot(src, vec2(127.1, 311.7))) * 43758.5453123);
}

// ═══════════════════════════════════════════════════════
//  OPTIONAL SOURCE TEXTURE SAMPLING
// ═══════════════════════════════════════════════════════

vec4 sampleSource(vec2 uv){
    if(uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) return vec4(0.0);
    return IMG_NORM_PIXEL(inputTex, uv);
}

// ═══════════════════════════════════════════════════════
//  MAIN
// ═══════════════════════════════════════════════════════

void main(){
    vec2 fragCoord = gl_FragCoord.xy;
    float aspect = RENDERSIZE.x / RENDERSIZE.y;
    // Centered coords: y in [-1,1], x scaled
    vec2 p = (fragCoord / RENDERSIZE.y)*2.0 - vec2(aspect, 1.0);

    // Audio signals
    float bass  = sat(audioBass  * reactivity);
    float mid   = sat(audioMid   * reactivity);
    float high  = sat(audioHigh  * reactivity);
    float aLvl  = sat(audioLevel * reactivity);

    float T = TIME * speed;

    // Effective lens radius (bass pulses it slightly)
    float r = lensSize * (1.0 + bass * 0.06 * reactivity);
    float d = length(p);

    // Sphere lens mask
    float mask = 1.0 - smoothstep(r - rimSoftness, r + rimSoftness, d);

    // Camera rays
    vec3 ro = vec3(p, -2.0);
    vec3 rd = normalize(vec3(p * (perspective + mid*0.08*reactivity), 1.0));

    // spin angle
    float spinA = T * spin;

    // ── Background (outside sphere) ──
    // Sky gradient modulated by audio
    float skyBlend = sat(0.5 + p.y*0.35 + aLvl*0.15);
    vec3 skyCol = mix(bgColor.rgb * 0.4, bgColor.rgb, skyBlend);
    // Soft animated nebula wisps behind the sphere
    float wisps = 0.5 + 0.5*sin(p.x*3.0 + T*0.3)*sin(p.y*2.5 + T*0.21);
    skyCol = mix(skyCol, hsv2rgb(vec3(fract(T*0.04 + 0.55 + mid*0.1), 0.7, 0.5)), wisps*0.18*intensity);

    vec4 outsideBg = vec4(skyCol, 1.0);

    // Drop shadow
    if(shadowStrength > 0.001){
        vec2 sCenter = vec2(shadowOffsetX, shadowOffsetY);
        float sd = length(p - sCenter);
        float shadow = (1.0 - smoothstep(r*0.6, r + 0.35, sd)) * shadowStrength;
        outsideBg.rgb = mix(outsideBg.rgb, vec3(0.0), shadow);
    }

    // ── Raytrace through the lens for three chromatic channels ──
    vec4 lensCol = vec4(0.0);

    if(mask > 0.001){
        float randVal = hash12(fragCoord + T);

        // Chromatic offsets driven by mids
        float chromShift = chromatic * (1.0 + mid*0.5*reactivity);

        float etaR = ior - chromShift;
        float etaG = ior;
        float etaB = ior + chromShift;

        vec3 sc = vec3(0.0);

        // For each channel we get a different refracted ray direction into the cloud volume
        vec3 cRo_R, cRd_R, cRo_G, cRd_G, cRo_B, cRd_B;
        bool hitR = traceRefractionRay(ro, rd, sc, r, etaR, spinA, cRo_R, cRd_R);
        bool hitG = traceRefractionRay(ro, rd, sc, r, etaG, spinA, cRo_G, cRd_G);
        bool hitB = traceRefractionRay(ro, rd, sc, r, etaB, spinA, cRo_B, cRd_B);

        // Render cloud for each channel
        float colR = 0.0, colG = 0.0, colB = 0.0, colA = 0.0;
        bool wantCloud = plasmaAmt < 0.999;

        if(hitG && wantCloud){
            vec4 cloudG = renderCloud(cRo_G, cRd_G, randVal, bass, T);
            vec3 sky2 = bgColor.rgb;
            vec3 cg = cloudG.a > 0.0 ? cloudG.rgb / cloudG.a : vec3(0.0);
            vec3 blendG = mix(sky2, cg, cloudG.a);
            colG = blendG.g;
            colA = cloudG.a;
            // Use cloud for all channels as base, then shift per chromatic channel
            colR = blendG.r;
            colB = blendG.b;
        }
        if(hitR && wantCloud){
            vec4 cloudR = renderCloud(cRo_R, cRd_R, randVal, bass, T);
            vec3 sky2 = bgColor.rgb;
            vec3 cr2 = cloudR.a > 0.0 ? cloudR.rgb / cloudR.a : vec3(0.0);
            colR = mix(sky2, cr2, cloudR.a).r;
        }
        if(hitB && wantCloud){
            vec4 cloudB = renderCloud(cRo_B, cRd_B, randVal, bass, T);
            vec3 sky2 = bgColor.rgb;
            vec3 cb2 = cloudB.a > 0.0 ? cloudB.rgb / cloudB.a : vec3(0.0);
            colB = mix(sky2, cb2, cloudB.a).b;
        }

        // Plasma-flame interior, seen through the same refracted rays
        if(plasmaAmt > 0.001 && hitG){
            float T3 = T * 0.6;
            vec3 plasG = plasmaFlame(cRo_G.xy * 1.4, T3, bass, mid, high);
            vec3 plasR = hitR ? plasmaFlame(cRo_R.xy * 1.4, T3, bass, mid, high) : plasG;
            vec3 plasB = hitB ? plasmaFlame(cRo_B.xy * 1.4, T3, bass, mid, high) : plasG;
            colR = mix(colR, plasR.r, plasmaAmt);
            colG = mix(colG, plasG.g, plasmaAmt);
            colB = mix(colB, plasB.b, plasmaAmt);
            colA = max(colA, plasmaAmt);
        }

        // Check if source texture is bound, blend over cloud
        bool hasTex = IMG_SIZE(inputTex).x > 0.5;
        if(hasTex){
            // Sample source via refraction UV (use green channel ray projected to plane)
            if(hitG && cRd_G.z > 0.0001){
                float planeZ = r*1.5;
                float tPl = (planeZ - cRo_G.z) / cRd_G.z;
                vec3 hit = cRo_G + cRd_G * tPl;
                float asp2 = aspect;
                vec2 texUV = vec2(hit.x / asp2, hit.y)*0.5 + 0.5;
                vec4 texSample = sampleSource(texUV);
                // Blend texture lightly over cloud
                colR = mix(colR, texSample.r, texSample.a*0.45);
                colG = mix(colG, texSample.g, texSample.a*0.45);
                colB = mix(colB, texSample.b, texSample.a*0.45);
            }
        }

        vec3 lensRGB = vec3(colR, colG, colB);
        // Gamma correct the cloud render
        lensRGB = pow(max(lensRGB, vec3(0.0)), vec3(1.0/2.2));
        lensRGB *= intensity;

        // Fresnel darkening at grazing angles
        float fres = pow(smoothstep(r*0.4, r, d), 2.0);
        lensRGB *= mix(1.0, 1.0 - fresnel, fres);

        // Rim glow (enhanced by bass)
        float rim = pow(smoothstep(r*0.82, r, d), 3.0);
        lensRGB += rim * rimGlow * (1.0 + bass*0.6*reactivity);

        // Subtle interior caustic shimmer driven by mid
        float caustic = 0.5 + 0.5*sin(p.x*12.0 + T*2.3)*sin(p.y*10.0 - T*1.7);
        lensRGB += caustic * mid * 0.08 * intensity;

        lensCol = vec4(lensRGB, colA > 0.5 ? 1.0 : 0.85);
    }

    // Composite lens over background
    vec4 col = mix(outsideBg, lensCol, mask);
    if(transparentBg) col.a = mask;

    // ── Neon ring (audio reactive: highs spike it) ──
    if(neonIntensity > 0.001){
        float ringDist = abs(d - r);
        float core = exp(-(ringDist*ringDist) / max(neonWidth*neonWidth, 1e-6));
        float halo = exp(-ringDist / max(neonBloom, 1e-4));
        // Pulse + high-freq spike
        float pulse = 1.0 - neonPulse*0.5 + neonPulse*0.5*sin(T*2.2);
        float hiSpike = 1.0 + high * 1.8 * reactivity;
        // Hue-drift the neon based on color drift and audio
        float neonHue = fract(T*colorSpeed*0.07 + high*0.15*reactivity);
        vec3 neonDrift = mix(neonColor.rgb, hsv2rgb(vec3(neonHue, 0.9, 1.0)), 0.35*aLvl);
        vec3 neon = neonDrift * (core*2.2 + halo*0.55) * neonIntensity * pulse * hiSpike;
        col.rgb += neon;
    }

    // ── Lens flare (inside sphere, warped by glass curvature) ──
    if(flare > 0.001 && mask > 0.001){
        float z = sqrt(max(r*r - dot(p,p), 1e-4));
        vec2 pw = p * (r / z);

        vec2 lightPos = vec2(cos(flareAngle), sin(flareAngle)) * (r*0.9);
        vec2 toLight = lightPos - pw;
        float distToLight = length(toLight);

        vec3 fc2 = flareColor.rgb;
        vec3 flareCol2 = vec3(0.0);

        // Hotspot
        float hot = exp(-distToLight*distToLight*80.0);
        flareCol2 += fc2 * hot * 3.5;

        // Streak
        vec2 rel = pw - lightPos;
        float along = dot(rel, vec2(1,0));
        float perp  = dot(rel, vec2(0,1));
        float streak = exp(-perp*perp*1000.0) * exp(-abs(along)*1.4);
        flareCol2 += fc2 * streak * 1.0;

        // Ring caustic
        float ring2 = exp(-pow(d - r*0.94, 2.0)*600.0);
        float dw = sat(dot(normalize(p + 1e-4), vec2(cos(flareAngle), sin(flareAngle)))*0.5 + 0.5);
        flareCol2 += fc2 * ring2 * dw * 0.8;

        // Ghost dots
        for(int i = 0; i < 5; i++){
            float gi = float(i);
            vec2 gpos = lightPos * (-0.35 + gi*0.35);
            float gd2 = distance(pw, gpos);
            float gs = 0.05 + 0.02*gi;
            float ghost = exp(-gd2*gd2/(gs*gs));
            flareCol2 += mix(fc2, vec3(0.5, 0.7, 1.0), fract(gi*0.37)) * ghost * 0.3;
        }

        // Radial bloom
        float bloom2 = 0.08 / (distToLight*distToLight + 0.02);
        flareCol2 += fc2 * bloom2 * 0.4;

        // Audio modulation on flare
        float flareMod = 1.0 + bass*0.5*reactivity;
        col.rgb += flareCol2 * flare * mask * flareMod;
    }

    // Final vignette
    float vign = 1.0 - smoothstep(0.6, 1.5, length(p) / max(aspect, 1.0));
    col.rgb *= 0.92 + 0.08*vign;

    // ---- universal color block (defaults = no-op) ----
    // (background handled by the existing bgColor "Sky / BG" input)
    vec3 uc = col.rgb;
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
    col.rgb = uc;

    gl_FragColor = col;
}
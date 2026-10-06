/*{
  "DESCRIPTION": "Trapped — squishy deformable balls bouncing inside a glass box",
  "CREDIT": "nimitz (Shadertoy), adapted for ShaderClaw",
  "CATEGORIES": [
    "Generator",
    "3D"
  ],
  "INPUTS": [
    {
      "NAME": "inputImage",
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
      "NAME": "ballSize",
      "LABEL": "Ball Size",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0.3,
      "MAX": 1.8,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "squish",
      "LABEL": "Squish",
      "TYPE": "float",
      "DEFAULT": 0.6,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "speed",
      "LABEL": "Speed",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 3,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "ballColor",
      "LABEL": "Ball Color",
      "TYPE": "color",
      "DEFAULT": [
        1,
        1,
        1,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "colorMix",
      "LABEL": "Color Mix",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "hueShift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
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
      "NAME": "zoom",
      "LABEL": "Zoom",
      "TYPE": "float",
      "DEFAULT": 5.5,
      "MIN": 3,
      "MAX": 15,
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
        1
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "transparentBg",
      "LABEL": "Transparent",
      "TYPE": "bool",
      "DEFAULT": 1,
      "GROUP": "Background"
    },
    {
      "NAME": "roughness",
      "LABEL": "Roughness",
      "TYPE": "float",
      "DEFAULT": 0.57,
      "MIN": 0.05,
      "MAX": 1
    }
  ]
}*/

#define ITR 100
#define FAR 10.0

// Audio conditioning (playbook soft knees) — shared by map() and main().
float aBass()  { return pow(smoothstep(0.05, 0.85, audioBass), 1.6); }
float aMid()   { return pow(smoothstep(0.08, 0.85, audioMid), 1.3); }
float aHigh()  { return pow(smoothstep(0.10, 0.90, audioHigh), 1.2); }
float aDrive() { return 0.25 + 0.75 * smoothstep(0.05, 0.9, audioEnergy); }
// Time-warp clock: the balls tumble with the track's energy, and the
// integrated band clocks (audioBassTime/audioMidTime advance only while the
// band is hot) make the tumble SPEED itself track bass+mid — beatless
// ambient swells read as the whole scene surging/relaxing. Silence: the
// integrated clocks hold still, so the look is exactly the authored tumble.
float musicT() { return (TIME * (0.7 + 0.6 * aDrive()) + 1.8 * audioBassTime + 1.0 * audioMidTime) * speed; }

float smax(float a, float b) {
    float pw = 14.0;
    float res = exp2(pw * a) + exp2(pw * b);
    return log2(res) / pw;
}

float sbox(vec3 p, vec3 b) {
    vec3 d = abs(p) - b;
    return min(max(d.x, max(d.y, d.z)), 0.0) + length(max(d, 0.0));
}

float matid = 0.0;

// Procedural ball positions (replaces physics buffer)
vec4 getBall(float fi, float t) {
    float r = ballSize * (0.9 + 0.1 * sin(fi * 2.37)) * (1.0 + 0.22 * aBass()); // bass swells the balls (deep enough to read on beatless swells)
    float px = sin(t * (0.7 + fi * 0.13) + fi * 1.3) * (1.8 - r);
    float py = sin(t * (0.9 + fi * 0.17) + fi * 2.7) * (1.8 - r);
    float pz = sin(t * (0.6 + fi * 0.11) + fi * 4.1) * (1.8 - r);
    return vec4(px, py, pz, r);
}

float map(vec3 p) {
    float t = musicT();
    float dmx = 1.0 - min(squish * (1.0 + 0.3 * aMid()), 1.0); // mids deepen the squish

    float df = 100.0;
    float d0, d1, d2, d3, d4;

    vec4 sp0 = getBall(0.0, t); d0 = length(p - sp0.xyz) - sp0.w;
    vec4 sp1 = getBall(1.0, t); d1 = length(p - sp1.xyz) - sp1.w;
    vec4 sp2 = getBall(2.0, t); d2 = length(p - sp2.xyz) - sp2.w;
    vec4 sp3 = getBall(3.0, t); d3 = length(p - sp3.xyz) - sp3.w;
    vec4 sp4 = getBall(4.0, t); d4 = length(p - sp4.xyz) - sp4.w;

    float dm;

    dm = d0;
    dm = mix(smax(dm, -d1), dm, dmx);
    dm = mix(smax(dm, -d2), dm, dmx);
    dm = mix(smax(dm, -d3), dm, dmx);
    dm = mix(smax(dm, -d4), dm, dmx);
    if (dm < df) { matid = 0.0; df = dm; }

    dm = d1;
    dm = mix(smax(dm, -d0), dm, dmx);
    dm = mix(smax(dm, -d2), dm, dmx);
    dm = mix(smax(dm, -d3), dm, dmx);
    dm = mix(smax(dm, -d4), dm, dmx);
    if (dm < df) { matid = 1.0; df = dm; }

    dm = d2;
    dm = mix(smax(dm, -d0), dm, dmx);
    dm = mix(smax(dm, -d1), dm, dmx);
    dm = mix(smax(dm, -d3), dm, dmx);
    dm = mix(smax(dm, -d4), dm, dmx);
    if (dm < df) { matid = 2.0; df = dm; }

    dm = d3;
    dm = mix(smax(dm, -d0), dm, dmx);
    dm = mix(smax(dm, -d1), dm, dmx);
    dm = mix(smax(dm, -d2), dm, dmx);
    dm = mix(smax(dm, -d4), dm, dmx);
    if (dm < df) { matid = 3.0; df = dm; }

    dm = d4;
    dm = mix(smax(dm, -d0), dm, dmx);
    dm = mix(smax(dm, -d1), dm, dmx);
    dm = mix(smax(dm, -d2), dm, dmx);
    dm = mix(smax(dm, -d3), dm, dmx);
    if (dm < df) { matid = 4.0; df = dm; }

    float box = -sbox(p, vec3(2.2));
    df = smax(df, -box);

    return df;
}

float march(vec3 ro, vec3 rd) {
    float precis = 0.001;
    float h = precis * 2.0;
    float d = 0.0;
    for (int i = 0; i < ITR; i++) {
        if (abs(h) < precis || d > FAR) break;
        d += h;
        h = map(ro + rd * d);
    }
    return d;
}

vec2 iBox(vec3 ro, vec3 rd, vec4 b) {
    vec3 m = 1.0 / rd;
    vec3 n = m * (ro - b.xyz);
    vec3 k = abs(m) * b.w;
    vec3 t1 = -n - k;
    vec3 t2 = -n + k;
    float tN = max(max(t1.x, t1.y), t1.z);
    float tF = min(min(t2.x, t2.y), t2.z);
    if (tN > tF || tF < 0.0) return vec2(-1.0);
    return vec2(tN, tF);
}

vec3 rotx(vec3 p, float a) {
    float s = sin(a), c = cos(a);
    return vec3(p.x, c * p.y - s * p.z, s * p.y + c * p.z);
}

vec3 roty(vec3 p, float a) {
    float s = sin(a), c = cos(a);
    return vec3(c * p.x + s * p.z, p.y, -s * p.x + c * p.z);
}

vec3 normal(vec3 p) {
    vec2 e = vec2(-1.0, 1.0) * 0.005;
    return normalize(
        e.yxx * map(p + e.yxx) + e.xxy * map(p + e.xxy) +
        e.xyx * map(p + e.xyx) + e.yyy * map(p + e.yyy)
    );
}

float calcAO(vec3 pos, vec3 nor) {
    float occ = 0.0;
    float sca = 1.0;
    for (int i = 0; i < 5; i++) {
        float hr = 0.01 + 0.12 * float(i) / 4.0;
        float dd = map(nor * hr + pos);
        occ += -(dd - hr) * sca;
        sca *= 0.95;
    }
    return clamp(occ * -2.0 + 1.0, 0.0, 1.0);
}

float calcShadow(vec3 ro, vec3 rd, float mint) {
    float res = 1.0;
    float t = mint;
    for (int i = 0; i < 20; i++) {
        float h = map(ro + rd * t);
        res = min(res, 8.0 * h / t);
        t += clamp(h, 0.1, 0.4);
        if (h < 0.001 || t > 5.0) break;
    }
    return clamp(res, 0.0, 1.0);
}

vec3 lgt = normalize(vec3(-0.5, 0.5, -0.2));
vec3 lcol = vec3(1.1);

vec3 shade(vec3 pos, vec3 rd, vec3 n, vec3 alb) {
    float nl = dot(n, lgt);
    float nv = dot(n, -rd);
    vec3 col = vec3(0.0);
    float ao = calcAO(pos, n);
    vec3 f0 = vec3(0.1);

    if (nl > 0.0) {
        vec3 haf = normalize(lgt - rd);
        float nh = clamp(dot(n, haf), 0.0, 1.0);
        float nvv = clamp(dot(n, -rd), 0.0, 1.0);
        float lh = clamp(dot(lgt, haf), 0.0, 1.0);
        float a = roughness * roughness;
        float a2 = a * a;
        float dnm = nh * nh * (a2 - 1.0) + 1.0;
        float D = a2 / (3.14159 * dnm * dnm);
        float k = pow(roughness + 1.0, 2.0) / 8.0;
        float G = (1.0 / (nl * (1.0 - k) + k)) * (1.0 / (nvv * (1.0 - k) + k));
        vec3 F = f0 + (1.0 - f0) * exp2((-5.55473 * lh - 6.98316) * lh);
        // HDR specular boost: tight highlights blow out to 1.4-2.5 linear for bloom
        // (highs sharpen the specular glints)
        float specHDR = 1.0 + (1.4 + 0.6 * aHigh()) * pow(nh, 8.0);
        vec3 spec = nl * D * F * G * specHDR;
        col.rgb = lcol * nl * (spec + alb * (1.0 - f0));
    }

    col *= calcShadow(pos, lgt, 0.1) * 0.8 + 0.2;

    float bnc = clamp(dot(n, normalize(vec3(-lgt.x, 5.0, -lgt.z))) * 0.5 + 0.28, 0.0, 1.0);
    col.rgb += lcol * alb * bnc * 0.1;

    col += 0.05 * alb;
    col *= ao;
    return col;
}

float tri(float x) { return abs(fract(x) - 0.5); }
vec3 tri3(vec3 p) {
    return vec3(tri(p.z + tri(p.y)), tri(p.z + tri(p.x)), tri(p.y + tri(p.x)));
}

float triNoise3d(vec3 p) {
    mat2 m2 = mat2(0.970, 0.242, -0.242, 0.970);
    p.y *= 0.57;
    float z = 1.5;
    float rz = 0.0;
    vec3 bp = p;
    for (int i = 0; i < 2; i++) {
        vec3 dg = tri3(bp * 0.5);
        p += (dg + 0.1);
        bp *= 2.2;
        z *= 1.4;
        p *= 1.2;
        p.xz *= m2;
        rz += tri(p.z + tri(p.x + tri(p.y))) / z;
        bp += 0.9;
    }
    return rz;
}

void main() {
    float t = musicT(); // must match map()'s clock
    vec2 q = gl_FragCoord.xy / RENDERSIZE.xy;
    vec2 p = q - 0.5;
    p.x *= RENDERSIZE.x / RENDERSIZE.y;

    vec2 mo = vec2(-0.2, 0.0);
    if (mouseDown > 0.5) {
        mo = mousePos - 0.5;
        mo.x *= RENDERSIZE.x / RENDERSIZE.y;
    }
    mo.x += sin(t * 0.1);
    mo *= 5.14;

    vec3 ro = vec3(0.0, 0.0, zoom);
    vec3 rd = normalize(vec3(p, -1.0));
    ro = rotx(ro, mo.y); rd = rotx(rd, mo.y);
    ro = roty(ro, mo.x); rd = roty(rd, mo.x);

    vec3 col = bgColor.rgb;
    vec3 brd = rd;

    float rz = march(ro, rd);

    if (rz < FAR) {
        vec3 pos = ro + rd * rz;
        vec3 nor = normal(pos);

        // Default procedural albedo per ball
        vec3 procAlb = sin(vec3(nor.x * 0.4, nor.y * 0.5 + 1.0, nor.z * 0.4 + 4.0) * 0.9 - 4.0 + matid * 1.1) * 0.47 + 0.5;

        // User color tint
        vec3 tintAlb = ballColor.rgb * (0.6 + 0.4 * dot(nor, lgt));

        // Blend procedural with user color
        vec3 alb = mix(procAlb, tintAlb, colorMix);

        // Texture mapping: spherical UV from the ball's local space
        if (texMix > 0.0) {
            // Find which ball we hit by checking each
            vec4 ball = getBall(0.0, t);
            if (matid > 0.5) ball = getBall(1.0, t);
            if (matid > 1.5) ball = getBall(2.0, t);
            if (matid > 2.5) ball = getBall(3.0, t);
            if (matid > 3.5) ball = getBall(4.0, t);
            vec3 localP = normalize(pos - ball.xyz);
            // Spherical UV
            float u = 0.5 + atan(localP.x, localP.z) / 6.2832;
            float v = 0.5 - asin(clamp(localP.y, -1.0, 1.0)) / 3.14159;
            vec3 texCol = texture2D(inputImage, vec2(u, v)).rgb;
            alb = mix(alb, texCol, texMix);
        }

        col = shade(pos, rd, nor, alb);
    }

    vec2 ib2 = iBox(ro, brd, vec4(0.0, 0.0, 0.0, 2.2));
    float brad = 2.28;

    // Soft AA width for the glass box edge mask, derived from screen-space derivative
    float aaw = max(fwidth(p.x) + fwidth(p.y), 1e-4) * 0.6;

    if (ib2.x > 0.0) {
        if (ib2.y < rz) {
            vec3 pos = ro + brd * ib2.y;
            vec3 e = smoothstep(brad - 0.15 - aaw, brad + aaw, abs(pos));
            float al = 1.0 - (1.0 - e.x * e.y) * (1.0 - e.y * e.z) * (1.0 - e.z * e.x);
            col = mix(col, vec3(0.03), 0.4 * al);
            // HDR edge accent: corners/edges punch above 1.0 for bloom (highs sparkle the frame)
            float edgeCore = smoothstep(0.55, 1.0, al);
            col += vec3(0.97, 1.0, 0.99) * edgeCore * 1.1 * (1.0 + 0.4 * aHigh());
            col *= (triNoise3d(pos * 2.0) * 0.1 + 0.95) * vec3(0.97, 1.0, 0.99);
        }
        if (ib2.x < rz) {
            vec3 pos = ro + brd * ib2.x;
            vec3 e = smoothstep(brad - 0.15 - aaw, brad + aaw, abs(pos));
            float al = 1.0 - (1.0 - e.x * e.y) * (1.0 - e.y * e.z) * (1.0 - e.z * e.x);
            col = mix(col, vec3(0.03), 0.4 * al);
            // HDR edge accent on the front faces (stronger — these read as highlights)
            // Highs sparkle it; punch adds a decaying flash on hits.
            float edgeCore = smoothstep(0.55, 1.0, al);
            col += vec3(0.97, 1.0, 0.99) * edgeCore * (1.4 * (1.0 + 0.4 * aHigh()) + 0.6 * audioPunch);
            col *= (triNoise3d(pos * 2.0) * 0.17 + 0.9) * vec3(0.97, 1.0, 0.99);
        }
    }

    // Central highlight HDR boost: brightest pixels (already-hot specular) push to 1.4-2.5 linear
    float lum = dot(col, vec3(0.2126, 0.7152, 0.0722));
    float hdrLift = smoothstep(0.85, 1.6, lum);
    col += col * hdrLift * 0.8;

    // Continuous band-follow (ambient fix): smooth bass/mid lift the whole
    // scene's light — no beat gating, so beatless swells read as breathing.
    // Silence: exactly 1.0, look unchanged.
    // Round-2: LINEAR bands here. aBass()/aMid() pow-knees crushed ambient's
    // 0.1-0.8 swells (0.3 bass -> 0.096) — knees stay on the geometry paths.
    col *= 1.0 + 0.25 * clamp(audioBass, 0.0, 1.0) + 0.15 * clamp(audioMid, 0.0, 1.0);

    // NO TONEMAP, NO sRGB OETF — leave linear HDR for downstream Phase Q v4 bloom.
    // Preserve the original vignette character.
    col *= pow(20.0 * q.x * q.y * (1.0 - q.x) * (1.0 - q.y), 0.07);

    // ---- universal color block (defaults = no-op) ----
    vec3 uc = col;
    float ucL = dot(uc, vec3(0.299, 0.587, 0.114));
    uc = mix(vec3(ucL), uc, colorBoost);                     // saturation
    if (hueShift > 0.0005) {                                  // cheap hue rotate (YIQ)
        float hA = hueShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hM * uc, 0.0, 1.0);
    }
    col = uc;

    float alpha = 1.0;
    if (transparentBg) {
        alpha = clamp(1.0 - dot(bgColor.rgb - col, vec3(0.333)), 0.0, 1.0);
    }

    gl_FragColor = vec4(col, alpha);
}

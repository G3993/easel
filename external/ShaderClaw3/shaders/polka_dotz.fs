/*{
  "DESCRIPTION": "Polka dot pixel tiles — flipping mosaic that reveals image through animated circular pixels",
  "CREDIT": "Florian Berger (flockaroo), adapted for ISF by ShaderClaw",
  "CATEGORIES": [
    "Effect"
  ],
  "INPUTS": [
    {
      "NAME": "inputImage",
      "LABEL": "Texture",
      "TYPE": "image"
    },
    {
      "NAME": "specAmt",
      "LABEL": "Specular",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 2
    },
    {
      "NAME": "vignetteAmt",
      "LABEL": "Vignette",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2
    },
    {
      "NAME": "gapShade",
      "LABEL": "Gap Shade",
      "TYPE": "float",
      "DEFAULT": 0.15,
      "MIN": 0,
      "MAX": 0.5
    },
    {
      "NAME": "tileScale",
      "LABEL": "Tile Size",
      "TYPE": "float",
      "DEFAULT": 20,
      "MIN": 5,
      "MAX": 60,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "dotSize",
      "LABEL": "Dot Size",
      "TYPE": "float",
      "DEFAULT": 0.45,
      "MIN": 0.1,
      "MAX": 0.5,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "dotShape",
      "LABEL": "Dot Shape",
      "TYPE": "long",
      "VALUES": [
        0,
        1,
        2,
        3
      ],
      "LABELS": [
        "Circle",
        "Star",
        "Square",
        "Hexagon"
      ],
      "DEFAULT": 0,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "flipSpeed",
      "LABEL": "Flip Speed",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0.1,
      "MAX": 5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "flipInterval",
      "LABEL": "Flip Interval",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0.2,
      "MAX": 4,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "dotPulse",
      "LABEL": "Dot Pulse",
      "TYPE": "float",
      "DEFAULT": 0.35,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
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
      "NAME": "scrollX",
      "LABEL": "Scroll X",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": -1,
      "MAX": 1,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "scrollY",
      "LABEL": "Scroll Y",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": -1,
      "MAX": 1,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "rotateGrid",
      "LABEL": "Rotate Grid",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "bgColor",
      "LABEL": "Gap Color",
      "TYPE": "color",
      "DEFAULT": [
        0.2,
        0.3,
        0.4,
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

// Number of history tiles in each direction
#define Xnum 10
#define Ynum 10

// Hash for pseudo-random per-tile values
float hash12(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}

vec4 hash42(vec2 p) {
    vec4 p4 = fract(vec4(p.xyxy) * vec4(0.1031, 0.1030, 0.0973, 0.1099));
    p4 += dot(p4, p4.wzxy + 33.33);
    return fract((p4.xxyz + p4.yzzw) * p4.zywx);
}

// Aspect-correct UV for texture sampling
vec2 fitUV(vec2 pos) {
    return (pos - 0.5 * RENDERSIZE) * min(IMG_SIZE_inputImage.y / RENDERSIZE.y, IMG_SIZE_inputImage.x / RENDERSIZE.x) / IMG_SIZE_inputImage + 0.5;
}

vec2 frameUV(int frame, vec2 uv) {
    frame = int(mod(float(frame), float(Xnum * Ynum)));
    return (uv + vec2(mod(float(frame), float(Xnum)), float(frame / Xnum))) / vec2(float(Xnum), float(Ynum));
}

vec4 getColor(int frame, vec2 uv) {
    return texture2D(histBuf, frameUV(frame, uv));
}

// Soft AA circle: smoothstep + fwidth for clean polka edges
float circle(vec2 uv, float r) {
    float l = length(uv - 0.5);
    float aa = max(fwidth(l), 1e-4);
    return 1.0 - smoothstep(r - aa, r + aa, l);
}

// Square primitive (Chebyshev / L-inf distance)
float squareShape(vec2 uv, float r) {
    vec2 d = abs(uv - 0.5);
    float l = max(d.x, d.y);
    float aa = max(fwidth(l), 1e-4);
    return 1.0 - smoothstep(r - aa, r + aa, l);
}

// Regular hexagon primitive
float hexShape(vec2 uv, float r) {
    vec2 p = abs(uv - 0.5);
    float l = max(p.x * 0.8660254 + p.y * 0.5, p.y);
    float aa = max(fwidth(l), 1e-4);
    return 1.0 - smoothstep(r - aa, r + aa, l);
}

// 5-pointed star primitive (polar petal distance)
float starShape(vec2 uv, float r) {
    vec2 p = uv - 0.5;
    float a = atan(p.y, p.x);
    float l = length(p);
    // 5-fold modulation: inner/outer radius ratio creates star points
    float mod5 = 0.5 + 0.5 * cos(5.0 * a);
    float petal = mix(0.45, 1.0, mod5);
    float lp = l / max(petal, 0.05);
    float aa = max(fwidth(lp), 1e-4);
    return 1.0 - smoothstep(r - aa, r + aa, lp);
}

// Dispatcher: pick primitive by dotShape enum (0=circle,1=star,2=square,3=hex)
// ISF wraps "long" as uniform float — use float comparisons with epsilon.
float dotPrim(vec2 uv, float r) {
    float s = float(dotShape);
    if (s > 2.5) return hexShape(uv, r);
    if (s > 1.5) return squareShape(uv, r);
    if (s > 0.5) return starShape(uv, r);
    return circle(uv, r);
}

float circleMask(vec2 uv, float y, float r1, float r2) {
    return dotPrim(uv + vec2(0.0, y - 1.0), r1) + dotPrim(uv + vec2(0.0, y), r2);
}

// Returns a soft "centerness" 0..1 — peaks at dot center, falls off near edge.
float circleCore(vec2 uv, float y, float r1, float r2) {
    float l1 = length(uv + vec2(0.0, y - 1.0) - 0.5);
    float l2 = length(uv + vec2(0.0, y) - 0.5);
    float aa = max(fwidth(l1 + l2), 1e-4);
    float c1 = 1.0 - smoothstep(r1 * 0.15, r1 * 0.85, l1);
    float c2 = 1.0 - smoothstep(r2 * 0.15, r2 * 0.85, l2);
    return clamp(c1 + c2, 0.0, 1.0);
}

float rectMask(float b, float w, vec2 uv) {
    vec4 e = smoothstep(vec4(-b - 0.5 * w), vec4(-b + 0.5 * w), vec4(uv, vec2(1.0) - uv));
    return e.x * e.y * e.z * e.w;
}

float getVign(vec2 fragCoord) {
    float rs = length(fragCoord - RENDERSIZE * 0.5) / RENDERSIZE.x;
    return 1.0 - rs * rs * rs;
}

void main() {
    vec2 pos = gl_FragCoord.xy;

    // Frame counter from TIME (since ISF has no iFrame)
    int iFrame = int(TIME * 30.0);
    // How many frames between flips
    int DFrame = int(30.0 * flipInterval);
    if (DFrame < 1) DFrame = 1;

    // Tile size in pixels, scaled proportionally
    float TileSize = tileScale * sqrt(RENDERSIZE.y / 1080.0);

    // ==== PASS 0: Record frame history in tile grid ====
    if (PASSINDEX == 0) {
        vec2 uv0 = pos / RENDERSIZE;
        int fr = int(uv0.x * float(Xnum)) + int(uv0.y * float(Ynum)) * Xnum;
        int slot = int(mod(float(iFrame), float(Xnum * Ynum)));

        if (fr == slot || FRAMEINDEX < 5) {
            // This tile slot is current — write fresh input
            vec2 tileUV = fract(uv0 * vec2(float(Xnum), float(Ynum)));
            gl_FragColor = texture2D(inputImage, fitUV(tileUV * RENDERSIZE));
        } else {
            // Keep previous frame data
            gl_FragColor = texture2D(histBuf, uv0);
        }
        return;
    }

    // ==== PASS 1: Polka dot mosaic composite ====
    int actFrame = (iFrame / DFrame) * DFrame;
    int prevFrame = ((iFrame / DFrame) - 1) * DFrame;

    // Optional grid rotation + scroll — adds the dynamic motion the user wants.
    vec2 mUV = pos / RENDERSIZE;
    if (rotateGrid > 0.001) {
        vec2 c = mUV - 0.5;
        float ra = TIME * rotateGrid * 0.10;
        float ca = cos(ra), sa = sin(ra);
        mUV = 0.5 + vec2(ca * c.x - sa * c.y, sa * c.x + ca * c.y);
    }
    mUV += vec2(scrollX * TIME * 0.05, scrollY * TIME * 0.05);
    vec2 movedPos = mUV * RENDERSIZE;

    // Per-tile random (seeded by tile position + frame cycle)
    vec2 tileIdx = floor(movedPos / TileSize + float(iFrame / DFrame) * 13.0) + 0.5;
    vec4 rand = hash42(tileIdx);

    vec2 uvQ = floor(movedPos / TileSize) * TileSize / RENDERSIZE;
    vec2 uv = movedPos / RENDERSIZE;
    vec2 duv = (uv - uvQ) * RENDERSIZE / TileSize;

    vec4 c1 = getColor(actFrame, uvQ);
    vec4 c2 = getColor(prevFrame, uvQ);

    // Flip animation: stagger per-tile using random offset
    float y = -rand.x * 2.0 + 3.0 * float(iFrame - actFrame) / float(DFrame) * flipSpeed;
    y = clamp(y, 0.0, 1.0);
    y *= y;

    // Pulsing dot size — bass and time both modulate the radius.
    // Audio non-gating: time-based pulse alive at audio=0.
    float pulse = 1.0 + sin(TIME * 2.5 + rand.x * 6.28) * dotPulse * 0.4
                + audioBass * audioReact * dotPulse * 0.3;
    float r1 = dotSize * pulse;
    float r2 = dotSize * pulse;

    // Mix between current and previous frame
    vec4 col = mix(c1, c2, smoothstep(y - 0.1, y + 0.1, 1.0 - duv.y));

    // Gap shading between dots
    float cmask = circleMask(duv, y, r1, r2);
    col = mix(col, bgColor, gapShade - gapShade * cmask);

    // Rectangular ambient darkening per tile
    col *= 0.5 + 0.5 * rectMask(0.2 * dot(col.xyz, vec3(0.333)), 0.7, duv);

    // Specular edge highlight on dots
    float spec = clamp(
        circleMask(duv - 0.02, y, r1, r2) - circleMask(duv + 0.02, y, r1, r2),
        -0.4, 1.0
    );
    col.xyz += specAmt * spec;

    // ==== HDR PEAKS for Phase Q v4 bloom ====
    // Dot centers lift to 1.4-2.0 linear; audio-flash adds extra hot punch on kicks.
    // Time-driven shimmer keeps it alive at audio=0 (non-gating).
    float core = circleCore(duv, y, r1, r2);
    float shimmer = 0.5 + 0.5 * sin(TIME * 3.7 + rand.y * 6.28);
    float idleHDR = 0.45 + 0.25 * shimmer;            // 0.45..0.70 baseline lift
    float audioHDR = audioBass * audioReact * 1.10;   // up to ~1.1 extra on kicks
    float hdrLift = idleHDR + audioHDR;
    // Apply lift weighted by dot core, modulated by base color luminance so dark
    // dots still bloom but bright dots peak hardest. No tonemap.
    float lum = dot(col.rgb, vec3(0.299, 0.587, 0.114));
    col.rgb += col.rgb * core * hdrLift * (0.55 + 0.85 * lum);

    // Spec highlight HDR boost — already-hot edges become real bloom seeds.
    col.rgb += vec3(spec * specAmt * (0.6 + 0.5 * audioBass * audioReact));

    // Vignette (no tonemap after this — preserve HDR for bloom)
    if (vignetteAmt > 0.0) {
        col *= mix(1.0, 1.2 * getVign(pos), vignetteAmt);
    }

    // ---- universal color block (defaults = no-op; bgColor already native) ----
    float ucL = dot(col.rgb, vec3(0.299, 0.587, 0.114));
    vec3 uc = mix(vec3(ucL), col.rgb, colorBoost);
    if (hueShift > 0.0005) {
        float hueA = hueShift * 6.2831853;
        float hueC = cos(hueA), hueS = sin(hueA);
        mat3 hueM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                  + hueC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                  + hueS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hueM * uc, 0.0, 1.0);
    }
    col.rgb = uc;

    gl_FragColor = col;
    gl_FragColor.w = 1.0;
}

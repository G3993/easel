/*{
  "DESCRIPTION": "Morphing 3D shape — box, prism, torus, knots with refraction and wave background",
  "CREDIT": "ISF Import by Old Salt, glslsandbox #73426, adapted for ShaderClaw",
  "CATEGORIES": [
    "Generator"
  ],
  "INPUTS": [
    {
      "NAME": "uIntensity",
      "LABEL": "Intensity",
      "TYPE": "float",
      "MAX": 4,
      "MIN": 0,
      "DEFAULT": 1
    },
    {
      "NAME": "uTorThick",
      "LABEL": "Toroid Thickness",
      "TYPE": "float",
      "MAX": 1,
      "MIN": 0,
      "DEFAULT": 0.25,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "uShape",
      "LABEL": "Shape",
      "TYPE": "long",
      "VALUES": [
        0,
        1,
        2,
        3,
        4,
        5,
        6,
        7
      ],
      "LABELS": [
        "Auto Morph",
        "Cube",
        "Prism",
        "Torus",
        "Torus Knot",
        "Sphere",
        "Octahedron",
        "Heart"
      ],
      "DEFAULT": 0,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "uXYrotate",
      "LABEL": "XY Rotation",
      "TYPE": "float",
      "MAX": 180,
      "MIN": -180,
      "DEFAULT": 60,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "uYZrotate",
      "LABEL": "YZ Rotation",
      "TYPE": "float",
      "MAX": 180,
      "MIN": -180,
      "DEFAULT": 68,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "uXZrotate",
      "LABEL": "XZ Rotation",
      "TYPE": "float",
      "MAX": 180,
      "MIN": -180,
      "DEFAULT": 52,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "uMorphSpeed",
      "LABEL": "Morph Speed",
      "TYPE": "float",
      "MAX": 4,
      "MIN": 0,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "uC1",
      "LABEL": "Color 1",
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
      "NAME": "uC2",
      "LABEL": "Color 2",
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
      "NAME": "uC3",
      "LABEL": "Color 3",
      "TYPE": "color",
      "DEFAULT": [
        1,
        0,
        0,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "uColMode",
      "LABEL": "Color Mode",
      "TYPE": "long",
      "VALUES": [
        0,
        1
      ],
      "LABELS": [
        "Default",
        "Custom Palette"
      ],
      "DEFAULT": 0,
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
      "NAME": "uZoom",
      "LABEL": "Zoom",
      "TYPE": "float",
      "MAX": 1,
      "MIN": -1,
      "DEFAULT": 0,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "uDisplay",
      "LABEL": "Display",
      "TYPE": "long",
      "VALUES": [
        0,
        1,
        2
      ],
      "LABELS": [
        "Object + BG",
        "Object Only",
        "Background Only"
      ],
      "DEFAULT": 0,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "bgColor",
      "TYPE": "color",
      "DEFAULT": [
        0,
        0,
        0,
        0
      ],
      "LABEL": "Background",
      "GROUP": "Background"
    }
  ]
}*/

#define PI 3.141592653589
#define TwoPI 6.283185307178

mat2 rot2D(float a) { return mat2(cos(a), -sin(a), sin(a), cos(a)); }

float sdBox2d(vec2 p, vec2 s) {
    p = abs(p) - s;
    return length(max(p, 0.0)) + min(max(p.x, p.y), 0.0);
}

float sdBox(vec3 p, vec3 s) {
    p = abs(p) - s;
    return length(max(p, 0.0)) + min(max(p.x, max(p.y, p.z)), 0.0);
}

float sdTorus(vec3 p, float outRadius) {
    vec2 q = vec2(length(p.xz) - outRadius, p.y);
    return length(q) - uTorThick * outRadius;
}

float sdTorusKnots(vec3 p, float outRadius) {
    vec2 cp = vec2(length(p.xz) - outRadius, p.y);
    float a = atan(p.x, p.z);
    cp *= rot2D(a * 8.0);
    cp.y = abs(cp.y) - 0.3;
    return sdBox2d(cp, vec2(uTorThick * outRadius * 0.5, uTorThick * outRadius));
}

float sdTriPrism(vec3 p, vec2 h) {
    vec3 q = abs(p);
    return max(q.z - h.y, max(q.x * sin(PI / 3.0) + p.y * sin(PI / 6.0), -p.y) - h.x * sin(PI / 6.0));
}

float morphing(vec3 p) {
    float tm = TIME / 18.0 + audioBass * 0.5;
    // Shape override — uShape != 0 forces a single primitive.
    int s = int(uShape);
    if (s > 0) {
        if (s == 1) return sdBox(p, vec3(1.0));
        if (s == 2) return sdTriPrism(p, vec2(1.0, 1.5));
        if (s == 3) return sdTorus(p, 2.0);
        if (s == 4) return sdTorusKnots(p, 2.0) * 0.4;
        if (s == 5) return length(p) - 1.0;                     // sphere
        if (s == 6) {
            // Octahedron
            p = abs(p);
            return (p.x + p.y + p.z - 1.0) * 0.57735;
        }
        // Heart shape (s == 7) — implicit (x^2 + (5y/4 - sqrt|x|)^2 + z^2 - 1)
        float scale = 1.2;
        vec3 h = p * scale;
        float xx = h.x * h.x;
        float zz = h.z * h.z;
        float yy = (1.25 * h.y - sqrt(abs(h.x))) * (1.25 * h.y - sqrt(abs(h.x)));
        return (xx + yy + zz - 1.0) / scale;
    }

    // Auto-morph default
    // (audioBass folded back in here — the first `tm` assignment above was
    // being unconditionally overwritten, silently killing bass-driven morph.)
    tm = TIME * 0.4 * uMorphSpeed + audioBass * 0.9;
    int idx = int(mod(tm, 4.0));
    float a = smoothstep(0.2, 0.8, mod(tm, 1.0));
    if (idx == 0) return mix(sdTriPrism(p, vec2(1.0, 1.5)), sdBox(p, vec3(1.0)), a);
    if (idx == 1) return mix(sdBox(p, vec3(1.0)), sdTorus(p, 2.0), a);
    if (idx == 2) return mix(sdTorus(p, 2.0), sdTorusKnots(p, 2.0) * 0.4, a);
    return mix(sdTorusKnots(p, 2.0) * 0.4, sdTriPrism(p, vec2(1.0, 1.5)), a);
}

float distFunc(vec3 p) {
    // Mouse orbits the object
    float mx = (mousePos.x - 0.5) * TwoPI;
    float my = (mousePos.y - 0.5) * PI;
    p.xy *= rot2D(TIME * uXYrotate / 36.0 + mx);
    p.yz *= rot2D(TIME * uYZrotate / 36.0 + my);
    p.xz *= rot2D(TIME * uXZrotate / 36.0);
    return morphing(p);
}

vec3 getNormal(vec3 p) {
    vec2 e = vec2(0.1, 0.0);
    return normalize(vec3(
        distFunc(p + e.xyy) - distFunc(p - e.xyy),
        distFunc(p + e.yxy) - distFunc(p - e.yxy),
        distFunc(p + e.yyx) - distFunc(p - e.yyx)
    ));
}

vec3 background(vec3 rd) {
    vec3 colT = vec3(0.313, 0.816, 0.816);
    vec3 colM = vec3(0.745, 0.118, 0.243);
    vec3 colK = vec3(0.475, 0.404, 0.765);
    vec3 colH = vec3(1.0, 0.776, 0.224);
    float k = rd.y * 0.5 + 0.5;
    vec3 bg = vec3(1.0 - k);
    float a = atan(rd.x, rd.z);
    float fade = smoothstep(0.8, 0.5, k);
    bg += sin(a * 2.0 + TIME) * sin(a * 10.0 + TIME) * sin(a * 4.0) * fade * colT;
    bg += sin(a * 10.0 + TIME + 10.0) * sin(a * 2.0 + TIME + 10.0) * sin(a * 6.0 + 10.0) * fade * colM;
    bg += sin(a * 5.0 + TIME + 20.0) * sin(a * 3.0 + TIME + 30.0) * sin(a * 8.0 + 20.0) * fade * colK;
    bg += sin(a * 3.0 + TIME + 30.0) * sin(a * 5.0 + TIME + 20.0) * sin(a * 10.0 + 30.0) * fade * colH;
    // User background: blend the wave backdrop toward the chosen color
    // (also seen through the refractive object, like a real environment).
    return mix(bg, bgColor.rgb, bgColor.a);
}

void main() {
    float zm = (uZoom < 0.0) ? (1.0 - abs(uZoom)) * 0.25 : (1.0 + uZoom * 3.0) * 0.25;
    vec2 uv = (gl_FragCoord.xy - RENDERSIZE * 0.5) / (RENDERSIZE.y * zm);

    vec3 camPos = vec3(0.0, 0.0, -4.0);
    vec3 forward = vec3(0.0, 0.0, 1.0);
    vec3 right = vec3(1.0, 0.0, 0.0);
    vec3 up = vec3(0.0, 1.0, 0.0);
    vec3 rd = normalize(uv.x * right + uv.y * up + forward);

    vec3 color = vec3(0.0);
    float df = 0.0;
    float d = 0.0;
    vec3 p;
    int disp = int(uDisplay);

    for (int i = 0; i < 64; i++) {
        p = camPos + rd * d;
        df = distFunc(p);
        if (df > 100.0 || df <= 0.001) break;
        d += df;
    }

    // Soft AA mask: pixel-width edge feathering for object silhouette.
    // fwidth on ray-march distance gives a screen-space derivative; smoothstep
    // collapses it to a 0..1 coverage so cube/torus outlines feel resolved
    // rather than aliased.
    float pixW = fwidth(df);
    float hitMask = 1.0 - smoothstep(0.0, max(pixW * 1.5, 0.002), df);

    if (disp == 2) {
        color = background(rd);
    } else {
        if (df <= 0.001) {
            vec3 normal = getNormal(p);
            vec3 rdR = refract(rd, normal, 0.1);
            // Fallback when refract returns zero (total internal reflection edge):
            // reflect along the normal so highlights still register.
            if (dot(rdR, rdR) < 1e-4) rdR = reflect(rd, normal);
            vec3 obj = background(rdR);

            // Specular rim — view-aligned highlight peaks well above 1.0 so
            // bloom catches the silhouette without redesigning shading.
            float fres = pow(1.0 - max(dot(normal, -rd), 0.0), 4.0);
            float spec = pow(max(dot(reflect(rd, normal), -rd), 0.0), 32.0);
            vec3 hdrEdge = vec3(1.0, 1.05, 1.15) * (fres * 1.4 + spec * 2.5);
            obj += hdrEdge;

            // Glow edge: thin AA-feathered band along silhouette (driven by
            // fresnel) pushed into HDR territory for bloom catch.
            float glow = smoothstep(0.55, 0.95, fres);
            obj += vec3(1.1, 0.9, 1.4) * glow * 1.6;

            if (disp < 2) {
                color = mix(obj, background(rd), smoothstep(0.0, 4.0, d));
            } else {
                color = obj;
            }
        } else if (disp == 0) {
            color = mix(color, background(rd), smoothstep(0.0, 4.0, d));
        }
    }

    vec3 cOut = color;
    int cm = int(uColMode);
    if (cm == 1) {
        cOut = uC1.rgb * color.r + uC2.rgb * color.g + uC3.rgb * color.b;
    }
    cOut *= uIntensity;

    // Audio non-gating: a subtle bass pulse boosts HDR peak when audio is
    // present, but at audio=0 we still hold a baseline 1.0 multiplier so the
    // shader is fully alive without any sound input. Applied frame-wide
    // (object + background) so the pulse actually reads at a glance,
    // not just as a sliver on the object silhouette.
    float audioLift = 1.0 + audioBass * 3.6;
    cOut *= audioLift;

    // Surprise: every ~29s the color basis briefly inverts — the cube
    // turns inside-out for ~0.6s, mapping each axis to its complement.
    {
        float _ph = fract(TIME / 29.0);
        float _f  = smoothstep(0.0, 0.04, _ph) * smoothstep(0.20, 0.10, _ph);
        cOut = mix(cOut, 1.0 - cOut, _f);
    }

    // ---- universal color block (defaults = no-op; max() keeps linear HDR) ----
    vec3 uc = cOut;
    float ucL = dot(uc, vec3(0.299, 0.587, 0.114));
    uc = mix(vec3(ucL), uc, colorBoost);                     // saturation
    if (hueShift > 0.0005) {                                  // cheap hue rotate (YIQ)
        float hA = hueShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = max(hM * uc, 0.0);
    }
    cOut = uc;

    // No tonemap: pass linear HDR straight through. Phase Q v4 bloom downstream
    // expects peaks in the 1.4–2.5 range, which the fresnel/spec/glow above
    // produce on cube faces and silhouette edges.
    gl_FragColor = vec4(cOut, 1.0);
}

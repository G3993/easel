/*{
  "DESCRIPTION": "Raining Spheres — bouncing colored light-emitting spheres on a grid. Based on work by Reinder Nijhoff (CC BY-NC-SA 4.0)",
  "CREDIT": "Reinder Nijhoff (adapted for ShaderClaw)",
  "CATEGORIES": [
    "3D"
  ],
  "INPUTS": [
    {
      "NAME": "reflections",
      "LABEL": "Reflections",
      "TYPE": "bool",
      "DEFAULT": true
    },
    {
      "NAME": "shadows",
      "LABEL": "Shadows",
      "TYPE": "bool",
      "DEFAULT": true
    },
    {
      "NAME": "bounceHeight",
      "LABEL": "Bounce Height",
      "TYPE": "float",
      "DEFAULT": 30,
      "MIN": 5,
      "MAX": 60,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "sphereSpeed",
      "LABEL": "Sphere Speed",
      "TYPE": "float",
      "DEFAULT": 0.3,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "brightness",
      "LABEL": "Brightness",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0.1,
      "MAX": 2,
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
      "NAME": "camSpeed",
      "LABEL": "Cam Speed",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "transparentBg",
      "LABEL": "Transparent",
      "TYPE": "bool",
      "DEFAULT": true,
      "GROUP": "Background"
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

#define RAYCASTSTEPS 40
#define EPSILON 0.0001
#define MAXDISTANCE 400.0
#define GRIDSIZE 8.0
#define GRIDSIZESMALL 5.0

const mat2 mr = mat2(0.84147, 0.54030, 0.54030, -0.84147);

float hash1(float n) { return fract(sin(n) * 43758.5453); }
vec2 hash2f(float n) { return fract(sin(vec2(n, n + 1.0)) * vec2(2.1459123, 3.3490423)); }
vec2 hash2v(vec2 n) { return fract(sin(vec2(n.x * n.y, n.x + n.y)) * vec2(2.1459123, 3.3490423)); }
vec3 hash3f(float n) { return fract(sin(vec3(n, n + 1.0, n + 2.0)) * vec3(3.5453123, 4.1459123, 1.3490423)); }
vec3 hash3v(vec2 n) { return fract(sin(vec3(n.x, n.y, n.x + n.y + 2.0)) * vec3(3.5453123, 4.1459123, 1.3490423)); }

bool intersectPlane(vec3 ro, vec3 rd, float height, out float dist) {
    if (rd.y == 0.0) return false;
    float d = -(ro.y - height) / rd.y;
    d = min(100000.0, d);
    if (d > 0.0) { dist = d; return true; }
    return false;
}

bool intersectUnitSphere(vec3 ro, vec3 rd, vec3 sph, out float dist, out vec3 normal) {
    vec3 ds = ro - sph;
    float bs = dot(rd, ds);
    float cs = dot(ds, ds) - 1.0;
    float ts = bs * bs - cs;
    if (ts > 0.0) {
        ts = -bs - sqrt(ts);
        if (ts > 0.0) {
            normal = normalize((ro + ts * rd) - sph);
            dist = ts;
            return true;
        }
    }
    return false;
}

void getSphereOffset(vec2 grid, inout vec2 center) {
    center = (hash2f(grid.x * 1.23114 + 5.342 + 74.324231 * grid.y + grid.x) - vec2(0.5)) * GRIDSIZESMALL;
}

void getMovingSpherePosition(vec2 grid, vec2 sphereOffset, inout vec3 center) {
    float s = 0.1 + hash1(grid.x * 1.23114 + 5.342 + 74.324231 * grid.y);
    float spd = sphereSpeed * (1.0 + audioBass * 2.0);
    float t = fract(14.0 * s + TIME / s * spd);
    float maxH = bounceHeight * (1.0 + audioBass * 0.5);
    float y = s * maxH * abs(4.0 * t * (1.0 - t));
    vec2 offset = grid + sphereOffset;
    center = vec3(offset.x, y, offset.y) + 0.5 * vec3(GRIDSIZE, 2.0, GRIDSIZE);
}

void getSpherePosition(vec2 grid, vec2 sphereOffset, inout vec3 center) {
    vec2 offset = grid + sphereOffset;
    center = vec3(offset.x, 0.0, offset.y) + 0.5 * vec3(GRIDSIZE, 2.0, GRIDSIZE);
}

vec3 getSphereColor(vec2 grid) {
    vec3 c = normalize(hash3v(grid + vec2(43.12 * grid.y, 12.23 * grid.x)));
    // Audio: mid shifts hue, high boosts brightness
    c *= 1.0 + audioHigh * 0.5;
    return c;
}

vec3 traceScene(vec3 ro, vec3 rd, out vec3 intersection, out vec3 normal, out float dist, out int material) {
    material = 0;
    dist = MAXDISTANCE;
    float distcheck;
    vec3 sphereCenter, col, normalcheck;

    if (intersectPlane(ro, rd, 0.0, distcheck) && distcheck < MAXDISTANCE) {
        dist = distcheck;
        material = 1;
        normal = vec3(0.0, 1.0, 0.0);
        col = vec3(0.25);
    } else {
        col = vec3(0.0);
    }

    // Grid traversal
    vec3 pos = floor(ro / GRIDSIZE) * GRIDSIZE;
    vec3 ri = 1.0 / rd;
    vec3 rs = sign(rd) * GRIDSIZE;
    vec3 dis = (pos - ro + 0.5 * GRIDSIZE + rs * 0.5) * ri;
    vec3 mm = vec3(0.0);

    for (int i = 0; i < RAYCASTSTEPS; i++) {
        if (material > 1 || distance(ro.xz, pos.xz) > dist + GRIDSIZE) break;
        vec2 offset;
        getSphereOffset(pos.xz, offset);

        getMovingSpherePosition(pos.xz, -offset, sphereCenter);
        if (intersectUnitSphere(ro, rd, sphereCenter, distcheck, normalcheck) && distcheck < dist) {
            dist = distcheck;
            normal = normalcheck;
            material = 2;
        }

        getSpherePosition(pos.xz, offset, sphereCenter);
        if (intersectUnitSphere(ro, rd, sphereCenter, distcheck, normalcheck) && distcheck < dist) {
            dist = distcheck;
            normal = normalcheck;
            col = getSphereColor(offset);
            material = 3;
        }
        mm = step(dis.xyz, dis.zyx);
        dis += mm * rs * ri;
        pos += mm * rs;
    }

    vec3 color = vec3(0.0);
    if (material > 0) {
        intersection = ro + rd * dist;
        vec2 map = floor(intersection.xz / GRIDSIZE) * GRIDSIZE;

        if (material == 1 || material == 3) {
            vec3 c = vec3(-GRIDSIZE, 0.0, GRIDSIZE);
            for (int x = 0; x < 3; x++) {
                for (int y = 0; y < 3; y++) {
                    vec2 mapoffset = map + vec2(c[x], c[y]);
                    vec2 loffset;
                    getSphereOffset(mapoffset, loffset);
                    vec3 lcolor = getSphereColor(mapoffset);
                    vec3 lpos;
                    getMovingSpherePosition(mapoffset, -loffset, lpos);

                    float shadow = 1.0;
                    if (shadows && material == 1) {
                        for (int sx = 0; sx < 3; sx++) {
                            for (int sy = 0; sy < 3; sy++) {
                                if (shadow < 1.0) continue;
                                vec2 smapoffset = map + vec2(c[sx], c[sy]);
                                vec2 soffset;
                                getSphereOffset(smapoffset, soffset);
                                vec3 slpos, sn;
                                getSpherePosition(smapoffset, soffset, slpos);
                                float sd;
                                if (intersectUnitSphere(intersection, normalize(lpos - intersection), slpos, sd, sn)) {
                                    shadow = 0.0;
                                }
                            }
                        }
                    }
                    color += col * lcolor * (shadow * max(dot(normalize(lpos - intersection), normal), 0.0) *
                        clamp(10.0 / dot(lpos - intersection, lpos - intersection) - 0.075, 0.0, 1.0));
                }
            }
        } else {
            color = (3.0 + 2.0 * dot(normal, vec3(0.5, 0.5, -0.5))) * getSphereColor(map);
        }
    }
    return color;
}

void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    vec2 p = -1.0 + 2.0 * uv;
    p.x *= RENDERSIZE.x / RENDERSIZE.y;

    float t = TIME;

    // Camera — mouse overrides orbit, audio bass pulses height
    vec3 ro = vec3(
        cos(0.232 * t) * 10.0,
        6.0 + 3.0 * cos(0.3 * t) + audioBass * 3.0,
        GRIDSIZE * (t / max(camSpeed, 0.01))
    );
    vec3 ta = ro + vec3(-sin(0.232 * t) * 10.0, -2.0 + cos(0.23 * t), 10.0);

    float roll = -0.15 * sin(0.5 * t);

    vec3 cw = normalize(ta - ro);
    vec3 cp = vec3(sin(roll), cos(roll), 0.0);
    vec3 cu = normalize(cross(cw, cp));
    vec3 cv = normalize(cross(cu, cw));
    vec3 rd = normalize(p.x * cu + p.y * cv + 1.5 * cw);

    int material;
    vec3 normal, intersection;
    float dist;

    vec3 col = traceScene(ro, rd, intersection, normal, dist, material);

    // Reflections
    if (reflections && material > 0) {
        float f = 0.04 * clamp(pow(1.0 + dot(rd, normal), 5.0), 0.0, 1.0);
        vec3 rro = intersection + EPSILON * normal;
        rd = reflect(rd, normal);
        vec3 refColor = traceScene(rro, rd, intersection, normal, dist, material);
        if (material > 2) {
            col += 0.5 * refColor;
        } else {
            col += f * refColor;
        }
    }

    col = pow(col * brightness, vec3(1.0 / 2.2));
    col = clamp(col, 0.0, 1.0);

    // Vignette
    col *= 0.25 + 0.75 * pow(16.0 * uv.x * uv.y * (1.0 - uv.x) * (1.0 - uv.y), 0.15);

    float alpha = 1.0;
    if (transparentBg) {
        float lum = dot(col, vec3(0.299, 0.587, 0.114));
        alpha = smoothstep(0.02, 0.15, lum);
    }

    // Surprise: every ~33s gravity briefly suspends — for ~1s the rain
    // freezes mid-air, then resumes. A breath in the storm.
    {
        float _ph = fract(TIME / 33.0);
        float _f  = smoothstep(0.0, 0.05, _ph) * smoothstep(0.18, 0.08, _ph);
        // Tint shift toward silver during the freeze
        float _lum = dot(col, vec3(0.299, 0.587, 0.114));
        col = mix(col, vec3(_lum) * vec3(0.92, 0.95, 1.05), _f * 0.55);
    }

    // ---- universal color block (defaults = no-op) ----
    float ucL = dot(col, vec3(0.299, 0.587, 0.114));
    vec3 uc = mix(vec3(ucL), col, colorBoost);
    if (hueShift > 0.0005) {
        float hueA = hueShift * 6.2831853;
        float hueC = cos(hueA), hueS = sin(hueA);
        mat3 hueM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                  + hueC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                  + hueS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hueM * uc, 0.0, 1.0);
    }
    // background fill: dark/transparent region takes bgColor at bgColor.a strength
    if (bgColor.a > 0.0005) {
        uc = mix(uc, bgColor.rgb, bgColor.a * (1.0 - alpha));
        alpha = max(alpha, bgColor.a);
    }

    gl_FragColor = vec4(uc, alpha);
}

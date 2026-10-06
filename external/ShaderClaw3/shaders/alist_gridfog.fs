/*{
  "DESCRIPTION": "Grid Fog — a true 3D neon grid world: the camera slowly orbits a glowing wireframe plane receding to a real vanishing depth, a volumetric fog cloud hangs above it lit from below by the grid, and primitive solids — spheres, cubes, pyramids at hashed sizes — rise and sink through the cloud on slow vertical paths, rim-lit with grid-glow bounce. Beats send a ripple ring across the plane, bass leans the shapes' climb, mids stir the cloud, highs sparkle the grid lines.",
  "CREDIT": "ShaderClaw3 A-List VFX",
  "CATEGORIES": [
    "Generator",
    "3D",
    "Abstract",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "fogAmount",
      "LABEL": "Fog Cloud",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 1
    },
    {
      "NAME": "dripAmount",
      "LABEL": "Floating Shapes",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "flowSpeed",
      "LABEL": "Orbit Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "spectrumGlow",
      "LABEL": "Shape Glow",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "hueShift",
      "LABEL": "Hue Shift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Color"
    },
    {
      "NAME": "colorBoost",
      "LABEL": "Color Boost",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "gridAmount",
      "LABEL": "Grid Glow",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.5,
      "DEFAULT": 1,
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
        0
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "audioReactivity",
      "LABEL": "Sound Reactivity",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Audio Reactivity"
    },
    { "NAME": "trailAmt",   "LABEL": "Motion Trails", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.2,  "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",   "LABEL": "Bloom Depth",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.55, "GROUP": "Depth / Passes" },
    { "NAME": "aberration", "LABEL": "Lens Depth",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.35, "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// GRID FOG v2 — full 3D redesign. A raymarched night scene: an infinite
//   neon drafting grid on the ground plane (perspective-true, fading to a
//   real vanishing depth), a volumetric fbm fog slab floating above it —
//   lit from below by the grid so the cloud's belly glows — and seven
//   primitive solids (spheres, rounded cubes, pyramids at hashed sizes)
//   riding slow vertical sine paths up through the cloud and back down.
//   The camera orbits the scene center continuously with a gentle height
//   bob. Shapes are shaded with a key light, neon rim light, and a
//   grid-glow bounce from below; each pools light onto the plane beneath
//   it. Beats launch a ripple ring across the grid; audio is phase-push
//   and envelope-gated only (no time-warping) so motion never chops.
// ─────────────────────────────────────────────────────────────────────────

#define R RENDERSIZE.xy
#define TAU 6.2831853

float hash11(float n) { return fract(sin(n) * 43758.5453123); }
float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}
vec3 pal(float h) { return 0.5 + 0.5 * cos(TAU * h + vec3(0.0, 2.094, 4.188)); }

float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    return mix(mix(hash21(i), hash21(i + vec2(1.0, 0.0)), f.x),
               mix(hash21(i + vec2(0.0, 1.0)), hash21(i + vec2(1.0, 1.0)), f.x), f.y);
}
float fbm(vec2 p) {
    float v = 0.0, a = 0.5;
    for (int i = 0; i < 3; i++) {
        v += a * vnoise(p);
        p = p * 2.13 + vec2(7.3, 3.1);
        a *= 0.5;
    }
    return v;
}

float sdBox(vec3 p, vec3 b) {
    vec3 d = abs(p) - b;
    return length(max(d, 0.0)) + min(max(d.x, max(d.y, d.z)), 0.0);
}
// square pyramid: bottom-cut octahedron (reads as a floating pyramid)
float sdPyr(vec3 p, float s) {
    float oct = (abs(p.x) + abs(p.y) + abs(p.z) - s) * 0.57735;
    return max(oct, -p.y - s * 0.22);
}

// one object's animated parameters, deterministic per index
void objParams(float fi, float t, float amt, float bassP,
               out vec3 c, out float s, out float ht, out float hu) {
    float hx = hash11(fi * 7.31 + 2.0);
    float hz = hash11(fi * 3.77 + 5.0);
    float hp = hash11(fi * 9.13 + 1.0);
    float hs = hash11(fi * 5.51 + 3.0);
    ht = hash11(fi * 11.70 + 8.0);                       // type selector
    hu = hash11(fi * 4.97 + 6.0);                        // hue selector
    float angO = hx * TAU + t * 0.045 * ((hx > 0.5) ? 1.0 : -1.0);
    float rad = 0.75 + 1.85 * hz;
    // rise/sink on a slow sine; bass leans the climb (additive phase push)
    float yph = t * (0.14 + 0.20 * hp) + hp * TAU + amt * 0.10 * bassP;
    float y = 0.38 + 1.70 * (0.5 + 0.5 * sin(yph));
    c = vec3(cos(angO) * rad, y, sin(angO) * rad);
    s = 0.15 + 0.26 * hs;
}

// scene SDF over 7 primitives; id out for material lookup
float mapScene(vec3 p, float t, float amt, float bassP, float shapeAmt, out float outId) {
    float dmin = 1e3;
    outId = -1.0;
    for (int i = 0; i < 7; i++) {
        float fi = float(i);
        if (hash11(fi * 2.23 + 9.0) > 0.25 + 0.75 * shapeAmt) continue;
        vec3 c; float s, ht, hu;
        objParams(fi, t, amt, bassP, c, s, ht, hu);
        vec3 lp = p - c;
        // slow spin about y for the faceted solids
        float yaw = t * (0.20 + 0.25 * hu) * ((hu > 0.5) ? 1.0 : -1.0) + hu * TAU;
        float cy = cos(yaw), sy = sin(yaw);
        lp.xz = vec2(cy * lp.x - sy * lp.z, sy * lp.x + cy * lp.z);
        float d;
        if (ht < 0.34)      d = length(lp) - s;                          // sphere
        else if (ht < 0.67) d = sdBox(lp, vec3(s * 0.72)) - s * 0.06;    // rounded cube
        else                d = sdPyr(lp, s * 1.15);                     // pyramid
        if (d < dmin) { dmin = d; outId = fi; }
    }
    return dmin;
}

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 uv = (fc - 0.5 * R) / R.y;

    float t    = TIME * flowSpeed * 0.55;
    float amt  = audioReactivity;
    float bassP  = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP   = pow(smoothstep(0.06, 0.85, audioMid),  1.2);
    float highP  = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float levelP = clamp(audioLevel, 0.0, 1.0);
    float beatP  = clamp(audioBeatPulse, 0.0, 1.0);

    vec3 hueA = pal(hueShift + 0.52);                    // cyan family
    vec3 hueB = pal(hueShift + 0.90);                    // magenta family
    vec3 hueC = pal(hueShift + 0.14);                    // amber family

    // ── orbiting camera with height bob ─────────────────────────────────
    float ca = t * 0.16;
    float camR = 4.05 + 0.35 * sin(t * 0.11);
    vec3 ro = vec3(cos(ca) * camR, 2.55 + 0.38 * sin(t * 0.13), sin(ca) * camR);
    vec3 ta = vec3(0.0, 0.78, 0.0);
    vec3 fw = normalize(ta - ro);
    vec3 rt = normalize(cross(fw, vec3(0.0, 1.0, 0.0)));
    vec3 up = cross(rt, fw);
    vec3 rd = normalize(fw * 1.35 + rt * uv.x + up * uv.y);

    // ── sky: deep night gradient + drifting stars ───────────────────────
    float horiz = smoothstep(-0.05, 0.45, rd.y);
    vec3 col = mix(vec3(0.045, 0.030, 0.085), vec3(0.008, 0.010, 0.026), horiz);
    col += hueB * 0.10 * exp(-abs(rd.y) * 7.0);                          // horizon bloom
    col += hueA * 0.04 * exp(-abs(rd.y - 0.35) * 5.0);
    col += mix(hueB, hueC, 0.5 + 0.5 * sin(rd.x * 2.0 + t * 0.07)) * 0.07
         * fbm(rd.xy * 3.0 + vec2(t * 0.04, 0.0)) * smoothstep(-0.1, 0.4, rd.y);
    vec2 sq = rd.xz / (0.35 + max(rd.y, 0.05));                           // star dome coords
    float sh = hash21(floor(sq * 26.0) + 7.0);
    col += vec3(0.7, 0.8, 1.0) * step(0.85, sh) * step(0.08, rd.y)
         * pow(0.5 + 0.5 * sin(t * 1.4 + sh * TAU), 10.0) * 0.55;
    // distant data-city: fine vertical glints hugging the horizon
    float az = atan(rd.x, rd.z);
    float glint = pow(0.5 + 0.5 * sin(az * 140.0 + sin(az * 31.0) * 2.0), 18.0);
    float glint2 = pow(0.5 + 0.5 * sin(az * 57.0 + 2.1), 22.0);
    col += mix(hueA, hueB, 0.5 + 0.5 * sin(az * 3.0)) * (glint * 0.30 + glint2 * 0.22)
         * exp(-abs(rd.y) * 9.0) * (0.8 + amt * 0.3 * highP);

    // ── raymarch the floating primitives ────────────────────────────────
    float tPlane = (rd.y < -0.02) ? (-ro.y / rd.y) : 1e3;
    float tMax = min(tPlane, 13.0);
    float tm = 0.0, hitId = -1.0;
    bool hit = false;
    for (int i = 0; i < 44; i++) {
        vec3 p = ro + rd * tm;
        float oid;
        float d = mapScene(p, t, amt, bassP, dripAmount, oid);
        if (d < 0.0015 * tm + 0.0009) { hit = true; hitId = oid; break; }
        tm += d * 0.92;
        if (tm > tMax) break;
    }

    float sceneT = 1e3;                                   // depth of what we shade
    if (hit) {
        sceneT = tm;
        vec3 p = ro + rd * tm;
        float oid;
        vec2 eps = vec2(0.0035, -0.0035);
        float d0 = mapScene(p + eps.xxx, t, amt, bassP, dripAmount, oid);
        float d1 = mapScene(p + eps.xyy, t, amt, bassP, dripAmount, oid);
        float d2 = mapScene(p + eps.yxy, t, amt, bassP, dripAmount, oid);
        float d3 = mapScene(p + eps.yyx, t, amt, bassP, dripAmount, oid);
        vec3 n = normalize(vec3(d0 + d1 - d2 - d3, d0 - d1 + d2 - d3, d0 - d1 - d2 + d3));
        vec3 c; float s, htp, hu;
        objParams(hitId, t, amt, bassP, c, s, htp, hu);
        vec3 objHue = pal(hueShift + 0.10 + 0.80 * hu);
        // dark glass body + key light
        vec3 L = normalize(vec3(0.45, 0.80, 0.30));
        float dif = max(dot(n, L), 0.0);
        vec3 body = mix(vec3(0.020, 0.024, 0.045), objHue * 0.16, 0.35 + 0.35 * dif);
        // neon rim light
        float rim = pow(1.0 - max(dot(n, -rd), 0.0), 3.0);
        body += objHue * rim * spectrumGlow * (0.95 + amt * 0.55 * highP);
        // grid-glow bounce from below: the plane lights the shapes' bellies
        float belly = max(-n.y, 0.0);
        body += hueA * belly * exp(-c.y * 0.9) * gridAmount * 0.55;
        // tight specular
        body += vec3(1.0) * pow(max(dot(reflect(-L, n), -rd), 0.0), 34.0) * 0.8;
        // height fog on far objects
        float atten = exp(-tm * 0.045);
        col = mix(vec3(0.030, 0.026, 0.060), body, atten);
    }

    // ── the grid plane (only where nothing closer was hit) ──────────────
    if (tPlane < sceneT && tPlane < 1e2) {
        sceneT = tPlane;
        vec3 hp = ro + rd * tPlane;
        float rr = length(hp.xz);
        float fade = exp(-tPlane * 0.095);
        // two-scale drafting grid, crisp lines with depth-stable widths
        vec2 f1 = abs(fract(hp.xz * 1.7) - 0.5);
        vec2 f4 = abs(fract(hp.xz * 6.8) - 0.5);
        float dl1 = min(f1.x, f1.y) / 1.7;
        float dl4 = min(f4.x, f4.y) / 6.8;
        float lw = 0.010 + tPlane * 0.0035;
        float lw4 = 0.005 + tPlane * 0.0016;
        float g1 = smoothstep(lw, lw * 0.30, dl1);
        float glow1 = exp(-dl1 * dl1 / (lw * lw * 20.0)) * 0.50;
        float g4 = smoothstep(lw4, lw4 * 0.30, dl4) * smoothstep(12.0, 3.0, tPlane) * 0.55;
        vec2 f8 = abs(fract(hp.xz * 13.6) - 0.5);
        float dl8 = min(f8.x, f8.y) / 13.6;
        float g8 = smoothstep(lw4 * 0.7, lw4 * 0.2, dl8) * smoothstep(5.0, 1.5, tPlane) * 0.30;
        // hue drifts with radius; sparse accent cells breathe
        vec3 gcol = mix(hueA, hueB, 0.5 + 0.5 * sin(rr * 0.55 - t * 0.35));
        float cellH = hash21(floor(hp.xz * 1.7) + 3.1);
        float accent = step(0.80, cellH) * (0.5 + 0.5 * sin(t * 1.1 + cellH * TAU));
        // beat ripple: a ring races outward from the center
        float ringR = fract(t * 0.30) * 8.5;
        float ripple = exp(-pow((rr - ringR) * 1.9, 2.0)) * amt * beatP;
        float gridI = gridAmount * fade
                    * (g1 * (1.10 + 0.60 * accent + amt * 0.32 * highP) + glow1 + g4 + g8 + ripple * 1.6);
        vec3 plane = vec3(0.028, 0.032, 0.062);           // plate base
        plane += gcol * gridI;
        plane += gcol * accent * 0.10 * fade;             // accent cells carry a faint fill glow
        plane += hueC * exp(-rr * 0.55) * 0.10;           // warm pool at world center
        // light pools + faint mirrored glow under each shape
        for (int i = 0; i < 7; i++) {
            float fi = float(i);
            if (hash11(fi * 2.23 + 9.0) > 0.25 + 0.75 * dripAmount) continue;
            vec3 c; float s, htp, hu;
            objParams(fi, t, amt, bassP, c, s, htp, hu);
            float dxy = length(hp.xz - c.xz);
            vec3 objHue = pal(hueShift + 0.10 + 0.80 * hu);
            plane += objHue * exp(-dxy * dxy / (s * s * 4.0)) * exp(-c.y * 0.85)
                   * spectrumGlow * 0.58 * fade;
        }
        col = mix(col, plane, smoothstep(0.0, 0.02, 1e2 - tPlane));
        col = mix(vec3(0.030, 0.026, 0.060), col, exp(-tPlane * 0.040));  // depth haze
    }

    // ── volumetric fog cloud floating above the grid ────────────────────
    float slabLo = 1.15, slabHi = 2.80;
    float rdy = (abs(rd.y) < 1e-4) ? 1e-4 : rd.y;
    float t0 = (slabLo - ro.y) / rdy;
    float t1 = (slabHi - ro.y) / rdy;
    float ts = min(t0, t1), te = max(t0, t1);
    ts = max(ts, 0.0);
    te = min(te, min(sceneT, 12.0));
    if (te > ts && fogAmount > 0.001) {
        float dt = (te - ts) / 8.0;
        float trans = 1.0;
        vec3 acc = vec3(0.0);
        float tf = t * 0.5 + amt * 0.8 * midP;            // mids stir the cloud (phase push)
        for (int i = 0; i < 8; i++) {
            float tc = ts + (float(i) + 0.5) * dt;
            vec3 p = ro + rd * tc;
            float hgt = smoothstep(slabLo, slabLo + 0.30, p.y) * smoothstep(slabHi, slabHi - 0.50, p.y);
            float dn = fbm(p.xz * 0.60 + vec2(tf * 0.22, -tf * 0.15) + fbm(p.xz * 0.9 - tf * 0.08) * 1.1);
            float dens = fogAmount * hgt * smoothstep(0.28, 0.72, dn);
            float sigma = dens * dt * 1.45;
            // a MILKY luminous cloud: grid-lit belly, cool crown, satin body
            float crown = smoothstep(slabLo, slabHi, p.y);
            vec3 fcol = mix(vec3(0.86, 0.89, 0.97) * (0.55 + 0.45 * dn),
                            vec3(0.30, 0.30, 0.52), crown);
            fcol += hueA * (1.0 - crown) * 0.35 * gridAmount;
            // the old inner spectrum, reborn: neon rainbow glowing through the thick middle
            vec3 spec = pal(hueShift + 0.84 + dn * 0.55 + 0.10 * sin(tf * 0.3));
            fcol += spec * smoothstep(0.45, 0.85, dn) * spectrumGlow * 0.70;
            acc += trans * fcol * sigma;
            trans *= exp(-sigma);
        }
        col = col * trans + acc;
    }

    // ── finish: film grain + level follower (dark scene, additive-safe) ─
    float gr = hash21(fc + fract(t) * vec2(17.0, 29.0)) - 0.5;
    col += 0.020 * gr;
    col *= mix(1.0, 0.84 + 0.22 * levelP + 0.14 * clamp(audioBass, 0.0, 1.0)
                    + 0.08 * clamp(audioMid, 0.0, 1.0) + 0.10 * beatP, clamp(amt, 0.0, 1.0));
    // universal color block (defaults = no-op)
    float ucL = dot(col, vec3(0.299, 0.587, 0.114));
    col = mix(vec3(ucL), col, colorBoost);
    // background color: blends into the empty sky
    float bgMask = (1.0 - smoothstep(0.0, 0.25, ucL));
    col = mix(col, bgColor.rgb, bgColor.a * clamp(bgMask, 0.0, 1.0));
    return col;
}

// ─── multipass: trail accumulation → bloom + lens-depth composite ────────
void main() {
    vec2 uv = gl_FragCoord.xy / R;
    if (PASSINDEX == 0) {
        vec3 col = renderScene();
        // luminous motion trails persist in the buffer (8-bit-safe decay floor)
        vec3 prev = texture2D(abTrail, uv).rgb;
        float decay = 0.50 + 0.46 * trailAmt;
        col = max(col, prev * decay - 0.0045);
        // write-dither: keeps soft gradients band-free in 8-bit buffer hosts
        col += (hash21(gl_FragCoord.xy + fract(TIME) * 61.0) - 0.5) * 0.006;
        gl_FragColor = vec4(clamp(col, 0.0, 1.0), 1.0);
    } else {
        // chromatic lens depth: R/B pulled apart along the radial axis
        vec2 dir = uv - 0.5;
        float ab = aberration * 0.0045;
        vec3 base;
        base.r = texture2D(abTrail, uv + dir * ab).r;
        base.g = texture2D(abTrail, uv).g;
        base.b = texture2D(abTrail, uv - dir * ab).b;
        // wide two-ring bloom of the bright field — real glow depth
        vec3 bl = vec3(0.0);
        for (int i = 0; i < 8; i++) {
            float a = float(i) * 0.7853982;
            vec2 o = vec2(cos(a), sin(a)) * (3.5 / R.y);
            bl += texture2D(abTrail, uv + o).rgb;
            bl += texture2D(abTrail, uv + o * 2.6).rgb * 0.6;
        }
        bl /= 12.8;
        bl = max(bl - 0.52, 0.0);
        vec3 col = base + bl * bl * bloomAmt * 1.8;
        // HD finish: S-curve contrast + saturation richness
        col = clamp(col, 0.0, 1.0);
        col = mix(col, col * col * (3.0 - 2.0 * col), 0.55);
        float lumHD = dot(col, vec3(0.299, 0.587, 0.114));
        col = clamp(mix(vec3(lumHD), col, 1.22), 0.0, 1.0);
        gl_FragColor = vec4(col, 1.0);
    }
}

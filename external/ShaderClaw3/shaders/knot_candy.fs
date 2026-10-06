/*{
  "DESCRIPTION": "Knot Candy — a real raymarched 3D knot of glossy candy tubes floating in the void: three intertwined strands twist around a shared ring like taffy, skinned in a flowing rainbow gradient with soft studio lighting, fresnel rim and a floor of pure black. The knot slowly tumbles; bass inflates the tubes, mids spin the twist, beats roll the rainbow around the surface, highs tighten the specular sparkle.",
  "CATEGORIES": ["Generator", "3D", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "knotTwist",    "LABEL": "Twist",         "TYPE": "float", "MIN": 0.5, "MAX": 4.0, "DEFAULT": 1.5,  "GROUP": "Shape / Geometry" },
    { "NAME": "tubeFat",      "LABEL": "Tube Fatness",  "TYPE": "float", "MIN": 0.3, "MAX": 1.4, "DEFAULT": 0.8,  "GROUP": "Shape / Geometry" },
    { "NAME": "strandSep",    "LABEL": "Strand Spread", "TYPE": "float", "MIN": 0.2, "MAX": 1.2, "DEFAULT": 0.7,  "GROUP": "Shape / Geometry" },
    { "NAME": "hueShift",     "LABEL": "Hue Shift",     "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "hueSpread",    "LABEL": "Hue Spread",    "TYPE": "float", "MIN": 0.2, "MAX": 2.0, "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "glowAmt",      "LABEL": "Rim Glow",      "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.5,  "GROUP": "Color" },
    { "NAME": "brightness",   "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3, "MAX": 2.0, "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "spinSpeed",    "LABEL": "Spin Speed",    "TYPE": "float", "MIN": 0.0, "MAX": 3.0, "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "audioReact",   "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.5,  "GROUP": "Audio Reactivity" }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// KNOT CANDY — from the twisted rainbow blob reference: a true raymarched
//   SDF. Three tube strands ride a torus ring; their cross-section
//   centers rotate around the ring at knotTwist revolutions per lap, so
//   the strands braid through each other like the reference. Material is
//   a cos-palette rainbow keyed to ring angle + twist phase + normal, so
//   color flows along the braid; lighting = key + fill + fresnel rim on
//   black. Marched at 56 steps with a relaxed epsilon — cheap enough for
//   mobile, still silky. Fuses: fisheye_sphere raymarch shell, orby's
//   airbrushed-glow finish, and the flowing gradients of the La B silks.
// ─────────────────────────────────────────────────────────────────────────

#define R RENDERSIZE.xy
#define TAU 6.2831853

float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}
vec3 pal(float h) { return 0.5 + 0.5 * cos(TAU * h + vec3(0.0, 2.094, 4.188)); }

float gTwist, gFat, gSep, gTime;

// braid SDF: p in object space → distance + strand phase (out via global)
float gPhase;
float mapSDF(vec3 p) {
    float ringA = atan(p.z, p.x);                       // angle around the ring
    vec2 qc = vec2(length(p.xz) - 1.0, p.y);            // cross-section plane
    float tw = ringA * gTwist + gTime * 0.5;
    float d = 1e3;
    for (int j = 0; j < 3; j++) {
        float fj = float(j);
        float a = tw + fj * TAU / 3.0;
        vec2 c = gSep * 0.32 * vec2(cos(a), sin(a));
        float dj = length(qc - c) - 0.22 * gFat;
        if (dj < d) { d = dj; gPhase = ringA / TAU + fj / 3.0; }
    }
    return d;
}

vec3 calcN(vec3 p) {
    vec2 e = vec2(0.0012, 0.0);
    return normalize(vec3(
        mapSDF(p + e.xyy) - mapSDF(p - e.xyy),
        mapSDF(p + e.yxy) - mapSDF(p - e.yxy),
        mapSDF(p + e.yyx) - mapSDF(p - e.yyx)));
}

void main() {
    vec2 fc = gl_FragCoord.xy;
    vec2 uv = (fc - 0.5 * R) / R.y;

    float t   = TIME * spinSpeed * 0.4;
    float amt = audioReact;
    float bassP  = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP   = pow(smoothstep(0.06, 0.85, audioMid),  1.2);
    float highP  = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float levelP = clamp(audioLevel, 0.0, 1.0);
    float beatP  = clamp(audioBeatPulse, 0.0, 1.0);

    gTwist = knotTwist;
    gFat   = tubeFat * (1.0 + amt * 0.30 * bassP);
    gSep   = strandSep;
    gTime  = t + amt * 0.6 * midP;      // phase push, not time-scale (chop-free)

    // camera orbits and nods slowly
    float ca = t * 0.30, cb = 0.45 + 0.25 * sin(t * 0.17);
    vec3 ro = 2.9 * vec3(cos(ca) * cos(cb), sin(cb), sin(ca) * cos(cb));
    vec3 ww = normalize(-ro);
    vec3 uu = normalize(cross(ww, vec3(0.0, 1.0, 0.0)));
    vec3 vv = cross(uu, ww);
    vec3 rd = normalize(uv.x * uu + uv.y * vv + 1.6 * ww);

    // ── march ───────────────────────────────────────────────────────────
    float dist = 0.0; float hit = -1.0; float ph = 0.0;
    for (int i = 0; i < 72; i++) {
        vec3 p = ro + rd * dist;
        float d = mapSDF(p);
        if (d < 0.002) { hit = 1.0; ph = gPhase; break; }
        dist += d * 0.85;
        if (dist > 7.0) break;
    }

    // ── shade ───────────────────────────────────────────────────────────
    vec3 col = vec3(0.008, 0.008, 0.012);
    // faint ambient halo behind the knot
    col += pal(hueShift + 0.6) * 0.05 * exp(-dot(uv, uv) * 2.0) * glowAmt;

    if (hit > 0.0) {
        vec3 p = ro + rd * dist;
        vec3 n = calcN(p);
        float hue = hueShift + hueSpread * (ph + 0.20 * n.y)
                  + t * 0.03 + amt * 0.05 * beatP;
        vec3 base = pal(hue);
        base = mix(base, vec3(1.0), 0.12);                       // candy lift
        vec3 L1 = normalize(vec3(0.6, 0.8, 0.3));
        vec3 L2 = normalize(vec3(-0.6, -0.2, 0.6));
        float dif = max(dot(n, L1), 0.0) * 0.85 + max(dot(n, L2), 0.0) * 0.35 + 0.18;
        float spe = pow(max(dot(normalize(L1 - rd), n), 0.0), mix(24.0, 90.0, amt * highP)) ;
        float fre = pow(1.0 - max(dot(n, -rd), 0.0), 3.0);
        col = base * dif + vec3(1.0) * spe * 0.7 + pal(hue + 0.5) * fre * (0.35 + 0.5 * glowAmt);
        // depth fog into the void
        col *= exp(-max(dist - 2.2, 0.0) * 0.35);
    }

    // ── finish ──────────────────────────────────────────────────────────
    float gr = hash21(fc + fract(t) * vec2(17.0, 29.0)) - 0.5;
    col += 0.02 * gr;
    col *= brightness * mix(1.0, 0.78 + 0.34 * levelP + 0.26 * clamp(audioBass, 0.0, 1.0)
                                 + 0.14 * clamp(audioMid, 0.0, 1.0) + 0.16 * beatP, amt);
    // HD finish: S-curve contrast + saturation richness — deep blacks, no fade
    col = clamp(col, 0.0, 1.0);
    col = mix(col, col * col * (3.0 - 2.0 * col), 0.55);
    float lumHD = dot(col, vec3(0.299, 0.587, 0.114));
    col = clamp(mix(vec3(lumHD), col, 1.22), 0.0, 1.0);
    gl_FragColor = vec4(col, 1.0);
}

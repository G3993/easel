/*{
  "DESCRIPTION": "Octa Strings — a neon octagon instrument in a living tunnel: volumetric tube rings (bright rim, dark core) recede past textured octagonal walls with crisp flying rulings, corner rails and glitter in the light cone, while the camera sways off-axis. Eight harp strings play the spectrum — bass low, mids center, highs top — with plucked-string harmonic overtones and specular hot-spots riding the traveling wave, mirrored in a smeared reflective floor. Dust motes drift through the light; beats send a pulse ring down the tunnel.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "ringCount",    "LABEL": "Ring Count",     "TYPE": "float", "MIN": 3.0, "MAX": 10.0, "DEFAULT": 7.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "twist",        "LABEL": "Ring Twist",     "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.5,  "GROUP": "Shape / Geometry" },
    { "NAME": "stringVibe",   "LABEL": "String Vibrato", "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Shape / Geometry" },
    { "NAME": "glowAmt",      "LABEL": "Neon Glow",      "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.7,  "GROUP": "Color" },
    { "NAME": "hueBase",      "LABEL": "Hue Base",       "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.55, "GROUP": "Color" },
    { "NAME": "hueSpread",    "LABEL": "Hue Spread",     "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.5,  "GROUP": "Color" },
    { "NAME": "brightness",   "LABEL": "Brightness",     "TYPE": "float", "MIN": 0.3, "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "tunnelSpeed",  "LABEL": "Tunnel Speed",   "TYPE": "float", "MIN": 0.0, "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "audioReact",   "LABEL": "Audio React",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.5,  "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",   "LABEL": "Motion Trails", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.6, "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",   "LABEL": "Bloom Depth",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.55, "GROUP": "Depth / Passes" },
    { "NAME": "aberration", "LABEL": "Lens Depth",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.35, "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// OCTA STRINGS — essence fusion of the AVA OS octagon + its master-strip
//   frequency strings + the star_trek capsule tunnel + neon text-motion
//   flicker. Octagon SDF via angular fold; rings live at exponential
//   depths flying toward camera (fract depth loop, fog by depth), each
//   ring twisted a little more than the last. Eight horizontal strings
//   cross the portal; string k vibrates as a standing wave whose gain is
//   a smooth per-band blend (bass→low, mid→center, high→top), so the
//   portal literally plays the spectrum. Beat launches a bright pulse
//   ring; palette runs on a controllable hue base/spread.
// ─────────────────────────────────────────────────────────────────────────

#define R RENDERSIZE.xy
#define PI 3.14159265
#define TAU 6.2831853

float hash11(float n) { return fract(sin(n) * 43758.5453123); }
float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}
vec3 pal(float h) { return 0.5 + 0.5 * cos(TAU * h + vec3(0.0, 2.094, 4.188)); }

// signed octagon distance (radius 1 across flats), with rotation
float octagon(vec2 p, float rot) {
    float ca = cos(rot), sa = sin(rot);
    p = vec2(ca * p.x - sa * p.y, sa * p.x + ca * p.y);
    float a = atan(p.y, p.x);
    float seg = TAU / 8.0;
    a = mod(a + seg * 0.5, seg) - seg * 0.5;
    return length(p) * cos(a) / cos(seg * 0.5);
}

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 q = (fc - 0.5 * R) / R.y;

    float t   = TIME * tunnelSpeed * 0.6;
    float amt = audioReact;
    float bassP  = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP   = pow(smoothstep(0.06, 0.85, audioMid),  1.2);
    float highP  = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float levelP = clamp(audioLevel, 0.0, 1.0);
    float beatP  = clamp(audioBeatPulse, 0.0, 1.0);

    // camera sway: the vanishing point drifts on two incommensurate orbits
    vec2 sway = vec2(0.050 * sin(t * 0.26) + 0.020 * sin(t * 0.71),
                     0.036 * cos(t * 0.21) + 0.015 * sin(t * 0.53));
    float tilt = 0.09 * sin(t * 0.17) + 0.05 * sin(t * 0.043 + 1.3);

    // ── nebula backdrop + tunnel light cone ─────────────────────────────
    float rq = length(q);
    vec3 hazeA = pal(hueBase + hueSpread * 0.55);
    vec3 hazeB = pal(hueBase + hueSpread * 0.10);
    vec3 col = mix(vec3(0.016, 0.018, 0.042), vec3(0.060, 0.040, 0.110),
                   1.0 - smoothstep(0.0, 0.95, rq));
    col += hazeA * 0.13 * (1.0 - smoothstep(0.0, 0.80, length(q - sway * 2.2)));
    col += hazeB * 0.08 * smoothstep(0.6, 0.0, rq);
    col += pal(hueBase + hueSpread * 0.9) * 0.05 * smoothstep(0.35, 1.0, rq);
    float dustBg = hash21(floor(q * 90.0));
    col += vec3(0.07, 0.08, 0.12) * step(0.985, dustBg) * (0.5 + 0.5 * sin(t * 3.0 + dustBg * 40.0));

    // ── tunnel walls: receding octagonal slats, hued per sector ─────────
    vec2 qs = q - sway;
    float thq = atan(qs.y, qs.x);
    float railRot = t * 0.05;
    float od0 = octagon(qs, railRot);
    float wcoord = 0.55 / max(od0, 0.045);                       // wall depth coordinate
    float wfog = smoothstep(0.05, 0.45, od0);                    // deep center fades out
    float sector = floor(4.0 * (thq - railRot) / PI + 0.5);
    float hsec = hash21(vec2(sector, 3.0));
    vec3 wallCol = pal(hueBase + hueSpread * (0.12 + 0.62 * hsec));
    float wphase = fract(wcoord - t * 0.50);
    float slat = smoothstep(0.0, 0.07, wphase) * smoothstep(0.52, 0.28, wphase);
    col += wallCol * slat * wfog * (0.11 + amt * 0.08 * midP);
    // crisp octagonal wall rulings flying outward with the slats
    float wline = smoothstep(0.050, 0.016, abs(wphase - 0.30))
                + 0.6 * smoothstep(0.035, 0.010, abs(wphase - 0.72));
    col += wallCol * wline * wfog * (0.44 + amt * 0.18 * midP);
    // fine ridge filaments riding the walls (micro-detail, sharp)
    float ridge = pow(0.5 + 0.5 * sin(wcoord * 46.0 - t * 23.0 + sector * 1.7), 8.0);
    col += wallCol * ridge * wfog * 0.20;
    // glitter suspended in the light: fine cells breathing on hashed phases
    vec2 gcell = floor(qs * 150.0);
    float gh = hash21(gcell + 7.7);
    float glit = pow(0.5 + 0.5 * sin(t * 2.4 + gh * TAU), 14.0) * step(0.86, gh);
    col += mix(wallCol, vec3(1.0), 0.55) * glit * wfog * 0.35;

    // 8 corner rails receding to the vanishing point
    float railA = abs(sin(4.0 * (thq - railRot)));
    float rail = smoothstep(0.075, 0.014, railA * rq) * smoothstep(0.16, 0.55, rq);
    col += pal(hueBase + hueSpread * 0.75) * rail * (0.24 + amt * 0.18 * highP);

    // ── octagon tunnel: neon-tube rings at exponential depths ───────────
    float rc = floor(ringCount + 0.5);
    float breathe = 1.0 + amt * 0.10 * levelP + 0.02 * sin(t * 0.7);
    for (int i = 0; i < 10; i++) {
        float fi = float(i);
        if (fi >= rc) continue;
        float dz = fract(fi / rc - t * 0.12);
        float z = mix(0.14, 3.4, dz * dz);                       // exponential recession
        float rad = breathe * 0.92 / z;
        float rot = twist * (0.5 * dz * TAU * 0.25) + t * 0.10 * ((mod(fi, 2.0) < 1.0) ? 1.0 : -1.0);
        // perspective: near rings sway harder + tilt foreshortens them
        vec2 pr = q - sway * (1.0 - dz) * 1.7;
        pr.y *= 1.0 + tilt * (1.0 - dz) * 0.9;
        pr.y += tilt * rad * 0.55 * (1.0 - dz);
        float od = octagon(pr, rot);
        float d = abs(od - rad);
        float fog = smoothstep(0.0, 1.0, dz);
        float hue = hueBase + hueSpread * (0.35 * dz + 0.09 * fi / rc);
        float lw = 0.0065 + 0.011 * (1.0 - dz);
        // neon TUBE: bright rim, slightly darker core line = volumetric glass
        float tube = smoothstep(lw, lw * 0.35, d);
        float coreDip = smoothstep(lw * 0.28, 0.0, d);
        float tubeI = tube * (1.05 - 0.38 * coreDip);
        vec3 ringCol = pal(hue) * (1.0 - 0.72 * fog);
        col = mix(col, max(col, ringCol * tubeI), tube);
        col += ringCol * glowAmt * (exp(-d * 150.0) * 0.55 + exp(-d * (42.0 + 85.0 * dz)) * 0.45)
             * (0.85 - 0.55 * fog);
        // corner studs: the 8 vertices carry bright beads
        float thr = atan(pr.y, pr.x);
        float stud = pow(abs(sin(4.0 * (thr - rot))), 28.0);
        col += mix(ringCol, vec3(1.0), 0.4) * stud * tube * (0.9 - 0.6 * fog);
        // beat pulse: a bright ghost ring sweeping down the tunnel
        float pz = fract(t * 0.55);
        float pulse = exp(-pow((dz - pz) * 9.0, 2.0)) * amt * beatP;
        col += ringCol * pulse * tube * 2.2;
    }

    // ── strings + reflective floor ──────────────────────────────────────
    float portal = 0.60 * breathe;
    float inPortal = smoothstep(portal + 0.05, portal - 0.05, octagon(q - sway * 1.2, t * 0.05));
    float floorY = -0.455;
    float belowF = smoothstep(floorY, floorY - 0.03, q.y);       // 1 below the floor line
    // soft light fill inside the portal so the instrument sits in lit air
    col += hazeA * inPortal * 0.16 * (0.8 + 0.2 * sin(t * 0.5));
    // faint floor line + sheen + perspective floor grid
    col += hazeB * 0.16 * smoothstep(0.004, 0.0012, abs(q.y - floorY)) * smoothstep(0.85, 0.2, abs(q.x));
    col += hazeA * belowF * 0.05 * smoothstep(-0.62, floorY, q.y);
    float pz2 = 0.10 / max(floorY - q.y, 0.004);                 // floor depth coordinate
    float gFade = smoothstep(16.0, 3.0, pz2);                    // fades at the horizon
    float gl1 = smoothstep(0.07, 0.022, abs(fract(q.x * pz2 * 5.0 + 0.5) - 0.5));
    float gl2 = smoothstep(0.07, 0.022, abs(fract(pz2 * 2.0 - t * 0.45) - 0.5));
    col += mix(hazeB, wallCol, 0.35) * max(gl1, gl2 * 0.8) * belowF * gFade * 0.20;

    for (int k = 0; k < 8; k++) {
        float fk = float(k);
        float u = (fk + 0.5) / 8.0;                              // 0 low → 1 high
        float sy = mix(-0.40, 0.42, u) * breathe;
        // smooth per-band gain: bass low, mids center, highs top
        float wB = max(0.0, 1.0 - u * 2.2);
        float wM = max(0.0, 1.0 - abs(u - 0.5) * 2.6);
        float wH = max(0.0, (u - 0.55) * 2.2);
        float gain = amt * (wB * bassP + wM * midP + wH * highP);
        float xn = (q.x / portal + 1.0);
        float ph = t * (3.0 + fk * 1.7) + fk * 2.3 + amt * 1.2 * gain;
        // fundamental + plucked-string overtones (2nd/3rd harmonics shimmer in with energy)
        float fund  = sin(xn * PI * (2.0 + fk)) * sin(ph);
        float harm  = 0.34 * sin(xn * PI * (4.0 + 2.0 * fk) + 1.3) * sin(ph * 2.03 + 0.7)
                    + 0.16 * sin(xn * PI * (6.0 + 3.0 * fk) + 0.4) * sin(ph * 2.98 + 2.1);
        float standing = fund + harm * (0.45 + 0.9 * gain);
        float ampl = stringVibe * (0.012 + 0.06 * gain);
        float ycv = sy + standing * ampl;
        float d = abs(q.y - ycv);
        float hue = hueBase + hueSpread * (0.15 + 0.6 * u);
        vec3 sc = pal(hue) * (0.7 + 1.6 * gain + 0.25 * sin(t * 2.0 + fk));
        float w = 0.0022 + 0.002 * gain;
        col += sc * inPortal * (smoothstep(w, w * 0.3, d) * 0.95
                              + glowAmt * exp(-d * 170.0) * 0.7);
        // specular hot-spot RIDING the traveling component of the wave
        float trav = pow(max(0.0, sin(xn * PI * (2.0 + fk) - ph)), 30.0);
        col += vec3(1.0, 0.98, 0.94) * inPortal * trav * exp(-d * 240.0) * (0.30 + 1.5 * gain);
        // mirrored smear on the floor below the portal
        float ryd = abs((2.0 * floorY - q.y) - ycv);
        float smear = exp(-ryd * 26.0) * exp(-(floorY - q.y) * 5.5);
        col += sc * belowF * smear * (0.13 + 0.35 * gain) * smoothstep(0.75, 0.25, abs(q.x));
    }

    // ── dust motes drifting through the tunnel light ────────────────────
    for (int L = 0; L < 3; L++) {
        float fL = float(L);
        float sc2 = 13.0 + 8.0 * fL;
        vec2 dp = q * sc2 + vec2(t * (0.20 + 0.11 * fL), t * (0.06 - 0.05 * fL)) + sway * (2.0 - fL) * 3.0;
        vec2 ci = floor(dp), cf = fract(dp) - 0.5;
        float hr = hash21(ci + fL * 17.0);
        vec2 off = 0.34 * vec2(sin(t * 0.5 + hr * TAU), cos(t * 0.42 + hr * 7.0));
        float dd = length(cf - off);
        float mote = exp(-dd * dd * 240.0) * step(0.58, hr);
        col += mix(vec3(0.85, 0.90, 1.0), pal(hueBase + hueSpread * 0.4), 0.5)
             * mote * (0.13 + 0.07 * fL) * smoothstep(0.95, 0.15, rq)
             * (0.7 + 0.3 * sin(t * 1.3 + hr * TAU));
    }

    // ── finish ──────────────────────────────────────────────────────────
    float gr = hash21(fc + fract(t) * vec2(17.0, 29.0)) - 0.5;
    col += 0.02 * gr;
    col *= brightness * mix(1.0, 0.78 + 0.34 * levelP + 0.26 * clamp(audioBass, 0.0, 1.0)
                                 + 0.14 * clamp(audioMid, 0.0, 1.0) + 0.18 * beatP, amt);
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

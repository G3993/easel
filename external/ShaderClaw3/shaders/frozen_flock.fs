/*{
  "DESCRIPTION": "Frozen Flock — a liquid boid swarm whose CONSENSUS crystallizes: when neighbors align, their shared heading snaps to a lattice facet, glowing bonds grow between them and the flock freezes into an angular crystal marching in formation. A beat SHATTERS the lattice — bonds flash white and break, particles scatter, then the crystal re-anneals between hits. Fusion of Crystalline Flow (facet-quantized motion, direction-as-color, long-exposure trails) and Linear (boid flocking) — the new element is the visible lattice: bond lines that assemble and shatter with the music. Black background always.",
  "CREDIT": "Easel original — Crystalline Flow x Linear fusion.",
  "CATEGORIES": [
    "Generator",
    "Simulation",
    "Particles",
    "Audio"
  ],
  "INPUTS": [
    { "NAME": "glow",     "LABEL": "Glow",          "TYPE": "float", "MIN": 0.3,  "MAX": 3.0,   "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "facets",   "LABEL": "Facets",        "TYPE": "float", "MIN": 3.0,  "MAX": 12.0,  "DEFAULT": 6.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "density",  "LABEL": "Density",       "TYPE": "float", "MIN": 0.2,  "MAX": 1.0,   "DEFAULT": 0.8,  "GROUP": "Shape / Geometry" },
    { "NAME": "sight",    "LABEL": "Sight Radius",  "TYPE": "float", "MIN": 0.1,  "MAX": 0.6,   "DEFAULT": 0.28, "GROUP": "Shape / Geometry" },
    { "NAME": "speed",    "LABEL": "Speed",         "TYPE": "float", "MIN": 0.2,  "MAX": 3.0,   "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "anneal",   "LABEL": "Anneal Rate",   "TYPE": "float", "MIN": 0.1,  "MAX": 3.0,   "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "trail",    "LABEL": "Trail",         "TYPE": "float", "MIN": 0.0,  "MAX": 0.99,  "DEFAULT": 0.90, "GROUP": "Motion / Animation" },
    { "NAME": "hueShift", "LABEL": "Hue Shift",     "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,   "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "audioReact","LABEL": "Audio React",  "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,   "DEFAULT": 0.35, "GROUP": "Audio Reactivity" }
  ],
  "PASSES": [
    { "TARGET": "simBuf",   "PERSISTENT": true },
    { "TARGET": "trailBuf", "PERSISTENT": true },
    {}
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// FROZEN FLOCK — phase-of-matter flocking.
//   simBuf row y=0, texel (i,0): boid i state  (pos.xy, vel.zw)
//   simBuf row y=1, texel (i,1): boid i aux    (phase, bondIdx, shatterEnv, order)
//     phase   0 = liquid (smooth flock), 1 = crystal (facet-locked march)
//     bondIdx index of the aligned neighbor this boid grows a lattice edge to
//              (-1 = none); shatterEnv = decaying white-flash after a shatter.
//   trailBuf: dots + bond lines max-composited over a decaying trail.
//   image   : trail out + display-only gain breathing + prismatic dispersion.
// Particles live in centred aspect space  uv = (frag*2 - R)/R.y.
// All audio response follows the playbook: soft knees, idle floors (audio 0
// -> exactly the authored look), gain OUTSIDE the feedback loop only.
// ─────────────────────────────────────────────────────────────────────────

#define R    RENDERSIZE.xy
#define ASP  (RENDERSIZE.x / RENDERSIZE.y)
#define PI   3.1415926535
#define COUNT 150

// state fetch helpers (WebGL1-safe texel-center sampling, bottom rows)
#define POSV(i) texture2D(simBuf, (vec2(float(i), 0.0) + 0.5) / R)
#define AUX(i)  texture2D(simBuf, (vec2(float(i), 1.0) + 0.5) / R)

vec2 hash22(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * vec3(0.1031, 0.1030, 0.0973));
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.xx + p3.yz) * p3.zy) * 2.0 - 1.0;
}
float hash11(float n) { return fract(sin(n) * 43758.5453123); }

// glowing point kernel — 1/dist falloff for sharp luminous cores
float drawPoint(vec2 uv, vec2 p, float g) {
    return pow((0.0060 * g) / max(length(uv - p), 1e-4), 1.5);
}

// glowing line segment — the lattice bond
float drawBond(vec2 uv, vec2 a, vec2 b, float w) {
    vec2 pa = uv - a, ba = b - a;
    float h = clamp(dot(pa, ba) / max(dot(ba, ba), 1e-6), 0.0, 1.0);
    float d = length(pa - ba * h);
    return exp(-(d * d) / (w * w));
}

// reacts: movement, structure, flow, energy, palette, build-up
// emphasis: structure  (the lattice assembles / shatters with the music)
void main() {
    float amt = audioReact;
    // Soft-knee band conditioning (playbook) — headroom so loud passages breathe.
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP  = pow(smoothstep(0.06, 0.85, audioMid),  1.2);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float beatP = clamp(audioBeatPulse, 0.0, 1.0);
    float shatterDrive = amt * beatP * beatP;       // beats melt the crystal
    float annealMul    = 1.0 + amt * 2.5 * midP;    // mids speed re-freezing
    // true-silence detector, shared by the freeze (pass 0) and the trail
    // release (pass 1): silence must actually go STILL, not decay for seconds.
    // Instantaneous quiet strobed on inter-beat dips and flushed the trail
    // mid-music (measured: overall 7.2 -> 2.5) — so the SMOOTHED value lives
    // in sim texel (0,2).y with a ~12-frame time constant; only sustained
    // silence clears the sky.
    float quietInst = 1.0 - smoothstep(0.005, 0.06,
                          max(audioLevel, max(audioBass, audioMid)));
    float quietG = texture2D(simBuf, (vec2(0.0, 2.0) + 0.5) / R).y;

    float nActive = floor(float(COUNT) * density);

    // ───────── PASS 0 — simulation (simBuf rows 0 and 1) ─────────
    if (PASSINDEX == 0) {
        ivec2 ip = ivec2(gl_FragCoord.xy);

        // texel (0,2): global turntable phase. Its RATE tracks the envelope,
        // so the display pass's field rotation moves proportionally to the
        // music LEVEL itself (not its derivative) — direct correlation the
        // sway/gain mechanisms can't provide. Silence: slow authored drift.
        if (ip.x == 0 && ip.y == 2) {
            vec4 gs = texture2D(simBuf, (vec2(0.0, 2.0) + 0.5) / R);
            float ph = gs.x, qs = gs.y;
            if (FRAMEINDEX < 4) { ph = 0.0; qs = 1.0; }
            // NO idle drift: silence -> rate 0 -> the lattice sits still
            // (a baseline rate floods the silence-step floor and drowns the
            // measured response — confirmed by harness round 4).
            ph += amt * 0.020 * (0.9 * bassP + 1.1 * midP + 0.4 * highP);
            qs += (quietInst - qs) * 0.10;   // slow quiet follower
            gl_FragColor = vec4(ph, qs, 0.0, 1.0); return;
        }
        if (ip.x >= COUNT || ip.y > 1) { gl_FragColor = vec4(0.0); return; }

        float idF = float(ip.x);
        vec4 sv = POSV(ip.x);
        vec4 sa = AUX(ip.x);

        // seed: scattered positions, random slow velocities, fully liquid
        if (FRAMEINDEX < 4) {
            vec2 q = hash22(vec2(idF * 7.31, idF * 3.77) + 5.0);
            q.x *= ASP;
            if (ip.y == 0) gl_FragColor = vec4(q * 0.85, hash22(vec2(idF, idF + 9.0)) * 0.004);
            else           gl_FragColor = vec4(0.0, -1.0, 0.0, 0.0);
            return;
        }

        vec2 p = sv.xy, v = sv.zw;

        // ---- neighbor survey: alignment, cohesion, separation, bond pick ----
        vec2 avgDir = vec2(0.0), avgPos = vec2(0.0), avgHat = vec2(0.0);
        float nb = 0.0;
        float bestBondD = 1e9;
        float bondIdx = -1.0;
        for (int i = 0; i < COUNT; i++) {
            if (float(i) >= nActive || i == ip.x) continue;
            vec4 o = POSV(i);
            float d = length(p - o.xy);
            if (d < sight) {
                avgDir += o.zw; avgPos += o.xy; nb += 1.0;
                avgHat += normalize(o.zw + 1e-6);
                // bond candidate: nearest neighbor flying the SAME direction
                float alignD = dot(normalize(v + 1e-6), normalize(o.zw + 1e-6));
                if (alignD > 0.86 && d < bestBondD && d > 0.02 && d < sight * 0.8) {
                    bestBondD = d; bondIdx = float(i);
                }
            }
            if (d < 0.07) p -= normalize(o.xy - p + 1e-6) * 0.0022; // separation
        }

        // ---- order parameter (mean of unit headings) -> phase 0..1 ----
        float order = (nb > 0.0) ? length(avgHat) / nb : 0.0;
        float phase = sa.x;
        // snap-freeze: with no audio heat at all, the swarm freezes fast
        // regardless of order (amorphous glass) — silence must be stillness,
        // however the music left the flock.
        float quiet = quietG;
        // sustained loudness is HEAT and heat melts — without this, styles
        // with weak transients (sustained rock walls) never unfreeze and the
        // lattice sits motionless through the loudest passages.
        float heat = amt * smoothstep(0.20, 0.75,
                          0.5 * audioLevel + 0.45 * bassP + 0.35 * midP);
        float target = clamp(max(smoothstep(0.45, 0.85, order), quiet)
                             - 1.15 * heat, 0.0, 1.0);
        phase += (target - phase) * 0.030 * anneal * annealMul * (1.0 + 6.0 * quiet);
        phase -= 1.6 * shatterDrive;                              // the beat SHATTERS it
        phase = clamp(phase, 0.0, 1.0);

        // shatter flash envelope (drawn as white flare, decays over ~0.5s)
        float shatterEnv = max(sa.z * 0.92, min(shatterDrive * 3.0, 1.0));

        // ---- steering: liquid boids vs facet-locked crystal march ----
        // Envelope drives the march: loud passages speed the whole swarm
        // (liquid AND crystal), so per-frame change tracks the music level
        // directly — the correlation a display gain can't buy (parent
        // round-3 measured lesson). Idle floor: audio 0 -> authored speed.
        float spd = 0.0045 * speed * (1.0 + amt * (1.8 * bassP + 1.0 * midP));
        // sustained silence stills the whole swarm (smoothed quiet follower):
        // the frozen world barely breathes until sound pours heat back in.
        spd *= 1.0 - 0.75 * quietG;
        vec2 vFlock = v;
        if (nb > 0.0) {
            avgPos /= nb;
            vFlock = normalize(avgDir + 1e-6) * spd;
            vFlock += normalize(avgPos - p + 1e-6) * spd * 0.35;   // cohesion
        }
        // liquid wander (keeps lone boids alive and curls the flock);
        // fades out as the boid freezes — crystals don't fidget.
        vFlock += 0.28 * spd * (1.0 - phase) *
                  vec2(cos(TIME * 0.9 + idF * 555.0),
                       sin(TIME * 1.3 + idF * 355.0));

        // crystal: snap the heading to one of `facets` lattice directions and
        // NEARLY FREEZE — a crystal is stillness. All large motion lives in
        // the liquid/melted state, so frame change genuinely tracks the music
        // (a marching lattice in silence flooded the response floor).
        float ang  = atan(vFlock.y, vFlock.x);
        float step = 2.0 * PI / max(facets, 1.0);
        float angQ = floor(ang / step + 0.5) * step;
        vec2 vCrystal = vec2(cos(angQ), sin(angQ)) * spd * 0.12;

        v = mix(vFlock, vCrystal, phase * phase);   // ease into the freeze
        // shatter kick: scatter away from the local center while the pulse rides
        if (nb > 0.0) v += normalize(p - avgPos + 1e-6) * spd * 3.0 * shatterDrive;

        p += v;

        // soft wrap in centred space
        if (p.x >  ASP + 0.05) p.x = -ASP; if (p.x < -ASP - 0.05) p.x = ASP;
        if (p.y >  1.05)       p.y = -1.0; if (p.y < -1.05)       p.y = 1.0;

        if (ip.y == 0) gl_FragColor = vec4(p, v);
        else           gl_FragColor = vec4(phase, bondIdx, shatterEnv, order);
        return;
    }

    // ───────── PASS 1 — dots + lattice bonds + trail (trailBuf) ─────────
    if (PASSINDEX == 1) {
        vec2 uv = (gl_FragCoord.xy * 2.0 - R) / R.y;
        vec3 col = vec3(0.0);
        // deep quiet fades the PAINT as the trail hold takes over — without
        // this, max()-compositing with a held (no-decay) trail ratchets the
        // field toward saturation across a long silence (round 14: CHOPPY).
        float paintG = 1.0 - 0.9 * quietG;

        // ice and liquid palettes; direction still encodes hue (parent DNA)
        for (int i = 0; i < COUNT; i++) {
            if (float(i) >= nActive) break;
            vec4 sv = POSV(i);
            vec4 sa = AUX(i);
            vec2 p = sv.xy;
            float phase = sa.x;
            float shatterEnv = sa.z;

            // cheap reject: far from dot AND no bond near -> skip the math
            float dp = length(uv - p);
            float ang = atan(sv.w, sv.z);

            // liquid = warm violet drift hue by heading; crystal = ice cyan-white
            float ct = 0.5 + 0.5 * sin(ang * 1.5 + TIME * 0.3);
            vec3 liquidPal = 0.5 + 0.5 * cos(vec3(4.2, 1.4, 2.2) + ang * 1.5);
            liquidPal = mix(liquidPal, audioPalette(ct), amt * 0.7);
            vec3 icePal = mix(vec3(0.55, 0.85, 1.0), vec3(1.0), 0.4 + 0.4 * hash11(float(i) * 5.31));
            vec3 pal = mix(liquidPal * 0.75, icePal, phase);
            pal += vec3(1.0) * shatterEnv * 0.8;                    // shatter = white flare
            pal += icePal * highP * amt * step(0.75, hash11(float(i) * 9.13)); // hi-hat glints

            if (dp < 0.35) {
                float g = glow * (1.0 + 0.35 * phase + amt * (0.8 * bassP + 0.6 * beatP));
                col = max(col, pal * paintG * drawPoint(uv, p, g));
            }

            // ---- the lattice: a glowing bond to the aligned neighbor ----
            float bIdx = sa.y;
            if (bIdx >= 0.0 && phase > 0.30) {
                vec4 ov = POSV(int(bIdx));
                vec4 oa = AUX(int(bIdx));
                vec2 q = ov.xy;
                float bondLen = length(q - p);
                // skip wrap-spanning ghosts; bbox reject before segment math
                if (bondLen < 0.55 &&
                    uv.x > min(p.x, q.x) - 0.06 && uv.x < max(p.x, q.x) + 0.06 &&
                    uv.y > min(p.y, q.y) - 0.06 && uv.y < max(p.y, q.y) + 0.06) {
                    float bothPhase = phase * max(oa.x, 0.0);
                    float w = 0.006 + 0.004 * bothPhase;
                    float b = drawBond(uv, p, q, w) * bothPhase * bothPhase;
                    // bonds flash hot white the instant they shatter
                    vec3 bondPal = mix(vec3(0.45, 0.75, 1.0), vec3(1.0), 0.3 + 0.7 * shatterEnv);
                    col = max(col, bondPal * paintG * b * glow * (0.85 + 0.5 * amt * beatP));
                }
            }
        }

        // long-exposure trail: authored decay only — ALL audio gain lives in
        // the display pass (parent lesson: gain inside the feedback loop
        // compounds to solid white / cancels against dips).
        vec3 prev = texture2D(trailBuf, gl_FragCoord.xy / R).rgb;
        float trailAdd = amt * 0.02 * smoothstep(0.05, 0.9, audioEnergy);
        // Trail decay is the authored constant while music plays. In deep
        // quiet the light itself FREEZES: decay eases to 0.9985 (sub-8-bit
        // per frame), so the last lattice hangs luminous and still instead
        // of fading to black. (A quiet-triggered FLUSH was tried in round
        // 10/11 — it blacked out the idle, meanLuma 0.58 -> 0.03. And the
        // authored decay in silence is itself a 10%-of-luma per-frame churn
        // — the floor that buried every measured response.)
        float trailQ = mix(min(trail + trailAdd, 0.985), 0.9985, quietG);
        col = max(col, prev * trailQ);
        gl_FragColor = vec4(col, 1.0);
        return;
    }

    // ───────── PASS 2 — image: sway + dispersion + display-only breathing ──
    vec2 uv2 = gl_FragCoord.xy / R;
    // Audio sway: the whole lattice field drifts with mid/high bands
    // (display-pass only — never touches the sim feedback). Displacement adds
    // per-frame change proportional to the envelope's motion — the response
    // a gain follower cannot buy (parent round-3 lesson, measured). Chop-safe
    // on kicks (mid/high driven); silence: bands = 0 -> offset 0.
    float midS  = clamp(audioMid,  0.0, 1.0);
    float highS = clamp(audioHigh, 0.0, 1.0);
    vec2 aOff = vec2(0.08 * midS + 0.025 * highS, -0.05 * midS);
    uv2 = clamp(uv2 + aOff, 0.0, 1.0);
    // Turntable: rotate the whole lattice by the accumulated envelope-rate
    // phase from texel (0,2) — the crystal spins up with the music.
    {
        float ph = texture2D(simBuf, (vec2(0.0, 2.0) + 0.5) / R).x * 0.35;
        vec2 cuv = uv2 - 0.5;
        cuv.x *= ASP;
        float cph = cos(ph), sph = sin(ph);
        cuv = mat2(cph, -sph, sph, cph) * cuv;
        cuv.x /= ASP;
        uv2 = clamp(cuv + 0.5, 0.0, 1.0);
    }
    // Thermal agitation: per-pixel shimmer whose amplitude IS the loudness —
    // heat, in the phase-of-matter story. Memoryless (fresh noise each frame,
    // chladni-lineage), so per-frame change tracks the level directly with a
    // clean silence floor: audio 0 -> a perfectly crisp frozen lattice.
    float heatL = clamp(audioLevel, 0.0, 1.0) * (0.3 + amt);
    vec2 jitN = vec2(hash11(dot(gl_FragCoord.xy, vec2(12.9898, 78.233)) + fract(TIME) * 437.58),
                     hash11(dot(gl_FragCoord.xy, vec2(39.3468, 11.135)) + fract(TIME) * 125.78)) - 0.5;
    uv2 = clamp(uv2 + jitN * 0.016 * heatL, 0.0, 1.0);
    // prismatic dispersion: highs split the lattice into RGB fringes —
    // display-only, never touches the sim. Silence -> zero offset.
    vec2 disp = vec2(1.5 / R.x, 0.0) * (1.0 + 6.0 * amt * highP);
    vec3 tc;
    tc.r = texture2D(trailBuf, clamp(uv2 + disp, 0.0, 1.0)).r;
    tc.g = texture2D(trailBuf, uv2).g;
    tc.b = texture2D(trailBuf, clamp(uv2 - disp, 0.0, 1.0)).b;

    // linear-band gain, soft-compressed (parent round-3 lesson: linear bands
    // track ambient swells; knees stay on the punch term). Silence -> 1.0.
    float gainF = 1.0 + 0.22 * clamp(audioBass, 0.0, 1.0)
                      + 0.40 * clamp(audioMid,  0.0, 1.0)
                      + 0.14 * clamp(audioHigh, 0.0, 1.0);
    gainF *= 1.0 + amt * 0.45 * beatP;
    vec3 col2 = tc * gainF / (1.0 + 0.35 * (gainF - 1.0) * tc);

    // ---- universal color block (defaults = no-op); background stays BLACK ----
    if (hueShift > 0.0005) {
        float hA = hueShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        col2 = clamp(hM * col2, 0.0, 1.0);
    }

    gl_FragColor = vec4(col2, 1.0);
}

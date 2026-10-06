/*{
  "DESCRIPTION": "Circuit Bloom Sphere: a raymarched sphere, bare and near-black, etched over time by a neon PCB-circuit-trace pattern. A persistent cellular-automaton growth field spreads and branches across the sphere's surface forever and never resets — real accumulated memory, not a decaying effect. Mids set the branch spawn rate, bass pulses the sphere size and trace brightness, highs send traveling sparks along already-lit traces, and beats flash the whole network white. The source image is sampled as a growth-density mask that steers where the circuit spreads thicker and faster.",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "3D",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "traceGlow",
      "LABEL": "Trace Glow",
      "TYPE": "float",
      "DEFAULT": 2.1,
      "MIN": 0,
      "MAX": 3
    },
    {
      "NAME": "sparkleAmt",
      "LABEL": "Spark Sparkle",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2
    },
    {
      "NAME": "texMaskAmt",
      "LABEL": "Image Growth Mask",
      "TYPE": "float",
      "DEFAULT": 0.75,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "inputImage",
      "LABEL": "Input Image",
      "TYPE": "image"
    },
    {
      "NAME": "sphereScale",
      "LABEL": "Sphere Size",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0.5,
      "MAX": 1.6,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "growthRate",
      "LABEL": "Growth Rate",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "hueA",
      "LABEL": "Trace Hue (settled)",
      "TYPE": "color",
      "DEFAULT": [
        0.1,
        0.85,
        1,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "hueB",
      "LABEL": "Trace Hue (frontier)",
      "TYPE": "color",
      "DEFAULT": [
        1,
        0.2,
        0.75,
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
      "NAME": "gridAmt",
      "LABEL": "Grid Backdrop",
      "TYPE": "float",
      "DEFAULT": 0.3,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "camSpin",
      "LABEL": "Camera Orbit Speed",
      "TYPE": "float",
      "DEFAULT": 0.35,
      "MIN": 0,
      "MAX": 1,
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
      "NAME": "audioReact",
      "LABEL": "Sound Reactivity",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Audio Reactivity"
    }
  ],
  "PASSES": [
    {
      "TARGET": "growthBuf",
      "PERSISTENT": true
    },
    {}
  ]
}*/

// ---------------------------------------------------------------------------
// helpers
// ---------------------------------------------------------------------------
float hash11(float p){
    p = fract(p * 0.1031);
    p *= p + 33.33;
    p *= p + p;
    return fract(p);
}
vec2 hash21(float p){
    vec3 p3 = fract(vec3(p) * vec3(0.1031, 0.1030, 0.0973));
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.xx + p3.yz) * p3.zy);
}
float knee(float x, float lo, float hi){ return smoothstep(lo, hi, x); }

// logical circuit-node lattice overlaid on the sphere's equirect unwrap —
// independent of RENDERSIZE, so trace density stays consistent at any res.
const vec2 GRES = vec2(84.0, 84.0);

// ---------------------------------------------------------------------------
// PASS 0 : cellular-automaton growth field written into growthBuf
//   r = lit (0/1), g = decaying "just grew" frontier flash.
//   Each already-lit cell may, on a slow tick, ignite exactly ONE neighbor
//   (its own hashed direction pick) — this keeps growth sparse and dendritic
//   (real circuit branches) instead of a flood-filling blob. A cell here only
//   ever *checks* whether one of its 8 neighbors chose to grow toward it, so
//   the whole field stays a pure function of neighbor state — no dynamic
//   indexing needed, just eight unrolled fixed-offset checks.
// ---------------------------------------------------------------------------
float tryNeighbor(vec2 fcoord, vec2 offs, float oppIdx, float tick, float spawnChance){
    vec2 nc = fcoord + offs;
    nc.x = mod(nc.x, GRES.x);              // longitude wraps (seam)
    nc.y = clamp(nc.y, 0.0, GRES.y - 1.0); // latitude clamps (poles)
    vec2 nuv = (nc + 0.5) / GRES;
    float nr = texture2D(growthBuf, nuv).r;
    if (nr < 0.5) return 0.0; // neighbor isn't lit — nothing to grow from

    float seed = nc.x * 127.1 + nc.y * 311.7 + tick * 17.917;
    float pick = floor(hash11(seed) * 8.0);       // which of its 8 directions the neighbor "chose" this tick
    float roll = hash11(seed + 53.173);           // independent probability roll
    if (abs(pick - oppIdx) < 0.5 && roll < spawnChance) return 1.0;
    return 0.0;
}

void growPass(){
    vec2 uv = gl_FragCoord.xy / RENDERSIZE;
    vec2 fcoord = floor(uv * GRES);
    vec2 cuv = (fcoord + 0.5) / GRES;

    vec4 self = texture2D(growthBuf, cuv);
    float r = self.r;
    float g = self.g;

    float bass = audioBass * audioReact;
    float mid  = audioMid  * audioReact;

    float bassP = pow(knee(bass, 0.05, 0.85), 1.6);
    float midP  = pow(knee(mid,  0.05, 0.85), 1.2); // mid = branch spawn-rate (structure, law 1)
    float drive = 0.35 + 0.65 * knee(audioEnergy, 0.05, 0.9); // idle floor, never zero

    // texture input = growth-direction/density mask: brighter regions of the
    // source image grow denser, faster circuitry (a real, non-decorative use)
    vec3 imgCol = texture2D(inputImage, cuv).rgb;
    float density = dot(imgCol, vec3(0.299, 0.587, 0.114));
    float densityFactor = mix(1.0, mix(0.5, 1.9, density), clamp(texMaskAmt, 0.0, 1.0));

    float growTickHz = 2.2 + 2.6 * midP;   // mids speed up the growth clock
    float tick = floor(TIME * growTickHz);

    float spawnChance = mix(0.08, 0.9, clamp(growthRate, 0.0, 1.0));
    spawnChance *= densityFactor;
    spawnChance *= drive;
    spawnChance = clamp(spawnChance, 0.02, 0.98);

    float grow = 0.0;
    grow = max(grow, tryNeighbor(fcoord, vec2( 1.0, 0.0), 1.0, tick, spawnChance)); // E  -> opp W
    grow = max(grow, tryNeighbor(fcoord, vec2(-1.0, 0.0), 0.0, tick, spawnChance)); // W  -> opp E
    grow = max(grow, tryNeighbor(fcoord, vec2( 0.0, 1.0), 3.0, tick, spawnChance)); // N  -> opp S
    grow = max(grow, tryNeighbor(fcoord, vec2( 0.0,-1.0), 2.0, tick, spawnChance)); // S  -> opp N
    grow = max(grow, tryNeighbor(fcoord, vec2( 1.0, 1.0), 7.0, tick, spawnChance)); // NE -> opp SW
    grow = max(grow, tryNeighbor(fcoord, vec2(-1.0, 1.0), 6.0, tick, spawnChance)); // NW -> opp SE
    grow = max(grow, tryNeighbor(fcoord, vec2( 1.0,-1.0), 5.0, tick, spawnChance)); // SE -> opp NW
    grow = max(grow, tryNeighbor(fcoord, vec2(-1.0,-1.0), 4.0, tick, spawnChance)); // SW -> opp NE

    float amLit    = step(0.5, r);
    float justGrew = (1.0 - amLit) * step(0.5, grow);

    float newR = max(r, justGrew);          // monotonic — real memory, never resets
    float newG = max(g * 0.90, justGrew);   // frontier flash decays after igniting

    // warm-up only: seed a scatter of small circuit nodes on the first frames,
    // since the persistent buffer starts with no prior state — enough seeds
    // to read as a sparse board from frame one, growth does the rest forever
    if (FRAMEINDEX < 2){
        float seedHit = 0.0;
        for (int i = 0; i < 14; i++){
            vec2 sc = floor(hash21(float(i) * 9.113 + 3.0) * GRES);
            if (distance(fcoord, sc) < 2.2) seedHit = 1.0;
        }
        newR = seedHit;
        newG = seedHit;
    }

    gl_FragColor = vec4(newR, newG, 0.0, 1.0);
}

// ---------------------------------------------------------------------------
// SCREEN PASS : raymarch the sphere SDF, light the circuit traces from growthBuf
// ---------------------------------------------------------------------------
float sdSphere(vec3 p, float r){ return length(p) - r; }

// equirectangular unwrap of a point on the unit sphere -> growth-buffer uv
vec2 sphereUV(vec3 n){
    float lon = atan(n.z, n.x);
    float lat = acos(clamp(n.y, -1.0, 1.0));
    return vec2(lon / 6.2831853 + 0.5, lat / 3.14159265);
}

vec4 sampGrowthCell(vec2 p){
    p.x = fract(p.x);          // wrap the longitude seam
    p.y = clamp(p.y, 0.0, 1.0); // poles don't wrap
    return texture2D(growthBuf, p);
}

void renderPass(){
    vec2 res = RENDERSIZE;
    vec2 ndc = (gl_FragCoord.xy - 0.5 * res) / res.y;

    float bass = audioBass * audioReact;
    float mid  = audioMid  * audioReact;
    float high = audioHigh * audioReact;

    float bassP = pow(knee(bass, 0.05, 0.85), 1.6); // bass = scale pulse + brightness
    float highP = pow(knee(high, 0.10, 0.90), 1.2); // high  = traveling sparks
    float drive = 0.25 + 0.75 * knee(audioEnergy, 0.05, 0.9);
    float beatPulse = audioBeatPulse * audioBeatPulse; // beat = network-wide flash

    // --- orbiting camera, ro/ta/fwd/rgt/upv pattern -------------------------
    float ang = TIME * camSpin * 0.35;
    float orbitR = 2.6;
    vec3 ro = vec3(sin(ang) * orbitR, 0.55 + 0.22 * sin(TIME * 0.15), cos(ang) * orbitR);
    vec3 ta = vec3(0.0);
    vec3 fwd = normalize(ta - ro);
    vec3 rgt = normalize(cross(vec3(0.0, 1.0, 0.0), fwd));
    vec3 upv = cross(fwd, rgt);
    float fov = 1.15;
    vec3 rd = normalize(fwd + (ndc.x * rgt + ndc.y * upv) * fov);

    float radius = clamp(sphereScale, 0.5, 1.6) * (1.0 + 0.16 * bassP);

    // --- backdrop: black with a faint drifting grid motif -------------------
    vec2 guv = ndc * 2.4 + vec2(TIME * 0.012, TIME * 0.006);
    vec2 gcell = fract(guv * 4.0);
    float glx = 1.0 - smoothstep(0.0, 0.06, abs(gcell.x - 0.5));
    float gly = 1.0 - smoothstep(0.0, 0.06, abs(gcell.y - 0.5));
    float gline = clamp(max(glx, gly), 0.0, 1.0);
    vec3 bg = hueA.rgb * 0.05 * gline * clamp(gridAmt, 0.0, 1.0);
    // User background: blend the void/grid backdrop toward the chosen color.
    bg = mix(bg, bgColor.rgb, bgColor.a);

    // --- raymarch the exact sphere SDF --------------------------------------
    float t = 0.0;
    vec3 pos = ro;
    bool hit = false;
    for (int i = 0; i < 48; i++){
        pos = ro + rd * t;
        float d = sdSphere(pos, radius);
        if (d < 0.0015){ hit = true; break; }
        t += d;
        if (t > 12.0) break;
    }

    vec3 col = bg;

    if (hit){
        vec3 n = normalize(pos);
        vec2 suv = sphereUV(n);

        vec4 gcol = sampGrowthCell(suv);
        float r = gcol.r;
        float frontier = gcol.g;

        // boundary detection: a lit cell touching an unlit neighbor is a
        // trace edge; a fully-interior lit cluster stays dark — this is what
        // keeps the surface a bare, unfilled tracery instead of a flat blob.
        // 8-connected so diagonal branch edges register too, plus a wider
        // ring sampled at 2x radius for a soft neon bleed around each trace.
        float st = 1.0 / 84.0;
        float rE = sampGrowthCell(suv + vec2(st, 0.0)).r;
        float rW = sampGrowthCell(suv - vec2(st, 0.0)).r;
        float rN = sampGrowthCell(suv + vec2(0.0, st)).r;
        float rS = sampGrowthCell(suv - vec2(0.0, st)).r;
        float rNE = sampGrowthCell(suv + vec2(st, st)).r;
        float rNW = sampGrowthCell(suv + vec2(-st, st)).r;
        float rSE = sampGrowthCell(suv + vec2(st, -st)).r;
        float rSW = sampGrowthCell(suv + vec2(-st, -st)).r;
        float mismatch = abs(rE - r) + abs(rW - r) + abs(rN - r) + abs(rS - r)
                        + 0.6 * (abs(rNE - r) + abs(rNW - r) + abs(rSE - r) + abs(rSW - r));
        mismatch = clamp(mismatch, 0.0, 1.6);

        float r2E = sampGrowthCell(suv + vec2(2.0 * st, 0.0)).r;
        float r2W = sampGrowthCell(suv - vec2(2.0 * st, 0.0)).r;
        float r2N = sampGrowthCell(suv + vec2(0.0, 2.0 * st)).r;
        float r2S = sampGrowthCell(suv - vec2(0.0, 2.0 * st)).r;
        float haloMismatch = clamp(abs(r2E - r) + abs(r2W - r) + abs(r2N - r) + abs(r2S - r), 0.0, 1.0);

        float traceStrength = r * mismatch + 0.45 * r * haloMismatch; // edge + soft neon halo, never interior fill

        // subtle directional lift so the sphere reads as a 3D form — never a
        // flat color fill, just enough charcoal rim + light bias to hold shape
        vec3 viewDir = normalize(ro - pos);
        float fres = pow(1.0 - clamp(dot(n, viewDir), 0.0, 1.0), 3.0);
        vec3 base = vec3(0.015, 0.02, 0.03) * fres;
        float lightFac = 0.55 + 0.45 * clamp(dot(n, normalize(vec3(0.4, 0.6, 0.35))), 0.0, 1.0);

        vec3 traceColor = mix(hueA.rgb, hueB.rgb, clamp(frontier * 1.3, 0.0, 1.0));
        traceColor *= traceStrength * traceGlow * lightFac;
        traceColor += hueB.rgb * frontier * traceStrength * 1.4 * traceGlow; // hot flash at fresh growth tips

        traceColor += vec3(1.0) * beatPulse * 0.6 * traceStrength;   // beat: flash the whole network
        traceColor *= (1.0 + 0.35 * bassP);                          // bass: overall trace brightness

        // treble: sparse traveling sparks riding the already-lit traces
        vec2 cellId = floor(suv * 84.0);
        float nodePhase = hash11(cellId.x * 127.1 + cellId.y * 311.7 + 9.0);
        float travel = fract(TIME * (0.7 + 1.8 * highP) + nodePhase * 3.0);
        float spark = smoothstep(0.965, 1.0, sin(travel * 6.2831853) * 0.5 + 0.5);
        float sparkAmt = spark * traceStrength * (0.15 * drive + 0.85 * highP) * clamp(sparkleAmt, 0.0, 2.0);
        traceColor += vec3(1.3, 1.5, 1.8) * sparkAmt;

        col = base + traceColor;
    }

    col = col / (1.0 + col);
    col = pow(max(col, 0.0), vec3(1.0 / 2.2));

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

    gl_FragColor = vec4(col, 1.0);
}

// ---------------------------------------------------------------------------
void main(){
    if (PASSINDEX == 0){
        growPass();
    } else {
        renderPass();
    }
}

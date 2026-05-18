/*{
    "DESCRIPTION": "Melt — viscous downward drip transition. The source layer breaks into pixel droplets that drip in the chosen direction with gravity acceleration, leaving elongated trails behind each particle. Cells melt on a staggered delay so the layer wears away unevenly; the destination is revealed wherever a droplet has fallen past or fully decayed.",
    "CATEGORIES": ["Transition"],
    "CREDIT": "Easel transitions v1",
    "INPUTS": [
        { "NAME": "from",            "TYPE": "image" },
        { "NAME": "to",              "TYPE": "image" },
        { "NAME": "progress",        "TYPE": "float", "DEFAULT": 0.0,    "MIN": 0.0,  "MAX": 1.0,  "LABEL": "Progress" },
        { "NAME": "particleGrid",    "TYPE": "float", "DEFAULT": 64.0,   "MIN": 16.0, "MAX": 200.0,"LABEL": "Particle Grid" },
        { "NAME": "direction",       "TYPE": "float", "DEFAULT": 4.7124, "MIN": 0.0,  "MAX": 6.2831,"LABEL": "Drip Direction (rad)" },
        { "NAME": "scatter",         "TYPE": "float", "DEFAULT": 0.45,   "MIN": 0.05, "MAX": 1.5,  "LABEL": "Drip Length" },
        { "NAME": "spread",          "TYPE": "float", "DEFAULT": 0.04,   "MIN": 0.0,  "MAX": 0.4,  "LABEL": "Wobble" },
        { "NAME": "gravity",         "TYPE": "float", "DEFAULT": -0.20,  "MIN": -0.6, "MAX": 0.6,  "LABEL": "Gravity" },
        { "NAME": "lifetime",        "TYPE": "float", "DEFAULT": 0.65,   "MIN": 0.05, "MAX": 1.0,  "LABEL": "Particle Life" },
        { "NAME": "dissolveSpread",  "TYPE": "float", "DEFAULT": 0.55,   "MIN": 0.0,  "MAX": 1.0,  "LABEL": "Stagger" },
        { "NAME": "dropletWidth",    "TYPE": "float", "DEFAULT": 1.0,    "MIN": 0.3,  "MAX": 2.5,  "LABEL": "Droplet Width" },
        { "NAME": "tailFade",        "TYPE": "float", "DEFAULT": 0.45,   "MIN": 0.0,  "MAX": 1.0,  "LABEL": "Tail Fade" },
        { "NAME": "edgeFlash",       "TYPE": "float", "DEFAULT": 0.30,   "MIN": 0.0,  "MAX": 1.5,  "LABEL": "Hot Edge" },
        { "NAME": "edgeColor",       "TYPE": "color", "DEFAULT": [1.0, 0.85, 0.55, 1.0], "LABEL": "Edge Color" }
    ]
}*/

float hash21(vec2 p){
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 78.233);
    return fract(p.x * p.y);
}

void main(){
    vec2  res    = RENDERSIZE;
    float aspect = res.x / res.y;
    vec2  uv     = isf_FragNormCoord;

    float gx = max(8.0, particleGrid);
    vec2  cellSize = vec2(1.0 / gx, aspect / gx);

    vec2  baseDir = vec2(cos(direction), sin(direction));

    // Search upstream along the drip direction: any cell whose trail has
    // dragged itself across this fragment must lie in -baseDir from us.
    vec2  stepUV = -baseDir * cellSize.y * 0.75;

    vec3  bestRGB = vec3(0.0);
    float bestA   = 0.0;

    vec4 dest = IMG_NORM_PIXEL(to, uv);

    const int MAX_STEPS = 22;
    for (int k = 0; k < MAX_STEPS; k++){
        vec2 sUV = uv + stepUV * float(k);
        if (sUV.x < 0.0 || sUV.x > 1.0 || sUV.y < 0.0 || sUV.y > 1.0) break;

        vec2 cId = floor(sUV / cellSize);

        float h1 = hash21(cId * 1.13 + 7.7);
        float h2 = hash21(cId * 1.91 + 17.3);
        float h3 = hash21(cId * 2.71 + 31.5);

        float delay = h1 * dissolveSpread;
        float pL    = clamp((progress - delay) / max(0.001, lifetime), 0.0, 1.0);
        if (pL <= 0.0)  continue;   // particle hasn't started yet
        if (pL >= 1.0)  continue;   // particle has fully decayed

        vec2 randDir = (vec2(h2, h3) - 0.5) * 2.0;
        vec2 vel = baseDir * scatter + randDir * spread;
        vel.y += gravity * pL;

        vec2 cCenter = (cId + 0.5) * cellSize;
        vec2 endPos  = cCenter + vel * pL;

        // Distance from uv to the trail segment cCenter→endPos
        vec2  segDir = endPos - cCenter;
        float segLen = length(segDir);
        if (segLen < 1e-5) continue;

        vec2  segN  = segDir / segLen;
        vec2  toUV  = uv - cCenter;
        float along = clamp(dot(toUV, segN), 0.0, segLen);
        vec2  closestPt = cCenter + segN * along;
        float perpD = length((uv - closestPt) * vec2(aspect, 1.0));

        float r = length(cellSize) * 0.5 * dropletWidth;
        float a = smoothstep(r * 1.4, r * 0.4, perpD);

        // Tapered trail: head bright, tail fades to tailFade
        float headFrac = along / segLen;
        float taper = mix(tailFade, 1.0, headFrac);

        // Age fade
        float ageFade = pow(1.0 - pL, 1.4);

        float pa = a * taper * ageFade;
        if (pa > bestA) {
            bestA = pa;
            bestRGB = IMG_NORM_PIXEL(from, cCenter).rgb;

            // Hot rim flash at the head of the drip while the cell is freshly melting
            float hot = edgeFlash
                      * smoothstep(0.0,  0.18, pL)
                      * smoothstep(0.45, 0.18, pL)
                      * smoothstep(0.55, 0.95, headFrac);
            bestRGB = mix(bestRGB, edgeColor.rgb, hot);
        }
    }

    // Home cell: still solid source if it hasn't started melting yet
    vec2 homeId = floor(uv / cellSize);
    float homeDelay = hash21(homeId * 1.13 + 7.7) * dissolveSpread;
    if (progress < homeDelay) {
        vec2 hc = (homeId + 0.5) * cellSize;
        gl_FragColor = IMG_NORM_PIXEL(from, hc);
        return;
    }

    if (bestA > 0.0) {
        gl_FragColor = mix(dest, vec4(bestRGB, 1.0), bestA);
    } else {
        gl_FragColor = dest;
    }
}

/*{
    "DESCRIPTION": "Disintegrate — pixelated particles fly off the source layer in a chosen direction with randomised spread and per-particle stagger, revealing the destination layer behind. Uses inverse fixed-point iteration so each fragment locates the cell whose particle currently covers it.",
    "CATEGORIES": ["Transition"],
    "CREDIT": "Easel transitions v1",
    "INPUTS": [
        { "NAME": "from",           "TYPE": "image" },
        { "NAME": "to",             "TYPE": "image" },
        { "NAME": "progress",       "TYPE": "float", "DEFAULT": 0.0,  "MIN": 0.0,  "MAX": 1.0,  "LABEL": "Progress" },
        { "NAME": "particleGrid",   "TYPE": "float", "DEFAULT": 80.0, "MIN": 16.0, "MAX": 240.0,"LABEL": "Particle Grid" },
        { "NAME": "direction",      "TYPE": "float", "DEFAULT": 1.5708,"MIN": 0.0, "MAX": 6.2831,"LABEL": "Direction (rad)" },
        { "NAME": "scatter",        "TYPE": "float", "DEFAULT": 0.22, "MIN": 0.0,  "MAX": 1.2,  "LABEL": "Scatter" },
        { "NAME": "spread",         "TYPE": "float", "DEFAULT": 0.18, "MIN": 0.0,  "MAX": 0.8,  "LABEL": "Random Spread" },
        { "NAME": "gravity",        "TYPE": "float", "DEFAULT": -0.05,"MIN": -0.5, "MAX": 0.5,  "LABEL": "Gravity" },
        { "NAME": "lifetime",       "TYPE": "float", "DEFAULT": 0.55, "MIN": 0.05, "MAX": 1.0,  "LABEL": "Particle Life" },
        { "NAME": "dissolveSpread", "TYPE": "float", "DEFAULT": 0.55, "MIN": 0.0,  "MAX": 1.0,  "LABEL": "Stagger" },
        { "NAME": "edgeFlash",      "TYPE": "float", "DEFAULT": 0.35, "MIN": 0.0,  "MAX": 1.5,  "LABEL": "Edge Flash" },
        { "NAME": "edgeColor",      "TYPE": "color", "DEFAULT": [1.0, 0.86, 0.55, 1.0], "LABEL": "Edge Color" }
    ]
}*/

// Stable per-cell hash
float hash21(vec2 p){
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 78.233);
    return fract(p.x * p.y);
}

void main(){
    vec2 res    = RENDERSIZE;
    float aspect = res.x / res.y;
    vec2 uv     = isf_FragNormCoord;

    // Cells are kept square in screen pixels: cellW=1/gx in x, cellH=aspect/gx in y
    float gx = max(8.0, particleGrid);
    vec2 cellSize = vec2(1.0 / gx, aspect / gx);

    // Inverse fixed-point: which cell's particle currently covers this fragment?
    // Each iteration takes the implied cell's velocity and walks the source UV back.
    vec2  sampleUV = uv;
    vec2  cellId   = vec2(0.0);
    float pLocal   = 0.0;
    vec2  vel      = vec2(0.0);

    vec2  baseDir  = vec2(cos(direction), sin(direction));

    for (int k = 0; k < 4; k++){
        cellId = floor(sampleUV / cellSize);
        float h1 = hash21(cellId * 1.13 + 7.7);
        float h2 = hash21(cellId * 1.91 + 17.3);
        float h3 = hash21(cellId * 2.71 + 31.5);

        float delay = h1 * dissolveSpread;
        pLocal = clamp((progress - delay) / max(0.001, lifetime), 0.0, 1.0);

        vec2 randDir = (vec2(h2, h3) - 0.5) * 2.0;
        vel = baseDir * scatter + randDir * spread;
        vel.y += gravity * pLocal;

        // Walk back along velocity to find where this fragment came from
        sampleUV = uv - vel * pLocal;
    }

    // Final answers from converged iteration
    vec2 cellCenter = (cellId + 0.5) * cellSize;

    vec4 dest = IMG_NORM_PIXEL(to, uv);

    // Particle has fully decayed → show destination
    if (pLocal >= 1.0) {
        gl_FragColor = dest;
        return;
    }

    // Sample-origin out of bounds → no source material to draw, show destination
    if (cellCenter.x < 0.0 || cellCenter.x > 1.0 ||
        cellCenter.y < 0.0 || cellCenter.y > 1.0) {
        gl_FragColor = dest;
        return;
    }

    // Pixelated source colour at the originating cell
    vec4 src = IMG_NORM_PIXEL(from, cellCenter);

    // Particle alpha falls off cubically over its life — soft head, hard tail
    float fade = pow(1.0 - pLocal, 1.6);

    // Edge flash: bright rim on each particle right as it begins to disintegrate
    // Driven by the local cell's progress, not the global progress, so the
    // flash sweeps across the layer following the dissolveSpread stagger.
    float flash = edgeFlash * smoothstep(0.0, 0.18, pLocal) * smoothstep(0.45, 0.18, pLocal);
    src.rgb = mix(src.rgb, edgeColor.rgb, flash);

    // Composite particle over destination with its current fade
    gl_FragColor = mix(dest, src, fade);
}

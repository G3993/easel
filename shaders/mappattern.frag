// MAPPING-workspace calibration patterns, drawn live at the zone's real
// resolution. They used to be square 1024² textures stretched onto a 16:9
// output, which turned the corner rings into ovals; here everything is
// laid out in pixels, so circles stay round and grid cells stay square.
// uAnimate drifts each pattern slowly so the mapping can be checked with
// motion. Index order MUST match kPatternNames in WarpEditor.cpp:
// 0 Grid, 1 Checkerboard, 2 Crosshair, 3 Color Bars, 4 Rainbow,
// 5 Solid White, 6 Colors.
#version 330 core
in  vec2 vTexCoord;
out vec4 FragColor;

uniform int   uPattern;
uniform float uTime;
uniform bool  uAnimate;
uniform bool  uMarkers = true;   // corner-pin rings (off for the mask-edit grid)
uniform vec2  uRes;       // zone size in pixels

const vec3 kOn  = vec3(1.0);
const vec3 kOff = vec3(12.0, 12.0, 16.0) / 255.0;

vec3 hue(float h) {
    return clamp(abs(fract(h + vec3(0.0, 2.0, 1.0) / 3.0) * 6.0 - 3.0) - 1.0, 0.0, 1.0);
}
float hash(vec2 p) { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }

// 1 on a line of half-width w (pixels) through every multiple of `cell`.
float gridLine(float x, float cell, float w) {
    float d = abs(mod(x + cell * 0.5, cell) - cell * 0.5);
    return 1.0 - smoothstep(w - 0.5, w + 0.5, d);
}

void main() {
    vec2 uv = vTexCoord;
    vec2 p  = uv * uRes;                    // pixels, origin bottom-left
    float t = uAnimate ? uTime : 0.0;
    float s = min(uRes.x, uRes.y);          // the short side sets every size
    vec3 col = kOff;

    if (uPattern == 0) {                    // Grid: square cells, border, slow drift + scan line
        float cell = s / 12.0;
        vec2 q = p - vec2(t * cell * 0.25, 0.0);
        float g = max(gridLine(q.x, cell, 1.0), gridLine(q.y, cell, 1.0));
        float edge = step(min(min(p.x, p.y), min(uRes.x - p.x, uRes.y - p.y)), 2.0);
        col = mix(kOff, kOn, max(g, edge));
        if (uAnimate) {
            float scan = fract(t * 0.15) * uRes.y;
            col += vec3(0.25, 0.9, 1.0) * (1.0 - smoothstep(0.0, 3.0, abs(p.y - scan)));
        }
    } else if (uPattern == 1) {             // Checkerboard: square cells, scrolling diagonally
        float cell = s / 10.0;
        vec2 q = floor((p + t * cell * vec2(0.3, 0.2)) / cell);
        col = mod(q.x + q.y, 2.0) < 0.5 ? kOn : kOff;
    } else if (uPattern == 2) {             // Crosshair + diagonals + border + round rings
        vec2 c = uRes * 0.5;
        vec2 d = p - c;
        float on = 0.0;
        on = max(on, 1.0 - smoothstep(1.5, 2.5, abs(d.x)));
        on = max(on, 1.0 - smoothstep(1.5, 2.5, abs(d.y)));
        // diagonals corner to corner (true diagonals of the output rect)
        vec2 n1 = normalize(vec2(uRes.y, -uRes.x)), n2 = normalize(vec2(uRes.y, uRes.x));
        on = max(on, 1.0 - smoothstep(1.5, 2.5, abs(dot(d, n1))));
        on = max(on, 1.0 - smoothstep(1.5, 2.5, abs(dot(d, n2))));
        on = max(on, step(min(min(p.x, p.y), min(uRes.x - p.x, uRes.y - p.y)), 2.0));
        float r = length(d), ring = s / 8.0;
        float rr = r - (uAnimate ? fract(t * 0.25) * ring : 0.0);
        on = max(on, (1.0 - smoothstep(1.0, 2.0, abs(mod(rr + ring * 0.5, ring) - ring * 0.5))) * step(r, s * 0.5));
        col = mix(kOff, kOn, on);
        if (uAnimate) {                     // radar sweep
            float a = atan(d.y, d.x), sw = mod(a - t * 1.2, 6.28318);
            col += vec3(0.2, 1.0, 0.6) * 0.35 * exp(-sw * 3.0) * step(r, s * 0.5);
        }
    } else if (uPattern == 3) {             // Color bars over a grey wedge, sliding when animated
        vec3 bars[7] = vec3[7](vec3(235,235,235), vec3(235,235,16), vec3(16,235,235), vec3(16,235,16),
                               vec3(235,16,235), vec3(235,16,16), vec3(16,16,235));
        float yy = 1.0 - uv.y;              // 0 at the top
        float x = fract(uv.x + t * 0.05);
        if (yy < 2.0 / 3.0)       col = bars[min(6, int(x * 7.0))] / 255.0;
        else if (yy < 0.75)       col = bars[6 - min(6, int(x * 7.0))] / 510.0;
        else                      col = vec3(float(min(10, int(uv.x * 11.0))) / 10.0);
    } else if (uPattern == 4) {             // Rainbow: hue sweep, white at the top, black at the bottom
        vec3 h = hue(uv.x + t * 0.08);
        float yy = 1.0 - uv.y;
        float lift = yy < 0.5 ? 1.0 - yy * 2.0 : 0.0;
        float dark = yy > 0.5 ? (yy - 0.5) * 2.0 : 0.0;
        col = (h + (1.0 - h) * lift) * (1.0 - dark);
    } else if (uPattern == 5) {             // Solid White (stays still: it's the brightness reference)
        col = kOn;
    } else {                                // Colors: a field of saturated tiles that keep changing hue
        float cell = s / 6.0;
        vec2 id = floor(p / cell), f = fract(p / cell);
        float h0 = hash(id);
        float h = h0 + t * (0.05 + 0.1 * hash(id + 7.0));
        col = hue(h) * (0.85 + 0.15 * hash(id + 3.0));
        // soft round blob of a second colour inside each tile
        float b = length(f - 0.5);
        col = mix(col, hue(h + 0.5), (1.0 - smoothstep(0.28, 0.32, b)) * step(0.5, hash(id + 11.0)));
    }

    // Corner markers on EVERY pattern: a magenta ring + dot with a black halo,
    // a fixed pixel distance from each source corner. The warp maps the
    // source corners onto the pinned corners, so these show the corner-pin
    // handles ON the projector. Sized in pixels, so they stay round.
    if (uMarkers) {
        float m = s * 0.045, R = s * 0.026, w = s * 0.004;
        vec2 mc = vec2(p.x < uRes.x * 0.5 ? m : uRes.x - m, p.y < uRes.y * 0.5 ? m : uRes.y - m);
        float r = length(p - mc);
        if (uAnimate) R *= 1.0 + 0.12 * sin(t * 3.0);
        float ring = 1.0 - smoothstep(w - 0.75, w + 0.75, abs(r - R));
        float halo = 1.0 - smoothstep(w * 2.0 - 0.75, w * 2.0 + 0.75, abs(r - R));
        float dot_ = 1.0 - smoothstep(w * 1.2 - 0.75, w * 1.2 + 0.75, r);
        col = mix(col, vec3(0.0), halo);
        col = mix(col, vec3(1.0, 40.0 / 255.0, 220.0 / 255.0), max(ring, dot_));
    }

    FragColor = vec4(col, 1.0);
}

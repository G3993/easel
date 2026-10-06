/*{
  "DESCRIPTION":"Droste Spiral — an Escher logarithmic-spiral that zooms into itself forever, an infinite self-similar tunnel spiraling toward the viewer via log-polar tiling. Optionally feed your own image into the spiral.",
  "CREDIT":"ShaderClaw3",
  "CATEGORIES":["Generator","Fractal","Tunnel","Audio Reactive"],
  "INPUTS":[
    {"NAME":"zoomSpeed","LABEL":"Zoom Speed","TYPE":"float","DEFAULT":0.12,"MIN":0.0,"MAX":1.0},
    {"NAME":"scaleStep","LABEL":"Scale Step","TYPE":"float","DEFAULT":2.718,"MIN":2.0,"MAX":4.0},
    {"NAME":"twistAmount","LABEL":"Twist","TYPE":"float","DEFAULT":1.0,"MIN":0.0,"MAX":2.0},
    {"NAME":"lineGlow","LABEL":"Line Glow","TYPE":"float","DEFAULT":1.0,"MIN":0.0,"MAX":3.0},
    {"NAME":"paletteShift","LABEL":"Palette Shift","TYPE":"float","DEFAULT":0.0,"MIN":0.0,"MAX":1.0},
    {"NAME":"inputImage","LABEL":"Your Image","TYPE":"image"},
    {"NAME":"texMix","LABEL":"Image Amount","TYPE":"float","DEFAULT":0.0,"MIN":0.0,"MAX":1.0},
    {"NAME":"audioReact","LABEL":"Sound Reactivity","TYPE":"float","DEFAULT":1.0,"MIN":0.0,"MAX":2.0}
  ]
}*/

// curated cosine palette (house style)
vec3 pal(float t){ return 0.5 + 0.5*cos(6.28318*(t + vec3(0.0,0.33,0.67))); }

const float TAU = 6.28318530718;

void main() {
    // live audio: runtime auto-provides audioBass/audioMid/audioHigh (mic FFT). K<=1.5 caps kept below.
    float bass   = audioBass * audioReact;
    float mid    = audioMid  * audioReact;
    float treble = audioHigh * audioReact;

    vec2 uv = (gl_FragCoord.xy - 0.5*RENDERSIZE) / min(RENDERSIZE.x, RENDERSIZE.y);

    // polar coordinates
    float r = length(uv);
    float a = atan(uv.y, uv.x);

    // guard the singularity at center
    r = max(r, 1e-4);

    // log-polar space
    vec2 lp = vec2(log(r), a);

    // Droste log-spiral remap: rotate the log-polar plane so concentric rings spiral
    float ratio = log(scaleStep);                 // radial period in log space (zoom factor per ring)
    float twist = atan(ratio / TAU) * twistAmount; // droste spiral angle, tunable
    float ct = cos(twist);
    // standard droste shear/rotation, normalized by cos(twist)
    lp = mat2(ct, -sin(twist), sin(twist), ct) * lp / max(ct, 1e-3);

    // continuous infinite zoom toward the camera, alive even at audio=0, bass adds drive (K<=1.5)
    float zoom = zoomSpeed * (1.0 + bass*0.7);
    lp.x += TIME * zoom;

    // self-similar tiling so the tunnel never ends
    vec2 cell = vec2(ratio, TAU);
    vec2 q = mod(lp, cell);

    // --- procedural Escher tunnel pattern from the tiled cell ---
    // local normalized coords within a single repeating tile (0..1 across ratio x TAU)
    vec2 t = q / cell;
    vec2 g = t - 0.5;             // centered

    float lines = 0.0;

    // concentric rings (radial bands marching toward viewer)
    float ring = abs(fract(lp.x / ratio * 3.0) - 0.5);
    lines += smoothstep(0.06, 0.0, ring) * 1.0;

    // radial spokes (angular spokes that twist with the spiral)
    float spokeN = 6.0;
    float spoke = abs(fract(lp.y / TAU * spokeN) - 0.5);
    lines += smoothstep(0.05, 0.0, spoke) * 0.8;

    // nested smaller copies for fractal texture — a couple of recursive scales
    float frac = 0.0;
    for (int i = 0; i < 3; i++) {
        float fi = float(i);
        float s = 2.0 + fi*2.0;
        vec2 fq = fract(t * s) - 0.5;
        float d = length(fq);
        // small glowing nodes nested inside each tile
        frac += smoothstep(0.32, 0.0, d) * (0.5 / (1.0 + fi));
        if (fi > 2.0) break;
    }
    lines += frac * 0.6;

    // a soft tile-edge web binding the structure
    vec2 web = abs(g);
    float edge = smoothstep(0.5, 0.46, max(web.x, web.y));
    lines += (1.0 - edge) * 0.25;

    // --- color ---
    // palette indexed by log-polar position + slow time + paletteShift + a little mid
    float pidx = lp.x*0.18 + lp.y*0.10 + TIME*0.03 + paletteShift + mid*0.25;
    vec3 base = pal(pidx);

    // treble brightens the line glow (K<=0.6)
    float glow = lineGlow * (1.0 + treble*0.6);

    // deep near-black between lines, neon lines
    vec3 col = base * lines * glow;

    // --- user image fed INTO the tunnel ---
    // sample the image with the wrapped/tiled log-polar coords (t = lp after mod(),
    // normalized to 0..1 within the ratio x TAU tile) so the picture spirals
    // infinitely toward the camera, then mix with the procedural ring/spoke pattern.
    if (texMix > 0.0) {
        vec2 imgUV = t;
        vec3 img = texture2D(inputImage, imgUV).rgb;
        // let the image glow through the same line/structure mask so it reads as the tunnel
        vec3 imgLit = img * (0.5 + 0.8*lines) * glow;
        col = mix(col, imgLit, texMix);
    }

    // subtle depth vignette — dark core sells infinity, brighter outward
    float depthGlow = smoothstep(0.0, 0.5, r);
    col *= 0.35 + 0.65*depthGlow;
    col += base * 0.04 * (1.0 - smoothstep(0.0, 0.25, r)); // faint core ember

    // gentle overall pulse
    col *= 0.9 + 0.1*sin(TIME*0.4);

    // tonemap + gamma (house style)
    col = col / (1.0 + col);
    col = pow(col, vec3(0.4545));

    gl_FragColor = vec4(col, 1.0);
}

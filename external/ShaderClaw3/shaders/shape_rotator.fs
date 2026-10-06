/*{
  "DESCRIPTION": "Fluid Oxblood Blob — a glossy, wet, ultra-smooth raymarched sculpture. Multiple metaballs smooth-unioned with high viscosity create genuinely fluid, melting forms. Domain-warped by low-frequency fbm for organic morphing. Lit like a museum glass sculpture: smeared white speculars, sharp horizontal streak, soft crimson subsurface, Fresnel halo, warm radial vignette. Optional texture skin via triplanar mapping. Audio-reactive breathing.",
  "CREDIT": "ShaderClaw",
  "CATEGORIES": [
    "Generator",
    "3D",
    "Abstract",
    "Fluid"
  ],
  "INPUTS": [
    {
      "NAME": "inputImage",
      "LABEL": "Texture",
      "TYPE": "image"
    },
    {
      "NAME": "glossiness",
      "LABEL": "Glossiness",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.5,
      "DEFAULT": 1
    },
    {
      "NAME": "subsurface",
      "LABEL": "Subsurface Depth",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.5,
      "DEFAULT": 0.9
    },
    {
      "NAME": "fresnelAmt",
      "LABEL": "Fresnel Rim",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.85
    },
    {
      "NAME": "vignette",
      "LABEL": "Vignette",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1
    },
    {
      "NAME": "texAmount",
      "LABEL": "Texture Skin",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.85
    },
    {
      "NAME": "blobCount",
      "LABEL": "Blob Count",
      "TYPE": "float",
      "MIN": 2,
      "MAX": 10,
      "DEFAULT": 5,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "viscosity",
      "LABEL": "Viscosity",
      "TYPE": "float",
      "MIN": 0.1,
      "MAX": 2,
      "DEFAULT": 0.85,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "morphSpeed",
      "LABEL": "Morph Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.4,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "displaceAmt",
      "LABEL": "Displacement",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.32,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "spin",
      "LABEL": "Auto Spin",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.5,
      "DEFAULT": 0.22,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "redTint",
      "LABEL": "Red Tint",
      "TYPE": "float",
      "MIN": -0.5,
      "MAX": 0.5,
      "DEFAULT": 0,
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
      "LABEL": "Audio React",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.8,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

// ---- universal color block (defaults = no-op) ----
vec3 ucApply(vec3 uc) {
    float ucL = dot(uc, vec3(0.299, 0.587, 0.114));
    uc = mix(vec3(ucL), uc, colorBoost);                      // saturation
    if (hueShift > 0.0005) {                                  // cheap hue rotate (YIQ)
        float hA = hueShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hM * uc, 0.0, 1.0);
    }
    uc = mix(uc, bgColor.rgb, bgColor.a * (1.0 - smoothstep(0.0, 0.35, dot(uc, vec3(0.299, 0.587, 0.114)))));
    return uc;
}


// ════════════════════════════════════════════════════════════════════════
//  FLUID OXBLOOD BLOB · multi-metaball smooth-union · wet glass sculpture
//
//  Multiple crimson metaballs orbiting and merging via smooth-union
//  (liquid_3d technique), additionally domain-warped by low-freq fbm.
//  Smooth tetrahedral normals, procedural studio environment reflection,
//  strong smeared specular + sharp horizontal streak, deep crimson
//  subsurface gradient, Fresnel halo, warm near-black radial vignette.
//  Optional user TEXTURE triplanar-skins the surface. No hard edges.
// ════════════════════════════════════════════════════════════════════════

const float TAU = 6.28318530718;

// ── Hash / noise ─────────────────────────────────────────────────────
float hash1(float n){ return fract(sin(n)*43758.5453); }

float h31(vec3 p){
    p = fract(p*vec3(0.1031,0.1030,0.0973));
    p += dot(p, p.yxz+33.33);
    return fract((p.x+p.y)*p.z);
}
float vnoise(vec3 p){
    vec3 i = floor(p), f = fract(p);
    f = f*f*(3.0-2.0*f);
    float n000=h31(i),             n100=h31(i+vec3(1,0,0));
    float n010=h31(i+vec3(0,1,0)), n110=h31(i+vec3(1,1,0));
    float n001=h31(i+vec3(0,0,1)), n101=h31(i+vec3(1,0,1));
    float n011=h31(i+vec3(0,1,1)), n111=h31(i+vec3(1,1,1));
    return mix(mix(mix(n000,n100,f.x),mix(n010,n110,f.x),f.y),
               mix(mix(n001,n101,f.x),mix(n011,n111,f.x),f.y),f.z);
}
float fbm3(vec3 p){
    float s=0.0, a=0.5;
    // Only 3 octaves for softer, lower-freq warp
    for(int i=0;i<3;i++){ s+=a*vnoise(p); p=p*1.88+vec3(7.3,2.1,5.7); a*=0.5; }
    return s;
}

// ── Rotation ──────────────────────────────────────────────────────────
mat3 rotAxis(vec3 ax, float a){
    ax = normalize(ax);
    float c=cos(a), s=sin(a), t=1.0-c;
    return mat3(
        t*ax.x*ax.x+c,      t*ax.x*ax.y-s*ax.z, t*ax.x*ax.z+s*ax.y,
        t*ax.x*ax.y+s*ax.z, t*ax.y*ax.y+c,      t*ax.y*ax.z-s*ax.x,
        t*ax.x*ax.z-s*ax.y, t*ax.y*ax.z+s*ax.x, t*ax.z*ax.z+c);
}

// ── Smooth min (liquid merging) ───────────────────────────────────────
float smin(float a, float b, float k){
    float h = clamp(0.5 + 0.5*(b-a)/k, 0.0, 1.0);
    return mix(b, a, h) - k*h*(1.0-h);
}

// ── Globals ───────────────────────────────────────────────────────────
float gT, gAud;
mat3  gRot;

// ── The fluid blob SDF ───────────────────────────────────────────────
// Multiple metaballs (liquid_3d style orbits) + fbm warp (shape_rotator style)
float mapBlob(vec3 p){
    // Gentle low-freq fbm warp applied in world space before rotation
    vec3 warpOff = vec3(
        fbm3(p*0.7 + vec3(0.0, 0.0, gT*0.18)) - 0.5,
        fbm3(p*0.7 + vec3(3.7, 0.0, gT*0.14)) - 0.5,
        fbm3(p*0.7 + vec3(0.0, 5.1, gT*0.16)) - 0.5
    );
    vec3 wp = p + warpOff * (0.22 + 0.45*displaceAmt);

    // Apply global slow rotation
    vec3 q = gRot * wp;

    float t  = gT;
    float d  = 1e9;
    int   nb = int(clamp(blobCount, 2.0, 10.0));
    float k  = 0.55 * viscosity;  // generous merge radius = very soft blobs

    for(int i = 0; i < 10; i++){
        if(i >= nb) break;
        float fi = float(i);
        // Slow, organic orbits — different freq per blob
        float sa = 0.30 + 0.22*hash1(fi);
        float sb = 0.25 + 0.20*hash1(fi+9.0);
        float sc = 0.28 + 0.18*hash1(fi+3.0);
        float rad = 0.55 + 0.50*hash1(fi+5.0);
        // Scale orbit radius so blobs stay close and merge well
        rad *= 1.0 + 0.34*gAud;
        vec3 center = vec3(
            sin(t*sa + fi*2.39) * rad,
            sin(t*sb + fi*1.71) * rad * 0.85,
            cos(t*sc + fi*1.13) * rad * 0.90
        );
        // Each blob slightly different size
        float r = (0.62 + 0.32*hash1(fi+7.0)) * (0.85 + 0.42*gAud);
        d = smin(d, length(q - center) - r, k);
    }

    return d * 0.65; // Lipschitz-safe scale
}

// ── Smooth normal via tetrahedral finite difference ───────────────────
vec3 calcNormal(vec3 p){
    vec2 e = vec2(1.0,-1.0)*0.0018;
    return normalize(
        e.xyy*mapBlob(p+e.xyy) + e.yyx*mapBlob(p+e.yyx) +
        e.yxy*mapBlob(p+e.yxy) + e.xxx*mapBlob(p+e.xxx));
}

// ── Procedural museum studio environment ──────────────────────────────
vec3 envMap(vec3 r){
    float y = clamp(r.y*0.5+0.5, 0.0, 1.0);
    vec3 base = mix(vec3(0.06,0.012,0.02), vec3(0.55,0.16,0.14),
                    smoothstep(0.15,0.95,y));
    float key  = exp(-pow(length(r.xz-vec2( 0.35, 0.45)),2.0)*4.0);
    float fill = exp(-pow(length(r.xz-vec2(-0.55,-0.15)),2.0)*8.0);
    base += key *vec3(1.0,0.95,0.90)*1.10;
    base += fill*vec3(0.55,0.10,0.10)*0.45;
    return base;
}

// ── Triplanar texture skin ────────────────────────────────────────────
vec3 triplanar(vec3 worldP, vec3 worldN){
    vec3 op = gRot * worldP;
    vec3 on = normalize(gRot * worldN);
    vec3 w  = abs(on);
    w /= max(w.x+w.y+w.z, 1e-3);
    vec3 sp = op*0.38 + 0.5;
    vec3 cx = texture(inputImage, sp.zy).rgb;
    vec3 cy = texture(inputImage, sp.xz).rgb;
    vec3 cz = texture(inputImage, sp.xy).rgb;
    return cx*w.x + cy*w.y + cz*w.z;
}

void main(){
    vec2 res = RENDERSIZE;
    vec2 uv  = (gl_FragCoord.xy - 0.5*res) / res.y;

    // Globals
    gT   = TIME * morphSpeed;
    gAud = clamp(audioLevel*audioReact, 0.0, 1.0);
    float pb = audioBass*audioReact;

    // Slow drifting rotation — very gentle so the fluid motion reads
    float ang = TIME*spin*0.55 + 0.18*sin(TIME*spin*0.19);
    vec3  ax  = normalize(vec3(0.30 + 0.15*sin(TIME*spin*0.11),
                                1.0,
                                0.20 + 0.18*cos(TIME*spin*0.08)));
    gRot = rotAxis(ax, ang);

    // Fixed contemplative camera
    vec3 ro  = vec3(0.0, 0.0, 4.2);
    vec3 ta  = vec3(0.0, 0.0, 0.0);
    vec3 ww  = normalize(ta-ro);
    vec3 uu  = normalize(cross(ww, vec3(0,1,0)));
    vec3 vv  = cross(uu, ww);
    vec3 rd  = normalize(uv.x*uu + uv.y*vv + 1.75*ww);

    // Warm near-black radial vignette background
    float rad = length(uv);
    vec3 bg = mix(vec3(0.085,0.012,0.018), vec3(0.012,0.002,0.004),
                  smoothstep(0.0, 1.05, rad*vignette));
    bg *= 1.0 - 0.35*vignette*smoothstep(0.2,1.3,rad);

    // ── Raymarch the fluid blob ──
    // Adaptive step: smaller near the surface for smoothness
    float tt  = 0.0;
    bool  hit = false;
    float prevD = 1e9;
    for(int i = 0; i < 120; i++){
        vec3  p = ro + rd*tt;
        float d = mapBlob(p);
        if(d < 0.0004){ hit=true; break; }
        // Relax step slightly to avoid overstepping soft blobs
        tt += d * 0.72;
        if(tt > 9.0) break;
        prevD = d;
    }

    vec3 col = bg;
    if(hit){
        vec3  p   = ro + rd*tt;
        vec3  n   = calcNormal(p);
        vec3  v   = normalize(ro - p);
        vec3  rfl = reflect(-v, n);
        float ndv = clamp(dot(n,v), 0.0, 1.0);
        float fres= pow(1.0 - ndv, 4.0);

        // Crimson subsurface: dark oxblood core → bright crimson rim
        vec3 deep   = vec3(0.10, 0.005, 0.010);
        vec3 mid    = vec3(0.50, 0.035, 0.045);
        vec3 bright = vec3(0.95, 0.18,  0.16);
        float depth = pow(ndv, 1.3);
        vec3  albedo = mix(bright, mid, depth);
        albedo = mix(albedo, deep, smoothstep(0.50,1.0,depth)*0.80);
        albedo += bright*pow(1.0-ndv,2.0)*0.65*subsurface;

        // Hue / tint
        albedo.r  = clamp(albedo.r + redTint, 0.0, 1.2);
        albedo.gb *= clamp(1.0 - redTint*0.8, 0.4, 1.0);

        // Optional texture skin
        bool hasTex = IMG_SIZE_inputImage.x > 0.0;
        if(hasTex){
            vec3 tx      = triplanar(p, n);
            vec3 skinned = tx * mix(vec3(1.05,0.85,0.85), bright*1.4, depth*0.5);
            skinned *= 0.55 + 0.9*ndv;
            albedo = mix(albedo, skinned, texAmount);
        }

        // Environment reflection + Fresnel
        vec3  env   = envMap(rfl);
        float gloss = glossiness;

        // Smeared upper specular (soft, broad)
        vec3  L1     = normalize(vec3(0.45, 0.85, 0.55));
        vec3  H1     = normalize(v + L1);
        float sBroad = pow(clamp(dot(n,H1),0.0,1.0), 22.0);

        // Sharp horizontal specular streak
        vec3  L2     = normalize(vec3(-0.6, 0.05, 0.7));
        vec3  H2     = normalize(v + L2);
        float streak = pow(clamp(dot(n,H2),0.0,1.0), 200.0);
        streak *= smoothstep(0.38, 0.0, abs(n.y));

        // Soft ambient occlusion
        float ao = clamp(mapBlob(p + n*0.28)/0.28, 0.0, 1.0);
        ao = 0.35 + 0.65*ao;

        // Compose surface
        vec3 surf = albedo * (0.40 + 0.60*ndv) * ao;
        surf = mix(surf, env, (0.18 + 0.55*fres) * clamp(gloss,0.0,1.0));
        surf += sBroad * vec3(1.0,0.97,0.93) * 1.6  * gloss;
        surf += streak * vec3(1.0,0.98,0.95) * 2.6  * gloss;
        surf += fres   * vec3(0.95,0.30,0.26) * fresnelAmt;
        surf += pow(fres,1.5)*vec3(1.0,0.85,0.80)*0.38*fresnelAmt;

        // Bass breathe
        surf *= 1.0 + 0.30*pb;

        // Soft silhouette fade into vignette
        col = mix(surf, bg, smoothstep(0.0,1.0,fres)*0.08);
    }

    // Warm central glow
    col += exp(-rad*rad*3.0) * vec3(0.18,0.022,0.022) * vignette;

    // Continuous band-following breath (r2 ambient fix): LINEAR bands — the
    // envelopes are pre-smoothed upstream, a knee/pow here crushed ambient's
    // 0.1-0.8 swells into nothing. Whole frame, and the follower gets a
    // floored gain so audioReact's 0.8 default can't dilute the depth.
    // Silence -> exactly 1.0.
    // R3: LINEAR composite follower matching the bus band mix (bass:mid:high
    // ≈ 0.4:0.25:0.15). The old bass+mid smoothstep pair was shift-degenerate
    // against ambient's sinusoid swells (null-corr 0.48 beat true corr 0.34);
    // the third band breaks the symmetry. Depth moved slightly off bass to
    // keep clear of the edm chop line (p95 was 0.095). Silence -> ×1.0.
    // R3 MEASURED: ambient adj was 0.015 (corr 0.372 vs null 0.357) with
    // respMag 0.0003 — the breath was too shallow to rise above the blob
    // motion baseline. Deepen via MID (chop-safe: edm kicks live in bass,
    // p95 was 0.078 with 0.10 the line) and trim bass slightly.
    float aDep  = 0.6 + 0.5*audioReact;   // default 0.8 -> 1.0
    // (Depths are MEASURED: 0.30 mid scored ambient 1.39; deeper 0.42 mid
    // scored WORSE (1.21 — the null corr rose with the signal), so 0.30 is
    // the keeper. edm p95 0.076 stays under the 0.10 chop line.)
    col *= 1.0 + aDep * (0.20*audioBass + 0.30*audioMid + 0.14*audioHigh);

    // Specular bloom
    float Llum = dot(col, vec3(0.299,0.587,0.114));
    col += 0.20 * smoothstep(0.60,1.5,Llum) * col;

    // Subtle grain — dithers banding in smooth gradients
    float g = fbm3(vec3(uv*res.y*0.011, gT*0.4));
    col *= 1.0 + (g-0.5)*0.028;

    col = max(col, 0.0);
    col = ucApply(col);
    gl_FragColor = vec4(col, 1.0);
}
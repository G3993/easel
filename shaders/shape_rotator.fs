/*{
  "DESCRIPTION": "Shape Rotator — a single glossy, wet oxblood blob raymarched in real 3D. A sphere SDF domain-warped by low-frequency fbm slowly morphs and rotates, lit like a museum glass sculpture: smeared white speculars, a sharp horizontal specular streak, soft crimson subsurface depth (dark core, bright rim) and a gentle Fresnel halo, floating in a near-black warm-red radial vignette. Supply a TEXTURE and it skins the sculpture (triplanar) while keeping the wet gloss; unbound falls back to procedural oxblood. No hard edges, no pixels. Returns LINEAR HDR — host applies ACES.",
  "CREDIT": "ShaderClaw",
  "CATEGORIES": ["Generator", "3D", "Abstract"],
  "INPUTS": [
    { "NAME": "inputImage",   "LABEL": "Texture",          "TYPE": "image" },
    { "NAME": "morphSpeed",   "LABEL": "Morph Speed",      "TYPE": "float", "MIN": 0.0,  "MAX": 2.0, "DEFAULT": 0.5 },
    { "NAME": "displaceAmt",  "LABEL": "Displacement",     "TYPE": "float", "MIN": 0.0,  "MAX": 1.0, "DEFAULT": 0.42 },
    { "NAME": "glossiness",   "LABEL": "Glossiness",       "TYPE": "float", "MIN": 0.0,  "MAX": 1.5, "DEFAULT": 1.0 },
    { "NAME": "redTint",      "LABEL": "Red Tint",         "TYPE": "float", "MIN": -0.5, "MAX": 0.5, "DEFAULT": 0.0 },
    { "NAME": "subsurface",   "LABEL": "Subsurface Depth", "TYPE": "float", "MIN": 0.0,  "MAX": 1.5, "DEFAULT": 0.9 },
    { "NAME": "fresnelAmt",   "LABEL": "Fresnel Rim",      "TYPE": "float", "MIN": 0.0,  "MAX": 2.0, "DEFAULT": 0.85 },
    { "NAME": "vignette",     "LABEL": "Vignette",         "TYPE": "float", "MIN": 0.0,  "MAX": 2.0, "DEFAULT": 1.0 },
    { "NAME": "texAmount",    "LABEL": "Texture Skin",     "TYPE": "float", "MIN": 0.0,  "MAX": 1.0, "DEFAULT": 0.85 },
    { "NAME": "audioReact",   "LABEL": "Audio React",      "TYPE": "float", "MIN": 0.0,  "MAX": 2.0, "DEFAULT": 0.25 }
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//  SHAPE ROTATOR · raymarched glossy oxblood blob · wet glass sculpture
//
//  One sphere SDF, domain-warped by slow low-frequency fbm and rotated
//  about a drifting axis. Smooth tetrahedral normals, a procedural
//  studio environment reflection, strong smeared specular + a sharp
//  horizontal streak, deep crimson subsurface gradient (dark core →
//  bright rim), Fresnel halo, and a warm near-black radial vignette.
//  Optional user TEXTURE triplanar-skins the surface as albedo while
//  keeping the wet gloss. Smooth only — no hard edges, no pixels.
// ════════════════════════════════════════════════════════════════════════

const float TAU = 6.28318530718;

float h31(vec3 p){
    p = fract(p*vec3(0.1031,0.1030,0.0973));
    p += dot(p, p.yxz+33.33);
    return fract((p.x+p.y)*p.z);
}
float vnoise(vec3 p){
    vec3 i = floor(p), f = fract(p);
    f = f*f*(3.0-2.0*f);
    float n000=h31(i),                 n100=h31(i+vec3(1,0,0));
    float n010=h31(i+vec3(0,1,0)),     n110=h31(i+vec3(1,1,0));
    float n001=h31(i+vec3(0,0,1)),     n101=h31(i+vec3(1,0,1));
    float n011=h31(i+vec3(0,1,1)),     n111=h31(i+vec3(1,1,1));
    return mix(mix(mix(n000,n100,f.x), mix(n010,n110,f.x), f.y),
               mix(mix(n001,n101,f.x), mix(n011,n111,f.x), f.y), f.z);
}
float fbm3(vec3 p){
    float s=0.0, a=0.55;
    for(int i=0;i<4;i++){ s+=a*vnoise(p); p=p*1.92+7.0; a*=0.5; }
    return s;
}

mat3 rotAxis(vec3 ax, float a){
    ax = normalize(ax);
    float c=cos(a), s=sin(a), t=1.0-c;
    return mat3(
        t*ax.x*ax.x+c,      t*ax.x*ax.y-s*ax.z, t*ax.x*ax.z+s*ax.y,
        t*ax.x*ax.y+s*ax.z, t*ax.y*ax.y+c,      t*ax.y*ax.z-s*ax.x,
        t*ax.x*ax.z-s*ax.y, t*ax.y*ax.z+s*ax.x, t*ax.z*ax.z+c);
}

float gT, gAud;
mat3  gRot, gRotInv;

// blob distance field: rotated sphere displaced by low-frequency fbm
float mapBlob(vec3 p){
    vec3 q = gRot * p;                                  // slow rotation
    float r = 1.15 + 0.06*gAud;
    // low-frequency lumpy morph (two octaves, time-evolving)
    float n  = fbm3(q*1.05 + vec3(0.0, 0.0, gT*0.35));
    n += 0.5*fbm3(q*2.1 - vec3(gT*0.22, 4.0, 0.0));
    float disp = (n - 0.75) * (0.55 + 1.0*displaceAmt);
    return (length(q) - r - disp) * 0.6;                // Lipschitz-safe
}

vec3 calcNormal(vec3 p){
    vec2 e = vec2(1.0,-1.0)*0.0014;
    return normalize(
        e.xyy*mapBlob(p+e.xyy) + e.yyx*mapBlob(p+e.yyx) +
        e.yxy*mapBlob(p+e.yxy) + e.xxx*mapBlob(p+e.xxx));
}

// procedural museum studio environment for reflections
vec3 envMap(vec3 r){
    float y = clamp(r.y*0.5+0.5, 0.0, 1.0);
    vec3 base = mix(vec3(0.06,0.012,0.02), vec3(0.55,0.16,0.14),
                    smoothstep(0.15,0.95,y));
    float key  = exp(-pow(length(r.xz-vec2( 0.35, 0.45)),2.0)*4.0);
    float fill = exp(-pow(length(r.xz-vec2(-0.55,-0.15)),2.0)*8.0);
    base += key *vec3(1.0,0.95,0.90)*1.10;              // big soft key
    base += fill*vec3(0.55,0.10,0.10)*0.45;             // dim warm fill
    return base;
}

// triplanar sample of the user texture using object-space position p.
// Blend weights come from the object-space surface normal (rotate the
// world normal back into object space so the skin stays glued as the
// blob rotates).
vec3 triplanar(vec3 worldP, vec3 worldN){
    vec3 op = gRot * worldP;
    vec3 on = normalize(gRot * worldN);
    vec3 w = abs(on);
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

    gT   = TIME * morphSpeed;
    gAud = clamp(audioLevel*audioReact, 0.0, 1.0);
    float pb = audioBass*audioReact;

    // slow drifting rotation axis + angle
    float ang = gT*0.6 + 0.25*sin(gT*0.27);
    vec3  ax  = normalize(vec3(0.35, 1.0, 0.22 + 0.25*sin(gT*0.15)));
    gRot    = rotAxis(ax,  ang);
    gRotInv = rotAxis(ax, -ang);

    // fixed contemplative camera
    vec3 ro = vec3(0.0, 0.0, 3.7);
    vec3 ta = vec3(0.0, 0.0, 0.0);
    vec3 ww = normalize(ta-ro);
    vec3 uuv= normalize(cross(ww, vec3(0,1,0)));
    vec3 vv = cross(uuv, ww);
    vec3 rd = normalize(uv.x*uuv + uv.y*vv + 1.7*ww);

    // ── warm near-black radial vignette background ──
    float rad = length(uv);
    vec3 bg = mix(vec3(0.085,0.012,0.018), vec3(0.012,0.002,0.004),
                  smoothstep(0.0, 1.05, rad*vignette));
    bg *= 1.0 - 0.35*vignette*smoothstep(0.2,1.3,rad);

    // ── raymarch the blob ──
    float tt = 0.0; bool hit=false;
    for (int i=0;i<96;i++){
        vec3 p = ro + rd*tt;
        float d = mapBlob(p);
        if (d < 0.0006){ hit=true; break; }
        tt += d*0.9;
        if (tt > 8.0) break;
    }

    vec3 col = bg;
    if (hit){
        vec3 p = ro + rd*tt;
        vec3 n = calcNormal(p);
        vec3 v = normalize(ro - p);
        vec3 rfl = reflect(-v, n);
        float ndv  = clamp(dot(n,v), 0.0, 1.0);
        float fres = pow(1.0 - ndv, 4.0);

        // crimson subsurface: dark oxblood core → bright crimson rim
        vec3 deep   = vec3(0.12, 0.006, 0.012);
        vec3 mid    = vec3(0.55, 0.04,  0.05);
        vec3 bright = vec3(0.95, 0.18,  0.16);
        float depth = pow(ndv, 1.4);
        vec3 albedo = mix(bright, mid, depth);
        albedo = mix(albedo, deep, smoothstep(0.55,1.0,depth)*0.85);
        albedo += bright*pow(1.0-ndv,2.2)*0.6*subsurface;          // rim glow
        // hue/tint trim
        albedo.r = clamp(albedo.r + redTint, 0.0, 1.2);
        albedo.gb *= clamp(1.0 - redTint*0.8, 0.4, 1.0);

        // optional user texture skin (triplanar, object space).
        // Convention: an unbound ISF image input reports IMG_SIZE == 0
        // (see soph_orb.fs: `bool hasVideo = IMG_SIZE_inputTex.x > 0.0;`).
        bool hasTex = IMG_SIZE_inputImage.x > 0.0;
        if (hasTex){
            vec3 tx = triplanar(p, n);
            // keep the crimson wet character: tint the skin by depth
            vec3 skinned = tx * mix(vec3(1.05,0.85,0.85), bright*1.4, depth*0.5);
            skinned *= 0.55 + 0.9*ndv;
            albedo = mix(albedo, skinned, texAmount);
        }

        // environment reflection + Fresnel weighting
        vec3 env = envMap(rfl);
        float gloss = glossiness;

        // smeared upper specular (soft, broad)
        vec3  L1 = normalize(vec3(0.45, 0.85, 0.55));
        vec3  H1 = normalize(v + L1);
        float sBroad = pow(clamp(dot(n,H1),0.0,1.0), 28.0);
        // sharp horizontal specular streak across the middle
        vec3  L2 = normalize(vec3(-0.6, 0.05, 0.7));
        vec3  H2 = normalize(v + L2);
        float streak = pow(clamp(dot(n,H2),0.0,1.0), 240.0);
        streak *= smoothstep(0.34, 0.0, abs(n.y));      // band it horizontally

        // soft ambient occlusion from the field
        float ao = clamp(mapBlob(p + n*0.22)/0.22, 0.0, 1.0);
        ao = 0.4 + 0.6*ao;

        vec3 surf = albedo * (0.45 + 0.55*ndv) * ao;
        surf = mix(surf, env, (0.16 + 0.55*fres) * clamp(gloss,0.0,1.0));
        surf += sBroad * vec3(1.0,0.97,0.93) * 1.5  * gloss;
        surf += streak * vec3(1.0,0.98,0.95) * 2.4  * gloss;
        surf += fres   * vec3(0.95,0.30,0.26) * fresnelAmt;        // crimson Fresnel halo
        surf += pow(fres,1.5)*vec3(1.0,0.85,0.80)*0.35*fresnelAmt; // cool white edge kiss

        surf *= 1.0 + 0.12*pb;                          // gentle bass breathe
        col = surf;

        // soft contact shadow / fade into the vignette at silhouette
        col = mix(col, bg, smoothstep(0.0,1.0,fres)*0.10);
    }

    // warm central glow lifting the sculpture off the black
    col += exp(-rad*rad*3.0) * vec3(0.16,0.02,0.02) * vignette;

    // bloom on the speculars
    float Llum = dot(col, vec3(0.299,0.587,0.114));
    col += 0.18 * smoothstep(0.65,1.6,Llum) * col;

    // continuous fine grain so gradients never band into a grid
    float g = fbm3(vec3(uv*res.y*0.013, gT*0.5));
    col *= 1.0 + (g-0.5)*0.035;

    col = max(col, 0.0);
    gl_FragColor = vec4(col, 1.0);
}

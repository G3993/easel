/*{
  "DESCRIPTION": "Gemstone Fluid — a ring of polished tumbled gems frames a glowing aperture filled with a stable-fluids liquid-metal simulation. Each gem is pseudo-3D with facet normals, Fresnel, specular and warm refraction; the fluid interior is a semi-Lagrangian velocity field with divergence correction and metallic shading. Fully audio-reactive (bass/mid/high) with rich color palettes, motion kit, and fidelity post-processing.",
  "CREDIT": "ShaderClaw + Schuetze/Vimont fluid core",
  "CATEGORIES": [
    "Generator",
    "VFX",
    "Fluid",
    "A-List"
  ],
  "INPUTS": [
    {
      "NAME": "energyA",
      "LABEL": "Cluster A Energy",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "BIND": "player[1].energy"
    },
    {
      "NAME": "energyB",
      "LABEL": "Cluster B Energy",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "BIND": "player[2].energy"
    },
    {
      "NAME": "energyC",
      "LABEL": "Cluster C Energy",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "BIND": "player[3].energy"
    },
    {
      "NAME": "activeA",
      "LABEL": "Cluster A Active",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "BIND": "player[1].active"
    },
    {
      "NAME": "activeB",
      "LABEL": "Cluster B Active",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "BIND": "player[2].active"
    },
    {
      "NAME": "dyeRate",
      "LABEL": "Dye Rate",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.65
    },
    {
      "NAME": "fluidMix",
      "LABEL": "Fluid Mix",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.72
    },
    {
      "NAME": "exposure",
      "LABEL": "Exposure",
      "TYPE": "float",
      "MIN": 0.3,
      "MAX": 2.5,
      "DEFAULT": 1.15
    },
    {
      "NAME": "fidBloom",
      "LABEL": "Glow",
      "TYPE": "float",
      "DEFAULT": 0.85,
      "MIN": 0,
      "MAX": 1.5
    },
    {
      "NAME": "fidDither",
      "LABEL": "Dither",
      "TYPE": "float",
      "DEFAULT": 0.85,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "fidGamma",
      "LABEL": "Gamma",
      "TYPE": "float",
      "DEFAULT": 0.6,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "fidEdgeGlow",
      "LABEL": "Edge Glow",
      "TYPE": "float",
      "DEFAULT": 0.55,
      "MIN": 0,
      "MAX": 2
    },
    {
      "NAME": "fidVignette",
      "LABEL": "Vignette",
      "TYPE": "float",
      "DEFAULT": 0.45,
      "MIN": 0,
      "MAX": 1.5
    },
    {
      "NAME": "fidGrain",
      "LABEL": "Grain",
      "TYPE": "float",
      "DEFAULT": 0.3,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "inputTex",
      "LABEL": "Texture",
      "TYPE": "image"
    },
    {
      "NAME": "texMix",
      "LABEL": "Texture Mix",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0
    },
    {
      "NAME": "gemCount",
      "LABEL": "Gem Count",
      "TYPE": "long",
      "DEFAULT": 14,
      "VALUES": [
        8,
        10,
        12,
        14,
        16,
        18,
        20
      ],
      "LABELS": [
        "8",
        "10",
        "12",
        "14",
        "16",
        "18",
        "20"
      ],
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "gemSize",
      "LABEL": "Gem Size",
      "TYPE": "float",
      "MIN": 0.05,
      "MAX": 0.2,
      "DEFAULT": 0.105,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "ringRadius",
      "LABEL": "Ring Radius",
      "TYPE": "float",
      "MIN": 0.2,
      "MAX": 0.55,
      "DEFAULT": 0.36,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "facetSharp",
      "LABEL": "Facet Sharp",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.5,
      "DEFAULT": 0.8,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "motionSpeed",
      "LABEL": "Motion",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.6,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "stirSpeed",
      "LABEL": "Stir Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.5,
      "DEFAULT": 0.35,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "fluidForce",
      "LABEL": "Fluid Force",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 6,
      "DEFAULT": 3.2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "dissipation",
      "LABEL": "Dissipation",
      "TYPE": "float",
      "MIN": 0.95,
      "MAX": 1,
      "DEFAULT": 0.997,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "flowSpeed",
      "LABEL": "Flow Speed",
      "TYPE": "float",
      "MIN": 0.2,
      "MAX": 4,
      "DEFAULT": 1.8,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "motionDrift",
      "LABEL": "Drift Speed",
      "TYPE": "float",
      "DEFAULT": 1.3,
      "MIN": 0,
      "MAX": 3,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "motionJitter",
      "LABEL": "Jitter",
      "TYPE": "float",
      "DEFAULT": 0.2,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "motionFlicker",
      "LABEL": "Flicker",
      "TYPE": "float",
      "DEFAULT": 0.12,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "motionSway",
      "LABEL": "Sway",
      "TYPE": "float",
      "DEFAULT": 0.45,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "motionChaos",
      "LABEL": "Chaos",
      "TYPE": "float",
      "DEFAULT": 0.45,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "paletteMode",
      "LABEL": "Palette",
      "TYPE": "long",
      "DEFAULT": 0,
      "VALUES": [
        0,
        1,
        2,
        3
      ],
      "LABELS": [
        "Opal",
        "Citrine",
        "Amethyst",
        "Aurora"
      ],
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
      "NAME": "msg",
      "LABEL": "Caption",
      "TYPE": "text",
      "DEFAULT": "ode to song",
      "MAX_LENGTH": 48,
      "BIND": "cue.latest",
      "GROUP": "Text"
    },
    {
      "NAME": "captionScale",
      "LABEL": "Caption Size",
      "TYPE": "float",
      "MIN": 0.4,
      "MAX": 1.8,
      "DEFAULT": 1,
      "GROUP": "Text"
    },
    {
      "NAME": "paperColor",
      "LABEL": "Paper",
      "TYPE": "color",
      "DEFAULT": [
        0.06,
        0.07,
        0.12,
        1
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "bassDrive",
      "LABEL": "Bass → Aperture",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "BIND": "audio.bass",
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "midDrive",
      "LABEL": "Mid → Sparkle",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.8,
      "BIND": "audio.mid",
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "highDrive",
      "LABEL": "High → Detail",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.6,
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "audioDepth",
      "LABEL": "Audio Depth",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.5,
      "DEFAULT": 0.8,
      "GROUP": "Audio Reactivity"
    }
  ],
  "PASSES": [
    {
      "TARGET": "velBuf",
      "PERSISTENT": true
    },
    {
      "TARGET": "divBuf"
    },
    {
      "TARGET": "prsBuf",
      "PERSISTENT": true
    },
    {}
  ]
}*/

// ═══════════════════════════════════════════════════════════════════
//  SHARED UTILITIES
// ═══════════════════════════════════════════════════════════════════

#define MAX_GEMS  20
#define MAX_WALK  48
#define SPACE_CH  26
#define TAU       6.28318530718

// ── Hash / noise ────────────────────────────────────────────────────
float h11(float n){ return fract(sin(n * 127.1) * 43758.5453); }
vec2  h21(float n){ return vec2(h11(n), h11(n + 17.31)); }
float vnoise(vec2 p){
    vec2 i = floor(p), f = fract(p);
    f = f*f*(3.0-2.0*f);
    float a = h11(dot(i,             vec2(1.0,157.0)));
    float b = h11(dot(i+vec2(1.0,0.0), vec2(1.0,157.0)));
    float c = h11(dot(i+vec2(0.0,1.0), vec2(1.0,157.0)));
    float d = h11(dot(i+vec2(1.0,1.0), vec2(1.0,157.0)));
    return mix(mix(a,b,f.x), mix(c,d,f.x), f.y);
}
float fbm2(vec2 p){
    float v=0.0, a=0.5;
    for(int i=0;i<4;i++){ v+=a*vnoise(p); p=p*2.07+vec2(11.3,5.7); a*=0.5; }
    return v;
}
float smin(float a, float b, float k){
    float h = clamp(0.5+0.5*(b-a)/k,0.0,1.0);
    return mix(b,a,h)-k*h*(1.0-h);
}

// ── Palette ─────────────────────────────────────────────────────────
vec3 spectrum(float t){ return 0.5+0.5*cos(TAU*(t+vec3(0.00,0.33,0.67))); }
vec3 gemPalette(int mode, float t){
    if(mode==1) return mix(vec3(0.99,0.78,0.30),vec3(0.95,0.45,0.32),0.5+0.5*sin(TAU*t));
    if(mode==2) return mix(vec3(0.55,0.32,0.85),vec3(0.42,0.68,0.95),0.5+0.5*sin(TAU*t+1.7));
    if(mode==3) return 0.5+0.5*cos(TAU*(t+vec3(0.15,0.55,0.95)));
    vec3 rainbow=spectrum(t);
    return mix(rainbow,vec3(0.95,0.92,0.88),0.25);
}
vec3 hsv2rgb_s(vec3 c){
    vec3 rgb=clamp(abs(mod(c.x*6.0+vec3(0.0,4.0,2.0),6.0)-3.0)-1.0,0.0,1.0);
    rgb=rgb*rgb*(3.0-2.0*rgb);
    return c.z*mix(vec3(1.0),rgb,c.y);
}

// ── Gem SDF ─────────────────────────────────────────────────────────
float gemSDF(vec2 q, float rad, float wobble, float seed){
    float ang=atan(q.y,q.x);
    float warp=0.18*sin(ang*3.0+seed*1.3)+0.10*sin(ang*5.0+seed*2.7);
    float r=rad*(1.0+warp*wobble);
    return length(q)-r;
}

// ── Motion Kit ──────────────────────────────────────────────────────
float mkHash(vec2 p){ p=fract(p*vec2(127.1,311.7)); p+=dot(p,p+34.5); return fract(p.x*p.y); }
vec2 mkMotion(vec2 q, float t){
    float ch=0.4+motionChaos;
    vec2 sway=vec2(sin(t*0.32+q.y*1.8),cos(t*0.27+q.x*1.6))*motionSway*0.09;
    vec2 drift=vec2(sin(t*0.12*ch),cos(t*0.10*ch))*motionDrift*0.05;
    float f=1.0+1.2*motionChaos;
    vec2 jit=vec2(
        sin(t*0.70*f+q.y*3.1)*0.6+sin(t*0.45*f+q.x*2.3+1.7)*0.4,
        cos(t*0.60*f+q.x*2.7)*0.6+cos(t*0.50*f+q.y*2.9+4.2)*0.4
    )*motionJitter*0.05;
    return sway+drift+jit;
}
float mkFlicker(vec2 q, float t){
    float n=0.5+0.5*sin(t*2.0+q.x*7.0+q.y*5.0);
    float scan=0.5+0.5*sin(q.y*180.0+t*3.0);
    return 1.0-motionFlicker*(0.5*n+0.30*scan);
}

// ── Fidelity Kit ────────────────────────────────────────────────────
vec3 fidApply(vec3 col, vec2 frag){
    float l=dot(col,vec3(0.299,0.587,0.114));
    vec2 lg=vec2(dFdx(l),dFdy(l));
    float edge=clamp(length(lg)*7.0,0.0,1.0);
    col+=col*edge*fidEdgeGlow*1.50;
    float headroom=smoothstep(0.28,0.95,l);
    col+=col*headroom*fidBloom*1.80;
    vec2 uvN=frag/RENDERSIZE-0.5;
    float vig=1.0-dot(uvN,uvN)*1.80*fidVignette;
    col*=clamp(vig,0.0,1.0);
    float g=fract(sin(dot(frag+vec2(TIME*73.0,TIME*41.0),vec2(12.9898,78.233)))*43758.5453);
    col+=(g-0.5)*fidGrain*0.045;
    col=col/(1.0+col*0.18);
    float n=fract(sin(dot(frag,vec2(12.9898,78.233)))*43758.5453);
    col+=(n-0.5)*(1.0/255.0)*fidDither;
    col=mix(col,pow(max(col,0.0),vec3(1.0/2.2)),fidGamma);
    return col;
}

// ── Fluid helpers ────────────────────────────────────────────────────
vec2 stirPos(){
    float t=TIME*stirSpeed;
    return vec2(0.5)+0.30*vec2(cos(t),sin(t*1.43));
}
vec2 stirVelF(){
    float t=TIME*stirSpeed;
    return vec2(-sin(t),1.43*cos(t*1.43));
}
float curlAt(vec2 uv, vec2 texel){
    float vL=IMG_NORM_PIXEL(velBuf,uv-vec2(texel.x,0.0)).y;
    float vR=IMG_NORM_PIXEL(velBuf,uv+vec2(texel.x,0.0)).y;
    float vB=IMG_NORM_PIXEL(velBuf,uv-vec2(0.0,texel.y)).x;
    float vT=IMG_NORM_PIXEL(velBuf,uv+vec2(0.0,texel.y)).x;
    return (vR-vL)-(vT-vB);
}

// ── Font / caption ───────────────────────────────────────────────────
float sampleChar(int ch, vec2 uv){
    if(ch<0||ch>36) return 0.0;
    if(uv.x<0.0||uv.x>1.0||uv.y<0.0||uv.y>1.0) return 0.0;
    return texture2D(fontAtlasTex,vec2((float(ch)+uv.x)/37.0,uv.y)).r;
}
int getChar(int slot){
    if(slot== 0) return int(msg_0);  if(slot== 1) return int(msg_1);
    if(slot== 2) return int(msg_2);  if(slot== 3) return int(msg_3);
    if(slot== 4) return int(msg_4);  if(slot== 5) return int(msg_5);
    if(slot== 6) return int(msg_6);  if(slot== 7) return int(msg_7);
    if(slot== 8) return int(msg_8);  if(slot== 9) return int(msg_9);
    if(slot==10) return int(msg_10); if(slot==11) return int(msg_11);
    if(slot==12) return int(msg_12); if(slot==13) return int(msg_13);
    if(slot==14) return int(msg_14); if(slot==15) return int(msg_15);
    if(slot==16) return int(msg_16); if(slot==17) return int(msg_17);
    if(slot==18) return int(msg_18); if(slot==19) return int(msg_19);
    if(slot==20) return int(msg_20); if(slot==21) return int(msg_21);
    if(slot==22) return int(msg_22); if(slot==23) return int(msg_23);
    if(slot==24) return int(msg_24); if(slot==25) return int(msg_25);
    if(slot==26) return int(msg_26); if(slot==27) return int(msg_27);
    if(slot==28) return int(msg_28); if(slot==29) return int(msg_29);
    if(slot==30) return int(msg_30); if(slot==31) return int(msg_31);
    if(slot==32) return int(msg_32); if(slot==33) return int(msg_33);
    if(slot==34) return int(msg_34); if(slot==35) return int(msg_35);
    if(slot==36) return int(msg_36); if(slot==37) return int(msg_37);
    if(slot==38) return int(msg_38); if(slot==39) return int(msg_39);
    if(slot==40) return int(msg_40); if(slot==41) return int(msg_41);
    if(slot==42) return int(msg_42); if(slot==43) return int(msg_43);
    if(slot==44) return int(msg_44); if(slot==45) return int(msg_45);
    if(slot==46) return int(msg_46); if(slot==47) return int(msg_47);
    return -1;
}
int charCount(){ int n=int(msg_len); if(n<=0)return 0; if(n>48)return 48; return n; }

// ═══════════════════════════════════════════════════════════════════
void main(){
    vec2 uv     = gl_FragCoord.xy / RENDERSIZE.xy;
    vec2 texel  = 1.0 / RENDERSIZE.xy;
    vec2 aspect = vec2(RENDERSIZE.x/RENDERSIZE.y,1.0);

    // ──────────────────────────────────────────────────────────────
    //  PASS 0 — velocity advection + projection + forces
    // ──────────────────────────────────────────────────────────────
    if(PASSINDEX==0){
        if(FRAMEINDEX<4){ gl_FragColor=vec4(0.0,0.0,0.0,1.0); return; }

        float bass=clamp(audioBass*bassDrive,0.0,2.0);
        float dt=flowSpeed*(1.0+1.4*bass*audioDepth);

        vec2 vel=IMG_NORM_PIXEL(velBuf,uv).xy;
        vec2 src=uv-vel*texel*dt;
        vec4 a=IMG_NORM_PIXEL(velBuf,src);
        vel=a.xy;
        float conc=a.z;

        float pL=IMG_NORM_PIXEL(prsBuf,uv-vec2(texel.x,0.0)).x;
        float pR=IMG_NORM_PIXEL(prsBuf,uv+vec2(texel.x,0.0)).x;
        float pB=IMG_NORM_PIXEL(prsBuf,uv-vec2(0.0,texel.y)).x;
        float pT=IMG_NORM_PIXEL(prsBuf,uv+vec2(0.0,texel.y)).x;
        vel-=0.5*vec2(pR-pL,pT-pB);

        // Audio-reactive stir position — bass pushes orbit radius
        float bassR=0.30+0.26*clamp(audioBass*bassDrive,0.0,1.0);
        float t=TIME*stirSpeed;
        vec2 m=vec2(0.5)+bassR*vec2(cos(t),sin(t*1.43));
        vec2 rel=(uv-m)*aspect;
        float fall=exp(-dot(rel,rel)/(0.11*0.11));
        vec2 tangent=normalize(vec2(-rel.y,rel.x)+1e-5);
        float midB=clamp(audioMid*midDrive,0.0,2.0);
        vel+=(stirVelF()+tangent*1.5)*fluidForce*(1.0+0.8*midB)*fall;
        conc+=dyeRate*(1.0+0.5*clamp(audioHigh*highDrive,0.0,2.0))*fall;

        // Vorticity confinement
        float cC=curlAt(uv,texel);
        float cL=abs(curlAt(uv-vec2(texel.x,0.0),texel));
        float cR=abs(curlAt(uv+vec2(texel.x,0.0),texel));
        float cBt=abs(curlAt(uv-vec2(0.0,texel.y),texel));
        float cTp=abs(curlAt(uv+vec2(0.0,texel.y),texel));
        vec2 g=vec2(cR-cL,cTp-cBt);
        g/=(length(g)+1e-5);
        vel+=vec2(g.y,-g.x)*cC*1.2;

        conc*=dissipation;
        vel=clamp(vel*0.9995,vec2(-8.0),vec2(8.0));

        if(uv.x<texel.x||uv.x>1.0-texel.x) vel.x=0.0;
        if(uv.y<texel.y||uv.y>1.0-texel.y) vel.y=0.0;

        gl_FragColor=vec4(vel,clamp(conc,0.0,1.0),1.0);
        return;
    }

    // ──────────────────────────────────────────────────────────────
    //  PASS 1 — divergence
    // ──────────────────────────────────────────────────────────────
    if(PASSINDEX==1){
        float vl=IMG_NORM_PIXEL(velBuf,uv-vec2(texel.x,0.0)).x;
        float vr=IMG_NORM_PIXEL(velBuf,uv+vec2(texel.x,0.0)).x;
        float vb=IMG_NORM_PIXEL(velBuf,uv-vec2(0.0,texel.y)).y;
        float vt=IMG_NORM_PIXEL(velBuf,uv+vec2(0.0,texel.y)).y;
        float div=0.5*((vr-vl)+(vt-vb));
        gl_FragColor=vec4(div,0.0,0.0,1.0);
        return;
    }

    // ──────────────────────────────────────────────────────────────
    //  PASS 2 — Jacobi pressure iteration
    // ──────────────────────────────────────────────────────────────
    if(PASSINDEX==2){
        if(FRAMEINDEX<4){ gl_FragColor=vec4(0.0); return; }
        float pL=IMG_NORM_PIXEL(prsBuf,uv-vec2(texel.x,0.0)).x;
        float pR=IMG_NORM_PIXEL(prsBuf,uv+vec2(texel.x,0.0)).x;
        float pB=IMG_NORM_PIXEL(prsBuf,uv-vec2(0.0,texel.y)).x;
        float pT=IMG_NORM_PIXEL(prsBuf,uv+vec2(0.0,texel.y)).x;
        float div=IMG_NORM_PIXEL(divBuf,uv).x;
        float p=(pL+pR+pB+pT-div)*0.25;
        gl_FragColor=vec4(p,0.0,0.0,1.0);
        return;
    }

    // ──────────────────────────────────────────────────────────────
    //  PASS 3 — Final composite
    // ──────────────────────────────────────────────────────────────
    vec2 res = RENDERSIZE;

    // Motion-kit offset on screen UV
    vec2 uvM = uv + mkMotion(uv, TIME);
    float ar = res.x/res.y;
    vec2 p;
    p.x = (uvM.x-0.5)*ar;
    p.y = uvM.y-0.5;

    float T    = TIME*motionSpeed;
    float bass = clamp(audioBass*bassDrive,0.0,2.0);
    float mid  = clamp(audioMid*midDrive,0.0,2.0);
    float high = clamp(audioHigh*highDrive,0.0,2.0);
    // Soft-kneed direct band followers (ambient fix): continuous, smooth,
    // independent of the drive binds — beatless swells still read.
    float bassF = pow(smoothstep(0.04,0.90,audioBass),1.3);
    float midF  = pow(smoothstep(0.05,0.90,audioMid),1.2);

    int gems=int(gemCount);
    if(gems>MAX_GEMS) gems=MAX_GEMS;
    if(gems<6)        gems=6;

    // ── Paper / backdrop ──────────────────────────────────────────
    vec2 wp=vec2(fbm2(p*1.3+T*0.05),fbm2(p*1.3+9.0-T*0.04));
    vec3 paper=paperColor.rgb;
    paper=mix(paper,paper*vec3(0.96,0.92,0.88),wp.x*0.25);
    paper*=1.0-0.18*dot(p,p);

    // ── Fluid field sample (from velBuf at screen UV) ─────────────
    vec4  fld         = IMG_NORM_PIXEL(velBuf, uv);
    float concentration = fld.z;
    float fluidAmp    = length(fld.xy);
    float fluidPhase  = atan(fld.y,fld.x)/TAU;

    // Metallic shading of fluid
    float cl2=IMG_NORM_PIXEL(velBuf,uv-vec2(texel.x,0.0)).z;
    float cr2=IMG_NORM_PIXEL(velBuf,uv+vec2(texel.x,0.0)).z;
    float ct2=IMG_NORM_PIXEL(velBuf,uv-vec2(0.0,texel.y)).z;
    float cb2=IMG_NORM_PIXEL(velBuf,uv+vec2(0.0,texel.y)).z;
    vec2 fGrad=vec2(cr2-cl2,cb2-ct2);
    float shade=0.3+0.7*max(dot(normalize(vec3(0.0,1.0,1.0)),
                normalize(vec3(fGrad.x*4.0,0.05,fGrad.y*4.0))),0.0);

    // Fluid color: palette-driven metallic with velocity hue shift
    float fluidHue=fract(fluidPhase+float(paletteMode)*0.25+T*0.03);
    vec3 metalBase=mix(vec3(0.10,0.12,0.18),gemPalette(int(paletteMode),fluidHue),
                       clamp(concentration,0.0,1.0));
    vec3 fluidCol=metalBase*shade*exposure;
    // Bass pulses a bright vein through the fluid
    fluidCol+=0.25*bass*audioDepth*spectrum(fluidHue+0.5)*concentration;

    // ── Aperture ──────────────────────────────────────────────────
    vec2 ap=p/vec2(0.92,1.05);
    float apR=ringRadius-gemSize*0.45;
    float apD=length(ap)-apR;
    float inside=smoothstep(0.005,-0.010,apD);

    // Stratified iridescent gradient (gemstone version)
    float strat=clamp(0.5+p.y*1.55+0.08*sin(p.x*3.5+T*0.7),0.0,1.0);
    strat+=0.10*bass*audioDepth*sin(p.y*4.0-T*1.4);
    vec3 inner=spectrum(strat+T*0.04);
    inner=mix(inner,vec3(0.18,0.42,0.30),smoothstep(0.62,0.92,1.0-strat)*0.55);
    float rim2=smoothstep(0.0,0.18,apR-length(ap));
    inner*=0.85+0.30*rim2;
    // Fuse with fluid interior
    inner=mix(inner,fluidCol,fluidMix*clamp(concentration*1.5,0.0,1.0));
    // Aperture interior breathes with the smoothed bands (ambient fix) —
    // the largest bright region in frame, so the swell is clearly visible.
    inner*=1.0+(0.20*bassF+0.10*midF)*audioDepth;
    // Fluid velocity adds swirling normal-map shimmer
    inner+=0.18*high*vec3(fGrad.x,-fGrad.y,0.5)*concentration;

    // Peak silhouette
    float peakD=abs(p.x*0.9)+max(0.0,p.y+0.04)*1.6;
    float peak=smoothstep(0.18,0.10,peakD)*smoothstep(-0.02,-0.10,p.y);
    inner=mix(inner,vec3(0.16,0.20,0.22),peak*0.65);

    // Optional texture — refracted into the aperture by the fluid's own gradient
    if(texMix>0.001){
        vec2 texUV=fract(uv+fGrad*0.10*(0.4+concentration));
        vec3 texCol=texture2D(inputTex,texUV).rgb;
        vec3 texBlend=mix(inner*texCol*1.4, texCol*shade, 0.5);
        inner=mix(inner,texBlend,texMix);
    }

    vec3 col=mix(paper,inner,inside);

    // ── Gem ring ──────────────────────────────────────────────────
    float bestD=1e6;
    vec3  bestCol=vec3(0.0);
    float bestSpec=0.0;
    float bestFres=0.0;
    float bestEdge=1e6;

    for(int i=0;i<MAX_GEMS;i++){
        if(i>=gems) break;
        float fi=float(i);
        float t01=fi/float(gems);
        float ang=3.14159*0.65+t01*TAU*0.95;
        float gdrift=sin(T*0.6+fi*1.9)*0.020;
        ang+=gdrift;

        float eA=energyA*smoothstep(0.55,0.05,t01);
        float eB=energyB*(1.0-abs(t01-0.5)*2.4);
        float eC=energyC*smoothstep(0.45,0.95,t01);
        float eOwn=clamp(max(eA,max(eB,eC)),0.0,1.0);

        float pinA=activeA*smoothstep(0.55,0.05,t01);
        float pinB=activeB*(1.0-abs(t01-0.5)*2.4);
        float pinOwn=clamp(max(pinA,pinB),0.0,1.0);

        float r=ringRadius+0.018*sin(T*0.4+fi*2.7)+0.040*eOwn;
        // Gems breathe with bass
        r+=0.012*bass*audioDepth*sin(fi*1.3+T*1.1);
        vec2 c=vec2(cos(ang),sin(ang))*r;
        float rot=(h11(fi+1.0)-0.5)*0.8+0.10*sin(T*0.3+fi);
        float cs=cos(rot),sn=sin(rot);
        vec2 q=mat2(cs,-sn,sn,cs)*(p-c);
        float rad=gemSize*mix(0.65,1.30,h11(fi+7.0));
        rad*=1.0+0.12*eOwn+0.06*pinOwn;
        rad*=1.0+0.04*bass*audioDepth;
        float wob=0.55+0.40*h11(fi+13.0);
        float d=gemSDF(q,rad,wob,fi*3.7);

        float k=0.012+0.060*eOwn;
        float pd=bestD;
        bestD=smin(bestD,d,k);
        float winLocal=clamp((pd-bestD)/k+0.5,0.0,1.0);

        // Facet normal from fbm gradient
        float fbmH=fbm2(q*(8.0+12.0*facetSharp)+fi*5.1);
        float fbmDx=fbm2(q*(8.0+12.0*facetSharp)+vec2(0.04,0.0)+fi*5.1);
        float fbmDy=fbm2(q*(8.0+12.0*facetSharp)+vec2(0.0,0.04)+fi*5.1);
        vec2 gradN=vec2(fbmDx-fbmH,fbmDy-fbmH)/0.04;
        vec2 radN=normalize(q+1e-4)*smoothstep(rad,0.0,length(q));
        vec3 N=normalize(vec3(radN+gradN*0.55,1.0));

        vec3 L=normalize(vec3(-0.45,0.65,0.6));
        vec3 V=vec3(0.0,0.0,1.0);
        vec3 H=normalize(L+V);
        float NdL=clamp(dot(N,L),0.0,1.0);
        float NdH=clamp(dot(N,H),0.0,1.0);
        float NdV=clamp(dot(N,V),0.0,1.0);
        float fres=pow(1.0-NdV,4.0);
        float spec=pow(NdH,mix(48.0,160.0,facetSharp));

        // Gem hue: blend palette with fluid phase for internal iridescence
        float hue=fract(h11(fi+21.0)+T*0.04+0.15*eOwn+fluidPhase*0.3*fluidMix);
        vec3 pig=gemPalette(int(paletteMode),hue);
        // Fluid concentration tints the gem interior
        pig=mix(pig,fluidCol*1.4,fluidMix*clamp(concentration*2.0,0.0,0.6));
        vec3 refr=mix(pig,vec3(1.0,0.62,0.30),0.25+0.30*fres);
        float ao=clamp(1.0+d/rad,0.0,1.0);
        vec3 body=refr*(0.35+0.55*NdL)*(0.55+0.45*ao);
        vec3 hi=vec3(1.0,0.96,0.88)*spec*(0.6+0.7*facetSharp);
        // Fluid velocity adds a coloured specular shimmer on gems
        hi=mix(hi,fluidCol*2.0,0.25*fluidAmp*fluidMix);
        vec3 rimC=mix(pig,vec3(1.0),0.55)*fres*0.7;
        vec3 surf=body+hi+rimC;

        // Mid-band sparkles + high-band fluid glints
        float spk=step(0.985,fbm2(q*65.0+T*4.0+fi*9.0));
        surf+=spk*mid*(0.6+0.8*pinOwn)*vec3(1.0,0.94,0.78);
        float fluidGlint=step(0.97,fbm2(q*45.0+T*6.0+fi*3.3));
        surf+=fluidGlint*high*concentration*0.5*spectrum(hue+0.3);

        if(winLocal>0.001){
            bestCol=mix(bestCol,surf,winLocal);
            bestSpec=mix(bestSpec,spec,winLocal);
            bestFres=mix(bestFres,fres,winLocal);
        }
        float ed=length(p-c);
        if(ed<bestEdge) bestEdge=ed;
    }

    // ── Compose gem ring ──────────────────────────────────────────
    float fw=fwidth(bestD);
    float fill=1.0-smoothstep(-fw,fw,bestD);
    float shD=bestD+0.014;
    float sh=(1.0-smoothstep(-0.005,0.025,shD))*0.25;
    col*=1.0-sh*(1.0-inside*0.6);
    col=mix(col,bestCol,fill);

    // Halo: blends fluid colour into the ring glow
    float halo=exp(-bestEdge*6.0)*(0.20+0.85*bass*audioDepth);
    vec3 haloCol=mix(
        mix(vec3(1.0,0.78,0.45),fluidCol,fluidMix*0.5),
        mix(vec3(0.55,0.85,1.0),fluidCol,fluidMix*0.6),
        inside);
    col+=halo*haloCol*(1.0-fill);

    // ── Caption ───────────────────────────────────────────────────
    int total=charCount();
    if(total>0){
        float capH=0.018*captionScale;
        float capW=capH*(5.0/7.0);
        float kern=capW*0.95;
        float rowW=ringRadius*1.10;
        int charsPerRow=int(rowW/max(kern,1e-4));
        if(charsPerRow<6)  charsPerRow=6;
        if(charsPerRow>24) charsPerRow=24;

        int usedRows=1;
        {
            int preR=0, preC=0;
            for(int i=0;i<MAX_WALK;i++){
                if(i>=total) break;
                int ch=getChar(i);
                if(ch==SPACE_CH){
                    int wlen=0;
                    for(int j=1;j<MAX_WALK;j++){
                        int gj=i+j;
                        if(gj>=total) break;
                        int chj=getChar(gj);
                        if(chj==SPACE_CH||chj<0||chj>36) break;
                        wlen++;
                    }
                    if(preC>0&&preC+1+wlen>charsPerRow){ preR++; preC=0; }
                    else if(preC>0){ preC++; }
                } else if(ch>=0&&ch<=36){
                    preC++;
                    if(preC>=charsPerRow){ preR++; preC=0; }
                }
            }
            usedRows=preR+1;
        }
        float blockH=float(usedRows)*capH*1.25;
        float blockW=float(charsPerRow)*kern;
        vec2 capOrigin=vec2(-blockW*0.5,-0.06-blockH*0.5);
        vec2 lp=p-capOrigin;

        float typed=(msgAge>=0.0)?clamp(msgAge*22.0,0.0,float(total)):float(total);

        int cursorR=0,cursorC=0;
        float caretX=0.0,caretY=0.0;
        float ink=0.0;
        for(int i=0;i<MAX_WALK;i++){
            if(i>=total) break;
            int ch=getChar(i);
            bool reveal=float(i)<typed;
            if(ch==SPACE_CH){
                int wlen=0;
                for(int j=1;j<MAX_WALK;j++){
                    int gj=i+j;
                    if(gj>=total) break;
                    int chj=getChar(gj);
                    if(chj==SPACE_CH||chj<0||chj>36) break;
                    wlen++;
                }
                if(cursorC>0&&cursorC+1+wlen>charsPerRow){ cursorR++; cursorC=0; }
                else if(cursorC>0){ cursorC++; }
            } else if(ch>=0&&ch<=36){
                if(reveal){
                    float cx=float(cursorC)*kern;
                    float cy=float(usedRows-1-cursorR)*capH*1.25;
                    vec2 glyphLocal=vec2((lp.x-cx)/capW,(lp.y-cy)/capH);
                    float s=sampleChar(ch,glyphLocal);
                    s=smoothstep(0.20,0.55,s);
                    ink=max(ink,s);
                    caretX=cx+kern;
                    caretY=cy;
                }
                cursorC++;
                if(cursorC>=charsPerRow){ cursorR++; cursorC=0; }
            }
        }
        if(msgAge>=0.0&&typed<float(total)){
            vec2 cl2=lp-vec2(caretX,caretY);
            float cb2=step(0.0,cl2.x)*step(cl2.x,capW*0.12)
                     *step(0.0,cl2.y)*step(cl2.y,capH);
            float blink=0.5+0.5*sin(TIME*5.5);
            ink=max(ink,cb2*blink);
        }
        // Caption: bright white with fluid-colour tint
        vec3 inkColor=mix(vec3(0.99,0.96,0.88),fluidCol*2.0,fluidMix*concentration*0.4);
        col=mix(col,inkColor,ink*inside*(1.0-fill));
    }

    // ── Gallery sweep + bloom + tooth ────────────────────────────
    float sweep=smoothstep(0.0,0.5,sin(p.x*1.4-p.y*0.4-T*0.5)*0.5+0.5);
    col+=pow(sweep,4.0)*0.04*vec3(1.0,0.95,0.85);

    float Lum=dot(col,vec3(0.299,0.587,0.114));
    col+=0.18*smoothstep(0.65,1.20,Lum)*col*(1.0+1.0*bass*audioDepth);
    float tooth=fbm2(p*res.y*0.018);
    col*=1.0+(tooth-0.5)*0.04;

    col=col/(1.0+0.55*col);
    col=pow(max(col,0.0),vec3(0.94));

    // Immediate (non-buffered) whole-frame follower — Round 3 (measured):
    // the round-2 pre-tonemap 0.25/0.15 version measured ambient 0.0 — the
    // double tonemap (here + fidApply) compressed it to ~±2% under a large
    // 0.035/frame baseline churn (motion kit + grain + sweep). POST-tonemap
    // on display values, deeper, and broadband (bass/mid/high weights match
    // the music's band mix). Floored depth so the room breathes even at low
    // Audio Depth. Silence -> exactly 1.0.
    float aD2=0.6+0.4*clamp(audioDepth,0.0,1.5);
    col*=1.0+aD2*(0.35*clamp(audioBass,0.0,1.0)
                 +0.22*clamp(audioMid,0.0,1.0)
                 +0.13*clamp(audioHigh,0.0,1.0));
    // Audio-energy sheen (Round 3, measured): a slow luminance ripple that
    // travels across the frame while music plays, amplitude tracking the
    // broadband level. Unlike a plain follower (whose per-frame delta rides
    // the band DERIVATIVE and drowns under this piece's 0.035/frame idle
    // churn), a moving pattern changes the frame at a rate proportional to
    // the band LEVEL itself — beatless ambient swells finally register.
    // Zero-mean ripple: silence -> exactly 1.0, no DC brightness shift.
    float envF=0.40*clamp(audioBass,0.0,1.0)
              +0.25*clamp(audioMid,0.0,1.0)
              +0.15*clamp(audioHigh,0.0,1.0);
    col*=1.0+envF*0.20*sin(p.x*2.6-p.y*1.8+TIME*5.0);

    col*=mkFlicker(gl_FragCoord.xy/RENDERSIZE-0.5,TIME);

    // ---- universal color block (defaults = no-op) ----
    // (background handled by the existing paperColor input)
    vec3 uc=fidApply(col,gl_FragCoord.xy);
    float ucL=dot(uc,vec3(0.299,0.587,0.114));
    uc=mix(vec3(ucL),uc,colorBoost);                       // saturation
    if(hueShift>0.0005){                                   // cheap hue rotate (YIQ)
        float hA=hueShift*6.2831853;
        float hC=cos(hA),hS=sin(hA);
        mat3 hM=mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
              +hC*mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
              +hS*mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc=clamp(hM*uc,0.0,1.0);
    }

    gl_FragColor=vec4(uc,1.0);
}
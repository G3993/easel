/*{
  "CATEGORIES": [
    "3D",
    "Generator",
    "Audio Reactive"
  ],
  "DESCRIPTION": "Fluid abstract 3D elements floating in space — SDF primitives (sphere, cube, capsule, torus, torus knot, glass disc) auto-morphing with refraction, studio three-point lighting, wave/nebula background, chromatic aberration, DoF, and full audio reactivity.",
  "INPUTS": [
    {
      "NAME": "keyAngle",
      "LABEL": "Key Light Angle",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 6.2832,
      "DEFAULT": 0.785
    },
    {
      "NAME": "keyElevation",
      "LABEL": "Key Elevation",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.5708,
      "DEFAULT": 0.7
    },
    {
      "NAME": "ambient",
      "LABEL": "Ambient",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.5,
      "DEFAULT": 0.09
    },
    {
      "NAME": "rimStrength",
      "LABEL": "Rim Strength",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.5,
      "DEFAULT": 0.6
    },
    {
      "NAME": "exposure",
      "LABEL": "Exposure",
      "TYPE": "float",
      "MIN": 0.3,
      "MAX": 3,
      "DEFAULT": 1
    },
    {
      "NAME": "uShape",
      "LABEL": "Shape",
      "TYPE": "long",
      "VALUES": [
        0,
        1,
        2,
        3,
        4,
        5,
        6,
        7
      ],
      "LABELS": [
        "Auto Morph",
        "Cube",
        "Prism",
        "Torus",
        "Torus Knot",
        "Sphere",
        "Octahedron",
        "Heart"
      ],
      "DEFAULT": 0,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "uTorThick",
      "LABEL": "Toroid Thickness",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.25,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "uMorphSpeed",
      "LABEL": "Morph Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 4,
      "DEFAULT": 0.7,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "floatAmp",
      "LABEL": "Float Amplitude",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.38,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "keyColor",
      "LABEL": "Key Light",
      "TYPE": "color",
      "DEFAULT": [
        1,
        0.94,
        0.82,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "fillColor",
      "LABEL": "Fill Light",
      "TYPE": "color",
      "DEFAULT": [
        0.55,
        0.7,
        1,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "uC1",
      "LABEL": "Color 1",
      "TYPE": "color",
      "DEFAULT": [
        1,
        0.82,
        0.55,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "uC2",
      "LABEL": "Color 2",
      "TYPE": "color",
      "DEFAULT": [
        0.4,
        0.7,
        1,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "uC3",
      "LABEL": "Color 3",
      "TYPE": "color",
      "DEFAULT": [
        1,
        0.25,
        0.45,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "uColMode",
      "LABEL": "Color Mode",
      "TYPE": "long",
      "VALUES": [
        0,
        1
      ],
      "LABELS": [
        "Lit",
        "Custom Palette"
      ],
      "DEFAULT": 0,
      "GROUP": "Color"
    },
    {
      "NAME": "chromaticAb",
      "LABEL": "Chromatic Ab.",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.18,
      "GROUP": "Color"
    },
    {
      "NAME": "hueShift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "LABEL": "Hue Shift",
      "GROUP": "Color"
    },
    {
      "NAME": "colorBoost",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "LABEL": "Color Boost",
      "GROUP": "Color"
    },
    {
      "NAME": "camDist",
      "LABEL": "Camera Distance",
      "TYPE": "float",
      "MIN": 1.5,
      "MAX": 12,
      "DEFAULT": 5.5,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "camHeight",
      "LABEL": "Camera Height",
      "TYPE": "float",
      "MIN": -3,
      "MAX": 4,
      "DEFAULT": 1,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "camOrbitSpeed",
      "LABEL": "Orbit Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.14,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "camAzimuth",
      "LABEL": "Camera Azimuth",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 6.2832,
      "DEFAULT": 0,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "dofStrength",
      "LABEL": "Depth of Field",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "bgStyle",
      "LABEL": "Background",
      "TYPE": "long",
      "VALUES": [
        0,
        1,
        2,
        3
      ],
      "LABELS": [
        "Void",
        "Nebula Wave",
        "Tillmans White",
        "Judd Cobalt"
      ],
      "DEFAULT": 1,
      "GROUP": "Background"
    },
    {
      "NAME": "bgColor",
      "TYPE": "color",
      "DEFAULT": [
        0,
        0,
        0,
        0
      ],
      "LABEL": "Background",
      "GROUP": "Background"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio React",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//  FLUID SHAPES — morphing SDF primitives, refractive surfaces,
//  studio three-point lighting, nebula background, full audio reactivity.
// ════════════════════════════════════════════════════════════════════════

#define MAX_STEPS 128
#define MAX_DIST  40.0
#define EPS       0.0008
#define PI        3.14159265359
#define TAU       6.28318530718

#define MAT_BG      0
#define MAT_CHALK   1
#define MAT_CHROME  2
#define MAT_COBALT  3
#define MAT_GLASS   4
#define MAT_MORPH   5

// ── Math helpers ────────────────────────────────────────────────────────
mat2 rot2(float a){ float c=cos(a),s=sin(a); return mat2(c,-s,s,c); }

// ── SDF primitives ──────────────────────────────────────────────────────
float sdSphere(vec3 p, float r){ return length(p)-r; }

float sdBox(vec3 p, vec3 s){
    p=abs(p)-s;
    return length(max(p,0.0))+min(max(p.x,max(p.y,p.z)),0.0);
}

float sdRoundBox(vec3 p, vec3 b, float r){
    vec3 q=abs(p)-b;
    return length(max(q,0.0))+min(max(q.x,max(q.y,q.z)),0.0)-r;
}

float sdCapsule(vec3 p, float h, float r){
    p.y-=clamp(p.y,-h,h);
    return length(p)-r;
}

float sdCylinder(vec3 p, float h, float r){
    vec2 d=vec2(length(p.xz)-r,abs(p.y)-h);
    return min(max(d.x,d.y),0.0)+length(max(d,0.0));
}

float sdTorus(vec3 p, float R, float r){
    vec2 q=vec2(length(p.xz)-R,p.y);
    return length(q)-r;
}

float sdBox2d(vec2 p, vec2 s){
    p=abs(p)-s;
    return length(max(p,0.0))+min(max(p.x,p.y),0.0);
}

float sdTorusKnot(vec3 p, float R){
    vec2 cp=vec2(length(p.xz)-R,p.y);
    float a=atan(p.x,p.z);
    cp=cp*rot2(a*8.0);
    cp.y=abs(cp.y)-0.3;
    return sdBox2d(cp,vec2(uTorThick*R*0.5,uTorThick*R))*0.4;
}

float sdTriPrism(vec3 p, vec2 h){
    vec3 q=abs(p);
    return max(q.z-h.y, max(q.x*sin(PI/3.0)+p.y*sin(PI/6.0),-p.y)-h.x*sin(PI/6.0));
}

float sdOctahedron(vec3 p){
    p=abs(p);
    return (p.x+p.y+p.z-1.0)*0.57735;
}

float sdHeart(vec3 p){
    float sc=1.2;
    vec3 h=p*sc;
    float yy=(1.25*h.y-sqrt(abs(h.x)));
    return (h.x*h.x+yy*yy+h.z*h.z-1.0)/sc;
}

// ── Morphing shape (from color_cube) ───────────────────────────────────
float morphShape(vec3 p){
    int s=int(uShape);
    if(s==1) return sdBox(p,vec3(1.0));
    if(s==2) return sdTriPrism(p,vec2(1.0,1.5));
    if(s==3) return sdTorus(p,1.8,uTorThick*1.8);
    if(s==4) return sdTorusKnot(p,1.8);
    if(s==5) return sdSphere(p,1.0);
    if(s==6) return sdOctahedron(p);
    if(s==7) return sdHeart(p);
    // Auto morph
    float tm=TIME*0.35*uMorphSpeed;
    float bass=clamp(audioBass,0.0,1.0)*audioReact;
    tm+=bass*0.3;
    int idx=int(mod(tm,4.0));
    float blend=smoothstep(0.2,0.8,mod(tm,1.0));
    float s0=sdTriPrism(p,vec2(1.0,1.5));
    float s1=sdBox(p,vec3(1.0));
    float s2=sdTorus(p,1.8,uTorThick*1.8);
    float s3=sdTorusKnot(p,1.8);
    if(idx==0) return mix(s0,s1,blend);
    if(idx==1) return mix(s1,s2,blend);
    if(idx==2) return mix(s2,s3,blend);
    return mix(s3,s0,blend);
}

// ── Background colour ───────────────────────────────────────────────────
vec3 bgNebula(vec3 rd){
    // Wave nebula — from color_cube background()
    vec3 colT=vec3(0.313,0.816,0.816);
    vec3 colM=vec3(0.745,0.118,0.243);
    vec3 colK=vec3(0.475,0.404,0.765);
    vec3 colH=vec3(1.0,0.776,0.224);
    float k=rd.y*0.5+0.5;
    vec3 bg=vec3(1.0-k);
    float a=atan(rd.x,rd.z);
    float fade=smoothstep(0.8,0.5,k);
    bg+=sin(a*2.0+TIME)*sin(a*10.0+TIME)*sin(a*4.0)*fade*colT;
    bg+=sin(a*10.0+TIME+10.0)*sin(a*2.0+TIME+10.0)*sin(a*6.0+10.0)*fade*colM;
    bg+=sin(a*5.0+TIME+20.0)*sin(a*3.0+TIME+30.0)*sin(a*8.0+20.0)*fade*colK;
    bg+=sin(a*3.0+TIME+30.0)*sin(a*5.0+TIME+20.0)*sin(a*10.0+30.0)*fade*colH;
    return bg;
}

vec3 bgVoid(vec3 rd){
    float g=clamp(rd.y*0.5+0.5,0.0,1.0);
    return mix(vec3(0.012,0.013,0.022),vec3(0.06,0.06,0.08),g);
}

vec3 bgWhite(vec3 rd){
    float g=clamp(rd.y*0.5+0.5,0.0,1.0);
    return mix(vec3(0.76,0.77,0.78),vec3(0.93,0.92,0.90),g);
}

vec3 bgCobalt(vec3 rd){
    float g=clamp(rd.y*0.5+0.5,0.0,1.0);
    return mix(vec3(0.06,0.16,0.55),vec3(0.10,0.28,0.78),g);
}

vec3 envColor(vec3 rd){
    int bs=int(bgStyle);
    vec3 env;
    if(bs==0)      env=bgVoid(rd);
    else if(bs==1) env=bgNebula(rd);
    else if(bs==2) env=bgWhite(rd);
    else           env=bgCobalt(rd);
    // User background: blend the environment/backdrop toward the chosen color.
    return mix(env, bgColor.rgb, bgColor.a);
}

// ── Scene map ───────────────────────────────────────────────────────────
struct Hit { float d; int mat; };

float breathe(int idx, float bass){
    float ph=float(idx)*1.91+TIME*1.4;
    float idle=0.5+0.5*sin(ph);
    float t=mix(idle,bass,clamp(bass*1.4,0.0,1.0));
    return mix(0.95,1.05,t);
}

vec3 floatPos(vec3 base, int idx){
    float amp=floatAmp;
    float mid=clamp(audioMid,0.0,1.0)*audioReact;
    float ph=float(idx)*2.39996+TIME;
    float px=sin(ph*0.71+1.3)*cos(ph*0.43)*amp;
    float py=sin(ph*0.83)*amp*(0.6+mid*0.4);
    float pz=cos(ph*0.61+0.7)*sin(ph*0.37)*amp;
    return base+vec3(px,py,pz);
}

Hit map(vec3 p){
    Hit best;
    best.d=1e9;
    best.mat=MAT_BG;

    float bass=clamp(audioBass,0.0,1.0)*audioReact;
    float high=clamp(audioHigh,0.0,1.0)*audioReact;

    // ── MORPHING central shape ──────────────────────────────────────────
    {
        float s=breathe(4,bass);
        vec3 lp=p-floatPos(vec3(0.0,0.2,0.0),4);
        // spin with audio
        float spinT=TIME*0.18+bass*0.3;
        lp.xy=rot2(spinT*1.1)*lp.xy;
        lp.yz=rot2(spinT*0.87)*lp.yz;
        lp.xz=rot2(spinT*0.63)*lp.xz;
        float d=morphShape(lp/s)*s;
        if(d<best.d){best.d=d;best.mat=MAT_MORPH;}
    }

    // ── Chalk sphere — left anchor ──────────────────────────────────────
    {
        float s=breathe(0,bass);
        vec3 lp=p-floatPos(vec3(-2.2,0.0,0.3),0);
        float d=sdSphere(lp,0.55*s);
        if(d<best.d){best.d=d;best.mat=MAT_CHALK;}
    }

    // ── Chrome cube — back right ────────────────────────────────────────
    {
        float s=breathe(1,bass);
        vec3 lp=p-floatPos(vec3(2.0,0.5,-0.8),1);
        lp.xz=rot2(TIME*0.22+high*0.3)*lp.xz;
        float d=sdRoundBox(lp,vec3(0.42*s),0.018);
        if(d<best.d){best.d=d;best.mat=MAT_CHROME;}
    }

    // ── Cobalt capsule — right ──────────────────────────────────────────
    {
        float s=breathe(2,bass);
        vec3 lp=p-floatPos(vec3(1.6,-0.4,1.1),2);
        lp.xz=rot2(-0.22)*lp.xz;
        float d=sdCapsule(lp,0.6*s,0.18*s);
        if(d<best.d){best.d=d;best.mat=MAT_COBALT;}
    }

    // ── Glass disc — lower front ────────────────────────────────────────
    {
        float s=breathe(3,bass);
        vec3 lp=p-floatPos(vec3(-1.4,-0.8,0.9),3);
        lp.xz=rot2(TIME*0.13)*lp.xz;
        float d=sdCylinder(lp,0.06*s,0.52*s);
        if(d<best.d){best.d=d;best.mat=MAT_GLASS;}
    }

    // ── Extra: floating torus — upper back ─────────────────────────────
    {
        float s=breathe(5,bass);
        vec3 lp=p-floatPos(vec3(-0.8,1.8,-1.4),5);
        lp.yz=rot2(TIME*0.19)*lp.yz;
        float d=sdTorus(lp,0.5*s,0.14*s);
        if(d<best.d){best.d=d;best.mat=MAT_CHROME;}
    }

    return best;
}

vec3 calcNormal(vec3 p){
    const vec2 e=vec2(EPS,0.0);
    return normalize(vec3(
        map(p+e.xyy).d-map(p-e.xyy).d,
        map(p+e.yxy).d-map(p-e.yxy).d,
        map(p+e.yyx).d-map(p-e.yyx).d
    ));
}

float softShadow(vec3 ro, vec3 rd){
    float res=1.0,t=0.05;
    for(int i=0;i<24;i++){
        if(t>7.0) break;
        float h=map(ro+rd*t).d;
        if(h<0.001){res=0.0;break;}
        res=min(res,8.0*h/t);
        t+=clamp(h,0.02,0.35);
    }
    return clamp(res,0.06,1.0);
}

float ao(vec3 p, vec3 n){
    float occ=0.0,sca=1.0;
    for(int i=0;i<5;i++){
        float h=0.02+0.12*float(i);
        occ+=(h-map(p+n*h).d)*sca;
        sca*=0.90;
    }
    return clamp(1.0-1.8*occ,0.0,1.0);
}

// ── Lighting direction helpers ──────────────────────────────────────────
vec3 sphDir(float a, float e){ return normalize(vec3(cos(a)*cos(e),sin(e),sin(a)*cos(e))); }
vec3 keyDir()  { return sphDir(keyAngle,keyElevation); }
vec3 fillDir() { return sphDir(keyAngle+PI,keyElevation*0.5); }
vec3 rimDir()  { return sphDir(keyAngle+PI*0.5,keyElevation+0.2); }
vec3 rimCol()  { return mix(keyColor.rgb,vec3(1.0),0.5); }

// ── PBR micro helpers ───────────────────────────────────────────────────
float ggx(float ndh,float a){float a2=a*a;float d=(ndh*ndh)*(a2-1.0)+1.0;return a2/(PI*d*d);}
float gSm(float ndv,float ndl,float a){float k=(a+1.0);k=k*k*0.125;return(ndv/(ndv*(1.0-k)+k))*(ndl/(ndl*(1.0-k)+k));}
vec3 fSch(float vdh,vec3 F0){return F0+(1.0-F0)*pow(1.0-vdh,5.0);}

// ── IBL proxy ───────────────────────────────────────────────────────────
vec3 ibl(vec3 dir){
    vec3 top=envColor(vec3(0.0,1.0,0.0));
    vec3 mid2=envColor(vec3(0.0,0.0,-1.0));
    vec3 bot=envColor(vec3(0.0,-1.0,0.0))*0.8;
    return (dir.y>0.0)?mix(mid2,top,smoothstep(0.0,1.0,dir.y))
                      :mix(mid2,bot,smoothstep(0.0,1.0,-dir.y));
}

// ── Material shading ────────────────────────────────────────────────────
vec3 shadeChalk(vec3 p,vec3 n,vec3 v){
    vec3 albedo=vec3(0.92,0.61,0.49);
    vec3 L=keyDir();
    float sh=softShadow(p+n*0.006,L);
    float wrap=max((dot(n,L)+0.25)/1.25,0.0);
    float fillT=max(dot(n,fillDir()),0.0);
    float rim=pow(1.0-max(dot(n,v),0.0),2.5)*max(dot(n,rimDir()),0.0);
    vec3 H=normalize(L+v);
    vec3 col=albedo*(ambient+keyColor.rgb*wrap*sh+fillColor.rgb*fillT*0.6)
            +rimCol()*rim*rimStrength*0.45
            +keyColor.rgb*pow(max(dot(n,H),0.0),28.0)*sh*0.07;
    return col*mix(0.55,1.0,ao(p,n));
}

vec3 shadeChrome(vec3 p,vec3 n,vec3 v){
    vec3 F0=vec3(0.92,0.94,0.96);
    vec3 R=reflect(-v,n);
    float shim=clamp(audioHigh,0.0,1.0)*audioReact;
    R=normalize(R+vec3(sin(TIME*7.3),cos(TIME*5.7),sin(TIME*4.1))*0.006*shim);
    vec3 reflCol=ibl(R);
    float t2=0.02;
    for(int i=0;i<24;i++){
        Hit s2=map(p+R*t2);
        if(s2.d<0.001){reflCol=ibl(R)*0.75;break;}
        t2+=s2.d;
        if(t2>5.0)break;
    }
    float fres=pow(1.0-max(dot(n,v),0.0),5.0);
    vec3 spec=mix(F0,vec3(1.0),fres)*reflCol;
    vec3 L=keyDir();
    vec3 H=normalize(L+v);
    float sh=softShadow(p+n*0.006,L);
    float rim=pow(1.0-max(dot(n,v),0.0),3.0)*max(dot(n,rimDir()),0.0);
    spec+=ambient*F0;
    spec+=keyColor.rgb*pow(max(dot(n,H),0.0),220.0)*sh*1.6;
    spec+=rimCol()*rim*rimStrength*0.35;
    return spec*mix(0.7,1.0,ao(p,n));
}

vec3 shadeCobalt(vec3 p,vec3 n,vec3 v){
    vec3 albedo=vec3(0.05,0.16,0.62);
    float a2=0.28;
    vec3 F0=mix(vec3(0.04),albedo,0.55);
    vec3 L=keyDir();
    vec3 H=normalize(L+v);
    float ndl=max(dot(n,L),0.0);
    float ndv=max(dot(n,v),1e-4);
    float ndh=max(dot(n,H),0.0);
    float vdh=max(dot(v,H),0.0);
    float sh=softShadow(p+n*0.006,L);
    vec3 F=fSch(vdh,F0);
    vec3 spec=(ggx(ndh,a2)*gSm(ndv,ndl,a2)*F)/max(4.0*ndv*ndl,1e-4);
    vec3 diff=(1.0-F)*0.55*albedo/PI;
    float fillT=max(dot(n,fillDir()),0.0);
    vec3 col=albedo*ambient+(diff+spec)*keyColor.rgb*ndl*sh+albedo*fillColor.rgb*fillT*0.55;
    vec3 R2=reflect(-v,n);
    float shim=clamp(audioHigh,0.0,1.0)*audioReact;
    R2=normalize(R2+vec3(sin(TIME*6.1),cos(TIME*4.3),sin(TIME*5.9))*0.005*shim);
    col+=ibl(R2)*F0*(1.0-a2*0.9)*0.5;
    float rim=pow(1.0-max(dot(n,v),0.0),3.0)*max(dot(n,rimDir()),0.0);
    col+=rimCol()*rim*rimStrength*0.7;
    return col*mix(0.7,1.0,ao(p,n));
}

vec3 shadeGlass(vec3 p,vec3 n,vec3 v){
    vec3 R=reflect(-v,n);
    vec3 T=refract(-v,n,1.0/1.45);
    if(length(T)<0.001) T=R;
    float t2=0.02; vec3 hp=p; int mh2=MAT_BG;
    for(int i=0;i<20;i++){
        Hit s2=map(p+T*t2);
        if(s2.d<0.001){hp=p+T*t2;mh2=s2.mat;break;}
        t2+=s2.d;
        if(t2>4.0)break;
    }
    vec3 trans;
    if(mh2==MAT_CHALK)       trans=vec3(0.92,0.61,0.49)*0.85;
    else if(mh2==MAT_COBALT) trans=vec3(0.05,0.16,0.62)*0.7;
    else if(mh2==MAT_CHROME) trans=vec3(0.85);
    else if(mh2==MAT_MORPH)  trans=ibl(T)*0.9;
    else                     trans=envColor(T);
    trans*=vec3(0.93,1.0,0.96);
    float fres=mix(0.04,1.0,pow(1.0-max(dot(n,v),0.0),5.0));
    vec3 col=mix(trans,ibl(R),fres);
    vec3 L=keyDir();
    vec3 H=normalize(L+v);
    float sh=softShadow(p+n*0.006,L);
    col+=ambient*vec3(0.05);
    col+=keyColor.rgb*pow(max(dot(n,H),0.0),200.0)*sh*1.3;
    float rim=pow(1.0-max(dot(n,v),0.0),4.0);
    col+=rimCol()*rim*rimStrength*0.45;
    return col;
}

vec3 shadeMorph(vec3 p,vec3 n,vec3 v){
    // Refractive morphing shape — glass-like with colour-cube-style inner nebula
    vec3 R=reflect(-v,n);
    vec3 T=refract(-v,n,1.0/1.38);
    if(length(T)<0.001) T=R;
    vec3 trans=envColor(T)*vec3(0.85,0.92,1.0);
    float fres=mix(0.05,1.0,pow(1.0-max(dot(n,v),0.0),4.0));
    // HDR specular glow on silhouette (from color_cube)
    float spec2=pow(max(dot(R,-v),0.0),32.0);
    vec3 hdrEdge=vec3(1.0,1.05,1.15)*(fres*1.4+spec2*2.5);
    float glow=smoothstep(0.55,0.95,fres);
    vec3 glowCol=vec3(1.1,0.9,1.4)*glow*1.6;
    vec3 col=mix(trans,ibl(R),fres)+hdrEdge+glowCol;
    // Subtle three-point
    vec3 L=keyDir();
    vec3 H=normalize(L+v);
    float sh=softShadow(p+n*0.005,L);
    col+=keyColor.rgb*pow(max(dot(n,H),0.0),180.0)*sh*1.0;
    float rim=pow(1.0-max(dot(n,v),0.0),3.5)*max(dot(n,rimDir()),0.0);
    col+=rimCol()*rim*rimStrength*0.6;
    return col*mix(0.75,1.0,ao(p,n));
}

vec3 shade(vec3 p,vec3 n,vec3 v,int mat){
    if(mat==MAT_CHALK)  return shadeChalk(p,n,v);
    if(mat==MAT_CHROME) return shadeChrome(p,n,v);
    if(mat==MAT_COBALT) return shadeCobalt(p,n,v);
    if(mat==MAT_GLASS)  return shadeGlass(p,n,v);
    if(mat==MAT_MORPH)  return shadeMorph(p,n,v);
    return envColor(v*-1.0); // background hit — shouldn't reach
}

// ── Ray march ────────────────────────────────────────────────────────────
vec4 march(vec3 ro, vec3 rd){
    float dist=0.0;
    int   mat=MAT_BG;
    bool  hit=false;
    for(int i=0;i<MAX_STEPS;i++){
        Hit s=map(ro+rd*dist);
        if(s.d<EPS){hit=true;mat=s.mat;break;}
        dist+=s.d*0.82;
        if(dist>MAX_DIST) break;
    }
    vec3 col;
    if(hit){
        vec3 hp=ro+rd*dist;
        vec3 n=calcNormal(hp);
        col=shade(hp,n,-rd,mat);
    } else {
        col=envColor(rd);
    }
    return vec4(col,hit?dist:MAX_DIST);
}

// ── Main ─────────────────────────────────────────────────────────────────
void main(){
    vec2 res=RENDERSIZE.xy;
    vec2 fc=(gl_FragCoord.xy-0.5*res)/res.y;

    float mid2=clamp(audioMid,0.0,1.0)*audioReact;
    float bass=clamp(audioBass,0.0,1.0)*audioReact;

    // Constant orbit rate + bounded phase offset — rate×audio scaled the
    // azimuth jump by elapsed TIME, so the camera teleported on mid swings.
    float orb=camAzimuth+TIME*camOrbitSpeed+0.3*mid2;
    // Camera-dolly breathing — smooth bounded zoom moves every edge in the
    // frame, restoring the continuous correlation the old rate-coupled
    // orbit provided (color gain alone was swamped by baseline motion).
    float cd=camDist*(1.0-0.10*bass-0.20*mid2);
    vec3 ro=vec3(cos(orb)*cd,camHeight,sin(orb)*cd);
    vec3 ta=vec3(0.0,0.2,0.0);
    vec3 fwd=normalize(ta-ro);
    vec3 rgt=normalize(cross(fwd,vec3(0.0,1.0,0.0)));
    vec3 up2=cross(rgt,fwd);

    float fov=1.05;

    // Chromatic aberration
    vec2 caOff=fc*chromaticAb*0.014;
    vec3 rdR=normalize(fwd+rgt*(fc.x+caOff.x)*fov+up2*(fc.y+caOff.y)*fov);
    vec3 rdG=normalize(fwd+rgt*fc.x*fov+up2*fc.y*fov);
    vec3 rdB=normalize(fwd+rgt*(fc.x-caOff.x)*fov+up2*(fc.y-caOff.y)*fov);

    vec4 sG=march(ro,rdG);
    vec3 col=sG.rgb;
    if(chromaticAb>0.001){
        col.r=march(ro,rdR).r;
        col.b=march(ro,rdB).b;
    }

    // Depth of field
    if(dofStrength>0.001){
        float focus=camDist*0.72;
        float coc=clamp(abs(sG.a-focus)/5.0,0.0,1.0)*dofStrength;
        vec3 blurAcc=vec3(0.0);
        for(int i=0;i<6;i++){
            float ang=TAU*(float(i)+0.5)/6.0;
            vec2 jj=vec2(cos(ang),sin(ang))*coc*0.05;
            vec3 rdJ=normalize(fwd+rgt*(fc.x+jj.x)*fov+up2*(fc.y+jj.y)*fov);
            blurAcc+=march(ro,rdJ).rgb;
        }
        col=mix(col,blurAcc/6.0,coc);
    }

    // Custom palette (color_cube style)
    int cm=int(uColMode);
    if(cm==1){
        col=uC1.rgb*col.r+uC2.rgb*col.g+uC3.rgb*col.b;
    }

    // Audio lift on object pixels — silhouette detection via scene depth.
    // (Was two stacked ~3x bass gains ⇒ up to ~17x per-frame swings = chop
    // and hard clipping; replaced with bounded lifts of matching feel.)
    float hitMask=sG.a<MAX_DIST*0.95?1.0:0.0;
    float audioLift=1.0+bass*0.6;
    col*=mix(1.0,audioLift,hitMask);

    // Calibrated whole-frame follower (linear bands) + decaying beat accent.
    // All terms are exactly 1.0 in silence.
    col*=1.0+0.25*clamp(audioBass,0.0,1.0)+0.15*clamp(audioMid,0.0,1.0);
    float kick=pow(max(audioBeatPulse,0.8*audioPunch),1.3);
    col*=1.0+0.08*kick;

    // Inversion flash every ~29s (from color_cube)
    {
        float ph=fract(TIME/29.0);
        float f=smoothstep(0.0,0.04,ph)*smoothstep(0.20,0.10,ph);
        col=mix(col,1.0-col,f);
    }

    // Vignette
    vec2 q=gl_FragCoord.xy/res-0.5;
    col*=clamp(1.0-dot(q,q)*0.52,0.0,1.0);

    // Film grain
    float gr=fract(sin(dot(gl_FragCoord.xy,vec2(12.9898,78.233))+TIME)*43758.5453);
    col+=(gr-0.5)*0.009;
    col*=0.96+0.04*sin(TIME*0.31);
    col*=exposure;

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

    gl_FragColor=vec4(col,1.0);
}
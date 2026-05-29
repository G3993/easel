/*{
  "DESCRIPTION": "Color World — a raymarched gallery. Real 3D: smooth-union'd liquid bodies and a flowing iridescent ribbon float in depth over a warm paper backdrop, lit in a procedural studio. Polished metal shading with environment reflection, Fresnel, thin-film iridescence and a brushed-bump normal that breaks highlights like oil paint. Soft atmospheric depth, drifting gold-leaf foil, a chromatic sun bloom and a slow gallery sheen are the analogue surprises. Camera breathes and parallaxes to the mouse like a window into the room. No hard edges, no pixels. Returns LINEAR HDR — host applies ACES.",
  "CATEGORIES": ["Generator", "Abstract", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "flow",        "LABEL": "Flow",            "TYPE": "float", "MIN": 0.0, "MAX": 2.5, "DEFAULT": 1.0 },
    { "NAME": "depth",       "LABEL": "Camera Drift",    "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 1.0 },
    { "NAME": "mouseParallax","LABEL":"Mouse Parallax",  "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 1.0 },
    { "NAME": "metal",       "LABEL": "Metallic Sheen",  "TYPE": "float", "MIN": 0.0, "MAX": 1.5, "DEFAULT": 0.9 },
    { "NAME": "iridescence", "LABEL": "Iridescence",     "TYPE": "float", "MIN": 0.0, "MAX": 1.5, "DEFAULT": 0.8 },
    { "NAME": "paint",       "LABEL": "Painterly",       "TYPE": "float", "MIN": 0.0, "MAX": 1.5, "DEFAULT": 0.7 },
    { "NAME": "goldLeaf",    "LABEL": "Gold Leaf",       "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 0.7 },
    { "NAME": "hueShift",    "LABEL": "Hue",             "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0 },
    { "NAME": "fog",         "LABEL": "Atmosphere",      "TYPE": "float", "MIN": 0.0, "MAX": 1.5, "DEFAULT": 0.8 },
    { "NAME": "bloom",       "LABEL": "Bloom",           "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 0.9 },
    { "NAME": "audioReact",  "LABEL": "Audio React",     "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 0.0 }
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//  COLOR WORLD  ·  raymarched · liquid metal · oil-painted light
//
//  This take is genuinely 3D — a distance field marched per pixel. Bodies
//  are ellipsoids smooth-min'd into one organic mass plus a flowing tube
//  that snakes through the room. Shading is studio-metal: reflected
//  procedural environment + Fresnel + thin-film, with the surface normal
//  perturbed by a fbm "brush bump" so the metal reads hand-painted, not
//  CG. Depth is real (camera dolly + atmospheric haze). Smooth only.
// ════════════════════════════════════════════════════════════════════════

const float TAU = 6.28318530718;

float h21(vec2 p){
    vec3 q = fract(vec3(p.xyx)*vec3(0.1031,0.1030,0.0973));
    q += dot(q, q.yzx+33.33);
    return fract((q.x+q.y)*q.z);
}
float vnoise(vec3 p){
    vec3 i = floor(p), f = fract(p);
    f = f*f*(3.0-2.0*f);
    vec2 uv = (i.xy + i.z*vec2(37.0,17.0)) + f.xy;
    float a = h21(uv), b = h21(uv+vec2(1,0));
    float c = h21(uv+vec2(0,1)), d = h21(uv+vec2(1,1));
    float lo = mix(mix(a,b,f.x), mix(c,d,f.x), f.y);
    uv += vec2(37.0,17.0);
    a = h21(uv); b = h21(uv+vec2(1,0)); c = h21(uv+vec2(0,1)); d = h21(uv+vec2(1,1));
    float hi = mix(mix(a,b,f.x), mix(c,d,f.x), f.y);
    return mix(lo,hi,f.z);
}
float fbm3(vec3 p){
    float s=0.0, a=0.55;
    for(int i=0;i<4;i++){ s+=a*vnoise(p); p=p*1.9+5.0; a*=0.52; }
    return s;
}
float smin(float a,float b,float k){
    float h=clamp(0.5+0.5*(b-a)/k,0.0,1.0);
    return mix(b,a,h)-k*h*(1.0-h);
}
vec3 spectrum(float t){ return 0.5+0.5*cos(TAU*(t+vec3(0.00,0.33,0.67))); }

float gT, gFlow, gAud;

// scene distance field — returns dist; writes colour-id in m
float map(vec3 p, out float m){
    float t = gT;
    // organic liquid mass: 8 drifting ellipsoids smooth-unioned
    float d = 1e5; m = 0.0;
    for (int i=0;i<8;i++){
        float fi=float(i);
        vec3 c = (vec3(h21(vec2(fi,1.0)),h21(vec2(fi,2.0)),h21(vec2(fi,3.0)))-0.5)*vec3(5.5,3.4,4.0);
        c += vec3(sin(t*0.23+fi*1.7), cos(t*0.19+fi*2.1), sin(t*0.17+fi*0.9))*1.1*gFlow;
        vec3 rad = mix(vec3(0.55), vec3(1.5), h21(vec2(fi,7.0)));
        rad *= 1.0 + 0.18*gAud;
        vec3 q = (p-c)/rad;
        float di = (length(q)-1.0)*min(min(rad.x,rad.y),rad.z);
        if (di<d || i==0){ if (i==0) d=di; }
        float k = 0.75;
        float pd = d;
        d = smin(d, di, k);
        m = mix(m, h21(vec2(fi,9.0)), clamp((pd-d)/k+0.5,0.0,1.0));
    }
    // flowing iridescent ribbon — the fluid surprise
    vec3 rp = p;
    float seg = 0.0;
    vec3 path = vec3(sin(t*0.4)*2.6, cos(t*0.33)*1.6 + sin(p.x*0.6+t)*0.7, 0.0);
    float tube = length((rp.xy-path.xy)) - (0.28 + 0.10*sin(p.x*1.3 - t*2.0));
    tube = max(tube, abs(p.z-0.6)-0.5);
    float pd2=d;
    d = smin(d, tube, 0.6);
    m = mix(m, 0.62, clamp((pd2-d)/0.6+0.5,0.0,1.0));
    // gentle global wobble so nothing is a perfect surface
    d += 0.04*fbm3(p*1.3 + t*0.2) - 0.02;
    return d;
}

vec3 calcNormal(vec3 p){
    vec2 e=vec2(0.0015,0.0);
    float m;
    return normalize(vec3(
        map(p+e.xyy,m)-map(p-e.xyy,m),
        map(p+e.yxy,m)-map(p-e.yxy,m),
        map(p+e.yyx,m)-map(p-e.yyx,m)));
}

// procedural polished-studio environment
vec3 envMap(vec3 r, float irid){
    float y=clamp(r.y*0.5+0.5,0.0,1.0);
    vec3 base=mix(vec3(0.55,0.52,0.50), vec3(0.97,0.95,0.92), smoothstep(0.2,0.85,y));
    float key =exp(-pow(length(r.xz-vec2( 0.40,0.35)),2.0)*5.0);
    float fill=exp(-pow(length(r.xz-vec2(-0.55,-0.1)),2.0)*7.0);
    base += key*vec3(1.0,0.96,0.86)*0.95 + fill*vec3(0.68,0.80,1.0)*0.45;
    vec3 iri=0.5+0.5*cos(TAU*(y*1.7 + gT*0.04 + vec3(0.0,0.35,0.70)));
    return mix(base, iri, 0.30*irid);
}

void main(){
    vec2 res=RENDERSIZE;
    vec2 uv=(gl_FragCoord.xy-0.5*res)/res.y;

    gT   = TIME*0.6*flow;
    gFlow= flow;
    gAud = clamp(audioLevel*audioReact,0.0,1.0);
    float pb = audioBass*audioReact;

    // camera breathes + dollies; mouse turns it into a 3D window
    vec2 m2 = (mousePos-0.5)*mouseParallax;
    float yaw = sin(gT*0.12)*0.30*depth + m2.x*0.8;
    float pit = sin(gT*0.09)*0.16*depth - m2.y*0.5;
    vec3 ro = vec3(sin(yaw)*8.0, pit*3.0 + 0.3, cos(yaw)*8.0);
    vec3 ta = vec3(0.0,0.0,0.0);
    vec3 ww=normalize(ta-ro), uu=normalize(cross(ww,vec3(0,1,0))), vv=cross(uu,ww);
    vec3 rd=normalize(uv.x*uu + uv.y*vv + 1.5*ww);

    // warm paper backdrop with marbled watercolour, soft chromatic sun
    vec2 wp = vec2(fbm3(vec3(uv*1.3,gT*0.2)), fbm3(vec3(uv*1.3+7.0,gT*0.2)));
    vec3 paper=mix(vec3(0.89,0.875,0.845), vec3(0.78,0.80,0.83), wp.x);
    float sun=exp(-length(uv-vec2(0.35,0.28))*2.2);
    paper += sun*vec3(1.05,0.85,0.55)*0.5;
    paper += vec3( sun*0.06, 0.0, -sun*0.05 );           // chromatic fringe
    paper *= 1.0-0.18*dot(uv,uv);

    // ── raymarch ──
    float tt=0.0, m=0.0; bool hit=false;
    for (int i=0;i<70;i++){
        vec3 p=ro+rd*tt;
        float d=map(p,m);
        if (d<0.004){ hit=true; break; }
        tt += d*0.85;
        if (tt>22.0) break;
    }

    vec3 col=paper;
    if (hit){
        vec3 p=ro+rd*tt;
        vec3 n=calcNormal(p);
        // brushed-bump normal → highlights break like oil paint
        vec3 bump=vec3(fbm3(p*4.0)-0.5, fbm3(p*4.0+11.0)-0.5, fbm3(p*4.0+23.0)-0.5);
        n=normalize(n + bump*0.35*paint);

        vec3 v=normalize(ro-p);
        vec3 rfl=reflect(-v,n);
        float fres=pow(1.0-clamp(dot(n,v),0.0,1.0),4.0);

        float hue=fract(m + hueShift + 0.10*fbm3(p*0.7+gT*0.1));
        vec3 pig=spectrum(hue); pig=mix(pig,pig*pig*1.4,0.4);

        vec3 env=envMap(rfl, iridescence);
        vec3 H=normalize(v+normalize(vec3(0.5,0.8,0.3)));
        float spec=pow(clamp(dot(n,H),0.0,1.0),96.0);
        float ao=clamp(map(p+n*0.25,m)/0.25,0.0,1.0);

        vec3 surf=mix(pig*0.5, env, 0.55*metal+0.4*fres);
        surf += spec*vec3(1.0,0.97,0.9)*1.5*metal;
        surf += fres*spectrum(hue+0.5)*0.6*iridescence;     // thin-film rim
        surf  = mix(surf, vec3(0.02,0.02,0.03),
                    smoothstep(0.55,0.95,m)*0.5);            // glossy near-black
        surf *= 0.35+0.65*ao;

        // soft-band the lighting → painterly blocking, not CG gradient
        float lum=dot(surf,vec3(0.299,0.587,0.114));
        float band=smoothstep(0.0,1.0, floor(lum*5.0+0.5)/5.0);
        surf=mix(surf, surf*(band/max(lum,1e-3)), 0.35*paint);

        // atmospheric depth — far bodies dissolve into the room
        float haze=1.0-exp(-tt*0.06*fog);
        col=mix(surf, paper, haze);
    }

    // gold leaf — soft foil flecks catching a travelling light
    float gm=smoothstep(0.55,0.95, fbm3(vec3(uv*2.2 + m2, gT*0.15)));
    float gl=pow(fbm3(vec3(uv*8.0 - vec2(gT*0.4,0.0), gT*0.3)),5.0);
    gl *= 0.6+0.8*(0.5+0.5*sin(gT*0.7+uv.x*6.0));
    col += gm*gl*goldLeaf*vec3(1.0,0.78,0.36)*(0.9+1.2*pb);

    // slow gallery sheen raking the canvas
    float sweep=smoothstep(0.0,0.5,sin(uv.x*1.0-uv.y*0.6-gT*0.45)*0.5+0.5);
    col += pow(sweep,3.0)*0.06*vec3(1.0,0.97,0.9);

    // bloom + bass lift
    float L=dot(col,vec3(0.299,0.587,0.114));
    col += bloom*0.16*smoothstep(0.6,1.3,L)*col;
    col *= 1.0+0.10*pb;

    // continuous canvas tooth (fibre, never a pixel grid)
    float tooth=fbm3(vec3(uv*res.y*0.012,0.0))+0.5*fbm3(vec3(uv*res.y*0.03,7.0));
    col *= 1.0+(tooth-0.75)*0.05;

    col=col/(1.0+0.6*col);
    col=pow(max(col,0.0),vec3(0.92));
    gl_FragColor=vec4(col,1.0);
}

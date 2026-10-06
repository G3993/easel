/*{
  "CATEGORIES": [
    "Automatically Converted"
  ],
  "DESCRIPTION": "Automatically converted from https://www.shadertoy.com/view/Ms2SD1",
  "IMPORTED": [],
  "INPUTS": [
    {
      "NAME": "iMouse",
      "TYPE": "point2D",
      "LABEL": "Mouse"
    },
    {
      "NAME": "SEA_HEIGHT",
      "MIN": 0,
      "MAX": 3,
      "TYPE": "float",
      "DEFAULT": 0.6,
      "GROUP": "Shape / Geometry",
      "LABEL": "Sea Height"
    },
    {
      "NAME": "SEA_FREQ",
      "MIN": 0,
      "MAX": 1,
      "TYPE": "float",
      "DEFAULT": 0.16,
      "GROUP": "Shape / Geometry",
      "LABEL": "Sea Frequency"
    },
    {
      "NAME": "SEA_CHOPPY",
      "MIN": 0,
      "MAX": 8,
      "TYPE": "float",
      "DEFAULT": 4,
      "GROUP": "Motion / Animation",
      "LABEL": "Sea Choppiness"
    },
    {
      "NAME": "SEA_SPEED",
      "MIN": 0,
      "MAX": 2,
      "TYPE": "float",
      "DEFAULT": 0.8,
      "GROUP": "Motion / Animation",
      "LABEL": "Sea Speed"
    },
    {
      "NAME": "SEA_BASE",
      "TYPE": "color",
      "DEFAULT": [
        0.1,
        0.19,
        0.22,
        1
      ],
      "GROUP": "Color",
      "LABEL": "Sea Base Color"
    },
    {
      "NAME": "SEA_WATER_COLOR",
      "TYPE": "color",
      "DEFAULT": [
        0.91,
        0.25,
        0.34,
        1
      ],
      "GROUP": "Color",
      "LABEL": "Water Color"
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
    return uc;
}



// "Seascape" by Alexander Alekseev aka TDM - 2014
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.

const int NUM_STEPS = 8;
const float PI	 	= 3.1415;
const float EPSILON	= 1e-3;
float EPSILON_NRM	= 0.1 / RENDERSIZE.x;

// sea
const int ITER_GEOMETRY = 3;
const int ITER_FRAGMENT = 5;
//const float SEA_HEIGHT = 0.6;
//const float SEA_CHOPPY = 4.0;
//const float SEA_SPEED = 0.8;
//const float SEA_FREQ = 0.16;
//const vec3 SEA_BASE = vec3(0.1,0.19,0.22);
//const vec3 SEA_WATER_COLOR = vec3(0.8,0.9,0.6);
// audio conditioning (playbook soft knees)
// Wider knees: top headroom at 0.95 so EDM's sustained near-peg bass keeps
// BREATHING through the knee instead of clamping flat (saturation fix).
float A_BASS  = pow(smoothstep(0.03, 0.95, audioBass), 1.3);
float A_MID   = pow(smoothstep(0.05, 0.92, audioMid), 1.2);
float A_HIGH  = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
float A_DRIVE = 0.25 + 0.75 * smoothstep(0.05, 0.9, audioEnergy);
// time-warp clock: the sea travels with the track's energy
float SEA_TIME = TIME * SEA_SPEED * (0.7 + 0.5 * A_DRIVE);
mat2 octave_m = mat2(1.6,1.2,-1.2,1.6);

// math
mat3 fromEuler(vec3 ang) {
	vec2 a1 = vec2(sin(ang.x),cos(ang.x));
    vec2 a2 = vec2(sin(ang.y),cos(ang.y));
    vec2 a3 = vec2(sin(ang.z),cos(ang.z));
    mat3 m;
    m[0] = vec3(a1.y*a3.y+a1.x*a2.x*a3.x,a1.y*a2.x*a3.x+a3.y*a1.x,-a2.y*a3.x);
	m[1] = vec3(-a2.y*a1.x,a1.y*a2.y,a2.x);
	m[2] = vec3(a3.y*a1.x*a2.x+a1.y*a3.x,a1.x*a3.x-a1.y*a3.y*a2.x,a2.y*a3.y);
	return m;
}
float hash( vec2 p ) {
	float h = dot(p,vec2(127.1,311.7));	
    return fract(sin(h)*43758.5453123);
}
float noise( in vec2 p ) {
    vec2 i = floor( p );
    vec2 f = fract( p );	
	vec2 u = f*f*(3.0-2.0*f);
    return -1.0+2.0*mix( mix( hash( i + vec2(0.0,0.0) ), 
                     hash( i + vec2(1.0,0.0) ), u.x),
                mix( hash( i + vec2(0.0,1.0) ), 
                     hash( i + vec2(1.0,1.0) ), u.x), u.y);
}

// lighting
float diffuse(vec3 n,vec3 l,float p) {
    return pow(dot(n,l) * 0.4 + 0.6,p);
}
float specular(vec3 n,vec3 l,vec3 e,float s) {    
    float nrm = (s + 8.0) / (3.1415 * 8.0);
    return pow(max(dot(reflect(e,n),l),0.0),s) * nrm;
}

// sky
vec3 getSkyColor(vec3 e) {
    e.y = max(e.y,0.0);
    vec3 ret;
    ret.x = pow(1.0-e.y,2.0);
    ret.y = 1.0-e.y;
    ret.z = 0.6+(1.0-e.y)*0.4;
    // HDR sun disc: lifts toward horizon glow to ~1.8 linear so bloom blooms it
    vec3 sunDir = normalize(vec3(0.0,0.1,0.8));
    float sun = max(dot(normalize(e + vec3(0.0,1e-4,0.0)), sunDir), 0.0);
    ret += vec3(1.0,0.85,0.7) * pow(sun, 80.0) * 1.8;
    return ret;
}

// sea
float sea_octave(vec2 uv, float choppy) {
    uv += noise(uv);        
    vec2 wv = 1.0-abs(sin(uv));
    vec2 swv = abs(cos(uv));    
    wv = mix(wv,swv,wv);
    return pow(1.0-pow(wv.x * wv.y,0.65),choppy);
}

float map(vec3 p) {
    float freq = SEA_FREQ;
    float amp = SEA_HEIGHT * (1.0 + 0.25 * A_BASS);   // bass swells the sea
    float choppy = SEA_CHOPPY * (1.0 + 0.2 * A_MID);  // mids chop the surface
    vec2 uv = p.xz; uv.x *= 0.75;
    
    float d, h = 0.0;    
    for(int i = 0; i < ITER_GEOMETRY; i++) {        
    	d = sea_octave((uv+SEA_TIME)*freq,choppy);
    	d += sea_octave((uv-SEA_TIME)*freq,choppy);
        h += d * amp;        
    	uv *= octave_m; freq *= 1.9; amp *= 0.22;
        choppy = mix(choppy,1.0,0.2);
    }
    return p.y - h;
}

float map_detailed(vec3 p) {
    float freq = SEA_FREQ;
    float amp = SEA_HEIGHT * (1.0 + 0.25 * A_BASS);   // keep in sync with map()
    float choppy = SEA_CHOPPY * (1.0 + 0.2 * A_MID);
    vec2 uv = p.xz; uv.x *= 0.75;
    
    float d, h = 0.0;    
    for(int i = 0; i < ITER_FRAGMENT; i++) {        
    	d = sea_octave((uv+SEA_TIME)*freq,choppy);
    	d += sea_octave((uv-SEA_TIME)*freq,choppy);
        h += d * amp;        
    	uv *= octave_m; freq *= 1.9; amp *= 0.22;
        choppy = mix(choppy,1.0,0.2);
    }
    return p.y - h;
}

vec3 getSeaColor(vec3 p, vec3 n, vec3 l, vec3 eye, vec3 dist) {
    float fresnel = 1.0 - max(dot(n,-eye),0.0);
    fresnel = pow(fresnel,3.0) * 0.65;

    vec3 reflected = getSkyColor(reflect(eye,n));
    vec3 refracted = SEA_BASE.rgb + diffuse(n,l,80.0) * SEA_WATER_COLOR.rgb * 0.12;

    vec3 color = mix(refracted,reflected,fresnel);

    float atten = max(1.0 - dot(dist,dist) * 0.001, 0.0);
    color += SEA_WATER_COLOR.rgb * (p.y - SEA_HEIGHT) * 0.18 * atten;

    // HDR specular peaks: sharp highlight core lifted to ~2.2 linear so bloom catches it
    // (highs sparkle the glints; punch adds a decaying flash on hits)
    float spec = specular(n,l,eye,60.0);
    float specCore = pow(max(dot(reflect(eye,n),l),0.0), 220.0);
    color += vec3(spec) * (1.0 + 0.3 * A_HIGH) + vec3(1.0,0.95,0.85) * specCore * 2.2 * (1.0 + 0.4 * A_HIGH + 0.5 * audioPunch);

    return color;
}

// tracing
vec3 getNormal(vec3 p, float eps) {
    vec3 n;
    n.y = map_detailed(p);    
    n.x = map_detailed(vec3(p.x+eps,p.y,p.z)) - n.y;
    n.z = map_detailed(vec3(p.x,p.y,p.z+eps)) - n.y;
    n.y = eps;
    return normalize(n);
}

float heightMapTracing(vec3 ori, vec3 dir, out vec3 p) {  
    float tm = 0.0;
    float tx = 1000.0;    
    float hx = map(ori + dir * tx);
    if(hx > 0.0) return tx;   
    float hm = map(ori + dir * tm);    
    float tmid = 0.0;
    for(int i = 0; i < NUM_STEPS; i++) {
        tmid = mix(tm,tx, hm/(hm-hx));                   
        p = ori + dir * tmid;                   
    	float hmid = map(p);
		if(hmid < 0.0) {
        	tx = tmid;
            hx = hmid;
        } else {
            tm = tmid;
            hm = hmid;
        }
    }
    return tmid;
}

// main
void main(){
	vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    uv = uv * 2.0 - 1.0;
    uv.x *= RENDERSIZE.x / RENDERSIZE.y;    
    float time = TIME * 0.3 + iMouse.x*0.01;
        
    // ray
    vec3 ang = vec3(sin(time*3.0)*0.1,sin(time)*0.2+0.3,time);    
    vec3 ori = vec3(0.0,3.5,time*5.0);
    vec3 dir = normalize(vec3(uv.xy,-2.0)); dir.z += length(uv) * 0.15;
    dir = normalize(dir) * fromEuler(ang);
    
    // tracing
    vec3 p;
    heightMapTracing(ori,dir,p);
    vec3 dist = p - ori;
    vec3 n = getNormal(p, dot(dist,dist) * EPSILON_NRM);
    vec3 light = normalize(vec3(0.0,1.0,0.8)); 
             
    // color — soft AA on horizon mix using fwidth
    float horizonAA = max(fwidth(dir.y) * 1.5, 1e-4);
    float horizonMix = pow(smoothstep(horizonAA, -0.05 - horizonAA, dir.y), 0.3);
    vec3 skyCol = mix(getSkyColor(dir), bgColor.rgb, bgColor.a); // universal background (a=0 -> no-op)
    vec3 color = mix(
        skyCol,
        getSeaColor(p,n,light,dir,dist),
        horizonMix);

    // Whole-frame luminance follower + decaying kick trace: the sea-geometry
    // swell alone is too slow/subtle to correlate; this keeps a visible,
    // non-pegging response at high input (audioBeatPulse already decays).
    color *= 1.0 + 0.22 * A_BASS + 0.10 * A_MID + 0.10 * audioBeatPulse;

    color = ucApply(color);

    // linear HDR out — host applies ACES tonemap
    gl_FragColor = vec4(color, 1.0);
}
/*{
  "DESCRIPTION": "Pattern 4 — five procedural math patterns (margarita spirals, plaid meltdown, sunlight rays, triple interference, digital bacteria) cycling on a curved CRT with scanlines and pixelated transitions. Bass punches zoom and pixel-crunch, mids add turbulence drift, highs shimmer the scanline grille.",
  "CREDIT": "Patterns by David A Roberts <https://davidar.io> (2016), CRT from shadertoy XtlSD7, ShaderClaw audio port",
  "CATEGORIES": [
    "Generator",
    "Pattern"
  ],
  "INPUTS": [
    {
      "NAME": "speed",
      "LABEL": "Speed",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.1,
      "MAX": 3.0
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio React",
      "TYPE": "float",
      "GROUP": "Audio Reactivity",
      "DEFAULT": 0.5,
      "MIN": 0.0,
      "MAX": 1.0
    },
    {
      "NAME": "crtAmount",
      "LABEL": "CRT Amount",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 1.0
    },
    {
      "NAME": "tintColor",
      "LABEL": "Tint",
      "TYPE": "color",
      "GROUP": "Color",
      "DEFAULT": [1.0, 1.0, 1.0, 1.0]
    },
    {
      "NAME": "brightness",
      "LABEL": "Brightness",
      "TYPE": "float",
      "GROUP": "Color",
      "DEFAULT": 1.0,
      "MIN": 0.2,
      "MAX": 3.0
    }
  ]
}*/

// 2016 David A Roberts <https://davidar.io>
#define AA 2.

#define PI 3.141592653589793

// CRT effects (curvature, vignette, scanlines and CRT grille)
// from <https://www.shadertoy.com/view/XtlSD7>
vec2 CRTCurveUV(vec2 uv) {
    uv = uv * 2.0 - 1.0;
    vec2 offset = abs(uv.yx) / vec2(6.0, 4.0);
    uv = uv + uv * offset * offset;
    uv = uv * 0.5 + 0.5;
    return uv;
}
void DrawVignette(inout vec3 color, vec2 uv) {
    float vignette = uv.x * uv.y * (1.0 - uv.x) * (1.0 - uv.y);
    vignette = clamp(pow(16.0 * vignette, 0.3), 0.0, 1.0);
    color *= mix(1.0, vignette, crtAmount);
}
void DrawScanline(inout vec3 color, vec2 uv, float shimmer) {
    float amp = 0.05 + 0.08 * shimmer;
    float scanline = clamp(1.0 - amp + amp * cos(3.14 * (uv.y + 0.008 * TIME) * 240.0), 0.0, 1.0);
    float grille = 0.85 + 0.15 * clamp((1.5 + 2.0 * shimmer) * cos(3.14 * uv.x * 640.0), 0.0, 1.0);
    color *= mix(1.0, scanline * grille * 1.2, crtAmount);
}

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

float atanp(in vec2 p) { return atan(p.y, p.x); }
float cube_root(float x) { return sign(x) * pow(abs(x), 1./3.); }
float sq(float x) { return x*x; }

vec3 margarita(in vec2 p) {
    float z = length(p) - 3.5 * atanp(p) + sin(p.x) + cos(p.y);
    if(mod(z,7.*PI) < PI/2.) return vec3(1,0,0);
    if(mod(z,1.*PI) < PI/2.) return vec3(0);
    return vec3(1);
}

vec3 digital_bacteria(in vec2 p) {
    p /= 4.;
    float x = sq(sin(p.x)+p.y) + sq(cos(p.y)+p.x);
    float y = cos(10.*p.x) + cos(10.*p.y) - sin(p.x*p.y);
    float z = sq(sin(floor(p.x))+floor(p.y)) + sq(cos(floor(p.y))+floor(p.x));
    if(17. < x && x < 21. && 17. < z && z < 21. && y < 0.)
        return vec3(1.,1.,85./256.);
    if(17. < z && z < 21.) return vec3(85./256.,0.,0.);
    if(17. < x && x < 21.) return vec3(170./256.,170./256.,0.);
    return vec3(85./256.,85./256.,0.);
}

vec3 threesome(in vec2 p) {
    p /= 3.;
    float z = 1.;
    z *= sin(length(p + vec2(5,0))) * cos(8.*atanp(p + vec2(5,0)));
    z *= sin(length(p - vec2(5,5))) * cos(8.*atanp(p - vec2(5,5)));
    z *= sin(length(p + vec2(0,5))) * cos(8.*atanp(p + vec2(0,5)));
    if(-0.1 < z && z < 0. || 0.2 < z) return vec3(0);
    return vec3(1);
}

vec3 plaid_meltdown(in vec2 p) {
    p /= 15.;
    p += 7.;
    float a = 2.*sin(p.x*sin(p.y) + p.y*sin(p.x));
    float b = cube_root(sin(2.5*sqrt(2.) * (p.x - p.y)));
    float c = cube_root(sin(2.5*sqrt(2.) * (p.x + p.y)));
    float d = sin(80.*p.x) + sin(80.*p.y);
    if(0.25 * (a + b + c) > 0.5 * d) return vec3(0);
    return vec3(1);
}

vec3 sunlight_revealed(in vec2 p) {
    p /= 6.;
    p.x += 2.;
    float a = length(vec2(3.-p.x,p.y)) + abs(p.y) + abs(1.-p.x);
    float f = atan(p.y,p.x-1.);
    float c = atan(p.y,p.x-3.);
    float R = sq(p.x-1.) + sq(p.y);
    vec3 col = vec3(0);
    bool mixed = false;
    if(5. < a && a < 7. && mod(f,PI/7.) < PI/14.) {
        col += vec3(0.,82./256.,173./256.);
        mixed = true;
    }
    if(5. < a && a < 7. && mod(c,PI/9.) < PI/18.) {
        col += vec3(1,0,0);
        if(mixed) col /= 2.;
        mixed = true;
    }
    if(5. < a && a < 7. && mod(f,PI/8.) < PI/16.) {
        col += vec3(1,1,0);
        if(mixed) col /= 2.;
        mixed = true;
    }
    if((45.-3.*p.x)*PI/180. < f && f < (47.-p.x)*PI/180. && p.y > 0.1*p.x
       && mod(log(R)/log(f),2.) < 1.) {
        col += vec3(1);
        if(mixed) col /= 2.;
    }
    return col;
}

void main() {
    float T = TIME * speed;
    float t = mod(T, 10.);

    // audio conditioning — soft knees + floors, never raw (playbook law 6)
    float bassP  = pow(knee(audioBass, 0.05, 0.85), 1.6);
    float midP   = pow(knee(audioMid,  0.08, 0.90), 1.3);
    float highP  = pow(knee(audioHigh, 0.10, 0.90), 1.2);
    float levelP = knee(audioLevel, 0.05, 0.90);
    float ar = audioReact;

    // pixelation: strong at pattern crossfades, bass crunches it back in mid-cycle
    float fade = smoothstep(0.,2.,t) - smoothstep(8.,10.,t);
    float pixAmt = fade * (1.0 - 0.95 * ar * bassP);
    float scale = RENDERSIZE.y/50.*AA*pixAmt + 1.;

    vec3 color = vec3(0);
    for(float i = 0.; i < AA*AA - 0.5; i += 1.) {
        vec2 uv = (gl_FragCoord.xy + vec2(floor(i/AA), mod(i,AA))/AA) / RENDERSIZE.xy;
        vec2 crtUV = mix(uv, CRTCurveUV(uv), crtAmount);
        if (crtUV.x < 0.0 || crtUV.x > 1.0 || crtUV.y < 0.0 || crtUV.y > 1.0) continue;
        vec2 p = 50.0 * crtUV - 25.0;
        p *= (0.8 + 0.03*mod(T,10.)) * (1.0 - 0.16 * ar * bassP);
        p += (mod(T,10.) - 5.) * 0.35;
        p.x *= RENDERSIZE.x / RENDERSIZE.y;
        // mids stir mid-scale turbulence into the pattern field (playbook law 3)
        p += ar * midP * 0.35 * vec2(sin(p.y*0.3 + TIME), cos(p.x*0.3 + TIME*1.3));
        // loud music adds continuous shimmer motion (sustained response)
        p += ar * levelP * 0.45 * vec2(sin(TIME*3.7 + p.y*0.7), cos(TIME*4.3 + p.x*0.7));
        p = floor(p*scale)/scale;

        vec3 c = vec3(0);
        if     (mod(0.1*T,5.) < 1.) c = margarita(p);
        else if(mod(0.1*T,5.) < 2.) c = plaid_meltdown(p);
        else if(mod(0.1*T,5.) < 3.) c = sunlight_revealed(p);
        else if(mod(0.1*T,5.) < 4.) c = threesome(p);
        else                        c = digital_bacteria(p);

        // sustained lift dips below 1 so it stays visible on saturated colors
        c *= mix(1.0, 0.72 + 0.42 * levelP + 0.14 * bassP, ar);
        DrawVignette(c, crtUV);
        DrawScanline(c, uv, ar * highP);
        color += c / (AA*AA);
    }

    gl_FragColor = vec4(color * tintColor.rgb * brightness, 1.0);
}

/*{
  "DESCRIPTION": "Snowing — a rippling glitter field: waves roll across a sparkling cloth whose micro-facet cells catch two drifting lights and a pulsing main beam, all wrapped in soft bloom. Bass pulses the main light, mids swell the waves and drifting lights, highs ignite the glitter sparkle. Glitter color is editable.",
  "CREDIT": "Glitter cloth + bloom chain by fenix (Shadertoy), ShaderClaw audio port",
  "CATEGORIES": [
    "Generator",
    "3D"
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
      "NAME": "glitterTint",
      "LABEL": "Glitter Color",
      "TYPE": "color",
      "DEFAULT": [0.4, 0.05, 0.9, 1.0]
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
  ],
  "PASSES": [
    {
      "TARGET": "sceneBuf"
    },
    {
      "TARGET": "bloomCut",
      "WIDTH": "$WIDTH/2",
      "HEIGHT": "$HEIGHT/2"
    },
    {
      "TARGET": "blurH",
      "WIDTH": "$WIDTH/2",
      "HEIGHT": "$HEIGHT/2"
    },
    {
      "TARGET": "blurV",
      "WIDTH": "$WIDTH/2",
      "HEIGHT": "$HEIGHT/2"
    },
    {}
  ]
}*/

#define PI 3.1415927
#define STEPS 100
#define BLOOM_THRESHOLD 1.0
#define GLITTER_SCALE (0.135 * RENDERSIZE.y)
#define LD normalize(vec3(0.5, 0.25, 1.0))
#define NUM_LIGHTS 2

float gBass, gMid, gHigh, gLevel;

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

vec3 hash32(vec2 src) {
    vec3 p3 = fract(src.xyx * vec3(0.1031, 0.1030, 0.0973));
    p3 += dot(p3, p3.yxz + 33.33);
    return fract((p3.xxy + p3.yzz) * p3.zyx);
}

vec4 hash41(float src) {
    vec4 p4 = fract(src * vec4(0.1031, 0.1030, 0.0973, 0.1099));
    p4 += dot(p4, p4.wzxy + 33.33);
    return fract((p4.xxyz + p4.yzzw) * p4.zywx);
}

float SineNoise(vec2 p) {
    float h = 0.0;
    float w = 1.0;
    float tw = 0.0;
    float T = TIME * speed;
    for (int i = 0; i < 6; i++) {
        vec2 uv = p - T * vec2(1.0, -0.4);
        h += sin(uv.x + p.y * 4.55) * sin(uv.y) * w;
        tw += w;
        w *= 0.5;
        p *= vec2(1.71828, 1.2);
    }
    return h / tw;
}

float map(vec3 p) {
    float d = p.z;
    // slow loudness swell only — fast bands here would re-roll the whole field
    float amp = 0.25 * mix(1.0, 0.75 + 0.5 * gLevel, audioReact);
    d += SineNoise(p.xy * 3.5 * vec2(1.0, 0.1)) * amp;
    return d;
}

float RM(vec3 ro, vec3 rd) {
    float t = 0.0;
    for (int i = 0; i < STEPS; i++) {
        float d = map(ro + t * rd);
        if (abs(d) < 0.001) break;
        t += d;
    }
    return t;
}

vec3 Normal(vec3 p) {
    const float h = 0.001;
    const vec2 k = vec2(1, -1);
    return normalize(k.xyy * map(p + k.xyy * h) +
                     k.yyx * map(p + k.yyx * h) +
                     k.yxy * map(p + k.yxy * h) +
                     k.xxx * map(p + k.xxx * h));
}

vec3 rayD(vec3 ro, vec3 lookAt, vec2 uv) {
    vec3 cf = normalize(lookAt - ro);
    vec3 cr = normalize(cross(cf, vec3(0, 1, 0)));
    vec3 cu = normalize(cross(cr, cf));
    return normalize(uv.x * cr + uv.y * cu + cf);
}

vec4 passScene() {
    vec2 uv = (2.0 * gl_FragCoord.xy - RENDERSIZE.xy) / RENDERSIZE.y;
    float T = TIME * speed;
    float ar = audioReact;
    vec3 glitterColor = glitterTint.rgb;

    vec3 ro = vec3(0, 0, 2);
    vec3 rd = rayD(ro, vec3(0.0), uv);
    float d = RM(ro, rd);
    vec3 p = ro + d * rd;
    vec3 nor = Normal(p);

    // glitter micro-facet cell
    vec3 cell = floor(p * GLITTER_SCALE);
    vec3 glNor = hash32(cell.xy);
    glNor = normalize(glNor * vec3(2.0, 2.0, 1.0) - vec3(1.0, 1.0, 0.0));
    // highs push the facets away from the base normal = more sparkle
    float sparkle = mix(1.0, 1.0 + 1.6 * gHigh, ar);
    vec3 glNor1 = normalize(mix(nor, glNor, 0.2 * sparkle));
    vec3 glNor2 = normalize(mix(nor, glNor, min(0.5 * sparkle, 0.85)));

    vec3 refl1 = reflect(rd, glNor1);
    vec3 refl2 = reflect(rd, glNor2);

    vec3 spec = vec3(0.0);
    vec3 baseCol = vec3(0.0);

    // main beam alternates with the drifting lights; bass pulses it back on
    float lightOnOffAlpha = smoothstep(-0.3, 0.3, sin(T * 1.5));
    lightOnOffAlpha = clamp(lightOnOffAlpha - ar * 0.7 * gBass, 0.0, 1.0);

    float diffuse = max(0.0, dot(nor, LD));
    float spec1 = max(0.0, dot(refl1, LD));
    float spec2 = max(0.0, dot(refl2, LD));
    spec += vec3(pow(spec1, 128.0));
    spec += vec3(pow(spec2, 16.0) * 0.75 * diffuse);
    spec *= glitterColor;
    float lightScalar = mix(1.0, 0.0, lightOnOffAlpha)
                      * mix(1.0, 0.5 + 1.8 * gBass, ar);
    baseCol += glitterColor * 0.1 * diffuse * lightScalar;
    spec *= 5.5 * lightScalar;

    // two drifting specular lights, mids feed them
    vec3 specLights = vec3(0.0);
    for (int i = 0; i < NUM_LIGHTS; i++) {
        vec4 rnd = hash41(float(i) + 1.7);
        vec4 movePrms = mix(vec4(0.3, 0.3, 0.0, 0.0), vec4(1.4, 1.4, 2.0 * PI, 2.0 * PI), rnd);
        vec2 lp2 = vec2(sin(T * movePrms.x + movePrms.z), cos(T * movePrms.y + movePrms.w))
                 * vec2(1.6, 1.2);
        vec3 lp = vec3(lp2, 1.0);
        vec3 ld2 = normalize(lp - p);

        float dif2 = max(0.0, dot(nor, ld2));
        float s = pow(max(0.0, dot(refl1, ld2)), 128.0)
                + pow(max(0.0, dot(refl2, ld2)), 16.0) * 0.75 * dif2;
        s *= exp(-length(lp - p) * 0.75);
        specLights += s * vec3(1.0, 0.9, 0.85);
    }
    spec += specLights * 4.25 * lightOnOffAlpha * mix(1.0, 0.5 + 1.7 * gMid, ar);

    return vec4(baseCol + spec, 1.0);
}

vec3 gaussBlur3(sampler2D smp, vec2 pixel, vec2 res, vec2 blurDir) {
    const int BSTEPS = 24;
    const float radius = 20.0;
    float stepSize = 2.0 * radius / float(BSTEPS - 1);
    vec2 invRes = 1.0 / res;
    const float gaussSigma = 5.0;
    float gaussExpFactor = 1.0 / (2.0 * gaussSigma * gaussSigma);
    vec3 v = vec3(0.0);
    float tw = 0.0;
    for (int i = 0; i < BSTEPS; i++) {
        float o = -radius + float(i) * stepSize;
        vec2 uv = (pixel + o * blurDir) * invRes;
        vec3 val = texture2D(smp, uv).rgb;
        float w = exp(-o * o * gaussExpFactor);
        v += val * w;
        tw += w;
    }
    return v / tw;
}

void main() {
    gBass  = pow(knee(audioBass, 0.05, 0.85), 1.6);
    gMid   = pow(knee(audioMid,  0.08, 0.90), 1.3);
    gHigh  = pow(knee(audioHigh, 0.10, 0.90), 1.2);
    gLevel = knee(audioLevel, 0.05, 0.90);

    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    if (PASSINDEX == 0) {
        gl_FragColor = passScene();
    } else if (PASSINDEX == 1) {
        vec3 col = texture2D(sceneBuf, uv).rgb;
        if (dot(col, col) < BLOOM_THRESHOLD * BLOOM_THRESHOLD) col = vec3(0.0);
        gl_FragColor = vec4(col, 1.0);
    } else if (PASSINDEX == 2) {
        gl_FragColor = vec4(gaussBlur3(bloomCut, gl_FragCoord.xy, RENDERSIZE.xy, vec2(1, 0)), 1.0);
    } else if (PASSINDEX == 3) {
        gl_FragColor = vec4(gaussBlur3(blurH, gl_FragCoord.xy, RENDERSIZE.xy, vec2(0, 1)), 1.0);
    } else {
        vec3 color = texture2D(sceneBuf, uv).rgb;
        vec3 bloom = texture2D(blurV, uv).rgb;
        color = pow(max(color, 0.0), vec3(1.0 / 2.2));
        color += bloom;
        // sustained loudness lift keeps quiet/loud states distinguishable
        color *= 1.0 + audioReact * 0.3 * gLevel;
        color *= tintColor.rgb * brightness;
        gl_FragColor = vec4(color, 1.0);
    }
}

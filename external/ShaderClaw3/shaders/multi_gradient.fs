/*{
  "DESCRIPTION": "Your shader description",
  "CREDIT": "by you",
  "CATEGORIES": [
    "Your category"
  ],
  "INPUTS": [
    {
      "NAME": "Color1_Y",
      "TYPE": "float",
      "MIN": -1.5,
      "MAX": 1.5,
      "DEFAULT": -0.5,
      "LABEL": "Color 1 Y Position"
    },
    {
      "NAME": "Color2_Y",
      "TYPE": "float",
      "MIN": -2,
      "MAX": 2,
      "DEFAULT": 0,
      "LABEL": "Color 2 Y Position"
    },
    {
      "NAME": "Color3_Y",
      "TYPE": "float",
      "MIN": -2,
      "MAX": 2,
      "DEFAULT": 0.5,
      "LABEL": "Color 3 Y Position"
    },
    {
      "NAME": "Color4_Y",
      "TYPE": "float",
      "MIN": -1.5,
      "MAX": 1.5,
      "DEFAULT": 0.5,
      "LABEL": "Color 4 Y Position"
    },
    {
      "NAME": "Color1_Radius",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 1,
      "LABEL": "Color 1 Radius",
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "Color2_Radius",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 1,
      "LABEL": "Color 2 Radius",
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "Color3_Radius",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 1,
      "LABEL": "Color 3 Radius",
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "Color4_Radius",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 1,
      "LABEL": "Color 4 Radius",
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "Color1",
      "TYPE": "color",
      "DEFAULT": [
        0.91,
        0.25,
        0.34,
        1
      ],
      "LABEL": "Color 1",
      "GROUP": "Color"
    },
    {
      "NAME": "Color2",
      "TYPE": "color",
      "DEFAULT": [
        0.91,
        0.25,
        0.34,
        1
      ],
      "LABEL": "Color 2",
      "GROUP": "Color"
    },
    {
      "NAME": "Color3",
      "TYPE": "color",
      "DEFAULT": [
        1,
        1,
        1,
        1
      ],
      "LABEL": "Color 3",
      "GROUP": "Color"
    },
    {
      "NAME": "Color4",
      "TYPE": "color",
      "DEFAULT": [
        0,
        0,
        0,
        1
      ],
      "LABEL": "Color 4",
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
      "NAME": "VERTICAL",
      "TYPE": "bool",
      "DEFAULT": 1,
      "LABEL": "Vertical Layout",
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "CanvasColor",
      "TYPE": "color",
      "DEFAULT": [
        0,
        0,
        0,
        1
      ],
      "LABEL": "Canvas Color",
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
    }
  ]
}*/

// Adapted from "RGB Soft Lights" by BitOfGold: https://www.shadertoy.com/view/llVGz1
// I've been playing a lot of "No Man's Sky" and thought this mod would help me recreate the vibe. :-)

vec3 iResolution = vec3(RENDERSIZE, 1.);

vec3 softLight(vec3 canvas, vec2 uv, vec2 center, float r, vec3 color) {
    float d = clamp(1.0-length(center-uv)/r,0.0,1.0);
    return(canvas + d*color);
}

void mainImage( out vec4 fragColor, in vec2 fragCoord )
{
	vec2 uv = fragCoord.xy / RENDERSIZE.xy;
	
	if (VERTICAL) {uv.y=uv.x;}
	
	float ASPECT = (RENDERSIZE.x/RENDERSIZE.y);
	uv.x = uv.y;
	
	vec3 canvas = vec3(CanvasColor.rgb);
	// Audio breathing on the blob radii — soft knee, ±22% depth so ambient's
	// slow band swells visibly grow/shrink the light pools (a gentle pulse,
	// never a jolt). Low knee start + mild pow so the 0.1-0.8 ambient bass
	// range maps across the whole travel; hi 0.92 keeps EDM headroom.
	float _aKnee = smoothstep(0.03, 0.92, audioBass);
	float _breathe = 1.0 + 0.22 * pow(_aKnee, 1.2);
    canvas = softLight(canvas, uv-(Color1_Y), vec2(.5, 0.5), Color1_Radius * _breathe, vec3(Color1.rgb));
    canvas = softLight(canvas, uv-Color2_Y, vec2(.5, 0.5), Color2_Radius * _breathe, vec3(Color2.rgb));
    canvas = softLight(canvas, (uv-Color3_Y), vec2(.5, 0.5), Color3_Radius * _breathe, vec3(Color3.rgb));
    canvas = softLight(canvas, (uv-Color4_Y), vec2(.5, 0.5), Color4_Radius * _breathe, vec3(Color4.rgb));
	// Mid band breathes overall luminance (+12%) — a second continuous
	// follower so phase-offset ambient swells read on two visual axes.
	float _midSm = smoothstep(0.05, 0.92, audioMid);
	canvas *= 1.0 + 0.12 * _midSm;
	// ---- universal color block (defaults = no-op) ----
	float ucL = dot(canvas, vec3(0.299, 0.587, 0.114));
	vec3 uc = mix(vec3(ucL), canvas, colorBoost);
	if (hueShift > 0.0005) {
		float hueA = hueShift * 6.2831853;
		float hueC = cos(hueA), hueS = sin(hueA);
		mat3 hueM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
		          + hueC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
		          + hueS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
		uc = clamp(hueM * uc, 0.0, 1.0);
	}
	uc = mix(uc, bgColor.rgb, bgColor.a * (1.0 - smoothstep(0.0, 0.35, ucL)));
	fragColor = vec4(uc,1.0);
}


void main(void) {
    mainImage(gl_FragColor, gl_FragCoord.xy);
}
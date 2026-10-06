/*{
  "CREDIT": "by mojovideotech, smoothed by assistant",
  "CATEGORIES": [
    "generator",
    "blobs",
    "distance",
    "noise"
  ],
  "DESCRIPTION": "Blobscillator — organic morphing edition. Blobs flow and merge naturally using smooth-min field accumulation and temporal smoothing. Two-pass: blob field + gaussian blur composite.",
  "ISFVSN": "2",
  "PASSES": [
    {
      "TARGET": "blobBuffer",
      "PERSISTENT": false
    },
    {}
  ],
  "INPUTS": [
    {
      "NAME": "seed1",
      "TYPE": "float",
      "DEFAULT": 233,
      "MIN": 89,
      "MAX": 1597,
      "LABEL": "Seed 1"
    },
    {
      "NAME": "seed2",
      "TYPE": "float",
      "DEFAULT": 13,
      "MIN": 5,
      "MAX": 55,
      "LABEL": "Seed 2"
    },
    {
      "NAME": "blurAmount",
      "LABEL": "Blur Amount",
      "TYPE": "float",
      "DEFAULT": 3,
      "MIN": 0,
      "MAX": 20
    },
    {
      "NAME": "blendSoftness",
      "LABEL": "Blend Softness",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "scale",
      "TYPE": "float",
      "DEFAULT": 3.5,
      "MIN": 0,
      "MAX": 10,
      "LABEL": "Scale",
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "loops",
      "TYPE": "float",
      "DEFAULT": 12,
      "MIN": 1,
      "MAX": 40,
      "LABEL": "Loops",
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "blobRadius",
      "LABEL": "Blob Radius",
      "TYPE": "float",
      "DEFAULT": 1.1,
      "MIN": 0.1,
      "MAX": 3,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "mergeSmoothness",
      "LABEL": "Merge Smoothness",
      "TYPE": "float",
      "DEFAULT": 0.85,
      "MIN": 0.05,
      "MAX": 2.5,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "rate",
      "TYPE": "float",
      "DEFAULT": 0.035,
      "MIN": 0,
      "MAX": 1,
      "LABEL": "Rate",
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "freq1",
      "TYPE": "float",
      "DEFAULT": 0.95,
      "MIN": 0.005,
      "MAX": 1,
      "LABEL": "Frequency 1",
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "freq2",
      "TYPE": "float",
      "DEFAULT": 2.8,
      "MIN": 0.5,
      "MAX": 10,
      "LABEL": "Frequency 2",
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "orbitSwirl",
      "LABEL": "Orbit Swirl",
      "TYPE": "float",
      "MIN": -2,
      "MAX": 2,
      "DEFAULT": 0,
      "GROUP": "Motion"
    },
    {
      "NAME": "radiusPulse",
      "LABEL": "Radius Pulse",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.6,
      "DEFAULT": 0,
      "GROUP": "Motion"
    },
    {
      "NAME": "pathDiversity",
      "LABEL": "Path Diversity",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Motion"
    },
    {
      "NAME": "colorShift",
      "LABEL": "Color Shift",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "colorBoost",
      "LABEL": "Color Boost",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Color"
    },
    {
      "NAME": "center",
      "TYPE": "point2D",
      "DEFAULT": [
        0,
        0
      ],
      "MAX": [
        1,
        1
      ],
      "MIN": [
        -1,
        -1
      ],
      "LABEL": "Center",
      "GROUP": "Camera / Layout"
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
      "DEFAULT": 0.35,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

////////////////////////////////////////////////////////////
// Blobscillator — organic morphing edition
//
// Key changes vs. previous version:
//  • Replaced sinusoidal distance accumulation with a true
//    smooth-minimum (smin) metaball field so blobs MERGE
//    organically rather than ripple/jitter.
//  • Each blob centre moves on its own slow Lissajous path
//    driven by distinct low-frequency sinusoids — no hashing
//    noise, which caused jitter.
//  • blobRadius and mergeSmoothness are independent controls.
//  • Audio nudges radii gently; no abrupt per-frame jumps.
//  • Pass 1 unchanged in structure but uses updated field.
////////////////////////////////////////////////////////////

// ── Smooth minimum (polynomial, Inigo Quilez) ──────────────
float smin(float a, float b, float k) {
	float h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0);
	return mix(b, a, h) - k * h * (1.0 - h);
}

// ── Pseudo-random scalar from float ───────────────────────
float hashF(float n) {
	return fract(sin(n * 127.1 + 311.7) * 43758.5453);
}

// ── Gaussian blur (9×9 radial) ────────────────────────────
vec4 gaussianBlur(vec2 fragCoord, float radius) {
	vec2 pixelSize = 1.0 / RENDERSIZE;
	vec4 result    = vec4(0.0);
	float total    = 0.0;
	float sigma    = max(radius * 0.4, 0.001);

	for (int xi = -4; xi <= 4; xi++) {
		for (int yi = -4; yi <= 4; yi++) {
			float fx   = float(xi);
			float fy   = float(yi);
			float d2   = fx * fx + fy * fy;
			float w    = exp(-d2 / (2.0 * sigma * sigma));
			vec2  off  = vec2(fx, fy) * pixelSize * radius;
			result    += IMG_PIXEL(blobBuffer, fragCoord + off * RENDERSIZE) * w;
			total     += w;
		}
	}
	return result / total;
}

void main() {

	// ── Shared audio shaping ───────────────────────────────
	float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.6);
	float midP  = smoothstep(0.08, 0.90, audioMid);
	float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);

	if (PASSINDEX == 0) {
		// ── Pass 0: metaball field ─────────────────────────

		vec2 uv = (2.0 * gl_FragCoord.xy - RENDERSIZE.xy) / RENDERSIZE.y;
		uv -= center.xy;

		float zoomA = 1.0 - audioReact * 0.18 * bassP;
		uv *= (10.5 - scale) * zoomA;

		// Very slow master time. Music advances it: audioBassTime integrates
		// the smoothed bass level, so the blobs literally drift in proportion
		// to the low end (frozen in silence — exact current look preserved).
		float T = TIME * rate * 0.25 + audioBassTime * 0.45;

		// Number of blobs: clamp loops to integer count
		float nBlobs = clamp(loops, 1.0, 40.0);

		// Audio-reactive radius modulation — LINEAR followers (no knees, no
		// dilution by the 0.35 audioReact default), plus the original
		// audioReact-scaled extra depth on top. Silence → exactly 1.0.
		float radAudio = 1.0 + 0.50 * audioBass + 0.20 * audioMid
		                     + audioReact * 0.20 * bassP
		                     + audioReact * 0.08 * midP;
		float R = blobRadius * radAudio;

		// Accumulate metaball field via smin
		// Field value = smooth union of (dist - R) for each blob.
		// Negative inside blob, positive outside.
		float field = 9999.0;

		float k = mergeSmoothness;

		for (float i = 0.0; i < 40.0; i++) {
			if (i >= nBlobs) break;

			// Each blob gets unique phase offsets via hashF
			float ph1 = hashF(i * 1.0)       * 6.2832;
			float ph2 = hashF(i * 1.0 + 0.5) * 6.2832;
			float ph3 = hashF(i * 1.0 + 1.3) * 6.2832;
			float ph4 = hashF(i * 1.0 + 2.7) * 6.2832;

			// Lissajous-style centre: sum of two slow sinusoids
			// per axis → smooth, organic, non-jittery path
			// Path Diversity: 1 = original per-blob frequency spread, 0 = all blobs share one tempo
			float pd  = pathDiversity;
			float fx1 = freq1 * (1.00 + (hashF(i + 10.0) - 0.5) * 0.6 * pd);
			float fx2 = freq1 * (0.50 + (hashF(i + 20.0) - 0.5) * 0.4 * pd);
			float fy1 = freq1 * (0.85 + (hashF(i + 30.0) - 0.5) * 0.7 * pd);
			float fy2 = freq1 * (0.65 + (hashF(i + 40.0) - 0.5) * 0.5 * pd);

			float spread = freq2 * (1.0 + audioReact * 0.15 * midP);
			float cx = (sin(T * fx1 + ph1) * 0.6 + sin(T * fx2 + ph2) * 0.4) * spread;
			float cy = (sin(T * fy1 + ph3) * 0.6 + sin(T * fy2 + ph4) * 0.4) * spread;

			// Orbit Swirl: the whole blob constellation revolves about the centre
			float sw = orbitSwirl * TIME * 0.12;
			float swc = cos(sw), sws = sin(sw);
			vec2 cc = vec2(cx * swc - cy * sws, cx * sws + cy * swc);
			// Radius Pulse: each blob throbs on its own phase
			float Ri = R * (1.0 + radiusPulse * sin(TIME * 0.9 + i * 1.7));
			float d = length(uv - cc) - Ri;
			field   = smin(field, d, k);
		}

		// Map signed field to 0..1 for storage
		// field < 0 → inside blobs, field > 0 → outside
		// Normalise with a scale that keeps the transition visible
		float mapped = field * 0.18 + 0.5;
		mapped = clamp(mapped, 0.0, 1.0);

		gl_FragColor = vec4(vec3(mapped), 1.0);

	} else {
		// ── Pass 1: blur + composite + colouring ──────────

		vec2 fragCoord = gl_FragCoord.xy;

		// Sample original and blurred
		vec4 sharp = IMG_PIXEL(blobBuffer, fragCoord);

		float dynBlur = blurAmount * (1.0 + audioReact * 0.25 * bassP);
		vec4 blurred  = gaussianBlur(fragCoord, dynBlur);

		vec4 blended = mix(sharp, blurred, blendSoftness);

		// Recover signed field value
		float d = blended.r * 2.0 - 1.0;

		// Smooth threshold: inside blobs = 1, outside = 0
		// blendSoftness widens the transition band
		float band    = 0.08 + blendSoftness * 0.18;
		float shape   = smoothstep(band, -band, d);

		// Inner glow / core layers
		float glow    = smoothstep(0.25, 0.75, shape);
		float core    = smoothstep(0.44, 0.56, shape);

		// Color shift: rotate hue of the palette
		float cs      = colorShift;
		vec3 darkBg   = vec3(0.02, 0.02, 0.06);
		// User background: blend the void behind the blobs toward the chosen color.
		darkBg        = mix(darkBg, bgColor.rgb, bgColor.a);

		// Mid blob colour — hue-rotate via simple RGB rotation
		vec3 midBase  = vec3(0.05, 0.22, 0.60);
		vec3 midShift = vec3(
			midBase.r * cos(cs * 6.2832) - midBase.g * sin(cs * 6.2832),
			midBase.r * sin(cs * 6.2832) + midBase.g * cos(cs * 6.2832),
			midBase.b
		);
		midShift = clamp(midShift + vec3(cs * 0.3, cs * 0.1, (1.0 - cs) * 0.3), 0.0, 1.0);

		vec3 coreColor = mix(vec3(0.85, 0.95, 1.0),
		                     vec3(1.0, 0.85, 0.5),
		                     cs);

		vec3 col = mix(darkBg, midShift, glow);
		col      = mix(col, coreColor, core * core);

		// Whole-frame LINEAR follower — the scene is dark (peak ~0.6) so a
		// bass gain can't clip; ambient swells read directly. Silence = 1.0.
		col     *= 1.0 + 0.60 * audioBass + 0.35 * audioMid;

		// Beat pulse on cores
		col += audioReact * 0.18 * bassP * coreColor * core;

		// Subtle high-frequency shimmer along edges
		float edge   = glow * (1.0 - core);
		col         += audioReact * 0.06 * highP * vec3(0.5, 0.8, 1.0) * edge;

		// ---- universal color block (defaults = no-op; hue via native colorShift) ----
		float ucL = dot(col, vec3(0.299, 0.587, 0.114));
		col = mix(vec3(ucL), col, colorBoost);               // saturation

		gl_FragColor = vec4(col, 1.0);
	}
}
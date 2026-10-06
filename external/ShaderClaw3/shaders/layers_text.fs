/*{
  "CATEGORIES": [
    "Generator",
    "Text"
  ],
  "DESCRIPTION": "Text repeated at multiple depth layers with parallax and scale",
  "INPUTS": [
    {
      "NAME": "layerCount",
      "LABEL": "Layer Count",
      "TYPE": "float",
      "MIN": 2,
      "MAX": 6,
      "DEFAULT": 4,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "depthSpread",
      "LABEL": "Depth Spread",
      "TYPE": "float",
      "MIN": 0.1,
      "MAX": 1,
      "DEFAULT": 0.5,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "speed",
      "LABEL": "Speed",
      "TYPE": "float",
      "MIN": 0.1,
      "MAX": 3,
      "DEFAULT": 0.5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "frontColor",
      "LABEL": "Front Color",
      "TYPE": "color",
      "DEFAULT": [
        1,
        1,
        1,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "backColor",
      "LABEL": "Back Color",
      "TYPE": "color",
      "DEFAULT": [
        0.3,
        0.3,
        0.5,
        1
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
      "LABEL": "Message",
      "TYPE": "text",
      "DEFAULT": "ETHEREA",
      "MAX_LENGTH": 12,
      "GROUP": "Text"
    },
    {
      "NAME": "textScale",
      "LABEL": "Text Scale",
      "TYPE": "float",
      "MIN": 0.3,
      "MAX": 2,
      "DEFAULT": 0.8,
      "GROUP": "Text"
    },
    {
      "NAME": "bgColor",
      "LABEL": "Background Color",
      "TYPE": "color",
      "DEFAULT": [
        0.03,
        0.03,
        0.08,
        1
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "transparentBg",
      "LABEL": "Transparent Background",
      "TYPE": "bool",
      "DEFAULT": true,
      "GROUP": "Background"
    }
  ]
}*/

// ---- Font engine ----
// Atlas-based (replaces legacy hardcoded 5x7 packed-bit charData() bitmap
// with a sample from the shared, high-resolution fontAtlasTex — same
// charPixel/sampleChar helper used by the migrated text_*.fs shaders).

float charPixel(int ch, float col, float row) {
	if (ch < 0 || ch > 36) return 0.0;
	vec2 uv = vec2(col / 5.0, row / 7.0);
	if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) return 0.0;
	return smoothstep(0.1, 0.55, texture2D(fontAtlasTex, vec2((float(ch) + uv.x) / 37.0, uv.y)).r);
}

// Accept both atlas indices (0-36, fed by the app) and raw ASCII codes
// (fed by some hosts): map ASCII letters/digits/space onto atlas indices.
int normChar(int c) {
	if (c >= 65 && c <= 90) return c - 65;      // ASCII 'A'-'Z'
	if (c >= 97 && c <= 122) return c - 97;     // ASCII 'a'-'z'
	if (c == 32) return 26;                     // ASCII space
	if (c >= 48 && c <= 57) return c - 48 + 27; // ASCII '0'-'9'
	return c;                                    // already an atlas index
}

int getChar(int slot) {
	int c;
	if (slot == 0) c = int(msg_0); else if (slot == 1) c = int(msg_1);
	else if (slot == 2) c = int(msg_2); else if (slot == 3) c = int(msg_3);
	else if (slot == 4) c = int(msg_4); else if (slot == 5) c = int(msg_5);
	else if (slot == 6) c = int(msg_6); else if (slot == 7) c = int(msg_7);
	else if (slot == 8) c = int(msg_8); else if (slot == 9) c = int(msg_9);
	else if (slot == 10) c = int(msg_10); else c = int(msg_11);
	return normChar(c);
}

int charCount() { int n = int(msg_len); return n > 0 ? n : 1; }

// ---- Main shader ----

void main() {
	vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
	float aspect = RENDERSIZE.x / RENDERSIZE.y;

	int numChars = charCount();

	// Soft-knee audio conditioning (playbook standard snippet).
	float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.6);
	float midP  = pow(smoothstep(0.08, 0.85, audioMid), 1.3);
	float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
	float drive = 0.25 + 0.75 * smoothstep(0.05, 0.9, audioEnergy);
	// Time-warp clock: parallax drifts with the track's energy.
	float musicTime = TIME * (0.7 + 0.6 * drive);
	float textAmt = 0.0;

	// Start with background color
	vec3 col = bgColor.rgb;
	float alpha = transparentBg ? 0.0 : 1.0;

	// Render layers back to front
	for (int L = 0; L < 6; L++) {
		if (float(L) >= layerCount) break;

		// Depth: 0.0 = back, 1.0 = front
		float depth = float(L) / (layerCount - 1.0);

		// Scale increases with depth (front layers are larger; bass swells the front)
		float layerScale = textScale * (0.5 + depth * 0.8) * (1.0 + 0.06 * bassP * depth);

		// Parallax drift: back layers move slower, front layers faster
		// (bass widens the horizontal spread, mids stir the vertical drift)
		float drift = sin(musicTime * speed * (0.3 + depth * 0.7) + depth * 2.0) * 0.1 * depthSpread * (1.0 + 0.3 * bassP);
		float vDrift = cos(musicTime * speed * (0.2 + depth * 0.5) + depth * 3.5) * 0.03 * depthSpread * (1.0 + 0.35 * midP);

		// Color interpolation from backColor to frontColor (highs glint the front layers)
		vec3 layerCol = mix(backColor.rgb, frontColor.rgb, depth) * (1.0 + 0.3 * highP * depth);

		// Opacity: back layers more transparent, front more opaque
		float layerAlpha = 0.3 + depth * 0.7;

		// Character dimensions in UV space
		float charW = 0.09 * layerScale;
		float charH = charW * 1.5;
		float gap = charW * 0.25;

		// Total width of the text string
		float totalW = float(numChars) * charW + float(numChars - 1) * gap;

		// Aspect-corrected position
		vec2 p = vec2((uv.x - 0.5) * aspect + 0.5, uv.y);

		// Apply drift
		p.x -= drift;
		p.y -= vDrift;

		// Center the text block horizontally and vertically
		float startX = 0.5 - totalW * 0.5;
		float startY = 0.5 - charH * 0.5;

		// Check each character
		for (int i = 0; i < 12; i++) {
			if (i >= numChars) break;

			int ch = getChar(i);

			// Skip spaces (space = 26)
			if (ch == 26) continue;

			// Character bounding box
			float cx = startX + float(i) * (charW + gap);
			float cy = startY;

			// Map pixel position into character grid (5 columns x 7 rows)
			float localX = (p.x - cx) / charW;
			float localY = (p.y - cy) / charH;

			// Check if within character bounds
			if (localX >= 0.0 && localX < 1.0 && localY >= 0.0 && localY < 1.0) {
				float gridCol = localX * 5.0;
				float gridRow = localY * 7.0;

				float filled = charPixel(ch, gridCol, gridRow);

				// Alpha-blend this layer onto the result
				col = mix(col, layerCol, filled * layerAlpha);
				alpha = mix(alpha, 1.0, filled * layerAlpha);
				textAmt = max(textAmt, filled * layerAlpha);
			}
		}
	}

	// Beat accent: brief glint on the glyphs, easing back.
	float kick = audioBeatPulse * audioBeatPulse;
	col += frontColor.rgb * textAmt * kick * 0.25;

	// ---- universal color block (defaults = no-op) ----
	float ucL = dot(col, vec3(0.299, 0.587, 0.114));
	col = mix(vec3(ucL), col, colorBoost);
	if (hueShift > 0.0005) {
	    float hA = hueShift * 6.2831853;
	    float hC = cos(hA), hS = sin(hA);
	    mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
	            + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
	            + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
	    col = clamp(hM * col, 0.0, 1.0);
	}
	gl_FragColor = vec4(col, alpha);
}

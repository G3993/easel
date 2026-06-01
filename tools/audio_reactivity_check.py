#!/usr/bin/env python3
"""
audio_reactivity_check.py — Easel shader Audio-Reactivity conformance (static pass).

The lightweight half of the rubric in .planning/audio-reactive-system.md: it reads
each shader's manifest comment (`// reacts: dim, dim, ...`) and verifies the shader
actually references at least one Audio Feature Bus uniform for every declared
dimension. (The full render-to-FBO scorecard is the heavier future half.)

Usage:
  python3 tools/audio_reactivity_check.py [shader_dir]
  exit 0 = all declared shaders conform; 1 = a declared dimension is unbacked.
"""
import os, re, sys

SHADER_DIR = sys.argv[1] if len(sys.argv) > 1 else os.path.expanduser("~/ShaderClaw3/shaders")

# dimension -> the bus symbols that satisfy it (uniforms or injected helpers)
DIM = {
    "synesthesia": ["audioPalette", "audioChroma", "audioDominantPitch", "audioBrightness"],
    "palette":     ["audioPalette", "audioPalShadow", "audioPalMid", "audioPalHigh", "audioPalAccent", "audioPalTemp", "audioPalSat"],
    "movement":    ["audioBeatPhase", "audioTempo01", "audioOnsetRate", "audioFlux", "audioBPM", "audioBeatPulse", "audioKick"],
    "flow":        ["audioFlow"],
    "noise":       ["audioFlatness", "audioZCR"],
    "grain":       ["audioOnset", "audioHit", "audioPunch", "audioBeat"],
    "texture":     ["audioTexture"],
    "energy":      ["audioLevel", "audioEnergy", "audioArousal", "audioBreath"],
    "melancholy":  ["audioValence", "audioArousal", "audioMajorMinor", "audioWarmth"],
    "mood":        ["audioMood", "audioValence", "audioArousal", "audioWarmth", "audioTilt"],
    "vibe":        ["audioMood", "audioWarmth", "audioTilt", "audioBrightness"],
    "charm":       ["audioCharm"],
    "softness":    ["audioSoftness"],
    "layering":    ["audioPresence", "audioLayers", "audioDensity"],
    "assembly":    ["audioNovelty", "audioSectionPhase", "audioSectionAge", "audioLayers"],
    "build-up":    ["audioBuildup", "audioBuildupRate", "audioEnergyAcc", "audioDrop", "audioKick"],
}

def main():
    files = sorted(f for f in os.listdir(SHADER_DIR) if f.endswith(".fs"))
    declared, passed, failed = 0, 0, 0
    problems = []
    for fn in files:
        src = open(os.path.join(SHADER_DIR, fn), encoding="utf-8", errors="ignore").read()
        m = re.search(r"//\s*reacts:\s*(.+)", src)
        if not m:
            continue  # shader hasn't opted into the rubric
        declared += 1
        dims = [d.strip().lower() for d in re.split(r"[,/]", m.group(1)) if d.strip()]
        missing = []
        for d in dims:
            syms = DIM.get(d)
            if syms is None:
                missing.append(f"{d}(unknown-dim)")
            elif not any(re.search(r"\b" + re.escape(s) + r"\b", src) for s in syms):
                missing.append(d)
        emph = re.search(r"//\s*emphasis:\s*(\w[\w-]*)", src)
        tag = f"  [emphasis: {emph.group(1)}]" if emph else ""
        if missing:
            failed += 1
            problems.append(f"  ✗ {fn}: declares {dims} but no uniform backs: {missing}")
        else:
            passed += 1
            print(f"  ✓ {fn}: {dims}{tag}")
    print(f"\n{passed}/{declared} declared shaders conform "
          f"({len(files)} total, {len(files)-declared} not yet on the rubric).")
    if problems:
        print("\nNON-CONFORMING:")
        print("\n".join(problems))
    return 1 if failed else 0

if __name__ == "__main__":
    sys.exit(main())

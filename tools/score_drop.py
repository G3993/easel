#!/usr/bin/env python3
"""
score_drop.py — score a rendered ISF drop against its reference image.

Two parts:
  (1) cheap numeric similarity — image space metrics (LAB hist EMD, SSIM,
      luminance delta). Cheap, deterministic. Runs immediately.
  (2) vision-LLM rubric pass — emitted as `score.prompt.json` for the
      orchestrating agent to consume (the agent does the vision pass and
      writes `score.rubric.json` next to it).

This script ASSEMBLES the final `score.json` from whichever parts exist:
  - numeric portion (always)
  - rubric portion (if `score.rubric.json` is present alongside)
If rubric is missing the final write has `numeric_only: true`.

Deps: Pillow, numpy, scikit-image. Lives in /Users/lu/easel/.venv-eval.

CLI:
    score_drop.py --slug <slug> --rendered <png> --reference <jpg>
                  --shader <fs> --out <score.json>
"""

from __future__ import annotations
import argparse
import json
import sys
from pathlib import Path

import numpy as np
from PIL import Image
from skimage import color
from skimage.metrics import structural_similarity as ssim


def resize_to_aspect(rendered_path: Path, reference_path: Path) -> tuple[Image.Image, Image.Image]:
    """Resize rendered to reference's pixel size for fair comparison."""
    ref = Image.open(reference_path).convert("RGB")
    ren = Image.open(rendered_path).convert("RGB")
    # Normalize to reference dimensions.
    ren_resized = ren.resize(ref.size, Image.LANCZOS)
    return ren_resized, ref


def lab_histogram_emd(a: Image.Image, b: Image.Image, bins: int = 32) -> dict:
    """Earth-mover distance on per-channel LAB histograms.
    Cumulative L1 distance per axis."""
    a_lab = color.rgb2lab(np.asarray(a) / 255.0)
    b_lab = color.rgb2lab(np.asarray(b) / 255.0)
    ranges = {
        "L": (0.0, 100.0),
        "a": (-128.0, 127.0),
        "b": (-128.0, 127.0),
    }
    out = {}
    for i, ch in enumerate(["L", "a", "b"]):
        lo, hi = ranges[ch]
        ha, _ = np.histogram(a_lab[..., i], bins=bins, range=(lo, hi), density=True)
        hb, _ = np.histogram(b_lab[..., i], bins=bins, range=(lo, hi), density=True)
        # normalize to sum=1
        ha = ha / max(ha.sum(), 1e-9)
        hb = hb / max(hb.sum(), 1e-9)
        ca = np.cumsum(ha)
        cb = np.cumsum(hb)
        out[ch] = float(np.abs(ca - cb).sum() / bins)  # 0..1 normalized-ish
    out["total"] = float((out["L"] + out["a"] + out["b"]) / 3.0)
    return out


def compute_ssim(a: Image.Image, b: Image.Image) -> float:
    a_arr = np.asarray(a.convert("L"), dtype=np.float32) / 255.0
    b_arr = np.asarray(b.convert("L"), dtype=np.float32) / 255.0
    val = ssim(a_arr, b_arr, data_range=1.0)
    return float(val)


def compute_lum_delta(a: Image.Image, b: Image.Image) -> float:
    a_lab = color.rgb2lab(np.asarray(a) / 255.0)
    b_lab = color.rgb2lab(np.asarray(b) / 255.0)
    return float(abs(a_lab[..., 0].mean() - b_lab[..., 0].mean()) / 100.0)


def numeric_scores(rendered: Path, reference: Path) -> dict:
    a, b = resize_to_aspect(rendered, reference)
    emd = lab_histogram_emd(a, b)
    s = compute_ssim(a, b)
    lum_d = compute_lum_delta(a, b)
    return {
        "histogram_emd": emd,
        "ssim": s,
        "l_mean_delta": lum_d,
    }


def _read_text_legibility(out_dir: Path) -> dict | None:
    """Read the text-legibility gate result if present. Returns None when
    the gate didn't run (no `msg` text input), so the caller can treat
    legibility as not-applicable."""
    p = out_dir / "check_text_legible.json"
    if not p.exists():
        return None
    try:
        data = json.loads(p.read_text())
    except Exception:
        return None
    # Pending stub (vision agent hasn't filled it in yet) — treat as None.
    if data.get("text_legible") is None or data.get("verdict") == "PENDING":
        return None
    return data


def _apply_text_legibility_cap(record: dict, leg: dict | None,
                               cap_total: int = 10) -> None:
    """If text_legible == false, cap total at cap_total/25. Mirrors the
    binding-floor cap. Records the cap source in the rationale so users
    know why the score is held back."""
    record["text_legibility"] = leg  # always recorded (None if N/A)
    if leg is None:
        return
    if not leg.get("text_legible", True):
        # Hard gate: cap whatever total the rubric proposed.
        original = record.get("total")
        if original is not None and original > cap_total:
            record["total"] = cap_total
            note = (f"[text-legibility cap] verdict="
                    f"{leg.get('verdict', '?')}; "
                    f"reason={leg.get('reason', '')}; "
                    f"capped from {original}/25 to {cap_total}/25.")
            existing = record.get("rationale", "") or ""
            record["rationale"] = (existing + "\n" + note).strip()


def assemble(slug: str, rendered: Path, reference: Path, shader: Path,
             out_path: Path) -> dict:
    rubric_path = out_path.parent / "score.rubric.json"
    numeric = numeric_scores(rendered, reference)
    record = {
        "slug": slug,
        "rendered": str(rendered),
        "reference": str(reference),
        "shader": str(shader),
        "numeric": numeric,
        "axes": None,
        "total": None,
        "anti_patterns": {},
        "rationale": "",
        "numeric_only": True,
        "text_legibility": None,
    }
    if rubric_path.exists():
        try:
            rubric = json.loads(rubric_path.read_text())
            record["axes"] = rubric.get("axes")
            record["total"] = rubric.get("total")
            record["anti_patterns"] = rubric.get("anti_patterns", {})
            record["rationale"] = rubric.get("rationale", "")
            record["numeric_only"] = False
        except Exception as e:
            record["rubric_error"] = str(e)
    # Text-legibility gate (post-rubric so the cap visibly overrides the
    # rubric total in the merged record).
    leg = _read_text_legibility(out_path.parent)
    _apply_text_legibility_cap(record, leg, cap_total=10)
    return record


def write_prompt_json(slug: str, rendered: Path, reference: Path, shader: Path,
                      out_dir: Path):
    rubric_path = Path("/Users/lu/easel/.planning/auto-improve/RUBRIC.md")
    prompt = {
        "slug": slug,
        "task": "score this rendered ISF shader against the reference per RUBRIC.md (5 axes 0-5, total /25, anti-pattern checklist).",
        "inputs": {
            "rendered_png": str(rendered),
            "reference_image": str(reference),
            "shader_source": str(shader),
            "rubric": str(rubric_path),
        },
        "output_to_write": str(out_dir / "score.rubric.json"),
        "schema_expected": {
            "axes": {"a": "int 0-5", "b": "int 0-5", "c": "int 0-5",
                     "d": "int 0-5", "e": "int 0-5"},
            "total": "int 0-25",
            "anti_patterns": {
                "literal_soccer_scoreboard_pitch": "bool",
                "sound_wave_ekg": "bool",
                "spectrum_analyzer_bars": "bool",
                "default_checkerboard_sdf_grid": "bool",
                "single_color_noise_plane": "bool",
                "mirror_symmetric_horizon": "bool",
                "logo_or_readable_text_central": "bool"
            },
            "rationale": "free text, ~3-6 lines explaining the per-axis scores and which anti-patterns (if any) fired"
        },
        "binding_floor_rule": "if shader has zero player[*].*/cue.*/data.* binds, cap total at 10/25. audio.*-only passes the floor but caps axis (a) at 1.",
        "anti_pattern_rule": "any anti-pattern auto-fails axis (d) to 0 and caps total at 8/25.",
        "text_legibility_rule": (
            "if check_text_legible.json reports text_legible=false, "
            "the assembler will cap total at 10/25. The rubric pass "
            "should still score the visual axes honestly; the cap is "
            "applied post-hoc and the cap source is appended to the "
            "rationale."),
        "text_legibility_input": str(out_dir / "check_text_legible.json"),
    }
    (out_dir / "score.prompt.json").write_text(json.dumps(prompt, indent=2))


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--slug", required=True)
    ap.add_argument("--rendered", required=True, type=Path)
    ap.add_argument("--reference", required=True, type=Path)
    ap.add_argument("--shader", required=True, type=Path)
    ap.add_argument("--out", required=True, type=Path)
    args = ap.parse_args()

    if not args.rendered.exists():
        print(f"error: rendered missing: {args.rendered}", file=sys.stderr); return 2
    if not args.reference.exists():
        print(f"error: reference missing: {args.reference}", file=sys.stderr); return 2
    if not args.shader.exists():
        print(f"error: shader missing: {args.shader}", file=sys.stderr); return 2

    args.out.parent.mkdir(parents=True, exist_ok=True)
    write_prompt_json(args.slug, args.rendered, args.reference, args.shader,
                      args.out.parent)
    record = assemble(args.slug, args.rendered, args.reference, args.shader,
                      args.out)
    args.out.write_text(json.dumps(record, indent=2))
    print(f"ok: wrote {args.out} (numeric_only={record['numeric_only']})")
    return 0


if __name__ == "__main__":
    sys.exit(main())

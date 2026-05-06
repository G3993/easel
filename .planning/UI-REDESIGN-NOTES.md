# UI Redesign — Skipped Phase Notes

## Phase 5 — Floating transport pill (skipped)

**Date:** 2026-05-06
**Why skipped:** The transport buttons (play/pause/stop/loop + timecode +
zoom + audio combo + meter + REC + GO LIVE) are interleaved inside
`Application::renderTimelinePanel` alongside the docked timeline's
tracks, ruler, and audio waveform. Extracting just the transport into a
standalone floating window is straightforward; the breaking criterion
is **"No bottom dock bar"** — that cannot be satisfied without also
moving or restyling the tracks/audio lane, which is Phase 7's scope.

**What was tried:**
- Located transport rendering at `Application.cpp:4538..5300+` inside
  `renderTimelinePanel`.
- Considered adding a floating `##TransportPill` window layered on top
  while keeping the docked timeline; this would visually duplicate the
  transport without satisfying the "no dock bar" criterion.
- Considered hiding the docked Timeline panel entirely; that would
  break track display and audio meter (no replacement until Phase 7).

**Recommendation:** complete Phase 7 (Timeline redesign) first, which
floats the entire timeline panel. Phase 5 then collapses to "split out
the transport row into a separate top pill above the new timeline
panel" — a much smaller change.

**Final screenshot path:** N/A (no code changes shipped)

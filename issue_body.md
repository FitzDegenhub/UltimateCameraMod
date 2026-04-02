## Problem

The HUD X-axis and Y-axis centering features (which modify `ui/minimaphudview2.html`, `ui/statusgaugeview2.html`, and `ui/gamecommon.css` in PAZ archive `0012`) now trigger a **Coherent Gameface publisher watermark** overlay in-game after a recent Crimson Desert update.

The watermark displays "WaterMarkTitle", "CompanyName", "UserName", "WARNING" across the screen and prompts for a Publisher ID. This occurs regardless of what max-width value is set.

## Root Cause

Crimson Desert uses **Coherent Gameface** (formerly Coherent GT) as its UI rendering engine. It appears a recent game update added integrity checks on the UI HTML/CSS files in the `0012` PAZ archive. Any modification to these files — even semantically identical changes — causes the Gameface engine to fall back to trial/debug mode, which renders the publisher watermark overlay.

## What We've Tried

- Modifying only the CSS `max-width` property (minimal change)
- Different max-width values including the vanilla default (2520px)
- Compacted vs uncompacted text payloads
- Matching the reference Centered HUD mod's approach exactly (sourced from Discord)

All produce the same watermark. The camera XML modifications in archive `0010` are unaffected — only `0012` (UI files) triggers this.

## Current Status

HUD X-axis and Y-axis controls have been **disabled in the UI** (v2.2) with a note explaining the issue. The underlying code is still present and functional — it just needs the integrity check issue resolved.

## Possible Solutions

1. **Identify and replicate the integrity check** — If Coherent Gameface validates files via a hash/checksum stored elsewhere in the archives, we could update that checksum after patching.
2. **Hook the Coherent Gameface license check** — A DLL-based approach could bypass the trial mode detection at runtime.
3. **Alternative HUD approach** — Instead of modifying HTML/CSS directly, inject CSS overrides through a different mechanism (e.g., a custom Gameface plugin or runtime style injection).
4. **Wait for community research** — Other modders working with Coherent Gameface in Crimson Desert may discover a workaround.

## References

- Centered HUD mod (shared via Discord) — same approach, likely affected by the same issue
- Archive: `0012/0.paz`
- Affected files: `ui/minimaphudview2.html`, `ui/statusgaugeview2.html`, `ui/gamecommon.css`

Any help or leads would be appreciated!

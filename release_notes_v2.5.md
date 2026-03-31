## v2.5 -- Critical Fix for European Locale Users

### The Bug

v2.4 (and all previous versions) had a critical bug affecting users with European Windows locale settings (Germany, France, Netherlands, Spain, etc.). When the mod converted camera values to XML, it used the system's regional number format. European locales use **commas** as decimal separators instead of periods:

- US/UK: `RightOffset="0.90"` (correct)
- European: `RightOffset="0,90"` (broken -- game reads as 0)

The game's XML parser expects periods. Any value with a comma was read as 0 or truncated, effectively **centering the camera** or **zeroing out height, blend times, zoom distances, and smoothing values**.

### Impact

This affected **every formatted number** in the mod:
- Horizontal shift values appearing centered
- Height/vertical offset not applying
- Zoom distances being wrong
- Blend times and smoothing values being zeroed
- Follow rates being zeroed

If you're a European user and the mod seemed broken or inconsistent, this was why.

### The Fix

v2.5 forces `InvariantCulture` (period decimal separator) for all number formatting regardless of Windows locale. One line fix that resolves the issue for all affected users globally.

### Also Includes

- Fixed game crash when using Advanced Editor (insufficient room for injected elements)
- All v2.4 features: proportional horizontal shift, horse camera overhaul, mount/aim consistency, version-aware backups, UI enhancements

---

**SHA-256:** `091BDB6456DF85B25CE80A90D26710AE1A7F55EDF189F8921CBAFB153262074A`

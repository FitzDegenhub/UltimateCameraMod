## v2.4 -- Proportional Horizontal Shift, Horse Camera Overhaul, UI Enhancements

### Horizontal Shift Overhaul
- **Proportional scaling formula** -- replaces v2.3's flat delta. Character holds consistent screen position across all zoom levels instead of drifting toward center at ZL3/ZL4.
- **Applies to all mounts** -- horse (8 states), elephant, wyvern, canoe, warmachine (normal/aim/dash), broom. Each uses its own vanilla RightOffset baseline.
- **Applies to all aim/interaction sections** -- lantern, blinding flash, bow, weapon aim/zoom, ride aim, CTRL focus/interact, glide aim, freefall aim, throw aim, boss aim. Camera no longer snaps when activating abilities.
- **Lantern aim baselines matched per zoom level** -- ZL2=0.5, ZL3=0.8, ZL4=1.1, matching the normal camera. Eliminates horizontal snap on lantern activation.

### Horse Camera
- **All 8 horse states fully normalized** -- uniform FoV (40), follow rates, blend times, damping, and velocity offset. Eliminates jolts during idle/run/sprint/dash transitions.
- **Horse zoom levels ZL0-ZL3 explicitly defined** -- vanilla only had ZL2/ZL3, leaving ZL0/ZL1 as centered engine defaults. Now all four have proper offsets.
- **Horse zoom distance scales with Custom distance slider** -- no more "too close or too far" on horse.
- **Fixed phantom zoom level injection** -- v2.3 was creating extra zoom levels that don't exist in vanilla, giving users "extra zoom" even with Extra Zoom unchecked.
- **RightOffset flattened** -- dash/dash_att no longer pull camera inward during sprints.

### Version-Aware Backup System
- **Auto-cleanup on startup** -- if backup version doesn't match current app version, stale data is deleted automatically. Upgrading UCM always starts clean.

### UI Enhancements
- **Game folder path moved to header** with folder icon button to open in Explorer.
- **FoV preview is now distance-aware** -- camera-to-player gap and horizontal offset update in real time.
- **FoV cone enlarged** for better visual representation at all FoV settings.
- **Window is now resizable** -- size persists between sessions.
- **Horizontal shift tooltip and hint text** explain that 0 = vanilla (char left), 0.5 = center.
- **Centered camera tooltip clarified**.

### Troubleshooting
If the mod appears to have no effect, try a clean reinstall:
1. Close the game and mod tool
2. Delete your entire old UCM folder (backups, last_install.json, everything)
3. Steam: right-click Crimson Desert > Properties > Installed Files > Verify integrity
4. Download v2.4 fresh, extract to a new folder and run

---

**SHA-256:** `0FFB077991395D35C3E002FF4C303993CFAE5094079FCE37870B5DB014076A71`
**VirusTotal:** [Scan Results](https://www.virustotal.com/gui/file-analysis/M2NmMTdmZWE2ZTkzZWMzODFhZjg3OTk3NjNkMjIxZjE6MTc3NDk3NTE5OA==)

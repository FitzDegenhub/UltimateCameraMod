## v2.3 -- Horizontal Shift Fix + Quality of Life

### Bug Fixes
- **Fixed horizontal shift not working for most users** -- v2.2 used flat absolute RightOffset values that fought against the game's escalating per-zoom-level offsets (ZL2=0.5, ZL3=0.8, ZL4=1.1). At slider -0.7, ZL3 and ZL4 were actually moving *toward* center. Now uses proportional scaling so the character holds screen position across all zoom levels.
- **Slider is now delta-based** -- 0 = vanilla position (not centered). Negative = character left, positive = character right.
- **Fixed false "Centered" detection** -- the status banner incorrectly showed "Centered" on vanilla and non-centered installs due to an overly broad regex.
- **Fixed slider labels not populating on startup** -- Distance, Height, and Horizontal Shift labels now display saved values when reopening the app.

### Improvements
- **Horizontal shift range expanded** from -1..1 to -3..3 for better visibility on 16:9 displays.
- **Status banner overhaul** -- now shows your full install config (FoV, Distance, Height, Shift, all global settings) read from saved state, updates immediately after every install.
- **Auto-switch to Custom tab** on startup if your last install was custom.
- **Updated tooltip** for horizontal shift: "0 = vanilla position. Negative = character left, positive = character right."

### Troubleshooting
If the mod appears to have no effect, try a clean reinstall:
1. Close the game and mod tool
2. Delete the `backups` folder next to `UltimateCameraMod.exe`
3. Delete `last_install.json` if it exists
4. Steam: right-click Crimson Desert > Properties > Installed Files > Verify integrity
5. Reopen the mod tool and click Install

---

**SHA-256:** `6F2258BDF7295BB99784D91BE3C650F4DEA12736C0B84504DB8B2DD4DD490EE2`
**VirusTotal:** [Scan Results](https://www.virustotal.com/gui/file-analysis/ZWJkODAyNzY3ZmQ5ZjA5OTY0OWVlZDg3OWUzZjI2OGM6MTc3NDk2MTE5Mg==)

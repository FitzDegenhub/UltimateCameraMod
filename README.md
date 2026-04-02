> **Nexus Mods:** New-author source review is in progress; the listing may be temporarily unavailable. **v2.5** downloads remain on **[GitHub Releases](https://github.com/FitzDegenhub/UltimateCameraMod/releases/latest)**. **v3** is under active development on branch **`v3-dev`** (export-first / Mod Manager workflow).

# Ultimate Camera Mod — Crimson Desert

Standalone camera toolkit for Crimson Desert with a full GUI, live camera preview, advanced XML editor, and HUD centering for ultrawide displays.

## v3 development (`v3-dev` branch)

**v3** targets an **export-first** flow: tune the camera in-app, **export JSON**, and activate it in **Crimson Desert Mod Manager**. The classic **v2.x** app on `main` still supports direct install into the game PAZ and ships from **Releases**.

| Area | What landed |
|------|-------------|
| **Presets** | File-backed catalog (`ucm_presets/`, `my_presets/`), migration from legacy `presets/`, sidebar manager, lock / import / export, unified session XML loading. |
| **Vanilla preset** | True stock camera: embedded `session_xml` is **raw** decoded `playercamerapreset` from your game backup / live PAZ — **no** `BuildModifications` layer. Quick sliders are filled from **`Player_Basic_Default/ZoomLevel[2]`** (`ZoomDistance`, `UpOffset`, `RightOffset`) via `CameraMod.TryParseUcmQuickFootBaselineFromXml`, so JSON `settings` match the XML. `vanilla_preset_rev` forces regeneration when bumped. **Steadycam** defaults **off** on Vanilla; turn it on if you want UCM smoothing on top of stock. |
| **Windows taskbar icon** | `SetCurrentProcessExplicitAppUserModelID` plus **`SHGetPropertyStoreForWindow`**: `System.AppUserModel.ID` and **`RelaunchIconResource`** (with `ucm.ico` next to the exe and under `Assets/`). Complements `WM_SETICON` / class-icon retries for title bar + shell. |
| **Preset JSON** | `JavaScriptEncoder.UnsafeRelaxedJsonEscaping` for preset files; header string extraction decodes JSON escapes (fixes garbled `+` / `\u002B` in descriptions). |
| **Game patch awareness** | `GameInstallBaselineTracker` saves install metadata (incl. Steam `appmanifest` where applicable) after a successful apply; UI can warn when the install may have changed — reinstall / re-export after updates. |

### Build & run v3 (Windows)

Requires [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0). **Close any running `UltimateCameraMod.V3`** before building, or the exe copy step fails (file in use).

```powershell
Stop-Process -Name "UltimateCameraMod.V3" -Force -ErrorAction SilentlyContinue
dotnet build "src/UltimateCameraMod.V3/UltimateCameraMod.V3.csproj" -c Release
Start-Process "src/UltimateCameraMod.V3/bin/Release/net6.0-windows/UltimateCameraMod.V3.exe"
```

[![Download](https://img.shields.io/badge/Download-v2.5-brightgreen?style=for-the-badge&logo=github)](https://github.com/FitzDegenhub/UltimateCameraMod/releases/latest)
[![Nexus Mods](https://img.shields.io/badge/Nexus_Mods-UCM-d98f40?style=for-the-badge&logo=nexusmods&logoColor=white)](https://www.nexusmods.com/crimsondesert/mods/438)
[![Ko-fi](https://img.shields.io/badge/Ko--fi-Support-FF5E5B?style=for-the-badge&logo=kofi&logoColor=white)](https://ko-fi.com/0xfitz)
[![Reddit](https://img.shields.io/badge/Reddit-Discussion-ff4500?style=for-the-badge&logo=reddit&logoColor=white)](https://www.reddit.com/r/CrimsonDesert/comments/1s8vllh/ultimate_camera_mod_ucm_v25_full_camera_toolkit/)
[![VirusTotal](https://img.shields.io/badge/VirusTotal-Clean-blue?style=for-the-badge&logo=virustotal&logoColor=white)](https://www.virustotal.com/gui/file/091bdb6456df85b25ce80a90d26710ae1a7f55edf189f8921cbafb153262074a)
[![License](https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge)](LICENSE)

## Features

### Simple Mode
- **8 Camera Presets** with live preview — Panoramic, Heroic, Vanilla, Close-Up, Low Rider, Knee Cam, Dirt Cam, Survival
- **Custom Camera** — Slider control over distance, height, and horizontal shift (-3 to +3). Horizontal shift uses proportional scaling so the character holds screen position across all zoom levels. 0 = vanilla (character slightly left), 0.5 = centered. Save unlimited named presets, share via import/export codes (`UCM:` strings)
- **Field of View** — Adjustable from vanilla 40° up to 80°, with universal FoV consistency across all camera states (guard, aim, mount, glide, cinematic) to eliminate jarring transitions
- **Centered Camera** — Character dead center, eliminating the left-offset shoulder cam across 150+ camera states
- **Combat Camera** — Three lock-on zoom levels: Default, Wider, Maximum
- **Mount Camera Sync** — Mount cameras match your chosen player camera height
- **Extra Zoom Levels** — Two additional zoom-out levels (ZL5 + ZL6) on foot and mounted. Scroll further out than vanilla allows
- **Horse First Person (Experimental)** — First-person camera while mounted. Scroll all the way in to see through your character's eyes. Works well at walk/trot; may clip during dashes
- **Horse Camera Overhaul** — All 8 horse states fully normalized with identical FoV, blend times, follow rates, and damping. 4 proper zoom levels (ZL0-ZL3) that scale with your Custom distance slider. No more jolts during speed transitions
- **Horizontal Shift on All Mounts** — Horse, elephant, wyvern, canoe, warmachine, and broom all respect your horizontal shift setting with proportional scaling across zoom levels
- **Skill Aiming Side-Consistency** — Lantern, Blinding Flash, Bow, and all aim/zoom/interaction skills respect your horizontal shift using proportional scaling. Camera no longer snaps when activating abilities
- **Steadycam Smoothing** — Completely revamped camera smoothing system. Normalizes blend timing and FOV across all movement states (idle, walk, run, sprint, combat, guard, mount) to eliminate jarring transitions. This is an ongoing community effort — the Advanced Editor lets anyone fine-tune it further
- **HUD Centering** — Adjustable width slider (1200–3840px) to constrain HUD elements for ultrawide. *Currently disabled — a recent game update added integrity checks that trigger a Coherent Gameface watermark. Controls will be re-enabled once a workaround is found.*
- **Update Notifications** — Automatically checks GitHub releases on launch and shows a banner when a new version is available

### Advanced Editor
- **Full XML Editor** — Every player camera parameter exposed in a searchable, filterable DataGrid
- **Vanilla Comparison** — Side-by-side vanilla vs. modified values; modified fields highlighted in gold
- **Grouped by Camera State** — Collapsible sections (Player_Basic_Default, Player_Weapon_Guard, etc.)
- **Save/Load/Delete** named advanced presets
- **Import/Export** advanced configurations as shareable strings (`UCM_ADV:` prefix, distinct from simple presets)
- **Import XML** — Load a `playercamerapreset.xml` file from other mods and merge the values into the editor
- **Expand/Collapse All** — One-click toggle to expand or collapse all section groups
- **Reset to Defaults** — One click to revert all advanced changes

### Quality of Life
- **Auto Game Detection** — Finds Crimson Desert across Steam, Epic Games, and Xbox/Game Pass
- **Automatic Backup** — Creates a vanilla backup before any modification; one-click restore. Version-aware — automatically cleans stale data when upgrading UCM versions
- **Install Config Banner** — Shows your full install config (FoV, distance, height, shift, settings) so you always know what's active
- **Live FoV Preview** — Distance-aware top-down view showing camera position, horizontal shift, and field of view cone in real time
- **Game Folder Shortcut** — Folder icon in the header opens your game directory in Explorer
- **Settings Persistence** — All selections remembered between sessions
- **Resizable Window** — Drag edges to resize; size persists between sessions
- **Portable** — Single `.exe`, no installer required

## How It Works

1. Locates the game's PAZ archive containing `playercamerapreset.xml`
2. Creates a backup of the original file (only once — never overwrites a clean backup)
3. Decrypts the archive entry (ChaCha20 + Jenkins hash key derivation)
4. Decompresses via LZ4
5. Parses and modifies the XML camera parameters based on your selections
6. Re-compresses, re-encrypts, and writes the modified entry back into the archive

HUD modifications follow the same pipeline for `ui/minimaphudview2.html`, `ui/statusgaugeview2.html`, and `ui/gamecommon.css` in archive `0012`.

No DLL injection, no memory hacking, no internet connection required — pure data file modification.

## Building from Source (v2.x)

Requires [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) (or later).

```bash
cd src/UltimateCameraMod
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

The compiled exe will be in `bin/Release/net6.0-windows/win-x64/publish/`.

Or run directly without compiling:

```bash
cd src/UltimateCameraMod
dotnet run
```

For **v3**, use the **Build & run v3** block in the section above (different `.csproj` path).

### Dependencies (NuGet, restored automatically)

- [K4os.Compression.LZ4](https://www.nuget.org/packages/K4os.Compression.LZ4/) — LZ4 block compression/decompression

## Project Structure

```
src/UltimateCameraMod/          # v2.x WPF app (direct PAZ install) + shared library code
├── Controls/
├── Models/
├── Paz/
├── Services/                   # CameraMod, GameDetector, JsonModExporter, GameInstallBaselineTracker, …
├── MainWindow.xaml
├── App.xaml
└── UltimateCameraMod.csproj

src/UltimateCameraMod.V3/       # v3 export-first UI (links shared Models/Services/Paz from above)
├── MainWindow.xaml
├── ShellTaskbarPropertyStore.cs
├── ApplicationIdentity.cs
├── Assets/ucm.ico
└── UltimateCameraMod.V3.csproj
```

## Support

If UCM helps your playthrough, you can tip the author on **[Ko-fi](https://ko-fi.com/0xfitz)**. Thanks.

## Community & Sharing

> **Nobody has perfected Crimson Desert's camera yet — and that's the point.**
>
> The vanilla game has over 150 camera states, each with dozens of parameters controlling distance, FOV, blend timing, damping, offsets, and more. No single developer can tune all of that for every playstyle and display setup. That's why UCM was built with sharing at its core.
>
> Every setting you tweak — whether it's a simple preset or a deep Advanced Editor override — can be exported as a string and shared with other players. If someone in the community figures out the perfect guard-camera transition, or nails the combat FOV for ultrawide, they can share it in seconds and everyone benefits.
>
> **This already works.** The guard-camera zoom snap that plagued the vanilla game was solved by a single user adjusting one FOV value in the Advanced Editor. That kind of community-driven fine-tuning is exactly what this tool is for.

### Preset Formats

UCM uses two string formats so they can't be mixed up:

| Format | Used in | Contains |
|--------|---------|----------|
| `UCM:...` | Custom tab → Export/Import | Distance, height, horizontal shift |
| `UCM_ADV:...` | Advanced editor → Export/Import | Full XML parameter overrides |

Export your config, post it on Nexus/Discord/Reddit, and others can import it in one click. The more people tweak, the better it gets for everyone.

## Compatibility

- **Platforms:** Steam, Epic Games, Xbox / Game Pass
- **OS:** Windows 10/11 (x64)
- **Display:** Any aspect ratio — 16:9, 21:9, 32:9

## FAQ

**Will this get me banned?**
UCM modifies offline data files only. It does not touch game memory, inject code, or interact with running processes. Use at your own discretion in online/multiplayer modes.

**The game updated and my camera is back to vanilla.**
Normal — game updates overwrite modded files. Re-open UCM and click Install. Your settings are saved automatically.

**My antivirus flagged the exe.**
Known false positive with self-contained .NET apps. [VirusTotal scan is clean](https://www.virustotal.com/gui/file/091bdb6456df85b25ce80a90d26710ae1a7f55edf189f8921cbafb153262074a). Full source is available here to review and build yourself.

**What does horizontal shift 0 mean?**
0 = vanilla camera position (character slightly to the left). 0.5 = character centered on screen. Negative values move further left, positive values move further right. Uses proportional scaling so the character holds position across all zoom levels.

**Upgrading from a previous version?**
v2.4 automatically cleans stale data from previous versions on first launch. If you experience any issues, verify game files in Steam and reinstall.

## Version History

- **v3.0-dev** (`v3-dev` branch) — Export-first preset workflow, file-based preset manager, true Vanilla preset from raw game XML + Quick baseline sync, Windows taskbar identity (`RelaunchIconResource`), preset JSON escaping fixes, game install baseline tracker, expanded Export JSON UI, new app icon assets. *Not yet on GitHub Releases — build from source or use CI artifacts when available.*
- **v2.4** — Proportional horizontal shift (fixes drift across zoom levels), horizontal shift on all mounts and all aim/interaction abilities, horse camera overhaul (all 8 states normalized, 4 zoom levels, distance scales with Custom slider), fixed phantom zoom level injection, version-aware backups with auto-cleanup, lantern aim baselines matched per zoom level, FoV preview distance-aware, game path in header with folder shortcut, resizable window with size persistence, improved tooltips.
- **v2.3** — Critical fix for horizontal shift not working on 16:9 displays. Delta-based slider (0 = vanilla). Range expanded to -3..3. Fixed false "Centered" detection, banner shows full install config from saved state.
- **v2.2** — Major feature release. Steadycam toggle, Extra Zoom Levels, Horse First Person, Horizontal Shift slider, universal FoV consistency, skill aiming side-consistency, Import XML in Advanced Editor, preset sharing, update notifications, Expand/Collapse All. HUD centering temporarily disabled.
- **v2.1** — Fixed custom preset sliders not writing InDoorUpOffset and RightOffset to all zoom levels.
- **v2.0** — Complete rewrite from Python to C# / .NET 6 / WPF. Advanced XML editor, preset management, import/export, auto game detection for Steam/Epic/Xbox, settings persistence, mod-active detection.
- **v1.5** — Python version with customtkinter GUI, camera presets, custom sliders, FOV control, HUD centering.

## Credits & Acknowledgements

- **0xFitz** — UCM development, camera tuning, advanced editor, ultrawide HUD support

### C# Rewrite (v2.0)
- **[MrIkso](https://github.com/MrIkso/CrimsonDesertTools)** — CrimsonDesertTools — C# PAZ/PAMT parser, ChaCha20 encryption, LZ4 compression, PaChecksum, archive repacker (.NET 8, MIT)
- **[mcraiha](https://github.com/mcraiha/CSharp-ChaCha20-NetStandard)** — Pure C# ChaCha20 stream cipher implementation used inside CrimsonDesertTools (BSD)
- **[MrIkso on Reshax](https://reshax.com/topic/18908-need-help-extracting-paz-pamt-files-from-crimson-desert-blackspace-engine/page/2/?&_rid=3118#findComment-103796)** — PAZ repacking guide: 16-byte alignment, PAMT checksum (skip first 12 bytes), PAPGT root index patching

### Original Python Version (v1.5)
- **[lazorr410](https://github.com/lazorr410/crimson-desert-unpacker)** — crimson-desert-unpacker — PAZ archive tooling, decryption research
- **@Maszradine** — CDCamera — Camera rules, steadycam system, style presets
- **@manymanecki** — CrimsonCamera — Dynamic PAZ modification architecture

## License

MIT

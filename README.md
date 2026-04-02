> **Nexus Mods:** New-author source review is in progress; the listing may be temporarily unavailable. Downloads remain on **[GitHub Releases](https://github.com/FitzDegenhub/UltimateCameraMod/releases/latest)**.

# Ultimate Camera Mod — Crimson Desert

Standalone camera toolkit for Crimson Desert. Full GUI, live camera preview, three editing tiers, file-based presets, **JSON export for [JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** and **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM), and ultrawide HUD support.

<p align="center">
  <img src="screenshots/banner.png" alt="Ultimate Camera Mod — Crimson Desert banner" width="100%" />
</p>

[![Download](https://img.shields.io/badge/Download-v2.5-brightgreen?style=for-the-badge&logo=github)](https://github.com/FitzDegenhub/UltimateCameraMod/releases/latest)
[![Nexus Mods](https://img.shields.io/badge/Nexus_Mods-UCM-d98f40?style=for-the-badge&logo=nexusmods&logoColor=white)](https://www.nexusmods.com/crimsondesert/mods/438)
[![VirusTotal](https://img.shields.io/badge/VirusTotal-Clean-blue?style=for-the-badge&logo=virustotal&logoColor=white)](https://www.virustotal.com/gui/file/091bdb6456df85b25ce80a90d26710ae1a7f55edf189f8921cbafb153262074a)
[![License](https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge)](LICENSE)
[![Buy me a coffee](https://img.shields.io/badge/Buy_me_a_coffee-FF5E5B?style=for-the-badge&logo=kofi&logoColor=white)](https://ko-fi.com/0xfitz)

---

## Branch overview

| Branch | Status | What it is |
|--------|--------|------------|
| **`main`** | Stable — ships on GitHub Releases | v2.x direct-install app (single exe, writes straight to game PAZ) |
| **`v3-dev`** | Active development | v3 export-first app — tune in-app, export **.json** for **[JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** or **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** |

v3 includes every camera feature from v2 plus a redesigned UI, file-based presets, a three-tier editor, and multi-format export. Direct PAZ install is still available in v3 as a secondary option.

---

## Features

### Camera controls

| Feature | Details |
|---------|---------|
| **8 built-in presets** | Panoramic, Heroic, Vanilla, Close-Up, Low Rider, Knee Cam, Dirt Cam, Survival — with live preview |
| **Custom camera** | Sliders for distance (1.5–12), height (-1.6–0.5), and horizontal shift (-3–3). Proportional scaling keeps the character at the same screen position across all zoom levels |
| **Field of view** | Vanilla 40° up to 80°. Universal FoV consistency across guard, aim, mount, glide, and cinematic states |
| **Centered camera** | Dead-center character across 150+ camera states, eliminating the left-offset shoulder cam |
| **Combat camera** | Three lock-on zoom levels: Default, Wider, Maximum |
| **Mount camera sync** | Mount cameras match your chosen player camera height |
| **Extra zoom levels** | Two additional zoom-out levels (ZL5 + ZL6) on foot and mounted |
| **Horse first person** | Scroll all the way in to see through your character's eyes. Works at walk/trot; may clip during dashes |
| **Horse camera overhaul** | All 8 horse states normalized — identical FoV, blend times, follow rates, damping. 4 zoom levels that scale with your distance slider. No jolts during speed transitions |
| **Horizontal shift on all mounts** | Horse, elephant, wyvern, canoe, warmachine, and broom all respect your shift setting with proportional scaling |
| **Skill aiming consistency** | Lantern, Blinding Flash, Bow, and all aim/zoom/interaction skills respect horizontal shift. No camera snap when activating abilities |
| **Steadycam smoothing** | Normalized blend timing and FoV across idle, walk, run, sprint, combat, guard, and mount states. Community-tunable via the editor |
| **HUD centering** | Width slider (1200–3840 px) for ultrawide. *Currently disabled — a game update added integrity checks that trigger a Coherent Gameface watermark. Will be re-enabled when a workaround is found.* |

### Three-tier editor (v3)

v3 organizes editing into three tabs so you can go as deep as you want:

| Tier | Tab | What it does |
|------|-----|--------------|
| 1 | **UCM Quick** | The fast layer — distance/height/shift sliders, FoV, centered camera, combat zoom, mount sync, steadycam, live camera + FoV previews |
| 2 | **Fine Tune** | Curated deep-tuning. Searchable sections for on-foot zoom levels, horse/mount zoom, global FoV, special mounts & traversal, combat & lock-on, camera smoothing, and aiming & crosshair position. Builds on top of UCM Quick |
| 3 | **God Mode** | Full raw XML editor — every parameter in a searchable, filterable DataGrid grouped by camera state. Vanilla comparison column with modified values highlighted. Expand/collapse all, search, and per-state filtering |

### File-based preset system (v3)

- Presets are **real JSON files** on disk — shareable, versionable, human-readable
- **Sidebar manager** with grouped sections: UCM Presets, Your Presets, Imported
- **New / Duplicate / Rename / Delete** from the sidebar
- **Lock** presets to prevent accidental edits
- **True Vanilla preset** — raw decoded `playercamerapreset` from your game backup with no modifications applied. Quick sliders are synced to the actual game baseline values
- **Import** from raw XML, PAZ archives, or Mod Manager packages
- Auto-migration from legacy `presets/` layout

### Multi-format export (v3)

The **Export for sharing** dialog outputs your session in four ways:

| Format | Use case |
|--------|----------|
| **JSON** (mod managers) | Byte patches + `modinfo` for **[JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** (PhorgeForge) or **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM). Export in UCM → import in the manager you use; recipients do not need UCM. **Prepare** is only offered when the live `playercamerapreset` entry still matches UCM’s vanilla backup (verify game files if you already applied camera mods). |
| **XML** | Raw `playercamerapreset.xml` for other tools or manual editing |
| **0.paz** | Patched archive ready to drop into the game's `0010` folder |
| **.ucmpreset** | Full UCM preset for other UCM users |

Includes title, version, author, Nexus URL, and description fields for JSON/XML. Shows patch region count and bytes changed before saving `.json`.

### Quality of life

- **Auto game detection** — Steam, Epic Games, Xbox / Game Pass
- **Automatic backup** — vanilla backup before any modification; one-click restore. Version-aware with auto-cleanup on upgrade
- **Install config banner** — shows your full active config (FoV, distance, height, shift, settings)
- **Game patch awareness** — tracks install metadata after apply; warns when the game may have updated so you can re-export
- **Live camera + FoV preview** — distance-aware top-down view with horizontal shift and field of view cone
- **Update notifications** — checks GitHub releases on launch
- **Game folder shortcut** — opens your game directory from the header
- **Windows taskbar identity** — proper icon grouping and title bar icon via shell property store
- **Settings persistence** — all selections remembered between sessions
- **Resizable window** — size persists between sessions
- **Portable** — single `.exe`, no installer required

### Preset sharing (v2 + v3)

> **Nobody has perfected Crimson Desert's camera yet — and that's the point.**
>
> The vanilla game has over 150 camera states, each with dozens of parameters. No single developer can tune all of that for every playstyle and display. UCM was built with sharing at its core.
>
> Every setting you tweak can be exported and shared. The guard-camera zoom snap that plagued the vanilla game was solved by a single user adjusting one FoV value. That kind of community-driven fine-tuning is exactly what this tool is for.

**v2 string formats** (still supported):

| Format | Contains |
|--------|----------|
| `UCM:...` | Distance, height, horizontal shift |
| `UCM_ADV:...` | Full XML parameter overrides |

**v3** adds file-based JSON presets and multi-format export on top of these.

---

## How it works

1. Locates the game's PAZ archive containing `playercamerapreset.xml`
2. Creates a backup of the original file (only once — never overwrites a clean backup)
3. Decrypts the archive entry (ChaCha20 + Jenkins hash key derivation)
4. Decompresses via LZ4
5. Parses and modifies the XML camera parameters based on your selections
6. Re-compresses, re-encrypts, and writes the modified entry back into the archive

HUD modifications follow the same pipeline for `ui/minimaphudview2.html`, `ui/statusgaugeview2.html`, and `ui/gamecommon.css` in archive `0012`.

No DLL injection, no memory hacking, no internet connection required — pure data file modification.

---

## Building from source

Requires [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) (or later). Windows x64.

### v3 (recommended)

Close any running instance before building — the exe copy step fails if the file is locked.

```powershell
Stop-Process -Name "UltimateCameraMod.V3" -Force -ErrorAction SilentlyContinue
dotnet build "src/UltimateCameraMod.V3/UltimateCameraMod.V3.csproj" -c Release
Start-Process "src/UltimateCameraMod.V3/bin/Release/net6.0-windows/UltimateCameraMod.V3.exe"
```

### v2.x (single-file publish)

```bash
cd src/UltimateCameraMod
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

Output: `bin/Release/net6.0-windows/win-x64/publish/`

### Dependencies (NuGet — restored automatically)

- [K4os.Compression.LZ4](https://www.nuget.org/packages/K4os.Compression.LZ4/) — LZ4 block compression/decompression

---

## Project structure

```
src/UltimateCameraMod/              Shared library + v2.x WPF app
├── Controls/                       CameraPreview, FovPreview
├── Models/                         PresetCodec, data models
├── Paz/                            ArchiveWriter, CompressionUtils, PAZ I/O
├── Services/                       CameraMod, GameDetector, JsonModExporter, GameInstallBaselineTracker
├── MainWindow.xaml                 v2.x UI
└── UltimateCameraMod.csproj

src/UltimateCameraMod.V3/           v3 export-first WPF app (references shared code above)
├── Controls/                       CameraPreview, FovPreview (v3 variants)
├── Models/                         PresetManagerItem, ImportedPreset
├── Assets/                         ucm.ico, ucm-app-icon.png
├── MainWindow.xaml                 Two-panel shell: sidebar + tabbed editor
├── ExportJsonDialog.xaml           Multi-format export wizard
├── ImportPresetDialog.xaml         Import from XML / PAZ / Mod Manager package
├── NewPresetDialog.xaml            Create / name new presets
├── ShellTaskbarPropertyStore.cs    Windows taskbar icon via shell property store
├── ApplicationIdentity.cs          Shared App User Model ID
└── UltimateCameraMod.V3.csproj

docs/                               Release notes, Nexus stub, PR summary
```

---

## Compatibility

- **Platforms:** Steam, Epic Games, Xbox / Game Pass
- **OS:** Windows 10/11 (x64)
- **Display:** Any aspect ratio — 16:9, 21:9, 32:9

---

## FAQ

**Will this get me banned?**
UCM modifies offline data files only. It does not touch game memory, inject code, or interact with running processes. Use at your own discretion in online/multiplayer modes.

**The game updated and my camera is back to vanilla.**
Normal — game updates overwrite modded files. Re-open UCM and click Install (or re-export JSON for JSON Mod Manager / CDUMM). Your settings are saved automatically.

**My antivirus flagged the exe.**
Known false positive with self-contained .NET apps. [VirusTotal scan is clean](https://www.virustotal.com/gui/file/091bdb6456df85b25ce80a90d26710ae1a7f55edf189f8921cbafb153262074a). Full source is available here to review and build yourself.

**What does horizontal shift 0 mean?**
0 = vanilla camera position (character slightly to the left). 0.5 = character centered on screen. Negative values move further left, positive values move further right.

**Upgrading from a previous version?**
v2.4+ automatically cleans stale data from previous versions on first launch. v3 migrates legacy presets on first run.

---

## Version history

- **v3.0-dev** (`v3-dev`) — Export-first redesign. Three-tier editor (UCM Quick / Fine Tune / God Mode), file-based preset system with sidebar manager, multi-format export (JSON for **JSON Mod Manager** + **Crimson Desert Ultimate Mods Manager**, XML, 0.paz, `.ucmpreset`), vanilla-guarded JSON prepare, true Vanilla preset from raw game XML, game patch awareness, Windows taskbar identity, preset JSON escaping fixes, new app icon.
- **v2.5** — Current stable release on GitHub Releases.
- **v2.4** — Proportional horizontal shift, shift on all mounts and aim abilities, horse camera overhaul, version-aware backups, FoV preview, resizable window.
- **v2.3** — Horizontal shift fix for 16:9, delta-based slider, full install config banner.
- **v2.2** — Steadycam, extra zoom levels, horse first person, horizontal shift, universal FoV, skill aiming consistency, Import XML, preset sharing, update notifications.
- **v2.1** — Fixed custom preset sliders not writing to all zoom levels.
- **v2.0** — Complete rewrite from Python to C# / .NET 6 / WPF. Advanced XML editor, preset management, auto game detection.
- **v1.5** — Python version with customtkinter GUI.

---

## Credits & acknowledgements

- **0xFitz** — UCM development, camera tuning, advanced editor, ultrawide HUD support

### C# rewrite (v2.0)
- **[MrIkso](https://github.com/MrIkso/CrimsonDesertTools)** — CrimsonDesertTools — C# PAZ/PAMT parser, ChaCha20 encryption, LZ4 compression, PaChecksum, archive repacker (.NET 8, MIT)
- **[mcraiha](https://github.com/mcraiha/CSharp-ChaCha20-NetStandard)** — Pure C# ChaCha20 stream cipher implementation (BSD)
- **[MrIkso on Reshax](https://reshax.com/topic/18908-need-help-extracting-paz-pamt-files-from-crimson-desert-blackspace-engine/page/2/?&_rid=3118#findComment-103796)** — PAZ repacking guide: 16-byte alignment, PAMT checksum, PAPGT root index patching

### Original Python version (v1.5)
- **[lazorr410](https://github.com/lazorr410/crimson-desert-unpacker)** — crimson-desert-unpacker — PAZ archive tooling, decryption research
- **@Maszradine** — CDCamera — Camera rules, steadycam system, style presets
- **@manymanecki** — CrimsonCamera — Dynamic PAZ modification architecture

## Support

If you find this useful, consider supporting development:

[![Buy me a coffee](https://img.shields.io/badge/Buy_me_a_coffee-FF5E5B?style=for-the-badge&logo=kofi&logoColor=white)](https://ko-fi.com/0xfitz)

## License

MIT

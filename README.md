# Ultimate Camera Mod — Crimson Desert

Standalone camera toolkit for Crimson Desert with a full GUI, live camera preview, advanced XML editor, and HUD centering for ultrawide displays.

[![Download](https://img.shields.io/badge/Download-v2.0-brightgreen?style=for-the-badge&logo=github)](https://github.com/FitzDegenhub/UltimateCameraMod/releases/latest)
[![Nexus Mods](https://img.shields.io/badge/Nexus_Mods-UCM-d98f40?style=for-the-badge&logo=nexusmods&logoColor=white)](https://www.nexusmods.com/crimsondesert/mods/438)
[![VirusTotal](https://img.shields.io/badge/VirusTotal-Clean-blue?style=for-the-badge&logo=virustotal&logoColor=white)](https://www.virustotal.com/gui/file/d0d2e8d483eae1cecca2cc6358e517d111cbad2984ba52c39d48e13cfcd2cf48?nocache=1)
[![License](https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge)](LICENSE)

![Presets with live camera preview](screenshots/presets.png)

## Features

### Simple Mode
- **8 Camera Presets** with live preview — Panoramic, Heroic, Smoothed, Close-Up, Low Rider, Knee Cam, Dirt Cam, Survival
- **Custom Camera** — Slider control over distance, height, and horizontal offset. Save unlimited named presets, share via import/export codes (`UCM:` strings)
- **Field of View** — Adjustable from vanilla 40° up to 80°, with recommendations per aspect ratio
- **Centered Camera** — Character dead center, eliminating the left-offset shoulder cam across 150+ camera states
- **Combat Camera** — Three lock-on zoom levels: Default, Wider, Maximum
- **Mount Camera Sync** — Mount cameras match your chosen player camera height
- **HUD Centering** — Adjustable width slider (1200–3840px) to constrain HUD elements for ultrawide
- **Steadycam Smoothing** — Eliminates camera sway/bobbing, consistent FOV across movement states

### Advanced Editor

![Advanced Editor with search and vanilla comparison](screenshots/advanced.png)

- **Full XML Editor** — Every player camera parameter exposed in a searchable, filterable DataGrid
- **Vanilla Comparison** — Side-by-side vanilla vs. modified values; modified fields highlighted in gold
- **Grouped by Camera State** — Collapsible sections (Player_Basic_Default, Player_Weapon_Guard, etc.)
- **Save/Load/Delete** named advanced presets
- **Import/Export** advanced configurations as shareable strings (`UCM_ADV:` prefix, distinct from simple presets)
- **Reset to Defaults** — One click to revert all advanced changes

### Quality of Life
- **Auto Game Detection** — Finds Crimson Desert across Steam, Epic Games, and Xbox/Game Pass
- **Automatic Backup** — Creates a vanilla backup before any modification; one-click restore
- **Mod-Active Detection** — Reads live game files to show which modifications are currently installed
- **Settings Persistence** — All selections remembered between sessions
- **Portable** — Single `.exe`, no installer required

![Custom camera with sliders and import/export](screenshots/custom.png)

## How It Works

1. Locates the game's PAZ archive containing `playercamerapreset.xml`
2. Creates a backup of the original file (only once — never overwrites a clean backup)
3. Decrypts the archive entry (ChaCha20 + Jenkins hash key derivation)
4. Decompresses via LZ4
5. Parses and modifies the XML camera parameters based on your selections
6. Re-compresses, re-encrypts, and writes the modified entry back into the archive

HUD modifications follow the same pipeline for `ui/minimaphudview2.html`, `ui/statusgaugeview2.html`, and `ui/gamecommon.css` in archive `0012`.

No DLL injection, no memory hacking, no internet connection required — pure data file modification.

## Building from Source

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

### Dependencies (NuGet, restored automatically)

- [K4os.Compression.LZ4](https://www.nuget.org/packages/K4os.Compression.LZ4/) — LZ4 block compression/decompression

## Project Structure

```
src/UltimateCameraMod/
├── Controls/           # WPF custom controls (camera preview, FOV preview)
├── Models/             # Data models (AdvancedRow, CameraRules, PresetCodec)
├── Paz/                # PAZ archive handling (ChaCha20, LZ4, Jenkins hash, PAMT parsing)
├── Services/           # Core logic (CameraMod, GameDetector, HudMod)
├── MainWindow.xaml     # Main application UI (Simple + Advanced modes)
├── App.xaml            # Application resources and dark theme
└── UltimateCameraMod.csproj
```

## Sharing Presets

UCM uses two string formats so they can't be mixed up:

| Format | Used in | Contains |
|--------|---------|----------|
| `UCM:...` | Custom tab → Export/Import | Distance, height, horizontal shift |
| `UCM_ADV:...` | Advanced editor → Export/Import | Full XML parameter overrides |

Copy a string, send it to a friend, they paste it into Import. Done.

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
Known false positive with self-contained .NET apps. [VirusTotal scan is clean](https://www.virustotal.com/gui/file/d0d2e8d483eae1cecca2cc6358e517d111cbad2984ba52c39d48e13cfcd2cf48?nocache=1). Full source is available here to review and build yourself.

## Version History

- **v2.0** — Complete rewrite from Python to C# / .NET 6 / WPF. Advanced XML editor, preset management, import/export, auto game detection for Steam/Epic/Xbox, settings persistence, mod-active detection.
- **v1.5** — Python version with customtkinter GUI, camera presets, custom sliders, FOV control, HUD centering.

## Credits & Acknowledgements

- **@TheFitzy** — UCM development, camera tuning, advanced editor, ultrawide HUD support

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

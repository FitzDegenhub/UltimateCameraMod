**English** | [한국어](README.ko.md) | [日本語](README.ja.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-Hant.md) | [ไทย](README.th.md) | [Bahasa Indonesia](README.id.md) | [Türkçe](README.tr.md) | [Polski](README.pl.md) | [Italiano](README.it.md) | [Svenska](README.sv.md) | [Norsk](README.nb.md) | [Dansk](README.da.md) | [Suomi](README.fi.md) | [Deutsch](README.de.md) | [Français](README.fr.md) | [Español](README.es.md) | [Português (BR)](README.pt-BR.md) | [Русский](README.ru.md)

---

> **v3.1.2 is here!** Sacred God Mode overrides, Lock-on Auto-Rotate toggle, and all bug fixes. Download from **[GitHub Releases](https://github.com/FitzDegenhub/UltimateCameraMod/releases/latest)** or **[Nexus Mods](https://www.nexusmods.com/crimsondesert/mods/438)**.

# Ultimate Camera Mod - Crimson Desert

Standalone camera toolkit for Crimson Desert. Full GUI, live camera preview, three editing tiers, file-based presets, **JSON export for [JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** and **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM), and ultrawide HUD support.

<p align="center">
  <img src="screenshots/banner.png" alt="Ultimate Camera Mod - Crimson Desert banner" width="100%" />
</p>

<p align="center">

[![Download v3.1.2](https://img.shields.io/badge/Download-v3.1.2-brightgreen?style=for-the-badge&logo=github)](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1.2)
[![Nexus Mods](https://img.shields.io/badge/Nexus_Mods-UCM-d98f40?style=for-the-badge&logo=nexusmods&logoColor=white)](https://www.nexusmods.com/crimsondesert/mods/438)
[![Wiki](https://img.shields.io/badge/Wiki-Documentation-8B5CF6?style=for-the-badge&logo=bookstack&logoColor=white)](https://github.com/FitzDegenhub/UltimateCameraMod/wiki)

[![VirusTotal v3.1.2](https://img.shields.io/badge/VirusTotal_v3.1.2-Clean-blue?style=for-the-badge&logo=virustotal&logoColor=white)](https://www.virustotal.com/gui/file/7c5ddbfce28cabecb799a00b87ad4c4641c30c9db65cd2560c6a91d578852021)
[![Reddit Discussion](https://img.shields.io/badge/Reddit-Discussion-FF4500?style=for-the-badge&logo=reddit&logoColor=white)](https://www.reddit.com/r/CrimsonDesert/comments/1sfou61/ucm_ultimate_camera_mod_v3_crimson_desert_full/)
[![License](https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge)](LICENSE)
[![Buy me a coffee](https://img.shields.io/badge/Buy_me_a_coffee-FF5E5B?style=for-the-badge&logo=kofi&logoColor=white)](https://ko-fi.com/0xfitz)

</p>

> Need help? Check the **[Wiki](https://github.com/FitzDegenhub/UltimateCameraMod/wiki)** for setup guides, camera settings explained, preset management, troubleshooting, and developer documentation.

---

<details>
<summary><strong>Screenshots (v3.x)</strong> — click to expand</summary>
<br>

**UCM Quick** — distance, height, shift, FoV, lock-on zoom, steadycam, live previews
![UCM Quick](screenshots/v3.x/ucm_quick.png)

**Fine Tune** — curated deep-tuning with searchable bordered cards
![Fine Tune](screenshots/v3.x/finetune.png)

**God Mode** — full raw XML editor with vanilla comparison
![God Mode](screenshots/v3.x/godmode.png)

**JSON Export** — export for JSON Mod Manager / CDUMM
![Export JSON](screenshots/v3.x/exportjson_menu.png)

**Import** — import from .ucmpreset, XML, PAZ, or Mod Manager packages
![Import](screenshots/v3.x/import_screen.png)

</details>

---

## Branch overview

| Branch | Status | What it is |
|--------|--------|------------|
| **`main`** | v3.1.2 Release | Standalone camera toolkit with three-tier editor (UCM Quick / Fine Tune / God Mode), file-based presets, community catalog, multi-format export, and direct PAZ install |
| **`development`** | Development | Next version development branch |

v3 includes every camera feature from v2 plus a redesigned UI, file-based presets, a three-tier editor, and multi-format export. Direct PAZ install is still available in v3 as a secondary option.

---

## Features

### Camera controls

| Feature | Details |
|---------|---------|
| **8 built-in presets** | Panoramic, Heroic, Vanilla, Close-Up, Low Rider, Knee Cam, Dirt Cam, Survival - with live preview |
| **Custom camera** | Sliders for distance (1.5-12), height (-1.6-1.5), and horizontal shift (-3-3). Proportional scaling keeps the character at the same screen position across all zoom levels |
| **Field of view** | Vanilla 40° up to 80°. Universal FoV consistency across guard, aim, mount, glide, and cinematic states |
| **Centered camera** | Dead-center character across 150+ camera states, eliminating the left-offset shoulder cam |
| **Lock-on zoom** | Slider from -60% (zoom in on target) to +60% (pull back wide). Affects all lock-on, guard, and rush states. Works independently of Steadycam |
| **Lock-on auto-rotate** | Disable camera snap-to-target when locking on. Prevents the camera whipping around to face enemies behind you. Credits to [@sillib1980](https://github.com/sillib1980) |
| **Mount camera sync** | Mount cameras match your chosen player camera height |
| **Horizontal shift on all mounts** | Horse, elephant, wyvern, canoe, warmachine, and broom all respect your shift setting with proportional scaling |
| **Skill aiming consistency** | Lantern, Blinding Flash, Bow, and all aim/zoom/interaction skills respect horizontal shift. No camera snap when activating abilities |
| **Steadycam smoothing** | Normalized blend timing and velocity sway across 30+ camera states: idle, walk, run, sprint, combat, guard, rush/charge, freefall, super jump, rope pull/swing, knockback, all lock-on variants, mount lock-on, revive lock-on, aggro/wanted, warmachine, and all mount states. Every value is community-tunable via the Fine Tune editor |
| **Sacred God Mode** | Values you edit in God Mode are permanently protected from Quick/Fine Tune rebuilds. Green indicators show which values are sacred. Per-preset storage |

> **v3 design philosophy: value edits only, no structural injection.**
>
> Earlier versions injected new XML lines into the camera file (extra zoom levels, horse first-person mode, horse camera overhaul with additional zoom tiers). v3 removes these features intentionally. Injecting structure has a much higher chance of breaking after game updates, and personal preferences for niche camera modes are better served by dedicated mods distributed through mod managers. UCM now modifies only existing values - the same line count, the same element structure, the same attributes. This makes presets safer to share and more resilient across game patches.

### Three-tier editor (v3)

v3 organizes editing into three tabs so you can go as deep as you want:

| Tier | Tab | What it does |
|------|-----|--------------|
| 1 | **UCM Quick** | The fast layer - distance/height/shift sliders, FoV, centered camera, lock-on zoom (-60% to +60%), lock-on auto-rotate, mount sync, steadycam, live camera + FoV previews |
| 2 | **Fine Tune** | Curated deep-tuning. Searchable sections for on-foot zoom levels, horse/mount zoom, global FoV, special mounts & traversal, combat & lock-on, camera smoothing, and aiming & crosshair position. Builds on top of UCM Quick |
| 3 | **God Mode** | Full raw XML editor - every parameter in a searchable, filterable DataGrid grouped by camera state. Vanilla comparison column. Sacred overrides (green) protected from rebuilds. "Sacred only" filter. 54 attribute tooltips |

### File-based preset system (v3)

- **`.ucmpreset` file format** - dedicated shareable format for UCM camera presets. Drop into any preset folder and it just works
- **Sidebar manager** with collapsible grouped sections: Game Default, UCM Presets, Community Presets, My Presets, Imported
- **New / Duplicate / Rename / Delete** from the sidebar
- **Lock** presets to prevent accidental edits - UCM presets are permanently locked; user presets toggleable via padlock icon
- **True Vanilla preset** - raw decoded `playercamerapreset` from your game backup with no modifications applied. Quick sliders are synced to the actual game baseline values
- **Import** from `.ucmpreset`, raw XML, PAZ archives, or Mod Manager packages. `.ucmpreset` imports get full UCM slider control; raw XML/PAZ/mod manager imports are standalone presets (God Mode editing only, no UCM rules applied) to preserve the original mod author's values
- **Auto-save** - changes to unlocked presets write back to the preset file automatically (debounced)
- Auto-migration from legacy `.json` presets to `.ucmpreset` on first launch

### Preset catalogs (v3)

Browse and download presets directly from UCM. One-click download, no accounts needed.

- **UCM Presets** - 7 official camera styles (Heroic, Panoramic, Close-Up, Low Rider, Knee Cam, Dirt Cam, Survival). Definitions hosted on GitHub, session XML baked locally from your game files + current camera rules. Auto-rebakes when camera rules are updated
- **[Community presets](https://github.com/FitzDegenhub/UltimateCameraMod/tree/main/community_presets)** - community-contributed presets in the main repo, catalog auto-generated by GitHub Actions
- **Browse button** on each sidebar group header opens the catalog browser
- Each preset shows name, author, description, tags, and a link to the creator's Nexus page
- **Update detection** - pulsating update icon when a newer version is available in the catalog. Click to download the update with optional backup to My Presets
- Downloaded presets appear in the sidebar (locked by default - duplicate to edit)
- **2MB file size limit** and JSON validation for safety

**Want to share your preset with the community?** Export as `.ucmpreset` from UCM, then either:
- Submit a [Pull Request](https://github.com/FitzDegenhub/UltimateCameraMod/pulls) adding your preset to the `community_presets/` folder
- Or send your `.ucmpreset` file to 0xFitz on Discord/Nexus and we'll add it for you

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

- **Auto game detection** - Steam, Epic Games, Xbox / Game Pass
- **Automatic backup** - vanilla backup before any modification; one-click restore. Version-aware with auto-cleanup on upgrade
- **Install config banner** - shows your full active config (FoV, distance, height, shift, settings)
- **Game patch awareness** - tracks install metadata after apply; warns when the game may have updated so you can re-export
- **Live camera + FoV preview** - distance-aware top-down view with horizontal shift and field of view cone
- **Update notifications** - checks GitHub releases on launch
- **Game folder shortcut** - opens your game directory from the header
- **Windows taskbar identity** - proper icon grouping and title bar icon via shell property store
- **Settings persistence** - all selections remembered between sessions
- **Resizable window** - size persists between sessions
- **Portable** - single `.exe`, no installer required

### Philosophy

> **Nobody has perfected Crimson Desert's camera yet -- and that's the point.**
>
> The vanilla game has over 150 camera states, each with dozens of parameters. No single developer can tune all of that for every playstyle and display. That's why UCM exists -- not to tell you what the perfect camera is, but to give you the tools to find it yourself and share it with others.
>
> Every setting you tweak can be exported and shared. The lock-on auto-rotate fix that eliminated camera snap during combat was discovered by a single community member experimenting in God Mode. That kind of community-driven fine-tuning is exactly what this tool is for.

### Preset sharing

Export your camera setup as a `.ucmpreset` file and share it with others. Import presets from the community catalog, Nexus Mods, or other players. UCM also exports to JSON (for [JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113) and [CDUMM](https://www.nexusmods.com/crimsondesert/mods/207)), raw XML, and direct PAZ install.

---

## How it works

1. Locates the game's PAZ archive containing `playercamerapreset.xml`
2. Creates a backup of the original file (only once - never overwrites a clean backup)
3. Decrypts the archive entry (ChaCha20 + Jenkins hash key derivation)
4. Decompresses via LZ4
5. Parses and modifies the XML camera parameters based on your selections
6. Re-compresses, re-encrypts, and writes the modified entry back into the archive

No DLL injection, no memory hacking, no internet connection required -- pure data file modification.

---

## Building from source

Requires [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) (or later). Windows x64.

### v3 (recommended)

Close any running instance before building - the exe copy step fails if the file is locked.

```powershell
Stop-Process -Name "UltimateCameraMod.V3" -Force -ErrorAction SilentlyContinue
dotnet build "src/UltimateCameraMod.V3/UltimateCameraMod.V3.csproj" -c Release
Start-Process "src/UltimateCameraMod.V3/bin/Release/net6.0-windows/UltimateCameraMod.V3.exe"
```

### Dependencies (NuGet - restored automatically)

- [K4os.Compression.LZ4](https://www.nuget.org/packages/K4os.Compression.LZ4/) - LZ4 block compression/decompression

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
├── ShippedPresets/                  Embedded community presets deployed on first launch
├── MainWindow.xaml                 Two-panel shell: sidebar + tabbed editor
├── ExportJsonDialog.xaml           Multi-format export wizard (JSON, XML, 0.paz, .ucmpreset)
├── ImportPresetDialog.xaml         Import from .ucmpreset / XML / PAZ
├── ImportMetadataDialog.xaml       Preset metadata entry (name, author, description, URL)
├── CommunityBrowserDialog.xaml     Browse & download community presets from GitHub
├── NewPresetDialog.xaml            Create / name new presets
├── ShellTaskbarPropertyStore.cs    Windows taskbar icon via shell property store
├── ApplicationIdentity.cs          Shared App User Model ID
└── UltimateCameraMod.V3.csproj

community_presets/                  Community-contributed camera presets
ucm_presets/                        Official UCM style preset definitions
```

---

## Compatibility

- **Platforms:** Steam, Epic Games, Xbox / Game Pass
- **OS:** Windows 10/11 (x64)
- **Display:** Any aspect ratio - 16:9, 21:9, 32:9

---

## FAQ

**Will this get me banned?**
UCM modifies offline data files only. It does not touch game memory, inject code, or interact with running processes. Use at your own discretion in online/multiplayer modes.

**The game updated and my camera is back to vanilla.**
Normal - game updates overwrite modded files. Re-open UCM and click Install (or re-export JSON for JSON Mod Manager / CDUMM). Your settings are saved automatically.

**My antivirus flagged the exe.**
Known false positive with self-contained .NET apps. VirusTotal scan is clean: [v3.1.2](https://www.virustotal.com/gui/file/7c5ddbfce28cabecb799a00b87ad4c4641c30c9db65cd2560c6a91d578852021). Full source is available here to review and build yourself.

**What does horizontal shift 0 mean?**
0 = vanilla camera position (character slightly to the left). 0.5 = character centered on screen. Negative values move further left, positive values move further right.

**Upgrading from a previous version?**
v3.x users: just replace the exe, all presets and settings are preserved. v2.x users: delete the old UCM folder, verify game files on Steam, then run v3.1 from a new folder. See the [release notes](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1) for detailed instructions.

---

## Version history

- **v3.1.2** - Fix sacred values missing from Install/exports on God Mode tab. See [release notes](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1.2).
- **v3.1.1** - Fix false-positive tainted backup detection on clean game files.
- **v3.1** - Sacred God Mode overrides (user edits permanently protected from rebuilds). Lock-on Auto-Rotate toggle (credits to [sillib1980](https://github.com/sillib1980)). Green sacred indicators. Full Manual Control install fix. Version-aware upgrade overlay. See [release notes](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1).
- **v3.0.2** - All dialogs converted to in-app overlay system. God Mode overrides persist across tab switches. Preset type selection (UCM Managed vs Full Manual Control). Community preset catalog moved to main repo. 54 God Mode attribute tooltips. Game crash fixes. Vanilla validation updated for June 2026 game patch. 21-page Wiki.
- **v3.0.1** - Export-first redesign. Three-tier editor (UCM Quick / Fine Tune / God Mode). `.ucmpreset` file format. File-based preset system. UCM and community preset catalogs. Multi-format export. Steadycam expanded to 30+ camera states. Lock-on zoom slider.
- **v2.5** - Last v2.x release.
- **v2.4** - Proportional horizontal shift, shift on all mounts and aim abilities, horse camera overhaul, version-aware backups, FoV preview, resizable window.
- **v2.3** - Horizontal shift fix for 16:9, delta-based slider, full install config banner.
- **v2.2** - Steadycam, extra zoom levels, horse first person, horizontal shift, universal FoV, skill aiming consistency, Import XML, preset sharing, update notifications.
- **v2.1** - Fixed custom preset sliders not writing to all zoom levels.
- **v2.0** - Complete rewrite from Python to C# / .NET 6 / WPF. Advanced XML editor, preset management, auto game detection.
- **v1.5** - Python version with customtkinter GUI.

---

## Credits & acknowledgements

- **0xFitz** - UCM development, camera tuning, advanced editor
- **[@sillib1980](https://github.com/sillib1980)** - Discovered Lock-on Auto-Rotate camera fields

### C# rewrite (v2.0)
- **[MrIkso](https://github.com/MrIkso/CrimsonDesertTools)** - CrimsonDesertTools - C# PAZ/PAMT parser, ChaCha20 encryption, LZ4 compression, PaChecksum, archive repacker (.NET 8, MIT)
- **[mcraiha](https://github.com/mcraiha/CSharp-ChaCha20-NetStandard)** - Pure C# ChaCha20 stream cipher implementation (BSD)
- **[MrIkso on Reshax](https://reshax.com/topic/18908-need-help-extracting-paz-pamt-files-from-crimson-desert-blackspace-engine/page/2/?&_rid=3118#findComment-103796)** - PAZ repacking guide: 16-byte alignment, PAMT checksum, PAPGT root index patching

### Original Python version (v1.5)
- **[lazorr410](https://github.com/lazorr410/crimson-desert-unpacker)** - crimson-desert-unpacker - PAZ archive tooling, decryption research
- **Maszradine** - CDCamera - Camera rules, steadycam system, style presets
- **manymanecki** - CrimsonCamera - Dynamic PAZ modification architecture

## Support

If you find this useful, consider supporting development:

[![Buy me a coffee](https://img.shields.io/badge/Buy_me_a_coffee-FF5E5B?style=for-the-badge&logo=kofi&logoColor=white)](https://ko-fi.com/0xfitz)

## License

MIT

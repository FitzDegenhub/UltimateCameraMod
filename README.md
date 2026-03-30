# Ultimate Camera Mod - Crimson Desert

Standalone camera overhaul for Crimson Desert with a full GUI, live camera preview, and HUD centering.

**[Download Latest Release](https://github.com/FitzDegenhub/UltimateCameraMod/releases)** | **[Nexus Mods](https://www.nexusmods.com/crimsondesert/mods/383)**

## Features

- **8 Camera Presets** with live preview diagram - Panoramic, Heroic, Smoothed, Close-Up, Hip/Knee/Dirt Cam, Survival
- **Custom Camera** - Full slider control over distance, height, horizontal offset. Save/load/share presets via import/export codes
- **Field of View** - Adjustable from vanilla 40° up to 80°
- **Centered Camera** - Character dead center instead of left-offset shoulder cam (150+ camera states)
- **Combat Camera** - Three lock-on zoom levels: Default, Wider (+50%), Maximum (+100%)
- **Mount Camera Sync** - Mount cameras match your chosen player camera height
- **HUD Centering** - Adjustable width slider (1200px-3840px) to constrain HUD elements for ultrawide
- **Steadycam Smoothing** - Eliminates camera sway/bobbing, consistent FoV across movement states
- **Patch-proof** - Reads game files on the fly, works on any game version

## How It Works

The mod reads `playercamerapreset.xml` from the game's PAZ archives, applies your chosen modifications dynamically, and writes the result back. For HUD centering, it modifies `ui/minimaphudview2.html`, `ui/statusgaugeview2.html`, and `ui/gamecommon.css` in archive `0012`.

No DLL injection, no memory hacking - pure data modification safe for online play.

## Building from Source

Requires Python 3.10+ with these packages:

```
pip install customtkinter lz4 cryptography darkdetect nuitka
```

Build the standalone exe:

```
cd src
python -m nuitka --mode=onefile --output-filename=UltimateCameraMod.exe --output-dir=../dist ^
  --include-module=camera_mod --include-module=camera_rules --include-module=hud_mod ^
  --include-module=paz_crypto --include-module=paz_parse --include-module=paz_repack ^
  --include-package=cryptography --include-package=lz4 ^
  --include-package=customtkinter --include-package=darkdetect ^
  --enable-plugin=tk-inter --windows-console-mode=attach ^
  --product-name="Ultimate Camera Mod" --file-version=1.5.0.0 --product-version=1.5.0.0 ^
  --file-description="Ultimate Camera Mod - Crimson Desert" ^
  --assume-yes-for-downloads main.py
```

Or run directly without compiling:

```
cd src
python main.py
```

## Credits

- **[@lazorr410](https://github.com/lazorr410)** - [crimson-desert-unpacker](https://github.com/lazorr410/crimson-desert-unpacker) - PAZ archive tooling
- **[@Maszradine](https://www.nexusmods.com/profile/Maszradine)** - [CDCamera](https://www.nexusmods.com/crimsondesert/mods/65) - Camera rules, steadycam system, style presets
- **[@manymanecki](https://www.nexusmods.com/profile/manymanecki)** - [CrimsonCamera](https://www.nexusmods.com/crimsondesert/mods/373) - Dynamic PAZ modification architecture

## License

MIT

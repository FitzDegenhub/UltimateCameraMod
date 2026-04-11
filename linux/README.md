# UltimateCameraMod — Linux CLI port

Single-file Python CLI port of [UltimateCameraMod](https://github.com/FitzDegenhub/UltimateCameraMod)
for Crimson Desert on Linux (native Steam / Proton). No GUI, no .NET, no Wine.

- **One file**: [`ucm.py`](./ucm.py) (~1,400 lines, pure Python)
- **One dependency**: [`python-lz4`](https://python-lz4.readthedocs.io/)
- **Minimum Python**: 3.10
- **Upstream parity**: reuses the exact same crypto (ChaCha20 + Jenkins Lookup3
  key derivation), LZ4 raw block format, camera rule engine, and size-matching
  strategies as the Windows toolkit. Presets read the same `.ucmpreset` files
  from the repo-level `ucm_presets/` folder.

> **This port was built with AI assistance from Claude (Anthropic),** working
> directly from the upstream C# source in `src/UltimateCameraMod/`. The goal
> was the smallest possible single-file tool that matches upstream output
> byte-for-byte on the camera archive entry, so Linux players can apply the
> same preset library as Windows users without running the WPF app under Wine.

---

## Why a Linux port?

The upstream WPF app targets Windows x64 and needs .NET 6+, which makes Wine
or Proton prefixes necessary for Linux users. A small CLI that only needs
`python` and `lz4` covers the 80% case — applying an official preset or
restoring the vanilla camera — without any of the GUI, preview, or JSON
export machinery.

Everything this script does is a straight port of the relevant C# files
under `src/UltimateCameraMod.V3/`:

| C# source                                          | Python equivalent in `ucm.py`        |
|----------------------------------------------------|--------------------------------------|
| `Paz/NameHasher.cs`                                | `compute_hash`                       |
| `Paz/StreamTransform.cs`                           | `stream_apply`                       |
| `Paz/AssetCodec.cs`                                | `build_codec_params`, `asset_decode` |
| `Paz/PamtReader.cs`                                | `pamt_parse`                         |
| `Paz/ArchiveWriter.cs` — `MatchCompressedSize`     | `match_compressed_size` + strategies |
| `Services/CameraMod.cs`                            | XML engine + `install_with_mod_set`  |
| `Models/CameraRules.cs`                            | `build_modifications` + builders     |

> Only the camera-XML path (`MatchCompressedSize`) is mirrored. The newer
> `MatchCompressedSizeHtml` / `MatchCompressedSizeCss` helpers added for
> the Center HUD feature target archive `0012` and are out of scope for
> this Linux CLI port.

---

## Requirements

- Linux (tested on Arch; any modern distro should work)
- Python ≥ 3.10
- `python-lz4` (Arch: `python-lz4`, Debian/Ubuntu: `python3-lz4`, or `pip install lz4`)
- A copy of Crimson Desert installed via Steam (native or Proton)

### Install the LZ4 dependency

```sh
# Arch / Manjaro
sudo pacman -S python-lz4

# Debian / Ubuntu
sudo apt install python3-lz4

# Fedora
sudo dnf install python3-lz4

# Anything else (user-local)
pip install --user lz4
```

---

## Usage

All commands run directly from the script — no install step, no config file.

### List the built-in presets

```sh
python linux/ucm.py list
```

```
Available presets:
  Close-Up        [close-up  ] Tight shoulder OTS with lock-on pull-back, cinematic framing
  Dirt Cam        [dirt-cam  ] Ground-level extreme low angle, intense and gritty
  Heroic          [heroic    ] Shoulder-level OTS with smoothed transitions
  Knee Cam        [knee-cam  ] Knee-height dramatic low angle, action movie feel
  Low Rider       [low-rider ] Hip-level full body view with wide horizon
  Panoramic       [panoramic ] Wide filmic pullback, head-height, great for exploration
  Survival        [survival  ] Tight OTS with offset, tense horror-game feel
```

### Apply a preset

```sh
python linux/ucm.py apply --preset panoramic
```

On first run against a clean install the vanilla PAZ bytes are copied into
`$XDG_DATA_HOME/ultimate_camera_mod/original_backup.bin` (default
`~/.local/share/ultimate_camera_mod/`) before any write. Subsequent applies
rebuild from that pristine copy — they do **not** compound on top of whatever
is currently in the game archive. This matches upstream behavior.

### Apply a style with tweaks (no preset file)

```sh
python linux/ucm.py apply --style heroic --fov 10 --bane
python linux/ucm.py apply --style panoramic --combat-pullback 0.25 --mount-height
```

### Apply a fully custom camera

```sh
python linux/ucm.py apply --custom --distance 7 --height 0 --right-offset 0 --fov 15
```

### Apply a preset file from anywhere

```sh
python linux/ucm.py apply --preset /path/to/MyCoolCam.ucmpreset
```

Supports the standard `.ucmpreset` JSON format used by the upstream app
(`kind: "style"` with `settings.{distance, height, right_offset, fov,
combat_pullback, centered, mount_height, steadycam}`).

### Dump the live camera XML

```sh
python linux/ucm.py extract --out /tmp/camera.xml
```

### Restore vanilla

```sh
python linux/ucm.py restore
```

### Point at a non-Steam install

```sh
python linux/ucm.py --game-dir "/path/to/Crimson Desert" apply --preset heroic
```

### Use a custom backup location

```sh
python linux/ucm.py --backup-dir ~/my_backups apply --preset close-up
```

---

## How auto-detection works

On every run the script tries to find `.../Crimson Desert/0010/0.pamt` in, in
order:

1. `~/.local/share/Steam/steamapps/common/Crimson Desert`
2. `~/.steam/steam/steamapps/common/Crimson Desert`
3. `~/.var/app/com.valvesoftware.Steam/.local/share/Steam/steamapps/common/Crimson Desert` (Flatpak)
4. Every library listed in `libraryfolders.vdf` from each of the above

If none match, pass `--game-dir` explicitly.

---

## Backups

- Default location: `$XDG_DATA_HOME/ultimate_camera_mod/` (i.e.
  `~/.local/share/ultimate_camera_mod/` on most setups)
- File: `original_backup.bin` — **raw encrypted+compressed PAZ entry bytes**,
  exactly as they appear in `0010/0.paz`
- Metadata: `backup_meta.txt` — contains the entry offset and sizes used
  when the backup was taken
- Debug dump: `debug_modified.xml` — the last modified XML written during
  `apply`, useful for diffing against vanilla

`ucm.py` refuses to overwrite a clean backup with a modded one: if the live
game file already looks UCM-touched (heuristic: inspects a couple of known
vanilla FoV values), it assumes the backup is still the real vanilla and
leaves it alone.

### Migrating from an older repo-local backup

If you ran an earlier version of this script that wrote to
`UltimateCameraMod/ucm_backups/`, copy it into the XDG location once:

```sh
mkdir -p ~/.local/share/ultimate_camera_mod
cp UltimateCameraMod/ucm_backups/original_backup.bin ~/.local/share/ultimate_camera_mod/
cp UltimateCameraMod/ucm_backups/backup_meta.txt     ~/.local/share/ultimate_camera_mod/ 2>/dev/null || true
```

Or point at the old location with `--backup-dir UltimateCameraMod/ucm_backups`.

---

## End-to-end verification

The port was smoke-tested with this sequence on an Arch Linux + native Steam
install:

```sh
python linux/ucm.py extract --out /tmp/vanilla.xml       # capture clean vanilla
python linux/ucm.py apply --preset panoramic             # apply
python linux/ucm.py extract --out /tmp/modded.xml        # confirm modifications
python linux/ucm.py restore                              # revert
python linux/ucm.py extract --out /tmp/after_restore.xml
diff -q /tmp/vanilla.xml /tmp/after_restore.xml          # byte-identical
```

The restored XML is byte-identical to the original extract, so the backup /
restore path is lossless.

---

## Scope / parity notes

**In scope (same output as upstream):**

- All 7 official built-in style presets (Heroic, Panoramic, Close-Up, Low
  Rider, Knee Cam, Dirt Cam, Survival)
- Custom distance / height / right-offset / FoV (delta)
- Combat pullback, mount-height sync, steadycam toggle, bane/centered
- Backup, restore, and safe re-apply semantics
- Matching encrypted+compressed PAZ entry size exactly (size-matching
  strategies ported from `ArchiveWriter.cs` — comment injection, binary
  search, scattered comments, tail inflate)

**Out of scope (Windows GUI-only features):**

- UCM Quick / Fine Tune / God Mode three-tier editor
- Sacred God Mode overrides
- JSON / `0.paz` / multi-format export wizard
- Preset catalog browser, community preset downloads, auto-update
- Import from mod manager packages
- Live camera + FoV preview panels
- Windows-only features like taskbar identity

If you need those, use the upstream WPF app on Windows (or Proton).

**FoV semantics note:** the rule engine treats `fov` as an *additive delta*
over vanilla (vanilla is 40°, so `--fov 10` → 50°). Preset files on disk use
absolute FoV (e.g. `"fov": 30`), and are translated automatically when loaded.

---

## Troubleshooting

### `ERROR: python 'lz4' package is required.`

Install `python-lz4` via your package manager (see *Install the LZ4
dependency* above) or `pip install --user lz4`.

### `Game archive index not found at: .../0010/0.pamt`

Auto-detect didn't find your install. Pass `--game-dir`:

```sh
python linux/ucm.py --game-dir "/path/to/Crimson Desert" list
```

### `Backup refused: live XML no longer matches vanilla`

The game file has already been modded by something (this tool or another
mod), so UCM won't overwrite the existing backup. Options:

1. Restore with the current backup first: `python linux/ucm.py restore`
2. Or, if you're sure the backup is stale: `rm ~/.local/share/ultimate_camera_mod/*`
   and then use Steam's *Verify integrity of game files* to reset the PAZ
   before re-running `apply`.

### Size-matching failed

The script ports every strategy from upstream, so this should be rare. If it
happens, check `~/.local/share/ultimate_camera_mod/debug_modified.xml` and
open an issue with that file + the preset you were applying.

---

## Credits

- **[0xFitz / FitzDegenhub](https://github.com/FitzDegenhub)** — original
  UltimateCameraMod C#/WPF app, camera rule engine, preset definitions, and
  the entire upstream ecosystem this port mirrors.
- **Matt Barham ([captainzonks](https://github.com/captainzonks))** — Linux
  CLI port.
- **Claude (Anthropic)** — AI pair programmer used during the port.
- Full upstream credit chain for PAZ/ChaCha20/LZ4 research, camera rule
  design, and preset authoring is in the [root README](../README.md#credits--acknowledgements).

## License

MIT, matching the upstream project.

# Ultimate Camera Mod Wiki

Welcome to the Ultimate Camera Mod (UCM) wiki for Crimson Desert. This wiki is split into two sections: one for users who want to learn how to use the mod, and one for developers who want to understand how the code works or contribute to the project.

## User Guide

Everything you need to get started, customize your camera, and troubleshoot issues.

| Page | What you will learn |
|------|-------------------|
| [Getting Started](user/Getting-Started.md) | First-time setup, game file verification, and your first preset |
| [The Three Editors](user/The-Three-Editors.md) | UCM Quick, Fine Tune, and God Mode explained in detail |
| [Camera Settings Explained](user/Camera-Settings-Explained.md) | What every slider and checkbox does, with examples |
| [Presets](user/Presets.md) | Creating, managing, locking, duplicating, and deleting presets |
| [Preset Types](user/Preset-Types.md) | Managed by UCM vs Full Manual Control, and what each means |
| [Importing Presets](user/Importing-Presets.md) | Importing from mod managers, XML files, PAZ archives, and .ucmpreset files |
| [Exporting and Sharing](user/Exporting-and-Sharing.md) | Exporting as JSON (for mod managers), XML, PAZ, and .ucmpreset |
| [UCM and Community Presets](user/UCM-and-Community-Presets.md) | Browsing and downloading official and community presets |
| [Installing to Game](user/Installing-to-Game.md) | How UCM writes your camera settings into the game |
| [Troubleshooting](user/Troubleshooting.md) | Common problems and their solutions |
| [FAQ](user/FAQ.md) | Frequently asked questions |

## Developer Guide

Technical documentation for contributors and anyone curious about how UCM works under the hood.

| Page | What it covers |
|------|--------------|
| [Architecture Overview](dev/Architecture-Overview.md) | High-level system design, data flow, and project layout |
| [PAZ and PAMT Archives](dev/PAZ-and-PAMT-Archives.md) | How Crimson Desert stores game data and how UCM reads and writes it |
| [Camera Rules Engine](dev/Camera-Rules-Engine.md) | The layered modification system that builds camera XML patches |
| [Session XML Pipeline](dev/Session-XML-Pipeline.md) | How UCM Quick, Fine Tune, and God Mode edits flow into a single XML document |
| [Preset System Internals](dev/Preset-System-Internals.md) | The .ucmpreset JSON format, preset kinds, catalog state, and file layout |
| [Catalog and Download System](dev/Catalog-and-Download-System.md) | How UCM and community preset catalogs work, update detection, and SHA256 tracking |
| [Export Pipeline](dev/Export-Pipeline.md) | JSON mod manager patch generation, XML export, PAZ writing, and .ucmpreset packaging |
| [UI Architecture](dev/UI-Architecture.md) | WPF structure, partial classes, the tutorial overlay, and theme system |
| [Building from Source](dev/Building-from-Source.md) | How to clone, build, and run UCM locally |
| [Known Pitfalls](dev/Known-Pitfalls.md) | Sections that crash the game, size constraints, and other hard-won lessons |

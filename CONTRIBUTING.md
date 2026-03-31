# Contributing to Ultimate Camera Mod

Thanks for your interest in helping improve UCM! Whether you're reporting a bug, sharing a camera preset, improving the code, or just have an idea — every contribution matters.

## Table of Contents

- [Reporting Bugs](#reporting-bugs)
- [Suggesting Features](#suggesting-features)
- [Sharing Camera Presets](#sharing-camera-presets)
- [Development Setup](#development-setup)
- [Project Structure](#project-structure)
- [Making Code Changes](#making-code-changes)
- [Code Style](#code-style)
- [Pull Request Process](#pull-request-process)
- [License](#license)

## Reporting Bugs

Open a [GitHub Issue](https://github.com/FitzDegenhub/UltimateCameraMod/issues/new) with:

- **UCM version** (shown in the app header, e.g. v2.5)
- **Game platform** — Steam, Epic, or Xbox / Game Pass
- **Display setup** — resolution and aspect ratio (16:9, 21:9, 32:9, etc.)
- **Steps to reproduce** — what you did, what you expected, what happened instead
- **Crash log** — if UCM crashed, attach `crash.log` from the same folder as the exe

The more detail you provide, the faster a fix can land.

## Suggesting Features

Feature ideas are welcome. Before opening an issue, search [existing issues](https://github.com/FitzDegenhub/UltimateCameraMod/issues) to see if someone has already suggested it. If so, add a thumbs-up or a comment instead of creating a duplicate.

When opening a feature request, describe:

- What you want to achieve (the goal, not just the mechanism)
- Why the current behavior doesn't cover it
- Any specific camera states, parameters, or UI flows involved

## Sharing Camera Presets

One of the most impactful ways to contribute doesn't require writing any code. UCM's preset system exists specifically so the community can collectively fine-tune over 150 camera states.

**How to share:**

1. Tune your camera in UCM (Simple mode or Advanced Editor)
2. Export your preset (`UCM:` string for simple presets, `UCM_ADV:` for advanced)
3. Post it on [Nexus Mods](https://www.nexusmods.com/crimsondesert/mods/438), [Reddit](https://www.reddit.com/r/CrimsonDesert/), or the project's GitHub Discussions / Issues

If you've solved a specific camera problem (e.g. a jarring transition in a particular combat state), mention which camera state you changed and why. This helps others learn from your work.

## Development Setup

### Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or later
- Windows 10/11 (x64) — the app uses WPF and Windows Forms, so it must be built and run on Windows
- Git

### Clone and Build

```bash
git clone https://github.com/FitzDegenhub/UltimateCameraMod.git
cd UltimateCameraMod/src/UltimateCameraMod
dotnet restore
dotnet build
```

### Run in Development

```bash
cd src/UltimateCameraMod
dotnet run
```

### Publish a Release Build

```bash
cd src/UltimateCameraMod
dotnet publish -c Release -r win-x64 --self-contained \
  -p:PublishSingleFile=true \
  -p:EnableCompressionInSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true
```

Output lands in `bin/Release/net6.0-windows/win-x64/publish/`.

### Dependencies

NuGet packages are restored automatically. The only external dependency is:

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

**Key areas for contributors:**

| Area | What lives there | Good for |
|------|-----------------|----------|
| `Services/CameraMod.cs` | Camera XML modification logic | Adding new camera tweaks or fixing preset math |
| `Models/CameraRules.cs` | Camera state definitions and parameter rules | Adjusting default tuning values |
| `Paz/` | PAZ archive read/write, encryption, compression | Archive format changes or performance improvements |
| `Controls/` | WPF preview controls | UI/UX improvements to the live preview |
| `MainWindow.xaml` / `.cs` | Main UI and event handling | Adding new UI options or improving layout |
| `Services/GameDetector.cs` | Steam/Epic/Xbox game path detection | Supporting new install locations or platforms |

## Making Code Changes

1. **Fork** the repository and create a branch from `main`:

   ```bash
   git checkout -b your-feature-name
   ```

2. **Make your changes.** Keep commits focused — one logical change per commit.

3. **Test your changes** by building and running the app against a Crimson Desert installation. Verify that:
   - The app launches without errors
   - Your change works as intended
   - Existing features (backup, restore, preset import/export) still work

4. **Push** your branch and open a Pull Request.

## Code Style

There is no strict enforced style at this time, but please follow these conventions to keep the codebase consistent:

- **C# conventions** — PascalCase for public members, camelCase for locals and private fields prefixed with underscore (`_fieldName`)
- **Nullable references** — the project has `<Nullable>enable</Nullable>`. Avoid suppressing nullable warnings without good reason
- **XAML** — keep element attributes aligned and use the existing dark theme resource keys in `App.xaml`
- **No commented-out code** — remove dead code instead of commenting it out
- **Meaningful names** — camera parameters are already confusing enough; use descriptive variable and method names

## Pull Request Process

1. **Title** — short, descriptive summary of the change (e.g. "Add ultrawide 32:9 preset" or "Fix mount camera snapping on speed change")
2. **Description** — explain *what* you changed and *why*. If it touches camera tuning, note which camera states are affected
3. **Keep it focused** — one feature or fix per PR. Smaller PRs are easier to review and merge
4. **Screenshots / recordings** — if your change affects the UI or camera behavior, include before/after screenshots or a short clip
5. **No version bumps** — don't change the version in the `.csproj`; version numbers are managed at release time

A maintainer will review your PR and may request changes. Don't be discouraged by feedback — it's how the project stays solid.

## License

By contributing to UCM, you agree that your contributions will be licensed under the [MIT License](LICENSE), the same license that covers the project.

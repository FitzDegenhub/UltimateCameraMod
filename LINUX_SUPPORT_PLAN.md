# Linux Support Plan for UltimateCameraMod

## Executive Summary

UltimateCameraMod (UCM) is a WPF/.NET 6 Windows app that modifies Crimson Desert camera files. The core engine (XML parsing, ChaCha20 crypto, LZ4 compression, preset codec) is **pure cross-platform C#**. The blockers are all in the **UI layer** (WPF) and **game detection** (Windows Registry). This is a favorable split — the hard domain logic ports for free.

**Recommended approach:** Avalonia UI migration (single cross-platform codebase replacing WPF).

> **Status:** Implementation in progress. Core library extracted, Linux game detector built, Avalonia project scaffolded with all dialogs, controls, and MainWindow ported. See `src/UltimateCameraMod.Core/` and `src/UltimateCameraMod.Avalonia/`.

---

## Deep Research: All Approaches Evaluated

Before choosing Avalonia, we researched **12 alternative approaches**:

| Rank | Approach | Viable? | Why Eliminated |
|------|----------|---------|----------------|
| **1** | **Avalonia UI** | **Yes - chosen** | Best fit: XAML-compatible, native feel, proven in modding (Nexus Mods App) |
| 2 | Wine/Proton | Partial | Game detection fails (Linux Steam doesn't use Wine registry). WPF rendering fragile |
| 3 | Blazor Server | Yes | Feels like a web page, not a desktop app. No native file dialogs |
| 4 | Photino + Blazor | Yes | Best web-based UX, but project health is concerning (understaffed) |
| 5 | Uno Platform | Yes | UWP XAML (not WPF) — more translation work, smaller Linux community |
| 6 | Tauri + .NET | Yes | Complete UI rewrite in JavaScript. Steepest learning curve |
| 7 | Eto.Forms | Marginal | Smaller ecosystem, no advantage over Avalonia |
| 8 | GTK# | Marginal | No XAML, limited DataGrid, different paradigm |
| 9 | .NET MAUI | **No** | No official Linux desktop support |
| 10 | Blazor WASM | **No** | Browser sandbox can't access filesystem |
| 11 | ImGui.NET | Marginal | Wrong paradigm for forms-based app |
| 12 | CLI/TUI only | Supplement | Can't replicate visual previews; viable as quick-win add-on |

---

## Current Architecture Audit

### What's Already Cross-Platform (No Work Needed)
| Component | Files | Notes |
|-----------|-------|-------|
| Camera XML engine | `Services/CameraMod.cs` | Regex-based XML modification, pure C# |
| PAZ archive handler | `Paz/ArchiveWriter.cs`, `StreamTransform.cs`, `CompressionUtils.cs`, `PamtReader.cs`, `NameHasher.cs`, `AssetCodec.cs` | ChaCha20 (RFC 7539) + LZ4, fully managed |
| Preset codec | `Models/PresetCodec.cs` | Base64 encode/decode |
| Camera rules | `Models/CameraRules.cs` | Parameter validation/clamping |
| Param documentation | `Models/CameraParamDocs.cs` | Static tooltip data |
| HUD mod | `Services/HudMod.cs` | XML modification |
| JSON mod exporter | `Services/JsonModExporter.cs` | Binary diff generation |
| Baseline tracker | `Services/GameInstallBaselineTracker.cs` | Backup/restore (file I/O only) |
| Preset file format | `.ucmpreset` (JSON) | Standard JSON serialization |
| NuGet dependency | `K4os.Compression.LZ4` | Cross-platform package |

> `ArchiveWriter.SaveTimestamps()` already has a `RuntimeInformation.IsOSPlatform(OSPlatform.Windows)` guard — the only existing cross-platform awareness in the codebase.

### What Needs Porting

| Blocker | Severity | Files Affected |
|---------|----------|---------------|
| **WPF UI framework** | **Critical** | `MainWindow.xaml` + 9 partial `.cs` files, 10 dialog `.xaml` files, `App.xaml`, 2 custom controls |
| **Windows Registry** (game detection) | **High** | `Services/GameDetector.cs` |
| **P/Invoke to user32/shell32/kernel32** | **Medium** | `MainWindow.Taskbar.cs`, `ShellTaskbarPropertyStore.cs` |
| **Win32 file dialogs** | **Medium** | `Microsoft.Win32.OpenFileDialog` / `SaveFileDialog` in Import/Export partials |
| **COM interop** | **Medium** | `ShellTaskbarPropertyStore.cs` (IPropertyStore) |
| **Target framework** | **Low** | Both `.csproj` files (`net6.0-windows`) |

---

## Approach Comparison

### Option A: Avalonia UI (Recommended)

**What:** Replace WPF with [Avalonia UI](https://avaloniaui.net/) — a cross-platform .NET UI framework with XAML syntax nearly identical to WPF.

| Pros | Cons |
|------|------|
| XAML syntax ~90% compatible with WPF | Requires translating all XAML (not 1:1, but close) |
| Single codebase for Windows + Linux + macOS | Some WPF controls have slight API differences |
| Active open-source project, mature ecosystem | Learning curve for Avalonia-specific patterns |
| .NET 6+ support, NuGet packages available | Initial migration effort is non-trivial |
| Native file dialogs built-in (`Avalonia.Dialogs`) | Testing on both platforms during development |
| Maintains the existing GUI experience for users | — |

**Effort estimate:** Medium-large. The core engine ports for free. The UI is ~23 XAML/CS files to translate.

### Option B: CLI / TUI (Linux-Only Frontend)

**What:** Build a terminal-based interface (e.g., using `Spectre.Console` or `Terminal.Gui`) as a separate project that shares the core engine.

| Pros | Cons |
|------|------|
| Much less UI code to write | Loses the visual preview (camera top-down, FoV cone) |
| Fast to ship an MVP | Two UIs to maintain forever |
| Linux users comfortable with CLI | Preset browsing/community catalog worse in terminal |
| No UI framework dependency | Feature parity gap with Windows version |

**Effort estimate:** Small-medium for MVP, but ongoing dual-maintenance cost.

### Option C: Hybrid (Keep WPF for Windows, Avalonia for Linux)

**What:** Keep the existing WPF app untouched, build a parallel Avalonia project sharing the core library.

| Pros | Cons |
|------|------|
| Zero risk to existing Windows app | Two full GUI codebases to maintain |
| Can ship Linux faster (no WPF regression risk) | Every new feature must be implemented twice |
| Shared core engine via class library | Drift between platforms over time |

**Effort estimate:** Medium initially, but highest long-term cost.

### Recommendation: Option A (Avalonia UI)

Avalonia is the clear winner because:
1. **One codebase** — no dual maintenance burden
2. **XAML familiarity** — the existing XAML translates with modest changes
3. **Full feature parity** — camera previews, sliders, dialogs all work cross-platform
4. **Growing ecosystem** — Avalonia is the de facto cross-platform .NET UI framework
5. **Future-proof** — also enables macOS support for free

---

## Detailed Implementation Plan

### Phase 0: Extract Core Library (Foundation)

**Goal:** Separate platform-independent code into a standalone `netstandard2.1` / `net6.0` class library.

**Steps:**
1. Create `src/UltimateCameraMod.Core/UltimateCameraMod.Core.csproj` targeting `net6.0` (no `-windows` suffix)
2. Move into it:
   - `Models/` — `AdvancedRow.cs`, `CameraParamDocs.cs`, `CameraRules.cs`, `PresetCodec.cs`
   - `Services/` — `CameraMod.cs`, `HudMod.cs`, `JsonModExporter.cs`, `GameInstallBaselineTracker.cs`
   - `Paz/` — all 6 files (ArchiveWriter, StreamTransform, CompressionUtils, PamtReader, NameHasher, AssetCodec)
3. Move `K4os.Compression.LZ4` NuGet reference to the Core project
4. Create `IGameDetector` interface in Core:
   ```csharp
   public interface IGameDetector
   {
       (string? Path, string Platform) FindGameDir();
       bool CheckWritePermission(string gameDir);
   }
   ```
5. Keep the existing `GameDetector.cs` as `WindowsGameDetector` implementing `IGameDetector`
6. Update V3 `.csproj` to reference `UltimateCameraMod.Core` instead of `<Compile Include>` links
7. Verify Windows build still works identically

**Files changed:** 3 `.csproj` files, new `IGameDetector.cs`, rename existing detector class.

---

### Phase 1: Linux Game Detector

**Goal:** Implement `LinuxGameDetector : IGameDetector` that finds Crimson Desert under Proton/Wine/Lutris.

**Linux game installation paths to search:**

```
# Steam (native library folders)
~/.steam/steam/steamapps/common/Crimson Desert
~/.local/share/Steam/steamapps/common/Crimson Desert
# + additional library folders from libraryfolders.vdf

# Steam Proton prefix (game runs via Proton)
~/.steam/steam/steamapps/compatdata/<APP_ID>/pfx/drive_c/...

# Flatpak Steam
~/.var/app/com.valvesoftware.Steam/.steam/steam/steamapps/common/Crimson Desert

# Lutris
~/Games/crimson-desert/...  (varies by install config)

# Heroic Games Launcher (Epic on Linux)
~/Games/Heroic/Crimson Desert/
~/.config/heroic/legendaryConfig/...

# Bottles (Wine manager)
~/.local/share/bottles/bottles/*/drive_c/...
```

**Implementation approach:**
1. Parse `~/.steam/steam/steamapps/libraryfolders.vdf` for Steam library paths (same VDF parsing as Windows, just different root)
2. Walk Proton `compatdata/` directories for the Crimson Desert App ID
3. Check Heroic launcher config (`~/.config/heroic/`) for Epic installs
4. Scan `XDG_DATA_HOME` and common game directories
5. Use `RuntimeInformation.IsOSPlatform()` to select the right detector at startup

**Key difference from Windows:** No registry. All detection is file-system based (VDF files, JSON configs, directory scanning).

---

### Phase 2: Avalonia UI Migration

**Goal:** Replace WPF XAML + code-behind with Avalonia equivalents.

#### 2a. Project Setup
1. Create `src/UltimateCameraMod.Avalonia/UltimateCameraMod.Avalonia.csproj`:
   ```xml
   <Project Sdk="Microsoft.NET.Sdk">
     <PropertyGroup>
       <OutputType>WinExe</OutputType>
       <TargetFramework>net6.0</TargetFramework>
       <Nullable>enable</Nullable>
     </PropertyGroup>
     <ItemGroup>
       <PackageReference Include="Avalonia" Version="11.*" />
       <PackageReference Include="Avalonia.Desktop" Version="11.*" />
       <PackageReference Include="Avalonia.Themes.Fluent" Version="11.*" />
     </ItemGroup>
     <ItemGroup>
       <ProjectReference Include="..\UltimateCameraMod.Core\UltimateCameraMod.Core.csproj" />
     </ItemGroup>
   </Project>
   ```

#### 2b. XAML Translation (file-by-file)

Each WPF XAML file maps to an Avalonia equivalent. Key syntax differences:

| WPF | Avalonia |
|-----|----------|
| `xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"` | `xmlns="https://github.com/avaloniaui"` |
| `Window` | `Window` (same) |
| `Visibility="Collapsed"` | `IsVisible="False"` |
| `MouseLeftButtonDown` | `PointerPressed` |
| `TextBlock.TextWrapping="Wrap"` | `TextBlock TextWrapping="Wrap"` (same) |
| `Microsoft.Win32.OpenFileDialog` | `Avalonia.Platform.Storage.IStorageProvider` |
| `DispatcherTimer` | `Avalonia.Threading.DispatcherTimer` |

**Files to translate (23 total):**

| WPF File | Priority | Complexity |
|----------|----------|------------|
| `App.xaml` / `App.xaml.cs` | P0 | Low — remove AUMID call, set Avalonia app lifecycle |
| `MainWindow.xaml` | P0 | High — largest file, main layout with tabs/sidebar |
| `MainWindow.xaml.cs` | P0 | High — initialization, state management |
| `MainWindow.Editors.cs` | P0 | Medium — slider/control event handlers |
| `MainWindow.FineTune.cs` | P0 | Medium — fine tune tab logic |
| `MainWindow.GodMode.cs` | P0 | Medium — DataGrid for raw XML editing |
| `MainWindow.Presets.cs` | P0 | Medium — preset CRUD |
| `MainWindow.Export.cs` | P1 | Medium — file save dialogs |
| `MainWindow.Import.cs` | P1 | Medium — file open dialogs |
| `MainWindow.Community.cs` | P1 | Medium — HTTP catalog fetch + list |
| `MainWindow.Overlay.cs` | P2 | Low — modal overlay helper |
| `MainWindow.Taskbar.cs` | P2 | **Drop entirely** — replace with Avalonia's built-in icon support |
| `ShellTaskbarPropertyStore.cs` | P2 | **Drop entirely** — Windows-only COM interop |
| `ApplicationIdentity.cs` | P2 | **Drop entirely** — Windows AUMID constant |
| `ExportDialog.xaml` | P1 | Low |
| `ExportJsonDialog.xaml` | P1 | Low |
| `ImportDialog.xaml` | P1 | Low |
| `ImportPresetDialog.xaml` | P1 | Low |
| `ImportMetadataDialog.xaml` | P1 | Low |
| `AdvancedImportDialog.xaml` | P1 | Low |
| `NewPresetDialog.xaml` | P1 | Low |
| `InputDialog.xaml` | P2 | Low |
| `CommunityBrowserDialog.xaml` | P1 | Low |
| `Controls/CameraPreview.cs` | P0 | Medium — custom drawing → Avalonia `DrawingContext` |
| `Controls/FovPreview.cs` | P0 | Medium — custom drawing → Avalonia `DrawingContext` |

#### 2c. Platform-Specific Code Elimination

| Current Code | Replacement |
|-------------|-------------|
| `ShellTaskbarPropertyStore.cs` (172 lines) | Delete — Avalonia handles app icon natively |
| `MainWindow.Taskbar.cs` (180 lines) | Delete — Avalonia `Window.Icon` property |
| P/Invoke `user32.dll` SendMessage/LoadImage | Delete — not needed |
| P/Invoke `shell32.dll` SHGetPropertyStoreForWindow | Delete — not needed |
| P/Invoke `kernel32.dll` GetModuleHandle | Delete — not needed |
| `Microsoft.Win32.OpenFileDialog` | `TopLevel.StorageProvider.OpenFilePickerAsync()` |
| `Microsoft.Win32.SaveFileDialog` | `TopLevel.StorageProvider.SaveFilePickerAsync()` |
| `WindowInteropHelper` | Not needed on Avalonia |
| `SetCurrentProcessExplicitAppUserModelID` | Not needed |

#### 2d. Custom Controls

The two custom drawing controls need translation:

- **`CameraPreview.cs`** — Top-down camera position visualization. Uses WPF `DrawingContext`. Avalonia has an equivalent `DrawingContext` via `Control.Render(DrawingContext)` override. Translation is mostly mechanical (same concepts: `DrawEllipse`, `DrawLine`, `DrawGeometry`).

- **`FovPreview.cs`** — Field-of-view cone visualization. Same story — straightforward Avalonia render override.

---

### Phase 3: Build & CI Pipeline

**Goal:** Multi-platform builds and automated testing.

#### GitHub Actions Workflow
```yaml
name: Build
on: [push, pull_request]
jobs:
  build:
    strategy:
      matrix:
        os: [windows-latest, ubuntu-latest]
        include:
          - os: windows-latest
            rid: win-x64
          - os: ubuntu-latest
            rid: linux-x64
    runs-on: ${{ matrix.os }}
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '6.0.x'
      - run: dotnet build src/UltimateCameraMod.Avalonia/
      - run: dotnet publish src/UltimateCameraMod.Avalonia/ -c Release -r ${{ matrix.rid }} --self-contained
```

#### Linux Packaging Options

| Format | Pros | Cons |
|--------|------|------|
| **AppImage** | Single file, runs anywhere, no install needed | Large file size (~70MB self-contained) |
| **Flatpak** | Sandboxed, auto-updates via Flathub | Complex manifest, permission setup |
| **tar.gz** | Simplest, unpack and run | No desktop integration |
| **.deb / .rpm** | Native package manager integration | Distro-specific |

**Recommendation:** Ship **AppImage** as the primary Linux distribution (matches the "single .exe download" experience Windows users have). Optionally add Flatpak later for discoverability.

---

### Phase 4: Testing Strategy

1. **Unit tests** for Core library (XML modification, preset codec, PAZ read/write) — run on both Windows and Linux CI
2. **Integration tests** for `LinuxGameDetector` with mock directory structures
3. **Manual QA matrix:**
   - Ubuntu 22.04+ / Fedora 38+ / Arch (latest)
   - Steam Deck (SteamOS — major Linux gaming platform)
   - Verify game detection with Steam Proton prefix
   - Verify PAZ archive modification round-trip
   - Verify preset import/export
4. **Steam Deck priority** — this is likely the #1 Linux use case for a game camera mod

---

## Project Structure (After Migration)

```
src/
├── UltimateCameraMod.Core/          # Cross-platform core library (net6.0)
│   ├── Models/
│   │   ├── AdvancedRow.cs
│   │   ├── CameraParamDocs.cs
│   │   ├── CameraRules.cs
│   │   └── PresetCodec.cs
│   ├── Services/
│   │   ├── IGameDetector.cs         # Interface
│   │   ├── CameraMod.cs
│   │   ├── HudMod.cs
│   │   ├── JsonModExporter.cs
│   │   └── GameInstallBaselineTracker.cs
│   ├── Paz/
│   │   ├── ArchiveWriter.cs
│   │   ├── StreamTransform.cs
│   │   ├── CompressionUtils.cs
│   │   ├── PamtReader.cs
│   │   ├── NameHasher.cs
│   │   └── AssetCodec.cs
│   └── UltimateCameraMod.Core.csproj
│
├── UltimateCameraMod.Avalonia/      # Cross-platform GUI (net6.0 + Avalonia)
│   ├── Platform/
│   │   ├── WindowsGameDetector.cs   # Registry-based (original logic)
│   │   └── LinuxGameDetector.cs     # File-system based (new)
│   ├── Controls/
│   │   ├── CameraPreview.cs         # Avalonia port
│   │   └── FovPreview.cs            # Avalonia port
│   ├── Dialogs/
│   │   ├── ExportDialog.axaml
│   │   ├── ImportDialog.axaml
│   │   └── ... (all dialogs)
│   ├── MainWindow.axaml             # .axaml = Avalonia XAML
│   ├── MainWindow.axaml.cs
│   ├── MainWindow.Editors.cs
│   ├── MainWindow.*.cs              # Same partial file structure
│   ├── App.axaml
│   ├── App.axaml.cs
│   └── UltimateCameraMod.Avalonia.csproj
│
├── UltimateCameraMod/               # Legacy v2 (keep as-is, no changes)
└── UltimateCameraMod.V3/            # Legacy WPF v3 (keep as-is, or deprecate)
```

---

## Risk Assessment

| Risk | Impact | Mitigation |
|------|--------|------------|
| Avalonia XAML differences cause subtle UI bugs | Medium | Side-by-side testing with WPF version |
| Steam Deck display scaling issues | Medium | Test on actual Steam Deck hardware |
| Game detection misses edge-case Proton configs | Low | Community feedback + fallback manual path selection |
| DataGrid behavior differs in Avalonia | Medium | God Mode tab needs careful testing |
| Performance on low-end Linux hardware | Low | App is lightweight (file I/O + simple UI) |
| .NET 6 runtime availability on Linux | Low | Self-contained publish bundles the runtime |

---

## Migration Order (Prioritized)

```
Phase 0  [~1-2 days]   Extract Core library, verify Windows build unchanged
Phase 1  [~1-2 days]   LinuxGameDetector implementation
Phase 2a [~1 day]      Avalonia project scaffold + App.axaml
Phase 2b [~3-5 days]   MainWindow + editors + controls (P0 files)
Phase 2c [~2-3 days]   Dialogs + export/import + community (P1 files)
Phase 2d [~1 day]      Cleanup: drop Windows-only taskbar code (P2 files)
Phase 3  [~1 day]      CI pipeline + AppImage packaging
Phase 4  [ongoing]     Testing, Steam Deck validation, community feedback
```

**Total estimated effort: ~10-15 days of focused development.**

---

## Quick Win: What Can Ship First?

If you want to get something into Linux users' hands fast before the full Avalonia migration:

1. **Phase 0 + Phase 1 + CLI wrapper** — Extract Core, add Linux game detection, ship a simple CLI that applies a preset file to the game. Linux power users (especially Steam Deck tinkerers) would absolutely use this.
2. Full Avalonia GUI follows later.

This gets a working Linux tool out in ~3-4 days while the GUI migration continues.

---

## Summary

The codebase is in good shape for a Linux port. The hard part (crypto, compression, XML manipulation, archive handling) is already pure cross-platform C#. The work is:

1. **Extract** the core engine into a shared library
2. **Add** Linux game detection (file-system scanning instead of Registry)
3. **Replace** WPF with Avalonia UI (XAML translation, not a rewrite)
4. **Delete** ~350 lines of Windows-only taskbar/shell interop code
5. **Package** as AppImage for easy distribution

No fundamental blockers. It's a tractable migration, not a rewrite.

# Building from Source

This document covers prerequisites, solution structure, project references, NuGet dependencies, build commands, and publish configuration for UCM.

## Prerequisites

| Requirement | Version | Notes |
|-------------|---------|-------|
| .NET SDK | 6.0+ | The projects target `net6.0` / `net6.0-windows`. Any 6.0+ SDK will work. |
| OS | Windows 10/11 | WPF is Windows-only. Linux/macOS cannot build the V3 app. |
| Visual Studio | 2022+ (optional) | For IDE-based development. VS Code with C# Dev Kit also works. |
| Git | Any recent version | For cloning the repository. |

WPF and Windows Forms support is required. The .NET SDK's Windows Desktop workload must be installed. If you installed the SDK via Visual Studio, this is included by default. For standalone SDK installs:

```bash
dotnet workload install microsoft-net-sdk-windowsdesktop
```

## Solution Structure

The repository contains two projects under `src/`:

```
src/
    UltimateCameraMod/                  # Shared library (v2 legacy + shared services)
        UltimateCameraMod.csproj
        Models/
            AdvancedRow.cs
            CameraParamDocs.cs
            CameraRules.cs
            PresetCodec.cs
        Services/
            CameraMod.cs
            GameDetector.cs
            GameInstallBaselineTracker.cs
            JsonModExporter.cs
        Paz/
            ArchiveWriter.cs
            AssetCodec.cs
            CompressionUtils.cs
            NameHasher.cs
            PamtReader.cs
            PazEntry.cs
            StreamTransform.cs
        MainWindow.xaml.cs              # v2 app entry (legacy)
        ...

    UltimateCameraMod.V3/              # V3 WPF application
        UltimateCameraMod.V3.csproj
        MainWindow.xaml
        MainWindow.xaml.cs
        MainWindow.Presets.cs
        MainWindow.Editors.cs
        MainWindow.FineTune.cs
        MainWindow.GodMode.cs
        MainWindow.Import.cs
        MainWindow.Export.cs
        MainWindow.Community.cs
        MainWindow.Taskbar.cs
        Controls/
            CameraPreview.cs
            FovPreview.cs
        Models/
            PresetManagerItem.cs
            ImportedPreset.cs
        Assets/
            ucm.ico
            ucm-app-icon.png
        ShippedPresets/
            *.json
        ...
```

### Why Two Projects?

The shared library (`UltimateCameraMod/`) was originally the v2 application. When v3 was developed as a new WPF app, the core services (camera modification engine, PAZ archive handling, game detection) were kept in the original project to allow potential reuse. The V3 project references shared source files via `<Compile Include>` links rather than a project reference.

### Project Reference Strategy

The V3 `.csproj` includes shared source files using linked compile items:

```xml
<ItemGroup>
    <Compile Include="..\UltimateCameraMod\Models\AdvancedRow.cs">
        <Link>Shared\Models\AdvancedRow.cs</Link>
    </Compile>
    <Compile Include="..\UltimateCameraMod\Models\CameraParamDocs.cs">
        <Link>Shared\Models\CameraParamDocs.cs</Link>
    </Compile>
    <Compile Include="..\UltimateCameraMod\Models\CameraRules.cs">
        <Link>Shared\Models\CameraRules.cs</Link>
    </Compile>
    <Compile Include="..\UltimateCameraMod\Models\PresetCodec.cs">
        <Link>Shared\Models\PresetCodec.cs</Link>
    </Compile>
    <Compile Include="..\UltimateCameraMod\Services\CameraMod.cs">
        <Link>Shared\Services\CameraMod.cs</Link>
    </Compile>
    <Compile Include="..\UltimateCameraMod\Services\GameDetector.cs">
        <Link>Shared\Services\GameDetector.cs</Link>
    </Compile>
    <Compile Include="..\UltimateCameraMod\Services\GameInstallBaselineTracker.cs">
        <Link>Shared\Services\GameInstallBaselineTracker.cs</Link>
    </Compile>
    <Compile Include="..\UltimateCameraMod\Services\JsonModExporter.cs">
        <Link>Shared\Services\JsonModExporter.cs</Link>
    </Compile>
    <Compile Include="..\UltimateCameraMod\Paz\*.cs">
        <Link>Shared\Paz\%(Filename)%(Extension)</Link>
    </Compile>
</ItemGroup>
```

This means the shared `.cs` files are compiled directly into the V3 assembly. There is no separate DLL output for the shared library when building V3. The `<Link>` elements control how these files appear in the Solution Explorer (under a virtual `Shared/` folder).

## NuGet Dependencies

| Package | Version | Used By | Purpose |
|---------|---------|---------|---------|
| `K4os.Compression.LZ4` | 1.3.8 | Both projects | LZ4 compression/decompression for PAZ archive entries |

This is the only external NuGet dependency. It is referenced in both `.csproj` files:

```xml
<PackageReference Include="K4os.Compression.LZ4" Version="1.3.8" />
```

All other functionality uses .NET BCL types (`System.Text.Json`, `System.Security.Cryptography`, `System.Net.Http`, etc.).

## V3 Project Configuration

Key properties from `UltimateCameraMod.V3.csproj`:

```xml
<PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net6.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>false</ImplicitUsings>
    <UseWPF>true</UseWPF>
    <UseWindowsForms>true</UseWindowsForms>
    <AssemblyName>UltimateCameraMod.V3</AssemblyName>
    <RootNamespace>UltimateCameraMod.V3</RootNamespace>
    <Version>3.0.1-beta</Version>
    <Authors>0xFitz</Authors>
    <Description>Export-first camera workflow for Crimson Desert Mod Manager</Description>
    <ApplicationIcon>Assets\ucm.ico</ApplicationIcon>
</PropertyGroup>
```

Notable settings:
- `ImplicitUsings` is **disabled** in V3 (explicit `using` statements throughout). The shared library project has it enabled.
- `UseWindowsForms` is enabled for `FolderBrowserDialog` usage (WPF does not have a native folder picker).
- `OutputType` is `WinExe` (not `Exe`), which suppresses the console window.

### Embedded Resources

Shipped presets are embedded as resources:

```xml
<ItemGroup>
    <EmbeddedResource Include="ShippedPresets\*.json" />
</ItemGroup>
```

### Icon Handling

The `.csproj` includes a custom target to copy the icon to the output directory:

```xml
<Target Name="CopyUcmIcoToOutput" AfterTargets="Build">
    <Copy SourceFiles="$(MSBuildProjectDirectory)\Assets\ucm.ico"
          DestinationFolder="$(OutputPath)Assets"
          SkipUnchangedFiles="true" />
    <Copy SourceFiles="$(MSBuildProjectDirectory)\Assets\ucm.ico"
          DestinationFiles="$(OutputPath)ucm.ico"
          SkipUnchangedFiles="true" />
</Target>
```

The icon is copied to both `Assets/ucm.ico` (standard location) and the output root `ucm.ico` (fallback for `LoadImage(LR_LOADFROMFILE)` when the Windows shell probes for the icon before the Assets directory exists).

## Shared Library Project Configuration

Key properties from `UltimateCameraMod.csproj`:

```xml
<PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net6.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <UseWindowsForms>true</UseWindowsForms>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <AssemblyName>UltimateCameraMod</AssemblyName>
    <RootNamespace>UltimateCameraMod</RootNamespace>
    <Version>2.5.0</Version>

    <!-- Publishing: single-file self-contained -->
    <PublishSingleFile>true</PublishSingleFile>
    <SelfContained>true</SelfContained>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>
</PropertyGroup>
```

This project is the v2 application. It has `OutputType=WinExe` because it was originally a standalone app. The publish properties are for the v2 distribution and are not used when building V3.

## Build Commands

### Debug Build (V3)

```bash
dotnet build src/UltimateCameraMod.V3/UltimateCameraMod.V3.csproj
```

Output: `src/UltimateCameraMod.V3/bin/Debug/net6.0-windows/`

### Release Build (V3)

```bash
dotnet build src/UltimateCameraMod.V3/UltimateCameraMod.V3.csproj -c Release
```

Output: `src/UltimateCameraMod.V3/bin/Release/net6.0-windows/`

### Build Both Projects

If a solution file exists:

```bash
dotnet build src/UltimateCameraMod.sln
```

Or build individually (the V3 project does not have a `<ProjectReference>` to the shared library, so they are independent build targets).

### NuGet Restore

Happens automatically on build, but can be triggered explicitly:

```bash
dotnet restore src/UltimateCameraMod.V3/UltimateCameraMod.V3.csproj
```

## Publish Configuration

For distribution, UCM is published as a single-file, self-contained executable.

### Publish Command (V3)

```bash
dotnet publish src/UltimateCameraMod.V3/UltimateCameraMod.V3.csproj \
    -c Release \
    -r win-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:EnableCompressionInSingleFile=true
```

Output: `src/UltimateCameraMod.V3/bin/Release/net6.0-windows/win-x64/publish/`

### Publish Properties Explained

| Property | Value | Purpose |
|----------|-------|---------|
| `PublishSingleFile` | `true` | Bundles all assemblies, resources, and native libraries into a single `.exe` |
| `SelfContained` | `true` | Includes the .NET runtime so users do not need to install .NET separately |
| `RuntimeIdentifier` | `win-x64` | Targets 64-bit Windows |
| `EnableCompressionInSingleFile` | `true` | Compresses the bundled assemblies to reduce file size |

The resulting executable is standalone and can be distributed as a single file (plus the `Assets/ucm.ico` that is copied alongside).

### Distribution Artifacts

After publish, the distribution package typically contains:

```
UltimateCameraMod.V3.exe        # Single-file self-contained executable
Assets/
    ucm.ico                     # Application icon
ucm.ico                         # Shell fallback icon
```

The preset directories (`ucm_presets/`, `my_presets/`, etc.) and sidecar files (`.catalog_state.json`, `advanced_overrides.json`, `install_trace.txt`) are created at runtime.

## Testing

There is no automated test project. Testing is performed manually through the application. The key verification points are:

- Export/install pipeline: Use `install_trace.txt` and `payload_changed` field to verify writes
- Binary diff: `VerifyPatchesRoundTrip()` in `JsonModExporter` provides runtime verification during JSON export
- Size matching: `ArchiveWriter.MatchCompressedSize` throws if it cannot hit the target size, preventing silent corruption

If contributing, manual testing should cover:
1. Fresh launch (no presets, no game detected)
2. Game detection and vanilla backup creation
3. Quick tab slider changes and preview updates
4. Fine Tune slider generation and sync from Quick
5. God Mode DataGrid editing
6. All four export formats
7. Install to Game and Restore
8. Preset download, update detection, and update flow
9. Import from each supported format (XML, PAZ, JSON, UCM preset)

## Common Build Issues

**"UseWPF is not supported"**: You are building on Linux or macOS. WPF requires Windows.

**Missing .NET SDK**: Install the .NET 6.0 SDK from https://dotnet.microsoft.com/download. The `dotnet` command must be on your PATH.

**NuGet restore failure**: Check network connectivity. The only external package is `K4os.Compression.LZ4` from NuGet.org.

**Icon not found at runtime**: The `CopyUcmIcoToOutput` target runs after build. If you are running from a non-standard output directory, the icon may not be present. The app handles this gracefully with fallback behavior.

## Relevant Source Files

| File | Role |
|------|------|
| `src/UltimateCameraMod.V3/UltimateCameraMod.V3.csproj` | V3 app project file |
| `src/UltimateCameraMod/UltimateCameraMod.csproj` | Shared library / v2 project file |
| `src/UltimateCameraMod.V3/GlobalUsings.cs` | Global using directives for V3 |
| `src/UltimateCameraMod/GlobalUsings.cs` | Global using directives for shared library |
| `src/UltimateCameraMod.V3/AssemblyInfo.cs` | Assembly metadata |

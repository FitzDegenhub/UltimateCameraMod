# PR: `v3-dev` → `main` — UCM v3 (export-first / Mod Manager)

**Use this file as the GitHub PR description** (copy from below the line). Refresh when `v3-dev` gains more commits.

---

## Summary

Introduces **Ultimate Camera Mod v3**: a **second WPF front-end** (`src/UltimateCameraMod.V3/`) aimed at an **export-first** workflow—tune the camera in-app, **export JSON** for **Crimson Desert Mod Manager**, and treat direct PAZ install as secondary compared to **v2.x** on `main`. Core PAZ/XML logic stays in **`src/UltimateCameraMod/`** and is **shared** with v3.

**v2.x** remains the shipping line on **GitHub Releases** until v3 is explicitly released; this PR is the integration vehicle for that future cut.

---

## Goals

- Preset workflow centered on **real JSON files** (shareable, versionable) instead of only in-app state.
- **One session XML** driving **UCM Quick**, **Fine Tune**, and **God Mode** where possible.
- **JSON patch export** aligned with current **Mod Manager** expectations.
- **True “Vanilla” built-in preset** that matches stock game camera data and Quick-slider baselines.
- **Windows polish**: correct **taskbar grouping and icon** for the v3 exe.
- **Game-update awareness**: optional warnings when install metadata drifts after patches.

---

## What shipped on this branch (high level)

### New app & UX (`UltimateCameraMod.V3`)

- **Two-panel shell**: sidebar **preset manager** + tabbed editor (**UCM Quick** / **Fine Tune** / **God Mode**).
- **Preset groups**: built-ins under `ucm_presets/`, user presets under `my_presets/`, **import** flows; migration from legacy `presets/` layout.
- **Dialogs**: **Export JSON** wizard (`ExportJsonDialog`), **import preset** (`ImportPresetDialog`), **new preset** (`NewPresetDialog`).
- **Performance-minded UI**: cached resources, debounced search, async file I/O where appropriate, lighter sidebar refresh for large `session_xml` (partial header reads).
- **Branding / shell**: `ucm.ico` + PNG asset; **App User Model ID** + **`RelaunchIconResource`** via `SHGetPropertyStoreForWindow` (`ShellTaskbarPropertyStore`, `ApplicationIdentity`) in addition to existing icon/class tricks.

### Shared library & services (`UltimateCameraMod`)

- **`JsonModExporter`**: binary diff → JSON patches; **`JavaScriptEncoder.UnsafeRelaxedJsonEscaping`** for preset files; **`ExtractJsonStringField`** so sidebar metadata decodes JSON escapes correctly (e.g. `+` / `\u002B` in descriptions).
- **`CameraMod`**: vanilla XML read path for built-in Vanilla preset; **`TryParseUcmQuickFootBaselineFromXml`** so Quick distance / height / right offset match **`Player_Basic_Default` / `ZoomLevel[2]`**; **`vanilla_preset_rev`** for forced regeneration of built-in Vanilla JSON.
- **`GameInstallBaselineTracker`**: persists **`game_install_baseline.json`** after a successful apply; tracks universal signals and **Steam `appmanifest`** fields where applicable; **MainWindow** wiring for snooze + informational banner (iterative).
- **`ArchiveWriter`** and related PAZ path tweaks as needed for the above flows.
- **v2 `MainWindow`**: minimal touch where shared behavior or docs alignment was required.

### Docs & repo

- **README**: dedicated **v3 development** section (feature table, **build & run v3** PowerShell snippet, project layout including V3).
- **`docs/NEXUS_MOD_PAGE.md`**: short Nexus-facing stub.
- **Repo cleanup** (on the path to v3): release notes relocated under `docs/release-notes/`, `.gitignore` hardened, tracked binaries removed from tree.

### Explicitly not in the tree anymore

- **`tools/ReadVanillaQuickBaseline`** was removed after the initial push; vanilla baseline debugging stays in-app / logs as needed.

---

## v3-dev — recent feature notes (after the original PR summary)

Use this subsection when updating the PR after newer `v3-dev` commits land.

### Import metadata, editors, and shell (`4460b42` and follow-ups)

- **`ImportMetadataDialog`**: optional **author / description / URL** when importing; fields persist on **`ImportedPreset`** and flow through save/build paths.
- **Active preset header**: **clickable preset URL** when metadata includes a link.
- **Locked presets**: edits surface a **toast** (instead of hard-disabling controls) so the rule is visible without dead UI.
- **UCM Quick → editors**: Quick slider changes **sync into Fine Tune and God Mode** so deeper tabs stay consistent with the session.
- **Fine Tune**: section groups in **bordered cards**; **wider label column** for readability.
- **Tab bar**: **DockPanel** layout so **God Mode** stays visually anchored on the right.
- **`PresetManagerItem`**: link/metadata plumbing for sidebar and selection summaries.
- **README**: Reddit badge removed; **Ko-fi / support** callouts added in separate doc commits (`4b27964`, `bd3f3b5`, `e57b591`).
- **`issue_body.md`**: tracked write-up of the **Coherent Gameface / HUD watermark** situation (archive `0012`), for GitHub issues or cross-linking.

### Shared `JsonModExporter` — patch region labels (`4460b42`)

- Builds an **XML offset map** over vanilla bytes and labels each diff region with a **human-readable hint** (e.g. which camera/XML area) instead of a generic “Camera parameter change” string everywhere. Improves Mod Manager JSON reviewability.

### Shipped community presets (embedded)

- **`ShippedPresets/*.json`** are **embedded resources** in `UltimateCameraMod.V3.csproj` (not only loose files on disk), so presets like **RDR2** ship inside the assembly.
- **`GenerateBuiltInPresets()`** ends with **`DeployShippedCommunityPresets()`**: after built-in Vanilla / styles are written (game folder detected), each embedded `UltimateCameraMod.V3.ShippedPresets.*.json` is deserialized (imported-preset shape: `Name`, `Author`, `Description`, `Url`, `RawXml`), converted to a **session JSON** under **`ucm_presets/{Name}.json`**, with Quick sliders derived from **ZL2** via **`TryParseUcmQuickFootBaselineFromXml`** and **`QuickShiftDeltaFromFootZl2RightOffset`**.
- **Does not overwrite** an existing file of the same name (user deletes/edits are respected).

### Vanilla built-in Quick slider semantics (shared)

- **`CameraRules.QuickShiftDeltaFromFootZl2RightOffset`**: maps literal XML **RightOffset** at on-foot ZL2 to the UCM Quick **horizontal shift delta** (inverse of `BuildCustom` mapping).
- **Built-in Vanilla.json** now stores **`right_offset`** using that delta so the Quick panel matches true vanilla XML (~0.5 → slider 0).
- **`CameraMod.TryParseUcmQuickFootBaselineFromXml`** documentation updated to spell out literal vs delta.

### Import / metadata / UI (`c1b83b1` + `430a8db`)

- **Save toast** can show **error styling** (`_pendingSaveToastIsError`) for failed saves.
- **`ExportJsonDialog`**: minor alignment with current export flow.
- **`MainWindow.xaml`**: layout / copy / tooltip text (e.g. horizontal shift help aligned with **UCM Quick**).
- **Startup**: **first** preset-manager refresh runs **before** game detection so **my_presets** / imports are visible without the game. When a game folder is present, **`OnGameDirResolved`** kicks off an **async** camera scan that **refreshes the sidebar again** (alongside built-in + shipped JSON written in `OnLoaded`).
- **`screenshots/banner.png`**: hero image in the root **README** (centered, responsive width; `430a8db`).

---

## Commits (`main`..`v3-dev`) — reference

| Commit   | Topic |
|----------|--------|
| `430a8db` | README: wire `screenshots/banner.png` hero image |
| `c1b83b1` | Embedded shipped presets (RDR2), Vanilla Quick delta, import/export UI touches |
| `4460b42` | Import metadata dialog, locked-preset toasts, Quick→Fine/God sync, Fine Tune cards, JSON patch labels |
| `4b27964` / `bd3f3b5` / `e57b591` | README: Ko-fi / support presentation tweaks |
| `44260b9` | Add this PR summary doc for GitHub |
| `4a78a84` | Remove `ReadVanillaQuickBaseline`; neutral `.gitignore` comment for local worktree path |
| `8d1f328` | Taskbar/icon, vanilla baseline sync, JSON + baseline tracker, README/docs |
| `a8f4ce7` | v3.0-dev: UI redesign + file-based preset system |
| `ae8ba3c` | README (Nexus / releases note) |
| `c9f3657` | Repo cleanup: release notes, `.gitignore`, remove tracked binaries |

---

## How to build & run v3 (Windows)

Requires **.NET 6 SDK**. **Stop** any running **`UltimateCameraMod.V3`** before `dotnet build` (locked exe → MSB3027).

```powershell
Stop-Process -Name "UltimateCameraMod.V3" -Force -ErrorAction SilentlyContinue
dotnet build "src/UltimateCameraMod.V3/UltimateCameraMod.V3.csproj" -c Release
Start-Process "src/UltimateCameraMod.V3/bin/Release/net6.0-windows/UltimateCameraMod.V3.exe"
```

v2.x publish flow is unchanged; see README **Building from Source (v2.x)**.

---

## Risk / review focus

- **Large shared surface**: `CameraMod`, `JsonModExporter`, PAZ writer—regressions could affect **v2** users; smoke-test v2 publish/run if anything in shared code paths changed.
- **v3** is **not** yet wired as the primary Release artifact; merge strategy and versioning (v3.0 vs continued v2.5.x) should be explicit before tagging.

---

## Checklist (for maintainers)

- [ ] v2.x: backup / install / restore still sane on `main`-equivalent + this branch’s shared changes  
- [ ] v3: preset create / duplicate / import / export JSON round-trip  
- [ ] v3: Vanilla built-in preset matches expected stock XML + Quick sliders  
- [ ] v3: taskbar icon + window title icon on a clean Windows profile  
- [ ] README / release messaging updated when v3 actually ships on Releases  

---

*Last refreshed: April 2026 — documents embedded shipped presets (e.g. RDR2), Vanilla Quick delta alignment, and import metadata/UI follow-ups. Match the branch tip on GitHub for the exact commit.*

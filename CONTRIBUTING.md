# Contributing to Ultimate Camera Mod

Thanks for wanting to help build `Ultimate Camera Mod` for `Crimson Desert`.

This project is still evolving quickly, and outside contributors are very welcome. Camera behavior, XML coverage, UI polish, preset tuning, archive tooling, and regression testing all benefit from extra hands.

## Ways To Help

- Fix bugs in camera install, restore, preset handling, and XML editing.
- Improve camera tuning for on-foot, horse, combat, aim, and special mounts.
- Improve `UCM Quick`, `Fine Tune`, and `God Mode` UX.
- Add missing parameter coverage where vanilla camera states are not yet exposed cleanly.
- Improve export/import support for external mod workflows.
- Improve docs, setup instructions, and troubleshooting.

## Before You Start

- Open an issue or discussion if you want to work on a larger change.
- For small fixes, a PR is fine without prior discussion.
- If your change affects live game file writes, camera state mapping, or preset compatibility, please describe the expected before/after behavior clearly.

## Project Layout

- `src/UltimateCameraMod/` - shared core logic, camera rules, archive tooling, legacy app
- `src/UltimateCameraMod.V3/` - current V3 WPF app
- `Additional Resources/` - reference files, mod manager docs, sample assets

## Local Setup

Requires:

- Windows
- .NET 6 SDK or later

Build V3:

```powershell
dotnet build "src/UltimateCameraMod.V3/UltimateCameraMod.V3.csproj" -c Release
```

Run V3:

```powershell
dotnet run --project "src/UltimateCameraMod.V3/UltimateCameraMod.V3.csproj"
```

Build the legacy app:

```powershell
dotnet build "src/UltimateCameraMod/UltimateCameraMod.csproj" -c Release
```

Important on Windows: if `UltimateCameraMod.V3.exe` is already running, stop it before rebuilding or MSBuild may fail to replace the executable.

```powershell
Stop-Process -Name "UltimateCameraMod.V3" -Force -ErrorAction SilentlyContinue
```

## Development Guidelines

- Keep behavior aligned with vanilla where the mod is meant to preserve original state coverage.
- Avoid injecting non-vanilla camera states unless that is an intentional feature and clearly documented.
- Prefer fixing shared logic in `src/UltimateCameraMod/` when V3 and the legacy app depend on the same code.
- Be careful with generated/runtime files under `bin/Release/...`; they should not drive source-of-truth behavior.
- Keep number formatting invariant for XML and patch output.
- Avoid committing temporary diagnostics, extracted XML dumps, local traces, or personal test files.

## Testing Expectations

There is currently no deep automated test suite for the full camera pipeline, so manual verification matters.

For changes that affect installs, presets, or XML generation, please verify as many of these as possible:

- Build succeeds in `Release`
- App launches successfully
- Install to game works
- Restore vanilla works
- Presets save and reload correctly
- Quick, Fine Tune, and God Mode stay in sync where expected
- No extra non-vanilla zoom levels or camera states are unintentionally injected
- Horse, combat, and aim behavior still feel correct in-game

If you cannot test against the game directly, say so in the PR.

## Pull Requests

Please keep PRs focused.

Good PRs usually include:

- a short summary of the problem
- what changed
- any risk areas
- a quick manual test plan

If the change touches camera tuning, screenshots, XML snippets, or a short gameplay note are helpful.

## Good First Contribution Areas

- Fine Tune / God Mode parameter coverage
- camera rule cleanup and parity fixes
- preset generation and migration bugs
- install/restore diagnostics and error handling
- docs and onboarding improvements

## Questions

If you are unsure where to contribute, open an issue or discussion and describe:

- what you want to improve
- whether it is code, tuning, UI, or documentation
- whether you can test in-game

That is enough to get started.

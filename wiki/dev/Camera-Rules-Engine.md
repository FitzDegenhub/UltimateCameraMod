# Camera Rules Engine

The camera rules engine (`src/UltimateCameraMod/Models/CameraRules.cs`) is the brain of UCM. It produces a `ModificationSet` (a dictionary of XML attribute patches) that `CameraMod.ApplyModifications()` applies to the vanilla `playercamerapreset.xml` inside the game's `0.paz` file.

This page explains every layer, section list, style builder, and design decision in the engine.

---

## ModificationSet

The output of CameraRules is a `ModificationSet`:

```csharp
public sealed class ModificationSet
{
    public Dictionary<string, Dictionary<string, (string Action, string Value)>> ElementMods { get; set; }
    public int FovValue { get; set; }
}
```

- `ElementMods` maps XML element keys (like `"Player_Basic_Default/ZoomLevel[2]"`) to a dictionary of attribute patches. Each patch is a tuple of action (`"SET"` or `"REMOVE"`) and value.
- `FovValue` is the additive FoV delta (e.g., `20` means +20 degrees on top of each section's vanilla FoV).

FoV is handled separately from ElementMods because it is additive (applied on top of whatever the vanilla value is), while all other modifications are absolute replacements.

---

## Section Lists

CameraRules organizes the game's camera XML into section groups. Every section in the XML represents a camera state (idle, walking, combat, mounted, etc.).

### BasicSections

On-foot non-combat camera states:

| Section | Camera state |
|---------|-------------|
| `Player_Basic_Default` | Standing idle |
| `Player_Basic_Default_Walk` | Walking |
| `Player_Basic_Default_Run` | Running |
| `Player_Basic_Default_Runfast` | Sprinting |

### WeaponSections

On-foot combat camera states:

| Section | Camera state |
|---------|-------------|
| `Player_Weapon_Default` | Combat idle (weapon drawn) |
| `Player_Weapon_Default_Walk` | Combat walking |
| `Player_Weapon_Default_Run` | Combat running |
| `Player_Weapon_Default_RunFast` | Combat sprinting |
| `Player_Weapon_Default_RunFast_Follow` | Sprint follow during combat |
| `Player_Weapon_Rush` | Rush/charge attack |
| `Player_Weapon_Guard` | Blocking/guarding |

### AllMain

`BasicSections` + `WeaponSections` combined. This is the primary target for style builders, which set distance, height, and horizontal offset across all on-foot states.

### LockOnSections

Combat lock-on camera states. These are NOT in AllMain and need separate distance scaling via `BuildLockOnDistances()`:

| Section | Camera state |
|---------|-------------|
| `Player_Weapon_LockOn` | Single-target lock-on |
| `Player_Weapon_TwoTarget` | Two-target lock-on |
| `Player_Weapon_LockOn_System` | System-triggered lock-on |
| `Player_Revive_LockOn_System` | Revive lock-on |
| `Player_FollowLearn_LockOn_Boss` | Boss follow lock-on |
| `Player_Weapon_LockOn_Non_Rotate` | Non-rotating lock-on |
| `Player_Weapon_LockOn_WrestleOnly` | Wrestling lock-on |
| `Player_Interaction_TwoTarget` | NPC interaction two-target |

### HorseRideSections

Horse-specific mount states:

| Section | Camera state |
|---------|-------------|
| `Player_Ride_Horse` | Horse idle/walk |
| `Player_Ride_Horse_Run` | Horse running |
| `Player_Ride_Horse_Fast_Run` | Horse galloping |
| `Player_Ride_Horse_Dash` | Horse dashing |
| `Player_Ride_Horse_Dash_Att` | Horse dash attack |
| `Player_Ride_Horse_Att_Thrust` | Horse thrust attack |
| `Player_Ride_Horse_Att_R` | Horse right attack |
| `Player_Ride_Horse_Att_L` | Horse left attack |

### AllMountSections

`HorseRideSections` plus all other mounts:
- `Player_Ride_Elephant`
- `Player_Ride_Wyvern`
- `Player_Ride_Canoe`
- `Player_Ride_Warmachine`
- `Player_Ride_Broom`

### BaneSections

Sections targeted by the Centered Camera feature. Same as `AllMain` (BasicSections + WeaponSections). Sets `RightOffset=0` across all of them.

---

## Excluded Sections (Do NOT Modify)

### SilenceKill Sections

`Player_SilenceKill` and `Player_SilenceKill_Back` are stealth finisher cameras. **Changing ANY attribute on these sections causes an immediate game crash on load.** This was confirmed via bisect testing. Root cause is unknown but likely the engine treats these sections specially.

These sections are not in any section list and are never targeted by any builder.

### NPC Dialogue Sections

`Player_Interaction_LockOn` and `Interaction_LookAt` are NPC dialogue cameras (triggered when pressing CTRL near an NPC). Scaling `ZoomDistance`, `MaxZoomDistance`, or injecting ZL2-4 here has correlated with game crashes on load.

These are intentionally NOT in `LockOnSections`. The tradeoff is that the dialogue camera stays at vanilla values, which can cause a visible camera jump when your custom offsets differ significantly from vanilla.

---

## Layered Composition System

`BuildModifications()` is the entry point. It composes the final `ModificationSet` by running layers in order. Each layer produces its own dictionary of patches, which is merged into the running result. Later layers override earlier ones for the same key/attribute.

### Layer Order

```
1. BuildSharedBase()
2. BuildSmoothing()           [only if Steadycam enabled]
2b. BuildSharedSteadycam()    [only if Steadycam enabled]
3. BuildLockOnDistances()     [default 3.4/6/8 seed]
4. Style builder              [Heroic/Panoramic/Custom/etc.]
5. BuildBaneMods()            [only if Centered Camera enabled]
6. BuildCombatPullback()      [only if lock-on zoom != 0]
7. BuildMountHeightMods()     [only if mount camera sync enabled]
```

### Layer 1: BuildSharedBase()

Global normalization applied to every UCM preset regardless of style.

**FoV normalization:** Sets `Fov="40"` across ~30 sections. This creates a consistent baseline so the additive FoV delta works predictably. Without this, different sections have different vanilla FoV values (40, 45, 53), making a "+20" delta feel inconsistent.

Sections normalized include all run/sprint states, combat states, lock-on variants, mount states, swimming, riding, gliding, aiming, and interaction states.

**Lock-on behavior tuning:** Sets `TargetRate` and `ScreenClampRate` on lock-on sections for smoother target tracking. Also sets `LimitUnderDistance` on `Player_Weapon_TwoTarget`.

**On-foot base distances:** Sets ZoomDistance on ZL2=3.4, ZL3=6, ZL4=8 for all `AllMain` sections. These are the default "vanilla-like" distances that style builders will override.

### Layer 2: BuildSmoothing() (Steadycam)

Only runs when Steadycam is enabled. Smooths camera transitions across 30+ states.

**CameraBlendParameter:** Sets `BlendInTime` and `BlendOutTime` on all major sections so transitions between camera states (idle to walk, walk to run, etc.) are smooth instead of jarring.

**OffsetByVelocity damping:** Zeros out `OffsetByVelocity` attributes on walk/run sections. In vanilla, the camera sways laterally based on movement speed. This removes that sway.

**MaxZoomDistance ceiling:** Sets `MaxZoomDistance="30"` on all lock-on and finisher section ZoomLevels. Vanilla has ceilings as low as 5, which clamp the camera inward the moment lock-on engages. Setting 30 is a non-constraining ceiling with no gameplay downside.

**Finisher smoothing:** Sets `BlendInTime` on `Player_SilenceKill` and `Player_Weapon_Down` so kill animations ease in instead of snapping. Note: only blend parameters are set on SilenceKill, NOT ZoomDistance or MaxZoomDistance (those cause crashes).

### Layer 2b: BuildSharedSteadycam()

Zeros `UpOffset` on horse/mount sections so the camera height doesn't jump when mounting. This runs separately from BuildSmoothing because it is always applied when Steadycam is on.

### Layer 3: BuildLockOnDistances(zl2, zl3, zl4)

This is the key fix for the "zoom-in on lock-on" problem.

**The problem:** When you lock onto a target, the game switches to a lock-on camera section. Vanilla lock-on sections have hardcoded low `ZoomDistance` values (e.g., 1.2, 3, 4) with `MaxZoomDistance` ceilings as low as 5. If your on-foot camera is at distance 12, lock-on snaps to distance 4 with a ceiling of 5. This causes a brutal zoom-in effect.

**The fix:** Set ZoomDistance on ALL lock-on sections to match the style's on-foot distances. Every style builder calls `BuildLockOnDistances()` with its own ZL2/ZL3/ZL4 values so lock-on always mirrors on-foot.

The default seed (3.4/6/8) is applied in `BuildModifications()` before the style layer, so even the "default" style and custom presets get lock-on distance matching.

After Fine Tune or God Mode overrides are applied, `BuildCuratedSessionXml` and `BuildGodModeSessionXml` re-read the actual on-foot ZL2/ZL3/ZL4 from the resulting XML and call `BuildLockOnDistancesPublic()` again so manual distance edits are also reflected in lock-on.

Sections targeted: all `LockOnSections` plus additional combat sections (`Player_Weapon_Rush`, `Player_Force_LockOn`, `Player_LockOn_Titan`, `Cinematic_LockOn`, `Player_Weapon_Down`, etc.).

### Layer 4: Style Builders

Each style sets `ZoomDistance`, `UpOffset`, and `RightOffset` for `AllMain` sections, then calls `BuildLockOnDistances()` with its own distances.

| Style | Distance (ZL2) | Height (UpOffset) | Right Offset | Description |
|-------|---------------|-------------------|-------------|-------------|
| Heroic | 5.0 | -0.2 | 0.0 | Shoulder-level over-the-shoulder |
| Panoramic | 7.5 | 0.0 | 0.0 | Head-height wide pullback, filmic |
| Default (Vanilla) | 3.4 | 0.0 | 0.0 | Unmodified game camera |
| Close-Up | 4.0 | -0.2 | 0.0 | Tighter shoulder OTS |
| Low Rider | 5.0 | -0.8 | 0.0 | Hip-level, full body + horizon |
| Knee Cam | 5.0 | -1.2 | 0.0 | Knee-height dramatic low angle |
| Dirt Cam | 5.0 | -1.5 | 0.0 | Ground-level, extreme low |
| Survival | 3.0 | 0.0 | 0.7 | Tight horror-game OTS with right offset |

**BuildCustom()** uses user-provided values from the UCM Quick sliders (`customDistance`, `customHeight`, `customRightOffset`). It applies proportional ZoomDistance scaling across ZL2/ZL3/ZL4 based on the distance value, and applies horizontal shift as a delta on `RightOffset` across all main sections plus mounts and aim states.

The horizontal shift uses delta semantics: the user's slider value is added to vanilla `RightOffset` values rather than replacing them. This preserves per-section offset differences while shifting the overall framing. `QuickShiftDeltaFromFootZl2RightOffset()` converts between the file's absolute RightOffset and the slider's delta value.

### Layer 5: BuildBaneMods() (Centered Camera)

When Centered Camera is enabled, sets `RightOffset="0"` across all `BaneSections` (BasicSections + WeaponSections). This eliminates the default left-offset shoulder cam and dead-centers the character.

When this is active, the Horizontal Shift slider is locked to 0 in the UI because the two features conflict.

### Layer 6: BuildCombatPullback()

Proportional offset on lock-on distances. The user's Lock-on Zoom slider (-60% to +60%) maps to this.

At 0%: lock-on distances match on-foot distances (no change).
At +25%: lock-on distances are 25% further out (wider battlefield view).
At -25%: lock-on distances are 25% closer (cinematic focus effect).

### Layer 7: BuildMountHeightMods()

When Mount Camera sync is enabled, sets `UpOffset` on all mount sections to match the player's on-foot `UpOffset` setting. This prevents the camera height from jumping when you mount a horse.

Also sets mount `ZoomDistance` values proportionally based on the on-foot distance.

---

## Helper Functions

### Set(mods, key, attr, value)

Adds a `("SET", value)` entry for the given attribute on the given element key. Creates the dictionary entry if it doesn't exist.

### Merge(baseDict, overlay)

Merges an overlay modification dictionary into a base dictionary. For each key, overlay attributes override base attributes. This is how layers compose: later layers override earlier ones.

---

## How CameraMod.ApplyModifications() Uses the ModificationSet

`CameraMod.ApplyModifications()` in `Services/CameraMod.cs` walks the vanilla XML line by line:

1. Maintains a depth stack to track which section we're inside
2. For each XML tag, constructs a key (e.g., `"Player_Basic_Default/ZoomLevel[2]"`)
3. Looks up the key in `ElementMods`
4. If found, applies SET operations (replace attribute value) or REMOVE operations (delete attribute)
5. Separately, applies the additive FoV delta to any `Fov` or `InDoorFov` attribute on Player_, Cinematic_, or Glide_ sections
6. Auto-injects missing ZoomLevel nodes when modifications target ZL3/ZL4 that don't exist in vanilla

The result is a complete modified XML string ready to be size-matched, compressed, encrypted, and written back to the PAZ archive.

---

## Re-syncing After Fine Tune / God Mode Edits

When the user makes manual edits in Fine Tune or God Mode, the on-foot ZoomDistance values may change. To keep lock-on in sync:

1. `BuildCuratedSessionXml()` (Fine Tune) and `BuildGodModeSessionXml()` (God Mode) re-read the actual ZL2/ZL3/ZL4 values from the current session XML
2. They call `BuildLockOnDistancesPublic()` with those values
3. The lock-on sections are re-patched to match

This ensures that even manual distance edits in the advanced editors are reflected in lock-on behavior.

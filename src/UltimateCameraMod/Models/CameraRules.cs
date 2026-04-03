namespace UltimateCameraMod.Models;

// ═══════════════════════════════════════════════════════════════════════════════
// CameraRules — Architecture Overview
// ═══════════════════════════════════════════════════════════════════════════════
//
// This file is the entire camera modification rule engine. It produces a
// ModificationSet (a dictionary of XML attribute patches) that CameraMod.cs
// applies to the vanilla playercamerapreset.xml inside the game's 0.paz file.
//
// ── Layering order (BuildModifications) ─────────────────────────────────────
//
//  1. BuildSharedBase()
//     Global normalisation: FOV=40 across all sections, on-foot ZoomDistance
//     set to 3.4/6/8 for ZL2/ZL3/ZL4, combat/guard distances matched to on-foot
//     so idle→walk→run→combat transitions have no distance jump.
//
//  2. BuildSmoothing()  [only when Steadycam is enabled]
//     Smooths every camera transition the user will notice:
//     - CameraBlendParameter (BlendInTime/BlendOutTime) on all major sections
//     - OffsetByVelocity damping so the camera doesn't sway on run/dash
//     - MaxZoomDistance=30 on all lock-on and finisher sections so vanilla
//       ceilings (e.g. "5") never clamp the camera when the user has a high
//       custom distance set
//     - Blend smoothing on stealth finishers (SilenceKill) and combat
//       finisher (Weapon_Down) so kill animations ease in instead of snapping
//     NOTE: ZoomDistance is NOT set here for lock-on/finisher sections.
//           That is handled by BuildLockOnDistances() called from each style
//           builder so distances always match the user's chosen style.
//
//  2b. BuildSharedSteadycam()  [only when Steadycam is enabled]
//      UpOffset zeroing on horse/mount sections so the camera height doesn't
//      jump when mounting.
//
//  3. BuildLockOnDistances(zl2, zl3, zl4)  [seeded with default 3.4/6/8]
//     Sets ZoomDistance on ALL lock-on sections, stealth finishers, and the
//     combat finisher (Weapon_Down) to match the style's on-foot distances.
//     This is the key fix for the "zoom-in on lock-on" problem:
//     - Vanilla lock-on sections have fixed low ZoomDistance values (e.g. 1.2,
//       3, 4) with MaxZoomDistance ceilings as low as 5 that clamp the camera
//       inward the moment lock-on engages.
//     - Every style builder calls Merge(m, BuildLockOnDistances(...)) with its
//       own distances so lock-on always mirrors on-foot.
//     - BuildModifications seeds the default (3.4/6/8) before the style layer
//       so "default" style and custom presets are also covered.
//     - After Fine Tune or God Mode overrides are applied, BuildCuratedSessionXml
//       and BuildGodModeSessionXml re-read the actual on-foot ZL2/ZL3/ZL4 from
//       the resulting XML and call BuildLockOnDistancesPublic() again so manual
//       ZoomDistance edits are also reflected in lock-on.
//
//  4. Style builder (BuildHeroic / BuildPanoramic / BuildCloseUp /
//                    BuildLowVariant / BuildSurvival / BuildCustom)
//     Sets ZoomDistance, UpOffset, RightOffset for AllMain sections.
//     Each builder ends with Merge(m, BuildLockOnDistances(zl2, zl3, zl4))
//     using its own distances so lock-on is always in sync.
//
//  5. BuildBaneMods()  [optional]
//     Centres the camera (RightOffset=0) for the Bane centred-camera option.
//
//  6. BuildCombatPullback(zl2, zl3, zl4, offset)  [optional]
//     Proportional offset on top of the base lock-on distances.
//     +0.25 = 25% further out; -0.25 = 25% closer (zoom-in on guard/lock-on).
//
//  7. BuildMountHeightMods()  [optional]
//     Matches horse camera height to the player's UpOffset setting.
//
// ── Lock-on zoom problem & fix ───────────────────────────────────────────────
//
//  Problem: When you lock onto a target the game switches to a lock-on camera
//  section (Player_Weapon_LockOn, Player_Weapon_TwoTarget, etc.). Vanilla these
//  sections have hardcoded ZoomDistance values much lower than on-foot, AND
//  MaxZoomDistance ceilings (e.g. 5) that clamp the camera inward. At a custom
//  distance of 12 the on-foot camera is at ZL3=12 but lock-on snaps to ZL3=4
//  with a ceiling of 5 — a brutal zoom-in.
//
//  Fix applied in two places:
//  a) BuildSmoothing sets MaxZoomDistance=30 on all lock-on/finisher ZLs
//     (a non-constraining ceiling — no gameplay downside).
//  b) BuildLockOnDistances sets ZoomDistance to match the style's on-foot
//     values, called by every style builder and seeded as a default in
//     BuildModifications before the style layer runs.
//
// ── Sections that are NOT in AllMain but need distance scaling ───────────────
//
//  Player_Weapon_LockOn, Player_Weapon_TwoTarget, Player_Weapon_LockOn_System,
//  Player_Revive_LockOn_System, Player_FollowLearn_LockOn_Boss,
//  Player_Weapon_LockOn_Non_Rotate, Player_Weapon_LockOn_WrestleOnly,
//  Player_Interaction_TwoTarget  → handled by BuildLockOnDistances (ZL2/3/4)
//
//  Player_Interaction_LockOn, Interaction_LookAt  → NPC dialogue camera (LB+X).
//  Intentionally NOT in LockOnSections: scaling ZoomDistance / MaxZoomDistance / injecting
//  ZL2–4 here has correlated with game crashes on load (same class of risk as SilenceKill).
//  Leave dialogue camera vanilla; combat lock-on scaling remains on LockOnSections below.
//
//  Player_SilenceKill, Player_SilenceKill_Back  → stealth finishers, only ZL2,
//  vanilla ZoomDistance=1.2 (extreme close-up).
//  !! DO NOT MODIFY THESE SECTIONS !! Changing any attribute (ZoomDistance,
//  MaxZoomDistance, or CameraBlendParameter) on either SilenceKill section
//  causes an immediate game crash on load. Confirmed via bisect testing.
//  Root cause unknown — likely the engine treats these sections specially.
//  Leave them at vanilla values.
//
//  Player_Weapon_Down  → combat finisher when enemy is downed, vanilla 3/4/6
//  fixed distances with BlendInTime=0.5 (hard snap). Scaled via
//  BuildLockOnDistances; blend softened in BuildSmoothing.
//
//  Player_Weapon_Guard  → IS in AllMain (WeaponSections) so it gets distance
//  scaling from the style builder automatically. Guard distance matches on-foot.
//
// ── XML injection ────────────────────────────────────────────────────────────
//
//  Some lock-on sections only have ZL1 or ZL2 in vanilla. When BuildSmoothing
//  or BuildLockOnDistances targets ZL3/ZL4 on those sections, CameraMod.cs
//  auto-injects the missing ZoomLevel nodes when it closes the ZoomLevelInfo
//  block. This is intentional — all lock-on sections should have ZL2/3/4 so
//  the game never forces a zoom level change on transition.
//  CAUTION: injected nodes increase XML payload size. The compressed result
//  must fit within the original PAZ slot (orig_size). If the game crashes on
//  load after a fresh Steam verify, check install_trace.txt — comp_size must
//  be <= orig_size. If it exceeds it, reduce the number of injections.
//
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Complete camera modification rule engine. Port of camera_rules.py.
/// All presets, styles, section lists, and the layered composition system.
/// </summary>
public sealed class ModificationSet
{
    public Dictionary<string, Dictionary<string, (string Action, string Value)>> ElementMods { get; set; } = new();
    public int FovValue { get; set; }
}

public static class CameraRules
{
    // ── Section lists ────────────────────────────────────────────────

    private static readonly string[] BasicSections =
    {
        "Player_Basic_Default",
        "Player_Basic_Default_Walk",
        "Player_Basic_Default_Run",
        "Player_Basic_Default_Runfast",
    };

    private static readonly string[] WeaponSections =
    {
        "Player_Weapon_Default",
        "Player_Weapon_Default_Walk",
        "Player_Weapon_Default_Run",
        "Player_Weapon_Default_RunFast",
        "Player_Weapon_Default_RunFast_Follow",
        "Player_Weapon_Rush",
        "Player_Weapon_Guard",
    };

    private static readonly string[] WalkRun =
    {
        "Player_Basic_Default_Walk",
        "Player_Basic_Default_Run",
        "Player_Basic_Default_Runfast",
        "Player_Weapon_Default_Walk",
        "Player_Weapon_Default_Run",
        "Player_Weapon_Default_RunFast",
        "Player_Weapon_Default_RunFast_Follow",
    };

    private static readonly string[] AllMain = BasicSections.Concat(WeaponSections).ToArray();

    private static readonly string[] LockOnSections =
    {
        "Player_Weapon_LockOn",
        "Player_Weapon_TwoTarget",
        "Player_Weapon_LockOn_System",
        "Player_Revive_LockOn_System",
        "Player_FollowLearn_LockOn_Boss",
        "Player_Weapon_LockOn_Non_Rotate",
        "Player_Weapon_LockOn_WrestleOnly",
        "Player_Interaction_TwoTarget",
    };

    private static readonly string[] BaneSections =
        BasicSections.Concat(WeaponSections).ToArray();

    private static readonly string[] HorseRideSections =
    {
        "Player_Ride_Horse",
        "Player_Ride_Horse_Run",
        "Player_Ride_Horse_Fast_Run",
        "Player_Ride_Horse_Dash",
        "Player_Ride_Horse_Dash_Att",
        "Player_Ride_Horse_Att_Thrust",
        "Player_Ride_Horse_Att_R",
        "Player_Ride_Horse_Att_L",
    };

    private static readonly string[] AllMountSections = HorseRideSections.Concat(new[]
    {
        "Player_Ride_Elephant",
        "Player_Ride_Wyvern",
        "Player_Ride_Canoe",
        "Player_Ride_Warmachine",
        "Player_Ride_Broom",
    }).ToArray();

    // ── Helpers ──────────────────────────────────────────────────────

    private static void Set(Dictionary<string, Dictionary<string, (string, string)>> mods,
        string key, string attr, string value)
    {
        if (!mods.TryGetValue(key, out var dict))
        {
            dict = new Dictionary<string, (string, string)>();
            mods[key] = dict;
        }
        dict[attr] = ("SET", value);
    }

    private static void Merge(Dictionary<string, Dictionary<string, (string, string)>> baseDict,
        Dictionary<string, Dictionary<string, (string, string)>> overlay)
    {
        foreach (var (key, attrs) in overlay)
        {
            if (!baseDict.TryGetValue(key, out var existing))
            {
                baseDict[key] = new Dictionary<string, (string, string)>(attrs);
            }
            else
            {
                foreach (var (attr, val) in attrs)
                    existing[attr] = val;
            }
        }
    }

    // ── Mount helpers ────────────────────────────────────────────────

    private static Dictionary<string, Dictionary<string, (string, string)>> BuildMountHeightMods(double upOffset)
    {
        var mods = new Dictionary<string, Dictionary<string, (string, string)>>();
        string upStr = $"{upOffset}";
        foreach (var sec in HorseRideSections)
            for (int level = 2; level <= 3; level++)
                Set(mods, $"{sec}/ZoomLevel[{level}]", "UpOffset", upStr);

        foreach (var sec in new[] { "Player_Ride_Elephant", "Player_Ride_Canoe", "Player_Ride_Warmachine", "Player_Ride_Broom" })
            for (int level = 2; level <= 3; level++)
                Set(mods, $"{sec}/ZoomLevel[{level}]", "UpOffset", upStr);

        for (int level = 2; level <= 4; level++)
            Set(mods, $"Player_Ride_Wyvern/ZoomLevel[{level}]", "UpOffset", upStr);

        return mods;
    }

    private static Dictionary<string, Dictionary<string, (string, string)>> BuildMountDistances(double scale)
    {
        var mods = new Dictionary<string, Dictionary<string, (string, string)>>();

        foreach (var sec in HorseRideSections)
        {
            Set(mods, $"{sec}/ZoomLevel[2]", "ZoomDistance", $"{7.5 * scale:F1}");
            Set(mods, $"{sec}/ZoomLevel[3]", "ZoomDistance", $"{10.5 * scale:F1}");
        }
        Set(mods, "Player_Ride_Elephant/ZoomLevel[2]", "ZoomDistance", $"{8 * scale:F1}");
        Set(mods, "Player_Ride_Elephant/ZoomLevel[3]", "ZoomDistance", $"{11 * scale:F1}");
        Set(mods, "Player_Ride_Wyvern/ZoomLevel[2]", "ZoomDistance", $"{12 * scale:F1}");
        Set(mods, "Player_Ride_Wyvern/ZoomLevel[3]", "ZoomDistance", $"{16 * scale:F1}");
        Set(mods, "Player_Ride_Wyvern/ZoomLevel[4]", "ZoomDistance", $"{20 * scale:F1}");
        Set(mods, "Player_Ride_Canoe/ZoomLevel[2]", "ZoomDistance", $"{6 * scale:F1}");
        Set(mods, "Player_Ride_Canoe/ZoomLevel[3]", "ZoomDistance", $"{9 * scale:F1}");
        Set(mods, "Player_Ride_Warmachine/ZoomLevel[2]", "ZoomDistance", $"{9 * scale:F1}");
        Set(mods, "Player_Ride_Warmachine/ZoomLevel[3]", "ZoomDistance", $"{11 * scale:F1}");
        Set(mods, "Player_Ride_Broom/ZoomLevel[2]", "ZoomDistance", $"{10 * scale:F1}");
        Set(mods, "Player_Ride_Broom/ZoomLevel[3]", "ZoomDistance", $"{14 * scale:F1}");
        return mods;
    }

    // ── Shared base ──────────────────────────────────────────────────

    private static Dictionary<string, Dictionary<string, (string, string)>> BuildSharedBase()
    {
        var m = new Dictionary<string, Dictionary<string, (string, string)>>();

        // FoV normalization
        foreach (var sec in new[]
        {
            "Player_Basic_Default_Run", "Player_Basic_Default_Runfast", "Player_Basic_Default_Walk",
            "Player_Weapon_Default", "Player_Weapon_Default_Run", "Player_Weapon_Default_RunFast",
            "Player_Weapon_Default_RunFast_Follow", "Player_Weapon_Default_Walk",
            "Player_Weapon_Rush", "Player_Force_LockOn", "Player_LockOn_Titan",
            "Cinematic_LockOn", "Player_Weapon_Down", "Player_Weapon_Throw",
            "Player_Weapon_Throwed", "Player_Weapon_CatchThrow",
            "Player_Weapon_Zoom", "Player_Weapon_Zoom_Light", "Player_Weapon_Zoom_Out",
            "Player_Weapon_Aim_BossAttack", "Player_Weapon_Aim_SmallBossAttack",
            "Player_Ride_Aim_LockOn", "Player_PushingObject_TwoTarget",
            "Player_Ride_Warmachine", "Player_Ride_Warmachine_Aim",
            "Player_Ride_Warmachine_Dash", "Player_Ride_Broom",
            "Player_Swim_Default",
        })
            Set(m, sec, "Fov", "40");

        // Lock-on behavior (ZoomDistance values are handled by BuildSmoothing when Steadycam is on)
        Set(m, "Player_Weapon_LockOn", "Fov", "40");
        Set(m, "Player_Weapon_LockOn", "TargetRate", "0.25");
        Set(m, "Player_Weapon_LockOn", "ScreenClampRate", "0.6");

        Set(m, "Player_Weapon_TwoTarget", "Fov", "40");
        Set(m, "Player_Weapon_TwoTarget", "TargetRate", "0.25");
        Set(m, "Player_Weapon_TwoTarget", "ScreenClampRate", "0.6");
        Set(m, "Player_Weapon_TwoTarget", "LimitUnderDistance", "3");

        Set(m, "Player_Interaction_TwoTarget", "Fov", "40");
        Set(m, "Player_Interaction_TwoTarget", "TargetRate", "0.45");
        Set(m, "Player_Interaction_TwoTarget", "ScreenClampRate", "0.65");
        Set(m, "Player_Interaction_TwoTarget/ZoomLevel[3]", "MaxZoomDistance", "10");
        Set(m, "Player_Interaction_TwoTarget/ZoomLevel[4]", "MaxZoomDistance", "10");

        Set(m, "Player_FollowLearn_LockOn_Boss", "Fov", "40");
        Set(m, "Player_FollowLearn_LockOn_Boss", "ScreenClampRate", "0.7");

        Set(m, "Player_Weapon_LockOn_System", "Fov", "40");
        Set(m, "Player_Weapon_LockOn_System", "TargetRate", "0.3");
        Set(m, "Player_Weapon_LockOn_System", "ScreenClampRate", "0.65");
        Set(m, "Player_Weapon_LockOn_System/ZoomLevel[3]", "Fov", "40");
        Set(m, "Player_Weapon_LockOn_System/ZoomLevel[4]", "Fov", "40");

        Set(m, "Player_Revive_LockOn_System", "Fov", "40");
        Set(m, "Player_Revive_LockOn_System", "ScreenClampRate", "0.65");
        Set(m, "Player_Revive_LockOn_System/ZoomLevel[3]", "Fov", "40");
        Set(m, "Player_Revive_LockOn_System/ZoomLevel[4]", "Fov", "40");

        Set(m, "Player_Weapon_LockOn_Non_Rotate", "Fov", "40");
        Set(m, "Player_Weapon_LockOn_Non_Rotate", "ScreenClampRate", "0.6");

        Set(m, "Player_Weapon_LockOn_WrestleOnly", "Fov", "40");
        Set(m, "Player_Weapon_LockOn_WrestleOnly", "ScreenClampRate", "0.6");

        Set(m, "Player_StartAggro_TwoTarget", "Fov", "40");
        Set(m, "Player_StartAggro_TwoTarget", "ScreenClampRate", "0.6");

        Set(m, "Player_Wanted_TwoTarget", "Fov", "40");
        Set(m, "Player_Wanted_TwoTarget", "ScreenClampRate", "0.6");

        // On-foot ZoomDistance normalization (includes idle to prevent idle→walk zoom jumps)
        // Also normalize ZoomLevel Fov to 40 so the FoV slider delta applies consistently
        // at every zoom level. Vanilla ZL3/ZL4 often carry Fov="53" which would otherwise
        // produce a different effective FoV when the user zooms out.
        foreach (var sec in new[] { "Player_Basic_Default", "Player_Basic_Default_Walk", "Player_Basic_Default_Run", "Player_Basic_Default_Runfast" })
        {
            Set(m, $"{sec}/ZoomLevel[2]", "ZoomDistance", "3.4");
            Set(m, $"{sec}/ZoomLevel[2]", "Fov", "40");
            Set(m, $"{sec}/ZoomLevel[3]", "ZoomDistance", "6");
            Set(m, $"{sec}/ZoomLevel[3]", "Fov", "40");
            Set(m, $"{sec}/ZoomLevel[4]", "ZoomDistance", "8");
            Set(m, $"{sec}/ZoomLevel[4]", "Fov", "40");
        }

        // Combat ZoomDistance normalization (includes idle so guard→idle→walk has no zoom gap)
        foreach (var sec in new[] { "Player_Weapon_Default",
                 "Player_Weapon_Default_Walk", "Player_Weapon_Default_Run",
                 "Player_Weapon_Default_RunFast", "Player_Weapon_Default_RunFast_Follow" })
        {
            Set(m, $"{sec}/ZoomLevel[2]", "ZoomDistance", "3.4");
            Set(m, $"{sec}/ZoomLevel[2]", "Fov", "40");
            Set(m, $"{sec}/ZoomLevel[3]", "ZoomDistance", "6");
            Set(m, $"{sec}/ZoomLevel[3]", "Fov", "40");
            Set(m, $"{sec}/ZoomLevel[4]", "ZoomDistance", "8");
            Set(m, $"{sec}/ZoomLevel[4]", "Fov", "40");
        }

        Set(m, "Player_Weapon_Guard", "Fov", "40");
        foreach (var sec in new[] { "Player_Weapon_Guard", "Player_Weapon_Rush" })
        {
            Set(m, $"{sec}/ZoomLevel[2]", "ZoomDistance", "3.4");
            Set(m, $"{sec}/ZoomLevel[2]", "Fov", "40");
            Set(m, $"{sec}/ZoomLevel[3]", "ZoomDistance", "6");
            Set(m, $"{sec}/ZoomLevel[3]", "Fov", "40");
            Set(m, $"{sec}/ZoomLevel[4]", "ZoomDistance", "8");
            Set(m, $"{sec}/ZoomLevel[4]", "Fov", "40");
        }

        // RightOffset normalization (prevents horizontal drift during walk/run/guard)
        foreach (var sec in new[] {
            "Player_Basic_Default_Walk", "Player_Basic_Default_Run",
            "Player_Basic_Default_Runfast" })
            Set(m, $"{sec}/ZoomLevel[2]", "RightOffset", "0.5");

        foreach (var sec in new[] {
            "Player_Weapon_Default", "Player_Weapon_Default_Walk",
            "Player_Weapon_Default_Run", "Player_Weapon_Default_RunFast",
            "Player_Weapon_Default_RunFast_Follow",
            "Player_Weapon_Guard", "Player_Weapon_Rush" })
            Set(m, $"{sec}/ZoomLevel[2]", "RightOffset", "0.5");

        // Mount FoV normalization -- ALL horse states to 40 to prevent FoV pops on transitions
        foreach (var sec in HorseRideSections)
            Set(m, sec, "Fov", "40");
        Set(m, "Player_Ride_Elephant", "Fov", "40");
        Set(m, "Player_Ride_Wyvern", "Fov", "50");

        // Lock-on MaxZoomDistance -- vanilla values (often 5) clamp the camera hard,
        // overriding any ZoomDistance we set. Raise the ceiling unconditionally so the
        // lock-on offset and style distances can actually take effect regardless of
        // whether Steadycam is enabled.
        foreach (var sec in LockOnSections)
        {
            Set(m, $"{sec}/ZoomLevel[1]", "MaxZoomDistance", "30");
            Set(m, $"{sec}/ZoomLevel[2]", "MaxZoomDistance", "30");
            Set(m, $"{sec}/ZoomLevel[3]", "MaxZoomDistance", "30");
            Set(m, $"{sec}/ZoomLevel[4]", "MaxZoomDistance", "30");
        }
        Set(m, "Player_Ride_Aim_LockOn/ZoomLevel[1]", "MaxZoomDistance", "30");
        Set(m, "Player_Ride_Aim_LockOn/ZoomLevel[2]", "MaxZoomDistance", "30");
        Set(m, "Player_Ride_Aim_LockOn/ZoomLevel[3]", "MaxZoomDistance", "30");

        return m;
    }

    // ── Smoothing layer (steadycam) ───────────────────────────────────

    private static Dictionary<string, Dictionary<string, (string, string)>> BuildSmoothing()
    {
        var m = new Dictionary<string, Dictionary<string, (string, string)>>();

        // On-foot idle/walk/run blend -- smooths the run→stop zoom-out snap.
        // Vanilla Player_Basic_Default (idle) has no BlendInTime so when the player
        // stops running the camera snaps back to the idle position immediately.
        Set(m, "Player_Basic_Default/CameraBlendParameter", "BlendInTime", "0.6");
        Set(m, "Player_Basic_Default/CameraBlendParameter", "BlendOutTime", "0.4");
        Set(m, "Player_Basic_Default_Walk/CameraBlendParameter", "BlendInTime", "0.4");
        Set(m, "Player_Basic_Default_Walk/CameraBlendParameter", "BlendOutTime", "0.3");
        Set(m, "Player_Basic_Default_Run/CameraBlendParameter", "BlendInTime", "0.4");
        Set(m, "Player_Basic_Default_Run/CameraBlendParameter", "BlendOutTime", "0.4");
        Set(m, "Player_Basic_Default_Runfast/CameraBlendParameter", "BlendInTime", "0.4");
        Set(m, "Player_Basic_Default_Runfast/CameraBlendParameter", "BlendOutTime", "0.4");

        // Guard blend: smooth enter/exit to prevent zoom snap on release
        Set(m, "Player_Weapon_Guard/CameraBlendParameter", "BlendInTime", "1.0");
        Set(m, "Player_Weapon_Guard/CameraBlendParameter", "BlendOutTime", "1.0");

        // OffsetByVelocity elimination (removes velocity-based camera sway)
        Set(m, "Player_Basic_Default_Run/OffsetByVelocity", "OffsetLength", "0");
        Set(m, "Player_Basic_Default_Runfast/OffsetByVelocity", "OffsetLength", "0.0");
        Set(m, "Player_Weapon_Default_Run/OffsetByVelocity", "OffsetLength", "0");
        Set(m, "Player_Weapon_Default_RunFast/OffsetByVelocity", "OffsetLength", "0.0");
        Set(m, "Player_Weapon_Default_RunFast_Follow/OffsetByVelocity", "OffsetLength", "0.0");

        // Animal mount smoothing
        Set(m, "Player_Animal_Default/CameraBlendParameter", "BlendInTime", "0.3");
        Set(m, "Player_Animal_Default_Run/CameraBlendParameter", "BlendInTime", "0.3");
        Set(m, "Player_Animal_Default_Run/OffsetByVelocity", "OffsetLength", "0");
        Set(m, "Player_Animal_Default_Runfast/CameraBlendParameter", "BlendInTime", "0.3");
        Set(m, "Player_Animal_Default_Runfast/OffsetByVelocity", "OffsetLength", "0.0");
        Set(m, "Player_Animal_Default_Runfast/OffsetByVelocity", "DampSpeed", "0.5");
        Set(m, "Player_Animal_Default_Walk/CameraBlendParameter", "BlendInTime", "0.3");

        // Horse normalization -- flatten ALL ride states so transitions are invisible.
        // Uniform follow rates, blend times, velocity offset, damping, pitch, and
        // lateral position (RightOffset) across every horse sub-state.
        foreach (var sec in HorseRideSections)
        {
            Set(m, sec, "FollowYawSpeedRate", "0.8");
            Set(m, sec, "FollowPitchSpeedRate", "0.8");
            Set(m, sec, "FollowStartTime", "1");
            Set(m, sec, "FollowDefaultPitch", "13");
            Set(m, $"{sec}/CameraBlendParameter", "BlendInTime", "1.0");
            Set(m, $"{sec}/CameraBlendParameter", "BlendOutTime", "1.0");
            Set(m, $"{sec}/CameraDamping", "PivotDampingMaxDistance", "0.5");
            Set(m, $"{sec}/OffsetByVelocity", "OffsetLength", "0.0");
            Set(m, $"{sec}/OffsetByVelocity", "DampSpeed", "0.5");
            Set(m, $"{sec}/ZoomLevel[2]", "RightOffset", "1.45");
            Set(m, $"{sec}/ZoomLevel[3]", "RightOffset", "1.8");
        }

        // Elephant
        Set(m, "Player_Ride_Elephant", "FollowPitchSpeedRate", "0.8");
        Set(m, "Player_Ride_Elephant", "FollowYawSpeedRate", "0.8");
        Set(m, "Player_Ride_Elephant/CameraBlendParameter", "BlendInTime", "1.0");
        Set(m, "Player_Ride_Elephant/CameraBlendParameter", "BlendOutTime", "1.0");
        Set(m, "Player_Ride_Elephant/CameraDamping", "PivotDampingMaxDistance", "0.5");
        Set(m, "Player_Ride_Elephant/OffsetByVelocity", "OffsetLength", "0.0");

        // Wyvern
        Set(m, "Player_Ride_Wyvern", "FollowStartTime", "1");
        Set(m, "Player_Ride_Wyvern", "FollowYawSpeedRate", "0.8");
        Set(m, "Player_Ride_Wyvern/OffsetByVelocity", "OffsetLength", "0.0");

        // Mount ZoomDistance normalization -- only target zoom levels that exist in
        // vanilla to avoid injecting phantom ZL elements. Horse/Elephant have ZL2+ZL3;
        // Wyvern has ZL2+ZL3+ZL4.
        foreach (var sec in HorseRideSections)
        {
            Set(m, $"{sec}/ZoomLevel[2]", "ZoomDistance", "7.5");
            Set(m, $"{sec}/ZoomLevel[3]", "ZoomDistance", "10.5");
        }
        Set(m, "Player_Ride_Elephant/ZoomLevel[2]", "ZoomDistance", "8.0");
        Set(m, "Player_Ride_Elephant/ZoomLevel[3]", "ZoomDistance", "11.0");
        Set(m, "Player_Ride_Wyvern/ZoomLevel[2]", "ZoomDistance", "12.0");
        Set(m, "Player_Ride_Wyvern/ZoomLevel[3]", "ZoomDistance", "16.0");
        Set(m, "Player_Ride_Wyvern/ZoomLevel[4]", "ZoomDistance", "20.0");


        // Combat finisher (Player_Weapon_Down) -- vanilla BlendInTime=0.5 snaps hard on kill
        Set(m, "Player_Weapon_Down/CameraBlendParameter", "BlendInTime", "1.2");
        Set(m, "Player_Weapon_Down/CameraBlendParameter", "BlendOutTime", "1.5");

        // Rush / charge -- vanilla 0.25 is extremely jarring when entering a charge attack
        Set(m, "Player_Weapon_Rush/CameraBlendParameter", "BlendInTime", "0.6");
        Set(m, "Player_Weapon_Rush/CameraBlendParameter", "BlendOutTime", "0.6");

        // Freefall -- vanilla 0.65 snaps on jump-off; ease in smoothly
        Set(m, "Player_Basic_FreeFall_Start/CameraBlendParameter", "BlendInTime", "1.0");
        Set(m, "Player_Basic_FreeFall_Start/CameraBlendParameter", "BlendOutTime", "1.2");
        Set(m, "Player_Basic_FreeFall/CameraBlendParameter", "BlendInTime", "1.0");
        Set(m, "Player_Basic_FreeFall/CameraBlendParameter", "BlendOutTime", "1.2");

        // Super jump -- vanilla 0.5 snaps on launch
        Set(m, "Player_Basic_SuperJump/CameraBlendParameter", "BlendInTime", "0.8");
        Set(m, "Player_Basic_SuperJump/CameraBlendParameter", "BlendOutTime", "1.2");

        // Rope pull / swing -- vanilla 0.5 snaps on grab
        Set(m, "Player_Basic_RopePull/CameraBlendParameter", "BlendInTime", "0.8");
        Set(m, "Player_Basic_RopePull/CameraBlendParameter", "BlendOutTime", "1.2");
        Set(m, "Player_Basic_RopeSwing/CameraBlendParameter", "BlendInTime", "0.8");
        Set(m, "Player_Basic_RopeSwing/CameraBlendParameter", "BlendOutTime", "1.2");

        // Hit / thrown -- vanilla 0.5 snaps when knocked back
        Set(m, "Player_Hit_Throw/CameraBlendParameter", "BlendInTime", "0.8");
        Set(m, "Player_Hit_Throw/CameraBlendParameter", "BlendOutTime", "1.2");

        // Warmachine aim / dash -- vanilla 0.5 snaps on weapon draw / dash
        Set(m, "Player_Ride_Warmachine_Aim/CameraBlendParameter", "BlendInTime", "0.8");
        Set(m, "Player_Ride_Warmachine_Aim/CameraBlendParameter", "BlendOutTime", "1.0");
        Set(m, "Player_Ride_Warmachine_Dash/CameraBlendParameter", "BlendInTime", "0.8");
        Set(m, "Player_Ride_Warmachine_Dash/CameraBlendParameter", "BlendOutTime", "1.0");

        // Mount lock-on -- vanilla 0.5 snaps when locking on from horseback
        Set(m, "Player_Ride_Aim_LockOn/CameraBlendParameter", "BlendInTime", "1.0");
        Set(m, "Player_Ride_Aim_LockOn/CameraBlendParameter", "BlendOutTime", "1.2");

        // Lock-on blend smoothing -- soften transitions into and out of lock-on cameras
        Set(m, "Player_Weapon_LockOn/CameraBlendParameter", "BlendInTime", "1.25");
        Set(m, "Player_Weapon_LockOn/CameraBlendParameter", "BlendOutTime", "1.2");
        Set(m, "Player_Weapon_TwoTarget/CameraBlendParameter", "BlendInTime", "1.0");
        Set(m, "Player_Weapon_TwoTarget/CameraBlendParameter", "BlendOutTime", "1.2");
        Set(m, "Player_Weapon_LockOn_System/CameraBlendParameter", "BlendInTime", "1.0");
        Set(m, "Player_Weapon_LockOn_System/CameraBlendParameter", "BlendOutTime", "1.0");
        Set(m, "Player_FollowLearn_LockOn_Boss/CameraBlendParameter", "BlendInTime", "0.6");
        Set(m, "Player_FollowLearn_LockOn_Boss/CameraBlendParameter", "BlendOutTime", "1.0");
        Set(m, "Player_Interaction_TwoTarget/CameraBlendParameter", "BlendInTime", "1.0");
        Set(m, "Player_Interaction_TwoTarget/CameraBlendParameter", "BlendOutTime", "1.0");

        // Extended lock-on coverage -- sections not previously smoothed
        Set(m, "Player_Revive_LockOn_System/CameraBlendParameter", "BlendInTime", "0.8");
        Set(m, "Player_Revive_LockOn_System/CameraBlendParameter", "BlendOutTime", "1.0");
        Set(m, "Player_Force_LockOn/CameraBlendParameter", "BlendInTime", "0.8");
        Set(m, "Player_Force_LockOn/CameraBlendParameter", "BlendOutTime", "1.2");
        Set(m, "Player_LockOn_Titan/CameraBlendParameter", "BlendInTime", "1.0");
        Set(m, "Player_LockOn_Titan/CameraBlendParameter", "BlendOutTime", "1.2");
        Set(m, "Player_Weapon_LockOn_Non_Rotate/CameraBlendParameter", "BlendInTime", "1.0");
        Set(m, "Player_Weapon_LockOn_Non_Rotate/CameraBlendParameter", "BlendOutTime", "1.2");
        Set(m, "Player_Weapon_LockOn_WrestleOnly/CameraBlendParameter", "BlendInTime", "0.8");
        Set(m, "Player_Weapon_LockOn_WrestleOnly/CameraBlendParameter", "BlendOutTime", "1.2");
        Set(m, "Player_StartAggro_TwoTarget/CameraBlendParameter", "BlendInTime", "0.8");
        Set(m, "Player_StartAggro_TwoTarget/CameraBlendParameter", "BlendOutTime", "1.0");
        Set(m, "Player_Wanted_TwoTarget/CameraBlendParameter", "BlendInTime", "0.8");
        Set(m, "Player_Wanted_TwoTarget/CameraBlendParameter", "BlendOutTime", "1.0");

        return m;
    }

    // ── Shared steadycam ─────────────────────────────────────────────

    private static Dictionary<string, Dictionary<string, (string, string)>> BuildSharedSteadycam()
    {
        var m = new Dictionary<string, Dictionary<string, (string, string)>>();

        Set(m, "Player_Basic_Default/ZoomLevel[2]", "UpOffset", "0.0");
        Set(m, "Player_Basic_Default/ZoomLevel[3]", "UpOffset", "0.0");
        Set(m, "Player_Basic_Default/ZoomLevel[4]", "UpOffset", "0.0");
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[3]", "UpOffset", "0.0");

        foreach (var sec in new[] { "Player_Basic_Default_Walk", "Player_Basic_Default_Run", "Player_Basic_Default_Runfast" })
        {
            Set(m, $"{sec}/ZoomLevel[2]", "UpOffset", "0.0");
            Set(m, $"{sec}/ZoomLevel[3]", "UpOffset", "0.0");
            Set(m, $"{sec}/ZoomLevel[4]", "UpOffset", "0.0");
        }

        foreach (var sec in WeaponSections)
        {
            Set(m, $"{sec}/ZoomLevel[2]", "UpOffset", "0.0");
            Set(m, $"{sec}/ZoomLevel[3]", "UpOffset", "0.0");
            Set(m, $"{sec}/ZoomLevel[4]", "UpOffset", "0.0");
        }

        return m;
    }

    // ── Steadycam key set ────────────────────────────────────────────

    /// <summary>
    /// Returns all "modKey.attr" strings that BuildSmoothing() and BuildSharedSteadycam()
    /// control. Used by the fine-tune UI to disable sliders when Steadycam is active.
    /// </summary>
    public static HashSet<string> GetSteadycamKeys()
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (modKey, attrs) in BuildSmoothing())
            foreach (var (attr, _) in attrs)
                keys.Add($"{modKey}.{attr}");
        foreach (var (modKey, attrs) in BuildSharedSteadycam())
            foreach (var (attr, _) in attrs)
                keys.Add($"{modKey}.{attr}");
        return keys;
    }

    // ── Lock-on distance helper ──────────────────────────────────────

    /// <summary>
    /// Sets ZoomDistance on all lock-on sections to match the given on-foot distances.
    /// Called by every style builder (private) and exposed publicly so the Fine Tune
    /// layer can re-sync lock-on after user overrides are applied.
    /// </summary>
    public static Dictionary<string, Dictionary<string, (string, string)>> BuildLockOnDistancesPublic(
        double zl2, double zl3, double zl4)
        => BuildLockOnDistances(zl2, zl3, zl4);

    private static Dictionary<string, Dictionary<string, (string, string)>> BuildLockOnDistances(
        double zl2, double zl3, double zl4)
    {
        var m = new Dictionary<string, Dictionary<string, (string, string)>>();
        string zl2s = $"{zl2}";
        string zl3s = $"{zl3}";
        string zl4s = $"{zl4}";
        foreach (var sec in LockOnSections)
        {
            Set(m, $"{sec}/ZoomLevel[2]", "ZoomDistance", zl2s);
            Set(m, $"{sec}/ZoomLevel[3]", "ZoomDistance", zl3s);
            Set(m, $"{sec}/ZoomLevel[4]", "ZoomDistance", zl4s);
        }
        // Player_Weapon_Down distance scaling -- scale with user's distance
        Set(m, "Player_Weapon_Down/ZoomLevel[2]", "ZoomDistance", zl2s);
        Set(m, "Player_Weapon_Down/ZoomLevel[3]", "ZoomDistance", zl3s);
        Set(m, "Player_Weapon_Down/ZoomLevel[4]", "ZoomDistance", zl4s);
        return m;
    }

    // ── Style layers ─────────────────────────────────────────────────

    private static Dictionary<string, Dictionary<string, (string, string)>> BuildHeroic()
    {
        var m = new Dictionary<string, Dictionary<string, (string, string)>>();
        foreach (var sec in AllMain)
        {
            Set(m, $"{sec}/ZoomLevel[2]", "UpOffset", "-0.2");
            Set(m, $"{sec}/ZoomLevel[2]", "InDoorUpOffset", "-0.2");
            Set(m, $"{sec}/ZoomLevel[2]", "ZoomDistance", "2.5");
            Set(m, $"{sec}/ZoomLevel[3]", "UpOffset", "-0.2");
            Set(m, $"{sec}/ZoomLevel[3]", "ZoomDistance", "5");
            Set(m, $"{sec}/ZoomLevel[4]", "UpOffset", "-0.2");
            Set(m, $"{sec}/ZoomLevel[4]", "ZoomDistance", "8");
        }
        Set(m, "Player_Weapon_Default/ZoomLevel[4]", "RightOffset", "0.8");
        foreach (var sec in WalkRun)
        {
            Set(m, $"{sec}/ZoomLevel[4]", "RightOffset", "0.8");
            if (sec.StartsWith("Player_Basic"))
                Set(m, $"{sec}/ZoomLevel[2]", "RightOffset", "0.5");
        }
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "UpOffset", "-0.2");
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "InDoorUpOffset", "-0.2");
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "ZoomDistance", "2.5");
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[3]", "UpOffset", "-0.2");
        Merge(m, BuildLockOnDistances(2.5, 5, 8));
        return m;
    }

    private static Dictionary<string, Dictionary<string, (string, string)>> BuildPanoramic()
    {
        var m = new Dictionary<string, Dictionary<string, (string, string)>>();
        foreach (var sec in AllMain)
        {
            Set(m, $"{sec}/ZoomLevel[2]", "UpOffset", "0.0");
            Set(m, $"{sec}/ZoomLevel[2]", "ZoomDistance", "3.75");
            Set(m, $"{sec}/ZoomLevel[3]", "UpOffset", "0.0");
            Set(m, $"{sec}/ZoomLevel[3]", "ZoomDistance", "7.5");
            Set(m, $"{sec}/ZoomLevel[4]", "UpOffset", "0.0");
            Set(m, $"{sec}/ZoomLevel[4]", "ZoomDistance", "11.25");
        }
        Set(m, "Player_Weapon_Default/ZoomLevel[4]", "RightOffset", "0.8");
        foreach (var sec in WalkRun)
        {
            Set(m, $"{sec}/ZoomLevel[4]", "RightOffset", "0.8");
            if (!sec.StartsWith("Player_Weapon"))
                Set(m, $"{sec}/ZoomLevel[2]", "RightOffset", "0.5");
        }
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "UpOffset", "0.0");
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "ZoomDistance", "3.75");
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[3]", "UpOffset", "0.0");
        Merge(m, BuildMountDistances(1.2));
        Merge(m, BuildLockOnDistances(3.75, 7.5, 11.25));
        return m;
    }

    private static Dictionary<string, Dictionary<string, (string, string)>> BuildCloseUp()
    {
        var m = new Dictionary<string, Dictionary<string, (string, string)>>();
        foreach (var sec in AllMain)
        {
            Set(m, $"{sec}/ZoomLevel[2]", "UpOffset", "-0.2");
            Set(m, $"{sec}/ZoomLevel[2]", "InDoorUpOffset", "-0.2");
            Set(m, $"{sec}/ZoomLevel[2]", "ZoomDistance", "2.0");
            Set(m, $"{sec}/ZoomLevel[3]", "UpOffset", "-0.2");
            Set(m, $"{sec}/ZoomLevel[3]", "ZoomDistance", "4.0");
            Set(m, $"{sec}/ZoomLevel[4]", "UpOffset", "-0.2");
            Set(m, $"{sec}/ZoomLevel[4]", "RightOffset", "0.8");
            Set(m, $"{sec}/ZoomLevel[4]", "ZoomDistance", "6.0");
        }
        foreach (var sec in WalkRun)
            Set(m, $"{sec}/ZoomLevel[2]", "RightOffset", "0.5");

        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "UpOffset", "-0.2");
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "InDoorUpOffset", "-0.2");
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "ZoomDistance", "2.0");
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[3]", "UpOffset", "-0.2");
        Merge(m, BuildMountDistances(0.75));
        Merge(m, BuildLockOnDistances(2.0, 4.0, 6.0));
        return m;
    }

    private static Dictionary<string, Dictionary<string, (string, string)>> BuildLowVariant(string baseUp, string? indoorUp = null)
    {
        indoorUp ??= baseUp;
        var m = new Dictionary<string, Dictionary<string, (string, string)>>();

        Set(m, "Player_Basic_Default/ZoomLevel[2]", "UpOffset", baseUp);
        Set(m, "Player_Basic_Default/ZoomLevel[2]", "InDoorUpOffset", baseUp);
        Set(m, "Player_Basic_Default/ZoomLevel[2]", "ZoomDistance", "2.5");
        Set(m, "Player_Basic_Default/ZoomLevel[3]", "UpOffset", baseUp);
        Set(m, "Player_Basic_Default/ZoomLevel[3]", "InDoorUpOffset", indoorUp);
        Set(m, "Player_Basic_Default/ZoomLevel[3]", "ZoomDistance", "5");
        Set(m, "Player_Basic_Default/ZoomLevel[4]", "UpOffset", baseUp);
        Set(m, "Player_Basic_Default/ZoomLevel[4]", "InDoorUpOffset", baseUp);
        Set(m, "Player_Basic_Default/ZoomLevel[4]", "ZoomDistance", "8");

        foreach (var sec in new[] { "Player_Basic_Default_Walk", "Player_Basic_Default_Run", "Player_Basic_Default_Runfast" })
        {
            Set(m, $"{sec}/ZoomLevel[2]", "UpOffset", baseUp);
            Set(m, $"{sec}/ZoomLevel[2]", "InDoorUpOffset", baseUp);
            Set(m, $"{sec}/ZoomLevel[2]", "RightOffset", "0.5");
            Set(m, $"{sec}/ZoomLevel[2]", "ZoomDistance", "2.5");
            Set(m, $"{sec}/ZoomLevel[3]", "UpOffset", baseUp);
            Set(m, $"{sec}/ZoomLevel[3]", "InDoorUpOffset", indoorUp);
            Set(m, $"{sec}/ZoomLevel[3]", "ZoomDistance", "5");
            Set(m, $"{sec}/ZoomLevel[4]", "UpOffset", baseUp);
            Set(m, $"{sec}/ZoomLevel[4]", "InDoorUpOffset", baseUp);
            Set(m, $"{sec}/ZoomLevel[4]", "RightOffset", "0.8000");
            Set(m, $"{sec}/ZoomLevel[4]", "ZoomDistance", "8");
        }

        Set(m, "Player_Weapon_Default/ZoomLevel[2]", "UpOffset", baseUp);
        Set(m, "Player_Weapon_Default/ZoomLevel[2]", "InDoorUpOffset", baseUp);
        Set(m, "Player_Weapon_Default/ZoomLevel[2]", "ZoomDistance", "2.5");
        Set(m, "Player_Weapon_Default/ZoomLevel[3]", "UpOffset", baseUp);
        Set(m, "Player_Weapon_Default/ZoomLevel[3]", "InDoorUpOffset", indoorUp);
        Set(m, "Player_Weapon_Default/ZoomLevel[3]", "ZoomDistance", "5");
        Set(m, "Player_Weapon_Default/ZoomLevel[4]", "UpOffset", baseUp);
        Set(m, "Player_Weapon_Default/ZoomLevel[4]", "InDoorUpOffset", baseUp);
        Set(m, "Player_Weapon_Default/ZoomLevel[4]", "RightOffset", "0.8000");
        Set(m, "Player_Weapon_Default/ZoomLevel[4]", "ZoomDistance", "8");

        foreach (var sec in new[] { "Player_Weapon_Default_Walk", "Player_Weapon_Default_Run",
                 "Player_Weapon_Default_RunFast", "Player_Weapon_Default_RunFast_Follow" })
        {
            Set(m, $"{sec}/ZoomLevel[2]", "UpOffset", baseUp);
            Set(m, $"{sec}/ZoomLevel[2]", "InDoorUpOffset", baseUp);
            Set(m, $"{sec}/ZoomLevel[2]", "ZoomDistance", "2.5");
            Set(m, $"{sec}/ZoomLevel[3]", "UpOffset", baseUp);
            Set(m, $"{sec}/ZoomLevel[3]", "InDoorUpOffset", indoorUp);
            Set(m, $"{sec}/ZoomLevel[3]", "ZoomDistance", "5");
            Set(m, $"{sec}/ZoomLevel[4]", "UpOffset", baseUp);
            Set(m, $"{sec}/ZoomLevel[4]", "InDoorUpOffset", baseUp);
            Set(m, $"{sec}/ZoomLevel[4]", "RightOffset", "0.8000");
            Set(m, $"{sec}/ZoomLevel[4]", "ZoomDistance", "8");
        }

        foreach (var sec in new[] { "Player_Weapon_Guard", "Player_Weapon_Rush" })
        {
            Set(m, $"{sec}/ZoomLevel[2]", "UpOffset", baseUp);
            Set(m, $"{sec}/ZoomLevel[2]", "InDoorUpOffset", baseUp);
            Set(m, $"{sec}/ZoomLevel[2]", "ZoomDistance", "2.5");
            Set(m, $"{sec}/ZoomLevel[3]", "UpOffset", baseUp);
            Set(m, $"{sec}/ZoomLevel[3]", "InDoorUpOffset", indoorUp);
            Set(m, $"{sec}/ZoomLevel[3]", "ZoomDistance", "5");
            Set(m, $"{sec}/ZoomLevel[4]", "UpOffset", baseUp);
            Set(m, $"{sec}/ZoomLevel[4]", "InDoorUpOffset", baseUp);
            Set(m, $"{sec}/ZoomLevel[4]", "RightOffset", "0.8000");
            Set(m, $"{sec}/ZoomLevel[4]", "ZoomDistance", "8");
        }

        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "UpOffset", baseUp);
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "InDoorUpOffset", baseUp);
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "ZoomDistance", "2.5");
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[3]", "UpOffset", baseUp);
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[3]", "InDoorUpOffset", indoorUp);
        Merge(m, BuildLockOnDistances(2.5, 5, 8));

        return m;
    }

    private static Dictionary<string, Dictionary<string, (string, string)>> BuildSurvival()
    {
        var m = new Dictionary<string, Dictionary<string, (string, string)>>();
        foreach (var sec in AllMain)
        {
            Set(m, $"{sec}/ZoomLevel[2]", "ZoomDistance", "1.8");
            Set(m, $"{sec}/ZoomLevel[3]", "ZoomDistance", "3");
            Set(m, $"{sec}/ZoomLevel[4]", "ZoomDistance", "6");
        }
        foreach (var sec in WalkRun)
        {
            Set(m, $"{sec}/ZoomLevel[2]", "RightOffset", "0.4");
            Set(m, $"{sec}/ZoomLevel[3]", "RightOffset", "0.7");
        }
        foreach (var sec in WalkRun.Concat(new[] { "Player_Weapon_Default" }))
            Set(m, $"{sec}/ZoomLevel[4]", "RightOffset", "0.7000");

        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "ZoomDistance", "1.8");
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[3]", "ZoomDistance", "3");
        Merge(m, BuildMountDistances(0.65));
        Merge(m, BuildLockOnDistances(1.8, 3, 6));
        return m;
    }

    // ── Bane layer ───────────────────────────────────────────────────

    private static Dictionary<string, Dictionary<string, (string, string)>> BuildBaneMods()
    {
        var m = new Dictionary<string, Dictionary<string, (string, string)>>();
        var indoorRightZl2 = new HashSet<string> { "Player_Basic_Default", "Player_Weapon_Default" };

        foreach (var sec in BaneSections)
        {
            for (int lvl = 2; lvl <= 4; lvl++)
            {
                string key = $"{sec}/ZoomLevel[{lvl}]";
                Set(m, key, "RightOffset", "0.0");
                if (lvl == 2 && indoorRightZl2.Contains(sec))
                    Set(m, key, "InDoorRightOffset", "0.0");
                if (lvl == 4 && sec == "Player_Basic_Default_Runfast")
                    Set(m, key, "InDoorRightOffset", "0.0");
            }
        }

        // Gliding and freefall
        foreach (var key in new[]
        {
            "Player_Basic_Gliding/ZoomLevel[2]", "Player_Basic_Gliding/ZoomLevel[3]",
            "Player_Basic_Gliding_Fast/ZoomLevel[2]", "Player_Basic_Gliding_Fast/ZoomLevel[3]",
            "Player_Basic_Gliding_Zoom/ZoomLevel[2]",
            "Player_Basic_Gliding_Fall/ZoomLevel[2]", "Player_Basic_Gliding_Fall/ZoomLevel[3]",
            "Glide_Kick_Aim_Zoom/ZoomLevel[2]", "Glide_Bow_Aim_Zoom/ZoomLevel[2]",
            "Player_Basic_FreeFall_Start/ZoomLevel[2]", "Player_Basic_FreeFall_Start/ZoomLevel[3]",
            "Player_Basic_FreeFall/ZoomLevel[2]", "Player_Basic_FreeFall/ZoomLevel[3]",
            "Player_Basic_FreeFall_Lv2/ZoomLevel[2]", "Player_Basic_FreeFall_Lv2/ZoomLevel[3]",
            "Player_Basic_FreeFall_Aim/ZoomLevel[2]", "Player_Basic_FreeFall_Aim/ZoomLevel[3]",
            "Player_Basic_SuperJump/ZoomLevel[2]", "Player_Basic_SuperJump/ZoomLevel[3]",
        })
            Set(m, key, "RightOffset", "0.0");

        // Swimming, climbing, traversal
        foreach (var key in new[]
        {
            "Player_Swim_Default/ZoomLevel[2]", "Player_Swim_Default/ZoomLevel[3]",
            "Player_Basic_PointClimb/ZoomLevel[2]", "Player_Basic_PointClimb/ZoomLevel[3]", "Player_Basic_PointClimb/ZoomLevel[4]",
            "Player_Basic_PointClimb_Follow/ZoomLevel[2]", "Player_Basic_PointClimb_Follow/ZoomLevel[3]", "Player_Basic_PointClimb_Follow/ZoomLevel[4]",
            "Player_Basic_CharacterClimb/ZoomLevel[2]", "Player_Basic_CharacterClimb/ZoomLevel[3]", "Player_Basic_CharacterClimb/ZoomLevel[4]",
            "Player_Basic_Climb/ZoomLevel[2]", "Player_Basic_Climb/ZoomLevel[3]",
            "Player_Basic_RopeSwing/ZoomLevel[2]", "Player_Basic_RopeSwing/ZoomLevel[3]", "Player_Basic_RopeSwing/ZoomLevel[4]",
            "Player_Basic_RopePull/ZoomLevel[2]", "Player_Basic_RopePull/ZoomLevel[3]", "Player_Basic_RopePull/ZoomLevel[4]",
            "Player_WaterFallPass/ZoomLevel[2]", "Player_WaterFallPass/ZoomLevel[3]", "Player_WaterFallPass/ZoomLevel[4]",
            "Player_Wood_Hanging/ZoomLevel[3]",
            "Player_Basic_Wagon/ZoomLevel[1]", "Player_Basic_Wagon/ZoomLevel[2]",
            "Player_Wagon_Wait/ZoomLevel[2]", "Player_Wagon_Wait/ZoomLevel[3]",
        })
            Set(m, key, "RightOffset", "0.0");

        // Combat states
        foreach (var key in new[]
        {
            "Player_Weapon_Guard/ZoomLevel[2]", "Player_Weapon_Guard/ZoomLevel[3]", "Player_Weapon_Guard/ZoomLevel[4]",
            "Player_Weapon_Rush/ZoomLevel[2]", "Player_Weapon_Rush/ZoomLevel[3]", "Player_Weapon_Rush/ZoomLevel[4]",
            "Player_Weapon_Down/ZoomLevel[2]", "Player_Weapon_Down/ZoomLevel[3]", "Player_Weapon_Down/ZoomLevel[4]",
            "Player_Weapon_Indoor/ZoomLevel[3]",
            "Player_Weapon_Zoom_Out/ZoomLevel[2]", "Player_Weapon_Zoom_Out/ZoomLevel[3]",
            "Player_Weapon_LockOn_System/ZoomLevel[2]", "Player_Weapon_LockOn_System/ZoomLevel[3]", "Player_Weapon_LockOn_System/ZoomLevel[4]",
            "Player_Revive_LockOn_System/ZoomLevel[2]", "Player_Revive_LockOn_System/ZoomLevel[3]", "Player_Revive_LockOn_System/ZoomLevel[4]",
            "Cinematic_LockOn/ZoomLevel[2]", "Cinematic_LockOn/ZoomLevel[3]", "Cinematic_LockOn/ZoomLevel[4]",
            "Player_Hit_Throw/ZoomLevel[2]", "Player_Hit_Throw/ZoomLevel[3]", "Player_Hit_Throw/ZoomLevel[4]",
        })
            Set(m, key, "RightOffset", "0.0");

        // Animal form
        foreach (var sec in new[] { "Player_Animal_Default", "Player_Animal_Default_Walk",
                 "Player_Animal_Default_Run", "Player_Animal_Default_Runfast" })
            for (int lvl = 2; lvl <= 4; lvl++)
                Set(m, $"{sec}/ZoomLevel[{lvl}]", "RightOffset", "0.0");

        // Misc
        foreach (var key in new[]
        {
            "Player_Rest/ZoomLevel[2]", "Player_Rest/ZoomLevel[3]", "Player_Rest/ZoomLevel[4]",
            "Player_Basic_NoZoom/ZoomLevel[3]", "Player_Basic_NoZoom/ZoomLevel[4]",
            "Player_Basic_Teleport/ZoomLevel[2]",
        })
            Set(m, key, "RightOffset", "0.0");

        // Ride and mount
        foreach (var key in new[]
        {
            "Player_Ride_Broom/ZoomLevel[2]", "Player_Ride_Broom/ZoomLevel[3]",
            "Player_Ride_Canoe/ZoomLevel[2]", "Player_Ride_Canoe/ZoomLevel[3]",
            "Player_Ride_Elephant/ZoomLevel[2]", "Player_Ride_Elephant/ZoomLevel[3]",
            "Player_Ride_Horse/ZoomLevel[2]", "Player_Ride_Horse/ZoomLevel[3]",
            "Player_Ride_Horse_Att_L/ZoomLevel[2]", "Player_Ride_Horse_Att_L/ZoomLevel[3]",
            "Player_Ride_Horse_Att_R/ZoomLevel[2]", "Player_Ride_Horse_Att_R/ZoomLevel[3]",
            "Player_Ride_Horse_Att_Thrust/ZoomLevel[2]", "Player_Ride_Horse_Att_Thrust/ZoomLevel[3]",
            "Player_Ride_Horse_Dash/ZoomLevel[2]", "Player_Ride_Horse_Dash/ZoomLevel[3]",
            "Player_Ride_Horse_Dash_Att/ZoomLevel[2]", "Player_Ride_Horse_Dash_Att/ZoomLevel[3]",
            "Player_Ride_Horse_Fast_Run/ZoomLevel[2]", "Player_Ride_Horse_Fast_Run/ZoomLevel[3]",
            "Player_Ride_Horse_Run/ZoomLevel[2]", "Player_Ride_Horse_Run/ZoomLevel[3]",
            "Player_Ride_Warmachine/ZoomLevel[2]", "Player_Ride_Warmachine/ZoomLevel[3]",
            "Player_Ride_Warmachine_Aim/ZoomLevel[2]",
            "Player_Ride_Warmachine_Dash/ZoomLevel[2]",
            "Player_Ride_Wyvern/ZoomLevel[2]", "Player_Ride_Wyvern/ZoomLevel[3]", "Player_Ride_Wyvern/ZoomLevel[4]",
        })
            Set(m, key, "RightOffset", "0.0");

        // Aim sections: keep positive offsets so aiming moves camera naturally
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "RightOffset", "0.50");
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[3]", "RightOffset", "0.60");
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[4]", "RightOffset", "0.60");

        Set(m, "Player_Taeguk_Aim/ZoomLevel[2]", "RightOffset", "0.50");
        Set(m, "Player_Taeguk_Aim/ZoomLevel[3]", "RightOffset", "0.60");

        Set(m, "Player_Weapon_Aim_Zoom/ZoomLevel[2]", "RightOffset", "0.80");
        Set(m, "Player_Weapon_Aim_Zoom/ZoomLevel[3]", "RightOffset", "0.90");

        Set(m, "Player_Weapon_Zoom/ZoomLevel[2]", "RightOffset", "0.68");
        Set(m, "Player_Weapon_Zoom/ZoomLevel[3]", "RightOffset", "0.60");
        Set(m, "Player_Weapon_Zoom/ZoomLevel[4]", "RightOffset", "0.90");

        Set(m, "Player_Weapon_Zoom_Light/ZoomLevel[2]", "RightOffset", "0.68");
        Set(m, "Player_Weapon_Zoom_Light/ZoomLevel[3]", "RightOffset", "0.68");

        return m;
    }

    // ── Combat lock-on pull-back ─────────────────────────────────────

    /// <summary>
    /// Applies a proportional offset to lock-on camera distances relative to the
    /// on-foot base set by BuildLockOnDistances. 0 = seamless with Steadycam.
    /// Positive values pull the camera further out (battlefield awareness).
    /// Negative values zoom in (cinematic focus / guard close-up).
    /// </summary>
    public static Dictionary<string, Dictionary<string, (string, string)>> BuildCombatPullback(
        double zl2, double zl3, double zl4, double offset)
    {
        var m = new Dictionary<string, Dictionary<string, (string, string)>>();
        if (offset == 0) return m;

        double f = 1.0 + offset;
        string zl2s = $"{Math.Round(zl2 * f, 1)}";
        string zl3s = $"{Math.Round(zl3 * f, 1)}";
        string zl4s = $"{Math.Round(zl4 * f, 1)}";

        // All lock-on sections
        foreach (var sec in LockOnSections)
        {
            Set(m, $"{sec}/ZoomLevel[2]", "ZoomDistance", zl2s);
            Set(m, $"{sec}/ZoomLevel[3]", "ZoomDistance", zl3s);
            Set(m, $"{sec}/ZoomLevel[4]", "ZoomDistance", zl4s);
        }

        // Guard and rush -- game switches to these when guarding/charging,
        // not to a LockOn section, so they need the offset applied separately
        foreach (var sec in new[] { "Player_Weapon_Guard", "Player_Weapon_Rush" })
        {
            Set(m, $"{sec}/ZoomLevel[2]", "ZoomDistance", zl2s);
            Set(m, $"{sec}/ZoomLevel[3]", "ZoomDistance", zl3s);
            Set(m, $"{sec}/ZoomLevel[4]", "ZoomDistance", zl4s);
        }

        // Force/Titan lock-on -- these use a wider base distance in vanilla
        double forceDist = Math.Round(zl3 * f * 2.0, 1);
        Set(m, "Player_Force_LockOn/ZoomLevel[2]", "ZoomDistance", $"{forceDist}");
        Set(m, "Player_Force_LockOn/ZoomLevel[3]", "ZoomDistance", $"{forceDist}");
        Set(m, "Player_Force_LockOn/ZoomLevel[4]", "ZoomDistance", $"{forceDist}");
        Set(m, "Player_LockOn_Titan/ZoomLevel[1]", "ZoomDistance", $"{forceDist}");
        Set(m, "Player_LockOn_Titan/ZoomLevel[2]", "ZoomDistance", $"{forceDist}");
        Set(m, "Player_LockOn_Titan/ZoomLevel[3]", "ZoomDistance", $"{forceDist}");
        Set(m, "Player_LockOn_Titan/ZoomLevel[4]", "ZoomDistance", $"{forceDist}");
        return m;
    }

    // ── Style up-offset map ──────────────────────────────────────────

    private static readonly Dictionary<string, double> StyleUpOffset = new()
    {
        ["default"] = 0.0,
        ["heroic"] = -0.2,
        ["panoramic"] = 0.0,
        ["close-up"] = -0.2,
        ["low-rider"] = -0.8,
        ["knee-cam"] = -1.2,
        ["dirt-cam"] = -1.5,
        ["survival"] = 0.0,
    };

    private static readonly Dictionary<string, Func<Dictionary<string, Dictionary<string, (string, string)>>>> StyleBuilders = new()
    {
        ["heroic"] = BuildHeroic,
        ["panoramic"] = BuildPanoramic,
        ["close-up"] = BuildCloseUp,
        ["low-rider"] = () => BuildLowVariant("-0.8"),
        ["knee-cam"] = () => BuildLowVariant("-1.2"),
        ["dirt-cam"] = () => BuildLowVariant("-1.5"),
        ["survival"] = BuildSurvival,
    };

    // ── Custom style builder ─────────────────────────────────────────

    // Vanilla RightOffset baselines per zoom level (from playercamerapreset.xml)
    private const double VanillaRoZL2 = 0.5;
    private const double VanillaRoZL3 = 0.8;
    private const double VanillaRoZL4 = 1.1;

    // Mount vanilla RightOffset baselines (section, zl, vanilla value)
    private static readonly (string Section, int ZL, double Vanilla)[] MountRoBaselines = BuildMountBaselines();

    // Aim, interaction, and focus RightOffset baselines.
    // Use on-foot baselines (0.5/0.8/1.1) for sections that transition from normal
    // gameplay so the camera doesn't snap horizontally when activating abilities.
    private static readonly (string Section, int ZL, double Vanilla)[] AimInteractionRoBaselines =
    {
        // Lantern / spotlight -- match each ZL to its corresponding normal camera baseline
        ("Player_Basic_Default_Aim_Zoom", 2, VanillaRoZL2),
        ("Player_Basic_Default_Aim_Zoom", 3, VanillaRoZL3),
        ("Player_Basic_Default_Aim_Zoom", 4, VanillaRoZL4),
        // Blinding flash
        ("Player_Taeguk_Aim", 2, VanillaRoZL2),
        ("Player_Taeguk_Aim", 3, VanillaRoZL3),
        // Weapon aim/zoom
        ("Player_Weapon_Aim_Zoom", 2, VanillaRoZL3),
        ("Player_Weapon_Aim_Zoom", 3, VanillaRoZL4),
        ("Player_Weapon_Zoom", 2, VanillaRoZL3),
        ("Player_Weapon_Zoom", 3, VanillaRoZL3),
        ("Player_Weapon_Zoom", 4, VanillaRoZL4),
        ("Player_Weapon_Zoom_Light", 2, VanillaRoZL3),
        ("Player_Weapon_Zoom_Light", 3, VanillaRoZL3),
        ("Player_Weapon_Zoom_Out", 2, VanillaRoZL3),
        ("Player_Weapon_Zoom_Out", 3, VanillaRoZL3),
        // Bow
        ("Player_Bow_Aim_Zoom_Start", 2, VanillaRoZL3),
        ("Player_Bow_Aim_Zoom_Ing", 2, VanillaRoZL3),
        ("Player_Bow_Aim_Zoom", 2, VanillaRoZL3),
        ("Player_Bow_Aim_LockOn", 2, VanillaRoZL3),
        // Ride aim
        ("Player_Ride_Aim_Zoom", 2, VanillaRoZL3),
        // Interaction / focus (CTRL)
        ("Player_Interaction_LockOn", 2, VanillaRoZL3),
        ("Interaction_LookAt", 2, VanillaRoZL3),
        // Glide aim
        ("Glide_Kick_Aim_Zoom", 2, VanillaRoZL4),
        ("Glide_Bow_Aim_Zoom", 2, VanillaRoZL4),
        // Freefall aim
        ("Player_Basic_FreeFall_Aim", 2, VanillaRoZL3),
        ("Player_Basic_FreeFall_Aim", 3, VanillaRoZL3),
        // Tool aim
        ("Player_Tool_Aim_Melee", 2, VanillaRoZL2),
        // Throw aim
        ("Player_Throw_Aim", 2, VanillaRoZL2),
        // Weapon aim boss
        ("Player_Weapon_Aim_BossAttack", 2, VanillaRoZL2),
        ("Player_Weapon_Aim_BossAttack", 3, VanillaRoZL3),
        ("Player_Weapon_Aim_SmallBossAttack", 2, VanillaRoZL2),
        ("Player_Weapon_Aim_SmallBossAttack", 3, VanillaRoZL2),
    };

    private static (string, int, double)[] BuildMountBaselines()
    {
        var list = new List<(string, int, double)>();
        foreach (var sec in HorseRideSections)
        {
            list.Add((sec, 2, 1.45));
            list.Add((sec, 3, 1.8));
        }
        list.Add(("Player_Ride_Elephant", 2, 1.3));
        list.Add(("Player_Ride_Elephant", 3, 1.6));
        list.Add(("Player_Ride_Wyvern", 2, 3.0));
        list.Add(("Player_Ride_Wyvern", 3, 4.0));
        list.Add(("Player_Ride_Wyvern", 4, 5.0));
        list.Add(("Player_Ride_Canoe", 2, 0.9));
        list.Add(("Player_Ride_Canoe", 3, 1.1));
        list.Add(("Player_Ride_Warmachine", 2, 2.4));
        list.Add(("Player_Ride_Warmachine", 3, 2.8));
        list.Add(("Player_Ride_Warmachine_Aim", 2, 1.8));
        list.Add(("Player_Ride_Warmachine_Dash", 2, 0.8));
        list.Add(("Player_Ride_Broom", 2, 0.3));
        list.Add(("Player_Ride_Broom", 3, 0.4));
        return list.ToArray();
    }

    /// <summary>
    /// Inverse of <see cref="BuildCustom"/> for on-foot ZL2: maps XML <c>RightOffset</c> at
    /// <c>Player_Basic_Default/ZoomLevel[2]</c> to the UCM Quick horizontal shift (delta).
    /// Vanilla (~0.5) → 0; file-centered (0) → 0.5.
    /// </summary>
    public static double QuickShiftDeltaFromFootZl2RightOffset(double rightOffsetZl2)
        => VanillaRoZL2 - rightOffsetZl2;

    public static Dictionary<string, Dictionary<string, (string, string)>> BuildCustom(
        double distance, double upOffset, double rightOffset)
    {
        double zl2Dist = Math.Round(distance * 0.5, 1);
        double zl3Dist = Math.Round(distance, 1);
        double zl4Dist = Math.Round(distance * 1.5, 1);
        string upStr = $"{upOffset:F2}";

        // Slider is a delta: 0 = vanilla, negative = character further left, positive = further right.
        // Scale proportionally so the character holds screen position across all zoom levels.
        double factor = 1.0 + (-rightOffset) / VanillaRoZL2;
        string roZL2 = $"{VanillaRoZL2 * factor:F2}";
        string roZL3 = $"{VanillaRoZL3 * factor:F2}";
        string roZL4 = $"{VanillaRoZL4 * factor:F2}";

        var m = new Dictionary<string, Dictionary<string, (string, string)>>();
        foreach (var sec in AllMain)
        {
            Set(m, $"{sec}/ZoomLevel[2]", "UpOffset", upStr);
            Set(m, $"{sec}/ZoomLevel[2]", "InDoorUpOffset", upStr);
            Set(m, $"{sec}/ZoomLevel[2]", "RightOffset", roZL2);
            Set(m, $"{sec}/ZoomLevel[2]", "ZoomDistance", $"{zl2Dist}");
            Set(m, $"{sec}/ZoomLevel[3]", "UpOffset", upStr);
            Set(m, $"{sec}/ZoomLevel[3]", "InDoorUpOffset", upStr);
            Set(m, $"{sec}/ZoomLevel[3]", "RightOffset", roZL3);
            Set(m, $"{sec}/ZoomLevel[3]", "ZoomDistance", $"{zl3Dist}");
            Set(m, $"{sec}/ZoomLevel[4]", "UpOffset", upStr);
            Set(m, $"{sec}/ZoomLevel[4]", "InDoorUpOffset", upStr);
            Set(m, $"{sec}/ZoomLevel[4]", "RightOffset", roZL4);
            Set(m, $"{sec}/ZoomLevel[4]", "ZoomDistance", $"{zl4Dist}");
        }
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "UpOffset", upStr);
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "InDoorUpOffset", upStr);
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "ZoomDistance", $"{zl2Dist}");
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[3]", "UpOffset", upStr);
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[3]", "InDoorUpOffset", upStr);
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[3]", "ZoomDistance", $"{zl3Dist}");
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[4]", "UpOffset", upStr);
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[4]", "InDoorUpOffset", upStr);
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[4]", "ZoomDistance", $"{zl4Dist}");

        // Apply horizontal shift to ALL mount sections (same proportional factor)
        foreach (var (sec, zl, vanilla) in MountRoBaselines)
            Set(m, $"{sec}/ZoomLevel[{zl}]", "RightOffset", $"{vanilla * factor:F2}");

        // Scale horse zoom distances from the user's distance value
        double horseZL2Dist = Math.Round(distance * 1.5, 1);
        double horseZL3Dist = Math.Round(distance * 2.1, 1);
        foreach (var sec in HorseRideSections)
        {
            Set(m, $"{sec}/ZoomLevel[2]", "ZoomDistance", $"{horseZL2Dist}");
            Set(m, $"{sec}/ZoomLevel[3]", "ZoomDistance", $"{horseZL3Dist}");
        }

        // Apply horizontal shift to aim, interaction, and focus sections so the
        // camera doesn't snap to a different position when activating abilities
        // (lantern, blinding flash, bow, CTRL focus, etc.)
        foreach (var (sec, zl, vanilla) in AimInteractionRoBaselines)
            Set(m, $"{sec}/ZoomLevel[{zl}]", "RightOffset", $"{vanilla * factor:F2}");

        // Lock-on distances scale with the user's chosen distance so there's no
        // zoom-in when transitioning from on-foot to lock-on.
        Merge(m, BuildLockOnDistances(zl2Dist, zl3Dist, zl4Dist));

        return m;
    }

    // ── Composition ──────────────────────────────────────────────────

    /// <summary>
    /// Register a custom style builder so it's available during BuildModifications.
    /// </summary>
    public static void RegisterCustomStyle(double distance, double height, double rightOffset)
    {
        StyleBuilders["custom"] = () => BuildCustom(distance, height, rightOffset);
        StyleUpOffset["custom"] = height;
    }

    public static ModificationSet BuildModifications(string style, int fov, bool bane,
        double combatPullback = 0.0,
        bool mountHeight = false, double? customUp = null, bool steadycam = true)
    {
        var mods = new Dictionary<string, Dictionary<string, (string, string)>>();

        // Layer 1: shared base (positioning: FOV, distances, offsets)
        Merge(mods, BuildSharedBase());

        // Layer 1b: steadycam smoothing (blends, damping, velocity offsets, UpOffset zeroing)
        if (steadycam)
        {
            Merge(mods, BuildSmoothing());
            Merge(mods, BuildSharedSteadycam());
        }

        // Layer 2: style overrides.
        // Always seed lock-on distances from the default (matching BuildSharedBase on-foot values)
        // so custom/user presets on the "default" style get the same normalization as named styles.
        // Named style builders will Merge their own BuildLockOnDistances on top and win.
        Merge(mods, BuildLockOnDistances(3.4, 6, 8));
        if (StyleBuilders.TryGetValue(style, out var builder))
            Merge(mods, builder());

        // Layer 3: bane (centered camera)
        if (bane)
            Merge(mods, BuildBaneMods());

        // Layer 4: lock-on offset -- proportional delta on top of base lock-on distances.
        // Read the actual ZL2/ZL3/ZL4 that ended up in mods after the style layer so the
        // offset always scales relative to the user's chosen distance.
        // Positive = pull back (more awareness), negative = zoom in (cinematic focus).
        if (combatPullback != 0)
        {
            double zl2 = mods.TryGetValue("Player_Basic_Default/ZoomLevel[2]", out var zl2d)
                && zl2d.TryGetValue("ZoomDistance", out var zl2v)
                && double.TryParse(zl2v.Item2, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out double zl2p) ? zl2p : 3.4;
            double zl3 = mods.TryGetValue("Player_Basic_Default/ZoomLevel[3]", out var zl3d)
                && zl3d.TryGetValue("ZoomDistance", out var zl3v)
                && double.TryParse(zl3v.Item2, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out double zl3p) ? zl3p : 6.0;
            double zl4 = mods.TryGetValue("Player_Basic_Default/ZoomLevel[4]", out var zl4d)
                && zl4d.TryGetValue("ZoomDistance", out var zl4v)
                && double.TryParse(zl4v.Item2, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out double zl4p) ? zl4p : 8.0;
            Merge(mods, BuildCombatPullback(zl2, zl3, zl4, combatPullback));
        }

        // Layer 5: match mount height to player
        if (mountHeight)
        {
            double up = customUp ?? (StyleUpOffset.TryGetValue(style, out var u) ? u : 0.0);
            Merge(mods, BuildMountHeightMods(up));
        }

        return new ModificationSet { ElementMods = mods, FovValue = fov };
    }
}

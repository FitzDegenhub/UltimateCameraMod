namespace UltimateCameraMod.Models;

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

    private static readonly string[] BaneSections =
        BasicSections.Concat(new[] { "Player_Basic_Default_Aim_Zoom" }).Concat(WeaponSections).ToArray();

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
        foreach (var sec in AllMountSections)
            for (int level = 2; level <= 5; level++)
                Set(mods, $"{sec}/ZoomLevel[{level}]", "UpOffset", upStr);
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

        // Lock-on behavior
        Set(m, "Player_Weapon_LockOn", "Fov", "40");
        Set(m, "Player_Weapon_LockOn", "TargetRate", "0.25");
        Set(m, "Player_Weapon_LockOn", "ScreenClampRate", "0.6");
        Set(m, "Player_Weapon_LockOn/ZoomLevel[3]", "ZoomDistance", "8");

        Set(m, "Player_Weapon_TwoTarget", "Fov", "40");
        Set(m, "Player_Weapon_TwoTarget", "TargetRate", "0.25");
        Set(m, "Player_Weapon_TwoTarget", "ScreenClampRate", "0.6");
        Set(m, "Player_Weapon_TwoTarget", "LimitUnderDistance", "3");
        Set(m, "Player_Weapon_TwoTarget/ZoomLevel[1]", "ZoomDistance", "6");
        Set(m, "Player_Weapon_TwoTarget/ZoomLevel[2]", "ZoomDistance", "8");

        Set(m, "Player_Interaction_TwoTarget", "Fov", "40");
        Set(m, "Player_Interaction_TwoTarget", "TargetRate", "0.45");
        Set(m, "Player_Interaction_TwoTarget", "ScreenClampRate", "0.65");
        Set(m, "Player_Interaction_TwoTarget/ZoomLevel[1]", "ZoomDistance", "6");
        Set(m, "Player_Interaction_TwoTarget/ZoomLevel[2]", "ZoomDistance", "8");
        Set(m, "Player_Interaction_TwoTarget/ZoomLevel[3]", "MaxZoomDistance", "10");
        Set(m, "Player_Interaction_TwoTarget/ZoomLevel[4]", "MaxZoomDistance", "10");

        Set(m, "Player_FollowLearn_LockOn_Boss", "Fov", "40");
        Set(m, "Player_FollowLearn_LockOn_Boss", "ScreenClampRate", "0.7");
        Set(m, "Player_FollowLearn_LockOn_Boss/ZoomLevel[2]", "ZoomDistance", "4.5");
        Set(m, "Player_FollowLearn_LockOn_Boss/ZoomLevel[3]", "ZoomDistance", "6.5");

        Set(m, "Player_Weapon_LockOn_System", "Fov", "40");
        Set(m, "Player_Weapon_LockOn_System", "TargetRate", "0.3");
        Set(m, "Player_Weapon_LockOn_System", "ScreenClampRate", "0.65");
        Set(m, "Player_Weapon_LockOn_System/ZoomLevel[2]", "ZoomDistance", "6");
        Set(m, "Player_Weapon_LockOn_System/ZoomLevel[3]", "Fov", "40");
        Set(m, "Player_Weapon_LockOn_System/ZoomLevel[3]", "ZoomDistance", "8");
        Set(m, "Player_Weapon_LockOn_System/ZoomLevel[4]", "Fov", "40");

        Set(m, "Player_Revive_LockOn_System", "Fov", "40");
        Set(m, "Player_Revive_LockOn_System", "ScreenClampRate", "0.65");
        Set(m, "Player_Revive_LockOn_System/ZoomLevel[2]", "ZoomDistance", "6");
        Set(m, "Player_Revive_LockOn_System/ZoomLevel[3]", "Fov", "40");
        Set(m, "Player_Revive_LockOn_System/ZoomLevel[3]", "ZoomDistance", "8");
        Set(m, "Player_Revive_LockOn_System/ZoomLevel[4]", "Fov", "40");

        Set(m, "Player_Weapon_LockOn_Non_Rotate", "Fov", "40");
        Set(m, "Player_Weapon_LockOn_Non_Rotate", "ScreenClampRate", "0.6");
        Set(m, "Player_Weapon_LockOn_Non_Rotate/ZoomLevel[3]", "ZoomDistance", "8");

        Set(m, "Player_Weapon_LockOn_WrestleOnly", "Fov", "40");
        Set(m, "Player_Weapon_LockOn_WrestleOnly", "ScreenClampRate", "0.6");
        Set(m, "Player_Weapon_LockOn_WrestleOnly/ZoomLevel[3]", "ZoomDistance", "8");

        Set(m, "Player_StartAggro_TwoTarget", "Fov", "40");
        Set(m, "Player_StartAggro_TwoTarget", "ScreenClampRate", "0.6");

        Set(m, "Player_Wanted_TwoTarget", "Fov", "40");
        Set(m, "Player_Wanted_TwoTarget", "ScreenClampRate", "0.6");

        // On-foot ZoomDistance normalization
        foreach (var sec in new[] { "Player_Basic_Default_Walk", "Player_Basic_Default_Run", "Player_Basic_Default_Runfast" })
        {
            Set(m, $"{sec}/ZoomLevel[2]", "ZoomDistance", "3.4");
            Set(m, $"{sec}/ZoomLevel[3]", "ZoomDistance", "6");
            Set(m, $"{sec}/ZoomLevel[4]", "ZoomDistance", "8");
        }

        // Combat ZoomDistance normalization (includes idle so guard→idle→walk has no zoom gap)
        foreach (var sec in new[] { "Player_Weapon_Default",
                 "Player_Weapon_Default_Walk", "Player_Weapon_Default_Run",
                 "Player_Weapon_Default_RunFast", "Player_Weapon_Default_RunFast_Follow" })
        {
            Set(m, $"{sec}/ZoomLevel[2]", "ZoomDistance", "3.4");
            Set(m, $"{sec}/ZoomLevel[3]", "ZoomDistance", "6");
            Set(m, $"{sec}/ZoomLevel[4]", "ZoomDistance", "8");
        }

        Set(m, "Player_Weapon_Guard", "Fov", "40");
        foreach (var sec in new[] { "Player_Weapon_Guard", "Player_Weapon_Rush" })
        {
            Set(m, $"{sec}/ZoomLevel[2]", "ZoomDistance", "3.4");
            Set(m, $"{sec}/ZoomLevel[3]", "ZoomDistance", "6");
            Set(m, $"{sec}/ZoomLevel[4]", "ZoomDistance", "8");
        }

        // Guard blend: fast enter, exit synced to Default/Walk BlendInTime to avoid dead-zone snap
        Set(m, "Player_Weapon_Guard/CameraBlendParameter", "BlendInTime", "0.3");
        Set(m, "Player_Weapon_Guard/CameraBlendParameter", "BlendOutTime", "0.8");

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

        // OffsetByVelocity elimination
        Set(m, "Player_Basic_Default_Run/OffsetByVelocity", "OffsetLength", "0");
        Set(m, "Player_Basic_Default_Runfast/OffsetByVelocity", "OffsetLength", "0.0");
        Set(m, "Player_Weapon_Default_Run/OffsetByVelocity", "OffsetLength", "0");
        Set(m, "Player_Weapon_Default_RunFast/OffsetByVelocity", "OffsetLength", "0.0");
        Set(m, "Player_Weapon_Default_RunFast_Follow/OffsetByVelocity", "OffsetLength", "0.0");

        // Smooth movement state transitions (reduces Y-axis slide between idle/walk/run)
        foreach (var sec in new[] {
            "Player_Basic_Default", "Player_Basic_Default_Walk",
            "Player_Basic_Default_Run", "Player_Basic_Default_Runfast",
            "Player_Weapon_Default", "Player_Weapon_Default_Walk",
            "Player_Weapon_Default_Run", "Player_Weapon_Default_RunFast",
            "Player_Weapon_Default_RunFast_Follow" })
        {
            Set(m, $"{sec}/CameraBlendParameter", "BlendInTime", "0.8");
            Set(m, $"{sec}/CameraBlendParameter", "BlendOutTime", "0.8");
        }

        // Animal mount smoothing
        Set(m, "Player_Animal_Default/CameraBlendParameter", "BlendInTime", "0.3");
        Set(m, "Player_Animal_Default_Run/CameraBlendParameter", "BlendInTime", "0.3");
        Set(m, "Player_Animal_Default_Run/OffsetByVelocity", "OffsetLength", "0");
        Set(m, "Player_Animal_Default_Runfast/CameraBlendParameter", "BlendInTime", "0.3");
        Set(m, "Player_Animal_Default_Runfast/OffsetByVelocity", "OffsetLength", "0.0");
        Set(m, "Player_Animal_Default_Runfast/OffsetByVelocity", "DampSpeed", "0.5");
        Set(m, "Player_Animal_Default_Walk/CameraBlendParameter", "BlendInTime", "0.3");

        // Horse
        Set(m, "Player_Ride_Horse/CameraBlendParameter", "BlendInTime", "0.3");
        Set(m, "Player_Ride_Horse/CameraBlendParameter", "BlendOutTime", "0.3");
        Set(m, "Player_Ride_Horse/OffsetByVelocity", "OffsetLength", "0.0");

        Set(m, "Player_Ride_Horse_Run", "FollowPitchSpeedRate", "0.8");
        Set(m, "Player_Ride_Horse_Run", "FollowStartTime", "1");
        Set(m, "Player_Ride_Horse_Run/OffsetByVelocity", "OffsetLength", "0.0");

        Set(m, "Player_Ride_Horse_Fast_Run", "FollowPitchSpeedRate", "0.8");
        Set(m, "Player_Ride_Horse_Fast_Run", "FollowStartTime", "1");
        Set(m, "Player_Ride_Horse_Fast_Run", "FollowYawSpeedRate", "0.8");
        Set(m, "Player_Ride_Horse_Fast_Run/CameraBlendParameter", "BlendInTime", "0.3");
        Set(m, "Player_Ride_Horse_Fast_Run/CameraBlendParameter", "BlendOutTime", "0.3");
        Set(m, "Player_Ride_Horse_Fast_Run/OffsetByVelocity", "DampSpeed", "0.5");
        Set(m, "Player_Ride_Horse_Fast_Run/OffsetByVelocity", "OffsetLength", "0.0");

        Set(m, "Player_Ride_Horse_Dash", "Fov", "40");
        Set(m, "Player_Ride_Horse_Dash", "FollowPitchSpeedRate", "0.8");
        Set(m, "Player_Ride_Horse_Dash", "FollowStartTime", "1");
        Set(m, "Player_Ride_Horse_Dash", "FollowYawSpeedRate", "0.8");
        Set(m, "Player_Ride_Horse_Dash/CameraBlendParameter", "BlendInTime", "0.3");
        Set(m, "Player_Ride_Horse_Dash/CameraBlendParameter", "BlendOutTime", "0.3");
        Set(m, "Player_Ride_Horse_Dash/OffsetByVelocity", "DampSpeed", "0.5");
        Set(m, "Player_Ride_Horse_Dash/OffsetByVelocity", "OffsetLength", "0.0");

        Set(m, "Player_Ride_Horse_Dash_Att", "Fov", "40");
        Set(m, "Player_Ride_Horse_Dash_Att", "FollowPitchSpeedRate", "0.8");
        Set(m, "Player_Ride_Horse_Dash_Att", "FollowStartTime", "1");
        Set(m, "Player_Ride_Horse_Dash_Att", "FollowYawSpeedRate", "0.8");
        Set(m, "Player_Ride_Horse_Dash_Att/CameraBlendParameter", "BlendInTime", "0.3");
        Set(m, "Player_Ride_Horse_Dash_Att/CameraBlendParameter", "BlendOutTime", "0.3");
        Set(m, "Player_Ride_Horse_Dash_Att/CameraDamping", "PivotDampingMaxDistance", "0.5");
        Set(m, "Player_Ride_Horse_Dash_Att/OffsetByVelocity", "DampSpeed", "0.5");
        Set(m, "Player_Ride_Horse_Dash_Att/OffsetByVelocity", "OffsetLength", "0.0");

        Set(m, "Player_Ride_Horse_Att_Thrust", "Fov", "40");
        Set(m, "Player_Ride_Horse_Att_Thrust", "FollowPitchSpeedRate", "0.8");
        Set(m, "Player_Ride_Horse_Att_Thrust", "FollowStartTime", "1");
        Set(m, "Player_Ride_Horse_Att_Thrust", "FollowYawSpeedRate", "0.8");

        Set(m, "Player_Ride_Horse_Att_L", "Fov", "40");
        Set(m, "Player_Ride_Horse_Att_L/CameraBlendParameter", "BlendInTime", "0.3");
        Set(m, "Player_Ride_Horse_Att_L/CameraBlendParameter", "BlendOutTime", "0.3");
        Set(m, "Player_Ride_Horse_Att_L/OffsetByVelocity", "OffsetLength", "0.0");
        Set(m, "Player_Ride_Horse_Att_R", "Fov", "40");
        Set(m, "Player_Ride_Horse_Att_R/CameraBlendParameter", "BlendInTime", "0.3");
        Set(m, "Player_Ride_Horse_Att_R/CameraBlendParameter", "BlendOutTime", "0.3");
        Set(m, "Player_Ride_Horse_Att_R/OffsetByVelocity", "OffsetLength", "0.0");

        // Elephant
        Set(m, "Player_Ride_Elephant", "Fov", "40");
        Set(m, "Player_Ride_Elephant", "FollowPitchSpeedRate", "0.8");
        Set(m, "Player_Ride_Elephant", "FollowYawSpeedRate", "0.8");
        Set(m, "Player_Ride_Elephant/CameraBlendParameter", "BlendInTime", "0.3");
        Set(m, "Player_Ride_Elephant/CameraBlendParameter", "BlendOutTime", "0.3");
        Set(m, "Player_Ride_Elephant/CameraDamping", "PivotDampingMaxDistance", "0.5");
        Set(m, "Player_Ride_Elephant/OffsetByVelocity", "OffsetLength", "0.0");

        // Wyvern
        Set(m, "Player_Ride_Wyvern", "Fov", "50");
        Set(m, "Player_Ride_Wyvern", "FollowStartTime", "1");
        Set(m, "Player_Ride_Wyvern", "FollowYawSpeedRate", "0.8");
        Set(m, "Player_Ride_Wyvern/OffsetByVelocity", "OffsetLength", "0.0");

        return m;
    }

    // ── Shared steadycam ─────────────────────────────────────────────

    private static Dictionary<string, Dictionary<string, (string, string)>> BuildSharedSteadycam()
    {
        var m = new Dictionary<string, Dictionary<string, (string, string)>>();

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
            Set(m, $"{sec}/ZoomLevel[3]", "UpOffset", "0.0");
            Set(m, $"{sec}/ZoomLevel[4]", "UpOffset", "0.0");
        }

        return m;
    }

    // ── Style layers ─────────────────────────────────────────────────

    private static Dictionary<string, Dictionary<string, (string, string)>> BuildWestern()
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
        return m;
    }

    private static Dictionary<string, Dictionary<string, (string, string)>> BuildCinematic()
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
        return m;
    }

    private static Dictionary<string, Dictionary<string, (string, string)>> BuildImmersive()
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
        return m;
    }

    private static Dictionary<string, Dictionary<string, (string, string)>> BuildLowcamVariant(string baseUp, string? indoorUp = null)
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

        return m;
    }

    private static Dictionary<string, Dictionary<string, (string, string)>> BuildRe2()
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
        return m;
    }

    // ── Bane layer ───────────────────────────────────────────────────

    private static Dictionary<string, Dictionary<string, (string, string)>> BuildBaneMods()
    {
        var m = new Dictionary<string, Dictionary<string, (string, string)>>();
        var indoorRightZl2 = new HashSet<string> { "Player_Basic_Default", "Player_Basic_Default_Aim_Zoom", "Player_Weapon_Default" };

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
            "Player_Weapon_Zoom/ZoomLevel[2]", "Player_Weapon_Zoom/ZoomLevel[3]", "Player_Weapon_Zoom/ZoomLevel[4]",
            "Player_Weapon_Zoom_Light/ZoomLevel[2]", "Player_Weapon_Zoom_Light/ZoomLevel[3]",
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

        return m;
    }

    // ── Combat lock-on ───────────────────────────────────────────────

    private static readonly Dictionary<string, Dictionary<string, Dictionary<string, (string, string)>>> CombatLockOn = new()
    {
        ["wide"] = BuildCombatWide(),
        ["max"] = BuildCombatMax(),
    };

    private static Dictionary<string, Dictionary<string, (string, string)>> BuildCombatWide()
    {
        var m = new Dictionary<string, Dictionary<string, (string, string)>>();
        Set(m, "Player_FollowLearn_LockOn_Boss/ZoomLevel[2]", "ZoomDistance", "4.5");
        Set(m, "Player_FollowLearn_LockOn_Boss/ZoomLevel[3]", "ZoomDistance", "8.3");
        Set(m, "Player_FollowLearn_LockOn_Boss/ZoomLevel[4]", "ZoomDistance", "9.9");
        Set(m, "Player_Force_LockOn/ZoomLevel[2]", "ZoomDistance", "15");
        Set(m, "Player_LockOn_Titan/ZoomLevel[1]", "ZoomDistance", "15");
        Set(m, "Player_Weapon_LockOn/ZoomLevel[3]", "ZoomDistance", "9.8");
        Set(m, "Player_Weapon_TwoTarget/ZoomLevel[1]", "ZoomDistance", "5.3");
        Set(m, "Player_Weapon_TwoTarget/ZoomLevel[2]", "ZoomDistance", "9");
        Set(m, "Player_Weapon_TwoTarget/ZoomLevel[3]", "ZoomDistance", "9");
        return m;
    }

    private static Dictionary<string, Dictionary<string, (string, string)>> BuildCombatMax()
    {
        var m = new Dictionary<string, Dictionary<string, (string, string)>>();
        Set(m, "Player_FollowLearn_LockOn_Boss/ZoomLevel[2]", "ZoomDistance", "6.0");
        Set(m, "Player_FollowLearn_LockOn_Boss/ZoomLevel[3]", "ZoomDistance", "9.9");
        Set(m, "Player_FollowLearn_LockOn_Boss/ZoomLevel[4]", "ZoomDistance", "9.9");
        Set(m, "Player_Force_LockOn/ZoomLevel[2]", "ZoomDistance", "20");
        Set(m, "Player_LockOn_Titan/ZoomLevel[1]", "ZoomDistance", "20");
        Set(m, "Player_Weapon_LockOn/ZoomLevel[3]", "ZoomDistance", "9.9");
        Set(m, "Player_Weapon_TwoTarget/ZoomLevel[1]", "ZoomDistance", "7.0");
        Set(m, "Player_Weapon_TwoTarget/ZoomLevel[2]", "ZoomDistance", "9");
        Set(m, "Player_Weapon_TwoTarget/ZoomLevel[3]", "ZoomDistance", "9");
        return m;
    }

    // ── Style up-offset map ──────────────────────────────────────────

    private static readonly Dictionary<string, double> StyleUpOffset = new()
    {
        ["default"] = 0.0,
        ["western"] = -0.2,
        ["cinematic"] = 0.0,
        ["immersive"] = -0.2,
        ["lowcam"] = -0.8,
        ["vlowcam"] = -1.2,
        ["ulowcam"] = -1.5,
        ["re2"] = 0.0,
    };

    private static readonly Dictionary<string, Func<Dictionary<string, Dictionary<string, (string, string)>>>> StyleBuilders = new()
    {
        ["western"] = BuildWestern,
        ["cinematic"] = BuildCinematic,
        ["immersive"] = BuildImmersive,
        ["lowcam"] = () => BuildLowcamVariant("-0.8"),
        ["vlowcam"] = () => BuildLowcamVariant("-1.2"),
        ["ulowcam"] = () => BuildLowcamVariant("-1.5"),
        ["re2"] = BuildRe2,
    };

    // ── Custom style builder ─────────────────────────────────────────

    public static Dictionary<string, Dictionary<string, (string, string)>> BuildCustom(
        double distance, double upOffset, double rightOffset)
    {
        double zl2Dist = Math.Round(distance * 0.5, 1);
        double zl3Dist = Math.Round(distance, 1);
        double zl4Dist = Math.Round(distance * 1.5, 1);
        string upStr = $"{upOffset:F2}";
        string roStr = $"{rightOffset:F2}";

        var m = new Dictionary<string, Dictionary<string, (string, string)>>();
        foreach (var sec in AllMain)
        {
            Set(m, $"{sec}/ZoomLevel[2]", "UpOffset", upStr);
            Set(m, $"{sec}/ZoomLevel[2]", "InDoorUpOffset", upStr);
            Set(m, $"{sec}/ZoomLevel[2]", "RightOffset", roStr);
            Set(m, $"{sec}/ZoomLevel[2]", "ZoomDistance", $"{zl2Dist}");
            Set(m, $"{sec}/ZoomLevel[3]", "UpOffset", upStr);
            Set(m, $"{sec}/ZoomLevel[3]", "InDoorUpOffset", upStr);
            Set(m, $"{sec}/ZoomLevel[3]", "RightOffset", roStr);
            Set(m, $"{sec}/ZoomLevel[3]", "ZoomDistance", $"{zl3Dist}");
            Set(m, $"{sec}/ZoomLevel[4]", "UpOffset", upStr);
            Set(m, $"{sec}/ZoomLevel[4]", "InDoorUpOffset", upStr);
            Set(m, $"{sec}/ZoomLevel[4]", "RightOffset", roStr);
            Set(m, $"{sec}/ZoomLevel[4]", "ZoomDistance", $"{zl4Dist}");
        }
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "UpOffset", upStr);
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "InDoorUpOffset", upStr);
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "RightOffset", roStr);
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "ZoomDistance", $"{zl2Dist}");
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[3]", "UpOffset", upStr);
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[3]", "InDoorUpOffset", upStr);
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[3]", "RightOffset", roStr);
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[3]", "ZoomDistance", $"{zl3Dist}");
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[4]", "UpOffset", upStr);
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[4]", "InDoorUpOffset", upStr);
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[4]", "RightOffset", roStr);
        Set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[4]", "ZoomDistance", $"{zl4Dist}");
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

    public static ModificationSet BuildModifications(string style, int fov, bool bane, string combat,
        bool mountHeight = false, double? customUp = null)
    {
        var mods = new Dictionary<string, Dictionary<string, (string, string)>>();

        // Layer 1: shared base
        Merge(mods, BuildSharedBase());
        Merge(mods, BuildSharedSteadycam());

        // Layer 2: style overrides
        if (StyleBuilders.TryGetValue(style, out var builder))
            Merge(mods, builder());

        // Layer 3: bane (centered camera)
        if (bane)
            Merge(mods, BuildBaneMods());

        // Layer 4: combat lock-on distance
        if (CombatLockOn.TryGetValue(combat, out var combatMods))
            Merge(mods, combatMods);

        // Layer 5: match mount height to player
        if (mountHeight)
        {
            double up = customUp ?? (StyleUpOffset.TryGetValue(style, out var u) ? u : 0.0);
            Merge(mods, BuildMountHeightMods(up));
        }

        return new ModificationSet { ElementMods = mods, FovValue = fov };
    }
}

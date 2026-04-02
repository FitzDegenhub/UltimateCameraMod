namespace UltimateCameraMod.Models;

public static class CameraParamDocs
{
    private static readonly Dictionary<string, string> Docs = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ZoomDistance"] =
            "Camera distance from the character at this zoom level (meters). " +
            "Higher values pull the camera further back. Vanilla ZL2 ≈ 3.4, ZL3 ≈ 5.5, ZL4 ≈ 8.0.",

        ["MaxZoomDistance"] =
            "Maximum allowed camera distance at this zoom level. " +
            "Acts as a ceiling — the camera won't pull back further than this value.",

        ["UpOffset"] =
            "Vertical camera offset. 0 = head level. Negative = lower (hip/knee-height). " +
            "Positive = above head. Vanilla is 0.0 for most states.",

        ["InDoorUpOffset"] =
            "Vertical offset used when the character is indoors. " +
            "Usually matches UpOffset. Reduces ceiling clipping in tight spaces.",

        ["RightOffset"] =
            "Horizontal camera offset (over-the-shoulder shift). " +
            "0.5 = vanilla (character slightly left of center). 0 = dead center. " +
            "Negative = character moves right. Higher = character further left.",

        ["InDoorRightOffset"] =
            "Horizontal offset used when indoors. " +
            "Usually matches RightOffset. Helps avoid wall clipping.",

        ["Fov"] =
            "Field of view in degrees. Vanilla is 40–45°. Higher values show more of the scene " +
            "but can cause a fisheye effect at extremes. UCM adds this as a delta on top of vanilla.",

        ["InDoorFov"] =
            "Field of view used when indoors. Often slightly lower than outdoor FoV " +
            "to reduce the fisheye effect in tight spaces.",

        ["BlendInTime"] =
            "Seconds to smoothly transition INTO this camera state. " +
            "Lower = snappier cut. Higher = cinematic ease-in. " +
            "UCM default is 0.8s for movement states, 0.3s for guard/horse.",

        ["BlendOutTime"] =
            "Seconds to smoothly transition OUT of this camera state. " +
            "Controls how long the camera lingers before switching to the next state.",

        ["OffsetLength"] =
            "Camera sway during movement. The vanilla game pushes the camera sideways when you run. " +
            "0 = no sway (steadycam feel). 1 = vanilla sway. Higher = exaggerated sway.",

        ["DampSpeed"] =
            "How fast the camera recovers from movement sway. " +
            "Higher = snappier return to center. Lower = slower, floatier recovery.",

        ["FollowPitchSpeedRate"] =
            "Camera vertical follow speed. Controls how fast the camera tracks up/down movement. " +
            "Lower values create a cinematic lag effect. Vanilla is typically 1.0.",

        ["FollowYawSpeedRate"] =
            "Camera horizontal follow speed. Controls how fast the camera tracks left/right. " +
            "Lower values make the camera feel more cinematic and less 'sticky'.",

        ["FollowStartTime"] =
            "Delay in seconds before the camera starts auto-following the character's look direction. " +
            "Higher = longer pause before the camera snaps back.",

        ["TargetRate"] =
            "How strongly the camera tracks a locked-on target (0–1). " +
            "0.25 = gentle tracking with more manual control. 0.5 = vanilla aggressive tracking. " +
            "UCM default is 0.25 for a less disorienting lock-on.",

        ["ScreenClampRate"] =
            "How much the locked-on target is kept toward screen center (0–1). " +
            "Higher = enemy stays more centered. Lower = more freedom to look around during lock-on.",

        ["LimitUnderDistance"] =
            "Minimum distance the camera maintains from the target during lock-on. " +
            "Prevents the camera from getting too close when locked onto nearby enemies.",

        ["PivotDampingMaxDistance"] =
            "Maximum distance the camera pivot can lag behind the character. " +
            "Controls how much the camera 'floats' before snapping back to follow.",

        ["PivotDampingSpeed"] =
            "Speed at which the camera pivot catches up to the character. " +
            "Higher = tighter follow. Lower = more floaty, cinematic feel.",

        ["CollisionRadius"] =
            "Radius of the camera's collision sphere. Larger values push the camera away from walls " +
            "earlier, preventing clipping but potentially causing jumpy movement near geometry.",

        ["CollisionOffset"] =
            "Additional offset applied when the camera collides with geometry. " +
            "Helps prevent the camera from clipping through walls and objects.",

        ["ZoomInSpeed"] =
            "How fast the camera zooms in when scrolling the mouse wheel. " +
            "Higher = faster zoom response.",

        ["ZoomOutSpeed"] =
            "How fast the camera zooms out when scrolling the mouse wheel. " +
            "Higher = faster zoom response.",

        ["RotateSpeed"] =
            "Camera rotation speed when moving the mouse. " +
            "Affects how quickly the view turns in response to mouse input.",

        ["PitchMin"] =
            "Minimum vertical look angle (degrees). Limits how far down the camera can tilt. " +
            "Negative values allow looking further down.",

        ["PitchMax"] =
            "Maximum vertical look angle (degrees). Limits how far up the camera can tilt.",

        ["DistanceMin"] =
            "Minimum camera distance (closest zoom). " +
            "The camera won't zoom in closer than this value.",

        ["DistanceMax"] =
            "Maximum camera distance (furthest zoom). " +
            "The camera won't zoom out further than this value.",
    };

    public static string? Get(string attributeName) =>
        Docs.TryGetValue(attributeName, out string? doc) ? doc : null;

    public static string GetOrDefault(string attributeName) =>
        Docs.TryGetValue(attributeName, out string? doc) ? doc : attributeName;
}

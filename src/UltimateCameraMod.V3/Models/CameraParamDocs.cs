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

        // ── Section-level attributes ────────────────────────────────

        ["Type"] =
            "Camera behavior type. 'TPS' = third-person shooter. 'CinematicLockOn' = NPC interaction/dialogue camera. " +
            "Determines how the camera tracks the character and targets.",

        ["PivotSetType"] =
            "What the camera orbits around. 'FocusActor' = player character. " +
            "Other values may focus on targets, NPCs, or world positions.",

        ["UseZoomInByPitch"] =
            "When true, the camera zooms in slightly as you look down (pitch down). " +
            "Creates a more cinematic feel when looking at the ground.",

        ["UseUpOffsetByPitch"] =
            "When true, the vertical offset shifts as you tilt the camera up/down. " +
            "Helps keep the character framed when looking at extreme angles.",

        // ── Interaction / lock-on attributes ────────────────────────

        ["CameraAutoRotateSpeed"] =
            "How aggressively the camera rotates toward a target (NPC, enemy). " +
            "Higher = stronger magnetic pull toward the target. Vanilla interaction = 10 (very aggressive). " +
            "Set to 0 to disable auto-rotation entirely.",

        ["CameraAutoRotateMaxSpeed"] =
            "Maximum rotation speed cap for auto-rotate. Limits how fast the camera can snap to a target. " +
            "Vanilla interaction = 300. Lower values = smoother, less jarring rotation.",

        ["CameraAutoRotateAccelSpeed"] =
            "How quickly the auto-rotate accelerates. Higher = snappier pull toward target. " +
            "Lower = more gradual acceleration, feels less magnetic.",

        ["CameraAutoRotateDecelSpeed"] =
            "How quickly the auto-rotate decelerates. -1 = instant stop. " +
            "Higher positive values = gradual slowdown when reaching the target.",

        ["IsTargetFixed"] =
            "When true, the camera locks rigidly onto the target with no player input allowed. " +
            "When false, the player can still move the camera while the auto-rotate pulls.",

        ["CloseTargetPitchCorrection"] =
            "When true, the camera adjusts its vertical angle when close to a target. " +
            "Prevents the camera from looking at the ground when standing right next to an NPC.",

        ["RestoreYawOnEnd"] =
            "When true, the camera snaps back to its original horizontal rotation after the interaction ends. " +
            "When false, the camera stays where the interaction left it.",

        ["RestorePitchOnEnd"] =
            "When true, the camera snaps back to its original vertical angle after the interaction ends. " +
            "When false, the camera stays at whatever pitch the interaction used.",

        ["TargetHeightRatio"] =
            "Where on the target the camera looks. 0 = feet, 0.5 = waist, 0.8 = head, 1.0 = above head. " +
            "Vanilla interaction = 0.8 (roughly head height).",

        ["InputDampingRate"] =
            "Smoothing applied to mouse/controller input. Higher = more sluggish camera movement. " +
            "Lower = snappier response. 0 = no smoothing.",

        // ── Blend / transition attributes ───────────────────────────

        ["BlendInEaseType"] =
            "Easing curve when blending INTO this camera state. " +
            "'OutQuad' = fast start, slow end (smooth arrival). 'InOutQuad' = smooth both ways. " +
            "'Linear' = constant speed.",

        ["BlendOutEaseType"] =
            "Easing curve when blending OUT of this camera state. " +
            "'OutQuad' = fast start, slow end. 'InQuad' = slow start, fast end.",

        // ── Zoom level attributes ───────────────────────────────────

        ["Level"] =
            "Zoom tier index. 0 = closest (over-shoulder), 1 = close, 2 = default on-foot, " +
            "3 = pulled back, 4 = furthest. Scroll wheel cycles through these.",

        ["InDoorZoomRatio"] =
            "Zoom distance multiplier when indoors. 1.0 = same as outdoor. " +
            "Lower values pull the camera in tighter to avoid clipping indoor geometry.",

        ["DisableOn"] =
            "Conditions that prevent this zoom level from being used. " +
            "Space-separated list: 'WeaponOut', 'Move', 'RemoteCatch', 'Catcher', etc.",

        ["EnableDepthOfField"] =
            "When true, enables depth-of-field blur at this zoom level. " +
            "Creates a cinematic background blur effect.",

        ["Aperture"] =
            "Depth-of-field aperture. Lower = more blur, higher = sharper. " +
            "Only has effect when EnableDepthOfField is true.",

        ["FocalLength"] =
            "Depth-of-field focal length. Controls the distance at which objects are in focus. " +
            "Only has effect when EnableDepthOfField is true.",

        ["EnableCameraReflector"] =
            "When true, enables environmental reflections in the camera view at this zoom level.",

        // ── Damping / smoothing attributes ──────────────────────────

        ["PivotDamping"] =
            "Camera pivot smoothing. Higher = more smoothing on the orbit point. " +
            "Reduces jitter when the character moves erratically.",

        ["HeightRatio"] =
            "Character height used for camera pivot calculation. " +
            "0.875 = typical for adult character. Affects where the camera orbits.",
    };

    public static string? Get(string attributeName) =>
        Docs.TryGetValue(attributeName, out string? doc) ? doc : null;

    public static string GetOrDefault(string attributeName) =>
        Docs.TryGetValue(attributeName, out string? doc) ? doc : attributeName;
}

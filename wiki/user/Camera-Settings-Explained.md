# Camera Settings Explained

This page explains every camera setting available in UCM in plain language. For each setting, you will find what it does, what changing it looks like in the game, the range of values you can use, and tips for getting the feel you want.

All of these settings are available on the **UCM Quick** tab unless otherwise noted. Some are also adjustable in more detail through Fine Tune or God Mode.

---

## Table of Contents

- [Distance](#distance)
- [Height](#height)
- [Horizontal Shift](#horizontal-shift)
- [Field of View (FoV)](#field-of-view-fov)
- [Centered Camera](#centered-camera)
- [Center Character on Screen](#center-character-on-screen)
- [Lock-on Zoom](#lock-on-zoom)
- [Mount Camera](#mount-camera)
- [Steadycam](#steadycam)

---

## Distance

**What it does:** Controls how far the camera sits behind your character.

**Range:** 1.5 to 12

**Where to find it:** UCM Quick tab, Distance slider.

### What You See in the Game

- **Low values (1.5 to 3):** The camera is very close to your character. You can see fine details on armor, hair, and weapons. The character fills a large portion of the screen. The world around you feels more enclosed and intimate. This is similar to a tight over-the-shoulder view like you would see in survival horror games.

- **Medium values (4 to 6):** The camera is at a comfortable middle distance. Your character is clearly visible but does not dominate the screen. You have a good view of your surroundings. This is roughly where most third-person action games place the camera by default.

- **High values (7 to 12):** The camera pulls far back from your character. You can see a wide area around you, which is great for situational awareness in combat or appreciating large environments. Your character appears smaller on screen. At the highest values, the camera almost reaches a tactical overhead-style perspective.

### How It Works Technically

The Distance slider controls the `ZoomDistance` attribute on three zoom levels in the camera XML:

| Zoom Level | What It Represents |
|---|---|
| ZoomLevel 2 | The idle/default camera distance when you are standing still or walking |
| ZoomLevel 3 | The medium distance the camera uses during certain transitions |
| ZoomLevel 4 | The far distance used when the camera pulls back |

UCM uses **proportional scaling** when you change the Distance value. This means all three zoom levels are adjusted together in a way that keeps your character at the same relative position on screen. You do not need to worry about the individual zoom levels; the slider handles the math for you.

### Tips

- If you play a lot of combat, a distance of 5 to 7 gives you a good balance between seeing your character's animations and having enough spatial awareness to dodge and position.
- For exploration and sightseeing, try higher values (8 to 10) to take in the scenery.
- For a cinematic, immersive feel, try lower values (2 to 4) with a slight FoV increase to compensate for the narrower view.

---

## Height

**What it does:** Controls the vertical position of the camera relative to your character.

**Range:** -1.6 to 1.5

**Where to find it:** UCM Quick tab, Height slider.

### What You See in the Game

- **Negative values (-1.6 to -0.1):** The camera is positioned below your character's shoulders. At -0.5, the camera sits roughly at hip level, looking slightly upward at the character. This makes your character look taller and more imposing. At extreme negative values like -1.6, the camera is near ground level, which gives a dramatic low-angle perspective that makes everything above you loom large.

- **Zero (0):** The camera is at roughly shoulder height. This is close to the vanilla default for most camera states.

- **Positive values (0.1 to 1.5):** The camera is positioned above your character's head. At 0.5, you get a slightly elevated view looking down at the character, which gives a better overview of the terrain ahead. At 1.5, the camera is well above the character, providing an almost bird's-eye angle.

### How It Works Technically

The Height slider controls the `UpOffset` attribute on ZoomLevel 2 (idle) and ZoomLevel 3 (medium). A negative UpOffset moves the camera downward; a positive UpOffset moves it upward.

### Visual Examples by Value

| Height Value | Camera Position | Game Feel |
|---|---|---|
| -1.6 | Near ground level | Dramatic, ground-level perspective. World feels huge. |
| -0.8 | Below hip level | Low and cinematic. Character dominates the frame. |
| -0.3 | Slightly below shoulder | Subtle cinematic angle. Slightly heroic feel. |
| 0 | Shoulder level | Neutral, similar to vanilla. |
| 0.5 | Above head | Slight overhead view. Better terrain visibility. |
| 1.0 | Well above head | High angle, tactical feel. |
| 1.5 | Highest setting | Strong overhead perspective. |

### Tips

- A small negative height (around -0.2 to -0.5) combined with a moderate distance (5 to 7) creates a classic heroic action game camera that makes the player character feel powerful.
- High positive values work well with increased distance for a more strategic, overview-style camera.
- Extreme negative values (below -1.0) look dramatic but can make navigation harder since you cannot see as far ahead.

---

## Horizontal Shift

**What it does:** Adjusts the left-right position of the camera. This controls how far off to the side the camera is from being directly behind your character.

**Range:** -3 to 3

**Where to find it:** UCM Quick tab, Horizontal Shift slider.

### What You See in the Game

Crimson Desert, like many third-person games, positions the camera slightly to the right of the character by default. This creates the classic "over-the-right-shoulder" view where you can see past the character on the left side of the screen.

- **Vanilla value (0):** The camera uses the game's built-in rightward bias. Your character appears slightly left of center on screen, with more visible space to the right.

- **Positive values (toward 0.5):** The camera moves toward being directly behind the character, reducing the shoulder offset. At around 0.5, the camera is nearly centered.

- **Higher positive values (above 0.5):** The camera shifts past center to the left, creating a left-shoulder view.

- **Negative values (down to -3):** The camera moves further to the right, exaggerating the over-the-right-shoulder perspective. Your character appears even further to the left on screen.

### How It Works Technically

Horizontal Shift applies a delta (a change) to the `RightOffset` attribute in the camera XML. The vanilla game already has a non-zero RightOffset that creates the shoulder cam look. Your slider value is added on top of that base value.

### Relationship with Centered Camera

If you enable the **Centered Camera** checkbox, the Horizontal Shift slider is automatically locked to 0 and becomes uneditable. Centered Camera forces `RightOffset` to 0 across all camera states, which overrides any horizontal shift. You cannot use both at the same time. If you want fine control over the horizontal position, leave Centered Camera off and use this slider instead.

### Tips

- If the over-the-shoulder view bothers you but you do not want a fully centered camera, try shifting toward 0.3 to 0.5 for a more balanced look.
- Negative values are unusual for most players, but some people prefer an exaggerated shoulder cam for a specific cinematic style.

---

## Field of View (FoV)

**What it does:** Widens or narrows how much of the game world you can see at once, measured in degrees.

**Range:** 0 to +40 degrees (added on top of the game's base 40-degree FoV)

**Where to find it:** UCM Quick tab, FoV dropdown.

### What You See in the Game

Field of View determines the width of the camera's "cone of vision." Think of it like this: a narrow FoV is like looking through binoculars (you see a small area in high detail), while a wide FoV is like using a wide-angle lens (you see a large area but things at the edges may look slightly stretched).

| FoV Setting | Total FoV (base + added) | What It Looks Like |
|---|---|---|
| 0 (vanilla) | 40 degrees | The game's default view. Moderate width. |
| +5 | 45 degrees | Slightly wider. A subtle change most people will not notice immediately. |
| +10 | 50 degrees | Noticeably wider. More of the environment is visible on the sides. |
| +15 | 55 degrees | A comfortable wide view. Popular choice for players who like seeing more. |
| +20 | 60 degrees | Wide and open. The world feels more expansive. |
| +25 | 65 degrees | Very wide. Starting to approach ultra-wide territory. |
| +30 | 70 degrees | Extremely wide. Edges of the screen show noticeable perspective distortion. |
| +35 | 75 degrees | Very aggressive wide angle. |
| +40 | 80 degrees | Maximum. Fisheye-like distortion at the edges. For those who want to see everything. |

### How It Works Technically

The FoV value you select is added on top of the vanilla FoV values in the camera XML. UCM applies this universally across **all** camera sections:

- All `Player_` sections (every camera state for the player character)
- All `Cinematic_` sections (cutscene and scripted camera angles)
- All `Glide_` sections (camera behavior while gliding)

This means your FoV adjustment is consistent everywhere. You will not get a jarring change in field of view when switching between exploration, combat, and cutscenes.

### Tips

- Most players find +10 to +20 to be a comfortable increase that shows more of the world without looking distorted.
- If you increase Distance significantly (pulling the camera far back), you may not need as much extra FoV since the wider camera position already shows more of the environment.
- Very high FoV values (+30 and above) can cause a "fisheye" look where objects at the edges of the screen appear stretched. This is a natural consequence of wide-angle projection and is not a bug.
- If you play on an ultrawide monitor, you may want less additional FoV since your monitor already provides a wider view.

---

## Centered Camera

**What it does:** Removes the over-the-shoulder offset across all camera states, placing the camera directly behind your character.

**Where to find it:** UCM Quick tab, Centered Camera checkbox.

### What You See in the Game

With Centered Camera **off** (the default), the camera sits slightly to the right of your character. Your character appears left-of-center on screen, and you have a clear view past them on their right side. This is the standard "over-the-shoulder" perspective used in most modern third-person games.

With Centered Camera **on**, the camera moves to sit directly behind your character. Your character appears in the exact center of the screen, and you have equal visibility on both sides. The view is symmetrical.

### Scope of the Change

Centered Camera is not a simple toggle. When you enable it, UCM sets `RightOffset` to 0 across **over 150 camera states** in the XML. This covers:

- Walking, running, sprinting
- Standing idle
- Combat stance
- Mounted movement
- Aiming
- Lock-on
- Various transition states
- And many more

This is a thorough, global change. You will not find situations where the shoulder offset suddenly reappears in one specific camera state.

### Interaction with Horizontal Shift

When Centered Camera is enabled:

- The **Horizontal Shift** slider is locked to 0 and becomes uneditable.
- You cannot have both a centered camera and a horizontal shift active at the same time.
- If you disable Centered Camera, the Horizontal Shift slider unlocks and you can use it again.

### When to Use It

- If you find the shoulder cam disorienting or prefer symmetrical framing.
- If you want a more classic, old-school third-person camera where the character is centered.
- If you are creating screenshots or videos and want the character perfectly centered in the frame.

---

## Center Character on Screen

**What it does:** This is a **different setting from Centered Camera**. Center Character on Screen adjusts how aggressively the game keeps the character model in the center of the viewport during movement and camera transitions.

**Where to find it:** Fine Tune tab (not on UCM Quick).

### How It Differs from Centered Camera

These two settings sound similar but do completely different things:

| Setting | What It Does |
|---|---|
| **Centered Camera** | Removes the left-right shoulder offset. The camera moves to sit directly behind the character. Affects `RightOffset`. |
| **Center Character on Screen** | Controls how much the game clamps the character's screen position to the center during camera movement. Affects `ScreenClampRate`. |

Think of it this way: Centered Camera changes **where the camera is positioned** relative to the character. Center Character on Screen changes **how the game keeps the character from drifting off-center** when the camera is moving or transitioning between states.

### What You See in the Game

- **Higher ScreenClampRate values:** The character stays more rigidly centered on screen. During fast movement or camera transitions, the character does not drift as much to the edges.
- **Lower ScreenClampRate values:** The character is allowed to drift further from center during movement and transitions, creating a more fluid, dynamic feel.

This setting is more subtle than Centered Camera and is mainly noticeable during rapid movement changes or camera transitions.

---

## Lock-on Zoom

**What it does:** Controls how the camera distance changes when you lock onto an enemy in combat.

**Range:** -60% to +60%

**Where to find it:** UCM Quick tab, Lock-on Zoom slider.

### What You See in the Game

When you lock onto an enemy in Crimson Desert, the camera adjusts its distance to frame both your character and the target. The Lock-on Zoom slider lets you control this behavior:

- **0% (default):** The lock-on camera distance matches your normal on-foot camera distance. No extra zoom in or out when you lock on.

- **Positive values (+1% to +60%):** The camera pulls back **wider** when you lock on to an enemy. This gives you a broader view of the battlefield, making it easier to see other enemies, environmental hazards, and the general lay of the land during combat. At +60%, the camera pulls back significantly, giving an almost tactical overview of the fight.

- **Negative values (-1% to -60%):** The camera zooms **in closer** when you lock on to an enemy. This creates a tighter, more cinematic combat feel. The locked-on enemy fills more of the screen, giving the fight a more intense, focused quality. At -60%, the camera zooms in quite aggressively, framing the fight in a very dramatic way.

### How It Works Technically

Lock-on Zoom applies a proportional offset to the lock-on camera distances in the XML. The slider value is a percentage relative to your on-foot camera distance. This means:

- If your on-foot Distance is set to 6 and Lock-on Zoom is +30%, the lock-on camera will sit about 30% farther back than 6.
- If your on-foot Distance is 6 and Lock-on Zoom is -30%, the lock-on camera will be about 30% closer than 6.

### Tips

- If you find combat too chaotic with the default lock-on view, try +20% to +40% for a wider combat perspective.
- If you want more cinematic, intense combat encounters, try -20% to -40% for a closer lock-on camera.
- Combining Lock-on Zoom with a moderate on-foot Distance (5 to 7) and Steadycam gives you a smooth, controlled combat camera experience.

---

## Mount Camera

**What it does:** Synchronizes your horse/mount camera height with your player (on-foot) camera height setting.

**Where to find it:** UCM Quick tab, Mount Camera checkbox.

### What You See in the Game

Without Mount Camera enabled, the mount camera uses its own independent height value. This means if you set your on-foot camera height to -0.5 (a lower, cinematic angle), the camera might jump to a completely different height when you get on your horse.

With Mount Camera **enabled**, UCM matches the mount camera height to your player camera height setting. So if your on-foot camera is at -0.5, the mount camera will also be at -0.5. This creates a consistent feel when transitioning between on-foot and mounted gameplay.

### When to Use It

- **Enable it** if you want a seamless, consistent camera experience where mounting and dismounting does not cause a jarring height change.
- **Leave it off** if you deliberately want different camera heights for on-foot and mounted gameplay. Some players prefer a higher camera when mounted (since horses are taller) and a lower camera when on foot.

### What It Does Not Do

Mount Camera only syncs the **height**. It does not sync distance, FoV, horizontal shift, or other settings between the on-foot and mount cameras. Those can still be different. If you want full control over every mount camera value independently, use the Fine Tune tab's "Mount" section.

---

## Steadycam

**What it does:** Smooths camera transitions across 30+ camera states, making the camera feel stable and fluid instead of jerky or snappy.

**Where to find it:** UCM Quick tab, Steadycam checkbox.

### What You See in the Game

Crimson Desert's camera transitions between many different states as you play: standing still, walking, running, sprinting, changing direction, entering combat, mounting a horse, and so on. Each transition can cause the camera to blend from one position, distance, and angle to another.

Without Steadycam, these transitions use the game's default timing. Some transitions might be fast and snappy while others are slow and smooth. This inconsistency can make the camera feel jerky or unpredictable, especially during rapid movement changes.

With Steadycam **enabled**, UCM normalizes the blend timing and velocity sway across all these transitions. The result:

- The camera moves smoothly from one state to another without sudden jumps
- Direction changes do not cause the camera to snap or lurch
- Starting and stopping movement produces gentle, gradual camera adjustments
- The overall camera feel becomes more "cinematic" and controlled, like a real camera operator is following your character

### How It Works Technically

Steadycam adjusts blend timing and velocity-related values across more than 30 camera states in the XML. It normalizes these values so that transitions between states happen at consistent, comfortable speeds.

### Effect on Fine Tune Sliders

When Steadycam is enabled, it takes control of certain sliders in the Fine Tune tab. Those sliders will appear **greyed out** (uneditable). If you hover over a greyed-out slider, a tooltip explains that Steadycam is controlling that value.

The specific sliders that Steadycam controls are in the **Smoothing** section of Fine Tune, as well as some blend-related values in other sections. If you want to manually adjust those values yourself, you need to disable Steadycam first.

### When to Use It

- **Enable it** if you want a smooth, consistent, cinematic camera that never jerks or snaps during gameplay. This is the recommended starting point for most players.
- **Disable it** if you prefer the game's default transition behavior, or if you want to manually control every smoothing value yourself through Fine Tune.

### Tips

- Steadycam works particularly well when combined with a moderate Distance (5 to 7) and a slight negative Height (-0.2 to -0.5). This creates a classic, polished third-person camera feel.
- If you find the camera still too snappy with Steadycam on, you can further adjust the non-locked smoothing values in Fine Tune for even more control.
- Steadycam is a convenience feature. Everything it does could be achieved manually by adjusting the individual smoothing values in Fine Tune, but Steadycam does it all at once with tested, balanced values.

---

## Settings Summary Table

| Setting | Tab | Type | Range | Quick Description |
|---|---|---|---|---|
| **Distance** | UCM Quick | Slider | 1.5 to 12 | How far behind the character the camera sits |
| **Height** | UCM Quick | Slider | -1.6 to 1.5 | Vertical position of the camera |
| **Horizontal Shift** | UCM Quick | Slider | -3 to 3 | Left-right offset of the camera |
| **Field of View** | UCM Quick | Dropdown | 0 to +40 degrees | How wide the camera's view is |
| **Centered Camera** | UCM Quick | Checkbox | On/Off | Removes shoulder cam offset, centers camera behind character |
| **Center Character on Screen** | Fine Tune | Slider | Varies | How aggressively the character is kept centered during camera motion |
| **Lock-on Zoom** | UCM Quick | Slider | -60% to +60% | Camera distance change when locking onto an enemy |
| **Mount Camera** | UCM Quick | Checkbox | On/Off | Syncs mount camera height to player camera height |
| **Steadycam** | UCM Quick | Checkbox | On/Off | Smooths camera transitions across 30+ states |

---

## Common Combinations

Here are some popular setting combinations to get you started:

### "Classic Action Game" Camera
- Distance: 5 to 6
- Height: -0.2 to 0
- FoV: +10 to +15
- Centered Camera: Off
- Steadycam: On

A familiar, comfortable camera like you would find in popular third-person action games. Good all-around visibility with a slight cinematic lean.

### "Cinematic Close-Up" Camera
- Distance: 2.5 to 3.5
- Height: -0.3 to -0.5
- FoV: +15 to +20
- Centered Camera: Off
- Lock-on Zoom: -30%
- Steadycam: On

A tight, low camera that puts you right behind the character's shoulder. The FoV increase compensates for the close distance by showing more peripheral vision. Lock-on zoom brings you even closer during fights for intense combat moments.

### "Wide Battlefield" Camera
- Distance: 8 to 10
- Height: 0.3 to 0.8
- FoV: +5 to +10
- Centered Camera: On
- Lock-on Zoom: +30% to +50%
- Steadycam: On

A pulled-back, elevated camera that gives you a broad view of the world and battlefield. The centered camera and high position create a tactical feel. Lock-on zoom pulls back even further during combat so you can see the whole fight.

### "Ground Level" Camera
- Distance: 3 to 5
- Height: -1.0 to -1.5
- FoV: +20 to +25
- Centered Camera: Off or On
- Steadycam: On

A low, dramatic camera that makes your character tower over the viewpoint. Everything above you looks large and imposing. Requires higher FoV to compensate for the limited upward visibility. Creates a unique, immersive perspective.

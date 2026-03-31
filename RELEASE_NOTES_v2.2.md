# Ultimate Camera Mod v2.2

## What's New

- **Steadycam Overhaul** -- Completely revamped camera smoothing system. Drastically reduces jolting and jarring transitions between movement states (walking, running, guarding, mounting, combat). Toggle on/off with the new Steadycam checkbox. Some minor jolts may remain -- the community can fine-tune these via the Advanced Editor over time.
- **Extra Zoom Levels** -- New checkbox to add two extra zoom-out levels. Scroll further out than ever before, on foot and mounted.
- **Horse First Person View (Experimental)** -- New checkbox to enable first-person camera while mounted. Scroll all the way in to see through your character's eyes. Works well at walk/trot but may clip during dashes -- a proper first-person mode would need engine support from Pearl Abyss.
- **Horizontal Shift** -- New slider (-1 to +1) to position your character left, center, or right on screen when using Custom style.
- **Universal FoV Consistency** -- Your chosen FoV now applies across every camera state. No more camera "breathing" when transitioning between guard, aim, mount, glide, or cinematic states.
- **Skill Aiming Respects Your Camera Side** -- Lantern, Blinding Flash, and all aim/zoom skills now keep your character on the same side of the screen as your normal gameplay. No more jarring side-swap when activating abilities.
- **Import XML** -- New "Import XML" button in the Advanced Editor. Load a `playercamerapreset.xml` file from other mods and merge the values into your setup. Only accepts the camera preset XML format.
- **Preset Sharing** -- Export and import custom presets as shareable copy-paste codes. Share your Distance/Height/Horizontal Shift setup with friends.
- **Advanced Import/Export** -- Export your Advanced Editor overrides as a string and import them back for granular community sharing.
- **Smoother Transitions** -- Guard release and horse mounting no longer cause sudden camera jolts. Blended over 1 second for a cinematic feel.
- **Update Notifications** -- The app now alerts you when a new release is available on GitHub. Never miss an update.

## Improvements

- **Advanced Editor: Expand/Collapse All** -- New buttons to quickly expand or collapse all sections in the Advanced Editor grid.
- **Advanced Editor: Stability & Performance** -- Major stability improvements. The Advanced Editor no longer hangs during large operations. Improved responsiveness across import, export, reset, and filtering.

## Fixes

- Fixed horizontal shift not applying correctly to custom style presets
- Custom presets stopped working, this is patched. 
- Fixed Advanced Editor crash when using "Reset to Defaults"
- Fixed broken extra zoom level on horseback when horse extra zoom was disabled
- Fixed RightOffset normalization to prevent horizontal drift during walk/run/guard transitions
- Fixed guard release causing a disorienting zoom snap
- Fixed camera jolting when mounting/dismounting horses

## Known Issues

- **HUD Centering Disabled** -- A recent Crimson Desert update added integrity checks to UI files. Modifying them triggers a Coherent Gameface watermark overlay. HUD X/Y controls are temporarily disabled until a workaround is found.
- **Horse First Person clipping** -- Dashing/sprinting in first-person on horseback can cause camera clipping. Experimental feature; proper first-person needs engine support from Pearl Abyss.
- **Bow aiming with far-left camera** -- When horizontal shift is negative (character on right side), bow aiming may briefly swap sides. Niche edge case.

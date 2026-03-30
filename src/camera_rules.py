"""CDCamera modification rules as pure data.

Extracted from CDCamera v1.6.1 presets. Each layer (shared base, style,
bane, combat) defines element-level attribute changes composed together
by build_modifications() into a single ModificationSet.

CDCamera uses additive FoV: fov_value is the delta (e.g. 10, 20) added
to every Fov attribute on TPS/TwoTargetLockOn sections.
"""

from dataclasses import dataclass, field


@dataclass
class ModificationSet:
    element_mods: dict = field(default_factory=dict)
    fov_value: int = 0


# ── Section lists ──────────────────────────────────────────────────

_BASIC_SECTIONS = [
    'Player_Basic_Default',
    'Player_Basic_Default_Walk',
    'Player_Basic_Default_Run',
    'Player_Basic_Default_Runfast',
]

_WEAPON_SECTIONS = [
    'Player_Weapon_Default',
    'Player_Weapon_Default_Walk',
    'Player_Weapon_Default_Run',
    'Player_Weapon_Default_RunFast',
    'Player_Weapon_Default_RunFast_Follow',
    'Player_Weapon_Rush',
    'Player_Weapon_Guard',
]

_DEFAULT_ONLY = [
    'Player_Basic_Default',
    'Player_Weapon_Default',
]

_WALK_RUN = [
    'Player_Basic_Default_Walk',
    'Player_Basic_Default_Run',
    'Player_Basic_Default_Runfast',
    'Player_Weapon_Default_Walk',
    'Player_Weapon_Default_Run',
    'Player_Weapon_Default_RunFast',
    'Player_Weapon_Default_RunFast_Follow',
]

_ALL_MAIN = _BASIC_SECTIONS + _WEAPON_SECTIONS

_BANE_SECTIONS = _BASIC_SECTIONS + [
    'Player_Basic_Default_Aim_Zoom',
] + _WEAPON_SECTIONS

_HORSE_RIDE_SECTIONS = [
    'Player_Ride_Horse',
    'Player_Ride_Horse_Run',
    'Player_Ride_Horse_Fast_Run',
    'Player_Ride_Horse_Dash',
    'Player_Ride_Horse_Dash_Att',
    'Player_Ride_Horse_Att_Thrust',
    'Player_Ride_Horse_Att_R',
    'Player_Ride_Horse_Att_L',
]

_ALL_MOUNT_SECTIONS = _HORSE_RIDE_SECTIONS + [
    'Player_Ride_Elephant',
    'Player_Ride_Wyvern',
    'Player_Ride_Canoe',
    'Player_Ride_Warmachine',
    'Player_Ride_Broom',
]


def _build_mount_height_mods(up_offset):
    """Apply player UpOffset to all mount sections so mounts share
    the same camera height as on-foot gameplay."""
    up_str = f'{up_offset}'
    mods = {}
    for sec in _ALL_MOUNT_SECTIONS:
        for level in (2, 3, 4, 5):
            key = f'{sec}/ZoomLevel[{level}]'
            mods.setdefault(key, {})['UpOffset'] = ('SET', up_str)
    return mods


def _build_mount_distances(scale):
    """Scale all mount ZoomDistances relative to vanilla.

    Vanilla distances:
      Horse   ZL2=7.5  ZL3=10.5
      Elephant ZL2=8   ZL3=11
      Wyvern  ZL2=12  ZL3=16  ZL4=20
      Canoe   ZL2=6   ZL3=9
      Warmachine ZL2=9 ZL3=11
      Broom   ZL2=10  ZL3=14
    """
    mods = {}
    for sec in _HORSE_RIDE_SECTIONS:
        mods[f'{sec}/ZoomLevel[2]'] = {
            'ZoomDistance': ('SET', f'{7.5 * scale:.1f}'),
        }
        mods[f'{sec}/ZoomLevel[3]'] = {
            'ZoomDistance': ('SET', f'{10.5 * scale:.1f}'),
        }
    mods['Player_Ride_Elephant/ZoomLevel[2]'] = {
        'ZoomDistance': ('SET', f'{8 * scale:.1f}'),
    }
    mods['Player_Ride_Elephant/ZoomLevel[3]'] = {
        'ZoomDistance': ('SET', f'{11 * scale:.1f}'),
    }
    mods['Player_Ride_Wyvern/ZoomLevel[2]'] = {
        'ZoomDistance': ('SET', f'{12 * scale:.1f}'),
    }
    mods['Player_Ride_Wyvern/ZoomLevel[3]'] = {
        'ZoomDistance': ('SET', f'{16 * scale:.1f}'),
    }
    mods['Player_Ride_Wyvern/ZoomLevel[4]'] = {
        'ZoomDistance': ('SET', f'{20 * scale:.1f}'),
    }
    mods['Player_Ride_Canoe/ZoomLevel[2]'] = {
        'ZoomDistance': ('SET', f'{6 * scale:.1f}'),
    }
    mods['Player_Ride_Canoe/ZoomLevel[3]'] = {
        'ZoomDistance': ('SET', f'{9 * scale:.1f}'),
    }
    mods['Player_Ride_Warmachine/ZoomLevel[2]'] = {
        'ZoomDistance': ('SET', f'{9 * scale:.1f}'),
    }
    mods['Player_Ride_Warmachine/ZoomLevel[3]'] = {
        'ZoomDistance': ('SET', f'{11 * scale:.1f}'),
    }
    mods['Player_Ride_Broom/ZoomLevel[2]'] = {
        'ZoomDistance': ('SET', f'{10 * scale:.1f}'),
    }
    mods['Player_Ride_Broom/ZoomLevel[3]'] = {
        'ZoomDistance': ('SET', f'{14 * scale:.1f}'),
    }
    return mods


# ── Shared base (always applied) ──────────────────────────────────
# Smoothing, horse camera fixes, steadycam behavior, FoV normalization.
# These changes are present in ALL CDCamera presets including steady_nofov.

_SHARED_BASE = {
    # Section-level FoV normalization (reduce vanilla 45/53 to 40)
    'Player_Basic_Default_Run': {'Fov': ('SET', '40')},
    'Player_Basic_Default_Runfast': {'Fov': ('SET', '40')},
    'Player_Basic_Default_Walk': {'Fov': ('SET', '40')},
    'Player_Weapon_Default': {'Fov': ('SET', '40')},
    'Player_Weapon_Default_Run': {'Fov': ('SET', '40')},
    'Player_Weapon_Default_RunFast': {'Fov': ('SET', '40')},
    'Player_Weapon_Default_RunFast_Follow': {'Fov': ('SET', '40')},
    'Player_Weapon_Default_Walk': {'Fov': ('SET', '40')},
    'Player_Weapon_Rush': {'Fov': ('SET', '40')},
    'Player_Force_LockOn': {'Fov': ('SET', '40')},
    'Player_LockOn_Titan': {'Fov': ('SET', '40')},
    'Cinematic_LockOn': {'Fov': ('SET', '40')},
    'Player_Weapon_Down': {'Fov': ('SET', '40')},
    'Player_Weapon_Throw': {'Fov': ('SET', '40')},
    'Player_Weapon_Throwed': {'Fov': ('SET', '40')},
    'Player_Weapon_CatchThrow': {'Fov': ('SET', '40')},
    'Player_Weapon_Zoom': {'Fov': ('SET', '40')},
    'Player_Weapon_Zoom_Light': {'Fov': ('SET', '40')},
    'Player_Weapon_Zoom_Out': {'Fov': ('SET', '40')},
    'Player_Weapon_Aim_BossAttack': {'Fov': ('SET', '40')},
    'Player_Weapon_Aim_SmallBossAttack': {'Fov': ('SET', '40')},
    'Player_Ride_Aim_LockOn': {'Fov': ('SET', '40')},
    'Player_PushingObject_TwoTarget': {'Fov': ('SET', '40')},
    'Player_Ride_Warmachine': {'Fov': ('SET', '40')},
    'Player_Ride_Warmachine_Aim': {'Fov': ('SET', '40')},
    'Player_Ride_Warmachine_Dash': {'Fov': ('SET', '40')},
    'Player_Ride_Broom': {'Fov': ('SET', '40')},
    'Player_Swim_Default': {'Fov': ('SET', '40')},

    # Lock-on behavior normalization — reduce aggressive zoom-in near enemies.
    # Vanilla TwoTargetLockOn sections zoom in instantly with high clamp rates.
    # We raise minimum distances, slow down zoom transitions, reduce target
    # tracking weight, and soften screen clamping.
    'Player_Weapon_LockOn': {
        'Fov': ('SET', '40'),
        'TargetRate': ('SET', '0.25'),
        'ScreenClampRate': ('SET', '0.6'),
    },
    'Player_Weapon_LockOn/ZoomLevel[3]': {
        'ZoomDistance': ('SET', '8'),
    },
    'Player_Weapon_TwoTarget': {
        'Fov': ('SET', '40'),
        'TargetRate': ('SET', '0.25'),
        'ScreenClampRate': ('SET', '0.6'),
        'LimitUnderDistance': ('SET', '3'),
    },
    'Player_Weapon_TwoTarget/ZoomLevel[1]': {
        'ZoomDistance': ('SET', '6'),
    },
    'Player_Weapon_TwoTarget/ZoomLevel[2]': {
        'ZoomDistance': ('SET', '8'),
    },
    'Player_Interaction_TwoTarget': {
        'Fov': ('SET', '40'),
        'TargetRate': ('SET', '0.45'),
        'ScreenClampRate': ('SET', '0.65'),
    },
    'Player_Interaction_TwoTarget/ZoomLevel[1]': {
        'ZoomDistance': ('SET', '6'),
    },
    'Player_Interaction_TwoTarget/ZoomLevel[2]': {
        'ZoomDistance': ('SET', '8'),
    },
    'Player_Interaction_TwoTarget/ZoomLevel[3]': {
        'MaxZoomDistance': ('SET', '10'),
    },
    'Player_Interaction_TwoTarget/ZoomLevel[4]': {
        'MaxZoomDistance': ('SET', '10'),
    },
    'Player_FollowLearn_LockOn_Boss': {
        'Fov': ('SET', '40'),
        'ScreenClampRate': ('SET', '0.7'),
    },
    'Player_FollowLearn_LockOn_Boss/ZoomLevel[2]': {
        'ZoomDistance': ('SET', '4.5'),
    },
    'Player_FollowLearn_LockOn_Boss/ZoomLevel[3]': {
        'ZoomDistance': ('SET', '6.5'),
    },
    'Player_Weapon_LockOn_System': {
        'Fov': ('SET', '40'),
        'TargetRate': ('SET', '0.3'),
        'ScreenClampRate': ('SET', '0.65'),
    },
    'Player_Weapon_LockOn_System/ZoomLevel[2]': {
        'ZoomDistance': ('SET', '6'),
    },
    'Player_Weapon_LockOn_System/ZoomLevel[3]': {
        'Fov': ('SET', '40'),
        'ZoomDistance': ('SET', '8'),
    },
    'Player_Weapon_LockOn_System/ZoomLevel[4]': {
        'Fov': ('SET', '40'),
    },
    'Player_Revive_LockOn_System': {
        'Fov': ('SET', '40'),
        'ScreenClampRate': ('SET', '0.65'),
    },
    'Player_Revive_LockOn_System/ZoomLevel[2]': {
        'ZoomDistance': ('SET', '6'),
    },
    'Player_Revive_LockOn_System/ZoomLevel[3]': {
        'Fov': ('SET', '40'),
        'ZoomDistance': ('SET', '8'),
    },
    'Player_Revive_LockOn_System/ZoomLevel[4]': {
        'Fov': ('SET', '40'),
    },
    'Player_Weapon_LockOn_Non_Rotate': {
        'Fov': ('SET', '40'),
        'ScreenClampRate': ('SET', '0.6'),
    },
    'Player_Weapon_LockOn_Non_Rotate/ZoomLevel[3]': {
        'ZoomDistance': ('SET', '8'),
    },
    'Player_Weapon_LockOn_WrestleOnly': {
        'Fov': ('SET', '40'),
        'ScreenClampRate': ('SET', '0.6'),
    },
    'Player_Weapon_LockOn_WrestleOnly/ZoomLevel[3]': {
        'ZoomDistance': ('SET', '8'),
    },
    'Player_StartAggro_TwoTarget': {
        'Fov': ('SET', '40'),
        'ScreenClampRate': ('SET', '0.6'),
    },
    'Player_Wanted_TwoTarget': {
        'Fov': ('SET', '40'),
        'ScreenClampRate': ('SET', '0.6'),
    },

    # On-foot ZoomDistance normalization — vanilla changes distance when you
    # walk/run causing constant zoom in/out. Anchor to idle values (3.4/6/8).
    'Player_Basic_Default_Walk/ZoomLevel[2]': {
        'ZoomDistance': ('SET', '3.4'),
    },
    'Player_Basic_Default_Walk/ZoomLevel[3]': {
        'ZoomDistance': ('SET', '6'),
    },
    'Player_Basic_Default_Walk/ZoomLevel[4]': {
        'ZoomDistance': ('SET', '8'),
    },
    'Player_Basic_Default_Run/ZoomLevel[2]': {
        'ZoomDistance': ('SET', '3.4'),
    },
    'Player_Basic_Default_Run/ZoomLevel[3]': {
        'ZoomDistance': ('SET', '6'),
    },
    'Player_Basic_Default_Run/ZoomLevel[4]': {
        'ZoomDistance': ('SET', '8'),
    },
    'Player_Basic_Default_Runfast/ZoomLevel[2]': {
        'ZoomDistance': ('SET', '3.4'),
    },
    'Player_Basic_Default_Runfast/ZoomLevel[3]': {
        'ZoomDistance': ('SET', '6'),
    },
    'Player_Basic_Default_Runfast/ZoomLevel[4]': {
        'ZoomDistance': ('SET', '8'),
    },

    # Combat ZoomDistance normalization — vanilla changes distance when you
    # walk/run/guard causing constant zoom in/out during combat.
    # Anchor everything to weapon idle values (3.4 / 6.0 / 8.0).
    'Player_Weapon_Default_Walk/ZoomLevel[2]': {
        'ZoomDistance': ('SET', '3.4'),
    },
    'Player_Weapon_Default_Walk/ZoomLevel[3]': {
        'ZoomDistance': ('SET', '6'),
    },
    'Player_Weapon_Default_Walk/ZoomLevel[4]': {
        'ZoomDistance': ('SET', '8'),
    },
    'Player_Weapon_Default_Run/ZoomLevel[2]': {
        'ZoomDistance': ('SET', '3.4'),
    },
    'Player_Weapon_Default_Run/ZoomLevel[3]': {
        'ZoomDistance': ('SET', '6'),
    },
    'Player_Weapon_Default_Run/ZoomLevel[4]': {
        'ZoomDistance': ('SET', '8'),
    },
    'Player_Weapon_Default_RunFast/ZoomLevel[2]': {
        'ZoomDistance': ('SET', '3.4'),
    },
    'Player_Weapon_Default_RunFast/ZoomLevel[3]': {
        'ZoomDistance': ('SET', '6'),
    },
    'Player_Weapon_Default_RunFast/ZoomLevel[4]': {
        'ZoomDistance': ('SET', '8'),
    },
    'Player_Weapon_Default_RunFast_Follow/ZoomLevel[2]': {
        'ZoomDistance': ('SET', '3.4'),
    },
    'Player_Weapon_Default_RunFast_Follow/ZoomLevel[3]': {
        'ZoomDistance': ('SET', '6'),
    },
    'Player_Weapon_Default_RunFast_Follow/ZoomLevel[4]': {
        'ZoomDistance': ('SET', '8'),
    },
    'Player_Weapon_Guard': {
        'Fov': ('SET', '40'),
    },
    'Player_Weapon_Guard/ZoomLevel[2]': {
        'ZoomDistance': ('SET', '3.4'),
    },
    'Player_Weapon_Guard/ZoomLevel[3]': {
        'ZoomDistance': ('SET', '6'),
    },
    'Player_Weapon_Guard/ZoomLevel[4]': {
        'ZoomDistance': ('SET', '8'),
    },
    'Player_Weapon_Rush/ZoomLevel[2]': {
        'ZoomDistance': ('SET', '3.4'),
    },
    'Player_Weapon_Rush/ZoomLevel[3]': {
        'ZoomDistance': ('SET', '6'),
    },
    'Player_Weapon_Rush/ZoomLevel[4]': {
        'ZoomDistance': ('SET', '8'),
    },

    # Player OffsetByVelocity elimination (remove camera sway on move)
    'Player_Basic_Default_Run/OffsetByVelocity': {
        'OffsetLength': ('SET', '0'),
    },
    'Player_Basic_Default_Runfast/OffsetByVelocity': {
        'OffsetLength': ('SET', '0.0'),
    },
    'Player_Weapon_Default_Run/OffsetByVelocity': {
        'OffsetLength': ('SET', '0'),
    },
    'Player_Weapon_Default_RunFast/OffsetByVelocity': {
        'OffsetLength': ('SET', '0.0'),
    },
    'Player_Weapon_Default_RunFast_Follow/OffsetByVelocity': {
        'OffsetLength': ('SET', '0.0'),
    },

    # Animal mount smoothing
    'Player_Animal_Default/CameraBlendParameter': {
        'BlendInTime': ('SET', '0.3'),
    },
    'Player_Animal_Default_Run/CameraBlendParameter': {
        'BlendInTime': ('SET', '0.3'),
    },
    'Player_Animal_Default_Run/OffsetByVelocity': {
        'OffsetLength': ('SET', '0'),
    },
    'Player_Animal_Default_Runfast/CameraBlendParameter': {
        'BlendInTime': ('SET', '0.3'),
    },
    'Player_Animal_Default_Runfast/OffsetByVelocity': {
        'OffsetLength': ('SET', '0.0'),
        'DampSpeed': ('SET', '0.5'),
    },
    'Player_Animal_Default_Walk/CameraBlendParameter': {
        'BlendInTime': ('SET', '0.3'),
    },

    # Horse — blend and offset
    'Player_Ride_Horse/CameraBlendParameter': {
        'BlendInTime': ('SET', '0.3'),
        'BlendOutTime': ('SET', '0.3'),
    },
    'Player_Ride_Horse/OffsetByVelocity': {
        'OffsetLength': ('SET', '0.0'),
    },

    # Horse Run
    'Player_Ride_Horse_Run': {
        'FollowPitchSpeedRate': ('SET', '0.8'),
        'FollowStartTime': ('SET', '1'),
    },
    'Player_Ride_Horse_Run/OffsetByVelocity': {
        'OffsetLength': ('SET', '0.0'),
    },

    # Horse Fast Run
    'Player_Ride_Horse_Fast_Run': {
        'FollowPitchSpeedRate': ('SET', '0.8'),
        'FollowStartTime': ('SET', '1'),
        'FollowYawSpeedRate': ('SET', '0.8'),
    },
    'Player_Ride_Horse_Fast_Run/CameraBlendParameter': {
        'BlendInTime': ('SET', '0.3'),
        'BlendOutTime': ('SET', '0.3'),
    },
    'Player_Ride_Horse_Fast_Run/OffsetByVelocity': {
        'DampSpeed': ('SET', '0.5'),
        'OffsetLength': ('SET', '0.0'),
    },

    # Horse Dash
    'Player_Ride_Horse_Dash': {
        'Fov': ('SET', '40'),
        'FollowPitchSpeedRate': ('SET', '0.8'),
        'FollowStartTime': ('SET', '1'),
        'FollowYawSpeedRate': ('SET', '0.8'),
    },
    'Player_Ride_Horse_Dash/CameraBlendParameter': {
        'BlendInTime': ('SET', '0.3'),
        'BlendOutTime': ('SET', '0.3'),
    },
    'Player_Ride_Horse_Dash/OffsetByVelocity': {
        'DampSpeed': ('SET', '0.5'),
        'OffsetLength': ('SET', '0.0'),
    },

    # Horse Dash Attack
    'Player_Ride_Horse_Dash_Att': {
        'Fov': ('SET', '40'),
        'FollowPitchSpeedRate': ('SET', '0.8'),
        'FollowStartTime': ('SET', '1'),
        'FollowYawSpeedRate': ('SET', '0.8'),
    },
    'Player_Ride_Horse_Dash_Att/CameraBlendParameter': {
        'BlendInTime': ('SET', '0.3'),
        'BlendOutTime': ('SET', '0.3'),
    },
    'Player_Ride_Horse_Dash_Att/CameraDamping': {
        'PivotDampingMaxDistance': ('SET', '0.5'),
    },
    'Player_Ride_Horse_Dash_Att/OffsetByVelocity': {
        'DampSpeed': ('SET', '0.5'),
        'OffsetLength': ('SET', '0.0'),
    },

    # Horse Thrust Attack
    'Player_Ride_Horse_Att_Thrust': {
        'Fov': ('SET', '40'),
        'FollowPitchSpeedRate': ('SET', '0.8'),
        'FollowStartTime': ('SET', '1'),
        'FollowYawSpeedRate': ('SET', '0.8'),
    },

    # Horse Left/Right Attacks
    'Player_Ride_Horse_Att_L': {
        'Fov': ('SET', '40'),
    },
    'Player_Ride_Horse_Att_L/CameraBlendParameter': {
        'BlendInTime': ('SET', '0.3'),
        'BlendOutTime': ('SET', '0.3'),
    },
    'Player_Ride_Horse_Att_L/OffsetByVelocity': {
        'OffsetLength': ('SET', '0.0'),
    },
    'Player_Ride_Horse_Att_R': {
        'Fov': ('SET', '40'),
    },
    'Player_Ride_Horse_Att_R/CameraBlendParameter': {
        'BlendInTime': ('SET', '0.3'),
        'BlendOutTime': ('SET', '0.3'),
    },
    'Player_Ride_Horse_Att_R/OffsetByVelocity': {
        'OffsetLength': ('SET', '0.0'),
    },

    # Elephant mount
    'Player_Ride_Elephant': {
        'Fov': ('SET', '40'),
        'FollowPitchSpeedRate': ('SET', '0.8'),
        'FollowYawSpeedRate': ('SET', '0.8'),
    },
    'Player_Ride_Elephant/CameraBlendParameter': {
        'BlendInTime': ('SET', '0.3'),
        'BlendOutTime': ('SET', '0.3'),
    },
    'Player_Ride_Elephant/CameraDamping': {
        'PivotDampingMaxDistance': ('SET', '0.5'),
    },
    'Player_Ride_Elephant/OffsetByVelocity': {
        'OffsetLength': ('SET', '0.0'),
    },

    # Wyvern mount
    'Player_Ride_Wyvern': {
        'Fov': ('SET', '50'),
        'FollowStartTime': ('SET', '1'),
        'FollowYawSpeedRate': ('SET', '0.8'),
    },
    'Player_Ride_Wyvern/OffsetByVelocity': {
        'OffsetLength': ('SET', '0.0'),
    },
}


# ── Shared steadycam UpOffset normalization ────────────────────────
# These UpOffset→0.3 changes are present in ALL CDCamera presets and
# form the steadycam foundation that style layers may override.

def _build_shared_steadycam():
    """Build UpOffset normalization applied to all configs.

    0.0 = head height, so the shared base sets everything to head level.
    Style layers then override with their specific heights.
    """
    mods = {}

    # Basic Default: ZL3, ZL4 only
    mods['Player_Basic_Default/ZoomLevel[3]'] = {'UpOffset': ('SET', '0.0')}
    mods['Player_Basic_Default/ZoomLevel[4]'] = {'UpOffset': ('SET', '0.0')}

    # Aim_Zoom: ZL3 only
    mods['Player_Basic_Default_Aim_Zoom/ZoomLevel[3]'] = {
        'UpOffset': ('SET', '0.0'),
    }

    # Basic Walk / Run / Runfast: ZL2, ZL3, ZL4
    for sec in ('Player_Basic_Default_Walk', 'Player_Basic_Default_Run',
                'Player_Basic_Default_Runfast'):
        mods[f'{sec}/ZoomLevel[2]'] = {'UpOffset': ('SET', '0.0')}
        mods[f'{sec}/ZoomLevel[3]'] = {'UpOffset': ('SET', '0.0')}
        mods[f'{sec}/ZoomLevel[4]'] = {'UpOffset': ('SET', '0.0')}

    # All Weapon sections: ZL3, ZL4 only
    for sec in _WEAPON_SECTIONS:
        mods[f'{sec}/ZoomLevel[3]'] = {'UpOffset': ('SET', '0.0')}
        mods[f'{sec}/ZoomLevel[4]'] = {'UpOffset': ('SET', '0.0')}

    return mods


# ── Style layers ───────────────────────────────────────────────────
# Applied on top of the shared base. 'default' = no style changes.

def _build_western():
    """Western/Heroic: shoulder-level OTS, ZoomDistance 2.5/5/8."""
    mods = {}
    for sec in _ALL_MAIN:
        mods[f'{sec}/ZoomLevel[2]'] = {
            'UpOffset': ('SET', '-0.2'),
            'InDoorUpOffset': ('SET', '-0.2'),
            'ZoomDistance': ('SET', '2.5'),
        }
        mods[f'{sec}/ZoomLevel[3]'] = {
            'UpOffset': ('SET', '-0.2'),
            'ZoomDistance': ('SET', '5'),
        }
        mods[f'{sec}/ZoomLevel[4]'] = {
            'UpOffset': ('SET', '-0.2'),
            'ZoomDistance': ('SET', '8'),
        }
    # RightOffset 0.8 on ZL4 for walk/run + Weapon_Default (not Basic Default)
    mods['Player_Weapon_Default/ZoomLevel[4]']['RightOffset'] = ('SET', '0.8')
    for sec in _WALK_RUN:
        mods[f'{sec}/ZoomLevel[4]']['RightOffset'] = ('SET', '0.8')
    # Basic walk/run ZL2 RightOffset (not Weapon)
    for sec in _WALK_RUN:
        if sec.startswith('Player_Basic'):
            mods[f'{sec}/ZoomLevel[2]']['RightOffset'] = ('SET', '0.5')

    mods['Player_Basic_Default_Aim_Zoom/ZoomLevel[2]'] = {
        'UpOffset': ('SET', '-0.2'),
        'InDoorUpOffset': ('SET', '-0.2'),
        'ZoomDistance': ('SET', '2.5'),
    }
    mods['Player_Basic_Default_Aim_Zoom/ZoomLevel[3]'] = {
        'UpOffset': ('SET', '-0.2'),
    }
    return mods


def _build_cinematic():
    """Cinematic/Panoramic: head-height wide pullback 3.75/7.5/11.25."""
    mods = {}
    for sec in _ALL_MAIN:
        mods[f'{sec}/ZoomLevel[2]'] = {
            'UpOffset': ('SET', '0.0'),
            'ZoomDistance': ('SET', '3.75'),
        }
        mods[f'{sec}/ZoomLevel[3]'] = {
            'UpOffset': ('SET', '0.0'),
            'ZoomDistance': ('SET', '7.5'),
        }
        mods[f'{sec}/ZoomLevel[4]'] = {
            'UpOffset': ('SET', '0.0'),
            'ZoomDistance': ('SET', '11.25'),
        }
    # RightOffset on walk/run + Weapon_Default ZL4
    mods['Player_Weapon_Default/ZoomLevel[4]']['RightOffset'] = ('SET', '0.8')
    for sec in _WALK_RUN:
        mods[f'{sec}/ZoomLevel[4]']['RightOffset'] = ('SET', '0.8')
        if not sec.startswith('Player_Weapon'):
            mods[f'{sec}/ZoomLevel[2]']['RightOffset'] = ('SET', '0.5')

    mods['Player_Basic_Default_Aim_Zoom/ZoomLevel[2]'] = {
        'UpOffset': ('SET', '0.0'),
        'ZoomDistance': ('SET', '3.75'),
    }
    mods['Player_Basic_Default_Aim_Zoom/ZoomLevel[3]'] = {
        'UpOffset': ('SET', '0.0'),
    }

    _merge(mods, _build_mount_distances(1.2))
    return mods


def _build_immersive():
    """Immersive/Survival: shoulder-level OTS, tighter 2.0/4.0/6.0."""
    mods = {}
    for sec in _ALL_MAIN:
        mods[f'{sec}/ZoomLevel[2]'] = {
            'UpOffset': ('SET', '-0.2'),
            'InDoorUpOffset': ('SET', '-0.2'),
            'ZoomDistance': ('SET', '2.0'),
        }
        mods[f'{sec}/ZoomLevel[3]'] = {
            'UpOffset': ('SET', '-0.2'),
            'ZoomDistance': ('SET', '4.0'),
        }
        mods[f'{sec}/ZoomLevel[4]'] = {
            'UpOffset': ('SET', '-0.2'),
            'RightOffset': ('SET', '0.8'),
            'ZoomDistance': ('SET', '6.0'),
        }
    for sec in _WALK_RUN:
        mods[f'{sec}/ZoomLevel[2]']['RightOffset'] = ('SET', '0.5')

    mods['Player_Basic_Default_Aim_Zoom/ZoomLevel[2]'] = {
        'UpOffset': ('SET', '-0.2'),
        'InDoorUpOffset': ('SET', '-0.2'),
        'ZoomDistance': ('SET', '2.0'),
    }
    mods['Player_Basic_Default_Aim_Zoom/ZoomLevel[3]'] = {
        'UpOffset': ('SET', '-0.2'),
    }

    _merge(mods, _build_mount_distances(0.75))
    return mods


def _build_lowcam_variant(base_up, indoor_up=None):
    """Low camera variants with configurable vertical offset.

    UpOffset 0.0 = head level, -1.5 = ground level.
    ZoomDistances consistent across all states: 2.5/5/8.

    Args:
        base_up: Main UpOffset value string for all sections.
        indoor_up: InDoorUpOffset for ZL3 (defaults to base_up).
    """
    if indoor_up is None:
        indoor_up = base_up
    up = base_up
    mods = {}

    mods['Player_Basic_Default/ZoomLevel[2]'] = {
        'UpOffset': ('SET', up),
        'InDoorUpOffset': ('SET', up),
        'ZoomDistance': ('SET', '2.5'),
    }
    mods['Player_Basic_Default/ZoomLevel[3]'] = {
        'UpOffset': ('SET', up),
        'InDoorUpOffset': ('SET', indoor_up),
        'ZoomDistance': ('SET', '5'),
    }
    mods['Player_Basic_Default/ZoomLevel[4]'] = {
        'UpOffset': ('SET', up),
        'InDoorUpOffset': ('SET', up),
        'ZoomDistance': ('SET', '8'),
    }

    for sec in ('Player_Basic_Default_Walk', 'Player_Basic_Default_Run',
                'Player_Basic_Default_Runfast'):
        mods[f'{sec}/ZoomLevel[2]'] = {
            'UpOffset': ('SET', up),
            'InDoorUpOffset': ('SET', up),
            'RightOffset': ('SET', '0.5'),
            'ZoomDistance': ('SET', '2.5'),
        }
        mods[f'{sec}/ZoomLevel[3]'] = {
            'UpOffset': ('SET', up),
            'InDoorUpOffset': ('SET', indoor_up),
            'ZoomDistance': ('SET', '5'),
        }
        mods[f'{sec}/ZoomLevel[4]'] = {
            'UpOffset': ('SET', up),
            'InDoorUpOffset': ('SET', up),
            'RightOffset': ('SET', '0.8000'),
            'ZoomDistance': ('SET', '8'),
        }

    mods['Player_Weapon_Default/ZoomLevel[2]'] = {
        'UpOffset': ('SET', up),
        'InDoorUpOffset': ('SET', up),
        'ZoomDistance': ('SET', '2.5'),
    }
    mods['Player_Weapon_Default/ZoomLevel[3]'] = {
        'UpOffset': ('SET', up),
        'InDoorUpOffset': ('SET', indoor_up),
        'ZoomDistance': ('SET', '5'),
    }
    mods['Player_Weapon_Default/ZoomLevel[4]'] = {
        'UpOffset': ('SET', up),
        'InDoorUpOffset': ('SET', up),
        'RightOffset': ('SET', '0.8000'),
        'ZoomDistance': ('SET', '8'),
    }

    for sec in ('Player_Weapon_Default_Walk', 'Player_Weapon_Default_Run',
                'Player_Weapon_Default_RunFast',
                'Player_Weapon_Default_RunFast_Follow'):
        mods[f'{sec}/ZoomLevel[2]'] = {
            'UpOffset': ('SET', up),
            'InDoorUpOffset': ('SET', up),
            'ZoomDistance': ('SET', '2.5'),
        }
        mods[f'{sec}/ZoomLevel[3]'] = {
            'UpOffset': ('SET', up),
            'InDoorUpOffset': ('SET', indoor_up),
            'ZoomDistance': ('SET', '5'),
        }
        mods[f'{sec}/ZoomLevel[4]'] = {
            'UpOffset': ('SET', up),
            'InDoorUpOffset': ('SET', up),
            'RightOffset': ('SET', '0.8000'),
            'ZoomDistance': ('SET', '8'),
        }

    for sec in ('Player_Weapon_Guard', 'Player_Weapon_Rush'):
        mods[f'{sec}/ZoomLevel[2]'] = {
            'UpOffset': ('SET', up),
            'InDoorUpOffset': ('SET', up),
            'ZoomDistance': ('SET', '2.5'),
        }
        mods[f'{sec}/ZoomLevel[3]'] = {
            'UpOffset': ('SET', up),
            'InDoorUpOffset': ('SET', indoor_up),
            'ZoomDistance': ('SET', '5'),
        }
        mods[f'{sec}/ZoomLevel[4]'] = {
            'UpOffset': ('SET', up),
            'InDoorUpOffset': ('SET', up),
            'RightOffset': ('SET', '0.8000'),
            'ZoomDistance': ('SET', '8'),
        }

    mods['Player_Basic_Default_Aim_Zoom/ZoomLevel[2]'] = {
        'UpOffset': ('SET', up),
        'InDoorUpOffset': ('SET', up),
        'ZoomDistance': ('SET', '2.5'),
    }
    mods['Player_Basic_Default_Aim_Zoom/ZoomLevel[3]'] = {
        'UpOffset': ('SET', up),
        'InDoorUpOffset': ('SET', indoor_up),
    }

    return mods


def _build_re2():
    """RE2-style tight over-the-shoulder: ZoomDistance 1.8/3/6, consistent."""
    mods = {}
    for sec in _ALL_MAIN:
        mods[f'{sec}/ZoomLevel[2]'] = {'ZoomDistance': ('SET', '1.8')}
        mods[f'{sec}/ZoomLevel[3]'] = {'ZoomDistance': ('SET', '3')}
        mods[f'{sec}/ZoomLevel[4]'] = {'ZoomDistance': ('SET', '6')}
    for sec in _WALK_RUN:
        mods[f'{sec}/ZoomLevel[2]']['RightOffset'] = ('SET', '0.4')
        mods[f'{sec}/ZoomLevel[3]']['RightOffset'] = ('SET', '0.7')

    # ZL4 RightOffset for walk/run + Weapon_Default
    for sec in _WALK_RUN + ['Player_Weapon_Default']:
        mods[f'{sec}/ZoomLevel[4]']['RightOffset'] = ('SET', '0.7000')

    mods['Player_Basic_Default_Aim_Zoom/ZoomLevel[2]'] = {
        'ZoomDistance': ('SET', '1.8'),
    }
    mods['Player_Basic_Default_Aim_Zoom/ZoomLevel[3]'] = {
        'ZoomDistance': ('SET', '3'),
    }

    _merge(mods, _build_mount_distances(0.65))
    return mods


def build_custom(distance, up_offset, right_offset):
    """Build style mods from user-supplied raw values.

    Applies the given ZoomDistance, UpOffset, and RightOffset uniformly
    across all main camera sections at ZoomLevel 2/3/4, scaling distance
    proportionally (ZL2=close, ZL3=mid, ZL4=far).
    """
    zl2_dist = round(distance * 0.5, 1)
    zl3_dist = round(distance, 1)
    zl4_dist = round(distance * 1.5, 1)

    up_str = f'{up_offset:.2f}'
    ro_str = f'{right_offset:.2f}'

    mods = {}
    for sec in _ALL_MAIN:
        mods[f'{sec}/ZoomLevel[2]'] = {
            'UpOffset': ('SET', up_str),
            'InDoorUpOffset': ('SET', up_str),
            'ZoomDistance': ('SET', f'{zl2_dist}'),
        }
        mods[f'{sec}/ZoomLevel[3]'] = {
            'UpOffset': ('SET', up_str),
            'ZoomDistance': ('SET', f'{zl3_dist}'),
        }
        mods[f'{sec}/ZoomLevel[4]'] = {
            'UpOffset': ('SET', up_str),
            'ZoomDistance': ('SET', f'{zl4_dist}'),
        }
        if abs(right_offset) > 0.01:
            mods[f'{sec}/ZoomLevel[3]']['RightOffset'] = ('SET', ro_str)
            mods[f'{sec}/ZoomLevel[4]']['RightOffset'] = ('SET', ro_str)

    for extra in ('Player_Basic_Default_Aim_Zoom',):
        mods[f'{extra}/ZoomLevel[2]'] = {
            'UpOffset': ('SET', up_str),
            'InDoorUpOffset': ('SET', up_str),
            'ZoomDistance': ('SET', f'{zl2_dist}'),
        }
        mods[f'{extra}/ZoomLevel[3]'] = {
            'UpOffset': ('SET', up_str),
            'ZoomDistance': ('SET', f'{zl3_dist}'),
        }
        mods[f'{extra}/ZoomLevel[4]'] = {
            'UpOffset': ('SET', up_str),
            'ZoomDistance': ('SET', f'{zl4_dist}'),
        }

    return mods


_STYLE_BUILDERS = {
    'western':   _build_western,
    'cinematic': _build_cinematic,
    'immersive': _build_immersive,
    'lowcam':    lambda: _build_lowcam_variant('-0.8'),
    'vlowcam':   lambda: _build_lowcam_variant('-1.2'),
    'ulowcam':   lambda: _build_lowcam_variant('-1.5'),
    're2':       _build_re2,
}


# ── Bane layer (zero RightOffset for centered framing) ────────────

def _build_bane_mods():
    """Set RightOffset=0.0 on ZL 2/3/4 across all TPS sections.

    Also zeros InDoorRightOffset where vanilla defines it:
    ZL2 on Basic sections + Aim_Zoom, and ZL4 on Runfast.
    """
    mods = {}
    _indoor_right_zl2 = {'Player_Basic_Default', 'Player_Basic_Default_Aim_Zoom',
                          'Player_Weapon_Default'}
    for sec in _BANE_SECTIONS:
        for lvl in (2, 3, 4):
            key = f'{sec}/ZoomLevel[{lvl}]'
            entry = mods.setdefault(key, {})
            entry['RightOffset'] = ('SET', '0.0')
            if lvl == 2 and sec in _indoor_right_zl2:
                entry['InDoorRightOffset'] = ('SET', '0.0')
            if lvl == 4 and sec == 'Player_Basic_Default_Runfast':
                entry['InDoorRightOffset'] = ('SET', '0.0')

    # Gliding and freefall
    _air_bane = {
        'Player_Basic_Gliding/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_Gliding/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_Gliding_Fast/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_Gliding_Fast/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_Gliding_Zoom/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_Gliding_Fall/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_Gliding_Fall/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Glide_Kick_Aim_Zoom/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Glide_Bow_Aim_Zoom/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_FreeFall_Start/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_FreeFall_Start/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_FreeFall/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_FreeFall/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_FreeFall_Lv2/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_FreeFall_Lv2/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_FreeFall_Aim/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_FreeFall_Aim/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_SuperJump/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_SuperJump/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
    }
    mods.update(_air_bane)

    # Swimming, climbing, traversal
    _traversal_bane = {
        'Player_Swim_Default/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Swim_Default/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_PointClimb/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_PointClimb/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_PointClimb/ZoomLevel[4]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_PointClimb_Follow/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_PointClimb_Follow/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_PointClimb_Follow/ZoomLevel[4]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_CharacterClimb/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_CharacterClimb/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_CharacterClimb/ZoomLevel[4]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_Climb/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_Climb/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_RopeSwing/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_RopeSwing/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_RopeSwing/ZoomLevel[4]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_RopePull/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_RopePull/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_RopePull/ZoomLevel[4]': {'RightOffset': ('SET', '0.0')},
        'Player_WaterFallPass/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_WaterFallPass/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_WaterFallPass/ZoomLevel[4]': {'RightOffset': ('SET', '0.0')},
        'Player_Wood_Hanging/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_Wagon/ZoomLevel[1]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_Wagon/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Wagon_Wait/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Wagon_Wait/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
    }
    mods.update(_traversal_bane)

    # Weapon combat states (guard, rush, zoom, lock-on, aim)
    _combat_bane = {
        'Player_Weapon_Guard/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Weapon_Guard/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Weapon_Guard/ZoomLevel[4]': {'RightOffset': ('SET', '0.0')},
        'Player_Weapon_Rush/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Weapon_Rush/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Weapon_Rush/ZoomLevel[4]': {'RightOffset': ('SET', '0.0')},
        'Player_Weapon_Down/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Weapon_Down/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Weapon_Down/ZoomLevel[4]': {'RightOffset': ('SET', '0.0')},
        'Player_Weapon_Indoor/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Weapon_Zoom/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Weapon_Zoom/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Weapon_Zoom/ZoomLevel[4]': {'RightOffset': ('SET', '0.0')},
        'Player_Weapon_Zoom_Light/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Weapon_Zoom_Light/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Weapon_Zoom_Out/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Weapon_Zoom_Out/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Weapon_LockOn_System/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Weapon_LockOn_System/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Weapon_LockOn_System/ZoomLevel[4]': {'RightOffset': ('SET', '0.0')},
        'Player_Revive_LockOn_System/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Revive_LockOn_System/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Revive_LockOn_System/ZoomLevel[4]': {'RightOffset': ('SET', '0.0')},
        'Cinematic_LockOn/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Cinematic_LockOn/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Cinematic_LockOn/ZoomLevel[4]': {'RightOffset': ('SET', '0.0')},
        'Player_Hit_Throw/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Hit_Throw/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Hit_Throw/ZoomLevel[4]': {'RightOffset': ('SET', '0.0')},
    }
    mods.update(_combat_bane)

    # Animal form
    _animal_bane = {
        'Player_Animal_Default/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Animal_Default/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Animal_Default/ZoomLevel[4]': {'RightOffset': ('SET', '0.0')},
        'Player_Animal_Default_Walk/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Animal_Default_Walk/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Animal_Default_Walk/ZoomLevel[4]': {'RightOffset': ('SET', '0.0')},
        'Player_Animal_Default_Run/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Animal_Default_Run/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Animal_Default_Run/ZoomLevel[4]': {'RightOffset': ('SET', '0.0')},
        'Player_Animal_Default_Runfast/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Animal_Default_Runfast/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Animal_Default_Runfast/ZoomLevel[4]': {'RightOffset': ('SET', '0.0')},
    }
    mods.update(_animal_bane)

    # Misc gameplay (rest, contemplation, fishing, etc.)
    _misc_bane = {
        'Player_Rest/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Rest/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Rest/ZoomLevel[4]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_NoZoom/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_NoZoom/ZoomLevel[4]': {'RightOffset': ('SET', '0.0')},
        'Player_Basic_Teleport/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
    }
    mods.update(_misc_bane)

    # Ride and mount sections
    _ride_bane = {
        'Player_Ride_Broom/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Ride_Broom/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Ride_Canoe/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Ride_Canoe/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Ride_Elephant/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Ride_Elephant/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Ride_Horse/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Ride_Horse/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Ride_Horse_Att_L/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Ride_Horse_Att_L/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Ride_Horse_Att_R/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Ride_Horse_Att_R/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Ride_Horse_Att_Thrust/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Ride_Horse_Att_Thrust/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Ride_Horse_Dash/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Ride_Horse_Dash/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Ride_Horse_Dash_Att/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Ride_Horse_Dash_Att/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Ride_Horse_Fast_Run/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Ride_Horse_Fast_Run/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Ride_Horse_Run/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Ride_Horse_Run/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Ride_Warmachine/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Ride_Warmachine/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Ride_Warmachine_Aim/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Ride_Warmachine_Dash/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Ride_Wyvern/ZoomLevel[2]': {'RightOffset': ('SET', '0.0')},
        'Player_Ride_Wyvern/ZoomLevel[3]': {'RightOffset': ('SET', '0.0')},
        'Player_Ride_Wyvern/ZoomLevel[4]': {'RightOffset': ('SET', '0.0')},
    }
    mods.update(_ride_bane)

    return mods


# ── Combat lock-on layers ─────────────────────────────────────────

_COMBAT_LOCKON = {
    'wide': {
        'Player_FollowLearn_LockOn_Boss/ZoomLevel[2]': {
            'ZoomDistance': ('SET', '4.5'),
        },
        'Player_FollowLearn_LockOn_Boss/ZoomLevel[3]': {
            'ZoomDistance': ('SET', '8.3'),
        },
        'Player_FollowLearn_LockOn_Boss/ZoomLevel[4]': {
            'ZoomDistance': ('SET', '9.9'),
        },
        'Player_Force_LockOn/ZoomLevel[2]': {
            'ZoomDistance': ('SET', '15'),
        },
        'Player_LockOn_Titan/ZoomLevel[1]': {
            'ZoomDistance': ('SET', '15'),
        },
        'Player_Weapon_LockOn/ZoomLevel[3]': {
            'ZoomDistance': ('SET', '9.8'),
        },
        'Player_Weapon_TwoTarget/ZoomLevel[1]': {
            'ZoomDistance': ('SET', '5.3'),
        },
        'Player_Weapon_TwoTarget/ZoomLevel[2]': {
            'ZoomDistance': ('SET', '9'),
        },
        'Player_Weapon_TwoTarget/ZoomLevel[3]': {
            'ZoomDistance': ('SET', '9'),
        },
    },
    'max': {
        'Player_FollowLearn_LockOn_Boss/ZoomLevel[2]': {
            'ZoomDistance': ('SET', '6.0'),
        },
        'Player_FollowLearn_LockOn_Boss/ZoomLevel[3]': {
            'ZoomDistance': ('SET', '9.9'),
        },
        'Player_FollowLearn_LockOn_Boss/ZoomLevel[4]': {
            'ZoomDistance': ('SET', '9.9'),
        },
        'Player_Force_LockOn/ZoomLevel[2]': {
            'ZoomDistance': ('SET', '20'),
        },
        'Player_LockOn_Titan/ZoomLevel[1]': {
            'ZoomDistance': ('SET', '20'),
        },
        'Player_Weapon_LockOn/ZoomLevel[3]': {
            'ZoomDistance': ('SET', '9.9'),
        },
        'Player_Weapon_TwoTarget/ZoomLevel[1]': {
            'ZoomDistance': ('SET', '7.0'),
        },
        'Player_Weapon_TwoTarget/ZoomLevel[2]': {
            'ZoomDistance': ('SET', '9'),
        },
        'Player_Weapon_TwoTarget/ZoomLevel[3]': {
            'ZoomDistance': ('SET', '9'),
        },
    },
}


# ── Composition ────────────────────────────────────────────────────

def _merge(base, overlay):
    """Deep-merge overlay into base (overlay wins on conflict)."""
    for key, attrs in overlay.items():
        if key in base:
            base[key].update(attrs)
        else:
            base[key] = dict(attrs)


_STYLE_UP_OFFSET = {
    'default':   0.0,
    'western':  -0.2,
    'cinematic': 0.0,
    'immersive':-0.2,
    'lowcam':   -0.8,
    'vlowcam':  -1.2,
    'ulowcam':  -1.5,
    're2':       0.0,
}


def build_modifications(style, fov, bane, combat, mount_height=False,
                        custom_up=None):
    """Build the complete modification set from user choices.

    Args:
        style: 'default', 'western', 'cinematic', 'immersive',
               'lowcam', 'vlowcam', 'ulowcam', 're2', or 'custom'
        fov: int — additive FoV delta (0=no change, 10/15/20/25/30/40)
        bane: bool — zero RightOffset for centered framing
        combat: 'default', 'wide', or 'max'
        mount_height: bool — apply player height to mount cameras
        custom_up: float or None — explicit UpOffset for custom style
                   (used with mount_height when style=='custom')

    Returns:
        ModificationSet with element_mods and fov_value
    """
    mods = {}

    # Layer 1: shared base — always applied
    _merge(mods, _SHARED_BASE)
    _merge(mods, _build_shared_steadycam())

    # Layer 2: style-specific overrides
    if style in _STYLE_BUILDERS:
        _merge(mods, _STYLE_BUILDERS[style]())

    # Layer 3: bane — zero RightOffset (applied after style so it wins)
    if bane:
        _merge(mods, _build_bane_mods())

    # Layer 4: combat lock-on distance
    if combat in _COMBAT_LOCKON:
        _merge(mods, _COMBAT_LOCKON[combat])

    # Layer 5: match mount camera height to player
    if mount_height:
        if custom_up is not None:
            up = custom_up
        else:
            up = _STYLE_UP_OFFSET.get(style, 0.0)
        _merge(mods, _build_mount_height_mods(up))

    return ModificationSet(element_mods=mods, fov_value=fov)

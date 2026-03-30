"""Ultimate Camera Mod — GUI camera mod installer for Crimson Desert.

Standalone entry point with a Windows desktop-style GUI using customtkinter.
Replaces install.bat entirely. Designed to be compiled with Nuitka into
a standalone .exe.
"""

import base64
import ctypes
import json
import math
import os
import sys
import threading
import tkinter as tk
import webbrowser
import winreg

VER = "1.5"

NEXUS_URL = "https://www.nexusmods.com/crimsondesert/mods/383"
GITHUB_URL = "https://github.com/FitzDegenhub/UltrawideDesert"

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import customtkinter as ctk
from camera_mod import install_camera_mod, restore_camera
import camera_mod
from camera_rules import build_custom, _STYLE_BUILDERS
from hud_mod import install_centered_hud, restore_hud
from paz_parse import parse_pamt

# ---------------------------------------------------------------------------
# Brand colors
# ---------------------------------------------------------------------------
BG_DARK = "#1a1a1a"
BG_PANEL = "#242424"
BG_INPUT = "#2e2e2e"
ACCENT = "#c8a24e"
ACCENT_HOVER = "#dab95e"
TEXT_PRIMARY = "#e0e0e0"
TEXT_SECONDARY = "#999999"
TEXT_DIM = "#666666"
SUCCESS = "#4ade80"
ERROR = "#ef4444"
WARN = "#f59e0b"
BORDER = "#333333"
TAB_ACTIVE = "#2e2e2e"
TAB_INACTIVE = "#1a1a1a"

CVS_BG = "#1e1e1e"
CVS_GROUND = "#3a3a3a"
CVS_CHAR = "#c8a24e"
CVS_CAM = "#aaaaaa"
CVS_CONE = "#5a4820"
CVS_CONE_LINE = "#7a6830"
CVS_LABEL = "#666666"
CVS_MEASURE = "#555555"
CVS_OFFSET_LINE = "#c8a24e"

# ---------------------------------------------------------------------------
# Config data — presets tab only uses the first 8 (no "custom" entry)
# ---------------------------------------------------------------------------
STYLES = [
    ("western",    "Heroic  -  Shoulder-level OTS, great framing"),
    ("cinematic",  "Panoramic  -  Head-height wide pullback, filmic"),
    ("default",    "Smoothed  -  Vanilla framing + smoothing"),
    ("immersive",  "Close-Up  -  Shoulder OTS, tighter (16:9 feel)"),
    ("lowcam",     "Low Rider  -  Hip-level, full body + horizon"),
    ("vlowcam",    "Knee Cam  -  Knee-height dramatic low angle"),
    ("ulowcam",    "Dirt Cam  -  Ground-level, extreme low"),
    ("re2",        "Survival  -  Tight horror-game OTS (16:9 feel)"),
]
STYLE_IDS = [s[0] for s in STYLES]
STYLE_LABELS = [s[1] for s in STYLES]

FOV_OPTIONS = [
    (0,  "No change (40\u00b0)  -  Vanilla"),
    (10, "+10\u00b0 (50\u00b0)  -  Minimal, good for 16:9"),
    (15, "+15\u00b0 (55\u00b0)  -  Subtle improvement"),
    (20, "+20\u00b0 (60\u00b0)  -  Sweet spot for 21:9"),
    (25, "+25\u00b0 (65\u00b0)  -  Great for 21:9 + 32:9"),
    (30, "+30\u00b0 (70\u00b0)  -  Perfect for 32:9"),
    (40, "+40\u00b0 (80\u00b0)  -  Extreme, slight fisheye"),
]
FOV_VALUES = [f[0] for f in FOV_OPTIONS]
FOV_LABELS = [f[1] for f in FOV_OPTIONS]

COMBAT_OPTIONS = [
    ("default", "Default  -  Standard combat camera"),
    ("wide",    "Wider  -  More room to see the battlefield"),
    ("max",     "Maximum  -  Widest possible combat view"),
]
COMBAT_IDS = [c[0] for c in COMBAT_OPTIONS]
COMBAT_LABELS = [c[1] for c in COMBAT_OPTIONS]

HUD_WIDTH_MIN = 1200
HUD_WIDTH_MAX = 3840
HUD_WIDTH_DEFAULT = 2520

STYLE_PARAMS = {
    "western":   (5.0, -0.2, 0.0),
    "cinematic": (7.5,  0.0, 0.0),
    "default":   (3.4,  0.0, 0.0),
    "immersive": (4.0, -0.2, 0.0),
    "lowcam":    (5.0, -0.8, 0.0),
    "vlowcam":   (5.0, -1.2, 0.0),
    "ulowcam":   (5.0, -1.5, 0.0),
    "re2":       (3.0,  0.0, 0.7),
}

CUSTOM_LIMITS = {
    "distance":     (1.5, 12.0),
    "height":       (-1.6, 0.5),
    "right_offset": (-1.0, 1.0),
}

_PLACEHOLDER_PRESET = "(new — adjust sliders)"


# ---------------------------------------------------------------------------
# Custom presets persistence
# ---------------------------------------------------------------------------
def _exe_dir():
    return os.path.dirname(os.path.abspath(sys.argv[0]))


def _presets_dir():
    d = os.path.join(_exe_dir(), "custom_presets")
    os.makedirs(d, exist_ok=True)
    return d


def _save_custom_preset(name, distance, height, right_offset):
    path = os.path.join(_presets_dir(), f"{name}.json")
    with open(path, "w") as f:
        json.dump({"distance": distance, "height": height,
                    "right_offset": right_offset}, f, indent=2)


def _load_custom_preset(name):
    path = os.path.join(_presets_dir(), f"{name}.json")
    with open(path, "r") as f:
        return json.load(f)


def _list_custom_presets():
    d = _presets_dir()
    return sorted(
        os.path.splitext(f)[0]
        for f in os.listdir(d) if f.endswith(".json")
    )


def _delete_custom_preset(name):
    path = os.path.join(_presets_dir(), f"{name}.json")
    if os.path.exists(path):
        os.remove(path)


# ---------------------------------------------------------------------------
# Import / export string codec
# ---------------------------------------------------------------------------
_IMPORT_PREFIX = "UWD:"


def _encode_preset_string(name, distance, height, right_offset):
    payload = json.dumps(
        {"n": name, "d": round(distance, 2),
         "h": round(height, 2), "r": round(right_offset, 2)},
        separators=(",", ":"))
    return _IMPORT_PREFIX + base64.urlsafe_b64encode(
        payload.encode("utf-8")).decode("ascii")


def _decode_preset_string(text):
    text = text.strip()
    if not text.startswith(_IMPORT_PREFIX):
        raise ValueError("Not a valid preset string "
                         "(must start with UWD:)")
    b64 = text[len(_IMPORT_PREFIX):]
    try:
        raw = base64.urlsafe_b64decode(b64)
    except Exception:
        raise ValueError("Corrupt import string — base64 decode failed")
    try:
        data = json.loads(raw)
    except Exception:
        raise ValueError("Corrupt import string — JSON decode failed")
    d_min, d_max = CUSTOM_LIMITS["distance"]
    h_min, h_max = CUSTOM_LIMITS["height"]
    r_min, r_max = CUSTOM_LIMITS["right_offset"]
    name = str(data.get("n", "Imported"))[:30]
    distance = max(d_min, min(d_max, float(data.get("d", 5.0))))
    height = max(h_min, min(h_max, float(data.get("h", 0.0))))
    right_offset = max(r_min, min(r_max, float(data.get("r", 0.0))))
    return name, distance, height, right_offset


# ---------------------------------------------------------------------------
# Guide line color — barely visible against the background
# ---------------------------------------------------------------------------
CVS_GUIDE = "#282828"

# ---------------------------------------------------------------------------
# Side-view preview (position / height / offset)
# Camera base (UpOffset 0) = head level. Negative lowers toward ground
# (-1.5 = ground). Positive raises above head.
# ---------------------------------------------------------------------------
class CameraPreview(tk.Canvas):

    W = 380
    H = 330
    GROUND_Y = 285
    CHAR_X = 280
    CHAR_H = 210
    DIST_SCALE = 24
    HEAD_R = 12

    def __init__(self, master, **kw):
        super().__init__(master, width=self.W, height=self.H,
                         bg=CVS_BG, highlightthickness=0, **kw)
        self._dist = 5.0
        self._up = -0.2
        self._label = "Heroic"
        self.redraw()

    def update_params(self, dist, up, label="Custom"):
        self._dist, self._up = dist, up
        self._label = label
        self.redraw()

    @property
    def _head_top(self):
        return self.GROUND_Y - self.CHAR_H

    @property
    def _head_cy(self):
        return self._head_top + self.HEAD_R

    @property
    def _scale(self):
        return (self.GROUND_Y - self._head_cy) / 1.5

    def _body_y(self, up_offset):
        """Pixel Y for a given UpOffset value (0.0=head, -1.5=ground)."""
        return int(self._head_cy + abs(up_offset) * self._scale)

    @property
    def _shoulder_y(self):
        return self._body_y(-0.2)

    @property
    def _chest_y(self):
        return self._body_y(-0.3)

    @property
    def _hip_y(self):
        return self._body_y(-0.8)

    @property
    def _knee_y(self):
        return self._body_y(-1.2)

    def _height_zone(self, v):
        """Return a descriptive label for a height value."""
        if v >= 0.1:
            return "above head"
        if v >= -0.1:
            return "head"
        if v >= -0.25:
            return "shoulder"
        if v >= -0.4:
            return "chest"
        if v >= -0.65:
            return "stomach"
        if v >= -0.9:
            return "hip"
        if v >= -1.1:
            return "waist"
        if v >= -1.35:
            return "knee"
        if v >= -1.48:
            return "shin"
        return "ground"

    def redraw(self):
        self.delete("all")

        scale = self._scale

        guides = [
            (self._head_cy,    "head",     "0.0"),
            (self._shoulder_y, "shoulder", "-0.2"),
            (self._hip_y,      "hip",     "-0.8"),
            (self._knee_y,     "knee",    "-1.2"),
            (self.GROUND_Y,    "",         "-1.5"),
        ]
        for gy, gl, gv in guides:
            self.create_line(10, gy, self.W - 10, gy,
                             fill=CVS_GUIDE, width=1, dash=(1, 6))
            right_text = f"{gl}  ({gv})" if gv and gl else (gv or gl)
            if right_text:
                self.create_text(self.W - 8, gy, text=right_text,
                                 fill=CVS_GROUND, font=("Segoe UI", 7),
                                 anchor="e")

        cam_x = self.CHAR_X - self._dist * self.DIST_SCALE
        cam_y = self._head_cy - self._up * scale
        cam_x = max(30, min(cam_x, self.CHAR_X - 35))
        cam_y = max(28, min(cam_y, self.GROUND_Y - 10))

        self._draw_ground()
        self._draw_character()
        self._draw_camera(cam_x, cam_y)

        # distance measurement
        ly = self.GROUND_Y + 16
        self.create_line(cam_x, ly, self.CHAR_X, ly,
                         fill=CVS_MEASURE, width=1, dash=(3, 3))
        for tx in (cam_x, self.CHAR_X):
            self.create_line(tx, ly - 3, tx, ly + 3, fill=CVS_MEASURE, width=1)
        self.create_text((cam_x + self.CHAR_X) / 2, ly + 11,
                         text=f"Distance: {self._dist:.1f}",
                         fill=CVS_LABEL, font=("Segoe UI", 8), anchor="n")

        # title label
        self.create_text(self.W // 2, 10, text=self._label,
                         fill=TEXT_PRIMARY, font=("Segoe UI", 11, "bold"),
                         anchor="n")

        # height label next to camera
        zone = self._height_zone(self._up)
        self.create_text(cam_x - 12, cam_y,
                         text=f"{zone}  ({self._up:+.1f})",
                         fill=CVS_LABEL, font=("Segoe UI", 8),
                         anchor="e")

    def _draw_ground(self):
        y = self.GROUND_Y
        self.create_line(10, y, self.W - 10, y, fill=CVS_GROUND, width=1)
        for gx in range(18, self.W - 10, 14):
            self.create_line(gx, y + 1, gx - 5, y + 6,
                             fill=CVS_GROUND, width=1)

    def _draw_character(self):
        x, gy = self.CHAR_X, self.GROUND_Y
        r = self.HEAD_R
        hc = self._head_cy
        sh = self._shoulder_y
        hp = self._hip_y
        kn = self._knee_y
        self.create_oval(x - r, hc - r, x + r, hc + r,
                         fill=CVS_CHAR, outline="")
        nk = hc + r
        self.create_line(x, nk, x, hp, fill=CVS_CHAR, width=3)
        self.create_line(x - 20, sh + 18, x, sh, fill=CVS_CHAR, width=2)
        self.create_line(x, sh, x + 20, sh + 18, fill=CVS_CHAR, width=2)
        self.create_line(x, hp, x - 16, gy - 2, fill=CVS_CHAR, width=2)
        self.create_line(x, hp, x + 16, gy - 2, fill=CVS_CHAR, width=2)

    def _draw_camera(self, cx, cy):
        bw, bh = 20, 14
        self.create_rectangle(cx - bw // 2, cy - bh // 2,
                              cx + bw // 2, cy + bh // 2,
                              fill=CVS_CAM, outline="#ccc", width=1)
        self.create_polygon(cx + bw // 2, cy - 5, cx + bw // 2 + 7, cy - 7,
                            cx + bw // 2 + 7, cy + 7, cx + bw // 2, cy + 5,
                            fill="#999", outline="#bbb", width=1)
        self.create_rectangle(cx - 4, cy - bh // 2 - 6, cx + 4, cy - bh // 2,
                              fill="#888", outline="#aaa", width=1)


# ---------------------------------------------------------------------------
# Top-down FoV preview — larger, more breathing room
# ---------------------------------------------------------------------------
class FovPreview(tk.Canvas):

    W = 380
    H = 210
    CAM_Y = 185
    CHAR_Y = 158
    CONE_LEN = 145

    def __init__(self, master, **kw):
        super().__init__(master, width=self.W, height=self.H,
                         bg=CVS_BG, highlightthickness=0, **kw)
        self._fov = 25
        self._roff = 0.0
        self._centered = False
        self.redraw()

    def update_params(self, fov_delta, roff, centered):
        self._fov, self._roff, self._centered = fov_delta, roff, centered
        self.redraw()

    def redraw(self):
        self.delete("all")
        total = 40 + self._fov
        half = math.radians(total / 2)
        cx = self.W // 2
        off = 0 if self._centered else int(self._roff * 35)
        cam_x, cam_y = cx - off, self.CAM_Y
        char_y = self.CHAR_Y

        lx = cam_x - self.CONE_LEN * math.tan(half)
        rx = cam_x + self.CONE_LEN * math.tan(half)
        ty = cam_y - self.CONE_LEN

        self.create_polygon(cam_x, cam_y, lx, ty, rx, ty,
                            fill=CVS_CONE, outline="", stipple="gray25")
        self.create_line(cam_x, cam_y, lx, ty,
                         fill=CVS_CONE_LINE, width=1, dash=(4, 4))
        self.create_line(cam_x, cam_y, rx, ty,
                         fill=CVS_CONE_LINE, width=1, dash=(4, 4))

        # character dot
        self.create_oval(cx - 7, char_y - 7, cx + 7, char_y + 7,
                         fill=CVS_CHAR, outline="")
        self.create_text(cx, char_y + 14, text="player",
                         fill=CVS_LABEL, font=("Segoe UI", 8), anchor="n")

        # camera icon
        self.create_rectangle(cam_x - 6, cam_y - 4, cam_x + 6, cam_y + 4,
                              fill=CVS_CAM, outline="#aaa", width=1)
        self.create_text(cam_x, cam_y + 10, text="cam",
                         fill=CVS_LABEL, font=("Segoe UI", 8), anchor="n")

        # FoV label
        self.create_text(cam_x, cam_y - 35, text=f"{total}\u00b0 FoV",
                         fill=CVS_CONE_LINE,
                         font=("Consolas", 12, "bold"))

        # title
        self.create_text(self.W // 2, 10,
                         text="FIELD OF VIEW  (top-down)",
                         fill=TEXT_DIM, font=("Segoe UI", 9), anchor="n")

        # forward arrow
        self.create_line(cx, ty + 6, cx, ty - 6, fill=CVS_GROUND, width=1,
                         arrow="last", arrowshape=(6, 6, 3))
        self.create_text(cx, ty - 10, text="forward",
                         fill=CVS_GROUND, font=("Segoe UI", 8), anchor="s")


# ---------------------------------------------------------------------------
# Game detection
# ---------------------------------------------------------------------------
def _find_game_dir():
    exe_dir = os.path.dirname(os.path.abspath(sys.argv[0]))
    parent = os.path.dirname(exe_dir)
    if os.path.isfile(os.path.join(parent, "0010", "0.paz")):
        return parent
    try:
        key = winreg.OpenKey(winreg.HKEY_CURRENT_USER, r"Software\Valve\Steam")
        steam_path = winreg.QueryValueEx(key, "SteamPath")[0]
        winreg.CloseKey(key)
        steam_path = steam_path.replace("/", "\\")
        candidate = os.path.join(steam_path, "steamapps", "common",
                                 "Crimson Desert")
        if os.path.isfile(os.path.join(candidate, "0010", "0.paz")):
            return candidate
        vdf = os.path.join(steam_path, "steamapps", "libraryfolders.vdf")
        if os.path.isfile(vdf):
            with open(vdf, "r", errors="replace") as f:
                for line in f:
                    if '"path"' in line:
                        parts = line.split('"')
                        if len(parts) >= 4:
                            lpath = parts[3].replace("\\\\", "\\")
                            c = os.path.join(lpath, "steamapps", "common",
                                             "Crimson Desert")
                            if os.path.isfile(os.path.join(c, "0010", "0.paz")):
                                return c
    except (OSError, FileNotFoundError):
        pass
    for drive in "CDEFGHIJKLMNOPQRSTUVWXYZ":
        for tmpl in (
            r"{D}:\SteamLibrary\steamapps\common\Crimson Desert",
            r"{D}:\Steam\steamapps\common\Crimson Desert",
            r"{D}:\Program Files (x86)\Steam\steamapps\common\Crimson Desert",
            r"{D}:\Program Files\Steam\steamapps\common\Crimson Desert",
            r"{D}:\Games\Crimson Desert",
            r"{D}:\XboxGames\Crimson Desert\Content",
        ):
            p = tmpl.format(D=drive)
            if os.path.isfile(os.path.join(p, "0010", "0.paz")):
                return p
    return None


def _get_backups_dir():
    return os.path.join(_exe_dir(), "backups")


def _state_path():
    return os.path.join(_exe_dir(), "last_install.json")


def _save_install_state(comp_size, style, fov, bane, combat,
                        custom_params=None, mount_height=False, hud_width=0):
    state = {"comp_size": comp_size, "style": style, "fov": fov,
             "bane": bane, "combat": combat, "mount_height": mount_height,
             "hud_width": hud_width}
    if custom_params:
        state["custom"] = custom_params
    try:
        with open(_state_path(), "w") as f:
            json.dump(state, f)
    except OSError:
        pass


def _load_install_state():
    try:
        with open(_state_path(), "r") as f:
            return json.load(f)
    except (OSError, json.JSONDecodeError, KeyError):
        return None


def _get_current_comp_size(game_dir):
    try:
        pamt_path = os.path.join(game_dir, "0010", "0.pamt")
        paz_dir = os.path.join(game_dir, "0010")
        entries = parse_pamt(pamt_path, paz_dir=paz_dir)
        for e in entries:
            if "playercamerapreset.xml" in e.path:
                return e.comp_size
    except Exception:
        pass
    return None


# ---------------------------------------------------------------------------
# Main application
# ---------------------------------------------------------------------------
class UltraWideDesertApp(ctk.CTk):

    def __init__(self, game_dir, update_detected=False, saved_state=None):
        super().__init__()
        self.game_dir = game_dir
        self._update_detected = update_detected
        self._saved = saved_state or {}
        self._active_tab = "presets"
        camera_mod._backups_dir = _get_backups_dir

        self.bane_var = ctk.BooleanVar(value=self._saved.get("bane", False))
        self.mount_height_var = ctk.BooleanVar(
            value=self._saved.get("mount_height", False))
        saved_hw = self._saved.get("hud_width", 0)
        if saved_hw == 0 and self._saved.get("hud") == "21x9":
            saved_hw = HUD_WIDTH_DEFAULT
        self.hud_enabled_var = ctk.BooleanVar(value=saved_hw > 0)
        self.hud_width_var = ctk.IntVar(
            value=saved_hw if saved_hw > 0 else HUD_WIDTH_DEFAULT)

        self.title(f"Ultimate Camera Mod v{VER}")
        self.configure(fg_color=BG_DARK)
        self.geometry("1020x880")
        self.resizable(False, False)
        self._apply_dark_titlebar()
        self._build_ui()

    def _apply_dark_titlebar(self):
        try:
            hwnd = ctypes.windll.user32.GetParent(self.winfo_id())
            ctypes.windll.dwmapi.DwmSetWindowAttribute(
                hwnd, 20, ctypes.byref(ctypes.c_int(2)), 4)
        except Exception:
            pass

    # ── UI build ──────────────────────────────────────────────────

    def _build_ui(self):
        pad = 20

        # ── Title bar ──
        title_frame = ctk.CTkFrame(self, fg_color=BG_PANEL, corner_radius=0,
                                    height=50)
        title_frame.pack(fill="x")
        title_frame.pack_propagate(False)

        ctk.CTkLabel(
            title_frame, text="ULTIMATE CAMERA MOD",
            font=ctk.CTkFont(family="Segoe UI", size=15, weight="bold"),
            text_color=ACCENT,
        ).pack(side="left", padx=pad, pady=10)
        ctk.CTkLabel(
            title_frame, text=f"v{VER}  |  by @TheFitzy",
            font=ctk.CTkFont(size=10), text_color=TEXT_DIM,
        ).pack(side="left", pady=10)

        ctk.CTkButton(
            title_frame, text="\u2691  GitHub", width=80, height=26,
            font=ctk.CTkFont(size=10), fg_color="transparent",
            hover_color=BG_INPUT, text_color=TEXT_SECONDARY,
            corner_radius=4, command=lambda: webbrowser.open(GITHUB_URL),
        ).pack(side="right", padx=(0, pad), pady=12)
        ctk.CTkButton(
            title_frame, text="\u2B22  NexusMods", width=100, height=26,
            font=ctk.CTkFont(size=10), fg_color="transparent",
            hover_color=BG_INPUT, text_color="#d98f40",
            corner_radius=4, command=lambda: webbrowser.open(NEXUS_URL),
        ).pack(side="right", pady=12)

        # ── Alert banner ──
        self._banner = None
        if self._update_detected:
            self._banner = ctk.CTkFrame(self, fg_color="#2d1f00",
                                         corner_radius=0, height=32)
            self._banner.pack(fill="x")
            self._banner.pack_propagate(False)
            ctk.CTkLabel(
                self._banner,
                text="\u26A0  Game update detected  \u2014  "
                     "hit Install to re-apply your settings",
                font=ctk.CTkFont(size=11), text_color=WARN,
            ).pack(side="left", padx=pad, pady=4)
        elif self._saved:
            bar = ctk.CTkFrame(self, fg_color="#0d1f0d", corner_radius=0,
                                height=32)
            bar.pack(fill="x")
            bar.pack_propagate(False)
            sn = self._saved.get("style", "?")
            for sid, lbl in STYLES:
                if sid == sn:
                    sn = lbl.split("  -  ")[0]; break
            if sn == "custom":
                sn = "Custom"
            ctk.CTkLabel(
                bar,
                text=f"\u2714  Mod active:  {sn}  |  "
                     f"FoV +{self._saved.get('fov', 0)}\u00b0  |  "
                     f"{'Centered' if self._saved.get('bane') else 'Shifted'}"
                     f"{'  |  HUD ' + str(self._saved.get('hud_width', 0)) + 'px' if self._saved.get('hud_width', 0) > 0 else ''}",
                font=ctk.CTkFont(size=11), text_color=SUCCESS,
            ).pack(side="left", padx=pad, pady=4)

        ctk.CTkFrame(self, fg_color=BORDER, height=1,
                      corner_radius=0).pack(fill="x")

        # ── Body: 2-column layout ──
        body = ctk.CTkFrame(self, fg_color=BG_DARK, corner_radius=0)
        body.pack(fill="both", expand=True, padx=pad, pady=(pad, 10))
        body.grid_columnconfigure(0, weight=1, minsize=520)
        body.grid_columnconfigure(1, weight=0)
        body.grid_rowconfigure(0, weight=1)

        # ══════════════════════════════════════════════════════════════
        # LEFT COLUMN — all controls
        # ══════════════════════════════════════════════════════════════
        left = ctk.CTkFrame(body, fg_color=BG_DARK, corner_radius=0)
        left.grid(row=0, column=0, sticky="nsew", padx=(0, pad))

        # ── Game path (subtle) ──
        pt = self.game_dir
        if len(pt) > 55:
            pt = "..." + pt[-52:]
        ctk.CTkLabel(
            left, text=f"Game folder:  {pt}",
            font=ctk.CTkFont(family="Consolas", size=9),
            text_color=TEXT_DIM, anchor="w",
        ).pack(fill="x", pady=(0, 12))

        # ── SECTION: Camera Style ──
        self._section_header(left, "CAMERA STYLE")

        # tab buttons
        tab_bar = ctk.CTkFrame(left, fg_color=BG_PANEL, corner_radius=6)
        tab_bar.pack(fill="x", pady=(6, 0))
        self._tab_btn_presets = ctk.CTkButton(
            tab_bar, text="Presets", width=140, height=32,
            font=ctk.CTkFont(size=12, weight="bold"),
            fg_color=ACCENT, hover_color=ACCENT_HOVER,
            text_color=BG_DARK, corner_radius=6,
            command=lambda: self._switch_tab("presets"))
        self._tab_btn_presets.pack(side="left", padx=4, pady=4)
        self._tab_btn_custom = ctk.CTkButton(
            tab_bar, text="Custom", width=140, height=32,
            font=ctk.CTkFont(size=12, weight="bold"),
            fg_color="transparent", hover_color=BORDER,
            text_color=TEXT_DIM, corner_radius=6,
            command=lambda: self._switch_tab("custom"))
        self._tab_btn_custom.pack(side="left", padx=(0, 4), pady=4)

        # tab content
        self._tab_container = ctk.CTkFrame(left, fg_color=BG_PANEL,
                                            corner_radius=6)
        self._tab_container.pack(fill="x", pady=(4, 0))
        self._tab_inner = ctk.CTkFrame(self._tab_container,
                                        fg_color="transparent")
        self._tab_inner.pack(fill="x", padx=16, pady=14)

        self._build_presets_tab()
        self._build_custom_tab()

        # ── SECTION: Global Settings ──
        self._section_header(left, "GLOBAL SETTINGS")
        gs = ctk.CTkFrame(left, fg_color=BG_PANEL, corner_radius=6)
        gs.pack(fill="x", pady=(6, 0))
        gs_inner = ctk.CTkFrame(gs, fg_color="transparent")
        gs_inner.pack(fill="x", padx=16, pady=14)
        gs_inner.grid_columnconfigure(1, weight=1)

        r = 0
        self._lbl(gs_inner, "Field of view", r)
        fi = 4
        sf = self._saved.get("fov", 25)
        if sf in FOV_VALUES:
            fi = FOV_VALUES.index(sf)
        self.fov_combo = self._combo(gs_inner, FOV_LABELS, fi, r,
                                      self._on_setting_changed)
        r += 1

        self._lbl(gs_inner, "Combat camera", r)
        ci = 0
        sc = self._saved.get("combat", "default")
        if sc in COMBAT_IDS:
            ci = COMBAT_IDS.index(sc)
        self.combat_combo = self._combo(gs_inner, COMBAT_LABELS, ci, r,
                                         self._on_setting_changed)
        r += 1

        self._lbl(gs_inner, "Centered camera", r)
        ctk.CTkCheckBox(
            gs_inner,
            text="Center character on screen (locks Horizontal Shift to 0)",
            variable=self.bane_var, font=ctk.CTkFont(size=11),
            text_color=TEXT_SECONDARY, fg_color=ACCENT,
            hover_color=ACCENT_HOVER, border_color=BORDER,
            checkmark_color=BG_DARK, command=self._on_setting_changed,
        ).grid(row=r, column=1, sticky="w", pady=(0, 2))
        r += 1

        self._lbl(gs_inner, "Mount camera", r)
        ctk.CTkCheckBox(
            gs_inner,
            text="Match mount/horse height to player camera height",
            variable=self.mount_height_var, font=ctk.CTkFont(size=11),
            text_color=TEXT_SECONDARY, fg_color=ACCENT,
            hover_color=ACCENT_HOVER, border_color=BORDER,
            checkmark_color=BG_DARK, command=self._on_setting_changed,
        ).grid(row=r, column=1, sticky="w", pady=(0, 2))
        r += 1

        self._lbl(gs_inner, "Center HUD", r)
        ctk.CTkCheckBox(
            gs_inner, text="",
            variable=self.hud_enabled_var, fg_color=ACCENT,
            hover_color=ACCENT_HOVER, border_color=BORDER,
            checkmark_color=BG_DARK, width=24,
            command=self._on_hud_toggle,
        ).grid(row=r, column=1, sticky="w", pady=(0, 2))
        r += 1

        self._lbl(gs_inner, "HUD width", r)
        self._hud_val_lbl = ctk.CTkLabel(
            gs_inner, text=f"{self.hud_width_var.get()}px",
            font=ctk.CTkFont(family="Consolas", size=11),
            text_color=TEXT_PRIMARY, width=50)
        self._hud_val_lbl.grid(row=r, column=2, padx=(6, 0), pady=2)

        def _hud_slide(v):
            w = int(round(float(v)))
            w = max(HUD_WIDTH_MIN, min(HUD_WIDTH_MAX, w))
            self.hud_width_var.set(w)
            self._hud_val_lbl.configure(text=f"{w}px")
            self._on_setting_changed()

        self._hud_slider = ctk.CTkSlider(
            gs_inner, from_=HUD_WIDTH_MIN, to=HUD_WIDTH_MAX,
            variable=self.hud_width_var,
            number_of_steps=(HUD_WIDTH_MAX - HUD_WIDTH_MIN) // 20,
            width=200, height=16, fg_color=BORDER, progress_color=ACCENT,
            button_color=ACCENT, button_hover_color=ACCENT_HOVER,
            command=_hud_slide,
        )
        self._hud_slider.grid(row=r, column=1, sticky="ew", padx=0, pady=2)
        self._apply_hud_lock()

        # ══════════════════════════════════════════════════════════════
        # RIGHT COLUMN — camera previews
        # ══════════════════════════════════════════════════════════════
        right = ctk.CTkFrame(body, fg_color=BG_DARK, corner_radius=0)
        right.grid(row=0, column=1, sticky="nsew")

        self._section_header(right, "CAMERA PREVIEW")

        b1 = ctk.CTkFrame(right, fg_color=BG_PANEL, corner_radius=6)
        b1.pack(anchor="n", fill="x", pady=(6, 8))
        self.preview = CameraPreview(b1)
        self.preview.pack(padx=6, pady=6)

        b2 = ctk.CTkFrame(right, fg_color=BG_PANEL, corner_radius=6)
        b2.pack(anchor="n", fill="x")
        self.fov_preview = FovPreview(b2)
        self.fov_preview.pack(padx=6, pady=6)

        # ── Bottom bar ──
        ctk.CTkFrame(self, fg_color=BORDER, height=1,
                      corner_radius=0).pack(fill="x")
        bottom = ctk.CTkFrame(self, fg_color=BG_PANEL, corner_radius=0,
                               height=56)
        bottom.pack(fill="x")
        bottom.pack_propagate(False)
        self.status_label = ctk.CTkLabel(
            bottom, text="Ready", font=ctk.CTkFont(size=11),
            text_color=TEXT_DIM, anchor="w")
        self.status_label.pack(side="left", padx=pad, fill="x", expand=True)
        self.restore_btn = ctk.CTkButton(
            bottom, text="Restore Vanilla", width=130, height=34,
            font=ctk.CTkFont(size=12), fg_color="transparent",
            hover_color=BG_INPUT, text_color=TEXT_SECONDARY, corner_radius=6,
            border_width=1, border_color=BORDER, command=self._on_restore)
        self.restore_btn.pack(side="right", padx=(4, pad), pady=11)
        self.install_btn = ctk.CTkButton(
            bottom, text="Install", width=110, height=34,
            font=ctk.CTkFont(size=13, weight="bold"), fg_color=ACCENT,
            hover_color=ACCENT_HOVER, text_color=BG_DARK, corner_radius=6,
            command=self._on_install)
        self.install_btn.pack(side="right", pady=11)

        self._switch_tab(
            "custom" if self._saved.get("style") == "custom" else "presets")
        self._sync_preview()

    # ── Tab frames ────────────────────────────────────────────────

    def _build_presets_tab(self):
        self._presets_frame = ctk.CTkFrame(self._tab_inner,
                                            fg_color="transparent")
        self._presets_frame.grid_columnconfigure(1, weight=1)
        r = 0
        self._lbl(self._presets_frame, "Camera style", r)
        di = 0
        ss = self._saved.get("style", "cinematic")
        if ss in STYLE_IDS:
            di = STYLE_IDS.index(ss)
        self.style_combo = self._combo(
            self._presets_frame, STYLE_LABELS, di, r,
            self._on_setting_changed)

    def _build_custom_tab(self):
        self._custom_frame = ctk.CTkFrame(self._tab_inner,
                                           fg_color="transparent")
        self._custom_frame.grid_columnconfigure(1, weight=1)

        saved_custom = self._saved.get("custom", {})
        d_min, d_max = CUSTOM_LIMITS["distance"]
        h_min, h_max = CUSTOM_LIMITS["height"]
        r_min, r_max = CUSTOM_LIMITS["right_offset"]

        r = 0
        # saved presets dropdown
        self._lbl(self._custom_frame, "Saved preset", r)
        self._refresh_preset_list()
        self.preset_combo = ctk.CTkOptionMenu(
            self._custom_frame, values=self._preset_values, height=32,
            fg_color=BG_INPUT, button_color=BORDER,
            button_hover_color=TEXT_DIM,
            dropdown_fg_color=BG_INPUT, dropdown_hover_color=ACCENT,
            dropdown_text_color=TEXT_PRIMARY, text_color=TEXT_PRIMARY,
            font=ctk.CTkFont(size=12),
            dropdown_font=ctk.CTkFont(size=12),
            corner_radius=6, command=self._on_preset_selected)
        self.preset_combo.set(_PLACEHOLDER_PRESET)
        self.preset_combo.grid(row=r, column=1, sticky="ew", pady=(0, 8))
        r += 1

        ctk.CTkFrame(self._custom_frame, fg_color=BORDER, height=1).grid(
            row=r, column=0, columnspan=3, sticky="ew", pady=(0, 8))
        r += 1

        # sliders
        self.dist_var = ctk.DoubleVar(value=saved_custom.get("distance", 5.0))
        self._slider(self._custom_frame, r, "Distance", self.dist_var,
                     d_min, d_max)
        r += 1
        self.height_var = ctk.DoubleVar(value=saved_custom.get("height", 0.0))
        self._slider(self._custom_frame, r, "Height", self.height_var,
                     h_min, h_max)
        r += 1
        self.roff_var = ctk.DoubleVar(
            value=saved_custom.get("right_offset", 0.0))
        self._build_hshift_slider(self._custom_frame, r, self.roff_var,
                                   r_min, r_max)
        r += 2

        # buttons row 1: save / delete
        b1 = ctk.CTkFrame(self._custom_frame, fg_color="transparent")
        b1.grid(row=r, column=0, columnspan=3, sticky="ew", pady=(8, 0))
        ctk.CTkButton(
            b1, text="Save preset", width=100, height=28,
            font=ctk.CTkFont(size=11), fg_color=BG_INPUT,
            hover_color=BORDER, text_color=TEXT_SECONDARY,
            corner_radius=4, border_width=1, border_color=BORDER,
            command=self._on_save_preset,
        ).pack(side="left", padx=(0, 4))
        ctk.CTkButton(
            b1, text="Delete preset", width=100, height=28,
            font=ctk.CTkFont(size=11), fg_color=BG_INPUT,
            hover_color="#3a2020", text_color="#aa6666",
            corner_radius=4, border_width=1, border_color=BORDER,
            command=self._on_delete_preset,
        ).pack(side="left")
        r += 1

        # buttons row 2: import / export
        b2 = ctk.CTkFrame(self._custom_frame, fg_color="transparent")
        b2.grid(row=r, column=0, columnspan=3, sticky="ew", pady=(6, 0))
        ctk.CTkLabel(b2, text="SHARE", font=ctk.CTkFont(size=9, weight="bold"),
                     text_color=TEXT_DIM).pack(side="left", padx=(0, 8))
        ctk.CTkButton(
            b2, text="Export string", width=100, height=28,
            font=ctk.CTkFont(size=11), fg_color=BG_INPUT,
            hover_color=BORDER, text_color=ACCENT,
            corner_radius=4, border_width=1, border_color=ACCENT,
            command=self._on_export_string,
        ).pack(side="left", padx=(0, 4))
        ctk.CTkButton(
            b2, text="Import string", width=100, height=28,
            font=ctk.CTkFont(size=11), fg_color=BG_INPUT,
            hover_color=BORDER, text_color=ACCENT,
            corner_radius=4, border_width=1, border_color=ACCENT,
            command=self._on_import_string,
        ).pack(side="left")
        r += 1

        ctk.CTkLabel(
            self._custom_frame,
            text=f"Limits: Dist {d_min}-{d_max}  |  "
                 f"Height {h_min} to {h_max}  |  "
                 f"Shift {r_min} to {r_max}",
            font=ctk.CTkFont(size=9), text_color=TEXT_DIM,
        ).grid(row=r, column=0, columnspan=3, sticky="w", pady=(6, 0))

    def _switch_tab(self, tab):
        self._active_tab = tab
        self._presets_frame.pack_forget()
        self._custom_frame.pack_forget()
        if tab == "presets":
            self._presets_frame.pack(fill="x")
            self._tab_btn_presets.configure(fg_color=ACCENT,
                                             text_color=BG_DARK)
            self._tab_btn_custom.configure(fg_color="transparent",
                                            text_color=TEXT_DIM)
        else:
            self._custom_frame.pack(fill="x")
            self._tab_btn_custom.configure(fg_color=ACCENT,
                                            text_color=BG_DARK)
            self._tab_btn_presets.configure(fg_color="transparent",
                                             text_color=TEXT_DIM)
        self._sync_preview()

    # ── Widget helpers ────────────────────────────────────────────

    def _lbl(self, parent, text, row):
        ctk.CTkLabel(parent, text=text, font=ctk.CTkFont(size=11),
                     text_color=TEXT_SECONDARY, anchor="e"
                     ).grid(row=row, column=0, sticky="e", padx=(0, 12),
                            pady=(0, 8))

    @staticmethod
    def _section_header(parent, text):
        ctk.CTkLabel(parent, text=text,
                     font=ctk.CTkFont(size=10, weight="bold"),
                     text_color=TEXT_DIM, anchor="w"
                     ).pack(fill="x", pady=(14, 0))

    def _combo(self, parent, values, idx, row, cmd):
        om = ctk.CTkOptionMenu(
            parent, values=values, height=32,
            fg_color=BG_INPUT, button_color=BORDER,
            button_hover_color=TEXT_DIM,
            dropdown_fg_color=BG_INPUT, dropdown_hover_color=ACCENT,
            dropdown_text_color=TEXT_PRIMARY, text_color=TEXT_PRIMARY,
            font=ctk.CTkFont(size=12),
            dropdown_font=ctk.CTkFont(size=12),
            corner_radius=6, command=cmd)
        om.set(values[idx])
        om.grid(row=row, column=1, sticky="ew", pady=(0, 8))
        return om

    def _slider(self, parent, row, label, var, vmin, vmax):
        ctk.CTkLabel(parent, text=label, font=ctk.CTkFont(size=11),
                     text_color=TEXT_SECONDARY, anchor="e"
                     ).grid(row=row, column=0, sticky="e", padx=(0, 8), pady=2)
        vl = ctk.CTkLabel(parent, text=f"{var.get():.1f}",
                           font=ctk.CTkFont(family="Consolas", size=11),
                           text_color=TEXT_PRIMARY, width=40)
        vl.grid(row=row, column=2, padx=(6, 0), pady=2)

        def _slide(v):
            c = max(vmin, min(vmax, float(v)))
            var.set(round(c, 2))
            vl.configure(text=f"{c:.1f}")
            self._sync_preview()

        ctk.CTkSlider(
            parent, from_=vmin, to=vmax, variable=var,
            number_of_steps=int((vmax - vmin) / 0.1),
            width=200, height=16, fg_color=BORDER, progress_color=ACCENT,
            button_color=ACCENT, button_hover_color=ACCENT_HOVER,
            command=_slide,
        ).grid(row=row, column=1, sticky="ew", padx=0, pady=2)

    def _build_hshift_slider(self, parent, row, var, vmin, vmax):
        """Build the Horizontal Shift slider with centered-lock awareness."""
        ctk.CTkLabel(parent, text="Horizontal shift",
                     font=ctk.CTkFont(size=11),
                     text_color=TEXT_SECONDARY, anchor="e"
                     ).grid(row=row, column=0, sticky="e", padx=(0, 8), pady=2)
        self._hshift_val_lbl = ctk.CTkLabel(
            parent, text=f"{var.get():.1f}",
            font=ctk.CTkFont(family="Consolas", size=11),
            text_color=TEXT_PRIMARY, width=40)
        self._hshift_val_lbl.grid(row=row, column=2, padx=(6, 0), pady=2)

        def _slide(v):
            c = max(vmin, min(vmax, float(v)))
            var.set(round(c, 2))
            self._hshift_val_lbl.configure(text=f"{c:.1f}")
            self._sync_preview()

        self._hshift_slider = ctk.CTkSlider(
            parent, from_=vmin, to=vmax, variable=var,
            number_of_steps=int((vmax - vmin) / 0.1),
            width=200, height=16, fg_color=BORDER, progress_color=ACCENT,
            button_color=ACCENT, button_hover_color=ACCENT_HOVER,
            command=_slide,
        )
        self._hshift_slider.grid(row=row, column=1, sticky="ew",
                                  padx=0, pady=2)

        # tooltip row
        self._hshift_tip = ctk.CTkLabel(
            parent,
            text="How far left/right the camera sits from the character. "
                 "Centered camera locks this to 0.",
            font=ctk.CTkFont(size=9), text_color=TEXT_DIM, anchor="w")
        self._hshift_tip.grid(row=row + 1, column=0, columnspan=3,
                               sticky="w", padx=(0, 0), pady=(0, 4))

        self._apply_centered_lock()

    def _apply_centered_lock(self):
        """Lock / unlock horizontal shift slider based on Centered Camera."""
        if not hasattr(self, "_hshift_slider"):
            return
        if self.bane_var.get():
            self.roff_var.set(0.0)
            self._hshift_slider.set(0.0)
            self._hshift_slider.configure(state="disabled",
                                           button_color=TEXT_DIM,
                                           progress_color=BORDER)
            self._hshift_val_lbl.configure(text="0.0",
                                            text_color=TEXT_DIM)
            self._hshift_tip.configure(
                text="\u26A0 Locked to 0 — untick Centered Camera to adjust")
        else:
            self._hshift_slider.configure(state="normal",
                                           button_color=ACCENT,
                                           progress_color=ACCENT)
            self._hshift_val_lbl.configure(
                text=f"{self.roff_var.get():.1f}",
                text_color=TEXT_PRIMARY)
            self._hshift_tip.configure(
                text="How far left/right the camera sits from the character. "
                     "Centered camera locks this to 0.")

    def _on_hud_toggle(self):
        self._apply_hud_lock()
        self._on_setting_changed()

    def _apply_hud_lock(self):
        if not hasattr(self, "_hud_slider"):
            return
        if self.hud_enabled_var.get():
            self._hud_slider.configure(state="normal",
                                        button_color=ACCENT,
                                        progress_color=ACCENT)
            self._hud_val_lbl.configure(
                text=f"{self.hud_width_var.get()}px",
                text_color=TEXT_PRIMARY)
        else:
            self._hud_slider.configure(state="disabled",
                                        button_color=TEXT_DIM,
                                        progress_color=BORDER)
            self._hud_val_lbl.configure(text="Off",
                                         text_color=TEXT_DIM)

    # ── Custom preset dropdown ────────────────────────────────────

    def _refresh_preset_list(self):
        presets = _list_custom_presets()
        self._preset_values = [_PLACEHOLDER_PRESET] + presets

    def _refresh_preset_combo(self):
        self._refresh_preset_list()
        self.preset_combo.configure(values=self._preset_values)

    def _on_preset_selected(self, value):
        if value == _PLACEHOLDER_PRESET:
            return
        try:
            data = _load_custom_preset(value)
            d_min, d_max = CUSTOM_LIMITS["distance"]
            h_min, h_max = CUSTOM_LIMITS["height"]
            r_min, r_max = CUSTOM_LIMITS["right_offset"]
            self.dist_var.set(max(d_min, min(d_max,
                                              data.get("distance", 5.0))))
            self.height_var.set(max(h_min, min(h_max,
                                                data.get("height", 0.0))))
            self.roff_var.set(max(r_min, min(r_max,
                                              data.get("right_offset", 0.0))))
            self._rebuild_custom_sliders()
            self._sync_preview()
            self._set_status(f"Preset '{value}' loaded.", SUCCESS)
        except Exception as e:
            self._set_status(f"Load failed: {e}", ERROR)

    def _rebuild_custom_sliders(self):
        """Rebuild the custom tab to reflect current var values."""
        saved = {"distance": self.dist_var.get(),
                 "height": self.height_var.get(),
                 "right_offset": self.roff_var.get()}
        sel = self.preset_combo.get()
        self._custom_frame.destroy()
        self._build_custom_tab()
        if sel in self._preset_values:
            self.preset_combo.set(sel)
        if self._active_tab == "custom":
            self._custom_frame.pack(fill="x")

    # ── Preset save / delete ──────────────────────────────────────

    def _on_save_preset(self):
        dialog = ctk.CTkInputDialog(
            text="Enter a name for this preset:",
            title="Save Custom Preset")
        name = dialog.get_input()
        if not name or not name.strip():
            return
        name = name.strip().replace(" ", "_")[:30]
        try:
            _save_custom_preset(name, self.dist_var.get(),
                                self.height_var.get(), self.roff_var.get())
            self._refresh_preset_combo()
            self.preset_combo.set(name)
            self._set_status(f"Preset '{name}' saved.", SUCCESS)
        except Exception as e:
            self._set_status(f"Save failed: {e}", ERROR)

    def _on_delete_preset(self):
        sel = self.preset_combo.get()
        if sel == _PLACEHOLDER_PRESET:
            self._set_status("Select a saved preset first.", TEXT_SECONDARY)
            return
        try:
            _delete_custom_preset(sel)
            self._refresh_preset_combo()
            self.preset_combo.set(_PLACEHOLDER_PRESET)
            self._set_status(f"Preset '{sel}' deleted.", SUCCESS)
        except Exception as e:
            self._set_status(f"Delete failed: {e}", ERROR)

    # ── Import / export ───────────────────────────────────────────

    def _on_export_string(self):
        dialog = ctk.CTkInputDialog(
            text="Name this preset for sharing:",
            title="Export Preset String")
        name = dialog.get_input()
        if not name or not name.strip():
            return
        name = name.strip()[:30]
        code = _encode_preset_string(
            name, self.dist_var.get(), self.height_var.get(),
            self.roff_var.get())
        _ExportDialog(self, name, code)
        self._set_status(f"Preset '{name}' exported.", SUCCESS)

    def _on_import_string(self):
        dialog = _ImportDialog(self)
        result = dialog.get_result()
        if not result:
            return
        name, dist, height, roff = result
        self.dist_var.set(dist)
        self.height_var.set(height)
        self.roff_var.set(roff)
        safe_name = name.replace(" ", "_")[:30]
        try:
            _save_custom_preset(safe_name, dist, height, roff)
        except Exception:
            pass
        self._rebuild_custom_sliders()
        self.preset_combo.set(safe_name)
        self._sync_preview()
        self._set_status(
            f"Imported '{name}' and saved — hit Install to apply.", SUCCESS)

    # ── Preview sync ──────────────────────────────────────────────

    def _get_fov_val(self):
        lbl = self.fov_combo.get()
        return FOV_VALUES[FOV_LABELS.index(lbl)] \
            if lbl in FOV_LABELS else 25

    def _sync_preview(self):
        fov = self._get_fov_val()
        centered = self.bane_var.get()

        if self._active_tab == "custom":
            d = self.dist_var.get()
            h = self.height_var.get()
            ro = self.roff_var.get()
            self.preview.update_params(d, h, "Custom")
            self.fov_preview.update_params(fov, ro, centered)
        else:
            lbl = self.style_combo.get()
            sid = STYLE_IDS[STYLE_LABELS.index(lbl)] \
                if lbl in STYLE_LABELS else "cinematic"
            d, u, ro = STYLE_PARAMS.get(sid, (3.4, 0.0, 0.0))
            name = sid
            for s, l in STYLES:
                if s == sid:
                    name = l.split("  -  ")[0]; break
            self.preview.update_params(d, u, name)
            self.fov_preview.update_params(fov, ro, centered)

    def _on_setting_changed(self, _v=None):
        self._apply_centered_lock()
        self._sync_preview()

    # ── Helpers ────────────────────────────────────────────────────

    def _set_status(self, text, color=TEXT_DIM):
        self.status_label.configure(text=text, text_color=color)

    def _set_buttons(self, enabled):
        st = "normal" if enabled else "disabled"
        self.install_btn.configure(state=st)
        self.restore_btn.configure(state=st)

    def _get_selections(self):
        if self._active_tab == "custom":
            style_id = "custom"
        else:
            lbl = self.style_combo.get()
            style_id = STYLE_IDS[STYLE_LABELS.index(lbl)] \
                if lbl in STYLE_LABELS else "cinematic"
        fov = self._get_fov_val()
        clbl = self.combat_combo.get()
        combat = COMBAT_IDS[COMBAT_LABELS.index(clbl)] \
            if clbl in COMBAT_LABELS else "default"
        bane = self.bane_var.get()
        mount_height = self.mount_height_var.get()
        hud_width = self.hud_width_var.get() if self.hud_enabled_var.get() else 0
        return style_id, fov, bane, combat, mount_height, hud_width

    # ── Install ───────────────────────────────────────────────────

    def _on_install(self):
        self._set_buttons(False)
        self._set_status("Installing...", ACCENT)
        style_id, fov, bane, combat, mount_height, hud_width = self._get_selections()

        custom_params = None
        custom_up = None
        if style_id == "custom":
            custom_params = {
                "distance": round(self.dist_var.get(), 2),
                "height": round(self.height_var.get(), 2),
                "right_offset": round(self.roff_var.get(), 2),
            }
            custom_up = custom_params["height"]

        def _worker():
            comp_size = None
            try:
                if style_id == "custom" and custom_params:
                    _STYLE_BUILDERS["custom"] = lambda: build_custom(
                        custom_params["distance"],
                        custom_params["height"],
                        custom_params["right_offset"])
                result = install_camera_mod(
                    self.game_dir, style_id, fov, bane, combat,
                    mount_height=mount_height, custom_up=custom_up)
                ok = result.get("status") == "ok"
                comp_size = result.get("comp_size")

                if ok and hud_width > 0:
                    hud_result = install_centered_hud(
                        self.game_dir, max_width=hud_width)
                    if hud_result.get("status") != "ok":
                        ok = False
                elif ok and hud_width == 0:
                    restore_hud(self.game_dir)
            except Exception as e:
                ok = False
                self.after(0, lambda: self._set_status(f"Error: {e}", ERROR))

            def _done():
                if ok:
                    self._set_status("Installed! Launch the game.", SUCCESS)
                    if comp_size:
                        _save_install_state(comp_size, style_id, fov,
                                            bane, combat, custom_params,
                                            mount_height, hud_width)
                    if self._banner:
                        self._banner.pack_forget()
                        self._banner = None
                else:
                    if "Error" not in self.status_label.cget("text"):
                        self._set_status(
                            "Install failed. Is the game running?", ERROR)
                self._set_buttons(True)
            self.after(0, _done)

        threading.Thread(target=_worker, daemon=True).start()

    # ── Restore ───────────────────────────────────────────────────

    def _on_restore(self):
        self._set_buttons(False)
        self._set_status("Restoring vanilla...", ACCENT)

        def _worker():
            try:
                result = restore_camera(self.game_dir)
                status = result.get("status", "error")
                restore_hud(self.game_dir)
            except Exception as e:
                status = "error"
                self.after(0, lambda: self._set_status(f"Error: {e}", ERROR))

            def _done():
                if status == "ok":
                    self._set_status("Vanilla restored (camera + HUD).", SUCCESS)
                elif status == "no_backup":
                    self._set_status(
                        "No backup found. Camera may already be vanilla.",
                        TEXT_SECONDARY)
                elif status == "stale_backup":
                    self._set_status(
                        "Backup is from old version. Verify game files.",
                        ERROR)
                else:
                    if "Error" not in self.status_label.cget("text"):
                        self._set_status(
                            "Restore failed. Verify game files.", ERROR)
                self._set_buttons(True)
            self.after(0, _done)

        threading.Thread(target=_worker, daemon=True).start()


# ---------------------------------------------------------------------------
# Dialogs
# ---------------------------------------------------------------------------
class _PresetPickerDialog(ctk.CTkToplevel):

    def __init__(self, parent, presets, title):
        super().__init__(parent)
        self.title(title)
        self.geometry("300x350")
        self.resizable(False, False)
        self.configure(fg_color=BG_DARK)
        self._result = None
        ctk.CTkLabel(self, text="Select a preset:",
                     font=ctk.CTkFont(size=12),
                     text_color=TEXT_PRIMARY).pack(padx=16, pady=(16, 8))
        self._lb = tk.Listbox(
            self, bg=BG_INPUT, fg=TEXT_PRIMARY,
            selectbackground=ACCENT, selectforeground=BG_DARK,
            font=("Segoe UI", 11), borderwidth=0, highlightthickness=0)
        self._lb.pack(fill="both", expand=True, padx=16, pady=(0, 8))
        for p in presets:
            self._lb.insert(tk.END, p)
        if presets:
            self._lb.selection_set(0)
        bf = ctk.CTkFrame(self, fg_color="transparent")
        bf.pack(fill="x", padx=16, pady=(0, 16))
        ctk.CTkButton(bf, text="OK", width=80, fg_color=ACCENT,
                       hover_color=ACCENT_HOVER, text_color=BG_DARK,
                       command=self._ok).pack(side="right", padx=(4, 0))
        ctk.CTkButton(bf, text="Cancel", width=80, fg_color=BG_INPUT,
                       hover_color=BORDER, text_color=TEXT_SECONDARY,
                       command=self._cancel).pack(side="right")
        self.grab_set()
        self.wait_window()

    def _ok(self):
        s = self._lb.curselection()
        if s:
            self._result = self._lb.get(s[0])
        self.destroy()

    def _cancel(self):
        self.destroy()

    def get_result(self):
        return self._result


class _ExportDialog(ctk.CTkToplevel):

    def __init__(self, parent, name, code):
        super().__init__(parent)
        self.title("Export Preset String")
        self.geometry("520x260")
        self.resizable(False, False)
        self.configure(fg_color=BG_DARK)
        ctk.CTkLabel(self, text=f"Share this string for \"{name}\":",
                     font=ctk.CTkFont(size=12),
                     text_color=TEXT_PRIMARY).pack(padx=20, pady=(20, 8))
        ctk.CTkLabel(self, text="Copy and paste on NexusMods, Discord, etc.",
                     font=ctk.CTkFont(size=10),
                     text_color=TEXT_DIM).pack(padx=20, pady=(0, 8))
        self._text = tk.Text(
            self, height=4, wrap="char", bg=BG_INPUT, fg=ACCENT,
            insertbackground=ACCENT, font=("Consolas", 11),
            borderwidth=0, highlightthickness=1,
            highlightcolor=ACCENT, highlightbackground=BORDER)
        self._text.pack(fill="x", padx=20, pady=(0, 12))
        self._text.insert("1.0", code)
        self._text.tag_add("sel", "1.0", "end")
        bf = ctk.CTkFrame(self, fg_color="transparent")
        bf.pack(fill="x", padx=20, pady=(0, 20))
        self._cb = ctk.CTkButton(
            bf, text="Copy to clipboard", width=140, height=32,
            font=ctk.CTkFont(size=12, weight="bold"),
            fg_color=ACCENT, hover_color=ACCENT_HOVER,
            text_color=BG_DARK, corner_radius=6,
            command=lambda: self._copy(code))
        self._cb.pack(side="left")
        ctk.CTkButton(bf, text="Close", width=80, height=32,
                       fg_color=BG_INPUT, hover_color=BORDER,
                       text_color=TEXT_SECONDARY, corner_radius=6,
                       command=self.destroy).pack(side="right")
        self.grab_set()

    def _copy(self, code):
        self.clipboard_clear()
        self.clipboard_append(code)
        self._cb.configure(text="Copied!")
        self.after(1500, lambda: self._cb.configure(text="Copy to clipboard"))


class _ImportDialog(ctk.CTkToplevel):

    def __init__(self, parent):
        super().__init__(parent)
        self.title("Import Preset String")
        self.geometry("520x310")
        self.resizable(False, False)
        self.configure(fg_color=BG_DARK)
        self._result = None
        ctk.CTkLabel(self, text="Paste a preset string below:",
                     font=ctk.CTkFont(size=12),
                     text_color=TEXT_PRIMARY).pack(padx=20, pady=(20, 4))
        ctk.CTkLabel(self, text="From NexusMods comments, Discord, etc.",
                     font=ctk.CTkFont(size=10),
                     text_color=TEXT_DIM).pack(padx=20, pady=(0, 8))
        self._text = tk.Text(
            self, height=4, wrap="char", bg=BG_INPUT, fg=TEXT_PRIMARY,
            insertbackground=ACCENT, font=("Consolas", 11),
            borderwidth=0, highlightthickness=1,
            highlightcolor=ACCENT, highlightbackground=BORDER)
        self._text.pack(fill="x", padx=20, pady=(0, 8))
        self._pv = ctk.CTkLabel(self, text="",
                                 font=ctk.CTkFont(size=11),
                                 text_color=TEXT_DIM, anchor="w")
        self._pv.pack(fill="x", padx=20, pady=(0, 8))
        self._text.bind("<KeyRelease>", self._chk)
        bf = ctk.CTkFrame(self, fg_color="transparent")
        bf.pack(fill="x", padx=20, pady=(0, 20))
        self._ib = ctk.CTkButton(
            bf, text="Import & Save", width=120, height=32,
            font=ctk.CTkFont(size=12, weight="bold"),
            fg_color=ACCENT, hover_color=ACCENT_HOVER,
            text_color=BG_DARK, corner_radius=6,
            command=self._imp, state="disabled")
        self._ib.pack(side="left")
        ctk.CTkButton(bf, text="Cancel", width=80, height=32,
                       fg_color=BG_INPUT, hover_color=BORDER,
                       text_color=TEXT_SECONDARY, corner_radius=6,
                       command=self._cancel).pack(side="right")
        self.grab_set()
        self.wait_window()

    def _chk(self, _e=None):
        raw = self._text.get("1.0", "end").strip()
        if not raw:
            self._pv.configure(text="", text_color=TEXT_DIM)
            self._ib.configure(state="disabled")
            return
        try:
            n, d, h, r = _decode_preset_string(raw)
            self._pv.configure(
                text=f"\u2714  {n}  |  Dist {d:.1f}  |  "
                     f"Height {h:+.2f}  |  Shift {r:+.2f}",
                text_color=SUCCESS)
            self._ib.configure(state="normal")
        except ValueError as e:
            self._pv.configure(text=f"\u2716  {e}", text_color=ERROR)
            self._ib.configure(state="disabled")

    def _imp(self):
        raw = self._text.get("1.0", "end").strip()
        try:
            self._result = _decode_preset_string(raw)
        except ValueError:
            return
        self.destroy()

    def _cancel(self):
        self.destroy()

    def get_result(self):
        return self._result


# ---------------------------------------------------------------------------
# Startup
# ---------------------------------------------------------------------------
def _show_error_and_exit(title, message):
    root = ctk.CTk()
    root.withdraw()
    from tkinter import messagebox
    messagebox.showerror(title, message)
    sys.exit(1)


def main():
    ctk.set_appearance_mode("dark")
    ctk.set_default_color_theme("dark-blue")

    game_dir = _find_game_dir()
    if not game_dir:
        root = ctk.CTk()
        root.withdraw()
        from tkinter import filedialog, messagebox
        messagebox.showinfo(
            "Ultimate Camera Mod",
            "Could not find Crimson Desert automatically.\n\n"
            "Please select the game folder\n"
            "(the one containing the '0010' folder).")
        game_dir = filedialog.askdirectory(title="Select Crimson Desert folder")
        root.destroy()
        if not game_dir or not os.path.isfile(
                os.path.join(game_dir, "0010", "0.paz")):
            _show_error_and_exit(
                "Game Not Found",
                "Could not find 0010\\0.paz in the selected folder.\n"
                "Make sure you selected the correct Crimson Desert directory.")

    test_file = os.path.join(game_dir, "0010", "_perm_test.tmp")
    try:
        with open(test_file, "wb") as f:
            f.write(b"\x00")
        os.remove(test_file)
    except (PermissionError, OSError):
        _show_error_and_exit(
            "Permission Error",
            f"Cannot write to game files at:\n{game_dir}\\0010\n\n"
            "If using Xbox/Game Pass, try moving the game to another drive.\n"
            "Otherwise, right-click the game folder > Properties > "
            "uncheck 'Read-only'.")

    camera_mod._backups_dir = _get_backups_dir
    saved_state = _load_install_state()
    update_detected = False
    if saved_state:
        current_comp = _get_current_comp_size(game_dir)
        if current_comp is not None:
            saved_comp = saved_state.get("comp_size")
            if saved_comp is not None and current_comp != saved_comp:
                update_detected = True

    app = UltraWideDesertApp(game_dir, update_detected=update_detected,
                             saved_state=saved_state)
    app.mainloop()


if __name__ == "__main__":
    main()

#!/usr/bin/env python3
# ==============================================================================
# UCM.PY - UltimateCameraMod for Crimson Desert, Linux CLI port
# ==============================================================================
# Description: Single-file Python port of UltimateCameraMod. Modifies the
#              camera preset XML inside Crimson Desert's 0010/0.paz archive
#              to apply style, FOV, and gameplay camera tweaks. No GUI, no
#              .NET, no Wine — pure Python + one LZ4 dependency.
# Author: Matt Barham (Linux/Python port)
# Credit: 0xFitz / FitzDegenhub (original UltimateCameraMod, C#/WPF)
#         https://github.com/FitzDegenhub/UltimateCameraMod
# Created: 2026-04-11
# Modified: 2026-04-11
# Version: 0.1.0
# License: MIT (matches upstream)
#
# Dependencies: python >= 3.10, lz4 (pacman -S python-lz4 | pip install lz4)
#
# Usage:
#   python ucm.py list                      # list available style presets
#   python ucm.py apply --preset panoramic  # apply a named preset
#   python ucm.py apply --style heroic --fov 10 --bane
#   python ucm.py apply --custom --distance 7 --height 0 --right-offset 0
#   python ucm.py extract --out camera.xml  # dump current XML
#   python ucm.py restore                   # restore vanilla backup
#
# Auto-detects Crimson Desert via Steam libraryfolders.vdf (native + flatpak).
# Override with --game-dir /path/to/Crimson\ Desert.
#
# Vanilla backups land in $XDG_DATA_HOME/ultimate_camera_mod/ (default
# ~/.local/share/ultimate_camera_mod/). Override with --backup-dir PATH.
# ==============================================================================

from __future__ import annotations

import argparse
import json
import os
import re
import struct
import sys
import secrets
import shutil
from dataclasses import dataclass, field
from pathlib import Path
from typing import Callable, Iterable

_lz4_block = None


def _get_lz4():
    """Lazy import of lz4.block so metadata-only commands don't require it."""
    global _lz4_block
    if _lz4_block is None:
        try:
            import lz4.block as lz4_block  # type: ignore
        except ImportError:
            sys.stderr.write(
                "ERROR: python 'lz4' package is required.\n"
                "Install with: pip install --user lz4\n"
            )
            sys.exit(1)
        _lz4_block = lz4_block
    return _lz4_block


# ═══════════════════════════════════════════════════════════════════════════════
# Helpers
# ═══════════════════════════════════════════════════════════════════════════════

U32_MASK = 0xFFFFFFFF


def _u32(x: int) -> int:
    return x & U32_MASK


def _rotl32(v: int, k: int) -> int:
    v &= U32_MASK
    return ((v << k) | (v >> (32 - k))) & U32_MASK


def _le32(buf: bytes, off: int) -> int:
    return struct.unpack_from("<I", buf, off)[0]


def fmt_num(x: float) -> str:
    """Match C# `$"{d}"` default double formatting: 8.0 -> "8", 3.4 -> "3.4"."""
    s = f"{x:.10g}"
    # Avoid scientific notation for tiny values we actually use
    if "e" in s or "E" in s:
        s = f"{x:.10f}".rstrip("0").rstrip(".")
        if s == "" or s == "-":
            s = "0"
    return s


def fmt_f1(x: float) -> str:
    return f"{x:.1f}"


def fmt_f2(x: float) -> str:
    return f"{x:.2f}"


# ═══════════════════════════════════════════════════════════════════════════════
# NameHasher — Jenkins Lookup3
# ═══════════════════════════════════════════════════════════════════════════════


def compute_hash(data: bytes, initval: int = 0) -> int:
    length = len(data)
    a = b = c = _u32(0xDEADBEEF + length + initval)
    off = 0

    while length > 12:
        a = _u32(a + _le32(data, off))
        b = _u32(b + _le32(data, off + 4))
        c = _u32(c + _le32(data, off + 8))

        a = _u32(a - c); a ^= _rotl32(c, 4);  c = _u32(c + b)
        b = _u32(b - a); b ^= _rotl32(a, 6);  a = _u32(a + c)
        c = _u32(c - b); c ^= _rotl32(b, 8);  b = _u32(b + a)
        a = _u32(a - c); a ^= _rotl32(c, 16); c = _u32(c + b)
        b = _u32(b - a); b ^= _rotl32(a, 19); a = _u32(a + c)
        c = _u32(c - b); c ^= _rotl32(b, 4);  b = _u32(b + a)

        off += 12
        length -= 12

    tail = bytearray(12)
    tail[:length] = data[off:off + length]

    if length >= 12:
        c = _u32(c + _le32(tail, 8))
    elif length >= 9:
        v = _le32(tail, 8)
        c = _u32(c + (v & (U32_MASK >> (8 * (12 - length)))))
    if length >= 8:
        b = _u32(b + _le32(tail, 4))
    elif length >= 5:
        v = _le32(tail, 4)
        b = _u32(b + (v & (U32_MASK >> (8 * (8 - length)))))
    if length >= 4:
        a = _u32(a + _le32(tail, 0))
    elif length >= 1:
        v = _le32(tail, 0)
        a = _u32(a + (v & (U32_MASK >> (8 * (4 - length)))))
    else:
        return c

    c ^= b; c = _u32(c - _rotl32(b, 14))
    a ^= c; a = _u32(a - _rotl32(c, 11))
    b ^= a; b = _u32(b - _rotl32(a, 25))
    c ^= b; c = _u32(c - _rotl32(b, 16))
    a ^= c; a = _u32(a - _rotl32(c, 4))
    b ^= a; b = _u32(b - _rotl32(a, 14))
    c ^= b; c = _u32(c - _rotl32(b, 24))

    return c


# ═══════════════════════════════════════════════════════════════════════════════
# StreamTransform — custom ChaCha20-style cipher (key=32, nonce=16)
# ═══════════════════════════════════════════════════════════════════════════════

_CHACHA_CONSTANTS = (0x61707865, 0x3320646E, 0x79622D32, 0x6B206574)


def _quarter_round(s: list[int], a: int, b: int, c: int, d: int) -> None:
    s[a] = _u32(s[a] + s[b]); s[d] ^= s[a]; s[d] = _rotl32(s[d], 16)
    s[c] = _u32(s[c] + s[d]); s[b] ^= s[c]; s[b] = _rotl32(s[b], 12)
    s[a] = _u32(s[a] + s[b]); s[d] ^= s[a]; s[d] = _rotl32(s[d], 8)
    s[c] = _u32(s[c] + s[d]); s[b] ^= s[c]; s[b] = _rotl32(s[b], 7)


def stream_apply(data: bytes, key: bytes, nonce: bytes) -> bytes:
    if len(key) != 32:
        raise ValueError("Key must be 32 bytes")
    if len(nonce) != 16:
        raise ValueError("Nonce must be 16 bytes")

    output = bytearray(len(data))
    counter = _le32(nonce, 0)

    key_words = struct.unpack("<8I", key)
    nonce_words = struct.unpack("<4I", nonce)

    for offset in range(0, len(data), 64):
        state = [
            _CHACHA_CONSTANTS[0], _CHACHA_CONSTANTS[1],
            _CHACHA_CONSTANTS[2], _CHACHA_CONSTANTS[3],
            key_words[0], key_words[1], key_words[2], key_words[3],
            key_words[4], key_words[5], key_words[6], key_words[7],
            counter,
            nonce_words[1], nonce_words[2], nonce_words[3],
        ]
        working = state[:]
        for _ in range(10):
            _quarter_round(working, 0, 4, 8, 12)
            _quarter_round(working, 1, 5, 9, 13)
            _quarter_round(working, 2, 6, 10, 14)
            _quarter_round(working, 3, 7, 11, 15)
            _quarter_round(working, 0, 5, 10, 15)
            _quarter_round(working, 1, 6, 11, 12)
            _quarter_round(working, 2, 7, 8, 13)
            _quarter_round(working, 3, 4, 9, 14)

        for i in range(16):
            working[i] = _u32(working[i] + state[i])

        block = struct.pack("<16I", *working)
        remaining = min(64, len(data) - offset)
        for i in range(remaining):
            output[offset + i] = data[offset + i] ^ block[i]

        counter = _u32(counter + 1)

    return bytes(output)


# ═══════════════════════════════════════════════════════════════════════════════
# AssetCodec — key derivation from filename
# ═══════════════════════════════════════════════════════════════════════════════

_HASH_INITVAL = 0x000C5EDE
_IV_XOR = 0x60616263
_XOR_DELTAS = (
    0x00000000, 0x0A0A0A0A, 0x0C0C0C0C, 0x06060606,
    0x0E0E0E0E, 0x0A0A0A0A, 0x06060606, 0x02020202,
)


def build_codec_params(filename: str) -> tuple[bytes, bytes]:
    basename = os.path.basename(filename).lower()
    seed = compute_hash(basename.encode("utf-8"), _HASH_INITVAL)

    seed_bytes = struct.pack("<I", seed)
    iv = seed_bytes * 4  # 16 bytes

    key_base = _u32(seed ^ _IV_XOR)
    key = bytearray(32)
    for i in range(8):
        val = _u32(key_base ^ _XOR_DELTAS[i])
        struct.pack_into("<I", key, i * 4, val)

    return bytes(key), iv


def asset_decode(data: bytes, filename: str) -> bytes:
    key, iv = build_codec_params(filename)
    return stream_apply(data, key, iv)


# Encode is symmetric XOR stream
asset_encode = asset_decode


# ═══════════════════════════════════════════════════════════════════════════════
# PazEntry / PamtReader
# ═══════════════════════════════════════════════════════════════════════════════


@dataclass
class PazEntry:
    path: str
    paz_file: str
    offset: int
    comp_size: int
    orig_size: int
    flags: int
    paz_index: int


def pamt_parse(pamt_path: str, paz_dir: str | None = None) -> list[PazEntry]:
    data = Path(pamt_path).read_bytes()
    paz_dir = paz_dir or os.path.dirname(pamt_path) or "."
    pamt_stem = os.path.splitext(os.path.basename(pamt_path))[0]

    off = 0
    off += 4  # magic
    paz_count = _le32(data, off); off += 4
    off += 8  # hash + zero

    for i in range(paz_count):
        off += 4  # hash
        off += 4  # size
        if i < paz_count - 1:
            off += 4  # separator

    # Folder section
    folder_size = _le32(data, off); off += 4
    folder_end = off + folder_size
    folder_prefix = ""
    while off < folder_end:
        parent = _le32(data, off)
        slen = data[off + 4]
        name = data[off + 5:off + 5 + slen].decode("utf-8")
        if parent == 0xFFFFFFFF:
            folder_prefix = name
        off += 5 + slen

    # Node section
    node_size = _le32(data, off); off += 4
    node_start = off
    nodes: dict[int, tuple[int, str]] = {}
    while off < node_start + node_size:
        rel = off - node_start
        parent = _le32(data, off)
        slen = data[off + 4]
        name = data[off + 5:off + 5 + slen].decode("utf-8")
        nodes[rel] = (parent, name)
        off += 5 + slen

    def build_path(node_ref: int) -> str:
        parts: list[str] = []
        cur = node_ref
        guard = 0
        while cur != 0xFFFFFFFF and guard < 64:
            node = nodes.get(cur)
            if node is None:
                break
            parts.append(node[1])
            cur = node[0]
            guard += 1
        return "".join(reversed(parts))

    # Record section
    folder_count = _le32(data, off); off += 4
    off += 4  # hash
    off += folder_count * 16

    entries: list[PazEntry] = []
    while off + 20 <= len(data):
        node_ref = _le32(data, off)
        paz_offset = _le32(data, off + 4)
        comp_size = _le32(data, off + 8)
        orig_size = _le32(data, off + 12)
        flags = _le32(data, off + 16)
        off += 20

        paz_index = flags & 0xFF
        node_path = build_path(node_ref)
        full_path = node_path if not folder_prefix else f"{folder_prefix}/{node_path}"

        paz_num = int(pamt_stem) + paz_index
        paz_file = os.path.join(paz_dir, f"{paz_num}.paz")

        entries.append(PazEntry(
            path=full_path,
            paz_file=paz_file,
            offset=paz_offset,
            comp_size=comp_size,
            orig_size=orig_size,
            flags=flags,
            paz_index=paz_index,
        ))

    return entries


# ═══════════════════════════════════════════════════════════════════════════════
# CompressionUtils — raw LZ4 block format (no frame header)
# ═══════════════════════════════════════════════════════════════════════════════


def lz4_decompress(data: bytes, original_size: int) -> bytes:
    # python-lz4 block mode, no prepended length header
    return _get_lz4().decompress(data, uncompressed_size=original_size)


def lz4_compress(data: bytes) -> bytes:
    return _get_lz4().compress(data, store_size=False, mode="default")


# ═══════════════════════════════════════════════════════════════════════════════
# ArchiveWriter — size-matching strategies to hit exact comp_size
# ═══════════════════════════════════════════════════════════════════════════════


def _pad_to_orig_size(data: bytes, orig_size: int) -> bytes:
    if len(data) >= orig_size:
        return data[:orig_size]
    return data + b"\x00" * (orig_size - len(data))


def _find_xml_comments(data: bytes) -> list[tuple[int, int]]:
    comments: list[tuple[int, int]] = []
    pos = 0
    while True:
        start = data.find(b"<!--", pos)
        if start == -1:
            break
        content_start = start + 4
        end = data.find(b"-->", content_start)
        if end == -1:
            break
        if end > content_start:
            comments.append((content_start, end))
        pos = end + 3
    return comments


_XML_SAFE_ALPHABET = bytes(
    c for c in range(0x20, 0x7F)
    if c not in (0x2D, 0x3C, 0x3E, 0x26)  # no - < > &
)


def _make_xml_safe_random(length: int) -> bytes:
    if length <= 0:
        return b""
    rand = secrets.token_bytes(length)
    alpha_len = len(_XML_SAFE_ALPHABET)
    return bytes(_XML_SAFE_ALPHABET[b % alpha_len] for b in rand)


def _shrink_to_orig_size(data: bytes, orig_size: int) -> bytes:
    if len(data) <= orig_size:
        return _pad_to_orig_size(data, orig_size)

    result = bytearray(data)
    excess = len(result) - orig_size

    # Strategy 1: trim comment bodies, longest first
    while excess > 0:
        comments = _find_xml_comments(bytes(result))
        comments.sort(key=lambda c: c[1] - c[0], reverse=True)
        trimmed = False
        for cstart, cend in comments:
            body_len = cend - cstart
            removable = body_len - 1
            if removable <= 0:
                continue
            to_remove = min(removable, excess)
            del result[cstart + 1:cstart + 1 + to_remove]
            excess -= to_remove
            trimmed = True
            break
        if not trimmed:
            break

    if excess <= 0:
        return bytes(result[:orig_size]) + b"\x00" * max(0, orig_size - len(result))

    # Strategy 2: collapse adjacent duplicate whitespace
    i = len(result) - 1
    while i > 0 and excess > 0:
        if result[i] in (0x20, 0x09) and result[i - 1] in (0x20, 0x09):
            del result[i]
            excess -= 1
        i -= 1

    if excess <= 0:
        return _pad_to_orig_size(bytes(result), orig_size)

    # Strategy 3: remove full comments
    while excess > 0:
        comments = _find_xml_comments(bytes(result))
        if not comments:
            break
        removed = False
        for cstart, cend in comments:
            full_start = cstart - 4
            full_end = cend + 3
            removable = full_end - full_start
            if removable <= excess + 7:
                to_remove = min(removable, excess)
                del result[full_start:full_start + to_remove]
                excess -= to_remove
                removed = True
                break
        if not removed:
            break

    if len(result) > orig_size:
        raise RuntimeError(
            f"Modified file is {len(data) - orig_size} bytes over orig_size ({orig_size}); "
            f"only trimmed {len(data) - len(result)} bytes."
        )
    return _pad_to_orig_size(bytes(result), orig_size)


def _binary_search_size(
    build_trial: Callable[[int], bytes],
    lo: int,
    hi: int,
    target_comp_size: int,
) -> bytes | None:
    """Binary search on a build_trial parameter that is monotone in compressed size."""
    while lo <= hi:
        mid = (lo + hi) // 2
        trial = build_trial(mid)
        c = len(lz4_compress(trial))
        if c == target_comp_size:
            return trial
        elif c < target_comp_size:
            lo = mid + 1
        else:
            hi = mid - 1
    # Scan near boundary
    for n in range(max(0, lo - 30), lo + 30):
        trial = build_trial(n)
        if len(lz4_compress(trial)) == target_comp_size:
            return trial
    return None


def _inflate_with_comments(
    padded: bytes, plaintext_len: int, target_comp_size: int, target_orig_size: int
) -> bytes | None:
    padding_available = target_orig_size - plaintext_len
    base_comp = len(lz4_compress(padded))
    needed = target_comp_size - base_comp
    if needed <= 0:
        return None

    # Comment-padding strategy at the tail
    if padding_available >= 8:
        max_body = padding_available - 7
        rand_body = _make_xml_safe_random(max_body)

        def build_trial(body_len: int) -> bytes:
            body = rand_body[:body_len]
            comment = b"<!--" + body + b"-->"
            combined = padded[:plaintext_len] + comment
            return combined.ljust(target_orig_size, b"\x00")[:target_orig_size]

        c_min = len(lz4_compress(build_trial(0)))
        c_max = len(lz4_compress(build_trial(max_body)))
        if c_min <= target_comp_size <= c_max:
            result = _binary_search_size(build_trial, 0, max_body, target_comp_size)
            if result is not None:
                return result

    return None


def _pad_with_scattered_comments(
    plaintext: bytes, target_comp_size: int, target_orig_size: int
) -> bytes | None:
    gap = target_orig_size - len(plaintext)
    if gap < 8:
        return None

    newlines = [i for i, b in enumerate(plaintext) if b == 0x0A]
    if len(newlines) < 4:
        return None

    target_slot_candidates = [20, 50, 100, 200, 400, min(800, len(newlines) // 2)]
    for target_slots in target_slot_candidates:
        num_slots = min(target_slots, len(newlines))
        if num_slots < 1:
            continue
        step = max(1, len(newlines) // num_slots)
        slots = []
        i = 0
        while i < len(newlines) and len(slots) < num_slots:
            slots.append(newlines[i])
            i += step
        if not slots:
            continue

        overhead = len(slots) * 7
        if overhead >= gap:
            continue
        max_total_body = gap - overhead

        for _ in range(6):
            rand_pool = _make_xml_safe_random(max_total_body + 64)
            slot_count = len(slots)
            captured_slots = slots[:]

            def build_trial(total_body: int,
                            _slots=captured_slots,
                            _pool=rand_pool,
                            _sc=slot_count) -> bytes:
                per_slot = total_body // _sc
                remainder = total_body % _sc

                output = bytearray(plaintext)
                pool_off = 0
                # Insert in reverse so positions stay valid
                for si in range(_sc - 1, -1, -1):
                    body_len = per_slot + (1 if si < remainder else 0)
                    insert_at = min(_slots[si] + 1, len(output))
                    body = bytes(_pool[(pool_off + b) % len(_pool)] for b in range(body_len))
                    pool_off += body_len
                    comment = b"<!--" + body + b"-->"
                    output[insert_at:insert_at] = comment

                result = bytes(output[:target_orig_size])
                if len(result) < target_orig_size:
                    result = result + b"\x00" * (target_orig_size - len(result))
                return result

            c_min = len(lz4_compress(build_trial(0)))
            c_max = len(lz4_compress(build_trial(max_total_body)))
            if not (c_min <= target_comp_size <= c_max):
                continue

            result = _binary_search_size(build_trial, 0, max_total_body, target_comp_size)
            if result is not None:
                return result

    return None


def _inflate_by_replacing_comment_bodies(padded: bytes, target_comp_size: int) -> bytes | None:
    comments = _find_xml_comments(padded)
    if not comments:
        return None
    positions = [i for cstart, cend in comments for i in range(cstart, cend)]
    if not positions:
        return None
    total = len(positions)

    for _ in range(8):
        rand_fill = _make_xml_safe_random(total)

        def build_trial(n: int, _rf=rand_fill, _pos=positions) -> bytes:
            trial = bytearray(padded)
            for idx in range(n):
                trial[_pos[idx]] = _rf[idx]
            return bytes(trial)

        c_none = len(lz4_compress(build_trial(0)))
        c_all = len(lz4_compress(build_trial(total)))
        if target_comp_size < c_none or target_comp_size > c_all:
            continue
        result = _binary_search_size(build_trial, 0, total, target_comp_size)
        if result is not None:
            return result
    return None


def match_compressed_size(plaintext: bytes, target_comp_size: int, target_orig_size: int) -> bytes:
    if len(plaintext) > target_orig_size:
        padded = _shrink_to_orig_size(plaintext, target_orig_size)
    else:
        padded = _pad_to_orig_size(plaintext, target_orig_size)

    comp = lz4_compress(padded)
    if len(comp) == target_comp_size:
        return padded

    delta = len(comp) - target_comp_size

    if delta < 0:
        # Needs to compress LESS well — scatter comments to break LZ4 matches
        result = _pad_with_scattered_comments(plaintext, target_comp_size, target_orig_size)
        if result is not None:
            return result

        effective_len = min(len(plaintext), target_orig_size)
        result = _inflate_with_comments(padded, effective_len, target_comp_size, target_orig_size)
        if result is not None:
            return result

        result = _inflate_by_replacing_comment_bodies(padded, target_comp_size)
        if result is not None:
            return result

        raise RuntimeError(
            f"Cannot match target comp_size {target_comp_size} "
            f"(got {len(comp)}, delta {delta}). File compresses too well."
        )

    over_by = len(comp) - target_comp_size
    raise RuntimeError(
        f"Preset is too large to install — camera data exceeds the game's slot "
        f"by {over_by:,} bytes ({len(comp):,} / {target_comp_size:,}).\n"
        "Try a simpler preset, delete ucm_backups and verify Steam files, or "
        "check for a recent game patch."
    )


def update_entry(entry: PazEntry, payload: bytes) -> None:
    with open(entry.paz_file, "r+b") as f:
        f.seek(entry.offset)
        f.write(payload)


# ═══════════════════════════════════════════════════════════════════════════════
# CameraMod — XML patch engine
# ═══════════════════════════════════════════════════════════════════════════════

_SUB_ELEMENT_TAGS = {
    "CameraDamping", "CameraBlendParameter",
    "OffsetByVelocity", "PivotHeight", "ZoomLevel",
}

_TAG_RE = re.compile(r"<(\w+)\s+([^>]*?)(/?)>")
_BARE_OPEN_RE = re.compile(r"^<(\w+)>$")
_ATTR_RE = re.compile(r'(\w+)="([^"]*)"')


def _parse_attrs(attrs_str: str) -> dict[str, str]:
    return {m.group(1): m.group(2) for m in _ATTR_RE.finditer(attrs_str)}


def strip_comments(xml_text: str) -> str:
    result: list[str] = []
    in_comment = False
    for line in xml_text.split("\n"):
        stripped = line.strip()
        if in_comment:
            if "-->" in stripped:
                in_comment = False
            continue
        if "<!--" in stripped and "-->" not in stripped:
            in_comment = True
            continue
        if stripped.startswith("<!--") and stripped.endswith("-->"):
            continue
        if not stripped:
            continue
        result.append(line)
    return "\n".join(result)


def strip_header_comments(xml_text: str) -> str:
    result: list[str] = []
    in_comment = False
    header_done = False
    for line in xml_text.split("\n"):
        stripped = line.strip()
        if header_done:
            result.append(line)
            continue
        if "<!--" in stripped and "-->" not in stripped:
            in_comment = True
            continue
        if in_comment:
            if "-->" in stripped:
                in_comment = False
            continue
        if stripped.startswith("<!--") and stripped.endswith("-->"):
            continue
        if not stripped:
            continue
        header_done = True
        result.append(line)
    return "\n".join(result)


# A ModificationSet is (element_mods, fov_value)
# element_mods: dict[mod_key, dict[attr, (action, value)]]
ModificationSet = tuple[dict[str, dict[str, tuple[str, str]]], int]


def apply_modifications(xml_text: str, mod_set: ModificationSet) -> str:
    element_mods, fov_value = mod_set
    lines = xml_text.split("\n")
    depth_stack: list[tuple[str, bool]] = []  # (tag, is_section)
    key_counter: dict[str, int] = {}
    result: list[str] = []
    applied_zoom_levels: set[str] = set()

    def section_tag() -> str:
        for i in range(len(depth_stack) - 1, -1, -1):
            if depth_stack[i][1]:
                return depth_stack[i][0]
        return ""

    for line in lines:
        stripped = line.strip()

        if stripped == "</>":
            # Closing — possibly inject ZoomLevel nodes before closing ZoomLevelInfo
            if depth_stack and depth_stack[-1][0] == "ZoomLevelInfo":
                sec_tag = section_tag()
                if sec_tag:
                    prefix = f"{sec_tag}/ZoomLevel["
                    pending: list[tuple[int, str]] = []
                    for mod_key, mod_attrs in element_mods.items():
                        if not mod_key.startswith(prefix):
                            continue
                        if mod_key in applied_zoom_levels:
                            continue
                        level_str = mod_key[len(prefix):].rstrip("]")
                        try:
                            level_num = int(level_str)
                        except ValueError:
                            continue
                        parts = [f'Level="{level_str}"']
                        for attr, (_, val) in mod_attrs.items():
                            parts.append(f'{attr}="{val}"')
                        indent = "\t" * len(depth_stack)
                        pending.append((level_num, f"{indent}<ZoomLevel {' '.join(parts)}/>"))
                    pending.sort(key=lambda x: x[0])
                    for _, xml_line in pending:
                        result.append(xml_line)

            result.append(line)
            if depth_stack:
                depth_stack.pop()
            continue

        bm = _BARE_OPEN_RE.match(stripped)
        if bm:
            bare_tag = bm.group(1)
            depth_stack.append((bare_tag, False))
            result.append(line)
            if bare_tag == "ZoomLevelInfo":
                sec_tag = section_tag()
                if sec_tag:
                    prefix = f"{sec_tag}/ZoomLevel["
                    early: list[tuple[int, str]] = []
                    for mod_key, mod_attrs in element_mods.items():
                        if not mod_key.startswith(prefix):
                            continue
                        level_str = mod_key[len(prefix):].rstrip("]")
                        try:
                            level_num = int(level_str)
                        except ValueError:
                            continue
                        if level_num >= 1:
                            continue
                        parts = [f'Level="{level_str}"']
                        for attr, (_, val) in mod_attrs.items():
                            parts.append(f'{attr}="{val}"')
                        indent = "\t" * len(depth_stack)
                        early.append((level_num, f"{indent}<ZoomLevel {' '.join(parts)}/>"))
                        applied_zoom_levels.add(mod_key)
                    early.sort(key=lambda x: x[0])
                    for _, xml_line in early:
                        result.append(xml_line)
            continue

        m = _TAG_RE.match(stripped)
        if not m:
            result.append(line)
            continue

        tag = m.group(1)
        attrs_str = m.group(2)
        self_closing = m.group(3) == "/"
        attrs = _parse_attrs(attrs_str)
        parent_tag = section_tag()

        if tag == "ZoomLevel":
            level = attrs.get("Level", "?")
            key = f"ZoomLevel[{level}]" if not parent_tag else f"{parent_tag}/ZoomLevel[{level}]"
        else:
            key = tag if not parent_tag else f"{parent_tag}/{tag}"

        key_counter[key] = key_counter.get(key, 0) + 1
        occurrence = key_counter[key]
        indexed_key = f"{key}#{occurrence}"

        modified_line = line
        match_key: str | None = None
        if indexed_key in element_mods:
            match_key = indexed_key
        elif key in element_mods:
            match_key = key

        if match_key is not None:
            if tag == "ZoomLevel":
                applied_zoom_levels.add(match_key)
            for attr, (action, value) in element_mods[match_key].items():
                if action == "SET":
                    if re.search(rf'{re.escape(attr)}="', modified_line):
                        modified_line = re.sub(
                            rf'{re.escape(attr)}="[^"]*"',
                            f'{attr}="{value}"',
                            modified_line,
                        )
                    else:
                        modified_line = re.sub(
                            r"(/?>)",
                            f' {attr}="{value}"\\1',
                            modified_line,
                            count=1,
                        )
                elif action == "REMOVE":
                    modified_line = re.sub(rf'\s+{re.escape(attr)}="[^"]*"', "", modified_line)

        if fov_value > 0:
            if tag in _SUB_ELEMENT_TAGS or tag == "ZoomLevel":
                sec = parent_tag
            else:
                sec = tag
            apply_fov = sec.startswith("Player_") or sec.startswith("Cinematic_") or sec.startswith("Glide_")
            if apply_fov:
                fov_match = re.search(r'(?<!\w)Fov="([^"]*)"', modified_line)
                if fov_match:
                    try:
                        cur_fov = float(fov_match.group(1))
                        new_fov = int(round(cur_fov + fov_value))
                        modified_line = re.sub(
                            r'(?<!\w)Fov="[^"]*"',
                            f'Fov="{new_fov}"',
                            modified_line,
                        )
                    except ValueError:
                        pass
                if tag == "ZoomLevel":
                    idfov_match = re.search(r'InDoorFov="([^"]*)"', modified_line)
                    if idfov_match:
                        try:
                            cur = float(idfov_match.group(1))
                            new = int(round(cur + fov_value))
                            modified_line = re.sub(
                                r'InDoorFov="[^"]*"',
                                f'InDoorFov="{new}"',
                                modified_line,
                            )
                        except ValueError:
                            pass

        result.append(modified_line)

        if tag not in _SUB_ELEMENT_TAGS and tag != "ZoomLevel" and not self_closing:
            depth_stack.append((tag, True))

    return "\n".join(result)


# ═══════════════════════════════════════════════════════════════════════════════
# CameraRules — section lists, style builders, composition
# ═══════════════════════════════════════════════════════════════════════════════

BASIC_SECTIONS = (
    "Player_Basic_Default",
    "Player_Basic_Default_Walk",
    "Player_Basic_Default_Run",
    "Player_Basic_Default_Runfast",
)

WEAPON_SECTIONS = (
    "Player_Weapon_Default",
    "Player_Weapon_Default_Walk",
    "Player_Weapon_Default_Run",
    "Player_Weapon_Default_RunFast",
    "Player_Weapon_Default_RunFast_Follow",
    "Player_Weapon_Rush",
    "Player_Weapon_Guard",
)

WALK_RUN = (
    "Player_Basic_Default_Walk",
    "Player_Basic_Default_Run",
    "Player_Basic_Default_Runfast",
    "Player_Weapon_Default_Walk",
    "Player_Weapon_Default_Run",
    "Player_Weapon_Default_RunFast",
    "Player_Weapon_Default_RunFast_Follow",
)

ALL_MAIN = BASIC_SECTIONS + WEAPON_SECTIONS

LOCKON_SECTIONS = (
    "Player_Weapon_LockOn",
    "Player_Weapon_TwoTarget",
    "Player_Weapon_LockOn_System",
    "Player_Revive_LockOn_System",
    "Player_FollowLearn_LockOn_Boss",
    "Player_Weapon_LockOn_Non_Rotate",
    "Player_Weapon_LockOn_WrestleOnly",
    "Player_Interaction_TwoTarget",
)

BANE_SECTIONS = BASIC_SECTIONS + WEAPON_SECTIONS

HORSE_RIDE_SECTIONS = (
    "Player_Ride_Horse",
    "Player_Ride_Horse_Run",
    "Player_Ride_Horse_Fast_Run",
    "Player_Ride_Horse_Dash",
    "Player_Ride_Horse_Dash_Att",
    "Player_Ride_Horse_Att_Thrust",
    "Player_Ride_Horse_Att_R",
    "Player_Ride_Horse_Att_L",
)

ALL_MOUNT_SECTIONS = HORSE_RIDE_SECTIONS + (
    "Player_Ride_Elephant",
    "Player_Ride_Wyvern",
    "Player_Ride_Canoe",
    "Player_Ride_Warmachine",
    "Player_Ride_Broom",
)

VANILLA_RO_ZL2 = 0.5
VANILLA_RO_ZL3 = 0.8
VANILLA_RO_ZL4 = 1.1


def _set(mods: dict[str, dict[str, tuple[str, str]]], key: str, attr: str, value: str) -> None:
    mods.setdefault(key, {})[attr] = ("SET", value)


def _merge(base: dict[str, dict[str, tuple[str, str]]],
           overlay: dict[str, dict[str, tuple[str, str]]]) -> None:
    for key, attrs in overlay.items():
        if key not in base:
            base[key] = dict(attrs)
        else:
            base[key].update(attrs)


def build_mount_height_mods(up_offset: float) -> dict:
    mods: dict = {}
    up_str = fmt_num(up_offset)
    for sec in HORSE_RIDE_SECTIONS:
        for lvl in (2, 3):
            _set(mods, f"{sec}/ZoomLevel[{lvl}]", "UpOffset", up_str)
    for sec in ("Player_Ride_Elephant", "Player_Ride_Canoe", "Player_Ride_Warmachine", "Player_Ride_Broom"):
        for lvl in (2, 3):
            _set(mods, f"{sec}/ZoomLevel[{lvl}]", "UpOffset", up_str)
    for lvl in (2, 3, 4):
        _set(mods, f"Player_Ride_Wyvern/ZoomLevel[{lvl}]", "UpOffset", up_str)
    return mods


def build_mount_distances(scale: float) -> dict:
    mods: dict = {}
    for sec in HORSE_RIDE_SECTIONS:
        _set(mods, f"{sec}/ZoomLevel[2]", "ZoomDistance", fmt_f1(7.5 * scale))
        _set(mods, f"{sec}/ZoomLevel[3]", "ZoomDistance", fmt_f1(10.5 * scale))
    _set(mods, "Player_Ride_Elephant/ZoomLevel[2]", "ZoomDistance", fmt_f1(8 * scale))
    _set(mods, "Player_Ride_Elephant/ZoomLevel[3]", "ZoomDistance", fmt_f1(11 * scale))
    _set(mods, "Player_Ride_Wyvern/ZoomLevel[2]", "ZoomDistance", fmt_f1(12 * scale))
    _set(mods, "Player_Ride_Wyvern/ZoomLevel[3]", "ZoomDistance", fmt_f1(16 * scale))
    _set(mods, "Player_Ride_Wyvern/ZoomLevel[4]", "ZoomDistance", fmt_f1(20 * scale))
    _set(mods, "Player_Ride_Canoe/ZoomLevel[2]", "ZoomDistance", fmt_f1(6 * scale))
    _set(mods, "Player_Ride_Canoe/ZoomLevel[3]", "ZoomDistance", fmt_f1(9 * scale))
    _set(mods, "Player_Ride_Warmachine/ZoomLevel[2]", "ZoomDistance", fmt_f1(9 * scale))
    _set(mods, "Player_Ride_Warmachine/ZoomLevel[3]", "ZoomDistance", fmt_f1(11 * scale))
    _set(mods, "Player_Ride_Broom/ZoomLevel[2]", "ZoomDistance", fmt_f1(10 * scale))
    _set(mods, "Player_Ride_Broom/ZoomLevel[3]", "ZoomDistance", fmt_f1(14 * scale))
    return mods


def build_shared_base() -> dict:
    m: dict = {}
    fov_sections = [
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
    ]
    for sec in fov_sections:
        _set(m, sec, "Fov", "40")

    _set(m, "Player_Weapon_LockOn", "Fov", "40")
    _set(m, "Player_Weapon_LockOn", "TargetRate", "0.25")
    _set(m, "Player_Weapon_LockOn", "ScreenClampRate", "0.6")

    _set(m, "Player_Weapon_TwoTarget", "Fov", "40")
    _set(m, "Player_Weapon_TwoTarget", "TargetRate", "0.25")
    _set(m, "Player_Weapon_TwoTarget", "ScreenClampRate", "0.6")
    _set(m, "Player_Weapon_TwoTarget", "LimitUnderDistance", "3")

    _set(m, "Player_Interaction_TwoTarget", "Fov", "40")
    _set(m, "Player_Interaction_TwoTarget", "TargetRate", "0.45")
    _set(m, "Player_Interaction_TwoTarget", "ScreenClampRate", "0.65")
    _set(m, "Player_Interaction_TwoTarget/ZoomLevel[3]", "MaxZoomDistance", "10")
    _set(m, "Player_Interaction_TwoTarget/ZoomLevel[4]", "MaxZoomDistance", "10")

    _set(m, "Player_FollowLearn_LockOn_Boss", "Fov", "40")
    _set(m, "Player_FollowLearn_LockOn_Boss", "ScreenClampRate", "0.7")

    _set(m, "Player_Weapon_LockOn_System", "Fov", "40")
    _set(m, "Player_Weapon_LockOn_System", "TargetRate", "0.3")
    _set(m, "Player_Weapon_LockOn_System", "ScreenClampRate", "0.65")
    _set(m, "Player_Weapon_LockOn_System/ZoomLevel[3]", "Fov", "40")
    _set(m, "Player_Weapon_LockOn_System/ZoomLevel[4]", "Fov", "40")

    _set(m, "Player_Revive_LockOn_System", "Fov", "40")
    _set(m, "Player_Revive_LockOn_System", "ScreenClampRate", "0.65")
    _set(m, "Player_Revive_LockOn_System/ZoomLevel[3]", "Fov", "40")
    _set(m, "Player_Revive_LockOn_System/ZoomLevel[4]", "Fov", "40")

    _set(m, "Player_Weapon_LockOn_Non_Rotate", "Fov", "40")
    _set(m, "Player_Weapon_LockOn_Non_Rotate", "ScreenClampRate", "0.6")
    _set(m, "Player_Weapon_LockOn_WrestleOnly", "Fov", "40")
    _set(m, "Player_Weapon_LockOn_WrestleOnly", "ScreenClampRate", "0.6")

    _set(m, "Player_StartAggro_TwoTarget", "Fov", "40")
    _set(m, "Player_StartAggro_TwoTarget", "ScreenClampRate", "0.6")
    _set(m, "Player_Wanted_TwoTarget", "Fov", "40")
    _set(m, "Player_Wanted_TwoTarget", "ScreenClampRate", "0.6")

    for sec in ("Player_Basic_Default", "Player_Basic_Default_Walk",
                "Player_Basic_Default_Run", "Player_Basic_Default_Runfast"):
        _set(m, f"{sec}/ZoomLevel[2]", "ZoomDistance", "3.4")
        _set(m, f"{sec}/ZoomLevel[2]", "Fov", "40")
        _set(m, f"{sec}/ZoomLevel[3]", "ZoomDistance", "6")
        _set(m, f"{sec}/ZoomLevel[3]", "Fov", "40")
        _set(m, f"{sec}/ZoomLevel[4]", "ZoomDistance", "8")
        _set(m, f"{sec}/ZoomLevel[4]", "Fov", "40")

    for sec in ("Player_Weapon_Default",
                "Player_Weapon_Default_Walk", "Player_Weapon_Default_Run",
                "Player_Weapon_Default_RunFast", "Player_Weapon_Default_RunFast_Follow"):
        _set(m, f"{sec}/ZoomLevel[2]", "ZoomDistance", "3.4")
        _set(m, f"{sec}/ZoomLevel[2]", "Fov", "40")
        _set(m, f"{sec}/ZoomLevel[3]", "ZoomDistance", "6")
        _set(m, f"{sec}/ZoomLevel[3]", "Fov", "40")
        _set(m, f"{sec}/ZoomLevel[4]", "ZoomDistance", "8")
        _set(m, f"{sec}/ZoomLevel[4]", "Fov", "40")

    _set(m, "Player_Weapon_Guard", "Fov", "40")
    for sec in ("Player_Weapon_Guard", "Player_Weapon_Rush"):
        _set(m, f"{sec}/ZoomLevel[2]", "ZoomDistance", "3.4")
        _set(m, f"{sec}/ZoomLevel[2]", "Fov", "40")
        _set(m, f"{sec}/ZoomLevel[3]", "ZoomDistance", "6")
        _set(m, f"{sec}/ZoomLevel[3]", "Fov", "40")
        _set(m, f"{sec}/ZoomLevel[4]", "ZoomDistance", "8")
        _set(m, f"{sec}/ZoomLevel[4]", "Fov", "40")

    for sec in ("Player_Basic_Default_Walk", "Player_Basic_Default_Run", "Player_Basic_Default_Runfast"):
        _set(m, f"{sec}/ZoomLevel[2]", "RightOffset", "0.5")
    for sec in ("Player_Weapon_Default", "Player_Weapon_Default_Walk",
                "Player_Weapon_Default_Run", "Player_Weapon_Default_RunFast",
                "Player_Weapon_Default_RunFast_Follow",
                "Player_Weapon_Guard", "Player_Weapon_Rush"):
        _set(m, f"{sec}/ZoomLevel[2]", "RightOffset", "0.5")

    for sec in HORSE_RIDE_SECTIONS:
        _set(m, sec, "Fov", "40")
    _set(m, "Player_Ride_Elephant", "Fov", "40")
    _set(m, "Player_Ride_Wyvern", "Fov", "50")

    for sec in LOCKON_SECTIONS:
        for lvl in (1, 2, 3, 4):
            _set(m, f"{sec}/ZoomLevel[{lvl}]", "MaxZoomDistance", "30")
    for lvl in (1, 2, 3):
        _set(m, f"Player_Ride_Aim_LockOn/ZoomLevel[{lvl}]", "MaxZoomDistance", "30")

    return m


def build_smoothing() -> dict:
    m: dict = {}
    # Idle/walk/run blends
    _set(m, "Player_Basic_Default/CameraBlendParameter", "BlendInTime", "0.6")
    _set(m, "Player_Basic_Default/CameraBlendParameter", "BlendOutTime", "0.4")
    _set(m, "Player_Basic_Default_Walk/CameraBlendParameter", "BlendInTime", "0.4")
    _set(m, "Player_Basic_Default_Walk/CameraBlendParameter", "BlendOutTime", "0.3")
    _set(m, "Player_Basic_Default_Run/CameraBlendParameter", "BlendInTime", "0.4")
    _set(m, "Player_Basic_Default_Run/CameraBlendParameter", "BlendOutTime", "0.4")
    _set(m, "Player_Basic_Default_Runfast/CameraBlendParameter", "BlendInTime", "0.4")
    _set(m, "Player_Basic_Default_Runfast/CameraBlendParameter", "BlendOutTime", "0.4")

    _set(m, "Player_Weapon_Guard/CameraBlendParameter", "BlendInTime", "1.0")
    _set(m, "Player_Weapon_Guard/CameraBlendParameter", "BlendOutTime", "1.0")

    _set(m, "Player_Basic_Default_Run/OffsetByVelocity", "OffsetLength", "0")
    _set(m, "Player_Basic_Default_Runfast/OffsetByVelocity", "OffsetLength", "0.0")
    _set(m, "Player_Weapon_Default_Run/OffsetByVelocity", "OffsetLength", "0")
    _set(m, "Player_Weapon_Default_RunFast/OffsetByVelocity", "OffsetLength", "0.0")
    _set(m, "Player_Weapon_Default_RunFast_Follow/OffsetByVelocity", "OffsetLength", "0.0")

    _set(m, "Player_Animal_Default/CameraBlendParameter", "BlendInTime", "0.3")
    _set(m, "Player_Animal_Default_Run/CameraBlendParameter", "BlendInTime", "0.3")
    _set(m, "Player_Animal_Default_Run/OffsetByVelocity", "OffsetLength", "0")
    _set(m, "Player_Animal_Default_Runfast/CameraBlendParameter", "BlendInTime", "0.3")
    _set(m, "Player_Animal_Default_Runfast/OffsetByVelocity", "OffsetLength", "0.0")
    _set(m, "Player_Animal_Default_Runfast/OffsetByVelocity", "DampSpeed", "0.5")
    _set(m, "Player_Animal_Default_Walk/CameraBlendParameter", "BlendInTime", "0.3")

    for sec in HORSE_RIDE_SECTIONS:
        _set(m, sec, "FollowYawSpeedRate", "0.8")
        _set(m, sec, "FollowPitchSpeedRate", "0.8")
        _set(m, sec, "FollowStartTime", "1")
        _set(m, sec, "FollowDefaultPitch", "13")
        _set(m, f"{sec}/CameraBlendParameter", "BlendInTime", "1.0")
        _set(m, f"{sec}/CameraBlendParameter", "BlendOutTime", "1.0")
        _set(m, f"{sec}/CameraDamping", "PivotDampingMaxDistance", "0.5")
        _set(m, f"{sec}/OffsetByVelocity", "OffsetLength", "0.0")
        _set(m, f"{sec}/OffsetByVelocity", "DampSpeed", "0.5")
        _set(m, f"{sec}/ZoomLevel[2]", "RightOffset", "1.45")
        _set(m, f"{sec}/ZoomLevel[3]", "RightOffset", "1.8")

    _set(m, "Player_Ride_Elephant", "FollowPitchSpeedRate", "0.8")
    _set(m, "Player_Ride_Elephant", "FollowYawSpeedRate", "0.8")
    _set(m, "Player_Ride_Elephant/CameraBlendParameter", "BlendInTime", "1.0")
    _set(m, "Player_Ride_Elephant/CameraBlendParameter", "BlendOutTime", "1.0")
    _set(m, "Player_Ride_Elephant/CameraDamping", "PivotDampingMaxDistance", "0.5")
    _set(m, "Player_Ride_Elephant/OffsetByVelocity", "OffsetLength", "0.0")

    _set(m, "Player_Ride_Wyvern", "FollowStartTime", "1")
    _set(m, "Player_Ride_Wyvern", "FollowYawSpeedRate", "0.8")
    _set(m, "Player_Ride_Wyvern/OffsetByVelocity", "OffsetLength", "0.0")

    for sec in HORSE_RIDE_SECTIONS:
        _set(m, f"{sec}/ZoomLevel[2]", "ZoomDistance", "7.5")
        _set(m, f"{sec}/ZoomLevel[3]", "ZoomDistance", "10.5")
    _set(m, "Player_Ride_Elephant/ZoomLevel[2]", "ZoomDistance", "8.0")
    _set(m, "Player_Ride_Elephant/ZoomLevel[3]", "ZoomDistance", "11.0")
    _set(m, "Player_Ride_Wyvern/ZoomLevel[2]", "ZoomDistance", "12.0")
    _set(m, "Player_Ride_Wyvern/ZoomLevel[3]", "ZoomDistance", "16.0")
    _set(m, "Player_Ride_Wyvern/ZoomLevel[4]", "ZoomDistance", "20.0")

    _set(m, "Player_Weapon_Down/CameraBlendParameter", "BlendInTime", "1.2")
    _set(m, "Player_Weapon_Down/CameraBlendParameter", "BlendOutTime", "1.5")
    _set(m, "Player_Weapon_Rush/CameraBlendParameter", "BlendInTime", "0.6")
    _set(m, "Player_Weapon_Rush/CameraBlendParameter", "BlendOutTime", "0.6")

    for sec in ("Player_Basic_FreeFall_Start", "Player_Basic_FreeFall"):
        _set(m, f"{sec}/CameraBlendParameter", "BlendInTime", "1.0")
        _set(m, f"{sec}/CameraBlendParameter", "BlendOutTime", "1.2")
    _set(m, "Player_Basic_SuperJump/CameraBlendParameter", "BlendInTime", "0.8")
    _set(m, "Player_Basic_SuperJump/CameraBlendParameter", "BlendOutTime", "1.2")
    for sec in ("Player_Basic_RopePull", "Player_Basic_RopeSwing", "Player_Hit_Throw"):
        _set(m, f"{sec}/CameraBlendParameter", "BlendInTime", "0.8")
        _set(m, f"{sec}/CameraBlendParameter", "BlendOutTime", "1.2")
    for sec in ("Player_Ride_Warmachine_Aim", "Player_Ride_Warmachine_Dash"):
        _set(m, f"{sec}/CameraBlendParameter", "BlendInTime", "0.8")
        _set(m, f"{sec}/CameraBlendParameter", "BlendOutTime", "1.0")
    _set(m, "Player_Ride_Aim_LockOn/CameraBlendParameter", "BlendInTime", "1.0")
    _set(m, "Player_Ride_Aim_LockOn/CameraBlendParameter", "BlendOutTime", "1.2")

    _set(m, "Player_Weapon_LockOn/CameraBlendParameter", "BlendInTime", "1.25")
    _set(m, "Player_Weapon_LockOn/CameraBlendParameter", "BlendOutTime", "1.2")
    _set(m, "Player_Weapon_TwoTarget/CameraBlendParameter", "BlendInTime", "1.0")
    _set(m, "Player_Weapon_TwoTarget/CameraBlendParameter", "BlendOutTime", "1.2")
    _set(m, "Player_Weapon_LockOn_System/CameraBlendParameter", "BlendInTime", "1.0")
    _set(m, "Player_Weapon_LockOn_System/CameraBlendParameter", "BlendOutTime", "1.0")
    _set(m, "Player_FollowLearn_LockOn_Boss/CameraBlendParameter", "BlendInTime", "0.6")
    _set(m, "Player_FollowLearn_LockOn_Boss/CameraBlendParameter", "BlendOutTime", "1.0")
    _set(m, "Player_Interaction_TwoTarget/CameraBlendParameter", "BlendInTime", "1.0")
    _set(m, "Player_Interaction_TwoTarget/CameraBlendParameter", "BlendOutTime", "1.0")

    _set(m, "Player_Revive_LockOn_System/CameraBlendParameter", "BlendInTime", "0.8")
    _set(m, "Player_Revive_LockOn_System/CameraBlendParameter", "BlendOutTime", "1.0")
    _set(m, "Player_Force_LockOn/CameraBlendParameter", "BlendInTime", "0.8")
    _set(m, "Player_Force_LockOn/CameraBlendParameter", "BlendOutTime", "1.2")
    _set(m, "Player_LockOn_Titan/CameraBlendParameter", "BlendInTime", "1.0")
    _set(m, "Player_LockOn_Titan/CameraBlendParameter", "BlendOutTime", "1.2")
    _set(m, "Player_Weapon_LockOn_Non_Rotate/CameraBlendParameter", "BlendInTime", "1.0")
    _set(m, "Player_Weapon_LockOn_Non_Rotate/CameraBlendParameter", "BlendOutTime", "1.2")
    _set(m, "Player_Weapon_LockOn_WrestleOnly/CameraBlendParameter", "BlendInTime", "0.8")
    _set(m, "Player_Weapon_LockOn_WrestleOnly/CameraBlendParameter", "BlendOutTime", "1.2")
    _set(m, "Player_StartAggro_TwoTarget/CameraBlendParameter", "BlendInTime", "0.8")
    _set(m, "Player_StartAggro_TwoTarget/CameraBlendParameter", "BlendOutTime", "1.0")
    _set(m, "Player_Wanted_TwoTarget/CameraBlendParameter", "BlendInTime", "0.8")
    _set(m, "Player_Wanted_TwoTarget/CameraBlendParameter", "BlendOutTime", "1.0")

    return m


def build_shared_steadycam() -> dict:
    m: dict = {}
    for lvl in (2, 3, 4):
        _set(m, f"Player_Basic_Default/ZoomLevel[{lvl}]", "UpOffset", "0.0")
    _set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[3]", "UpOffset", "0.0")
    for sec in ("Player_Basic_Default_Walk", "Player_Basic_Default_Run", "Player_Basic_Default_Runfast"):
        for lvl in (2, 3, 4):
            _set(m, f"{sec}/ZoomLevel[{lvl}]", "UpOffset", "0.0")
    for sec in WEAPON_SECTIONS:
        for lvl in (2, 3, 4):
            _set(m, f"{sec}/ZoomLevel[{lvl}]", "UpOffset", "0.0")
    return m


def build_lockon_distances(zl2: float, zl3: float, zl4: float) -> dict:
    m: dict = {}
    zl2s, zl3s, zl4s = fmt_num(zl2), fmt_num(zl3), fmt_num(zl4)
    for sec in LOCKON_SECTIONS:
        _set(m, f"{sec}/ZoomLevel[2]", "ZoomDistance", zl2s)
        _set(m, f"{sec}/ZoomLevel[3]", "ZoomDistance", zl3s)
        _set(m, f"{sec}/ZoomLevel[4]", "ZoomDistance", zl4s)
    _set(m, "Player_Weapon_Down/ZoomLevel[2]", "ZoomDistance", zl2s)
    _set(m, "Player_Weapon_Down/ZoomLevel[3]", "ZoomDistance", zl3s)
    _set(m, "Player_Weapon_Down/ZoomLevel[4]", "ZoomDistance", zl4s)
    return m


def build_heroic() -> dict:
    m: dict = {}
    for sec in ALL_MAIN:
        _set(m, f"{sec}/ZoomLevel[2]", "UpOffset", "-0.2")
        _set(m, f"{sec}/ZoomLevel[2]", "InDoorUpOffset", "-0.2")
        _set(m, f"{sec}/ZoomLevel[2]", "ZoomDistance", "2.5")
        _set(m, f"{sec}/ZoomLevel[3]", "UpOffset", "-0.2")
        _set(m, f"{sec}/ZoomLevel[3]", "ZoomDistance", "5")
        _set(m, f"{sec}/ZoomLevel[4]", "UpOffset", "-0.2")
        _set(m, f"{sec}/ZoomLevel[4]", "ZoomDistance", "8")
    _set(m, "Player_Weapon_Default/ZoomLevel[4]", "RightOffset", "0.8")
    for sec in WALK_RUN:
        _set(m, f"{sec}/ZoomLevel[4]", "RightOffset", "0.8")
        if sec.startswith("Player_Basic"):
            _set(m, f"{sec}/ZoomLevel[2]", "RightOffset", "0.5")
    _set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "UpOffset", "-0.2")
    _set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "InDoorUpOffset", "-0.2")
    _set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "ZoomDistance", "2.5")
    _set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[3]", "UpOffset", "-0.2")
    _merge(m, build_lockon_distances(2.5, 5, 8))
    return m


def build_panoramic() -> dict:
    m: dict = {}
    for sec in ALL_MAIN:
        _set(m, f"{sec}/ZoomLevel[2]", "UpOffset", "0.0")
        _set(m, f"{sec}/ZoomLevel[2]", "ZoomDistance", "3.75")
        _set(m, f"{sec}/ZoomLevel[3]", "UpOffset", "0.0")
        _set(m, f"{sec}/ZoomLevel[3]", "ZoomDistance", "7.5")
        _set(m, f"{sec}/ZoomLevel[4]", "UpOffset", "0.0")
        _set(m, f"{sec}/ZoomLevel[4]", "ZoomDistance", "11.25")
    _set(m, "Player_Weapon_Default/ZoomLevel[4]", "RightOffset", "0.8")
    for sec in WALK_RUN:
        _set(m, f"{sec}/ZoomLevel[4]", "RightOffset", "0.8")
        if not sec.startswith("Player_Weapon"):
            _set(m, f"{sec}/ZoomLevel[2]", "RightOffset", "0.5")
    _set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "UpOffset", "0.0")
    _set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "ZoomDistance", "3.75")
    _set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[3]", "UpOffset", "0.0")
    _merge(m, build_mount_distances(1.2))
    _merge(m, build_lockon_distances(3.75, 7.5, 11.25))
    return m


def build_close_up() -> dict:
    m: dict = {}
    for sec in ALL_MAIN:
        _set(m, f"{sec}/ZoomLevel[2]", "UpOffset", "-0.2")
        _set(m, f"{sec}/ZoomLevel[2]", "InDoorUpOffset", "-0.2")
        _set(m, f"{sec}/ZoomLevel[2]", "ZoomDistance", "2.0")
        _set(m, f"{sec}/ZoomLevel[3]", "UpOffset", "-0.2")
        _set(m, f"{sec}/ZoomLevel[3]", "ZoomDistance", "4.0")
        _set(m, f"{sec}/ZoomLevel[4]", "UpOffset", "-0.2")
        _set(m, f"{sec}/ZoomLevel[4]", "RightOffset", "0.8")
        _set(m, f"{sec}/ZoomLevel[4]", "ZoomDistance", "6.0")
    for sec in WALK_RUN:
        _set(m, f"{sec}/ZoomLevel[2]", "RightOffset", "0.5")
    _set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "UpOffset", "-0.2")
    _set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "InDoorUpOffset", "-0.2")
    _set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "ZoomDistance", "2.0")
    _set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[3]", "UpOffset", "-0.2")
    _merge(m, build_mount_distances(0.75))
    _merge(m, build_lockon_distances(2.0, 4.0, 6.0))
    return m


def build_low_variant(base_up: str, indoor_up: str | None = None) -> dict:
    indoor_up = indoor_up or base_up
    m: dict = {}

    _set(m, "Player_Basic_Default/ZoomLevel[2]", "UpOffset", base_up)
    _set(m, "Player_Basic_Default/ZoomLevel[2]", "InDoorUpOffset", base_up)
    _set(m, "Player_Basic_Default/ZoomLevel[2]", "ZoomDistance", "2.5")
    _set(m, "Player_Basic_Default/ZoomLevel[3]", "UpOffset", base_up)
    _set(m, "Player_Basic_Default/ZoomLevel[3]", "InDoorUpOffset", indoor_up)
    _set(m, "Player_Basic_Default/ZoomLevel[3]", "ZoomDistance", "5")
    _set(m, "Player_Basic_Default/ZoomLevel[4]", "UpOffset", base_up)
    _set(m, "Player_Basic_Default/ZoomLevel[4]", "InDoorUpOffset", base_up)
    _set(m, "Player_Basic_Default/ZoomLevel[4]", "ZoomDistance", "8")

    for sec in ("Player_Basic_Default_Walk", "Player_Basic_Default_Run", "Player_Basic_Default_Runfast"):
        _set(m, f"{sec}/ZoomLevel[2]", "UpOffset", base_up)
        _set(m, f"{sec}/ZoomLevel[2]", "InDoorUpOffset", base_up)
        _set(m, f"{sec}/ZoomLevel[2]", "RightOffset", "0.5")
        _set(m, f"{sec}/ZoomLevel[2]", "ZoomDistance", "2.5")
        _set(m, f"{sec}/ZoomLevel[3]", "UpOffset", base_up)
        _set(m, f"{sec}/ZoomLevel[3]", "InDoorUpOffset", indoor_up)
        _set(m, f"{sec}/ZoomLevel[3]", "ZoomDistance", "5")
        _set(m, f"{sec}/ZoomLevel[4]", "UpOffset", base_up)
        _set(m, f"{sec}/ZoomLevel[4]", "InDoorUpOffset", base_up)
        _set(m, f"{sec}/ZoomLevel[4]", "RightOffset", "0.8000")
        _set(m, f"{sec}/ZoomLevel[4]", "ZoomDistance", "8")

    _set(m, "Player_Weapon_Default/ZoomLevel[2]", "UpOffset", base_up)
    _set(m, "Player_Weapon_Default/ZoomLevel[2]", "InDoorUpOffset", base_up)
    _set(m, "Player_Weapon_Default/ZoomLevel[2]", "ZoomDistance", "2.5")
    _set(m, "Player_Weapon_Default/ZoomLevel[3]", "UpOffset", base_up)
    _set(m, "Player_Weapon_Default/ZoomLevel[3]", "InDoorUpOffset", indoor_up)
    _set(m, "Player_Weapon_Default/ZoomLevel[3]", "ZoomDistance", "5")
    _set(m, "Player_Weapon_Default/ZoomLevel[4]", "UpOffset", base_up)
    _set(m, "Player_Weapon_Default/ZoomLevel[4]", "InDoorUpOffset", base_up)
    _set(m, "Player_Weapon_Default/ZoomLevel[4]", "RightOffset", "0.8000")
    _set(m, "Player_Weapon_Default/ZoomLevel[4]", "ZoomDistance", "8")

    for sec in ("Player_Weapon_Default_Walk", "Player_Weapon_Default_Run",
                "Player_Weapon_Default_RunFast", "Player_Weapon_Default_RunFast_Follow"):
        _set(m, f"{sec}/ZoomLevel[2]", "UpOffset", base_up)
        _set(m, f"{sec}/ZoomLevel[2]", "InDoorUpOffset", base_up)
        _set(m, f"{sec}/ZoomLevel[2]", "ZoomDistance", "2.5")
        _set(m, f"{sec}/ZoomLevel[3]", "UpOffset", base_up)
        _set(m, f"{sec}/ZoomLevel[3]", "InDoorUpOffset", indoor_up)
        _set(m, f"{sec}/ZoomLevel[3]", "ZoomDistance", "5")
        _set(m, f"{sec}/ZoomLevel[4]", "UpOffset", base_up)
        _set(m, f"{sec}/ZoomLevel[4]", "InDoorUpOffset", base_up)
        _set(m, f"{sec}/ZoomLevel[4]", "RightOffset", "0.8000")
        _set(m, f"{sec}/ZoomLevel[4]", "ZoomDistance", "8")

    for sec in ("Player_Weapon_Guard", "Player_Weapon_Rush"):
        _set(m, f"{sec}/ZoomLevel[2]", "UpOffset", base_up)
        _set(m, f"{sec}/ZoomLevel[2]", "InDoorUpOffset", base_up)
        _set(m, f"{sec}/ZoomLevel[2]", "ZoomDistance", "2.5")
        _set(m, f"{sec}/ZoomLevel[3]", "UpOffset", base_up)
        _set(m, f"{sec}/ZoomLevel[3]", "InDoorUpOffset", indoor_up)
        _set(m, f"{sec}/ZoomLevel[3]", "ZoomDistance", "5")
        _set(m, f"{sec}/ZoomLevel[4]", "UpOffset", base_up)
        _set(m, f"{sec}/ZoomLevel[4]", "InDoorUpOffset", base_up)
        _set(m, f"{sec}/ZoomLevel[4]", "RightOffset", "0.8000")
        _set(m, f"{sec}/ZoomLevel[4]", "ZoomDistance", "8")

    _set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "UpOffset", base_up)
    _set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "InDoorUpOffset", base_up)
    _set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "ZoomDistance", "2.5")
    _set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[3]", "UpOffset", base_up)
    _set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[3]", "InDoorUpOffset", indoor_up)
    _merge(m, build_lockon_distances(2.5, 5, 8))
    return m


def build_survival() -> dict:
    m: dict = {}
    for sec in ALL_MAIN:
        _set(m, f"{sec}/ZoomLevel[2]", "ZoomDistance", "1.8")
        _set(m, f"{sec}/ZoomLevel[3]", "ZoomDistance", "3")
        _set(m, f"{sec}/ZoomLevel[4]", "ZoomDistance", "6")
    for sec in WALK_RUN:
        _set(m, f"{sec}/ZoomLevel[2]", "RightOffset", "0.4")
        _set(m, f"{sec}/ZoomLevel[3]", "RightOffset", "0.7")
    for sec in list(WALK_RUN) + ["Player_Weapon_Default"]:
        _set(m, f"{sec}/ZoomLevel[4]", "RightOffset", "0.7000")
    _set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "ZoomDistance", "1.8")
    _set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[3]", "ZoomDistance", "3")
    _merge(m, build_mount_distances(0.65))
    _merge(m, build_lockon_distances(1.8, 3, 6))
    return m


def build_bane_mods() -> dict:
    m: dict = {}
    indoor_right_zl2 = {"Player_Basic_Default", "Player_Weapon_Default"}

    for sec in BANE_SECTIONS:
        for lvl in (2, 3, 4):
            key = f"{sec}/ZoomLevel[{lvl}]"
            _set(m, key, "RightOffset", "0.0")
            if lvl == 2 and sec in indoor_right_zl2:
                _set(m, key, "InDoorRightOffset", "0.0")
            if lvl == 4 and sec == "Player_Basic_Default_Runfast":
                _set(m, key, "InDoorRightOffset", "0.0")

    bane_zero_right_keys = [
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
        "Player_Weapon_Guard/ZoomLevel[2]", "Player_Weapon_Guard/ZoomLevel[3]", "Player_Weapon_Guard/ZoomLevel[4]",
        "Player_Weapon_Rush/ZoomLevel[2]", "Player_Weapon_Rush/ZoomLevel[3]", "Player_Weapon_Rush/ZoomLevel[4]",
        "Player_Weapon_Down/ZoomLevel[2]", "Player_Weapon_Down/ZoomLevel[3]", "Player_Weapon_Down/ZoomLevel[4]",
        "Player_Weapon_Indoor/ZoomLevel[3]",
        "Player_Weapon_Zoom_Out/ZoomLevel[2]", "Player_Weapon_Zoom_Out/ZoomLevel[3]",
        "Player_Weapon_LockOn_System/ZoomLevel[2]", "Player_Weapon_LockOn_System/ZoomLevel[3]", "Player_Weapon_LockOn_System/ZoomLevel[4]",
        "Player_Revive_LockOn_System/ZoomLevel[2]", "Player_Revive_LockOn_System/ZoomLevel[3]", "Player_Revive_LockOn_System/ZoomLevel[4]",
        "Cinematic_LockOn/ZoomLevel[2]", "Cinematic_LockOn/ZoomLevel[3]", "Cinematic_LockOn/ZoomLevel[4]",
        "Player_Hit_Throw/ZoomLevel[2]", "Player_Hit_Throw/ZoomLevel[3]", "Player_Hit_Throw/ZoomLevel[4]",
        "Player_Rest/ZoomLevel[2]", "Player_Rest/ZoomLevel[3]", "Player_Rest/ZoomLevel[4]",
        "Player_Basic_NoZoom/ZoomLevel[3]", "Player_Basic_NoZoom/ZoomLevel[4]",
        "Player_Basic_Teleport/ZoomLevel[2]",
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
    ]
    for key in bane_zero_right_keys:
        _set(m, key, "RightOffset", "0.0")

    for sec in ("Player_Animal_Default", "Player_Animal_Default_Walk",
                "Player_Animal_Default_Run", "Player_Animal_Default_Runfast"):
        for lvl in (2, 3, 4):
            _set(m, f"{sec}/ZoomLevel[{lvl}]", "RightOffset", "0.0")

    # Aim sections with positive offsets
    _set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "RightOffset", "0.50")
    _set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[3]", "RightOffset", "0.60")
    _set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[4]", "RightOffset", "0.60")
    _set(m, "Player_Taeguk_Aim/ZoomLevel[2]", "RightOffset", "0.50")
    _set(m, "Player_Taeguk_Aim/ZoomLevel[3]", "RightOffset", "0.60")
    _set(m, "Player_Weapon_Aim_Zoom/ZoomLevel[2]", "RightOffset", "0.80")
    _set(m, "Player_Weapon_Aim_Zoom/ZoomLevel[3]", "RightOffset", "0.90")
    _set(m, "Player_Weapon_Zoom/ZoomLevel[2]", "RightOffset", "0.68")
    _set(m, "Player_Weapon_Zoom/ZoomLevel[3]", "RightOffset", "0.60")
    _set(m, "Player_Weapon_Zoom/ZoomLevel[4]", "RightOffset", "0.90")
    _set(m, "Player_Weapon_Zoom_Light/ZoomLevel[2]", "RightOffset", "0.68")
    _set(m, "Player_Weapon_Zoom_Light/ZoomLevel[3]", "RightOffset", "0.68")
    return m


def build_combat_pullback(zl2: float, zl3: float, zl4: float, offset: float) -> dict:
    m: dict = {}
    if offset == 0:
        return m
    f = 1.0 + offset
    zl2s = fmt_num(round(zl2 * f, 1))
    zl3s = fmt_num(round(zl3 * f, 1))
    zl4s = fmt_num(round(zl4 * f, 1))

    for sec in LOCKON_SECTIONS:
        _set(m, f"{sec}/ZoomLevel[2]", "ZoomDistance", zl2s)
        _set(m, f"{sec}/ZoomLevel[3]", "ZoomDistance", zl3s)
        _set(m, f"{sec}/ZoomLevel[4]", "ZoomDistance", zl4s)
    for sec in ("Player_Weapon_Guard", "Player_Weapon_Rush"):
        _set(m, f"{sec}/ZoomLevel[2]", "ZoomDistance", zl2s)
        _set(m, f"{sec}/ZoomLevel[3]", "ZoomDistance", zl3s)
        _set(m, f"{sec}/ZoomLevel[4]", "ZoomDistance", zl4s)

    force_dist = fmt_num(round(zl3 * f * 2.0, 1))
    for lvl in (2, 3, 4):
        _set(m, f"Player_Force_LockOn/ZoomLevel[{lvl}]", "ZoomDistance", force_dist)
    for lvl in (1, 2, 3, 4):
        _set(m, f"Player_LockOn_Titan/ZoomLevel[{lvl}]", "ZoomDistance", force_dist)
    return m


_STYLE_UP_OFFSET: dict[str, float] = {
    "default": 0.0,
    "heroic": -0.2,
    "panoramic": 0.0,
    "close-up": -0.2,
    "low-rider": -0.8,
    "knee-cam": -1.2,
    "dirt-cam": -1.5,
    "survival": 0.0,
}


def _get_style_builder(style: str):
    if style == "heroic":
        return build_heroic
    if style == "panoramic":
        return build_panoramic
    if style == "close-up":
        return build_close_up
    if style == "low-rider":
        return lambda: build_low_variant("-0.8")
    if style == "knee-cam":
        return lambda: build_low_variant("-1.2")
    if style == "dirt-cam":
        return lambda: build_low_variant("-1.5")
    if style == "survival":
        return build_survival
    return None


# Mount/aim baselines for BuildCustom
_HORSE_MOUNT_BASELINES = [(sec, 2, 1.45) for sec in HORSE_RIDE_SECTIONS] + \
                        [(sec, 3, 1.8) for sec in HORSE_RIDE_SECTIONS] + [
    ("Player_Ride_Elephant", 2, 1.3),
    ("Player_Ride_Elephant", 3, 1.6),
    ("Player_Ride_Wyvern", 2, 3.0),
    ("Player_Ride_Wyvern", 3, 4.0),
    ("Player_Ride_Wyvern", 4, 5.0),
    ("Player_Ride_Canoe", 2, 0.9),
    ("Player_Ride_Canoe", 3, 1.1),
    ("Player_Ride_Warmachine", 2, 2.4),
    ("Player_Ride_Warmachine", 3, 2.8),
    ("Player_Ride_Warmachine_Aim", 2, 1.8),
    ("Player_Ride_Warmachine_Dash", 2, 0.8),
    ("Player_Ride_Broom", 2, 0.3),
    ("Player_Ride_Broom", 3, 0.4),
]

_AIM_INTERACTION_BASELINES = [
    ("Player_Basic_Default_Aim_Zoom", 2, VANILLA_RO_ZL2),
    ("Player_Basic_Default_Aim_Zoom", 3, VANILLA_RO_ZL3),
    ("Player_Basic_Default_Aim_Zoom", 4, VANILLA_RO_ZL4),
    ("Player_Taeguk_Aim", 2, VANILLA_RO_ZL2),
    ("Player_Taeguk_Aim", 3, VANILLA_RO_ZL3),
    ("Player_Weapon_Aim_Zoom", 2, VANILLA_RO_ZL3),
    ("Player_Weapon_Aim_Zoom", 3, VANILLA_RO_ZL4),
    ("Player_Weapon_Zoom", 2, VANILLA_RO_ZL3),
    ("Player_Weapon_Zoom", 3, VANILLA_RO_ZL3),
    ("Player_Weapon_Zoom", 4, VANILLA_RO_ZL4),
    ("Player_Weapon_Zoom_Light", 2, VANILLA_RO_ZL3),
    ("Player_Weapon_Zoom_Light", 3, VANILLA_RO_ZL3),
    ("Player_Weapon_Zoom_Out", 2, VANILLA_RO_ZL3),
    ("Player_Weapon_Zoom_Out", 3, VANILLA_RO_ZL3),
    ("Player_Bow_Aim_Zoom_Start", 2, VANILLA_RO_ZL3),
    ("Player_Bow_Aim_Zoom_Ing", 2, VANILLA_RO_ZL3),
    ("Player_Bow_Aim_Zoom", 2, VANILLA_RO_ZL3),
    ("Player_Bow_Aim_LockOn", 2, VANILLA_RO_ZL3),
    ("Player_Ride_Aim_Zoom", 2, VANILLA_RO_ZL3),
    ("Player_Interaction_LockOn", 2, VANILLA_RO_ZL3),
    ("Interaction_LookAt", 2, VANILLA_RO_ZL3),
    ("Glide_Kick_Aim_Zoom", 2, VANILLA_RO_ZL4),
    ("Glide_Bow_Aim_Zoom", 2, VANILLA_RO_ZL4),
    ("Player_Basic_FreeFall_Aim", 2, VANILLA_RO_ZL3),
    ("Player_Basic_FreeFall_Aim", 3, VANILLA_RO_ZL3),
    ("Player_Tool_Aim_Melee", 2, VANILLA_RO_ZL2),
    ("Player_Throw_Aim", 2, VANILLA_RO_ZL2),
    ("Player_Weapon_Aim_BossAttack", 2, VANILLA_RO_ZL2),
    ("Player_Weapon_Aim_BossAttack", 3, VANILLA_RO_ZL3),
    ("Player_Weapon_Aim_SmallBossAttack", 2, VANILLA_RO_ZL2),
    ("Player_Weapon_Aim_SmallBossAttack", 3, VANILLA_RO_ZL2),
]


def build_custom(distance: float, up_offset: float, right_offset: float) -> dict:
    zl2_dist = round(distance * 0.5, 1)
    zl3_dist = round(distance, 1)
    zl4_dist = round(distance * 1.5, 1)
    up_str = fmt_f2(up_offset)

    factor = 1.0 + (-right_offset) / VANILLA_RO_ZL2
    ro_zl2 = fmt_f2(VANILLA_RO_ZL2 * factor)
    ro_zl3 = fmt_f2(VANILLA_RO_ZL3 * factor)
    ro_zl4 = fmt_f2(VANILLA_RO_ZL4 * factor)

    m: dict = {}
    for sec in ALL_MAIN:
        _set(m, f"{sec}/ZoomLevel[2]", "UpOffset", up_str)
        _set(m, f"{sec}/ZoomLevel[2]", "InDoorUpOffset", up_str)
        _set(m, f"{sec}/ZoomLevel[2]", "RightOffset", ro_zl2)
        _set(m, f"{sec}/ZoomLevel[2]", "ZoomDistance", fmt_num(zl2_dist))
        _set(m, f"{sec}/ZoomLevel[3]", "UpOffset", up_str)
        _set(m, f"{sec}/ZoomLevel[3]", "InDoorUpOffset", up_str)
        _set(m, f"{sec}/ZoomLevel[3]", "RightOffset", ro_zl3)
        _set(m, f"{sec}/ZoomLevel[3]", "ZoomDistance", fmt_num(zl3_dist))
        _set(m, f"{sec}/ZoomLevel[4]", "UpOffset", up_str)
        _set(m, f"{sec}/ZoomLevel[4]", "InDoorUpOffset", up_str)
        _set(m, f"{sec}/ZoomLevel[4]", "RightOffset", ro_zl4)
        _set(m, f"{sec}/ZoomLevel[4]", "ZoomDistance", fmt_num(zl4_dist))

    _set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "UpOffset", up_str)
    _set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "InDoorUpOffset", up_str)
    _set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[2]", "ZoomDistance", fmt_num(zl2_dist))
    _set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[3]", "UpOffset", up_str)
    _set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[3]", "InDoorUpOffset", up_str)
    _set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[3]", "ZoomDistance", fmt_num(zl3_dist))
    _set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[4]", "UpOffset", up_str)
    _set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[4]", "InDoorUpOffset", up_str)
    _set(m, "Player_Basic_Default_Aim_Zoom/ZoomLevel[4]", "ZoomDistance", fmt_num(zl4_dist))

    for sec, zl, vanilla in _HORSE_MOUNT_BASELINES:
        _set(m, f"{sec}/ZoomLevel[{zl}]", "RightOffset", fmt_f2(vanilla * factor))

    horse_zl2_dist = round(distance * 1.5, 1)
    horse_zl3_dist = round(distance * 2.1, 1)
    for sec in HORSE_RIDE_SECTIONS:
        _set(m, f"{sec}/ZoomLevel[2]", "ZoomDistance", fmt_num(horse_zl2_dist))
        _set(m, f"{sec}/ZoomLevel[3]", "ZoomDistance", fmt_num(horse_zl3_dist))

    for sec, zl, vanilla in _AIM_INTERACTION_BASELINES:
        _set(m, f"{sec}/ZoomLevel[{zl}]", "RightOffset", fmt_f2(vanilla * factor))

    _merge(m, build_lockon_distances(zl2_dist, zl3_dist, zl4_dist))
    return m


def build_modifications(
    style: str,
    fov: int,
    bane: bool,
    *,
    combat_pullback: float = 0.0,
    mount_height: bool = False,
    custom_up: float | None = None,
    steadycam: bool = True,
    lockon_auto_rotate: bool = True,
    custom_params: tuple[float, float, float] | None = None,
) -> ModificationSet:
    mods: dict = {}

    _merge(mods, build_shared_base())

    if steadycam:
        _merge(mods, build_smoothing())
        _merge(mods, build_shared_steadycam())

    _merge(mods, build_lockon_distances(3.4, 6, 8))

    if style == "custom" and custom_params is not None:
        dist, up, ro = custom_params
        _merge(mods, build_custom(dist, up, ro))
    else:
        builder = _get_style_builder(style)
        if builder is not None:
            _merge(mods, builder())

    if bane:
        _merge(mods, build_bane_mods())

    if combat_pullback != 0:
        def _get_zl(level: int, default: float) -> float:
            key = f"Player_Basic_Default/ZoomLevel[{level}]"
            if key in mods and "ZoomDistance" in mods[key]:
                try:
                    return float(mods[key]["ZoomDistance"][1])
                except ValueError:
                    return default
            return default

        zl2v = _get_zl(2, 3.4)
        zl3v = _get_zl(3, 6.0)
        zl4v = _get_zl(4, 8.0)
        _merge(mods, build_combat_pullback(zl2v, zl3v, zl4v, combat_pullback))

    if mount_height:
        if custom_up is not None:
            up = custom_up
        elif style == "custom" and custom_params is not None:
            up = custom_params[1]
        else:
            up = _STYLE_UP_OFFSET.get(style, 0.0)
        _merge(mods, build_mount_height_mods(up))

    if not lockon_auto_rotate:
        ar: dict = {}
        _set(ar, "Player_Weapon_LockOn", "IsAutoRotate", "false")
        _set(ar, "Player_Weapon_LockOn_System", "IsAutoRotate", "false")
        _set(ar, "Player_Weapon_TwoTarget", "IsTargetFixed", "false")
        _merge(mods, ar)

    return mods, fov


# ═══════════════════════════════════════════════════════════════════════════════
# Pipeline — install / extract / restore
# ═══════════════════════════════════════════════════════════════════════════════


SCRIPT_DIR = Path(__file__).resolve().parent


def _default_backups_dir() -> Path:
    """XDG-compliant default for vanilla backups.

    Falls back to ~/.local/share/ultimate_camera_mod if XDG_DATA_HOME is unset.
    """
    xdg = os.environ.get("XDG_DATA_HOME")
    base = Path(xdg) if xdg else Path.home() / ".local" / "share"
    return base / "ultimate_camera_mod"


BACKUPS_DIR = _default_backups_dir()


def _log(msg: str) -> None:
    print(f"[ucm] {msg}")


def find_camera_entry(game_dir: str) -> PazEntry:
    pamt_path = os.path.join(game_dir, "0010", "0.pamt")
    paz_dir = os.path.join(game_dir, "0010")
    if not os.path.exists(pamt_path):
        raise FileNotFoundError(
            f"Game archive index not found at:\n  {pamt_path}\n"
            "Select the correct Crimson Desert install folder (must contain 0010/0.paz)."
        )
    entries = pamt_parse(pamt_path, paz_dir)
    for e in entries:
        if "playercamerapreset.xml" in e.path:
            return e
    raise RuntimeError(
        "playercamerapreset.xml was not found in the game archive. "
        "Try verifying game files on Steam."
    )


def read_entry_bytes(entry: PazEntry) -> bytes:
    with open(entry.paz_file, "rb") as f:
        f.seek(entry.offset)
        return f.read(entry.comp_size)


def decode_entry_xml(entry: PazEntry, raw_bytes: bytes) -> str:
    dec = asset_decode(raw_bytes, os.path.basename(entry.path))
    plain = lz4_decompress(dec, entry.orig_size)
    return plain.decode("utf-8").rstrip("\x00")


def read_live_xml(game_dir: str) -> str:
    entry = find_camera_entry(game_dir)
    raw = read_entry_bytes(entry)
    return decode_entry_xml(entry, raw)


def _validate_vanilla(xml_text: str) -> bool:
    # Heuristic: if UCM has already touched the file, refuse to back it up.
    m1 = re.search(r'<Player_Basic_Default_Run\s+[^>]*?Fov="(\d+)"', xml_text)
    if m1 and m1.group(1) == "40":
        return False
    m2 = re.search(r'<Player_Basic_Default_Runfast\s+[^>]*?Fov="(\d+)"', xml_text)
    if m2 and m2.group(1) == "40":
        return False
    if re.search(
        r'<Player_Basic_Default_Run\s+[^>]*?>[\s\S]*?<OffsetByVelocity[^>]*?OffsetLength="0"',
        xml_text,
    ):
        return False
    return True


def ensure_backup(entry: PazEntry) -> None:
    backup_path = BACKUPS_DIR / "original_backup.bin"
    meta_path = BACKUPS_DIR / "backup_meta.txt"

    if backup_path.exists() and meta_path.exists():
        meta = meta_path.read_text()
        if f"comp_size={entry.comp_size}" in meta and "vanilla_verified" in meta:
            return

    BACKUPS_DIR.mkdir(parents=True, exist_ok=True)
    _log("Capturing vanilla backup from live PAZ...")
    data = read_entry_bytes(entry)

    try:
        dec = asset_decode(data, "playercamerapreset.xml")
        xml_bytes = lz4_decompress(dec, entry.orig_size)
        xml_text = xml_bytes.decode("utf-8").rstrip("\x00")
    except Exception as exc:
        raise RuntimeError(
            "Could not decode camera data from the game archive. The file may be "
            "corrupted or from a different game version. Verify game files on Steam."
        ) from exc

    if not _validate_vanilla(xml_text):
        raise RuntimeError(
            "Game camera files are not vanilla — they look already modified.\n"
            "TO FIX:\n"
            "  1. Delete your game's 0010/0.paz\n"
            "  2. Steam → Crimson Desert → Properties → Installed Files → "
            "\"Verify integrity of game files\"\n"
            "  3. Re-run this script."
        )

    backup_path.write_bytes(data)
    meta_path.write_text(
        f"comp_size={entry.comp_size} orig_size={entry.orig_size} vanilla_verified"
    )
    _log(f"Backup saved ({entry.comp_size} bytes) → {backup_path}")


def get_vanilla_xml(entry: PazEntry) -> str:
    backup_path = BACKUPS_DIR / "original_backup.bin"
    raw = backup_path.read_bytes()
    dec = asset_decode(raw, "playercamerapreset.xml")
    xml_bytes = lz4_decompress(dec, entry.orig_size)
    return xml_bytes.decode("utf-8").rstrip("\x00")


def install_with_mod_set(game_dir: str, mod_set: ModificationSet) -> None:
    _log("Finding camera entry...")
    entry = find_camera_entry(game_dir)
    _log(f"Found: offset={entry.offset}, comp_size={entry.comp_size}, orig_size={entry.orig_size}")

    ensure_backup(entry)

    _log("Reading vanilla XML from backup...")
    vanilla_xml = strip_comments(get_vanilla_xml(entry))

    _log("Applying modifications...")
    modified_xml = apply_modifications(vanilla_xml, mod_set)

    try:
        debug_path = BACKUPS_DIR / "debug_modified.xml"
        debug_path.write_text(modified_xml)
        _log(f"Debug XML: {debug_path}")
    except Exception:
        pass

    _log("Encoding and size-matching...")
    xml_bytes = b"\xef\xbb\xbf" + modified_xml.encode("utf-8")  # UTF-8 BOM
    matched = match_compressed_size(xml_bytes, entry.comp_size, entry.orig_size)

    _log("Compressing...")
    compressed = lz4_compress(matched)
    if len(compressed) != entry.comp_size:
        raise RuntimeError(
            f"Size mismatch after compression: {len(compressed)} != {entry.comp_size}"
        )

    _log("Encoding with game cipher...")
    encoded = asset_encode(compressed, "playercamerapreset.xml")

    _log("Writing into game PAZ...")
    update_entry(entry, encoded)

    _log("Done! Camera preset installed.")


def restore_camera(game_dir: str) -> int:
    entry = find_camera_entry(game_dir)
    backup_path = BACKUPS_DIR / "original_backup.bin"

    if not backup_path.exists():
        _log("No backup found — camera may already be vanilla (or never installed by ucm).")
        return 1

    data = backup_path.read_bytes()
    if len(data) != entry.comp_size:
        _log("Backup size mismatch — game may have updated. Deleting stale backup.")
        backup_path.unlink()
        (BACKUPS_DIR / "backup_meta.txt").unlink(missing_ok=True)
        return 2

    _log("Restoring vanilla camera...")
    update_entry(entry, data)
    _log("Done! Camera restored.")
    return 0


def extract_xml(game_dir: str, out_path: str) -> None:
    xml = read_live_xml(game_dir)
    Path(out_path).write_text(xml)
    _log(f"Wrote live camera XML → {out_path}")


# ═══════════════════════════════════════════════════════════════════════════════
# Preset loading + Steam auto-detect
# ═══════════════════════════════════════════════════════════════════════════════


def _resolve_preset_dir() -> Path:
    """Find the official ucm_presets/ folder.

    Supports the script living either at the repo root (legacy) or under a
    `linux/` subdirectory (current layout). Walks up a few parents looking
    for the first `ucm_presets/` directory.
    """
    for candidate in (
        SCRIPT_DIR / "ucm_presets",
        SCRIPT_DIR.parent / "ucm_presets",
        SCRIPT_DIR.parent.parent / "ucm_presets",
    ):
        if candidate.is_dir():
            return candidate
    # Fall back to script-local even if missing, so errors mention a sane path.
    return SCRIPT_DIR / "ucm_presets"


PRESET_DIR = _resolve_preset_dir()


def list_builtin_presets() -> list[Path]:
    if not PRESET_DIR.exists():
        return []
    return sorted(PRESET_DIR.glob("*.ucmpreset"))


def load_preset(name_or_path: str) -> dict:
    """Accept a preset name (case/space tolerant) or an explicit path."""
    path = Path(name_or_path)
    if path.exists():
        return json.loads(path.read_text())

    # Match against built-in presets
    needle = name_or_path.lower().replace("-", "").replace(" ", "").replace("_", "")
    for candidate in list_builtin_presets():
        stem = candidate.stem.lower().replace("-", "").replace(" ", "").replace("_", "")
        if stem == needle:
            return json.loads(candidate.read_text())

    # Try style_id match
    for candidate in list_builtin_presets():
        try:
            data = json.loads(candidate.read_text())
            sid = str(data.get("style_id", "")).lower().replace("-", "").replace(" ", "")
            if sid == needle:
                return data
        except Exception:
            continue

    raise FileNotFoundError(f"Preset not found: {name_or_path}")


def auto_detect_game_dir() -> str | None:
    # Check Steam library folders (flatpak + native + snap paths).
    candidates = [
        Path.home() / ".steam/steam/steamapps/libraryfolders.vdf",
        Path.home() / ".local/share/Steam/steamapps/libraryfolders.vdf",
        Path.home() / ".var/app/com.valvesoftware.Steam/data/Steam/steamapps/libraryfolders.vdf",
    ]
    for vdf in candidates:
        if not vdf.exists():
            continue
        try:
            text = vdf.read_text(errors="ignore")
        except Exception:
            continue
        for m in re.finditer(r'"path"\s+"([^"]+)"', text):
            lib = Path(m.group(1))
            game = lib / "steamapps" / "common" / "Crimson Desert"
            if (game / "0010" / "0.pamt").exists():
                return str(game)
    # Final fallback: conventional path
    fallback = Path.home() / ".steam/steam/steamapps/common/Crimson Desert"
    if (fallback / "0010" / "0.pamt").exists():
        return str(fallback)
    return None


# ═══════════════════════════════════════════════════════════════════════════════
# CLI
# ═══════════════════════════════════════════════════════════════════════════════


def _resolve_game_dir(args: argparse.Namespace) -> str:
    if args.game_dir:
        return args.game_dir
    auto = auto_detect_game_dir()
    if auto is None:
        sys.stderr.write(
            "ERROR: could not auto-detect Crimson Desert install.\n"
            "Pass --game-dir /path/to/Crimson\\ Desert\n"
        )
        sys.exit(2)
    _log(f"Auto-detected game at: {auto}")
    return auto


def _cmd_list(args: argparse.Namespace) -> int:
    presets = list_builtin_presets()
    if not presets:
        print("No presets found in", PRESET_DIR)
        return 1
    print("Available presets:")
    for p in presets:
        try:
            data = json.loads(p.read_text())
            desc = data.get("description", "")
            sid = data.get("style_id", "?")
            print(f"  {p.stem:<15} [{sid:<10}] {desc}")
        except Exception:
            print(f"  {p.stem}")
    return 0


def _cmd_extract(args: argparse.Namespace) -> int:
    game_dir = _resolve_game_dir(args)
    extract_xml(game_dir, args.out)
    return 0


def _cmd_restore(args: argparse.Namespace) -> int:
    game_dir = _resolve_game_dir(args)
    return restore_camera(game_dir)


def _cmd_apply(args: argparse.Namespace) -> int:
    game_dir = _resolve_game_dir(args)

    style = args.style
    fov = args.fov
    bane = args.bane
    combat_pullback = args.combat_pullback
    mount_height = args.mount_height
    steadycam = args.steadycam
    custom_params: tuple[float, float, float] | None = None

    if args.preset:
        data = load_preset(args.preset)
        settings = data.get("settings", {})
        kind = data.get("kind", "style")
        if kind == "style":
            style = data.get("style_id", style) or "default"
            fov = int(settings.get("fov", 0))
            bane = bool(settings.get("centered", False))
            combat_pullback = float(settings.get("combat_pullback", 0.0))
            mount_height = bool(settings.get("mount_height", False))
            steadycam = bool(settings.get("steadycam", True))
            if "distance" in settings and "height" in settings:
                style = "custom"
                custom_params = (
                    float(settings["distance"]),
                    float(settings["height"]),
                    float(settings.get("right_offset", 0.0)),
                )
                # The FoV here is an absolute target (30/25). Translate to a delta
                # from vanilla base FOV (45) so FoV layering in apply_modifications works.
                # Negative values are allowed — we clamp to 0 since the engine only
                # supports positive additive FoV through this code path.
                abs_fov = int(settings.get("fov", 0))
                # Fov slider is additive; presets typically express as absolute "30"
                # but the engine treats fov as delta on top of 40 (post-base). So for
                # preset fov=30 the effective delta is (30 - 40) = -10, which the
                # engine clamps to 0. For preset fov=25, likewise. Users can tweak
                # via CLI for additional FoV.
                fov = max(0, abs_fov - 40) if abs_fov else 0

    if args.custom:
        style = "custom"
        custom_params = (args.distance, args.height, args.right_offset)

    mod_set = build_modifications(
        style=style or "default",
        fov=fov,
        bane=bane,
        combat_pullback=combat_pullback,
        mount_height=mount_height,
        custom_up=args.height if args.custom else None,
        steadycam=steadycam,
        custom_params=custom_params,
    )

    install_with_mod_set(game_dir, mod_set)
    return 0


def build_parser() -> argparse.ArgumentParser:
    p = argparse.ArgumentParser(
        prog="ucm",
        description="UltimateCameraMod — Linux CLI port for Crimson Desert",
    )
    p.add_argument("--game-dir", help="Path to Crimson Desert install folder")
    p.add_argument(
        "--backup-dir",
        help="Where to store/read the vanilla backup "
             "(default: $XDG_DATA_HOME/ultimate_camera_mod)",
    )
    sub = p.add_subparsers(dest="command", required=True)

    sub.add_parser("list", help="List built-in presets").set_defaults(func=_cmd_list)

    ext = sub.add_parser("extract", help="Dump live camera XML to a file")
    ext.add_argument("--out", default="playercamerapreset.xml")
    ext.set_defaults(func=_cmd_extract)

    rst = sub.add_parser("restore", help="Restore vanilla camera from backup")
    rst.set_defaults(func=_cmd_restore)

    ap = sub.add_parser("apply", help="Apply a camera preset")
    ap.add_argument("--preset", help="Preset name (e.g. 'panoramic') or path to .ucmpreset")
    ap.add_argument("--style", default="default",
                    choices=["default", "heroic", "panoramic", "close-up",
                             "low-rider", "knee-cam", "dirt-cam", "survival", "custom"])
    ap.add_argument("--fov", type=int, default=0, help="Additive FOV delta")
    ap.add_argument("--bane", action="store_true", help="Centered camera")
    ap.add_argument("--no-steadycam", dest="steadycam", action="store_false", default=True)
    ap.add_argument("--mount-height", action="store_true")
    ap.add_argument("--combat-pullback", type=float, default=0.0,
                    help="Proportional lock-on offset (+0.25 = 25%% further)")
    ap.add_argument("--custom", action="store_true", help="Use custom style (requires distance/height/right-offset)")
    ap.add_argument("--distance", type=float, default=6.0)
    ap.add_argument("--height", type=float, default=0.0)
    ap.add_argument("--right-offset", type=float, default=0.0)
    ap.set_defaults(func=_cmd_apply)

    return p


def main(argv: list[str] | None = None) -> int:
    global BACKUPS_DIR
    args = build_parser().parse_args(argv)
    if getattr(args, "backup_dir", None):
        BACKUPS_DIR = Path(args.backup_dir).expanduser().resolve()
    try:
        return args.func(args)
    except (FileNotFoundError, RuntimeError) as exc:
        sys.stderr.write(f"ERROR: {exc}\n")
        return 1


if __name__ == "__main__":
    sys.exit(main())

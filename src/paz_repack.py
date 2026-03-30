"""PAZ asset repacker for Crimson Desert.

Patches modified files back into PAZ archives. Handles encryption and
compression to produce output the game will accept.

Pipeline: modified file -> LZ4 compress -> ChaCha20 encrypt -> write to PAZ

Constraints:
  - Encrypted blob must be exactly comp_size bytes (original size in PAMT)
  - Decompressed output must be exactly orig_size bytes
  - PAMT files must never be modified (game integrity check)
  - NTFS timestamps on .paz files must be preserved

Usage:
    # Repack using PAMT metadata (recommended)
    python paz_repack.py modified.xml --pamt 0.pamt --paz-dir ./0003 \
        --entry "technique/rendererconfiguration.xml"

    # Repack to a standalone file (for testing)
    python paz_repack.py modified.xml --pamt 0.pamt --paz-dir ./0003 \
        --entry "technique/rendererconfiguration.xml" --output repacked.bin

Library usage:
    from paz_repack import repack_entry
    from paz_parse import parse_pamt

    entries = parse_pamt("0.pamt", paz_dir="./0003")
    entry = next(e for e in entries if "rendererconfiguration" in e.path)
    repack_entry("modified.xml", entry)
"""

import os
import sys
import struct
import ctypes
import argparse

import lz4.block

from paz_parse import parse_pamt, PazEntry
from paz_crypto import encrypt, lz4_compress


# ── Timestamp preservation (Windows) ────────────────────────────────

def _save_timestamps(path: str):
    """Capture NTFS timestamps. Returns a callable to restore them."""
    if sys.platform != 'win32':
        return lambda: None

    kernel32 = ctypes.WinDLL('kernel32', use_last_error=True)

    class FILETIME(ctypes.Structure):
        _fields_ = [("lo", ctypes.c_uint32), ("hi", ctypes.c_uint32)]

    OPEN_EXISTING = 3
    GENERIC_READ = 0x80000000
    GENERIC_WRITE = 0x40000000
    FILE_ATTR = 0x80 | 0x02000000  # NORMAL | BACKUP_SEMANTICS

    h = kernel32.CreateFileW(path, GENERIC_READ, 1, None, OPEN_EXISTING, FILE_ATTR, None)
    if h == -1:
        return lambda: None

    ct, at, mt = FILETIME(), FILETIME(), FILETIME()
    kernel32.GetFileTime(h, ctypes.byref(ct), ctypes.byref(at), ctypes.byref(mt))
    kernel32.CloseHandle(h)

    def restore():
        h2 = kernel32.CreateFileW(path, GENERIC_WRITE, 0, None, OPEN_EXISTING, FILE_ATTR, None)
        if h2 != -1:
            kernel32.SetFileTime(h2, ctypes.byref(ct), ctypes.byref(at), ctypes.byref(mt))
            kernel32.CloseHandle(h2)

    return restore


# ── Size matching ────────────────────────────────────────────────────

def _pad_to_orig_size(data: bytes, orig_size: int) -> bytes:
    """Pad data to exactly orig_size bytes with zero bytes."""
    if len(data) >= orig_size:
        return data[:orig_size]
    return data + b'\x00' * (orig_size - len(data))


def _shrink_to_orig_size(data: bytes, orig_size: int) -> bytes:
    """Shrink XML data to exactly orig_size by removing comment content
    and collapsing redundant whitespace.

    Removes bytes from the end of XML comments first (replacing the
    comment body with fewer characters). If that's not enough, collapses
    runs of multiple spaces/tabs into single spaces.

    Returns:
        data trimmed to exactly orig_size bytes

    Raises:
        ValueError if the data can't be shrunk enough
    """
    if len(data) <= orig_size:
        return _pad_to_orig_size(data, orig_size)

    excess = len(data) - orig_size
    result = bytearray(data)

    # Phase 1: trim comment bodies from the end (preserve <!-- -->)
    comments = _find_xml_comments(bytes(result))
    # Process largest comments first for maximum yield
    comments.sort(key=lambda c: c[1] - c[0], reverse=True)

    for cstart, cend in comments:
        if excess <= 0:
            break
        body_len = cend - cstart
        # Keep at least 1 space in the comment so it stays valid
        removable = body_len - 1
        if removable <= 0:
            continue
        to_remove = min(removable, excess)
        # Replace the end of the comment body with nothing
        result[cstart + 1:cstart + 1 + to_remove] = b''
        excess -= to_remove
        if excess <= 0:
            break
        # Recalculate comments since offsets shifted
        comments = _find_xml_comments(bytes(result))
        comments.sort(key=lambda c: c[1] - c[0], reverse=True)

    if excess <= 0:
        return bytes(result[:orig_size]) if len(result) >= orig_size else \
            bytes(result) + b'\x00' * (orig_size - len(result))

    # Phase 2: collapse runs of 2+ whitespace chars into single space
    i = len(result) - 1
    while i > 0 and excess > 0:
        if result[i] in (0x20, 0x09) and result[i - 1] in (0x20, 0x09):
            # Found consecutive whitespace, remove one
            del result[i]
            excess -= 1
        i -= 1

    if excess <= 0:
        return bytes(result[:orig_size]) if len(result) >= orig_size else \
            bytes(result) + b'\x00' * (orig_size - len(result))

    # Phase 3: remove entire empty comments (<!-- ... --> -> nothing)
    comments = _find_xml_comments(bytes(result))
    for cstart, cend in comments:
        if excess <= 0:
            break
        # Remove <!-- + body + -->  (4 + body_len + 3 bytes)
        full_start = cstart - 4
        full_end = cend + 3
        removable = full_end - full_start
        if removable <= excess + 7:  # worth removing the whole comment
            to_remove = min(removable, excess)
            # Just remove bytes from the comment
            result[full_start:full_start + to_remove] = b''
            excess -= to_remove
            if excess <= 0:
                break
            comments = _find_xml_comments(bytes(result))

    if len(result) > orig_size:
        raise ValueError(
            f"Modified file is {len(data) - orig_size} bytes over orig_size "
            f"({orig_size}). Could only trim {len(data) - len(result)} bytes "
            f"from comments and whitespace. Reduce content manually.")

    return bytes(result) + b'\x00' * (orig_size - len(result))


def _find_xml_comments(data: bytes) -> list[tuple[int, int]]:
    """Find all XML comment bodies (content between <!-- and -->).

    Returns list of (start, end) byte offsets for the comment content
    (not including the delimiters themselves).
    """
    comments = []
    search_from = 0
    while True:
        start = data.find(b'<!--', search_from)
        if start == -1:
            break
        content_start = start + 4
        end = data.find(b'-->', content_start)
        if end == -1:
            break
        if end > content_start:
            comments.append((content_start, end))
        search_from = end + 3
    return comments


def _make_xml_safe_incompressible(length: int) -> bytes:
    """Generate incompressible content safe inside an XML comment.

    Uses printable ASCII bytes excluding XML-special chars and '--' pairs.
    Valid UTF-8, valid XML comment content, and varied enough (88 distinct
    values) to be highly incompressible under LZ4.
    """
    # Printable ASCII (0x20-0x7E) minus XML-special: < > & and -
    # 0x20=space 0x2D='-' 0x3C='<' 0x3E='>' 0x26='&'
    _ALPHABET = bytes(
        c for c in range(0x20, 0x7F)
        if c not in (0x2D, 0x3C, 0x3E, 0x26)
    )  # 88 distinct values
    rand = os.urandom(length)
    return bytes(_ALPHABET[b % len(_ALPHABET)] for b in rand)


def _find_insertion_points(data: bytes) -> list[int]:
    """Return byte offsets of newlines in data — good places to insert comments."""
    return [i for i, b in enumerate(data) if b == 0x0A]


def _inflate_with_comments(padded: bytes, plaintext_len: int,
                           target_comp_size: int,
                           target_orig_size: int) -> bytes | None:
    """Inflate compressed size to exactly target_comp_size.

    Strategies tried in order:

    1. Replace zero bytes in the trailing padding with spaces (small deltas).

    2. Insert a single XML comment with incompressible content into the
       trailing padding. Binary-search body length.

    3. Distribute incompressible XML comments across multiple newline
       positions in the file body. Each comment is <!--BODY--> inserted
       after a newline. Binary-search total body bytes across N slots.
       This handles large deltas even when padding room is small.

    Returns adjusted plaintext (exactly target_orig_size bytes) or None.
    """
    padding_available = target_orig_size - plaintext_len

    base_comp = len(lz4.block.compress(padded, store_size=False))
    needed = target_comp_size - base_comp  # positive = need to add bytes

    if needed <= 0:
        return None

    # ── Strategy 1: replace zero bytes in padding with spaces ──────────
    if padding_available > 0:
        max_replaceable = padding_available

        def _build_zero_trial(n: int) -> bytes:
            trial = bytearray(padded)
            for i in range(n):
                trial[plaintext_len + i] = 0x20
            return bytes(trial)

        c_one = len(lz4.block.compress(_build_zero_trial(1), store_size=False))
        if c_one <= target_comp_size:
            lo, hi = 1, max_replaceable
            while lo <= hi:
                mid = (lo + hi) // 2
                c = len(lz4.block.compress(_build_zero_trial(mid), store_size=False))
                if c == target_comp_size:
                    return _build_zero_trial(mid)
                elif c < target_comp_size:
                    lo = mid + 1
                else:
                    hi = mid - 1
            for n in range(max(1, lo - 5), min(lo + 5, max_replaceable + 1)):
                trial = _build_zero_trial(n)
                if len(lz4.block.compress(trial, store_size=False)) == target_comp_size:
                    return trial

    # ── Strategy 2: single XML comment in trailing padding ─────────────
    if padding_available >= 8:
        max_body = padding_available - 7  # 7 = len("<!---->")
        rand_body = _make_xml_safe_incompressible(max_body)

        def _build_comment_trial(body_len: int) -> bytes:
            body = rand_body[:body_len]
            comment = b'<!--' + body + b'-->'
            trial = padded[:plaintext_len] + comment
            if len(trial) < target_orig_size:
                trial = trial + b'\x00' * (target_orig_size - len(trial))
            else:
                trial = trial[:target_orig_size]
            return trial

        c_min = len(lz4.block.compress(_build_comment_trial(0), store_size=False))
        c_max = len(lz4.block.compress(_build_comment_trial(max_body), store_size=False))
        if c_min <= target_comp_size <= c_max:
            lo, hi = 0, max_body
            while lo <= hi:
                mid = (lo + hi) // 2
                trial = _build_comment_trial(mid)
                c = lz4.block.compress(trial, store_size=False)
                if len(c) == target_comp_size:
                    return trial
                elif len(c) < target_comp_size:
                    lo = mid + 1
                else:
                    hi = mid - 1
            for n in range(max(0, lo - 20), min(lo + 20, max_body + 1)):
                trial = _build_comment_trial(n)
                if len(lz4.block.compress(trial, store_size=False)) == target_comp_size:
                    return trial

    # ── Strategy 3: distribute comments across newline positions ───────
    # Insert incompressible XML comments at newline positions throughout
    # the file body. Each inserted byte displaces one byte of content off
    # the tail (which gets trimmed to stay within orig_size).
    #
    # Budget = padding_available + trailing_whitespace_in_plaintext.
    # Trailing whitespace (spaces/tabs/CR/LF at end of file) can be trimmed
    # safely — the game's XML parser ignores trailing whitespace.
    #
    # We try multiple slot counts (more slots = more overhead = higher c_max)
    # until we find one that brackets the target.
    plaintext = padded[:plaintext_len]
    tail_ws = 0
    for b in reversed(plaintext):
        if b in (0x20, 0x09, 0x0D, 0x0A):
            tail_ws += 1
        else:
            break
    effective_budget = padding_available + tail_ws
    base_content = bytes(plaintext[:plaintext_len - tail_ws])

    newlines = _find_insertion_points(plaintext)
    if newlines and effective_budget >= 7:
        # Try increasing slot counts until the target is bracketed.
        # Generate rand_pool once per slot-count attempt and reuse it
        # consistently so c_min/c_max/binary-search all use the same bytes.
        # Retry with fresh random bytes if the bracket check fails (c_max
        # varies slightly with different random content).
        for n_slots_try in [50, 100, 200, min(500, len(newlines))]:
            n_slots_try = min(n_slots_try, len(newlines))
            step = max(1, len(newlines) // n_slots_try)
            slots = newlines[::step][:n_slots_try]

            max_slots_usable = min(len(slots), effective_budget // 7)
            if max_slots_usable < 1:
                continue
            max_total_body = effective_budget - max_slots_usable * 7
            if max_total_body <= 0:
                continue

            for _attempt in range(8):  # retry with fresh random bytes
                # Generate once and capture in closure — all calls use same pool
                _rand_pool = _make_xml_safe_incompressible(
                    max_total_body + max_slots_usable * 7 + 8)
                _slots_cap = slots
                _msu_cap = max_slots_usable
                _bc_cap = base_content

                def _build_multi_comment_trial(total_body: int,
                                               _s=_slots_cap, _msu=_msu_cap,
                                               _rp=_rand_pool, _bc=_bc_cap) -> bytes:
                    n_active = _msu
                    per_slot = total_body // n_active
                    remainder = total_body % n_active
                    insertions = []
                    pool_offset = 0
                    for slot_idx in range(n_active):
                        body_len = per_slot + (1 if slot_idx < remainder else 0)
                        body = _rp[pool_offset:pool_offset + body_len]
                        pool_offset += body_len
                        comment = b'<!--' + body + b'-->'
                        insertions.append((_s[slot_idx], comment))
                    result = bytearray(_bc)
                    for ins_offset, comment in sorted(insertions, key=lambda x: -x[0]):
                        result[ins_offset + 1:ins_offset + 1] = comment
                    if len(result) > target_orig_size:
                        result = result[:target_orig_size]
                    else:
                        result.extend(b'\x00' * (target_orig_size - len(result)))
                    return bytes(result)

                c_min = len(lz4.block.compress(_build_multi_comment_trial(0), store_size=False))
                c_max = len(lz4.block.compress(_build_multi_comment_trial(max_total_body), store_size=False))
                if not (c_min <= target_comp_size <= c_max):
                    continue

                lo, hi = 0, max_total_body
                while lo <= hi:
                    mid = (lo + hi) // 2
                    trial = _build_multi_comment_trial(mid)
                    c = len(lz4.block.compress(trial, store_size=False))
                    if c == target_comp_size:
                        return trial
                    elif c < target_comp_size:
                        lo = mid + 1
                    else:
                        hi = mid - 1
                for n in range(max(0, lo - 30), min(lo + 30, max_total_body + 1)):
                    trial = _build_multi_comment_trial(n)
                    if len(lz4.block.compress(trial, store_size=False)) == target_comp_size:
                        return trial

    return None


def _inflate_by_replacing_comment_bodies(padded: bytes, target_comp_size: int) -> bytes | None:
    """Replace existing XML comment body bytes with incompressible content
    in-place (same byte count, no size change).

    Tries multiple random fills — each gives a different compressed-size curve,
    so retrying finds one where the target is reachable.

    Returns adjusted data or None.
    """
    comments = _find_xml_comments(padded)
    if not comments:
        return None

    positions = [i for cstart, cend in comments for i in range(cstart, cend)]
    if not positions:
        return None

    total = len(positions)

    def _try_fill(rand_fill: bytes) -> bytes | None:
        def _build_trial(n: int) -> bytes:
            trial = bytearray(padded)
            for idx, pos in enumerate(positions[:n]):
                trial[pos] = rand_fill[idx]
            return bytes(trial)

        c_none = len(lz4.block.compress(_build_trial(0), store_size=False))
        c_all  = len(lz4.block.compress(_build_trial(total), store_size=False))
        if target_comp_size < c_none or target_comp_size > c_all:
            return None

        lo, hi = 0, total
        while lo <= hi:
            mid = (lo + hi) // 2
            c = len(lz4.block.compress(_build_trial(mid), store_size=False))
            if c == target_comp_size:
                return _build_trial(mid)
            elif c < target_comp_size:
                lo = mid + 1
            else:
                hi = mid - 1

        # Linear scan near boundary — wider window since curve isn't perfectly monotonic
        for n in range(max(0, lo - 50), min(lo + 50, total + 1)):
            if len(lz4.block.compress(_build_trial(n), store_size=False)) == target_comp_size:
                return _build_trial(n)

        return None

    for _ in range(8):
        result = _try_fill(_make_xml_safe_incompressible(total))
        if result is not None:
            return result

    return None


def _inflate_by_replacing_whitespace_runs(padded: bytes, target_comp_size: int) -> bytes | None:
    """Replace whitespace-only runs with XML comments containing incompressible
    content, in-place (same byte count, no size change).

    Finds runs of 8+ consecutive whitespace bytes (indentation/blank lines in
    formatted XML). Replaces each run with <!--BODY--> padded back to the same
    length with spaces. The random body bytes are incompressible, breaking LZ4
    back-references and inflating the compressed size.

    Only runs of 8+ bytes are used (minimum for a valid <!--X--> comment).
    The surrounding XML structure is preserved — the comment replaces whitespace
    that the game's XML parser ignores anyway.

    Returns adjusted data or None.
    """
    # Find whitespace-only runs of 8+ bytes (space, tab, CR, LF)
    runs = []  # list of (start, end) for each qualifying run
    i = 0
    data = padded
    n = len(data)
    while i < n:
        if data[i] in (0x20, 0x09, 0x0D, 0x0A):
            run_start = i
            while i < n and data[i] in (0x20, 0x09, 0x0D, 0x0A):
                i += 1
            run_len = i - run_start
            if run_len >= 8:
                runs.append((run_start, i))
        else:
            i += 1

    if not runs:
        return None

    # For each run we can embed a comment <!--BODY--> where body fills
    # (run_len - 7) bytes, padded back with spaces to run_len.
    # We treat each run as a slot: either fully activated (max body) or
    # not activated (original whitespace). Binary-search how many slots
    # to activate to hit the target compressed size.
    total_slots = len(runs)

    def _build_trial_with_slots(n_active: int, rand_fill: bytes) -> bytes:
        trial = bytearray(padded)
        fill_offset = 0
        for run_start, run_end in runs[:n_active]:
            run_len = run_end - run_start
            body_len = run_len - 7  # 4 (<!--) + body + 3 (-->)
            body = rand_fill[fill_offset:fill_offset + body_len]
            fill_offset += body_len
            comment = b'<!--' + body + b'-->'
            # Pad back to run_len with spaces
            replacement = comment + b' ' * (run_len - len(comment))
            trial[run_start:run_end] = replacement
        return bytes(trial)

    total_body = sum(max(0, (e - s) - 7) for s, e in runs)

    def _try_fill(rand_fill: bytes) -> bytes | None:
        c_none = len(lz4.block.compress(_build_trial_with_slots(0, rand_fill), store_size=False))
        c_all  = len(lz4.block.compress(_build_trial_with_slots(total_slots, rand_fill), store_size=False))
        if target_comp_size < c_none or target_comp_size > c_all:
            return None

        lo, hi = 0, total_slots
        while lo <= hi:
            mid = (lo + hi) // 2
            c = len(lz4.block.compress(_build_trial_with_slots(mid, rand_fill), store_size=False))
            if c == target_comp_size:
                return _build_trial_with_slots(mid, rand_fill)
            elif c < target_comp_size:
                lo = mid + 1
            else:
                hi = mid - 1

        for n in range(max(0, lo - 10), min(lo + 10, total_slots + 1)):
            trial = _build_trial_with_slots(n, rand_fill)
            if len(lz4.block.compress(trial, store_size=False)) == target_comp_size:
                return trial

        return None

    for _ in range(12):
        result = _try_fill(_make_xml_safe_incompressible(total_body + 16))
        if result is not None:
            return result

    return None


def _match_compressed_size(plaintext: bytes, target_comp_size: int,
                           target_orig_size: int) -> bytes:
    """Adjust plaintext so it compresses to exactly target_comp_size.

    If the plaintext is larger than target_orig_size, trims comment content
    and whitespace to fit. Then finds individual byte positions where
    replacing with a space changes the LZ4 compressed output to exactly
    the target.

    Returns:
        adjusted plaintext (exactly target_orig_size bytes)

    Raises:
        ValueError if size matching fails
    """
    if len(plaintext) > target_orig_size:
        excess = len(plaintext) - target_orig_size
        comments = _find_xml_comments(plaintext)
        comment_room = sum(max(0, end - start - 1) for start, end in comments)
        if comment_room < excess:
            raise ValueError(
                f"Modified file is {excess} bytes over orig_size ({target_orig_size}) "
                f"with only {comment_room} bytes of XML comment content available to trim. "
                f"You need to shorten your changes by {excess - comment_room} bytes "
                f"(e.g. remove added attributes or shorten values).")
        padded = _shrink_to_orig_size(plaintext, target_orig_size)
    else:
        padded = _pad_to_orig_size(plaintext, target_orig_size)

    comp = lz4.block.compress(padded, store_size=False)
    if len(comp) == target_comp_size:
        return padded

    delta = len(comp) - target_comp_size  # positive = need to shrink

    # Branch based on direction needed
    if delta < 0:
        # Need to INFLATE: compressed output is too small.
        result = _inflate_with_comments(padded, len(plaintext),
                                        target_comp_size, target_orig_size)
        if result is not None:
            return result
        # Fallback: replace existing comment bodies with incompressible content
        result = _inflate_by_replacing_comment_bodies(padded, target_comp_size)
        if result is not None:
            return result
        # Last resort: replace whitespace runs with incompressible content
        result = _inflate_by_replacing_whitespace_runs(padded, target_comp_size)
        if result is not None:
            return result
        raise ValueError(
            f"Cannot match target comp_size {target_comp_size} "
            f"(got {len(comp)}, delta {delta}). "
            f"File compresses too well — need ~{-delta} more bytes of incompressible content. "
            f"Try making fewer changes, or add more content to bring the file closer to orig_size ({target_orig_size} bytes).")

    # Need to SHRINK: compressed output is too large.
    # Strategy: replace non-space bytes in the padded data with spaces.
    # Spaces are maximally compressible in LZ4 (back-references to prior runs).
    #
    # We build a sorted list of candidate positions — bytes that are currently
    # not spaces, prioritised by proximity to existing space runs (so each
    # replacement is most likely to extend a back-reference and reduce size).
    # Then we do a cumulative scan: replace 1, 2, 3 … positions and stop as
    # soon as compressed size hits the target.
    #
    # We do NOT binary-search because LZ4 compression is not strictly monotonic
    # byte-by-byte — a single replacement can sometimes increase size slightly
    # before the next one decreases it. A linear scan is the only safe approach.

    # Build candidate list: non-space bytes, ordered by proximity to spaces
    # (positions immediately adjacent to a space first, then the rest).
    space_set = set(i for i in range(len(padded)) if padded[i] == 0x20)
    adjacent = []
    non_adjacent = []
    for i in range(len(padded)):
        if padded[i] == 0x20:
            continue
        if (i > 0 and padded[i - 1] == 0x20) or (i + 1 < len(padded) and padded[i + 1] == 0x20):
            adjacent.append(i)
        else:
            non_adjacent.append(i)
    candidates = adjacent + non_adjacent

    if not candidates:
        raise ValueError(
            f"Cannot match target comp_size {target_comp_size} "
            f"(got {len(comp)}, delta {delta}): no replaceable bytes")

    # Cumulative replacement scan — apply replacements one at a time and check
    trial = bytearray(padded)
    for n, pos in enumerate(candidates):
        trial[pos] = 0x20
        c = len(lz4.block.compress(bytes(trial), store_size=False))
        if c == target_comp_size:
            return bytes(trial)
        if c < target_comp_size:
            # Overshot — the previous state was closest; do a local scan
            # around the last few replacements to find the exact target
            break
    else:
        raise ValueError(
            f"Cannot match target comp_size {target_comp_size} "
            f"(got {len(comp)}, delta {delta}): exhausted all candidates")

    # We overshot at position n. Try reverting the last few replacements
    # one at a time to find the exact target.
    for revert_count in range(1, min(n + 2, 200)):
        # Revert the last `revert_count` replacements
        trial2 = bytearray(padded)
        for pos in candidates[:n + 1 - revert_count]:
            trial2[pos] = 0x20
        c = len(lz4.block.compress(bytes(trial2), store_size=False))
        if c == target_comp_size:
            return bytes(trial2)

    raise ValueError(
        f"Cannot match target comp_size {target_comp_size} "
        f"(got {len(comp)}, delta {delta})")


# ── Core repack ──────────────────────────────────────────────────────

def repack_entry(modified_path: str, entry: PazEntry,
                 output_path: str = None, dry_run: bool = False) -> dict:
    """Repack a modified file and patch it into the PAZ archive.

    Args:
        modified_path: path to the modified plaintext file
        entry: PAMT entry for the file being replaced
        output_path: if set, write to this file instead of patching the PAZ
        dry_run: if True, compute sizes but don't write anything

    Returns:
        dict with repack stats
    """
    with open(modified_path, 'rb') as f:
        plaintext = f.read()

    basename = os.path.basename(entry.path)
    is_compressed = entry.compressed and entry.compression_type == 2

    if is_compressed:
        # Need to match both orig_size and comp_size exactly
        adjusted = _match_compressed_size(plaintext, entry.comp_size, entry.orig_size)
        compressed = lz4.block.compress(adjusted, store_size=False)
        assert len(compressed) == entry.comp_size, \
            f"Size mismatch: {len(compressed)} != {entry.comp_size}"
        payload = compressed
    else:
        # Uncompressed: pad/truncate to comp_size, zero-pad remainder
        if len(plaintext) > entry.comp_size:
            raise ValueError(
                f"Modified file ({len(plaintext)} bytes) exceeds budget "
                f"({entry.comp_size} bytes). Reduce content.")
        payload = plaintext + b'\x00' * (entry.comp_size - len(plaintext))

    # Encrypt if it's an XML file
    if entry.encrypted:
        payload = encrypt(payload, basename)

    result = {
        "entry_path": entry.path,
        "modified_size": len(plaintext),
        "comp_size": entry.comp_size,
        "orig_size": entry.orig_size,
        "compressed": is_compressed,
        "encrypted": entry.encrypted,
    }

    if dry_run:
        result["action"] = "dry_run"
        return result

    if output_path:
        # Write to standalone file
        os.makedirs(os.path.dirname(output_path) or '.', exist_ok=True)
        with open(output_path, 'wb') as f:
            f.write(payload)
        result["action"] = "written"
        result["output"] = output_path
    else:
        # Patch directly into PAZ archive
        restore_ts = _save_timestamps(entry.paz_file)

        with open(entry.paz_file, 'r+b') as f:
            f.seek(entry.offset)
            f.write(payload)

        restore_ts()
        result["action"] = "patched"
        result["paz_file"] = entry.paz_file
        result["offset"] = f"0x{entry.offset:08X}"

    return result


# ── CLI ──────────────────────────────────────────────────────────────

def find_entry(entries: list[PazEntry], entry_path: str) -> PazEntry:
    """Find a PAMT entry by path (case-insensitive, partial match)."""
    entry_path = entry_path.lower().replace('\\', '/')

    # Exact match first
    for e in entries:
        if e.path.lower().replace('\\', '/') == entry_path:
            return e

    # Partial match (basename or suffix)
    matches = [e for e in entries if entry_path in e.path.lower().replace('\\', '/')]
    if len(matches) == 1:
        return matches[0]
    elif len(matches) > 1:
        print(f"Ambiguous entry path '{entry_path}', matches:", file=sys.stderr)
        for m in matches[:10]:
            print(f"  {m.path}", file=sys.stderr)
        if len(matches) > 10:
            print(f"  ... ({len(matches) - 10} more)", file=sys.stderr)
        sys.exit(1)

    print(f"Entry not found: '{entry_path}'", file=sys.stderr)
    sys.exit(1)


def main():
    parser = argparse.ArgumentParser(
        description="Repack a modified file into a PAZ archive",
        epilog="Example: python paz_repack.py modified.xml --pamt 0.pamt "
               "--paz-dir ./0003 --entry technique/rendererconfiguration.xml")
    parser.add_argument("modified", help="Path to modified file")
    parser.add_argument("--pamt", required=True, help="Path to .pamt index file")
    parser.add_argument("--paz-dir", help="Directory containing .paz files")
    parser.add_argument("--entry", required=True,
                        help="Entry path within the archive (or partial match)")
    parser.add_argument("--output", help="Write to file instead of patching PAZ")
    parser.add_argument("--dry-run", action="store_true",
                        help="Show what would happen without writing")
    args = parser.parse_args()

    entries = parse_pamt(args.pamt, paz_dir=args.paz_dir)
    entry = find_entry(entries, args.entry)

    print(f"Entry:      {entry.path}")
    print(f"PAZ:        {entry.paz_file} @ 0x{entry.offset:08X}")
    print(f"comp_size:  {entry.comp_size:,}")
    print(f"orig_size:  {entry.orig_size:,}")
    print(f"Compressed: {'LZ4' if entry.compressed else 'no'}")
    print(f"Encrypted:  {'yes' if entry.encrypted else 'no'}")
    print()

    try:
        result = repack_entry(args.modified, entry,
                              output_path=args.output,
                              dry_run=args.dry_run)
    except ValueError as e:
        print(f"Error: {e}", file=sys.stderr)
        sys.exit(1)

    if result["action"] == "dry_run":
        print("Dry run — no changes made.")
    elif result["action"] == "written":
        print(f"Written to {result['output']}")
    elif result["action"] == "patched":
        print(f"Patched {result['paz_file']} at {result['offset']}")

    print(f"Modified file: {result['modified_size']:,} bytes")


if __name__ == "__main__":
    main()

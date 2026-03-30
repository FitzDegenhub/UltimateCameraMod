"""CDCamera — on-the-fly camera modification for Crimson Desert.

Extracts the vanilla camera XML from the game's PAZ archive, applies
user-chosen modifications, size-matches, compresses, encrypts, and
patches the result back.  Works on any game version automatically.

Usage:
    python camera_mod.py <game_dir> --style western --fov 20 --bane --combat wide
    python camera_mod.py <game_dir> --restore
"""

import argparse
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from paz_crypto import decrypt, encrypt, lz4_decompress
from paz_parse import parse_pamt
from paz_repack import _match_compressed_size
import lz4.block

from camera_rules import build_modifications


# ── XML modification engine ────────────────────────────────────────

TAG_RE = re.compile(r'<(\w+)\s+([^>]*?)(/?)>')

SUBELEMENT_TAGS = frozenset({
    'CameraDamping', 'CameraBlendParameter', 'OffsetByVelocity',
    'PivotHeight', 'ZoomLevel',
})

def _parse_attrs(attrs_str):
    return dict(re.findall(r'(\w+)="([^"]*)"', attrs_str))


def apply_modifications(xml_text, mod_set):
    """Apply element-level modifications and additive FoV to XML text.

    Walks line by line, tracks parent context, and modifies attribute
    values using regex substitution.  Also applies the global FoV pass
    on Type="TPS" and Type="TwoTargetLockOn" sections.
    """
    element_mods = mod_set.element_mods
    fov_value = mod_set.fov_value

    lines = xml_text.split('\n')
    # depth_stack tracks ALL non-self-closing opening tags (both matched and
    # unmatched). Entries are (tag_name, is_section) where is_section means
    # it was a TAG_RE match with attributes — usable as a parent for key gen.
    depth_stack = []
    key_counter = {}  # tracks occurrence count per key for duplicate handling
    result = []

    BARE_OPEN_RE = re.compile(r'^<(\w+)>$')

    for line in lines:
        stripped = line.strip()

        if stripped == '</>':
            result.append(line)
            if depth_stack:
                depth_stack.pop()
            continue

        # Detect attribute-less container tags (ZoomLevelInfo, CameraPreset, etc.)
        bm = BARE_OPEN_RE.match(stripped)
        if bm:
            depth_stack.append((bm.group(1), False))
            result.append(line)
            continue

        m = TAG_RE.match(stripped)
        if not m:
            result.append(line)
            continue

        tag = m.group(1)
        attrs_str = m.group(2)
        self_closing = m.group(3) == '/'
        attrs = _parse_attrs(attrs_str)

        # Find nearest section-level parent (skip wrapper entries)
        parent_tag = ''
        for tag_name, is_section in reversed(depth_stack):
            if is_section:
                parent_tag = tag_name
                break

        if tag == 'ZoomLevel':
            level = attrs.get('Level', '?')
            key = f'{parent_tag}/ZoomLevel[{level}]' if parent_tag else f'ZoomLevel[{level}]'
        else:
            key = f'{parent_tag}/{tag}' if parent_tag else tag

        # Track occurrence count for duplicate keys (e.g. multiple
        # CameraBlendParameter at root level). Try key#N first.
        key_counter[key] = key_counter.get(key, 0) + 1
        occurrence = key_counter[key]
        indexed_key = f'{key}#{occurrence}'

        modified_line = line

        match_key = (indexed_key if indexed_key in element_mods
                     else key if key in element_mods else None)
        if match_key:
            for attr, (action, value) in element_mods[match_key].items():
                if action == 'SET':
                    if re.search(rf'{attr}="', modified_line):
                        modified_line = re.sub(
                            rf'{attr}="[^"]*"',
                            f'{attr}="{value}"',
                            modified_line, count=1,
                        )
                    else:
                        modified_line = re.sub(
                            r'(/?>)',
                            f' {attr}="{value}"\\1',
                            modified_line, count=1,
                        )
                elif action == 'REMOVE':
                    modified_line = re.sub(rf'\s+{attr}="[^"]*"', '', modified_line)

        if fov_value > 0:
            # CDCamera v1.60+: FoV delta only applies to on-foot TPS sections
            # (Player_Basic_Default* and Player_Weapon_Default*). Combat lock-on,
            # ride/mount, animal, and interaction sections keep their vanilla FoV.
            section = parent_tag if tag in SUBELEMENT_TAGS or tag == 'ZoomLevel' else tag
            apply_fov = section.startswith(('Player_Basic_Default', 'Player_Weapon_Default'))
            fov_match = (apply_fov and
                         re.search(r'(?<!\w)Fov="([^"]*)"', modified_line))
            if fov_match:
                try:
                    cur_fov = float(fov_match.group(1))
                    new_fov = int(round(cur_fov + fov_value))
                    modified_line = re.sub(
                        r'(?<!\w)Fov="[^"]*"',
                        f'Fov="{new_fov}"',
                        modified_line, count=1,
                    )
                except ValueError:
                    pass
            if tag == 'ZoomLevel' and apply_fov:
                idfov_match = re.search(r'InDoorFov="([^"]*)"', modified_line)
                if idfov_match:
                    try:
                        cur_idfov = float(idfov_match.group(1))
                        new_idfov = int(round(cur_idfov + fov_value))
                        modified_line = re.sub(
                            r'InDoorFov="[^"]*"',
                            f'InDoorFov="{new_idfov}"',
                            modified_line, count=1,
                        )
                    except ValueError:
                        pass

        result.append(modified_line)

        if tag not in SUBELEMENT_TAGS and tag != 'ZoomLevel':
            if not self_closing:
                depth_stack.append((tag, True))

    return '\n'.join(result)


def strip_header_comments(xml_text):
    """Strip Korean comment blocks at the top of the XML.

    Creates ~700 bytes of zero-padding room for the size matcher.
    """
    lines = xml_text.split('\n')
    result = []
    in_comment = False
    header_done = False

    for line in lines:
        stripped = line.strip()

        if header_done:
            result.append(line)
            continue

        if '<!--' in stripped and '-->' not in stripped:
            in_comment = True
            continue
        if in_comment:
            if '-->' in stripped:
                in_comment = False
            continue
        if stripped.startswith('<!--') and stripped.endswith('-->'):
            continue
        if not stripped:
            continue

        header_done = True
        result.append(line)

    return '\n'.join(result)


# ── Backup management ──────────────────────────────────────────────

def _backups_dir():
    return os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))),
                        'backups')


_VANILLA_FOV = {'45', '53', '40'}

def _validate_vanilla(xml_text):
    """Check if XML has vanilla values. Returns True if vanilla."""
    m = re.search(r'<Player_Basic_Default_Run\s+[^>]*?Fov="(\d+)"', xml_text)
    if m and m.group(1) not in _VANILLA_FOV:
        return False
    m = re.search(r'<Player_Basic_Default_Runfast\s+[^>]*?Fov="(\d+)"', xml_text)
    if m and m.group(1) not in _VANILLA_FOV:
        return False
    return True


def _ensure_backup(entry):
    """Create backup of original PAZ bytes on first run.

    If the game has updated (comp_size changed), the old backup is
    stale — delete it and re-backup from the now-vanilla PAZ.
    Validates that the data is actually vanilla before accepting it.
    """
    bdir = _backups_dir()
    backup_path = os.path.join(bdir, 'original_backup.bin')
    meta_path = os.path.join(bdir, 'backup_meta.txt')

    if os.path.exists(backup_path) and os.path.exists(meta_path):
        with open(meta_path) as f:
            for part in f.read().split():
                if part.startswith('comp_size='):
                    saved_comp = int(part.split('=')[1])
                    if saved_comp == entry.comp_size:
                        return

    os.makedirs(bdir, exist_ok=True)
    with open(entry.paz_file, 'rb') as f:
        f.seek(entry.offset)
        data = f.read(entry.comp_size)

    try:
        dec = decrypt(data, 'playercamerapreset.xml')
        xml_bytes = lz4_decompress(dec, entry.orig_size)
        xml_text = xml_bytes.rstrip(b'\x00').decode('utf-8-sig', errors='replace')
        if not _validate_vanilla(xml_text):
            print()
            print('  !! WARNING: Game files appear to be already modified !!')
            print()
            print('  Another camera mod (CDCamera, etc.) may still be installed.')
            print('  This will cause crashes or broken camera behavior.')
            print()
            print('  TO FIX:')
            print('    1. Close this installer')
            print('    2. Steam > Crimson Desert > Properties > Installed Files')
            print('       > "Verify integrity of game files"')
            print('    3. Run this installer again')
            print()
            sys.exit(1)
    except Exception:
        pass

    with open(backup_path, 'wb') as f:
        f.write(data)
    with open(meta_path, 'w') as f:
        f.write(f'comp_size={entry.comp_size} orig_size={entry.orig_size}')
    print(f'  Backup saved ({entry.comp_size} bytes)')


def _get_vanilla_xml(entry):
    """Extract vanilla XML from backup."""
    backup_path = os.path.join(_backups_dir(), 'original_backup.bin')
    with open(backup_path, 'rb') as f:
        encrypted = f.read()
    decrypted = decrypt(encrypted, 'playercamerapreset.xml')
    xml_bytes = lz4_decompress(decrypted, entry.orig_size)
    return xml_bytes.rstrip(b'\x00').decode('utf-8-sig')


# ── PAZ writing ────────────────────────────────────────────────────

def _write_to_paz(entry, encrypted_bytes):
    """Write encrypted bytes to the PAZ file at the entry's offset."""
    paz_path = entry.paz_file
    stat_before = os.stat(paz_path)

    with open(paz_path, 'r+b') as f:
        f.seek(entry.offset)
        f.write(encrypted_bytes)

    os.utime(paz_path, (stat_before.st_atime, stat_before.st_mtime))


# ── Main operations ────────────────────────────────────────────────

def _find_camera_entry(game_dir):
    """Parse PAMT and find the playercamerapreset.xml entry."""
    pamt_path = os.path.join(game_dir, '0010', '0.pamt')
    paz_dir = os.path.join(game_dir, '0010')

    if not os.path.exists(pamt_path):
        print(f'  ERROR: PAMT not found: {pamt_path}')
        sys.exit(1)

    entries = parse_pamt(pamt_path, paz_dir=paz_dir)
    for e in entries:
        if 'playercamerapreset.xml' in e.path:
            return e

    print('  ERROR: playercamerapreset.xml not found in PAMT')
    sys.exit(1)


def install_camera_mod(game_dir, style, fov, bane, combat,
                       mount_height=False, custom_up=None):
    """Apply camera modifications to the game."""
    print('  Finding camera entry...')
    entry = _find_camera_entry(game_dir)
    print(f'  Found: offset={entry.offset}, comp_size={entry.comp_size}, '
          f'orig_size={entry.orig_size}')

    print('  Ensuring backup...')
    _ensure_backup(entry)

    print('  Extracting vanilla XML...')
    vanilla_xml = _get_vanilla_xml(entry)

    print('  Stripping header comments...')
    vanilla_xml = strip_header_comments(vanilla_xml)

    print('  Building modification rules...')
    mod_set = build_modifications(style, fov, bane, combat,
                                  mount_height=mount_height,
                                  custom_up=custom_up)
    mod_count = sum(len(v) for v in mod_set.element_mods.values())
    print(f'  Rules: {mod_count} attribute changes'
          + (f', FoV=+{mod_set.fov_value}' if mod_set.fov_value else ''))

    print('  Applying modifications...')
    modified_xml = apply_modifications(vanilla_xml, mod_set)

    try:
        debug_dir = os.path.join(os.path.dirname(os.path.dirname(
            os.path.abspath(__file__))), 'debug')
        os.makedirs(debug_dir, exist_ok=True)
        debug_path = os.path.join(debug_dir, 'modified_camera.xml')
        with open(debug_path, 'w', encoding='utf-8-sig') as f:
            f.write(modified_xml)
        print(f'  Debug XML saved to {debug_path}')
    except Exception:
        pass

    print('  Encoding and size-matching...')
    xml_bytes = modified_xml.encode('utf-8-sig')
    matched = _match_compressed_size(xml_bytes, entry.comp_size, entry.orig_size)

    print('  Compressing...')
    compressed = lz4.block.compress(matched, store_size=False)
    assert len(compressed) == entry.comp_size, \
        f'Size mismatch: {len(compressed)} != {entry.comp_size}'

    print('  Encrypting...')
    encrypted = encrypt(compressed, 'playercamerapreset.xml')

    print('  Patching game files...')
    _write_to_paz(entry, encrypted)

    print('  Done!')
    return {'status': 'ok', 'comp_size': entry.comp_size}


def restore_camera(game_dir):
    """Restore vanilla camera from backup."""
    entry = _find_camera_entry(game_dir)
    backup_path = os.path.join(_backups_dir(), 'original_backup.bin')

    if not os.path.exists(backup_path):
        print('  No backup found. The camera may already be vanilla.')
        print('  If needed, use Steam > Verify Integrity of Game Files.')
        return {'status': 'no_backup'}

    with open(backup_path, 'rb') as f:
        backup_data = f.read()

    if len(backup_data) != entry.comp_size:
        print(f'  Backup size ({len(backup_data)}) does not match current '
              f'game version ({entry.comp_size}).')
        print('  The game may have been updated. Deleting stale backup.')
        os.remove(backup_path)
        meta_path = os.path.join(_backups_dir(), 'backup_meta.txt')
        if os.path.exists(meta_path):
            os.remove(meta_path)
        return {'status': 'stale_backup'}

    print('  Restoring original camera...')
    _write_to_paz(entry, backup_data)
    print('  Done! Camera restored to vanilla.')
    return {'status': 'ok'}


# ── CLI ────────────────────────────────────────────────────────────

def main():
    parser = argparse.ArgumentParser(description='CDCamera mod tool')
    parser.add_argument('game_dir', help='Path to Crimson Desert game folder')
    parser.add_argument('--restore', action='store_true',
                        help='Restore vanilla camera')
    parser.add_argument('--style',
                        choices=['default', 'western', 'cinematic', 'immersive',
                                 'lowcam', 'vlowcam', 'ulowcam', 're2'],
                        default='default')
    parser.add_argument('--fov', type=int, default=0,
                        help='Additive FoV delta (0=no change, 10/15/20/25/30/40)')
    parser.add_argument('--bane', action='store_true',
                        help='Centered camera (Bane mode)')
    parser.add_argument('--combat', choices=['default', 'wide', 'max'],
                        default='default')

    args = parser.parse_args()

    if args.restore:
        result = restore_camera(args.game_dir)
    else:
        if args.fov and not (0 <= args.fov <= 40):
            print(f'  ERROR: FoV delta must be 0-40 (got {args.fov})')
            sys.exit(1)
        result = install_camera_mod(
            args.game_dir, args.style, args.fov,
            args.bane, args.combat)

    sys.exit(0 if result.get('status') == 'ok' else 1)


if __name__ == '__main__':
    main()

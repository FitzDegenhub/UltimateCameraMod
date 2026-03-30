"""HUD centering for Crimson Desert ultrawide monitors.

Modifies UI HTML/CSS in PAZ archive 0012 to constrain HUD elements
within a 21:9 safe area, centering them on ultrawide displays.
"""

import os
import re
import sys
from pathlib import Path

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from paz_crypto import decrypt, encrypt, lz4_decompress, lz4_compress
from paz_parse import parse_pamt
from paz_repack import _match_compressed_size, _save_timestamps

_UI_ARCHIVE = '0012'

_HUD_HTML_PATHS = (
    'ui/minimaphudview2.html',
    'ui/statusgaugeview2.html',
)
_CSS_PATH = 'ui/gamecommon.css'
_ALL_PATHS = (*_HUD_HTML_PATHS, _CSS_PATH)

_SAFEFRAME_ID = 'HUDSafeFrame'
_SAFEFRAME_CLASS = 'ui-view-max-size-21-9'
_SAFEFRAME_MAX_WIDTH = 2520
_SAFEFRAME_OPEN = f'    <div id="{_SAFEFRAME_ID}" class="{_SAFEFRAME_CLASS}">'
_SAFEFRAME_CLOSE = '    </div>'

_BODY_OPEN_RE = re.compile(r'(<body\b[^>]*>)', re.IGNORECASE | re.DOTALL)
_BODY_CLOSE_RE = re.compile(r'(</body>)', re.IGNORECASE)
_HTML_COMMENT_RE = re.compile(r'<!--.*?-->', re.DOTALL)

_GAUGE_TRACKER = '<div id="HPGaugeTracker"'
_GAUGE_SKILLPOINT = '<div id="UIHudScaleSkillPointStatusGaugeContainer"'


# ── Text compaction (matching centerhud approach) ──────────────────

def _compact_css(text):
    t = re.sub(r'\n\s*\n+', '\n', text)
    t = re.sub(r'[ \t]+\n', '\n', t)
    return re.sub(r'\s*([{}:;,])\s*', r'\1', t)


def _compact_text(entry_path, text):
    if entry_path.lower().endswith('.css'):
        return _compact_css(text)
    t = text
    if entry_path.lower().endswith('.html'):
        t = _HTML_COMMENT_RE.sub('', t)
    return re.sub(r'\n\s*\n+', '\n', t)


# ── PAZ I/O ────────────────────────────────────────────────────────

def _find_ui_entries(game_dir):
    pamt = os.path.join(game_dir, _UI_ARCHIVE, '0.pamt')
    paz_dir = os.path.join(game_dir, _UI_ARCHIVE)
    if not os.path.exists(pamt):
        raise RuntimeError(f'UI archive not found: {pamt}')
    entries = parse_pamt(pamt, paz_dir=paz_dir)
    by_path = {e.path.lower(): e for e in entries}
    result = []
    for p in _ALL_PATHS:
        e = by_path.get(p)
        if e is None:
            raise RuntimeError(f'{p} not found in archive')
        result.append(e)
    return result


def _read_raw(entry):
    with open(entry.paz_file, 'rb') as f:
        f.seek(entry.offset)
        return f.read(entry.comp_size)


def _decode_text(entry, raw):
    """Decode payload, detecting encryption. Returns (text, encrypted)."""
    if entry.compressed:
        if entry.compression_type != 2:
            raise RuntimeError(f'Unsupported compression for {entry.path}')
        try:
            plain = lz4_decompress(raw, entry.orig_size)
        except Exception:
            dec = decrypt(raw, Path(entry.path).name)
            plain = lz4_decompress(dec, entry.orig_size)
            return plain.rstrip(b'\x00').decode('utf-8', errors='replace'), True
        try:
            return plain.rstrip(b'\x00').decode('utf-8'), False
        except UnicodeDecodeError:
            pass
        try:
            dec = decrypt(raw, Path(entry.path).name)
            dp = lz4_decompress(dec, entry.orig_size)
            return dp.rstrip(b'\x00').decode('utf-8', errors='replace'), True
        except Exception:
            return plain.rstrip(b'\x00').decode('utf-8', errors='replace'), False
    try:
        return raw.rstrip(b'\x00').decode('utf-8'), False
    except UnicodeDecodeError:
        dec = decrypt(raw, Path(entry.path).name)
        return dec.rstrip(b'\x00').decode('utf-8', errors='replace'), True


# ── HTML modification ──────────────────────────────────────────────

def _modify_html(entry_path, text):
    if _SAFEFRAME_ID in text:
        return text
    if not _BODY_OPEN_RE.search(text):
        raise RuntimeError(f'No <body> in {entry_path}')
    if not _BODY_CLOSE_RE.search(text):
        raise RuntimeError(f'No </body> in {entry_path}')

    modified = _BODY_OPEN_RE.sub(
        rf'\1\n{_SAFEFRAME_OPEN}\n', text, count=1)

    if entry_path.lower() == 'ui/statusgaugeview2.html':
        if _GAUGE_TRACKER not in modified:
            raise RuntimeError('HPGaugeTracker not found')
        if _GAUGE_SKILLPOINT not in modified:
            raise RuntimeError('SkillPointContainer not found')
        modified = modified.replace(
            _GAUGE_TRACKER,
            f'{_SAFEFRAME_CLOSE}\n{_GAUGE_TRACKER}', 1)
        modified = modified.replace(
            _GAUGE_SKILLPOINT,
            f'{_SAFEFRAME_OPEN}\n{_GAUGE_SKILLPOINT}', 1)

    modified = _BODY_CLOSE_RE.sub(
        rf'{_SAFEFRAME_CLOSE}\n\1', modified, count=1)
    return modified


# ── CSS modification ───────────────────────────────────────────────

def _modify_css(text, max_width=_SAFEFRAME_MAX_WIDTH):
    """Update .ui-view-max-size-21-9 max-width in existing rule."""
    pat = re.compile(
        r'(\.ui-view-max-size-21-9\s*\{)([^}]*)(\})',
        re.IGNORECASE | re.DOTALL)
    m = pat.search(text)
    if m is None:
        raise RuntimeError('.ui-view-max-size-21-9 not found in CSS')

    body = m.group(2)
    prop_re = re.compile(r'(max-width\s*:\s*)([^;]+)(;)', re.IGNORECASE)
    if prop_re.search(body):
        new_body = prop_re.sub(
            rf'\g<1>{max_width}px\3', body, count=1)
    else:
        new_body = body.rstrip()
        if new_body and not new_body.endswith(';'):
            new_body += ';'
        new_body += f' max-width: {max_width}px;'

    return text[:m.start(2)] + new_body + text[m.end(2):]


# ── Payload building (follows centerhud pattern) ───────────────────

def _build_payload(modified_text, entry, encrypted):
    compacted = _compact_text(entry.path, modified_text)
    candidates = [compacted] if compacted != modified_text else []
    candidates.append(modified_text)

    last_err = None
    for candidate in candidates:
        plaintext = candidate.encode('utf-8')
        try:
            if entry.compressed:
                adjusted = _match_compressed_size(
                    plaintext, entry.comp_size, entry.orig_size)
                payload = lz4_compress(adjusted)
                if len(payload) != entry.comp_size:
                    raise RuntimeError(
                        f'comp_size mismatch: {len(payload)} != {entry.comp_size}')
            else:
                if len(plaintext) > entry.comp_size:
                    raise RuntimeError(
                        f'Too large: {len(plaintext)} > {entry.comp_size}')
                payload = plaintext + b'\x00' * (entry.comp_size - len(plaintext))
        except Exception as e:
            last_err = e
            continue

        if encrypted:
            payload = encrypt(payload, Path(entry.path).name)
            if len(payload) != entry.comp_size:
                raise RuntimeError('Encrypted size mismatch')
        return payload

    raise RuntimeError(f'Cannot fit {entry.path}: {last_err}')


# ── Backup management ─────────────────────────────────────────────

def _backups_dir():
    return os.path.join(
        os.path.dirname(os.path.dirname(os.path.abspath(__file__))),
        'backups', 'hud')


def _backup_exists():
    return os.path.exists(os.path.join(_backups_dir(), 'meta.txt'))


def _save_backups(entries):
    bdir = _backups_dir()
    os.makedirs(bdir, exist_ok=True)
    for entry in entries:
        raw = _read_raw(entry)
        fname = entry.path.replace('/', '__') + '.bin'
        with open(os.path.join(bdir, fname), 'wb') as f:
            f.write(raw)
    meta = '\n'.join(
        f'{e.path}|{e.comp_size}|{e.orig_size}|{e.offset}'
        for e in entries)
    with open(os.path.join(bdir, 'meta.txt'), 'w') as f:
        f.write(meta)


def _load_backup(entry):
    fname = entry.path.replace('/', '__') + '.bin'
    path = os.path.join(_backups_dir(), fname)
    if not os.path.exists(path):
        return None
    with open(path, 'rb') as f:
        return f.read()


# ── Public API ─────────────────────────────────────────────────────

def install_centered_hud(game_dir, max_width=_SAFEFRAME_MAX_WIDTH):
    print(f'  [HUD] Finding UI entries (max-width: {max_width}px)...')
    entries = _find_ui_entries(game_dir)

    decoded = {}
    for entry in entries:
        raw = _read_raw(entry)
        text, enc = _decode_text(entry, raw)
        decoded[entry.path] = (text, enc)

    html_already = all(
        _SAFEFRAME_ID in decoded[e.path][0]
        for e in entries if e.path.lower().endswith('.html'))
    if html_already:
        print('  [HUD] Already installed.')
        return {'status': 'ok'}

    if not _backup_exists():
        print('  [HUD] Saving backups...')
        _save_backups(entries)

    writes = []
    for entry in entries:
        text, enc = decoded[entry.path]
        if entry.path.lower().endswith('.html'):
            modified = _modify_html(entry.path, text)
        elif entry.path.lower().endswith('.css'):
            modified = _modify_css(text, max_width=max_width)
        else:
            continue
        if modified == text:
            continue
        payload = _build_payload(modified, entry, enc)
        writes.append((entry, payload))

    for entry, payload in writes:
        restore_ts = _save_timestamps(entry.paz_file)
        with open(entry.paz_file, 'r+b') as f:
            f.seek(entry.offset)
            f.write(payload)
        restore_ts()
        print(f'  [HUD] Patched: {entry.path}')

    print('  [HUD] Done!')
    return {'status': 'ok'}


def restore_hud(game_dir):
    if not _backup_exists():
        return {'status': 'no_backup'}
    entries = _find_ui_entries(game_dir)

    for entry in entries:
        data = _load_backup(entry)
        if data is None:
            return {'status': 'error'}
        if len(data) != entry.comp_size:
            return {'status': 'stale_backup'}

    for entry in entries:
        data = _load_backup(entry)
        restore_ts = _save_timestamps(entry.paz_file)
        with open(entry.paz_file, 'r+b') as f:
            f.seek(entry.offset)
            f.write(data)
        restore_ts()
        print(f'  [HUD] Restored: {entry.path}')

    return {'status': 'ok'}

"""Verify the tested sources, UI journeys and delivered store build without touching player saves."""
import hashlib
import json
from datetime import datetime, timezone
from pathlib import Path

root = Path(__file__).resolve().parents[2]
audit = root / 'Documentation/IapStore-2026-10-20'
run = audit / 'Run'
isolated = root / 'Build/ParityProject'
player = isolated / 'Build/ParityAuditPlayer'
delivery = root / 'Build/Windows'


def load(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


def sha(path):
    digest = hashlib.sha256()
    with path.open('rb') as stream:
        while chunk := stream.read(1024 * 1024):
            digest.update(chunk)
    return digest.hexdigest()


models = load(run / 'model-checks.json')
build = load(run / 'audit-build.json')
assert models['failed'] == 0 and models['passed'] >= 330, models
assert build['result'] == 'Succeeded' and build['errors'] == 0, build
journeys = {}
sizes = {'runtime-540x960': (540, 960), 'runtime-720x1080': (720, 1080),
         'runtime-480x1060': (480, 1060), 'runtime-960x540': (960, 540), 'runtime-regression': (540, 960)}
for name, size in sizes.items():
    result_path = run / name / 'runtime-check.json'
    data = load(result_path)
    assert data['passed'], (name, data.get('error'))
    assert (data['width'], data['height']) == size, (name, data['width'], data['height'])
    assert result_path.stat().st_mtime >= (player / 'BufuRebuild_Data/Managed/Assembly-CSharp.dll').stat().st_mtime, 'Stale runtime result: ' + name
    journeys[name] = dict(passed=data['passed'], checks=data['checks'], width=data['width'], height=data['height'])

sources = list((root / 'Assets/Scripts').rglob('*.cs')) + list((root / 'Assets/Editor').rglob('*.cs'))
sources += list((root / 'Assets/Resources/Commerce').glob('*.json'))
for path in sources:
    tested = isolated / path.relative_to(root)
    assert tested.is_file() and sha(path) == sha(tested), str(path)
assert sha(root / 'Tools/ParityAudit/IapStoreProbe.cs') == sha(isolated / 'Assets/Editor/IapStoreProbe.cs')

files = []
for path in sorted(player.rglob('*')):
    if not path.is_file():
        continue
    relative = path.relative_to(player)
    copied = delivery / relative
    assert copied.is_file() and path.stat().st_size == copied.stat().st_size and sha(path) == sha(copied), str(relative)
    files.append(dict(path=relative.as_posix(), bytes=path.stat().st_size, sha256=sha(path)))
assert all((player / p.relative_to(delivery)).is_file() for p in delivery.rglob('*') if p.is_file())
backups = sorted((root / 'Build').glob('Windows-Before-Iap-*'))
assert backups
report = dict(checked_utc=datetime.now(timezone.utc).isoformat(), verified=True,
              model_checks=dict(passed=models['passed'], failed=models['failed']), build=build,
              journeys=journeys, compared_sources=len(sources),
              snapshots=[dict(path=p.relative_to(root).as_posix(), sha256=sha(p)) for p in sources],
              screenshots=[dict(path=p.relative_to(audit).as_posix(), sha256=sha(p))
                           for name in journeys for p in sorted((run / name).glob('*.png'))],
              billing_live=False, real_payment_tested=False, player_save_modified=False,
              delivery=str(delivery), backup=str(backups[-1]), file_count=len(files), files=files)
(audit / 'execution-evidence.json').write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps(dict(verified=True, models=models['passed'], journeys=journeys, sources=len(sources),
                     screenshots=len(report['screenshots']), files=len(files), backup=str(backups[-1])), ensure_ascii=False))

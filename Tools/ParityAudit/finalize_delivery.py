import hashlib
import json
from datetime import datetime, timezone
from pathlib import Path

root = Path(__file__).resolve().parents[2]
source = root / 'Build/ParityProject/Build/ParityAuditPlayer'
target = root / 'Build/Windows'
audit = root / 'Documentation/OriginalParityAudit-2026-10-20'

def sha(path):
    digest = hashlib.sha256()
    with path.open('rb') as stream:
        while chunk := stream.read(1024 * 1024):
            digest.update(chunk)
    return digest.hexdigest()

files = []
for path in sorted(source.rglob('*')):
    if not path.is_file():
        continue
    relative = path.relative_to(source)
    copy = target / relative
    if not copy.is_file() or path.stat().st_size != copy.stat().st_size:
        raise RuntimeError('Missing or incomplete delivered file: ' + str(relative))
    digest = sha(path)
    if digest != sha(copy):
        raise RuntimeError('Delivered file differs: ' + str(relative))
    files.append(dict(path=relative.as_posix(), bytes=path.stat().st_size, sha256=digest))
extra = [str(p.relative_to(target)) for p in target.rglob('*') if p.is_file()
         and not (source / p.relative_to(target)).is_file()]
if extra:
    raise RuntimeError('Unexpected delivered files: ' + str(extra))
backup = sorted((root / 'Build').glob('Windows-Before-Parity-*'))[-1]
report = dict(checked_utc=datetime.now(timezone.utc).isoformat(), verified=True,
              source=str(source), destination=str(target), previous_build_backup=str(backup),
              launcher=str(root / '启动游戏.cmd'), file_count=len(files), files=files)
(audit / 'delivery-manifest.json').write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps(dict(verified=True, files=len(files), bytes=sum(f['bytes'] for f in files),
                      destination=str(target), backup=str(backup)), ensure_ascii=False))

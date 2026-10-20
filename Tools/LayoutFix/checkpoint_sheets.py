from pathlib import Path
import shutil,json,hashlib
root=Path(__file__).resolve().parents[2]
source=(root/'Build/Windows').resolve();backup=(root/'Build/Windows-before-sheets-20261020').resolve()
assert source.parent==backup.parent==root/'Build' and source.is_dir()
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
if not backup.exists():shutil.copytree(source,backup)
before={str(p.relative_to(source)):dict(bytes=p.stat().st_size,sha256=sha(p)) for p in source.rglob('*') if p.is_file()}
assert {k:v['sha256'] for k,v in before.items()}=={str(p.relative_to(backup)):sha(p) for p in backup.rglob('*') if p.is_file()}
report=dict(build_mode='Development Windows64, Intro/Main scenes, identical Unity editor and build options',files=before,total_bytes=sum(v['bytes'] for v in before.values()),backup=str(backup))
(root/'Documentation/SpriteSheetAudit-2026-10-20/before-delivery.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print('BASELINE_CHECKPOINT_OK',len(before),'files',round(report['total_bytes']/2**20,2),'MiB')

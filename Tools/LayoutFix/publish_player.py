"""Stage and hash-check the tested player before replacing the local delivery folder."""
from pathlib import Path
import hashlib,json,shutil

root=Path(__file__).resolve().parents[2]
build=(root/'Build').resolve()
source=(build/'LayoutFixCommercialPlayer').resolve()
release=(build/'Windows').resolve()
stage=(build/'Windows-layout-staged').resolve()
backup=(build/'Windows-before-layout-20261020').resolve()
for path in (source,release,stage,backup):
    assert path.parent==build and path.is_relative_to(root),path
assert source.is_dir() and release.is_dir()
sha=lambda path:hashlib.sha256(path.read_bytes()).hexdigest()
manifest={str(p.relative_to(source)):sha(p) for p in source.rglob('*') if p.is_file()}
if not stage.exists():shutil.copytree(source,stage)
assert {str(p.relative_to(stage)):sha(p) for p in stage.rglob('*') if p.is_file()}==manifest
if backup.exists():
    previous_identity=json.loads((root/'Documentation/LayoutFix-2026-10-20/delivery-identity.json').read_text())
    assert previous_identity['backup']==str(backup) and previous_identity['delivery']==str(release)
    current={str(p.relative_to(release)):sha(p) for p in release.rglob('*') if p.is_file()}
    assert current==previous_identity['files'], 'Inspect external delivery changes before replacing.'
    assert set(current).issubset(manifest)
    changed=[name for name,digest in manifest.items() if current.get(name)!=digest]
    prior_bytes={name:(release/name).read_bytes() for name in changed if name in current}
    try:
        for name in changed:
            destination=(release/name).resolve();assert destination.is_relative_to(release)
            destination.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(stage/name,destination)
    except Exception:
        for name,data in prior_bytes.items():(release/name).write_bytes(data)
        for name in set(changed)-set(prior_bytes):
            added=(release/name).resolve();assert added.is_relative_to(release)
            if added.exists():added.unlink()
        raise
    print(f'UPDATED_REGISTERED_DELIVERY {len(changed)} changed files; original backup preserved')
else:
    try:
        release.rename(backup)
    except PermissionError:
        # An open directory handle can prevent renaming while its files remain writable.
        # Preserve the entire old delivery first, then replace only verified staged files.
        old_files={str(p.relative_to(release)) for p in release.rglob('*') if p.is_file()}
        assert old_files.issubset(manifest), 'Old delivery contains extra files; inspect before replacing.'
        shutil.copytree(release,backup)
        try:shutil.copytree(stage,release,dirs_exist_ok=True)
        except Exception:
            shutil.copytree(backup,release,dirs_exist_ok=True)
            for relative in set(manifest)-old_files:
                added=(release/relative).resolve()
                assert added.is_relative_to(release)
                if added.is_file():added.unlink()
            raise
    else:
        try:stage.rename(release)
        except Exception:
            backup.rename(release)
            raise
assert {str(p.relative_to(release)):sha(p) for p in release.rglob('*') if p.is_file()}==manifest
out=root/'Documentation/LayoutFix-2026-10-20'
(out/'delivery-identity.json').write_text(json.dumps(dict(source=str(source),delivery=str(release),backup=str(backup),files=manifest),indent=2),encoding='utf-8')
print(f'DELIVERY_VERIFIED {len(manifest)} files; baseline preserved at {backup}')
if stage.exists():
    assert stage.parent==build and stage.name=='Windows-layout-staged'
    shutil.rmtree(stage)

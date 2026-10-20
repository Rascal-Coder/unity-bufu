import shutil
import json,hashlib
from pathlib import Path
root=Path(__file__).resolve().parents[2]
project=root/'Build/ParityProject'
baseline=root/'Documentation/LayoutFix-2026-10-20/BaselineSources'
if not baseline.exists():shutil.copytree(project/'Assets/Scripts',baseline)
shutil.copytree(root/'Assets/Scripts',project/'Assets/Scripts',dirs_exist_ok=True)
shutil.copytree(root/'Assets/Resources/CommerceArt',project/'Assets/Resources/CommerceArt',dirs_exist_ok=True)
if (root/'Assets/Resources/PackedUI').exists():
    shutil.copytree(root/'Assets/Resources/PackedUI',project/'Assets/Resources/PackedUI',dirs_exist_ok=True)
    migration=json.loads((root/'Documentation/SpriteSheetAudit-2026-10-20/migration-manifest.json').read_text(encoding='utf-8'))
    for item in migration['migrated']+migration['removed_duplicates']:
        old=(project/item['path']).resolve()
        assert old.is_relative_to((project/'Assets/Resources').resolve())
        if old.exists():
            assert hashlib.sha256(old.read_bytes()).hexdigest()==item['sha256'],old
            old.unlink()
            meta=old.with_suffix('.png.meta')
            if meta.exists():meta.unlink()
    shutil.copy2(root/'Assets/Editor/PackedUiTextureImporter.cs',project/'Assets/Editor/PackedUiTextureImporter.cs')
shutil.copy2(root/'Tools/LayoutFix/LayoutFixBuild.cs',project/'Assets/Editor/LayoutFixBuild.cs')
print('ISOLATED_UNITY_PROJECT_READY')

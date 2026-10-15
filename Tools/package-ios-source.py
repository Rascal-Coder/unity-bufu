"""Package the Unity source project for a Mac with iOS Build Support installed."""
from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED
import json

root = Path(__file__).resolve().parent.parent
destination = root / "Build/iOS/Bufu-iOS-Source.zip"
destination.parent.mkdir(parents=True, exist_ok=True)
files = []
for directory in ("Assets", "Packages", "ProjectSettings"):
    files.extend(path for path in (root / directory).rglob("*") if path.is_file())
for name in ("README.txt", "Tools/export-ios.command", "Documentation/iOS打包说明.txt",
             "Documentation/HonorSupportParity.txt", "Documentation/PromotionParity.md",
             "Documentation/validation.json", "Documentation/build-result.json"):
    path = root / name
    if path.is_file():
        files.append(path)
with ZipFile(destination, "w", compression=ZIP_DEFLATED, compresslevel=6) as archive:
    for path in sorted(files):
        archive.write(path, path.relative_to(root).as_posix())
with ZipFile(destination, "r") as archive:
    damaged = archive.testzip()
    if damaged is not None:
        raise RuntimeError("Source archive integrity check failed: " + damaged)
print(json.dumps({"source_archive": str(destination), "files": len(files),
                  "bytes": destination.stat().st_size, "ios_built": False}, ensure_ascii=False))

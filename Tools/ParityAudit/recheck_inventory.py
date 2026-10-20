import hashlib
import json
import sys
from pathlib import Path

root = Path(__file__).resolve().parents[2]
out = root / 'Documentation/OriginalParityAudit-2026-10-20'
baseline = json.loads((out / 'snapshot.json').read_text(encoding='utf-8-sig'))
previous = {x['path'].replace('\\', '/'): x['sha256'] for x in baseline['files']}
files = []
for folder, suffix in [('Assets/Scripts', '.cs'), ('Assets/Editor', '.cs'),
                       ('Assets/Resources/Config', '.json'), ('Reference/DecompiledCSharp', '.cs')]:
    for path in sorted((root / folder).rglob('*' + suffix)):
        data = path.read_bytes()
        relative = path.relative_to(root).as_posix()
        digest = hashlib.sha256(data).hexdigest()
        files.append(dict(path=relative, sha256=digest, bytes=len(data),
                          baseline='new' if relative not in previous else
                          'unchanged' if previous[relative] == digest else 'changed'))
configs = [x for x in files if x['path'].startswith('Assets/Resources/Config/')]
result = dict(baseline='snapshot.json', workspace=str(root),
              reference_package='2026-10-14-626', keep_english=True,
              config_count=len(configs), unchanged_configs=sum(x['baseline']=='unchanged' for x in configs),
              files=files)
(out / (sys.argv[1] if len(sys.argv)>1 else 'recheck-snapshot.json')).write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps(dict(files=len(files), configs=len(configs),
                      unchanged_configs=result['unchanged_configs']), ensure_ascii=False))

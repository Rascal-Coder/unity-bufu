import hashlib
import json
import sys
from datetime import datetime, timezone
from pathlib import Path

root = Path(__file__).resolve().parents[2]
audit = root / 'Documentation/OriginalParityAudit-2026-10-20'
fix = '--fix' in sys.argv
run_name = 'FixRun' if fix else 'RecheckRun'
run = audit / run_name
isolated = root / 'Build/ParityProject'
load = lambda path: json.loads(path.read_text(encoding='utf-8-sig'))
sha = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
models = load(run / 'model-checks.json')
ui = load(run / 'runtime-540x960/runtime-check.json')
build = load(run / 'audit-build.json')
sources = list((root / 'Assets/Scripts').rglob('*.cs')) + list((root / 'Assets/Editor').rglob('*.cs'))
mismatch = [p.relative_to(root).as_posix() for p in sources
            if not (isolated / p.relative_to(root)).is_file() or sha(p) != sha(isolated / p.relative_to(root))]
evidence = dict(checked_utc=datetime.now(timezone.utc).isoformat(), reference_package='2026-10-14-626',
                keep_english=True, runtime_original_package_observed=False,
                source_snapshot='fix-snapshot.json' if fix else 'recheck-snapshot.json', source_match_count=len(sources), source_mismatches=mismatch,
                model_checks=dict(editor=models['editor'], passed=models['passed'], failed=models['failed'],
                                  result=run_name+'/model-checks.json'),
                build=build, ui_checks=dict(passed=ui['passed'], checks=ui['checks'], width=ui['width'], height=ui['height'],
                                           result=run_name+'/runtime-540x960/runtime-check.json'),
                audit_player_sha256=sha(isolated / 'Build/ParityAuditPlayer/BufuRebuild_Data/Managed/Assembly-CSharp.dll'),
                probe_source_sha256=sha(root / 'Tools/ParityAudit/ParityRecheckProbe.cs'),
                screenshots=[dict(path=p.relative_to(audit).as_posix(),sha256=sha(p))
                             for p in sorted((run / 'runtime-540x960').glob('*.png'))])
(audit / ('fix-execution-evidence.json' if fix else 'recheck-execution-evidence.json')).write_text(json.dumps(evidence,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(dict(model_pass=models['passed'],model_fail=models['failed'],ui_pass=ui['passed'],ui_checks=ui['checks'],
                      screenshots=len(evidence['screenshots']),compared_sources=len(sources),source_mismatches=mismatch)))
if mismatch or models['failed'] or not ui['passed'] or build['result'] != 'Succeeded':
    raise SystemExit(1)

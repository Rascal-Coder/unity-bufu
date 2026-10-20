"""Build script-only UI updates with the pinned Unity player compiler response."""
import hashlib,json,shutil,subprocess,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
out=root/'Documentation/LayoutFix-2026-10-20'
out.mkdir(parents=True,exist_ok=True)
commercial='--commercial' in sys.argv
player=root/('Build/LayoutFixCommercialPlayer' if commercial else 'Build/LayoutFixPlayer')
if not player.exists():shutil.copytree(root/'Build/Windows',player)
project=root/'Build/ParityProject'
response=project/'Library/Bee/artifacts/1900b0aPDevDbg.dag/Assembly-CSharp.rsp'
lines=[]
for line in response.read_text(encoding='utf-8-sig').splitlines():
    if line.startswith(('-out:','-refout:','/additionalfile:','-analyzer:','/analyzer:')) or line.strip('"').endswith('.cs'):continue
    lines.append(line)
baseline='--baseline-guards' in sys.argv
sources=sorted((root/'Assets/Scripts').rglob('*.cs'))
if baseline:sources=[p if p.name.startswith('RuntimeVerification') else out/'BaselineSources'/p.relative_to(root/'Assets/Scripts') for p in sources]
lines.extend('"'+str(p)+'"' for p in sources)
assembly=player/'BufuRebuild_Data/Managed/Assembly-CSharp.dll'
lines.append('-out:"'+str(assembly)+'"')
rsp=out/'player.rsp';rsp.write_text('\n'.join(lines),encoding='utf-8')
editor=Path('E:/Unity/Editors/2022.3.62f3c1/Editor/Data')
proc=subprocess.run([str(editor/'NetCoreRuntime/dotnet.exe'),str(editor/'DotNetSdkRoslyn/csc.dll'),'@'+str(rsp)],cwd=project)
if proc.returncode:raise SystemExit(proc.returncode)
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
(out/('baseline-guard-identity.json' if baseline else 'commercial-identity.json' if commercial else 'build-identity.json')).write_text(json.dumps({'compiler':str(editor),'assembly_sha256':sha(assembly),'baseline_assembly_sha256':sha(root/'Build/Windows/BufuRebuild_Data/Managed/Assembly-CSharp.dll'),'sources':{str(p.relative_to(root)):sha(p) for p in sources},'resources_changed':commercial,'player':str(player)},indent=2),encoding='utf-8')
print('UI_PLAYER_COMPILE_OK '+str(player))

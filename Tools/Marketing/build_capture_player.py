"""Build a separate media-capture player; the normal delivery and human save are untouched."""
from pathlib import Path
import hashlib,json,shutil,subprocess
root=Path(__file__).resolve().parents[2]
player=root/'Build/GooglePlayCapturePlayer';source=root/'Build/Windows'
if player.exists():assert (player/'.marketing-capture-player').exists()
shutil.copytree(source,player,dirs_exist_ok=True)
(player/'.marketing-capture-player').write_text('Dedicated marketing capture build; never publish this player.')
project=root/'Build/ParityProject';response=project/'Library/Bee/artifacts/1900b0aPDevDbg.dag/Assembly-CSharp.rsp'
lines=[]
for line in response.read_text(encoding='utf-8-sig').splitlines():
    if line.startswith(('-out:','-refout:','/additionalfile:','-analyzer:','/analyzer:')) or line.strip('"').endswith('.cs'):continue
    lines.append(line)
sources=sorted((root/'Assets/Scripts').rglob('*.cs'))+[root/'Tools/Marketing/GooglePlayCapture.cs']
# RuntimeInitialize methods are registered at Unity build time. Route through the
# existing registered QA initializer in a capture-only source copy, not the game repo.
original=root/'Assets/Scripts/RuntimeVerification.cs';patched=root/'Build/GooglePlayCaptureSources/RuntimeVerification.cs';patched.parent.mkdir(parents=True,exist_ok=True)
text=original.read_text(encoding='utf-8');needle='static void Init(){if(GameController.VerificationMode)'
assert needle in text
text=text.replace(needle,'static void Init(){if(Environment.GetCommandLineArgs().Contains("--capture-marketing")){GooglePlayCapture.Begin();return;}if(GameController.VerificationMode)',1)
patched.write_text(text,encoding='utf-8');sources=[patched if p==original else p for p in sources]
lines.extend('"'+str(p)+'"' for p in sources)
assembly=player/'BufuRebuild_Data/Managed/Assembly-CSharp.dll';lines.append('-out:"'+str(assembly)+'"')
docs=root/'Marketing/GooglePlay/2026-10-21';docs.mkdir(parents=True,exist_ok=True)
rsp=docs/'capture-player.rsp';rsp.write_text('\n'.join(lines),encoding='utf-8')
editor=Path('E:/Unity/Editors/2022.3.62f3c1/Editor/Data')
subprocess.run([str(editor/'NetCoreRuntime/dotnet.exe'),str(editor/'DotNetSdkRoslyn/csc.dll'),'@'+str(rsp)],cwd=project,check=True)
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
(docs/'capture-identity.json').write_text(json.dumps(dict(assembly_sha256=sha(assembly),sources={str(p.relative_to(root)):sha(p) for p in sources},scope='Separate capture-only build, isolated verification save. Existing game UI and actors; tutorial pointers and unlock reminder labels hidden for photography.'),indent=2),encoding='utf-8')
print('MARKETING_CAPTURE_PLAYER_READY')

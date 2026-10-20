"""Compile changed scripts using the existing Unity Bee references, without importing the project."""
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
EDITOR = Path(r"C:\Program Files\Tuanjie\Hub\Editor\2022.3.62t16\Editor\Data")
OUT = ROOT / "Build" / "ParityCompile"
OUT.mkdir(parents=True, exist_ok=True)


def compile_assembly(name, sources):
    candidates = sorted((ROOT / "Library/Bee/artifacts").glob(f"*/{name}.rsp"))
    if not candidates:
        raise RuntimeError(f"No existing compiler references for {name}")
    lines = []
    old_sources = []
    for line in candidates[-1].read_text(encoding="utf-8-sig").splitlines():
        if line.strip('"').endswith(".cs"):
            path=Path(line.strip('"'))
            if (ROOT / path).is_file():
                old_sources.append(path)
            continue
        if line.startswith(("-out:", "-refout:", "/out:", "/refout:")):
            continue
        if name.endswith("-Editor") and ("Assembly-CSharp.dll" in line or "Assembly-CSharp.ref.dll" in line) and line.startswith(("-r:", "/reference:")):
            line = f'-r:"{OUT / "Assembly-CSharp.dll"}"'
        lines.append(line)
    lines.append(f'-out:"{OUT / (name + ".dll")}"')
    lines.extend(f'"{path.as_posix()}"' for path in sorted(set(old_sources + sources)))
    response = OUT / f"{name}.rsp"
    response.write_text("\n".join(lines), encoding="utf-8")
    result = subprocess.run([str(EDITOR / "NetCoreRuntime/dotnet.exe"), str(EDITOR / "DotNetSdkRoslyn/csc.dll"), f"@{response}"], cwd=ROOT)
    if result.returncode:
        sys.exit(result.returncode)


compile_assembly("Assembly-CSharp", [p.relative_to(ROOT) for p in (ROOT / "Assets/Scripts").rglob("*.cs")])
compile_assembly("Assembly-CSharp-Editor", [p.relative_to(ROOT) for p in (ROOT / "Assets/Editor").rglob("*.cs")])
print("PARITY_CSHARP_COMPILE_OK")

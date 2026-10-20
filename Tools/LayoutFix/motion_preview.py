"""Export the real runtime capture sequence with its recorded timing."""
from pathlib import Path
from PIL import Image

root=Path(__file__).resolve().parents[2]
folder=root/'Documentation/LayoutFix-2026-10-20/ReleaseStore390'
files=sorted(folder.glob('motion-*.png'))
times=[float(s) for s in (folder/'motion-times.txt').read_text().splitlines()]
assert len(files)==len(times)>2
frames=[Image.open(p).convert('RGB') for p in files]
palette=frames[0].convert('P',palette=Image.Palette.ADAPTIVE,colors=256)
frames=[p.quantize(palette=palette,dither=Image.Dither.NONE) for p in frames]
durations=[max(20,round(((times[i+1] if i+1<len(times) else max(8,times[i]+.2))-times[i])*1000)) for i in range(len(times))]
out=folder.parent/'shop-offer-motion.gif'
frames[0].save(out,save_all=True,append_images=frames[1:],duration=durations,loop=0,optimize=True)
print(f'RUNTIME_MOTION_PREVIEW {out} frames={len(frames)} duration_ms={sum(durations)}')

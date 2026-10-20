"""Compare static UI regions against pre-sheet runtime screenshots."""
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import numpy as np,json
root=Path(__file__).resolve().parents[2]
folder=root/'Documentation/LayoutFix-2026-10-20'
out=root/'Documentation/SpriteSheetAudit-2026-10-20'
cases=[
    ('Store390/01-store-diamonds.png','Diamond store',(20,190,370,548)),
    ('Store390/02-store-no-ads.png','No Ads store',(20,220,370,510)),
    ('Locale390/01-english-settings.png','Settings',(60,320,335,625)),
    ('Locale390/11-english-worlds.png','Coin World title',(94,220,296,280)),
    ('Locale390/15-english-evolution.png','Evolution title',(130,252,260,295)),
    ('Activities390/15a-local-activities-overview.png','Activity overview',(20,120,370,390))]
results=[]
for suffix,name,box in cases:
    before=folder/('NativeWords'+suffix);after=folder/('SpriteSheets'+suffix)
    a=Image.open(before).convert('RGB');b=Image.open(after).convert('RGB')
    assert a.size==b.size,(before,after)
    x=np.asarray(a.crop(box)).astype(np.float32);y=np.asarray(b.crop(box)).astype(np.float32)
    delta=np.abs(x-y);mae=float(delta.mean());p99=float(np.percentile(delta,99))
    results.append(dict(scene=name,before=str(before.relative_to(root)),after=str(after.relative_to(root)),region=box,mean_absolute_rgb_error_255=round(mae,4),p99_rgb_error_255=p99))
    # A targeted secondary guard; these static crops must remain visually close.
    assert mae<5,(name,mae)
result=dict(passed=True,interpretation='Same-size static UI crops. Mean RGB error on 0..255 scale; compression may alter pixels. Full runtime screenshots were also inspected. This does not measure animated content or frame rate.',cases=results)
(out/'visual-comparison.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
print(json.dumps(result,indent=2))

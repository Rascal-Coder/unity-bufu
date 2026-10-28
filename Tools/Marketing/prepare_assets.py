"""Archive generated art, trim transparent lettering, and export the Play feature graphic."""
from pathlib import Path
from PIL import Image,ImageOps
import hashlib,json,shutil
root=Path(__file__).resolve().parents[2];out=root/'Marketing/GooglePlay/2026-10-21'
specs=json.loads((out/'generation-sources.json').read_text(encoding='utf-8'))
assets=out/'Assets';originals=out/'Sources';upload=out/'Upload';assets.mkdir(exist_ok=True);originals.mkdir(exist_ok=True);upload.mkdir(exist_ok=True)
records=[];sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
for spec in specs['assets']:
    original=originals/(spec['id']+'-generated.png')
    if not original.exists():shutil.copy2(spec['source'],original)
    image=Image.open(original).convert('RGBA')
    if spec['transparent']:
        assert image.getextrema()[3][0]==0,spec['id']
        box=image.getchannel('A').point(lambda a:255 if a>8 else 0).getbbox();box=(max(0,box[0]-8),max(0,box[1]-8),min(image.width,box[2]+8),min(image.height,box[3]+8))
        image=image.crop(box);path=assets/(spec['id']+'.png');image.save(path)
    else:
        image=ImageOps.fit(image.convert('RGB'),(1024,500),method=Image.Resampling.LANCZOS)
        path=upload/'TycoonRush-feature-1024x500.png';image.save(path);image.save(upload/'TycoonRush-feature-1024x500.jpg',quality=95,subsampling=0)
    records.append(dict(id=spec['id'],text=spec['text'],source=str(original.relative_to(root)),asset=str(path.relative_to(root)),size=list(image.size),mode=image.mode,sha256=sha(path)))
manifest=dict(tool='Built-in image_gen.imagegen',brand='TycoonRush',source_spec='generation-sources.json',normalization='Transparent alpha trim with an 8px guard only; feature graphic proportional fit to 1024x500 and RGB export. Original generated sources retained. No screenshots are repainted.',assets=records)
(out/'art-manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
print('MARKETING_ART_READY',len(records))

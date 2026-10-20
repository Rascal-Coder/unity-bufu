"""Normalize generated word sprites with transparent trim/resize/padding only."""
from pathlib import Path
from PIL import Image
import hashlib,json
root=Path(__file__).resolve().parents[2]
entries=json.loads((Path(__file__).with_name('word_art_sources.json')).read_text(encoding='utf-8'))
assets=[]
for entry in entries:
    source=Path(entry['source']);image=Image.open(source).convert('RGBA')
    assert image.getextrema()[3][0]==0,source
    bounds=image.getchannel('A').point(lambda a:255 if a>80 else 0).getbbox()
    trimmed=image.crop(bounds)
    trimmed.thumbnail((1500,300),Image.Resampling.LANCZOS)
    output=Image.new('RGBA',(trimmed.width+16,trimmed.height+16))
    output.alpha_composite(trimmed,(8,8))
    path=root/'Assets/Resources/CommerceArt'/('word-'+entry['id']+'-native-v4.png')
    archived=root/'Assets/SourceArt/SpriteSheetSources/CommerceArt'/path.name
    if archived.exists():path=archived
    output.save(path)
    assets.append(dict(entry,asset=str(path.relative_to(root)),size=output.size,alpha_bounds=bounds,sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
report=dict(tool='Built-in image_gen.imagegen',normalization='Alpha trim, proportional resize and transparent padding only. No repainting.',assets=assets)
(root/'Documentation/LayoutFix-2026-10-20/word-art-expansion-manifest.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print('WORD_ART_READY',len(assets))

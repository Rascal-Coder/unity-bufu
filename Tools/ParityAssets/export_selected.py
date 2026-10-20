"""Use the project's read-only prefab exporter on selected original bundle objects.

Never invokes Exporter.run(), whose whole-library replacement is inappropriate here.
"""
import hashlib
import json
import shutil
import sys
from pathlib import Path

HERE=Path(__file__).resolve().parent
ROOT=HERE.parents[1]
sys.path.insert(0,str(HERE/'deps'))
sys.path.insert(0,str(ROOT/'Tools'))
import export_ui
from PIL import Image

output=HERE/'export'
export_ui.RAW=str(HERE/'raw')
export_ui.OUT=str(output/'UI')
export_ui.FONT_OUT=str(output/'Fonts')
export_ui.SPRITE_ASSET_OUT=str(output/'SpriteAssets')
for folder in ('UI/Sprites','UI/Textures','Fonts','SpriteAssets'):(output/folder).mkdir(parents=True,exist_ok=True)
exporter=export_ui.Exporter()
for path in (ROOT/'Assets/Resources/UI/Sprites').glob('*.png'):
    if '@' not in path.stem:
        with Image.open(path) as image:exporter.sprite_keys[path.stem]=hashlib.md5(image.tobytes()).hexdigest()[:8]

wanted={
    'Assets/GameRes/NewUI/Ad/perfabs/AdLaoHuJi.prefab':'Ad/AdLaoHuJi',
    'Assets/GameRes/NewUI/Ad/perfabs/AdDiamondMerchant.prefab':'Ad/AdDiamondMerchant',
    'Assets/GameRes/NewUI/video/gameTargetPanel.prefab':'video/gameTargetPanel',
    'Assets/GameRes/NewUI/fakevideo/prefabs/FakeVideoPanel.prefab':'fakevideo/FakeVideoPanel',
}
records=[]
for original,relative in wanted.items():
    matches=[(p,o) for p,o in exporter.containers if p.lower()==original.lower()]
    if len(matches)!=1:raise RuntimeError(f'{original}: found {len(matches)}')
    path,obj=matches[0]
    data=exporter.export_prefab(path,obj)
    dest=ROOT/'Assets/Resources/UI/Prefabs'/(relative+'.json')
    dest.parent.mkdir(parents=True,exist_ok=True)
    dest.write_text(json.dumps(export_ui.finite(data),ensure_ascii=False,separators=(',',':')),encoding='utf-8')
    record={'path':original,'resource':'UI/Prefabs/'+relative,'nodes':len(data['nodes']),'object_sha256':hashlib.sha256(obj.get_raw_data()).hexdigest()}
    records.append(record)
    print(json.dumps(record))

new_sprites=[]
for row in exporter.sprites.values():
    key=row.get('key')
    if not key:continue
    source=output/'UI/Sprites'/(key+'.png');dest=ROOT/'Assets/Resources/UI/Sprites'/(key+'.png')
    if dest.exists():
        with Image.open(source) as a,Image.open(dest) as b:
            if a.size!=b.size or a.convert('RGBA').tobytes()!=b.convert('RGBA').tobytes():raise RuntimeError('Existing sprite key mismatch: '+key)
    else:shutil.copy2(source,dest);new_sprites.append(row)
if new_sprites:
    path=ROOT/'Assets/Resources/UI/sprites.json';catalog=json.loads(path.read_text(encoding='utf-8'));catalog['sprites'].extend(new_sprites)
    # A missing texture may already have a catalog record from the old export.
    new_keys={r['key'] for r in new_sprites};seen=set()
    catalog['sprites']=[r for r in catalog['sprites'] if r['key'] not in new_keys or (r['key'] not in seen and not seen.add(r['key']))]
    path.write_text(json.dumps(catalog,ensure_ascii=False,separators=(',',':')),encoding='utf-8')
(HERE/'export-evidence.json').write_text(json.dumps({'prefabs':records,'new_sprite_count':len(new_sprites),'new_sprites':new_sprites},ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({'new_sprites':len(new_sprites)}))

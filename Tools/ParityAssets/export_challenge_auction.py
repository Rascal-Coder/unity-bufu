"""Download declared public bundles and add only five original local-game UI prefabs."""
import concurrent.futures
import hashlib
import json
import shutil
import sys
import urllib.request
from pathlib import Path

HERE=Path(__file__).resolve().parent
ROOT=HERE.parents[1]
WANTED={
    'Assets/GameRes/NewUI/Challenge/prefabs/ChallengePanel.prefab':'Challenge/ChallengePanel',
    'Assets/GameRes/NewUI/Challenge/prefabs/ChallengeMain.prefab':'Challenge/ChallengeMain',
    'Assets/GameRes/NewUI/Challenge/prefabs/helperSelectInfo.prefab':'Challenge/helperSelectInfo',
    'Assets/GameRes/NewUI/Auction/prefabs/auctionSeclect.prefab':'Auction/auctionSeclect',
    'Assets/GameRes/NewUI/Auction/prefabs/auctionPlayPanel.prefab':'Auction/auctionPlayPanel',
}
PREFIX='challenge-auction'
if '--activity-items' in sys.argv:
    PREFIX='challenge-auction-items'
    WANTED={
        'Assets/GameRes/NewUI/Challenge/prefabs/challengeUpgradeItem.prefab':'Challenge/challengeUpgradeItem',
        'Assets/GameRes/NewUI/Challenge/prefabs/selectSkill.prefab':'Challenge/selectSkill',
        'Assets/GameRes/NewUI/Auction/prefabs/selectSelectItem.prefab':'Auction/selectSelectItem',
        'Assets/GameRes/NewUI/Auction/prefabs/auctionItem.prefab':'Auction/auctionItem',
        'Assets/GameRes/NewUI/Auction/prefabs/auctionItemBox.prefab':'Auction/auctionItemBox',
    }
manifest=json.loads((ROOT/'Reference/package-manifest.json').read_text(encoding='utf-8'))
assets=[a for a in manifest['assets'] if a['AssetPath'] in WANTED]
assert len(assets)==len(WANTED)
bundles={b['FileName']:b for b in manifest['bundles']}
files={name for a in assets for name in [a['bundle_file'],*a['dependent_files']]}
raw=HERE/'raw';raw.mkdir(exist_ok=True)
evidence={'assets':assets}

def fetch(name):
    bundle=bundles[name];dest=raw/name
    data=dest.read_bytes() if dest.exists() else urllib.request.urlopen(bundle['url'],timeout=60).read()
    if len(data)!=bundle['FileSize']:raise ValueError('Size mismatch: '+name)
    if not dest.exists():dest.write_bytes(data)
    return {'file':name,'url':bundle['url'],'bytes':len(data),'sha256':hashlib.sha256(data).hexdigest()}

with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:evidence['downloads']=list(pool.map(fetch,sorted(files)))
(HERE/(PREFIX+'-downloads.json')).write_text(json.dumps(evidence,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({'downloaded_or_cached':len(files),'bytes':sum(r['bytes'] for r in evidence['downloads'])}),flush=True)
if '--download-only' in sys.argv:raise SystemExit(0)
sys.path.insert(0,str(HERE/'deps'));sys.path.insert(0,str(ROOT/'Tools'))
import export_ui
from PIL import Image
output=HERE/('export-'+PREFIX)
export_ui.RAW=str(raw);export_ui.OUT=str(output/'UI')
export_ui.FONT_OUT=str(output/'Fonts');export_ui.SPRITE_ASSET_OUT=str(output/'SpriteAssets')
for folder in ('UI/Sprites','UI/Textures','Fonts','SpriteAssets'):(output/folder).mkdir(parents=True,exist_ok=True)
exporter=export_ui.Exporter()
for path in (ROOT/'Assets/Resources/UI/Sprites').glob('*.png'):
    if '@' not in path.stem:
        with Image.open(path) as image:exporter.sprite_keys[path.stem]=hashlib.md5(image.tobytes()).hexdigest()[:8]
evidence['prefabs']=[]
for original,resource in WANTED.items():
    matches=[(p,o) for p,o in exporter.containers if p.lower()==original.lower()]
    if len(matches)!=1:raise RuntimeError(f'{original}: {len(matches)} matches')
    path,obj=matches[0];data=exporter.export_prefab(path,obj)
    dest=ROOT/'Assets/Resources/UI/Prefabs'/(resource+'.json');dest.parent.mkdir(parents=True,exist_ok=True)
    if dest.exists():raise FileExistsError(str(dest))
    dest.write_text(json.dumps(export_ui.finite(data),ensure_ascii=False,separators=(',',':')),encoding='utf-8')
    row={'original':path,'resource':'UI/Prefabs/'+resource,'nodes':len(data['nodes']),'sha256':hashlib.sha256(obj.get_raw_data()).hexdigest()}
    evidence['prefabs'].append(row);print(json.dumps(row),flush=True)
new_sprites=[]
for row in exporter.sprites.values():
    key=row.get('key')
    if not key:continue
    source=output/'UI/Sprites'/(key+'.png');dest=ROOT/'Assets/Resources/UI/Sprites'/(key+'.png')
    if dest.exists():
        with Image.open(source) as a,Image.open(dest) as b:
            if a.size!=b.size or a.convert('RGBA').tobytes()!=b.convert('RGBA').tobytes():raise RuntimeError('Sprite key conflict: '+key)
    else:shutil.copy2(source,dest);new_sprites.append(row)
if new_sprites:
    path=ROOT/'Assets/Resources/UI/sprites.json';catalog=json.loads(path.read_text(encoding='utf-8'))
    keys={r['key'] for r in new_sprites};catalog['sprites']=[r for r in catalog['sprites'] if r['key'] not in keys]+new_sprites
    path.write_text(json.dumps(catalog,ensure_ascii=False,separators=(',',':')),encoding='utf-8')
evidence['new_sprites']=new_sprites
(HERE/(PREFIX+'-export.json')).write_text(json.dumps(evidence,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({'new_sprites':len(new_sprites)}),flush=True)

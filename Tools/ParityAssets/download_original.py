"""Fetch only manifest-selected public original prefabs and declared dependencies."""
import concurrent.futures
import hashlib
import json
import urllib.request
from pathlib import Path

ROOT=Path(__file__).resolve().parents[2]
HERE=Path(__file__).resolve().parent
manifest=json.loads((ROOT/'Reference/package-manifest.json').read_text(encoding='utf-8'))
names={'AdLaoHuJi.prefab','AdDiamondMerchant.prefab','gameTargetPanel.prefab','FakeVideoPanel.prefab','coins_tornado.prefab','coins_tornado2.prefab','magic_ani.prefab','lanqiu.prefab','dashehandani.prefab','aixinqiu.prefab','iron_bullet.prefab','iron_bullet2.prefab','iron_boom.prefab','iron_boom2.prefab','zuqiu.prefab','dushen_DiceMove.prefab'}
assets=[a for a in manifest['assets'] if a['AssetPath'].split('/')[-1] in names]
files={a['bundle_file'] for a in assets}
for asset in assets:files.update(asset['dependent_files'])
bundles={b['FileName']:b for b in manifest['bundles']}
raw=HERE/'raw';raw.mkdir(parents=True,exist_ok=True)
(HERE/'selected-assets.json').write_text(json.dumps(assets,ensure_ascii=False,indent=2),encoding='utf-8')

def fetch(name):
    bundle=bundles[name];path=raw/name
    data=path.read_bytes() if path.exists() else urllib.request.urlopen(bundle['url'],timeout=60).read()
    if len(data)!=bundle['FileSize']:raise ValueError(f"Size mismatch: {name}: {len(data)} != {bundle['FileSize']}")
    if not path.exists():path.write_bytes(data)
    return {'file':name,'url':bundle['url'],'bytes':len(data),'sha256':hashlib.sha256(data).hexdigest()}

if __name__=='__main__':
    with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:records=list(pool.map(fetch,sorted(files)))
    (HERE/'downloads.json').write_text(json.dumps(records,indent=2),encoding='utf-8')
    print(json.dumps({'bundles':len(records),'bytes':sum(r['bytes'] for r in records),'raw':str(raw)}))

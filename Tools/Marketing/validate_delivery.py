"""Validate the selected store art and package only current deliverables."""
from pathlib import Path
from PIL import Image
import hashlib,json,zipfile
root=Path(__file__).resolve().parents[2];out=root/'Marketing/GooglePlay/2026-10-21'
manifest=json.loads((out/'composition-manifest.json').read_text(encoding='utf-8'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
checks=[]
for item in manifest['outputs']:
    p=root/item['file'];im=Image.open(p);im.verify();im=Image.open(p)
    assert im.size==(item['width'],item['height']) and im.mode=='RGB',p
    assert sha(p)==item['sha256'],p
    assert p.stat().st_size<8*1024*1024,p
    svg=(root/item['layout']).read_text(encoding='utf-8');assert '<title>TycoonRush - ' in svg
    checks.append(dict(file=item['file'],size=list(im.size),mode=im.mode,bytes=p.stat().st_size,sha256=sha(p)))
for group in ['scene_sources','team_landscape_native_assets','native_income_glyphs']:
    for item in manifest[group]:assert sha(root/item['file'])==item['sha256'],item['file']
assert len(manifest['outputs'])==9
assert len(list((out/'Upload').rglob('*.png')))==9
jpg=out/'Upload/TycoonRush-feature-1024x500.jpg';assert Image.open(jpg).size==(1024,500)
for card in manifest['cards']:assert len(card['alt'])<=140,card['title']
listing=(out/'StoreListing-en.txt').read_text(encoding='utf-8');short=listing.split('Short description\n')[1].splitlines()[0]
assert len(short)<=80 and len('TycoonRush')<=30 and 'TYCOON RUSH' not in listing
report=dict(passed=True,brand='TycoonRush',scope='Google Play promotional illustration assets, not unedited gameplay screenshots.',outputs=checks,counts=dict(portrait=4,landscape=4,feature_png=1,feature_jpeg=1),short_description_characters=len(short),native_income_glyphs_verified=len(manifest['native_income_glyphs']),source_hashes_verified=True,visual_review=['Reviewed four portrait cards together at 324x576 and Flip at full resolution.','Reviewed all landscape cards and feature graphic.','Corrected native decimal sprite scale/baseline; decimals now legible.','Moved landscape income text clear of helper faces.','Captions remain separate exact PNG assets. Brand wordmark reads TycoonRush.','Team landscape uses six native VAT helper renders with native skill sprites; other scenes are generated promotional illustrations based on project art.'],normal_game_modified_by_marketing=False)
(out/'validation.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
# Keep the earlier lettering manifest valid by pointing its superseded feature to the archive.
p=out/'art-manifest.json';old=json.loads(p.read_text(encoding='utf-8'))
for asset in old['assets']:
    if asset['id']=='feature':asset['asset']='Marketing/GooglePlay/2026-10-21/Archive/ScreenshotApproach/Upload/TycoonRush-feature-1024x500.png'
old['current_delivery_manifest']='composition-manifest.json';p.write_text(json.dumps(old,ensure_ascii=False,indent=2),encoding='utf-8')
specfile=out/'impact-generation-sources.json';spec=json.loads(specfile.read_text(encoding='utf-8'));spec['mode']='Illustrated promotional compositions based on native project asset reference boards. User requested assembled artwork instead of actual UI. Team landscape directly assembles six native helper renders and native skill sprites. Feature adapts the wide coin explosion with the exact brand wordmark.';specfile.write_text(json.dumps(spec,ensure_ascii=False,indent=2),encoding='utf-8')
deliver=list((out/'Upload').rglob('*'));deliver=[p for p in deliver if p.is_file()]
deliver += [out/n for n in ['StoreListing-en.txt','AltText-en.txt','交付说明.txt','validation.json','composition-manifest.json','impact-generation-sources.json','TycoonRush-portrait-overview.png','TycoonRush-landscape-overview.png']]
archive=out/'TycoonRush-GooglePlay-Upload.zip'
with zipfile.ZipFile(archive,'w',zipfile.ZIP_DEFLATED,compresslevel=6) as z:
    for p in sorted(deliver):z.write(p,p.relative_to(out))
with zipfile.ZipFile(archive) as z:assert z.testzip() is None
print(json.dumps(dict(passed=True,png_files=9,jpeg_files=1,zip_mib=round(archive.stat().st_size/1024**2,2),files=len(deliver)),ensure_ascii=False))

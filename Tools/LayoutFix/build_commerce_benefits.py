"""Normalize authored benefit icons and extend the shared UI index; no loose runtime PNGs."""
from pathlib import Path
from PIL import Image
import json,hashlib,uuid
root=Path(__file__).resolve().parents[2]
specs=json.loads(Path(__file__).with_name('commerce_benefit_sources.json').read_text(encoding='utf-8'))
source=root/'Assets/SourceArt/CommerceBenefits'
source.mkdir(parents=True,exist_ok=True)
docs=root/'Documentation/NoAdsBenefits-2026-10-21';docs.mkdir(parents=True,exist_ok=True)
sheet_name='commerce-benefits-v1';sheet_path=root/'Assets/Resources/PackedUI'/(sheet_name+'.png')
sheet=Image.new('RGBA',(792,264));entries=[];records=[]
sha=lambda path:hashlib.sha256(path.read_bytes()).hexdigest()
for i,spec in enumerate(specs):
    key='benefit-'+spec['id']+'-native-v1';path=source/(key+'.png')
    if not path.exists():
        original=Image.open(spec['generated_source']).convert('RGBA')
        assert original.getextrema()[3][0]==0
        box=original.getchannel('A').point(lambda a:255 if a>8 else 0).getbbox()
        icon=original.crop(box);icon.thumbnail((236,236),Image.Resampling.LANCZOS)
        normalized=Image.new('RGBA',(256,256));normalized.alpha_composite(icon,((256-icon.width)//2,(256-icon.height)//2));normalized.save(path)
    normalized=Image.open(path).convert('RGBA');assert normalized.size==(256,256)
    x=4+i*264;sheet.paste(normalized,(x,4))
    entry=dict(key='CommerceArt/'+key,name=key,sheet=sheet_name,rect=[x,4,256,256],pivot=[.5,.5],border=[0,0,0,0],ppu=100)
    entries.append(entry);records.append(dict(spec,asset=str(path.relative_to(root)),size=[256,256],sha256=sha(path),resource_key=entry['key'],packed_rect=entry['rect']))
sheet.save(sheet_path)
index_path=root/'Assets/Resources/PackedUI/index.json';index=json.loads(index_path.read_text(encoding='utf-8'))
keys={e['key'] for e in entries};index['entries']=[e for e in index['entries'] if e['key'] not in keys]+entries
index_path.write_text(json.dumps(index,separators=(',',':')),encoding='utf-8')
for folder in [source.parent,source]:
    meta=folder.with_name(folder.name+'.meta')
    if not meta.exists():meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n',encoding='utf-8')
for path in source.glob('*.png'):
    meta=path.with_suffix('.png.meta')
    if not meta.exists():meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\nTextureImporter:\n  serializedVersion: 13\n  textureType: 0\n  alphaIsTransparency: 1\n  mipmaps:\n    enableMipMap: 0\n',encoding='utf-8')
report=dict(tool='Built-in image_gen.imagegen',reference='Existing no-ads pass and native diamond pouch; user commercial screenshot informs visual hierarchy, not copied art or pricing.',assets=records,sheet=str(sheet_path.relative_to(root)),sheet_size=[792,264],sheet_sha256=sha(sheet_path),normalization='Alpha trim, proportional fit to 236px and transparent padding to 256px only. Four-pixel external guards. No standalone runtime icon textures.',rgba_pixel_bytes=792*264*4,bc7_pixel_bytes=792*264)
(docs/'benefit-art-manifest.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print('COMMERCE_BENEFIT_SHEET_READY',len(entries),'icons; total sprites',len(index['entries']))

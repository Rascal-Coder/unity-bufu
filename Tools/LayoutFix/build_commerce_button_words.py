"""Normalize generated purchase state lettering and pack one guarded sprite page."""
from pathlib import Path
from PIL import Image
import hashlib,json,uuid
root=Path(__file__).resolve().parents[2]
specs=json.loads(Path(__file__).with_name('commerce_button_word_sources.json').read_text(encoding='utf-8'))
source=root/'Assets/SourceArt/CommerceButtonWords';source.mkdir(parents=True,exist_ok=True)
docs=root/'Documentation/PurchaseButtons-2026-10-21';docs.mkdir(parents=True,exist_ok=True)
sheet_name='commerce-purchase-words-v1';items=[]
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
for spec in specs:
    name='word-purchase-'+spec['id']+'-native-v4';path=source/(name+'.png')
    if not path.exists():
        image=Image.open(spec['generated_source']).convert('RGBA');assert image.getextrema()[3][0]==0
        box=image.getchannel('A').point(lambda a:255 if a>32 else 0).getbbox()
        image=image.crop(box);image.thumbnail((768,128),Image.Resampling.LANCZOS)
        output=Image.new('RGBA',(image.width+16,image.height+16));output.alpha_composite(image,(8,8));output.save(path)
    items.append((spec,name,path,Image.open(path).convert('RGBA')))
width=(max(i[3].width for i in items)+8+3)//4*4
height=(sum(i[3].height+8 for i in items)+3)//4*4
sheet=Image.new('RGBA',(width,height));entries=[];records=[];y=4
for spec,name,path,image in items:
    sheet.paste(image,(4,y));entry=dict(key='CommerceArt/'+name,name=name,sheet=sheet_name,rect=[4,height-y-image.height,image.width,image.height],pivot=[.5,.5],border=[0,0,0,0],ppu=100)
    entries.append(entry);records.append(dict(spec,asset=str(path.relative_to(root)),size=list(image.size),sha256=sha(path),resource_key=entry['key'],packed_rect=entry['rect']));y+=image.height+8
sheet_path=root/'Assets/Resources/PackedUI'/(sheet_name+'.png');sheet.save(sheet_path)
for (_,name,path,image),entry in zip(items,entries):
    x,bottom,w,h=entry['rect'];top=height-bottom-h
    assert sheet.crop((x,top,x+w,top+h)).tobytes()==image.tobytes(),name
index_path=root/'Assets/Resources/PackedUI/index.json';index=json.loads(index_path.read_text(encoding='utf-8'));keys={e['key'] for e in entries}
index['entries']=[e for e in index['entries'] if e['key'] not in keys]+entries;index_path.write_text(json.dumps(index,separators=(',',':')),encoding='utf-8')
for folder in [source.parent,source]:
    meta=folder.with_name(folder.name+'.meta')
    if not meta.exists():meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n',encoding='utf-8')
for path in source.glob('*.png'):
    meta=path.with_suffix('.png.meta')
    if not meta.exists():meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\nTextureImporter:\n  serializedVersion: 13\n  textureType: 0\n  alphaIsTransparency: 1\n  mipmaps:\n    enableMipMap: 0\n',encoding='utf-8')
report=dict(tool='Built-in image_gen.imagegen',reference='Existing word-back-native-v4.png: rounded ivory letters and dark brown outlines.',normalization='Alpha trim, proportional resize to at most 768x128, 8px transparent padding. No repainting. Shared runtime sheet with 4px outer guards. Bottom-left Unity sprite coordinates are verified against original RGBA pixels.',pixel_integrity_verified=True,assets=records,sheet=str(sheet_path.relative_to(root)),sheet_size=[width,height],sheet_sha256=sha(sheet_path))
(docs/'word-art-manifest.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print('PURCHASE_WORD_SHEET_READY',len(entries),'words;',width,height,'total sprites',len(index['entries']))

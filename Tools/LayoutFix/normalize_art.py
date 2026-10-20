"""Trim transparent padding and normalize generated sprites without repainting them."""
from pathlib import Path
from PIL import Image
import json, hashlib

root=Path(__file__).resolve().parents[2]
generated=Path('C:/Users/aaa/.codex/generated_images/01a157e5-fb8b-7423-acd7-bb0a9c8a1882')
target=root/'Assets/Resources/CommerceArt'
files={
    'diamond-vault-native-v2':('exec-58d87b8e-b55d-4536-87eb-8ce2bd508521.png',(1024,1024)),
    'no-ads-pass-native-v2':('exec-d0e05dfe-35f2-4936-ab6c-f60a61134b3a.png',(1024,1024)),
    'shop-buy-native-v2':('exec-bd463136-32c2-494e-a774-5350a4aa9478.png',(720,216)),
    'shop-disabled-native-v2':('exec-6ca7ff58-a6fe-4b8b-ab90-d5fa50e98229.png',(720,216)),
    'shop-tab-native-v2':('exec-ebd5c342-90c6-460f-af24-28df07c82b67.png',(720,216)),
    'fragment-pack-native-v2':('exec-6a2c24b2-daa7-4917-8a48-59254ea9b88e.png',(1024,1024)),
    'title-diamonds-native-v3':('exec-93f8411e-60ec-475d-9961-00225d9c8771.png',(1024,180)),
    'title-no-ads-native-v3':('exec-ffe5ac70-c311-4688-8d18-ecda429c1055.png',(1024,180)),
    'title-fragments-native-v3':('exec-26c49968-2f93-4d70-a7fe-2e9905f58b4c.png',(1024,180)),
    'label-no-ads-native-v3':('exec-316db7fb-448a-4daf-99ff-f8000c821767.png',(768,160)),
    'label-diamonds-native-v3':('exec-9e29b0a2-1032-4a6a-b0c7-a1479af711ad.png',(768,160)),
    'label-fragments-native-v3':('exec-0034b2c2-cfe5-4b45-b931-50984988dc42.png',(768,160)),
}
report=[]
for key,(name,size) in files.items():
    source=generated/name
    image=Image.open(source).convert('RGBA')
    assert image.getextrema()[3][0]==0, f'{name}: alpha required'
    alpha=image.getchannel('A')
    bounds=alpha.point(lambda a:255 if a>80 else 0).getbbox() if size[0]==720 or key.startswith(('title-','label-')) else alpha.getbbox()
    trimmed=image.crop(bounds)
    margin=12 if size[0]==1024 else 2
    fit=(size[0]-2*margin,size[1]-2*margin)
    if key.startswith('title-'):fit=(1000,128)
    if key.startswith('label-'):fit=(744,150 if 'fragments' in key else 132)
    trimmed.thumbnail(fit,Image.Resampling.LANCZOS)
    output=Image.new('RGBA',size)
    output.alpha_composite(trimmed,((size[0]-trimmed.width)//2,(size[1]-trimmed.height)//2))
    path=target/(key+'.png')
    archived=root/'Assets/SourceArt/SpriteSheetSources/CommerceArt'/(key+'.png')
    if archived.exists():path=archived
    output.save(path)
    report.append(dict(asset=str(path.relative_to(root)),source=str(source),size=size,source_alpha_bounds=bounds,sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
manifest=dict(tool='Built-in image_gen.imagegen',art_direction='Existing game: dark comic outlines, angular few-tone shading, cyan diamonds, purple wood, warm gold trim, native green actions. Referenced original shop bag, chest, spin button, native word art and screenshot. Gold page word art and white outlined side labels are baked images; prices remain live.',normalization='Transparent trim, proportional resize and alpha-preserving padding only.',assets=report)
(root/'Documentation/LayoutFix-2026-10-20/native-art-manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2))

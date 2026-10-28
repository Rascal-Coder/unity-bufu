"""Pack lossless UI sheets and archive migrated sources outside Resources."""
from pathlib import Path
from collections import defaultdict,Counter
from PIL import Image
import json,hashlib,re,math,shutil,sys

root=Path(__file__).resolve().parents[2]
resources=root/'Assets/Resources'
archive=root/'Assets/SourceArt/SpriteSheetSources'
output=resources/'PackedUI'
docs=root/'Documentation/SpriteSheetAudit-2026-10-20'
inventory=json.loads((docs/'inventory.json').read_text(encoding='utf-8'))
metas={s['key']:s for s in json.loads((resources/'UI/sprites.json').read_text(encoding='utf-8'))['sprites']}
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()

def locate(r):
    p=root/r['path']
    return p if p.exists() else archive/p.relative_to(resources)

# Do not migrate PNGs referenced by native serialized materials, sprites or scenes.
guid_paths={}
for r in inventory['records']:
    p=locate(r).with_suffix('.png.meta')
    if p.exists():
        match=re.search(r'^guid: (\w+)',p.read_text(encoding='utf-8'),re.M)
        if match:guid_paths[match[1]]=r['path']
referenced=set()
for p in (root/'Assets').rglob('*'):
    if p.suffix.lower() not in ('.asset','.mat','.prefab','.unity','.controller'):continue
    for guid in re.findall(r'guid: ([a-fA-F0-9]{32})',p.read_text(encoding='utf-8',errors='ignore')):
        if guid in guid_paths:referenced.add(guid_paths[guid])

groups=defaultdict(list);excluded=[];duplicates=[]
for r in inventory['records']:
    rel=r['path'];key=Path(rel).relative_to('Assets/Resources').as_posix()[:-4]
    if rel in referenced:
        excluded.append(dict(path=rel,reason='Serialized native reference'));continue
    if key.startswith('Art/UI/'):
        peers=[p for g in inventory['duplicate_pixels'] if rel in g['paths'] for p in g['paths'] if p.startswith('Assets/Resources/UI/Sprites/')]
        assert peers,rel
        duplicates.append(dict(path=rel,sha256=sha(locate(r)),identical_to=peers));continue
    if key.startswith('CommerceArt/'):
        if r['group']=='commerce_illustrations':continue
        family='commerce-controls' if r['group']=='commerce_buttons' else 'word-art'
        spec=dict(w=r['width'],h=r['height'],ppu=100,pivot=[.5,.5],border=[0,0,0,0])
    elif key.startswith('UI/Sprites/'):
        spec=metas.get(r['name'],dict(w=r['width'],h=r['height'],ppu=100,pivot=[.5,.5],border=[0,0,0,0]))
        if max(spec['w'],spec['h'])>1024 or spec['w']*spec['h']>650000:
            excluded.append(dict(path=rel,reason='Large logical sprite; retain separate compressed texture'));continue
        if r['width']>spec['w'] or r['height']>spec['h']:
            excluded.append(dict(path=rel,reason='Runtime downsample; retain existing sampling behavior'));continue
        if '/en/' in key:family='english-ui'
        elif r['group']=='effect_frames':family='fx-'+re.sub(r'_\d+$','',r['name'])
        elif r['group']=='item_currency_icons':family='items'
        elif r['name'].startswith('challenge_'):family='activities'
        elif r['name'].startswith(('shop_','ad_')):family='shop'
        elif r['name'].startswith('world_'):family='world'
        elif r['name'].startswith(('ty_','zdjm_','zm_','zjm_')):family='hud'
        elif r['name'].startswith('talent_'):family='talents'
        elif r['name'].startswith(('wupin_','bs_','coin_','dice_')):family='upgrades'
        else:family='menus'
    else:continue
    groups[family].append(dict(r,key=key,meta=spec))

def pack(items,edge):
    pages=[]
    for r in sorted(items,key=lambda r:max(r['meta']['w'],r['meta']['h'])*100000+min(r['meta']['w'],r['meta']['h']),reverse=True):
        w,h=r['meta']['w']+8,r['meta']['h']+8;best=None
        assert max(w,h)<=edge,(r['path'],w,h)
        for pi,page in enumerate(pages):
            for x,y,fw,fh in page['free']:
                if w<=fw and h<=fh:
                    score=(min(fw-w,fh-h),max(fw-w,fh-h),pi,y,x)
                    if best is None or score<best[0]:best=(score,pi,x,y)
        if best is None:pages.append(dict(free=[(0,0,edge,edge)],entries=[]));pi=len(pages)-1;x=y=0
        else:_,pi,x,y=best
        page=pages[pi];free=[]
        for fx,fy,fw,fh in page['free']:
            if x>=fx+fw or x+w<=fx or y>=fy+fh or y+h<=fy:free.append((fx,fy,fw,fh));continue
            if x>fx:free.append((fx,fy,x-fx,fh))
            if x+w<fx+fw:free.append((x+w,fy,fx+fw-x-w,fh))
            if y>fy:free.append((fx,fy,fw,y-fy))
            if y+h<fy+fh:free.append((fx,y+h,fw,fy+fh-y-h))
        page['free']=[a for i,a in enumerate(free) if not any(j!=i and b[0]<=a[0] and b[1]<=a[1] and b[0]+b[2]>=a[0]+a[2] and b[1]+b[3]>=a[1]+a[3] and (a!=b or j<i) for j,b in enumerate(free))]
        page['entries'].append(dict(r,x=x+4,y=y+4))
    for page in pages:
        page['w']=math.ceil(max(r['x']+r['meta']['w']+4 for r in page['entries'])/4)*4
        page['h']=math.ceil(max(r['y']+r['meta']['h']+4 for r in page['entries'])/4)*4
    return pages

all_pages=[]
for family,items in sorted(groups.items()):
    choices=[pack(items,e) for e in [1024,2048] if all(max(r['meta']['w'],r['meta']['h'])+8<=e for r in items)]
    pages=min(choices,key=lambda pages:(sum(p['w']*p['h'] for p in pages),len(pages)))
    all_pages.extend(dict(page,family=family,name=family+'-'+str(i).zfill(2)) for i,page in enumerate(pages))

plan=dict(pages=[dict(name=p['name'],width=p['w'],height=p['h'],count=len(p['entries'])) for p in all_pages],sprites=sum(len(p['entries']) for p in all_pages),duplicates=len(duplicates),excluded=excluded,serialized_references=sorted(referenced))
print(json.dumps(plan,ensure_ascii=False,indent=2) if '--apply' not in sys.argv else json.dumps(dict(sprites=plan['sprites'],pages=len(all_pages),duplicates=len(duplicates))))
if '--apply' not in sys.argv:sys.exit(0)
output.mkdir(parents=True,exist_ok=True)
entries=[];migrated=[]
for page in all_pages:
    canvas=Image.new('RGBA',(page['w'],page['h']))
    for r in page['entries']:
        p=locate(r);assert sha(p)==r['sha256'],p
        im=Image.open(p).convert('RGBA');m=r['meta'];logical=Image.new('RGBA',(m['w'],m['h']))
        # SpriteMeta places a cropped export at floor((logical - crop)/2), in Unity's bottom-left convention.
        ox=(m['w']-im.width)//2;oy=m['h']-im.height-(m['h']-im.height)//2
        logical.paste(im,(ox,oy));x,y=r['x'],r['y'];canvas.paste(logical,(x,y))
        # Extrude two edge pixels into the four-pixel guard; the remaining gap protects compression blocks.
        canvas.paste(logical.crop((0,0,m['w'],1)).resize((m['w'],2)),(x,y-2))
        canvas.paste(logical.crop((0,m['h']-1,m['w'],m['h'])).resize((m['w'],2)),(x,y+m['h']))
        canvas.paste(logical.crop((0,0,1,m['h'])).resize((2,m['h'])),(x-2,y))
        canvas.paste(logical.crop((m['w']-1,0,m['w'],m['h'])).resize((2,m['h'])),(x+m['w'],y))
        entries.append(dict(key=r['key'],name=r['name'],sheet=page['name'],rect=[x,page['h']-y-m['h'],m['w'],m['h']],pivot=m['pivot'],border=m['border'],ppu=m['ppu'],rgba_sha256=hashlib.sha256(logical.tobytes()).hexdigest()))
        migrated.append(dict(path=r['path'],sha256=r['sha256']))
    path=output/(page['name']+'.png');canvas.save(path)
    # Verify every region before the original source is moved.
    for r in page['entries']:
        e=next(e for e in entries if e['key']==r['key']);m=r['meta']
        assert hashlib.sha256(canvas.crop((r['x'],r['y'],r['x']+m['w'],r['y']+m['h'])).tobytes()).hexdigest()==e['rgba_sha256']
(output/'index.json').write_text(json.dumps(dict(version=1,padding=4,entries=entries),separators=(',',':')),encoding='utf-8')
manifest=dict(plan,migrated=migrated,removed_duplicates=duplicates,index_sha256=sha(output/'index.json'),page_sha256={p['name']:sha(output/(p['name']+'.png')) for p in all_pages},normalization='No image resampling or trimming. Original SpriteMeta logical dimensions, bottom-left padding, pivot, ppu and border preserved. Four-pixel guards, two-pixel extrusion, no rotation. Multiples of four, maximum 2048.')
(docs/'migration-manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
for item in migrated+duplicates:
    src=(root/item['path']).resolve();dst=(archive/src.relative_to(resources)).resolve()
    assert src.is_relative_to(resources.resolve()) and dst.is_relative_to(archive.resolve())
    if src.exists():
        assert sha(src)==item['sha256'];dst.parent.mkdir(parents=True,exist_ok=True)
        if dst.exists():assert sha(dst)==sha(src);src.unlink()
        else:shutil.move(str(src),str(dst))
        sm=src.with_suffix('.png.meta');dm=dst.with_suffix('.png.meta')
        if sm.exists():
            if dm.exists():assert sha(dm)==sha(sm);sm.unlink()
            else:shutil.move(str(sm),str(dm))
    else:assert dst.exists() and sha(dst)==item['sha256']
print('SPRITE_SHEETS_READY',len(entries),'sprites',len(all_pages),'pages',len(duplicates),'duplicate source textures archived')
if (root/'Tools/LayoutFix/commerce_benefit_sources.json').exists():
    import subprocess
    subprocess.run([sys.executable,str(root/'Tools/LayoutFix/build_commerce_benefits.py')],check=True)
if (root/'Tools/LayoutFix/commerce_button_word_sources.json').exists():
    import subprocess
    subprocess.run([sys.executable,str(root/'Tools/LayoutFix/build_commerce_button_words.py')],check=True)

"""Read-only texture inventory and atlas feasibility audit. No asset changes."""
from pathlib import Path
from PIL import Image
from collections import defaultdict,Counter
import hashlib,json,re,csv,math

root=Path(__file__).resolve().parents[2]
out=root/'Documentation/SpriteSheetAudit-2026-10-20';out.mkdir(parents=True,exist_ok=True)
log=(root/'Documentation/LayoutFix-2026-10-20/unity-word-expansion.log').read_text(encoding='utf-8',errors='replace')
built={m[3]:float(m[1])*dict(kb=1024,mb=1024**2,b=1)[m[2]] for m in re.finditer(r'^\s*([\d.]+)\s+(kb|mb|b)\s+[^\r\n]*?\s(Assets/[^\r\n]+)',log,re.M)}
records=[];pixels=defaultdict(list);files=defaultdict(list)
for path in sorted((root/'Assets/Resources').rglob('*.png')):
    rel=path.relative_to(root).as_posix()
    with Image.open(path) as source:
        im=source.convert('RGBA');w,h=im.size;bounds=im.getchannel('A').getbbox()
        digest=hashlib.sha256(str(im.size).encode()+im.tobytes()).hexdigest()
        has_alpha=im.getchannel('A').getextrema()[0]<255
    raw=path.read_bytes();meta=path.with_suffix('.png.meta').read_text(encoding='utf-8') if path.with_suffix('.png.meta').exists() else ''
    def value(key):
        m=re.search(r'^  '+key+r': (.+)$',meta,re.M);return m[1] if m else None
    tw,th=(bounds[2]-bounds[0],bounds[3]-bounds[1]) if bounds else (1,1)
    rec=dict(path=rel,name=path.stem,width=w,height=h,trim_width=tw,trim_height=th,alpha_bounds=bounds,has_alpha=has_alpha,transparent_border_fraction=1-tw*th/(w*h),png_bytes=len(raw),rgba_bytes=w*h*4,build_bytes=built.get(rel,0),type=value('textureType'),max_size=value('maxTextureSize'),readable=value('isReadable'),mipmaps=int(re.search(r'enableMipMap: (\d+)',meta)[1]) if 'enableMipMap:' in meta else None,sha256=hashlib.sha256(raw).hexdigest())
    records.append(rec);pixels[digest].append(rec);files[rec['sha256']].append(rel)

en_names={r['name'] for r in records if '/UI/Sprites/en/' in r['path']}
def group(r):
    p,n=r['path'],r['name'];small=max(r['width'],r['height'])<=512
    if '/CommerceArt/' in p:
        return 'commerce_words' if n.startswith(('word-','title-','label-')) else 'commerce_buttons' if n.startswith('shop-') else 'commerce_illustrations'
    if '/UI/Sprites/en/' in p:return 'english_ui_art'
    if '/UI/Sprites/' in p:
        if n in en_names:return 'replaced_chinese_originals'
        if n.startswith(('bullet_','zgygy_roleskill_')):return 'effect_frames'
        if n.isdigit() or n.startswith(('world_icon_','ty_icon_','zgygy_coin_','zgygy_dice_','shop_icon_')):return 'item_currency_icons'
        if not small and any(x in n for x in ('bg','BG','Bg','background','Background')):return 'large_backgrounds'
        if n.startswith(('shop_','ad_')):return 'shop_native_ui'
        if n.startswith('challenge_'):return 'activity_ui'
        if n.startswith('world_'):return 'world_ui'
        if n.startswith(('ty_','zdjm_','zm_')):return 'shared_hud_ui'
        return 'other_ui_small' if small else 'other_ui_large'
    return str(Path(p).parent.relative_to('Assets/Resources')).replace('\\','/')
groups=defaultdict(list)
for r in records:r['group']=group(r);groups[r['group']].append(r)

# Deterministic MaxRects best-short-side-fit, no rotation; four-pixel padding each edge.
def pack(items,edge):
    pages=[];oversized=[]
    for r in sorted(items,key=lambda r:max(r['width'],r['height'])*100000+min(r['width'],r['height']),reverse=True):
        w,h=r['width']+8,r['height']+8
        if max(w,h)>edge:oversized.append(r['path']);continue
        best=None
        for pi,page in enumerate(pages):
            for x,y,fw,fh in page['free']:
                if w<=fw and h<=fh:
                    score=(min(fw-w,fh-h),max(fw-w,fh-h),pi,y,x)
                    if best is None or score<best[0]:best=(score,pi,x,y)
        if best is None:
            pages.append(dict(free=[(0,0,edge,edge)],sprites=[]));pi=len(pages)-1;x=y=0
        else:_,pi,x,y=best
        page=pages[pi];free=[]
        for fx,fy,fw,fh in page['free']:
            if x>=fx+fw or x+w<=fx or y>=fy+fh or y+h<=fy:free.append((fx,fy,fw,fh));continue
            if x>fx:free.append((fx,fy,x-fx,fh))
            if x+w<fx+fw:free.append((x+w,fy,fx+fw-x-w,fh))
            if y>fy:free.append((fx,fy,fw,y-fy))
            if y+h<fy+fh:free.append((fx,y+h,fw,fy+fh-y-h))
        page['free']=[a for i,a in enumerate(free) if not any(j!=i and b[0]<=a[0] and b[1]<=a[1] and b[0]+b[2]>=a[0]+a[2] and b[1]+b[3]>=a[1]+a[3] and (a!=b or j<i) for j,b in enumerate(free))]
        page['sprites'].append(dict(path=r['path'],x=x+4,y=y+4,w=r['width'],h=r['height']))
    result=[]
    for page in pages:
        sw=max(s['x']+s['w']+4 for s in page['sprites']);sh=max(s['y']+s['h']+4 for s in page['sprites'])
        pw=2**math.ceil(math.log2(sw));ph=2**math.ceil(math.log2(sh))
        result.append(dict(width=pw,height=ph,used_pixels=sum(s['w']*s['h'] for s in page['sprites']),sprites=page['sprites']))
    return dict(pages=result,oversized=oversized,rgba_bytes=sum(p['width']*p['height']*4 for p in result),bc7_bytes=sum(p['width']*p['height'] for p in result))

summary=[];plans={}
for name,items in sorted(groups.items()):
    summary.append(dict(group=name,count=len(items),png_mib=round(sum(r['png_bytes'] for r in items)/2**20,3),raw_rgba_mib=round(sum(r['rgba_bytes'] for r in items)/2**20,3),build_mib=round(sum(r['build_bytes'] for r in items)/2**20,3),size_max=max(max(r['width'],r['height']) for r in items),alpha_border_pixels_saved=sum((r['width']*r['height']-r['trim_width']*r['trim_height']) for r in items)))
    if name in ['commerce_words','commerce_buttons','commerce_illustrations','english_ui_art','item_currency_icons','shop_native_ui','shared_hud_ui','activity_ui','world_ui','effect_frames']:
        # Reports feasibility at original resolution, not a promised Unity build result.
        plans[name]=pack(items,2048)

duplicates=[dict(paths=[r['path'] for r in rs],width=rs[0]['width'],height=rs[0]['height'],possible_build_bytes_saved=sum(r['build_bytes'] for r in rs)-max(r['build_bytes'] for r in rs)) for rs in pixels.values() if len(rs)>1]
experiments={}
for name,items,scale in [('commerce_words_half',groups['commerce_words'],.5),('commerce_buttons_half',groups['commerce_buttons'],.5),('commerce_words_buttons_half',groups['commerce_words']+groups['commerce_buttons'],.5),('commerce_illustrations_half',groups['commerce_illustrations'],.5)]:
    scaled=[dict(r,width=math.ceil(r['width']*scale),height=math.ceil(r['height']*scale)) for r in items]
    experiments[name]=dict(scale=scale,input_count=len(items),original_build_mib=sum(r['build_bytes'] for r in items)/2**20,**pack(scaled,2048))
sprite_metas={s['key']:s for s in json.loads((root/'Assets/Resources/UI/sprites.json').read_text(encoding='utf-8'))['sprites']}
padding_cases=[dict(path=r['path'],source=(r['width'],r['height']),restored=(sprite_metas[r['name']]['w'],sprite_metas[r['name']]['h'])) for r in records if '/UI/Sprites/' in r['path'] and r['name'] in sprite_metas and (r['width'],r['height'])!=(sprite_metas[r['name']]['w'],sprite_metas[r['name']]['h'])]
report=dict(total_png=len(records),source_png_mib=sum(r['png_bytes'] for r in records)/2**20,build_texture_mib_from_log=sum(r['build_bytes'] for r in records)/2**20,groups=summary,duplicate_pixels=duplicates,atlas_plans=plans,experiments=experiments,runtime_padding_cases=padding_cases,notes=['Build sizes are rounded uncompressed per-asset figures from the latest Unity build log; they are not PNG file sizes or downloaded APK sizes.','Packing is a no-resize CPU feasibility simulation at 2048 with 4px padding per side, no rotation, power-of-two trimmed pages. Not a Unity build measurement.','Half-resolution experiments are numeric packing simulations only; no resampling or asset change was performed. Visual approval at target scale is still required.','BC7 numbers assume 8bpp and supported standalone target; no compression quality acceptance is implied.','No source assets, importer settings, loaders or delivered builds were modified.'],records=records)
(out/'inventory.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
with (out/'images.csv').open('w',encoding='utf-8-sig',newline='') as f:
    writer=csv.DictWriter(f,fieldnames=[k for k in records[0] if k!='alpha_bounds']);writer.writeheader();writer.writerows({k:v for k,v in r.items() if k!='alpha_bounds'} for r in records)
print(json.dumps(dict(total_png=len(records),groups=summary,duplicates=len(duplicates),top_duplicates=sorted(duplicates,key=lambda x:x['possible_build_bytes_saved'],reverse=True)[:8],plans={name:dict(pages=[(p['width'],p['height'],len(p['sprites'])) for p in data['pages']],oversized=len(data['oversized']),rgba_mib=round(data['rgba_bytes']/2**20,2),bc7_mib=round(data['bc7_bytes']/2**20,2)) for name,data in plans.items()}),ensure_ascii=False,indent=2))

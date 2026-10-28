"""Tie the purchase lettering/padding evidence to the tested Windows release."""
from pathlib import Path
from PIL import Image
import hashlib,json
root=Path(__file__).resolve().parents[2]
docs=root/'Documentation/PurchaseButtons-2026-10-21';layout=root/'Documentation/LayoutFix-2026-10-20'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
identity=json.loads((layout/'commercial-identity.json').read_text(encoding='utf-8'))
player=root/'Build/LayoutFixCommercialPlayer';assembly=player/'BufuRebuild_Data/Managed/Assembly-CSharp.dll'
assert sha(assembly)==identity['assembly_sha256']
for name,digest in identity['sources'].items():assert sha(root/name)==digest,name
cases=[]
for name in ['PurchaseWordsStore390','PurchaseWordsStore540','PurchaseWordsStoreLandscape','PurchaseWordsAlignment540']:
    path=layout/name/'runtime-check.json';result=json.loads(path.read_text(encoding='utf-8-sig'))
    assert result['passed'],(name,result['error']);assert path.stat().st_mtime>assembly.stat().st_mtime
    cases.append(dict(name=name,checks=result['checks'],width=result['width'],height=result['height']))
build=json.loads((layout/'unity-build-result.json').read_text(encoding='utf-8-sig'))
assert build['result']=='Succeeded' and build['errors']==0
runtime=json.loads((layout/'PurchaseWordsAlignment540/sprite-sheet-runtime.json').read_text(encoding='utf-8-sig'))
assert runtime['sprites']==1258 and runtime['pages']==68 and runtime['looseTextures']==0
previous=json.loads((root/'Documentation/StorePacks-2026-10-21/validation.json').read_text(encoding='utf-8'))
assert sha(root/'Assets/Resources/Commerce/products.json')==previous['purchase_catalog_sha256']
art=json.loads((docs/'word-art-manifest.json').read_text(encoding='utf-8'))
sheet=Image.open(root/art['sheet']).convert('RGBA')
for item in art['assets']:
    assert sha(root/item['asset'])==item['sha256']
    x,y,w,h=item['packed_rect'];top=sheet.height-y-h
    assert sheet.crop((x,top,x+w,top+h)).tobytes()==Image.open(root/item['asset']).convert('RGBA').tobytes()
assert sha(root/art['sheet'])==art['sheet_sha256']
release=root/'Build/Windows';delivery=json.loads((layout/'delivery-identity.json').read_text(encoding='utf-8'))
files={str(p.relative_to(release)):sha(p) for p in release.rglob('*') if p.is_file()}
assert files==delivery['files']=={str(p.relative_to(player)):sha(p) for p in player.rglob('*') if p.is_file()}
before=json.loads((docs/'before-delivery.json').read_text(encoding='utf-8'))['bytes']
after=sum(p.stat().st_size for p in release.rglob('*') if p.is_file())
number_sources=[]
for glyph in list(range(40,50))+[39,74]:
    path=root/'Assets/SourceArt/SpriteSheetSources/UI/Sprites'/f'ty_num_both_{glyph}.png'
    number_sources.append(dict(asset=str(path.relative_to(root)),sha256=sha(path),resource_key=f'UI/Sprites/ty_num_both_{glyph}'))
(docs/'native-number-assets.json').write_text(json.dumps(dict(source='Existing project ty_num_both white number PNG sprites. No generated numeric bitmap assets.',assets=number_sources),indent=2),encoding='utf-8')
report=dict(passed=True,checks=sum(c['checks'] for c in cases),cases=cases,build=build,
            assembly_sha256=identity['assembly_sha256'],runtime=runtime,new_word_sprites=4,new_atlas_pages=1,
            delivery_files=len(files),before_mib=before/2**20,after_mib=after/2**20,delta_mib=(after-before)/2**20,
            native_number_glyphs=12,new_number_bitmaps=0,
            scope='Native ty_num_both PNG quantities with dark brown outlines and proportional spacing. Wider small purchase controls, scaled frame borders and padded PNG status labels; actual localized prices remain live text. Catalog rewards and checkout behavior retained.')
(docs/'validation.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
shots=layout/'PurchaseWordsStore390';images=[Image.open(shots/p).convert('RGB') for p in ['01-store-diamonds.png','03a-store-fragments-top.png']]
preview=Image.new('RGB',(sum(i.width for i in images)+12,max(i.height for i in images)),(42,25,17));preview.paste(images[0],(0,0));preview.paste(images[1],(images[0].width+12,0));preview.save(docs/'purchase-pages-preview.png')
(docs/'实施说明.txt').write_text(f'''礼包数字、购买按钮内边距与字图优化（2026-10-21）
数量数字直接复用项目原生 ty_num_both 的白色数字、加号和 X 图片：ty_num_both_40 至 49、39、74；白字加深棕描边，按真实字宽调整间距，小 X 与主数字区分大小。
钻石 +1200、+100、+550 和角色碎片 x20 均由原生 PNG 字形动态组合，保留底层真实数量文本，未把数量写死成整张图片，也没有新增生成数字素材。
碎片数量徽标同步缩放边框并留出上下内边距。原生素材路径、资源键与哈希见 native-number-assets.json。
小按钮最大宽度从 360 增加到 440 UI 单位，同时不超过商品卡宽度的 69%；继续水平居中。
修正九宫格边框的缩放：pixelsPerUnitMultiplier=2.1，使 112 高按钮不再被上下边框挤压，中央文字面板留出足够高度。
钻石与碎片购买底栏高度 160、按钮高度 112，上下各留 24 UI 单位。免广告原有权益布局保持，购买状态文字统一使用字图。
新增 Unavailable、Owned、Pending、Please wait 四张透明字图，与现有 Back 一致的白字深棕描边、圆润卡通字体。
按钮字图高度 60，左右各 64 UI 单位留白；字图与九宫格中央区域保留至少 10 UI 单位的合计竖向空隙。
价格由支付服务动态返回并显示，字体加粗、描边和留白；状态与价格切换会明确隐藏或恢复字图，未把真实价格烘焙为图片。
字图使用内置 image_gen.imagegen 生成。素材、原始提示词、生成源、归档路径、归一化与图集信息见 word-art-manifest.json。
新增素材保存在 Assets/SourceArt/CommerceButtonWords；运行时合为 commerce-purchase-words-v1 一张图集，共 1,258 个子图与 68 张图集。
碎片卡增加少量高度并移动数量、类型、库存，让扩大的购买底栏与内容保持分离；按屏幕高度显示完整行并支持滚动。
三种商店尺寸与完整界面/图集检查共 {report['checks']} 项通过；包含价格/繁忙/待支付/已拥有状态切换。Unity 构建 {build['errors']} 错误、{build['warnings']} 警告。
Windows 交付 {len(files)} 个文件与实测版本一致，目录 {after/2**20:.2f} MiB，较前版增加 {(after-before)/2**20:.2f} MiB。没有生成 Android APK。
''',encoding='utf-8')
print('PURCHASE_BUTTONS_VERIFIED',report['checks'],'checks;',f'{after/2**20:.2f} MiB')

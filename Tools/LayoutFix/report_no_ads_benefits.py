from pathlib import Path
import json,hashlib,subprocess,sys
from PIL import Image
import numpy as np
root=Path(__file__).resolve().parents[2]
docs=root/'Documentation/NoAdsBenefits-2026-10-21';layout=root/'Documentation/LayoutFix-2026-10-20'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
if '--before' in sys.argv:
    delivery=json.loads((layout/'delivery-identity.json').read_text(encoding='utf-8'))
    files={str(p.relative_to(root/'Build/Windows')):p.stat().st_size for p in (root/'Build/Windows').rglob('*') if p.is_file()}
    (docs/'before-delivery.json').write_text(json.dumps(dict(bytes=sum(files.values()),files=files,identity=delivery),indent=2),encoding='utf-8')
    print('NO_ADS_BASELINE_RECORDED');sys.exit(0)
names=['NoAdsBenefitsStore390','NoAdsBenefitsStore540','NoAdsBenefitsStoreLandscape','NoAdsBenefitsAlignment540']
cases=[]
for name in names:
    result=json.loads((layout/name/'runtime-check.json').read_text(encoding='utf-8-sig'))
    assert result['passed'],(name,result['error']);cases.append(dict(name=name,checks=result['checks'],width=result['width'],height=result['height']))
identity=json.loads((layout/'commercial-identity.json').read_text(encoding='utf-8'))
for name,digest in identity['sources'].items():assert sha(root/name)==digest,name
index=json.loads((root/'Assets/Resources/PackedUI/index.json').read_text(encoding='utf-8'))
before_index=json.loads(subprocess.check_output(['git','show','bb0a835:Assets/Resources/PackedUI/index.json'],cwd=root))
current={e['key']:e for e in index['entries']}
assert all(current[e['key']]==e for e in before_index['entries'])
runtime=json.loads((layout/'NoAdsBenefitsAlignment540/sprite-sheet-runtime.json').read_text(encoding='utf-8-sig'))
assert runtime['sprites']==1254 and runtime['pages']==67 and runtime['looseTextures']==0
comparisons=[]
for scene in ['01-store-diamonds.png','03a-store-fragments-top.png']:
    a=np.array(Image.open(layout/'SpriteSheetsStore390'/scene).convert('RGB')).astype(float)
    b=np.array(Image.open(layout/'NoAdsBenefitsStore390'/scene).convert('RGB')).astype(float)
    # Compare product areas; animated bottom unlock hints and clocks are excluded.
    delta=float(np.abs(a[150:625]-b[150:625]).mean());assert delta<1,(scene,delta)
    comparisons.append(dict(scene=scene,mean_rgb_delta_255=delta))
delivery=json.loads((layout/'delivery-identity.json').read_text(encoding='utf-8'))
release=root/'Build/Windows';player=root/'Build/LayoutFixCommercialPlayer'
files={str(p.relative_to(release)):sha(p) for p in release.rglob('*') if p.is_file()}
assert files==delivery['files']=={str(p.relative_to(player)):sha(p) for p in player.rglob('*') if p.is_file()}
before=json.loads((docs/'before-delivery.json').read_text(encoding='utf-8'))['bytes'];after=sum(p.stat().st_size for p in release.rglob('*') if p.is_file())
report=dict(passed=True,cases=cases,checks=sum(r['checks'] for r in cases),source_assembly_sha256=identity['assembly_sha256'],runtime=runtime,existing_sprite_entries_unchanged=len(before_index['entries']),other_offer_comparison=comparisons,before_mib=before/2**20,after_mib=after/2**20,delta_mib=(after-before)/2**20,delivery_files=len(files),scope='Current source identity: three product viewport runs and full alignment/sprite catalog. Benefit art and No Ads composition changed; billing, other pack layouts and gameplay retained.')
(docs/'validation.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
(docs/'实施说明.txt').write_text(f'''免广告权益展示优化（2026-10-21）
原来的三个重复勾勾改为独立的免视频电影券、点击宝箱和永久盾牌图标。
新增三张同画风图标：内置 image_gen 生成，以现有紫金免广告券和原生钻石袋为参考。
保留原游戏金色卡通画风，借鉴用户商业项目图标与权益卡片的层级，没有照搬配色、价格或赠送承诺。
权益图标为 176 UI 单位，放入三块浅色权益小卡；保留购买按钮上下 16 UI 单位内边距。
主插画与文案上移，权益区、限制说明、标签、购买底栏分别留出间距。
三张图标合在一张 792x264 图集中，归档原图在 Assets/SourceArt/CommerceBenefits，未新增零散运行时图。
提示词、源输出、尺寸与哈希见 benefit-art-manifest.json。主图集重建后会自动追加这组图标。
四个当前版本实机用例通过，共 {report['checks']} 项检查；包含三种屏幕尺寸、购买与恢复流程、全部 1,254 子图与 67 图集加载。
原 1,251 个子图索引未改变，钻石与碎片商品区域截图与前版平均差异小于 1/255。
交付 Windows 目录从 {report['before_mib']:.2f} MiB 变为 {report['after_mib']:.2f} MiB，增加 {report['delta_mib']:.2f} MiB。
213 个交付文件已核对 SHA-256。没有生成 Android APK；不把 Windows 目录体积当作 APK 体积。
截图位于 Documentation/LayoutFix-2026-10-20/NoAdsBenefitsStore*，最新免广告页为 02-store-no-ads.png。
''',encoding='utf-8')
print('NO_ADS_BENEFITS_VERIFIED',report['checks'],'checks;',f"{report['after_mib']:.2f} MiB")

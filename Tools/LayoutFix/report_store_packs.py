"""Verify the current diamond/fragment composition against the tested delivery."""
from pathlib import Path
import hashlib
import json
from PIL import Image, ImageChops, ImageStat

root = Path(__file__).resolve().parents[2]
layout = root / 'Documentation/LayoutFix-2026-10-20'
docs = root / 'Documentation/StorePacks-2026-10-21'
docs.mkdir(parents=True, exist_ok=True)
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
identity = json.loads((layout / 'commercial-identity.json').read_text(encoding='utf-8'))
player = root / 'Build/LayoutFixCommercialPlayer'
assembly = player / 'BufuRebuild_Data/Managed/Assembly-CSharp.dll'
assert sha(assembly) == identity['assembly_sha256']
for name, digest in identity['sources'].items():
    assert sha(root / name) == digest, name
cases = []
for name in ['PackPolishStore390', 'PackPolishStore540', 'PackPolishStoreLandscape', 'PackPolishAlignment540']:
    path = layout / name / 'runtime-check.json'
    result = json.loads(path.read_text(encoding='utf-8-sig'))
    assert result['passed'], (name, result['error'])
    assert path.stat().st_mtime > assembly.stat().st_mtime, name
    cases.append(dict(name=name, checks=result['checks'], width=result['width'], height=result['height']))
runtime = json.loads((layout / 'PackPolishAlignment540/sprite-sheet-runtime.json').read_text(encoding='utf-8-sig'))
assert runtime['sprites'] == 1254 and runtime['pages'] == 67 and runtime['looseTextures'] == 0
before = Image.open(layout / 'NoAdsBenefitsStore390/02-store-no-ads.png').convert('RGB').crop((20,180,370,530))
after = Image.open(layout / 'PackPolishStore390/02-store-no-ads.png').convert('RGB').crop((20,180,370,530))
no_ads_delta = sum(ImageStat.Stat(ImageChops.difference(before, after)).mean) / 3
assert no_ads_delta < 1, no_ads_delta
delivery = json.loads((layout / 'delivery-identity.json').read_text(encoding='utf-8'))
release = root / 'Build/Windows'
files = {str(p.relative_to(release)): sha(p) for p in release.rglob('*') if p.is_file()}
assert files == delivery['files'] == {str(p.relative_to(player)): sha(p) for p in player.rglob('*') if p.is_file()}
report = dict(passed=True, cases=cases, checks=sum(c['checks'] for c in cases),
              source_assembly_sha256=identity['assembly_sha256'],
              purchase_catalog_sha256=sha(root / 'Assets/Resources/Commerce/products.json'),
              no_ads_product_region_mean_rgb_delta_255=no_ads_delta,
              runtime=runtime, new_bitmap_assets=0, delivery_files=len(files),
              delivery_mib=sum(p.stat().st_size for p in release.rglob('*') if p.is_file())/2**20,
              scope='Diamond quantity/art hierarchy, native product name bands, fragment portrait/count layout, complete visible rows and scroll indicator. Existing rewards, quantities, payment states and No Ads benefits retained.')
(docs / 'validation.json').write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
(docs / '实施说明.txt').write_text(f'''钻石与角色碎片购买页优化（2026-10-21）
钻石页：保留一大两小商品结构。统一棕金标题栏，扩大钻石图标，突出 +100、+550、+1,200 的奖励数量。
碎片页：保留十个实际角色商品；角色名置于标题栏，扩大原生角色碎片插画，右侧显示数量徽标、Fragments 和已拥有数量。
产品含义：现有商品发放角色升级碎片，未改成整件皮肤，也没有添加不存在的折扣、首充赠送或价格。
布局：保持整组垂直居中、购买按钮窄幅居中和上下各 16 UI 单位留白。碎片页按可用高度显示两排或三排完整商品，并显示滚动位置。
美术：复用现有图集中的卡框、钻石、角色碎片、金色数量底、柔光和文字图；本轮未新增位图素材。
验证：三种商店尺寸与完整界面/图集检查通过，共 {report['checks']} 项。验证图集仍为 1,254 个子图、67 张图集，当前免广告页商品区域截图与此前版本平均 RGB 差异 {no_ads_delta:.4f}/255。
本地 Windows 包已更新，{len(files)} 个交付文件与实测版本 SHA-256 一致，目录体积 {report['delivery_mib']:.2f} MiB。未生成 Android APK。
截图：Documentation/LayoutFix-2026-10-20/PackPolishStore390、PackPolishStore540、PackPolishStoreLandscape。
''', encoding='utf-8')
print('STORE_PACKS_VERIFIED', report['checks'], 'checks;', f"{report['delivery_mib']:.2f} MiB")

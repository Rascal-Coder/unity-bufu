from pathlib import Path
import json,hashlib,re

root=Path(__file__).resolve().parents[2]
out=root/'Documentation/SpriteSheetAudit-2026-10-20'
layout=root/'Documentation/LayoutFix-2026-10-20'
before=json.loads((out/'before-delivery.json').read_text(encoding='utf-8'))
migration=json.loads((out/'migration-manifest.json').read_text(encoding='utf-8'))
validation=json.loads((layout/'final-validation.json').read_text(encoding='utf-8'))
wake=json.loads((layout/'SpriteSheetsWake390/runtime-check.json').read_text(encoding='utf-8-sig'))
runtime=json.loads((layout/'SpriteSheetsAlignment540/sprite-sheet-runtime.json').read_text(encoding='utf-8-sig'))
visual=json.loads((out/'visual-comparison.json').read_text(encoding='utf-8'))
delivery=json.loads((layout/'delivery-identity.json').read_text(encoding='utf-8'))
identity=json.loads((layout/'commercial-identity.json').read_text(encoding='utf-8'))
build=json.loads((layout/'unity-build-result.json').read_text(encoding='utf-8-sig'))
assert build['result']=='Succeeded' and build['errors']==0 and build['warnings']==0
assert validation['passed'] and wake['passed'] and runtime['looseTextures']==0 and visual['passed']
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
for rel,digest in identity['sources'].items():assert sha(root/rel)==digest,rel
release=root/'Build/Windows';player=root/'Build/LayoutFixCommercialPlayer'
actual={str(p.relative_to(release)):sha(p) for p in release.rglob('*') if p.is_file()}
assert actual==delivery['files']=={str(p.relative_to(player)):sha(p) for p in player.rglob('*') if p.is_file()}
after=sum(p.stat().st_size for p in release.rglob('*') if p.is_file());saved=before['total_bytes']-after
log=(out/'unity-sheets.log').read_text(encoding='utf-8',errors='replace')
used={m[1] for m in re.finditer(r'^\s*[\d.]+\s+(?:kb|mb|b)\s+[^\r\n]*?\s(Assets/[^\r\n]+)',log,re.M)}
assert not any(item['path'] in used for item in migration['migrated']+migration['removed_duplicates'])
assert not any(p.startswith('Assets/SourceArt/') for p in used)
for item in migration['migrated']+migration['removed_duplicates']:
    src=root/item['path'];backup=root/'Assets/SourceArt/SpriteSheetSources'/src.relative_to(root/'Assets/Resources')
    assert not src.exists() and sha(backup)==item['sha256'],src
result=dict(passed=True,before_bytes=before['total_bytes'],after_bytes=after,saved_bytes=saved,saved_percent=100*saved/before['total_bytes'],before_mib=before['total_bytes']/2**20,after_mib=after/2**20,saved_mib=saved/2**20,migrated_sprites=len(migration['migrated']),sheet_pages=len(migration['pages']),duplicate_textures_archived=len(migration['removed_duplicates']),geometry_resampled=False,texture_profile='Windows BC7; Android/iPhone ASTC 4x4 configured but not built or tested',build=build,validation_cases=len(validation['cases'])+1,assertions=validation['total_checks']+wake['checks'],runtime=runtime,visual=visual,delivery_files=len(actual),before_backup=before['backup'],source_backup='Assets/SourceArt/SpriteSheetSources',source_assembly_sha256=identity['assembly_sha256'],scope='Identical Development Windows64 build options and same editor. Full delivered folders compared, not APK download sizes. No source resampling. Block texture compression changes pixels slightly; static screenshot crop metrics retained.')
(out/'implementation-result.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
text=f'''图集合并与减包实施结果（2026-10-20）

交付
Build/Windows/BufuRebuild.exe
完整 Windows 目录：{result['before_mib']:.2f} MiB -> {result['after_mib']:.2f} MiB。
减少 {result['saved_mib']:.2f} MiB（{result['saved_percent']:.2f}%）。
采用同一 Unity 2022.3.62f3c1、同一 Development Windows64 配置及相同 Intro/Main 场景比较。
这是 Windows 完整目录体积，未生成 Android/iOS 安装包，也不是网络下载压缩包大小。

已经落地
1,251 张 UI 图片转为 66 张独立分组图集，按需加载。包括商城字图与按钮、英文 UI、公共 HUD、道具货币、升级、天赋、世界、活动，以及按动画族组织的技能和子弹特效。
212 张重复 Art/UI 图片移出 Resources。
没有缩小原图，没有裁掉有效画面；原逻辑尺寸、透明边界偏移、pivot、pixels-per-unit、Sprite 名称和九宫格 border 保留。
图集无旋转，四像素保护间距、两像素边缘扩展，尺寸为四的倍数，最大 2048。
图集及保留的大 UI 使用 Windows BC7；配置了 Android/iPhone ASTC 4x4，但本次只验证 Windows。
大背景等 24 张不适合合图的 UI 保留原加载结构，单独压缩；Spine、VAT、字体与粒子的原图集结构保留。
三张商城插画保留独立纹理并压缩，没有缩图。
原资源与 meta 完整保留在 Assets/SourceArt/SpriteSheetSources，未进入 Player。

加载与维护
Assets/Scripts/UI/PackedSpriteSheets.cs：按原资源 key 查找共享纹理上的子 Sprite，页面首次使用时才加载。
Assets/Resources/PackedUI/index.json：资源名称、图集、rect、pivot、border 与 ppu 索引。
UiAssets 和商城图片加载器统一优先使用图集；其余独立图继续走原路径。
资源在合图前离线恢复原逻辑透明边界，避免运行时 Pad 重新生成独立纹理、破坏共享效果。
Assets/Editor/PackedUiTextureImporter.cs：可复用的纹理平台导入设置。
Tools/LayoutFix/build_sprite_sheets.py --apply：按本次源图清单重建图集并归档源图。后续修改或新增源图时先更新清单；没有自动监听重新合图。
normalize_art.py / normalize_words.py 会把已迁移图片写回归档源目录，避免无意重新引入重复散图。

验证
Unity 构建：0 错误、0 警告。
12 个实机用例，{result['assertions']} 项检查通过，覆盖 390x844、540x960、960x540、商城购买模拟、英文显示、完整玩法对齐、骰子、挑战、拍卖与唤醒气泡。
逐一验证全部 1,251 个子 Sprite 的名称、区域、pivot、ppu 与 border；确认 Resources 中不存在迁移后的原散图。
66 张共享纹理在 Windows Player 中均为 BC7。全量验证强制加载所有图集约占 90.6 MiB 纹理内存，此值不代表正常游戏峰值。
6 处静态界面区域与优化前实机截图比较，RGB 平均绝对误差为 0.13–0.33 / 255，P99 误差为 2–3 / 255；全图已检查。
所有源图归档哈希、构建不含原散图/归档源图、交付目录 {len(actual)} 文件 SHA-256 均核对通过。
没有修改已确认的 UI 布局、按钮留白和交互区域，也没有调整用户排除的 Upgrade 箭头及模拟广告布局。
不声称已测得帧率或 Draw Call 改善。

备份
合图前交付：{before['backup']}
更早的布局改版前备份仍保留在 Build/Windows-before-layout-20261020。

证据
migration-manifest.json：迁移图片、页尺寸与哈希、归档重复图片。
implementation-result.json：精确体积、完整版本和交付验证。
visual-comparison.json：截图静态区域像素差异。
unity-sheets.log：本次完整 Unity 构建记录。
Documentation/LayoutFix-2026-10-20/SpriteSheets*：各分辨率截图及实机检查。
'''
(out/'实施结果.txt').write_text(text,encoding='utf-8')
print(f'SHEETS_DELIVERY_OK {result["before_mib"]:.2f} -> {result["after_mib"]:.2f} MiB; saved {result["saved_percent"]:.2f}%; {result["assertions"]} checks; {len(actual)} files verified')

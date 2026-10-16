"""List image lettering for manual remaking. Reads original PNGs; never edits artwork."""
import csv
import json
import pathlib
from collections import defaultdict
from PIL import Image

ROOT = pathlib.Path(__file__).resolve().parent.parent
SPRITES = ROOT / 'Assets/Resources/UI/Sprites'


def sprite_keys(value):
    if isinstance(value, dict):
        for key, child in value.items():
            if key == 'sprite' and isinstance(child, str):
                yield child
            else:
                yield from sprite_keys(child)
    elif isinstance(value, list):
        for child in value:
            yield from sprite_keys(child)


def build():
    source = json.loads((ROOT / 'Tools/localization-artwork.json').read_text(encoding='utf-8'))['assets']
    metadata = {x['key']: x for x in json.loads((ROOT / 'Assets/Resources/UI/sprites.json').read_text(encoding='utf-8'))['sprites']}
    paths = defaultdict(list)
    for row in json.loads((ROOT / 'Assets/Resources/UI/sprite_paths.json').read_text(encoding='utf-8'))['paths']:
        paths[row['key']].append(row['path'])
    usage = defaultdict(set)
    prefabs = ROOT / 'Assets/Resources/UI/Prefabs'
    for path in prefabs.rglob('*.json'):
        data = json.loads(path.read_text(encoding='utf-8'))
        for node in data['nodes']:
            for key in sprite_keys(node.get('c', [])):
                usage[key].add(str(path.relative_to(prefabs).with_suffix('')).replace('\\', '/') + '/' + node['n'])
    hidden = {'zjm_btn_tiaozhan01', 'ty_zi_restoreSave', 'qd_zi_dingyue', 'bs_btn_subscribe', 'zm_lab_sign3', 'zm_lab_sign7'}
    important = {'zjm_btn_wupin01', 'zjm_btn_sheshi01', 'zjm_btn_tianfu01', 'zjm_btn_shangcheng01', 'zjm_btn_shijie01', 'ty_icon_qiandao', 'qd_pic_bg_scale2', 'qd_zi_nomorl', 'qd_zi_as', 'zjm_pic_chenghao1', 'ty_pic_title_setup'}
    rows = []
    for item in source:
        key = item['sprite']
        path = SPRITES / (key + '.png')
        if not path.exists():
            raise FileNotFoundError(path)
        with Image.open(path) as image:
            width, height = image.size
        original = metadata.get(key, {})
        priority = 'P2 备用/当前隐藏' if key in hidden or key.startswith('lv8Dice_') else 'P0 主界面优先' if key in important else 'P1 其他界面/特效'
        category = '粒子文字' if key.startswith('caidai_') else '导航图标' if key.startswith('zjm_btn_') else '合图背景' if key in ['qd_pic_bg_scale2', 'ty_pic_dachengmubiao_scale2'] else '图片文字'
        rows.append(dict(priority=priority, category=category, sprite=key, path=str(path), png=f'{width} x {height}', original=f'{original.get("w", width)} x {original.get("h", height)}', chinese=item['chinese'], english=' / '.join(x.replace('\n', ' ') for x in item['english']), usage='; '.join(sorted(usage[key])), original_path='; '.join(paths[key]), note=item.get('note', '')))
    rows.sort(key=lambda row: (row['priority'], row['category'], row['sprite']))
    destination = ROOT / 'Documentation'
    columns = [('priority', '优先级'), ('category', '类型'), ('sprite', '素材键'), ('path', '原图完整路径'), ('png', 'PNG实际尺寸'), ('original', '原始Sprite尺寸'), ('chinese', '图片内中文'), ('english', '建议英文'), ('usage', '使用位置'), ('original_path', '原版资源路径'), ('note', '制作注意')]
    with (destination / 'LocalizationArtwork.csv').open('w', encoding='utf-8-sig', newline='') as handle:
        writer = csv.writer(handle)
        writer.writerow(['序号'] + [title for _, title in columns])
        for index, row in enumerate(rows, 1):
            writer.writerow([index] + [row[key] for key, _ in columns])
    lines = [f'英文素材重制清单：{len(rows)} 张候选图片', '', '当前只做真实文本组件的中英文切换；本清单中的 PNG、粒子文字和原图都没有修改。', 'P0 为主界面优先，P1 为其他界面和特效，P2 为备用或当前隐藏的素材，可最后确认是否重制。', '建议保持 PNG 实际画布尺寸、透明通道和图标位置；原始 Sprite 尺寸包含导出时裁去的透明边缘，单独列出供对齐参考。', '建议英文供制作参考，可按版面缩短。带动态数字的图请保留原来的数字占位。', '完整字段和原版资源路径见同目录 LocalizationArtwork.csv。', '']
    for index, row in enumerate(rows, 1):
        lines += [f'{index:02}. [{row["priority"]}] {row["sprite"]}.png', f'    中文：{row["chinese"]}', f'    英文：{row["english"]}', f'    PNG：{row["png"]}；Sprite：{row["original"]}', f'    文件：{row["path"]}', f'    位置：{row["usage"] or "备用素材，需确认接入位置"}']
        if row['note']:
            lines.append('    注意：' + row['note'])
        lines.append('')
    (destination / '英文素材重制清单.txt').write_text('\n'.join(lines) + '\n', encoding='utf-8-sig')
    print(f'Listed {len(rows)} images. No image files changed.')


if __name__ == '__main__':
    build()

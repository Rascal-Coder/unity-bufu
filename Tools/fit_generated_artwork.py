"""Place a newly generated sticker onto the original canvas size.

The Chinese PNG is used only for its pixel size. None of its pixels are copied.
"""
from pathlib import Path

import cv2
import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parent.parent
DEST = ROOT / "Assets/Resources/UI/Sprites/en"
GENERATED = Path(r"C:/Users/aaa/.cursor/projects/e-rea-games-unity-bufu/assets")

JOBS = {
    "en-new-helper.jpg": ("lv8Dice_pic_slogn_newHelper_x2", (266, 165)),
    "en-next-stop.jpg": ("lv8Dice_pic_slogn_nextStation_x2", (367, 91)),
    "en-new-world.jpg": ("lv8Dice_pic_title_main_x2", (582, 132)),
    "en-awakened-skill.jpg": ("bs_zi_juexingjineng", (392, 138)),
    "en-adventure.jpg": ("lv8Dice_pic_title_sub_x2", (408, 55)),
    "en-coming-soon.jpg": ("lv8Dice_zi_jingqingqidai", (294, 83)),
    "en-first-free.jpg": ("shop_zi_everydayFree", (384, 72)),
    "en-upgrade.jpg": ("ty_zi_upgrade", (134, 72)),
    "en-go.jpg": ("caidai_jiayou", (144, 106)),
    "en-earn.jpg": ("caidai_zhuanqian", (144, 106)),
    "en-summon.jpg": ("ad_btn_summon", (360, 142)),
    "en-early-unlock.jpg": ("bs_btn_unlock", (413, 98)),
    "en-confirm.jpg": ("lxsy_btn_queding", (360, 142)),
    "en-double-reward.jpg": ("lxsy_btn_shuangbeihuoqu", (436, 142)),
    "en-leave.jpg": ("ty_btn_exitChallenge", (275, 159)),
    "en-settings.jpg": ("ty_pic_title_setup", (463, 167)),
    "en-tycoon-title.jpg": ("ad_pic_title_dafuweng", (543, 167)),
    "en-cheer-title.jpg": ("ad_pic_title_guwudashi", (543, 167)),
    "en-earn-title.jpg": ("ad_pic_title_zhuanqiandashi", (543, 167)),
    "en-offline-title.jpg": ("lxsy_pic_title_lixianshouyi", (543, 167)),
    "en-cancel.jpg": ("talent_btn_cancle", (288, 114)),
    "en-rebirth-btn.jpg": ("talent_btn_yellow", (432, 170)),
    "en-reset.jpg": ("talent_btn_reset", (432, 170)),
    "en-rebirth-small.jpg": ("talent_btn_reborn", (288, 114)),
    "en-quxiao.jpg": ("ty_btn_quxiao", (360, 142)),
    "en-use.jpg": ("zm_btn_use", (360, 142)),
    "en-helper-upgrade.jpg": ("zm_btn_bs_upgrade", (353, 98)),
    "en-subscribe-btn.jpg": ("bs_btn_subscribe", (373, 98)),
    "en-clear-save.jpg": ("ty_zi_cleanSave", (259, 72)),
    "en-more.jpg": ("ty_zi_gengduo", (140, 50)),
    "en-go-shop.jpg": ("ty_zi_qushangcheng", (197, 73)),
    "en-restore-save.jpg": ("ty_zi_restoreSave", (260, 72)),
    "en-rewards.jpg": ("ty_title_gongxihuode 1", (580, 248)),
    "en-missing.jpg": ("zm_zi_cailiaobuzu", (260, 71)),
    "en-reset-small.jpg": ("talent_btn_reset02", (288, 114)),
    "en-less.jpg": ("ty_zi_shouqi", (141, 50)),
    "en-get.jpg": ("qd_lab_get", (78, 80)),
    "en-subscribe.jpg": ("qd_zi_dingyue", (141, 77)),
    "en-in-use.jpg": ("zm_lab_inUse", (173, 80)),
    "en-rebirth-title.jpg": ("talent_pic_title_reborn", (463, 167)),
    "en-reset-title.jpg": ("talent_pic_title_reset", (463, 167)),
    "en-shards-title.jpg": ("ty_pic_title_suipianbuzu", (543, 167)),
    "en-go-go.jpg": ("sw_bac_jiayou_", (163, 129)),
    "en-nav-shop.jpg": ("zjm_btn_shangcheng01", (152, 180)),
    "en-checkin-icon.jpg": ("ty_icon_qiandao", (158, 172)),
    "en-growth-badge.jpg": ("zjm_pic_chenghao1", (225, 233)),
    "en-icon-tycoon.jpg": ("ad_icon_dafuweng", (169, 189)),
    "en-nav-helpers.jpg": ("zjm_btn_sheshi01", (181, 181)),
    "en-nav-worlds.jpg": ("zjm_btn_shijie01", (154, 189)),
    "en-nav-talents.jpg": ("zjm_btn_tianfu01", (193, 185)),
    "en-nav-items.jpg": ("zjm_btn_wupin01", (179, 184)),
    "en-nav-challenge.jpg": ("zjm_btn_tiaozhan01", (173, 200)),
    "en-icon-cheer.jpg": ("ad_icon_guwudashi", (176, 185)),
    "en-icon-earn.jpg": ("ad_icon_zhuanqiandashi", (192, 196)),
    "en-free-rewards.jpg": ("qd_zi_nomorl", (389, 60)),
    "en-time-left.jpg": ("ty_pic_countdown", (457, 71)),
    "en-now-ribbon.jpg": ("world_zi_dangqian", (289, 173)),
    "en-unlocked.jpg": ("xsyd_yijies", (230, 189)),
    "en-earn-up.jpg": ("xsyd_zhuanqianshengji", (191, 230)),
    "en-in-use-plate.jpg": ("zm_pic_using", (360, 142)),
    "en-tab-bar.jpg": ("talent_pic_tabBG", (1057, 127)),
    "en-tab-coins.jpg": ("talent_pic_tabBG_coin", (232, 78)),
    "en-tab-helpers.jpg": ("talent_pic_tabBG_helper", (254, 78)),
    "en-tab-special.jpg": ("talent_pic_tabBG_special", (254, 78)),
    "en-tab-coins-on.jpg": ("talent_pic_tabJinqian", (279, 131)),
    "en-tab-helpers-on.jpg": ("talent_pic_tabBangshou", (279, 131)),
    "en-tab-special-on.jpg": ("talent_pic_tabTeshu", (279, 131)),
    "en-tag-helpers.jpg": ("zm_tags_bs", (402, 141)),
    "en-tag-tables.jpg": ("zm_tags_zm", (402, 141)),
    "en-sign3.jpg": ("zm_lab_sign3", (259, 159)),
    "en-sign7.jpg": ("zm_lab_sign7", (259, 159)),
    "en-talent-points.jpg": ("talent_pic_talentPoint", (299, 339)),
    "en-talent-points-dice.jpg": ("talent_pic_talentPoint_dice", (299, 339)),
    "en-challenge-btn.jpg": ("ty_btn_challenge", (599, 225)),
    "en-helpers-count.jpg": ("zm_pic_bs_count", (631, 297)),
    "en-sr-shards.jpg": ("shop_zi_srDes", (499, 67)),
    "en-challenge-fail.jpg": ("ty_pic_challenge_fail_scale2", (512, 414)),
    "en-challenge-win.jpg": ("ty_pic_challenge_successl_scale2", (720, 730)),
    "en-newspaper.jpg": ("ty_pic_dachengmubiao_scale2", (512, 484)),
    "en-checkin-board.jpg": ("qd_pic_bg_scale2", (540, 1023)),
    "en-ad-draw.jpg": ("shop_zi_ad", (299, 68)),
    "en-ad-rewards.jpg": ("qd_zi_as", (389, 60)),
    "en-island-soon.jpg": ("world_pic_island_comingSoon_x2", (427, 309)),
    "en-tab-bg.jpg": ("zm_tags_BG01", (726, 131)),
}


def cutout(path):
    rgb = np.array(Image.open(path).convert("RGB"))
    height, width = rgb.shape[:2]
    edges = np.concatenate([
        rgb[:8].reshape(-1, 3),
        rgb[-8:].reshape(-1, 3),
        rgb[:, :8].reshape(-1, 3),
        rgb[:, -8:].reshape(-1, 3),
    ])
    key = np.median(edges, axis=0).astype(np.int16)
    distance = np.abs(rgb.astype(np.int16) - key).sum(axis=2)
    background = (distance < 100).astype(np.uint8)
    count, labels = cv2.connectedComponents(background, connectivity=4)
    cut = distance < 100
    alpha = np.where(cut, 0, 255).astype(np.uint8)
    ys, xs = np.where(alpha > 20)
    if len(xs) == 0:
        raise SystemExit(f"no foreground in {path.name}")
    crop = np.dstack([rgb, alpha])[ys.min():ys.max() + 1, xs.min():xs.max() + 1]
    print(path.name, "key", key.tolist(), "crop", crop.shape[1], crop.shape[0], "labels", count)
    return crop


def attach_left_piece(crop, overlap=8):
    """Slide a detached left pointer until it touches the main shape."""
    fg = (crop[:, :, 3] > 20).astype(np.uint8)
    count, labels = cv2.connectedComponents(fg, connectivity=8)
    if count < 3:
        return crop
    parts = []
    for i in range(1, count):
        ys, xs = np.where(labels == i)
        parts.append((len(xs), i, int(xs.min()), int(xs.max()), int(ys.min()), int(ys.max())))
    big = max(parts)
    left = [p for p in parts if p[0] < big[0] * 0.3 and p[3] < big[2]]
    if not left:
        return crop
    _, _, x0, x1, y0, y1 = max(left, key=lambda p: p[0])
    shift = big[2] + overlap - x1
    if shift <= 2:
        return crop
    piece = crop[y0:y1 + 1, x0:x1 + 1].copy()
    crop = crop.copy()
    crop[y0:y1 + 1, x0:x1 + 1, 3] = 0
    nx = min(crop.shape[1] - piece.shape[1], x0 + shift)
    dest = crop[y0:y1 + 1, nx:nx + piece.shape[1]]
    mask = piece[:, :, 3] > dest[:, :, 3]
    dest[mask] = piece[mask]
    crop[y0:y1 + 1, nx:nx + piece.shape[1]] = dest
    alpha = crop[:, :, 3]
    ys, xs = np.where(alpha > 20)
    return crop[ys.min():ys.max() + 1, xs.min():xs.max() + 1]


def place(crop, size):
    target_w, target_h = size
    max_w = max(1, int(target_w * 0.96))
    max_h = max(1, int(target_h * 0.92))
    scale = min(max_w / crop.shape[1], max_h / crop.shape[0])
    resized = Image.fromarray(crop, "RGBA").resize(
        (max(1, int(crop.shape[1] * scale)), max(1, int(crop.shape[0] * scale))),
        Image.Resampling.LANCZOS,
    )
    canvas = Image.new("RGBA", size, (0, 0, 0, 0))
    canvas.paste(resized, ((target_w - resized.width) // 2, (target_h - resized.height) // 2), resized)
    return canvas


def place_split(crop, size, left_end=0.13, right_start=0.28):
    """Keep a transparent count gap between two text clusters."""
    alpha = crop[:, :, 3]
    cols = np.where(alpha.max(axis=0) > 20)[0]
    gaps = np.where(np.diff(cols) > 8)[0]
    if len(gaps) == 0:
        return place(crop, size)
    split = cols[gaps[np.argmax(np.diff(cols)[gaps])]]
    left = crop[:, :split + 1]
    right = crop[:, split + 1:]
    target_w, target_h = size
    canvas = Image.new("RGBA", size, (0, 0, 0, 0))

    def fit_side(part, x0, x1):
        box = part[:, :, 3] > 20
        ys, xs = np.where(box)
        part = part[ys.min():ys.max() + 1, xs.min():xs.max() + 1]
        max_w = max(1, x1 - x0)
        max_h = max(1, int(target_h * 0.88))
        scale = min(max_w / part.shape[1], max_h / part.shape[0])
        resized = Image.fromarray(part, "RGBA").resize(
            (max(1, int(part.shape[1] * scale)), max(1, int(part.shape[0] * scale))),
            Image.Resampling.LANCZOS,
        )
        y = (target_h - resized.height) // 2
        canvas.paste(resized, (x0, y), resized)

    fit_side(left, 4, int(target_w * left_end))
    fit_side(right, int(target_w * right_start), target_w - 4)
    return canvas


def main():
    DEST.mkdir(parents=True, exist_ok=True)
    chosen = __import__("sys").argv[1:]
    jobs = JOBS if not chosen else {name: JOBS[name] for name in chosen}
    for filename, (key, size) in jobs.items():
        crop = cutout(GENERATED / filename)
        if filename == "en-sr-shards.jpg":
            canvas = place_split(crop, (499, 67))
        elif filename == "en-tab-bg.jpg":
            canvas = Image.fromarray(crop, "RGBA").resize(size, Image.Resampling.LANCZOS)
        else:
            if filename == "en-ad-rewards.jpg":
                crop = attach_left_piece(crop)
            canvas = place(crop, size)
        canvas.save(DEST / f"{key}.png")
        print("wrote", key, canvas.size)


if __name__ == "__main__":
    main()

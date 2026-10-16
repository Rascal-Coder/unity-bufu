"""Build English UI sprites from the original PNGs.

Original files are never modified. English files keep the same pixel size and alpha
so the existing sprite padding still lines up.
"""
import sys
from pathlib import Path

import cv2
import numpy as np
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / "Assets/Resources/UI/Sprites"
DEST = ROOT / "Assets/Resources/UI/Sprites/en"
FONT = Path(r"C:/Windows/Fonts/ariblk.ttf")
FONT_BODY = Path(r"C:/Windows/Fonts/arialbd.ttf")

WHITE = ((255, 255, 255, 255), (18, 12, 10, 255), 0.13, FONT)
CREAM = ((255, 248, 214, 255), (32, 20, 12, 255), 0.14, FONT)
BROWN = ((96, 68, 46, 255), (255, 255, 255, 0), 0.0, FONT)
INK = ((42, 40, 38, 255), (255, 255, 255, 0), 0.0, FONT)
PAPER = ((48, 46, 44, 255), (255, 255, 255, 0), 0.0, FONT_BODY)
GOLD = ((255, 228, 90, 255), (150, 72, 16, 255), 0.08, FONT)
DARK = ((55, 58, 62, 255), (255, 255, 255, 0), 0.0, FONT)


def zone_box(shape, zone):
    height, width = shape[:2]
    x, y, w, h = zone
    left, top = int(round(x * width)), int(round(y * height))
    right, bottom = int(round((x + w) * width)), int(round((y + h) * height))
    return max(0, left), max(0, top), min(width, max(left + 2, right)), min(height, max(top + 2, bottom))


def erase_plate(image, box, textured=False):
    left, top, right, bottom = box
    roi = image[top:bottom, left:right]
    rgb = roi[:, :, :3]
    alpha = roi[:, :, 3]
    ph, pw = max(3, roi.shape[0] // 8), max(3, roi.shape[1] // 14)
    samples = []
    for y0, x0 in (
        (0, (roi.shape[1] - pw) // 2),
        (roi.shape[0] - ph, (roi.shape[1] - pw) // 2),
        ((roi.shape[0] - ph) // 2, 0),
        ((roi.shape[0] - ph) // 2, roi.shape[1] - pw),
    ):
        y0 = min(max(0, y0), roi.shape[0] - ph)
        x0 = min(max(0, x0), roi.shape[1] - pw)
        block, ba = rgb[y0:y0 + ph, x0:x0 + pw], alpha[y0:y0 + ph, x0:x0 + pw]
        if int((ba > 40).sum()) >= 4:
            samples.append(np.median(block[ba > 40], axis=0))
    if len(samples) < 2:
        return 0
    plate = np.median(np.stack(samples), axis=0).astype(np.int16)
    distance = np.abs(rgb.astype(np.int16) - plate).sum(axis=2)
    luma = rgb.max(axis=2)
    ink = ((distance > 48) | (luma > 250) | (luma < 18)) & (alpha > 30)
    ink = ink.astype(np.uint8)
    ink = cv2.morphologyEx(ink, cv2.MORPH_OPEN, np.ones((2, 2), np.uint8))
    ink = cv2.dilate(ink, np.ones((3, 3), np.uint8), iterations=1)
    if textured:
        painted = cv2.inpaint(rgb, ink * 255, 3, cv2.INPAINT_NS)
        roi[ink > 0, :3] = painted[ink > 0]
    else:
        roi[ink > 0, :3] = np.clip(plate, 0, 255).astype(np.uint8)
    return float(ink.mean())


def erase_clear(image, box):
    left, top, right, bottom = box
    roi = image[top:bottom, left:right]
    roi[roi[:, :, 3] > 8, :4] = 0


def erase_caption(image, box):
    erase_lights(image, box, "white")


def glyph_mask(roi, glyphs):
    rgb = roi[:, :, :3]
    alpha = roi[:, :, 3]
    luma = rgb.max(axis=2)
    saturation = luma.astype(np.int16) - rgb.min(axis=2).astype(np.int16)
    white = (luma > 190) & (saturation < 80) & (alpha > 60)
    yellow = (
        (rgb[:, :, 0] > 185)
        & (rgb[:, :, 1] > 90)
        & (rgb[:, :, 2] < 170)
        & (rgb[:, :, 0] > rgb[:, :, 2] + 40)
        & (luma > 150)
        & (alpha > 60)
    )
    bright = (luma > 210) & (alpha > 60)
    if glyphs == "yellow":
        seed = yellow
    elif glyphs == "bright":
        seed = bright
    elif glyphs == "both":
        seed = white | yellow
    else:
        seed = white
    seed = seed.astype(np.uint8)
    dark = ((luma < 70) & (alpha > 60)).astype(np.uint8)
    near = cv2.dilate(seed, np.ones((9, 9), np.uint8))
    ink = ((seed > 0) | ((dark > 0) & (near > 0))).astype(np.uint8)
    return cv2.dilate(ink, np.ones((3, 3), np.uint8), iterations=1)


def erase_lights(image, box, glyphs="white"):
    """Remove bright lettering and its dark outline. Opaque artwork stays."""
    left, top, right, bottom = box
    roi = image[top:bottom, left:right]
    rgb = roi[:, :, :3]
    alpha = roi[:, :, 3]
    ink = glyph_mask(roi, glyphs)
    colors = [None] * ink.shape[0]
    for row in range(ink.shape[0]):
        keep = (ink[row] == 0) & (alpha[row] > 40)
        if int(keep.sum()) >= 4:
            colors[row] = np.median(rgb[row][keep], axis=0)
    known = [i for i, color in enumerate(colors) if color is not None]
    if not known:
        return
    for row in range(ink.shape[0]):
        color = colors[row]
        if color is None:
            color = colors[min(known, key=lambda i: abs(i - row))]
        selected = ink[row] > 0
        roi[row, selected, :3] = color


def erase_inpaint(image, box, glyphs="both"):
    """Rebuild the plate under thick lettering instead of flattening the button."""
    left, top, right, bottom = box
    roi = image[top:bottom, left:right]
    ink = glyph_mask(roi, glyphs)
    painted = cv2.inpaint(roi[:, :, :3], ink * 255, 5, cv2.INPAINT_TELEA)
    roi[ink > 0, :3] = painted[ink > 0]


def erase_flat(image, box):
    """Paint a flat brown plate over lettering. Dividers outside the box stay."""
    rgb = image[:, :, :3]
    luma = rgb.max(axis=2)
    alpha = image[:, :, 3]
    sample = rgb[(luma > 90) & (luma < 150) & (alpha > 200)]
    if len(sample) < 8:
        return
    plate = np.median(sample, axis=0)
    left, top, right, bottom = box
    roi = image[top:bottom, left:right]
    dist = np.abs(roi[:, :, :3].astype(np.int16) - plate).sum(axis=2)
    ink = ((dist > 36) & (roi[:, :, 3] > 40)).astype(np.uint8)
    ink = cv2.dilate(ink, np.ones((3, 3), np.uint8), iterations=1)
    roi[ink > 0, :3] = np.clip(plate, 0, 255).astype(np.uint8)


def erase_yellow_label(image, box):
    """Remove dark lettering and its cream outline from a yellow tab without flattening the stripes."""
    left, top, right, bottom = box
    roi = image[top:bottom, left:right]
    rgb = roi[:, :, :3]
    alpha = roi[:, :, 3]
    luma = rgb.max(axis=2)
    dark = ((luma < 145) & (alpha > 80)).astype(np.uint8)
    if int(dark.sum()) < 8:
        return
    grown = cv2.dilate(dark, np.ones((17, 17), np.uint8))
    cream = ((rgb[:, :, 0] > 230) & (rgb[:, :, 1] > 210) & (rgb[:, :, 2] > 140) & (alpha > 80)).astype(np.uint8)
    ink = ((dark > 0) | ((cream > 0) & (grown > 0))).astype(np.uint8)
    ink = cv2.dilate(ink, np.ones((3, 3), np.uint8), iterations=1)
    painted = cv2.inpaint(rgb, ink * 255, 4, cv2.INPAINT_NS)
    roi[ink > 0, :3] = painted[ink > 0]


def erase_dark(image, box, thresh=120):
    """Remove dark lettering from a light plate. Borders outside the box stay."""
    left, top, right, bottom = box
    roi = image[top:bottom, left:right]
    rgb = roi[:, :, :3]
    alpha = roi[:, :, 3]
    luma = rgb.max(axis=2)
    light = (luma > 170) & (alpha > 40)
    if int(light.sum()) < 8:
        return
    plate = np.median(rgb[light], axis=0)
    ink = ((luma < thresh) & (alpha > 40)).astype(np.uint8)
    ink = cv2.dilate(ink, np.ones((3, 3), np.uint8), iterations=1)
    roi[ink > 0, :3] = np.clip(plate, 0, 255).astype(np.uint8)


def paint_upgrade_pill(image):
    """Replace the Chinese label inside a table card's yellow upgrade pill."""
    height, width = image.shape[:2]
    rgb = image[:, :, :3]
    alpha = image[:, :, 3]
    r, g, b = rgb[:, :, 0], rgb[:, :, 1], rgb[:, :, 2]
    yellow = ((alpha > 200) & (r > 230) & (g > 185) & (b < 170) & (r.astype(np.int16) > b.astype(np.int16) + 50)).astype(np.uint8)
    yellow[: int(height * 0.72)] = 0
    count, labels = cv2.connectedComponents(yellow, connectivity=8)
    best = None
    for i in range(1, count):
        ys, xs = np.where(labels == i)
        if len(xs) < 400:
            continue
        x0, x1, y0, y1 = int(xs.min()), int(xs.max()), int(ys.min()), int(ys.max())
        bw, bh = x1 - x0 + 1, y1 - y0 + 1
        if bw < width * 0.35 or bw > width * 0.85 or bh < 24 or bh > height * 0.16:
            continue
        if abs((x0 + x1) / 2 - width / 2) > width * 0.12:
            continue
        if best is None or len(xs) > best[0]:
            best = (len(xs), x0, y0, x1, y1)
    if best is None:
        raise SystemExit("upgrade pill not found")
    _, x0, y0, x1, y1 = best
    # Erase almost the whole yellow face. The old strokes reach close to the rim.
    left, top, right, bottom = x0 + 8, y0 + 3, x1 - 8, y1 - 3
    roi = rgb[top:bottom, left:right]
    luma = roi.max(axis=2)
    plate = np.median(roi[luma > 210], axis=0).astype(np.int16)
    distance = np.abs(roi.astype(np.int16) - plate).sum(axis=2)
    ink = ((distance > 36) | (luma < 185) | (luma > 248)).astype(np.uint8)
    ink = cv2.morphologyEx(ink, cv2.MORPH_CLOSE, np.ones((3, 3), np.uint8))
    ink = cv2.dilate(ink, np.ones((3, 3), np.uint8), iterations=2)
    painted = cv2.inpaint(roi, ink * 255, 3, cv2.INPAINT_NS)
    roi[ink > 0] = painted[ink > 0]
    label_x = max(12, int((x1 - x0) * 0.16))
    label_y = max(8, int((y1 - y0) * 0.18))
    draw_text(image, (x0 + label_x, y0 + label_y, x1 - label_x, y1 - label_y), "Upgrade", WHITE)
    print("  pill", x0, y0, x1, y1, "ink", int(ink.sum()))


def draw_text(image, box, text, style, angle=0):
    if not text:
        return
    fill, stroke, ratio, font_path = style
    left, top, right, bottom = box
    width, height = right - left, bottom - top
    lines = text.split("\n")
    size, stroke_w, font = 8, 1, ImageFont.truetype(str(font_path), 8)
    limit_w = width * (0.72 if angle else 0.94)
    limit_h = height * (0.72 if angle else 0.9)
    while size < max(height, width):
        trial = ImageFont.truetype(str(font_path), size + 1)
        trial_stroke = max(0, int(round((size + 1) * ratio)))
        boxes = [trial.getbbox(line, stroke_width=trial_stroke) for line in lines]
        tw = max(b[2] - b[0] for b in boxes)
        th = sum(b[3] - b[1] for b in boxes) + int(size * 0.06) * (len(lines) - 1)
        if tw > limit_w or th > limit_h:
            break
        size, stroke_w, font = size + 1, trial_stroke, trial
    overlay = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    pen = ImageDraw.Draw(overlay)
    boxes = [font.getbbox(line, stroke_width=stroke_w) for line in lines]
    heights = [b[3] - b[1] for b in boxes]
    y0 = (height - (sum(heights) + int(size * 0.06) * (len(lines) - 1))) // 2
    for line, bounds, line_height in zip(lines, boxes, heights):
        pen.text(
            ((width - (bounds[2] - bounds[0])) / 2 - bounds[0], y0 - bounds[1]),
            line, font=font, fill=fill, stroke_width=stroke_w, stroke_fill=stroke,
        )
        y0 += line_height + int(size * 0.06)
    if angle:
        overlay = overlay.rotate(angle, resample=Image.Resampling.BICUBIC, expand=False)
    plate = Image.fromarray(image[top:bottom, left:right], "RGBA")
    image[top:bottom, left:right] = np.array(Image.alpha_composite(plate, overlay))


def paint(image, spec):
    if spec.get("mode") == "pill":
        paint_upgrade_pill(image)
        return
    box = zone_box(image.shape, spec["box"])
    mode = spec.get("mode", "plate")
    if mode == "plate":
        cover = erase_plate(image, box, spec.get("textured", False))
        if cover > 0.9:
            print("  wide erase", round(cover, 2), spec.get("text"))
    elif mode == "clear":
        erase_clear(image, box)
    elif mode == "caption":
        erase_caption(image, box)
    elif mode == "lights":
        erase_lights(image, box, spec.get("glyphs", "white"))
    elif mode == "inpaint":
        erase_inpaint(image, box, spec.get("glyphs", "both"))
    elif mode == "dark":
        erase_dark(image, box, spec.get("thresh", 120))
    elif mode == "flat":
        erase_flat(image, box)
    elif mode == "yellow":
        erase_yellow_label(image, box)
    elif mode == "draw":
        pass
    draw_text(image, box, spec.get("text", ""), spec.get("style", WHITE), spec.get("angle", 0))


def build(key, specs):
    image = np.array(Image.open(SOURCE / f"{key}.png").convert("RGBA"))
    for spec in specs:
        paint(image, spec)
    DEST.mkdir(parents=True, exist_ok=True)
    Image.fromarray(image, "RGBA").save(DEST / f"{key}.png")


# box is the fraction of the canvas that contains the baked lettering.
CAPTION = (0.0, 0.56, 1.0, 0.42)
BUTTON = (0.08, 0.14, 0.84, 0.72)
TITLE = (0.12, 0.16, 0.76, 0.66)
LABEL = (0.04, 0.02, 0.92, 0.96)

JOBS = {
    "qd_pic_bg_scale2": [{"box": (0.04, 0.008, 0.58, 0.112), "text": "7-DAY\nCHECK-IN", "style": CREAM, "mode": "lights"}],
    "qd_zi_as": [{"box": LABEL, "text": "Ad Rewards", "style": BROWN}],
    "qd_zi_nomorl": [{"box": LABEL, "text": "Free Rewards", "style": BROWN}],
    "ty_icon_qiandao": [{"box": (0.0, 0.60, 1, 0.38), "text": "Check-In", "mode": "caption"}],
    "ty_pic_title_setup": [{"box": TITLE, "text": "Settings", "style": CREAM, "textured": True}],
    "zjm_pic_chenghao1": [{"box": (0.08, 0.16, 0.84, 0.30), "text": "Growth", "style": GOLD, "mode": "caption"}],
    "zjm_btn_shangcheng01": [{"box": CAPTION, "text": "Shop", "mode": "caption"}],
    "zjm_btn_sheshi01": [{"box": (0.0, 0.60, 1, 0.38), "text": "Helpers", "mode": "caption"}],
    "zjm_btn_shijie01": [{"box": CAPTION, "text": "Worlds", "mode": "caption"}],
    "zjm_btn_tianfu01": [{"box": CAPTION, "text": "Talents", "mode": "caption"}],
    "zjm_btn_wupin01": [{"box": (0.0, 0.60, 1, 0.38), "text": "Items", "mode": "caption"}],
    "zjm_btn_tiaozhan01": [{"box": CAPTION, "text": "Challenge", "mode": "caption"}],
    "ad_icon_dafuweng": [{"box": CAPTION, "text": "Tycoon", "mode": "caption"}],
    "ad_icon_guwudashi": [{"box": CAPTION, "text": "Cheer", "mode": "caption"}],
    "ad_icon_zhuanqiandashi": [{"box": CAPTION, "text": "Earn", "mode": "caption"}],
    "ad_btn_summon": [{"box": BUTTON, "text": "Summon", "style": WHITE}],
    "ad_pic_title_dafuweng": [{"box": TITLE, "text": "Tycoon", "style": CREAM, "textured": True}],
    "ad_pic_title_guwudashi": [{"box": TITLE, "text": "Cheer Master", "style": CREAM, "textured": True}],
    "ad_pic_title_zhuanqiandashi": [{"box": TITLE, "text": "Earnings\nMaster", "style": CREAM, "textured": True}],
    "bs_btn_unlock": [{"box": BUTTON, "text": "Early Unlock", "style": WHITE}],
    "bs_btn_subscribe": [{"box": BUTTON, "text": "Subscription\nUnlock", "style": WHITE}],
    "bs_zi_juexingjineng": [{"box": (0.26, 0.04, 0.72, 0.92), "text": "Awakened\nSkill", "mode": "clear"}],
    "lxsy_btn_queding": [{"box": BUTTON, "text": "Confirm", "style": WHITE}],
    "lxsy_btn_shuangbeihuoqu": [{"box": BUTTON, "text": "Double Reward", "style": WHITE}],
    "lxsy_pic_title_lixianshouyi": [{"box": TITLE, "text": "Offline\nEarnings", "style": CREAM, "textured": True}],
    "qd_lab_get": [{"box": LABEL, "text": "Get", "mode": "clear"}],
    "shop_zi_ad": [{"box": (0.30, 0.0, 0.70, 1.0), "text": "Ad Draw", "mode": "clear"}],
    "shop_zi_everydayFree": [{"box": LABEL, "text": "First Free Daily", "mode": "clear"}],
    "shop_zi_srDes": [
        {"box": (0.0, 0.0, 0.13, 1.0), "text": "Every", "mode": "clear"},
        {"box": (0.28, 0.0, 0.72, 1.0), "text": "pulls: SR shards!", "mode": "clear"},
    ],
    "sw_bac_jiayou_": [{"box": (0.06, 0.08, 0.88, 0.84), "text": "GO!\nGO!", "style": INK}],
    "talent_btn_cancle": [{"box": BUTTON, "text": "Cancel", "style": WHITE}],
    "talent_btn_reborn": [{"box": BUTTON, "text": "Rebirth", "style": WHITE}],
    "talent_btn_reset": [{"box": BUTTON, "text": "Reset", "style": WHITE}],
    "talent_btn_reset02": [{"box": BUTTON, "text": "Reset", "style": WHITE}],
    "talent_btn_yellow": [{"box": BUTTON, "text": "Rebirth", "style": WHITE}],
    # Blank the three words. The lock slot and the dividers stay. Tab plates carry the English labels.
    "talent_pic_tabBG": [
        {"box": (0.07, 0.16, 0.16, 0.68), "text": "", "mode": "flat"},
        {"box": (0.30, 0.16, 0.16, 0.68), "text": "", "mode": "flat"},
        {"box": (0.53, 0.16, 0.17, 0.68), "text": "", "mode": "flat"},
    ],
    "talent_pic_tabBG_coin": [
        {"box": (0.14, 0.0, 0.72, 1.0), "text": "", "mode": "flat"},
        {"box": (0.12, 0.10, 0.76, 0.80), "text": "Coins", "style": CREAM, "mode": "draw"},
    ],
    "talent_pic_tabBG_helper": [
        {"box": (0.16, 0.0, 0.70, 1.0), "text": "", "mode": "flat"},
        {"box": (0.06, 0.12, 0.88, 0.76), "text": "Helpers", "style": CREAM, "mode": "draw"},
    ],
    "talent_pic_tabBG_special": [
        {"box": (0.14, 0.0, 0.72, 1.0), "text": "", "mode": "flat"},
        # The notification dot covers the right 19% of this plate.
        {"box": (0.04, 0.16, 0.66, 0.68), "text": "Special", "style": CREAM, "mode": "draw"},
    ],
    "talent_pic_tabBangshou": [
        {"box": (0.10, 0.22, 0.80, 0.58), "text": "", "mode": "yellow"},
        {"box": (0.10, 0.32, 0.80, 0.38), "text": "Helpers", "style": CREAM, "mode": "draw"},
    ],
    "talent_pic_tabJinqian": [
        {"box": (0.10, 0.22, 0.80, 0.58), "text": "", "mode": "yellow"},
        {"box": (0.16, 0.30, 0.68, 0.40), "text": "Coins", "style": CREAM, "mode": "draw"},
    ],
    "talent_pic_tabTeshu": [
        {"box": (0.10, 0.22, 0.80, 0.58), "text": "", "mode": "yellow"},
        {"box": (0.06, 0.32, 0.64, 0.36), "text": "Special", "style": CREAM, "mode": "draw"},
    ],
    "talent_pic_talentPoint": [{"box": (0.02, 0.52, 0.96, 0.24), "text": "Talent\nPoints", "mode": "caption"}],
    "talent_pic_talentPoint_dice": [{"box": (0.02, 0.52, 0.96, 0.24), "text": "Talent\nPoints", "mode": "caption"}],
    "talent_pic_title_reborn": [{"box": TITLE, "text": "Rebirth", "style": CREAM, "textured": True}],
    "talent_pic_title_reset": [{"box": TITLE, "text": "Reset", "style": CREAM, "textured": True}],
    "ty_btn_challenge": [
        {"box": (0.40, 0.14, 0.56, 0.36), "text": "Enter Challenge", "mode": "lights", "glyphs": "both"},
        {"box": (0.40, 0.52, 0.56, 0.36), "text": "Title Promotion", "mode": "lights", "glyphs": "both"},
    ],
    "ty_btn_exitChallenge": [{"box": (0.08, 0.18, 0.84, 0.64), "text": "Leave\nChallenge", "mode": "lights", "glyphs": "both"}],
    "ty_btn_quxiao": [{"box": BUTTON, "text": "Cancel", "style": WHITE}],
    "ty_pic_challenge_fail_scale2": [
        {"box": (0.18, 0.62, 0.64, 0.13), "text": "FAILED", "style": WHITE, "mode": "lights"},
        {"box": (0.12, 0.80, 0.76, 0.12), "text": "Upgrade and try again", "mode": "lights"},
    ],
    "ty_pic_challenge_successl_scale2": [{"box": (0.20, 0.66, 0.60, 0.11), "text": "COMPLETE!", "style": CREAM, "mode": "lights", "glyphs": "yellow"}],
    "ty_pic_countdown": [{"box": (0.02, 0.04, 0.96, 0.92), "text": "Time Left", "mode": "clear"}],
    "ty_pic_title_suipianbuzu": [{"box": TITLE, "text": "Not Enough\nShards", "style": CREAM, "textured": True}],
    "ty_title_gongxihuode 1": [{"box": (0.02, 0.58, 0.96, 0.40), "text": "REWARDS!", "style": GOLD, "mode": "clear"}],
    "ty_zi_cleanSave": [{"box": LABEL, "text": "Clear Save", "mode": "clear"}],
    "ty_zi_gengduo": [{"box": LABEL, "text": "More", "mode": "clear"}],
    "ty_zi_qushangcheng": [{"box": LABEL, "text": "Go to Shop", "mode": "clear"}],
    "ty_zi_shouqi": [{"box": LABEL, "text": "Less", "mode": "clear"}],
    "ty_zi_upgrade": [{"box": LABEL, "text": "Upgrade", "mode": "clear"}],
    "ty_zi_restoreSave": [{"box": LABEL, "text": "Restore Save", "mode": "clear"}],
    "world_zi_dangqian": [{"box": (0.16, 0.12, 0.68, 0.74), "text": "NOW", "style": WHITE, "mode": "lights", "angle": 22}],
    "xsyd_yijies": [{"box": (0.12, 0.08, 0.76, 0.42), "text": "Unlocked", "style": INK, "mode": "dark"}],
    "xsyd_zhuanqianshengji": [{"box": (0.40, 0.06, 0.52, 0.88), "text": "EARN\nUP", "style": INK}],
    "zm_btn_bs_upgrade": [{"box": BUTTON, "text": "Upgrade", "style": WHITE}],
    # Upgrade is painted into the table card. Keep the yellow pill, notches and frame.
    "zm_pic_zm_notInUse": [{"box": (0, 0, 1, 1), "mode": "pill"}],
    "zm_pic_zm_inUse": [{"box": (0, 0, 1, 1), "mode": "pill"}],
    "zm_btn_use": [{"box": BUTTON, "text": "Use", "style": WHITE}],
    "zm_lab_inUse": [{"box": (0.16, 0.22, 0.68, 0.56), "text": "In Use", "style": INK, "mode": "dark"}],
    "zm_pic_bs_count": [{"box": (0.0, 0.74, 0.52, 0.22), "text": "Available\nHelpers", "style": CREAM, "mode": "lights"}],
    "zm_pic_using": [{"box": (0.14, 0.18, 0.72, 0.64), "text": "In Use", "style": GOLD, "mode": "clear"}],
    "zm_tags_bs": [{"box": (0.12, 0.18, 0.76, 0.64), "text": "Helpers", "style": CREAM, "textured": True}],
    "zm_tags_zm": [{"box": (0.12, 0.18, 0.76, 0.64), "text": "Tables", "style": CREAM, "textured": True}],
    "zm_zi_cailiaobuzu": [{"box": LABEL, "text": "Missing\nMaterials", "mode": "clear"}],
    "qd_zi_dingyue": [{"box": LABEL, "text": "Subscribe", "mode": "clear"}],
    "lv8Dice_pic_slogn_newHelper_x2": [{"box": (0.20, 0.04, 0.78, 0.92), "text": "A New Helper\nArrives!", "mode": "clear"}],
    "lv8Dice_pic_slogn_nextStation_x2": [{"box": (0.04, 0.08, 0.92, 0.84), "text": "Next Stop: Dice Island", "style": CREAM}],
    "lv8Dice_pic_title_main_x2": [{"box": (0.04, 0.1, 0.92, 0.8), "text": "New World\nComing Soon", "style": CREAM, "textured": True}],
    "lv8Dice_pic_title_sub_x2": [{"box": (0.02, 0.0, 0.96, 1.0), "text": "A New Adventure Awaits!", "style": CREAM}],
    "lv8Dice_zi_jingqingqidai": [{"box": LABEL, "text": "Coming Soon", "mode": "clear"}],
    "zm_lab_sign3": [
        {"box": (0.04, 0.02, 0.78, 0.40), "text": "GET", "mode": "lights"},
        {"box": (0.02, 0.48, 0.46, 0.42), "text": "DAY", "mode": "lights"},
        {"box": (0.70, 0.48, 0.26, 0.42), "text": "", "mode": "lights"},
    ],
    "zm_lab_sign7": [
        {"box": (0.04, 0.02, 0.78, 0.40), "text": "GET", "mode": "lights"},
        {"box": (0.02, 0.48, 0.46, 0.42), "text": "DAY", "mode": "lights"},
        {"box": (0.70, 0.48, 0.26, 0.42), "text": "", "mode": "lights"},
    ],
    "caidai_jiayou": [{"box": (0.0, 0.0, 1.0, 1.0), "text": "GO!", "style": DARK, "mode": "clear", "angle": -12}],
    "caidai_zhuanqian": [{"box": (0.0, 0.0, 1.0, 1.0), "text": "EARN!", "style": DARK, "mode": "clear", "angle": -12}],
    "ty_pic_dachengmubiao_scale2": [
        {"box": (0.06, 0.17, 0.42, 0.13), "text": "", "style": PAPER, "mode": "dark"},
        {"box": (0.06, 0.30, 0.28, 0.12), "text": "", "style": PAPER, "mode": "dark"},
        {"box": (0.04, 0.16, 0.42, 0.26), "text": "GROWTH\nLEVEL UP!", "style": PAPER, "mode": "draw"},
        {"box": (0.16, 0.435, 0.40, 0.07), "text": "EXCLUSIVE SCOOP:", "mode": "lights"},
        {"box": (0.14, 0.505, 0.42, 0.08), "text": "IDENTITY VERIFIED!!", "mode": "lights"},
        {"box": (0.60, 0.425, 0.34, 0.075), "text": "CITY HALL OK!!!", "style": PAPER, "mode": "dark"},
        {"box": (0.60, 0.51, 0.34, 0.12), "text": "SECRET FUND", "style": PAPER, "mode": "dark"},
    ],
}


def main():
    chosen = sys.argv[1:] or list(JOBS)
    missing = [key for key in chosen if key not in JOBS or not (SOURCE / f"{key}.png").exists()]
    if missing:
        raise SystemExit("missing originals: " + ", ".join(missing))
    for key in chosen:
        build(key, JOBS[key])
        print("wrote", key)
    print("count", len(chosen))


if __name__ == "__main__":
    main()

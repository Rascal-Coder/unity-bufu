"""Build the English loading background (ty_pic_kaolu_bg_scale2) for Tycoon Rush.

The Chinese title on the wooden board and the healthy-gaming advisory are removed by
masking only their lettering and inpainting it, then English text is drawn in the same
places. The original PNG is never modified; the result keeps its 1024x2048 size.
"""
from pathlib import Path

import cv2
import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageFont

ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / "Assets/Resources/UI/Sprites/ty_pic_kaolu_bg_scale2.png"
DEST = ROOT / "Assets/Resources/UI/Sprites/en/ty_pic_kaolu_bg_scale2.png"
TITLE_FONT = Path(r"C:/Windows/Fonts/ariblk.ttf")
BODY_FONT = Path(r"C:/Windows/Fonts/arialbd.ttf")

# Pixel boxes measured on the original (left, top, right, bottom).
TITLE_TOP = (315, 188, 690, 392)      # 不服, lime with a dark outline
TITLE_BOTTOM = (285, 383, 748, 620)   # 开一局, white/lavender with a purple outline
ADVISORY_HEAD = (318, 1632, 702, 1708)
ADVISORY_BODY = (212, 1730, 828, 1958)

ADVISORY_TITLE = "Healthy Gaming Advisory"
ADVISORY_LINES = [
    "Say no to harmful and pirated games.",
    "Protect yourself and beware of scams.",
    "Play in moderation; overplaying is harmful.",
    "Manage your time and live a healthy life.",
]


def region(shape, box):
    left, top, right, bottom = box
    inside = np.zeros(shape, bool)
    inside[top:bottom, left:right] = True
    return inside


def title_mask(rgb):
    pixels = rgb.astype(np.int16)
    r, g, b = pixels[..., 0], pixels[..., 1], pixels[..., 2]
    top, bottom = region(r.shape, TITLE_TOP), region(r.shape, TITLE_BOTTOM)
    lime = ((g > r + 15) & (g > b + 15) & (g > 60)) | ((g > b + 50) & (g >= r - 5) & (g > 120))
    lavender = ((b > r + 15) & (b > g + 10)) | (pixels.min(axis=2) > 170)
    seed = ((lime & top) | (lavender & bottom)).astype(np.uint8)
    near = cv2.dilate(seed, np.ones((19, 19), np.uint8)) > 0
    ink = (seed > 0) | ((pixels.max(axis=2) < 110) & near & (top | bottom))
    ink &= coin_mask(rgb) == 0
    ink = cv2.morphologyEx(ink.astype(np.uint8), cv2.MORPH_CLOSE, np.ones((9, 9), np.uint8))
    return cv2.dilate(ink, np.ones((5, 5), np.uint8))


def coin_mask(rgb):
    """The round coins that sit in front of the logo; the board frame is the same gold but not round."""
    pixels = rgb.astype(np.int16)
    r, g, b = pixels[..., 0], pixels[..., 1], pixels[..., 2]
    gold = ((r > 190) & (g > 120) & (b < 120) & (r > g + 20)).astype(np.uint8)
    count, labels, stats, _ = cv2.connectedComponentsWithStats(gold)
    coins = np.zeros_like(gold)
    for i in range(1, count):
        x, y, w, h, area = stats[i]
        if area > 2500 and y < 700 and y + h > 350 and 0.6 < w / h < 1.6 and area / (w * h) > 0.45:
            outline, _ = cv2.findContours((labels == i).astype(np.uint8), cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)
            cv2.drawContours(coins, [cv2.convexHull(outline[0])], -1, 1, -1)
    return cv2.dilate(coins, cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (13, 13)))


def advisory_mask(rgb, box):
    left, top, right, bottom = box
    roi = rgb[top:bottom, left:right].astype(np.int16)
    r, g, b = roi[..., 0], roi[..., 1], roi[..., 2]
    seed = ((r > 225) & (g > 205) & (b > 150)).astype(np.uint8)
    near = cv2.dilate(seed, np.ones((15, 15), np.uint8))
    ink = (seed > 0) | ((roi.max(axis=2) < 95) & (near > 0))
    ink = cv2.morphologyEx(ink.astype(np.uint8), cv2.MORPH_CLOSE, np.ones((5, 5), np.uint8))
    full = np.zeros(rgb.shape[:2], np.uint8)
    full[top:bottom, left:right] = cv2.dilate(ink, np.ones((5, 5), np.uint8))
    return full


def inpaint(image, mask, radius):
    rgb = np.ascontiguousarray(image[..., :3])
    painted = cv2.inpaint(rgb, mask * 255, radius, cv2.INPAINT_NS)
    image[..., :3][mask > 0] = painted[mask > 0]


def shade(image, box, radius, alpha):
    """Frosted dark panel behind the advisory; hides inpaint seams on the busy coin pile."""
    left, top, right, bottom = box
    panel = Image.new("L", (image.shape[1], image.shape[0]), 0)
    ImageDraw.Draw(panel).rounded_rectangle(box, radius, fill=alpha)
    panel = np.array(panel.filter(ImageFilter.GaussianBlur(18)), np.float32)[..., None] / 255
    frosted = cv2.GaussianBlur(image[..., :3], (0, 0), 14).astype(np.float32)
    frosted = frosted * (1 - alpha / 255) + np.array([40, 22, 8]) * (alpha / 255)
    weight = np.clip(panel * 255 / alpha, 0, 1)
    image[..., :3] = (image[..., :3] * (1 - weight) + frosted * weight).astype(np.uint8)


def draw_logo(image, text, box, face_top, face_bottom, side, outline, depth):
    """Arial Black stretched to fill the old glyph footprint, with the original logo's extruded edge."""
    left, top, right, bottom = box
    width, height = right - left, bottom - top
    font = ImageFont.truetype(str(TITLE_FONT), 200)
    bounds = font.getbbox(text)
    text_w, text_h = bounds[2] - bounds[0], bounds[3] - bounds[1]
    pad, stroke = 40, 14

    def shape(stroke_width):
        canvas = Image.new("L", (text_w + 2 * pad, text_h + 2 * pad), 0)
        ImageDraw.Draw(canvas).text((pad - bounds[0], pad - bounds[1]), text, font=font, fill=255,
                                    stroke_width=stroke_width, stroke_fill=255)
        return canvas

    scale_x, scale_y = width / (text_w + 2 * stroke), (height - depth) / (text_h + 2 * stroke)
    size = (int((text_w + 2 * pad) * scale_x), int((text_h + 2 * pad) * scale_y))
    body = shape(0).resize(size, Image.LANCZOS)
    edge = shape(stroke).resize(size, Image.LANCZOS)
    x, y = left - int((pad - stroke) * scale_x), top - int((pad - stroke) * scale_y)

    layer = Image.new("RGBA", (image.shape[1], image.shape[0]), (0, 0, 0, 0))
    for d in range(depth, 0, -1):
        layer.paste(Image.new("RGBA", size, outline), (x, y + d), edge)
    for d in range(depth, 0, -1):
        layer.paste(Image.new("RGBA", size, side), (x, y + d), body)
    layer.paste(Image.new("RGBA", size, outline), (x, y), edge)
    ramp = np.linspace(0, 1, size[1])[:, None, None]
    face = (np.array(face_top) * (1 - ramp) + np.array(face_bottom) * ramp).astype(np.uint8)
    face = np.concatenate([np.repeat(face, size[0], axis=1), np.full((size[1], size[0], 1), 255, np.uint8)], axis=2)
    layer.paste(Image.fromarray(face, "RGBA"), (x, y), body)
    image[:] = np.array(Image.alpha_composite(Image.fromarray(image, "RGBA"), layer))


def fit_font(path, lines, width, height, stroke_ratio, gap_ratio):
    size = 8
    while True:
        font = ImageFont.truetype(str(path), size + 1)
        stroke = max(1, round((size + 1) * stroke_ratio))
        boxes = [font.getbbox(line, stroke_width=stroke) for line in lines]
        w = max(b[2] - b[0] for b in boxes)
        h = sum(b[3] - b[1] for b in boxes) + int((size + 1) * gap_ratio) * (len(lines) - 1)
        if w > width or h > height:
            return size
        size += 1


def draw_lines(image, box, lines, path, fill, stroke_fill, stroke_ratio, gap_ratio=0.18, shadow=None):
    left, top, right, bottom = box
    width, height = right - left, bottom - top
    size = fit_font(path, lines, width, height, stroke_ratio, gap_ratio)
    font = ImageFont.truetype(str(path), size)
    stroke = max(1, round(size * stroke_ratio))
    boxes = [font.getbbox(line, stroke_width=stroke) for line in lines]
    heights = [b[3] - b[1] for b in boxes]
    gap = int(size * gap_ratio)
    y = top + (height - sum(heights) - gap * (len(lines) - 1)) // 2
    layer = Image.new("RGBA", (image.shape[1], image.shape[0]), (0, 0, 0, 0))
    pen = ImageDraw.Draw(layer)
    for line, bounds, line_height in zip(lines, boxes, heights):
        x = left + (width - (bounds[2] - bounds[0])) // 2 - bounds[0]
        if shadow:
            pen.text((x + shadow[0], y - bounds[1] + shadow[1]), line, font=font, fill=stroke_fill,
                     stroke_width=stroke, stroke_fill=stroke_fill)
        pen.text((x, y - bounds[1]), line, font=font, fill=fill, stroke_width=stroke, stroke_fill=stroke_fill)
        y += line_height + gap
    base = Image.fromarray(image, "RGBA")
    image[:] = np.array(Image.alpha_composite(base, layer))


def main():
    source = np.array(Image.open(SOURCE).convert("RGBA"))
    image = source.copy()
    inpaint(image, title_mask(source[..., :3]), 9)
    inpaint(image, advisory_mask(source[..., :3], ADVISORY_HEAD) | advisory_mask(source[..., :3], ADVISORY_BODY), 5)

    draw_logo(image, "TYCOON", (312, 212, 698, 388), (222, 246, 70), (160, 222, 48),
              (40, 150, 80, 255), (74, 38, 20, 255), 12)
    draw_logo(image, "RUSH", (285, 392, 745, 615), (255, 255, 255), (214, 208, 246),
              (112, 96, 200, 255), (58, 36, 110, 255), 14)
    coins = coin_mask(source[..., :3]) > 0
    image[coins] = source[coins]

    shade(image, (190, 1625, 834, 1965), 40, 150)
    cream, brown = (255, 244, 214, 255), (72, 40, 16, 255)
    draw_lines(image, ADVISORY_HEAD, [ADVISORY_TITLE], BODY_FONT, cream, brown, 0.09)
    draw_lines(image, ADVISORY_BODY, ADVISORY_LINES, BODY_FONT, cream, brown, 0.08, gap_ratio=0.32)

    DEST.parent.mkdir(parents=True, exist_ok=True)
    Image.fromarray(image, "RGBA").save(DEST)
    print("wrote", DEST)


if __name__ == "__main__":
    main()

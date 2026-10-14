# -*- coding: utf-8 -*-
"""Print the serialized fields of original scripts in an exported prefab: python Tools/dump_refs.py Common/upgradePanel [Class]"""
import json
import os
import sys

ROOT = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "Assets", "Resources", "UI", "Prefabs")
SKIP = {"Image", "TextMeshProUGUI", "Button", "Toggle", "Slider", "ScrollRect", "Mask", "RectMask2D", "HorizontalLayoutGroup",
        "VerticalLayoutGroup", "GridLayoutGroup", "ContentSizeFitter", "LayoutElement", "CanvasGroup", "Canvas", "GraphicRaycaster",
        "SkeletonGraphic", "UpDownAnimator", "LeftRightAnimator", "TextMeshProOutline", "RawImage", "UISpriteAnimationSequence"}


def show(value, nodes):
    if isinstance(value, dict):
        if "node" in value:
            return "#%d %s%s" % (value["node"], nodes[value["node"]]["n"], (" (" + value["comp"] + ")") if "comp" in value else "")
        return json.dumps(value, ensure_ascii=False)[:160]
    if isinstance(value, list):
        return "[" + ", ".join(show(v, nodes) for v in value) + "]"
    return json.dumps(value, ensure_ascii=False)[:160]


def main():
    sys.stdout.reconfigure(encoding="utf-8")
    data = json.load(open(os.path.join(ROOT, sys.argv[1] + ".json"), encoding="utf-8"))
    only = sys.argv[2] if len(sys.argv) > 2 else None
    nodes = data["nodes"]
    for i, n in enumerate(nodes):
        for c in n["c"]:
            if c["t"] in SKIP or (only and c["t"] != only):
                continue
            print("== %s on #%d %s" % (c["t"], i, n["n"]))
            for k, v in c.items():
                if k in ("t", "m_Enabled"):
                    continue
                print("  %s = %s" % (k, show(v, nodes)))


if __name__ == "__main__":
    main()

# -*- coding: utf-8 -*-
"""Print an exported prefab as an indented tree: python Tools/dump_tree.py Common/topPanel [...]"""
import json
import os
import sys

ROOT = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "Assets", "Resources", "UI", "Prefabs")


def describe(c):
    t = c["t"]
    if t == "Image":
        s = c.get("m_Sprite", {}).get("sprite", "-")
        col = c.get("m_Color", {})
        extra = "" if col.get("r", 1) == col.get("g", 1) == col.get("b", 1) == col.get("a", 1) == 1 else " col=%.2f,%.2f,%.2f,%.2f" % (col["r"], col["g"], col["b"], col["a"])
        return "Img[%s t%d%s%s]" % (s, c.get("m_Type", 0), " pa" if c.get("m_PreserveAspect") else "", extra)
    if t == "TextMeshProUGUI":
        col = c.get("m_fontColor", {})
        return "TMP[%r sz%.1f%s col=%.2f,%.2f,%.2f h%d v%d st%d]" % (c.get("m_text", "")[:30], c.get("m_fontSize", 0),
                                                                     " auto%.0f-%.0f" % (c["m_fontSizeMin"], c["m_fontSizeMax"]) if c.get("m_enableAutoSizing") else "",
                                                                     col.get("r", 0), col.get("g", 0), col.get("b", 0),
                                                                     c.get("m_HorizontalAlignment", 0), c.get("m_VerticalAlignment", 0), c.get("m_fontStyle", 0))
    if t == "TextMeshProOutline":
        oc = c.get("outlineColor", {})
        return "Outline[%s w%.2f col=%.2f,%.2f,%.2f]" % (c.get("enableOutline"), c.get("outlineWidth", 0), oc.get("r", 0), oc.get("g", 0), oc.get("b", 0))
    if t in ("Button", "Toggle"):
        return t + ("(off)" if not c.get("m_Enabled", 1) else "")
    known = {"ContentSizeFitter", "HorizontalLayoutGroup", "VerticalLayoutGroup", "GridLayoutGroup", "Mask", "RectMask2D",
             "ScrollRect", "Canvas", "CanvasGroup", "LayoutElement", "GraphicRaycaster"}
    if t in known:
        return t
    return "<" + t + ">"


def main():
    for arg in sys.argv[1:]:
        data = json.load(open(os.path.join(ROOT, arg + ".json"), encoding="utf-8"))
        nodes = data["nodes"]
        depth = {}
        print("=====", data["path"], len(nodes))
        for i, n in enumerate(nodes):
            d = 0 if n["p"] < 0 else depth[n["p"]] + 1
            depth[i] = d
            rt = n.get("rt")
            r = ""
            if rt:
                r = "a(%g,%g)-(%g,%g) pos(%.0f,%.0f) size(%.0f,%.0f) pv(%g,%g)" % tuple(rt[:10])
                if rt[10] != 1 or rt[11] != 1:
                    r += " sc(%g,%g)" % (rt[10], rt[11])
                if rt[13]:
                    r += " rot%.0f" % rt[13]
            comps = " ".join(describe(c) for c in n["c"])
            print("%s%d %s%s %s %s" % ("  " * d, i, n["n"], "" if n["a"] else " (inactive)", r, comps))


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    main()

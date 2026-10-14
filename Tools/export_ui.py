# -*- coding: utf-8 -*-
"""Export original uGUI prefabs, sprites, materials and TMP fonts from the shipped bundles.

Usage: python Tools/export_ui.py <raw bundle folder>
Writes into Assets/Resources/UI and Assets/BufuSource/Fonts of this project.
"""
import hashlib
import json
import os
import shutil
import sys

import UnityPy

PROJECT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
RAW = sys.argv[1] if len(sys.argv) > 1 else os.path.join(os.path.dirname(PROJECT), "bufu-art-all-2026-10-14", "原始资源包")
OUT = os.path.join(PROJECT, "Assets", "Resources", "UI")
FONT_OUT = os.path.join(PROJECT, "Assets", "BufuSource", "Fonts")
SPRITE_ASSET_OUT = os.path.join(PROJECT, "Assets", "BufuSource", "SpriteAssets")

# Ads, auction, ranking, challenge, platform side panels and test tools are not part of the offline rebuild.
INCLUDE = [
    "Assets/GameRes/NewUI/UIbase/",
    "Assets/GameRes/NewUI/Common/prefabs/",
    "Assets/GameRes/NewUI/Popup/prefabs/",
    "Assets/GameRes/NewUI/Facility/prefabs/",
    "Assets/GameRes/NewUI/world/prefabs/",
    "Assets/GameRes/NewUI/CheckIn/prefabs/",
    "Assets/GameRes/NewUI/Shop/prefabs/",
    "Assets/GameRes/NewUI/Talent/prefabs/",
    "Assets/GameRes/NewUI/Evolve/prefabs/",
    "Assets/GameRes/NewUI/loading/prefabs/",
    "Assets/GameRes/NewUI/fly/prefabs/",
    "Assets/GameRes/Role/coinNormalText.prefab",
    "Assets/GameRes/Role/coinCriticalText.prefab",
    "Assets/GameRes/font/gamefont/",
]

DROP = {
    "m_GameObject", "m_Script", "m_Name", "m_OnCullStateChanged", "m_RaycastPadding", "m_Maskable",
    "m_AnimationTriggers", "m_Navigation", "m_havePropertiesChanged", "m_isUsingLegacyAnimationComponent",
    "m_isVolumetricText", "m_hasFontAssetChanged", "m_baseMaterial", "m_maskOffset", "m_renderer",
    "m_subTextObjects", "m_isInputParsingRequired", "m_inputSource", "m_IsTextObjectScaleStatic",
    "m_VertexBufferAutoSizeReduction", "m_geometrySortingOrder", "m_parseCtrlCharacters", "m_isOrthographic",
    "m_isCullingEnabled", "m_horizontalMapping", "m_verticalMapping", "m_uvLineOffset", "m_ignoreCulling",
    "m_enableKerning", "m_enableExtraPadding", "checkPaddingRequired", "m_isRightToLeft", "m_TextStyleHashCode",
    "m_ActiveFontFeatures", "m_fontColor32", "m_EmojiFallbackSupport",
}


def cab_key(name):
    return os.path.basename(name).lower()


def finite(value):
    if isinstance(value, float):
        if value != value:
            return 0.0
        if value in (float("inf"), float("-inf")):
            return 3.0e38 if value > 0 else -3.0e38
        return value
    if isinstance(value, dict):
        return {k: finite(v) for k, v in value.items()}
    if isinstance(value, list):
        return [finite(v) for v in value]
    return value


class Exporter:
    def __init__(self):
        files = [os.path.join(RAW, f) for f in os.listdir(RAW) if f.endswith(".bundle")]
        self.env = UnityPy.load(*files)
        self.index = {}
        for obj in self.env.objects:
            self.index[(cab_key(obj.assets_file.name), obj.path_id)] = obj
        self.trees = {}
        self.prefab_roots = {}
        self.containers = []
        for obj in self.env.objects:
            if obj.type.name != "AssetBundle":
                continue
            bundle = obj.read_typetree()
            for path, info in bundle["m_Container"]:
                asset = info["asset"]
                target = self.resolve_obj(obj, asset)
                if target is None:
                    continue
                if path.endswith(".prefab"):
                    self.prefab_roots[(cab_key(target.assets_file.name), target.path_id)] = path
                    self.containers.append((path, target))
        self.asset_paths = []
        for obj in self.env.objects:
            if obj.type.name != "AssetBundle":
                continue
            for path, info in obj.read_typetree()["m_Container"]:
                target = self.resolve_obj(obj, info["asset"])
                if target is not None and target.type.name in ("Sprite", "Texture2D"):
                    self.asset_paths.append((path, target))
        self.sprites = {}
        self.sprite_keys = {}
        self.textures = {}
        self.materials = {}
        self.fonts = {}
        self.sprite_assets = {}
        self.scripts = {}

    def tree(self, obj):
        key = (cab_key(obj.assets_file.name), obj.path_id)
        if key not in self.trees:
            self.trees[key] = obj.read_typetree()
        return self.trees[key]

    def resolve_obj(self, owner, pptr):
        file_id = pptr.get("m_FileID", 0)
        path_id = pptr.get("m_PathID", 0)
        if path_id == 0:
            return None
        if file_id == 0:
            cab = cab_key(owner.assets_file.name)
        else:
            externals = owner.assets_file.externals
            if file_id - 1 >= len(externals):
                return None
            cab = cab_key(externals[file_id - 1].path)
        return self.index.get((cab, path_id))

    def script_name(self, obj):
        data = self.tree(obj)
        script = self.resolve_obj(obj, data.get("m_Script", {}))
        if script is None:
            return "MonoBehaviour"
        key = (cab_key(script.assets_file.name), script.path_id)
        if key not in self.scripts:
            s = self.tree(script)
            self.scripts[key] = s.get("m_ClassName", "MonoBehaviour")
        return self.scripts[key]

    # ---------------------------------------------------------------- assets
    def export_sprite(self, obj):
        key = (cab_key(obj.assets_file.name), obj.path_id)
        if key in self.sprites:
            return self.sprites[key]["key"]
        sprite = obj.read()
        try:
            image = sprite.image
        except ValueError:
            # The shipped data contains one remote Sprite whose texture PPtr is empty.
            self.sprites[key] = {"key": None}
            return None
        digest = hashlib.md5(image.tobytes()).hexdigest()[:8]
        name = sprite.m_Name.replace("/", "_").replace("\\", "_")
        out_key = name
        known = self.sprite_keys.get(name)
        if known is not None and known != digest:
            out_key = name + "@" + digest
        self.sprite_keys.setdefault(name, digest)
        path = os.path.join(OUT, "Sprites", out_key + ".png")
        if not os.path.exists(path):
            image.save(path)
        border = sprite.m_Border
        rect = sprite.m_Rect
        self.sprites[key] = {
            "key": out_key,
            "w": int(round(rect.width)),
            "h": int(round(rect.height)),
            "border": [border.x, border.y, border.z, border.w],
            "ppu": sprite.m_PixelsToUnits,
            "pivot": [sprite.m_Pivot.x, sprite.m_Pivot.y],
        }
        return out_key

    def export_texture(self, obj):
        key = (cab_key(obj.assets_file.name), obj.path_id)
        if key in self.textures:
            return self.textures[key]
        tex = obj.read()
        name = tex.m_Name.replace("/", "_") or str(obj.path_id)
        out_key = name + "@" + str(abs(obj.path_id))[:8]
        path = os.path.join(OUT, "Textures", out_key + ".png")
        if not os.path.exists(path):
            tex.image.save(path)
        self.textures[key] = out_key
        return out_key

    def export_material(self, obj):
        key = (cab_key(obj.assets_file.name), obj.path_id)
        if key in self.materials:
            return self.materials[key]["key"]
        data = self.tree(obj)
        shader = self.resolve_obj(obj, data.get("m_Shader", {}))
        shader_name = ""
        if shader is not None:
            try:
                shader_name = shader.read().m_ParsedForm.m_Name
            except Exception:
                shader_name = self.tree(shader).get("m_ParsedForm", {}).get("m_Name", "")
        props = data.get("m_SavedProperties", {})
        floats = {k: v for k, v in props.get("m_Floats", [])}
        colors = {k: [v["r"], v["g"], v["b"], v["a"]] for k, v in props.get("m_Colors", [])}
        out_key = data.get("m_Name", "") + "@" + str(abs(obj.path_id))[:8]
        self.materials[key] = {"key": out_key, "name": data.get("m_Name", ""), "shader": shader_name,
                               "floats": floats, "colors": colors,
                               "keywords": data.get("m_ValidKeywords", data.get("m_ShaderKeywords", ""))}
        return out_key

    def export_font(self, obj):
        key = (cab_key(obj.assets_file.name), obj.path_id)
        if key in self.fonts:
            return self.fonts[key]
        data = self.tree(obj)
        name = data["m_Name"]
        folder = os.path.join(FONT_OUT, name)
        os.makedirs(folder, exist_ok=True)
        atlases = []
        for i, pptr in enumerate(data.get("m_AtlasTextures", [])):
            tex = self.resolve_obj(obj, pptr)
            if tex is None:
                continue
            file = "atlas%d.png" % i
            tex.read().image.save(os.path.join(folder, file))
            atlases.append(file)
        material = self.resolve_obj(obj, data.get("material", {}))
        mat_key = self.export_material(material) if material is not None else ""
        fallbacks = []
        for pptr in data.get("m_FallbackFontAssetTable", []):
            fb = self.resolve_obj(obj, pptr)
            if fb is not None:
                fallbacks.append(self.export_font(fb))
        keep = ["m_Version", "m_FaceInfo", "m_GlyphTable", "m_CharacterTable", "m_AtlasWidth", "m_AtlasHeight",
                "m_AtlasPadding", "m_AtlasRenderMode", "m_UsedGlyphRects", "m_FreeGlyphRects", "normalStyle",
                "normalSpacingOffset", "boldStyle", "boldSpacing", "italicStyle", "tabSize", "m_FontFeatureTable"]
        font = {k: data[k] for k in keep if k in data}
        meta = {"name": name, "atlases": atlases, "fallbacks": fallbacks,
                "material": self.materials.get((cab_key(material.assets_file.name), material.path_id)) if material is not None else None}
        with open(os.path.join(folder, "font.json"), "w", encoding="utf-8") as h:
            json.dump(finite(font), h, ensure_ascii=False)
        with open(os.path.join(folder, "meta.json"), "w", encoding="utf-8") as h:
            json.dump(finite(meta), h, ensure_ascii=False, indent=1)
        self.fonts[key] = name
        return name

    def export_sprite_asset(self, obj):
        key = (cab_key(obj.assets_file.name), obj.path_id)
        if key in self.sprite_assets:
            return self.sprite_assets[key]
        data = self.tree(obj)
        name = data["m_Name"]
        folder = os.path.join(SPRITE_ASSET_OUT, name)
        os.makedirs(folder, exist_ok=True)
        sheet = self.resolve_obj(obj, data.get("spriteSheet", {}))
        if sheet is not None:
            sheet.read().image.save(os.path.join(folder, "atlas.png"))
        glyphs = [{k: v for k, v in g.items() if k != "sprite"} for g in data.get("m_SpriteGlyphTable", [])]
        asset = {"m_Version": data.get("m_Version", ""), "m_FaceInfo": data.get("m_FaceInfo", {}),
                 "m_SpriteCharacterTable": data.get("m_SpriteCharacterTable", []), "m_SpriteGlyphTable": glyphs}
        material = self.resolve_obj(obj, data.get("material", {}))
        meta = {"name": name, "material": None}
        if material is not None:
            self.export_material(material)
            meta["material"] = self.materials[(cab_key(material.assets_file.name), material.path_id)]
        with open(os.path.join(folder, "sprite.json"), "w", encoding="utf-8") as h:
            json.dump(finite(asset), h, ensure_ascii=False)
        with open(os.path.join(folder, "meta.json"), "w", encoding="utf-8") as h:
            json.dump(finite(meta), h, ensure_ascii=False, indent=1)
        self.sprite_assets[key] = name
        return name

    # ---------------------------------------------------------------- prefabs
    def export_prefab(self, path, root):
        nodes = []
        go_index = {}
        comp_owner = {}

        def walk(go_obj, parent):
            go = self.tree(go_obj)
            idx = len(nodes)
            go_index[(cab_key(go_obj.assets_file.name), go_obj.path_id)] = idx
            node = {"n": go["m_Name"], "p": parent, "a": bool(go.get("m_IsActive", True)), "c": []}
            nodes.append(node)
            transform = None
            for entry in go["m_Component"]:
                comp = self.resolve_obj(go_obj, entry["component"])
                if comp is None:
                    continue
                comp_owner[(cab_key(comp.assets_file.name), comp.path_id)] = idx
                if comp.type.name in ("RectTransform", "Transform"):
                    transform = comp
            node["_components"] = [self.resolve_obj(go_obj, e["component"]) for e in go["m_Component"]]
            if transform is None:
                return
            t = self.tree(transform)
            node["_transform"] = (transform, t)
            for child in t.get("m_Children", []):
                child_t = self.resolve_obj(transform, child)
                if child_t is None:
                    continue
                child_go = self.resolve_obj(child_t, self.tree(child_t)["m_GameObject"])
                if child_go is not None:
                    walk(child_go, idx)

        walk(root, -1)

        def convert(owner, value):
            if isinstance(value, dict):
                if set(value.keys()) == {"m_FileID", "m_PathID"}:
                    return ref(owner, value)
                out = {}
                for k, v in value.items():
                    if k in DROP:
                        continue
                    c = convert(owner, v)
                    if c is not None:
                        out[k] = c
                return out
            if isinstance(value, list):
                return [convert(owner, v) for v in value]
            return finite(value)

        def ref(owner, pptr):
            target = self.resolve_obj(owner, pptr)
            if target is None:
                if pptr.get("m_PathID", 0) != 0:
                    return {"builtin": pptr["m_PathID"]}
                return None
            key = (cab_key(target.assets_file.name), target.path_id)
            kind = target.type.name
            if kind == "GameObject":
                if key in go_index:
                    return {"node": go_index[key]}
                if key in self.prefab_roots:
                    return {"prefab": self.prefab_roots[key]}
                return {"gameObject": self.tree(target).get("m_Name", "")}
            if key in comp_owner:
                comp_type = kind if kind != "MonoBehaviour" else self.script_name(target)
                return {"node": comp_owner[key], "comp": comp_type}
            if kind == "Sprite":
                return {"sprite": self.export_sprite(target)}
            if kind == "Texture2D":
                return {"texture": self.export_texture(target)}
            if kind == "Material":
                return {"material": self.export_material(target)}
            if kind == "MonoBehaviour":
                cls = self.script_name(target)
                data = self.tree(target)
                if cls == "TMP_FontAsset":
                    return {"font": self.export_font(target)}
                if cls == "TMP_SpriteAsset":
                    return {"spriteAsset": self.export_sprite_asset(target)}
                go_ptr = data.get("m_GameObject", {})
                if go_ptr.get("m_PathID", 0) != 0:
                    owner_go = self.resolve_obj(target, go_ptr)
                    okey = (cab_key(owner_go.assets_file.name), owner_go.path_id) if owner_go else None
                    if okey in self.prefab_roots:
                        return {"prefab": self.prefab_roots[okey], "comp": cls}
                return {"asset": cls, "name": data.get("m_Name", "")}
            if kind in ("Transform", "RectTransform"):
                owner_go = self.resolve_obj(target, self.tree(target)["m_GameObject"])
                okey = (cab_key(owner_go.assets_file.name), owner_go.path_id) if owner_go else None
                if okey in self.prefab_roots:
                    return {"prefab": self.prefab_roots[okey], "comp": kind}
            name = ""
            try:
                name = self.tree(target).get("m_Name", "")
            except Exception:
                pass
            return {"asset": kind, "name": name}

        for node in nodes:
            transform, t = node.pop("_transform", (None, None))
            if t is not None:
                q = t["m_LocalRotation"]
                rot_z = 0.0
                if abs(q["z"]) > 1e-6 or abs(q["x"]) > 1e-6 or abs(q["y"]) > 1e-6:
                    import math
                    rot_z = math.degrees(2 * math.atan2(q["z"], q["w"]))
                s = t["m_LocalScale"]
                p = t["m_LocalPosition"]
                if transform.type.name == "RectTransform":
                    node["rt"] = [t["m_AnchorMin"]["x"], t["m_AnchorMin"]["y"], t["m_AnchorMax"]["x"], t["m_AnchorMax"]["y"],
                                  t["m_AnchoredPosition"]["x"], t["m_AnchoredPosition"]["y"], t["m_SizeDelta"]["x"], t["m_SizeDelta"]["y"],
                                  t["m_Pivot"]["x"], t["m_Pivot"]["y"], s["x"], s["y"], s["z"], rot_z, p["z"]]
                else:
                    node["tr"] = [p["x"], p["y"], p["z"], s["x"], s["y"], s["z"], rot_z]
            for comp in node.pop("_components"):
                if comp is None or comp.type.name in ("RectTransform", "Transform", "CanvasRenderer"):
                    continue
                kind = comp.type.name
                data = self.tree(comp)
                if kind == "MonoBehaviour":
                    kind = self.script_name(comp)
                entry = convert(comp, data)
                entry["t"] = kind
                node["c"].append(entry)
        return {"path": path, "nodes": nodes}

    def run(self):
        for sub in ("Prefabs", "Sprites", "Textures"):
            folder = os.path.join(OUT, sub)
            if os.path.isdir(folder):
                shutil.rmtree(folder)
            os.makedirs(folder)
        for folder in (FONT_OUT, SPRITE_ASSET_OUT):
            if os.path.isdir(folder):
                shutil.rmtree(folder)
            os.makedirs(folder)
        exported = []
        for path, root in sorted(self.containers, key=lambda x: x[0]):
            if not any(path.startswith(p) if p.endswith("/") else path == p for p in INCLUDE):
                continue
            data = self.export_prefab(path, root)
            rel = path.replace("Assets/GameRes/", "")
            parts = rel.split("/")
            module = parts[1] if parts[0] == "NewUI" else parts[0]
            name = os.path.splitext(parts[-1])[0]
            folder = os.path.join(OUT, "Prefabs", module)
            os.makedirs(folder, exist_ok=True)
            with open(os.path.join(folder, name + ".json"), "w", encoding="utf-8") as h:
                json.dump(data, h, ensure_ascii=False, separators=(",", ":"))
            exported.append({"path": path, "resource": "UI/Prefabs/%s/%s" % (module, name), "nodes": len(data["nodes"])})
        # Sprites the original code loads by asset path (honor titles, item and chest icons...).
        paths = {}
        texture_sprites = {}
        for obj in self.env.objects:
            if obj.type.name == "Sprite":
                rd = self.tree(obj).get("m_RD", {}).get("texture", {})
                tex = self.resolve_obj(obj, rd)
                if tex is not None:
                    texture_sprites.setdefault((cab_key(tex.assets_file.name), tex.path_id), obj)
        for path, target in self.asset_paths:
            if target.type.name == "Texture2D":
                target = texture_sprites.get((cab_key(target.assets_file.name), target.path_id))
                if target is None:
                    continue
            sprite_key = self.export_sprite(target)
            if sprite_key:
                paths[path] = sprite_key
        for obj in self.env.objects:
            if obj.type.name == "Sprite":
                self.export_sprite(obj)
        with open(os.path.join(OUT, "sprite_paths.json"), "w", encoding="utf-8") as h:
            json.dump({"paths": [{"path": k, "key": v} for k, v in sorted(paths.items())]}, h, ensure_ascii=False, indent=0)
        with open(os.path.join(OUT, "sprites.json"), "w", encoding="utf-8") as h:
            json.dump({"sprites": sorted((s for s in self.sprites.values() if s["key"]), key=lambda x: x["key"])}, h, ensure_ascii=False, indent=0)
        with open(os.path.join(OUT, "materials.json"), "w", encoding="utf-8") as h:
            json.dump(finite({"materials": sorted(self.materials.values(), key=lambda x: x["key"])}), h, ensure_ascii=False, indent=0)
        with open(os.path.join(OUT, "prefabs.json"), "w", encoding="utf-8") as h:
            json.dump({"prefabs": exported}, h, ensure_ascii=False, indent=1)
        print("prefabs", len(exported), "sprites", len(self.sprites), "textures", len(self.textures),
              "materials", len(self.materials), "fonts", sorted(self.fonts.values()),
              "sprite assets", sorted(self.sprite_assets.values()))


if __name__ == "__main__":
    Exporter().run()

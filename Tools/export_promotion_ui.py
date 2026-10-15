"""Recover promotion UI into an isolated directory, without touching project assets.

Uses the existing format-aware UI exporter but never calls its destructive run().
"""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import sys


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("raw", type=Path)
    parser.add_argument("--deps", type=Path)
    parser.add_argument("--output", type=Path)
    args = parser.parse_args()
    if args.deps:
        sys.path.insert(0, str(args.deps))
    import export_ui
    from PIL import Image

    project = Path(__file__).resolve().parents[1]
    output = (args.output or project / "Build/PromotionEvidence").resolve()
    export_ui.RAW = str(args.raw.resolve())
    export_ui.OUT = str(output / "UI")
    export_ui.FONT_OUT = str(output / "Fonts")
    export_ui.SPRITE_ASSET_OUT = str(output / "SpriteAssets")
    for name in ("UI/Prefabs/Challenge", "UI/Sprites", "UI/Textures", "Fonts", "SpriteAssets"):
        (output / name).mkdir(parents=True, exist_ok=True)

    exporter = export_ui.Exporter()
    # Preserve the original export's first collision winner instead of assigning
    # a different sprite to an existing unsuffixed key in the live project.
    for path in (project / "Assets/Resources/UI/Sprites").glob("*.png"):
        if "@" not in path.stem:
            with Image.open(path) as image:
                exporter.sprite_keys[path.stem] = hashlib.md5(image.tobytes()).hexdigest()[:8]

    wanted = "Assets/GameRes/NewUI/Challenge/prefabs/challengeUI.prefab"
    matches = [(path, root) for path, root in exporter.containers if path.lower() == wanted.lower()]
    if len(matches) != 1:
        raise RuntimeError(f"Expected one original challengeUI prefab, found {len(matches)}")
    path, root = matches[0]
    data = exporter.export_prefab(path, root)
    (output / "UI/Prefabs/Challenge/challengeUI.json").write_text(
        json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8")

    metadata = {
        "original_path": path,
        "asset_file": root.assets_file.name,
        "path_id": root.path_id,
        "raw_sha256": hashlib.sha256(root.get_raw_data()).hexdigest(),
        "nodes": len(data["nodes"]),
        "animations": [],
        "sprite_key_conflicts": [],
    }
    catalog = json.loads((project / "Reference/package-manifest.json").read_text(encoding="utf-8"))
    catalog_asset = next(row for row in catalog["assets"] if row["AssetPath"].lower() == wanted.lower())
    source_bundle = args.raw / catalog_asset["bundle_file"]
    metadata["source_bundle"] = str(source_bundle)
    metadata["source_bundle_sha256"] = hashlib.sha256(source_bundle.read_bytes()).hexdigest()
    # Retain any animation/controller tracks from this prefab's serialized file;
    # absence is evidence of this bounded file, not a claim about the whole game.
    for obj in exporter.env.objects:
        if obj.assets_file.name != root.assets_file.name:
            continue
        if obj.type.name in ("AnimationClip", "AnimatorController", "AnimatorOverrideController"):
            tree = exporter.tree(obj)
            metadata["animations"].append({
                "type": obj.type.name, "path_id": obj.path_id, "tree": tree,
            })

    sprites = sorted((row for row in exporter.sprites.values() if row.get("key")), key=lambda row: row["key"])
    for row in sprites:
        generated = output / "UI/Sprites" / (row["key"] + ".png")
        existing = project / "Assets/Resources/UI/Sprites" / (row["key"] + ".png")
        if existing.exists():
            with Image.open(existing) as before, Image.open(generated) as after:
                if before.size != after.size or before.convert("RGBA").tobytes() != after.convert("RGBA").tobytes():
                    metadata["sprite_key_conflicts"].append(row["key"])
    for name, value in (
        ("sprites", {"sprites": sprites}),
        ("materials", {"materials": list(exporter.materials.values())}),
        ("prefabs", {"prefabs": [{"path": path, "resource": "UI/Prefabs/Challenge/challengeUI", "nodes": len(data["nodes"])}]}),
    ):
        (output / "UI" / (name + ".json")).write_text(
            json.dumps(export_ui.finite(value), ensure_ascii=False, indent=2), encoding="utf-8")

    spine = project / "Reference/Spine/Uieff_ty_btn_challenge_3ef2b7d6"
    target = output / "Spine/Uieff_ty_btn_challenge"
    target.mkdir(parents=True, exist_ok=True)
    for suffix in ("json", "atlas", "png"):
        shutil.copy2(spine / ("Uieff_ty_btn_challenge." + suffix), target)
    for source in (spine / "editor/images").glob("*.png"):
        (target / "images").mkdir(exist_ok=True)
        shutil.copy2(source, target / "images")
    (output / "evidence.json").write_text(json.dumps(export_ui.finite(metadata), ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps({"output": str(output), "nodes": len(data["nodes"]), "sprites": len(sprites),
                      "animation_assets_in_prefab_file": len(metadata["animations"]),
                      "sprite_key_conflicts": metadata["sprite_key_conflicts"]}, ensure_ascii=False))


if __name__ == "__main__":
    main()

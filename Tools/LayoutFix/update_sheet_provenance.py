from pathlib import Path
import json
root=Path(__file__).resolve().parents[2]
entries=json.loads((root/'Assets/Resources/PackedUI/index.json').read_text(encoding='utf-8'))['entries']
lookup={e['key']:e for e in entries}
folder=root/'Documentation/LayoutFix-2026-10-20'
for filename in ['native-art-manifest.json','word-art-expansion-manifest.json']:
    path=folder/filename;data=json.loads(path.read_text(encoding='utf-8'))
    for item in data['assets']:
        key=item.get('sprite_resource_key') or item['asset'].replace('\\','/').removeprefix('Assets/Resources/').removesuffix('.png')
        if key in lookup:
            e=lookup[key];item['asset']='Assets/SourceArt/SpriteSheetSources/'+key+'.png'
            item['sprite_resource_key']=key;item['packed_asset']='Assets/Resources/PackedUI/'+e['sheet']+'.png';item['packed_rect']=e['rect']
        assert (root/item['asset']).exists(),item['asset']
    data['packing_record']='Documentation/SpriteSheetAudit-2026-10-20/migration-manifest.json'
    path.write_text(json.dumps(data,ensure_ascii=False,indent=2),encoding='utf-8')
print('GENERATED_ART_PROVENANCE_UPDATED')

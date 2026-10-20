"""Read selected original prefab parameters without exporting art or executing game code."""
import concurrent.futures
import hashlib
import json
import re
import sys
import urllib.request
from pathlib import Path
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[1]
manifest=json.loads((ROOT/'Reference/package-manifest.json').read_text(encoding='utf-8'))
constants=(ROOT/'Reference/DecompiledCSharp/Constants.cs').read_text(encoding='utf-8')
method=constants.split('public static string GetRolePrefabPath(int roleType)',1)[1].split('public static int GetHelperSkillLevel',1)[0]
roles={p:int(i) for i,p in re.findall(r'(\d+)\s*=>\s*"([^"]+\.prefab)"',method)}
challenge=json.loads((ROOT/'Assets/Resources/UI/Prefabs/Challenge/ChallengeMain.json').read_text(encoding='utf-8'))
script=next(c for n in challenge['nodes'] for c in n.get('c',[]) if c.get('t')=='ChallengeMain')
coins={script[k]['prefab']:k for k in ('coinCopperPrefab','coinSilverPrefab','coinGoldPrefab','coinPurplePrefab')}
wanted={**{p:{'role_type':v} for p,v in roles.items()},**{p:{'challenge_field':v} for p,v in coins.items()}}
assets=[a for a in manifest['assets'] if a['AssetPath'] in wanted]
assert len(assets)==len(wanted),(len(assets),len(wanted))
bundles={b['FileName']:b for b in manifest['bundles']};raw=HERE/'raw';raw.mkdir(exist_ok=True)
# Serialized MonoScripts are already in the shared dependency cache. Texture/Spine dependencies
# are deliberately not downloaded: reading component type trees does not require their pixels.
files={a['bundle_file'] for a in assets}
def fetch(name):
 b=bundles[name];dest=raw/name;cached=dest.exists()
 data=dest.read_bytes() if cached else urllib.request.urlopen(b['url'],timeout=60).read()
 if len(data)!=b['FileSize']:raise ValueError('Size mismatch: '+name)
 if not cached:dest.write_bytes(data)
 return {'file':name,'url':b['url'],'bytes':len(data),'sha256':hashlib.sha256(data).hexdigest(),'cached':cached}
with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:downloads=list(pool.map(fetch,sorted(files)))
(HERE/'actor-parameter-downloads.json').write_text(json.dumps({'assets':assets,'downloads':downloads},ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({'bundles':len(downloads),'new_bytes':sum(r['bytes'] for r in downloads if not r['cached'])}),flush=True)
if '--download-only' in sys.argv:raise SystemExit(0)
sys.path.insert(0,str(HERE/'deps'));sys.path.insert(0,str(ROOT/'Tools'))
import export_ui
export_ui.RAW=str(raw);exporter=export_ui.Exporter()
def small(v):
 if v is None or isinstance(v,(str,int,float,bool)):return v
 if isinstance(v,list) and len(v)<=12:
  values=[small(x) for x in v]
  return None if any(x is None and original is not None for x,original in zip(values,v)) else values
 if isinstance(v,dict) and 'm_PathID' not in v and all(isinstance(x,(str,int,float,bool)) for x in v.values()):return v
 return None
records=[]
for path,identity in wanted.items():
 matches=[(p,o) for p,o in exporter.containers if p.lower()==path.lower()]
 if len(matches)!=1:raise RuntimeError(f'{path}: {len(matches)} matches')
 actual,obj=matches[0];record={'prefab':actual,**identity,'root_path_id':obj.path_id,'root_sha256':hashlib.sha256(obj.get_raw_data()).hexdigest(),'components':[]}
 seen=set()
 def walk(go,parent=''):
  if go.path_id in seen:return
  seen.add(go.path_id);tree=exporter.tree(go);node=parent+'/'+tree.get('m_Name','')
  transform=None
  for pp in tree.get('m_Component',[]):
   comp=exporter.resolve_obj(go,pp.get('component',pp))
   if comp is None:continue
   ct=exporter.tree(comp)
   if comp.type.name in ('Transform','RectTransform'):
    transform=comp
    record['components'].append({'node':node,'class':comp.type.name,'path_id':comp.path_id,'fields':{k:v for k,v in ct.items() if k in ('m_LocalScale','m_LocalPosition','m_LocalRotation','m_SizeDelta')}})
   elif comp.type.name=='MonoBehaviour':
    fields={k:small(v) for k,v in ct.items() if not k.startswith('m_') and small(v) is not None}
    cls=exporter.script_name(comp)
    if fields:record['components'].append({'node':node,'class':cls,'path_id':comp.path_id,'raw_sha256':hashlib.sha256(comp.get_raw_data()).hexdigest(),'fields':fields})
  if transform:
   for pp in exporter.tree(transform).get('m_Children',[]):
    child=exporter.resolve_obj(transform,pp)
    if child is not None:
     childgo=exporter.resolve_obj(child,exporter.tree(child).get('m_GameObject',{}))
     if childgo is not None:walk(childgo,node)
 walk(obj);records.append(record)
 print(json.dumps({'prefab':path,'components':len(record['components'])}),flush=True)
(HERE/'prefab-actor-parameters.json').write_text(json.dumps(export_ui.finite({'sources':{'role_path_method':'Reference/DecompiledCSharp/Constants.cs:325-341','challenge_binding':'Assets/GameRes/NewUI/Challenge/prefabs/ChallengeMain.prefab'},'prefabs':records}),ensure_ascii=False,indent=2),encoding='utf-8')

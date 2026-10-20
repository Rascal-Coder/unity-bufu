using System;
using System.IO;
using System.Linq;
using Bufu.Rebuild.UI;
using UnityEngine;

namespace Bufu.Rebuild {
    public sealed partial class RuntimeVerification {
        [Serializable] sealed class SheetRuntimeResult {public int sprites,pages,looseTextures;public long textureBytes;public string scope;}
        void VerifyPackedSprites() {
            var entries=PackedSpriteSheets.Entries.ToArray();
            Check(entries.Length>=1200,"sheets: the complete migrated UI catalog is available in the built player");
            var textures=new System.Collections.Generic.HashSet<Texture>();int loose=0;
            foreach(var e in entries) {
                Check(PackedSpriteSheets.TryGet(e.key,out var sprite)&&sprite&&sprite.name==e.name,"sheets: named sprite resolves: "+e.key);
                var r=e.rect;var b=e.border;var p=e.pivot;
                Check(sprite.rect==new Rect(r[0],r[1],r[2],r[3])&&sprite.border==new Vector4(b[0],b[1],b[2],b[3])&&Vector2.Distance(sprite.pivot,new Vector2(p[0]*r[2],p[1]*r[3]))<.01f&&Mathf.Abs(sprite.pixelsPerUnit-e.ppu)<.01f,"sheets: logical size, pivot, pixels-per-unit and nine-slice borders survive packing: "+e.name);
                Check(sprite.textureRect.xMin>=4&&sprite.textureRect.yMin>=4&&sprite.textureRect.xMax<=sprite.texture.width-4&&sprite.textureRect.yMax<=sprite.texture.height-4,"sheets: sprite has a guard region inside the shared page: "+e.name);
                if(Resources.Load<Texture2D>(e.key))loose++;
                textures.Add(sprite.texture);
            }
            Check(loose==0,"sheets: migrated source PNG textures are absent from Resources in the delivered player");
            Check(textures.Count<entries.Length/10&&textures.All(t=>((Texture2D)t).format==TextureFormat.BC7),"sheets: UI shares compressed BC7 textures on the Windows target");
            Check(Resources.Load<Texture2D>("Art/UI/fight_bg_1")==null,"sheets: redundant Art/UI texture copies are absent from the player");
            var result=new SheetRuntimeResult{sprites=entries.Length,pages=textures.Count,looseTextures=loose,textureBytes=textures.Sum(t=>UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(t)),scope="QA loads every migrated sprite for validation; these bytes are not a normal gameplay peak. Pages load on demand during normal play."};
            File.WriteAllText(Path.Combine(output,"sprite-sheet-runtime.json"),JsonUtility.ToJson(result,true));
        }
    }
}

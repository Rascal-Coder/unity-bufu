using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bufu.Rebuild.UI {
    /// <summary>Named sub-sprites on shared, offline-padded sheets. Pages load on demand.</summary>
    public static class PackedSpriteSheets {
        [Serializable] public sealed class Entry {
            public string key,name,sheet;public float[] rect,pivot,border;public float ppu;
        }
        [Serializable] sealed class Index {public int version;public Entry[] entries;}
        static Dictionary<string,Entry> entries;
        static readonly Dictionary<string,Texture2D> pages=new Dictionary<string,Texture2D>();
        static readonly Dictionary<string,Sprite> sprites=new Dictionary<string,Sprite>();
        static void EnsureIndex() {
            if(entries!=null)return;entries=new Dictionary<string,Entry>();
            var asset=Resources.Load<TextAsset>("PackedUI/index");if(!asset)return;
            var index=JsonUtility.FromJson<Index>(asset.text);
            if(index.version!=1)throw new InvalidOperationException("Unsupported packed UI index");
            foreach(var item in index.entries)entries.Add(item.key,item);
        }
        public static bool Contains(string key){EnsureIndex();return entries.ContainsKey(key);}
        public static IEnumerable<Entry> Entries {get {EnsureIndex();return entries.Values;}}
        public static int LoadedPageCount=>pages.Count;
        public static bool TryGet(string key,out Sprite sprite) {
            EnsureIndex();if(sprites.TryGetValue(key,out sprite))return true;
            if(!entries.TryGetValue(key,out var entry))return false;
            if(!pages.TryGetValue(entry.sheet,out var texture)) {
                texture=Resources.Load<Texture2D>("PackedUI/"+entry.sheet);
                if(!texture)throw new InvalidOperationException("Missing UI sheet: "+entry.sheet);
                pages.Add(entry.sheet,texture);
            }
            var r=entry.rect;var b=entry.border;var p=entry.pivot;
            sprite=Sprite.Create(texture,new Rect(r[0],r[1],r[2],r[3]),new Vector2(p[0],p[1]),entry.ppu,0,SpriteMeshType.FullRect,new Vector4(b[0],b[1],b[2],b[3]));
            sprite.name=entry.name;sprites.Add(key,sprite);return true;
        }
    }
}

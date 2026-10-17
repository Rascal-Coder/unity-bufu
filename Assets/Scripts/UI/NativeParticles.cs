using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Coffee.UIExtensions;
using UnityEngine;

namespace Bufu.Rebuild.UI {
    using Data=Dictionary<string,object>;

    /// <summary>Original serialized Unity ParticleSystems, rendered by upstream Coffee UIParticle.</summary>
    public static class NativeParticles {
        public static readonly string[] Sources={"Popup/goalReachPanel","Role/Ads_coinrain","Role/Ads_jiayou","Role/Ads_zuanqian"};

        public static string ResourcePath(string resource,int node) {
            const string prefix="UI/Prefabs/";
            if(resource.StartsWith(prefix,StringComparison.Ordinal))resource=resource.Substring(prefix.Length);
            return "UI/NativeParticles/"+resource.Replace('/','_').Replace('\\','_')+"_"+node;
        }

        /// <summary>Create the actual UiView node, before applying its original transform/parent/active state.</summary>
        public static void RestorePresentation(GameObject go,Data particle,Data renderer) {
            var system=go.GetComponent<ParticleSystem>();
            var sheetModule=particle.Obj("UVModule");
            // Only an authored sprite sheet may replace the module. The cash flipbook is grid
            // data plus one unused sprite; that sprite is a blank quad, so it is not applied here.
            if(system&&sheetModule!=null&&sheetModule.I("mode")==1) {
                var sprites=(sheetModule.Arr("sprites")??new List<object>()).OfType<Data>()
                    .Select(entry=>UiAssets.Sprite(entry.Obj("sprite")?.Str("sprite"))).Where(sprite=>sprite).ToArray();
                if(sprites.Length>0) {
                    var sheet=system.textureSheetAnimation;sheet.mode=ParticleSystemAnimationMode.Sprites;
                    while(sheet.spriteCount>0)sheet.RemoveSprite(0);
                    foreach(var sprite in sprites)sheet.AddSprite(sprite);
                }
            }
            var particleRenderer=go.GetComponent<ParticleSystemRenderer>();
            var materials=renderer?.Arr("m_Materials");
            if(particleRenderer&&materials!=null&&materials.Count>0&&materials[0] is Data material) {
                var loaded=Resources.Load<Material>("UI/NativeParticles/Materials/"+material.Str("material"));
                if(loaded) {
                    bool cash=material.Str("material")=="cash@73694825";
                    var atlas=cash?CashAtlas():null;
                    if(cash&&atlas&&loaded.mainTexture!=atlas){loaded=new Material(loaded);loaded.mainTexture=atlas;}
                    particleRenderer.sharedMaterial=loaded;
                    // Coffee only copies sprite-sheet UVs. A grid atlas otherwise bakes as one tinted quad.
                    if(cash&&system&&sheetModule!=null&&sheetModule.I("mode")==0&&sheetModule.I("tilesX")==3&&sheetModule.I("tilesY")==3)ApplyCashFrames(system);
                }
            }
        }

        static Texture2D cashAtlas;
        static Sprite[] cashFrames;
        static Texture2D CashAtlas() {
            if(!cashAtlas)cashAtlas=Resources.Load<Texture2D>("UI/Textures/zgygy_cash@88101757");
            return cashAtlas;
        }
        static void ApplyCashFrames(ParticleSystem system) {
            var tex=CashAtlas();if(!tex||tex.width<3||tex.height<3)return;
            if(cashFrames==null) {
                int cols=3,rows=3,w=tex.width/cols,h=tex.height/rows;cashFrames=new Sprite[cols*rows];
                for(int i=0;i<cashFrames.Length;i++) {
                    int col=i%cols,row=i/cols;
                    cashFrames[i]=Sprite.Create(tex,new Rect(col*w,row*h,w,h),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
                    cashFrames[i].name="cash_"+i;
                }
            }
            var sheet=system.textureSheetAnimation;sheet.mode=ParticleSystemAnimationMode.Sprites;
            while(sheet.spriteCount>0)sheet.RemoveSprite(0);
            foreach(var sprite in cashFrames)sheet.AddSprite(sprite);
        }

        public static GameObject Create(string resource,int node,Transform parent) {
            var path=ResourcePath(resource,node);
            var prefab=Resources.Load<GameObject>(path);
            if(!prefab)throw new MissingReferenceException("Native particle asset missing: "+path+". Run NativeParticleBuilder.Build().");
            var go=UnityEngine.Object.Instantiate(prefab,parent,false);
            go.SetActive(false);
            return go;
        }

        /// <summary>Bind only after all original nodes and ParticleSystems exist. Keeps the original particle order/settings.</summary>
        public static UIParticle Bind(UiView view,int node,Data serialized) {
            return Bind(view.Nodes[node],view.Nodes,serialized);
        }

        public static UIParticle Bind(GameObject root,GameObject[] nodes,Data serialized) {
            if(!root.GetComponent<CanvasRenderer>())root.AddComponent<CanvasRenderer>();
            var particle=root.GetComponent<UIParticle>();
            if(!particle)particle=root.AddComponent<UIParticle>();
            var fields=new Data(serialized);fields.Remove("t");fields.Remove("m_Particles");
            JsonUtility.FromJsonOverwrite(NativeParticleJson.Stringify(fields),particle);
            var systems=new List<ParticleSystem>();
            var references=serialized.Arr("m_Particles");
            if(references!=null)foreach(Data reference in references) {
                if(reference==null){systems.Add(null);continue;}
                int index=reference.I("node",-1);
                if(index<0||index>=nodes.Length)throw new InvalidOperationException("Invalid original UIParticle node reference "+index);
                var system=nodes[index].GetComponent<ParticleSystem>();
                if(!system)throw new MissingComponentException("Original UIParticle particle node "+index+" has no native ParticleSystem.");
                // These are stationary UI emitters. Canvas/scaler compensation moves
                // their world transforms while Coffee simulates and bakes them; that
                // movement must not generate thousands of distance-based particles.
                // The original time-based rates, bursts, lifetime and atlas remain intact.
                var emission=system.emission;emission.rateOverDistanceMultiplier=0;
                var main=system.main;
                var size=main.startSize;
                bool pair=size.mode==ParticleSystemCurveMode.TwoConstants;
                float max=pair?Mathf.Max(size.constant,size.constantMin):size.constantMax;
                float min=pair?Mathf.Min(size.constant,size.constantMin):size.constant;
                // UIParticle.scale is 10. TwoConstants stores the range in constant/constantMin;
                // constantMax reads the unused curve and skipped this shrink, so bills filled the screen.
                if(max>20f)main.startSize=new ParticleSystem.MinMaxCurve(min*.2f,max*.2f);
                systems.Add(system);
            }
            // Coffee's documented RefreshParticles API preserves hierarchy, curves, subemitters and native simulation.
            particle.particles.Clear();particle.particles.AddRange(systems);
            particle.RefreshParticles(particle.particles);
            return particle;
        }

        public static void Play(UiView view,int node) {
            var ps=view.Nodes[node].GetComponent<ParticleSystem>();
            if(!ps)throw new MissingComponentException("Native particle node "+node+" missing");
            ps.gameObject.SetActive(true);ps.Play(true);
        }
    }

    /// <summary>Small serializer shared by the native asset builder and Coffee configuration bridge.</summary>
    public static class NativeParticleJson {
        public static string Stringify(object value){var builder=new StringBuilder();Write(builder,value);return builder.ToString();}
        static void Write(StringBuilder builder,object value) {
            if(value==null){builder.Append("null");return;}
            if(value is string text){String(builder,text);return;}
            if(value is bool boolean){builder.Append(boolean?"true":"false");return;}
            if(value is IDictionary<string,object> data) {
                builder.Append('{');bool first=true;
                foreach(var pair in data){if(!first)builder.Append(',');first=false;String(builder,pair.Key);builder.Append(':');Write(builder,pair.Value);}
                builder.Append('}');return;
            }
            if(value is System.Collections.IEnumerable list) {
                builder.Append('[');bool first=true;
                foreach(var item in list){if(!first)builder.Append(',');first=false;Write(builder,item);}
                builder.Append(']');return;
            }
            builder.Append(Convert.ToString(value,CultureInfo.InvariantCulture));
        }
        static void String(StringBuilder builder,string value) {
            builder.Append('"');
            foreach(char c in value)switch(c) {
                case '"':builder.Append("\\\"");break;
                case '\\':builder.Append("\\\\");break;
                case '\n':builder.Append("\\n");break;
                case '\r':builder.Append("\\r");break;
                case '\t':builder.Append("\\t");break;
                default:if(c<32)builder.Append("\\u").Append(((int)c).ToString("x4"));else builder.Append(c);break;
            }
            builder.Append('"');
        }
    }
}

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

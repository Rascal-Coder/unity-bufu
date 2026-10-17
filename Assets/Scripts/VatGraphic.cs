using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace Bufu.Rebuild {
    [Serializable] public sealed class VatSegment {
        public string animation,texture; public float duration; public int totalFrames,start,frames,slice,vertices;
        public float[] uv,colors; public int[] triangles;
    }
    [Serializable] public sealed class VatData {
        public string name; public int width,height,depth; public Vector3 min,max; public bool rgbNeedsAlpha;
        public VatSegment[] segments; [NonSerialized] public float[] positions;
    }
    // Same normalized float32 position/alpha arrays as the shipped VAT shader.
    // Reconstruction uses CPU sampling into uGUI so it needs no platform bundle or Spine SDK.
    public sealed class VatGraphic:MaskableGraphic {
        static readonly Dictionary<string,VatData> Cache=new Dictionary<string,VatData>();
        static readonly Dictionary<string,Texture2D> Textures=new Dictionary<string,Texture2D>();
        public VatData Data {get;private set;} public string AnimationName {get;private set;}
        VatSegment segment; Texture2D texture; float elapsed,localFrame; bool loop,playing;
        public float PixelsPerUnit=95; public Action Finished;
        public override Texture mainTexture=>texture?texture:Texture2D.whiteTexture;
        // Some editor play-mode restores skip Graphic's [RequireComponent(CanvasRenderer)] for project scripts.
        public static VatGraphic AddTo(GameObject go){if(!go.GetComponent<CanvasRenderer>())go.AddComponent<CanvasRenderer>();return go.AddComponent<VatGraphic>();}
        public void Load(string name) {
            if(!Cache.TryGetValue(name,out var data)) {
                var asset=Resources.Load<TextAsset>("Art/Vat/"+name);if(!asset)throw new InvalidOperationException("Missing VAT "+name);
                data=JsonUtility.FromJson<VatData>(asset.text);var raw=Resources.Load<TextAsset>("Art/Vat/"+name+"_positions").bytes;
                data.positions=new float[raw.Length/4];Buffer.BlockCopy(raw,0,data.positions,0,raw.Length);Cache[name]=data;
            }
            Data=data;Play(data.segments[0].animation,true);
        }
        public bool Has(string animation)=>Data!=null&&Data.segments.Any(x=>x.animation==animation);
        public void Play(string name,bool repeat,Action done=null) {
            if(Data==null)return;
            if(!Has(name))name=Data.segments[0].animation;
            AnimationName=name;elapsed=0;loop=repeat;playing=true;Finished=done;Sample(0);
        }
        public void Pose(string name,float fraction=0) {Play(name,false);playing=false;Sample(Mathf.Clamp01(fraction));}
        void Update() {
            if(!playing||Data==null)return;
            var first=Data.segments.First(x=>x.animation==AnimationName);elapsed+=Time.deltaTime;
            float t=elapsed/Mathf.Max(.001f,first.duration);Sample(loop?Mathf.Repeat(t,1):Mathf.Clamp01(t));
            if(!loop&&t>=1){playing=false;var done=Finished;Finished=null;done?.Invoke();}
        }
        public void Sample(float normalized) {
            var candidates=Data.segments.Where(x=>x.animation==AnimationName).OrderBy(x=>x.start).ToArray();
            float global=Mathf.Clamp01(normalized)*Mathf.Max(0,candidates[0].totalFrames-1);
            segment=candidates[0];foreach(var s in candidates)if(s.start<=Mathf.FloorToInt(global))segment=s;
            localFrame=Mathf.Clamp(global-segment.start,0,segment.frames-1);
            if(!Textures.TryGetValue(segment.texture,out texture)){texture=Resources.Load<Texture2D>("Art/Textures/"+segment.texture);Textures[segment.texture]=texture;}
            SetVerticesDirty();SetMaterialDirty();
        }
        protected override void OnPopulateMesh(VertexHelper vh) {
            vh.Clear();if(Data==null||segment==null)return;
            int frame=Mathf.FloorToInt(localFrame),next=Mathf.Min(frame+1,segment.frames-1);float blend=localFrame-frame;
            Vector3 size=Data.max-Data.min;
            float AlphaAt(int index) {
                int a=((segment.slice*Data.height+index)*Data.width+frame)*4;
                int b=((segment.slice*Data.height+index)*Data.width+next)*4;
                return Mathf.Lerp(Data.positions[a+3],Data.positions[b+3],blend);
            }
            for(int i=0;i<segment.vertices;i++) {
                int a=((segment.slice*Data.height+i)*Data.width+frame)*4;
                int b=((segment.slice*Data.height+i)*Data.width+next)*4;
                var p=Data.positions;var xyz=new Vector3(Mathf.Lerp(p[a],p[b],blend),Mathf.Lerp(p[a+1],p[b+1],blend),Mathf.Lerp(p[a+2],p[b+2],blend));
                xyz=Data.min+Vector3.Scale(xyz,size);float alpha=Mathf.Lerp(p[a+3],p[b+3],blend);
                var c=new Color(segment.colors[i*4],segment.colors[i*4+1],segment.colors[i*4+2],alpha)*color;
                // Unity UI uses straight alpha. Shipped vertex RGB is premultiplied by VAT alpha.
                if(!Data.rgbNeedsAlpha&&alpha>.00001f){c.r/=alpha;c.g/=alpha;c.b/=alpha;}
                vh.AddVert(new Vector3(xyz.x*PixelsPerUnit,xyz.y*PixelsPerUnit,0),c,new Vector2(segment.uv[i*2],segment.uv[i*2+1]));
            }
            for(int i=0;i<segment.triangles.Length;i+=3) {
                int t0=segment.triangles[i],t1=segment.triangles[i+1],t2=segment.triangles[i+2];
                if(AlphaAt(t0)<.02f&&AlphaAt(t1)<.02f&&AlphaAt(t2)<.02f)continue;
                vh.AddTriangle(t0,t1,t2);
            }
        }
    }
}

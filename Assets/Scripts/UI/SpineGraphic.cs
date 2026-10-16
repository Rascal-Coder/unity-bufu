using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild.UI {
    /// <summary>
    /// Spine 3.8 region and weighted-mesh player for shop lights and the original
    /// rebirth cutscene. One canvas unit is one spine pixel.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SpineGraphic:MaskableGraphic {
        sealed class Bone {
            public int parent=-1; public float setupRot,setupX,setupY,setupSx=1,setupSy=1;
            public float rot,x,y,sx=1,sy=1,a=1,b,c,d=1,wx,wy;
        }
        sealed class Region { public Vector2[] corner,uv; public int[] triangles,weightedBones;public float[] weights; }
        sealed class Track {
            public float[] time,x,y,cx1,cy1,cx2,cy2; public bool[] step,bezier; public string[] attachment; public Color[] color;
            public bool scale; public int Count=>time==null?0:time.Length;
        }
        sealed class Anim {
            public float duration;
            public readonly Dictionary<int,Track> rotate=new Dictionary<int,Track>(),translate=new Dictionary<int,Track>(),scale=new Dictionary<int,Track>();
            public readonly Dictionary<int,Track> slotColor=new Dictionary<int,Track>(),slotAttach=new Dictionary<int,Track>();
        }
        sealed class Clip {
            public Texture2D texture; public Bone[] bones; public string[] slotName,setupAttach; public int[] boneOf;public Color[] setupColor;
            public Dictionary<string,Region> regions; public Dictionary<string,Anim> anims;
        }

        static readonly Dictionary<string,Clip> cache=new Dictionary<string,Clip>();
        Clip clip; Anim anim; float elapsed; bool loop,playing;
        public Action Finished;
        public bool UseScaledTime;
        public float AnimationDuration=>anim==null?0:anim.duration;
        public override Texture mainTexture {
            get { if(clip!=null&&clip.texture)return clip.texture; if(!s_WhiteTexture)s_WhiteTexture=Texture2D.whiteTexture; return s_WhiteTexture; }
        }

        public bool Load(string name) {
            if(!cache.TryGetValue(name,out clip)) {
                var json=Resources.Load<TextAsset>("Spine/"+name+"/"+name);
                var atlas=Resources.Load<TextAsset>("Spine/"+name+"/atlas");
                var tex=Resources.Load<Texture2D>("Spine/"+name+"/"+name);
                if(!json||!atlas||!tex){clip=null;return false;}
                clip=Build(json.text,atlas.text,tex); cache[name]=clip;
            }
            raycastTarget=false; return clip!=null;
        }

        public void Play(string name,bool repeat,Action done=null) {
            if(clip==null||!clip.anims.TryGetValue(name,out anim)){playing=false;return;}
            elapsed=0; loop=repeat; playing=true; Finished=done; Apply(0); SetVerticesDirty();
        }

        void Update() {
            if(!playing||anim==null)return;
            elapsed+=UseScaledTime?Time.deltaTime:Time.unscaledDeltaTime; float dur=Mathf.Max(.0001f,anim.duration);
            if(elapsed>=dur) {
                if(loop){elapsed%=dur;Finished?.Invoke();}
                else{elapsed=dur;playing=false;var done=Finished;Finished=null;done?.Invoke();}
            }
            Apply(Mathf.Min(elapsed,dur)); SetVerticesDirty();
        }

        void Apply(float time) {
            var bones=clip.bones;
            for(int i=0;i<bones.Length;i++){var b=bones[i];b.rot=b.setupRot;b.x=b.setupX;b.y=b.setupY;b.sx=b.setupSx;b.sy=b.setupSy;}
            foreach(var pair in anim.rotate)Nudge(bones[pair.Key],pair.Value,time,0);
            foreach(var pair in anim.translate)Nudge(bones[pair.Key],pair.Value,time,1);
            foreach(var pair in anim.scale)Nudge(bones[pair.Key],pair.Value,time,2);
            for(int i=0;i<bones.Length;i++)World(bones[i],bones[i].parent<0?null:bones[bones[i].parent]);
        }

        static void Nudge(Bone bone,Track track,float time,int mode) {
            Sample(track,time,out float x,out float y);
            if(mode==0)bone.rot+=x;
            else if(mode==1){bone.x+=x;bone.y+=y;}
            else{bone.sx*=x;bone.sy*=y;}
        }

        static void World(Bone bone,Bone parent) {
            float rad=bone.rot*Mathf.Deg2Rad, cos=Mathf.Cos(rad), sin=Mathf.Sin(rad);
            float la=cos*bone.sx, lb=-sin*bone.sy, lc=sin*bone.sx, ld=cos*bone.sy;
            if(parent==null){bone.a=la;bone.b=lb;bone.c=lc;bone.d=ld;bone.wx=bone.x;bone.wy=bone.y;return;}
            bone.wx=parent.a*bone.x+parent.b*bone.y+parent.wx; bone.wy=parent.c*bone.x+parent.d*bone.y+parent.wy;
            bone.a=parent.a*la+parent.b*lc; bone.b=parent.a*lb+parent.b*ld;
            bone.c=parent.c*la+parent.d*lc; bone.d=parent.c*lb+parent.d*ld;
        }

        protected override void OnPopulateMesh(VertexHelper vh) {
            vh.Clear(); if(clip==null||anim==null)return;
            float time=anim==null?0:Mathf.Min(elapsed,anim.duration);
            for(int s=0;s<clip.setupAttach.Length;s++) {
                string attachment=clip.setupAttach[s]; var tintSlot=clip.setupColor[s];
                if(anim.slotAttach.TryGetValue(s,out var at)){if(!SampleAttach(at,time,ref attachment))continue;}
                if(string.IsNullOrEmpty(attachment))continue;
                if(anim.slotColor.TryGetValue(s,out var ct))tintSlot=SampleColor(ct,time);
                if(tintSlot.a<=.004f||!clip.regions.TryGetValue(clip.slotName[s]+"/"+attachment,out var region))continue;
                var bone=clip.bones[clip.boneOf[s]]; var c=tintSlot*color; if(c.a<=.004f)continue;
                int v=vh.currentVertCount;
                for(int i=0;i<region.uv.Length;i++) {
                    if(region.weightedBones!=null) {
                        int boneAt=0,weightAt=0;Vector2 point=Vector2.zero;
                        for(int vertex=0;vertex<=i;vertex++) {
                            int count=region.weightedBones[boneAt++];point=Vector2.zero;
                            for(int weight=0;weight<count;weight++) {
                                var b=clip.bones[region.weightedBones[boneAt++]];float x=region.weights[weightAt++],y=region.weights[weightAt++],w=region.weights[weightAt++];
                                point+=new Vector2(x*b.a+y*b.b+b.wx,x*b.c+y*b.d+b.wy)*w;
                            }
                        }
                        vh.AddVert(point,c,region.uv[i]);continue;
                    }
                    var p=region.corner[i];
                    vh.AddVert(new Vector3(p.x*bone.a+p.y*bone.b+bone.wx, p.x*bone.c+p.y*bone.d+bone.wy, 0), c, region.uv[i]);
                }
                if(region.triangles!=null)for(int i=0;i<region.triangles.Length;i+=3)vh.AddTriangle(v+region.triangles[i],v+region.triangles[i+1],v+region.triangles[i+2]);
                else {vh.AddTriangle(v,v+1,v+2); vh.AddTriangle(v,v+2,v+3);}
            }
        }

        static bool SampleAttach(Track track,float time,ref string name) {
            int i=Band(track,time); if(i<0)return true;
            name=track.attachment[i]; return !string.IsNullOrEmpty(name);
        }
        static Color SampleColor(Track track,float time) {
            int i=Band(track,time); if(i<0)return Color.white; int n=track.Count;
            if(i>=n-1||track.step[i])return track.color[i];
            float u=Blend(track,i,time); return Color.Lerp(track.color[i],track.color[i+1],u);
        }
        static void Sample(Track track,float time,out float x,out float y) {
            int i=Band(track,time);
            if(i<0){x=track.scale?1:0;y=track.scale?1:0;return;}
            if(i>=track.Count-1||track.step[i]){x=track.x[i];y=track.y[i];return;}
            float u=Blend(track,i,time);
            x=Mathf.Lerp(track.x[i],track.x[i+1],u); y=Mathf.Lerp(track.y[i],track.y[i+1],u);
        }
        static int Band(Track track,float time) {
            int n=track.Count; if(n==0||time<track.time[0])return -1; int i=0;
            while(i<n-1&&track.time[i+1]<=time)i++; return i;
        }
        static float Blend(Track track,int i,float time) {
            float span=track.time[i+1]-track.time[i]; float u=span<=.00001f?1:(time-track.time[i])/span;
            if(!track.bezier[i])return Mathf.Clamp01(u);
            return Bezier(u,track.cx1[i],track.cy1[i],track.cx2[i],track.cy2[i]);
        }
        static float Bezier(float t,float cx1,float cy1,float cx2,float cy2) {
            float u=Mathf.Clamp01(t);
            for(int n=0;n<8;n++) {
                float iu=1-u, x=3*iu*iu*u*cx1+3*iu*u*u*cx2+u*u*u, dx=3*iu*iu*cx1+6*iu*u*(cx2-cx1)+3*u*u*(1-cx2);
                if(Mathf.Abs(x-t)<.0001f)break;
                u=Mathf.Clamp01(dx==0?u:u-(x-t)/dx);
            }
            float k=1-u; return 3*k*k*u*cy1+3*k*u*u*cy2+u*u*u;
        }

        sealed class Page { public float u,v,u2,v2; public int degrees,ox,oy,pw,ph,ow,oh; }

        static Clip Build(string json,string atlas,Texture2D tex) {
            var root=(Dictionary<string,object>)MiniJson.Parse(json);
            var clip=new Clip{texture=tex,regions=new Dictionary<string,Region>(),anims=new Dictionary<string,Anim>()};
            var pages=ParseAtlas(atlas,tex.width,tex.height);
            var boneList=root.Arr("bones")??new List<object>();
            clip.bones=new Bone[boneList.Count]; var boneIndex=new Dictionary<string,int>();
            for(int i=0;i<boneList.Count;i++) {
                var b=(Dictionary<string,object>)boneList[i];
                var bone=new Bone{setupRot=b.F("rotation"),setupX=b.F("x"),setupY=b.F("y"),setupSx=b.F("scaleX",1),setupSy=b.F("scaleY",1)};
                string parent=b.Str("parent",null);
                if(!string.IsNullOrEmpty(parent)&&boneIndex.TryGetValue(parent,out int p))bone.parent=p;
                clip.bones[i]=bone; boneIndex[b.Str("name")]=i;
            }
            var slotList=root.Arr("slots")??new List<object>();
            clip.slotName=new string[slotList.Count]; clip.setupAttach=new string[slotList.Count]; clip.boneOf=new int[slotList.Count];clip.setupColor=new Color[slotList.Count];
            var slotIndex=new Dictionary<string,int>();
            for(int i=0;i<slotList.Count;i++) {
                var s=(Dictionary<string,object>)slotList[i];
                clip.slotName[i]=s.Str("name"); slotIndex[clip.slotName[i]]=i;
                clip.setupColor[i]=Hex(s.Str("color","ffffffff"));
                string setup=s.Str("attachment",null); clip.setupAttach[i]=string.IsNullOrEmpty(setup)?null:setup;
                clip.boneOf[i]=boneIndex.TryGetValue(s.Str("bone"),out int bi)?bi:0;
            }
            foreach(Dictionary<string,object> skin in root.Arr("skins")??new List<object>()) {
                var atts=skin.Obj("attachments"); if(atts==null)continue;
                foreach(var slot in atts) {
                    if(!slotIndex.ContainsKey(slot.Key))continue;
                    foreach(var item in (Dictionary<string,object>)slot.Value) {
                        var data=(Dictionary<string,object>)item.Value;
                        string type=data.Str("type","region");if(type!="region"&&type!="mesh")continue;
                        string image=data.Str("path",data.Str("name",item.Key)); if(string.IsNullOrEmpty(image))image=item.Key;
                        if(!pages.TryGetValue(image,out var page))continue;
                        clip.regions[slot.Key+"/"+item.Key]=type=="mesh"?MakeMesh(data,page):MakeRegion(data,page);
                    }
                }
            }
            var anims=root.Obj("animations");
            if(anims!=null)foreach(var pair in anims)clip.anims[pair.Key]=MakeAnim((Dictionary<string,object>)pair.Value,boneIndex,slotIndex);
            return clip;
        }

        static Region MakeRegion(Dictionary<string,object> data,Page page) {
            float w=data.F("width"), h=data.F("height"), sx=data.F("scaleX",1), sy=data.F("scaleY",1);
            float localX2=w*.5f, localY2=h*.5f, localX=-localX2, localY=-localY2;
            if(page.ow>0&&page.oh>0) {
                localX+=page.ox/(float)page.ow*w; localY+=page.oy/(float)page.oh*h;
                localX2-=(page.ow-page.ox-page.pw)/(float)page.ow*w; localY2-=(page.oh-page.oy-page.ph)/(float)page.oh*h;
            }
            localX*=sx; localY*=sy; localX2*=sx; localY2*=sy;
            float rot=data.F("rotation")*Mathf.Deg2Rad, cos=Mathf.Cos(rot), sin=Mathf.Sin(rot), ax=data.F("x"), ay=data.F("y");
            Vector2 Map(float lx,float ly) {
                float rx=lx*cos-ly*sin+ax, ry=lx*sin+ly*cos+ay; return new Vector2(rx,ry);
            }
            var corner=new[]{Map(localX,localY),Map(localX,localY2),Map(localX2,localY2),Map(localX2,localY)};
            Vector2 bl,tl,tr,br;
            if(page.degrees==90){bl=new Vector2(page.u2,page.v);tl=new Vector2(page.u,page.v);tr=new Vector2(page.u,page.v2);br=new Vector2(page.u2,page.v2);}
            else{bl=new Vector2(page.u,page.v);tl=new Vector2(page.u,page.v2);tr=new Vector2(page.u2,page.v2);br=new Vector2(page.u2,page.v);}
            return new Region{corner=corner,uv=new[]{bl,tl,tr,br}};
        }

        static Region MakeMesh(Dictionary<string,object> data,Page page) {
            var raw=data.Arr("uvs");var vertices=data.Arr("vertices");int count=raw.Count/2;
            var region=new Region{uv=new Vector2[count],triangles=data.Arr("triangles").ConvertAll(x=>Convert.ToInt32(x)).ToArray()};
            for(int i=0;i<count;i++) {
                float u=Num(raw[i*2]),v=Num(raw[i*2+1]);
                region.uv[i]=page.degrees==90?new Vector2(Mathf.Lerp(page.u,page.u2,v),Mathf.Lerp(page.v2,page.v,u)):new Vector2(Mathf.Lerp(page.u,page.u2,u),Mathf.Lerp(page.v2,page.v,v));
            }
            if(vertices.Count==count*2) {
                region.corner=new Vector2[count];for(int i=0;i<count;i++)region.corner[i]=new Vector2(Num(vertices[i*2]),Num(vertices[i*2+1]));
            } else {
                var bones=new List<int>();var weights=new List<float>();int at=0;
                for(int i=0;i<count;i++) {int n=Convert.ToInt32(vertices[at++]);bones.Add(n);
                    for(int k=0;k<n;k++){bones.Add(Convert.ToInt32(vertices[at++]));weights.Add(Num(vertices[at++]));weights.Add(Num(vertices[at++]));weights.Add(Num(vertices[at++]));}}
                region.weightedBones=bones.ToArray();region.weights=weights.ToArray();
            }
            return region;
        }

        static Anim MakeAnim(Dictionary<string,object> data,Dictionary<string,int> bones,Dictionary<string,int> slots) {
            var anim=new Anim();
            var boneAnims=data.Obj("bones");
            if(boneAnims!=null)foreach(var pair in boneAnims) {
                if(!bones.TryGetValue(pair.Key,out int index))continue;
                var timelines=(Dictionary<string,object>)pair.Value;
                if(timelines.Arr("rotate")!=null){var t=Keys(timelines.Arr("rotate"),false,false);anim.rotate[index]=t;anim.duration=Mathf.Max(anim.duration,t.time[t.Count-1]);}
                if(timelines.Arr("translate")!=null){var t=Keys(timelines.Arr("translate"),false,false);anim.translate[index]=t;anim.duration=Mathf.Max(anim.duration,t.time[t.Count-1]);}
                if(timelines.Arr("scale")!=null){var t=Keys(timelines.Arr("scale"),true,false);anim.scale[index]=t;anim.duration=Mathf.Max(anim.duration,t.time[t.Count-1]);}
            }
            var slotAnims=data.Obj("slots");
            if(slotAnims!=null)foreach(var pair in slotAnims) {
                if(!slots.TryGetValue(pair.Key,out int index))continue;
                var timelines=(Dictionary<string,object>)pair.Value;
                if(timelines.Arr("attachment")!=null){var t=Keys(timelines.Arr("attachment"),false,true);anim.slotAttach[index]=t;anim.duration=Mathf.Max(anim.duration,t.time[t.Count-1]);}
                if(timelines.Arr("color")!=null){var t=Keys(timelines.Arr("color"),false,false);anim.slotColor[index]=t;anim.duration=Mathf.Max(anim.duration,t.time[t.Count-1]);}
            }
            return anim;
        }

        static Track Keys(List<object> list,bool scale,bool attachment) {
            int n=list.Count; var t=new Track{scale=scale,time=new float[n],x=new float[n],y=new float[n],step=new bool[n],bezier=new bool[n],cx1=new float[n],cy1=new float[n],cx2=new float[n],cy2=new float[n],attachment=new string[n],color=new Color[n]};
            for(int i=0;i<n;i++) {
                var k=(Dictionary<string,object>)list[i];
                t.time[i]=k.F("time");
                float fallback=scale?1:0;
                t.x[i]=k.ContainsKey("angle")?k.F("angle"):k.F("x",fallback);
                t.y[i]=k.F("y",fallback);
                if(k.TryGetValue("name",out var name))t.attachment[i]=name as string;
                if(k.TryGetValue("color",out var hex)&&hex is string s)t.color[i]=Hex(s); else t.color[i]=Color.white;
                if(!k.TryGetValue("curve",out var curve)||curve==null)continue;
                if(curve is string){t.step[i]=true;continue;}
                var arr=curve as List<object>; if(arr==null||arr.Count<4)continue;
                t.bezier[i]=true; t.cx1[i]=Num(arr[0]); t.cy1[i]=Num(arr[1]); t.cx2[i]=Num(arr[2]); t.cy2[i]=Num(arr[3]);
            }
            return t;
        }
        static float Num(object v)=>v is double d?(float)d:0;
        static Color Hex(string s) {
            if(string.IsNullOrEmpty(s)||s.Length<6)return Color.white;
            byte C(int i)=>Convert.ToByte(s.Substring(i,2),16);
            float a=s.Length>=8?C(6)/255f:1; return new Color(C(0)/255f,C(2)/255f,C(4)/255f,a);
        }

        static Dictionary<string,Page> ParseAtlas(string atlas,int texW,int texH) {
            var pages=new Dictionary<string,Page>();
            var lines=atlas.Replace("\r","").Split('\n');
            int i=0; while(i<lines.Length&&string.IsNullOrWhiteSpace(lines[i]))i++;
            i++;
            // Unity can resize a non-power-of-two texture on import. Atlas coordinates still use
            // the source page dimensions, so normalizing by the imported texture breaks every UV.
            while(i<lines.Length&&lines[i].Contains(":")) {
                var header=lines[i++].Split(':');
                if(header[0].Trim()!="size"||header.Length<2)continue;
                var size=header[1].Split(',');
                if(size.Length==2&&int.TryParse(size[0].Trim(),out int width)&&int.TryParse(size[1].Trim(),out int height)&&width>0&&height>0){texW=width;texH=height;}
            }
            while(i<lines.Length) {
                while(i<lines.Length&&string.IsNullOrWhiteSpace(lines[i]))i++;
                if(i>=lines.Length)break;
                string name=lines[i++].Trim(); if(name.Length==0||name.Contains(":"))continue;
                bool rotate=false; int x=0,y=0,w=0,h=0,ox=0,oy=0,ow=0,oh=0;
                while(i<lines.Length&&lines[i].Contains(":")) {
                    var parts=lines[i++].Split(':'); string key=parts[0].Trim(); string val=parts.Length>1?parts[1].Trim():"";
                    if(key=="rotate")rotate=val=="true"||val=="90";
                    else if(key=="xy"){var p=val.Split(',');x=int.Parse(p[0]);y=int.Parse(p[1]);}
                    else if(key=="size"){var p=val.Split(',');w=int.Parse(p[0]);h=int.Parse(p[1]);}
                    else if(key=="orig"){var p=val.Split(',');ow=int.Parse(p[0]);oh=int.Parse(p[1]);}
                    else if(key=="offset"){var p=val.Split(',');ox=int.Parse(p[0]);oy=int.Parse(p[1]);}
                }
                float u=x/(float)texW, v=y/(float)texH, u2, v2;
                if(rotate){u2=(x+h)/(float)texW;v2=(y+w)/(float)texH;}
                else{u2=(x+w)/(float)texW;v2=(y+h)/(float)texH;}
                float fv=1-v2, fv2=1-v;
                pages[name]=new Page{u=u,v=fv,u2=u2,v2=fv2,degrees=rotate?90:0,ox=ox,oy=oy,pw=w,ph=h,ow=ow==0?w:ow,oh=oh==0?h:oh};
            }
            return pages;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Coffee.UIExtensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild.UI {
    using Data=Dictionary<string,object>;

    /// <summary>Sprites, fonts and materials exported from the original bundles by Tools/export_ui.py.</summary>
    public static class UiAssets {
        sealed class SpriteMeta {public int w,h;public Vector4 border;public float ppu=100;public Vector2 pivot=new Vector2(.5f,.5f);}
        static Dictionary<string,SpriteMeta> metas;
        static readonly Dictionary<string,Sprite> sprites=new Dictionary<string,Sprite>();
        static readonly Dictionary<string,bool> englishArt=new Dictionary<string,bool>();
        static readonly Dictionary<string,Sprite> authored=new Dictionary<string,Sprite>();
        static readonly Dictionary<string,TMP_FontAsset> fonts=new Dictionary<string,TMP_FontAsset>();

        static void LoadMetas() {
            if(metas!=null)return;metas=new Dictionary<string,SpriteMeta>();
            var file=Resources.Load<TextAsset>("UI/sprites");if(!file)return;
            var root=(Data)MiniJson.Parse(file.text);
            foreach(Data s in root.Arr("sprites")) {
                var b=s.Arr("border");var p=s.Arr("pivot");
                metas[s.Str("key")]=new SpriteMeta{w=s.I("w"),h=s.I("h"),ppu=s.F("ppu",100),
                    border=b==null?Vector4.zero:new Vector4(Convert.ToSingle(b[0]),Convert.ToSingle(b[1]),Convert.ToSingle(b[2]),Convert.ToSingle(b[3])),
                    pivot=p==null?new Vector2(.5f,.5f):new Vector2(Convert.ToSingle(p[0]),Convert.ToSingle(p[1]))};
            }
        }

        public static bool HasEnglishArt(string key) {
            if(string.IsNullOrEmpty(key))return false;
            if(englishArt.TryGetValue(key,out var yes))return yes;
            yes=PackedSpriteSheets.Contains("UI/Sprites/en/"+key)||Resources.Load<Texture2D>("UI/Sprites/en/"+key);
            englishArt[key]=yes;return yes;
        }

        public static Sprite Sprite(string key) {
            if(string.IsNullOrEmpty(key))return null;
            bool english=L10n.IsEnglish&&HasEnglishArt(key);
            return LoadSprite(key,english?"en:"+key:"zh:"+key,english?"UI/Sprites/en/"+key:"UI/Sprites/"+key);
        }

        static Sprite LoadSprite(string key,string cacheKey,string resource) {
            if(sprites.TryGetValue(cacheKey,out var cached))return cached;
            if(PackedSpriteSheets.TryGet(resource,out var packed)){sprites[cacheKey]=packed;return packed;}
            LoadMetas();
            var tex=Resources.Load<Texture2D>(resource);
            Sprite sprite=null;
            if(tex) {
                metas.TryGetValue(key,out var m);m=m??new SpriteMeta();
                // The exported PNG is the opaque crop. The original sprite rect (m.w, m.h) still includes the transparent margin,
                // and sliced borders are measured in that rect. Put the crop back in the middle so icons keep the original size.
                if(m.w>0&&m.h>0&&(tex.width!=m.w||tex.height!=m.h))tex=Pad(tex,m.w,m.h);
                int w=tex.width,h=tex.height;
                sprite=UnityEngine.Sprite.Create(tex,new Rect(0,0,w,h),m.pivot,m.ppu<=0?100:m.ppu,0,SpriteMeshType.FullRect,m.border);
                sprite.name=key;
            }
            sprites[cacheKey]=sprite;return sprite;
        }

        /// <summary>Swap remade artwork on an existing tree. Sprite names stay the original keys.</summary>
        public static void ApplyLanguage(GameObject root) {
            if(!root)return;
            foreach(var image in root.GetComponentsInChildren<Image>(true)) {
                var next=Localize(image.sprite);
                if(next&&next!=image.sprite)image.sprite=next;
            }
            foreach(var sequence in root.GetComponentsInChildren<UISpriteAnimationSequence>(true))sequence.Retarget(Localize);
            foreach(var system in root.GetComponentsInChildren<ParticleSystem>(true)) {
                var sheet=system.textureSheetAnimation;
                if(sheet.mode!=ParticleSystemAnimationMode.Sprites)continue;
                for(int i=0;i<sheet.spriteCount;i++) {
                    var next=Localize(sheet.GetSprite(i));
                    if(next&&next!=sheet.GetSprite(i))sheet.SetSprite(i,next);
                }
            }
            foreach(var particle in root.GetComponentsInChildren<UIParticle>(true))particle.RefreshParticles();
        }

        static Sprite Localize(Sprite current) {
            if(!current||string.IsNullOrEmpty(current.name)||!HasEnglishArt(current.name))return current;
            if(L10n.IsEnglish) {
                var next=Sprite(current.name);
                if(next&&next!=current&&!authored.ContainsKey(current.name))authored[current.name]=current;
                return next?next:current;
            }
            if(authored.TryGetValue(current.name,out var original))return original;
            return LoadSprite(current.name,"zh:"+current.name,"UI/Sprites/"+current.name)??current;
        }

        /// <summary>Puts a cropped sprite back inside its original pixel rect. A larger source is scaled to that rect.</summary>
        static Texture2D Pad(Texture2D src,int w,int h) {
            if(src.width>w||src.height>h)return Read(src,w,h);
            var copy=Read(src,src.width,src.height);var pixels=copy.GetPixels32();
            if(Application.isPlaying)UnityEngine.Object.Destroy(copy);else UnityEngine.Object.DestroyImmediate(copy);
            var canvas=new Color32[w*h];int ox=(w-src.width)/2,oy=(h-src.height)/2;
            for(int y=0;y<src.height;y++)Array.Copy(pixels,y*src.width,canvas,(oy+y)*w+ox,src.width);
            var tex=new Texture2D(w,h,TextureFormat.RGBA32,false,false);
            tex.SetPixels32(canvas);tex.filterMode=src.filterMode;tex.wrapMode=TextureWrapMode.Clamp;tex.Apply(false,true);return tex;
        }
        static Texture2D Read(Texture2D src,int w,int h) {
            var rt=RenderTexture.GetTemporary(w,h,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);
            Graphics.Blit(src,rt);var prev=RenderTexture.active;RenderTexture.active=rt;
            var tex=new Texture2D(w,h,TextureFormat.RGBA32,false,false);tex.ReadPixels(new Rect(0,0,w,h),0,0);tex.Apply();
            RenderTexture.active=prev;RenderTexture.ReleaseTemporary(rt);tex.filterMode=src.filterMode;tex.wrapMode=TextureWrapMode.Clamp;return tex;
        }

        static Dictionary<string,string> paths;
        /// <summary>Sprite by original asset path, e.g. "Assets/GameRes/honor/3.png".</summary>
        public static Sprite SpriteAt(string assetPath) {
            if(paths==null) {
                paths=new Dictionary<string,string>();var file=Resources.Load<TextAsset>("UI/sprite_paths");
                if(file)foreach(Data p in ((Data)MiniJson.Parse(file.text)).Arr("paths"))paths[p.Str("path")]=p.Str("key");
            }
            return paths.TryGetValue(assetPath,out var key)?Sprite(key):null;
        }

        public static Texture2D Texture(string key)=>Resources.Load<Texture2D>("UI/Textures/"+key);

        public static TMP_FontAsset Font(string name="hdhSDF") {
            if(fonts.TryGetValue(name,out var f))return f;
            f=Resources.Load<TMP_FontAsset>("UI/Fonts/"+name);fonts[name]=f;return f;
        }

        static readonly Dictionary<string,TMP_SpriteAsset> spriteAssets=new Dictionary<string,TMP_SpriteAsset>();
        public static TMP_SpriteAsset SpriteAsset(string name) {
            if(string.IsNullOrEmpty(name))return null;
            if(spriteAssets.TryGetValue(name,out var a))return a;
            a=Resources.Load<TMP_SpriteAsset>("UI/SpriteAssets/"+name);spriteAssets[name]=a;return a;
        }
    }

    /// <summary>An instantiated original prefab. Nodes keep the export order (depth first).</summary>
    public sealed class UiView {
        public readonly string Resource;public readonly GameObject Root;public readonly GameObject[] Nodes;
        readonly List<Data>[] components;
        internal UiView(string resource,GameObject[] nodes,List<Data>[] components){Resource=resource;Nodes=nodes;Root=nodes[0];this.components=components;}

        public RectTransform Rect=>(RectTransform)Root.transform;
        public GameObject Node(string name) {foreach(var n in Nodes)if(n.name==name)return n;throw new KeyNotFoundException(Resource+" has no node "+name);}
        public GameObject Find(string path) {
            var t=Root.transform.Find(path);if(!t)throw new KeyNotFoundException(Resource+" has no path "+path);return t.gameObject;
        }
        public T Get<T>(string pathOrName) where T:Component {
            var go=pathOrName.Contains("/")?Find(pathOrName):Node(pathOrName);var c=go.GetComponent<T>();
            if(!c)throw new MissingComponentException(Resource+"/"+pathOrName+" has no "+typeof(T).Name);return c;
        }
        public int IndexOf(GameObject go)=>Array.IndexOf(Nodes,go);
        public Data Script(string cls) {foreach(var list in components)foreach(var c in list)if(c.Str("t")==cls)return c;return null;}
        public Data Script(string cls,int node){foreach(var c in components[node])if(c.Str("t")==cls)return c;return null;}

        /// <summary>Resolves a serialized field of an original script (e.g. TopPanel.goldNumTmp) to the node it referenced.</summary>
        public GameObject Ref(string cls,string field) {
            var r=Script(cls).Obj(field);
            if(r==null||!r.ContainsKey("node"))throw new KeyNotFoundException(Resource+" "+cls+"."+field+" is not a node reference");
            return Nodes[r.I("node")];
        }
        public T Ref<T>(string cls,string field) where T:Component=>Ref(cls,field).GetComponent<T>();
        public GameObject[] Refs(string cls,string field) {
            var list=Script(cls).Arr(field);if(list==null)return new GameObject[0];
            return list.OfType<Data>().Where(x=>x.ContainsKey("node")).Select(x=>Nodes[x.I("node")]).ToArray();
        }
        public string SpriteField(string cls,string field)=>Script(cls).Obj(field).Str("sprite");
    }

    public static class UiPrefab {
        static readonly Dictionary<string,Data> cache=new Dictionary<string,Data>();
        public const string Font="hdhSDF";

        public static Data Load(string resource) {
            if(cache.TryGetValue(resource,out var d))return d;
            var file=Resources.Load<TextAsset>(resource);if(!file)throw new Exception("Missing UI prefab "+resource);
            d=(Data)MiniJson.Parse(file.text);cache[resource]=d;return d;
        }

        /// <summary>Instantiates "UI/Prefabs/Module/name" under parent with the original RectTransform and component values.</summary>
        public static UiView Instantiate(string resource,Transform parent) {
            if(!resource.StartsWith("UI/"))resource="UI/Prefabs/"+resource;
            var data=Load(resource);var list=data.Arr("nodes");int count=list.Count;
            bool nativeParticles=NativeParticles.Sources.Any(x=>resource=="UI/Prefabs/"+x);
            var nodes=new GameObject[count];var comps=new List<Data>[count];var raw=new Data[count];
            for(int i=0;i<count;i++) {
                var n=(Data)list[i];raw[i]=n;int p=n.I("p",-1);
                bool particle=nativeParticles&&(n.Arr("c")??new List<object>()).OfType<Data>().Any(c=>c.Str("t")=="ParticleSystem");
                var go=particle?NativeParticles.Create(resource,i,p<0?parent:nodes[p].transform):n.ContainsKey("rt")?new GameObject(n.Str("n"),typeof(RectTransform)):new GameObject(n.Str("n"));
                go.name=n.Str("n");
                go.SetActive(false);go.transform.SetParent(p<0?parent:nodes[p].transform,false);nodes[i]=go;
                ApplyTransform(go.transform,n);
                comps[i]=(n.Arr("c")??new List<object>()).Cast<Data>().ToList();
            }
            var view=new UiView(resource,nodes,comps);
            for(int i=0;i<count;i++)foreach(var c in comps[i].Where(IsGraphic))AddComponent(view,nodes[i],c);
            for(int i=0;i<count;i++)foreach(var c in comps[i].Where(c=>!IsGraphic(c)))AddComponent(view,nodes[i],c);
            if(nativeParticles) {
                for(int i=0;i<count;i++) {
                    var particle=comps[i].FirstOrDefault(c=>c.Str("t")=="ParticleSystem");
                    var renderer=comps[i].FirstOrDefault(c=>c.Str("t")=="ParticleSystemRenderer");
                    if(particle!=null)NativeParticles.RestorePresentation(nodes[i],particle,renderer);
                }
                for(int i=0;i<count;i++)foreach(var c in comps[i].Where(c=>c.Str("t")=="UIParticle"))NativeParticles.Bind(view,i,c);
            }
            // The original list scripts resized their grids after populating them. Their exported
            // content rects are only placeholders; ScrollRect needs the populated layout's size.
            foreach(var node in nodes) {
                var scroll=node.GetComponent<ScrollRect>();
                if(!scroll||!scroll.content||!scroll.content.GetComponent<GridLayoutGroup>())continue;
                var fit=scroll.content.GetComponent<ContentSizeFitter>()??scroll.content.gameObject.AddComponent<ContentSizeFitter>();
                if(scroll.vertical)fit.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
                if(scroll.horizontal)fit.horizontalFit=ContentSizeFitter.FitMode.PreferredSize;
            }
            for(int i=count-1;i>=0;i--)nodes[i].SetActive(raw[i].B("a",true));
            if(L10n.IsEnglish)UiAssets.ApplyLanguage(view.Root);
            return view;
        }

        static bool IsGraphic(Data c){var t=c.Str("t");return t=="Image"||t=="RawImage"||t=="TextMeshProUGUI";}

        static void ApplyTransform(Transform t,Data n) {
            var rt=n.Arr("rt");
            if(rt!=null) {
                float f(int i)=>Convert.ToSingle(rt[i]);
                var r=(RectTransform)t;
                r.anchorMin=new Vector2(f(0),f(1));r.anchorMax=new Vector2(f(2),f(3));r.pivot=new Vector2(f(8),f(9));
                r.sizeDelta=new Vector2(f(6),f(7));r.anchoredPosition3D=new Vector3(f(4),f(5),f(14));
                r.localScale=new Vector3(f(10),f(11),f(12));r.localEulerAngles=new Vector3(0,0,f(13));
                return;
            }
            var tr=n.Arr("tr");
            if(tr!=null) {
                float f(int i)=>Convert.ToSingle(tr[i]);
                t.localPosition=new Vector3(f(0),f(1),f(2));t.localScale=new Vector3(f(3),f(4),f(5));t.localEulerAngles=new Vector3(0,0,f(6));
            }
        }

        static Graphic GraphicAt(UiView view,Data r) {
            if(r==null||!r.ContainsKey("node"))return null;return view.Nodes[r.I("node")].GetComponent<Graphic>();
        }
        static RectTransform RectAt(UiView view,Data r) {
            if(r==null||!r.ContainsKey("node"))return null;return view.Nodes[r.I("node")].transform as RectTransform;
        }
        static Sprite SpriteOf(Data c,string field)=>UiAssets.Sprite(c.Obj(field).Str("sprite",null));

        static void Selectable(UiView view,Selectable s,Data c) {
            s.transition=(Selectable.Transition)c.I("m_Transition",1);
            var colors=c.Obj("m_Colors");
            if(colors!=null) {
                var block=ColorBlock.defaultColorBlock;
                block.normalColor=colors.Color("m_NormalColor",block.normalColor);block.highlightedColor=colors.Color("m_HighlightedColor",block.highlightedColor);
                block.pressedColor=colors.Color("m_PressedColor",block.pressedColor);block.selectedColor=colors.Color("m_SelectedColor",block.selectedColor);
                block.disabledColor=colors.Color("m_DisabledColor",block.disabledColor);block.colorMultiplier=colors.F("m_ColorMultiplier",1);block.fadeDuration=colors.F("m_FadeDuration",.1f);
                s.colors=block;
            }
            var states=c.Obj("m_SpriteState");
            if(states!=null)s.spriteState=new SpriteState{highlightedSprite=SpriteOf(states,"m_HighlightedSprite"),pressedSprite=SpriteOf(states,"m_PressedSprite"),selectedSprite=SpriteOf(states,"m_SelectedSprite"),disabledSprite=SpriteOf(states,"m_DisabledSprite")};
            s.targetGraphic=GraphicAt(view,c.Obj("m_TargetGraphic"));
            s.interactable=c.B("m_Interactable",true);
            var nav=s.navigation;nav.mode=Navigation.Mode.None;s.navigation=nav;
        }

        static void LayoutGroup(HorizontalOrVerticalLayoutGroup g,Data c) {
            Padding(g,c);g.spacing=c.F("m_Spacing");g.childAlignment=(TextAnchor)c.I("m_ChildAlignment");
            g.childForceExpandWidth=c.B("m_ChildForceExpandWidth",true);g.childForceExpandHeight=c.B("m_ChildForceExpandHeight",true);
            g.childControlWidth=c.B("m_ChildControlWidth");g.childControlHeight=c.B("m_ChildControlHeight");
            g.childScaleWidth=c.B("m_ChildScaleWidth");g.childScaleHeight=c.B("m_ChildScaleHeight");g.reverseArrangement=c.B("m_ReverseArrangement");
        }
        static void Padding(LayoutGroup g,Data c){var p=c.Obj("m_Padding");if(p!=null)g.padding=new RectOffset(p.I("m_Left"),p.I("m_Right"),p.I("m_Top"),p.I("m_Bottom"));}

        static void AddComponent(UiView view,GameObject go,Data c) {
            string type=c.Str("t");bool enabled=c.B("m_Enabled",true);Behaviour behaviour=null;
            switch(type) {
                case "Image": {
                    var img=go.AddComponent<Image>();behaviour=img;
                    img.sprite=SpriteOf(c,"m_Sprite");img.color=c.Color("m_Color",Color.white);img.raycastTarget=c.B("m_RaycastTarget",true);
                    img.type=(Image.Type)c.I("m_Type");img.preserveAspect=c.B("m_PreserveAspect");img.fillCenter=c.B("m_FillCenter",true);
                    img.fillMethod=(Image.FillMethod)c.I("m_FillMethod",4);img.fillAmount=c.F("m_FillAmount",1);img.fillClockwise=c.B("m_FillClockwise",true);
                    img.fillOrigin=c.I("m_FillOrigin");img.useSpriteMesh=c.B("m_UseSpriteMesh");img.pixelsPerUnitMultiplier=c.F("m_PixelsPerUnitMultiplier",1);
                    break;
                }
                case "RawImage": {
                    var raw=go.AddComponent<RawImage>();behaviour=raw;
                    raw.texture=UiAssets.Texture(c.Obj("m_Texture").Str("texture",null));raw.color=c.Color("m_Color",Color.white);raw.raycastTarget=c.B("m_RaycastTarget",true);
                    var uv=c.Obj("m_UVRect");if(uv!=null)raw.uvRect=new Rect(uv.F("x"),uv.F("y"),uv.F("width",1),uv.F("height",1));
                    break;
                }
                case "TextMeshProUGUI": {
                    var t=go.AddComponent<TextMeshProUGUI>();behaviour=t;
                    t.font=UiAssets.Font(c.Obj("m_fontAsset").Str("font",Font))??UiAssets.Font();
                    t.raycastTarget=c.B("m_RaycastTarget",true);t.richText=c.B("m_isRichText",true);
                    t.fontStyle=(FontStyles)c.I("m_fontStyle");t.fontSize=c.F("m_fontSize",36);
                    t.enableAutoSizing=c.B("m_enableAutoSizing");t.fontSizeMin=c.F("m_fontSizeMin",18);t.fontSizeMax=c.F("m_fontSizeMax",72);
                    t.horizontalAlignment=(HorizontalAlignmentOptions)c.I("m_HorizontalAlignment",1);t.verticalAlignment=(VerticalAlignmentOptions)c.I("m_VerticalAlignment",256);
                    t.characterSpacing=c.F("m_characterSpacing");t.wordSpacing=c.F("m_wordSpacing");t.lineSpacing=c.F("m_lineSpacing");t.paragraphSpacing=c.F("m_paragraphSpacing");
                    t.enableWordWrapping=c.B("m_enableWordWrapping",true);t.overflowMode=(TextOverflowModes)c.I("m_overflowMode");
                    t.margin=c.V4("m_margin");t.color=c.Color("m_fontColor",Color.white);
                    if(c.B("m_enableVertexGradient")) {
                        var g=c.Obj("m_fontColorGradient");t.enableVertexGradient=true;
                        t.colorGradient=new VertexGradient(g.Color("topLeft",Color.white),g.Color("topRight",Color.white),g.Color("bottomLeft",Color.white),g.Color("bottomRight",Color.white));
                    }
                    var spriteAsset=UiAssets.SpriteAsset(c.Obj("m_spriteAsset").Str("spriteAsset",null));
                    if(spriteAsset)t.spriteAsset=spriteAsset;
                    t.text=c.Str("m_text");
                    LocalizedText.Set(t,t.text);
                    break;
                }
                case "TextMeshProOutline": {
                    if(!enabled||!c.B("enableOutline",true))return;
                    var t=go.GetComponent<TextMeshProUGUI>();if(!t)return;
                    t.outlineColor=c.Color("outlineColor",Color.black);t.outlineWidth=c.F("outlineWidth",.2f);return;
                }
                case "Button": {
                    var b=go.AddComponent<Button>();behaviour=b;Selectable(view,b,c);break;
                }
                case "Toggle": {
                    var tg=go.AddComponent<Toggle>();behaviour=tg;Selectable(view,tg,c);
                    tg.graphic=GraphicAt(view,c.Obj("graphic"));tg.toggleTransition=(Toggle.ToggleTransition)c.I("toggleTransition",1);tg.isOn=c.B("m_IsOn",true);break;
                }
                case "Slider": {
                    var s=go.AddComponent<Slider>();behaviour=s;Selectable(view,s,c);
                    s.fillRect=RectAt(view,c.Obj("m_FillRect"));s.handleRect=RectAt(view,c.Obj("m_HandleRect"));s.direction=(Slider.Direction)c.I("m_Direction");
                    s.minValue=c.F("m_MinValue");s.maxValue=c.F("m_MaxValue",1);s.wholeNumbers=c.B("m_WholeNumbers");s.value=c.F("m_Value");break;
                }
                case "ScrollRect": {
                    var s=go.AddComponent<ScrollRect>();behaviour=s;
                    s.content=RectAt(view,c.Obj("m_Content"));s.viewport=RectAt(view,c.Obj("m_Viewport"));s.horizontal=c.B("m_Horizontal");s.vertical=c.B("m_Vertical",true);
                    s.movementType=(ScrollRect.MovementType)c.I("m_MovementType",1);s.elasticity=c.F("m_Elasticity",.1f);s.inertia=c.B("m_Inertia",true);
                    s.decelerationRate=c.F("m_DecelerationRate",.135f);s.scrollSensitivity=Mathf.Max(30,c.F("m_ScrollSensitivity",1)*30);break;
                }
                case "Mask": {var m=go.AddComponent<Mask>();behaviour=m;m.showMaskGraphic=c.B("m_ShowMaskGraphic",true);break;}
                case "RectMask2D": {
                    var m=go.AddComponent<RectMask2D>();behaviour=m;m.padding=c.V4("m_Padding");
                    var soft=c.Obj("m_Softness");if(soft!=null)m.softness=new Vector2Int(soft.I("x"),soft.I("y"));break;
                }
                case "HorizontalLayoutGroup": {var g=go.AddComponent<HorizontalLayoutGroup>();behaviour=g;LayoutGroup(g,c);break;}
                case "VerticalLayoutGroup": {var g=go.AddComponent<VerticalLayoutGroup>();behaviour=g;LayoutGroup(g,c);break;}
                case "GridLayoutGroup": {
                    var g=go.AddComponent<GridLayoutGroup>();behaviour=g;Padding(g,c);g.cellSize=c.V2("m_CellSize");g.spacing=c.V2("m_Spacing");
                    g.startCorner=(GridLayoutGroup.Corner)c.I("m_StartCorner");g.startAxis=(GridLayoutGroup.Axis)c.I("m_StartAxis");g.childAlignment=(TextAnchor)c.I("m_ChildAlignment");
                    g.constraint=(GridLayoutGroup.Constraint)c.I("m_Constraint");g.constraintCount=Mathf.Max(1,c.I("m_ConstraintCount",2));break;
                }
                case "ContentSizeFitter": {
                    var f=go.AddComponent<ContentSizeFitter>();behaviour=f;f.horizontalFit=(ContentSizeFitter.FitMode)c.I("m_HorizontalFit");f.verticalFit=(ContentSizeFitter.FitMode)c.I("m_VerticalFit");break;
                }
                case "LayoutElement": {
                    var e=go.AddComponent<LayoutElement>();behaviour=e;e.ignoreLayout=c.B("m_IgnoreLayout");
                    e.minWidth=c.F("m_MinWidth",-1);e.minHeight=c.F("m_MinHeight",-1);e.preferredWidth=c.F("m_PreferredWidth",-1);e.preferredHeight=c.F("m_PreferredHeight",-1);
                    e.flexibleWidth=c.F("m_FlexibleWidth",-1);e.flexibleHeight=c.F("m_FlexibleHeight",-1);e.layoutPriority=c.I("m_LayoutPriority",1);break;
                }
                case "CanvasGroup": {
                    var g=go.AddComponent<CanvasGroup>();g.alpha=c.F("m_Alpha",1);g.interactable=c.B("m_Interactable",true);g.blocksRaycasts=c.B("m_BlocksRaycasts",true);g.ignoreParentGroups=c.B("m_IgnoreParentGroups");
                    if(!enabled)g.enabled=false;return;
                }
                case "Canvas": {
                    if(go.transform.parent==null)return;
                    var canvas=go.AddComponent<Canvas>();behaviour=canvas;
                    canvas.overrideSorting=c.B("m_OverrideSorting");canvas.sortingOrder=c.I("m_SortingOrder");
                    canvas.additionalShaderChannels=(AdditionalCanvasShaderChannels)c.I("m_AdditionalShaderChannelsFlag");break;
                }
                case "GraphicRaycaster": {if(go.GetComponent<Canvas>()&&!go.GetComponent<GraphicRaycaster>())behaviour=go.AddComponent<GraphicRaycaster>();break;}
                case "UpDownAnimator": {var a=go.AddComponent<UpDownAnimator>();behaviour=a;a.Configure(c.F("moveHeight",.1f),c.F("moveTime",.5f),c.B("isUI"));break;}
                case "LeftRightAnimator": {var a=go.AddComponent<LeftRightAnimator>();behaviour=a;a.Configure(c.F("moveWidth",10),c.F("moveTime",.5f));break;}
                case "UISpriteAnimationSequence": {
                    var a=go.AddComponent<UISpriteAnimationSequence>();behaviour=a;
                    var frames=(c.Arr("spriteFrames")??new List<object>()).OfType<Data>().Select(x=>UiAssets.Sprite(x.Str("sprite",null))).Where(x=>x).ToArray();
                    a.Configure(frames,c.F("frameInterval",.04f),c.B("isLooping",true),c.B("autoPlay",true));break;
                }
                default:return;
            }
            if(behaviour&&!enabled)behaviour.enabled=false;
        }
    }
}

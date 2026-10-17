using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Bufu.Rebuild.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace Bufu.Rebuild {
    // Activated only by an explicit QA command-line flag; never touches the player's save.
    public sealed partial class RuntimeVerification:MonoBehaviour {
        [Serializable] public sealed class Result {public bool passed;public string error,editor;public int width,height,checks;public string[] messages;}
        readonly List<string> checks=new List<string>();readonly List<string> errors=new List<string>();
        string output;GameController game;float timeout;bool finished;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init(){if(GameController.VerificationMode)new GameObject("Runtime QA",typeof(RuntimeVerification));}
        void Awake() {var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"--verify-rebuild");output=at>=0&&at+1<args.Length?args[at+1]:Application.persistentDataPath;Directory.CreateDirectory(output);Application.logMessageReceived+=Log;}
        void OnDestroy(){Application.logMessageReceived-=Log;}
        void Log(string message,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message+"\n"+trace);}
        void Update(){timeout+=Time.unscaledDeltaTime;if(timeout>90&&!finished)Complete(false,"Runtime QA timed out\n"+string.Join("\n",errors));}
        void Check(bool condition,string text) {if(!condition)throw new Exception(text);checks.Add(text);}

        // Drive nested verification iterators so failures immediately produce a useful result file.
        IEnumerator Start() {
            var stack=new Stack<IEnumerator>();stack.Push(Verify());
            while(stack.Count>0&&!finished) {
                object next=null;bool moved=false;Exception failure=null;
                try {moved=stack.Peek().MoveNext();if(moved)next=stack.Peek().Current;}catch(Exception e){failure=e;}
                if(failure!=null){Fail(failure);yield break;}
                if(!moved){stack.Pop();continue;}
                if(next is IEnumerator nested)stack.Push(nested);else yield return next;
            }
        }

        IEnumerator Verify() {
            yield return null;yield return new WaitForSeconds(.8f);game=FindObjectOfType<GameController>();
            Check(game!=null&&game.Model!=null,"runtime initialization");
            VerifyBottomNavigation();
            if(Environment.GetCommandLineArgs().Contains("--verify-wake")){yield return VerifyWakeTip();yield break;}
            if(Environment.GetCommandLineArgs().Contains("--verify-localization")){yield return VerifyLocalization();yield break;}
            if(Environment.GetCommandLineArgs().Contains("--verify-transition")){yield return VerifyTransition();yield break;}
            if(Environment.GetCommandLineArgs().Contains("--verify-parity")){yield return VerifyParity();yield break;}
            if(Environment.GetCommandLineArgs().Contains("--verify-support")){yield return VerifySupport();yield break;}
            if(Environment.GetCommandLineArgs().Contains("--verify-promotion")){yield return VerifyPromotion();yield break;}
            if(game.Model.DeveloperMode){yield return VerifyDeveloperMode();yield break;}
            CheckGrowthBadge(true,"initial item tab");
            Check(game.Coins.Count==3,"new game renders 3 real copper VAT coins");
            Check(Resources.Load<AudioClip>("Audio/coinup")!=null,"runtime coin audio loaded");
            CheckGrowthLevel(1);yield return Capture("01-new-game.png");
            var upgrade=Field<UiView>("upgrade");
            var items=upgrade.Find("ItemPanel/panel1/Scroll View").GetComponent<ScrollRect>();
            yield return VerifyScroll(items,"item list without unlocked tab hints",64);
            yield return Capture("06-items-scroll-bottom.png");
            items.verticalNormalizedPosition=1;items.StopMovement();Canvas.ForceUpdateCanvases();

            UnityEngine.Random.InitState(20261014);Canvas.ForceUpdateCanvases();
            var coin=game.Coins.FirstOrDefault(c=>FirstHit(ScreenPoint(c.Rect))?.GetComponent<CoinActor>()==c);
            Check(coin!=null,"actual UI raycast reaches a coin");
            var pointer=Pointer(ScreenPoint(coin.Rect));var coinHit=FirstHit(pointer.position);
            ExecuteEvents.ExecuteHierarchy(coinHit,pointer,ExecuteEvents.pointerDownHandler);
            Check(coin.Busy,"pointer input starts coin animation");yield return new WaitForSeconds(.8f);
            for(int i=0;i<12&&game.Model.State.gold<25;i++){game.FlipAll();yield return new WaitForSeconds(.8f);}
            Check(game.Model.State.gold>=25,"manual animation completion settles actual income");
            var helper=upgrade.Find("ItemPanel/panel1/Scroll View/Viewport/Content/item0_1");
            Click(PrefabRef<Button>(helper,"Common/upgradeItem","UpgradeItem","upgradeBtn"),"buy helper");
            Check(game.Model.Helpers==1&&game.Field.GetComponentsInChildren<HelperActor>().Length==1,"helper purchase creates animated actor");
            double before=game.Model.State.lifetime;yield return new WaitForSeconds(7);
            Check(game.Model.State.lifetime>before,"automatic helpers produce income without manual input");
            yield return DismissGoals();

            // Reproduce the reported skill list at growth level 7 before the high-level stress checks.
            game.Model.State.mission=6;Refresh();CheckGrowthLevel(7);
            Click(upgrade.Ref<Button>("UpgradeItemPanel","button2"),"open level 7 skills");yield return null;
            CheckGrowthBadge(true,"item skills subpage");
            var boosts=upgrade.Find("ItemPanel/panel2/Scroll View").GetComponent<ScrollRect>();
            yield return VerifyScroll(boosts,"level 7 skill list",64);
            yield return Capture("07-skills-scroll-bottom-level7.png");

            // Jump only the isolated test save to unlocked pages after exercising normal earnings.
            foreach(int level in new[]{10,20,91,120}){game.Model.State.mission=level-1;Refresh();CheckGrowthLevel(level);}
            game.Model.State.mission=game.Model.WorldMissions.Length;game.Model.State.gold=20000;Refresh();
            Check(Field<TMP_Text>("taskTmp").text==""&&Field<Image>("taskFill").fillAmount==0,"completed growth goals clear the target and progress");
            yield return Capture("04-growth-levels.png");

            Check(boosts.IsActive(),"skills remain selected after growth label checks");yield return null;
            yield return VerifyScroll(boosts,"item enhancement list with unlocked tab hints",64);
            yield return Capture("08-skills-scroll-bottom-unlocked.png");
            boosts.verticalNormalizedPosition=1;boosts.StopMovement();Canvas.ForceUpdateCanvases();
            var copper=boosts.content.Find("item1_1").gameObject;int boostBefore=game.Model.State.boosts[1];
            Click(PrefabRef<Button>(copper,"Common/upgradeItem","UpgradeItem","upgradeBtn"),"buy copper enhancement");
            Check(game.Model.State.boosts[1]==boostBefore+1,"prefab value upgrade changes economy");
            yield return Capture("02-helpers-upgrades.png");

            Click(upgrade.Ref<Button>("UpgradePanel","button2"),"open facilities");yield return null;yield return null;
            var facility=Field<UiView>("facilityView");
            CheckGrowthBadge(false,"facility tab");
            Check(facility.Ref("FacilityPanel","helperPanel").activeInHierarchy&&!facility.Ref("FacilityPanel","deskPanel").activeInHierarchy,"facilities initially open the helper subpage");
            var helpers=facility.Ref<RectTransform>("HelperPanel","contentParent").GetComponentInParent<ScrollRect>();
            yield return VerifyScroll(helpers,"facility helper list",64);yield return Capture("10-facility-helpers-scroll-bottom.png");
            Click(facility.Ref<Button>("FacilityPanel","deskTabBtn"),"open facility desk list");yield return null;
            var desks=facility.Ref<RectTransform>("DeskPanel","contentParent").GetComponentInParent<ScrollRect>();
            yield return VerifyScroll(desks,"facility desk list",64);yield return Capture("05-facility-scroll.png");

            Click(upgrade.Ref<Button>("UpgradePanel","button3"),"open talents");yield return null;
            CheckGrowthBadge(false,"talent tab");

            yield return VerifyShop();
            string json=JsonUtility.ToJson(game.Model.Data,true);File.WriteAllText(Path.Combine(output,"test-save.json"),json);
            var restored=new GameModel(JsonUtility.FromJson<SaveData>(File.ReadAllText(Path.Combine(output,"test-save.json"))));
            Check(restored.Helpers==game.Model.Helpers&&restored.State.gold==game.Model.State.gold&&restored.Data.diamonds==game.Model.Data.diamonds,"save to disk and restore preserves progress");

            Click(upgrade.Ref<Button>("UpgradePanel","button6"),"open world selection");yield return null;yield return null;
            CheckGrowthBadge(false,"world selection tab");
            var worlds=Field<UiView>("worldView");var worldScroll=worlds.Ref<ScrollRect>("WorldSelectionPanel","worldScrollView");
            var dice=worldScroll.content.Cast<Transform>().First(t=>t.GetComponentsInChildren<TMP_Text>(true).Any(x=>x.text==L10n.Translate(game.Model.WorldName(2)))).gameObject;
            var island=PrefabRef<Button>(dice,"world/WorldNode 1","WorldNode","islandButton");
            CenterInScroll(worldScroll,(RectTransform)island.transform);yield return null;
            Click(island,"select dice world");yield return null;
            var popupLayer=Field<RectTransform>("popupLayer");var dialog=popupLayer.GetChild(popupLayer.childCount-1).gameObject;
            Click(PrefabRef<Button>(dialog,"Common/commonDialog","CommonDialog","confirmButton"),"confirm world switch");
            Check(game.Model.Data.world==2&&game.Coins.Count==3,"world UI switches to original dice assets");
            CheckGrowthBadge(true,"item tab after world switch");
            game.FlipAll();yield return new WaitForSeconds(.8f);yield return Capture("03-dice-world.png");
            Check(game.Model.State.gold>0,"dice animation completes and settles face rewards");
            game.Model.State.mission=Math.Max(8,(int)game.Model.Factor(80));Refresh();
            for(int page=1;page<=3;page++) {
                Click(upgrade.Ref<Button>("UpgradeItemPanel","button"+page),"open dice item subpage "+page);yield return null;
                var scroll=upgrade.Find("ItemPanel/panel"+page+"/Scroll View").GetComponent<ScrollRect>();
                yield return VerifyScroll(scroll,"dice item subpage "+page,64);
                yield return Capture("09-dice-subpage"+page+"-scroll-bottom.png");
            }
            Check(errors.Count==0,"no runtime errors or exceptions");Complete(true,null);
        }

        void VerifyBottomNavigation() {
            Canvas.ForceUpdateCanvases();var view=Field<UiView>("upgrade");
            Check(!view.Ref("UpgradePanel","button4").activeInHierarchy,"removed challenge tab has no visible or clickable entry");
            int[] ids={1,2,3,5,6};Rect previous=default;float width=0;
            foreach(int id in ids) {
                var button=view.Ref<RectTransform>("UpgradePanel","button"+id);var bounds=RootBounds(button);
                Check(button.gameObject.activeInHierarchy&&bounds.xMin>=game.Root.rect.xMin&&bounds.xMax<=game.Root.rect.xMax,"bottom navigation stays inside safe content width: "+id);
                if(width>0)Check(Mathf.Abs(bounds.width-width)<1&&bounds.xMin>previous.xMax,"five equal navigation cells do not overlap: "+id);
                width=bounds.width;previous=bounds;
            }
            Check(width>260,"five-tab layout uses available width for larger touch targets");
        }

        T Field<T>(string name) {var field=typeof(GameController).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic);if(field==null)throw new MissingFieldException(name);return (T)field.GetValue(game);}
        void Refresh(){typeof(GameController).GetMethod("RefreshAll",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,null);Canvas.ForceUpdateCanvases();}
        static Vector2 ScreenPoint(RectTransform rect)=>RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center));
        static PointerEventData Pointer(Vector2 position)=>new PointerEventData(EventSystem.current){position=position,pressPosition=position,button=PointerEventData.InputButton.Left};
        GameObject FirstHit(Vector2 point) {var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(Pointer(point),hits);return hits.Count>0?hits[0].gameObject:null;}
        static string Hierarchy(GameObject go){if(!go)return "<no raycast hit>";string path=go.name;for(var t=go.transform.parent;t;t=t.parent)path=t.name+"/"+path;return path;}
        void Click(Button button,string label) {
            Check(button&&button.IsActive()&&button.IsInteractable(),label+" button is active and interactable");Canvas.ForceUpdateCanvases();
            var pointer=Pointer(ScreenPoint((RectTransform)button.transform));var hit=FirstHit(pointer.position);
            var handler=hit?ExecuteEvents.GetEventHandler<IPointerClickHandler>(hit):null;
            Check(handler==button.gameObject,label+" real raycast reaches its button (first hit: "+Hierarchy(hit)+")");
            ExecuteEvents.ExecuteHierarchy(hit,pointer,ExecuteEvents.pointerDownHandler);ExecuteEvents.ExecuteHierarchy(hit,pointer,ExecuteEvents.pointerUpHandler);ExecuteEvents.ExecuteHierarchy(hit,pointer,ExecuteEvents.pointerClickHandler);
        }

        // Resolve serialized fields on dynamic cards instead of using synthetic Cards/Tab names.
        T PrefabRef<T>(GameObject instance,string resource,string cls,string field) where T:Component {
            var data=UiPrefab.Load("UI/Prefabs/"+resource);var nodes=data.Arr("nodes");Dictionary<string,object> script=null;
            foreach(Dictionary<string,object> n in nodes)foreach(Dictionary<string,object> c in n.Arr("c")??new List<object>())if(c.Str("t")==cls){script=c;break;}
            if(script==null)throw new Exception(resource+" missing "+cls);
            int index=script.Obj(field).I("node");string path="";
            while(index>0){var n=(Dictionary<string,object>)nodes[index];path=n.Str("n")+(path.Length==0?"":"/"+path);index=n.I("p");}
            var target=path.Length==0?instance.transform:instance.transform.Find(path);
            if(!target||!target.GetComponent<T>())throw new Exception(instance.name+" missing prefab field "+cls+"."+field+" at "+path);
            return target.GetComponent<T>();
        }

        void CheckGrowthBadge(bool visible,string label) {
            Check(Field<UiView>("top").Ref("TopPanel","mainLevelBg").activeInHierarchy==visible,label+" growth badge is "+(visible?"visible":"hidden"));
        }

        void CheckGrowthLevel(int expected) {
            var level=Field<TMP_Text>("levelTmp");level.ForceMeshUpdate();Canvas.ForceUpdateCanvases();
            Check(level.text==expected.ToString(),"growth label shows "+expected);
            Check(level.textInfo.characterCount==expected.ToString().Length&&level.textInfo.lineCount==1&&!level.isTextOverflowing&&level.textInfo.meshInfo.Any(m=>m.vertexCount>0),"growth "+expected+" renders all digits on one visible line");
            var title=game.Model.TitleMission(expected);Check(title!=null&&Field<TMP_Text>("honorTxt").text==L10n.Translate(title.titleName),"growth "+expected+" uses the configured title");
        }

        IEnumerator DismissGoals() {
            for(int i=0;i<12&&game.Model.PendingGoal!=null;i++) {
                yield return new WaitForSeconds(.6f);var view=Field<UiView>("goalView");
                if(view!=null&&view.Root)Click(view.Ref<Button>("GoalReachPanel","confirmBtn"),"claim growth reward");
                yield return new WaitForSeconds(.5f);
            }
            Check(!game.ModalOpen,"growth reward popup closes after claiming");
        }

        PointerEventData ScrollPointer(ScrollRect scroll,string label) {
            var viewport=scroll.viewport?scroll.viewport:(RectTransform)scroll.transform;var rect=viewport.rect;GameObject last=null;
            foreach(var p in new[]{new Vector2(.5f,.5f),new Vector2(.25f,.35f),new Vector2(.75f,.35f),new Vector2(.25f,.7f),new Vector2(.75f,.7f)}) {
                var point=RectTransformUtility.WorldToScreenPoint(null,viewport.TransformPoint(new Vector3(Mathf.Lerp(rect.xMin,rect.xMax,p.x),Mathf.Lerp(rect.yMin,rect.yMax,p.y))));
                last=FirstHit(point);if(!last||ExecuteEvents.GetEventHandler<IScrollHandler>(last)!=scroll.gameObject)continue;
                var pointer=Pointer(point);pointer.pointerCurrentRaycast=new RaycastResult{gameObject=last};return pointer;
            }
            throw new Exception(label+" viewport raycast is blocked; first hit: "+Hierarchy(last));
        }

        Rect RootBounds(RectTransform rect) {
            var corners=new Vector3[4];rect.GetWorldCorners(corners);
            var min=new Vector2(float.PositiveInfinity,float.PositiveInfinity);var max=new Vector2(float.NegativeInfinity,float.NegativeInfinity);
            foreach(var corner in corners){var p=game.Root.InverseTransformPoint(corner);min=Vector2.Min(min,p);max=Vector2.Max(max,p);}
            return Rect.MinMaxRect(min.x,min.y,max.x,max.y);
        }

        float BottomNavigationTop() {
            var upgrade=Field<UiView>("upgrade");float top=RootBounds(upgrade.Get<RectTransform>("bottomBgImg")).yMax;
            for(int i=1;i<=6;i++)top=Mathf.Max(top,RootBounds(upgrade.Ref<RectTransform>("UpgradePanel","button"+i)).yMax);
            // Entry 0 is the skill sub-tab hint near the list's top, not a bottom navigation overlay.
            foreach(var hint in Field<GameObject[]>("unlockedHints").Skip(1))
                if(hint&&hint.activeInHierarchy)top=Mathf.Max(top,RootBounds((RectTransform)hint.transform).yMax);
            return top;
        }

        IEnumerator VerifyScroll(ScrollRect scroll,string label,float requiredBottomPadding=0) {
            Check(scroll&&scroll.IsActive()&&scroll.vertical&&scroll.content,label+" ScrollRect is active");
            yield return null;Canvas.ForceUpdateCanvases();LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);Canvas.ForceUpdateCanvases();
            var viewport=scroll.viewport?scroll.viewport:(RectTransform)scroll.transform;
            Check(scroll.content.rect.height>viewport.rect.height+1,label+" populated content is taller than its viewport");
            scroll.verticalNormalizedPosition=1;scroll.StopMovement();Canvas.ForceUpdateCanvases();
            var pointer=ScrollPointer(scroll,label);var hit=pointer.pointerCurrentRaycast.gameObject;float before=scroll.content.anchoredPosition.y;
            pointer.scrollDelta=new Vector2(0,-1);var handler=ExecuteEvents.ExecuteHierarchy(hit,pointer,ExecuteEvents.scrollHandler);
            Check(handler==scroll.gameObject&&scroll.content.anchoredPosition.y>before+.5f,label+" real raycast and mouse wheel move content");
            scroll.verticalNormalizedPosition=1;scroll.StopMovement();Canvas.ForceUpdateCanvases();pointer=ScrollPointer(scroll,label);hit=pointer.pointerCurrentRaycast.gameObject;
            ExecuteEvents.ExecuteHierarchy(hit,pointer,ExecuteEvents.initializePotentialDrag);ExecuteEvents.ExecuteHierarchy(hit,pointer,ExecuteEvents.beginDragHandler);
            before=scroll.content.anchoredPosition.y;var start=pointer.position;pointer.position+=Vector2.up*Mathf.Max(24,Screen.height*.12f);pointer.delta=pointer.position-start;
            handler=ExecuteEvents.ExecuteHierarchy(hit,pointer,ExecuteEvents.dragHandler);ExecuteEvents.ExecuteHierarchy(hit,pointer,ExecuteEvents.endDragHandler);scroll.StopMovement();
            Check(handler==scroll.gameObject&&scroll.content.anchoredPosition.y>before+.5f,label+" real pointer drag moves content");
            scroll.verticalNormalizedPosition=1;scroll.StopMovement();Canvas.ForceUpdateCanvases();
            for(int i=0;i<256&&scroll.verticalNormalizedPosition>.001f;i++) {pointer=ScrollPointer(scroll,label);pointer.scrollDelta=new Vector2(0,-1);ExecuteEvents.ExecuteHierarchy(pointer.pointerCurrentRaycast.gameObject,pointer,ExecuteEvents.scrollHandler);}
            // A short list can overscroll after one wheel notch. Let ScrollRect's real
            // elastic spring settle rather than assuming a fixed animation duration.
            scroll.StopMovement();float settleUntil=Time.realtimeSinceStartup+3;int stableFrames=0;
            while(Time.realtimeSinceStartup<settleUntil&&stableFrames<3) {
                yield return null;Canvas.ForceUpdateCanvases();
                bool settled=Mathf.Abs(scroll.verticalNormalizedPosition)<.01f&&Mathf.Abs(RootBounds(scroll.content).yMin-RootBounds(viewport).yMin)<.25f;
                stableFrames=settled?stableFrames+1:0;
            }
            scroll.StopMovement();Canvas.ForceUpdateCanvases();
            Check(stableFrames==3,label+" mouse wheel reaches the settled bottom (normalized="+scroll.verticalNormalizedPosition+", bottom error="+(RootBounds(scroll.content).yMin-RootBounds(viewport).yMin)+")");
            var available=RootBounds(viewport);available.yMin=Mathf.Max(available.yMin,BottomNavigationTop());
            available.xMin=Mathf.Max(available.xMin,game.Root.rect.xMin);available.xMax=Mathf.Min(available.xMax,game.Root.rect.xMax);
            available.yMax=Mathf.Min(available.yMax,game.Root.rect.yMax);
            var cards=scroll.content.Cast<Transform>().Where(x=>x.gameObject.activeSelf).Cast<RectTransform>().ToArray();
            var last=RootBounds(cards.Last());
            foreach(var card in cards.Where(card=>Mathf.Abs(RootBounds(card).yMin-last.yMin)<1)) {
                var bounds=RootBounds(card);
                Check(bounds.xMin>=available.xMin-1&&bounds.xMax<=available.xMax+1&&bounds.yMin>=available.yMin-1&&bounds.yMax<=available.yMax+1,
                    label+" entire final-row card clears viewport and bottom navigation: "+card.name+" (card="+bounds+", usable="+available+")");
            }
            if(requiredBottomPadding>0)Check(last.yMin-available.yMin>=requiredBottomPadding-1,
                label+" final row has visible bottom padding of at least "+(requiredBottomPadding-1)+" design units (actual="+(last.yMin-available.yMin)+")");
        }

        static void CenterInScroll(ScrollRect scroll,RectTransform item) {
            Canvas.ForceUpdateCanvases();var viewport=scroll.viewport?scroll.viewport:(RectTransform)scroll.transform;
            var center=viewport.InverseTransformPoint(item.TransformPoint(item.rect.center));scroll.content.anchoredPosition+=Vector2.up*(viewport.rect.center.y-center.y);scroll.StopMovement();Canvas.ForceUpdateCanvases();
        }

        IEnumerator Capture(string name) {
            yield return null;
            // Render the real uGUI hierarchy into an offscreen framebuffer for hidden Windows QA.
            var canvases=game.GetComponentsInChildren<Canvas>().Where(c=>c.isRootCanvas).ToArray();var camera=FindObjectOfType<Camera>();
            var modes=canvases.Select(c=>c.renderMode).ToArray();var oldCameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();var oldTarget=camera.targetTexture;
            var rt=new RenderTexture(Screen.width,Screen.height,24,RenderTextureFormat.ARGB32);rt.Create();
            camera.targetTexture=rt;foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;}
            Canvas.ForceUpdateCanvases();camera.Render();var old=RenderTexture.active;RenderTexture.active=rt;
            var tex=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);tex.Apply();
            File.WriteAllBytes(Path.Combine(output,name),tex.EncodeToPNG());
            var colors=tex.GetPixels32();Check(colors.Where((c,i)=>i%271==0).Select(c=>((int)c.r<<16)|((int)c.g<<8)|c.b).Distinct().Count()>(Field<GameObject>("originalAd")?4:20),"rendered screenshot contains real UI colors "+name);
            RenderTexture.active=old;camera.targetTexture=oldTarget;for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=oldCameras[i];canvases[i].planeDistance=distances[i];}Destroy(tex);rt.Release();Destroy(rt);yield return null;
        }
        void Fail(Exception e){Debug.LogWarning("REBUILD_QA_FAILED "+e);Complete(false,e.ToString()+"\n"+string.Join("\n",errors));}
        void Complete(bool success,string error) {
            if(finished)return;finished=true;
            var result=new Result{passed=success,error=error,editor=Application.unityVersion,width=Screen.width,height=Screen.height,checks=checks.Count,messages=checks.ToArray()};
            File.WriteAllText(Path.Combine(output,"runtime-check.json"),JsonUtility.ToJson(result,true));
            Debug.Log("REBUILD_RUNTIME_QA "+success+" checks="+checks.Count);Application.Quit(success?0:2);
#if UNITY_EDITOR
            if(Environment.GetCommandLineArgs().Contains("--verify-wake"))UnityEditor.EditorApplication.Exit(success?0:2);
#endif
        }
    }
}

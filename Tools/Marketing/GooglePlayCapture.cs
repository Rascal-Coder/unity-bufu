using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Bufu.Rebuild.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    // Compiled only into the separate capture player by build_capture_player.py.
    public sealed class GooglePlayCapture:MonoBehaviour {
        [Serializable] sealed class CaptureResult {public bool passed;public int width,height;public string error,scope;public string[] files;}
        GameController game;RuntimeVerification qa;string output;float elapsed;bool done;
        readonly List<string> files=new List<string>();readonly List<string> errors=new List<string>();
        const BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
        public static void Begin(){var context=new GameObject("Marketing QA Context",typeof(RuntimeVerification));context.GetComponent<RuntimeVerification>().enabled=false;new GameObject("Marketing Capture",typeof(GooglePlayCapture));}
        void Awake(){DontDestroyOnLoad(gameObject);var args=Environment.GetCommandLineArgs();output=args[Array.IndexOf(args,"--verify-rebuild")+1];Directory.CreateDirectory(output);Application.logMessageReceived+=(m,t,k)=>{if(k==LogType.Error||k==LogType.Exception)errors.Add(m);};}
        void Update(){if((elapsed+=Time.unscaledDeltaTime)>90&&!done)Finish("Capture timed out");}
        void LateUpdate(){if(game&&game.BootFinished&&!done)CleanPointers();}
        object Call(string name,params object[] args)=>typeof(GameController).GetMethod(name,Flags).Invoke(game,args);
        T Field<T>(string name)=>(T)typeof(GameController).GetField(name,Flags).GetValue(game);
        IEnumerator Start(){var stack=new Stack<IEnumerator>();stack.Push(Flow());while(stack.Count>0&&!done){object next=null;bool moved=false;Exception failure=null;try{moved=stack.Peek().MoveNext();if(moved)next=stack.Peek().Current;}catch(Exception ex){failure=ex;}if(failure!=null){Finish(failure.ToString());yield break;}if(!moved){stack.Pop();continue;}if(next is IEnumerator child)stack.Push(child);else yield return next;}}
        void CleanPointers(){
            foreach(var hint in Field<GameObject[]>("unlockedHints"))if(hint)hint.SetActive(false);
            foreach(var rect in game.GetComponentsInChildren<RectTransform>(true))if(rect.name.ToLowerInvariant().Contains("finger"))rect.gameObject.SetActive(false);
        }
        IEnumerator Shot(string name){CleanPointers();Canvas.ForceUpdateCanvases();yield return null;CleanPointers();
            var canvases=game.GetComponentsInChildren<Canvas>().Where(c=>c.isRootCanvas).ToArray();var camera=FindObjectOfType<Camera>();
            var modes=canvases.Select(c=>c.renderMode).ToArray();var cameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();var oldTarget=camera.targetTexture;
            var rt=new RenderTexture(1080,1920,24,RenderTextureFormat.ARGB32);rt.Create();camera.targetTexture=rt;
            foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;}
            Canvas.ForceUpdateCanvases();camera.Render();SaveFrame(rt,name,false);
            camera.targetTexture=oldTarget;for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=cameras[i];canvases[i].planeDistance=distances[i];}rt.Release();Destroy(rt);files.Add(name);yield return null;
        }
        void SaveFrame(RenderTexture rt,string name,bool alpha){var old=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(rt.width,rt.height,alpha?TextureFormat.RGBA32:TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(output,name),tex.EncodeToPNG());RenderTexture.active=old;Destroy(tex);}
        IEnumerator NativePortraits(){
            var canvases=game.GetComponentsInChildren<Canvas>().Where(c=>c.isRootCanvas&&c.enabled).ToArray();foreach(var c in canvases)c.enabled=false;
            var cameraObject=new GameObject("Native art camera",typeof(Camera));var camera=cameraObject.GetComponent<Camera>();camera.orthographic=true;camera.orthographicSize=7.2f;camera.transform.position=new Vector3(0,0,-10);camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;camera.cullingMask=1<<30;
            var canvasObject=new GameObject("Native art stage",typeof(RectTransform),typeof(Canvas));canvasObject.layer=30;var canvas=canvasObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=camera;var root=(RectTransform)canvasObject.transform;root.sizeDelta=new Vector2(1440,1440);root.localScale=Vector3.one*.01f;
            var rt=new RenderTexture(1024,1024,24,RenderTextureFormat.ARGB32);rt.Create();camera.targetTexture=rt;
            for(int id=1;id<=10;id++){
                var rect=GameController.NewRect("Native helper "+id,root);rect.gameObject.layer=30;rect.sizeDelta=new Vector2(1440,1440);var visual=VatGraphic.AddTo(rect.gameObject);visual.Load(HelperSkills.RoleVat(id,game.Model.HelperAwake(id)));
                var size=visual.Data.max-visual.Data.min;visual.PixelsPerUnit=1150/Mathf.Max(size.x,size.y);rect.anchoredPosition=-(Vector2)(visual.Data.min+visual.Data.max)*.5f*visual.PixelsPerUnit;
                visual.Pose(visual.Has("throw_front")?"throw_front":"idle_front",.58f);Canvas.ForceUpdateCanvases();camera.Render();SaveFrame(rt,"native-helper-"+id+".png",true);Destroy(rect.gameObject);yield return null;
            }
            rt.Release();Destroy(rt);Destroy(canvasObject);Destroy(cameraObject);foreach(var c in canvases)c.enabled=true;yield return null;
        }
        IEnumerator Flow(){
            while(!(game=FindObjectOfType<GameController>()))yield return null;
            qa=FindObjectOfType<RuntimeVerification>();if(!qa)throw new Exception("Missing isolated capture context");qa.StopAllCoroutines();qa.enabled=false;
            while(!game.BootFinished||LaunchScreen.Current)yield return null;
            typeof(RuntimeVerification).GetField("game",Flags).SetValue(qa,game);
            UnityEngine.Random.InitState(20261021);
            var model=game.Model;model.State.mission=model.WorldMissions.Length;model.State.pendingGoals.Clear();model.State.gold=4.8e15;model.Data.diamonds=550;model.Data.diceUnlocked=true;
            model.State.quantity[1]=15;model.State.talents[102]=1;model.State.talents[107]=1;model.State.talents[108]=3;model.State.talents[303]=5;
            model.State.boosts[4]=1;model.State.boosts[5]=1;model.State.boosts[9]=20;
            for(int q=1;q<=4;q++){int id=q+1;model.State.evolution[id]=4;model.State.evolvedAt[id]=101;model.State.quantity[id]=q==4?118:124;model.State.boosts[q==4?14:q]=20;}
            model.State.helpers.Clear();
            foreach(var role in model.helperRoles){int id=role.id,level=role.upgradeLevelMax;model.Data.globalHelperLevels[id]=level;model.Data.shards[id+100]=20;model.State.helpers.Add(new HelperState{id=id,level=level,count=1,max=4});}
            Call("SelectTab",1);game.RebuildActors();yield return new WaitForSecondsRealtime(.5f);
            game.FlipAll();yield return new WaitForSecondsRealtime(.85f);yield return Shot("01-flip-coins.png");
            yield return new WaitForSecondsRealtime(.5f);foreach(var helper in game.HelperActors.Where(h=>h.RoleId>=5))HelperSkills.Cast(helper,game.Coins.FirstOrDefault(c=>!c.Busy));yield return new WaitForSecondsRealtime(.35f);yield return Shot("02-helper-action.png");
            yield return new WaitForSecondsRealtime(.5f);game.FlipAll();yield return new WaitForSecondsRealtime(.85f);yield return Shot("03-coin-upgrades.png");
            Call("SelectTab",2);yield return null;yield return null;var facility=Field<UiView>("facilityView");var content=facility.Ref<RectTransform>("HelperPanel","contentParent");var helperScroll=content.GetComponentInParent<ScrollRect>();helperScroll.verticalNormalizedPosition=0;helperScroll.StopMovement();Canvas.ForceUpdateCanvases();yield return null;yield return Shot("02-helper-team.png");
            Call("SelectTab",6);yield return null;
            var world=Field<UiView>("worldView");var scroll=world.Root.GetComponentInChildren<ScrollRect>();var snap=scroll.GetComponent<WorldPageSnap>();
            snap.ScrollToIndex(2,.14f);yield return new WaitForSecondsRealtime(.8f);yield return Shot("04-coin-world.png");
            snap.ScrollToIndex(1,.14f);yield return new WaitForSecondsRealtime(.8f);yield return Shot("05-dice-world.png");
            yield return NativePortraits();
            Finish(errors.Count==0?null:string.Join("\n",errors));
        }
        void Finish(string error){if(done)return;done=true;File.WriteAllText(Path.Combine(output,"capture-result.json"),JsonUtility.ToJson(new CaptureResult{passed=error==null,error=error,width=1080,height=1920,files=files.ToArray(),scope="Native 1080x1920 offscreen renders of actual advanced progression in an isolated verification save: maximum-tier coins, awakened helpers, configured talents and genuine settlement numbers and skills. Native helper art is separately rendered from the game's VAT files. Normal delivery not changed."},true));Application.Quit(error==null?0:1);}
    }
}

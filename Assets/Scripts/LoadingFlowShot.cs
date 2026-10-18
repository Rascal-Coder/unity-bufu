using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    /// <summary>Command-line playthrough. Writes loading, film and game screenshots for visual comparison.</summary>
    public sealed class LoadingFlowShot:MonoBehaviour {
        string output;float elapsed;int heartbeat;bool shotLoading,shotFilm,shotGame,quitting,guide;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Prepare() {
            if(!Enabled())return;
            PlayerPrefs.DeleteKey("IntroVideoPlayed");
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"--guide-shot")>=0)PlayerPrefs.DeleteKey("guide_step");
            PlayerPrefs.Save();
            Screen.SetResolution(540,960,false);
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot() {
            if(!Enabled()||FindObjectOfType<LoadingFlowShot>())return;
            var go=new GameObject("Loading Flow Shot");DontDestroyOnLoad(go);go.AddComponent<LoadingFlowShot>();
        }
        static bool Enabled(){return Array.IndexOf(Environment.GetCommandLineArgs(),"--loading-shot")>=0;}
        void Awake() {
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"--loading-shot");
            output=at>=0&&at+1<args.Length&&!args[at+1].StartsWith("-")?args[at+1]:Path.Combine("Build","LoadingShots");
            Directory.CreateDirectory(output);guide=Array.IndexOf(args,"--guide-shot")>=0;
            File.WriteAllText(Path.Combine(output,"log.txt"),"started "+DateTime.Now.ToString("HH:mm:ss")+"\n");
        }
        void Update() {
            if(quitting)return;
            elapsed+=Time.unscaledDeltaTime;
            int second=Mathf.FloorToInt(elapsed);
            if(second!=heartbeat&&second%5==0){heartbeat=second;Note("t="+second+" loading="+(GameObject.Find("Loading")?"yes":"no")+" film="+(GameObject.Find("IntroVideoContainer")?"yes":"no")+" boot="+(GameController.Active!=null&&GameController.Active.BootFinished)+" launch="+(LaunchScreen.Current!=null));}
            if(!shotLoading&&elapsed>0.8f){shotLoading=true;StartCoroutine(Grab("01-loading.png"));}
            var film=GameObject.Find("IntroVideoContainer");
            var skipButton=GameObject.Find("Skip");
            if(!shotFilm&&film&&film.activeInHierarchy&&skipButton&&skipButton.activeInHierarchy){shotFilm=true;StartCoroutine(Grab("02-film.png"));}
            if(!shotGame&&GameController.Active!=null&&GameController.Active.BootFinished&&LaunchScreen.Current==null&&elapsed>2f){shotGame=true;StartCoroutine(Grab("03-game.png",true));}
            if(elapsed>70f)Quit(3,"timed out");
        }
        /// <summary>Raycast-catching guide graphics at an offset (canvas units) from the hole centre. Same-canvas geometry, so the window size does not matter.</summary>
        static string GuideBlockers(float dx,float dy) {
            var layer=GameObject.Find("Guide");var hole=GameObject.Find("Hole");if(!layer||!hole)return "no guide";
            var point=hole.transform.TransformPoint(new Vector3(dx,dy,0));
            var names=layer.GetComponentsInChildren<UnityEngine.UI.Graphic>().Where(g=>g.raycastTarget&&g.isActiveAndEnabled&&g.rectTransform.rect.Contains(g.rectTransform.InverseTransformPoint(point))).Select(g=>g.name).ToArray();
            return names.Length==0?"none":string.Join(",",names);
        }
        void Note(string line){File.AppendAllText(Path.Combine(output,"log.txt"),line+"\n");Debug.Log("LOADING_SHOT "+line);}
        IEnumerator Grab(string name,bool quit=false) {
            yield return null;
            var camera=FindObjectOfType<Camera>();
            if(!camera){Note("no camera for "+name);if(quit)Quit(4,name);yield break;}
            var canvases=FindObjectsOfType<Canvas>().Where(c=>c.gameObject.activeInHierarchy).ToArray();
            var modes=canvases.Select(c=>c.renderMode).ToArray();var oldCameras=canvases.Select(c=>c.worldCamera).ToArray();var distances=canvases.Select(c=>c.planeDistance).ToArray();
            var oldTarget=camera.targetTexture;
            var rt=new RenderTexture(540,960,24,RenderTextureFormat.ARGB32);rt.Create();
            camera.targetTexture=rt;
            foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;}
            var covers=FindObjectsOfType<Image>(true).Where(i=>i&&(i.name=="Outer"||i.name=="Left Bar"||i.name=="Right Bar")).ToArray();
            foreach(var cover in covers)cover.enabled=false;
            Canvas.ForceUpdateCanvases();camera.Render();
            var previous=RenderTexture.active;RenderTexture.active=rt;
            var tex=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);tex.Apply();
            File.WriteAllBytes(Path.Combine(output,name),tex.EncodeToPNG());
            var progress=GameObject.Find("Text");
            Note(name+" canvases="+string.Join(",",canvases.Select(c=>c.name))+" progress="+(progress?progress.GetComponent<TMPro.TMP_Text>()?.text:""));
            foreach(var cover in covers)if(cover)cover.enabled=true;
            RenderTexture.active=previous;camera.targetTexture=oldTarget;
            for(int i=0;i<canvases.Length;i++){canvases[i].renderMode=modes[i];canvases[i].worldCamera=oldCameras[i];canvases[i].planeDistance=distances[i];}
            Destroy(tex);rt.Release();Destroy(rt);
            if(quit&&guide){StartCoroutine(GuideFlow());yield break;}
            if(quit&&Array.IndexOf(Environment.GetCommandLineArgs(),"--tab-shot")>=0){StartCoroutine(TabFlow());yield break;}
            if(quit)Quit(shotLoading&&shotGame?0:5,name);
        }
        /// <summary>Opens the Helpers tab and grabs a few frames while the helpers keep tossing coins.</summary>
        IEnumerator TabFlow() {
            var game=GameController.Active;var closeTop=typeof(GameController).GetMethod("CloseTopPopup",BindingFlags.NonPublic|BindingFlags.Instance);
            for(int i=0;i<6&&game.ModalOpen;i++){closeTop.Invoke(game,null);yield return null;}
            game.OpenTab(2);yield return new WaitForSecondsRealtime(.3f);
            for(int i=0;i<6&&game.ModalOpen;i++){closeTop.Invoke(game,null);yield return null;}
            Note("tab field="+typeof(GameController).GetField("tab",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(game)+" modal="+game.ModalOpen);
            var field=GameObject.Find("Background").GetComponent<Canvas>();double gold=game.Model.State.gold;
            for(int i=0;i<4;i++){yield return new WaitForSecondsRealtime(.7f);yield return Grab("10-helpers-"+i+".png");}
            Note("helpers="+game.Model.Helpers+" coins="+game.Coins.Count+" fieldDrawn="+field.enabled+" goldGained="+(game.Model.State.gold>gold));
            game.OpenTab(1);yield return new WaitForSecondsRealtime(.7f);yield return Grab("11-items.png");
            Note("back on items fieldDrawn="+field.enabled);
            Quit(field.enabled?0:6,"tab");
        }
        /// <summary>Plays the first-session guide on a fresh save: toss (tails), toss again (heads), hire at 25 gold.</summary>
        IEnumerator GuideFlow() {
            var game=GameController.Active;var coinField=typeof(GameController).GetField("guideCoin",BindingFlags.NonPublic|BindingFlags.Instance);
            CoinActor Guided()=>(CoinActor)coinField.GetValue(game);
            IEnumerator WaitFor(Func<bool> done,float limit){float t=0;while(!done()&&t<limit){t+=Time.unscaledDeltaTime;yield return null;}}
            yield return WaitFor(()=>Guided()!=null&&GameObject.Find("Finger"),8);
            yield return new WaitForSecondsRealtime(.6f);yield return Grab("04-guide-toss.png");
            Note("coin hole blockers="+GuideBlockers(0,0)+" outside blockers="+GuideBlockers(400,0));
            var coin=Guided();Note("guide coin "+(coin?coin.name:"none"));if(!coin){Quit(6,"no guide coin");yield break;}
            coin.Throw(true);
            yield return WaitFor(()=>!coin.Busy,5);yield return new WaitForSecondsRealtime(.6f);yield return Grab("05-guide-tails.png");
            coin.Throw(true);
            yield return WaitFor(()=>!coin.Busy,5);yield return new WaitForSecondsRealtime(.6f);yield return Grab("06-guide-heads.png");
            var close=GameObject.Find("Guide")?.GetComponentsInChildren<UnityEngine.UI.Button>().FirstOrDefault(b=>b.name=="mask");
            Note("heads dialog "+(close?"yes":"no"));close?.onClick.Invoke();
            game.Model.State.gold=Math.Max(game.Model.State.gold,30);
            yield return WaitFor(()=>GameObject.Find("Finger"),5);yield return new WaitForSecondsRealtime(.6f);yield return Grab("07-guide-hire.png");
            Note("hire hole blockers="+GuideBlockers(0,0)+" outside blockers="+GuideBlockers(0,400));
            var hire=GameObject.FindObjectsOfType<UnityEngine.UI.Button>().FirstOrDefault(b=>b.name=="upgradeBtn"&&b.GetComponentsInParent<Transform>().Any(t=>t.name=="item0_1"));
            Note("hire button "+(hire?"found":"missing"));hire?.onClick.Invoke();
            yield return new WaitForSecondsRealtime(.5f);
            Note("helpers="+game.Model.Helpers+" gold="+game.Model.State.gold+" guide visible="+(GameObject.Find("Finger")!=null));
            double diamonds=game.Model.Data.diamonds;
            yield return WaitFor(()=>GameObject.Find("fortunePanel"),8);
            yield return new WaitForSecondsRealtime(.5f);yield return Grab("08-fortune-pop.png");
            yield return new WaitForSecondsRealtime(1.6f);yield return Grab("09-fortune.png");
            Note("fortune "+(GameObject.Find("fortunePanel")?"shown":"missing")+" diamonds "+diamonds+" -> "+game.Model.Data.diamonds);
            var start=GameObject.Find("fortunePanel")?.GetComponentsInChildren<UnityEngine.UI.Button>().FirstOrDefault(b=>b.name=="normalGetBtn");
            start?.onClick.Invoke();yield return null;
            Note("after start: panel "+(GameObject.Find("fortunePanel")?"still open":"closed")+" modal="+game.ModalOpen);
            Quit(0,"guide");
        }
        void Quit(int code,string why){if(quitting)return;quitting=true;Note("exit "+code+" "+why);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.Exit(code);
#else
            Application.Quit(code);
#endif
        }
    }
}

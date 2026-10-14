using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace Bufu.Rebuild {
    // Activated only by an explicit QA command-line flag; never touches the player's save.
    public sealed class RuntimeVerification:MonoBehaviour {
        [Serializable] public sealed class Result {public bool passed;public string error,editor;public int width,height,checks;public string[] messages;}
        readonly List<string> checks=new List<string>();readonly List<string> errors=new List<string>();
        string output;GameController game;float timeout;bool finished;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init(){if(GameController.VerificationMode)new GameObject("Runtime QA",typeof(RuntimeVerification));}
        void Awake() {var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"--verify-rebuild");output=at>=0&&at+1<args.Length?args[at+1]:Application.persistentDataPath;Directory.CreateDirectory(output);Application.logMessageReceived+=Log;}
        void Log(string message,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message+"\n"+trace);}
        void Update(){timeout+=Time.unscaledDeltaTime;if(timeout>45&&!finished)Complete(false,"Runtime QA timed out\n"+string.Join("\n",errors));}
        void Check(bool condition,string text) {if(!condition)throw new Exception(text);checks.Add(text);}
        IEnumerator Start() {
            yield return null;yield return new WaitForSeconds(.8f);game=FindObjectOfType<GameController>();
            try {Check(game!=null&&game.Model!=null,"runtime initialization");Check(game.Coins.Count==3,"new game renders 3 real copper VAT coins");Check(Resources.Load<AudioClip>("Audio/coinup")!=null,"runtime coin audio loaded");}
            catch(Exception e){Fail(e);yield break;}
            yield return Capture("01-new-game.png");
            try {
                Canvas.ForceUpdateCanvases();var coin=game.Coins[0];var p=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,coin.Rect.position)};
                var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(p,hits);
                Check(hits.Count>0&&hits[0].gameObject.GetComponent<CoinActor>()!=null,"actual UI raycast reaches a coin");
                ExecuteEvents.Execute(hits[0].gameObject,p,ExecuteEvents.pointerDownHandler);
                Check(hits[0].gameObject.GetComponent<CoinActor>().Busy,"pointer input starts coin animation");
            }catch(Exception e){Fail(e);yield break;}
            yield return new WaitForSeconds(.75f);
            UnityEngine.Random.InitState(20261014);
            for(int i=0;i<10;i++){game.FlipAll();yield return new WaitForSeconds(.75f);}
            try {
                Check(game.Model.State.gold>=25,"manual animation completion settles actual income");
                var card=GameObject.Find("Cards").transform.Find("帮手/Buy").GetComponent<Button>();Check(card.interactable,"affordable helper button enabled");card.onClick.Invoke();
                Check(game.Model.Helpers==1&&game.Field.GetComponentsInChildren<HelperActor>().Length==1,"helper purchase creates animated actor");
                if(game.Model.State.gold>=40)card.onClick.Invoke();
            }catch(Exception e){Fail(e);yield break;}
            double before=game.Model.State.lifetime;yield return new WaitForSeconds(7);
            try {Check(game.Model.State.lifetime>before,"automatic helpers produce income without manual input");
                GameObject.Find("Tab 强化").GetComponent<Button>().onClick.Invoke();
                var copper=GameObject.Find("Cards").transform.Find("铜币上涨/Buy").GetComponent<Button>();
                if(game.Model.State.gold>=30){copper.onClick.Invoke();Check(game.Model.State.boosts[1]==1,"UI value upgrade changes economy");}
            }catch(Exception e){Fail(e);yield break;}
            yield return Capture("02-helpers-upgrades.png");
            try {
                string json=JsonUtility.ToJson(game.Model.Data,true);File.WriteAllText(Path.Combine(output,"test-save.json"),json);
                var restored=new GameModel(JsonUtility.FromJson<SaveData>(File.ReadAllText(Path.Combine(output,"test-save.json"))));
                Check(restored.Helpers==game.Model.Helpers&&restored.State.gold==game.Model.State.gold,"save to disk and restore preserves progress");
                game.Model.Data.worlds[0].mission=10;GameObject.Find("Tab 世界").GetComponent<Button>().onClick.Invoke();
                var world=GameObject.Find("World 2").GetComponent<Button>();world.onClick.Invoke();
                Check(game.Model.Data.world==2&&game.Coins.Count==3,"world UI switches to original dice assets");
            }catch(Exception e){Fail(e);yield break;}
            game.FlipAll();yield return new WaitForSeconds(.8f);yield return Capture("03-dice-world.png");
            try {Check(game.Model.State.gold>0,"dice animation completes and settles face rewards");Check(errors.Count==0,"no runtime errors or exceptions");Complete(true,null);}catch(Exception e){Fail(e);}
        }
        IEnumerator Capture(string name) {
            yield return null;
            // A hidden Windows swap chain may return black. Render the real uGUI hierarchy
            // through the scene camera into an offscreen framebuffer for deterministic QA.
            var canvas=game.GetComponentInChildren<Canvas>();var camera=FindObjectOfType<Camera>();
            var mode=canvas.renderMode;var oldCamera=canvas.worldCamera;var oldTarget=camera.targetTexture;
            var rt=new RenderTexture(Screen.width,Screen.height,24,RenderTextureFormat.ARGB32);rt.Create();
            camera.targetTexture=rt;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
            Canvas.ForceUpdateCanvases();camera.Render();var old=RenderTexture.active;RenderTexture.active=rt;
            var tex=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);tex.Apply();
            File.WriteAllBytes(Path.Combine(output,name),tex.EncodeToPNG());
            var colors=tex.GetPixels32();Check(colors.Where((c,i)=>i%271==0).Select(c=>((int)c.r<<16)|((int)c.g<<8)|c.b).Distinct().Count()>20,"rendered screenshot contains real UI colors "+name);
            RenderTexture.active=old;camera.targetTexture=oldTarget;canvas.renderMode=mode;canvas.worldCamera=oldCamera;Destroy(tex);rt.Release();Destroy(rt);yield return null;
        }
        void Fail(Exception e){Debug.LogWarning("REBUILD_QA_FAILED "+e);Complete(false,e.ToString()+"\n"+string.Join("\n",errors));}
        void Complete(bool success,string error) {
            if(finished)return;finished=true;
            var result=new Result{passed=success,error=error,editor=Application.unityVersion,width=Screen.width,height=Screen.height,checks=checks.Count,messages=checks.ToArray()};
            File.WriteAllText(Path.Combine(output,"runtime-check.json"),JsonUtility.ToJson(result,true));
            Debug.Log("REBUILD_RUNTIME_QA "+success+" checks="+checks.Count);Application.Quit(success?0:2);
        }
    }
}

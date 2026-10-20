using System.Collections;
using Bufu.Rebuild.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    public sealed partial class RuntimeVerification {
        void Call(string name,params object[] args) {
            var method=typeof(GameController).GetMethod(name,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            var parameters=method.GetParameters();var supplied=new object[parameters.Length];
            for(int i=0;i<supplied.Length;i++)supplied[i]=i<args.Length?args[i]:parameters[i].DefaultValue;
            method.Invoke(game,supplied);
        }
        Transform RebirthNode(Transform root,int index) {
            var nodes=UiPrefab.Load("UI/Prefabs/Talent/talentRebornTipsItem").Arr("nodes");string path="";
            while(index>0){var node=(System.Collections.Generic.Dictionary<string,object>)nodes[index];path=node.Str("n")+(path.Length==0?"":"/"+path);index=node.I("p");}
            return root.Find(path);
        }
        IEnumerator VerifyTransition() {
            // Drive the actual confirmation buttons; the isolated QA model never saves.
            var model=game.Model;
            for(int world=1;world<=2;world++) {
                if(world==2){model.State.mission=90;Check(model.SwitchWorld(2),"unlock dice world for rebirth transition");game.RebuildActors();}
                model.State.mission=90;model.State.pendingGoals.Clear();model.State.gold=model.RebornTable[1].talentName;Refresh();
                int points=model.RebornPoints(),before=model.State.points,rebirth=model.State.rebirth;
                Check(points>0,"world "+world+" reaches a real rebirth target");
                Call("SelectTab",3);var talent=Field<UiView>("talentView");
                yield return null;Click(talent.Ref<Button>("TalentCoin","rebornBtn"),"open rebirth panel");
                var host=talent.Ref<RectTransform>("TalentCoin","rebornPanel");
                var popup=host.GetChild(host.childCount-1);
                yield return Capture("0"+world+"-before-rebirth.png");
                Click(RebirthNode(popup,20).GetComponent<Button>(),"open rebirth confirmation");
                yield return null;yield return Capture("0"+world+"-confirm-rebirth.png");
                var confirm=RebirthNode(popup,32).GetComponent<Button>();Click(confirm,"confirm rebirth");
                Check(game.CutscenePlaying&&game.ModalOpen,"world "+world+" successful rebirth starts blocking original cutscene");
                Check(model.State.points==before+points&&model.State.rebirth==rebirth+1,"rebirth grants points exactly once before animation");
                var transition=Field<GameObject>("cutsceneTransition");var graphic=transition.GetComponentInChildren<SpineGraphic>();
                Check(graphic&&Mathf.Abs(graphic.AnimationDuration-2.3333f)<.01f,"original guochang timeline lasts 2.3333 seconds");
                Call("PlayCutsceneTransition");Check(Field<GameObject>("cutsceneTransition")==transition,"repeated transition request reuses the active cutscene");
                Call("CloseTopPopup");Check(game.CutscenePlaying,"Escape cannot dismiss rebirth transition");
                confirm.onClick.Invoke();Check(model.State.points==before+points&&model.State.rebirth==rebirth+1,"repeated confirmation does not grant another rebirth");
                yield return new WaitForSecondsRealtime(.35f);Canvas.ForceUpdateCanvases();
                var mesh=graphic.canvasRenderer.GetMesh();Check(mesh&&mesh.vertexCount>100,"original roller attachments render real geometry");
                var hit=FirstHit(ScreenPoint((RectTransform)transition.transform));
                Check(hit==transition,"transition blocks real UI raycasts");
                yield return Capture("0"+world+"-roller-enter.png");
                yield return new WaitForSecondsRealtime(.55f);yield return Capture("0"+world+"-roller-cover.png");
                yield return new WaitForSecondsRealtime(1.6f);yield return null;
                Check(!game.CutscenePlaying&&!transition,"one-shot transition destroys its renderers and releases input");
                Check(Field<int>("tab")==1,"rebirth returns to the items page");
                yield return Capture("0"+world+"-rebirth-complete.png");
            }
            Check(errors.Count==0,"rebirth cutscene has no runtime errors");Complete(true,null);
        }
    }
}

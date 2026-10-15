using System;
using System.Collections;
using System.Linq;
using Bufu.Rebuild.UI;
using Coffee.UIExtensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    public sealed partial class RuntimeVerification {
        IEnumerator VerifySupport() {
            var model=game.Model;var top=Field<UiView>("top");var down=Field<UiView>("promotionView");
            string originalTitle=top.Ref<TMP_Text>("TopPanel","honorTxt").text;
            Time.timeScale=0;model.AddEarnedGold(60,Time.unscaledTime);yield return null;yield return null;
            var goal=Field<UiView>("goalView");
            Check(goal!=null&&goal.Root,"actual growth income opens native newspaper panel");
            Check(goal.Rect.anchoredPosition.y>0&&goal.Rect.localScale.x>1,"entire native panel starts scaled and above screen");
            Check(goal.Node("GameObject").transform.localScale.x==1.2f,"original newspaper body retains native inner scale");
            Check(goal.Ref("GoalReachPanel","confirmBtn").activeInHierarchy&&goal.Ref("GoalReachPanel","adBtn").activeInHierarchy,"both original reward buttons stay visible during entry");
            double before=model.State.gold;
            goal.Ref<Button>("GoalReachPanel","confirmBtn").onClick.Invoke();
            Check(model.State.gold==before,"entry animation blocks early claim without hiding buttons");
            Time.timeScale=1;yield return new WaitForSeconds(.15f);yield return Capture("01-honor-newspaper-entering.png");
            yield return new WaitForSeconds(.6f);
            var rain=goal.Ref<ParticleSystem>("GoalReachPanel","coinRainEffect");
            // Coffee drives native Simulate itself, which deliberately leaves isPlaying false.
            Check(rain&&rain.time>0&&rain.particleCount>0,"original native paper rain simulates after entry completes (time="+(rain?rain.time:0)+", particles="+(rain?rain.particleCount:0)+")");
            Check(rain.noise.enabled&&rain.textureSheetAnimation.enabled,"paper rain retains original noise and frame animation modules");
            Check(goal.Root.GetComponentInChildren<UIParticle>()!=null,"original Coffee UIParticle renders native simulation into uGUI");
            Check(top.Ref<TMP_Text>("TopPanel","honorTxt").text==originalTitle,"top honor waits until newspaper exit completes");
            yield return Capture("02-honor-newspaper-with-rain.png");
            Check(ParticleMeshVertices(goal)>0,"paper rain has actual generated uGUI mesh vertices");
            Time.timeScale=0;yield return null;
            Click(goal.Ref<Button>("GoalReachPanel","confirmBtn"),"claim native newspaper reward");
            Check(model.State.gold==before+150&&!rain.gameObject.activeSelf,"claim grants original reward once and stops rain");
            Time.timeScale=.1f;yield return new WaitForSeconds(.02f);
            Check(goal.Rect.anchoredPosition.y>0&&goal.Rect.localScale.x>1,"whole newspaper exit begins with original InBack anticipation");
            Check(top.Ref<TMP_Text>("TopPanel","honorTxt").text==originalTitle,"honor remains unchanged while exit is in progress");yield return Capture("03-honor-newspaper-exiting.png");
            Time.timeScale=1;yield return new WaitForSeconds(.5f);
            // A slow frame can resume this iterator before the exit tween's callback.
            float exitDeadline=Time.realtimeSinceStartup+2;
            while(Field<UiView>("goalView")!=null&&Time.realtimeSinceStartup<exitDeadline)yield return null;
            Check(Field<UiView>("goalView")==null&&top.Ref<TMP_Text>("TopPanel","honorTxt").text==model.TitleMission(2).titleName,"honor updates when native newspaper finishes closing");

            GameObject node(string field)=>down.Ref("DownItem",field);
            model.State.mission=3;model.State.gold=0;model.State.pendingGoals.Clear();Refresh();yield return null;yield return null;
            Check(node("instantMoneyButton").activeInHierarchy&&node("openBtn").activeInHierarchy&&!node("foldBtn").activeInHierarchy,"default support shows millionaire portrait plus more");
            Click(node("openBtn").GetComponent<Button>(),"expand support before master unlocks");yield return null;
            Check(node("instantMoneyButton").activeInHierarchy&&node("foldBtn").activeInHierarchy,"expanding support keeps millionaire visible before master unlocks");
            Click(node("foldBtn").GetComponent<Button>(),"restore default support before master unlocks");yield return null;
            Refresh();yield return null;
            Check(node("instantMoneyButton").activeInHierarchy&&node("openBtn").activeInHierarchy&&!node("foldBtn").activeInHierarchy,"support refresh preserves millionaire portrait plus more");
            model.State.mission=4;Refresh();yield return null;yield return null;
            Check(node("instantMoneyButton").activeInHierarchy&&!node("inspireButton").activeInHierarchy&&!node("earnMoneyButton").activeInHierarchy&&node("openBtn").activeInHierarchy,"unlocking masters keeps default millionaire portrait plus more");
            yield return Capture("04a-millionaire-default.png");
            Click(node("openBtn").GetComponent<Button>(),"show all three support entries");yield return null;
            foreach(string field in new[]{"instantMoneyButton","inspireButton","earnMoneyButton"}) {
                var button=node(field).GetComponent<Button>();Check(button.IsActive(),"native right-side support entry visible: "+field);
                var bounds=RootBounds((RectTransform)button.transform);Check(bounds.xMin>=game.Root.rect.xMin&&bounds.xMax<=game.Root.rect.xMax,"support entry fits within safe portrait width: "+field);
            }
            yield return Capture("04-three-original-support-entries.png");
            Click(node("foldBtn").GetComponent<Button>(),"collapse original support column");yield return null;
            Check(node("instantMoneyButton").activeInHierarchy&&!node("inspireButton").activeInHierarchy&&!node("earnMoneyButton").activeInHierarchy&&node("openBtn").activeInHierarchy&&!node("foldBtn").activeInHierarchy,"collapse restores default millionaire portrait plus more");
            yield return Capture("04b-millionaire-restored.png");
            Click(node("openBtn").GetComponent<Button>(),"expand original support column");yield return null;
            Click(node("instantMoneyButton").GetComponent<Button>(),"open native millionaire dialog");yield return null;
            var dialog=Field<UiView>("supportDialog");
            Check(dialog.Resource.EndsWith("Ad/AdGetMoney")&&dialog.Ref<TMP_Text>("CoinSlotSupportDialog","coinText").text=="500","native millionaire popup displays original minimum reward");
            yield return Capture("05-millionaire-native-dialog.png");
            Click(dialog.Ref<Button>("CoinSlotSupportDialog","closeButton"),"cancel millionaire dialog");yield return null;
            Check(model.State.gold==0&&model.SupportRemaining(SupportKind.Money)==0,"cancelling support grants no money and starts no cooldown");
            Click(node("inspireButton").GetComponent<Button>(),"open native inspire dialog");yield return null;
            dialog=Field<UiView>("supportDialog");Check(dialog.Resource.EndsWith("Ad/AdSpeed"),"inspire uses original support popup");
            yield return Capture("06-inspire-native-dialog.png");
            Click(dialog.Ref<Button>("CoinSlotSupportDialog","getButton"),"start original offline ad");yield return null;
            var ad=Field<GameObject>("originalAd");Check(ad&&!ad.transform.Find("CloseButton").gameObject.activeSelf&&model.SupportSpeedMultiplier==1,"offline ad begins without an early reward");
            yield return new WaitForSecondsRealtime(2.2f);
            Check(ad.transform.Find("CloseButton").gameObject.activeSelf&&model.SupportSpeedMultiplier==1,"two-second ad completion waits for explicit close before reward");
            Click(ad.transform.Find("CloseButton").GetComponent<Button>(),"close completed native offline ad");yield return null;yield return null;
            Check(model.SupportSpeedMultiplier==2&&model.SupportRemaining(SupportKind.Speed)>295,"closing finished ad grants original speed buff and cooldown");
            Check(Field<UiView>("supportDialog")==null,"successful support closes original popup");
            var effects=Field<UiView[]>("supportEffects");
            yield return new WaitForSeconds(.25f);
            Check(effects[2].Root.activeInHierarchy&&effects[2].Root.GetComponentsInChildren<ParticleSystem>(true).Any(x=>x.time>0&&x.particleCount>0),"native inspire particle effect simulates on current scene");
            yield return Capture("07-inspire-native-effect-and-timer.png");
            Check(ParticleMeshVertices(effects[2])>0,"inspire effect has actual generated uGUI mesh vertices");
            Click(node("inspireButton").GetComponent<Button>(),"reopen cooling inspire entry");yield return null;dialog=Field<UiView>("supportDialog");
            Check(!dialog.Ref<Button>("CoinSlotSupportDialog","getButton").interactable&&dialog.Ref<TMP_Text>("CoinSlotSupportDialog","bianlanText").text.StartsWith("过"),"cooling popup disables claim and displays original wait message");
            Click(dialog.Ref<Button>("CoinSlotSupportDialog","closeButton"),"close cooling popup");yield return null;
            yield return new WaitForSeconds(3.1f);Check(!Field<ParticleSystem[]>("supportEmitters")[2].gameObject.activeSelf,"original support effect hides after three scaled seconds");
            foreach(var kind in new[]{SupportKind.Money,SupportKind.Income}) {
                int index=(int)kind;
                Click(node(supportButtonsForVerification[index]).GetComponent<Button>(),"open support celebration "+kind);yield return null;
                dialog=Field<UiView>("supportDialog");
                Click(dialog.Ref<Button>("CoinSlotSupportDialog","getButton"),"claim support celebration "+kind);yield return null;
                yield return new WaitForSecondsRealtime(2.2f);
                ad=Field<GameObject>("originalAd");
                Click(ad.transform.Find("CloseButton").GetComponent<Button>(),"finish support celebration "+kind);yield return null;yield return null;
                yield return new WaitForSeconds(.4f);
                Check(ParticleMeshVertices(effects[index])>0,"support celebration renders particles: "+kind);
                yield return Capture("08-"+kind+"-celebration.png");
                yield return new WaitForSeconds(1f);
                DumpParticleGeometry(effects[index],kind.ToString());
                if(kind==SupportKind.Money) {
                    var cash=effects[index].Root.GetComponentInChildren<ParticleSystem>();
                    Check(cash.particleCount>0&&cash.particleCount<=84,"stationary money rain respects its original 40-per-second rate and two-second lifetime");
                    Check(CashBillboardsUndistorted(effects[index]),"every money billboard remains square and orthogonal while falling");
                }
                yield return Capture("09-"+kind+"-celebration-falling.png");
                yield return new WaitForSeconds(2f);
            }
            Check(errors.Count==0,"native support and honor transition produce no runtime errors");Complete(true,null);
        }
        static readonly string[] supportButtonsForVerification={"instantMoneyButton","earnMoneyButton","inspireButton"};
        static bool CashBillboardsUndistorted(UiView view) {
            bool found=false;
            foreach(var renderer in view.Root.GetComponentsInChildren<CanvasRenderer>()) {
                var mesh=renderer.GetMesh();if(!mesh||mesh.vertexCount==0)continue;found=true;
                var vertices=mesh.vertices;
                for(int i=0;i+3<vertices.Length;i+=4) {
                    var a=vertices[i+1]-vertices[i];var b=vertices[i+2]-vertices[i+1];
                    if(a.sqrMagnitude<.001f||b.sqrMagnitude<.001f||Mathf.Abs(a.magnitude/b.magnitude-1)>.02f||Mathf.Abs(Vector3.Dot(a.normalized,b.normalized))>.02f)return false;
                }
            }
            return found;
        }
        void DumpParticleGeometry(UiView view,string name) {
            var meshes=new System.Collections.Generic.List<object>();
            foreach(var renderer in view.Root.GetComponentsInChildren<CanvasRenderer>()) {
                var mesh=renderer.GetMesh();if(!mesh||mesh.vertexCount==0)continue;
                meshes.Add(new System.Collections.Generic.Dictionary<string,object>{{"name",renderer.name},
                    {"vertices",mesh.vertices.Select(v=>new[]{v.x,v.y,v.z}).ToArray()},
                    {"uv",mesh.uv.Select(v=>new[]{v.x,v.y}).ToArray()},{"triangles",mesh.triangles}});
            }
            System.IO.File.WriteAllText(System.IO.Path.Combine(output,name+"-particle-geometry.json"),NativeParticleJson.Stringify(meshes));
        }
        static int ParticleMeshVertices(UiView view) {
            int vertices=0;
            foreach(var particle in view.Root.GetComponentsInChildren<UIParticle>())
                foreach(var renderer in particle.GetComponentsInChildren<CanvasRenderer>()) {
                    var mesh=renderer.GetMesh();if(mesh)vertices+=mesh.vertexCount;
                }
            return vertices;
        }
    }
}

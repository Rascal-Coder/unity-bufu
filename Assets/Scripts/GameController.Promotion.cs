using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bufu.Rebuild.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    public sealed partial class GameController {
        UiView promotionView,promotionHud,promotionDialog;
        PromotionRun shownPromotion;PromotionStatus shownPromotionStatus;
        float promotionResultTime;bool promotionGuidePrompted,promotionGuideRead;
        int promotionWorld;
        GameObject PromotionNode(string field)=>promotionView.Ref("DownItem",field);

        void BuildPromotion() {
            promotionView=UiPrefab.Instantiate("Common/DownPanel",popupLayer);
            // Retain the complete native hierarchy and transforms, enabling only the promotion feature.
            foreach(Transform child in promotionView.Root.transform)child.gameObject.SetActive(child.name=="challengePanel");
            promotionHud=UiPrefab.Instantiate("Challenge/challengeUI",PromotionNode("challengePanel").transform);
            On(promotionHud.Root,false);On(PromotionNode("victoryPanel"),false);On(PromotionNode("failPanel"),false);
            BindPromotionSpine(PromotionNode("challengeBtn"),"Uieff_ty_btn_challenge","idle",true);
            BindPromotionSpine(PromotionNode("guideFinger"),"Uieff_common_guide","dianji",false);
            Click(PromotionNode("challengeBtn"),EnterPromotion);
            Click(PromotionNode("closeChallengeBtn"),()=>Model.CancelPromotion());
            Click(PromotionNode("victoryBtn"),()=>ConfirmPromotion(false));
            Click(PromotionNode("victoryAd"),()=>ConfirmPromotion(true));
            Click(PromotionNode("victoryMask"),()=>ConfirmPromotion(false));
            Click(PromotionNode("failBtn"),()=>On(PromotionNode("failPanel"),false));
            Click(PromotionNode("failMask"),()=>On(PromotionNode("failPanel"),false));
            BuildSupport();
            promotionGuideRead=PlayerPrefs.GetInt(PrefKey("ChallengeGuide_HasShown"),0)==1;
            promotionGuidePrompted=false;promotionWorld=Model.Data.world;shownPromotion=null;
        }
        void BindPromotionSpine(GameObject go,string resource,string animation,bool hit) {
            if(!go.GetComponent<CanvasRenderer>())go.AddComponent<CanvasRenderer>();
            var graphic=go.AddComponent<SpineGraphic>();
            if(!graphic.Load(resource))throw new InvalidOperationException("Missing original promotion Spine "+resource);
            graphic.UseScaledTime=true;graphic.Play(animation,true);graphic.raycastTarget=hit;
            var button=go.GetComponent<Button>();if(button)button.targetGraphic=graphic;
        }
        void EnterPromotion() {
            if(Model.StartPromotion(Time.unscaledTime)==null)return;
            if(!promotionGuideRead) {
                promotionGuideRead=true;
                if(!VerificationMode){PlayerPrefs.SetInt(PrefKey("ChallengeGuide_HasShown"),1);PlayerPrefs.Save();}
                On(PromotionNode("guideFinger"),false);Time.timeScale=0;
                ShowPromotionDialog("60秒内显示搜集足够多的金钱!",()=>Time.timeScale=1);
            }
        }
        void ShowPromotionDialog(string message,Action done=null) {
            if(promotionDialog!=null)ClosePopup(promotionDialog);
            var v=OpenPopup("Common/dialogPanel");promotionDialog=v;
            Txt(v.Ref<TMP_Text>("DialogPanel","dialogText"),message);
            var pos=v.Script("DialogPanel").Obj("topPosition");
            v.Ref<RectTransform>("DialogPanel","textParent").anchoredPosition=new Vector2(pos.F("x"),pos.F("y"));
            var mask=v.Ref<Image>("DialogPanel","backgroundImage");var color=mask.color;color.a=.001f;mask.color=color;
            // The source mask's graphic is non-raycastable. Native dialog routing forwards the click.
            mask.raycastTarget=true;
            Click(v.Ref<Button>("DialogPanel","bgButton"),()=>{ClosePopup(v);promotionDialog=null;done?.Invoke();});
        }
        void RefreshPromotion() {
            if(promotionView==null||!promotionView.Root)return;
            bool worldChanged=promotionWorld!=Model.Data.world;
            if(worldChanged)promotionWorld=Model.Data.world;
            var run=Model.Promotion;
            if(run!=null&&(shownPromotion!=run||shownPromotionStatus!=run.Status)) {
                shownPromotion=run;shownPromotionStatus=run.Status;
                if(run.Status==PromotionStatus.Won||run.Status==PromotionStatus.Lost) {
                    bool won=run.Status==PromotionStatus.Won;
                    On(PromotionNode("victoryPanel"),won&&!run.Claimed);On(PromotionNode("failPanel"),!won);
                    promotionResultTime=won?3:5;
                    if(won){BuildPromotionRewards(run);if(!VerificationMode)SaveStore.Save(Model);}
                }
            }
            On(promotionView.Root,tab==1);
            RefreshSupport();
            On(promotionHud.Root,Model.PromotionRunning);
            On(PromotionNode("closeChallengeBtn"),Model.PromotionRunning);
            On(PromotionNode("challengeBtn"),Model.PromotionReady&&!Model.PromotionRunning&&!Model.PromotionWonPending);
            ShowTopMain(tab==1);
            if(promotionHud.Root.activeSelf) {
                float fill=(float)(run.Collected/run.Target);
                promotionHud.Ref<Image>("ChallengeManager","progress").fillAmount=fill;
                promotionHud.Ref<RectTransform>("ChallengeManager","follow").anchoredPosition=new Vector2(-360+720*fill,0);
                Txt(promotionHud.Ref<TMP_Text>("ChallengeManager","sum"),"<color=#40FE84>"+GameModel.Format(run.Collected)+"</color>/"+GameModel.Format(run.Target));
                int left=(int)run.Remaining;Txt(promotionHud.Ref<TMP_Text>("ChallengeManager","time"),(left>=60?left/60:0).ToString("00")+":"+(left>=60?left%60:left).ToString("00"));
            }
            bool guide=Model.PromotionReady&&!Model.PromotionRunning&&!promotionGuideRead;
            On(PromotionNode("guideFinger"),guide);
            if(guide){PromotionNode("guideFinger").transform.position=PromotionNode("challengeBtn").transform.position+new Vector3(0,-40,0);
                if(!promotionGuidePrompted&&!VerificationMode){promotionGuidePrompted=true;ShowPromotionDialog("点击进入挑战,获得等级晋升!");}}
        }
        void UpdatePromotion() {
            UpdateSupport();
            if(promotionView==null||!promotionView.Root||!promotionView.Root.activeInHierarchy)return;
            bool result=PromotionNode("victoryPanel").activeSelf||PromotionNode("failPanel").activeSelf;
            if(result) {
                if(promotionResultTime>0)promotionResultTime-=Time.deltaTime;
                else if(PromotionNode("victoryPanel").activeSelf)ConfirmPromotion(false);
                else On(PromotionNode("failPanel"),false);
            }
            Model.TickPromotion(Time.deltaTime);RefreshPromotion();
        }
        void BuildPromotionRewards(PromotionRun run) {
            var grid=promotionView.Ref<RectTransform>("DownItem","victoryGrid");
            foreach(Transform child in grid)Destroy(child.gameObject);
            foreach(var reward in run.Rewards) {
                var v=UiPrefab.Instantiate("Common/rewardKuang",grid);
                var image=v.Get<Image>("itemIcon");int index=reward.Key>=101?6+reward.Key-101:reward.Key-1;var icons=promotionView.Script("DownItem").Arr("itemSprites");
                image.sprite=UiAssets.Sprite(((System.Collections.Generic.Dictionary<string,object>)icons[index]).Str("sprite"));image.SetNativeSize();
                Txt(v.Get<TMP_Text>("countTmp"),GameModel.Format(reward.Value));
            }
        }
        void ConfirmPromotion(bool ad) {
            if(ad){ShowOriginalAd(()=>ConfirmPromotionPrize(true));return;}
            ConfirmPromotionPrize(false);
        }
        void ConfirmPromotionPrize(bool ad) {
            var run=Model.Promotion;
            if(!Model.ClaimPromotion(ad))return;
            On(PromotionNode("victoryPanel"),false);
            FlyPromotionRewards(run.Rewards.Select(x=>x.Key).ToList());
            if(!VerificationMode)SaveStore.Save(Model);
        }

        // UIManager.ShowCoinDimondTween: original fly prefabs, particle counts, spacing and DOTween curves.
        void FlyPromotionRewards(List<int> ids) {
            var particles=ids.SelectMany(id=>Enumerable.Repeat(id,id>=3?8:15)).ToArray();
            bool items=ids.Any(id=>id>=3),multiple=ids.Count>1;
            float radius=multiple?(items?220:280):(items?180:240),spacing=multiple?(items?40:50):(items?60:80);
            var positions=new List<Vector2>();
            for(int tries=0;positions.Count<particles.Length&&tries<particles.Length*50;tries++) {
                var p=UnityEngine.Random.insideUnitCircle*radius;if(positions.All(x=>Vector2.Distance(p,x)>=spacing))positions.Add(p);
            }
            while(positions.Count<particles.Length)positions.Add(UnityEngine.Random.insideUnitCircle*radius);
            for(int i=0;i<particles.Length;i++) {
                int id=particles[i];var v=UiPrefab.Instantiate("fly/"+(multiple?"all":id.ToString()),popupLayer);
                if(multiple)foreach(Transform child in v.Root.transform)On(child.gameObject,child.name==id.ToString());
                foreach(var image in v.Root.GetComponentsInChildren<Graphic>(true))image.raycastTarget=false;
                var r=v.Rect;r.anchoredPosition=positions[i];var world=r.position;r.anchorMin=r.anchorMax=new Vector2(0,1);r.position=world;
                float start=UnityEngine.Random.Range(id>=3?.25f:.3f,id>=3?.45f:.55f),end=UnityEngine.Random.Range(id>=3?.4f:.5f,.8f);
                r.localScale=new Vector3(start,start,1);
                float pop=UnityEngine.Random.Range(.18f,.3f),hold=UnityEngine.Random.Range(0,.2f),drift=UnityEngine.Random.Range(.35f,.55f),fly=UnityEngine.Random.Range(.55f,.85f),delay=UnityEngine.Random.Range(0,.4f);
                Vector3 target=(id==1||id==3?goldTmp:diamondTmp).transform.position;
                Vector3 offset=new Vector3(UnityEngine.Random.Range(-60f,60f),UnityEngine.Random.Range(80f,140f),0);
                StartCoroutine(PromotionParticle(r,start,end,pop,hold,drift,fly,delay,target,offset));
            }
        }
        IEnumerator PromotionParticle(RectTransform r,float start,float end,float pop,float hold,float drift,float fly,float delay,Vector3 target,Vector3 offset) {
            yield return new WaitForSeconds(delay);
            yield return PromotionTween(pop,t=>{if(r)r.localScale=new Vector3(Mathf.LerpUnclamped(start,end,OutBack(t)),Mathf.LerpUnclamped(start,end,OutBack(t)),1);});
            yield return new WaitForSeconds(hold);if(!r)yield break;Vector3 from=r.position;
            yield return PromotionTween(drift,t=>{if(r)r.position=Vector3.LerpUnclamped(from,from+offset,Mathf.Sin(t*Mathf.PI*.5f));});
            if(!r)yield break;from=r.position;
            yield return PromotionTween(fly,t=>{if(r)r.position=Vector3.LerpUnclamped(from,target,t*t);});
            if(r)Destroy(r.gameObject);
        }
        static IEnumerator PromotionTween(float duration,Action<float> step,Action done=null) {
            float time=0;while(time<duration){time+=Time.deltaTime;step(Mathf.Clamp01(time/duration));yield return null;}done?.Invoke();
        }
    }
}

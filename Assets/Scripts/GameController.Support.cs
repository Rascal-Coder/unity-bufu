using System;
using System.Collections;
using System.Linq;
using Bufu.Rebuild.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    public sealed partial class GameController {
        bool supportExpanded,supportShown;int supportWorld;
        UiView supportDialog;SupportKind supportDialogKind;
        readonly UiView[] supportEffects=new UiView[3];readonly float[] supportEffectTime=new float[3];
        readonly ParticleSystem[] supportEmitters=new ParticleSystem[3];
        readonly long[] supportTimerStamp=new long[3];readonly float[] supportTimerLeft=new float[3],supportTimerNextTick=new float[3];
        readonly string[] supportButtons={"instantMoneyButton","earnMoneyButton","inspireButton"};
        readonly string[] supportTimers={"instantMoneyTimer","earnMoneyTimer","inspireTimer"};
        readonly string[] supportTexts={"instantMoneyButtonText","earnMoneyButtonText","inspireButtonText"};

        void BuildSupport() {
            On(PromotionNode("worldCup"),false);On(PromotionNode("gongfuButton"),false);On(PromotionNode("dasheButton"),false);
            for(int i=0;i<3;i++) {
                var kind=(SupportKind)i;var button=PromotionNode(supportButtons[i]);
                Click(button,()=>ShowSupport(kind));
                var btnRect=(RectTransform)button.transform;btnRect.sizeDelta=new Vector2(200,200);btnRect.localScale=Vector3.one;
                var btnImg=button.GetComponent<Image>();if(btnImg)btnImg.preserveAspect=true;
                var timer=PromotionNode(supportTimers[i]);On(timer,false);
                var timerRect=(RectTransform)timer.transform;timerRect.anchoredPosition=new Vector2(0,-110.6f);timerRect.sizeDelta=new Vector2(158,31);
                var timerTxt=PromotionNode(supportTexts[i]).GetComponent<TMP_Text>();
                if(timerTxt){timerTxt.enableAutoSizing=false;timerTxt.enableWordWrapping=false;timerTxt.fontSize=34;timerTxt.alignment=TextAlignmentOptions.Center;}
                string effect=i==0?"Ads_coinrain":i==1?"Ads_zuanqian":"Ads_jiayou";
                var v=UiPrefab.Instantiate("Role/"+effect,promotionView.Root.transform);v.Rect.anchoredPosition=new Vector2(0,1300);
                var emitter=v.Root.transform.GetChild(0).GetComponentInChildren<ParticleSystem>(true);
                emitter.Stop();emitter.Play();emitter.gameObject.SetActive(false);
                supportEffects[i]=v;supportEmitters[i]=emitter;supportEffectTime[i]=0;
            }
            Click(PromotionNode("openBtn"),()=>{supportExpanded=true;LayoutSupport();});
            Click(PromotionNode("foldBtn"),()=>{supportExpanded=false;LayoutSupport();});
            supportExpanded=false;supportShown=false;supportWorld=Model.Data.world;sceneAvoidRects.Clear();
            LayoutSupport();
        }
        void RefreshSupport() {
            if(promotionView==null||!promotionView.Root)return;
            bool shown=promotionView.Root.activeInHierarchy;
            bool resumed=shown&&(!supportShown||supportWorld!=Model.Data.world);
            if(resumed)Model.ResumeSupportBuffs();
            supportShown=shown;supportWorld=Model.Data.world;
            On(PromotionNode("adSummonPanel"),!Model.PromotionRunning);
            bool changed=false;
            for(int i=0;i<3;i++) {
                var button=PromotionNode(supportButtons[i]);bool unlocked=Model.DeveloperMode||shownMainLevel>Model.Factor(i==0?58:19);
                changed|=button.activeSelf!=unlocked;On(button,unlocked);
                long stamp=Model.State.supportLastTime[2-i];
                if(resumed||stamp!=supportTimerStamp[i]) {supportTimerStamp[i]=stamp;supportTimerLeft[i]=Model.SupportRemaining((SupportKind)i);supportTimerNextTick[i]=Time.unscaledTime+1;}
                if(shown&&supportTimerLeft[i]>0&&Time.unscaledTime>=supportTimerNextTick[i]){supportTimerLeft[i]-=1;supportTimerNextTick[i]=Time.unscaledTime+1;}
                On(PromotionNode(supportTimers[i]),supportTimerLeft[i]>0);
                Txt(PromotionNode(supportTexts[i]).GetComponent<TMP_Text>(),SupportTime(supportTimerLeft[i]));
            }
            if(changed)LayoutSupport();
            RefreshSupportDialog();
        }
        static string SupportTime(float seconds) {
            int t=Mathf.RoundToInt(seconds);return t>=3600?(t/3600).ToString("00")+":"+(t%3600/60).ToString("00")+":"+(t%60).ToString("00"):(t/60).ToString("00")+":"+(t%60).ToString("00");
        }
        void LayoutSupport() {
            if(promotionView==null)return;
            var content=promotionView.Ref<RectTransform>("DownItem","adSummonContent");
            var bg=promotionView.Ref<RectTransform>("DownItem","adSummonBg");
            var mask=promotionView.Ref<RectTransform>("DownItem","adMask");
            int count=content.Cast<Transform>().Count(x=>x.gameObject.activeSelf);
            bool moneyUnlocked=Model.DeveloperMode||shownMainLevel>Model.Factor(58);
            bool anyUnlocked=moneyUnlocked||count>0;
            On(PromotionNode("instantMoneyButton"),moneyUnlocked);
            On(content.gameObject,supportExpanded&&count>0);On(bg.gameObject,count>0);
            On(PromotionNode("openBtn"),!supportExpanded&&anyUnlocked);On(PromotionNode("foldBtn"),supportExpanded&&anyUnlocked);
            float y=-235;
            if(supportExpanded&&count>0) {
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                mask.sizeDelta=new Vector2(mask.sizeDelta.x,count>=4?795:content.rect.height-180);
                y=mask.anchoredPosition.y-mask.rect.height-50;
                bg.sizeDelta=new Vector2(bg.sizeDelta.x,mask.rect.height+120);
                promotionView.Ref<ScrollRect>("DownItem","scrollFrame").verticalNormalizedPosition=1;
            } else bg.sizeDelta=new Vector2(bg.sizeDelta.x,85);
            foreach(string field in new[]{"openBtn","foldBtn"})promotionView.Ref<RectTransform>("DownItem",field).anchoredPosition=new Vector2(0,y);
        }
        void UpdateSupport() {
            if(promotionView==null||!promotionView.Root)return;
            RefreshSupport();
            RefreshSceneAvoidRects();
            if(!promotionView.Root.activeInHierarchy)return;
            for(int i=0;i<3;i++)if(supportEmitters[i]&&supportEmitters[i].gameObject.activeSelf) {
                supportEffectTime[i]+=Time.deltaTime;
                if(supportEffectTime[i]>=3)supportEmitters[i].gameObject.SetActive(false);
            }
        }
        void ShowSupport(SupportKind kind) {
            if(!Model.SupportUnlocked(kind)||supportDialog!=null)return;
            supportDialogKind=kind;
            string resource=kind==SupportKind.Money?"AdGetMoney":kind==SupportKind.Income?"AdIncome":"AdSpeed";
            var v=OpenPopup("Ad/"+resource);supportDialog=v;
            if(kind==SupportKind.Money)Txt(v.Ref<TMP_Text>("CoinSlotSupportDialog","coinText"),GameModel.Format(Model.SupportMoney(Time.unscaledTime)));
            else {
                string prefix=kind==SupportKind.Income?"所有帮手获取金钱<color=#e66e17>+":"所有帮手速度提高<color=#e66e17>";
                float value=(float)Model.Factor(kind==SupportKind.Income?11:8);
                Txt(v.Ref<TMP_Text>("CoinSlotSupportDialog","adDescText"),prefix+Mathf.RoundToInt(value*100)+"%</color>；<br>持续<color=#e66e17>"+Model.SupportMinutes(kind)+"min</color>");
            }
            Action close=()=>{if(v.Root)ClosePopup(v);supportDialog=null;};
            Click(v.Ref<Button>("CoinSlotSupportDialog","closeButton"),close);Click(v.Ref<Button>("CoinSlotSupportDialog","bgButton"),close);
            Click(v.Ref<Button>("CoinSlotSupportDialog","getButton"),()=>{
                if(Model.SupportRemaining(kind)>0)return;
                double money=kind==SupportKind.Money?Model.SupportMoney(Time.unscaledTime):0;
                ShowOriginalAd(()=>{
                    if(Model.ClaimSupport(kind,money)) {
                        int index=(int)kind;var emitter=supportEmitters[index];emitter.Stop();emitter.Play();emitter.gameObject.SetActive(true);
                        supportEffectTime[index]=0;if(!VerificationMode)SaveStore.Save(Model);
                    }
                    close();
                });
            });
            RefreshSupportDialog();
        }
        void RefreshSupportDialog() {
            if(supportDialog==null)return;if(!supportDialog.Root){supportDialog=null;return;}
            float left=Model.SupportRemaining(supportDialogKind);var get=supportDialog.Ref<Button>("CoinSlotSupportDialog","getButton");get.interactable=left<=0;
            var tip=supportDialog.Ref<TMP_Text>("CoinSlotSupportDialog","bianlanText");On(tip.gameObject,left>0);Txt(tip,"过"+SupportTime(left)+"后使用");
        }

        // Exact offline FakeAdUI flow: two unscaled seconds, then an explicit close awards the callback.
        GameObject originalAd;bool originalAdFinished;
        void ShowOriginalAd(Action awarded) {
            if(originalAd)return;
            var go=new GameObject("Original Fake Ad",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            go.transform.SetParent(transform,false);originalAd=go;popups.Add(go);
            var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=9999;
            var root=(RectTransform)go.transform;Stretch(root);
            var background=NewRect("Background",root);Stretch(background);background.gameObject.AddComponent<Image>().color=Color.black;
            TMP_Text label(string name,float font,Vector2 size,Vector2 anchor,string value) {
                var r=NewRect(name,root);r.anchorMin=r.anchorMax=anchor;r.sizeDelta=size;
                var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=UiAssets.Font();t.color=Color.white;t.fontSize=font;t.alignment=TextAlignmentOptions.Center;Txt(t,value);return t;
            }
            var countdown=label("CountdownText",48,new Vector2(200,100),new Vector2(.5f,.32f),"0:02");
            var content=label("AdContentText",Mathf.Clamp(Screen.width*.055f,18,42),new Vector2(1000,500),new Vector2(.5f,.6f),"观看广告获得奖励\n倒计时结束后可关闭广告");
            var contentRect=content.rectTransform;contentRect.anchorMin=new Vector2(.08f,.5f);contentRect.anchorMax=new Vector2(.92f,.76f);contentRect.offsetMin=contentRect.offsetMax=Vector2.zero;
            var rect=NewRect("CloseButton",root);rect.anchorMin=rect.anchorMax=rect.pivot=Vector2.one;rect.anchoredPosition=new Vector2(-50,-50);rect.sizeDelta=new Vector2(120,50);
            var image=rect.gameObject.AddComponent<Image>();image.color=new Color(1,.3f,.3f,1);var button=rect.gameObject.AddComponent<Button>();button.targetGraphic=image;
            var text=NewRect("ButtonText",rect);Stretch(text);var tmp=text.gameObject.AddComponent<TextMeshProUGUI>();tmp.font=UiAssets.Font();tmp.fontSize=24;tmp.alignment=TextAlignmentOptions.Center;Txt(tmp,"关闭广告");
            On(rect.gameObject,false);originalAdFinished=false;
            Click(button,()=>{if(!originalAdFinished)return;popups.Remove(go);Destroy(go);originalAd=null;StartCoroutine(OriginalAdAward(awarded));});
            StartCoroutine(OriginalAdCountdown(countdown,content,rect.gameObject));
        }
        IEnumerator OriginalAdCountdown(TMP_Text timer,TMP_Text content,GameObject close) {
            float left=2;
            while(left>0&&originalAd){left=Mathf.Max(0,left-Time.unscaledDeltaTime);timer.text=Mathf.FloorToInt(left/60)+":"+Mathf.FloorToInt(left%60).ToString("00");yield return null;}
            if(!originalAd)yield break;originalAdFinished=true;close.SetActive(true);timer.text="0:00";Txt(content,"广告播放完成！\n点击关闭按钮获取奖励");
        }
        static IEnumerator OriginalAdAward(Action action){yield return null;action?.Invoke();}
    }
}

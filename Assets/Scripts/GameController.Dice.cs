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
        readonly HashSet<CoinActor> diceResonanceThrows=new HashSet<CoinActor>();
        UiView diceBoundPanel,diceSuperDialog;
        int dicePresentationWorld,diceGuideStep;
        CoinActor diceGuideTarget,diceGuideWaiting;
        int diceForcedFace;
        bool diceGuideMessage;
        int diceObservedWorld,diceObservedSuper,diceObservedMerchant;
        readonly Queue<int> diceUnlockNotifications=new Queue<int>();
        public bool DiceGuideBlocksHelpers=>Model!=null&&Model.Data.world==2&&diceGuideStep>0;
        static string DiceText(string chinese,string english)=>L10n.IsEnglish?english:chinese;

        /// <summary>Call before RollOutcome in CoinActor.Launch; blocks non-target/automatic tosses during the original guide.</summary>
        public bool PrepareDiceThrow(CoinActor coin,bool byHand) {
            // A System resonance uses the original default role -1 and therefore receives manual
            // income, but it is not a physical tutorial click.
            if(diceResonanceThrows.Contains(coin))return !DiceGuideBlocksHelpers;
            if(coin.World!=2||!DiceGuideBlocksHelpers)return true;
            if(!byHand||diceGuideMessage||coin!=diceGuideTarget||(diceGuideStep!=1&&diceGuideStep!=3))return false;
            diceForcedFace=diceGuideStep==1?1:6;diceGuideWaiting=coin;
            diceGuideTarget=null;diceGuideStep++;HideGuide();Time.timeScale=1;
            return true;
        }
        public int RollDiceFace(CoinActor coin) {
            int forced=coin==diceGuideWaiting?diceForcedFace:0;
            if(forced>0)diceForcedFace=0;
            return Model.RollDiceFace(UnityEngine.Random.value,forced);
        }
        public void DiceLanded(CoinActor dice) {
            if(dice.World!=2||Model.Data.world!=2)return;
            int face=dice.CurrentFace;
            Model.RecordDiceLanded(face);
            HandleDiceGuideLanded(dice);
            bool fromResonance=diceResonanceThrows.Remove(dice);
            if(fromResonance||DiceGuideBlocksHelpers||UnityEngine.Random.value>=Model.DiceSkillEffect(104))return;
            // Original source uses squared distance <= 3 world units; one world unit is 100 canvas pixels.
            foreach(var other in Coins.ToArray()) {
                if(!other||other==dice||other.World!=2||other.Busy||other.CurrentFace!=face)continue;
                if((other.Rect.anchoredPosition-dice.Rect.anchoredPosition).sqrMagnitude>30000f)continue;
                diceResonanceThrows.Add(other);
                if(!other.Launch(RandomLanding(other.Rect.anchoredPosition),true))diceResonanceThrows.Remove(other);
            }
        }

        public void ResetDiceWorldPresentation() {
            if(Model==null)return;
            if(dicePresentationWorld!=Model.Data.world) {
                if(diceGuideStep>0){HideGuide();Time.timeScale=1;}
                diceGuideStep=0;diceGuideTarget=diceGuideWaiting=null;diceForcedFace=0;diceGuideMessage=false;
                diceResonanceThrows.Clear();dicePresentationWorld=Model.Data.world;
            }
        }
        /// <summary>Call after UpdateGuide so the shared guide layer can also present dice-world teaching.</summary>
        public void TickDiceSystems() {
            if(Model==null||promotionView==null)return;
            ResetDiceWorldPresentation();BindDiceHud();
            ObserveDiceUnlocks();
            Model.TickDiceTimers(Time.deltaTime);
            if(Model.Data.world==2)Model.RefreshDiceMerchantDay(GameModel.TodayNumber());
            RefreshDiceHud();TickDiceGuide();
        }
        void BindDiceHud() {
            if(diceBoundPanel==promotionView)return;diceBoundPanel=promotionView;
            var super=PromotionNode("superSix");var merchant=PromotionNode("zuanshiShangRen");
            // The exported shortcuts extend into the upgrade cards. Move their common parent
            // into the playfield, keeping the original buttons, roles, bubbles and animation.
            var shortcuts=(RectTransform)super.transform.parent;
            shortcuts.anchoredPosition+=Vector2.up*540;
            // Enable only the parent path shared by these original nodes; keep unrelated children disabled.
            for(var parent=super.transform.parent;parent&&parent!=promotionView.Root.transform;parent=parent.parent)parent.gameObject.SetActive(true);
            foreach(Transform sibling in super.transform.parent)if(sibling.gameObject!=super&&sibling.gameObject!=merchant)sibling.gameObject.SetActive(false);
            Click(PromotionNode("superBtn"),ShowDiceSuperActivation);
            Click(PromotionNode("shangrenBtn"),ClaimDiceMerchant);
            AddDiceRoleAnimation(super.transform.Find("supperSixRole"),"zgygy_666","idle_front");
            AddDiceRoleAnimation(merchant.transform.Find("role"),"zgygy_zuanshishangren","idle_front");
            On(PromotionNode("supperGuideObj"),false);
            foreach(var text in new[]{"superProgress","tipTmp","predictNumTmp"}) {
                var t=PromotionNode(text).GetComponent<TMP_Text>();if(!t)continue;
                t.enableAutoSizing=true;t.fontSizeMax=t.fontSize;t.fontSizeMin=22;t.enableWordWrapping=true;
            }
        }
        void AddDiceRoleAnimation(Transform node,string resource,string animation) {
            if(!node)return;if(!node.GetComponent<CanvasRenderer>())node.gameObject.AddComponent<CanvasRenderer>();
            var graphic=node.GetComponent<SpineGraphic>()??node.gameObject.AddComponent<SpineGraphic>();
            if(!graphic.Load(resource))throw new InvalidOperationException("Missing original dice Spine "+resource);
            graphic.UseScaledTime=true;graphic.Play(animation,true);graphic.raycastTarget=false;
        }
        void RefreshDiceHud() {
            bool shown=Model.Data.world==2&&!Model.PromotionRunning;
            On(PromotionNode("superSix"),shown&&Model.DiceSuperUnlocked);
            On(PromotionNode("zuanshiShangRen"),shown&&Model.DiceMerchantUnlocked);
            On(PromotionNode("jihuoEffect"),Model.DiceSuperReady);
            if(Model.DiceSuperUnlocked) {
                string text=Model.DiceSuperActive
                    ?DiceText("生效中：","Active: ")+Mathf.CeilToInt(Model.DiceSuperRemaining)+"s"
                    :GameModel.Format(Model.State.diceSixCount)+"/"+GameModel.Format(Model.DiceSuperTarget);
                Txt(PromotionNode("superProgress").GetComponent<TMP_Text>(),text);
            }
            if(!Model.DiceMerchantUnlocked)return;
            long now=GameModel.DiceLocalMilliseconds;bool limited=Model.DiceMerchantAtLimit,ready=Model.DiceMerchantReady(now);
            On(PromotionNode("diamondImg"),!limited);On(PromotionNode("predictNumTmp"),!limited);On(PromotionNode("shangrenProgress"),false);
            Txt(PromotionNode("predictNumTmp").GetComponent<TMP_Text>(),Model.DiceMerchantMinimum+"~"+Model.DiceMerchantMaximum);
            var tip=PromotionNode("tipTmp").GetComponent<TMP_Text>();
            Txt(tip,limited?DiceText("今日已达上限","Daily limit reached"):ready?DiceText("点击获取钻石","Tap to collect diamonds"):DiceText("剩余：","Ready in ")+Mathf.CeilToInt(Model.DiceMerchantRemaining(now))+"s");
            tip.transform.localScale=Vector3.one*(ready?1+.12f*Mathf.Sin(Time.unscaledTime*4):1);
        }
        void ShowDiceSuperActivation() {
            if(!Model.DiceSuperReady){ShowToast(Model.DiceSuperActive?DiceText("超级666模式开启中","Super 666 is active"):DiceText("6点的投掷次数不足","Roll more sixes to activate"));return;}
            if(diceSuperDialog!=null&&diceSuperDialog.Root)return;
            const string cls="AdLaoHuJi";
            var view=OpenPopup("Ad/AdLaoHuJi");diceSuperDialog=view;
            var animation=view.Ref(cls,"laohujiEffect");
            if(!animation.GetComponent<CanvasRenderer>())animation.AddComponent<CanvasRenderer>();
            var spine=animation.AddComponent<SpineGraphic>();
            if(!spine.Load("Uieff_laohuji"))throw new InvalidOperationException("Missing original slot-machine Spine");
            spine.UseScaledTime=true;spine.Play("effect",false);spine.raycastTarget=false;
            var normal=view.Ref<Button>(cls,"jihuoBtn");var ad=view.Ref<Button>(cls,"adjihuoBtn");
            var hint=view.Ref<TMP_Text>(cls,"txtTip");
            Txt(hint,DiceText("激活超级666,全场骰子变为6点!","Activate Super 666: every die rolls a 6!"));
            var normalLabel=view.Ref<TMP_Text>(cls,"jihuoBtnTxt");
            Txt(normalLabel,DiceText("激活","Activate")+" ("+Mathf.RoundToInt(Model.DiceSuperDuration(false))+"s)");
            var adLabel=view.Ref<TMP_Text>(cls,"adTxt");
            Txt(adLabel,((float)Model.Factor(76)+(float)Model.TalentValue(309)).ToString("0.#")+DiceText("倍时间","× time"));
            foreach(var text in new[]{hint,normalLabel,adLabel}){text.enableAutoSizing=true;text.fontSizeMax=text.fontSize;text.fontSizeMin=24;}
            On(normal.gameObject,false);On(ad.gameObject,false);On(hint.gameObject,false);
            StartCoroutine(DiceActivationDelay(view,normal,ad,hint));
            Click(normal,()=>{if(Model.ActivateDiceSuper(false)){ClosePopup(view);diceSuperDialog=null;}});
            Click(ad,()=>ShowOriginalAd(()=>{if(Model.ActivateDiceSuper(true)){ClosePopup(view);diceSuperDialog=null;}}));
        }
        IEnumerator DiceActivationDelay(UiView view,Button normal,Button ad,TMP_Text hint) {
            yield return new WaitForSeconds(2);if(view.Root){On(normal.gameObject,true);On(ad.gameObject,true);On(hint.gameObject,true);}
        }
        void DiceButtonLabel(Button button,string text) {
            // Use a blank original button with live English text instead of retaining baked cancel/confirm lettering.
            var image=button.GetComponent<Image>();var blank=FortuneButton();if(blank){image.sprite=blank;image.type=Image.Type.Sliced;}
            var rect=NewRect("Dice Button Label",button.transform);Stretch(rect);
            var t=rect.gameObject.AddComponent<TextMeshProUGUI>();t.font=UiAssets.Font();t.fontSize=36;t.enableAutoSizing=true;t.fontSizeMin=20;t.fontSizeMax=36;t.alignment=TextAlignmentOptions.Center;t.color=FortuneInk;t.raycastTarget=false;Txt(t,text);
        }
        void ClaimDiceMerchant() {
            int award=Model.ClaimDiceMerchant(UnityEngine.Random.Range(1,7),UnityEngine.Random.value,GameModel.DiceLocalMilliseconds,GameModel.TodayNumber());
            if(award<=0)return;
            PlayReward();ShowRewardBox(new List<KeyValuePair<int,double>>{new KeyValuePair<int,double>(2,award)});
        }

        void ObserveDiceUnlocks() {
            if(diceObservedWorld!=Model.Data.world) {
                diceObservedWorld=Model.Data.world;diceObservedSuper=Model.State.skills[106];diceObservedMerchant=Model.State.skills[107];
                diceUnlockNotifications.Clear();return;
            }
            if(Model.Data.world!=2)return;
            if(diceObservedSuper==0&&Model.State.skills[106]>0)diceUnlockNotifications.Enqueue(106);
            if(diceObservedMerchant==0&&Model.State.skills[107]>0)diceUnlockNotifications.Enqueue(107);
            diceObservedSuper=Model.State.skills[106];diceObservedMerchant=Model.State.skills[107];
            if(diceUnlockNotifications.Count>0&&!ModalOpen&&!VerificationMode)ShowDiceUnlockIntroduction(diceUnlockNotifications.Dequeue());
        }
        void ShowDiceUnlockIntroduction(int skill) {
            const string cls="AdDiamondMerchant";bool super=skill==106;
            var view=OpenPopup("Ad/AdDiamondMerchant");
            var card=view.Ref(cls,super?"supper":"zuanshi");On(view.Ref(cls,"supper"),super);On(view.Ref(cls,"zuanshi"),!super);
            var title=card.transform.Find("Title");var titleImage=title.GetComponent<Image>();titleImage.enabled=false;
            var label=NewRect("English Title",title);Stretch(label);var text=label.gameObject.AddComponent<TextMeshProUGUI>();
            text.font=UiAssets.Font();text.fontSize=60;text.enableAutoSizing=true;text.fontSizeMax=60;text.fontSizeMin=30;text.color=Color.white;text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false;
            Txt(text,super?"Super 666":"Diamond Merchant");
            var body=card.transform.Find("adTmp").GetComponent<TMP_Text>();body.enableAutoSizing=true;body.fontSizeMax=body.fontSize;body.fontSizeMin=24;
            Txt(body,super?"Roll sixes to charge Super 666.\nActivate it to make every die roll a 6!":"The merchant delivers free diamonds from time to time.");
            var confirm=view.Ref<Button>(cls,super?"supperBtn":"zuanshiBtn");DiceButtonLabel(confirm,"Confirm");
            Click(confirm,()=>StartCoroutine(FlyDiceUnlockCard(view,card,super)));
        }
        IEnumerator FlyDiceUnlockCard(UiView view,GameObject card,bool super) {
            On(view.Ref("AdDiamondMerchant","mask"),false);
            var button=view.Ref<Button>("AdDiamondMerchant",super?"supperBtn":"zuanshiBtn");button.interactable=false;
            var rect=(RectTransform)card.transform;var start=rect.position;var scale=rect.localScale;
            var destination=PromotionNode(super?"superSix":"zuanshiShangRen").transform.position;
            float elapsed=0;while(elapsed<1.5f&&view.Root){elapsed+=Time.deltaTime;float t=Mathf.Clamp01(elapsed/1.5f);rect.position=Vector3.Lerp(start,destination,t);rect.localScale=Vector3.Lerp(scale,scale*.1f,Mathf.Clamp01(elapsed));yield return null;}
            if(view.Root)ClosePopup(view);
        }

        void TickDiceGuide() {
            if(Model.Data.world!=2)return;
            if(diceGuideStep==0&&!VerificationMode&&!Model.OpenAccess&&PlayerPrefs.GetInt(PrefKey("dice_world_guide_step"),0)==0&&!ModalOpen&&Coins.Count>1)ShowDiceGuideClick(1,0,true);
            if(diceGuideStep>0&&guideLayer)guideLayer.gameObject.SetActive(!ModalOpen);
        }
        void ShowDiceGuideClick(int step,int index,bool dialog) {
            var candidates=Coins.Where(x=>x&&x.World==2).ToArray();
            if(index>=candidates.Length){FinishDiceGuide();return;}
            diceGuideStep=step;diceGuideMessage=false;diceGuideTarget=candidates[index];diceGuideWaiting=null;
            Time.timeScale=0;PointGuideAt(diceGuideTarget.Rect,true);
            if(dialog)ShowGuideDialog(DiceText("点击投掷骰子，根据投掷的点数获取奖励","Tap a die to roll. Its number determines your reward."),guideHole,null);
        }
        void HandleDiceGuideLanded(CoinActor dice) {
            if(!DiceGuideBlocksHelpers||diceGuideWaiting!=dice)return;
            diceGuideWaiting=null;diceGuideMessage=true;HideGuide();Time.timeScale=0;
            bool first=diceGuideStep==2;
            ShowGuideDialog(first?DiceText("投出了1点，获得了1倍奖励","You rolled 1 and received a 1× reward."):DiceText("6点可以获得6倍奖励！","A 6 gives you a 6× reward!"),null,()=>{
                if(first)ShowDiceGuideClick(3,1,false);else FinishDiceGuide();
            });
        }
        void FinishDiceGuide() {
            if(!VerificationMode){PlayerPrefs.SetInt(PrefKey("dice_world_guide_step"),1);PlayerPrefs.Save();}
            diceGuideStep=0;diceGuideTarget=diceGuideWaiting=null;diceGuideMessage=false;HideGuide();Time.timeScale=1;
        }
    }
}

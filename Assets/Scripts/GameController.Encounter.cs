using System;
using Bufu.Rebuild.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    /// <summary>
    /// The original random investment event. Art with baked Chinese is not reused:
    /// the plaque and the bet button are English drawings, and labels sit on the blank button.
    /// </summary>
    public sealed partial class GameController {
        enum EncounterPhase { None, Gamble, Fail, Lucky }
        GameObject encounterRoot;
        EncounterPhase encounterPhase;
        bool encounterAd;
        GameModel.EncounterOffer encounterOffer;
        double encounterStake, encounterPayout;
        Image encounterLight, encounterCharacter, encounterCurrency;
        TMP_Text encounterAmount, encounterBody, encounterOdds, encounterActionLabel;
        Button encounterDim, encounterClose, encounterAction;

        void OnEncounterOffered(GameModel.EncounterOffer offer) {
            if(VerificationMode){Model.ClearEncounter();return;}
            if(!interfaceReady||CutscenePlaying||offlineView!=null)return;
            ShowEncounter();
        }

        void ShowEncounter() {
            var offer=Model.PendingEncounter;
            if(offer==null||encounterRoot||!popupLayer)return;
            encounterOffer=offer;
            encounterStake=offer.Diamond?DiamondStake():offer.Stake;
            encounterPhase=EncounterPhase.Gamble;
            var font=UiAssets.Font();
            var root=NewRect("Encounter",popupLayer);Stretch(root);encounterRoot=root.gameObject;popups.Add(encounterRoot);
            var dim=NewRect("Dim",root);Stretch(dim);
            var dimImage=dim.gameObject.AddComponent<Image>();dimImage.color=new Color(0f,0f,0f,.72f);
            encounterDim=dim.gameObject.AddComponent<Button>();encounterDim.targetGraphic=dimImage;
            var content=NewRect("Content",root);content.anchorMin=content.anchorMax=new Vector2(.5f,.5f);content.sizeDelta=new Vector2(1100,1500);
            Image Picture(string name,Transform parent,string key,float width,Vector2 position) {
                var rect=NewRect(name,parent);rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.anchoredPosition=position;
                var image=rect.gameObject.AddComponent<Image>();image.raycastTarget=false;
                var sprite=Sp(key);image.sprite=sprite;image.enabled=sprite;image.preserveAspect=true;
                if(sprite){float height=width*sprite.rect.height/Mathf.Max(1f,sprite.rect.width);rect.sizeDelta=new Vector2(width,height);}
                return image;
            }
            TMP_Text Words(string name,Transform parent,Vector2 position,Vector2 size,float fontSize,Color color) {
                var rect=NewRect(name,parent);rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.anchoredPosition=position;rect.sizeDelta=size;
                var text=rect.gameObject.AddComponent<TextMeshProUGUI>();if(font)text.font=font;
                text.fontSize=fontSize;text.color=color;text.alignment=TextAlignmentOptions.Center;text.enableWordWrapping=true;text.raycastTarget=false;
                return text;
            }
            encounterLight=Picture("Light",content,"ad_pic_investBgLight_scale2.png",980,new Vector2(0,180));
            Picture("Paper",content,"ad_pic_investBg01_scale2",900,new Vector2(0,20));
            encounterCharacter=Picture("Character",content,"ad_pic_investAsk_scale2",520,new Vector2(0,250));
            var title=Picture("Title",content,"ad_zi_encounter",980,new Vector2(0,680));
            if(!title.enabled) {
                var heading=Words("Title Text",content,new Vector2(0,680),new Vector2(900,140),72,new Color(1f,.95f,.7f));
                Txt(heading,"特殊奇遇");
            }
            encounterCurrency=Picture("Currency",content,"ty_icon_coin01@4f9b2316",96,new Vector2(-150,-40));
            encounterAmount=Words("Amount",content,new Vector2(70,-40),new Vector2(420,110),64,new Color(.90f,.45f,.09f));
            encounterBody=Words("Body",content,new Vector2(0,-210),new Vector2(760,180),36,FortuneInk);
            encounterOdds=Words("Odds",content,new Vector2(0,-360),new Vector2(760,120),30,FortuneInk);
            var actionSprite=Sp("ad_btn_encounter");
            float actionWidth=680f,actionHeight=actionSprite?actionWidth*actionSprite.rect.height/Mathf.Max(1f,actionSprite.rect.width):180f;
            var action=NewRect("Action",content);action.anchorMin=action.anchorMax=new Vector2(.5f,.5f);action.anchoredPosition=new Vector2(0,-560);action.sizeDelta=new Vector2(actionWidth,actionHeight);
            var actionImage=action.gameObject.AddComponent<Image>();actionImage.sprite=actionSprite;actionImage.preserveAspect=true;
            if(!actionImage.sprite)actionImage.color=new Color(.2f,.75f,.28f);
            encounterAction=action.gameObject.AddComponent<Button>();encounterAction.targetGraphic=actionImage;
            encounterActionLabel=Words("Label",action,new Vector2(0,8),new Vector2(actionWidth*.72f,actionHeight*.5f),48,Color.white);
            var close=NewRect("Close",content);close.anchorMin=close.anchorMax=new Vector2(.5f,.5f);close.anchoredPosition=new Vector2(430,500);close.sizeDelta=new Vector2(128,128);
            var closeImage=close.gameObject.AddComponent<Image>();closeImage.sprite=Sp("talent_btn_back@415814fc");closeImage.preserveAspect=true;
            encounterClose=close.gameObject.AddComponent<Button>();encounterClose.targetGraphic=closeImage;
            Click(encounterDim,()=>{if(encounterPhase!=EncounterPhase.Lucky)DismissEncounter();});
            Click(encounterClose,()=>{if(encounterPhase!=EncounterPhase.Lucky)DismissEncounter();});
            Click(encounterAction,OnEncounterAction);
            content.localScale=Vector3.one*.7f;
            StartCoroutine(Tween(.28f,t=>{if(content)content.localScale=Vector3.one*Mathf.LerpUnclamped(.7f,1f,OutBack(t));}));
            ApplyEncounterPhase();
        }

        void ApplyEncounterPhase() {
            if(!encounterRoot||encounterOffer==null)return;
            bool diamond=encounterOffer.Diamond,lucky=encounterPhase==EncounterPhase.Lucky,fail=encounterPhase==EncounterPhase.Fail;
            string character=lucky
                ?(diamond?"ad_pic_investGetDiamond_scale2.png":"ad_pic_investGetMoney_scale2")
                :(fail?(diamond?"ad_pic_investLostDiamond_scale2":"ad_pic_investLostMoney_scale2.png"):"ad_pic_investAsk_scale2");
            var sprite=Sp(character);
            encounterCharacter.sprite=sprite;encounterCharacter.enabled=sprite;
            if(sprite){float height=520f*sprite.rect.height/Mathf.Max(1f,sprite.rect.width);encounterCharacter.rectTransform.sizeDelta=new Vector2(520f,height);}
            encounterLight.enabled=lucky&&encounterLight.sprite;
            var money=RewardIcon(diamond?2:1);encounterCurrency.sprite=money;encounterCurrency.enabled=money;
            Txt(encounterAmount,GameModel.Format(lucky?encounterPayout:encounterStake));
            if(lucky)Txt(encounterBody,"你运气真好,双倍奖励直接奉上");
            else if(fail)Txt(encounterBody,Filled("encounter.refund",GameModel.Format(encounterStake)));
            else Txt(encounterBody,Filled(diamond?"original.694":"original.531",GameModel.Format(encounterStake)));
            On(encounterOdds.gameObject,encounterPhase==EncounterPhase.Gamble);
            if(encounterPhase==EncounterPhase.Gamble) {
                string odds=L10n.Translate(L10n.Source("original.475")).Replace("{Mathf.RoundToInt(Constants.RandomEventWinRate * 100)}",Mathf.RoundToInt(Model.EncounterWinRate*100).ToString());
                Txt(encounterOdds,odds);
            }
            On(encounterClose.gameObject,!lucky);
            encounterDim.interactable=!lucky;
            Txt(encounterActionLabel,lucky?"领取":fail?"要回本钱":"投入");
        }

        /// <summary>Translate while the placeholder is still intact, then drop the number in.</summary>
        static string Filled(string key,string value)=>L10n.Translate(L10n.Source(key)).Replace("xx",value);

        void OnEncounterAction() {
            if(encounterPhase==EncounterPhase.Gamble)BetEncounter();
            else if(encounterPhase==EncounterPhase.Fail)RepayEncounter();
            else if(encounterPhase==EncounterPhase.Lucky)ClaimEncounter();
        }

        double DiamondStake() {
            double held=Model.Data.diamonds;if(held<=0)return 0;
            return Math.Min(60d,Math.Floor((float)held*.6f));
        }

        void BetEncounter() {
            if(encounterPhase!=EncounterPhase.Gamble||encounterOffer==null)return;
            double stake;
            if(encounterOffer.Diamond) {
                stake=DiamondStake();encounterStake=stake;
                if(stake<=0||!Model.TrySpendDiamonds(stake)){PlayCantBuy();ShowToast("钻石不足");return;}
            } else {
                stake=encounterOffer.Stake;encounterStake=stake;
                if(stake<=0)return;
                if(!Model.TrySpendGold(stake)){PlayCantBuy();ShowToast("金币不足");return;}
            }
            bool win=UnityEngine.Random.value<Model.EncounterWinRate;
            if(win) {
                if(encounterOffer.Diamond) {
                    long count=(long)Math.Floor(encounterStake);
                    encounterPayout=count>long.MaxValue/2?long.MaxValue:count*2L;
                } else encounterPayout=encounterStake*2d;
                encounterPhase=EncounterPhase.Lucky;
            } else encounterPhase=EncounterPhase.Fail;
            ApplyEncounterPhase();
        }

        void ClaimEncounter() {
            if(encounterPhase!=EncounterPhase.Lucky||encounterOffer==null)return;
            bool diamond=encounterOffer.Diamond;double payout=encounterPayout;
            encounterPhase=EncounterPhase.None;
            DismissEncounter();
            if(diamond)Model.AddEncounterDiamonds(payout);else Model.AddEncounterGold(payout);
            if(diamond)PlayReward();else PlaySupportEffect(0);
        }

        void RepayEncounter() {
            if(encounterPhase!=EncounterPhase.Fail||encounterAd)return;
            encounterAd=true;
            if(AdManager.Instance!=null) {
                AdManager.Instance.ShowRewarded(watched=>{
                    if(watched)StartCoroutine(OriginalAdAward(PayEncounterRefund));
                    else {encounterAd=false;DismissEncounter();}
                });
                return;
            }
            ShowOriginalAd(PayEncounterRefund);
        }

        void PayEncounterRefund() {
            encounterAd=false;
            if(encounterOffer==null)return;
            bool diamond=encounterOffer.Diamond;double stake=encounterStake;
            encounterPhase=EncounterPhase.None;
            DismissEncounter();
            if(diamond)Model.AddEncounterDiamonds(stake);else Model.AddEncounterGold(stake);
            if(diamond)PlayReward();else PlaySupportEffect(0);
        }

        void CloseEncounterFromBack() {
            if(encounterAd)return;
            if(encounterPhase==EncounterPhase.Lucky)ClaimEncounter();
            else DismissEncounter();
        }

        void DismissEncounter() {
            encounterPhase=EncounterPhase.None;encounterOffer=null;encounterAd=false;
            if(encounterRoot){popups.Remove(encounterRoot);Destroy(encounterRoot);}
            encounterRoot=null;
            if(Model!=null)Model.ClearEncounter();
        }
    }
}

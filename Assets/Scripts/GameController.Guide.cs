using System.Collections;
using Bufu.Rebuild.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    /// <summary>
    /// First-session guide in the original order (MainSceneVat / UpgradePanel guide_step_1..3): toss a coin and see
    /// tails, toss again and see heads, then hire the first helper once 25 gold is in hand. Not in the original:
    /// a "Your Fortune Begins!" popup with a starter gift closes the guide.
    /// Uses the original finger Spine and Tycoon dialog card. A dim cover with a hole keeps the tap on the target.
    /// </summary>
    public sealed partial class GameController {
        const string GuideStepKey="guide_step";
        const int GuideToss=1,GuideTossAgain=2,GuideHire=3,GuideCelebrate=4,GuideDone=5;
        const double GuideHelperGold=25;const int GuideStarterDiamonds=20;
        int guideStep=GuideDone;CoinActor guideCoin;Coroutine guideRoutine,guideFollow;
        RectTransform guideLayer,guideHole,guideGlow,guideFinger;UiView guideDialog;readonly Image[] guideShades=new Image[4];
        Image guideHoleImage,guideGlowImage;static Sprite guideRoundCut,guideRoundRing,guideBoxCut,guideBoxRing;
        static readonly Color GuideDim=new Color(0,0,0,.55f),GuideGold=new Color(1f,.84f,.32f,1f);

        bool GuideActive=>guideStep<GuideDone;

        void StartGuide() {
            int saved=Mathf.Clamp(PlayerPrefs.GetInt(PrefKey(GuideStepKey),GuideToss),GuideToss,GuideDone);
            // The celebration is owed after the hire, so a restart before it still shows it.
            bool fresh=Model.Data.world==1&&!VerificationMode&&!Model.OpenAccess&&Model.MainLevel<=3&&(Model.Helpers==0||saved==GuideCelebrate);
            guideStep=fresh?saved:GuideDone;
            if(GuideActive)guideRoutine=StartCoroutine(GuideStepRoutine());
        }
        void ResetGuide() {
            if(guideRoutine!=null)StopCoroutine(guideRoutine);
            HideGuide();guideCoin=null;
            if(!VerificationMode){PlayerPrefs.DeleteKey(PrefKey(GuideStepKey));PlayerPrefs.Save();}
            StartGuide();
        }
        void SaveGuideStep(int step) {
            guideStep=step;
            if(!VerificationMode){PlayerPrefs.SetInt(PrefKey(GuideStepKey),step);PlayerPrefs.Save();}
        }

        /// <summary>CoinBase guide_step_2_count: the first guided toss lands tails, the second lands heads.</summary>
        public bool GuideForcedFace(CoinActor coin,out int face) {
            face=guideStep==GuideToss?0:1;
            return guideStep==GuideToss||guideStep==GuideTossAgain&&coin==guideCoin;
        }
        public void GuideCoinTossed(CoinActor coin) {
            if(guideStep==GuideToss){guideCoin=coin;SaveGuideStep(GuideTossAgain);HideGuide();}
            else if(guideStep==GuideTossAgain&&coin==guideCoin){guideCoin=null;SaveGuideStep(GuideHire);HideGuide();}
        }
        public void GuideCoinLanded(CoinActor coin) {
            if(guideStep==GuideTossAgain&&coin==guideCoin)PointGuideAtCoin(coin,"反面没有收益，正面才有钱哦");
            else if(guideStep==GuideHire&&guideRoutine==null) {
                ShowGuideDialog("赚到钱啦！接下来先赚"+GameModel.Format(GuideHelperGold)+"块钱",null,()=>guideRoutine=StartCoroutine(GuideStepRoutine()));
            }
        }

        IEnumerator GuideStepRoutine() {
            while(!interfaceReady||ModalOpen)yield return null;
            yield return new WaitForSecondsRealtime(.4f);
            if(guideStep==GuideToss||guideStep==GuideTossAgain) {
                CoinActor coin=null;
                while(!coin){coin=Coins.Find(x=>x&&x.Quality==1&&!x.Busy);if(!coin)yield return null;}
                guideCoin=coin;
                PointGuideAtCoin(coin,"点击抛起铜币");
                guideRoutine=null;yield break;
            }
            if(guideStep==GuideCelebrate){yield return CelebrateGuide();yield break;}
            while(Model.State.gold<GuideHelperGold||ModalOpen)yield return null;
            if(Model.Helpers==0) {
                if(tab!=1)SelectTab(1);
                if(subTab!=1)OnSubTab(1);
                var content=subContents[0];content.anchoredPosition=new Vector2(content.anchoredPosition.x,0);
                Canvas.ForceUpdateCanvases();
                // Item cards are rebuilt on refresh, so find the button again whenever the old one is gone.
                Button button=null;
                while(Model.Helpers==0) {
                    if(!button&&(button=HelperUpgradeButton())) {
                        PointGuideAt((RectTransform)button.transform,false);
                        ShowGuideDialog("现在该雇佣一个帮手，为你打工啦！",guideHole,null);
                    }
                    yield return null;
                }
            }
            SaveGuideStep(GuideCelebrate);HideGuide();
            // Let the new helper walk in and make its first toss before the popup covers the field.
            yield return new WaitForSecondsRealtime(1.5f);
            yield return CelebrateGuide();
        }
        IEnumerator CelebrateGuide() {
            while(!interfaceReady||ModalOpen||Model.Data.world!=1)yield return null;
            ShowFortunePopup();guideRoutine=null;
        }
        Button HelperUpgradeButton() {
            var item=subContents[0].Find("item0_"+(Model.BaseId+1));if(!item)return null;
            foreach(var b in item.GetComponentsInChildren<Button>(true))if(b.name=="upgradeBtn")return b;
            return null;
        }

        /// <summary>Hidden while a popup (goal, offline, settings) is open, so the cover never sits on top of it.</summary>
        void UpdateGuide(){if(guideLayer)guideLayer.gameObject.SetActive(GuideActive&&!ModalOpen&&Model.Data.world==1);}

        void PointGuideAtCoin(CoinActor coin,string message) {
            PointGuideAt(coin.Rect,true);
            ShowGuideDialog(message,guideHole,null);
        }
        void PointGuideAt(RectTransform target,bool round) {
            EnsureGuideLayer();
            if(guideFollow!=null)StopCoroutine(guideFollow);
            guideFinger.gameObject.SetActive(true);guideHole.gameObject.SetActive(true);
            guideHoleImage.sprite=round?guideRoundCut:guideBoxCut;guideHoleImage.type=round?Image.Type.Simple:Image.Type.Sliced;
            guideGlowImage.sprite=round?guideRoundRing:guideBoxRing;guideGlowImage.type=guideHoleImage.type;
            guideFollow=StartCoroutine(FollowGuideTarget(target,round));
        }
        IEnumerator FollowGuideTarget(RectTransform target,bool round) {
            var corners=new Vector3[4];
            float pulse=0;
            while(target&&guideHole.gameObject.activeSelf) {
                PlaceGuideHole(target,round,corners);
                // The gold ring breathes outward and fades, like the original click highlight.
                pulse=(pulse+Time.unscaledDeltaTime/1.1f)%1f;
                guideGlow.sizeDelta=guideHole.sizeDelta+Vector2.one*Mathf.Lerp(0,56,pulse);
                guideGlowImage.color=new Color(1,1,1,1-pulse);
                yield return null;
            }
        }
        void PlaceGuideHole(RectTransform target,bool round,Vector3[] corners) {
            target.GetWorldCorners(corners);
            Vector2 min=guideLayer.InverseTransformPoint(corners[0]),max=guideLayer.InverseTransformPoint(corners[2]);
            var size=max-min;size=round?Vector2.one*Mathf.Max(size.x,size.y)*1.2f:size+new Vector2(28,28);
            guideHole.anchoredPosition=(min+max)*.5f;guideHole.sizeDelta=size;
            // The Spine origin is the fingertip, so the finger sits on the target as in the original.
            guideFinger.anchoredPosition=guideHole.anchoredPosition;
            SetGuideShade(true);
        }

        /// <summary>DialogPanel with the Tycoon card. With onClose it covers the screen until tapped; otherwise taps pass through.</summary>
        void ShowGuideDialog(string message,RectTransform avoid,System.Action onClose) {
            EnsureGuideLayer();
            if(guideDialog==null){guideDialog=UiPrefab.Instantiate("Common/dialogPanel",guideLayer);GuideStretch(guideDialog.Rect);}
            guideDialog.Root.SetActive(true);guideDialog.Root.transform.SetAsLastSibling();
            Txt(guideDialog.Ref<TMP_Text>("DialogPanel","dialogText"),message);
            var card=guideDialog.Ref<RectTransform>("DialogPanel","textParent");
            float half=card.rect.height*Mathf.Abs(card.localScale.y)*.5f,room=guideLayer.rect.height*.5f;
            float y=guideDialog.Script("DialogPanel").Obj("bottomPosition").F("y");
            if(avoid&&avoid.gameObject.activeSelf) {
                // Keep the card off the target: below it when there is room, otherwise above.
                float gap=avoid.sizeDelta.y*.5f+half+40f,cy=avoid.anchoredPosition.y;
                y=cy-gap-half>-room?cy-gap:cy+gap;
            }
            card.anchoredPosition=new Vector2(0,Mathf.Clamp(y,-room+half,room-half));
            var mask=guideDialog.Ref<Image>("DialogPanel","backgroundImage");
            var color=mask.color;color.a=onClose!=null?.55f:0f;mask.color=color;mask.raycastTarget=onClose!=null;
            var button=guideDialog.Ref<Button>("DialogPanel","bgButton");button.onClick.RemoveAllListeners();
            if(onClose!=null) {
                SetGuideShade(false);
                button.onClick.AddListener(()=>{PlayClick();guideDialog.Root.SetActive(false);onClose();});
            }
        }

        void HideGuide() {
            if(guideFollow!=null){StopCoroutine(guideFollow);guideFollow=null;}
            if(!guideLayer)return;
            guideFinger.gameObject.SetActive(false);guideHole.gameObject.SetActive(false);
            if(guideDialog!=null&&guideDialog.Root)guideDialog.Root.SetActive(false);
            SetGuideShade(false);
        }

        void EnsureGuideLayer() {
            if(guideLayer)return;
            guideLayer=NewRect("Guide",systemLayer);GuideStretch(guideLayer);guideLayer.SetAsLastSibling();
            for(int i=0;i<4;i++) {
                var rect=NewRect("Shade",guideLayer);rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.pivot=Vector2.zero;
                var image=rect.gameObject.AddComponent<Image>();image.color=GuideDim;image.raycastTarget=true;guideShades[i]=image;
            }
            if(!guideRoundCut) {
                guideRoundCut=GuideCutout(192,96,true);guideRoundRing=GuideCutout(192,96,false);
                guideBoxCut=GuideCutout(96,30,true);guideBoxRing=GuideCutout(96,30,false);
            }
            guideHole=NewRect("Hole",guideLayer);guideHole.anchorMin=guideHole.anchorMax=new Vector2(.5f,.5f);
            guideHoleImage=guideHole.gameObject.AddComponent<Image>();guideHoleImage.raycastTarget=false;
            guideGlow=NewRect("Glow",guideHole);guideGlow.anchorMin=guideGlow.anchorMax=new Vector2(.5f,.5f);
            guideGlowImage=guideGlow.gameObject.AddComponent<Image>();guideGlowImage.raycastTarget=false;
            guideFinger=NewRect("Finger",guideLayer);guideFinger.anchorMin=guideFinger.anchorMax=new Vector2(.5f,.5f);guideFinger.sizeDelta=new Vector2(100,100);
            guideFinger.gameObject.AddComponent<CanvasRenderer>();
            var spine=guideFinger.gameObject.AddComponent<SpineGraphic>();
            if(spine.Load("Uieff_common_guide"))spine.Play("dianji",true);
            guideFinger.gameObject.SetActive(false);guideHole.gameObject.SetActive(false);
            SetGuideShade(false);
        }
        /// <summary>Four shades around the hole. Taps inside the hole reach the coin or button underneath.</summary>
        void SetGuideShade(bool on) {
            if(!guideLayer)return;
            var full=guideLayer.rect;bool hole=on&&guideHole.gameObject.activeSelf;
            var gap=new Rect(guideHole.anchoredPosition-guideHole.sizeDelta*.5f,guideHole.sizeDelta);
            var parts=hole
                ?new[]{Rect.MinMaxRect(full.xMin,gap.yMax,full.xMax,full.yMax),Rect.MinMaxRect(full.xMin,full.yMin,full.xMax,gap.yMin),
                       Rect.MinMaxRect(full.xMin,gap.yMin,gap.xMin,gap.yMax),Rect.MinMaxRect(gap.xMax,gap.yMin,full.xMax,gap.yMax)}
                :new[]{full,Rect.zero,Rect.zero,Rect.zero};
            for(int i=0;i<4;i++) {
                var rect=guideShades[i].rectTransform;rect.anchoredPosition=parts[i].min;rect.sizeDelta=parts[i].size;
                guideShades[i].gameObject.SetActive(on&&parts[i].width>0&&parts[i].height>0);
            }
        }
        /// <summary>
        /// The hole fill: transparent inside a rounded shape with a gold rim, dimmed in the corners outside it,
        /// so the square gap between the four shades reads as a soft round spotlight. Without dim it is the rim alone.
        /// </summary>
        static Sprite GuideCutout(int size,float radius,bool dim) {
            const float rim=7f;
            var tex=new Texture2D(size,size,TextureFormat.RGBA32,false){wrapMode=TextureWrapMode.Clamp};
            var pixels=new Color[size*size];float half=size*.5f;
            for(int y=0;y<size;y++)for(int x=0;x<size;x++) {
                float qx=Mathf.Abs(x+.5f-half)-(half-radius),qy=Mathf.Abs(y+.5f-half)-(half-radius);
                float edge=new Vector2(Mathf.Max(qx,0),Mathf.Max(qy,0)).magnitude+Mathf.Min(Mathf.Max(qx,qy),0)-radius;
                float inside=Mathf.Clamp01(.5f-edge),gold=inside*Mathf.Clamp01(edge+rim+.5f),shade=dim?(1-inside)*GuideDim.a:0;
                pixels[y*size+x]=new Color(GuideGold.r,GuideGold.g,GuideGold.b,gold)*(gold/Mathf.Max(.0001f,gold+shade))
                    +new Color(0,0,0,shade)*(shade/Mathf.Max(.0001f,gold+shade));
                pixels[y*size+x].a=gold+shade;
            }
            tex.SetPixels(pixels);tex.Apply();
            float border=radius+rim;
            return Sprite.Create(tex,new Rect(0,0,size,size),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(border,border,border,border));
        }
        static void GuideStretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
    }
}

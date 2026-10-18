using System.Collections;
using Bufu.Rebuild.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    /// <summary>
    /// The guide's closing popup, built on the original congratulationPanel: spinning light, a gold title,
    /// the Tycoon on the card, a starter gift and a coin shower. The gift is granted when the popup opens,
    /// so the Android back key (which closes the top popup) cannot skip it.
    /// </summary>
    public sealed partial class GameController {
        const string FortuneTitleArt="ty_title_kaishizhifu";
        static readonly Color FortuneInk=new Color(.36f,.2f,.08f,1f);
        static Sprite fortuneTycoon,fortuneButton;

        void ShowFortunePopup() {
            const string cls="RewardBoxUI";
            Model.GrantItem(2,GuideStarterDiamonds);SaveGuideStep(GuideDone);
            if(!VerificationMode)SaveStore.Save(Model);
            RefreshAll();PlayReward();

            var v=OpenPopup("Popup/congratulationPanel");v.Root.name="fortunePanel";
            On(v.Ref(cls,"closeButton"),false);On(v.Ref(cls,"doubleButton"),false);
            var font=UiAssets.Font();
            RectTransform Node(string name)=>(RectTransform)v.Node(name).transform;
            TMP_Text Label(string name,Transform parent,Vector2 position,Vector2 size,float fontSize,Color color,string source) {
                var rect=NewRect(name,parent);rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.anchoredPosition=position;rect.sizeDelta=size;
                var text=rect.gameObject.AddComponent<TextMeshProUGUI>();if(font)text.font=font;
                text.fontSize=fontSize;text.color=color;text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false;
                Txt(text,source);return text;
            }

            // Title: the original art reads "REWARDS!". Drawn title art (UI/Sprites/en/ty_title_kaishizhifu.png) is used when present;
            // otherwise the words are set in the game font with the same gold and brown outline.
            var title=Node("Title");title.anchoredPosition=new Vector2(0,560);
            var titleImage=title.GetComponent<Image>();var titleArt=UiAssets.Sprite(FortuneTitleArt);
            if(titleArt) {
                titleImage.sprite=titleArt;titleImage.preserveAspect=true;
                float fit=Mathf.Min(1300f/titleArt.rect.width,420f/titleArt.rect.height);title.sizeDelta=titleArt.rect.size*fit;
            } else {
                titleImage.enabled=false;
                var heading=Label("Text",title,Vector2.zero,new Vector2(1300,330),132,Color.white,L10n.Source("guide.fortune.title"));
                heading.fontStyle=FontStyles.UpperCase;heading.lineSpacing=-18;
                heading.enableVertexGradient=true;heading.colorGradient=new VertexGradient(new Color(1f,.98f,.5f),new Color(1f,.98f,.5f),new Color(1f,.7f,.08f),new Color(1f,.7f,.08f));
                InkOutline(heading,15f);
            }
            foreach(var light in new[]{"TitleBgLight","TitleBgLight2"})Node(light).anchoredPosition=new Vector2(0,560);

            // Card: the Tycoon rises from its lower left, the message sits to his right.
            var card=Node("AdBg");card.anchoredPosition=new Vector2(0,150);
            var tycoon=NewRect("Tycoon",v.Root.transform);tycoon.SetSiblingIndex(card.GetSiblingIndex()+1);
            tycoon.anchorMin=tycoon.anchorMax=new Vector2(.5f,.5f);tycoon.pivot=new Vector2(.5f,0f);
            var tycoonImage=tycoon.gameObject.AddComponent<Image>();tycoonImage.sprite=FortuneTycoon();tycoonImage.raycastTarget=false;
            if(tycoonImage.sprite){tycoon.sizeDelta=new Vector2(382,365);tycoon.anchoredPosition=new Vector2(-500,150-212+8);}else tycoonImage.enabled=false;
            var message=Label("Message",v.Root.transform,new Vector2(170,150),new Vector2(980,360),52,Color.white,L10n.Source("guide.fortune.body"));
            message.transform.SetSiblingIndex(tycoon.GetSiblingIndex()+1);message.lineSpacing=8;

            // Starter gift: one reward cell, as ShowRewardBox builds them.
            InkOutline(Label("Gift",v.Root.transform,new Vector2(0,-95),new Vector2(800,90),58,new Color(1f,.86f,.3f),L10n.Source("guide.fortune.gift")),5f);
            var template=v.Ref(cls,"item1");var gifts=v.Ref<RectTransform>(cls,"con");gifts.anchoredPosition=new Vector2(0,-330);
            var gift=Instantiate(template,gifts,false);gift.name="reward2";
            var giftRect=(RectTransform)gift.transform;giftRect.anchorMin=giftRect.anchorMax=new Vector2(.5f,.5f);giftRect.localScale=template.transform.localScale;
            var icon=gift.transform.Find("Icon").GetComponent<Image>();var diamond=RewardIcon(2);if(diamond){icon.sprite=diamond;icon.SetNativeSize();}
            Txt(gift.transform.Find("countTmp").GetComponent<TMP_Text>(),"x"+GuideStarterDiamonds);
            gift.SetActive(true);On(template,false);

            // Button: the green one has "Confirm" painted in, so use the blank yellow one with a label.
            var start=v.Ref<Button>(cls,"GetButton");var startRect=(RectTransform)start.transform;
            startRect.parent.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,-620);
            var startImage=start.GetComponent<Image>();var blank=FortuneButton();
            if(blank){startImage.sprite=blank;startImage.type=Image.Type.Sliced;}
            startRect.sizeDelta=new Vector2(560,142);
            var layout=startRect.GetComponent<LayoutElement>()??startRect.gameObject.AddComponent<LayoutElement>();layout.preferredWidth=560;layout.preferredHeight=142;
            // Dark brown on the yellow face reads at a glance; the font cannot draw outlines (see InkOutline).
            Label("Label",startRect,new Vector2(0,8),new Vector2(500,120),56,FortuneInk,L10n.Source("guide.fortune.start"));
            Click(start,()=>ClosePopup(v));

            StartCoroutine(FortuneMotion(v));
        }

        /// <summary>Pop in (0.6 to 1.08 to 1), spin the title light, and drop a shower of coins behind the card.</summary>
        IEnumerator FortuneMotion(UiView v) {
            var root=v.Root.transform;var light=v.Node("TitleBgLight").transform;var glow=v.Node("TitleBgLight2").transform;
            var rain=NewRect("Coin Shower",root);rain.SetSiblingIndex(2);rain.anchorMin=rain.anchorMax=new Vector2(.5f,.5f);
            var coinSprite=RewardIcon(1);const int count=28;
            var coins=new RectTransform[count];var velocity=new Vector2[count];var spin=new float[count];var wait=new float[count];
            for(int i=0;i<count&&coinSprite;i++) {
                var coin=NewRect("Coin",rain);coin.anchorMin=coin.anchorMax=new Vector2(.5f,.5f);
                var image=coin.gameObject.AddComponent<Image>();image.sprite=coinSprite;image.SetNativeSize();image.raycastTarget=false;
                coin.localScale=Vector3.one*Random.Range(.55f,1f);coin.anchoredPosition=new Vector2(Random.Range(-680f,680f),Random.Range(1350f,1650f));
                coin.localEulerAngles=new Vector3(0,0,Random.Range(0f,360f));
                velocity[i]=new Vector2(Random.Range(-90f,90f),Random.Range(-700f,-400f));spin[i]=Random.Range(-240f,240f);wait[i]=Random.Range(0f,1f);
                coins[i]=coin;coin.gameObject.SetActive(false);
            }
            float t=0;
            while(root) {
                float dt=Time.unscaledDeltaTime;t+=dt;
                float p=Mathf.Clamp01(t/.35f);
                root.localScale=Vector3.one*(p<.6f?Mathf.Lerp(.6f,1.08f,p/.6f):Mathf.Lerp(1.08f,1f,(p-.6f)/.4f));
                light.localEulerAngles=new Vector3(0,0,-t*24f);
                glow.localScale=Vector3.one*10f*(1f+.06f*Mathf.Sin(t*3f));
                bool falling=false;
                for(int i=0;i<count;i++) {
                    var coin=coins[i];if(!coin)continue;
                    if(t<wait[i]){falling=true;continue;}
                    if(!coin.gameObject.activeSelf)coin.gameObject.SetActive(true);
                    velocity[i].y-=1800f*dt;coin.anchoredPosition+=velocity[i]*dt;coin.Rotate(0,0,spin[i]*dt);
                    if(coin.anchoredPosition.y<-1700f){Destroy(coin.gameObject);coins[i]=null;}else falling=true;
                }
                if(!falling&&rain)Destroy(rain.gameObject);
                yield return null;
            }
        }

        /// <summary>
        /// hdhSDF has a 1 px atlas padding, so TMP outlines cannot grow past the glyph; the original bakes outlined titles
        /// into art for that reason. Eight brown copies behind the text, offset around it, draw the outline instead.
        /// </summary>
        static void InkOutline(TMP_Text text,float width) {
            text.ForceMeshUpdate();
            var rect=text.rectTransform;int at=rect.GetSiblingIndex();
            for(int i=0;i<8;i++) {
                float angle=i*Mathf.PI/4f;
                var copy=NewRect(text.name+" Ink",rect.parent);copy.SetSiblingIndex(at);
                copy.anchorMin=rect.anchorMin;copy.anchorMax=rect.anchorMax;copy.pivot=rect.pivot;copy.sizeDelta=rect.sizeDelta;
                copy.anchoredPosition=rect.anchoredPosition+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*width;
                var ink=copy.gameObject.AddComponent<TextMeshProUGUI>();ink.font=text.font;ink.raycastTarget=false;
                ink.enableAutoSizing=false;ink.fontSize=text.fontSize;ink.fontStyle=text.fontStyle;ink.alignment=text.alignment;
                ink.lineSpacing=text.lineSpacing;ink.enableWordWrapping=text.enableWordWrapping;ink.color=FortuneInk;ink.text=text.text;
            }
        }

        /// <summary>The Tycoon from the guide dialog card, cut off just above the card's gold rim.</summary>
        static Sprite FortuneTycoon() {
            if(fortuneTycoon)return fortuneTycoon;
            var source=UiAssets.Sprite("ty_pic_dialogBg_scale2");if(!source)return null;
            // Measured on the 544×420 art: the figure spans x 20..245 and the rim starts 215 px from the top.
            var area=source.textureRect;float sx=area.width/544f,sy=area.height/420f;
            var cut=new Rect(area.x+20*sx,area.y+(420-215)*sy,225*sx,215*sy);
            fortuneTycoon=Sprite.Create(source.texture,cut,new Vector2(.5f,0f),source.pixelsPerUnit);
            return fortuneTycoon;
        }
        /// <summary>The blank yellow button, re-sliced so it can widen for a longer label without stretching its corners.</summary>
        static Sprite FortuneButton() {
            if(fortuneButton)return fortuneButton;
            var source=UiAssets.Sprite("ty_btn_yellow01");if(!source)return null;
            fortuneButton=Sprite.Create(source.texture,source.textureRect,new Vector2(.5f,.5f),source.pixelsPerUnit,0,SpriteMeshType.FullRect,new Vector4(56,56,56,56));
            return fortuneButton;
        }
    }
}

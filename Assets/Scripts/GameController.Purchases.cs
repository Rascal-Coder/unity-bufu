using System;
using System.Collections.Generic;
using System.Linq;
using Bufu.Rebuild.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    public sealed partial class GameController {
        PurchaseService purchases;
        GameObject purchaseView,purchaseReturnFocus;
        RectTransform purchaseContent;ScrollRect purchaseScroll;
        TMP_Text purchaseBalance,purchaseStatus,purchaseBuyLabel;
        Button purchaseBuy,purchaseRestore,purchaseReconnect;
        readonly Dictionary<string,Button> purchaseCards=new Dictionary<string,Button>(),purchaseActions=new Dictionary<string,Button>(),purchaseDetails=new Dictionary<string,Button>();
        readonly Dictionary<string,TMP_Text> purchaseCardCopy=new Dictionary<string,TMP_Text>(),purchaseCardStock=new Dictionary<string,TMP_Text>();
        readonly Dictionary<StoreCategory,Button> shopPurchaseEntries=new Dictionary<StoreCategory,Button>();
        readonly Dictionary<GameObject,bool> purchaseShopVisibility=new Dictionary<GameObject,bool>();
        StoreCategory purchaseCategory;PurchaseProduct selectedPurchase;
        static readonly Color StoreWhite=new Color(1,.96f,.85f),StoreGold=new Color(1,.77f,.29f),StoreInk=new Color(.28f,.14f,.06f);
        static readonly Dictionary<string,Sprite> storeFrames=new Dictionary<string,Sprite>();

        void InitializePurchases() {
            purchases?.Dispose();
            purchases=new PurchaseService(Model,new UnavailableBillingGateway(),new RejectPurchaseVerifier(),()=>SaveStore.Save(Model));
            purchases.Changed+=RefreshPurchaseStore;purchases.Connect();
        }
        void BindPurchaseEntry() {
            var plus=StoreButton(top.Node("diamond").transform,"Open Purchase Store","+",ShowPurchaseStore);
            var rect=(RectTransform)plus.transform;rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
            rect.anchoredPosition=new Vector2(420,0);rect.sizeDelta=new Vector2(116,116);
            plus.GetComponent<Image>().sprite=StoreFrame("ty_btn_yellow01",24);
        }
        void BindShopPurchaseEntry(UiView shop) {
            var categories=new[]{StoreCategory.NoAds,StoreCategory.Diamonds,StoreCategory.Fragments};
            string[] labels={"label-no-ads-native-v3","label-diamonds-native-v3","label-fragments-native-v3"},icons={"art:no-ads-pass-native-v2","art:diamond-vault-native-v2","art:fragment-pack-native-v2"};
            for(int i=0;i<categories.Length;i++) {
                var category=categories[i];var rect=NewRect("Shop Offer "+category,shop.Root.transform);
                rect.anchorMin=rect.anchorMax=new Vector2(1,0);rect.anchoredPosition=new Vector2(-132,1900-i*300);rect.sizeDelta=new Vector2(236,260);
                var hit=rect.gameObject.AddComponent<Image>();hit.color=Color.clear;
                var button=rect.gameObject.AddComponent<Button>();button.targetGraphic=hit;Click(button,()=>ShowShopOffer(category));
                var icon=StoreArt(rect,"Offer Icon",icons[i],new Vector2(118,20),new Vector2(236,216));
                var caption=NewRect("Caption",rect);caption.anchorMin=caption.anchorMax=new Vector2(.5f,0);caption.anchoredPosition=new Vector2(0,14);caption.sizeDelta=new Vector2(236,80);
                var captionImage=caption.gameObject.AddComponent<Image>();captionImage.sprite=StoreIllustration("art:"+labels[i]);captionImage.preserveAspect=true;captionImage.raycastTarget=false;
                var focus=caption.gameObject.AddComponent<Outline>();focus.effectColor=StoreGold;focus.effectDistance=new Vector2(3,-3);focus.enabled=false;
                button.targetGraphic=captionImage;var colors=button.colors;colors.selectedColor=Color.white;colors.pressedColor=new Color(.85f,.85f,.85f);button.colors=colors;
                rect.gameObject.AddComponent<ShopOfferMotion>().Initialize(icon,i*2.4f,focus);
                shopPurchaseEntries.Add(category,button);
            }
        }
        static Sprite StoreFrame(string key,float border=24) {
            if(storeFrames.TryGetValue(key,out var frame))return frame;
            var source=Sp(key);if(!source)return null;
            frame=Sprite.Create(source.texture,source.textureRect,new Vector2(.5f,.5f),source.pixelsPerUnit,0,SpriteMeshType.FullRect,new Vector4(border,border,border,border));
            storeFrames[key]=frame;return frame;
        }
        static Sprite StoreIllustration(string key) {
            if(!key.StartsWith("art:"))return Sp(key);
            if(storeFrames.TryGetValue(key,out var cached))return cached;
            if(PackedSpriteSheets.TryGet("CommerceArt/"+key.Substring(4),out var packed)){storeFrames[key]=packed;return packed;}
            var texture=Resources.Load<Texture2D>("CommerceArt/"+key.Substring(4));if(!texture)return null;
            var sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f),100);storeFrames[key]=sprite;return sprite;
        }
        static Sprite StoreProductFrame() {
            const string key="Product Frame";if(storeFrames.TryGetValue(key,out var frame))return frame;
            var source=Sp("shop_btn_getItem");
            frame=Sprite.Create(source.texture,source.textureRect,new Vector2(.5f,.5f),source.pixelsPerUnit,0,SpriteMeshType.FullRect,new Vector4(22,116,22,22));
            storeFrames[key]=frame;return frame;
        }
        static Sprite StoreFooterFill() {
            const string key="Product Footer Fill";if(storeFrames.TryGetValue(key,out var fill))return fill;
            var source=Sp("shop_btn_getItem");var area=source.textureRect;
            fill=Sprite.Create(source.texture,new Rect(area.x+area.width*.5f,area.y+52,2,2),new Vector2(.5f,.5f),100);
            storeFrames[key]=fill;return fill;
        }
        static Sprite StoreControl(string asset) {
            string key="Control:"+asset;if(storeFrames.TryGetValue(key,out var cached))return cached;
            var art=StoreIllustration("art:"+asset);if(!art)return StoreFrame("ty_btn_yellow01",24);
            var frame=Sprite.Create(art.texture,art.rect,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(64,42,64,42));
            storeFrames[key]=frame;return frame;
        }
        static RectTransform StoreArt(Transform parent,string name,string sprite,Vector2 position,Vector2 size) {
            var rect=NewRect(name,parent);rect.anchorMin=rect.anchorMax=new Vector2(0,.5f);rect.anchoredPosition=position;rect.sizeDelta=size;
            var image=rect.gameObject.AddComponent<Image>();image.sprite=StoreIllustration(sprite);image.preserveAspect=true;image.raycastTarget=false;return rect;
        }
        static void StoreLayout(RectTransform rect,float height=-1,float flexible=0) {
            var layout=rect.gameObject.AddComponent<LayoutElement>();if(height>=0){layout.minHeight=height;layout.preferredHeight=height;}layout.flexibleHeight=flexible;
        }
        TMP_Text StoreText(Transform parent,string name,string copy,float height,float size=40) {
            var rect=NewRect(name,parent);StoreLayout(rect,height);
            var label=rect.gameObject.AddComponent<TextMeshProUGUI>();label.font=UiAssets.Font();label.fontSize=size;
            label.enableAutoSizing=true;label.fontSizeMin=size*.82f;label.fontSizeMax=size;label.color=StoreWhite;
            label.alignment=TextAlignmentOptions.Center;label.raycastTarget=false;label.richText=false;Txt(label,copy);return label;
        }
        TMP_Text StoreCardText(Transform parent,string name,string copy,Vector2 position,Vector2 size,float fontSize) {
            var rect=NewRect(name,parent);rect.anchorMin=rect.anchorMax=new Vector2(0,.5f);rect.anchoredPosition=position;rect.sizeDelta=size;
            var text=rect.gameObject.AddComponent<TextMeshProUGUI>();text.font=UiAssets.Font();text.fontSize=text.fontSizeMax=fontSize;text.fontSizeMin=fontSize*.85f;
            text.enableAutoSizing=true;text.enableWordWrapping=true;text.alignment=TextAlignmentOptions.MidlineLeft;text.color=StoreWhite;text.raycastTarget=false;Txt(text,copy);return text;
        }
        Button StoreButton(Transform parent,string name,string caption,Action action) {
            var rect=NewRect(name,parent);var image=rect.gameObject.AddComponent<Image>();image.sprite=StoreControl("shop-tab-native-v2");image.type=Image.Type.Sliced;
            var button=rect.gameObject.AddComponent<Button>();button.targetGraphic=image;
            var colors=button.colors;colors.highlightedColor=new Color(1,.96f,.83f);colors.selectedColor=new Color(1,.94f,.76f);colors.disabledColor=Color.white;button.colors=colors;
            var label=StoreText(rect,"Label",caption,0,44);Destroy(label.GetComponent<LayoutElement>());Stretch(label.rectTransform);
            label.color=StoreInk;label.margin=new Vector4(20,6,20,6);ButtonWordArt(label,caption);Click(button,action);return button;
        }
        RectTransform StoreRow(Transform parent,string name,float height) {
            var rect=NewRect(name,parent);StoreLayout(rect,height);var row=rect.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.spacing=20;row.childControlWidth=row.childControlHeight=true;row.childForceExpandWidth=row.childForceExpandHeight=true;return rect;
        }
        void ShowPurchaseStore()=>ShowShopOffer(StoreCategory.Diamonds);
        void ShowShopOffer(StoreCategory category) {
            if(purchaseView) {if(purchaseCategory==category){purchaseView.transform.SetAsLastSibling();return;}ClosePurchaseStore();}
            if(purchases==null)InitializePurchases();purchaseReturnFocus=EventSystem.current?EventSystem.current.currentSelectedGameObject:null;
            SelectTab(5);
            var overlay=NewRect("Purchase Store",shopView.Root.transform);Stretch(overlay);purchaseView=overlay.gameObject;popups.Add(purchaseView);
            // Shop is one page with two exclusive sections. Covering only the catalog area
            // leaves the old item row visible beneath the restore controls on tall screens.
            purchaseShopVisibility.Clear();
            foreach(Transform child in shopView.Root.transform) {
                if(child==overlay||child.name=="shopBg")continue;
                purchaseShopVisibility[child.gameObject]=child.gameObject.activeSelf;child.gameObject.SetActive(false);
            }
            overlay.offsetMin=new Vector2(28,410);overlay.offsetMax=new Vector2(-28,-212);
            var background=overlay.gameObject.AddComponent<Image>();background.sprite=Sp("shop_pic_scene01_scale2");
            Canvas.ForceUpdateCanvases();
            float bodyHeight=category==StoreCategory.NoAds?964:category==StoreCategory.Fragments?1752:1168;
            var panel=NewRect("Panel",overlay);panel.anchorMin=panel.anchorMax=new Vector2(.5f,.5f);panel.pivot=new Vector2(.5f,.5f);
            panel.anchoredPosition=Vector2.zero;panel.sizeDelta=new Vector2(overlay.rect.width-64,Mathf.Min(overlay.rect.height-48,bodyHeight+386));
            var layout=panel.gameObject.AddComponent<VerticalLayoutGroup>();layout.spacing=22;layout.childControlHeight=layout.childControlWidth=true;layout.childForceExpandHeight=false;layout.childForceExpandWidth=true;
            string titleArt=category==StoreCategory.NoAds?"title-no-ads-native-v3":category==StoreCategory.Fragments?"title-fragments-native-v3":"title-diamonds-native-v3";
            var header=StoreRow(panel,"Header",160);var headerLayout=header.GetComponent<HorizontalLayoutGroup>();headerLayout.childAlignment=TextAnchor.MiddleCenter;headerLayout.childForceExpandHeight=false;
            var title=NewRect("Title Image",header);StoreLayout(title,160);title.GetComponent<LayoutElement>().flexibleWidth=1;
            var titleImage=title.gameObject.AddComponent<Image>();titleImage.sprite=StoreIllustration("art:"+titleArt);titleImage.preserveAspect=true;titleImage.raycastTarget=false;
            var close=StoreButton(header,"Close Store","Back",ClosePurchaseStore);var closeLayout=close.gameObject.AddComponent<LayoutElement>();closeLayout.preferredWidth=320;closeLayout.flexibleWidth=0;closeLayout.preferredHeight=126;
            purchaseBalance=StoreText(panel,"Category Introduction","",0,34);purchaseBalance.gameObject.SetActive(false);
            var scroll=NewRect("Product Scroll",panel);StoreLayout(scroll,120,1);purchaseScroll=scroll.gameObject.AddComponent<ScrollRect>();purchaseScroll.horizontal=false;purchaseScroll.movementType=ScrollRect.MovementType.Clamped;
            var viewport=NewRect("Viewport",scroll);Stretch(viewport);viewport.gameObject.AddComponent<Image>().color=Color.clear;viewport.gameObject.AddComponent<RectMask2D>();
            purchaseContent=NewRect("Products",viewport);purchaseContent.anchorMin=new Vector2(0,1);purchaseContent.anchorMax=Vector2.one;purchaseContent.pivot=new Vector2(.5f,1);purchaseContent.sizeDelta=Vector2.zero;
            purchaseScroll.content=purchaseContent;purchaseScroll.viewport=viewport;
            purchaseStatus=StoreText(panel,"Purchase Status","",72,32);purchaseStatus.color=StoreWhite;purchaseStatus.outlineColor=StoreInk;purchaseStatus.outlineWidth=.15f;
            var utilities=StoreRow(panel,"Purchase Utilities",88);
            purchaseRestore=StoreButton(utilities,"Check Purchases","Restore purchases",()=>purchases.Restore());
            purchaseReconnect=StoreButton(utilities,"Reconnect Store","Reconnect",()=>purchases.Connect());
            foreach(var utility in new[]{purchaseRestore,purchaseReconnect}) {
                utility.GetComponent<Image>().color=Color.clear;
                var label=utility.GetComponentInChildren<TMP_Text>();label.color=StoreWhite;label.fontSizeMax=label.fontSize=36;label.fontStyle=FontStyles.Underline;
            }
            SelectPurchaseCategory(category);Watch(purchaseView,RefreshPurchaseStore);
            if(EventSystem.current)EventSystem.current.SetSelectedGameObject(close.gameObject);
        }
        void ClosePurchaseStore() {
            if(!purchaseView)return;popups.Remove(purchaseView);Destroy(purchaseView);purchaseView=null;
            foreach(var entry in purchaseShopVisibility)if(entry.Key)entry.Key.SetActive(entry.Value);purchaseShopVisibility.Clear();
            purchaseCards.Clear();purchaseCardCopy.Clear();purchaseCardStock.Clear();purchaseActions.Clear();purchaseDetails.Clear();
            selectedPurchase=null;purchaseBuy=null;purchaseBuyLabel=null;
            if(EventSystem.current&&purchaseReturnFocus&&purchaseReturnFocus.activeInHierarchy)EventSystem.current.SetSelectedGameObject(purchaseReturnFocus);
        }
        void SelectPurchaseCategory(StoreCategory category) {
            purchaseCategory=category;
            foreach(Transform child in purchaseContent){child.gameObject.SetActive(false);Destroy(child.gameObject);}
            purchaseCards.Clear();purchaseCardCopy.Clear();purchaseCardStock.Clear();purchaseActions.Clear();purchaseDetails.Clear();
            var products=PurchaseCatalog.Products.Where(p=>p.category==category).ToArray();selectedPurchase=products[0];
            Canvas.ForceUpdateCanvases();float width=purchaseScroll.viewport.rect.width-32,gap=24;
            bool fragments=category==StoreCategory.Fragments,noAds=category==StoreCategory.NoAds;
            float heroHeight=600,smallHeight=520,noAdsHeight=940,fragmentHeight=560;
            purchaseContent.sizeDelta=new Vector2(0,fragments?5*(fragmentHeight+gap)+12:noAds?noAdsHeight+24:heroHeight+smallHeight+gap+24);
            for(int i=0;i<products.Length;i++) {
                var product=products[i];var capture=product;bool featured=category==StoreCategory.Diamonds&&i==2;
                float w=fragments||(!noAds&&!featured)?(width-gap)/2:width,h=fragments?fragmentHeight:noAds?noAdsHeight:featured?heroHeight:smallHeight;
                float x=fragments?i%2*(w+gap):noAds||featured?0:i*(w+gap),y=fragments?i/2*(h+gap):noAds||featured?0:heroHeight+gap;
                var card=StoreButton(purchaseContent,product.id,"",()=>{selectedPurchase=capture;RefreshPurchaseStore();});var rect=(RectTransform)card.transform;
                rect.anchorMin=rect.anchorMax=new Vector2(0,1);rect.pivot=new Vector2(0,1);rect.sizeDelta=new Vector2(w,h);rect.anchoredPosition=new Vector2(16+x,-12-y);
                card.GetComponentInChildren<TMP_Text>().gameObject.SetActive(false);
                var frame=card.GetComponent<Image>();frame.sprite=StoreProductFrame();frame.type=Image.Type.Sliced;
                var cardColors=card.colors;cardColors.highlightedColor=cardColors.selectedColor=Color.white;card.colors=cardColors;
                var outline=card.gameObject.AddComponent<Outline>();outline.effectColor=StoreInk;outline.effectDistance=new Vector2(4,-4);outline.useGraphicAlpha=true;
                var footer=NewRect("Purchase Footer",card.transform);footer.anchorMin=footer.anchorMax=new Vector2(0,.5f);footer.anchoredPosition=new Vector2(w*.5f,-h*.5f+94);footer.sizeDelta=new Vector2(w-44,144);
                var footerImage=footer.gameObject.AddComponent<Image>();footerImage.sprite=StoreFooterFill();footerImage.raycastTarget=false;
                string art=fragments?product.rewardId.ToString():noAds?"art:no-ads-pass-native-v2":featured?"art:diamond-vault-native-v2":i==0?"ty_icon_diamond02":"shop_icon_adDiamond";
                Vector2 iconPosition=fragments?new Vector2(w*.5f,110):noAds?new Vector2(w*.29f,85):featured?new Vector2(w*.73f,75):new Vector2(w*.5f,65);
                Vector2 iconSize=fragments?new Vector2(250,220):noAds?new Vector2(460,430):featured?new Vector2(w*.49f,h-200):new Vector2(230,180);
                StoreArt(card.transform,"Product Icon",art,iconPosition,iconSize);
                var stock=StoreCardText(card.transform,"Product Stock","",new Vector2(w*.5f,-92),new Vector2(w-50,36),34);stock.alignment=TextAlignmentOptions.Center;stock.color=StoreInk;
                if(fragments) {
                    string name=L10n.Translate(Model.helperRoles.First(p=>p.id==product.rewardId-100).helperName);
                    var title=StoreCardText(card.transform,"Product Name",name,new Vector2(w*.5f,-40),new Vector2(w-60,60),46);title.alignment=TextAlignmentOptions.Center;title.color=StoreInk;
                    var amount=StoreCardText(card.transform,"Pack Amount","x"+product.amount,new Vector2(92,h*.5f-61),new Vector2(130,75),52);amount.color=StoreInk;amount.alignment=TextAlignmentOptions.MidlineLeft;
                } else if(noAds||featured) {
                    float cx=noAds?w*.5f:w*.255f,tw=noAds?w-100:w*.43f;
                    var badge=StoreCardText(card.transform,"Pack Label",noAds?"ONE-TIME PURCHASE":"DIAMOND VAULT",new Vector2(cx,h*.5f-62),new Vector2(tw,60),noAds?40:42);badge.color=StoreInk;if(noAds)badge.alignment=TextAlignmentOptions.Center;
                    if(!noAds)WordArtLabel(badge,"diamond-vault",60);
                    if(!noAds) {
                        var amount=StoreCardText(card.transform,"Reward Amount","+"+product.amount.ToString("N0",L10n.Culture),new Vector2(cx,45),new Vector2(tw,150),130);amount.color=StoreInk;
                        var type=StoreCardText(card.transform,"Reward Type","Diamonds",new Vector2(cx,-75),new Vector2(tw,74),46);type.color=StoreInk;
                    }
                    if(noAds) {
                        var tagline=StoreCardText(card.transform,"Reward Type","Every ad reward.\nNo videos.",new Vector2(w*.73f,110),new Vector2(w*.43f,170),58);tagline.alignment=TextAlignmentOptions.Center;tagline.color=StoreInk;
                        var limits=StoreCardText(card.transform,"Reward Limits","Cooldowns & daily limits apply.",new Vector2(w*.73f,-65),new Vector2(w*.43f,95),34);limits.alignment=TextAlignmentOptions.Center;limits.color=StoreInk;
                        string[] benefits={"All ad rewards","One-tap claims","Permanent"};
                        for(int b=0;b<benefits.Length;b++) {
                            float bx=w*(b+.5f)/3;
                            StoreArt(card.transform,"Benefit Icon "+b,"ty_icon_confirmed",new Vector2(bx,-180),new Vector2(54,54));
                            var caption=StoreCardText(card.transform,"Benefit "+b,benefits[b],new Vector2(bx,-245),new Vector2(w/3-36,58),36);caption.alignment=TextAlignmentOptions.Center;caption.color=StoreInk;
                        }
                    }
                } else {
                    var name=StoreCardText(card.transform,"Pack Label",i==0?"POCKET PACK":"DIAMOND STASH",new Vector2(w*.5f,h*.5f-48),new Vector2(w-60,50),36);name.alignment=TextAlignmentOptions.Center;name.color=StoreInk;
                    WordArtLabel(name,i==0?"pocket-pack":"diamond-stash",50);
                    var amount=StoreCardText(card.transform,"Reward Amount","+"+product.amount.ToString("N0",L10n.Culture),new Vector2(w*.5f,-65),new Vector2(w-70,50),60);amount.alignment=TextAlignmentOptions.Center;amount.color=StoreInk;
                }
                var buy=StoreButton(card.transform,"Buy Product","",()=>{selectedPurchase=capture;RefreshPurchaseStore();purchases.Buy(capture.id);});var buyRect=(RectTransform)buy.transform;
                buyRect.anchorMin=buyRect.anchorMax=new Vector2(0,.5f);buyRect.sizeDelta=new Vector2(Mathf.Min(w-80,noAds||featured?520:360),112);
                buyRect.anchoredPosition=new Vector2(w*.5f,-h*.5f+94);
                buy.GetComponent<Image>().color=Color.white;
                var buyText=buy.GetComponentInChildren<TMP_Text>();buyText.color=Color.white;buyText.fontSize=buyText.fontSizeMax=42;
                var infoRect=NewRect("Details",card.transform);infoRect.anchorMin=infoRect.anchorMax=new Vector2(1,1);infoRect.anchoredPosition=new Vector2(-48,-45);infoRect.sizeDelta=new Vector2(60,60);
                var infoImage=infoRect.gameObject.AddComponent<Image>();infoImage.sprite=Sp("upgrade_icon_information");var info=infoRect.gameObject.AddComponent<Button>();info.targetGraphic=infoImage;
                Click(info,()=>ShowDialog(PurchaseCatalog.Title(capture,Model)+"\n\n"+PurchaseCatalog.Description(capture),null));
                purchaseCards.Add(product.id,card);purchaseActions.Add(product.id,buy);purchaseDetails.Add(product.id,info);purchaseCardCopy.Add(product.id,buy.GetComponentInChildren<TMP_Text>());purchaseCardStock.Add(product.id,stock);
                var focus=card.gameObject.AddComponent<StoreProductFocus>();focus.Scroll=purchaseScroll;focus.Selected=()=>{selectedPurchase=capture;RefreshPurchaseStore();};
            }
            purchaseScroll.StopMovement();purchaseScroll.verticalNormalizedPosition=1;RefreshPurchaseStore();Canvas.ForceUpdateCanvases();
        }
        void RefreshPurchaseStore() {
            if(!purchaseView||selectedPurchase==null)return;
            Txt(purchaseBalance,purchaseCategory==StoreCategory.Diamonds?"A little sparkle. A bigger empire.":purchaseCategory==StoreCategory.NoAds?"More play. Fewer interruptions.":"Build your dream team.");
            foreach(var pair in purchaseCards) {
                var product=PurchaseCatalog.Find(pair.Key);var details=purchases.Details(pair.Key);
                string state=purchases.IsOwned(product)?"Owned":purchases.IsPending(pair.Key)?"Payment pending":purchases.Busy?"Please wait...":details!=null&&purchases.Available?details.localizedPrice:"Unavailable";
                Txt(purchaseCardCopy[pair.Key],state);purchaseActions[pair.Key].interactable=purchases.CanBuy(product);
                purchaseActions[pair.Key].GetComponent<Image>().sprite=StoreControl(purchases.CanBuy(product)?"shop-buy-native-v2":"shop-disabled-native-v2");
                pair.Value.GetComponent<Image>().color=Color.white;
                pair.Value.GetComponent<Outline>().enabled=selectedPurchase.id==pair.Key;
                Txt(purchaseCardStock[pair.Key],product.category==StoreCategory.Fragments?"Owned: "+Model.Data.shards[product.rewardId]:"");
            }
            purchaseBuy=purchaseActions[selectedPurchase.id];purchaseBuyLabel=purchaseCardCopy[selectedPurchase.id];
            purchaseRestore.interactable=purchaseReconnect.interactable=!purchases.Busy;Txt(purchaseStatus,purchases.Message);ConfigurePurchaseNavigation();
        }
        void ConfigurePurchaseNavigation() {
            var all=new List<Button>{purchaseView.transform.Find("Panel/Header/Close Store").GetComponent<Button>()};
            all.AddRange(purchaseCards.Values);all.AddRange(purchaseActions.Values);all.AddRange(purchaseDetails.Values);all.Add(purchaseRestore);all.Add(purchaseReconnect);
            all=all.Where(b=>b&&b.interactable&&b.gameObject.activeInHierarchy).ToList();
            for(int i=0;i<all.Count;i++)all[i].navigation=new Navigation{mode=Navigation.Mode.Explicit,selectOnUp=all[(i+all.Count-1)%all.Count],selectOnDown=all[(i+1)%all.Count],selectOnLeft=all[(i+all.Count-1)%all.Count],selectOnRight=all[(i+1)%all.Count]};
        }
    }
}

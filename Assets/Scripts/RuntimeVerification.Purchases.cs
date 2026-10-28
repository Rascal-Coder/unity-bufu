using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Bufu.Rebuild {
    public sealed partial class RuntimeVerification {
        sealed class StoreProbeGateway:IBillingGateway,IPurchaseVerifier {
            public StoreProbeGateway(){if(!GameController.VerificationMode)throw new InvalidOperationException("QA gateway is restricted to isolated verification sessions");}
            public event Action<BillingPurchase> PurchaseUpdated;
            public bool Available=>true;public string requested;public int launches,completed;
            public IReadOnlyList<BillingProduct> Products=>PurchaseCatalog.Products.Select(p=>new BillingProduct{productId=p.id,available=true,localizedPrice="TEST ONLY"}).ToArray();
            public void Connect(Action<bool> done)=>done(true);
            public void QueryPurchases(Action<bool> done)=>done(true);
            public void Purchase(string id){requested=id;launches++;}
            public void Complete(BillingPurchase p,bool consume,Action<bool> done){completed++;done(true);}
            public void Verify(BillingPurchase p,Action<ReceiptValidation> done)=>done(ReceiptValidation.Verified);
            public void Send(string tx,BillingState state)=>PurchaseUpdated?.Invoke(new BillingPurchase{productId=requested,transactionId=tx,state=state});
            public void Dispose(){}
        }
        IEnumerator OpenIndependentOffer(StoreCategory category,string description) {
            Call("ClosePurchaseStore");yield return null;
            Click(Field<Dictionary<StoreCategory,Button>>("shopPurchaseEntries")[category],description);yield return null;
            Check(Field<PurchaseProduct>("selectedPurchase").category==category,"store: each independent entrance opens its own product category directly");
            CheckStoreTitleArt();
        }
        void CheckStoreTitleArt() {
            var page=Field<GameObject>("purchaseView");var panel=page.transform.Find("Panel");
            Check(Mathf.Abs(RootBounds((RectTransform)panel).center.y-RootBounds((RectTransform)page.transform).center.y)<.1f,"store: title, products and controls form a vertically centered group");
            var header=panel.Find("Header/Title Image");
            var art=header?header.GetComponent<Image>():null;
            Check(art&&art.sprite&&art.sprite.rect.width==1024&&!header.GetComponent<TMP_Text>(),"store: product page heading uses imported title artwork instead of a font label");
            foreach(var button in Field<Dictionary<string,Button>>("purchaseActions").Values)CheckStoreActionBounds(button);
        }
        void CheckStoreActionBounds(Button button) {
            var action=RootBounds((RectTransform)button.transform);var card=RootBounds((RectTransform)button.transform.parent);
            Check(action.width<=card.width*.7f&&Mathf.Abs(action.center.x-card.center.x)<.1f,"store: purchase button is compact and centered with clear side margins");
            var footer=RootBounds((RectTransform)button.transform.parent.Find("Purchase Footer"));
            Check(action.yMin-footer.yMin>=12&&footer.yMax-action.yMax>=12,"store: purchase buttons reserve padding above and below inside the footer");
            var icon=RootBounds((RectTransform)button.transform.parent.Find("Product Icon"));
            Check(icon.yMin-action.yMax>=8,"store: reward illustration stays above the purchase control");
            var label=button.GetComponentInChildren<TMP_Text>();var word=label.transform.Find("Word Art");
            if(word&&word.gameObject.activeSelf) {
                var bounds=RootBounds((RectTransform)word);var frame=button.GetComponent<Image>();
                float faceHeight=action.height-(frame.sprite.border.y+frame.sprite.border.w)/frame.pixelsPerUnitMultiplier;
                Check(!label.enabled&&word.GetComponent<Image>().sprite.name.StartsWith("word-purchase-"),"store: fixed purchase status uses matching outlined PNG lettering");
                if(label.text=="Unavailable")Check(word.GetComponent<Image>().sprite.name=="word-purchase-unavailable-native-v4","store: unavailable checkout binds the correct PNG status label");
                Check(bounds.xMin-action.xMin>=60&&action.xMax-bounds.xMax>=60&&bounds.height<=faceHeight-10,"store: purchase word art reserves clear padding inside the scaled frame face");
            } else Check(label.enabled,"store: actual billing prices use live localized text when no fixed word art applies");
        }
        IEnumerator VerifyPurchaseStore() {
            ClearAlignmentGoals();Call("SelectTab",1);FreezeHelpers();foreach(var coin in game.Coins)coin.enabled=false;yield return null;
            double initial=game.Model.Data.diamonds;var service=Field<PurchaseService>("purchases");
            var top=Field<Bufu.Rebuild.UI.UiView>("top");
            Call("SelectTab",5);yield return null;
            var entries=Field<Dictionary<StoreCategory,Button>>("shopPurchaseEntries");
            Check(entries.Count==3&&entries.Values.Select(b=>b.transform.Find("Offer Icon").GetComponent<Image>().sprite.texture).Distinct().Count()==3,"store: three independent right-side entrances have distinct real illustrations");
            foreach(var entry in entries.Values) {
                var caption=entry.transform.Find("Caption").GetComponent<Image>();
                Check(caption&&caption.sprite&&caption.sprite.rect.width==768&&!caption.GetComponent<TMP_Text>(),"store: side entrance captions use imported white outlined word images");
            }
            var motion=entries[StoreCategory.NoAds].GetComponent<Bufu.Rebuild.UI.ShopOfferMotion>();bool reduced=motion.ReduceMotion;motion.ReduceMotion=false;
            var iconPosition=motion.Icon.anchoredPosition;var buttonBounds=RootBounds((RectTransform)entries[StoreCategory.NoAds].transform);bool moved=false,stable=true;
            for(int sample=0;sample<5;sample++) {
                yield return new WaitForSecondsRealtime(.2f);moved|=Vector2.Distance(iconPosition,motion.Icon.anchoredPosition)>.1f;
                var current=RootBounds((RectTransform)entries[StoreCategory.NoAds].transform);
                stable&=Vector2.Distance(buttonBounds.center,current.center)<.01f&&Vector2.Distance(buttonBounds.size,current.size)<.01f;
            }
            Check(moved&&stable,"store: icon attention motion leaves the hit rectangle stable");
            motion.ReduceMotion=true;yield return null;var still=motion.Icon.anchoredPosition;yield return new WaitForSecondsRealtime(.2f);
            Check(still==motion.Icon.anchoredPosition&&motion.Icon.localScale==Vector3.one,"store: reduced motion keeps offer artwork static");motion.ReduceMotion=reduced;
            yield return Capture("00-shop-independent-entries.png");
            if(Environment.GetCommandLineArgs().Contains("--record-shop-motion")) {
                float start=Time.realtimeSinceStartup;int frame=0;var times=new List<string>();
                while(Time.realtimeSinceStartup-start<8) {
                    times.Add((Time.realtimeSinceStartup-start).ToString("0.000",System.Globalization.CultureInfo.InvariantCulture));
                    yield return Capture("motion-"+(frame++).ToString("000")+".png");yield return new WaitForSecondsRealtime(.1f);
                }
                File.WriteAllLines(Path.Combine(output,"motion-times.txt"),times);
            }
            Click(top.Node("diamond").transform.Find("Open Purchase Store").GetComponent<Button>(),"store: diamond counter plus opens the storefront");yield return null;
            var root=Field<GameObject>("purchaseView");
            CheckWordArtSet(root.transform,"back","diamond-vault","pocket-pack","diamond-stash");
            CheckStoreTitleArt();
            Check(root&&game.ModalOpen&&!Field<Button>("purchaseBuy").interactable,"store: unconnected checkout is visibly disabled");
            var shopPage=Field<Bufu.Rebuild.UI.UiView>("shopView");
            Check(shopPage!=null&&root.transform.IsChildOf(shopPage.Root.transform),"store: paid packs are an illustrated product page inside Shop");
            Check(!root.transform.Find("Panel/Categories"),"store: independent product pages have no category tabs");
            Check(!shopPage.Node("itemLayout").activeInHierarchy&&!shopPage.Node("spinnerGroup").activeInHierarchy,"store: original spin products are hidden completely while viewing packs");
            var pageBounds=RootBounds((RectTransform)root.transform);
            Check(pageBounds.yMin>BottomNavigationTop(),"store: packs page reserves the complete bottom navigation and unlock hints");
            Check(Field<Dictionary<string,Button>>("purchaseCards").Count==3,"store: three diamond packs appear");
            var vault=Field<Dictionary<string,Button>>("purchaseCards")["diamonds_1200"].transform.Find("Product Icon").GetComponent<Image>();
            Check(vault.sprite&&vault.sprite.rect.width>=1000,"store: the featured vault uses its imported high-resolution reward illustration");
            foreach(var action in Field<Dictionary<string,Button>>("purchaseActions").Values) {
                Check(action.GetComponent<Image>().type==Image.Type.Sliced&&action.GetComponent<Image>().sprite.rect.width==720,"store: product actions use the generated matching nine-slice button family");
                CheckStoreActionBounds(action);
            }
            foreach(var card in Field<Dictionary<string,Button>>("purchaseCards").Values) {
                var amount=card.transform.Find("Reward Amount").GetComponent<TMP_Text>();
                var art=RootBounds((RectTransform)card.transform.Find("Product Icon"));
                var footer=RootBounds((RectTransform)card.transform.Find("Purchase Footer"));
                var band=RootBounds((RectTransform)card.transform.Find("Product Name Band"));
                Check(amount.fontSizeMax>=100&&!RootBounds(amount.rectTransform).Overlaps(art)&&RootBounds(amount.rectTransform).yMin-footer.yMax>=12,"store: diamond quantity is prominent and stays separate from the illustration and price footer");
                Check(band.yMin-art.yMax>=10,"store: diamond illustration stays clear of the product title band");
                CheckNativeStoreNumbers(amount);
            }
            CheckEnglish(root.transform,"diamond store");yield return Capture("01-store-diamonds.png");
            Click(Field<Button>("purchaseRestore"),"store: checking purchases while unavailable is safe");
            Click(Field<Button>("purchaseReconnect"),"store: reconnect while billing is absent is safe");
            Check(game.Model.Data.diamonds==initial&&Field<TMP_Text>("purchaseStatus").text==PurchaseService.UnavailableMessage,"store: unavailable operations never change the wallet");
            yield return OpenIndependentOffer(StoreCategory.NoAds,"store: independent No Ads entrance opens its offer");root=Field<GameObject>("purchaseView");
            var benefitCard=Field<Dictionary<string,Button>>("purchaseCards")["permanent_no_ads"];
            var benefitIcons=Enumerable.Range(0,3).Select(i=>benefitCard.transform.Find("Benefit Icon "+i).GetComponent<Image>()).ToArray();
            Check(benefitIcons.Select(i=>i.sprite.name).Distinct().Count()==3&&benefitIcons.All(i=>i.sprite.name.StartsWith("benefit-")&&i.rectTransform.rect.height>=176),"store: No Ads benefits use three distinct readable illustrations instead of repeated checkmarks");
            Check(benefitIcons.Select(i=>i.sprite.texture).Distinct().Count()==1,"store: No Ads benefit art shares one compressed sheet");
            var benefitFooter=RootBounds((RectTransform)benefitCard.transform.Find("Purchase Footer"));
            for(int i=0;i<3;i++) {
                var bounds=RootBounds((RectTransform)benefitCard.transform.Find("Benefit Tile "+i));
                var label=RootBounds((RectTransform)benefitCard.transform.Find("Benefit "+i));
                Check(bounds.yMin-benefitFooter.yMax>=10&&label.yMin>bounds.yMin&&RootBounds(benefitIcons[i].rectTransform).yMin>label.yMax&&RootBounds((RectTransform)benefitCard.transform.Find("Reward Limits")).yMin-bounds.yMax>=20,"store: benefit illustration, label, limits and purchase footer have separate clear space");
            }
            Click(Field<Dictionary<string,Button>>("purchaseDetails")["permanent_no_ads"],"store: open the No Ads product information");yield return null;
            var details=LatestPopup("Common/commonDialog");
            Check(details.Ref<TMP_Text>("CommonDialog","dialogText").text.Contains("Cooldowns and daily limits"),"store: No Ads product information promises exact scope and retains limits");
            game.ClosePopup(details);yield return null;
            CheckEnglish(root.transform,"no ads store");yield return Capture("02-store-no-ads.png");
            yield return OpenIndependentOffer(StoreCategory.Fragments,"store: independent fragments entrance opens its products");root=Field<GameObject>("purchaseView");
            Check(Field<Dictionary<string,Button>>("purchaseCards").Count==10,"store: ten named helper fragment products are available for selection");
            var fragmentScroll=Field<ScrollRect>("purchaseScroll");var firstCard=Field<Dictionary<string,Button>>("purchaseCards").Values.First();
            float rowHeight=((RectTransform)firstCard.transform).rect.height+24;
            Check(Mathf.Abs(fragmentScroll.viewport.rect.height/rowHeight-Mathf.Round(fragmentScroll.viewport.rect.height/rowHeight))<.01f&&fragmentScroll.verticalScrollbar&&fragmentScroll.verticalScrollbar.size<1,"store: fragment viewport fits whole product rows at rest and shows a scroll position indicator");
            foreach(var card in Field<Dictionary<string,Button>>("purchaseCards").Values) {
                var icon=card.transform.Find("Product Icon").GetComponent<Image>();
                Check(icon.sprite&&icon.rectTransform.rect.width>=170,"store: each helper product has a real, prominent reward portrait");
                var amount=card.transform.Find("Pack Amount").GetComponent<TMP_Text>();var info=card.transform.Find("Details").GetComponent<RectTransform>();
                CheckNativeStoreNumbers(amount);
                Check(!RootBounds(amount.rectTransform).Overlaps(RootBounds(info)),"store: fragment quantity stays clear of the information button");
                var stock=card.transform.Find("Product Stock").GetComponent<TMP_Text>();var buy=card.transform.Find("Buy Product").GetComponent<RectTransform>();
                Check(!RootBounds(stock.rectTransform).Overlaps(RootBounds(buy)),"store: owned fragment count stays above its purchase button");
                var title=card.transform.Find("Product Name").GetComponent<TMP_Text>();var type=card.transform.Find("Reward Type").GetComponent<TMP_Text>();
                var badge=RootBounds((RectTransform)card.transform.Find("Fragment Quantity Badge"));var art=RootBounds(icon.rectTransform);
                Check(icon.rectTransform.rect.height>=280&&amount.fontSizeMax>=80&&!art.Overlaps(badge)&&!RootBounds(title.rectTransform).Overlaps(RootBounds(info)),"store: named fragment portrait and quantity badge remain large and separate from each other and the details control");
                Check(type.text=="Fragments"&&badge.yMin-RootBounds(type.rectTransform).yMax>=10&&RootBounds(type.rectTransform).yMin-RootBounds(stock.rectTransform).yMax>=10,"store: fragment amount, reward type and owned inventory have separate readable rows");
                CheckStoreActionBounds(buy.GetComponent<Button>());
            }
            yield return Capture("03a-store-fragments-top.png");
            var scroll=Field<ScrollRect>("purchaseScroll");scroll.verticalNormalizedPosition=0;Canvas.ForceUpdateCanvases();yield return null;
            Click(Field<Dictionary<string,Button>>("purchaseCards")["helper_10_fragments_20"],"store: scroll to and select the final helper pack");yield return null;
            CheckEnglish(root.transform,"helper fragment store");yield return Capture("03-store-fragments-bottom.png");
            var cards=Field<Dictionary<string,Button>>("purchaseCards");
            EventSystem.current.SetSelectedGameObject(cards["helper_01_fragments_20"].gameObject);yield return null;
            Check(scroll.verticalNormalizedPosition>.9f,"store: keyboard focus scrolls the first product into view");
            for(int i=0;i<9;i++) {
                var selected=EventSystem.current.currentSelectedGameObject;
                ExecuteEvents.Execute(selected,new AxisEventData(EventSystem.current){moveDir=MoveDirection.Down},ExecuteEvents.moveHandler);yield return null;
            }
            Check(EventSystem.current.currentSelectedGameObject==cards["helper_10_fragments_20"].gameObject&&scroll.verticalNormalizedPosition<.1f,"store: keyboard Down reaches and reveals the final helper product");
            foreach(var button in root.GetComponentsInChildren<Button>())if(button.interactable)
                Check(button.navigation.mode==Navigation.Mode.Explicit&&button.navigation.selectOnDown.transform.IsChildOf(root.transform),"store: navigation stays inside the modal for "+button.name);
            Call("CloseTopPopup");yield return null;
            Check(!root&&!game.ModalOpen,"store: Android back / Escape closes the store without orphaning the blocker");
            Check(shopPage.Node("itemLayout").activeInHierarchy&&shopPage.Node("spinnerGroup").activeInHierarchy,"store: closing packs restores the original spin page and its products");
            // Inject an explicit QA-only adapter into the isolated verification model. Normal builds
            // always construct the unavailable gateway; no debug purchase button exists in the shop.
            service.Dispose();var gateway=new StoreProbeGateway();
            service=new PurchaseService(game.Model,gateway,gateway,()=>SaveStore.Save(game.Model));
            typeof(GameController).GetField("purchases",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(game,service);
            service.Changed+=()=>Call("RefreshPurchaseStore");service.Connect();Call("ShowPurchaseStore");yield return null;
            Check(Field<Dictionary<string,TMP_Text>>("purchaseCardCopy").Values.All(t=>t.enabled&&t.text=="TEST ONLY"&&(!t.transform.Find("Word Art")||!t.transform.Find("Word Art").gameObject.activeSelf)),"store QA: real price strings replace status images completely");
            Click(Field<Button>("purchaseBuy"),"store QA: buy a selected diamond pack through the actual UI");
            Check(gateway.launches==1&&!Field<Button>("purchaseBuy").interactable,"store QA: checkout locks repeated purchase taps");
            Check(Field<TMP_Text>("purchaseBuyLabel").transform.Find("Word Art").GetComponent<Image>().sprite.name.Contains("please-wait"),"store QA: busy checkout swaps to its PNG state label");
            gateway.Send("",BillingState.Cancelled);yield return null;
            Check(game.Model.Data.diamonds==initial&&Field<Button>("purchaseBuy").interactable,"store QA: cancellation returns to a retryable selection without rewards");
            Click(Field<Button>("purchaseBuy"),"store QA: retry diamond checkout");gateway.Send("qa-diamonds",BillingState.Pending);yield return null;
            Check(service.IsPending("diamonds_100")&&game.Model.Data.diamonds==initial,"store QA: pending payment neither grants nor permits another purchase");
            Check(Field<TMP_Text>("purchaseBuyLabel").transform.Find("Word Art").GetComponent<Image>().sprite.name.Contains("pending"),"store QA: pending payment uses its distinct PNG state label");
            Call("ClosePurchaseStore");yield return null;
            gateway.Send("qa-diamonds",BillingState.Purchased);gateway.Send("qa-diamonds",BillingState.Purchased);yield return null;
            Check(game.Model.Data.diamonds==initial+100,"store QA: completed payment while store is closed settles exactly once");
            yield return OpenIndependentOffer(StoreCategory.Fragments,"store QA: choose helper fragments");
            int before=game.Model.Data.shards[101];Click(Field<Button>("purchaseBuy"),"store QA: purchase the first helper fragment pack");gateway.Send("qa-helper",BillingState.Purchased);yield return null;
            Check(game.Model.Data.shards[101]==before+20&&game.Model.Fragments(1)==before+20,"store QA: purchased helper fragments feed the existing upgrade inventory");
            yield return OpenIndependentOffer(StoreCategory.NoAds,"store QA: choose permanent No Ads");
            Click(Field<Button>("purchaseBuy"),"store QA: purchase permanent No Ads");gateway.Send("qa-ads",BillingState.Purchased);yield return null;
            Check(game.Model.HasNoAds&&!Field<Button>("purchaseBuy").interactable&&Field<TMP_Text>("purchaseBuyLabel").text=="Owned","store QA: owned entitlement is visible and cannot be repurchased");
            Check(Field<TMP_Text>("purchaseBuyLabel").transform.Find("Word Art").GetComponent<Image>().sprite.name.Contains("owned"),"store QA: owned entitlement swaps to its PNG state label");
            yield return Capture("04-store-owned-qa-only.png");Call("ClosePurchaseStore");yield return null;
            int claims=0;Call("ShowOriginalAd",(Action)(()=>claims++),null);Call("ShowOriginalAd",(Action)(()=>claims++),null);
            Check(!Field<GameObject>("originalAd"),"store QA: No Ads bypasses the ad surface");yield return null;yield return null;
            Check(claims==1,"store QA: duplicate ad requests cannot double-award during a skip");
            game.Model.Data.diamondReadyAt=0;game.Model.Data.diamondClaims=0;
            Call("SelectTab",5);yield return null;var shop=Field<Bufu.Rebuild.UI.UiView>("shopView");
            double wallet=game.Model.Data.diamonds;int reward=game.Model.shop.First(p=>p.id==201).count[0];
            Click(shop.Ref<Button>("ShopTurntablePanel","button201"),"store QA: claim the original ad-diamond reward with No Ads");yield return null;yield return null;
            Check(!Field<GameObject>("originalAd")&&game.Model.Data.diamonds==wallet+reward,"store QA: original ad button claims its real reward without a video");
            Call("CloseTopPopup");yield return null;
            Check(game.Model.DiamondCooling&&!game.Model.ClaimDiamondAd(),"store QA: original rewarded offer retains its cooldown after skipping the video");
            if(game.ModalOpen)Call("CloseTopPopup");yield return null;
            Click(Field<Dictionary<StoreCategory,Button>>("shopPurchaseEntries")[StoreCategory.Diamonds],"store QA: independent diamonds entrance opens paid packs");yield return null;
            Check(Field<GameObject>("purchaseView"),"store QA: Shop store entry is reachable");
            Call("SelectTab",2);yield return null;
            Check(!Field<GameObject>("purchaseView")&&!game.ModalOpen,"store: leaving Shop closes the product page without a hidden input blocker");
            int receiptCount=game.Model.Data.purchases.receipts.Count;Call("ResetGame");yield return null;
            Check(game.Model.HasNoAds&&game.Model.Data.purchases.receipts.Count==receiptCount,"store QA: clearing game progress preserves the purchased entitlement and transaction history");
            service.Dispose();Call("InitializePurchases");
        }
        void CheckNativeStoreNumbers(TMP_Text amount) {
            var art=amount.transform.Find("Number Art");var images=art?art.GetComponentsInChildren<Image>():Array.Empty<Image>();
            var expected=amount.text.Where(c=>char.IsDigit(c)||c=='+'||c=='x'||c=='X').Select(c=>"ty_num_both_"+(char.IsDigit(c)?40+c-'0':c=='+'?39:74)).ToArray();
            Check(!amount.enabled&&images.Length==expected.Length&&images.Select(i=>i.sprite?i.sprite.name:"").SequenceEqual(expected),"store: quantity digits and prefix reuse the game's original number PNG sprites in the exact reward order");
            Check(art&&RootBounds((RectTransform)art).width<=RootBounds(amount.rectTransform).width+.1f&&RootBounds((RectTransform)art).height<=RootBounds(amount.rectTransform).height+.1f,"store: native number row fits its quantity area with consistent spacing");
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bufu.Rebuild.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    public sealed partial class RuntimeVerification {
        Button AdCloseButton(GameObject ad)=>PrefabRef<Button>(ad,"fakevideo/FakeVideoPanel","FakeAdUI","closeButton");
        T GameResult<T>(string name,params object[] args)=>(T)typeof(GameController).GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,args);
        Vector2 SceneScreenPoint(Vector2 local)=>RectTransformUtility.WorldToScreenPoint(null,game.Field.TransformPoint(local));
        IEnumerator AlignmentWait(Func<bool> ready,float seconds,string description) {
            float elapsed=0;while(!ready()&&elapsed<seconds){elapsed+=Time.unscaledDeltaTime;yield return null;}
            Check(ready(),description);
        }
        void FreezeHelpers(){foreach(var helper in game.HelperActors)if(helper)helper.enabled=false;}
        void ClearAlignmentGoals(){game.Model.State.mission=game.Model.WorldMissions.Length;game.Model.State.pendingGoals.Clear();Refresh();}
        Vector2 FreeScenePoint() {
            foreach(float y in new[]{0f,150f,-150f,300f,-300f})foreach(float x in new[]{-150f,0f,-300f}) {
                var local=new Vector2(x,y);
                if(!GameResult<bool>("ScenePointerBlockedByUi",SceneScreenPoint(local))&&!GameResult<bool>("ScenePointerBlockedByUi",SceneScreenPoint(local+Vector2.right*250)))return local;
            }
            throw new Exception("No unobscured scene input test position");
        }
        IEnumerator VerifyAlignment() {
            Time.timeScale=1;ClearAlignmentGoals();
            if(Environment.GetCommandLineArgs().Contains("--purchases-only")) {
                yield return VerifyPurchaseStore();Check(errors.Count==0,"store has no runtime errors or exceptions");Complete(true,null);yield break;
            }
            if(Environment.GetCommandLineArgs().Contains("--activities-only")) {
                yield return VerifyLocalActivities();Check(errors.Count==0,"activities have no runtime errors or exceptions");Complete(true,null);yield break;
            }
            if(Environment.GetCommandLineArgs().Contains("--dice-only")) {
                yield return VerifyAlignedDiceSystems();Check(errors.Count==0,"dice layout has no runtime errors or exceptions");Complete(true,null);yield break;
            }
            yield return VerifyAlignedSceneInput();
            yield return VerifyUncappedSceneAndEvolution();
            yield return VerifyAlignedDiceSystems();
            yield return VerifyFixedSettlementPaths();
            yield return VerifyAdvancedRebirthPreservation();
            yield return VerifyAlignedRewardsAndShop();
            yield return VerifyAlignedWorldPaging();
            yield return VerifyIntegrationPlaceholders();
            yield return VerifyLocalActivities();
            Check(errors.Count==0,"alignment journey has no runtime errors or exceptions");Complete(true,null);
        }

        IEnumerator VerifyAlignedSceneInput() {
            var model=game.Model;Call("SelectTab",1);yield return null;
            var p=FreeScenePoint();var coins=game.Coins.Take(3).ToArray();
            coins[0].Rect.anchoredPosition=p-Vector2.right*10;coins[1].Rect.anchoredPosition=p+Vector2.right*30;coins[2].Rect.anchoredPosition=p+Vector2.right*150;
            var point=SceneScreenPoint(p);game.BeginScenePointer(point);
            Check(coins.All(c=>!c.Busy),"input: pressing an overlapping actor rectangle does not throw");
            game.EndScenePointer(point);
            Check(coins[0].Busy&&!coins[1].Busy,"input: release throws only the nearest idle coin within 50 canvas pixels");
            yield return null;
            point=SceneScreenPoint(coins[2].Rect.anchoredPosition+Vector2.right*65);game.BeginScenePointer(point);game.EndScenePointer(point);
            Check(!coins[2].Busy,"input: an actor's rectangular corner outside the original 50-pixel radius does not throw");
            yield return null;
            point=SceneScreenPoint(coins[1].Rect.anchoredPosition);game.BeginScenePointer(point);game.CancelScenePointer();game.EndScenePointer(point);
            Check(!coins[1].Busy,"input: a cancelled touch cannot become a release click");
            var setup=Field<UiView>("top").Ref<RectTransform>("TopPanel","setupBtn");
            game.BeginScenePointer(ScreenPoint(setup));game.MoveScenePointer(point);game.EndScenePointer(point);
            Check(!coins[1].Busy,"input: a gesture begun on UI cannot swipe into the scene");
            yield return new WaitForSeconds(.9f);
            coins[1].Rect.anchoredPosition=p+Vector2.right*140;coins[2].Rect.anchoredPosition=p+Vector2.right*170;
            var end=SceneScreenPoint(p+Vector2.right*155);var start=end-Vector2.right*80;
            model.State.boosts[6]=0;game.BeginScenePointer(start);game.MoveScenePointer(end);game.EndScenePointer(end);
            Check(!coins[1].Busy&&!coins[2].Busy,"input: dragging without the swipe upgrade cannot throw");
            model.State.boosts[6]=1;game.BeginScenePointer(start);game.MoveScenePointer(end);game.EndScenePointer(end);
            Check(coins[1].Busy&&coins[2].Busy,"input: one swipe sample throws every eligible idle coin inside the original radius");
            yield return Capture("01-aligned-scene-input.png");
        }

        IEnumerator VerifyUncappedSceneAndEvolution() {
            var model=game.Model;model.State.quantity[1]=65;model.State.quantity[2]=120;model.State.boosts[6]=0;
            game.RebuildActors();FreezeHelpers();yield return null;
            Check(game.HelperActors.Count==65&&game.HelperActors.All(h=>h&&h.Visual&&h.Visual.Data!=null),"scene: all 65 configured helpers have live VAT actors without a render cap");
            Check(game.Coins.Count==model.Count(1)&&game.Coins.Count>100,"scene: more than 100 configured coins remain on the field without truncation");
            yield return Capture("02-65-helpers-123-coins.png");
            var p=FreeScenePoint();foreach(var coin in game.Coins)coin.Rect.anchoredPosition=p+Vector2.up*450;
            game.SleepCount=3;
            var nearest=game.HelperActors[0];var other=game.HelperActors[1];nearest.Rect.anchoredPosition=p+Vector2.right*10;other.Rect.anchoredPosition=p+Vector2.right*70;
            var sleep=typeof(HelperActor).GetMethod("EnterSleeping",BindingFlags.Instance|BindingFlags.NonPublic);sleep.Invoke(nearest,null);sleep.Invoke(other,null);
            var point=SceneScreenPoint(p);game.BeginScenePointer(point);game.EndScenePointer(point);
            Check(!nearest.Sleeping&&other.Sleeping,"input: a click missing coins wakes only the nearest sleeper within 90 canvas pixels");
            var helpers=game.HelperActors.ToArray();var helperPositions=helpers.Select(h=>h.Rect.anchoredPosition).ToArray();
            int threshold=model.Config(2,false).evolutionLevel[0];model.State.quantity[2]=threshold+4;game.SyncActors();
            foreach(var coin in game.Coins)coin.Rect.anchoredPosition=game.RandomCoinSpawn();
            var oldCoins=game.Coins.ToArray();float before=oldCoins.Sum(c=>c.Rect.anchoredPosition.magnitude);
            Check(GameResult<bool>("StartCoinEvolution",1),"evolution: actual controller starts the configured evolution sequence");
            Check(oldCoins.All(c=>c.Evolving)&&game.Coins.Any(c=>c.Evolving&&c.Rect.localScale.x>2.9f),"evolution: old coins lock and an enlarged aggregate appears before the new skin");
            Check(game.Coins.Except(oldCoins).Count()==model.Count(1)&&model.Count(1)>1,"evolution: every retained result participates in the centre sequence without a late population jump");
            yield return new WaitForSeconds(.45f);
            Check(oldCoins.Where(c=>c).Sum(c=>c.Rect.anchoredPosition.magnitude)<before,"evolution: coins visibly converge toward the scene centre");
            yield return Capture("03-evolution-aggregate.png");
            yield return AlignmentWait(()=>game.Coins.All(c=>c&&!c.Evolving),8,"evolution: reveal and shrink sequence reaches its idle result");
            Check(game.HelperActors.SequenceEqual(helpers)&&helpers.Select((h,i)=>h&&h.Rect.anchoredPosition==helperPositions[i]).All(x=>x),"evolution: helper identities and in-progress positions survive coin evolution");
            Check(model.State.evolution[2]==1&&game.Coins.Count==model.Count(1)&&game.Coins.All(c=>c.Visual.Data.name=="coin_copper_2"&&Mathf.Abs(c.Rect.localScale.x-1)<.001f),"evolution: the exact evolved skin replaces the aggregate at native scale");
            yield return Capture("04-evolution-result-preserved-helpers.png");
        }

        IEnumerator VerifyAlignedDiceSystems() {
            var model=game.Model;Check(model.SwitchWorld(2),"dice: unlock and switch the isolated model to the dice world");game.RebuildActors();FreezeHelpers();Call("SelectTab",1);ClearAlignmentGoals();
            model.State.skills[101]=20;model.State.skills[102]=20;model.State.skills[105]=1;model.State.skills[106]=1;model.State.skills[107]=1;
            Refresh();yield return null;yield return null;
            var dice=game.Coins.First();var roll=typeof(CoinActor).GetMethod("RollOutcome",BindingFlags.Instance|BindingFlags.NonPublic);int[] faces=new int[6];
            UnityEngine.Random.InitState(20261020);
            for(int i=0;i<2400;i++){roll.Invoke(dice,null);faces[dice.CurrentFace-1]++;}
            Check(faces[5]>faces[0]*3&&faces.Take(6).All(n=>n>0),"dice: actual CoinActor outcomes use the upgraded weighted distribution rather than uniform faces");
            double gold=model.State.gold;Check(dice.Throw(true),"dice: real VAT die begins a manual throw");
            yield return AlignmentWait(()=>!dice.Busy,3,"dice: actual throw animation reaches settlement");
            Check(model.State.gold>gold&&dice.CurrentFace>=1&&dice.CurrentFace<=6,"dice: the displayed weighted face settles a positive payout");
            var hud=Field<UiView>("promotionView");
            Check(hud.Ref("DownItem","superSix").activeInHierarchy&&hud.Ref("DownItem","zuanshiShangRen").activeInHierarchy,"dice: original Super 666 and merchant HUD nodes are visible");
            model.State.diceSixCount=model.DiceSuperTarget;Refresh();yield return null;
            Click(hud.Ref<Button>("DownItem","superBtn"),"dice: open the original Super 666 activation panel");
            var activation=Field<UiView>("diceSuperDialog");Check(activation!=null&&activation.Script("AdLaoHuJi")!=null,"dice: activation uses the native AdLaoHuJi prefab");
            yield return new WaitForSeconds(2.1f);yield return Capture("05-original-super666-activation.png");
            Click(activation.Ref<Button>("AdLaoHuJi","jihuoBtn"),"dice: activate the ordinary Super 666 mode");yield return null;
            Check(model.DiceSuperActive&&model.State.diceSixCount==0,"dice: UI activation consumes the charge and starts its real duration");
            Check(dice.Throw(true),"dice: throw while Super 666 is active");
            yield return AlignmentWait(()=>!dice.Busy,3,"dice: Super 666 throw completes");
            Check(dice.CurrentFace==6,"dice: active Super 666 forces the real visible die to land on six");
            yield return Capture("06-real-super666-and-merchant-hud.png");
            var upgradePanel=Field<UiView>("upgrade");
            var bronze=(RectTransform)upgradePanel.Find("ItemPanel/panel1/Scroll View/Viewport/Content/item0_102").transform;
            foreach(var bubble in new[]{hud.Ref("DownItem","superSix").transform.Find("superBtn/kuang"),hud.Ref("DownItem","zuanshiShangRen").transform.Find("kuang")})
                Check(!RootBounds((RectTransform)bubble).Overlaps(RootBounds(bronze)),"dice: shortcut bubble stays clear of the Bronze Die upgrade card");
            Check(!RootBounds(hud.Ref<RectTransform>("DownItem","superBtn")).Overlaps(RootBounds(bronze)),"dice: Super 666 role and hit target stay outside the Bronze Die upgrade card");
            double diamonds=model.Data.diamonds;
            Click(hud.Ref<Button>("DownItem","shangrenBtn"),"dice: collect the merchant's real reward");yield return null;
            Check(model.Data.diamonds>diamonds&&model.State.diceMerchantToday>0&&!model.DiceMerchantReady(GameModel.DiceLocalMilliseconds),"dice: merchant HUD action pays once and starts the persisted cooldown");
            var receipt=RewardPopup();Click(receipt.Ref<Button>("RewardBoxUI","GetButton"),"dice: close merchant receipt");yield return null;
            diamonds=model.Data.diamonds;Click(hud.Ref<Button>("DownItem","shangrenBtn"),"dice: retry cooling merchant");
            Check(model.Data.diamonds==diamonds,"dice: repeated merchant action cannot bypass cooldown");
            yield return Capture("07-merchant-real-cooldown.png");
        }

        IEnumerator VerifyAlignedRewardsAndShop() {
            var model=game.Model;var receipt=new List<KeyValuePair<int,double>>{new KeyValuePair<int,double>(2,7)};
            double before=model.Data.diamonds;model.GrantRewardReceipt(2,receipt);game.ShowRewardBox(receipt,world:2);yield return null;
            var popup=RewardPopup();var doubleButton=popup.Ref<Button>("RewardBoxUI","doubleButton");
            Click(doubleButton,"rewards: request the original advertised double prize");doubleButton.onClick.Invoke();yield return null;
            Check(model.Data.diamonds==before+7&&Field<GameObject>("originalAd"),"rewards: repeated double clicks open one ad and do not grant early");
            yield return new WaitForSecondsRealtime(2.15f);yield return Capture("08-original-pc-ad-complete.png");
            var ad=Field<GameObject>("originalAd");var close=AdCloseButton(ad);Click(close,"rewards: explicitly close the completed desktop ad");close.onClick.Invoke();yield return null;yield return null;
            Check(model.Data.diamonds==before+14&&!Field<GameObject>("originalAd"),"rewards: ad completion adds one exact base receipt once, even after duplicate close input");
            Call("SelectTab",5);yield return null;var shop=Field<UiView>("shopView");model.Data.diamonds=10000;model.Data.spinCount=0;model.Data.spinReadyAt=0;model.Data.shopDate=GameModel.TodayNumber();Refresh();
            Click(shop.Ref<Button>("ShopTurntablePanel","button202"),"shop: buy the normal diamond chest");yield return null;
            popup=RewardPopup();Check(!popup.Ref("RewardBoxUI","doubleButton").activeSelf,"shop: purchased 202 chest never offers ad doubling");
            Click(popup.Ref<Button>("RewardBoxUI","GetButton"),"shop: close purchased chest receipt");yield return null;
            double spinGold=model.State.gold,spinDiamonds=model.Data.diamonds;int spinPoints=model.State.points,spinProgress=model.Data.spinProgress;var spinShards=(int[])model.Data.shards.Clone();
            Func<bool> spinWalletUnchanged=()=>model.State.gold==spinGold&&model.Data.diamonds==spinDiamonds&&model.State.points==spinPoints&&model.Data.shards.SequenceEqual(spinShards);
            Click(shop.Ref<Button>("ShopTurntablePanel","spinAdButton"),"shop: take the first free daily spin");
            Check(model.Data.spinCount==1&&!Field<GameObject>("originalAd"),"shop: first daily spin begins without an ad");
            Check(model.Data.spinProgress==spinProgress&&spinWalletUnchanged(),"shop: starting the wheel neither grants its prize nor increments finished-spin progress");
            yield return AlignmentWait(()=>!Field<bool>("spinning"),12,"shop: original segmented wheel and burst animation finish");
            // CompleteSpin creates the receipt in the frame that clears spinning; allow its graphics to reach the canvas before a real raycast.
            yield return null;
            Check(model.Data.spinProgress==Math.Min(GameModel.SpinProgressMax,spinProgress+1)&&spinWalletUnchanged(),"shop: finishing the wheel advances progress while its unconfirmed receipt remains unpaid");
            popup=RewardPopup();Click(popup.Ref<Button>("RewardBoxUI","GetButton"),"shop: close free spin receipt");yield return null;
            Check(!spinWalletUnchanged(),"shop: confirming the wheel receipt grants its resolved prize");
            model.Data.spinReadyAt=0;Refresh();Click(shop.Ref<Button>("ShopTurntablePanel","spinAdButton"),"shop: request the second daily spin");
            Check(model.Data.spinCount==1&&Field<GameObject>("originalAd"),"shop: non-free spin waits for the real ad-success path");
            yield return new WaitForSecondsRealtime(2.15f);Click(AdCloseButton(Field<GameObject>("originalAd")),"shop: complete second spin ad");yield return null;yield return null;
            Check(model.Data.spinCount==2,"shop: successful ad consumes the second spin exactly once");
            yield return AlignmentWait(()=>!Field<bool>("spinning"),12,"shop: advertised spin finishes without locking shop input");
            popup=RewardPopup();yield return Capture("09-original-shop-double-reward.png");Click(popup.Ref<Button>("RewardBoxUI","GetButton"),"shop: close advertised spin receipt");yield return null;
        }

        IEnumerator VerifyAlignedWorldPaging() {
            Call("SelectTab",6);yield return null;yield return null;
            var world=Field<UiView>("worldView");var scroll=world.Ref<ScrollRect>("WorldSelectionPanel","worldScrollView");
            Check(scroll.content.childCount==3,"world: the original page contains Coming Soon and two configured worlds");
            Check(scroll.content.GetChild(0).GetComponentsInChildren<TMP_Text>(true).Any(t=>t.text==L10n.Translate("更多世界"))&&scroll.content.GetChild(1).GetComponentsInChildren<TMP_Text>(true).Any(t=>t.text==L10n.Translate(game.Model.WorldName(2)))&&scroll.content.GetChild(2).GetComponentsInChildren<TMP_Text>(true).Any(t=>t.text==L10n.Translate(game.Model.WorldName(1))),"world: original order is Coming Soon, dice, then coin");
            var snap=scroll.GetComponent<WorldPageSnap>();Check(snap,"world: native drag-end snapping is installed");
            snap.ScrollToIndex(2,.14f);yield return new WaitForSecondsRealtime(.8f);
            var target=(RectTransform)scroll.content.GetChild(2);float expected=-target.anchoredPosition.y-scroll.viewport.rect.height*.5f;
            Check(Mathf.Abs(scroll.content.anchoredPosition.y-expected)<2,"world: smooth paging centres the requested original island");
            yield return Capture("10-original-world-coin-island.png");
            snap.ScrollToIndex(1,.14f);yield return new WaitForSecondsRealtime(.8f);yield return Capture("11-original-world-dice-island.png");
        }
    }
}

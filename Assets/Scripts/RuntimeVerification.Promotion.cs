using System;
using System.Collections;
using System.Linq;
using Bufu.Rebuild.UI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Bufu.Rebuild {
    public sealed partial class RuntimeVerification {
        IEnumerator VerifyPromotion() {
            var model=game.Model;
            model.State.mission=2;model.State.gold=0;model.State.pendingGoals.Clear();
            SetField("promotionGuideRead",false);Refresh();yield return null;
            var view=Field<UiView>("promotionView");var hud=Field<UiView>("promotionHud");
            GameObject node(string field)=>view.Ref("DownItem",field);
            Check(node("challengeBtn").activeInHierarchy&&!hud.Root.activeSelf,"native promotion entry appears on main item page at growth three");
            Check(!Field<GameObject>("topMain").activeSelf&&Field<GameObject>("levelBadge").activeSelf,"pending promotion replaces honor progress and retains growth badge");
            Check(view.Rect.anchoredPosition==new Vector2(0,-1363)&&node("challengePanel").transform.localEulerAngles.z==0,"native DownPanel placement and zero rotation retained");
            var spine=node("challengeBtn").GetComponent<SpineGraphic>();
            Check(spine&&Math.Abs(spine.AnimationDuration-.5f)<.001f&&spine.UseScaledTime,"native entry Spine idle runs original half-second scaled clip");
            yield return Capture("01-promotion-entry.png");
            Click(node("challengeBtn").GetComponent<Button>(),"enter native promotion challenge");yield return null;
            var run=model.Promotion;Check(run!=null&&model.PromotionRunning&&Time.timeScale==0,"first entry opens native guide and pauses scaled time");
            var dialog=Field<UiView>("promotionDialog");Check(dialog.Ref<TMP_Text>("DialogPanel","dialogText").text==L10n.Translate("60秒内显示搜集足够多的金钱!"),"first-entry dialog text translated to English");
            yield return Capture("02-promotion-guide.png");
            Click(dialog.Ref<Button>("DialogPanel","bgButton"),"dismiss native first-use guide");yield return null;
            Check(Time.timeScale==1&&hud.Root.activeInHierarchy&&!Field<GameObject>("levelBadge").activeSelf,"guide resumes countdown; running HUD hides growth badge");
            Check(game.Field.gameObject.activeSelf&&!game.ModalOpen,"promotion keeps actual field and actors running");
            Check(hud.Nodes.Length==9&&hud.Ref<Image>("ChallengeManager","progress").sprite,"original nine-node progress prefab and sprites loaded");
            var before=run.Remaining;yield return new WaitForSeconds(.2f);Check(run.Remaining<before,"scaled live countdown advances on item tab");
            model.Data.worldEntry=true;game.OpenTab(6);yield return null;before=run.Remaining;
            yield return new WaitForSeconds(.2f);Check(run.Remaining==before&&hud.Root.activeSelf&&!hud.Root.activeInHierarchy,"other tab suspends native countdown while retaining child activeSelf");
            double part=run.Target*.25;model.AddEarnedGold(part,Time.realtimeSinceStartupAsDouble);
            Check(run.Collected==part,"income event still collects while parent is hidden");
            game.OpenTab(1);yield return null;
            Check(hud.Ref<RectTransform>("ChallengeManager","follow").anchoredPosition.x==-180,"progress endpoint follows original minus360 plus720 formula");
            yield return Capture("03-promotion-running.png");
            double wallet=model.State.gold;model.GrantAmount(1,100);Check(run.Collected==part,"internal grants are excluded from promotion collection");
            model.AddEarnedGold(run.Target-part,Time.realtimeSinceStartupAsDouble);yield return null;
            Check(run.Status==PromotionStatus.Won&&node("victoryPanel").activeInHierarchy&&!hud.Root.activeSelf,"success swaps immediately to native victory panel");
            Check(node("victoryPanel").GetComponentsInChildren<Image>().Any(x=>x.sprite==UiAssets.Sprite("ty_pic_challenge_successl_scale2")),"native success artwork retained");
            Check(node("victoryGrid").transform.childCount==run.Rewards.Count&&!run.Claimed,"native rewardKuang entries show prepared, unclaimed rewards");
            Check(Math.Abs(Field<float>("promotionResultTime")-3)<.1,"success auto-confirm timer is three seconds");
            yield return Capture("04-promotion-success.png");
            double reward=run.Rewards[0].Value;wallet=model.State.gold;
            Click(node("victoryBtn").GetComponent<Button>(),"confirm native promotion prize");yield return null;
            Check(run.Claimed&&model.State.gold==wallet+reward&&!node("victoryPanel").activeSelf,"confirmation grants once and closes native success panel");
            Check(!model.ClaimPromotion(),"duplicate reward confirmation cannot pay again");
            yield return new WaitForSeconds(.4f);yield return Capture("05-promotion-reward-flight.png");yield return new WaitForSeconds(2);
            model.State.mission=4;model.State.promotionId=2;model.State.gold=0;Refresh();yield return null;yield return null;
            var entryGraphic=node("challengeBtn").GetComponent<SpineGraphic>();
            Check(entryGraphic.raycastTarget&&entryGraphic.depth>=0&&!entryGraphic.canvasRenderer.cull,"retry entry graphic remains registered for raycasts (depth="+entryGraphic.depth+", culled="+entryGraphic.canvasRenderer.cull+")");
            Click(node("challengeBtn").GetComponent<Button>(),"enter second native promotion");yield return null;
            run=model.Promotion;run.Remaining=.01f;model.TickPromotion(.02f);model.TickPromotion(.01f);yield return null;
            Check(node("failPanel").activeInHierarchy&&run.Status==PromotionStatus.Lost,"timeout shows native failure panel");
            Check(node("failPanel").GetComponentsInChildren<Image>().Any(x=>x.sprite==UiAssets.Sprite("ty_pic_challenge_fail_scale2")),"native failure artwork retained");
            Check(Field<float>("promotionResultTime")>4.8f,"failure auto-confirm timer is five seconds");yield return Capture("06-promotion-failure.png");
            yield return new WaitForSeconds(5.1f);Check(!node("failPanel").activeSelf,"failure closes automatically at original deadline");
            Click(node("challengeBtn").GetComponent<Button>(),"retry failed promotion");yield return null;
            Check(model.Promotion.Collected==0&&model.PromotionRunning,"retry resets actual promotion collection");
            Click(node("closeChallengeBtn").GetComponent<Button>(),"exit native promotion");yield return null;
            Check(!model.PromotionRunning&&!hud.Root.activeSelf&&node("challengeBtn").activeSelf,"exit restores native promotion entry without result or fee");
            Check(errors.Count==0,"promotion has no runtime exceptions or errors");Complete(true,null);
        }
        void SetField(string name,object value)=>typeof(GameController).GetField(name,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(game,value);
    }
}

using System;
using System.Collections;
using System.Linq;
using Bufu.Rebuild.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace Bufu.Rebuild {
    public sealed partial class RuntimeVerification {
        Button ReachableActivityCoin(UiView arena) {
            foreach(var button in arena.Ref<RectTransform>("ChallengeMain","coinPool").GetComponentsInChildren<Button>()) {
                var hit=FirstHit(ScreenPoint((RectTransform)button.transform));
                if(hit&&ExecuteEvents.GetEventHandler<IPointerClickHandler>(hit)==button.gameObject)return button;
            }
            return null;
        }
        IEnumerator VerifyLocalActivities() {
            Check(GameController.VerificationMode,"activities: all activity fixtures use the isolated verification model");
            var model=game.Model;ClearAlignmentGoals();FreezeHelpers();model.State.gold=100000;
            Call("SelectTab",4);yield return null;yield return null;
            var overview=Field<UiView>("activityView");
            Check(overview!=null&&overview.Root.activeInHierarchy,"activities: original challenge and auction overview is reachable");
            CheckEnglish(overview.Root.transform,"activities overview");
            CheckWordArtSet(overview.Root.transform,"timed-challenge","auction-house","challenge","auction");
            yield return Capture("15a-local-activities-overview.png");
            Click(overview.Ref<Button>("ChallengePanel","challengeBtn"),"activities: enter the actual challenge team-selection page");yield return null;
            Click(overview.Ref<Button>("ChallengePanel","zuduiBtn"),"activities: populate the team through the original auto-team button");yield return null;
            double before=model.State.gold,fee=model.CurrentActivityChallenge.costMoney;
            Click(overview.Ref<Button>("ChallengePanel","playBtn"),"activities: pay entry through the real challenge ready button");yield return null;
            var arena=Field<UiView>("activityArena");var run=model.LocalChallenge;
            Check(arena!=null&&run!=null&&!run.Finished&&Math.Abs(model.State.gold-before+fee)<.0001,"activities: challenge entry deducts its configured fee once and starts its own session");
            var backBounds=RootBounds(arena.Ref<RectTransform>("ChallengeMain","backBtn"));var timeBounds=RootBounds(arena.Ref<TMP_Text>("ChallengeMain","time").rectTransform);
            Check(backBounds.yMax<=game.Root.rect.yMax+1&&backBounds.yMin>=game.Root.rect.yMin-1&&timeBounds.yMax<=game.Root.rect.yMax+1,"activities: challenge back control and countdown fit inside the safe screen height");
            CheckEnglish(arena.Root.transform,"challenge arena");yield return Capture("15-local-challenge-arena.png");
            var coin=ReachableActivityCoin(arena);Check(coin,"activities: a challenge coin receives a real pointer raycast");
            Click(coin,"activities: manually throw a real challenge coin");
            // A near-goal fixture keeps this UI regression short; real landing and reward paths remain in use.
            double smallest=Enumerable.Range(1,4).Where(t=>model.LocalChallengeCoinCount(t)>0).Select(model.LocalChallengeCoinValue).Min();
            run.Collected=run.Config.limitGoalFactor-smallest*.5;
            for(int attempt=0;attempt<8&&!run.Finished;attempt++) {
                yield return new WaitForSeconds(1.05f);
                if(!run.Finished){coin=ReachableActivityCoin(arena);if(coin)Click(coin,"activities: retry the actual challenge coin after a tails landing");}
            }
            Check(run.Won&&!run.Claimed&&model.Data.challengeLevel==run.Config.id+1,"activities: a real heads landing reaches victory and advances challenge progress without paying before collection");
            CheckEnglish(arena.Root.transform,"challenge victory");
            yield return null;yield return Capture("16-local-challenge-victory.png");
            Click(arena.Ref<Button>("ChallengeMain","victoryBtn"),"activities: collect the configured challenge prize through its original victory button");yield return null;
            Check(run.Claimed&&(Field<UiView>("activityArena")==null||!Field<UiView>("activityArena").Root),"activities: collecting a challenge settles once and restores the overview");
            yield return null;

            model.Data.auctionCoin=1000000;model.Data.ticketCount=3;
            Click(overview.Ref<Button>("ChallengePanel","auctionEnterBtn"),"activities: open original auction-house selection");yield return null;
            var selection=Field<UiView>("activityAuctionSelection");CheckEnglish(selection.Root.transform,"auction selection");
            Click(selection.Ref<Button>("AuctionSeclect","enterBtn"),"activities: use a local ticket to enter a real auction");yield return null;
            var auction=Field<UiView>("activityAuctionView");var bidding=model.LocalAuction;
            Check(auction!=null&&bidding!=null&&model.Data.ticketCount==2&&bidding.ItemIds.Length==6&&bidding.GivenUp.Length==4,"activities: local auction consumes one ticket and creates the original six items and four logical opponents");
            var bid=auction.Refs("AuctionPlayPanel","bidBtn")[0].GetComponent<Button>();
            Check(!bid.interactable,"activities: the original auction introduction protects bidding before it opens");
            yield return AlignmentWait(()=>bid.interactable,8,"activities: the actual introduction enables bidding normally");
            Click(bid,"activities: place a bid through the real auction control");
            Check(bidding.LastBidder==-1&&model.Data.auctionCoin==1000000,"activities: a player bid becomes the leading offer without deducting money before settlement");
            // Enter the legal raised-bid presentation deterministically, independent of the randomized lot value.
            bidding.RaiseMode=true;Call("RefreshActivityAuction");yield return null;
            foreach(string name in new[]{"tag1","tag2","tag3"}) {
                var label=auction.Node(name).GetComponent<TMP_Text>();label.ForceMeshUpdate();
                Check(!label.isTextOverflowing&&label.rectTransform.rect.width>=260,"auction: English heat labels reserve their own area before the arrow: "+name);
            }
            CheckEnglish(auction.Root.transform,"auction bidding");
            // All opponents having withdrawn is a legal terminal state; leave the production host timer running.
            for(int i=0;i<bidding.GivenUp.Length;i++)bidding.GivenUp[i]=true;
            yield return Capture("17-local-auction-bidding.png");
            yield return AlignmentWait(()=>bidding.Finished,12,"activities: the actual auction host countdown reaches the winning result");
            Check(bidding.PlayerWon&&!bidding.Claimed,"activities: auction victory remains unclaimed until the result button is used");
            CheckEnglish(auction.Root.transform,"auction result");
            Check(!auction.Ref("AuctionPlayPanel","hostImg").activeInHierarchy,"auction: auctioneer no longer obscures any item name or price on the results grid");
            yield return null;yield return Capture("18-local-auction-result.png");
            before=model.Data.auctionCoin;double net=bidding.FinalValue-bidding.Price;
            Click(auction.Ref<Button>("AuctionPlayPanel","freeSoldBtn"),"activities: sell the actual local auction result");yield return null;
            Check(bidding.Claimed&&Math.Abs(model.Data.auctionCoin-before-net)<.0001,"activities: ordinary sale grants final item value minus the winning price exactly once");
            CheckEnglish(auction.Root.transform,"auction settlement");yield return Capture("19-local-auction-settled.png");
            Click(auction.Ref<Button>("AuctionPlayPanel","backBtn"),"activities: return from auction settlement");yield return null;
            Click(selection.Ref<Button>("AuctionSeclect","backBtn"),"activities: close auction-house selection");yield return null;
            Check(!game.ModalOpen,"activities: both local activity flows return without a blocking popup");Call("SelectTab",1);
        }
    }
}

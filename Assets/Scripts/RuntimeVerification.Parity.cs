using System.Collections;
using System.Linq;
using Bufu.Rebuild.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    public sealed partial class RuntimeVerification {
        IEnumerator VerifyParity() {
            var model=game.Model;var scene=Field<UiView>("mainScene");var bg=scene.Ref<RectTransform>("MainScene","bgImage");
            var starts=game.Coins.Select(c=>(Vector2)bg.InverseTransformPoint(c.Rect.position)).ToArray();
            Check(starts.Length==3&&Mathf.Abs(starts[0].x+95)<1&&Mathf.Abs(starts[0].y-295)<1&&
                Mathf.Abs(starts[1].x+87)<1&&Mathf.Abs(starts[1].y+200)<1&&Mathf.Abs(starts[2].x-213)<1&&Mathf.Abs(starts[2].y+150)<1,
                "initial copper coins use the original three staggered background-relative positions");
            yield return Capture("01-original-initial-coins.png");
            var upgrade=Field<UiView>("upgrade");var dots=upgrade.Refs("UpgradePanel","redDots");
            model.State.gold=30;Refresh();yield return null;
            Check(dots[5].activeSelf&&dots[0].activeSelf,"affording copper activates its item subtab and aggregate red dots");
            Check(model.Buy(2,false),"buy copper at exact price");game.SyncActors();yield return null;
            Check(!dots[5].activeSelf&&!dots[0].activeSelf,"spending all gold clears both item red dots");
            Check(game.Coins.Count==4&&game.Coins.Take(3).Select(c=>c.Rect.anchoredPosition).Distinct().Count()==3,"buying an extra coin keeps the original three positions");
            model.State.mission=90;model.State.pendingGoals.Clear();model.State.checkDay=3;model.State.checkDate=GameModel.TodayNumber();
            model.State.gold=1e30;model.Data.diamonds=100000;model.State.points=100;model.RefreshHelperUnlocks(1);Refresh();yield return null;
            Check(dots[1].activeSelf&&dots[2].activeSelf,"facility and talent aggregate red dots activate from real upgrade opportunities");
            Click(upgrade.Ref<Button>("UpgradePanel","button2"),"inspect facility notifications");yield return null;
            var facility=Field<UiView>("facilityView");
            Check(facility.Ref("FacilityPanel","helperTabRedDot").activeSelf&&facility.Ref("FacilityPanel","deskTabRedDot").activeSelf,"facility child red dots match available helper and desk upgrades");
            foreach(var helper in model.State.helpers)helper.level=model.HelperConfig(helper.id).upgradeLevelMax;
            foreach(var desk in model.desks.Where(x=>x.WorldId==1))model.State.deskLevel[desk.id]=desk.skillLevelMax;
            Refresh();yield return null;
            Check(!dots[1].activeSelf&&!facility.Ref("FacilityPanel","helperTabRedDot").activeSelf&&!facility.Ref("FacilityPanel","deskTabRedDot").activeSelf,
                "capping all helpers and desks clears both child red dots and their aggregate");
            Click(upgrade.Ref<Button>("UpgradePanel","button3"),"inspect talent notifications");yield return null;
            var talents=Field<UiView>("talentView");
            Check(talents.Refs("TalentManager","tabRepoint").Any(x=>x.activeSelf),"talent page red dots identify affordable talents");
            model.State.points=0;Refresh();yield return null;
            Check(!dots[2].activeSelf&&talents.Refs("TalentManager","tabRepoint").All(x=>!x.activeSelf),"spending talent points clears page and aggregate red dots");
            Click(upgrade.Ref<Button>("UpgradePanel","button1"),"return to items");yield return null;
            var top=Field<UiView>("top");Click(top.Ref<Button>("TopPanel","checkInBtn"),"open complete original sign-in panel");yield return null;
            var signIn=SignInPopup();
            var free=signIn.Ref<RectTransform>("DailyCheckIn","freeRewardListParent");var special=signIn.Ref<RectTransform>("DailyCheckIn","adRewardListParent");
            Check(free.childCount==7&&special.childCount==7&&special.gameObject.activeInHierarchy,"sign-in shows seven free and seven special reward rows");
            foreach(var parent in new[]{free,special})foreach(Transform row in parent) {
                var visible=row.GetComponentsInChildren<Image>().Count(x=>x.sprite&&x.sprite.name=="tu_pic_redDot");
                Check(visible==(row.GetSiblingIndex()<3?1:0),"one row-level sign-in red dot, with future rewards hidden: "+row.name);
                Check(!row.Find("rewardbtn1/redpoint").gameObject.activeSelf&&!row.Find("rewardbtn2/redpoint").gameObject.activeSelf,"sign-in reward slots do not duplicate the row red dot: "+row.name);
            }
            Check(top.Ref("TopPanel","checkRepoint").activeSelf,"sign-in entrance reports available rewards from either column");
            VerifyCheckInBadgeBounds(free,special);
            yield return Capture("02-sign-in-complete-two-columns.png");
            Click(free.GetChild(0).Find("allRewardBtn").GetComponent<Button>(),"claim first free sign-in row");yield return null;
            var receipt=RewardPopup();Click(receipt.Ref<Button>("RewardBoxUI","GetButton"),"close sign-in receipt");yield return null;
            Check(model.CheckInClaimed(1)&&!model.CheckInClaimed(1,true)&&!free.GetChild(0).Find("allredpoint").gameObject.activeSelf&&special.GetChild(0).Find("allredpoint").gameObject.activeSelf,
                "free claim clears only its own red dot and preserves the special reward");
            Click(special.GetChild(0).Find("allRewardBtn").GetComponent<Button>(),"start special sign-in reward ad");yield return null;
            Check(!model.CheckInClaimed(1,true),"special reward is not granted before the offline ad completes");
            yield return new WaitForSecondsRealtime(2.2f);
            var ad=Field<GameObject>("originalAd");Click(ad.transform.Find("CloseButton").GetComponent<Button>(),"finish special sign-in reward ad");yield return null;yield return null;
            Check(model.CheckInClaimed(1,true)&&!special.GetChild(0).Find("allredpoint").gameObject.activeSelf,"completed special reward clears its independent red dot");
            receipt=RewardPopup();Click(receipt.Ref<Button>("RewardBoxUI","GetButton"),"close special sign-in receipt");yield return null;
            yield return Capture("03-sign-in-after-both-claims.png");
            Click(signIn.Ref<Button>("DailyCheckIn","closeButton"),"close sign-in panel");yield return null;
            model.State.mission=2;model.State.gold=0;model.State.averagePerMinute=1;model.State.pendingGoals.Clear();
            model.State.checkDay=7;model.State.helpers.RemoveAll(x=>x.id==4||x.id==7);Refresh();yield return null;
            Click(top.Ref<Button>("TopPanel","checkInBtn"),"inspect unowned special helper rewards");yield return null;
            signIn=SignInPopup();special=signIn.Ref<RectTransform>("DailyCheckIn","adRewardListParent");
            Check(special.GetChild(2).Find("rewardbtn2/huoqv").gameObject.activeSelf&&special.GetChild(6).Find("rewardbtn2/huoqv").gameObject.activeSelf,
                "unowned day-three and day-seven helpers show the original acquire badges");
            VerifyCheckInBadgeBounds(signIn.Ref<RectTransform>("DailyCheckIn","freeRewardListParent"),special);
            yield return Capture("04-sign-in-special-helper-rewards.png");
            int shards=model.Fragments(4);
            Click(special.GetChild(2).Find("allRewardBtn").GetComponent<Button>(),"claim day-three kungfu master");yield return null;
            yield return new WaitForSecondsRealtime(2.2f);
            ad=Field<GameObject>("originalAd");Click(ad.transform.Find("CloseButton").GetComponent<Button>(),"finish day-three special reward ad");yield return null;yield return null;
            Check(model.HelperOwned(4)&&model.HelperOf(4).ad==1&&model.Fragments(4)==shards&&model.CheckInClaimed(3,true),
                "actual day-three special reward unlocks kungfu master without granting duplicate shards");
            receipt=RewardPopup();Click(receipt.Ref<Button>("RewardBoxUI","GetButton"),"close helper unlock receipt");yield return null;
            Check(!special.GetChild(2).Find("rewardbtn2/huoqv").gameObject.activeSelf&&!special.GetChild(2).Find("allredpoint").gameObject.activeSelf,
                "claimed helper reward removes both its acquire badge and red dot");
            yield return Capture("05-sign-in-special-helper-unlocked.png");
            Click(signIn.Ref<Button>("DailyCheckIn","closeButton"),"close completed special helper check");yield return null;
            Check(errors.Count==0,"notifications and full sign-in panel have no runtime errors");Complete(true,null);
        }
        UiView SignInPopup()=>Field<UiView>("checkInView");
        UiView RewardPopup()=>Field<UiView>("rewardBoxView");
        void VerifyCheckInBadgeBounds(params RectTransform[] columns) {
            Canvas.ForceUpdateCanvases();
            foreach(var column in columns)foreach(RectTransform row in column) {
                var dot=(RectTransform)row.Find("allredpoint");if(!dot.gameObject.activeInHierarchy)continue;
                var bounds=RootBounds(dot);var card=RootBounds(row);var viewport=RootBounds((RectTransform)column.parent);
                Check(card.Contains(bounds.min)&&card.Contains(bounds.max),"sign-in badge stays fully inside its card: "+row.name);
                Check(viewport.Contains(bounds.min)&&viewport.Contains(bounds.max),"sign-in badge stays fully inside the column mask: "+row.name);
            }
        }
    }
}

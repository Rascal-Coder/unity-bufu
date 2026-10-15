using System.Collections;
using System.Linq;
using Bufu.Rebuild.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    public sealed partial class RuntimeVerification {
        IEnumerator VerifyDeveloperMode() {
            var model=game.Model;var upgrade=Field<UiView>("upgrade");
            Check(model.DeveloperMode&&model.WorldUnlocked(2)&&model.State.gold>=1e30,"dev runtime starts unlocked with debug resources");
            Check(game.Coins.Count>3&&game.Field.GetComponentsInChildren<HelperActor>().Length==model.helperRoles.Length,"dev renders all currency types and all ten helper actors");
            var badge=Field<RectTransform>("systemLayer").Find("Developer Mode").GetComponent<Button>();
            Click(badge,"open developer controls");yield return null;
            var panel=Field<GameObject>("developerPanel");
            Check(panel&&game.ModalOpen,"developer panel blocks game input while open");
            model.Data.diamonds=0;model.State.gold=0;model.State.points=0;model.Data.shards[109]=0;
            Click(panel.transform.Find("Controls/Refill").GetComponent<Button>(),"refill developer resources");
            Check(model.Data.diamonds>=1e6&&model.State.gold>=1e30&&model.State.points>=1000000&&model.Data.shards[109]>=100000,"dev refill UI restores every resource type");
            yield return Capture("01-dev-controls.png");
            Click(panel.transform.Find("Controls/Switch World").GetComponent<Button>(),"dev quick world switch");yield return null;
            Check(model.Data.world==2&&game.Coins.Count>3,"dev quick switch renders dice world");
            Click(panel.transform.Find("Controls/Close").GetComponent<Button>(),"close developer controls");yield return null;
            Check(!game.ModalOpen,"closing developer panel restores gameplay input");
            foreach(int page in new[]{2,3,1}) {
                Click(upgrade.Ref<Button>("UpgradeItemPanel","button"+page),"dev dice item subpage "+page);yield return null;
                yield return VerifyScroll(upgrade.Find("ItemPanel/panel"+page+"/Scroll View").GetComponent<ScrollRect>(),"dev dice list "+page,64);
            }
            yield return Capture("02-dev-dice.png");
            foreach(int tab in new[]{2,3,5,6}) {
                Click(upgrade.Ref<Button>("UpgradePanel","button"+tab),"dev unlocked main tab "+tab);yield return null;
                CheckGrowthBadge(false,"dev main tab "+tab);
            }
            Click(upgrade.Ref<Button>("UpgradePanel","button2"),"dev helper list");yield return null;
            var helperContent=Field<UiView>("facilityView").Ref<RectTransform>("HelperPanel","contentParent");
            Check(helperContent.Find("helper-9")!=null,"dev event helper appears in actual helper list");
            yield return Capture("03-dev-helpers.png");
            Click(upgrade.Ref<Button>("UpgradePanel","button5"),"dev shop");yield return null;
            model.Data.shopDate=GameModel.TodayNumber();model.Data.chestNormal=1000;model.Data.diamondClaims=1000;
            model.Data.diamondReadyAt=long.MaxValue;model.Data.spinReadyAt=long.MaxValue;model.Data.spinCount=model.WheelLimit;Refresh();
            var shop=Field<UiView>("shopView");double diamonds=model.Data.diamonds;
            Click(shop.Ref<Button>("ShopTurntablePanel","button202"),"dev chest beyond daily limit");yield return null;
            Check(model.Data.diamonds==diamonds-50,"dev chest UI bypasses daily cap and settles purchase");
            Click(ShopRewardPopup().Find("backBtn").GetComponent<Button>(),"close dev chest receipt");yield return null;
            Check(shop.Ref<Button>("ShopTurntablePanel","spinAdButton").interactable&&!model.DiamondCooling,"dev shop controls ignore cooldown and daily limits");
            yield return Capture("04-dev-shop.png");
            Click(badge,"reopen developer controls");yield return null;
            panel=Field<GameObject>("developerPanel");
            Click(panel.transform.Find("Controls/Reset Dev").GetComponent<Button>(),"reset developer profile");yield return null;yield return null;
            Check(game.Model!=model&&game.Model.DeveloperMode&&game.Model.HelperOwned(9)&&game.Model.Data.world==1&&game.Model.WorldUnlocked(2),"reset keeps dev mode and all content unlocked");
            Check(!game.ModalOpen,"dev reset cleans popup state");
            yield return Capture("05-dev-reset.png");
            yield return VerifyHelperVisuals();
            yield return VerifyWorldLayout();
            Check(errors.Count==0,"dev has no runtime errors or exceptions");Complete(true,null);
        }
    }
}

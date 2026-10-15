using System.Linq;

namespace Bufu.Rebuild {
    public sealed partial class GameController {
        // UpgradePanel.redDots: main item/helper/talent/collection/shop, coin item,
        // upgrades, world, dice item, dice skills. Item children aggregate into dot 0.
        void RefreshNotificationDots() {
            var dots=upgrade.Refs("UpgradePanel","redDots");
            bool coin=Model.Data.world==1&&Model.quantities.Any(Model.UpgradeNotification);
            bool dice=Model.Data.world==2&&Model.quantities.Any(Model.UpgradeNotification);
            bool upgrades=Model.boosts.Any(Model.UpgradeNotification);
            bool skills=Model.Data.world==2&&Model.skills.Any(Model.UpgradeNotification);
            bool[] states={coin||dice||upgrades||skills,
                TabUnlocked(2)&&(Model.AnyHelperUpgradeNotification||Model.AnyDeskUpgradeNotification),
                TabUnlocked(3)&&Model.Talents.Any(Model.TalentUpgradeable),false,false,coin,upgrades,false,dice,skills};
            for(int i=0;i<dots.Length;i++)On(dots[i],i<states.Length&&states[i]);
        }
    }
}

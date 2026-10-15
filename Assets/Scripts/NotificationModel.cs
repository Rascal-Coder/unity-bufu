using System;
using System.Linq;

namespace Bufu.Rebuild {
    public sealed partial class GameModel {
        // UpgradeItem.UpdateButtonState includes ad upgrades and the helper ad-cap action.
        public bool UpgradeNotification(Upgrade item) {
            if(item==null||item.worldId!=Data.world||!Unlocked(item)||AtCap(item,false))return false;
            int kind=KindOf(item);
            if(item.costType==3||(kind==0&&item.id==BaseId+1&&HelperGated))return true;
            double price=PriceKind(item.id,kind);
            return Finite(price)&&price>0&&(item.costType==2?Data.diamonds>=Math.Ceiling(price):State.gold>=price);
        }
        public bool AnyHelperUpgradeNotification=>helperRoles.Any(r=>CanUpgradeHelper(r.id));
        public bool AnyDeskUpgradeNotification=>desks.Any(d=>d.WorldId==Data.world&&DeskUpgradeable(d));
    }
}

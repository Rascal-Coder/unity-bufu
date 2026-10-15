using System;
using System.Linq;
using UnityEngine;
using Bufu.Rebuild;

public static partial class DevModeChecks {
    public static void Verify(Action<bool,string> check) {
        var normal=new GameModel();var dev=new GameModel(null,true);
        check(!normal.DeveloperMode&&!normal.WorldUnlocked(2)&&!normal.RefillDeveloperResources()&&normal.State.gold==0,"dev: normal model retains progression and cannot refill");
        check(SaveStore.PathForMode(true)!=SaveStore.PathForMode(false)&&SaveStore.PathForMode(true).EndsWith("bufu-local-dev-v1.json"),"dev: normal and developer save paths are isolated");
        for(int w=1;w<=2;w++) {
            if(dev.Data.world!=w)dev.SwitchWorld(w);
            check(dev.WorldUnlocked(w)&&dev.WorldEntryUnlocked&&dev.PendingGoal==null,"dev: world unlocked without queued growth popups "+w);
            check(dev.quantities.Concat(dev.boosts).Concat(dev.skills).Where(x=>x.worldId==w).All(dev.Unlocked),"dev: every item and skill unlocked "+w);
            check(dev.Talents.All(dev.TalentUnlocked)&&dev.desks.Where(x=>x.WorldId==w).All(dev.DeskUnlocked),"dev: every talent and desk unlocked "+w);
            check(dev.helperRoles.All(r=>dev.HelperOwned(r.id)&&dev.HelperOf(r.id).level==r.upgradeLevelMax)&&dev.AssignedHelpers==dev.helperRoles.Length,"dev: all helpers including event helper owned, awakened and deployed "+w);
            check(Enumerable.Range(1,4).All(q=>dev.Count(q)>0)&&dev.Helpers<=40&&GameModel.Finite(dev.Value(1,true,false)),"dev: all currency qualities available with bounded actors and finite income "+w);
            check(dev.CanClaimCheckIn(7)&&dev.BuyTalent(dev.Talents.First(t=>dev.State.talents[t.id]<t.upgradeLevelMax).id),"dev: day seven and talent purchase immediately available "+w);
            check(dev.Buy(dev.BaseId+1,false),"dev: hiring past initial ad gate works "+w);
        }
        dev.Data.shopDate=GameModel.TodayNumber();dev.Data.spinCount=dev.WheelLimit;dev.Data.spinReadyAt=long.MaxValue;
        dev.Data.diamondReadyAt=long.MaxValue;dev.Data.diamondClaims=1000;dev.Data.chestNormal=1000;
        check(dev.CanSpin&&!dev.SpinCooling&&dev.SpinCooldownLeft==0&&dev.Spin(.5f)!=null,"dev: wheel bypasses daily cap and cooldown");
        check(!dev.DiamondCooling&&dev.DiamondCooldownLeft==0&&dev.ClaimDiamondAd()&&dev.ClaimDiamondAd()&&dev.BuyChest(202),"dev: diamond claims and chests bypass daily cap and cooldown");
        dev.State.gold=0;dev.Data.diamonds=0;dev.State.points=0;dev.Data.shards[109]=0;
        check(dev.RefillDeveloperResources()&&dev.State.gold>=1e30&&dev.Data.diamonds>=1e6&&dev.State.points>=1000000&&dev.Data.shards[109]>=100000,"dev: refill replenishes all resource types including event fragments");
        var restored=new GameModel(JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(dev.Data)),true);
        check(restored.DeveloperMode&&restored.HelperOwned(9)&&restored.WorldUnlocked(2),"dev: independent save roundtrip preserves unlocks");
        check(dev.Rebirth()&&dev.Helpers==dev.helperRoles.Length&&dev.quantities.All(dev.Unlocked)&&dev.PendingGoal==null,"dev: rebirth retains full debug access");
    }
}

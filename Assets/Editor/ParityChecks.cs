using System;
using System.Linq;
using Bufu.Rebuild;

public static class ParityChecks {
    public static void Verify(Action<bool,string> check) {
        var model=new GameModel();model.State.checkDay=3;model.State.checkDate=GameModel.TodayNumber();
        model.State.helpers.RemoveAll(h=>h.id==4);
        check(!model.CanClaimCheckIn(4)&&!model.CanClaimCheckIn(4,true),"sign-in: both columns respect the current day");
        model.State.checkMask=1<<1;
        check(model.CheckInClaimed(1)&&!model.CheckInClaimed(1,true)&&model.AnyCheckInClaimable(),"sign-in: old free claims preserve the independent special reward and red dot");
        double diamonds=model.Data.diamonds;
        check(model.ClaimCheckIn(3,true,out var unlocked)&&model.HelperOwned(4)&&model.HelperOf(4).ad==1&&model.Fragments(4)==0&&model.Data.diamonds==diamonds+8,
            "sign-in: day-three special reward unlocks kungfu master once instead of granting its shards");
        check(unlocked.Count==1&&unlocked[0].Key==2&&!model.ClaimCheckIn(3,true,out _),"sign-in: helper unlock receipt and duplicate protection match the actual reward");
        model.State.checkDay=7;
        if(!model.HelperOwned(7))model.State.helpers.Add(new HelperState{id=7});
        int shards=model.Fragments(7);
        check(model.ClaimCheckIn(7,true,out var owned)&&model.Fragments(7)==shards+2&&owned.Any(x=>x.Key==107&&x.Value==2),
            "sign-in: owned day-seven helper receives the configured two shards");
        check(model.ClaimCheckIn(5,false,out var random)&&random.Where(x=>x.Key>=101).Sum(x=>x.Value)==5&&random.All(x=>x.Key<3||x.Key>5),
            "sign-in: random rarity shards resolve once and the receipt names concrete helpers");
        var copy=new GameModel(UnityEngine.JsonUtility.FromJson<SaveData>(UnityEngine.JsonUtility.ToJson(model.Data)));
        check(copy.CheckInClaimed(3,true)&&copy.CheckInClaimed(5)&&copy.CheckInClaimed(7,true),"sign-in: both reward columns survive a save roundtrip");
        copy.Data.world=2;
        check(!copy.CheckInClaimed(3,true)&&!copy.CheckInClaimed(5),"sign-in: each world has independent claim states");
        model.AdvanceCheckIn(GameModel.TodayNumber()+1);
        check(model.State.checkDay==1&&model.State.checkMask==0&&model.State.checkAdMask==0,"sign-in: the next seven-day cycle resets both columns");
        check(model.Data.worlds[1].checkDate==GameModel.TodayNumber()+1,"sign-in: crossing the date also advances the inactive world's calendar");
        var onlySpecial=new GameModel();onlySpecial.State.checkDate=GameModel.TodayNumber();onlySpecial.State.checkMask=1<<1;
        check(onlySpecial.AnyCheckInClaimable()&&onlySpecial.ClaimCheckIn(1,true,out _)&&!onlySpecial.AnyCheckInClaimable(),
            "sign-in: entrance red dot remains for special rewards and clears after both columns are claimed");

        var dots=new GameModel();var copper=dots.Config(2,false);double price=dots.Price(2,false);
        dots.State.gold=price-1;check(!dots.UpgradeNotification(copper),"notifications: unaffordable item does not show a red dot");
        dots.State.gold=price;check(dots.UpgradeNotification(copper),"notifications: item red dot appears exactly at its price");
        check(dots.Buy(2,false)&&!dots.UpgradeNotification(copper),"notifications: spending the money clears the item's red dot");
        dots.State.quantity[2]=dots.MaxLevel(copper);dots.State.gold=1e30;
        check(!dots.UpgradeNotification(copper),"notifications: capped upgrades do not show a red dot");
        check(dots.quantities.Where(x=>x.worldId==2).All(x=>!dots.UpgradeNotification(x)),"notifications: the other world's items do not leak into this world's red dots");
    }
}

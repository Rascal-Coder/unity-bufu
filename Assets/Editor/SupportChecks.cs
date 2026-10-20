using System;
using UnityEngine;
using Bufu.Rebuild;

/// <summary>Source-based checks for DownItem's original three rewarded support entries.</summary>
public static class SupportChecks {
    static bool Near(double a,double b)=>Math.Abs(a-b)<=Math.Max(0.00001,Math.Abs(b)*0.000001);
    static GameModel Unlocked() {
        var g=new GameModel();g.State.mission=4;g.State.promotionId=-1;g.Data.diceUnlocked=true;return g;
    }
    public static void Verify(Action<bool,string> check) {
        var locked=new GameModel();
        check(!locked.SupportUnlocked(SupportKind.Money)&&!locked.SupportUnlocked(SupportKind.Income)&&!locked.SupportUnlocked(SupportKind.Speed),"support: fresh game hides all three original entries");
        locked.State.mission=2;
        check(!locked.SupportUnlocked(SupportKind.Money)&&!locked.ClaimSupport(SupportKind.Money,500),"support: title level three does not unlock millionaire");
        locked.State.mission=3;
        check(locked.SupportUnlocked(SupportKind.Money)&&!locked.SupportUnlocked(SupportKind.Income)&&!locked.SupportUnlocked(SupportKind.Speed),"support: millionaire unlocks at level four; masters remain locked");
        locked.State.mission=4;
        check(locked.SupportUnlocked(SupportKind.Income)&&locked.SupportUnlocked(SupportKind.Speed),"support: both masters unlock at level five");

        var money=Unlocked();double now=Time.unscaledTime;
        double preview=money.SupportMoney(now);
        check(Near(preview,500)&&money.SupportRemaining(SupportKind.Money)==0&&money.State.gold==0,"support: millionaire preview has original five-hundred floor and no claim side effects");
        money.AddEarnedGold(1000,now);
        double prepared=money.SupportMoney(now);
        check(prepared>preview&&Near(prepared,money.State.averagePerMinute*money.Factor(10)),"support: amount can be recalculated immediately before the ad after income changes");
        check(!money.ClaimSupport(SupportKind.Money,double.NaN)&&!money.ClaimSupport(SupportKind.Money,double.PositiveInfinity)&&!money.ClaimSupport(SupportKind.Money,-1)&&money.SupportRemaining(SupportKind.Money)==0,"support: invalid prepared reward neither pays nor starts cooldown");
        money.State.mission=2;money.State.promotionId=1;var run=money.StartPromotion(now);
        double gold=money.State.gold,rate=money.State.averagePerMinute,lifetime=money.State.lifetime;
        // The level gate is checked at reward time; keep the already-running title challenge for income observation.
        money.State.mission=4;
        check(money.ClaimSupport(SupportKind.Money,prepared)&&Near(money.State.gold,gold+prepared),"support: millionaire pays the exact prepared amount once after successful ad");
        check(run!=null&&run.Status==PromotionStatus.Running&&run.Collected==0&&Near(money.State.averagePerMinute,rate)&&Near(money.State.lifetime,lifetime),"support: millionaire internal payout counts toward neither challenge collection nor rolling earned income");
        check(money.SupportRemaining(SupportKind.Money)>59&&money.SupportRemaining(SupportKind.Money)<=60&&!money.ClaimSupport(SupportKind.Money,prepared),"support: successful millionaire claim starts sixty-second cooldown and blocks repeat payout");
        check(money.State.supportLastTime[2]>0&&money.State.supportLastTime[0]==0&&money.State.supportLastTime[1]==0,"support: original timestamp slots map speed zero, income one, money two");
        money.SwitchWorld(2);money.State.mission=4;
        check(money.SupportRemaining(SupportKind.Money)==0&&money.ClaimSupport(SupportKind.Money,500),"support: second world has an independent millionaire cooldown");
        money.SwitchWorld(1);
        check(money.SupportRemaining(SupportKind.Money)>0,"support: returning world retains its previous cooldown");
        long stamp=money.State.supportLastTime[2];
        check(money.SupportRemaining(SupportKind.Money,stamp+59000)>0&&money.SupportRemaining(SupportKind.Money,stamp+60000)==0,"support: wall-clock cooldown expires exactly at its configured deadline");

        var monopoly=Unlocked();monopoly.State.talents[305]=1;
        check(Near(monopoly.SupportMoney(Time.unscaledTime),500*(1+monopoly.TalentValue(305))),"support: coin-world millionaire multiplier adds one to original talent result");
        monopoly.SwitchWorld(2);monopoly.State.talents[307]=1;
        check(Near(monopoly.SupportMoney(Time.unscaledTime),500*monopoly.TalentValue(307)),"support: dice-world millionaire multiplier uses talent result directly");

        var income=Unlocked();income.State.talents[101]=1;
        double manual=income.Value(1,true,false),automatic=income.Value(1,false,false),alchemy=income.TalentValue(101);
        check(income.ClaimSupport(SupportKind.Income),"support: successful income-master ad activates its buff");
        double expected=(2.0002+alchemy)/(1.0001+alchemy);
        check(Near(income.Value(1,true,false),manual*expected)&&Near(income.Value(1,false,false),automatic*expected),"support: income-master affects manual and helper income, adding its multiplier alongside alchemy");
        check(income.SupportRemaining(SupportKind.Income)>299&&income.SupportRemaining(SupportKind.Income)<=300&&!income.ClaimSupport(SupportKind.Income),"support: income-master lasts five minutes and cannot be reclaimed during cooldown");
        income.ResumeSupportBuffs();
        check(Near(income.Value(1,true,false),manual*(3.0003+alchemy)/(1.0001+alchemy)),"support: resuming original anonymous same-value income buff stacks rather than replacing it");

        var speed=Unlocked();
        check(Near(speed.SupportSpeedMultiplier,1)&&speed.ClaimSupport(SupportKind.Speed)&&Near(speed.SupportSpeedMultiplier,2),"support: original speed-master adds one full helper-speed multiplier");
        speed.SwitchWorld(2);speed.State.mission=4;
        check(Near(speed.SupportSpeedMultiplier,2)&&speed.SupportRemaining(SupportKind.Speed)==0,"support: runtime speed buff remains global while world cooldown is independent");
        check(speed.ClaimSupport(SupportKind.Speed)&&Near(speed.SupportSpeedMultiplier,3),"support: another world's same-value speed buff stacks with the surviving global buff");
        speed.ResumeSupportBuffs();
        check(Near(speed.SupportSpeedMultiplier,4),"support: reopening DownItem reapplies and stacks remaining same-value speed buff");
        speed.State.talents[304]=1;
        check(Near(speed.SupportMinutes(SupportKind.Speed),5.2),"support: original dice cheerleader rounds fifteen seconds to point-two minutes");
        speed.State.talents[304]=2;
        check(Near(speed.SupportMinutes(SupportKind.Speed),5.5),"support: dice cheerleader thirty seconds extends speed cooldown by half a minute");

        var roundtrip=new GameModel(JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(income.Data)));
        check(roundtrip.SupportRemaining(SupportKind.Income)>0&&roundtrip.State.supportLastTime[1]==income.State.supportLastTime[1],"support: per-world cooldown timestamp survives save roundtrip");
        double beforeResume=roundtrip.Value(1,true,false);roundtrip.ResumeSupportBuffs();
        check(Near(roundtrip.Value(1,true,false),beforeResume*(2.0002+alchemy)/(1.0001+alchemy)),"support: loaded save restores one remaining buff when DownItem resumes, not previous transient stack count");
        var legacy=new SaveData();legacy.worlds[0].supportLastTime=null;
        check(new GameModel(legacy).State.supportLastTime.Length==3,"support: old saves without support timestamps initialize a usable array");
        legacy=new SaveData();legacy.worlds[0].supportLastTime=new long[]{11,22};
        var migrated=new GameModel(legacy);
        check(migrated.State.supportLastTime.Length==3&&migrated.State.supportLastTime[0]==11&&migrated.State.supportLastTime[1]==22&&migrated.State.supportLastTime[2]==0,"support: timestamp-array expansion preserves existing cooldown slots like original EnsureSupportArraySize");
        legacy=new SaveData();legacy.worlds[0].supportLastTime=new long[]{11,22,33,44,55,66,77,88};
        migrated=new GameModel(legacy);
        check(migrated.State.supportLastTime.Length>=3&&migrated.State.supportLastTime[0]==11&&migrated.State.supportLastTime[1]==22&&migrated.State.supportLastTime[2]==33,"support: importing original longer support array preserves all three implemented cooldowns");
    }
}

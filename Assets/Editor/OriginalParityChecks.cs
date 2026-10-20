using System;
using System.Linq;
using Bufu.Rebuild;
using UnityEngine;

public static class OriginalParityChecks {
    static bool Near(double value,double expected)=>Math.Abs(value-expected)<Math.Max(1e-7,Math.Abs(expected)*1e-7);
    public static void Verify(Action<bool,string> check) {
        var coin=new GameModel();
        check(Near(coin.Value(1,true,false),5.0005),"parity: default coin includes the shipped 0.0001 common income buff");
        coin.State.mission=7;coin.State.talents[103]=1;
        check(Near(coin.Value(1,true,false)/coin.Value(1,false,false),3),"parity: hand table and click talent add to three rather than multiply to four");
        check(Near(coin.Value(1,false,true)/coin.Value(1,false,false),4),"parity: critical desk adds two to the original two-fold base critical damage");
        double roleCritical=coin.CritMultiplier()+coin.HelperSkillValue(1);
        check(Near(coin.Value(1,false,true,1)/coin.Value(1,false,false,1),roleCritical),"parity: premium helper critical damage retains both base and desk contributions");
        coin.State.mission=29;coin.State.talents[103]=0;
        check(Near(coin.IncomeMultiplier(),2.50025),"parity: 1.5 global table is an additive buff factor of 2.5");
        var talents=new GameModel();talents.State.quantity[1]=4;talents.State.helpers.Clear();
        talents.State.helpers.Add(new HelperState{id=1,count=2});talents.State.helpers.Add(new HelperState{id=3,count=1});
        talents.State.talents[202]=talents.State.talents[204]=talents.State.talents[205]=1;
        check(Near(talents.IncomeMultiplier(),3.5001),"parity: strength, magician and two distinct helpers grant the source-defined global bonuses");
        talents.Data.world=2;talents.State.quantity[101]=6;talents.State.helpers.Clear();
        talents.State.helpers.Add(new HelperState{id=1,count=1});talents.State.helpers.Add(new HelperState{id=3,count=2});talents.State.helpers.Add(new HelperState{id=8,count=1});
        talents.State.talents[203]=talents.State.talents[206]=talents.State.talents[208]=talents.State.talents[210]=talents.State.talents[211]=1;
        check(Near(talents.IncomeMultiplier(),6.8001),"parity: dice talents count specific helpers, R helpers, basic helpers and distinct types independently");
        check(Near(talents.Value(1,false,true)/talents.Value(1,false,false),2.2),"parity: high-end force adds SR critical damage after base critical calculation");
        talents.State.talents[212]=1;
        check(talents.HelperCapacity(1)==2&&talents.AdjustHelperCount(1,1)&&!talents.AdjustHelperCount(1,1),"parity: free labor adds exactly one dispatch place without allowing over-assignment");

        var world=new GameModel();world.Data.diceUnlocked=true;
        world.State.mission=1;world.State.averagePerMinute=30;world.State.lastExit=1000;
        world.Data.worlds[1].mission=1;world.Data.worlds[1].averagePerMinute=60;world.Data.worlds[1].lastExit=1000;
        check(world.SwitchWorld(2,1600)&&Near(world.State.pendingOffline,600)&&world.Data.worlds[0].lastExit==1600,"parity: world entry settles the target world's real absence interval");
        world.MarkActiveWorldExit(2000);
        check(world.Data.worlds[0].lastExit==1600&&world.State.lastExit==2000,"parity: auto-save cannot overwrite the inactive world's exit timestamp");
        check(world.SwitchWorld(1,2200)&&Near(world.State.pendingOffline,300),"parity: returning from another world grants all ten minutes of offline earnings");
        world.ClaimOffline(1,true);double gold=world.State.gold;world.ClaimOffline(1,true);
        check(Near(gold,600)&&world.State.gold==gold,"parity: offline doubled receipt can only be consumed once");
        world.State.lastExit=3000;world.State.averagePerMinute=60;world.ResumeFromBackground(3300);
        check(Near(world.State.pendingOffline,300),"parity: returning to foreground settles the active world without restarting the application");

        var inherited=new SaveData();inherited.worlds[0].helpers.Add(new HelperState{id=1,level=12});
        var skills=new GameModel(inherited);skills.Data.world=2;skills.RefreshHelperUnlocks(2);
        check(skills.HelperOf(1).level==12&&skills.GlobalHelperLevel(1)==12,"parity: old save migration preserves the highest learned helper level for new-world ownership");
        var reborn=new GameModel();reborn.State.gold=1e15;reborn.State.quantity[1]=3;reborn.State.helpers.Clear();reborn.State.helpers.Add(new HelperState{id=1,count=2});reborn.State.boosts[1]=4;
        check(reborn.Rebirth(false)&&reborn.State.quantity[1]==3&&reborn.State.boosts[1]==4&&reborn.HelperCount(1)==2,"parity: advanced rebirth preserves upgrades and dispatched helpers");
        reborn.State.gold=1e30;
        check(reborn.Rebirth()&&reborn.Helpers==0&&reborn.State.boosts[1]==0&&reborn.AssignedHelpers==0,"parity: normal rebirth clears upgrade and dispatch state together");
        var capacity=new GameModel();capacity.State.talents[207]=5;
        check(capacity.MaxLevel(capacity.Config(1,false))==65,"parity: elite education retains all sixty-five legal helper positions");

        var firstEntry=new GameModel();firstEntry.Data.diceUnlocked=true;
        check(!firstEntry.Data.worlds[1].entered&&firstEntry.Data.worlds[1].helpers.Count==0,"parity: untouched worlds do not pre-create helper ownership");
        firstEntry.HelperOf(1).level=30;firstEntry.Data.globalHelperLevels[1]=30;firstEntry.SwitchWorld(2,1600);
        check(firstEntry.HelperOf(1).level==30,"parity: first dice-world entry inherits helper level thirty");
        firstEntry.HelperOf(1).level=12;firstEntry.Validate();
        check(firstEntry.HelperOf(1).level==12,"parity: revisiting an owned helper does not overwrite its independent level");
        var legacy=new SaveData();legacy.worlds[0].helpers.Add(new HelperState{id=1,level=30});legacy.worlds[1].helpers.Add(new HelperState{id=1});legacy.worlds[1].lastExit=1000;
        var migrated=new GameModel(legacy);migrated.Data.diceUnlocked=true;migrated.SwitchWorld(2,2000);
        check(migrated.HelperOf(1).level==30,"parity: legacy phantom ownership in a pristine world is migrated before first entry");
        migrated.Data.worlds[0].gold=77;migrated.Data.worlds[0].boosts[1]=5;migrated.Data.worlds[0].evolution[2]=2;
        migrated.Data.worlds[0].helpers[0].count=1;migrated.State.gold=1e15;migrated.State.averagePerMinute=123;
        check(migrated.Rebirth()&&migrated.Data.worlds[0].boosts[1]==0&&migrated.Data.worlds[0].evolution[2]==0&&migrated.Data.worlds[0].gold==77&&migrated.Data.worlds[0].helpers[0].count==1,"parity: ordinary rebirth resets all-world upgrades while retaining other-world money and helper dispatch");
        check(Near(migrated.State.averagePerMinute,123),"parity: ordinary rebirth retains the saved rolling-income seed");
        var advancedRate=new GameModel();advancedRate.AddEarnedGold(12,Time.unscaledTime);advancedRate.State.gold=1e15;
        double liveRate=advancedRate.State.averagePerMinute;
        bool advancedDone=advancedRate.Rebirth(false);advancedRate.UpdateRate(Time.unscaledTime);
        check(advancedDone&&Near(advancedRate.State.averagePerMinute,liveRate),"parity: advanced rebirth preserves the actual rolling-income entries after the next rate update");
        var snapshot=new GameModel();snapshot.Data.world=2;
        double rolled=snapshot.SnapshotDiceValue(1,6),initial=snapshot.SettlementValue(1,6,true,false,0,1,rolled);
        snapshot.State.boosts[101]++;snapshot.State.evolution[102]++;
        check(Near(snapshot.SettlementValue(1,6,true,false,0,1,rolled),initial),"parity: rolled dice freeze base upgrades, evolution, face and independent multipliers");
        snapshot.State.talents[101]=1;
        check(snapshot.SettlementValue(1,6,true,false,0,1,rolled)>initial,"parity: final income buffs remain live until a rolled die lands");
    }

    public static void Build(){RebuildEditor.BuildCurrent();}
    public static void Check(){RebuildEditor.VerifyCore();Debug.Log("ORIGINAL_PARITY_CHECKS_OK");}
}

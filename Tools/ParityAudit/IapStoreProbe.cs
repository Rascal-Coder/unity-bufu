using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bufu.Rebuild;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Audit-only entry point, copied to the isolated project's Editor folder.
public static class IapStoreProbe {
    [Serializable] public class Entry { public string suite,name,expected,actual; public bool passed; }
    [Serializable] public class Report { public string editor; public int passed,failed; public Entry[] checks; }
    [Serializable] public class BuildReportData { public string result,editor,path; public int errors,warnings;public string[] warningMessages; }
    static readonly List<Entry> entries=new List<Entry>();
    static readonly string Output=Path.GetFullPath("../../Documentation/IapStore-2026-10-20/Run");
    static void Record(string suite,string name,bool pass,string expected="",string actual="") {
        entries.Add(new Entry{suite=suite,name=name,passed=pass,expected=expected,actual=actual});
    }
    static void Suite(string name,Action<Action<bool,string>> verify) {
        try {verify((ok,text)=>Record(name,text,ok));}
        catch(Exception e){Record(name,"suite threw exception",false,"no exception",e.ToString());}
    }
    static void OriginalBehaviorProbes() {
        var initial=new GameModel();initial.State.averagePerMinute=60;initial.State.lastExit=1000;
        double initialReward=initial.AccumulateOfflineWorld(1,1600);
        Record("original-behavior","Initial growth level 1 permits offline earnings",Math.Abs(initialReward-600)<1e-6,"600",initialReward.ToString("R"));

        var pending=new GameModel();pending.State.mission=1;pending.State.averagePerMinute=60;pending.State.lastExit=1000;
        pending.AccumulateOfflineWorld(1,1600);pending.AccumulateOfflineWorld(1,2200);
        Record("original-behavior","An unclaimed offline receipt remains one fixed reward",Math.Abs(pending.State.pendingOffline-600)<1e-6,"600",pending.State.pendingOffline.ToString("R"));

        var inherited=new GameModel();inherited.Data.diceUnlocked=true;
        inherited.HelperOf(1).level=30;inherited.Data.globalHelperLevels[1]=30;
        inherited.SwitchWorld(2,1600);
        int actualLevel=inherited.HelperOf(1)?.level??0;
        Record("original-behavior","First entry inherits the globally trained helper level",actualLevel==30,"30",actualLevel.ToString());

        var reborn=new GameModel();reborn.State.gold=1e15;reborn.Data.worlds[1].boosts[101]=4;
        bool rebornDone=reborn.Rebirth();int otherBoost=reborn.Data.worlds[1].boosts[101];
        Record("original-behavior","Ordinary rebirth resets other-world upgrades as in the reference",rebornDone&&otherBoost==0,"rebirth=true, other boost=0",rebornDone+", "+otherBoost);

        foreach(bool normal in new[]{false,true}) {
            var rate=new GameModel();rate.State.gold=1e15;rate.State.averagePerMinute=123;
            bool done=rate.Rebirth(normal);
            Record("original-behavior",(normal?"Ordinary":"Advanced")+" rebirth retains the saved income seed",done&&Math.Abs(rate.State.averagePerMinute-123)<1e-6,"rebirth=true, rate=123",done+", "+rate.State.averagePerMinute.ToString("R"));
        }

        // Exercise the complete roll snapshot used by the repaired actor.
        var dice=new GameModel();dice.Data.world=2;
        double multiplier=dice.SnapshotDiceValue(1,6),expected=dice.SettlementValue(1,6,true,false,0,1,multiplier);
        dice.State.boosts[101]++;
        double actual=dice.Settle(2,1,6,true,false,1,0,1,multiplier);
        Record("original-behavior","A rolled die retains pre-upgrade base value until landing",Math.Abs(actual-expected)<1e-6,expected.ToString("R"),actual.ToString("R"));
    }
    public static void CheckAndBuild() {
        Directory.CreateDirectory(Output);entries.Clear();
        Suite("developer",DevModeChecks.Verify);Suite("promotion",PromotionChecks.Verify);
        Suite("support",SupportChecks.Verify);Suite("legacy-parity",ParityChecks.Verify);
        Suite("original-parity",OriginalParityChecks.Verify);Suite("dice",DiceParityChecks.Verify);
        Suite("helpers",HelperParityChecks.Verify);Suite("ui",UiParityChecks.Verify);
        Suite("activities",ActivityChecks.Verify);Suite("purchases",PurchaseChecks.Verify);
        Suite("original-behavior",_=>OriginalBehaviorProbes());
        var report=new Report{editor=Application.unityVersion,passed=entries.Count(x=>x.passed),failed=entries.Count(x=>!x.passed),checks=entries.ToArray()};
        File.WriteAllText(Path.Combine(Output,"model-checks.json"),JsonUtility.ToJson(report,true));
        Debug.Log("PARITY_RECHECK_MODELS passed="+report.passed+" failed="+report.failed);
        BuildAuditPlayer();
    }
    public static void BuildAuditPlayer() {
        Directory.CreateDirectory(Output);
        string player=Path.GetFullPath("Build/ParityAuditPlayer/BufuRebuild.exe");Directory.CreateDirectory(Path.GetDirectoryName(player));
        var build=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Intro.unity","Assets/Scenes/Main.unity"},locationPathName=player,target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
        var buildResult=new BuildReportData{editor=Application.unityVersion,result=build.summary.result.ToString(),path=player,errors=build.summary.totalErrors,warnings=build.summary.totalWarnings,warningMessages=build.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Warning).Select(m=>m.content).Distinct().ToArray()};
        File.WriteAllText(Path.Combine(Output,"audit-build.json"),JsonUtility.ToJson(buildResult,true));
        if(build.summary.result!=BuildResult.Succeeded)throw new Exception("Audit player build failed: "+build.summary.result);
        Debug.Log("PARITY_RECHECK_BUILD_OK "+player);
    }
}

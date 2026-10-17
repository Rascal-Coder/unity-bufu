using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using Bufu.Rebuild;

public static class RebuildEditor {
    static readonly List<string> passed=new List<string>();
    static void Assert(bool condition,string name) {if(!condition)throw new Exception("CHECK FAILED: "+name);passed.Add(name);}
    [MenuItem("不服开一局/生成主场景与验证")]
    public static void Setup() {
        passed.Clear();
        Directory.CreateDirectory("Assets/Scenes");Directory.CreateDirectory("Documentation");
        PlayerSettings.companyName="REA Local";PlayerSettings.productName="不服开一局 本地复刻";
        PlayerSettings.defaultScreenWidth=540;PlayerSettings.defaultScreenHeight=960;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
        PlayerSettings.resizableWindow=true;PlayerSettings.runInBackground=true;PlayerSettings.colorSpace=ColorSpace.Gamma;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone,ScriptingImplementation.Mono2x);
        EditorSettings.serializationMode=SerializationMode.ForceText;
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var camera=new GameObject("Camera",typeof(Camera),typeof(AudioListener));camera.GetComponent<Camera>().backgroundColor=new Color(.09f,.11f,.15f);
        camera.GetComponent<Camera>().clearFlags=CameraClearFlags.SolidColor;camera.GetComponent<Camera>().orthographic=true;
        new GameObject("Game");
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/Main.unity");
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Main.unity",true)};
        UiAssetBuilder.BuildFonts();UiAssetBuilder.BuildSpriteAssets();
        AssetDatabase.SaveAssets();NativeParticleBuilder.Build();VerifyCore();VerifyVAT();VerifyUi();
        File.WriteAllText("Documentation/validation.json",JsonUtility.ToJson(new Checks{editor=Application.unityVersion,checks=passed.ToArray(),passed=true},true));
        Debug.Log("REBUILD_VALIDATION_OK "+passed.Count);
    }
    [Serializable] sealed class Checks {public string editor;public bool passed;public string[] checks;}
    public static void Build() {
        Setup();BuildPlayer();
    }
    // Rebuild code fixes using the existing scene and imported UI assets.
    public static void BuildCurrent() {
        passed.Clear();NativeParticleBuilder.Build();VerifyCore();VerifyVAT();VerifyUi();
        Directory.CreateDirectory("Documentation");
        File.WriteAllText("Documentation/validation.json",JsonUtility.ToJson(new Checks{editor=Application.unityVersion,checks=passed.ToArray(),passed=true},true));
        BuildPlayer();
    }
    static void BuildPlayer() {
        Directory.CreateDirectory("Build/Windows");
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Main.unity"},locationPathName="Build/Windows/BufuRebuild.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
        File.WriteAllText("Documentation/build-result.json",JsonUtility.ToJson(new BuildResult{result=report.summary.result.ToString(),errors=report.summary.totalErrors,warnings=report.summary.totalWarnings,bytes=(long)report.summary.totalSize,editor=Application.unityVersion},true));
        if(report.summary.result!=BuildResultType())throw new Exception("Build failed: "+report.summary.result);
        Debug.Log("REBUILD_BUILD_OK "+report.summary.totalSize);
    }
    static UnityEditor.Build.Reporting.BuildResult BuildResultType()=>UnityEditor.Build.Reporting.BuildResult.Succeeded;
    [Serializable] sealed class BuildResult {public string result,editor;public int errors,warnings;public long bytes;}
    public static void VerifyCore() {
        DevModeChecks.Verify(Assert);
        PromotionChecks.Verify(Assert);
        SupportChecks.Verify(Assert);
        ParityChecks.Verify(Assert);
        var g=new GameModel();var copper=g.Config(2,false);var helpers=g.Config(1,false);
        Assert(g.Count(1)==3&&g.State.gold==0,"new game: 3 copper coins, zero gold");
        Assert(GameModel.Cost(copper,0)==30&&GameModel.Cost(copper,1)==45,"original copper early price table");
        Assert(Math.Abs(GameModel.Cost(copper,2)-58.5)<1e-8,"copper geometric continuation");
        double expected=180;for(int level=4;level<=10;level++)expected*=level<10?1.6:2;
        Assert(Math.Abs(GameModel.Cost(helpers,10)-expected)<1e-7,"strict level threshold cost growth");
        Assert(g.Value(1,true,false)==5&&g.Value(2,true,false)==50&&g.Value(3,true,false)==500,"original coin base values");
        Assert(g.Settle(2,1,1,true,false,0)==0&&g.State.gold==0,"reject stale world settlement");
        Assert(g.Settle(1,1,0,true,false,0)==0,"reverse coin pays zero before talent");
        Assert(g.Settle(1,1,1,true,true,0)==10,"critical coin pays 2x");
        Assert(!g.Buy(1,false)&&g.State.gold==10,"unaffordable purchase makes no state change");
        g.State.gold=100;Assert(g.Buy(1,false)&&g.State.gold==75&&g.Helpers==1,"buy helper atomically deducts cost");
        g.State.gold=0;g.State.mission=0;g.State.quantity[2]=0;for(int i=0;i<12;i++)g.Settle(1,1,1,true,false,i);
        Assert(g.State.mission==1&&g.State.gold==60&&g.PendingGoal!=null,"60 coin income completes the first goal");
        Assert(g.ClaimGoal()&&g.State.gold==210&&g.PendingGoal==null,"goal reward 150 granted once on confirm");
        Assert(GameModel.Format(9.6)=="10"&&GameModel.Format(999.9)=="999"&&GameModel.Format(1234)=="1.23K"&&GameModel.Format(12345)=="12.3K"&&GameModel.Format(123456)=="123K"&&GameModel.Format(1.5e15)=="1.50AA","original number formatter");
        var hire=new GameModel();hire.State.quantity[1]=10;hire.State.gold=1e30;
        Assert(hire.HelperGated&&!hire.Buy(1,false)&&hire.UnlockHelperCap()&&hire.Helpers==11&&hire.HelperCap()==15,"helper ad step unlocks for free");
        var off=new GameModel();off.State.mission=1;off.State.averagePerMinute=20;off.State.lastExit=1000;
        Assert(off.AccumulateOffline(1119)==0,"offline minimum 120 seconds");off.State.lastExit=1000;
        Assert(Math.Abs(off.AccumulateOffline(22600)-7200)<1e-8,"offline capped at six hours");off.State.gold=0;off.ClaimOffline();double claimed=off.State.gold;off.ClaimOffline();
        Assert(off.State.gold==claimed&&off.State.pendingOffline==0,"offline reward claimed once");
        var evo=new GameModel();evo.State.quantity[2]=30;Assert(evo.CanEvolve(1)&&evo.Evolve(1)&&evo.Count(1)==1&&evo.Value(1,true,false)==50,"evolution compacts coins and multiplies income");
        var restored=new GameModel(JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(evo.Data)));Assert(restored.Count(1)==1&&restored.State.evolution[2]==1,"save JSON roundtrip retains evolution");
        var dice=new GameModel();dice.Data.worlds[0].mission=10;Assert(dice.SwitchWorld(2)&&dice.Count(1)==3,"world 2 unlock preserves independent progress");
        Assert(dice.Settle(2,1,6,true,false,0)==6,"dice face six settlement");
        Assert(!GameModel.Finite(double.NaN)&&!GameModel.Finite(double.PositiveInfinity),"non-finite reward guard");
        var prizes=new GameModel();prizes.Data.diamonds=500;
        Assert(prizes.BuyChest(202,out var normal)&&normal.Sum(x=>x.Value)==4&&normal.All(x=>x.Key>=101&&x.Key<=104),"normal chest resolves four usable N/R helper fragments");
        Assert(normal.Where(x=>prizes.HelperConfig(x.Key-100).Rare==1).Sum(x=>x.Value)==2&&normal.Where(x=>prizes.HelperConfig(x.Key-100).Rare==2).Sum(x=>x.Value)==2,"normal chest preserves two N and two R fragments");
        Assert(normal.All(x=>prizes.Data.shards[x.Key]==x.Value)&&prizes.Data.diamonds==450,"chest receipt matches inventory and diamond deduction");
        var earned=normal.First(x=>prizes.HelperConfig(x.Key-100).Rare==1);int helperId=earned.Key-100;
        prizes.State.mission=30;prizes.RefreshHelperUnlocks(1);prizes.HelperOf(helperId).level=4;prizes.State.gold=1e12;
        int earnedBefore=prizes.Fragments(helperId),earnedCost=prizes.HelperFragmentCost(prizes.HelperConfig(helperId),4);
        Assert(earnedCost>0&&prizes.UpgradeHelper(helperId)&&prizes.Fragments(helperId)==earnedBefore-earnedCost,"chest fragments can be consumed by actual helper upgrades");
        Assert(prizes.BuyChest(203,out var rare)&&rare.Sum(x=>x.Value)==6&&rare.All(x=>x.Key>=103&&x.Key<=110),"rare chest resolves six usable R/SR helper fragments");
        Assert(rare.Where(x=>prizes.HelperConfig(x.Key-100).Rare==2).Sum(x=>x.Value)==3&&rare.Where(x=>prizes.HelperConfig(x.Key-100).Rare==3).Sum(x=>x.Value)==3&&rare.All(x=>x.Key!=109),"rare chest preserves three R and three obtainable SR fragments");
        prizes.State.averagePerMinute=123;
        var goldPrize=prizes.shop.First(x=>x.id==101);var receipt=prizes.BuildShopRewards(goldPrize);double goldBefore=prizes.State.gold;
        prizes.GrantShop(goldPrize,receipt);
        Assert(receipt.Count==1&&receipt[0].Value==369&&prizes.State.gold-goldBefore==369,"wheel gold receipt uses actual income multiplier");
        prizes.Data.spinProgress=GameModel.SpinProgressMax;
        Assert(prizes.ClaimSpinChest(out var bonus)&&bonus.Sum(x=>x.Value)==2&&bonus.All(x=>x.Key>=105&&x.Key<=110),"progress chest resolves two SR helper fragments");
        Assert(!prizes.ClaimSpinChest(out _)&&prizes.Data.spinProgress==0,"progress chest cannot be claimed twice");
        prizes.Data.diamonds=0;int fragmentsBefore=prizes.Data.shards.Sum();
        Assert(!prizes.BuyChest(202,out var denied)&&denied==null&&prizes.Data.shards.Sum()==fragmentsBefore,"unaffordable chest grants no fragments");
        var legacySave=new SaveData();legacySave.shards[3]=4;legacySave.shards[4]=3;legacySave.shards[5]=2;legacySave.shards[110]=7;
        var migrated=new GameModel(legacySave);
        Assert(migrated.Data.shards[3]+migrated.Data.shards[4]+migrated.Data.shards[5]==0&&migrated.Data.shards.Skip(101).Sum()==16&&migrated.Data.shards[110]>=7,"legacy generic fragments migrate without losing existing helper fragments");
        var migratedAgain=new GameModel(JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(migrated.Data)));
        Assert(migratedAgain.Data.shards.SequenceEqual(migrated.Data.shards),"fragment migration is stable after save and reload");
        foreach(var c in g.quantities.Concat(g.boosts)){Assert(c.costMoneyBuff.Length==c.costMoneyLevel.Length&&c.baseCostMoney.Length>0,"config cost schema "+c.id+" "+c.itemName);}
    }
    static void VerifyUi() {
        var font=Resources.Load<TMPro.TMP_FontAsset>("UI/Fonts/hdhSDF");
        Assert(font&&font.characterTable.Count>7000&&font.atlasTextures.Length>0&&font.material,"original TMP font hdhSDF rebuilt");
        Assert(font.HasCharacters("不服开一局铜币升级设施天赋商城世界签到离线收益"),"original font covers core UI text");
        var digits=Resources.Load<TMPro.TMP_SpriteAsset>("UI/SpriteAssets/ty_num_both");
        Assert(digits&&digits.spriteCharacterTable.Count>=77&&digits.spriteSheet,"original number sprite asset rebuilt");
        var index=(Dictionary<string,object>)Bufu.Rebuild.UI.MiniJson.Parse(Resources.Load<TextAsset>("UI/prefabs").text);
        var host=new GameObject("UiVerify",typeof(RectTransform));
        try {
            int count=0;
            foreach(Dictionary<string,object> p in (List<object>)index["prefabs"]) {
                var view=Bufu.Rebuild.UI.UiPrefab.Instantiate((string)p["resource"],host.transform);
                if(view.Nodes.Length!=Convert.ToInt32(p["nodes"]))throw new Exception("Node count mismatch "+p["resource"]);
                foreach(var scroll in view.Root.GetComponentsInChildren<UnityEngine.UI.ScrollRect>(true)) {
                    if(!scroll.content||!scroll.content.GetComponent<UnityEngine.UI.GridLayoutGroup>())continue;
                    var fit=scroll.content.GetComponent<UnityEngine.UI.ContentSizeFitter>();
                    if(scroll.vertical&&(!fit||fit.verticalFit!=UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize))throw new Exception("Grid content does not grow: "+p["resource"]);
                }
                UnityEngine.Object.DestroyImmediate(view.Root);count++;
            }
            Assert(count>=80,"all original UI prefabs instantiate ("+count+")");
            Assert(Bufu.Rebuild.UI.UiAssets.Sprite("wupin_btn_shengji01")&&Bufu.Rebuild.UI.UiAssets.SpriteAt("Assets/GameRes/honor/1.png"),"original sprites resolve by key and asset path");
        } finally {UnityEngine.Object.DestroyImmediate(host);}
    }
    public static void PlayWakeCheck() {
        PlayModeWindow.SetCustomRenderingResolution(540,960,"Wake tip");
        Debug.Log("WAKE_QA_ENTER_PLAY");
        EditorApplication.delayCall+=()=>EditorApplication.EnterPlaymode();
    }
    static void VerifyVAT() {
        var files=Directory.GetFiles("Assets/Resources/Art/Vat","*.json");Assert(files.Length==61,"all 61 original VAT assets imported");
        Assert(Resources.Load<AudioClip>("Audio/coinup")!=null,"original coin sound imported as WAV");
        foreach(var path in files) {
            var data=JsonUtility.FromJson<VatData>(File.ReadAllText(path));string bin=path.Replace(".json","_positions.bytes");var raw=File.ReadAllBytes(bin);
            Assert(raw.Length==data.width*data.height*data.depth*16,"VAT float32 byte dimensions "+data.name);
            foreach(var s in data.segments) {
                if(s.vertices>data.height||s.frames>data.width||s.slice>=data.depth||s.uv.Length!=s.vertices*2||s.colors.Length!=s.vertices*4||s.triangles.Any(x=>x<0||x>=s.vertices))throw new Exception("Invalid VAT segment "+data.name+"/"+s.animation);
                if(!Resources.Load<Texture2D>("Art/Textures/"+s.texture))throw new Exception("Missing atlas "+s.texture);
            }
        }
    }
}

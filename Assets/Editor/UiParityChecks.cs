using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bufu.Rebuild;
using Bufu.Rebuild.UI;
using UnityEngine;

public static class UiParityChecks {
    public static void Verify(Action<bool,string> check) {
        var model=new GameModel();
        var receipt=new List<KeyValuePair<int,double>>{
            new KeyValuePair<int,double>(1,125),new KeyValuePair<int,double>(2,8),new KeyValuePair<int,double>(104,3)};
        double activeGold=model.State.gold,otherGold=model.Data.worlds[1].gold,diamonds=model.Data.diamonds;
        int shards=model.Data.shards[104];
        model.GrantRewardReceipt(2,receipt);
        check(model.State.gold==activeGold&&model.Data.worlds[1].gold==otherGold+125&&model.Data.diamonds==diamonds+8&&model.Data.shards[104]==shards+3,
            "UI parity: a double reward adds exactly one resolved receipt to its original world without rerolling shards");
        model.Data.worlds[1].pendingOffline=75;model.ClaimOffline(2,true);
        check(model.Data.worlds[1].pendingOffline==0&&model.Data.worlds[1].gold==otherGold+275&&model.State.gold==activeGold,
            "UI parity: offline double settles the displayed world's pending gold twice and leaves the active wallet alone");
        model.ClaimOffline(2,true);
        check(model.Data.worlds[1].gold==otherGold+275,"UI parity: repeated offline completion cannot claim the same pending reward twice");
        var curve=typeof(GameController).GetMethod("ShopSpinDistance",BindingFlags.Static|BindingFlags.NonPublic);
        float Distance(float elapsed)=>(float)curve.Invoke(null,new object[]{elapsed,4248f,6f});
        float speed=4254f/2.95f;
        check(Mathf.Abs(Distance(.8f)-.4f*speed)<.002f&&Mathf.Abs(Distance(2.5f)-2.1f*speed)<.002f&&Mathf.Abs(Distance(4.2f)-4254f)<.002f&&Distance(4.5f)==4248f,
            "UI parity: wheel uses original accelerate, cruise, decelerate and overshoot-return boundaries");
        check(Distance(4.3f)>Distance(4.5f)&&Distance(0)==0,"UI parity: the wheel settles back from its overshoot to the selected slot");
        var merge=typeof(GameController).GetMethod("EvolutionMergeEase",BindingFlags.Static|BindingFlags.NonPublic);
        var shrink=typeof(GameController).GetMethod("EvolutionShrinkEase",BindingFlags.Static|BindingFlags.NonPublic);
        var shake=typeof(GameController).GetMethod("EvolutionShakeOffset",BindingFlags.Static|BindingFlags.NonPublic);
        check(Mathf.Abs((float)merge.Invoke(null,new object[]{.25f})-.15625f)<.00001f&&Mathf.Abs((float)shrink.Invoke(null,new object[]{.25f})-.4375f)<.00001f,
            "UI parity: evolution uses original SmoothStep merge and OutQuad shrink samples");
        var offset=(Vector2)shake.Invoke(null,new object[]{0f});
        check(offset.x==0&&Mathf.Abs(offset.y-6.6f)<.00001f,"UI parity: evolution shaking includes the original vertical cosine displacement");
        model.Data.worlds[0].averagePerMinute=11;model.Data.worlds[1].averagePerMinute=29;
        var incomeSlot=model.WheelItems.First(x=>x.rewardType.Contains(1));int incomeIndex=Array.IndexOf(incomeSlot.rewardType,1);
        var wheelReceipt=model.BuildShopRewards(incomeSlot,2);
        check(wheelReceipt.First(x=>x.Key==1).Value==29*incomeSlot.count[incomeIndex]&&model.Data.world==1,
            "UI parity: a wheel receipt samples the original world's average at completion without changing the active world");
        var version=typeof(GameController).GetField("ResourcePackageVersion",BindingFlags.Static|BindingFlags.NonPublic);
        check((string)version.GetRawConstantValue()=="2026-10-14-626","UI parity: settings show the pinned shipped content version rather than the Unity application version");
        var display=model.worlds.OrderByDescending(w=>w.sort).ThenByDescending(w=>w.id).ToArray();
        check(display.Length==2&&display[0].id==2&&display[1].id==1,"UI parity: configured worlds descend after the first Coming Soon node");
        check(Resources.Load<TextAsset>("UI/Prefabs/Common/diceWorldGuide")&&Resources.Load<TextAsset>("Spine/Uieff_common_guide/Uieff_common_guide"),
            "UI parity: native dice-world entry overlay and animated guide resources are packaged");
        check(Resources.Load<TextAsset>("UI/Prefabs/video/gameTargetPanel"),"UI parity: intro confirms through the exact original game-target prefab");
        check(Resources.Load<TextAsset>("UI/Prefabs/fakevideo/FakeVideoPanel"),"UI parity: desktop uses the shipped fake-video countdown and close-button layout");
        var host=new GameObject("UI parity checks",typeof(RectTransform));host.SetActive(false);
        try {
            var game=host.AddComponent<GameController>();game.enabled=false;
            typeof(GameController).GetProperty("Model").SetValue(game,model);
            typeof(GameController).GetField("popupLayer",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(game,(RectTransform)host.transform);
            game.ShowRewardBox(receipt);
            var view=(UiView)typeof(GameController).GetField("rewardBoxView",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
            check(view.Ref("RewardBoxUI","doubleButton").activeSelf,"UI parity: ordinary rewards expose the original ad-double button");
            game.ShowRewardBox(receipt,noDouble:true);
            view=(UiView)typeof(GameController).GetField("rewardBoxView",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
            check(!view.Ref("RewardBoxUI","doubleButton").activeSelf,"UI parity: purchased chests can explicitly suppress doubling");
            double pendingGold=model.State.gold,pendingDiamonds=model.Data.diamonds;
            game.ShowRewardBox(new List<KeyValuePair<int,double>>{new KeyValuePair<int,double>(1,100),new KeyValuePair<int,double>(2,3)},deferGrant:true);
            check(model.State.gold==pendingGold&&model.Data.diamonds==pendingDiamonds,
                "UI parity: displaying an unconfirmed wheel receipt grants neither its gold nor its diamonds early");
            var down=UiPrefab.Instantiate("Common/DownPanel",host.transform);
            check(down.Ref("DownItem","superSix").name=="SuperSix"&&down.Ref<UnityEngine.UI.Button>("DownItem","shangrenBtn"),
                "UI parity: dice HUD binds serialized DownItem fields, including the original capitalized SuperSix node");
        } finally {UnityEngine.Object.DestroyImmediate(host);}
    }
}

using System;
using Bufu.Rebuild;
using Bufu.Rebuild.UI;
using UnityEngine;

public static class DiceParityChecks {
    public static void Verify(Action<bool,string> check) {
        var model=new GameModel();model.Data.world=2;
        var weights=model.DiceFaceWeights();
        check(weights[0]==1&&weights[5]==1&&model.RollDiceFace(0)==1&&model.RollDiceFace(.999999f)==6,"dice: an unupgraded roll includes both range endpoints");
        model.State.skills[101]=20;model.State.skills[102]=20;
        weights=model.DiceFaceWeights();
        check(Math.Abs(weights[0]-.4f)<.00001&&Math.Abs(weights[5]-1.6f)<.00001,"dice: maximum early upgrades lower all three small-face weights and raise the six-face weight");
        int[] counts=new int[6];for(int i=0;i<6000;i++)counts[model.RollDiceFace((i+.5f)/6000)-1]++;
        check(counts[5]>3*counts[0]&&counts[0]==counts[1]&&counts[1]==counts[2]&&counts[3]==counts[4],"dice: the actual deterministic roll path produces the upgraded weighted distribution");
        model.State.skills[105]=2;
        check(Math.Abs(model.DiceValueMultiplier(1,6)-1.3225)<.00001&&model.DiceValueMultiplier(1,5)==1,"dice: six-point raise compounds and leaves non-sixes unchanged");
        model.State.talents[106]=1;
        check(Math.Abs(model.DiceValueMultiplier(4,6)-1.8225)<.00001,"dice: purple talent adds after the six-point multiplier rather than to its base upgrade");
        model.State.skills[106]=1;model.State.mission=9;model.State.diceSixTarget=0;
        check(model.DiceSuperTarget==500&&!model.DiceSuperReady,"dice: Super 666 target derives from current main level and configured 40/100 factors");
        model.State.talents[313]=1;model.RecordDiceLanded(5);check(model.State.diceSixCount==0,"dice: non-six landings cannot charge Super 666");
        model.RecordDiceLanded(6);check(model.State.diceSixCount==2,"dice: six charge talent applies on actual landings");
        model.State.diceSixCount=499;model.RecordDiceLanded(6);
        check(model.State.diceSixCount==500&&model.DiceSuperReady,"dice: progress caps at the activation threshold");
        model.State.talents[308]=1;model.State.talents[309]=1;model.State.talents[312]=1;
        check(model.ActivateDiceSuper(true)&&Math.Abs(model.DiceSuperRemaining-127.5f)<.001&&model.State.diceSixCount==0,"dice: ad activation uses (20+5)*(5+0.1), consumes charge once and starts its timer");
        check(!model.ActivateDiceSuper(false)&&model.RollDiceFace(0)==6&&model.RollDiceFace(0,1)==1,"dice: active Super 666 blocks duplicate activation and guide-forced faces take precedence");
        check(Math.Abs(model.DiceValueMultiplier(4,6)-2.0225)<.00001,"dice: active and purple talent bonuses add after the raised-six multiplier");
        model.RecordDiceLanded(6);check(model.State.diceSixCount==0,"dice: Super 666 rolls do not charge the next activation");
        model.Data.diceUnlocked=true;
        check(model.SwitchWorld(1,1000),"dice: active Super 666 can leave the dice world");
        model.TickDiceTimers(5);
        check(model.SwitchWorld(2,1005)&&model.DiceSuperActive&&Math.Abs(model.DiceSuperRemaining-122.5f)<.001,"dice: Super 666 survives a world round trip and spends the five elapsed game seconds");
        model.TickDiceTimers(122.5f);check(!model.DiceSuperActive&&model.RollDiceFace(0)==1,"dice: expiry restores the actual weighted roll path");

        model.State.skills[107]=1;model.State.talents[306]=2;
        const long now=1000000;const int date=20261020;
        model.RefreshDiceMerchantDay(date);double before=model.Data.diamonds;
        check(model.DiceMerchantReady(now)&&model.ClaimDiceMerchant(6,.5f,now,date)==8&&model.Data.diamonds==before+8,"dice: merchant starts ready and awards the sampled base plus talent");
        check(model.State.diceMerchantToday==6&&model.DiceMerchantRemaining(now)==180&&!model.DiceMerchantReady(now+179999),"dice: daily cap counts base diamonds and the persisted 180-second interval has a strict boundary");
        check(model.ClaimDiceMerchant(1,0,now+1000,date)==0,"dice: repeated merchant clicks cannot claim during cooldown");
        model.TickDiceTimers(0);
        check(model.DiceMerchantRemaining(now+600000)==180,"dice: a paused live merchant does not consume ten minutes of wall time");
        model.Data.world=1;model.TickDiceTimers(90);model.Data.world=2;
        check(model.DiceMerchantRemaining(now+690000)==180,"dice: the merchant's live timer pauses while the coin world is active");
        model.TickDiceTimers(179.5f);
        check(!model.DiceMerchantReady(now+900000)&&Math.Abs(model.DiceMerchantRemaining(now+900000)-.5f)<.001,"dice: active scaled time, rather than clock queries, advances the merchant to its last half second");
        model.TickDiceTimers(.5f);check(model.DiceMerchantReady(now),"dice: the merchant becomes ready at exactly 180 active game seconds");
        var copy=new GameModel(JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(model.Data)));
        check(copy.DiceMerchantReady(now+180000)&&copy.ClaimDiceMerchant(1,0,now+180000,date)==3,"dice: merchant cooldown resumes from the saved timestamp and unlocks at its boundary");
        copy.State.diceMerchantToday=199;copy.State.diceMerchantWaitStart=0;
        check(copy.ClaimDiceMerchant(6,0,now,date)==8&&copy.DiceMerchantAtLimit&&copy.State.diceMerchantToday==205,"dice: the last merchant payout may cross the original daily base cap");
        check(copy.ClaimDiceMerchant(1,0,now+10000000,date)==0,"dice: the daily cap blocks further collections");
        copy.RefreshDiceMerchantDay(date+1);check(copy.State.diceMerchantToday==0&&copy.DiceMerchantReady(now),"dice: next day resets both merchant count and cooldown");
        var offline=new GameModel();offline.State.mission=0;offline.State.averagePerMinute=60;offline.State.lastExit=400;
        check(offline.AccumulateOfflineWorld(1,1000)==600&&offline.State.pendingOffline==600&&offline.State.pendingOfflineSeconds==600,"offline: the first valid mission receives its ten-minute receipt and effective duration");
        check(offline.AccumulateOfflineWorld(1,1600)==0&&offline.State.pendingOffline==600&&offline.State.pendingOfflineSeconds==600,"offline: another entry preserves the unclaimed receipt instead of appending ten more minutes");
        var savedOffline=new GameModel(JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(offline.Data)));
        check(savedOffline.State.pendingOfflineSeconds==600&&savedOffline.AccumulateOfflineWorld(1,2200)==0&&savedOffline.State.pendingOffline==600,"offline: JSON restore preserves the receipt's own duration and does not recalculate it");
        savedOffline.ClaimOffline();
        check(savedOffline.State.pendingOffline==0&&savedOffline.State.pendingOfflineSeconds==0,"offline: claiming consumes both the receipt and its displayed duration");
        offline=new GameModel();offline.State.averagePerMinute=60;offline.State.lastExit=1000;
        check(offline.AccumulateOfflineWorld(1,1119)==0&&offline.State.pendingOffline==0,"offline: 119 seconds remains below the original threshold");
        offline.State.lastExit=1000;
        check(offline.AccumulateOfflineWorld(1,1120)==120&&offline.State.pendingOfflineSeconds==120,"offline: exactly 120 seconds crosses the original threshold");
        offline=new GameModel();offline.State.averagePerMinute=60;offline.State.lastExit=1000;
        check(offline.AccumulateOfflineWorld(1,100000)==21600&&offline.State.pendingOfflineSeconds==21600,"offline: a long absence stores the six-hour effective duration together with its capped income");
        foreach(int world in new[]{1,2}) {
            var extended=new GameModel();extended.Data.world=world;extended.State.averagePerMinute=60;extended.State.lastExit=1000;
            extended.State.talents[world==1?105:109]=1;
            check(extended.AccumulateOfflineWorld(world,100000)==32400&&extended.State.pendingOfflineSeconds==32400,"offline: world "+world+" Golden Age level one stores its actual nine-hour capped duration, not six hours");
        }
        check(Resources.Load<TextAsset>("Spine/Uieff_laohuji/Uieff_laohuji")&&Resources.Load<TextAsset>("Spine/zgygy_666/zgygy_666")&&Resources.Load<Texture2D>("Spine/zgygy_zuanshishangren/zgygy_zuanshishangren"),"dice: original slot-machine and both character animation resources are packaged");
        var host=new GameObject("Dice Animation Parity Check",typeof(RectTransform));
        try {
            var activation=UiPrefab.Instantiate("Ad/AdLaoHuJi",host.transform);
            check(activation.Ref("AdLaoHuJi","jihuoBtn")&&!activation.Ref("AdLaoHuJi","jihuoBtn").activeSelf&&activation.Ref("AdLaoHuJi","laohujiEffect"),"dice: original activation prefab exposes its animation and initially hidden activation button");
            var merchant=UiPrefab.Instantiate("Ad/AdDiamondMerchant",host.transform);
            check(merchant.Ref("AdDiamondMerchant","supper")&&merchant.Ref("AdDiamondMerchant","zuanshi")&&merchant.Ref("AdDiamondMerchant","mask"),"dice: original unlock prefab preserves both cards and its dismissible animation mask");
            foreach(string resource in new[]{"Uieff_laohuji","zgygy_666","zgygy_zuanshishangren"}) {
                var node=new GameObject(resource,typeof(RectTransform));node.transform.SetParent(host.transform,false);
                var graphic=node.AddComponent<SpineGraphic>();
                check(graphic.Load(resource),"dice: original Spine asset parses through the shipped renderer: "+resource);
                graphic.Play(resource=="Uieff_laohuji"?"effect":"idle_front",false);
                check(graphic.AnimationDuration>0,"dice: the original animation has a playable nonzero timeline: "+resource);
            }
        } finally {UnityEngine.Object.DestroyImmediate(host);}
    }
}

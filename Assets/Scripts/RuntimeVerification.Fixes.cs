using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Bufu.Rebuild {
    public sealed partial class RuntimeVerification {
        static FieldInfo ActorField(string name)=>typeof(CoinActor).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic);
        static void ActorCall(CoinActor actor,string name)=>typeof(CoinActor).GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(actor,null);
        IEnumerator VerifyFixedSettlementPaths() {
            var model=game.Model;model.State.skills[103]=model.State.skills[104]=0;
            var dice=game.Coins.First();var crit=model.common.First(x=>x.id==16);double originalCrit=crit.factor;
            crit.factor=0;
            Check(dice.Launch(game.RandomLanding(dice.Rect.anchoredPosition),true),"fixes: start a die before changing its base upgrade and critical chance");
            double snapshot=(double)ActorField("dicePayoutSnapshot").GetValue(dice),before=model.State.gold;
            model.State.boosts[101]++;crit.factor=1;
            double expected=model.SettlementValue(dice.Quality,dice.CurrentFace,true,true,0,1,snapshot);
            yield return AlignmentWait(()=>!dice.Busy,3,"fixes: changed-upgrade die reaches its real landing");
            Check(Math.Abs(model.State.gold-before-expected)<Math.Max(.00001,expected*.000001),"fixes: dice freeze rolled base value and use landing-time critical chance");
            Check(dice.Launch(game.RandomLanding(dice.Rect.anchoredPosition),true,1),"fixes: launch helper-tagged die with a manual source");
            yield return AlignmentWait(()=>!dice.Busy,3,"fixes: source-tagged die lands");
            ActorCall(dice,"Echo");
            Check(dice.Busy&&(bool)ActorField("manual").GetValue(dice)&&(int)ActorField("throwRole").GetValue(dice)==1,"fixes: dice echo retains the original manual source and helper role");
            yield return AlignmentWait(()=>!dice.Busy,3,"fixes: source-preserving echo lands");

            model.State.boosts[106]=1;
            var pair=game.Coins.Take(2).ToArray();var p=FreeScenePoint();
            foreach(var other in game.Coins)other.Rect.anchoredPosition=p+Vector2.up*400;
            pair[0].Rect.anchoredPosition=p;pair[1].Rect.anchoredPosition=p+Vector2.right*30;
            var end=SceneScreenPoint(p+Vector2.right*5);game.BeginScenePointer(end-Vector2.right*80);game.MoveScenePointer(end);game.EndScenePointer(end);
            Check(pair[0].Busy&&!pair[1].Busy,"fixes: a dice swipe throws only the nearest die in its radius");
            yield return AlignmentWait(()=>pair.All(c=>!c.Busy),3,"fixes: dice swipe settlement completes");

            Check(model.SwitchWorld(1),"fixes: return to the coin world for explosion settlement");
            model.State.quantity[3]=1;model.State.boosts[10]=1;model.State.boosts[12]=100;
            game.RebuildActors();FreezeHelpers();Call("SelectTab",1);ClearAlignmentGoals();yield return null;
            crit.factor=0;var silver=game.Coins.First(c=>c.Quality==2);var copper=game.Coins.First(c=>c.Quality==1);
            foreach(var other in game.Coins)other.Rect.anchoredPosition=Vector2.up*400;
            silver.Rect.anchoredPosition=Vector2.zero;copper.Rect.anchoredPosition=Vector2.right*20;
            before=model.State.gold;expected=model.SettlementValue(2,1,false,false);
            Check(silver.Launch(Vector2.zero,false,0,0,6,1,false,true),"fixes: begin an explosion throw with a six-fold floating-text multiplier");
            ActorField("guidedCoinFace").SetValue(silver,1);ActorCall(silver,"LandCoin");
            Check(Math.Abs(model.State.gold-before-expected)<Math.Max(.00001,expected*.000001),"fixes: gambler display multiplier does not multiply the actual coin wallet payout");
            Check(!copper.Busy&&!(bool)ActorField("luckyEcho").GetValue(silver)&&(float)ActorField("echoIn").GetValue(silver)==0,"fixes: suppressed explosion landing triggers neither a neighbouring coin nor a lucky echo");
            model.State.boosts[12]=0;
            var frontTalent=model.coinTalents.First(x=>x.id==108);double frontResult=frontTalent.upgradeResult[0];int frontLevel=model.State.talents[108];
            model.State.talents[108]=0;crit.factor=0;
            Check(copper.Launch(copper.Rect.anchoredPosition,false,0,0,6),"fixes: start a normal coin before its landing buffs change");
            ActorField("guidedCoinFace").SetValue(copper,-1);
            frontTalent.upgradeResult[0]=1;model.State.talents[108]=1;crit.factor=1;
            before=model.State.gold;expected=model.SettlementValue(1,1,false,true);
            ActorCall(copper,"LandCoin");
            Check(copper.CurrentFace==1&&Math.Abs(model.State.gold-before-expected)<Math.Max(.00001,expected*.000001),"fixes: coin front and critical buffs are sampled at landing while display-only multipliers stay out of the wallet");
            frontTalent.upgradeResult[0]=frontResult;model.State.talents[108]=frontLevel;crit.factor=0;
            Check(copper.Launch(copper.Rect.anchoredPosition,false,9),"fixes: start Haaland's coin-world callback path");
            HelperSkills.RepeatHaalandThrow(game,copper);ActorField("guidedCoinFace").SetValue(copper,1);ActorCall(copper,"LandCoin");
            Check(!copper.Busy&&(int)ActorField("throwRole").GetValue(copper)==0,"fixes: reference CoinVat idle reset cancels a callback-time rethrow without adding another payout");
            crit.factor=originalCrit;
            yield return Capture("12-fixed-settlement-paths.png");
        }
    }
}

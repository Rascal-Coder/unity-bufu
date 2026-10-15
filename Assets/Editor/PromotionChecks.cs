using System;
using System.Linq;
using UnityEngine;
using Bufu.Rebuild;

public static class PromotionChecks {
    public static void Verify(Action<bool,string> check) {
        var g=new GameModel();
        check(g.Promotions.Length==40&&g.Promotions[0].limitMissionCondition==3&&g.Promotions[39].limitMissionCondition==81,"promotion: all forty original tblimitmission rows");
        check(!g.PromotionReady&&g.StartPromotion(0)==null,"promotion: entry requires original growth condition");
        g.AddEarnedGold(1e8,0);
        check(g.MainLevel==3&&g.PromotionReady,"promotion: growth stops exactly at first pending promotion");
        var run=g.StartPromotion(0);
        check(run!=null&&Math.Abs(run.Target-(double)1.8f*1e8)<1&&run.Remaining==60,"promotion: target uses float config times rolling income; duration sixty seconds");
        check(g.StartPromotion(0)==null,"promotion: cannot initialize duplicate active HUD");
        g.GrantAmount(1,run.Target*10);check(run.Collected==0,"promotion: internal gold rewards do not count");
        g.AddEarnedGold(run.Target/2,1);double collected=run.Collected;
        check(collected>0&&run.Status==PromotionStatus.Running&&g.MainLevel==3,"promotion: eligible income counts while growth remains blocked");
        g.CancelPromotion();check(run.Status==PromotionStatus.Cancelled&&g.PromotionReady,"promotion: exit costs nothing and retains entry/growth gate");
        run=g.StartPromotion(1);check(run.Collected==0&&run.Remaining==60,"promotion: retry resets collection and duration");
        run.Remaining=.1f;g.TickPromotion(.2f);check(run.Status==PromotionStatus.Running,"promotion: original timeout checks before decrement");
        g.TickPromotion(.01f);check(run.Status==PromotionStatus.Lost&&g.State.promotionId==1,"promotion: next frame timeout preserves stage");
        run=g.StartPromotion(2);double gold=g.State.gold;g.AddEarnedGold(run.Target,2);
        check(run.Status==PromotionStatus.Won&&run.Collected==run.Target&&g.PromotionWonPending,"promotion: exact target completes and clamps collection");
        check(g.State.gold==gold+run.Target&&run.Rewards[0].Value==g.State.averagePerMinute*3,"promotion: success-time average sets prize; no early reward");
        check(g.State.promotionId==5&&g.MainLevel==3,"promotion: original mission scan skips stages using current wallet without advancing growth");
        double prize=run.Rewards[0].Value;gold=g.State.gold;
        check(g.ClaimPromotion()&&g.State.gold==gold+prize&&!g.PromotionWonPending,"promotion: confirmation grants prepared reward");
        check(!g.ClaimPromotion()&&g.State.gold==gold+prize,"promotion: duplicate confirmation grants nothing");
        var restored=new GameModel(JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(g.Data)));
        check(restored.State.promotionId==5&&restored.Promotion==null,"promotion: per-world ID persists; transient round does not resume");
        var talent=new GameModel();talent.State.mission=2;talent.State.talents[307]=1;
        run=talent.StartPromotion(0);
        check(Math.Abs(run.Remaining-60*(1+(float)talent.TalentValue(307)))<.001f&&Math.Abs(run.Target-1.8)<.000001,"promotion: extra duration does not multiply target");
        talent.Data.diceUnlocked=true;check(talent.SwitchWorld(2)&&run.Status==PromotionStatus.Cancelled&&talent.State.promotionId==1,"promotion: switching worlds cancels live round and uses independent ID");
        talent.State.mission=2;run=talent.StartPromotion(0);
        check(run.Config==talent.Promotions[0]&&run.Config.limitGoalFactor==1.8f,"promotion: dice world uses original shared table, not unused tblimitmission2");
        talent.CancelPromotion();talent.State.promotionId=40;talent.State.mission=80;run=talent.StartPromotion(0);talent.AddEarnedGold(run.Target,0);
        check(talent.State.promotionId==-1&&run.Rewards[0].Key==2&&run.Rewards[0].Value==15,"promotion: last stage completes to minus one and original diamond prize");
        check(talent.ClaimPromotion()&&!talent.PromotionReady,"promotion: last victory confirmation removes entry");
        var legacy=new SaveData();legacy.worlds[0].promotionId=0;
        check(new GameModel(legacy).State.promotionId==1,"promotion: legacy saves initialize stage one");
        var saved=new SaveData();saved.worlds[0].averagePerMinute=100;saved.worlds[0].mission=2;
        var loaded=new GameModel(saved);run=loaded.StartPromotion(Time.unscaledTime);
        check(Math.Abs(run.Target-90)<.00001,"promotion: restored sample queue uses saved average times one half before original factor");
        var crossed=new GameModel();crossed.State.mission=2;run=crossed.StartPromotion(Time.unscaledTime);
        crossed.Settle(1,1,1,true,false,Time.unscaledTime);
        check(run.Status==PromotionStatus.Won&&crossed.PromotionWonPending,"promotion: actual coin settlement completes the live promotion");
        crossed.Data.diceUnlocked=true;double firstWorldGold=crossed.State.gold;crossed.SwitchWorld(2);
        check(crossed.Promotion==run&&crossed.PromotionWonPending&&!crossed.PromotionReady,"promotion: world change retains prepared result without locking the new world");
        double crossPrize=run.Rewards[0].Value;
        check(crossed.ClaimPromotion()&&crossed.State.gold==crossPrize&&crossed.Data.worlds[0].gold==firstWorldGold,"promotion: original result pays the world active at confirmation");
    }
}

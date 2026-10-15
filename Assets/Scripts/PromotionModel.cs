using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bufu.Rebuild {
    [Serializable] public sealed class PromotionConfig {
        public int id,limitTIme,rewardType;
        public double limitMissionCondition,rewardNum;
        public float limitGoalFactor;
    }
    public enum PromotionStatus { Running,Won,Lost,Cancelled }
    public sealed class PromotionRun {
        public PromotionConfig Config;public int World;
        public double Target,Collected;public float Remaining;
        public PromotionStatus Status;public bool Claimed;
        public readonly List<KeyValuePair<int,double>> Rewards=new List<KeyValuePair<int,double>>();
    }
    public sealed partial class GameModel {
        PromotionConfig[] promotionConfigs;
        public PromotionConfig[] Promotions=>promotionConfigs??(promotionConfigs=Read<PromotionConfig>("tblimitmission"));
        public PromotionConfig CurrentPromotion=>Promotions.FirstOrDefault(x=>x.id==State.promotionId);
        public PromotionRun Promotion {get;private set;}
        public bool PromotionRunning=>Promotion!=null&&Promotion.World==Data.world&&Promotion.Status==PromotionStatus.Running;
        public bool PromotionWonPending=>Promotion!=null&&Promotion.Status==PromotionStatus.Won&&!Promotion.Claimed;
        // DownItem.challengeIsOpen gates growth even before entering, and until victory confirmation.
        public bool PromotionReady=>promotionResultGate||promotionClaimGate||CurrentPromotion!=null&&MainLevel>=CurrentPromotion.limitMissionCondition;
        bool promotionClaimGate,promotionResultGate;
        public PromotionRun StartPromotion(double now) {
            if(PromotionRunning||PromotionWonPending||!PromotionReady||CurrentPromotion==null)return null;
            UpdateRate(now);var c=CurrentPromotion;
            Promotion=new PromotionRun{Config=c,World=Data.world,Target=(double)(c.limitGoalFactor*((float)c.limitTIme/60f))*State.averagePerMinute,
                Remaining=c.limitTIme*(1+(float)TalentValue(Data.world==1?307:112)),Status=PromotionStatus.Running};
            Changed?.Invoke();return Promotion;
        }
        public void TickPromotion(float delta) {
            if(!PromotionRunning)return;
            if(Promotion.Remaining>0)Promotion.Remaining-=delta;
            else {Promotion.Status=PromotionStatus.Lost;Changed?.Invoke();}
        }
        public void CancelPromotion() {
            if(PromotionRunning)Promotion.Status=PromotionStatus.Cancelled;
            Changed?.Invoke();
        }
        void ObservePromotionIncome(double amount) {
            if(!PromotionRunning)return;
            var run=Promotion;run.Collected+=amount;
            if(run.Collected<run.Target)return;
            run.Collected=run.Target;run.Status=PromotionStatus.Won;promotionResultGate=true;
            double reward=run.Config.rewardNum*(run.Config.rewardType==1?State.averagePerMinute:1);
            if(UnityEngine.Random.value<=(float)TalentValue(Data.world==1?306:113))reward*=2;
            if(run.Config.rewardType>=3&&run.Config.rewardType<=5)run.Rewards.AddRange(ResolveFragments(run.Config.rewardType,(int)Math.Ceiling(reward)));
            else run.Rewards.Add(new KeyValuePair<int,double>(run.Config.rewardType,reward));
            int index=Array.IndexOf(Promotions,run.Config);
            if(index==Promotions.Length-1)State.promotionId=-1;
            else {
                int level=MainLevel;var same=Promotions.FirstOrDefault(x=>x.limitMissionCondition==level);if(same!=null)level=same.id;
                // The original searches the complete main-mission table, including world 2 IDs.
                var mission=missions.FirstOrDefault(x=>x.mainMissionNum>State.gold)??missions.Last();
                int calculated=(Promotions.FirstOrDefault(x=>x.limitMissionCondition>mission.id)??Promotions.Last()).id;
                State.promotionId=Math.Min(Promotions.Last().id,Math.Max(Promotions[index+1].id,Math.Max(calculated,level)));
            }
        }
        public bool ClaimPromotion(bool ad=false) {
            if(!PromotionWonPending)return false;
            var run=Promotion;bool gate=PromotionReady;run.Claimed=true;promotionResultGate=false;promotionClaimGate=gate;
            foreach(var reward in run.Rewards) {
                if(reward.Key==1)AddEarnedGold(reward.Value,Time.unscaledTime);
                else GrantAmount(reward.Key,reward.Value);
            }
            // Native ad callback omits CheckChallenge; normal confirmation refreshes its growth gate.
            if(!ad)promotionClaimGate=false;
            Changed?.Invoke();return true;
        }
        public void AddEarnedGold(double amount,double now) {
            if(!Finite(amount)||amount<=0)return;
            State.gold+=amount;State.lifetime+=amount;income.Enqueue(new KeyValuePair<double,double>(now,amount));
            UpdateRate(now);ObservePromotionIncome(amount);CheckGrowth();Changed?.Invoke();
        }
        void InitializeIncome(double now) {
            income.Clear();double saved=State.averagePerMinute;
            if(saved>0)for(int i=0;i<5;i++)income.Enqueue(new KeyValuePair<double,double>(now-6*i,saved*.5/5));
        }
    }
}

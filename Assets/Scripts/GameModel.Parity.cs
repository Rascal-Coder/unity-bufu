using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bufu.Rebuild {
    public sealed partial class GameModel {
        // TalentDataManager.GetNSRHelperCount: basic, N, R, SR are distinct groups.
        public int HelperRarityCount(int rarity) {
            if(rarity<=0)return Math.Max(0,Helpers-AssignedHelpers);
            int count=0;
            foreach(var h in State.helpers) {
                int group=h.id<=2?1:h.id<=4?2:3;
                if(group==rarity)count+=Math.Max(0,h.count);
            }
            return count;
        }

        public int GlobalHelperLevel(int id)=>id>0&&id<Data.globalHelperLevels.Length?Math.Max(1,Data.globalHelperLevels[id]):1;
        public int HelperCapacity(int id) {
            var helper=HelperOf(id);
            return helper==null?0:helper.max+(Data.world==2&&State.talents[212]>0?1:0);
        }

        // BuffManager.GetFinalValue(Income): multiply active buff factors, then add talent bonuses.
        // InitializeCommon deliberately installs the original 0.0001f income buff.
        public double IncomeMultiplier() {
            double product=(1d+(double).0001f)*SupportIncomeMultiplier;
            foreach(var desk in desks)
                if(desk.WorldId==Data.world&&DeskLevel(desk)>0&&DeskKind(desk,6))product*=1d+DeskPower(desk);
            double talent=TalentValue(101);
            int kinds=State.helpers.Count(h=>h.count>0);
            if(Data.world==1) {
                talent+=TalentValue(202)*HelperCount(1)+TalentValue(203)*HelperCount(2)
                    +TalentValue(204)*HelperCount(3)+TalentValue(205)*kinds;
            } else {
                talent+=TalentValue(203)*HelperCount(1)+TalentValue(205)*HelperCount(2)
                    +TalentValue(209)*HelperCount(3)+TalentValue(210)*kinds
                    +TalentValue(206)*HelperRarityCount(2)+TalentValue(208)*HelperRarityCount(0);
            }
            return product+talent;
        }

        public void MarkActiveWorldExit(long utc=0) {
            State.lastExit=utc>0?utc:DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public double AccumulateOfflineWorld(int world,long utc) {
            if(world<1||world>Data.worlds.Length)return 0;
            var state=Data.worlds[world-1];long elapsed=utc-state.lastExit;double reward=0;
            // OfflineIncomeManager restores a single receipt before attempting another calculation.
            if(state.pendingOffline>0)return 0;
            state.pendingOfflineSeconds=0;
            if(state.lastExit>0&&elapsed>=120&&state.mission>=0) {
                double cap=21600*(1+TalentValueForWorld(world,world==1?105:109));
                double multiplier=1+TalentValueForWorld(world,world==1?104:108);
                double seconds=Math.Min(elapsed,cap);
                reward=seconds/60d*state.averagePerMinute*multiplier;
                if(Finite(reward)&&reward>0){state.pendingOffline=reward;state.pendingOfflineSeconds=seconds;}else reward=0;
            }
            state.lastExit=utc;
            return reward;
        }

        public void ResumeFromBackground(long utc) {
            AccumulateOfflineWorld(Data.world,utc);InitializeIncome(Time.unscaledTime);Changed?.Invoke();
        }

        // The wheel resolves its receipt after animation; keep its originating world's income basis.
        public List<KeyValuePair<int,double>> BuildShopRewards(ShopItem item,int world) {
            if(world<1||world>Data.worlds.Length)throw new ArgumentOutOfRangeException(nameof(world));
            var rewards=new List<KeyValuePair<int,double>>();
            for(int i=0;i<item.rewardType.Length&&i<item.count.Length;i++) {
                int type=item.rewardType[i];
                if(type>=3&&type<=5)rewards.AddRange(ResolveFragments(type,item.count[i]));
                else rewards.Add(new KeyValuePair<int,double>(type,type==1?Data.worlds[world-1].averagePerMinute*item.count[i]:item.count[i]));
            }
            return rewards;
        }

        // RewardBoxUI doubles the already resolved receipt; it must never roll shard rarity a second time.
        public void GrantRewardReceipt(int world,IEnumerable<KeyValuePair<int,double>> rewards) {
            if(world<1||world>Data.worlds.Length||rewards==null)return;
            var state=Data.worlds[world-1];
            foreach(var reward in rewards) {
                if(!Finite(reward.Value)||reward.Value<=0)continue;
                if(reward.Key==1)state.gold+=reward.Value;
                else if(reward.Key==6)state.points+=(int)Math.Round(reward.Value);
                else GrantAmount(reward.Key,reward.Value);
            }
            if(world==Data.world)CheckGrowth();
            Changed?.Invoke();
        }
    }
}

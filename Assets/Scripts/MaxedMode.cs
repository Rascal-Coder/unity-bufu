using System;
using System.Linq;

namespace Bufu.Rebuild {
    public static class MaxedMode {
        public const string SceneName="Maxed";
    }

    public sealed partial class GameModel {
        // Full unlock for the Maxed scene. Level caps stay on the configured tables so payouts remain finite.
        void PrepareMaxedSave() {
            const double pile=1e30;
            Data.diceUnlocked=Data.worldEntry=true;
            Data.diamonds=pile;
            Data.spinProgress=SpinProgressMax;Data.spinReadyAt=Data.diamondReadyAt=0;
            Data.spinCount=Data.chestNormal=Data.chestRare=Data.diamondClaims=0;
            Array.Clear(Data.shards,0,Data.shards.Length);
            foreach(var role in helperRoles) {
                int index=100+role.id;
                if(index>=0&&index<Data.shards.Length)Data.shards[index]=999999;
            }
            for(int w=1;w<=2;w++) {
                Data.world=w;
                var s=Data.worlds[w-1];
                s.mission=missions.Count(x=>x.worldId==w);s.pendingGoals.Clear();
                s.pendingOffline=0;s.lastExit=0;s.gold=pile;s.lifetime=pile;s.points=999999999;s.averagePerMinute=1;
                s.checkDay=7;s.checkDate=TodayNumber();s.checkMask=s.checkAdMask=0;s.checkClaimed=false;
                if(s.supportLastTime==null||s.supportLastTime.Length!=3)s.supportLastTime=new long[3];
                else Array.Clear(s.supportLastTime,0,3);
                s.promotionId=1;
                Array.Clear(s.talents,0,s.talents.Length);
                foreach(var t in w==1?coinTalents:diceTalents)
                    if(t.upgradeLevelMax>0&&t.id>=0&&t.id<s.talents.Length)s.talents[t.id]=t.upgradeLevelMax;
                Array.Clear(s.quantity,0,128);Array.Clear(s.boosts,0,128);Array.Clear(s.skills,0,128);
                Array.Clear(s.evolution,0,128);Array.Clear(s.evolvedAt,0,128);Array.Clear(s.deskLevel,0,128);
                foreach(var c in quantities.Where(x=>x.worldId==w)) {
                    if(c.id<0||c.id>=128)continue;
                    if(c.upgradeLvMax<=0) {
                        int tiers=c.evolutionLevel==null?0:c.evolutionLevel.Count(x=>x>0);
                        int level=tiers>0?c.evolutionLevel[tiers-1]:1;
                        s.quantity[c.id]=level;s.evolution[c.id]=Math.Min(4,tiers);s.evolvedAt[c.id]=level;
                    } else s.quantity[c.id]=MaxLevel(c);
                }
                foreach(var c in boosts.Where(x=>x.worldId==w))if(c.id>=0&&c.id<128)s.boosts[c.id]=Math.Max(1,MaxLevel(c));
                foreach(var c in skills.Where(x=>x.worldId==w))if(c.id>=0&&c.id<128)s.skills[c.id]=Math.Max(1,MaxLevel(c));
                foreach(var d in desks.Where(x=>x.WorldId==w))if(d.id>=0&&d.id<128)s.deskLevel[d.id]=Math.Max(1,d.skillLevelMax);
                s.helpers.Clear();
                foreach(var role in helperRoles)
                    s.helpers.Add(new HelperState{id=role.id,level=Math.Max(1,role.upgradeLevelMax),count=1,max=10});
                var table=w==1?coinPoints:dicePoints;
                s.rebornLevel=table.Length==0?0:table.Length-1;s.rebirth=s.rebornLevel+1;
                s.helperAdCap=MaxLevel(Config(w==1?1:101,false));s.usingDesk=0;
            }
            Data.world=1;income.Clear();
        }
    }
}

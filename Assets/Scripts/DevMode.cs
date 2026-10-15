using System;
using System.Linq;

namespace Bufu.Rebuild {
    public static class DevMode {
        public const string EditorPreference="Bufu.Rebuild.DevMode";
        public static bool Enabled {
            get {
                if(Environment.GetCommandLineArgs().Contains("--dev-mode"))return true;
#if UNITY_EDITOR
                return UnityEditor.EditorPrefs.GetBool(EditorPreference,false);
#else
                return false;
#endif
            }
        }
    }

    public sealed partial class GameModel {
        // Unlock and seed modest actor counts; never max exponential upgrade tables.
        void PrepareDeveloperSave() {
            Data.diceUnlocked=Data.worldEntry=true;
            for(int w=1;w<=2;w++) {
                var s=Data.worlds[w-1];int baseId=w==1?0:100;
                s.mission=missions.Count(x=>x.worldId==w);s.pendingGoals.Clear();
                s.pendingOffline=0;s.lastExit=0;
                foreach(var c in quantities.Where(x=>x.worldId==w))s.quantity[c.id]=Math.Max(s.quantity[c.id],c.id==baseId+1?helperRoles.Length:1);
                foreach(var c in boosts.Where(x=>x.worldId==w))s.boosts[c.id]=Math.Max(1,s.boosts[c.id]);
                foreach(var c in skills.Where(x=>x.worldId==w))s.skills[c.id]=Math.Max(1,s.skills[c.id]);
                foreach(var t in w==1?coinTalents:diceTalents)s.talents[t.id]=Math.Max(1,s.talents[t.id]);
                foreach(var role in helperRoles) {
                    var helper=s.helpers.FirstOrDefault(x=>x.id==role.id);
                    if(helper==null){helper=new HelperState{id=role.id,count=1};s.helpers.Add(helper);}
                    helper.level=role.upgradeLevelMax;helper.max=Math.Max(10,helper.max);helper.count=1;
                }
            }
            RefillDeveloperResources();
        }

        public bool RefillDeveloperResources() {
            if(!DeveloperMode)return false;
            Data.diamonds=Math.Max(Data.diamonds,1000000);
            foreach(var s in Data.worlds) {
                s.gold=Math.Max(s.gold,1e30);s.points=Math.Max(s.points,1000000);
                s.checkDay=7;s.checkDate=TodayNumber();s.checkMask=0;s.checkClaimed=false;
            }
            foreach(var role in helperRoles)Data.shards[100+role.id]=Math.Max(Data.shards[100+role.id],100000);
            Data.spinProgress=SpinProgressMax;Data.spinReadyAt=Data.diamondReadyAt=0;
            Changed?.Invoke();return true;
        }
    }
}

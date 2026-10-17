using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bufu.Rebuild {
    [Serializable] public sealed class Table<T> { public T[] rows; }
    [Serializable] public sealed class Common { public int id; public string itemDesc; public double factor; }
    [Serializable] public sealed class Upgrade {
        public int id, worldId, costType, upgradeLvMax, unLockCondition, conditionIconType;
        public string itemName, itemDesc, itemDescUI;
        public double[] baseCostMoney, costMoneyBuff, evolutionBuff;
        public int[] costMoneyLevel, evolutionLevel;
        public double upgradeResult;
        public int position;
    }
    [Serializable] public sealed class Mission { public int id,worldId,rewardType,picId; public string titleName; public double mainMissionNum,rewardNum; }
    [Serializable] public sealed class Talent { public int id,talentType,position,baseTalentPoint,costPointAdd,upgradeLevelMax; public string talentName,talentDesc; public double[] upgradeResult; }
    [Serializable] public sealed class World { public int id,sort,preWorldId,requiredPreWorldLevel,entryUnlockLevel,gameplayType; public string worldName; }
    [Serializable] public sealed class PointReward { public int id; public double talentName,rebornBuff; public string talentDesc; }
    [Serializable] public sealed class Desk { public int id,WorldId,skillLevelMax,buyType,deskUnlockCondition; public string deskName,deskDesc; public double skillNum,skillUpgradeBuff,buyNum,buyNumCostBuff; }
    /// <summary>tbhelperrole. Field names match the exported table so JsonUtility can read it.</summary>
    [Serializable] public sealed class HelperRole {
        public int id,Rare,upgradeLevelMax,UpgradeType,buyType,buyNumLevel,UpgradeNumLevel,helperUnlockCondition,helperAdUnlockCondition,UnlockLevel,sorting;
        public string helperName,helperDesc,awakeSkill1,awakeSkill2,awakeSkill3,awakeSkill1Name,awakeSkill2Name,awakeSkill3Name;
        public float skillFactor1,cdTime,baseSkillNum,upgradeSkillNum,helperIncomeAddBuff,UpgradeNumCostBuff;
        public int[] upgradeLevel,awakeSkillNum,UpgradeCostType; public float[] buyNumCostBuff;
        public double UpgradeNum,buyNum,buyNumDiamond;
    }
    /// <summary>HelperInfo of one world: level, how many are on the field, and the hire cap.</summary>
    [Serializable] public sealed class HelperState { public int id,level=1,count,max=1,ad; }
    [Serializable] public sealed class SignReward { public int id,worldId; public int[] normalReward,AdReward; }
    [Serializable] public sealed class ShopItem { public int id,todayLimit; public string name,desc; public int[] rewardType,count; public double rate,cost; }
    [Serializable] public sealed class WorldState {
        public double gold, lifetime, averagePerMinute=1, pendingOffline, rebirthMultiplier=1;
        public long[] supportLastTime=new long[3]; public int promotionId=1; public int mission,rebirth,points,checkDay=1,checkDate,checkMask,checkAdMask,helperAdCap,usingDesk,rebornLevel=-1; public bool checkClaimed; public long lastExit;
        public int[] quantity=new int[128], boosts=new int[128], evolution=new int[128], evolvedAt=new int[128], talents=new int[400], skills=new int[128], deskLevel=new int[128];
        public List<int> pendingGoals=new List<int>(); public List<HelperState> helpers=new List<HelperState>();
    }
    [Serializable] public sealed class SaveData { public int schema=1,world=1,shopDate,chestNormal,chestRare,spinCount,spinProgress,diamondClaims; public long spinReadyAt,diamondReadyAt; public bool diceUnlocked,worldEntry; public double diamonds=20; public int[] shards=new int[128]; public WorldState[] worlds={new WorldState(),new WorldState()}; }
    public sealed partial class GameModel {
        public bool DeveloperMode {get;private set;}
        public bool Maxed {get;private set;}
        public bool OpenAccess=>DeveloperMode||Maxed;
        public readonly Common[] common; public readonly Upgrade[] quantities, boosts, skills;
        public readonly Mission[] missions; public readonly Talent[] coinTalents,diceTalents; public readonly PointReward[] coinPoints,dicePoints;
        public readonly Desk[] desks; public readonly SignReward[] signs; public readonly ShopItem[] shop; public readonly World[] worlds; public readonly HelperRole[] helperRoles;
        public SaveData Data {get;private set;} public WorldState State => Data.worlds[Data.world-1];
        public event Action Changed; public event Action<string> Notice;
        readonly Queue<KeyValuePair<double,double>> income=new Queue<KeyValuePair<double,double>>();
        public GameModel(SaveData data=null,bool developerMode=false,bool maxed=false) {
            DeveloperMode=developerMode;Maxed=maxed;
            common=Read<Common>("tbcommon"); quantities=Read<Upgrade>("tbitemupgrade1"); boosts=Read<Upgrade>("tbitemupgrade2"); skills=Read<Upgrade>("tbitemupgrade3"); missions=Read<Mission>("tbmainmission");
            coinTalents=Read<Talent>("tbtalent"); diceTalents=Read<Talent>("tbtalent2"); coinPoints=Read<PointReward>("tbtalentpoint");dicePoints=Read<PointReward>("tbtalentpoint2");
            desks=Read<Desk>("tbdesk"); signs=Read<SignReward>("tbsignreward"); shop=Read<ShopItem>("tbshop"); worlds=Read<World>("tbworld"); helperRoles=Read<HelperRole>("tbhelperrole");
            Data=data??new SaveData(); Validate();InitializeIncome(Time.unscaledTime);
            if(Maxed)PrepareMaxedSave();
            else if(DeveloperMode)PrepareDeveloperSave();
        }
        static T[] Read<T>(string name) {var asset=Resources.Load<TextAsset>("Config/"+name); if(!asset) throw new InvalidOperationException("Missing config "+name); return JsonUtility.FromJson<Table<T>>(asset.text).rows;}
        public void Validate() {
            if(Data.schema!=1 || Data.worlds==null || Data.worlds.Length!=2) throw new InvalidOperationException("不支持的存档格式");
            if(Data.world<1||Data.world>2) Data.world=1;
            if(!Finite(Data.diamonds)||Data.diamonds<0)Data.diamonds=20;
            if(Data.shards==null||Data.shards.Length!=128)Data.shards=new int[128];
            // Earlier saves kept rarity IDs here, but helper upgrades consume concrete helper IDs.
            for(int rarity=3;rarity<=5;rarity++)if(Data.shards[rarity]>0) {
                int count=Data.shards[rarity];Data.shards[rarity]=0;GrantItem(rarity,count);
            }
            foreach(var s in Data.worlds) {
                if(s==null || s.quantity==null || s.quantity.Length!=128 || s.boosts==null || s.boosts.Length!=128 || s.talents==null || s.talents.Length!=400 || s.evolution==null || s.evolution.Length!=128 || s.evolvedAt==null || s.evolvedAt.Length!=128) throw new InvalidOperationException("存档数据损坏");
                if(s.supportLastTime==null||s.supportLastTime.Length!=3)Array.Resize(ref s.supportLastTime,3);
                if(s.promotionId==0||s.promotionId < -1||s.promotionId>40)s.promotionId=1;
                if(s.skills==null || s.skills.Length!=128)s.skills=new int[128];
                if(s.deskLevel==null || s.deskLevel.Length!=128)s.deskLevel=new int[128];
                if(s.checkDay<1||s.checkDay>7)s.checkDay=1;
                if(s.pendingGoals==null)s.pendingGoals=new List<int>();
                if(s.helpers==null)s.helpers=new List<HelperState>();
                if(s.rebornLevel<0&&s.rebirth>0)s.rebornLevel=s.rebirth-1;
                if(!Finite(s.gold)||s.gold<0)s.gold=0; if(!Finite(s.averagePerMinute)||s.averagePerMinute<1)s.averagePerMinute=1;
                if(!Finite(s.pendingOffline)||s.pendingOffline<0)s.pendingOffline=0;
                for(int i=0;i<128;i++){s.quantity[i]=Math.Max(0,s.quantity[i]);s.boosts[i]=Math.Max(0,s.boosts[i]);s.evolution[i]=Math.Max(0,Math.Min(4,s.evolution[i]));}
            }
            RefreshWorldUnlocks();
            for(int w=1;w<=2;w++)RefreshHelperUnlocks(w);
        }
        World WorldConfig(int id)=>worlds.FirstOrDefault(x=>x.id==id);
        /// <summary>GameDataManager.IsWorldUnlocked / IsWorldEntryUnlocked; both unlocks are remembered once reached.</summary>
        void RefreshWorldUnlocks() {
            var dice=WorldConfig(2);int need=dice==null?10:dice.requiredPreWorldLevel;
            if(Data.world==2||Data.worlds[0].mission+1>=need)Data.diceUnlocked=true;
            int entry=(int)Factor(59);
            if(Data.worlds.Any(w=>w.mission+1>entry))Data.worldEntry=true;
        }
        public bool WorldUnlocked(int world)=>world==1||world==2&&Data.diceUnlocked;
        public bool WorldEntryUnlocked{get{RefreshWorldUnlocks();return Data.worldEntry;}}
        public string WorldName(int id)=>WorldConfig(id)?.worldName??"";
        public int RequiredPreWorldLevel(int id)=>WorldConfig(id)?.requiredPreWorldLevel??0;
        public double Factor(int id)=>common.First(x=>x.id==id).factor;
        public static bool Finite(double x)=>!double.IsNaN(x)&&!double.IsInfinity(x);
        public int BaseId=>Data.world==1?0:100;
        /// <summary>The original "main mission level": the 1-based index of the goal currently being worked on.</summary>
        public int MainLevel=>State.mission+1;
        public bool LevelAbove(int factorId)=>OpenAccess||MainLevel>(int)Factor(factorId);
        public Mission[] WorldMissions=>missions.Where(x=>x.worldId==Data.world).OrderBy(x=>x.id).ToArray();
        public Mission TitleMission(int level){var all=WorldMissions;return all.Length==0?null:all[Math.Max(1,Math.Min(level,all.Length))-1];}
        public int KindOf(Upgrade c){if(skills!=null&&Array.IndexOf(skills,c)>=0)return 2;if(Array.IndexOf(boosts,c)>=0)return 1;return 0;}
        public int Level(int id,bool boost)=>LevelKind(id,boost?1:0);
        public int LevelKind(int id,int kind)=>kind==2?State.skills[id]:kind==1?State.boosts[id]:State.quantity[id];
        public Upgrade Config(int id,bool boost)=>ConfigKind(id,boost?1:0);
        public Upgrade ConfigKind(int id,int kind)=>(kind==2?skills:kind==1?boosts:quantities).First(x=>x.id==id);
        public static double Cost(Upgrade c,int level) {
            if(level<0||c.baseCostMoney==null||c.baseCostMoney.Length==0)return 0;
            if(level<c.baseCostMoney.Length)return c.baseCostMoney[level];
            double result=c.baseCostMoney[c.baseCostMoney.Length-1];
            for(int i=c.baseCostMoney.Length;i<=level;i++) {
                int index=Array.FindIndex(c.costMoneyLevel,x=>i<x);
                if(index<0)index=c.costMoneyBuff.Length-1;
                result*=c.costMoneyBuff[Math.Min(index,c.costMoneyBuff.Length-1)];
            }
            return result;
        }
        public double Price(int id,bool boost)=>PriceKind(id,boost?1:0);
        public double PriceKind(int id,int kind) {
            var c=ConfigKind(id,kind); double cost=Cost(c,LevelKind(id,kind));
            if(kind==0&&(id==1||id==101)){int labor=Data.world==1?206:213;if(State.talents[labor]>0)cost*=TalentValue(labor);}
            if(c.costType==1&&Data.world==2&&State.talents[105]>0)cost*=TalentValue(105);
            return cost;
        }
        public int Count(int quality) {
            int id=BaseId+quality+1,level=State.quantity[id],initial=quality==1?(int)Factor(Data.world==1?3:77):0;
            int tier=State.evolution[id];
            if(tier<=0)return level+initial;
            var c=Config(id,false); int threshold=c.evolutionLevel[tier-1];
            return Math.Max(1,State.evolvedAt[id]-1-threshold)+Math.Max(0,level-State.evolvedAt[id]);
        }
        public int Helpers=>State.quantity[BaseId+1];
        public double TalentValue(int id) {
            int lv=State.talents[id]; if(lv<=0)return 0;
            var t=(Data.world==1?coinTalents:diceTalents).FirstOrDefault(x=>x.id==id);
            return t==null?0:t.upgradeResult[Math.Min(lv-1,t.upgradeResult.Length-1)];
        }
        public bool Unlocked(Upgrade c) {
            if(OpenAccess)return true;
            if(c.unLockCondition==-1)return true;
            if(c.unLockCondition==-2) {
                if(c.id==BaseId+5)return TalentValue(102)>0;
                if(c.id==12)return TalentValue(302)>0;
                if(KindOf(c)==2&&c.id==103)return TalentValue(303)>0;
                return false;
            }
            if(c.conditionIconType<0)return true;
            if(c.conditionIconType>4)return false;
            int need=c.unLockCondition,have=CurrencyLevel(c.conditionIconType);
            if(KindOf(c)==0&&Data.world==1&&c.id==3){need+=(int)Factor(3);have+=(int)Factor(3);}
            if(KindOf(c)==0&&Data.world==2&&c.id==103)need+=(int)Factor(77);
            return have>=need;
        }
        /// <summary>UpgradeManager.GetCurrencyCountForUnlock: the purchase level, plus the starting dice in world 2.</summary>
        public int CurrencyLevel(int quality){int lv=State.quantity[BaseId+quality+1];if(Data.world==2&&quality==1)lv+=(int)Factor(77);return lv;}
        /// <summary>UpgradeItem.GetLockConditionText.</summary>
        public string LockText(Upgrade c) {
            int n=c.unLockCondition,icon=c.conditionIconType;
            if(n>=0&&icon>0) {
                if(KindOf(c)==0&&c.id==3)n+=(int)Factor(3);else if(KindOf(c)==0&&c.id==103)n+=(int)Factor(77);
                string[] names=Data.world==1?new[]{"铜币","银币","金币","紫金币"}:new[]{"铜骰子","银骰子","金骰子","紫金骰子"};
                return icon<=4?names[icon-1]+"等级达到Lv"+n:"需要 "+n+" 个";
            }
            return n>=0||n==-1?"已解锁":n==-2?"天赋点系统解锁":"未知条件";
        }
        public int NextEvolutionLevel(int id){var c=Config(id,false);int lv=State.quantity[id];foreach(int t in c.evolutionLevel)if(t>=lv&&t>0)return t;return -1;}
        public bool CanHelperFlip(int quality) {
            if(OpenAccess)return quality>=1&&quality<=4;
            if(quality<=1)return true;
            if(quality==2)return Level(Data.world==1?4:104,true)>0;
            if(quality==3)return Level(Data.world==1?5:105,true)>0;
            if(quality==4)return TalentValue(Data.world==1?107:104)>0;
            return false;
        }
        public bool CanSwipe(int quality){if(quality<1||quality>3)return false;return OpenAccess||Level(Data.world==1?5+quality:105+quality,true)>0;}
        public float FrontChance(){float weight=.65f;if(Data.world==1)weight*=1f+(float)TalentValue(108);return Math.Min(.95f,Math.Max(.05f,weight));}
        public float FlightSeconds(){float scale=Data.world==1?1f-(float)TalentValue(106):(State.talents[107]>0?1f-(float)TalentValue(107):1f);return .8f*Math.Max(.2f,scale);}
        /// <summary>RoleVat.RefreshSleepProbability: 30% every 5 s; the talent "失眠" (201 coin / 202 dice) scales it.</summary>
        public float SleepChance(){float p=.3f;int id=Data.world==1?201:202;if(State.talents[id]>0)p*=(float)TalentValue(id);return Math.Min(1f,Math.Max(0f,p));}
        /// <summary>RoleVat.sleepDuration 6 s; in the dice world the talent 204 scales it.</summary>
        public float SleepSeconds(){float s=6f;if(Data.world==2&&State.talents[204]>0)s*=(float)TalentValue(204);return s;}
        /// <summary>RoleVat.EnterSkillCDWait for the basic helper: none in the coin world, the world's roll interval (2.4 s, sped up by upgrade 109 and talent 207) in the dice world.</summary>
        public float HelperCooldown() {
            if(Data.world!=2)return 0;
            float v=2.4f;var c=boosts.FirstOrDefault(x=>x.id==109);
            if(c!=null)v/=Math.Max(.1f,1f+(float)c.upgradeResult*State.boosts[109]);
            if(State.talents[207]>0)v*=(float)TalentValue(207);
            return v;
        }
        /// <summary>Premium helpers wait tbhelperrole.cdTime. Dice-world talent 207 scales that wait.</summary>
        public float RoleCooldown(int role) {
            if(role<=0)return HelperCooldown();
            var config=HelperConfig(role);if(config==null)return HelperCooldown();
            float cd=config.cdTime;if(Data.world==2&&State.talents[207]>0)cd*=(float)TalentValue(207);return cd;
        }
        public float EchoChance(){if(Data.world==1)return State.boosts[12]*(float)(Config(12,true).upgradeResult-1);return State.skills[103]*(float)(ConfigKind(103,2).upgradeResult-1);}
        public float CritChance(){float rate=(float)Factor(16);if(Data.world==1)rate+=(float)TalentValue(303);else if(State.talents[302]>0)rate+=(float)TalentValue(302);if(HelperCount(7)>0&&HelperAwake(7)>=3)rate+=.5f;return Math.Min(1f,Math.Max(0,rate));}
        /// <summary>DeskItem: a desk is owned at level 1 once the main mission level reaches its condition.</summary>
        public bool DeskUnlocked(Desk d)=>OpenAccess||d.deskUnlockCondition==-1||MainLevel>=d.deskUnlockCondition;
        public int DeskLevel(Desk d)=>DeskUnlocked(d)?Math.Max(1,State.deskLevel[d.id]):0;
        public double DeskPower(Desk d){int lv=DeskLevel(d);return lv<=0?1:d.skillNum*Math.Pow(d.skillUpgradeBuff,lv-1);}
        public double DeskPowerAt(Desk d,int lv)=>d.skillNum*Math.Pow(d.skillUpgradeBuff,Math.Max(0,lv-1));
        public double DeskPrice(Desk d){int lv=Math.Max(1,DeskLevel(d));return d.buyType==1?d.buyNum*Math.Pow(d.buyNumCostBuff,lv-1):d.buyNum+d.buyNumCostBuff*(lv-1);}
        public bool DeskUpgradeable(Desk d){if(!DeskUnlocked(d)||DeskLevel(d)>=d.skillLevelMax)return false;double cost=DeskPrice(d);return d.buyType==2?Data.diamonds>=(int)cost:State.gold>=cost;}
        bool DeskKind(Desk d,int kind){int id=d.id%100;return kind==1?id==1||id==4:kind==2?id==5:kind==3?id==6:kind==4?id==2:kind==5?id==3||id==7:id==8;}
        double DeskProduct(int kind){double product=1;foreach(var d in desks)if(d.WorldId==Data.world&&DeskLevel(d)>0&&DeskKind(d,kind))product*=DeskPower(d);return product;}
        public double CritMultiplier(){double sum=0;foreach(var d in desks)if(d.WorldId==Data.world&&DeskLevel(d)>0&&DeskKind(d,5))sum+=DeskPower(d);return sum>0?sum:Factor(17);}
        public bool BuyDesk(int id) {
            var d=desks.First(x=>x.id==id);int lv=DeskLevel(d);
            if(d.WorldId!=Data.world||!DeskUnlocked(d)||lv>=d.skillLevelMax)return false;
            double cost=DeskPrice(d);if(!Finite(cost)||cost<=0)return false;
            if(d.buyType==2){cost=(int)cost;if(Data.diamonds<cost)return false;Data.diamonds-=cost;}else if(State.gold<cost)return false;else State.gold-=cost;
            State.deskLevel[id]=lv+1;Changed?.Invoke();return true;
        }
        public int UsingDesk=>State.usingDesk>0&&desks.Any(x=>x.id==State.usingDesk&&x.WorldId==Data.world)?State.usingDesk:BaseId+1;
        public bool UseDesk(int id){var d=desks.FirstOrDefault(x=>x.id==id);if(d==null||d.WorldId!=Data.world||!DeskUnlocked(d)||UsingDesk==id)return false;State.usingDesk=id;Changed?.Invoke();return true;}
        public void AdvanceCheckIn(int today) {
            // GameDataManager.UpdateAllWorldsCheckInDay advances every world's
            // calendar together while preserving each world's separate claims.
            foreach(var state in Data.worlds) {
                if(state.checkDate==0){state.checkDate=today;if(state.checkDay<1)state.checkDay=1;continue;}
                if(state.checkDate==today)continue;
                state.checkDay++;if(state.checkDay>7){state.checkDay=1;state.checkMask=0;state.checkAdMask=0;}
                state.checkClaimed=false;state.checkDate=today;
            }
        }
        public SignReward SignFor(int day)=>signs.Where(x=>x.worldId==Data.world).OrderBy(x=>x.id).Skip(day-1).FirstOrDefault();
        public SignReward TodaySign()=>SignFor(State.checkDay);
        public bool CheckInClaimed(int day,bool special=false)=>((special?State.checkAdMask:State.checkMask)&(1<<day))!=0;
        public bool CanClaimCheckIn(int day,bool special=false)=>day>=1&&day<=(OpenAccess?7:State.checkDay)&&!CheckInClaimed(day,special)&&CheckInRewards(day,special)?.Length>=4;
        public int[] CheckInRewards(int day,bool special=false){var sign=SignFor(day);return special?sign?.AdReward:sign?.normalReward;}
        public bool AnyCheckInClaimable(){for(int d=1;d<=7;d++)if(CanClaimCheckIn(d)||CanClaimCheckIn(d,true))return true;return false;}
        /// <summary>Gold rewards scale with the recent per-minute income, like CheckInRewardItem.realCounts.</summary>
        public double RewardAmount(int itemId,int count)=>itemId==1?count*State.averagePerMinute:count;
        public bool ClaimCheckIn(int day)=>ClaimCheckIn(day,false,out _);
        public bool ClaimCheckIn(int day,bool special,out List<KeyValuePair<int,double>> rewards) {
            rewards=null;AdvanceCheckIn(TodayNumber());if(!CanClaimCheckIn(day,special))return false;
            var pairs=CheckInRewards(day,special);rewards=new List<KeyValuePair<int,double>>();
            bool unlock=special&&(day==3||day==7)&&pairs[2]>=101&&pairs[2]<=110&&!HelperOwned(pairs[2]-100);
            // Mark the claim before notifying listeners or granting gold: re-entrant refreshes cannot claim twice.
            if(special)State.checkAdMask|=1<<day;else State.checkMask|=1<<day;
            if(!special)State.checkClaimed=day==State.checkDay;
            for(int i=0;i+1<pairs.Length;i+=2) {
                int id=pairs[i];double amount=RewardAmount(id,pairs[i+1]);
                if(i==2&&unlock){State.helpers.Add(new HelperState{id=id-100,level=1,max=1,ad=1});continue;}
                if(id>=3&&id<=5)rewards.AddRange(ResolveFragments(id,(int)amount));
                else rewards.Add(new KeyValuePair<int,double>(id,amount));
            }
            foreach(var reward in rewards)if(reward.Key==1)AddEarnedGold(reward.Value,Time.unscaledTime);else GrantAmount(reward.Key,reward.Value);
            CheckGrowth();Changed?.Invoke();return true;
        }
        public bool ClaimCheckIn(){for(int d=1;d<=7;d++)if(CanClaimCheckIn(d))return ClaimCheckIn(d);return false;}
        public void GrantAmount(int itemId,double amount) {
            if(!Finite(amount)||amount<=0)return;
            if(itemId==1)State.gold+=amount;else GrantItem(itemId,(int)Math.Round(amount));
        }
        public static int TodayNumber(){var n=DateTime.Now;return n.Year*10000+n.Month*100+n.Day;}
        public void GrantPairs(int[] reward){if(reward==null)return;for(int i=0;i+1<reward.Length;i+=2)GrantItem(reward[i],reward[i+1]);}
        public void GrantItem(int itemId,int count) {
            if(count<=0)return;
            if(itemId>=3&&itemId<=5){foreach(var reward in ResolveFragments(itemId,count))Data.shards[reward.Key]+=(int)reward.Value;return;}
            if(itemId==1)State.gold+=count;else if(itemId==2)Data.diamonds+=count;else if(itemId==6)State.points+=count;else Data.shards[itemId]+=count;
        }
        public string ItemName(int id){if(id==1)return "金币";if(id==2)return "钻石";if(id==3)return "N碎片";if(id==4)return "R碎片";if(id==5)return "SR碎片";if(id==6)return "天赋点";if(id>=101&&id<=110)return (HelperConfig(id-100)?.helperName??"帮手"+(id-100))+"碎片";return "道具"+id;}
        public bool BuyChest(int id)=>BuyChest(id,out _);
        public bool BuyChest(int id,out List<KeyValuePair<int,double>> rewards) {
            rewards=null;
            var item=shop.FirstOrDefault(x=>x.id==id);if(item==null||item.cost<=0)return false;
            ResetShopDay();
            int used=id==202?Data.chestNormal:Data.chestRare;if(!OpenAccess&&item.todayLimit>0&&used>=item.todayLimit)return false;
            if(Data.diamonds<item.cost)return false;
            rewards=BuildShopRewards(item);Data.diamonds-=item.cost;
            foreach(var reward in rewards)GrantAmount(reward.Key,reward.Value);
            if(id==202)Data.chestNormal++;else Data.chestRare++;CheckGrowth();Changed?.Invoke();return true;
        }
        public bool CanEvolve(int quality) {int id=BaseId+quality+1;var c=Config(id,false);int tier=State.evolution[id];return tier<c.evolutionLevel.Length&&c.evolutionLevel[tier]>0&&State.quantity[id]>=c.evolutionLevel[tier];}
        public int MaxLevel(Upgrade c) {
            int kind=KindOf(c); int max=c.upgradeLvMax>0?c.upgradeLvMax:999999;
            if(kind==1&&Data.world==1&&(c.id<=3||c.id==14)&&State.talents[301]>0)max+=(int)(TalentValue(301)*State.talents[301]);
            if(kind==1&&Data.world==2&&((c.id>=101&&c.id<=103)||c.id==110)&&State.talents[301]>0)max+=(int)(TalentValue(301)*State.talents[301]);
            if(kind==0&&(c.id==1||c.id==101)){int talent=Data.world==1?207:201;int lv=State.talents[talent];if(lv>0)max+=(int)(TalentValue(talent)*lv);}
            return max;
        }
        public bool AtCap(Upgrade c,bool boost) {
            if(LevelKind(c.id,KindOf(c))>=MaxLevel(c))return true;
            return KindOf(c)==0&&c.id!=BaseId+1&&CanEvolve(c.id-BaseId-1);
        }
        /// <summary>Helper hiring stops every 5 levels (from 10) until the ad step is taken; ads are free here.</summary>
        public int HelperCap() {
            if(OpenAccess)return MaxLevel(Config(BaseId+1,false));
            int lv=Helpers,max=MaxLevel(Config(BaseId+1,false));
            int need=Math.Max(10,(int)Math.Ceiling(lv/5.0)*5);
            return Math.Min(Math.Max(State.helperAdCap,need),max);
        }
        public bool HelperGated=>Helpers>=HelperCap()&&Helpers<MaxLevel(Config(BaseId+1,false));
        public bool UnlockHelperCap() {
            if(!HelperGated)return false;
            State.helperAdCap=HelperCap()+5;State.quantity[BaseId+1]++;Changed?.Invoke();return true;
        }
        public bool Buy(int id,bool boost)=>BuyKind(id,boost?1:0);
        public bool BuyKind(int id,int kind) {
            var c=ConfigKind(id,kind); if(!Unlocked(c)||AtCap(c,false))return false;
            if(kind==0&&id==BaseId+1&&HelperGated)return false;
            if(c.costType!=3) {
                double cost=PriceKind(id,kind);if(!Finite(cost)||cost<=0)return false;
                if(c.costType==2){cost=Math.Ceiling(cost);if(Data.diamonds<cost)return false;Data.diamonds-=cost;}
                else{if(State.gold<cost)return false;State.gold-=cost;}
            }
            if(kind==2)State.skills[id]++;else if(kind==1)State.boosts[id]++;else State.quantity[id]++;
            Changed?.Invoke();return true;
        }
        public bool Evolve(int quality) {
            if(!CanEvolve(quality))return false;int id=BaseId+quality+1;
            State.quantity[id]++;State.evolution[id]++;State.evolvedAt[id]=State.quantity[id];
            Notice?.Invoke(Config(id,false).itemName+"进化！收益 ×10");Changed?.Invoke();return true;
        }
        public double Value(int quality,bool manual,bool critical,int role=0) {
            int valueId=quality==4?(Data.world==1?14:110):BaseId+quality;
            double baseValue=Factor(Data.world==1?(quality==4?15:4+quality):59+quality);
            double grown=Math.Pow(Config(valueId,true).upgradeResult,State.boosts[valueId]);
            if(quality==4)grown+=Data.world==1?TalentValue(109):TalentValue(106);
            if(role==3&&HelperAwake(3)>1)grown+=.3; // magician awake: the thrown coin's value multiplier gains 0.3
            double value=baseValue*grown;
            var evo=Config(BaseId+quality+1,false);for(int i=0;i<State.evolution[evo.id];i++)value*=evo.evolutionBuff[i];
            value*=SupportIncomeMultiplier+TalentValue(101);if(manual)value*=1+TalentValue(103);
            value*=RebornGain();
            if(quality>=1&&quality<=3)value*=DeskProduct(quality);
            value*=DeskProduct(6);if(manual)value*=DeskProduct(4);
            if(critical)value*=role>0?Math.Max(1,HelperSkillValue(role)):CritMultiplier();
            if(role>0)value*=HelperIncomeMul(role);
            return value;
        }
        public double Settle(int requestWorld,int quality,int face,bool manual,bool critical,double now,int role=0,float rewardScale=1) {
            if(requestWorld!=Data.world||quality<1||quality>4)return 0;
            if(Data.world==1 && face!=0&&face!=1)return 0;if(Data.world==2 && (face<1||face>6))return 0;
            double multiplier=Data.world==1?(face==1?1:TalentValue(309)):(face==1?1:Factor(62+face));
            double reward=Value(quality,manual,critical,role)*multiplier*Math.Max(0,rewardScale);
            if(!Finite(reward)||reward<=0)return 0;
            State.gold+=reward;State.lifetime+=reward;income.Enqueue(new KeyValuePair<double,double>(now,reward));
            UpdateRate(now);ObservePromotionIncome(reward);CheckGrowth();Changed?.Invoke();return reward;
        }
        public void UpdateRate(double now) {while(income.Count>0&&now-income.Peek().Key>30)income.Dequeue();State.averagePerMinute=Math.Max(1,income.Sum(x=>x.Value)*2*Math.Max(.1,Factor(13)));}
        public Mission NextMission=>missions.Where(x=>x.worldId==Data.world).OrderBy(x=>x.id).Skip(State.mission).FirstOrDefault();
        public event Action Growth;
        void CheckGrowth() {
            var m=NextMission;int guard=0;bool grew=false;
            while(!PromotionReady&&m!=null&&State.gold>=m.mainMissionNum&&guard++<100) {
                State.mission++;State.pendingGoals.Add(m.id);grew=true;
                m=NextMission;
            }
            if(grew){RefreshWorldUnlocks();RefreshHelperUnlocks(Data.world);}
            if(grew)Growth?.Invoke();
        }
        public Mission PendingGoal=>State.pendingGoals.Count==0?null:missions.FirstOrDefault(x=>x.id==State.pendingGoals[0]);
        /// <summary>Mission rewards are granted when the goal panel is confirmed, like GoalReachPanel.ClaimCurrentReward.</summary>
        public bool ClaimGoal(int multiplier=1) {
            var m=PendingGoal;if(m==null)return false;State.pendingGoals.RemoveAt(0);
            GrantAmount(m.rewardType,m.rewardNum*multiplier);CheckGrowth();Changed?.Invoke();return true;
        }
        public int MissionLevel(Mission m)=>Array.IndexOf(WorldMissions.Select(x=>x.id).ToArray(),m.id)+1;
        double TalentValueForWorld(int world,int id) {
            int level=Data.worlds[world-1].talents[id];var t=(world==1?coinTalents:diceTalents).FirstOrDefault(x=>x.id==id);
            return level<=0||t==null?0:t.upgradeResult[Math.Min(level-1,t.upgradeResult.Length-1)];
        }
        public double AccumulateOffline(long utc) {
            double total=0;
            for(int i=0;i<2;i++) {
                var s=Data.worlds[i];long elapsed=utc-s.lastExit;
                if(s.lastExit>0&&elapsed>=120&&s.mission>=1) {double cap=21600*(1+TalentValueForWorld(i+1,i==0?105:109));double multiplier=1+TalentValueForWorld(i+1,i==0?104:108);double reward=Math.Min(elapsed,cap)/60d*s.averagePerMinute*multiplier;if(Finite(reward)&&reward>0){s.pendingOffline+=reward;total+=reward;}}
                s.lastExit=utc;
            }
            return total;
        }
        public void ClaimOffline() {double value=State.pendingOffline;State.pendingOffline=0;if(Finite(value)&&value>0)State.gold+=value;CheckGrowth();Changed?.Invoke();}
        public bool SwitchWorld(int world) {
            RefreshWorldUnlocks();
            if(world<1||world>2||world==Data.world || !WorldUnlocked(world))return false;
            CancelPromotion();promotionResultGate=false;promotionClaimGate=false;Data.world=world;InitializeIncome(Time.unscaledTime);Changed?.Invoke();return true;
        }
        public PointReward[] RebornTable=>Data.world==1?coinPoints:dicePoints;
        /// <summary>TalentRebornTip.CalculateLevel: the highest reborn goal the current gold has passed.</summary>
        public int RebornReachedLevel(){var t=RebornTable;for(int i=0;i<t.Length;i++)if(State.gold<t[i].talentName)return i-1;return t.Length-1;}
        int RebornBonus=>(int)TalentValue(Data.world==1?308:310);
        public int RebornPoints() {
            var t=RebornTable;int reach=RebornReachedLevel(),sum=0;
            if(reach>=t.Length||reach<=State.rebornLevel)return 0;
            for(int i=State.rebornLevel+1;i<=reach;i++){int p;int.TryParse(t[i].talentDesc,out p);sum+=p+RebornBonus;}
            return sum;
        }
        /// <summary>Target shown in the reborn panel: the goal after the higher of the reached and claimed levels.</summary>
        public int RebornTargetIndex(){var t=RebornTable;int next=Math.Max(RebornReachedLevel(),State.rebornLevel)+1;return Math.Max(0,Math.Min(next,t.Length-1));}
        public int RebornTargetPoints(){int p;int.TryParse(RebornTable[RebornTargetIndex()].talentDesc,out p);return p+RebornBonus;}
        public bool RebornAtLimit=>RebornTargetIndex()>=RebornTable.Length-1&&RebornPoints()<=0;
        public double RebornGainAt(int level){var t=RebornTable;double g=1;for(int i=0;i<=Math.Min(level,t.Length-1);i++)g*=t[i].rebornBuff;return g;}
        public double RebornGain()=>RebornGainAt(State.rebornLevel);
        /// <summary>TalentRebornTip.RebornTalent(normal): grants points and resets upgrades; mission level and desks stay.</summary>
        public bool Rebirth() {
            int points=RebornPoints();if(points<=0)return false;
            State.rebornLevel=RebornReachedLevel();State.points+=points;State.rebirth++;
            double kept=State.gold*(Data.world==1?TalentValue(304):TalentValue(311));
            State.gold=Finite(kept)&&kept>0?kept:0;State.averagePerMinute=1;
            Array.Clear(State.quantity,0,128);Array.Clear(State.boosts,0,128);Array.Clear(State.skills,0,128);Array.Clear(State.evolution,0,128);Array.Clear(State.evolvedAt,0,128);income.Clear();
            State.helperAdCap=0;
            if(Maxed)PrepareMaxedSave();
            else if(DeveloperMode)PrepareDeveloperSave();
            Changed?.Invoke();return true;
        }
        public Talent[] Talents=>Data.world==1?coinTalents:diceTalents;
        public Talent[] TalentPage(int type)=>Talents.Where(x=>x.talentType==type).OrderBy(x=>x.position).ToArray();
        public int TalentCost(Talent t)=>t.baseTalentPoint+State.talents[t.id]*t.costPointAdd;
        /// <summary>TalentCardItem.SetItemInfo: the card at sibling index i needs the card at index Parent[i]-1 upgraded once.</summary>
        static readonly int[] TalentParent={-1,1,1,2,3,4,4,5,5,6,7,8,9};
        public bool TalentUnlocked(Talent t) {
            if(OpenAccess)return true;
            var page=TalentPage(t.talentType);int i=Array.IndexOf(page,t);if(i<0)return false;
            if(Data.world==1&&(t.id==1||t.id==7||t.id==14))return true;
            int parent=i<TalentParent.Length?TalentParent[i]:-1;
            if(parent<0)return true;
            return parent-1<page.Length&&State.talents[page[parent-1].id]>0&&TalentUnlocked(page[parent-1]);
        }
        public bool TalentUpgradeable(Talent t)=>TalentUnlocked(t)&&State.talents[t.id]<t.upgradeLevelMax&&State.points>=TalentCost(t);
        public bool BuyTalent(int id) {
            var t=Talents.First(x=>x.id==id);int lv=State.talents[id];int cost=TalentCost(t);
            if(!TalentUnlocked(t)||lv>=t.upgradeLevelMax||State.points<cost)return false;
            State.points-=cost;State.talents[id]++;Changed?.Invoke();return true;
        }
        public int TalentSpent(int type){int sum=0;foreach(var t in TalentPage(type))for(int l=0;l<State.talents[t.id];l++)sum+=t.baseTalentPoint+l*t.costPointAdd;return sum;}
        public bool ResetTalents(int type) {
            int refund=TalentSpent(type);if(refund<=0)return false;
            foreach(var t in TalentPage(type))State.talents[t.id]=0;
            State.points+=refund;Changed?.Invoke();return true;
        }
        void ResetShopDay(){int today=TodayNumber();if(Data.shopDate!=today){Data.shopDate=today;Data.chestNormal=Data.chestRare=0;Data.spinCount=0;Data.diamondClaims=0;}}
        static long UnixNow()=>DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public ShopItem[] WheelItems=>shop.Where(x=>x.id>=101&&x.id<=110).OrderBy(x=>x.id).ToArray();
        /// <summary>ShopTurntablePanel wheel limit is the first wedge's todayLimit (10). The first spin each day is free; later ones wait out common factor 35.</summary>
        public int WheelLimit{get{ResetShopDay();var w=WheelItems;return w.Length==0?0:Math.Max(0,w[0].todayLimit);}}
        public bool SpinIsFree{get{ResetShopDay();return Data.spinCount==0;}}
        public bool SpinCooling=>!OpenAccess&&UnixNow()<Data.spinReadyAt;
        public int SpinCooldownLeft{get{long s=Data.spinReadyAt-UnixNow();return OpenAccess||s<=0?0:(int)s;}}
        public bool CanSpin{get{ResetShopDay();return OpenAccess||Data.spinCount<WheelLimit&&!SpinCooling;}}
        public bool DiamondCooling=>!OpenAccess&&UnixNow()<Data.diamondReadyAt;
        public int DiamondCooldownLeft{get{long s=Data.diamondReadyAt-UnixNow();return OpenAccess||s<=0?0:(int)s;}}
        public int SpinProgress=>Data.spinProgress;
        public const int SpinProgressMax=5;
        public ShopItem Spin(float roll) {
            if(!CanSpin)return null;var items=WheelItems;if(items.Length==0)return null;
            bool free=Data.spinCount==0;
            double total=items.Sum(x=>Math.Max(0,x.rate)),pick=roll*total,acc=0;ShopItem result=items[items.Length-1];
            foreach(var it in items){acc+=Math.Max(0,it.rate);if(pick<=acc){result=it;break;}}
            Data.spinCount++;
            if(!free)Data.spinReadyAt=UnixNow()+(long)(Factor(35)*60);
            Changed?.Invoke();return result;
        }
        /// <summary>Shop button 201. Ads are not played; the claim still waits out common factor 36 and the daily cap.</summary>
        public bool ClaimDiamondAd() {
            ResetShopDay();var item=shop.FirstOrDefault(x=>x.id==201);
            if(item==null||DiamondCooling)return false;
            if(!OpenAccess&&item.todayLimit>0&&Data.diamondClaims>=item.todayLimit)return false;
            if(item.count==null||item.count.Length==0)return false;
            GrantItem(item.rewardType[0],item.count[0]);
            Data.diamondClaims++;Data.diamondReadyAt=UnixNow()+(long)(Factor(36)*60);
            Changed?.Invoke();return true;
        }
        public double ShopRewardAmount(ShopItem item,int index)=>item.rewardType[index]==1?State.averagePerMinute*item.count[index]:item.count[index];
        List<KeyValuePair<int,double>> ResolveFragments(int type,int count) {
            // Helper 9 is event-only and has no acquisition path in the offline rebuild.
            var pool=helperRoles.Where(r=>r.Rare==type-2&&r.id!=9).Select(r=>100+r.id).ToArray();
            var amounts=new Dictionary<int,double>();
            for(int i=0;i<count&&pool.Length>0;i++) {
                int id=pool[UnityEngine.Random.Range(0,pool.Length)];
                amounts[id]=amounts.TryGetValue(id,out var amount)?amount+1:1;
            }
            return amounts.OrderBy(x=>x.Key).ToList();
        }
        /// <summary>Resolve random helper shards once, so the receipt exactly matches the granted items.</summary>
        public List<KeyValuePair<int,double>> BuildShopRewards(ShopItem item) {
            var rewards=new List<KeyValuePair<int,double>>();
            for(int i=0;i<item.rewardType.Length&&i<item.count.Length;i++) {
                int type=item.rewardType[i];
                if(type>=3&&type<=5)rewards.AddRange(ResolveFragments(type,item.count[i]));
                else rewards.Add(new KeyValuePair<int,double>(type,ShopRewardAmount(item,i)));
            }
            return rewards;
        }
        public void GrantShop(ShopItem item)=>GrantShop(item,BuildShopRewards(item));
        public void GrantShop(ShopItem item,List<KeyValuePair<int,double>> rewards) {
            foreach(var reward in rewards)GrantAmount(reward.Key,reward.Value);
            if(item.id>=101&&item.id<=110)Data.spinProgress=Math.Min(SpinProgressMax,Data.spinProgress+1);
            CheckGrowth();Changed?.Invoke();
        }
        public bool ClaimSpinChest()=>ClaimSpinChest(out _);
        public bool ClaimSpinChest(out List<KeyValuePair<int,double>> rewards){rewards=null;var item=shop.FirstOrDefault(x=>x.id==301);if(item==null||Data.spinProgress<SpinProgressMax)return false;rewards=BuildShopRewards(item);Data.spinProgress=0;GrantShop(item,rewards);return true;}
        // ------------------------------------------------------------ premium helpers (HelperInfo / UseHelperUI / RoleVat)
        public HelperRole HelperConfig(int id){return helperRoles==null?null:helperRoles.FirstOrDefault(x=>x.id==id);}
        public HelperState HelperOf(int id){return State.helpers==null?null:State.helpers.Find(x=>x.id==id);}
        public int HelperCount(int id){var s=HelperOf(id);return s==null?0:s.count;}
        public int AssignedHelpers{get{int n=0;if(State.helpers!=null)foreach(var h in State.helpers)n+=h.count;return n;}}
        int MissionOf(int world){if(world<1||world>Data.worlds.Length)return 1;return Data.worlds[world-1].mission+1;}
        /// <summary>HelperItem.UpdateUnlockedState. Ownership is remembered per world. 哈大壮 (id 9) stays hidden until granted.</summary>
        public void RefreshHelperUnlocks(int world) {
            if(helperRoles==null||world<1||world>Data.worlds.Length)return;
            var state=Data.worlds[world-1];if(state.helpers==null)state.helpers=new List<HelperState>();
            int level=state.mission+1;
            foreach(var role in helperRoles) {
                if(role.id==9||state.helpers.Exists(x=>x.id==role.id))continue;
                bool ready=role.UnlockLevel<=0||role.UnlockLevel==world||MissionOf(role.UnlockLevel)>=Math.Max(1,role.helperUnlockCondition);
                if(!ready)continue;
                if(role.helperUnlockCondition==-1||(role.helperUnlockCondition>0&&level>=role.helperUnlockCondition))
                    state.helpers.Add(new HelperState{id=role.id,level=1,max=1});
            }
        }
        public bool HelperOwned(int id)=>HelperOf(id)!=null;
        public bool CanAdUnlock(HelperRole role) {
            if(role==null||role.id==9||HelperOwned(role.id)||role.helperAdUnlockCondition<=0||MainLevel<role.helperAdUnlockCondition)return false;
            if(role.UnlockLevel>0&&role.UnlockLevel!=Data.world&&MissionOf(role.UnlockLevel)<Math.Max(1,role.helperUnlockCondition))return false;
            return true;
        }
        public bool AdUnlockHelper(int id) {
            var role=HelperConfig(id);if(!CanAdUnlock(role))return false;
            State.helpers.Add(new HelperState{id=id,level=1,max=1,ad=1});Changed?.Invoke();return true;
        }
        public static int SkillRank(HelperRole role,int level) {
            if(role==null||role.upgradeLevel==null||role.upgradeLevel.Length==0||level<role.upgradeLevel[0])return 0;
            for(int i=role.upgradeLevel.Length-1;i>=0;i--)if(level>=role.upgradeLevel[i])return i+1;
            return 0;
        }
        public int HelperAwake(int id) {
            var role=HelperConfig(id);var state=HelperOf(id);int level=state==null?1:Math.Max(1,state.level);
            if(role==null||role.awakeSkillNum==null||role.awakeSkillNum.Length==0||level<role.awakeSkillNum[0])return 0;
            for(int i=role.awakeSkillNum.Length-1;i>=0;i--)if(level>=role.awakeSkillNum[i])return i+1;
            return 0;
        }
        /// <summary>UpgradeManager.GetHelperExtraBuff: base count plus one step per reached upgrade level.</summary>
        public float HelperSkillValue(int id) {
            var role=HelperConfig(id);var state=HelperOf(id);if(role==null)return 1;
            return role.baseSkillNum+SkillRank(role,state==null?1:state.level)*role.upgradeSkillNum;
        }
        /// <summary>UpgradeManager.GetHelperIncomeMultiplier: Pow(buff, level-1), and 1 at level 1.</summary>
        public float HelperIncomeMul(int id) {
            if(id<=0)return 1;var role=HelperConfig(id);var state=HelperOf(id);int level=state==null?1:state.level;
            if(role==null||level<=1)return 1;return (float)Math.Pow(role.helperIncomeAddBuff,level-1);
        }
        public double HelperUpgradeCost(HelperRole role,int level)=>role.UpgradeNum*Math.Pow(role.UpgradeNumCostBuff,Math.Max(0,level-1));
        public int HelperFragmentCost(HelperRole role,int level) {
            int now=SkillRank(role,level),next=SkillRank(role,level+1);
            if(next<=now||role.UpgradeCostType==null||now<0||now>=role.UpgradeCostType.Length)return 0;
            return role.UpgradeCostType[now];
        }
        public int Fragments(int id){int i=100+id;return Data.shards!=null&&i>=0&&i<Data.shards.Length?Data.shards[i]:0;}
        public bool CanUpgradeHelper(int id) {
            var role=HelperConfig(id);var state=HelperOf(id);
            if(role==null||state==null||state.level>=role.upgradeLevelMax)return false;
            return State.gold>=HelperUpgradeCost(role,state.level)&&Fragments(id)>=HelperFragmentCost(role,state.level);
        }
        public bool UpgradeHelper(int id) {
            if(!CanUpgradeHelper(id))return false;
            var role=HelperConfig(id);var state=HelperOf(id);
            State.gold-=HelperUpgradeCost(role,state.level);
            int frag=HelperFragmentCost(role,state.level),index=100+id;
            if(frag>0&&index>=0&&index<Data.shards.Length)Data.shards[index]-=frag;
            state.level++;Changed?.Invoke();return true;
        }
        public bool SlotUsesGold(HelperRole role,int max)=>role.buyType==1&&max<=role.buyNumLevel;
        public double SlotGoldCost(HelperRole role,int max){float buff=role.buyNumCostBuff!=null&&role.buyNumCostBuff.Length>0?role.buyNumCostBuff[0]:1;return role.buyNum*Math.Pow(buff,Math.Max(0,max-1));}
        public double SlotDiamondCost(HelperRole role,int max) {
            float buff=role.buyNumCostBuff!=null&&role.buyNumCostBuff.Length>1?role.buyNumCostBuff[1]:0;
            if(role.buyType==1)return role.buyNumDiamond+buff*(max-role.buyNumLevel-1);
            return role.buyNumDiamond+buff*(max-1);
        }
        public bool BuyHelperSlot(int id) {
            var role=HelperConfig(id);var state=HelperOf(id);if(role==null||state==null)return false;
            if(SlotUsesGold(role,state.max)){double cost=SlotGoldCost(role,state.max);if(State.gold<cost)return false;State.gold-=cost;}
            else {double cost=SlotDiamondCost(role,state.max);if(Data.diamonds<cost)return false;Data.diamonds-=cost;}
            state.max++;Changed?.Invoke();return true;
        }
        public bool AdjustHelperCount(int id,int delta) {
            var state=HelperOf(id);if(state==null)return false;
            int next=state.count+delta;if(next<0||next>state.max)return false;
            if(delta>0&&AssignedHelpers>=Helpers)return false;
            state.count=next;Changed?.Invoke();return true;
        }
        /// <summary>GameDataManager.GetHelperIds: assigned premium helpers first, the remaining slots are the basic helper.</summary>
        public int[] HelperLineup(int slots) {
            RefreshHelperUnlocks(Data.world);
            var ids=new int[Math.Max(0,slots)];int n=0;
            if(State.helpers!=null)foreach(var h in State.helpers.OrderBy(x=>x.id))for(int i=0;i<h.count&&n<slots;i++)ids[n++]=h.id;
            return ids;
        }
        static readonly string[] Units=BuildUnits();
        static string[] BuildUnits() {
            var list=new List<string>{"","K","M","B","T"};
            for(char a='A';a<='C';a++)for(char b='A';b<='Z';b++)list.Add(a.ToString()+b);
            return list.ToArray();
        }
        /// <summary>NumberFormatter.FormatNumber of the original game.</summary>
        public static string Format(double value) {
            var inv=System.Globalization.CultureInfo.InvariantCulture;
            bool negative=value<0;if(negative)value=-value;string sign=negative?"-":"";
            if(value<100)return sign+value.ToString("F0",inv);
            if(value<1000)return sign+(int)value;
            int unit=0;double n=value;
            while(n>=1000&&unit<Units.Length-1){n/=1000;unit++;}
            string text=n<10?n.ToString("F2",inv)+Units[unit]:n<100?n.ToString("F1",inv)+Units[unit]:(int)n+Units[unit];
            return sign+text;
        }
    }
}

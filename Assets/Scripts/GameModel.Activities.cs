using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bufu.Rebuild {
    [Serializable] public sealed class ActivityChallengeConfig {
        public int id,limitTIme,helperNum,coinType1,coinNum1,coinType2,coinNum2,rewardType;
        public double costMoney,limitMissionCondition,limitGoalFactor,coinBaseValue1,coinBaseValue2,rewardNum;
        public float coinBuff1,coinBuff2;
        public int[] baseCostMoney1,baseCostMoney2,costMoneyLevel1,costMoneyLevel2;
        public float[] costMoneyBuff1,costMoneyBuff2;
    }
    [Serializable] public sealed class ActivityAuctionConfig {
        public int auctionHouseID,freeTicket,timeRecover,itemValueMin,itemValueMax;
        public string auctionName;public float conditionLevel,playerOfferAdd1,playerOfferAdd2,playerOfferAdd3,quitOutTime;
        public int[] auctionRoundMin,auctionRoundMax,resultItemRarity,resultItemWeight,resultItemPriceWeight;
        public float[] startPriceRange,expectPriceRange,npcOfferAddRange,npcOfferRate,npcQuitOfferRange,finalValueBuff,raisePriceStage,raiseBuffRange,raisePriceIncomeRange,giveUpChance1,giveUpChance2,giveUpChance3,giveUpChance4,angerProbability,angerRound;
    }
    [Serializable] public sealed class ActivityAuctionItemConfig {public int id,specialItem;public string itemName;public int[] rarity;}
    public sealed class LocalChallengeSession {
        public ActivityChallengeConfig Config;public int World;public float Remaining,Elapsed,Multiplier=1;
        public double Collected;public bool Finished,Won,Claimed,SkillPending,Revived;public int SkillChoices;
        public int[] Upgrades=new int[4];public Dictionary<int,int> Team;
        public List<KeyValuePair<int,double>> Receipt;
    }
    public sealed class LocalAuctionSession {
        public ActivityAuctionConfig Config;public int LastBidder=-2,BidCount,MinimumBids,MaximumBids,NextNpc,LastGiveUpRound,AngryNpc=-1,AngryRounds;
        public double RealValue,Price,StartingPrice,FinalValue,RaiseIncome;public int Phase;public float Elapsed,PhaseTime,HostRemaining=10,NpcRemaining=2,OfferRate,CompareRate,QuitRatio;
        public bool Finished,Claimed,RaiseMode,AngerRolled;public bool[] GivenUp=new bool[4];public float[] RaiseStages;
        public int[] ItemIds,Rarities;public double[] ItemValues;public System.Random Random;
        public bool PlayerWon=>LastBidder==-1;
    }
    public sealed partial class GameModel {
        ActivityChallengeConfig[] activityChallenges;ActivityAuctionConfig[] activityAuctions;ActivityAuctionItemConfig[] activityItems;
        public ActivityChallengeConfig[] ActivityChallenges=>activityChallenges??(activityChallenges=Read<ActivityChallengeConfig>("tbchallenge"));
        public ActivityAuctionConfig[] ActivityAuctionHouses=>activityAuctions??(activityAuctions=Read<ActivityAuctionConfig>("tbauction"));
        public ActivityAuctionItemConfig[] ActivityAuctionItems=>activityItems??(activityItems=Read<ActivityAuctionItemConfig>("tbauctionitem"));
        public LocalChallengeSession LocalChallenge{get;private set;}
        public LocalAuctionSession LocalAuction{get;private set;}
        public void EnsureActivitiesData() {
            if(Data.activitiesVersion>0)return;
            Data.challengeLevel=Math.Max(1,Data.challengeLevel);Data.ticketCount=3;Data.auctionCoin=Factor(52);
            Data.auctionCoinExchangeCount=Data.auctionDiamondExchangeCount=1;
            Data.lastTicketRecoverTime=DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();Data.activitiesVersion=1;
        }
        public ActivityChallengeConfig CurrentActivityChallenge=>ActivityChallenges.FirstOrDefault(x=>x.id==Math.Max(1,Data.challengeLevel))??ActivityChallenges.Last();
        public int ActivityHelperCap(int id) {var role=HelperConfig(id);var state=HelperOf(id);return role!=null&&state!=null&&(role.helperUnlockCondition==-1||MainLevel>=role.helperUnlockCondition||state.ad==1)?Math.Max(0,state.max):0;}
        public Dictionary<int,int> AutoActivityTeam() {
            int left=CurrentActivityChallenge.helperNum;var result=new Dictionary<int,int>();
            foreach(var role in helperRoles.OrderByDescending(x=>x.id)){int count=Math.Min(left,ActivityHelperCap(role.id));if(count>0){result[role.id]=count;left-=count;}}
            return result;
        }
        public bool StartLocalChallenge(Dictionary<int,int> team) {
            EnsureActivitiesData();
            var config=CurrentActivityChallenge;
            if(LocalChallenge!=null&&!LocalChallenge.Finished||State.gold<config.costMoney||team==null||team.Values.Any(x=>x<0)||team.Values.Sum()>config.helperNum||team.Any(x=>x.Value>ActivityHelperCap(x.Key)))return false;
            State.gold-=config.costMoney;
            LocalChallenge=new LocalChallengeSession{Config=config,World=Data.world,Remaining=config.limitTIme,Team=new Dictionary<int,int>(team)};
            Changed?.Invoke();return true;
        }
        public void TickLocalChallenge(float delta) {
            var run=LocalChallenge;if(run==null||run.Finished||run.SkillPending||delta<=0)return;
            run.Elapsed+=delta;run.Remaining=Math.Max(0,run.Remaining-delta);
            if(run.SkillChoices<2&&run.Elapsed>=(run.SkillChoices+1)*20){run.SkillPending=true;return;}
            if(run.Remaining<=0){run.Finished=true;Changed?.Invoke();}
        }
        public bool ChooseLocalChallengeSkill() {var run=LocalChallenge;if(run==null||run.Finished||!run.SkillPending)return false;run.Multiplier*=1.2f;run.SkillChoices++;run.SkillPending=false;return true;}
        public int LocalChallengeCoinCount(int type) {
            var run=LocalChallenge;if(run==null)return 0;
            int initial=(run.Config.coinType1==type?run.Config.coinNum1:0)+(run.Config.coinType2==type?run.Config.coinNum2:0);
            return initial+(type==2?run.Upgrades[0]:type==3?run.Upgrades[2]:0);
        }
        public double LocalChallengeCoinValue(int type) {
            var run=LocalChallenge;if(run==null)return 0;double value=1;
            // ChallengeMain.GetChallengeCoinValue enumerates the table; retain the last matching base.
            foreach(var config in ActivityChallenges){if(config.coinType1==type)value=config.coinBaseValue1;if(config.coinType2==type)value=config.coinBaseValue2;}
            return value*run.Multiplier*(type==2?Math.Pow(run.Config.coinBuff1,run.Upgrades[1]):type==3?Math.Pow(run.Config.coinBuff2,run.Upgrades[3]):1);
        }
        public bool AddLocalChallengeCoin(int type,bool heads,double multiplier=1) {
            var run=LocalChallenge;if(run==null||run.Finished||run.SkillPending)return false;
            if(heads)run.Collected+=LocalChallengeCoinValue(type)*Math.Max(0,multiplier);
            if(run.Collected>=run.Config.limitGoalFactor) {
                run.Collected=run.Config.limitGoalFactor;run.Finished=run.Won=true;
                int rewardType=run.Config.rewardType;double reward=rewardType==1?Data.worlds[run.World-1].averagePerMinute*run.Config.rewardNum:run.Config.rewardNum;
                run.Receipt=rewardType>=3&&rewardType<=5?ResolveFragments(rewardType,(int)reward):new List<KeyValuePair<int,double>>{new KeyValuePair<int,double>(rewardType,reward)};
                Data.challengeLevel=Math.Min(ActivityChallenges.Max(x=>x.id),run.Config.id+1);Changed?.Invoke();
            }
            return true;
        }
        public double LocalChallengeUpgradeCost(int kind) {
            var run=LocalChallenge;if(run==null||kind<0||kind>3)return 0;
            bool silver=kind<2;int level=run.Upgrades[kind];var levels=silver?run.Config.costMoneyLevel1:run.Config.costMoneyLevel2;
            var bases=silver?run.Config.baseCostMoney1:run.Config.baseCostMoney2;var buffs=silver?run.Config.costMoneyBuff1:run.Config.costMoneyBuff2;
            if(levels==null||levels.Length==0||bases==null||bases.Length==0)return 0;
            int stage=0;for(int i=0;i<levels.Length;i++){stage=i;if(level<=levels[i])break;}
            return bases[0]*Math.Pow(buffs!=null&&stage<buffs.Length?buffs[stage]:1,level);
        }
        public bool BuyLocalChallengeUpgrade(int kind) {
            var run=LocalChallenge;if(run==null||run.Finished||run.SkillPending||kind<0||kind>3)return false;
            double cost=LocalChallengeUpgradeCost(kind);if(cost<=0||run.Collected<cost)return false;
            run.Collected-=cost;run.Upgrades[kind]++;return true;
        }
        public List<KeyValuePair<int,double>> ClaimLocalChallenge(int multiplier=1) {
            var run=LocalChallenge;if(run==null||!run.Won||run.Claimed||multiplier<1||multiplier>2)return null;
            run.Claimed=true;var receipt=run.Receipt.Select(x=>new KeyValuePair<int,double>(x.Key,x.Value*multiplier)).ToList();GrantRewardReceipt(run.World,receipt);return receipt;
        }
        public void AbandonLocalChallenge(){if(LocalChallenge!=null){LocalChallenge.Finished=true;LocalChallenge.Claimed=true;}}
        public bool ReviveLocalChallenge(){var run=LocalChallenge;if(run==null||!run.Finished||run.Won||run.Revived)return false;run.Revived=true;run.Finished=false;run.Remaining=20;return true;}

        public int RecoverAuctionTickets(long nowMilliseconds) {
            EnsureActivitiesData();
            Data.ticketCount=Mathf.Clamp(Data.ticketCount,0,3);long interval=Math.Max(1,ActivityAuctionHouses[0].timeRecover)*60000L;
            if(Data.ticketCount<3&&Data.lastTicketRecoverTime>0&&nowMilliseconds-Data.lastTicketRecoverTime>=interval) {
                Data.ticketCount=Math.Min(3,Data.ticketCount+(int)((nowMilliseconds-Data.lastTicketRecoverTime)/interval));Data.lastTicketRecoverTime=nowMilliseconds;
            } else if(Data.ticketCount>=3)Data.lastTicketRecoverTime=nowMilliseconds;
            return Data.ticketCount;
        }
        public float ActivityMissionProgress {
            get {double goal=NextMission?.mainMissionNum??0;return MainLevel+(goal>0?(float)Math.Min(1,State.gold/goal):0);}
        }
        public double AuctionEntryValue(int houseId) {
            var config=ActivityAuctionHouses.FirstOrDefault(x=>x.auctionHouseID==houseId);if(config==null)return 0;
            var next=ActivityAuctionHouses.FirstOrDefault(x=>x.auctionHouseID==houseId+1);
            if(next==null)return ActivityMissionProgress>=config.conditionLevel?config.itemValueMax:config.itemValueMin;
            return config.itemValueMin+(config.itemValueMax-config.itemValueMin)*Mathf.Clamp01((ActivityMissionProgress-config.conditionLevel)/(next.conditionLevel-config.conditionLevel));
        }
        static float ActivityRange(System.Random random,float[] values,float fallback=1)=>values==null||values.Length==0?fallback:values.Length==1?values[0]:Mathf.Lerp(values[0],values[1],(float)random.NextDouble());
        static int ActivityIntRange(System.Random random,int[] values,int fallback=1)=>values==null||values.Length==0?fallback:values.Length==1?values[0]:random.Next(values[0],values[1]+1);
        public bool StartLocalAuction(int houseId,long nowMilliseconds,int seed,bool ticketAd=false) {
            var config=ActivityAuctionHouses.FirstOrDefault(x=>x.auctionHouseID==houseId);
            if(LocalAuction!=null&&!LocalAuction.Finished||config==null||MainLevel<config.conditionLevel||Data.auctionCoin<AuctionEntryValue(houseId))return false;
            if(RecoverAuctionTickets(nowMilliseconds)<=0&&!ticketAd)return false;
            if(Data.ticketCount>0){Data.ticketCount--;if(Data.lastTicketRecoverTime<=0)Data.lastTicketRecoverTime=nowMilliseconds;}
            var random=new System.Random(seed);double value=AuctionEntryValue(houseId);
            var run=new LocalAuctionSession{Config=config,Random=random,RealValue=value,MinimumBids=ActivityIntRange(random,config.auctionRoundMin),MaximumBids=ActivityIntRange(random,config.auctionRoundMax),OfferRate=ActivityRange(random,config.npcOfferRate),CompareRate=(float)random.NextDouble(),QuitRatio=ActivityRange(random,config.npcQuitOfferRange)};
            run.StartingPrice=run.Price=Math.Floor(value*ActivityRange(random,config.startPriceRange));run.FinalValue=value*ActivityRange(random,config.finalValueBuff);
            float raise=ActivityRange(random,config.raiseBuffRange);run.RaiseStages=(config.raisePriceStage??new float[0]).Select(x=>x*raise).ToArray();
            // Original auctionPlayPanel has six result slots; each rarity is weighted, then shares the real final value.
            int count=6;run.ItemIds=new int[count];run.Rarities=new int[count];run.ItemValues=new double[count];double weights=0;
            for(int i=0;i<count;i++) {
                int total=config.resultItemWeight.Sum(),pick=random.Next(1,total+1),acc=0,rarity=config.resultItemRarity[0];
                for(int j=0;j<config.resultItemWeight.Length;j++){acc+=config.resultItemWeight[j];if(pick<=acc){rarity=config.resultItemRarity[j];break;}}
                run.Rarities[i]=rarity;var pool=ActivityAuctionItems.Where(x=>x.rarity.Contains(rarity)).ToArray();run.ItemIds[i]=pool[random.Next(pool.Length)].id;
                run.ItemValues[i]=rarity*(.8+random.NextDouble()*.4);weights+=run.ItemValues[i];
            }
            for(int i=0;i<count;i++)run.ItemValues[i]=run.FinalValue*run.ItemValues[i]/weights;
            Data.auctionCount++;LocalAuction=run;Changed?.Invoke();return true;
        }
        public bool BidLocalAuction(int option) {
            var run=LocalAuction;if(run==null||run.Finished||run.Phase!=2||run.LastBidder==-1||option<0||option>2)return false;
            float factor=option==0?run.Config.playerOfferAdd1:option==1?run.Config.playerOfferAdd2:run.Config.playerOfferAdd3;
            double price=run.Price*factor;if(price>Data.auctionCoin)return false;
            run.Price=price;run.LastBidder=-1;run.HostRemaining=10;UpdateAuctionRaise(run);return true;
        }
        void UpdateAuctionRaise(LocalAuctionSession run) {
            if(!run.RaiseMode&&run.RealValue>0&&run.RaiseStages.Length>0&&run.Price/run.RealValue>=run.RaiseStages.Min())run.RaiseMode=true;
            if(!run.RaiseMode||run.AngerRolled)return;run.AngerRolled=true;
            if(run.Random.NextDouble()<=ActivityRange(run.Random,run.Config.angerProbability,0)) {
                var pool=Enumerable.Range(0,run.GivenUp.Length).Where(i=>!run.GivenUp[i]).ToArray();if(pool.Length==0)return;
                run.AngryNpc=pool[run.Random.Next(pool.Length)];var range=run.Config.angerRound;
                if(range!=null&&range.Length>0)run.AngryRounds=run.Random.Next(Math.Max(1,Mathf.RoundToInt(range[0])),Math.Max(1,Mathf.RoundToInt(range[range.Length-1]))+1);
            }
        }
        void LocalAuctionNpcBid(LocalAuctionSession run) {
            int npc=-1;bool angry=run.AngryRounds>0&&run.AngryNpc>=0&&!run.GivenUp[run.AngryNpc];
            if(angry){if(run.LastBidder==run.AngryNpc)return;npc=run.AngryNpc;}
            else {var pool=Enumerable.Range(0,run.GivenUp.Length).Where(i=>i!=run.LastBidder&&!run.GivenUp[i]).ToArray();if(pool.Length>0)npc=pool[run.Random.Next(pool.Length)];}
            if(npc<0)return;run.BidCount++;
            if(run.BidCount>run.MinimumBids) {
                int round=run.BidCount-run.MinimumBids;
                if(round>run.LastGiveUpRound) {
                    run.LastGiveUpRound=round;float[] chances=round==1?run.Config.giveUpChance1:round==2?run.Config.giveUpChance2:round==3?run.Config.giveUpChance3:run.Config.giveUpChance4;
                    for(int i=0;i<run.GivenUp.Length;i++)if(!run.GivenUp[i]&&!(run.AngryRounds>0&&run.AngryNpc==i)&&run.Random.NextDouble()<=ActivityRange(run.Random,chances,0))run.GivenUp[i]=true;
                }
            }
            run.NpcRemaining=4+(float)run.Random.NextDouble()*4;run.CompareRate=(float)run.Random.NextDouble();if(run.GivenUp[npc])return;
            run.Price*=ActivityRange(run.Random,run.Config.npcOfferAddRange);run.LastBidder=npc;run.HostRemaining=10;
            if(angry)run.AngryRounds--;UpdateAuctionRaise(run);
        }
        public void TickLocalAuction(float delta) {
            var run=LocalAuction;if(run==null||run.Finished||delta<=0)return;run.Elapsed+=delta;
            if(run.Phase==0){if(run.PhaseTime<1){run.PhaseTime+=delta;return;}run.Phase=1;run.PhaseTime=0;return;}
            if(run.Phase==1) {
                // The original welcomes the room while incrementing this timer twice below 3.
                if(run.PhaseTime<3)run.PhaseTime+=delta;
                if(run.PhaseTime<5)run.PhaseTime+=delta;else {run.Phase=2;run.PhaseTime=0;}
                return;
            }
            bool angry=run.AngryRounds>0&&run.AngryNpc>=0&&!run.GivenUp[run.AngryNpc];
            bool npcAllowed=angry||(run.CompareRate<=run.OfferRate?(run.BidCount<run.MinimumBids||run.BidCount<run.MaximumBids)&&run.Price<run.RealValue*run.QuitRatio:run.BidCount<run.MinimumBids);
            if(npcAllowed){run.NpcRemaining-=delta;if(run.NpcRemaining<=0)LocalAuctionNpcBid(run);}
            if(run.Price!=run.StartingPrice){run.HostRemaining-=delta;if(run.HostRemaining<=1)FinishLocalAuction(run);}
        }
        void FinishLocalAuction(LocalAuctionSession run) {
            run.Finished=true;
            if(!run.PlayerWon&&run.RaiseMode&&run.FinalValue>0) {
                double ratio=run.Price/run.FinalValue;int stage=run.RaiseStages.Count(x=>ratio>=x);
                var rates=run.Config.raisePriceIncomeRange;
                if(rates!=null&&rates.Length>0)run.RaiseIncome=Math.Abs((run.Price-run.FinalValue)*rates[Math.Min(stage,rates.Length-1)]);
            }
        }
        public void LeaveLocalAuctionBidding(){var run=LocalAuction;if(run==null||run.Finished)return;if(run.LastBidder==-1)return;FinishLocalAuction(run);}
        public double ClaimLocalAuction(bool doubled=false) {
            var run=LocalAuction;if(run==null||!run.Finished||run.Claimed)return 0;run.Claimed=true;
            double change=run.PlayerWon?run.FinalValue*(doubled?2:1)-run.Price:run.RaiseIncome;
            double before=Data.auctionCoin;Data.auctionCoin=Math.Max(0,before+change);Changed?.Invoke();return Data.auctionCoin-before;
        }
        public double AuctionExchangeCost(bool diamond)=>Factor(diamond?55:54)*Math.Pow(1+Factor(diamond?51:50),(diamond?Data.auctionDiamondExchangeCount:Data.auctionCoinExchangeCount)-1);
        public bool ExchangeAuctionCoins(bool diamond) {
            EnsureActivitiesData();if(diamond&&MainLevel<Factor(49))return false;
            double cost=AuctionExchangeCost(diamond);if(cost<=0||Data.auctionCoin<cost)return false;
            Data.auctionCoin-=cost;
            if(diamond){Data.diamonds+=(long)Math.Ceiling(Factor(57));Data.auctionDiamondExchangeCount++;}
            else {State.gold+=Math.Ceiling(Factor(56)*State.averagePerMinute);Data.auctionCoinExchangeCount++;CheckGrowth();}
            Changed?.Invoke();return true;
        }
        public void ClaimAuctionCoinAd(){EnsureActivitiesData();Data.auctionCoin+=2000+Data.adEnterExchangeCount*1000;Data.adEnterExchangeCount++;Changed?.Invoke();}
    }
}

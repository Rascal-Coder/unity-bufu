using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Bufu.Rebuild;
using Bufu.Rebuild.UI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Original ChallengeMain/AuctionDataManager/AuctionPlayPanel economy and exported-prefab regressions.</summary>
public static class ActivityChecks {
    const long Epoch=1000000,TicketInterval=600000;
    static bool Near(double a,double b,double epsilon=.00001)=>Math.Abs(a-b)<=epsilon;
    public static void Verify(Action<bool,string> check) {
        VerifyMigration(check);VerifyTickets(check);VerifyChallenge(check);
        VerifyAuction(check);VerifyExchange(check);VerifyPrefabs(check);
        VerifyChallengeHelperParameters(check);
    }
    static void VerifyChallengeHelperParameters(Action<bool,string> check) {
        var method=typeof(GameController).GetMethod("ActivityHelperParameter",BindingFlags.Static|BindingFlags.NonPublic);
        check(method!=null,"challenge helpers: original prefab parameter API is available for regression checks");
        var path=Path.Combine(Application.dataPath,"../Tools/ParityAssets/prefab-actor-parameters.json");
        check(File.Exists(path),"challenge helpers: recovered actor parameter evidence is present");
        var evidence=(Dictionary<string,object>)MiniJson.Parse(File.ReadAllText(path));var covered=new HashSet<int>();
        string[] fields={"baseMoveSpeed","reachThreshold","coinMinDistance","waitAfterThrow","sleepCheckInterval","sleepProbability","sleepDuration","scatterRadius","halandradius"};
        foreach(Dictionary<string,object> prefab in evidence.Arr("prefabs")) {
            int role=prefab.I("role_type",-1);if(role<0)continue;
            var component=prefab.Arr("components").Cast<Dictionary<string,object>>().Single(c=>c.Str("class")=="Role");
            var original=component.Obj("fields");
            check(fields.All(field=>Near((float)method.Invoke(null,new object[]{role,field}),original.F(field))),
                "challenge helpers: role "+role+" matches every recovered RoleSpine movement, interaction, wait and sleep field");
            covered.Add(role);
        }
        check(covered.SetEquals(Enumerable.Range(0,11)),"challenge helpers: original actor parameter evidence covers all eleven role types including the magician exception");
    }
    static GameModel AuctionModel() {
        var model=new GameModel();model.EnsureActivitiesData();
        model.State.mission=4;model.State.gold=0;model.Data.auctionCoin=5000;
        model.Data.ticketCount=3;model.Data.lastTicketRecoverTime=Epoch;
        return model;
    }
    static void VerifyMigration(Action<bool,string> check) {
        var model=new GameModel();model.Data.activitiesVersion=0;model.Data.auctionCoin=1200;
        model.Data.ticketCount=0;model.Data.challengeLevel=0;
        model.Data.auctionCoinExchangeCount=model.Data.auctionDiamondExchangeCount=0;
        model.EnsureActivitiesData();
        check(model.Data.activitiesVersion==1&&model.Data.auctionCoin==2000&&model.Data.ticketCount==3&&model.Data.challengeLevel==1,
            "activities: old save migration installs the original global 2000 auction coins and three tickets");
        check(model.Data.auctionCoinExchangeCount==1&&model.Data.auctionDiamondExchangeCount==1&&model.Data.lastTicketRecoverTime>0,
            "activities: migration initializes both first-exchange counters and a ticket clock");
        model.Data.auctionCoin=777;model.Data.ticketCount=1;model.Data.challengeLevel=2;
        model.Data.lastTicketRecoverTime=Epoch;model.Data.auctionCoinExchangeCount=3;
        model.EnsureActivitiesData();
        check(model.Data.auctionCoin==777&&model.Data.ticketCount==1&&model.Data.challengeLevel==2&&model.Data.lastTicketRecoverTime==Epoch&&model.Data.auctionCoinExchangeCount==3,
            "activities: reopening an initialized save does not grant the migration gift twice");
        var restored=new GameModel(JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(model.Data)));restored.EnsureActivitiesData();
        check(restored.Data.auctionCoin==777&&restored.Data.ticketCount==1&&restored.Data.activitiesVersion==1,
            "activities: the one-time migration marker and spent global resources survive JSON restore");
        restored.Data.diceUnlocked=true;restored.SwitchWorld(2,1000);restored.EnsureActivitiesData();
        check(restored.Data.auctionCoin==777&&restored.Data.ticketCount==1,
            "activities: entering another world shares the same tickets and auction wallet without a second gift");
    }
    static void VerifyTickets(Action<bool,string> check) {
        var model=AuctionModel();
        check(model.StartLocalAuction(1,Epoch,11)&&model.Data.ticketCount==2&&model.Data.lastTicketRecoverTime==Epoch,
            "auction: the first ticket starts its ten-minute recovery clock");
        model.LocalAuction.Finished=true;
        check(model.StartLocalAuction(1,Epoch+TicketInterval/2,12)&&model.Data.ticketCount==1&&model.Data.lastTicketRecoverTime==Epoch,
            "auction: spending a second ticket preserves the already running recovery interval");
        check(model.RecoverAuctionTickets(Epoch+TicketInterval-1)==1,
            "auction: a ticket does not recover one millisecond before ten minutes");
        check(model.RecoverAuctionTickets(Epoch+TicketInterval)==2&&model.Data.lastTicketRecoverTime==Epoch+TicketInterval,
            "auction: exactly ten minutes recovers one ticket and moves the recovery checkpoint");
        check(model.RecoverAuctionTickets(Epoch+2*TicketInterval)==3&&model.RecoverAuctionTickets(Epoch+20*TicketInterval)==3,
            "auction: accumulated ticket recovery caps at three");
        model.Data.ticketCount=0;model.Data.lastTicketRecoverTime=Epoch;
        check(model.RecoverAuctionTickets(Epoch+TicketInterval*3/2)==1&&model.Data.lastTicketRecoverTime==Epoch+TicketInterval*3/2,
            "auction: original recovery discards the partial interval when it stores the new checkpoint");
        check(model.RecoverAuctionTickets(Epoch+2*TicketInterval)==1,
            "auction: the discarded half interval cannot be recovered again");
        model.LocalAuction.Finished=true;model.Data.ticketCount=0;model.Data.lastTicketRecoverTime=Epoch;
        check(!model.StartLocalAuction(1,Epoch,13)&&model.StartLocalAuction(1,Epoch,13,true)&&model.Data.ticketCount==0,
            "auction: an empty ticket balance requires an ad-backed entry and does not create a ticket");
    }
    static GameModel ChallengeModel(int level=1) {
        var model=new GameModel();model.EnsureActivitiesData();model.Data.challengeLevel=level;
        model.State.mission=10;model.State.gold=model.CurrentActivityChallenge.costMoney;
        return model;
    }
    static void VerifyChallenge(Action<bool,string> check) {
        var model=ChallengeModel();model.State.gold=11999;
        check(!model.StartLocalChallenge(new Dictionary<int,int>())&&model.State.gold==11999,
            "challenge: the configured 12000 entry fee cannot be paid with an insufficient wallet");
        model.State.gold=12000;model.State.lifetime=77;
        check(model.StartLocalChallenge(new Dictionary<int,int>())&&model.State.gold==0&&model.State.lifetime==77,
            "challenge: entry spends the real wallet once without rewriting lifetime income");
        check(!model.StartLocalChallenge(new Dictionary<int,int>())&&model.State.gold==0,
            "challenge: an active match cannot charge a duplicate entry fee");
        model.TickLocalChallenge(19.5f);
        check(!model.LocalChallenge.SkillPending&&Near(model.LocalChallenge.Remaining,40.5),
            "challenge: the first skill choice does not open before twenty seconds");
        model.TickLocalChallenge(.5f);float remaining=model.LocalChallenge.Remaining;
        model.TickLocalChallenge(100);
        check(model.LocalChallenge.SkillPending&&Near(remaining,40)&&Near(model.LocalChallenge.Remaining,40)&&!model.AddLocalChallengeCoin(3,true),
            "challenge: the twenty-second skill dialog pauses time and rejects incoming match coins");
        check(model.ChooseLocalChallengeSkill()&&Near(model.LocalChallenge.Multiplier,1.2)&&!model.ChooseLocalChallengeSkill(),
            "challenge: a skill choice grants one twenty-percent multiplier and cannot be selected twice");
        model.TickLocalChallenge(20);model.TickLocalChallenge(100);
        check(model.LocalChallenge.SkillPending&&Near(model.LocalChallenge.Remaining,20)&&model.ChooseLocalChallengeSkill()&&Near(model.LocalChallenge.Multiplier,1.44),
            "challenge: the second choice opens at forty seconds, pauses, and compounds the multiplier");
        model.TickLocalChallenge(20);
        double diamonds=model.Data.diamonds;
        check(model.LocalChallenge.Finished&&!model.LocalChallenge.Won&&model.ClaimLocalChallenge()==null&&model.State.gold==0&&model.Data.diamonds==diamonds&&model.Data.challengeLevel==1,
            "challenge: a sixty-second loss grants no prize and does not advance the challenge level");
        check(model.ReviveLocalChallenge()&&model.LocalChallenge.Remaining==20&&!model.ReviveLocalChallenge(),
            "challenge: a failed run permits one twenty-second revival");

        model=ChallengeModel();model.StartLocalChallenge(new Dictionary<int,int>());
        check(model.LocalChallengeUpgradeCost(0)==50&&model.LocalChallengeUpgradeCost(2)==80,
            "challenge: silver and gold upgrades use the original first base costs 50 and 80");
        model.LocalChallenge.Upgrades[0]=10;
        check(Near(model.LocalChallengeUpgradeCost(0),2883.251953125),
            "challenge: upgrade level ten remains in the first inclusive cost segment");
        model.LocalChallenge.Upgrades[0]=11;
        check(Near(model.LocalChallengeUpgradeCost(0),58245.129449,.1),
            "challenge: level eleven switches to the second growth factor while retaining the first base cost");
        model.LocalChallenge.Upgrades[0]=0;model.LocalChallenge.Collected=100;
        check(model.BuyLocalChallengeUpgrade(0)&&model.LocalChallenge.Collected==50&&model.LocalChallengeCoinCount(2)==11&&model.State.gold==0,
            "challenge: upgrades spend match coins and add one silver coin without spending the main wallet");

        model=ChallengeModel();model.State.averagePerMinute=123;model.StartLocalChallenge(new Dictionary<int,int>());
        model.AddLocalChallengeCoin(3,false);
        check(model.LocalChallenge.Collected==0,"challenge: reverse faces do not contribute match income");
        for(int i=0;i<3;i++)model.AddLocalChallengeCoin(3,true);
        check(model.LocalChallenge.Won&&model.Data.challengeLevel==2&&model.State.gold==0&&model.LocalChallenge.Receipt.Single().Value==1230,
            "challenge: three original gold heads meet the 600 goal and resolve the configured ten-minute prize before claim");
        var receipt=model.ClaimLocalChallenge();
        check(receipt!=null&&model.State.gold==1230&&model.ClaimLocalChallenge(2)==null&&model.State.gold==1230,
            "challenge: the resolved gold prize is granted exactly once");
        model=ChallengeModel(3);model.StartLocalChallenge(new Dictionary<int,int>());diamonds=model.Data.diamonds;
        model.AddLocalChallengeCoin(3,true,1000);
        check(model.LocalChallenge.Won&&model.LocalChallenge.Receipt.Single().Key==2&&model.LocalChallenge.Receipt.Single().Value==10,
            "challenge: challenge three resolves its fixed ten-diamond table prize rather than an income multiplier");
        check(model.ClaimLocalChallenge(2)!=null&&model.Data.diamonds==diamonds+20&&model.ClaimLocalChallenge()==null,
            "challenge: the fixed diamond receipt can double once and cannot be claimed again");
    }
    static void VerifyAuction(Action<bool,string> check) {
        var model=AuctionModel();check(model.StartLocalAuction(1,Epoch,41),"auction: an unlocked funded house opens using one ticket");
        var run=model.LocalAuction;
        check(run.ItemIds.Length==6&&run.ItemValues.Length==6&&run.Rarities.Length==6&&Near(run.ItemValues.Sum(),run.FinalValue),
            "auction: the six original item slots partition the full resolved container value");
        check(run.GivenUp.Length==4,"auction: the original prefab's four logical NPC slots are retained despite three visible NPC portraits");
        check(run.ItemIds.Select((id,i)=>model.ActivityAuctionItems.Any(x=>x.id==id&&x.rarity.Contains(run.Rarities[i]))).All(x=>x),
            "auction: each generated item belongs to its sampled rarity pool");
        double price=run.Price;model.TickLocalAuction(.5f);
        check(!model.BidLocalAuction(0)&&run.BidCount==0&&run.Price==price&&!run.Finished,
            "auction: the first introduction phase blocks both player and NPC bidding");
        for(int frame=0;frame<1000&&run.Phase<2;frame++)model.TickLocalAuction(.01f);
        check(run.Phase==2&&run.Elapsed>4.4f&&run.Elapsed<4.7f&&run.BidCount==0,
            "auction: original double timer increments open bidding near 4.5 seconds rather than a fixed six-second delay");
        model.Data.auctionCoin=0;
        check(!model.BidLocalAuction(0)&&run.Price==price&&run.LastBidder!=-1,
            "auction: an unaffordable bid leaves the price and bidder unchanged");
        model.Data.auctionCoin=5000;
        check(model.BidLocalAuction(0)&&model.Data.auctionCoin==5000&&run.LastBidder==-1&&!model.BidLocalAuction(1),
            "auction: an affordable bid reserves the leading offer without immediate debit or consecutive self-bids");

        foreach(bool doubled in new[]{false,true}) {
            model=AuctionModel();model.StartLocalAuction(1,Epoch,42);run=model.LocalAuction;
            // Isolate settlement using a valid leading-player state; the production timer still closes it.
            run.Phase=2;run.Elapsed=6;run.LastBidder=-1;run.Price=200;run.FinalValue=500;run.HostRemaining=1.05f;
            for(int i=0;i<run.GivenUp.Length;i++)run.GivenUp[i]=true;
            model.TickLocalAuction(.1f);
            check(run.Finished&&run.PlayerWon,"auction: the final host countdown closes the leading player's auction");
            double expected=doubled?800:300;
            check(Near(model.ClaimLocalAuction(doubled),expected)&&Near(model.Data.auctionCoin,5000+expected),
                "auction: "+(doubled?"double sale adds 1000 minus the single 200 bid":"normal sale adds 500 minus the single 200 bid"));
            check(model.ClaimLocalAuction(!doubled)==0&&Near(model.Data.auctionCoin,5000+expected),
                "auction: a settled receipt cannot be paid again through the other sale button");
        }
        model=AuctionModel();model.StartLocalAuction(1,Epoch,43);run=model.LocalAuction;
        run.Phase=2;run.Elapsed=6;run.LastBidder=0;run.Price=400;run.FinalValue=200;run.HostRemaining=1.05f;
        run.RaiseMode=true;run.RaiseStages=new[]{1f,1.5f,2f};
        for(int i=0;i<run.GivenUp.Length;i++)run.GivenUp[i]=true;
        model.TickLocalAuction(.1f);
        check(run.Finished&&!run.PlayerWon&&Near(run.RaiseIncome,160,.0001)&&Near(model.ClaimLocalAuction(),160,.0001),
            "auction: a losing raised-price auction uses its resolved 200 value and inclusive stage to pay the original 160 bonus");
    }
    static void VerifyExchange(Action<bool,string> check) {
        var model=AuctionModel();model.State.mission=6;model.State.averagePerMinute=100;
        model.State.gold=100;model.State.lifetime=314;model.Data.auctionCoin=10000;
        check(model.AuctionExchangeCost(false)==1500&&model.ExchangeAuctionCoins(false)&&model.Data.auctionCoin==8500&&model.State.gold==900&&model.Data.auctionCoinExchangeCount==2,
            "auction exchange: first gold exchange spends 1500 auction coins and grants eight income minutes");
        check(model.State.lifetime==314&&model.State.averagePerMinute==100&&model.AuctionExchangeCost(false)==1875,
            "auction exchange: AddGoldInternal changes only the wallet while the next price grows by twenty-five percent");
        check(model.ExchangeAuctionCoins(false)&&model.Data.auctionCoin==6625&&model.State.gold==1700&&Near(model.AuctionExchangeCost(false),2343.75),
            "auction exchange: a second exchange uses its own escalating counter without rounding the third price");
        model.Data.auctionCoin=20000;double diamonds=model.Data.diamonds;
        check(model.AuctionExchangeCost(true)==5000&&model.ExchangeAuctionCoins(true)&&model.Data.auctionCoin==15000&&model.Data.diamonds==diamonds+5&&model.AuctionExchangeCost(true)==7500,
            "auction exchange: diamond exchange grants five diamonds and grows its independent cost by fifty percent");
        model.Data.auctionCoin=0;int count=model.Data.auctionDiamondExchangeCount;
        check(!model.ExchangeAuctionCoins(true)&&model.Data.auctionCoin==0&&model.Data.auctionDiamondExchangeCount==count,
            "auction exchange: insufficient auction funds cannot grant diamonds or advance the counter");
        model=AuctionModel();model.Data.auctionCoin=0;model.ClaimAuctionCoinAd();model.ClaimAuctionCoinAd();
        check(model.Data.auctionCoin==5000&&model.Data.adEnterExchangeCount==2,
            "auction exchange: successive ad receipts grant the original 2000 then 3000 auction coins");
    }
    static void VerifyPrefabs(Action<bool,string> check) {
        var host=new GameObject("Activity Original Prefab Checks",typeof(RectTransform));
        try {
            void Prefab(string resource,string cls,params string[] fields) {
                check(Resources.Load<TextAsset>("UI/Prefabs/"+resource),"activity resources: original "+resource+" JSON is packaged");
                var view=UiPrefab.Instantiate(resource,host.transform);
                check(view.Root&&fields.All(field=>view.Ref(cls,field)),"activity resources: "+resource+" preserves its original bound nodes");
            }
            Prefab("Challenge/ChallengePanel","ChallengePanel","playBtn","helperCont","auctionEnterBtn");
            Prefab("Challenge/ChallengeMain","ChallengeMain","victoryBtn","victoryAd","failBtn","adFailBtn","upgradeCont","skillCont");
            Prefab("Challenge/helperSelectInfo","ChallengeHelperInfo","addBtn","reduceBtn","sumTxt");
            Prefab("Challenge/challengeUpgradeItem","ChallengeUpgradeItem","btn","upgradeMoneyTmp","lvTmp");
            Prefab("Challenge/selectSkill","SelectSkill","selectBtn","name","introduce");
            Prefab("Auction/auctionSeclect","AuctionSeclect","enterBtn","ticketTxt","enterExchangeBtn","auctionExchangeObj");
            Prefab("Auction/auctionPlayPanel","AuctionPlayPanel","freeSoldBtn","adSoldBtn","enterEndBtn","continueBtn");
            Prefab("Auction/selectSelectItem","AuctionSelectItem","icon","tip","suo");
            Prefab("Auction/auctionItem","AuctionItem","icon","nameTxt","priceTxt");
            check(Resources.Load<TextAsset>("UI/Prefabs/Auction/auctionItemBox"),"activity resources: original auction item box JSON is packaged");
            var box=UiPrefab.Instantiate("Auction/auctionItemBox",host.transform);
            check(box.Root.GetComponent<Button>(),"activity resources: the original one-node item box remains clickable");
            var auction=UiPrefab.Instantiate("Auction/auctionPlayPanel",host.transform);
            check(auction.Refs("AuctionPlayPanel","itemPos").Length==6&&auction.Refs("AuctionPlayPanel","itemBoxPos").Length==6&&auction.Refs("AuctionPlayPanel","npcPos").Length==4,
                "activity resources: serialized original arrays independently confirm six item slots and four logical NPC positions");
        } finally {UnityEngine.Object.DestroyImmediate(host);}
    }
}

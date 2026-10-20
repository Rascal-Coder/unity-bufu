using System;
using System.Collections.Generic;
using System.Linq;
using Bufu.Rebuild.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    public sealed partial class GameController {
        UiView activityView,activityArena,activityAuctionSelection,activityAuctionView;
        ActivityAuctionPager activityAuctionPager;
        readonly Dictionary<int,int> activityTeam=new Dictionary<int,int>();
        readonly List<Action> activityTeamRefresh=new List<Action>();
        readonly List<ActivityCoinVisual> activityCoins=new List<ActivityCoinVisual>();
        readonly List<ActivityHelperVisual> activityHelpers=new List<ActivityHelperVisual>();
        readonly List<Button> activityUpgradeButtons=new List<Button>();
        readonly List<UiView> activityUpgradeCards=new List<UiView>();
        int activitySelectedHouse=1;bool activityResultShown;float activityRefreshTimer;
        sealed class ActivityCoinVisual {
            public RectTransform Rect;public VatGraphic Vat;public int Type,RoleId,FlipFrame;public float Flight,Duration=.8f,Delay,FlipTime,RewardScale=1;public Vector2 Start,Target;public bool Heads,Echo;public Action<bool> Landed;public bool Busy=>Flight>0;
        }
        sealed class ActivityHelperVisual {
            public RectTransform Rect;public VatGraphic Vat;public int Id;public float Wait;public ActivityCoinVisual Target;public string Animation;
        }
        static readonly string[] ActivityHouseNames={"Beginner Auction","Bronze Auction","Silver Auction","Gold Auction","Platinum Auction","Black Gold Auction"};
        static readonly string[] ActivityItemNames={"Old towel","Plastic cup","Electric fan","Perfume","Toy car","Trainers","Wireless earbuds","Handheld console","Coffee maker","Suitcase","Tablet","Designer bag","Camera","Laptop","Speakers","Luxury watch","Gold necklace","Antique vase","Limited trainers","Painting","Diamond ring","Gold pocket watch","Stamp collection","Bronze antique","Rare gemstone","Broken television","Old chair","Record player","Safe","Mystery crate"};
        static readonly Dictionary<string,string> ActivityArtLabels=new Dictionary<string,string>{
            {"challenge_zi_xianshitiaozhan","Timed Challenge"},{"challenge_zi_jiangli","Reward"},{"challenge_zi_tiaozhan","Challenge"},
            {"challenge_zi_paimaichang","Auction House"},{"challenge_zi_chujichang","Beginner Auction"},{"challenge_zi_paimai","Enter Auction"},
            {"challenge_zi_bangshouzudui","Choose Helpers"},{"challenge_zi_yijianzudui","Auto Team"},{"challenge_zi_zhunbeihaola","Ready"},
            {"challenge_pic_title_huobiduihuan","Currency Exchange"},{"challenge_zi_duihuan","Exchange"},{"challenge_zi_taijiajieduan","Raise Stage"},
            {"challenge_zi_zongjia","Total Value"},{"challenge_zi_yingli","Profit"},{"challenge_zi_maichu","Sell"},{"challenge_zi_shuangbeimaichu","Sell x2"},
            {"challenge_zi_jixu","Continue"},{"challenge_zi_fanhui","Back"},{"challenge_zi_zailaiyiju","Play Again"},{"ty_zi_fuhuo","Revive"}};
        static readonly Dictionary<string,string> ActivityCopy=new Dictionary<string,string>{
            {"门票:","Tickets:"},{"普通","Normal"},{"激烈","Intense"},{"白热化","Frenzied"},{"投入","Cost"},{"预计获得","Receive"},{"后获得  1","until +1 ticket"},
            {"集装箱<color=#20A51A>盈利</color>:","Container profit:"},{"抬价<color=#20A51A>盈利</color>:","Raise income:"},{"事件<color=#20A51A>盈利</color>:","Event income:"},{"其他<color=#20A51A>盈利</color>:","Other income:"},{"总盈利:","Total profit:"}};
        TMP_Text ActivityText(Transform parent,string name,string value,float size=36) {
            var rect=NewRect(name,parent);Stretch(rect);var text=rect.gameObject.AddComponent<TextMeshProUGUI>();text.font=UiAssets.Font();
            text.fontSize=size;text.enableAutoSizing=true;text.fontSizeMin=16;text.fontSizeMax=size;text.alignment=TextAlignmentOptions.Center;text.color=FortuneInk;text.raycastTarget=false;Txt(text,value);return text;
        }
        void ActivityImageCaption(Image image,string caption) {
            string word=caption=="Timed Challenge"?"timed-challenge":caption=="Auction House"?"auction-house":null;
            if(word!=null&&WordArt(word)) {
                image.sprite=WordArt(word);image.type=Image.Type.Simple;image.preserveAspect=true;image.enabled=true;image.raycastTarget=false;
                var previous=image.transform.Find("English Art Label");if(previous)previous.gameObject.SetActive(false);return;
            }
            image.enabled=false;var child=image.transform.Find("English Art Label");
            var text=child?child.GetComponent<TMP_Text>():ActivityText(image.transform,"English Art Label",caption,42);Txt(text,caption);
        }
        void ActivityArt(UiView view) {
            foreach(var image in view.Root.GetComponentsInChildren<Image>(true))if(image.sprite) {
                string key=image.sprite.name;
                if(ActivityArtLabels.TryGetValue(key,out var label))ActivityImageCaption(image,label);
                else if(key.StartsWith("challenge_pic_title_")&&int.TryParse(key.Substring("challenge_pic_title_".Length),out int room)&&room>=1&&room<=6)ActivityImageCaption(image,ActivityHouseNames[room-1]);
            }
            foreach(var text in view.Root.GetComponentsInChildren<TMP_Text>(true))if(text.text.Any(c=>c>='\u4e00'&&c<='\u9fff')) {
                string translated=ActivityCopy.TryGetValue(text.text.Trim(),out var copy)?copy:L10n.Translate(text.text);Txt(text,translated.Any(c=>c>='\u4e00'&&c<='\u9fff')?"":translated);
            }
        }
        void ActivityButton(Button button,string label,Action action) {IntegrationButtonLabel(button,label);Click(button,action);}
        Button NewActivityButton(Transform parent,string name,string label,Vector2 position,Vector2 size,Action action) {
            var rect=NewRect(name,parent);rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.anchoredPosition=position;rect.sizeDelta=size;
            var image=rect.gameObject.AddComponent<Image>();var button=rect.gameObject.AddComponent<Button>();button.targetGraphic=image;ActivityButton(button,label,action);return button;
        }
        void SaveActivities(){if(!VerificationMode)SaveStore.Save(Model);}
        void EnsureActivities() {
            Model.EnsureActivitiesData();
            if(activityView!=null&&activityView.Root)return;
            const string cls="ChallengePanel";
            activityView=OpenTabPanel("Challenge/ChallengePanel");ActivityArt(activityView);var view=activityView;
            On(view.Ref(cls,"LevelGrid"),true);On(view.Node("challengeSelect"),true);On(view.Ref(cls,"helperSelect"),false);On(view.Ref(cls,"auctionEnter"),true);
            ActivityButton(view.Ref<Button>(cls,"challengeBtn"),"Challenge",()=>{
                if(Model.State.gold<Model.CurrentActivityChallenge.costMoney){ShowToast("Not enough gold to enter this challenge.");return;}
                On(view.Ref(cls,"LevelGrid"),false);On(view.Ref(cls,"helperSelect"),true);RefreshActivityTeam();
            });
            ActivityButton(view.Ref<Button>(cls,"playBtn"),"Ready",StartActivityChallenge);
            ActivityButton(view.Ref<Button>(cls,"zuduiBtn"),"Auto Team",()=>{activityTeam.Clear();foreach(var pair in Model.AutoActivityTeam())activityTeam[pair.Key]=pair.Value;RefreshActivityTeam();});
            Click(view.Ref<Button>(cls,"selectBackBtn"),()=>{On(view.Ref(cls,"LevelGrid"),true);On(view.Ref(cls,"helperSelect"),false);});
            ActivityButton(view.Ref<Button>(cls,"auctionEnterBtn"),"Auction",ShowActivityAuctionSelection);
            var parent=view.Ref<RectTransform>(cls,"helperCont");activityTeamRefresh.Clear();
            foreach(var role in Model.helperRoles.OrderBy(x=>x.id)) {
                int id=role.id;var card=UiPrefab.Instantiate("Challenge/helperSelectInfo",parent);ActivityArt(card);
                Txt(card.Ref<TMP_Text>("ChallengeHelperInfo","helperNameText"),role.helperName);
                var icon=card.Ref<Image>("ChallengeHelperInfo","helperIcon");var sprite=Sp(HelperIcons[id]);if(sprite)icon.sprite=sprite;
                Action<int> adjust=delta=>{
                    int count=activityTeam.TryGetValue(id,out var current)?current:0,total=activityTeam.Values.Sum();
                    if(delta>0&&(total>=Model.CurrentActivityChallenge.helperNum||count>=Model.ActivityHelperCap(id)))return;
                    activityTeam[id]=Mathf.Clamp(count+delta,0,Model.ActivityHelperCap(id));RefreshActivityTeam();
                };
                Click(card.Ref<Button>("ChallengeHelperInfo","addBtn"),()=>adjust(1));Click(card.Ref<Button>("ChallengeHelperInfo","reduceBtn"),()=>adjust(-1));
                activityTeamRefresh.Add(()=>{
                    int cap=Model.ActivityHelperCap(id),count=activityTeam.TryGetValue(id,out var value)?Math.Min(value,cap):0;activityTeam[id]=count;
                    Txt(card.Ref<TMP_Text>("ChallengeHelperInfo","sumTxt"),count+"/"+cap);Txt(card.Ref<TMP_Text>("ChallengeHelperInfo","levelText"),"Lv."+Model.GlobalHelperLevel(id));
                    On(card.Ref("ChallengeHelperInfo","lockMask"),cap<=0);On(card.Ref("ChallengeHelperInfo","xianshi"),cap>0);
                });
            }
            Watch(view.Root,RefreshActivitiesOverview);RefreshActivitiesOverview();SaveActivities();
        }
        void RefreshActivityTeam(){foreach(var refresh in activityTeamRefresh)refresh();if(activityView!=null&&activityView.Root)Txt(activityView.Ref<TMP_Text>("ChallengePanel","playCount"),activityTeam.Values.Sum()+"/"+Model.CurrentActivityChallenge.helperNum);}
        void RefreshActivitiesOverview() {
            if(activityView==null||!activityView.Root)return;var view=activityView;const string cls="ChallengePanel";var config=Model.CurrentActivityChallenge;
            Txt(view.Ref<TMP_Text>(cls,"title"),config.id.ToString());Txt(view.Ref<TMP_Text>(cls,"needCoin"),GameModel.Format(config.costMoney));
            Txt(view.Ref<TMP_Text>(cls,"rewardTxt"),GameModel.Format(config.rewardType==1?Model.State.averagePerMinute*config.rewardNum:config.rewardNum));
            var icon=RewardIcon(config.rewardType);if(icon)view.Ref<Image>(cls,"rewardIcon").sprite=icon;
            Txt(view.Ref<TMP_Text>(cls,"ticketTxt"),Model.RecoverAuctionTickets(GameModel.DiceLocalMilliseconds).ToString());Txt(view.Ref<TMP_Text>(cls,"coinTxt"),GameModel.Format(Model.Data.auctionCoin));
            var house=Model.ActivityAuctionHouses.LastOrDefault(x=>Model.MainLevel>=x.conditionLevel)??Model.ActivityAuctionHouses[0];
            Txt(view.Ref<TMP_Text>(cls,"auctionrewardTxt"),house.itemValueMin+" - "+house.itemValueMax);ActivityImageCaption(view.Ref<Image>(cls,"auctionLeveImg"),ActivityHouseNames[Mathf.Clamp(house.auctionHouseID-1,0,5)]);
            Txt(view.Nodes[14].GetComponent<TMP_Text>(),"Time limit: "+config.limitTIme+"s");RefreshActivityTeam();
        }
        void StartActivityChallenge() {
            if(!Model.StartLocalChallenge(activityTeam)){ShowToast("Not enough gold, or the selected team is invalid.");return;}
            SaveActivities();activityArena=OpenPopup("Challenge/ChallengeMain");ActivityArt(activityArena);activityResultShown=false;
            if(mainScene!=null&&mainScene.Root)mainScene.Root.SetActive(false);
            const string cls="ChallengeMain";var arena=activityArena;
            FitActivityChallengeHud(arena);
            foreach(string field in new[]{"victoryPanel","failPanel","selectSkillPanel"})On(arena.Ref(cls,field),false);
            Click(arena.Ref<Button>(cls,"backBtn"),()=>{Model.AbandonLocalChallenge();CloseActivityChallenge();});
            ActivityButton(arena.Ref<Button>(cls,"victoryBtn"),"Collect",()=>ClaimActivityChallenge(1));
            ActivityButton(arena.Ref<Button>(cls,"victoryAd"),"Ad: Collect x2",()=>ShowOriginalAd(()=>ClaimActivityChallenge(2)));
            ActivityButton(arena.Ref<Button>(cls,"failBtn"),"Back",CloseActivityChallenge);
            ActivityButton(arena.Ref<Button>(cls,"adFailBtn"),"Ad: Revive",()=>ShowOriginalAd(()=>{if(Model.ReviveLocalChallenge()){activityResultShown=false;On(arena.Ref(cls,"failPanel"),false);}}));
            activityCoins.Clear();activityHelpers.Clear();activityUpgradeButtons.Clear();activityUpgradeCards.Clear();Canvas.ForceUpdateCanvases();SyncActivityCoins();
            var rolePool=arena.Ref<RectTransform>(cls,"rolePool");
            foreach(var pair in activityTeam)for(int i=0;i<pair.Value;i++) {
                var rect=NewRect("Challenge Helper "+pair.Key+" "+i,rolePool);rect.sizeDelta=new Vector2(200,220);rect.anchoredPosition=RandomActivityPosition();
                var vat=VatGraphic.AddTo(rect.gameObject);vat.raycastTarget=false;vat.PixelsPerUnit=144;vat.Load(HelperSkills.RoleVat(pair.Key,ActivityHelperAwake(pair.Key)));vat.Play("idle_front",true);
                activityHelpers.Add(new ActivityHelperVisual{Rect=rect,Vat=vat,Id=pair.Key,Wait=UnityEngine.Random.Range(.1f,.6f)});
            }
            InitializeActivitySkills();
            string[] upgrades={"More Silver","Silver Value","More Gold","Gold Value"};var upgradeParent=arena.Ref<RectTransform>(cls,"upgradeCont");
            for(int i=0;i<4;i++) {
                int kind=i;var card=UiPrefab.Instantiate("Challenge/challengeUpgradeItem",upgradeParent);ActivityArt(card);activityUpgradeCards.Add(card);
                var button=card.Ref<Button>("ChallengeUpgradeItem","btn");activityUpgradeButtons.Add(button);
                Txt(card.Ref<TMP_Text>("ChallengeUpgradeItem","nameTmp"),upgrades[i]);On(card.Ref("ChallengeUpgradeItem","button_upgrade1"),false);
                FitActivityUpgradeTitle(card);
                var icon=ItemIcon(i%2==0?0:1,i<2?(i%2==0?3:2):(i%2==0?4:3));if(icon)card.Ref<Image>("ChallengeUpgradeItem","itemIcon").sprite=icon;
                Click(button,()=>{if(Model.BuyLocalChallengeUpgrade(kind)){PlayUpgrade();SyncActivityCoins();}});
            }
            var skillParent=arena.Ref<RectTransform>(cls,"skillCont");
            for(int i=0;i<3;i++) {
                var card=UiPrefab.Instantiate("Challenge/selectSkill",skillParent);ActivityArt(card);Txt(card.Ref<TMP_Text>("SelectSkill","name"),"Skill "+(i+1));Txt(card.Ref<TMP_Text>("SelectSkill","introduce"),"Coin value +20%");
                Click(card.Ref<Button>("SelectSkill","selectBtn"),()=>{Model.ChooseLocalChallengeSkill();On(arena.Ref(cls,"selectSkillPanel"),false);});
            }
            RefreshActivityChallenge();
        }
        void FitActivityChallengeHud(UiView arena) {
            Canvas.ForceUpdateCanvases();
            var time=arena.Ref<TMP_Text>("ChallengeMain","time");var title=(RectTransform)time.transform.parent;
            title.GetComponent<Image>().enabled=false;
            var label=ActivityText(title,"English Time Label","Time left",34);label.color=Color.white;label.outlineColor=Color.black;label.outlineWidth=.15f;
            label.rectTransform.anchorMin=label.rectTransform.anchorMax=new Vector2(.5f,.5f);label.rectTransform.sizeDelta=new Vector2(215,71);label.rectTransform.anchoredPosition=new Vector2(-110,0);
            time.enableWordWrapping=false;time.enableAutoSizing=true;time.fontSizeMax=48;time.fontSizeMin=28;
            var corners=new Vector3[4];title.GetWorldCorners(corners);
            float top=arena.Rect.rect.yMax-32,maxY=corners.Max(p=>arena.Rect.InverseTransformPoint(p).y);
            var group=(RectTransform)title.parent;var position=group.anchoredPosition;position.y+=Mathf.Min(0,top-maxY);group.anchoredPosition=position;
            var back=arena.Ref<RectTransform>("ChallengeMain","backBtn");back.anchorMin=back.anchorMax=new Vector2(0,1);
            back.anchoredPosition=new Vector2(32+back.rect.width*back.pivot.x,-32-back.rect.height*(1-back.pivot.y));
        }
        void FitActivityUpgradeTitle(UiView card) {
            var name=card.Ref<TMP_Text>("ChallengeUpgradeItem","nameTmp");var group=(RectTransform)name.transform.parent;
            var layout=group.GetComponent<HorizontalLayoutGroup>();if(layout)layout.enabled=false;
            var fitter=group.GetComponent<ContentSizeFitter>();if(fitter)fitter.enabled=false;
            group.anchorMin=group.anchorMax=new Vector2(.5f,.5f);group.localRotation=Quaternion.identity;group.anchoredPosition=new Vector2(-150,87);group.sizeDelta=new Vector2(320,66);
            var rect=name.rectTransform;rect.anchorMin=rect.anchorMax=new Vector2(0,.5f);rect.pivot=new Vector2(0,.5f);rect.localRotation=Quaternion.identity;rect.anchoredPosition=Vector2.zero;rect.sizeDelta=new Vector2(266,66);
            name.enableWordWrapping=false;name.enableAutoSizing=true;name.fontSize=36;name.fontSizeMax=36;name.fontSizeMin=22;name.alignment=TextAlignmentOptions.MidlineLeft;
            var info=(RectTransform)group.Find("GameObject");if(info){info.anchorMin=info.anchorMax=new Vector2(1,.5f);info.pivot=new Vector2(.5f,.5f);info.localRotation=Quaternion.identity;info.anchoredPosition=new Vector2(-20,0);info.sizeDelta=new Vector2(40,40);}
            var icon=info?info.Find("informationIcon") as RectTransform:null;if(icon){icon.anchorMin=icon.anchorMax=icon.pivot=new Vector2(.5f,.5f);icon.anchoredPosition=Vector2.zero;icon.sizeDelta=new Vector2(36,32);}
        }
        Vector2 RandomActivityPosition() {
            var bounds=ActivitySceneBounds();return new Vector2(UnityEngine.Random.Range(bounds.xMin+80,bounds.xMax-80),UnityEngine.Random.Range(bounds.yMin+80,bounds.yMax-80));
        }
        Rect ActivitySceneBounds() {
            var scene=activityArena.Ref<RectTransform>("ChallengeMain","sceneBoxUI");var pool=activityArena.Ref<RectTransform>("ChallengeMain","coinPool");
            var corners=new Vector3[4];scene.GetWorldCorners(corners);Vector2 min=pool.InverseTransformPoint(corners[0]),max=pool.InverseTransformPoint(corners[2]);return Rect.MinMaxRect(min.x,min.y,max.x,max.y);
        }
        Vector2 ClampActivityPosition(Vector2 position,float padding=40) {var bounds=ActivitySceneBounds();return new Vector2(Mathf.Clamp(position.x,bounds.xMin+padding,bounds.xMax-padding),Mathf.Clamp(position.y,bounds.yMin+padding,bounds.yMax-padding));}
        void SyncActivityCoins() {
            var run=Model.LocalChallenge;var parent=activityArena.Ref<RectTransform>("ChallengeMain","coinPool");
            for(int type=1;type<=4;type++) {
                int count=activityCoins.Count(x=>x.Type==type),wanted=Model.LocalChallengeCoinCount(type);
                for(int i=count;i<wanted;i++) {
                    int quality=type;var rect=NewRect("Challenge Coin "+type+" "+i,parent);rect.sizeDelta=new Vector2(152,152);rect.anchoredPosition=RandomActivityPosition();
                    var image=rect.gameObject.AddComponent<Image>();image.color=Color.clear;var button=rect.gameObject.AddComponent<Button>();button.targetGraphic=image;
                    var visual=NewRect("Coin VAT",rect);Stretch(visual);var vat=VatGraphic.AddTo(visual.gameObject);vat.raycastTarget=false;vat.PixelsPerUnit=190;vat.Load("coin_"+CoinKinds[type-1]);vat.Pose("flip",1);
                    var coin=new ActivityCoinVisual{Type=quality,Rect=rect,Vat=vat,Heads=true};activityCoins.Add(coin);Click(button,()=>ThrowActivityCoin(coin));
                }
            }
        }
        bool ThrowActivityCoin(ActivityCoinVisual coin,int role=0,float rewardScale=1,bool stationary=false,float delay=0,Vector2? landing=null,bool instant=false) {
            var run=Model.LocalChallenge;if(run==null||run.Finished||run.SkillPending||coin.Busy)return false;
            coin.Start=coin.Rect.anchoredPosition;coin.Target=landing??(stationary?coin.Start:coin.Start+UnityEngine.Random.insideUnitCircle.normalized*UnityEngine.Random.Range(60f,120f));
            coin.Target=ClampActivityPosition(coin.Target);
            coin.RoleId=role;coin.RewardScale=Mathf.Max(0,rewardScale);coin.Duration=.8f;coin.Delay=Mathf.Max(0,delay);coin.FlipTime=0;coin.FlipFrame=0;
            if(role==6){int awake=ActivityHelperAwake(6);if(awake>0)coin.Duration=.4f;if(awake>1)instant=true;}
            coin.Flight=instant?0:coin.Duration;
            if(instant){ResolveActivityCoin(coin);return true;}
            coin.Vat.Pose("flip",coin.Delay>0&&coin.Heads?1:0);PlayCoinSound();return true;
        }
        void ResolveActivityCoin(ActivityCoinVisual coin) {
            coin.Flight=0;coin.Rect.anchoredPosition=coin.Target;coin.Heads=UnityEngine.Random.value<(coin.Type==4?.60f:.65f);coin.Vat.Pose("flip",coin.Heads?1:0);
            Model.AddLocalChallengeCoin(coin.Type,coin.Heads,coin.RewardScale);PlayHit();coin.Landed?.Invoke(coin.Heads);
            // CoinBase resets after OnFlipComplete and OnLanded, including a callback rethrow.
            coin.Flight=0;coin.RoleId=0;coin.RewardScale=1;coin.Vat.Pose("flip",coin.Heads?1:0);
        }
        void TickActivityChallenge(float dt) {
            var run=Model.LocalChallenge;if(run==null||activityArena==null||!activityArena.Root)return;
            if(dt<=0){RefreshActivityChallenge();return;}
            Model.TickLocalChallenge(dt);
            if(!run.Finished&&!run.SkillPending) {
                TickActivityHelpers(dt);
                foreach(var coin in activityCoins)if(coin.Busy) {
                    if(coin.Delay>0)coin.Delay=Mathf.Max(0,coin.Delay-dt);
                    if(coin.Delay<=0){coin.FlipTime+=dt;int frames=coin.Vat.Data.segments[0].totalFrames;if(coin.FlipTime>=.04f){coin.FlipTime=0;coin.FlipFrame=(coin.FlipFrame+1)%Math.Max(1,frames);}coin.Vat.Pose("flip",coin.FlipFrame/(float)Math.Max(1,frames-1));}
                    coin.Flight=Mathf.Max(0,coin.Flight-dt);float t=1-coin.Flight/Mathf.Max(.01f,coin.Duration);
                    coin.Rect.anchoredPosition=Vector2.Lerp(coin.Start,coin.Target,t)+Vector2.up*(Mathf.Sin(t*Mathf.PI)*500);
                    if(coin.Flight<=0)ResolveActivityCoin(coin);
                }
                TickActivitySkills(dt);
            }
            RefreshActivityChallenge();
        }
        void RefreshActivityChallenge() {
            var run=Model.LocalChallenge;if(run==null||activityArena==null||!activityArena.Root)return;var arena=activityArena;const string cls="ChallengeMain";
            Txt(arena.Ref<TMP_Text>(cls,"sum"),GameModel.Format(run.Collected)+"/"+GameModel.Format(run.Config.limitGoalFactor));Txt(arena.Ref<TMP_Text>(cls,"time"),TimeSpan.FromSeconds(Math.Ceiling(run.Remaining)).ToString(@"mm\:ss"));
            arena.Ref<Image>(cls,"progress").fillAmount=(float)(run.Collected/Math.Max(1,run.Config.limitGoalFactor));On(arena.Ref(cls,"selectSkillPanel"),run.SkillPending&&!run.Finished);
            string[] names={"More Silver","Silver Value","More Gold","Gold Value"};
            for(int i=0;i<activityUpgradeButtons.Count;i++) {
                var button=activityUpgradeButtons[i];var card=activityUpgradeCards[i];Txt(card.Ref<TMP_Text>("ChallengeUpgradeItem","lvTmp"),run.Upgrades[i].ToString());Txt(card.Ref<TMP_Text>("ChallengeUpgradeItem","upgradeMoneyTmp"),GameModel.Format(Model.LocalChallengeUpgradeCost(i)));button.interactable=!run.Finished&&!run.SkillPending&&run.Collected>=Model.LocalChallengeUpgradeCost(i);On(card.Ref("ChallengeUpgradeItem","canUpgrade"),button.interactable);
            }
            if(run.Finished&&!activityResultShown) {
                activityResultShown=true;On(arena.Ref(cls,run.Won?"victoryPanel":"failPanel"),true);On(arena.Ref(cls,"adFailBtn"),!run.Revived);
                if(run.Won){BuildActivityVictoryReceipt(arena,run.Receipt);SaveActivities();}
            }
        }
        void BuildActivityVictoryReceipt(UiView arena,List<KeyValuePair<int,double>> receipt) {
            var parent=arena.Ref<RectTransform>("ChallengeMain","victoryGrid");
            foreach(var reward in receipt){var rect=NewRect("Challenge Reward "+reward.Key,parent);rect.sizeDelta=new Vector2(250,100);string name=L10n.Translate(Model.ItemName(reward.Key));if(L10n.HasChinese(name))name=reward.Key==1?"Gold":reward.Key==2?"Diamonds":reward.Key==6?"Talent points":"Helper fragments";ActivityText(rect,"Reward",name+" x"+GameModel.Format(reward.Value),36);}
        }
        void ClaimActivityChallenge(int multiplier) {var receipt=Model.ClaimLocalChallenge(multiplier);if(receipt==null)return;SaveActivities();CloseActivityChallenge();PlayReward();FlyPromotionRewards(receipt.Select(x=>RewardFlyType(x.Key)).Where(x=>x>0).ToList());}
        void CloseActivityChallenge() {
            ClearActivitySkills();if(activityArena!=null)ClosePopup(activityArena);activityArena=null;activityCoins.Clear();activityHelpers.Clear();
            if(mainScene!=null&&mainScene.Root)mainScene.Root.SetActive(true);
            if(activityView!=null&&activityView.Root){On(activityView.Ref("ChallengePanel","LevelGrid"),true);On(activityView.Ref("ChallengePanel","helperSelect"),false);}
            RefreshActivitiesOverview();
        }

        void ShowActivityAuctionSelection() {
            if(activityAuctionSelection!=null&&activityAuctionSelection.Root){On(activityAuctionSelection.Root,true);return;}
            var view=OpenPopup("Auction/auctionSeclect");activityAuctionSelection=view;ActivityArt(view);const string cls="AuctionSeclect";
            On(view.Ref(cls,"auctionExchangeObj"),false);Click(view.Ref<Button>(cls,"backBtn"),()=>{ClosePopup(view);activityAuctionSelection=null;});Click(view.Ref<Button>(cls,"setupBtn"),ShowSetup);
            ActivityButton(view.Ref<Button>(cls,"enterBtn"),"Enter Auction",()=>StartActivityAuction(false));
            Click(view.Ref<Button>(cls,"enterExchangeBtn"),()=>{On(view.Ref(cls,"auctionExchangeObj"),true);RefreshActivityExchange();});
            Click(view.Ref<Button>(cls,"exchangeBackBtn"),()=>On(view.Ref(cls,"auctionExchangeObj"),false));
            Click(view.Ref<Button>(cls,"adEnterExchangeBtn"),()=>ShowOriginalAd(()=>{Model.ClaimAuctionCoinAd();SaveActivities();RefreshActivityAuctionSelection();}));
            ActivityButton(view.Ref<Button>("AuctionExchange","exchangebtn1"),"Exchange",()=>{if(Model.ExchangeAuctionCoins(false)){SaveActivities();PlayReward();}else ShowToast("Not enough auction coins.");RefreshActivityExchange();});
            ActivityButton(view.Ref<Button>("AuctionExchange","exchangebtn2"),"Exchange",()=>{if(Model.ExchangeAuctionCoins(true)){SaveActivities();PlayReward();}else ShowToast("Not enough auction coins, or growth level is too low.");RefreshActivityExchange();});
            var content=view.Ref<RectTransform>(cls,"cont");
            foreach(var house in Model.ActivityAuctionHouses) {
                int id=house.auctionHouseID;var card=UiPrefab.Instantiate("Auction/selectSelectItem",content);ActivityArt(card);
                var icon=card.Ref<Image>("AuctionSelectItem","icon");var sprite=SpAt("Assets/GameRes/icon/auction_title/challenge_pic_island_0"+id+"_scale2.png");if(sprite)icon.sprite=sprite;
                Txt(card.Ref<TMP_Text>("AuctionSelectItem","tip"),Model.MainLevel>=house.conditionLevel?ActivityHouseNames[Mathf.Clamp(id-1,0,5)]:"Requires growth level "+house.conditionLevel);On(card.Ref("AuctionSelectItem","suo"),Model.MainLevel<house.conditionLevel);
            }
            activityAuctionPager=view.Ref("AuctionSeclect","grid").AddComponent<ActivityAuctionPager>();
            activityAuctionPager.Initialize(content,Model.ActivityAuctionHouses.Length,1281,index=>{activitySelectedHouse=index+1;RefreshActivityAuctionSelection();});
            var left=view.Node("leftBtn").GetComponent<Button>();var right=view.Node("rightBtn").GetComponent<Button>();
            Click(left,()=>{activitySelectedHouse=Math.Max(1,activitySelectedHouse-1);RefreshActivityAuctionSelection();});Click(right,()=>{activitySelectedHouse=Math.Min(Model.ActivityAuctionHouses.Length,activitySelectedHouse+1);RefreshActivityAuctionSelection();});
            activitySelectedHouse=Math.Max(1,Model.ActivityAuctionHouses.Count(x=>Model.MainLevel>=x.conditionLevel));RefreshActivityAuctionSelection();
        }
        void RefreshActivityAuctionSelection() {
            if(activityAuctionSelection==null||!activityAuctionSelection.Root)return;var view=activityAuctionSelection;const string cls="AuctionSeclect";
            ActivityImageCaption(view.Ref<Image>(cls,"nameImg"),ActivityHouseNames[Mathf.Clamp(activitySelectedHouse-1,0,5)]);
            Txt(view.Ref<TMP_Text>(cls,"auctionCoinTxt"),GameModel.Format(Model.Data.auctionCoin));Txt(view.Ref<TMP_Text>(cls,"ticketTxt"),Model.RecoverAuctionTickets(GameModel.DiceLocalMilliseconds).ToString());
            Txt(view.Ref<TMP_Text>(cls,"needCoin"),GameModel.Format(Model.AuctionEntryValue(activitySelectedHouse)));On(view.Ref(cls,"adImg"),Model.Data.ticketCount<=0);
            long interval=Model.ActivityAuctionHouses[0].timeRecover*60000L,left=Math.Max(0,interval-(GameModel.DiceLocalMilliseconds-Model.Data.lastTicketRecoverTime));
            Txt(view.Ref<TMP_Text>(cls,"shijianTxt"),Model.Data.ticketCount<3?TimeSpan.FromMilliseconds(left).ToString(@"mm\:ss"):"");On(view.Ref(cls,"shijian"),Model.Data.ticketCount<3);
            var config=Model.ActivityAuctionHouses.First(x=>x.auctionHouseID==activitySelectedHouse);var button=view.Ref<Button>(cls,"enterBtn");button.interactable=Model.MainLevel>=config.conditionLevel&&Model.Data.auctionCoin>=Model.AuctionEntryValue(activitySelectedHouse);
            if(activityAuctionPager)activityAuctionPager.Select(activitySelectedHouse-1);
        }
        void RefreshActivityExchange() {
            if(activityAuctionSelection==null||!activityAuctionSelection.Root)return;var view=activityAuctionSelection;const string cls="AuctionExchange";
            Txt(view.Ref<TMP_Text>(cls,"coinTxt"),GameModel.Format(Model.Data.auctionCoin));Txt(view.Ref<TMP_Text>(cls,"coinCostTxt"),GameModel.Format(Model.AuctionExchangeCost(false)));Txt(view.Ref<TMP_Text>(cls,"diamondCostTxt"),GameModel.Format(Model.AuctionExchangeCost(true)));
            Txt(view.Ref<TMP_Text>(cls,"GetCoinTxt"),GameModel.Format(Math.Ceiling(Model.Factor(56)*Model.State.averagePerMinute)));Txt(view.Ref<TMP_Text>(cls,"GetDiamondTxt"),GameModel.Format(Model.Factor(57)));
            Txt(view.Ref<TMP_Text>(cls,"missionCoinNumTmp"),GameModel.Format(Model.State.gold));Txt(view.Ref<TMP_Text>(cls,"diamondNumTmp"),GameModel.Format(Model.Data.diamonds));
            bool unlocked=Model.MainLevel>=Model.Factor(49);On(view.Ref(cls,"exchangebtn2"),unlocked);On(view.Ref(cls,"mask"),!unlocked);On(view.Ref(cls,"duihuanTip"),!unlocked);Txt(view.Ref<TMP_Text>(cls,"duihuanTip"),"Requires growth level "+Model.Factor(49));RefreshActivityAuctionSelection();
        }
        void StartActivityAuction(bool ticketAd) {
            if(Model.RecoverAuctionTickets(GameModel.DiceLocalMilliseconds)<=0&&!ticketAd){ShowOriginalAd(()=>StartActivityAuction(true));return;}
            if(!Model.StartLocalAuction(activitySelectedHouse,GameModel.DiceLocalMilliseconds,UnityEngine.Random.Range(1,int.MaxValue),ticketAd)){ShowToast("This auction is locked, or you need more auction coins.");return;}
            SaveActivities();if(activityAuctionSelection!=null)On(activityAuctionSelection.Root,false);
            activityAuctionView=OpenPopup("Auction/auctionPlayPanel");var view=activityAuctionView;const string cls="AuctionPlayPanel";ActivityArt(view);
            FitAuctionPlayerLabel(view);
            FitAuctionHeatLabels(view);
            foreach(var pair in new[]{new KeyValuePair<string,string>("jizhuangxiangImg","01"),new KeyValuePair<string,string>("banshanImg","02"),new KeyValuePair<string,string>("jzxOpenImg","03")}) {
                var sprite=SpAt("Assets/GameRes/auction_challenge/challenge_pic_carton_0"+activitySelectedHouse+"_"+pair.Value+".png");if(sprite)view.Ref<Image>(cls,pair.Key).sprite=sprite;
            }
            ActivityImageCaption(view.Ref<Image>(cls,"selectImg"),ActivityHouseNames[Mathf.Clamp(activitySelectedHouse-1,0,5)]);
            var island=SpAt("Assets/GameRes/icon/auction_title/challenge_pic_island_0"+activitySelectedHouse+"_scale2.png");if(island)view.Ref<Image>(cls,"daoImg").sprite=island;
            foreach(var field in new[]{"auctionCalculatPanel","auctionEndPanel","EndPanel","curtain","bidding","fingerGuide","dengguang","jzxOpen"})On(view.Ref(cls,field),false);
            var bid=view.Refs(cls,"bidBtn");for(int i=0;i<bid.Length;i++){int option=i;Click(bid[i],()=>{if(!Model.BidLocalAuction(option))ShowToast("Not enough auction coins to bid.");RefreshActivityAuction();});}
            ActivityButton(view.Ref<Button>(cls,"freeSoldBtn"),"Sell",()=>ClaimActivityAuction(false));ActivityButton(view.Ref<Button>(cls,"adSoldBtn"),"Ad: Sell x2",()=>ShowOriginalAd(()=>ClaimActivityAuction(true)));
            ActivityButton(view.Ref<Button>(cls,"enterEndBtn"),"Continue",()=>ClaimActivityAuction(false));ActivityButton(view.Ref<Button>(cls,"backBtn"),"Back",CloseActivityAuction);
            ActivityButton(view.Ref<Button>(cls,"continueBtn"),"Play Again",()=>{CloseActivityAuction();StartActivityAuction(false);});Click(view.Ref<Button>(cls,"setupBtn"),ShowSetup);
            activityResultShown=false;RefreshActivityAuction();
        }
        void FitAuctionPlayerLabel(UiView view) {
            var marker=view.Ref("AuctionPlayPanel","biaoshi");var original=marker.GetComponent<Image>();var rect=(RectTransform)marker.transform;
            // Retain the native arrow by clipping the sprite's right half, replacing only
            // the Chinese character on its left with a live English caption.
            float width=rect.rect.width,height=rect.rect.height;original.enabled=false;
            var clip=NewRect("Original Player Arrow",rect);clip.anchorMin=clip.anchorMax=new Vector2(.5f,.5f);clip.sizeDelta=new Vector2(width*.5f,height);clip.anchoredPosition=new Vector2(width*.25f,0);clip.gameObject.AddComponent<RectMask2D>();
            var arrow=NewRect("Arrow",clip);arrow.anchorMin=arrow.anchorMax=new Vector2(.5f,.5f);arrow.sizeDelta=new Vector2(width,height);arrow.anchoredPosition=new Vector2(-width*.25f,0);
            var image=arrow.gameObject.AddComponent<Image>();image.sprite=original.sprite;image.color=original.color;image.raycastTarget=false;
            var label=ActivityText(rect,"English Player Caption","You",30);label.rectTransform.anchorMin=label.rectTransform.anchorMax=new Vector2(.5f,.5f);label.rectTransform.sizeDelta=new Vector2(width*.5f,height);label.rectTransform.anchoredPosition=new Vector2(-width*.25f,0);label.color=Color.white;label.outlineColor=Color.black;label.outlineWidth=.15f;
        }
        void FitAuctionItemCaption(UiView view) {
            var name=view.Ref<TMP_Text>("AuctionItem","nameTxt");var price=view.Ref<TMP_Text>("AuctionItem","priceTxt");
            foreach(var text in new[]{name,price}){text.enableWordWrapping=false;text.enableAutoSizing=true;text.fontSizeMax=30;text.fontSizeMin=20;text.overflowMode=TextOverflowModes.Ellipsis;text.alignment=TextAlignmentOptions.Center;text.margin=Vector4.zero;text.rectTransform.anchorMin=text.rectTransform.anchorMax=text.rectTransform.pivot=new Vector2(.5f,.5f);}
            name.rectTransform.sizeDelta=new Vector2(210,46);name.rectTransform.anchoredPosition=new Vector2(0,-95);
            price.rectTransform.sizeDelta=new Vector2(190,40);price.rectTransform.anchoredPosition=new Vector2(0,-144);
        }
        void FitAuctionHeatLabels(UiView view) {
            foreach(string name in new[]{"tag1","tag2","tag3"}) {
                var label=view.Node(name).GetComponent<TMP_Text>();var rect=label.rectTransform;
                // Keep the native triangular markers; reserve their right edge separately.
                var plate=(RectTransform)rect.parent;float extra=300-plate.sizeDelta.x;
                plate.sizeDelta=new Vector2(300,plate.sizeDelta.y);plate.anchoredPosition-=Vector2.right*(extra*.5f);
                rect.sizeDelta=new Vector2(260,54);rect.anchoredPosition=new Vector2(-18,0);
                label.enableWordWrapping=false;label.fontSizeMax=34;label.fontSizeMin=30;label.margin=new Vector4(12,0,32,0);
                label.alignment=TextAlignmentOptions.MidlineRight;
            }
        }
        void RefreshActivityAuction() {
            if(activityAuctionView==null||!activityAuctionView.Root||Model.LocalAuction==null)return;var run=Model.LocalAuction;var view=activityAuctionView;const string cls="AuctionPlayPanel";
            var bids=view.Refs(cls,"bidTxt");float[] factors={run.Config.playerOfferAdd1,run.Config.playerOfferAdd2,run.Config.playerOfferAdd3};
            for(int i=0;i<bids.Length;i++)Txt(bids[i].GetComponent<TMP_Text>(),GameModel.Format(run.Price*factors[i]));
            foreach(var bid in view.Refs(cls,"bidBtn"))bid.GetComponent<Button>().interactable=!run.Finished&&run.Phase==2&&run.LastBidder!=-1;
            Txt(view.Ref<TMP_Text>(cls,"auctionCoinTxt"),GameModel.Format(Model.Data.auctionCoin));Txt(view.Ref<TMP_Text>(cls,"ticketTxt"),Model.Data.ticketCount.ToString());
            Txt(view.Ref<TMP_Text>(cls,"yuguTxt"),"Estimate: "+GameModel.Format(run.RealValue*run.Config.expectPriceRange[0])+" - "+GameModel.Format(run.RealValue*run.Config.expectPriceRange[1]));
            var speech=view.Refs(cls,"speakTxt");Txt(speech[0].GetComponent<TMP_Text>(),run.Phase<2?"Welcome to the auction!":"Current bid: "+GameModel.Format(run.Price)+"\n"+Mathf.CeilToInt(run.HostRemaining)+"s");
            Txt(speech[1].GetComponent<TMP_Text>(),run.LastBidder>=0?"Bidder "+(run.LastBidder+1)+": "+GameModel.Format(run.Price):"");Txt(speech[2].GetComponent<TMP_Text>(),run.LastBidder==-1?"Your bid: "+GameModel.Format(run.Price):"");
            On(view.Ref(cls,"bidding"),run.RaiseMode&&!run.Finished);
            if(run.Finished&&!activityResultShown) {
                activityResultShown=true;On(view.Ref(cls,"downpanel"),false);On(view.Ref(cls,"auctionCalculatPanel"),true);On(view.Ref(cls,"auctionEndPanel"),run.PlayerWon);On(view.Ref(cls,"enterEndBtn"),!run.PlayerWon);On(view.Ref(cls,"jzxOpen"),true);
                foreach(var bubble in view.Refs(cls,"speakImg"))On(bubble,false);
                // The auctioneer's work ends with bidding. Clear the result grid for all six rewards.
                On(view.Ref(cls,"hostImg"),false);
                Txt(view.Ref<TMP_Text>(cls,"zongjiaTxt"),GameModel.Format(run.FinalValue));Txt(view.Ref<TMP_Text>(cls,"yingliTxt"),GameModel.Format(run.PlayerWon?run.FinalValue-run.Price:run.RaiseIncome));
                var positions=view.Refs(cls,"itemPos");
                for(int i=0;i<positions.Length&&i<run.ItemIds.Length;i++) {
                    string name=ActivityItemNames[Mathf.Clamp(run.ItemIds[i]-10001,0,ActivityItemNames.Length-1)];
                    var card=UiPrefab.Instantiate("Auction/auctionItem",positions[i].transform);ActivityArt(card);var sprite=SpAt("Assets/GameRes/icon/auction/"+run.ItemIds[i]+".png");if(sprite)card.Ref<Image>("AuctionItem","icon").sprite=sprite;
                    FitAuctionItemCaption(card);
                    Txt(card.Ref<TMP_Text>("AuctionItem","nameTxt"),name);Txt(card.Ref<TMP_Text>("AuctionItem","priceTxt"),GameModel.Format(run.ItemValues[i]));On(card.Ref("AuctionItem","nameTxt"),true);On(card.Ref("AuctionItem","priceTxt"),true);
                }
            }
        }
        void ClaimActivityAuction(bool doubled) {
            var run=Model.LocalAuction;if(run==null||!run.Finished||run.Claimed)return;double change=Model.ClaimLocalAuction(doubled);SaveActivities();PlayReward();
            var view=activityAuctionView;const string cls="AuctionPlayPanel";On(view.Ref(cls,"auctionCalculatPanel"),false);On(view.Ref(cls,"auctionEndPanel"),false);On(view.Ref(cls,"enterEndBtn"),false);On(view.Ref(cls,"EndPanel"),true);
            Txt(view.Ref<TMP_Text>(cls,"paimai_yingli"),GameModel.Format(run.PlayerWon?change:0));Txt(view.Ref<TMP_Text>(cls,"taijia_yingli"),GameModel.Format(run.RaiseIncome));
            Txt(view.Ref<TMP_Text>(cls,"shijian_yingli"),"0");Txt(view.Ref<TMP_Text>(cls,"qita_yingli"),"0");Txt(view.Ref<TMP_Text>(cls,"zong_yingli"),GameModel.Format(change));
        }
        void CloseActivityAuction() {if(activityAuctionView!=null)ClosePopup(activityAuctionView);activityAuctionView=null;if(activityAuctionSelection!=null&&activityAuctionSelection.Root)On(activityAuctionSelection.Root,true);RefreshActivityAuctionSelection();RefreshActivitiesOverview();}
        bool TryHandleActivityPopup(GameObject root) {
            if(activityArena!=null&&activityArena.Root==root){Model.AbandonLocalChallenge();CloseActivityChallenge();return true;}
            if(activityAuctionView!=null&&activityAuctionView.Root==root){if(Model.LocalAuction!=null&&Model.LocalAuction.Claimed)CloseActivityAuction();return true;}
            if(activityAuctionSelection!=null&&activityAuctionSelection.Root==root){ClosePopup(activityAuctionSelection);activityAuctionSelection=null;return true;}
            return false;
        }
        void UpdateActivities(float delta) {
            if(Model==null)return;
            if(activityArena!=null&&activityArena.Root)TickActivityChallenge(IsBackgrounded||originalAd?0:delta);
            if(activityAuctionView!=null&&activityAuctionView.Root){if(!IsBackgrounded&&!originalAd)Model.TickLocalAuction(delta);RefreshActivityAuction();}
            activityRefreshTimer+=delta;if(activityRefreshTimer>=1){activityRefreshTimer=0;if(activityView!=null&&activityView.Root.activeInHierarchy)RefreshActivitiesOverview();RefreshActivityAuctionSelection();}
        }
    }
    public sealed class ActivityAuctionPager:MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler {
        RectTransform content;int count,index=-1;float spacing,startPointer,startContent,target;bool dragging;Action<int> selected;
        public void Initialize(RectTransform content,int count,float spacing,Action<int> selected){this.content=content;this.count=count;this.spacing=spacing;this.selected=selected;target=content.anchoredPosition.x;}
        public void Select(int value){value=Mathf.Clamp(value,0,count-1);if(value==index)return;index=value;target=-720-index*spacing;}
        public void OnBeginDrag(PointerEventData e){dragging=true;startPointer=e.position.x;startContent=content.anchoredPosition.x;}
        public void OnDrag(PointerEventData e){if(!dragging)return;var canvas=GetComponentInParent<Canvas>();float scale=canvas?canvas.scaleFactor:1;content.anchoredPosition=new Vector2(startContent+(e.position.x-startPointer)/Mathf.Max(.001f,scale),content.anchoredPosition.y);}
        public void OnEndDrag(PointerEventData e){if(!dragging)return;dragging=false;int value=Mathf.Clamp(Mathf.RoundToInt((-720-content.anchoredPosition.x)/spacing),0,count-1);index=-1;Select(value);selected?.Invoke(value);}
        void Update(){if(!content||dragging)return;var position=content.anchoredPosition;position.x=Mathf.Lerp(position.x,target,Mathf.Clamp01(Time.unscaledDeltaTime*10));if(Mathf.Abs(position.x-target)<.5f)position.x=target;content.anchoredPosition=position;}
    }
}

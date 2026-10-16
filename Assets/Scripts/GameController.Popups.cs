using Bufu.Rebuild.UI;
using Data=System.Collections.Generic.Dictionary<string,object>;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    /// <summary>Dialogs and popups: commonDialog, goalReachPanel, OfflineReward, EvolvePanel, setupPanel, CheckIn, congratulationPanel.</summary>
    public sealed partial class GameController {
        UiView goalView,offlineView,checkInView,rewardBoxView;

        // ------------------------------------------------------------ commonDialog
        public void ShowDialog(string message,Action onConfirm,Action onCancel=null) {
            const string cls="CommonDialog";
            var v=OpenPopup("Common/commonDialog");
            Txt(v.Ref<TMP_Text>(cls,"dialogText"),message);
            Click(v.Ref<Button>(cls,"confirmButton"),()=>{ClosePopup(v);onConfirm?.Invoke();});
            Action cancel=()=>{ClosePopup(v);onCancel?.Invoke();};
            Click(v.Ref<Button>(cls,"cancelButton"),cancel);Click(v.Ref<Button>(cls,"bgButton"),cancel);
        }

        // ------------------------------------------------------------ item icons shared by reward popups
        Sprite RewardIcon(int id) {
            if(id==1)return Sp(Model.Data.world==2?"ty_icon_coin02":"ty_icon_coin01@4f9b2316");
            if(id==2)return Sp("ty_icon_diamond01");
            if(id==6)return Sp("ty_icon_talentPoint");
            return Sp(id.ToString());
        }

        // ------------------------------------------------------------ goalReachPanel
        void ShowGoal() {
            const string cls="GoalReachPanel";
            var m=Model.PendingGoal;if(m==null)return;
            var v=OpenPopup("Popup/goalReachPanel");goalView=v;
            int level=Model.MissionLevel(m);var honorMission=Model.TitleMission(level+1);
            Txt(v.Ref<TMP_Text>(cls,"currentLevel"),level.ToString());Txt(v.Ref<TMP_Text>(cls,"nextLevel"),(level+1).ToString());
            Txt(v.Ref<TMP_Text>(cls,"countTmp"),GameModel.Format(m.rewardNum));
            if(honorMission!=null) {
                var s=SpAt("Assets/GameRes/honor/"+honorMission.picId+".png");var honor=v.Ref<Image>(cls,"honor");if(s)honor.sprite=s;
                Txt(v.Node("honorTxt").GetComponent<TMP_Text>(),honorMission.titleName);
            }
            var icon=RewardIcon(m.rewardType);if(icon){var image=v.Ref<Image>(cls,"itemIcon");image.sprite=icon;image.SetNativeSize();}
            Txt(v.Ref<TMP_Text>(cls,"goalTmp"),"恭喜达成<color=#f8642a>"+GameModel.Format(m.mainMissionNum)+"</color>小目标！");
            var root=v.Rect;var script=v.Script(cls);
            float fall=script.F("fallDuration",.5f),exit=script.F("exitDuration",.4f),startScale=script.F("startScale",1.3f),startRotation=script.F("startRotation",15),startY=script.F("startYOffset",1200),exitScale=script.F("exitScale",.3f),exitRotation=script.F("exitRotation",-10);
            root.anchoredPosition=Vector2.up*startY;root.localScale=Vector3.one*startScale;root.localEulerAngles=new Vector3(0,0,startRotation);
            bool ready=false,closing=false;
            var rain=v.Ref<ParticleSystem>(cls,"coinRainEffect");
            StartCoroutine(PromotionTween(fall,t=>{
                if(!root)return;float e=OutBack(t);
                root.anchoredPosition=Vector2.up*startY*(1-e);root.localScale=Vector3.one*Mathf.LerpUnclamped(startScale,1,e);root.localEulerAngles=new Vector3(0,0,startRotation*(1-e));
            },()=>{ready=true;if(rain){rain.gameObject.SetActive(true);rain.Play();}}));
            Action<int> claim=multiplier=>{
                if(!ready||closing)return;closing=true;ready=false;Model.ClaimGoal(multiplier);if(multiplier>1)PlayClick();
                if(rain){rain.Stop();rain.gameObject.SetActive(false);}
                StartCoroutine(PromotionTween(exit,t=>{
                    if(!root)return;float e=InBack(t);
                    root.anchoredPosition=Vector2.down*startY*e;root.localScale=Vector3.one*Mathf.LerpUnclamped(1,exitScale,e);root.localEulerAngles=new Vector3(0,0,exitRotation*e);
                },()=>{ClosePopup(v);goalView=null;shownMainLevel=Model.MainLevel;RefreshAll();}));
            };
            Click(v.Ref<Button>(cls,"confirmBtn"),()=>claim(1));
            Click(v.Ref<Button>(cls,"adBtn"),()=>{if(ready&&!closing)ShowOriginalAd(()=>claim(2));});
        }

        // ------------------------------------------------------------ OfflineReward
        void ShowOffline() {
            const string cls="OfflineIncomeUI";
            if(offlineView!=null||Model.State.pendingOffline<=0)return;
            var v=OpenPopup("Popup/OfflineReward");offlineView=v;
            int world=Model.Data.world;On(v.Node("AdBgImg"),world!=2);On(v.Node("DiceBgImg"),world==2);
            Txt(v.Ref<TMP_Text>(cls,"rewardText"),"+"+GameModel.Format(Model.State.pendingOffline));
            double seconds=awaySeconds[world-1];int hours=(int)(seconds/3600),minutes=(int)(seconds%3600/60);
            Txt(v.Ref<TMP_Text>(cls,"offlineTimeText"),hours+"小时"+minutes+"分");
            On(v.Ref(cls,"doubleClaimButton"),false);
            var normal=v.Ref<Button>(cls,"normalClaimButton");var rect=(RectTransform)normal.transform;rect.anchoredPosition=new Vector2(0,rect.anchoredPosition.y);
            Action claim=()=>{Model.ClaimOffline();PlayReward();ClosePopup(v);offlineView=null;};
            Click(normal,claim);Click(v.Ref<Button>(cls,"closeButton"),claim);Click(v.Ref<Button>(cls,"bgButton"),claim);
        }

        // ------------------------------------------------------------ EvolvePanel
        static readonly string[] EvolveTitles={"铜币进化","银币进化","金币进化","紫金币进化"};
        static readonly string[] DiceEvolveTitles={"铜骰子进化","银骰子进化","金骰子进化","紫金骰子进化"};
        static string Number(double v)=>v.ToString(CultureInfo.InvariantCulture);
        double EvolveMultiplier(Upgrade c,int tier){double m=1;for(int i=0;i<tier&&i<c.evolutionBuff.Length;i++)m*=c.evolutionBuff[i];return m;}

        void ShowEvolve(int quality,Action onConfirm) {
            const string cls="EvolvePanel";
            int world=Model.Data.world;int id=Model.BaseId+quality+1;var c=Model.Config(id,false);int tier=Model.State.evolution[id];
            var v=OpenPopup("Evolve/EvolvePanel");
            string title=(world==1?EvolveTitles:DiceEvolveTitles)[Mathf.Clamp(quality-1,0,3)];Txt(v.Ref<TMP_Text>(cls,"titleTxt"),title);
            var titleImage=v.Ref<RectTransform>(cls,"titleImage");titleImage.sizeDelta=new Vector2(title.Length==4?500:title.Length==5?580:660,166);
            var titleText=v.Ref<TMP_Text>(cls,"titleTxt");var titleSize=titleText.rectTransform.sizeDelta;bool titleWrap=titleText.enableWordWrapping;
            Watch(v.Root,()=>{
                titleText.rectTransform.sizeDelta=L10n.IsEnglish?new Vector2((titleImage.rect.width-135)*titleImage.lossyScale.x/titleText.rectTransform.lossyScale.x,titleSize.y):titleSize;
                titleText.enableWordWrapping=L10n.IsEnglish?false:titleWrap;
            });
            string kind=world==1?"coin":"dice";
            var first=SpAt("Assets/GameRes/NewUI/Evolve/images/"+kind+"_"+quality+"_"+(tier+1)+".png");if(first)v.Ref<Image>(cls,"objImage1").sprite=first;
            var second=SpAt("Assets/GameRes/NewUI/Evolve/images/"+kind+"_"+quality+"_"+(tier+2)+".png");if(second)v.Ref<Image>(cls,"objImage2").sprite=second;
            double before=EvolveMultiplier(c,tier),after=EvolveMultiplier(c,tier+1),ratio=before>0?after/before:1;
            Txt(v.Ref<TMP_Text>(cls,"priceTxt1"),Number(before));Txt(v.Ref<TMP_Text>(cls,"priceTxt2"),Number(after));
            string name=new[]{"铜","银","金","紫"}[Mathf.Clamp(quality-1,0,3)]+(world==1?"币":"骰子");
            Txt(v.Ref<TMP_Text>(cls,"decTxt"),"进化后,所有"+name+"将合成为1个更强的"+name+",新"+name+"收益提升"+Number(ratio)+"倍");
            Txt(v.Ref<TMP_Text>(cls,"decTxt2"),name+"收益*<color=#1a9211>"+Number(ratio)+"</color>倍");
            Click(v.Ref<Button>(cls,"backBtn"),()=>ClosePopup(v));
            Click(v.Ref<Button>(cls,"evolveBtn"),()=>{ClosePopup(v);onConfirm?.Invoke();});
        }

        // ------------------------------------------------------------ setupPanel
        void ShowSetup() {
            const string cls="SetUpPanel";
            var v=OpenPopup("Popup/setupPanel");
            var music=v.Ref<Toggle>(cls,"musicToggle");var sound=v.Ref<Toggle>(cls,"soundToggle");
            var musicSlider=v.Ref<Slider>(cls,"musicSlider");var soundSlider=v.Ref<Slider>(cls,"soundSlider");
            music.isOn=musicOn;sound.isOn=soundOn;musicSlider.value=musicVolume;soundSlider.value=soundVolume;
            music.onValueChanged.AddListener(on=>{musicOn=on;SavePrefs();ApplyAudio();});sound.onValueChanged.AddListener(on=>{soundOn=on;SavePrefs();});
            musicSlider.onValueChanged.AddListener(x=>{musicVolume=x;SavePrefs();ApplyAudio();});soundSlider.onValueChanged.AddListener(x=>{soundVolume=x;SavePrefs();});
            Click(v.Ref<Button>(cls,"backBtn"),()=>ClosePopup(v));Click(v.Ref<Button>(cls,"mask"),()=>ClosePopup(v));
            Click(v.Ref<Button>(cls,"resetButton"),()=>ShowDialog("确定要清除存档吗？\n清除后无法恢复",()=>{ClosePopup(v);ResetGame();}));
            // Server features of the original (restore archive, exit, version, GM) are not part of the offline rebuild.
            foreach(var field in new[]{"restoreButton","exitButton","gameVersion"})On(v.Ref(cls,field),false);
            var gm=v.Ref<Button>(cls,"gmBtn");if(gm)gm.onClick.RemoveAllListeners();
            BuildLanguageSettings(v);
        }

        // ------------------------------------------------------------ CheckIn
        static int CheckInIconIndex(int itemId)=>itemId<=6?itemId-1:6+(itemId-101);
        void ShowCheckIn() {
            const string cls="DailyCheckIn";
            Model.AdvanceCheckIn(GameModel.TodayNumber());
            var v=OpenPopup("CheckIn/CheckIn");
            checkInView=v;
            On(v.Ref(cls,"subscribeRect"),false);
            Click(v.Ref<Button>(cls,"closeButton"),()=>ClosePopup(v));Click(v.Ref<Button>(cls,"maskButton"),()=>ClosePopup(v));
            var icons=v.Script(cls).Arr("itemIcons");
            Func<int,Sprite> iconFor=id=>{
                if(id==1&&Model.Data.world==2)return RewardIcon(id);
                int index=CheckInIconIndex(id);
                if(icons!=null&&index>=0&&index<icons.Count&&icons[index] is Data d&&!string.IsNullOrEmpty(d.Str("sprite")))return Sp(d.Str("sprite"));
                return RewardIcon(id);
            };
            for(int column=0;column<2;column++) {
            bool special=column==1;
            var parent=v.Ref<RectTransform>(cls,special?"adRewardListParent":"freeRewardListParent");
            for(int day=1;day<=7;day++) {
                int d=day;var reward=Model.CheckInRewards(d,special);if(reward==null||reward.Length<4)continue;
                var item=UiPrefab.Instantiate(special?"CheckIn/AdCheckInReward":"CheckIn/FreeCheckInReward",parent);
                item.Root.name=(special?"Special":"Free")+"Day"+day;
                Txt(item.Node("title").GetComponent<TMP_Text>(),"第"+d+"天");
                var slots=new[]{item.Find("rewardbtn1"),item.Find("rewardbtn2")};
                // Original rows claim both rewards together and show only allredpoint.
                foreach(var slot in slots)On(slot.transform.Find("redpoint").gameObject,false);
                Click(item.Node("allRewardBtn"),()=>{
                    if(!Model.CanClaimCheckIn(d,special)){ShowToast(Model.CheckInClaimed(d,special)?"已领取":"还没有到领取时间");return;}
                    Action claim=()=>{if(Model.ClaimCheckIn(d,special,out var gained)){PlayReward();ShowRewardBox(gained);}};
                    if(special)ShowOriginalAd(claim);else claim();
                });
                Watch(item.Root,()=>{
                    bool claimed=Model.CheckInClaimed(d,special),canClaim=Model.CanClaimCheckIn(d,special);
                    On(item.Node("mask"),!Model.DeveloperMode&&d>Model.State.checkDay);On(item.Node("allredpoint"),canClaim);
                    for(int s=0;s<2;s++) {
                        var slot=slots[s].transform;int id=reward[s*2];
                        bool unlock=special&&s==1&&(d==3||d==7)&&id>=101&&id<=110&&!Model.HelperOwned(id-100);
                        var sprite=unlock?Sp(HelperIcons[id-100]):iconFor(id);
                        var image=slot.Find("itemimg").GetComponent<Image>();if(sprite&&image.sprite!=sprite){image.sprite=sprite;image.SetNativeSize();}
                        Txt(slot.Find("count").GetComponent<TMP_Text>(),unlock?"1":GameModel.Format(Model.RewardAmount(id,reward[s*2+1])));
                        On(slot.Find("clamped").gameObject,claimed);
                        var get=slot.Find("huoqv");if(get)On(get.gameObject,unlock);
                    }
                });
            }
            }
        }

        // ------------------------------------------------------------ congratulationPanel
        public void ShowRewardBox(List<KeyValuePair<int,double>> rewards) {
            const string cls="RewardBoxUI";
            var v=OpenPopup("Popup/congratulationPanel");
            rewardBoxView=v;
            On(v.Ref(cls,"doubleButton"),false);
            var template=v.Ref(cls,"item1");var container=v.Ref<RectTransform>(cls,"con");
            foreach(var r in rewards) {
                var copy=Instantiate(template,container,false);copy.name="reward"+r.Key;
                var rt=(RectTransform)copy.transform;rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);rt.localScale=template.transform.localScale;
                var icon=copy.transform.Find("Icon").GetComponent<Image>();var s=RewardIcon(r.Key);if(s){icon.sprite=s;icon.SetNativeSize();}
                Txt(copy.transform.Find("countTmp").GetComponent<TMP_Text>(),GameModel.Format(r.Value));copy.SetActive(true);
            }
            On(template,false);
            Action close=()=>ClosePopup(v);
            Click(v.Ref<Button>(cls,"GetButton"),close);Click(v.Ref<Button>(cls,"closeButton"),close);
        }
    }
}

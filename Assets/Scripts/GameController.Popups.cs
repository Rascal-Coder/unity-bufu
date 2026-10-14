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
        UiView goalView,offlineView;

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
            On(v.Ref(cls,"goalTmp"),false);On(v.Ref(cls,"adBtn"),false);
            var confirm=v.Ref<Button>(cls,"confirmBtn");var confirmRect=(RectTransform)confirm.transform;confirmRect.anchoredPosition=new Vector2(0,confirmRect.anchoredPosition.y);
            var body=(RectTransform)v.Node("GameObject").transform;Vector2 home=body.anchoredPosition;Vector3 scale=body.localScale;
            var script=v.Script(cls);float fall=script.F("fallDuration",.5f),exit=script.F("exitDuration",.4f),startScale=script.F("startScale",1.3f),startRotation=script.F("startRotation",15),startY=script.F("startYOffset",1200),exitScale=script.F("exitScale",.3f),exitRotation=script.F("exitRotation",-10);
            On(confirm.gameObject,false);
            StartCoroutine(Tween(fall,t=>{
                if(!body)return;float e=OutBack(t);
                body.anchoredPosition=home+Vector2.up*startY*(1-e);body.localScale=Vector3.LerpUnclamped(scale*startScale,scale,e);body.localEulerAngles=new Vector3(0,0,startRotation*(1-e));
            },()=>{if(confirm)On(confirm.gameObject,true);}));
            bool closing=false;
            Click(confirm,()=>{
                if(closing)return;closing=true;Model.ClaimGoal();PlayReward();
                StartCoroutine(Tween(exit,t=>{
                    if(!body)return;float e=InBack(t);
                    body.anchoredPosition=home+Vector2.down*startY*e;body.localScale=Vector3.LerpUnclamped(scale,scale*exitScale,e);body.localEulerAngles=new Vector3(0,0,exitRotation*e);
                },()=>{ClosePopup(v);goalView=null;}));
            });
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
        }

        // ------------------------------------------------------------ CheckIn
        static int CheckInIconIndex(int itemId)=>itemId<=6?itemId-1:6+(itemId-101);
        void ShowCheckIn() {
            const string cls="DailyCheckIn";
            Model.AdvanceCheckIn(GameModel.TodayNumber());
            var v=OpenPopup("CheckIn/CheckIn");
            // Only the free column exists here: ad rewards and subscription are left out, so it is centred.
            foreach(var name in new[]{"adgrid","tip2"})On(v.Node(name),false);On(v.Ref(cls,"subscribeRect"),false);
            foreach(var name in new[]{"freegrid","tip1"}){var r=(RectTransform)v.Node(name).transform;r.anchoredPosition=new Vector2(0,r.anchoredPosition.y);}
            Click(v.Ref<Button>(cls,"closeButton"),()=>ClosePopup(v));Click(v.Ref<Button>(cls,"maskButton"),()=>ClosePopup(v));
            var icons=v.Script(cls).Arr("itemIcons");
            Func<int,Sprite> iconFor=id=>{
                int index=CheckInIconIndex(id);
                if(icons!=null&&index>=0&&index<icons.Count&&icons[index] is Data d&&!string.IsNullOrEmpty(d.Str("sprite")))return Sp(d.Str("sprite"));
                return RewardIcon(id);
            };
            var parent=v.Ref<RectTransform>(cls,"freeRewardListParent");
            for(int day=1;day<=7;day++) {
                int d=day;var sign=Model.SignFor(d);if(sign==null)continue;
                var item=UiPrefab.Instantiate("CheckIn/FreeCheckInReward",parent);var n=item.Nodes;
                Txt(n[1].GetComponent<TMP_Text>(),"第"+d+"天");
                var reward=sign.normalReward;int pairs=reward==null?0:reward.Length/2;
                var slots=new[]{new[]{n[2],n[3],n[4],n[5],n[6]},new[]{n[7],n[8],n[9],n[10],n[11]}};
                for(int s=0;s<2;s++) {
                    On(slots[s][0],s<pairs);if(s>=pairs)continue;
                    int itemId=reward[s*2];int count=reward[s*2+1];
                    var sprite=iconFor(itemId);if(sprite){var image=slots[s][1].GetComponent<Image>();image.sprite=sprite;image.SetNativeSize();}
                    Txt(slots[s][2].GetComponent<TMP_Text>(),GameModel.Format(Model.RewardAmount(itemId,count)));
                }
                if(pairs==1){var r=(RectTransform)n[2].transform;r.anchoredPosition=new Vector2(r.anchoredPosition.x+83,r.anchoredPosition.y);}
                Click(n[12],()=>{
                    if(!Model.CanClaimCheckIn(d)){if(Model.CheckInClaimed(d))ShowToast("已领取");else ShowToast("还没有到领取时间");return;}
                    var gained=new List<KeyValuePair<int,double>>();var r=Model.SignFor(d).normalReward;
                    for(int k=0;k+1<r.Length;k+=2)gained.Add(new KeyValuePair<int,double>(r[k],Model.RewardAmount(r[k],r[k+1])));
                    if(Model.ClaimCheckIn(d)){PlayReward();ShowRewardBox(gained);}
                });
                Watch(item.Root,()=>{
                    bool claimed=Model.CheckInClaimed(d),canClaim=Model.CanClaimCheckIn(d);
                    On(n[13],d>Model.State.checkDay);On(n[14],canClaim);
                    for(int s=0;s<pairs&&s<2;s++){On(slots[s][3],canClaim);On(slots[s][4],claimed);}
                });
            }
        }

        // ------------------------------------------------------------ congratulationPanel
        public void ShowRewardBox(List<KeyValuePair<int,double>> rewards) {
            const string cls="RewardBoxUI";
            var v=OpenPopup("Popup/congratulationPanel");
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

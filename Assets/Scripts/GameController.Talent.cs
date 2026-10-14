using Bufu.Rebuild.UI;
using Data=System.Collections.Generic.Dictionary<string,object>;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    /// <summary>Talent tab (Talent/talent or Talent/diceTalent): three skill trees, skill tip, reset and reborn panels.</summary>
    public sealed partial class GameController {
        UiView talentView;int talentWorld,talentCurrent;

        // TalentCardItem positions (13 slots) and the connection lines that leave each card (children of processparent).
        static readonly Vector2[] TalentSlots={
            new Vector2(0,613),new Vector2(-350,330),new Vector2(350,330),new Vector2(-350,-12),new Vector2(350,-12),
            new Vector2(-504,-361),new Vector2(-198,-361),new Vector2(198,-361),new Vector2(502,-361),
            new Vector2(-504,-685),new Vector2(-198,-685),new Vector2(198,-685),new Vector2(502,-685)};
        static readonly int[][] TalentLines={new[]{0,1},new[]{2},new[]{3},new[]{4,5},new[]{6,7},new[]{8},new[]{9},new[]{10},new[]{11}};
        static readonly Regex TalentValuePattern=new Regex(@"(\+?)SSS(%?)");

        static GameObject Pick(GameObject[] list,int i)=>list!=null&&i>=0&&i<list.Length?list[i]:null;

        void EnsureTalent() {
            int world=Model.Data.world;
            if(talentView!=null&&talentView.Root) {
                if(talentWorld==world)return;
                Destroy(talentView.Root);talentView=null;
            }
            talentWorld=world;talentCurrent=0;
            const string mgr="TalentManager";
            var v=UiPrefab.Instantiate(world==2?"Talent/diceTalent":"Talent/talent",talentMount.transform);talentView=v;
            Stretch(v.Rect);On(v.Root,true);

            string[] classes={"TalentCoin","TalentHelper","TalentSpecial"};
            var panels=new[]{v.Ref(mgr,"talentCoin"),v.Ref(mgr,"talentHelper"),v.Ref(mgr,"talentSpecial")};
            var tabButtons=new[]{v.Ref<Button>(mgr,"talentCoinBtn"),v.Ref<Button>(mgr,"talentHelperBtn"),v.Ref<Button>(mgr,"talentSpecialBtn")};
            var backupButton=v.Ref<Button>(mgr,"talentBackupBtn");
            var tabBar=v.Ref(mgr,"tabBar").transform;
            var rebornDots=v.Refs(mgr,"rebornRedPoint");var tabDots=v.Refs(mgr,"tabRepoint");var lines=v.Refs(mgr,"processparent");
            var iconList=v.Script(mgr).Arr("SkillIconList");
            var pointText=v.Ref<TMP_Text>("TalentCoin","pointDisplay");
            var rebornHost=v.Ref<RectTransform>("TalentCoin","rebornPanel");
            On(v.Ref(mgr,"fingerGuide"),false);On(v.Ref(mgr,"talentBackup"),false);

            var refreshers=new List<Action>();var closers=new List<Action>();
            for(int type=0;type<3;type++)BuildTalentPage(v,type,classes[type],Pick(lines,type),iconList,Pick(rebornDots,type),rebornHost,refreshers,closers);

            Action<int> open=t=>{
                talentCurrent=t;foreach(var close in closers)close();
                for(int i=0;i<3;i++){On(panels[i],i==t);On(tabButtons[i].gameObject,i!=t);if(i<tabBar.childCount)On(tabBar.GetChild(i).gameObject,i==t);}
                if(tabBar.childCount>3)On(tabBar.GetChild(3).gameObject,false);
            };
            for(int i=0;i<3;i++){int index=i;Click(tabButtons[i],()=>open(index));}
            Click(backupButton,()=>ShowToast("该功能暂未开放"));
            open(0);

            Watch(v.Root,()=>{
                foreach(var refresh in refreshers)refresh();
                Txt(pointText,GameModel.Format(Model.State.points));
                for(int i=0;i<3;i++)On(Pick(tabDots,i),Model.TalentPage(i+1).Any(Model.TalentUpgradeable));
            });
        }

        static Sprite TalentIcon(List<object> list,int id) {
            if(list==null)return null;
            int index=(id/100-1)*9+id%100-1;if(index<0||index>=list.Count)return null;
            var entry=list[index] as Data;return entry==null?null:Sp(entry.Str("sprite"));
        }

        static string TalentDescription(Talent t,int level) {
            var results=t.upgradeResult;
            double value=0;if(results!=null&&results.Length>0)value=results[Math.Max(0,Math.Min(Math.Min(level,t.upgradeLevelMax-1),results.Length-1))];
            return TalentValuePattern.Replace(t.talentDesc,m=>{
                bool percent=m.Groups[2].Length>0;double shown=percent?value*100:value;
                return "<color=#E57533>"+m.Groups[1].Value+shown.ToString("0.##",CultureInfo.InvariantCulture)+m.Groups[2].Value+"</color>";
            });
        }

        void BuildTalentPage(UiView v,int type,string cls,GameObject linesRoot,List<object> iconList,GameObject rebornDot,RectTransform rebornHost,List<Action> refreshers,List<Action> closers) {
            var parent=v.Ref<RectTransform>(cls,"talentParent");
            var tip=v.Ref(cls,"SkillTip");var tipMask=v.Ref<Button>(cls,"skillTipOffBg");var tipBack=v.Ref<Button>(cls,"backBtn");
            var tipTitle=v.Ref<TMP_Text>(cls,"skillTipTitle");var tipCost=v.Ref<TMP_Text>(cls,"skillTipCost");var tipDesc=v.Ref<TMP_Text>(cls,"skillTipDesc");
            var maxTip=v.Ref(cls,"maxLevelTip");var upButton=v.Ref<Button>(cls,"talentUpBtn");var tipIcon=v.Ref<Image>(cls,"talentSkillIcon");
            var resetButton=v.Ref<Button>(cls,"chongzhiBtn");var rebornButton=v.Ref<Button>(cls,"rebornBtn");
            var resetHost=v.Ref<RectTransform>(cls,"chongzhiPanel");

            Talent selected=null;Action refreshTip=null;
            Action closeTip=()=>{selected=null;On(tip,false);};
            closers.Add(closeTip);On(tip,false);
            Click(tipMask,closeTip);Click(tipBack,closeTip);

            var page=Model.TalentPage(type+1);var cardRefresh=new List<Action>();
            // TalentManager.HideConect: a page of N talents only keeps the first N-1 wires. The rest are the extra bottom stems.
            if(linesRoot) {
                int keep=page.Length>=1&&page.Length<=13?page.Length-1:0;
                for(int j=0;j<linesRoot.transform.childCount;j++)On(linesRoot.transform.GetChild(j).gameObject,j<keep);
            }
            for(int i=0;i<page.Length&&i<TalentSlots.Length;i++) {
                var t=page[i];int index=i;
                var c=UiPrefab.Instantiate("Talent/talentCardItem",parent);On(c.Root,true);c.Root.name="card"+t.id;
                var rect=c.Rect;rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.anchoredPosition=TalentSlots[i];
                var bgOn=c.Nodes[1];var bgOff=c.Nodes[2];var lockImage=c.Nodes[3];var icon=c.Nodes[4].GetComponent<Image>();
                var level=c.Nodes[5].GetComponent<TMP_Text>();var red=c.Nodes[7];
                var sprite=TalentIcon(iconList,t.id);if(sprite&&icon){icon.sprite=sprite;icon.SetNativeSize();}
                Click(c.Nodes[6],()=>{selected=t;On(tip,true);if(refreshTip!=null)refreshTip();});
                cardRefresh.Add(()=>{
                    bool unlocked=Model.TalentUnlocked(t);int lv=Model.State.talents[t.id];
                    On(bgOn,unlocked&&lv>0);On(bgOff,unlocked&&lv==0);On(lockImage,!unlocked);
                    if(icon)On(icon.gameObject,unlocked);On(level.gameObject,unlocked);
                    Txt(level,lv+"/"+t.upgradeLevelMax);On(red,Model.TalentUpgradeable(t));
                    if(linesRoot&&index<TalentLines.Length)foreach(int li in TalentLines[index]) {
                        if(li>=linesRoot.transform.childCount)continue;
                        var line=linesRoot.transform.GetChild(li);
                        if(line.childCount>1)On(line.GetChild(1).gameObject,unlocked&&lv>0);
                    }
                });
            }

            refreshTip=()=>{
                if(selected==null)return;
                var t=selected;int lv=Model.State.talents[t.id];bool max=lv>=t.upgradeLevelMax;int cost=Model.TalentCost(t);
                Txt(tipTitle,t.talentName);Txt(tipDesc,TalentDescription(t,lv));Txt(tipCost,cost.ToString());
                var sprite=TalentIcon(iconList,t.id);if(sprite&&tipIcon)tipIcon.sprite=sprite;
                On(maxTip,max);On(upButton.gameObject,!max);
                var graphic=upButton.targetGraphic;if(graphic)graphic.color=Model.State.points>=cost?Color.white:new Color(.6f,.6f,.6f,1);
            };
            Click(upButton,()=>{
                if(selected==null)return;
                var t=selected;if(Model.State.talents[t.id]>=t.upgradeLevelMax)return;
                if(Model.BuyTalent(t.id))PlayUpgrade();else{PlayCantBuy();ShowToast("金钱未达到升级目标");}
            });
            Click(resetButton,()=>ShowTalentReset(type,resetHost));
            Click(rebornButton,()=>ShowTalentReborn(rebornHost));

            refreshers.Add(()=>{
                foreach(var refresh in cardRefresh)refresh();
                if(refreshTip!=null)refreshTip();
                On(rebornDot,Model.RebornPoints()>0);
            });
        }

        void ShowTalentReset(int type,RectTransform host) {
            int refund=Model.TalentSpent(type+1);
            if(refund<=0){ShowToast("当前可回收天赋点为0,无法重置");return;}
            var r=UiPrefab.Instantiate("Talent/talentResetTipsItem",host);Stretch(r.Rect);On(r.Root,true);
            var n=r.Nodes;
            Txt(n[9].GetComponent<TMP_Text>(),refund.ToString());
            On(n[12],false); // the ad icon: resetting is free in the rebuild
            Action close=()=>{if(r.Root)Destroy(r.Root);};
            Click(n[1],close);Click(n[4],close);Click(n[13],close);
            Click(n[11],()=>{close();if(Model.ResetTalents(type+1))PlayUpgrade();});
        }

        void ShowTalentReborn(RectTransform host) {
            var r=UiPrefab.Instantiate("Talent/talentRebornTipsItem",host);Stretch(r.Rect);On(r.Root,true);
            var n=r.Nodes;
            Action close=()=>{if(r.Root)Destroy(r.Root);};
            var select=n[21];On(select,false);On(n[29],false); // the ad (advanced) reborn is not part of the rebuild
            var normal=(RectTransform)n[32].transform;normal.anchoredPosition=new Vector2(0,normal.anchoredPosition.y);
            Click(n[1],close);Click(n[5],close);
            Click(n[22],()=>On(select,false));Click(n[34],()=>On(select,false));
            Click(n[20],()=>{if(Model.RebornPoints()>0)On(select,true);else ShowToast("金钱未达到重生目标");});
            Click(n[32],()=>{
                if(!Model.Rebirth())return;
                close();SelectTab(1);RebuildActors();SaveStore.Save(Model);PlayUpgrade();
            });
            if(Model.Data.world==2){var coin=Sp("ty_icon_coin02");var image=n[9].GetComponent<Image>();if(coin&&image)image.sprite=coin;}
            Watch(r.Root,()=>{
                int target=Model.RebornTargetIndex();double goal=Model.RebornTable[target].talentName,gold=Model.State.gold;
                bool can=Model.RebornPoints()>0,limit=Model.RebornAtLimit;
                Txt(n[8].GetComponent<TMP_Text>(),"<color=#d55319>"+GameModel.Format(gold)+"</color>/"+GameModel.Format(goal));
                var fill=n[11].GetComponent<Image>();if(fill)fill.fillAmount=goal>0?(float)Math.Min(1d,gold/goal):1f;
                int bonus=(int)Model.TalentValue(Model.Data.world==1?308:310);
                Txt(n[12].GetComponent<TMP_Text>(),"额外+"+bonus);On(n[12],bonus>0);
                Txt(n[16].GetComponent<TMP_Text>(),Model.RebornPoints().ToString());
                On(n[18],can);On(n[19],!can);
                Txt(n[13].GetComponent<TMP_Text>(),limit?"重生已达上限":can?"全局收益*"+Model.RebornGainAt(Model.RebornReachedLevel()).ToString("0.##",CultureInfo.InvariantCulture)+"倍":"未达到全局收益条件");
                var button=n[20].GetComponent<Image>();if(button)button.color=can?Color.white:new Color(.6f,.6f,.6f,1);
            });
        }
    }
}

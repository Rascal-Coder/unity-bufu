using Bufu.Rebuild.UI;
using Data=System.Collections.Generic.Dictionary<string,object>;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    /// <summary>TopPanel and UpgradePanel (bottom tab bar + ItemPanel with the upgradeItem cards).</summary>
    public sealed partial class GameController {
        // ------------------------------------------------------------ top panel
        UiView top;TMP_Text goldTmp,diamondTmp,levelTmp,taskTmp,honorTxt;Image taskFill,honorImage;int shownMainLevel;
        GameObject[] coinIcons,rewardIcons;GameObject topMain,permanentPanel,levelBadge,checkInButton,checkRedPoint;

        void BuildTop() {
            const string cls="TopPanel";
            top=UiPrefab.Instantiate("Common/topPanel",mainLayer);
            shownMainLevel=Model.MainLevel;
            goldTmp=top.Ref<TMP_Text>(cls,"goldNumTmp");diamondTmp=top.Ref<TMP_Text>(cls,"diamondNumTmp");levelTmp=top.Ref<TMP_Text>(cls,"levelTmp");
            // The exported badge has almost no spare line height at 96pt. Keep every
            // growth level on one line and let TMP fit the digits inside the badge.
            levelTmp.enableWordWrapping=false;levelTmp.enableAutoSizing=true;
            levelTmp.fontSizeMax=levelTmp.fontSize;levelTmp.fontSizeMin=Mathf.Min(48,levelTmp.fontSize);
            taskTmp=top.Ref<TMP_Text>(cls,"taskTmp");honorTxt=top.Ref<TMP_Text>(cls,"honorTxt");taskFill=top.Ref<Image>(cls,"taskProgress");honorImage=top.Ref<Image>(cls,"honor");
            coinIcons=top.Refs(cls,"goldCoinIcons");rewardIcons=top.Refs(cls,"rewardTypeImgs");
            topMain=top.Ref(cls,"main");permanentPanel=top.Ref(cls,"permanentPanel");checkInButton=top.Ref(cls,"checkInBtn");checkRedPoint=top.Ref(cls,"checkRepoint");
            levelBadge=top.Ref(cls,"mainLevelBg");
            // Not part of the offline rebuild: ranking, invite, recovery, income tip, guide arrow.
            foreach(var field in new[]{"averageIncome","tipRect","recoveryBtn","rankBtn","inviteBtn","yindaotu"})On(top.Ref(cls,field),false);
            Click(top.Ref<Button>(cls,"setupBtn"),ShowSetup);
            Click(checkInButton,ShowCheckIn);
            var gm=top.Ref<Button>(cls,"gmBtn");if(gm)gm.onClick.RemoveAllListeners();
        }

        void RefreshTop() {
            if(top==null||!top.Root)return;
            Txt(goldTmp,GameModel.Format(Model.State.gold));Txt(diamondTmp,GameModel.Format(Model.Data.diamonds));
            for(int i=0;i<coinIcons.Length;i++)On(coinIcons[i],i==Model.Data.world-1);
            if(Model.PendingGoal==null&&goalView==null)shownMainLevel=Model.MainLevel;
            int level=shownMainLevel;Txt(levelTmp,level.ToString());
            var title=Model.TitleMission(level);
            if(title!=null) {
                var s=SpAt("Assets/GameRes/honor/"+title.picId+".png");if(s&&honorImage.sprite!=s)honorImage.sprite=s;
                Txt(honorTxt,title.titleName);
            }
            var next=Model.NextMission;
            Txt(taskTmp,next==null?"":GameModel.Format(Model.State.gold)+"/"+GameModel.Format(next.mainMissionNum));
            taskFill.fillAmount=next==null?0:(float)Math.Min(1,Model.State.gold/Math.Max(double.Epsilon,next.mainMissionNum));
            for(int i=0;i<rewardIcons.Length;i++)On(rewardIcons[i],next!=null&&next.rewardType==i+1);
            On(checkInButton,Model.LevelAbove(28));On(checkRedPoint,Model.AnyCheckInClaimable());
        }

        void ShowTopMain(bool main){On(topMain,main&&!Model.PromotionReady);On(permanentPanel,main&&!Model.PromotionRunning);On(levelBadge,main&&!Model.PromotionRunning&&!Model.PromotionWonPending);}

        // ------------------------------------------------------------ upgrade panel
        UiView upgrade;int tab=1,subTab=1,builtWorld;
        static readonly int[] MainTabs={1,2,3,5,6};
        GameObject[] chooseImages,lockImages,unlockedHints,subWorldBack,subWorldButton,subWorldSelected;GameObject[] panelRoots=new GameObject[7];
        Image[] tabImages;Button[] subButtons;GameObject sub2Selected,sub3Selected,sub3Hint,itemPanelRoot,talentMount,talentRedDot;
        RectTransform[] subPanels=new RectTransform[3],subContents=new RectTransform[3];RectTransform itemTip;TMP_Text itemTipText;GameObject itemTipMask;

        void BuildUpgrade() {
            const string cls="UpgradePanel",sub="UpgradeItemPanel";
            upgrade=UiPrefab.Instantiate("Common/upgradePanel",mainLayer);
            foreach(int n in MainTabs){int index=n;Click(upgrade.Ref<Button>(cls,"button"+n),()=>OnTabClicked(index));}
            var removed=upgrade.Ref<Button>(cls,"button4");removed.onClick.RemoveAllListeners();On(removed.gameObject,false);
            LayoutBottomNavigation();
            chooseImages=Enumerable.Range(1,6).Select(n=>upgrade.Ref(cls,"chooseImg"+n)).ToArray();
            lockImages=upgrade.Refs(cls,"lockImgs");unlockedHints=upgrade.Refs(cls,"yijiesuoImgs");
            tabImages=upgrade.Refs(cls,"btnImgs").Select(x=>x.GetComponent<Image>()).ToArray();
            itemPanelRoot=upgrade.Ref(cls,"panel1");talentMount=upgrade.Ref(cls,"panel3");
            foreach(var guide in new[]{upgrade.Ref(cls,"guideFinger"),upgrade.Ref(sub,"guideFinger")})On(guide,false);
            foreach(var dot in upgrade.Refs(cls,"redDots"))On(dot,false);
            foreach(var p in new[]{"panel4","panel5","panel6"})On(upgrade.Ref(cls,p),false);
            // sub tabs of the item panel
            subButtons=new[]{upgrade.Ref<Button>(sub,"button1"),upgrade.Ref<Button>(sub,"button2"),upgrade.Ref<Button>(sub,"button3")};
            for(int n=1;n<=3;n++){int index=n;Click(subButtons[n-1],()=>OnSubTab(index));}
            subWorldBack=upgrade.Refs(sub,"worldbackImage");subWorldButton=upgrade.Refs(sub,"worldbutton1");subWorldSelected=upgrade.Refs(sub,"worldbutton1Image");
            sub2Selected=upgrade.Ref(sub,"button2Image");sub3Selected=upgrade.Ref(sub,"button3Image");sub3Hint=upgrade.Ref(sub,"yijiesuoImg3");talentRedDot=upgrade.Node("talentRedDot");
            subPanels[0]=upgrade.Ref<RectTransform>(sub,"panel1");subPanels[1]=upgrade.Ref<RectTransform>(sub,"panel2");subPanels[2]=upgrade.Ref<RectTransform>(sub,"panel3");
            subContents[0]=(RectTransform)upgrade.Find("ItemPanel/panel1/Scroll View/Viewport/Content").transform;
            subContents[1]=(RectTransform)upgrade.Find("ItemPanel/panel2/Scroll View/Viewport/Content").transform;
            subContents[2]=(RectTransform)upgrade.Find("ItemPanel/panel3/Scroll View/Viewport/Content").transform;
            foreach(var content in subContents) {
                var grid=content.GetComponent<GridLayoutGroup>();
                grid.padding.bottom=Math.Max(grid.padding.bottom,64);
            }
            // item tip popup
            itemTip=upgrade.Ref<RectTransform>(cls,"itemTip");itemTipText=upgrade.Ref<TMP_Text>("SkillTip","tipText");itemTipMask=upgrade.Ref(cls,"itemTipMask");
            Click(itemTipMask,HideItemTip);HideItemTip();
            BuildItemCards();
            Canvas.ForceUpdateCanvases();
        }

        void LayoutBottomNavigation() {
            var row=upgrade.Get<RectTransform>("bottoms");
            var fitter=row.GetComponent<ContentSizeFitter>();if(fitter)fitter.enabled=false;
            row.anchorMin=new Vector2(0,0);row.anchorMax=new Vector2(1,0);
            row.sizeDelta=new Vector2(-32,232);row.anchoredPosition=new Vector2(0,116);
            var layout=row.GetComponent<HorizontalLayoutGroup>();
            layout.spacing=8;layout.childControlWidth=true;layout.childForceExpandWidth=true;
            layout.childControlHeight=false;layout.childForceExpandHeight=false;
            foreach(int n in MainTabs) {
                var button=upgrade.Ref<RectTransform>("UpgradePanel","button"+n);
                var cell=button.gameObject.AddComponent<LayoutElement>();cell.minWidth=0;cell.preferredWidth=0;cell.flexibleWidth=1;
                var selection=upgrade.Ref<RectTransform>("UpgradePanel","chooseImg"+n);
                selection.anchorMin=new Vector2(0,.5f);selection.anchorMax=new Vector2(1,.5f);
                selection.sizeDelta=new Vector2(0,232);selection.anchoredPosition=Vector2.zero;
            }
        }

        // Tab unlock rules of UpgradePanel: Button2..6 depend on the growth level (Level > factor).
        bool TabUnlocked(int n) {
            if(!MainTabs.Contains(n))return false;
            switch(n) {
                case 1:return true;
                case 2:return Model.LevelAbove(20);
                case 3:return Model.LevelAbove(21);
                case 4:return false;
                case 5:return Model.LevelAbove(27);
                default:return Model.WorldEntryUnlocked;
            }
        }
        string TabLockText(int n) {
            switch(n) {
                case 2:return "完成成长等级"+(int)Model.Factor(20)+"后解锁";
                case 3:return "完成成长等级"+(int)Model.Factor(21)+"后解锁";
                case 4:return "该功能暂未开放";
                case 5:return "完成成长等级"+(int)Model.Factor(27)+"后解锁";
                default:return "完成成长等级"+(int)Model.Factor(59)+"后解锁";
            }
        }
        static bool Clicked(string key)=>!VerificationMode&&PlayerPrefs.GetInt(PrefKey("button_clicked_"+key),0)==1;
        static void MarkClicked(string key){if(VerificationMode)return;PlayerPrefs.SetInt(PrefKey("button_clicked_"+key),1);PlayerPrefs.Save();}

        void OnTabClicked(int n) {
            if(!TabUnlocked(n)){ShowToast(TabLockText(n));return;}
            if(n>1&&!Clicked(n.ToString())){MarkClicked(n.ToString());RefreshUpgradePanel();}
            SelectTab(n);
        }
        static void Show(UiView view,bool on){if(view!=null&&view.Root)On(view.Root,on);}

        void SelectTab(int n) {
            tab=n;HideItemTip();HideUpgradeFeedback();
            for(int i=0;i<chooseImages.Length;i++)On(chooseImages[i],i==n-1);
            On(itemPanelRoot,n==1);On(talentMount,n==3);
            if(n==2)EnsureFacility();if(n==3)EnsureTalent();if(n==5)EnsureShop();if(n==6)EnsureWorld();
            Show(facilityView,n==2);Show(talentView,n==3);Show(shopView,n==5);Show(worldView,n==6);
            ShowTopMain(n==1);
            // Keep currency controls above the full-screen pages.
            if(top!=null&&top.Root)top.Root.transform.SetAsLastSibling();
            RefreshAll();
        }
        public void OpenTab(int n){if(TabUnlocked(n))SelectTab(n);}

        void OnSubTab(int n) {
            if(n==2&&!Model.LevelAbove(18)){ShowToast("完成成长等级"+(int)Model.Factor(18)+"后解锁");return;}
            if(n==3&&!Model.LevelAbove(80)){ShowToast("完成成长等级"+(int)Model.Factor(80)+"后解锁");return;}
            if(n==2&&!Clicked("sub2"))MarkClicked("sub2");
            if(n==3&&!Clicked("sub3"))MarkClicked("sub3");
            subTab=n;HideItemTip();HideUpgradeFeedback();ApplySubTabs();RefreshUpgradePanel();
        }

        void ApplySubTabs() {
            int world=Model.Data.world;if(subTab==3&&world!=2)subTab=1;
            for(int i=0;i<2;i++){On(subWorldBack[i],world==i+1);On(subWorldButton[i],world==i+1);On(subWorldSelected[i],world==i+1&&subTab==1);}
            On(sub2Selected,subTab==2);On(sub3Selected,world==2&&subTab==3);On(subButtons[2].gameObject,world==2);
            for(int i=0;i<3;i++)On(subPanels[i].gameObject,i==subTab-1);
        }

        void RefreshUpgradePanel() {
            if(upgrade==null||!upgrade.Root)return;
            if(builtWorld!=Model.Data.world)BuildItemCards();
            foreach(int n in MainTabs.Skip(1)) {
                bool open=TabUnlocked(n);On(lockImages[n-1],!open);On(unlockedHints[n-1],open&&!Clicked(n.ToString()));
                tabImages[n-1].color=open?Color.white:new Color(.5f,.5f,.5f,1);
            }
            bool sub2=Model.LevelAbove(18),sub3=Model.LevelAbove(80);
            On(lockImages[0],!sub2);On(unlockedHints[0],sub2&&!Clicked("sub2"));
            On(lockImages[6],!sub3&&Model.Data.world==2);On(sub3Hint,sub3&&!Clicked("sub3")&&Model.Data.world==2);
            ApplySubTabs();
            FitCardScrollAreas();
            On(talentRedDot,TabUnlocked(3)&&Model.Talents.Any(Model.TalentUpgradeable));
            RefreshNotificationDots();
        }

        void FitCardScrollAreas() {
            // The exported viewport extends behind the bottom tabs. Content bounds alone
            // cannot make the final row visible; the viewport must end above that overlay.
            var corners=new Vector3[4];float navTop=float.NegativeInfinity;
            void include(RectTransform rect,float rise=0) {
                rect.GetWorldCorners(corners);
                Vector3 offset=rect.parent.TransformVector(Vector3.up*rise);
                foreach(var corner in corners)navTop=Mathf.Max(navTop,upgrade.Rect.InverseTransformPoint(corner+offset).y);
            }
            include(upgrade.Get<RectTransform>("bottomBgImg"));
            foreach(int i in MainTabs)include(upgrade.Ref<RectTransform>("UpgradePanel","button"+i));
            // Newly unlocked tab hints rise above their buttons and also cover the list.
            foreach(var hint in unlockedHints.Skip(1))if(hint&&hint.activeInHierarchy) {
                var rect=(RectTransform)hint.transform;var motion=hint.GetComponent<UpDownAnimator>();
                include(rect,motion?motion.HighestY-rect.anchoredPosition.y:0);
            }
            Vector3 edge=upgrade.Rect.TransformPoint(new Vector3(0,navTop,0));
            Action<ScrollRect> fit=scroll=>{
                var grid=scroll.content.GetComponent<GridLayoutGroup>();
                if(grid&&grid.padding.bottom<64)grid.padding=new RectOffset(grid.padding.left,grid.padding.right,grid.padding.top,64);
                var viewport=scroll.viewport;
                var parent=(RectTransform)viewport.parent;var inset=viewport.offsetMin;
                float bottom=Mathf.Clamp(parent.InverseTransformPoint(edge).y-parent.rect.yMin,0,parent.rect.height-1);
                if(!Mathf.Approximately(inset.y,bottom)){inset.y=bottom;viewport.offsetMin=inset;}
            };
            foreach(var panel in subPanels)fit(panel.GetComponentInChildren<ScrollRect>(true));
            if(facilityView!=null&&facilityView.Root)foreach(var scroll in facilityView.Root.GetComponentsInChildren<ScrollRect>(true))fit(scroll);
        }

        // ------------------------------------------------------------ item tip
        void ShowItemTip(RectTransform item,string text) {
            if(!itemTip||!item)return;
            Txt(itemTipText,text);
            // UpgradePanel.OnTipButtonClick: place the tip above the card, then move it onto the panel so the scroll mask cannot clip it.
            itemTip.SetParent(item,false);itemTip.anchoredPosition=new Vector2(50f,item.rect.height+100f);
            itemTip.SetParent(upgrade.Rect,true);
            On(itemTipMask,true);On(itemTip.gameObject,true);itemTip.SetAsLastSibling();
        }
        void HideItemTip(){if(itemTip)On(itemTip.gameObject,false);On(itemTipMask,false);}
        void HideUpgradeFeedback() {
            // Purchase feedback is temporarily lifted outside the card's scroll mask.
            // Hide it with its source page when switching tabs.
            if(upgrade==null||!upgrade.Root)return;
            foreach(Transform child in upgrade.Rect)if(child.name=="upgradeTips")On(child.gameObject,false);
        }

        // ------------------------------------------------------------ cards
        static readonly Dictionary<int,string> QuantityIcons=new Dictionary<int,string>{
            {1,"xiaobangshou"},{101,"xiaobangshou"},{2,"tongbi"},{3,"yinbi"},{4,"jinbi"},{5,"zhaohuanzibi"},
            {102,"copperDice"},{103,"silverDice"},{104,"goldDice"},{105,"purpleDice"}};
        static readonly Dictionary<int,string> BoostIcons=new Dictionary<int,string>{
            {1,"tongbishangzhang"},{2,"yinbishangzhang"},{3,"jinbishangzhang"},{4,"biliqianghua1"},{5,"biliqianghua2"},
            {6,"kuaisushouge1"},{7,"kuaisushouge2"},{8,"kuaisushouge3"},{9,"kuaisuyidong"},{10,"liansuofanying"},{11,"liansuofanying_02"},
            {12,"xingyunhuixiang"},{13,"zijinbi"},{14,"zibishangzhang"},{101,"tongtouzishangzhang"},{102,"yintouzishangzhang"},
            {103,"jintouzishangzhang"},{104,"fanqiyintouzi"},{105,"fanqijintouzi"},{106,"huadongshouge1Dice"},{107,"huadongshouge2Dice"},
            {108,"huadongshouge3Dice"},{109,"kuaisuyidong"},{110,"zijintouzishangzhang"}};
        static readonly Dictionary<int,string> SkillIcons=new Dictionary<int,string>{
            {101,"eyunxiaochu"},{102,"haoyunchangzai"},{103,"xingyunhuixiangDice"},{104,"gongmingDice"},{105,"liudianjiazhu"},{106,"chaoji666"},{107,"zuanshishangren"}};
        static Sprite ItemIcon(int kind,int id) {
            var map=kind==0?QuantityIcons:kind==1?BoostIcons:SkillIcons;
            return map.TryGetValue(id,out var name)?SpAt("Assets/GameRes/icon/item/icon_"+name+".png"):null;
        }

        void BuildItemCards() {
            builtWorld=Model.Data.world;
            foreach(var content in subContents)foreach(Transform child in content)Destroy(child.gameObject);
            int world=Model.Data.world;
            foreach(var c in Model.quantities.Where(x=>x.worldId==world).OrderBy(x=>x.id))BuildCard(subContents[0],c,0);
            foreach(var c in Model.boosts.Where(x=>x.worldId==world).OrderBy(x=>x.position))BuildCard(subContents[1],c,1);
            foreach(var c in Model.skills.Where(x=>x.worldId==world).OrderBy(x=>x.position))BuildCard(subContents[2],c,2);
            ApplySubTabs();
        }

        static readonly int[] MaxTipBoostsCoin={1,2,3,101,102,103};
        void BuildCard(RectTransform parent,Upgrade c,int kind) {
            const string cls="UpgradeItem";
            var v=UiPrefab.Instantiate(Model.Data.world==1?"Common/upgradeItem":"Common/upgradeItemDice",parent);v.Root.name="item"+kind+"_"+c.id;
            var levelMax=v.Ref(cls,"levelMaxBg");var unlockImage=v.Ref(cls,"unlockImg");var icon=v.Ref<Image>(cls,"itemIcon");
            var nameTmp=v.Ref<TMP_Text>(cls,"itemNameText");var lvTmp=v.Ref<TMP_Text>(cls,"lvTmp");var lockTmp=v.Ref<TMP_Text>(cls,"lockTmp");
            var upgradeBtn=v.Ref<Button>(cls,"upgradeBtn");var btnImage=v.Ref<Image>(cls,"upgradeBtnImage");var moneyTmp=v.Ref<TMP_Text>(cls,"upgradeMoneyTmp");
            var lvImage=v.Ref(cls,"lvImg");var arrow=v.Ref(cls,"lvAnimator");var tips=v.Ref(cls,"upgradeTips");var tipsTmp=v.Ref<TMP_Text>(cls,"lvTipsTmp");
            var tipButton=v.Ref<Button>(cls,"tipBtn");var maxTip=v.Ref(cls,"maxLevelTip");var evolveBtn=v.Ref<Button>(cls,"evolveBtn");
            Sprite normal=Sp(v.SpriteField(cls,"normalBtnSprite")),disabled=Sp(v.SpriteField(cls,"disabledBtnSprite")),
                normalDiamond=Sp(v.SpriteField(cls,"normalDiamondBtnSprite")),disabledDiamond=Sp(v.SpriteField(cls,"disabledDiamondBtnSprite")),ad=Sp(v.SpriteField(cls,"adBtnSprite"));
            int baseId=Model.BaseId;bool isHelper=kind==0&&c.id==baseId+1;int quality=c.id-baseId-1;bool currency=kind==0&&quality>=1&&quality<=4;
            var iconSprite=ItemIcon(kind,c.id);if(iconSprite)icon.sprite=iconSprite;
            Txt(nameTmp,c.itemName);On(tips,false);var tipsRect=(RectTransform)tips.transform;Vector2 tipsHome=tipsRect.anchoredPosition;var tipCard=tipsRect.parent;Coroutine tipRoutine=null;
            var cardRect=v.Rect;

            Func<bool> affordable=()=>{
                if(isHelper&&Model.HelperGated)return true;if(c.costType==3)return true;
                double cost=Model.PriceKind(c.id,kind);
                return c.costType==2?Model.Data.diamonds>=Math.Ceiling(cost):Model.State.gold>=cost;
            };
            Action<string> feedback=text=>{
                if(tipRoutine!=null)StopCoroutine(tipRoutine);
                tipRoutine=StartCoroutine(CardFeedback(tips,tipsRect,tipCard,tipsHome,tipsTmp,text));
            };
            Click(tipButton,()=>ShowItemTip(cardRect,kind==0?c.itemDesc:string.IsNullOrEmpty(c.itemDescUI)?c.itemDesc:c.itemDescUI));
            Click(upgradeBtn,()=>{
                if(isHelper&&Model.HelperGated){if(Model.UnlockHelperCap()){PlayUpgrade();SyncActors();feedback(c.itemDesc);}return;}
                if(!affordable()){PlayCantBuy();ShowToast(c.costType==2?"钻石不足":"金币不足");return;}
                if(Model.BuyKind(c.id,kind)){PlayUpgrade();if(kind==0)SyncActors();feedback(c.itemDesc);}
            });
            Click(evolveBtn,()=>ShowEvolve(quality,()=>{if(Model.Evolve(quality)){RebuildActors();feedback("进化完成!");}}));

            Watch(v.Root,()=>{
                bool unlocked=Model.Unlocked(c);int lv=Model.LevelKind(c.id,kind);int max=Model.MaxLevel(c);bool maxed=lv>=max;
                On(unlockImage,!unlocked);On(lockTmp.gameObject,!unlocked);
                if(!unlocked){
                    // The English requirement is wider than the card. Keep it inside the icon and the card edge.
                    PlaceLocalized(lockTmp.rectTransform,new Vector2(400,92),new Vector2(68,-50));
                    Txt(lockTmp,Model.LockText(c));
                    if(L10n.IsEnglish){
                        lockTmp.enableWordWrapping=true;lockTmp.overflowMode=TextOverflowModes.Ellipsis;
                        lockTmp.enableAutoSizing=true;lockTmp.fontSizeMax=32;lockTmp.fontSizeMin=20;
                        lockTmp.horizontalAlignment=HorizontalAlignmentOptions.Left;
                    }
                    On(upgradeBtn.gameObject,false);On(lvImage,false);On(lvTmp.gameObject,false);On(evolveBtn.gameObject,false);On(levelMax,false);On(maxTip,false);
                    return;
                }
                On(lvImage,true);On(lvTmp.gameObject,true);
                bool canEvolve=currency&&Model.CanEvolve(quality);
                string lvText;
                if(isHelper)lvText=lv+"/"+Model.HelperCap();
                else if(currency){int next=Model.NextEvolutionLevel(c.id);lvText=next>0?lv+"/"+next:lv.ToString();}
                else lvText=c.upgradeLvMax>0||max<999999?lv+"/"+max:lv.ToString();
                Txt(lvTmp,lvText);
                On(levelMax,maxed);
                bool tip=maxed&&(kind==0&&(c.id==1||c.id==101)||kind==1&&Array.IndexOf(MaxTipBoostsCoin,c.id)>=0);On(maxTip,tip);
                if(maxed){On(upgradeBtn.gameObject,false);On(evolveBtn.gameObject,false);return;}
                On(evolveBtn.gameObject,canEvolve);On(upgradeBtn.gameObject,!canEvolve);
                if(canEvolve)return;
                bool gated=isHelper&&Model.HelperGated,free=c.costType==3,diamond=c.costType==2,afford=affordable();
                btnImage.sprite=gated||free?ad:afford?(diamond?normalDiamond:normal):(diamond?disabledDiamond:disabled);
                On(moneyTmp.gameObject,!(gated||free));if(!(gated||free))Txt(moneyTmp,GameModel.Format(Model.PriceKind(c.id,kind)));
                On(arrow,afford);
            });
        }

        /// <summary>UpgradeItem.PlayUpgradeFeedback: the hint rises 30px and stays 2s. It is moved onto the panel so the scroll mask does not cut off the top row.</summary>
        IEnumerator CardFeedback(GameObject tips,RectTransform rect,Transform card,Vector2 home,TMP_Text text,string message) {
            Txt(text,message);PlaceUpgradeTip(rect,card,home,true);
            On(tips,true);Vector2 start=rect.anchoredPosition;
            yield return Tween(.3f,t=>{if(rect)rect.anchoredPosition=start+Vector2.up*30f*t;});
            float wait=2;while(wait>0&&tips){wait-=Time.unscaledDeltaTime;yield return null;}
            if(!tips||!rect)yield break;On(tips,false);PlaceUpgradeTip(rect,card,home,false);
        }

        /// <summary>
        /// upgradeTips owns a nested world-space canvas. SetParent(worldPositionStays) is correct once;
        /// after that canvas has run, the stored world position is the screen centre, so the next lift flies there.
        /// Always park on the card, then copy the card-local point onto the panel.
        /// </summary>
        void PlaceUpgradeTip(RectTransform rect,Transform card,Vector2 home,bool lift) {
            if(!rect||!card)return;
            var canvas=rect.GetComponent<Canvas>();if(canvas)canvas.enabled=false;
            rect.SetParent(card,false);rect.localScale=Vector3.one;rect.localRotation=Quaternion.identity;rect.anchoredPosition=home;
            if(!lift||upgrade==null||!upgrade.Rect){if(canvas)canvas.enabled=true;return;}
            Vector3 world=card.TransformPoint(rect.localPosition);var panel=upgrade.Rect;
            rect.SetParent(panel,false);rect.localScale=Vector3.one;
            var local=panel.InverseTransformPoint(world);local.z=0;
            if(canvas){canvas.enabled=true;canvas.overrideSorting=true;canvas.sortingOrder=30;}
            rect.localPosition=local;rect.SetAsLastSibling();
        }
    }
}

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
    /// <summary>Full-screen tab panels: facility (desks), shop, world selection.</summary>
    public sealed partial class GameController {
        UiView facilityView,shopView,worldView;

        UiView OpenTabPanel(string resource) {
            var v=UiPrefab.Instantiate(resource,mainLayer);v.Root.transform.SetAsFirstSibling();return v;
        }

        // ------------------------------------------------------------ facility
        int facilityTab=1,facilityWorld;

        void EnsureFacility() {
            if(facilityView!=null&&facilityView.Root)return;
            const string cls="FacilityPanel";
            var v=OpenTabPanel("Facility/facility");facilityView=v;helperWorld=-1;
            On(v.Ref(cls,"getDiamondBtn"),false);On(v.Ref(cls,"helperTabRedDot"),false);
            var helperBtn=v.Ref<Button>(cls,"helperTabBtn");var deskBtn=v.Ref<Button>(cls,"deskTabBtn");
            var helperSelected=v.Ref(cls,"helperTabSelected");var deskSelected=v.Ref(cls,"deskTabSelected");
            var helperPanel=v.Ref(cls,"helperPanel");var deskPanel=v.Ref(cls,"deskPanel");
            var deskRed=v.Ref(cls,"deskTabRedDot");
            Action applyTab=()=>{On(helperSelected,facilityTab==1);On(deskSelected,facilityTab==2);On(helperPanel,facilityTab==1);On(deskPanel,facilityTab==2);};
            Click(helperBtn,()=>{facilityTab=1;applyTab();EnsureHelperList();});
            Click(deskBtn,()=>{facilityTab=2;applyTab();});
            var content=v.Ref<RectTransform>("DeskPanel","contentParent");
            // deskItemPrefab is an external prefab reference, not a node. Cards are instantiated from Facility/deskItem.
            // The embedded helperItem template is not used here.
            foreach(var n in v.Nodes)if(n.name=="helperItem"&&n.GetComponent<RectTransform>()&&n.transform.parent==v.Root.transform)On(n,false);
            facilityTab=1;applyTab();
            Watch(v.Root,()=>{
                int world=Model.Data.world;
                if(facilityWorld!=world){
                    facilityWorld=world;
                    foreach(Transform child in content)Destroy(child.gameObject);
                    var backs=v.Refs(cls,"bgImages");var tiles=v.Refs(cls,"bgTileImages");
                    for(int i=0;i<backs.Length;i++)On(backs[i],i==world-1);for(int i=0;i<tiles.Length;i++)On(tiles[i],i==world-1);
                    foreach(var d in Model.desks.Where(x=>x.WorldId==world).OrderBy(x=>x.id))BuildDeskItem(content,d);
                }
                On(deskRed,Model.desks.Any(d=>d.WorldId==world&&Model.DeskUpgradeable(d)));
                EnsureHelperList();
                On(v.Ref(cls,"helperTabRedDot"),Model.AnyHelperUpgradeNotification);
            });
        }

        static Sprite DeskIcon(Desk d)=>SpAt("Assets/GameRes/icon/desk/icon_zm"+(d.id<100?d.id.ToString("00"):d.id.ToString())+".png");

        void BuildDeskItem(RectTransform parent,Desk d) {
            const string cls="DeskItem";
            var v=UiPrefab.Instantiate("Facility/deskItem",parent);On(v.Root,true);v.Root.name="desk"+d.id;
            var icon=v.Ref<Image>(cls,"usingIcon");var sprite=DeskIcon(d);if(sprite){icon.sprite=sprite;icon.SetNativeSize();}
            Txt(v.Ref<TMP_Text>(cls,"deskNameText"),d.deskName);
            var usingBg=v.Ref(cls,"usingImage");var usingLabel=v.Node("usingLabelImp");var red=v.Ref(cls,"reddotGo");var lockRoot=v.Ref(cls,"lockIcon");
            Action open=()=>{if(Model.DeskUnlocked(d))ShowUseDesk(d);};
            Click(v.Ref<Button>(cls,"itemButton"),open);Click(usingBg,open);
            Watch(v.Root,()=>{
                bool unlocked=Model.DeskUnlocked(d),using_=Model.UsingDesk==d.id;
                On(lockRoot,!unlocked);
                Txt(v.Ref<TMP_Text>(cls,"unlockConditionText"),"解锁条件\n成长等级达到"+d.deskUnlockCondition+"级");
                Txt(v.Ref<TMP_Text>(cls,"deskDescText"),d.deskDesc+"*"+Model.DeskPower(d).ToString("0.0")+"倍");
                Txt(v.Ref<TMP_Text>(cls,"levelText"),Model.DeskLevel(d).ToString());
                On(usingBg,unlocked&&using_);On(usingLabel,unlocked&&using_);On(red,Model.DeskUpgradeable(d));
            });
        }

        void ShowUseDesk(Desk d) {
            const string cls="UseDeskUI";
            var v=OpenPopup("Facility/deskUpgrade");
            var sprite=DeskIcon(d);if(sprite)v.Ref<Image>(cls,"deskIcon").sprite=sprite;
            Txt(v.Ref<TMP_Text>(cls,"deskNameText"),d.deskName);Txt(v.Ref<TMP_Text>(cls,"skillUpgradeText"),d.deskDesc);
            var useButton=v.Ref<Button>(cls,"useButton");var usingImage=v.Ref(cls,"useButtonText");
            var buy=v.Ref<Button>(cls,"buyButton");var moneyBuy=v.Ref<Button>(cls,"moneyBuyButton");var maxLevel=v.Ref(cls,"maxLevel");
            Click(v.Ref<Button>(cls,"closeButton"),()=>ClosePopup(v));Click(v.Ref<Button>(cls,"bgButton"),()=>ClosePopup(v));
            Click(useButton,()=>{Model.UseDesk(d.id);});
            Action purchase=()=>{
                if(Model.BuyDesk(d.id)){PlayUpgrade();return;}
                PlayCantBuy();ShowToast(Model.DeskLevel(d)>=d.skillLevelMax?"已达最大等级":d.buyType==2?"钻石不足":"金币不足");
            };
            Click(buy,purchase);Click(moneyBuy,purchase);
            Watch(v.Root,()=>{
                int lv=Model.DeskLevel(d);bool max=lv>=d.skillLevelMax,using_=Model.UsingDesk==d.id;
                Txt(v.Ref<TMP_Text>(cls,"levelText"),lv.ToString());
                Txt(v.Ref<TMP_Text>(cls,"currentSkillText"),Model.DeskPowerAt(d,lv).ToString("0.0")+"倍");
                Txt(v.Ref<TMP_Text>(cls,"nextSkillText"),Model.DeskPowerAt(d,lv+1).ToString("0.0")+"倍");
                On(maxLevel,max);On(buy.gameObject,!max&&d.buyType!=2);On(moneyBuy.gameObject,!max&&d.buyType==2);
                string cost=GameModel.Format(d.buyType==2?(int)Model.DeskPrice(d):Model.DeskPrice(d));
                Txt(v.Ref<TMP_Text>(cls,"buyButtonText"),cost);Txt(v.Ref<TMP_Text>(cls,"moneyBuyButtonText"),cost);
                On(useButton.gameObject,!using_);On(usingImage,using_);
            });
        }

        // ------------------------------------------------------------ shop
        bool spinning;
        SpineGraphic wheelGlow,wheelBurst,wheelLamps;

        static string BoxDescription(ShopItem item,GameModel model) {
            var parts=new List<string>();
            for(int i=0;i<item.rewardType.Length&&i<item.count.Length;i++) {
                int n=item.count[i];
                if(item.rewardType[i]==3)parts.Add(n+"个N级碎片");
                else if(item.rewardType[i]==4)parts.Add("<color=#e4771e>"+n+"个R级碎片</color>");
                else if(item.rewardType[i]==5)parts.Add("<color=#9227dd>"+n+"个SR碎片</color>");
                else parts.Add(n+"个"+model.ItemName(item.rewardType[i]));
            }
            return "包含："+string.Join("和",parts);
        }

        void EnsureShop() {
            if(shopView!=null&&shopView.Root)return;
            const string cls="ShopTurntablePanel";
            var v=OpenTabPanel("Shop/shop");shopView=v;
            // Original bottom row: ad-diamond card, then the two chests, laid out by the prefab's HorizontalLayoutGroup.
            FitShopLayout(v);
            var closeLayer=v.Node("closebtn");On(closeLayer,false);
            On(v.Node("mask"),false);On(v.Node("mask2"),false);
            FitClock(v.Ref<TMP_Text>(cls,"spinAdButtonText"));FitClock(v.Ref<TMP_Text>(cls,"diamondButtonText"));
            wheelGlow=AttachWheelLight(v.Node("shop_0"),"Uieff_zhuanpan_light","idle");
            wheelLamps=AttachWheelLight(v.Node("shop_2"),"Uieff_zhuanpan_light_2","idle");
            wheelBurst=AttachWheelLight(v.Node("shop_1"),"Uieff_zhuanpan_light_1","effect");
            if(wheelBurst)wheelBurst.gameObject.SetActive(false);
            var infoGroup=v.Ref<RectTransform>(cls,"infoGroup");
            var title1=v.Ref(cls,"titleTmp1");var title2=v.Ref(cls,"titleTmp2");var normalTmp=v.Ref<TMP_Text>(cls,"normalTmp");var seniorTmp=v.Ref<TMP_Text>(cls,"seniorTmp");
            Action hideInfo=()=>{On(infoGroup.gameObject,false);On(closeLayer,false);};
            Action<int,float> showInfo=(id,x)=>{
                var item=Model.shop.First(s=>s.id==id);
                bool normal=id==202;
                Txt(normal?normalTmp:seniorTmp,BoxDescription(item,Model));
                On(title1,normal);On(title2,!normal);On(normalTmp.gameObject,normal);On(seniorTmp.gameObject,!normal);
                infoGroup.anchoredPosition=new Vector2(x,infoGroup.anchoredPosition.y);On(infoGroup.gameObject,true);On(closeLayer,true);
            };
            Click(closeLayer,hideInfo);
            Click(v.Ref<Button>(cls,"info50"),()=>showInfo(202,-699));Click(v.Ref<Button>(cls,"info100"),()=>showInfo(203,-216));
            var diamondButton=v.Ref<Button>(cls,"button201");var diamondMask=v.Node("mask");var diamondClock=v.Ref<TMP_Text>(cls,"diamondButtonText");
            Click(diamondButton,()=>{
                hideInfo();
                if(spinning)return;
                if(Model.DiamondCooling)return;
                var offer=Model.shop.First(s=>s.id==201);
                if(!Model.ClaimDiamondAd()){PlayCantBuy();ShowToast("今日次数已用完");return;}
                PlayReward();
                ShowRewardBox(new List<KeyValuePair<int,double>>{new KeyValuePair<int,double>(offer.rewardType[0],offer.count[0])});
            });
            Txt(v.Ref<TMP_Text>(cls,"adDiamondText"),Model.shop.First(s=>s.id==201).count[0].ToString());
            // chest purchases
            foreach(var pair in new[]{new KeyValuePair<int,string>(202,"button202"),new KeyValuePair<int,string>(203,"button203")}) {
                int id=pair.Key;var item=Model.shop.First(s=>s.id==id);
                Click(v.Ref<Button>(cls,pair.Value),()=>{
                    hideInfo();
                    if(spinning)return;
                    int used=Model.Data.shopDate==GameModel.TodayNumber()?(id==202?Model.Data.chestNormal:Model.Data.chestRare):0;
                    if(!Model.DeveloperMode&&item.todayLimit>0&&used>=item.todayLimit){PlayCantBuy();ShowToast("今日购买次数已达上限");return;}
                    if(Model.Data.diamonds<item.cost){PlayCantBuy();ShowToast("钻石不足");return;}
                    if(Model.BuyChest(id,out var rewards)){PlayUpgrade();ShowRewardBox(rewards);}else PlayCantBuy();
                });
            }
            Txt(v.Ref<TMP_Text>(cls,"adDiamondText2"),((int)Model.shop.First(s=>s.id==202).cost).ToString());
            Txt(v.Ref<TMP_Text>(cls,"adDiamondText3"),((int)Model.shop.First(s=>s.id==203).cost).ToString());
            // wheel
            var slots=v.Refs(cls,"wheelSlots");var wheelItems=Model.WheelItems.Take(slots.Length).ToArray();
            var refreshWheel=new List<Action>();
            for(int i=0;i<slots.Length;i++) {
                const string view="ShopRewardItemView";
                int node=v.IndexOf(slots[i]);var script=v.Script(view,node);if(script==null||i>=wheelItems.Length){On(slots[i],false);continue;}
                var item=wheelItems[i];int type=item.rewardType[0];
                Func<string,GameObject> r=field=>v.Nodes[script.Obj(field).I("node")];
                var image=r("icon").GetComponent<Image>();var value=r("valueText").GetComponent<TMP_Text>();
                refreshWheel.Add(()=>{
                    var iconSprite=ShopWheelIcon(item);
                    if(iconSprite){image.sprite=iconSprite;image.preserveAspect=true;}
                    Txt(value,GameModel.Format(Model.ShopRewardAmount(item,0)));
                });
                On(r("valueCon"),type<3||type>5);On(r("colorbg"),type==5);
                var quality=script.Arr("qualityImages");
                for(int q=0;q<3&&quality!=null&&q<quality.Count;q++)On(v.Nodes[((Data)quality[q]).I("node")],type>=3&&type<=5&&q==type-3);
            }
            var wheel=v.Ref<RectTransform>(cls,"wheelTransform");
            var spinButton=v.Ref<Button>(cls,"spinAdButton");var freeIcon=v.Ref(cls,"freeSpinIcon");var adIcon=v.Ref(cls,"adSpinIcon");var cooldown=v.Node("mask2");var cooldownText=v.Ref<TMP_Text>(cls,"spinAdButtonText");
            Click(spinButton,()=>{
                if(spinning)return;
                if(Model.SpinCooling)return;
                if(!Model.CanSpin){PlayCantBuy();ShowToast("今日次数已用完");return;}
                spinning=true;spinButton.interactable=false;
                var result=Model.Spin(UnityEngine.Random.value);if(result==null){spinning=false;return;}
                // Settle the displayed reward once before presentation. Hiding the shop stops its
                // graphics, and quitting during an animation must not consume a spin without a prize.
                var rewards=Model.BuildShopRewards(result);Model.GrantShop(result,rewards);
                if(!VerificationMode)SaveStore.Save(Model);
                int index=Math.Max(0,Array.IndexOf(wheelItems,result));
                wheelGlow?.Play("rolling",true);wheelLamps?.Play("rolling",true);
                float start=wheel.localEulerAngles.z>180?wheel.localEulerAngles.z-360:wheel.localEulerAngles.z;
                float selected=360f/Math.Max(1,wheelItems.Length)*index+v.Script(cls).F("spinAngleOffset");
                float target=start-Mathf.Repeat(start-selected,360)-360*(v.Script(cls).F("spinExtraRounds",6)+1);
                StartCoroutine(Tween(4.5f,t=>{if(wheel)wheel.localEulerAngles=new Vector3(0,0,Mathf.Lerp(start,target,1-Mathf.Pow(1-t,3)));},()=>FinishSpin(rewards)));
            });
            // guaranteed chest progress
            var progress=v.Ref<Image>(cls,"progressSlider");var progressText=v.Ref<TMP_Text>(cls,"progressText");var chest=v.Ref<Button>(cls,"button301");
            Click(chest,()=>{
                if(spinning)return;
                if(Model.SpinProgress<GameModel.SpinProgressMax){ShowToast("转盘进度未满");return;}
                if(Model.ClaimSpinChest(out var gained)){PlayReward();ShowRewardBox(gained);}
            });
            var shaker=chest.gameObject.GetComponent<ChestShake>()??chest.gameObject.AddComponent<ChestShake>();
            Watch(v.Root,()=>{
                if(!spinning)foreach(var refresh in refreshWheel)refresh();
                bool free=Model.SpinIsFree,cool=Model.SpinCooling,can=Model.CanSpin;
                On(freeIcon,free);On(adIcon,!free);On(cooldown,cool);spinButton.interactable=can&&!spinning;
                if(cool)Txt(cooldownText,Clock(Model.SpinCooldownLeft));
                bool diamondCool=Model.DiamondCooling;
                On(diamondMask,diamondCool);diamondButton.interactable=!spinning&&!diamondCool;
                v.Ref<Button>(cls,"button202").interactable=!spinning;v.Ref<Button>(cls,"button203").interactable=!spinning;
                if(diamondCool)Txt(diamondClock,Clock(Model.DiamondCooldownLeft));
                progress.fillAmount=Mathf.Clamp01(Model.SpinProgress/(float)GameModel.SpinProgressMax);
                Txt(progressText,Model.SpinProgress+"/"+GameModel.SpinProgressMax);
                shaker.shaking=Model.SpinProgress>=GameModel.SpinProgressMax;
                chest.interactable=!spinning&&Model.SpinProgress>=GameModel.SpinProgressMax;
            });
            StartCoroutine(ShopClock(v));
        }

        static void FitClock(TMP_Text text) {
            if(!text)return;text.enableWordWrapping=false;text.overflowMode=TextOverflowModes.Overflow;
            var rect=text.rectTransform;rect.sizeDelta=new Vector2(520,rect.sizeDelta.y);
        }
        static void FitShopLayout(UiView view) {
            // Keep progress and its count below the shared currency bar.
            var progress=(RectTransform)view.Node("processBar").transform;
            progress.anchorMin=progress.anchorMax=new Vector2(.5f,1);
            progress.anchoredPosition=new Vector2(progress.anchoredPosition.x,-518);
            // The exported scene places the 426-high offer cards underneath the bottom tabs
            // and their unlock labels (which reach 374 units above the screen bottom).
            // Keep the original artwork and card sizes, reserving that complete navigation area.
            var offers=(RectTransform)view.Node("itemLayout").transform;
            offers.anchorMin=offers.anchorMax=new Vector2(.5f,0);
            offers.anchoredPosition=new Vector2(offers.anchoredPosition.x,612);
            var spin=(RectTransform)view.Node("rollBtnGroup").transform;
            spin.anchorMin=spin.anchorMax=new Vector2(.5f,0);spin.anchoredPosition=new Vector2(0,968);
            var wheel=(RectTransform)view.Node("spinnerGroup").transform;
            wheel.anchorMin=wheel.anchorMax=new Vector2(.5f,0);wheel.anchoredPosition=new Vector2(0,1608);wheel.localScale=new Vector3(.68f,.68f,1);
        }
        static string Clock(int seconds){if(seconds<0)seconds=0;return (seconds/60).ToString("00")+":"+(seconds%60).ToString("00");}
        static SpineGraphic AttachWheelLight(GameObject go,string asset,string animation) {
            if(!go)return null;var graphic=go.GetComponent<SpineGraphic>()??go.AddComponent<SpineGraphic>();
            if(!graphic.Load(asset))return null;graphic.Play(animation,true);return graphic;
        }
        Sprite ShopWheelIcon(ShopItem item) {
            if(item.id==101)return RewardIcon(1);
            string image=null;
            if(item.id==102||item.id==103)image="ty_icon_money0"+(item.id-100)+(Model.Data.world==2?"_dice":"");
            else if(item.id>=104&&item.id<=106)image="ty_icon_diamond0"+(item.id-103);
            return image==null?RewardIcon(item.rewardType[0]):SpAt("Assets/GameRes/NewUI/Shop/images/"+image+".png");
        }
        void FinishSpin(List<KeyValuePair<int,double>> rewards) {
            wheelGlow?.Play("idle",true);wheelLamps?.Play("idle",true);
            StartCoroutine(CompleteSpin(rewards));
        }
        IEnumerator CompleteSpin(List<KeyValuePair<int,double>> rewards) {
            if(wheelBurst) {
                wheelBurst.gameObject.SetActive(true);wheelBurst.Play("effect",true);
                // A graphic's Update is suspended when its tab is hidden. The controller owns
                // completion so switching tabs cannot leave the shop locked forever.
                yield return new WaitForSecondsRealtime(wheelBurst.AnimationDuration*3);
                if(wheelBurst)wheelBurst.gameObject.SetActive(false);
            }
            spinning=false;RefreshAll();PlayReward();ShowRewardBox(rewards);
        }
        IEnumerator ShopClock(UiView v){while(v!=null&&v.Root){yield return new WaitForSecondsRealtime(1);if(v.Root&&v.Root.activeInHierarchy)RefreshAll();}}

        // ------------------------------------------------------------ world selection
        TMP_Text worldStatus;Coroutine worldStatusRoutine;
        void EnsureWorld() {
            if(worldView!=null&&worldView.Root)return;
            const string cls="WorldSelectionPanel";
            var v=OpenTabPanel("world/worldSelectionPanel");worldView=v;
            On(v.Ref(cls,"worldNodePrefab"),false);On(v.Ref(cls,"worldNodePrefab1"),false);On(v.Ref(cls,"guideAnimationObj"),false);
            worldStatus=v.Ref<TMP_Text>(cls,"statusText");Txt(worldStatus,"");
            Click(v.Ref<Button>(cls,"closeButton"),()=>SelectTab(1));
            var rootButton=v.Root.GetComponent<Button>();if(rootButton)rootButton.onClick.RemoveAllListeners();
            FitWorldPage(v);BuildWorldNodes(v);
            Watch(v.Root,()=>RefreshWorldNodes());
        }

        readonly List<Action> worldNodeRefreshers=new List<Action>();
        void RefreshWorldNodes(){foreach(var r in worldNodeRefreshers)r();}

        void BuildWorldNodes(UiView panel) {
            var scroll=panel.Ref<ScrollRect>("WorldSelectionPanel","worldScrollView");var content=scroll.content;
            var viewport=scroll.viewport?scroll.viewport:(RectTransform)content.parent;
            foreach(Transform child in content)Destroy(child.gameObject);worldNodeRefreshers.Clear();
            var configs=Model.worlds.OrderBy(x=>x.sort).ToArray();
            var layout=content.GetComponent<VerticalLayoutGroup>();
            float height=viewport.rect.height;
            if(layout){layout.padding.top=layout.padding.bottom=Mathf.Max(64,Mathf.RoundToInt((height-WorldNodeHeight)/2));layout.spacing=layout.padding.top+64;layout.childControlHeight=false;layout.childControlWidth=false;layout.childForceExpandHeight=false;layout.childForceExpandWidth=false;layout.childAlignment=TextAnchor.UpperCenter;}
            var nodes=new RectTransform[configs.Length+1];
            for(int index=0;index<=configs.Length;index++) {
                const string cls="WorldNode";
                World config=index<configs.Length?configs[index]:null;
                string prefab=config==null?"world/WorldNode ComingSoon":config.id==2?"world/WorldNode 1":"world/WorldNode";
                var v=UiPrefab.Instantiate(prefab,content);On(v.Root,true);nodes[index]=v.Rect;
                On(v.Ref(cls,"tail"),false);FitWorldNode(v,config);
                var island=v.Ref<Button>(cls,"islandButton");var image=v.Ref<Image>(cls,"islandImage");var lockPanel=v.Ref(cls,"lockPanel");var unlockPanel=v.Ref(cls,"unlockPanel");var lockText=v.Ref<TMP_Text>(cls,"lockText");
                GameObject currentImage=null;try{currentImage=v.Ref(cls,"currentImage");}catch(KeyNotFoundException){}
                if(config==null){Txt(v.Ref<TMP_Text>(cls,"titleText"),"更多世界");Txt(lockText,"敬请期待");On(lockPanel,true);On(unlockPanel,false);Click(island,()=>ShowWorldStatus("更多世界敬请期待"));continue;}
                Txt(v.Ref<TMP_Text>(cls,"titleText"),config.worldName);
                var captured=config;
                Click(island,()=>OnSelectWorld(captured));
                worldNodeRefreshers.Add(()=>{
                    if(!v.Root)return;
                    bool unlocked=Model.WorldUnlocked(captured.id);
                    image.color=unlocked?Color.white:Color.black;On(lockPanel,!unlocked);On(unlockPanel,unlocked);
                    if(!unlocked){var prev=Model.worlds.FirstOrDefault(w=>w.id==captured.preWorldId);Txt(lockText,(prev==null?"":prev.worldName)+"完成<color=#c30505>"+captured.requiredPreWorldLevel+"</color>级后解锁");}
                    On(currentImage,Model.Data.world==captured.id);
                });
            }
            RefreshWorldNodes();
            Canvas.ForceUpdateCanvases();LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            content.sizeDelta=new Vector2(content.sizeDelta.x,Mathf.Max(LayoutUtility.GetPreferredHeight(content),viewport.rect.height));
            int current=Math.Max(0,Array.FindIndex(configs,w=>w.id==Model.Data.world));
            float center=-nodes[current].anchoredPosition.y;
            content.anchoredPosition=new Vector2(content.anchoredPosition.x,Mathf.Clamp(center-viewport.rect.height/2,0,Mathf.Max(0,content.sizeDelta.y-viewport.rect.height)));
        }

        void ShowWorldStatus(string message) {
            if(!worldStatus)return;Txt(worldStatus,message);
            if(worldStatusRoutine!=null)StopCoroutine(worldStatusRoutine);
            worldStatusRoutine=StartCoroutine(ClearWorldStatus());
        }
        IEnumerator ClearWorldStatus(){yield return new WaitForSecondsRealtime(2);if(worldStatus)Txt(worldStatus,WorldHint);}

        void OnSelectWorld(World config) {
            if(Model.Data.world==config.id){ShowWorldStatus("当前已处于该世界");return;}
            if(!Model.WorldUnlocked(config.id)){ShowWorldStatus("该世界尚未解锁");return;}
            ShowDialog("是否确定切换至"+config.worldName+"？",()=>{
                if(!Model.SwitchWorld(config.id))return;
                RebuildActors();SelectTab(1);SaveStore.Save(Model);
                if(Model.State.pendingOffline>0)ShowOffline();
            });
        }
    }
}

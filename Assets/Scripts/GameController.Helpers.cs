using Bufu.Rebuild.UI;
using Data=System.Collections.Generic.Dictionary<string,object>;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    /// <summary>HelperPanel and UseHelperUI: hire, upgrade and assign the ten premium helpers. Ad unlocks are free.</summary>
    public sealed partial class GameController {
        int helperWorld=-1;
        static readonly string[] HelperIcons={"","icon_dali","icon_pifeng","icon_mofa","icon_kungfu","icon_jikun","icon_dashe","icon_jiuwei","icon_gangtierenpng","icon_halande","icon_dushen"};

        void EnsureHelperList() {
            if(facilityView==null||!facilityView.Root||Model.helperRoles==null)return;
            var content=facilityView.Ref<RectTransform>("HelperPanel","contentParent");
            var total=facilityView.Ref<TMP_Text>("HelperPanel","totalCountText");
            if(helperWorld!=Model.Data.world) {
                helperWorld=Model.Data.world;Model.RefreshHelperUnlocks(Model.Data.world);
                for(int i=content.childCount-1;i>=0;i--){var child=content.GetChild(i);if(child.name.StartsWith("helper-"))Destroy(child.gameObject);}
                foreach(var role in Model.helperRoles.OrderBy(x=>x.sorting)) {
                    if(role.id==9&&!Model.HelperOwned(9))continue;
                    BuildHelperCard(content,role);
                }
            }
            int slots=Mathf.Max(1,Model.Helpers);
            Txt(total,Mathf.Max(0,Model.Helpers-Model.AssignedHelpers)+"/"+slots);
        }

        static string SkillIconName(int id,int level) {
            switch(id) {
                case 1:return "ty_icon_toughguy_1"; case 2:return "ty_icon_miracle_1";
                case 3:return "ty_icon_magician_"+level; case 4:return "ty_icon_kungfu_"+level; case 5:return "ty_icon_jikun_"+level;
                case 6:return "ty_icon_dashe_"+level; case 7:return "ty_icon_jiuwei_"+level; case 8:return "ty_icon_gangtiern_"+level;
                case 9:return "ty_icon_halande_"+level; default:return "ty_icon_dushen_"+level;
            }
        }
        static string SkillText(HelperRole role,float value) {
            bool range=role.id==4||role.id==9||role.id==10;
            string number=range?value.ToString("0.0"):value.ToString("0");
            return (role.helperDesc??"").Replace("sss",number).Replace("SSS",number);
        }

        void BuildHelperCard(RectTransform parent,HelperRole role) {
            const string cls="HelperItem";
            var v=UiPrefab.Instantiate("Facility/helperItem",parent);v.Root.name="helper-"+role.id;On(v.Root,true);
            var icon=Sp(role.id>=0&&role.id<HelperIcons.Length?HelperIcons[role.id]:"");
            if(icon){var face=v.Ref<Image>(cls,"helperIcon");var face2=v.Ref<Image>(cls,"helperIcon2");face.sprite=icon;face.SetNativeSize();face2.sprite=icon;face2.SetNativeSize();}
            Txt(v.Ref<TMP_Text>(cls,"helperNameText"),role.helperName);
            ApplyHelperCardFrame(v,role.Rare);
            On(v.Ref(cls,"thirstDayTip"),false);On(v.Ref(cls,"sevenDayTip"),false);On(v.Ref(cls,"halandeTip"),false);
            On(v.Ref(cls,"subscribeUnlockBtn"),false);
            Click(v.Ref<Button>(cls,"upgradeBtn1"),()=>{if(Model.HelperOwned(role.id))ShowHelperUpgrade(role);});
            Click(v.Ref<Button>(cls,"plusBtn"),()=>{if(!Model.AdjustHelperCount(role.id,1))PlayCantBuy();else SyncActors();});
            Click(v.Ref<Button>(cls,"minusBtn"),()=>{if(Model.AdjustHelperCount(role.id,-1))SyncActors();});
            Click(v.Ref<Button>(cls,"adUnlockBtn"),()=>{if(Model.AdUnlockHelper(role.id)){PlayUpgrade();helperWorld=-1;EnsureHelperList();}});
            Watch(v.Root,()=>{
                bool owned=Model.HelperOwned(role.id);var state=Model.HelperOf(role.id);int level=state==null?1:state.level;
                On(v.Ref(cls,"lockIcon"),!owned);On(v.Ref(cls,"upgradeBtn1"),owned);On(v.Ref(cls,"redDot"),Model.CanUpgradeHelper(role.id));
                On(v.Ref(cls,"adUnlockBtn"),Model.CanAdUnlock(role));
                string unlock=role.UnlockLevel==2?"在骰子世界"+role.helperUnlockCondition+"级解锁":"解锁条件\n成长等级达到"+role.helperUnlockCondition+"级";
                Txt(v.Ref<TMP_Text>(cls,"unlockConditionText"),owned?"":unlock);
                On(v.Ref(cls,"helperLevelText"),false);
                Txt(v.Ref<TMP_Text>(cls,"helperDescText"),SkillText(role,Model.HelperSkillValue(role.id)));
                Txt(v.Ref<TMP_Text>(cls,"countText"),(state==null?0:state.count)+"/"+(state==null?1:state.max));
            });
        }

        /// <summary>HelperItem.UpdateUI: bgSprites[(rare-1)*2 .. +1] is the card frame, rateIcons[rare-1] is the N/R/SR corner badge.</summary>
        static void ApplyHelperCardFrame(UiView v,int rare) {
            const string cls="HelperItem";
            int index=Mathf.Clamp(rare,1,3)-1;
            var script=v.Script(cls);
            var frames=script.Arr("bgSprites");var badges=script.Arr("rateIcons");
            if(frames!=null&&index*2+1<frames.Count) {
                var front=Sp(((Data)frames[index*2]).Str("sprite"));var back=Sp(((Data)frames[index*2+1]).Str("sprite"));
                var bg1=v.Ref<Image>(cls,"bg1");var bg2=v.Ref<Image>(cls,"bg2");
                if(front)bg1.sprite=front;if(back)bg2.sprite=back;
            }
            if(badges!=null&&index<badges.Count) {
                var mark=Sp(((Data)badges[index]).Str("sprite"));var icon=v.Ref<Image>(cls,"rateIcon");
                if(mark&&icon){icon.sprite=mark;On(icon.gameObject,true);}
            }
        }

        void ShowHelperUpgrade(HelperRole role) {
            const string cls="UseHelperUI";
            var v=OpenPopup("Facility/helperUpgrade");
            On(v.Ref(cls,"upgradeTip1"),false);On(v.Ref(cls,"upgradeTip2"),false);On(v.Ref(cls,"maxObject"),false);
            var portrait=v.Ref<Image>(cls,"helperIcon");portrait.enabled=false;MountHelperPortrait(portrait.transform,role);
            Txt(v.Ref<TMP_Text>(cls,"helperNameText"),role.helperName);
            Txt(v.Ref<TMP_Text>(cls,"limitInfoText"),role.helperName+"数量上限+1");
            On(v.Ref(cls,"addhelperAdsBtn"),false);
            var frames=v.Refs(cls,"NSRbg");var marks=v.Refs(cls,"NSR");int rareIndex=Mathf.Clamp(role.Rare,1,3)-1;
            for(int i=0;i<frames.Length;i++)On(frames[i],i==rareIndex);
            for(int i=0;i<marks.Length;i++)On(marks[i],i==rareIndex);
            Click(v.Ref<Button>(cls,"closeButton"),()=>ClosePopup(v));Click(v.Ref<Button>(cls,"bgButton"),()=>ClosePopup(v));
            Click(v.Ref<Button>(cls,"upgradeBtn"),()=>{if(Model.UpgradeHelper(role.id))PlayUpgrade();else{PlayCantBuy();ShowToast("金币或碎片不足");}});
            Click(v.Ref<Button>(cls,"addhelperMoneyBtn"),()=>{if(Model.BuyHelperSlot(role.id))PlayUpgrade();else{PlayCantBuy();ShowToast("金币不足");}});
            Click(v.Ref<Button>(cls,"addhelperDiamondBtn"),()=>{if(Model.BuyHelperSlot(role.id))PlayUpgrade();else{PlayCantBuy();ShowToast("钻石不足");}});
            var awakeNodes=new List<int>();
            for(int i=0;i<v.Nodes.Length;i++)if(v.Script("HelperSkill",i)!=null)awakeNodes.Add(i);
            Watch(v.Root,()=>{
                var state=Model.HelperOf(role.id);if(state==null)return;
                int level=state.level,rank=GameModel.SkillRank(role,level),nextRank=GameModel.SkillRank(role,level+1);
                float now=role.baseSkillNum+role.upgradeSkillNum*rank,next=role.baseSkillNum+role.upgradeSkillNum*nextRank;
                string unit=role.id==1?"倍":(role.id==4||role.id==9||role.id==10)?"范围":(role.id==7||role.id==8)?"颗":"个";
                string num=role.id==4||role.id==9||role.id==10?"0.0":"0";
                Txt(v.Ref<TMP_Text>(cls,"skillLevelText"),level.ToString());
                Txt(v.Ref<TMP_Text>(cls,"currentCountText"),state.count+"/"+state.max);
                Txt(v.Ref<TMP_Text>(cls,"skillDescText"),SkillText(role,now));
                Txt(v.Ref<TMP_Text>(cls,"currentSkillText"),now.ToString(num)+unit);
                Txt(v.Ref<TMP_Text>(cls,"nextSkillText"),next.ToString(num)+unit);
                float income=Model.HelperIncomeMul(role.id),incomeNext=(float)System.Math.Pow(role.helperIncomeAddBuff,level);
                Txt(v.Ref<TMP_Text>(cls,"skillDescText2"),role.helperName+"翻硬币收益：");
                Txt(v.Ref<TMP_Text>(cls,"currentSkillText2"),Sig(income)+"倍");
                Txt(v.Ref<TMP_Text>(cls,"nextSkillText2"),Sig(incomeNext)+"倍");
                var levelDesc=v.Ref<TMP_Text>(cls,"upgradeLevelDescText");
                if(role.upgradeLevel!=null){var text="";for(int i=0;i<role.upgradeLevel.Length;i++)text+=(i==nextRank?"<color=#453110>":"")+role.upgradeLevel[i]+(i==nextRank?"</color>":"")+(i==role.upgradeLevel.Length-1?"":"-");Txt(levelDesc,text+"    ");}
                levelDesc.enableWordWrapping=false;levelDesc.overflowMode=TextOverflowModes.Overflow;levelDesc.ForceMeshUpdate();
                var marker=v.Ref(cls,"upgradeLevelbg");bool topped=role.upgradeLevel==null||rank>=role.upgradeLevel.Length;
                On(marker,!topped);
                if(!topped){var rt=(RectTransform)marker.transform;float width=levelDesc.preferredWidth;rt.anchoredPosition=new Vector2(width/(role.upgradeLevel.Length-.5f)*nextRank-width/2f,rt.anchoredPosition.y);}
                int frag=Model.HelperFragmentCost(role,level);bool showFrag=nextRank>rank&&frag>0;
                On(v.Ref(cls,"suipianObject"),showFrag);
                var fragIcon=SpAt("Assets/GameRes/icon/suipian/"+(100+role.id)+".png");
                if(fragIcon){v.Ref<Image>(cls,"suipianown").sprite=fragIcon;if(showFrag)v.Ref<Image>(cls,"suipian").sprite=fragIcon;}
                Txt(v.Ref<TMP_Text>(cls,"suipianownText"),Model.Fragments(role.id).ToString());
                if(showFrag)Txt(v.Ref<TMP_Text>(cls,"suipianText"),Model.Fragments(role.id)+"/"+frag);
                double cost=Model.HelperUpgradeCost(role,level);bool enough=Model.State.gold>=cost&&Model.Fragments(role.id)>=frag;
                Txt(v.Ref<TMP_Text>(cls,"moneyText"),(enough?"<color=#15860d>":"<color=#EE3333>")+GameModel.Format(Model.State.gold)+"</color>/"+GameModel.Format(cost));
                On(v.Ref(cls,"UpgradeBtnImg1"),enough);On(v.Ref(cls,"UpgradeBtnImg2"),!enough);
                bool maxed=level>=role.upgradeLevelMax;On(v.Ref(cls,"upgradeBtn"),!maxed);On(v.Ref(cls,"maxObject"),maxed);
                bool gold=Model.SlotUsesGold(role,state.max);
                On(v.Ref(cls,"addhelperMoneyBtn"),gold);On(v.Ref(cls,"addhelperDiamondBtn"),!gold);
                if(gold)Txt(v.Ref<TMP_Text>(cls,"addhelperMoneyCostText"),GameModel.Format(Model.SlotGoldCost(role,state.max)));
                else Txt(v.Ref<TMP_Text>(cls,"addhelperDiamondCostText"),GameModel.Format(Model.SlotDiamondCost(role,state.max)));
                int world=Mathf.Clamp(Model.Data.world,1,2);var worldBtn=Sp("wupin_btn_shengji0"+world);
                if(worldBtn)v.Ref<Image>(cls,"addhelperMoneyBtnIcon").sprite=worldBtn;
                string[] body={role.awakeSkill1,role.awakeSkill2,role.awakeSkill3},names={role.awakeSkill1Name,role.awakeSkill2Name,role.awakeSkill3Name};
                for(int i=0;i<awakeNodes.Count;i++) {
                    bool show=role.awakeSkillNum!=null&&i<role.awakeSkillNum.Length&&body[i]!="0";
                    On(v.Nodes[awakeNodes[i]],show);if(!show)continue;
                    var data=v.Script("HelperSkill",awakeNodes[i]);bool on=level>=role.awakeSkillNum[i];
                    Txt(v.Nodes[data.Obj("skillunLockText").I("node")].GetComponent<TMP_Text>(),on?names[i]:role.awakeSkillNum[i]+"级解锁");
                    On(v.Nodes[data.Obj("skillLock").I("node")],!on);
                    var sprite=Sp(SkillIconName(role.id,i+1));var image=v.Nodes[data.Obj("skillIcon").I("node")].GetComponent<Image>();if(sprite&&image)image.sprite=sprite;
                }
            });
        }

        static string Sig(float value){if(value>=100f)return GameModel.Format(value);if(value>=10f)return value.ToString("0.0");return value.ToString("0.00");}

        /// <summary>UseHelperUI loads the role prefab under helperIcon, plays idle_front, flips X and sits 50px below the icon pivot.</summary>
        void MountHelperPortrait(Transform parent,HelperRole role) {
            var go=new GameObject("portrait",typeof(RectTransform));
            var rt=(RectTransform)go.transform;rt.SetParent(parent,false);
            rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);rt.pivot=new Vector2(.5f,.5f);
            rt.anchoredPosition=new Vector2(0,-50f);rt.localScale=new Vector3(-1f,1f,1f);
            var vat=VatGraphic.AddTo(go);vat.raycastTarget=false;vat.PixelsPerUnit=100;
            vat.Load(HelperSkills.RoleVat(role.id,Model.HelperAwake(role.id)));vat.Play("idle_front",true);
        }
    }
}

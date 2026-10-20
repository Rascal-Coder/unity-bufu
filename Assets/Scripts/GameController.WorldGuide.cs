using System.Collections;
using Bufu.Rebuild.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    public sealed partial class GameController {
        UiView worldUnlockGuide;bool worldPageGuideStarted;Coroutine worldPageGuideRoutine;

        void UpdateWorldUnlockGuide() {
            if(!interfaceReady||VerificationMode||Model.OpenAccess||GuideActive||Model.Data.world!=1||ModalOpen||CutscenePlaying)return;
            if(worldUnlockGuide!=null&&worldUnlockGuide.Root)return;
            if(PlayerPrefs.GetInt(PrefKey("IsUnlockDiceWorld"),0)!=0||Model.MainLevel<=Model.Factor(59)||Model.Data.worlds[1].lifetime>0)return;
            const string cls="DiceWolrdGuide";
            var v=OpenPopup("Common/diceWorldGuide");worldUnlockGuide=v;
            var entry=v.Ref<RectTransform>(cls,"enterWorldBtn");var icon=v.Ref<RectTransform>(cls,"tubiao");
            var target=upgrade.Ref<RectTransform>("UpgradePanel","button6");
            entry.position=target.position;icon.position=target.position;
            entry.sizeDelta=target.rect.size;
            var entryGraphic=AttachWheelLight(entry.gameObject,"Uieff_common_guide","dianji");
            if(entryGraphic){entryGraphic.raycastTarget=true;entry.GetComponent<Button>().targetGraphic=entryGraphic;}
            AttachWheelLight(v.Ref(cls,"guideFinger"),"Uieff_common_guide","dianji");
            AttachWheelLight(v.Nodes[15],"Uieff_common_guide","dianji");
            On(v.Ref(cls,"guideEnterDiceWorld"),false);On(v.Ref(cls,"guideChangeWorld"),false);
            Click(entry.gameObject,()=>{
                On(v.Ref(cls,"mask1"),false);On(v.Ref(cls,"bg1"),false);On(entry.gameObject,false);On(icon.gameObject,false);
                SelectTab(6);On(v.Ref(cls,"guideEnterDiceWorld"),true);
                On(v.Ref(cls,"enterDiceWorldBtn"),false);On(v.Ref(cls,"guideFinger"),false);
                StartCoroutine(PlaceDiceIslandGuide(v));
            });
            Click(v.Ref<Button>(cls,"enterDiceWorldBtn"),()=>On(v.Ref(cls,"guideChangeWorld"),true));
            Click(v.Ref<Button>(cls,"changeworldBtn"),()=>{
                if(CutscenePlaying)return;
                ClosePopup(v);worldUnlockGuide=null;
                PlayWorldCutscene(()=>{
                    if(!Model.SwitchWorld(2))return;
                    PlayerPrefs.SetInt(PrefKey("IsUnlockDiceWorld"),1);PlayerPrefs.Save();
                    RebuildActors();SelectTab(1);if(!VerificationMode)SaveStore.Save(Model);
                    if(Model.State.pendingOffline>0)ShowOffline();
                });
            });
        }
        IEnumerator PlaceDiceIslandGuide(UiView v) {
            yield return new WaitForSeconds(2.5f);
            if(!v.Root||worldView==null||!worldView.Root)yield break;
            var content=worldView.Ref<ScrollRect>("WorldSelectionPanel","worldScrollView").content;
            if(content.childCount<2)yield break;
            var island=content.GetChild(1).GetComponentInChildren<Button>(true);
            if(!island)yield break;
            var button=v.Ref<RectTransform>("DiceWolrdGuide","enterDiceWorldBtn");
            var finger=v.Ref<RectTransform>("DiceWolrdGuide","guideFinger");
            button.position=island.transform.position;finger.position=island.transform.position;
            var original=(RectTransform)island.transform;
            button.sizeDelta=new Vector2(original.rect.width*Mathf.Abs(original.lossyScale.x/button.lossyScale.x),original.rect.height*Mathf.Abs(original.lossyScale.y/button.lossyScale.y));
            On(button.gameObject,true);On(finger.gameObject,true);
        }
        void UpdateWorldPageGuide(UiView v) {
            if(tab!=6||worldPageGuideStarted||VerificationMode)return;
            worldPageGuideStarted=true;
            if(PlayerPrefs.GetInt(PrefKey("HasShownWorldSelectionGuide"),0)!=0)return;
            PlayerPrefs.SetInt(PrefKey("HasShownWorldSelectionGuide"),1);PlayerPrefs.Save();
            if(Model.Data.world>=2)return;
            var scroll=v.Ref<ScrollRect>("WorldSelectionPanel","worldScrollView");
            var snap=scroll.GetComponent<WorldPageSnap>();if(snap)snap.ScrollToIndex(1,.65f);
            if(worldPageGuideRoutine!=null)StopCoroutine(worldPageGuideRoutine);
            worldPageGuideRoutine=StartCoroutine(PlayWorldPageGesture(v));
        }
        IEnumerator PlayWorldPageGesture(UiView view) {
            var node=view.Ref("WorldSelectionPanel","guideAnimationObj");
            var graphic=AttachWheelLight(node,"Uieff_common_guide","shangxiatuodong");
            if(!graphic)yield break;
            On(node,true);
            // WorldSelectionPanel plays this gesture twice on the first visit.
            yield return new WaitForSecondsRealtime(Mathf.Max(.01f,graphic.AnimationDuration)*2);
            if(node)On(node,false);worldPageGuideRoutine=null;
        }
    }
}

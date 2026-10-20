using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Bufu.Rebuild {
    public sealed partial class GameController {
        const float SceneCoinClickRadius=50f,SceneRoleClickRadius=90f,SceneClickMaxScreenDistance=20f;
        bool scenePointerTracking,scenePointerBlocked;Vector2 scenePointerStart;
        int sceneTouchFinger=-1,sceneHandledClickFrame=-1,sceneSwipeFrame=-1;
        readonly List<RaycastResult> sceneInputHits=new List<RaycastResult>();
        readonly HashSet<CoinActor> sceneSwipedCoins=new HashSet<CoinActor>();
        bool SceneInputAvailable=>interfaceReady&&Model!=null&&Field&&tab==1&&!IsBackgrounded&&!CutscenePlaying&&!ModalOpen;
        Camera SceneInputCamera {
            get {var canvas=Field?Field.GetComponentInParent<Canvas>():null;return canvas&&canvas.renderMode!=RenderMode.ScreenSpaceOverlay?canvas.worldCamera:null;}
        }

        // MainSceneVat.CheckMouseToThrow/CheckTouchToThrow. uGUI actor callbacks never throw,
        // so the same touch cannot be handled both here and by a rectangular actor raycast.
        void UpdateSceneInput() {
            if(!SceneInputAvailable){CancelScenePointer();sceneTouchFinger=-1;return;}
            if(Input.touchCount>0) {
                if(sceneTouchFinger<0)for(int i=0;i<Input.touchCount;i++) {
                    var touch=Input.GetTouch(i);if(touch.phase!=TouchPhase.Began)continue;
                    sceneTouchFinger=touch.fingerId;BeginScenePointer(touch.position);break;
                }
                for(int i=0;i<Input.touchCount;i++) {
                    var touch=Input.GetTouch(i);if(touch.fingerId!=sceneTouchFinger)continue;
                    if(touch.phase==TouchPhase.Moved||touch.phase==TouchPhase.Stationary)MoveScenePointer(touch.position);
                    else if(touch.phase==TouchPhase.Ended){EndScenePointer(touch.position);sceneTouchFinger=-1;}
                    else if(touch.phase==TouchPhase.Canceled){CancelScenePointer();sceneTouchFinger=-1;}
                    break;
                }
                return;
            }
            if(sceneTouchFinger>=0){CancelScenePointer();sceneTouchFinger=-1;return;}
            if(Input.GetMouseButtonDown(0))BeginScenePointer(Input.mousePosition);
            else if(Input.GetMouseButtonUp(0))EndScenePointer(Input.mousePosition);
            else if(Input.GetMouseButton(0))MoveScenePointer(Input.mousePosition);
        }
        public void BeginScenePointer(Vector2 screenPosition) {
            scenePointerStart=screenPosition;scenePointerTracking=true;
            scenePointerBlocked=!SceneInputAvailable||ScenePointerBlockedByUi(screenPosition);
        }
        public void MoveScenePointer(Vector2 screenPosition) {
            if(!scenePointerTracking||scenePointerBlocked||!SceneInputAvailable||Vector2.Distance(screenPosition,scenePointerStart)<=SceneClickMaxScreenDistance||ScenePointerBlockedByUi(screenPosition))return;
            if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(Field,screenPosition,SceneInputCamera,out var local))return;
            if(sceneSwipeFrame!=Time.frameCount){sceneSwipeFrame=Time.frameCount;sceneSwipedCoins.Clear();}
            CoinActor nearest=null;float best=SceneCoinClickRadius*SceneCoinClickRadius;
            foreach(var coin in Coins) {
                if(!coin||coin.World!=Model.Data.world||coin.Busy||sceneSwipedCoins.Contains(coin)||!Model.CanSwipe(coin.Quality))continue;
                float distance=((Vector2)Field.InverseTransformPoint(coin.Rect.position)-local).sqrMagnitude;
                if(distance>SceneCoinClickRadius*SceneCoinClickRadius)continue;
                if(Model.Data.world==2){if(distance<=best){best=distance;nearest=coin;}}
                else if(coin.Throw(true))sceneSwipedCoins.Add(coin);
            }
            // DiceWorldController.TrySwipeDiceAt selects one nearest eligible die per sample.
            if(nearest&&nearest.Throw(true))sceneSwipedCoins.Add(nearest);
        }
        public void EndScenePointer(Vector2 screenPosition) {
            bool click=scenePointerTracking&&!scenePointerBlocked&&SceneInputAvailable&&Vector2.Distance(screenPosition,scenePointerStart)<=SceneClickMaxScreenDistance;
            CancelScenePointer();
            if(!click||sceneHandledClickFrame==Time.frameCount||ScenePointerBlockedByUi(screenPosition))return;
            sceneHandledClickFrame=Time.frameCount;
            if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(Field,screenPosition,SceneInputCamera,out var local))return;
            CoinActor nearest=null;float best=SceneCoinClickRadius*SceneCoinClickRadius;
            foreach(var coin in Coins) {
                if(!coin||coin.World!=Model.Data.world||coin.Busy)continue;
                float distance=((Vector2)Field.InverseTransformPoint(coin.Rect.position)-local).sqrMagnitude;
                if(distance<=best){best=distance;nearest=coin;}
            }
            if(nearest&&nearest.Throw(true))return;
            HelperActor sleeper=null;best=SceneRoleClickRadius*SceneRoleClickRadius;
            foreach(var helper in HelperActors) {
                if(!helper||!helper.Sleeping)continue;
                float distance=((Vector2)Field.InverseTransformPoint(helper.Rect.position)-local).sqrMagnitude;
                if(distance<=best){best=distance;sleeper=helper;}
            }
            if(sleeper)sleeper.OnPointerClick(null);
        }
        public void CancelScenePointer(){scenePointerTracking=false;scenePointerBlocked=false;}
        bool ScenePointerBlockedByUi(Vector2 position) {
            if(!EventSystem.current)return false;
            var pointer=new PointerEventData(EventSystem.current){position=position};
            sceneInputHits.Clear();EventSystem.current.RaycastAll(pointer,sceneInputHits);
            foreach(var hit in sceneInputHits) {
                if(!hit.gameObject)continue;
                var target=hit.gameObject.transform;
                // Outer is a reconstruction backdrop behind the original scene, not a control.
                // Raycasts are sorted front to back: once the scene itself is reached, lower
                // background graphics cannot block its circular input search.
                if(target.parent==canvasRect&&target.name=="Outer")continue;
                if(mainScene!=null&&mainScene.Root&&(target==mainScene.Root.transform||target.IsChildOf(mainScene.Root.transform)))return false;
                return true;
            }
            return false;
        }
    }
}

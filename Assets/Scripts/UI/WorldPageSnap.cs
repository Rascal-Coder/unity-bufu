using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    /// <summary>WorldSelectionPanel's drag-end paging, after ScrollRect's inertia slows down.</summary>
    public sealed class WorldPageSnap:MonoBehaviour,IBeginDragHandler,IEndDragHandler {
        ScrollRect scroll;RectTransform[] pages;bool dragging,pending,hasTarget;float target,velocity,smoothTime=.14f;
        public void Configure(ScrollRect view,RectTransform[] nodes){scroll=view;pages=nodes;pending=hasTarget=false;velocity=0;}
        public void OnBeginDrag(PointerEventData data){dragging=true;pending=hasTarget=false;velocity=0;smoothTime=.14f;}
        public void OnEndDrag(PointerEventData data){dragging=false;pending=true;hasTarget=false;velocity=0;smoothTime=.14f;}
        float PagePosition(RectTransform page) {
            float center=-page.anchoredPosition.y+page.rect.height*page.pivot.y-page.rect.height*.5f;
            return Mathf.Clamp(center-scroll.viewport.rect.height*.5f,0,Mathf.Max(0,scroll.content.rect.height-scroll.viewport.rect.height));
        }
        public void ScrollToIndex(int index,float duration) {
            if(!scroll||pages==null||index<0||index>=pages.Length)return;
            target=PagePosition(pages[index]);smoothTime=duration;velocity=0;pending=hasTarget=true;scroll.StopMovement();
        }
        void OnDisable(){dragging=pending=hasTarget=false;velocity=0;}
        void LateUpdate() {
            if(dragging||!pending||!scroll||pages==null||pages.Length==0)return;
            if(!hasTarget) {
                if(Mathf.Abs(scroll.velocity.y)>40)return;
                float nearest=float.PositiveInfinity;target=scroll.content.anchoredPosition.y;
                foreach(var page in pages) {
                    if(!page)continue;
                    float y=PagePosition(page);
                    float gap=Mathf.Abs(y-scroll.content.anchoredPosition.y);
                    if(gap<nearest){nearest=gap;target=y;}
                }
                scroll.StopMovement();hasTarget=true;
            }
            var position=scroll.content.anchoredPosition;
            position.y=Mathf.SmoothDamp(position.y,target,ref velocity,smoothTime,float.PositiveInfinity,Time.unscaledDeltaTime);
            if(Mathf.Abs(position.y-target)<.5f){position.y=target;pending=hasTarget=false;}
            scroll.content.anchoredPosition=position;
        }
    }
}

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bufu.Rebuild.UI {
    // Animate the illustration only: the button's rectangle and caption never move.
    public sealed class ShopOfferMotion:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerDownHandler,IPointerUpHandler,ISelectHandler,IDeselectHandler {
        public RectTransform Icon {get;private set;}
        public bool ReduceMotion {get;set;}
        Vector2 restingPosition;float phase,started;bool hovering,pressed;Outline focus;
        public void Initialize(RectTransform icon,float offset,Outline focusOutline) {
            Icon=icon;restingPosition=icon.anchoredPosition;phase=offset;focus=focusOutline;started=Time.unscaledTime;ReduceMotion=ReducedMotionRequested();
        }
        void OnEnable(){started=Time.unscaledTime;hovering=pressed=false;}
        void OnDisable(){hovering=pressed=false;if(focus)focus.enabled=false;ResetIcon();}
        void ResetIcon(){if(!Icon)return;Icon.anchoredPosition=restingPosition;Icon.localScale=Vector3.one;Icon.localRotation=Quaternion.identity;}
        void Update() {
            if(!Icon)return;
            if(ReduceMotion){ResetIcon();return;}
            float elapsed=Time.unscaledTime-started,beat=Mathf.Repeat(elapsed-phase+8,8);
            float emphasis=beat<1.15f?Mathf.Sin(beat/1.15f*Mathf.PI):0;
            float target=pressed?.97f:1+emphasis*.065f+(hovering?.035f:0);
            float scale=Mathf.Lerp(Icon.localScale.x,target,1-Mathf.Exp(-22*Time.unscaledDeltaTime));Icon.localScale=Vector3.one*scale;
            Icon.anchoredPosition=restingPosition+Vector2.up*(4*Mathf.Sin(elapsed*1.7f+phase)+emphasis*7);
            Icon.localRotation=Quaternion.Euler(0,0,2*Mathf.Sin(beat*6)*emphasis);
        }
        public void OnPointerEnter(PointerEventData e){hovering=true;}
        public void OnPointerExit(PointerEventData e){hovering=pressed=false;}
        public void OnPointerDown(PointerEventData e){if(e.button==PointerEventData.InputButton.Left)pressed=true;}
        public void OnPointerUp(PointerEventData e){pressed=false;}
        public void OnSelect(BaseEventData e){if(focus)focus.enabled=true;}
        public void OnDeselect(BaseEventData e){if(focus)focus.enabled=false;}
        static bool ReducedMotionRequested() {
            if(PlayerPrefs.GetInt("ui_reduce_motion",0)!=0)return true;
#if UNITY_STANDALONE_WIN
            try {bool enabled=true;if(SystemParametersInfo(0x1042,0,ref enabled,0))return !enabled;}catch(Exception){}
#endif
            return false;
        }
#if UNITY_STANDALONE_WIN
        [DllImport("user32.dll",SetLastError=true)]
        [return:MarshalAs(UnmanagedType.Bool)]
        static extern bool SystemParametersInfo(uint action,uint param,[MarshalAs(UnmanagedType.Bool)]ref bool value,uint flags);
#endif
    }
}

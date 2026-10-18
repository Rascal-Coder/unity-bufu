using System.Collections;
using Bufu.Rebuild.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    /// <summary>
    /// Original PreloadManager bar: 20% when the page opens, then the real boot steps fill the rest.
    /// The drawn value eases toward the real one, so a fast device still sees the bar travel.
    /// </summary>
    public sealed class LaunchScreen:MonoBehaviour {
        public static LaunchScreen Current {get;private set;}
        public static bool HoldForIntro;
        public Transform PageRoot {get;private set;}
        public const float BaseProgress=.2f;
        // Bar travel per second: 20% to 100% takes at least one second.
        const float FillSpeed=.8f;
        // Advisory title starts about 500 above the art's bottom edge; this keeps the bar clear of it.
        const float ProgressAboveArtBottom=680f;
        Image bar;RectTransform cat;TMP_Text percent;CanvasGroup group;
        float catLeft,catRight,shown,target;

        public static void Show() {
            if(Current)return;
            // Original bootstrap scene: one persistent Canvas, 1440×2880, match width. Loading is its child.
            var canvasObject=new GameObject("Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            DontDestroyOnLoad(canvasObject);
            var canvas=canvasObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=99;
            var scaler=canvasObject.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1440f,2880f);scaler.matchWidthOrHeight=0f;
            var screen=canvasObject.AddComponent<LaunchScreen>();
            var view=UiPrefab.Instantiate("loading/Loading",canvasObject.transform);
            GameController.Stretch(view.Rect);
            // The advisory is painted into the bottom of bg, which stays 2880 tall and centred on taller screens.
            // Pin the bar to the art instead of the screen edge so it sits above the advisory on every aspect.
            var progress=(RectTransform)view.Node("progress").transform;
            progress.SetParent(view.Node("bg").transform,false);
            progress.anchoredPosition=new Vector2(0f,ProgressAboveArtBottom);
            screen.bar=view.Node("bar").GetComponent<Image>();
            screen.bar.type=Image.Type.Filled;screen.bar.fillMethod=Image.FillMethod.Horizontal;screen.bar.fillAmount=0f;
            screen.cat=(RectTransform)view.Node("cat").transform;
            screen.percent=view.Node("Text").GetComponent<TMP_Text>();
            float width=screen.bar.rectTransform.rect.width;
            if(width<10f)width=screen.bar.rectTransform.sizeDelta.x;
            if(width<10f)width=1140f;
            screen.catLeft=-width*.5f;screen.catRight=width*.5f;
            screen.PageRoot=view.Root.transform;
            screen.group=view.Root.GetComponent<CanvasGroup>()??view.Root.AddComponent<CanvasGroup>();
            Current=screen;
            screen.shown=screen.target=BaseProgress;
            screen.Apply(BaseProgress);
        }

        public static void SetProgress(float value){if(Current)Current.target=Mathf.Max(Current.target,Mathf.Clamp01(value));}
        /// <summary>Sets the bar to 100% and waits until it is drawn full.</summary>
        public static IEnumerator Complete() {
            SetProgress(1f);
            while(Current&&Current.shown<1f)yield return null;
            yield return null;
        }
        public static void Hide(){if(!Current)return;var go=Current.gameObject;Current=null;Destroy(go);}
        public static IEnumerator FadeAndHide(float duration=1.5f) {
            var screen=Current;if(!screen)yield break;
            // The game below is already live. Let touches through while the page fades.
            if(screen.group){screen.group.blocksRaycasts=false;screen.group.interactable=false;}
            float t=0f;duration=Mathf.Max(.01f,duration);float start=screen.group?screen.group.alpha:1f;
            while(t<duration&&screen&&screen.group) {
                t+=Time.unscaledDeltaTime;float p=Mathf.Clamp01(t/duration);
                float eased=p<.5f?8f*p*p*p*p:1f-8f*Mathf.Pow(p-1f,4f);
                screen.group.alpha=Mathf.Lerp(start,0f,eased);yield return null;
            }
            Hide();
        }

        void Update() {
            if(shown>=target)return;
            // Boot steps stall the main thread. Cap the step so one long frame does not make the bar jump.
            shown=Mathf.MoveTowards(shown,target,Mathf.Min(Time.unscaledDeltaTime,1f/30f)*FillSpeed);
            Apply(shown);
        }

        void Apply(float value) {
            value=Mathf.Clamp01(value);
            if(bar)bar.fillAmount=value;
            if(cat){var p=cat.anchoredPosition;p.x=Mathf.Lerp(catLeft,catRight,value);cat.anchoredPosition=p;}
            if(percent)percent.text=Mathf.RoundToInt(value*100f)+"%";
        }
    }
}

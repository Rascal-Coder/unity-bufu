using TMPro;
using UnityEngine;

namespace Bufu.Rebuild.UI {
    /// <summary>Keeps the original source so switching either way also updates already-open panels.</summary>
    [DisallowMultipleComponent]
    public sealed class LocalizedText:MonoBehaviour {
        TMP_Text target;string source,last;
        bool auto,wrap;float fontSize,min,max;
        HorizontalAlignmentOptions alignment;
        public string Source=>source;
        public static void Set(TMP_Text target,string source) {
            if(!target)return;
            var binding=target.GetComponent<LocalizedText>();
            if(!binding&&!L10n.IsEnglish&&!L10n.HasChinese(source)){if(target.text!=source)target.text=source;return;}
            if(!binding)binding=target.gameObject.AddComponent<LocalizedText>();
            if(binding.target==null)binding.Setup(target);
            if(binding.source==source&&binding.last==target.text)return;
            binding.source=source;binding.Refresh();
        }
        void Awake(){Setup(GetComponent<TMP_Text>());}
        void Setup(TMP_Text text) {
            if(target||!text)return;target=text;source=text.text;
            auto=text.enableAutoSizing;wrap=text.enableWordWrapping;fontSize=text.fontSize;min=text.fontSizeMin;max=text.fontSizeMax;
            alignment=text.horizontalAlignment;
            L10n.Changed+=Refresh;
        }
        void OnEnable(){if(target)Refresh();}
        void OnDestroy(){L10n.Changed-=Refresh;}
        void Refresh() {
            if(!target)return;
            if(last!=null&&target.text!=last)source=target.text;
            last=L10n.Translate(source);target.text=last;
            bool fit=L10n.IsEnglish;
            target.enableAutoSizing=fit||auto;target.fontSizeMax=fit?fontSize:max;
            target.fontSizeMin=fit?Mathf.Min(min,Mathf.Max(16,fontSize*.32f)):min;
            target.enableWordWrapping=wrap;
            target.horizontalAlignment=fit&&(alignment==HorizontalAlignmentOptions.Justified||alignment==HorizontalAlignmentOptions.Flush)?HorizontalAlignmentOptions.Left:alignment;
            if(!fit&&!auto)target.fontSize=fontSize;
        }
        void LateUpdate(){if(target&&target.text!=last){source=target.text;Refresh();}}
    }
}

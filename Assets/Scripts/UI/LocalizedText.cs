using TMPro;
using UnityEngine;

namespace Bufu.Rebuild.UI {
    /// <summary>Translates source strings to English and applies default auto-sizing on initial attachment.</summary>
    [DisallowMultipleComponent]
    public sealed class LocalizedText:MonoBehaviour {
        TMP_Text target;string source,last;
        public string Source=>source;
        public static void Set(TMP_Text target,string source) {
            if(!target)return;
            var binding=target.GetComponent<LocalizedText>();
            if(!binding)binding=target.gameObject.AddComponent<LocalizedText>();
            if(binding.target==null)binding.Setup(target);
            if(binding.source==source&&binding.last==target.text)return;
            binding.source=source;binding.Refresh();
        }
        void Awake(){Setup(GetComponent<TMP_Text>());}
        void Setup(TMP_Text text) {
            if(target||!text)return;target=text;source=text.text;
            float size=text.fontSize;
            text.enableAutoSizing=true;
            text.fontSizeMax=size;
            text.fontSizeMin=Mathf.Min(text.fontSizeMin,Mathf.Max(18f,size*.4f));
            if(text.horizontalAlignment==HorizontalAlignmentOptions.Justified||text.horizontalAlignment==HorizontalAlignmentOptions.Flush)
                text.horizontalAlignment=HorizontalAlignmentOptions.Left;
            L10n.Changed+=Refresh;
        }
        void OnEnable(){if(target)Refresh();}
        void OnDestroy(){L10n.Changed-=Refresh;}
        void Refresh() {
            if(!target)return;
            if(last!=null&&target.text!=last)source=target.text;
            last=L10n.Translate(source);target.text=last;
        }
        void LateUpdate(){if(target&&target.text!=last){source=target.text;Refresh();}}
    }
}

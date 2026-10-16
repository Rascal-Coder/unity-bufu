using Bufu.Rebuild.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    public sealed partial class GameController {
        void RefreshLanguage(){RefreshAll();RefreshSupportDialog();if(canvasRect)UiAssets.ApplyLanguage(canvasRect.gameObject);}
        void OnDestroy(){L10n.Changed-=RefreshLanguage;}

        void BuildLanguageSettings(UiView view) {
            var bg=(RectTransform)view.Node("AdBg").transform;bg.sizeDelta=new Vector2(bg.sizeDelta.x,965);bg.anchoredPosition=new Vector2(0,-160);
            var reset=(RectTransform)view.Nodes[25].transform;reset.anchoredPosition=new Vector2(7,-455);
            var row=NewRect("Language Settings",view.Rect);row.sizeDelta=new Vector2(830,110);row.anchoredPosition=new Vector2(0,-270);
            var title=LanguageLabel(row,"语言",new Vector2(200,100));title.rectTransform.anchoredPosition=new Vector2(-310,0);
            for(int i=0;i<2;i++) {
                string locale=i==0?"zh-CN":"en";
                var rect=NewRect("Language "+locale,row);rect.sizeDelta=new Vector2(260,100);rect.anchoredPosition=new Vector2(i==0?-35:250,0);
                var image=rect.gameObject.AddComponent<Image>();image.sprite=Sp("ty_btn_yellow01");image.type=Image.Type.Sliced;
                var button=rect.gameObject.AddComponent<Button>();button.targetGraphic=image;
                var label=LanguageLabel(rect,i==0?"简体中文":"English",new Vector2(235,90));
                // Native language names stay readable in either locale.
                Click(button,()=>L10n.SetLanguage(locale));
                Watch(view.Root,()=>image.color=L10n.Language==locale?Color.white:new Color(.65f,.65f,.65f,1));
            }
        }
        static TMP_Text LanguageLabel(Transform parent,string source,Vector2 size) {
            var rect=NewRect("Label",parent);rect.sizeDelta=size;
            var text=rect.gameObject.AddComponent<TextMeshProUGUI>();text.font=UiAssets.Font();text.fontSize=42;
            text.alignment=TextAlignmentOptions.Center;text.color=new Color(.32f,.22f,.12f);text.raycastTarget=false;
            LocalizedText.Set(text,source);return text;
        }
    }
}

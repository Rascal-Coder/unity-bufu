using System;
using Bufu.Rebuild.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    public sealed partial class GameController {
        GameObject developerPanel;

        void BuildDeveloperControls() {
            if(!Model.DeveloperMode)return;
            var badge=DeveloperButton(systemLayer,"Developer Mode","DEV  /  F1",new Vector2(230,64),ToggleDeveloperPanel);
            badge.anchorMin=badge.anchorMax=new Vector2(1,1);badge.anchoredPosition=new Vector2(-135,-42);
        }

        void RefillDeveloperResources() {
            if(!Model.RefillDeveloperResources())return;
            SaveStore.Save(Model);ShowToast("调试资源已补充，签到和进度宝箱已重置");
        }

        void ToggleDeveloperPanel() {
            if(!Model.DeveloperMode)return;
            if(developerPanel){popups.Remove(developerPanel);Destroy(developerPanel);return;}
            var overlay=NewRect("Developer Panel",popupLayer);Stretch(overlay);developerPanel=overlay.gameObject;
            overlay.gameObject.AddComponent<Image>().color=new Color(0,0,0,.8f);popups.Add(developerPanel);
            var panel=NewRect("Controls",overlay);panel.sizeDelta=new Vector2(1080,1000);
            panel.gameObject.AddComponent<Image>().color=new Color(.12f,.17f,.23f,1);
            DeveloperText(panel,"DEV · 独立调试存档",new Vector2(960,100),new Vector2(0,390));
            DeveloperText(panel,"全部现有内容已解锁\nF2 补资源 · F1 打开/关闭",new Vector2(960,150),new Vector2(0,260));
            DeveloperButton(panel,"Refill","补充资源 / 重置领取",new Vector2(880,115),RefillDeveloperResources).anchoredPosition=new Vector2(0,70);
            DeveloperButton(panel,"Switch World","切换硬币 / 骰子世界",new Vector2(880,115),()=>{
                if(Model.SwitchWorld(Model.Data.world==1?2:1)){RebuildActors();SelectTab(1);SaveStore.Save(Model);}
            }).anchoredPosition=new Vector2(0,-80);
            DeveloperButton(panel,"Reset Dev","重置开发存档（全部解锁）",new Vector2(880,115),ResetGame).anchoredPosition=new Vector2(0,-230);
            DeveloperButton(panel,"Close","关闭",new Vector2(880,115),ToggleDeveloperPanel).anchoredPosition=new Vector2(0,-380);
        }

        static void DeveloperText(Transform parent,string text,Vector2 size,Vector2 position) {
            var rect=NewRect("Label",parent);rect.sizeDelta=size;rect.anchoredPosition=position;
            var label=rect.gameObject.AddComponent<TextMeshProUGUI>();label.font=UiAssets.Font();label.fontSize=42;
            label.alignment=TextAlignmentOptions.Center;label.color=Color.white;label.text=text;label.raycastTarget=false;
        }

        RectTransform DeveloperButton(Transform parent,string name,string label,Vector2 size,Action action) {
            var rect=NewRect(name,parent);rect.sizeDelta=size;
            var image=rect.gameObject.AddComponent<Image>();image.color=new Color(.15f,.39f,.48f,1);
            var button=rect.gameObject.AddComponent<Button>();button.targetGraphic=image;Click(button,action);
            DeveloperText(rect,label,size,Vector2.zero);return rect;
        }
    }
}

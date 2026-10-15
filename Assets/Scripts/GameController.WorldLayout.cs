using Bufu.Rebuild.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    public sealed partial class GameController {
        const float WorldNodeHeight=1460;
        const string WorldHint="上下滑动，点击岛屿切换世界";

        void FitWorldPage(UiView view) {
            var scroll=view.Ref<ScrollRect>("WorldSelectionPanel","worldScrollView");
            Stretch((RectTransform)scroll.transform.parent);
            var rect=(RectTransform)scroll.transform;Stretch(rect);
            rect.offsetMin=new Vector2(32,420);rect.offsetMax=new Vector2(-32,-390);
            scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=100;
            var content=scroll.content;content.anchorMin=content.anchorMax=new Vector2(.5f,1);
            content.pivot=new Vector2(.5f,1);content.sizeDelta=new Vector2(1260,content.sizeDelta.y);
            var heading=NewRect("World Heading",view.Rect);
            WorldPlace(heading,view.Rect,new Vector2(0,-278),new Vector2(1040,90));
            WorldLabel(heading,"选择世界",64,new Color(.13f,.27f,.39f));
            WorldPlace(worldStatus.rectTransform,view.Rect,new Vector2(0,-350),new Vector2(1180,64));
            worldStatus.fontSize=36;worldStatus.fontSizeMax=36;worldStatus.color=new Color(.16f,.33f,.48f);
            Txt(worldStatus,WorldHint);On(view.Ref("WorldSelectionPanel","closeButton"),false);
            Canvas.ForceUpdateCanvases();
        }

        void FitWorldNode(UiView view,World config) {
            const string cls="WorldNode";var root=view.Rect;root.sizeDelta=new Vector2(1260,WorldNodeHeight);
            WorldPlace(view.Node("TitleBanner").transform as RectTransform,root,new Vector2(0,-110),new Vector2(720,190));
            var island=view.Ref<RectTransform>(cls,"islandButton");
            WorldPlace(island,root,new Vector2(0,-540),new Vector2(452,378));island.localScale=Vector3.one*1.7f;
            var image=view.Ref<Image>(cls,"islandImage");image.preserveAspect=true;
            var current=view.Node("currentImg").transform as RectTransform;
            WorldPlace(current,root,new Vector2(410,-320),new Vector2(260,156));
            var locked=view.Ref<RectTransform>(cls,"lockPanel");
            WorldPlace(locked,root,new Vector2(0,-1120),new Vector2(1060,210));
            var unlocked=view.Ref<RectTransform>(cls,"unlockPanel");
            WorldPlace(unlocked,root,new Vector2(0,-1140),new Vector2(1100,350));
            // Rebuild the small summary inside the original panel art. The exported
            // coin panel sits below its 1800-unit node, and the dice layout is rotated.
            foreach(Transform child in unlocked)child.gameObject.SetActive(false);
            if(config==null)return;
            var title=NewRect("Summary Heading",unlocked);
            title.anchoredPosition=new Vector2(config.id==2?155:0,105);title.sizeDelta=new Vector2(config.id==2?740:1000,80);
            WorldLabel(title,"道具",48,new Color(.13f,.27f,.39f));
            string[] icons=config.id==2?new[]{"world_icon_dice_copper_3","world_icon_dice_silver_5","world_icon_dice_gold_4","world_icon_dice_purple_3"}:new[]{"zgygy_coin_copper_2","zgygy_coin_silver_2","zgygy_coin_gold_2","zgygy_coin_purple_2"};
            if(config.id==2) {
                var helperTitle=NewRect("Helper Heading",unlocked);helperTitle.sizeDelta=new Vector2(300,80);helperTitle.anchoredPosition=new Vector2(-360,105);
                WorldLabel(helperTitle,"专属帮手",48,new Color(.13f,.27f,.39f));
                WorldIcon(unlocked,"World Helper","world_character_touziren",new Vector2(-360,-40),new Vector2(130,220));
            }
            for(int i=0;i<icons.Length;i++)WorldIcon(unlocked,"World Item "+i,icons[i],new Vector2((config.id==2?-100:-300)+i*(config.id==2?170:200),-40),new Vector2(135,135));
        }

        static void WorldPlace(RectTransform rect,Transform parent,Vector2 position,Vector2 size) {
            rect.SetParent(parent,false);rect.anchorMin=rect.anchorMax=new Vector2(.5f,1);rect.pivot=new Vector2(.5f,.5f);
            rect.localScale=Vector3.one;rect.localRotation=Quaternion.identity;rect.sizeDelta=size;rect.anchoredPosition=position;
        }
        static void WorldLabel(RectTransform rect,string text,float size,Color color) {
            var label=rect.gameObject.AddComponent<TextMeshProUGUI>();label.font=UiAssets.Font();label.text=text;
            label.fontSize=size;label.color=color;label.alignment=TextAlignmentOptions.Center;label.raycastTarget=false;
        }
        static void WorldIcon(Transform parent,string name,string key,Vector2 position,Vector2 size) {
            var rect=NewRect(name,parent);rect.sizeDelta=size;rect.anchoredPosition=position;
            var image=rect.gameObject.AddComponent<Image>();image.sprite=UiAssets.Sprite(key);image.preserveAspect=true;image.raycastTarget=false;image.enabled=image.sprite;
        }
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    public sealed partial class GameController {
        static Sprite WordArt(string key) {
            string asset="word-"+key+"-native-v4";
            var sprite=StoreIllustration("art:"+asset);if(sprite)sprite.name=asset;return sprite;
        }

        // Retain the semantic/live text binding; only its static visual is replaced.
        static void WordArtLabel(TMP_Text text,string key,float height,float inset=8) {
            var sprite=WordArt(key);if(!text||!sprite)return;
            var existing=text.transform.Find("Word Art");
            var rect=existing?(RectTransform)existing:NewRect("Word Art",text.transform);
            rect.anchorMin=new Vector2(0,.5f);rect.anchorMax=new Vector2(1,.5f);
            rect.anchoredPosition=Vector2.zero;rect.sizeDelta=new Vector2(-inset*2,height);
            var image=rect.GetComponent<Image>();if(!image)image=rect.gameObject.AddComponent<Image>();
            image.sprite=sprite;image.preserveAspect=true;image.raycastTarget=false;
            text.enabled=false;
        }

        static bool ButtonWordArt(TMP_Text label,string caption) {
            string key=caption=="Back"?"back":caption=="Challenge"?"challenge":caption=="Auction"?"auction":caption=="Gift Packs"?"gift-packs":caption=="Cloud save"?"cloud-save":null;
            if(key==null||!WordArt(key))return false;
            WordArtLabel(label,key,caption=="Back"?66:caption=="Challenge"||caption=="Auction"?88:66,28);return true;
        }
    }
}

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
        static void PurchaseButtonCopy(TMP_Text label,string state) {
            string key=state=="Unavailable"?"unavailable":state=="Owned"?"owned":state=="Payment pending"?"pending":state=="Please wait..."?"please-wait":null;
            var art=label.transform.Find("Word Art");
            if(key!=null&&WordArt("purchase-"+key)) {
                if(art)art.gameObject.SetActive(true);
                WordArtLabel(label,"purchase-"+key,60,64);
            } else {
                if(art)art.gameObject.SetActive(false);
                label.enabled=true; // Live localized billing prices must never be baked into art.
            }
        }
        // Reuse the game's original ty_num_both white PNG glyphs with proportional advances.
        // Keep the underlying localized text as the semantic quantity value.
        static void StoreNumberArt(TMP_Text label,float height) {
            string value=label.text;int[] digitWidths={46,33,44,42,47,43,45,43,45,45};
            var row=NewRect("Number Art",label.transform);row.anchorMin=row.anchorMax=new Vector2(.5f,.5f);
            float units=0;
            foreach(char c in value)units+=c>='0'&&c<='9'?digitWidths[c-'0']+8:c=='x'||c=='X'?38:c=='+'?54:22;
            height=Mathf.Min(height,label.rectTransform.rect.width*80/units);float width=units*height/80;
            row.sizeDelta=new Vector2(width,height);float x=-width*.5f;
            foreach(char c in value) {
                int glyph=c>='0'&&c<='9'?40+c-'0':c=='+'?39:c=='x'||c=='X'?74:c=='.'?50:-1;
                float advance=(c>='0'&&c<='9'?digitWidths[c-'0']+8:c=='x'||c=='X'?38:c=='+'?54:22)*height/80;
                var rect=NewRect("Glyph "+c,row);rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.anchoredPosition=new Vector2(x+advance*.5f,0);
                if(glyph>=0) {
                    float scale=c=='x'||c=='X'?.62f:1;
                    rect.sizeDelta=new Vector2(height*.8f*scale,height*scale);
                    var image=rect.gameObject.AddComponent<Image>();image.sprite=Sp("ty_num_both_"+glyph);image.preserveAspect=true;image.raycastTarget=false;
                    var edge=rect.gameObject.AddComponent<Outline>();edge.effectColor=StoreInk;edge.effectDistance=new Vector2(4,-4);
                } else {
                    rect.sizeDelta=new Vector2(advance,height);
                    var separator=rect.gameObject.AddComponent<TextMeshProUGUI>();separator.font=label.font;separator.text=c.ToString();separator.fontSize=height*.85f;separator.alignment=TextAlignmentOptions.Midline;separator.color=Color.white;separator.raycastTarget=false;separator.outlineColor=StoreInk;separator.outlineWidth=.18f;
                }
                x+=advance;
            }
            label.enabled=false;
        }
    }
}

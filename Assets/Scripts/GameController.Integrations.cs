using Bufu.Rebuild.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    public sealed partial class GameController {
        // The shipped resource package is pinned to Reference/package-manifest.json.
        // This is the bundled content version; no remote version check is simulated.
        internal const string ResourcePackageVersion="2026-10-14-626";

        void ShowIntegrationUnavailable(string feature) {
            string message=feature=="Cloud save"
                ?"Cloud saving and recovery are unavailable in this build.\nYour progress is saved automatically on this device."
                :feature=="Purchases"
                    ?"Purchases are unavailable in this build.\nNo payment will be taken."
                    :feature=="Leaderboard"
                        ?"The online leaderboard is unavailable in this build."
                        :feature=="Invite friends"
                            ?"Friend invitations and assistance rewards are unavailable in this build."
                            :"Platform subscriptions are unavailable in this build.";
            ShowDialog(message,null);
        }
        void BindIntegrationPlaceholders() {
            var invite=top.Ref<Button>("TopPanel","inviteBtn");invite.onClick.RemoveAllListeners();On(invite.gameObject,false);
            var rank=top.Ref<Button>("TopPanel","rankBtn");
            IntegrationIconCaption(rank,"Rankings");Click(rank,()=>ShowIntegrationUnavailable("Leaderboard"));
            var shortcuts=top.Ref<RectTransform>("TopPanel","permanentPanel");
            shortcuts.anchoredPosition=new Vector2(-365,shortcuts.anchoredPosition.y);
            rank.GetComponent<RectTransform>().anchoredPosition=new Vector2(230,-9);
            Watch(top.Root,()=>{
                On(top.Ref("TopPanel","rankBtn"),Model.LevelAbove(29));
            });
        }
        void IntegrationIconCaption(Button button,string caption) {
            // The native icon includes Chinese lettering in its bottom ribbon. Cover only
            // that ribbon, preserving the ranking podium and invitation character above it.
            var rect=NewRect("English Icon Caption",button.transform);rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
            rect.anchoredPosition=new Vector2(0,-70);rect.sizeDelta=new Vector2(200,62);
            var background=rect.gameObject.AddComponent<Image>();background.sprite=FortuneButton();background.type=Image.Type.Sliced;background.raycastTarget=false;
            var label=NewRect("Label",rect);Stretch(label);var text=label.gameObject.AddComponent<TextMeshProUGUI>();text.font=UiAssets.Font();
            text.fontSize=36;text.enableAutoSizing=true;text.fontSizeMin=20;text.fontSizeMax=36;text.alignment=TextAlignmentOptions.Center;text.color=FortuneInk;text.raycastTarget=false;Txt(text,caption);
        }
        void IntegrationButtonLabel(Button button,string caption) {
            foreach(Transform child in button.transform)child.gameObject.SetActive(false);
            var image=button.GetComponent<Image>();var sprite=FortuneButton();
            if(image&&sprite){image.sprite=sprite;image.type=Image.Type.Sliced;}
            var rect=NewRect("Availability Label",button.transform);Stretch(rect);
            var text=rect.gameObject.AddComponent<TextMeshProUGUI>();text.font=UiAssets.Font();
            text.fontSize=32;text.enableAutoSizing=true;text.fontSizeMin=20;text.fontSizeMax=32;
            text.alignment=TextAlignmentOptions.Center;text.color=FortuneInk;text.raycastTarget=false;Txt(text,caption);
            ButtonWordArt(text,caption);
        }
        void BindSettingsIntegrations(UiView view) {
            var cloud=view.Ref<Button>("SetUpPanel","restoreButton");On(cloud.gameObject,true);
            IntegrationButtonLabel(cloud,"Cloud save");Click(cloud,()=>ShowIntegrationUnavailable("Cloud save"));
            var rect=NewRect("Purchase Store Entry",view.Root.transform);rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
            rect.anchoredPosition=new Vector2(7,-470);rect.sizeDelta=new Vector2(790,108);
            var image=rect.gameObject.AddComponent<Image>();var purchases=rect.gameObject.AddComponent<Button>();purchases.targetGraphic=image;
            IntegrationButtonLabel(purchases,"Gift Packs");Click(purchases,()=>{ClosePopup(view);ShowPurchaseStore();});
            var background=view.Get<RectTransform>("AdBg");background.sizeDelta=new Vector2(background.sizeDelta.x,1100);background.anchoredPosition=new Vector2(background.anchoredPosition.x,-180);
            var version=view.Ref<RectTransform>("SetUpPanel","gameVersion");version.anchoredPosition=new Vector2(0,-610);version.sizeDelta=new Vector2(800,64);
        }
        void BindSubscriptionPlaceholder(UiView view) {
            var entry=view.Ref("DailyCheckIn","subscribeRect");
            On(entry,!Model.HelperOwned(4));
            var button=view.Ref<Button>("DailyCheckIn","subscribeButton");
            IntegrationButtonLabel(button,"Subscribe (unavailable)");Click(button,()=>ShowIntegrationUnavailable("Subscriptions"));
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bufu.Rebuild.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    public sealed partial class RuntimeVerification {
        IEnumerator VerifyIntegrationPlaceholders() {
            Check(GameController.VerificationMode&&L10n.IsEnglish,"integrations: unavailable-entry checks use the isolated English verification model");
            ClearAlignmentGoals();Call("SelectTab",1);yield return null;
            // Pause earnings, including instantaneous skill tosses, so a wallet change cannot
            // be concealed by unrelated settlement while the actual UI buttons are exercised.
            var paused=game.Coins.Cast<Behaviour>().Concat(game.HelperActors.Cast<Behaviour>())
                .Concat(game.SmokePool.GetComponentsInChildren<SkillShot>(true).Cast<Behaviour>())
                .Concat(game.SmokePool.GetComponentsInChildren<HelperTossQueue>(true).Cast<Behaviour>()).Distinct().ToArray();
            var enabled=paused.Select(b=>b.enabled).ToArray();
            try {
                foreach(var component in paused)component.enabled=false;
                var gold=game.Model.Data.worlds.Select(w=>w.gold).ToArray();double diamonds=game.Model.Data.diamonds;
                var top=Field<UiView>("top");
                Click(top.Ref<Button>("TopPanel","setupBtn"),"integrations: open Settings through its actual top button");yield return null;
                var setup=LatestPopup("Popup/setupPanel");
                yield return VerifyUnavailableEntry(setup.Ref<Button>("SetUpPanel","restoreButton"),"Cloud save",
                    "Cloud saving and recovery are unavailable in this build.\nYour progress is saved automatically on this device.",gold,diamonds,setup.Root);
                var purchases=setup.Root.transform.Find("Purchase Store Entry");
                Check(purchases&&purchases.GetComponent<Button>(),"integrations: Settings contains its purchase store entry");
                Click(purchases.GetComponent<Button>(),"integrations: Settings opens the purchase store");yield return null;
                Check(Field<GameObject>("purchaseView")&&!Field<Button>("purchaseBuy").interactable,"integrations: store shows real products with checkout unavailable until connected");
                Call("CloseTopPopup");yield return null;
                Check(!setup.Root&&Field<int>("tab")==5&&game.Model.Data.diamonds==diamonds,"integrations: settings opens the Shop product page and returns to Lucky Spin without changing the wallet");
                Call("SelectTab",1);yield return null;
                Click(top.Ref<Button>("TopPanel","setupBtn"),"integrations: reopen Settings after visiting Shop");yield return null;
                setup=LatestPopup("Popup/setupPanel");
                CheckEnglish(setup.Root.transform,"integration settings");yield return Capture("14-placeholder-settings.png");
                Click(setup.Ref<Button>("SetUpPanel","backBtn"),"integrations: return from Settings");yield return null;
                Check(!setup.Root&&!game.ModalOpen,"integrations: Settings closes without leaving a blocking popup");
                var rank=top.Ref<Button>("TopPanel","rankBtn");
                Check(rank.gameObject.activeInHierarchy,"integrations: completed growth level exposes the original leaderboard entry");
                yield return VerifyUnavailableEntry(rank,"Leaderboard","The online leaderboard is unavailable in this build.",gold,diamonds,null);
                Check(Field<int>("tab")==1&&!game.ModalOpen,"integrations: leaderboard placeholder returns directly to the item page");
            } finally {
                for(int i=0;i<paused.Length;i++)if(paused[i])paused[i].enabled=enabled[i];
            }
        }

        IEnumerator VerifyUnavailableEntry(Button entry,string feature,string expected,double[] gold,double diamonds,GameObject returnPopup) {
            var before=Field<List<GameObject>>("popups").Where(p=>p).ToArray();
            Click(entry,"integrations: click the actual "+feature+" entry");yield return null;
            var added=Field<List<GameObject>>("popups").Where(p=>p).Except(before).ToArray();
            Check(added.Length==1,"integrations: "+feature+" opens exactly one availability dialog");
            var dialog=added[0];
            var message=PrefabRef<TMP_Text>(dialog,"Common/commonDialog","CommonDialog","dialogText");
            Check(message.text==expected&&message.text.IndexOf("unavailable",StringComparison.OrdinalIgnoreCase)>=0&&!L10n.HasChinese(message.text),
                "integrations: "+feature+" clearly reports its English unavailable state");
            Check(dialog.GetComponentsInChildren<ScrollRect>(true).Length==0,"integrations: "+feature+" dialog has no fabricated leaderboard or store list");
            Check(game.Model.Data.worlds.Select(w=>w.gold).SequenceEqual(gold)&&game.Model.Data.diamonds==diamonds,
                "integrations: opening "+feature+" changes neither world's gold nor diamonds");
            Click(PrefabRef<Button>(dialog,"Common/commonDialog","CommonDialog","confirmButton"),"integrations: dismiss "+feature+" unavailable dialog");yield return null;
            Check(!dialog&&Field<List<GameObject>>("popups").Where(p=>p).SequenceEqual(before),"integrations: "+feature+" dismissal restores the previous popup stack");
            Check(!returnPopup||returnPopup.activeInHierarchy,"integrations: "+feature+" returns to its underlying menu");
            Check(game.Model.Data.worlds.Select(w=>w.gold).SequenceEqual(gold)&&game.Model.Data.diamonds==diamonds,
                "integrations: confirming "+feature+" changes neither world's gold nor diamonds");
        }
    }
}

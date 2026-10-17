using System.Collections;
using System.Linq;
using Bufu.Rebuild.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    public sealed partial class RuntimeVerification {
        IEnumerator VerifyHelperVisuals() {
            var actors=game.Field.GetComponentsInChildren<HelperActor>();
            foreach(var actor in actors)actor.enabled=false;
            foreach(Transform child in game.SmokePool)Destroy(child.gameObject);
            yield return null;
            var dice=actors.First(x=>x.RoleId==10);HelperSkills.Cast(dice,game.Coins.First());
            yield return new WaitForSeconds(.85f);Canvas.ForceUpdateCanvases();
            var shots=game.SmokePool.GetComponentsInChildren<SkillShot>();
            Check(shots.Length==8,"awakened dice helper emits eight real directional skill projectiles");
            Check(shots.All(shot=>shot.GetComponent<VatGraphic>()&&shot.GetComponent<VatGraphic>().Data.name=="zgygy_roleskill_dushen_1"&&shot.GetComponent<VatGraphic>().AnimationName=="effect"&&!shot.GetComponent<Image>()),"dice projectiles use the matching VAT animation instead of sprite-less white Image quads");
            Check(shots.All(shot=>shot.GetComponent<CanvasRenderer>().GetMesh().vertexCount>0),"each dice projectile produces actual textured VAT geometry");
            yield return Capture("06-dice-skill-fixed.png");
            foreach(var actor in actors.Where(x=>x.RoleId>1))HelperSkills.Cast(actor,game.Coins.First());
            for(int frame=0;frame<90;frame++) {
                CheckNoWhiteSkillImages();yield return null;
            }
            Check(true,"all nine premium helper skills run without white sprite-less images");
            // Exercise the defensive path as well as the real fixed asset.
            var missing=GameController.NewRect("Missing skill visual test",game.SmokePool);
            var image=missing.gameObject.AddComponent<Image>();missing.gameObject.AddComponent<FrameAnim>().Begin(image,"missing-skill-test-",0,false);
            Check(!image.enabled,"missing animation frames cannot render a white rectangle");Destroy(missing.gameObject);
            foreach(var actor in actors)if(actor)actor.enabled=true;
        }

        void CheckNoWhiteSkillImages() {
            if(game.SmokePool.GetComponentsInChildren<Image>().Any(image=>image.enabled&&image.color.a>0&&!image.sprite))throw new System.Exception("A live helper skill still renders a sprite-less white Image");
        }

        IEnumerator VerifyWorldLayout() {
            var upgrade=Field<UiView>("upgrade");
            Click(upgrade.Ref<Button>("UpgradePanel","button6"),"open improved world layout");yield return null;yield return null;
            var world=Field<UiView>("worldView");var scroll=world.Ref<ScrollRect>("WorldSelectionPanel","worldScrollView");
            var available=RootBounds(scroll.viewport);
            Check(available.yMin>=BottomNavigationTop()&&available.yMax<RootBounds(Field<TMP_Text>("goldTmp").rectTransform).yMin,"world viewport clears currency HUD and bottom navigation");
            var current=scroll.content.Cast<RectTransform>().First(node=>node.GetComponentsInChildren<TMP_Text>(true).Any(text=>text.text==L10n.Translate(game.Model.WorldName(game.Model.Data.world))));
            VerifyWorldCard(current,available,"initial current world");
            yield return Capture("07-world-current.png");
            yield return VerifyScroll(scroll,"world selection list",64);
            yield return Capture("08-world-bottom.png");
            foreach(int id in new[]{1,2}) {
                var node=scroll.content.Cast<RectTransform>().First(child=>child.GetComponentsInChildren<TMP_Text>(true).Any(text=>text.text==L10n.Translate(game.Model.WorldName(id))));
                CenterInScroll(scroll,node);yield return null;
                VerifyWorldCard(node,RootBounds(scroll.viewport),"world "+id);
                Check(node.Find("UnlockPanel")?node.Find("UnlockPanel").GetComponentsInChildren<Image>().All(image=>!image.enabled||image.sprite):node.GetComponentsInChildren<Image>().Any(image=>image.name=="World Item 0"&&image.sprite),"world "+id+" summary icons have real sprites");
                yield return Capture("09-world-"+id+".png");
            }
            Click(upgrade.Ref<Button>("UpgradePanel","button1"),"return from world layout");yield return null;
        }

        void VerifyWorldCard(RectTransform node,Rect available,string label) {
            foreach(string name in new[]{"TitleBanner","IslandButton","UnlockPanel"}) {
                var part=node.Find(name) as RectTransform;
                Check(part&&part.gameObject.activeInHierarchy,label+" has active "+name);
                var bounds=RootBounds(part);
                Check(bounds.xMin>=available.xMin-1&&bounds.xMax<=available.xMax+1&&bounds.yMin>=available.yMin-1&&bounds.yMax<=available.yMax+1,label+" shows the complete "+name+" inside its usable viewport");
            }
            var island=RootBounds((RectTransform)node.Find("IslandButton"));var summary=RootBounds((RectTransform)node.Find("UnlockPanel"));
            Check(island.yMin-summary.yMax>=0&&island.yMin-summary.yMax<=160,label+" keeps island and summary close without overlapping");
        }
    }
}

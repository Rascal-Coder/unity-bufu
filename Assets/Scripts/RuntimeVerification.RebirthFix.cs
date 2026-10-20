using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Bufu.Rebuild.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    public sealed partial class RuntimeVerification {
        IEnumerator VerifyAdvancedRebirthPreservation() {
            Check(GameController.VerificationMode,"advanced rebirth: fixture uses only the isolated verification model");
            var model=game.Model;
            Check(model.Data.world==1,"advanced rebirth: coin-world fixture follows the settlement journey");
            ClearAlignmentGoals();Call("SelectTab",1);
            model.State.quantity[model.BaseId+1]=Math.Max(2,model.Helpers);game.SyncActors();
            var helpers=game.HelperActors.ToArray();var coins=game.Coins.ToArray();
            Check(helpers.Length>=2&&coins.Length>0,"advanced rebirth: existing scene has helpers and a real VAT coin");
            var helperEnabled=helpers.Select(h=>h.enabled).ToArray();var coinEnabled=coins.Select(c=>c.enabled).ToArray();
            SkillShot[] skills=null;
            try {
                // Freeze progression across the real two-second ad and cutscene. A rebuild still
                // destroys these exact instances, so it cannot be hidden by the paused simulation.
                FreezeHelpers();foreach(var coin in coins)coin.enabled=false;
                game.SleepCount=3;
                typeof(HelperActor).GetMethod("EnterSleeping",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(helpers[0],null);
                helpers[0].Rect.anchoredPosition=new Vector2(-170,80);helpers[1].Rect.anchoredPosition=new Vector2(170,-80);
                var flight=coins.FirstOrDefault(c=>!c.Busy);
                Check(flight,"advanced rebirth: an existing idle coin is available for the in-flight fixture");
                Check(flight.Launch(game.ClampScene(flight.Rect.anchoredPosition+Vector2.right*120,40),false),"advanced rebirth: real coin launch starts before opening rebirth UI");

                // Select a fox cast on an existing actor without changing its saved roster or skin.
                // Cast creates the production prefab/projectile, not a placeholder marker object.
                var previousSkills=game.SmokePool.GetComponentsInChildren<SkillShot>(true);
                int role=helpers[1].RoleId;
                try {helpers[1].RoleId=7;HelperSkills.Cast(helpers[1],flight);}
                finally {helpers[1].RoleId=role;}
                skills=game.SmokePool.GetComponentsInChildren<SkillShot>(true).Except(previousSkills).ToArray();
                Check(skills.Length>0,"advanced rebirth: the real helper cast creates live skill objects");
                foreach(var skill in skills)skill.enabled=false;
                var positions=helpers.Select(h=>h.Rect.anchoredPosition).ToArray();var sleeping=helpers.Select(h=>h.Sleeping).ToArray();
                var coinPosition=flight.Rect.anchoredPosition;
                var quantities=(int[])model.State.quantity.Clone();var boosts=(int[])model.State.boosts.Clone();var upgrades=(int[])model.State.skills.Clone();
                int generation=Field<int>("actorGeneration");
                Action<string> preserved=phase=>{
                    Check(game.HelperActors.SequenceEqual(helpers)&&helpers.Select((h,i)=>h&&h.Rect.anchoredPosition==positions[i]&&h.Sleeping==sleeping[i]).All(x=>x),"advanced rebirth: "+phase+" preserves helper identities, positions and sleeping state");
                    Check(game.Coins.SequenceEqual(coins)&&flight&&flight.Busy&&flight.Rect.anchoredPosition==coinPosition,"advanced rebirth: "+phase+" preserves the same in-flight coin and its position");
                    Check(skills.All(s=>s&&!s.Finished&&s.transform.IsChildOf(game.SmokePool))&&Field<int>("actorGeneration")==generation,"advanced rebirth: "+phase+" preserves live skill objects without rebuilding the actor generation");
                };

                Check(model.State.rebornLevel<model.RebornTable.Length-1,"advanced rebirth: another unclaimed configured rebirth tier exists");
                int targetTier=Math.Min(model.RebornTable.Length-1,Math.Max(1,model.State.rebornLevel+1));
                model.State.gold=Math.Max(model.State.gold,model.RebornTable[targetTier].talentName);
                int points=model.RebornPoints(),before=model.State.points,rebirth=model.State.rebirth;
                Check(points>0,"advanced rebirth: fixture reaches an actual configured rebirth payout");
                Refresh();Call("SelectTab",3);yield return null;
                var talent=Field<UiView>("talentView");Click(talent.Ref<Button>("TalentCoin","rebornBtn"),"advanced rebirth: open the actual talent rebirth panel");yield return null;
                var host=talent.Ref<RectTransform>("TalentCoin","rebornPanel");
                Check(host.childCount>0,"advanced rebirth: original rebirth prefab is instantiated");
                var popup=host.GetChild(host.childCount-1).gameObject;
                Click(PrefabRef<Button>(popup,"Talent/talentRebornTipsItem","TalentRebornTip","rebornBtn"),"advanced rebirth: open the real confirmation choice");yield return null;
                Click(PrefabRef<Button>(popup,"Talent/talentRebornTipsItem","TalentRebornTip","adRebornBtn"),"advanced rebirth: choose preserve-level rebirth through its real button");
                var ad=Field<GameObject>("originalAd");
                Check(ad&&!AdCloseButton(ad).gameObject.activeSelf&&model.State.rebirth==rebirth&&model.State.points==before,"advanced rebirth: PC ad starts with no early rebirth or close button");
                preserved("during the advertisement");
                yield return new WaitForSecondsRealtime(2.15f);
                Check(ad&&AdCloseButton(ad).gameObject.activeSelf&&model.State.rebirth==rebirth&&model.State.points==before,"advanced rebirth: two-second completion still waits for explicit reward confirmation");
                Click(AdCloseButton(ad),"advanced rebirth: confirm the completed PC advertisement");
                yield return AlignmentWait(()=>game.CutscenePlaying,1,"advanced rebirth: the actual ad-success callback starts the cutscene");
                Check(model.State.points==before+points&&model.State.rebirth==rebirth+1,"advanced rebirth: ad-success grants configured points and one rebirth exactly once");
                Check(model.State.quantity.SequenceEqual(quantities)&&model.State.boosts.SequenceEqual(boosts)&&model.State.skills.SequenceEqual(upgrades),"advanced rebirth: the actual advanced path keeps quantity and upgrade levels");
                preserved("at cutscene start");
                yield return AlignmentWait(()=>!game.CutscenePlaying,4,"advanced rebirth: original cutscene finishes normally");yield return null;
                preserved("after the cutscene");
                Check(!popup&&!Field<GameObject>("originalAd")&&Field<int>("tab")==1&&!game.ModalOpen,"advanced rebirth: popup and ad close and the item page receives control");
                yield return Capture("13-advanced-rebirth-preserved-scene.png");
            } finally {
                for(int i=0;i<helpers.Length;i++)if(helpers[i])helpers[i].enabled=helperEnabled[i];
                for(int i=0;i<coins.Length;i++)if(coins[i])coins[i].enabled=coinEnabled[i];
                if(skills!=null)foreach(var skill in skills)if(skill)Destroy(skill.gameObject);
            }
        }
    }
}

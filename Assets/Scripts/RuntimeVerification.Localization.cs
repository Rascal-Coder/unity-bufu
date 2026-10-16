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
        static bool SameArtwork(Sprite current,Sprite original,bool english) {
            if(!current&&!original)return true;
            if(!current||!original||current.name!=original.name)return false;
            bool remade=UiAssets.HasEnglishArt(original.name);
            return remade&&english?current!=original:current==original;
        }
        void CheckEnglish(Transform root,string name) {
            var texts=root.GetComponentsInChildren<TMP_Text>().Where(t=>t.isActiveAndEnabled).ToArray();
            var untranslated=texts.Where(t=>L10n.HasChinese(t.text)&&!Hierarchy(t.gameObject).Contains("Language zh-CN")).Select(t=>Hierarchy(t.gameObject)+" = "+t.text).ToArray();
            Check(untranslated.Length==0,name+" has no untranslated Chinese: "+string.Join("; ",untranslated));
            foreach(var text in texts) {
                text.ForceMeshUpdate();
                var characters=text.GetParsedText().Where(c=>!char.IsControl(c)).ToArray();
                if(characters.Length>0)Check(text.font.HasCharacters(new string(characters)),name+" font covers "+text.gameObject.name+" = "+text.text);
            }
        }
        IEnumerator LocaleScreenshot(string name,Transform root) {
            yield return null;Canvas.ForceUpdateCanvases();CheckEnglish(root,name);yield return Capture(name+".png");
        }
        UiView LatestPopup(string resource) {
            var root=Field<List<GameObject>>("popups").Last(x=>x);
            // Runtime UI refs are not retained for every popup; resolve only its exported refs.
            var source=UiPrefab.Load("UI/Prefabs/"+resource);var nodes=source.Arr("nodes");
            var objects=new GameObject[nodes.Count];var components=new List<Dictionary<string,object>>[nodes.Count];objects[0]=root;
            for(int i=0;i<nodes.Count;i++) {
                var node=(Dictionary<string,object>)nodes[i];components[i]=(node.Arr("c")??new List<object>()).Cast<Dictionary<string,object>>().ToList();
                if(i>0)objects[i]=objects[node.I("p")].transform.Find(node.Str("n")).gameObject;
            }
            return new UiView(resource,objects,components);
        }
        IEnumerator VerifyLocalization() {
            var model=game.Model;Check(L10n.Language=="zh-CN","verification starts in Chinese independently of player preferences");
            model.State.mission=90;model.State.pendingGoals.Clear();model.State.gold=1e30;model.State.points=100;model.Data.diamonds=100000;Refresh();
            string savedLanguage=PlayerPrefs.GetString("bufu.language","<unset>");
            yield return Capture("00-chinese-main.png");
            var top=Field<UiView>("top");Click(top.Ref<Button>("TopPanel","setupBtn"),"open language settings");yield return null;
            var setup=LatestPopup("Popup/setupPanel");
            var english=setup.Root.transform.Find("Language Settings/Language en").GetComponent<Button>();
            var originalSprites=game.GetComponentsInChildren<Image>(true).ToDictionary(image=>image,image=>image.sprite);
            var particleSprites=game.GetComponentsInChildren<ParticleSystem>(true).Where(p=>p.textureSheetAnimation.mode==ParticleSystemAnimationMode.Sprites).SelectMany(p=>Enumerable.Range(0,p.textureSheetAnimation.spriteCount).Select(i=>new {system=p,index=i,sprite=p.textureSheetAnimation.GetSprite(i)})).ToArray();
            string progress=JsonUtility.ToJson(model.Data);Click(english,"select English");yield return null;
            Check(L10n.Language=="en"&&JsonUtility.ToJson(model.Data)==progress,"language switch preserves every saved progress value");
            Check(PlayerPrefs.GetString("bufu.language","<unset>")==savedLanguage,"verification never changes the player's saved language");
            Check(originalSprites.All(pair=>SameArtwork(pair.Key?pair.Key.sprite:null,pair.Value,true)),"English replaces remade image artwork and keeps every other sprite, including its name");
            Check(particleSprites.All(x=>SameArtwork(x.system?x.system.textureSheetAnimation.GetSprite(x.index):null,x.sprite,true)),"English replaces remade particle words and keeps every other particle sprite");
            yield return LocaleScreenshot("01-english-settings",setup.Root.transform);
            var chinese=setup.Root.transform.Find("Language Settings/Language zh-CN").GetComponent<Button>();Click(chinese,"switch back to Chinese");yield return null;
            Check(setup.Nodes[4].GetComponent<TMP_Text>().text=="音乐","already-open settings immediately return to Chinese");
            yield return Capture("02-chinese-settings.png");Click(english,"switch to English again");yield return null;
            Click(setup.Ref<Button>("SetUpPanel","backBtn"),"close settings");yield return null;
            yield return LocaleScreenshot("03-english-items",game.Root);
            var promotion=Field<UiView>("promotionView");Click(promotion.Ref<Button>("DownItem","openBtn"),"expand English support shortcuts");yield return null;
            Check(Field<bool>("supportExpanded"),"original More image retains its real click target in English");
            Click(promotion.Ref<Button>("DownItem","foldBtn"),"collapse English support shortcuts");yield return null;

            var catalog=(Dictionary<string,object>)MiniJson.Parse(Resources.Load<TextAsset>("Localization/catalog").text);
            foreach(Dictionary<string,object> entry in catalog.Arr("entries"))Check(!L10n.HasChinese(L10n.Translate(entry.Str("zh"))),"catalog translation "+entry.Str("key"));
            foreach(var text in new[]{"解锁条件\n成长等级达到12级","铜币等级达到Lv8","功夫翻硬币收益：","铜币收益*1.5倍","硬币城市完成<color=#c30505>10</color>级后解锁","全局收益*4倍"})
                Check(!L10n.HasChinese(L10n.Translate(text)),"formatted dynamic message: "+text);
            foreach(int id in new[]{202,203}) {
                var item=model.shop.First(x=>x.id==id);
                string description=(string)typeof(GameController).GetMethod("BoxDescription",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).Invoke(null,new object[]{item,model});
                Check(!L10n.HasChinese(L10n.Translate(description)),"chest description translates complete colored shard counts "+id);
            }
            Call("SelectTab",2);yield return LocaleScreenshot("04-english-helpers",game.Root);
            var facility=Field<UiView>("facilityView");
            Click(facility.Ref<Button>("FacilityPanel","deskTabBtn"),"open English tables");
            yield return LocaleScreenshot("04b-english-tables",game.Root);
            Call("ShowHelperUpgrade",model.HelperConfig(1));yield return null;var helper=LatestPopup("Facility/helperUpgrade");
            yield return LocaleScreenshot("05-english-helper-upgrade",helper.Root.transform);game.ClosePopup(helper);yield return null;
            Call("ShowUseDesk",model.desks.First(x=>x.WorldId==1));yield return null;var desk=LatestPopup("Facility/deskUpgrade");
            yield return LocaleScreenshot("06-english-table-upgrade",desk.Root.transform);game.ClosePopup(desk);yield return null;
            Call("SelectTab",3);yield return LocaleScreenshot("07-english-talents",game.Root);
            var talent=Field<UiView>("talentView");Click(talent.Ref<Button>("TalentCoin","rebornBtn"),"open English rebirth");yield return null;
            var reborn=talent.Ref<RectTransform>("TalentCoin","rebornPanel").GetChild(0);
            yield return LocaleScreenshot("08-english-rebirth",reborn);
            Click(RebirthNode(reborn,5).GetComponent<Button>(),"close English rebirth");yield return null;
            Call("SelectTab",1);Call("ShowCheckIn");yield return null;var signIn=Field<UiView>("checkInView");
            yield return LocaleScreenshot("09-english-check-in",signIn.Root.transform);
            var checkInArt=signIn.Node("bg").GetComponent<Image>().sprite;
            Check(checkInArt&&checkInArt.name=="qd_pic_bg_scale2"&&checkInArt==UiAssets.Sprite("qd_pic_bg_scale2"),"check-in shows the English title artwork under the original sprite name");
            game.ClosePopup(signIn);yield return null;
            Call("SelectTab",5);yield return LocaleScreenshot("10-english-shop",game.Root);
            Call("SelectTab",6);yield return LocaleScreenshot("11-english-worlds",game.Root);
            Call("SelectTab",1);Call("ShowSupport",SupportKind.Income);yield return null;var support=Field<UiView>("supportDialog");
            yield return LocaleScreenshot("12-english-support",support.Root.transform);
            Click(support.Ref<Button>("CoinSlotSupportDialog","closeButton"),"close English support");yield return null;
            Call("ShowOriginalAd",(Action)(()=>{}));yield return null;var ad=Field<GameObject>("originalAd");
            yield return LocaleScreenshot("13-english-ad",ad.transform);yield return new WaitForSecondsRealtime(2.2f);
            var adText=ad.transform.Find("AdContentText").GetComponent<TMP_Text>();adText.ForceMeshUpdate();
            Check(adText.rectTransform.rect.width<=Screen.width*.85f&&!adText.isTextOverflowing,"English ad copy fits the actual narrow viewport");
            yield return LocaleScreenshot("14-english-ad-complete",ad.transform);Click(ad.transform.Find("CloseButton").GetComponent<Button>(),"close English reward ad");yield return null;yield return null;
            Call("ShowEvolve",1,(Action)(()=>{}));yield return null;var evolve=LatestPopup("Evolve/EvolvePanel");
            yield return LocaleScreenshot("15-english-evolution",evolve.Root.transform);game.ClosePopup(evolve);yield return null;
            model.State.pendingOffline=12345;Field<double[]>("awaySeconds")[0]=7380;Call("ShowOffline");yield return null;
            var offline=Field<UiView>("offlineView");yield return LocaleScreenshot("16-english-offline",offline.Root.transform);
            Click(offline.Ref<Button>("OfflineIncomeUI","normalClaimButton"),"claim English offline reward");yield return null;
            game.ShowRewardBox(new List<KeyValuePair<int,double>>{new KeyValuePair<int,double>(2,5)});yield return null;
            var reward=Field<UiView>("rewardBoxView");yield return LocaleScreenshot("17-english-reward",reward.Root.transform);
            Click(reward.Ref<Button>("RewardBoxUI","GetButton"),"close English reward receipt");yield return null;
            model.State.pendingGoals.Add(model.WorldMissions[0].id);yield return new WaitForSecondsRealtime(.7f);
            var goal=Field<UiView>("goalView");yield return LocaleScreenshot("18-english-growth-news",goal.Root.transform);
            Click(goal.Ref<Button>("GoalReachPanel","confirmBtn"),"claim English growth reward");yield return new WaitForSecondsRealtime(.6f);
            L10n.SetLanguage("zh-CN");yield return null;
            Check(originalSprites.Where(pair=>pair.Key).All(pair=>pair.Key.sprite==pair.Value),"returning to Chinese restores the original image sprites");
            Check(particleSprites.All(x=>x.system&&x.system.textureSheetAnimation.GetSprite(x.index)==x.sprite),"returning to Chinese restores the original particle sprites");
            yield return Capture("19-return-to-chinese.png");
            Check(errors.Count==0,"Chinese and English UI have no runtime errors");Complete(true,null);
        }
    }
}

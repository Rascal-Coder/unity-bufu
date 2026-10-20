using Bufu.Rebuild.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bufu.Rebuild {
    /// <summary>
    /// Builds the main interface and the current world while the original loading page is up, then preloads
    /// everything the first coin throw touches, so the first taps after the page react at once.
    /// The other world, and talents and the shop on early games, wait until they are opened.
    /// </summary>
    public sealed partial class GameController {
        IEnumerator BootGame() {
            // Let the loading screen render before the save, sprites and actors stall the main thread.
            yield return null;
            PrepareSession();
            PrepareAudio();LoadClip("btnclick");
            RequestMusic(Model.Data.world==2?"diceBGM":"BGM");
            LaunchScreen.SetProgress(.3f);yield return null;
            BuildCanvas();BuildMainScene();
            LaunchScreen.SetProgress(.4f);yield return null;
            Model.AdvanceCheckIn(GameModel.TodayNumber());
            BuildTop();BuildUpgrade();
            LaunchScreen.SetProgress(.5f);yield return null;
            BuildPromotion();RebuildActors();SelectTab(1);RefreshAll();
            ApplyScreen();Canvas.ForceUpdateCanvases();
            LaunchScreen.SetProgress(.6f);yield return null;
            if(Model.MainLevel>3) {
                EnsureFacility();LaunchScreen.SetProgress(.63f);yield return null;
                EnsureShop();LaunchScreen.SetProgress(.66f);yield return null;
                EnsureTalent();LaunchScreen.SetProgress(.7f);yield return null;
                SelectTab(1);
            }
            yield return PreloadFirstPlay(.7f,.95f);
            BuildDeveloperControls();
            L10n.Changed+=RefreshLanguage;
            yield return LaunchScreen.Complete();
            BootFinished=true;
            StartGuide();
            // The opening film plays on top of this page. Keep the game hidden until the film ends.
            if(LaunchScreen.HoldForIntro)yield break;
            RevealGame();
            interfaceReady=true;
            if(Model.State.pendingOffline>0)ShowOffline();
            StartCoroutine(LaunchScreen.FadeAndHide(.35f));
        }

        void PrepareSession() {
            maxedSession=SceneManager.GetActiveScene().name==MaxedMode.SceneName;
            bool dev=developerSession=!maxedSession&&DevMode.Enabled;
            L10n.Initialize(dev,VerificationMode);
            soundOn=PlayerPrefs.GetInt(PrefKey("bufu.sound.on"),1)==1;musicOn=PlayerPrefs.GetInt(PrefKey("bufu.music.on"),1)==1;
            soundVolume=PlayerPrefs.GetFloat(PrefKey("bufu.sound.volume"),1);musicVolume=PlayerPrefs.GetFloat(PrefKey("bufu.music.volume"),1);
            Model=new GameModel(maxedSession||VerificationMode?new SaveData():SaveStore.Load(dev),dev,maxedSession);
            long now=System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            for(int i=0;i<2;i++){var s=Model.Data.worlds[i];awaySeconds[i]=s.lastExit>0?System.Math.Min(System.Math.Max(0,now-s.lastExit),21600):0;}
            Model.AccumulateOffline(now);Model.Changed+=OnChanged;Model.Encounter+=OnEncounterOffered;
        }

        /// <summary>
        /// Everything a throw loads on first use: effect clips (decompressed on load, so the first play would decode on the
        /// main thread), every VAT clip texture, the smoke frames, the floating reward text and the support particles.
        /// </summary>
        IEnumerator PreloadFirstPlay(float from,float to) {
            var sfx=new List<string>(CurrentWorldSfx());
            int total=sfx.Count+6,done=0;
            void Step(){done++;LaunchScreen.SetProgress(Mathf.Lerp(from,to,(float)done/total));}
            foreach(var name in sfx) {
                var clip=LoadClip(name);
                if(clip&&clip.loadState==AudioDataLoadState.Unloaded)clip.LoadAudioData();
                float guard=3f;
                while(clip&&clip.loadState==AudioDataLoadState.Loading&&guard>0f){guard-=Time.unscaledDeltaTime;yield return null;}
                Step();yield return null;
            }
            VatGraphic.WarmTextures();Step();yield return null;
            SmokeEffect.Preload();Step();yield return null;
            foreach(bool critical in new[]{false,true}) {
                var v=UiPrefab.Instantiate(critical?"Role/coinCriticalText":"Role/coinNormalText",Field);
                var text=v.Root.GetComponent<TMP_Text>();text.richText=true;text.text=SpriteText("+0",critical);text.ForceMeshUpdate();
                Destroy(v.Root);
            }
            Step();yield return null;
            for(int i=0;i<3;i++){EnsureSupportParticle(i);Step();yield return null;}
        }
        IEnumerable<string> CurrentWorldSfx() {
            if(Model!=null&&Model.Data.world==2){yield return "diceUp";yield return "diceHitGround";}
            else {yield return "coinup";yield return "hitground";}
            yield return "btnclick";yield return "cantbuy";yield return "upgrade";yield return "revive";yield return "maintaskreward";
        }
    }
}

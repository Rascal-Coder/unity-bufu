using Bufu.Rebuild.UI;
using Data=System.Collections.Generic.Dictionary<string,object>;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    /// <summary>
    /// Runtime controller. Every screen is instantiated from the prefabs exported from the original game
    /// (UiPrefab) and bound through the serialized script fields of those prefabs (UiView.Ref), so layout,
    /// sprites, fonts and colours are the original ones. Billing and online services use explicit integration boundaries.
    /// Partial files: Core (this), Main (top bar, upgrade panel), Popups, Panels (facility/shop/world), Talent.
    /// </summary>
    public sealed partial class GameController:MonoBehaviour {
        public const float DesignWidth=1440,DesignHeight=2560;
        public GameModel Model {get;private set;}
        public readonly List<CoinActor> Coins=new List<CoinActor>();
        public RectTransform Root,Field;
        public bool ModalOpen {get{popups.RemoveAll(x=>!x);return CutscenePlaying||popups.Count>0;}}
        public static bool VerificationMode=>Environment.GetCommandLineArgs().Contains("--verify-rebuild")||Environment.GetCommandLineArgs().Contains("--verify-alignment");
        static bool developerSession,maxedSession;
        static string PrefKey(string key)=>(maxedSession?"max.":developerSession?"dev.":"")+key;

        readonly List<GameObject> popups=new List<GameObject>();
        readonly List<KeyValuePair<GameObject,Action>> watchers=new List<KeyValuePair<GameObject,Action>>();
        RectTransform canvasRect,leftBar,rightBar,bgLayer,mainLayer,popupLayer,systemLayer;CanvasScaler scaler;
        UiView mainScene;
        float saveTimer;bool dirty;AudioSource musicSource;readonly AudioSource[] sfxSources=new AudioSource[5];
        readonly Dictionary<string,AudioClip> clips=new Dictionary<string,AudioClip>();
        readonly Dictionary<string,List<float>> sfxEnds=new Dictionary<string,List<float>>();
        string musicName="",loadingMusic;
        readonly HashSet<string> musicTried=new HashSet<string>();
        bool soundOn=true,musicOn=true,interfaceReady;float soundVolume=1,musicVolume=1;
        public static GameController Active {get;private set;}
        public bool BootFinished {get;private set;}
        readonly double[] awaySeconds=new double[2];

        // Not serialized into Main.unity: play-mode scene restore can drop scene script references.
        // AfterSceneLoad runs only for the first scene, so a later load of Main would otherwise stay empty.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ListenForScenes(){SceneManager.sceneLoaded-=OnSceneLoaded;SceneManager.sceneLoaded+=OnSceneLoaded;}
        static void OnSceneLoaded(Scene scene,LoadSceneMode mode){if(scene.name==IntroScene.SceneName||FindObjectOfType<GameController>())return;(GameObject.Find("Game")??new GameObject("Game")).AddComponent<GameController>();}

        void Awake(){Active=this;}
        void Start() {
            if(!AdManager.Instance)new GameObject("Ad Manager").AddComponent<AdManager>();
            Application.targetFrameRate=60;
            LaunchScreen.Show();
            StartCoroutine(BootGame());
        }
        CanvasGroup gameGroup;
        /// <summary>Shows the built game under the loading page, so the page can fade out over it.</summary>
        void RevealGame() {
            if(!canvasRect)return;
            ApplyScreen();Canvas.ForceUpdateCanvases();
            if(gameGroup){gameGroup.alpha=1f;gameGroup.blocksRaycasts=true;gameGroup.interactable=true;}
        }
        public void PresentAfterIntro(){if(!interfaceReady)StartCoroutine(RevealAfterLoading());}
        IEnumerator RevealAfterLoading() {
            LaunchScreen.HoldForIntro=false;
            RevealGame();
            interfaceReady=true;
            if(Model!=null&&Model.State.pendingOffline>0)ShowOffline();
            yield return LaunchScreen.FadeAndHide(.5f);
        }

        void BuildAll() {
            Model.AdvanceCheckIn(GameModel.TodayNumber());
            BuildMainScene();BuildTop();BuildUpgrade();BuildPromotion();
            RebuildActors();SelectTab(1);RefreshAll();
        }

        void Update() {
            if(!interfaceReady||IsBackgrounded)return;
            ApplyScreen();saveTimer+=Time.unscaledDeltaTime;
            UpdateSceneInput();
            if(dirty&&saveTimer>10&&!VerificationMode){SaveStore.Save(Model);dirty=false;saveTimer=0;}
            if(Input.GetKeyDown(KeyCode.Escape)&&ModalOpen)CloseTopPopup();
            else if(Input.GetKeyDown(KeyCode.Space)&&!ModalOpen)FlipAll();
            if(Model!=null&&encounterRoot==null&&Model.PendingEncounter!=null&&interfaceReady&&!VerificationMode&&!CutscenePlaying&&offlineView==null)ShowEncounter();
            if(Model!=null&&!CutscenePlaying&&!Model.PromotionRunning&&!Model.PromotionWonPending&&goalView==null&&offlineView==null&&encounterRoot==null&&Model.PendingEncounter==null&&Model.PendingGoal!=null)ShowGoal();
            UpdatePromotion();
            UpdateGuide();
            TickDiceSystems();
            UpdateActivities(Time.deltaTime);
            ApplyAudio();
            if(Model!=null&&Model.DeveloperMode) {
                if(Input.GetKeyDown(KeyCode.F1))ToggleDeveloperPanel();
                if(Input.GetKeyDown(KeyCode.F2))RefillDeveloperResources();
            }
        }
        void LateUpdate(){SortActors();}
        void OnApplicationQuit(){if(Model!=null&&!VerificationMode){Model.CancelPromotion();SaveStore.Save(Model);}}
        void OnApplicationPause(bool pause){applicationPaused=pause;RefreshApplicationActivity();}
        void OnApplicationFocus(bool focused){applicationFocused=focused;RefreshApplicationActivity();}

        // ---------------------------------------------------------------- canvas
        void BuildCanvas() {
            if(!FindObjectOfType<EventSystem>())new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
            var canvas=new GameObject("Game Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas.transform.SetParent(transform,false);
            canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            // Built under the loading page but kept invisible and untouchable until the reveal.
            // Sorting order alone is not enough: on device the game canvas drew over the order 99 loading canvas.
            gameGroup=canvas.AddComponent<CanvasGroup>();gameGroup.alpha=0f;gameGroup.blocksRaycasts=false;gameGroup.interactable=false;
            scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(DesignWidth,DesignHeight);
            canvasRect=(RectTransform)canvas.transform;
            var outer=NewRect("Outer",canvasRect);Stretch(outer);var outerImage=outer.gameObject.AddComponent<Image>();outerImage.color=Color.black;
            Root=NewRect("Portrait Game",canvasRect);Root.anchorMin=Root.anchorMax=new Vector2(.5f,.5f);Root.sizeDelta=new Vector2(DesignWidth,DesignHeight);
            bgLayer=Layer("UIbase/Background");mainLayer=Layer("UIbase/Main");popupLayer=Layer("UIbase/Popup");systemLayer=Layer("UIbase/System");
            leftBar=Bar("Left Bar");rightBar=Bar("Right Bar");
            ApplyScreen();
        }
        RectTransform Layer(string resource){var v=UiPrefab.Instantiate(resource,Root);return v.Rect;}
        RectTransform Bar(string name){var r=NewRect(name,canvasRect);var i=r.gameObject.AddComponent<Image>();i.color=Color.black;return r;}
        public static RectTransform NewRect(string name,Transform parent){var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);return (RectTransform)go.transform;}
        public static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}

        /// <summary>The original reference is 1440 wide; wider windows get black side bars, taller ones extend the layout.</summary>
        void ApplyScreen() {
            if(!canvasRect||!Root)return;
            float aspect=(float)Screen.width/Mathf.Max(1,Screen.height);
            scaler.matchWidthOrHeight=aspect>DesignWidth/DesignHeight?1:0;
            Rect safe=Screen.safeArea;float scale=canvasRect.rect.height/Mathf.Max(1,Screen.height);
            float h=safe.height*scale,w=Mathf.Min(safe.width*scale,DesignWidth);
            Root.sizeDelta=new Vector2(w,h);Root.anchoredPosition=(safe.center-new Vector2(Screen.width/2f,Screen.height/2f))*scale;
            float full=canvasRect.rect.width,edge=full/2-w/2;
            leftBar.anchorMin=new Vector2(0,0);leftBar.anchorMax=new Vector2(0,1);leftBar.pivot=new Vector2(0,.5f);leftBar.sizeDelta=new Vector2(Mathf.Max(0,edge+Root.anchoredPosition.x),0);leftBar.anchoredPosition=Vector2.zero;
            rightBar.anchorMin=new Vector2(1,0);rightBar.anchorMax=new Vector2(1,1);rightBar.pivot=new Vector2(1,.5f);rightBar.sizeDelta=new Vector2(Mathf.Max(0,edge-Root.anchoredPosition.x),0);rightBar.anchoredPosition=Vector2.zero;
        }

        // ---------------------------------------------------------------- helpers
        static void Txt(TMP_Text t,string s)=>LocalizedText.Set(t,s);
        static void On(GameObject go,bool value){if(go&&go.activeSelf!=value)go.SetActive(value);}
        void Click(Button b,Action action){if(!b)return;b.onClick.RemoveAllListeners();b.onClick.AddListener(()=>{if(CutscenePlaying)return;PlayClick();action();});}
        void Click(GameObject go,Action action){if(go)Click(go.GetComponent<Button>(),action);}
        static Sprite Sp(string key)=>UiAssets.Sprite(key);
        static Sprite SpAt(string path)=>UiAssets.SpriteAt(path);
        /// <summary>Calls refresh now and again on every model change for as long as owner exists.</summary>
        void Watch(GameObject owner,Action refresh){watchers.Add(new KeyValuePair<GameObject,Action>(owner,refresh));refresh();}
        readonly Dictionary<int,Vector4> textHome=new Dictionary<int,Vector4>();
        /// <summary>English copy is longer. Remember the prefab rect and swap in a fitted one while English is active.</summary>
        void PlaceLocalized(RectTransform rt,Vector2 size,Vector2 position) {
            int id=rt.GetInstanceID();
            if(!textHome.TryGetValue(id,out var home)){home=new Vector4(rt.sizeDelta.x,rt.sizeDelta.y,rt.anchoredPosition.x,rt.anchoredPosition.y);textHome[id]=home;}
            if(L10n.IsEnglish){rt.sizeDelta=size;rt.anchoredPosition=position;}
            else {rt.sizeDelta=new Vector2(home.x,home.y);rt.anchoredPosition=new Vector2(home.z,home.w);}
        }
        void OnChanged(){dirty=true;RefreshAll();}
        void RefreshAll() {
            watchers.RemoveAll(x=>!x.Key);
            RefreshTop();RefreshUpgradePanel();RefreshPromotion();
            foreach(var w in watchers.ToArray())if(w.Key)w.Value();
        }
        public static IEnumerator Tween(float duration,Action<float> step,Action done=null) {
            float t=0;while(t<1){t+=Time.unscaledDeltaTime/Mathf.Max(.0001f,duration);step(Mathf.Clamp01(t));yield return null;}
            done?.Invoke();
        }
        public static float OutBack(float t){const float c1=1.70158f,c3=c1+1;return 1+c3*Mathf.Pow(t-1,3)+c1*Mathf.Pow(t-1,2);}
        public static float InBack(float t){const float c1=1.70158f,c3=c1+1;return c3*t*t*t-c1*t*t;}
        public static float OutQuad(float t)=>1-(1-t)*(1-t);

        // ---------------------------------------------------------------- popups
        public UiView OpenPopup(string resource) {var v=UiPrefab.Instantiate(resource,popupLayer);popups.Add(v.Root);return v;}
        public void ClosePopup(UiView v){if(v==null||!v.Root)return;popups.Remove(v.Root);Destroy(v.Root);}
        void CloseTopPopup() {
            if(CutscenePlaying||originalAd||encounterAd||rewardedAdPending||(worldUnlockGuide!=null&&worldUnlockGuide.Root))return;
            if(promotionDialog!=null&&promotionDialog.Root&&popups.LastOrDefault()==promotionDialog.Root){promotionDialog.Ref<Button>("DialogPanel","bgButton").onClick.Invoke();return;}
            popups.RemoveAll(x=>!x);if(popups.Count==0)return;
            var go=popups[popups.Count-1];
            if(go==purchaseView){ClosePurchaseStore();return;}
            if(encounterRoot&&go==encounterRoot){CloseEncounterFromBack();return;}
            if(TryDismissRewardPopup(go)||TryHandleActivityPopup(go))return;
            popups.RemoveAt(popups.Count-1);Destroy(go);
        }

        GameObject toastObject;
        /// <summary>BroadcastManager.ShowPopUpTip: fades in, holds 2s, then fades out while rising.</summary>
        public void ShowToast(string message) {
            if(!popupLayer||string.IsNullOrEmpty(message))return;
            if(toastObject)Destroy(toastObject);
            var v=UiPrefab.Instantiate("Common/tip",popupLayer);toastObject=v.Root;
            var text=v.Node("text").GetComponent<TMP_Text>();Txt(text,message);text.color=Color.yellow;text.raycastTarget=false;
            var bg=v.Node("bg").GetComponent<Image>();bg.raycastTarget=false;
            Vector2 pref=text.GetPreferredValues(text.text,text.rectTransform.rect.width,100);
            bg.rectTransform.sizeDelta=new Vector2(Mathf.Max(pref.x+40,200),Mathf.Max(pref.y+40,80));
            var rt=v.Rect;rt.anchoredPosition=new Vector2(0,600);rt.SetAsLastSibling();
            var group=v.Root.AddComponent<CanvasGroup>();group.alpha=0;group.blocksRaycasts=false;group.interactable=false;
            StartCoroutine(ToastRoutine(v.Root,rt,group));
        }
        IEnumerator ToastRoutine(GameObject go,RectTransform rt,CanvasGroup group) {
            yield return Tween(.3f,t=>{if(group)group.alpha=t;});
            float wait=2;while(wait>0&&go){wait-=Time.unscaledDeltaTime;yield return null;}
            if(!go)yield break;Vector2 start=rt.anchoredPosition;
            yield return Tween(.5f,t=>{if(go){group.alpha=1-t;rt.anchoredPosition=start+Vector2.up*100*t;}});
            if(go)Destroy(go);
        }

        // ---------------------------------------------------------------- audio
        /// <summary>SoundManager: at most three copies of one effect. Only the click is loaded with the first screen; other effects load on use. BGM streams.</summary>
        void PrepareAudio() {
            if(musicSource)return;
            musicSource=gameObject.AddComponent<AudioSource>();musicSource.loop=true;musicSource.playOnAwake=false;
            for(int i=0;i<sfxSources.Length;i++){sfxSources[i]=gameObject.AddComponent<AudioSource>();sfxSources[i].playOnAwake=false;}
        }
        AudioClip LoadClip(string name) {
            AudioClip clip;if(clips.TryGetValue(name,out clip)&&clip)return clip;
            clip=Resources.Load<AudioClip>("Audio/"+name);if(clip)clips[name]=clip;return clip;
        }
        public void PlaySfx(string name) {
            if(!soundOn||string.IsNullOrEmpty(name)||name=="BGM"||name=="diceBGM")return;
            var clip=LoadClip(name);if(!clip)return;
            List<float> ends;if(!sfxEnds.TryGetValue(name,out ends))sfxEnds[name]=ends=new List<float>();
            float now=Time.unscaledTime;ends.RemoveAll(t=>t<=now);if(ends.Count>=3)return;
            AudioSource source=null;foreach(var s in sfxSources)if(!s.isPlaying){source=s;break;}
            if(source==null)source=sfxSources[0];
            source.PlayOneShot(clip,soundVolume);ends.Add(now+Mathf.Max(.05f,clip.length));
        }
        public void PlayClick(){PlaySfx("btnclick");}
        public void PlayUpgrade(){PlaySfx("upgrade");}
        public void PlayCantBuy(){PlaySfx("cantbuy");}
        public void PlayCoinSound(){PlaySfx("coinup");}
        public void PlayHit(){PlaySfx("hitground");}
        public void PlayDiceUp(){PlaySfx("diceUp");}
        public void PlayDiceLand(){PlaySfx("diceHitGround");}
        public void PlayRevive(){PlaySfx("revive");}
        public void PlayReward(){PlaySfx("maintaskreward");}
        public void ApplyAudio() {
            if(!musicSource||Model==null)return;
            if(!musicOn){if(musicSource.isPlaying)musicSource.Stop();musicName="";return;}
            string want=Model.Data.world==2?"diceBGM":"BGM";
            AudioClip clip;if(!clips.TryGetValue(want,out clip)||!clip){RequestMusic(want);return;}
            musicSource.volume=musicVolume*.5f;
            if(musicName!=want||!musicSource.isPlaying){musicSource.clip=clip;musicSource.Play();musicName=want;}
        }
        void RequestMusic(string name) {
            if(loadingMusic==name||musicTried.Contains(name))return;
            loadingMusic=name;StartCoroutine(StreamMusic(name));
        }
        // Streaming clips decode during playback. Loading the asset must not call LoadAudioData, which would expand the whole bed into PCM.
        IEnumerator StreamMusic(string name) {
            var request=Resources.LoadAsync<AudioClip>("Audio/"+name);yield return request;
            musicTried.Add(name);var clip=request.asset as AudioClip;if(clip)clips[name]=clip;
            if(loadingMusic==name)loadingMusic=null;
        }
        void SavePrefs(){if(VerificationMode)return;PlayerPrefs.SetInt(PrefKey("bufu.sound.on"),soundOn?1:0);PlayerPrefs.SetInt(PrefKey("bufu.music.on"),musicOn?1:0);PlayerPrefs.SetFloat(PrefKey("bufu.sound.volume"),soundVolume);PlayerPrefs.SetFloat(PrefKey("bufu.music.volume"),musicVolume);PlayerPrefs.Save();}

        // ---------------------------------------------------------------- main scene
        const string BackgroundPath="Assets/GameRes/MapRes/fight_bg_";
        void BuildMainScene() {
            mainScene=UiPrefab.Instantiate("Common/MainScene",bgLayer);
            Field=mainScene.Ref<RectTransform>("MainScene","sceneBoxUI");
            On(mainScene.Node("zhezhu"),false);
            foreach(var guide in new[]{"guideFinger"})On(mainScene.Ref("MainScene",guide),false);
            BuildScenePools();
            Watch(mainScene.Root,()=>{var s=SpAt(BackgroundPath+Model.UsingDesk+".png");var img=mainScene.Ref<Image>("MainScene","bgImage");if(s&&img.sprite!=s)img.sprite=s;});
        }

        public void RebuildActors() {
            actorGeneration++;evolvingQualities.Clear();
            foreach(var coin in Coins)if(coin)Destroy(coin.gameObject);Coins.Clear();
            foreach(var helper in HelperActors)if(helper)Destroy(helper.gameObject);HelperActors.Clear();SleepTipActive=false;
            if(SmokePool)foreach(Transform effect in SmokePool)Destroy(effect.gameObject);
            SyncActors(true);
            ResetDiceWorldPresentation();
        }
        static readonly string[] CoinKinds={"copper","silver","gold","purple"};
        /// <summary>Adds the coins and helpers the model says should exist without moving the ones already on the field.</summary>
        public void SyncActors(bool initial=false) {
            if(!Field)return;Canvas.ForceUpdateCanvases();
            int world=Model.Data.world;
            for(int quality=1;quality<=4;quality++) {
                if(evolvingQualities.Contains(quality))continue;
                int visible=Math.Max(0,Model.Count(quality));
                var list=Coins.Where(x=>x&&x.Quality==quality).ToList();
                for(int i=list.Count-1;i>=visible;i--){Coins.Remove(list[i]);Destroy(list[i].gameObject);}
                int tier=Model.State.evolution[Model.BaseId+quality+1];string asset=(world==1?"coin_":"dice_")+CoinKinds[quality-1]+(tier==0?"":"_"+(tier+1));
                for(int i=Math.Min(list.Count,visible);i<visible;i++) {
                    var rt=NewRect("Coin "+quality+" "+i,ActorPool);rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);rt.sizeDelta=new Vector2(CoinSize,CoinSize);
                    rt.anchoredPosition=initial&&quality==1&&visible<=3?InitialCoinSpawn(i):RandomCoinSpawn();
                    var coin=rt.gameObject.AddComponent<CoinActor>();coin.Init(this,quality,world,asset);Coins.Add(coin);
                }
            }
            int slots=Math.Max(0,Model.Helpers);var want=Model.HelperLineup(slots);
            var need=new int[11];foreach(var id in want)if(id>=0&&id<need.Length)need[id]++;
            var got=new int[11];HelperActors.RemoveAll(x=>!x);foreach(var h in HelperActors)if(h.RoleId>=0&&h.RoleId<got.Length)got[h.RoleId]++;
            for(int id=0;id<need.Length;id++)while(got[id]>need[id]) {
                int index=HelperActors.FindLastIndex(x=>x&&x.RoleId==id);if(index<0)break;
                Destroy(HelperActors[index].gameObject);HelperActors.RemoveAt(index);got[id]--;
            }
            for(int id=0;id<need.Length;id++)while(got[id]<need[id]) {
                var rt=NewRect("Helper "+id,ActorPool);rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);rt.sizeDelta=new Vector2(200,220);
                var helper=rt.gameObject.AddComponent<HelperActor>();HelperActors.Add(helper);helper.Init(this,id);got[id]++;
            }
        }
        
        public const float CoinSize=152;
        
        public void FlipAll(){foreach(var coin in Coins)if(coin)coin.Throw(true);}

        // ---------------------------------------------------------------- floating reward text (VatFloatingTextPool)
        int floatingCount;const int FloatingMax=60;
        public void FloatReward(Vector2 position,double reward,bool critical,int face) {
            if(reward<=0||floatingCount>=FloatingMax||!Field)return;
            var v=UiPrefab.Instantiate(critical?"Role/coinCriticalText":"Role/coinNormalText",Field);
            var text=v.Root.GetComponent<TMP_Text>();text.raycastTarget=false;
            float scale=Mathf.Clamp(1-.4f*floatingCount/FloatingMax,.6f,1f);
            // Sprite 12 is the red crit bolt, not a Chinese character. Both languages keep it.
            string shown=(critical?"暴":"")+"+"+GameModel.Format(reward);
            text.richText=true;text.text=SpriteText(shown,critical);text.fontSize=48*scale;
            var rt=v.Rect;rt.localScale=Vector3.one*2.8f;rt.anchoredPosition=position+new Vector2(0,83);floatingCount++;
            StartCoroutine(FloatRoutine(v.Root,rt,text));
        }
        IEnumerator FloatRoutine(GameObject go,RectTransform rt,TMP_Text text) {
            Vector2 start=rt.anchoredPosition;float offsetX=UnityEngine.Random.Range(-30f,30f),elapsed=0;
            while(elapsed<1&&go) {
                elapsed+=Time.deltaTime*2;float p=Mathf.Min(1,elapsed);
                rt.anchoredPosition=new Vector2(start.x+offsetX*(2-p),start.y+120*(1-(1-p)*(1-p)));
                text.alpha=p>.5f?1-(p-.5f)/.5f:1;yield return null;
            }
            floatingCount--;if(go)Destroy(go);
        }
        /// <summary>VatFloatingTextPool.ConvertToSpriteText: digits and letters are glyphs of the ty_num_both sprite asset.</summary>
        static string SpriteText(string text,bool critical) {
            var sb=new System.Text.StringBuilder();
            foreach(char c in text) {
                int index;
                if(c>='0'&&c<='9')index=(critical?0:39)+(c-'0');
                else if(c=='.')index=critical?10:49;
                else if(c=='+')index=critical?11:50;
                else if(c=='暴')index=12;
                else if(c>='A'&&c<='Z')index=(critical?13:51)+(c-'A');
                else if(c>='a'&&c<='z')index=(critical?13:51)+(c-'a');
                else continue;
                sb.Append("<sprite=").Append(index).Append('>');
            }
            return sb.ToString();
        }

        // ---------------------------------------------------------------- restart after "clear save"
        void ResetGame() {
            if(purchases!=null&&purchases.Busy){ShowToast("Please wait for the purchase check to finish.");return;}
            bool dev=Model.DeveloperMode,maxed=Model.Maxed;
            encounterRoot=null;encounterPhase=EncounterPhase.None;encounterAd=false;
            var paid=Model.Data.purchases;ClosePurchaseStore();
            Model.Encounter-=OnEncounterOffered;Model.Changed-=OnChanged;Model=new GameModel(new SaveData{purchases=paid},dev,maxed);Model.Changed+=OnChanged;Model.Encounter+=OnEncounterOffered;InitializePurchases();SaveStore.Save(Model);
            watchers.Clear();popups.Clear();rewardPopupClosers.Clear();goalView=null;offlineView=null;facilityView=null;shopView=null;worldView=null;talentView=null;activityView=null;
            helperWorld=-1;promotionView=null;promotionHud=null;promotionDialog=null;shownPromotion=null;Time.timeScale=1;
            foreach(var layer in new[]{bgLayer,mainLayer,popupLayer})foreach(Transform child in layer)Destroy(child.gameObject);
            Coins.Clear();BuildAll();ResetGuide();
        }
    }
}

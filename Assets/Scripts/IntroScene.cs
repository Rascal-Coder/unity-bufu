using Bufu.Rebuild.UI;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Bufu.Rebuild {
    /// <summary>
    /// Original MainEntry order. The loading page is up first. The film plays on top of that
    /// page, in parallel with building the hidden first screen. The game is revealed only after
    /// both the film and that first screen are finished.
    /// </summary>
    public sealed class IntroScene:MonoBehaviour {
        public const string SceneName="Intro";
        const string GameScene="Main";
        const string PlayedKey="IntroVideoPlayed";
        // Temporary for testing: play the film on every launch. Set false to restore play-once.
        const bool AlwaysPlayFilm=true;
        const float FilmSeconds=15.083f;
        static readonly Color Ink=new Color(0.09f,0.09f,0.09f,1f);

        VideoPlayer player;RawImage filmImage;RenderTexture filmTexture;GameObject filmRoot,skipRoot;
        bool playbackStarted,filmFailed,videoDone;
        float played;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot() {
            if(SceneManager.GetActiveScene().name!=SceneName||FindObjectOfType<IntroScene>())return;
            new GameObject("Intro Player").AddComponent<IntroScene>();
        }

        IEnumerator Start() {
            bool film=!GameController.VerificationMode&&(AlwaysPlayFilm||PlayerPrefs.GetInt(PlayedKey,0)!=1);
            // Loading page first, as MainEntry.Awake does, then the main scene loads behind it.
            LaunchScreen.Show();
            DontDestroyOnLoad(gameObject);
            if(!film){yield return LoadMainScene();Destroy(gameObject);yield break;}
            LaunchScreen.HoldForIntro=true;
            yield return null;
            // The original streams from its CDN. The APK ships the same film in StreamingAssets and has no network permission.
            BuildFilm(System.IO.Path.Combine(Application.streamingAssetsPath,"Intro","opening.mp4"));
            StartCoroutine(LoadMainScene());
            float waited=0f;
            while(player&&!player.isPrepared&&!filmFailed&&waited<8f){waited+=Time.unscaledDeltaTime;yield return null;}
            if(!player||!player.isPrepared||filmFailed) {
                Debug.LogWarning("Intro video did not prepare");
                videoDone=true;
            } else {
                if(filmRoot)filmRoot.SetActive(true);
                if(filmImage)filmImage.gameObject.SetActive(true);
                player.Play();
                playbackStarted=true;
                while(!videoDone) {
                    played+=Time.unscaledDeltaTime;
                    if(played>=2f&&skipRoot)skipRoot.SetActive(true);
                    float length=player!=null&&player.length>1d?(float)player.length:FilmSeconds;
                    if(played>=length-0.05f)CompleteVideo();
                    yield return null;
                }
            }
            float guard=45f;
            while((GameController.Active==null||!GameController.Active.BootFinished)&&guard>0f){guard-=Time.unscaledDeltaTime;yield return null;}
            if(GameController.Active)GameController.Active.PresentAfterIntro();
            else LaunchScreen.Hide();
            Destroy(gameObject);
        }

        void Update() {
            if(!playbackStarted||videoDone)return;
            if(Input.GetKeyDown(KeyCode.Escape)||Input.GetKeyDown(KeyCode.Space))CompleteVideo();
        }
        void OnDestroy(){ReleaseFilm();}

        void CompleteVideo() {
            if(videoDone)return;
            videoDone=true;
            if(player&&player.isPlaying)player.Stop();
            PlayerPrefs.SetInt(PlayedKey,1);PlayerPrefs.Save();
            if(filmRoot)filmRoot.SetActive(false);
            if(skipRoot)skipRoot.SetActive(false);
            ShowConfirm();
        }
        void ShowConfirm() {
            var parent=LaunchScreen.Current!=null?LaunchScreen.Current.PageRoot:transform;
            var panel=NewRect("Intro Confirm",parent);Stretch(panel);panel.SetAsLastSibling();
            var dim=panel.gameObject.AddComponent<Image>();dim.color=new Color(0f,0f,0f,.55f);dim.raycastTarget=true;
            var card=NewRect("Card",panel);card.anchorMin=card.anchorMax=new Vector2(.5f,.5f);card.sizeDelta=new Vector2(880f,520f);
            var cardImage=card.gameObject.AddComponent<Image>();cardImage.color=new Color(.98f,.95f,.88f,1f);
            var body=NewRect("Body",card);body.anchorMin=new Vector2(0f,0.28f);body.anchorMax=new Vector2(1f,1f);body.offsetMin=new Vector2(36f,12f);body.offsetMax=new Vector2(-36f,-28f);
            var message=body.gameObject.AddComponent<TextMeshProUGUI>();
            var font=UiAssets.Font();if(font)message.font=font;
            message.text="已看完片头，点击确定开始你的财富之旅！";message.fontSize=42;message.alignment=TextAlignmentOptions.Center;message.color=Ink;message.raycastTarget=false;
            var ok=NewRect("开始游戏",card);ok.anchorMin=ok.anchorMax=new Vector2(.5f,0f);ok.pivot=new Vector2(.5f,0f);ok.sizeDelta=new Vector2(420f,96f);ok.anchoredPosition=new Vector2(0f,36f);
            var okImage=ok.gameObject.AddComponent<Image>();okImage.color=new Color(.96f,.75f,.16f,1f);
            var okButton=ok.gameObject.AddComponent<Button>();okButton.targetGraphic=okImage;okButton.onClick.AddListener(ConfirmIntro);
            var okLabel=NewRect("Label",ok);Stretch(okLabel);
            var okText=okLabel.gameObject.AddComponent<TextMeshProUGUI>();if(font)okText.font=font;
            okText.text="开始游戏";okText.fontSize=40;okText.alignment=TextAlignmentOptions.Center;okText.color=Ink;okText.raycastTarget=false;
        }
        void ConfirmIntro(){PlayerPrefs.SetInt(PlayedKey,1);PlayerPrefs.Save();}

        void BuildFilm(string path) {
            if(!FindObjectOfType<EventSystem>())new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
            var parent=LaunchScreen.Current!=null?LaunchScreen.Current.PageRoot:transform;
            var root=NewRect("IntroVideoContainer",parent);filmRoot=root.gameObject;Stretch(root);root.SetAsLastSibling();filmRoot.SetActive(false);
            var backing=root.gameObject.AddComponent<Image>();backing.color=Color.black;backing.raycastTarget=true;
            var imageObject=new GameObject("VideoImage",typeof(RectTransform),typeof(RawImage));
            imageObject.transform.SetParent(root,false);
            var imageRect=(RectTransform)imageObject.transform;imageRect.anchorMin=Vector2.zero;imageRect.anchorMax=Vector2.one;imageRect.offsetMin=imageRect.offsetMax=Vector2.zero;
            filmImage=imageObject.GetComponent<RawImage>();filmImage.color=Color.white;filmImage.raycastTarget=false;
            filmTexture=new RenderTexture(1440,2880,0);filmTexture.Create();filmImage.texture=filmTexture;
            imageObject.SetActive(false);

            var host=new GameObject("Video");host.transform.SetParent(transform,false);
            player=host.AddComponent<VideoPlayer>();
            player.playOnAwake=false;player.isLooping=false;player.skipOnDrop=false;player.waitForFirstFrame=true;
            player.renderMode=VideoRenderMode.RenderTexture;player.targetTexture=filmTexture;
            player.aspectRatio=VideoAspectRatio.FitOutside;
            player.source=VideoSource.Url;player.url=path;
            player.timeUpdateMode=VideoTimeUpdateMode.UnscaledGameTime;
            player.audioOutputMode=VideoAudioOutputMode.Direct;
            player.errorReceived+=(vp,message)=>{filmFailed=true;Debug.LogWarning("Intro video failed: "+message);};
            player.loopPointReached+=vp=>CompleteVideo();
            player.prepareCompleted+=source=>{source.SetDirectAudioMute(0,false);source.SetDirectAudioVolume(0,1f);};

            // Canvas matches width at 1440, so one canvas unit is Screen.width/1440 pixels. Keep the button below the notch.
            float unitsPerPixel=1440f/Mathf.Max(1,Screen.width);
            float topInset=(Screen.height-Screen.safeArea.yMax)*unitsPerPixel,rightInset=(Screen.width-Screen.safeArea.xMax)*unitsPerPixel;
            var skip=NewRect("Skip",root);skipRoot=skip.gameObject;skipRoot.SetActive(false);skip.SetAsLastSibling();
            skip.anchorMin=skip.anchorMax=new Vector2(1,1);skip.pivot=new Vector2(1,1);
            skip.sizeDelta=new Vector2(260,104);skip.anchoredPosition=new Vector2(-48-rightInset,-48-topInset);
            var skipImage=skip.gameObject.AddComponent<Image>();skipImage.sprite=PillSprite();skipImage.type=Image.Type.Sliced;skipImage.pixelsPerUnitMultiplier=31f/52f;
            skipImage.color=new Color(0f,0f,0f,.5f);skipImage.raycastTarget=true;
            var button=skip.gameObject.AddComponent<Button>();button.targetGraphic=skipImage;button.onClick.AddListener(CompleteVideo);
            var colors=button.colors;colors.pressedColor=new Color(.7f,.7f,.7f,1f);button.colors=colors;
            var navigation=button.navigation;navigation.mode=Navigation.Mode.None;button.navigation=navigation;
            var label=NewRect("Label",skip);Stretch(label);
            var text=label.gameObject.AddComponent<TextMeshProUGUI>();
            var font=UiAssets.Font();if(font)text.font=font;
            text.text=L10n.Translate("跳过")+"  >>";text.fontSize=44;text.color=Color.white;text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false;
            try{player.Prepare();}
            catch(Exception ex){filmFailed=true;Debug.LogWarning("Intro video prepare failed: "+ex.Message);}
        }

        void ReleaseFilm() {
            if(player){player.Stop();player.targetTexture=null;player=null;}
            if(filmTexture){filmTexture.Release();Destroy(filmTexture);filmTexture=null;}
        }

        IEnumerator LoadMainScene() {
            const string path="Assets/Scenes/Main.unity";
#if UNITY_EDITOR
            var operation=UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(path,new LoadSceneParameters(LoadSceneMode.Single));
            if(operation==null){SceneManager.LoadScene(GameScene);yield break;}
            while(!operation.isDone)yield return null;
#else
            var operation=SceneManager.LoadSceneAsync(GameScene);
            while(operation!=null&&!operation.isDone)yield return null;
#endif
        }

        static Sprite PillSprite() {
            const int size=64;const float radius=size/2f;
            var tex=new Texture2D(size,size,TextureFormat.RGBA32,false){wrapMode=TextureWrapMode.Clamp};
            var pixels=new Color32[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++) {
                float dx=Mathf.Abs(x+.5f-radius),dy=Mathf.Abs(y+.5f-radius);
                float a=Mathf.Clamp01(radius-Mathf.Sqrt(dx*dx+dy*dy)+.5f);
                pixels[y*size+x]=new Color32(255,255,255,(byte)(a*255));
            }
            tex.SetPixels32(pixels);tex.Apply();
            return Sprite.Create(tex,new Rect(0,0,size,size),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(radius-1,radius-1,radius-1,radius-1));
        }

        static RectTransform NewRect(string name,Transform parent) {
            var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);return (RectTransform)go.transform;
        }
        static void Stretch(RectTransform rect){rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;}
    }
}

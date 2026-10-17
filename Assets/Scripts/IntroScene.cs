using Bufu.Rebuild.UI;
using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Bufu.Rebuild {
    /// <summary>
    /// Standalone opening scene. Plays the prerendered film and its own soundtrack,
    /// then loads the main scene when the film ends or the player skips.
    /// </summary>
    public sealed class IntroScene:MonoBehaviour {
        public const string SceneName="Intro";
        const string GameScene="Main";
        const float FilmSeconds=15.083f;
        static readonly Color Ink=new Color(0.09f,0.09f,0.09f,1f);

        VideoPlayer player;
        bool loading,playbackStarted;
        float played;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot() {
            if(SceneManager.GetActiveScene().name!=SceneName||FindObjectOfType<IntroScene>())return;
            new GameObject("Intro Player").AddComponent<IntroScene>();
        }

        IEnumerator Start() {
            yield return null;
            var path=Path.Combine(Application.streamingAssetsPath,"Intro","opening.mp4");
            if(GameController.VerificationMode||!File.Exists(path)){RequestMain();yield break;}
            BuildFilm(path);
            var film=player;
            bool preparing=true;
            try{film.Prepare();}
            catch(Exception ex){preparing=false;Debug.LogWarning("Intro video prepare failed: "+ex.Message);}
            if(!preparing){RequestMain();yield break;}
            float waited=0f;
            while(preparing&&!film.isPrepared&&!loading&&waited<8f){waited+=Time.unscaledDeltaTime;yield return null;}
            if(loading)yield break;
            if(!film.isPrepared){Debug.LogWarning("Intro video did not prepare");RequestMain();yield break;}
            film.Play();
            playbackStarted=true;
        }

        void Update() {
            if(loading)return;
            if(Input.GetKeyDown(KeyCode.Escape)||Input.GetKeyDown(KeyCode.Space)){RequestMain();return;}
            if(!playbackStarted||player==null)return;
            played+=Time.unscaledDeltaTime;
            if(played>=FilmSeconds-0.05f)RequestMain();
        }

        void OnDestroy(){if(player)player.Stop();}

        void BuildFilm(string path) {
            var camera=Camera.main?Camera.main:FindObjectOfType<Camera>();
            if(camera){camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;}
            if(!FindObjectOfType<EventSystem>())new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
            var canvasObject=new GameObject("Intro Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            var canvas=canvasObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=100;
            var scaler=canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(GameController.DesignWidth,GameController.DesignHeight);
            scaler.matchWidthOrHeight=Screen.width/(float)Mathf.Max(1,Screen.height)>GameController.DesignWidth/GameController.DesignHeight?1:0;
            var root=canvasObject.GetComponent<RectTransform>();

            var host=new GameObject("Video");host.transform.SetParent(transform,false);
            player=host.AddComponent<VideoPlayer>();
            player.playOnAwake=false;player.isLooping=false;player.skipOnDrop=false;player.waitForFirstFrame=true;
            player.renderMode=camera?VideoRenderMode.CameraFarPlane:VideoRenderMode.CameraNearPlane;
            player.targetCamera=camera;player.targetCameraAlpha=1f;player.aspectRatio=VideoAspectRatio.FitInside;
            player.source=VideoSource.Url;player.url=path.Replace('\\','/');
            player.timeUpdateMode=VideoTimeUpdateMode.UnscaledGameTime;
            player.audioOutputMode=VideoAudioOutputMode.Direct;
            player.errorReceived+=(vp,message)=>Debug.LogWarning("Intro video failed: "+message);
            player.loopPointReached+=vp=>{if(played>14f)RequestMain();};
            player.prepareCompleted+=source=>{source.SetDirectAudioMute(0,false);source.SetDirectAudioVolume(0,1f);};

            var skip=NewRect("跳过",root);
            skip.anchorMin=skip.anchorMax=new Vector2(1,1);skip.pivot=new Vector2(1,1);
            skip.sizeDelta=new Vector2(188,76);skip.anchoredPosition=new Vector2(-28,-22);
            var skipImage=skip.gameObject.AddComponent<Image>();skipImage.color=Color.white;
            var button=skip.gameObject.AddComponent<Button>();button.targetGraphic=skipImage;button.onClick.AddListener(RequestMain);
            var outline=skip.gameObject.AddComponent<Outline>();outline.effectColor=Ink;outline.effectDistance=new Vector2(3,-3);
            var navigation=button.navigation;navigation.mode=Navigation.Mode.None;button.navigation=navigation;
            var label=NewRect("Label",skip);Stretch(label);
            var text=label.gameObject.AddComponent<TextMeshProUGUI>();
            var font=UiAssets.Font();if(font)text.font=font;
            text.text=L10n.Translate("跳过");text.fontSize=36;text.color=Ink;text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false;
        }

        void RequestMain() {
            if(loading)return;
            loading=true;
            StartCoroutine(LoadMain());
        }

        IEnumerator LoadMain() {
            if(player&&player.isPlaying)player.Stop();
            yield return null;
            const string path="Assets/Scenes/Main.unity";
            Debug.Log("Intro finished, loading Main");
#if UNITY_EDITOR
            var operation=UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(path,new LoadSceneParameters(LoadSceneMode.Single));
            if(operation==null)SceneManager.LoadScene(GameScene);
#else
            SceneManager.LoadScene(GameScene);
#endif
        }

        static RectTransform NewRect(string name,Transform parent) {
            var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);return (RectTransform)go.transform;
        }
        static void Stretch(RectTransform rect){rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;}
    }
}

#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bufu.Rebuild {
    sealed class IntroPlaybackProbe:MonoBehaviour {
        float elapsed;
        bool shot2,shot8,shot14,shotMain;
        string logPath;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot() {
            if(!Environment.GetCommandLineArgs().Any(arg=>arg=="--intro-check"))return;
            if(FindObjectOfType<IntroPlaybackProbe>())return;
            var go=new GameObject("Intro Check");
            DontDestroyOnLoad(go);
            go.AddComponent<IntroPlaybackProbe>();
        }

        void Awake() {
            Directory.CreateDirectory("Documentation/IntroCheck");
            logPath="Documentation/IntroCheck/result.txt";
        }

        void Update() {
            elapsed+=Time.unscaledDeltaTime;
            var scene=SceneManager.GetActiveScene().name;
            if(!shot2&&elapsed>=2f){shot2=true;StartCoroutine(Shot("t02",scene));}
            if(!shot8&&elapsed>=8f){shot8=true;StartCoroutine(Shot("t08",scene));}
            if(!shot14&&elapsed>=14f){shot14=true;StartCoroutine(Shot("t14",scene));}
            if(!shotMain&&scene=="Main"){shotMain=true;StartCoroutine(Finish(scene));}
            if(!shotMain&&elapsed>22f){shotMain=true;StartCoroutine(Finish(scene));}
        }

        IEnumerator Shot(string name,string scene) {
            yield return new WaitForEndOfFrame();
            WriteFrame(name,scene);
        }

        IEnumerator Finish(string scene) {
            float wait=0f;
            while(wait<2f){wait+=Time.unscaledDeltaTime;yield return null;}
            yield return new WaitForEndOfFrame();
            scene=SceneManager.GetActiveScene().name;
            int canvases=FindObjectsOfType<Canvas>().Length;
            bool game=FindObjectOfType<GameController>()!=null;
            File.AppendAllText(logPath,"after2s scene="+scene+" canvases="+canvases+" game="+game+"\n");
            WriteFrame(scene=="Main"?"main":"failed",scene);
            yield return null;
            EditorApplication.isPlaying=false;
            EditorApplication.Exit(scene=="Main"&&game&&canvases>0?0:1);
        }

        void WriteFrame(string name,string scene) {
            int w=Mathf.Max(8,Screen.width),h=Mathf.Max(8,Screen.height);
            var tex=new Texture2D(w,h,TextureFormat.RGB24,false);
            tex.ReadPixels(new Rect(0,0,w,h),0,0);
            tex.Apply();
            var pixel=tex.GetPixel(w/2,h/2);
            File.WriteAllBytes("Documentation/IntroCheck/"+name+".png",tex.EncodeToPNG());
            Destroy(tex);
            File.AppendAllText(logPath,name+" scene="+scene+" "+w+"x"+h+" center="+pixel+" t="+elapsed.ToString("0.00")+"\n");
            Debug.Log("INTRO_CHECK "+name+" scene="+scene+" center="+pixel);
        }
    }
}
#endif

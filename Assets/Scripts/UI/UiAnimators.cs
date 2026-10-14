using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild.UI {
    public sealed class UpDownAnimator:MonoBehaviour {
        float moveHeight=.1f,moveTime=.5f,timer,baseY;bool isUI;RectTransform rect;
        public void Configure(float height,float time,bool ui) {
            moveHeight=height;moveTime=Mathf.Max(.01f,time);isUI=ui;rect=transform as RectTransform;
            baseY=isUI&&rect?rect.anchoredPosition.y:transform.localPosition.y;
        }
        void Update() {
            timer+=Time.deltaTime;float t=timer%(moveTime*2);
            float y=t<moveTime?Mathf.Lerp(baseY,baseY+moveHeight,t/moveTime):Mathf.Lerp(baseY+moveHeight,baseY,(t-moveTime)/moveTime);
            if(isUI&&rect){var p=rect.anchoredPosition;p.y=y;rect.anchoredPosition=p;}
            else{var p=transform.localPosition;p.y=y;transform.localPosition=p;}
        }
    }

    public sealed class LeftRightAnimator:MonoBehaviour {
        float moveWidth=10,moveTime=.5f,timer,startX;RectTransform rect;
        public void Configure(float width,float time){moveWidth=width;moveTime=Mathf.Max(.01f,time);rect=transform as RectTransform;if(rect)startX=rect.anchoredPosition.x;}
        void Update() {
            if(!rect)return;timer+=Time.deltaTime;float t=timer%(moveTime*2);var p=rect.anchoredPosition;
            p.x=t<moveTime?Mathf.Lerp(startX,startX+moveWidth,t/moveTime):Mathf.Lerp(startX+moveWidth,startX,(t-moveTime)/moveTime);rect.anchoredPosition=p;
        }
    }

    /// <summary>ChestShakeReminder: the progress chest rocks ±15° while the five-spin reward is ready.</summary>
    public sealed class ChestShake:MonoBehaviour {
        public bool shaking; RectTransform rect;
        void Awake(){rect=transform as RectTransform;}
        void Update() {
            if(!rect)return;
            if(!shaking){if(rect.localEulerAngles.z!=0)rect.localEulerAngles=Vector3.zero;return;}
            float w=Mathf.PingPong(Time.unscaledTime/0.15f,2f)-1f;
            rect.localEulerAngles=new Vector3(0,0,w*15f);
        }
    }

    public sealed class UISpriteAnimationSequence:MonoBehaviour {
        Sprite[] frames;float interval=.04f,timer;bool loop=true,playing;int index;Image image;
        public void Configure(Sprite[] spriteFrames,float frameInterval,bool isLooping,bool autoPlay) {
            frames=spriteFrames;interval=Mathf.Max(.001f,frameInterval);loop=isLooping;image=GetComponent<Image>();
            if(autoPlay)Play();
        }
        public void Play(){index=0;timer=0;playing=frames!=null&&frames.Length>0;if(playing&&image)image.sprite=frames[0];}
        void Update() {
            if(!playing||!image)return;timer+=Time.deltaTime;
            while(timer>=interval) {
                timer-=interval;index++;
                if(index>=frames.Length){if(!loop){playing=false;return;}index=0;}
                image.sprite=frames[index];
            }
        }
    }
}

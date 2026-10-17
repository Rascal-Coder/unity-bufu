using Bufu.Rebuild.UI;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    public sealed partial class GameController {
        GameObject cutsceneTransition;
        public bool CutscenePlaying=>cutsceneTransition;

        // UIManager.PlayCutsceneTransition: original prefab at local zero, one-shot
        // guochang, destroyed on completion. Spine pixels map to canvas units.
        void PlayCutsceneTransition() { StartCutscene(null); }

        void PlayWorldCutscene(Action onCovered) { StartCutscene(onCovered); }

        void StartCutscene(Action onCovered) {
            if(cutsceneTransition)return;
            var host=NewRect("Rebirth Transition",systemLayer);Stretch(host);
            cutsceneTransition=host.gameObject;
            var blocker=host.gameObject.AddComponent<Image>();blocker.color=Color.clear;blocker.raycastTarget=true;
            var rect=NewRect("Original Road Roller",host);rect.sizeDelta=new Vector2(100,100);
            var graphic=rect.gameObject.AddComponent<SpineGraphic>();
            if(!graphic.Load("zgygy_cutscene")) {
                Destroy(host.gameObject);cutsceneTransition=null;
                Debug.LogWarning("Original rebirth cutscene resources are missing.");
                onCovered?.Invoke();return;
            }
            graphic.Play("guochang",false,()=>{
                cutsceneTransition=null;
                if(host)Destroy(host.gameObject);
            });
            if(onCovered!=null)StartCoroutine(InvokeWhenCovered(onCovered));
        }

        // Original world switch pauses guochang after 1.5s, while the screen is covered.
        IEnumerator InvokeWhenCovered(Action onCovered) {
            yield return new WaitForSecondsRealtime(1.5f);
            if(cutsceneTransition)onCovered?.Invoke();
        }
    }
}

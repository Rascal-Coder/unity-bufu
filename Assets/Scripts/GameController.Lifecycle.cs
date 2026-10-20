using System;
using UnityEngine;

namespace Bufu.Rebuild {
    public sealed partial class GameController {
        bool applicationPaused,applicationFocused=true;
        float backgroundTimeScale=1;
        public bool IsBackgrounded {get;private set;}

        void RefreshApplicationActivity() {
            if(!interfaceReady||Model==null||VerificationMode)return;
            bool background=applicationPaused||!applicationFocused;
            if(background==IsBackgrounded)return;
            IsBackgrounded=background;
            if(background) {
                backgroundTimeScale=Time.timeScale;Time.timeScale=0;
                SaveStore.Save(Model);
                return;
            }
            Time.timeScale=backgroundTimeScale;
            if(purchases!=null&&!purchases.Busy)purchases.Connect();
            Model.ResumeFromBackground(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            if(Model.State.pendingOffline>0&&offlineView==null&&!CutscenePlaying)ShowOffline();
        }
    }
}

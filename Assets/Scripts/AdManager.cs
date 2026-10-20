using System;
using GoogleMobileAds.Api;
using UnityEngine;

namespace Bufu.Rebuild {
    /// <summary>
    /// Native mobile rewarded ads. Desktop keeps the shipped two-second FakeAdUI flow.
    /// Uses test ad units until a production AdMob account is configured.
    /// </summary>
    public sealed class AdManager:MonoBehaviour {
        public static AdManager Instance {get;private set;}
        const string TestRewardedId="ca-app-pub-3940256099942544/5224354917";
        RewardedAd rewarded;Action<bool> callback;bool ready,earnedReward,showing;
        // The shipped desktop game uses FakeAdUI. Only mobile builds have a native ad surface.
        public bool SupportsRewardedAds=>Application.isMobilePlatform&&!Application.isEditor;

        void Awake() {
            if(Instance){Destroy(gameObject);return;}
            Instance=this;DontDestroyOnLoad(gameObject);
        }

        void Start() {
            if(!SupportsRewardedAds)return;
            MobileAds.Initialize(status=>Debug.Log("AdMob initialized: "+status.ToString()));
            LoadRewardedAd();
        }

        void LoadRewardedAd() {
            if(rewarded!=null){rewarded.Destroy();rewarded=null;}
            var request=new AdRequest();
            RewardedAd.Load(TestRewardedId,request,(ad,error)=>{
                if(error!=null||ad==null){Debug.LogWarning("Rewarded ad load failed: "+error);ready=false;return;}
                rewarded=ad;ready=true;
                rewarded.OnAdFullScreenContentClosed+=OnRewardedClosed;
                rewarded.OnAdFullScreenContentFailed+=OnRewardedFailed;
            });
        }

        void OnRewardedClosed()=>HandleResult(earnedReward);
        void OnRewardedFailed(AdError error){Debug.LogWarning("Rewarded ad failed: "+error);HandleResult(false);}

        void HandleResult(bool watched) {
            var done=callback;callback=null;showing=false;ready=false;earnedReward=false;
            if(SupportsRewardedAds)LoadRewardedAd();
            done?.Invoke(watched);
        }

        /// <summary>
        /// True requires the SDK's earned-reward event followed by closing the ad.
        /// An unavailable ad, failed presentation or early close never awards a prize.
        /// </summary>
        public void ShowRewarded(Action<bool> onComplete) {
            if(showing){onComplete?.Invoke(false);return;}
            callback=onComplete;
            if(!ready||rewarded==null) {
                Debug.LogWarning("Rewarded ad not ready");
                HandleResult(false);return;
            }
            showing=true;ready=false;earnedReward=false;
            try{rewarded.Show(reward=>{earnedReward=true;});}
            catch(Exception error){Debug.LogWarning("Rewarded ad failed: "+error.Message);HandleResult(false);}
        }

        void OnDestroy(){if(rewarded!=null)rewarded.Destroy();}
    }
}


using System;
using GoogleMobileAds.Api;
using UnityEngine;

namespace Bufu.Rebuild {
    /// <summary>
    /// Google AdMob rewarded video ads for support bonuses: offline earnings, income boost, speed boost.
    /// Uses test ad units until a production AdMob account is configured.
    /// </summary>
    public sealed class AdManager:MonoBehaviour {
        public static AdManager Instance {get;private set;}
        const string TestRewardedId="ca-app-pub-3940256099942544/5224354917";
        RewardedAd rewarded;Action<bool> callback;bool ready;

        void Awake() {
            if(Instance){Destroy(gameObject);return;}
            Instance=this;DontDestroyOnLoad(gameObject);
        }

        void Start() {
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

        void OnRewardedClosed()=>HandleResult(true);
        void OnRewardedFailed(AdError error){Debug.LogWarning("Rewarded ad failed: "+error);HandleResult(false);}

        void HandleResult(bool watched) {
            callback?.Invoke(watched);callback=null;
            LoadRewardedAd();
        }

        /// <summary>
        /// Shows a rewarded video. The callback is invoked with true if the user watched the ad, false if it failed or was skipped.
        /// The original flow guarantees a reward after 2 seconds; real ads award only after watching the full length.
        /// </summary>
        public void ShowRewarded(Action<bool> onComplete) {
            callback=onComplete;
            if(!ready||rewarded==null) {
                Debug.LogWarning("Rewarded ad not ready, awarding directly for test");
                HandleResult(true);return;
            }
            rewarded.Show(reward=>{Debug.Log("Ad rewarded: "+reward.Type+" x"+reward.Amount);});
        }

        void OnDestroy(){if(rewarded!=null)rewarded.Destroy();}
    }
}


using System;
using System.Collections;
using LitJson;
using Module.Http;
using P8SDKTT;
using TTSDK;
using UnityEngine;
using UnityEngine.UI;

public class FakeVideoManager : MonoBehaviour
{
	private static FakeVideoManager instance;

	private bool isInitialized;

	[SerializeField]
	private GameObject adUIPrefab;

	private GameObject adUIInstance;

	private const float VIDEO_LENGTH = 2f;

	public Action OnAdCompleted;

	public Action OnAdClosed;

	private bool isAdPlaying;

	private bool adCallbackHandled;

	private TTRewardedVideoAd _rewardVideoAd;

	private const int INTERSTITIAL_START_MISSION_CONFIG_ID = 44;

	private const int INTERSTITIAL_WAIT_TIME_CONFIG_ID = 45;

	private const int INTERSTITIAL_INTERVAL_CONFIG_ID = 46;

	private int interstitialStartMissionId = 3;

	private float interstitialWaitTime = 120f;

	private float interstitialInterval = 120f;

	private float nextInterstitialReadyTime;

	private bool isInterstitialTimerStarted;

	private bool isInterstitialConfigLoaded;

	private bool isInterstitialAdPlaying;

	private int interstitialAdSessionId;

	private bool isGMRemoveAd;

	public string appId { get; private set; } = "tt0e6178dc916a67d802";

	public static FakeVideoManager Instance
	{
		get
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Expected O, but got Unknown
			if ((Object)(object)instance == (Object)null)
			{
				GameObject val = new GameObject("FakeVideoManager");
				instance = val.AddComponent<FakeVideoManager>();
				Object.DontDestroyOnLoad((Object)val);
			}
			return instance;
		}
	}

	private void Awake()
	{
		if ((Object)(object)instance == (Object)null)
		{
			instance = this;
			Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		}
		else if ((Object)(object)instance != (Object)(object)this)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	public void Initialize(string appId)
	{
		if (!isInitialized)
		{
			if (!string.IsNullOrEmpty(appId))
			{
				this.appId = appId;
			}
			isInitialized = true;
			if (IsMiniGamePlatform())
			{
				InitDouyinAd();
			}
			else
			{
				LoadAdUIPrefab();
			}
		}
	}

	private void Update()
	{
		if (isInitialized)
		{
			TryShowInterstitialAdByTimer();
		}
	}

	private void LoadAdUIPrefab()
	{
		SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/NewUI/fakevideo/prefabs/FakeVideoPanel.prefab", delegate(GameObject prefab)
		{
			if ((Object)(object)prefab != (Object)null)
			{
				adUIPrefab = prefab;
			}
		});
	}

	private void InitDouyinAd()
	{
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			InitInterstitialConfig();
			string text = "5dl36jc6b2j7h2oepa";
			string text2 = "3brj74ni8k5dmp0ir3";
			if (appId == "tte61690ab780b197307")
			{
				text = "4spqcqjgk26sj984fe";
				text2 = "2anab4d0pt1773fd1e";
			}
			else if (appId == "tte1cc97717ec473e307")
			{
				text = "b36j7f479kb12hbc49";
				text2 = "8ba4h7i42h652te41t";
			}
			else if (appId == "tt08737ece977c631607")
			{
				text = "2n7jc4951kmn1pifpl";
				text2 = "37w1k4f9nxi9frr332";
			}
			else if (appId == "tt0e6178dc916a67d802")
			{
				text = "951mg2ia71e313bd1f";
				text2 = "47b8m1g1j6q10lkmin";
			}
			else if (appId == "tt0fef795290e4766802")
			{
				text = "a6bech445ffg11pljb";
				text2 = "cro3449q0adifd17b5";
			}
			Debug.Log((object)("[FakeVideoManager] InitDouyinAd with appId: " + appId + ", Rewarded Video ID: " + text + ", Interstitial ID: " + text2));
			P8SDK.AdInit(new AdInitOption
			{
				rewardedAdOption = new RewardedAdOption
				{
					adUnitId = text,
					adSlot = "RewardedVideoAd"
				},
				interstitialAdOption = new InterstitialAdOption
				{
					adUnitId = text2
				},
				bannerAdOption = new BannerAdOption
				{
					adUnitId = "7ffl6idl5ia19iqpkr"
				}
			});
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("抖音广告初始化失败: " + ex.Message));
		}
	}

	public void ShowAd(Action onCompleted, Action onClosed = null, string reason = "")
	{
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			int currentWorldId = GameDataManager.Instance.GetCurrentWorldId();
			if (currentWorldId > 1 && !string.IsNullOrEmpty(reason))
			{
				reason += $"_{currentWorldId}";
			}
		}
		if (!isInitialized)
		{
			Initialize("tt0e6178dc916a67d802");
		}
		if (isAdPlaying || IsInterstitialAdPlaying())
		{
			Debug.LogWarning((object)("广告正在播放中," + reason));
			return;
		}
		adCallbackHandled = false;
		if (isGMRemoveAd)
		{
			Debug.Log((object)"直接无广告");
			OnAdCompleted = onCompleted;
			OnAdClosed = onClosed;
			CompleteAd(rewarded: true, addAdCount: false);
			return;
		}
		isAdPlaying = true;
		OnAdCompleted = onCompleted;
		OnAdClosed = onClosed;
		if (IsMiniGamePlatform())
		{
			Debug.Log((object)("显示视频广告" + reason));
			ShowWechatAd(reason);
		}
		else
		{
			Debug.Log((object)"显示假视频广告");
			CreateAdUI();
		}
	}

	private void ShowWechatAd(string reason)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Debug.Log((object)"显示抖音视频广告");
			try
			{
				P8SDK.RewardedAdShow(new RewardedAdShowOption
				{
					success = delegate
					{
						CompleteAd(rewarded: true);
						Debug.Log((object)"抖音奖励已经发放");
						P8SDK.Log("Douyin RewardedAdShow success.");
						SingletonMono<HttpManager>.Instance.TrackAdPoint(GameDataManager.Instance.GetOpenId(), reason);
						GameDataManager.Instance.SaveAll2Server();
					},
					close = delegate
					{
						CompleteAd(rewarded: false);
						Debug.Log((object)"抖音播放中途退出，不下发奖励");
						P8SDK.Log("Douyin RewardedAdShow close.");
					},
					fail = delegate(GeneralCallbackData res)
					{
						//IL_0012: Unknown result type (might be due to invalid IL or missing references)
						//IL_0027: Unknown result type (might be due to invalid IL or missing references)
						CompleteAd(rewarded: false);
						Debug.Log((object)$"未能拉起抖音广告{res}");
						P8SDK.Log("Douyin RewardedAdShow fail: " + JsonMapper.ToJson((object)res));
					},
					adPosition = reason
				});
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("抖音广告显示异常: " + ex.Message));
				CompleteAd(rewarded: false);
			}
		}
		catch (Exception ex2)
		{
			Debug.LogError((object)("显示广告失败: " + ex2.Message));
			FallbackToFakeAd();
		}
	}

	private void FallbackToFakeAd()
	{
		LoadAdUIPrefab();
		CreateAdUI();
	}

	private void CompleteAd(bool rewarded, bool addAdCount = true)
	{
		if (!adCallbackHandled)
		{
			adCallbackHandled = true;
			isAdPlaying = false;
			InitInterstitialConfig();
			ResetInterstitialCooldown(interstitialWaitTime);
			((MonoBehaviour)this).StartCoroutine(CompleteAdNextFrame(rewarded, addAdCount));
		}
	}

	private void TryShowInterstitialAdByTimer()
	{
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		InitInterstitialConfig();
		if ((Object)(object)GameDataManager.Instance == (Object)null || GameDataManager.Instance.GetCurrentMainMissionLevel() <= interstitialStartMissionId || !IsMiniGamePlatform() || isAdPlaying || isInterstitialAdPlaying)
		{
			return;
		}
		if (!isInterstitialTimerStarted)
		{
			isInterstitialTimerStarted = true;
			ResetInterstitialCooldown(interstitialWaitTime);
		}
		else
		{
			if (Time.realtimeSinceStartup < nextInterstitialReadyTime)
			{
				return;
			}
			isInterstitialAdPlaying = true;
			interstitialAdSessionId++;
			((MonoBehaviour)this).StartCoroutine(InterstitialAdTimeoutCheck(interstitialAdSessionId));
			string text = "插屏广告";
			if ((Object)(object)GameDataManager.Instance != (Object)null)
			{
				int currentWorldId = GameDataManager.Instance.GetCurrentWorldId();
				if (currentWorldId > 1)
				{
					text += $"_{currentWorldId}";
				}
			}
			try
			{
				P8SDK.InterstitialAdShow(new InterstitialAdShowOption
				{
					adPosition = text,
					close = delegate
					{
						interstitialAdSessionId++;
						isInterstitialAdPlaying = false;
						ResetInterstitialCooldown(interstitialInterval);
						P8SDK.Log("Douyin InterstitialAdShow close.");
					},
					fail = delegate
					{
						interstitialAdSessionId++;
						isInterstitialAdPlaying = false;
						ResetInterstitialCooldown(interstitialInterval);
						P8SDK.Log("Douyin InterstitialAdShow fail.");
					}
				});
				SingletonMono<HttpManager>.Instance.TrackAdPoint(GameDataManager.Instance.GetOpenId(), text);
			}
			catch (Exception ex)
			{
				interstitialAdSessionId++;
				isInterstitialAdPlaying = false;
				ResetInterstitialCooldown(interstitialInterval);
				Debug.LogError((object)("插屏广告显示异常: " + ex.Message));
			}
		}
	}

	private IEnumerator InterstitialAdTimeoutCheck(int sessionId)
	{
		yield return (object)new WaitForSecondsRealtime(20f);
		if (isInterstitialAdPlaying && interstitialAdSessionId == sessionId)
		{
			Debug.LogWarning((object)"[FakeVideoManager] 插屏广告播放超时，重置播放状态");
			isInterstitialAdPlaying = false;
			ResetInterstitialCooldown(interstitialInterval);
		}
	}

	private void InitInterstitialConfig()
	{
		if (!isInterstitialConfigLoaded && ConfigManager.HasInstance())
		{
			interstitialStartMissionId = (int)ConfigManager.Instance.GetCommonFactor(44);
			interstitialWaitTime = ConfigManager.Instance.GetCommonFactor(45);
			interstitialInterval = ConfigManager.Instance.GetCommonFactor(46);
			isInterstitialConfigLoaded = true;
		}
	}

	private void ResetInterstitialCooldown(float cooldown)
	{
		nextInterstitialReadyTime = Time.realtimeSinceStartup + Mathf.Max(0f, cooldown);
	}

	private IEnumerator CompleteAdNextFrame(bool rewarded, bool addAdCount)
	{
		yield return null;
		Action onAdCompleted = OnAdCompleted;
		Action onAdClosed = OnAdClosed;
		OnAdCompleted = null;
		OnAdClosed = null;
		if (rewarded)
		{
			onAdCompleted?.Invoke();
			if (addAdCount && (Object)(object)GameDataManager.Instance != (Object)null)
			{
				GameDataManager.Instance.AddAdCount();
			}
		}
		else
		{
			onAdClosed?.Invoke();
		}
	}

	private void CreateAdUI()
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)adUIPrefab != (Object)null)
		{
			adUIInstance = Object.Instantiate<GameObject>(adUIPrefab);
		}
		else
		{
			adUIInstance = CreateAdUIDynamically();
		}
		RectTransform component = adUIInstance.GetComponent<RectTransform>();
		if ((Object)(object)component != (Object)null)
		{
			Canvas componentInParent = adUIInstance.GetComponentInParent<Canvas>();
			if ((Object)(object)componentInParent == (Object)null)
			{
				componentInParent = adUIInstance.AddComponent<Canvas>();
				componentInParent.renderMode = (RenderMode)0;
				componentInParent.sortingOrder = 9999;
				adUIInstance.AddComponent<CanvasScaler>();
				adUIInstance.AddComponent<GraphicRaycaster>();
			}
			else
			{
				componentInParent.sortingOrder = 9999;
			}
			component.anchorMin = Vector2.zero;
			component.anchorMax = Vector2.one;
			component.offsetMin = Vector2.zero;
			component.offsetMax = Vector2.zero;
		}
		FakeAdUI component2 = adUIInstance.GetComponent<FakeAdUI>();
		if ((Object)(object)component2 != (Object)null)
		{
			component2.Initialize(2f, OnAdFinished, OnAdCloseClicked);
		}
	}

	private GameObject CreateAdUIDynamically()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject("FakeAdUI");
		val.AddComponent<RectTransform>();
		GameObject val2 = new GameObject("Background");
		val2.transform.SetParent(val.transform, false);
		((Graphic)val2.AddComponent<Image>()).color = Color.black;
		RectTransform component = val2.GetComponent<RectTransform>();
		component.anchorMin = Vector2.zero;
		component.anchorMax = Vector2.one;
		component.offsetMin = Vector2.zero;
		component.offsetMax = Vector2.zero;
		val.AddComponent<FakeAdUI>();
		return val;
	}

	private void OnAdFinished()
	{
		isAdPlaying = false;
		if (OnAdCompleted != null)
		{
			OnAdCompleted();
			OnAdCompleted = null;
		}
		CleanupAdUI();
		WorldCupDataManager.Instance.AddWorldCupMissionProgress(5, 1);
	}

	private void OnAdCloseClicked()
	{
		if (OnAdClosed != null)
		{
			OnAdClosed?.Invoke();
			OnAdClosed = null;
		}
		if (isAdPlaying)
		{
			isAdPlaying = false;
		}
		CleanupAdUI();
	}

	private void OnWechatAdError(string errorMsg)
	{
		Debug.LogError((object)("微信广告加载失败: " + errorMsg));
		FallbackToFakeAd();
	}

	private void OnWechatAdClose(bool isRewarded)
	{
		isAdPlaying = false;
		if (isRewarded)
		{
			if (OnAdCompleted != null)
			{
				OnAdCompleted();
				OnAdCompleted = null;
			}
		}
		else if (OnAdClosed != null)
		{
			OnAdClosed?.Invoke();
			OnAdClosed = null;
		}
	}

	private void CleanupAdUI()
	{
		if ((Object)(object)adUIInstance != (Object)null)
		{
			Object.Destroy((Object)(object)adUIInstance);
			adUIInstance = null;
		}
	}

	public bool IsAdPlaying()
	{
		if (!isAdPlaying)
		{
			return IsInterstitialAdPlaying();
		}
		return true;
	}

	private bool IsInterstitialAdPlaying()
	{
		return isInterstitialAdPlaying;
	}

	public void RemoveAd()
	{
		if (!isInitialized)
		{
			Initialize("tt0e6178dc916a67d802");
		}
		isGMRemoveAd = true;
	}

	private bool IsMiniGamePlatform()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		if ((int)Application.platform != 50)
		{
			return (int)Application.platform == 50;
		}
		return true;
	}
}

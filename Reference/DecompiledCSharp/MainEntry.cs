using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using LitJson;
using Module.Http;
using Module.Mission;
using P8SDKTT;
using TTSDK;
using TTSDK.UNBridgeLib.LitJson;
using UnityEngine;
using YooAsset;

public class MainEntry : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static Action<LoginResult> _003C_003E9__9_5;

		public static Action<GeneralCallbackData> _003C_003E9__9_6;

		public static Action<int, string> _003C_003E9__9_8;

		public static OnLoginFailedCallback _003C_003E9__9_1;

		public static Action<bool, string> _003C_003E9__9_2;

		public static OnAppHideEvent _003C_003E9__9_3;

		public static OnShowEventWithDict _003C_003E9__9_4;

		public static Action<bool, string> _003C_003E9__11_1;

		public static Action<GeneralCallbackData> _003C_003E9__15_1;

		public static Action<JsonData> _003C_003E9__39_0;

		public static Action<int, string> _003C_003E9__39_1;

		public static Action _003C_003E9__39_2;

		public static Action _003C_003E9__39_3;

		public static Action<GeneralCallbackData> _003C_003E9__39_4;

		public static Action _003C_003E9__40_0;

		internal void _003CAwake_003Eb__9_5(LoginResult res)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			P8SDK.Log("login success: " + JsonMapper.ToJson((object)res));
		}

		internal void _003CAwake_003Eb__9_6(GeneralCallbackData res)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			P8SDK.Log("login fail: " + JsonMapper.ToJson((object)res));
		}

		internal void _003CAwake_003Eb__9_8(int errCode, string errMsg)
		{
			Debug.Log((object)$"抖音登录失败：{errCode} {errMsg}");
		}

		internal void _003CAwake_003Eb__9_1(string errorMsg)
		{
			Debug.Log((object)("登录失败：" + errorMsg));
		}

		internal void _003CAwake_003Eb__9_2(bool isSuccess, string msg)
		{
			if (isSuccess)
			{
				Debug.Log((object)("助力成功: " + msg));
			}
			else
			{
				Debug.Log((object)("助力失败: " + msg));
			}
		}

		internal void _003CAwake_003Eb__9_3()
		{
			Debug.Log((object)"[MainEntry] OnHide 被调用，执行 SaveGameData");
			GameDataManager.Instance.SaveGameData();
			GameDataManager.Instance.SaveAll2Server();
		}

		internal void _003CAwake_003Eb__9_4(Dictionary<string, object> options)
		{
			Debug.LogWarning((object)("OnShow options: " + JsonMapper.ToJson((object)options)));
			object value;
			string text = (options.TryGetValue("query", out value) ? value.ToString() : "");
			if ((Object)(object)GameDataManager.Instance != (Object)null)
			{
				GameDataManager.Instance.MarkPlayerActivelyEntered();
				GameDataManager.Instance.ForceUpdateGameFocus();
			}
			Debug.LogWarning((object)("OnShow query~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~: " + text));
		}

		internal void _003CProcessLoginData_003Eb__11_1(bool isSuccess, string msg)
		{
			if (isSuccess)
			{
				Debug.Log((object)("助力成功: " + msg));
			}
			else
			{
				Debug.Log((object)("助力失败: " + msg));
			}
		}

		internal void _003CTrySendP8SignLog_003Eb__15_1(GeneralCallbackData res)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			P8SDK.Log("SignLog fail: " + JsonMapper.ToJson((object)res));
		}

		internal void _003CLazyInitializeManagers_003Eb__39_0(JsonData successRes)
		{
			Debug.Log((object)$"直玩场景上报成功{successRes}");
		}

		internal void _003CLazyInitializeManagers_003Eb__39_1(int code, string failRes)
		{
			Debug.LogError((object)$"直玩场景上报失败{code},{failRes}");
		}

		internal void _003CLazyInitializeManagers_003Eb__39_2()
		{
			Debug.Log((object)"ReportScene onComplete");
		}

		internal void _003CLazyInitializeManagers_003Eb__39_3()
		{
			P8SDK.Log("PushLoginData success.");
		}

		internal void _003CLazyInitializeManagers_003Eb__39_4(GeneralCallbackData res)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			P8SDK.Log("PushLoginData fail: " + JsonMapper.ToJson((object)res));
		}

		internal void _003CRedundantWork_003Eb__40_0()
		{
			UIManager.Instance.ShowDownItem();
		}
	}

	public EPlayMode playMode;

	private string role_uuid;

	private bool isPreloadComplete;

	private bool isVideoConfirmComplete;

	private bool isSaveRecoveryComplete = true;

	private bool hasStartedGameReal;

	private OfflineIncomeManager offlineIncomeManager;

	private bool hasDirectPlayFeedEntered;

	private bool _isFeedAcquisitionGame;

	private string _supportOpenid = "";

	private string _activityID = "";

	private int straitPlay;

	private bool pendingDirectPlayP8Logs;

	private bool isDirectPlayPushLoginInProgress;

	private bool isDirectPlayPushLoginCompleted;

	private static string SideBAR_HAS_GET_KEY = "SideBAR_HAS_GET_M1B";

	private void Awake()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Invalid comparison between Unknown and I4
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Invalid comparison between Unknown and I4
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Invalid comparison between Unknown and I4
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Expected O, but got Unknown
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Expected O, but got Unknown
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Invalid comparison between Unknown and I4
		//IL_03b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Expected O, but got Unknown
		//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03aa: Invalid comparison between Unknown and I4
		//IL_03d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03db: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Expected O, but got Unknown
		//IL_03ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0404: Unknown result type (might be due to invalid IL or missing references)
		//IL_040a: Expected O, but got Unknown
		if (Object.FindObjectsOfType<MainEntry>().Length > 1)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		role_uuid = LocalUserId.GetExisting();
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		SingletonMono<PreloadManager>.Instance.Init();
		InitializeYooAssets();
		Debug.Log((object)("MainEntry Awake:" + Application.streamingAssetsPath));
		string _code = "";
		if ((int)Application.platform == 17 || (int)Application.platform == 50)
		{
			Debug.Log((object)"P8登录:P8SDK.Login");
			P8SDK.Login(new LoginOption
			{
				success = delegate(LoginResult res)
				{
					//IL_0005: Unknown result type (might be due to invalid IL or missing references)
					P8SDK.Log("login success: " + JsonMapper.ToJson((object)res));
				},
				fail = delegate(GeneralCallbackData res)
				{
					//IL_0005: Unknown result type (might be due to invalid IL or missing references)
					P8SDK.Log("login fail: " + JsonMapper.ToJson((object)res));
				}
			});
		}
		((Component)this).gameObject.AddComponent<TTFeedDirectPlayManager>();
		LaunchOption launchOptionsSync = TT.GetLaunchOptionsSync();
		string appId = "tt0e6178dc916a67d802";
		if ((int)Application.platform == 50)
		{
			if (launchOptionsSync != null && launchOptionsSync.Extra != null && launchOptionsSync.Extra.ContainsKey("appId"))
			{
				appId = launchOptionsSync.Extra["appId"].ToString();
			}
			FakeVideoManager.Instance.Initialize(appId);
		}
		OnLoginSuccessCallback val = delegate(string code, string anonymousCode, bool isNewUser)
		{
			Debug.Log((object)$"登录成功：{code} \n{anonymousCode} \n{isNewUser}");
			_code = code;
			if (HttpManager.CanSendServerRequests)
			{
				SingletonMono<HttpManager>.Instance.GetDouyinOpenId(_code, anonymousCode, appId, delegate(string res)
				{
					Debug.Log((object)("抖音登录成功：" + res));
					JsonData obj4 = JsonMapper.ToObject<JsonData>(res);
					string text = ((object)obj4["openId"]).ToString();
					string sessionKey = ((object)obj4["sessionKey"]).ToString();
					Debug.Log((object)("抖音openID：" + text));
					((MonoBehaviour)this).StartCoroutine(ProcessLoginData(text, sessionKey));
				}, delegate(int errCode, string errMsg)
				{
					Debug.Log((object)$"抖音登录失败：{errCode} {errMsg}");
				});
			}
			TTFeedDirectPlayManager.Instance.OnGameStart(appId);
		};
		object obj = _003C_003Ec._003C_003E9__9_1;
		if (obj == null)
		{
			OnLoginFailedCallback val2 = delegate(string errorMsg)
			{
				Debug.Log((object)("登录失败：" + errorMsg));
			};
			_003C_003Ec._003C_003E9__9_1 = val2;
			obj = (object)val2;
		}
		TT.Login(val, (OnLoginFailedCallback)obj, false);
		TT.SetOnBeforeExitAppListener((Func<bool>)OnDouyinExit);
		if (launchOptionsSync != null)
		{
			Debug.Log((object)("path :" + launchOptionsSync.Path));
			Debug.Log((object)("scene :" + launchOptionsSync.Scene));
			Debug.Log((object)("subScene :" + launchOptionsSync.SubScene));
			Debug.Log((object)("group_id :" + launchOptionsSync.GroupId));
			Debug.Log((object)("shareTicket :" + launchOptionsSync.ShareTicket));
			Debug.Log((object)("is_sticky :" + launchOptionsSync.IsSticky));
		}
		Debug.Log((object)"query :");
		if (launchOptionsSync != null && launchOptionsSync.Query != null)
		{
			foreach (KeyValuePair<string, string> item in launchOptionsSync.Query)
			{
				if (item.Value != null)
				{
					Debug.Log((object)(item.Key + ": " + item.Value));
					if (item.Key.ToString() == "feed_game_channel")
					{
						_isFeedAcquisitionGame = true;
						Debug.Log((object)("[MainEntry] 検测到获客版直玩入口，feed_game_channel = " + item.Value));
					}
					if (item.Key.ToString() == "uid")
					{
						_supportOpenid = item.Value;
					}
					if (item.Key.ToString() == "activity")
					{
						_activityID = item.Value;
					}
				}
				else
				{
					Debug.Log((object)(item.Key + ": null "));
				}
			}
			if (SingletonMono<HttpManager>.Instance.IsInitialized)
			{
				SingletonMono<HttpManager>.Instance.ShareHelp(_supportOpenid, null, delegate(bool isSuccess, string msg)
				{
					if (isSuccess)
					{
						Debug.Log((object)("助力成功: " + msg));
					}
					else
					{
						Debug.Log((object)("助力失败: " + msg));
					}
				});
			}
		}
		if (launchOptionsSync != null && (launchOptionsSync.Scene ?? "").IndexOf("3041", StringComparison.Ordinal) != -1)
		{
			straitPlay = 2;
		}
		TTFeedDirectPlayManager.Instance.CheckDirectPlayEntry(launchOptionsSync);
		if ((int)Application.platform == 17 || (int)Application.platform == 50)
		{
			TT.OnFeedStatusChange(new OnFeedStatusChangeCallback(OnFeedStatusChangeCallback));
		}
		TTAppLifeCycle appLifeCycle = TT.GetAppLifeCycle();
		object obj2 = _003C_003Ec._003C_003E9__9_3;
		if (obj2 == null)
		{
			OnAppHideEvent val3 = delegate
			{
				Debug.Log((object)"[MainEntry] OnHide 被调用，执行 SaveGameData");
				GameDataManager.Instance.SaveGameData();
				GameDataManager.Instance.SaveAll2Server();
			};
			_003C_003Ec._003C_003E9__9_3 = val3;
			obj2 = (object)val3;
		}
		appLifeCycle.OnHide += (OnAppHideEvent)obj2;
		TTAppLifeCycle appLifeCycle2 = TT.GetAppLifeCycle();
		object obj3 = _003C_003Ec._003C_003E9__9_4;
		if (obj3 == null)
		{
			OnShowEventWithDict val4 = delegate(Dictionary<string, object> options)
			{
				Debug.LogWarning((object)("OnShow options: " + JsonMapper.ToJson((object)options)));
				object value;
				string text = (options.TryGetValue("query", out value) ? value.ToString() : "");
				if ((Object)(object)GameDataManager.Instance != (Object)null)
				{
					GameDataManager.Instance.MarkPlayerActivelyEntered();
					GameDataManager.Instance.ForceUpdateGameFocus();
				}
				Debug.LogWarning((object)("OnShow query~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~: " + text));
			};
			_003C_003Ec._003C_003E9__9_4 = val4;
			obj3 = (object)val4;
		}
		appLifeCycle2.OnShow += (OnShowEventWithDict)obj3;
	}

	private bool OnDouyinExit()
	{
		GameDataManager.Instance.SaveGameData();
		GameDataManager.Instance.SaveAll2Server();
		TTFeedDirectPlayManager.Instance.ReportRevisitData();
		return false;
	}

	private IEnumerator ProcessLoginData(string openID, string sessionKey)
	{
		while ((Object)(object)GameDataManager.Instance == (Object)null)
		{
			yield return null;
		}
		GameDataManager.Instance.SetOpenId(openID, sessionKey);
		SingletonMono<HttpManager>.Instance.Initialize(openID, sessionKey);
		if (GameDataManager.Instance.IsGameDataLoadError || GameDataManager.Instance.IsFacilityDataLoadError || GameDataManager.Instance.IsUpgradeDataLoadError || GameDataManager.Instance.IsTalentDataLoadError || GameDataManager.Instance.IsWorldCupDataLoadError)
		{
			bool isRecovering = true;
			bool isRecoverSuccess = false;
			bool shouldResumeCloudSync = false;
			isSaveRecoveryComplete = false;
			GameDataManager.Instance.SetCloudSyncSuspended(suspended: true);
			Debug.LogWarning((object)"检测到部分存档加载失败，开始从服务器请求恢复数据...");
			while ((Object)(object)UIManager.Instance == (Object)null)
			{
				yield return null;
			}
			SingletonMono<HttpManager>.Instance.GetCurrentWorldForRestore(openID, delegate(bool success, string json)
			{
				if (success && !string.IsNullOrEmpty(json))
				{
					if (GameDataManager.Instance.RestoreWorldFromServerResponse(json, out var error))
					{
						isRecoverSuccess = true;
						shouldResumeCloudSync = true;
						Debug.Log((object)"[MainEntry] 当前活动世界已从服务器恢复。");
					}
					else
					{
						Debug.LogWarning((object)("服务器当前世界存档无效，云同步保持挂起: " + error));
					}
				}
				else
				{
					Debug.LogError((object)"获取备份存档失败，继续入场但保持云同步挂起，避免覆盖服务器备份");
				}
				isRecovering = false;
			});
			while (isRecovering)
			{
				yield return null;
			}
			if (shouldResumeCloudSync)
			{
				GameDataManager.Instance.SetCloudSyncSuspended(suspended: false);
				GameDataManager.Instance.RetryPendingWorldSaves();
			}
			else
			{
				Debug.LogWarning((object)"[MainEntry] Save recovery did not produce safe data; cloud sync remains suspended to protect server backup.");
			}
			isSaveRecoveryComplete = true;
			Debug.Log((object)$"[MainEntry] Save recovery finished. success={isRecoverSuccess}, resumeCloudSync={shouldResumeCloudSync}");
			CheckEnterGame();
		}
		else
		{
			GameDataManager.Instance.RetryPendingWorldSaves();
		}
		TrySendDirectPlayP8Logs();
		Debug.Log((object)("_supportOpenid = " + _supportOpenid + ",我的openid:" + GameDataManager.Instance.GetOpenId()));
		Debug.Log((object)("_activityID = " + _activityID));
		if (string.IsNullOrEmpty(_supportOpenid))
		{
			yield break;
		}
		Debug.Log((object)("需要助力:" + _supportOpenid));
		string supportOpenid = _supportOpenid;
		_supportOpenid = "";
		_activityID = "";
		SingletonMono<HttpManager>.Instance.ShareHelp(supportOpenid, null, delegate(bool isSuccess, string msg)
		{
			if (isSuccess)
			{
				Debug.Log((object)("助力成功: " + msg));
			}
			else
			{
				Debug.Log((object)("助力失败: " + msg));
			}
		});
	}

	private void RestartMiniGame()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		try
		{
			if ((int)Application.platform == 50)
			{
				TT.RestartMiniProgramSync();
			}
		}
		catch
		{
		}
	}

	private void TrySendP8SignLog()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		if (!GameDataManager.HasInstance())
		{
			return;
		}
		string roleId = GameDataManager.Instance.GetUserId();
		if (!string.IsNullOrEmpty(roleId) && !string.Equals(roleId, role_uuid, StringComparison.Ordinal))
		{
			string text = GameDataManager.Instance.GetOpenId();
			if (string.IsNullOrEmpty(text))
			{
				text = roleId;
			}
			role_uuid = roleId;
			P8SDK.SignLog(new SignLogOption
			{
				level = "1",
				roleid = roleId,
				rolename = text,
				sid = SingletonMono<ResManager>.Instance.Version,
				success = delegate
				{
					P8SDK.Log("SignLog success. roleId=" + roleId);
				},
				fail = delegate(GeneralCallbackData res)
				{
					//IL_0005: Unknown result type (might be due to invalid IL or missing references)
					P8SDK.Log("SignLog fail: " + JsonMapper.ToJson((object)res));
				}
			});
		}
	}

	private void OnFeedStatusChangeCallback(OnFeedStatusChangeResult result)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		SendP8(result);
		Debug.Log((object)("OnFeedStatusChangeCallback, Type -->" + ((object)result.Type/*cast due to .constrained prefix*/).ToString()));
		if (!(((object)result.Type/*cast due to .constrained prefix*/).ToString().ToLower() != "feedenter") && IsFromFeedScene())
		{
			hasDirectPlayFeedEntered = true;
			offlineIncomeManager?.NotifyDirectPlayEntered();
		}
	}

	private void SendP8(OnFeedStatusChangeResult result)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (((object)result.Type/*cast due to .constrained prefix*/).ToString().ToLower() == "feedenter" && straitPlay == 2)
		{
			pendingDirectPlayP8Logs = true;
			TrySendDirectPlayP8Logs();
		}
	}

	private void TrySendDirectPlayP8Logs()
	{
		if (pendingDirectPlayP8Logs && isSaveRecoveryComplete && GameDataManager.HasInstance())
		{
			GameDataManager.Instance.GetGameData();
			string userId = GameDataManager.Instance.GetUserId();
			TrySendP8SignLog();
			if (!string.IsNullOrEmpty(userId) && !isDirectPlayPushLoginInProgress && !isDirectPlayPushLoginCompleted && !string.IsNullOrEmpty(SingletonMono<ResManager>.Instance.Version))
			{
				isDirectPlayPushLoginInProgress = true;
			}
		}
	}

	private void InitializeYooAssets()
	{
		string text = AppConst.WebUrl + "StreamingAssets/stonehunter/gameres";
		SingletonMono<ResManager>.Instance.InitializeInMiniGameMode(text, text, OnYooAssetsInitialized);
	}

	private void OnYooAssetsInitialized(bool success, string message)
	{
		if (success)
		{
			Debug.Log((object)"资源系统初始化成功");
			((MonoBehaviour)this).StartCoroutine(InitializeGameConfig());
			CheckAndPlayIntroVideo();
		}
		else
		{
			Debug.LogError((object)("资源管理器初始化失败: " + message));
		}
	}

	private IEnumerator InitializeGameConfig()
	{
		ConfigManager configManager = ((Component)this).gameObject.AddComponent<ConfigManager>();
		if (Application.isMobilePlatform || 50 == (int)Application.platform)
		{
			yield return configManager.InitializeAsync();
		}
		else
		{
			configManager.Initialize();
		}
		Debug.Log((object)"游戏配置初始化完成");
		InitializeManagers();
		Debug.Log((object)"初始化了GameManager");
		SingletonMono<PreloadManager>.Instance.StartPreload(StartGame);
	}

	private void CheckAndPlayIntroVideo()
	{
		if (IntroVideoPlayer.HasVideoPlayed)
		{
			isVideoConfirmComplete = true;
			return;
		}
		isVideoConfirmComplete = false;
		Debug.Log((object)"检测到用户尚未观看开场视频，开始加载预制体并行播放...");
		string prefabPath = "Assets/GameRes/NewUI/video/IntroVideoContainer.prefab";
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(prefabPath, delegate(GameObject prefab)
		{
			if ((Object)(object)prefab == (Object)null)
			{
				Debug.LogError((object)("加载视频预制体失败: " + prefabPath));
				isVideoConfirmComplete = true;
				CheckEnterGame();
			}
			else
			{
				GameObject val = GameObject.Find("Canvas");
				if ((Object)(object)val != (Object)null)
				{
					Transform val2 = val.transform.Find("Loading");
					if ((Object)(object)val2 != (Object)null)
					{
						((Component)val2).gameObject.SetActive(true);
						GameObject containerInstance = Object.Instantiate<GameObject>(prefab, val2, false);
						((Object)containerInstance).name = "IntroVideoContainer";
						IntroVideoPlayer videoPlayer = containerInstance.GetOrAddComponent<IntroVideoPlayer>();
						videoPlayer.SetVideoContainer(containerInstance);
						videoPlayer.PlayVideo("https://bsls-cdn.jvplay.cn/m1b/mv.mp4", delegate
						{
							if (ShouldSkipIntroConfirmForDirectPlay())
							{
								PlayerPrefs.SetInt("IntroVideoPlayed", 1);
								PlayerPrefs.Save();
								Debug.Log((object)"抖音直玩入口跳过片头目标确认弹窗，直接进入游戏");
								UIManager.Instance?.HideGameTargetDialog();
								Object.Destroy((Object)(object)containerInstance);
								isVideoConfirmComplete = true;
								CheckEnterGame();
							}
							else
							{
								videoPlayer.ShowConfirmUI("温馨提示", "已看完片头，点击确定开始你的财富之旅！", "开始游戏", delegate
								{
									PlayerPrefs.SetInt("IntroVideoPlayed", 1);
									PlayerPrefs.Save();
									Debug.Log((object)"视频播放完毕且确认，写入标志位并销毁容器");
									Object.Destroy((Object)(object)containerInstance);
								});
								isVideoConfirmComplete = true;
								CheckEnterGame();
							}
						});
					}
				}
			}
		});
	}

	private bool ShouldSkipIntroConfirmForDirectPlay()
	{
		return IsDirectPlayAcquisitionEntry();
	}

	private bool IsDirectPlayAcquisitionEntry()
	{
		return _isFeedAcquisitionGame;
	}

	private bool IsFromFeedScene()
	{
		if (straitPlay == 0)
		{
			if ((Object)(object)TTFeedDirectPlayManager.Instance != (Object)null)
			{
				return TTFeedDirectPlayManager.Instance.IsFromDirectPlay;
			}
			return false;
		}
		return true;
	}

	private void StartGame()
	{
		isPreloadComplete = true;
		Debug.Log((object)"后台资源预加载跑完");
		CheckEnterGame();
	}

	private void CheckEnterGame()
	{
		if (!hasStartedGameReal)
		{
			if (!isSaveRecoveryComplete)
			{
				Debug.Log((object)$"[MainEntry] Waiting for save recovery before entering game. preload={isPreloadComplete}, video={isVideoConfirmComplete}");
			}
			else if (isPreloadComplete && isVideoConfirmComplete)
			{
				Debug.Log((object)"并行加载同步栅栏解锁，正式进入游戏！");
				hasStartedGameReal = true;
				StartGameReal();
			}
			else
			{
				Debug.Log((object)$"同步栅栏等待中... 预加载完成: {isPreloadComplete}, 视频确认完成: {isVideoConfirmComplete}");
			}
		}
	}

	private bool ShouldBlockForSaveRecovery()
	{
		return !Application.isEditor;
	}

	private bool HasLocalSaveLoadError()
	{
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			if (!GameDataManager.Instance.IsGameDataLoadError && !GameDataManager.Instance.IsFacilityDataLoadError && !GameDataManager.Instance.IsUpgradeDataLoadError && !GameDataManager.Instance.IsTalentDataLoadError)
			{
				return GameDataManager.Instance.IsWorldCupDataLoadError;
			}
			return true;
		}
		return false;
	}

	private void StartGameReal()
	{
		PlayerPrefs.SetInt("PreloadComplete", 1);
		PlayerPrefs.Save();
		Debug.Log((object)"预加载完成，已写入标志位");
		SingletonMono<PreloadManager>.Instance.LoadingFadeOut();
		Object.DontDestroyOnLoad((Object)(object)GameObject.Find("Canvas"));
		Object.DontDestroyOnLoad((Object)(object)GameObject.Find("EventSystem"));
		UIManager.Instance.ShowMainScene(delegate
		{
			OnLoadMainScene();
		});
	}

	private void Update()
	{
	}

	private void InitializeManagers()
	{
		Application.targetFrameRate = 60;
		Application.runInBackground = true;
		Screen.orientation = (ScreenOrientation)1;
		CreateDataManager();
		if (ShouldBlockForSaveRecovery() && HasLocalSaveLoadError())
		{
			isSaveRecoveryComplete = false;
			GameDataManager.Instance.SetCloudSyncSuspended(suspended: true);
			Debug.Log((object)"[MainEntry] Local save load error detected; waiting for server recovery before entering game.");
		}
		TrySendDirectPlayP8Logs();
		GameObject.Find("GameDataManager").AddComponent<UIManager>();
		SingletonMono<UpgradeManager>.Instance.LateInitialize();
	}

	private void CreateDataManager()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		GameObject val = GameObject.Find("GameDataManager");
		if ((Object)(object)val == (Object)null)
		{
			val = new GameObject("GameDataManager");
		}
		val.GetOrAddComponent<GameDataManager>();
	}

	private void OnLoadMainScene()
	{
		_ = GameDataManager.Instance.GetGameData().CurrentLevel;
		UIManager.Instance.ShowUpgradeUI(delegate
		{
			LazyInitializeManagers(1);
		});
	}

	private void LazyInitializeManagers(int currentLevel)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Expected O, but got Unknown
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)((Component)UIManager.Instance).gameObject == (Object)null)
		{
			Debug.LogError((object)"GameController实例不存在");
		}
		GameObject val = GameObject.Find("Managers");
		if ((Object)(object)val == (Object)null)
		{
			val = new GameObject("Managers");
		}
		Object.DontDestroyOnLoad((Object)(object)val);
		_ = 1;
		GameObject val2 = new GameObject("SoundManager");
		val2.transform.SetParent(val.transform);
		val2.AddComponent<SoundManager>();
		if (OfflineIncomeManager.HasInstance())
		{
			offlineIncomeManager = OfflineIncomeManager.Instance;
		}
		else
		{
			GameObject val3 = new GameObject("OfflineManager");
			val3.transform.SetParent(val.transform);
			offlineIncomeManager = val3.AddComponent<OfflineIncomeManager>();
		}
		offlineIncomeManager.ConfigureStartupPopup(IsFromFeedScene(), hasDirectPlayFeedEntered, IsDirectPlayAcquisitionEntry(), IsDirectPlayAcquisitionEntry());
		MissionManager.Instance.Initialize();
		((Component)SingletonMono<VibrationManager>.Instance).transform.SetParent(val.transform);
		GameData gameData = GameDataManager.Instance.GetGameData();
		((Component)this).gameObject.AddComponent<GM>();
		TT.ReportScene(new JsonData
		{
			["sceneId"] = JsonData.op_Implicit(7001),
			["costTime"] = JsonData.op_Implicit((double)Time.realtimeSinceStartup)
		}, (Action<JsonData>)delegate(JsonData successRes)
		{
			Debug.Log((object)$"直玩场景上报成功{successRes}");
		}, (Action<int, string>)delegate(int code, string failRes)
		{
			Debug.LogError((object)$"直玩场景上报失败{code},{failRes}");
		}, (Action)delegate
		{
			Debug.Log((object)"ReportScene onComplete");
		});
		string userId = GameDataManager.Instance.GetUserId();
		if (straitPlay != 2)
		{
			TrySendP8SignLog();
		}
		Debug.Log((object)"登录上报PushLoginData");
		if (straitPlay != 2)
		{
			P8SDK.PushLoginData(new PushLoginDataOption
			{
				sid = SingletonMono<ResManager>.Instance.Version,
				level = "1",
				onlinetime = gameData.lastEnterGameTime.ToString(),
				roleid = userId,
				rolename = userId,
				username = userId,
				vip = straitPlay.ToString(),
				success = delegate
				{
					P8SDK.Log("PushLoginData success.");
				},
				fail = delegate(GeneralCallbackData res)
				{
					//IL_0005: Unknown result type (might be due to invalid IL or missing references)
					P8SDK.Log("PushLoginData fail: " + JsonMapper.ToJson((object)res));
				}
			});
		}
		RedundantWork();
	}

	private void RedundantWork()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Invalid comparison between Unknown and I4
		_ = ((Component)UIManager.Instance).gameObject;
		UIManager.Instance.ShowTopUI(delegate
		{
			UIManager.Instance.ShowDownItem();
		});
		FakeVideoManager instance = FakeVideoManager.Instance;
		if ((int)Application.platform != 50)
		{
			instance.Initialize("");
		}
	}

	private void OnApplicationQuit()
	{
	}

	public void RunGame(string versiontxt)
	{
		Debug.Log((object)versiontxt);
	}

	public void SideBarIn(bool isFromSidebar)
	{
		if (isFromSidebar)
		{
			Debug.Log((object)"从侧边栏进入，发放复访奖励！");
			if (PlayerPrefs.HasKey(SideBAR_HAS_GET_KEY))
			{
				Debug.Log((object)"已领过奖励！");
				return;
			}
			PlayerPrefs.SetInt(SideBAR_HAS_GET_KEY, 1);
			PlayerPrefs.Save();
			int num = Mathf.CeilToInt(ConfigManager.Instance.GetCommonFactor(23));
			double averageGoldPerMinuteFromLast30Seconds = SingletonMono<UpgradeManager>.Instance.GetAverageGoldPerMinuteFromLast30Seconds();
			double item = Math.Max(num, averageGoldPerMinuteFromLast30Seconds * 5.0);
			UIManager.Instance.ShowRewardBoxUI(new List<double> { 1.0, item }, isAddItem: true);
		}
		else
		{
			Debug.Log((object)"从其他渠道进入，不发放复访奖励");
		}
	}
}

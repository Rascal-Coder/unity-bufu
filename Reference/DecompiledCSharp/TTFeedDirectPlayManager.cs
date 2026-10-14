using System;
using TTSDK;
using TTSDK.UNBridgeLib.LitJson;
using UnityEngine;

public class TTFeedDirectPlayManager : MonoBehaviour
{
	public const string DefaultContentId = "CONTENT13572718594";

	private const int OfflineIncomeScene = 1;

	private const int ImportantEventScene = 3;

	[Header("开放平台-推荐流卡片 ContentID（必填）")]
	[SerializeField]
	private string _customContentId;

	[Header("复访提醒时间（秒，建议2~8小时）")]
	public int TriggerTime = 7200;

	[Header("重要事件复访提醒时间（秒）")]
	public int ImportantEventTriggerTime = 7200;

	private const string userCancelFeedKey = "USER_CANCEL_FEED";

	private bool status;

	public static TTFeedDirectPlayManager Instance { get; private set; }

	public string appId { get; private set; } = "tt0e6178dc916a67d802";

	private string CapeHeroContentId => appId switch
	{
		"tte61690ab780b197307" => "CONTENT13863171330", 
		"tte1cc97717ec473e307" => "CONTENT13943810050", 
		"tt08737ece977c631607" => "CONTENT13831840258", 
		"tt0e6178dc916a67d802" => "CONTENT14046415618", 
		"tt0fef795290e4766802" => "CONTENT14389271298", 
		_ => "CONTENT13805414914", 
	};

	private string KungfuMasterThirdDaySignInContentId => appId switch
	{
		"tte61690ab780b197307" => "CONTENT13863171586", 
		"tte1cc97717ec473e307" => "CONTENT13885903106", 
		"tt08737ece977c631607" => "CONTENT13885903618", 
		"tt0e6178dc916a67d802" => "CONTENT14046415874", 
		"tt0fef795290e4766802" => "CONTENT14388970498", 
		_ => "CONTENT13805261826", 
	};

	private string AcquisitionContentId
	{
		get
		{
			_ = appId == "tt0fef795290e4766802";
			return "CONTENT14246872834";
		}
	}

	public string ContentId
	{
		get
		{
			if (!string.IsNullOrEmpty(_customContentId))
			{
				return _customContentId;
			}
			return appId switch
			{
				"tte61690ab780b197307" => "CONTENT13572718594", 
				"tte1cc97717ec473e307" => "CONTENT13814329090", 
				"tt08737ece977c631607" => "CONTENT13814329858", 
				"tt0e6178dc916a67d802" => "CONTENT14119533314", 
				"tt0fef795290e4766802" => "CONTENT14298629378", 
				_ => "CONTENT13805414658", 
			};
		}
		set
		{
			_customContentId = value;
		}
	}

	public bool IsFromDirectPlay { get; private set; }

	private void Awake()
	{
		if ((Object)(object)Instance != (Object)null && (Object)(object)Instance != (Object)(object)this)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		Instance = this;
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
	}

	public void OnGameStart(string appId)
	{
		if (!string.IsNullOrEmpty(appId))
		{
			this.appId = appId;
		}
		RequestFeedSubscribe();
		ReportOfflineIncomeRevisitData();
	}

	public void CheckDirectPlayEntry(LaunchOption launchOpts)
	{
		try
		{
			if (launchOpts == null)
			{
				Debug.LogWarning((object)"[抖音直玩] GetLaunchOptionsSync 返回空");
				return;
			}
			string text = launchOpts.Scene ?? "";
			if (IsFromDirectPlay = text.EndsWith("3041") || (launchOpts.Query != null && launchOpts.Query.ContainsKey("direct_play")))
			{
				Debug.Log((object)"[抖音直玩] ✅ 用户从推荐流直玩卡片进入");
				OnDirectPlayEnterCallback();
			}
			else
			{
				Debug.Log((object)("[抖音直玩] 正常启动，Scene=" + text));
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("[抖音直玩] 判断直玩入口异常：" + ex.Message));
		}
	}

	private void OnDirectPlayEnterCallback()
	{
		Debug.Log((object)"[抖音直玩] 直玩进入后的业务逻辑");
	}

	private void RequestFeedSubscribe()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		if ((int)Application.platform == 17 || (int)Application.platform == 50)
		{
			CheckOfflineIncomeSubscribeStatus(delegate(bool isSubscribed)
			{
				status = isSubscribed;
				Debug.Log((object)$"[抖音复访] 离线收益订阅状态：{isSubscribed}");
			});
			CheckImportantEventSubscribeStatus(delegate(bool isSubscribed)
			{
				Debug.Log((object)$"[抖音复访] 重要事件订阅状态：{isSubscribed}");
			});
		}
	}

	public void CheckOfflineIncomeSubscribeStatus(Action<bool> onSuccess, Action onFail = null)
	{
		CheckFeedSubscribeStatusByScene(1, onSuccess, onFail);
	}

	public void CheckImportantEventSubscribeStatus(Action<bool> onSuccess, Action onFail = null)
	{
		CheckFeedSubscribeStatusByScene(3, onSuccess, onFail);
	}

	private void CheckFeedSubscribeStatusByScene(int scene, Action<bool> onSuccess, Action onFail = null)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Invalid comparison between Unknown and I4
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		if ((int)Application.platform == 17 || (int)Application.platform == 50)
		{
			try
			{
				TT.CheckFeedSubscribeStatus(new JsonData
				{
					["type"] = JsonData.op_Implicit("play"),
					["scene"] = JsonData.op_Implicit(scene)
				}, (Action<JsonData>)delegate(JsonData res)
				{
					Debug.Log((object)$"[抖音复访] scene={scene} 订阅状态：{res}");
					JsonData val = JsonMapper.ToObject(((object)res).ToString());
					bool obj = false;
					if (val.ContainsKey("status"))
					{
						obj = ((object)val["status"]).ToString().ToLower() == "true";
					}
					onSuccess?.Invoke(obj);
				}, (Action<int, string>)delegate(int errNo, string errMsg)
				{
					Debug.LogError((object)$"[抖音复访] scene={scene} 订阅状态异常：{errNo} {errMsg}");
					onFail?.Invoke();
				}, (Action)delegate
				{
					Debug.Log((object)$"[抖音复访] scene={scene} 订阅状态查询完成");
				});
				return;
			}
			catch (Exception ex)
			{
				Debug.LogError((object)$"[抖音复访] scene={scene} 订阅状态查询异常：{ex.Message}");
				onFail?.Invoke();
				return;
			}
		}
		onSuccess?.Invoke(obj: false);
	}

	private bool IsRequestFeedSubscribeSuccess(object res)
	{
		try
		{
			JsonData val = JsonMapper.ToObject(res.ToString());
			if (val.ContainsKey("success") && ((object)val["success"]).ToString().ToLower() == "true")
			{
				return true;
			}
			if (val.ContainsKey("errMsg") && ((object)val["errMsg"]).ToString().ToLower().Contains("requestfeedsubscribe:ok"))
			{
				return true;
			}
			return false;
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("[抖音复访] 订阅结果解析异常：" + ex.Message));
			return false;
		}
	}

	private void HandleRequestFeedSubscribeResult(string logPrefix, object res, Action onSuccess, Action onFail)
	{
		Debug.Log((object)$"[{logPrefix}RequestFeedSubscribe] 订阅结果：{res}");
		if (IsRequestFeedSubscribeSuccess(res))
		{
			onSuccess?.Invoke();
		}
		else
		{
			onFail?.Invoke();
		}
	}

	private JsonData CreateImportantEventContentIds()
	{
		JsonData obj = JsonData.NewJsonArray();
		obj.Add((object)CapeHeroContentId);
		obj.Add((object)KungfuMasterThirdDaySignInContentId);
		return obj;
	}

	public void ZhuDongRequestFeedSubscribe(Action onSuccess, Action onFail)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected O, but got Unknown
		JsonData val = new JsonData
		{
			["type"] = JsonData.op_Implicit("play"),
			["scene"] = JsonData.op_Implicit(1)
		};
		JsonData val2 = JsonData.NewJsonArray();
		val2.Add((object)ContentId);
		val["contentIDs"] = val2;
		Debug.Log((object)"主动请求订阅权限，场景1");
		TT.RequestFeedSubscribe(val, (Action<JsonData>)delegate(JsonData res)
		{
			HandleRequestFeedSubscribeResult("主动", res, onSuccess, onFail);
		}, (Action<int, string>)delegate(int errNo, string errMsg)
		{
			Debug.LogError((object)$"[主动RequestFeedSubscribe] 订阅异常：{errNo} {errMsg}");
			onFail?.Invoke();
		}, (Action)delegate
		{
			Debug.Log((object)"[主动RequestFeedSubscribe] 订阅完成");
		});
	}

	public void ZhuDongRequestCapeHeroFeedSubscribe(Action onSuccess, Action onFail)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		JsonData val = new JsonData
		{
			["type"] = JsonData.op_Implicit("play"),
			["scene"] = JsonData.op_Implicit(3),
			["contentIDs"] = CreateImportantEventContentIds()
		};
		Debug.Log((object)"主动请求披风侠订阅权限，场景3");
		TT.RequestFeedSubscribe(val, (Action<JsonData>)delegate(JsonData res)
		{
			HandleRequestFeedSubscribeResult("披风侠", res, delegate
			{
				if (AppConst.DebugMode)
				{
					ImportantEventTriggerTime = 30;
				}
				StoreFeedData(3, CapeHeroContentId, ImportantEventTriggerTime, "cape_hero");
				onSuccess?.Invoke();
			}, onFail);
		}, (Action<int, string>)delegate(int errNo, string errMsg)
		{
			Debug.LogError((object)$"[披风侠RequestFeedSubscribe] 订阅异常：{errNo} {errMsg}");
			onFail?.Invoke();
		}, (Action)delegate
		{
			Debug.Log((object)"[披风侠RequestFeedSubscribe] 订阅完成");
		});
	}

	public void ZhuDongRequestKungfuMasterThirdDaySignInFeedSubscribe(Action onSuccess, Action onFail)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		JsonData val = new JsonData
		{
			["type"] = JsonData.op_Implicit("play"),
			["scene"] = JsonData.op_Implicit(3),
			["contentIDs"] = CreateImportantEventContentIds()
		};
		Debug.Log((object)"主动请求第三天签到功夫大师订阅权限，场景3");
		TT.RequestFeedSubscribe(val, (Action<JsonData>)delegate(JsonData res)
		{
			HandleRequestFeedSubscribeResult("第三天签到功夫大师", res, delegate
			{
				if (AppConst.DebugMode)
				{
					ImportantEventTriggerTime = 30;
				}
				StoreFeedData(3, KungfuMasterThirdDaySignInContentId, ImportantEventTriggerTime, "kungfu_master_third_day_sign_in");
				onSuccess?.Invoke();
			}, onFail);
		}, (Action<int, string>)delegate(int errNo, string errMsg)
		{
			Debug.LogError((object)$"[第三天签到功夫大师RequestFeedSubscribe] 订阅异常：{errNo} {errMsg}");
			onFail?.Invoke();
		}, (Action)delegate
		{
			Debug.Log((object)"[第三天签到功夫大师RequestFeedSubscribe] 订阅完成");
		});
	}

	public void ReportRevisitData()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		if ((int)Application.platform != 17 && (int)Application.platform != 50)
		{
			return;
		}
		if (string.IsNullOrEmpty(ContentId))
		{
			Debug.LogError((object)"[抖音复访] ContentId 未配置！");
			return;
		}
		try
		{
			if (AppConst.DebugMode)
			{
				TriggerTime = 30;
			}
			StoreFeedData(1, ContentId, TriggerTime, "offline_income");
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("[抖音复访] 上报异常：" + ex.Message));
		}
	}

	public void ReportImportantEventRevisitData()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		if ((int)Application.platform == 17 || (int)Application.platform == 50)
		{
			if (AppConst.DebugMode)
			{
				ImportantEventTriggerTime = 30;
			}
			StoreFeedData(3, CapeHeroContentId, ImportantEventTriggerTime, "cape_hero");
			StoreFeedData(3, KungfuMasterThirdDaySignInContentId, ImportantEventTriggerTime, "kungfu_master_third_day_sign_in");
		}
	}

	public void ReportOfflineIncomeRevisitData()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		if ((int)Application.platform == 17 || (int)Application.platform == 50)
		{
			if (AppConst.DebugMode)
			{
				TriggerTime = 30;
			}
			StoreFeedData(1, ContentId, TriggerTime, "offline_income");
		}
	}

	private void StoreFeedData(int scene, string contentId, int triggerTime, string extra)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Expected O, but got Unknown
		if (string.IsNullOrEmpty(contentId))
		{
			Debug.LogError((object)$"[抖音复访] scene={scene} ContentId 未配置！");
			return;
		}
		long num = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + triggerTime * 1000;
		TT.StoreFeedData(new StoreFeedDataParam
		{
			Scene = scene,
			ContentID = contentId,
			LeftValue = "timeStampMs",
			Operator = ">=",
			RightValue = num.ToString(),
			Status = 1,
			Extra = extra,
			Success = (StoreFeedDataSuccessCallback)delegate
			{
				Debug.Log((object)$"StoreFeedData success. scene={scene}, contentId={contentId}, extra={extra}");
			},
			Fail = delegate(ErrorInfo err)
			{
				Debug.Log((object)$"StoreFeedData failed. scene={scene}, contentId={contentId}, extra={extra}, err={err}");
			}
		});
		Debug.Log((object)$"[抖音复访] 上报托管复访数据：scene={scene}, contentId={contentId}, extra={extra}");
	}

	public void SetContentId(string contentId)
	{
		ContentId = contentId;
	}

	public void GetFeedData()
	{
		GetFeedData(1, ContentId);
	}

	public void GetCapeHeroFeedData()
	{
		GetFeedData(3, CapeHeroContentId);
	}

	public void GetKungfuMasterThirdDaySignInFeedData()
	{
		GetFeedData(3, KungfuMasterThirdDaySignInContentId);
	}

	private void GetFeedData(int scene, string contentId)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		TT.GetFeedData(new GetFeedDataParam
		{
			Scene = scene,
			ContentID = contentId,
			Success = (GetFeedDataSuccessCallback)delegate(GetFeedDataSuccessResult res)
			{
				Debug.Log((object)$"GetFeedData success. scene={scene}, contentId={contentId}, status={res.Status}, extra={res.Extra}");
			},
			Fail = delegate(ErrorInfo err)
			{
				Debug.Log((object)$"GetFeedData failed. scene={scene}, contentId={contentId}, err={err}");
			}
		});
	}
}

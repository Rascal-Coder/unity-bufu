using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using TTSDK;
using TTSDK.UNBridgeLib.LitJson;
using UnityEngine;
using UnityEngine.Networking;

namespace Module.Http;

public class HttpManager : SingletonMono<HttpManager>
{
	[Serializable]
	private class HeartbeatResponse
	{
		public string openId;

		public string serverUtc;
	}

	[Serializable]
	private class TrackRequest
	{
		public string openId;

		public string eventType;

		public string eventValue;
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static Action<bool, string> _003C_003E9__20_0;

		public static OnShareSuccessCallback _003C_003E9__66_0;

		public static OnShareFailedCallback _003C_003E9__66_1;

		internal void _003CUpdate_003Eb__20_0(bool success, string response)
		{
			if (success)
			{
				try
				{
					HeartbeatResponse heartbeatResponse = JsonUtility.FromJson<HeartbeatResponse>(response);
					if (heartbeatResponse != null && !string.IsNullOrEmpty(heartbeatResponse.serverUtc) && DateTime.TryParse(heartbeatResponse.serverUtc, out var result))
					{
						result = DateTime.SpecifyKind(result, DateTimeKind.Utc);
						if ((Object)(object)GameDataManager.Instance != (Object)null)
						{
							GameDataManager.Instance.UpdateServerTime(result);
						}
						Debug.Log((object)$"[HttpManager] 心跳包更新服务器时间成功: {result}");
					}
					return;
				}
				catch (Exception ex)
				{
					Debug.LogError((object)("[HttpManager] 解析心跳包响应失败: " + ex.Message));
					return;
				}
			}
			Debug.LogWarning((object)("[HttpManager] 发送心跳包失败: " + response));
		}

		internal void _003CShareAppMessage_003Eb__66_0(Dictionary<string, object> res)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			TT.ShowToast(new TTShowToastParam
			{
				title = "分享成功",
				duration = 2
			});
		}

		internal void _003CShareAppMessage_003Eb__66_1(string e)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			TT.ShowToast(new TTShowToastParam
			{
				title = "分享失败",
				duration = 2
			});
		}
	}

	[Header("服务器基础地址")]
	private string baseUrl = "https://bsls-game.jvplay.cn/";

	public const string DouyinLoginEndpoint = "api/auth/douyin/code2session";

	public const string WeChatLoginEndpoint = "api/auth/wechat/code2session";

	public const string BilibiliLoginEndpoint = "api/auth/bilibili/code2session";

	private string _currentOpenId;

	private string _currentSessionKey;

	private float _heartbeatTimer;

	private const float HeartbeatInterval = 120f;

	public string BaseUrl
	{
		get
		{
			return baseUrl;
		}
		set
		{
			baseUrl = value;
		}
	}

	public override bool DontDestory => true;

	public bool IsInitialized => !string.IsNullOrEmpty(_currentOpenId);

	public static bool CanSendServerRequests
	{
		get
		{
			if (!Application.isEditor)
			{
				return IsMiniGamePlatform();
			}
			return false;
		}
	}

	protected override void OnAwake()
	{
		baseUrl = (AppConst.DebugMode ? "https://bsls-game.jvplay.cn/test/" : "https://bsls-game.jvplay.cn/");
	}

	public void Initialize(string openId, string sessionKey)
	{
		_currentOpenId = openId;
		_currentSessionKey = sessionKey;
		_heartbeatTimer = 118f;
		Debug.Log((object)("[HttpManager] 初始化完成，已设置 OpenId: " + _currentOpenId + "，开启心跳机制。"));
	}

	private void Update()
	{
		if (!CanSendServerRequests || string.IsNullOrEmpty(_currentOpenId))
		{
			return;
		}
		_heartbeatTimer += Time.deltaTime;
		if (!(_heartbeatTimer >= 120f))
		{
			return;
		}
		_heartbeatTimer = 0f;
		SendHeartbeat(_currentOpenId, delegate(bool success, string response)
		{
			if (success)
			{
				try
				{
					HeartbeatResponse heartbeatResponse = JsonUtility.FromJson<HeartbeatResponse>(response);
					if (heartbeatResponse != null && !string.IsNullOrEmpty(heartbeatResponse.serverUtc) && DateTime.TryParse(heartbeatResponse.serverUtc, out var result))
					{
						result = DateTime.SpecifyKind(result, DateTimeKind.Utc);
						if ((Object)(object)GameDataManager.Instance != (Object)null)
						{
							GameDataManager.Instance.UpdateServerTime(result);
						}
						Debug.Log((object)$"[HttpManager] 心跳包更新服务器时间成功: {result}");
					}
					return;
				}
				catch (Exception ex)
				{
					Debug.LogError((object)("[HttpManager] 解析心跳包响应失败: " + ex.Message));
					return;
				}
			}
			Debug.LogWarning((object)("[HttpManager] 发送心跳包失败: " + response));
		});
	}

	protected override void BeforOnDestroy()
	{
	}

	public void Get(string endpoint, Action<bool, string> callback)
	{
		((MonoBehaviour)this).StartCoroutine(SendRequest(endpoint, "GET", null, callback));
	}

	public void Post(string endpoint, string jsonData, Action<bool, string> callback)
	{
		((MonoBehaviour)this).StartCoroutine(SendRequest(endpoint, "POST", jsonData, callback));
	}

	public void Post(string endpoint, string jsonData, Action<string> onSuccess, Action<int, string> onFail)
	{
		((MonoBehaviour)this).StartCoroutine(SendRequestEx(endpoint, "POST", jsonData, onSuccess, onFail));
	}

	private IEnumerator SendRequest(string endpoint, string method, string jsonData, Action<bool, string> callback)
	{
		if (!CanSendServerRequests)
		{
			callback?.Invoke(arg1: false, "Server requests are disabled on this platform");
			yield break;
		}
		string url = baseUrl + endpoint;
		UnityWebRequest request = new UnityWebRequest(url, method);
		try
		{
			if (!string.IsNullOrEmpty(jsonData))
			{
				byte[] bytes = Encoding.UTF8.GetBytes(jsonData);
				request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bytes);
			}
			request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
			request.SetRequestHeader("Content-Type", "application/json");
			ApplyCommonHeaders(request);
			yield return request.SendWebRequest();
			if ((int)request.result == 1)
			{
				callback?.Invoke(arg1: true, request.downloadHandler.text);
				yield break;
			}
			Debug.LogError((object)("[HttpManager] " + method + " 请求失败: " + url + "\n错误信息: " + request.error));
			callback?.Invoke(arg1: false, request.error);
		}
		finally
		{
			((IDisposable)request)?.Dispose();
		}
	}

	private IEnumerator SendRequestEx(string endpoint, string method, string jsonData, Action<string> onSuccess, Action<int, string> onFail)
	{
		if (!CanSendServerRequests)
		{
			onFail?.Invoke(0, "Server requests are disabled on this platform");
			yield break;
		}
		string url = baseUrl + endpoint;
		UnityWebRequest request = new UnityWebRequest(url, method);
		try
		{
			if (!string.IsNullOrEmpty(jsonData))
			{
				byte[] bytes = Encoding.UTF8.GetBytes(jsonData);
				request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bytes);
			}
			request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
			request.SetRequestHeader("Content-Type", "application/json");
			ApplyCommonHeaders(request);
			yield return request.SendWebRequest();
			if ((int)request.result == 1)
			{
				onSuccess?.Invoke(request.downloadHandler.text);
				yield break;
			}
			Debug.LogError((object)$"[HttpManager] {method} 请求失败: {url}\n状态码: {request.responseCode}\n错误信息: {request.error}");
			onFail?.Invoke((int)request.responseCode, request.error);
		}
		finally
		{
			((IDisposable)request)?.Dispose();
		}
	}

	private void ApplyCommonHeaders(UnityWebRequest request)
	{
		if (!string.IsNullOrEmpty(_currentSessionKey))
		{
			request.SetRequestHeader("X-Session-Key", _currentSessionKey);
		}
		if (!string.IsNullOrEmpty(_currentOpenId))
		{
			request.SetRequestHeader("X-Open-Id", _currentOpenId);
		}
		request.SetRequestHeader("X-Platform", GetPlatformHeader());
		request.SetRequestHeader("X-Client-Version", Application.version);
	}

	private static string GetPlatformHeader()
	{
		return "douyin";
	}

	public void GetDouyinOpenId(string code, string anonymousCode, Action<string> onSuccess, Action<int, string> onFail)
	{
		GetDouyinOpenId(code, anonymousCode, null, onSuccess, onFail);
	}

	public void GetDouyinOpenId(string code, string anonymousCode, string appId, Action<string> onSuccess, Action<int, string> onFail)
	{
		string jsonData = ((!string.IsNullOrEmpty(appId)) ? ("{\"code\":\"" + code + "\", \"anonymous_code\":\"" + anonymousCode + "\", \"appid\":\"" + appId + "\"}") : ("{\"code\":\"" + code + "\", \"anonymous_code\":\"" + anonymousCode + "\"}"));
		Post("api/auth/douyin/code2session", jsonData, onSuccess, onFail);
	}

	public void GetWeChatOpenId(string code, Action<string> onSuccess, Action<int, string> onFail)
	{
		GetWeChatOpenId(code, null, onSuccess, onFail);
	}

	public void GetWeChatOpenId(string code, string appId, Action<string> onSuccess, Action<int, string> onFail)
	{
		string jsonData = ((!string.IsNullOrEmpty(appId)) ? ("{\"code\":\"" + code + "\", \"appid\":\"" + appId + "\"}") : ("{\"code\":\"" + code + "\"}"));
		Post("api/auth/wechat/code2session", jsonData, onSuccess, onFail);
	}

	public void GetBilibiliOpenId(string code, Action<string> onSuccess, Action<int, string> onFail)
	{
		GetBilibiliOpenId(code, null, onSuccess, onFail);
	}

	public void GetBilibiliOpenId(string code, string appId, Action<string> onSuccess, Action<int, string> onFail)
	{
		string jsonData = ((!string.IsNullOrEmpty(appId)) ? ("{\"code\":\"" + code + "\", \"appid\":\"" + appId + "\"}") : ("{\"code\":\"" + code + "\"}"));
		Post("api/auth/bilibili/code2session", jsonData, onSuccess, onFail);
	}

	public void GetGameVersion(Action<bool, string> callback)
	{
		Get("api/game/version", callback);
	}

	public void GetPlayerInfo(string openId, Action<bool, string> callback)
	{
		GetPlayerInfo(openId, null, callback);
	}

	public void GetPlayerInfo(string openId, string part, Action<bool, string> callback)
	{
		string endpoint = (string.IsNullOrEmpty(part) ? ("api/player/" + openId) : ("api/player/" + openId + "?part=" + part));
		Get(endpoint, callback);
	}

	public void GetPlayInfoBackUP(string openId, Action<bool, string> callback)
	{
		GetPlayInfoBackUP(openId, null, callback);
	}

	public void GetPlayInfoBackUP(string openId, string part, Action<bool, string> callback)
	{
		string endpoint = (string.IsNullOrEmpty(part) ? ("api/player/" + openId + "/backup/highest") : ("api/player/" + openId + "/backup/highest?part=" + part));
		Get(endpoint, callback);
	}

	public void UpdatePlayerInfo(string openId, string jsonData, Action<bool, string> callback)
	{
		Post("api/player/" + openId, jsonData, callback);
	}

	public void UpdateWorldData(string openId, int worldId, string jsonData, Action<bool, string> callback)
	{
		Post($"api/player/{openId}/world/{Mathf.Max(1, worldId)}", jsonData, callback);
	}

	public void GetWorldData(string openId, int worldId, Action<bool, string> callback)
	{
		Get($"api/player/{openId}/world/{Mathf.Max(1, worldId)}", callback);
	}

	public void GetCurrentWorldForRestore(string openId, Action<bool, string> callback)
	{
		Get("api/player/" + openId + "/restore/current", callback);
	}

	public void GetHighestWorldBackup(string openId, int worldId, Action<bool, string> callback)
	{
		Get($"api/player/{openId}/world/{Mathf.Max(1, worldId)}/backup/highest", callback);
	}

	public void UpdateGameData(string openId, string jsonData, Action<bool, string> callback)
	{
		Post("api/player/" + openId + "/gamedata", jsonData, callback);
	}

	public void UpdateFacilityData(string openId, string jsonData, Action<bool, string> callback)
	{
		Post("api/player/" + openId + "/facilitydata", jsonData, callback);
	}

	public void UpdateUpgradeData(string openId, string jsonData, Action<bool, string> callback)
	{
		Post("api/player/" + openId + "/upgradedata", jsonData, callback);
	}

	public void UpdateTalentData(string openId, string jsonData, Action<bool, string> callback)
	{
		Post("api/player/" + openId + "/talentdata", jsonData, callback);
	}

	public void UpdatePlayerPrefsData(string openId, string jsonData, Action<bool, string> callback)
	{
		Post("api/player/" + openId + "/playerprefsdata", jsonData, callback);
	}

	public void UpdateTimeInfo(string openId, string jsonData, Action<bool, string> callback)
	{
		Post("api/player/" + openId + "/timeinfo", jsonData, callback);
	}

	public void SendHeartbeat(string openId, Action<bool, string> callback)
	{
		Post("api/player/" + openId + "/heartbeat", null, callback);
	}

	public void PlayerExit(string openId, string jsonData, Action<bool, string> callback)
	{
		Post("api/player/" + openId + "/exit", jsonData, callback);
	}

	public void GetOnlinePlayerCount(Action<bool, string> callback)
	{
		Get("api/player/online/count", callback);
	}

	public void CheckPlayerOnlineStatus(string openId, Action<bool, string> callback)
	{
		Get("api/player/online/" + openId, callback);
	}

	public void SubmitScore(long score, Action<bool, string> callback = null)
	{
		if (string.IsNullOrEmpty(_currentOpenId))
		{
			Debug.LogWarning((object)"[HttpManager] 提交分数失败：_currentOpenId 为空，请先完成初始化。");
			callback?.Invoke(arg1: false, "OpenId is null");
		}
		else
		{
			int num = ((!((Object)(object)GameDataManager.Instance != (Object)null)) ? 1 : GameDataManager.Instance.GetCurrentWorldId());
			string jsonData = $"{{\"openId\":\"{_currentOpenId}\", \"score\":{score}, \"worldId\":{num}}}";
			Post("api/score/submit", jsonData, callback);
		}
	}

	public void SubmitScore(Action<bool, string> callback = null)
	{
		if (!((Object)(object)GameDataManager.Instance == (Object)null))
		{
			long score = GameDataManager.Instance.GetCurrentMainMissionLevel();
			SubmitScore(score, callback);
		}
	}

	public void GetLeaderboard(int limit, Action<bool, string> callback)
	{
		int worldId = ((!((Object)(object)GameDataManager.Instance != (Object)null)) ? 1 : GameDataManager.Instance.GetCurrentWorldId());
		GetLeaderboard(worldId, limit, callback);
	}

	public void GetLeaderboard(int worldId, int limit, Action<bool, string> callback)
	{
		worldId = Mathf.Max(1, worldId);
		limit = Mathf.Clamp(limit, 1, 100);
		Get($"api/leaderboard?worldId={worldId}&limit={limit}", callback);
	}

	public void ShareHelp(string targetOpenId, string helperOpenId, Action<bool, string> callback)
	{
		if (string.IsNullOrEmpty(helperOpenId))
		{
			helperOpenId = _currentOpenId;
		}
		string jsonData = "{\"targetOpenId\":\"" + targetOpenId + "\", \"helperOpenId\":\"" + helperOpenId + "\"}";
		Post("api/share/help", jsonData, callback);
	}

	public void GetShareStatus(string targetOpenId, string day, Action<bool, string> callback)
	{
		string endpoint = (string.IsNullOrEmpty(day) ? ("api/share/status/" + targetOpenId) : ("api/share/status/" + targetOpenId + "?day=" + day));
		Get(endpoint, callback);
	}

	public void TrackEvent(string openId, string eventType, string eventValue, Action<bool, string> callback = null)
	{
		if (!IsMiniGamePlatform())
		{
			callback?.Invoke(arg1: true, "Skip tracking in non-minigame environment");
			return;
		}
		if (string.IsNullOrEmpty(openId))
		{
			openId = _currentOpenId;
		}
		if (string.IsNullOrEmpty(openId))
		{
			Debug.LogWarning((object)"[HttpManager] 追踪事件失败：openId 为空，且未初始化。");
			callback?.Invoke(arg1: false, "OpenId is null");
			return;
		}
		string jsonData = JsonUtility.ToJson((object)new TrackRequest
		{
			openId = openId,
			eventType = eventType,
			eventValue = eventValue
		});
		Post("api/track", jsonData, callback);
	}

	public void TrackAdPoint(string openId, string eventValue, Action<bool, string> callback = null)
	{
		TrackEvent(openId, "AdPoint", eventValue, callback);
	}

	public void TrackSwitchUI(string openId, string eventValue, Action<bool, string> callback = null)
	{
		TrackEvent(openId, "SwitchUI", eventValue, callback);
	}

	public void GetImRankData(string relationType, Action<RankData> success, Action<string> fail)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Expected O, but got Unknown
		TT.GetImRankData(new JsonData
		{
			["relationType"] = JsonData.op_Implicit(relationType),
			["dataType"] = JsonData.op_Implicit(0),
			["rankType"] = JsonData.op_Implicit("week"),
			["pageNum"] = JsonData.op_Implicit(1),
			["pageSize"] = JsonData.op_Implicit(40),
			["zoneId"] = JsonData.op_Implicit(WorldRankUtility.GetZoneId(GameDataManager.Instance.GetCurrentWorldId()))
		}, (OnGetRankDataSuccessCallback)delegate(ref RankData data)
		{
			Debug.Log((object)JsonUtility.ToJson((object)data));
			Debug.Log((object)$"data: {data}");
			Debug.Log((object)$"data: {data.Items.Count}");
			Debug.Log((object)$"data: {data.SelfItem}");
			if (data.SelfItem != null)
			{
				Debug.Log((object)$"data: {data.SelfItem.Rank}");
			}
			for (int i = 0; i < data.Items.Count; i++)
			{
				Debug.Log((object)("item-->nickName :" + data.Items[i].Nickname + ", openId:" + data.Items[i].OpenId));
			}
			Debug.Log((object)"GetImRankData success");
			success?.Invoke(data);
		}, (OnGetRankDataFailCallback)delegate(string msg)
		{
			Debug.Log((object)("GetImRankData fail error message:" + msg));
			fail?.Invoke(msg);
		});
	}

	public void ShareAppMessage(int activityID, Action<string> success, Action<string> fail)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Expected O, but got Unknown
		//IL_00b2: Expected O, but got Unknown
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		JsonData val = new JsonData
		{
			["title"] = JsonData.op_Implicit("你也试一下"),
			["desc"] = JsonData.op_Implicit("从0开始,赚够1个亿吧"),
			["query"] = JsonData.op_Implicit($"uid={GameDataManager.Instance.GetOpenId()}&activity={activityID}"),
			["templateId"] = JsonData.op_Implicit("")
		};
		object obj = _003C_003Ec._003C_003E9__66_0;
		if (obj == null)
		{
			OnShareSuccessCallback val2 = delegate
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				//IL_0005: Unknown result type (might be due to invalid IL or missing references)
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				//IL_0021: Expected O, but got Unknown
				TT.ShowToast(new TTShowToastParam
				{
					title = "分享成功",
					duration = 2
				});
			};
			_003C_003Ec._003C_003E9__66_0 = val2;
			obj = (object)val2;
		}
		object obj2 = _003C_003Ec._003C_003E9__66_1;
		if (obj2 == null)
		{
			OnShareFailedCallback val3 = delegate
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				//IL_0005: Unknown result type (might be due to invalid IL or missing references)
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				//IL_0021: Expected O, but got Unknown
				TT.ShowToast(new TTShowToastParam
				{
					title = "分享失败",
					duration = 2
				});
			};
			_003C_003Ec._003C_003E9__66_1 = val3;
			obj2 = (object)val3;
		}
		TT.ShareAppMessage(val, (OnShareSuccessCallback)obj, (OnShareFailedCallback)obj2, (OnShareCancelledCallback)null);
	}

	private static bool IsMiniGamePlatform()
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

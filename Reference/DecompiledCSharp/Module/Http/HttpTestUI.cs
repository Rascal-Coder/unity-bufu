using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Module.Http;

public class HttpTestUI : MonoBehaviour
{
	[Header("按钮预制体")]
	[SerializeField]
	private GameObject buttonPrefab;

	[Header("关闭按钮")]
	[SerializeField]
	private Button closeBtn;

	[Header("容器")]
	[SerializeField]
	private Transform container;

	[Header("滚动视图（可选）")]
	[SerializeField]
	private ScrollRect scrollRect;

	[Header("测试数据")]
	[SerializeField]
	private string testOpenId = "test_openid_12345";

	[SerializeField]
	private string testCode = "test_code_12345";

	[SerializeField]
	private string testAnonymousCode = "test_anonymous_code_12345";

	private void Start()
	{
		InitializeTestButtons();
	}

	private void InitializeTestButtons()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		if ((Object)(object)buttonPrefab == (Object)null || (Object)(object)container == (Object)null)
		{
			Debug.LogError((object)"[HttpTestUI] 按钮预制体或容器未设置！");
			return;
		}
		ClearContainer();
		CreateTestButtons();
		((UnityEvent)closeBtn.onClick).AddListener((UnityAction)delegate
		{
			((Component)this).gameObject.SetActive(false);
		});
	}

	private void ClearContainer()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		foreach (Transform item in container)
		{
			Object.Destroy((Object)(object)((Component)item).gameObject);
		}
	}

	private void CreateTestButtons()
	{
		AddTestButton("获取游戏版本", TestGetGameVersion);
		AddTestButton("获取玩家信息", TestGetPlayerInfo);
		AddTestButton("更新玩家信息", TestUpdatePlayerInfo);
		AddTestButton("更新游戏数据", TestUpdateGameData);
		AddTestButton("更新设施数据", TestUpdateFacilityData);
		AddTestButton("更新升级数据", TestUpdateUpgradeData);
		AddTestButton("更新设置数据", TestUpdatePlayerPrefsData);
		AddTestButton("更新时间信息", TestUpdateTimeInfo);
		AddTestButton("发送心跳包", TestSendHeartbeat);
		AddTestButton("玩家退出", TestPlayerExit);
		AddTestButton("获取在线人数", TestGetOnlinePlayerCount);
		AddTestButton("检查玩家在线状态", TestCheckPlayerOnlineStatus);
		AddTestButton("提交分数", TestSubmitScore);
		AddTestButton("获取排行榜", TestGetLeaderboard);
		AddTestButton("分享助力", TestShareHelp);
		AddTestButton("获取分享状态", TestGetShareStatus);
	}

	private void AddTestButton(string displayName, Action onClick)
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Expected O, but got Unknown
		buttonPrefab.SetActive(true);
		GameObject val = Object.Instantiate<GameObject>(buttonPrefab, container);
		HttpTestButton component = val.GetComponent<HttpTestButton>();
		if ((Object)(object)component != (Object)null)
		{
			component.Initialize(displayName, delegate
			{
				Debug.Log((object)("[HttpTestUI] 点击测试按钮: " + displayName));
				onClick?.Invoke();
			});
		}
		else
		{
			TextMeshProUGUI componentInChildren = val.GetComponentInChildren<TextMeshProUGUI>();
			if ((Object)(object)componentInChildren != (Object)null)
			{
				((TMP_Text)componentInChildren).text = displayName;
			}
			Button component2 = val.GetComponent<Button>();
			if ((Object)(object)component2 != (Object)null)
			{
				((UnityEvent)component2.onClick).AddListener((UnityAction)delegate
				{
					Debug.Log((object)("[HttpTestUI] 点击测试按钮: " + displayName));
					onClick?.Invoke();
				});
			}
		}
		buttonPrefab.SetActive(false);
	}

	private void TestGetGameVersion()
	{
		SingletonMono<HttpManager>.Instance.GetGameVersion(delegate(bool success, string response)
		{
			LogResult("获取游戏版本", success, response);
		});
	}

	private void TestGetPlayerInfo()
	{
		SingletonMono<HttpManager>.Instance.GetPlayerInfo(testOpenId, delegate(bool success, string response)
		{
			LogResult("获取玩家信息", success, response);
		});
	}

	private void TestUpdatePlayerInfo()
	{
		GameDataManager.Instance.SaveAll2Server(delegate(bool success, string response)
		{
			LogResult("更新玩家信息", success, response);
		});
	}

	private void TestUpdateGameData()
	{
		GameDataManager.Instance.SaveGameData2Server(delegate(bool success, string response)
		{
			LogResult("更新游戏数据", success, response);
		});
	}

	private void TestUpdateFacilityData()
	{
		GameDataManager.Instance.SaveFacilityData2Server(delegate(bool success, string response)
		{
			LogResult("更新设施数据", success, response);
		});
	}

	private void TestUpdateUpgradeData()
	{
		GameDataManager.Instance.SaveUpgradeData2Server(delegate(bool success, string response)
		{
			LogResult("更新升级数据", success, response);
		});
	}

	private void TestUpdatePlayerPrefsData()
	{
		GameDataManager.Instance.SavePlayerPrefsData2Server(delegate(bool success, string response)
		{
			LogResult("更新升级数据", success, response);
		});
	}

	private void TestUpdateTimeInfo()
	{
		string jsonData = $"{{\"lastLoginTime\":\"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\", \"totalOnlineTime\":3600}}";
		SingletonMono<HttpManager>.Instance.UpdateTimeInfo(testOpenId, jsonData, delegate(bool success, string response)
		{
			LogResult("更新时间信息", success, response);
		});
	}

	private void TestSendHeartbeat()
	{
		SingletonMono<HttpManager>.Instance.SendHeartbeat(testOpenId, delegate(bool success, string response)
		{
			LogResult("发送心跳包", success, response);
		});
	}

	private void TestPlayerExit()
	{
		string jsonData = "{\"exitReason\":\"user_quit\", \"finalScore\":10000}";
		SingletonMono<HttpManager>.Instance.PlayerExit(testOpenId, jsonData, delegate(bool success, string response)
		{
			LogResult("玩家退出", success, response);
		});
	}

	private void TestGetOnlinePlayerCount()
	{
		SingletonMono<HttpManager>.Instance.GetOnlinePlayerCount(delegate(bool success, string response)
		{
			LogResult("获取在线人数", success, response);
		});
	}

	private void TestCheckPlayerOnlineStatus()
	{
		SingletonMono<HttpManager>.Instance.CheckPlayerOnlineStatus(testOpenId, delegate(bool success, string response)
		{
			LogResult("检查玩家在线状态", success, response);
		});
	}

	private void TestSubmitScore()
	{
		long score = Random.Range(1000, 100000);
		SingletonMono<HttpManager>.Instance.Initialize(testOpenId, testCode);
		SingletonMono<HttpManager>.Instance.SubmitScore(score, delegate(bool success, string response)
		{
			LogResult($"提交分数 ({score})", success, response);
		});
	}

	private void TestGetLeaderboard()
	{
		SingletonMono<HttpManager>.Instance.GetLeaderboard(10, delegate(bool success, string response)
		{
			LogResult("获取排行榜 (Top 10)", success, response);
		});
	}

	private void TestShareHelp()
	{
		string helperOpenId = "helper_openid_67890";
		SingletonMono<HttpManager>.Instance.ShareHelp(testOpenId, helperOpenId, delegate(bool success, string response)
		{
			LogResult("分享助力", success, response);
		});
	}

	private void TestGetShareStatus()
	{
		SingletonMono<HttpManager>.Instance.GetShareStatus(testOpenId, DateTime.Now.ToString("yyyy-MM-dd"), delegate(bool success, string response)
		{
			LogResult("获取分享状态", success, response);
		});
	}

	private void LogResult(string operation, bool success, string response)
	{
		if (success)
		{
			Debug.Log((object)("[HttpTestUI] ✓ " + operation + " 成功\n响应: " + response));
		}
		else
		{
			Debug.LogError((object)("[HttpTestUI] ✗ " + operation + " 失败\n响应: " + response));
		}
	}

	public void RefreshUI()
	{
		InitializeTestButtons();
	}
}

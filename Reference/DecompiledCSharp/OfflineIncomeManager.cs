using System;
using System.Collections;
using UnityEngine;

public class OfflineIncomeManager : MonoBehaviour
{
	[Serializable]
	public class OfflineRewardData
	{
		public double coin;

		public double offlineTime;

		public bool hasReward;

		public OfflineRewardData()
		{
			coin = 0.0;
			offlineTime = 0.0;
			hasReward = false;
		}
	}

	private const float DirectPlayPopupDelaySeconds = 3f;

	private static OfflineIncomeManager _instance;

	private bool _waitForDirectPlayEntry;

	private bool _directPlayEntered;

	private bool _startupComplete;

	private bool _startupPopupConfigured;

	private bool _startupPopupScheduled;

	private bool _delayAfterFeedEnter;

	private bool _isAcquisitionEntry;

	private OfflineIncomeUI offlineUI;

	private bool _isOfflineUIShowing;

	private static float MIN_OFFLINE_TIME = 120f;

	private static float MAX_OFFLINE_TIME = 21600f;

	private OfflineRewardData _currentRewardData;

	private const string PersistedRewardKey = "OfflineReward_Persisted";

	private const string PersistedRewardCoinKey = "OfflineReward_Coin";

	private const string PersistedRewardTimeKey = "OfflineReward_OfflineTime";

	public static OfflineIncomeManager Instance => _instance;

	public OfflineRewardData CurrentRewardData => _currentRewardData;

	public static bool HasInstance()
	{
		return (Object)(object)_instance != (Object)null;
	}

	private void Awake()
	{
		if ((Object)(object)_instance != (Object)null && (Object)(object)_instance != (Object)(object)this)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		_instance = this;
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		if (AppConst.DebugMode)
		{
			MIN_OFFLINE_TIME = 30f;
			Debug.Log((object)$"[OfflineIncome] DebugMode 激活，修改最小离线时间为 {MIN_OFFLINE_TIME} 秒");
		}
	}

	private void OnEnable()
	{
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			GameDataManager.Instance.OnGameFocus += OnGameFocusRegained;
		}
	}

	private void OnDisable()
	{
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			GameDataManager.Instance.OnGameFocus -= OnGameFocusRegained;
		}
	}

	private void OnGameFocusRegained()
	{
		if (!_startupComplete || _isOfflineUIShowing)
		{
			return;
		}
		if (HasOfflineReward())
		{
			Debug.Log((object)"[OfflineIncome] 切回前台：已有未领取离线收益，直接弹窗");
			_isOfflineUIShowing = true;
			ShowOffLineUI();
			return;
		}
		long lastExitTime = GameDataManager.Instance.GetLastExitTime();
		bool flag = GameDataManager.Instance.IsOfflineRewardProtected();
		Debug.Log((object)$"[OfflineIncome] 切回前台：lastExitTime={lastExitTime}, IsProtected={flag}, _startupComplete={_startupComplete}, _isAcquisitionEntry={_isAcquisitionEntry}, _isOfflineUIShowing={_isOfflineUIShowing}");
		CheckOfflineIncome(lastExitTime);
		if (HasOfflineReward())
		{
			Debug.Log((object)$"[OfflineIncome] 切回前台：计算出离线收益 coin={_currentRewardData?.coin}, 准备弹窗");
			_isOfflineUIShowing = true;
			ShowOffLineUI();
		}
		else
		{
			Debug.Log((object)"[OfflineIncome] 切回前台：无离线收益");
		}
	}

	public void ConfigureStartupPopup(bool waitForFeedEnter, bool feedEntered, bool delayAfterFeedEnter, bool isAcquisitionEntry = false)
	{
		_waitForDirectPlayEntry = waitForFeedEnter;
		_directPlayEntered |= feedEntered;
		_delayAfterFeedEnter = delayAfterFeedEnter;
		_isAcquisitionEntry = isAcquisitionEntry;
		_startupPopupConfigured = true;
		if (_startupComplete)
		{
			TryScheduleStartupPopup();
		}
	}

	public void NotifyDirectPlayEntered()
	{
		_directPlayEntered = true;
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			GameDataManager.Instance.MarkPlayerActivelyEntered();
		}
		if (_startupComplete)
		{
			TryScheduleStartupPopup();
		}
	}

	public void Start()
	{
		if (TryRestorePersistedReward())
		{
			Debug.Log((object)"[OfflineIncome] 恢复了上次未领取的持久化离线收益数据");
		}
		else
		{
			CheckOfflineIncome();
		}
		_startupComplete = true;
		TryScheduleStartupPopup();
	}

	private void TryScheduleStartupPopup()
	{
		if (_startupPopupConfigured && !_startupPopupScheduled)
		{
			if (_waitForDirectPlayEntry && !_directPlayEntered)
			{
				Debug.Log((object)"[OfflineIncome] 复访场景：等待用户点击进入游戏（feedEnter），暂不弹离线收益。");
				return;
			}
			_startupPopupScheduled = true;
			_isOfflineUIShowing = true;
			((MonoBehaviour)this).StartCoroutine(OpenStartupPopupAfterDelay(_delayAfterFeedEnter));
		}
	}

	private IEnumerator OpenStartupPopupAfterDelay(bool isDirectPlay)
	{
		if (isDirectPlay)
		{
			yield return (object)new WaitForSecondsRealtime(3f);
		}
		else
		{
			yield return (object)new WaitForSeconds(0.001f);
		}
		Debug.Log((object)$"[OfflineIncome] 延迟打开离线收益UI: hasReward={_currentRewardData != null && _currentRewardData.hasReward}, 收益金币={((_currentRewardData != null) ? _currentRewardData.coin : 0.0)}");
		ShowOffLineUI();
	}

	public bool GetOffLineUIIsActive()
	{
		if ((Object)(object)offlineUI != (Object)null)
		{
			return ((Component)offlineUI).gameObject.activeSelf;
		}
		if (_isOfflineUIShowing)
		{
			return HasOfflineReward();
		}
		return false;
	}

	public void CloseOfflineUI()
	{
		if ((Object)(object)offlineUI != (Object)null)
		{
			offlineUI.OnClose();
		}
		_isOfflineUIShowing = false;
	}

	public void ShowOffLineUI()
	{
		Debug.Log((object)$"[OfflineIncome] ShowOffLineUI 被调用，offlineUI是否为空: {(Object)(object)offlineUI == (Object)null}");
		if ((Object)(object)offlineUI == (Object)null)
		{
			LoadMainScene();
		}
		else if (_currentRewardData != null && _currentRewardData.hasReward)
		{
			Debug.Log((object)"[OfflineIncome] 打开离线收益界面");
			offlineUI.OnOpen();
		}
		else
		{
			Debug.Log((object)"[OfflineIncome] 无离线收益，不打开界面");
			_isOfflineUIShowing = false;
		}
	}

	private void LoadMainScene(Action onLoaded = null)
	{
		string assetPath = "Assets/GameRes/NewUI/Popup/prefabs/OfflineReward.prefab";
		Debug.Log((object)("[OfflineIncome] 开始加载预制体: " + assetPath));
		if (SingletonMono<ResManager>.Instance.IsAssetLoaded(assetPath) == 1 && (Object)(object)offlineUI != (Object)null)
		{
			Debug.Log((object)"[OfflineIncome] 预制体已加载过，直接打开");
			if (_currentRewardData != null && _currentRewardData.hasReward)
			{
				offlineUI?.OnOpen();
			}
			else
			{
				_isOfflineUIShowing = false;
			}
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(assetPath, delegate(GameObject prefab)
		{
			if ((Object)(object)prefab != (Object)null)
			{
				Transform layerTransform = UIManager.Instance.GetLayerTransform(UILayer.Popup);
				offlineUI = Object.Instantiate<GameObject>(prefab, layerTransform).GetComponent<OfflineIncomeUI>();
				Debug.Log((object)$"[OfflineIncome] 预制体实例化成功，hasReward={_currentRewardData != null && _currentRewardData.hasReward}");
				if (_currentRewardData != null && _currentRewardData.hasReward)
				{
					offlineUI?.OnOpen();
				}
				else
				{
					_isOfflineUIShowing = false;
				}
			}
			else
			{
				Debug.LogError((object)("[OfflineIncome] 加载预制体失败: " + assetPath));
				_isOfflineUIShowing = false;
			}
		});
	}

	public void CheckOfflineIncomeAfterWorldSwitch()
	{
		WorldData currentWorldData = GameDataManager.Instance.GetCurrentWorldData();
		long lastExitTime = ((currentWorldData != null) ? Math.Max(currentWorldData.lastExitTime, (currentWorldData.gameData != null) ? currentWorldData.gameData.lastExitGameTime : 0) : 0);
		CheckOfflineIncomeAfterWorldSwitch(lastExitTime);
	}

	public void CheckOfflineIncomeAfterWorldSwitch(long lastExitTime)
	{
		CheckOfflineIncome(lastExitTime);
		if (HasOfflineReward())
		{
			_isOfflineUIShowing = true;
			ShowOffLineUI();
		}
	}

	public void CheckOfflineIncome()
	{
		CheckOfflineIncome(GameDataManager.Instance.originalLastExitTime);
	}

	private void CheckOfflineIncome(long lastExitTime)
	{
		try
		{
			if (GameDataManager.Instance.GetCurrentMainMissionLevel() < 1)
			{
				_currentRewardData = new OfflineRewardData();
				Debug.Log((object)"新手阶段无离线收益");
				return;
			}
			int currentMainMissionLevel = GameDataManager.Instance.GetCurrentMainMissionLevel();
			double averageCoin = GameDataManager.Instance.GetAverageCoin();
			long num = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
			Debug.Log((object)$"[OfflineIncome] 开始检测离线收益: 主线等级={currentMainMissionLevel}, 金币系数={averageCoin}, 当前本地时间={num}");
			Debug.Log((object)$"[OfflineIncome] 获取到的初始 lastExitTime={lastExitTime}");
			if (lastExitTime <= 0)
			{
				_currentRewardData = new OfflineRewardData();
				Debug.LogWarning((object)$"[OfflineIncome] 离线时间异常：lastExitTime={lastExitTime} <= 0，无离线收益");
				return;
			}
			long num2 = num - lastExitTime;
			Debug.Log((object)$"[OfflineIncome] 计算时间差: currentTime - lastExitTime = {num2}ms");
			if (lastExitTime > num)
			{
				_currentRewardData = new OfflineRewardData();
				Debug.LogError((object)$"[OfflineIncome] 离线时间异常：lastExitTime={lastExitTime} > currentTime={num}");
				return;
			}
			double num3 = (double)num2 / 1000.0;
			Debug.Log((object)$"[OfflineIncome] 计算出的离线时长={num3}秒, 最小要求={MIN_OFFLINE_TIME}秒");
			if (num3 < (double)MIN_OFFLINE_TIME)
			{
				_currentRewardData = new OfflineRewardData();
				Debug.Log((object)$"[OfflineIncome] 离线时间不足{MIN_OFFLINE_TIME}秒，无离线收益");
				return;
			}
			int currentWorldId = GameDataManager.Instance.GetCurrentWorldId();
			double val = MAX_OFFLINE_TIME;
			switch (currentWorldId)
			{
			case 1:
				val = (double)MAX_OFFLINE_TIME * (1.0 + (double)GameDataManager.Instance.GetGoldAge());
				break;
			case 2:
				val = (double)MAX_OFFLINE_TIME * (1.0 + (double)TalentDataManager.Instance.GetDiceTalentBuff(109));
				break;
			}
			double num4 = Math.Min(num3, val);
			Debug.Log((object)$"[OfflineIncome] 离线时间有效值={num4:F1}秒，开始计算离线收益");
			_currentRewardData = new OfflineRewardData
			{
				coin = num4 / 60.0 * averageCoin,
				offlineTime = num4,
				hasReward = true
			};
			double num5 = 1.0;
			switch (currentWorldId)
			{
			case 1:
				num5 += (double)GameDataManager.Instance.GetSoundSleep();
				break;
			case 2:
				num5 += (double)TalentDataManager.Instance.GetDiceTalentBuff(108);
				break;
			}
			_currentRewardData.coin *= num5;
			PersistRewardData();
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("计算离线收益失败: " + ex.Message + "\n" + ex.StackTrace));
			_currentRewardData = new OfflineRewardData();
		}
	}

	public void ClaimOfflineReward(bool isDouble = false)
	{
		if (!_currentRewardData.hasReward)
		{
			Debug.LogWarning((object)"没有可领取的离线收益");
			return;
		}
		try
		{
			double num = (isDouble ? 2.0 : 1.0);
			double num2 = _currentRewardData.coin * num;
			if (num2 <= 0.0 || double.IsNaN(num2) || double.IsInfinity(num2))
			{
				Debug.LogWarning((object)$"收益有问题: {num2}");
				_currentRewardData = new OfflineRewardData();
				return;
			}
			SingletonMono<UpgradeManager>.Instance.AddGoldInternal(num2);
			_currentRewardData = new OfflineRewardData();
			ClearPersistedRewardData();
			if ((Object)(object)GameDataManager.Instance != (Object)null)
			{
				GameDataManager.Instance.MarkPlayerActivelyEntered();
			}
			Debug.Log((object)$"离线收益领取成功，倍率: {num}x");
			GameDataManager.Instance.SaveGameData();
			UIManager.Instance.ShowCoinDimondTween(1);
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("领取离线收益失败: " + ex.Message + "\n" + ex.StackTrace));
		}
	}

	public string FormatTime(double seconds)
	{
		if (seconds < 60.0)
		{
			return $"{seconds:F0}秒";
		}
		if (seconds < 3600.0)
		{
			int num = Mathf.FloorToInt((float)(seconds / 60.0));
			int num2 = Mathf.FloorToInt((float)(seconds % 60.0));
			return $"{num}分{num2}秒";
		}
		int num3 = Mathf.FloorToInt((float)(seconds / 3600.0));
		int num4 = Mathf.FloorToInt((float)(seconds % 3600.0 / 60.0));
		return $"{num3}小时{num4}分";
	}

	public bool HasOfflineReward()
	{
		if (_currentRewardData != null)
		{
			return _currentRewardData.hasReward;
		}
		return false;
	}

	private void PersistRewardData()
	{
		if (_currentRewardData != null && _currentRewardData.hasReward)
		{
			PlayerPrefs.SetInt("OfflineReward_Persisted", 1);
			PlayerPrefs.SetString("OfflineReward_Coin", _currentRewardData.coin.ToString("R"));
			PlayerPrefs.SetString("OfflineReward_OfflineTime", _currentRewardData.offlineTime.ToString("R"));
			PlayerPrefs.Save();
		}
		else
		{
			ClearPersistedRewardData();
		}
	}

	private bool TryRestorePersistedReward()
	{
		if (PlayerPrefs.GetInt("OfflineReward_Persisted", 0) != 1)
		{
			return false;
		}
		try
		{
			string text = PlayerPrefs.GetString("OfflineReward_Coin", "");
			string text2 = PlayerPrefs.GetString("OfflineReward_OfflineTime", "");
			if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(text2))
			{
				ClearPersistedRewardData();
				return false;
			}
			double num = double.Parse(text);
			double num2 = double.Parse(text2);
			if (num <= 0.0 || double.IsNaN(num) || double.IsInfinity(num) || num2 <= 0.0 || double.IsNaN(num2) || double.IsInfinity(num2))
			{
				ClearPersistedRewardData();
				return false;
			}
			_currentRewardData = new OfflineRewardData
			{
				coin = num,
				offlineTime = num2,
				hasReward = true
			};
			Debug.Log((object)$"[OfflineIncome] 恢复持久化收益: coin={num}, offlineTime={num2}s");
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("[OfflineIncome] 恢复持久化收益失败: " + ex.Message));
			ClearPersistedRewardData();
			return false;
		}
	}

	private void ClearPersistedRewardData()
	{
		PlayerPrefs.DeleteKey("OfflineReward_Persisted");
		PlayerPrefs.DeleteKey("OfflineReward_Coin");
		PlayerPrefs.DeleteKey("OfflineReward_OfflineTime");
		PlayerPrefs.Save();
	}
}

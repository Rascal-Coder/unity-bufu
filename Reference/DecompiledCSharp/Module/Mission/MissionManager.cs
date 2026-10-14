using System;
using System.Collections.Generic;
using LitJson;
using Module.Http;
using P8SDKTT;
using TTSDK;
using TTSDK.UNBridgeLib.LitJson;
using UnityEngine;
using cfg;

namespace Module.Mission;

public class MissionManager : MonoBehaviour
{
	private const int LegacyCompletedMissionLevel = 33;

	private static MissionManager _instance;

	private List<mainMission> mainMissionList;

	private int currentMissionId;

	private mainMission currentMission;

	private bool _isProcessingMission;

	private List<mainMission> _pendingRewards = new List<mainMission>();

	private float _lastSaveImRankTime = -1f;

	public static MissionManager Instance
	{
		get
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Expected O, but got Unknown
			if ((Object)(object)_instance == (Object)null)
			{
				_instance = Object.FindObjectOfType<MissionManager>();
				if ((Object)(object)_instance == (Object)null)
				{
					GameObject val = new GameObject("MissionManager");
					_instance = val.AddComponent<MissionManager>();
					Object.DontDestroyOnLoad((Object)val);
				}
			}
			return _instance;
		}
	}

	public event Action<mainMission> OnMissionUpdated;

	public event Action<mainMission> OnMissionCompleted;

	public event Action<mainMission[]> AfterGoldReachUnlockEvent;

	public event Action<List<mainMission>> OnRewardsClaimed;

	public event Action CheckLimitChallenge;

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
	}

	public void Initialize()
	{
		LoadMissions();
		LoadCurrentMission();
	}

	public void ReloadCurrentWorld()
	{
		_pendingRewards.Clear();
		LoadMissions();
		LoadCurrentMission();
	}

	private void LoadMissions()
	{
		if (ConfigManager.HasInstance())
		{
			int worldId = ((!((Object)(object)GameDataManager.Instance != (Object)null)) ? 1 : GameDataManager.Instance.GetCurrentWorldId());
			mainMissionList = ConfigManager.Instance.GetAllMainMissionByWorld(worldId) ?? new List<mainMission>();
			mainMissionList.Sort((mainMission a, mainMission b) => a.Id.CompareTo(b.Id));
		}
	}

	private void LoadCurrentMission()
	{
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)GameDataManager.Instance == (Object)null)
		{
			return;
		}
		currentMissionId = GameDataManager.Instance.GetCurrentMainMissionLevel();
		currentMission = null;
		if (mainMissionList != null && mainMissionList.Count != 0)
		{
			bool flag = false;
			int num = mainMissionList.Count + 1;
			int num2 = mainMissionList.FindIndex((mainMission mission) => mission.Id == currentMissionId);
			if (currentMissionId > num && num2 >= 0)
			{
				currentMissionId = num2 + 1;
				flag = true;
			}
			if (currentMissionId <= 0)
			{
				currentMissionId = 1;
				flag = true;
			}
			else if (currentMissionId > num)
			{
				currentMissionId = num;
				flag = true;
			}
			if (currentMissionId == 33 && (Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null && SingletonMono<UpgradeManager>.Instance.TotalGold >= mainMissionList[mainMissionList.Count - 1].MainMissionNum)
			{
				currentMissionId = num;
				flag = true;
			}
			currentMission = GetMissionByLevel(currentMissionId);
			if (flag)
			{
				SaveCurrentMission();
			}
			SaveImRank();
			if (currentMission != null)
			{
				P8SDK.LevelUpgrade(new LevelUpgradeOption
				{
					level = GetReportedLevel(currentMissionId),
					level_status = "0"
				});
			}
		}
	}

	public mainMission GetCurrentMission()
	{
		return currentMission;
	}

	public int GetCurrentMissionId()
	{
		return currentMissionId;
	}

	public mainMission GetMissionById(int id)
	{
		if (mainMissionList == null)
		{
			return null;
		}
		return mainMissionList.Find((mainMission mission) => mission.Id == id);
	}

	private mainMission GetMissionByLevel(int level)
	{
		if (mainMissionList == null || level < 1 || level > mainMissionList.Count)
		{
			return null;
		}
		return mainMissionList[level - 1];
	}

	public mainMission GetNextMission()
	{
		return GetMissionByLevel(currentMissionId + 1);
	}

	public int GetMissionLevel(mainMission mission)
	{
		if (mission == null || mainMissionList == null)
		{
			return 0;
		}
		int num = mainMissionList.FindIndex((mainMission item) => item.Id == mission.Id);
		if (num < 0)
		{
			return 0;
		}
		return num + 1;
	}

	public bool HasNextMission()
	{
		return GetNextMission() != null;
	}

	public void CheckMissionProgress(double totalGold)
	{
		if (_isProcessingMission)
		{
			return;
		}
		_isProcessingMission = true;
		try
		{
			_pendingRewards.Clear();
			TryTriggerRandomEvent(totalGold);
			if ((Object)(object)UIManager.Instance != (Object)null)
			{
				DownItem downItem = UIManager.Instance.GetDownItem();
				if ((Object)(object)downItem != (Object)null && !downItem.GetChallengeIsOpen() && (currentMission == null || !(totalGold >= currentMission.MainMissionNum)))
				{
					if (IsFactorConditionMet(31, totalGold))
					{
						downItem.TriggerGongfuPopupOnce();
					}
					if (IsFactorConditionMet(33, totalGold))
					{
						downItem.TriggerDashePopupOnce();
					}
				}
			}
			while (currentMission != null && totalGold >= currentMission.MainMissionNum)
			{
				if ((Object)(object)UIManager.Instance != (Object)null)
				{
					DownItem downItem2 = UIManager.Instance.GetDownItem();
					if ((Object)(object)downItem2 != (Object)null && downItem2.GetChallengeIsOpen())
					{
						return;
					}
				}
				AuctionDataManager instance = AuctionDataManager.Instance;
				if (instance != null && instance.GetAuctionIsOpen())
				{
					return;
				}
				CompleteMission();
			}
			if (_pendingRewards.Count > 0)
			{
				mainMission[] rewardsArray = _pendingRewards.ToArray();
				_pendingRewards.Clear();
				UIManager.Instance.ShowGoalReachPanel(rewardsArray, delegate
				{
					this.AfterGoldReachUnlockEvent?.Invoke(rewardsArray);
				});
			}
		}
		finally
		{
			_isProcessingMission = false;
		}
	}

	private bool TryTriggerRandomEvent(double totalGold)
	{
		if (currentMission == null)
		{
			return false;
		}
		if ((Object)(object)UIManager.Instance != (Object)null)
		{
			DownItem downItem = UIManager.Instance.GetDownItem();
			if ((Object)(object)downItem != (Object)null && downItem.GetChallengeIsOpen())
			{
				return false;
			}
		}
		if (!ConfigManager.HasInstance())
		{
			return false;
		}
		if ((Object)(object)GameDataManager.Instance == (Object)null || (Object)(object)UIManager.Instance == (Object)null || (Object)(object)SingletonMono<UpgradeManager>.Instance == (Object)null)
		{
			return false;
		}
		if (TryTriggerRandomMoneyEvent(37, 0, totalGold))
		{
			return true;
		}
		if (TryTriggerRandomMoneyEvent(38, 1, totalGold))
		{
			return true;
		}
		if (TryTriggerRandomMoneyEvent(39, 2, totalGold))
		{
			return true;
		}
		if (TryTriggerRandomDiamondEvent(40, 3, totalGold))
		{
			return true;
		}
		return false;
	}

	private bool TryTriggerRandomMoneyEvent(int commonId, int eventIndex, double totalGold)
	{
		if (GameDataManager.Instance.IsRandomEventTriggered(eventIndex))
		{
			return false;
		}
		float commonFactor = ConfigManager.Instance.GetCommonFactor(commonId);
		if (!TryParseRandomEventFactor(commonFactor, out var missionId, out var progress))
		{
			return false;
		}
		if (missionId != currentMissionId)
		{
			return false;
		}
		double num = currentMission.MainMissionNum * (double)progress;
		if (totalGold < num)
		{
			return false;
		}
		if (num <= 0.0)
		{
			return false;
		}
		GameDataManager.Instance.MarkRandomEventTriggered(eventIndex);
		CoinSlotSupportDialog.PendingRandomMoneyEventId = commonId;
		UIManager.Instance.ShowSupportUI(DialogType.randomMoney, num);
		return true;
	}

	private bool TryTriggerRandomDiamondEvent(int commonId, int eventIndex, double totalGold)
	{
		if (GameDataManager.Instance.IsRandomEventTriggered(eventIndex))
		{
			return false;
		}
		float commonFactor = ConfigManager.Instance.GetCommonFactor(commonId);
		if (!TryParseRandomEventFactor(commonFactor, out var missionId, out var progress))
		{
			return false;
		}
		if (missionId != currentMissionId)
		{
			return false;
		}
		double num = currentMission.MainMissionNum * (double)progress;
		if (totalGold < num)
		{
			return false;
		}
		GameDataManager.Instance.MarkRandomEventTriggered(eventIndex);
		UIManager.Instance.ShowSupportUI(DialogType.randomDiamond, 0.0);
		return true;
	}

	private bool TryParseRandomEventFactor(float factor, out int missionId, out float progress)
	{
		missionId = 0;
		progress = 0f;
		if (factor <= 0f)
		{
			return false;
		}
		missionId = Mathf.FloorToInt(factor + 0.0001f);
		progress = factor - (float)missionId;
		progress = Mathf.Round(progress * 100f) / 100f;
		if (missionId <= 0)
		{
			return false;
		}
		if (progress <= 0f || progress >= 1f)
		{
			return false;
		}
		return true;
	}

	public bool IsFactorConditionMet(int commonId, double totalGold)
	{
		if (!ConfigManager.HasInstance())
		{
			return false;
		}
		float commonFactor = ConfigManager.Instance.GetCommonFactor(commonId);
		if (commonFactor <= 0f)
		{
			return false;
		}
		int num = Mathf.FloorToInt(commonFactor + 0.0001f);
		float num2 = commonFactor - (float)num;
		num2 = Mathf.Round(num2 * 100f) / 100f;
		if (num <= 0)
		{
			return false;
		}
		if (currentMissionId > num)
		{
			return true;
		}
		if (currentMissionId == num)
		{
			if (num2 <= 0f)
			{
				return true;
			}
			if (currentMission != null)
			{
				double num3 = currentMission.MainMissionNum * (double)num2;
				if (totalGold >= num3)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void CompleteMission()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		if (currentMission == null)
		{
			return;
		}
		this.OnMissionCompleted?.Invoke(currentMission);
		_pendingRewards.Add(currentMission);
		try
		{
			GameData gameData = GameDataManager.Instance.GetGameData();
			string reportedLevel = GetReportedLevel(currentMissionId);
			P8SDK.LevelUpgrade(new LevelUpgradeOption
			{
				level = reportedLevel,
				level_status = "1"
			});
			P8SDK.UpgradeRecord(new UpgradeRecordOption
			{
				level = reportedLevel,
				oaid = "",
				onlinetime = Time.realtimeSinceStartup.ToString(),
				roleid = gameData.localUserId,
				rolename = gameData.openId,
				sid = SingletonMono<ResManager>.Instance.Version,
				vip = "0",
				success = delegate
				{
					P8SDK.Log("UpgradeRecord success.");
				},
				fail = delegate(GeneralCallbackData res)
				{
					//IL_0005: Unknown result type (might be due to invalid IL or missing references)
					P8SDK.Log("UpgradeRecord fail: " + JsonMapper.ToJson((object)res));
				}
			});
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("P8Complete Level error: " + ex.Message));
		}
		if (HasNextMission())
		{
			currentMissionId++;
			currentMission = GetMissionByLevel(currentMissionId);
			SaveCurrentMission();
			this.OnMissionUpdated?.Invoke(currentMission);
			WorldCupDataManager.Instance.AddWorldCupMissionProgress(2, 1);
			if (currentMissionId > 2)
			{
				UIManager.Instance.topPanel.ShowTip(isShow: false);
			}
			P8SDK.LevelUpgrade(new LevelUpgradeOption
			{
				level = GetReportedLevel(currentMissionId),
				level_status = "0"
			});
		}
		else
		{
			currentMission = null;
			currentMissionId = mainMissionList.Count + 1;
			SaveCurrentMission();
			this.OnMissionUpdated?.Invoke(null);
		}
		SaveImRank(force: true);
		this.CheckLimitChallenge?.Invoke();
	}

	public void SaveImRank(bool force = false)
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Expected O, but got Unknown
		if (!force && _lastSaveImRankTime > 0f && Time.unscaledTime - _lastSaveImRankTime < 1f)
		{
			return;
		}
		_lastSaveImRankTime = Time.unscaledTime;
		int worldId = GameDataManager.Instance.GetCurrentWorldId();
		int rankLevel = ((mainMissionList != null && mainMissionList.Count > 0) ? Mathf.Clamp(currentMissionId, 1, mainMissionList.Count) : 0);
		if (string.IsNullOrEmpty(GameDataManager.Instance.GetOpenId()))
		{
			return;
		}
		TT.SetImRankData(new JsonData
		{
			["dataType"] = JsonData.op_Implicit(0),
			["value"] = JsonData.op_Implicit(rankLevel),
			["priority"] = JsonData.op_Implicit(0),
			["zoneId"] = JsonData.op_Implicit(WorldRankUtility.GetZoneId(worldId))
		}, (Action<bool, string>)delegate(bool isSuccess, string errMsg)
		{
			if (isSuccess)
			{
				Debug.Log((object)$"SetImRankData success, worldId={worldId}, rankLevel={rankLevel}");
			}
			else
			{
				Debug.Log((object)$"SetImRankData fail, worldId={worldId}, res: {errMsg}");
			}
		});
		if ((Object)(object)SingletonMono<HttpManager>.Instance != (Object)null)
		{
			SingletonMono<HttpManager>.Instance.SubmitScore(rankLevel);
		}
		GameDataManager.Instance.SaveAll2Server();
	}

	private void ClaimAllRewards()
	{
		double num = 0.0;
		int num2 = 0;
		foreach (mainMission pendingReward in _pendingRewards)
		{
			if (pendingReward.RewardType == 1)
			{
				num += pendingReward.RewardNum;
			}
			else if (pendingReward.RewardType == 2)
			{
				num2 += (int)pendingReward.RewardNum;
			}
		}
		if (num > 0.0)
		{
			SingletonMono<UpgradeManager>.Instance.AddGoldInternal(num);
		}
		if (num2 > 0)
		{
			GameDataManager.Instance.AddDiamond(num2);
		}
		this.OnRewardsClaimed?.Invoke(new List<mainMission>(_pendingRewards));
		_pendingRewards.Clear();
	}

	public void ClaimReward()
	{
		if (_pendingRewards.Count > 0)
		{
			ClaimAllRewards();
		}
	}

	public void ClearPendingRewards()
	{
		_pendingRewards.Clear();
	}

	public void SetCurrentMission(int missionLevel)
	{
		currentMissionId = missionLevel;
		currentMission = GetMissionByLevel(missionLevel);
		SaveCurrentMission();
	}

	private void SaveCurrentMission()
	{
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			GameDataManager.Instance.SetCurrentMainMissionLevel(currentMissionId);
		}
		SingletonMono<UpgradeManager>.Instance.ForceSave();
	}

	public double GetCurrentMissionTarget()
	{
		if (currentMission == null)
		{
			return 0.0;
		}
		return currentMission.MainMissionNum;
	}

	public double GetCurrentMissionReward()
	{
		if (currentMission == null)
		{
			return 0.0;
		}
		return currentMission.RewardNum;
	}

	public int GetCurrentMissionRewardType()
	{
		if (currentMission == null)
		{
			return 0;
		}
		return currentMission.RewardType;
	}

	public int GetTotalMissionCount()
	{
		return mainMissionList?.Count ?? 0;
	}

	public List<mainMission> GetAllMissions()
	{
		return mainMissionList;
	}

	public int GetPendingRewardCount()
	{
		return _pendingRewards.Count;
	}

	public List<mainMission> GetPendingRewards()
	{
		return new List<mainMission>(_pendingRewards);
	}

	private string GetReportedLevel(int missionId)
	{
		int num = ((!GameDataManager.HasInstance()) ? 1 : GameDataManager.Instance.GetCurrentWorldId());
		if (num <= 1)
		{
			return missionId.ToString();
		}
		return $"{missionId}_{num}";
	}

	public void GMCompleteMission()
	{
		CompleteMission();
		if (_pendingRewards.Count > 0)
		{
			mainMission[] rewardsArray = _pendingRewards.ToArray();
			_pendingRewards.Clear();
			UIManager.Instance.ShowGoalReachPanel(rewardsArray, delegate
			{
				this.AfterGoldReachUnlockEvent?.Invoke(rewardsArray);
			});
		}
	}
}

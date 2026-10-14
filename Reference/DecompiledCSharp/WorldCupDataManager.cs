using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using cfg;

public class WorldCupDataManager
{
	private static WorldCupDataManager instance;

	private List<double> reward = new List<double>();

	private List<int> rewardType = new List<int>();

	private List<double> rewardCount = new List<double>();

	private string worldCupGuid = "worldCupGuid";

	private GameObject wolrdCupObj;

	private Button worldCupSignBtn;

	private GameObject suo;

	private int worldIdMomentary;

	private GameObject worldCupPanelObj;

	public static WorldCupDataManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new WorldCupDataManager();
			}
			return instance;
		}
	}

	public int GetRewardDay()
	{
		int result = 0;
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			result = GetWorldCupSignDay();
		}
		return result;
	}

	public bool IsCanClamp(int day, int index)
	{
		bool result = false;
		if (GetRewardDay() >= day && (Object)(object)GameDataManager.Instance != (Object)null)
		{
			result = GetWorldCupItemClamp(index) <= 0;
		}
		return result;
	}

	public bool GetSignPanelRedPoint()
	{
		if (!IsWorldCupActive())
		{
			return false;
		}
		int worldCupSignDay = GetWorldCupSignDay();
		for (int i = 0; i < worldCupSignDay && i < 7; i++)
		{
			int index = i * 2;
			int index2 = i * 2 + 1;
			if (GetWorldCupItemClamp(index) == 0 || GetWorldCupItemClamp(index2) == 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool GetMissionPanelRedPoint()
	{
		if (!IsWorldCupActive())
		{
			return false;
		}
		for (int i = 0; i < 6; i++)
		{
			WorldCupMisson oneWorldCupMisson = ConfigManager.Instance.GetOneWorldCupMisson(i + 1);
			if (oneWorldCupMisson != null && GetWorldCupMissionProgress(i) >= oneWorldCupMisson.MissionNum && GetWorldCupMissionClamp(i) == 0)
			{
				return true;
			}
		}
		int worldScore = GetWorldScore();
		for (int j = 0; j < 4; j++)
		{
			WorldUpScoreReward worldCupScoreReward = ConfigManager.Instance.GetWorldCupScoreReward(j + 1);
			if (worldCupScoreReward != null && worldScore >= worldCupScoreReward.ScoreProgress && GetWorldCupScoreReward(j) == 0)
			{
				return true;
			}
		}
		return false;
	}

	public void IsOpenTopRedPoint()
	{
		if (GameDataManager.Instance.GetCurrentMainMissionLevel() < 5)
		{
			return;
		}
		bool signPanelRedPoint = GetSignPanelRedPoint();
		bool missionPanelRedPoint = GetMissionPanelRedPoint();
		DownItem downItem = UIManager.Instance.GetDownItem();
		if (!((Object)(object)downItem != (Object)null))
		{
			return;
		}
		GameObject worldCupRepoint = downItem.worldCupRepoint;
		if (worldCupRepoint != null)
		{
			worldCupRepoint.SetActive(signPanelRedPoint || missionPanelRedPoint);
		}
		if (PlayerPrefs.GetInt(worldCupGuid, 0) <= 0)
		{
			if (GameDataManager.Instance.GetCurrentMainMissionLevel() >= 5)
			{
				downItem.worldCupTip.SetActive(true);
			}
			PlayerPrefs.SetInt(worldCupGuid, 10);
		}
	}

	public List<double> GetSignItemList(int day, int index)
	{
		reward.Clear();
		rewardType.Clear();
		rewardCount.Clear();
		signWorldCup oneSignWorld = ConfigManager.Instance.GetOneSignWorld(day);
		List<int> list = new List<int>();
		rewardType = new List<int>();
		rewardCount = new List<double>();
		list = ((index != 0) ? oneSignWorld.AdReward : oneSignWorld.NormalReward);
		if (list[0] == 3 || list[0] == 4 || list[0] == 5)
		{
			for (int i = 0; i < list[1]; i++)
			{
				int item = ConfigManager.Instance.TransferSuipianRealId(list[0]);
				if (!rewardType.Contains(item))
				{
					rewardType.Add(item);
					rewardCount.Add(1.0);
				}
				else
				{
					int index2 = rewardType.IndexOf(item);
					rewardCount[index2]++;
				}
			}
		}
		else if (list[0] == 1)
		{
			rewardType.Add(list[0]);
			rewardCount.Add((double)list[1] * SingletonMono<UpgradeManager>.Instance.GetAverageGoldPerMinuteFromLast30Seconds());
		}
		else
		{
			rewardType.Add(list[0]);
			rewardCount.Add(list[1]);
		}
		if (list[2] == 3 || list[2] == 4 || list[2] == 5)
		{
			for (int j = 0; j < list[3]; j++)
			{
				int item2 = ConfigManager.Instance.TransferSuipianRealId(list[2]);
				if (!rewardType.Contains(item2))
				{
					rewardType.Add(item2);
					rewardCount.Add(1.0);
				}
				else
				{
					int index3 = rewardType.IndexOf(item2);
					rewardCount[index3]++;
				}
			}
		}
		else if (list[2] == 1)
		{
			rewardType.Add(list[2]);
			rewardCount.Add((double)list[3] * SingletonMono<UpgradeManager>.Instance.GetAverageGoldPerMinuteFromLast30Seconds());
		}
		else
		{
			rewardType.Add(list[2]);
			rewardCount.Add(list[3]);
		}
		for (int k = 0; k < rewardType.Count; k++)
		{
			reward.Add(rewardType[k]);
			reward.Add(rewardCount[k]);
		}
		return reward;
	}

	public List<double> ChuLiSuiJiSuiPian(int itemid, int count)
	{
		List<double> list = new List<double>();
		List<double> list2 = new List<double>();
		List<double> list3 = new List<double>();
		if (itemid == 3 || itemid == 4 || itemid == 5)
		{
			for (int i = 0; i < count; i++)
			{
				int num = ConfigManager.Instance.TransferSuipianRealId(itemid);
				if (!list2.Contains(num))
				{
					list2.Add(num);
					list3.Add(1.0);
				}
				else
				{
					list3[list2.IndexOf(num)]++;
				}
			}
		}
		for (int j = 0; j < list2.Count; j++)
		{
			list.Add(list2[j]);
			list.Add(list3[j]);
		}
		return list;
	}

	public long GetWorldCupSignFirstShowTime()
	{
		return GameDataManager.Instance.CurrentWorldCupData?.signFirstShowTime ?? 0;
	}

	public void SetWorldCupSignFirstShowTime(long timestampMs)
	{
		if (GameDataManager.Instance.CurrentWorldCupData != null)
		{
			GameDataManager.Instance.CurrentWorldCupData.signFirstShowTime = timestampMs;
			GameDataManager.Instance.SaveWorldCupDataOnly();
		}
	}

	public int GetWorldCupSignDay()
	{
		return GameDataManager.Instance.CurrentWorldCupData?.signDay ?? 1;
	}

	public void SetWorldCupSignDay(int day)
	{
		if (GameDataManager.Instance.CurrentWorldCupData != null)
		{
			GameDataManager.Instance.CurrentWorldCupData.signDay = day;
			GameDataManager.Instance.SaveWorldCupDataOnly();
		}
	}

	public void UpdateWorldCupSignDay()
	{
		if (GameDataManager.Instance.CurrentWorldCupData == null)
		{
			return;
		}
		long signFirstShowTime = GameDataManager.Instance.CurrentWorldCupData.signFirstShowTime;
		if (signFirstShowTime <= 0)
		{
			return;
		}
		DateTime date = new DateTime(1970, 1, 1).AddMilliseconds(signFirstShowTime).AddHours(8.0).Date;
		int num = (DateTime.UtcNow.AddHours(8.0).Date - date).Days + 1;
		if (num > 7)
		{
			num = 7;
		}
		if (num < 1)
		{
			num = 1;
		}
		if (GameDataManager.Instance.CurrentWorldCupData.signDay != num)
		{
			if (GameDataManager.Instance.CurrentWorldCupData.signDay < num)
			{
				GameDataManager.Instance.CurrentWorldCupData.signDay = num;
			}
			GameDataManager.Instance.SaveWorldCupDataOnly();
		}
	}

	public void UpdateWorldCupScoreCrossDay()
	{
		if (GameDataManager.Instance.CurrentWorldCupData == null)
		{
			return;
		}
		long signFirstShowTime = GameDataManager.Instance.CurrentWorldCupData.signFirstShowTime;
		if (signFirstShowTime <= 0)
		{
			return;
		}
		DateTime date = new DateTime(1970, 1, 1).AddMilliseconds(signFirstShowTime).AddHours(8.0).Date;
		int num = (DateTime.UtcNow.AddHours(8.0).Date - date).Days + 1;
		if (num > 7)
		{
			return;
		}
		if (num < 1)
		{
			num = 1;
		}
		if (GameDataManager.Instance.CurrentWorldCupData.scoreLastDay != num)
		{
			GameDataManager.Instance.CurrentWorldCupData.score = 0;
			GameDataManager.Instance.CurrentWorldCupData.scoreLastDay = num;
			for (int i = 0; i < GameDataManager.Instance.CurrentWorldCupData.missionProgress.Length; i++)
			{
				GameDataManager.Instance.CurrentWorldCupData.missionProgress[i] = 0;
			}
			for (int j = 0; j < GameDataManager.Instance.CurrentWorldCupData.missionClamp.Length; j++)
			{
				GameDataManager.Instance.CurrentWorldCupData.missionClamp[j] = 0;
			}
			for (int k = 0; k < GameDataManager.Instance.CurrentWorldCupData.scoreClamp.Length; k++)
			{
				GameDataManager.Instance.CurrentWorldCupData.scoreClamp[k] = 0;
			}
			GameDataManager.Instance.SaveWorldCupDataOnly();
			Debug.Log((object)$"[WorldCup] 跨天重置积分，当前第 {num} 天");
		}
	}

	public int GetWorldCupItemClamp(int index)
	{
		return GameDataManager.Instance.CurrentWorldCupData.clamp[index];
	}

	public void SetWorldCupItemClamp(int index, int isclamp)
	{
		GameDataManager.Instance.CurrentWorldCupData.clamp[index] = isclamp;
		GameDataManager.Instance.SaveWorldCupDataOnly();
	}

	public int[] GetWroldCupItems()
	{
		return GameDataManager.Instance.CurrentWorldCupData.clamp;
	}

	public int GetWorldScore()
	{
		return GameDataManager.Instance.CurrentWorldCupData.score;
	}

	public void SetWorldScore(int score)
	{
		GameDataManager.Instance.CurrentWorldCupData.score = score;
		GameDataManager.Instance.SaveWorldCupDataOnly();
	}

	public void AddWorldScore(int score)
	{
		GameDataManager.Instance.CurrentWorldCupData.score += score;
		if (GameDataManager.Instance.CurrentWorldCupData.score >= 100)
		{
			GameDataManager.Instance.CurrentWorldCupData.score = 100;
		}
		GameDataManager.Instance.SaveWorldCupDataOnly();
	}

	public int GetWorldCupMissionProgress(int index)
	{
		if (GameDataManager.Instance.CurrentWorldCupData.missionProgress.Length > index)
		{
			return GameDataManager.Instance.CurrentWorldCupData.missionProgress[index];
		}
		return 0;
	}

	public void AddWorldCupMissionProgress(int missionIndex, int addCount)
	{
		if (GameDataManager.Instance.CurrentWorldCupData == null || missionIndex < 0 || missionIndex >= GameDataManager.Instance.CurrentWorldCupData.missionProgress.Length)
		{
			return;
		}
		WorldCupMisson oneWorldCupMisson = ConfigManager.Instance.GetOneWorldCupMisson(missionIndex + 1);
		if (oneWorldCupMisson == null)
		{
			return;
		}
		int missionNum = oneWorldCupMisson.MissionNum;
		int num = GameDataManager.Instance.CurrentWorldCupData.missionProgress[missionIndex];
		if (num < missionNum)
		{
			num += addCount;
			if (num > missionNum)
			{
				num = missionNum;
			}
			GameDataManager.Instance.CurrentWorldCupData.missionProgress[missionIndex] = num;
			GameDataManager.Instance.SaveWorldCupDataOnly();
			IsOpenTopRedPoint();
		}
	}

	public int GetWorldCupMissionClamp(int index)
	{
		if (GameDataManager.Instance.CurrentWorldCupData.missionClamp.Length > index)
		{
			return GameDataManager.Instance.CurrentWorldCupData.missionClamp[index];
		}
		return 0;
	}

	public void SetWorldCupMissionClamp(int index, int state)
	{
		if (GameDataManager.Instance.CurrentWorldCupData.missionClamp.Length > index)
		{
			GameDataManager.Instance.CurrentWorldCupData.missionClamp[index] = state;
			GameDataManager.Instance.SaveWorldCupDataOnly();
		}
	}

	public int GetWorldCupScoreReward(int index)
	{
		if (GameDataManager.Instance.CurrentWorldCupData.scoreClamp.Length > index)
		{
			return GameDataManager.Instance.CurrentWorldCupData.scoreClamp[index];
		}
		return 0;
	}

	public void SetWorldCupScoreReward(int index, int state)
	{
		GameDataManager gameDataManager = GameDataManager.Instance;
		if (!((Object)(object)gameDataManager == (Object)null) && gameDataManager.CurrentWorldCupData != null && index >= 0 && gameDataManager.CurrentWorldCupData.scoreClamp != null && gameDataManager.CurrentWorldCupData.scoreClamp.Length > index && gameDataManager.CurrentWorldCupData.scoreClamp[index] != state)
		{
			gameDataManager.CurrentWorldCupData.scoreClamp[index] = state;
			gameDataManager.SaveWorldCupDataOnly();
		}
	}

	public bool IsWorldCupAvailable()
	{
		if (!ConfigManager.HasInstance() || ConfigManager.Instance.GetCommonFactor(47) <= 0f)
		{
			return false;
		}
		if (GameDataManager.Instance.GetCurrentMainMissionLevel() < 3)
		{
			return false;
		}
		return true;
	}

	public bool IsWorldCupActive()
	{
		if (!IsWorldCupAvailable())
		{
			return false;
		}
		long worldCupSignFirstShowTime = GetWorldCupSignFirstShowTime();
		if (worldCupSignFirstShowTime <= 0)
		{
			return false;
		}
		long num = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
		long num2 = 604800000L;
		return num - worldCupSignFirstShowTime <= num2;
	}

	public void CheckWorldCupSignBtn(GameObject wolrdCupObj, Button worldCupSignBtn, GameObject suo, int wolrid)
	{
		if ((Object)(object)wolrdCupObj == (Object)null || (Object)(object)worldCupSignBtn == (Object)null || (Object)(object)worldCupSignBtn == (Object)null || (Object)(object)suo == (Object)null)
		{
			return;
		}
		if (wolrid != 1)
		{
			wolrdCupObj.SetActive(false);
			return;
		}
		if ((Object)(object)this.wolrdCupObj == (Object)null)
		{
			this.wolrdCupObj = wolrdCupObj;
		}
		if ((Object)(object)this.worldCupSignBtn == (Object)null)
		{
			this.worldCupSignBtn = worldCupSignBtn;
		}
		if ((Object)(object)this.suo == (Object)null)
		{
			this.suo = suo;
		}
		if (!IsWorldCupAvailable())
		{
			((Component)worldCupSignBtn).gameObject.SetActive(false);
			wolrdCupObj.SetActive(false);
			return;
		}
		long num = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
		long num2 = 604800000L;
		long worldCupSignFirstShowTime = GetWorldCupSignFirstShowTime();
		if (worldCupSignFirstShowTime > 0)
		{
			if (num - worldCupSignFirstShowTime > num2)
			{
				((Component)worldCupSignBtn).gameObject.SetActive(false);
				wolrdCupObj.SetActive(false);
				return;
			}
			UpdateWorldCupSignDay();
		}
		else
		{
			SetWorldCupSignFirstShowTime(num);
			UpdateWorldCupSignDay();
		}
		((Component)worldCupSignBtn).gameObject.SetActive(true);
		if (wolrid == 1)
		{
			wolrdCupObj.SetActive(true);
		}
		else
		{
			wolrdCupObj.SetActive(false);
		}
		if (GameDataManager.Instance.GetCurrentMainMissionLevel() < 5)
		{
			suo.SetActive(true);
		}
		else
		{
			suo.SetActive(false);
		}
		IsOpenTopRedPoint();
	}

	public void HandleWorldCupSignClick()
	{
		SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
		if (GameDataManager.Instance.GetCurrentMainMissionLevel() < 5)
		{
			BroadcastManager.ShowPopUpTip("活动将在5级开放");
			return;
		}
		UpdateWorldCupSignDay();
		UpdateWorldCupScoreCrossDay();
		Transform main = UIManager.Instance.GetLayerTransform(UILayer.Popup);
		if ((Object)(object)worldCupPanelObj == (Object)null)
		{
			SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/NewUI/CheckInWorldCup/prefabs/WorldCupPanel.prefab", delegate(GameObject prefab)
			{
				if ((Object)(object)main != (Object)null)
				{
					worldCupPanelObj = Object.Instantiate<GameObject>(prefab, main);
					if ((Object)(object)WorldCupPanel.Instance != (Object)null)
					{
						WorldCupPanel.Instance.RefrashPanel(1);
					}
				}
			});
		}
		else
		{
			worldCupPanelObj.SetActive(true);
			if ((Object)(object)WorldCupPanel.Instance != (Object)null)
			{
				WorldCupPanel.Instance.RefrashPanel(1);
			}
		}
		DownItem downItem = UIManager.Instance.GetDownItem();
		if ((Object)(object)downItem != (Object)null)
		{
			downItem.worldCupTip.SetActive(false);
		}
	}
}

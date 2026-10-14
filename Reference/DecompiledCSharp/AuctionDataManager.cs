using System;
using System.Collections.Generic;
using Module.Mission;
using UnityEngine;
using cfg;

public class AuctionDataManager
{
	public AuctionSeclect auctionSelect;

	private GameObject auctionObj;

	private int currentSelectIndex;

	private string[] surnames = new string[30]
	{
		"张", "李", "王", "刘", "陈", "杨", "赵", "黄", "周", "吴",
		"徐", "孙", "马", "朱", "胡", "郭", "林", "何", "高", "罗",
		"梁", "宋", "郑", "谢", "韩", "唐", "冯", "于", "董", "萧"
	};

	private string[] singleNames = new string[30]
	{
		"伟", "强", "芳", "敏", "静", "丽", "磊", "军", "洋", "勇",
		"杰", "娟", "艳", "涛", "明", "超", "秀", "平", "刚", "桂",
		"华", "辉", "宇", "浩", "然", "轩", "豪", "辰", "诺", "逸"
	};

	private string[] doubleNames = new string[20]
	{
		"子涵", "宇航", "浩然", "梓轩", "子豪", "雨桐", "诗涵", "思颖", "梦琪", "欣怡",
		"文博", "天佑", "俊熙", "晨曦", "语嫣", "紫萱", "佳怡", "子墨", "云熙", "景行"
	};

	public bool auctionIsOpen;

	private const string auctionCount = "auctionCount";

	private static AuctionDataManager instance;

	private Dictionary<int, List<int>> rarityItemDict;

	public static AuctionDataManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new AuctionDataManager();
			}
			return instance;
		}
	}

	public void OpenAuctionObj()
	{
		if ((Object)(object)auctionObj == (Object)null)
		{
			SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/NewUI/Auction/prefabs/auctionSeclect.prefab", delegate(GameObject prefab)
			{
				//IL_0037: Unknown result type (might be due to invalid IL or missing references)
				if ((Object)(object)UpgradePanel.Instance != (Object)null)
				{
					Transform layerTransform = UIManager.Instance.GetLayerTransform(UILayer.Main);
					auctionObj = Object.Instantiate<GameObject>(prefab, layerTransform);
					Transform transform = auctionObj.transform;
					((RectTransform)((transform is RectTransform) ? transform : null)).anchoredPosition = Vector2.zero;
					auctionObj.SetActive(true);
					auctionSelect = auctionObj.GetComponent<AuctionSeclect>();
					auctionSelect?.RefreshPanel();
				}
			});
		}
		else
		{
			auctionObj.SetActive(true);
			auctionSelect?.MoveToMaxLevel();
			auctionSelect?.RefreshPanel();
		}
	}

	public int GetSelectCount()
	{
		return ConfigManager.Instance.GetAllAuction()?.Count ?? 0;
	}

	public int GetTicketCount()
	{
		GameData gameData = GameDataManager.Instance.GetGameData();
		if (gameData == null)
		{
			return 0;
		}
		auction auctionConfig = ConfigManager.Instance.GetAuctionConfig(1);
		if (gameData.ticketCount < 3 && gameData.lastTicketRecoverTime > 0)
		{
			long num = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
			long num2 = num - gameData.lastTicketRecoverTime;
			long num3 = (auctionConfig?.TimeRecover ?? 10) * 60 * 1000;
			if (num2 >= num3)
			{
				int num4 = (int)(num2 / num3);
				int ticketCount = Mathf.Min(3, gameData.ticketCount + num4);
				gameData.ticketCount = ticketCount;
				gameData.lastTicketRecoverTime = num;
				GameDataManager.Instance?.SaveGameDataOnly();
			}
		}
		else if (gameData.ticketCount >= 3)
		{
			gameData.lastTicketRecoverTime = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
		}
		return gameData.ticketCount;
	}

	public void SetTicketCount(int count)
	{
		GameData gameData = GameDataManager.Instance.GetGameData();
		if (gameData != null)
		{
			gameData.ticketCount = count;
			if (gameData.ticketCount < 3 && gameData.lastTicketRecoverTime == 0L)
			{
				gameData.lastTicketRecoverTime = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
			}
			else if (gameData.ticketCount >= 3)
			{
				gameData.lastTicketRecoverTime = 0L;
			}
		}
		GameDataManager.Instance?.SaveGameDataOnly();
	}

	public bool ConsumeTicket()
	{
		int ticketCount = GetTicketCount();
		if (ticketCount <= 0)
		{
			return false;
		}
		SetTicketCount(ticketCount - 1);
		return true;
	}

	public double GetAuctionCoin()
	{
		return GameDataManager.Instance.GetGameData()?.auctionCoin ?? 0.0;
	}

	public void SetAuctionCoin(double count)
	{
		GameData gameData = GameDataManager.Instance.GetGameData();
		if (gameData != null)
		{
			gameData.auctionCoin = count;
		}
		GameDataManager.Instance?.SaveGameDataOnly();
	}

	public bool GetIsCanAuction(int id)
	{
		bool result = false;
		auction auctionConfig = ConfigManager.Instance.GetAuctionConfig(id);
		if (auctionConfig == null)
		{
			Debug.LogError((object)$"AuctionSelectItem: 找不到 id={id} 的 auction 配置");
			return result;
		}
		if ((float)(GameDataManager.Instance?.GetCurrentMainMissionLevel() ?? 1) >= auctionConfig.ConditionLevel)
		{
			return true;
		}
		return false;
	}

	public int GetUnlockAuctionId()
	{
		int num = 0;
		List<auction> allAuction = ConfigManager.Instance.GetAllAuction();
		int num2 = GameDataManager.Instance?.GetCurrentMainMissionLevel() ?? 0;
		foreach (auction item in allAuction)
		{
			if ((float)num2 >= item.ConditionLevel)
			{
				num++;
				continue;
			}
			break;
		}
		return num;
	}

	public int GetCurrentSelect()
	{
		return currentSelectIndex;
	}

	public void SetCurrentSelect(int index)
	{
		currentSelectIndex = index;
	}

	public string[] GetRandomNames(int count = 5)
	{
		List<string> list = new List<string>();
		int num = 1000;
		int num2 = 0;
		while (list.Count < count && num2 < num)
		{
			num2++;
			string text = surnames[Random.Range(0, surnames.Length)];
			string item;
			if (Random.Range(0, 2) == 0)
			{
				string text2 = doubleNames[Random.Range(0, doubleNames.Length)];
				item = text + text2;
			}
			else
			{
				string text3 = singleNames[Random.Range(0, singleNames.Length)];
				item = text + text3;
			}
			if (!list.Contains(item))
			{
				list.Add(item);
			}
		}
		return list.ToArray();
	}

	public int GetItemIconId(int type)
	{
		if (rarityItemDict == null)
		{
			BuildRarityItemDict();
		}
		if (rarityItemDict.TryGetValue(type, out var value) && value.Count > 0)
		{
			int index = Random.Range(0, value.Count);
			return value[index];
		}
		return 1001;
	}

	private void BuildRarityItemDict()
	{
		rarityItemDict = new Dictionary<int, List<int>>();
		for (int i = 1; i <= 4; i++)
		{
			rarityItemDict[i] = new List<int>();
		}
		List<auctionItem> list = ConfigManager.Instance?.AllTables?.TbauctionItem?.DataList;
		if (list == null)
		{
			return;
		}
		foreach (auctionItem item in list)
		{
			if (item == null || item.Rarity == null)
			{
				continue;
			}
			foreach (int item2 in item.Rarity)
			{
				if (rarityItemDict.ContainsKey(item2))
				{
					rarityItemDict[item2].Add(item.Id);
				}
			}
		}
	}

	public bool GetAuctionIsOpen()
	{
		return auctionIsOpen;
	}

	public void SetAuctionOpen(bool isOpen)
	{
		auctionIsOpen = isOpen;
	}

	public float GetCurrentLevelWithProgress()
	{
		int num = GameDataManager.Instance?.GetCurrentMainMissionLevel() ?? 1;
		mainMission mainMission = MissionManager.Instance?.GetCurrentMission();
		if (mainMission == null)
		{
			return num;
		}
		double num2 = GameDataManager.Instance?.GetTotalGold() ?? 0.0;
		double mainMissionNum = mainMission.MainMissionNum;
		if (mainMissionNum <= 0.0)
		{
			return num;
		}
		float num3 = (float)(num2 / mainMissionNum);
		if (num3 > 1f)
		{
			num3 = 1f;
		}
		return (float)num + num3;
	}

	public int GetEnterAuctionCount()
	{
		return PlayerPrefs.GetInt("auctionCount", 0);
	}

	public void AddEnterAuctionCount()
	{
		int num = PlayerPrefs.GetInt("auctionCount", 0);
		num++;
		PlayerPrefs.SetInt("auctionCount", num);
	}
}

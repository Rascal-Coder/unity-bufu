using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;
using UnityEngine;
using cfg;

public class ConfigManager : MonoBehaviour
{
	private Tables allTables;

	public static bool isInitialized;

	private static ConfigManager _instance;

	public Tables AllTables => allTables;

	public static ConfigManager Instance
	{
		get
		{
			if ((Object)(object)_instance == (Object)null)
			{
				Debug.LogError((object)"ConfigManager not initialized!");
			}
			return _instance;
		}
	}

	public static bool HasInstance()
	{
		if ((Object)(object)_instance != (Object)null)
		{
			return isInitialized;
		}
		return false;
	}

	public void Initialize(Action<bool> onComplete = null)
	{
		_instance = this;
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		string gameConfDir = Path.Combine(Application.dataPath, "GameRes/Config");
		allTables = new Tables((string file) => JSON.Parse(File.ReadAllText(gameConfDir + "/" + file + ".json.txt")));
		onComplete?.Invoke(obj: true);
		isInitialized = true;
	}

	public IEnumerator InitializeAsync(Action<bool> onComplete = null)
	{
		_instance = this;
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		Dictionary<string, string> configFiles = new Dictionary<string, string>();
		bool configBundleLoaded = false;
		yield return SingletonMono<ResManager>.Instance.LoadAllConfigCoroutine("Assets/GameRes/Config/tbauction", delegate(Dictionary<string, TextAsset> allConfig)
		{
			if (allConfig != null)
			{
				foreach (KeyValuePair<string, TextAsset> item in allConfig)
				{
					if ((Object)(object)item.Value != (Object)null)
					{
						configFiles[item.Key] = item.Value.text;
					}
				}
				configBundleLoaded = configFiles.Count > 0;
			}
		}, ".json.txt");
		if (!configBundleLoaded)
		{
			Debug.LogError((object)"Failed to load the config asset bundle.");
			onComplete?.Invoke(obj: false);
			yield break;
		}
		allTables = new Tables(delegate(string file)
		{
			if (configFiles.TryGetValue(file, out var value))
			{
				return JSON.Parse(value);
			}
			Debug.LogError((object)("Config file not loaded: " + file));
			return (JSONNode)null;
		});
		if (allTables != null)
		{
			isInitialized = true;
			onComplete?.Invoke(obj: true);
		}
		else
		{
			Debug.LogError((object)"Failed to initialize config tables");
			onComplete?.Invoke(obj: false);
		}
	}

	public Common GetCommonConfig(int id)
	{
		return allTables.TbCommon.GetOrDefault(id);
	}

	public float GetCommonFactor(int id)
	{
		return GetCommonConfig(id)?.Factor ?? 0f;
	}

	public itemUpgrade1 GetUpgrade1Config(int id)
	{
		return allTables.TbitemUpgrade1.GetOrDefault(id);
	}

	public itemUpgrade2 GetUpgrade2Config(int id)
	{
		return allTables.TbitemUpgrade2.GetOrDefault(id);
	}

	public itemUpgrade3 GetUpgrade3Config(int id)
	{
		return allTables.TbitemUpgrade3.GetOrDefault(id);
	}

	public List<itemUpgrade2> GetAllUpgrade2()
	{
		return allTables.TbitemUpgrade2.DataList;
	}

	public List<itemUpgrade3> GetAllUpgrade3()
	{
		return allTables.TbitemUpgrade3.DataList;
	}

	public List<itemUpgrade1> GetAllUpgrade1ByWorld(int worldId)
	{
		return allTables.TbitemUpgrade1.DataList.FindAll((itemUpgrade1 config) => config.WorldId == worldId);
	}

	public List<itemUpgrade2> GetAllUpgrade2ByWorld(int worldId)
	{
		return allTables.TbitemUpgrade2.DataList.FindAll((itemUpgrade2 config) => config.WorldId == worldId);
	}

	public List<itemUpgrade3> GetAllUpgrade3ByWorld(int worldId)
	{
		return allTables.TbitemUpgrade3.DataList.FindAll((itemUpgrade3 config) => config.WorldId == worldId);
	}

	public world GetWorldConfig(int worldId)
	{
		return allTables.Tbworld.GetOrDefault(worldId);
	}

	public List<world> GetAllWorldConfigs()
	{
		return allTables.Tbworld.DataList;
	}

	public List<talent2> GetAllDiceTalents()
	{
		return allTables.Tbtalent2.DataList;
	}

	public int GetUpgradeMaxLevel(int itemId, string configType)
	{
		if (configType == "itemUpgrade1")
		{
			itemUpgrade1 upgrade1Config = GetUpgrade1Config(itemId);
			if (upgrade1Config == null || upgrade1Config.UpgradeLvMax != -1)
			{
				return upgrade1Config?.UpgradeLvMax ?? 0;
			}
			return 999999;
		}
		if (configType == "itemUpgrade2")
		{
			itemUpgrade2 upgrade2Config = GetUpgrade2Config(itemId);
			if (upgrade2Config == null || upgrade2Config.UpgradeLvMax != -1)
			{
				return upgrade2Config?.UpgradeLvMax ?? 0;
			}
			return 999999;
		}
		itemUpgrade3 upgrade3Config = GetUpgrade3Config(itemId);
		if (upgrade3Config == null || upgrade3Config.UpgradeLvMax != -1)
		{
			return upgrade3Config?.UpgradeLvMax ?? 0;
		}
		return 999999;
	}

	public string GetUpgradeName(int itemId)
	{
		itemUpgrade1 upgrade1Config = GetUpgrade1Config(itemId);
		if (upgrade1Config != null)
		{
			return upgrade1Config.ItemName;
		}
		itemUpgrade2 upgrade2Config = GetUpgrade2Config(itemId);
		if (upgrade2Config != null)
		{
			return upgrade2Config.ItemName;
		}
		return "未知升级";
	}

	public string GetUpgradeDesc(int itemId)
	{
		itemUpgrade1 upgrade1Config = GetUpgrade1Config(itemId);
		if (upgrade1Config != null)
		{
			return upgrade1Config.ItemDesc;
		}
		itemUpgrade2 upgrade2Config = GetUpgrade2Config(itemId);
		if (upgrade2Config != null)
		{
			return upgrade2Config.ItemDesc;
		}
		return "";
	}

	public float GetUpgrade2Result(int itemId)
	{
		return GetUpgrade2Config(itemId)?.UpgradeResult ?? 1f;
	}

	public float GetUpgrade3Result(int itemId)
	{
		return GetUpgrade3Config(itemId)?.UpgradeResult ?? 1f;
	}

	public talent GetCoinTalentConfig(int id)
	{
		return allTables.Tbtalent.GetOrDefault(id);
	}

	public List<talent> GetAllCoinTalentItem()
	{
		return allTables.Tbtalent.DataList;
	}

	public talentPoint GetTalentPointConfig(int id)
	{
		return allTables.TbtalentPoint.GetOrDefault(id);
	}

	public List<talentPoint> GetAllTalentPoint()
	{
		return allTables.TbtalentPoint.DataList;
	}

	public List<talentPoint2> GetAllTalentPoint2()
	{
		return allTables.TbtalentPoint2.DataList;
	}

	public talent2 GetDiceTalentConfig(int id)
	{
		return allTables.Tbtalent2.GetOrDefault(id);
	}

	public List<talent2> GetAllDiceTalentItem()
	{
		return allTables.Tbtalent2.DataList;
	}

	public talentPoint GetDiceTalentPoint(int id)
	{
		return allTables.TbtalentPoint.GetOrDefault(id);
	}

	public List<talentPoint> GetAllDiceTalentPoint()
	{
		return allTables.TbtalentPoint.DataList;
	}

	public List<item> GetAllItem()
	{
		return allTables.Tbitem.DataList;
	}

	public item GetItem(int id)
	{
		return allTables.Tbitem.GetOrDefault(id);
	}

	public List<signReward> GetAllSignReward()
	{
		return allTables.TbsignReward.DataList;
	}

	public List<signReward> GetAllSignRewardByWorld(int worldId)
	{
		List<signReward> list = allTables.TbsignReward.DataList.FindAll((signReward config) => config != null && config.WorldId == worldId);
		list.Sort((signReward a, signReward b) => a.Id.CompareTo(b.Id));
		return list;
	}

	public signReward GetSignRewardItem(int id)
	{
		return allTables.TbsignReward.GetOrDefault(id);
	}

	public signReward GetSignRewardItemByWorldDay(int worldId, int day)
	{
		if (day <= 0)
		{
			return null;
		}
		List<signReward> allSignRewardByWorld = GetAllSignRewardByWorld(worldId);
		if (allSignRewardByWorld != null && day <= allSignRewardByWorld.Count)
		{
			return allSignRewardByWorld[day - 1];
		}
		int id = day + Mathf.Max(0, worldId - 1) * 100;
		signReward signRewardItem = GetSignRewardItem(id);
		if (signRewardItem != null && signRewardItem.WorldId == worldId)
		{
			return signRewardItem;
		}
		return GetSignRewardItem(day);
	}

	public List<desk> GetAllDesk()
	{
		return allTables.Tbdesk.DataList;
	}

	public List<desk> GetAllDeskByWorld(int worldId)
	{
		return allTables.Tbdesk.DataList.FindAll((desk config) => config.WorldId == worldId);
	}

	public desk GetDeskConfig(int id)
	{
		return allTables.Tbdesk.GetOrDefault(id);
	}

	public List<helperRole> GetAllHelperRole()
	{
		return allTables.TbhelperRole.DataList;
	}

	public helperRole GetHelperRoleConfig(int id)
	{
		return allTables.TbhelperRole.GetOrDefault(id);
	}

	public float GetHelperFactor(int id)
	{
		return allTables.TbhelperRole.GetOrDefault(id)?.SkillFactor1 ?? 0f;
	}

	public float GetHelperCD(int id)
	{
		return allTables.TbhelperRole.GetOrDefault(id)?.CdTime ?? 0f;
	}

	public List<mainMission> GetAllMainMission()
	{
		return allTables.TbmainMission.DataList;
	}

	public List<mainMission> GetAllMainMissionByWorld(int worldId)
	{
		return allTables.TbmainMission.DataList.FindAll((mainMission config) => config.WorldId == worldId);
	}

	public shop GetShopConfig(int id)
	{
		return allTables.Tbshop.GetOrDefault(id);
	}

	public List<shop> GetAllShop()
	{
		return allTables.Tbshop.DataList;
	}

	public List<limitMission> GetAllLimitMission()
	{
		return allTables.TblimitMission.DataList;
	}

	public List<challenge> GetAllChallenge()
	{
		return allTables.Tbchallenge.DataList;
	}

	public challenge GetChallengeCofig(int id)
	{
		return allTables.Tbchallenge.GetOrDefault(id);
	}

	public limitMission GetLimitMission(int id)
	{
		return allTables.TblimitMission.GetOrDefault(id);
	}

	public int TransferSuipianRealId(int id)
	{
		int num = -1;
		switch (id)
		{
		case 3:
			num = 1;
			break;
		case 4:
			num = 2;
			break;
		case 5:
			num = 3;
			break;
		}
		if (num != -1)
		{
			List<int> list = new List<int>();
			foreach (helperRole data in allTables.TbhelperRole.DataList)
			{
				if (data.Rare == num)
				{
					list.Add(data.Id + 100);
				}
			}
			if (list.Count > 0)
			{
				int index = Random.Range(0, list.Count);
				return list[index];
			}
		}
		return id;
	}

	public List<signWorldCup> GetAllSignWorld()
	{
		return allTables.TbsignWorldCup.DataList;
	}

	public signWorldCup GetOneSignWorld(int id)
	{
		return allTables.TbsignWorldCup.GetOrDefault(id);
	}

	public List<WorldCupMisson> GetAllWorldCupMisson()
	{
		return allTables.TbWorldCupMisson.DataList;
	}

	public WorldCupMisson GetOneWorldCupMisson(int id)
	{
		return allTables.TbWorldCupMisson.GetOrDefault(id);
	}

	public List<WorldUpScoreReward> GetAllWorldCupScoreReward()
	{
		return allTables.TbWorldUpScoreReward.DataList;
	}

	public WorldUpScoreReward GetWorldCupScoreReward(int id)
	{
		return allTables.TbWorldUpScoreReward.GetOrDefault(id);
	}

	public List<auction> GetAllAuction()
	{
		return allTables.Tbauction.DataList;
	}

	public auction GetAuctionConfig(int id)
	{
		return allTables.Tbauction.GetOrDefault(id);
	}

	public List<auctionItem> GetAllAuctionItems()
	{
		return allTables.TbauctionItem.DataList;
	}

	public auctionItem GetAuctionItems(int id)
	{
		return allTables.TbauctionItem.GetOrDefault(id);
	}
}

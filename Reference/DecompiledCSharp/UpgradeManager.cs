using System;
using System.Collections.Generic;
using Module.Http;
using Module.Mission;
using Module.Role;
using UnityEngine;
using cfg;

public class UpgradeManager : SingletonMono<UpgradeManager>
{
	private struct GoldIncomeSample
	{
		public float Time;

		public double Amount;
	}

	private Dictionary<int, int> _upgradeLevels = new Dictionary<int, int>();

	private List<int> _unlockedUpgradeItems = new List<int>();

	private readonly Dictionary<int, CurrencyEvolutionData> _currencyEvolutionItems = new Dictionary<int, CurrencyEvolutionData>();

	private readonly Dictionary<int, int> _lastEvolutionFrame = new Dictionary<int, int>();

	private double _totalGold;

	private const float GoldIncomeWindowSeconds = 30f;

	private readonly Queue<GoldIncomeSample> _goldIncomeSamples = new Queue<GoldIncomeSample>();

	private double _goldIncomeSumInWindow;

	private Dictionary<int, Common> _commonConfigCache = new Dictionary<int, Common>();

	private bool _isDirty;

	private bool _saveScheduled;

	private float _saveDelayTimer;

	private readonly float[] _saveIntervals = new float[3] { 10f, 30f, 60f };

	private int _saveIntervalIndex;

	private bool _isInitialized;

	private MissionManager _subscribedMissionManager;

	private GameDataManager _subscribedGameDataManager;

	private float _invitePollTimer;

	private const float INVITE_POLL_INTERVAL = 30f;

	public static bool EnableCurrencyEvolution = true;

	public double TotalGold => _totalGold;

	public bool IsInitialized => _isInitialized;

	public override bool DontDestory => true;

	public event Action<int, int> OnUpgradeSuccess;

	public event Action<int, int, string> OnTypedUpgradeSuccess;

	public event Action<double> OnGoldChanged;

	public event Action OnCoinCountChanged;

	public event Action<double> CurrentGoldGain;

	public int GetUpgradeLevel(int itemId)
	{
		if (_upgradeLevels.ContainsKey(itemId))
		{
			return _upgradeLevels[itemId];
		}
		return 0;
	}

	public int GetUpgradeMaxLevel(int itemId)
	{
		itemUpgrade1 upgrade1Config = ConfigManager.Instance.GetUpgrade1Config(itemId);
		if (upgrade1Config != null)
		{
			if (upgrade1Config.UpgradeLvMax != -1)
			{
				return upgrade1Config.UpgradeLvMax;
			}
			return 999999;
		}
		itemUpgrade2 upgrade2Config = ConfigManager.Instance.GetUpgrade2Config(itemId);
		if (upgrade2Config != null)
		{
			if (upgrade2Config.UpgradeLvMax != -1)
			{
				return upgrade2Config.UpgradeLvMax;
			}
			return 999999;
		}
		return 0;
	}

	public int GetUpgradeCostType(int itemId)
	{
		return ConfigManager.Instance.GetUpgrade1Config(itemId)?.CostType ?? ConfigManager.Instance.GetUpgrade2Config(itemId)?.CostType ?? 1;
	}

	public bool IsUnlocked(int itemId)
	{
		if (_unlockedUpgradeItems.Contains(itemId))
		{
			return true;
		}
		return CheckUnlockCondition(itemId);
	}

	public int GetUpgradeLevel(int itemId, string configType)
	{
		int storageKey = GetStorageKey(itemId, configType);
		if (_upgradeLevels.ContainsKey(storageKey))
		{
			return _upgradeLevels[storageKey];
		}
		return 0;
	}

	public float GetHelperExtraBuff(int helperId)
	{
		int helperLevel = GameDataManager.Instance.GetHelperLevel(helperId);
		helperRole helperRoleConfig = ConfigManager.Instance.GetHelperRoleConfig(helperId);
		if (helperRoleConfig == null)
		{
			Debug.LogError((object)$"HelperRoleConfig 未找到 id={helperId}");
			return 0f;
		}
		int helperSkillLevel = Constants.GetHelperSkillLevel(helperRoleConfig, helperLevel);
		return helperRoleConfig.BaseSkillNum + (float)helperSkillLevel * helperRoleConfig.UpgradeSkillNum;
	}

	public float GetHelperIncomeMultiplier(int helperId)
	{
		if (helperId <= 0)
		{
			return 1f;
		}
		int helperLevel = GameDataManager.Instance.GetHelperLevel(helperId);
		if (helperLevel <= 1)
		{
			return 1f;
		}
		helperRole helperRoleConfig = ConfigManager.Instance.GetHelperRoleConfig(helperId);
		if (helperRoleConfig == null)
		{
			return 1f;
		}
		return Mathf.Pow(helperRoleConfig.HelperIncomeAddBuff, (float)(helperLevel - 1));
	}

	public bool IsUnlocked(int itemId, string configType)
	{
		int storageKey = GetStorageKey(itemId, configType);
		if (_unlockedUpgradeItems.Contains(storageKey))
		{
			return true;
		}
		return CheckUnlockConditionWithType(itemId, configType);
	}

	public double GetUpgradeCost(int itemId, string configType)
	{
		int upgradeLevel = GetUpgradeLevel(itemId, configType);
		switch (configType)
		{
		case "itemUpgrade1":
		{
			itemUpgrade1 upgrade1Config = ConfigManager.Instance.GetUpgrade1Config(itemId);
			if (upgrade1Config != null)
			{
				if (upgrade1Config.CostMoneyBuff.Count != upgrade1Config.CostMoneyLevel.Count)
				{
					Debug.LogError((object)$"Upgrade1Config  id={itemId} 的 CostMoneyBuff 和 CostMoneyLevel 数量不一致");
				}
				if (upgradeLevel < upgrade1Config.BaseCostMoney.Count)
				{
					return upgrade1Config.BaseCostMoney[upgradeLevel];
				}
				return CalculateCostMoney(upgradeLevel, upgrade1Config.BaseCostMoney, upgrade1Config.CostMoneyLevel, upgrade1Config.CostMoneyBuff);
			}
			break;
		}
		case "itemUpgrade2":
		{
			itemUpgrade2 upgrade2Config = ConfigManager.Instance.GetUpgrade2Config(itemId);
			if (upgrade2Config != null)
			{
				if (upgrade2Config.CostMoneyBuff.Count != upgrade2Config.CostMoneyLevel.Count)
				{
					Debug.LogError((object)$"Upgrade2Config  id={itemId} 的 CostMoneyBuff 和 CostMoneyLevel 数量不一致");
				}
				return CalculateCostMoney(upgradeLevel, upgrade2Config.BaseCostMoney, upgrade2Config.CostMoneyLevel, upgrade2Config.CostMoneyBuff);
			}
			break;
		}
		case "itemUpgrade3":
		{
			itemUpgrade3 upgrade3Config = ConfigManager.Instance.GetUpgrade3Config(itemId);
			if (upgrade3Config != null)
			{
				return CalculateCostMoney(upgradeLevel, upgrade3Config.BaseCostMoney, upgrade3Config.CostMoneyLevel, upgrade3Config.CostMoneyBuff);
			}
			break;
		}
		}
		return 0.0;
	}

	public double CalculateCostMoney(int currentLevel, List<int> baseCostMoney, List<int> costMoneyLevel, List<float> costMoneyBuff)
	{
		if (currentLevel < 0)
		{
			return 0.0;
		}
		if (baseCostMoney == null || baseCostMoney.Count == 0)
		{
			return 0.0;
		}
		double num = baseCostMoney[baseCostMoney.Count - 1];
		for (int i = baseCostMoney.Count; i <= currentLevel; i++)
		{
			int num2 = GetBuffIndex(i, costMoneyLevel);
			if (num2 >= costMoneyBuff.Count)
			{
				num2 = costMoneyBuff.Count - 1;
			}
			double num3 = costMoneyBuff[num2];
			num *= num3;
		}
		return num;
	}

	public double CalculateCostMoney(int currentLevel, List<float> baseCostMoney, List<int> costMoneyLevel, List<float> costMoneyBuff)
	{
		if (currentLevel < 0 || baseCostMoney == null || baseCostMoney.Count == 0)
		{
			return 0.0;
		}
		if (currentLevel < baseCostMoney.Count)
		{
			return baseCostMoney[currentLevel];
		}
		double num = baseCostMoney[baseCostMoney.Count - 1];
		for (int i = baseCostMoney.Count; i <= currentLevel; i++)
		{
			int num2 = Mathf.Min(GetBuffIndex(i, costMoneyLevel), costMoneyBuff.Count - 1);
			if (num2 >= 0)
			{
				num *= (double)costMoneyBuff[num2];
			}
		}
		return num;
	}

	private int GetBuffIndex(int level, List<int> costMoneyLevel)
	{
		if (costMoneyLevel == null || costMoneyLevel.Count == 0)
		{
			return 0;
		}
		for (int i = 0; i < costMoneyLevel.Count; i++)
		{
			if (level < costMoneyLevel[i])
			{
				return i;
			}
		}
		return costMoneyLevel.Count;
	}

	public int GetUpgradeCostType(int itemId, string configType)
	{
		if (configType == "itemUpgrade1")
		{
			return ConfigManager.Instance.GetUpgrade1Config(itemId)?.CostType ?? 1;
		}
		if (configType == "itemUpgrade2")
		{
			return ConfigManager.Instance.GetUpgrade2Config(itemId)?.CostType ?? 1;
		}
		return ConfigManager.Instance.GetUpgrade3Config(itemId)?.CostType ?? 1;
	}

	public bool TryUpgrade(int itemId, string configType)
	{
		if (!IsUnlocked(itemId, configType))
		{
			Debug.LogWarning((object)$"[{configType}] 升级项 {itemId} 未解锁");
			return false;
		}
		int upgradeLevel = GetUpgradeLevel(itemId, configType);
		if (configType == "itemUpgrade1" && CanEvolveCurrency(itemId))
		{
			Debug.LogWarning((object)$"[{configType}] 升级项 {itemId} 有待完成的进化，不能执行普通升级");
			return false;
		}
		int num = 0;
		num = ConfigManager.Instance.GetUpgradeMaxLevel(itemId, configType);
		int num2 = GameDataManager.Instance?.GetCurrentWorldId() ?? 1;
		if (configType == "itemUpgrade2")
		{
			if (!IsCurrentDiceWorld() && TalentDataManager.Instance != null)
			{
				if (itemId <= 3 || itemId == 14)
				{
					num = ConfigManager.Instance.GetUpgradeMaxLevel(itemId, configType) + TalentDataManager.Instance.GetMaxLevelUp();
				}
			}
			else if (IsCurrentDiceWorld() && TalentDataManager.Instance != null && ((itemId >= 101 && itemId <= 103) || itemId == 110))
			{
				num = ConfigManager.Instance.GetUpgradeMaxLevel(itemId, configType) + (int)TalentDataManager.Instance.GetDiceTalentBuff(301);
			}
		}
		else if (configType == "itemUpgrade1")
		{
			if (itemId == 1 || itemId == 101)
			{
				switch (num2)
				{
				case 1:
					num = ConfigManager.Instance.GetUpgradeMaxLevel(itemId, configType) + GameDataManager.Instance.GetTalentJingYing();
					break;
				case 2:
					num = ConfigManager.Instance.GetUpgradeMaxLevel(itemId, configType) + (int)TalentDataManager.Instance.GetDiceTalentBuff(201);
					break;
				}
			}
			else
			{
				num = ConfigManager.Instance.GetUpgradeMaxLevel(itemId, configType);
			}
		}
		if (upgradeLevel >= num)
		{
			Debug.LogWarning((object)$"[{configType}] 升级项 {itemId} 已满级");
			return false;
		}
		double num3 = GetUpgradeCost(itemId, configType);
		int upgradeCostType = GetUpgradeCostType(itemId, configType);
		if (configType == "itemUpgrade1" && (itemId == 1 || itemId == 101) && (Object)(object)GameDataManager.Instance != (Object)null)
		{
			float num4 = 1f;
			switch (num2)
			{
			case 1:
				num4 = GameDataManager.Instance.GetLaoDongLi();
				break;
			case 2:
				num4 = TalentDataManager.Instance.GetDiceTalentBuff(213);
				break;
			}
			if (num4 > 0f)
			{
				num3 *= (double)num4;
			}
		}
		if (upgradeCostType == 1 && num2 == 2)
		{
			num3 *= (double)TalentDataManager.Instance.GetDiceTalentBuff(105);
		}
		switch (upgradeCostType)
		{
		case 2:
			if ((double)GameDataManager.Instance.GetDiamond() < num3)
			{
				Debug.LogWarning((object)$"钻石不足，需要 {num3}，当前 {GameDataManager.Instance.GetDiamond()}");
				return false;
			}
			GameDataManager.Instance.RemoveDiamond((long)Math.Ceiling(num3));
			break;
		case 3:
			Debug.Log((object)$"[{configType}] 升级项 {itemId} 为广告升级，跳过资源扣除");
			break;
		default:
			if (!HasEnoughGold(num3))
			{
				Debug.LogWarning((object)$"金钱不足，需要 {num3}，当前 {_totalGold}");
				return false;
			}
			TrySpendGold(num3);
			break;
		}
		int storageKey = GetStorageKey(itemId, configType);
		if (!_upgradeLevels.ContainsKey(storageKey))
		{
			_upgradeLevels[storageKey] = 0;
		}
		_upgradeLevels[storageKey]++;
		if (!_unlockedUpgradeItems.Contains(storageKey))
		{
			_unlockedUpgradeItems.Add(storageKey);
		}
		RealSaveData();
		this.OnUpgradeSuccess?.Invoke(itemId, _upgradeLevels[storageKey]);
		this.OnTypedUpgradeSuccess?.Invoke(itemId, _upgradeLevels[storageKey], configType);
		SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/upgrade.mp3");
		Debug.Log((object)$"[{configType}] 升级成功：{itemId}, 当前等级：{_upgradeLevels[storageKey]}");
		HandleSpecialUpgradeEffects(itemId, configType);
		return true;
	}

	public bool ForceUpgradeLevel(int itemId, string configType)
	{
		string text = "itemUpgrade1";
		int num = 1;
		if (!IsUnlocked(num, text))
		{
			Debug.LogWarning((object)"帮手升级项未解锁，无法免费升级");
			return false;
		}
		int upgradeLevel = GetUpgradeLevel(num, text);
		int num2 = ConfigManager.Instance.GetUpgradeMaxLevel(num, text);
		switch (GameDataManager.Instance?.GetCurrentWorldId() ?? 1)
		{
		case 1:
			num2 += GameDataManager.Instance.GetTalentJingYing();
			break;
		case 2:
			num2 += (int)TalentDataManager.Instance.GetDiceTalentBuff(201);
			break;
		}
		if (upgradeLevel >= num2)
		{
			Debug.LogWarning((object)$"帮手已满级（{upgradeLevel}/{num2}），无法免费升级");
			return false;
		}
		int storageKey = GetStorageKey(num, text);
		if (!_upgradeLevels.ContainsKey(storageKey))
		{
			_upgradeLevels[storageKey] = 0;
		}
		_upgradeLevels[storageKey]++;
		RealSaveData();
		this.OnUpgradeSuccess?.Invoke(num, _upgradeLevels[storageKey]);
		this.OnTypedUpgradeSuccess?.Invoke(num, _upgradeLevels[storageKey], text);
		SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/upgrade.mp3");
		Debug.Log((object)$"帮手广告免费升级：{_upgradeLevels[storageKey]}/{num2}");
		HandleSpecialUpgradeEffects(num, text);
		return true;
	}

	private int GetStorageKey(int itemId, string configType)
	{
		if (configType == "itemUpgrade2")
		{
			return itemId + 1000;
		}
		if (!(configType == "itemUpgrade3"))
		{
			return itemId;
		}
		return itemId + 2000;
	}

	private void HandleSpecialUpgradeEffects(int itemId, string configType)
	{
		if (configType != "itemUpgrade1")
		{
			return;
		}
		if (itemId == Constants.GetCurrentWorldHelperUpgradeId())
		{
			if ((Object)(object)UIManager.Instance != (Object)null && UIManager.Instance.IsVatMainSceneActive)
			{
				UIManager.Instance.MainSceneVat?.SpawnRole(delegate(RoleVat role)
				{
					if ((Object)(object)role != (Object)null)
					{
						Debug.Log((object)"[UpgradeManager] VAT role spawned after helper upgrade.");
					}
				});
				return;
			}
			SingletonMono<RoleManager>.Instance?.SpawnRole(delegate(Role role)
			{
				if ((Object)(object)role != (Object)null)
				{
					Debug.Log((object)"[UpgradeManager] 升级帮手成功，新帮手已生成");
				}
			});
		}
		if (IsCurrentDiceWorld() && (Object)(object)UIManager.Instance != (Object)null && (Object)(object)UIManager.Instance.MainSceneVat != (Object)null)
		{
			int upgradeLevel = GetUpgradeLevel(itemId, "itemUpgrade1");
			bool flag = IsEvolutionLevel(itemId, upgradeLevel);
			UIManager.Instance.MainSceneVat.TryRefreshDicePopulationForSpawnUpgrade(itemId, flag ? new bool?(false) : ((bool?)null));
		}
		if (itemId >= 2 && itemId <= 5)
		{
			int upgradeLevel2 = GetUpgradeLevel(itemId, "itemUpgrade1");
			bool flag2 = IsEvolutionLevel(itemId, upgradeLevel2);
			SpawnCoinForUpgrade(itemId, flag2 ? new bool?(false) : ((bool?)null));
		}
	}

	private bool CheckUnlockConditionWithType(int itemId, string configType)
	{
		int num = 0;
		int num2 = -1;
		if (configType == "itemUpgrade1")
		{
			itemUpgrade1 upgrade1Config = ConfigManager.Instance.GetUpgrade1Config(itemId);
			num = upgrade1Config?.UnLockCondition ?? 0;
			num2 = upgrade1Config?.ConditionIconType ?? (-1);
			if (GameDataManager.Instance.GetCurrentWorldId() == 1 && itemId == 3)
			{
				num += GetInitialCopperCount();
			}
			if (GameDataManager.Instance.GetCurrentWorldId() == 2 && itemId == 103)
			{
				num += GetInitialDiceCount();
			}
		}
		else if (configType == "itemUpgrade2")
		{
			itemUpgrade2 upgrade2Config = ConfigManager.Instance.GetUpgrade2Config(itemId);
			num = upgrade2Config?.UnLockCondition ?? 0;
			num2 = upgrade2Config?.ConditionIconType ?? (-1);
		}
		else
		{
			itemUpgrade3 upgrade3Config = ConfigManager.Instance.GetUpgrade3Config(itemId);
			num = upgrade3Config?.UnLockCondition ?? 0;
			num2 = upgrade3Config?.ConditionIconType ?? (-1);
		}
		return CheckUnlockConditionImpl(num, itemId, configType, num2);
	}

	protected override void OnAwake()
	{
		Debug.Log((object)"UpgradeManager Awake 完成，等待 LateInitialize...");
	}

	private void Start()
	{
		if (!_isInitialized)
		{
			Debug.LogWarning((object)"UpgradeManager 未通过 LateInitialize 初始化，使用默认数据");
			_totalGold = 0.0;
			_upgradeLevels = new Dictionary<int, int>();
			_unlockedUpgradeItems = new List<int>();
		}
	}

	public void ResetData()
	{
		_totalGold = 0.0;
		_upgradeLevels.Clear();
		_unlockedUpgradeItems.Clear();
		_currencyEvolutionItems.Clear();
		_lastEvolutionFrame.Clear();
		GameDataManager.Instance.ResetUpgradeData();
		_isInitialized = true;
		Debug.Log((object)"UpgradeManager 数据已重置");
	}

	private void InitializeConfigCache()
	{
		if ((Object)(object)ConfigManager.Instance == (Object)null)
		{
			Debug.LogWarning((object)"ConfigManager 未初始化，延迟配置表缓存初始化");
		}
	}

	public void LateInitialize()
	{
		Debug.Log((object)"UpgradeManager 开始延迟初始化...");
		InitializeConfigCache();
		SubscribeDeskUnlockEvents();
		LoadData();
		InitializeCommon();
		_isInitialized = true;
		Debug.Log((object)"UpgradeManager 延迟初始化完成");
	}

	private void SubscribeDeskUnlockEvents()
	{
		if (!((Object)(object)_subscribedMissionManager != (Object)null) && !((Object)(object)_subscribedGameDataManager != (Object)null))
		{
			_subscribedMissionManager = MissionManager.Instance;
			_subscribedGameDataManager = GameDataManager.Instance;
			if ((Object)(object)_subscribedMissionManager == (Object)null || (Object)(object)_subscribedGameDataManager == (Object)null)
			{
				_subscribedMissionManager = null;
				_subscribedGameDataManager = null;
			}
			else
			{
				_subscribedMissionManager.OnMissionUpdated += OnMissionUpdated;
				_subscribedGameDataManager.OnWorldChanged += OnWorldChanged;
			}
		}
	}

	private void OnMissionUpdated(mainMission mission)
	{
		SyncCurrentWorldDeskUnlocks();
	}

	private void OnWorldChanged(int previousWorldId, int currentWorldId)
	{
		SyncCurrentWorldDeskUnlocks();
	}

	private void SyncCurrentWorldDeskUnlocks()
	{
		if ((Object)(object)GameDataManager.Instance == (Object)null || !ConfigManager.HasInstance())
		{
			return;
		}
		int currentWorldId = GameDataManager.Instance.GetCurrentWorldId();
		int currentMainMissionLevel = GameDataManager.Instance.GetCurrentMainMissionLevel();
		List<desk> allDeskByWorld = ConfigManager.Instance.GetAllDeskByWorld(currentWorldId);
		if (allDeskByWorld != null)
		{
			foreach (desk item in allDeskByWorld)
			{
				if (item != null && (item.DeskUnlockCondition == -1 || currentMainMissionLevel >= item.DeskUnlockCondition) && !GameDataManager.Instance.IsDeskOwned(item.Id))
				{
					GameDataManager.Instance.SetDeskOwned(item.Id, isOwned: true);
				}
			}
		}
		RebuildCurrentWorldDeskBuffs();
	}

	private void InitializeCommon()
	{
		float commonFactor = ConfigManager.Instance.GetCommonFactor(16);
		SingletonMono<BuffManager>.Instance.AddBuff(BuffType.CriticalRate, commonFactor, 2.1474836E+09f);
		float commonFactor2 = ConfigManager.Instance.GetCommonFactor(17);
		SingletonMono<BuffManager>.Instance.AddBuff(BuffType.CriticalDamage, commonFactor2, 2.1474836E+09f);
		SingletonMono<BuffManager>.Instance?.AddBuff(BuffType.Income, 0.0001f, float.PositiveInfinity);
		SyncCurrentWorldDeskUnlocks();
		if (GameDataManager.Instance.GetHelperAwakeSkillLevel(7) > 2 && GameDataManager.Instance.GetHelperCount(7) > 0)
		{
			SingletonMono<BuffManager>.Instance.AddBuff(BuffType.CriticalRate, 0.2f, 2.1474836E+09f);
		}
		if ((Object)(object)ConfigManager.Instance != (Object)null && (Object)(object)GameDataManager.Instance != (Object)null && GameDataManager.Instance.GetGameData() != null)
		{
			long num = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
			long helpedTime = GameDataManager.Instance.GetHelpedTime();
			long num2 = 86400000L;
			if (helpedTime > 0 && num - helpedTime < num2)
			{
				float commonFactor3 = ConfigManager.Instance.GetCommonFactor(26);
				SingletonMono<BuffManager>.Instance.AddBuff(BuffType.Income, commonFactor3, 2.1474836E+09f, "InviteHelpedBuff");
			}
		}
	}

	public void LoadData()
	{
		UpgradeData upgradeData = GameDataManager.Instance?.GetUpgradeData();
		if (upgradeData == null)
		{
			Debug.LogError((object)"UpgradeData 为空，无法加载升级数据，将使用默认值");
			return;
		}
		_upgradeLevels.Clear();
		foreach (UpgradeLevelData upgradeItemLevel in upgradeData.upgradeItemLevels)
		{
			_upgradeLevels[upgradeItemLevel.itemId] = upgradeItemLevel.level;
		}
		_unlockedUpgradeItems = new List<int>(upgradeData.unlockedUpgradeItems);
		_currencyEvolutionItems.Clear();
		_lastEvolutionFrame.Clear();
		if (upgradeData.currencyEvolutionItems != null)
		{
			foreach (CurrencyEvolutionData currencyEvolutionItem in upgradeData.currencyEvolutionItems)
			{
				int num = ((currencyEvolutionItem != null) ? GetUpgradeLevel(currencyEvolutionItem.itemId, "itemUpgrade1") : 0);
				int num2 = ((currencyEvolutionItem != null) ? (ConfigManager.Instance?.GetUpgrade1Config(currencyEvolutionItem.itemId)?.EvolutionLevel?.Count).GetValueOrDefault() : 0);
				if (currencyEvolutionItem != null && IsEvolvableCurrencyUpgrade(currencyEvolutionItem.itemId) && currencyEvolutionItem.tier > 0 && currencyEvolutionItem.completedAtLevel > 0 && currencyEvolutionItem.completedAtLevel <= num && num2 > 0)
				{
					_currencyEvolutionItems[currencyEvolutionItem.itemId] = new CurrencyEvolutionData
					{
						itemId = currencyEvolutionItem.itemId,
						tier = Mathf.Min(currencyEvolutionItem.tier, num2),
						completedAtLevel = currencyEvolutionItem.completedAtLevel
					};
				}
			}
		}
		GameData gameData = GameDataManager.Instance?.GetGameData();
		if (gameData == null)
		{
			Debug.LogError((object)"GameData 为空，无法加载升级数据，将使用默认值");
			return;
		}
		GameDataManager.Instance.InitializeCoinDiamond();
		_totalGold = gameData.totalGold;
		EnsureGoldNotNegative();
		InitializeGoldIncomeFromSavedData(gameData);
		Debug.Log((object)$"升级数据加载完成，总金钱：{_totalGold}, 升级项数量：{_upgradeLevels.Count}");
		this.OnGoldChanged?.Invoke(_totalGold);
		NotifyCoinCountChanged();
	}

	public void RebuildCurrentWorldDeskBuffs()
	{
		if ((Object)(object)SingletonMono<BuffManager>.Instance == (Object)null || (Object)(object)GameDataManager.Instance == (Object)null || !ConfigManager.HasInstance())
		{
			return;
		}
		ClearDeskExtraBuffs();
		int currentWorldId = GameDataManager.Instance.GetCurrentWorldId();
		List<DeskInfo> allDeskInfo = GameDataManager.Instance.GetAllDeskInfo();
		if (allDeskInfo == null)
		{
			return;
		}
		foreach (DeskInfo item in allDeskInfo)
		{
			if (item != null)
			{
				desk deskConfig = ConfigManager.Instance.GetDeskConfig(item.deskId);
				if (deskConfig != null && deskConfig.WorldId == currentWorldId)
				{
					AddDeskExtraBuff(item.deskId);
				}
			}
		}
	}

	private static void ClearDeskExtraBuffs()
	{
		for (int i = 1; i <= 8; i++)
		{
			ClearDeskExtraBuff(i);
		}
		for (int j = 101; j <= 108; j++)
		{
			ClearDeskExtraBuff(j);
		}
	}

	private static void ClearDeskExtraBuff(int deskId)
	{
		if (TryGetDeskBuffType(deskId, out var buffType))
		{
			SingletonMono<BuffManager>.Instance.RemoveBuff(buffType, $"DeskExtraBuff_{deskId}");
		}
	}

	private static bool TryGetDeskBuffType(int deskId, out BuffType buffType)
	{
		switch (deskId)
		{
		case 1:
		case 4:
		case 101:
		case 104:
			buffType = BuffType.Copper;
			return true;
		case 2:
		case 102:
			buffType = BuffType.Manual;
			return true;
		case 3:
		case 7:
		case 103:
		case 107:
			buffType = BuffType.CriticalDamage;
			return true;
		case 5:
		case 105:
			buffType = BuffType.Silver;
			return true;
		case 6:
		case 106:
			buffType = BuffType.Gold;
			return true;
		case 8:
		case 108:
			buffType = BuffType.Income;
			return true;
		default:
			buffType = BuffType.Income;
			return false;
		}
	}

	private void InitializeGoldIncomeFromSavedData(GameData gameData)
	{
		_goldIncomeSamples.Clear();
		if (gameData.averageCoin > 0.0)
		{
			double num = (_goldIncomeSumInWindow = gameData.averageCoin * 0.5);
			for (int i = 0; i < 5; i++)
			{
				float time = Time.unscaledTime - 6f * (float)i;
				_goldIncomeSamples.Enqueue(new GoldIncomeSample
				{
					Time = time,
					Amount = num / 5.0
				});
			}
			Debug.Log((object)$"初始化收益统计：平均每分钟收益 {gameData.averageCoin}，30秒收益 {num}");
		}
		else
		{
			_goldIncomeSumInWindow = 0.0;
		}
	}

	private void SaveData()
	{
		_isDirty = true;
		if (!_saveScheduled)
		{
			_saveDelayTimer = _saveIntervals[Mathf.Min(_saveIntervalIndex, _saveIntervals.Length - 1)];
			_saveScheduled = true;
		}
	}

	public void ForceSave()
	{
		if (_isDirty || _saveScheduled)
		{
			RealSaveData();
			_isDirty = false;
			_saveScheduled = false;
			_saveDelayTimer = 0f;
		}
	}

	public void FlushToSaveData(bool clearDirtyState = false)
	{
		RealSaveData(writeFiles: false);
		if (clearDirtyState)
		{
			_isDirty = false;
			_saveScheduled = false;
			_saveDelayTimer = 0f;
		}
	}

	private void Update()
	{
		if (_saveScheduled)
		{
			_saveDelayTimer -= Time.unscaledDeltaTime;
			if (_saveDelayTimer <= 0f)
			{
				RealSaveData();
				_isDirty = false;
				_saveScheduled = false;
				_saveDelayTimer = 0f;
				if (_saveIntervalIndex < _saveIntervals.Length - 1)
				{
					_saveIntervalIndex++;
				}
			}
		}
		if (!((Object)(object)GameDataManager.Instance != (Object)null) || GameDataManager.Instance.GetGameData() == null)
		{
			return;
		}
		long helpedTime = GameDataManager.Instance.GetHelpedTime();
		if (helpedTime == -1)
		{
			_invitePollTimer += Time.deltaTime;
			if (_invitePollTimer >= 30f)
			{
				_invitePollTimer = 0f;
				PollInviteStatus();
			}
		}
		else
		{
			_invitePollTimer = 0f;
		}
		if (helpedTime <= 0)
		{
			return;
		}
		long num = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
		long num2 = 86400000L;
		long num3 = num - helpedTime;
		if (num3 < num2)
		{
			if ((Object)(object)SingletonMono<BuffManager>.Instance != (Object)null && !HasInviteHelpedBuff())
			{
				float value = (((Object)(object)ConfigManager.Instance != (Object)null) ? ConfigManager.Instance.GetCommonFactor(26) : 0f);
				SingletonMono<BuffManager>.Instance.AddBuff(BuffType.Income, value, 2.1474836E+09f, "InviteHelpedBuff");
				Debug.Log((object)$"[UpgradeManager] 检测到有未添加的助力Buff，已自动添加 InviteHelpedBuff，已过去: {(float)num3 / 1000f} 秒");
			}
		}
		else
		{
			if ((Object)(object)SingletonMono<BuffManager>.Instance != (Object)null)
			{
				SingletonMono<BuffManager>.Instance.RemoveBuff(BuffType.Income, "InviteHelpedBuff");
			}
			GameDataManager.Instance.SetHelpedTime(0L);
			Debug.Log((object)"[UpgradeManager] 助力 Buff 24小时已到期，已移除 InviteHelpedBuff 并重置 HelpedTime 为 0");
		}
	}

	private bool HasInviteHelpedBuff()
	{
		if ((Object)(object)SingletonMono<BuffManager>.Instance == (Object)null)
		{
			return false;
		}
		List<BuffData> activeBuffs = SingletonMono<BuffManager>.Instance.GetActiveBuffs(BuffType.Income);
		if (activeBuffs == null)
		{
			return false;
		}
		foreach (BuffData item in activeBuffs)
		{
			if (item.Name == "InviteHelpedBuff")
			{
				return true;
			}
		}
		return false;
	}

	private void PollInviteStatus()
	{
		if ((Object)(object)SingletonMono<HttpManager>.Instance == (Object)null || (Object)(object)GameDataManager.Instance == (Object)null)
		{
			return;
		}
		string openId = GameDataManager.Instance.GetOpenId();
		if (string.IsNullOrEmpty(openId))
		{
			return;
		}
		Debug.Log((object)"[UpgradeManager] 正在自动轮询助力状态...");
		SingletonMono<HttpManager>.Instance.GetShareStatus(openId, null, delegate(bool isSuccess, string msg)
		{
			if (isSuccess)
			{
				Debug.Log((object)("[UpgradeManager] 自动轮询助力状态成功: " + msg));
				PlayerHelpStatusData playerHelpStatusData = JsonUtility.FromJson<PlayerHelpStatusData>(msg);
				if (playerHelpStatusData != null && playerHelpStatusData.isHelped)
				{
					GameDataManager.Instance.SetHelpedTime((long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds);
					Debug.Log((object)"[UpgradeManager] 自动轮询检测到玩家已被助力，更新 HelpedTime，停止轮询");
				}
			}
			else
			{
				Debug.LogWarning((object)("[UpgradeManager] 自动轮询助力状态失败: " + msg));
			}
		});
	}

	public void RealSaveData(bool writeFiles = true)
	{
		UpgradeData upgradeData = GameDataManager.Instance?.GetUpgradeData();
		if (upgradeData == null)
		{
			Debug.LogError((object)"UpgradeData 为空，无法保存升级数据");
			return;
		}
		upgradeData.upgradeItemLevels.Clear();
		foreach (KeyValuePair<int, int> upgradeLevel in _upgradeLevels)
		{
			upgradeData.upgradeItemLevels.Add(new UpgradeLevelData
			{
				itemId = upgradeLevel.Key,
				level = upgradeLevel.Value
			});
		}
		upgradeData.unlockedUpgradeItems = new List<int>(_unlockedUpgradeItems);
		if (upgradeData.currencyEvolutionItems == null)
		{
			upgradeData.currencyEvolutionItems = new List<CurrencyEvolutionData>();
		}
		else
		{
			upgradeData.currencyEvolutionItems.Clear();
		}
		foreach (CurrencyEvolutionData value in _currencyEvolutionItems.Values)
		{
			upgradeData.currencyEvolutionItems.Add(new CurrencyEvolutionData
			{
				itemId = value.itemId,
				tier = value.tier,
				completedAtLevel = value.completedAtLevel
			});
		}
		if (writeFiles)
		{
			GameDataManager.Instance?.SaveUpgradeDataOnly();
		}
		GameData gameData = GameDataManager.Instance?.GetGameData();
		if (gameData == null)
		{
			Debug.LogError((object)"GameData 为空，无法保存升级数据");
			return;
		}
		gameData.totalGold = _totalGold;
		gameData.averageCoin = GetAverageGoldPerMinuteFromLast30Seconds();
		if (writeFiles)
		{
			GameDataManager.Instance?.SaveGameDataOnly();
		}
	}

	private void EnsureGoldNotNegative()
	{
		if (_totalGold < 0.0)
		{
			Debug.LogWarning((object)$"检测到_totalGold为负数({_totalGold})，已自动修正为0");
			_totalGold = 0.0;
		}
	}

	public void AddGold(double amount)
	{
		_totalGold += amount;
		EnsureGoldNotNegative();
		RecordGoldIncome(amount);
		SaveData();
		this.OnGoldChanged?.Invoke(_totalGold);
		this.CurrentGoldGain?.Invoke(amount);
		CheckMissionProgress();
	}

	public void AddGoldInternal(double amount)
	{
		_totalGold += amount;
		EnsureGoldNotNegative();
		SaveData();
		this.OnGoldChanged?.Invoke(_totalGold);
	}

	private void CheckMissionProgress()
	{
		if ((Object)(object)MissionManager.Instance != (Object)null)
		{
			MissionManager.Instance.CheckMissionProgress(_totalGold);
		}
	}

	private void ResetGoldIncomeStats()
	{
		_goldIncomeSamples.Clear();
		_goldIncomeSumInWindow = 0.0;
	}

	private void RecordGoldIncome(double amount)
	{
		if (!(amount <= 0.0))
		{
			float unscaledTime = Time.unscaledTime;
			PruneGoldIncome(unscaledTime);
			_goldIncomeSamples.Enqueue(new GoldIncomeSample
			{
				Time = unscaledTime,
				Amount = amount
			});
			_goldIncomeSumInWindow += amount;
		}
	}

	private void PruneGoldIncome(float now)
	{
		while (_goldIncomeSamples.Count > 0)
		{
			GoldIncomeSample goldIncomeSample = _goldIncomeSamples.Peek();
			if (!(now - goldIncomeSample.Time <= 30f))
			{
				_goldIncomeSamples.Dequeue();
				_goldIncomeSumInWindow -= goldIncomeSample.Amount;
				continue;
			}
			break;
		}
	}

	public double GetGoldIncomeInLast30Seconds()
	{
		float unscaledTime = Time.unscaledTime;
		PruneGoldIncome(unscaledTime);
		return _goldIncomeSumInWindow;
	}

	public double GetAverageGoldPerMinuteFromLast30Seconds()
	{
		double goldIncomeInLast30Seconds = GetGoldIncomeInLast30Seconds();
		double val = ConfigManager.Instance.GetCommonFactor(13);
		val = Math.Max(0.1, val);
		return Math.Max(1.0, goldIncomeInLast30Seconds * 2.0 * val);
	}

	public bool TrySpendGold(double amount)
	{
		if (_totalGold >= amount)
		{
			_totalGold -= amount;
			EnsureGoldNotNegative();
			SaveData();
			this.OnGoldChanged?.Invoke(_totalGold);
			return true;
		}
		return false;
	}

	public bool HasEnoughGold(double amount)
	{
		return _totalGold >= amount;
	}

	public void SetGold(double amount)
	{
		_totalGold = amount;
		EnsureGoldNotNegative();
		SaveData();
		this.OnGoldChanged?.Invoke(_totalGold);
	}

	private bool CheckUnlockCondition(int itemId)
	{
		itemUpgrade1 upgrade1Config = ConfigManager.Instance.GetUpgrade1Config(itemId);
		if (upgrade1Config != null)
		{
			return CheckUnlockConditionImplOld(upgrade1Config.UnLockCondition, itemId);
		}
		itemUpgrade2 upgrade2Config = ConfigManager.Instance.GetUpgrade2Config(itemId);
		if (upgrade2Config != null)
		{
			return CheckUnlockConditionImplOld(upgrade2Config.UnLockCondition, itemId);
		}
		return false;
	}

	private bool CheckUnlockConditionImplOld(int condition, int itemId)
	{
		if (condition == -1)
		{
			return true;
		}
		if (condition == -2)
		{
			return true;
		}
		if (condition > 0)
		{
			return CheckCoinCountUnlock(itemId, condition, "itemUpgrade1");
		}
		return false;
	}

	private bool CheckUnlockConditionImpl(int condition, int itemId, string configType, int conditionIconType = -1)
	{
		if (condition == -1)
		{
			return true;
		}
		if (condition == -2)
		{
			int num = GameDataManager.Instance?.GetCurrentWorldId() ?? 1;
			bool result = false;
			switch (num)
			{
			case 1:
				result = CheckTalentUnlock(itemId);
				break;
			case 2:
				if (configType == "itemUpgrade1" && itemId == 105)
				{
					result = !(TalentDataManager.Instance.GetDiceTalentBuff(102) <= 0f);
				}
				if (configType == "itemUpgrade3" && itemId == 103)
				{
					result = !(TalentDataManager.Instance.GetDiceTalentBuff(303) <= 0f);
				}
				break;
			}
			return result;
		}
		if (condition >= 0)
		{
			if (conditionIconType == -1)
			{
				return true;
			}
			if (TryGetCurrencyUpgradeId(conditionIconType, out var upgradeId))
			{
				int num2 = GetCurrencyCountForUnlock(upgradeId);
				if (GameDataManager.Instance.GetCurrentWorldId() == 1 && configType == "itemUpgrade1" && itemId == 3)
				{
					num2 += GetInitialCopperCount();
				}
				return num2 >= condition;
			}
			return false;
		}
		return false;
	}

	private bool CheckCoinCountUnlock(int itemId, int requiredCount, string configType)
	{
		if (IsCurrentDiceWorld())
		{
			itemUpgrade1 itemUpgrade = ((configType == "itemUpgrade1") ? ConfigManager.Instance.GetUpgrade1Config(itemId) : null);
			itemUpgrade2 itemUpgrade2 = ((configType == "itemUpgrade2") ? ConfigManager.Instance.GetUpgrade2Config(itemId) : null);
			itemUpgrade3 itemUpgrade3 = ((configType == "itemUpgrade3") ? ConfigManager.Instance.GetUpgrade3Config(itemId) : null);
			int conditionIconType = itemUpgrade?.ConditionIconType ?? itemUpgrade2?.ConditionIconType ?? itemUpgrade3?.ConditionIconType ?? (-1);
			if (TryGetCurrencyUpgradeId(conditionIconType, out var upgradeId))
			{
				return GetCurrencyCountForUnlock(upgradeId) >= requiredCount;
			}
			return false;
		}
		int num = -1;
		if (configType == "itemUpgrade1")
		{
			switch (itemId)
			{
			case 3:
				num = 2;
				break;
			case 4:
				num = 3;
				break;
			default:
				return false;
			}
		}
		else
		{
			switch (itemId)
			{
			case 2:
				num = 3;
				break;
			case 3:
				num = 4;
				break;
			case 4:
				num = 3;
				break;
			case 5:
				num = 4;
				break;
			case 6:
				num = 2;
				break;
			case 7:
				num = 3;
				break;
			case 8:
				num = 4;
				break;
			case 10:
				num = 3;
				break;
			case 11:
				num = 4;
				break;
			case 14:
				num = 5;
				break;
			default:
				return false;
			}
		}
		int num2 = GetUpgradeLevel(num);
		if (configType == "itemUpgrade1" && itemId == 3)
		{
			num2 += GetInitialCopperCount();
		}
		return num2 >= requiredCount;
	}

	private bool IsCurrentDiceWorld()
	{
		if ((Object)(object)GameDataManager.Instance == (Object)null || !ConfigManager.HasInstance())
		{
			return false;
		}
		world worldConfig = ConfigManager.Instance.GetWorldConfig(GameDataManager.Instance.GetCurrentWorldId());
		if (worldConfig != null)
		{
			return worldConfig.GameplayType == 2;
		}
		return false;
	}

	private bool TryGetCurrencyUpgradeId(int conditionIconType, out int upgradeId)
	{
		upgradeId = -1;
		if (conditionIconType <= 0 || !ConfigManager.HasInstance())
		{
			return false;
		}
		if (IsCurrentDiceWorld())
		{
			if (conditionIconType >= 1 && conditionIconType <= 5)
			{
				upgradeId = 101 + conditionIconType;
				return true;
			}
			return false;
		}
		switch (conditionIconType)
		{
		case 1:
			upgradeId = 2;
			break;
		case 2:
			upgradeId = 3;
			break;
		case 3:
			upgradeId = 4;
			break;
		case 4:
			upgradeId = 5;
			break;
		}
		return upgradeId > 0;
	}

	private int GetCurrencyCountForUnlock(int upgradeId)
	{
		int num = GetUpgradeLevel(upgradeId, "itemUpgrade1");
		if (!IsCurrentDiceWorld())
		{
			return num;
		}
		if (upgradeId == 102)
		{
			num += GetInitialDiceCount();
		}
		return num;
	}

	private CoinType GetCoinTypeForUpgradeItem(int itemId)
	{
		return itemId switch
		{
			2 => CoinType.Copper, 
			3 => CoinType.Silver, 
			4 => CoinType.Gold, 
			5 => CoinType.Purple, 
			_ => CoinType.Copper, 
		};
	}

	public void SpawnCoinForUpgrade(int upgradeItemId, bool? forceSpawnAtCenter = null)
	{
		CoinType coinTypeForUpgradeItem = GetCoinTypeForUpgradeItem(upgradeItemId);
		int upgradeLevel = GetUpgradeLevel(upgradeItemId, "itemUpgrade1");
		bool flag = forceSpawnAtCenter ?? IsEvolutionLevel(upgradeItemId, upgradeLevel);
		if ((Object)(object)UIManager.Instance != (Object)null && UIManager.Instance.IsVatMainSceneActive)
		{
			UIManager.Instance.MainSceneVat?.SyncCoinPopulation(coinTypeForUpgradeItem, flag);
			if (flag)
			{
				Debug.Log((object)$"[UpgradeManager] 硬币进化: id={upgradeItemId}, type={coinTypeForUpgradeItem}, level={upgradeLevel}, tier={GetEvolutionTier(upgradeItemId)}, buff×{GetEvolutionIncomeMultiplier(upgradeItemId)}, field={GetCurrencyFieldCount(upgradeItemId)}");
			}
			else
			{
				Debug.Log((object)$"[UpgradeManager] VAT upgrade {upgradeItemId} synced {coinTypeForUpgradeItem} coin, field={GetCurrencyFieldCount(upgradeItemId)}");
			}
		}
		else
		{
			MainScene mainScene = UIManager.Instance.MainScene;
			if ((Object)(object)mainScene != (Object)null)
			{
				mainScene.SpawnCoin(coinTypeForUpgradeItem);
				Debug.Log((object)$"[UpgradeManager] 升级 {upgradeItemId} 成功，召唤 {coinTypeForUpgradeItem} 硬币");
			}
			else
			{
				Debug.LogWarning((object)"[UpgradeManager] MainScene 未找到，无法召唤硬币");
			}
		}
	}

	public bool TriggerEvolutionAnimation(int itemId, int expectedTier = -1)
	{
		int evolutionTier = GetEvolutionTier(itemId);
		if (expectedTier >= 0 && evolutionTier != expectedTier)
		{
			Debug.LogWarning((object)$"[UpgradeManager] 忽略过期的进化请求: id={itemId}, expectedTier={expectedTier}, currentTier={evolutionTier}");
			return false;
		}
		int evolutionTargetTier = GetEvolutionTargetTier(itemId);
		if (evolutionTargetTier <= evolutionTier)
		{
			Debug.LogWarning((object)string.Format("[UpgradeManager] 当前不可进化: id={0}, level={1}, tier={2}", itemId, GetUpgradeLevel(itemId, "itemUpgrade1"), evolutionTier));
			return false;
		}
		if (_lastEvolutionFrame.TryGetValue(itemId, out var value) && value == Time.frameCount)
		{
			Debug.LogWarning((object)$"[UpgradeManager] 忽略同一帧内的重复进化请求: id={itemId}");
			return false;
		}
		_lastEvolutionFrame[itemId] = Time.frameCount;
		if (itemId >= 2 && itemId <= 5)
		{
			CoinType coinTypeForUpgradeItem = GetCoinTypeForUpgradeItem(itemId);
			if ((Object)(object)UIManager.Instance != (Object)null && UIManager.Instance.IsVatMainSceneActive)
			{
				UIManager.Instance.MainSceneVat?.SyncCoinPopulation(coinTypeForUpgradeItem, spawnAtCenter: true, 1, evolutionTargetTier);
				Debug.Log((object)$"[UpgradeManager] 硬币进化合成动画: id={itemId}, type={coinTypeForUpgradeItem}, desired=1, targetTier={evolutionTargetTier}");
			}
		}
		else if (itemId >= 102 && itemId <= 105 && IsCurrentDiceWorld() && (Object)(object)UIManager.Instance != (Object)null && (Object)(object)UIManager.Instance.MainSceneVat != (Object)null)
		{
			UIManager.Instance.MainSceneVat.TryRefreshDicePopulationForSpawnUpgrade(itemId, true, 1, evolutionTargetTier);
			Debug.Log((object)$"[UpgradeManager] 骰子进化合成动画: id={itemId}, desired=1, targetTier={evolutionTargetTier}");
		}
		string text = "itemUpgrade1";
		int storageKey = GetStorageKey(itemId, text);
		if (!_upgradeLevels.ContainsKey(storageKey))
		{
			_upgradeLevels[storageKey] = 0;
		}
		_upgradeLevels[storageKey]++;
		_currencyEvolutionItems[itemId] = new CurrencyEvolutionData
		{
			itemId = itemId,
			tier = evolutionTargetTier,
			completedAtLevel = _upgradeLevels[storageKey]
		};
		if (!_unlockedUpgradeItems.Contains(storageKey))
		{
			_unlockedUpgradeItems.Add(storageKey);
		}
		RealSaveData();
		this.OnUpgradeSuccess?.Invoke(itemId, _upgradeLevels[storageKey]);
		this.OnTypedUpgradeSuccess?.Invoke(itemId, _upgradeLevels[storageKey], text);
		Debug.Log((object)$"[UpgradeManager] 进化升级成功（免消耗）: id={itemId}, level={_upgradeLevels[storageKey]}, tier={evolutionTargetTier}, buff×{GetEvolutionIncomeMultiplier(itemId)}");
		return true;
	}

	public static bool IsEvolvableCurrencyUpgrade(int itemId)
	{
		if (!EnableCurrencyEvolution)
		{
			return false;
		}
		if (itemId < 2 || itemId > 5)
		{
			if (itemId >= 102)
			{
				return itemId <= 105;
			}
			return false;
		}
		return true;
	}

	public int GetEvolutionTier(int itemId)
	{
		if (!IsEvolvableCurrencyUpgrade(itemId))
		{
			return 0;
		}
		if (!_currencyEvolutionItems.TryGetValue(itemId, out var value) || value == null)
		{
			return 0;
		}
		int valueOrDefault = ((ConfigManager.Instance?.GetUpgrade1Config(itemId))?.EvolutionLevel?.Count).GetValueOrDefault();
		return Mathf.Clamp(value.tier, 0, valueOrDefault);
	}

	public int GetEvolutionTargetTier(int itemId)
	{
		if (!IsEvolvableCurrencyUpgrade(itemId))
		{
			return 0;
		}
		List<int> list = (ConfigManager.Instance?.GetUpgrade1Config(itemId))?.EvolutionLevel;
		if (list == null || list.Count == 0)
		{
			return GetEvolutionTier(itemId);
		}
		int upgradeLevel = GetUpgradeLevel(itemId, "itemUpgrade1");
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			int num2 = list[i];
			if (num2 > 0 && upgradeLevel >= num2)
			{
				num++;
			}
		}
		return Mathf.Clamp(Mathf.Max(GetEvolutionTier(itemId), num), 0, list.Count);
	}

	public int GetEvolutionTierByLevel(int itemId, int level)
	{
		if (!IsEvolvableCurrencyUpgrade(itemId) || level <= 0)
		{
			return 0;
		}
		if (!_currencyEvolutionItems.TryGetValue(itemId, out var value) || value == null)
		{
			return 0;
		}
		if (value.completedAtLevel <= 0 || level < value.completedAtLevel)
		{
			return 0;
		}
		return GetEvolutionTier(itemId);
	}

	public bool CanEvolveCurrency(int itemId)
	{
		if (!IsEvolvableCurrencyUpgrade(itemId))
		{
			return false;
		}
		return GetEvolutionTargetTier(itemId) > GetEvolutionTier(itemId);
	}

	public bool IsEvolutionLevel(int itemId, int level)
	{
		if (!IsEvolvableCurrencyUpgrade(itemId) || level <= 0)
		{
			return false;
		}
		List<int> list = (ConfigManager.Instance?.GetUpgrade1Config(itemId))?.EvolutionLevel;
		if (list == null || list.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] == level)
			{
				return true;
			}
		}
		return false;
	}

	public int GetNextEvolutionLevel(int itemId, int currentLevel)
	{
		if (!IsEvolvableCurrencyUpgrade(itemId))
		{
			return -1;
		}
		List<int> list = (ConfigManager.Instance?.GetUpgrade1Config(itemId))?.EvolutionLevel;
		if (list == null || list.Count == 0)
		{
			return -1;
		}
		int evolutionTier = GetEvolutionTier(itemId);
		if (evolutionTier < 0 || evolutionTier >= list.Count || list[evolutionTier] <= 0)
		{
			return -1;
		}
		return list[evolutionTier];
	}

	public bool WillNextUpgradeTriggerEvolution(int itemId)
	{
		if (!IsEvolvableCurrencyUpgrade(itemId))
		{
			return false;
		}
		int nextEvolutionLevel = GetNextEvolutionLevel(itemId, GetUpgradeLevel(itemId, "itemUpgrade1"));
		if (nextEvolutionLevel > 0)
		{
			return GetUpgradeLevel(itemId, "itemUpgrade1") + 1 >= nextEvolutionLevel;
		}
		return false;
	}

	public int GetEvolutionMergeCount(int itemId, int evolutionLevel)
	{
		if (!IsEvolvableCurrencyUpgrade(itemId))
		{
			return 0;
		}
		List<int> list = (ConfigManager.Instance?.GetUpgrade1Config(itemId))?.EvolutionLevel;
		if (list == null || list.Count == 0)
		{
			return 0;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] == evolutionLevel)
			{
				int num = ((i > 0) ? list[i - 1] : 0);
				return Mathf.Max(1, evolutionLevel - num);
			}
		}
		return 0;
	}

	public double GetEvolutionIncomeMultiplier(int itemId)
	{
		return GetEvolutionIncomeMultiplierForTier(itemId, GetEvolutionTier(itemId));
	}

	public double GetEvolutionIncomeMultiplierForTier(int itemId, int tier)
	{
		if (!IsEvolvableCurrencyUpgrade(itemId) || tier <= 0)
		{
			return 1.0;
		}
		List<float> list = (ConfigManager.Instance?.GetUpgrade1Config(itemId))?.EvolutionBuff;
		if (list == null || list.Count == 0)
		{
			return 1.0;
		}
		double num = 1.0;
		int num2 = Mathf.Min(tier, list.Count);
		for (int i = 0; i < num2; i++)
		{
			float num3 = list[i];
			if (num3 > 0f)
			{
				num *= (double)num3;
			}
		}
		return num;
	}

	public int GetEvolvedEntityCount(int itemId, int purchasedLevel, int initialCount = 0)
	{
		int num = Mathf.Max(0, purchasedLevel);
		int num2 = Mathf.Max(0, initialCount);
		if (!IsEvolvableCurrencyUpgrade(itemId))
		{
			return num + num2;
		}
		int evolutionTier = GetEvolutionTier(itemId);
		if (!_currencyEvolutionItems.TryGetValue(itemId, out var value) || value == null || evolutionTier <= 0 || value.completedAtLevel <= 0)
		{
			return num + num2;
		}
		List<int> list = (ConfigManager.Instance?.GetUpgrade1Config(itemId))?.EvolutionLevel;
		if (list == null || list.Count == 0)
		{
			return num + num2;
		}
		int index = Mathf.Min(evolutionTier, list.Count) - 1;
		int num3 = list[index];
		if (num3 <= 0)
		{
			return num + num2;
		}
		int num4 = Mathf.Max(0, value.completedAtLevel - 1);
		int num5 = Mathf.Max(1, num4 - num3);
		int num6 = Mathf.Max(0, num - value.completedAtLevel);
		return num5 + num6;
	}

	public int GetCurrencyFieldCount(int itemId)
	{
		int upgradeLevel = GetUpgradeLevel(itemId, "itemUpgrade1");
		int initialCount = 0;
		switch (itemId)
		{
		case 2:
			initialCount = GetInitialCopperCount();
			break;
		case 102:
			initialCount = GetInitialDiceCount();
			break;
		}
		return GetEvolvedEntityCount(itemId, upgradeLevel, initialCount);
	}

	public static int GetCoinEvolutionUpgradeId(CoinType coinType)
	{
		return coinType switch
		{
			CoinType.Copper => 2, 
			CoinType.Silver => 3, 
			CoinType.Gold => 4, 
			CoinType.Purple => 5, 
			_ => 2, 
		};
	}

	public void UnlockedUpgradeRemove(int itemId, string configType)
	{
		int storageKey = GetStorageKey(itemId, configType);
		if (!_unlockedUpgradeItems.Contains(storageKey))
		{
			return;
		}
		for (int num = _unlockedUpgradeItems.Count - 1; num >= 0; num--)
		{
			if (_unlockedUpgradeItems[num] == storageKey)
			{
				_unlockedUpgradeItems.RemoveAt(num);
				break;
			}
		}
	}

	public float GetCommonFactor(int id)
	{
		return ConfigManager.Instance.GetCommonConfig(id)?.Factor ?? 0f;
	}

	public double GetDiceOnePointValue(int qualityId)
	{
		if (qualityId <= 0)
		{
			return 0.0;
		}
		int num = -1;
		int num2;
		switch (qualityId)
		{
		case 102:
			num2 = 60;
			num = 101;
			break;
		case 103:
			num2 = 61;
			num = 102;
			break;
		case 104:
			num2 = 62;
			num = 103;
			break;
		case 105:
			num2 = 63;
			num = 110;
			break;
		case 106:
			num2 = 63;
			num = 110;
			break;
		default:
			Debug.LogWarning((object)$"未配置骰子品质 id={qualityId} 的 Common 基础价值");
			return 1.0;
		}
		float commonFactor = GetCommonFactor(num2);
		float num3 = 1f;
		if (num != -1)
		{
			int upgradeLevel = GetUpgradeLevel(num, "itemUpgrade2");
			num3 = Mathf.Pow(ConfigManager.Instance.GetUpgrade2Result(num), (float)upgradeLevel);
		}
		double evolutionIncomeMultiplier = GetEvolutionIncomeMultiplier(qualityId);
		if (commonFactor > 0f)
		{
			return (double)(commonFactor * num3) * evolutionIncomeMultiplier;
		}
		Debug.LogWarning((object)$"Common id={num2} 的骰子基础价值无效");
		return 1.0 * (double)num3 * evolutionIncomeMultiplier;
	}

	public double GetDiceFaceValueMultiplier(int face)
	{
		if (face < 1 || face > 6)
		{
			Debug.LogWarning((object)$"骰子点数超出有效范围: {face}");
			return 0.0;
		}
		int num;
		switch (face)
		{
		case 1:
			return 1.0;
		case 2:
			num = 64;
			break;
		case 3:
			num = 65;
			break;
		case 4:
			num = 66;
			break;
		case 5:
			num = 67;
			break;
		case 6:
			num = 68;
			break;
		default:
			return 0.0;
		}
		float commonFactor = GetCommonFactor(num);
		if (commonFactor > 0f)
		{
			return commonFactor;
		}
		Debug.LogWarning((object)$"Common id={num} 的骰子点数倍率无效，使用默认倍率 {face}");
		return face;
	}

	public double GetDiceBaseValue(int qualityId, int face)
	{
		return GetDiceOnePointValue(qualityId) * GetDiceFaceValueMultiplier(face);
	}

	public double GetDiceFinalValue(double baseValue, int roleType)
	{
		float num = 1f;
		if (TalentDataManager.Instance != null)
		{
			if (roleType == 7)
			{
				num += TalentDataManager.Instance.JiuWei();
			}
			if (roleType == 8)
			{
				num += TalentDataManager.Instance.GangTie();
			}
		}
		switch (roleType)
		{
		case 3:
			if (GameDataManager.Instance.GetHelperAwakeSkillLevel(roleType) > 1)
			{
				num += 0.5f;
			}
			break;
		case 8:
			if (GameDataManager.Instance.GetHelperAwakeSkillLevel(roleType) > 1)
			{
				num += 0.5f;
			}
			break;
		}
		float extraBuff = ((roleType > 0) ? GetHelperExtraBuff(roleType) : 0f);
		float extraCriteRate = ((roleType == 1) ? 1f : 0f);
		bool isManual = roleType == -1;
		float num2 = ((roleType > 0) ? GetHelperIncomeMultiplier(roleType) : 1f);
		return SingletonMono<BuffManager>.Instance.GetFinalIncome(0, baseValue * (double)num * (double)num2, extraBuff, extraCriteRate, isManual);
	}

	public double GetCopperCoinValue(int roleType)
	{
		int num = (int)GetCommonFactor(5);
		int upgradeLevel = GetUpgradeLevel(1, "itemUpgrade2");
		float num2 = Mathf.Pow(ConfigManager.Instance.GetUpgrade2Result(1), (float)upgradeLevel);
		if (TalentDataManager.Instance != null)
		{
			if (roleType == 7)
			{
				num2 += TalentDataManager.Instance.JiuWei();
			}
			if (roleType == 8)
			{
				num2 += TalentDataManager.Instance.GangTie();
			}
		}
		switch (roleType)
		{
		case 3:
			if (GameDataManager.Instance.GetHelperAwakeSkillLevel(roleType) > 1)
			{
				num2 += 0.3f;
			}
			break;
		case 8:
			if (GameDataManager.Instance.GetHelperAwakeSkillLevel(roleType) > 1)
			{
				num2 += 0.5f;
			}
			break;
		}
		float extraBuff = ((roleType > 0) ? GetHelperExtraBuff(roleType) : 0f);
		float extraCriteRate = ((roleType == 1) ? 1f : 0f);
		bool isManual = roleType == -1;
		float num3 = ((roleType > 0) ? GetHelperIncomeMultiplier(roleType) : 1f);
		double evolutionIncomeMultiplier = GetEvolutionIncomeMultiplier(2);
		return SingletonMono<BuffManager>.Instance.GetFinalIncome(1, (double)((float)num * num2 * num3) * evolutionIncomeMultiplier, extraBuff, extraCriteRate, isManual);
	}

	public double GetSilverCoinValue(int roleType)
	{
		int num = (int)GetCommonFactor(6);
		int upgradeLevel = GetUpgradeLevel(2, "itemUpgrade2");
		float num2 = Mathf.Pow(ConfigManager.Instance.GetUpgrade2Result(2), (float)upgradeLevel);
		if (TalentDataManager.Instance != null)
		{
			if (roleType == 7)
			{
				num2 += TalentDataManager.Instance.JiuWei();
			}
			if (roleType == 8)
			{
				num2 += TalentDataManager.Instance.GangTie();
			}
		}
		switch (roleType)
		{
		case 3:
			if (GameDataManager.Instance.GetHelperAwakeSkillLevel(roleType) > 1)
			{
				num2 += 0.5f;
			}
			break;
		case 8:
			if (GameDataManager.Instance.GetHelperAwakeSkillLevel(roleType) > 1)
			{
				num2 += 0.5f;
			}
			break;
		}
		float extraBuff = ((roleType > 0) ? GetHelperExtraBuff(roleType) : 0f);
		float extraCriteRate = ((roleType == 1) ? 1f : 0f);
		bool isManual = roleType == -1;
		float num3 = ((roleType > 0) ? GetHelperIncomeMultiplier(roleType) : 1f);
		double evolutionIncomeMultiplier = GetEvolutionIncomeMultiplier(3);
		return SingletonMono<BuffManager>.Instance.GetFinalIncome(2, (double)((float)num * num2 * num3) * evolutionIncomeMultiplier, extraBuff, extraCriteRate, isManual);
	}

	public double GetGoldCoinValue(int roleType)
	{
		int num = (int)GetCommonFactor(7);
		int upgradeLevel = GetUpgradeLevel(3, "itemUpgrade2");
		float num2 = Mathf.Pow(ConfigManager.Instance.GetUpgrade2Result(3), (float)upgradeLevel);
		if (TalentDataManager.Instance != null)
		{
			if (roleType == 7)
			{
				num2 += TalentDataManager.Instance.JiuWei();
			}
			if (roleType == 8)
			{
				num2 += TalentDataManager.Instance.GangTie();
			}
		}
		switch (roleType)
		{
		case 3:
			if (GameDataManager.Instance.GetHelperAwakeSkillLevel(roleType) > 1)
			{
				num2 += 0.5f;
			}
			break;
		case 8:
			if (GameDataManager.Instance.GetHelperAwakeSkillLevel(roleType) > 1)
			{
				num2 += 0.5f;
			}
			break;
		}
		float extraBuff = ((roleType > 0) ? GetHelperExtraBuff(roleType) : 0f);
		float extraCriteRate = ((roleType == 1) ? 1f : 0f);
		bool isManual = roleType == -1;
		float num3 = ((roleType > 0) ? GetHelperIncomeMultiplier(roleType) : 1f);
		double evolutionIncomeMultiplier = GetEvolutionIncomeMultiplier(4);
		return SingletonMono<BuffManager>.Instance.GetFinalIncome(3, (double)((float)num * num2 * num3) * evolutionIncomeMultiplier, extraBuff, extraCriteRate, isManual);
	}

	public double GetPurpleCoinValue(int roleType)
	{
		int num = (int)GetCommonFactor(15);
		int upgradeLevel = GetUpgradeLevel(14, "itemUpgrade2");
		float num2 = Mathf.Pow(ConfigManager.Instance.GetUpgrade2Result(14), (float)upgradeLevel);
		if (TalentDataManager.Instance != null)
		{
			num2 += TalentDataManager.Instance.UpPurpleValue();
		}
		if (TalentDataManager.Instance != null)
		{
			if (roleType == 7)
			{
				num2 += TalentDataManager.Instance.JiuWei();
			}
			if (roleType == 8)
			{
				num2 += TalentDataManager.Instance.GangTie();
			}
		}
		switch (roleType)
		{
		case 3:
			if (GameDataManager.Instance.GetHelperAwakeSkillLevel(roleType) > 1)
			{
				num2 += 0.5f;
			}
			break;
		case 8:
			if (GameDataManager.Instance.GetHelperAwakeSkillLevel(roleType) > 1)
			{
				num2 += 0.5f;
			}
			break;
		}
		float extraBuff = ((roleType > 0) ? GetHelperExtraBuff(roleType) : 0f);
		float extraCriteRate = ((roleType == 1) ? 1f : 0f);
		bool isManual = roleType == -1;
		float num3 = ((roleType > 0) ? GetHelperIncomeMultiplier(roleType) : 1f);
		double evolutionIncomeMultiplier = GetEvolutionIncomeMultiplier(5);
		return SingletonMono<BuffManager>.Instance.GetFinalIncome(4, (double)((float)num * num2 * num3) * evolutionIncomeMultiplier, extraBuff, extraCriteRate, isManual);
	}

	public int GetInitialCopperCount()
	{
		return (int)GetCommonFactor(3);
	}

	public int GetInitialDiceCount()
	{
		if (!ConfigManager.HasInstance())
		{
			return 3;
		}
		int num = (int)ConfigManager.Instance.GetCommonFactor(77);
		if (num <= 0)
		{
			return 3;
		}
		return num;
	}

	public bool CanRoleFlipCoin(CoinType coinType)
	{
		switch (coinType)
		{
		case CoinType.Copper:
			return true;
		case CoinType.Silver:
			return GetUpgradeLevel(4, "itemUpgrade2") > 0;
		case CoinType.Gold:
			return GetUpgradeLevel(5, "itemUpgrade2") > 0;
		case CoinType.Purple:
			if (TalentDataManager.Instance != null)
			{
				int currentWorldId = GameDataManager.Instance.GetCurrentWorldId();
				bool result = false;
				switch (currentWorldId)
				{
				case 1:
					result = TalentDataManager.Instance.IsFlipPurple();
					break;
				case 2:
					result = !(TalentDataManager.Instance.GetDiceTalentBuff(104) <= 0f);
					break;
				}
				return result;
			}
			return false;
		default:
			return false;
		}
	}

	public bool CanPlayerSwipeToThrow(CoinType coinType)
	{
		return coinType switch
		{
			CoinType.Copper => GetUpgradeLevel(6, "itemUpgrade2") > 0, 
			CoinType.Silver => GetUpgradeLevel(7, "itemUpgrade2") > 0, 
			CoinType.Gold => GetUpgradeLevel(8, "itemUpgrade2") > 0, 
			_ => false, 
		};
	}

	public bool CanRoleFlipDice(int qualityId)
	{
		switch (qualityId)
		{
		case 102:
			return true;
		case 103:
			return GetUpgradeLevel(104, "itemUpgrade2") > 0;
		case 104:
			return GetUpgradeLevel(105, "itemUpgrade2") > 0;
		case 105:
			if (TalentDataManager.Instance != null)
			{
				return TalentDataManager.Instance.GetDiceTalentBuff(104) > 0f;
			}
			return false;
		case 106:
			return false;
		default:
			return false;
		}
	}

	public bool CanPlayerSwipeToThrowDice(int qualityId)
	{
		return qualityId switch
		{
			102 => GetUpgradeLevel(106, "itemUpgrade2") > 0, 
			103 => GetUpgradeLevel(107, "itemUpgrade2") > 0, 
			104 => GetUpgradeLevel(108, "itemUpgrade2") > 0, 
			_ => false, 
		};
	}

	public float GetRoleSpeedMultiplier()
	{
		int upgradeLevel = GetUpgradeLevel(9, "itemUpgrade2");
		float upgrade2Result = ConfigManager.Instance.GetUpgrade2Result(9);
		return (float)SingletonMono<BuffManager>.Instance.GetFinalSpeed(1f + upgrade2Result * (float)upgradeLevel);
	}

	public bool CanSilverTriggerCopper()
	{
		return GetUpgradeLevel(10, "itemUpgrade2") > 0;
	}

	public bool CanGoldTriggerCoin(CoinType coinType)
	{
		if (GetUpgradeLevel(11, "itemUpgrade2") <= 0)
		{
			return false;
		}
		if ((uint)coinType <= 1u)
		{
			return true;
		}
		return false;
	}

	public float GetLuckyEchoProbability()
	{
		int upgradeLevel = GetUpgradeLevel(12, "itemUpgrade2");
		float num = ConfigManager.Instance.GetUpgrade2Result(12) - 1f;
		return (float)upgradeLevel * num;
	}

	public void NotifyCoinCountChanged()
	{
		this.OnCoinCountChanged?.Invoke();
	}

	protected override void BeforOnDestroy()
	{
		if ((Object)(object)_subscribedMissionManager != (Object)null)
		{
			_subscribedMissionManager.OnMissionUpdated -= OnMissionUpdated;
			_subscribedMissionManager = null;
		}
		if ((Object)(object)_subscribedGameDataManager != (Object)null)
		{
			_subscribedGameDataManager.OnWorldChanged -= OnWorldChanged;
			_subscribedGameDataManager = null;
		}
		ForceSave();
	}

	private void OnApplicationQuit()
	{
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		if (pauseStatus)
		{
			ForceSave();
		}
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (!hasFocus)
		{
			ForceSave();
		}
	}

	public bool CheckTalentUnlock(int itemId)
	{
		switch (itemId)
		{
		case 5:
			if (TalentDataManager.Instance != null)
			{
				return TalentDataManager.Instance.IsUnlockedPurpleCoin();
			}
			break;
		case 12:
			if (TalentDataManager.Instance != null)
			{
				return TalentDataManager.Instance.IsUnlockedHuiXiang();
			}
			break;
		}
		return false;
	}

	public void AddDeskExtraBuff(int deskId)
	{
		if ((Object)(object)SingletonMono<BuffManager>.Instance == (Object)null || (Object)(object)GameDataManager.Instance == (Object)null || !ConfigManager.HasInstance())
		{
			return;
		}
		desk deskConfig = ConfigManager.Instance.GetDeskConfig(deskId);
		BuffType buffType;
		if (deskConfig == null)
		{
			Debug.LogError((object)$"DeskConfig 未找到 id={deskId}");
		}
		else if (deskConfig.WorldId == GameDataManager.Instance.GetCurrentWorldId() && TryGetDeskBuffType(deskId, out buffType))
		{
			int deskLevel = GameDataManager.Instance.GetDeskLevel(deskId);
			if (deskLevel > 0)
			{
				float value = deskConfig.SkillNum * Mathf.Pow(deskConfig.SkillUpgradeBuff, (float)(deskLevel - 1));
				string buffName = $"DeskExtraBuff_{deskId}";
				SingletonMono<BuffManager>.Instance.RemoveBuff(buffType, buffName);
				SingletonMono<BuffManager>.Instance.AddBuff(buffType, value, float.MaxValue, buffName);
			}
		}
	}

	public float GetItemUpgrade3Effect(int itemId)
	{
		int upgradeLevel = GetUpgradeLevel(itemId, "itemUpgrade3");
		if ((Object)(object)ConfigManager.Instance == (Object)null)
		{
			if (itemId != 103)
			{
				return 1f;
			}
			return 0f;
		}
		float num = ConfigManager.Instance.GetUpgrade3Result(itemId) - 1f;
		switch (itemId)
		{
		case 101:
			return Mathf.Max(0f, 1f - num * (float)upgradeLevel);
		case 102:
			return 1f + num * (float)upgradeLevel;
		case 103:
			return Mathf.Clamp01(num * (float)upgradeLevel);
		case 104:
			return Mathf.Clamp01(num * (float)upgradeLevel);
		case 105:
			return Mathf.Pow(ConfigManager.Instance.GetUpgrade3Result(itemId), (float)upgradeLevel);
		case 106:
			if (upgradeLevel <= 0)
			{
				return 0f;
			}
			return 1f;
		case 107:
			if (upgradeLevel <= 0)
			{
				return 0f;
			}
			return 1f;
		default:
			return 1f;
		}
	}
}

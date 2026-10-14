using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Module.Http;
using Module.Mission;
using Module.Role;
using Module.Shop;
using P8SDKTT;
using TTSDK;
using UnityEngine;
using cfg;

public class GameDataManager : MonoBehaviour
{
	private static GameDataManager _instance;

	public long originalLastExitTime;

	private bool _playerActivelyEntered;

	private DateTime _appStartTime;

	[SerializeField]
	private GameData _currentGameData;

	[SerializeField]
	private FacilityData _currentFacilityData;

	[SerializeField]
	private UpgradeData _currentUpgradeData;

	[SerializeField]
	private TalentData _currentTalentData;

	[SerializeField]
	private GlobalProfileData _globalProfileData;

	[SerializeField]
	private WorldDataCollection _worldDataCollection;

	private bool _loadedGlobalProfileFromGameData;

	private bool _loadedWorldCollectionFromGameData;

	private long _worldArchiveRevision;

	[SerializeField]
	private WorldCupData _currentWorldCupData;

	private bool _worldCupDataNeedsLocalRewrite;

	private bool isDataDirtyForCloudSync;

	private bool _cloudSyncSuspended;

	private float saveToServerTimer;

	private const float SAVE_TO_SERVER_INTERVAL = 300f;

	private const float FIRST_SAVE_TO_SERVER_DELAY = 20f;

	private Coroutine _firstCloudSaveCoroutine;

	private bool _dirtyGameDataFast;

	private bool _dirtyFacilityDataFast;

	private bool _dirtyUpgradeDataFast;

	private bool _dirtyTalentDataFast;

	private bool _dirtyFullSaveFast;

	private bool _worldSaveInFlight;

	private bool _worldSavePending;

	private bool _worldSwitchCloudSyncInProgress;

	private readonly HashSet<int> _worldOutboxRetriesInFlight = new HashSet<int>();

	private long _cloudDirtyGeneration;

	private float _fastSyncTimer;

	private const float FAST_SYNC_INTERVAL = 120f;

	private bool _isSwitchingWorld;

	private DateTime _serverTime;

	private const int MAX_LOGS_COUNT = 100;

	private DateTime _currentLoginTime = DateTime.Now;

	private string _currentSessionKey = "";

	public bool skipSave;

	private PlayerPrefsData playerPrefsData;

	private TimeInfoSave timeSave;

	public bool PlayerActivelyEntered => _playerActivelyEntered;

	public static GameDataManager Instance
	{
		get
		{
			if ((Object)(object)_instance == (Object)null)
			{
				_instance = Object.FindObjectOfType<GameDataManager>();
				_ = (Object)(object)_instance == (Object)null;
			}
			return _instance;
		}
	}

	public WorldCupData CurrentWorldCupData => _currentWorldCupData;

	public int IsLocalSaveMissing { get; set; }

	public bool IsGameDataLoadError { get; private set; }

	public bool IsFacilityDataLoadError { get; private set; }

	public bool IsUpgradeDataLoadError { get; private set; }

	public bool IsTalentDataLoadError { get; private set; }

	public bool IsWorldCupDataLoadError { get; private set; }

	public DateTime ServerTime
	{
		get
		{
			return _serverTime;
		}
		private set
		{
			if (_serverTime != value)
			{
				_serverTime = value;
				this.OnServerTimeChanged?.Invoke(_serverTime);
			}
		}
	}

	private string GameDataFilePath => Path.Combine(Application.persistentDataPath, "game_data.json");

	private string FacilityDataFilePath => Path.Combine(Application.persistentDataPath, "facility_data.json");

	private string UpgradeDataFilePath => Path.Combine(Application.persistentDataPath, "upgrade_data.json");

	private string TimeInfoFilePath => Path.Combine(Application.persistentDataPath, "time_info.json");

	private string TalentDataFilePath => Path.Combine(Application.persistentDataPath, "talent_data.json");

	private string GlobalProfileDataFilePath => Path.Combine(Application.persistentDataPath, "global_profile_data.json");

	private string WorldDataCollectionFilePath => Path.Combine(Application.persistentDataPath, "world_data.json");

	private string WorldArchiveFilePath => Path.Combine(Application.persistentDataPath, "world_archive.json");

	private string WorldCupDataFilePath => Path.Combine(Application.persistentDataPath, "world_cup_data.json");

	private string PlayerPrefsDataFilePath => Path.Combine(Application.persistentDataPath, "player_prefs_data.json");

	private string TaskDataFilePath => Path.Combine(Application.persistentDataPath, "task_data.json");

	public event Action OnGameDataUpdated;

	public event Action OnCoinItemDiamondUpdated;

	public event Action<int, int> OnWorldChanged;

	public event Action OnNewGuideUpdated;

	public event Action OnGameFocus;

	public event Action<DateTime> OnServerTimeChanged;

	public void MarkPlayerActivelyEntered()
	{
		_playerActivelyEntered = true;
	}

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
		_appStartTime = DateTime.Now;
		LoadGameData();
	}

	private void Update()
	{
		if (_cloudSyncSuspended)
		{
			return;
		}
		_fastSyncTimer += Time.deltaTime;
		if (_fastSyncTimer >= 120f)
		{
			_fastSyncTimer = 0f;
			if (_dirtyFullSaveFast || _dirtyGameDataFast || _dirtyFacilityDataFast || _dirtyUpgradeDataFast || _dirtyTalentDataFast)
			{
				_dirtyFullSaveFast = false;
				_dirtyGameDataFast = false;
				_dirtyFacilityDataFast = false;
				_dirtyUpgradeDataFast = false;
				_dirtyTalentDataFast = false;
				Debug.Log((object)"[GameDataManager] 合并上传当前世界存档");
				SaveCurrentWorld2Server(delegate(bool success, string response)
				{
					if (!success)
					{
						isDataDirtyForCloudSync = true;
						_dirtyFullSaveFast = true;
					}
				});
			}
		}
		saveToServerTimer += Time.deltaTime;
		if (!(saveToServerTimer >= 300f))
		{
			return;
		}
		saveToServerTimer = 0f;
		if (!isDataDirtyForCloudSync)
		{
			return;
		}
		Debug.Log((object)"[GameDataManager] 5分钟兜底定时器触发，上传当前世界存档");
		SaveCurrentWorld2Server(delegate(bool success, string response)
		{
			if (success)
			{
				Debug.Log((object)"[GameDataManager] 定时上传成功！");
			}
			else
			{
				Debug.LogWarning((object)("[GameDataManager] 定时上传失败: " + response));
			}
		});
	}

	public void UpdateServerTime(DateTime serverTime)
	{
		ServerTime = serverTime;
	}

	private string GetWorldOutboxFilePath(int worldId)
	{
		return Path.Combine(Application.persistentDataPath, $"world_server_outbox_{Math.Max(1, worldId)}.json");
	}

	private void SyncWorldCupCompactDataToTalentData()
	{
		if (_currentTalentData != null && _currentWorldCupData != null)
		{
			_currentTalentData.worldCupCompactData = WorldCupCompactData.FromWorldCupData(_currentWorldCupData);
		}
	}

	private WorldCupCompactData GetWorldCupCompactDataForServer()
	{
		if (_currentWorldCupData == null)
		{
			return null;
		}
		WorldCupCompactData worldCupCompactData = WorldCupCompactData.FromWorldCupData(_currentWorldCupData);
		if (!worldCupCompactData.HasData)
		{
			return null;
		}
		return worldCupCompactData;
	}

	private bool TryRestoreWorldCupFromCompact(WorldCupCompactData compact, string source)
	{
		if (compact == null || !compact.HasData)
		{
			return false;
		}
		_currentWorldCupData = compact.ToWorldCupData();
		SyncWorldCupCompactDataToTalentData();
		_worldCupDataNeedsLocalRewrite = true;
		Debug.Log((object)$"[GameDataManager] 已从{source}恢复世界杯紧凑存档，活动第{_currentWorldCupData.signDay}天");
		return true;
	}

	private bool TryRestoreWorldCupFromServerTalent(ServerData serverData)
	{
		if (serverData == null)
		{
			return false;
		}
		WorldCupCompactData compact = serverData.talentDataSave?.talentData?.worldCupCompactData;
		if (TryRestoreWorldCupFromCompact(compact, "服务器 TalentData"))
		{
			return true;
		}
		List<WorldData> list = serverData.gameDataSave?.worldDataCollection?.worlds;
		if (list == null)
		{
			return false;
		}
		int worldId = ((_globalProfileData == null) ? 1 : _globalProfileData.currentWorldId);
		WorldData worldData = list.Find((WorldData item) => item != null && item.worldId == worldId);
		if (worldData == null)
		{
			worldData = list.Find((WorldData item) => item?.talentData?.worldCupCompactData != null && item.talentData.worldCupCompactData.HasData);
		}
		return TryRestoreWorldCupFromCompact(worldData?.talentData?.worldCupCompactData, "服务器世界 TalentData");
	}

	public DateTime GetLoginTime()
	{
		if (_currentLoginTime == DateTime.MinValue && PlayerPrefs.HasKey("LastLoginTime"))
		{
			DateTime.TryParse(PlayerPrefs.GetString("LastLoginTime", ""), out _currentLoginTime);
		}
		return _currentLoginTime;
	}

	public bool IsOfflineRewardProtected()
	{
		if (!OfflineIncomeManager.HasInstance() || !OfflineIncomeManager.Instance.HasOfflineReward())
		{
			return false;
		}
		if (!_playerActivelyEntered)
		{
			return true;
		}
		return (DateTime.Now - _appStartTime).TotalSeconds < 600.0;
	}

	private void UpdateLoginTime()
	{
		_currentLoginTime = DateTime.Now;
		PlayerPrefs.SetString("LastLoginTime", _currentLoginTime.ToString());
		PlayerPrefs.Save();
		if (_currentGameData == null)
		{
			return;
		}
		_currentGameData.lastEnterGameTime = (long)(_currentLoginTime - new DateTime(1970, 1, 1)).TotalMilliseconds;
		int[] todayDateNumber = GetTodayDateNumber();
		if (_currentGameData.todayMonth != todayDateNumber[0] || _currentGameData.today != todayDateNumber[1])
		{
			_currentGameData.todayMonth = todayDateNumber[0];
			_currentGameData.today = todayDateNumber[1];
			int checkInDay = _currentGameData.checkInDay;
			checkInDay++;
			if (checkInDay > 7)
			{
				checkInDay = 1;
				ResetClampItem(_currentGameData);
			}
			SetCheckInDay(checkInDay);
		}
		UpdateAllWorldsCheckInDay(todayDateNumber);
		WorldCupDataManager.Instance.UpdateWorldCupSignDay();
		WorldCupDataManager.Instance.UpdateWorldCupScoreCrossDay();
	}

	public GameData GetGameData()
	{
		return _currentGameData;
	}

	public GlobalProfileData GetGlobalProfileData()
	{
		EnsureWorldPersistenceInitialized();
		return _globalProfileData;
	}

	public WorldDataCollection GetWorldDataCollection()
	{
		EnsureWorldPersistenceInitialized();
		return _worldDataCollection;
	}

	public int GetCurrentWorldId()
	{
		EnsureWorldPersistenceInitialized();
		if (_globalProfileData == null)
		{
			return 1;
		}
		return _globalProfileData.currentWorldId;
	}

	public WorldData GetCurrentWorldData()
	{
		return FindWorldData(GetCurrentWorldId());
	}

	public WorldData GetWorldData(int worldId)
	{
		EnsureWorldPersistenceInitialized();
		return FindWorldData(worldId);
	}

	public int GetWorldMainMissionLevel(int worldId)
	{
		EnsureWorldPersistenceInitialized();
		if (worldId == GetCurrentWorldId())
		{
			if (_currentGameData == null)
			{
				return 0;
			}
			return _currentGameData.currentMainMissionLevel;
		}
		WorldData worldData = FindWorldData(worldId);
		if (worldData == null || worldData.gameData == null)
		{
			return 0;
		}
		return worldData.gameData.currentMainMissionLevel;
	}

	public bool IsHelperOwnedInWorld(int worldId, int helperId)
	{
		EnsureWorldPersistenceInitialized();
		if (worldId == GetCurrentWorldId())
		{
			return IsHelperOwned(helperId);
		}
		WorldData worldData = FindWorldData(worldId);
		if (worldData != null && worldData.facilityData != null && worldData.facilityData.helperOwned != null)
		{
			return worldData.facilityData.helperOwned.Exists((HelperInfo helper) => helper != null && helper.helperId == helperId);
		}
		return false;
	}

	public DiceWorldProgressData GetCurrentDiceProgress()
	{
		WorldData currentWorldData = GetCurrentWorldData();
		if (currentWorldData == null)
		{
			return new DiceWorldProgressData();
		}
		WorldData worldData = currentWorldData;
		if (worldData.diceProgress == null)
		{
			worldData.diceProgress = new DiceWorldProgressData();
		}
		currentWorldData.diceProgress.Normalize();
		return currentWorldData.diceProgress;
	}

	public void SetDiceFaceWeightMultiplier(int face, float multiplier)
	{
		if (face >= 1 && face <= 6)
		{
			GetCurrentDiceProgress().faceWeightMultipliers[face - 1] = Math.Max(0f, multiplier);
			SaveWorldDataOnly();
			MarkDataDirty();
		}
	}

	public void SetDiceValueMultiplier(float multiplier)
	{
		GetCurrentDiceProgress().valueMultiplier = Math.Max(0f, multiplier);
		SaveWorldDataOnly();
		MarkDataDirty();
	}

	public bool IsWorldUnlocked(int worldId)
	{
		EnsureWorldPersistenceInitialized();
		if (_globalProfileData == null)
		{
			return false;
		}
		if (_globalProfileData.unlockedWorldIds.Contains(worldId))
		{
			return true;
		}
		if (!ConfigManager.HasInstance())
		{
			return false;
		}
		world worldConfig = ConfigManager.Instance.GetWorldConfig(worldId);
		if (worldConfig == null)
		{
			return false;
		}
		if (worldConfig.PreWorldId != 0)
		{
			WorldData worldData = FindWorldData(worldConfig.PreWorldId);
			if (((worldData != null && worldData.gameData != null) ? worldData.gameData.currentMainMissionLevel : 0) < worldConfig.RequiredPreWorldLevel)
			{
				return false;
			}
		}
		_globalProfileData.unlockedWorldIds.Add(worldId);
		GetOrCreateWorldData(worldId);
		MarkDataDirty();
		SaveWorldDataOnly();
		return true;
	}

	public bool IsWorldEntryUnlocked()
	{
		EnsureWorldPersistenceInitialized();
		if (_globalProfileData == null || _worldDataCollection == null)
		{
			return false;
		}
		if (_globalProfileData.worldEntryUnlocked)
		{
			return true;
		}
		int worldEntryUnlockLevel = GetWorldEntryUnlockLevel();
		foreach (WorldData world in _worldDataCollection.worlds)
		{
			if (world != null && world.gameData != null && world.gameData.currentMainMissionLevel > worldEntryUnlockLevel)
			{
				_globalProfileData.worldEntryUnlocked = true;
				MarkDataDirty();
				SaveWorldDataOnly();
				return true;
			}
		}
		return false;
	}

	public int GetWorldEntryUnlockLevel()
	{
		if (!ConfigManager.HasInstance())
		{
			return 10;
		}
		return (int)ConfigManager.Instance.GetCommonFactor(59);
	}

	public int GetHighestWorldMainMissionLevel()
	{
		int val = 0;
		if (_worldDataCollection != null && _worldDataCollection.worlds != null)
		{
			foreach (WorldData world in _worldDataCollection.worlds)
			{
				if (world != null && world.gameData != null)
				{
					val = Math.Max(val, world.gameData.currentMainMissionLevel);
				}
			}
		}
		return Math.Max(val, (_currentGameData != null) ? _currentGameData.currentMainMissionLevel : 0);
	}

	public bool TrySwitchWorld(int worldId, out string error)
	{
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		EnsureWorldPersistenceInitialized();
		if (_isSwitchingWorld)
		{
			error = "世界切换中";
			return false;
		}
		if (!TryUnlockWorldIfEligible(worldId, out error))
		{
			return false;
		}
		if (worldId == GetCurrentWorldId())
		{
			return true;
		}
		_isSwitchingWorld = true;
		_worldSwitchCloudSyncInProgress = true;
		int previousWorldId = GetCurrentWorldId();
		long switchTimeMs = GetCurrentLocalUnixTimeMs();
		long targetWorldLastExitTime = GetWorldLastExitTime(worldId);
		Debug.Log((object)$"[GameDataManager] Switch world {previousWorldId}->{worldId}, switchTime={switchTimeMs}, targetLastExitBeforeSwitch={targetWorldLastExitTime}");
		if (worldId > 1)
		{
			P8SDK.LevelUpgrade(new LevelUpgradeOption
			{
				level = $"world_{worldId}",
				level_status = "0"
			});
		}
		if (UIManager.HasInstance())
		{
			UIManager.Instance.PlayCutsceneTransition(delegate(Action onCovered)
			{
				BeginServerWorldSwitch(previousWorldId, worldId, switchTimeMs, refreshActiveWorldManually: true, onCovered);
			}, delegate
			{
				_isSwitchingWorld = false;
				if (OfflineIncomeManager.HasInstance())
				{
					OfflineIncomeManager.Instance.CheckOfflineIncomeAfterWorldSwitch(targetWorldLastExitTime);
				}
			});
			return true;
		}
		BeginServerWorldSwitch(previousWorldId, worldId, switchTimeMs, refreshActiveWorldManually: false, delegate
		{
			if (OfflineIncomeManager.HasInstance())
			{
				OfflineIncomeManager.Instance.CheckOfflineIncomeAfterWorldSwitch(targetWorldLastExitTime);
			}
		});
		return true;
	}

	private void BeginServerWorldSwitch(int previousWorldId, int targetWorldId, long switchTimeMs, bool refreshActiveWorldManually, Action onCompleted)
	{
		if (_worldSaveInFlight || _worldOutboxRetriesInFlight.Contains(previousWorldId) || _worldOutboxRetriesInFlight.Contains(targetWorldId))
		{
			((MonoBehaviour)this).StartCoroutine(WaitForWorldSaveThenBeginSwitch(previousWorldId, targetWorldId, switchTimeMs, refreshActiveWorldManually, onCompleted));
			return;
		}
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null && SingletonMono<UpgradeManager>.Instance.IsInitialized)
		{
			SingletonMono<UpgradeManager>.Instance.FlushToSaveData();
		}
		MarkWorldOfflineStart(previousWorldId, switchTimeMs);
		CaptureCurrentWorldData();
		SaveWorldDataOnly();
		string snapshotJson = BuildWorldServerDataJson(previousWorldId);
		if (!((Object)(object)SingletonMono<HttpManager>.Instance != (Object)null) || !HttpManager.CanSendServerRequests || string.IsNullOrEmpty(GetOpenId()))
		{
			CompleteServerWorldSwitch(previousWorldId, targetWorldId, switchTimeMs, refreshActiveWorldManually, onCompleted);
			return;
		}
		WriteFile(GetWorldOutboxFilePath(previousWorldId), snapshotJson);
		SingletonMono<HttpManager>.Instance.UpdateWorldData(GetOpenId(), previousWorldId, snapshotJson, delegate(bool saveSuccess, string response)
		{
			if (saveSuccess)
			{
				DeleteWorldOutboxIfUnchanged(previousWorldId, snapshotJson);
			}
			else
			{
				Debug.LogWarning((object)$"[GameDataManager] World {previousWorldId} snapshot queued for retry: {response}");
			}
			LoadTargetWorld();
		});
		void LoadTargetWorld()
		{
			if ((Object)(object)SingletonMono<HttpManager>.Instance == (Object)null || !HttpManager.CanSendServerRequests || string.IsNullOrEmpty(GetOpenId()))
			{
				CompleteServerWorldSwitch(previousWorldId, targetWorldId, switchTimeMs, refreshActiveWorldManually, onCompleted);
			}
			else
			{
				bool fileExists;
				string json = ReadFile(GetWorldOutboxFilePath(targetWorldId), out fileExists);
				if (fileExists && TryApplyPendingWorldOutbox(json, targetWorldId))
				{
					Debug.LogWarning((object)$"[GameDataManager] Using pending local outbox for world {targetWorldId} after retry did not clear it.");
					CompleteServerWorldSwitch(previousWorldId, targetWorldId, switchTimeMs, refreshActiveWorldManually, onCompleted);
				}
				else
				{
					SingletonMono<HttpManager>.Instance.GetWorldData(GetOpenId(), targetWorldId, delegate(bool loadSuccess, string response)
					{
						if (loadSuccess)
						{
							if (!TryApplyWorldServerResponse(response, targetWorldId, restoreGlobal: false))
							{
								Debug.LogWarning((object)$"[GameDataManager] Invalid world {targetWorldId} response; using local data.");
							}
						}
						else
						{
							Debug.LogWarning((object)$"[GameDataManager] Failed to load world {targetWorldId}; using local data: {response}");
						}
						CompleteServerWorldSwitch(previousWorldId, targetWorldId, switchTimeMs, refreshActiveWorldManually, onCompleted);
					});
				}
			}
		}
	}

	private IEnumerator WaitForWorldSaveThenBeginSwitch(int previousWorldId, int targetWorldId, long switchTimeMs, bool refreshActiveWorldManually, Action onCompleted)
	{
		while (_worldSaveInFlight || _worldOutboxRetriesInFlight.Contains(previousWorldId) || _worldOutboxRetriesInFlight.Contains(targetWorldId))
		{
			yield return null;
		}
		BeginServerWorldSwitch(previousWorldId, targetWorldId, switchTimeMs, refreshActiveWorldManually, onCompleted);
	}

	private void CompleteServerWorldSwitch(int previousWorldId, int targetWorldId, long switchTimeMs, bool refreshActiveWorldManually, Action onCompleted)
	{
		ExecuteWorldSwitch(previousWorldId, targetWorldId, switchTimeMs, refreshActiveWorldManually, delegate
		{
			_isSwitchingWorld = false;
			_worldSwitchCloudSyncInProgress = false;
			_worldSavePending = false;
			SaveCurrentWorld2Server();
			onCompleted?.Invoke();
		});
	}

	private void ExecuteWorldSwitch(int previousWorldId, int worldId, long switchTimeMs, bool refreshActiveWorldManually, Action onWorldInitialized)
	{
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null && SingletonMono<UpgradeManager>.Instance.IsInitialized)
		{
			SingletonMono<UpgradeManager>.Instance.FlushToSaveData();
		}
		_globalProfileData.currentWorldId = worldId;
		BindCurrentWorld(worldId);
		MarkWorldEnter(worldId, switchTimeMs);
		MarkDataDirty();
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null && SingletonMono<UpgradeManager>.Instance.IsInitialized)
		{
			SingletonMono<UpgradeManager>.Instance.LoadData();
			SingletonMono<UpgradeManager>.Instance.RebuildCurrentWorldDeskBuffs();
		}
		MissionManager.Instance?.ReloadCurrentWorld();
		SaveWorldDataOnly();
		this.OnGameDataUpdated?.Invoke();
		this.OnCoinItemDiamondUpdated?.Invoke();
		if (refreshActiveWorldManually && UIManager.HasInstance())
		{
			UIManager.Instance.SuppressNextWorldChangedRefresh();
		}
		this.OnWorldChanged?.Invoke(previousWorldId, worldId);
		TalentDataManager.Instance?.DisplayTalentEnterRoPint(0.0);
		if (refreshActiveWorldManually && UIManager.HasInstance())
		{
			UIManager.Instance.RefreshActiveWorld(onWorldInitialized);
		}
		else
		{
			onWorldInitialized?.Invoke();
		}
	}

	private static long GetCurrentLocalUnixTimeMs()
	{
		return (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
	}

	private void MarkWorldOfflineStart(int worldId, long unixTimeMs)
	{
		WorldData orCreateWorldData = GetOrCreateWorldData(worldId);
		if (orCreateWorldData.gameData != null)
		{
			orCreateWorldData.gameData.lastExitGameTime = unixTimeMs;
		}
		orCreateWorldData.lastExitTime = unixTimeMs;
		if (worldId == GetCurrentWorldId() && _currentGameData != null)
		{
			_currentGameData.lastExitGameTime = unixTimeMs;
		}
	}

	private void MarkWorldEnter(int worldId, long unixTimeMs)
	{
		WorldData orCreateWorldData = GetOrCreateWorldData(worldId);
		if (orCreateWorldData.gameData != null)
		{
			orCreateWorldData.gameData.lastEnterGameTime = unixTimeMs;
		}
		orCreateWorldData.lastEnterTime = unixTimeMs;
		if (worldId == GetCurrentWorldId() && _currentGameData != null)
		{
			_currentGameData.lastEnterGameTime = unixTimeMs;
		}
	}

	private long GetWorldLastExitTime(int worldId)
	{
		WorldData worldData = FindWorldData(worldId);
		if (worldData == null)
		{
			return 0L;
		}
		long lastExitTime = worldData.lastExitTime;
		long num = ((worldData.gameData != null) ? worldData.gameData.lastExitGameTime : 0);
		if (lastExitTime <= 0)
		{
			return num;
		}
		if (num <= 0)
		{
			return lastExitTime;
		}
		return Math.Min(lastExitTime, num);
	}

	private bool TryUnlockWorldIfEligible(int worldId, out string error)
	{
		error = string.Empty;
		if (_globalProfileData == null || _worldDataCollection == null)
		{
			error = "世界数据尚未初始化";
			return false;
		}
		if (IsWorldUnlocked(worldId))
		{
			return true;
		}
		if (!ConfigManager.HasInstance())
		{
			error = "世界配置尚未加载";
			return false;
		}
		world worldConfig = ConfigManager.Instance.GetWorldConfig(worldId);
		if (worldConfig == null)
		{
			error = "未找到该关卡配置";
			return false;
		}
		if (worldConfig.PreWorldId != 0)
		{
			WorldData worldData = FindWorldData(worldConfig.PreWorldId);
			if (((worldData != null && worldData.gameData != null) ? worldData.gameData.currentMainMissionLevel : 0) < worldConfig.RequiredPreWorldLevel)
			{
				world worldConfig2 = ConfigManager.Instance.GetWorldConfig(worldConfig.PreWorldId);
				string arg = ((worldConfig2 != null) ? worldConfig2.WorldName : "前置关卡");
				error = $"{arg}关卡达到{worldConfig.RequiredPreWorldLevel}级解锁";
				return false;
			}
		}
		_globalProfileData.unlockedWorldIds.Add(worldId);
		GetOrCreateWorldData(worldId);
		MarkDataDirty();
		return true;
	}

	private void EnsureWorldPersistenceInitialized()
	{
		if ((_globalProfileData == null || _worldDataCollection == null || _worldDataCollection.worlds == null) && _currentGameData != null && _currentFacilityData != null && _currentUpgradeData != null && _currentTalentData != null)
		{
			InitializeWorldPersistence();
		}
	}

	private void InitializeWorldPersistence()
	{
		bool flag = TryLoadWorldArchive();
		bool flag2 = flag || _loadedGlobalProfileFromGameData || TryLoadGlobalProfileData();
		bool flag3 = flag || _loadedWorldCollectionFromGameData || TryLoadWorldDataCollection();
		bool flag4 = !flag && (!flag2 || !flag3);
		Debug.Log((object)($"[GameDataManager] World persistence: archive={flag}, global={flag2}, worlds={flag3}, " + $"embeddedGlobal={_loadedGlobalProfileFromGameData}, embeddedWorlds={_loadedWorldCollectionFromGameData}, migrateLegacy={flag4}"));
		if (flag4)
		{
			MigrateLegacyDataToWorldOne();
		}
		if (_globalProfileData == null)
		{
			_globalProfileData = new GlobalProfileData();
		}
		if (_worldDataCollection == null)
		{
			_worldDataCollection = new WorldDataCollection();
		}
		if (_worldDataCollection.worlds == null)
		{
			_worldDataCollection.worlds = new List<WorldData>();
		}
		if (_globalProfileData.unlockedWorldIds == null || _globalProfileData.unlockedWorldIds.Count == 0)
		{
			_globalProfileData.unlockedWorldIds = new List<int> { 1 };
		}
		if (!IsWorldUnlocked(1))
		{
			_globalProfileData.unlockedWorldIds.Add(1);
		}
		if (FindWorldData(_globalProfileData.currentWorldId) == null)
		{
			_globalProfileData.currentWorldId = 1;
		}
		if (_globalProfileData.globalHelperSkills == null)
		{
			_globalProfileData.globalHelperSkills = new List<GlobalHelperSkillData>();
		}
		if (_globalProfileData.globalHelperSkills.Count == 0 && _globalProfileData.helperOwned != null && _globalProfileData.helperOwned.Count > 0)
		{
			Debug.Log((object)"[GameDataManager] 检测到旧存档帮手数据，正在迁移 skillLevel 至全局列表...");
			foreach (HelperInfo item in _globalProfileData.helperOwned)
			{
				if (item != null)
				{
					_globalProfileData.globalHelperSkills.Add(new GlobalHelperSkillData
					{
						helperId = item.helperId,
						skillLevel = Math.Max(1, item.helperLevel)
					});
				}
			}
		}
		bool flag5 = EnsureGlobalIdentity();
		MergeInviteHelpDataFromAllWorlds();
		BindCurrentWorld(_globalProfileData.currentWorldId);
		if (flag4 || !flag || flag5)
		{
			SaveWorldDataOnly();
		}
	}

	private bool TryLoadWorldArchive()
	{
		bool fileExists;
		string text = ReadFile(WorldArchiveFilePath, out fileExists);
		if (!fileExists)
		{
			return false;
		}
		try
		{
			WorldArchiveSave worldArchiveSave = JsonUtility.FromJson<WorldArchiveSave>(text);
			if (worldArchiveSave == null || worldArchiveSave.globalProfileData == null || worldArchiveSave.worldDataCollection == null || worldArchiveSave.worldDataCollection.worlds == null)
			{
				return false;
			}
			_globalProfileData = worldArchiveSave.globalProfileData;
			_worldDataCollection = worldArchiveSave.worldDataCollection;
			_worldArchiveRevision = Math.Max(0L, worldArchiveSave.revision);
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("[GameDataManager] Failed to load world archive: " + ex.Message));
			return false;
		}
	}

	private bool TryLoadGlobalProfileData()
	{
		bool fileExists;
		string text = ReadFile(GlobalProfileDataFilePath, out fileExists);
		if (!fileExists)
		{
			return false;
		}
		try
		{
			GlobalProfileDataSave globalProfileDataSave = JsonUtility.FromJson<GlobalProfileDataSave>(text);
			if (globalProfileDataSave != null && globalProfileDataSave.globalProfileData != null)
			{
				_globalProfileData = globalProfileDataSave.globalProfileData;
				return true;
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("[GameDataManager] Failed to load global profile: " + ex.Message));
		}
		return false;
	}

	private bool TryLoadWorldDataCollection()
	{
		bool fileExists;
		string text = ReadFile(WorldDataCollectionFilePath, out fileExists);
		if (!fileExists)
		{
			return false;
		}
		try
		{
			WorldDataCollectionSave worldDataCollectionSave = JsonUtility.FromJson<WorldDataCollectionSave>(text);
			if (worldDataCollectionSave != null && worldDataCollectionSave.worldDataCollection != null && worldDataCollectionSave.worldDataCollection.worlds != null)
			{
				_worldDataCollection = worldDataCollectionSave.worldDataCollection;
				return true;
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("[GameDataManager] Failed to load world data: " + ex.Message));
		}
		return false;
	}

	private void MigrateLegacyDataToWorldOne()
	{
		_currentGameData = _currentGameData ?? new GameData();
		_currentFacilityData = _currentFacilityData ?? new FacilityData();
		_currentUpgradeData = _currentUpgradeData ?? new UpgradeData();
		_currentTalentData = _currentTalentData ?? new TalentData();
		List<HelperInfo> list = _currentFacilityData.helperOwned ?? new List<HelperInfo>();
		List<GlobalHelperSkillData> list2 = new List<GlobalHelperSkillData>();
		foreach (HelperInfo item in list)
		{
			if (item != null)
			{
				list2.Add(new GlobalHelperSkillData
				{
					helperId = item.helperId,
					skillLevel = Math.Max(1, item.helperLevel)
				});
			}
		}
		_globalProfileData = new GlobalProfileData
		{
			localUserId = _currentGameData.localUserId,
			openId = _currentGameData.openId,
			nickName = _currentGameData.nickName,
			avatarUrl = _currentGameData.avatarUrl,
			diamonds = _currentGameData.diamonds,
			itemList = (_currentGameData.itemList ?? new List<GameItem>()),
			helperOwned = list,
			globalHelperSkills = list2,
			inviteTime = _currentGameData.InviteTime,
			helpedTime = _currentGameData.HelpedTime,
			currentWorldId = 1,
			unlockedWorldIds = new List<int> { 1 }
		};
		_worldDataCollection = new WorldDataCollection();
		_worldDataCollection.worlds.Add(new WorldData
		{
			worldId = 1,
			gameData = _currentGameData,
			facilityData = _currentFacilityData,
			upgradeData = _currentUpgradeData,
			talentData = _currentTalentData,
			bestScore = _currentGameData.currentMainMissionLevel,
			lastEnterTime = _currentGameData.lastEnterGameTime,
			lastExitTime = _currentGameData.lastExitGameTime
		});
	}

	private bool EnsureGlobalIdentity()
	{
		bool result = false;
		if (_globalProfileData == null)
		{
			_globalProfileData = new GlobalProfileData();
			result = true;
		}
		string text = _globalProfileData.localUserId;
		if (string.IsNullOrEmpty(text))
		{
			text = _currentGameData?.localUserId;
		}
		if (string.IsNullOrEmpty(text) && _worldDataCollection?.worlds != null)
		{
			foreach (WorldData world in _worldDataCollection.worlds)
			{
				if (!string.IsNullOrEmpty(world?.gameData?.localUserId))
				{
					text = world.gameData.localUserId;
					break;
				}
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			text = LocalUserId.GetExisting();
		}
		if (string.IsNullOrEmpty(text))
		{
			text = LocalUserId.Get();
		}
		if (!string.Equals(_globalProfileData.localUserId, text, StringComparison.Ordinal))
		{
			_globalProfileData.localUserId = text;
			result = true;
		}
		if (!string.Equals(LocalUserId.GetExisting(), text, StringComparison.Ordinal))
		{
			LocalUserId.Set(text);
			result = true;
		}
		if (_currentGameData != null && !string.Equals(_currentGameData.localUserId, text, StringComparison.Ordinal))
		{
			_currentGameData.localUserId = text;
			result = true;
		}
		if (_worldDataCollection?.worlds != null)
		{
			foreach (WorldData world2 in _worldDataCollection.worlds)
			{
				if (world2?.gameData != null && !string.Equals(world2.gameData.localUserId, text, StringComparison.Ordinal))
				{
					world2.gameData.localUserId = text;
					result = true;
				}
			}
		}
		return result;
	}

	private WorldData FindWorldData(int worldId)
	{
		if (_worldDataCollection == null || _worldDataCollection.worlds == null)
		{
			return null;
		}
		return _worldDataCollection.worlds.Find((WorldData data) => data != null && data.worldId == worldId);
	}

	private WorldData GetOrCreateWorldData(int worldId)
	{
		WorldData worldData = FindWorldData(worldId);
		if (worldData != null)
		{
			return worldData;
		}
		worldData = new WorldData
		{
			worldId = worldId
		};
		EnsureWorldDeskId(worldData);
		if (worldData.gameData != null)
		{
			int[] todayDateNumber = GetTodayDateNumber();
			worldData.gameData.todayMonth = todayDateNumber[0];
			worldData.gameData.today = todayDateNumber[1];
			if (worldData.gameData.checkInDay <= 0)
			{
				worldData.gameData.checkInDay = 1;
			}
		}
		_worldDataCollection.worlds.Add(worldData);
		return worldData;
	}

	private void EnsureWorldDeskId(WorldData world)
	{
		if (world == null)
		{
			return;
		}
		if (world.gameData == null)
		{
			world.gameData = new GameData();
		}
		if (!ConfigManager.HasInstance())
		{
			return;
		}
		desk deskConfig = ConfigManager.Instance.GetDeskConfig(world.gameData.currentDeskId);
		if (deskConfig != null && deskConfig.WorldId == world.worldId)
		{
			return;
		}
		List<desk> allDeskByWorld = ConfigManager.Instance.GetAllDeskByWorld(world.worldId);
		if (allDeskByWorld != null && allDeskByWorld.Count != 0)
		{
			desk desk = allDeskByWorld.Find((desk config) => config.DeskUnlockCondition == -1) ?? allDeskByWorld[0];
			world.gameData.currentDeskId = desk.Id;
		}
	}

	private void BindCurrentWorld(int worldId)
	{
		WorldData orCreateWorldData = GetOrCreateWorldData(worldId);
		EnsureWorldDeskId(orCreateWorldData);
		WorldData worldData = orCreateWorldData;
		if (worldData.diceProgress == null)
		{
			worldData.diceProgress = new DiceWorldProgressData();
		}
		orCreateWorldData.diceProgress.Normalize();
		_currentGameData = orCreateWorldData.gameData ?? new GameData();
		_currentFacilityData = orCreateWorldData.facilityData ?? new FacilityData();
		_currentUpgradeData = orCreateWorldData.upgradeData ?? new UpgradeData();
		_currentTalentData = orCreateWorldData.talentData ?? new TalentData();
		SynchronizeCurrentWorldFromGlobal();
		CaptureCurrentWorldData();
	}

	private void SynchronizeCurrentWorldFromGlobal()
	{
		if (_globalProfileData == null || _currentGameData == null || _currentFacilityData == null)
		{
			return;
		}
		_currentGameData.localUserId = _globalProfileData.localUserId;
		_currentGameData.openId = _globalProfileData.openId;
		_currentGameData.nickName = _globalProfileData.nickName;
		_currentGameData.avatarUrl = _globalProfileData.avatarUrl;
		_currentGameData.diamonds = _globalProfileData.diamonds;
		_currentGameData.itemList = _globalProfileData.itemList ?? new List<GameItem>();
		MergeInviteHelpDataFromGameData(_currentGameData);
		_currentGameData.InviteTime = _globalProfileData.inviteTime;
		_currentGameData.HelpedTime = _globalProfileData.helpedTime;
		if (_currentFacilityData.helperOwned == null)
		{
			_currentFacilityData.helperOwned = new List<HelperInfo>();
		}
		if (_globalProfileData.globalHelperSkills == null)
		{
			return;
		}
		foreach (GlobalHelperSkillData globalSkill in _globalProfileData.globalHelperSkills)
		{
			HelperInfo helperInfo = _currentFacilityData.helperOwned.Find((HelperInfo h) => h.helperId == globalSkill.helperId);
			if (helperInfo != null)
			{
				int val = Math.Max(helperInfo.helperLevel, globalSkill.skillLevel);
				helperInfo.helperLevel = Math.Max(1, val);
				helperInfo.skillLevel = GetAwakeSkillLevelFromHelperLevel(helperInfo.helperId, helperInfo.helperLevel);
			}
			else if (globalSkill.helperId == 9 || globalSkill.helperId == 10)
			{
				int helperLevel = Math.Max(1, globalSkill.skillLevel);
				_currentFacilityData.helperOwned.Add(new HelperInfo
				{
					helperId = globalSkill.helperId,
					helperLevel = helperLevel,
					helperCount = 0,
					maxCount = 1,
					isAdUnlock = 0,
					skillLevel = GetAwakeSkillLevelFromHelperLevel(globalSkill.helperId, helperLevel)
				});
			}
		}
	}

	private void SynchronizeGlobalFromCurrentWorld()
	{
		if (_currentGameData == null || _currentFacilityData == null)
		{
			return;
		}
		if (_globalProfileData == null)
		{
			_globalProfileData = new GlobalProfileData();
		}
		_currentGameData.localUserId = _globalProfileData.localUserId;
		_currentGameData.openId = _globalProfileData.openId;
		_globalProfileData.nickName = _currentGameData.nickName;
		_globalProfileData.avatarUrl = _currentGameData.avatarUrl;
		_globalProfileData.diamonds = _currentGameData.diamonds;
		_globalProfileData.itemList = _currentGameData.itemList ?? new List<GameItem>();
		MergeInviteHelpDataFromGameData(_currentGameData);
		if (_globalProfileData.globalHelperSkills == null)
		{
			_globalProfileData.globalHelperSkills = new List<GlobalHelperSkillData>();
		}
		if (_currentFacilityData.helperOwned == null)
		{
			return;
		}
		foreach (HelperInfo helper in _currentFacilityData.helperOwned)
		{
			if (helper == null)
			{
				continue;
			}
			GlobalHelperSkillData globalHelperSkillData = _globalProfileData.globalHelperSkills.Find((GlobalHelperSkillData g) => g.helperId == helper.helperId);
			if (globalHelperSkillData != null)
			{
				if (helper.helperLevel > globalHelperSkillData.skillLevel)
				{
					globalHelperSkillData.skillLevel = helper.helperLevel;
				}
			}
			else
			{
				_globalProfileData.globalHelperSkills.Add(new GlobalHelperSkillData
				{
					helperId = helper.helperId,
					skillLevel = Math.Max(1, helper.helperLevel)
				});
			}
		}
	}

	private void MergeInviteHelpDataFromGameData(GameData gameData)
	{
		if (_globalProfileData != null && gameData != null)
		{
			_globalProfileData.inviteTime = Math.Max(_globalProfileData.inviteTime, gameData.InviteTime);
			_globalProfileData.helpedTime = MergeHelpedTime(_globalProfileData.helpedTime, gameData.HelpedTime);
		}
	}

	private static long MergeHelpedTime(long left, long right)
	{
		if (left > 0 || right > 0)
		{
			return Math.Max(left, right);
		}
		if (left == -1 || right == -1)
		{
			return -1L;
		}
		return 0L;
	}

	private void MergeInviteHelpDataFromAllWorlds()
	{
		if (_globalProfileData == null || _worldDataCollection == null || _worldDataCollection.worlds == null)
		{
			return;
		}
		foreach (WorldData world in _worldDataCollection.worlds)
		{
			if (world?.gameData != null)
			{
				MergeInviteHelpDataFromGameData(world.gameData);
			}
		}
	}

	private void CaptureCurrentWorldData()
	{
		EnsureWorldPersistenceInitialized();
		if (_worldDataCollection != null && _globalProfileData != null)
		{
			SynchronizeGlobalFromCurrentWorld();
			WorldData orCreateWorldData = GetOrCreateWorldData(_globalProfileData.currentWorldId);
			orCreateWorldData.gameData = _currentGameData;
			orCreateWorldData.facilityData = _currentFacilityData;
			orCreateWorldData.upgradeData = _currentUpgradeData;
			orCreateWorldData.talentData = _currentTalentData;
			orCreateWorldData.bestScore = ((_currentGameData != null) ? _currentGameData.currentMainMissionLevel : 0);
			orCreateWorldData.lastEnterTime = ((_currentGameData != null) ? _currentGameData.lastEnterGameTime : 0);
			orCreateWorldData.lastExitTime = ((_currentGameData != null) ? _currentGameData.lastExitGameTime : 0);
		}
	}

	public void SaveWorldDataOnly()
	{
		EnsureWorldPersistenceInitialized();
		if (!skipSave && _globalProfileData != null && _worldDataCollection != null)
		{
			CaptureCurrentWorldData();
			string saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
			GlobalProfileDataSave globalProfileDataSave = new GlobalProfileDataSave
			{
				globalProfileData = _globalProfileData,
				saveTime = saveTime
			};
			WorldDataCollectionSave worldDataCollectionSave = new WorldDataCollectionSave
			{
				worldDataCollection = _worldDataCollection,
				saveTime = saveTime
			};
			WorldArchiveSave archive = new WorldArchiveSave
			{
				revision = ++_worldArchiveRevision,
				savedAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
				globalProfileData = _globalProfileData,
				worldDataCollection = _worldDataCollection
			};
			WriteWorldArchive(archive);
			WriteFile(GlobalProfileDataFilePath, JsonUtility.ToJson((object)globalProfileDataSave, true));
			WriteFile(WorldDataCollectionFilePath, JsonUtility.ToJson((object)worldDataCollectionSave, true));
		}
	}

	private void WriteWorldArchive(WorldArchiveSave archive)
	{
		string jsonData = JsonUtility.ToJson((object)archive, true);
		WriteFile(WorldArchiveFilePath, jsonData);
	}

	private bool TryRestoreWorldData(GameDataSave save)
	{
		if (save == null || save.globalProfileData == null || save.worldDataCollection == null || save.worldDataCollection.worlds == null)
		{
			return false;
		}
		int currentWorldId = GetCurrentWorldId();
		_globalProfileData = save.globalProfileData;
		_worldDataCollection = save.worldDataCollection;
		if (_globalProfileData.currentWorldId <= 0 || FindWorldData(_globalProfileData.currentWorldId) == null)
		{
			_globalProfileData.currentWorldId = 1;
		}
		if (_globalProfileData.unlockedWorldIds == null)
		{
			_globalProfileData.unlockedWorldIds = new List<int> { 1 };
		}
		if (!IsWorldUnlocked(1))
		{
			_globalProfileData.unlockedWorldIds.Add(1);
		}
		EnsureGlobalIdentity();
		BindCurrentWorld(_globalProfileData.currentWorldId);
		SaveWorldDataOnly();
		this.OnWorldChanged?.Invoke(currentWorldId, _globalProfileData.currentWorldId);
		return true;
	}

	public UpgradeData GetUpgradeData()
	{
		return _currentUpgradeData;
	}

	public FacilityData GetFacilityData()
	{
		return _currentFacilityData;
	}

	public void SetGameData(GameData gameData)
	{
		_currentGameData = gameData;
		WorldData currentWorldData = GetCurrentWorldData();
		if (currentWorldData != null)
		{
			currentWorldData.gameData = gameData;
			SynchronizeGlobalFromCurrentWorld();
		}
		this.OnGameDataUpdated?.Invoke();
	}

	public string GetUserId()
	{
		EnsureWorldPersistenceInitialized();
		if (_globalProfileData != null)
		{
			EnsureGlobalIdentity();
		}
		return _globalProfileData?.localUserId ?? "";
	}

	public string GetOpenId()
	{
		if (!string.IsNullOrEmpty(_globalProfileData?.openId))
		{
			return _globalProfileData.openId;
		}
		return _currentGameData?.openId ?? "";
	}

	public string GetSessionKey()
	{
		return _currentSessionKey;
	}

	public void SetOpenId(string openId, string sessionKey = "")
	{
		EnsureWorldPersistenceInitialized();
		if (_globalProfileData == null)
		{
			_globalProfileData = new GlobalProfileData();
		}
		if (_currentGameData == null)
		{
			_currentGameData = new GameData();
		}
		_globalProfileData.openId = openId;
		_currentGameData.openId = openId;
		_currentSessionKey = sessionKey;
		saveToServerTimer = 0f;
		MarkDataDirty();
		if (!string.IsNullOrEmpty(openId) && !_cloudSyncSuspended)
		{
			if (_firstCloudSaveCoroutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(_firstCloudSaveCoroutine);
			}
			_firstCloudSaveCoroutine = ((MonoBehaviour)this).StartCoroutine(DelayFirstCloudSave());
		}
	}

	public void SetCloudSyncSuspended(bool suspended)
	{
		if (_cloudSyncSuspended == suspended)
		{
			return;
		}
		_cloudSyncSuspended = suspended;
		if (suspended)
		{
			if (_firstCloudSaveCoroutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(_firstCloudSaveCoroutine);
				_firstCloudSaveCoroutine = null;
			}
			Debug.Log((object)"[GameDataManager] Cloud sync suspended.");
			return;
		}
		Debug.Log((object)"[GameDataManager] Cloud sync resumed.");
		saveToServerTimer = 0f;
		if (!string.IsNullOrEmpty(GetOpenId()) && isDataDirtyForCloudSync)
		{
			if (_firstCloudSaveCoroutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(_firstCloudSaveCoroutine);
			}
			_firstCloudSaveCoroutine = ((MonoBehaviour)this).StartCoroutine(DelayFirstCloudSave());
		}
	}

	private IEnumerator DelayFirstCloudSave()
	{
		yield return (object)new WaitForSeconds(20f);
		if (_cloudSyncSuspended)
		{
			_firstCloudSaveCoroutine = null;
			yield break;
		}
		if (string.IsNullOrEmpty(GetOpenId()))
		{
			_firstCloudSaveCoroutine = null;
			yield break;
		}
		if (!isDataDirtyForCloudSync)
		{
			_firstCloudSaveCoroutine = null;
			yield break;
		}
		Debug.Log((object)"[GameDataManager] 获取openId后触发首次全量云存档");
		SaveAll2Server(delegate(bool success, string response)
		{
			if (success)
			{
				saveToServerTimer = 0f;
			}
			else
			{
				Debug.LogWarning((object)("[GameDataManager] 首次全量云存档失败: " + response));
				MarkDataDirty();
			}
		});
		_firstCloudSaveCoroutine = null;
	}

	public void SetUserInfo(TTUserInfo user)
	{
		_currentGameData.nickName = user.nickName;
		_currentGameData.avatarUrl = user.avatarUrl;
	}

	public long GetHelpedTime()
	{
		EnsureWorldPersistenceInitialized();
		if (_globalProfileData != null)
		{
			return _globalProfileData.helpedTime;
		}
		return _currentGameData?.HelpedTime ?? 0;
	}

	public long GetInviteTime()
	{
		EnsureWorldPersistenceInitialized();
		if (_globalProfileData != null)
		{
			return _globalProfileData.inviteTime;
		}
		return _currentGameData?.InviteTime ?? 0;
	}

	public void SetHelpedTime(long helpedTime, bool save = true)
	{
		EnsureWorldPersistenceInitialized();
		if (_globalProfileData == null)
		{
			_globalProfileData = new GlobalProfileData();
		}
		_globalProfileData.helpedTime = helpedTime;
		if (_currentGameData != null)
		{
			_currentGameData.HelpedTime = helpedTime;
		}
		if (helpedTime == 0L && _worldDataCollection != null && _worldDataCollection.worlds != null)
		{
			foreach (WorldData world in _worldDataCollection.worlds)
			{
				if (world != null && world.gameData != null)
				{
					world.gameData.HelpedTime = 0L;
				}
			}
		}
		if (save)
		{
			SaveGameDataOnly();
		}
	}

	public void SetInviteTime(long inviteTime, bool save = true)
	{
		EnsureWorldPersistenceInitialized();
		if (_globalProfileData == null)
		{
			_globalProfileData = new GlobalProfileData();
		}
		_globalProfileData.inviteTime = inviteTime;
		if (_currentGameData != null)
		{
			_currentGameData.InviteTime = inviteTime;
		}
		if (save)
		{
			SaveGameDataOnly();
		}
	}

	public double GetAverageCoin()
	{
		return _currentGameData?.averageCoin ?? 0.0;
	}

	public double GetTotalGold()
	{
		return _currentGameData?.totalGold ?? 0.0;
	}

	public int GetGuideStep()
	{
		if (_currentGameData.Guide_Step == 0)
		{
			_currentGameData.Guide_Step = 1;
		}
		return _currentGameData.Guide_Step;
	}

	public void SetGuideStep(int step)
	{
		_currentGameData.Guide_Step = step;
		SaveGameDataOnly();
	}

	private void WriteFile(string filePath, string jsonData)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		try
		{
			if ((int)Application.platform == 50)
			{
				TTFileSystemManager fileSystemManager = TT.GetFileSystemManager();
				string text = filePath + ".bak";
				try
				{
					string text2 = fileSystemManager.ReadFileSync(filePath, "utf8");
					if (!string.IsNullOrEmpty(text2))
					{
						fileSystemManager.WriteFileSync(text, text2, "utf8");
					}
				}
				catch (Exception)
				{
				}
				fileSystemManager.WriteFileSync(filePath, jsonData, "utf8");
			}
			else
			{
				File.WriteAllText(filePath, jsonData, Encoding.UTF8);
			}
		}
		catch (Exception ex2)
		{
			Debug.LogError((object)("写入文件失败 " + filePath + ": " + ex2.Message + "\n" + ex2.StackTrace));
		}
	}

	private string ReadFile(string filePath, out bool fileExists)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		string text = string.Empty;
		fileExists = false;
		try
		{
			if ((int)Application.platform == 50)
			{
				TTFileSystemManager fileSystemManager = TT.GetFileSystemManager();
				string text2 = filePath + ".bak";
				try
				{
					text = fileSystemManager.ReadFileSync(filePath, "utf8");
					if (!IsValidJson(text))
					{
						throw new Exception("主文件JSON格式不完整或损坏");
					}
					fileExists = true;
				}
				catch (Exception ex)
				{
					Debug.LogWarning((object)("读取主文件失败，尝试读取备份文件: " + ex.Message));
					try
					{
						text = fileSystemManager.ReadFileSync(text2, "utf8");
						if (IsValidJson(text))
						{
							fileExists = true;
							Debug.Log((object)("已成功从备份文件恢复数据: " + text2));
						}
					}
					catch (Exception)
					{
					}
				}
			}
			else if (File.Exists(filePath))
			{
				text = File.ReadAllText(filePath, Encoding.UTF8);
				fileExists = true;
			}
		}
		catch (Exception)
		{
			fileExists = false;
		}
		return text;
	}

	private bool IsValidJson(string json)
	{
		if (string.IsNullOrEmpty(json))
		{
			return false;
		}
		string text = json.Trim().TrimStart('\ufeff').Trim();
		if (text.StartsWith("{"))
		{
			return text.EndsWith("}");
		}
		return false;
	}

	private string GetJsonPreview(string json)
	{
		if (string.IsNullOrEmpty(json))
		{
			return "<empty>";
		}
		char c = json[0];
		char c2 = json[json.Length - 1];
		return $"first=U+{(int)c:X4}, last=U+{(int)c2:X4}, bom={c == '\ufeff'}";
	}

	public void MarkDataDirty()
	{
		if (!isDataDirtyForCloudSync)
		{
			Debug.Log((object)"[GameDataManager] 数据已变更，标记为脏数据，等待下次定时上传。");
		}
		_cloudDirtyGeneration++;
		isDataDirtyForCloudSync = true;
		_dirtyFullSaveFast = true;
	}

	private void ClearCloudDirtyIfNoPendingFastSave()
	{
		if (!_dirtyFullSaveFast && !_dirtyGameDataFast && !_dirtyFacilityDataFast && !_dirtyUpgradeDataFast && !_dirtyTalentDataFast)
		{
			isDataDirtyForCloudSync = false;
		}
	}

	public void SaveGameData(bool isClear = false, bool updateExitTime = true)
	{
		if (skipSave)
		{
			return;
		}
		try
		{
			if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null && SingletonMono<UpgradeManager>.Instance.IsInitialized)
			{
				SingletonMono<UpgradeManager>.Instance.FlushToSaveData(clearDirtyState: true);
			}
			if (_currentGameData == null)
			{
				Debug.Log((object)"重置GameData onSave");
				_currentGameData = new GameData();
			}
			CaptureCurrentWorldData();
			DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
			_currentGameData.lastEnterGameTime = (long)(_currentLoginTime - new DateTime(1970, 1, 1)).TotalMilliseconds;
			bool flag = updateExitTime;
			if (updateExitTime && IsOfflineRewardProtected())
			{
				flag = false;
			}
			if (flag)
			{
				long lastExitGameTime = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
				_currentGameData.lastExitGameTime = lastExitGameTime;
			}
			SaveGameDataOnly(flag);
			SaveFacilityDataOnly();
			SaveUpgradeDataOnly();
			SaveTalentDataOnly();
			SaveWorldCupDataOnly();
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("保存游戏数据失败: " + ex.Message + "\n" + ex.StackTrace));
		}
	}

	public PlayerPrefsData GetPlayerPrefsData()
	{
		return playerPrefsData;
	}

	public void SavePlayerPrefsDataOnly(bool isWrite = false, bool markDirty = true)
	{
		if (markDirty)
		{
			MarkDataDirty();
		}
		try
		{
			string saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
			playerPrefsData = new PlayerPrefsData
			{
				saveTime = saveTime,
				guide_step_1 = PlayerPrefs.GetInt("guide_step_1", 0),
				guide_step_2 = PlayerPrefs.GetInt("guide_step_2", 0),
				guide_step_3 = PlayerPrefs.GetInt("guide_step_3", 0),
				guide_step_4 = PlayerPrefs.GetInt("guide_step_4", 0),
				guide_step_5 = PlayerPrefs.GetInt("guide_step_5", 0),
				guide_step_6 = PlayerPrefs.GetInt("guide_step_6", 0),
				dice_world_guide_step = PlayerPrefs.GetInt("dice_world_guide_step", 0),
				guide_step_4_progress = PlayerPrefs.GetInt("guide_step_4_progress", 0),
				firstReset = PlayerPrefs.GetInt(Constants.firstReset, 0),
				SideBAR_HAS_GET_M1B = PlayerPrefs.GetInt("SideBAR_HAS_GET_M1B", 0),
				Subscribe_HAS_GET_M1B = PlayerPrefs.GetInt("Subscribe_HAS_GET_M1B", 0),
				ADD_DESK_HAS_GET = PlayerPrefs.GetInt("ADD_DESK_HAS_GET", 0),
				gongfu_support_time = PlayerPrefs.GetInt("gongfu_support_time", 0),
				dashe_support_time = PlayerPrefs.GetInt("dashe_support_time", 0),
				gongfu_popup_triggered = PlayerPrefs.GetInt("gongfu_popup_triggered", 0),
				dashe_popup_triggered = PlayerPrefs.GetInt("dashe_popup_triggered", 0),
				gongfu_ad_state = PlayerPrefs.GetInt("gongfu_ad_state", (PlayerPrefs.GetInt("gongfu_support_time", 0) == 1) ? 2 : 0),
				dashe_ad_state = PlayerPrefs.GetInt("dashe_ad_state", (PlayerPrefs.GetInt("dashe_support_time", 0) == 1) ? 2 : 0),
				gongfu_ad_start_time = PlayerPrefs.GetString("gongfu_ad_start_time", ""),
				dashe_ad_start_time = PlayerPrefs.GetString("dashe_ad_start_time", "")
			};
			int[] array = new int[5] { 990001, 990002, 201, 202, 203 };
			playerPrefsData.shop = new int[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				string key = ShopDailyLimit.GetKey(array[i]);
				playerPrefsData.shop[i] = PlayerPrefs.GetInt(key, 0);
			}
			if (isWrite)
			{
				string jsonData = JsonUtility.ToJson((object)new PlayerPrefsDataSave
				{
					playerPrefsData = playerPrefsData,
					saveTime = saveTime
				}, true);
				WriteFile(PlayerPrefsDataFilePath, jsonData);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("保存PlayerPrefsData失败: " + ex.Message + "\n" + ex.StackTrace));
		}
	}

	public void LoadPlayerPrefsDataFromFile(PlayerPrefsDataSave playerPrefsSave)
	{
		if (playerPrefsSave == null || playerPrefsSave.playerPrefsData == null)
		{
			Debug.LogWarning((object)"传入的PlayerPrefsDataSave为空，无法写入");
			return;
		}
		try
		{
			PlayerPrefsData playerPrefsData = playerPrefsSave.playerPrefsData;
			RestoreGuideStepIfNewer("guide_step_1", playerPrefsData.guide_step_1);
			RestoreGuideStepIfNewer("guide_step_2", playerPrefsData.guide_step_2);
			RestoreGuideStepIfNewer("guide_step_3", playerPrefsData.guide_step_3);
			RestoreGuideStepIfNewer("guide_step_4", playerPrefsData.guide_step_4);
			RestoreGuideStepIfNewer("guide_step_5", playerPrefsData.guide_step_5);
			RestoreGuideStepIfNewer("guide_step_6", playerPrefsData.guide_step_6);
			RestoreGuideStepIfNewer("dice_world_guide_step", playerPrefsData.dice_world_guide_step);
			PlayerPrefs.SetInt("guide_step_4_progress", Mathf.Max(PlayerPrefs.GetInt("guide_step_4_progress", 0), playerPrefsData.guide_step_4_progress));
			if (playerPrefsData.firstReset > 0 || !PlayerPrefs.HasKey(Constants.firstReset))
			{
				PlayerPrefs.SetInt(Constants.firstReset, playerPrefsData.firstReset);
			}
			PlayerPrefs.SetInt("SideBAR_HAS_GET_M1B", playerPrefsData.SideBAR_HAS_GET_M1B);
			PlayerPrefs.SetInt("Subscribe_HAS_GET_M1B", playerPrefsData.Subscribe_HAS_GET_M1B);
			PlayerPrefs.SetInt("ADD_DESK_HAS_GET", playerPrefsData.ADD_DESK_HAS_GET);
			PlayerPrefs.SetInt("gongfu_support_time", playerPrefsData.gongfu_support_time);
			PlayerPrefs.SetInt("dashe_support_time", playerPrefsData.dashe_support_time);
			PlayerPrefs.SetInt("gongfu_popup_triggered", playerPrefsData.gongfu_popup_triggered);
			PlayerPrefs.SetInt("dashe_popup_triggered", playerPrefsData.dashe_popup_triggered);
			PlayerPrefs.SetInt("gongfu_ad_state", playerPrefsData.gongfu_ad_state);
			PlayerPrefs.SetInt("dashe_ad_state", playerPrefsData.dashe_ad_state);
			PlayerPrefs.SetString("gongfu_ad_start_time", playerPrefsData.gongfu_ad_start_time ?? "");
			PlayerPrefs.SetString("dashe_ad_start_time", playerPrefsData.dashe_ad_start_time ?? "");
			if (playerPrefsData.shop != null)
			{
				int[] array = new int[1] { 990001 };
				for (int i = 0; i < Mathf.Min(playerPrefsData.shop.Length, array.Length); i++)
				{
					PlayerPrefs.SetInt(ShopDailyLimit.GetKey(array[i]), playerPrefsData.shop[i]);
				}
			}
			PlayerPrefs.Save();
			Debug.Log((object)"PlayerPrefsData从传入对象写入成功");
			ForceCompleteGuideStepsByLevel();
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("写入PlayerPrefsData失败: " + ex.Message + "\n" + ex.StackTrace));
		}
	}

	private void RestoreGuideStepIfNewer(string key, int cloudValue)
	{
		if (GetCurrentMainMissionLevel() >= 3)
		{
			PlayerPrefs.SetInt(key, 1);
		}
		else if (cloudValue == 1 || !PlayerPrefs.HasKey(key))
		{
			PlayerPrefs.SetInt(key, cloudValue);
		}
	}

	private void ForceCompleteGuideStepsByLevel()
	{
		int currentMainMissionLevel = GetCurrentMainMissionLevel();
		if (currentMainMissionLevel < 3)
		{
			return;
		}
		bool flag = false;
		for (int i = 1; i <= 6; i++)
		{
			string text = $"guide_step_{i}";
			if (PlayerPrefs.GetInt(text, 0) != 1)
			{
				PlayerPrefs.SetInt(text, 1);
				flag = true;
			}
		}
		if (PlayerPrefs.GetInt("dice_world_guide_step", 0) != 1)
		{
			PlayerPrefs.SetInt("dice_world_guide_step", 1);
			flag = true;
		}
		if (PlayerPrefs.GetInt("guide_step_4_progress", 0) < 100)
		{
			PlayerPrefs.SetInt("guide_step_4_progress", 100);
			flag = true;
		}
		if (_currentGameData != null && _currentGameData.Guide_Step < 7)
		{
			_currentGameData.Guide_Step = 7;
			flag = true;
		}
		if (flag)
		{
			PlayerPrefs.Save();
			Debug.Log((object)$"[GameDataManager] 玩家等级 {currentMainMissionLevel} >= 3，已强制完成所有新手引导步骤");
		}
	}

	private bool LoadPlayerPrefsDataFromFile()
	{
		bool fileExists;
		string text = ReadFile(PlayerPrefsDataFilePath, out fileExists);
		if (fileExists)
		{
			try
			{
				PlayerPrefsDataSave playerPrefsSave = JsonUtility.FromJson<PlayerPrefsDataSave>(text);
				LoadPlayerPrefsDataFromFile(playerPrefsSave);
				return true;
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("PlayerPrefsData解析失败: " + ex.Message + "\n" + ex.StackTrace));
			}
		}
		return false;
	}

	public void SaveAll2Server(Action<bool, string> callback = null)
	{
		SaveCurrentWorld2Server(callback);
	}

	private string BuildWorldServerDataJson(int worldId)
	{
		EnsureWorldPersistenceInitialized();
		WorldData orCreateWorldData = GetOrCreateWorldData(worldId);
		string saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
		SavePlayerPrefsDataOnly(isWrite: false, markDirty: false);
		return JsonUtility.ToJson((object)new WorldServerData
		{
			openId = GetOpenId(),
			worldId = worldId,
			globalProfileDataSave = new GlobalProfileDataSave
			{
				globalProfileData = _globalProfileData,
				saveTime = saveTime
			},
			worldDataSave = new WorldDataSave
			{
				worldData = orCreateWorldData,
				saveTime = saveTime
			},
			playerPrefsDataSave = new PlayerPrefsDataSave
			{
				playerPrefsData = playerPrefsData,
				saveTime = saveTime
			},
			worldCupCompactData = GetWorldCupCompactDataForServer()
		});
	}

	private void DeleteWorldOutboxIfUnchanged(int worldId, string uploadedJson)
	{
		string worldOutboxFilePath = GetWorldOutboxFilePath(worldId);
		bool fileExists;
		string text = ReadFile(worldOutboxFilePath, out fileExists);
		if (!fileExists || text != uploadedJson)
		{
			return;
		}
		try
		{
			TTFileSystemManager fileSystemManager = TT.GetFileSystemManager();
			fileSystemManager.UnlinkSync(worldOutboxFilePath);
			try
			{
				fileSystemManager.UnlinkSync(worldOutboxFilePath + ".bak");
			}
			catch (Exception)
			{
			}
		}
		catch (Exception ex2)
		{
			Debug.LogWarning((object)$"[GameDataManager] Failed to clear world outbox {worldId}: {ex2.Message}");
		}
	}

	public void RetryPendingWorldSaves()
	{
		if (_cloudSyncSuspended || _worldSwitchCloudSyncInProgress || (Object)(object)SingletonMono<HttpManager>.Instance == (Object)null || !HttpManager.CanSendServerRequests || string.IsNullOrEmpty(GetOpenId()))
		{
			return;
		}
		EnsureWorldPersistenceInitialized();
		HashSet<int> hashSet = new HashSet<int>
		{
			1,
			2,
			GetCurrentWorldId()
		};
		if (_worldDataCollection?.worlds != null)
		{
			foreach (WorldData world in _worldDataCollection.worlds)
			{
				if (world != null && world.worldId > 0)
				{
					hashSet.Add(world.worldId);
				}
			}
		}
		foreach (int item in hashSet)
		{
			RetryWorldOutbox(item);
		}
	}

	private void RetryWorldOutbox(int worldId)
	{
		if (_worldOutboxRetriesInFlight.Contains(worldId))
		{
			return;
		}
		bool fileExists;
		string json = ReadFile(GetWorldOutboxFilePath(worldId), out fileExists);
		if (!fileExists || string.IsNullOrWhiteSpace(json))
		{
			return;
		}
		WorldServerData worldServerData;
		try
		{
			worldServerData = JsonUtility.FromJson<WorldServerData>(json);
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)$"[GameDataManager] Invalid world outbox {worldId}: {ex.Message}");
			return;
		}
		if (worldServerData != null && worldServerData.worldId == worldId)
		{
			WorldDataSave worldDataSave = worldServerData.worldDataSave;
			if (worldDataSave != null && worldDataSave.worldData?.worldId == worldId && string.Equals(worldServerData.openId, GetOpenId(), StringComparison.Ordinal))
			{
				_worldOutboxRetriesInFlight.Add(worldId);
				SingletonMono<HttpManager>.Instance.UpdateWorldData(GetOpenId(), worldId, json, delegate(bool success, string response)
				{
					_worldOutboxRetriesInFlight.Remove(worldId);
					if (success)
					{
						DeleteWorldOutboxIfUnchanged(worldId, json);
					}
					else
					{
						Debug.LogWarning((object)$"[GameDataManager] World outbox retry failed for {worldId}: {response}");
					}
				});
				return;
			}
		}
		Debug.LogWarning((object)$"[GameDataManager] Ignoring mismatched world outbox {worldId}.");
	}

	private bool TryApplyPendingWorldOutbox(string json, int expectedWorldId)
	{
		if (string.IsNullOrWhiteSpace(json))
		{
			return false;
		}
		try
		{
			WorldServerData worldServerData = JsonUtility.FromJson<WorldServerData>(json);
			if (worldServerData != null && worldServerData.worldId == expectedWorldId)
			{
				WorldDataSave worldDataSave = worldServerData.worldDataSave;
				if (worldDataSave != null && worldDataSave.worldData?.worldId == expectedWorldId && string.Equals(worldServerData.openId, GetOpenId(), StringComparison.Ordinal))
				{
					return TryApplyWorldServerResponse(json, expectedWorldId, restoreGlobal: false);
				}
			}
			return false;
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)$"[GameDataManager] Invalid pending world outbox {expectedWorldId}: {ex.Message}");
			return false;
		}
	}

	private bool TryApplyWorldServerResponse(string json, int expectedWorldId, bool restoreGlobal)
	{
		if (string.IsNullOrWhiteSpace(json))
		{
			return false;
		}
		try
		{
			WorldServerData worldServerData = JsonUtility.FromJson<WorldServerData>(json);
			if (worldServerData?.worldDataSave?.worldData == null)
			{
				return false;
			}
			WorldData worldData = worldServerData.worldDataSave.worldData;
			if (worldServerData.worldId != expectedWorldId || worldData.worldId != expectedWorldId)
			{
				Debug.LogError((object)$"[GameDataManager] World response mismatch: expected={expectedWorldId}, response={worldServerData.worldId}, payload={worldData.worldId}");
				return false;
			}
			EnsureWorldPersistenceInitialized();
			int num = _worldDataCollection.worlds.FindIndex((WorldData world) => world != null && world.worldId == expectedWorldId);
			if (num >= 0)
			{
				_worldDataCollection.worlds[num] = worldData;
			}
			else
			{
				_worldDataCollection.worlds.Add(worldData);
			}
			if (IsWorldCupDataLoadError)
			{
				bool flag = TryRestoreWorldCupFromCompact(worldServerData.worldCupCompactData, "服务器世界切换快照");
				if (!flag)
				{
					flag = TryRestoreWorldCupFromCompact(worldData.talentData?.worldCupCompactData, "服务器世界切换 TalentData");
				}
				if (flag)
				{
					SaveWorldCupDataOnly();
					IsWorldCupDataLoadError = false;
				}
			}
			if (restoreGlobal && worldServerData.globalProfileDataSave?.globalProfileData != null)
			{
				_globalProfileData = worldServerData.globalProfileDataSave.globalProfileData;
			}
			if (restoreGlobal && worldServerData.playerPrefsDataSave?.playerPrefsData != null)
			{
				LoadPlayerPrefsDataFromFile(worldServerData.playerPrefsDataSave);
			}
			EnsureGlobalIdentity();
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("[GameDataManager] Failed to parse world server response: " + ex.Message));
			return false;
		}
	}

	public bool RestoreWorldFromServerResponse(string json, out string error)
	{
		bool cloudSyncSuspended = _cloudSyncSuspended;
		bool flag = false;
		SetCloudSyncSuspended(suspended: true);
		try
		{
			if (string.IsNullOrWhiteSpace(json))
			{
				error = "服务器存档为空";
				return false;
			}
			WorldServerData serverData = JsonUtility.FromJson<WorldServerData>(json);
			WorldData worldData = serverData?.worldDataSave?.worldData;
			GlobalProfileData globalProfileData = serverData?.globalProfileDataSave?.globalProfileData;
			if (serverData == null || worldData == null || globalProfileData == null)
			{
				error = "服务器存档结构不完整";
				return false;
			}
			if (serverData.worldId <= 0 || worldData.worldId != serverData.worldId)
			{
				error = "服务器存档的世界编号不一致";
				return false;
			}
			if (!string.IsNullOrEmpty(serverData.openId) && !string.IsNullOrEmpty(GetOpenId()) && !string.Equals(serverData.openId, GetOpenId(), StringComparison.Ordinal))
			{
				error = "服务器存档不属于当前玩家";
				return false;
			}
			EnsureWorldPersistenceInitialized();
			string openId = GetOpenId();
			int currentWorldId = GetCurrentWorldId();
			_globalProfileData = globalProfileData;
			_globalProfileData.currentWorldId = serverData.worldId;
			if (!string.IsNullOrEmpty(openId))
			{
				_globalProfileData.openId = openId;
			}
			GlobalProfileData globalProfileData2 = _globalProfileData;
			if (globalProfileData2.unlockedWorldIds == null)
			{
				globalProfileData2.unlockedWorldIds = new List<int>();
			}
			if (!_globalProfileData.unlockedWorldIds.Contains(1))
			{
				_globalProfileData.unlockedWorldIds.Add(1);
			}
			if (!_globalProfileData.unlockedWorldIds.Contains(serverData.worldId))
			{
				_globalProfileData.unlockedWorldIds.Add(serverData.worldId);
			}
			if (_worldDataCollection == null)
			{
				_worldDataCollection = new WorldDataCollection();
			}
			WorldDataCollection worldDataCollection = _worldDataCollection;
			if (worldDataCollection.worlds == null)
			{
				worldDataCollection.worlds = new List<WorldData>();
			}
			int num = _worldDataCollection.worlds.FindIndex((WorldData world) => world != null && world.worldId == serverData.worldId);
			if (num >= 0)
			{
				_worldDataCollection.worlds[num] = worldData;
			}
			else
			{
				_worldDataCollection.worlds.Add(worldData);
			}
			EnsureGlobalIdentity();
			BindCurrentWorld(serverData.worldId);
			if (!TryRestoreWorldCupFromCompact(serverData.worldCupCompactData, "服务器世界快照"))
			{
				TryRestoreWorldCupFromCompact(worldData.talentData?.worldCupCompactData, "服务器世界 TalentData");
			}
			if (serverData.playerPrefsDataSave?.playerPrefsData != null)
			{
				playerPrefsData = serverData.playerPrefsDataSave.playerPrefsData;
				LoadPlayerPrefsDataFromFile(serverData.playerPrefsDataSave);
			}
			SingletonMono<UpgradeManager>.Instance?.LoadData();
			SingletonMono<UpgradeManager>.Instance?.RebuildCurrentWorldDeskBuffs();
			MissionManager.Instance?.ReloadCurrentWorld();
			SaveGameData(isClear: false, updateExitTime: false);
			_worldCupDataNeedsLocalRewrite = false;
			bool fileExists;
			string uploadedJson = ReadFile(GetWorldOutboxFilePath(serverData.worldId), out fileExists);
			if (fileExists)
			{
				DeleteWorldOutboxIfUnchanged(serverData.worldId, uploadedJson);
			}
			originalLastExitTime = ((_currentGameData != null) ? _currentGameData.lastExitGameTime : 0);
			IsLocalSaveMissing = 0;
			IsGameDataLoadError = false;
			IsFacilityDataLoadError = false;
			IsUpgradeDataLoadError = false;
			IsTalentDataLoadError = false;
			IsWorldCupDataLoadError = false;
			this.OnGameDataUpdated?.Invoke();
			this.OnCoinItemDiamondUpdated?.Invoke();
			if (currentWorldId != serverData.worldId)
			{
				this.OnWorldChanged?.Invoke(currentWorldId, serverData.worldId);
			}
			flag = true;
			error = null;
			return true;
		}
		catch (Exception ex)
		{
			error = ex.Message;
			Debug.LogError((object)("[GameDataManager] Restore world archive failed: " + ex.Message + "\n" + ex.StackTrace));
			return false;
		}
		finally
		{
			if (!cloudSyncSuspended)
			{
				SetCloudSyncSuspended(suspended: false);
			}
			else if (flag)
			{
				Debug.Log((object)"[GameDataManager] World restore completed while cloud sync remains suspended for caller validation.");
			}
		}
	}

	public void SaveCurrentWorld2Server(Action<bool, string> callback = null)
	{
		try
		{
			bool canSendServerRequests = HttpManager.CanSendServerRequests;
			if (canSendServerRequests && _cloudSyncSuspended)
			{
				callback?.Invoke(arg1: false, "Cloud sync suspended");
				return;
			}
			if (canSendServerRequests && _worldSwitchCloudSyncInProgress)
			{
				_worldSavePending = true;
				callback?.Invoke(arg1: true, "World switch cloud sync in progress; queued latest state");
				return;
			}
			if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null && SingletonMono<UpgradeManager>.Instance.IsInitialized)
			{
				SingletonMono<UpgradeManager>.Instance.FlushToSaveData();
			}
			SyncWorldCupCompactDataToTalentData();
			if (canSendServerRequests && string.IsNullOrEmpty(GetOpenId()))
			{
				Debug.LogError((object)"用户ID为空，无法保存数据到服务器");
				callback?.Invoke(arg1: false, "用户ID为空");
				return;
			}
			string saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
			if (_currentGameData != null)
			{
				_currentGameData.lastEnterGameTime = (long)(_currentLoginTime - new DateTime(1970, 1, 1)).TotalMilliseconds;
				if (!IsOfflineRewardProtected())
				{
					long lastExitGameTime = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
					_currentGameData.lastExitGameTime = lastExitGameTime;
				}
			}
			CaptureCurrentWorldData();
			int worldId = GetCurrentWorldId();
			WorldData currentWorldData = GetCurrentWorldData();
			SavePlayerPrefsDataOnly(isWrite: false, markDirty: false);
			if (!canSendServerRequests)
			{
				SaveWorldDataOnly();
				callback?.Invoke(arg1: true, "Local-only platform save completed");
				return;
			}
			WorldServerData worldServerData = new WorldServerData
			{
				openId = GetOpenId(),
				worldId = worldId,
				globalProfileDataSave = new GlobalProfileDataSave
				{
					globalProfileData = _globalProfileData,
					saveTime = saveTime
				},
				worldDataSave = new WorldDataSave
				{
					worldData = currentWorldData,
					saveTime = saveTime
				},
				playerPrefsDataSave = new PlayerPrefsDataSave
				{
					playerPrefsData = playerPrefsData,
					saveTime = saveTime
				},
				worldCupCompactData = GetWorldCupCompactDataForServer()
			};
			string jsonData = JsonUtility.ToJson((object)worldServerData);
			WriteFile(GetWorldOutboxFilePath(worldId), jsonData);
			long saveGeneration = _cloudDirtyGeneration;
			if (_worldSaveInFlight)
			{
				_worldSavePending = true;
				callback?.Invoke(arg1: true, "World save already in flight; queued latest state");
				return;
			}
			_worldSaveInFlight = true;
			Action<bool, string> action = delegate(bool success, string response)
			{
				_worldSaveInFlight = false;
				if (success)
				{
					DeleteWorldOutboxIfUnchanged(worldId, jsonData);
					if (_cloudDirtyGeneration == saveGeneration)
					{
						isDataDirtyForCloudSync = false;
						_dirtyFullSaveFast = false;
						_dirtyGameDataFast = false;
						_dirtyFacilityDataFast = false;
						_dirtyUpgradeDataFast = false;
						_dirtyTalentDataFast = false;
					}
					else
					{
						isDataDirtyForCloudSync = true;
						_dirtyFullSaveFast = true;
					}
				}
				callback?.Invoke(success, response);
				if (_worldSavePending)
				{
					_worldSavePending = false;
					SaveCurrentWorld2Server();
				}
			};
			if (Application.isEditor)
			{
				if ((Object)(object)SingletonMono<HttpManager>.Instance != (Object)null)
				{
					SingletonMono<HttpManager>.Instance.UpdateWorldData(worldServerData.openId, worldId, jsonData, action);
					return;
				}
				string text = Path.Combine(Application.persistentDataPath, "server_data_test.json");
				File.WriteAllText(text, jsonData, Encoding.UTF8);
				Debug.Log((object)("[GameDataManager] Editor模式：合并数据已保存到本地: " + text));
				action(arg1: true, "Editor模式保存成功");
			}
			else if ((Object)(object)SingletonMono<HttpManager>.Instance != (Object)null)
			{
				SingletonMono<HttpManager>.Instance.UpdateWorldData(worldServerData.openId, worldId, jsonData, action);
			}
			else
			{
				Debug.LogError((object)"[GameDataManager] HttpManager 未初始化");
				action(arg1: false, "HttpManager 未初始化");
			}
		}
		catch (Exception ex)
		{
			_worldSaveInFlight = false;
			Debug.LogError((object)("[GameDataManager] SaveCurrentWorld2Server 失败: " + ex.Message + "\n" + ex.StackTrace));
			callback?.Invoke(arg1: false, ex.Message);
		}
	}

	public void SaveGameData2Server(Action<bool, string> callback = null)
	{
		try
		{
			if (_cloudSyncSuspended)
			{
				callback?.Invoke(arg1: false, "Cloud sync suspended");
				return;
			}
			if (string.IsNullOrEmpty(GetOpenId()))
			{
				callback?.Invoke(arg1: false, "用户ID为空");
				return;
			}
			string saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
			if (_currentGameData != null && !IsOfflineRewardProtected())
			{
				long lastExitGameTime = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
				_currentGameData.lastExitGameTime = lastExitGameTime;
			}
			CaptureCurrentWorldData();
			string jsonData = JsonUtility.ToJson((object)new GameDataSave
			{
				gameData = _currentGameData,
				globalProfileData = _globalProfileData,
				worldDataCollection = _worldDataCollection,
				saveTime = saveTime
			});
			if (Application.isEditor)
			{
				callback?.Invoke(arg1: true, "Editor模式保存成功");
			}
			else if ((Object)(object)SingletonMono<HttpManager>.Instance != (Object)null)
			{
				SingletonMono<HttpManager>.Instance.UpdateGameData(GetOpenId(), jsonData, callback);
			}
			else
			{
				callback?.Invoke(arg1: false, "HttpManager 未初始化");
			}
		}
		catch (Exception ex)
		{
			callback?.Invoke(arg1: false, ex.Message);
		}
	}

	public void SaveFacilityData2Server(Action<bool, string> callback = null)
	{
		try
		{
			if (_cloudSyncSuspended)
			{
				callback?.Invoke(arg1: false, "Cloud sync suspended");
				return;
			}
			if (string.IsNullOrEmpty(GetOpenId()))
			{
				callback?.Invoke(arg1: false, "用户ID为空");
				return;
			}
			string saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
			string jsonData = JsonUtility.ToJson((object)new FacilityDataSave
			{
				facilityData = _currentFacilityData,
				saveTime = saveTime
			});
			if (Application.isEditor)
			{
				callback?.Invoke(arg1: true, "Editor模式保存成功");
			}
			else if ((Object)(object)SingletonMono<HttpManager>.Instance != (Object)null)
			{
				SingletonMono<HttpManager>.Instance.UpdateFacilityData(GetOpenId(), jsonData, callback);
			}
			else
			{
				callback?.Invoke(arg1: false, "HttpManager 未初始化");
			}
		}
		catch (Exception ex)
		{
			callback?.Invoke(arg1: false, ex.Message);
		}
	}

	public void SaveUpgradeData2Server(Action<bool, string> callback = null)
	{
		try
		{
			if (_cloudSyncSuspended)
			{
				callback?.Invoke(arg1: false, "Cloud sync suspended");
				return;
			}
			if (string.IsNullOrEmpty(GetOpenId()))
			{
				callback?.Invoke(arg1: false, "用户ID为空");
				return;
			}
			string saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
			string jsonData = JsonUtility.ToJson((object)new UpgradeDataSave
			{
				upgradeData = _currentUpgradeData,
				saveTime = saveTime
			});
			if (Application.isEditor)
			{
				callback?.Invoke(arg1: true, "Editor模式保存成功");
			}
			else if ((Object)(object)SingletonMono<HttpManager>.Instance != (Object)null)
			{
				SingletonMono<HttpManager>.Instance.UpdateUpgradeData(GetOpenId(), jsonData, callback);
			}
			else
			{
				callback?.Invoke(arg1: false, "HttpManager 未初始化");
			}
		}
		catch (Exception ex)
		{
			callback?.Invoke(arg1: false, ex.Message);
		}
	}

	public void SavePlayerPrefsData2Server(Action<bool, string> callback = null)
	{
		try
		{
			if (_cloudSyncSuspended)
			{
				callback?.Invoke(arg1: false, "Cloud sync suspended");
				return;
			}
			if (string.IsNullOrEmpty(GetOpenId()))
			{
				callback?.Invoke(arg1: false, "用户ID为空");
				return;
			}
			string saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
			SavePlayerPrefsDataOnly(isWrite: false, markDirty: false);
			string jsonData = JsonUtility.ToJson((object)new PlayerPrefsDataSave
			{
				playerPrefsData = playerPrefsData,
				saveTime = saveTime
			});
			if (Application.isEditor)
			{
				callback?.Invoke(arg1: true, "Editor模式保存成功");
			}
			else if ((Object)(object)SingletonMono<HttpManager>.Instance != (Object)null)
			{
				SingletonMono<HttpManager>.Instance.UpdatePlayerPrefsData(GetOpenId(), jsonData, callback);
			}
			else
			{
				callback?.Invoke(arg1: false, "HttpManager 未初始化");
			}
		}
		catch (Exception ex)
		{
			callback?.Invoke(arg1: false, ex.Message);
		}
	}

	public void SaveTalentData2Server(Action<bool, string> callback = null)
	{
		try
		{
			if (_cloudSyncSuspended)
			{
				callback?.Invoke(arg1: false, "Cloud sync suspended");
				return;
			}
			if (string.IsNullOrEmpty(GetOpenId()))
			{
				callback?.Invoke(arg1: false, "用户ID为空");
				return;
			}
			SyncWorldCupCompactDataToTalentData();
			string saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
			string jsonData = JsonUtility.ToJson((object)new TalentDataSave
			{
				talentData = _currentTalentData,
				rebornCurrentLevel = ((_currentTalentData != null) ? _currentTalentData.rebornCurrentLevel : (-1)),
				saveTime = saveTime
			});
			if (Application.isEditor)
			{
				callback?.Invoke(arg1: true, "Editor模式保存成功");
			}
			else if ((Object)(object)SingletonMono<HttpManager>.Instance != (Object)null)
			{
				SingletonMono<HttpManager>.Instance.UpdateTalentData(GetOpenId(), jsonData, callback);
			}
			else
			{
				callback?.Invoke(arg1: false, "HttpManager 未初始化");
			}
		}
		catch (Exception ex)
		{
			callback?.Invoke(arg1: false, ex.Message);
		}
	}

	public void SaveFacilityDataOnly()
	{
		if (skipSave)
		{
			return;
		}
		MarkDataDirty();
		_dirtyFacilityDataFast = true;
		try
		{
			if (_currentFacilityData == null)
			{
				Debug.Log((object)"FacilityData为空，无法保存");
				return;
			}
			string saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
			string jsonData = JsonUtility.ToJson((object)new FacilityDataSave
			{
				facilityData = _currentFacilityData,
				saveTime = saveTime
			}, true);
			WriteFile(FacilityDataFilePath, jsonData);
			SaveWorldDataOnly();
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("保存FacilityData失败: " + ex.Message + "\n" + ex.StackTrace));
		}
	}

	public void SaveUpgradeDataOnly()
	{
		if (skipSave)
		{
			return;
		}
		MarkDataDirty();
		_dirtyUpgradeDataFast = true;
		try
		{
			if (_currentUpgradeData == null)
			{
				Debug.Log((object)"UpgradeData为空，无法保存");
				return;
			}
			string saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
			string jsonData = JsonUtility.ToJson((object)new UpgradeDataSave
			{
				upgradeData = _currentUpgradeData,
				saveTime = saveTime
			}, true);
			WriteFile(UpgradeDataFilePath, jsonData);
			SaveWorldDataOnly();
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("保存UpgradeData失败: " + ex.Message + "\n" + ex.StackTrace));
		}
	}

	public void SaveGameDataOnly(bool updateExitTime = true)
	{
		if (skipSave)
		{
			return;
		}
		MarkDataDirty();
		_dirtyGameDataFast = true;
		try
		{
			if (_currentGameData == null)
			{
				Debug.Log((object)"GameData为空，无法保存");
				return;
			}
			bool flag = updateExitTime;
			if (updateExitTime && IsOfflineRewardProtected())
			{
				flag = false;
			}
			if (flag)
			{
				long lastExitGameTime = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
				_currentGameData.lastExitGameTime = lastExitGameTime;
			}
			CaptureCurrentWorldData();
			string saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
			string jsonData = JsonUtility.ToJson((object)new GameDataSave
			{
				gameData = _currentGameData,
				globalProfileData = _globalProfileData,
				worldDataCollection = _worldDataCollection,
				saveTime = saveTime
			}, true);
			WriteFile(GameDataFilePath, jsonData);
			SaveWorldDataOnly();
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("保存GameData失败: " + ex.Message + "\n" + ex.StackTrace));
		}
	}

	public void SaveTalentDataOnly()
	{
		if (skipSave)
		{
			return;
		}
		MarkDataDirty();
		_dirtyTalentDataFast = true;
		try
		{
			if (_currentTalentData == null)
			{
				Debug.Log((object)"TalentData为空，无法保存");
				return;
			}
			SyncWorldCupCompactDataToTalentData();
			string saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
			string jsonData = JsonUtility.ToJson((object)new TalentDataSave
			{
				talentData = _currentTalentData,
				rebornCurrentLevel = ((_currentTalentData != null) ? _currentTalentData.rebornCurrentLevel : (-1)),
				saveTime = saveTime
			}, true);
			WriteFile(TalentDataFilePath, jsonData);
			SaveWorldDataOnly();
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("保存TalentData失败: " + ex.Message + "\n" + ex.StackTrace));
		}
	}

	public void SaveWorldCupDataOnly()
	{
		if (skipSave)
		{
			return;
		}
		try
		{
			if (_currentWorldCupData == null)
			{
				Debug.Log((object)"WorldCupData为空，无法保存");
				return;
			}
			string saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
			string jsonData = JsonUtility.ToJson((object)new WorldCupDataSave
			{
				worldCupData = _currentWorldCupData,
				saveTime = saveTime
			}, true);
			WriteFile(WorldCupDataFilePath, jsonData);
			SyncWorldCupCompactDataToTalentData();
			SaveTalentDataOnly();
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("保存WorldCupData失败: " + ex.Message + "\n" + ex.StackTrace));
		}
	}

	public void SaveTimeInfoOnly(bool updateExitTime = true)
	{
		try
		{
			string saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
			TimeInfoSave timeInfoSave = new TimeInfoSave
			{
				loginTime = _currentLoginTime.ToString("yyyy-MM-dd HH:mm:ss"),
				saveTime = saveTime
			};
			if (updateExitTime)
			{
				timeInfoSave.lastExitTime = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
			}
			else
			{
				try
				{
					bool fileExists;
					string text = ReadFile(TimeInfoFilePath, out fileExists);
					if (fileExists)
					{
						TimeInfoSave timeInfoSave2 = JsonUtility.FromJson<TimeInfoSave>(text);
						if (timeInfoSave2 != null && timeInfoSave2.lastExitTime > 0)
						{
							timeInfoSave.lastExitTime = timeInfoSave2.lastExitTime;
						}
					}
				}
				catch (Exception)
				{
					timeInfoSave.lastExitTime = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
				}
			}
			string jsonData = JsonUtility.ToJson((object)timeInfoSave, true);
			WriteFile(TimeInfoFilePath, jsonData);
			Debug.Log((object)"TimeInfo保存成功");
		}
		catch (Exception ex2)
		{
			Debug.LogError((object)("保存TimeInfo失败: " + ex2.Message + "\n" + ex2.StackTrace));
		}
	}

	public void LoadGameData()
	{
		try
		{
			IsLocalSaveMissing = 0;
			_loadedGlobalProfileFromGameData = false;
			_loadedWorldCollectionFromGameData = false;
			bool fileExists = false;
			bool flag = LoadGameDataFromFile(out fileExists);
			Debug.Log((object)$"[GameDataManager] Local GameData load: exists={fileExists}, loaded={flag}, path={GameDataFilePath}");
			bool fileExists2 = false;
			bool flag2 = LoadFacilityDataFromFile(out fileExists2);
			bool fileExists3 = false;
			bool flag3 = LoadUpgradeDataFromFile(out fileExists3);
			bool fileExists4 = false;
			bool flag4 = LoadTalentDataFromFile(out fileExists4);
			bool fileExists5 = false;
			bool flag5 = LoadWorldCupDataFromFile(out fileExists5);
			Debug.Log((object)($"[SaveCheck] game={fileExists}/{flag}, " + $"facility={fileExists2}/{flag2}, " + $"upgrade={fileExists3}/{flag3}, " + $"talent={fileExists4}/{flag4}, " + $"worldCup={fileExists5}/{flag5}"));
			IsGameDataLoadError = !flag;
			IsFacilityDataLoadError = !flag2;
			IsUpgradeDataLoadError = !flag3;
			IsTalentDataLoadError = !flag4;
			IsWorldCupDataLoadError = !flag5;
			bool num = flag || flag2 || flag3 || flag4 || flag5;
			bool flag6 = IsGameDataLoadError || IsFacilityDataLoadError || IsUpgradeDataLoadError || IsTalentDataLoadError || IsWorldCupDataLoadError;
			if (!num)
			{
				Debug.Log((object)"所有存档文件都不存在，创建新游戏数据");
				IsLocalSaveMissing = 1;
			}
			else if (flag6)
			{
				Debug.LogWarning((object)"部分本地存档加载失败或损坏，标记 IsLocalSaveMissing = 1 准备触发云端恢复");
				IsLocalSaveMissing = 1;
			}
			if (_currentGameData == null)
			{
				_currentGameData = new GameData();
			}
			if (_currentFacilityData == null)
			{
				_currentFacilityData = new FacilityData();
			}
			if (_currentUpgradeData == null)
			{
				_currentUpgradeData = new UpgradeData();
			}
			if (_currentTalentData == null)
			{
				_currentTalentData = new TalentData();
			}
			if (_currentWorldCupData == null)
			{
				_currentWorldCupData = new WorldCupData();
			}
			InitializeWorldPersistence();
			originalLastExitTime = _currentGameData.lastExitGameTime;
			UpdateLoginTime();
			if (_worldCupDataNeedsLocalRewrite)
			{
				_worldCupDataNeedsLocalRewrite = false;
				SaveWorldCupDataOnly();
			}
			ForceCompleteGuideStepsByLevel();
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("加载游戏数据失败: " + ex.Message + "\n" + ex.StackTrace));
			if (_currentGameData == null)
			{
				_currentGameData = new GameData();
			}
			if (_currentFacilityData == null)
			{
				_currentFacilityData = new FacilityData();
			}
			if (_currentUpgradeData == null)
			{
				_currentUpgradeData = new UpgradeData();
			}
			if (_currentTalentData == null)
			{
				_currentTalentData = new TalentData();
			}
			if (_currentWorldCupData == null)
			{
				_currentWorldCupData = new WorldCupData();
			}
			originalLastExitTime = _currentGameData.lastExitGameTime;
		}
	}

	public bool RestoreBackupData(ServerData serverData)
	{
		try
		{
			if (serverData != null)
			{
				bool flag = false;
				if (IsLocalSaveMissing == 1 && TryRestoreWorldData(serverData.gameDataSave))
				{
					bool num = TryRestoreWorldCupFromServerTalent(serverData);
					if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
					{
						SingletonMono<UpgradeManager>.Instance.LoadData();
					}
					if (num)
					{
						SaveWorldCupDataOnly();
						IsWorldCupDataLoadError = false;
					}
					ForceUpdateData();
					IsLocalSaveMissing = 0;
					IsGameDataLoadError = false;
					IsFacilityDataLoadError = false;
					IsUpgradeDataLoadError = false;
					IsTalentDataLoadError = false;
					return true;
				}
				if (IsGameDataLoadError && serverData.gameDataSave != null && serverData.gameDataSave.gameData != null && !string.IsNullOrEmpty(serverData.gameDataSave.gameData.localUserId))
				{
					int currentMainMissionLevel = serverData.gameDataSave.gameData.currentMainMissionLevel;
					int num2 = ((_currentGameData != null) ? _currentGameData.currentMainMissionLevel : 0);
					if (currentMainMissionLevel - num2 >= 2)
					{
						_currentGameData = serverData.gameDataSave.gameData;
						flag = true;
					}
					else
					{
						Debug.LogWarning((object)$"[GameDataManager] GameData备份的 currentMainMissionLevel({currentMainMissionLevel}) 未比当前({num2})大至少2级，跳过该模块恢复以防误覆盖。");
					}
				}
				if (IsFacilityDataLoadError && serverData.facilityDataSave != null && serverData.facilityDataSave.facilityData != null)
				{
					_currentFacilityData = serverData.facilityDataSave.facilityData;
					flag = true;
				}
				if (IsUpgradeDataLoadError && serverData.upgradeDataSave != null && serverData.upgradeDataSave.upgradeData != null)
				{
					_currentUpgradeData = serverData.upgradeDataSave.upgradeData;
					flag = true;
				}
				if (IsTalentDataLoadError && serverData.talentDataSave != null && serverData.talentDataSave.talentData != null)
				{
					Debug.Log((object)"天赋数据恢复成功");
					_currentTalentData = serverData.talentDataSave.talentData;
					TryRestoreWorldCupFromCompact(_currentTalentData.worldCupCompactData, "服务器 TalentData");
					_currentTalentData.IsNeedResetTalent = 10;
					PlayerPrefs.SetInt(Constants.firstReset, 10);
					PlayerPrefs.Save();
					Debug.Log((object)_currentTalentData.talentPoint);
					flag = true;
				}
				if (IsWorldCupDataLoadError && TryRestoreWorldCupFromServerTalent(serverData))
				{
					flag = true;
				}
				if (serverData.playerPrefsDataSave != null && serverData.playerPrefsDataSave.playerPrefsData != null)
				{
					LoadPlayerPrefsDataFromFile(serverData.playerPrefsDataSave);
					flag = true;
				}
				if (flag)
				{
					Debug.Log((object)"数据恢复成功2");
					if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
					{
						SingletonMono<UpgradeManager>.Instance.LoadData();
					}
					ForceUpdateData();
					SaveGameData(isClear: false, updateExitTime: false);
					originalLastExitTime = _currentGameData.lastExitGameTime;
					IsLocalSaveMissing = 0;
					IsGameDataLoadError = false;
					IsFacilityDataLoadError = false;
					IsUpgradeDataLoadError = false;
					IsTalentDataLoadError = false;
					IsWorldCupDataLoadError = false;
					Debug.Log((object)"[GameDataManager] 成功使用服务器备份数据覆盖本地出错的存档并保存。");
					return true;
				}
				Debug.LogWarning((object)"[GameDataManager] 备份数据中没有符合恢复条件的模块，跳过恢复。");
				return false;
			}
			return false;
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("[GameDataManager] 解析备份数据失败: " + ex.Message));
			return false;
		}
	}

	private bool LoadGameDataFromFile(out bool fileExists)
	{
		string text = ReadFile(GameDataFilePath, out fileExists);
		if (fileExists)
		{
			try
			{
				bool flag = HasRootJsonProperty(text, "globalProfileData");
				bool flag2 = HasRootJsonProperty(text, "worldDataCollection");
				GameDataSave gameDataSave = JsonUtility.FromJson<GameDataSave>(text);
				if (gameDataSave != null && gameDataSave.gameData != null)
				{
					_currentGameData = gameDataSave.gameData;
					if (flag && gameDataSave.globalProfileData != null)
					{
						_globalProfileData = gameDataSave.globalProfileData;
						_loadedGlobalProfileFromGameData = true;
					}
					if (flag2 && gameDataSave.worldDataCollection != null && gameDataSave.worldDataCollection.worlds != null)
					{
						_worldDataCollection = gameDataSave.worldDataCollection;
						_loadedWorldCollectionFromGameData = true;
					}
					if (_currentGameData.totalGold < 0.0)
					{
						double totalGold = _currentGameData.totalGold;
						long num = (long)Math.Abs((float)totalGold);
						if (num > 0 && Math.Abs(totalGold - (double)num) / (double)num < 0.1)
						{
							Debug.LogWarning((object)$"totalGold为负数({totalGold})，疑似float精度损失/溢出，估算原始值约为: {num}，已修正");
							_currentGameData.totalGold = num;
						}
						else
						{
							Debug.LogWarning((object)$"totalGold为负数({totalGold})，无法确定原因，已设置为21亿");
							_currentGameData.totalGold = 2147483647.0;
						}
					}
					if (_currentGameData.averageCoin < 0.0)
					{
						Debug.LogWarning((object)$"averageCoin为负数({_currentGameData.averageCoin})，已自动修正为0");
						_currentGameData.averageCoin = 0.0;
					}
					return true;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("GameData解析失败: " + ex.Message + "\n" + ex.StackTrace));
			}
		}
		else
		{
			Debug.Log((object)"GameData本地文件不存在或内容无效，使用默认数据");
		}
		if (_currentGameData == null)
		{
			_currentGameData = new GameData();
		}
		return false;
	}

	private static bool HasRootJsonProperty(string json, string propertyName)
	{
		if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(propertyName))
		{
			return false;
		}
		int num = 0;
		int num2 = -1;
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < json.Length; i++)
		{
			char c = json[i];
			if (flag)
			{
				if (flag2)
				{
					flag2 = false;
					continue;
				}
				switch (c)
				{
				case '\\':
					flag2 = true;
					break;
				case '"':
					flag = false;
					if (num == 1 && num2 >= 0 && i - num2 == propertyName.Length && string.CompareOrdinal(json, num2, propertyName, 0, propertyName.Length) == 0)
					{
						int j;
						for (j = i + 1; j < json.Length && char.IsWhiteSpace(json[j]); j++)
						{
						}
						if (j < json.Length && json[j] == ':')
						{
							return true;
						}
					}
					break;
				}
			}
			else
			{
				switch (c)
				{
				case '"':
					flag = true;
					flag2 = false;
					num2 = i + 1;
					break;
				case '[':
				case '{':
					num++;
					break;
				case ']':
				case '}':
					num--;
					break;
				}
			}
		}
		return false;
	}

	private bool LoadFacilityDataFromFile(out bool fileExists)
	{
		string text = ReadFile(FacilityDataFilePath, out fileExists);
		if (fileExists)
		{
			try
			{
				FacilityDataSave facilityDataSave = JsonUtility.FromJson<FacilityDataSave>(text);
				if (facilityDataSave != null && facilityDataSave.facilityData != null)
				{
					_currentFacilityData = facilityDataSave.facilityData;
					return true;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("FacilityData解析失败: " + ex.Message + "\n" + ex.StackTrace));
			}
		}
		else
		{
			Debug.Log((object)"FacilityData本地文件不存在或内容无效，使用默认数据");
		}
		if (_currentFacilityData == null)
		{
			_currentFacilityData = new FacilityData();
		}
		return false;
	}

	private bool LoadUpgradeDataFromFile(out bool fileExists)
	{
		string text = ReadFile(UpgradeDataFilePath, out fileExists);
		if (fileExists)
		{
			try
			{
				UpgradeDataSave upgradeDataSave = JsonUtility.FromJson<UpgradeDataSave>(text);
				if (upgradeDataSave != null && upgradeDataSave.upgradeData != null)
				{
					_currentUpgradeData = upgradeDataSave.upgradeData;
					return true;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("UpgradeData解析失败: " + ex.Message + "\n" + ex.StackTrace));
			}
		}
		else
		{
			Debug.Log((object)"UpgradeData本地文件不存在或内容无效，使用默认数据");
		}
		if (_currentUpgradeData == null)
		{
			_currentUpgradeData = new UpgradeData();
		}
		return false;
	}

	private bool LoadTalentDataFromFile(out bool fileExists)
	{
		string text = ReadFile(TalentDataFilePath, out fileExists);
		if (fileExists)
		{
			try
			{
				TalentDataSave talentDataSave = JsonUtility.FromJson<TalentDataSave>(text);
				if (talentDataSave != null && talentDataSave.talentData != null)
				{
					_currentTalentData = talentDataSave.talentData;
					return true;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("TalentData解析失败: " + ex.Message + "\n" + ex.StackTrace));
			}
		}
		else
		{
			Debug.Log((object)"TalentData文件不存在，尝试从 GameData.talentData 恢复");
		}
		if (_currentTalentData == null)
		{
			_currentTalentData = new TalentData();
		}
		return false;
	}

	private bool LoadWorldCupDataFromFile(out bool fileExists)
	{
		string text = ReadFile(WorldCupDataFilePath, out fileExists);
		if (fileExists)
		{
			try
			{
				WorldCupDataSave worldCupDataSave = JsonUtility.FromJson<WorldCupDataSave>(text);
				if (worldCupDataSave != null && worldCupDataSave.worldCupData != null)
				{
					_currentWorldCupData = worldCupDataSave.worldCupData;
					SyncWorldCupCompactDataToTalentData();
					return true;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("WorldCupData解析失败: " + ex.Message + "\n" + ex.StackTrace));
			}
		}
		if (TryRestoreWorldCupFromCompact(_currentTalentData?.worldCupCompactData, "本地 TalentData"))
		{
			return true;
		}
		if (_currentWorldCupData == null)
		{
			_currentWorldCupData = new WorldCupData();
		}
		return false;
	}

	private bool LoadTimeInfoFromFile(out bool fileExists)
	{
		string text = ReadFile(TimeInfoFilePath, out fileExists);
		if (fileExists)
		{
			try
			{
				timeSave = JsonUtility.FromJson<TimeInfoSave>(text);
				if (timeSave != null)
				{
					if (!string.IsNullOrEmpty(timeSave.loginTime))
					{
						DateTime.TryParse(timeSave.loginTime, out _currentLoginTime);
					}
					if (_currentGameData != null && _currentGameData.lastExitGameTime <= 0 && timeSave.lastExitTime > 0)
					{
						_currentGameData.lastExitGameTime = timeSave.lastExitTime;
					}
					return true;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("TimeInfo解析失败: " + ex.Message + "\n" + ex.StackTrace));
			}
		}
		else
		{
			Debug.Log((object)"TimeInfo文件不存在，使用默认时间");
		}
		return false;
	}

	public long GetLastExitTime()
	{
		try
		{
			if (_currentGameData != null && _currentGameData.lastExitGameTime > 0)
			{
				return _currentGameData.lastExitGameTime;
			}
			bool fileExists = false;
			if (timeSave == null)
			{
				string text = ReadFile(TimeInfoFilePath, out fileExists);
				if (fileExists)
				{
					timeSave = JsonUtility.FromJson<TimeInfoSave>(text);
				}
			}
			if (timeSave != null && timeSave.lastExitTime > 0)
			{
				if (_currentGameData != null)
				{
					_currentGameData.lastExitGameTime = timeSave.lastExitTime;
				}
				return timeSave.lastExitTime;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("获取上次退出时间失败: " + ex.Message));
		}
		Debug.LogWarning((object)"[GameDataManager] GetLastExitTime: 没有找到任何退出时间记录，返回0");
		return 0L;
	}

	public void ResetGameData()
	{
		string userId = GetUserId();
		string openId = GetOpenId();
		_currentGameData = new GameData();
		_currentFacilityData = new FacilityData();
		_currentUpgradeData = new UpgradeData();
		_currentTalentData = new TalentData();
		_currentWorldCupData = new WorldCupData();
		_globalProfileData = new GlobalProfileData
		{
			localUserId = userId,
			currentWorldId = 1,
			openId = openId,
			unlockedWorldIds = new List<int> { 1 }
		};
		_worldDataCollection = new WorldDataCollection
		{
			worlds = new List<WorldData>()
		};
		GetOrCreateWorldData(1);
		_loadedGlobalProfileFromGameData = false;
		_loadedWorldCollectionFromGameData = false;
		SaveGameData();
		SaveWorldDataOnly();
		Debug.Log((object)"游戏数据已重置");
	}

	private void OnApplicationQuit()
	{
		if (_currentGameData != null && !IsOfflineRewardProtected())
		{
			long lastExitGameTime = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
			_currentGameData.lastExitGameTime = lastExitGameTime;
		}
		if (isDataDirtyForCloudSync)
		{
			Debug.LogWarning((object)"[GameDataManager] 尝试在应用退出时同步脏数据，但这可能不会成功。");
			SaveAll2Server();
		}
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
		{
			SingletonMono<UpgradeManager>.Instance.FlushToSaveData(clearDirtyState: true);
		}
		SaveGameData(isClear: true);
	}

	public void OnApplicationPause(bool pauseStatus)
	{
		if (!pauseStatus)
		{
			return;
		}
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
		{
			SingletonMono<UpgradeManager>.Instance.FlushToSaveData(clearDirtyState: true);
		}
		SaveGameData();
		if (!isDataDirtyForCloudSync)
		{
			return;
		}
		Debug.Log((object)"[GameDataManager] 应用切入后台，触发脏数据保存到服务器。");
		SaveAll2Server(delegate(bool success, string response)
		{
			if (success)
			{
				Debug.Log((object)"[GameDataManager] 切后台保存成功！");
			}
			else
			{
				Debug.LogWarning((object)("[GameDataManager] 切后台保存失败: " + response));
			}
		});
	}

	public void OnApplicationFocus(bool hasFocus)
	{
		if (!hasFocus)
		{
			if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
			{
				SingletonMono<UpgradeManager>.Instance.FlushToSaveData(clearDirtyState: true);
			}
			SaveGameData();
			if (!isDataDirtyForCloudSync)
			{
				return;
			}
			Debug.Log((object)"[GameDataManager] 应用失去焦点，触发脏数据保存到服务器。");
			SaveAll2Server(delegate(bool success, string response)
			{
				if (success)
				{
					Debug.Log((object)"[GameDataManager] 失去焦点保存成功！");
				}
				else
				{
					Debug.LogWarning((object)("[GameDataManager] 失去焦点保存失败: " + response));
				}
			});
		}
		else
		{
			MarkPlayerActivelyEntered();
			this.OnGameFocus?.Invoke();
		}
	}

	public void InitializeCoinDiamond()
	{
		if (_currentGameData == null)
		{
			return;
		}
		if (!_currentGameData.isInitializedCoinDiamond)
		{
			_currentGameData.isInitializedCoinDiamond = true;
			if ((Object)(object)ConfigManager.Instance != (Object)null)
			{
				_currentGameData.totalGold = ConfigManager.Instance.GetCommonConfig(1).Factor;
				EnsureGlobalProfileForResourceMutation();
				if (GetCurrentWorldId() == 1 && _globalProfileData.diamonds == 0L)
				{
					_globalProfileData.diamonds = (long)ConfigManager.Instance.GetCommonConfig(2).Factor;
				}
				_currentGameData.diamonds = _globalProfileData.diamonds;
				_currentGameData.auctionCoin = ConfigManager.Instance.GetCommonConfig(52).Factor;
			}
		}
		else
		{
			if (_globalProfileData != null)
			{
				_currentGameData.diamonds = _globalProfileData.diamonds;
			}
			if (_currentGameData.auctionCoin == 0.0 && (Object)(object)ConfigManager.Instance != (Object)null)
			{
				_currentGameData.auctionCoin = ConfigManager.Instance.GetCommonConfig(52).Factor;
			}
		}
	}

	public long GetDiamond()
	{
		if (_globalProfileData != null)
		{
			return _globalProfileData.diamonds;
		}
		if (_currentGameData == null)
		{
			return 0L;
		}
		return _currentGameData.diamonds;
	}

	public bool AddDiamond(long count)
	{
		if (_currentGameData == null)
		{
			return false;
		}
		if (count < 0)
		{
			return false;
		}
		EnsureGlobalProfileForResourceMutation();
		_globalProfileData.diamonds += count;
		_currentGameData.diamonds = _globalProfileData.diamonds;
		SaveGameDataOnly();
		SingletonMono<UpgradeManager>.Instance?.AddGold(0.0);
		return true;
	}

	public long GetItemCount(int itemId)
	{
		List<GameItem> list = ((_globalProfileData != null) ? _globalProfileData.itemList : _currentGameData?.itemList);
		if (list == null)
		{
			return 0L;
		}
		return list.Find((GameItem i) => i.itemId == itemId)?.count ?? 0;
	}

	public void AddItem(int itemId, long count)
	{
		if (_currentGameData != null)
		{
			EnsureGlobalProfileForResourceMutation();
			_currentGameData.itemList = _globalProfileData.itemList;
			GameItem gameItem = _globalProfileData.itemList.Find((GameItem i) => i.itemId == itemId);
			if (gameItem != null)
			{
				gameItem.count += count;
			}
			else
			{
				_globalProfileData.itemList.Add(new GameItem
				{
					itemId = itemId,
					count = count
				});
			}
			if (itemId >= 105 && WorldCupDataManager.Instance != null)
			{
				WorldCupDataManager.Instance.AddWorldCupMissionProgress(3, (int)count);
			}
			ForceUpdateCoinItemDiamond();
			SaveGameDataOnly();
		}
	}

	public bool RemoveItem(int itemId, long count)
	{
		if (_currentGameData == null || count < 0)
		{
			return false;
		}
		EnsureGlobalProfileForResourceMutation();
		if (_globalProfileData.itemList == null)
		{
			return false;
		}
		GameItem gameItem = _globalProfileData.itemList.Find((GameItem i) => i.itemId == itemId);
		if (gameItem != null && gameItem.count >= count)
		{
			gameItem.count -= count;
			ForceUpdateCoinItemDiamond();
			SaveGameDataOnly();
			return true;
		}
		return false;
	}

	public bool RemoveDiamond(long count)
	{
		if (_currentGameData == null)
		{
			return false;
		}
		if (count < 0)
		{
			return false;
		}
		if (GetDiamond() < count)
		{
			return false;
		}
		EnsureGlobalProfileForResourceMutation();
		_globalProfileData.diamonds -= count;
		_currentGameData.diamonds = _globalProfileData.diamonds;
		SaveGameDataOnly();
		SingletonMono<UpgradeManager>.Instance?.AddGold(0.0);
		if (WorldCupDataManager.Instance != null)
		{
			WorldCupDataManager.Instance.AddWorldCupMissionProgress(4, (int)count);
		}
		return true;
	}

	public void ForceUpdateData()
	{
		this.OnGameDataUpdated?.Invoke();
	}

	public int GetShangRenTodayDiamond()
	{
		return _currentGameData?.shangRenTodayDiamond ?? 0;
	}

	public void AddShangRenTodayDiamond(int count)
	{
		if (_currentGameData != null)
		{
			_currentGameData.shangRenTodayDiamond += count;
			SaveGameDataOnly();
		}
	}

	public string GetShangRenLastDate()
	{
		return _currentGameData?.shangRenLastDate ?? "";
	}

	public void SetShangRenLastDate(string date)
	{
		if (_currentGameData != null)
		{
			_currentGameData.shangRenLastDate = date;
			SaveGameDataOnly();
		}
	}

	public void ResetShangRenDailyData()
	{
		if (_currentGameData != null)
		{
			_currentGameData.shangRenTodayDiamond = 0;
			_currentGameData.shangRenWaitStartTime = 0L;
			_currentGameData.shangRenWaitTargetTime = 0f;
			SaveGameDataOnly();
		}
	}

	public long GetShangRenWaitStartTime()
	{
		return _currentGameData?.shangRenWaitStartTime ?? 0;
	}

	public void SetShangRenWaitStartTime(long time)
	{
		if (_currentGameData != null)
		{
			_currentGameData.shangRenWaitStartTime = time;
			SaveGameDataOnly();
		}
	}

	public float GetShangRenWaitTarget()
	{
		return _currentGameData?.shangRenWaitTargetTime ?? 0f;
	}

	public void SetShangRenWaitTarget(float target)
	{
		if (_currentGameData != null)
		{
			_currentGameData.shangRenWaitTargetTime = target;
			SaveGameDataOnly();
		}
	}

	public void ClearShangRenWait()
	{
		if (_currentGameData != null)
		{
			_currentGameData.shangRenWaitStartTime = 0L;
			_currentGameData.shangRenWaitTargetTime = 0f;
			SaveGameDataOnly();
		}
	}

	public void ForceUpdateCoinItemDiamond()
	{
		this.OnCoinItemDiamondUpdated?.Invoke();
	}

	private void EnsureGlobalProfileForResourceMutation()
	{
		if (_globalProfileData == null)
		{
			_globalProfileData = new GlobalProfileData
			{
				diamonds = ((_currentGameData != null) ? _currentGameData.diamonds : 0),
				itemList = (_currentGameData?.itemList ?? new List<GameItem>()),
				helperOwned = (_currentFacilityData?.helperOwned ?? new List<HelperInfo>())
			};
		}
		GlobalProfileData globalProfileData = _globalProfileData;
		if (globalProfileData.itemList == null)
		{
			globalProfileData.itemList = new List<GameItem>();
		}
		globalProfileData = _globalProfileData;
		if (globalProfileData.helperOwned == null)
		{
			globalProfileData.helperOwned = new List<HelperInfo>();
		}
	}

	public void ForceUpdateNewGuide()
	{
		this.OnNewGuideUpdated?.Invoke();
	}

	public void ForceUpdateGameFocus()
	{
		this.OnGameFocus?.Invoke();
	}

	public void AddAdCount()
	{
		if (_currentGameData == null)
		{
			_currentGameData = new GameData();
		}
		string text = DateTime.Now.ToString("yyyy-MM-dd");
		if (_currentGameData.lastAdDate != text)
		{
			_currentGameData.todayAdCount = 0;
			_currentGameData.lastAdDate = text;
		}
		_currentGameData.todayAdCount++;
		_currentGameData.totalAdCount++;
		if (WorldCupDataManager.Instance != null)
		{
			WorldCupDataManager.Instance.AddWorldCupMissionProgress(5, 1);
		}
		this.OnGameDataUpdated?.Invoke();
	}

	public long GetAdCount(bool isToday = true)
	{
		if (_currentGameData == null)
		{
			_currentGameData = new GameData();
		}
		string text = DateTime.Now.ToString("yyyy-MM-dd");
		if (_currentGameData.lastAdDate != text)
		{
			_currentGameData.todayAdCount = 0;
			_currentGameData.lastAdDate = text;
		}
		if (!isToday)
		{
			return _currentGameData.totalAdCount;
		}
		return _currentGameData.todayAdCount;
	}

	private int GenerateUniqueId()
	{
		if (_currentGameData == null)
		{
			Debug.LogWarning((object)"GameDataManager: 当前游戏数据为空");
			return -1;
		}
		_currentGameData.currentMaxUniqueId++;
		return _currentGameData.currentMaxUniqueId;
	}

	public void ChangeTalentPoint(bool isAdd, int count)
	{
		ChangeTalentPoint(isAdd, count, TalentSceneType.CoinSceneTalent);
	}

	public void ChangeTalentPoint(bool isAdd, int count, TalentSceneType sceneType)
	{
		if (sceneType == TalentSceneType.DiceSceneTalent)
		{
			if (isAdd)
			{
				_currentTalentData.diceTalentPoint += count;
			}
			else
			{
				_currentTalentData.diceTalentPoint -= count;
				if (_currentTalentData.diceTalentPoint <= 0)
				{
					_currentTalentData.diceTalentPoint = 0;
				}
			}
		}
		else if (isAdd)
		{
			_currentTalentData.talentPoint += count;
		}
		else
		{
			_currentTalentData.talentPoint -= count;
			if (_currentTalentData.talentPoint <= 0)
			{
				_currentTalentData.talentPoint = 0;
			}
		}
		SaveTalentDataOnly();
		TalentManager talentManager = TalentDataManager.Instance?.GetCurrentTalentManager();
		if ((Object)(object)talentManager != (Object)null && (Object)(object)talentManager.talentPanels[0] != (Object)null)
		{
			talentManager.talentPanels[0].RefreshTalentPanel();
		}
	}

	public int GetCostPoint(TalentType type)
	{
		return GetCostPoint(type, TalentSceneType.CoinSceneTalent);
	}

	public int GetCostPoint(TalentType type, TalentSceneType sceneType)
	{
		int[] array = ((sceneType == TalentSceneType.DiceSceneTalent) ? _currentTalentData.diceCostTalentPoint : _currentTalentData.costTalentPoint);
		return type switch
		{
			TalentType.talentCoin => array[0], 
			TalentType.talentHelper => array[1], 
			TalentType.talentSpecial => array[2], 
			_ => 0, 
		};
	}

	public void ResetCostPoint(TalentType type, int point)
	{
		ResetCostPoint(type, point, TalentSceneType.CoinSceneTalent);
	}

	public void ResetCostPoint(TalentType type, int point, TalentSceneType sceneType)
	{
		int[] array = ((sceneType == TalentSceneType.DiceSceneTalent) ? _currentTalentData.diceCostTalentPoint : _currentTalentData.costTalentPoint);
		switch (type)
		{
		case TalentType.talentCoin:
			array[0] = point;
			break;
		case TalentType.talentHelper:
			array[1] = point;
			break;
		case TalentType.talentSpecial:
			array[2] = point;
			break;
		}
	}

	public void ResetTalentPoint()
	{
		ResetTalentPoint(TalentSceneType.CoinSceneTalent);
	}

	public void ResetTalentPoint(TalentSceneType sceneType)
	{
		int count = 0;
		TalentManager talentManager = TalentDataManager.Instance?.GetCurrentTalentManager();
		if ((Object)(object)talentManager != (Object)null && (Object)(object)talentManager.talentPanels[0] != (Object)null)
		{
			count = GetCostPoint(TalentType.talentCoin, sceneType);
			talentManager.talentPanels[0].talentResetTipsItem.SetActive(false);
			talentManager.talentPanels[0].ResetTalentDic();
		}
		ChangeTalentPoint(isAdd: true, count, sceneType);
		ResetCostPoint(TalentType.talentCoin, 0, sceneType);
		if (sceneType == TalentSceneType.CoinSceneTalent && (Object)(object)SingletonMono<RoleManager>.Instance != (Object)null)
		{
			SingletonMono<RoleManager>.Instance.RefreshAllRoleSleepProbability();
		}
		if (sceneType == TalentSceneType.CoinSceneTalent)
		{
			SingletonMono<UpgradeManager>.Instance.UnlockedUpgradeRemove(5, "itemUpgrade1");
		}
		if (sceneType == TalentSceneType.DiceSceneTalent)
		{
			SingletonMono<UpgradeManager>.Instance.UnlockedUpgradeRemove(105, "itemUpgrade1");
		}
		SaveGameDataOnly();
		SaveTalentDataOnly();
	}

	public void ResetRebornCurrentLevel(TalentSceneType sceneType)
	{
		SetRebornCurrentLevel(-1, sceneType);
	}

	public int GetTalentPoint()
	{
		return GetTalentPoint(TalentSceneType.CoinSceneTalent);
	}

	public int GetTalentPoint(TalentSceneType sceneType)
	{
		if (sceneType == TalentSceneType.DiceSceneTalent)
		{
			return _currentTalentData.diceTalentPoint;
		}
		return _currentTalentData.talentPoint;
	}

	public List<bool> GetTalentUnlock()
	{
		return _currentTalentData.talentUnlock;
	}

	public List<int> GetTalentLevel()
	{
		return _currentTalentData.talentLevel;
	}

	public int GetTalentJingYing()
	{
		if (GetCurrentWorldId() != 1)
		{
			return 0;
		}
		List<talent> list = ConfigManager.Instance?.GetAllCoinTalentItem();
		talent talent = ConfigManager.Instance?.GetCoinTalentConfig(207);
		int num = 0;
		if (list == null && talent == null)
		{
			return 0;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].TalentName == "精英教育")
			{
				num = i;
				break;
			}
		}
		List<int> talentLevel = _currentTalentData.talentLevel;
		if (talentLevel == null || talentLevel.Count <= num)
		{
			return 0;
		}
		int num2 = talentLevel[num];
		if (num2 > 0 && talent.UpgradeResult.Count > num2 - 1)
		{
			return num2 * (int)talent.UpgradeResult[num2 - 1];
		}
		return 0;
	}

	public TalentData GetTalentData()
	{
		return _currentTalentData;
	}

	public float GetSoundSleep()
	{
		return _currentTalentData.soundSleep;
	}

	public void SetSoundSleep(float sum)
	{
		_currentTalentData.soundSleep = sum;
		SaveTalentDataOnly();
	}

	public float GetGoldAge()
	{
		return _currentTalentData.goldAge;
	}

	public void SetGoldAge(float sum)
	{
		_currentTalentData.goldAge = sum;
		SaveTalentDataOnly();
	}

	public float GetLaoDongLi()
	{
		if (GetCurrentWorldId() != 1)
		{
			return 1f;
		}
		return _currentTalentData.laodongli;
	}

	public void SetLaoDongLi(float sum)
	{
		_currentTalentData.laodongli = sum;
		SaveTalentDataOnly();
	}

	public int GetRebornCurrentLevel()
	{
		return GetRebornCurrentLevel(TalentSceneType.CoinSceneTalent);
	}

	public int GetRebornCurrentLevel(TalentSceneType sceneType)
	{
		if (_currentTalentData == null)
		{
			return -1;
		}
		if (sceneType != TalentSceneType.DiceSceneTalent)
		{
			return _currentTalentData.rebornCurrentLevel;
		}
		return _currentTalentData.diceRebornCurrentLevel;
	}

	public float GetRebornGain()
	{
		return GetRebornGain(TalentSceneType.CoinSceneTalent);
	}

	public float GetRebornGain(TalentSceneType sceneType)
	{
		if (_currentTalentData == null)
		{
			return 1f;
		}
		int num = ((sceneType == TalentSceneType.DiceSceneTalent) ? _currentTalentData.diceRebornCurrentLevel : _currentTalentData.rebornCurrentLevel);
		float num2 = 1f;
		if (sceneType == TalentSceneType.DiceSceneTalent)
		{
			List<talentPoint2> allTalentPoint = ConfigManager.Instance.GetAllTalentPoint2();
			if (allTalentPoint == null)
			{
				return 1f;
			}
			if (num >= allTalentPoint.Count - 1)
			{
				num = allTalentPoint.Count - 1;
			}
			for (int i = 0; i <= num; i++)
			{
				num2 *= allTalentPoint[i].RebornBuff;
			}
		}
		else
		{
			List<talentPoint> allTalentPoint2 = ConfigManager.Instance.GetAllTalentPoint();
			if (allTalentPoint2 == null)
			{
				return 1f;
			}
			if (num >= allTalentPoint2.Count - 1)
			{
				num = allTalentPoint2.Count - 1;
			}
			for (int j = 0; j <= num; j++)
			{
				num2 *= allTalentPoint2[j].RebornBuff;
			}
		}
		return num2;
	}

	public void SetRebornCurrentLevel(int level)
	{
		SetRebornCurrentLevel(level, TalentSceneType.CoinSceneTalent);
	}

	public void SetRebornCurrentLevel(int level, TalentSceneType sceneType)
	{
		if (_currentTalentData != null)
		{
			if (sceneType == TalentSceneType.DiceSceneTalent)
			{
				_currentTalentData.diceRebornCurrentLevel = level;
			}
			else
			{
				_currentTalentData.rebornCurrentLevel = level;
			}
			SaveTalentDataOnly();
		}
	}

	public void ResetUpgradeData()
	{
		if (_worldDataCollection != null && _worldDataCollection.worlds != null)
		{
			foreach (WorldData world in _worldDataCollection.worlds)
			{
				if (world != null && world.upgradeData != null)
				{
					world.upgradeData.upgradeItemLevels.Clear();
					world.upgradeData.unlockedUpgradeItems.Clear();
					world.upgradeData.currencyEvolutionItems?.Clear();
					world.upgradeData.helperADLevel = 5;
				}
			}
		}
		if (_currentUpgradeData != null)
		{
			_currentUpgradeData.upgradeItemLevels.Clear();
			_currentUpgradeData.unlockedUpgradeItems.Clear();
			_currentUpgradeData.currencyEvolutionItems?.Clear();
			_currentUpgradeData.helperADLevel = 5;
		}
	}

	public int GetSupportIndex(DialogType dialogType)
	{
		return dialogType switch
		{
			DialogType.speedBuff => 0, 
			DialogType.incomeBuff => 1, 
			DialogType.money => 2, 
			DialogType.diamond => 3, 
			DialogType.gongfu => 4, 
			DialogType.turnDiamond => 5, 
			DialogType.dashe => 6, 
			DialogType.spinAd => 7, 
			_ => 0, 
		};
	}

	private void EnsureSupportArraySize(int index)
	{
		if (_currentGameData != null && (_currentGameData.lastSupportTime == null || _currentGameData.lastSupportTime.Length <= index))
		{
			long[] array = new long[index + 1];
			if (_currentGameData.lastSupportTime != null)
			{
				_currentGameData.lastSupportTime.CopyTo(array, 0);
			}
			_currentGameData.lastSupportTime = array;
		}
	}

	public long LastSupportTime(DialogType dialogType)
	{
		if (_currentGameData == null)
		{
			return 0L;
		}
		int supportIndex = GetSupportIndex(dialogType);
		EnsureSupportArraySize(supportIndex);
		return _currentGameData.lastSupportTime[supportIndex];
	}

	public void RecordSupport(DialogType dialogType)
	{
		if (_currentGameData != null)
		{
			int supportIndex = GetSupportIndex(dialogType);
			EnsureSupportArraySize(supportIndex);
			_currentGameData.lastSupportTime[supportIndex] = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
		}
	}

	public bool CanShowSupport(DialogType dialogType)
	{
		if (_currentGameData == null)
		{
			return true;
		}
		int supportIndex = GetSupportIndex(dialogType);
		EnsureSupportArraySize(supportIndex);
		long num = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
		int currentWorldId = GetCurrentWorldId();
		float num2;
		switch (dialogType)
		{
		case DialogType.money:
			num2 = ConfigManager.Instance.GetCommonFactor(24);
			if (num2 <= 0f)
			{
				num2 = 1f;
			}
			break;
		case DialogType.incomeBuff:
			num2 = ConfigManager.Instance.GetCommonFactor(12);
			break;
		case DialogType.speedBuff:
			num2 = ConfigManager.Instance.GetCommonFactor(9);
			if (currentWorldId == 2)
			{
				num2 += TalentDataManager.Instance?.GetDiceTalentBuff(304) ?? 0f;
			}
			break;
		case DialogType.turnDiamond:
			num2 = ConfigManager.Instance.GetCommonFactor(36);
			break;
		case DialogType.spinAd:
			num2 = ConfigManager.Instance.GetCommonFactor(35);
			break;
		default:
			num2 = 5f;
			break;
		}
		float num3 = num2 * 60f * 1000f;
		return (float)(num - _currentGameData.lastSupportTime[supportIndex]) >= num3;
	}

	public float GetSupportRemainingTime(DialogType dialogType)
	{
		if (_currentGameData == null)
		{
			return 0f;
		}
		int supportIndex = GetSupportIndex(dialogType);
		EnsureSupportArraySize(supportIndex);
		long num = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
		int currentWorldId = GetCurrentWorldId();
		float num2;
		switch (dialogType)
		{
		case DialogType.money:
			num2 = ConfigManager.Instance.GetCommonFactor(24);
			if (num2 <= 0f)
			{
				num2 = 1f;
			}
			break;
		case DialogType.incomeBuff:
			num2 = ConfigManager.Instance.GetCommonFactor(12);
			break;
		case DialogType.speedBuff:
			num2 = ConfigManager.Instance.GetCommonFactor(9);
			if (currentWorldId == 2)
			{
				num2 += TalentDataManager.Instance?.GetDiceTalentBuff(304) ?? 0f;
			}
			break;
		case DialogType.diamond:
			num2 = ConfigManager.Instance.GetCommonFactor(32);
			break;
		case DialogType.gongfu:
		case DialogType.dashe:
			num2 = 5f;
			break;
		case DialogType.turnDiamond:
			num2 = ConfigManager.Instance.GetCommonFactor(36);
			break;
		case DialogType.spinAd:
			num2 = ((currentWorldId != 2) ? ConfigManager.Instance.GetCommonFactor(35) : (ConfigManager.Instance.GetCommonFactor(35) - TalentDataManager.Instance?.GetDiceTalentBuff(111)).GetValueOrDefault());
			break;
		default:
			num2 = 5f;
			break;
		}
		float num3 = num2 * 60f * 1000f;
		long num4 = num - _currentGameData.lastSupportTime[supportIndex];
		float num5 = (num3 - (float)num4) / 1000f;
		return Mathf.Max(0f, num5);
	}

	public void ClearSupportTime(DialogType dialogType)
	{
		if (_currentGameData != null)
		{
			int supportIndex = GetSupportIndex(dialogType);
			EnsureSupportArraySize(supportIndex);
			_currentGameData.lastSupportTime[supportIndex] = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds + Mathf.RoundToInt(300000f);
		}
	}

	public bool IsRandomEventTriggered(int eventIndex)
	{
		if (_currentGameData == null)
		{
			return false;
		}
		if (eventIndex < 0 || eventIndex >= 31)
		{
			return false;
		}
		return (_currentGameData.randomEventMask & (1 << eventIndex)) != 0;
	}

	public void MarkRandomEventTriggered(int eventIndex)
	{
		if (_currentGameData != null && eventIndex >= 0 && eventIndex < 31)
		{
			_currentGameData.randomEventMask |= 1 << eventIndex;
			SaveGameDataOnly();
		}
	}

	public int GetCurrentDeskId()
	{
		if (_currentGameData == null)
		{
			return 1;
		}
		return _currentGameData.currentDeskId;
	}

	public void SetCurrentDeskId(int deskId)
	{
		if (_currentGameData != null)
		{
			_currentGameData.currentDeskId = deskId;
			SaveGameDataOnly();
		}
	}

	public void ResetFacilityData()
	{
		if (_currentFacilityData != null)
		{
			for (int i = 0; i < _currentFacilityData.deskOwned.Count; i++)
			{
			}
			for (int j = 0; j < _currentFacilityData.helperOwned.Count; j++)
			{
				_currentFacilityData.helperOwned[j].helperCount = 0;
			}
			SaveFacilityDataOnly();
		}
	}

	public bool IsDeskOwned(int deskId)
	{
		if (_currentFacilityData == null)
		{
			return false;
		}
		return _currentFacilityData.deskOwned.Exists((DeskInfo d) => d.deskId == deskId);
	}

	public void SetDeskOwned(int deskId, bool isOwned)
	{
		if (_currentFacilityData == null)
		{
			return;
		}
		if (isOwned)
		{
			if (!IsDeskOwned(deskId))
			{
				_currentFacilityData.deskOwned.Add(new DeskInfo
				{
					deskId = deskId,
					deskLevel = 1
				});
			}
			else
			{
				SetDeskLevel(deskId, GetDeskLevel(deskId) + 1);
			}
		}
		else
		{
			_currentFacilityData.deskOwned.RemoveAll((DeskInfo d) => d.deskId == deskId);
		}
		SaveFacilityDataOnly();
	}

	public int GetDeskLevel(int deskId)
	{
		if (_currentFacilityData == null)
		{
			return 1;
		}
		return _currentFacilityData.deskOwned.Find((DeskInfo d) => d.deskId == deskId)?.deskLevel ?? 0;
	}

	public DeskInfo GetDeskInfo(int deskId)
	{
		if (_currentFacilityData == null)
		{
			return null;
		}
		return _currentFacilityData.deskOwned.Find((DeskInfo d) => d.deskId == deskId);
	}

	public List<DeskInfo> GetAllDeskInfo()
	{
		if (_currentFacilityData == null)
		{
			return null;
		}
		return _currentFacilityData.deskOwned;
	}

	public void SetDeskLevel(int deskId, int level)
	{
		if (_currentFacilityData != null)
		{
			DeskInfo deskInfo = _currentFacilityData.deskOwned.Find((DeskInfo d) => d.deskId == deskId);
			if (deskInfo != null)
			{
				deskInfo.deskLevel = level;
				SaveFacilityDataOnly();
			}
		}
	}

	public bool IsHelperOwned(int helperId)
	{
		if (_currentFacilityData == null)
		{
			return false;
		}
		return _currentFacilityData.helperOwned.Exists((HelperInfo h) => h.helperId == helperId);
	}

	public void SetHelperOwned(int helperId, bool isOwned, bool isADUnlock = false)
	{
		if (_currentFacilityData == null)
		{
			return;
		}
		if (isOwned)
		{
			if (!IsHelperOwned(helperId))
			{
				int globalHelperSkillLevel = GetGlobalHelperSkillLevel(helperId);
				_currentFacilityData.helperOwned.Add(new HelperInfo
				{
					helperId = helperId,
					helperLevel = globalHelperSkillLevel,
					helperCount = 0,
					maxCount = 1,
					isAdUnlock = (isADUnlock ? 1 : 0),
					skillLevel = GetAwakeSkillLevelFromHelperLevel(helperId, globalHelperSkillLevel)
				});
			}
		}
		else
		{
			_currentFacilityData.helperOwned.RemoveAll((HelperInfo h) => h.helperId == helperId);
		}
		SaveFacilityDataOnly();
	}

	public HelperInfo GetHelperInfo(int helperId)
	{
		if (_currentFacilityData == null)
		{
			return null;
		}
		HelperInfo helperInfo = _currentFacilityData.helperOwned.Find((HelperInfo h) => h.helperId == helperId);
		if (helperInfo == null)
		{
			return null;
		}
		return helperInfo;
	}

	public int GetHelperAdUnlock(int helperId)
	{
		if (_currentFacilityData == null)
		{
			return 0;
		}
		HelperInfo helperInfo = _currentFacilityData.helperOwned.Find((HelperInfo h) => h.helperId == helperId);
		if (helperInfo == null)
		{
			return 0;
		}
		if (helperInfo.isAdUnlock < 1)
		{
			return 0;
		}
		return 1;
	}

	public int GetHelperLevel(int helperId)
	{
		if (_currentFacilityData == null)
		{
			return 1;
		}
		return _currentFacilityData.helperOwned.Find((HelperInfo h) => h.helperId == helperId)?.helperLevel ?? 1;
	}

	private int GetAwakeSkillLevelFromHelperLevel(int helperId, int helperLevel)
	{
		helperRole helperRole = (ConfigManager.HasInstance() ? ConfigManager.Instance.GetHelperRoleConfig(helperId) : null);
		if (helperRole == null)
		{
			return 0;
		}
		return Constants.GetHelperAwakeSkillLevel(helperRole, helperLevel);
	}

	public int GetHelperAwakeSkillLevel(int helperId)
	{
		if (_currentFacilityData == null)
		{
			return 0;
		}
		HelperInfo helperInfo = _currentFacilityData.helperOwned.Find((HelperInfo h) => h.helperId == helperId);
		if (helperInfo != null)
		{
			helperInfo.skillLevel = GetAwakeSkillLevelFromHelperLevel(helperId, helperInfo.helperLevel);
			return helperInfo.skillLevel;
		}
		return GetAwakeSkillLevelFromHelperLevel(helperId, GetGlobalHelperSkillLevel(helperId));
	}

	private int GetGlobalHelperSkillLevel(int helperId)
	{
		if (_globalProfileData?.globalHelperSkills == null)
		{
			return 1;
		}
		GlobalHelperSkillData globalHelperSkillData = _globalProfileData.globalHelperSkills.Find((GlobalHelperSkillData g) => g.helperId == helperId);
		if (globalHelperSkillData == null)
		{
			return 1;
		}
		return Math.Max(1, globalHelperSkillData.skillLevel);
	}

	private void UpdateGlobalHelperSkillLevel(int helperId, int skillLevel)
	{
		if (_globalProfileData == null)
		{
			return;
		}
		if (_globalProfileData.globalHelperSkills == null)
		{
			_globalProfileData.globalHelperSkills = new List<GlobalHelperSkillData>();
		}
		GlobalHelperSkillData globalHelperSkillData = _globalProfileData.globalHelperSkills.Find((GlobalHelperSkillData g) => g.helperId == helperId);
		if (globalHelperSkillData != null)
		{
			if (skillLevel > globalHelperSkillData.skillLevel)
			{
				globalHelperSkillData.skillLevel = skillLevel;
			}
		}
		else
		{
			_globalProfileData.globalHelperSkills.Add(new GlobalHelperSkillData
			{
				helperId = helperId,
				skillLevel = Math.Max(1, skillLevel)
			});
		}
	}

	public void SetHelperLevel(int helperId, int level)
	{
		if (_currentFacilityData != null)
		{
			HelperInfo helperInfo = _currentFacilityData.helperOwned.Find((HelperInfo h) => h.helperId == helperId);
			if (helperInfo != null)
			{
				helperInfo.helperLevel = level;
				helperInfo.skillLevel = GetAwakeSkillLevelFromHelperLevel(helperId, level);
				UpdateGlobalHelperSkillLevel(helperId, level);
				SaveFacilityDataOnly();
			}
		}
	}

	public int[] GetHelperIds(int count)
	{
		if (_currentFacilityData == null)
		{
			return new int[count];
		}
		if (_currentFacilityData.helperOwned == null || _currentFacilityData.helperOwned.Count == 0)
		{
			return new int[count];
		}
		int[] array = new int[count];
		int num = 0;
		foreach (HelperInfo item in _currentFacilityData.helperOwned)
		{
			for (int i = 0; i < item.helperCount; i++)
			{
				array[num] = item.helperId;
				num++;
			}
		}
		for (int j = num; j < count; j++)
		{
			array[j] = 0;
		}
		return array;
	}

	public int GetAllHelperCount()
	{
		if (_currentFacilityData == null)
		{
			return 0;
		}
		int num = 0;
		foreach (HelperInfo item in _currentFacilityData.helperOwned)
		{
			num += item.helperCount;
		}
		return num;
	}

	public int GetHelperCount(int helperId)
	{
		if (_currentFacilityData == null)
		{
			return 0;
		}
		return _currentFacilityData.helperOwned.Find((HelperInfo h) => h.helperId == helperId)?.helperCount ?? 0;
	}

	public void SetHelperCount(int helperId, int count)
	{
		if (_currentFacilityData != null)
		{
			HelperInfo helperInfo = _currentFacilityData.helperOwned.Find((HelperInfo h) => h.helperId == helperId);
			if (helperInfo != null)
			{
				helperInfo.helperCount = count;
				SaveFacilityDataOnly();
			}
		}
	}

	public int GetHelperMaxCount(int helperId)
	{
		if (_currentFacilityData == null)
		{
			return 88;
		}
		return _currentFacilityData.helperOwned.Find((HelperInfo h) => h.helperId == helperId)?.maxCount ?? 0;
	}

	public void SetHelperMaxCount(int helperId, int maxCount)
	{
		if (_currentFacilityData != null)
		{
			HelperInfo helperInfo = _currentFacilityData.helperOwned.Find((HelperInfo h) => h.helperId == helperId);
			if (helperInfo != null)
			{
				helperInfo.maxCount = maxCount;
				SaveFacilityDataOnly();
			}
		}
	}

	public List<HelperInfo> GetAllHeperInfo()
	{
		if (_currentFacilityData == null)
		{
			return null;
		}
		return _currentFacilityData.helperOwned;
	}

	public int GetCurrentMainMissionLevel()
	{
		if (_currentGameData == null)
		{
			return 0;
		}
		return _currentGameData.currentMainMissionLevel;
	}

	public void SetCurrentMainMissionLevel(int level)
	{
		if (_currentGameData != null)
		{
			_currentGameData.currentMainMissionLevel = level;
			SaveGameDataOnly();
		}
	}

	private static byte[] CompressGZip(byte[] data)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress);
		gZipStream.Write(data, 0, data.Length);
		gZipStream.Close();
		return memoryStream.ToArray();
	}

	private static byte[] DecompressGZip(byte[] compressed)
	{
		using MemoryStream stream = new MemoryStream(compressed);
		using GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress);
		using MemoryStream memoryStream = new MemoryStream();
		gZipStream.CopyTo(memoryStream);
		return memoryStream.ToArray();
	}

	public int GetCheckInDay()
	{
		return _currentGameData.checkInDay;
	}

	public void CheckAndAdvanceCheckInDayCrossDay()
	{
		if (_currentGameData == null)
		{
			return;
		}
		int[] todayDateNumber = GetTodayDateNumber();
		if (_currentGameData.todayMonth != todayDateNumber[0] || _currentGameData.today != todayDateNumber[1])
		{
			_currentGameData.todayMonth = todayDateNumber[0];
			_currentGameData.today = todayDateNumber[1];
			int checkInDay = _currentGameData.checkInDay;
			checkInDay++;
			if (checkInDay > 7)
			{
				checkInDay = 1;
				ResetClampItem(_currentGameData);
			}
			SetCheckInDay(checkInDay);
			UpdateAllWorldsCheckInDay(todayDateNumber);
			SaveGameDataOnly();
		}
	}

	public int GetCheckInDayByWorld(int worldId)
	{
		WorldData worldData = GetWorldData(worldId);
		if (worldData != null && worldData.gameData != null)
		{
			return worldData.gameData.checkInDay;
		}
		return _currentGameData.checkInDay;
	}

	public int[] GetClampItemInfo()
	{
		return _currentGameData.isClampItem;
	}

	public int[] GetClampItemInfoByWorld(int worldId)
	{
		WorldData worldData = GetWorldData(worldId);
		if (worldData != null && worldData.gameData != null)
		{
			return worldData.gameData.isClampItem;
		}
		return _currentGameData.isClampItem;
	}

	public void SetCheckInDay(int day)
	{
		_currentGameData.checkInDay = day;
		SaveGameDataOnly();
	}

	public void SetCheckInDayByWorld(int worldId, int day)
	{
		if (worldId == GetCurrentWorldId())
		{
			_currentGameData.checkInDay = day;
			SaveGameDataOnly();
			return;
		}
		WorldData worldData = GetWorldData(worldId);
		if (worldData != null && worldData.gameData != null)
		{
			worldData.gameData.checkInDay = day;
			SaveWorldDataOnly();
		}
	}

	public void SetClampItemInfoByWorld(int worldId, int[] clampInfo)
	{
		if (worldId == GetCurrentWorldId())
		{
			_currentGameData.isClampItem = clampInfo;
			SaveGameDataOnly();
			return;
		}
		WorldData worldData = GetWorldData(worldId);
		if (worldData != null && worldData.gameData != null)
		{
			worldData.gameData.isClampItem = clampInfo;
			SaveWorldDataOnly();
		}
	}

	private void UpdateAllWorldsCheckInDay(int[] monthDay)
	{
		if (_worldDataCollection == null || _worldDataCollection.worlds == null)
		{
			return;
		}
		foreach (WorldData world in _worldDataCollection.worlds)
		{
			if (world != null && world.gameData != null && world.worldId != GetCurrentWorldId() && (world.gameData.todayMonth != monthDay[0] || world.gameData.today != monthDay[1]))
			{
				world.gameData.todayMonth = monthDay[0];
				world.gameData.today = monthDay[1];
				int checkInDay = world.gameData.checkInDay;
				checkInDay++;
				if (checkInDay > 7)
				{
					checkInDay = 1;
					ResetClampItem(world.gameData);
				}
				world.gameData.checkInDay = checkInDay;
			}
		}
	}

	private void ResetClampItem(GameData gameData)
	{
		if (gameData != null && gameData.isClampItem != null)
		{
			for (int i = 0; i < gameData.isClampItem.Length; i++)
			{
				gameData.isClampItem[i] = 0;
			}
		}
	}

	public void SetCheckInDateByWorld(int worldId, int month, int day)
	{
		if (worldId == GetCurrentWorldId())
		{
			_currentGameData.todayMonth = month;
			_currentGameData.today = day;
			SaveGameDataOnly();
			return;
		}
		WorldData worldData = GetWorldData(worldId);
		if (worldData != null && worldData.gameData != null)
		{
			worldData.gameData.todayMonth = month;
			worldData.gameData.today = day;
			SaveWorldDataOnly();
		}
	}

	public int[] GetTodayDateNumber()
	{
		DateTime dateTime = DateTime.UtcNow.AddHours(8.0);
		return new int[2] { dateTime.Month, dateTime.Day };
	}

	public int GetChallengeId()
	{
		return _currentGameData.currentChallengeId;
	}

	public void SetChallengeId(int id)
	{
		_currentGameData.currentChallengeId = id;
		SaveGameDataOnly();
	}

	public int GetChallengeLevel()
	{
		return _currentGameData.challengeLevel;
	}

	public void SetChallengeLevel(int sum)
	{
		_currentGameData.challengeLevel = sum;
		SaveGameDataOnly();
	}

	public int GetHelperADLevel()
	{
		return _currentUpgradeData.helperADLevel;
	}

	public void UpgradeHelperADLevel()
	{
		_currentUpgradeData.helperADLevel += 5;
		SaveUpgradeDataOnly();
	}

	public void ResetHelperADLevel()
	{
		_currentUpgradeData.helperADLevel = 5;
		SaveUpgradeDataOnly();
	}

	public void SetSupperSixCount(float count)
	{
		_currentGameData.supperSixCount = count;
	}

	public float GetSupperSixCount()
	{
		return _currentGameData.supperSixCount;
	}

	public void SetSupperSixTarget(float count)
	{
		_currentGameData.supperSixTarget = count;
		SaveGameDataOnly();
	}

	public float GetSupperSixTarget()
	{
		return _currentGameData.supperSixTarget;
	}

	public void RestoreData(ServerData serverData)
	{
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Invalid comparison between Unknown and I4
		if (serverData != null && TryRestoreWorldData(serverData.gameDataSave))
		{
			TryRestoreWorldCupFromServerTalent(serverData);
			if (serverData.playerPrefsDataSave != null && serverData.playerPrefsDataSave.playerPrefsData != null)
			{
				playerPrefsData = serverData.playerPrefsDataSave.playerPrefsData;
				LoadPlayerPrefsDataFromFile(serverData.playerPrefsDataSave);
			}
			SingletonMono<UpgradeManager>.Instance?.LoadData();
			SaveGameData();
			originalLastExitTime = _currentGameData.lastExitGameTime;
			ForceUpdateData();
			return;
		}
		_currentGameData = serverData.gameDataSave.gameData;
		if (_currentGameData == null)
		{
			_currentGameData = new GameData();
		}
		_currentFacilityData = serverData.facilityDataSave.facilityData;
		if (_currentFacilityData == null)
		{
			_currentFacilityData = new FacilityData();
		}
		_currentUpgradeData = serverData.upgradeDataSave.upgradeData;
		if (_currentUpgradeData == null)
		{
			_currentUpgradeData = new UpgradeData();
		}
		_currentTalentData = serverData.talentDataSave?.talentData;
		if (_currentTalentData == null)
		{
			_currentTalentData = new TalentData();
		}
		else
		{
			_currentTalentData.IsNeedResetTalent = 10;
			PlayerPrefs.SetInt(Constants.firstReset, 10);
			PlayerPrefs.Save();
		}
		if (!TryRestoreWorldCupFromCompact(_currentTalentData.worldCupCompactData, "服务器 TalentData") && _currentWorldCupData == null)
		{
			_currentWorldCupData = new WorldCupData();
		}
		playerPrefsData = serverData.playerPrefsDataSave?.playerPrefsData;
		LoadPlayerPrefsDataFromFile(serverData.playerPrefsDataSave);
		Debug.Log((object)"数据恢复成功1");
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
		{
			SingletonMono<UpgradeManager>.Instance.LoadData();
		}
		SaveGameData();
		originalLastExitTime = _currentGameData.lastExitGameTime;
		Debug.Log((object)"数据恢复成功2");
		ForceUpdateData();
		Debug.Log((object)"数据恢复成功3");
		BroadcastManager.ShowPopUpTip("数据恢复成功,如果小游戏没有自动重启,请手动点击重启游戏");
		Debug.Log((object)"数据恢复成功4");
		if ((int)Application.platform == 50)
		{
			TT.RestartMiniProgramSync();
		}
	}
}

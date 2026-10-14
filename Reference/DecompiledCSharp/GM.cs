using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Module.Http;
using Module.Mission;
using Module.Shop;
using Module.Vat;
using Spine.Unity;
using TMPro;
using TTSDK;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using cfg;

public class GM : SingletonMono<GM>
{
	private class PlayerPrefsSnapshot
	{
		public string restoreCooldownTime;

		public Dictionary<string, int> shopDailyCounters = new Dictionary<string, int>();
	}

	private Canvas gmCanvas;

	private RectTransform buttonContainer;

	private List<Button> buttons = new List<Button>();

	private const float BUTTON_WIDTH = 320f;

	private const float BUTTON_HEIGHT = 80f;

	private const float BUTTON_SPACING = 15f;

	private const float COLUMN_SPACING = 20f;

	private int maxColumns;

	private int buttonsPerColumn;

	private TextMeshProUGUI itemInfoText;

	private GameObject addItemPanel;

	private TMP_InputField itemIdInput;

	private TMP_InputField itemCountInput;

	private Button confirmAddItemButton;

	private GameObject addSeasoningPanel;

	private TMP_InputField seasoningIdInput;

	private Button confirmAddSeasoningButton;

	private GameObject shareHelpPanel;

	private TMP_InputField helperOpenIdInput;

	private TMP_InputField targetOpenIdInput;

	private Button confirmShareHelpButton;

	private bool _hasAddedGoldOnce;

	private bool _hasAddedDiamondOnce;

	private Dictionary<int, long> _isAddedFragment = new Dictionary<int, long>();

	private bool isShowItemInfoText;

	private Coroutine enableClickRoutine;

	public override bool DontDestory => true;

	protected override void OnAwake()
	{
		CreateGMCanvas();
		CreateTestButtons();
		ToggleGMUI(show: false);
	}

	protected override void BeforOnDestroy()
	{
	}

	private void CreateGMCanvas()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected O, but got Unknown
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject("GM_Canvas");
		gmCanvas = val.AddComponent<Canvas>();
		gmCanvas.renderMode = (RenderMode)0;
		gmCanvas.sortingOrder = 1000;
		Object.DontDestroyOnLoad((Object)(object)val);
		CanvasScaler obj = val.AddComponent<CanvasScaler>();
		obj.uiScaleMode = (ScaleMode)1;
		obj.referenceResolution = new Vector2(1440f, 2880f);
		obj.matchWidthOrHeight = 0f;
		val.GetOrAddComponent<RectTransform>();
		val.AddComponent<GraphicRaycaster>();
		float num = 1420f;
		float num2 = 340f;
		maxColumns = Mathf.FloorToInt(num / num2);
		maxColumns = Mathf.Max(1, maxColumns);
		GameObject val2 = new GameObject("ButtonContainer");
		val2.transform.SetParent(val.transform);
		buttonContainer = val2.AddComponent<RectTransform>();
		buttonContainer.anchorMin = new Vector2(0f, 1f);
		buttonContainer.anchorMax = new Vector2(0f, 1f);
		buttonContainer.pivot = new Vector2(0f, 1f);
		buttonContainer.anchoredPosition = new Vector2(20f, -20f);
		buttonContainer.sizeDelta = new Vector2(num2 * (float)maxColumns, 0f);
		GameObject val3 = new GameObject("Background");
		val3.transform.SetParent(val.transform);
		val3.transform.localPosition = Vector3.zero;
		val3.transform.localScale = Vector3.one;
		((Graphic)val3.AddComponent<Image>()).color = new Color(0f, 0f, 0f, 0.9f);
		RectTransform component = val3.GetComponent<RectTransform>();
		component.anchorMin = Vector2.zero;
		component.anchorMax = Vector2.one;
		component.pivot = new Vector2(0.5f, 0.5f);
		component.sizeDelta = Vector2.zero;
		component.anchoredPosition = Vector2.zero;
		((Transform)component).SetAsFirstSibling();
		CreateItemInfoText(val);
	}

	private void CreateItemInfoText(GameObject parent)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject("ItemInfoText");
		val.transform.SetParent(parent.transform);
		itemInfoText = val.AddComponent<TextMeshProUGUI>();
		((TMP_Text)itemInfoText).fontSize = 60f;
		((Graphic)itemInfoText).color = Color.white;
		((TMP_Text)itemInfoText).alignment = (TextAlignmentOptions)257;
		RectTransform component = val.GetComponent<RectTransform>();
		component.anchorMin = new Vector2(0f, 0f);
		component.anchorMax = new Vector2(1f, 0f);
		component.pivot = new Vector2(0f, 0f);
		component.sizeDelta = new Vector2(0f, 600f);
		component.anchoredPosition = new Vector2(20f, 20f);
		val.SetActive(false);
	}

	private void CreateTestButtons()
	{
		AddGMButton("关闭GM", delegate
		{
			ToggleGMUI(show: false);
		});
		AddGMButton("关闭GM", delegate
		{
			ToggleGMUI(show: false);
		});
		AddGMButton("关闭GM", delegate
		{
			ToggleGMUI(show: false);
		});
		AddGMButton("退出副本", delegate
		{
			ToggleGMUI(show: false);
		});
		AddGMButton("显示物品信息", delegate
		{
			ShowItemInfoText(!isShowItemInfoText);
		});
		AddGMButton("重置玩家信息", delegate
		{
			ResetAllPlayerData(istest: true);
		});
		AddGMButton("清空GameData", delegate
		{
			ClearSingleDataAndRestart("game_data.json");
		});
		AddGMButton("清空天赋数据", delegate
		{
			ClearSingleDataAndRestart("talent_data.json");
		});
		AddGMButton("清空设施数据", delegate
		{
			ClearSingleDataAndRestart("facility_data.json");
		});
		AddGMButton("清空升级数据", delegate
		{
			DeleteUpgradeSaveAndRestart();
		});
		AddGMButton("删除骰子世界存档", delegate
		{
			DeleteDiceWorldSaveAndRestart();
		});
		AddGMButton("删除天赋存档", delegate
		{
			DeleteTalentSaveAndRestart();
		});
		AddGMButton("删除帮手存档", delegate
		{
			DeleteHelperSaveAndRestart();
		});
		AddGMButton("删除升级存档", delegate
		{
			DeleteUpgradeSaveAndRestart();
		});
		AddGMButton("主线关卡+1", delegate
		{
			if ((Object)(object)MissionManager.Instance != (Object)null)
			{
				MissionManager.Instance.GMCompleteMission();
			}
		});
		AddGMButton("世界1升到解锁等级", PrepareWorldTwoUnlock);
		AddGMButton("切换世界2", delegate
		{
			SwitchWorldByGM(2);
		});
		AddGMButton("切换世界1", delegate
		{
			SwitchWorldByGM(1);
		});
		AddGMButton("增加物品", delegate
		{
			ToggleAddItemPanel(show: true);
		});
		AddGMButton("GM去广告", delegate
		{
			FakeVideoManager.Instance.RemoveAd();
		});
		AddGMButton("增加金钱", delegate
		{
			AddGold();
		});
		AddGMButton("增加钻石", delegate
		{
			AddDiamond();
		});
		AddGMButton("增加拍卖金钱", delegate
		{
			AddAuctionCoin();
		});
		AddGMButton("清空金钱", delegate
		{
			ClearGold();
		});
		AddGMButton("增加天赋点", delegate
		{
			AddTalentPoint();
		});
		AddGMButton("清空天赋点", delegate
		{
			ClearTalentPoint();
		});
		AddGMButton("劳动力+1", delegate
		{
			AddLaoDongLi();
		});
		AddGMButton("全开最大帮手", delegate
		{
			UnlockAndMaxAllHelpers();
		});
		AddGMButton("增加签到天数", delegate
		{
			AddCheckInDay();
		});
		AddGMButton("重置签到天数", delegate
		{
			ResetCheckInDay();
		});
		AddGMButton("世界杯天数+1", delegate
		{
			AddWorldCupSignDay();
		});
		AddGMButton("重置世界杯签到", delegate
		{
			ResetWorldCupSignDay();
		});
		AddGMButton("开启限时挑战", delegate
		{
			ChallengeStart();
		});
		AddGMButton("关闭限时挑战", delegate
		{
			ChallengeOff();
		});
		AddGMButton("增加101碎片", delegate
		{
			AddFragment(101);
		});
		AddGMButton("增加102碎片", delegate
		{
			AddFragment(102);
		});
		AddGMButton("增加103碎片", delegate
		{
			AddFragment(103);
		});
		AddGMButton("增加104碎片", delegate
		{
			AddFragment(104);
		});
		AddGMButton("增加105碎片", delegate
		{
			AddFragment(105);
		});
		AddGMButton("增加106碎片", delegate
		{
			AddFragment(106);
		});
		AddGMButton("增加107碎片", delegate
		{
			AddFragment(107);
		});
		AddGMButton("增加108碎片", delegate
		{
			AddFragment(108);
		});
		AddGMButton("增加109碎片", delegate
		{
			AddFragment(109);
		});
		AddGMButton("增加110碎片", delegate
		{
			AddFragment(110);
		});
		AddGMButton("清除每日抽奖", delegate
		{
			PlayerPrefs.DeleteKey(ShopDailyLimit.GetKey(990001));
			PlayerPrefs.DeleteKey(ShopDailyLimit.GetKey(990002));
			PlayerPrefs.DeleteKey(ShopDailyLimit.GetKey(201));
			PlayerPrefs.DeleteKey(ShopDailyLimit.GetKey(202));
			PlayerPrefs.DeleteKey(ShopDailyLimit.GetKey(203));
		});
		AddGMButton("商城次数不限", delegate
		{
			ShopDailyLimit.IsUnlimited = !ShopDailyLimit.IsUnlimited;
			Debug.Log((object)("商城不限次数模式：" + (ShopDailyLimit.IsUnlimited ? "已开启" : "已关闭")));
		});
		AddGMButton("分享助力", delegate
		{
			ToggleShareHelpPanel(show: true);
		});
		AddGMButton("不恢复清档", delegate
		{
			ResetAllPlayerData();
			PlayerPrefs.DeleteAll();
			GameDataManager.Instance.SaveGameData();
			GameDataManager.Instance.SaveWorldDataOnly();
		});
		AddGMButton("保存数据到服务器", delegate
		{
			GameDataManager.Instance.SaveGameData();
			GameDataManager.Instance.SaveAll2Server();
			BroadcastManager.ShowPopUpTip("正在请求保存数据到服务器...");
		});
		AddGMButton("主动请求订阅", delegate
		{
			if ((Object)(object)TTFeedDirectPlayManager.Instance != (Object)null)
			{
				TTFeedDirectPlayManager.Instance.ZhuDongRequestFeedSubscribe(delegate
				{
					TTFeedDirectPlayManager.Instance.ReportRevisitData();
					BroadcastManager.ShowPopUpTip("主动订阅成功，并已调用 ReportRevisitData 上报复访数据");
				}, delegate
				{
					BroadcastManager.ShowPopUpTip("主动订阅失败");
				});
			}
			else
			{
				BroadcastManager.ShowPopUpTip("TTFeedDirectPlayManager 未初始化");
			}
		});
	}

	private static void PrepareWorldTwoUnlock()
	{
		if ((Object)(object)GameDataManager.Instance == (Object)null || !ConfigManager.HasInstance())
		{
			ShowWorldGMResult("世界数据或配置尚未初始化", isError: true);
			return;
		}
		world worldConfig = ConfigManager.Instance.GetWorldConfig(2);
		if (worldConfig == null || worldConfig.PreWorldId <= 0)
		{
			ShowWorldGMResult("未找到世界2或其前置世界配置", isError: true);
			return;
		}
		if (!GameDataManager.Instance.TrySwitchWorld(worldConfig.PreWorldId, out var error))
		{
			ShowWorldGMResult(error, isError: true);
			return;
		}
		int requiredPreWorldLevel = worldConfig.RequiredPreWorldLevel;
		int currentMainMissionLevel = GameDataManager.Instance.GetCurrentMainMissionLevel();
		if (currentMainMissionLevel < requiredPreWorldLevel)
		{
			GameDataManager.Instance.SetCurrentMainMissionLevel(requiredPreWorldLevel);
			MissionManager.Instance?.ReloadCurrentWorld();
		}
		string arg = ConfigManager.Instance.GetWorldConfig(worldConfig.PreWorldId)?.WorldName ?? $"世界{worldConfig.PreWorldId}";
		ShowWorldGMResult($"{arg}已达到{Math.Max(currentMainMissionLevel, requiredPreWorldLevel)}级，可切换世界2", isError: false);
	}

	private static void SwitchWorldByGM(int worldId)
	{
		if ((Object)(object)GameDataManager.Instance == (Object)null)
		{
			ShowWorldGMResult("世界数据尚未初始化", isError: true);
			return;
		}
		if (!GameDataManager.Instance.TrySwitchWorld(worldId, out var error))
		{
			ShowWorldGMResult(error, isError: true);
			return;
		}
		string text = ((!ConfigManager.HasInstance()) ? null : ConfigManager.Instance.GetWorldConfig(worldId)?.WorldName);
		ShowWorldGMResult("已切换到" + (string.IsNullOrEmpty(text) ? $"世界{worldId}" : text), isError: false);
		SingletonMono<GM>.Instance?.ToggleGMUI(show: false);
	}

	private static void ShowWorldGMResult(string message, bool isError)
	{
		if (isError)
		{
			Debug.LogError((object)("[GM][World] " + message));
		}
		else
		{
			Debug.Log((object)("[GM][World] " + message));
		}
		BroadcastManager.ShowPopUpTip(message);
	}

	private void AddVatPerformanceButtons()
	{
		AddGMButton("VAT 硬币显示", delegate
		{
			bool flag = VatDebugSettings.ToggleCoins();
			ApplyVatDebugSettings($"硬币显示: {flag}");
		});
		AddGMButton("VAT 角色显示", delegate
		{
			bool flag = VatDebugSettings.ToggleRoles();
			ApplyVatDebugSettings($"角色显示: {flag}");
		});
		AddGMButton("VAT 技能特效", delegate
		{
			bool flag = VatDebugSettings.ToggleSkillEffects();
			ApplyVatDebugSettings($"技能特效: {flag}");
		});
		AddGMButton("VAT 落地烟雾", delegate
		{
			bool flag = VatDebugSettings.ToggleCoinSmoke();
			ApplyVatDebugSettings($"落地烟雾: {flag}");
		});
		AddGMButton("VAT 收益飘字", delegate
		{
			bool flag = VatDebugSettings.ToggleFloatingText();
			ApplyVatDebugSettings($"收益飘字: {flag}");
		});
		AddRoleSkillVisualButton(2, "龙卷风");
		AddRoleSkillVisualButton(3, "魔术师");
		AddRoleSkillVisualButton(4, "功夫");
		AddRoleSkillVisualButton(5, "篮球");
		AddRoleSkillVisualButton(6, "大蛇");
		AddRoleSkillVisualButton(7, "妲己");
		AddRoleSkillVisualButton(8, "钢铁人");
		AddRoleSkillVisualButton(9, "哈兰德");
		AddGMButton("VAT 扫描UI Spine", DumpVatUiSpineUsage);
	}

	private void AddRoleSkillVisualButton(int roleType, string roleName)
	{
		AddGMButton("VAT " + roleName + "特效", delegate
		{
			bool flag = VatDebugSettings.ToggleRoleSkillVisuals(roleType);
			ApplyVatDebugSettings($"{roleName}技能特效: {flag}");
		});
	}

	private static void ApplyVatDebugSettings(string message)
	{
		(UIManager.HasInstance() ? UIManager.Instance.MainSceneVat : null)?.ApplyDebugVisualSettings();
		Debug.Log((object)("[GM][VAT] " + message));
	}

	private static void DumpVatUiSpineUsage()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		SkeletonGraphic[] array = Resources.FindObjectsOfTypeAll<SkeletonGraphic>();
		int num = 0;
		int num2 = 0;
		foreach (SkeletonGraphic val in array)
		{
			if ((Object)(object)val == (Object)null)
			{
				continue;
			}
			Scene scene = ((Component)val).gameObject.scene;
			if (((Scene)(ref scene)).IsValid())
			{
				num++;
				bool flag = ((Behaviour)val).isActiveAndEnabled && ((Component)val).gameObject.activeInHierarchy;
				if (flag)
				{
					num2++;
				}
				Debug.Log((object)$"[GM][VAT][UI Spine] active={flag}, path={GetHierarchyPath(((Component)val).transform)}");
			}
		}
		Debug.Log((object)$"[GM][VAT][UI Spine] loaded={num}, active={num2}");
	}

	private static string GetHierarchyPath(Transform target)
	{
		if ((Object)(object)target == (Object)null)
		{
			return "<null>";
		}
		string text = ((Object)target).name;
		Transform parent = target.parent;
		while ((Object)(object)parent != (Object)null)
		{
			text = ((Object)parent).name + "/" + text;
			parent = parent.parent;
		}
		return text;
	}

	private void AddGold()
	{
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance == (Object)null)
		{
			Debug.LogError((object)"UpgradeManager 未初始化！");
			return;
		}
		double totalGold = SingletonMono<UpgradeManager>.Instance.TotalGold;
		if (!_hasAddedGoldOnce)
		{
			SingletonMono<UpgradeManager>.Instance.AddGoldInternal(1000.0);
			_hasAddedGoldOnce = true;
			Debug.Log((object)$"增加金钱：+1000，当前金钱：{SingletonMono<UpgradeManager>.Instance.TotalGold}");
		}
		else
		{
			double num = totalGold * 10.0;
			SingletonMono<UpgradeManager>.Instance.AddGoldInternal(num);
			Debug.Log((object)$"增加金钱：{totalGold} * 10 = {num}");
		}
	}

	private void AddDiamond()
	{
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance == (Object)null)
		{
			Debug.LogError((object)"UpgradeManager 未初始化！");
			return;
		}
		long diamond = GameDataManager.Instance.GetDiamond();
		if (!_hasAddedDiamondOnce)
		{
			GameDataManager.Instance.AddDiamond(1000L);
			_hasAddedDiamondOnce = true;
			Debug.Log((object)$"增加钻石：+1000，当前钻石：{GameDataManager.Instance.GetDiamond()}");
		}
		else
		{
			long num = diamond * 10;
			GameDataManager.Instance.AddDiamond(num);
			Debug.Log((object)$"增加钻石：{diamond} * 10 = {num}");
		}
	}

	private void AddAuctionCoin()
	{
		if (AuctionDataManager.Instance == null)
		{
			Debug.LogError((object)"AuctionDataManager 未初始化！");
			return;
		}
		double auctionCoin = AuctionDataManager.Instance.GetAuctionCoin() + 1000.0;
		AuctionDataManager.Instance.SetAuctionCoin(auctionCoin);
		Debug.Log((object)$"增加拍卖行金钱：+1000，当前拍卖行金钱：{AuctionDataManager.Instance.GetAuctionCoin()}");
	}

	public void ResetAllPlayerData(bool istest = false)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Invalid comparison between Unknown and I4
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Invalid comparison between Unknown and I4
		PlayerPrefsSnapshot snapshot = null;
		if ((int)Application.platform == 50 || (int)Application.platform == 17)
		{
			snapshot = CaptureProtectedPrefs();
		}
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
		{
			SingletonMono<UpgradeManager>.Instance.ResetData();
		}
		string text = (((Object)(object)GameDataManager.Instance != (Object)null) ? GameDataManager.Instance.GetOpenId() : "");
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			GameDataManager.Instance.ResetGameData();
		}
		try
		{
			DeleteSaveFile(Path.Combine(Application.persistentDataPath, "game_data.json"));
			DeleteSaveFile(Path.Combine(Application.persistentDataPath, "facility_data.json"));
			DeleteSaveFile(Path.Combine(Application.persistentDataPath, "upgrade_data.json"));
			DeleteSaveFile(Path.Combine(Application.persistentDataPath, "talent_data.json"));
			DeleteSaveFile(Path.Combine(Application.persistentDataPath, "time_info.json"));
			DeleteSaveFile(Path.Combine(Application.persistentDataPath, "task_data.json"));
			DeleteSaveFile(Path.Combine(Application.persistentDataPath, "player_prefs_data.json"));
			DeleteSaveFile(Path.Combine(Application.persistentDataPath, "game_save.json"));
			DeleteSaveFile(Path.Combine(Application.persistentDataPath, "world_cup_data.json"));
			DeleteSaveFile(Path.Combine(Application.persistentDataPath, "global_profile_data.json"));
			DeleteSaveFile(Path.Combine(Application.persistentDataPath, "world_data.json"));
			DeleteSaveFile(Path.Combine(Application.persistentDataPath, "world_archive.json"));
			if (!string.IsNullOrEmpty(text))
			{
				DeleteSaveFile(Path.Combine(Application.persistentDataPath, text + "_game_data.json"));
				DeleteSaveFile(Path.Combine(Application.persistentDataPath, text + "_facility_data.json"));
				DeleteSaveFile(Path.Combine(Application.persistentDataPath, text + "_upgrade_data.json"));
				DeleteSaveFile(Path.Combine(Application.persistentDataPath, text + "_talent_data.json"));
				DeleteSaveFile(Path.Combine(Application.persistentDataPath, text + "_time_info.json"));
				DeleteSaveFile(Path.Combine(Application.persistentDataPath, text + "_task_data.json"));
				DeleteSaveFile(Path.Combine(Application.persistentDataPath, text + "_player_prefs_data.json"));
				DeleteSaveFile(Path.Combine(Application.persistentDataPath, text + "_game_save.json"));
				DeleteSaveFile(Path.Combine(Application.persistentDataPath, text + "_world_cup_data.json"));
				DeleteSaveFile(Path.Combine(Application.persistentDataPath, text + "_global_profile_data.json"));
				DeleteSaveFile(Path.Combine(Application.persistentDataPath, text + "_world_data.json"));
				DeleteSaveFile(Path.Combine(Application.persistentDataPath, text + "_world_archive.json"));
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("删除存档文件失败: " + ex.Message));
		}
		PlayerPrefs.DeleteAll();
		if ((int)Application.platform == 50 || (int)Application.platform == 17)
		{
			RestoreProtectedPrefs(snapshot);
		}
		PlayerPrefs.Save();
		if ((Object)(object)GameDataManager.Instance != (Object)null && istest)
		{
			GameDataManager.Instance.skipSave = true;
			Object.Destroy((Object)(object)((Component)GameDataManager.Instance).gameObject);
		}
		Debug.Log((object)"已重置所有玩家数据！");
	}

	public void ClearSingleDataAndRestart(string fileName)
	{
		try
		{
			DeleteSaveFile(Path.Combine(Application.persistentDataPath, fileName));
			DeleteSaveFile(Path.Combine(Application.persistentDataPath, "world_archive.json"));
			DeleteSaveFile(Path.Combine(Application.persistentDataPath, "world_data.json"));
			DeleteSaveFile(Path.Combine(Application.persistentDataPath, "world_server_outbox_1.json"));
			DeleteSaveFile(Path.Combine(Application.persistentDataPath, "world_server_outbox_2.json"));
			string text = (((Object)(object)GameDataManager.Instance != (Object)null) ? GameDataManager.Instance.GetOpenId() : "");
			if (!string.IsNullOrEmpty(text))
			{
				DeleteSaveFile(Path.Combine(Application.persistentDataPath, text + "_" + fileName));
				DeleteSaveFile(Path.Combine(Application.persistentDataPath, text + "_world_archive.json"));
				DeleteSaveFile(Path.Combine(Application.persistentDataPath, text + "_world_data.json"));
				DeleteSaveFile(Path.Combine(Application.persistentDataPath, text + "_world_server_outbox_1.json"));
				DeleteSaveFile(Path.Combine(Application.persistentDataPath, text + "_world_server_outbox_2.json"));
			}
			Debug.Log((object)("已成功删除 " + fileName + " 及多世界主存档，准备重启以触发恢复逻辑"));
			if ((Object)(object)GameDataManager.Instance != (Object)null)
			{
				GameDataManager.Instance.skipSave = true;
				Object.Destroy((Object)(object)((Component)GameDataManager.Instance).gameObject);
			}
			RestartMiniGame();
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("删除单一存档文件失败: " + ex.Message));
		}
	}

	private void RestartMiniGame()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if ((int)Application.platform == 50 || (int)Application.platform == 17)
		{
			try
			{
				TT.RestartMiniProgramSync();
				return;
			}
			catch
			{
			}
		}
		try
		{
			Scene activeScene = SceneManager.GetActiveScene();
			SceneManager.LoadScene(((Scene)(ref activeScene)).buildIndex);
		}
		catch
		{
		}
	}

	private PlayerPrefsSnapshot CaptureProtectedPrefs()
	{
		PlayerPrefsSnapshot playerPrefsSnapshot = new PlayerPrefsSnapshot();
		if (PlayerPrefs.HasKey("RestoreButtonClickedTime"))
		{
			playerPrefsSnapshot.restoreCooldownTime = PlayerPrefs.GetString("RestoreButtonClickedTime", string.Empty);
		}
		int[] array = new int[5] { 990001, 990002, 201, 202, 203 };
		for (int i = 0; i < array.Length; i++)
		{
			string key = ShopDailyLimit.GetKey(array[i]);
			if (PlayerPrefs.HasKey(key))
			{
				playerPrefsSnapshot.shopDailyCounters[key] = PlayerPrefs.GetInt(key, 0);
			}
		}
		if (ConfigManager.HasInstance())
		{
			List<shop> allShop = ConfigManager.Instance.GetAllShop();
			if (allShop != null)
			{
				for (int j = 0; j < allShop.Count; j++)
				{
					shop shop = allShop[j];
					if (shop != null && shop.TodayLimit > 0)
					{
						string key2 = ShopDailyLimit.GetKey(shop.Id);
						if (PlayerPrefs.HasKey(key2))
						{
							playerPrefsSnapshot.shopDailyCounters[key2] = PlayerPrefs.GetInt(key2, 0);
						}
					}
				}
			}
		}
		return playerPrefsSnapshot;
	}

	private void RestoreProtectedPrefs(PlayerPrefsSnapshot snapshot)
	{
		if (snapshot == null)
		{
			return;
		}
		if (!string.IsNullOrEmpty(snapshot.restoreCooldownTime))
		{
			PlayerPrefs.SetString("RestoreButtonClickedTime", snapshot.restoreCooldownTime);
		}
		if (snapshot.shopDailyCounters == null)
		{
			return;
		}
		foreach (KeyValuePair<string, int> shopDailyCounter in snapshot.shopDailyCounters)
		{
			PlayerPrefs.SetInt(shopDailyCounter.Key, shopDailyCounter.Value);
		}
	}

	private void DeleteSaveFile(string filePath)
	{
		TryDeleteFile(filePath);
		TryDeleteFile(filePath + ".bak");
	}

	private void TryDeleteFile(string filePath)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		try
		{
			if ((int)Application.platform == 50 || (int)Application.platform == 17)
			{
				TTFileSystemManager fileSystemManager = TT.GetFileSystemManager();
				InvokeDeleteFile(fileSystemManager, filePath);
			}
			else if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}
		}
		catch (Exception)
		{
		}
	}

	private void InvokeDeleteFile(object fileSystemMgr, string filePath)
	{
		if (fileSystemMgr == null)
		{
			return;
		}
		string[] obj = new string[6] { "UnlinkSync", "unlinkSync", "DeleteFileSync", "deleteFileSync", "RemoveFileSync", "removeFileSync" };
		Type type = fileSystemMgr.GetType();
		string[] array = obj;
		foreach (string name in array)
		{
			MethodInfo method = type.GetMethod(name);
			if (!(method == null))
			{
				try
				{
					method.Invoke(fileSystemMgr, new object[1] { filePath });
					break;
				}
				catch (Exception)
				{
				}
			}
		}
	}

	private void ClearGold()
	{
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance == (Object)null)
		{
			Debug.LogError((object)"UpgradeManager 未初始化！");
			return;
		}
		SingletonMono<UpgradeManager>.Instance.SetGold(0.0);
		_hasAddedGoldOnce = false;
		Debug.Log((object)"已清空所有金钱");
	}

	private void AddFragment(int itemId)
	{
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			if (!_isAddedFragment.ContainsKey(itemId))
			{
				_isAddedFragment.Add(itemId, 5L);
			}
			long num = _isAddedFragment[itemId] * 2;
			GameDataManager.Instance.AddItem(itemId, num);
			_isAddedFragment[itemId] = num;
		}
	}

	public void AddTalentPoint()
	{
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			GameDataManager.Instance.ChangeTalentPoint(isAdd: true, 100, TalentSceneType.CoinSceneTalent);
			GameDataManager.Instance.ChangeTalentPoint(isAdd: true, 100, TalentSceneType.DiceSceneTalent);
		}
	}

	public void ClearTalentPoint()
	{
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			GameDataManager.Instance.ChangeTalentPoint(isAdd: false, 9999, TalentSceneType.CoinSceneTalent);
			GameDataManager.Instance.ChangeTalentPoint(isAdd: false, 9999, TalentSceneType.DiceSceneTalent);
		}
	}

	public void AddLaoDongLi()
	{
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			float laoDongLi = GameDataManager.Instance.GetLaoDongLi();
			GameDataManager.Instance.SetLaoDongLi(laoDongLi + 1f);
			Debug.Log((object)$"[GM] 劳动力值 +1，当前值为: {GameDataManager.Instance.GetLaoDongLi()}");
		}
	}

	public void UnlockAndMaxAllHelpers()
	{
		if (!((Object)(object)GameDataManager.Instance != (Object)null))
		{
			return;
		}
		FacilityData facilityData = GameDataManager.Instance.GetFacilityData();
		if (facilityData == null)
		{
			return;
		}
		int id;
		for (id = 1; id <= 10; id++)
		{
			HelperInfo helperInfo = facilityData.helperOwned.Find((HelperInfo h) => h.helperId == id);
			if (helperInfo != null)
			{
				helperInfo.helperLevel = 30;
				helperInfo.helperCount = 0;
				helperInfo.maxCount = 10;
				helperInfo.skillLevel = 30;
				continue;
			}
			HelperInfo item = new HelperInfo
			{
				helperId = id,
				helperLevel = 30,
				helperCount = 10,
				maxCount = 10,
				skillLevel = 1,
				isAdUnlock = 0
			};
			facilityData.helperOwned.Add(item);
		}
		GameDataManager.Instance.SaveFacilityDataOnly();
		if (!string.IsNullOrEmpty(GameDataManager.Instance.GetOpenId()))
		{
			GameDataManager.Instance.SaveFacilityData2Server();
		}
		(((Object)(object)UIManager.Instance != (Object)null) ? UIManager.Instance.ActiveMainScene : null)?.InitializeRoles();
		if ((Object)(object)UpgradePanel.Instance != (Object)null)
		{
			UpgradePanel.Instance.RefreshUpgradeItemsForPanel(1);
		}
		Debug.Log((object)"[GM] 所有帮手已全开，数量设为 10，等级设为 30！");
	}

	public void AddCheckInDay()
	{
		if (!((Object)(object)GameDataManager.Instance != (Object)null))
		{
			return;
		}
		int checkInDay = GameDataManager.Instance.GetCheckInDay();
		checkInDay++;
		if (checkInDay > 7)
		{
			checkInDay = 1;
			GameData gameData = GameDataManager.Instance.GetGameData();
			for (int i = 0; i < gameData.isClampItem.Length; i++)
			{
				gameData.isClampItem[i] = 0;
			}
		}
		GameDataManager.Instance.SetCheckInDay(checkInDay);
		if ((Object)(object)DailyCheckIn.Instance != (Object)null)
		{
			DailyCheckIn.Instance.RefreshUI();
		}
		if ((Object)(object)UIManager.Instance.topPanel != (Object)null)
		{
			UIManager.Instance.topPanel.RefreshCheckInRedDot();
		}
	}

	public void ResetCheckInDay()
	{
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			GameDataManager.Instance.SetCheckInDay(1);
			GameData gameData = GameDataManager.Instance.GetGameData();
			for (int i = 0; i < gameData.isClampItem.Length; i++)
			{
				gameData.isClampItem[i] = 0;
			}
			if ((Object)(object)DailyCheckIn.Instance != (Object)null)
			{
				DailyCheckIn.Instance.RefreshUI();
			}
			if ((Object)(object)UIManager.Instance.topPanel != (Object)null)
			{
				UIManager.Instance.topPanel.RefreshCheckInRedDot();
			}
		}
	}

	public void AddWorldCupSignDay()
	{
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			int worldCupSignDay = WorldCupDataManager.Instance.GetWorldCupSignDay();
			worldCupSignDay++;
			if (worldCupSignDay >= 7)
			{
				worldCupSignDay = 7;
				Debug.Log((object)$"[GM] 世界杯签到天数已达到上限: {worldCupSignDay} 天");
			}
			else
			{
				Debug.Log((object)$"[GM] 世界杯签到天数增加: {worldCupSignDay} 天");
			}
			WorldCupDataManager.Instance.SetWorldCupSignDay(worldCupSignDay);
			if ((Object)(object)WorldCupPanel.Instance != (Object)null)
			{
				WorldCupPanel.Instance.RefrashPanel(1);
			}
		}
	}

	public void ResetWorldCupSignDay()
	{
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			WorldCupDataManager.Instance.SetWorldCupSignDay(1);
			int[] wroldCupItems = WorldCupDataManager.Instance.GetWroldCupItems();
			for (int i = 0; i < wroldCupItems.Length; i++)
			{
				wroldCupItems[i] = 0;
			}
			if ((Object)(object)WorldCupPanel.Instance != (Object)null)
			{
				WorldCupPanel.Instance.RefrashPanel(1);
			}
		}
	}

	public void ChallengeStart()
	{
		DownItem downItem = UIManager.Instance.GetDownItem();
		if ((Object)(object)downItem != (Object)null)
		{
			downItem.ChallengeStartGM();
		}
		else
		{
			Debug.LogWarning((object)"DownItem尚未加载，无法开启限时挑战！");
		}
	}

	public void ChallengeOff()
	{
		DownItem downItem = UIManager.Instance.GetDownItem();
		if ((Object)(object)downItem != (Object)null)
		{
			downItem.ChallengeOffGm();
		}
		else
		{
			Debug.LogWarning((object)"DownItem尚未加载，无法关闭限时挑战！");
		}
	}

	public void AddGMButton(string buttonText, Action onClick)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Expected O, but got Unknown
		if ((Object)(object)gmCanvas == (Object)null || (Object)(object)buttonContainer == (Object)null)
		{
			Debug.LogError((object)"GM Canvas未初始化!");
			return;
		}
		GameObject val = new GameObject($"GMButton_{buttons.Count}");
		val.transform.SetParent(((Component)buttonContainer).transform);
		((Graphic)val.AddComponent<Image>()).color = Color.gray;
		Button val2 = val.AddComponent<Button>();
		RectTransform component = val.GetComponent<RectTransform>();
		component.anchorMin = new Vector2(0f, 1f);
		component.anchorMax = new Vector2(0f, 1f);
		component.pivot = new Vector2(0f, 1f);
		component.sizeDelta = new Vector2(320f, 80f);
		int num = buttons.Count % maxColumns;
		int num2 = buttons.Count / maxColumns;
		float num3 = (float)num * 340f;
		float num4 = -95f * (float)num2;
		component.anchoredPosition = new Vector2(num3, num4);
		GameObject val3 = new GameObject("ButtonText");
		val3.transform.localPosition = Vector3.zero;
		val3.transform.SetParent(val.transform);
		TextMeshProUGUI obj = val3.AddComponent<TextMeshProUGUI>();
		((TMP_Text)obj).text = buttonText;
		((TMP_Text)obj).fontSize = 40f;
		((Graphic)obj).color = Color.white;
		RectTransform component2 = val3.GetComponent<RectTransform>();
		component2.sizeDelta = new Vector2(320f, 80f);
		component2.anchoredPosition = new Vector2(0f, 0f);
		ColorBlock colors = ((Selectable)val2).colors;
		((ColorBlock)(ref colors)).normalColor = Color.gray;
		((ColorBlock)(ref colors)).highlightedColor = Color.white;
		((ColorBlock)(ref colors)).pressedColor = Color.black;
		((Selectable)val2).colors = colors;
		((UnityEvent)val2.onClick).AddListener((UnityAction)delegate
		{
			onClick?.Invoke();
		});
		buttons.Add(val2);
	}

	private void PrintPlayerInfo()
	{
		Debug.Log((object)"打印玩家信息 - 示例方法");
	}

	private void Update()
	{
		if (Input.GetKeyDown((KeyCode)27))
		{
			if ((Object)(object)addItemPanel != (Object)null && addItemPanel.activeSelf)
			{
				ToggleAddItemPanel(show: false);
			}
			if ((Object)(object)shareHelpPanel != (Object)null && shareHelpPanel.activeSelf)
			{
				ToggleShareHelpPanel(show: false);
			}
		}
	}

	private void ShowItemInfoText(bool show)
	{
		isShowItemInfoText = show;
		if ((Object)(object)itemInfoText != (Object)null)
		{
			((Component)itemInfoText).gameObject.SetActive(show);
			((Graphic)itemInfoText).raycastTarget = false;
			if (show)
			{
				UpdateItemInfoText();
			}
		}
	}

	public void ToggleGMUI(bool show)
	{
		if (!((Object)(object)gmCanvas != (Object)null))
		{
			return;
		}
		if (show)
		{
			GraphicRaycaster component = ((Component)gmCanvas).GetComponent<GraphicRaycaster>();
			if ((Object)(object)component != (Object)null)
			{
				((Behaviour)component).enabled = false;
				if (enableClickRoutine != null)
				{
					((MonoBehaviour)this).StopCoroutine(enableClickRoutine);
				}
				enableClickRoutine = ((MonoBehaviour)this).StartCoroutine(EnableGraphicRaycasterDelay(component));
			}
			((Component)gmCanvas).gameObject.SetActive(true);
			return;
		}
		if (enableClickRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(enableClickRoutine);
			enableClickRoutine = null;
		}
		((Component)gmCanvas).gameObject.SetActive(false);
		if ((Object)(object)addItemPanel != (Object)null)
		{
			addItemPanel.SetActive(false);
		}
		if ((Object)(object)addSeasoningPanel != (Object)null)
		{
			addSeasoningPanel.SetActive(false);
		}
		if ((Object)(object)shareHelpPanel != (Object)null)
		{
			shareHelpPanel.SetActive(false);
		}
	}

	private IEnumerator EnableGraphicRaycasterDelay(GraphicRaycaster raycaster)
	{
		yield return (object)new WaitForSecondsRealtime(0.3f);
		if ((Object)(object)raycaster != (Object)null)
		{
			((Behaviour)raycaster).enabled = true;
		}
		enableClickRoutine = null;
	}

	private void UpdateItemInfoText()
	{
	}

	private void ToggleAddItemPanel(bool show)
	{
		if ((Object)(object)addItemPanel == (Object)null)
		{
			CreateAddItemPanel();
		}
		addItemPanel.SetActive(show);
	}

	private void CreateAddItemPanel()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Expected O, but got Unknown
		if ((Object)(object)gmCanvas == (Object)null)
		{
			Debug.LogError((object)"GM Canvas未初始化!");
			return;
		}
		addItemPanel = new GameObject("AddItemPanel");
		addItemPanel.transform.SetParent(((Component)gmCanvas).transform);
		((Graphic)addItemPanel.AddComponent<Image>()).color = new Color(0f, 0f, 0f, 0.8f);
		RectTransform component = addItemPanel.GetComponent<RectTransform>();
		component.anchorMin = new Vector2(0.5f, 0.5f);
		component.anchorMax = new Vector2(0.5f, 0.5f);
		component.pivot = new Vector2(0.5f, 0.5f);
		component.sizeDelta = new Vector2(1000f, 600f);
		component.anchoredPosition = Vector2.zero;
		CreateInputField(addItemPanel, "物品ID", new Vector2(0f, 80f), ref itemIdInput);
		CreateInputField(addItemPanel, "物品数量", new Vector2(0f, -20f), ref itemCountInput);
		confirmAddItemButton = CreateButton(addItemPanel, "确定", new Vector2(0f, -120f));
		((UnityEvent)confirmAddItemButton.onClick).AddListener(new UnityAction(OnConfirmAddItem));
	}

	private void CreateInputField(GameObject parent, string label, Vector2 position, ref TMP_InputField inputFieldRef)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Expected O, but got Unknown
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject("InputContainer_" + label);
		val.transform.SetParent(parent.transform);
		RectTransform obj = val.AddComponent<RectTransform>();
		obj.sizeDelta = new Vector2(800f, 120f);
		obj.anchoredPosition = position;
		GameObject val2 = new GameObject("Label_" + label);
		val2.transform.SetParent(val.transform);
		TextMeshProUGUI obj2 = val2.AddComponent<TextMeshProUGUI>();
		((TMP_Text)obj2).text = label;
		((TMP_Text)obj2).fontSize = 48f;
		((Graphic)obj2).color = Color.white;
		RectTransform component = val2.GetComponent<RectTransform>();
		component.sizeDelta = new Vector2(200f, 120f);
		component.anchoredPosition = new Vector2(-300f, 0f);
		component.anchorMin = new Vector2(0.5f, 0.5f);
		component.anchorMax = new Vector2(0.5f, 0.5f);
		component.pivot = new Vector2(0.5f, 0.5f);
		GameObject val3 = new GameObject("Input_" + label);
		val3.transform.SetParent(val.transform);
		((Graphic)val3.AddComponent<Image>()).color = Color.white;
		inputFieldRef = val3.AddComponent<TMP_InputField>();
		GameObject val4 = new GameObject("InputText_" + label);
		val4.transform.SetParent(val3.transform);
		TextMeshProUGUI val5 = val4.AddComponent<TextMeshProUGUI>();
		((TMP_Text)val5).fontSize = 48f;
		((Graphic)val5).color = Color.black;
		inputFieldRef.textComponent = (TMP_Text)(object)val5;
		RectTransform component2 = val3.GetComponent<RectTransform>();
		component2.sizeDelta = new Vector2(400f, 80f);
		component2.anchoredPosition = new Vector2(100f, 0f);
		component2.anchorMin = new Vector2(0.5f, 0.5f);
		component2.anchorMax = new Vector2(0.5f, 0.5f);
		component2.pivot = new Vector2(0.5f, 0.5f);
		RectTransform component3 = val4.GetComponent<RectTransform>();
		component3.sizeDelta = new Vector2(360f, 80f);
		component3.anchoredPosition = Vector2.zero;
		component3.anchorMin = new Vector2(0.5f, 0.5f);
		component3.anchorMax = new Vector2(0.5f, 0.5f);
		component3.pivot = new Vector2(0.5f, 0.5f);
		if (!(label == "物品ID"))
		{
			_ = label == "物品数量";
		}
	}

	private Button CreateButton(GameObject parent, string text, Vector2 position)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject("Button_" + text);
		val.transform.SetParent(parent.transform);
		((Graphic)val.AddComponent<Image>()).color = Color.gray;
		Button obj = val.AddComponent<Button>();
		RectTransform component = val.GetComponent<RectTransform>();
		component.sizeDelta = new Vector2(300f, 100f);
		component.anchoredPosition = position;
		component.anchorMin = new Vector2(0.5f, 0.5f);
		component.anchorMax = new Vector2(0.5f, 0.5f);
		component.pivot = new Vector2(0.5f, 0.5f);
		GameObject val2 = new GameObject("Text_" + text);
		val2.transform.SetParent(val.transform);
		TextMeshProUGUI obj2 = val2.AddComponent<TextMeshProUGUI>();
		((TMP_Text)obj2).text = text;
		((TMP_Text)obj2).fontSize = 58f;
		((Graphic)obj2).color = Color.white;
		RectTransform component2 = val2.GetComponent<RectTransform>();
		component2.sizeDelta = new Vector2(300f, 100f);
		component2.anchoredPosition = Vector2.zero;
		component2.anchorMin = new Vector2(0.5f, 0.5f);
		component2.anchorMax = new Vector2(0.5f, 0.5f);
		component2.pivot = new Vector2(0.5f, 0.5f);
		ColorBlock colors = ((Selectable)obj).colors;
		((ColorBlock)(ref colors)).normalColor = Color.gray;
		((ColorBlock)(ref colors)).highlightedColor = Color.white;
		((ColorBlock)(ref colors)).pressedColor = Color.black;
		((Selectable)obj).colors = colors;
		return obj;
	}

	private void OnConfirmAddItem()
	{
		if ((Object)(object)itemIdInput == (Object)null || (Object)(object)itemCountInput == (Object)null)
		{
			Debug.LogError((object)"输入框未初始化!");
			return;
		}
		string text = itemIdInput.text;
		string text2 = itemCountInput.text;
		if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(text2))
		{
			Debug.LogWarning((object)"物品ID和数量不能为空!");
			return;
		}
		if (!int.TryParse(text2, out var result))
		{
			Debug.LogWarning((object)"物品数量必须是有效的数字!");
			return;
		}
		string[] array = text.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length == 0)
		{
			Debug.LogWarning((object)"请输入有效的物品ID!");
			return;
		}
		int num = 0;
		int num2 = 0;
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			string text3 = array2[i].Trim();
			if (int.TryParse(text3, out var result2))
			{
				if (false)
				{
					Debug.Log((object)$"成功添加物品: ID={result2}, 数量={result}");
					num++;
				}
				else
				{
					Debug.LogWarning((object)$"添加物品失败: ID={result2}（可能是无效的物品ID）");
					num2++;
				}
			}
			else
			{
				Debug.LogWarning((object)("无效的物品ID: " + text3));
				num2++;
			}
		}
		Debug.Log((object)$"批量添加物品完成: 成功{num}个, 失败{num2}个");
		itemIdInput.text = "";
		itemCountInput.text = "";
		ToggleAddItemPanel(show: false);
	}

	private void ToggleShareHelpPanel(bool show)
	{
		if ((Object)(object)shareHelpPanel == (Object)null)
		{
			CreateShareHelpPanel();
		}
		if (show && (Object)(object)helperOpenIdInput != (Object)null)
		{
			helperOpenIdInput.text = GameDataManager.Instance.GetOpenId();
		}
		shareHelpPanel.SetActive(show);
	}

	private void CreateShareHelpPanel()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Expected O, but got Unknown
		if ((Object)(object)gmCanvas == (Object)null)
		{
			Debug.LogError((object)"GM Canvas未初始化!");
			return;
		}
		shareHelpPanel = new GameObject("ShareHelpPanel");
		shareHelpPanel.transform.SetParent(((Component)gmCanvas).transform);
		((Graphic)shareHelpPanel.AddComponent<Image>()).color = new Color(0f, 0f, 0f, 0.8f);
		RectTransform component = shareHelpPanel.GetComponent<RectTransform>();
		component.anchorMin = new Vector2(0.5f, 0.5f);
		component.anchorMax = new Vector2(0.5f, 0.5f);
		component.pivot = new Vector2(0.5f, 0.5f);
		component.sizeDelta = new Vector2(1000f, 600f);
		component.anchoredPosition = Vector2.zero;
		CreateInputField(shareHelpPanel, "自己OpenID", new Vector2(0f, 80f), ref helperOpenIdInput);
		CreateInputField(shareHelpPanel, "对方OpenID", new Vector2(0f, -20f), ref targetOpenIdInput);
		confirmShareHelpButton = CreateButton(shareHelpPanel, "确定", new Vector2(0f, -120f));
		((UnityEvent)confirmShareHelpButton.onClick).AddListener(new UnityAction(OnConfirmShareHelp));
	}

	private void OnConfirmShareHelp()
	{
		if ((Object)(object)helperOpenIdInput == (Object)null || (Object)(object)targetOpenIdInput == (Object)null)
		{
			return;
		}
		string text = helperOpenIdInput.text.Trim();
		string text2 = targetOpenIdInput.text.Trim();
		if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(text2))
		{
			Debug.LogWarning((object)"OpenID 不能为空!");
			return;
		}
		SingletonMono<HttpManager>.Instance?.ShareHelp(text2, text, delegate(bool success, string msg)
		{
			Debug.Log((object)("GM 助力请求：" + (success ? "成功" : "失败") + ", 响应: " + msg));
		});
		ToggleShareHelpPanel(show: false);
	}

	private void DeleteTalentSaveAndRestart()
	{
		try
		{
			if ((Object)(object)GameDataManager.Instance != (Object)null)
			{
				TalentData talentData = GameDataManager.Instance.GetTalentData();
				if (talentData != null)
				{
					talentData.IsNeedResetTalent = 0;
					talentData.talentPoint = 0;
					talentData.rebornCurrentLevel = -1;
					talentData.costTalentPoint = new int[3];
					talentData.talentLevel.Clear();
					talentData.talentUnlock.Clear();
					talentData.soundSleep = 0f;
					talentData.goldAge = 0f;
					talentData.laodongli = 1f;
				}
				GameDataManager.Instance.SaveWorldDataOnly();
				if (!string.IsNullOrEmpty(GameDataManager.Instance.GetOpenId()))
				{
					GameDataManager.Instance.SaveTalentData2Server(delegate(bool success, string msg)
					{
						Debug.Log((object)$"[GM] 天赋数据上传服务器结果: {success}, {msg}");
					});
				}
			}
			DeleteSaveFile(Path.Combine(Application.persistentDataPath, "talent_data.json"));
			DeleteSaveFile(Path.Combine(Application.persistentDataPath, "world_archive.json"));
			DeleteSaveFile(Path.Combine(Application.persistentDataPath, "world_data.json"));
			string text = (((Object)(object)GameDataManager.Instance != (Object)null) ? GameDataManager.Instance.GetOpenId() : "");
			if (!string.IsNullOrEmpty(text))
			{
				DeleteSaveFile(Path.Combine(Application.persistentDataPath, text + "_talent_data.json"));
				DeleteSaveFile(Path.Combine(Application.persistentDataPath, text + "_world_archive.json"));
				DeleteSaveFile(Path.Combine(Application.persistentDataPath, text + "_world_data.json"));
			}
			if ((Object)(object)GameDataManager.Instance != (Object)null)
			{
				GameDataManager.Instance.skipSave = true;
				Object.Destroy((Object)(object)((Component)GameDataManager.Instance).gameObject);
			}
			RestartMiniGame();
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("删除天赋存档失败: " + ex.Message));
		}
	}

	private void DeleteDiceWorldSaveAndRestart()
	{
		try
		{
			if ((Object)(object)GameDataManager.Instance != (Object)null)
			{
				(GameDataManager.Instance.GetWorldDataCollection()?.worlds)?.RemoveAll((WorldData w) => w.worldId == 2);
				if (GameDataManager.Instance.GetGlobalProfileData() != null && GameDataManager.Instance.GetGlobalProfileData().currentWorldId == 2)
				{
					GameDataManager.Instance.GetGlobalProfileData().currentWorldId = 1;
				}
				GameDataManager.Instance.SaveWorldDataOnly();
			}
			DeleteSaveFile(Path.Combine(Application.persistentDataPath, "world_server_outbox_2.json"));
			string text = (((Object)(object)GameDataManager.Instance != (Object)null) ? GameDataManager.Instance.GetOpenId() : "");
			if (!string.IsNullOrEmpty(text))
			{
				DeleteSaveFile(Path.Combine(Application.persistentDataPath, text + "_world_server_outbox_2.json"));
			}
			if ((Object)(object)GameDataManager.Instance != (Object)null)
			{
				GameDataManager.Instance.skipSave = true;
				Object.Destroy((Object)(object)((Component)GameDataManager.Instance).gameObject);
			}
			RestartMiniGame();
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("删除骰子世界存档失败: " + ex.Message));
		}
	}

	private void DeleteHelperSaveAndRestart()
	{
		try
		{
			if ((Object)(object)GameDataManager.Instance != (Object)null)
			{
				GameDataManager.Instance.GetFacilityData()?.helperOwned.Clear();
				GameDataManager.Instance.SaveWorldDataOnly();
				if (!string.IsNullOrEmpty(GameDataManager.Instance.GetOpenId()))
				{
					GameDataManager.Instance.SaveFacilityData2Server(delegate(bool success, string msg)
					{
						Debug.Log((object)$"[GM] 帮手/设施数据上传服务器结果: {success}, {msg}");
					});
				}
			}
			DeleteSaveFile(Path.Combine(Application.persistentDataPath, "facility_data.json"));
			DeleteSaveFile(Path.Combine(Application.persistentDataPath, "world_archive.json"));
			DeleteSaveFile(Path.Combine(Application.persistentDataPath, "world_data.json"));
			string text = (((Object)(object)GameDataManager.Instance != (Object)null) ? GameDataManager.Instance.GetOpenId() : "");
			if (!string.IsNullOrEmpty(text))
			{
				DeleteSaveFile(Path.Combine(Application.persistentDataPath, text + "_facility_data.json"));
				DeleteSaveFile(Path.Combine(Application.persistentDataPath, text + "_world_archive.json"));
				DeleteSaveFile(Path.Combine(Application.persistentDataPath, text + "_world_data.json"));
			}
			if ((Object)(object)GameDataManager.Instance != (Object)null)
			{
				GameDataManager.Instance.skipSave = true;
				Object.Destroy((Object)(object)((Component)GameDataManager.Instance).gameObject);
			}
			RestartMiniGame();
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("删除帮手存档失败: " + ex.Message));
		}
	}

	private void DeleteUpgradeSaveAndRestart()
	{
		try
		{
			if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
			{
				SingletonMono<UpgradeManager>.Instance.ResetData();
			}
			if ((Object)(object)GameDataManager.Instance != (Object)null)
			{
				GameDataManager.Instance.ResetUpgradeData();
				GameDataManager.Instance.SaveWorldDataOnly();
				GameDataManager.Instance.SaveGameDataOnly(updateExitTime: false);
			}
			DeleteSaveFile(Path.Combine(Application.persistentDataPath, "upgrade_data.json"));
			DeleteSaveFile(Path.Combine(Application.persistentDataPath, "world_archive.json"));
			DeleteSaveFile(Path.Combine(Application.persistentDataPath, "world_data.json"));
			DeleteSaveFile(Path.Combine(Application.persistentDataPath, "world_server_outbox_1.json"));
			DeleteSaveFile(Path.Combine(Application.persistentDataPath, "world_server_outbox_2.json"));
			string text = (((Object)(object)GameDataManager.Instance != (Object)null) ? GameDataManager.Instance.GetOpenId() : "");
			if (!string.IsNullOrEmpty(text))
			{
				DeleteSaveFile(Path.Combine(Application.persistentDataPath, text + "_upgrade_data.json"));
				DeleteSaveFile(Path.Combine(Application.persistentDataPath, text + "_world_archive.json"));
				DeleteSaveFile(Path.Combine(Application.persistentDataPath, text + "_world_data.json"));
				DeleteSaveFile(Path.Combine(Application.persistentDataPath, text + "_world_server_outbox_1.json"));
				DeleteSaveFile(Path.Combine(Application.persistentDataPath, text + "_world_server_outbox_2.json"));
			}
			if ((Object)(object)UpgradePanel.Instance != (Object)null)
			{
				UpgradePanel.Instance.RefreshForWorldChange();
			}
			if ((Object)(object)GameDataManager.Instance != (Object)null)
			{
				GameDataManager.Instance.skipSave = true;
				Object.Destroy((Object)(object)((Component)GameDataManager.Instance).gameObject);
			}
			RestartMiniGame();
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("删除升级存档失败: " + ex.Message));
		}
	}
}

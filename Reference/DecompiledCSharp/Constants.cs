using UnityEngine;
using cfg;

public static class Constants
{
	public const int COIN_WORLD_ID = 1;

	public const int DICE_WORLD_ID = 2;

	public const float SupportInterval = 300000f;

	public const string RESTORE_COOLDOWN_KEY = "RestoreButtonClickedTime";

	public const int RANDOM_EVENT_WIN_RATE = 42;

	public const float RandomEventDiamondStakeRate = 0.6f;

	public const string GUIDE_STEP_1 = "guide_step_1";

	public const string GUIDE_STEP_2 = "guide_step_2";

	public const string GUIDE_STEP_3 = "guide_step_3";

	public const string GUIDE_STEP_4 = "guide_step_4";

	public const string GUIDE_STEP_5 = "guide_step_5";

	public const string GUIDE_STEP_6 = "guide_step_6";

	public const string GUIDE_STEP_7 = "guide_step_7";

	public const string GUIDE_STEP_8 = "guide_step_8";

	public const string GUIDE_STEP_9 = "guide_step_9";

	public const string GUIDE_STEP_10 = "guide_step_10";

	public const string DICE_WORLD_GUIDE_STEP = "dice_world_guide_step";

	public const string GUIDE4_PROGRESS_KEY = "guide_step_4_progress";

	public static string firstReset = "FirstReset";

	public const string GONGFU_SUPPORT_TIME = "gongfu_support_time";

	public const string DASHE_SUPPORT_TIME = "dashe_support_time";

	public const string GONGFU_POPUP_TRIGGERED = "gongfu_popup_triggered";

	public const string DASHE_POPUP_TRIGGERED = "dashe_popup_triggered";

	public const string GONGFU_AD_STATE = "gongfu_ad_state";

	public const string DASHE_AD_STATE = "dashe_ad_state";

	public const string GONGFU_AD_START_TIME = "gongfu_ad_start_time";

	public const string DASHE_AD_START_TIME = "dashe_ad_start_time";

	public const int INITIAL_GOLD = 1;

	public const int INITIAL_DIAMOND = 2;

	public const int INITIAL_COPPER_COUNT = 3;

	public const int INITIAL_COPPER_HEIGHT = 4;

	public const int INITIAL_COPPER_VALUE = 5;

	public const int INITIAL_SILVER_VALUE = 6;

	public const int INITIAL_GOLD_VALUE = 7;

	public const int BUFF_MASTER_SPEED_BONUS = 8;

	public const int BUFF_MASTER_DURATION_MIN = 9;

	public const int RICH_MASTER_GOLD_BONUS_MIN = 10;

	public const int MONEY_MASTER_GOLD_BONUS = 11;

	public const int MONEY_MASTER_DURATION_MIN = 12;

	public const int OFFLINE_INCOME_BONUS = 13;

	public const int OFFLINE_POPUP_INTERVAL_MIN = 14;

	public const int INITIAL_PURPLE_COIN_VALUE = 15;

	public const int INITIAL_CRIT_RATE = 16;

	public const int INITIAL_CRIT_DAMAGE = 17;

	public const int UPGRADE_SYSTEM_UNLOCK_MISSION = 18;

	public const int UPGRADE_SYSTEM3_UNLOCK_MISSION = 80;

	public const int FACILITY_SYSTEM_UNLOCK_MISSION = 20;

	public const int TALENT_SYSTEM_UNLOCK_MISSION = 21;

	public const int INVITE_HELPED_INCOME_BONUS = 26;

	public const int SHOP_SYSTEM_UNLOCK_MISSION = 27;

	public const int UPGRADE_GUIDE_END_COUNT = 34;

	public const int AUCTION_SYSTEM_UNLOCK_MISSION = 48;

	public const int WORLD_ENTRY_UNLOCK_LEVEL = 59;

	public const int INITIAL_COPPER_DICE_ONE_POINT_VALUE = 60;

	public const int INITIAL_SILVER_DICE_ONE_POINT_VALUE = 61;

	public const int INITIAL_GOLD_DICE_ONE_POINT_VALUE = 62;

	public const int INITIAL_PURPLE_DICE_ONE_POINT_VALUE = 63;

	public const int DICE_TWO_POINT_VALUE_MULTIPLIER = 64;

	public const int DICE_THREE_POINT_VALUE_MULTIPLIER = 65;

	public const int DICE_FOUR_POINT_VALUE_MULTIPLIER = 66;

	public const int DICE_FIVE_POINT_VALUE_MULTIPLIER = 67;

	public const int DICE_SIX_POINT_VALUE_MULTIPLIER = 68;

	public const int INITIAL_DICE_COUNT = 77;

	public const int COIN_CRITICAL_RATE_TALENT_ID = 303;

	public const int DICE_CRITICAL_RATE_TALENT_ID = 302;

	public const int COPPER_DICE_QUALITY_ID = 1;

	public const int SILVER_DICE_QUALITY_ID = 2;

	public const int GOLD_DICE_QUALITY_ID = 3;

	public const int PURPLE_DICE_QUALITY_ID = 4;

	public const int COPPER_COIN_UPGRADE_ITEM_ID = 2;

	public const int SILVER_COIN_UPGRADE_ITEM_ID = 3;

	public const int GOLD_COIN_UPGRADE_ITEM_ID = 4;

	public const int PURPLE_COIN_UPGRADE_ITEM_ID = 5;

	public const int KUNGFU_MASTER_HELPER_ID = 4;

	public const string ITEM_UPGRADE_TYPE_1 = "itemUpgrade1";

	public const string ITEM_UPGRADE_TYPE_2 = "itemUpgrade2";

	public const string ITEM_UPGRADE_TYPE_3 = "itemUpgrade3";

	private const string UPGRADE_ITEM_ICON_DIRECTORY = "Assets/GameRes/icon/item/";

	public const int RANDOM_MONEY_EVENT_PROGRESS_1 = 37;

	public const int RANDOM_MONEY_EVENT_PROGRESS_2 = 38;

	public const int RANDOM_MONEY_EVENT_PROGRESS_3 = 39;

	public const int RANDOM_DIAMOND_EVENT_PROGRESS = 40;

	public static float DalishiExtraBuff = 5f;

	public const float DALISHI_CRIT_RATE = 1f;

	public static float MiracleExtraBuff = 1f;

	public static float MagicianExtraBuff = 10f;

	public static float GongfuAffectArea = 120f;

	private const string ROLE_PREFAB_PATH = "GameRes/RoleSpine/";

	public static float RandomEventWinRate
	{
		get
		{
			if (!ConfigManager.HasInstance())
			{
				return 0.55f;
			}
			float commonFactor = ConfigManager.Instance.GetCommonFactor(42);
			commonFactor = Mathf.Clamp01(commonFactor);
			if (commonFactor <= 0f)
			{
				return 0.55f;
			}
			return commonFactor;
		}
	}

	public static string GetDeskIconPath(int helperId)
	{
		return helperId switch
		{
			1 => "Assets/GameRes/icon/desk/icon_zm01.png", 
			2 => "Assets/GameRes/icon/desk/icon_zm02.png", 
			3 => "Assets/GameRes/icon/desk/icon_zm03.png", 
			4 => "Assets/GameRes/icon/desk/icon_zm04.png", 
			5 => "Assets/GameRes/icon/desk/icon_zm05.png", 
			6 => "Assets/GameRes/icon/desk/icon_zm06.png", 
			7 => "Assets/GameRes/icon/desk/icon_zm07.png", 
			8 => "Assets/GameRes/icon/desk/icon_zm08.png", 
			9 => "Assets/GameRes/icon/desk/icon_zm09.png", 
			101 => "Assets/GameRes/icon/desk/icon_zm101.png", 
			102 => "Assets/GameRes/icon/desk/icon_zm102.png", 
			103 => "Assets/GameRes/icon/desk/icon_zm103.png", 
			104 => "Assets/GameRes/icon/desk/icon_zm104.png", 
			105 => "Assets/GameRes/icon/desk/icon_zm105.png", 
			106 => "Assets/GameRes/icon/desk/icon_zm106.png", 
			107 => "Assets/GameRes/icon/desk/icon_zm107.png", 
			108 => "Assets/GameRes/icon/desk/icon_zm108.png", 
			_ => "Assets/GameRes/icon/desk/icon_zm01.png", 
		};
	}

	public static string GetMoneyPath(int coinType)
	{
		return coinType switch
		{
			1 => (GameDataManager.Instance?.GetCurrentWorldId() ?? 1) switch
			{
				1 => "Assets/GameRes/NewUI/Common/images1/ty_icon_coin01.png", 
				2 => "Assets/GameRes/NewUI/Common/images1/ty_icon_coin02.png", 
				_ => "Assets/GameRes/NewUI/Common/images1/ty_icon_coin01.png", 
			}, 
			2 => "Assets/GameRes/NewUI/Common/images1/ty_icon_diamond01.png", 
			_ => "Assets/GameRes/NewUI/Common/images1/ty_icon_coin01.png", 
		};
	}

	public static string GetShopIconPath(int id)
	{
		int num = GameDataManager.Instance?.GetCurrentWorldId() ?? 1;
		return id switch
		{
			101 => num switch
			{
				1 => "Assets/GameRes/NewUI/Common/images1/ty_icon_coin01.png", 
				2 => "Assets/GameRes/NewUI/Common/images1/ty_icon_coin02.png", 
				_ => "Assets/GameRes/NewUI/Common/images1/ty_icon_coin01.png", 
			}, 
			102 => num switch
			{
				1 => "Assets/GameRes/NewUI/Shop/images/ty_icon_money02.png", 
				2 => "Assets/GameRes/NewUI/Shop/images/ty_icon_money02_dice.png", 
				_ => "Assets/GameRes/NewUI/Shop/images/ty_icon_money02.png", 
			}, 
			103 => num switch
			{
				1 => "Assets/GameRes/NewUI/Shop/images/ty_icon_money03.png", 
				2 => "Assets/GameRes/NewUI/Shop/images/ty_icon_money03_dice.png", 
				_ => "Assets/GameRes/NewUI/Shop/images/ty_icon_money03.png", 
			}, 
			104 => "Assets/GameRes/NewUI/Shop/images/ty_icon_diamond01.png", 
			105 => "Assets/GameRes/NewUI/Shop/images/ty_icon_diamond02.png", 
			106 => "Assets/GameRes/NewUI/Shop/images/ty_icon_diamond03.png", 
			_ => "Assets/GameRes/NewUI/Shop/images/ty_icon_coin01.png", 
		};
	}

	public static string GetNSR(int coinType)
	{
		return coinType switch
		{
			1 => "Assets/GameRes/NewUI/Facility/images/ty_lab_N.png", 
			2 => "Assets/GameRes/NewUI/Facility/images/ty_lab_R.png", 
			3 => "Assets/GameRes/NewUI/Facility/images/ty_lab_SR.png", 
			_ => "Assets/GameRes/NewUI/Facility/images/ty_lab_N.png", 
		};
	}

	public static string GetHelperIconPath(int helperId)
	{
		return helperId switch
		{
			1 => "Assets/GameRes/icon/helper/icon_dali.png", 
			2 => "Assets/GameRes/icon/helper/icon_pifeng.png", 
			3 => "Assets/GameRes/icon/helper/icon_mofa.png", 
			4 => "Assets/GameRes/icon/helper/icon_kungfu.png", 
			5 => "Assets/GameRes/icon/helper/icon_jikun.png", 
			6 => "Assets/GameRes/icon/helper/icon_dashe.png", 
			7 => "Assets/GameRes/icon/helper/icon_jiuwei.png", 
			8 => "Assets/GameRes/icon/helper/icon_gangtierenpng.png", 
			9 => "Assets/GameRes/icon/helper/icon_halande.png", 
			10 => "Assets/GameRes/icon/helper/icon_dushen.png", 
			_ => "Assets/GameRes/icon/helper/icon_dashe.png", 
		};
	}

	public static string GetSuipianIconPath(int itemId)
	{
		switch (itemId)
		{
		case 6:
			return "Assets/GameRes/NewUI/Common/images1/ty_icon_talentPoint.png";
		case 3:
		case 4:
		case 5:
		case 101:
		case 102:
		case 103:
		case 104:
		case 105:
		case 106:
		case 107:
		case 108:
		case 109:
		case 110:
			return $"Assets/GameRes/icon/suipian/{itemId}.png";
		default:
			Debug.LogError((object)$"GetSuipianIconPath: itemId {itemId} not found");
			return $"Assets/GameRes/icon/suipian/{itemId}.png";
		}
	}

	public static string GetRolePrefabPath(int roleType)
	{
		return roleType switch
		{
			0 => "Assets/GameRes/RoleSpine/role.prefab", 
			1 => "Assets/GameRes/RoleSpine/Role_toughguy.prefab", 
			2 => "Assets/GameRes/RoleSpine/Role_miracle.prefab", 
			3 => "Assets/GameRes/RoleSpine/Role_magician.prefab", 
			4 => "Assets/GameRes/RoleSpine/Role_gongfu.prefab", 
			5 => "Assets/GameRes/RoleSpine/Role_cxk.prefab", 
			6 => "Assets/GameRes/RoleSpine/Role_dashe.prefab", 
			7 => "Assets/GameRes/RoleSpine/Role_daji.prefab", 
			8 => "Assets/GameRes/RoleSpine/Role_iron.prefab", 
			9 => "Assets/GameRes/RoleSpine/Role_halande.prefab", 
			10 => "Assets/GameRes/RoleSpine/Role_dushen.prefab", 
			_ => "Assets/GameRes/RoleSpine/role.prefab", 
		};
	}

	public static int GetHelperSkillLevel(helperRole helperConfig, int currentLevel)
	{
		if (currentLevel < helperConfig.UpgradeLevel[0])
		{
			return 0;
		}
		for (int num = helperConfig.UpgradeLevel.Count - 1; num >= 0; num--)
		{
			if (currentLevel >= helperConfig.UpgradeLevel[num])
			{
				return num + 1;
			}
		}
		return 0;
	}

	public static int GetHelperAwakeSkillLevel(helperRole helperConfig, int currentLevel)
	{
		if (currentLevel < helperConfig.AwakeSkillNum[0])
		{
			return 0;
		}
		for (int num = helperConfig.AwakeSkillNum.Count - 1; num >= 0; num--)
		{
			if (currentLevel >= helperConfig.AwakeSkillNum[num])
			{
				return num + 1;
			}
		}
		return 0;
	}

	public static string GetHelperSkillIconPath(int helperId, int level)
	{
		return helperId switch
		{
			1 => "Assets/GameRes/icon/helperskill/ty_icon_toughguy_1.png", 
			2 => "Assets/GameRes/icon/helperskill/ty_icon_miracle_1.png", 
			3 => $"Assets/GameRes/icon/helperskill/ty_icon_magician_{level}.png", 
			4 => $"Assets/GameRes/icon/helperskill/ty_icon_kungfu_{level}.png", 
			5 => $"Assets/GameRes/icon/helperskill/ty_icon_jikun_{level}.png", 
			6 => $"Assets/GameRes/icon/helperskill/ty_icon_dashe_{level}.png", 
			7 => $"Assets/GameRes/icon/helperskill/ty_icon_jiuwei_{level}.png", 
			8 => $"Assets/GameRes/icon/helperskill/ty_icon_gangtiern_{level}.png", 
			9 => $"Assets/GameRes/icon/helperskill/ty_icon_halande_{level}.png", 
			10 => $"Assets/GameRes/icon/helperskill/ty_icon_dushen_{level}.png", 
			_ => "Assets/GameRes/icon/helperskill/ty_icon_toughguy_1.png", 
		};
	}

	public static string GetUpgradeItemIcon(int itemId, string itemType = "itemUpgrade1")
	{
		return itemType switch
		{
			"itemUpgrade3" => itemId switch
			{
				101 => GetUpgradeItemIconPath("icon_eyunxiaochu"), 
				102 => GetUpgradeItemIconPath("icon_haoyunchangzai"), 
				103 => GetUpgradeItemIconPath("icon_xingyunhuixiangDice"), 
				104 => GetUpgradeItemIconPath("icon_gongmingDice"), 
				105 => GetUpgradeItemIconPath("icon_liudianjiazhu"), 
				106 => GetUpgradeItemIconPath("icon_chaoji666"), 
				107 => GetUpgradeItemIconPath("icon_zuanshishangren"), 
				_ => $"Assets/GameRes/NewUI/Icon/talent/talent_icon_{itemId}.png", 
			}, 
			"itemUpgrade1" => itemId switch
			{
				1 => GetUpgradeItemIconPath("icon_xiaobangshou"), 
				2 => GetUpgradeItemIconPath("icon_tongbi"), 
				3 => GetUpgradeItemIconPath("icon_yinbi"), 
				4 => GetUpgradeItemIconPath("icon_jinbi"), 
				5 => GetUpgradeItemIconPath("icon_zhaohuanzibi"), 
				101 => GetUpgradeItemIconPath("icon_xiaobangshou"), 
				102 => GetUpgradeItemIconPath("icon_copperDice"), 
				103 => GetUpgradeItemIconPath("icon_silverDice"), 
				104 => GetUpgradeItemIconPath("icon_goldDice"), 
				105 => GetUpgradeItemIconPath("icon_purpleDice"), 
				_ => "", 
			}, 
			"itemUpgrade2" => itemId switch
			{
				1 => GetUpgradeItemIconPath("icon_tongbishangzhang"), 
				2 => GetUpgradeItemIconPath("icon_yinbishangzhang"), 
				3 => GetUpgradeItemIconPath("icon_jinbishangzhang"), 
				4 => GetUpgradeItemIconPath("icon_biliqianghua1"), 
				5 => GetUpgradeItemIconPath("icon_biliqianghua2"), 
				6 => GetUpgradeItemIconPath("icon_kuaisushouge1"), 
				7 => GetUpgradeItemIconPath("icon_kuaisushouge2"), 
				8 => GetUpgradeItemIconPath("icon_kuaisushouge3"), 
				9 => GetUpgradeItemIconPath("icon_kuaisuyidong"), 
				10 => GetUpgradeItemIconPath("icon_liansuofanying"), 
				11 => GetUpgradeItemIconPath("icon_liansuofanying_02"), 
				12 => GetUpgradeItemIconPath("icon_xingyunhuixiang"), 
				13 => GetUpgradeItemIconPath("icon_zijinbi"), 
				14 => GetUpgradeItemIconPath("icon_zibishangzhang"), 
				101 => GetUpgradeItemIconPath("icon_tongtouzishangzhang"), 
				102 => GetUpgradeItemIconPath("icon_yintouzishangzhang"), 
				103 => GetUpgradeItemIconPath("icon_jintouzishangzhang"), 
				104 => GetUpgradeItemIconPath("icon_fanqiyintouzi"), 
				105 => GetUpgradeItemIconPath("icon_fanqijintouzi"), 
				106 => GetUpgradeItemIconPath("icon_huadongshouge1Dice"), 
				107 => GetUpgradeItemIconPath("icon_huadongshouge2Dice"), 
				108 => GetUpgradeItemIconPath("icon_huadongshouge3Dice"), 
				109 => GetUpgradeItemIconPath("icon_kuaisuyidong"), 
				110 => GetUpgradeItemIconPath("icon_zijintouzishangzhang"), 
				_ => "", 
			}, 
			_ => "", 
		};
	}

	private static string GetUpgradeItemIconPath(string iconName)
	{
		return "Assets/GameRes/icon/item/" + iconName + ".png";
	}

	public static int GetCurrentWorldHelperUpgradeId()
	{
		if ((Object)(object)GameDataManager.Instance != (Object)null && ConfigManager.HasInstance())
		{
			world worldConfig = ConfigManager.Instance.GetWorldConfig(GameDataManager.Instance.GetCurrentWorldId());
			if (worldConfig != null && worldConfig.HelperUpgradeId > 0)
			{
				return worldConfig.HelperUpgradeId;
			}
		}
		return 1;
	}
}

using System.Collections.Generic;
using UnityEngine;
using cfg;

namespace Module.Facility;

public static class FacilityRedDot
{
	public static void CheckAllRedDots()
	{
		bool flag = CheckAnyHelperUpgradeable();
		bool flag2 = CheckAnyDeskUpgradeable();
		if ((Object)(object)UIManager.Instance != (Object)null)
		{
			FacilityPanel facilityPanel = UIManager.Instance.GetFacilityPanel();
			if ((Object)(object)facilityPanel != (Object)null && ((Component)facilityPanel).gameObject.activeInHierarchy)
			{
				facilityPanel.RefreshTabRedDots(flag, flag2);
			}
		}
		if ((Object)(object)UpgradePanel.Instance != (Object)null)
		{
			UpgradePanel.Instance.RedDotsSet(1, flag || flag2);
		}
	}

	public static bool CheckAnyHelperUpgradeable()
	{
		if (!ConfigManager.HasInstance())
		{
			return false;
		}
		List<helperRole> allHelperRole = ConfigManager.Instance.GetAllHelperRole();
		if (allHelperRole == null)
		{
			return false;
		}
		foreach (helperRole item in allHelperRole)
		{
			if (IsHelperUpgradeable(item.Id))
			{
				return true;
			}
		}
		return false;
	}

	public static bool CheckAnyDeskUpgradeable()
	{
		if (!ConfigManager.HasInstance() || (Object)(object)GameDataManager.Instance == (Object)null)
		{
			return false;
		}
		int currentWorldId = GameDataManager.Instance.GetCurrentWorldId();
		List<desk> allDeskByWorld = ConfigManager.Instance.GetAllDeskByWorld(currentWorldId);
		if (allDeskByWorld == null)
		{
			return false;
		}
		foreach (desk item in allDeskByWorld)
		{
			if (IsDeskUpgradeable(item.Id))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsHelperUpgradeable(int helperId)
	{
		if ((Object)(object)GameDataManager.Instance == (Object)null)
		{
			return false;
		}
		if (!GameDataManager.Instance.IsHelperOwned(helperId))
		{
			return false;
		}
		helperRole helperRoleConfig = ConfigManager.Instance.GetHelperRoleConfig(helperId);
		if (helperRoleConfig == null)
		{
			return false;
		}
		int helperLevel = GameDataManager.Instance.GetHelperLevel(helperId);
		if (helperLevel >= helperRoleConfig.UpgradeLevelMax)
		{
			return false;
		}
		int helperSkillLevel = Constants.GetHelperSkillLevel(helperRoleConfig, helperLevel);
		int helperSkillLevel2 = Constants.GetHelperSkillLevel(helperRoleConfig, helperLevel + 1);
		long itemCount = GameDataManager.Instance.GetItemCount(helperRoleConfig.Id + 100);
		int num = 0;
		if (helperSkillLevel2 > helperSkillLevel)
		{
			num = helperRoleConfig.UpgradeCostType[helperSkillLevel];
		}
		if (itemCount < num)
		{
			return false;
		}
		double num2 = helperRoleConfig.UpgradeNum * (double)Mathf.Pow(helperRoleConfig.UpgradeNumCostBuff, (float)(helperLevel - 1));
		double totalGold = SingletonMono<UpgradeManager>.Instance.TotalGold;
		long diamond = GameDataManager.Instance.GetDiamond();
		if (helperRoleConfig.UpgradeType == 1)
		{
			return totalGold >= num2;
		}
		if (helperRoleConfig.UpgradeType == 2)
		{
			return (double)diamond >= num2;
		}
		if (helperRoleConfig.UpgradeType == 3)
		{
			return true;
		}
		return false;
	}

	public static bool IsDeskUpgradeable(int deskId)
	{
		if ((Object)(object)GameDataManager.Instance == (Object)null)
		{
			return false;
		}
		if (!GameDataManager.Instance.IsDeskOwned(deskId))
		{
			return false;
		}
		desk deskConfig = ConfigManager.Instance.GetDeskConfig(deskId);
		if (deskConfig == null)
		{
			return false;
		}
		if (deskConfig.DeskUnlockCondition != -1 && GameDataManager.Instance.GetCurrentMainMissionLevel() < deskConfig.DeskUnlockCondition)
		{
			return false;
		}
		int deskLevel = GameDataManager.Instance.GetDeskLevel(deskId);
		if (deskLevel >= deskConfig.SkillLevelMax)
		{
			return false;
		}
		double totalGold = SingletonMono<UpgradeManager>.Instance.TotalGold;
		long diamond = GameDataManager.Instance.GetDiamond();
		if (deskConfig.BuyType == 1)
		{
			double num = deskConfig.BuyNum * (double)Mathf.Pow(deskConfig.BuyNumCostBuff, (float)(deskLevel - 1));
			return totalGold >= num;
		}
		if (deskConfig.BuyType == 2)
		{
			double num2 = deskConfig.BuyNum + (double)(deskConfig.BuyNumCostBuff * (float)(deskLevel - 1));
			return (double)diamond >= num2;
		}
		return false;
	}
}

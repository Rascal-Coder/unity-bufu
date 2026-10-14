using System.Collections.Generic;

namespace Module.Vat;

public static class VatDebugSettings
{
	private static readonly HashSet<int> HiddenRoleSkillVisuals = new HashSet<int>();

	public static bool ShowCoins { get; private set; } = true;

	public static bool ShowRoles { get; private set; } = true;

	public static bool ShowSkillEffects { get; private set; } = true;

	public static bool ShowCoinSmoke { get; private set; } = true;

	public static bool ShowFloatingText { get; private set; } = true;

	public static bool ToggleCoins()
	{
		ShowCoins = !ShowCoins;
		return ShowCoins;
	}

	public static bool ToggleRoles()
	{
		ShowRoles = !ShowRoles;
		return ShowRoles;
	}

	public static bool ToggleSkillEffects()
	{
		ShowSkillEffects = !ShowSkillEffects;
		return ShowSkillEffects;
	}

	public static bool ToggleCoinSmoke()
	{
		ShowCoinSmoke = !ShowCoinSmoke;
		return ShowCoinSmoke;
	}

	public static bool ToggleFloatingText()
	{
		ShowFloatingText = !ShowFloatingText;
		return ShowFloatingText;
	}

	public static bool ToggleRoleSkillVisuals(int roleType)
	{
		if (!HiddenRoleSkillVisuals.Add(roleType))
		{
			HiddenRoleSkillVisuals.Remove(roleType);
			return true;
		}
		return false;
	}

	public static bool AreRoleSkillVisualsVisible(int roleType)
	{
		if (ShowSkillEffects)
		{
			return !HiddenRoleSkillVisuals.Contains(roleType);
		}
		return false;
	}
}

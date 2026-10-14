using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UpgradeData
{
	[SerializeField]
	public List<UpgradeLevelData> upgradeItemLevels = new List<UpgradeLevelData>();

	[SerializeField]
	public List<int> unlockedUpgradeItems = new List<int>();

	[SerializeField]
	public List<CurrencyEvolutionData> currencyEvolutionItems = new List<CurrencyEvolutionData>();

	[SerializeField]
	public int helperADLevel = 5;
}

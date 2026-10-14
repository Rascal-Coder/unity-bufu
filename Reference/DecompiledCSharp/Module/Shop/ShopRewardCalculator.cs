using System.Collections.Generic;
using UnityEngine;
using cfg;

namespace Module.Shop;

public static class ShopRewardCalculator
{
	private static Dictionary<shop, double> goldReward = new Dictionary<shop, double>();

	public static void AddGoldReward(shop config, double gold)
	{
		goldReward[config] = gold;
	}

	public static List<double> BuildRewardList(shop config)
	{
		List<double> list = new List<double>();
		if (config == null)
		{
			return list;
		}
		int num = ((config.RewardType != null) ? config.RewardType.Count : 0);
		for (int i = 0; i < num; i++)
		{
			int num2 = config.RewardType[i];
			int num3 = ((config.Count != null && i < config.Count.Count) ? config.Count[i] : 0);
			switch (num2)
			{
			case 1:
			{
				double item = 0.0;
				if (goldReward.ContainsKey(config))
				{
					item = goldReward[config];
				}
				else if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
				{
					item = SingletonMono<UpgradeManager>.Instance.GetAverageGoldPerMinuteFromLast30Seconds() * (double)num3;
				}
				list.Add(1.0);
				list.Add(item);
				break;
			}
			case 3:
			case 4:
			case 5:
			{
				Dictionary<int, int> dictionary = new Dictionary<int, int>();
				for (int j = 0; j < num3; j++)
				{
					int key = 0;
					switch (num2)
					{
					case 3:
						key = Random.Range(101, 103);
						break;
					case 4:
						key = Random.Range(103, 105);
						break;
					case 5:
						key = Random.Range(105, 111);
						break;
					}
					if (dictionary.ContainsKey(key))
					{
						dictionary[key]++;
					}
					else
					{
						dictionary[key] = 1;
					}
				}
				foreach (KeyValuePair<int, int> item2 in dictionary)
				{
					list.Add(item2.Key);
					list.Add(item2.Value);
				}
				break;
			}
			default:
				list.Add(num2);
				list.Add(num3);
				break;
			}
		}
		return list;
	}
}

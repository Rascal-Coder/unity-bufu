using System;
using UnityEngine;

namespace Module.Shop;

public static class ShopDailyLimit
{
	public static bool IsUnlimited { get; set; }

	private static string GetDayKey()
	{
		int[] array = (((Object)(object)GameDataManager.Instance != (Object)null) ? GameDataManager.Instance.GetTodayDateNumber() : null);
		if (array == null || array.Length < 2)
		{
			return DateTime.Now.ToString("yyyyMMdd");
		}
		return $"m{array[0]:00}d{array[1]:00}";
	}

	public static string GetKey(int id)
	{
		return $"shop_daily_{GetDayKey()}_{id}";
	}

	public static int GetCount(int id)
	{
		return PlayerPrefs.GetInt(GetKey(id), 0);
	}

	public static void SetCount(int id, int value)
	{
		PlayerPrefs.SetInt(GetKey(id), Mathf.Max(0, value));
	}

	public static bool TryConsume(int id, int limit, int amount = 1)
	{
		if (IsUnlimited)
		{
			return true;
		}
		if (amount <= 0)
		{
			return true;
		}
		if (limit <= 0)
		{
			SetCount(id, GetCount(id) + amount);
			return true;
		}
		int count = GetCount(id);
		if (count + amount > limit)
		{
			return false;
		}
		SetCount(id, count + amount);
		return true;
	}

	public static bool CanConsume(int id, int limit, int amount = 1)
	{
		if (IsUnlimited)
		{
			return true;
		}
		if (amount <= 0 || limit <= 0)
		{
			return true;
		}
		return GetCount(id) + amount <= limit;
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class BuffManager : SingletonMono<BuffManager>
{
	private Dictionary<BuffType, List<BuffData>> activeBuffs = new Dictionary<BuffType, List<BuffData>>();

	private bool _isCritical;

	public override bool DontDestory => true;

	private BuffManager()
	{
	}

	public void AddBuff(BuffType type, float value, float durationSeconds, string buffName = "")
	{
		if (!activeBuffs.ContainsKey(type))
		{
			activeBuffs[type] = new List<BuffData>();
		}
		float num = Time.time + durationSeconds;
		bool flag = false;
		foreach (BuffData item in activeBuffs[type])
		{
			bool num2 = !string.IsNullOrEmpty(buffName) && item.Name == buffName;
			bool flag2 = string.IsNullOrEmpty(buffName) && string.IsNullOrEmpty(item.Name) && Math.Abs(item.Value - value) < 1E-05f;
			if (num2)
			{
				item.Value = value;
				item.Duration = durationSeconds;
				item.EndTime = Math.Max(item.EndTime, num);
				item.StackCount = 1;
				flag = true;
				break;
			}
			if (flag2)
			{
				item.EndTime = Math.Max(item.EndTime, num);
				item.StackCount++;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			activeBuffs[type].Add(new BuffData
			{
				Type = type,
				Name = buffName,
				Value = value,
				Duration = durationSeconds,
				EndTime = num,
				StackCount = 1
			});
		}
	}

	public void RemoveBuff(BuffType type, float value)
	{
		if (activeBuffs.ContainsKey(type))
		{
			activeBuffs[type].RemoveAll((BuffData buff) => Math.Abs(buff.Value - value) < 0.001f);
		}
	}

	public void RemoveBuff(BuffType type, string buffName)
	{
		if (activeBuffs.ContainsKey(type))
		{
			activeBuffs[type].RemoveAll((BuffData buff) => buff.Name == buffName);
		}
	}

	public void ClearAllBuffs()
	{
		activeBuffs.Clear();
	}

	public void ClearBuffsByType(BuffType type)
	{
		if (activeBuffs.ContainsKey(type))
		{
			activeBuffs[type].Clear();
		}
	}

	private float GetBuffValue(BuffType type)
	{
		float num = 1f;
		if (!activeBuffs.ContainsKey(type))
		{
			return num;
		}
		foreach (BuffData item in activeBuffs[type])
		{
			float num2 = item.Value * (float)item.StackCount;
			if (type == item.Type)
			{
				num *= num2;
			}
		}
		return num;
	}

	private double GetFinalValue(BuffType type, double baseValue)
	{
		CleanExpiredBuffs(type);
		if (!activeBuffs.ContainsKey(type) || activeBuffs[type].Count == 0)
		{
			return baseValue;
		}
		double num = 1.0;
		double num2 = 0.0;
		double num3 = 0.0;
		foreach (BuffData item in activeBuffs[type])
		{
			double num4 = item.Value * (float)item.StackCount;
			if (type == BuffType.CriticalRate || type == BuffType.Income || type == BuffType.Speed)
			{
				num *= 1.0 + num4;
			}
			else
			{
				num3 += num4;
			}
		}
		if (type == BuffType.Income)
		{
			if (TalentDataManager.Instance != null)
			{
				switch (GameDataManager.Instance.GetCurrentWorldId())
				{
				case 1:
					num2 += (double)TalentDataManager.Instance.GetTalentExtraGold();
					break;
				case 2:
					num2 += (double)TalentDataManager.Instance.GetDiceTalentBuff(203);
					num2 += (double)TalentDataManager.Instance.GetDiceTalentBuff(205);
					num2 += (double)TalentDataManager.Instance.GetDiceTalentBuff(209);
					num2 += (double)TalentDataManager.Instance.GetDiceTalentBuff(210);
					num2 += (double)TalentDataManager.Instance.GetDiceTalentBuff(206);
					num2 += (double)TalentDataManager.Instance.GetDiceTalentBuff(208);
					break;
				}
			}
			if (TalentDataManager.Instance != null)
			{
				TalentDataManager talentDataManager = TalentDataManager.Instance;
				num2 = ((talentDataManager != null && !talentDataManager.IsCoinScene) ? (num2 + (double)TalentDataManager.Instance.GetDiceTalentBuff(101)) : (num2 + (double)TalentDataManager.Instance.LianJinBuff()));
			}
		}
		return (baseValue + num3) * (num2 + num);
	}

	public double GetFinalIncome(int coinType, double baseIncome, float extraBuff = 0f, float extraCriteRate = 0f, bool isManual = false)
	{
		float num = GetFinalCriticalRate() + extraCriteRate;
		bool num2 = Random.value <= num;
		double num3 = GetCoinIncome(coinType);
		double num4 = 1.0;
		int currentWorldId = GameDataManager.Instance.GetCurrentWorldId();
		TalentSceneType sceneType = ((currentWorldId != 1) ? TalentSceneType.DiceSceneTalent : TalentSceneType.CoinSceneTalent);
		float num5 = GameDataManager.Instance.GetRebornGain(sceneType);
		if (num5 <= 1f)
		{
			num5 = 1f;
		}
		if (isManual)
		{
			num4 = GetBuffValue(BuffType.Manual);
			if (TalentDataManager.Instance != null)
			{
				switch (currentWorldId)
				{
				case 1:
					num4 += (double)TalentDataManager.Instance.ManualBuff();
					break;
				case 2:
					num4 += (double)TalentDataManager.Instance.GetDiceTalentBuff(103);
					break;
				}
			}
		}
		if (num2)
		{
			SetIsCritical(isCritical: true);
			double num6 = Math.Max(1.0, GetFinalCriticalDamage(extraBuff));
			if (currentWorldId == 2)
			{
				num6 += (double)(TalentDataManager.Instance?.GetDiceTalentBuff(211) ?? 0f);
			}
			return GetFinalValue(BuffType.Income, baseIncome) * num6 * num3 * num4 * (double)num5;
		}
		SetIsCritical(isCritical: false);
		return GetFinalValue(BuffType.Income, baseIncome) * num3 * num4 * (double)num5;
	}

	public float GetCoinIncome(int coinType)
	{
		return coinType switch
		{
			1 => GetBuffValue(BuffType.Copper), 
			2 => GetBuffValue(BuffType.Silver), 
			3 => GetBuffValue(BuffType.Gold), 
			4 => GetBuffValue(BuffType.Violet), 
			_ => 1f, 
		};
	}

	public double GetFinalSpeed(double baseSpeed)
	{
		return GetFinalValue(BuffType.Speed, baseSpeed);
	}

	public double GetFinalDamage(double baseDamage)
	{
		return GetFinalValue(BuffType.Damage, baseDamage);
	}

	public double GetFinalDefense(double baseDefense)
	{
		return GetFinalValue(BuffType.Defense, baseDefense);
	}

	public double GetFinalCriticalDamage(double baseDamage)
	{
		return GetFinalValue(BuffType.CriticalDamage, baseDamage);
	}

	private float GetFinalCriticalRate()
	{
		float num = 0f;
		num = ConfigManager.Instance.GetCommonConfig(16).Factor;
		if ((Object)(object)ConfigManager.Instance != (Object)null)
		{
			switch (GameDataManager.Instance?.GetCurrentWorldId() ?? 1)
			{
			case 1:
				if (TalentDataManager.Instance != null && TalentDataManager.Instance.GetTalentCriticalRate() > 0f)
				{
					num += TalentDataManager.Instance.GetTalentCriticalRate();
				}
				break;
			case 2:
				num += TalentDataManager.Instance.GetDiceTalentBuff(302);
				break;
			}
		}
		return num;
	}

	private float GetFinalCriticalDamage()
	{
		float result = 0f;
		if ((Object)(object)ConfigManager.Instance != (Object)null)
		{
			result = ConfigManager.Instance.GetCommonConfig(17).Factor;
		}
		return result;
	}

	public bool GetIsCritical()
	{
		return _isCritical;
	}

	public void SetIsCritical(bool isCritical)
	{
		_isCritical = isCritical;
	}

	public void Update()
	{
		foreach (object value in Enum.GetValues(typeof(BuffType)))
		{
			CleanExpiredBuffs((BuffType)value);
		}
	}

	private void CleanExpiredBuffs(BuffType type)
	{
		if (activeBuffs.ContainsKey(type))
		{
			activeBuffs[type].RemoveAll((BuffData buff) => Time.time >= buff.EndTime);
		}
	}

	public List<BuffData> GetActiveBuffs(BuffType type)
	{
		CleanExpiredBuffs(type);
		if (!activeBuffs.ContainsKey(type))
		{
			return new List<BuffData>();
		}
		return new List<BuffData>(activeBuffs[type]);
	}

	public int GetBuffCount(BuffType type)
	{
		CleanExpiredBuffs(type);
		if (!activeBuffs.ContainsKey(type))
		{
			return 0;
		}
		return activeBuffs[type].Count;
	}

	protected override void OnAwake()
	{
	}

	protected override void BeforOnDestroy()
	{
	}
}

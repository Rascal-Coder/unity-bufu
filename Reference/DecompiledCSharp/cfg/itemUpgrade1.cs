using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public sealed class itemUpgrade1 : BeanBase
{
	public readonly int Id;

	public readonly int WorldId;

	public readonly string ItemName;

	public readonly string ItemDesc;

	public readonly List<int> BaseCostMoney;

	public readonly int CostType;

	public readonly List<int> CostMoneyLevel;

	public readonly List<float> CostMoneyBuff;

	public readonly float UpgradeResult;

	public readonly int UpgradeLvMax;

	public readonly int UnLockCondition;

	public readonly int ConditionIconType;

	public readonly List<int> EvolutionLevel;

	public readonly List<float> EvolutionBuff;

	public const int __ID__ = 2052048328;

	public itemUpgrade1(JSONNode _buf)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0350: Unknown result type (might be due to invalid IL or missing references)
		//IL_0394: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
		if (!_buf["id"].IsNumber)
		{
			throw new SerializationException();
		}
		Id = JSONNode.op_Implicit(_buf["id"]);
		if (!_buf["worldId"].IsNumber)
		{
			throw new SerializationException();
		}
		WorldId = JSONNode.op_Implicit(_buf["worldId"]);
		if (!_buf["itemName"].IsString)
		{
			throw new SerializationException();
		}
		ItemName = JSONNode.op_Implicit(_buf["itemName"]);
		if (!_buf["itemDesc"].IsString)
		{
			throw new SerializationException();
		}
		ItemDesc = JSONNode.op_Implicit(_buf["itemDesc"]);
		JSONNode val = _buf["baseCostMoney"];
		if (!val.IsArray)
		{
			throw new SerializationException();
		}
		BaseCostMoney = new List<int>(val.Count);
		foreach (JSONNode child in val.Children)
		{
			if (!child.IsNumber)
			{
				throw new SerializationException();
			}
			int num = JSONNode.op_Implicit(child);
			BaseCostMoney.Add(num);
		}
		if (!_buf["costType"].IsNumber)
		{
			throw new SerializationException();
		}
		CostType = JSONNode.op_Implicit(_buf["costType"]);
		JSONNode val2 = _buf["costMoneyLevel"];
		if (!val2.IsArray)
		{
			throw new SerializationException();
		}
		CostMoneyLevel = new List<int>(val2.Count);
		foreach (JSONNode child2 in val2.Children)
		{
			if (!child2.IsNumber)
			{
				throw new SerializationException();
			}
			int num2 = JSONNode.op_Implicit(child2);
			CostMoneyLevel.Add(num2);
		}
		JSONNode val3 = _buf["costMoneyBuff"];
		if (!val3.IsArray)
		{
			throw new SerializationException();
		}
		CostMoneyBuff = new List<float>(val3.Count);
		foreach (JSONNode child3 in val3.Children)
		{
			if (!child3.IsNumber)
			{
				throw new SerializationException();
			}
			float num3 = JSONNode.op_Implicit(child3);
			CostMoneyBuff.Add(num3);
		}
		if (!_buf["upgradeResult"].IsNumber)
		{
			throw new SerializationException();
		}
		UpgradeResult = JSONNode.op_Implicit(_buf["upgradeResult"]);
		if (!_buf["upgradeLvMax"].IsNumber)
		{
			throw new SerializationException();
		}
		UpgradeLvMax = JSONNode.op_Implicit(_buf["upgradeLvMax"]);
		if (!_buf["unLockCondition"].IsNumber)
		{
			throw new SerializationException();
		}
		UnLockCondition = JSONNode.op_Implicit(_buf["unLockCondition"]);
		if (!_buf["conditionIconType"].IsNumber)
		{
			throw new SerializationException();
		}
		ConditionIconType = JSONNode.op_Implicit(_buf["conditionIconType"]);
		JSONNode val4 = _buf["evolutionLevel"];
		if (!val4.IsArray)
		{
			throw new SerializationException();
		}
		EvolutionLevel = new List<int>(val4.Count);
		foreach (JSONNode child4 in val4.Children)
		{
			if (!child4.IsNumber)
			{
				throw new SerializationException();
			}
			int num4 = JSONNode.op_Implicit(child4);
			EvolutionLevel.Add(num4);
		}
		JSONNode val5 = _buf["evolutionBuff"];
		if (!val5.IsArray)
		{
			throw new SerializationException();
		}
		EvolutionBuff = new List<float>(val5.Count);
		foreach (JSONNode child5 in val5.Children)
		{
			if (!child5.IsNumber)
			{
				throw new SerializationException();
			}
			float num5 = JSONNode.op_Implicit(child5);
			EvolutionBuff.Add(num5);
		}
	}

	public static itemUpgrade1 DeserializeitemUpgrade1(JSONNode _buf)
	{
		return new itemUpgrade1(_buf);
	}

	public override int GetTypeId()
	{
		return 2052048328;
	}

	public void ResolveRef(Tables tables)
	{
	}

	public override string ToString()
	{
		return "{ id:" + Id + ",worldId:" + WorldId + ",itemName:" + ItemName + ",itemDesc:" + ItemDesc + ",baseCostMoney:" + StringUtil.CollectionToString<int>((IEnumerable<int>)BaseCostMoney) + ",costType:" + CostType + ",costMoneyLevel:" + StringUtil.CollectionToString<int>((IEnumerable<int>)CostMoneyLevel) + ",costMoneyBuff:" + StringUtil.CollectionToString<float>((IEnumerable<float>)CostMoneyBuff) + ",upgradeResult:" + UpgradeResult + ",upgradeLvMax:" + UpgradeLvMax + ",unLockCondition:" + UnLockCondition + ",conditionIconType:" + ConditionIconType + ",evolutionLevel:" + StringUtil.CollectionToString<int>((IEnumerable<int>)EvolutionLevel) + ",evolutionBuff:" + StringUtil.CollectionToString<float>((IEnumerable<float>)EvolutionBuff) + ",}";
	}
}

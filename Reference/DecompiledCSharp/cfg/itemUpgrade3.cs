using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public sealed class itemUpgrade3 : BeanBase
{
	public readonly int Id;

	public readonly int WorldId;

	public readonly string ItemName;

	public readonly string ItemDesc;

	public readonly string ItemDescUI;

	public readonly List<float> BaseCostMoney;

	public readonly int CostType;

	public readonly List<int> CostMoneyLevel;

	public readonly List<float> CostMoneyBuff;

	public readonly float UpgradeResult;

	public readonly int UpgradeLvMax;

	public readonly int UnLockCondition;

	public readonly int ConditionIconType;

	public readonly int Position;

	public const int __ID__ = 2052048330;

	public itemUpgrade3(JSONNode _buf)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Unknown result type (might be due to invalid IL or missing references)
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
		if (!_buf["itemDescUI"].IsString)
		{
			throw new SerializationException();
		}
		ItemDescUI = JSONNode.op_Implicit(_buf["itemDescUI"]);
		JSONNode val = _buf["baseCostMoney"];
		if (!val.IsArray)
		{
			throw new SerializationException();
		}
		BaseCostMoney = new List<float>(val.Count);
		foreach (JSONNode child in val.Children)
		{
			if (!child.IsNumber)
			{
				throw new SerializationException();
			}
			float num = JSONNode.op_Implicit(child);
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
		if (!_buf["position"].IsNumber)
		{
			throw new SerializationException();
		}
		Position = JSONNode.op_Implicit(_buf["position"]);
	}

	public static itemUpgrade3 DeserializeitemUpgrade3(JSONNode _buf)
	{
		return new itemUpgrade3(_buf);
	}

	public override int GetTypeId()
	{
		return 2052048330;
	}

	public void ResolveRef(Tables tables)
	{
	}

	public override string ToString()
	{
		return "{ id:" + Id + ",worldId:" + WorldId + ",itemName:" + ItemName + ",itemDesc:" + ItemDesc + ",itemDescUI:" + ItemDescUI + ",baseCostMoney:" + StringUtil.CollectionToString<float>((IEnumerable<float>)BaseCostMoney) + ",costType:" + CostType + ",costMoneyLevel:" + StringUtil.CollectionToString<int>((IEnumerable<int>)CostMoneyLevel) + ",costMoneyBuff:" + StringUtil.CollectionToString<float>((IEnumerable<float>)CostMoneyBuff) + ",upgradeResult:" + UpgradeResult + ",upgradeLvMax:" + UpgradeLvMax + ",unLockCondition:" + UnLockCondition + ",conditionIconType:" + ConditionIconType + ",position:" + Position + ",}";
	}
}

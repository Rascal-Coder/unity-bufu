using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public sealed class helperRole : BeanBase
{
	public readonly int Id;

	public readonly string HelperName;

	public readonly int Rare;

	public readonly string HelperDesc;

	public readonly float SkillFactor1;

	public readonly float CdTime;

	public readonly string AwakeSkill1;

	public readonly string AwakeSkill2;

	public readonly string AwakeSkill3;

	public readonly string AwakeSkill1Name;

	public readonly string AwakeSkill2Name;

	public readonly string AwakeSkill3Name;

	public readonly float BaseSkillNum;

	public readonly float UpgradeSkillNum;

	public readonly List<int> UpgradeLevel;

	public readonly int UpgradeLevelMax;

	public readonly float HelperIncomeAddBuff;

	public readonly List<int> AwakeSkillNum;

	public readonly int UpgradeType;

	public readonly double UpgradeNum;

	public readonly float UpgradeNumCostBuff;

	public readonly int BuyType;

	public readonly double BuyNum;

	public readonly double BuyNumDiamond;

	public readonly List<float> BuyNumCostBuff;

	public readonly int BuyNumLevel;

	public readonly int UpgradeNumLevel;

	public readonly List<int> UpgradeCostType;

	public readonly int HelperUnlockCondition;

	public readonly int HelperAdUnlockCondition;

	public readonly int UnlockLevel;

	public readonly int Sorting;

	public const int __ID__ = 1733655876;

	public helperRole(JSONNode _buf)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_040e: Unknown result type (might be due to invalid IL or missing references)
		//IL_043c: Unknown result type (might be due to invalid IL or missing references)
		//IL_046a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0498: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_052d: Unknown result type (might be due to invalid IL or missing references)
		//IL_056d: Unknown result type (might be due to invalid IL or missing references)
		//IL_059b: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0602: Unknown result type (might be due to invalid IL or missing references)
		//IL_0642: Unknown result type (might be due to invalid IL or missing references)
		//IL_0670: Unknown result type (might be due to invalid IL or missing references)
		//IL_069e: Unknown result type (might be due to invalid IL or missing references)
		//IL_06cc: Unknown result type (might be due to invalid IL or missing references)
		if (!_buf["id"].IsNumber)
		{
			throw new SerializationException();
		}
		Id = JSONNode.op_Implicit(_buf["id"]);
		if (!_buf["helperName"].IsString)
		{
			throw new SerializationException();
		}
		HelperName = JSONNode.op_Implicit(_buf["helperName"]);
		if (!_buf["Rare"].IsNumber)
		{
			throw new SerializationException();
		}
		Rare = JSONNode.op_Implicit(_buf["Rare"]);
		if (!_buf["helperDesc"].IsString)
		{
			throw new SerializationException();
		}
		HelperDesc = JSONNode.op_Implicit(_buf["helperDesc"]);
		if (!_buf["skillFactor1"].IsNumber)
		{
			throw new SerializationException();
		}
		SkillFactor1 = JSONNode.op_Implicit(_buf["skillFactor1"]);
		if (!_buf["cdTime"].IsNumber)
		{
			throw new SerializationException();
		}
		CdTime = JSONNode.op_Implicit(_buf["cdTime"]);
		if (!_buf["awakeSkill1"].IsString)
		{
			throw new SerializationException();
		}
		AwakeSkill1 = JSONNode.op_Implicit(_buf["awakeSkill1"]);
		if (!_buf["awakeSkill2"].IsString)
		{
			throw new SerializationException();
		}
		AwakeSkill2 = JSONNode.op_Implicit(_buf["awakeSkill2"]);
		if (!_buf["awakeSkill3"].IsString)
		{
			throw new SerializationException();
		}
		AwakeSkill3 = JSONNode.op_Implicit(_buf["awakeSkill3"]);
		if (!_buf["awakeSkill1Name"].IsString)
		{
			throw new SerializationException();
		}
		AwakeSkill1Name = JSONNode.op_Implicit(_buf["awakeSkill1Name"]);
		if (!_buf["awakeSkill2Name"].IsString)
		{
			throw new SerializationException();
		}
		AwakeSkill2Name = JSONNode.op_Implicit(_buf["awakeSkill2Name"]);
		if (!_buf["awakeSkill3Name"].IsString)
		{
			throw new SerializationException();
		}
		AwakeSkill3Name = JSONNode.op_Implicit(_buf["awakeSkill3Name"]);
		if (!_buf["baseSkillNum"].IsNumber)
		{
			throw new SerializationException();
		}
		BaseSkillNum = JSONNode.op_Implicit(_buf["baseSkillNum"]);
		if (!_buf["upgradeSkillNum"].IsNumber)
		{
			throw new SerializationException();
		}
		UpgradeSkillNum = JSONNode.op_Implicit(_buf["upgradeSkillNum"]);
		JSONNode val = _buf["upgradeLevel"];
		if (!val.IsArray)
		{
			throw new SerializationException();
		}
		UpgradeLevel = new List<int>(val.Count);
		foreach (JSONNode child in val.Children)
		{
			if (!child.IsNumber)
			{
				throw new SerializationException();
			}
			int num = JSONNode.op_Implicit(child);
			UpgradeLevel.Add(num);
		}
		if (!_buf["upgradeLevelMax"].IsNumber)
		{
			throw new SerializationException();
		}
		UpgradeLevelMax = JSONNode.op_Implicit(_buf["upgradeLevelMax"]);
		if (!_buf["helperIncomeAddBuff"].IsNumber)
		{
			throw new SerializationException();
		}
		HelperIncomeAddBuff = JSONNode.op_Implicit(_buf["helperIncomeAddBuff"]);
		JSONNode val2 = _buf["awakeSkillNum"];
		if (!val2.IsArray)
		{
			throw new SerializationException();
		}
		AwakeSkillNum = new List<int>(val2.Count);
		foreach (JSONNode child2 in val2.Children)
		{
			if (!child2.IsNumber)
			{
				throw new SerializationException();
			}
			int num2 = JSONNode.op_Implicit(child2);
			AwakeSkillNum.Add(num2);
		}
		if (!_buf["UpgradeType"].IsNumber)
		{
			throw new SerializationException();
		}
		UpgradeType = JSONNode.op_Implicit(_buf["UpgradeType"]);
		if (!_buf["UpgradeNum"].IsNumber)
		{
			throw new SerializationException();
		}
		UpgradeNum = JSONNode.op_Implicit(_buf["UpgradeNum"]);
		if (!_buf["UpgradeNumCostBuff"].IsNumber)
		{
			throw new SerializationException();
		}
		UpgradeNumCostBuff = JSONNode.op_Implicit(_buf["UpgradeNumCostBuff"]);
		if (!_buf["buyType"].IsNumber)
		{
			throw new SerializationException();
		}
		BuyType = JSONNode.op_Implicit(_buf["buyType"]);
		if (!_buf["buyNum"].IsNumber)
		{
			throw new SerializationException();
		}
		BuyNum = JSONNode.op_Implicit(_buf["buyNum"]);
		if (!_buf["buyNumDiamond"].IsNumber)
		{
			throw new SerializationException();
		}
		BuyNumDiamond = JSONNode.op_Implicit(_buf["buyNumDiamond"]);
		JSONNode val3 = _buf["buyNumCostBuff"];
		if (!val3.IsArray)
		{
			throw new SerializationException();
		}
		BuyNumCostBuff = new List<float>(val3.Count);
		foreach (JSONNode child3 in val3.Children)
		{
			if (!child3.IsNumber)
			{
				throw new SerializationException();
			}
			float num3 = JSONNode.op_Implicit(child3);
			BuyNumCostBuff.Add(num3);
		}
		if (!_buf["buyNumLevel"].IsNumber)
		{
			throw new SerializationException();
		}
		BuyNumLevel = JSONNode.op_Implicit(_buf["buyNumLevel"]);
		if (!_buf["UpgradeNumLevel"].IsNumber)
		{
			throw new SerializationException();
		}
		UpgradeNumLevel = JSONNode.op_Implicit(_buf["UpgradeNumLevel"]);
		JSONNode val4 = _buf["UpgradeCostType"];
		if (!val4.IsArray)
		{
			throw new SerializationException();
		}
		UpgradeCostType = new List<int>(val4.Count);
		foreach (JSONNode child4 in val4.Children)
		{
			if (!child4.IsNumber)
			{
				throw new SerializationException();
			}
			int num4 = JSONNode.op_Implicit(child4);
			UpgradeCostType.Add(num4);
		}
		if (!_buf["helperUnlockCondition"].IsNumber)
		{
			throw new SerializationException();
		}
		HelperUnlockCondition = JSONNode.op_Implicit(_buf["helperUnlockCondition"]);
		if (!_buf["helperAdUnlockCondition"].IsNumber)
		{
			throw new SerializationException();
		}
		HelperAdUnlockCondition = JSONNode.op_Implicit(_buf["helperAdUnlockCondition"]);
		if (!_buf["UnlockLevel"].IsNumber)
		{
			throw new SerializationException();
		}
		UnlockLevel = JSONNode.op_Implicit(_buf["UnlockLevel"]);
		if (!_buf["sorting"].IsNumber)
		{
			throw new SerializationException();
		}
		Sorting = JSONNode.op_Implicit(_buf["sorting"]);
	}

	public static helperRole DeserializehelperRole(JSONNode _buf)
	{
		return new helperRole(_buf);
	}

	public override int GetTypeId()
	{
		return 1733655876;
	}

	public void ResolveRef(Tables tables)
	{
	}

	public override string ToString()
	{
		return "{ id:" + Id + ",helperName:" + HelperName + ",Rare:" + Rare + ",helperDesc:" + HelperDesc + ",skillFactor1:" + SkillFactor1 + ",cdTime:" + CdTime + ",awakeSkill1:" + AwakeSkill1 + ",awakeSkill2:" + AwakeSkill2 + ",awakeSkill3:" + AwakeSkill3 + ",awakeSkill1Name:" + AwakeSkill1Name + ",awakeSkill2Name:" + AwakeSkill2Name + ",awakeSkill3Name:" + AwakeSkill3Name + ",baseSkillNum:" + BaseSkillNum + ",upgradeSkillNum:" + UpgradeSkillNum + ",upgradeLevel:" + StringUtil.CollectionToString<int>((IEnumerable<int>)UpgradeLevel) + ",upgradeLevelMax:" + UpgradeLevelMax + ",helperIncomeAddBuff:" + HelperIncomeAddBuff + ",awakeSkillNum:" + StringUtil.CollectionToString<int>((IEnumerable<int>)AwakeSkillNum) + ",UpgradeType:" + UpgradeType + ",UpgradeNum:" + UpgradeNum + ",UpgradeNumCostBuff:" + UpgradeNumCostBuff + ",buyType:" + BuyType + ",buyNum:" + BuyNum + ",buyNumDiamond:" + BuyNumDiamond + ",buyNumCostBuff:" + StringUtil.CollectionToString<float>((IEnumerable<float>)BuyNumCostBuff) + ",buyNumLevel:" + BuyNumLevel + ",UpgradeNumLevel:" + UpgradeNumLevel + ",UpgradeCostType:" + StringUtil.CollectionToString<int>((IEnumerable<int>)UpgradeCostType) + ",helperUnlockCondition:" + HelperUnlockCondition + ",helperAdUnlockCondition:" + HelperAdUnlockCondition + ",UnlockLevel:" + UnlockLevel + ",sorting:" + Sorting + ",}";
	}
}

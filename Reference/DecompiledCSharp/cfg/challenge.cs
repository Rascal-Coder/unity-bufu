using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public sealed class challenge : BeanBase
{
	public readonly int Id;

	public readonly double CostMoney;

	public readonly double LimitMissionCondition;

	public readonly int LimitTIme;

	public readonly float LimitGoalFactor;

	public readonly int HelperNum;

	public readonly int CoinType1;

	public readonly int CoinNum1;

	public readonly int CoinType2;

	public readonly int CoinNum2;

	public readonly List<int> BaseCostMoney1;

	public readonly List<int> BaseCostMoney2;

	public readonly double CoinBaseValue1;

	public readonly double CoinBaseValue2;

	public readonly float CoinBuff1;

	public readonly float CoinBuff2;

	public readonly List<int> CostMoneyLevel1;

	public readonly List<float> CostMoneyBuff1;

	public readonly List<int> CostMoneyLevel2;

	public readonly List<float> CostMoneyBuff2;

	public readonly int RewardType;

	public readonly double RewardNum;

	public const int __ID__ = 1402633315;

	public challenge(JSONNode _buf)
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
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_0356: Unknown result type (might be due to invalid IL or missing references)
		//IL_0388: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0401: Unknown result type (might be due to invalid IL or missing references)
		//IL_0436: Unknown result type (might be due to invalid IL or missing references)
		//IL_047a: Unknown result type (might be due to invalid IL or missing references)
		//IL_04af: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0528: Unknown result type (might be due to invalid IL or missing references)
		//IL_0568: Unknown result type (might be due to invalid IL or missing references)
		//IL_0596: Unknown result type (might be due to invalid IL or missing references)
		if (!_buf["id"].IsNumber)
		{
			throw new SerializationException();
		}
		Id = JSONNode.op_Implicit(_buf["id"]);
		if (!_buf["costMoney"].IsNumber)
		{
			throw new SerializationException();
		}
		CostMoney = JSONNode.op_Implicit(_buf["costMoney"]);
		if (!_buf["limitMissionCondition"].IsNumber)
		{
			throw new SerializationException();
		}
		LimitMissionCondition = JSONNode.op_Implicit(_buf["limitMissionCondition"]);
		if (!_buf["limitTIme"].IsNumber)
		{
			throw new SerializationException();
		}
		LimitTIme = JSONNode.op_Implicit(_buf["limitTIme"]);
		if (!_buf["limitGoalFactor"].IsNumber)
		{
			throw new SerializationException();
		}
		LimitGoalFactor = JSONNode.op_Implicit(_buf["limitGoalFactor"]);
		if (!_buf["helperNum"].IsNumber)
		{
			throw new SerializationException();
		}
		HelperNum = JSONNode.op_Implicit(_buf["helperNum"]);
		if (!_buf["coinType1"].IsNumber)
		{
			throw new SerializationException();
		}
		CoinType1 = JSONNode.op_Implicit(_buf["coinType1"]);
		if (!_buf["coinNum1"].IsNumber)
		{
			throw new SerializationException();
		}
		CoinNum1 = JSONNode.op_Implicit(_buf["coinNum1"]);
		if (!_buf["coinType2"].IsNumber)
		{
			throw new SerializationException();
		}
		CoinType2 = JSONNode.op_Implicit(_buf["coinType2"]);
		if (!_buf["coinNum2"].IsNumber)
		{
			throw new SerializationException();
		}
		CoinNum2 = JSONNode.op_Implicit(_buf["coinNum2"]);
		JSONNode val = _buf["baseCostMoney1"];
		if (!val.IsArray)
		{
			throw new SerializationException();
		}
		BaseCostMoney1 = new List<int>(val.Count);
		foreach (JSONNode child in val.Children)
		{
			if (!child.IsNumber)
			{
				throw new SerializationException();
			}
			int num = JSONNode.op_Implicit(child);
			BaseCostMoney1.Add(num);
		}
		JSONNode val2 = _buf["baseCostMoney2"];
		if (!val2.IsArray)
		{
			throw new SerializationException();
		}
		BaseCostMoney2 = new List<int>(val2.Count);
		foreach (JSONNode child2 in val2.Children)
		{
			if (!child2.IsNumber)
			{
				throw new SerializationException();
			}
			int num2 = JSONNode.op_Implicit(child2);
			BaseCostMoney2.Add(num2);
		}
		if (!_buf["coinBaseValue1"].IsNumber)
		{
			throw new SerializationException();
		}
		CoinBaseValue1 = JSONNode.op_Implicit(_buf["coinBaseValue1"]);
		if (!_buf["coinBaseValue2"].IsNumber)
		{
			throw new SerializationException();
		}
		CoinBaseValue2 = JSONNode.op_Implicit(_buf["coinBaseValue2"]);
		if (!_buf["coinBuff1"].IsNumber)
		{
			throw new SerializationException();
		}
		CoinBuff1 = JSONNode.op_Implicit(_buf["coinBuff1"]);
		if (!_buf["coinBuff2"].IsNumber)
		{
			throw new SerializationException();
		}
		CoinBuff2 = JSONNode.op_Implicit(_buf["coinBuff2"]);
		JSONNode val3 = _buf["costMoneyLevel1"];
		if (!val3.IsArray)
		{
			throw new SerializationException();
		}
		CostMoneyLevel1 = new List<int>(val3.Count);
		foreach (JSONNode child3 in val3.Children)
		{
			if (!child3.IsNumber)
			{
				throw new SerializationException();
			}
			int num3 = JSONNode.op_Implicit(child3);
			CostMoneyLevel1.Add(num3);
		}
		JSONNode val4 = _buf["costMoneyBuff1"];
		if (!val4.IsArray)
		{
			throw new SerializationException();
		}
		CostMoneyBuff1 = new List<float>(val4.Count);
		foreach (JSONNode child4 in val4.Children)
		{
			if (!child4.IsNumber)
			{
				throw new SerializationException();
			}
			float num4 = JSONNode.op_Implicit(child4);
			CostMoneyBuff1.Add(num4);
		}
		JSONNode val5 = _buf["costMoneyLevel2"];
		if (!val5.IsArray)
		{
			throw new SerializationException();
		}
		CostMoneyLevel2 = new List<int>(val5.Count);
		foreach (JSONNode child5 in val5.Children)
		{
			if (!child5.IsNumber)
			{
				throw new SerializationException();
			}
			int num5 = JSONNode.op_Implicit(child5);
			CostMoneyLevel2.Add(num5);
		}
		JSONNode val6 = _buf["costMoneyBuff2"];
		if (!val6.IsArray)
		{
			throw new SerializationException();
		}
		CostMoneyBuff2 = new List<float>(val6.Count);
		foreach (JSONNode child6 in val6.Children)
		{
			if (!child6.IsNumber)
			{
				throw new SerializationException();
			}
			float num6 = JSONNode.op_Implicit(child6);
			CostMoneyBuff2.Add(num6);
		}
		if (!_buf["rewardType"].IsNumber)
		{
			throw new SerializationException();
		}
		RewardType = JSONNode.op_Implicit(_buf["rewardType"]);
		if (!_buf["rewardNum"].IsNumber)
		{
			throw new SerializationException();
		}
		RewardNum = JSONNode.op_Implicit(_buf["rewardNum"]);
	}

	public static challenge Deserializechallenge(JSONNode _buf)
	{
		return new challenge(_buf);
	}

	public override int GetTypeId()
	{
		return 1402633315;
	}

	public void ResolveRef(Tables tables)
	{
	}

	public override string ToString()
	{
		return "{ id:" + Id + ",costMoney:" + CostMoney + ",limitMissionCondition:" + LimitMissionCondition + ",limitTIme:" + LimitTIme + ",limitGoalFactor:" + LimitGoalFactor + ",helperNum:" + HelperNum + ",coinType1:" + CoinType1 + ",coinNum1:" + CoinNum1 + ",coinType2:" + CoinType2 + ",coinNum2:" + CoinNum2 + ",baseCostMoney1:" + StringUtil.CollectionToString<int>((IEnumerable<int>)BaseCostMoney1) + ",baseCostMoney2:" + StringUtil.CollectionToString<int>((IEnumerable<int>)BaseCostMoney2) + ",coinBaseValue1:" + CoinBaseValue1 + ",coinBaseValue2:" + CoinBaseValue2 + ",coinBuff1:" + CoinBuff1 + ",coinBuff2:" + CoinBuff2 + ",costMoneyLevel1:" + StringUtil.CollectionToString<int>((IEnumerable<int>)CostMoneyLevel1) + ",costMoneyBuff1:" + StringUtil.CollectionToString<float>((IEnumerable<float>)CostMoneyBuff1) + ",costMoneyLevel2:" + StringUtil.CollectionToString<int>((IEnumerable<int>)CostMoneyLevel2) + ",costMoneyBuff2:" + StringUtil.CollectionToString<float>((IEnumerable<float>)CostMoneyBuff2) + ",rewardType:" + RewardType + ",rewardNum:" + RewardNum + ",}";
	}
}

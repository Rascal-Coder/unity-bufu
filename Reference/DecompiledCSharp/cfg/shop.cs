using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public sealed class shop : BeanBase
{
	public readonly int Id;

	public readonly string Name;

	public readonly string Desc;

	public readonly List<int> RewardType;

	public readonly List<int> Count;

	public readonly float Rate;

	public readonly int Cost;

	public readonly int TodayLimit;

	public const int __ID__ = 3529462;

	public shop(JSONNode _buf)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		if (!_buf["id"].IsNumber)
		{
			throw new SerializationException();
		}
		Id = JSONNode.op_Implicit(_buf["id"]);
		if (!_buf["name"].IsString)
		{
			throw new SerializationException();
		}
		Name = JSONNode.op_Implicit(_buf["name"]);
		if (!_buf["desc"].IsString)
		{
			throw new SerializationException();
		}
		Desc = JSONNode.op_Implicit(_buf["desc"]);
		JSONNode val = _buf["rewardType"];
		if (!val.IsArray)
		{
			throw new SerializationException();
		}
		RewardType = new List<int>(val.Count);
		foreach (JSONNode child in val.Children)
		{
			if (!child.IsNumber)
			{
				throw new SerializationException();
			}
			int num = JSONNode.op_Implicit(child);
			RewardType.Add(num);
		}
		JSONNode val2 = _buf["count"];
		if (!val2.IsArray)
		{
			throw new SerializationException();
		}
		Count = new List<int>(val2.Count);
		foreach (JSONNode child2 in val2.Children)
		{
			if (!child2.IsNumber)
			{
				throw new SerializationException();
			}
			int num2 = JSONNode.op_Implicit(child2);
			Count.Add(num2);
		}
		if (!_buf["rate"].IsNumber)
		{
			throw new SerializationException();
		}
		Rate = JSONNode.op_Implicit(_buf["rate"]);
		if (!_buf["cost"].IsNumber)
		{
			throw new SerializationException();
		}
		Cost = JSONNode.op_Implicit(_buf["cost"]);
		if (!_buf["todayLimit"].IsNumber)
		{
			throw new SerializationException();
		}
		TodayLimit = JSONNode.op_Implicit(_buf["todayLimit"]);
	}

	public static shop Deserializeshop(JSONNode _buf)
	{
		return new shop(_buf);
	}

	public override int GetTypeId()
	{
		return 3529462;
	}

	public void ResolveRef(Tables tables)
	{
	}

	public override string ToString()
	{
		return "{ id:" + Id + ",name:" + Name + ",desc:" + Desc + ",rewardType:" + StringUtil.CollectionToString<int>((IEnumerable<int>)RewardType) + ",count:" + StringUtil.CollectionToString<int>((IEnumerable<int>)Count) + ",rate:" + Rate + ",cost:" + Cost + ",todayLimit:" + TodayLimit + ",}";
	}
}

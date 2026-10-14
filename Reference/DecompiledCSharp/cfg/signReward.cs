using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public sealed class signReward : BeanBase
{
	public readonly int Id;

	public readonly int WorldId;

	public readonly List<int> NormalReward;

	public readonly List<int> AdReward;

	public const int __ID__ = -1521871028;

	public signReward(JSONNode _buf)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
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
		JSONNode val = _buf["normalReward"];
		if (!val.IsArray)
		{
			throw new SerializationException();
		}
		NormalReward = new List<int>(val.Count);
		foreach (JSONNode child in val.Children)
		{
			if (!child.IsNumber)
			{
				throw new SerializationException();
			}
			int num = JSONNode.op_Implicit(child);
			NormalReward.Add(num);
		}
		JSONNode val2 = _buf["AdReward"];
		if (!val2.IsArray)
		{
			throw new SerializationException();
		}
		AdReward = new List<int>(val2.Count);
		foreach (JSONNode child2 in val2.Children)
		{
			if (!child2.IsNumber)
			{
				throw new SerializationException();
			}
			int num2 = JSONNode.op_Implicit(child2);
			AdReward.Add(num2);
		}
	}

	public static signReward DeserializesignReward(JSONNode _buf)
	{
		return new signReward(_buf);
	}

	public override int GetTypeId()
	{
		return -1521871028;
	}

	public void ResolveRef(Tables tables)
	{
	}

	public override string ToString()
	{
		return "{ id:" + Id + ",worldId:" + WorldId + ",normalReward:" + StringUtil.CollectionToString<int>((IEnumerable<int>)NormalReward) + ",AdReward:" + StringUtil.CollectionToString<int>((IEnumerable<int>)AdReward) + ",}";
	}
}

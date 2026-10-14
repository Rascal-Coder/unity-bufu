using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public sealed class auctionItem : BeanBase
{
	public readonly int Id;

	public readonly string ItemName;

	public readonly List<int> Rarity;

	public readonly int SpecialItem;

	public const int __ID__ = -289344522;

	public auctionItem(JSONNode _buf)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		if (!_buf["id"].IsNumber)
		{
			throw new SerializationException();
		}
		Id = JSONNode.op_Implicit(_buf["id"]);
		if (!_buf["itemName"].IsString)
		{
			throw new SerializationException();
		}
		ItemName = JSONNode.op_Implicit(_buf["itemName"]);
		JSONNode val = _buf["rarity"];
		if (!val.IsArray)
		{
			throw new SerializationException();
		}
		Rarity = new List<int>(val.Count);
		foreach (JSONNode child in val.Children)
		{
			if (!child.IsNumber)
			{
				throw new SerializationException();
			}
			int num = JSONNode.op_Implicit(child);
			Rarity.Add(num);
		}
		if (!_buf["specialItem"].IsNumber)
		{
			throw new SerializationException();
		}
		SpecialItem = JSONNode.op_Implicit(_buf["specialItem"]);
	}

	public static auctionItem DeserializeauctionItem(JSONNode _buf)
	{
		return new auctionItem(_buf);
	}

	public override int GetTypeId()
	{
		return -289344522;
	}

	public void ResolveRef(Tables tables)
	{
	}

	public override string ToString()
	{
		return "{ id:" + Id + ",itemName:" + ItemName + ",rarity:" + StringUtil.CollectionToString<int>((IEnumerable<int>)Rarity) + ",specialItem:" + SpecialItem + ",}";
	}
}

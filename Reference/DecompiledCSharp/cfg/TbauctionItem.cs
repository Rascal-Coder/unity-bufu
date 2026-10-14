using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public class TbauctionItem
{
	private readonly Dictionary<int, auctionItem> _dataMap;

	private readonly List<auctionItem> _dataList;

	public Dictionary<int, auctionItem> DataMap => _dataMap;

	public List<auctionItem> DataList => _dataList;

	public auctionItem this[int key] => _dataMap[key];

	public TbauctionItem(JSONNode _buf)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		int count = _buf.Count;
		_dataMap = new Dictionary<int, auctionItem>(count);
		_dataList = new List<auctionItem>(count);
		foreach (JSONNode child in _buf.Children)
		{
			if (!child.IsObject)
			{
				throw new SerializationException();
			}
			auctionItem auctionItem2 = auctionItem.DeserializeauctionItem(child);
			_dataList.Add(auctionItem2);
			_dataMap.Add(auctionItem2.Id, auctionItem2);
		}
	}

	public auctionItem GetOrDefault(int key)
	{
		if (!_dataMap.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	public auctionItem Get(int key)
	{
		return _dataMap[key];
	}

	public void ResolveRef(Tables tables)
	{
		foreach (auctionItem data in _dataList)
		{
			data.ResolveRef(tables);
		}
	}
}

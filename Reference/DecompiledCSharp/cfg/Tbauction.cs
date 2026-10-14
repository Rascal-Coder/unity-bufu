using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public class Tbauction
{
	private readonly Dictionary<int, auction> _dataMap;

	private readonly List<auction> _dataList;

	public Dictionary<int, auction> DataMap => _dataMap;

	public List<auction> DataList => _dataList;

	public auction this[int key] => _dataMap[key];

	public Tbauction(JSONNode _buf)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		int count = _buf.Count;
		_dataMap = new Dictionary<int, auction>(count);
		_dataList = new List<auction>(count);
		foreach (JSONNode child in _buf.Children)
		{
			if (!child.IsObject)
			{
				throw new SerializationException();
			}
			auction auction2 = auction.Deserializeauction(child);
			_dataList.Add(auction2);
			_dataMap.Add(auction2.AuctionHouseID, auction2);
		}
	}

	public auction GetOrDefault(int key)
	{
		if (!_dataMap.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	public auction Get(int key)
	{
		return _dataMap[key];
	}

	public void ResolveRef(Tables tables)
	{
		foreach (auction data in _dataList)
		{
			data.ResolveRef(tables);
		}
	}
}

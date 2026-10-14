using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public class Tbshop
{
	private readonly Dictionary<int, shop> _dataMap;

	private readonly List<shop> _dataList;

	public Dictionary<int, shop> DataMap => _dataMap;

	public List<shop> DataList => _dataList;

	public shop this[int key] => _dataMap[key];

	public Tbshop(JSONNode _buf)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		int count = _buf.Count;
		_dataMap = new Dictionary<int, shop>(count);
		_dataList = new List<shop>(count);
		foreach (JSONNode child in _buf.Children)
		{
			if (!child.IsObject)
			{
				throw new SerializationException();
			}
			shop shop2 = shop.Deserializeshop(child);
			_dataList.Add(shop2);
			_dataMap.Add(shop2.Id, shop2);
		}
	}

	public shop GetOrDefault(int key)
	{
		if (!_dataMap.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	public shop Get(int key)
	{
		return _dataMap[key];
	}

	public void ResolveRef(Tables tables)
	{
		foreach (shop data in _dataList)
		{
			data.ResolveRef(tables);
		}
	}
}

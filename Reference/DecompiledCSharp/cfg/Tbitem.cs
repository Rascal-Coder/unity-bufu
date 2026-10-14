using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public class Tbitem
{
	private readonly Dictionary<int, item> _dataMap;

	private readonly List<item> _dataList;

	public Dictionary<int, item> DataMap => _dataMap;

	public List<item> DataList => _dataList;

	public item this[int key] => _dataMap[key];

	public Tbitem(JSONNode _buf)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		int count = _buf.Count;
		_dataMap = new Dictionary<int, item>(count);
		_dataList = new List<item>(count);
		foreach (JSONNode child in _buf.Children)
		{
			if (!child.IsObject)
			{
				throw new SerializationException();
			}
			item item2 = item.Deserializeitem(child);
			_dataList.Add(item2);
			_dataMap.Add(item2.Id, item2);
		}
	}

	public item GetOrDefault(int key)
	{
		if (!_dataMap.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	public item Get(int key)
	{
		return _dataMap[key];
	}

	public void ResolveRef(Tables tables)
	{
		foreach (item data in _dataList)
		{
			data.ResolveRef(tables);
		}
	}
}

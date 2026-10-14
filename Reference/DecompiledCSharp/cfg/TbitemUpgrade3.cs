using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public class TbitemUpgrade3
{
	private readonly Dictionary<int, itemUpgrade3> _dataMap;

	private readonly List<itemUpgrade3> _dataList;

	public Dictionary<int, itemUpgrade3> DataMap => _dataMap;

	public List<itemUpgrade3> DataList => _dataList;

	public itemUpgrade3 this[int key] => _dataMap[key];

	public TbitemUpgrade3(JSONNode _buf)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		int count = _buf.Count;
		_dataMap = new Dictionary<int, itemUpgrade3>(count);
		_dataList = new List<itemUpgrade3>(count);
		foreach (JSONNode child in _buf.Children)
		{
			if (!child.IsObject)
			{
				throw new SerializationException();
			}
			itemUpgrade3 itemUpgrade4 = itemUpgrade3.DeserializeitemUpgrade3(child);
			_dataList.Add(itemUpgrade4);
			_dataMap.Add(itemUpgrade4.Id, itemUpgrade4);
		}
	}

	public itemUpgrade3 GetOrDefault(int key)
	{
		if (!_dataMap.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	public itemUpgrade3 Get(int key)
	{
		return _dataMap[key];
	}

	public void ResolveRef(Tables tables)
	{
		foreach (itemUpgrade3 data in _dataList)
		{
			data.ResolveRef(tables);
		}
	}
}

using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public class TbitemUpgrade2
{
	private readonly Dictionary<int, itemUpgrade2> _dataMap;

	private readonly List<itemUpgrade2> _dataList;

	public Dictionary<int, itemUpgrade2> DataMap => _dataMap;

	public List<itemUpgrade2> DataList => _dataList;

	public itemUpgrade2 this[int key] => _dataMap[key];

	public TbitemUpgrade2(JSONNode _buf)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		int count = _buf.Count;
		_dataMap = new Dictionary<int, itemUpgrade2>(count);
		_dataList = new List<itemUpgrade2>(count);
		foreach (JSONNode child in _buf.Children)
		{
			if (!child.IsObject)
			{
				throw new SerializationException();
			}
			itemUpgrade2 itemUpgrade4 = itemUpgrade2.DeserializeitemUpgrade2(child);
			_dataList.Add(itemUpgrade4);
			_dataMap.Add(itemUpgrade4.Id, itemUpgrade4);
		}
	}

	public itemUpgrade2 GetOrDefault(int key)
	{
		if (!_dataMap.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	public itemUpgrade2 Get(int key)
	{
		return _dataMap[key];
	}

	public void ResolveRef(Tables tables)
	{
		foreach (itemUpgrade2 data in _dataList)
		{
			data.ResolveRef(tables);
		}
	}
}

using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public class TbitemUpgrade1
{
	private readonly Dictionary<int, itemUpgrade1> _dataMap;

	private readonly List<itemUpgrade1> _dataList;

	public Dictionary<int, itemUpgrade1> DataMap => _dataMap;

	public List<itemUpgrade1> DataList => _dataList;

	public itemUpgrade1 this[int key] => _dataMap[key];

	public TbitemUpgrade1(JSONNode _buf)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		int count = _buf.Count;
		_dataMap = new Dictionary<int, itemUpgrade1>(count);
		_dataList = new List<itemUpgrade1>(count);
		foreach (JSONNode child in _buf.Children)
		{
			if (!child.IsObject)
			{
				throw new SerializationException();
			}
			itemUpgrade1 itemUpgrade4 = itemUpgrade1.DeserializeitemUpgrade1(child);
			_dataList.Add(itemUpgrade4);
			_dataMap.Add(itemUpgrade4.Id, itemUpgrade4);
		}
	}

	public itemUpgrade1 GetOrDefault(int key)
	{
		if (!_dataMap.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	public itemUpgrade1 Get(int key)
	{
		return _dataMap[key];
	}

	public void ResolveRef(Tables tables)
	{
		foreach (itemUpgrade1 data in _dataList)
		{
			data.ResolveRef(tables);
		}
	}
}

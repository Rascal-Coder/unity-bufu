using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public class Tbworld
{
	private readonly Dictionary<int, world> _dataMap;

	private readonly List<world> _dataList;

	public Dictionary<int, world> DataMap => _dataMap;

	public List<world> DataList => _dataList;

	public world this[int key] => _dataMap[key];

	public Tbworld(JSONNode _buf)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		int count = _buf.Count;
		_dataMap = new Dictionary<int, world>(count);
		_dataList = new List<world>(count);
		foreach (JSONNode child in _buf.Children)
		{
			if (!child.IsObject)
			{
				throw new SerializationException();
			}
			world world2 = world.Deserializeworld(child);
			_dataList.Add(world2);
			_dataMap.Add(world2.Id, world2);
		}
	}

	public world GetOrDefault(int key)
	{
		if (!_dataMap.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	public world Get(int key)
	{
		return _dataMap[key];
	}

	public void ResolveRef(Tables tables)
	{
		foreach (world data in _dataList)
		{
			data.ResolveRef(tables);
		}
	}
}

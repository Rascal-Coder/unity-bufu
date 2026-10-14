using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public class Tbtalent3
{
	private readonly Dictionary<int, talent3> _dataMap;

	private readonly List<talent3> _dataList;

	public Dictionary<int, talent3> DataMap => _dataMap;

	public List<talent3> DataList => _dataList;

	public talent3 this[int key] => _dataMap[key];

	public Tbtalent3(JSONNode _buf)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		int count = _buf.Count;
		_dataMap = new Dictionary<int, talent3>(count);
		_dataList = new List<talent3>(count);
		foreach (JSONNode child in _buf.Children)
		{
			if (!child.IsObject)
			{
				throw new SerializationException();
			}
			talent3 talent5 = talent3.Deserializetalent3(child);
			_dataList.Add(talent5);
			_dataMap.Add(talent5.Id, talent5);
		}
	}

	public talent3 GetOrDefault(int key)
	{
		if (!_dataMap.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	public talent3 Get(int key)
	{
		return _dataMap[key];
	}

	public void ResolveRef(Tables tables)
	{
		foreach (talent3 data in _dataList)
		{
			data.ResolveRef(tables);
		}
	}
}

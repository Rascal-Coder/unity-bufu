using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public class Tbtalent2
{
	private readonly Dictionary<int, talent2> _dataMap;

	private readonly List<talent2> _dataList;

	public Dictionary<int, talent2> DataMap => _dataMap;

	public List<talent2> DataList => _dataList;

	public talent2 this[int key] => _dataMap[key];

	public Tbtalent2(JSONNode _buf)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		int count = _buf.Count;
		_dataMap = new Dictionary<int, talent2>(count);
		_dataList = new List<talent2>(count);
		foreach (JSONNode child in _buf.Children)
		{
			if (!child.IsObject)
			{
				throw new SerializationException();
			}
			talent2 talent5 = talent2.Deserializetalent2(child);
			_dataList.Add(talent5);
			_dataMap.Add(talent5.Id, talent5);
		}
	}

	public talent2 GetOrDefault(int key)
	{
		if (!_dataMap.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	public talent2 Get(int key)
	{
		return _dataMap[key];
	}

	public void ResolveRef(Tables tables)
	{
		foreach (talent2 data in _dataList)
		{
			data.ResolveRef(tables);
		}
	}
}

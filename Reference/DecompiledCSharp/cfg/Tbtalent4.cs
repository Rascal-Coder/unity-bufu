using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public class Tbtalent4
{
	private readonly Dictionary<int, talent4> _dataMap;

	private readonly List<talent4> _dataList;

	public Dictionary<int, talent4> DataMap => _dataMap;

	public List<talent4> DataList => _dataList;

	public talent4 this[int key] => _dataMap[key];

	public Tbtalent4(JSONNode _buf)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		int count = _buf.Count;
		_dataMap = new Dictionary<int, talent4>(count);
		_dataList = new List<talent4>(count);
		foreach (JSONNode child in _buf.Children)
		{
			if (!child.IsObject)
			{
				throw new SerializationException();
			}
			talent4 talent5 = talent4.Deserializetalent4(child);
			_dataList.Add(talent5);
			_dataMap.Add(talent5.Id, talent5);
		}
	}

	public talent4 GetOrDefault(int key)
	{
		if (!_dataMap.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	public talent4 Get(int key)
	{
		return _dataMap[key];
	}

	public void ResolveRef(Tables tables)
	{
		foreach (talent4 data in _dataList)
		{
			data.ResolveRef(tables);
		}
	}
}

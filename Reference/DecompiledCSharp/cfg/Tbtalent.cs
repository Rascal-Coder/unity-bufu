using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public class Tbtalent
{
	private readonly Dictionary<int, talent> _dataMap;

	private readonly List<talent> _dataList;

	public Dictionary<int, talent> DataMap => _dataMap;

	public List<talent> DataList => _dataList;

	public talent this[int key] => _dataMap[key];

	public Tbtalent(JSONNode _buf)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		int count = _buf.Count;
		_dataMap = new Dictionary<int, talent>(count);
		_dataList = new List<talent>(count);
		foreach (JSONNode child in _buf.Children)
		{
			if (!child.IsObject)
			{
				throw new SerializationException();
			}
			talent talent5 = talent.Deserializetalent(child);
			_dataList.Add(talent5);
			_dataMap.Add(talent5.Id, talent5);
		}
	}

	public talent GetOrDefault(int key)
	{
		if (!_dataMap.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	public talent Get(int key)
	{
		return _dataMap[key];
	}

	public void ResolveRef(Tables tables)
	{
		foreach (talent data in _dataList)
		{
			data.ResolveRef(tables);
		}
	}
}

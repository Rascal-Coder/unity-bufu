using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public class Tbdesk
{
	private readonly Dictionary<int, desk> _dataMap;

	private readonly List<desk> _dataList;

	public Dictionary<int, desk> DataMap => _dataMap;

	public List<desk> DataList => _dataList;

	public desk this[int key] => _dataMap[key];

	public Tbdesk(JSONNode _buf)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		int count = _buf.Count;
		_dataMap = new Dictionary<int, desk>(count);
		_dataList = new List<desk>(count);
		foreach (JSONNode child in _buf.Children)
		{
			if (!child.IsObject)
			{
				throw new SerializationException();
			}
			desk desk2 = desk.Deserializedesk(child);
			_dataList.Add(desk2);
			_dataMap.Add(desk2.Id, desk2);
		}
	}

	public desk GetOrDefault(int key)
	{
		if (!_dataMap.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	public desk Get(int key)
	{
		return _dataMap[key];
	}

	public void ResolveRef(Tables tables)
	{
		foreach (desk data in _dataList)
		{
			data.ResolveRef(tables);
		}
	}
}

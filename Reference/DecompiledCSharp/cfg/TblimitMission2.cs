using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public class TblimitMission2
{
	private readonly Dictionary<int, limitMission2> _dataMap;

	private readonly List<limitMission2> _dataList;

	public Dictionary<int, limitMission2> DataMap => _dataMap;

	public List<limitMission2> DataList => _dataList;

	public limitMission2 this[int key] => _dataMap[key];

	public TblimitMission2(JSONNode _buf)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		int count = _buf.Count;
		_dataMap = new Dictionary<int, limitMission2>(count);
		_dataList = new List<limitMission2>(count);
		foreach (JSONNode child in _buf.Children)
		{
			if (!child.IsObject)
			{
				throw new SerializationException();
			}
			limitMission2 limitMission3 = limitMission2.DeserializelimitMission2(child);
			_dataList.Add(limitMission3);
			_dataMap.Add(limitMission3.Id, limitMission3);
		}
	}

	public limitMission2 GetOrDefault(int key)
	{
		if (!_dataMap.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	public limitMission2 Get(int key)
	{
		return _dataMap[key];
	}

	public void ResolveRef(Tables tables)
	{
		foreach (limitMission2 data in _dataList)
		{
			data.ResolveRef(tables);
		}
	}
}

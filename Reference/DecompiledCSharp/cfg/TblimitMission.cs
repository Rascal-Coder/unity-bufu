using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public class TblimitMission
{
	private readonly Dictionary<int, limitMission> _dataMap;

	private readonly List<limitMission> _dataList;

	public Dictionary<int, limitMission> DataMap => _dataMap;

	public List<limitMission> DataList => _dataList;

	public limitMission this[int key] => _dataMap[key];

	public TblimitMission(JSONNode _buf)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		int count = _buf.Count;
		_dataMap = new Dictionary<int, limitMission>(count);
		_dataList = new List<limitMission>(count);
		foreach (JSONNode child in _buf.Children)
		{
			if (!child.IsObject)
			{
				throw new SerializationException();
			}
			limitMission limitMission3 = limitMission.DeserializelimitMission(child);
			_dataList.Add(limitMission3);
			_dataMap.Add(limitMission3.Id, limitMission3);
		}
	}

	public limitMission GetOrDefault(int key)
	{
		if (!_dataMap.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	public limitMission Get(int key)
	{
		return _dataMap[key];
	}

	public void ResolveRef(Tables tables)
	{
		foreach (limitMission data in _dataList)
		{
			data.ResolveRef(tables);
		}
	}
}

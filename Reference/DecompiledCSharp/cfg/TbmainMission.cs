using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public class TbmainMission
{
	private readonly Dictionary<int, mainMission> _dataMap;

	private readonly List<mainMission> _dataList;

	public Dictionary<int, mainMission> DataMap => _dataMap;

	public List<mainMission> DataList => _dataList;

	public mainMission this[int key] => _dataMap[key];

	public TbmainMission(JSONNode _buf)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		int count = _buf.Count;
		_dataMap = new Dictionary<int, mainMission>(count);
		_dataList = new List<mainMission>(count);
		foreach (JSONNode child in _buf.Children)
		{
			if (!child.IsObject)
			{
				throw new SerializationException();
			}
			mainMission mainMission2 = mainMission.DeserializemainMission(child);
			_dataList.Add(mainMission2);
			_dataMap.Add(mainMission2.Id, mainMission2);
		}
	}

	public mainMission GetOrDefault(int key)
	{
		if (!_dataMap.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	public mainMission Get(int key)
	{
		return _dataMap[key];
	}

	public void ResolveRef(Tables tables)
	{
		foreach (mainMission data in _dataList)
		{
			data.ResolveRef(tables);
		}
	}
}

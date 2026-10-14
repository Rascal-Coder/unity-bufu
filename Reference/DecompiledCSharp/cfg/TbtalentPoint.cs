using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public class TbtalentPoint
{
	private readonly Dictionary<int, talentPoint> _dataMap;

	private readonly List<talentPoint> _dataList;

	public Dictionary<int, talentPoint> DataMap => _dataMap;

	public List<talentPoint> DataList => _dataList;

	public talentPoint this[int key] => _dataMap[key];

	public TbtalentPoint(JSONNode _buf)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		int count = _buf.Count;
		_dataMap = new Dictionary<int, talentPoint>(count);
		_dataList = new List<talentPoint>(count);
		foreach (JSONNode child in _buf.Children)
		{
			if (!child.IsObject)
			{
				throw new SerializationException();
			}
			talentPoint talentPoint3 = talentPoint.DeserializetalentPoint(child);
			_dataList.Add(talentPoint3);
			_dataMap.Add(talentPoint3.Id, talentPoint3);
		}
	}

	public talentPoint GetOrDefault(int key)
	{
		if (!_dataMap.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	public talentPoint Get(int key)
	{
		return _dataMap[key];
	}

	public void ResolveRef(Tables tables)
	{
		foreach (talentPoint data in _dataList)
		{
			data.ResolveRef(tables);
		}
	}
}

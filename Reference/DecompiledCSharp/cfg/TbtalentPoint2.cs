using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public class TbtalentPoint2
{
	private readonly Dictionary<int, talentPoint2> _dataMap;

	private readonly List<talentPoint2> _dataList;

	public Dictionary<int, talentPoint2> DataMap => _dataMap;

	public List<talentPoint2> DataList => _dataList;

	public talentPoint2 this[int key] => _dataMap[key];

	public TbtalentPoint2(JSONNode _buf)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		int count = _buf.Count;
		_dataMap = new Dictionary<int, talentPoint2>(count);
		_dataList = new List<talentPoint2>(count);
		foreach (JSONNode child in _buf.Children)
		{
			if (!child.IsObject)
			{
				throw new SerializationException();
			}
			talentPoint2 talentPoint3 = talentPoint2.DeserializetalentPoint2(child);
			_dataList.Add(talentPoint3);
			_dataMap.Add(talentPoint3.Id, talentPoint3);
		}
	}

	public talentPoint2 GetOrDefault(int key)
	{
		if (!_dataMap.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	public talentPoint2 Get(int key)
	{
		return _dataMap[key];
	}

	public void ResolveRef(Tables tables)
	{
		foreach (talentPoint2 data in _dataList)
		{
			data.ResolveRef(tables);
		}
	}
}

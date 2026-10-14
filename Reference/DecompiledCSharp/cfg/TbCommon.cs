using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public class TbCommon
{
	private readonly Dictionary<int, Common> _dataMap;

	private readonly List<Common> _dataList;

	public Dictionary<int, Common> DataMap => _dataMap;

	public List<Common> DataList => _dataList;

	public Common this[int key] => _dataMap[key];

	public TbCommon(JSONNode _buf)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		int count = _buf.Count;
		_dataMap = new Dictionary<int, Common>(count);
		_dataList = new List<Common>(count);
		foreach (JSONNode child in _buf.Children)
		{
			if (!child.IsObject)
			{
				throw new SerializationException();
			}
			Common common = Common.DeserializeCommon(child);
			_dataList.Add(common);
			_dataMap.Add(common.Id, common);
		}
	}

	public Common GetOrDefault(int key)
	{
		if (!_dataMap.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	public Common Get(int key)
	{
		return _dataMap[key];
	}

	public void ResolveRef(Tables tables)
	{
		foreach (Common data in _dataList)
		{
			data.ResolveRef(tables);
		}
	}
}

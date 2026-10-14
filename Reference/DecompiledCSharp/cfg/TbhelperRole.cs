using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public class TbhelperRole
{
	private readonly Dictionary<int, helperRole> _dataMap;

	private readonly List<helperRole> _dataList;

	public Dictionary<int, helperRole> DataMap => _dataMap;

	public List<helperRole> DataList => _dataList;

	public helperRole this[int key] => _dataMap[key];

	public TbhelperRole(JSONNode _buf)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		int count = _buf.Count;
		_dataMap = new Dictionary<int, helperRole>(count);
		_dataList = new List<helperRole>(count);
		foreach (JSONNode child in _buf.Children)
		{
			if (!child.IsObject)
			{
				throw new SerializationException();
			}
			helperRole helperRole2 = helperRole.DeserializehelperRole(child);
			_dataList.Add(helperRole2);
			_dataMap.Add(helperRole2.Id, helperRole2);
		}
	}

	public helperRole GetOrDefault(int key)
	{
		if (!_dataMap.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	public helperRole Get(int key)
	{
		return _dataMap[key];
	}

	public void ResolveRef(Tables tables)
	{
		foreach (helperRole data in _dataList)
		{
			data.ResolveRef(tables);
		}
	}
}

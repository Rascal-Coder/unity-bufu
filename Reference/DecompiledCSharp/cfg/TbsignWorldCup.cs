using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public class TbsignWorldCup
{
	private readonly Dictionary<int, signWorldCup> _dataMap;

	private readonly List<signWorldCup> _dataList;

	public Dictionary<int, signWorldCup> DataMap => _dataMap;

	public List<signWorldCup> DataList => _dataList;

	public signWorldCup this[int key] => _dataMap[key];

	public TbsignWorldCup(JSONNode _buf)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		int count = _buf.Count;
		_dataMap = new Dictionary<int, signWorldCup>(count);
		_dataList = new List<signWorldCup>(count);
		foreach (JSONNode child in _buf.Children)
		{
			if (!child.IsObject)
			{
				throw new SerializationException();
			}
			signWorldCup signWorldCup2 = signWorldCup.DeserializesignWorldCup(child);
			_dataList.Add(signWorldCup2);
			_dataMap.Add(signWorldCup2.Id, signWorldCup2);
		}
	}

	public signWorldCup GetOrDefault(int key)
	{
		if (!_dataMap.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	public signWorldCup Get(int key)
	{
		return _dataMap[key];
	}

	public void ResolveRef(Tables tables)
	{
		foreach (signWorldCup data in _dataList)
		{
			data.ResolveRef(tables);
		}
	}
}

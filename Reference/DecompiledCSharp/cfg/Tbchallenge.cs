using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public class Tbchallenge
{
	private readonly Dictionary<int, challenge> _dataMap;

	private readonly List<challenge> _dataList;

	public Dictionary<int, challenge> DataMap => _dataMap;

	public List<challenge> DataList => _dataList;

	public challenge this[int key] => _dataMap[key];

	public Tbchallenge(JSONNode _buf)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		int count = _buf.Count;
		_dataMap = new Dictionary<int, challenge>(count);
		_dataList = new List<challenge>(count);
		foreach (JSONNode child in _buf.Children)
		{
			if (!child.IsObject)
			{
				throw new SerializationException();
			}
			challenge challenge2 = challenge.Deserializechallenge(child);
			_dataList.Add(challenge2);
			_dataMap.Add(challenge2.Id, challenge2);
		}
	}

	public challenge GetOrDefault(int key)
	{
		if (!_dataMap.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	public challenge Get(int key)
	{
		return _dataMap[key];
	}

	public void ResolveRef(Tables tables)
	{
		foreach (challenge data in _dataList)
		{
			data.ResolveRef(tables);
		}
	}
}

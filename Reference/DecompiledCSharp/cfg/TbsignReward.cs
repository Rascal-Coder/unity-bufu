using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public class TbsignReward
{
	private readonly Dictionary<int, signReward> _dataMap;

	private readonly List<signReward> _dataList;

	public Dictionary<int, signReward> DataMap => _dataMap;

	public List<signReward> DataList => _dataList;

	public signReward this[int key] => _dataMap[key];

	public TbsignReward(JSONNode _buf)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		int count = _buf.Count;
		_dataMap = new Dictionary<int, signReward>(count);
		_dataList = new List<signReward>(count);
		foreach (JSONNode child in _buf.Children)
		{
			if (!child.IsObject)
			{
				throw new SerializationException();
			}
			signReward signReward2 = signReward.DeserializesignReward(child);
			_dataList.Add(signReward2);
			_dataMap.Add(signReward2.Id, signReward2);
		}
	}

	public signReward GetOrDefault(int key)
	{
		if (!_dataMap.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	public signReward Get(int key)
	{
		return _dataMap[key];
	}

	public void ResolveRef(Tables tables)
	{
		foreach (signReward data in _dataList)
		{
			data.ResolveRef(tables);
		}
	}
}

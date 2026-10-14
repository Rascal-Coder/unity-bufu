using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public class TbWorldUpScoreReward
{
	private readonly Dictionary<int, WorldUpScoreReward> _dataMap;

	private readonly List<WorldUpScoreReward> _dataList;

	public Dictionary<int, WorldUpScoreReward> DataMap => _dataMap;

	public List<WorldUpScoreReward> DataList => _dataList;

	public WorldUpScoreReward this[int key] => _dataMap[key];

	public TbWorldUpScoreReward(JSONNode _buf)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		int count = _buf.Count;
		_dataMap = new Dictionary<int, WorldUpScoreReward>(count);
		_dataList = new List<WorldUpScoreReward>(count);
		foreach (JSONNode child in _buf.Children)
		{
			if (!child.IsObject)
			{
				throw new SerializationException();
			}
			WorldUpScoreReward worldUpScoreReward = WorldUpScoreReward.DeserializeWorldUpScoreReward(child);
			_dataList.Add(worldUpScoreReward);
			_dataMap.Add(worldUpScoreReward.Id, worldUpScoreReward);
		}
	}

	public WorldUpScoreReward GetOrDefault(int key)
	{
		if (!_dataMap.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	public WorldUpScoreReward Get(int key)
	{
		return _dataMap[key];
	}

	public void ResolveRef(Tables tables)
	{
		foreach (WorldUpScoreReward data in _dataList)
		{
			data.ResolveRef(tables);
		}
	}
}

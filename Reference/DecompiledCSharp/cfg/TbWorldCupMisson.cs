using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public class TbWorldCupMisson
{
	private readonly Dictionary<int, WorldCupMisson> _dataMap;

	private readonly List<WorldCupMisson> _dataList;

	public Dictionary<int, WorldCupMisson> DataMap => _dataMap;

	public List<WorldCupMisson> DataList => _dataList;

	public WorldCupMisson this[int key] => _dataMap[key];

	public TbWorldCupMisson(JSONNode _buf)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		int count = _buf.Count;
		_dataMap = new Dictionary<int, WorldCupMisson>(count);
		_dataList = new List<WorldCupMisson>(count);
		foreach (JSONNode child in _buf.Children)
		{
			if (!child.IsObject)
			{
				throw new SerializationException();
			}
			WorldCupMisson worldCupMisson = WorldCupMisson.DeserializeWorldCupMisson(child);
			_dataList.Add(worldCupMisson);
			_dataMap.Add(worldCupMisson.Id, worldCupMisson);
		}
	}

	public WorldCupMisson GetOrDefault(int key)
	{
		if (!_dataMap.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	public WorldCupMisson Get(int key)
	{
		return _dataMap[key];
	}

	public void ResolveRef(Tables tables)
	{
		foreach (WorldCupMisson data in _dataList)
		{
			data.ResolveRef(tables);
		}
	}
}

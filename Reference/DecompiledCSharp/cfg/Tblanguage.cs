using System.Collections.Generic;
using Luban;
using SimpleJSON;

namespace cfg;

public class Tblanguage
{
	private readonly Dictionary<int, language> _dataMap;

	private readonly List<language> _dataList;

	public Dictionary<int, language> DataMap => _dataMap;

	public List<language> DataList => _dataList;

	public language this[int key] => _dataMap[key];

	public Tblanguage(JSONNode _buf)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		int count = _buf.Count;
		_dataMap = new Dictionary<int, language>(count);
		_dataList = new List<language>(count);
		foreach (JSONNode child in _buf.Children)
		{
			if (!child.IsObject)
			{
				throw new SerializationException();
			}
			language language2 = language.Deserializelanguage(child);
			_dataList.Add(language2);
			_dataMap.Add(language2.Id, language2);
		}
	}

	public language GetOrDefault(int key)
	{
		if (!_dataMap.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	public language Get(int key)
	{
		return _dataMap[key];
	}

	public void ResolveRef(Tables tables)
	{
		foreach (language data in _dataList)
		{
			data.ResolveRef(tables);
		}
	}
}

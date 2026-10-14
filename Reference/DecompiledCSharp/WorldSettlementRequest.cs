using System;
using System.Collections.Generic;

public readonly struct WorldSettlementRequest
{
	private readonly WorldSettlementEntry _singleEntry;

	private readonly IReadOnlyList<WorldSettlementEntry> _entries;

	public int WorldId { get; }

	public int GameplayType { get; }

	public WorldSettlementSource Source { get; }

	public int EntryCount
	{
		get
		{
			if (_entries == null)
			{
				return 1;
			}
			return _entries.Count;
		}
	}

	public WorldSettlementRequest(int worldId, int gameplayType, WorldSettlementSource source, WorldSettlementEntry entry)
	{
		WorldId = worldId;
		GameplayType = gameplayType;
		Source = source;
		_singleEntry = entry;
		_entries = null;
	}

	public WorldSettlementRequest(int worldId, int gameplayType, WorldSettlementSource source, IReadOnlyList<WorldSettlementEntry> entries)
	{
		WorldId = worldId;
		GameplayType = gameplayType;
		Source = source;
		_singleEntry = default(WorldSettlementEntry);
		_entries = entries;
	}

	public WorldSettlementEntry GetEntry(int index)
	{
		if (_entries != null)
		{
			return _entries[index];
		}
		if (index != 0)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return _singleEntry;
	}
}

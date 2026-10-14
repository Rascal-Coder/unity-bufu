public readonly struct WorldSettlementResult
{
	public int WorldId { get; }

	public int GameplayType { get; }

	public WorldSettlementSource Source { get; }

	public int EntryCount { get; }

	public double TotalReward { get; }

	public WorldSettlementResult(int worldId, int gameplayType, WorldSettlementSource source, int entryCount, double totalReward)
	{
		WorldId = worldId;
		GameplayType = gameplayType;
		Source = source;
		EntryCount = entryCount;
		TotalReward = totalReward;
	}
}

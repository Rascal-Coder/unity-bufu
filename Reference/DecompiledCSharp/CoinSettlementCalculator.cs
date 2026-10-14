public sealed class CoinSettlementCalculator : IWorldSettlementCalculator
{
	public int GameplayType => 1;

	public bool TryCalculate(in WorldSettlementRequest request, out double totalReward, out int entryCount)
	{
		entryCount = request.EntryCount;
		totalReward = ((entryCount == 1) ? request.GetEntry(0).Reward : 0.0);
		return entryCount == 1;
	}

	bool IWorldSettlementCalculator.TryCalculate(in WorldSettlementRequest request, out double totalReward, out int entryCount)
	{
		return TryCalculate(in request, out totalReward, out entryCount);
	}
}

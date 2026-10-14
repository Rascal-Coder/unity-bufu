public sealed class DiceSettlementCalculator : IWorldSettlementCalculator
{
	public int GameplayType => 2;

	public bool TryCalculate(in WorldSettlementRequest request, out double totalReward, out int entryCount)
	{
		entryCount = request.EntryCount;
		totalReward = 0.0;
		for (int i = 0; i < entryCount; i++)
		{
			totalReward += request.GetEntry(i).Reward;
		}
		return entryCount > 0;
	}

	bool IWorldSettlementCalculator.TryCalculate(in WorldSettlementRequest request, out double totalReward, out int entryCount)
	{
		return TryCalculate(in request, out totalReward, out entryCount);
	}
}

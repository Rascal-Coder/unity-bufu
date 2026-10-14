public interface IWorldSettlementCalculator
{
	int GameplayType { get; }

	bool TryCalculate(in WorldSettlementRequest request, out double totalReward, out int entryCount);
}

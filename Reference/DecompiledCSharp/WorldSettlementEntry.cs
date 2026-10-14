using System;

public readonly struct WorldSettlementEntry
{
	public int EntityId { get; }

	public int Outcome { get; }

	public double BaseValue { get; }

	public double Multiplier { get; }

	public int Quantity { get; }

	public double Reward => BaseValue * Multiplier * (double)Math.Max(0, Quantity);

	public WorldSettlementEntry(int entityId, int outcome, double baseValue, double multiplier = 1.0, int quantity = 1)
	{
		EntityId = entityId;
		Outcome = outcome;
		BaseValue = baseValue;
		Multiplier = multiplier;
		Quantity = quantity;
	}
}

namespace Module.Role;

public readonly struct DiceRollResult
{
	public int QualityId { get; }

	public int Face { get; }

	public double BaseValue { get; }

	public double ValueMultiplier { get; }

	public double Value => BaseValue * ValueMultiplier;

	public DiceRollResult(int qualityId, int face, double baseValue, double valueMultiplier)
	{
		QualityId = qualityId;
		Face = face;
		BaseValue = baseValue;
		ValueMultiplier = valueMultiplier;
	}
}

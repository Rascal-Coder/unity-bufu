namespace Module.Role;

public class CoinPurple : CoinBase
{
	protected override void Awake()
	{
		coinType = CoinType.Purple;
		base.Awake();
	}
}

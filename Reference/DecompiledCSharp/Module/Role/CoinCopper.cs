namespace Module.Role;

public class CoinCopper : CoinBase
{
	protected override void Awake()
	{
		coinType = CoinType.Copper;
		base.Awake();
	}
}

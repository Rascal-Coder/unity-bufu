namespace Module.Role;

public class CoinSilver : CoinBase
{
	protected override void Awake()
	{
		coinType = CoinType.Silver;
		base.Awake();
	}
}

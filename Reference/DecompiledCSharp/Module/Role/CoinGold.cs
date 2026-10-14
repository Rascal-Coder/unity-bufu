namespace Module.Role;

public class CoinGold : CoinBase
{
	protected override void Awake()
	{
		coinType = CoinType.Gold;
		base.Awake();
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;
using cfg;

public sealed class WorldSettlementService
{
	private static WorldSettlementService _instance;

	private readonly Dictionary<int, IWorldSettlementCalculator> _calculators = new Dictionary<int, IWorldSettlementCalculator>();

	public static WorldSettlementService Instance => _instance ?? (_instance = new WorldSettlementService());

	public event Action<WorldSettlementRequest, WorldSettlementResult> Settled;

	private WorldSettlementService()
	{
		Register(new CoinSettlementCalculator());
		Register(new DiceSettlementCalculator());
	}

	public void Register(IWorldSettlementCalculator calculator)
	{
		if (calculator == null)
		{
			throw new ArgumentNullException("calculator");
		}
		_calculators[calculator.GameplayType] = calculator;
	}

	public bool TrySettle(in WorldSettlementRequest request, out WorldSettlementResult result)
	{
		result = default(WorldSettlementResult);
		if (request.WorldId <= 0 || (Object)(object)GameDataManager.Instance == (Object)null || (Object)(object)SingletonMono<UpgradeManager>.Instance == (Object)null)
		{
			return false;
		}
		if (GameDataManager.Instance.GetCurrentWorldId() != request.WorldId)
		{
			return false;
		}
		if (!ConfigManager.HasInstance())
		{
			return false;
		}
		world worldConfig = ConfigManager.Instance.GetWorldConfig(request.WorldId);
		if (worldConfig == null || worldConfig.GameplayType != request.GameplayType)
		{
			return false;
		}
		if (!_calculators.TryGetValue(request.GameplayType, out var value) || !value.TryCalculate(in request, out var totalReward, out var entryCount) || double.IsNaN(totalReward) || double.IsInfinity(totalReward) || totalReward <= 0.0)
		{
			return false;
		}
		SingletonMono<UpgradeManager>.Instance.AddGold(totalReward);
		result = new WorldSettlementResult(request.WorldId, request.GameplayType, request.Source, entryCount, totalReward);
		this.Settled?.Invoke(request, result);
		return true;
	}
}

using System;
using cfg;

public sealed class WorldGameplayContext
{
	public world Config { get; }

	public WorldData Data { get; }

	public int WorldId => Config.Id;

	public int GameplayType => Config.GameplayType;

	public WorldGameplayContext(world config, WorldData data)
	{
		Config = config ?? throw new ArgumentNullException("config");
		Data = data;
	}
}

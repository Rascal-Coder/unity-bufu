using System;

public static class WorldRankUtility
{
	public static string GetZoneId(int worldId)
	{
		return "world_" + Math.Max(1, worldId);
	}

	public static string GetWechatStorageKey(int worldId)
	{
		return "user_high_score_" + Math.Max(1, worldId);
	}
}

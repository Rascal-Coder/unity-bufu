using System;

[Serializable]
public sealed class WorldServerData
{
	public string openId = "";

	public int worldId = 1;

	public GlobalProfileDataSave globalProfileDataSave;

	public WorldDataSave worldDataSave;

	public PlayerPrefsDataSave playerPrefsDataSave;

	public WorldCupCompactData worldCupCompactData;

	public string source = "current";

	public int selectedLevel;

	public int currentLevel;

	public int backupLevel;
}

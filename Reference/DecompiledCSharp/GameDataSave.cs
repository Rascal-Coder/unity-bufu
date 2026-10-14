using System;

[Serializable]
public class GameDataSave
{
	public GameData gameData;

	public GlobalProfileData globalProfileData;

	public WorldDataCollection worldDataCollection;

	public string version = "1.0.0";

	public string saveTime;
}

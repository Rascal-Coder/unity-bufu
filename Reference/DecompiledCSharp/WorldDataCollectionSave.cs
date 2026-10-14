using System;

[Serializable]
public sealed class WorldDataCollectionSave
{
	public WorldDataCollection worldDataCollection;

	public string version = "1.0.0";

	public string saveTime;
}

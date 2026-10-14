using System;

[Serializable]
public sealed class WorldArchiveSave
{
	public const int CurrentVersion = 2;

	public int dataVersion = 2;

	public long revision;

	public long savedAtUnixMs;

	public GlobalProfileData globalProfileData;

	public WorldDataCollection worldDataCollection;
}

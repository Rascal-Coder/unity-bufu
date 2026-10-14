using System;
using System.Collections.Generic;

[Serializable]
public sealed class WorldDataCollection
{
	public const int CurrentVersion = 2;

	public int dataVersion = 2;

	public List<WorldData> worlds = new List<WorldData>();
}

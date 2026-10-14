using System;

[Serializable]
public class WorldCupData
{
	public long signFirstShowTime;

	public int signDay;

	public int[] clamp = new int[14];

	public int score;

	public int scoreLastDay = -1;

	public int[] missionProgress = new int[6];

	public int[] missionClamp = new int[6];

	public int[] scoreClamp = new int[4];
}

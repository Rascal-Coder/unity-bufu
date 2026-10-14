using System;

[Serializable]
public class PlayerHelpStatusData
{
	public string targetOpenId;

	public string day;

	public int todayHelpCount;

	public int totalHelpCount;

	public string helperOpenId;

	public bool isHelped;

	public int secondsRemaining;

	public int cooldownHours;
}

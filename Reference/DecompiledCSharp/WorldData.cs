using System;

[Serializable]
public sealed class WorldData
{
	public int worldId = 1;

	public GameData gameData = new GameData();

	public FacilityData facilityData = new FacilityData();

	public UpgradeData upgradeData = new UpgradeData();

	public TalentData talentData = new TalentData();

	public DiceWorldProgressData diceProgress = new DiceWorldProgressData();

	public long bestScore;

	public long lastEnterTime;

	public long lastExitTime;
}

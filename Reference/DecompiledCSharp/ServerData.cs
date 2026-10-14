using System;

[Serializable]
public class ServerData
{
	public string openId = "";

	public GlobalProfileDataSave globalProfileDataSave;

	public WorldDataCollectionSave worldDataCollectionSave;

	public GameDataSave gameDataSave;

	public FacilityDataSave facilityDataSave;

	public UpgradeDataSave upgradeDataSave;

	public TalentDataSave talentDataSave;

	public PlayerPrefsDataSave playerPrefsDataSave;

	public int bestScore;
}

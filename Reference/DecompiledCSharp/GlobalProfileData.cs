using System;
using System.Collections.Generic;

[Serializable]
public sealed class GlobalProfileData
{
	public const int CurrentVersion = 2;

	public int dataVersion = 2;

	public string localUserId = "";

	public string openId = "";

	public string nickName = "";

	public string avatarUrl = "";

	public long diamonds;

	public List<GameItem> itemList = new List<GameItem>();

	public List<HelperInfo> helperOwned = new List<HelperInfo>();

	public List<GlobalHelperSkillData> globalHelperSkills = new List<GlobalHelperSkillData>();

	public long inviteTime;

	public long helpedTime;

	public int currentWorldId = 1;

	public bool worldEntryUnlocked;

	public List<int> unlockedWorldIds = new List<int> { 1 };
}

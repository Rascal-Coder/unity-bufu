using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
	public string localUserId = "";

	public string openId = "";

	public string nickName = "";

	public string avatarUrl = "";

	public int CurrentLevel = 1;

	public int currentMaxUniqueId;

	public int Guide_Step;

	public long lastEnterGameTime;

	public long lastExitGameTime;

	public bool isExitTimeUtc;

	public long diamonds;

	public List<GameItem> itemList = new List<GameItem>();

	public bool isInitializedCoinDiamond;

	public int currentDeskId = 1;

	public int currentMainMissionLevel;

	public long[] lastSupportTime = new long[4];

	public int randomEventMask;

	public int level = 1;

	public int experience;

	public int talentPoint;

	public int costTalentPoint;

	public int rebornCurrentLevel = -1;

	public int todayAdCount;

	public long totalAdCount;

	public string lastAdDate = "";

	public double totalGold;

	public double averageCoin;

	public int checkInDay;

	public int todayMonth;

	public int today;

	public int[] isClampItem = new int[28];

	public long InviteTime;

	public long HelpedTime;

	[NonSerialized]
	public Dictionary<int, int> upgradeItemLevelsDict = new Dictionary<int, int>();

	public int currentChallengeId = 1;

	public int challengeLevel = 1;

	public double auctionCoin = 1200.0;

	public int ticketCount = 3;

	public long lastTicketRecoverTime;

	public int auctionCoinExchangeCount = 1;

	public int auctionDiamondExchangeCount = 1;

	public int adEnterExchangeCount;

	public float supperSixCount;

	public float supperSixTarget;

	public int shangRenTodayDiamond;

	public string shangRenLastDate = "";

	public long shangRenWaitStartTime;

	public float shangRenWaitTargetTime;

	public int platform = 1;
}

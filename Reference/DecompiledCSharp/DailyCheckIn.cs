using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using cfg;

public class DailyCheckIn : MonoBehaviour
{
	public enum RewardType
	{
		Gold,
		Diamond,
		Item
	}

	public enum CheckInType
	{
		Free,
		Ad
	}

	public class RewardItemInfo
	{
		public CheckInType checkInType;

		public RewardType[] rewardType = new RewardType[2];

		public Sprite[] itemIcon = (Sprite[])(object)new Sprite[2];

		public int[] amount = new int[2];

		public int[] id = new int[2];
	}

	[Header("奖励道具配置icon")]
	[SerializeField]
	private Sprite[] itemIcons;

	[Header("奖励道具配置icon2,主要用于非金币世界道具icon，如不同样式的金钱icon")]
	[SerializeField]
	private Sprite[] itemIcons2;

	[Header("UI 引用")]
	[SerializeField]
	private GameObject checkInPanel;

	[SerializeField]
	private Transform freeRewardListParent;

	[SerializeField]
	private Transform adRewardListParent;

	[SerializeField]
	private GameObject FreeCheckInReward;

	[SerializeField]
	private GameObject AdCheckInReward;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private Button maskButton;

	[SerializeField]
	private RectTransform subscribeRect;

	[SerializeField]
	private Button subscribeButton;

	private List<CheckInRewardItem> _freeRewardItems = new List<CheckInRewardItem>();

	private List<CheckInRewardItem> _adRewardItems = new List<CheckInRewardItem>();

	private int _loadedWorldId = -1;

	private static DailyCheckIn _instance;

	private bool _isInitialized;

	public static DailyCheckIn Instance
	{
		get
		{
			if ((Object)(object)_instance == (Object)null)
			{
				_instance = Object.FindObjectOfType<DailyCheckIn>();
			}
			return _instance;
		}
	}

	private void Awake()
	{
		_instance = this;
	}

	public void ManualInitialize()
	{
		if (!_isInitialized)
		{
			_isInitialized = true;
			InitializeUI();
		}
	}

	private void Start()
	{
		ManualInitialize();
	}

	private void InitializeUI()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		if ((Object)(object)closeButton != (Object)null)
		{
			((UnityEvent)closeButton.onClick).AddListener(new UnityAction(HideCheckInPanel));
		}
		if ((Object)(object)maskButton != (Object)null)
		{
			((UnityEvent)maskButton.onClick).AddListener(new UnityAction(HideCheckInPanel));
		}
		if ((Object)(object)subscribeButton != (Object)null)
		{
			((UnityEvent)subscribeButton.onClick).AddListener(new UnityAction(OnSubscribe));
		}
		CreateRewardList();
		RefreshUI();
		RefreshSignHelperGuide();
		RefreshSubscribeEntry();
	}

	private void CreateRewardList()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)freeRewardListParent == (Object)null || (Object)(object)FreeCheckInReward == (Object)null || (Object)(object)AdCheckInReward == (Object)null || (Object)(object)adRewardListParent == (Object)null)
		{
			return;
		}
		foreach (Transform item in freeRewardListParent)
		{
			Object.Destroy((Object)(object)((Component)item).gameObject);
		}
		foreach (Transform item2 in adRewardListParent)
		{
			Object.Destroy((Object)(object)((Component)item2).gameObject);
		}
		_freeRewardItems.Clear();
		_adRewardItems.Clear();
		_loadedWorldId = GetCurrentWorldId();
		for (int i = 1; i <= 7; i++)
		{
			CheckInRewardItem component = Object.Instantiate<GameObject>(FreeCheckInReward, freeRewardListParent).GetComponent<CheckInRewardItem>();
			CheckInRewardItem component2 = Object.Instantiate<GameObject>(AdCheckInReward, adRewardListParent).GetComponent<CheckInRewardItem>();
			RewardItemInfo dayReward = GetDayReward(i, isFree: true);
			RewardItemInfo dayReward2 = GetDayReward(i, isFree: false);
			component.Initialize(dayReward.id, dayReward.itemIcon, dayReward.amount, dayReward.rewardType, dayReward.checkInType, i);
			component2.Initialize(dayReward2.id, dayReward2.itemIcon, dayReward2.amount, dayReward2.rewardType, dayReward2.checkInType, i);
			_freeRewardItems.Add(component);
			_adRewardItems.Add(component2);
		}
	}

	public void RefreshUI()
	{
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			GameDataManager.Instance.CheckAndAdvanceCheckInDayCrossDay();
		}
		int currentWorldId = GetCurrentWorldId();
		if (_loadedWorldId != currentWorldId || _freeRewardItems.Count == 0 || _adRewardItems.Count == 0)
		{
			CreateRewardList();
		}
		bool flag = false;
		for (int i = 0; i < _freeRewardItems.Count; i++)
		{
			_freeRewardItems[i].RefreshItem();
			_adRewardItems[i].RefreshItem();
			if (_freeRewardItems[i].GetIsClamped() || _adRewardItems[i].GetIsClamped())
			{
				flag = true;
			}
		}
		if ((Object)(object)UIManager.Instance.topPanel != (Object)null)
		{
			if (flag)
			{
				UIManager.Instance.topPanel.SetCheckRepoint(isOpen: true);
			}
			else
			{
				UIManager.Instance.topPanel.SetCheckRepoint(isOpen: false);
			}
		}
		RefreshSignHelperGuide();
		RefreshSubscribeEntry();
	}

	private void RefreshSignHelperGuide()
	{
		if ((Object)(object)UIManager.Instance == (Object)null || (Object)(object)UIManager.Instance.topPanel == (Object)null || (Object)(object)GameDataManager.Instance == (Object)null)
		{
			return;
		}
		bool yinDaoTu = false;
		int checkInDayByWorld = GameDataManager.Instance.GetCheckInDayByWorld(GetCurrentWorldId());
		if (checkInDayByWorld == 3 || checkInDayByWorld == 7)
		{
			signReward currentWorldSignReward = GetCurrentWorldSignReward(checkInDayByWorld);
			int num = 0;
			if (currentWorldSignReward != null && currentWorldSignReward.AdReward != null && currentWorldSignReward.AdReward.Count > 2)
			{
				num = GetHelperId(currentWorldSignReward.AdReward[2]);
			}
			yinDaoTu = num > 0 && !GameDataManager.Instance.IsHelperOwned(num);
		}
		UIManager.Instance.topPanel.SetYinDaoTu(yinDaoTu);
	}

	private void OnSubscribe()
	{
		if (ShouldUseKungfuMasterSubscribe(3, CheckInType.Ad))
		{
			TTFeedDirectPlayManager.Instance.ZhuDongRequestKungfuMasterThirdDaySignInFeedSubscribe(delegate
			{
				BroadcastManager.ShowPopUpTip("订阅成功，请在第三天回来签到领取奖励");
				((Behaviour)subscribeButton).enabled = false;
			}, delegate
			{
				BroadcastManager.ShowPopUpTip("订阅失败，当天无法订阅，明天再试!");
				((Behaviour)subscribeButton).enabled = false;
			});
		}
	}

	private void RefreshSubscribeEntry()
	{
		if (!((Object)(object)subscribeRect == (Object)null))
		{
			bool flag = ShouldUseKungfuMasterSubscribe(3, CheckInType.Ad) && (Object)(object)GameDataManager.Instance != (Object)null && !GameDataManager.Instance.IsHelperOwned(4);
			((Component)subscribeRect).gameObject.SetActive(flag);
			if ((Object)(object)subscribeButton != (Object)null)
			{
				((Behaviour)subscribeButton).enabled = flag;
			}
		}
	}

	public void HideCheckInPanel()
	{
		if ((Object)(object)checkInPanel != (Object)null)
		{
			checkInPanel.SetActive(false);
		}
	}

	private void SaveCheckInData()
	{
		PlayerPrefs.Save();
	}

	public bool ClaimReward(int day)
	{
		SaveCheckInData();
		RefreshUI();
		Debug.Log((object)$"[DailyCheckIn] 第{day}天免费奖励领取成功！");
		return true;
	}

	public void GiveReward(int id, RewardType rewardType, double amount)
	{
		if (id == 1 && rewardType == RewardType.Gold)
		{
			SingletonMono<UpgradeManager>.Instance?.AddGold(amount);
		}
		else if (id == 2 && rewardType == RewardType.Diamond)
		{
			GameDataManager.Instance.AddDiamond((long)amount);
		}
		else if (id >= 101 && rewardType == RewardType.Item)
		{
			GameDataManager.Instance.AddItem(id, (long)amount);
		}
	}

	public RewardItemInfo GetDayReward(int day, bool isFree)
	{
		RewardItemInfo rewardItemInfo = new RewardItemInfo();
		signReward currentWorldSignReward = GetCurrentWorldSignReward(day);
		if (currentWorldSignReward == null)
		{
			Debug.LogError((object)$"[DailyCheckIn] Missing signReward config. worldId={GetCurrentWorldId()}, day={day}");
			return rewardItemInfo;
		}
		if (isFree)
		{
			if (!IsRewardListValid(currentWorldSignReward.NormalReward))
			{
				Debug.LogError((object)$"[DailyCheckIn] Invalid normalReward config. signRewardId={currentWorldSignReward.Id}, worldId={currentWorldSignReward.WorldId}");
				return rewardItemInfo;
			}
			rewardItemInfo.checkInType = CheckInType.Free;
			rewardItemInfo.rewardType[0] = GetRewardType(currentWorldSignReward.NormalReward[0]);
			rewardItemInfo.rewardType[1] = GetRewardType(currentWorldSignReward.NormalReward[2]);
			rewardItemInfo.itemIcon[0] = GetRewardSprite(currentWorldSignReward.NormalReward[0]);
			rewardItemInfo.itemIcon[1] = GetRewardSprite(currentWorldSignReward.NormalReward[2]);
			rewardItemInfo.amount[0] = currentWorldSignReward.NormalReward[1];
			rewardItemInfo.amount[1] = currentWorldSignReward.NormalReward[3];
			rewardItemInfo.id[0] = currentWorldSignReward.NormalReward[0];
			rewardItemInfo.id[1] = currentWorldSignReward.NormalReward[2];
		}
		else
		{
			if (!IsRewardListValid(currentWorldSignReward.AdReward))
			{
				Debug.LogError((object)$"[DailyCheckIn] Invalid adReward config. signRewardId={currentWorldSignReward.Id}, worldId={currentWorldSignReward.WorldId}");
				return rewardItemInfo;
			}
			rewardItemInfo.checkInType = CheckInType.Ad;
			rewardItemInfo.rewardType[0] = GetRewardType(currentWorldSignReward.AdReward[0]);
			rewardItemInfo.rewardType[1] = GetRewardType(currentWorldSignReward.AdReward[2]);
			rewardItemInfo.itemIcon[0] = GetRewardSprite(currentWorldSignReward.AdReward[0]);
			rewardItemInfo.itemIcon[1] = GetRewardSprite(currentWorldSignReward.AdReward[2]);
			rewardItemInfo.amount[0] = currentWorldSignReward.AdReward[1];
			rewardItemInfo.amount[1] = currentWorldSignReward.AdReward[3];
			rewardItemInfo.id[0] = currentWorldSignReward.AdReward[0];
			rewardItemInfo.id[1] = currentWorldSignReward.AdReward[2];
		}
		return rewardItemInfo;
	}

	public int GetCurrentWorldId()
	{
		if (!((Object)(object)GameDataManager.Instance != (Object)null))
		{
			return 1;
		}
		return GameDataManager.Instance.GetCurrentWorldId();
	}

	private signReward GetCurrentWorldSignReward(int day)
	{
		return ConfigManager.Instance.GetSignRewardItemByWorldDay(GetCurrentWorldId(), day);
	}

	public bool ShouldUseKungfuMasterSubscribe(int day, CheckInType type)
	{
		if (type != CheckInType.Ad || day != 3)
		{
			return false;
		}
		signReward currentWorldSignReward = GetCurrentWorldSignReward(day);
		if (currentWorldSignReward == null || currentWorldSignReward.AdReward == null || currentWorldSignReward.AdReward.Count <= 2)
		{
			return false;
		}
		return GetHelperId(currentWorldSignReward.AdReward[2]) == 4;
	}

	private static bool IsRewardListValid(List<int> rewards)
	{
		if (rewards != null)
		{
			return rewards.Count >= 4;
		}
		return false;
	}

	public RewardType GetRewardType(int id)
	{
		return ConfigManager.Instance.GetItem(id).Id switch
		{
			1 => RewardType.Gold, 
			2 => RewardType.Diamond, 
			_ => RewardType.Item, 
		};
	}

	public Sprite GetRewardSprite(int id)
	{
		item item = ConfigManager.Instance.GetItem(id);
		List<item> allItem = ConfigManager.Instance.GetAllItem();
		int num = 0;
		for (int i = 0; i < allItem.Count; i++)
		{
			if (allItem[i].Id < 101)
			{
				num++;
			}
		}
		if (item.Id > 0 && item.Id < 101)
		{
			int num2 = GameDataManager.Instance?.GetCurrentWorldId() ?? 1;
			if (item.Id == 1)
			{
				switch (num2)
				{
				case 1:
					return itemIcons[item.Id - 1];
				case 2:
					return itemIcons2[0];
				}
			}
			return itemIcons[item.Id - 1];
		}
		if (item.Id >= 101)
		{
			return itemIcons[num + item.Id - 101];
		}
		return itemIcons[0];
	}

	public int GetHelperId(int suipianId)
	{
		return suipianId switch
		{
			101 => 1, 
			102 => 2, 
			103 => 3, 
			104 => 4, 
			105 => 5, 
			106 => 6, 
			107 => 7, 
			108 => 8, 
			109 => 9, 
			110 => 10, 
			_ => 1, 
		};
	}
}

using System.Collections.Generic;
using Module.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CheckInRewardItem : MonoBehaviour
{
	private DailyCheckIn.CheckInType checkInType;

	private DailyCheckIn.RewardType[] rewardType;

	private int dayIndex;

	[Header("两个道具id")]
	private int[] id;

	[Header("两个奖励按键")]
	private Button[] rewardbtns;

	[Header("两个奖励道具图片")]
	private Image[] itemImgs;

	[Header("两个要显示的道具图片精灵")]
	public Sprite[] itemIcons;

	[Header("两个奖励数量显示")]
	private TextMeshProUGUI[] rewardAmounts;

	[Header("两个奖励道具红点提示")]
	private Image[] redPoints;

	[Header("两个奖励道具已领取遮罩图片")]
	private GameObject[] clampedImgs;

	[Header("签到奖励行标题")]
	private TextMeshProUGUI title;

	[Header("获取奖励按键")]
	[SerializeField]
	private Button allRewardBtn;

	[Header("视频图标")]
	[SerializeField]
	private Image videoIcon;

	[Header("订阅图标")]
	[SerializeField]
	private Image subscribeIcon;

	private GameObject allRepoint;

	private Transform mask;

	private double[] rewardCounts;

	private double[] realCounts;

	private List<double> rewardList = new List<double>();

	private bool canClamped;

	private void Awake()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		Load();
		BtnClick();
		((UnityEvent)allRewardBtn.onClick).AddListener((UnityAction)delegate
		{
			ButtonClickedEvent onClick = rewardbtns[0].onClick;
			if (onClick != null)
			{
				((UnityEvent)onClick).Invoke();
			}
			ButtonClickedEvent onClick2 = rewardbtns[1].onClick;
			if (onClick2 != null)
			{
				((UnityEvent)onClick2).Invoke();
			}
		});
		Image obj = videoIcon;
		if (obj != null)
		{
			((Component)obj).gameObject.SetActive(true);
		}
		Image obj2 = subscribeIcon;
		if (obj2 != null)
		{
			((Component)obj2).gameObject.SetActive(false);
		}
	}

	private void Load()
	{
		rewardbtns = (Button[])(object)new Button[2];
		itemImgs = (Image[])(object)new Image[2];
		rewardAmounts = (TextMeshProUGUI[])(object)new TextMeshProUGUI[2];
		redPoints = (Image[])(object)new Image[2];
		itemIcons = (Sprite[])(object)new Sprite[2];
		rewardCounts = new double[2];
		rewardType = new DailyCheckIn.RewardType[2];
		clampedImgs = (GameObject[])(object)new GameObject[2];
		realCounts = new double[2];
		title = ((Component)((Component)this).transform.Find("title")).GetComponent<TextMeshProUGUI>();
		Button component = ((Component)((Component)this).transform.Find("rewardbtn1")).GetComponent<Button>();
		Button component2 = ((Component)((Component)this).transform.Find("rewardbtn2")).GetComponent<Button>();
		if ((Object)(object)component != (Object)null && (Object)(object)component2 != (Object)null)
		{
			rewardbtns[0] = component;
			rewardbtns[1] = component2;
		}
		Image component3 = ((Component)((Component)this).transform.Find("rewardbtn1/itemimg")).GetComponent<Image>();
		Image component4 = ((Component)((Component)this).transform.Find("rewardbtn2/itemimg")).GetComponent<Image>();
		if ((Object)(object)component3 != (Object)null && (Object)(object)component4 != (Object)null)
		{
			itemImgs[0] = component3;
			itemImgs[1] = component4;
		}
		TextMeshProUGUI component5 = ((Component)((Component)this).transform.Find("rewardbtn1/count")).GetComponent<TextMeshProUGUI>();
		TextMeshProUGUI component6 = ((Component)((Component)this).transform.Find("rewardbtn2/count")).GetComponent<TextMeshProUGUI>();
		if ((Object)(object)component5 != (Object)null && (Object)(object)component6 != (Object)null)
		{
			rewardAmounts[0] = component5;
			rewardAmounts[1] = component6;
		}
		Image component7 = ((Component)((Component)this).transform.Find("rewardbtn1/redpoint")).GetComponent<Image>();
		Image component8 = ((Component)((Component)this).transform.Find("rewardbtn2/redpoint")).GetComponent<Image>();
		if ((Object)(object)component7 != (Object)null && (Object)(object)component8 != (Object)null)
		{
			redPoints[0] = component7;
			redPoints[1] = component8;
		}
		clampedImgs[0] = ((Component)((Component)this).transform.Find("rewardbtn1/clamped")).gameObject;
		clampedImgs[1] = ((Component)((Component)this).transform.Find("rewardbtn2/clamped")).gameObject;
		allRewardBtn = ((Component)((Component)this).transform.Find("allRewardBtn")).GetComponent<Button>();
		mask = ((Component)this).transform.Find("mask");
		allRepoint = ((Component)((Component)this).transform.Find("allredpoint")).gameObject;
	}

	public void Initialize(int[] id, Sprite[] sprite, int[] rewardCount, DailyCheckIn.RewardType[] rType, DailyCheckIn.CheckInType cType, int day)
	{
		this.id = id;
		itemIcons = sprite;
		for (int i = 0; i < rewardCount.Length; i++)
		{
			rewardCounts[i] = rewardCount[i];
		}
		rewardType = rType;
		checkInType = cType;
		dayIndex = day;
		((TMP_Text)title).text = $"第{dayIndex}天";
		RefreshItem();
	}

	public void RefreshItem()
	{
		RefreshRewardActionIcon();
		int worldId = (((Object)(object)DailyCheckIn.Instance != (Object)null) ? DailyCheckIn.Instance.GetCurrentWorldId() : GameDataManager.Instance.GetCurrentWorldId());
		int[] array = GameDataManager.Instance.GetClampItemInfoByWorld(worldId);
		int num = GameDataManager.Instance.GetCheckInDayByWorld(worldId);
		if (num <= 0)
		{
			num = 1;
			GameDataManager.Instance.SetCheckInDayByWorld(worldId, num);
		}
		if (array == null || array.Length < 28)
		{
			array = new int[28];
			GameDataManager.Instance.SetClampItemInfoByWorld(worldId, array);
		}
		if (rewardType[0] == DailyCheckIn.RewardType.Gold)
		{
			realCounts[0] = rewardCounts[0] * SingletonMono<UpgradeManager>.Instance.GetAverageGoldPerMinuteFromLast30Seconds();
		}
		if (rewardType[1] == DailyCheckIn.RewardType.Gold)
		{
			realCounts[1] = rewardCounts[1] * SingletonMono<UpgradeManager>.Instance.GetAverageGoldPerMinuteFromLast30Seconds();
		}
		if (itemIcons.Length != 0)
		{
			for (int i = 0; i < itemImgs.Length; i++)
			{
				itemImgs[i].sprite = itemIcons[i];
				((Graphic)itemImgs[i]).SetNativeSize();
				int num2 = i;
				if (checkInType == DailyCheckIn.CheckInType.Ad)
				{
					num2 += 2;
				}
				if (array[(dayIndex - 1) * 4 + num2] <= 0)
				{
					clampedImgs[i].gameObject.SetActive(false);
					if (num >= dayIndex)
					{
						allRepoint.SetActive(true);
						canClamped = true;
					}
					else
					{
						allRepoint.SetActive(false);
						canClamped = false;
					}
				}
				else
				{
					clampedImgs[i].gameObject.SetActive(true);
					allRepoint.SetActive(false);
					canClamped = false;
				}
			}
		}
		if (rewardCounts.Length != 0)
		{
			for (int j = 0; j < rewardCounts.Length; j++)
			{
				if (rewardType[j] == DailyCheckIn.RewardType.Gold)
				{
					((TMP_Text)rewardAmounts[j]).text = NumberFormatter.FormatNumber(realCounts[j]);
				}
				else
				{
					((TMP_Text)rewardAmounts[j]).text = rewardCounts[j].ToString();
				}
			}
		}
		if ((dayIndex == 3 || dayIndex == 7) && checkInType == DailyCheckIn.CheckInType.Ad)
		{
			int helperId = 1;
			if ((Object)(object)DailyCheckIn.Instance != (Object)null)
			{
				helperId = DailyCheckIn.Instance.GetHelperId(id[1]);
			}
			if (GameDataManager.Instance.IsHelperOwned(helperId))
			{
				itemImgs[1].sprite = itemIcons[1];
				((Graphic)itemImgs[1]).SetNativeSize();
				GameObject gameObject = ((Component)((Component)rewardbtns[1]).transform.Find("huoqv")).gameObject;
				if ((Object)(object)gameObject != (Object)null)
				{
					gameObject.SetActive(false);
				}
			}
			else
			{
				SingletonMono<ResManager>.Instance.LoadSpriteAsync(Constants.GetHelperIconPath(helperId), delegate(Sprite sprite)
				{
					itemImgs[1].sprite = sprite;
					((Graphic)itemImgs[1]).SetNativeSize();
					((TMP_Text)rewardAmounts[1]).text = "1";
				});
				GameObject gameObject2 = ((Component)((Component)rewardbtns[1]).transform.Find("huoqv")).gameObject;
				if ((Object)(object)gameObject2 != (Object)null)
				{
					gameObject2.SetActive(true);
				}
			}
		}
		if (dayIndex > num)
		{
			((Component)mask).gameObject.SetActive(true);
		}
		else
		{
			((Component)mask).gameObject.SetActive(false);
		}
	}

	private void BtnClick()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		((UnityEvent)allRewardBtn.onClick).AddListener((UnityAction)delegate
		{
			if (rewardList != null)
			{
				rewardList.Clear();
			}
			int worldId = (((Object)(object)DailyCheckIn.Instance != (Object)null) ? DailyCheckIn.Instance.GetCurrentWorldId() : GameDataManager.Instance.GetCurrentWorldId());
			int num = GameDataManager.Instance.GetCheckInDayByWorld(worldId);
			int[] array = GameDataManager.Instance.GetClampItemInfoByWorld(worldId);
			if (num <= 0)
			{
				num = 1;
				GameDataManager.Instance.SetCheckInDayByWorld(worldId, num);
			}
			if (array == null || array.Length < 28)
			{
				array = new int[28];
				GameDataManager.Instance.SetClampItemInfoByWorld(worldId, array);
			}
			if (checkInType == DailyCheckIn.CheckInType.Free)
			{
				int num2 = 0;
				if (num >= dayIndex)
				{
					if (array[(dayIndex - 1) * 4 + num2] <= 0)
					{
						array[(dayIndex - 1) * 4 + num2] = 1;
						array[(dayIndex - 1) * 4 + num2 + 1] = 1;
						double num3 = 0.0;
						num3 = ((rewardType[num2] != DailyCheckIn.RewardType.Gold) ? rewardCounts[num2] : realCounts[num2]);
						double num4 = 0.0;
						num4 = ((rewardType[num2 + 1] != DailyCheckIn.RewardType.Gold) ? rewardCounts[num2 + 1] : realCounts[num2 + 1]);
						GetItemFreeAd(num2, num3);
						GetItemFreeAd(num2 + 1, num4);
						UIManager.Instance.ShowRewardBoxUI(rewardList, isAddItem: true, isSuipian: true, "签到双倍奖励");
						BroadcastManager.ShowPopUpTip("领取成功");
						if ((Object)(object)DailyCheckIn.Instance != (Object)null)
						{
							DailyCheckIn.Instance.RefreshUI();
						}
						WorldCupDataManager.Instance.AddWorldCupMissionProgress(0, 1);
						int worldId2 = (((Object)(object)DailyCheckIn.Instance != (Object)null) ? DailyCheckIn.Instance.GetCurrentWorldId() : GameDataManager.Instance.GetCurrentWorldId());
						GameDataManager.Instance.SetClampItemInfoByWorld(worldId2, array);
						GameDataManager.Instance.SaveGameDataOnly();
					}
					else
					{
						BroadcastManager.ShowPopUpTip("道具不能重复领取");
					}
				}
				else
				{
					BroadcastManager.ShowPopUpTip("签到天数不足");
				}
			}
			else
			{
				int index = 0;
				int num5 = 2;
				if (num >= dayIndex)
				{
					if (array[(dayIndex - 1) * 4 + num5] <= 0)
					{
						ClaimAdRewardWithAction(array, index, num5);
					}
					else
					{
						BroadcastManager.ShowPopUpTip("道具不能重复领取");
					}
				}
				else
				{
					BroadcastManager.ShowPopUpTip("签到天数不足");
				}
			}
		});
	}

	private bool UseKungfuMasterSubscribe()
	{
		if ((Object)(object)DailyCheckIn.Instance != (Object)null)
		{
			return DailyCheckIn.Instance.ShouldUseKungfuMasterSubscribe(dayIndex, checkInType);
		}
		return false;
	}

	private void RefreshRewardActionIcon()
	{
		bool flag = UseKungfuMasterSubscribe();
		if ((Object)(object)videoIcon != (Object)null)
		{
			((Component)videoIcon).gameObject.SetActive(!flag);
		}
		if ((Object)(object)subscribeIcon != (Object)null)
		{
			((Component)subscribeIcon).gameObject.SetActive(flag);
		}
	}

	private void ClaimAdRewardWithAction(int[] clampInfo, int index, int index2)
	{
		if (UseKungfuMasterSubscribe())
		{
			TTFeedDirectPlayManager.Instance.ZhuDongRequestKungfuMasterThirdDaySignInFeedSubscribe(delegate
			{
				CompleteAdRewardClaim(clampInfo, index, index2);
			}, delegate
			{
				BroadcastManager.ShowPopUpTip("订阅失败，无法领取奖励");
			});
		}
		else
		{
			FakeVideoManager.Instance.ShowAd(delegate
			{
				CompleteAdRewardClaim(clampInfo, index, index2);
			}, delegate
			{
				BroadcastManager.ShowPopUpTip("广告观看未完成，无法领取奖励");
			}, "7日签到");
		}
	}

	private void CompleteAdRewardClaim(int[] clampInfo, int index, int index2)
	{
		clampInfo[(dayIndex - 1) * 4 + index2] = 1;
		clampInfo[(dayIndex - 1) * 4 + index2 + 1] = 1;
		double num = 0.0;
		num = ((rewardType[index] != DailyCheckIn.RewardType.Gold) ? rewardCounts[index] : realCounts[index]);
		double num2 = 0.0;
		num2 = ((rewardType[index + 1] != DailyCheckIn.RewardType.Gold) ? rewardCounts[index + 1] : realCounts[index + 1]);
		if (dayIndex == 3 || dayIndex == 7)
		{
			int helperId = 1;
			if ((Object)(object)DailyCheckIn.Instance != (Object)null)
			{
				helperId = DailyCheckIn.Instance.GetHelperId(id[1]);
			}
			if (!GameDataManager.Instance.IsHelperOwned(helperId))
			{
				GameDataManager.Instance.SetHelperOwned(helperId, isOwned: true, isADUnlock: true);
				GetItemFreeAd(index, num);
			}
			else
			{
				GetItemFreeAd(index, num);
				GetItemFreeAd(index + 1, num2);
			}
		}
		else
		{
			GetItemFreeAd(index, num);
			GetItemFreeAd(index + 1, num2);
		}
		UIManager.Instance.ShowRewardBoxUI(rewardList, isAddItem: true, isSuipian: true, "签到双倍奖励");
		WorldCupDataManager.Instance.AddWorldCupMissionProgress(0, 1);
		BroadcastManager.ShowPopUpTip("领取成功");
		if ((Object)(object)DailyCheckIn.Instance != (Object)null)
		{
			DailyCheckIn.Instance.RefreshUI();
		}
		int worldId = (((Object)(object)DailyCheckIn.Instance != (Object)null) ? DailyCheckIn.Instance.GetCurrentWorldId() : GameDataManager.Instance.GetCurrentWorldId());
		GameDataManager.Instance.SetClampItemInfoByWorld(worldId, clampInfo);
		GameDataManager.Instance.SaveGameDataOnly();
	}

	public void GetItemFreeAd(int index, double sum)
	{
		if (rewardType[index] == DailyCheckIn.RewardType.Item && (id[index] == 3 || id[index] == 4 || id[index] == 5))
		{
			List<double> list = new List<double>();
			List<double> list2 = new List<double>();
			List<double> list3 = new List<double>();
			for (int i = 0; (double)i < sum; i++)
			{
				int num = ConfigManager.Instance.TransferSuipianRealId(id[index]);
				list.Add(num);
			}
			foreach (double item in list)
			{
				int num2 = (int)item;
				int num3 = list2.IndexOf(num2);
				if (num3 == -1)
				{
					list2.Add(num2);
					list3.Add(1.0);
				}
				else
				{
					list3[num3]++;
				}
			}
			for (int j = 0; j < list2.Count; j++)
			{
				rewardList.Add(list2[j]);
				rewardList.Add(list3[j]);
			}
		}
		else
		{
			rewardList.Add(id[index]);
			rewardList.Add(sum);
		}
	}

	private void OpenADEffect(bool isOpen)
	{
		if (checkInType != DailyCheckIn.CheckInType.Ad)
		{
			return;
		}
		for (int i = 0; i < rewardbtns.Length; i++)
		{
			UISpriteAnimationSequence componentInChildren = ((Component)((Component)rewardbtns[i]).transform).GetComponentInChildren<UISpriteAnimationSequence>();
			if ((Object)(object)componentInChildren != (Object)null)
			{
				((Behaviour)componentInChildren).enabled = isOpen;
			}
		}
	}

	public bool GetIsClamped()
	{
		return canClamped;
	}
}

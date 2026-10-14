using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using cfg;

public class WorldCupItemInfo : MonoBehaviour
{
	private int signDay;

	[Header("物品框1")]
	[SerializeField]
	private RectTransform rewardobj1;

	[Header("物品框1")]
	[SerializeField]
	private RectTransform rewardobj2;

	[Header("奖励1道具图显示")]
	[SerializeField]
	private Image oneItemImg;

	[Header("奖励2道具图显示")]
	[SerializeField]
	private Image twoItemImg;

	[Header("奖励1已领取显示")]
	[SerializeField]
	private Image clamped1;

	[Header("奖励2已领取显示")]
	[SerializeField]
	private Image clamped2;

	[Header("奖励1数量显示")]
	[SerializeField]
	private TextMeshProUGUI oneCount;

	[Header("奖励2数量显示")]
	[SerializeField]
	private TextMeshProUGUI twoCount;

	[Header("未达到条件遮罩")]
	[SerializeField]
	private Image mask;

	[Header("领取按键")]
	[SerializeField]
	private Button rewardBtn;

	[Header("红点")]
	[SerializeField]
	private Image redPoint;

	[Header("特殊人物的背景图")]
	[SerializeField]
	private GameObject tshubg;

	[Header("特殊人物的金色框图")]
	[SerializeField]
	private GameObject jinse;

	private List<double> rewardList = new List<double>();

	private WorldSignType itemType;

	private void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		((UnityEvent)rewardBtn.onClick).AddListener(new UnityAction(BtnGetReward));
	}

	public void Init(int dayIndex, WorldSignType type)
	{
		signDay = dayIndex;
		itemType = type;
		if (type == WorldSignType.free)
		{
			Transform val = ((Component)this).transform.Find("title");
			if ((Object)(object)val != (Object)null)
			{
				((TMP_Text)((Component)val).GetComponent<TextMeshProUGUI>()).text = $"第{signDay}天";
			}
		}
		RefrshItem();
	}

	public void RefrshItem()
	{
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		signWorldCup oneSignWorld = ConfigManager.Instance.GetOneSignWorld(signDay);
		if (itemType == WorldSignType.free)
		{
			if ((Object)(object)WorldCupPanel.Instance != (Object)null)
			{
				rewardList = new List<double>(WorldCupDataManager.Instance.GetSignItemList(signDay, 0));
				bool flag = false;
				string spritePath = WorldCupPanel.Instance.GetSpritePath(oneSignWorld.NormalReward[0]);
				flag = IsTuji(oneSignWorld.NormalReward[0]);
				if (oneSignWorld.NormalReward[0] < 0)
				{
					((Component)rewardobj1).gameObject.SetActive(true);
					((Component)rewardobj2).gameObject.SetActive(false);
					rewardobj1.anchoredPosition = new Vector2(65f, 0f);
					spritePath = Constants.GetHelperIconPath(9);
					tshubg.SetActive(true);
					jinse.SetActive(true);
					SingletonMono<ResManager>.Instance.LoadSpriteAsync(spritePath, delegate(Sprite sprite)
					{
						oneItemImg.sprite = sprite;
						((Graphic)oneItemImg).SetNativeSize();
					});
				}
				else
				{
					SingletonMono<ResManager>.Instance.LoadSpriteAsync(spritePath, delegate(Sprite sprite)
					{
						oneItemImg.sprite = sprite;
					}, flag);
					((Component)rewardobj1).gameObject.SetActive(true);
					((Component)rewardobj2).gameObject.SetActive(true);
					rewardobj1.anchoredPosition = new Vector2(-1f, 0f);
					rewardobj2.anchoredPosition = new Vector2(166f, 0f);
				}
				bool flag2 = false;
				string spritePath2 = WorldCupPanel.Instance.GetSpritePath(oneSignWorld.NormalReward[2]);
				flag2 = IsTuji(oneSignWorld.NormalReward[2]);
				SingletonMono<ResManager>.Instance.LoadSpriteAsync(spritePath2, delegate(Sprite sprite)
				{
					twoItemImg.sprite = sprite;
				}, flag2);
				int num = oneSignWorld.NormalReward[1];
				int num2 = oneSignWorld.NormalReward[3];
				if (num <= 0)
				{
					num = 1;
				}
				if (num2 <= 0)
				{
					num2 = 1;
				}
				((TMP_Text)oneCount).text = num.ToString();
				((TMP_Text)twoCount).text = num2.ToString();
				if (oneSignWorld.NormalReward[0] == 1)
				{
					((TMP_Text)oneCount).text = NumberFormatter.FormatNumber((double)oneSignWorld.NormalReward[1] * SingletonMono<UpgradeManager>.Instance.GetAverageGoldPerMinuteFromLast30Seconds());
				}
				if (oneSignWorld.NormalReward[2] == 1)
				{
					((TMP_Text)twoCount).text = NumberFormatter.FormatNumber((double)oneSignWorld.NormalReward[3] * SingletonMono<UpgradeManager>.Instance.GetAverageGoldPerMinuteFromLast30Seconds());
				}
			}
		}
		else if ((Object)(object)WorldCupPanel.Instance != (Object)null)
		{
			rewardList = new List<double>(WorldCupDataManager.Instance.GetSignItemList(signDay, 1));
			bool flag3 = false;
			string spritePath3 = WorldCupPanel.Instance.GetSpritePath(oneSignWorld.AdReward[0]);
			flag3 = IsTuji(oneSignWorld.AdReward[0]);
			SingletonMono<ResManager>.Instance.LoadSpriteAsync(spritePath3, delegate(Sprite sprite)
			{
				oneItemImg.sprite = sprite;
			}, flag3);
			bool flag4 = false;
			string spritePath4 = WorldCupPanel.Instance.GetSpritePath(oneSignWorld.AdReward[2]);
			flag4 = IsTuji(oneSignWorld.AdReward[2]);
			SingletonMono<ResManager>.Instance.LoadSpriteAsync(spritePath4, delegate(Sprite sprite)
			{
				twoItemImg.sprite = sprite;
			}, flag4);
			int num3 = oneSignWorld.AdReward[1];
			int num4 = oneSignWorld.AdReward[3];
			if (num3 <= 0)
			{
				num3 = 1;
			}
			if (num4 <= 0)
			{
				num4 = 1;
			}
			((TMP_Text)oneCount).text = num3.ToString();
			((TMP_Text)twoCount).text = num4.ToString();
			if (oneSignWorld.AdReward[0] == 1)
			{
				((TMP_Text)oneCount).text = NumberFormatter.FormatNumber((double)oneSignWorld.AdReward[1] * SingletonMono<UpgradeManager>.Instance.GetAverageGoldPerMinuteFromLast30Seconds());
			}
			if (oneSignWorld.AdReward[2] == 1)
			{
				((TMP_Text)twoCount).text = NumberFormatter.FormatNumber((double)oneSignWorld.AdReward[3] * SingletonMono<UpgradeManager>.Instance.GetAverageGoldPerMinuteFromLast30Seconds());
			}
		}
		if (WorldCupDataManager.Instance == null)
		{
			return;
		}
		int rewardDay = WorldCupDataManager.Instance.GetRewardDay();
		((Component)mask).gameObject.SetActive((rewardDay < signDay) ? true : false);
		int num5 = 0;
		num5 = ((itemType != WorldSignType.free) ? ((signDay - 1) * 2 + 1) : ((signDay - 1) * 2));
		((Component)redPoint).gameObject.SetActive(false);
		if (WorldCupDataManager.Instance.GetWorldCupItemClamp(num5) == 0)
		{
			((Component)clamped1).gameObject.SetActive(false);
			((Component)clamped2).gameObject.SetActive(false);
			if (rewardDay >= signDay)
			{
				((Component)redPoint).gameObject.SetActive(true);
			}
			else
			{
				((Component)redPoint).gameObject.SetActive(false);
			}
		}
		else
		{
			((Component)clamped1).gameObject.SetActive(true);
			((Component)clamped2).gameObject.SetActive(true);
			((Component)redPoint).gameObject.SetActive(false);
		}
	}

	public bool IsTuji(int Itemid)
	{
		bool result = false;
		if (Itemid <= 2)
		{
			result = false;
		}
		else if (Itemid > 2 && Itemid <= 109)
		{
			result = true;
			if (Itemid == 6)
			{
				result = false;
			}
		}
		return result;
	}

	public void BtnGetReward()
	{
		if (WorldCupDataManager.Instance == null)
		{
			return;
		}
		int num = 0;
		bool flag = false;
		if (itemType == WorldSignType.free)
		{
			num = (signDay - 1) * 2;
			flag = true;
		}
		else
		{
			num = (signDay - 1) * 2 + 1;
			flag = false;
		}
		bool num2 = WorldCupDataManager.Instance.IsCanClamp(signDay, num);
		signWorldCup oneSignWorld = ConfigManager.Instance.GetOneSignWorld(signDay);
		if (!num2)
		{
			return;
		}
		if (flag)
		{
			if (WorldCupDataManager.Instance != null && (Object)(object)GameDataManager.Instance != (Object)null)
			{
				if (oneSignWorld.NormalReward[0] < 0)
				{
					GameDataManager.Instance.SetHelperOwned(9, isOwned: true);
					BroadcastManager.ShowPopUpTip("已解锁新帮手角色,快去试试吧");
				}
				else
				{
					UIManager.Instance.ShowRewardBoxUI(rewardList, isAddItem: true, isSuipian: true, "世界杯签到免费奖励");
				}
			}
			WorldCupDataManager.Instance.SetWorldCupItemClamp((signDay - 1) * 2, 1);
			WorldCupDataManager.Instance.AddWorldCupMissionProgress(0, 1);
			if ((Object)(object)WorldCupPanel.Instance != (Object)null)
			{
				WorldCupPanel.Instance.RefrashPanel(1);
			}
			return;
		}
		FakeVideoManager.Instance.ShowAd(delegate
		{
			UIManager.Instance.ShowRewardBoxUI(rewardList, isAddItem: true, isSuipian: true, "世界杯签到广告奖励");
			WorldCupDataManager.Instance.SetWorldCupItemClamp((signDay - 1) * 2 + 1, 1);
			WorldCupDataManager.Instance.AddWorldCupMissionProgress(0, 1);
			if ((Object)(object)WorldCupPanel.Instance != (Object)null)
			{
				WorldCupPanel.Instance.RefrashPanel(1);
			}
		}, delegate
		{
		}, "世界杯活动广告");
	}
}

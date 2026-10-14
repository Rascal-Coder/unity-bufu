using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using cfg;

public class WorldCupPanel : MonoBehaviour
{
	public static WorldCupPanel Instance;

	public Button closeBtn;

	[Header("签到面板物体")]
	[SerializeField]
	private GameObject signPanel;

	[Header("任务面板物体")]
	[SerializeField]
	private GameObject missionPanel;

	[Header("签到面板打开按键")]
	[SerializeField]
	private Button signBtn;

	[Header("任务面板打开按键")]
	[SerializeField]
	private Button missionBtn;

	[Header("签到面板免费Content")]
	[SerializeField]
	private Transform freeCont;

	[Header("签到面板广告Content")]
	[SerializeField]
	private Transform adCont;

	[Header("积分进度")]
	[SerializeField]
	private Image scoreProgress;

	[Header("几个积分领取按的父物体")]
	[SerializeField]
	private Transform scoreReward;

	[Header("目前积分显示")]
	[SerializeField]
	private TextMeshProUGUI currentScoreTxt;

	[Header("几个活动任务创建的父物体")]
	[SerializeField]
	private Transform scoreCont;

	[Header("签到面板红点")]
	[SerializeField]
	private Transform signRedPoint;

	[Header("积分面板红点")]
	[SerializeField]
	private Transform missionRedPoint;

	private List<WorldCupItemInfo> iteminfolist;

	private List<WorldMissionItem> worldMissionItemList;

	[SerializeField]
	private Button maskBtn;

	private void Awake()
	{
		if ((Object)(object)Instance != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
		else
		{
			Instance = this;
		}
		iteminfolist = new List<WorldCupItemInfo>();
		worldMissionItemList = new List<WorldMissionItem>();
	}

	private void Start()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected O, but got Unknown
		Init();
		((UnityEvent)closeBtn.onClick).AddListener(new UnityAction(ClosePanel));
		Button obj = maskBtn;
		if (obj != null)
		{
			((UnityEvent)obj.onClick).AddListener(new UnityAction(ClosePanel));
		}
		((UnityEvent)signBtn.onClick).AddListener((UnityAction)delegate
		{
			RefrashPanel(1);
		});
		((UnityEvent)missionBtn.onClick).AddListener((UnityAction)delegate
		{
			RefrashPanel(2);
		});
	}

	private void Init()
	{
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Expected O, but got Unknown
		for (int i = 1; i < 8; i++)
		{
			int index = i;
			SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/NewUI/CheckInWorldCup/prefabs/World_FreeReward.prefab", delegate(GameObject prefab)
			{
				WorldCupItemInfo component3 = Object.Instantiate<GameObject>(prefab, freeCont).GetComponent<WorldCupItemInfo>();
				if ((Object)(object)component3 != (Object)null)
				{
					component3.Init(index, WorldSignType.free);
				}
				iteminfolist.Add(component3);
			});
			SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/NewUI/CheckInWorldCup/prefabs/World_AdReward.prefab", delegate(GameObject prefab)
			{
				WorldCupItemInfo component3 = Object.Instantiate<GameObject>(prefab, adCont).GetComponent<WorldCupItemInfo>();
				if ((Object)(object)component3 != (Object)null)
				{
					component3.Init(index, WorldSignType.ad);
				}
				iteminfolist.Add(component3);
			});
		}
		List<WorldCupMisson> allWorldCupMisson = ConfigManager.Instance.GetAllWorldCupMisson();
		for (int num = 0; num < allWorldCupMisson.Count; num++)
		{
			int missonId = allWorldCupMisson[num].Id;
			SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/NewUI/CheckInWorldCup/prefabs/worldMissionItem.prefab", delegate(GameObject prefab)
			{
				WorldMissionItem component3 = Object.Instantiate<GameObject>(prefab, scoreCont).GetComponent<WorldMissionItem>();
				if ((Object)(object)component3 != (Object)null)
				{
					component3.Init(missonId);
				}
				worldMissionItemList.Add(component3);
			});
		}
		for (int num2 = 0; num2 < scoreReward.childCount; num2++)
		{
			int index2 = num2;
			Button component = ((Component)scoreReward.GetChild(index2)).GetComponent<Button>();
			TextMeshProUGUI component2 = ((Component)scoreReward.GetChild(index2).Find("kuang/txt")).GetComponent<TextMeshProUGUI>();
			WorldUpScoreReward worldCupScoreReward = ConfigManager.Instance.GetWorldCupScoreReward(index2 + 1);
			if ((Object)(object)component2 != (Object)null)
			{
				((TMP_Text)component2).text = worldCupScoreReward.RewardNum.ToString();
			}
			((UnityEvent)component.onClick).AddListener((UnityAction)delegate
			{
				if (GetIsClampScoreReward(index2) && WorldCupDataManager.Instance != null)
				{
					List<double> list = new List<double>();
					WorldUpScoreReward worldCupScoreReward2 = ConfigManager.Instance.GetWorldCupScoreReward(index2 + 1);
					int num3 = int.Parse(worldCupScoreReward2.ExchangRewardId);
					int rewardNum = worldCupScoreReward2.RewardNum;
					if (num3 == 3 || num3 == 4 || num3 == 5)
					{
						list = new List<double>(WorldCupDataManager.Instance.ChuLiSuiJiSuiPian(num3, rewardNum));
						UIManager.Instance.ShowRewardBoxUI(list, isAddItem: true, isSuipian: true, "世界杯活动积分奖励领取");
					}
					else
					{
						list.Add(num3);
						list.Add(rewardNum);
						UIManager.Instance.ShowRewardBoxUI(list, isAddItem: true, isSuipian: true, "世界杯活动积分奖励领取");
					}
					WorldCupDataManager.Instance.SetWorldCupScoreReward(index2, 1);
					RefrashPanel(2);
				}
			});
		}
	}

	public void RefrashPanel(int index)
	{
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		if (index == 1)
		{
			signPanel.SetActive(true);
			missionPanel.SetActive(false);
			((Component)((Component)signBtn).transform.GetChild(0)).gameObject.SetActive(true);
			((Component)((Component)missionBtn).transform.GetChild(0)).gameObject.SetActive(false);
			for (int i = 0; i < iteminfolist.Count; i++)
			{
				iteminfolist[i].RefrshItem();
			}
		}
		else
		{
			signPanel.SetActive(false);
			missionPanel.SetActive(true);
			((Component)((Component)signBtn).transform.GetChild(0)).gameObject.SetActive(false);
			((Component)((Component)missionBtn).transform.GetChild(0)).gameObject.SetActive(true);
			for (int j = 0; j < worldMissionItemList.Count; j++)
			{
				worldMissionItemList[j].Refresh();
			}
			if ((Object)(object)ConfigManager.Instance != (Object)null)
			{
				List<WorldUpScoreReward> allWorldCupScoreReward = ConfigManager.Instance.GetAllWorldCupScoreReward();
				for (int k = 0; k < scoreReward.childCount; k++)
				{
					Image img = ((Component)scoreReward.GetChild(k)).GetComponent<Image>();
					TextMeshProUGUI component = ((Component)scoreReward.GetChild(k).GetChild(0)).GetComponent<TextMeshProUGUI>();
					if ((Object)(object)img != (Object)null)
					{
						int itemId = int.Parse(allWorldCupScoreReward[k].ExchangRewardId);
						string spritePath = GetSpritePath(itemId);
						SingletonMono<ResManager>.Instance.LoadSpriteAsync(spritePath, delegate(Sprite sprite)
						{
							img.sprite = sprite;
						});
					}
					if (Object.op_Implicit((Object)(object)component))
					{
						((TMP_Text)component).text = allWorldCupScoreReward[k].ScoreProgress.ToString();
					}
					int worldScore = WorldCupDataManager.Instance.GetWorldScore();
					int worldCupScoreReward = WorldCupDataManager.Instance.GetWorldCupScoreReward(k);
					Transform val = ((Component)img).transform.Find("duihao");
					Transform val2 = ((Component)img).transform.Find("redpoint");
					if (worldScore >= allWorldCupScoreReward[k].ScoreProgress)
					{
						if (worldCupScoreReward == 0)
						{
							((Graphic)img).color = Color.white;
							if ((Object)(object)val != (Object)null)
							{
								((Component)val).gameObject.SetActive(false);
							}
							if ((Object)(object)val2 != (Object)null)
							{
								((Component)val2).gameObject.SetActive(true);
							}
						}
						else
						{
							((Graphic)img).color = Color32.op_Implicit(new Color32((byte)89, (byte)130, (byte)105, byte.MaxValue));
							if ((Object)(object)val != (Object)null)
							{
								((Component)val).gameObject.SetActive(true);
							}
							if ((Object)(object)val2 != (Object)null)
							{
								((Component)val2).gameObject.SetActive(false);
							}
						}
					}
					else
					{
						((Graphic)img).color = Color.white;
						((Graphic)img).color = Color32.op_Implicit(new Color32((byte)89, (byte)130, (byte)105, byte.MaxValue));
						if ((Object)(object)val != (Object)null)
						{
							((Component)val).gameObject.SetActive(false);
						}
						if ((Object)(object)val2 != (Object)null)
						{
							((Component)val2).gameObject.SetActive(false);
						}
					}
				}
			}
			int worldScore2 = WorldCupDataManager.Instance.GetWorldScore();
			scoreProgress.fillAmount = (float)worldScore2 / 100f;
			((TMP_Text)currentScoreTxt).text = worldScore2.ToString();
		}
		((Component)this).gameObject.SetActive(true);
		if (WorldCupDataManager.Instance.GetSignPanelRedPoint())
		{
			((Component)signRedPoint).gameObject.SetActive(true);
		}
		else
		{
			((Component)signRedPoint).gameObject.SetActive(false);
		}
		if (WorldCupDataManager.Instance.GetMissionPanelRedPoint())
		{
			((Component)missionRedPoint).gameObject.SetActive(true);
		}
		else
		{
			((Component)missionRedPoint).gameObject.SetActive(false);
		}
		if (WorldCupDataManager.Instance != null)
		{
			WorldCupDataManager.Instance.IsOpenTopRedPoint();
		}
	}

	public void ClosePanel()
	{
		UIManager.Instance.ShowTopBtn();
		UIManager.Instance.ShowDownItem();
		UIManager.Instance.HideShopTurntableUI();
		UIManager.Instance.topPanel?.DisplayMain();
		((Component)this).gameObject.SetActive(false);
	}

	public string GetSpritePath(int itemId)
	{
		string moneyPath = Constants.GetMoneyPath(1);
		if (itemId < 0)
		{
			_ = -1;
			return moneyPath;
		}
		if (itemId > 0 && itemId <= 2)
		{
			return Constants.GetMoneyPath(itemId);
		}
		if (itemId > 2 && itemId <= 109)
		{
			return Constants.GetSuipianIconPath(itemId);
		}
		return moneyPath;
	}

	public bool GetIsClampScoreReward(int index)
	{
		bool flag = false;
		List<WorldUpScoreReward> allWorldCupScoreReward = ConfigManager.Instance.GetAllWorldCupScoreReward();
		int worldScore = WorldCupDataManager.Instance.GetWorldScore();
		int worldCupScoreReward = WorldCupDataManager.Instance.GetWorldCupScoreReward(index);
		if (worldScore >= allWorldCupScoreReward[index].ScoreProgress)
		{
			if (worldCupScoreReward == 0)
			{
				return true;
			}
			return false;
		}
		return false;
	}
}

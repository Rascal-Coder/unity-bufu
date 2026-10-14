using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using cfg;

public class AuctionSeclect : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static UnityAction _003C_003E9__18_2;

		public static Comparison<auction> _003C_003E9__18_6;

		public static Action _003C_003E9__20_1;

		public static Comparison<auction> _003C_003E9__33_0;

		internal void _003CAwake_003Eb__18_2()
		{
			SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
			UIManager.Instance?.topPanel.ShowSetUpPanel();
		}

		internal int _003CAwake_003Eb__18_6(auction a, auction b)
		{
			return a.ConditionLevel.CompareTo(b.ConditionLevel);
		}

		internal void _003COnEnterBtnClick_003Eb__20_1()
		{
		}

		internal int _003CCalculateAuctionNeedCoin_003Eb__33_0(auction a, auction b)
		{
			return a.ConditionLevel.CompareTo(b.ConditionLevel);
		}
	}

	[Header("关卡存放父物体")]
	[SerializeField]
	private RectTransform cont;

	[Header("移动关卡控制脚本")]
	[SerializeField]
	private AuctionGrid grid;

	[Header("进入拍卖行按键")]
	[SerializeField]
	private Button enterBtn;

	[Header("关卡名称icon")]
	[SerializeField]
	private Image nameImg;

	[Header("拍卖行退出按键")]
	[SerializeField]
	private Button backBtn;

	[Header("拍卖场金钱文本")]
	[SerializeField]
	private TextMeshProUGUI auctionCoinTxt;

	[Header("拍卖场门票文本")]
	[SerializeField]
	private TextMeshProUGUI ticketTxt;

	[Header("拍卖行退出按键")]
	[SerializeField]
	private Button setupBtn;

	[Header("拍卖场进入需要的拍卖金钱")]
	[SerializeField]
	private TextMeshProUGUI needCoin;

	[Header("还有多久时间获取门票文本")]
	[SerializeField]
	private TextMeshProUGUI shijianTxt;

	[Header("门票获取显示物体")]
	[SerializeField]
	private GameObject shijian;

	[Header("拍卖行进入交换场景按键")]
	[SerializeField]
	private Button enterExchangeBtn;

	[Header("交换场景退出按键")]
	[SerializeField]
	private Button exchangeBackBtn;

	[Header("交换场景物体")]
	[SerializeField]
	private GameObject auctionExchangeObj;

	[Header("进入看广告获取拍卖金钱按键")]
	[SerializeField]
	private Button adEnterExchangeBtn;

	[Header("广告icon")]
	[SerializeField]
	private Image adImg;

	private GameObject auctionMatchPanel;

	private List<AuctionSelectItem> selectItemList = new List<AuctionSelectItem>();

	private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	private int _recoverMinute = -1;

	private string _lastTimeStr = string.Empty;

	private float _timer;

	private void Awake()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Expected O, but got Unknown
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Expected O, but got Unknown
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Expected O, but got Unknown
		grid.Init();
		InitSelectItem();
		if ((Object)(object)enterBtn != (Object)null)
		{
			((UnityEvent)enterBtn.onClick).AddListener((UnityAction)delegate
			{
				SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
				OnEnterBtnClick();
			});
		}
		if ((Object)(object)backBtn != (Object)null)
		{
			((UnityEvent)backBtn.onClick).AddListener((UnityAction)delegate
			{
				SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
				((Component)this).gameObject.SetActive(false);
				if ((Object)(object)SoundManager.Instance != (Object)null)
				{
					SoundManager.Instance.EnableCoinSound = true;
				}
				AuctionDataManager.Instance?.SetAuctionOpen(isOpen: false);
				ChallengeDataManager.Instance?.challengePanel.RefreshAuctionEnter();
			});
		}
		if ((Object)(object)setupBtn != (Object)null)
		{
			ButtonClickedEvent onClick = setupBtn.onClick;
			object obj = _003C_003Ec._003C_003E9__18_2;
			if (obj == null)
			{
				UnityAction val = delegate
				{
					SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
					UIManager.Instance?.topPanel.ShowSetUpPanel();
				};
				_003C_003Ec._003C_003E9__18_2 = val;
				obj = (object)val;
			}
			((UnityEvent)onClick).AddListener((UnityAction)obj);
		}
		if ((Object)(object)enterExchangeBtn != (Object)null)
		{
			((UnityEvent)enterExchangeBtn.onClick).AddListener((UnityAction)delegate
			{
				SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
				auctionExchangeObj.SetActive(true);
			});
		}
		if ((Object)(object)exchangeBackBtn != (Object)null)
		{
			((UnityEvent)exchangeBackBtn.onClick).AddListener((UnityAction)delegate
			{
				SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
				auctionExchangeObj.SetActive(false);
			});
		}
		if (!((Object)(object)adEnterExchangeBtn != (Object)null))
		{
			return;
		}
		((UnityEvent)adEnterExchangeBtn.onClick).AddListener((UnityAction)delegate
		{
			SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
			double num = 0.0;
			Common common = ConfigManager.Instance?.GetCommonConfig(53);
			float num2 = AuctionDataManager.Instance?.GetCurrentLevelWithProgress() ?? 0f;
			List<auction> allAuction = ConfigManager.Instance.GetAllAuction();
			allAuction.Sort((auction a, auction b) => a.ConditionLevel.CompareTo(b.ConditionLevel));
			auction auction = null;
			auction auction2 = null;
			for (int num3 = 0; num3 < allAuction.Count; num3++)
			{
				if (!(allAuction[num3].ConditionLevel <= num2))
				{
					auction2 = allAuction[num3];
					break;
				}
				auction = allAuction[num3];
			}
			if (auction != null && auction2 != null && common != null)
			{
				float num4 = auction2.ConditionLevel - auction.ConditionLevel;
				if (num4 > 0f)
				{
					Mathf.Clamp01((num2 - auction.ConditionLevel) / num4);
					num = 2000.0;
				}
			}
			else if (auction != null && common != null)
			{
				num = 2000.0;
			}
			else
			{
				int num5 = AuctionDataManager.Instance?.GetUnlockAuctionId() ?? 1;
				num5++;
				auction auctionConfig = ConfigManager.Instance.GetAuctionConfig(num5);
				if (common != null && auctionConfig != null)
				{
					num = 2000.0;
				}
			}
			int valueOrDefault = (GameDataManager.Instance?.GetGameData()?.adEnterExchangeCount).GetValueOrDefault();
			num = 2000 + valueOrDefault * 1000;
			UIManager.Instance?.ShowSupportUI(DialogType.acutionMoney, num, delegate
			{
				GameData gameData = GameDataManager.Instance?.GetGameData();
				if (gameData != null)
				{
					gameData.adEnterExchangeCount++;
					GameDataManager.Instance?.SaveGameDataOnly();
				}
				RefreshPanel();
			});
		});
	}

	private void OnDestroy()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		if ((Object)(object)enterBtn != (Object)null)
		{
			((UnityEvent)enterBtn.onClick).RemoveListener(new UnityAction(OnEnterBtnClick));
		}
	}

	private void OnEnterBtnClick()
	{
		int num = (AuctionDataManager.Instance?.GetCurrentSelect() ?? 0) + 1;
		AuctionDataManager instance = AuctionDataManager.Instance;
		if (instance != null && instance.GetIsCanAuction(num))
		{
			auction auctionConfig = ConfigManager.Instance.GetAuctionConfig(num);
			double num2 = AuctionDataManager.Instance?.GetAuctionCoin() ?? 0.0;
			double num3 = CalculateAuctionNeedCoin(num);
			if (auctionConfig != null && num2 < num3)
			{
				BroadcastManager.ShowPopUpTip("拍卖金钱不足，需要" + NumberFormatter.FormatNumber(num3));
				return;
			}
			AuctionDataManager instance2 = AuctionDataManager.Instance;
			if (instance2 == null || !instance2.ConsumeTicket())
			{
				FakeVideoManager.Instance.ShowAd(delegate
				{
					EnterMatchScene();
				}, delegate
				{
				}, "拍卖门票");
			}
			else
			{
				EnterMatchScene();
			}
		}
		else
		{
			BroadcastManager.ShowPopUpTip("此拍卖场还未解锁");
		}
	}

	private void InitSelectItem()
	{
		List<auction> auctionConfigs = ConfigManager.Instance.GetAllAuction();
		if (auctionConfigs == null || auctionConfigs.Count == 0)
		{
			Debug.LogError((object)"AuctionSeclect: 没有读取到 auction 配置表数据");
			return;
		}
		string prefabPath = "Assets/GameRes/NewUI/Auction/prefabs/selectSelectItem.prefab";
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(prefabPath, delegate(GameObject prefab)
		{
			if ((Object)(object)prefab == (Object)null)
			{
				Debug.LogError((object)("AuctionSeclect: 加载预制体失败，路径 " + prefabPath));
				return;
			}
			foreach (auction item in auctionConfigs)
			{
				GameObject obj = Object.Instantiate<GameObject>(prefab, (Transform)(object)cont);
				obj.SetActive(true);
				AuctionSelectItem component = obj.GetComponent<AuctionSelectItem>();
				if ((Object)(object)component != (Object)null)
				{
					component.Init(item.AuctionHouseID, grid);
					selectItemList.Add(component);
				}
			}
		});
		MoveToMaxLevel();
	}

	public void RefreshPanel()
	{
		int num = AuctionDataManager.Instance?.GetCurrentSelect() ?? 0;
		num++;
		auction auctionConfig = ConfigManager.Instance.GetAuctionConfig(num);
		if (((Object)this).name != null && auctionConfig != null)
		{
			SingletonMono<ResManager>.Instance.LoadSpriteAsync($"Assets/GameRes/icon/auction_title/challenge_pic_title_{auctionConfig.AuctionHouseID}.png", delegate(Sprite sprite)
			{
				nameImg.sprite = sprite;
			}, isAtlas: false);
			double value = CalculateAuctionNeedCoin(num);
			((TMP_Text)needCoin).text = NumberFormatter.FormatNumber(value);
		}
		if (selectItemList != null)
		{
			for (int num2 = 0; num2 < selectItemList.Count; num2++)
			{
				selectItemList[num2].Refresh();
			}
		}
		RefrshAuctionTxt();
	}

	public void MoveToMaxLevel()
	{
		List<auction> allAuction = ConfigManager.Instance.GetAllAuction();
		int num = GameDataManager.Instance?.GetCurrentMainMissionLevel() ?? 0;
		int num2 = 0;
		foreach (auction item in allAuction)
		{
			if ((float)num >= item.ConditionLevel)
			{
				num2++;
				continue;
			}
			break;
		}
		num2--;
		if (num2 < 0)
		{
			num2 = 0;
		}
		double num3 = AuctionDataManager.Instance?.GetAuctionCoin() ?? 0.0;
		int count = 0;
		for (int num4 = num2; num4 >= 0; num4--)
		{
			int auctionId = num4 + 1;
			double num5 = CalculateAuctionNeedCoin(auctionId);
			if (num3 >= num5)
			{
				count = num4;
				break;
			}
		}
		grid?.MoveTargetPosX(count);
	}

	public void EnterMatchScene()
	{
		if ((Object)(object)auctionMatchPanel == (Object)null)
		{
			SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/NewUI/Auction/prefabs/aunctionMatchPanel.prefab", delegate(GameObject prefab)
			{
				Transform layerTransform = UIManager.Instance.GetLayerTransform(UILayer.Main);
				auctionMatchPanel = Object.Instantiate<GameObject>(prefab, layerTransform);
			});
		}
		else
		{
			auctionMatchPanel.SetActive(true);
			auctionMatchPanel.GetComponent<AuctionMatchPanel>()?.ResetPanelInfo();
		}
		((Component)this).gameObject.SetActive(false);
	}

	public void RefrshAuctionTxt()
	{
		((TMP_Text)auctionCoinTxt).text = NumberFormatter.FormatNumber(AuctionDataManager.Instance?.GetAuctionCoin() ?? 0.0);
		((TMP_Text)ticketTxt).text = NumberFormatter.FormatNumber(AuctionDataManager.Instance?.GetTicketCount() ?? 0);
		RefreshTicketRecoverTime();
		if ((Object)(object)adImg != (Object)null)
		{
			int num = AuctionDataManager.Instance?.GetTicketCount() ?? 0;
			((Component)adImg).gameObject.SetActive(num <= 0);
		}
	}

	private void Update()
	{
		_timer += Time.deltaTime;
		if (_timer >= 1f)
		{
			_timer -= 1f;
			RefreshTicketRecoverTime();
		}
	}

	private void RefreshTicketRecoverTime()
	{
		if ((Object)(object)shijianTxt == (Object)null)
		{
			return;
		}
		if ((AuctionDataManager.Instance?.GetTicketCount() ?? 0) >= 3)
		{
			SetTimeText("--:--");
			if (shijian.activeSelf)
			{
				shijian.SetActive(false);
			}
			return;
		}
		GameData gameData = GameDataManager.Instance?.GetGameData();
		if (gameData == null || gameData.lastTicketRecoverTime <= 0)
		{
			SetTimeText("--:--");
			if (shijian.activeSelf)
			{
				shijian.SetActive(false);
			}
			return;
		}
		if (!shijian.activeSelf)
		{
			shijian.SetActive(true);
		}
		if (_recoverMinute < 0)
		{
			_recoverMinute = ConfigManager.Instance.GetAuctionConfig(1)?.TimeRecover ?? 10;
		}
		long num = (long)(DateTime.Now - _epoch).TotalMilliseconds - gameData.lastTicketRecoverTime;
		long num2 = (long)_recoverMinute * 60L * 1000 - num;
		if (num2 <= 0)
		{
			SetTimeText("00:00");
			return;
		}
		int num3 = (int)(num2 / 1000);
		int num4 = num3 / 60;
		int num5 = num3 % 60;
		SetTimeText($"{num4:00}:{num5:00}");
	}

	private void SetTimeText(string text)
	{
		if (!(_lastTimeStr == text))
		{
			_lastTimeStr = text;
			((TMP_Text)shijianTxt).text = text;
		}
	}

	private double CalculateAuctionNeedCoin(int auctionId)
	{
		List<auction> allAuction = ConfigManager.Instance.GetAllAuction();
		if (allAuction == null || allAuction.Count == 0)
		{
			return 0.0;
		}
		allAuction.Sort((auction a, auction b) => a.ConditionLevel.CompareTo(b.ConditionLevel));
		auction auction = null;
		int num = -1;
		for (int num2 = 0; num2 < allAuction.Count; num2++)
		{
			if (allAuction[num2].AuctionHouseID == auctionId)
			{
				auction = allAuction[num2];
				num = num2;
				break;
			}
		}
		if (auction == null)
		{
			return 0.0;
		}
		float num3 = AuctionDataManager.Instance?.GetCurrentLevelWithProgress() ?? 0f;
		if (num + 1 < allAuction.Count)
		{
			float conditionLevel = allAuction[num + 1].ConditionLevel;
			if (num3 >= conditionLevel)
			{
				return auction.ItemValueMax;
			}
			float num4 = conditionLevel - auction.ConditionLevel;
			if (num4 > 0f)
			{
				float num5 = (num3 - auction.ConditionLevel) / num4;
				num5 = Mathf.Clamp01(num5);
				return (float)auction.ItemValueMin + (float)(auction.ItemValueMax - auction.ItemValueMin) * num5;
			}
		}
		else if (num3 >= auction.ConditionLevel)
		{
			return auction.ItemValueMax;
		}
		return auction.ItemValueMin;
	}
}

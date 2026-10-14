using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Module.Http;
using P8SDKTT;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using cfg;

public class AuctionPlayPanel : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static Action _003C_003E9__118_7;

		public static UnityAction _003C_003E9__118_4;

		internal void _003CStart_003Eb__118_7()
		{
		}

		internal void _003CStart_003Eb__118_4()
		{
			SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
			UIManager.Instance?.topPanel.ShowSetUpPanel();
		}
	}

	[Header("竞拍物品箱子的创建父物体")]
	[SerializeField]
	private Transform itemBoxList;

	[Header("npc的目标位置")]
	[SerializeField]
	private Transform[] npcPos;

	[Header("竞拍物品箱子的目标位置")]
	[SerializeField]
	private Transform[] itemBoxPos;

	[Header("竞拍物品的目标位置")]
	[SerializeField]
	private Transform[] itemPos;

	[Header("集装箱物体")]
	[SerializeField]
	private RectTransform jizhuangxiang;

	[Header("集装箱物体icon")]
	[SerializeField]
	private Image jizhuangxiangImg;

	[Header("集装箱半扇门icon")]
	[SerializeField]
	private Image banshanImg;

	[Header("集装箱全开icon")]
	[SerializeField]
	private Image jzxOpenImg;

	[Header("主持人,npc,玩家说话框")]
	[SerializeField]
	private Image[] speakImg;

	[Header("主持人,npc,玩家说话文本")]
	[SerializeField]
	private TextMeshProUGUI[] speakTxt;

	[Header("玩家预估价格显示")]
	[SerializeField]
	private TextMeshProUGUI yuguTxt;

	[Header("玩家三个出价按键")]
	[SerializeField]
	private Button[] bidBtn;

	[Header("玩家三个出价文本显示")]
	[SerializeField]
	private TextMeshProUGUI[] bidTxt;

	[Header("NPC喊话间隔最小时间设置")]
	[SerializeField]
	private float bidTimeMin;

	[Header("NPC喊话间隔最大时间设置")]
	[SerializeField]
	private float bidTimeMax;

	[Header("结算面板物体")]
	[SerializeField]
	private GameObject auctionCalculatPanel;

	[Header("结算总价文本")]
	[SerializeField]
	private TextMeshProUGUI zongjiaTxt;

	[Header("盈利icon")]
	[SerializeField]
	private Image yingliImg;

	[Header("盈利文本")]
	[SerializeField]
	private TextMeshProUGUI yingliTxt;

	[Header("竞拍结束面板物体")]
	[SerializeField]
	private GameObject auctionEndPanel;

	[Header("普通卖出物品按键")]
	[SerializeField]
	private Button freeSoldBtn;

	[Header("广告卖出物品按键")]
	[SerializeField]
	private Button adSoldBtn;

	[Header("拍卖场金钱文本")]
	[SerializeField]
	private TextMeshProUGUI auctionCoinTxt;

	[Header("拍卖场门票文本")]
	[SerializeField]
	private TextMeshProUGUI ticketTxt;

	[Header("拍卖行退出按键")]
	[SerializeField]
	private Button setupBtn;

	[Header("场景中三个npc背景图片")]
	[SerializeField]
	private RectTransform[] npcs;

	[Header("场景中主持人背景图片")]
	[SerializeField]
	private Transform hostImg;

	[Header("场景中玩家image")]
	[SerializeField]
	private Image auctionPlayer;

	[Header("幕布父物体")]
	[SerializeField]
	private GameObject curtain;

	[Header("幕布动画")]
	[SerializeField]
	private SkeletonGraphic mubuAni;

	[Header("箱子开启画面")]
	[SerializeField]
	private GameObject jzxOpen;

	[Header("最终结算面板")]
	[SerializeField]
	private GameObject EndPanel;

	[Header("关卡岛屿img")]
	[SerializeField]
	private Image daoImg;

	[Header("关卡名字img")]
	[SerializeField]
	private Image selectImg;

	[Header("拍卖盈利文本")]
	[SerializeField]
	private TextMeshProUGUI paimai_yingli;

	[Header("拍卖抬价文本")]
	[SerializeField]
	private TextMeshProUGUI taijia_yingli;

	[Header("拍卖事件文本")]
	[SerializeField]
	private TextMeshProUGUI shijian_yingli;

	[Header("拍卖其他收益文本")]
	[SerializeField]
	private TextMeshProUGUI qita_yingli;

	[Header("拍卖总收益文本")]
	[SerializeField]
	private TextMeshProUGUI zong_yingli;

	[Header("返回按键")]
	[SerializeField]
	private Button backBtn;

	[Header("再来一局")]
	[SerializeField]
	private Button continueBtn;

	[Header("NPC表情图片数组")]
	[SerializeField]
	private Sprite[] emojiSprites;

	[Header("NPC放弃跟价表情（在预制体中拖拽引用）")]
	[SerializeField]
	private Sprite npcGiveUpEmojiSprite;

	[Header("NPC愤怒表情（在预制体中拖拽引用）")]
	[SerializeField]
	private Sprite npcAngryEmojiSprite;

	[Header("下方downpanel显示的物体")]
	[SerializeField]
	private GameObject downpanel;

	[Header("六个物品箱子开箱特效")]
	public SkeletonGraphic[] smokeEffects;

	[Header("集装箱中三个箱子icon")]
	public Image[] boxs;

	[Header("进入结算面板按键")]
	public Button enterEndBtn;

	[Header("抬价物体")]
	[SerializeField]
	private GameObject bidding;

	[Header("速度线")]
	[SerializeField]
	private GameObject[] speedLines;

	[Header("抬价进度")]
	[SerializeField]
	private Image processing;

	[Header("玩家的标识")]
	[SerializeField]
	private GameObject biaoshi;

	[Header("手指指引点击特效")]
	[SerializeField]
	private SkeletonGraphic fingerGuide;

	[Header("结算时灯光")]
	[SerializeField]
	private GameObject dengguang;

	[Header("抬价阶段进度文本显示")]
	private auction auctionInfo;

	private List<float> currentRaiseStages;

	private int selectId;

	private float jizhuangxiangMoveSpeed = 1200f;

	[Header("箱子统一缩放大小")]
	[SerializeField]
	private float boxScale = 1f;

	[Header("物品开箱旋转圈数")]
	[SerializeField]
	private float itemRotateCount = 2f;

	[Header("物品飞向目标时长")]
	[SerializeField]
	private float itemFlyDuration = 1f;

	[Header("箱子落地Y轴偏移")]
	[SerializeField]
	private float boxPosYOffset = -200f;

	private int auctionState;

	private float stateJiShi;

	private float[] speakImgJishi;

	private float realPrice;

	private float realsuiji;

	private float qipaiPrice;

	private float sourceQipaiPrice;

	private float yuguShowRandom;

	private float yuguPriceLow;

	private float yuguPriceHigh;

	private int npcBidCount;

	private int npcBidMin;

	private int npcBidMax;

	private float currentBidTime;

	private float npcFangqi;

	private int bidNpcIndex;

	private bool[] npcHasGivenUp;

	private int lastGiveUpRound;

	private bool angerRollCompleted;

	private int angryNpcIndex;

	private int angryRoundsRemaining;

	private int angryRoundsTotal;

	private float hostBidTime;

	private int hostBidReminderIndex;

	private Button[] itemBoxs;

	private AuctionItem[] items;

	private bool[] isGetItemPos;

	private bool isAcutionSuccess;

	private int openBoxCount;

	private float itemCalculatPirce;

	private float yingliPrice;

	private float tuijiaPrice;

	private float shijianPrice;

	private float otehrPrice;

	private float zongPrice;

	private int npcSuiji;

	private bool isBiddingMode;

	private float speedLineTimer;

	private int speedLineIndex;

	private bool hasCollectedTuijia;

	private float currentNpcOfferRate;

	private float compareNpcOfferRate;

	private const float SPEED_LINE_INTERVAL = 0.1f;

	private Image[] npcEmojiImgs;

	private float[] npcEmojiJishi;

	private Image[] npcSpeakImgs;

	private TextMeshProUGUI[] npcSpeakTxts;

	private float[] npcSpeakJishi;

	private Image[] npcSpecialEmojiImgs;

	private static readonly string[] NPC_GIVE_UP_TEXTS = new string[4] { "算了不争了", "这价不值得", "我先退场了", "留给你吧" };

	private static readonly string[] NPC_ANGER_TEXTS = new string[5] { "跟你到底", "我偏要争", "谁也别想要", "这价我跟定", "今天杠上了" };

	private float speakJellyStartScale = 1.3f;

	private float speakJellyDuration = 0.35f;

	private float speakJellyBounceIntensity = 0.25f;

	private const string PRICE_COLOR = "#185890";

	private bool IsRatioAboveMinStage(float ratio)
	{
		if (currentRaiseStages == null || currentRaiseStages.Count == 0)
		{
			return true;
		}
		float num = currentRaiseStages[0];
		for (int i = 1; i < currentRaiseStages.Count; i++)
		{
			if (currentRaiseStages[i] < num)
			{
				num = currentRaiseStages[i];
			}
		}
		return ratio >= num;
	}

	private void Start()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Expected O, but got Unknown
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Expected O, but got Unknown
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Expected O, but got Unknown
		openBoxCount = 0;
		isAcutionSuccess = false;
		hostBidReminderIndex = 0;
		bidNpcIndex = -1;
		speakImgJishi = new float[3];
		if (itemBoxPos != null)
		{
			isGetItemPos = new bool[itemBoxPos.Length];
		}
		InitNpcSpeakBubbles();
		InitNpcEmojis();
		((UnityEvent)freeSoldBtn.onClick).AddListener((UnityAction)delegate
		{
			SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
			double num = AuctionDataManager.Instance?.GetAuctionCoin() ?? 0.0;
			yingliPrice = itemCalculatPirce - qipaiPrice;
			zongPrice = yingliPrice + tuijiaPrice + shijianPrice + otehrPrice;
			double num2 = num + (double)zongPrice;
			if (num2 <= 0.0)
			{
				num2 = 0.0;
			}
			AuctionDataManager.Instance?.SetAuctionCoin(num2);
			RefreshCoinTxt();
			OpenEndPanel();
		});
		((UnityEvent)adSoldBtn.onClick).AddListener((UnityAction)delegate
		{
			SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
			FakeVideoManager.Instance.ShowAd(delegate
			{
				double num = AuctionDataManager.Instance?.GetAuctionCoin() ?? 0.0;
				yingliPrice = itemCalculatPirce * 2f - qipaiPrice;
				zongPrice = yingliPrice + tuijiaPrice + shijianPrice + otehrPrice;
				double num2 = num + (double)zongPrice;
				if (num2 <= 0.0)
				{
					num2 = 0.0;
				}
				AuctionDataManager.Instance?.SetAuctionCoin(num2);
				RefreshCoinTxt();
				OpenEndPanel();
			}, delegate
			{
			}, "拍卖双倍卖出");
		});
		((UnityEvent)enterEndBtn.onClick).AddListener((UnityAction)delegate
		{
			SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
			AuctionDataManager.Instance?.GetAuctionCoin();
			yingliPrice = 0f;
			zongPrice = yingliPrice + tuijiaPrice + shijianPrice + otehrPrice;
			RefreshCoinTxt();
			OpenEndPanel();
		});
		((UnityEvent)backBtn.onClick).AddListener((UnityAction)delegate
		{
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
			((Component)this).gameObject.SetActive(false);
			auctionCalculatPanel.SetActive(false);
			auctionEndPanel.SetActive(false);
			AuctionEndPanel(isAcutionSuccess, isOpen: false);
			EndPanel.SetActive(false);
			P8SDK.LevelUpgrade(new LevelUpgradeOption
			{
				level = "AUCTION",
				level_status = "1"
			});
			try
			{
				SingletonMono<HttpManager>.Instance.TrackSwitchUI(null, "panel_ auction_end");
			}
			catch (Exception)
			{
				throw;
			}
			AuctionDataManager.Instance?.OpenAuctionObj();
		});
		if ((Object)(object)setupBtn != (Object)null)
		{
			ButtonClickedEvent onClick = setupBtn.onClick;
			object obj = _003C_003Ec._003C_003E9__118_4;
			if (obj == null)
			{
				UnityAction val = delegate
				{
					SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
					UIManager.Instance?.topPanel.ShowSetUpPanel();
				};
				_003C_003Ec._003C_003E9__118_4 = val;
				obj = (object)val;
			}
			((UnityEvent)onClick).AddListener((UnityAction)obj);
		}
		if ((Object)(object)continueBtn != (Object)null)
		{
			((UnityEvent)continueBtn.onClick).AddListener((UnityAction)delegate
			{
				SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
				if ((AuctionDataManager.Instance?.GetTicketCount() ?? 0) <= 0)
				{
					BroadcastManager.ShowPopUpTip("门票不足,请返回");
				}
				else
				{
					double num = AuctionDataManager.Instance?.GetAuctionCoin() ?? 0.0;
					if (auctionInfo != null && num < (double)auctionInfo.ItemValueMin)
					{
						BroadcastManager.ShowPopUpTip("拍卖金钱不足,请返回");
					}
					else
					{
						AuctionDataManager.Instance?.ConsumeTicket();
						AuctionSeclect auctionSelect = AuctionDataManager.Instance.auctionSelect;
						if ((Object)(object)auctionSelect != (Object)null)
						{
							auctionSelect.EnterMatchScene();
						}
						((Component)this).gameObject.SetActive(false);
						auctionCalculatPanel.SetActive(false);
						auctionEndPanel.SetActive(false);
						AuctionEndPanel(isAcutionSuccess, isOpen: false);
						EndPanel.SetActive(false);
					}
				}
			});
		}
		fingerGuide.AnimationState.SetAnimation(0, "dianji", true);
		ResetSceneInfo();
	}

	private void Update()
	{
		AuctionProgress();
	}

	public void ResetSceneInfo()
	{
		//IL_053b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0545: Expected O, but got Unknown
		//IL_0806: Unknown result type (might be due to invalid IL or missing references)
		//IL_0824: Unknown result type (might be due to invalid IL or missing references)
		isAcutionSuccess = false;
		itemCalculatPirce = 0f;
		yingliPrice = 0f;
		tuijiaPrice = 0f;
		auctionState = 0;
		openBoxCount = 0;
		stateJiShi = 0f;
		isBiddingMode = false;
		speedLineTimer = 0f;
		speedLineIndex = 0;
		hasCollectedTuijia = false;
		if (AuctionDataManager.Instance != null)
		{
			if (AuctionDataManager.Instance.GetEnterAuctionCount() < 2)
			{
				AuctionDataManager.Instance.AddEnterAuctionCount();
				yuguShowRandom = 1f;
			}
			else
			{
				yuguShowRandom = Random.value;
			}
		}
		if ((Object)(object)bidding != (Object)null)
		{
			bidding.SetActive(false);
		}
		if (speedLines != null)
		{
			for (int i = 0; i < speedLines.Length; i++)
			{
				if ((Object)(object)speedLines[i] != (Object)null)
				{
					speedLines[i].SetActive(false);
				}
			}
		}
		if ((Object)(object)fingerGuide != (Object)null)
		{
			((Component)fingerGuide).gameObject.SetActive(false);
		}
		selectId = AuctionDataManager.Instance?.GetCurrentSelect() ?? 0;
		selectId++;
		SetItemPrice();
		ClearDownPanel(isPlayerBid: false);
		npcBidCount = 0;
		lastGiveUpRound = 0;
		Transform[] array = npcPos;
		npcHasGivenUp = new bool[(array != null) ? array.Length : 0];
		angerRollCompleted = false;
		angryNpcIndex = -1;
		angryRoundsRemaining = 0;
		angryRoundsTotal = 0;
		auctionInfo = ConfigManager.Instance.GetAuctionConfig(selectId);
		if (auctionInfo != null)
		{
			if (auctionInfo.AuctionRoundMin.Count > 1)
			{
				npcBidMin = Random.Range(auctionInfo.AuctionRoundMin[0], auctionInfo.AuctionRoundMin[1] + 1);
			}
			if (auctionInfo.AuctionRoundMax.Count > 1)
			{
				npcBidMax = Random.Range(auctionInfo.AuctionRoundMax[0], auctionInfo.AuctionRoundMax[1] + 1);
			}
			Debug.Log((object)$"本局最小喊价次数:{npcBidMin},最大喊价次数:{npcBidMax}");
			if (auctionInfo.NpcOfferRate.Count > 1)
			{
				currentNpcOfferRate = Random.Range(auctionInfo.NpcOfferRate[0], auctionInfo.NpcOfferRate[1]);
			}
			compareNpcOfferRate = Random.value;
			Debug.Log((object)$"本局npc跟价放弃概率:{currentNpcOfferRate},本次对比放弃概率随机数:{compareNpcOfferRate},已喊次数{npcBidCount},本剧最低喊价次数:{npcBidMin}");
		}
		List<float> npcQuitOfferRange = auctionInfo.NpcQuitOfferRange;
		if (npcQuitOfferRange.Count > 1)
		{
			npcFangqi = Random.Range(npcQuitOfferRange[0], npcQuitOfferRange[1]);
		}
		currentRaiseStages = new List<float>();
		if (auctionInfo != null && auctionInfo.RaisePriceStage != null)
		{
			float num = 1f;
			if (auctionInfo.RaiseBuffRange != null && auctionInfo.RaiseBuffRange.Count > 1)
			{
				num = Random.Range(auctionInfo.RaiseBuffRange[0], auctionInfo.RaiseBuffRange[1]);
			}
			foreach (float item in auctionInfo.RaisePriceStage)
			{
				currentRaiseStages.Add(item * num);
			}
			if (currentRaiseStages.Count > 3)
			{
				Debug.Log((object)$"抬价相关数据:随机数{num}++++四个阈值{currentRaiseStages[0]}++++{currentRaiseStages[1]}++++{currentRaiseStages[2]}++++{currentRaiseStages[3]}");
			}
		}
		if (bidBtn.Length > 2)
		{
			((UnityEventBase)bidBtn[0].onClick).RemoveAllListeners();
			((UnityEventBase)bidBtn[1].onClick).RemoveAllListeners();
			((UnityEventBase)bidBtn[2].onClick).RemoveAllListeners();
		}
		for (int j = 0; j < isGetItemPos.Length; j++)
		{
			isGetItemPos[j] = false;
		}
		for (int k = 0; k < 3; k++)
		{
			int num2 = k;
			float multiplier = 1f;
			switch (num2)
			{
			case 0:
				multiplier = auctionInfo.PlayerOfferAdd1;
				break;
			case 1:
				multiplier = auctionInfo.PlayerOfferAdd2;
				break;
			default:
				multiplier = auctionInfo.PlayerOfferAdd3;
				break;
			}
			if (bidBtn.Length <= 2)
			{
				continue;
			}
			((UnityEvent)bidBtn[num2].onClick).AddListener((UnityAction)delegate
			{
				SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
				float num6 = qipaiPrice * multiplier;
				double num7 = AuctionDataManager.Instance?.GetAuctionCoin() ?? 0.0;
				if ((double)num6 > num7)
				{
					BroadcastManager.ShowPopUpTip("拍卖金钱不足");
				}
				else
				{
					ResetHostBidTime();
					ClearDownPanel(isPlayerBid: true);
					qipaiPrice = num6;
					bidNpcIndex = -1;
					if (!isBiddingMode && realPrice > 0f)
					{
						float ratio = qipaiPrice / realPrice;
						if (IsRatioAboveMinStage(ratio))
						{
							isBiddingMode = true;
							if ((Object)(object)bidding != (Object)null)
							{
								bidding.SetActive(true);
							}
						}
					}
					DisplayPlayerSpeak("我出价" + ColorPrice(qipaiPrice));
					isAcutionSuccess = true;
				}
			});
		}
		for (int num3 = 0; num3 < boxs.Length; num3++)
		{
			int x = num3;
			if (boxs.Length <= num3)
			{
				continue;
			}
			switch (Random.Range(0, 3))
			{
			case 0:
				SingletonMono<ResManager>.Instance.LoadSpriteAsync("Assets/GameRes/icon/auction/challenge_pic_case_black.png", delegate(Sprite sprite)
				{
					boxs[x].sprite = sprite;
				}, isAtlas: false);
				break;
			case 1:
				SingletonMono<ResManager>.Instance.LoadSpriteAsync("Assets/GameRes/icon/auction/challenge_pic_case_green.png", delegate(Sprite sprite)
				{
					boxs[x].sprite = sprite;
				}, isAtlas: false);
				break;
			case 2:
				SingletonMono<ResManager>.Instance.LoadSpriteAsync("Assets/GameRes/icon/auction/challenge_pic_case_purple.png", delegate(Sprite sprite)
				{
					boxs[x].sprite = sprite;
				}, isAtlas: false);
				break;
			}
		}
		dengguang.SetActive(false);
		biaoshi.SetActive(true);
		int num4 = AuctionDataManager.Instance?.GetCurrentSelect() ?? 0;
		num4++;
		SingletonMono<ResManager>.Instance.LoadSpriteAsync($"Assets/GameRes/auction_challenge/challenge_pic_carton_0{num4}_01.png", delegate(Sprite sprite)
		{
			jizhuangxiangImg.sprite = sprite;
		}, isAtlas: false);
		SingletonMono<ResManager>.Instance.LoadSpriteAsync($"Assets/GameRes/auction_challenge/challenge_pic_carton_0{num4}_02.png", delegate(Sprite sprite)
		{
			banshanImg.sprite = sprite;
		}, isAtlas: false);
		SingletonMono<ResManager>.Instance.LoadSpriteAsync($"Assets/GameRes/auction_challenge/challenge_pic_carton_0{num4}_03.png", delegate(Sprite sprite)
		{
			jzxOpenImg.sprite = sprite;
		}, isAtlas: false);
		downpanel.SetActive(true);
		SingletonMono<ResManager>.Instance.LoadSpriteAsync("Assets/GameRes/icon/auction_role/challenge_pic_character_01_back.png", delegate(Sprite sprite)
		{
			auctionPlayer.sprite = sprite;
		}, isAtlas: false);
		npcSuiji = Random.Range(2, 7);
		((Component)npcs[0]).gameObject.SetActive(true);
		((Component)npcs[1]).gameObject.SetActive(true);
		((Component)npcs[2]).gameObject.SetActive(true);
		((Component)hostImg).gameObject.SetActive(true);
		jzxOpen.SetActive(false);
		curtain.SetActive(true);
		OpenCurtain(isOpen: true, 0.3f);
		CreatItemBoxs();
		CreatItems();
		bidNpcIndex = -1;
		hostBidTime = 10f;
		hostBidReminderIndex = 0;
		RefreshCoinTxt();
		HideAllNpcEmojis();
		HideAllNpcSpeakBubbles();
		int num5 = AuctionDataManager.Instance?.GetCurrentSelect() ?? 0;
		num5++;
		auction auctionConfig = ConfigManager.Instance.GetAuctionConfig(num5);
		SingletonMono<ResManager>.Instance.LoadSpriteAsync($"Assets/GameRes/icon/auction_title/challenge_pic_title_{auctionConfig.AuctionHouseID}.png", delegate(Sprite sprite)
		{
			selectImg.sprite = sprite;
		}, isAtlas: false);
		SingletonMono<ResManager>.Instance.LoadSpriteAsync($"Assets/GameRes/icon/auction_title/challenge_pic_island_0{auctionConfig.AuctionHouseID}_scale2.png", delegate(Sprite sprite)
		{
			daoImg.sprite = sprite;
		}, isAtlas: false);
		P8SDK.LevelUpgrade(new LevelUpgradeOption
		{
			level = "AUCTION",
			level_status = "0"
		});
	}

	public void RefreshDownPanel()
	{
		auction auctionConfig = ConfigManager.Instance.GetAuctionConfig(selectId);
		for (int i = 0; i < bidBtn.Length; i++)
		{
			if (!((Selectable)bidBtn[i]).interactable)
			{
				((Selectable)bidBtn[i]).interactable = true;
			}
		}
		if (bidTxt.Length > 2)
		{
			((TMP_Text)bidTxt[0]).text = NumberFormatter.FormatNumber(qipaiPrice * auctionConfig.PlayerOfferAdd1);
			((TMP_Text)bidTxt[1]).text = NumberFormatter.FormatNumber(qipaiPrice * auctionConfig.PlayerOfferAdd2);
			((TMP_Text)bidTxt[2]).text = NumberFormatter.FormatNumber(qipaiPrice * auctionConfig.PlayerOfferAdd3);
		}
		if (yuguShowRandom < 0.2f)
		{
			((TMP_Text)yuguTxt).text = $"我的预估价格: ???";
		}
		else
		{
			((TMP_Text)yuguTxt).text = $"我的预估价格:{NumberFormatter.FormatNumber(yuguPriceLow)}~{NumberFormatter.FormatNumber(yuguPriceHigh)}";
		}
	}

	public void ClearDownPanel(bool isPlayerBid)
	{
		for (int i = 0; i < bidBtn.Length; i++)
		{
			if (((Selectable)bidBtn[i]).interactable)
			{
				((Selectable)bidBtn[i]).interactable = false;
			}
		}
		if (bidTxt.Length > 2)
		{
			if (!isPlayerBid)
			{
				((TMP_Text)bidTxt[0]).text = "-";
				((TMP_Text)bidTxt[1]).text = "-";
				((TMP_Text)bidTxt[2]).text = "-";
			}
			else
			{
				((TMP_Text)bidTxt[0]).text = "已出价";
				((TMP_Text)bidTxt[1]).text = "已出价";
				((TMP_Text)bidTxt[2]).text = "已出价";
			}
		}
		if (!isPlayerBid)
		{
			if (yuguShowRandom < 0.2f)
			{
				((TMP_Text)yuguTxt).text = $"我的预估价格: ???";
			}
			else
			{
				((TMP_Text)yuguTxt).text = $"我的预估价格:{NumberFormatter.FormatNumber(yuguPriceLow)}~{NumberFormatter.FormatNumber(yuguPriceHigh)}";
			}
		}
	}

	public void DisplayHostSpeak(string str)
	{
		if (speakTxt.Length != 0)
		{
			((TMP_Text)speakTxt[0]).text = str;
		}
		if (speakImgJishi.Length != 0)
		{
			speakImgJishi[0] = 2f;
		}
		((Component)speakImg[0]).gameObject.SetActive(true);
		RectTransform component = ((Component)speakImg[0]).GetComponent<RectTransform>();
		if ((Object)(object)component != (Object)null)
		{
			((MonoBehaviour)this).StartCoroutine(JellySpeakAnimation(component));
		}
	}

	public void DisplayPlayerSpeak(string str)
	{
		if (speakTxt.Length != 0)
		{
			((TMP_Text)speakTxt[2]).text = str;
		}
		if (speakImgJishi.Length > 1)
		{
			speakImgJishi[2] = 2f;
		}
		((Component)speakImg[2]).gameObject.SetActive(true);
		RectTransform component = ((Component)speakImg[2]).GetComponent<RectTransform>();
		if ((Object)(object)component != (Object)null)
		{
			((MonoBehaviour)this).StartCoroutine(JellySpeakAnimation(component));
		}
		ShowRandomNpcEmojis(-1);
	}

	public void DisplayNpcSpeak(int index, string str, bool showOtherNpcEmojis = true)
	{
		if (npcSpeakImgs == null || npcSpeakTxts == null || index < 0 || index >= npcSpeakImgs.Length)
		{
			return;
		}
		Image val = npcSpeakImgs[index];
		TextMeshProUGUI val2 = npcSpeakTxts[index];
		HideNpcSpecialEmoji(index);
		if ((Object)(object)val2 != (Object)null)
		{
			((TMP_Text)val2).text = str;
		}
		if (npcSpeakJishi != null && index < npcSpeakJishi.Length)
		{
			npcSpeakJishi[index] = 2f;
		}
		if ((Object)(object)val != (Object)null)
		{
			((Component)val).gameObject.SetActive(true);
			RectTransform component = ((Component)val).GetComponent<RectTransform>();
			if ((Object)(object)component != (Object)null)
			{
				((MonoBehaviour)this).StartCoroutine(JellySpeakAnimation(component));
			}
		}
		if (showOtherNpcEmojis)
		{
			ShowRandomNpcEmojis(index);
		}
	}

	public void SetItemPrice()
	{
		realPrice = 0f;
		qipaiPrice = 0f;
		yuguPriceLow = 0f;
		yuguPriceHigh = 0f;
		auction auctionConfig = ConfigManager.Instance.GetAuctionConfig(selectId);
		if (auctionConfig == null)
		{
			return;
		}
		List<auction> allAuction = ConfigManager.Instance.GetAllAuction();
		int num = 0;
		if (allAuction != null)
		{
			foreach (auction item in allAuction)
			{
				if (item.AuctionHouseID > num)
				{
					num = item.AuctionHouseID;
				}
			}
		}
		if (selectId <= num)
		{
			float conditionLevel = auctionConfig.ConditionLevel;
			float num2 = AuctionDataManager.Instance?.GetCurrentLevelWithProgress() ?? conditionLevel;
			auction auctionConfig2 = ConfigManager.Instance.GetAuctionConfig(selectId + 1);
			if (auctionConfig2 != null)
			{
				float num3 = auctionConfig2.ConditionLevel - conditionLevel;
				if (num3 > 0f)
				{
					float num4 = (num2 - conditionLevel) / num3;
					num4 = Mathf.Clamp01(num4);
					realPrice = (float)auctionConfig.ItemValueMin + (float)(auctionConfig.ItemValueMax - auctionConfig.ItemValueMin) * num4;
				}
				else
				{
					realPrice = auctionConfig.ItemValueMin;
					if (auctionConfig.FinalValueBuff.Count >= 2)
					{
						realPrice *= Random.Range(auctionConfig.FinalValueBuff[0], auctionConfig.FinalValueBuff[1]);
					}
				}
			}
			else
			{
				realPrice = auctionConfig.ItemValueMax;
			}
		}
		else
		{
			auction auctionConfig3 = ConfigManager.Instance.GetAuctionConfig(num);
			if (auctionConfig3 != null)
			{
				realPrice = auctionConfig3.ItemValueMax;
			}
		}
		if (auctionConfig.StartPriceRange != null && auctionConfig.StartPriceRange.Count >= 2)
		{
			float num5 = Random.Range(auctionConfig.StartPriceRange[0], auctionConfig.StartPriceRange[1]);
			qipaiPrice = Mathf.Floor(realPrice * num5);
		}
		else
		{
			qipaiPrice = Mathf.Floor(realPrice);
		}
		sourceQipaiPrice = qipaiPrice;
		if (auctionConfig.ExpectPriceRange != null && auctionConfig.ExpectPriceRange.Count >= 2)
		{
			yuguPriceLow = realPrice * auctionConfig.ExpectPriceRange[0];
			yuguPriceHigh = realPrice * auctionConfig.ExpectPriceRange[1];
		}
		else
		{
			yuguPriceLow = realPrice;
			yuguPriceHigh = realPrice;
		}
	}

	public void ChangeNpcBidTime()
	{
		if (bidTimeMax > bidTimeMin && bidTimeMin > 0f && bidTimeMax > 0f)
		{
			currentBidTime = Random.Range(bidTimeMin, bidTimeMax);
		}
		else
		{
			currentBidTime = Random.Range(1, 3);
		}
	}

	public void ResetHostBidTime()
	{
		hostBidTime = 10f;
		hostBidReminderIndex = 0;
	}

	public void CreatItemBoxs()
	{
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Expected O, but got Unknown
		if (itemBoxs == null)
		{
			itemBoxs = (Button[])(object)new Button[itemBoxPos.Length];
			for (int i = 0; i < itemBoxPos.Length; i++)
			{
				int index = i;
				UnityAction val4 = default(UnityAction);
				TweenCallback val7 = default(TweenCallback);
				SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/NewUI/Auction/prefabs/auctionItemBox.prefab", delegate(GameObject prefab)
				{
					//IL_006a: Unknown result type (might be due to invalid IL or missing references)
					//IL_0084: Unknown result type (might be due to invalid IL or missing references)
					//IL_0089: Unknown result type (might be due to invalid IL or missing references)
					//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
					//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
					//IL_011d: Unknown result type (might be due to invalid IL or missing references)
					//IL_0150: Unknown result type (might be due to invalid IL or missing references)
					//IL_0155: Unknown result type (might be due to invalid IL or missing references)
					//IL_0157: Expected O, but got Unknown
					//IL_015c: Expected O, but got Unknown
					GameObject val2 = Object.Instantiate<GameObject>(prefab, itemBoxList);
					Button component = val2.GetComponent<Button>();
					if ((Object)(object)component != (Object)null)
					{
						itemBoxs[index] = component;
					}
					((Component)itemBoxs[index]).gameObject.SetActive(false);
					val2.transform.position = itemBoxPos[index].position + new Vector3(0f, boxPosYOffset, 0f);
					Transform transform3 = val2.transform;
					_003F val3 = ((transform3 is RectTransform) ? transform3 : null);
					Transform transform4 = val2.transform;
					((RectTransform)val3).anchoredPosition = new Vector2(((RectTransform)((transform4 is RectTransform) ? transform4 : null)).anchoredPosition.x, (float)Screen.height / 2f + 400f);
					((UnityEventBase)itemBoxs[index].onClick).RemoveAllListeners();
					((Component)itemBoxs[index]).transform.localScale = new Vector3(boxScale, boxScale, 1f);
					ButtonClickedEvent onClick = itemBoxs[index].onClick;
					UnityAction obj = val4;
					if (obj == null)
					{
						UnityAction val5 = delegate
						{
							//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
							//IL_0167: Unknown result type (might be due to invalid IL or missing references)
							//IL_0188: Unknown result type (might be due to invalid IL or missing references)
							//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
							//IL_020b: Unknown result type (might be due to invalid IL or missing references)
							//IL_0233: Unknown result type (might be due to invalid IL or missing references)
							//IL_0238: Unknown result type (might be due to invalid IL or missing references)
							//IL_023a: Expected O, but got Unknown
							//IL_023f: Expected O, but got Unknown
							SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
							if ((Object)(object)fingerGuide != (Object)null)
							{
								((Component)fingerGuide).gameObject.SetActive(false);
							}
							((Component)itemBoxs[index]).gameObject.SetActive(false);
							if ((Object)(object)smokeEffects[index] != (Object)null)
							{
								((Component)smokeEffects[index]).transform.position = ((Component)itemBoxs[index]).transform.position;
								((Component)smokeEffects[index]).gameObject.SetActive(true);
								smokeEffects[index].AnimationState.SetAnimation(0, "effect", false);
							}
							if (items != null && items.Length > index)
							{
								((Component)items[index]).gameObject.SetActive(true);
								((Component)items[index]).transform.position = ((Component)itemBoxs[index]).transform.position;
								((Component)items[index]).transform.localRotation = Quaternion.identity;
								int itemPosIndex = GetItemPosIndex();
								ShortcutExtensions.DOMove(((Component)items[index]).transform, itemPos[itemPosIndex].position, itemFlyDuration, false);
								TweenerCore<Quaternion, Vector3, QuaternionOptions> obj2 = ShortcutExtensions.DORotate(((Component)items[index]).transform, new Vector3(0f, 0f, 360f * itemRotateCount), itemFlyDuration, (RotateMode)1);
								TweenCallback obj3 = val7;
								if (obj3 == null)
								{
									TweenCallback val8 = delegate
									{
										//IL_0017: Unknown result type (might be due to invalid IL or missing references)
										((Component)items[index]).transform.localRotation = Quaternion.identity;
										itemCalculatPirce += items[index].GetPrice();
										RefrshCalculatePanel();
										items[index].DisplayUI();
									};
									TweenCallback val9 = val8;
									val7 = val8;
									obj3 = val9;
								}
								TweenSettingsExtensions.OnComplete<TweenerCore<Quaternion, Vector3, QuaternionOptions>>(obj2, obj3);
							}
							openBoxCount++;
							if (openBoxCount >= itemBoxs.Length)
							{
								auctionState = 4;
								stateJiShi = 3f;
							}
						};
						UnityAction val6 = val5;
						val4 = val5;
						obj = val6;
					}
					((UnityEvent)onClick).AddListener(obj);
				});
			}
		}
		else
		{
			if (itemBoxs.Length == 0)
			{
				return;
			}
			for (int num = 0; num < itemBoxs.Length; num++)
			{
				int index2 = num;
				if (!((Object)(object)itemBoxs[index2] != (Object)null))
				{
					continue;
				}
				((Component)itemBoxs[index2]).gameObject.SetActive(false);
				((Component)itemBoxs[index2]).transform.position = itemBoxPos[index2].position + new Vector3(0f, boxPosYOffset, 0f);
				Transform transform = ((Component)itemBoxs[index2]).transform;
				_003F val = ((transform is RectTransform) ? transform : null);
				Transform transform2 = ((Component)itemBoxs[index2]).transform;
				((RectTransform)val).anchoredPosition = new Vector2(((RectTransform)((transform2 is RectTransform) ? transform2 : null)).anchoredPosition.x, (float)Screen.height / 2f + 400f);
				((Component)itemBoxs[index2]).transform.localScale = new Vector3(boxScale, boxScale, 1f);
				((UnityEventBase)itemBoxs[index2].onClick).RemoveAllListeners();
				((UnityEvent)itemBoxs[index2].onClick).AddListener((UnityAction)delegate
				{
					//IL_008c: Unknown result type (might be due to invalid IL or missing references)
					//IL_0167: Unknown result type (might be due to invalid IL or missing references)
					//IL_0188: Unknown result type (might be due to invalid IL or missing references)
					//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
					//IL_020b: Unknown result type (might be due to invalid IL or missing references)
					//IL_0228: Unknown result type (might be due to invalid IL or missing references)
					//IL_0232: Expected O, but got Unknown
					SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
					if ((Object)(object)fingerGuide != (Object)null)
					{
						((Component)fingerGuide).gameObject.SetActive(false);
					}
					if ((Object)(object)smokeEffects[index2] != (Object)null)
					{
						((Component)smokeEffects[index2]).transform.position = ((Component)itemBoxs[index2]).transform.position;
						((Component)smokeEffects[index2]).gameObject.SetActive(true);
						smokeEffects[index2].AnimationState.SetAnimation(0, "effect", false);
					}
					((Component)itemBoxs[index2]).gameObject.SetActive(false);
					if (items != null && items.Length > index2)
					{
						((Component)items[index2]).gameObject.SetActive(true);
						((Component)items[index2]).transform.position = ((Component)itemBoxs[index2]).transform.position;
						((Component)items[index2]).transform.localRotation = Quaternion.identity;
						int itemPosIndex = GetItemPosIndex();
						ShortcutExtensions.DOMove(((Component)items[index2]).transform, itemPos[itemPosIndex].position, itemFlyDuration, false);
						TweenSettingsExtensions.OnComplete<TweenerCore<Quaternion, Vector3, QuaternionOptions>>(ShortcutExtensions.DORotate(((Component)items[index2]).transform, new Vector3(0f, 0f, 360f * itemRotateCount), itemFlyDuration, (RotateMode)1), (TweenCallback)delegate
						{
							//IL_0017: Unknown result type (might be due to invalid IL or missing references)
							((Component)items[index2]).transform.localRotation = Quaternion.identity;
							itemCalculatPirce += items[index2].GetPrice();
							RefrshCalculatePanel();
							items[index2].DisplayUI();
						});
					}
					openBoxCount++;
					if (openBoxCount >= itemBoxs.Length)
					{
						auctionState = 4;
						stateJiShi = 3f;
					}
				});
			}
		}
	}

	public void CreatItems()
	{
		int num = itemPos.Length;
		int[] itemRarities = new int[num];
		float[] itemValues = new float[num];
		if (auctionInfo != null && auctionInfo.ResultItemRarity.Count > 0 && auctionInfo.ResultItemWeight.Count > 0)
		{
			int num2 = 0;
			for (int i = 0; i < auctionInfo.ResultItemWeight.Count; i++)
			{
				num2 += auctionInfo.ResultItemWeight[i];
			}
			if (num2 > 0)
			{
				for (int j = 0; j < num; j++)
				{
					int num3 = Random.Range(1, num2 + 1);
					int num4 = 0;
					int num5 = auctionInfo.ResultItemRarity[0];
					for (int k = 0; k < auctionInfo.ResultItemWeight.Count; k++)
					{
						num4 += auctionInfo.ResultItemWeight[k];
						if (num3 <= num4)
						{
							num5 = auctionInfo.ResultItemRarity[k];
							break;
						}
					}
					itemRarities[j] = num5;
				}
			}
			else
			{
				for (int l = 0; l < num; l++)
				{
					itemRarities[l] = ((auctionInfo.ResultItemRarity.Count > 0) ? auctionInfo.ResultItemRarity[0] : 0);
				}
			}
			float[] array = new float[num];
			float num6 = 0f;
			for (int m = 0; m < num; m++)
			{
				array[m] = (float)itemRarities[m] * Random.Range(0.8f, 1.2f);
				num6 += array[m];
			}
			auction auctionConfig = ConfigManager.Instance.GetAuctionConfig(selectId);
			float num7 = 0f;
			if (auctionConfig.FinalValueBuff.Count >= 2)
			{
				realsuiji = Random.Range(auctionConfig.FinalValueBuff[0], auctionConfig.FinalValueBuff[1]);
				num7 = realPrice * realsuiji;
			}
			if (num6 > 0f)
			{
				for (int n = 0; n < num; n++)
				{
					itemValues[n] = num7 * (array[n] / num6);
				}
			}
			else
			{
				for (int num8 = 0; num8 < num; num8++)
				{
					itemValues[num8] = num7 / (float)num;
				}
			}
			float num9 = 0f;
			for (int num10 = 0; num10 < num; num10++)
			{
				num9 += itemValues[num10];
			}
			float num11 = num7 - num9;
			if (Mathf.Abs(num11) > 0.001f && num > 0)
			{
				int num12 = 0;
				for (int num13 = 1; num13 < num; num13++)
				{
					if (itemRarities[num13] > itemRarities[num12])
					{
						num12 = num13;
					}
				}
				itemValues[num12] += num11;
			}
		}
		if (items == null)
		{
			items = new AuctionItem[itemPos.Length];
			for (int num14 = 0; num14 < itemPos.Length; num14++)
			{
				int index = num14;
				SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/NewUI/Auction/prefabs/auctionItem.prefab", delegate(GameObject prefab)
				{
					AuctionItem component = Object.Instantiate<GameObject>(prefab, itemBoxList).GetComponent<AuctionItem>();
					if ((Object)(object)component != (Object)null && items.Length > index)
					{
						items[index] = component;
					}
					((Component)items[index]).gameObject.SetActive(false);
					if ((Object)(object)items[index] != (Object)null && index < itemRarities.Length && index < itemValues.Length)
					{
						items[index].Init(itemRarities[index], itemValues[index]);
					}
				});
			}
			return;
		}
		for (int num15 = 0; num15 < items.Length; num15++)
		{
			int num16 = num15;
			((Component)items[num16]).gameObject.SetActive(false);
			if (num16 < itemRarities.Length && num16 < itemValues.Length)
			{
				items[num16].Init(itemRarities[num16], itemValues[num16]);
			}
		}
	}

	public int GetItemPosIndex()
	{
		int result = 0;
		if (isGetItemPos != null)
		{
			for (int i = 0; i < isGetItemPos.Length; i++)
			{
				if (!isGetItemPos[i])
				{
					isGetItemPos[i] = true;
					return i;
				}
			}
		}
		return result;
	}

	private void AuctionProgress()
	{
		//IL_0a4e: Unknown result type (might be due to invalid IL or missing references)
		//IL_093e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0943: Unknown result type (might be due to invalid IL or missing references)
		//IL_0948: Unknown result type (might be due to invalid IL or missing references)
		//IL_0965: Unknown result type (might be due to invalid IL or missing references)
		//IL_096a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0984: Unknown result type (might be due to invalid IL or missing references)
		//IL_0986: Unknown result type (might be due to invalid IL or missing references)
		//IL_09ab: Unknown result type (might be due to invalid IL or missing references)
		switch (auctionState)
		{
		case 0:
			if (stateJiShi < 1f)
			{
				stateJiShi += Time.deltaTime;
				break;
			}
			auctionState = 1;
			stateJiShi = 0f;
			break;
		case 1:
			DisplayHostSpeak("欢迎来到拍卖场");
			if (stateJiShi < 3f)
			{
				stateJiShi += Time.deltaTime;
			}
			else if (stateJiShi >= 3f)
			{
				DisplayHostSpeak("本次拍卖物品起拍价格为" + ColorPrice(qipaiPrice));
			}
			if (stateJiShi < 5f)
			{
				stateJiShi += Time.deltaTime;
			}
			else if (stateJiShi >= 5f)
			{
				stateJiShi = 0f;
				auctionState = 2;
				RefreshDownPanel();
				DisplayHostSpeak("请各位开始竞拍");
				currentBidTime = 2f;
			}
			break;
		case 2:
		{
			if (!isBiddingMode && realPrice > 0f)
			{
				float ratio = qipaiPrice / realPrice;
				if (IsRatioAboveMinStage(ratio))
				{
					isBiddingMode = true;
					if ((Object)(object)bidding != (Object)null)
					{
						bidding.SetActive(true);
					}
				}
			}
			if (isBiddingMode)
			{
				TryStartNpcAnger();
			}
			if (auctionInfo != null)
			{
				if (CanAngryNpcBid())
				{
					NpcBid(angryNpcIndex);
				}
				else if (compareNpcOfferRate <= currentNpcOfferRate)
				{
					if ((npcBidCount < npcBidMin || (npcBidCount <= npcBidMax && npcBidCount < npcBidMax)) && qipaiPrice < realPrice * npcFangqi)
					{
						NpcBid();
					}
				}
				else if (compareNpcOfferRate > currentNpcOfferRate && npcBidCount < npcBidMin)
				{
					NpcBid();
				}
			}
			if (sourceQipaiPrice != qipaiPrice)
			{
				if (hostBidTime <= 7f && hostBidReminderIndex == 0)
				{
					hostBidReminderIndex++;
					DisplayHostSpeak(ColorPrice(qipaiPrice) + "第一次");
				}
				else if (hostBidTime <= 4f && hostBidReminderIndex == 1)
				{
					hostBidReminderIndex++;
					DisplayHostSpeak(ColorPrice(qipaiPrice) + "第二次");
				}
				else if (hostBidTime <= 1f && hostBidReminderIndex == 2)
				{
					stateJiShi = 10f;
					auctionState = 3;
					realPrice *= realsuiji;
					hostBidReminderIndex = 0;
					ResetHostBidTime();
					DisplayHostSpeak(ColorPrice(qipaiPrice) + "第三次,成交");
					ClearDownPanel(isPlayerBid: false);
					Debug.Log((object)$"抬价最终开包随机数:{realsuiji} 物品最终开包价格{realPrice}");
				}
				if (hostBidTime > 0f)
				{
					hostBidTime -= Time.deltaTime;
				}
				else if (hostBidTime <= 0f)
				{
					stateJiShi = 10f;
					auctionState = 3;
					realPrice *= realsuiji;
					hostBidReminderIndex = 0;
					ResetHostBidTime();
					DisplayHostSpeak("竞拍完成");
					ClearDownPanel(isPlayerBid: false);
					Debug.Log((object)$"抬价最终开包随机数:{realsuiji} 物品最终开包价格{realPrice}");
				}
			}
			if (!isBiddingMode)
			{
				break;
			}
			if ((Object)(object)processing != (Object)null && realPrice > 0f)
			{
				float num12 = qipaiPrice / realPrice;
				if (currentRaiseStages != null && currentRaiseStages.Count > 1)
				{
					int index = Mathf.Max(0, currentRaiseStages.Count - 3);
					float num13 = currentRaiseStages[index];
					float num14 = currentRaiseStages[currentRaiseStages.Count - 1] - num13;
					if (num14 > 0f)
					{
						processing.fillAmount = Mathf.Clamp01((num12 - num13) / num14);
					}
					else
					{
						processing.fillAmount = Mathf.Clamp01(num12 - num13);
					}
				}
				else
				{
					processing.fillAmount = Mathf.Clamp01((num12 - 1f) / 0.3f);
				}
			}
			speedLineTimer += Time.deltaTime;
			if (!(speedLineTimer >= 0.1f))
			{
				break;
			}
			speedLineTimer -= 0.1f;
			if (speedLines == null)
			{
				break;
			}
			for (int num15 = 0; num15 < speedLines.Length; num15++)
			{
				if ((Object)(object)speedLines[num15] != (Object)null)
				{
					speedLines[num15].SetActive(num15 == speedLineIndex);
				}
			}
			speedLineIndex = (speedLineIndex + 1) % speedLines.Length;
			break;
		}
		case 3:
		{
			float num = 0f;
			if (!isAcutionSuccess && isBiddingMode && realPrice > 0f && auctionInfo != null)
			{
				float num2 = qipaiPrice / realPrice;
				List<float> list = currentRaiseStages;
				List<float> raisePriceIncomeRange = auctionInfo.RaisePriceIncomeRange;
				float num3 = 0f;
				if (list != null && list.Count > 0 && raisePriceIncomeRange != null && raisePriceIncomeRange.Count > 0)
				{
					int num4 = 0;
					bool flag = false;
					for (int num5 = 0; num5 < list.Count; num5++)
					{
						if (num2 < list[num5])
						{
							num4 = num5;
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						num4 = list.Count;
					}
					if (num4 < raisePriceIncomeRange.Count)
					{
						num3 = raisePriceIncomeRange[num4];
					}
					else if (raisePriceIncomeRange.Count > 0)
					{
						num3 = raisePriceIncomeRange[raisePriceIncomeRange.Count - 1];
					}
				}
				if (num3 > 0f)
				{
					tuijiaPrice = (qipaiPrice - realPrice) * num3;
				}
				else
				{
					tuijiaPrice = 0f;
				}
				tuijiaPrice = Mathf.Abs(tuijiaPrice);
				num = num3;
			}
			else
			{
				tuijiaPrice = 0f;
			}
			if (isBiddingMode)
			{
				if ((Object)(object)bidding != (Object)null)
				{
					bidding.SetActive(false);
				}
				if (speedLines != null)
				{
					GameObject[] array = speedLines;
					foreach (GameObject val in array)
					{
						if ((Object)(object)val != (Object)null)
						{
							val.SetActive(false);
						}
					}
				}
			}
			if (stateJiShi <= 10f && hostBidReminderIndex == 0)
			{
				OpenCurtain(isOpen: false, 0.5f);
				hostBidReminderIndex++;
				Debug.Log((object)$"抬价相关数据:抬价收益{tuijiaPrice}++++最后喊价{qipaiPrice}+++货物真实价格{realPrice}++++抬价获利比例{num}");
			}
			if (stateJiShi <= 9f && hostBidReminderIndex == 1)
			{
				dengguang.SetActive(true);
				downpanel.SetActive(false);
				((Component)npcs[0]).gameObject.SetActive(false);
				((Component)npcs[1]).gameObject.SetActive(false);
				((Component)npcs[2]).gameObject.SetActive(false);
				((Component)hostImg).gameObject.SetActive(false);
				if (isAcutionSuccess)
				{
					SingletonMono<ResManager>.Instance.LoadSpriteAsync("Assets/GameRes/icon/auction_role/challenge_pic_character_01_front.png", delegate(Sprite sprite)
					{
						auctionPlayer.sprite = sprite;
					}, isAtlas: false);
				}
				else
				{
					biaoshi.SetActive(false);
					SingletonMono<ResManager>.Instance.LoadSpriteAsync($"Assets/GameRes/icon/auction_role/challenge_pic_character_0{npcSuiji}_front.png", delegate(Sprite sprite)
					{
						auctionPlayer.sprite = sprite;
					}, isAtlas: false);
				}
				OpenCurtain(isOpen: true, 0.5f);
				jzxOpen.SetActive(true);
				hostBidReminderIndex++;
				RefrshCalculatePanel();
			}
			if (stateJiShi <= 7f && hostBidReminderIndex == 2)
			{
				hostBidReminderIndex++;
				if (itemBoxs.Length != 0)
				{
					for (int num7 = 0; num7 < itemBoxs.Length; num7++)
					{
						if (isAcutionSuccess)
						{
							((Behaviour)itemBoxs[num7]).enabled = true;
						}
						else
						{
							((Behaviour)itemBoxs[num7]).enabled = false;
						}
						((Component)itemBoxs[num7]).gameObject.SetActive(true);
					}
				}
				for (int num8 = 0; num8 < itemBoxs.Length; num8++)
				{
					int num9 = num8;
					if ((Object)(object)itemBoxs[num9] != (Object)null && itemBoxPos.Length > num8)
					{
						Vector3 val2 = Camera.main.WorldToScreenPoint(itemBoxPos[num9].position);
						if ((Object)(object)((Component)itemBoxs[num9]).transform.parent != (Object)null)
						{
							Vector2 zero = Vector2.zero;
							Transform parent = ((Component)itemBoxs[num9]).transform.parent;
							RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)(object)((parent is RectTransform) ? parent : null), Vector2.op_Implicit(val2), Camera.main, ref zero);
							Transform transform = ((Component)itemBoxs[num9]).transform;
							ShortcutExtensions.DOLocalMoveY((transform is RectTransform) ? transform : null, zero.y, 1f + (float)num9 / 10f, false);
						}
					}
				}
			}
			if (stateJiShi <= 5f && hostBidReminderIndex == 3)
			{
				hostBidReminderIndex++;
				if (isAcutionSuccess && (Object)(object)fingerGuide != (Object)null && itemBoxs.Length != 0 && (Object)(object)itemBoxs[0] != (Object)null)
				{
					((Component)fingerGuide).transform.position = ((Component)itemBoxs[0]).transform.position;
					((Component)fingerGuide).gameObject.SetActive(true);
					fingerGuide.AnimationState.SetAnimation(0, "dianji", true);
				}
			}
			if (!isAcutionSuccess)
			{
				for (int num10 = 0; num10 < itemPos.Length; num10++)
				{
					int num11 = num10;
					if (!(stateJiShi <= 4f - (float)num11 * 0.2f) || hostBidReminderIndex != num11 + 4)
					{
						continue;
					}
					hostBidReminderIndex++;
					if (itemBoxs.Length > num10)
					{
						ButtonClickedEvent onClick = itemBoxs[num11].onClick;
						if (onClick != null)
						{
							((UnityEvent)onClick).Invoke();
						}
					}
				}
			}
			if (stateJiShi > 0f)
			{
				stateJiShi -= Time.deltaTime;
			}
			else if (stateJiShi <= 0f)
			{
				stateJiShi = 0f;
			}
			break;
		}
		case 4:
			if (stateJiShi > 0f)
			{
				stateJiShi -= Time.deltaTime;
			}
			else
			{
				if (!(stateJiShi <= 0f))
				{
					break;
				}
				if (isAcutionSuccess)
				{
					if (itemCalculatPirce - qipaiPrice < 0f)
					{
						SingletonMono<ResManager>.Instance.LoadSpriteAsync("Assets/GameRes/icon/auction_role/challenge_pic_character_01_sad.png", delegate(Sprite sprite)
						{
							auctionPlayer.sprite = sprite;
						}, isAtlas: false);
					}
					else
					{
						SingletonMono<ResManager>.Instance.LoadSpriteAsync("Assets/GameRes/icon/auction_role/challenge_pic_character_01_happy.png", delegate(Sprite sprite)
						{
							auctionPlayer.sprite = sprite;
						}, isAtlas: false);
					}
				}
				else if (itemCalculatPirce - qipaiPrice < 0f)
				{
					SingletonMono<ResManager>.Instance.LoadSpriteAsync($"Assets/GameRes/icon/auction_role/challenge_pic_character_0{npcSuiji}_sad.png", delegate(Sprite sprite)
					{
						auctionPlayer.sprite = sprite;
					}, isAtlas: false);
				}
				else
				{
					SingletonMono<ResManager>.Instance.LoadSpriteAsync($"Assets/GameRes/icon/auction_role/challenge_pic_character_0{npcSuiji}_happy.png", delegate(Sprite sprite)
					{
						auctionPlayer.sprite = sprite;
					}, isAtlas: false);
				}
				stateJiShi = 0f;
				auctionState = 4;
				AuctionEndPanel(isAcutionSuccess, isOpen: true);
			}
			break;
		}
		for (int num16 = 0; num16 < speakImgJishi.Length; num16++)
		{
			if (speakImgJishi[num16] > 0f)
			{
				speakImgJishi[num16] -= Time.deltaTime;
			}
			else if (speakImgJishi[num16] <= 0f)
			{
				speakImgJishi[num16] = 0f;
				if (speakImg.Length > num16 && ((Component)speakImg[num16]).gameObject.activeSelf)
				{
					((Component)speakImg[num16]).gameObject.SetActive(false);
				}
			}
		}
		if (npcSpeakJishi != null && npcSpeakImgs != null)
		{
			for (int num17 = 0; num17 < npcSpeakJishi.Length; num17++)
			{
				if (!(npcSpeakJishi[num17] > 0f))
				{
					continue;
				}
				npcSpeakJishi[num17] -= Time.deltaTime;
				if (npcSpeakJishi[num17] <= 0f)
				{
					npcSpeakJishi[num17] = 0f;
					HideNpcSpecialEmoji(num17);
					if ((Object)(object)npcSpeakImgs[num17] != (Object)null)
					{
						((Component)npcSpeakImgs[num17]).gameObject.SetActive(false);
					}
				}
			}
		}
		if (npcEmojiJishi == null || npcEmojiImgs == null)
		{
			return;
		}
		for (int num18 = 0; num18 < npcEmojiJishi.Length; num18++)
		{
			if (!(npcEmojiJishi[num18] > 0f))
			{
				continue;
			}
			npcEmojiJishi[num18] -= Time.deltaTime;
			if (npcEmojiJishi[num18] <= 0f)
			{
				npcEmojiJishi[num18] = 0f;
				if ((Object)(object)npcEmojiImgs[num18] != (Object)null)
				{
					((Component)npcEmojiImgs[num18]).gameObject.SetActive(false);
				}
			}
		}
	}

	private void RefrshCalculatePanel()
	{
		if (!auctionCalculatPanel.activeSelf)
		{
			auctionCalculatPanel.SetActive(true);
		}
		((TMP_Text)zongjiaTxt).text = NumberFormatter.FormatNumber(itemCalculatPirce);
		yingliPrice = itemCalculatPirce - qipaiPrice;
		if (itemCalculatPirce - qipaiPrice < 0f)
		{
			SingletonMono<ResManager>.Instance.LoadSpriteAsync("Assets/GameRes/NewUI/Auction/images/challenge_zi_kuisun.png", delegate(Sprite sprite)
			{
				yingliImg.sprite = sprite;
			}, isAtlas: false);
			((TMP_Text)yingliTxt).text = "<color=#ED213F>" + NumberFormatter.FormatNumber(itemCalculatPirce - qipaiPrice) + "</color>";
		}
		else
		{
			SingletonMono<ResManager>.Instance.LoadSpriteAsync("Assets/GameRes/NewUI/Auction/images/challenge_zi_yingli.png", delegate(Sprite sprite)
			{
				yingliImg.sprite = sprite;
			}, isAtlas: false);
			((TMP_Text)yingliTxt).text = "<color=#3AED21>" + NumberFormatter.FormatNumber(itemCalculatPirce - qipaiPrice) + "</color>";
		}
	}

	private void AuctionEndPanel(bool isSuccessAcution, bool isOpen)
	{
		if (isSuccessAcution)
		{
			((Component)freeSoldBtn).gameObject.SetActive(true);
			((Component)adSoldBtn).gameObject.SetActive(true);
			((Component)enterEndBtn).gameObject.SetActive(false);
		}
		else
		{
			((Component)freeSoldBtn).gameObject.SetActive(false);
			((Component)adSoldBtn).gameObject.SetActive(false);
			((Component)enterEndBtn).gameObject.SetActive(true);
		}
		auctionEndPanel.SetActive(isOpen);
	}

	public void RefrshAuctionTxt()
	{
		((TMP_Text)auctionCoinTxt).text = NumberFormatter.FormatNumber(AuctionDataManager.Instance?.GetAuctionCoin() ?? 0.0);
		((TMP_Text)ticketTxt).text = NumberFormatter.FormatNumber(AuctionDataManager.Instance?.GetTicketCount() ?? 0);
	}

	public void OpenCurtain(bool isOpen, float openSpeed)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		mubuAni.AnimationState.ClearTrack(0);
		if (isOpen)
		{
			mubuAni.AnimationState.SetAnimation(0, "open", false).TrackTime = 0f;
			if (mubuAni.AnimationState != null)
			{
				TrackEntryDelegate completeHandler = null;
				completeHandler = (TrackEntryDelegate)delegate
				{
					mubuAni.AnimationState.Complete -= completeHandler;
					curtain.SetActive(false);
				};
				mubuAni.AnimationState.Complete += completeHandler;
			}
		}
		else
		{
			mubuAni.AnimationState.SetAnimation(0, "close", false).TrackTime = 0f;
		}
		mubuAni.AnimationState.Apply(mubuAni.Skeleton);
		mubuAni.UpdateMesh(false);
		mubuAni.AnimationState.TimeScale = openSpeed;
		((Component)mubuAni).gameObject.SetActive(true);
		curtain.SetActive(true);
	}

	private string ColorPrice(float price)
	{
		return "<color=#185890>" + NumberFormatter.FormatNumber(price) + "</color>";
	}

	private void InitNpcSpeakBubbles()
	{
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		if (npcPos == null || npcPos.Length == 0 || speakImg == null || speakImg.Length <= 1 || (Object)(object)speakImg[1] == (Object)null)
		{
			return;
		}
		npcSpeakImgs = (Image[])(object)new Image[npcPos.Length];
		npcSpeakTxts = (TextMeshProUGUI[])(object)new TextMeshProUGUI[npcPos.Length];
		npcSpeakJishi = new float[npcPos.Length];
		npcSpecialEmojiImgs = (Image[])(object)new Image[npcPos.Length];
		GameObject gameObject = ((Component)speakImg[1]).gameObject;
		gameObject.SetActive(false);
		for (int i = 0; i < npcPos.Length; i++)
		{
			if ((Object)(object)npcPos[i] == (Object)null)
			{
				continue;
			}
			GameObject val = Object.Instantiate<GameObject>(gameObject, npcPos[i], false);
			((Object)val).name = $"NpcSpeak_{i + 1}";
			Transform transform = val.transform;
			RectTransform val2 = (RectTransform)(object)((transform is RectTransform) ? transform : null);
			if ((Object)(object)val2 != (Object)null)
			{
				val2.anchoredPosition = new Vector2(0f, 180f);
				((Transform)val2).localScale = Vector3.one;
			}
			npcSpeakImgs[i] = val.GetComponent<Image>();
			npcSpeakTxts[i] = val.GetComponentInChildren<TextMeshProUGUI>(true);
			Transform val3 = val.transform.Find("emojiImg");
			if ((Object)(object)val3 != (Object)null)
			{
				npcSpecialEmojiImgs[i] = ((Component)val3).GetComponent<Image>();
				if ((Object)(object)npcSpecialEmojiImgs[i] != (Object)null)
				{
					((Component)npcSpecialEmojiImgs[i]).gameObject.SetActive(false);
				}
			}
			val.SetActive(false);
		}
	}

	private void HideAllNpcSpeakBubbles()
	{
		if (npcSpeakImgs != null)
		{
			for (int i = 0; i < npcSpeakImgs.Length; i++)
			{
				if ((Object)(object)npcSpeakImgs[i] != (Object)null)
				{
					((Component)npcSpeakImgs[i]).gameObject.SetActive(false);
				}
				HideNpcSpecialEmoji(i);
			}
		}
		if (npcSpeakJishi != null)
		{
			for (int j = 0; j < npcSpeakJishi.Length; j++)
			{
				npcSpeakJishi[j] = 0f;
			}
		}
	}

	private void ShowNpcSpecialEmoji(int npcIndex, Sprite sprite)
	{
		HideNpcNormalEmoji(npcIndex);
		if (npcSpecialEmojiImgs != null && npcIndex >= 0 && npcIndex < npcSpecialEmojiImgs.Length)
		{
			Image val = npcSpecialEmojiImgs[npcIndex];
			if ((Object)(object)val == (Object)null || (Object)(object)sprite == (Object)null)
			{
				HideNpcSpecialEmoji(npcIndex);
				return;
			}
			val.sprite = sprite;
			((Component)val).gameObject.SetActive(true);
		}
	}

	private void HideNpcSpecialEmoji(int npcIndex)
	{
		if (npcSpecialEmojiImgs != null && npcIndex >= 0 && npcIndex < npcSpecialEmojiImgs.Length)
		{
			Image val = npcSpecialEmojiImgs[npcIndex];
			if ((Object)(object)val != (Object)null)
			{
				((Component)val).gameObject.SetActive(false);
			}
		}
	}

	private void HideNpcNormalEmoji(int npcIndex)
	{
		if (npcEmojiImgs != null && npcIndex >= 0 && npcIndex < npcEmojiImgs.Length && (Object)(object)npcEmojiImgs[npcIndex] != (Object)null)
		{
			((Component)npcEmojiImgs[npcIndex]).gameObject.SetActive(false);
		}
		if (npcEmojiJishi != null && npcIndex >= 0 && npcIndex < npcEmojiJishi.Length)
		{
			npcEmojiJishi[npcIndex] = 0f;
		}
	}

	private void InitNpcEmojis()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		if (npcPos != null && npcPos.Length != 0)
		{
			npcEmojiImgs = (Image[])(object)new Image[npcPos.Length];
			npcEmojiJishi = new float[npcPos.Length];
			for (int i = 0; i < npcPos.Length; i++)
			{
				GameObject val = new GameObject("NpcEmoji", new Type[2]
				{
					typeof(RectTransform),
					typeof(Image)
				});
				val.transform.SetParent(npcPos[i]);
				Transform transform = val.transform;
				Transform obj = ((transform is RectTransform) ? transform : null);
				((RectTransform)obj).anchoredPosition = new Vector2(0f, -120f);
				((RectTransform)obj).sizeDelta = new Vector2(80f, 80f);
				obj.localScale = Vector3.one;
				Image component = val.GetComponent<Image>();
				((Graphic)component).raycastTarget = false;
				component.preserveAspect = true;
				((Component)component).gameObject.SetActive(false);
				npcEmojiImgs[i] = component;
			}
		}
	}

	private void HideAllNpcEmojis()
	{
		if (npcEmojiImgs != null)
		{
			for (int i = 0; i < npcEmojiImgs.Length; i++)
			{
				if ((Object)(object)npcEmojiImgs[i] != (Object)null)
				{
					((Component)npcEmojiImgs[i]).gameObject.SetActive(false);
				}
			}
		}
		if (npcEmojiJishi != null)
		{
			for (int j = 0; j < npcEmojiJishi.Length; j++)
			{
				npcEmojiJishi[j] = 0f;
			}
		}
	}

	private bool EvaluateNpcGiveUps(int biddingNpcIndex)
	{
		if (auctionInfo == null || npcHasGivenUp == null)
		{
			return false;
		}
		if (npcBidCount <= npcBidMin)
		{
			return false;
		}
		int num = npcBidCount - npcBidMin;
		if (num <= lastGiveUpRound)
		{
			return false;
		}
		lastGiveUpRound = num;
		List<float> giveUpChanceRange = GetGiveUpChanceRange(num);
		if (giveUpChanceRange == null || giveUpChanceRange.Count == 0)
		{
			return false;
		}
		bool result = false;
		for (int i = 0; i < npcHasGivenUp.Length; i++)
		{
			if (npcHasGivenUp[i] || IsNpcAngry(i))
			{
				continue;
			}
			float randomChance = GetRandomChance(giveUpChanceRange);
			float value = Random.value;
			if (value <= randomChance)
			{
				npcHasGivenUp[i] = true;
				if (i == biddingNpcIndex)
				{
					result = true;
				}
				DisplayNpcGiveUp(i);
				Debug.Log((object)$"NPC{i + 1}在放弃轮次{num}退出跟价，概率:{randomChance}，随机数:{value}");
			}
		}
		if (!HasActiveNpc())
		{
			Debug.Log((object)$"所有NPC已在放弃轮次{num}退出跟价，后续不再产生NPC报价");
		}
		return result;
	}

	private List<float> GetGiveUpChanceRange(int giveUpRound)
	{
		if (auctionInfo == null)
		{
			return null;
		}
		return giveUpRound switch
		{
			1 => auctionInfo.GiveUpChance1, 
			2 => auctionInfo.GiveUpChance2, 
			3 => auctionInfo.GiveUpChance3, 
			_ => auctionInfo.GiveUpChance4, 
		};
	}

	private float GetRandomChance(List<float> chanceRange)
	{
		if (chanceRange == null || chanceRange.Count == 0)
		{
			return 0f;
		}
		if (chanceRange.Count == 1)
		{
			return Mathf.Clamp01(chanceRange[0]);
		}
		float num = Mathf.Min(chanceRange[0], chanceRange[1]);
		float num2 = Mathf.Max(chanceRange[0], chanceRange[1]);
		return Mathf.Clamp01(Random.Range(num, num2));
	}

	private void TryStartNpcAnger()
	{
		if (!isBiddingMode || angerRollCompleted || auctionInfo == null)
		{
			return;
		}
		angerRollCompleted = true;
		List<int> list = new List<int>();
		if (npcHasGivenUp != null)
		{
			for (int i = 0; i < npcHasGivenUp.Length; i++)
			{
				if (!npcHasGivenUp[i])
				{
					list.Add(i);
				}
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		float randomChance = GetRandomChance(auctionInfo.AngerProbability);
		float value = Random.value;
		if (value > randomChance)
		{
			Debug.Log((object)$"本局NPC未进入愤怒状态，概率:{randomChance}，随机数:{value}");
			return;
		}
		int randomAngerRounds = GetRandomAngerRounds(auctionInfo.AngerRound);
		if (randomAngerRounds > 0)
		{
			angryNpcIndex = list[Random.Range(0, list.Count)];
			angryRoundsRemaining = randomAngerRounds;
			angryRoundsTotal = randomAngerRounds;
			DisplayNpcAnger(angryNpcIndex);
			ChangeNpcBidTime();
			Debug.Log((object)$"NPC{angryNpcIndex + 1}进入愤怒状态，将必跟价{angryRoundsTotal}回合，概率:{randomChance}，随机数:{value}");
		}
	}

	private int GetRandomAngerRounds(List<float> roundRange)
	{
		if (roundRange == null || roundRange.Count == 0)
		{
			return 0;
		}
		float num = roundRange[0];
		float num2 = ((roundRange.Count > 1) ? roundRange[1] : num);
		int num3 = Mathf.Max(1, Mathf.RoundToInt(Mathf.Min(num, num2)));
		int num4 = Mathf.Max(num3, Mathf.RoundToInt(Mathf.Max(num, num2)));
		return Random.Range(num3, num4 + 1);
	}

	private bool IsNpcAngry(int npcIndex)
	{
		if (angryRoundsRemaining > 0 && npcIndex >= 0 && npcIndex == angryNpcIndex && npcHasGivenUp != null && npcIndex < npcHasGivenUp.Length)
		{
			return !npcHasGivenUp[npcIndex];
		}
		return false;
	}

	private bool CanAngryNpcBid()
	{
		if (IsNpcAngry(angryNpcIndex))
		{
			return bidNpcIndex != angryNpcIndex;
		}
		return false;
	}

	private void DisplayNpcAnger(int npcIndex)
	{
		string str = NPC_ANGER_TEXTS[Random.Range(0, NPC_ANGER_TEXTS.Length)];
		DisplayNpcSpeak(npcIndex, str, showOtherNpcEmojis: false);
		ShowNpcSpecialEmoji(npcIndex, npcAngryEmojiSprite);
	}

	private void CompleteOneAngryBidRound()
	{
		if (IsNpcAngry(angryNpcIndex))
		{
			int num = angryNpcIndex;
			angryRoundsRemaining--;
			Debug.Log((object)$"NPC{num + 1}完成愤怒跟价，剩余{angryRoundsRemaining}/{angryRoundsTotal}回合");
			if (angryRoundsRemaining <= 0)
			{
				angryNpcIndex = -1;
				HideNpcSpecialEmoji(num);
				Debug.Log((object)$"NPC{num + 1}愤怒状态结束");
			}
		}
	}

	private void DisplayNpcGiveUp(int npcIndex)
	{
		string str = NPC_GIVE_UP_TEXTS[Random.Range(0, NPC_GIVE_UP_TEXTS.Length)];
		DisplayNpcSpeak(npcIndex, str, showOtherNpcEmojis: false);
		ShowNpcSpecialEmoji(npcIndex, npcGiveUpEmojiSprite);
	}

	private bool HasActiveNpc()
	{
		if (npcHasGivenUp == null || npcHasGivenUp.Length == 0)
		{
			return false;
		}
		for (int i = 0; i < npcHasGivenUp.Length; i++)
		{
			if (!npcHasGivenUp[i])
			{
				return true;
			}
		}
		return false;
	}

	private bool TryGetNextActiveNpcIndex(out int npcIndex)
	{
		npcIndex = -1;
		if (npcHasGivenUp == null || npcHasGivenUp.Length == 0)
		{
			return false;
		}
		List<int> list = new List<int>();
		for (int i = 0; i < npcHasGivenUp.Length; i++)
		{
			if (!npcHasGivenUp[i] && i != bidNpcIndex)
			{
				list.Add(i);
			}
		}
		if (list.Count == 0)
		{
			return false;
		}
		npcIndex = list[Random.Range(0, list.Count)];
		return true;
	}

	private void ShowRandomNpcEmojis(int excludeIndex)
	{
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		if (emojiSprites == null || emojiSprites.Length == 0 || npcEmojiImgs == null || npcEmojiImgs.Length == 0)
		{
			return;
		}
		List<int> list = new List<int>();
		for (int i = 0; i < npcEmojiImgs.Length; i++)
		{
			bool flag = npcHasGivenUp != null && i < npcHasGivenUp.Length && npcHasGivenUp[i];
			bool flag2 = IsNpcAngry(i);
			if (i != excludeIndex && !flag && !flag2)
			{
				list.Add(i);
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		int num = Random.Range(1, Mathf.Min(3, list.Count + 1));
		for (int j = 0; j < list.Count; j++)
		{
			int index = Random.Range(0, list.Count);
			int value = list[j];
			list[j] = list[index];
			list[index] = value;
		}
		for (int k = 0; k < num && k < list.Count; k++)
		{
			int num2 = list[k];
			if ((Object)(object)npcEmojiImgs[num2] != (Object)null && emojiSprites.Length != 0)
			{
				int num3 = Random.Range(0, emojiSprites.Length);
				npcEmojiImgs[num2].sprite = emojiSprites[num3];
				((Component)npcEmojiImgs[num2]).gameObject.SetActive(true);
				Transform transform = ((Component)npcEmojiImgs[num2]).transform;
				((RectTransform)((transform is RectTransform) ? transform : null)).anchoredPosition = Vector2.zero;
				if (npcEmojiJishi != null && npcEmojiJishi.Length > num2)
				{
					npcEmojiJishi[num2] = 2f;
				}
			}
		}
	}

	private IEnumerator JellySpeakAnimation(RectTransform target)
	{
		if (!((Object)(object)target == (Object)null))
		{
			float elapsed = 0f;
			((Transform)target).localScale = Vector3.one * speakJellyStartScale;
			while (elapsed < speakJellyDuration)
			{
				elapsed += Time.deltaTime;
				float num = elapsed / speakJellyDuration;
				float num2 = Mathf.Exp(-6f * num);
				float num3 = Mathf.Cos(14f * num);
				float num4 = 1f + (speakJellyStartScale - 1f) * num2 * (1f + speakJellyBounceIntensity * num3);
				((Transform)target).localScale = Vector3.one * num4;
				yield return null;
			}
			((Transform)target).localScale = Vector3.one;
		}
	}

	public void RefreshCoinTxt()
	{
		((TMP_Text)auctionCoinTxt).text = NumberFormatter.FormatNumber(AuctionDataManager.Instance?.GetAuctionCoin() ?? 0.0);
		((TMP_Text)ticketTxt).text = NumberFormatter.FormatNumber(AuctionDataManager.Instance?.GetTicketCount() ?? 0);
	}

	public void OpenEndPanel()
	{
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/maintaskreward.mp3");
		EndPanel.SetActive(true);
		((TMP_Text)paimai_yingli).text = NumberFormatter.FormatNumber(yingliPrice);
		SetTextColorByValue(paimai_yingli, yingliPrice);
		if (!isAcutionSuccess && isBiddingMode && !hasCollectedTuijia && tuijiaPrice > 0f)
		{
			hasCollectedTuijia = true;
			double num = AuctionDataManager.Instance?.GetAuctionCoin() ?? 0.0;
			AuctionDataManager.Instance?.SetAuctionCoin(num + (double)tuijiaPrice);
			RefreshCoinTxt();
		}
		((TMP_Text)taijia_yingli).text = NumberFormatter.FormatNumber(tuijiaPrice);
		SetTextColorByValue(taijia_yingli, tuijiaPrice);
		((TMP_Text)shijian_yingli).text = NumberFormatter.FormatNumber(0);
		SetTextColorByValue(shijian_yingli, 0f);
		((TMP_Text)qita_yingli).text = NumberFormatter.FormatNumber(0);
		SetTextColorByValue(qita_yingli, 0f);
		((TMP_Text)zong_yingli).text = NumberFormatter.FormatNumber(zongPrice);
		if (zongPrice >= 0f)
		{
			((Graphic)zong_yingli).color = Color.white;
		}
		else
		{
			((Graphic)zong_yingli).color = new Color(0.945f, 0.086f, 0.078f);
		}
	}

	private void SetTextColorByValue(TextMeshProUGUI text, float value)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)text == (Object)null))
		{
			if (value < 0f)
			{
				((Graphic)text).color = new Color(0.945f, 0.086f, 0.078f);
			}
			else
			{
				((Graphic)text).color = new Color(0.176f, 0.678f, 0.157f);
			}
		}
	}

	private void NpcBid(int forcedNpcIndex = -1)
	{
		if (currentBidTime > 0f)
		{
			currentBidTime -= Time.deltaTime;
		}
		else
		{
			if (!(currentBidTime <= 0f))
			{
				return;
			}
			bool flag = forcedNpcIndex >= 0;
			int npcIndex;
			if (flag)
			{
				if (!IsNpcAngry(forcedNpcIndex) || forcedNpcIndex == bidNpcIndex)
				{
					return;
				}
				npcIndex = forcedNpcIndex;
			}
			else if (!TryGetNextActiveNpcIndex(out npcIndex))
			{
				return;
			}
			npcBidCount++;
			if (EvaluateNpcGiveUps(npcIndex))
			{
				ChangeNpcBidTime();
				compareNpcOfferRate = Random.value;
				Debug.Log((object)$"NPC{npcIndex + 1}本轮放弃叫价，价格保持{qipaiPrice}，当前计入叫价轮次:{npcBidCount}");
				return;
			}
			List<float> npcOfferAddRange = auctionInfo.NpcOfferAddRange;
			float num = 1f;
			if (npcOfferAddRange != null && npcOfferAddRange.Count > 1)
			{
				num = Random.Range(npcOfferAddRange[0], npcOfferAddRange[1]);
			}
			qipaiPrice *= num;
			if (!isBiddingMode && realPrice > 0f)
			{
				float ratio = qipaiPrice / realPrice;
				if (IsRatioAboveMinStage(ratio))
				{
					isBiddingMode = true;
					if ((Object)(object)bidding != (Object)null)
					{
						bidding.SetActive(true);
					}
				}
			}
			bidNpcIndex = npcIndex;
			DisplayNpcSpeak(bidNpcIndex, "我出价" + ColorPrice(qipaiPrice));
			ChangeNpcBidTime();
			if (flag)
			{
				CompleteOneAngryBidRound();
			}
			ResetHostBidTime();
			RefreshDownPanel();
			isAcutionSuccess = false;
			compareNpcOfferRate = Random.value;
			Debug.Log((object)$"本局npc跟价放弃概率:{currentNpcOfferRate},本次对比放弃概率随机数:{compareNpcOfferRate},已喊次数{npcBidCount},本剧最低喊价次数:{npcBidMin}");
		}
	}
}

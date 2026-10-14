using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Module.Mission;
using Module.UI;
using Spine.Unity;
using TMPro;
using TTSDK;
using UI.Component;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using cfg;

public class DownItem : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static UnityAction _003C_003E9__107_3;

		public static Action _003C_003E9__166_1;

		public static Action<int, string> _003C_003E9__166_2;

		public static Action<GameObject> _003C_003E9__172_0;

		public static Action _003C_003E9__176_6;

		public static Action _003C_003E9__178_0;

		public static Action _003C_003E9__182_0;

		internal void _003CAwake_003Eb__107_3()
		{
			WorldCupDataManager.Instance.HandleWorldCupSignClick();
		}

		internal void _003CCheckSideUI_003Eb__166_1()
		{
		}

		internal void _003CCheckSideUI_003Eb__166_2(int code, string msg)
		{
			Debug.LogError((object)$"检查侧边栏失败，错误码：{code}，错误信息：{msg}");
		}

		internal void _003CCreatRoleAD_003Eb__172_0(GameObject prefab)
		{
			GameObject panelByIndex = UpgradePanel.Instance.GetPanelByIndex(1);
			Object.Instantiate<GameObject>(prefab, panelByIndex.transform);
		}

		internal void _003CBtnInit_003Eb__176_6()
		{
		}

		internal void _003CChallengeStart_003Eb__178_0()
		{
			UIManager.Instance.HideDialogPanel();
		}

		internal void _003COpenChallenge_003Eb__182_0()
		{
			Time.timeScale = 1f;
			UIManager.Instance.HideDialogPanel();
		}
	}

	[Header("按钮组件")]
	private static string SideBAR_HAS_GET_KEY = "SideBAR_HAS_GET_M1B";

	[SerializeField]
	private Button earnMoneyButton;

	[SerializeField]
	private Button inspireButton;

	[SerializeField]
	private Button instantMoneyButton;

	[SerializeField]
	private Button gongfuButton;

	[SerializeField]
	private Button dasheButton;

	[SerializeField]
	private Button OnSideBtn;

	[SerializeField]
	private Button OnSubscribeBtn;

	[SerializeField]
	private Button OnDeskBtn;

	[Header("挑战入口按键")]
	[SerializeField]
	private Button challengeBtn;

	[Header("离开挑战按键")]
	[SerializeField]
	public Button closeChallengeBtn;

	[Header("计时文本组件")]
	[SerializeField]
	private CountdownTimer earnMoneyTimer;

	[SerializeField]
	private CountdownTimer inspireTimer;

	[SerializeField]
	private CountdownTimer instantMoneyTimer;

	[SerializeField]
	private CountdownTimer gongfuTimer;

	[SerializeField]
	private CountdownTimer dasheTimer;

	[Header("按钮文本")]
	[SerializeField]
	private TextMeshProUGUI earnMoneyButtonText;

	[SerializeField]
	private TextMeshProUGUI inspireButtonText;

	[SerializeField]
	private TextMeshProUGUI instantMoneyButtonText;

	[SerializeField]
	private TextMeshProUGUI gongfuButtonText;

	[SerializeField]
	private TextMeshProUGUI dasheButtonText;

	[SerializeField]
	private Image bgImage;

	[SerializeField]
	private Button expandButton;

	[SerializeField]
	private Image expandButtonImage;

	[SerializeField]
	private RectTransform container;

	[Header("桌面左侧按键,边栏,桌面,订阅面板父物体")]
	[SerializeField]
	private GameObject adSummonPanel;

	[Header("桌面右侧广告面板父物体")]
	[SerializeField]
	private GameObject adFoldPanel;

	[Header("挑战面板父物体")]
	[SerializeField]
	private GameObject challengePanel;

	[SerializeField]
	private bool updateCoinAvoidRects = true;

	[SerializeField]
	private float coinAvoidRectPadding = 30f;

	[Header("广告人物是否生成")]
	[SerializeField]
	private bool enableAdCharacter;

	[Header("挑战预制体")]
	private GameObject ChallengeUI;

	[Header("挑战胜利面板")]
	[SerializeField]
	private GameObject victoryPanel;

	[Header("挑战失败面板")]
	[SerializeField]
	private GameObject failPanel;

	[Header("道具图精灵列表")]
	[SerializeField]
	private List<Sprite> itemSprites;

	[Header("胜利面板遮罩")]
	[SerializeField]
	private Button victoryMask;

	[Header("失败面板遮罩")]
	[SerializeField]
	private Button failMask;

	[Header("挑战胜利按键确定")]
	[SerializeField]
	private Button victoryBtn;

	[Header("挑战胜利广告按键")]
	[SerializeField]
	private Button victoryAd;

	[Header("挑战失败确定按键")]
	[SerializeField]
	private Button failBtn;

	[SerializeField]
	private Transform victoryGrid;

	[Header("世界杯主要父物体")]
	[SerializeField]
	private GameObject worldCup;

	[Header("世界杯签到界面开启按键")]
	[SerializeField]
	private Button worldCupSignBtn;

	[Header("世界杯签到指引图标")]
	[SerializeField]
	public GameObject worldCupTip;

	[Header("世界杯入口图标红点")]
	[SerializeField]
	public GameObject worldCupRepoint;

	[Header("世界杯入口图标锁")]
	[SerializeField]
	public GameObject worldCupSuo;

	[Header("更多按键")]
	[SerializeField]
	public Button openBtn;

	[Header("收起按键")]
	[SerializeField]
	public Button foldBtn;

	[Header("右侧广告content")]
	[SerializeField]
	public RectTransform adSummonContent;

	[Header("右侧广告content背景")]
	[SerializeField]
	public RectTransform adSummonBg;

	[Header("右侧广告的遮罩物体")]
	[SerializeField]
	public RectTransform adMask;

	[Header("右侧广告的scrollrect物体")]
	[SerializeField]
	public ScrollRect scrollFrame;

	[Header("super666物体")]
	[SerializeField]
	public GameObject superSix;

	[Header("super666按键")]
	[SerializeField]
	public Button superBtn;

	[Header("super666进度文本")]
	[SerializeField]
	public TextMeshProUGUI superProgress;

	[Header("super666引导手指物体")]
	[SerializeField]
	public GameObject supperGuideObj;

	[Header("super666满进度特效物体")]
	[SerializeField]
	public GameObject jihuoEffect;

	[Header("钻石商人物体")]
	[SerializeField]
	public GameObject zuanshiShangRen;

	[Header("钻石商人进度图显示")]
	[SerializeField]
	public Image shangrenProgress;

	[Header("钻石商人按键点击")]
	[SerializeField]
	public Button shangrenBtn;

	[Header("钻石商人基础钻石范围（不含天赋加成，实际随机范围与预测范围一致）")]
	[SerializeField]
	private int shangRenDiamondMin = 1;

	[SerializeField]
	private int shangRenDiamondMax = 6;

	[Header("钻石商人预计获得数量文本")]
	[SerializeField]
	public TextMeshProUGUI predictNumTmp;

	[Header("钻石商人提示文本(倒计时/点击获取钻石)")]
	[SerializeField]
	public TextMeshProUGUI tipTmp;

	[Header("钻石商人特效")]
	[SerializeField]
	public SkeletonGraphic shangrenEffect;

	[Header("钻石商人钻石图片（今日达上限时隐藏）")]
	[SerializeField]
	private GameObject diamondImg;

	private bool challengeIsOpen;

	private bool challengeOffGm;

	private List<limitMission> limitList;

	private List<int> challengeRewardId = new List<int>();

	private List<double> challengeRewardCount = new List<double>();

	private float compliteTime = 5f;

	private ParticleSystem[] AdsEffect = (ParticleSystem[])(object)new ParticleSystem[3];

	private float[] AdsPlayTime = new float[3];

	private Dictionary<Button, bool> _buttonLogicStates = new Dictionary<Button, bool>();

	private bool _isExpanded;

	[SerializeField]
	private RectTransform guideFinger;

	private const string CHALLENGE_GUIDE_KEY = "ChallengeGuide_HasShown";

	private readonly List<Rect> _coinAvoidRects = new List<Rect>();

	private readonly Vector3[] _rectCorners = (Vector3[])(object)new Vector3[4];

	private bool isOpenSuperSix;

	private bool isOpenSuperAni;

	private const string supperGuide = "supperGuide";

	private float supperTime;

	private float supperjishi;

	private GameObject adLaoHuJi;

	private float shangRenTimer;

	private float shangRenTargetTime;

	private bool shangRenIsWaiting;

	private bool shangRenIsReady;

	private Vector3 _tipBaseScale = Vector3.one;

	private float _tipBreathTime;

	private int _lastPredictMin = -999;

	private int _lastPredictMax = -999;

	private int WorldIdMomentary;

	private const string SHANGREN_DATE_KEY = "ShangRen_LastDate";

	private DateTime _shangRenDateCheck;

	private GameObject diceWorldGuid;

	private AddDeskUI deskUI;

	private SideBarUI sideBarUI;

	private SubscribeUI subscribeUI;

	private static string Subscribe_HAS_GET_KEY = "Subscribe_HAS_GET_M1B";

	public event Action OnEarnMoneyClicked;

	public event Action OnInspireClicked;

	public event Action OnInstantMoneyClicked;

	private void Awake()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Expected O, but got Unknown
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Expected O, but got Unknown
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Expected O, but got Unknown
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Expected O, but got Unknown
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Expected O, but got Unknown
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Expected O, but got Unknown
		//IL_039e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Expected O, but got Unknown
		//IL_03ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c4: Expected O, but got Unknown
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Expected O, but got Unknown
		((Component)this).transform.SetAsFirstSibling();
		((UnityEvent)earnMoneyButton.onClick).AddListener(new UnityAction(HandleEarnMoneyClick));
		((UnityEvent)inspireButton.onClick).AddListener(new UnityAction(HandleInspireClick));
		((UnityEvent)instantMoneyButton.onClick).AddListener(new UnityAction(HandleInstantMoneyClick));
		((UnityEvent)gongfuButton.onClick).AddListener(new UnityAction(HandleGongfuClick));
		if ((Object)(object)dasheButton != (Object)null)
		{
			((UnityEvent)dasheButton.onClick).AddListener(new UnityAction(HandleDasheClick));
		}
		if ((Object)(object)gongfuTimer != (Object)null)
		{
			gongfuTimer.OnFinished += OnGongfuTimerFinished;
		}
		if ((Object)(object)dasheTimer != (Object)null)
		{
			dasheTimer.OnFinished += OnDasheTimerFinished;
		}
		if ((Object)(object)expandButton != (Object)null)
		{
			((UnityEvent)expandButton.onClick).AddListener((UnityAction)delegate
			{
				//IL_0046: Unknown result type (might be due to invalid IL or missing references)
				SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
				_isExpanded = !_isExpanded;
				((Component)expandButtonImage).transform.localScale = new Vector3(1f, (float)(_isExpanded ? 1 : (-1)), 1f);
				RefreshLayout();
			});
			((Component)expandButtonImage).transform.localScale = new Vector3(1f, (float)(_isExpanded ? 1 : (-1)), 1f);
		}
		if ((Object)(object)shangrenBtn != (Object)null)
		{
			((UnityEvent)shangrenBtn.onClick).AddListener(new UnityAction(HandleShangRenClick));
		}
		if ((Object)(object)zuanshiShangRen != (Object)null)
		{
			zuanshiShangRen.SetActive(false);
		}
		InitShangRenState();
		Button obj = earnMoneyButton;
		if (obj != null)
		{
			((Component)obj).gameObject.SetActive(false);
		}
		Button obj2 = inspireButton;
		if (obj2 != null)
		{
			((Component)obj2).gameObject.SetActive(false);
		}
		Button obj3 = instantMoneyButton;
		if (obj3 != null)
		{
			((Component)obj3).gameObject.SetActive(false);
		}
		Button obj4 = gongfuButton;
		if (obj4 != null)
		{
			((Component)obj4).gameObject.SetActive(false);
		}
		Button obj5 = dasheButton;
		if (obj5 != null)
		{
			((Component)obj5).gameObject.SetActive(false);
		}
		if ((Object)(object)guideFinger != (Object)null)
		{
			((Component)guideFinger).gameObject.SetActive(false);
		}
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			GameDataManager.Instance.OnWorldChanged += OnWorldChangedRefreshButtons;
		}
		if ((Object)(object)OnDeskBtn != (Object)null)
		{
			((UnityEvent)OnDeskBtn.onClick).AddListener(new UnityAction(OnDeskBtnClicked));
		}
		if ((Object)(object)OnSideBtn != (Object)null)
		{
			((UnityEvent)OnSideBtn.onClick).AddListener(new UnityAction(OnSideBtnClicked));
		}
		_ = (Object)(object)OnSubscribeBtn != (Object)null;
		SetButtonActive(OnDeskBtn, active: true);
		SetButtonActive(OnSideBtn, active: true);
		SetButtonActive(OnSubscribeBtn, active: false);
		CreatAdsEffect("Assets/GameRes/Role/Ads_zuanqian.prefab", 0);
		CreatAdsEffect("Assets/GameRes/Role/Ads_jiayou.prefab", 1);
		CreatAdsEffect("Assets/GameRes/Role/Ads_coinrain.prefab", 2);
		((UnityEvent)challengeBtn.onClick).AddListener((UnityAction)delegate
		{
			OpenChallenge();
		});
		((UnityEvent)closeChallengeBtn.onClick).AddListener((UnityAction)delegate
		{
			CloseChallenge();
		});
		MissionManager.Instance.AfterGoldReachUnlockEvent += OnAfterGoldReachUnlockEvent;
		if ((Object)(object)worldCupSignBtn != (Object)null)
		{
			ButtonClickedEvent onClick = worldCupSignBtn.onClick;
			object obj6 = _003C_003Ec._003C_003E9__107_3;
			if (obj6 == null)
			{
				UnityAction val = delegate
				{
					WorldCupDataManager.Instance.HandleWorldCupSignClick();
				};
				_003C_003Ec._003C_003E9__107_3 = val;
				obj6 = (object)val;
			}
			((UnityEvent)onClick).AddListener((UnityAction)obj6);
			((Component)worldCupSignBtn).gameObject.SetActive(false);
		}
		((UnityEvent)openBtn.onClick).AddListener((UnityAction)delegate
		{
			AdSummonDisplay(isOpen: true);
		});
		((UnityEvent)foldBtn.onClick).AddListener((UnityAction)delegate
		{
			AdSummonDisplay(isOpen: false);
		});
	}

	public void SetGongFuActive(bool isActive)
	{
		if (PlayerPrefs.GetInt("gongfu_ad_state", 0) == 2 || PlayerPrefs.GetInt("gongfu_support_time", 0) == 1)
		{
			((Component)gongfuButton).gameObject.SetActive(false);
			RefreshCoinAvoidRects();
			return;
		}
		((Component)gongfuButton).gameObject.SetActive(isActive);
		CountdownTimer countdownTimer = gongfuTimer;
		if (countdownTimer != null)
		{
			((Component)countdownTimer).gameObject.SetActive(isActive);
		}
		RefreshCoinAvoidRects();
	}

	public void SetDasheActive(bool isActive)
	{
		if (PlayerPrefs.GetInt("dashe_ad_state", 0) == 2 || PlayerPrefs.GetInt("dashe_support_time", 0) == 1)
		{
			((Component)dasheButton).gameObject.SetActive(false);
			RefreshCoinAvoidRects();
			return;
		}
		((Component)dasheButton).gameObject.SetActive(isActive);
		CountdownTimer countdownTimer = dasheTimer;
		if (countdownTimer != null)
		{
			((Component)countdownTimer).gameObject.SetActive(isActive);
		}
		RefreshCoinAvoidRects();
	}

	private void OnGongfuTimerFinished()
	{
		PlayerPrefs.SetInt("gongfu_ad_state", 2);
		PlayerPrefs.Save();
		SetGongFuActive(isActive: false);
	}

	private void OnDasheTimerFinished()
	{
		PlayerPrefs.SetInt("dashe_ad_state", 2);
		PlayerPrefs.Save();
		SetDasheActive(isActive: false);
	}

	public void TriggerGongfuPopupOnce()
	{
		if (PlayerPrefs.GetInt("gongfu_ad_state", 0) == 0 && PlayerPrefs.GetInt("gongfu_support_time", 0) != 1 && PlayerPrefs.GetInt("gongfu_popup_triggered", 0) == 0)
		{
			PlayerPrefs.SetInt("gongfu_popup_triggered", 1);
			PlayerPrefs.SetInt("gongfu_ad_state", 1);
			PlayerPrefs.SetString("gongfu_ad_start_time", ((long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds).ToString());
			PlayerPrefs.Save();
			HandleGongfuClick();
			AdSummonDisplay(isOpen: true);
		}
	}

	public void TriggerDashePopupOnce()
	{
		if (PlayerPrefs.GetInt("dashe_ad_state", 0) == 0 && PlayerPrefs.GetInt("dashe_support_time", 0) != 1 && PlayerPrefs.GetInt("dashe_popup_triggered", 0) == 0)
		{
			PlayerPrefs.SetInt("dashe_popup_triggered", 1);
			PlayerPrefs.SetInt("dashe_ad_state", 1);
			PlayerPrefs.SetString("dashe_ad_start_time", ((long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds).ToString());
			PlayerPrefs.Save();
			HandleDasheClick();
		}
	}

	private void Start()
	{
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Expected O, but got Unknown
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		BtnInit();
		RefreshAdFoldPanel(GameDataManager.Instance?.GetCurrentWorldId() ?? 1);
		CheckChallenge();
		RefreshChallenge();
		if ((Object)(object)MissionManager.Instance != (Object)null)
		{
			MissionManager.Instance.CheckLimitChallenge += CheckChallenge;
		}
		RefreshCoinAvoidRects();
		if ((Object)(object)openBtn != (Object)null && (Object)(object)foldBtn != (Object)null && (Object)(object)adSummonContent != (Object)null)
		{
			if (PlayerPrefs.GetInt("gongfu_popup_triggered", 0) == 1)
			{
				AdSummonDisplay(isOpen: true);
			}
			else
			{
				((Component)openBtn).gameObject.SetActive(false);
				((Component)foldBtn).gameObject.SetActive(false);
				((Component)adSummonBg).gameObject.SetActive(false);
			}
		}
		CheckSuperSix();
		((UnityEvent)superBtn.onClick).AddListener(new UnityAction(SupperBtn));
		supperTime = ConfigManager.Instance.GetCommonFactor(75);
		StartShangRenLogic();
		if ((Object)(object)UpgradePanel.Instance != (Object)null)
		{
			Transform transform = zuanshiShangRen.transform;
			Vector2 anchoredPosition = ((RectTransform)((transform is RectTransform) ? transform : null)).anchoredPosition;
			Transform transform2 = superSix.transform;
			Vector2 anchoredPosition2 = ((RectTransform)((transform2 is RectTransform) ? transform2 : null)).anchoredPosition;
			Vector3 topPos = UpgradePanel.Instance.GetTopPos();
			Vector3 val = Camera.main.WorldToScreenPoint(topPos);
			Vector2 zero = Vector2.zero;
			if ((Object)(object)zuanshiShangRen.transform.parent != (Object)null)
			{
				Transform parent = zuanshiShangRen.transform.parent;
				RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)(object)((parent is RectTransform) ? parent : null), Vector2.op_Implicit(val), Camera.main, ref zero);
				Transform transform3 = zuanshiShangRen.transform;
				((RectTransform)((transform3 is RectTransform) ? transform3 : null)).anchoredPosition = new Vector2(anchoredPosition.x, zero.y + 28f);
				Transform transform4 = superSix.transform;
				((RectTransform)((transform4 is RectTransform) ? transform4 : null)).anchoredPosition = new Vector2(anchoredPosition2.x, zero.y + 150f);
			}
		}
		int num = (int)ConfigManager.Instance.GetCommonFactor(59);
		int currentMainMissionLevel = GameDataManager.Instance.GetCurrentMainMissionLevel();
		if (GameDataManager.Instance.GetCheckInDay() >= 2 && currentMainMissionLevel <= num && PlayerPrefs.GetInt("IsUnlockDiceWorld", 0) == 0 && !HasPendingOfflineReward())
		{
			UIManager.Instance.ShowLv8DicePreview();
		}
		CheckOpenWorldGuid();
	}

	private void OnAfterGoldReachUnlockEvent(mainMission[] missions = null)
	{
		int currentMainMissionLevel = GameDataManager.Instance.GetCurrentMainMissionLevel();
		int num = (int)ConfigManager.Instance.GetCommonFactor(19);
		if (currentMainMissionLevel > num)
		{
			Button obj = earnMoneyButton;
			if (obj != null)
			{
				((Component)obj).gameObject.SetActive(true);
			}
			Button obj2 = inspireButton;
			if (obj2 != null)
			{
				((Component)obj2).gameObject.SetActive(true);
			}
			if ((GameDataManager.Instance?.GetCurrentWorldId() ?? 1) == 2)
			{
				AdSummonDisplay(isOpen: true);
			}
		}
		num = (int)ConfigManager.Instance.GetCommonFactor(58);
		if (currentMainMissionLevel > num)
		{
			Button obj3 = instantMoneyButton;
			if (obj3 != null)
			{
				((Component)obj3).gameObject.SetActive(true);
			}
		}
		if ((Object)(object)MissionManager.Instance != (Object)null && (Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
		{
			double totalGold = SingletonMono<UpgradeManager>.Instance.TotalGold;
			if (MissionManager.Instance.IsFactorConditionMet(31, totalGold))
			{
				TriggerGongfuPopupOnce();
			}
			if (MissionManager.Instance.IsFactorConditionMet(33, totalGold))
			{
				TriggerDashePopupOnce();
			}
		}
		int worldIdMomentary = GetWorldIdMomentary();
		WorldCupDataManager.Instance.CheckWorldCupSignBtn(worldCup, worldCupSignBtn, worldCupSuo, worldIdMomentary);
		RefreshCoinAvoidRects();
		WorldPreview();
		CheckOpenWorldGuid();
	}

	private void WorldPreview()
	{
		int num = (int)ConfigManager.Instance.GetCommonFactor(79);
		if (GameDataManager.Instance.GetCurrentMainMissionLevel() >= num && PlayerPrefs.GetInt("HasShownWorldFeaturePreview", 0) == 0 && !HasPendingOfflineReward())
		{
			PlayerPrefs.SetInt("HasShownWorldFeaturePreview", 1);
			PlayerPrefs.Save();
			UIManager.Instance.ShowLv8DicePreview();
		}
	}

	private bool HasPendingOfflineReward()
	{
		if (OfflineIncomeManager.HasInstance())
		{
			return OfflineIncomeManager.Instance.HasOfflineReward();
		}
		return false;
	}

	private void OnWorldChangedRefreshButtons(int previousWorldId, int currentWorldId)
	{
		int currentMainMissionLevel = GameDataManager.Instance.GetCurrentMainMissionLevel();
		int num = (int)ConfigManager.Instance.GetCommonFactor(19);
		bool active = currentMainMissionLevel > num;
		((Component)earnMoneyButton).gameObject.SetActive(active);
		((Component)inspireButton).gameObject.SetActive(active);
		int num2 = (int)ConfigManager.Instance.GetCommonFactor(58);
		((Component)instantMoneyButton).gameObject.SetActive(currentMainMissionLevel > num2);
		RefreshLayout();
		AdSummonDisplay(isOpen: true);
	}

	public void SetButtonActive(Button btn, bool active)
	{
		if (!((Object)(object)btn == (Object)null))
		{
			_buttonLogicStates[btn] = active;
			RefreshLayout();
		}
	}

	private void RefreshLayout()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		List<Button> list = new List<Button>();
		if ((Object)(object)container != (Object)null)
		{
			foreach (Transform item in (Transform)container)
			{
				Button component = ((Component)item).GetComponent<Button>();
				if ((Object)(object)component != (Object)null && _buttonLogicStates.ContainsKey(component) && _buttonLogicStates[component])
				{
					list.Add(component);
				}
			}
		}
		foreach (KeyValuePair<Button, bool> buttonLogicState in _buttonLogicStates)
		{
			if (buttonLogicState.Value && (Object)(object)buttonLogicState.Key != (Object)null && !list.Contains(buttonLogicState.Key))
			{
				list.Add(buttonLogicState.Key);
			}
		}
		int count = list.Count;
		foreach (KeyValuePair<Button, bool> buttonLogicState2 in _buttonLogicStates)
		{
			if ((Object)(object)buttonLogicState2.Key != (Object)null)
			{
				((Component)buttonLogicState2.Key).gameObject.SetActive(false);
			}
		}
		if (count == 0)
		{
			if ((Object)(object)bgImage != (Object)null)
			{
				((Component)bgImage).gameObject.SetActive(false);
			}
			if ((Object)(object)expandButton != (Object)null)
			{
				((Component)expandButton).gameObject.SetActive(false);
			}
			RefreshCoinAvoidRects();
			return;
		}
		if ((Object)(object)bgImage != (Object)null)
		{
			((Component)bgImage).gameObject.SetActive(true);
		}
		if (count <= 2)
		{
			foreach (Button item2 in list)
			{
				((Component)item2).gameObject.SetActive(true);
			}
			UpdateBgHeight(count);
		}
		else
		{
			int num = (_isExpanded ? count : 2);
			for (int i = 0; i < num; i++)
			{
				((Component)list[i]).gameObject.SetActive(true);
			}
			UpdateBgHeight(num);
		}
		RefreshCoinAvoidRects();
	}

	private void RefreshCoinAvoidRects()
	{
		_coinAvoidRects.Clear();
		MainScene mainScene = (((Object)(object)UIManager.Instance != (Object)null) ? UIManager.Instance.MainScene : null);
		MainSceneVat mainSceneVat = (((Object)(object)UIManager.Instance != (Object)null) ? UIManager.Instance.MainSceneVat : null);
		if (!((Object)(object)mainScene == (Object)null) || !((Object)(object)mainSceneVat == (Object)null))
		{
			if (!updateCoinAvoidRects)
			{
				mainScene?.SetCoinAvoidRects(_coinAvoidRects);
				mainSceneVat?.SetCoinAvoidRects(_coinAvoidRects);
				return;
			}
			AddAvoidRectIfVisible(earnMoneyButton);
			AddAvoidRectIfVisible(inspireButton);
			AddAvoidRectIfVisible(instantMoneyButton);
			AddAvoidRectIfVisible(gongfuButton);
			AddAvoidRectIfVisible(dasheButton);
			AddAvoidRectIfVisible(challengeBtn);
			AddAvoidRectIfVisible(OnSideBtn);
			AddAvoidRectIfVisible(OnDeskBtn);
			AddAvoidRectIfVisible(OnSubscribeBtn);
			mainScene?.SetCoinAvoidRects(_coinAvoidRects);
			mainSceneVat?.SetCoinAvoidRects(_coinAvoidRects);
		}
	}

	private void AddAvoidRectIfVisible(Button button)
	{
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)button == (Object)null || !((Component)button).gameObject.activeInHierarchy)
		{
			return;
		}
		Transform transform = ((Component)button).transform;
		RectTransform val = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		if (!((Object)(object)val == (Object)null))
		{
			val.GetWorldCorners(_rectCorners);
			float num = _rectCorners[0].x;
			float num2 = _rectCorners[0].y;
			float num3 = _rectCorners[0].x;
			float num4 = _rectCorners[0].y;
			for (int i = 1; i < _rectCorners.Length; i++)
			{
				num = Mathf.Min(num, _rectCorners[i].x);
				num2 = Mathf.Min(num2, _rectCorners[i].y);
				num3 = Mathf.Max(num3, _rectCorners[i].x);
				num4 = Mathf.Max(num4, _rectCorners[i].y);
			}
			Rect item = Rect.MinMaxRect(num - coinAvoidRectPadding, num2 - coinAvoidRectPadding, num3 + coinAvoidRectPadding, num4 + coinAvoidRectPadding);
			_coinAvoidRects.Add(item);
		}
	}

	private void UpdateBgHeight(int shownButtonCount)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)shownButtonCount * 200f + 34f;
		if ((Object)(object)bgImage != (Object)null)
		{
			RectTransform rectTransform = ((Graphic)bgImage).rectTransform;
			rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, num);
		}
		if ((Object)(object)expandButton != (Object)null)
		{
			RectTransform component = ((Component)expandButton).GetComponent<RectTransform>();
			if ((Object)(object)component != (Object)null)
			{
				component.anchoredPosition = new Vector2(component.anchoredPosition.x, 0f - num + 400f);
			}
		}
	}

	private void OnEnable()
	{
		float supportRemainingTime = GameDataManager.Instance.GetSupportRemainingTime(DialogType.incomeBuff);
		if (supportRemainingTime > 0f)
		{
			float commonFactor = ConfigManager.Instance.GetCommonFactor(11);
			Button obj = earnMoneyButton;
			if (obj != null)
			{
				((Component)obj).gameObject.SetActive(true);
			}
			earnMoneyTimer.StartCountdown(supportRemainingTime);
			SingletonMono<BuffManager>.Instance.AddBuff(BuffType.Income, commonFactor, supportRemainingTime);
		}
		supportRemainingTime = GameDataManager.Instance.GetSupportRemainingTime(DialogType.speedBuff);
		if (supportRemainingTime > 0f)
		{
			float commonFactor2 = ConfigManager.Instance.GetCommonFactor(8);
			Button obj2 = inspireButton;
			if (obj2 != null)
			{
				((Component)obj2).gameObject.SetActive(true);
			}
			inspireTimer.StartCountdown(supportRemainingTime);
			SingletonMono<BuffManager>.Instance.AddBuff(BuffType.Speed, commonFactor2, supportRemainingTime);
		}
		supportRemainingTime = GameDataManager.Instance.GetSupportRemainingTime(DialogType.money);
		if (supportRemainingTime > 0f)
		{
			Button obj3 = instantMoneyButton;
			if (obj3 != null)
			{
				((Component)obj3).gameObject.SetActive(true);
			}
			instantMoneyTimer.StartCountdown(supportRemainingTime);
		}
		int num = PlayerPrefs.GetInt("gongfu_ad_state", 0);
		int num2 = PlayerPrefs.GetInt("gongfu_support_time", 0);
		if (num == 2 || num2 == 1)
		{
			SetGongFuActive(isActive: false);
			if (num2 == 1)
			{
				UIManager.Instance.ActiveMainScene?.AddGongfu();
			}
		}
		else if (num == 1)
		{
			string text = PlayerPrefs.GetString("gongfu_ad_start_time", "");
			long result = 0L;
			if (!string.IsNullOrEmpty(text))
			{
				long.TryParse(text, out result);
			}
			long num3 = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
			float num4 = 300000f;
			float num5 = num3 - result;
			float num6 = (num4 - num5) / 1000f;
			if (num6 <= 0f)
			{
				PlayerPrefs.SetInt("gongfu_ad_state", 2);
				PlayerPrefs.Save();
				SetGongFuActive(isActive: false);
			}
			else
			{
				SetGongFuActive(isActive: true);
				if ((Object)(object)gongfuTimer != (Object)null && ((Component)gongfuTimer).gameObject.activeInHierarchy)
				{
					gongfuTimer.StartCountdown(num6);
				}
			}
		}
		int num7 = PlayerPrefs.GetInt("dashe_ad_state", 0);
		int num8 = PlayerPrefs.GetInt("dashe_support_time", 0);
		if (num7 == 2 || num8 == 1)
		{
			SetDasheActive(isActive: false);
			if (num8 == 1)
			{
				UIManager.Instance.ActiveMainScene?.AddDashe();
			}
		}
		else if (num7 == 1)
		{
			string text2 = PlayerPrefs.GetString("dashe_ad_start_time", "");
			long result2 = 0L;
			if (!string.IsNullOrEmpty(text2))
			{
				long.TryParse(text2, out result2);
			}
			long num9 = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
			float num10 = 300000f;
			float num11 = num9 - result2;
			float num12 = (num10 - num11) / 1000f;
			if (num12 <= 0f)
			{
				PlayerPrefs.SetInt("dashe_ad_state", 2);
				PlayerPrefs.Save();
				SetDasheActive(isActive: false);
			}
			else
			{
				SetDasheActive(isActive: true);
				if ((Object)(object)dasheTimer != (Object)null && ((Component)dasheTimer).gameObject.activeInHierarchy)
				{
					dasheTimer.StartCountdown(num12);
				}
			}
		}
		if ((Object)(object)MissionManager.Instance != (Object)null)
		{
			MissionManager.Instance.OnMissionCompleted += OnMissionCompleted;
		}
		int currentMainMissionLevel = GameDataManager.Instance.GetCurrentMainMissionLevel();
		int num13 = (int)ConfigManager.Instance.GetCommonFactor(19);
		if (currentMainMissionLevel > num13)
		{
			Button obj4 = earnMoneyButton;
			if (obj4 != null)
			{
				((Component)obj4).gameObject.SetActive(true);
			}
			Button obj5 = inspireButton;
			if (obj5 != null)
			{
				((Component)obj5).gameObject.SetActive(true);
			}
		}
		num13 = (int)ConfigManager.Instance.GetCommonFactor(58);
		if (currentMainMissionLevel > num13)
		{
			Button obj6 = instantMoneyButton;
			if (obj6 != null)
			{
				((Component)obj6).gameObject.SetActive(true);
			}
		}
		if ((Object)(object)MissionManager.Instance != (Object)null && (Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
		{
			double totalGold = SingletonMono<UpgradeManager>.Instance.TotalGold;
			if (MissionManager.Instance.IsFactorConditionMet(31, totalGold))
			{
				TriggerGongfuPopupOnce();
			}
			if (MissionManager.Instance.IsFactorConditionMet(33, totalGold))
			{
				TriggerDashePopupOnce();
			}
		}
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			GameDataManager.Instance.OnNewGuideUpdated += OnNewGuideUpdated;
		}
		if (GameDataManager.HasInstance())
		{
			GameDataManager.Instance.OnWorldChanged += OnWorldChangedHandler;
		}
		if (GameDataManager.HasInstance())
		{
			GameDataManager.Instance.OnWorldChanged += ExchangeWorldCheckChallenge;
		}
		if ((Object)(object)GameDataManager.Instance != (Object)null && GameDataManager.HasInstance())
		{
			GameDataManager.Instance.OnWorldChanged += ExchangeDisplayObj;
		}
		CheckUI();
		int worldIdMomentary = GetWorldIdMomentary();
		WorldCupDataManager.Instance.CheckWorldCupSignBtn(worldCup, worldCupSignBtn, worldCupSuo, worldIdMomentary);
		RefreshCoinAvoidRects();
		if ((Object)(object)zuanshiShangRen != (Object)null && zuanshiShangRen.activeSelf)
		{
			RefreshPredictNum();
		}
	}

	private void OnDisable()
	{
		if ((Object)(object)MissionManager.Instance != (Object)null)
		{
			MissionManager.Instance.OnMissionCompleted -= OnMissionCompleted;
		}
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			GameDataManager.Instance.OnNewGuideUpdated -= OnNewGuideUpdated;
		}
		_coinAvoidRects.Clear();
		if (UIManager.HasInstance())
		{
			UIManager.Instance?.MainScene?.SetCoinAvoidRects(_coinAvoidRects);
			UIManager.Instance?.MainSceneVat?.SetCoinAvoidRects(_coinAvoidRects);
		}
		if ((Object)(object)GameDataManager.Instance != (Object)null && GameDataManager.HasInstance())
		{
			GameDataManager.Instance.OnWorldChanged -= OnWorldChangedHandler;
		}
		if ((Object)(object)GameDataManager.Instance != (Object)null && GameDataManager.HasInstance())
		{
			GameDataManager.Instance.OnWorldChanged -= ExchangeWorldCheckChallenge;
		}
	}

	private void Update()
	{
		for (int i = 0; i < 3; i++)
		{
			if ((Object)(object)AdsEffect[i] != (Object)null && ((Component)AdsEffect[i]).gameObject.activeSelf)
			{
				AdsPlayTime[i] += Time.deltaTime;
				if (AdsPlayTime[i] >= 3f)
				{
					((Component)AdsEffect[i]).gameObject.SetActive(false);
					AdsPlayTime[i] = 0f;
				}
			}
		}
		if (victoryPanel.activeSelf || failPanel.activeSelf)
		{
			if (compliteTime > 0f)
			{
				compliteTime -= Time.deltaTime;
			}
			else if (compliteTime <= 0f)
			{
				if (victoryPanel.activeSelf)
				{
					ButtonClickedEvent onClick = victoryMask.onClick;
					if (onClick != null)
					{
						((UnityEvent)onClick).Invoke();
					}
				}
				else if (failPanel.activeSelf)
				{
					ButtonClickedEvent onClick2 = failMask.onClick;
					if (onClick2 != null)
					{
						((UnityEvent)onClick2).Invoke();
					}
				}
				compliteTime = 5f;
			}
		}
		if (isOpenSuperSix && supperjishi <= supperTime)
		{
			supperjishi += Time.deltaTime;
			RefreshSupperSixPorgress();
			if (supperjishi >= supperTime)
			{
				supperjishi = 0f;
				isOpenSuperSix = false;
				if (jihuoEffect.activeSelf)
				{
					jihuoEffect.SetActive(false);
				}
				RefreshSupperSixPorgress();
			}
		}
		UpdateShangRenTimer();
		if (shangRenIsReady)
		{
			UpdateTipBreath();
		}
	}

	private void OnMissionCompleted(mainMission missionType)
	{
		int currentMainMissionLevel = GameDataManager.Instance.GetCurrentMainMissionLevel();
		int num = (int)ConfigManager.Instance.GetCommonFactor(19);
		if (currentMainMissionLevel > num)
		{
			Button obj = earnMoneyButton;
			if (obj != null)
			{
				((Component)obj).gameObject.SetActive(true);
			}
			Button obj2 = inspireButton;
			if (obj2 != null)
			{
				((Component)obj2).gameObject.SetActive(true);
			}
		}
		num = (int)ConfigManager.Instance.GetCommonFactor(58);
		if (currentMainMissionLevel > num)
		{
			Button obj3 = instantMoneyButton;
			if (obj3 != null)
			{
				((Component)obj3).gameObject.SetActive(true);
			}
		}
		if ((Object)(object)MissionManager.Instance != (Object)null && (Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
		{
			double totalGold = SingletonMono<UpgradeManager>.Instance.TotalGold;
			if (MissionManager.Instance.IsFactorConditionMet(31, totalGold))
			{
				TriggerGongfuPopupOnce();
			}
			if (MissionManager.Instance.IsFactorConditionMet(33, totalGold))
			{
				TriggerDashePopupOnce();
			}
		}
		int worldIdMomentary = GetWorldIdMomentary();
		WorldCupDataManager.Instance.CheckWorldCupSignBtn(worldCup, worldCupSignBtn, worldCupSuo, worldIdMomentary);
	}

	private void OnNewGuideUpdated()
	{
		if (PlayerPrefs.GetInt("guide_step_3", 0) == 1)
		{
			int currentMainMissionLevel = GameDataManager.Instance.GetCurrentMainMissionLevel();
			int num = (int)ConfigManager.Instance.GetCommonFactor(58);
			if (currentMainMissionLevel > num)
			{
				Button obj = instantMoneyButton;
				if (obj != null)
				{
					((Component)obj).gameObject.SetActive(true);
				}
			}
		}
		RefreshCoinAvoidRects();
	}

	private void HandleEarnMoneyClick()
	{
		float commonFactor = ConfigManager.Instance.GetCommonFactor(11);
		UIManager.Instance.ShowSupportUI(DialogType.incomeBuff, commonFactor, delegate
		{
			float seconds = ConfigManager.Instance.GetCommonFactor(12) * 60f;
			StartEarnMoneyCountdown(seconds);
			GameDataManager.Instance.RecordSupport(DialogType.incomeBuff);
			PlayAdsEffect(0);
			CreatRoleAD("Assets/GameRes/Role/roleAdCheeringteam_2.prefab");
		});
	}

	private void HandleInspireClick()
	{
		float commonFactor = ConfigManager.Instance.GetCommonFactor(8);
		UIManager.Instance.ShowSupportUI(DialogType.speedBuff, commonFactor, delegate
		{
			float num = ConfigManager.Instance.GetCommonFactor(9);
			if ((GameDataManager.Instance?.GetCurrentWorldId() ?? 1) == 2)
			{
				num += TalentDataManager.Instance?.GetDiceTalentBuff(304) ?? 0f;
			}
			float seconds = num * 60f;
			StartInspireCountdown(seconds);
			GameDataManager.Instance.RecordSupport(DialogType.speedBuff);
			PlayAdsEffect(1);
			CreatRoleAD("Assets/GameRes/Role/roleAdCheeringteam_1.prefab");
		});
	}

	private void HandleInstantMoneyClick()
	{
		SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
		UIManager.Instance.ShowSupportUI(DialogType.money, 0.0, delegate
		{
			float num = ConfigManager.Instance.GetCommonFactor(24);
			if (num <= 0f)
			{
				num = 1f;
			}
			float seconds = num * 60f;
			StartInstantMoneyCountdown(seconds);
			GameDataManager.Instance.RecordSupport(DialogType.money);
			PlayAdsEffect(2);
			CreatRoleAD("Assets/GameRes/Role/roleMillionaire.prefab");
		});
	}

	private void HandleGongfuClick()
	{
		SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
		UIManager.Instance.ShowSupportUI(DialogType.gongfu, 30.0, delegate
		{
			PlayerPrefs.SetInt("gongfu_ad_state", 2);
			PlayerPrefs.Save();
			StopGongfuCountdown();
			SetGongFuActive(isActive: false);
		}, delegate
		{
			if (PlayerPrefs.GetInt("gongfu_ad_state", 0) == 1)
			{
				string text = PlayerPrefs.GetString("gongfu_ad_start_time", "");
				long result = 0L;
				if (!string.IsNullOrEmpty(text))
				{
					long.TryParse(text, out result);
				}
				long num = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
				float num2 = 300000f;
				float num3 = num - result;
				float num4 = (num2 - num3) / 1000f;
				if (num4 > 0f)
				{
					SetGongFuActive(isActive: true);
					StartGongfuCountdown(num4);
				}
				else
				{
					PlayerPrefs.SetInt("gongfu_ad_state", 2);
					PlayerPrefs.Save();
					SetGongFuActive(isActive: false);
				}
			}
		});
	}

	private void HandleDasheClick()
	{
		SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
		UIManager.Instance.ShowSupportUI(DialogType.dashe, 30.0, delegate
		{
			PlayerPrefs.SetInt("dashe_ad_state", 2);
			PlayerPrefs.Save();
			StopDasheCountdown();
			SetDasheActive(isActive: false);
		}, delegate
		{
			if (PlayerPrefs.GetInt("dashe_ad_state", 0) == 1)
			{
				string text = PlayerPrefs.GetString("dashe_ad_start_time", "");
				long result = 0L;
				if (!string.IsNullOrEmpty(text))
				{
					long.TryParse(text, out result);
				}
				long num = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
				float num2 = 300000f;
				float num3 = num - result;
				float num4 = (num2 - num3) / 1000f;
				if (num4 > 0f)
				{
					SetDasheActive(isActive: true);
					StartDasheCountdown(num4);
				}
				else
				{
					PlayerPrefs.SetInt("dashe_ad_state", 2);
					PlayerPrefs.Save();
					SetDasheActive(isActive: false);
				}
			}
		});
	}

	public void SetEarnMoneyButtonActive(bool active)
	{
	}

	public void SetInspireButtonActive(bool active)
	{
	}

	public void SetInstantMoneyButtonActive(bool active)
	{
	}

	public void StartEarnMoneyCountdown(float seconds)
	{
		earnMoneyTimer?.StartCountdown(seconds);
	}

	public void StartInspireCountdown(float seconds)
	{
		inspireTimer?.StartCountdown(seconds);
	}

	public void StartInstantMoneyCountdown(float seconds)
	{
		instantMoneyTimer?.StartCountdown(seconds);
	}

	public void StartGongfuCountdown(float seconds)
	{
		gongfuTimer?.StartCountdown(seconds);
	}

	public void StartDasheCountdown(float seconds)
	{
		dasheTimer?.StartCountdown(seconds);
	}

	public void StopEarnMoneyCountdown()
	{
		earnMoneyTimer?.Stop();
	}

	public void StopInspireCountdown()
	{
		inspireTimer?.Stop();
	}

	public void StopInstantMoneyCountdown()
	{
		instantMoneyTimer?.Stop();
	}

	public void StopGongfuCountdown()
	{
		gongfuTimer?.Stop();
	}

	public void StopDasheCountdown()
	{
		dasheTimer?.Stop();
	}

	public void SetEarnMoneyButtonText(string text)
	{
		if ((Object)(object)earnMoneyButtonText != (Object)null)
		{
			((TMP_Text)earnMoneyButtonText).text = text;
		}
	}

	public void SetInspireButtonText(string text)
	{
		if ((Object)(object)inspireButtonText != (Object)null)
		{
			((TMP_Text)inspireButtonText).text = text;
		}
	}

	public void SetInstantMoneyButtonText(string text)
	{
		if ((Object)(object)instantMoneyButtonText != (Object)null)
		{
			((TMP_Text)instantMoneyButtonText).text = text;
		}
	}

	public void SetDasheButtonText(string text)
	{
		if ((Object)(object)dasheButtonText != (Object)null)
		{
			((TMP_Text)dasheButtonText).text = text;
		}
	}

	private void OnDestroy()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Expected O, but got Unknown
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Expected O, but got Unknown
		((UnityEvent)earnMoneyButton.onClick).RemoveListener(new UnityAction(HandleEarnMoneyClick));
		((UnityEvent)inspireButton.onClick).RemoveListener(new UnityAction(HandleInspireClick));
		((UnityEvent)instantMoneyButton.onClick).RemoveListener(new UnityAction(HandleInstantMoneyClick));
		if ((Object)(object)gongfuButton != (Object)null)
		{
			((UnityEvent)gongfuButton.onClick).RemoveListener(new UnityAction(HandleGongfuClick));
		}
		if ((Object)(object)dasheButton != (Object)null)
		{
			((UnityEvent)dasheButton.onClick).RemoveListener(new UnityAction(HandleDasheClick));
		}
		if ((Object)(object)gongfuTimer != (Object)null)
		{
			gongfuTimer.OnFinished -= OnGongfuTimerFinished;
		}
		if ((Object)(object)dasheTimer != (Object)null)
		{
			dasheTimer.OnFinished -= OnDasheTimerFinished;
		}
		if ((Object)(object)MissionManager.Instance != (Object)null)
		{
			MissionManager.Instance.CheckLimitChallenge -= CheckChallenge;
		}
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			GameDataManager.Instance.OnWorldChanged -= OnWorldChangedRefreshButtons;
		}
		if ((Object)(object)OnDeskBtn != (Object)null)
		{
			((UnityEvent)OnDeskBtn.onClick).RemoveListener(new UnityAction(OnDeskBtnClicked));
		}
		if ((Object)(object)OnSideBtn != (Object)null)
		{
			((UnityEvent)OnSideBtn.onClick).RemoveListener(new UnityAction(OnSideBtnClicked));
		}
	}

	private void OnDeskBtnClicked()
	{
		ShowDeskUI();
	}

	private void ShowDeskUI()
	{
		if ((Object)(object)deskUI == (Object)null)
		{
			LoadDesk();
			return;
		}
		deskUI.OpenDesk();
		TT.CheckShortcut((Action<bool>)delegate(bool isExist)
		{
			if (isExist)
			{
				Debug.Log((object)"已存在快捷方式");
				SetButtonActive(OnDeskBtn, active: false);
			}
		});
	}

	private void LoadDesk()
	{
		string assetPath = "Assets/GameRes/NewUI/fufang/prefabs/add2DeskPanel.prefab";
		if (SingletonMono<ResManager>.Instance.IsAssetLoaded(assetPath) == 1)
		{
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(assetPath, delegate(GameObject go)
		{
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)go == (Object)null))
			{
				Transform layerTransform = UIManager.Instance.GetLayerTransform(UILayer.Popup);
				deskUI = Object.Instantiate<GameObject>(go).GetComponent<AddDeskUI>();
				((Component)deskUI).transform.SetParent(layerTransform);
				Transform transform = ((Component)deskUI).transform;
				((transform is RectTransform) ? transform : null).localScale = Vector3.one;
				Transform transform2 = ((Component)deskUI).transform;
				((RectTransform)((transform2 is RectTransform) ? transform2 : null)).anchorMin = Vector2.zero;
				Transform transform3 = ((Component)deskUI).transform;
				((RectTransform)((transform3 is RectTransform) ? transform3 : null)).anchorMax = Vector2.one;
				Transform transform4 = ((Component)deskUI).transform;
				((RectTransform)((transform4 is RectTransform) ? transform4 : null)).offsetMin = Vector2.zero;
				Transform transform5 = ((Component)deskUI).transform;
				((RectTransform)((transform5 is RectTransform) ? transform5 : null)).offsetMax = Vector2.zero;
				ShowDeskUI();
			}
		});
	}

	private void OnSideBtnClicked()
	{
		ShowSideBarUI();
	}

	private void OnSubscribeBtnClicked()
	{
		ShowSubscribeUI();
	}

	private void ShowSideBarUI()
	{
		if ((Object)(object)sideBarUI == (Object)null)
		{
			LoadSideBar();
		}
		else
		{
			sideBarUI.OpenSideBar();
		}
	}

	private void LoadSideBar()
	{
		string assetPath = "Assets/GameRes/NewUI/fufang/prefabs/enterSidePanel.prefab";
		if (SingletonMono<ResManager>.Instance.IsAssetLoaded(assetPath) == 1)
		{
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(assetPath, delegate(GameObject go)
		{
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)go == (Object)null))
			{
				Transform layerTransform = UIManager.Instance.GetLayerTransform(UILayer.Popup);
				sideBarUI = Object.Instantiate<GameObject>(go).GetComponent<SideBarUI>();
				((Component)sideBarUI).transform.SetParent(layerTransform);
				((Component)sideBarUI).transform.localScale = Vector3.one;
				Transform transform = ((Component)sideBarUI).transform;
				((RectTransform)((transform is RectTransform) ? transform : null)).anchorMin = Vector2.zero;
				Transform transform2 = ((Component)sideBarUI).transform;
				((RectTransform)((transform2 is RectTransform) ? transform2 : null)).anchorMax = Vector2.one;
				Transform transform3 = ((Component)sideBarUI).transform;
				((RectTransform)((transform3 is RectTransform) ? transform3 : null)).offsetMin = Vector2.zero;
				Transform transform4 = ((Component)sideBarUI).transform;
				((RectTransform)((transform4 is RectTransform) ? transform4 : null)).offsetMax = Vector2.zero;
				ShowSideBarUI();
			}
		});
	}

	private void ShowSubscribeUI()
	{
		if ((Object)(object)subscribeUI == (Object)null)
		{
			LoadSubscribeUI();
		}
		else
		{
			subscribeUI.OpenSubscribeUI(CheckSideUI);
		}
	}

	private void LoadSubscribeUI()
	{
		string assetPath = "Assets/GameRes/NewUI/fufang/prefabs/subscribePanel.prefab";
		if (SingletonMono<ResManager>.Instance.IsAssetLoaded(assetPath) == 1)
		{
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(assetPath, delegate(GameObject go)
		{
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)go == (Object)null))
			{
				Transform layerTransform = UIManager.Instance.GetLayerTransform(UILayer.Popup);
				subscribeUI = Object.Instantiate<GameObject>(go).GetComponent<SubscribeUI>();
				((Component)subscribeUI).transform.SetParent(layerTransform);
				Transform transform = ((Component)subscribeUI).transform;
				((RectTransform)((transform is RectTransform) ? transform : null)).anchorMin = Vector2.zero;
				Transform transform2 = ((Component)subscribeUI).transform;
				((RectTransform)((transform2 is RectTransform) ? transform2 : null)).anchorMax = Vector2.one;
				Transform transform3 = ((Component)subscribeUI).transform;
				((RectTransform)((transform3 is RectTransform) ? transform3 : null)).offsetMin = Vector2.zero;
				Transform transform4 = ((Component)subscribeUI).transform;
				((RectTransform)((transform4 is RectTransform) ? transform4 : null)).offsetMax = Vector2.zero;
				ShowSubscribeUI();
			}
		});
	}

	private void CheckUI()
	{
		CheckSideUI();
		CheckSubscribeUI();
		CheckOnDeskUI();
	}

	private void CheckSideUI()
	{
		if (PlayerPrefs.HasKey(SideBAR_HAS_GET_KEY))
		{
			Debug.Log((object)"已领取侧边栏奖励");
			SetButtonActive(OnSideBtn, active: false);
			return;
		}
		TT.CheckScene((SceneEnum)0, (Action<bool>)delegate(bool isSideBar)
		{
			if (isSideBar)
			{
				SetButtonActive(OnSideBtn, active: true);
			}
		}, (Action)delegate
		{
		}, (Action<int, string>)delegate(int code, string msg)
		{
			Debug.LogError((object)$"检查侧边栏失败，错误码：{code}，错误信息：{msg}");
		});
	}

	private void CheckOnDeskUI()
	{
		SetButtonActive(OnDeskBtn, active: false);
		try
		{
			TTSystemInfo systemInfo = TT.GetSystemInfo();
			Debug.Log((object)("抖音系统平台: " + systemInfo.platform));
			if (PlayerPrefs.HasKey("ADD_DESK_HAS_GET"))
			{
				Debug.Log((object)"已获取过奖励");
				SetButtonActive(OnDeskBtn, active: false);
			}
			else
			{
				SetButtonActive(OnDeskBtn, active: true);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("获取抖音系统平台失败: " + ex.Message));
		}
	}

	private void CheckSubscribeUI()
	{
		SetButtonActive(OnSubscribeBtn, active: false);
		try
		{
			if (PlayerPrefs.GetInt(Subscribe_HAS_GET_KEY, 0) == 2)
			{
				Debug.Log((object)"已获取过奖励");
				SetButtonActive(OnSubscribeBtn, active: false);
			}
			else
			{
				SetButtonActive(OnSubscribeBtn, active: false);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("OnSubscribeBtn: " + ex.Message));
		}
	}

	public void CreatAdsEffect(string path, int index)
	{
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(path, delegate(GameObject prefab)
		{
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			if ((((Object)(object)UIManager.Instance != (Object)null) ? UIManager.Instance.ActiveMainScene : null) != null)
			{
				GameObject val = Object.Instantiate<GameObject>(prefab, ((Component)this).transform);
				Transform transform = val.transform;
				((RectTransform)((transform is RectTransform) ? transform : null)).anchoredPosition = new Vector2(0f, 1300f);
				AdsEffect[index] = ((Component)val.transform.GetChild(0)).GetComponentInChildren<ParticleSystem>();
				ParticleSystem obj = AdsEffect[index];
				if (obj != null)
				{
					obj.Stop();
				}
				ParticleSystem obj2 = AdsEffect[index];
				if (obj2 != null)
				{
					obj2.Play();
				}
				AdsPlayTime[index] = 0f;
				ParticleSystem obj3 = AdsEffect[index];
				if (obj3 != null)
				{
					((Component)obj3).gameObject.SetActive(false);
				}
			}
		});
	}

	public void PlayAdsEffect(int index)
	{
		if ((Object)(object)AdsEffect[index] != (Object)null)
		{
			ParticleSystem obj = AdsEffect[index];
			if (obj != null)
			{
				obj.Stop();
			}
			ParticleSystem obj2 = AdsEffect[index];
			if (obj2 != null)
			{
				obj2.Play();
			}
			AdsPlayTime[index] = 0f;
			ParticleSystem obj3 = AdsEffect[index];
			if (obj3 != null)
			{
				((Component)obj3).gameObject.SetActive(true);
			}
		}
	}

	public void StopAdsEffect(int index)
	{
		if ((Object)(object)AdsEffect[index] != (Object)null)
		{
			ParticleSystem obj = AdsEffect[index];
			if (obj != null)
			{
				obj.Stop();
			}
			AdsPlayTime[index] = 0f;
			ParticleSystem obj2 = AdsEffect[index];
			if (obj2 != null)
			{
				((Component)obj2).gameObject.SetActive(false);
			}
		}
	}

	public void CreatRoleAD(string path)
	{
		if (enableAdCharacter)
		{
			SingletonMono<ResManager>.Instance.LoadPrefabAsync(path, delegate(GameObject prefab)
			{
				GameObject panelByIndex = UpgradePanel.Instance.GetPanelByIndex(1);
				Object.Instantiate<GameObject>(prefab, panelByIndex.transform);
			});
		}
	}

	public void CheckChallenge()
	{
		if (!((Object)(object)GameDataManager.Instance != (Object)null))
		{
			return;
		}
		int challengeId = GameDataManager.Instance.GetChallengeId();
		if (challengeId < 0)
		{
			challengeIsOpen = false;
			CloseChallenge();
			ChallengeOff();
			return;
		}
		double num = 0.0;
		int currentMainMissionLevel = GameDataManager.Instance.GetCurrentMainMissionLevel();
		if (limitList != null)
		{
			for (int i = 0; i < limitList.Count; i++)
			{
				if (limitList[i].Id == challengeId)
				{
					num = limitList[i].LimitMissionCondition;
					break;
				}
			}
		}
		if (!challengeOffGm)
		{
			if ((double)currentMainMissionLevel >= num)
			{
				ChallengeStart();
			}
			else
			{
				ChallengeOff();
			}
		}
	}

	private void ExchangeWorldCheckChallenge(int sourceId, int challengeId)
	{
		CloseChallenge();
		CheckChallenge();
	}

	private void ExchangeDisplayObj(int sourceId, int challengeId)
	{
		switch (challengeId)
		{
		case 1:
			superSix.SetActive(false);
			zuanshiShangRen.SetActive(false);
			break;
		case 2:
		{
			float num = SingletonMono<UpgradeManager>.Instance?.GetItemUpgrade3Effect(106) ?? 0f;
			if ((Object)(object)superSix != (Object)null)
			{
				superSix.SetActive(num > 0f);
			}
			float num2 = SingletonMono<UpgradeManager>.Instance?.GetItemUpgrade3Effect(107) ?? 0f;
			if ((Object)(object)zuanshiShangRen != (Object)null)
			{
				zuanshiShangRen.SetActive(num2 > 0f);
			}
			break;
		}
		}
	}

	public void BtnInit()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected O, but got Unknown
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Expected O, but got Unknown
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		((UnityEvent)victoryMask.onClick).AddListener((UnityAction)delegate
		{
			victoryPanel.SetActive(false);
			if (challengeRewardId.Count > 0 && challengeRewardCount.Count > 0)
			{
				for (int i = 0; i < challengeRewardId.Count; i++)
				{
					if (challengeRewardId[i] == 1)
					{
						SingletonMono<UpgradeManager>.Instance?.AddGold(challengeRewardCount[i]);
					}
					else if (challengeRewardId[i] == 2)
					{
						GameDataManager.Instance?.AddDiamond((long)challengeRewardCount[i]);
					}
					else if (challengeRewardId[i] == 6)
					{
						GameDataManager.Instance.ChangeTalentPoint(isAdd: true, (int)challengeRewardCount[i]);
					}
					else
					{
						GameDataManager.Instance.AddItem(challengeRewardId[i], (long)challengeRewardCount[i]);
					}
				}
				UIManager.Instance.ShowCoinDimondTween(challengeRewardId);
			}
			CheckChallenge();
		});
		((UnityEvent)failMask.onClick).AddListener((UnityAction)delegate
		{
			failPanel.SetActive(false);
		});
		limitList = new List<limitMission>();
		if ((Object)(object)ConfigManager.Instance != (Object)null)
		{
			limitList = ConfigManager.Instance.GetAllLimitMission();
		}
		((UnityEvent)victoryBtn.onClick).AddListener((UnityAction)delegate
		{
			ButtonClickedEvent onClick = victoryMask.onClick;
			if (onClick != null)
			{
				((UnityEvent)onClick).Invoke();
			}
		});
		((UnityEvent)victoryAd.onClick).AddListener((UnityAction)delegate
		{
			FakeVideoManager.Instance.ShowAd(delegate
			{
				victoryPanel.SetActive(false);
				if (challengeRewardId.Count > 0 && challengeRewardCount.Count > 0)
				{
					for (int i = 0; i < challengeRewardId.Count; i++)
					{
						if (challengeRewardId[i] == 1)
						{
							SingletonMono<UpgradeManager>.Instance?.AddGold(challengeRewardCount[i]);
						}
						else if (challengeRewardId[i] == 2)
						{
							GameDataManager.Instance?.AddDiamond((long)challengeRewardCount[i]);
						}
						else if (challengeRewardId[i] == 6)
						{
							GameDataManager.Instance.ChangeTalentPoint(isAdd: true, (int)challengeRewardCount[i]);
						}
						else
						{
							GameDataManager.Instance.AddItem(challengeRewardId[i], (long)challengeRewardCount[i]);
						}
					}
					UIManager.Instance.ShowCoinDimondTween(challengeRewardId);
				}
			}, delegate
			{
			}, "限时挑战奖励双倍");
		});
		((UnityEvent)failBtn.onClick).AddListener((UnityAction)delegate
		{
			ButtonClickedEvent onClick = failMask.onClick;
			if (onClick != null)
			{
				((UnityEvent)onClick).Invoke();
			}
		});
	}

	public bool GetChallengeIsOpen()
	{
		if (challengeOffGm)
		{
			return false;
		}
		return challengeIsOpen;
	}

	public void ChallengeStart()
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		challengeIsOpen = true;
		((Component)challengeBtn).gameObject.SetActive(true);
		UIManager.Instance.topPanel.HideMian();
		UIManager.Instance.topPanel.DisplayMainLevelBg();
		if ((Object)(object)guideFinger != (Object)null && PlayerPrefs.GetInt("ChallengeGuide_HasShown", 0) == 0)
		{
			((Component)guideFinger).gameObject.SetActive(true);
			((Component)guideFinger).transform.position = ((Component)challengeBtn).transform.position + new Vector3(0f, -40f, 0f);
			UIManager.Instance.ShowDialogPanel("点击进入挑战,获得等级晋升!", DialogPosition.Top, 0.001f, delegate
			{
				UIManager.Instance.HideDialogPanel();
			});
		}
	}

	public void ChallengeStartGM()
	{
		challengeOffGm = false;
		challengeIsOpen = true;
		((Component)challengeBtn).gameObject.SetActive(true);
		UIManager.Instance.topPanel.HideMian();
		UIManager.Instance.topPanel.DisplayMainLevelBg();
	}

	public void ChallengeOffGm()
	{
		challengeOffGm = true;
		challengeIsOpen = false;
		CloseChallenge();
		ChallengeOff();
	}

	public void ChallengeOff()
	{
		challengeIsOpen = false;
		((Component)challengeBtn).gameObject.SetActive(false);
		UIManager.Instance?.topPanel?.DisplayMain();
		if ((Object)(object)guideFinger != (Object)null)
		{
			((Component)guideFinger).gameObject.SetActive(false);
		}
	}

	public void OpenChallenge()
	{
		if ((Object)(object)guideFinger != (Object)null && PlayerPrefs.GetInt("ChallengeGuide_HasShown", 0) == 0)
		{
			PlayerPrefs.SetInt("ChallengeGuide_HasShown", 1);
			PlayerPrefs.Save();
			((Component)guideFinger).gameObject.SetActive(false);
			Time.timeScale = 0f;
			UIManager.Instance.ShowDialogPanel("60秒内显示搜集足够多的金钱!", DialogPosition.Top, 0.001f, delegate
			{
				Time.timeScale = 1f;
				UIManager.Instance.HideDialogPanel();
			});
		}
		challengeRewardId.Clear();
		challengeRewardCount.Clear();
		adSummonPanel.SetActive(false);
		adFoldPanel.SetActive(false);
		RefreshCoinAvoidRects();
		if ((Object)(object)UIManager.Instance.topPanel != (Object)null)
		{
			UIManager.Instance.topPanel.SetPermanentPanel(isOpen: false);
		}
		((Component)challengeBtn).gameObject.SetActive(false);
		UIManager.Instance.topPanel.HideMainLevelBg();
		((Component)closeChallengeBtn).gameObject.SetActive(true);
		if ((Object)(object)ChallengeUI == (Object)null)
		{
			SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/NewUI/Challenge/prefabs/challengeUI.prefab", delegate(GameObject prefab)
			{
				ChallengeUI = Object.Instantiate<GameObject>(prefab, challengePanel.transform);
				if ((Object)(object)ChallengeUI != (Object)null)
				{
					ChallengeUI.GetComponent<ChallengeManager>().Init();
				}
				ChallengeUI.SetActive(true);
			});
		}
		else
		{
			if ((Object)(object)ChallengeUI != (Object)null)
			{
				ChallengeUI.GetComponent<ChallengeManager>().Init();
			}
			ChallengeUI.SetActive(true);
		}
	}

	public void CloseChallenge()
	{
		adSummonPanel.SetActive(true);
		RefreshAdFoldPanel(GameDataManager.Instance?.GetCurrentWorldId() ?? 1);
		RefreshCoinAvoidRects();
		if ((Object)(object)UIManager.Instance.topPanel != (Object)null)
		{
			UIManager.Instance.topPanel.SetPermanentPanel(isOpen: true);
		}
		((Component)challengeBtn).gameObject.SetActive(true);
		UIManager.Instance.topPanel.DisplayMainLevelBg();
		((Component)closeChallengeBtn).gameObject.SetActive(false);
		if ((Object)(object)ChallengeUI != (Object)null)
		{
			ChallengeUI.SetActive(false);
		}
	}

	public void ChallengeVictory(List<int> itemId, List<double> itemCount)
	{
		((Component)challengeBtn).gameObject.SetActive(false);
		UIManager.Instance.topPanel.HideMainLevelBg();
		for (int i = 0; i < victoryGrid.childCount; i++)
		{
			Object.Destroy((Object)(object)((Component)victoryGrid.GetChild(i)).gameObject);
		}
		compliteTime = 3f;
		victoryPanel.SetActive(true);
		List<item> allItem = ConfigManager.Instance.GetAllItem();
		int index = 0;
		for (int j = 0; j < allItem.Count; j++)
		{
			if (allItem[j].Id < 101)
			{
				index++;
			}
		}
		for (int k = 0; k < itemId.Count; k++)
		{
			int index2 = k;
			SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/NewUI/Common/prefabs/rewardKuang.prefab", delegate(GameObject prefab)
			{
				GameObject obj = Object.Instantiate<GameObject>(prefab, victoryGrid);
				Image component = ((Component)obj.transform.Find("itemIcon")).GetComponent<Image>();
				TextMeshProUGUI component2 = ((Component)obj.transform.Find("countTmp")).GetComponent<TextMeshProUGUI>();
				if (itemId[index2] > 0 && itemId[index2] < 101)
				{
					if ((Object)(object)component != (Object)null)
					{
						if (itemSprites.Count > itemId[index2] - 1)
						{
							component.sprite = itemSprites[itemId[index2] - 1];
						}
						else
						{
							component.sprite = itemSprites[0];
						}
						((Graphic)component).SetNativeSize();
					}
				}
				else if (itemId[index2] >= 101 && (Object)(object)component != (Object)null)
				{
					if (itemSprites.Count > itemId[index2] - 101)
					{
						component.sprite = itemSprites[index + itemId[index2] - 101];
					}
					else
					{
						component.sprite = itemSprites[0];
					}
					((Graphic)component).SetNativeSize();
				}
				if ((Object)(object)component2 != (Object)null)
				{
					((TMP_Text)component2).text = NumberFormatter.FormatNumber(itemCount[index2]);
				}
			});
			challengeRewardId.Add(itemId[k]);
			challengeRewardCount.Add(itemCount[k]);
		}
	}

	public void ChallengeFail()
	{
		compliteTime = 5f;
		failPanel.SetActive(true);
	}

	public void SetChallengeBtnActive(bool isOpen)
	{
		((Component)challengeBtn).gameObject.SetActive(isOpen);
		if (!isOpen)
		{
			UIManager.Instance.topPanel?.HideMainLevelBg();
		}
	}

	public void RefreshChallenge()
	{
		if (challengeIsOpen)
		{
			UIManager.Instance.topPanel?.HideMian();
			UIManager.Instance.topPanel?.DisplayMainLevelBg();
			if (((Component)closeChallengeBtn).gameObject.activeSelf)
			{
				adSummonPanel.SetActive(false);
				adFoldPanel.SetActive(false);
				if ((Object)(object)UIManager.Instance.topPanel != (Object)null)
				{
					UIManager.Instance.topPanel.SetPermanentPanel(isOpen: false);
					UIManager.Instance.topPanel.HideMainLevelBg();
				}
			}
		}
		else
		{
			((Component)challengeBtn).gameObject.SetActive(false);
			UIManager.Instance.topPanel?.DisplayMain();
		}
	}

	public void AdSummonDisplay(bool isOpen)
	{
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		if (((Transform)adSummonContent).childCount > 0)
		{
			for (int i = 0; i < ((Transform)adSummonContent).childCount; i++)
			{
				if (((Component)((Transform)adSummonContent).GetChild(i)).gameObject.activeSelf)
				{
					num++;
				}
			}
		}
		if (isOpen)
		{
			((Component)openBtn).gameObject.SetActive(false);
			((Component)foldBtn).gameObject.SetActive(true);
			((Component)adSummonContent).gameObject.SetActive(true);
			((Component)adSummonBg).gameObject.SetActive(true);
			LayoutRebuilder.ForceRebuildLayoutImmediate(adSummonContent);
			LayoutRebuilder.ForceRebuildLayoutImmediate(adSummonBg);
			LayoutRebuilder.ForceRebuildLayoutImmediate(adMask);
			Rect rect;
			if (num >= 4)
			{
				adMask.sizeDelta = new Vector2(adMask.sizeDelta.x, 760f);
			}
			else
			{
				RectTransform obj = adMask;
				float x = adMask.sizeDelta.x;
				rect = adSummonContent.rect;
				obj.sizeDelta = new Vector2(x, ((Rect)(ref rect)).height - 215f);
			}
			Transform transform = ((Component)openBtn).transform;
			Transform obj2 = ((transform is RectTransform) ? transform : null);
			float y = adMask.anchoredPosition.y;
			rect = adMask.rect;
			((RectTransform)obj2).anchoredPosition = new Vector2(0f, y - ((Rect)(ref rect)).height - 50f);
			Transform transform2 = ((Component)foldBtn).transform;
			Transform obj3 = ((transform2 is RectTransform) ? transform2 : null);
			float y2 = adMask.anchoredPosition.y;
			rect = adMask.rect;
			((RectTransform)obj3).anchoredPosition = new Vector2(0f, y2 - ((Rect)(ref rect)).height - 50f);
			RectTransform obj4 = adSummonBg;
			float x2 = adSummonBg.sizeDelta.x;
			rect = adMask.rect;
			obj4.sizeDelta = new Vector2(x2, ((Rect)(ref rect)).height + 120f);
			if ((Object)(object)scrollFrame != (Object)null)
			{
				scrollFrame.verticalNormalizedPosition = 1f;
				LayoutRebuilder.ForceRebuildLayoutImmediate(adSummonContent);
			}
		}
		else
		{
			((Component)openBtn).gameObject.SetActive(true);
			((Component)foldBtn).gameObject.SetActive(false);
			((Component)adSummonContent).gameObject.SetActive(false);
			((Component)adSummonBg).gameObject.SetActive(true);
			LayoutRebuilder.ForceRebuildLayoutImmediate(adSummonContent);
			LayoutRebuilder.ForceRebuildLayoutImmediate(adSummonBg);
			adSummonBg.sizeDelta = new Vector2(adSummonBg.sizeDelta.x, 85f);
			Transform transform3 = ((Component)openBtn).transform;
			((RectTransform)((transform3 is RectTransform) ? transform3 : null)).anchoredPosition = new Vector2(0f, -235f);
			Transform transform4 = ((Component)foldBtn).transform;
			((RectTransform)((transform4 is RectTransform) ? transform4 : null)).anchoredPosition = new Vector2(0f, -235f);
		}
		if (num <= 0)
		{
			((Component)openBtn).gameObject.SetActive(false);
			((Component)foldBtn).gameObject.SetActive(false);
			((Component)adSummonContent).gameObject.SetActive(false);
			((Component)adSummonBg).gameObject.SetActive(false);
		}
	}

	public void CheckSuperSix()
	{
		int num = GameDataManager.Instance?.GetCurrentWorldId() ?? 1;
		float num2 = SingletonMono<UpgradeManager>.Instance?.GetItemUpgrade3Effect(106) ?? 0f;
		if (num == 2 && num2 > 0f)
		{
			if ((Object)(object)superSix != (Object)null)
			{
				superSix.gameObject.SetActive(true);
			}
			float supperSixCount = GameDataManager.Instance.GetSupperSixCount();
			float supperSixTarget = GameDataManager.Instance.GetSupperSixTarget();
			if (!jihuoEffect.activeSelf && supperSixCount >= supperSixTarget)
			{
				jihuoEffect.SetActive(true);
			}
			RefreshSupperSixPorgress();
		}
		else if ((Object)(object)superSix != (Object)null)
		{
			superSix.gameObject.SetActive(false);
		}
	}

	public bool IsDispaySuperSix()
	{
		int num = GameDataManager.Instance?.GetCurrentWorldId() ?? 1;
		float num2 = SingletonMono<UpgradeManager>.Instance?.GetItemUpgrade3Effect(106) ?? 0f;
		if (num == 2 && num2 > 0f)
		{
			return true;
		}
		return false;
	}

	public void AddSuperSixProgress()
	{
		if (isOpenSuperSix || isOpenSuperAni)
		{
			return;
		}
		float supperSixCount = GameDataManager.Instance.GetSupperSixCount();
		float supperSixTarget = GameDataManager.Instance.GetSupperSixTarget();
		supperSixCount += 1f;
		if ((GameDataManager.Instance?.GetCurrentWorldId() ?? 1) == 2)
		{
			supperSixCount += TalentDataManager.Instance?.GetDiceTalentBuff(313) ?? 0f;
		}
		if (supperSixCount >= supperSixTarget)
		{
			supperSixCount = supperSixTarget;
			if (!jihuoEffect.activeSelf)
			{
				jihuoEffect.SetActive(true);
			}
			if (PlayerPrefs.GetInt("supperGuide", 0) < 2 && !supperGuideObj.activeSelf)
			{
				supperGuideObj.SetActive(true);
			}
		}
		GameDataManager.Instance.SetSupperSixCount(supperSixCount);
		RefreshSupperSixPorgress();
	}

	public void ResetSupperSixTarget()
	{
		int currentMainMissionLevel = GameDataManager.Instance.GetCurrentMainMissionLevel();
		Common commonConfig = ConfigManager.Instance.GetCommonConfig(73);
		Common commonConfig2 = ConfigManager.Instance.GetCommonConfig(74);
		if (currentMainMissionLevel > 0 && commonConfig != null && commonConfig2 != null)
		{
			GameDataManager.Instance.SetSupperSixTarget((float)currentMainMissionLevel * commonConfig.Factor + commonConfig2.Factor);
		}
		RefreshSupperSixPorgress();
	}

	public void RefreshSupperSixPorgress()
	{
		float supperSixCount = GameDataManager.Instance.GetSupperSixCount();
		float supperSixTarget = GameDataManager.Instance.GetSupperSixTarget();
		string arg = NumberFormatter.FormatNumber(supperSixCount);
		string arg2 = NumberFormatter.FormatNumber(supperSixTarget);
		if (isOpenSuperSix)
		{
			float num = Mathf.Max(0f, supperTime - supperjishi);
			((TMP_Text)superProgress).text = $"生效中：<color=#704137>{Mathf.Ceil(num)}</color>秒";
		}
		else
		{
			((TMP_Text)superProgress).text = $"<color=#704137>{arg}</color>/<color=#009E3A>{arg2}</color>";
		}
	}

	public void SupperBtn()
	{
		float supperSixCount = GameDataManager.Instance.GetSupperSixCount();
		float supperSixTarget = GameDataManager.Instance.GetSupperSixTarget();
		if (supperSixCount >= supperSixTarget)
		{
			if (!isOpenSuperSix)
			{
				isOpenSuperAni = true;
				if (supperGuideObj.activeSelf)
				{
					supperGuideObj.SetActive(false);
				}
				if (PlayerPrefs.GetInt("supperGuide", 0) < 2)
				{
					int num = PlayerPrefs.GetInt("supperGuide", 0);
					num++;
					PlayerPrefs.SetInt("supperGuide", num);
				}
				if ((Object)(object)adLaoHuJi == (Object)null)
				{
					SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/NewUI/Ad/perfabs/AdLaoHuJi.prefab", delegate(GameObject prefab)
					{
						Transform layerTransform = UIManager.Instance.GetLayerTransform(UILayer.Popup);
						if ((Object)(object)layerTransform != (Object)null)
						{
							adLaoHuJi = Object.Instantiate<GameObject>(prefab, layerTransform);
							adLaoHuJi.SetActive(true);
						}
					});
				}
				else
				{
					adLaoHuJi.transform.Find("laohujiEffect");
					adLaoHuJi.SetActive(true);
				}
			}
			else
			{
				BroadcastManager.ShowPopUpTip("超级666模式开启中");
			}
		}
		else
		{
			BroadcastManager.ShowPopUpTip("6点的投掷次数不足");
		}
	}

	public void SetIsOpenSuperAni(bool isOpen)
	{
		isOpenSuperAni = isOpen;
	}

	public void SetIsOpenSuperSix(bool isOpen)
	{
		isOpenSuperSix = isOpen;
		if (isOpen)
		{
			supperjishi = 0f;
		}
		RefreshSupperSixPorgress();
	}

	public bool GetIsOpenSuperSix()
	{
		return isOpenSuperSix;
	}

	public void SetSupperTime(float time)
	{
		supperTime = time;
	}

	private void InitShangRenState()
	{
		if (!((Object)(object)GameDataManager.Instance == (Object)null))
		{
			string text = DateTime.Now.ToString("yyyy-MM-dd");
			if (GameDataManager.Instance.GetShangRenLastDate() != text)
			{
				GameDataManager.Instance.ResetShangRenDailyData();
				GameDataManager.Instance.SetShangRenLastDate(text);
			}
			shangRenTimer = 0f;
			shangRenTargetTime = 0f;
			shangRenIsWaiting = false;
			shangRenIsReady = false;
			_shangRenDateCheck = DateTime.Now.Date;
			StopTipBreath();
		}
	}

	public void StartShangRenLogic()
	{
		if ((GameDataManager.Instance?.GetCurrentWorldId() ?? 1) != 2)
		{
			HideShangRen();
			return;
		}
		if ((SingletonMono<UpgradeManager>.Instance?.GetItemUpgrade3Effect(107) ?? 0f) <= 0f)
		{
			HideShangRen();
			return;
		}
		if (IsShangRenDailyLimitReached())
		{
			EnterShangRenLimitState();
			return;
		}
		long shangRenWaitStartTime = GameDataManager.Instance.GetShangRenWaitStartTime();
		if (shangRenWaitStartTime <= 0)
		{
			EnterReadyState();
		}
		else
		{
			ResumeShangRenWaitCycle(shangRenWaitStartTime);
		}
	}

	private bool IsShangRenDailyLimitReached()
	{
		int num = GameDataManager.Instance?.GetShangRenTodayDiamond() ?? 0;
		Common common = ConfigManager.Instance?.GetCommonConfig(71);
		int num2 = ((common != null) ? ((int)common.Factor) : 999);
		return num >= num2;
	}

	private void HideShangRen()
	{
		shangRenIsWaiting = false;
		shangRenIsReady = false;
		StopTipBreath();
		if ((Object)(object)zuanshiShangRen != (Object)null)
		{
			zuanshiShangRen.SetActive(false);
		}
	}

	private void EnterShangRenLimitState()
	{
		shangRenIsWaiting = false;
		shangRenIsReady = false;
		StopTipBreath();
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			GameDataManager.Instance.ClearShangRenWait();
		}
		if ((Object)(object)zuanshiShangRen != (Object)null)
		{
			zuanshiShangRen.SetActive(true);
		}
		if ((Object)(object)diamondImg != (Object)null)
		{
			diamondImg.SetActive(false);
		}
		if ((Object)(object)predictNumTmp != (Object)null)
		{
			((Component)predictNumTmp).gameObject.SetActive(false);
		}
		if ((Object)(object)shangrenProgress != (Object)null)
		{
			((Component)shangrenProgress).gameObject.SetActive(false);
		}
		if ((Object)(object)tipTmp != (Object)null)
		{
			((Component)tipTmp).gameObject.SetActive(true);
			((TMP_Text)tipTmp).text = "今日已达上限";
		}
	}

	private void StartShangRenWaitCycle()
	{
		Common common = ConfigManager.Instance?.GetCommonConfig(69);
		Common common2 = ConfigManager.Instance?.GetCommonConfig(70);
		if (common != null && common2 != null)
		{
			float factor = common.Factor;
			float factor2 = common2.Factor;
			shangRenTargetTime = Random.Range(factor, factor2);
			shangRenTimer = 0f;
			shangRenIsWaiting = true;
			shangRenIsReady = false;
			if ((Object)(object)GameDataManager.Instance != (Object)null)
			{
				long shangRenWaitStartTime = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
				GameDataManager.Instance.SetShangRenWaitStartTime(shangRenWaitStartTime);
				GameDataManager.Instance.SetShangRenWaitTarget(shangRenTargetTime);
			}
			StopTipBreath();
			ShowShangRenObject();
			if ((Object)(object)shangrenProgress != (Object)null)
			{
				((Component)shangrenProgress).gameObject.SetActive(false);
			}
			RefreshPredictNum();
			if ((Object)(object)tipTmp != (Object)null)
			{
				((TMP_Text)tipTmp).text = "剩余：" + Mathf.Ceil(shangRenTargetTime) + "秒";
			}
		}
	}

	private void ResumeShangRenWaitCycle(long waitStart)
	{
		float num = (((Object)(object)GameDataManager.Instance != (Object)null) ? GameDataManager.Instance.GetShangRenWaitTarget() : 0f);
		if (num <= 0f)
		{
			StartShangRenWaitCycle();
			return;
		}
		float num2 = (float)((long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds - waitStart) / 1000f;
		shangRenTargetTime = num;
		shangRenTimer = Mathf.Min(num2, num);
		shangRenIsWaiting = true;
		shangRenIsReady = false;
		StopTipBreath();
		ShowShangRenObject();
		if ((Object)(object)shangrenProgress != (Object)null)
		{
			((Component)shangrenProgress).gameObject.SetActive(false);
		}
		RefreshPredictNum();
		if (num2 >= num)
		{
			EnterReadyState();
		}
		else if ((Object)(object)tipTmp != (Object)null)
		{
			((TMP_Text)tipTmp).text = "剩余：" + Mathf.Ceil(num - shangRenTimer) + "秒";
		}
	}

	private void UpdateShangRenTimer()
	{
		DateTime date = DateTime.Now.Date;
		if (date != _shangRenDateCheck)
		{
			_shangRenDateCheck = date;
			string text = date.ToString("yyyy-MM-dd");
			if ((Object)(object)GameDataManager.Instance != (Object)null && GameDataManager.Instance.GetShangRenLastDate() != text)
			{
				InitShangRenState();
				StartShangRenLogic();
				return;
			}
		}
		if ((GameDataManager.Instance?.GetCurrentWorldId() ?? 1) == 2 && !((SingletonMono<UpgradeManager>.Instance?.GetItemUpgrade3Effect(107) ?? 0f) <= 0f) && !IsShangRenDailyLimitReached() && !shangRenIsReady && shangRenIsWaiting)
		{
			shangRenTimer += Time.deltaTime;
			if (shangRenTimer >= shangRenTargetTime)
			{
				EnterReadyState();
			}
			else if ((Object)(object)tipTmp != (Object)null)
			{
				((TMP_Text)tipTmp).text = "剩余：" + Mathf.Ceil(shangRenTargetTime - shangRenTimer) + "秒";
			}
		}
	}

	private void ShowShangRenObject()
	{
		if ((Object)(object)zuanshiShangRen != (Object)null)
		{
			zuanshiShangRen.SetActive(true);
			shangrenEffect.AnimationState.SetAnimation(0, "animation", false);
		}
		if ((Object)(object)diamondImg != (Object)null)
		{
			diamondImg.SetActive(true);
		}
		if ((Object)(object)predictNumTmp != (Object)null)
		{
			((Component)predictNumTmp).gameObject.SetActive(true);
		}
	}

	private void EnterReadyState()
	{
		shangRenIsWaiting = false;
		shangRenIsReady = true;
		shangRenTimer = 0f;
		if ((Object)(object)tipTmp != (Object)null)
		{
			((TMP_Text)tipTmp).text = "点击获取钻石";
		}
		StartTipBreath();
		ShowShangRenObject();
		if ((Object)(object)shangrenProgress != (Object)null)
		{
			((Component)shangrenProgress).gameObject.SetActive(false);
		}
		RefreshPredictNum();
	}

	private void RefreshPredictNum()
	{
		if (!((Object)(object)predictNumTmp == (Object)null))
		{
			int num = GameDataManager.Instance?.GetCurrentWorldId() ?? 1;
			int num2 = 0;
			if (num == 2)
			{
				num2 = (int)(TalentDataManager.Instance?.GetDiceTalentBuff(306) ?? 0f);
			}
			int num3 = shangRenDiamondMin + num2;
			int num4 = shangRenDiamondMax + num2;
			if (num3 != _lastPredictMin || num4 != _lastPredictMax)
			{
				_lastPredictMin = num3;
				_lastPredictMax = num4;
				((TMP_Text)predictNumTmp).text = $"{num3}~{num4}";
			}
		}
	}

	private void StartTipBreath()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)tipTmp == (Object)null))
		{
			_tipBaseScale = ((TMP_Text)tipTmp).transform.localScale;
			_tipBreathTime = 0f;
		}
	}

	private void UpdateTipBreath()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)tipTmp == (Object)null))
		{
			_tipBreathTime += Time.deltaTime;
			float num = 1f + 0.12f * Mathf.Sin(_tipBreathTime * 4f);
			((TMP_Text)tipTmp).transform.localScale = _tipBaseScale * num;
		}
	}

	private void StopTipBreath()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		_tipBreathTime = 0f;
		if ((Object)(object)tipTmp != (Object)null)
		{
			((TMP_Text)tipTmp).transform.localScale = _tipBaseScale;
		}
	}

	private void HandleShangRenClick()
	{
		if (!shangRenIsReady)
		{
			return;
		}
		int num = Random.Range(shangRenDiamondMin, shangRenDiamondMax + 1);
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			int num2 = GameDataManager.Instance?.GetCurrentWorldId() ?? 1;
			int num3 = num;
			if (num2 == 2)
			{
				num3 += (int)(TalentDataManager.Instance?.GetDiceTalentBuff(306) ?? 0f);
			}
			UIManager.Instance.ShowRewardBoxUI(new List<double> { 2.0, num3 }, isAddItem: true, isSuipian: false, "钻石商人触发时，获得双倍钻石");
			GameDataManager.Instance.AddShangRenTodayDiamond(num);
		}
		shangRenIsReady = false;
		StopTipBreath();
		if (!IsShangRenDailyLimitReached())
		{
			StartShangRenWaitCycle();
		}
		else
		{
			EnterShangRenLimitState();
		}
	}

	public Vector2 GetSupperUIPos()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)superSix != (Object)null)
		{
			Transform transform = superSix.transform;
			return ((RectTransform)((transform is RectTransform) ? transform : null)).anchoredPosition;
		}
		return Vector2.zero;
	}

	public Vector2 GetZuanShiUIPos()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)zuanshiShangRen != (Object)null)
		{
			Transform transform = zuanshiShangRen.transform;
			return ((RectTransform)((transform is RectTransform) ? transform : null)).anchoredPosition;
		}
		return Vector2.zero;
	}

	private void OnWorldChangedHandler(int exchangeId, int currentId)
	{
		WorldIdMomentary = currentId;
		RefreshAdFoldPanel(currentId);
		WorldCupDataManager.Instance.CheckWorldCupSignBtn(worldCup, worldCupSignBtn, worldCupSuo, currentId);
	}

	private void RefreshAdFoldPanel(int worldId)
	{
		if ((Object)(object)adFoldPanel != (Object)null)
		{
			adFoldPanel.SetActive(worldId != 2);
		}
	}

	public int GetWorldIdMomentary()
	{
		if (WorldIdMomentary <= 0)
		{
			WorldIdMomentary = GameDataManager.Instance?.GetCurrentWorldId() ?? 1;
		}
		return WorldIdMomentary;
	}

	public void CheckOpenWorldGuid()
	{
		int num = (int)ConfigManager.Instance.GetCommonFactor(59);
		if (GameDataManager.Instance.GetCurrentMainMissionLevel() <= num || PlayerPrefs.GetInt("IsUnlockDiceWorld", 0) != 0)
		{
			return;
		}
		WorldData worldData = GameDataManager.Instance.GetWorldData(2);
		if ((worldData != null && worldData.gameData != null && worldData.gameData.totalGold > 0.0) || !((Object)(object)diceWorldGuid == (Object)null))
		{
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/NewUI/Common/prefabs/diceWorldGuide.prefab", delegate(GameObject prefab)
		{
			Transform layerTransform = UIManager.Instance.GetLayerTransform(UILayer.Popup);
			diceWorldGuid = Object.Instantiate<GameObject>(prefab, layerTransform);
			if (OfflineIncomeManager.HasInstance() && OfflineIncomeManager.Instance.GetOffLineUIIsActive())
			{
				OfflineIncomeManager.Instance.CloseOfflineUI();
			}
		});
	}
}

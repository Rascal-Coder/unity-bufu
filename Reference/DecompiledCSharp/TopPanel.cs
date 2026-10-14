using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Module.Http;
using Module.Mission;
using TMPro;
using TTSDK;
using TTSDK.UNBridgeLib.LitJson;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using cfg;

public class TopPanel : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static OnGetUserInfoSuccessCallback _003C_003E9__36_3;

		public static OnGetUserInfoFailedCallback _003C_003E9__36_4;

		public static UnityAction _003C_003E9__36_0;

		public static UnityAction _003C_003E9__36_1;

		public static Action<bool, string> _003C_003E9__69_0;

		internal void _003CAwake_003Eb__36_0()
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Expected O, but got Unknown
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Expected O, but got Unknown
			object obj = _003C_003E9__36_3;
			if (obj == null)
			{
				OnGetUserInfoSuccessCallback val = delegate(ref TTUserInfo user)
				{
					Debug.Log((object)"获取抖音用户信息成功，打开排行榜");
					GameDataManager.Instance.SetUserInfo(user);
					UIManager.Instance.ShowRankUI();
				};
				_003C_003E9__36_3 = val;
				obj = (object)val;
			}
			object obj2 = _003C_003E9__36_4;
			if (obj2 == null)
			{
				OnGetUserInfoFailedCallback val2 = delegate
				{
					Debug.LogError((object)"获取抖音用户信息失败");
				};
				_003C_003E9__36_4 = val2;
				obj2 = (object)val2;
			}
			TT.GetUserInfo((OnGetUserInfoSuccessCallback)obj, (OnGetUserInfoFailedCallback)obj2);
		}

		internal void _003CAwake_003Eb__36_3(ref TTUserInfo user)
		{
			Debug.Log((object)"获取抖音用户信息成功，打开排行榜");
			GameDataManager.Instance.SetUserInfo(user);
			UIManager.Instance.ShowRankUI();
		}

		internal void _003CAwake_003Eb__36_4(string err)
		{
			Debug.LogError((object)"获取抖音用户信息失败");
		}

		internal void _003CAwake_003Eb__36_1()
		{
			UIManager.Instance.ShowInviteUI();
		}

		internal void _003CGetImRank_003Eb__69_0(bool isSuccess, string errMsg)
		{
			if (isSuccess)
			{
				Debug.Log((object)"GetImRankList success");
			}
			else
			{
				Debug.Log((object)("GetImRankList fail with error message: " + errMsg));
			}
		}
	}

	[Header("金钱显示")]
	[SerializeField]
	public Image[] goldCoinIcons;

	[SerializeField]
	public TextMeshProUGUI goldNumTmp;

	[SerializeField]
	public TextMeshProUGUI diamondNumTmp;

	[Header("金钱和钻石文本父物体")]
	[SerializeField]
	public GameObject money;

	[SerializeField]
	public GameObject diamond;

	[SerializeField]
	private TextMeshProUGUI levelTmp;

	[SerializeField]
	private TextMeshProUGUI taskTmp;

	[SerializeField]
	private TextMeshProUGUI averageIncome;

	[SerializeField]
	private Image taskProgress;

	[Header("奖励类型图标显示")]
	[SerializeField]
	private Image[] rewardTypeImgs;

	[SerializeField]
	private Button gmBtn;

	[SerializeField]
	private Button recoveryBtn;

	[SerializeField]
	private RectTransform tipRect;

	[SerializeField]
	private Button setupBtn;

	[Header("成长等级相关的父物体")]
	[SerializeField]
	private GameObject main;

	[Header("称号icon")]
	[SerializeField]
	private Image honor;

	[Header("称号字体")]
	[SerializeField]
	private TextMeshProUGUI honorTxt;

	[Header("主线等级相关物体")]
	[SerializeField]
	private GameObject mainLevelBg;

	[Header("签到红点物体")]
	[SerializeField]
	private GameObject checkRepoint;

	[Header("签到提示可解锁人物提示")]
	[SerializeField]
	private GameObject yindaotu;

	private GameObject setUpPanel;

	private int _gmClickCount;

	private const int GM_UNLOCK_CLICKS = 100;

	private const string IS_INTERNAL_USER_KEY = "is_internal_user";

	private bool _isInternalUser;

	[Header("签到入口按键")]
	[SerializeField]
	private RectTransform permanentPanel;

	[SerializeField]
	private Button checkInBtn;

	[SerializeField]
	private Button rankBtn;

	private Button worldBtn;

	[SerializeField]
	private Button inviteBtn;

	[SerializeField]
	private TextMeshProUGUI inviteTimeText;

	private TextMeshProUGUI fpsText;

	private float fpsElapsed;

	private float fpsFrameTime;

	private int fpsFrameCount;

	private const float FpsRefreshInterval = 0.5f;

	private HttpTestUI httpTestUI;

	private void Awake()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Expected O, but got Unknown
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Expected O, but got Unknown
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Expected O, but got Unknown
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Expected O, but got Unknown
		InitializeFpsDisplay();
		_isInternalUser = PlayerPrefs.GetInt("is_internal_user", 0) == 1;
		if (AppConst.DebugMode)
		{
			Button obj = gmBtn;
			if (obj != null)
			{
				((UnityEvent)obj.onClick).AddListener(new UnityAction(OnGMButtonClicked));
			}
		}
		if (_isInternalUser)
		{
			Debug.Log((object)"已检测到内部用户，GM功能已解锁");
		}
		if (rewardTypeImgs != null)
		{
			for (int i = 0; i < rewardTypeImgs.Length; i++)
			{
				Image obj2 = rewardTypeImgs[i];
				if (obj2 != null)
				{
					((Component)obj2).gameObject.SetActive(false);
				}
			}
		}
		TextMeshProUGUI obj3 = averageIncome;
		if (obj3 != null)
		{
			((Component)obj3).gameObject.SetActive(false);
		}
		((UnityEvent)recoveryBtn.onClick).AddListener(new UnityAction(HandleRecoveryClick));
		Button obj4 = rankBtn;
		if (obj4 != null)
		{
			ButtonClickedEvent onClick = obj4.onClick;
			object obj5 = _003C_003Ec._003C_003E9__36_0;
			if (obj5 == null)
			{
				UnityAction val = delegate
				{
					//IL_0014: Unknown result type (might be due to invalid IL or missing references)
					//IL_0019: Unknown result type (might be due to invalid IL or missing references)
					//IL_001f: Expected O, but got Unknown
					//IL_0033: Unknown result type (might be due to invalid IL or missing references)
					//IL_0038: Unknown result type (might be due to invalid IL or missing references)
					//IL_003e: Expected O, but got Unknown
					object obj8 = _003C_003Ec._003C_003E9__36_3;
					if (obj8 == null)
					{
						OnGetUserInfoSuccessCallback val3 = delegate(ref TTUserInfo user)
						{
							Debug.Log((object)"获取抖音用户信息成功，打开排行榜");
							GameDataManager.Instance.SetUserInfo(user);
							UIManager.Instance.ShowRankUI();
						};
						_003C_003Ec._003C_003E9__36_3 = val3;
						obj8 = (object)val3;
					}
					object obj9 = _003C_003Ec._003C_003E9__36_4;
					if (obj9 == null)
					{
						OnGetUserInfoFailedCallback val4 = delegate
						{
							Debug.LogError((object)"获取抖音用户信息失败");
						};
						_003C_003Ec._003C_003E9__36_4 = val4;
						obj9 = (object)val4;
					}
					TT.GetUserInfo((OnGetUserInfoSuccessCallback)obj8, (OnGetUserInfoFailedCallback)obj9);
				};
				_003C_003Ec._003C_003E9__36_0 = val;
				obj5 = (object)val;
			}
			((UnityEvent)onClick).AddListener((UnityAction)obj5);
		}
		Button obj6 = inviteBtn;
		if (obj6 != null)
		{
			ButtonClickedEvent onClick2 = obj6.onClick;
			object obj7 = _003C_003Ec._003C_003E9__36_1;
			if (obj7 == null)
			{
				UnityAction val2 = delegate
				{
					UIManager.Instance.ShowInviteUI();
				};
				_003C_003Ec._003C_003E9__36_1 = val2;
				obj7 = (object)val2;
			}
			((UnityEvent)onClick2).AddListener((UnityAction)obj7);
		}
		if ((Object)(object)inviteTimeText != (Object)null)
		{
			((TMP_Text)inviteTimeText).text = "";
		}
		((UnityEvent)checkInBtn.onClick).AddListener((UnityAction)delegate
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			UIManager.Instance.ShowCheckInPanel(isActive: true, Vector2.zero);
			SetYinDaoTu(isOpen: false);
		});
	}

	private void Update()
	{
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		if (AppConst.DebugMode && !((Object)(object)fpsText == (Object)null))
		{
			float unscaledDeltaTime = Time.unscaledDeltaTime;
			fpsElapsed += unscaledDeltaTime;
			fpsFrameTime += unscaledDeltaTime;
			fpsFrameCount++;
			if (!(fpsElapsed < 0.5f))
			{
				float num = ((fpsFrameCount > 0) ? (fpsFrameTime / (float)fpsFrameCount) : 0f);
				float num2 = ((num > 0f) ? (1f / num) : 0f);
				((TMP_Text)fpsText).text = $"FPS {num2:0.0}  {num * 1000f:0.0} ms";
				((Graphic)fpsText).color = ((num2 >= 50f) ? new Color(0.3f, 1f, 0.4f, 1f) : ((num2 >= 30f) ? new Color(1f, 0.85f, 0.2f, 1f) : new Color(1f, 0.3f, 0.25f, 1f)));
				fpsElapsed = 0f;
				fpsFrameTime = 0f;
				fpsFrameCount = 0;
			}
		}
	}

	private void InitializeFpsDisplay()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		if (AppConst.DebugMode && !((Object)(object)fpsText != (Object)null))
		{
			GameObject val = new GameObject("DebugFpsText");
			val.transform.SetParent(((Component)this).transform, false);
			fpsText = val.AddComponent<TextMeshProUGUI>();
			((Graphic)fpsText).raycastTarget = false;
			((TMP_Text)fpsText).fontSize = 34f;
			((TMP_Text)fpsText).fontStyle = (FontStyles)1;
			((TMP_Text)fpsText).alignment = (TextAlignmentOptions)258;
			((TMP_Text)fpsText).text = "FPS --";
			((Graphic)fpsText).color = Color.white;
			if ((Object)(object)goldNumTmp != (Object)null)
			{
				((TMP_Text)fpsText).font = ((TMP_Text)goldNumTmp).font;
			}
			RectTransform rectTransform = ((TMP_Text)fpsText).rectTransform;
			rectTransform.anchorMin = new Vector2(0.5f, 1f);
			rectTransform.anchorMax = new Vector2(0.5f, 1f);
			rectTransform.pivot = new Vector2(0.5f, 1f);
			rectTransform.anchoredPosition = new Vector2(-242f, -500f);
			rectTransform.sizeDelta = new Vector2(420f, 60f);
			val.transform.SetAsLastSibling();
		}
	}

	public void ShowTopBtn(bool isShow)
	{
		RectTransform obj = permanentPanel;
		if (obj != null)
		{
			((Component)obj).gameObject.SetActive(isShow);
		}
		if ((Object)(object)worldBtn != (Object)null)
		{
			((Component)worldBtn).gameObject.SetActive(isShow && IsWorldEntryUnlocked());
		}
	}

	private bool IsWorldEntryUnlocked()
	{
		if ((Object)(object)GameDataManager.Instance == (Object)null || !ConfigManager.HasInstance())
		{
			return false;
		}
		return GameDataManager.Instance.IsWorldEntryUnlocked();
	}

	private void HandleRecoveryClick()
	{
	}

	private void Start()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		Initialize();
		((Component)tipRect).gameObject.SetActive(false);
		((UnityEvent)setupBtn.onClick).AddListener((UnityAction)delegate
		{
			ShowSetUpPanel();
		});
		OnMissionLevelChanged();
		MissionManager.Instance.AfterGoldReachUnlockEvent += OnMissionLevelChanged;
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			GameDataManager.Instance.OnWorldChanged += OnWorldChanged;
		}
		RefreshCheckInRedDot();
		RefreshGoldCoinIcon();
	}

	private void RefreshGoldCoinIcon()
	{
		if (goldCoinIcons != null && goldCoinIcons.Length >= 2)
		{
			int num = ((!((Object)(object)GameDataManager.Instance != (Object)null)) ? 1 : GameDataManager.Instance.GetCurrentWorldId());
			if ((Object)(object)goldCoinIcons[0] != (Object)null)
			{
				((Component)goldCoinIcons[0]).gameObject.SetActive(num == 1);
			}
			if ((Object)(object)goldCoinIcons[1] != (Object)null)
			{
				((Component)goldCoinIcons[1]).gameObject.SetActive(num == 2);
			}
		}
	}

	private void OnWorldChanged(int previousWorldId, int currentWorldId)
	{
		OnMissionLevelChanged();
		RefreshCheckInRedDot();
		RefreshGoldCoinIcon();
		RefreshGoldDisplay();
	}

	private void OnMissionLevelChanged(mainMission[] missions = null)
	{
		int currentMainMissionLevel = GameDataManager.Instance.GetCurrentMainMissionLevel();
		int num = (int)ConfigManager.Instance.GetCommonFactor(28);
		if (currentMainMissionLevel > num)
		{
			((Component)checkInBtn).gameObject.SetActive(true);
		}
		else
		{
			((Component)checkInBtn).gameObject.SetActive(false);
		}
		num = (int)ConfigManager.Instance.GetCommonFactor(29);
		if (currentMainMissionLevel > num)
		{
			((Component)rankBtn).gameObject.SetActive(true);
		}
		else
		{
			((Component)rankBtn).gameObject.SetActive(false);
		}
		num = (int)ConfigManager.Instance.GetCommonFactor(30);
		if (currentMainMissionLevel > num)
		{
			((Component)inviteBtn).gameObject.SetActive(true);
		}
		else
		{
			((Component)inviteBtn).gameObject.SetActive(false);
		}
		List<mainMission> allMissions = MissionManager.Instance.GetAllMissions();
		if (allMissions == null || allMissions.Count == 0)
		{
			return;
		}
		int num2 = Mathf.Clamp(currentMainMissionLevel, 1, allMissions.Count);
		for (int i = 0; i < allMissions.Count; i++)
		{
			if (i + 1 != num2)
			{
				continue;
			}
			((TMP_Text)honorTxt).text = allMissions[i].TitleName;
			int picId = allMissions[i].PicId;
			SingletonMono<ResManager>.Instance.LoadSpriteAsync($"Assets/GameRes/honor/{picId}.png", delegate(Sprite sprite)
			{
				if ((Object)(object)sprite != (Object)null)
				{
					honor.sprite = sprite;
				}
			}, isAtlas: false);
			break;
		}
	}

	private void OnGMButtonClicked()
	{
		if (AppConst.DebugMode)
		{
			SingletonMono<GM>.Instance.ToggleGMUI(show: true);
		}
		else
		{
			_gmClickCount++;
		}
	}

	public void ShowTip(bool isShow)
	{
		if (GameDataManager.Instance.GetCurrentMainMissionLevel() > 1)
		{
			((Component)tipRect).gameObject.SetActive(false);
		}
		else
		{
			((Component)tipRect).gameObject.SetActive(isShow);
		}
	}

	private void Initialize()
	{
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null && SingletonMono<UpgradeManager>.Instance.IsInitialized)
		{
			SubscribeEvents();
			RefreshGoldDisplay();
		}
		else
		{
			((MonoBehaviour)this).Invoke("Initialize", 0.1f);
		}
	}

	public void OnShow()
	{
		((Component)this).gameObject.SetActive(true);
	}

	public void OnHide()
	{
		((Component)this).gameObject.SetActive(false);
	}

	public void SetInviteTimeText(string text)
	{
		if ((Object)(object)inviteTimeText != (Object)null)
		{
			((TMP_Text)inviteTimeText).text = text;
		}
	}

	private void SubscribeEvents()
	{
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
		{
			SingletonMono<UpgradeManager>.Instance.OnGoldChanged += OnGoldChanged;
		}
	}

	private void UnsubscribeEvents()
	{
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
		{
			SingletonMono<UpgradeManager>.Instance.OnGoldChanged -= OnGoldChanged;
		}
	}

	private void OnDestroy()
	{
		((MonoBehaviour)this).CancelInvoke();
		if ((Object)(object)MissionManager.Instance != (Object)null)
		{
			MissionManager.Instance.AfterGoldReachUnlockEvent -= OnMissionLevelChanged;
		}
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			GameDataManager.Instance.OnWorldChanged -= OnWorldChanged;
		}
		UnsubscribeEvents();
	}

	private void OnSoundUpClicked()
	{
		if ((Object)(object)SoundManager.Instance != (Object)null)
		{
			float bGMVolume = SoundManager.Instance.GetBGMVolume();
			SoundManager.Instance.SetBGMVolume(bGMVolume + 0.05f);
			bGMVolume = SoundManager.Instance.GetSoundVolume();
			SoundManager.Instance.SetSoundVolume(bGMVolume + 0.1f);
			Debug.Log((object)$"[TopPanel] 声音加：当前BGM音量 {SoundManager.Instance.GetBGMVolume()}，当前音音量 {SoundManager.Instance.GetSoundVolume()}");
		}
	}

	private void OnSoundDownClicked()
	{
		if ((Object)(object)SoundManager.Instance != (Object)null)
		{
			float bGMVolume = SoundManager.Instance.GetBGMVolume();
			SoundManager.Instance.SetBGMVolume(bGMVolume - 0.05f);
			bGMVolume = SoundManager.Instance.GetSoundVolume();
			SoundManager.Instance.SetSoundVolume(bGMVolume - 0.1f);
			Debug.Log((object)$"[TopPanel] 声音减：当前BGM音量 {SoundManager.Instance.GetBGMVolume()}，当前音音量 {SoundManager.Instance.GetSoundVolume()}");
		}
	}

	private void RefreshGoldDisplay()
	{
		if ((Object)(object)goldNumTmp == (Object)null)
		{
			return;
		}
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null && SingletonMono<UpgradeManager>.Instance.IsInitialized)
		{
			double totalGold = SingletonMono<UpgradeManager>.Instance.TotalGold;
			((TMP_Text)goldNumTmp).text = NumberFormatter.FormatNumber(totalGold);
			((TMP_Text)diamondNumTmp).text = NumberFormatter.FormatNumber(GameDataManager.Instance.GetDiamond());
			((TMP_Text)levelTmp).text = GameDataManager.Instance.GetCurrentMainMissionLevel().ToString();
			mainMission currentMission = MissionManager.Instance.GetCurrentMission();
			if (currentMission != null)
			{
				((TMP_Text)taskTmp).text = NumberFormatter.FormatNumber(totalGold) + "/" + NumberFormatter.FormatNumber(currentMission.MainMissionNum);
				taskProgress.fillAmount = (float)(totalGold / (currentMission.MainMissionNum + 0.0001));
			}
			else
			{
				((TMP_Text)taskTmp).text = "";
				taskProgress.fillAmount = 0f;
			}
		}
		else
		{
			((TMP_Text)goldNumTmp).text = "0";
			((TMP_Text)diamondNumTmp).text = "0";
			((TMP_Text)levelTmp).text = "0";
			((TMP_Text)taskTmp).text = "";
			taskProgress.fillAmount = 0f;
		}
	}

	public void HideMian()
	{
		main.SetActive(false);
		HideMainLevelBg();
	}

	public void DisplayMain()
	{
		main.SetActive(true);
		DisplayMainLevelBg();
	}

	public void HideMainLevelBg()
	{
		mainLevelBg.SetActive(false);
	}

	public void DisplayMainLevelBg()
	{
		mainLevelBg.SetActive(true);
	}

	public void SetPermanentPanel(bool isOpen)
	{
		((Component)permanentPanel).gameObject.SetActive(isOpen);
	}

	public void SetCheckRepoint(bool isOpen)
	{
		checkRepoint.SetActive(isOpen);
	}

	public void RefreshCheckInRedDot()
	{
		bool flag = false;
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			int[] clampItemInfo = GameDataManager.Instance.GetClampItemInfo();
			int checkInDay = GameDataManager.Instance.GetCheckInDay();
			if (clampItemInfo != null)
			{
				int num = 7;
				if (ConfigManager.HasInstance())
				{
					List<signReward> allSignRewardByWorld = ConfigManager.Instance.GetAllSignRewardByWorld(GameDataManager.Instance.GetCurrentWorldId());
					if (allSignRewardByWorld != null && allSignRewardByWorld.Count > 0)
					{
						num = allSignRewardByWorld.Count;
					}
				}
				for (int i = 1; i <= num; i++)
				{
					if (checkInDay >= i)
					{
						for (int j = 0; j < 4; j++)
						{
							int num2 = (i - 1) * 4 + j;
							if (num2 < clampItemInfo.Length && clampItemInfo[num2] <= 0)
							{
								flag = true;
								break;
							}
						}
					}
					if (flag)
					{
						break;
					}
				}
			}
		}
		SetCheckRepoint(flag);
	}

	public void SetYinDaoTu(bool isOpen)
	{
		yindaotu.SetActive(isOpen);
	}

	private void OnGoldChanged(double newGold)
	{
		RefreshGoldDisplay();
	}

	public void ShowSetUpPanel()
	{
		Transform mianParent = UIManager.Instance.GetLayerTransform(UILayer.Popup);
		if (!((Object)(object)mianParent != (Object)null))
		{
			return;
		}
		if ((Object)(object)setUpPanel == (Object)null)
		{
			SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/NewUI/Popup/prefabs/setupPanel.prefab", delegate(GameObject prefab)
			{
				setUpPanel = Object.Instantiate<GameObject>(prefab, mianParent);
			});
		}
		else
		{
			setUpPanel.SetActive(true);
		}
	}

	public void GetImRank()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		TT.GetImRankList(new JsonData
		{
			["zoneId"] = JsonData.op_Implicit(WorldRankUtility.GetZoneId(GameDataManager.Instance.GetCurrentWorldId())),
			["suffix"] = JsonData.op_Implicit("等级"),
			["rankTitle"] = JsonData.op_Implicit("每周排行榜"),
			["dataType"] = JsonData.op_Implicit(0),
			["relationType"] = JsonData.op_Implicit("default"),
			["rankType"] = JsonData.op_Implicit("week"),
			["shareTitle"] = JsonData.op_Implicit("你就说我强不强嘛")
		}, (Action<bool, string>)delegate(bool isSuccess, string errMsg)
		{
			if (isSuccess)
			{
				Debug.Log((object)"GetImRankList success");
			}
			else
			{
				Debug.Log((object)("GetImRankList fail with error message: " + errMsg));
			}
		});
	}
}

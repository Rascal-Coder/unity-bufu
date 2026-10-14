using System;
using System.Collections;
using System.Collections.Generic;
using Module.Facility;
using Module.Http;
using Module.Mission;
using Module.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using cfg;

public class UpgradePanel : MonoBehaviour
{
	[Header("底部按钮")]
	[SerializeField]
	private Button button1;

	[SerializeField]
	private Button button2;

	[SerializeField]
	private Button button3;

	[SerializeField]
	private Button button4;

	[SerializeField]
	private Button button5;

	[SerializeField]
	private Button button6;

	[Header("引导动画")]
	[SerializeField]
	private RectTransform guideFinger;

	[Header("引导动画")]
	[SerializeField]
	private SkillTip itemTip;

	[SerializeField]
	private Button itemTipMask;

	[Header("对应的 Panel")]
	[SerializeField]
	private UpgradeItemPanel itemPanel;

	[SerializeField]
	private GameObject panel1;

	[SerializeField]
	private GameObject panel3;

	[SerializeField]
	private GameObject panel4;

	[SerializeField]
	private GameObject panel5;

	[SerializeField]
	private GameObject panel6;

	[Header("按钮选中图片")]
	[SerializeField]
	private GameObject chooseImg1;

	[SerializeField]
	private GameObject chooseImg2;

	[SerializeField]
	private GameObject chooseImg3;

	[SerializeField]
	private GameObject chooseImg4;

	[SerializeField]
	private GameObject chooseImg5;

	[SerializeField]
	private GameObject chooseImg6;

	[SerializeField]
	private RectTransform[] lockImgs;

	[SerializeField]
	private RectTransform[] yijiesuoImgs;

	[SerializeField]
	private Image[] btnImgs;

	[Header("升级项相关")]
	[SerializeField]
	private GameObject upgradeItemPrefab;

	[Header("骰子世界升级项相关")]
	[SerializeField]
	private GameObject upgradeItemPrefab2;

	[SerializeField]
	private Transform contentParent;

	[SerializeField]
	private Transform content2Parent;

	public int currentSelectedIndex = 1;

	private GameObject talent;

	private GameObject diceTalent;

	[Header("临时存放talent物体用来激活")]
	[SerializeField]
	private Transform linshiTalent;

	private GameObject challengePanel;

	private Dictionary<int, List<UpgradeItem>> panelUpgradeItems = new Dictionary<int, List<UpgradeItem>>();

	public static UpgradePanel Instance;

	[Header("主界面天赋界面进入按键的红点物体")]
	[SerializeField]
	public GameObject talentRedDot;

	[Header("upgradePanel界面的红点")]
	[SerializeField]
	private GameObject[] redDots = (GameObject[])(object)new GameObject[11];

	[Header("panel1中top物体")]
	[SerializeField]
	private Transform top;

	private const string BUTTON_CLICKED_KEY_PREFIX = "button_clicked_";

	private bool isGuideClicked;

	private Coroutine _redDotRefreshCoroutine;

	private int _guide4UpgradeCount;

	private static int GUIDE4_TOTAL_UPGRADES = 6;

	private UpgradeItem _currentGuideItem;

	private void Awake()
	{
		if ((Object)(object)Instance != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		Instance = this;
		InitUI();
	}

	protected virtual void InitUI()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Expected O, but got Unknown
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Expected O, but got Unknown
		((UnityEvent)button1.onClick).AddListener((UnityAction)delegate
		{
			OnButtonClicked(1);
		});
		((UnityEvent)button2.onClick).AddListener((UnityAction)delegate
		{
			OnButtonClicked(2);
		});
		((UnityEvent)button3.onClick).AddListener((UnityAction)delegate
		{
			OnButtonClicked(3);
		});
		((UnityEvent)button4.onClick).AddListener((UnityAction)delegate
		{
			OnButtonClicked(4);
		});
		((UnityEvent)button5.onClick).AddListener((UnityAction)delegate
		{
			OnButtonClicked(5);
		});
		if ((Object)(object)button6 != (Object)null)
		{
			((UnityEvent)button6.onClick).AddListener((UnityAction)delegate
			{
				OnButtonClicked(6);
			});
		}
		itemPanel.OnRefreshUpgradeItemsForPanel += OnPanel12Switch;
		SkillTip skillTip = itemTip;
		if (skillTip != null)
		{
			((Component)skillTip).gameObject.SetActive(false);
		}
		Button obj = itemTipMask;
		if (obj != null)
		{
			((Component)obj).gameObject.SetActive(false);
		}
		if ((Object)(object)itemTipMask != (Object)null)
		{
			((UnityEvent)itemTipMask.onClick).AddListener(new UnityAction(OnTipMaskClick));
		}
		InitializeDisplayState();
		for (int num = 1; num <= 4; num++)
		{
			panelUpgradeItems[num] = new List<UpgradeItem>();
		}
		RectTransform obj2 = guideFinger;
		if (obj2 != null)
		{
			((Component)obj2).gameObject.SetActive(false);
		}
		for (int num2 = 0; num2 < lockImgs.Length; num2++)
		{
			GameObjectClickListener gameObjectClickListener = GameObjectClickListener.Get(((Component)lockImgs[num2]).gameObject);
			gameObjectClickListener.onClick = (GameObjectClickListener.VoidDelegate)Delegate.Combine(gameObjectClickListener.onClick, new GameObjectClickListener.VoidDelegate(HandleLockClick));
		}
		StartGuide4();
		MissionManager.Instance.AfterGoldReachUnlockEvent += CheckUnLock;
	}

	private void HandleLockClick(GameObject lockObj, float x, float y)
	{
		if ((Object)(object)lockObj == (Object)null)
		{
			return;
		}
		int num = 1;
		if ((Object)(object)lockObj == (Object)(object)((Component)lockImgs[0]).gameObject)
		{
			num = (int)ConfigManager.Instance.GetCommonFactor(18);
			BroadcastManager.ShowPopUpTip($"完成成长等级{num}后解锁");
		}
		else if ((Object)(object)lockObj == (Object)(object)((Component)lockImgs[1]).gameObject)
		{
			num = (int)ConfigManager.Instance.GetCommonFactor(20);
			BroadcastManager.ShowPopUpTip($"完成成长等级{num}后解锁");
		}
		else if ((Object)(object)lockObj == (Object)(object)((Component)lockImgs[2]).gameObject)
		{
			num = (int)ConfigManager.Instance.GetCommonFactor(21);
			BroadcastManager.ShowPopUpTip($"完成成长等级{num}后解锁");
		}
		else if ((Object)(object)lockObj == (Object)(object)((Component)lockImgs[3]).gameObject)
		{
			num = (int)ConfigManager.Instance.GetCommonFactor(48);
			if (num >= 0)
			{
				BroadcastManager.ShowPopUpTip($"完成成长等级{num}后解锁");
			}
			else
			{
				BroadcastManager.ShowPopUpTip("功能完善中,敬请期待");
			}
		}
		else if ((Object)(object)lockObj == (Object)(object)((Component)lockImgs[4]).gameObject)
		{
			num = (int)ConfigManager.Instance.GetCommonFactor(27);
			BroadcastManager.ShowPopUpTip($"完成成长等级{num}后解锁");
		}
		else if ((Object)(object)lockObj == (Object)(object)((Component)lockImgs[6]).gameObject)
		{
			num = (int)ConfigManager.Instance.GetCommonFactor(80);
			BroadcastManager.ShowPopUpTip($"完成成长等级{num}后解锁");
		}
		else if (lockImgs != null && lockImgs.Length >= 6 && (Object)(object)lockImgs[5] != (Object)null && (Object)(object)lockObj == (Object)(object)((Component)lockImgs[5]).gameObject && ConfigManager.HasInstance())
		{
			num = (int)ConfigManager.Instance.GetCommonFactor(59);
			BroadcastManager.ShowPopUpTip($"完成成长等级{num}后解锁");
		}
	}

	private void Start()
	{
		FacilityRedDot.CheckAllRedDots();
	}

	private void DelayedRefresh()
	{
	}

	public virtual void OnOpen()
	{
		((Component)this).gameObject.SetActive(true);
		CheckUnLock();
		RefreshWorldSpecificNavigation();
		RefreshAllPanels();
	}

	private void InitializeDisplayState()
	{
		HideAllPanels();
		HideAllChooseImages();
		CheckUnLock();
		panel1.SetActive(true);
		chooseImg1.SetActive(true);
		currentSelectedIndex = 1;
	}

	public void RefreshForWorldChange()
	{
		currentSelectedIndex = 0;
		OnButtonClicked(1);
	}

	public void OnButtonClicked(int buttonIndex)
	{
		if (buttonIndex == 3 && !SupportsTalentTree())
		{
			BroadcastManager.ShowPopUpTip("当前世界的天赋配置暂未开放");
		}
		else
		{
			if (buttonIndex == currentSelectedIndex)
			{
				return;
			}
			((Component)guideFinger).gameObject.SetActive(false);
			SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
			currentSelectedIndex = buttonIndex;
			HideAllPanels();
			HideAllChooseImages();
			ShowPanel(buttonIndex);
			ShowChooseImage(buttonIndex);
			if (buttonIndex > 0 && buttonIndex <= 6 && !IsButtonClicked(buttonIndex))
			{
				if (yijiesuoImgs != null && yijiesuoImgs.Length >= buttonIndex)
				{
					RectTransform obj = yijiesuoImgs[buttonIndex - 1];
					if (obj != null)
					{
						((Component)obj).gameObject.SetActive(false);
					}
				}
				SetButtonClicked(buttonIndex, clicked: true);
			}
			DownItem downItem = UIManager.Instance.GetDownItem();
			if ((Object)(object)downItem != (Object)null)
			{
				downItem.RefreshChallenge();
				if (buttonIndex != 1)
				{
					UIManager.Instance.topPanel.HideMian();
				}
			}
		}
	}

	private void RefreshWorldSpecificNavigation()
	{
		bool flag = SupportsTalentTree();
		if ((Object)(object)button3 != (Object)null)
		{
			((Component)button3).gameObject.SetActive(flag);
		}
		if (!flag && currentSelectedIndex == 3)
		{
			currentSelectedIndex = 0;
			OnButtonClicked(1);
		}
	}

	private static bool SupportsTalentTree()
	{
		if ((Object)(object)GameDataManager.Instance == (Object)null || !ConfigManager.HasInstance())
		{
			return true;
		}
		world worldConfig = ConfigManager.Instance.GetWorldConfig(GameDataManager.Instance.GetCurrentWorldId());
		if (worldConfig != null)
		{
			if (worldConfig.GameplayType != 1)
			{
				return worldConfig.GameplayType == 2;
			}
			return true;
		}
		return true;
	}

	public void CheckUnLock(mainMission[] missions = null)
	{
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0363: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_047b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0474: Unknown result type (might be due to invalid IL or missing references)
		//IL_051b: Unknown result type (might be due to invalid IL or missing references)
		int currentMainMissionLevel = GameDataManager.Instance.GetCurrentMainMissionLevel();
		((Selectable)button1).interactable = true;
		int num = (int)ConfigManager.Instance.GetCommonFactor(18);
		itemPanel.CheckUnLock();
		if (currentMainMissionLevel > num)
		{
			((Component)lockImgs[0]).gameObject.SetActive(false);
			RectTransform obj = yijiesuoImgs[0];
			if (obj != null)
			{
				((Component)obj).gameObject.SetActive(currentMainMissionLevel > num && !IsButtonClicked(0));
			}
			if (!IsButtonClicked(0))
			{
				((Component)guideFinger).gameObject.SetActive(false);
			}
		}
		else
		{
			((Component)lockImgs[0]).gameObject.SetActive(true);
			RectTransform obj2 = yijiesuoImgs[0];
			if (obj2 != null)
			{
				((Component)obj2).gameObject.SetActive(false);
			}
		}
		int num2 = (int)ConfigManager.Instance.GetCommonFactor(80);
		itemPanel.SetButtonInteractable(2, currentMainMissionLevel > num);
		itemPanel.SetButtonInteractable(3, currentMainMissionLevel > num2);
		num = (int)ConfigManager.Instance.GetCommonFactor(20);
		((Selectable)button2).interactable = currentMainMissionLevel > num;
		((Component)lockImgs[1]).gameObject.SetActive(currentMainMissionLevel <= num);
		((Graphic)btnImgs[1]).color = ((currentMainMissionLevel > num) ? Color.white : Color.gray);
		RectTransform obj3 = yijiesuoImgs[1];
		if (obj3 != null)
		{
			((Component)obj3).gameObject.SetActive(currentMainMissionLevel > num && !IsButtonClicked(2));
		}
		if (currentMainMissionLevel > num && !IsButtonClicked(2))
		{
			((Component)guideFinger).gameObject.SetActive(true);
			((Component)guideFinger).transform.position = ((Component)button2).transform.position;
		}
		num = (int)ConfigManager.Instance.GetCommonFactor(21);
		((Selectable)button3).interactable = currentMainMissionLevel > num;
		((Component)lockImgs[2]).gameObject.SetActive(currentMainMissionLevel <= num);
		((Graphic)btnImgs[2]).color = ((currentMainMissionLevel > num) ? Color.white : Color.gray);
		RectTransform obj4 = yijiesuoImgs[2];
		if (obj4 != null)
		{
			((Component)obj4).gameObject.SetActive(currentMainMissionLevel > num && !IsButtonClicked(3));
		}
		if (currentMainMissionLevel > num && !IsButtonClicked(3))
		{
			((Component)guideFinger).gameObject.SetActive(true);
			((Component)guideFinger).transform.position = ((Component)button3).transform.position;
		}
		num = (int)ConfigManager.Instance.GetCommonFactor(48);
		bool flag = currentMainMissionLevel > num && num >= 0;
		((Selectable)button4).interactable = flag;
		((Component)lockImgs[3]).gameObject.SetActive(!flag);
		((Graphic)btnImgs[3]).color = (flag ? Color.white : Color.gray);
		RectTransform obj5 = yijiesuoImgs[3];
		if (obj5 != null)
		{
			((Component)obj5).gameObject.SetActive(flag && !IsButtonClicked(4));
		}
		if (flag && !IsButtonClicked(4))
		{
			((Component)guideFinger).gameObject.SetActive(true);
			((Component)guideFinger).transform.position = ((Component)button4).transform.position;
		}
		num = (int)ConfigManager.Instance.GetCommonFactor(27);
		((Selectable)button5).interactable = currentMainMissionLevel > num;
		((Component)lockImgs[4]).gameObject.SetActive(currentMainMissionLevel <= num);
		((Graphic)btnImgs[4]).color = ((currentMainMissionLevel > num) ? Color.white : Color.gray);
		RectTransform obj6 = yijiesuoImgs[4];
		if (obj6 != null)
		{
			((Component)obj6).gameObject.SetActive(currentMainMissionLevel > num && !IsButtonClicked(5));
		}
		if (currentMainMissionLevel > num && !IsButtonClicked(5))
		{
			((Component)guideFinger).gameObject.SetActive(true);
			((Component)guideFinger).transform.position = ((Component)button5).transform.position;
		}
		bool flag2 = false;
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			flag2 = GameDataManager.Instance.IsWorldEntryUnlocked();
		}
		if ((Object)(object)button6 != (Object)null)
		{
			((Selectable)button6).interactable = flag2;
		}
		if (lockImgs != null && lockImgs.Length >= 6 && (Object)(object)lockImgs[5] != (Object)null)
		{
			((Component)lockImgs[5]).gameObject.SetActive(!flag2);
		}
		if (btnImgs != null && btnImgs.Length >= 6 && (Object)(object)btnImgs[5] != (Object)null)
		{
			((Graphic)btnImgs[5]).color = (flag2 ? Color.white : Color.gray);
		}
		if (yijiesuoImgs != null && yijiesuoImgs.Length >= 6 && (Object)(object)yijiesuoImgs[5] != (Object)null)
		{
			((Component)yijiesuoImgs[5]).gameObject.SetActive(flag2 && !IsButtonClicked(6));
		}
		if (flag2 && !IsButtonClicked(6) && (Object)(object)guideFinger != (Object)null && (Object)(object)button6 != (Object)null)
		{
			((Component)guideFinger).gameObject.SetActive(true);
			((Component)guideFinger).transform.position = ((Component)button6).transform.position;
		}
	}

	private bool IsButtonClicked(int buttonIndex)
	{
		return PlayerPrefs.GetInt("button_clicked_" + buttonIndex, 0) == 1;
	}

	private void SetButtonClicked(int buttonIndex, bool clicked)
	{
		if (buttonIndex == 3 || buttonIndex == 2)
		{
			((MonoBehaviour)this).StartCoroutine(DelayShowDialog(buttonIndex));
		}
		PlayerPrefs.SetInt("button_clicked_" + buttonIndex, clicked ? 1 : 0);
		PlayerPrefs.Save();
	}

	private IEnumerator DelayShowDialog(int buttonIndex)
	{
		yield return (object)new WaitForSeconds(1f);
		string dialogText = "";
		if (buttonIndex == 2)
		{
			dialogText = "新的帮手有更多强劲的技能";
		}
		if (buttonIndex == 3)
		{
			dialogText = "重生后可获得天赋点和全局收益！";
		}
		UIManager.Instance.ShowDialogPanel(dialogText, DialogPosition.Bottom, 0.001f, delegate
		{
			UIManager.Instance.HideDialogPanel();
			Time.timeScale = 1f;
			if (PlayerPrefs.GetInt("guide_step_2_count", 0) >= 2)
			{
				PlayerPrefs.SetInt("guide_step_2", 1);
				PlayerPrefs.Save();
				GameDataManager.Instance.ForceUpdateNewGuide();
			}
		});
	}

	public void GuideClick(Transform clickObj)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)clickObj != (Object)null)
		{
			isGuideClicked = true;
			((Component)guideFinger).gameObject.SetActive(true);
			((Component)guideFinger).transform.SetParent(clickObj);
			Transform transform = ((Component)guideFinger).transform;
			((RectTransform)((transform is RectTransform) ? transform : null)).anchoredPosition = Vector2.zero;
		}
		else
		{
			isGuideClicked = false;
			((Component)guideFinger).transform.SetParent(((Component)this).transform, false);
			((Component)guideFinger).gameObject.SetActive(false);
		}
	}

	private void HideAllPanels()
	{
		panel1.SetActive(false);
		panel3.SetActive(false);
		UIManager.Instance.HideFacilityUI();
		UIManager.Instance.HideWorldUI();
		RectTransform obj = guideFinger;
		if (obj != null)
		{
			((Component)obj).gameObject.SetActive(false);
		}
		panel4.SetActive(false);
		if ((Object)(object)panel6 != (Object)null)
		{
			panel6.SetActive(false);
		}
		ReturnTalentToLinshi();
	}

	private void HideAllChooseImages()
	{
		chooseImg1.SetActive(false);
		chooseImg2.SetActive(false);
		chooseImg3.SetActive(false);
		chooseImg4.SetActive(false);
		chooseImg5.SetActive(false);
		if ((Object)(object)chooseImg6 != (Object)null)
		{
			chooseImg6.SetActive(false);
		}
	}

	private void ShowPanel(int index)
	{
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		switch (index)
		{
		case 1:
			itemPanel?.RefreshForCurrentWorld();
			panel1.SetActive(true);
			UIManager.Instance.ShowTopBtn();
			UIManager.Instance.ShowDownItem();
			UIManager.Instance.HideFacilityUI();
			UIManager.Instance.HideShopTurntableUI();
			UIManager.Instance.topPanel.DisplayMain();
			break;
		case 2:
			UIManager.Instance.HideDownItem();
			UIManager.Instance.ShowTopBtn(show: false);
			UIManager.Instance.ShowFacilityUI();
			UIManager.Instance.HideShopTurntableUI();
			UIManager.Instance.topPanel.HideMian();
			break;
		case 3:
		{
			panel3.SetActive(true);
			RefreshUpgradeItemsForPanel(3);
			UIManager.Instance.HideDownItem();
			UIManager.Instance.ShowTopBtn(show: false);
			TalentManager currentTalentManager = TalentDataManager.Instance?.GetCurrentTalentManager();
			TalentDataManager instance = TalentDataManager.Instance;
			if (instance != null && instance.CurrentTalentSceneType == TalentSceneType.CoinSceneTalent)
			{
				if ((Object)(object)talent == (Object)null)
				{
					SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/NewUI/Talent/prefabs/talent.prefab", delegate(GameObject prefab)
					{
						//IL_0031: Unknown result type (might be due to invalid IL or missing references)
						talent = Object.Instantiate<GameObject>(prefab, panel3.transform);
						talent.transform.localPosition = Vector3.zero;
						talent.gameObject.SetActive(true);
						currentTalentManager?.OpenPanel(TalentType.talentCoin);
					});
				}
				else if ((Object)(object)talent != (Object)null && (Object)(object)currentTalentManager != (Object)null)
				{
					talent.gameObject.SetActive(true);
					if ((Object)(object)talent.transform.parent != (Object)(object)panel3.transform)
					{
						talent.transform.SetParent(panel3.transform);
						talent.transform.localPosition = Vector3.zero;
					}
					currentTalentManager?.OpenPanel(TalentType.talentCoin);
				}
			}
			else if ((Object)(object)diceTalent == (Object)null)
			{
				SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/NewUI/Talent/prefabs/diceTalent.prefab", delegate(GameObject prefab)
				{
					//IL_0022: Unknown result type (might be due to invalid IL or missing references)
					diceTalent = Object.Instantiate<GameObject>(prefab, panel3.transform);
					diceTalent.transform.localPosition = Vector3.zero;
					diceTalent.gameObject.SetActive(true);
					(TalentDataManager.Instance?.GetCurrentTalentManager())?.OpenPanel(TalentType.talentCoin);
				});
			}
			else
			{
				diceTalent.gameObject.SetActive(true);
				if ((Object)(object)diceTalent.transform.parent != (Object)(object)panel3.transform)
				{
					diceTalent.transform.SetParent(panel3.transform);
					diceTalent.transform.localPosition = Vector3.zero;
				}
				currentTalentManager?.OpenPanel(TalentType.talentCoin);
			}
			UIManager.Instance.HideFacilityUI();
			UIManager.Instance.HideShopTurntableUI();
			UIManager.Instance.topPanel.HideMian();
			break;
		}
		case 4:
			panel4.SetActive(true);
			if (ChallengeDataManager.Instance != null)
			{
				ChallengeDataManager.Instance.OpenChallengePanel();
			}
			UIManager.Instance.HideDownItem();
			UIManager.Instance.ShowTopBtn(show: false);
			UIManager.Instance.HideShopTurntableUI();
			UIManager.Instance.HideFacilityUI();
			UIManager.Instance.topPanel?.HideMian();
			break;
		case 5:
			UIManager.Instance.HideDownItem();
			UIManager.Instance.ShowTopBtn(show: false);
			UIManager.Instance.ShowShopTurntableUI();
			UIManager.Instance.HideFacilityUI();
			UIManager.Instance.topPanel?.HideMian();
			break;
		case 6:
			UIManager.Instance.ShowWorldUI();
			UIManager.Instance.HideDownItem();
			UIManager.Instance.ShowTopBtn(show: false);
			UIManager.Instance.HideShopTurntableUI();
			UIManager.Instance.HideFacilityUI();
			UIManager.Instance.topPanel?.HideMian();
			break;
		}
		try
		{
			SingletonMono<HttpManager>.Instance.TrackSwitchUI(null, $"panel_{index}");
		}
		catch (Exception)
		{
			throw;
		}
	}

	private void ReturnTalentToLinshi()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)talent != (Object)null && (Object)(object)linshiTalent != (Object)null && (Object)(object)talent.transform.parent != (Object)(object)linshiTalent)
		{
			talent.transform.SetParent(linshiTalent);
			talent.transform.localPosition = Vector3.zero;
		}
		if ((Object)(object)diceTalent != (Object)null && (Object)(object)linshiTalent != (Object)null && (Object)(object)diceTalent.transform.parent != (Object)(object)linshiTalent)
		{
			diceTalent.transform.SetParent(linshiTalent);
			diceTalent.transform.localPosition = Vector3.zero;
		}
		TalentManager talentManager = TalentDataManager.Instance?.GetCurrentTalentManager();
		if ((Object)(object)talentManager != (Object)null)
		{
			talentManager.CloseAllSubPanels();
		}
	}

	private void ShowChooseImage(int index)
	{
		switch (index)
		{
		case 1:
			chooseImg1.SetActive(true);
			break;
		case 2:
			chooseImg2.SetActive(true);
			break;
		case 3:
			chooseImg3.SetActive(true);
			break;
		case 4:
			chooseImg4.SetActive(true);
			break;
		case 5:
			chooseImg5.SetActive(true);
			break;
		case 6:
			if ((Object)(object)chooseImg6 != (Object)null)
			{
				chooseImg6.SetActive(true);
			}
			break;
		}
	}

	protected virtual void OnDestroy()
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Expected O, but got Unknown
		Button obj = button1;
		if (obj != null)
		{
			((UnityEventBase)obj.onClick).RemoveAllListeners();
		}
		Button obj2 = button2;
		if (obj2 != null)
		{
			((UnityEventBase)obj2.onClick).RemoveAllListeners();
		}
		Button obj3 = button3;
		if (obj3 != null)
		{
			((UnityEventBase)obj3.onClick).RemoveAllListeners();
		}
		Button obj4 = button4;
		if (obj4 != null)
		{
			((UnityEventBase)obj4.onClick).RemoveAllListeners();
		}
		Button obj5 = button5;
		if (obj5 != null)
		{
			((UnityEventBase)obj5.onClick).RemoveAllListeners();
		}
		Button obj6 = button6;
		if (obj6 != null)
		{
			((UnityEventBase)obj6.onClick).RemoveAllListeners();
		}
		Button obj7 = itemTipMask;
		if (obj7 != null)
		{
			((UnityEvent)obj7.onClick).RemoveListener(new UnityAction(OnTipMaskClick));
		}
		if ((Object)(object)itemPanel != (Object)null)
		{
			itemPanel.OnRefreshUpgradeItemsForPanel -= OnPanel12Switch;
		}
		if ((Object)(object)MissionManager.Instance != (Object)null)
		{
			MissionManager.Instance.AfterGoldReachUnlockEvent -= CheckUnLock;
		}
		if (lockImgs != null)
		{
			RectTransform[] array = lockImgs;
			foreach (RectTransform val in array)
			{
				if ((Object)(object)val != (Object)null)
				{
					GameObjectClickListener gameObjectClickListener = GameObjectClickListener.Get(((Component)val).gameObject);
					gameObjectClickListener.onClick = (GameObjectClickListener.VoidDelegate)Delegate.Remove(gameObjectClickListener.onClick, new GameObjectClickListener.VoidDelegate(HandleLockClick));
				}
			}
		}
		ClearAllUpgradeItems();
		if ((Object)(object)Instance == (Object)(object)this)
		{
			Instance = null;
		}
	}

	private void RefreshAllPanels()
	{
		if ((Object)(object)upgradeItemPrefab == (Object)null && (Object)(object)upgradeItemPrefab2 == (Object)null)
		{
			Debug.LogError((object)"upgradeItemPrefab 为空，请在 Inspector 中赋值！");
			return;
		}
		ClearAllUpgradeItems();
		GenerateUpgradeItemsForPanel(1, 1, 10, "itemUpgrade1");
		GenerateUpgradeItemsForPanel(2, 1, 20, "itemUpgrade2");
		GenerateUpgradeItemsForPanel(3, 1, 20, "itemUpgrade3");
		int num = ((!((Object)(object)GameDataManager.Instance != (Object)null)) ? 1 : GameDataManager.Instance.GetCurrentWorldId());
		if (panelUpgradeItems.TryGetValue(1, out var value))
		{
			List<UpgradeItem> list = value.FindAll((UpgradeItem item) => (Object)(object)item != (Object)null && item.isUpgradeable);
			RedDotsSet((num == 1) ? 5 : 8, list.Count > 0);
		}
		if (panelUpgradeItems.TryGetValue(2, out var value2))
		{
			List<UpgradeItem> list2 = value2.FindAll((UpgradeItem item) => (Object)(object)item != (Object)null && item.isUpgradeable);
			RedDotsSet(6, list2.Count > 0);
		}
		if (panelUpgradeItems.TryGetValue(3, out var value3))
		{
			List<UpgradeItem> list3 = value3.FindAll((UpgradeItem item) => (Object)(object)item != (Object)null && item.isUpgradeable);
			RedDotsSet(9, list3.Count > 0);
		}
		FacilityRedDot.CheckAllRedDots();
		RefreshUpgradeItemsForPanel(1);
		RefreshUpgradeItemsForPanel(2);
		RefreshUpgradeItemsForPanel(3);
	}

	private void GenerateUpgradeItemsForPanel(int panelIndex, int startId, int endId, string configType)
	{
		GameObject panelByIndex = GetPanelByIndex(panelIndex);
		if ((Object)(object)panelByIndex == (Object)null)
		{
			return;
		}
		Transform val = panelByIndex.transform.Find("Scroll View/Viewport/Content");
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		int num = ((!((Object)(object)GameDataManager.Instance != (Object)null)) ? 1 : GameDataManager.Instance.GetCurrentWorldId());
		List<int> list = new List<int>();
		if (configType == "itemUpgrade1")
		{
			foreach (itemUpgrade1 item in ConfigManager.Instance.GetAllUpgrade1ByWorld(num))
			{
				list.Add(item.Id);
			}
			list.Sort();
		}
		else if (configType == "itemUpgrade2")
		{
			foreach (itemUpgrade2 item2 in ConfigManager.Instance.GetAllUpgrade2ByWorld(num))
			{
				list.Add(item2.Id);
			}
			list.Sort((int left, int right) => ConfigManager.Instance.GetUpgrade2Config(left).Position.CompareTo(ConfigManager.Instance.GetUpgrade2Config(right).Position));
		}
		else
		{
			foreach (itemUpgrade3 item3 in ConfigManager.Instance.GetAllUpgrade3ByWorld(num))
			{
				list.Add(item3.Id);
			}
			list.Sort((int left, int right) => ConfigManager.Instance.GetUpgrade3Config(left).Position.CompareTo(ConfigManager.Instance.GetUpgrade3Config(right).Position));
		}
		foreach (int item4 in list)
		{
			bool num2;
			if (!(configType == "itemUpgrade1"))
			{
				if (!(configType == "itemUpgrade2"))
				{
					itemUpgrade3 upgrade3Config = ConfigManager.Instance.GetUpgrade3Config(item4);
					if (upgrade3Config == null)
					{
						continue;
					}
					num2 = upgrade3Config.WorldId == num;
				}
				else
				{
					itemUpgrade2 upgrade2Config = ConfigManager.Instance.GetUpgrade2Config(item4);
					if (upgrade2Config == null)
					{
						continue;
					}
					num2 = upgrade2Config.WorldId == num;
				}
			}
			else
			{
				itemUpgrade1 upgrade1Config = ConfigManager.Instance.GetUpgrade1Config(item4);
				if (upgrade1Config == null)
				{
					continue;
				}
				num2 = upgrade1Config.WorldId == num;
			}
			if (!num2)
			{
				continue;
			}
			GameObject val2 = null;
			switch (num)
			{
			case 1:
				val2 = Object.Instantiate<GameObject>(upgradeItemPrefab, val);
				break;
			case 2:
				val2 = Object.Instantiate<GameObject>(upgradeItemPrefab2, val);
				break;
			}
			if ((Object)(object)val2 != (Object)null)
			{
				UpgradeItem component = val2.GetComponent<UpgradeItem>();
				if ((Object)(object)component != (Object)null)
				{
					component.Initialize(item4, configType);
					component.OnTipButtonClick += OnTipButtonClick;
					component.onUpgradeButtonClick += OnUpgradeButtonClick;
					panelUpgradeItems[panelIndex].Add(component);
				}
			}
		}
	}

	private void OnUpgradeButtonClick(UpgradeItem item)
	{
		if (_redDotRefreshCoroutine == null)
		{
			RefreshRedDotsState();
			_redDotRefreshCoroutine = ((MonoBehaviour)this).StartCoroutine(DelayRefreshRedDots());
		}
	}

	private IEnumerator DelayRefreshRedDots()
	{
		yield return (object)new WaitForSeconds(2f);
		RefreshRedDotsState();
		_redDotRefreshCoroutine = null;
	}

	private void RefreshRedDotsState()
	{
		int num = ((!((Object)(object)GameDataManager.Instance != (Object)null)) ? 1 : GameDataManager.Instance.GetCurrentWorldId());
		bool isOpen = false;
		if (panelUpgradeItems.TryGetValue(1, out var value))
		{
			isOpen = value.Exists((UpgradeItem i) => (Object)(object)i != (Object)null && i.isUpgradeable);
		}
		RedDotsSet((num == 1) ? 5 : 8, isOpen);
		bool isOpen2 = false;
		if (panelUpgradeItems.TryGetValue(2, out var value2))
		{
			isOpen2 = value2.Exists((UpgradeItem i) => (Object)(object)i != (Object)null && i.isUpgradeable);
		}
		RedDotsSet(6, isOpen2);
		bool isOpen3 = false;
		if (panelUpgradeItems.TryGetValue(3, out var value3))
		{
			isOpen3 = value3.Exists((UpgradeItem i) => (Object)(object)i != (Object)null && i.isUpgradeable);
		}
		RedDotsSet(9, isOpen3);
	}

	private void OnPanel12Switch(int panelIndex)
	{
		Debug.Log((object)"隐藏手指");
		((Component)guideFinger).gameObject.SetActive(false);
		RefreshUpgradeItemsForPanel(panelIndex);
		if (PlayerPrefs.GetInt("guide_step_4", 0) == 0)
		{
			ShowGuide4Finger(panelIndex);
		}
	}

	public void RefreshUpgradeItemsForPanel(int panelIndex)
	{
		bool flag = false;
		if (panelUpgradeItems.TryGetValue(panelIndex, out var value))
		{
			foreach (UpgradeItem item in value)
			{
				if (!((Object)(object)item == (Object)null))
				{
					item.ForceRefresh();
					if (panelIndex == 1 || panelIndex == 2 || panelIndex == 3)
					{
						flag = flag || item.isUpgradeable;
					}
				}
			}
			if (panelIndex == 2)
			{
				for (int num = value.Count - 1; num >= 0; num--)
				{
					int index = num;
					if ((Object)(object)value[index] != (Object)null && (Object)(object)content2Parent != (Object)null)
					{
						int position = value[index].position;
						if (content2Parent.childCount > position - 1 && position - 1 >= 0)
						{
							((Component)value[index]).transform.SetSiblingIndex(position - 1);
						}
					}
				}
			}
		}
		int num2 = ((!((Object)(object)GameDataManager.Instance != (Object)null)) ? 1 : GameDataManager.Instance.GetCurrentWorldId());
		if (panelIndex == 1)
		{
			RedDotsSet((num2 == 1) ? 5 : 8, flag);
		}
		if (panelIndex == 2)
		{
			RedDotsSet(6, flag);
		}
		if (panelIndex == 3)
		{
			RedDotsSet(9, flag);
		}
		GameObject panelByIndex = GetPanelByIndex(panelIndex);
		if ((Object)(object)panelByIndex != (Object)null)
		{
			Transform val = panelByIndex.transform.Find("Scroll View/Viewport/Content");
			if ((Object)(object)val != (Object)null)
			{
				UpdateContentHeight((RectTransform)(object)((val is RectTransform) ? val : null));
			}
		}
	}

	private void UpdateContentHeight(RectTransform content)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Invalid comparison between Unknown and I4
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)content == (Object)null)
		{
			return;
		}
		GridLayoutGroup component = ((Component)content).GetComponent<GridLayoutGroup>();
		if (!((Object)(object)component != (Object)null))
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < ((Transform)content).childCount; i++)
		{
			if (((Component)((Transform)content).GetChild(i)).gameObject.activeSelf)
			{
				num++;
			}
		}
		int num2 = (((int)component.constraint == 1) ? component.constraintCount : 2);
		int num3 = Mathf.CeilToInt((float)num / (float)num2);
		float num4 = (float)(((LayoutGroup)component).padding.top + ((LayoutGroup)component).padding.bottom) + (float)num3 * component.cellSize.y + (float)Mathf.Max(0, num3 - 1) * component.spacing.y;
		content.sizeDelta = new Vector2(content.sizeDelta.x, num4 + 100f);
	}

	private void ClearAllUpgradeItems()
	{
		foreach (List<UpgradeItem> value in panelUpgradeItems.Values)
		{
			foreach (UpgradeItem item in value)
			{
				if ((Object)(object)item != (Object)null && (Object)(object)((Component)item).gameObject != (Object)null)
				{
					item.PrepareForDestroy();
					((Component)item).gameObject.SetActive(false);
					((Component)item).transform.SetParent((Transform)null, false);
					Object.Destroy((Object)(object)((Component)item).gameObject);
				}
			}
			value.Clear();
		}
	}

	public GameObject GetPanelByIndex(int index)
	{
		switch (index)
		{
		case 1:
			return itemPanel.GetPanelByIndex(1);
		case 2:
			return itemPanel.GetPanelByIndex(2);
		case 3:
			if (!((Object)(object)itemPanel != (Object)null))
			{
				return null;
			}
			return itemPanel.GetPanelByIndex(3);
		case 4:
			return panel4;
		case 5:
			return panel5;
		default:
			return null;
		}
	}

	public void ShowGuideFinger(int panelIndex, int itemId)
	{
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)guideFinger == (Object)null)
		{
			return;
		}
		if (currentSelectedIndex != panelIndex)
		{
			((Component)guideFinger).gameObject.SetActive(false);
			return;
		}
		UpgradeItem upgradeItem = null;
		if (panelUpgradeItems.TryGetValue(panelIndex, out var value))
		{
			upgradeItem = value.Find((UpgradeItem item) => (Object)(object)item != (Object)null && item.itemId == itemId);
		}
		if ((Object)(object)upgradeItem == (Object)null)
		{
			((Component)guideFinger).gameObject.SetActive(false);
			return;
		}
		upgradeItem.onUpgradeButtonClick -= OnItemUpgrade;
		upgradeItem.onUpgradeButtonClick += OnItemUpgrade;
		RectTransform component = ((Component)upgradeItem).GetComponent<RectTransform>();
		if ((Object)(object)component == (Object)null)
		{
			((Component)guideFinger).gameObject.SetActive(false);
			return;
		}
		Transform parent = ((Transform)guideFinger).parent;
		RectTransform val = (RectTransform)(object)((parent is RectTransform) ? parent : null);
		if ((Object)(object)val == (Object)null)
		{
			((Transform)guideFinger).position = ((Transform)component).position;
		}
		else
		{
			Vector2 anchoredPosition = default(Vector2);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(val, Vector2.op_Implicit(((Transform)component).position), (Camera)null, ref anchoredPosition);
			guideFinger.anchoredPosition = anchoredPosition;
		}
		((Component)guideFinger).gameObject.SetActive(true);
	}

	private void OnItemUpgrade(UpgradeItem item)
	{
		UIManager.Instance.HideDialogPanel();
		Time.timeScale = 1f;
		PlayerPrefs.SetInt("guide_step_3", 1);
		PlayerPrefs.Save();
		GameDataManager.Instance.ForceUpdateNewGuide();
		RectTransform obj = guideFinger;
		if (obj != null)
		{
			((Component)obj).gameObject.SetActive(false);
		}
		item.onUpgradeButtonClick -= OnItemUpgrade;
		UIManager.Instance.topPanel.ShowTip(isShow: true);
		StartGuide4();
	}

	private void StartGuide4()
	{
		GUIDE4_TOTAL_UPGRADES = (int)ConfigManager.Instance.GetCommonFactor(34);
		if (PlayerPrefs.GetInt("guide_step_4", 0) == 0)
		{
			_guide4UpgradeCount = PlayerPrefs.GetInt("guide_step_4_progress", 0);
			SingletonMono<PreloadManager>.Instance.PreloadFacility();
			((MonoBehaviour)this).StartCoroutine(ShowGuideTip());
		}
	}

	private IEnumerator ShowGuideTip()
	{
		yield return (object)new WaitForSeconds(0.001f);
		if (PlayerPrefs.GetInt("guide_step_3", 0) == 1 && currentSelectedIndex == 1 && itemPanel.isPanel1Active)
		{
			yield return ((MonoBehaviour)this).StartCoroutine(Guide4Routine());
		}
	}

	private IEnumerator Guide4Routine()
	{
		while (_guide4UpgradeCount < GUIDE4_TOTAL_UPGRADES)
		{
			if (currentSelectedIndex == 1 && (Object)(object)itemPanel != (Object)null && itemPanel.isPanel1Active)
			{
				ShowGuide4Finger();
			}
			yield return (object)new WaitForSeconds(5f);
		}
		CompleteGuide4();
	}

	private void ShowGuide4Finger(int panelIndex = -1)
	{
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		int key = ((panelIndex == -1) ? currentSelectedIndex : panelIndex);
		if ((Object)(object)guideFinger == (Object)null)
		{
			return;
		}
		if (!panelUpgradeItems.TryGetValue(key, out var value))
		{
			((Component)guideFinger).gameObject.SetActive(false);
			UnsubscribeAllGuide4Items();
			return;
		}
		List<UpgradeItem> list = value.FindAll((UpgradeItem item) => (Object)(object)item != (Object)null && item.isUpgradeable);
		if (list.Count == 0)
		{
			((Component)guideFinger).gameObject.SetActive(false);
			UnsubscribeAllGuide4Items();
			return;
		}
		UpgradeItem upgradeItem = list[0];
		UnsubscribeAllGuide4Items();
		foreach (UpgradeItem item in list)
		{
			item.onUpgradeButtonClick += OnGuide4ItemUpgrade;
		}
		_currentGuideItem = upgradeItem;
		RectTransform component = ((Component)upgradeItem).GetComponent<RectTransform>();
		if ((Object)(object)component == (Object)null)
		{
			((Component)guideFinger).gameObject.SetActive(false);
			return;
		}
		Transform parent = ((Transform)guideFinger).parent;
		RectTransform val = (RectTransform)(object)((parent is RectTransform) ? parent : null);
		if ((Object)(object)val == (Object)null)
		{
			((Transform)guideFinger).position = ((Transform)component).position;
		}
		else
		{
			Vector2 anchoredPosition = default(Vector2);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(val, Vector2.op_Implicit(((Transform)component).position), (Camera)null, ref anchoredPosition);
			guideFinger.anchoredPosition = anchoredPosition;
		}
		if (currentSelectedIndex == 1)
		{
			((Component)guideFinger).gameObject.SetActive(true);
		}
	}

	private void UnsubscribeAllGuide4Items()
	{
		if (!panelUpgradeItems.TryGetValue(currentSelectedIndex, out var value))
		{
			return;
		}
		foreach (UpgradeItem item in value)
		{
			if ((Object)(object)item != (Object)null)
			{
				item.onUpgradeButtonClick -= OnGuide4ItemUpgrade;
			}
		}
		_currentGuideItem = null;
	}

	private void OnGuide4ItemUpgrade(UpgradeItem item)
	{
		RectTransform obj = guideFinger;
		if (obj != null)
		{
			((Component)obj).gameObject.SetActive(false);
		}
		UnsubscribeAllGuide4Items();
		_guide4UpgradeCount++;
		PlayerPrefs.SetInt("guide_step_4_progress", _guide4UpgradeCount);
		PlayerPrefs.Save();
	}

	private void CompleteGuide4()
	{
		PlayerPrefs.SetInt("guide_step_4", 1);
		PlayerPrefs.Save();
	}

	private void OnTipButtonClick(UpgradeItem item, string configType)
	{
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)$"itemId: {item.itemId}, configType: {configType}");
		if (!((Object)(object)itemTip == (Object)null))
		{
			RectTransform component = ((Component)item).GetComponent<RectTransform>();
			if (!((Object)(object)component == (Object)null))
			{
				string text = "";
				text = ((configType == "itemUpgrade1") ? (ConfigManager.Instance.GetUpgrade1Config(item.itemId)?.ItemDesc ?? "未知升级") : ((!(configType == "itemUpgrade2")) ? (ConfigManager.Instance.GetUpgrade3Config(item.itemId)?.ItemDescUI ?? "未知升级") : (ConfigManager.Instance.GetUpgrade2Config(item.itemId)?.ItemDescUI ?? "未知升级")));
				itemTip.SetText(text ?? "");
				((Transform)itemTip.rectTransform).SetParent(((Component)item).transform);
				itemTip.rectTransform.anchoredPosition = new Vector2(50f, component.sizeDelta.y + 100f);
				((Transform)itemTip.rectTransform).SetParent(((Component)this).transform, true);
				((Component)itemTip).gameObject.SetActive(true);
				((Component)itemTipMask).gameObject.SetActive(true);
				((Component)itemTipMask).transform.SetAsLastSibling();
			}
		}
	}

	private void OnTipMaskClick()
	{
		((Component)itemTip).gameObject.SetActive(false);
		((Component)itemTipMask).gameObject.SetActive(false);
	}

	public void TalentRepointSet(bool isOpen)
	{
		talentRedDot.SetActive(isOpen);
	}

	public void RedDotsSet(int index, bool isOpen)
	{
		if (index < 0 || index >= redDots.Length)
		{
			Debug.LogWarning((object)$"红点索引越界: {index}");
			return;
		}
		if ((Object)(object)redDots[index] != (Object)null)
		{
			redDots[index].SetActive(isOpen);
		}
		if (index == 5 || index == 6 || index == 8 || index == 9)
		{
			bool isOpen2 = false;
			if (redDots.Length > 5 && (Object)(object)redDots[5] != (Object)null && redDots[5].activeSelf)
			{
				isOpen2 = true;
			}
			if (redDots.Length > 6 && (Object)(object)redDots[6] != (Object)null && redDots[6].activeSelf)
			{
				isOpen2 = true;
			}
			if (redDots.Length > 8 && (Object)(object)redDots[8] != (Object)null && redDots[8].activeSelf)
			{
				isOpen2 = true;
			}
			if (redDots.Length > 9 && (Object)(object)redDots[9] != (Object)null && redDots[9].activeSelf)
			{
				isOpen2 = true;
			}
			RedDotsSet(0, isOpen2);
		}
	}

	public Vector3 GetTopPos()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Transform obj = top;
		if (obj == null)
		{
			return Vector3.zero;
		}
		return ((Component)obj).transform.position;
	}

	public Vector3 GetButtonPosition(int index)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		Vector3 result = Vector3.zero;
		switch (index)
		{
		case 1:
			if ((Object)(object)button1 != (Object)null)
			{
				result = ((Component)button1).transform.position;
			}
			break;
		case 2:
			if ((Object)(object)button2 != (Object)null)
			{
				result = ((Component)button2).transform.position;
			}
			break;
		case 3:
			if ((Object)(object)button3 != (Object)null)
			{
				result = ((Component)button3).transform.position;
			}
			break;
		case 4:
			if ((Object)(object)button4 != (Object)null)
			{
				result = ((Component)button4).transform.position;
			}
			break;
		case 5:
			if ((Object)(object)button5 != (Object)null)
			{
				result = ((Component)button5).transform.position;
			}
			break;
		case 6:
			if ((Object)(object)button6 != (Object)null)
			{
				result = ((Component)button6).transform.position;
			}
			break;
		}
		return result;
	}
}

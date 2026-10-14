using System;
using System.Collections.Generic;
using Game.Module.Facility;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using cfg;

namespace Module.Facility;

public class UseHelperUI : MonoBehaviour
{
	[Header("UI元素")]
	[SerializeField]
	private TextMeshProUGUI helperNameText;

	[SerializeField]
	private TextMeshProUGUI currentCountText;

	[SerializeField]
	private Image helperIcon;

	[Header("增加上限")]
	[SerializeField]
	private Button addhelperMoneyBtn;

	[SerializeField]
	private Button addhelperDiamondBtn;

	[SerializeField]
	private Button addhelperAdsBtn;

	[SerializeField]
	private TextMeshProUGUI addhelperMoneyCostText;

	[SerializeField]
	private TextMeshProUGUI addhelperDiamondCostText;

	[SerializeField]
	private TextMeshProUGUI limitInfoText;

	[Header("技能升级")]
	[SerializeField]
	private RectTransform skillItem1;

	[SerializeField]
	private TextMeshProUGUI skillLevelText;

	[SerializeField]
	private TextMeshProUGUI skillDescText;

	[SerializeField]
	private TextMeshProUGUI currentSkillText;

	[SerializeField]
	private TextMeshProUGUI nextSkillText;

	[Header("技能升级2")]
	[SerializeField]
	private RectTransform skillItem2;

	[SerializeField]
	private TextMeshProUGUI skillDescText2;

	[SerializeField]
	private TextMeshProUGUI currentSkillText2;

	[SerializeField]
	private TextMeshProUGUI nextSkillText2;

	[Header("升级按钮")]
	[SerializeField]
	private Button upgradeBtn;

	[SerializeField]
	private RectTransform maxObject;

	[SerializeField]
	private Button goldUpgradeBtn;

	[SerializeField]
	private TextMeshProUGUI goldUpgradeCostText;

	[SerializeField]
	private Button diamondUpgradeBtn;

	[SerializeField]
	private TextMeshProUGUI diamondUpgradeCostText;

	[Header("新增的")]
	[SerializeField]
	private TextMeshProUGUI upgradeLevelDescText;

	[SerializeField]
	private Image upgradeLevelbg;

	[SerializeField]
	private List<Image> NSRbg;

	[SerializeField]
	private List<Image> NSR;

	[SerializeField]
	private Image moneyIcon;

	[SerializeField]
	private TextMeshProUGUI moneyText;

	[SerializeField]
	private RectTransform suipianObject;

	[SerializeField]
	private Image suipian;

	[SerializeField]
	private TextMeshProUGUI suipianText;

	[SerializeField]
	private Image suipianown;

	[SerializeField]
	private TextMeshProUGUI suipianownText;

	[Header("关闭按钮")]
	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private Button bgButton;

	[SerializeField]
	private RectTransform tipcon;

	[SerializeField]
	private HelperUpgradeAttributePopup upgradeTip1;

	[SerializeField]
	private HelperUpgradeAttributePopup upgradeTip2;

	[Header("升级按键文本显示")]
	[SerializeField]
	private Image UpgradeBtnImg1;

	[Header("升级按键材料不足文本显示")]
	[SerializeField]
	private Image UpgradeBtnImg2;

	[Header("增加帮手上限按键icon")]
	[SerializeField]
	private Image addhelperMoneyBtnIcon;

	private bool isShowtip2;

	private helperRole helperConfig;

	private int currentLevel = 1;

	private int currentCount;

	private int maxCount;

	private bool isOwned;

	private HelperInfo helperInfo;

	public Action<int> onUpgradeChanged;

	public Action<bool> onLimitChanged;

	[SerializeField]
	private List<HelperSkill> helperSkills;

	private int suipianCost;

	private bool _isSuipianEnough;

	private bool _isGoldEnougth;

	private bool _isDiamondEnougth;

	private int loadedHelperId = -1;

	private int loadingHelperId = -1;

	private SkeletonGraphic skeletonGraphic;

	private void Awake()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		if ((Object)(object)bgButton != (Object)null)
		{
			((UnityEvent)bgButton.onClick).AddListener(new UnityAction(OnCloseClicked));
		}
		if ((Object)(object)closeButton != (Object)null)
		{
			((UnityEvent)closeButton.onClick).AddListener(new UnityAction(OnCloseClicked));
		}
		Button obj = addhelperMoneyBtn;
		if (obj != null)
		{
			((Component)obj).gameObject.SetActive(false);
		}
		Button obj2 = addhelperDiamondBtn;
		if (obj2 != null)
		{
			((Component)obj2).gameObject.SetActive(false);
		}
		Button obj3 = addhelperAdsBtn;
		if (obj3 != null)
		{
			((Component)obj3).gameObject.SetActive(false);
		}
		Button obj4 = goldUpgradeBtn;
		if (obj4 != null)
		{
			((Component)obj4).gameObject.SetActive(false);
		}
		Button obj5 = diamondUpgradeBtn;
		if (obj5 != null)
		{
			((Component)obj5).gameObject.SetActive(false);
		}
		HelperUpgradeAttributePopup helperUpgradeAttributePopup = upgradeTip1;
		if (helperUpgradeAttributePopup != null)
		{
			((Component)helperUpgradeAttributePopup).gameObject.SetActive(false);
		}
		HelperUpgradeAttributePopup helperUpgradeAttributePopup2 = upgradeTip2;
		if (helperUpgradeAttributePopup2 != null)
		{
			((Component)helperUpgradeAttributePopup2).gameObject.SetActive(false);
		}
		RectTransform obj6 = maxObject;
		if (obj6 != null)
		{
			((Component)obj6).gameObject.SetActive(false);
		}
		AddDisableListener();
	}

	private void AddDisableListener()
	{
		GameObjectClickListener gameObjectClickListener = GameObjectClickListener.Get(((Component)addhelperMoneyBtn).gameObject);
		gameObjectClickListener.onClick = (GameObjectClickListener.VoidDelegate)Delegate.Combine(gameObjectClickListener.onClick, (GameObjectClickListener.VoidDelegate)delegate
		{
			if (!((Selectable)addhelperMoneyBtn).interactable)
			{
				UIManager.Instance.ShowSupportUI(DialogType.money, 0.0);
			}
		});
		GameObjectClickListener gameObjectClickListener2 = GameObjectClickListener.Get(((Component)addhelperDiamondBtn).gameObject);
		gameObjectClickListener2.onClick = (GameObjectClickListener.VoidDelegate)Delegate.Combine(gameObjectClickListener2.onClick, (GameObjectClickListener.VoidDelegate)delegate
		{
			if (!((Selectable)addhelperDiamondBtn).interactable)
			{
				UIManager.Instance.ShowSupportUI(DialogType.diamond, 30.0);
			}
		});
	}

	public void Initialize(helperRole config)
	{
		helperConfig = config;
		helperInfo = GameDataManager.Instance.GetHelperInfo(helperConfig.Id);
		maxCount = ((helperInfo != null) ? helperInfo.maxCount : 0);
		SingletonMono<ResManager>.Instance.LoadSpriteAsync(Constants.GetHelperIconPath(helperConfig.Id), delegate(Sprite sprite)
		{
			if ((Object)(object)sprite != (Object)null)
			{
				upgradeTip2.SetIcon(sprite);
			}
		});
		CheckOwnedState();
		UpdateLevel();
		UpdateCount();
		UpdateUI();
	}

	private void CheckOwnedState()
	{
		isOwned = GameDataManager.Instance.IsHelperOwned(helperConfig.Id);
	}

	private void UpdateLevel()
	{
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			currentLevel = GameDataManager.Instance.GetHelperLevel(helperConfig.Id);
		}
		else
		{
			currentLevel = 1;
		}
	}

	private void UpdateCount()
	{
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			currentCount = GameDataManager.Instance.GetHelperCount(helperConfig.Id);
		}
		else
		{
			currentCount = 0;
		}
	}

	private void UpdateUI()
	{
		//IL_0692: Unknown result type (might be due to invalid IL or missing references)
		//IL_069c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0abb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ac5: Expected O, but got Unknown
		//IL_0eb2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ebc: Expected O, but got Unknown
		//IL_0dd4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dde: Expected O, but got Unknown
		//IL_0fb2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fbc: Expected O, but got Unknown
		int num = GameDataManager.Instance?.GetCurrentWorldId() ?? 1;
		int targetId = helperConfig.Id;
		if ((!((Object)(object)skeletonGraphic != (Object)null) || loadedHelperId != targetId) && loadingHelperId != targetId)
		{
			loadingHelperId = targetId;
			loadedHelperId = -1;
			if ((Object)(object)skeletonGraphic != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)skeletonGraphic).gameObject);
				skeletonGraphic = null;
			}
			SingletonMono<ResManager>.Instance.LoadPrefabAsync(Constants.GetRolePrefabPath(targetId), delegate(GameObject gameobj)
			{
				//IL_0124: Unknown result type (might be due to invalid IL or missing references)
				//IL_014d: Unknown result type (might be due to invalid IL or missing references)
				if (!((Object)(object)this == (Object)null) && helperConfig != null && helperConfig.Id == targetId)
				{
					loadingHelperId = -1;
					if ((Object)(object)gameobj != (Object)null)
					{
						loadedHelperId = targetId;
						if ((Object)(object)skeletonGraphic != (Object)null)
						{
							Object.Destroy((Object)(object)((Component)skeletonGraphic).gameObject);
						}
						skeletonGraphic = Object.Instantiate<GameObject>(gameobj, ((Component)helperIcon).transform).GetComponent<SkeletonGraphic>();
						if ((Object)(object)skeletonGraphic != (Object)null)
						{
							skeletonGraphic.AnimationState.SetAnimation(0, "idle_front", true);
							((Component)skeletonGraphic).gameObject.SetActive(true);
							skeletonGraphic.initialFlipX = true;
							((Component)skeletonGraphic).transform.localScale = new Vector3(-1f, 1f, 1f);
							Transform transform = ((Component)skeletonGraphic).transform;
							((RectTransform)((transform is RectTransform) ? transform : null)).anchoredPosition = new Vector2(0f, -50f);
						}
						((Behaviour)helperIcon).enabled = false;
					}
					else
					{
						loadedHelperId = -1;
					}
				}
			});
		}
		if ((Object)(object)helperNameText != (Object)null)
		{
			((TMP_Text)helperNameText).text = helperConfig.HelperName;
		}
		if ((Object)(object)currentCountText != (Object)null)
		{
			int num2 = maxCount;
			num2 = ((num != 2) ? maxCount : (maxCount + (int)(TalentDataManager.Instance?.GetDiceTalentBuff(212) ?? 0f)));
			((TMP_Text)currentCountText).text = $"{currentCount}/{num2}";
		}
		if ((Object)(object)limitInfoText != (Object)null)
		{
			((TMP_Text)limitInfoText).text = helperConfig.HelperName + "数量上限+1";
		}
		if (helperConfig != null)
		{
			for (int num3 = 0; num3 < NSRbg.Count; num3++)
			{
				((Component)NSRbg[num3]).gameObject.SetActive(num3 == helperConfig.Rare - 1);
				((Component)NSR[num3]).gameObject.SetActive(num3 == helperConfig.Rare - 1);
			}
		}
		string[] array = new string[3] { helperConfig.AwakeSkill1, helperConfig.AwakeSkill2, helperConfig.AwakeSkill3 };
		for (int num4 = 0; num4 < 3; num4++)
		{
			if (num4 < helperConfig.AwakeSkillNum.Count)
			{
				((Component)helperSkills[num4]).gameObject.SetActive(true);
				string[] array2 = new string[3] { helperConfig.AwakeSkill1Name, helperConfig.AwakeSkill2Name, helperConfig.AwakeSkill3Name };
				if (currentLevel >= helperConfig.AwakeSkillNum[num4])
				{
					helperSkills[num4].SetData(isUnlocked: true, helperConfig.AwakeSkillNum[num4], array[num4], array2[num4]);
					helperSkills[num4].SetSkillIcon(helperConfig.Id, num4 + 1);
				}
				else
				{
					helperSkills[num4].SetData(isUnlocked: false, helperConfig.AwakeSkillNum[num4], array[num4], array2[num4]);
					helperSkills[num4].SetSkillIcon(helperConfig.Id, num4 + 1);
				}
			}
			else
			{
				((Component)helperSkills[num4]).gameObject.SetActive(false);
			}
		}
		if ((Object)(object)skillLevelText != (Object)null)
		{
			((TMP_Text)skillLevelText).text = $"Lv.{currentLevel}";
		}
		int helperSkillLevel = Constants.GetHelperSkillLevel(helperConfig, currentLevel);
		int helperSkillLevel2 = Constants.GetHelperSkillLevel(helperConfig, currentLevel + 1);
		long itemCount = GameDataManager.Instance.GetItemCount(helperConfig.Id + 100);
		if (helperSkillLevel2 > helperSkillLevel)
		{
			((Component)suipianObject).gameObject.SetActive(true);
			isShowtip2 = true;
			SingletonMono<ResManager>.Instance.LoadSpriteAsync(Constants.GetSuipianIconPath(helperConfig.Id + 100), delegate(Sprite sprite)
			{
				if ((Object)(object)sprite != (Object)null)
				{
					suipian.sprite = sprite;
					if ((Object)(object)suipianown != (Object)null)
					{
						suipianown.sprite = sprite;
					}
					((Component)suipian).gameObject.SetActive(true);
				}
			});
			suipianCost = helperConfig.UpgradeCostType[helperSkillLevel];
			((TMP_Text)suipianText).text = $"{itemCount}/{suipianCost}";
			if ((Object)(object)suipianownText != (Object)null)
			{
				((TMP_Text)suipianownText).text = $"{itemCount}";
			}
		}
		else
		{
			isShowtip2 = false;
			((Component)suipianObject).gameObject.SetActive(false);
			suipianCost = 0;
			SingletonMono<ResManager>.Instance.LoadSpriteAsync(Constants.GetSuipianIconPath(helperConfig.Id + 100), delegate(Sprite sprite)
			{
				if ((Object)(object)sprite != (Object)null && (Object)(object)suipianown != (Object)null)
				{
					suipianown.sprite = sprite;
				}
			});
			if ((Object)(object)suipianownText != (Object)null)
			{
				((TMP_Text)suipianownText).text = $"{itemCount}";
			}
		}
		string text = "";
		for (int num5 = 0; num5 < helperConfig.UpgradeLevel.Count; num5++)
		{
			text = ((num5 != helperConfig.UpgradeLevel.Count - 1) ? ((num5 != helperSkillLevel2) ? (text + helperConfig.UpgradeLevel[num5] + "-") : (text + $"<color=#453110>{helperConfig.UpgradeLevel[num5]}</color>-")) : ((num5 != helperSkillLevel2) ? (text + helperConfig.UpgradeLevel[num5]) : (text + $"<color=#453110>{helperConfig.UpgradeLevel[num5]}</color>")));
		}
		((TMP_Text)upgradeLevelDescText).text = text + "    ";
		if ((Object)(object)upgradeLevelbg != (Object)null)
		{
			((TMP_Text)upgradeLevelDescText).enableWordWrapping = false;
			((TMP_Text)upgradeLevelDescText).overflowMode = (TextOverflowModes)0;
			((TMP_Text)upgradeLevelDescText).ForceMeshUpdate(false, false);
			((Graphic)upgradeLevelbg).rectTransform.anchoredPosition = new Vector2(((TMP_Text)upgradeLevelDescText).preferredWidth / ((float)helperConfig.UpgradeLevel.Count - 0.5f) * (float)helperSkillLevel2 - ((TMP_Text)upgradeLevelDescText).preferredWidth / 2f, ((Graphic)upgradeLevelbg).rectTransform.anchoredPosition.y);
			if (helperSkillLevel == helperConfig.UpgradeLevel.Count)
			{
				((Component)upgradeLevelbg).gameObject.SetActive(false);
			}
		}
		float num6 = helperConfig.BaseSkillNum + helperConfig.UpgradeSkillNum * (float)helperSkillLevel;
		float num7 = helperConfig.BaseSkillNum + helperConfig.UpgradeSkillNum * (float)helperSkillLevel2;
		((Component)skillItem1).gameObject.SetActive(true);
		string text2 = ((helperConfig.Id == 1) ? "倍" : ((helperConfig.Id != 4 && helperConfig.Id != 9 && helperConfig.Id != 10) ? ((helperConfig.Id != 5 && helperConfig.Id != 6) ? ((helperConfig.Id != 7 && helperConfig.Id != 8) ? "个" : "颗") : "个") : "范围"));
		if (helperConfig.Id == 4 || helperConfig.Id == 9 || helperConfig.Id == 10)
		{
			if ((Object)(object)skillDescText != (Object)null)
			{
				((TMP_Text)skillDescText).text = helperConfig.HelperDesc.ToLower().Replace("sss", num6.ToString("F1") ?? "");
			}
		}
		else if ((Object)(object)skillDescText != (Object)null)
		{
			((TMP_Text)skillDescText).text = helperConfig.HelperDesc.ToLower().Replace("sss", $"{num6:0}");
		}
		string text3 = ((helperConfig.Id == 4 || helperConfig.Id == 9 || helperConfig.Id == 10) ? "F1" : "0");
		if ((Object)(object)currentSkillText != (Object)null)
		{
			((TMP_Text)currentSkillText).text = num6.ToString(text3) + text2;
		}
		if ((Object)(object)nextSkillText != (Object)null)
		{
			((TMP_Text)nextSkillText).text = num7.ToString(text3) + text2;
		}
		for (int num8 = 0; num8 < helperConfig.AwakeSkillNum.Count; num8++)
		{
			_ = currentLevel;
			_ = helperConfig.AwakeSkillNum[num8];
		}
		float value = Mathf.Pow(helperConfig.HelperIncomeAddBuff, (float)(currentLevel - 1));
		float value2 = Mathf.Pow(helperConfig.HelperIncomeAddBuff, (float)currentLevel);
		text2 = "倍";
		if ((Object)(object)skillDescText2 != (Object)null)
		{
			string helperName = helperConfig.HelperName;
			((TMP_Text)skillDescText2).text = $"{helperName}翻硬币收益：";
		}
		if ((Object)(object)currentSkillText2 != (Object)null)
		{
			((TMP_Text)currentSkillText2).text = FormatToThreeSignificantDigits(value) + text2;
		}
		if ((Object)(object)nextSkillText2 != (Object)null)
		{
			((TMP_Text)nextSkillText2).text = FormatToThreeSignificantDigits(value2) + text2;
		}
		upgradeTip1.SetAttributeValues("", ((TMP_Text)skillDescText2).text, value2.ToString());
		if (isShowtip2)
		{
			upgradeTip2.SetAttributeValues("", ((TMP_Text)skillDescText).text, ((TMP_Text)nextSkillText).text);
		}
		double num9 = helperConfig.UpgradeNum * (double)Mathf.Pow(helperConfig.UpgradeNumCostBuff, (float)(currentLevel - 1));
		double totalGold = SingletonMono<UpgradeManager>.Instance.TotalGold;
		long diamond = GameDataManager.Instance.GetDiamond();
		if ((Object)(object)upgradeBtn != (Object)null)
		{
			((Selectable)upgradeBtn).interactable = true;
			((UnityEventBase)upgradeBtn.onClick).RemoveAllListeners();
			((UnityEvent)upgradeBtn.onClick).AddListener(new UnityAction(OnUpgradeClicked));
		}
		if (helperConfig.UpgradeType == 1)
		{
			_isSuipianEnough = itemCount >= suipianCost;
			_isGoldEnougth = totalGold >= num9;
			if (isOwned && currentLevel < helperConfig.UpgradeLevelMax && totalGold >= num9 && itemCount >= suipianCost)
			{
				((Component)UpgradeBtnImg1).gameObject.SetActive(true);
				((Component)UpgradeBtnImg2).gameObject.SetActive(false);
			}
			else
			{
				((Component)UpgradeBtnImg1).gameObject.SetActive(false);
				((Component)UpgradeBtnImg2).gameObject.SetActive(true);
			}
			if (totalGold >= num9)
			{
				((TMP_Text)moneyText).text = "<color=#15860d>" + NumberFormatter.FormatNumber(totalGold) + "</color>/" + NumberFormatter.FormatNumber(num9);
			}
			else
			{
				((TMP_Text)moneyText).text = "<color=#EE3333>" + NumberFormatter.FormatNumber(totalGold) + "</color>/" + NumberFormatter.FormatNumber(num9);
			}
			SingletonMono<ResManager>.Instance.LoadSpriteAsync(Constants.GetMoneyPath(1), delegate(Sprite sprite)
			{
				moneyIcon.sprite = sprite;
			}, isAtlas: false);
		}
		else if (helperConfig.UpgradeType == 2)
		{
			if (isOwned && currentLevel < helperConfig.UpgradeLevelMax && totalGold >= num9 && itemCount >= suipianCost)
			{
				((Component)UpgradeBtnImg1).gameObject.SetActive(true);
				((Component)UpgradeBtnImg2).gameObject.SetActive(false);
			}
			else
			{
				((Component)UpgradeBtnImg1).gameObject.SetActive(false);
				((Component)UpgradeBtnImg2).gameObject.SetActive(true);
			}
			_isSuipianEnough = itemCount >= suipianCost;
			_isDiamondEnougth = (double)diamond >= num9;
			if (totalGold >= num9)
			{
				((TMP_Text)moneyText).text = $"<color=#15860d>{diamond}</color>/{NumberFormatter.FormatNumber(num9)}";
			}
			else
			{
				((TMP_Text)moneyText).text = $"<color=#EE3333>{diamond}</color>/{NumberFormatter.FormatNumber(num9)}";
			}
			SingletonMono<ResManager>.Instance.LoadSpriteAsync(Constants.GetMoneyPath(2), delegate(Sprite sprite)
			{
				moneyIcon.sprite = sprite;
			}, isAtlas: false);
		}
		double num10 = helperConfig.BuyNum * (double)Mathf.Pow(helperConfig.BuyNumCostBuff[0], (float)(maxCount - 1));
		if (helperConfig.BuyType == 1 && maxCount <= helperConfig.BuyNumLevel)
		{
			if ((Object)(object)addhelperMoneyBtn != (Object)null)
			{
				((Component)addhelperMoneyBtn).gameObject.SetActive(true);
				((Component)addhelperDiamondBtn).gameObject.SetActive(false);
				((Component)addhelperAdsBtn).gameObject.SetActive(false);
				((Selectable)addhelperMoneyBtn).interactable = isOwned && totalGold >= num10;
				((UnityEventBase)addhelperMoneyBtn.onClick).RemoveAllListeners();
				((UnityEvent)addhelperMoneyBtn.onClick).AddListener(new UnityAction(OnAddHelperMoneyClicked));
			}
			if ((Object)(object)addhelperDiamondBtn != (Object)null)
			{
				((Component)addhelperDiamondBtn).gameObject.SetActive(false);
			}
			if ((Object)(object)addhelperMoneyCostText != (Object)null)
			{
				((TMP_Text)addhelperMoneyCostText).text = NumberFormatter.FormatNumber(num10) ?? "";
			}
		}
		else if (helperConfig.BuyType == 3)
		{
			if ((Object)(object)addhelperAdsBtn != (Object)null)
			{
				((Component)addhelperAdsBtn).gameObject.SetActive(true);
				((Component)addhelperDiamondBtn).gameObject.SetActive(false);
				((Component)addhelperMoneyBtn).gameObject.SetActive(false);
				((Selectable)addhelperAdsBtn).interactable = true;
				((UnityEventBase)addhelperAdsBtn.onClick).RemoveAllListeners();
				((UnityEvent)addhelperAdsBtn.onClick).AddListener(new UnityAction(OnAddHelperAdsClicked));
			}
		}
		else
		{
			num10 = helperConfig.BuyNumDiamond + (double)(helperConfig.BuyNumCostBuff[1] * (float)(maxCount - 1));
			if (helperConfig.BuyType == 1)
			{
				num10 = helperConfig.BuyNumDiamond + (double)(helperConfig.BuyNumCostBuff[1] * (float)(maxCount - helperConfig.BuyNumLevel - 1));
			}
			if ((Object)(object)addhelperDiamondBtn != (Object)null)
			{
				((Component)addhelperDiamondBtn).gameObject.SetActive(true);
				((Component)addhelperMoneyBtn).gameObject.SetActive(false);
				((Component)addhelperAdsBtn).gameObject.SetActive(false);
				((Selectable)addhelperDiamondBtn).interactable = isOwned && (double)diamond >= num10;
				((UnityEventBase)addhelperDiamondBtn.onClick).RemoveAllListeners();
				((UnityEvent)addhelperDiamondBtn.onClick).AddListener(new UnityAction(OnAddHelperDiamondClicked));
			}
			if ((Object)(object)addhelperMoneyBtn != (Object)null)
			{
				((Component)addhelperMoneyBtn).gameObject.SetActive(false);
			}
			if ((Object)(object)addhelperDiamondCostText != (Object)null)
			{
				((TMP_Text)addhelperDiamondCostText).text = NumberFormatter.FormatNumber(num10) ?? "";
			}
		}
		if (currentLevel >= helperConfig.UpgradeLevelMax)
		{
			if ((Object)(object)upgradeBtn != (Object)null)
			{
				((Component)upgradeBtn).gameObject.SetActive(false);
			}
			if ((Object)(object)maxObject != (Object)null)
			{
				((Component)maxObject).gameObject.SetActive(true);
			}
		}
		else
		{
			if ((Object)(object)upgradeBtn != (Object)null)
			{
				((Component)upgradeBtn).gameObject.SetActive(true);
			}
			if ((Object)(object)maxObject != (Object)null)
			{
				((Component)maxObject).gameObject.SetActive(false);
			}
		}
		int num11 = num;
		if (num11 > 2)
		{
			num11 = 2;
		}
		SingletonMono<ResManager>.Instance.LoadSpriteAsync($"Assets/GameRes/NewUI/Common/images1/wupin_btn_shengji0{num11}.png", delegate(Sprite sprite)
		{
			if ((Object)(object)sprite != (Object)null)
			{
				addhelperMoneyBtnIcon.sprite = sprite;
			}
		}, isAtlas: false);
	}

	private void PlayEffect()
	{
		SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/Role/button_upgrade3.prefab", delegate(GameObject prefab)
		{
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Expected O, but got Unknown
			if ((Object)(object)prefab != (Object)null)
			{
				GameObject effect = Object.Instantiate<GameObject>(prefab, ((Component)helperIcon).transform);
				effect.transform.localPosition = Vector3.one * 0.8f;
				Transform transform = effect.transform;
				((RectTransform)((transform is RectTransform) ? transform : null)).anchoredPosition = new Vector2(0f, -103f);
				SkeletonGraphic component = effect.GetComponent<SkeletonGraphic>();
				component.AnimationState.SetAnimation(0, "upgrade", false);
				component.AnimationState.Complete += (TrackEntryDelegate)delegate
				{
					Object.Destroy((Object)(object)effect);
				};
			}
		});
	}

	private void PlayUpgrade1()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		HelperUpgradeAttributePopup helperUpgradeAttributePopup = Object.Instantiate<HelperUpgradeAttributePopup>(upgradeTip1, (Transform)(object)tipcon);
		Transform transform = ((Component)helperUpgradeAttributePopup).transform;
		((RectTransform)((transform is RectTransform) ? transform : null)).anchoredPosition = new Vector2(0f, 0f);
		float num = Mathf.Pow(helperConfig.HelperIncomeAddBuff, (float)currentLevel);
		helperUpgradeAttributePopup.SetAttributeValues("", "帮手翻硬币收益*", $"{num:F2}倍", showUpgradeValue: true, addPlusPrefix: false);
		helperUpgradeAttributePopup.ShowWithAnimation();
	}

	private void PlayUpgrade2()
	{
		if (isShowtip2)
		{
			HelperUpgradeAttributePopup helperUpgradeAttributePopup = Object.Instantiate<HelperUpgradeAttributePopup>(upgradeTip2, (Transform)(object)tipcon);
			((Component)helperUpgradeAttributePopup).transform.SetParent(((Component)this).transform, true);
			int helperSkillLevel = Constants.GetHelperSkillLevel(helperConfig, currentLevel + 1);
			float num = helperConfig.BaseSkillNum + helperConfig.UpgradeSkillNum * (float)helperSkillLevel;
			string property = ((helperConfig.Id != 4 && helperConfig.Id != 9 && helperConfig.Id != 10) ? helperConfig.HelperDesc.ToLower().Replace("sss", $"<color=#99FD25>{num:0}</color>") : helperConfig.HelperDesc.ToLower().Replace("sss", $"<color=#99FD25>{num:F1}</color>"));
			helperUpgradeAttributePopup.SetAttributeValues("", property, "", showUpgradeValue: false, addPlusPrefix: false);
			helperUpgradeAttributePopup.ShowWithAnimation();
		}
	}

	private void OnUpgradeClicked()
	{
		double num = helperConfig.UpgradeNum * (double)Mathf.Pow(helperConfig.UpgradeNumCostBuff, (float)(currentLevel - 1));
		double totalGold = SingletonMono<UpgradeManager>.Instance.TotalGold;
		long itemCount = GameDataManager.Instance.GetItemCount(helperConfig.Id + 100);
		if (isOwned && currentLevel < helperConfig.UpgradeLevelMax && totalGold >= num && itemCount >= suipianCost)
		{
			double num2 = 0.0;
			bool flag = false;
			int num3 = suipianCost;
			if (helperConfig.UpgradeType == 3)
			{
				if (num3 > 0)
				{
					GameDataManager.Instance.RemoveItem(100 + helperConfig.Id, num3);
				}
				FakeVideoManager.Instance.ShowAd(delegate
				{
					SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/upgrade.mp3");
					currentLevel++;
					GameDataManager.Instance.SetHelperLevel(helperConfig.Id, currentLevel);
					WorldCupDataManager.Instance.AddWorldCupMissionProgress(1, 1);
					UpdateUI();
					onUpgradeChanged?.Invoke(helperConfig.Id);
					PlayEffect();
				}, delegate
				{
					SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/cantbuy.mp3");
				}, "帮手升级");
				return;
			}
			if (helperConfig.UpgradeType == 1)
			{
				num2 = helperConfig.UpgradeNum * (double)Mathf.Pow(helperConfig.UpgradeNumCostBuff, (float)(currentLevel - 1));
				flag = SingletonMono<UpgradeManager>.Instance.TrySpendGold(num2);
				if (num3 > 0)
				{
					flag = flag && GameDataManager.Instance.RemoveItem(100 + helperConfig.Id, num3);
				}
			}
			else if (helperConfig.UpgradeType == 2)
			{
				num2 = helperConfig.UpgradeNum * (double)Mathf.Pow(helperConfig.UpgradeNumCostBuff, (float)(currentLevel - 1));
				flag = GameDataManager.Instance.RemoveDiamond((int)num2);
				if (num3 > 0)
				{
					flag = flag && GameDataManager.Instance.RemoveItem(100 + helperConfig.Id, num3);
				}
			}
			if (flag)
			{
				SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/upgrade.mp3");
				PlayUpgrade1();
				PlayUpgrade2();
				currentLevel++;
				GameDataManager.Instance.SetHelperLevel(helperConfig.Id, currentLevel);
				WorldCupDataManager.Instance.AddWorldCupMissionProgress(1, 1);
				UpdateUI();
				onUpgradeChanged?.Invoke(helperConfig.Id);
				PlayEffect();
				FacilityRedDot.CheckAllRedDots();
			}
			else
			{
				SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/cantbuy.mp3");
			}
		}
		else if (helperConfig.UpgradeType == 1)
		{
			if (suipianCost > 0 && !_isSuipianEnough)
			{
				UIManager.Instance.ShowShardNotEnoughDialog("去商城补充一些碎片吧", delegate
				{
					OnCloseClicked();
					UIManager.Instance.upgradePanel?.OnButtonClicked(5);
				});
			}
			else
			{
				UIManager.Instance.ShowSupportUI(DialogType.money, 0.0);
			}
		}
		else if (suipianCost > 0 && !_isSuipianEnough)
		{
			UIManager.Instance.ShowShardNotEnoughDialog("去商城补充一些碎片吧", delegate
			{
				OnCloseClicked();
				UIManager.Instance.upgradePanel?.OnButtonClicked(5);
			});
		}
		else
		{
			float commonFactor = ConfigManager.Instance.GetCommonFactor(25);
			UIManager.Instance.ShowSupportUI(DialogType.diamond, commonFactor);
		}
	}

	private void OnGoldUpgradeClicked()
	{
		if (currentLevel < helperConfig.UpgradeLevelMax)
		{
			double amount = helperConfig.UpgradeNum * (double)Mathf.Pow(helperConfig.UpgradeNumCostBuff, (float)(currentLevel - 1));
			if (SingletonMono<UpgradeManager>.Instance.TrySpendGold(amount))
			{
				SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/upgrade.mp3");
				currentLevel++;
				GameDataManager.Instance.SetHelperLevel(helperConfig.Id, currentLevel);
				UpdateUI();
				onUpgradeChanged?.Invoke(helperConfig.Id);
				PlayEffect();
			}
			else
			{
				SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/cantbuy.mp3");
			}
		}
	}

	private void OnDiamondUpgradeClicked()
	{
		if (currentLevel < helperConfig.UpgradeLevelMax)
		{
			double num = helperConfig.UpgradeNum + (double)(helperConfig.UpgradeNumCostBuff * (float)(currentLevel - 1));
			if (GameDataManager.Instance.RemoveDiamond((int)num))
			{
				SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/upgrade.mp3");
				currentLevel++;
				GameDataManager.Instance.SetHelperLevel(helperConfig.Id, currentLevel);
				UpdateUI();
				onUpgradeChanged?.Invoke(helperConfig.Id);
				PlayEffect();
			}
			else
			{
				SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/cantbuy.mp3");
			}
		}
	}

	private void OnAddHelperAdsClicked()
	{
		FakeVideoManager.Instance.ShowAd(delegate
		{
			SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/upgrade.mp3");
			maxCount++;
			GameDataManager.Instance.SetHelperMaxCount(helperConfig.Id, maxCount);
			UpdateUI();
			onLimitChanged?.Invoke(obj: true);
		}, delegate
		{
			SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/cantbuy.mp3");
		}, "帮手升级2");
	}

	private void OnAddHelperMoneyClicked()
	{
		double amount = helperConfig.BuyNum * (double)Mathf.Pow(helperConfig.BuyNumCostBuff[0], (float)(maxCount - 1));
		if (SingletonMono<UpgradeManager>.Instance.TrySpendGold(amount))
		{
			SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/upgrade.mp3");
			maxCount++;
			GameDataManager.Instance.SetHelperMaxCount(helperConfig.Id, maxCount);
			UpdateUI();
			onLimitChanged?.Invoke(obj: true);
		}
		else
		{
			SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/cantbuy.mp3");
		}
	}

	private void OnAddHelperDiamondClicked()
	{
		double num = helperConfig.BuyNumDiamond + (double)(helperConfig.BuyNumCostBuff[1] * (float)(maxCount - 1));
		if (helperConfig.BuyType == 1)
		{
			num = helperConfig.BuyNumDiamond + (double)(helperConfig.BuyNumCostBuff[1] * (float)(maxCount - helperConfig.BuyNumLevel - 1));
		}
		if (GameDataManager.Instance.RemoveDiamond((int)num))
		{
			SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/upgrade.mp3");
			maxCount++;
			GameDataManager.Instance.SetHelperMaxCount(helperConfig.Id, maxCount);
			UpdateUI();
			onLimitChanged?.Invoke(obj: true);
			BroadcastManager.ShowPopUpTip("增加成功");
		}
		else
		{
			BroadcastManager.ShowPopUpTip("增加失败");
			SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/cantbuy.mp3");
		}
	}

	private void OnCloseClicked()
	{
		SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
		((Component)this).gameObject.SetActive(false);
	}

	private string FormatToThreeSignificantDigits(float value)
	{
		if (value >= 100f)
		{
			return NumberFormatter.FormatNumber(value);
		}
		if (value >= 10f)
		{
			return value.ToString("F1");
		}
		return value.ToString("F2");
	}

	private void OnDestroy()
	{
		if ((Object)(object)goldUpgradeBtn != (Object)null)
		{
			((UnityEventBase)goldUpgradeBtn.onClick).RemoveAllListeners();
		}
		if ((Object)(object)diamondUpgradeBtn != (Object)null)
		{
			((UnityEventBase)diamondUpgradeBtn.onClick).RemoveAllListeners();
		}
		if ((Object)(object)addhelperMoneyBtn != (Object)null)
		{
			((UnityEventBase)addhelperMoneyBtn.onClick).RemoveAllListeners();
		}
		if ((Object)(object)addhelperDiamondBtn != (Object)null)
		{
			((UnityEventBase)addhelperDiamondBtn.onClick).RemoveAllListeners();
		}
		if ((Object)(object)closeButton != (Object)null)
		{
			((UnityEventBase)closeButton.onClick).RemoveAllListeners();
		}
		if ((Object)(object)bgButton != (Object)null)
		{
			((UnityEventBase)bgButton.onClick).RemoveAllListeners();
		}
	}
}

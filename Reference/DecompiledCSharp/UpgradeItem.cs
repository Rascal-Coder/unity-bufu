using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using cfg;

public class UpgradeItem : MonoBehaviour
{
	[Header("基础组件")]
	[SerializeField]
	private Image bgImg;

	[SerializeField]
	private Image levelMaxBg;

	[SerializeField]
	private Image unlockImg;

	[SerializeField]
	private Image itemIcon;

	[Header("文本组件")]
	[SerializeField]
	private TextMeshProUGUI itemNameText;

	[SerializeField]
	private TextMeshProUGUI lvTmp;

	[SerializeField]
	private TextMeshProUGUI lockTmp;

	[Header("按钮组件")]
	[SerializeField]
	private Button upgradeBtn;

	[SerializeField]
	private Image upgradeBtnImage;

	[SerializeField]
	private TextMeshProUGUI upgradeMoneyTmp;

	[SerializeField]
	private Image lvImg;

	[SerializeField]
	private Sprite diamondUpgradeBtnSprite;

	[SerializeField]
	private UpDownAnimator lvAnimator;

	[Header("升级反馈")]
	[SerializeField]
	private GameObject upgradeTips;

	[SerializeField]
	private TextMeshProUGUI lvTipsTmp;

	[SerializeField]
	private SkeletonGraphic buttonUpgradeSpine;

	[Header("图片资源")]
	[SerializeField]
	private Sprite normalBtnSprite;

	[SerializeField]
	private Sprite disabledBtnSprite;

	[Header("图片资源钻石升级")]
	[SerializeField]
	private Sprite normalDiamondBtnSprite;

	[SerializeField]
	private Sprite disabledDiamondBtnSprite;

	[Header("广告升级图片")]
	[SerializeField]
	private Sprite adBtnSprite;

	[Header("提示按钮")]
	[SerializeField]
	private Button tipBtn;

	[Header("等级提升到最大时提示")]
	[SerializeField]
	private GameObject maxLevelTip;

	[Header("进化按钮")]
	[SerializeField]
	private Button evolveBtn;

	private int _itemId;

	private string _configType;

	private int _costType;

	private int _currentLevel;

	private int _maxLevel;

	private bool _isUnlocked;

	private double _upgradeCost;

	private bool _isUpgradeable;

	private bool _isPlayingUpgradeFeedback;

	private bool _isDisposed;

	private string _loadedIconPath;

	private string _loadingIconPath;

	private int _helperAdCap;

	private Vector2 _tipsInitAnchoredPos;

	private Tween _tipsHideTween;

	public int position;

	private const string EvolvePanelPrefabPath = "Assets/GameRes/NewUI/Evolve/prefabs/EvolvePanel.prefab";

	public int itemId => _itemId;

	public bool isUpgradeable => _isUpgradeable;

	public event Action<UpgradeItem, string> OnTipButtonClick;

	public event Action<UpgradeItem> onUpgradeButtonClick;

	private void OnTipButton()
	{
		this.OnTipButtonClick?.Invoke(this, _configType);
	}

	private void Awake()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)upgradeBtn != (Object)null)
		{
			((UnityEvent)upgradeBtn.onClick).AddListener(new UnityAction(OnUpgradeButtonClick));
		}
		if ((Object)(object)tipBtn != (Object)null)
		{
			((UnityEvent)tipBtn.onClick).AddListener(new UnityAction(OnTipButton));
		}
		if ((Object)(object)evolveBtn != (Object)null)
		{
			((UnityEvent)evolveBtn.onClick).AddListener(new UnityAction(OnEvolveButtonClick));
		}
		if ((Object)(object)levelMaxBg != (Object)null)
		{
			((Component)levelMaxBg).gameObject.SetActive(false);
		}
		if ((Object)(object)maxLevelTip != (Object)null)
		{
			maxLevelTip.gameObject.SetActive(false);
		}
		if ((Object)(object)unlockImg != (Object)null)
		{
			((Component)unlockImg).gameObject.SetActive(false);
		}
		if ((Object)(object)lockTmp != (Object)null)
		{
			((Component)lockTmp).gameObject.SetActive(false);
		}
		if ((Object)(object)upgradeTips != (Object)null)
		{
			RectTransform component = upgradeTips.GetComponent<RectTransform>();
			if ((Object)(object)component != (Object)null)
			{
				_tipsInitAnchoredPos = component.anchoredPosition;
			}
		}
		AddDisableListener();
	}

	private void AddDisableListener()
	{
		if ((Object)(object)upgradeBtn == (Object)null)
		{
			return;
		}
		GameObjectClickListener gameObjectClickListener = GameObjectClickListener.Get(((Component)upgradeBtn).gameObject);
		gameObjectClickListener.onClick = (GameObjectClickListener.VoidDelegate)Delegate.Combine(gameObjectClickListener.onClick, (GameObjectClickListener.VoidDelegate)delegate
		{
			if (_costType == 1)
			{
				if ((Object)(object)upgradeBtnImage.sprite == (Object)(object)disabledBtnSprite)
				{
					UIManager.Instance.ShowSupportUI(DialogType.money, 0.0);
				}
			}
			else if ((Object)(object)upgradeBtnImage.sprite == (Object)(object)disabledDiamondBtnSprite)
			{
				float commonFactor = ConfigManager.Instance.GetCommonFactor(25);
				UIManager.Instance.ShowSupportUI(DialogType.diamond, commonFactor);
			}
		});
	}

	private void OnDisable()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		_isPlayingUpgradeFeedback = false;
		if (_tipsHideTween != null)
		{
			TweenExtensions.Kill(_tipsHideTween, false);
			_tipsHideTween = null;
		}
		if ((Object)(object)upgradeTips != (Object)null)
		{
			RectTransform component = upgradeTips.GetComponent<RectTransform>();
			if ((Object)(object)component != (Object)null)
			{
				ShortcutExtensions.DOKill((Component)(object)component, false);
				component.anchoredPosition = _tipsInitAnchoredPos;
			}
			upgradeTips.SetActive(false);
		}
		if ((Object)(object)buttonUpgradeSpine != (Object)null)
		{
			if (buttonUpgradeSpine.AnimationState != null)
			{
				buttonUpgradeSpine.AnimationState.Complete -= new TrackEntryDelegate(OnUpgradeAnimComplete);
				buttonUpgradeSpine.AnimationState.ClearTrack(0);
			}
			((Component)buttonUpgradeSpine).gameObject.SetActive(false);
		}
	}

	private void OnDestroy()
	{
		PrepareForDestroy();
	}

	public void PrepareForDestroy()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Expected O, but got Unknown
		if (!_isDisposed)
		{
			_isDisposed = true;
			if ((Object)(object)upgradeBtn != (Object)null)
			{
				((UnityEvent)upgradeBtn.onClick).RemoveListener(new UnityAction(OnUpgradeButtonClick));
			}
			if ((Object)(object)tipBtn != (Object)null)
			{
				((UnityEvent)tipBtn.onClick).RemoveListener(new UnityAction(OnTipButton));
			}
			if ((Object)(object)evolveBtn != (Object)null)
			{
				((UnityEvent)evolveBtn.onClick).RemoveListener(new UnityAction(OnEvolveButtonClick));
			}
			if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
			{
				SingletonMono<UpgradeManager>.Instance.OnTypedUpgradeSuccess -= OnTypedUpgradeSuccess;
				SingletonMono<UpgradeManager>.Instance.OnGoldChanged -= OnGoldChanged;
				SingletonMono<UpgradeManager>.Instance.OnCoinCountChanged -= OnCoinCountChanged;
			}
			this.OnTipButtonClick = null;
			this.onUpgradeButtonClick = null;
		}
	}

	public void Initialize(int itemId, string configType)
	{
		_isDisposed = false;
		_itemId = itemId;
		_configType = configType;
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
		{
			SingletonMono<UpgradeManager>.Instance.OnTypedUpgradeSuccess += OnTypedUpgradeSuccess;
			SingletonMono<UpgradeManager>.Instance.OnGoldChanged += OnGoldChanged;
			SingletonMono<UpgradeManager>.Instance.OnCoinCountChanged += OnCoinCountChanged;
		}
		if (_configType == "itemUpgrade2")
		{
			position = ConfigManager.Instance.GetUpgrade2Config(_itemId)?.Position ?? 0;
		}
		else if (_configType == "itemUpgrade3")
		{
			position = ConfigManager.Instance.GetUpgrade3Config(_itemId)?.Position ?? 0;
		}
		RefreshData();
		RefreshUI();
	}

	private void RefreshData()
	{
		_isUpgradeable = false;
		_currentLevel = SingletonMono<UpgradeManager>.Instance.GetUpgradeLevel(_itemId, _configType);
		_maxLevel = ConfigManager.Instance.GetUpgradeMaxLevel(_itemId, _configType);
		int num = GameDataManager.Instance?.GetCurrentWorldId() ?? 1;
		if (_configType == "itemUpgrade2")
		{
			if (TalentDataManager.Instance != null && (_itemId <= 3 || _itemId == 14 || (_itemId >= 101 && _itemId <= 103) || _itemId == 110))
			{
				switch (num)
				{
				case 1:
					_maxLevel = ConfigManager.Instance.GetUpgradeMaxLevel(_itemId, _configType) + TalentDataManager.Instance.GetMaxLevelUp();
					break;
				case 2:
					_maxLevel = ConfigManager.Instance.GetUpgradeMaxLevel(_itemId, _configType) + (int)TalentDataManager.Instance.GetDiceTalentBuff(301);
					break;
				}
			}
		}
		else if (_configType == "itemUpgrade1" && (_itemId == 1 || _itemId == 101))
		{
			switch (num)
			{
			case 1:
				_maxLevel = ConfigManager.Instance.GetUpgradeMaxLevel(itemId, _configType) + GameDataManager.Instance.GetTalentJingYing();
				break;
			case 2:
				_maxLevel = ConfigManager.Instance.GetUpgradeMaxLevel(itemId, _configType) + (int)TalentDataManager.Instance.GetDiceTalentBuff(201);
				break;
			}
		}
		_isUnlocked = SingletonMono<UpgradeManager>.Instance.IsUnlocked(_itemId, _configType);
		if (_configType == "itemUpgrade1" && (_itemId == 1 || _itemId == 101))
		{
			_helperAdCap = GameDataManager.Instance.GetHelperADLevel();
			int num2 = Mathf.CeilToInt((float)_currentLevel / 5f) * 5;
			if (num2 < 10)
			{
				num2 = 10;
			}
			if (num2 > _maxLevel)
			{
				num2 = _maxLevel;
			}
			while (_helperAdCap < num2)
			{
				GameDataManager.Instance.UpgradeHelperADLevel();
				_helperAdCap = GameDataManager.Instance.GetHelperADLevel();
			}
		}
		if (_configType == "itemUpgrade1" && (_itemId == 1 || _itemId == 101))
		{
			float num3 = 1f;
			if ((Object)(object)GameDataManager.Instance != (Object)null)
			{
				switch (num)
				{
				case 1:
					num3 = GameDataManager.Instance.GetLaoDongLi();
					if (num3 <= 0f)
					{
						num3 = 1f;
					}
					break;
				case 2:
					num3 = TalentDataManager.Instance?.GetDiceTalentBuff(213) ?? 1f;
					break;
				}
			}
			_upgradeCost = SingletonMono<UpgradeManager>.Instance.GetUpgradeCost(_itemId, _configType) * (double)num3;
		}
		else
		{
			_upgradeCost = SingletonMono<UpgradeManager>.Instance.GetUpgradeCost(_itemId, _configType);
		}
		if (_costType == 1 && num == 2)
		{
			_upgradeCost *= TalentDataManager.Instance?.GetDiceTalentBuff(105) ?? 1f;
		}
		_costType = SingletonMono<UpgradeManager>.Instance.GetUpgradeCostType(_itemId, _configType);
	}

	private void RefreshUI()
	{
		if (!_isPlayingUpgradeFeedback)
		{
			if ((Object)(object)buttonUpgradeSpine != (Object)null)
			{
				((Component)buttonUpgradeSpine).gameObject.SetActive(false);
			}
			if ((Object)(object)upgradeTips != (Object)null)
			{
				upgradeTips.SetActive(false);
			}
		}
		string text = ((_configType == "itemUpgrade1") ? (ConfigManager.Instance.GetUpgrade1Config(_itemId)?.ItemName ?? "未知升级") : ((!(_configType == "itemUpgrade2")) ? (ConfigManager.Instance.GetUpgrade3Config(_itemId)?.ItemName ?? "未知升级") : (ConfigManager.Instance.GetUpgrade2Config(_itemId)?.ItemName ?? "未知升级")));
		if ((Object)(object)itemNameText != (Object)null)
		{
			((TMP_Text)itemNameText).text = text;
		}
		bool flag = _currentLevel >= _maxLevel && _maxLevel > 0;
		if (!_isUnlocked)
		{
			ShowLockedState();
		}
		else if (flag)
		{
			ShowMaxLevelState();
		}
		else
		{
			ShowNormalState();
		}
		RefreshItemIcon();
	}

	private void RefreshItemIcon()
	{
		string iconPath = Constants.GetUpgradeItemIcon(_itemId, _configType);
		if (_loadedIconPath == iconPath && (Object)(object)itemIcon != (Object)null && (Object)(object)itemIcon.sprite != (Object)null)
		{
			((Component)itemIcon).gameObject.SetActive(true);
		}
		else
		{
			if (_loadingIconPath == iconPath)
			{
				return;
			}
			_loadingIconPath = iconPath;
			SingletonMono<ResManager>.Instance.LoadSpriteAsync(iconPath, delegate(Sprite sprite)
			{
				if (!((Object)(object)this == (Object)null) && !((Object)(object)itemIcon == (Object)null) && !(_loadingIconPath != iconPath))
				{
					_loadingIconPath = null;
					if ((Object)(object)sprite != (Object)null)
					{
						_loadedIconPath = iconPath;
						itemIcon.sprite = sprite;
						((Component)itemIcon).gameObject.SetActive(true);
					}
				}
			}, isAtlas: false);
		}
	}

	private void ShowLockedState()
	{
		if ((Object)(object)upgradeBtn != (Object)null)
		{
			((Component)upgradeBtn).gameObject.SetActive(false);
		}
		if ((Object)(object)upgradeBtnImage != (Object)null)
		{
			((Component)upgradeBtnImage).gameObject.SetActive(false);
		}
		if ((Object)(object)upgradeMoneyTmp != (Object)null)
		{
			((Component)upgradeMoneyTmp).gameObject.SetActive(false);
		}
		if ((Object)(object)lvImg != (Object)null)
		{
			((Component)lvImg).gameObject.SetActive(false);
		}
		if ((Object)(object)lvTmp != (Object)null)
		{
			((Component)lvTmp).gameObject.SetActive(false);
		}
		if ((Object)(object)evolveBtn != (Object)null)
		{
			((Component)evolveBtn).gameObject.SetActive(false);
		}
		if ((Object)(object)unlockImg != (Object)null)
		{
			((Component)unlockImg).gameObject.SetActive(true);
		}
		else
		{
			Debug.LogError((object)$"[ShowLockedState] id={_itemId} → unlockImg 为 NULL！请在 Inspector 中赋值");
		}
		if ((Object)(object)lockTmp != (Object)null)
		{
			((Component)lockTmp).gameObject.SetActive(true);
			string lockConditionText = GetLockConditionText();
			((TMP_Text)lockTmp).text = lockConditionText;
		}
		else
		{
			Debug.LogError((object)$"[ShowLockedState] id={_itemId} → lockTmp 为 NULL！请在 Inspector 中赋值");
		}
		if ((Object)(object)levelMaxBg != (Object)null)
		{
			((Component)levelMaxBg).gameObject.SetActive(false);
		}
		if ((Object)(object)maxLevelTip != (Object)null)
		{
			maxLevelTip.gameObject.SetActive(false);
		}
	}

	private void ShowMaxLevelState()
	{
		if ((Object)(object)upgradeBtn != (Object)null)
		{
			((Component)upgradeBtn).gameObject.SetActive(false);
		}
		if ((Object)(object)upgradeBtnImage != (Object)null)
		{
			((Component)upgradeBtnImage).gameObject.SetActive(false);
		}
		if ((Object)(object)upgradeMoneyTmp != (Object)null)
		{
			((Component)upgradeMoneyTmp).gameObject.SetActive(false);
		}
		if ((Object)(object)lvImg != (Object)null)
		{
			((Component)lvImg).gameObject.SetActive(false);
		}
		if ((Object)(object)evolveBtn != (Object)null)
		{
			((Component)evolveBtn).gameObject.SetActive(false);
		}
		if ((Object)(object)unlockImg != (Object)null)
		{
			((Component)unlockImg).gameObject.SetActive(false);
		}
		if ((Object)(object)lockTmp != (Object)null)
		{
			((Component)lockTmp).gameObject.SetActive(false);
		}
		if ((Object)(object)levelMaxBg != (Object)null)
		{
			((Component)levelMaxBg).gameObject.SetActive(true);
		}
		if (_configType == "itemUpgrade1")
		{
			if ((_itemId == 1 || _itemId == 101) && (Object)(object)maxLevelTip != (Object)null)
			{
				maxLevelTip.gameObject.SetActive(true);
			}
		}
		else if (_configType == "itemUpgrade2" && (_itemId == 1 || _itemId == 2 || _itemId == 3 || _itemId == 101 || _itemId == 102 || _itemId == 103) && (Object)(object)maxLevelTip != (Object)null)
		{
			maxLevelTip.gameObject.SetActive(true);
		}
		if (!((Object)(object)lvTmp != (Object)null))
		{
			return;
		}
		((Component)lvTmp).gameObject.SetActive(true);
		if (_configType == "itemUpgrade1" && UpgradeManager.IsEvolvableCurrencyUpgrade(_itemId))
		{
			int num = SingletonMono<UpgradeManager>.Instance?.GetNextEvolutionLevel(_itemId, _currentLevel) ?? (-1);
			if (num > 0)
			{
				((TMP_Text)lvTmp).text = $"{_currentLevel}/{num}";
			}
			else
			{
				((TMP_Text)lvTmp).text = $"{_currentLevel}";
			}
		}
		else if (_maxLevel > 0 && _maxLevel < 999999)
		{
			((TMP_Text)lvTmp).text = $"{_currentLevel}/{_maxLevel}";
		}
		else
		{
			((TMP_Text)lvTmp).text = $"{_currentLevel}";
		}
	}

	private void ShowNormalState()
	{
		if (_configType == "itemUpgrade1" && (Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null && UpgradeManager.IsEvolvableCurrencyUpgrade(_itemId) && SingletonMono<UpgradeManager>.Instance.CanEvolveCurrency(_itemId))
		{
			if ((Object)(object)upgradeBtn != (Object)null)
			{
				((Component)upgradeBtn).gameObject.SetActive(false);
			}
			if ((Object)(object)upgradeBtnImage != (Object)null)
			{
				((Component)upgradeBtnImage).gameObject.SetActive(false);
			}
			if ((Object)(object)upgradeMoneyTmp != (Object)null)
			{
				((Component)upgradeMoneyTmp).gameObject.SetActive(false);
			}
			if ((Object)(object)lvImg != (Object)null)
			{
				((Component)lvImg).gameObject.SetActive(true);
			}
			if ((Object)(object)evolveBtn != (Object)null)
			{
				((Component)evolveBtn).gameObject.SetActive(true);
			}
		}
		else
		{
			if ((Object)(object)upgradeBtn != (Object)null)
			{
				((Component)upgradeBtn).gameObject.SetActive(true);
			}
			if ((Object)(object)upgradeBtnImage != (Object)null)
			{
				((Component)upgradeBtnImage).gameObject.SetActive(true);
			}
			if ((Object)(object)upgradeMoneyTmp != (Object)null)
			{
				((Component)upgradeMoneyTmp).gameObject.SetActive(true);
			}
			if ((Object)(object)lvImg != (Object)null)
			{
				((Component)lvImg).gameObject.SetActive(true);
			}
			if ((Object)(object)evolveBtn != (Object)null)
			{
				((Component)evolveBtn).gameObject.SetActive(false);
			}
		}
		if ((Object)(object)unlockImg != (Object)null)
		{
			((Component)unlockImg).gameObject.SetActive(false);
		}
		if ((Object)(object)lockTmp != (Object)null)
		{
			((Component)lockTmp).gameObject.SetActive(false);
		}
		if ((Object)(object)levelMaxBg != (Object)null)
		{
			((Component)levelMaxBg).gameObject.SetActive(false);
		}
		if ((Object)(object)maxLevelTip != (Object)null)
		{
			maxLevelTip.gameObject.SetActive(false);
		}
		bool num = _configType == "itemUpgrade1" && (_itemId == 1 || _itemId == 101) && _currentLevel == _helperAdCap && _helperAdCap < _maxLevel;
		if ((Object)(object)lvTmp != (Object)null)
		{
			((Component)lvTmp).gameObject.SetActive(true);
			if (_configType == "itemUpgrade1" && (_itemId == 1 || _itemId == 101))
			{
				((TMP_Text)lvTmp).text = $"{_currentLevel}/{_helperAdCap}";
			}
			else if (_configType == "itemUpgrade1" && UpgradeManager.IsEvolvableCurrencyUpgrade(_itemId))
			{
				int num2 = SingletonMono<UpgradeManager>.Instance?.GetNextEvolutionLevel(_itemId, _currentLevel) ?? (-1);
				if (num2 > 0)
				{
					((TMP_Text)lvTmp).text = $"{_currentLevel}/{num2}";
				}
				else
				{
					((TMP_Text)lvTmp).text = $"{_currentLevel}";
				}
			}
			else if (_maxLevel > 0 && _maxLevel < 999999)
			{
				((TMP_Text)lvTmp).text = $"{_currentLevel}/{_maxLevel}";
			}
			else
			{
				((TMP_Text)lvTmp).text = $"{_currentLevel}";
			}
		}
		if (num)
		{
			if ((Object)(object)upgradeMoneyTmp != (Object)null)
			{
				((Component)upgradeMoneyTmp).gameObject.SetActive(false);
			}
			if ((Object)(object)adBtnSprite != (Object)null && (Object)(object)upgradeBtnImage != (Object)null)
			{
				upgradeBtnImage.sprite = adBtnSprite;
			}
			((Selectable)upgradeBtn).interactable = true;
			if ((Object)(object)lvAnimator != (Object)null)
			{
				((Component)lvAnimator).gameObject.SetActive(true);
				((Behaviour)lvAnimator).enabled = true;
			}
			_isUpgradeable = true;
		}
		else
		{
			if ((Object)(object)upgradeMoneyTmp != (Object)null)
			{
				((TMP_Text)upgradeMoneyTmp).text = NumberFormatter.FormatNumber(_upgradeCost);
			}
			UpdateButtonState();
		}
	}

	private void UpdateButtonState()
	{
		if ((Object)(object)upgradeBtnImage == (Object)null)
		{
			return;
		}
		if (_configType == "itemUpgrade1" && (_itemId == 1 || _itemId == 101) && _currentLevel == _helperAdCap && _helperAdCap < _maxLevel)
		{
			if ((Object)(object)adBtnSprite != (Object)null)
			{
				upgradeBtnImage.sprite = adBtnSprite;
			}
			((Selectable)upgradeBtn).interactable = true;
			if ((Object)(object)lvAnimator != (Object)null)
			{
				((Component)lvAnimator).gameObject.SetActive(true);
				((Behaviour)lvAnimator).enabled = true;
			}
			_isUpgradeable = true;
			return;
		}
		bool flag = false;
		if (_costType == 2)
		{
			flag = (double)GameDataManager.Instance.GetDiamond() >= _upgradeCost;
			if (flag)
			{
				if ((Object)(object)normalDiamondBtnSprite != (Object)null)
				{
					upgradeBtnImage.sprite = normalDiamondBtnSprite;
				}
			}
			else if ((Object)(object)disabledDiamondBtnSprite != (Object)null)
			{
				upgradeBtnImage.sprite = disabledDiamondBtnSprite;
			}
		}
		else if (_costType == 1)
		{
			flag = SingletonMono<UpgradeManager>.Instance.TotalGold >= _upgradeCost;
			if (flag)
			{
				if ((Object)(object)normalBtnSprite != (Object)null)
				{
					upgradeBtnImage.sprite = normalBtnSprite;
				}
			}
			else if ((Object)(object)disabledBtnSprite != (Object)null)
			{
				upgradeBtnImage.sprite = disabledBtnSprite;
			}
		}
		else if (_costType == 3)
		{
			if ((Object)(object)adBtnSprite != (Object)null)
			{
				upgradeBtnImage.sprite = adBtnSprite;
			}
			flag = true;
			((Component)upgradeMoneyTmp).gameObject.SetActive(false);
		}
		((Selectable)upgradeBtn).interactable = flag;
		if ((Object)(object)lvAnimator != (Object)null)
		{
			((Component)lvAnimator).gameObject.SetActive(flag);
			((Behaviour)lvAnimator).enabled = flag;
		}
		_isUpgradeable = flag;
	}

	private void OnUpgradeButtonClick()
	{
		int requestedWorldId = (((Object)(object)GameDataManager.Instance != (Object)null) ? GameDataManager.Instance.GetCurrentWorldId() : 0);
		int requestedItemId = _itemId;
		string requestedConfigType = _configType;
		if (_configType == "itemUpgrade1" && (_itemId == 1 || _itemId == 101) && _currentLevel == _helperAdCap && _helperAdCap < _maxLevel)
		{
			FakeVideoManager.Instance.ShowAd(delegate
			{
				if (IsAdUpgradeContextValid(requestedWorldId))
				{
					GameDataManager.Instance.UpgradeHelperADLevel();
					bool flag = SingletonMono<UpgradeManager>.Instance.ForceUpgradeLevel(requestedItemId, requestedConfigType);
					if (!((Object)(object)this == (Object)null) && !_isDisposed)
					{
						if (flag)
						{
							PlayUpgradeFeedback();
							this.onUpgradeButtonClick?.Invoke(this);
						}
						else
						{
							SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/cantbuy.mp3");
							RefreshUI();
						}
					}
				}
			}, delegate
			{
			}, "帮手升级广告");
		}
		else if (_costType == 3)
		{
			string reason = $"升级项广告{_itemId}";
			if (_itemId == 4 || _itemId == 5 || _itemId == 104 || _itemId == 105)
			{
				reason = "升级项中翻起银币金币";
			}
			else if (_itemId == 10 || _itemId == 11 || _itemId == 110 || _itemId == 111)
			{
				reason = "升级项中连环反应";
			}
			if (_configType == "itemUpgrade3" || _itemId == 107)
			{
				reason = "升级项中钻石商人，解锁";
			}
			FakeVideoManager.Instance.ShowAd(delegate
			{
				if (IsAdUpgradeContextValid(requestedWorldId))
				{
					bool flag = SingletonMono<UpgradeManager>.Instance.TryUpgrade(requestedItemId, requestedConfigType);
					if (!((Object)(object)this == (Object)null) && !_isDisposed)
					{
						if (flag)
						{
							PlayUpgradeFeedback();
							this.onUpgradeButtonClick?.Invoke(this);
						}
						else
						{
							SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/cantbuy.mp3");
							RefreshUI();
						}
					}
				}
			}, delegate
			{
			}, reason);
		}
		else if (!SingletonMono<UpgradeManager>.Instance.TryUpgrade(_itemId, _configType))
		{
			SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/cantbuy.mp3");
			RefreshUI();
		}
		else
		{
			this.onUpgradeButtonClick?.Invoke(this);
		}
	}

	private void OnEvolveButtonClick()
	{
		int requestedItemId = _itemId;
		int requestedTier = SingletonMono<UpgradeManager>.Instance?.GetEvolutionTier(requestedItemId) ?? (-1);
		SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/NewUI/Evolve/prefabs/EvolvePanel.prefab", delegate(GameObject prefab)
		{
			if ((Object)(object)prefab == (Object)null || (Object)(object)this == (Object)null || _isDisposed)
			{
				UpgradeManager instance = SingletonMono<UpgradeManager>.Instance;
				if (instance != null && instance.TriggerEvolutionAnimation(requestedItemId, requestedTier))
				{
					this.onUpgradeButtonClick?.Invoke(this);
				}
				else
				{
					SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/cantbuy.mp3");
					RefreshUI();
				}
			}
			else
			{
				Transform layerTransform = UIManager.Instance.GetLayerTransform(UILayer.Popup);
				EvolvePanel component = Object.Instantiate<GameObject>(prefab, layerTransform).GetComponent<EvolvePanel>();
				if ((Object)(object)component != (Object)null)
				{
					int objType = ((requestedItemId < 102 || requestedItemId > 105) ? 1 : 2);
					int type = ((requestedItemId >= 102 && requestedItemId <= 105) ? (requestedItemId - 101) : (requestedItemId - 1));
					component.Init(requestedItemId, objType, type);
					component.OnEvolveConfirmed += delegate
					{
						if (!((Object)(object)this == (Object)null) && !_isDisposed)
						{
							UpgradeManager instance3 = SingletonMono<UpgradeManager>.Instance;
							if (instance3 != null && instance3.TriggerEvolutionAnimation(requestedItemId, requestedTier))
							{
								this.onUpgradeButtonClick?.Invoke(this);
							}
							else
							{
								SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/cantbuy.mp3");
								RefreshUI();
							}
						}
					};
				}
				else
				{
					UpgradeManager instance2 = SingletonMono<UpgradeManager>.Instance;
					if (instance2 != null && instance2.TriggerEvolutionAnimation(requestedItemId, requestedTier))
					{
						this.onUpgradeButtonClick?.Invoke(this);
					}
					else
					{
						SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/cantbuy.mp3");
						RefreshUI();
					}
				}
			}
		});
	}

	private void OnDiamondUpgradeButtonClick()
	{
		bool num = SingletonMono<UpgradeManager>.Instance.TryUpgrade(_itemId, _configType);
		if (!num)
		{
			SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/cantbuy.mp3");
			RefreshUI();
		}
		if (num)
		{
			this.onUpgradeButtonClick?.Invoke(this);
		}
	}

	private void OnTypedUpgradeSuccess(int itemId, int newLevel, string configType)
	{
		if ((Object)(object)this == (Object)null || _isDisposed || itemId != _itemId || !(configType == _configType))
		{
			return;
		}
		RefreshData();
		RefreshUI();
		bool flag = _configType == "itemUpgrade1" && (Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null && UpgradeManager.IsEvolvableCurrencyUpgrade(_itemId) && SingletonMono<UpgradeManager>.Instance.CanEvolveCurrency(_itemId);
		if (((Component)this).gameObject.activeInHierarchy && !flag)
		{
			PlayUpgradeFeedback();
		}
		if (configType == "itemUpgrade3" && itemId == 106)
		{
			SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/NewUI/Ad/perfabs/AdDiamondMerchant.prefab", delegate(GameObject prefab)
			{
				if ((Object)(object)prefab != (Object)null)
				{
					Transform layerTransform = UIManager.Instance.GetLayerTransform(UILayer.Popup);
					AdDiamondMerchant component = Object.Instantiate<GameObject>(prefab, layerTransform).GetComponent<AdDiamondMerchant>();
					if ((Object)(object)component != (Object)null)
					{
						component.SetObjActive(isSupper: true);
					}
				}
			});
			DownItem obj = UIManager.Instance?.GetDownItem();
			obj?.ResetSupperSixTarget();
			obj?.CheckSuperSix();
		}
		if (!(configType == "itemUpgrade3") || itemId != 107)
		{
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/NewUI/Ad/perfabs/AdDiamondMerchant.prefab", delegate(GameObject prefab)
		{
			Transform layerTransform = UIManager.Instance.GetLayerTransform(UILayer.Popup);
			AdDiamondMerchant component = Object.Instantiate<GameObject>(prefab, layerTransform).GetComponent<AdDiamondMerchant>();
			if ((Object)(object)component != (Object)null)
			{
				component.SetObjActive(isSupper: false);
			}
		});
		(UIManager.Instance?.GetDownItem())?.StartShangRenLogic();
	}

	private static bool IsAdUpgradeContextValid(int requestedWorldId)
	{
		if (requestedWorldId > 0 && (Object)(object)GameDataManager.Instance != (Object)null && GameDataManager.Instance.GetCurrentWorldId() == requestedWorldId)
		{
			return (Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null;
		}
		return false;
	}

	private void PlayUpgradeFeedback()
	{
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Expected O, but got Unknown
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Expected O, but got Unknown
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Expected O, but got Unknown
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		_isPlayingUpgradeFeedback = true;
		string upgradeFeedbackDesc = GetUpgradeFeedbackDesc();
		if ((Object)(object)lvTipsTmp != (Object)null)
		{
			((TMP_Text)lvTipsTmp).text = upgradeFeedbackDesc;
		}
		if ((Object)(object)upgradeTips != (Object)null)
		{
			upgradeTips.SetActive(true);
			RectTransform component = upgradeTips.GetComponent<RectTransform>();
			if ((Object)(object)component != (Object)null)
			{
				ShortcutExtensions.DOKill((Component)(object)component, false);
				if (_tipsHideTween != null)
				{
					TweenExtensions.Kill(_tipsHideTween, false);
					_tipsHideTween = null;
				}
				component.anchoredPosition = _tipsInitAnchoredPos;
				TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(component.DOAnchorPosY(_tipsInitAnchoredPos.y + 30f, 0.3f), (Ease)6);
			}
		}
		if ((Object)(object)buttonUpgradeSpine != (Object)null && buttonUpgradeSpine.AnimationState != null)
		{
			((Component)buttonUpgradeSpine).gameObject.SetActive(true);
			AnimationState animationState = buttonUpgradeSpine.AnimationState;
			animationState.Complete -= new TrackEntryDelegate(OnUpgradeAnimComplete);
			animationState.Complete += new TrackEntryDelegate(OnUpgradeAnimComplete);
			animationState.SetAnimation(0, "upgrade", false);
		}
		_tipsHideTween = DOVirtual.DelayedCall(2f, (TweenCallback)delegate
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)upgradeTips != (Object)null)
			{
				RectTransform component2 = upgradeTips.GetComponent<RectTransform>();
				if ((Object)(object)component2 != (Object)null)
				{
					component2.anchoredPosition = _tipsInitAnchoredPos;
				}
				upgradeTips.SetActive(false);
			}
			_isPlayingUpgradeFeedback = false;
			_tipsHideTween = null;
		}, true);
	}

	private void OnUpgradeAnimComplete(TrackEntry trackEntry)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		if ((Object)(object)buttonUpgradeSpine != (Object)null)
		{
			((Component)buttonUpgradeSpine).gameObject.SetActive(false);
		}
		if ((Object)(object)buttonUpgradeSpine != (Object)null && buttonUpgradeSpine.AnimationState != null)
		{
			buttonUpgradeSpine.AnimationState.Complete -= new TrackEntryDelegate(OnUpgradeAnimComplete);
		}
	}

	private string GetItemDesc()
	{
		if (!(_configType == "itemUpgrade1"))
		{
			if (!(_configType == "itemUpgrade2"))
			{
				return ConfigManager.Instance.GetUpgrade3Config(_itemId)?.ItemDesc ?? "";
			}
			return ConfigManager.Instance.GetUpgrade2Config(_itemId)?.ItemDesc ?? "";
		}
		return ConfigManager.Instance.GetUpgrade1Config(_itemId)?.ItemDesc ?? "";
	}

	private string GetUpgradeFeedbackDesc()
	{
		if (_configType == "itemUpgrade1" && (Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null && UpgradeManager.IsEvolvableCurrencyUpgrade(_itemId) && SingletonMono<UpgradeManager>.Instance.IsEvolutionLevel(_itemId, _currentLevel))
		{
			SingletonMono<UpgradeManager>.Instance.GetEvolutionMergeCount(_itemId, _currentLevel);
			SingletonMono<UpgradeManager>.Instance.GetEvolutionIncomeMultiplier(_itemId);
			_ = _itemId;
			_ = 102;
			return "进化完成!";
		}
		return GetItemDesc();
	}

	public bool WillNextUpgradeEvolve()
	{
		if (_configType != "itemUpgrade1" || (Object)(object)SingletonMono<UpgradeManager>.Instance == (Object)null)
		{
			return false;
		}
		return SingletonMono<UpgradeManager>.Instance.WillNextUpgradeTriggerEvolution(_itemId);
	}

	private static string FormatEvolutionMult(double mult)
	{
		if (mult >= 100.0 || Math.Abs(mult - Math.Round(mult)) < 0.001)
		{
			return Math.Round(mult).ToString("0");
		}
		return mult.ToString("0.##");
	}

	private void OnGoldChanged(double newGold)
	{
		if (!((Object)(object)this == (Object)null) && !_isDisposed && _isUnlocked && (_currentLevel < _maxLevel || _maxLevel <= 0))
		{
			UpdateButtonState();
		}
	}

	private void OnCoinCountChanged()
	{
		if ((Object)(object)this == (Object)null || _isDisposed)
		{
			return;
		}
		bool isUnlocked = _isUnlocked;
		RefreshData();
		if (!isUnlocked && _isUnlocked)
		{
			RefreshUI();
		}
		else if (_configType == "itemUpgrade1")
		{
			itemUpgrade1 upgrade1Config = ConfigManager.Instance.GetUpgrade1Config(_itemId);
			int num = upgrade1Config?.UnLockCondition ?? 0;
			int num2 = upgrade1Config?.ConditionIconType ?? (-1);
			if (_itemId == 3 || _itemId == 103)
			{
				num += SingletonMono<UpgradeManager>.Instance?.GetInitialCopperCount() ?? 3;
			}
			if (num >= 0 && num2 > 0)
			{
				((TMP_Text)lockTmp).text = GetCoinConditionText(num, num2);
			}
		}
		else if (_configType == "itemUpgrade2")
		{
			itemUpgrade2 upgrade2Config = ConfigManager.Instance.GetUpgrade2Config(_itemId);
			int num = upgrade2Config?.UnLockCondition ?? 0;
			int num3 = upgrade2Config?.ConditionIconType ?? (-1);
			if (num >= 0 && num3 > 0)
			{
				((TMP_Text)lockTmp).text = GetCoinConditionText(num, num3);
			}
		}
		else
		{
			itemUpgrade3 upgrade3Config = ConfigManager.Instance.GetUpgrade3Config(_itemId);
			int num = upgrade3Config?.UnLockCondition ?? 0;
			int num4 = upgrade3Config?.ConditionIconType ?? (-1);
			if (num >= 0 && num4 > 0)
			{
				((TMP_Text)lockTmp).text = GetCoinConditionText(num, num4);
			}
		}
	}

	private string GetLockConditionText()
	{
		int num;
		if (_configType == "itemUpgrade1")
		{
			itemUpgrade1 upgrade1Config = ConfigManager.Instance.GetUpgrade1Config(_itemId);
			num = upgrade1Config?.UnLockCondition ?? 0;
			int num2 = upgrade1Config?.ConditionIconType ?? (-1);
			if (num >= 0 && num2 > 0)
			{
				if (_itemId == 3)
				{
					num += SingletonMono<UpgradeManager>.Instance?.GetInitialCopperCount() ?? 3;
				}
				else if (_itemId == 103)
				{
					num += SingletonMono<UpgradeManager>.Instance?.GetInitialDiceCount() ?? 3;
				}
				return GetCoinConditionText(num, num2);
			}
			if (num >= 0 && num2 == -1)
			{
				return "已解锁";
			}
		}
		else if (_configType == "itemUpgrade2")
		{
			itemUpgrade2 upgrade2Config = ConfigManager.Instance.GetUpgrade2Config(_itemId);
			num = upgrade2Config?.UnLockCondition ?? 0;
			int num3 = upgrade2Config?.ConditionIconType ?? (-1);
			if (num >= 0 && num3 > 0)
			{
				return GetCoinConditionText(num, num3);
			}
			if (num >= 0 && num3 == -1)
			{
				return "已解锁";
			}
		}
		else
		{
			itemUpgrade3 upgrade3Config = ConfigManager.Instance.GetUpgrade3Config(_itemId);
			num = upgrade3Config?.UnLockCondition ?? 0;
			int num4 = upgrade3Config?.ConditionIconType ?? (-1);
			if (num >= 0 && num4 > 0)
			{
				return GetCoinConditionText(num, num4);
			}
			if (num >= 0 && num4 == -1)
			{
				return "已解锁";
			}
		}
		return num switch
		{
			-1 => "已解锁", 
			-2 => "天赋点系统解锁", 
			_ => "未知条件", 
		};
	}

	private string GetCoinConditionText(int count, int conditionIconType = -1)
	{
		int num = -1;
		int currentWorldId = GameDataManager.Instance.GetCurrentWorldId();
		if (_configType == "itemUpgrade1")
		{
			switch (conditionIconType)
			{
			case 1:
				num = 0;
				break;
			case 2:
				num = 1;
				break;
			case 3:
				num = 2;
				break;
			case 4:
				num = 3;
				break;
			case -1:
				switch (_itemId)
				{
				case 3:
				case 103:
					num = 1;
					break;
				case 4:
				case 104:
					num = 1;
					break;
				}
				break;
			}
		}
		else
		{
			switch (conditionIconType)
			{
			case 1:
				num = 0;
				break;
			case 2:
				num = 1;
				break;
			case 3:
				num = 2;
				break;
			case 4:
				num = 3;
				break;
			case -1:
				switch (_itemId)
				{
				case 2:
				case 102:
					num = 1;
					break;
				case 3:
				case 103:
					num = 2;
					break;
				case 4:
				case 104:
					num = 1;
					break;
				case 5:
				case 105:
					num = 2;
					break;
				case 6:
				case 106:
					num = 0;
					break;
				case 7:
				case 107:
					num = 1;
					break;
				case 8:
				case 108:
					num = 2;
					break;
				case 10:
					num = 1;
					break;
				case 11:
					num = 2;
					break;
				case 14:
				case 110:
					num = 3;
					break;
				}
				break;
			}
		}
		if (currentWorldId == 2)
		{
			num += 100;
		}
		return num switch
		{
			0 => $"铜币等级达到Lv{count}", 
			1 => $"银币等级达到Lv{count}", 
			2 => $"金币等级达到Lv{count}", 
			3 => $"紫金币等级达到Lv{count}", 
			100 => $"铜骰子等级达到Lv{count}", 
			101 => $"银骰子等级达到Lv{count}", 
			102 => $"金骰子等级达到Lv{count}", 
			103 => $"紫金骰子等级达到Lv{count}", 
			_ => $"需要 {count} 个", 
		};
	}

	public void ForceRefresh()
	{
		RefreshData();
		RefreshUI();
	}
}

using System;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using cfg;

namespace Module.Facility;

public class UseDeskUI : MonoBehaviour
{
	[Header("UI元素")]
	[SerializeField]
	private Image deskIcon;

	[SerializeField]
	private TextMeshProUGUI deskNameText;

	[SerializeField]
	private Button useButton;

	[SerializeField]
	private Image useButtonText;

	[SerializeField]
	private Button buyButton;

	[SerializeField]
	private TextMeshProUGUI buyButtonText;

	[SerializeField]
	private Button moneyBuyButton;

	[SerializeField]
	private TextMeshProUGUI moneyBuyButtonText;

	[SerializeField]
	private TextMeshProUGUI levelText;

	[SerializeField]
	private TextMeshProUGUI skillUpgradeText;

	[SerializeField]
	private TextMeshProUGUI currentSkillText;

	[SerializeField]
	private TextMeshProUGUI nextSkillText;

	[SerializeField]
	private Image maxLevel;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private Button bgButton;

	private desk deskConfig;

	private bool isUsing;

	private bool isOwned;

	public Action<int> onUseChanged;

	public Action<bool> onBuyClicked;

	private DeskInfo deskInfo;

	private double upgradeCost
	{
		get
		{
			int deskLevel = GameDataManager.Instance.GetDeskLevel(deskConfig.Id);
			return deskConfig.BuyNum * (double)Mathf.Pow(deskConfig.BuyNumCostBuff, (float)(deskLevel - 1));
		}
	}

	private double upgradeCostDengCha
	{
		get
		{
			int deskLevel = GameDataManager.Instance.GetDeskLevel(deskConfig.Id);
			return deskConfig.BuyNum + (double)(deskConfig.BuyNumCostBuff * (float)(deskLevel - 1));
		}
	}

	private void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		((UnityEvent)bgButton.onClick).AddListener(new UnityAction(OnCloseClicked));
		((UnityEvent)closeButton.onClick).AddListener(new UnityAction(OnCloseClicked));
		Button obj = buyButton;
		if (obj != null)
		{
			((Component)obj).gameObject.SetActive(false);
		}
		Button obj2 = moneyBuyButton;
		if (obj2 != null)
		{
			((Component)obj2).gameObject.SetActive(false);
		}
		AddDisableListener();
	}

	private void AddDisableListener()
	{
		GameObjectClickListener gameObjectClickListener = GameObjectClickListener.Get(((Component)buyButton).gameObject);
		gameObjectClickListener.onClick = (GameObjectClickListener.VoidDelegate)Delegate.Combine(gameObjectClickListener.onClick, (GameObjectClickListener.VoidDelegate)delegate
		{
			if (!((Selectable)buyButton).interactable)
			{
				UIManager.Instance.ShowSupportUI(DialogType.money, 0.0);
			}
		});
		GameObjectClickListener gameObjectClickListener2 = GameObjectClickListener.Get(((Component)moneyBuyButton).gameObject);
		gameObjectClickListener2.onClick = (GameObjectClickListener.VoidDelegate)Delegate.Combine(gameObjectClickListener2.onClick, (GameObjectClickListener.VoidDelegate)delegate
		{
			if (!((Selectable)moneyBuyButton).interactable)
			{
				float commonFactor = ConfigManager.Instance.GetCommonFactor(25);
				UIManager.Instance.ShowSupportUI(DialogType.diamond, commonFactor);
			}
		});
	}

	public void Initialize(desk config)
	{
		deskConfig = config;
		deskInfo = GameDataManager.Instance.GetDeskInfo(deskConfig.Id);
		if (deskInfo == null)
		{
			Debug.LogError((object)$"DeskInfo not found for deskId: {deskConfig.Id}");
		}
		CheckUsingState();
		CheckOwnedState();
		UpdateUI();
	}

	private void CheckUsingState()
	{
		int currentDeskId = GameDataManager.Instance.GetCurrentDeskId();
		isUsing = currentDeskId == deskConfig.Id;
	}

	private void CheckOwnedState()
	{
		isOwned = GameDataManager.Instance.IsDeskOwned(deskConfig.Id);
	}

	private void UpdateUI()
	{
		//IL_0438: Unknown result type (might be due to invalid IL or missing references)
		//IL_0442: Expected O, but got Unknown
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Expected O, but got Unknown
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Expected O, but got Unknown
		//IL_03fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0408: Expected O, but got Unknown
		if ((Object)(object)deskNameText != (Object)null)
		{
			((TMP_Text)deskNameText).text = deskConfig.DeskName;
		}
		SingletonMono<ResManager>.Instance.LoadSpriteAsync(Constants.GetDeskIconPath(deskConfig.Id), delegate(Sprite sprite)
		{
			if ((Object)(object)sprite != (Object)null)
			{
				deskIcon.sprite = sprite;
			}
		});
		int deskLevel = GameDataManager.Instance.GetDeskLevel(deskConfig.Id);
		if ((Object)(object)levelText != (Object)null)
		{
			((TMP_Text)levelText).text = $"{deskLevel}";
		}
		float num = deskConfig.SkillNum * Mathf.Pow(deskConfig.SkillUpgradeBuff, (float)(deskLevel - 1));
		float num2 = deskConfig.SkillNum * Mathf.Pow(deskConfig.SkillUpgradeBuff, (float)deskLevel);
		if ((Object)(object)currentSkillText != (Object)null)
		{
			((TMP_Text)currentSkillText).text = $"{num:0.0}倍";
		}
		if ((Object)(object)nextSkillText != (Object)null)
		{
			((TMP_Text)nextSkillText).text = $"{num2:0.0}倍";
		}
		if ((Object)(object)skillUpgradeText != (Object)null)
		{
			((TMP_Text)skillUpgradeText).text = deskConfig.DeskDesc + $"*{num:0.0}倍";
		}
		if ((Object)(object)useButton != (Object)null)
		{
			if (isUsing)
			{
				Image obj = useButtonText;
				if (obj != null)
				{
					((Component)obj).gameObject.SetActive(true);
				}
				((Component)useButton).gameObject.SetActive(false);
				((Selectable)useButton).interactable = false;
			}
			else
			{
				Image obj2 = useButtonText;
				if (obj2 != null)
				{
					((Component)obj2).gameObject.SetActive(false);
				}
				((Component)useButton).gameObject.SetActive(true);
				((Selectable)useButton).interactable = isOwned;
			}
			((UnityEventBase)useButton.onClick).RemoveAllListeners();
			((UnityEvent)useButton.onClick).AddListener(new UnityAction(OnUseClicked));
		}
		int skillLevelMax = deskConfig.SkillLevelMax;
		double totalGold = SingletonMono<UpgradeManager>.Instance.TotalGold;
		long diamond = GameDataManager.Instance.GetDiamond();
		if (deskConfig.BuyType == 1)
		{
			((Component)buyButton).gameObject.SetActive(true);
			((Component)moneyBuyButton).gameObject.SetActive(false);
			string text = ((deskConfig.BuyType == 1) ? "" : "");
			if (deskLevel < skillLevelMax)
			{
				((TMP_Text)buyButtonText).text = text + " " + NumberFormatter.FormatNumber(upgradeCost);
				((Component)maxLevel).gameObject.SetActive(false);
			}
			else
			{
				((Component)maxLevel).gameObject.SetActive(true);
			}
			((UnityEventBase)buyButton.onClick).RemoveAllListeners();
			((Selectable)buyButton).interactable = deskLevel < skillLevelMax && totalGold >= upgradeCost;
			((UnityEvent)buyButton.onClick).AddListener(new UnityAction(OnUpgradeClicked));
		}
		if (deskConfig.BuyType == 2)
		{
			((Component)moneyBuyButton).gameObject.SetActive(true);
			((Component)buyButton).gameObject.SetActive(false);
			string text2 = ((deskConfig.BuyType == 1) ? "" : "");
			((UnityEventBase)moneyBuyButton.onClick).RemoveAllListeners();
			((Selectable)moneyBuyButton).interactable = deskLevel < skillLevelMax && (double)diamond >= upgradeCostDengCha;
			if (deskLevel < skillLevelMax)
			{
				((TMP_Text)moneyBuyButtonText).text = text2 + " " + NumberFormatter.FormatNumber(upgradeCostDengCha);
				((Component)maxLevel).gameObject.SetActive(false);
			}
			else
			{
				((TMP_Text)moneyBuyButtonText).text = "最大等级";
				((Component)maxLevel).gameObject.SetActive(true);
			}
			((UnityEvent)moneyBuyButton.onClick).AddListener(new UnityAction(OnUpgradeClicked));
		}
		if ((Object)(object)closeButton != (Object)null)
		{
			((UnityEventBase)closeButton.onClick).RemoveAllListeners();
			((UnityEvent)closeButton.onClick).AddListener(new UnityAction(OnCloseClicked));
		}
	}

	private void OnUseClicked()
	{
		if (!isUsing && isOwned)
		{
			GameDataManager.Instance.SetCurrentDeskId(deskConfig.Id);
			isUsing = true;
			UIManager.Instance.ActiveMainScene?.LoadBgImage(deskConfig.Id);
			UpdateUI();
			onUseChanged?.Invoke(deskConfig.Id);
		}
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
				GameObject effect = Object.Instantiate<GameObject>(prefab, ((Component)deskIcon).transform);
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

	private void OnUpgradeClicked()
	{
		bool flag = false;
		if ((deskConfig.BuyType != 1) ? GameDataManager.Instance.RemoveDiamond((int)upgradeCostDengCha) : SingletonMono<UpgradeManager>.Instance.TrySpendGold(upgradeCost))
		{
			SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/upgrade.mp3");
			GameDataManager.Instance.SetDeskLevel(deskConfig.Id, GameDataManager.Instance.GetDeskLevel(deskConfig.Id) + 1);
			SingletonMono<UpgradeManager>.Instance.AddDeskExtraBuff(deskConfig.Id);
			UpdateUI();
			PlayEffect();
			onBuyClicked?.Invoke(obj: true);
			FacilityRedDot.CheckAllRedDots();
		}
		else
		{
			SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/cantbuy.mp3");
			_ = deskConfig.BuyType;
			_ = 1;
		}
	}

	private void OnCloseClicked()
	{
		SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
		((Component)this).gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		if ((Object)(object)useButton != (Object)null)
		{
			((UnityEventBase)useButton.onClick).RemoveAllListeners();
		}
		if ((Object)(object)buyButton != (Object)null)
		{
			((UnityEventBase)buyButton.onClick).RemoveAllListeners();
		}
		if ((Object)(object)moneyBuyButton != (Object)null)
		{
			((UnityEventBase)moneyBuyButton.onClick).RemoveAllListeners();
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

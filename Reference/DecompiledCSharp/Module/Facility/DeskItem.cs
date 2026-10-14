using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using cfg;

namespace Module.Facility;

public class DeskItem : MonoBehaviour
{
	[Header("UI元素")]
	[SerializeField]
	private RectTransform usingImage;

	[SerializeField]
	private TextMeshProUGUI deskNameText;

	[SerializeField]
	private TextMeshProUGUI deskDescText;

	[SerializeField]
	private TextMeshProUGUI levelText;

	[SerializeField]
	private Button upgradeBtn;

	[SerializeField]
	private GameObject lockIcon;

	[SerializeField]
	private Image usingIcon;

	[SerializeField]
	private TextMeshProUGUI unlockConditionText;

	[SerializeField]
	private Button itemButton;

	[SerializeField]
	private GameObject reddotGo;

	private desk deskConfig;

	private int currentLevel;

	private bool isUnlocked;

	private bool isUsing;

	public Action<int> onUseChanged;

	public void Initialize(desk config)
	{
		deskConfig = config;
		UpdateUnlockedState();
		UpdateUsingState();
		UpdateLevel();
		UpdateUI();
	}

	private void UpdateUnlockedState()
	{
		if (deskConfig.DeskUnlockCondition == -1)
		{
			isUnlocked = true;
		}
		else
		{
			isUnlocked = CheckUnlockCondition(deskConfig.DeskUnlockCondition);
		}
		if (isUnlocked && !GameDataManager.Instance.IsDeskOwned(deskConfig.Id))
		{
			GameDataManager.Instance.SetDeskOwned(deskConfig.Id, isOwned: true);
		}
	}

	private bool CheckUnlockCondition(int condition)
	{
		if (GameDataManager.Instance.GetCurrentMainMissionLevel() < condition)
		{
			return condition == -1;
		}
		return true;
	}

	private void UpdateUsingState()
	{
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			isUsing = GameDataManager.Instance.GetCurrentDeskId() == deskConfig.Id;
		}
		else
		{
			isUsing = false;
		}
	}

	private void UpdateLevel()
	{
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			currentLevel = GameDataManager.Instance.GetDeskLevel(deskConfig.Id);
		}
		else
		{
			currentLevel = 1;
		}
	}

	private void UpdateUI()
	{
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Expected O, but got Unknown
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Expected O, but got Unknown
		if ((Object)(object)deskNameText != (Object)null)
		{
			((TMP_Text)deskNameText).text = deskConfig.DeskName;
		}
		float num = deskConfig.SkillNum * Mathf.Pow(deskConfig.SkillUpgradeBuff, (float)(currentLevel - 1));
		if ((Object)(object)deskDescText != (Object)null)
		{
			((TMP_Text)deskDescText).text = deskConfig.DeskDesc + $"*{num:0.0}倍";
		}
		if ((Object)(object)levelText != (Object)null)
		{
			((TMP_Text)levelText).text = $"{currentLevel}";
		}
		if ((Object)(object)lockIcon != (Object)null)
		{
			lockIcon.SetActive(!isUnlocked);
		}
		if ((Object)(object)usingImage != (Object)null)
		{
			((Component)usingImage).gameObject.SetActive(isUsing);
		}
		if ((Object)(object)unlockConditionText != (Object)null)
		{
			if (!isUnlocked)
			{
				((TMP_Text)unlockConditionText).text = $"解锁条件\n成长等级达到{deskConfig.DeskUnlockCondition}级";
			}
			else
			{
				((TMP_Text)unlockConditionText).text = string.Empty;
			}
		}
		if ((Object)(object)upgradeBtn != (Object)null)
		{
			((Component)upgradeBtn).gameObject.SetActive(isUnlocked);
			((UnityEventBase)upgradeBtn.onClick).RemoveAllListeners();
			((UnityEvent)upgradeBtn.onClick).AddListener(new UnityAction(OnUpgradeClicked));
		}
		if ((Object)(object)itemButton != (Object)null)
		{
			((UnityEventBase)itemButton.onClick).RemoveAllListeners();
			((UnityEvent)itemButton.onClick).AddListener(new UnityAction(OnItemClicked));
		}
		((Component)usingIcon).gameObject.SetActive(false);
		SingletonMono<ResManager>.Instance.LoadSpriteAsync(Constants.GetDeskIconPath(deskConfig.Id), delegate(Sprite sprite)
		{
			if ((Object)(object)sprite != (Object)null)
			{
				usingIcon.sprite = sprite;
				((Component)usingIcon).gameObject.SetActive(true);
				((Graphic)usingIcon).SetNativeSize();
			}
		});
		if ((Object)(object)reddotGo != (Object)null)
		{
			reddotGo.SetActive(isUnlocked && FacilityRedDot.IsDeskUpgradeable(deskConfig.Id));
		}
	}

	private void OnItemClicked()
	{
		if (isUnlocked)
		{
			SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
			UIManager.Instance.ShowUseDeskUI(deskConfig, onUseChanged, OnBuyClicked);
		}
	}

	public void OnUseChanged(int deskItemId)
	{
		isUsing = deskItemId == deskConfig.Id;
		UpdateUI();
	}

	private void OnBuyClicked(bool success)
	{
		UpdateUsingState();
		UpdateLevel();
		UpdateUI();
		FacilityRedDot.CheckAllRedDots();
	}

	private void OnUpgradeClicked()
	{
		if (currentLevel < deskConfig.SkillLevelMax)
		{
			bool flag = false;
			if (deskConfig.BuyType == 1)
			{
				double amount = deskConfig.BuyNum * (double)Mathf.Pow(deskConfig.BuyNumCostBuff, (float)(currentLevel - 1));
				flag = SingletonMono<UpgradeManager>.Instance.TrySpendGold(amount);
			}
			else if (deskConfig.BuyType == 2)
			{
				double num = deskConfig.BuyNum + (double)(deskConfig.BuyNumCostBuff * (float)(currentLevel - 1));
				flag = GameDataManager.Instance.RemoveDiamond((int)num);
			}
			if (flag)
			{
				SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
				currentLevel++;
				GameDataManager.Instance.SetDeskLevel(deskConfig.Id, currentLevel);
				SingletonMono<UpgradeManager>.Instance.AddDeskExtraBuff(deskConfig.Id);
				UpdateLevel();
				UpdateUI();
				FacilityRedDot.CheckAllRedDots();
			}
			else
			{
				SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/cantbuy.mp3");
			}
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)upgradeBtn != (Object)null)
		{
			((UnityEventBase)upgradeBtn.onClick).RemoveAllListeners();
		}
		if ((Object)(object)itemButton != (Object)null)
		{
			((UnityEventBase)itemButton.onClick).RemoveAllListeners();
		}
	}
}

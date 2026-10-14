using System;
using Module.Role;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using cfg;

namespace Module.Facility;

public class HelperItem : MonoBehaviour
{
	[Header("UI元素")]
	[SerializeField]
	private Image helperIcon;

	[SerializeField]
	private Image helperIcon2;

	[SerializeField]
	private TextMeshProUGUI helperNameText;

	[SerializeField]
	private TextMeshProUGUI helperLevelText;

	[SerializeField]
	private TextMeshProUGUI helperDescText;

	[SerializeField]
	private Button upgradeBtn1;

	[SerializeField]
	private GameObject redDot;

	[SerializeField]
	private Button minusBtn;

	[SerializeField]
	private Button plusBtn;

	[SerializeField]
	private TextMeshProUGUI countText;

	[SerializeField]
	private GameObject lockIcon;

	[SerializeField]
	private TextMeshProUGUI unlockConditionText;

	[SerializeField]
	private Button itemButton;

	[SerializeField]
	private Button adUnlockBtn;

	[SerializeField]
	private Button subscribeUnlockBtn;

	[Header("背景与稀有度")]
	[SerializeField]
	private Image bg1;

	[SerializeField]
	private Image bg2;

	[SerializeField]
	private Image rateIcon;

	[SerializeField]
	private Sprite[] bgSprites;

	[SerializeField]
	private Sprite[] rateIcons;

	[SerializeField]
	private Image thirstDayTip;

	[SerializeField]
	private Image sevenDayTip;

	[SerializeField]
	private Image halandeTip;

	private helperRole helperConfig;

	private HelperInfo helperInfo;

	private int currentLevel = 1;

	private int currentCount;

	private int maxCount = 88;

	private bool isUnlocked;

	public Action<int> onCountChanged;

	private const string GUIDE_STEP_PLUS_BTN = "guide_step_2_plus_btn";

	private bool isGuideing;

	public void Initialize(helperRole config, HelperInfo info)
	{
		helperConfig = config;
		helperInfo = info;
		UpdateUnlockedState();
		UpdateLevel();
		UpdateCount();
		UpdateMaxCount();
		UpdateUI();
		CheckGuide();
	}

	private void UpdateUnlockedState()
	{
		if (helperConfig.Id == 9)
		{
			isUnlocked = GameDataManager.Instance.IsHelperOwned(9);
			return;
		}
		if (GameDataManager.Instance.IsHelperOwned(helperConfig.Id))
		{
			isUnlocked = true;
			return;
		}
		if (RequiresSpecificUnlockWorld() && !CanUseCurrentWorldUnlockCondition())
		{
			isUnlocked = false;
			return;
		}
		if (helperConfig.HelperUnlockCondition == -1)
		{
			isUnlocked = true;
		}
		else
		{
			isUnlocked = CheckUnlockCondition(helperConfig.HelperUnlockCondition);
		}
		if (isUnlocked)
		{
			GameDataManager.Instance.SetHelperOwned(helperConfig.Id, isOwned: true);
		}
		else if (GameDataManager.Instance.IsHelperOwned(helperConfig.Id))
		{
			isUnlocked = true;
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

	private bool RequiresSpecificUnlockWorld()
	{
		if (helperConfig != null)
		{
			return helperConfig.UnlockLevel > 0;
		}
		return false;
	}

	private int GetRequiredUnlockWorldId()
	{
		if (!RequiresSpecificUnlockWorld())
		{
			return -1;
		}
		return helperConfig.UnlockLevel;
	}

	private bool CanUseCurrentWorldUnlockCondition()
	{
		int requiredUnlockWorldId = GetRequiredUnlockWorldId();
		if (requiredUnlockWorldId <= 0)
		{
			return true;
		}
		if (GameDataManager.Instance.GetCurrentWorldId() == requiredUnlockWorldId)
		{
			return true;
		}
		return IsRequiredWorldFirstUnlockReady();
	}

	private bool IsRequiredWorldFirstUnlockReady()
	{
		int requiredUnlockWorldId = GetRequiredUnlockWorldId();
		if (requiredUnlockWorldId <= 0)
		{
			return true;
		}
		if (GameDataManager.Instance.IsHelperOwnedInWorld(requiredUnlockWorldId, helperConfig.Id))
		{
			return true;
		}
		int helperUnlockCondition = helperConfig.HelperUnlockCondition;
		int worldMainMissionLevel = GameDataManager.Instance.GetWorldMainMissionLevel(requiredUnlockWorldId);
		if (helperUnlockCondition > 0)
		{
			return worldMainMissionLevel >= helperUnlockCondition;
		}
		return worldMainMissionLevel > 0;
	}

	private bool ShouldShowRequiredWorldUnlockText()
	{
		if (!RequiresSpecificUnlockWorld())
		{
			return false;
		}
		int requiredUnlockWorldId = GetRequiredUnlockWorldId();
		if (GameDataManager.Instance.GetCurrentWorldId() == requiredUnlockWorldId)
		{
			return false;
		}
		return !IsRequiredWorldFirstUnlockReady();
	}

	private string GetRequiredWorldName()
	{
		int requiredUnlockWorldId = GetRequiredUnlockWorldId();
		world world = (ConfigManager.HasInstance() ? ConfigManager.Instance.GetWorldConfig(requiredUnlockWorldId) : null);
		if (world == null || string.IsNullOrEmpty(world.WorldName))
		{
			return $"世界{requiredUnlockWorldId}";
		}
		return world.WorldName;
	}

	private string GetRequiredWorldUnlockConditionText()
	{
		string requiredWorldName = GetRequiredWorldName();
		if (helperConfig.HelperUnlockCondition <= 0)
		{
			return "<color=#FFC578>解锁条件:</color>\n进入" + requiredWorldName + "解锁";
		}
		return $"<color=#FFC578>解锁条件:</color>\n在{requiredWorldName}{helperConfig.HelperUnlockCondition}级解锁";
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

	private void UpdateMaxCount()
	{
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			if (GameDataManager.Instance.GetCurrentWorldId() == 2)
			{
				maxCount = GameDataManager.Instance.GetHelperMaxCount(helperConfig.Id) + (int)(TalentDataManager.Instance?.GetDiceTalentBuff(212) ?? 0f);
			}
			else
			{
				maxCount = GameDataManager.Instance.GetHelperMaxCount(helperConfig.Id);
			}
		}
		else
		{
			maxCount = 0;
		}
	}

	public void UpdateUI()
	{
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Expected O, but got Unknown
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Expected O, but got Unknown
		//IL_052f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0539: Expected O, but got Unknown
		//IL_0595: Unknown result type (might be due to invalid IL or missing references)
		//IL_059f: Expected O, but got Unknown
		//IL_05e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f0: Expected O, but got Unknown
		//IL_06a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_06aa: Expected O, but got Unknown
		if ((Object)(object)helperNameText != (Object)null)
		{
			((TMP_Text)helperNameText).text = helperConfig.HelperName ?? "";
		}
		if ((Object)(object)helperLevelText != (Object)null)
		{
			((TMP_Text)helperLevelText).text = $"Lv.{currentLevel}";
			((Component)helperLevelText).gameObject.SetActive(isUnlocked);
		}
		if ((Object)(object)helperDescText != (Object)null)
		{
			if (helperConfig.HelperDesc.IndexOf("%") != -1)
			{
				((TMP_Text)helperDescText).text = helperConfig.HelperDesc.ToLower().Replace("sss", (SingletonMono<UpgradeManager>.Instance.GetHelperExtraBuff(helperConfig.Id) * 100f).ToString("F1") ?? "");
			}
			else
			{
				((TMP_Text)helperDescText).text = helperConfig.HelperDesc.ToLower().Replace("sss", $"{SingletonMono<UpgradeManager>.Instance.GetHelperExtraBuff(helperConfig.Id)}");
			}
		}
		if ((Object)(object)countText != (Object)null)
		{
			((TMP_Text)countText).text = $"{currentCount}/{maxCount}";
		}
		_ = currentCount;
		bool flag = !isUnlocked && CanUseCurrentWorldUnlockCondition() && helperConfig.HelperAdUnlockCondition > 0 && GameDataManager.Instance.GetCurrentMainMissionLevel() >= helperConfig.HelperAdUnlockCondition;
		bool flag2 = flag && helperConfig.Id == 2;
		if ((Object)(object)adUnlockBtn != (Object)null)
		{
			((Component)adUnlockBtn).gameObject.SetActive(flag && !flag2);
			((UnityEventBase)adUnlockBtn.onClick).RemoveAllListeners();
			((UnityEvent)adUnlockBtn.onClick).AddListener(new UnityAction(OnAdUnlockClicked));
		}
		if ((Object)(object)subscribeUnlockBtn != (Object)null)
		{
			((Component)subscribeUnlockBtn).gameObject.SetActive(flag2);
			((UnityEventBase)subscribeUnlockBtn.onClick).RemoveAllListeners();
			((UnityEvent)subscribeUnlockBtn.onClick).AddListener(new UnityAction(OnSubscribeUnlockClicked));
		}
		if ((Object)(object)lockIcon != (Object)null)
		{
			lockIcon.SetActive(!isUnlocked);
		}
		if ((Object)(object)thirstDayTip != (Object)null && (Object)(object)sevenDayTip != (Object)null)
		{
			int worldId = ((!((Object)(object)GameDataManager.Instance != (Object)null)) ? 1 : GameDataManager.Instance.GetCurrentWorldId());
			signReward signRewardItemByWorldDay = ConfigManager.Instance.GetSignRewardItemByWorldDay(worldId, 3);
			signReward signRewardItemByWorldDay2 = ConfigManager.Instance.GetSignRewardItemByWorldDay(worldId, 7);
			int num = 0;
			int num2 = 0;
			if (signRewardItemByWorldDay != null && signRewardItemByWorldDay.AdReward != null && signRewardItemByWorldDay.AdReward.Count > 2)
			{
				num = GetHelperId(signRewardItemByWorldDay.AdReward[2]);
			}
			if (signRewardItemByWorldDay2 != null && signRewardItemByWorldDay2.AdReward != null && signRewardItemByWorldDay2.AdReward.Count > 2)
			{
				num2 = GetHelperId(signRewardItemByWorldDay2.AdReward[2]);
			}
			if (!isUnlocked && helperConfig.Id == num)
			{
				((Component)thirstDayTip).gameObject.SetActive(true);
			}
			else if (!isUnlocked && helperConfig.Id == num2)
			{
				((Component)sevenDayTip).gameObject.SetActive(true);
			}
			else
			{
				((Component)thirstDayTip).gameObject.SetActive(false);
				((Component)sevenDayTip).gameObject.SetActive(false);
			}
		}
		if ((Object)(object)unlockConditionText != (Object)null)
		{
			if (!isUnlocked)
			{
				if (helperConfig.Id == 9)
				{
					((TMP_Text)unlockConditionText).text = "<color=#FFC578>获取方式:</color>\n绿茵活动获取";
					if ((Object)(object)halandeTip != (Object)null)
					{
						((Component)halandeTip).gameObject.SetActive(true);
					}
				}
				else if (ShouldShowRequiredWorldUnlockText())
				{
					((TMP_Text)unlockConditionText).text = GetRequiredWorldUnlockConditionText();
					if ((Object)(object)halandeTip != (Object)null)
					{
						((Component)halandeTip).gameObject.SetActive(false);
					}
				}
				else
				{
					((TMP_Text)unlockConditionText).text = $"<color=#FFC578>解锁条件:</color>\n成长等级到{helperConfig.HelperUnlockCondition}级";
					if ((Object)(object)halandeTip != (Object)null)
					{
						((Component)halandeTip).gameObject.SetActive(false);
					}
				}
			}
			else
			{
				((TMP_Text)unlockConditionText).text = string.Empty;
				if ((Object)(object)halandeTip != (Object)null)
				{
					((Component)halandeTip).gameObject.SetActive(false);
				}
			}
		}
		if ((Object)(object)upgradeBtn1 != (Object)null)
		{
			((Component)upgradeBtn1).gameObject.SetActive(isUnlocked);
			((UnityEventBase)upgradeBtn1.onClick).RemoveAllListeners();
			((UnityEvent)upgradeBtn1.onClick).AddListener(new UnityAction(OnUpgradeBtn1Clicked));
		}
		int currentWorldHelperUpgradeId = Constants.GetCurrentWorldHelperUpgradeId();
		int totalHelperCount = SingletonMono<UpgradeManager>.Instance.GetUpgradeLevel(currentWorldHelperUpgradeId, "itemUpgrade1");
		int usedCount = GameDataManager.Instance.GetAllHelperCount();
		if ((Object)(object)minusBtn != (Object)null)
		{
			((UnityEventBase)minusBtn.onClick).RemoveAllListeners();
			((UnityEvent)minusBtn.onClick).AddListener(new UnityAction(OnMinusClicked));
			((Selectable)minusBtn).interactable = currentCount > 0;
		}
		if ((Object)(object)plusBtn != (Object)null)
		{
			((UnityEventBase)plusBtn.onClick).RemoveAllListeners();
			((UnityEvent)plusBtn.onClick).AddListener(new UnityAction(OnPlusClicked));
			((Selectable)plusBtn).interactable = totalHelperCount > usedCount && currentCount < maxCount;
			if (!((Selectable)plusBtn).interactable)
			{
				GameObjectClickListener gameObjectClickListener = GameObjectClickListener.Get(((Component)plusBtn).gameObject);
				gameObjectClickListener.onClick = (GameObjectClickListener.VoidDelegate)Delegate.Combine(gameObjectClickListener.onClick, (GameObjectClickListener.VoidDelegate)delegate
				{
					if (totalHelperCount <= usedCount)
					{
						BroadcastManager.ShowPopUpTip("没有可分配的帮手了");
					}
					else if (currentCount >= maxCount)
					{
						BroadcastManager.ShowPopUpTip("已达到" + helperConfig.HelperName + "的最大上限");
					}
				});
			}
			else
			{
				GameObjectClickListener.Get(((Component)plusBtn).gameObject).onClick = null;
			}
		}
		if ((Object)(object)itemButton != (Object)null)
		{
			((UnityEventBase)itemButton.onClick).RemoveAllListeners();
			((UnityEvent)itemButton.onClick).AddListener(new UnityAction(OnItemClicked));
		}
		int rare = helperConfig.Rare;
		int num3 = (rare - 1) * 2;
		if (bgSprites != null && bgSprites.Length >= num3 + 2)
		{
			if ((Object)(object)bg1 != (Object)null)
			{
				bg1.sprite = bgSprites[num3];
			}
			if ((Object)(object)bg2 != (Object)null)
			{
				bg2.sprite = bgSprites[num3 + 1];
			}
		}
		if (rateIcons != null && rateIcons.Length >= rare && rare >= 1 && (Object)(object)rateIcon != (Object)null)
		{
			rateIcon.sprite = rateIcons[rare - 1];
			((Component)rateIcon).gameObject.SetActive(true);
		}
		((Component)helperIcon).gameObject.SetActive(false);
		if ((Object)(object)helperIcon2 != (Object)null)
		{
			((Component)helperIcon2).gameObject.SetActive(false);
		}
		SingletonMono<ResManager>.Instance.LoadSpriteAsync(Constants.GetHelperIconPath(helperConfig.Id), delegate(Sprite sprite)
		{
			if ((Object)(object)sprite != (Object)null)
			{
				helperIcon.sprite = sprite;
				((Component)helperIcon).gameObject.SetActive(true);
				((Graphic)helperIcon).SetNativeSize();
				if ((Object)(object)helperIcon2 != (Object)null)
				{
					helperIcon2.sprite = sprite;
					((Component)helperIcon2).gameObject.SetActive(true);
					((Graphic)helperIcon2).SetNativeSize();
				}
			}
		});
		if ((Object)(object)redDot != (Object)null)
		{
			redDot.SetActive(isUnlocked && FacilityRedDot.IsHelperUpgradeable(helperConfig.Id));
		}
	}

	private void OnItemClicked()
	{
		if (isUnlocked)
		{
			SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
			UIManager.Instance.ShowUseHelperUI(helperConfig, OnUpgradeChanged, OnLimitChanged);
		}
	}

	private void OnUpgradeChanged(int helperId)
	{
		UpdateLevel();
		UpdateLevel();
		UpdateUI();
		FacilityRedDot.CheckAllRedDots();
	}

	private void OnLimitChanged(bool success)
	{
		UpdateMaxCount();
		UpdateLevel();
		UpdateUI();
		FacilityRedDot.CheckAllRedDots();
	}

	private void OnUpgradeBtn1Clicked()
	{
		UIManager.Instance.ShowUseHelperUI(helperConfig, OnUpgrade, OnLimitChanged);
	}

	private void OnUpgrade(int helperId)
	{
		UpdateLevel();
		UpdateUI();
		FacilityRedDot.CheckAllRedDots();
	}

	private void OnAdUnlockClicked()
	{
		SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
		FakeVideoManager.Instance.ShowAd(delegate
		{
			CompleteEarlyUnlock();
		}, delegate
		{
			BroadcastManager.ShowPopUpTip("视频播放失败");
		}, "帮手提前解锁");
	}

	private void OnSubscribeUnlockClicked()
	{
		SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
		TTFeedDirectPlayManager.Instance.ZhuDongRequestCapeHeroFeedSubscribe(delegate
		{
			CompleteEarlyUnlock();
		}, delegate
		{
			BroadcastManager.ShowPopUpTip("订阅失败");
		});
	}

	private void CompleteEarlyUnlock()
	{
		GameDataManager.Instance.SetHelperOwned(helperConfig.Id, isOwned: true, isADUnlock: true);
		isUnlocked = true;
		UpdateUnlockedState();
		UpdateLevel();
		UpdateCount();
		UpdateMaxCount();
		UpdateUI();
	}

	private void OnMinusClicked()
	{
		if (currentCount > 0)
		{
			currentCount--;
			if (currentCount == 0 && helperConfig.Id == 7)
			{
				SingletonMono<BuffManager>.Instance.RemoveBuff(BuffType.CriticalRate, 0.5f);
			}
			GameDataManager.Instance.SetHelperCount(helperConfig.Id, currentCount);
			SingletonMono<RoleManager>.Instance.ChangeRole(helperConfig.Id, 0);
			UpdateUI();
			onCountChanged?.Invoke(-1);
		}
	}

	private void OnPlusClicked()
	{
		int currentWorldHelperUpgradeId = Constants.GetCurrentWorldHelperUpgradeId();
		int upgradeLevel = SingletonMono<UpgradeManager>.Instance.GetUpgradeLevel(currentWorldHelperUpgradeId);
		int allHelperCount = GameDataManager.Instance.GetAllHelperCount();
		if (currentCount < maxCount && upgradeLevel - allHelperCount > 0)
		{
			SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
			currentCount++;
			if (currentCount == 1 && helperConfig.Id == 7)
			{
				SingletonMono<BuffManager>.Instance.AddBuff(BuffType.CriticalRate, 0.5f, 2.1474836E+09f);
			}
			GameDataManager.Instance.SetHelperCount(helperConfig.Id, currentCount);
			SingletonMono<RoleManager>.Instance.ChangeRole(0, helperConfig.Id);
			UpdateUI();
			onCountChanged?.Invoke(1);
			if (isGuideing)
			{
				UIManager.Instance.upgradePanel?.GuideClick(null);
				isGuideing = false;
				PlayerPrefs.SetInt("guide_step_2_plus_btn", 1);
			}
		}
	}

	private void CheckGuide()
	{
		if (helperConfig.Id == 1)
		{
			if (GameDataManager.Instance.GetHelperCount(helperConfig.Id) > 0)
			{
				isGuideing = false;
				PlayerPrefs.SetInt("guide_step_2_plus_btn", 1);
			}
			else if (PlayerPrefs.GetInt("guide_step_2_plus_btn", 0) == 0)
			{
				UIManager.Instance.upgradePanel?.GuideClick(((Component)plusBtn).transform);
				isGuideing = true;
			}
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)upgradeBtn1 != (Object)null)
		{
			((UnityEventBase)upgradeBtn1.onClick).RemoveAllListeners();
		}
		if ((Object)(object)minusBtn != (Object)null)
		{
			((UnityEventBase)minusBtn.onClick).RemoveAllListeners();
		}
		if ((Object)(object)plusBtn != (Object)null)
		{
			((UnityEventBase)plusBtn.onClick).RemoveAllListeners();
		}
		if ((Object)(object)itemButton != (Object)null)
		{
			((UnityEventBase)itemButton.onClick).RemoveAllListeners();
		}
		if ((Object)(object)adUnlockBtn != (Object)null)
		{
			((UnityEventBase)adUnlockBtn.onClick).RemoveAllListeners();
		}
		if ((Object)(object)subscribeUnlockBtn != (Object)null)
		{
			((UnityEventBase)subscribeUnlockBtn.onClick).RemoveAllListeners();
		}
	}

	public int GetHelperId(int suipianId)
	{
		return suipianId switch
		{
			101 => 1, 
			102 => 2, 
			103 => 3, 
			104 => 4, 
			105 => 5, 
			106 => 6, 
			107 => 7, 
			108 => 8, 
			109 => 9, 
			110 => 10, 
			_ => 1, 
		};
	}
}

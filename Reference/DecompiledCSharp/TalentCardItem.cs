using System.Collections;
using System.Collections.Generic;
using Module.Role;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using cfg;

public class TalentCardItem : MonoBehaviour
{
	[Header("天赋种类")]
	[SerializeField]
	public TalentType talentType;

	[Header("天赋id")]
	[SerializeField]
	public int id;

	[Header("天赋名称")]
	[SerializeField]
	public string talentName;

	[Header("天赋技能介绍")]
	[SerializeField]
	public string talentDesc;

	[Header("天赋初始消耗天赋点")]
	[SerializeField]
	public int baseTalentPoint;

	[Header("天赋消耗增加系数")]
	[SerializeField]
	public int costPointAdd;

	[Header("天赋此次升级需要消耗天赋点")]
	private int costPointCurrent;

	[Header("天赋升级效果")]
	[SerializeField]
	public List<float> upgradeResult;

	[Header("天赋物体目前等级")]
	[SerializeField]
	public int upgradeLevelCurrent;

	[Header("天赋最大升级次数")]
	[SerializeField]
	public int upgradeLevelMax;

	[Header("天赋是否解锁")]
	[SerializeField]
	public bool isUnlock;

	[Header("天赋物体已升级icon")]
	[SerializeField]
	private Transform talentBgOnImg;

	[Header("天赋物体未升级icon")]
	[SerializeField]
	private Transform talentBgOffImg;

	[Header("天赋物体未解锁icon")]
	[SerializeField]
	private Transform talentBgLock;

	[Header("天赋物体icon")]
	[SerializeField]
	private Image talentSkillIcon;

	[Header("天赋物体等级信息")]
	[SerializeField]
	private TextMeshProUGUI levelText;

	[Header("天赋解锁前置条件")]
	[SerializeField]
	public int UnlockId;

	[Header("天赋解锁后相关连线显示索引")]
	[SerializeField]
	public List<int> connectIndex = new List<int>();

	[Header("天赋点击按键")]
	[SerializeField]
	public Button btnClick;

	[Header("天赋升级红点提示")]
	[SerializeField]
	private Transform redDotImg;

	private bool IsCoinScene
	{
		get
		{
			if (TalentDataManager.Instance != null)
			{
				return TalentDataManager.Instance.IsCoinScene;
			}
			return false;
		}
	}

	private TalentFramework GetTalentFramework(TalentType type)
	{
		TalentManager talentManager = TalentDataManager.Instance?.GetCurrentTalentManager();
		if ((Object)(object)talentManager == (Object)null)
		{
			return null;
		}
		return type switch
		{
			TalentType.talentHelper => talentManager.talentPanels[1], 
			TalentType.talentSpecial => talentManager.talentPanels[2], 
			_ => talentManager.talentPanels[0], 
		};
	}

	private int GetTalentId(object configItem)
	{
		if (configItem is talent talent)
		{
			return talent.Id;
		}
		if (configItem is talent2 talent2)
		{
			return talent2.Id;
		}
		return 0;
	}

	private void Awake()
	{
		InitPrefab();
	}

	private void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		((UnityEvent)btnClick.onClick).AddListener((UnityAction)delegate
		{
			SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
			BtnClick();
		});
	}

	private void InitPrefab()
	{
		talentBgOnImg = ((Component)this).transform.Find("talentBgOnImg");
		talentBgOffImg = ((Component)this).transform.Find("talentBgOffImg ");
		talentBgLock = ((Component)this).transform.Find("talentBgLock");
		talentSkillIcon = ((Component)((Component)this).transform.Find("talentSkillIcon")).GetComponent<Image>();
		levelText = ((Component)((Component)this).transform.Find("levelText")).GetComponent<TextMeshProUGUI>();
		btnClick = ((Component)((Component)this).transform.Find("btnClick")).GetComponent<Button>();
		redDotImg = ((Component)this).transform.Find("redDotImg");
	}

	public void SetItemInfo(TalentType type, int itemId, bool isUnlock)
	{
		id = itemId;
		IList list;
		if (!IsCoinScene)
		{
			IList allDiceTalentItem = ConfigManager.Instance.GetAllDiceTalentItem();
			list = allDiceTalentItem;
		}
		else
		{
			IList allDiceTalentItem = ConfigManager.Instance.GetAllCoinTalentItem();
			list = allDiceTalentItem;
		}
		IList list2 = list;
		if (list2 == null)
		{
			return;
		}
		for (int i = 0; i < list2.Count; i++)
		{
			if (GetTalentId(list2[i]) == itemId)
			{
				talentType = type;
				if (IsCoinScene)
				{
					talent talent = list2[i] as talent;
					talentName = talent.TalentName;
					talentDesc = talent.TalentDesc;
					baseTalentPoint = talent.BaseTalentPoint;
					costPointAdd = talent.CostPointAdd;
					upgradeResult = talent.UpgradeResult;
					upgradeLevelMax = talent.UpgradeLevelMax;
				}
				else
				{
					talent2 talent2 = list2[i] as talent2;
					talentName = talent2.TalentName;
					talentDesc = talent2.TalentDesc;
					baseTalentPoint = talent2.BaseTalentPoint;
					costPointAdd = talent2.CostPointAdd;
					upgradeResult = talent2.UpgradeResult;
					upgradeLevelMax = talent2.UpgradeLevelMax;
				}
				this.isUnlock = isUnlock;
				TalentManager talentManager = TalentDataManager.Instance?.GetCurrentTalentManager();
				if ((Object)(object)talentManager != (Object)null)
				{
					talentSkillIcon.sprite = talentManager.GetSkillIcon(i);
				}
				((Graphic)talentSkillIcon).SetNativeSize();
				switch (((Component)this).transform.GetSiblingIndex())
				{
				case 0:
					UnlockId = -1;
					connectIndex.Add(0);
					connectIndex.Add(1);
					break;
				case 1:
					UnlockId = 1;
					connectIndex.Add(2);
					break;
				case 2:
					UnlockId = 1;
					connectIndex.Add(3);
					break;
				case 3:
					UnlockId = 2;
					connectIndex.Add(4);
					connectIndex.Add(5);
					break;
				case 4:
					UnlockId = 3;
					connectIndex.Add(6);
					connectIndex.Add(7);
					break;
				case 5:
					UnlockId = 4;
					connectIndex.Add(8);
					break;
				case 6:
					UnlockId = 4;
					connectIndex.Add(9);
					break;
				case 7:
					UnlockId = 5;
					connectIndex.Add(10);
					break;
				case 8:
					UnlockId = 5;
					connectIndex.Add(11);
					break;
				case 9:
					UnlockId = 6;
					break;
				case 10:
					UnlockId = 7;
					break;
				case 11:
					UnlockId = 8;
					break;
				case 12:
					UnlockId = 9;
					break;
				}
				RefeshItem(talentType);
				break;
			}
		}
	}

	public void RefeshItem(TalentType type)
	{
		if (!isUnlock)
		{
			if (UnlockId < 0)
			{
				isUnlock = true;
			}
			else
			{
				TalentFramework talentFramework = GetTalentFramework(type);
				if ((Object)(object)talentFramework != (Object)null)
				{
					TalentManager talentManager = TalentDataManager.Instance?.GetCurrentTalentManager();
					if ((Object)(object)talentManager == (Object)null)
					{
						return;
					}
					IList currentTalentConfig = talentManager.GetCurrentTalentConfig(talentType);
					Dictionary<int, TalentCardItem> talentItems = talentFramework.TalentItems;
					if (UnlockId - 1 < currentTalentConfig.Count && talentItems.ContainsKey(GetTalentId(currentTalentConfig[UnlockId - 1])))
					{
						int talentId = GetTalentId(currentTalentConfig[UnlockId - 1]);
						if (talentItems[talentId].isUnlock && talentItems[talentId].upgradeLevelCurrent > 0)
						{
							List<bool> list = (IsCoinScene ? GameDataManager.Instance.GetTalentUnlock() : GameDataManager.Instance.GetTalentData()?.diceTalentUnlock);
							if (list == null)
							{
								return;
							}
							isUnlock = true;
							_ = ((Component)this).transform.parent.childCount;
							if (list.Count >= id)
							{
								list[id - 1] = true;
							}
							GameDataManager.Instance.SaveTalentDataOnly();
						}
						else
						{
							isUnlock = false;
						}
					}
				}
			}
		}
		((Component)talentBgOnImg).gameObject.SetActive(false);
		((Component)talentBgOffImg).gameObject.SetActive(false);
		((Component)talentBgLock).gameObject.SetActive(false);
		((Component)talentSkillIcon).gameObject.SetActive(isUnlock);
		((Component)levelText).gameObject.SetActive(isUnlock);
		if (isUnlock)
		{
			if (upgradeLevelCurrent <= 0)
			{
				((Component)talentBgOffImg).gameObject.SetActive(true);
			}
			else if (upgradeLevelCurrent > 0)
			{
				((Component)talentBgOnImg).gameObject.SetActive(true);
			}
		}
		else
		{
			((Component)talentBgLock).gameObject.SetActive(true);
		}
		((TMP_Text)levelText).text = $"{upgradeLevelCurrent}/{upgradeLevelMax}";
	}

	public int GetCostPoint()
	{
		costPointCurrent = baseTalentPoint + upgradeLevelCurrent * costPointAdd;
		return costPointCurrent;
	}

	public void TalentUpgrade()
	{
		upgradeLevelCurrent++;
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			List<int> list = (IsCoinScene ? GameDataManager.Instance.GetTalentLevel() : GameDataManager.Instance.GetTalentData()?.diceTalentLevel);
			IList list2;
			if (!IsCoinScene)
			{
				IList allDiceTalentItem = ConfigManager.Instance.GetAllDiceTalentItem();
				list2 = allDiceTalentItem;
			}
			else
			{
				IList allDiceTalentItem = ConfigManager.Instance.GetAllCoinTalentItem();
				list2 = allDiceTalentItem;
			}
			IList list3 = list2;
			if (list != null && list3 != null)
			{
				for (int i = 0; i < list3.Count; i++)
				{
					if (id == GetTalentId(list3[i]))
					{
						if (list.Count > i)
						{
							list[i]++;
						}
						GameDataManager.Instance.SaveTalentDataOnly();
						break;
					}
				}
			}
		}
		if (!IsCoinScene)
		{
			return;
		}
		switch (id)
		{
		case 102:
			if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
			{
				SingletonMono<UpgradeManager>.Instance.CheckTalentUnlock(5);
			}
			if ((Object)(object)UpgradePanel.Instance != (Object)null)
			{
				UpgradePanel.Instance.RefreshUpgradeItemsForPanel(1);
			}
			break;
		case 104:
		{
			List<talent> allCoinTalentItem2 = ConfigManager.Instance.GetAllCoinTalentItem();
			for (int k = 0; k < allCoinTalentItem2.Count; k++)
			{
				if (allCoinTalentItem2[k].Id == 104 && upgradeLevelCurrent > 0)
				{
					float soundSleep = allCoinTalentItem2[k].UpgradeResult[upgradeLevelCurrent - 1];
					GameDataManager.Instance.SetSoundSleep(soundSleep);
					break;
				}
			}
			break;
		}
		case 105:
		{
			List<talent> allCoinTalentItem3 = ConfigManager.Instance.GetAllCoinTalentItem();
			for (int l = 0; l < allCoinTalentItem3.Count; l++)
			{
				if (allCoinTalentItem3[l].Id == 105 && upgradeLevelCurrent > 0)
				{
					float goldAge = allCoinTalentItem3[l].UpgradeResult[upgradeLevelCurrent - 1];
					GameDataManager.Instance.SetGoldAge(goldAge);
					break;
				}
			}
			break;
		}
		case 201:
			if ((Object)(object)SingletonMono<RoleManager>.Instance != (Object)null)
			{
				SingletonMono<RoleManager>.Instance.RefreshAllRoleSleepProbability();
			}
			break;
		case 206:
		{
			List<talent> allCoinTalentItem = ConfigManager.Instance.GetAllCoinTalentItem();
			for (int j = 0; j < allCoinTalentItem.Count; j++)
			{
				if (allCoinTalentItem[j].Id == 206 && upgradeLevelCurrent > 0)
				{
					float laoDongLi = allCoinTalentItem[j].UpgradeResult[upgradeLevelCurrent - 1];
					GameDataManager.Instance.SetLaoDongLi(laoDongLi);
					break;
				}
			}
			break;
		}
		case 207:
			if ((Object)(object)UpgradePanel.Instance != (Object)null)
			{
				UpgradePanel.Instance.RefreshUpgradeItemsForPanel(1);
			}
			break;
		case 301:
			if ((Object)(object)UpgradePanel.Instance != (Object)null)
			{
				UpgradePanel.Instance.RefreshUpgradeItemsForPanel(2);
			}
			break;
		case 302:
			if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
			{
				SingletonMono<UpgradeManager>.Instance.CheckTalentUnlock(12);
			}
			if ((Object)(object)UpgradePanel.Instance != (Object)null)
			{
				UpgradePanel.Instance.RefreshUpgradeItemsForPanel(2);
			}
			break;
		}
	}

	public void SetRedDotState(bool isShow)
	{
		((Component)redDotImg).gameObject.SetActive(isShow);
	}

	public void SetConnectState()
	{
		Transform val = GetTalentFramework(talentType)?.Getprocessparent();
		if (isUnlock && upgradeLevelCurrent > 0 && connectIndex.Count > 0)
		{
			for (int i = 0; i < connectIndex.Count; i++)
			{
				((Component)val.GetChild(connectIndex[i]).GetChild(1)).gameObject.SetActive(true);
			}
		}
	}

	public void BtnClick()
	{
		if (!isUnlock)
		{
			return;
		}
		TalentFramework talentFramework = GetTalentFramework(talentType);
		if ((Object)(object)talentFramework != (Object)null)
		{
			if (upgradeLevelCurrent < upgradeLevelMax)
			{
				talentFramework.OpenSkillTip(id);
				talentFramework.HideMaxLevelTip();
			}
			else
			{
				talentFramework.OpenSkillTip(id);
				talentFramework.ShowMaxLevelTip();
			}
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using cfg;

public class TalentFramework : MonoBehaviour
{
	[Header("天赋种类")]
	[SerializeField]
	public TalentType talentType;

	[Header("天赋界面物体")]
	[SerializeField]
	private GameObject talent;

	[Header("所有天赋的父节点物体")]
	[SerializeField]
	private Transform talentParent;

	[Header("天赋升级提示物体")]
	[SerializeField]
	public Transform SkillTip;

	[Header("天赋升级物体关闭按键")]
	[SerializeField]
	private Button backBtn;

	[Header("天赋提示面板标题")]
	[SerializeField]
	private TextMeshProUGUI skillTipTitle;

	[Header("天赋升级消耗天赋点字体提示")]
	[SerializeField]
	private TextMeshProUGUI skillTipCost;

	[Header("天赋升级介绍字体提示")]
	[SerializeField]
	private TextMeshProUGUI skillTipDesc;

	[Header("天赋升级介绍字体提示")]
	[SerializeField]
	public TextMeshProUGUI pointDisplay;

	[Header("天赋等级满了之后的提示字体提示")]
	[SerializeField]
	public TextMeshProUGUI maxLevelTip;

	[Header("天赋升级按键")]
	[SerializeField]
	private Button talentUpBtn;

	[Header("天赋重置按键")]
	[SerializeField]
	private Button chongzhiBtn;

	[Header("重置面板父物体")]
	[SerializeField]
	private Transform chongzhiPanel;

	[Header("重置面板物体加载路径")]
	[SerializeField]
	public string resetPanelPath;

	[Header("技能提示背景物体按键")]
	[SerializeField]
	private Button skillTipOffBg;

	[Header("技能提示框中的显示技能图标")]
	[SerializeField]
	private Image talentSkillIcon;

	[Header("天赋连线父物体")]
	[SerializeField]
	private Transform processparent;

	[Header("天赋重生按键")]
	[SerializeField]
	public Button rebornBtn;

	[HideInInspector]
	public GameObject talentResetTipsItem;

	private TalentResetTip talentResetTip;

	[Header("目前点击升级的天赋id")]
	private int clickItemId;

	private bool isDisplayRepoint;

	private bool isInitialized;

	public Dictionary<int, TalentCardItem> talentItems = new Dictionary<int, TalentCardItem>();

	public Dictionary<int, TalentCardItem> TalentItems => talentItems;

	protected TalentManager CurrentTalentManager
	{
		get
		{
			if (TalentDataManager.Instance == null)
			{
				return null;
			}
			return TalentDataManager.Instance.GetCurrentTalentManager();
		}
	}

	protected bool IsCoinScene
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

	protected int GetTalentId(object configItem)
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

	public virtual void Start()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Expected O, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		CreatTalentCard(talentType);
		if ((Object)(object)talentResetTipsItem == (Object)null)
		{
			SingletonMono<ResManager>.Instance.LoadPrefabAsync(resetPanelPath, delegate(GameObject prefab)
			{
				talentResetTipsItem = Object.Instantiate<GameObject>(prefab, chongzhiPanel);
				talentResetTipsItem.SetActive(false);
				talentResetTip = talentResetTipsItem.GetComponent<TalentResetTip>();
			});
		}
		((UnityEvent)chongzhiBtn.onClick).AddListener((UnityAction)delegate
		{
			SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
			if ((Object)(object)talentResetTip != (Object)null)
			{
				talentResetTip.RefreshTalentResetTip(talentType);
			}
			talentResetTipsItem.SetActive(true);
		});
		((UnityEvent)backBtn.onClick).AddListener((UnityAction)delegate
		{
			SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
			((Component)SkillTip).gameObject.SetActive(false);
		});
		((UnityEvent)talentUpBtn.onClick).AddListener((UnityAction)delegate
		{
			if (talentItems.ContainsKey(clickItemId) && talentItems[clickItemId].upgradeLevelCurrent < talentItems[clickItemId].upgradeLevelMax)
			{
				int costPoint = talentItems[clickItemId].GetCostPoint();
				if ((Object)(object)GameDataManager.Instance != (Object)null)
				{
					TalentSceneType sceneType = ((!IsCoinScene) ? TalentSceneType.DiceSceneTalent : TalentSceneType.CoinSceneTalent);
					if (GameDataManager.Instance.GetTalentPoint(sceneType) >= costPoint)
					{
						GameDataManager.Instance.ChangeTalentPoint(isAdd: false, costPoint, sceneType);
						int costPoint2 = GameDataManager.Instance.GetCostPoint(talentType, sceneType);
						costPoint2 += costPoint;
						GameDataManager.Instance.ResetCostPoint(talentType, costPoint2, sceneType);
						PlayUpgradeEffect();
						talentItems[clickItemId].TalentUpgrade();
						RefreshTalentPanel();
						if ((Object)(object)talentItems[clickItemId] != (Object)null)
						{
							talentItems[clickItemId].BtnClick();
						}
						SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/upgrade.mp3");
						PlayUpgradeEffect();
					}
					else
					{
						SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/cantbuy.mp3");
					}
				}
			}
		});
		((UnityEvent)skillTipOffBg.onClick).AddListener((UnityAction)delegate
		{
			SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
			((Component)SkillTip).gameObject.SetActive(false);
		});
		((UnityEvent)rebornBtn.onClick).AddListener((UnityAction)delegate
		{
			if ((Object)(object)CurrentTalentManager != (Object)null)
			{
				TalentCoin talentCoin = CurrentTalentManager.talentPanels[0] as TalentCoin;
				if ((Object)(object)talentCoin != (Object)null)
				{
					talentCoin.RebornInvok();
				}
				CurrentTalentManager.OffFingerGuide();
			}
		});
		GameObjectClickListener gameObjectClickListener = GameObjectClickListener.Get(((Component)talentUpBtn).gameObject);
		gameObjectClickListener.onClick = (GameObjectClickListener.VoidDelegate)Delegate.Combine(gameObjectClickListener.onClick, (GameObjectClickListener.VoidDelegate)delegate
		{
			if (!((Selectable)talentUpBtn).interactable)
			{
				BroadcastManager.ShowPopUpTip("金钱未达到升级目标");
			}
		});
	}

	public void InitTalentPanel()
	{
		if ((Object)(object)talentParent != (Object)null && (Object)(object)GameDataManager.Instance != (Object)null && TalentDataManager.Instance != null && (Object)(object)CurrentTalentManager != (Object)null)
		{
			IList currentTalentConfig = CurrentTalentManager.GetCurrentTalentConfig(talentType);
			if (currentTalentConfig == null)
			{
				return;
			}
			List<int> list;
			List<bool> list2;
			IList list3;
			if (IsCoinScene)
			{
				list = GameDataManager.Instance.GetTalentLevel();
				list2 = GameDataManager.Instance.GetTalentUnlock();
				list3 = ConfigManager.Instance.GetAllCoinTalentItem();
			}
			else
			{
				TalentData talentData = GameDataManager.Instance.GetTalentData();
				list = talentData?.diceTalentLevel;
				list2 = talentData?.diceTalentUnlock;
				list3 = ConfigManager.Instance.GetAllDiceTalentItem();
			}
			if (list == null || list2 == null || list3 == null)
			{
				return;
			}
			for (int i = 0; i < talentParent.childCount && i < currentTalentConfig.Count; i++)
			{
				TalentCardItem component = ((Component)talentParent.GetChild(i)).GetComponent<TalentCardItem>();
				component.id = (IsCoinScene ? ((talent)currentTalentConfig[i]).Id : ((talent2)currentTalentConfig[i]).Id);
				for (int j = 0; j < list3.Count; j++)
				{
					if ((IsCoinScene ? ((talent)list3[j]).Id : ((talent2)list3[j]).Id) != component.id)
					{
						continue;
					}
					if (list.Count > j)
					{
						component.upgradeLevelCurrent = list[j];
					}
					else
					{
						while (list.Count <= j)
						{
							list.Add(0);
						}
					}
					if (list2.Count > j)
					{
						component.isUnlock = list2[j];
						break;
					}
					while (list2.Count <= j)
					{
						list2.Add(item: false);
					}
					if (IsCoinScene && (component.id == 1 || component.id == 7 || component.id == 14))
					{
						component.isUnlock = true;
						list2[j] = true;
					}
					break;
				}
				component.SetItemInfo(talentType, component.id, component.isUnlock);
				talentItems.Add(component.id, component);
			}
		}
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			GameDataManager.Instance.SaveTalentDataOnly();
		}
		isInitialized = true;
		RefreshTalentPanel();
	}

	public void RefreshTalentPanel()
	{
		if (!isInitialized || TalentDataManager.Instance == null || (Object)(object)CurrentTalentManager == (Object)null)
		{
			return;
		}
		talent.SetActive(true);
		IList currentTalentConfig = CurrentTalentManager.GetCurrentTalentConfig(talentType);
		if (currentTalentConfig == null || talentItems == null)
		{
			Debug.LogError((object)$"[TalentFramework] RefreshTalentPanel 数据未就绪: talents={currentTalentConfig}, talentItems={talentItems}");
			return;
		}
		for (int i = 0; i < currentTalentConfig.Count; i++)
		{
			int talentId = GetTalentId(currentTalentConfig[i]);
			if (talentItems.ContainsKey(talentId))
			{
				talentItems[talentId].RefeshItem(talentType);
			}
		}
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			TalentSceneType sceneType = ((!IsCoinScene) ? TalentSceneType.DiceSceneTalent : TalentSceneType.CoinSceneTalent);
			((TMP_Text)pointDisplay).text = GameDataManager.Instance.GetTalentPoint(sceneType).ToString();
		}
		for (int j = 0; j < processparent.childCount; j++)
		{
			((Component)processparent.GetChild(j).GetChild(1)).gameObject.SetActive(false);
		}
		for (int k = 0; k < currentTalentConfig.Count; k++)
		{
			int talentId2 = GetTalentId(currentTalentConfig[k]);
			if (!talentItems.ContainsKey(talentId2))
			{
				continue;
			}
			if (talentItems[talentId2].upgradeLevelCurrent < talentItems[talentId2].upgradeLevelMax)
			{
				int costPoint = talentItems[talentId2].GetCostPoint();
				TalentSceneType sceneType2 = ((!IsCoinScene) ? TalentSceneType.DiceSceneTalent : TalentSceneType.CoinSceneTalent);
				if (GameDataManager.Instance.GetTalentPoint(sceneType2) >= costPoint && talentItems[talentId2].isUnlock)
				{
					talentItems[talentId2].SetRedDotState(isShow: true);
				}
				else
				{
					talentItems[talentId2].SetRedDotState(isShow: false);
				}
			}
			else
			{
				talentItems[talentId2].SetRedDotState(isShow: false);
			}
			talentItems[talentId2].SetConnectState();
		}
		if (TalentDataManager.Instance != null && (Object)(object)CurrentTalentManager != (Object)null)
		{
			TalentDataManager.Instance?.DisplayTalentEnterRoPint(0.0);
		}
	}

	public void OpenSkillTip(int skillId)
	{
		if (talentItems.ContainsKey(skillId))
		{
			clickItemId = skillId;
			((TMP_Text)skillTipTitle).text = talentItems[skillId].talentName;
			string talentDesc = talentItems[skillId].talentDesc;
			int num = talentItems[skillId].upgradeLevelCurrent;
			if (num >= talentItems[skillId].upgradeLevelMax - 1)
			{
				num = talentItems[skillId].upgradeLevelMax - 1;
			}
			talentItems[skillId].talentDesc = talentItems[skillId].talentDesc.Replace("+", "<color=#E57533>+</color>");
			talentItems[skillId].talentDesc = talentItems[skillId].talentDesc.Replace("SSS", "<color=#E57533>SSS</color>");
			talentItems[skillId].talentDesc = talentItems[skillId].talentDesc.Replace("%", "<color=#E57533>%</color>");
			talentDesc = ((!talentItems[skillId].talentDesc.Contains("%")) ? talentItems[skillId].talentDesc.Replace("SSS", talentItems[skillId].upgradeResult[num].ToString()) : talentItems[skillId].talentDesc.Replace("SSS", (float.Parse(talentItems[skillId].upgradeResult[num].ToString()) * 100f).ToString()));
			((TMP_Text)skillTipDesc).text = talentDesc;
			((TMP_Text)skillTipCost).text = (talentItems[skillId].baseTalentPoint + talentItems[skillId].costPointAdd * num).ToString();
			int costPoint = talentItems[clickItemId].GetCostPoint();
			if ((Object)(object)GameDataManager.Instance != (Object)null)
			{
				TalentSceneType sceneType = ((!IsCoinScene) ? TalentSceneType.DiceSceneTalent : TalentSceneType.CoinSceneTalent);
				if (GameDataManager.Instance.GetTalentPoint(sceneType) >= costPoint)
				{
					((Selectable)talentUpBtn).interactable = true;
				}
				else
				{
					((Selectable)talentUpBtn).interactable = false;
				}
			}
			int index = 0;
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
			for (int i = 0; i < list2.Count; i++)
			{
				if (GetTalentId(list2[i]) == clickItemId)
				{
					index = i;
					break;
				}
			}
			if ((Object)(object)CurrentTalentManager != (Object)null)
			{
				talentSkillIcon.sprite = CurrentTalentManager.GetSkillIcon(index);
			}
			((Graphic)talentSkillIcon).SetNativeSize();
			((Component)SkillTip).gameObject.SetActive(true);
		}
		else
		{
			Debug.LogError((object)"该天赋配置表中还未配置");
		}
	}

	public TalentCardItem GetTalentUnlock(int id)
	{
		if (id > 0 && talentItems.ContainsKey(id))
		{
			return talentItems[id];
		}
		return null;
	}

	public virtual void ResetTalent()
	{
		int num = 0;
		List<int> list;
		List<bool> list2;
		IList list3;
		if (IsCoinScene)
		{
			list = GameDataManager.Instance.GetTalentLevel();
			list2 = GameDataManager.Instance.GetTalentUnlock();
			list3 = ConfigManager.Instance.GetAllCoinTalentItem();
		}
		else
		{
			TalentData talentData = GameDataManager.Instance.GetTalentData();
			list = talentData?.diceTalentLevel;
			list2 = talentData?.diceTalentUnlock;
			list3 = ConfigManager.Instance.GetAllDiceTalentItem();
		}
		if (list == null || list2 == null || list3 == null)
		{
			return;
		}
		for (int i = 0; i < list.Count && i < list3.Count; i++)
		{
			int talentId = GetTalentId(list3[i]);
			if (talentItems.ContainsKey(talentId) && talentItems[talentId].talentType == talentType)
			{
				list[i] = 0;
				if (IsCoinScene && (talentItems[talentId].id == 1 || talentItems[talentId].id == 7 || talentItems[talentId].id == 14))
				{
					list2[i] = true;
				}
				else
				{
					list2[i] = false;
				}
			}
		}
		TalentSceneType sceneType = ((!IsCoinScene) ? TalentSceneType.DiceSceneTalent : TalentSceneType.CoinSceneTalent);
		num = GameDataManager.Instance.GetCostPoint(talentType, sceneType);
		talentResetTipsItem.SetActive(false);
		ResetTalentDic();
		GameDataManager.Instance.ChangeTalentPoint(isAdd: true, num, sceneType);
		GameDataManager.Instance.ResetCostPoint(talentType, 0, sceneType);
		GameDataManager.Instance.SaveTalentDataOnly();
		_ = IsCoinScene;
		if (TalentDataManager.Instance != null && (Object)(object)CurrentTalentManager != (Object)null)
		{
			TalentDataManager.Instance?.DisplayTalentEnterRoPint(0.0);
		}
	}

	public void ResetTalentDic()
	{
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
		for (int num = list2.Count - 1; num >= 0; num--)
		{
			int talentId = GetTalentId(list2[num]);
			if (talentItems.ContainsKey(talentId) && talentItems[talentId].talentType == talentType)
			{
				talentItems.Remove(talentId);
			}
		}
		InitTalentPanel();
	}

	public virtual void OffWindow()
	{
		talentResetTipsItem.gameObject.SetActive(false);
		((Component)SkillTip).gameObject.SetActive(false);
	}

	public void PlayUpgradeEffect()
	{
		SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/Role/button_upgrade3.prefab", delegate(GameObject prefab)
		{
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Expected O, but got Unknown
			if ((Object)(object)prefab != (Object)null)
			{
				GameObject effect = Object.Instantiate<GameObject>(prefab, ((Component)talentSkillIcon).transform);
				effect.transform.localPosition = Vector3.one * 0.8f;
				effect.transform.localScale = Vector3.one * 0.8f;
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

	public void ShowMaxLevelTip()
	{
		((Component)talentUpBtn).gameObject.SetActive(false);
		((Component)maxLevelTip).gameObject.SetActive(true);
	}

	public void HideMaxLevelTip()
	{
		((Component)talentUpBtn).gameObject.SetActive(true);
		((Component)maxLevelTip).gameObject.SetActive(false);
	}

	public Transform Getprocessparent()
	{
		return processparent;
	}

	public void CreatTalentCard(TalentType type)
	{
		if (TalentDataManager.Instance == null || (Object)(object)CurrentTalentManager == (Object)null)
		{
			return;
		}
		Vector2[] talentPoints = CurrentTalentManager.GetTalentCardPiont();
		IList talent = CurrentTalentManager.GetCurrentTalentConfig(type);
		if (talent == null)
		{
			return;
		}
		if (IsCoinScene)
		{
			(talent as List<talent>)?.Sort((talent a, talent b) => a.Position.CompareTo(b.Position));
		}
		else
		{
			(talent as List<talent2>)?.Sort((talent2 a, talent2 b) => a.Position.CompareTo(b.Position));
		}
		int count = 0;
		for (int num = 0; num < talent.Count; num++)
		{
			int index = num;
			SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/NewUI/Talent/prefabs/talentCardItem.prefab", delegate(GameObject prefab)
			{
				//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
				GameObject obj = Object.Instantiate<GameObject>(prefab, talentParent);
				int num2 = (IsCoinScene ? ((talent)talent[index]).Position : ((talent2)talent[index]).Position);
				int num3 = 0;
				if (type == TalentType.talentCoin)
				{
					num3 = num2 - 101;
				}
				else if (type == TalentType.talentHelper)
				{
					num3 = num2 - 201;
				}
				else if (type == TalentType.talentSpecial)
				{
					num3 = num2 - 301;
				}
				Transform transform = obj.transform;
				((RectTransform)((transform is RectTransform) ? transform : null)).anchoredPosition = talentPoints[num3];
				count++;
				if (count >= talent.Count)
				{
					InitTalentPanel();
				}
			});
		}
	}

	public bool GetPanelRepoint()
	{
		if (TalentDataManager.Instance == null || (Object)(object)CurrentTalentManager == (Object)null)
		{
			return false;
		}
		IList currentTalentConfig = CurrentTalentManager.GetCurrentTalentConfig(talentType);
		if (currentTalentConfig == null)
		{
			return false;
		}
		isDisplayRepoint = false;
		TalentSceneType sceneType = ((!IsCoinScene) ? TalentSceneType.DiceSceneTalent : TalentSceneType.CoinSceneTalent);
		for (int i = 0; i < currentTalentConfig.Count; i++)
		{
			int talentId = GetTalentId(currentTalentConfig[i]);
			if (talentItems.ContainsKey(talentId) && talentItems[talentId].upgradeLevelCurrent < talentItems[talentId].upgradeLevelMax)
			{
				int costPoint = talentItems[talentId].GetCostPoint();
				if (GameDataManager.Instance.GetTalentPoint(sceneType) >= costPoint && talentItems[talentId].isUnlock)
				{
					isDisplayRepoint = true;
					break;
				}
			}
		}
		return isDisplayRepoint;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using cfg;

public class TalentManager : MonoBehaviour
{
	[Header("面板配置天赋场景类型")]
	public TalentSceneType talentSceneType;

	[Header("金币天赋面板")]
	[SerializeField]
	private Transform talentCoin;

	[Header("助手天赋面板")]
	[SerializeField]
	private Transform talentHelper;

	[Header("特殊天赋面板")]
	[SerializeField]
	private Transform talentSpecial;

	[Header("备份天赋面板")]
	[SerializeField]
	private Transform talentBackup;

	[Header("金钱天赋开启按键")]
	[SerializeField]
	private Button talentCoinBtn;

	[Header("助手天赋开启按键")]
	[SerializeField]
	private Button talentHelperBtn;

	[Header("特殊天赋开启按键")]
	[SerializeField]
	private Button talentSpecialBtn;

	[Header("备份天赋开启按键")]
	[SerializeField]
	private Button talentBackupBtn;

	[Header("天赋页签父物体")]
	[SerializeField]
	private Transform tabBar;

	[Header("目前所在的天赋面板")]
	private TalentType currentTalentType;

	[Header("技能提示框中存储图片列表")]
	[SerializeField]
	private Sprite[] SkillIconList;

	[Header("三个界面的连线父物体")]
	[SerializeField]
	private Transform[] processparent;

	[Header("三个天赋面板上方的红点")]
	[SerializeField]
	public GameObject[] tabRepoint;

	[Header("重生处的指引物体")]
	[SerializeField]
	public GameObject fingerGuide;

	[Header("三个面板重生按键处的红点icon")]
	[SerializeField]
	public GameObject[] rebornRedPoint;

	private List<talent> coinTalentList;

	private List<talent> helperTalentList;

	private List<talent> specialTalentList;

	private List<talent2> diceCoinTalentList;

	private List<talent2> diceHelperTalentList;

	private List<talent2> diceSpecialTalentList;

	private Vector2[] talentPoints;

	private Dictionary<int, TalentCardItem> talentItems;

	public TalentFramework[] talentPanels;

	private const string rebornTip = "rebornTip";

	private void Awake()
	{
		talentPanels = new TalentFramework[3];
		talentItems = new Dictionary<int, TalentCardItem>();
		CloseAllSubPanels();
		Init();
		if (TalentDataManager.Instance != null)
		{
			if (TalentDataManager.Instance.CurrentTalentSceneType == TalentSceneType.DiceSceneTalent)
			{
				TalentDataManager.Instance.DiceSceneTalent = this;
			}
			else
			{
				TalentDataManager.Instance.CoinSceneTalent = this;
			}
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown((KeyCode)32))
		{
			TalentDataManager.Instance.GetNSRHelperCount(0);
		}
	}

	private void Start()
	{
		if ((Object)(object)talentPanels[0] == (Object)null)
		{
			TalentFramework[] array = talentPanels;
			Transform obj = talentCoin;
			array[0] = ((obj != null) ? ((Component)obj).GetComponent<TalentCoin>() : null);
		}
		if ((Object)(object)talentPanels[1] == (Object)null)
		{
			TalentFramework[] array2 = talentPanels;
			Transform obj2 = talentHelper;
			array2[1] = ((obj2 != null) ? ((Component)obj2).GetComponent<TalentHelper>() : null);
		}
		if ((Object)(object)talentPanels[2] == (Object)null)
		{
			TalentFramework[] array3 = talentPanels;
			Transform obj3 = talentSpecial;
			array3[2] = ((obj3 != null) ? ((Component)obj3).GetComponent<TalentSpecial>() : null);
		}
		HideConect();
		TalentDataManager.Instance?.DisplayTalentEnterRoPint(0.0);
	}

	private void Init()
	{
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Expected O, but got Unknown
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Expected O, but got Unknown
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Expected O, but got Unknown
		coinTalentList = new List<talent>();
		helperTalentList = new List<talent>();
		specialTalentList = new List<talent>();
		diceCoinTalentList = new List<talent2>();
		diceHelperTalentList = new List<talent2>();
		diceSpecialTalentList = new List<talent2>();
		if (TalentDataManager.Instance != null && TalentDataManager.Instance.CurrentTalentSceneType == TalentSceneType.DiceSceneTalent)
		{
			List<talent2> allDiceTalentItem = ConfigManager.Instance.GetAllDiceTalentItem();
			for (int i = 0; i < allDiceTalentItem.Count; i++)
			{
				if (allDiceTalentItem[i].TalentType == 1)
				{
					diceCoinTalentList.Add(allDiceTalentItem[i]);
				}
				else if (allDiceTalentItem[i].TalentType == 2)
				{
					diceHelperTalentList.Add(allDiceTalentItem[i]);
				}
				else if (allDiceTalentItem[i].TalentType == 3)
				{
					diceSpecialTalentList.Add(allDiceTalentItem[i]);
				}
			}
		}
		else
		{
			List<talent> allCoinTalentItem = ConfigManager.Instance.GetAllCoinTalentItem();
			for (int j = 0; j < allCoinTalentItem.Count; j++)
			{
				if (allCoinTalentItem[j].TalentType == 1)
				{
					coinTalentList.Add(allCoinTalentItem[j]);
				}
				else if (allCoinTalentItem[j].TalentType == 2)
				{
					helperTalentList.Add(allCoinTalentItem[j]);
				}
				else if (allCoinTalentItem[j].TalentType == 3)
				{
					specialTalentList.Add(allCoinTalentItem[j]);
				}
			}
		}
		talentPoints = (Vector2[])(object)new Vector2[13]
		{
			new Vector2(0f, 613f),
			new Vector2(-350f, 330f),
			new Vector2(350f, 330f),
			new Vector2(-350f, -12f),
			new Vector2(350f, -12f),
			new Vector2(-504f, -361f),
			new Vector2(-198f, -361f),
			new Vector2(198f, -361f),
			new Vector2(502f, -361f),
			new Vector2(-504f, -685f),
			new Vector2(-198f, -685f),
			new Vector2(198f, -685f),
			new Vector2(502f, -685f)
		};
		currentTalentType = TalentType.talentCoin;
		((UnityEvent)talentCoinBtn.onClick).AddListener((UnityAction)delegate
		{
			OpenPanel(TalentType.talentCoin);
		});
		((UnityEvent)talentHelperBtn.onClick).AddListener((UnityAction)delegate
		{
			OpenPanel(TalentType.talentHelper);
		});
		((UnityEvent)talentSpecialBtn.onClick).AddListener((UnityAction)delegate
		{
			OpenPanel(TalentType.talentSpecial);
		});
		OpenPanel(TalentType.talentCoin);
	}

	public void OpenPanel(TalentType type)
	{
		((Component)talentCoin).gameObject.SetActive(false);
		((Component)talentHelper).gameObject.SetActive(false);
		((Component)talentSpecial).gameObject.SetActive(false);
		((Component)talentBackup).gameObject.SetActive(false);
		((Component)talentCoinBtn).gameObject.SetActive(true);
		((Component)talentHelperBtn).gameObject.SetActive(true);
		((Component)talentSpecialBtn).gameObject.SetActive(true);
		((Component)talentBackupBtn).gameObject.SetActive(true);
		currentTalentType = type;
		for (int i = 0; i < tabBar.childCount; i++)
		{
			((Component)tabBar.GetChild(i)).gameObject.SetActive(false);
		}
		switch (type)
		{
		case TalentType.talentCoin:
			((Component)talentCoin).gameObject.SetActive(true);
			if ((Object)(object)talentPanels[0] != (Object)null)
			{
				talentPanels[0].RefreshTalentPanel();
				talentPanels[0].OffWindow();
			}
			((Component)tabBar.GetChild(0)).gameObject.SetActive(true);
			((Component)talentCoinBtn).gameObject.SetActive(false);
			break;
		case TalentType.talentHelper:
			((Component)talentHelper).gameObject.SetActive(true);
			if ((Object)(object)talentPanels[1] != (Object)null)
			{
				talentPanels[1].RefreshTalentPanel();
				talentPanels[1].OffWindow();
			}
			((Component)tabBar.GetChild(1)).gameObject.SetActive(true);
			((Component)talentHelperBtn).gameObject.SetActive(false);
			break;
		case TalentType.talentSpecial:
			((Component)talentSpecial).gameObject.SetActive(true);
			if ((Object)(object)talentPanels[2] != (Object)null)
			{
				talentPanels[2].RefreshTalentPanel();
				talentPanels[2].OffWindow();
			}
			((Component)tabBar.GetChild(2)).gameObject.SetActive(true);
			((Component)talentSpecialBtn).gameObject.SetActive(false);
			break;
		}
	}

	public void CloseAllSubPanels()
	{
		if ((Object)(object)talentCoin != (Object)null)
		{
			((Component)talentCoin).gameObject.SetActive(false);
		}
		if ((Object)(object)talentHelper != (Object)null)
		{
			((Component)talentHelper).gameObject.SetActive(false);
		}
		if ((Object)(object)talentSpecial != (Object)null)
		{
			((Component)talentSpecial).gameObject.SetActive(false);
		}
		if ((Object)(object)talentBackup != (Object)null)
		{
			((Component)talentBackup).gameObject.SetActive(false);
		}
	}

	public TalentType GetCurrentTalentType()
	{
		return currentTalentType;
	}

	public Vector2[] GetTalentCardPiont()
	{
		return talentPoints;
	}

	public List<talent> GetTaletnConfig(TalentType type)
	{
		if (coinTalentList == null)
		{
			Init();
		}
		return type switch
		{
			TalentType.talentCoin => coinTalentList, 
			TalentType.talentHelper => helperTalentList, 
			TalentType.talentSpecial => specialTalentList, 
			_ => null, 
		};
	}

	public IList GetCurrentTalentConfig(TalentType type)
	{
		if (TalentDataManager.Instance != null && TalentDataManager.Instance.CurrentTalentSceneType == TalentSceneType.DiceSceneTalent)
		{
			return type switch
			{
				TalentType.talentCoin => diceCoinTalentList, 
				TalentType.talentHelper => diceHelperTalentList, 
				TalentType.talentSpecial => diceSpecialTalentList, 
				_ => null, 
			};
		}
		return type switch
		{
			TalentType.talentCoin => coinTalentList, 
			TalentType.talentHelper => helperTalentList, 
			TalentType.talentSpecial => specialTalentList, 
			_ => null, 
		};
	}

	public Sprite GetSkillIcon(int index)
	{
		if (index < SkillIconList.Length)
		{
			return SkillIconList[index];
		}
		return SkillIconList[0];
	}

	public Dictionary<int, TalentCardItem> GetTalentCards()
	{
		return talentItems;
	}

	private void HideConect()
	{
		int[] array = new int[3];
		IList currentTalentConfig = GetCurrentTalentConfig(TalentType.talentCoin);
		IList currentTalentConfig2 = GetCurrentTalentConfig(TalentType.talentHelper);
		IList currentTalentConfig3 = GetCurrentTalentConfig(TalentType.talentSpecial);
		array[0] = GetMaxConnect(currentTalentConfig?.Count ?? 0);
		array[1] = GetMaxConnect(currentTalentConfig2?.Count ?? 0);
		array[2] = GetMaxConnect(currentTalentConfig3?.Count ?? 0);
		for (int i = 0; i < processparent.Length; i++)
		{
			for (int j = 0; j < processparent[i].childCount; j++)
			{
				if (j >= array[i])
				{
					((Component)processparent[i].GetChild(j)).gameObject.SetActive(false);
				}
			}
		}
	}

	private int GetMaxConnect(int index)
	{
		return index switch
		{
			1 => 0, 
			2 => 1, 
			3 => 2, 
			4 => 3, 
			5 => 4, 
			6 => 5, 
			7 => 6, 
			8 => 7, 
			9 => 8, 
			10 => 9, 
			11 => 10, 
			12 => 11, 
			13 => 12, 
			_ => 0, 
		};
	}

	public void OffFingerGuide()
	{
		fingerGuide.SetActive(false);
	}

	private void OnDestroy()
	{
	}
}

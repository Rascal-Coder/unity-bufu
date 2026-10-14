using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using cfg;

public class TalentDataManager
{
	public TalentManager CoinSceneTalent;

	public TalentManager DiceSceneTalent;

	private const string rebornTip = "rebornTip";

	private TalentSceneType _currentTalentSceneType;

	private static TalentDataManager instance;

	public TalentSceneType CurrentTalentSceneType
	{
		get
		{
			GameDataManager gameDataManager = GameDataManager.Instance;
			return _currentTalentSceneType = ((gameDataManager == null || gameDataManager.GetCurrentWorldId() != 1) ? TalentSceneType.DiceSceneTalent : TalentSceneType.CoinSceneTalent);
		}
	}

	public bool IsCoinScene => CurrentTalentSceneType == TalentSceneType.CoinSceneTalent;

	public static TalentDataManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new TalentDataManager();
			}
			return instance;
		}
	}

	public TalentManager GetCurrentTalentManager()
	{
		if (CurrentTalentSceneType != TalentSceneType.DiceSceneTalent)
		{
			return CoinSceneTalent;
		}
		return DiceSceneTalent;
	}

	public TalentDataManager()
	{
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
		{
			SingletonMono<UpgradeManager>.Instance.OnGoldChanged += DisplayTalentEnterRoPint;
			SingletonMono<UpgradeManager>.Instance.OnGoldChanged += NewPlayerRebornGuide;
		}
	}

	public int GetTalentLevel(int id, TalentSceneType sceneType = TalentSceneType.CoinSceneTalent)
	{
		TalentData talentData = GameDataManager.Instance.GetTalentData();
		if (talentData == null)
		{
			return 0;
		}
		IList list2;
		if (sceneType != TalentSceneType.DiceSceneTalent)
		{
			IList list = ConfigManager.Instance?.GetAllCoinTalentItem();
			list2 = list;
		}
		else
		{
			IList list = ConfigManager.Instance?.GetAllDiceTalentItem();
			list2 = list;
		}
		IList list3 = list2;
		int num = 0;
		if (list3 == null)
		{
			return 0;
		}
		for (int i = 0; i < list3.Count; i++)
		{
			if (((sceneType == TalentSceneType.DiceSceneTalent) ? ((talent2)list3[i]).Id : ((talent)list3[i]).Id) == id)
			{
				num = i;
				break;
			}
		}
		List<int> list4 = ((sceneType == TalentSceneType.DiceSceneTalent) ? talentData.diceTalentLevel : talentData.talentLevel);
		if (list4 == null || list4.Count <= num)
		{
			return 0;
		}
		return list4[num];
	}

	public float GetUpgradeResult(int id, TalentSceneType sceneType = TalentSceneType.CoinSceneTalent)
	{
		int talentLevel = GetTalentLevel(id, sceneType);
		object obj = ((sceneType != TalentSceneType.DiceSceneTalent) ? ((object)ConfigManager.Instance?.GetCoinTalentConfig(id)) : ((object)ConfigManager.Instance?.GetDiceTalentConfig(id)));
		if (obj is talent talent && talentLevel > 0)
		{
			if (talent.UpgradeResult.Count > talentLevel - 1)
			{
				return talent.UpgradeResult[talentLevel - 1];
			}
		}
		else
		{
			if (!(obj is talent2 talent2) || talentLevel <= 0)
			{
				return 0f;
			}
			if (talent2.UpgradeResult.Count > talentLevel - 1)
			{
				return talent2.UpgradeResult[talentLevel - 1];
			}
		}
		return 0f;
	}

	public float LianJinBuff()
	{
		if (!IsCoinScene)
		{
			return 0f;
		}
		return GetUpgradeResult(101);
	}

	public bool IsUnlockedPurpleCoin()
	{
		if (!IsCoinScene)
		{
			return false;
		}
		if (GetTalentLevel(102) <= 0)
		{
			return false;
		}
		return true;
	}

	public float ManualBuff()
	{
		if (!IsCoinScene)
		{
			return 0f;
		}
		return GetUpgradeResult(103);
	}

	public float CoinRain()
	{
		if (!IsCoinScene)
		{
			return 1f;
		}
		float upgradeResult = GetUpgradeResult(106);
		return 1f - upgradeResult;
	}

	public bool IsFlipPurple()
	{
		if (!IsCoinScene)
		{
			return false;
		}
		if (GetTalentLevel(107) > 0)
		{
			return true;
		}
		return false;
	}

	public float LuckyFlip()
	{
		if (!IsCoinScene)
		{
			return 1f;
		}
		float upgradeResult = GetUpgradeResult(108);
		return 1f + upgradeResult;
	}

	public float UpPurpleValue()
	{
		if (!IsCoinScene)
		{
			return 0f;
		}
		return GetUpgradeResult(109);
	}

	public float ShiMian()
	{
		if (!IsCoinScene)
		{
			return 1f;
		}
		if (GetTalentLevel(201) > 0)
		{
			return GetUpgradeResult(201);
		}
		return 1f;
	}

	public float GetTalentExtraGold()
	{
		if (!IsCoinScene)
		{
			return 0f;
		}
		float num = 0f;
		int num2 = 0;
		int[] array = new int[ConfigManager.Instance.GetAllHelperRole().Count];
		if ((Object)(object)ConfigManager.Instance != (Object)null && (Object)(object)GameDataManager.Instance != (Object)null)
		{
			int count = ConfigManager.Instance.GetAllHelperRole().Count;
			for (int i = 1; i <= count; i++)
			{
				if (GameDataManager.Instance.GetHelperCount(i) > 0)
				{
					num2++;
				}
				if (GameDataManager.Instance.GetHelperCount(i) >= 0)
				{
					array[i - 1] = GameDataManager.Instance.GetHelperCount(i);
				}
			}
		}
		if ((Object)(object)SingletonMono<BuffManager>.Instance != (Object)null)
		{
			num = ((GetTalentLevel(205) > 0) ? (GetUpgradeResult(205) * (float)num2) : 0f);
			if (GetTalentLevel(202) > 0)
			{
				num += GetUpgradeResult(202) * (float)array[0];
			}
			if (GetTalentLevel(203) > 0)
			{
				num += GetUpgradeResult(203) * (float)array[1];
			}
			if (GetTalentLevel(204) > 0)
			{
				num += GetUpgradeResult(204) * (float)array[2];
			}
		}
		return num;
	}

	public float JiuWei()
	{
		if (!IsCoinScene)
		{
			return 0f;
		}
		return GetTalentLevel(208);
	}

	public float GangTie()
	{
		if (!IsCoinScene)
		{
			return 0f;
		}
		return GetTalentLevel(209);
	}

	public int GetMaxLevelUp()
	{
		if (!IsCoinScene)
		{
			return 0;
		}
		int result = 0;
		int talentLevel = GetTalentLevel(301);
		if (talentLevel > 0)
		{
			result = (int)GetUpgradeResult(301) * talentLevel;
		}
		return result;
	}

	public bool IsUnlockedHuiXiang()
	{
		if (!IsCoinScene)
		{
			return false;
		}
		if (GetTalentLevel(302) <= 0)
		{
			return false;
		}
		return true;
	}

	public float GetTalentCriticalRate()
	{
		TalentSceneType sceneType = ((!IsCoinScene) ? TalentSceneType.DiceSceneTalent : TalentSceneType.CoinSceneTalent);
		int id = (IsCoinScene ? 303 : 302);
		return GetUpgradeResult(id, sceneType);
	}

	public float GetMonopoly()
	{
		if (!IsCoinScene)
		{
			return 1f;
		}
		float num = 1f;
		if (GetTalentLevel(305) > 0)
		{
			num += GetUpgradeResult(305);
		}
		return num;
	}

	public float ChallengeMaster()
	{
		if (!IsCoinScene)
		{
			return 0f;
		}
		float result = 0f;
		if (GetTalentLevel(306) > 0)
		{
			result = GetUpgradeResult(306);
		}
		return result;
	}

	public float SpacetimeWrap()
	{
		if (!IsCoinScene)
		{
			return 1f;
		}
		float num = 1f;
		if (GetTalentLevel(307) > 0)
		{
			num += GetUpgradeResult(307);
		}
		return num;
	}

	public int RebornReward()
	{
		if (!IsCoinScene)
		{
			return 0;
		}
		float num = 0f;
		if (GetTalentLevel(308) > 0)
		{
			num += GetUpgradeResult(308);
		}
		return (int)num;
	}

	public float LossBrings()
	{
		if (!IsCoinScene)
		{
			return 0f;
		}
		float num = 0f;
		if (GetTalentLevel(309) > 0)
		{
			num += GetUpgradeResult(309);
		}
		return num;
	}

	public bool IsCanReborn()
	{
		TalentSceneType sceneType = ((!IsCoinScene) ? TalentSceneType.DiceSceneTalent : TalentSceneType.CoinSceneTalent);
		return IsCanReborn(sceneType);
	}

	public bool IsCanReborn(TalentSceneType sceneType)
	{
		if ((Object)(object)ConfigManager.Instance == (Object)null || (Object)(object)GameDataManager.Instance == (Object)null || (Object)(object)SingletonMono<UpgradeManager>.Instance == (Object)null || GameDataManager.Instance.GetTalentData() == null)
		{
			return false;
		}
		int num = 0;
		List<talentPoint> allTalentPoint = ConfigManager.Instance.GetAllTalentPoint();
		List<talentPoint2> list = null;
		if (sceneType == TalentSceneType.DiceSceneTalent)
		{
			list = ConfigManager.Instance.GetAllTalentPoint2();
		}
		int num2 = CalculateLevel(sceneType);
		int rebornCurrentLevel = GameDataManager.Instance.GetRebornCurrentLevel(sceneType);
		int num3 = ((sceneType != TalentSceneType.DiceSceneTalent) ? allTalentPoint.Count : (list?.Count ?? 0));
		if (num2 < num3 && num2 > rebornCurrentLevel)
		{
			int num4 = 0;
			for (int i = rebornCurrentLevel + 1; i <= num2; i++)
			{
				num = ((sceneType != TalentSceneType.DiceSceneTalent || list == null) ? (num + int.Parse(allTalentPoint[i].TalentDesc)) : (num + int.Parse(list[i].TalentDesc)));
			}
			num4 = ((sceneType != TalentSceneType.CoinSceneTalent) ? ((int)GetDiceTalentBuff(310)) : RebornReward());
			num += num4;
		}
		if (num <= 0)
		{
			return false;
		}
		return true;
	}

	public int CalculateLevel()
	{
		TalentSceneType sceneType = ((!IsCoinScene) ? TalentSceneType.DiceSceneTalent : TalentSceneType.CoinSceneTalent);
		return CalculateLevel(sceneType);
	}

	public int CalculateLevel(TalentSceneType sceneType)
	{
		double totalGold = SingletonMono<UpgradeManager>.Instance.TotalGold;
		if (sceneType == TalentSceneType.DiceSceneTalent)
		{
			List<talentPoint2> allTalentPoint = ConfigManager.Instance.GetAllTalentPoint2();
			if (allTalentPoint == null)
			{
				return -1;
			}
			for (int i = 0; i < allTalentPoint.Count; i++)
			{
				double num = allTalentPoint[i].TalentName;
				if (totalGold < 0.0)
				{
					return allTalentPoint.Count - 1;
				}
				if (totalGold < num)
				{
					return i - 1;
				}
			}
			return allTalentPoint.Count - 1;
		}
		List<talentPoint> allTalentPoint2 = ConfigManager.Instance.GetAllTalentPoint();
		if (allTalentPoint2 == null)
		{
			return -1;
		}
		for (int j = 0; j < allTalentPoint2.Count; j++)
		{
			double num2 = allTalentPoint2[j].TalentName;
			if (totalGold < 0.0)
			{
				return allTalentPoint2.Count - 1;
			}
			if (totalGold < num2)
			{
				return j - 1;
			}
		}
		return allTalentPoint2.Count - 1;
	}

	public void DisplayTalentEnterRoPint(double count)
	{
		bool flag = IsCanReborn();
		TalentManager currentTalentManager = GetCurrentTalentManager();
		if (flag)
		{
			if ((Object)(object)currentTalentManager != (Object)null && currentTalentManager.rebornRedPoint.Length >= 3 && !currentTalentManager.rebornRedPoint[0].activeSelf)
			{
				currentTalentManager.rebornRedPoint[0].SetActive(true);
				currentTalentManager.rebornRedPoint[1].SetActive(true);
				currentTalentManager.rebornRedPoint[2].SetActive(true);
			}
		}
		else if ((Object)(object)currentTalentManager != (Object)null && currentTalentManager.rebornRedPoint.Length >= 3 && currentTalentManager.rebornRedPoint[0].activeSelf)
		{
			currentTalentManager.rebornRedPoint[0].SetActive(false);
			currentTalentManager.rebornRedPoint[1].SetActive(false);
			currentTalentManager.rebornRedPoint[2].SetActive(false);
		}
		bool flag2 = false;
		if ((Object)(object)currentTalentManager != (Object)null && (Object)(object)currentTalentManager.talentPanels[0] != (Object)null && (Object)(object)currentTalentManager.talentPanels[1] != (Object)null && (Object)(object)currentTalentManager.talentPanels[2] != (Object)null)
		{
			bool panelRepoint = currentTalentManager.talentPanels[0].GetPanelRepoint();
			bool panelRepoint2 = currentTalentManager.talentPanels[1].GetPanelRepoint();
			bool panelRepoint3 = currentTalentManager.talentPanels[2].GetPanelRepoint();
			bool flag3 = IsCanReborn();
			if (panelRepoint || panelRepoint2 || panelRepoint3 || flag3)
			{
				flag2 = true;
			}
			currentTalentManager.tabRepoint[0].SetActive(panelRepoint || flag3);
			currentTalentManager.tabRepoint[1].SetActive(panelRepoint2 || flag3);
			currentTalentManager.tabRepoint[2].SetActive(panelRepoint3 || flag3);
		}
		int currentMainMissionLevel = GameDataManager.Instance.GetCurrentMainMissionLevel();
		int num = (int)ConfigManager.Instance.GetCommonFactor(21);
		if ((Object)(object)UpgradePanel.Instance != (Object)null)
		{
			UpgradePanel.Instance.TalentRepointSet((flag2 || flag) && currentMainMissionLevel > num);
		}
	}

	public void NewPlayerRebornGuide(double count)
	{
		if (!IsCoinScene || (OfflineIncomeManager.HasInstance() && OfflineIncomeManager.Instance.GetOffLineUIIsActive()) || (Object)(object)ConfigManager.Instance == (Object)null || (Object)(object)GameDataManager.Instance == (Object)null || (Object)(object)SingletonMono<UpgradeManager>.Instance == (Object)null || GameDataManager.Instance.GetTalentData() == null || PlayerPrefs.GetInt("rebornTip", 0) > 0)
		{
			return;
		}
		bool num = IsCanReborn();
		int missionLevel = GameDataManager.Instance.GetCurrentMainMissionLevel();
		int factor = (int)ConfigManager.Instance.GetCommonFactor(21);
		int num2 = 0;
		if ((Object)(object)UpgradePanel.Instance != (Object)null)
		{
			num2 = UpgradePanel.Instance.currentSelectedIndex;
		}
		if (!num || missionLevel <= factor || (num2 != 1 && num2 != 3))
		{
			return;
		}
		int rebornCurrentLevel = GameDataManager.Instance.GetRebornCurrentLevel();
		PlayerPrefs.SetInt("rebornTip", 10);
		PlayerPrefs.Save();
		if (rebornCurrentLevel >= 0)
		{
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/NewUI/Talent/prefabs/GoTalentTip.prefab", delegate(GameObject prefab)
		{
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Expected O, but got Unknown
			Transform layerTransform = UIManager.Instance.GetLayerTransform(UILayer.Popup);
			GameObject obj = Object.Instantiate<GameObject>(prefab, layerTransform);
			Button component = ((Component)obj.transform.Find("gobtn")).GetComponent<Button>();
			if ((Object)(object)component != (Object)null)
			{
				((UnityEvent)component.onClick).AddListener((UnityAction)delegate
				{
					UpgradePanel.Instance.OnButtonClicked(3);
					if ((Object)(object)CoinSceneTalent != (Object)null)
					{
						if (missionLevel > factor && CoinSceneTalent.rebornRedPoint.Length >= 3 && !CoinSceneTalent.rebornRedPoint[0].activeSelf)
						{
							CoinSceneTalent.rebornRedPoint[0].SetActive(true);
							CoinSceneTalent.rebornRedPoint[1].SetActive(true);
							CoinSceneTalent.rebornRedPoint[2].SetActive(true);
						}
						if ((Object)(object)CoinSceneTalent.fingerGuide != (Object)null)
						{
							CoinSceneTalent.fingerGuide.gameObject.SetActive(true);
							SkeletonGraphic component2 = ((Component)CoinSceneTalent.fingerGuide.transform).GetComponent<SkeletonGraphic>();
							if ((Object)(object)component2 != (Object)null)
							{
								component2.AnimationState.SetAnimation(0, "dianji", true);
							}
						}
					}
					Object.Destroy((Object)(object)obj);
				});
			}
		});
	}

	public float GetDiceTalentBuff(int id)
	{
		float num = 0f;
		float upgradeResult = GetUpgradeResult(id, TalentSceneType.DiceSceneTalent);
		float num2 = GetTalentLevel(id, TalentSceneType.DiceSceneTalent);
		num = upgradeResult;
		switch (id)
		{
		case 102:
		case 104:
			num = ((!(num2 <= 0f)) ? 1 : 0);
			break;
		case 110:
			num = ((num2 <= 0f) ? 0f : upgradeResult);
			break;
		case 105:
			num = ((num2 <= 0f) ? 1f : upgradeResult);
			break;
		case 112:
			num = ((num2 <= 0f) ? 1f : (1f + upgradeResult));
			break;
		case 107:
			num = ((num2 <= 0f) ? 1f : (1f - upgradeResult));
			break;
		case 111:
			num = ((num2 <= 0f) ? 0f : (upgradeResult / 60f));
			break;
		case 201:
			num = ((num2 <= 0f) ? 0f : (upgradeResult * num2));
			break;
		case 203:
			num = ((num2 <= 0f) ? 0f : (upgradeResult * (float)GetHelperCount(1)));
			break;
		case 205:
			num = ((num2 <= 0f) ? 0f : (upgradeResult * (float)GetHelperCount(2)));
			break;
		case 209:
			num = ((num2 <= 0f) ? 0f : (upgradeResult * (float)GetHelperCount(3)));
			break;
		case 210:
			num = ((num2 <= 0f) ? 0f : (upgradeResult * (float)GetHelperCount(4)));
			break;
		case 204:
		case 207:
		case 213:
			num = ((num2 <= 0f) ? 1f : upgradeResult);
			break;
		case 206:
			num = ((num2 <= 0f) ? 0f : (upgradeResult * (float)GetNSRHelperCount(3)));
			break;
		case 208:
			num = ((num2 <= 0f) ? 0f : (upgradeResult * (float)GetNSRHelperCount(1)));
			break;
		case 211:
			num = ((num2 <= 0f) ? 0f : (upgradeResult * (float)GetNSRHelperCount(4)));
			break;
		case 212:
			num = ((!(num2 <= 0f)) ? 1 : 0);
			break;
		case 301:
			num = ((num2 <= 0f) ? 0f : (upgradeResult * num2));
			break;
		case 304:
			num = ((num2 <= 0f) ? 0f : (Mathf.Round(upgradeResult / 60f * 10f) / 10f));
			break;
		case 305:
			num = ((num2 <= 0f) ? 1f : (1f + upgradeResult));
			break;
		case 307:
			num = ((num2 <= 0f) ? 1f : upgradeResult);
			break;
		}
		return num;
	}

	public int GetHelperCount(int index)
	{
		int result = 0;
		int num = 0;
		int[] array = new int[ConfigManager.Instance.GetAllHelperRole().Count];
		if ((Object)(object)ConfigManager.Instance != (Object)null && (Object)(object)GameDataManager.Instance != (Object)null)
		{
			int count = ConfigManager.Instance.GetAllHelperRole().Count;
			for (int i = 1; i <= count; i++)
			{
				if (GameDataManager.Instance.GetHelperCount(i) > 0)
				{
					num++;
				}
				if (GameDataManager.Instance.GetHelperCount(i) >= 0)
				{
					array[i - 1] = GameDataManager.Instance.GetHelperCount(i);
				}
			}
		}
		switch (index)
		{
		case 1:
			result = array[0];
			break;
		case 2:
			result = array[1];
			break;
		case 3:
			result = array[2];
			break;
		case 4:
			result = num;
			break;
		}
		return result;
	}

	public int GetNSRHelperCount(int index)
	{
		int result = 0;
		int[] array = new int[4];
		int num = ConfigManager.Instance.GetAllHelperRole().Count + 1;
		if ((Object)(object)ConfigManager.Instance != (Object)null && (Object)(object)GameDataManager.Instance != (Object)null)
		{
			for (int i = 1; i < num; i++)
			{
				if (i >= 1 && i <= 2)
				{
					array[1] += GameDataManager.Instance.GetHelperCount(i);
				}
				else if (i >= 3 && i <= 4)
				{
					array[2] += GameDataManager.Instance.GetHelperCount(i);
				}
				else
				{
					array[3] += GameDataManager.Instance.GetHelperCount(i);
				}
			}
		}
		int currentWorldHelperUpgradeId = Constants.GetCurrentWorldHelperUpgradeId();
		int upgradeLevel = SingletonMono<UpgradeManager>.Instance.GetUpgradeLevel(currentWorldHelperUpgradeId, "itemUpgrade1");
		int allHelperCount = GameDataManager.Instance.GetAllHelperCount();
		array[0] = upgradeLevel - allHelperCount;
		array[0] = Mathf.Clamp(array[0], 0, upgradeLevel);
		switch (index)
		{
		case 1:
			result = array[0];
			break;
		case 2:
			result = array[1];
			break;
		case 3:
			result = array[2];
			break;
		case 4:
			result = array[3];
			break;
		}
		return result;
	}

	private void OnDisable()
	{
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
		{
			SingletonMono<UpgradeManager>.Instance.OnGoldChanged -= DisplayTalentEnterRoPint;
			SingletonMono<UpgradeManager>.Instance.OnGoldChanged -= NewPlayerRebornGuide;
		}
	}

	public int AddSumCount()
	{
		int[] array = new int[4];
		for (int i = 1; i <= 4; i++)
		{
			array[i - 1] = GetNSRHelperCount(i);
		}
		int num = array.Length;
		for (int j = 0; j < num - 1; j++)
		{
			bool flag = false;
			for (int k = 0; k < num - 1 - j; k++)
			{
				if (array[k] < array[k + 1])
				{
					int num2 = array[k];
					array[k] = array[k + 1];
					array[k + 1] = num2;
					flag = true;
				}
			}
			if (!flag)
			{
				break;
			}
		}
		int num3 = 0;
		for (int l = 0; l < num; l++)
		{
			num3 += array[l];
		}
		Debug.Log((object)$"[TalentDataManager] AddSumCount 排序后数量: [{array[0]}, {array[1]}, {array[2]}, {array[3]}], 总数: {num3}");
		return num3;
	}
}

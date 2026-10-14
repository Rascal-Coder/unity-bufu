using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using cfg;

public class TalentRebornTip : MonoBehaviour
{
	[Header("关闭界面按键")]
	[SerializeField]
	private Button backBtn;

	[Header("重生可获取天赋进度文本")]
	[SerializeField]
	private TextMeshProUGUI progresstxt;

	[Header("重生可获取天赋进度ui")]
	[SerializeField]
	private Image processing;

	[Header("重生可获取天赋点数文本1")]
	[SerializeField]
	private TextMeshProUGUI addpointtxt1;

	[Header("重生可获取天赋点数文本2")]
	[SerializeField]
	private TextMeshProUGUI addpointtxt2;

	[Header("重生点击按键")]
	[SerializeField]
	private Button rebornBtn;

	[Header("可重生时候文本")]
	[SerializeField]
	private TextMeshProUGUI canRebornTxt;

	[Header("不可重生时候文本")]
	[SerializeField]
	private TextMeshProUGUI noRebornTxt;

	[Header("全局获取收益文本")]
	[SerializeField]
	private TextMeshProUGUI GainText;

	[Header("背景遮罩按键")]
	[SerializeField]
	private Button mask;

	[Header("重生模式选择框物体")]
	[SerializeField]
	private GameObject rebornSelectPanel;

	[Header("重生模式选择框物体")]
	[SerializeField]
	private Button adRebornBtn;

	[Header("重生模式选择框物体")]
	[SerializeField]
	private Button normalRebornBtn;

	[Header("重生模式选择框物体")]
	[SerializeField]
	private Button selectBackBtn;

	private List<talentPoint> talentPointList;

	private List<talentPoint2> talentPoint2List;

	private void Awake()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected O, but got Unknown
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected O, but got Unknown
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Expected O, but got Unknown
		if ((Object)(object)ConfigManager.Instance != (Object)null)
		{
			talentPointList = ConfigManager.Instance.GetAllTalentPoint();
			talentPoint2List = ConfigManager.Instance.GetAllTalentPoint2();
		}
		if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
		{
			SingletonMono<UpgradeManager>.Instance.OnGoldChanged += RefreshTalentRebornTip;
		}
		((UnityEvent)backBtn.onClick).AddListener((UnityAction)delegate
		{
			SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
			((Component)this).gameObject.SetActive(false);
		});
		((UnityEvent)mask.onClick).AddListener((UnityAction)delegate
		{
			SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
			((Component)this).gameObject.SetActive(false);
		});
		((UnityEvent)rebornBtn.onClick).AddListener((UnityAction)delegate
		{
			SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
			if ((Object)(object)rebornSelectPanel != (Object)null)
			{
				rebornSelectPanel.SetActive(true);
			}
		});
		((UnityEvent)adRebornBtn.onClick).AddListener((UnityAction)delegate
		{
			FakeVideoManager.Instance.ShowAd(delegate
			{
				RebornTalent(isNormalReborn: false);
				if ((Object)(object)rebornSelectPanel != (Object)null)
				{
					rebornSelectPanel.SetActive(false);
				}
			}, delegate
			{
			}, "重生保留等级");
		});
		((UnityEvent)normalRebornBtn.onClick).AddListener((UnityAction)delegate
		{
			RebornTalent(isNormalReborn: true);
			if ((Object)(object)rebornSelectPanel != (Object)null)
			{
				rebornSelectPanel.SetActive(false);
			}
		});
		((UnityEvent)selectBackBtn.onClick).AddListener((UnityAction)delegate
		{
			if ((Object)(object)rebornSelectPanel != (Object)null)
			{
				rebornSelectPanel.SetActive(false);
			}
		});
		GameObjectClickListener gameObjectClickListener = GameObjectClickListener.Get(((Component)rebornBtn).gameObject);
		gameObjectClickListener.onClick = (GameObjectClickListener.VoidDelegate)Delegate.Combine(gameObjectClickListener.onClick, (GameObjectClickListener.VoidDelegate)delegate
		{
			if (!((Selectable)rebornBtn).interactable)
			{
				BroadcastManager.ShowPopUpTip("金钱未达到重生目标");
			}
		});
	}

	public void RefreshTalentRebornTip(double gold)
	{
		((Component)canRebornTxt).gameObject.SetActive(false);
		((Component)noRebornTxt).gameObject.SetActive(false);
		if (!((Object)(object)GameDataManager.Instance != (Object)null) || !((Object)(object)ConfigManager.Instance != (Object)null))
		{
			return;
		}
		int num = GameDataManager.Instance?.GetCurrentWorldId() ?? 1;
		TalentSceneType sceneType = ((num != 1) ? TalentSceneType.DiceSceneTalent : TalentSceneType.CoinSceneTalent);
		double totalGold = SingletonMono<UpgradeManager>.Instance.TotalGold;
		double num2 = 0.0;
		int num3 = CalculateLevel(sceneType);
		int rebornCurrentLevel = GameDataManager.Instance.GetRebornCurrentLevel(sceneType);
		int num4 = 0;
		num4 = ((num3 <= rebornCurrentLevel) ? rebornCurrentLevel : num3);
		int num5 = num4 + 1;
		int num6 = ((num != 2) ? (talentPointList?.Count ?? 0) : (talentPoint2List?.Count ?? 0));
		if (num6 <= 0)
		{
			Debug.LogError((object)"[TalentRebornTip] 配置表为空，无法刷新重生面板");
			return;
		}
		if (num5 >= num6 - 1)
		{
			num5 = num6 - 1;
		}
		else if (num5 <= 0)
		{
			num5 = 0;
		}
		num2 = ((num == 2) ? (talentPoint2List?[num5]?.TalentName).GetValueOrDefault() : (talentPointList?[num5]?.TalentName).GetValueOrDefault());
		((TMP_Text)progresstxt).text = $"<color=#d55319>{NumberFormatter.FormatNumber(totalGold)}</color>/{NumberFormatter.FormatNumber(num2)}";
		if (totalGold < 0.0)
		{
			processing.fillAmount = 1f;
		}
		else
		{
			processing.fillAmount = (float)(totalGold / num2);
		}
		int num7 = ((num == 2) ? int.Parse(talentPoint2List?[num5].TalentDesc ?? "0") : int.Parse(talentPointList[num5].TalentDesc));
		if (TalentDataManager.Instance != null)
		{
			switch (num)
			{
			case 1:
				num7 += TalentDataManager.Instance.RebornReward();
				break;
			case 2:
				num7 += (int)(TalentDataManager.Instance?.GetDiceTalentBuff(310) ?? 0f);
				break;
			}
		}
		((TMP_Text)addpointtxt1).text = $"额外+{num7}";
		((TMP_Text)addpointtxt2).text = CalculateGetPoint(sceneType).ToString();
		float gainCount = GetGainCount(num4, sceneType);
		int num8 = CalculateGetPoint(sceneType);
		if (num8 > 0)
		{
			((TMP_Text)GainText).text = "全局收益*" + NumberFormatter.FormatNumber(gainCount) + "倍";
		}
		else if (num8 <= 0)
		{
			((TMP_Text)GainText).text = "未达到全局收益条件";
		}
		if (num3 < 0)
		{
			((Component)noRebornTxt).gameObject.SetActive(true);
		}
		else
		{
			((Component)canRebornTxt).gameObject.SetActive(true);
		}
		int num9 = CalculateGetPoint();
		((Selectable)rebornBtn).interactable = num9 > 0;
		if (num5 >= num6 - 1)
		{
			if (num9 <= 0)
			{
				((Component)rebornBtn).gameObject.SetActive(false);
				((TMP_Text)GainText).text = "重生已达上限";
			}
			else if (num9 > 0)
			{
				((Component)rebornBtn).gameObject.SetActive(true);
			}
		}
		else if (num5 <= 0)
		{
			((Component)rebornBtn).gameObject.SetActive(true);
		}
	}

	public int CalculateLevel()
	{
		TalentSceneType sceneType = (((GameDataManager.Instance?.GetCurrentWorldId() ?? 1) != 1) ? TalentSceneType.DiceSceneTalent : TalentSceneType.CoinSceneTalent);
		return CalculateLevel(sceneType);
	}

	public int CalculateLevel(TalentSceneType sceneType)
	{
		double totalGold = SingletonMono<UpgradeManager>.Instance.TotalGold;
		if (sceneType == TalentSceneType.DiceSceneTalent)
		{
			if (talentPoint2List == null)
			{
				return -1;
			}
			for (int i = 0; i < talentPoint2List.Count; i++)
			{
				double num = talentPoint2List[i].TalentName;
				if (totalGold < 0.0)
				{
					return talentPoint2List.Count - 1;
				}
				if (totalGold < num)
				{
					return i - 1;
				}
			}
			return talentPoint2List.Count - 1;
		}
		if (talentPointList == null)
		{
			return -1;
		}
		for (int j = 0; j < talentPointList.Count; j++)
		{
			double num2 = talentPointList[j].TalentName;
			if (totalGold < 0.0)
			{
				return talentPointList.Count - 1;
			}
			if (totalGold < num2)
			{
				return j - 1;
			}
		}
		return talentPointList.Count - 1;
	}

	public int CalculateGetPoint()
	{
		TalentSceneType sceneType = (((GameDataManager.Instance?.GetCurrentWorldId() ?? 1) != 1) ? TalentSceneType.DiceSceneTalent : TalentSceneType.CoinSceneTalent);
		return CalculateGetPoint(sceneType);
	}

	public int CalculateGetPoint(TalentSceneType sceneType)
	{
		int num = 0;
		int num2 = CalculateLevel(sceneType);
		int rebornCurrentLevel = GameDataManager.Instance.GetRebornCurrentLevel(sceneType);
		int num3 = ((sceneType != TalentSceneType.DiceSceneTalent) ? (talentPointList?.Count ?? 0) : (talentPoint2List?.Count ?? 0));
		if (num2 < num3 && num2 > rebornCurrentLevel)
		{
			int num4 = 0;
			for (int i = rebornCurrentLevel + 1; i <= num2; i++)
			{
				if (sceneType == TalentSceneType.DiceSceneTalent && talentPoint2List != null)
				{
					num += int.Parse(talentPoint2List[i].TalentDesc);
				}
				else if (talentPointList != null)
				{
					num += int.Parse(talentPointList[i].TalentDesc);
				}
				if (TalentDataManager.Instance != null)
				{
					num4 = ((sceneType != TalentSceneType.CoinSceneTalent) ? (num4 + (int)TalentDataManager.Instance.GetDiceTalentBuff(310)) : (num4 + TalentDataManager.Instance.RebornReward()));
				}
			}
			num += num4;
		}
		return num;
	}

	public void RebornTalent(bool isNormalReborn)
	{
		int num = GameDataManager.Instance?.GetCurrentWorldId() ?? 1;
		TalentSceneType sceneType = ((num != 1) ? TalentSceneType.DiceSceneTalent : TalentSceneType.CoinSceneTalent);
		int num2 = CalculateGetPoint(sceneType);
		if (num2 <= 0)
		{
			return;
		}
		int level = CalculateLevel(sceneType);
		GameDataManager.Instance.SetRebornCurrentLevel(level, sceneType);
		SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/upgrade.mp3");
		if ((Object)(object)GameDataManager.Instance != (Object)null && (Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
		{
			GameDataManager.Instance.ChangeTalentPoint(isAdd: true, num2, sceneType);
			double num3 = 0.0;
			double totalGold = SingletonMono<UpgradeManager>.Instance.TotalGold;
			if (isNormalReborn)
			{
				RestUpgradeData(sceneType);
			}
			UpgradePanel.Instance.OnButtonClicked(1);
			switch (num)
			{
			case 1:
			{
				TalentManager talentManager = TalentDataManager.Instance?.GetCurrentTalentManager();
				if (!((Object)(object)talentManager != (Object)null) || !((Object)(object)talentManager.talentPanels[2] != (Object)null))
				{
					break;
				}
				TalentCardItem talentUnlock = talentManager.talentPanels[2].GetTalentUnlock(304);
				if ((Object)(object)talentUnlock != (Object)null)
				{
					int upgradeLevelCurrent = talentUnlock.upgradeLevelCurrent;
					if (talentUnlock.isUnlock && upgradeLevelCurrent > 0)
					{
						num3 = totalGold * (double)talentUnlock.upgradeResult[upgradeLevelCurrent - 1];
					}
				}
				break;
			}
			case 2:
				num3 = (totalGold * (double?)TalentDataManager.Instance?.GetDiceTalentBuff(311)).GetValueOrDefault();
				break;
			}
			double gold = num3;
			SingletonMono<UpgradeManager>.Instance.SetGold(gold);
			UIManager.Instance?.PlayCutsceneTransition();
		}
		((Component)this).gameObject.SetActive(false);
		DownItem downItem = UIManager.Instance.GetDownItem();
		if ((Object)(object)downItem != (Object)null)
		{
			downItem.RefreshChallenge();
		}
	}

	public void RestUpgradeData()
	{
		TalentSceneType sceneType = (((GameDataManager.Instance?.GetCurrentWorldId() ?? 1) != 1) ? TalentSceneType.DiceSceneTalent : TalentSceneType.CoinSceneTalent);
		RestUpgradeData(sceneType);
	}

	public void RestUpgradeData(TalentSceneType sceneType)
	{
		IMainSceneView mainSceneView = (((Object)(object)UIManager.Instance != (Object)null) ? UIManager.Instance.ActiveMainScene : null);
		if (mainSceneView == null)
		{
			Debug.LogWarning((object)"[TalentRebornTip] Active MainScene is null, skip scene cleanup.");
		}
		else
		{
			mainSceneView.ClearRuntimeContent();
		}
		SingletonMono<UpgradeManager>.Instance.ResetData();
		SingletonMono<UpgradeManager>.Instance.LoadData();
		SingletonMono<UpgradeManager>.Instance.NotifyCoinCountChanged();
		SingletonMono<UpgradeManager>.Instance.AddGold(0.0);
		GameDataManager.Instance.ResetFacilityData();
		mainSceneView?.InitializeCoins();
		mainSceneView?.InitializeRoles();
		GameDataManager.Instance.ResetHelperADLevel();
		GameDataManager.Instance.SaveGameData();
	}

	public float GetGainCount(int currentlevel, TalentSceneType sceneType)
	{
		int num = currentlevel;
		float num2 = 1f;
		if (num <= 0)
		{
			num = 0;
		}
		if (sceneType == TalentSceneType.DiceSceneTalent)
		{
			if (talentPoint2List == null)
			{
				return 1f;
			}
			if (num >= talentPoint2List.Count - 1)
			{
				num = talentPoint2List.Count - 1;
			}
			for (int i = 0; i <= num; i++)
			{
				num2 *= talentPoint2List[i].RebornBuff;
			}
		}
		else
		{
			if (talentPointList == null)
			{
				return 1f;
			}
			if (num >= talentPointList.Count - 1)
			{
				num = talentPointList.Count - 1;
			}
			for (int j = 0; j <= num; j++)
			{
				num2 *= talentPointList[j].RebornBuff;
			}
		}
		return num2;
	}
}

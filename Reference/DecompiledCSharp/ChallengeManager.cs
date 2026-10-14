using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using cfg;

public class ChallengeManager : MonoBehaviour
{
	[Header("挑战进度条")]
	[SerializeField]
	private Image progress;

	[Header("进度条跟随")]
	[SerializeField]
	private RectTransform follow;

	[Header("目前收集金钱提示文本")]
	[SerializeField]
	private TextMeshProUGUI sum;

	[Header("剩余时间文本显示")]
	[SerializeField]
	private TextMeshProUGUI time;

	private double collectCoin;

	private double targetCoin;

	private float leftTime;

	private bool isOpenChallenge;

	private Vector2 position;

	private List<int> rewardType = new List<int>();

	private List<double> rewardCount = new List<double>();

	private DownItem downItem;

	public static ChallengeManager Instance;

	private void Awake()
	{
		if ((Object)(object)Instance != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
		else
		{
			Instance = this;
		}
		if ((Object)(object)UIManager.Instance != (Object)null)
		{
			SingletonMono<UpgradeManager>.Instance.CurrentGoldGain += ChallengeAddCoin;
		}
		isOpenChallenge = false;
	}

	private void Start()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)UIManager.Instance != (Object)null)
		{
			downItem = UIManager.Instance.GetDownItem();
		}
		position = Vector2.zero;
	}

	private void Update()
	{
		Follow();
		TimeCalculate();
	}

	public void Init()
	{
		collectCoin = 0.0;
		isOpenChallenge = true;
		int challengeId = GameDataManager.Instance.GetChallengeId();
		limitMission limitMission = ConfigManager.Instance.GetLimitMission(challengeId);
		targetCoin = 0.0;
		leftTime = 0f;
		if (limitMission != null)
		{
			targetCoin = (double)(limitMission.LimitGoalFactor * ((float)limitMission.LimitTIme / 60f)) * SingletonMono<UpgradeManager>.Instance.GetAverageGoldPerMinuteFromLast30Seconds();
			leftTime = limitMission.LimitTIme;
			if (TalentDataManager.Instance != null)
			{
				float num = 1f;
				switch (GameDataManager.Instance.GetCurrentWorldId())
				{
				case 1:
					num = TalentDataManager.Instance.SpacetimeWrap();
					break;
				case 2:
					num = TalentDataManager.Instance.GetDiceTalentBuff(112);
					break;
				}
				leftTime *= num;
			}
		}
		RefrshUI();
	}

	private void Follow()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		position.x = -360f + 720f * progress.fillAmount / 1f;
		follow.anchoredPosition = position;
	}

	private void TimeCalculate()
	{
		if (leftTime > 0f)
		{
			leftTime -= Time.deltaTime;
		}
		else if (leftTime <= 0f)
		{
			IsCompliteChallenge(isComplite: false);
			isOpenChallenge = false;
		}
		int num = 0;
		int num2 = 0;
		if (leftTime >= 60f)
		{
			num = (int)leftTime / 60;
			num2 = (int)leftTime % 60;
		}
		else
		{
			num = 0;
			num2 = (int)leftTime;
		}
		((TMP_Text)time).text = num.ToString("00") + ":" + num2.ToString("00");
	}

	public void ChallengeAddCoin(double count)
	{
		if (isOpenChallenge && ((Component)this).gameObject.activeSelf)
		{
			collectCoin += count;
			if (collectCoin >= targetCoin)
			{
				collectCoin = targetCoin;
				IsCompliteChallenge(isComplite: true);
			}
			RefrshUI();
		}
	}

	public void RefrshUI()
	{
		progress.fillAmount = (float)(collectCoin / targetCoin);
		((TMP_Text)sum).text = $"<color=#40FE84>{NumberFormatter.FormatNumber(collectCoin)}</color>" + "/" + NumberFormatter.FormatNumber(targetCoin);
	}

	public void IsCompliteChallenge(bool isComplite)
	{
		if (isComplite)
		{
			if ((Object)(object)GameDataManager.Instance != (Object)null && (Object)(object)ConfigManager.Instance != (Object)null)
			{
				List<limitMission> allLimitMission = ConfigManager.Instance.GetAllLimitMission();
				int num = GameDataManager.Instance.GetChallengeId();
				int num2 = GameDataManager.Instance.GetCurrentMainMissionLevel();
				for (int i = 0; i < allLimitMission.Count; i++)
				{
					if (allLimitMission[i].LimitMissionCondition == (double)num2)
					{
						num2 = allLimitMission[i].Id;
						break;
					}
				}
				for (int j = 0; j < allLimitMission.Count; j++)
				{
					if (allLimitMission[j].Id != num)
					{
						continue;
					}
					double num3 = 0.0;
					num3 = ((allLimitMission[j].RewardType != 1) ? allLimitMission[j].RewardNum : (allLimitMission[j].RewardNum * SingletonMono<UpgradeManager>.Instance.GetAverageGoldPerMinuteFromLast30Seconds()));
					if (TalentDataManager.Instance != null)
					{
						int currentWorldId = GameDataManager.Instance.GetCurrentWorldId();
						float num4 = 0f;
						switch (currentWorldId)
						{
						case 1:
							num4 = TalentDataManager.Instance.ChallengeMaster();
							break;
						case 2:
							num4 = TalentDataManager.Instance.GetDiceTalentBuff(113);
							break;
						}
						if (Random.value <= num4)
						{
							num3 *= 2.0;
						}
					}
					GetSuipian(allLimitMission[j].RewardType, num3);
					downItem.ChallengeVictory(rewardType, rewardCount);
					int num5 = j + 1;
					int num6 = num;
					int num7 = 0;
					if (num5 < allLimitMission.Count)
					{
						num6 = allLimitMission[num5].Id;
					}
					else if (num5 >= allLimitMission.Count)
					{
						num = -1;
						num6 = -1;
					}
					if (num < 0)
					{
						GameDataManager.Instance.SetChallengeId(num);
						break;
					}
					num7 = CalculteLimitLevel();
					num = ((num7 > num6) ? num7 : num6);
					num = ((num > num2) ? num : num2);
					if (num >= allLimitMission[allLimitMission.Count - 1].Id)
					{
						num = allLimitMission[allLimitMission.Count - 1].Id;
					}
					GameDataManager.Instance.SetChallengeId(num);
					break;
				}
			}
			if ((Object)(object)UIManager.Instance != (Object)null)
			{
				downItem.CloseChallenge();
				downItem.SetChallengeBtnActive(isOpen: false);
			}
		}
		else
		{
			if ((Object)(object)UIManager.Instance != (Object)null)
			{
				downItem.CloseChallenge();
			}
			downItem.ChallengeFail();
		}
	}

	public int CalculteLimitLevel()
	{
		double totalGold = SingletonMono<UpgradeManager>.Instance.TotalGold;
		List<mainMission> allMainMission = ConfigManager.Instance.GetAllMainMission();
		int id = allMainMission[allMainMission.Count - 1].Id;
		List<limitMission> allLimitMission = ConfigManager.Instance.GetAllLimitMission();
		int id2 = allLimitMission[allLimitMission.Count - 1].Id;
		if ((Object)(object)ConfigManager.Instance != (Object)null)
		{
			for (int i = 0; i < allMainMission.Count; i++)
			{
				if (allMainMission[i].MainMissionNum > totalGold)
				{
					id = allMainMission[i].Id;
					break;
				}
			}
			List<limitMission> allLimitMission2 = ConfigManager.Instance.GetAllLimitMission();
			for (int j = 0; j < allLimitMission2.Count; j++)
			{
				if ((double)id < allLimitMission2[j].LimitMissionCondition)
				{
					id2 = allLimitMission2[j].Id;
					break;
				}
			}
		}
		return id2;
	}

	public void GetSuipian(int Id, double count)
	{
		rewardType.Clear();
		rewardCount.Clear();
		if (Id == 3 || Id == 4 || Id == 5)
		{
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			for (int i = 0; (double)i < count; i++)
			{
				int key = ConfigManager.Instance.TransferSuipianRealId(Id);
				if (dictionary.ContainsKey(key))
				{
					dictionary[key]++;
				}
				else
				{
					dictionary[key] = 1;
				}
			}
			{
				foreach (KeyValuePair<int, int> item in dictionary)
				{
					rewardType.Add(item.Key);
					rewardCount.Add(item.Value);
				}
				return;
			}
		}
		rewardType.Add(Id);
		rewardCount.Add(count);
	}

	private void OnDestroy()
	{
		if ((Object)(object)UIManager.Instance != (Object)null)
		{
			SingletonMono<UpgradeManager>.Instance.CurrentGoldGain -= ChallengeAddCoin;
		}
	}
}

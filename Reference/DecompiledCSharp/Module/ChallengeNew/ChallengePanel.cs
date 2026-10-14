using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Module.Http;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using cfg;

namespace Module.ChallengeNew;

public class ChallengePanel : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static UnityAction _003C_003E9__23_0;

		public static Comparison<ChallengeHelperInfo> _003C_003E9__40_0;

		internal void _003CStart_003Eb__23_0()
		{
			SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
			if ((Object)(object)SoundManager.Instance != (Object)null)
			{
				SoundManager.Instance.EnableCoinSound = false;
			}
			AuctionDataManager.Instance?.SetAuctionOpen(isOpen: true);
			AuctionDataManager.Instance?.OpenAuctionObj();
		}

		internal int _003COnZuDuiBtnClicked_003Eb__40_0(ChallengeHelperInfo a, ChallengeHelperInfo b)
		{
			return b.HelperId.CompareTo(a.HelperId);
		}
	}

	[Header("关卡选择界面")]
	[SerializeField]
	private GameObject LevelGrid;

	[Header("帮手选择界面")]
	[SerializeField]
	private GameObject helperSelect;

	[Header("关卡详情")]
	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private Image rewardIcon;

	[SerializeField]
	private TextMeshProUGUI needCoin;

	[SerializeField]
	private Button challengeBtn;

	[Header("帮手列表")]
	[SerializeField]
	private Transform helperCont;

	[Header("进入挑战按键")]
	[SerializeField]
	private Button playBtn;

	[Header("挑战奖励")]
	[SerializeField]
	private TextMeshProUGUI rewardTxt;

	[Header("帮手上场数量")]
	[SerializeField]
	private TextMeshProUGUI playCount;

	[Header("一键组队")]
	[SerializeField]
	private Button zuduiBtn;

	[Header("组队界面返回按键")]
	[SerializeField]
	private Button selectBackBtn;

	private challenge _currentChallengeConfig;

	private GameObject challengeScene;

	private List<ChallengeHelperInfo> _helperItems = new List<ChallengeHelperInfo>();

	private Dictionary<int, int> _battleHelperCounts = new Dictionary<int, int>();

	[Header("拍卖会入口显示物体")]
	[SerializeField]
	private GameObject auctionEnter;

	[Header("拍卖会等级种类显示文本")]
	[SerializeField]
	private Image auctionLeveImg;

	[Header("拍卖会物品奖励显示文本")]
	[SerializeField]
	private TextMeshProUGUI auctionrewardTxt;

	[Header("拍卖会入口按键")]
	[SerializeField]
	private Button auctionEnterBtn;

	[Header("拍卖会需要门票数量显示文本")]
	[SerializeField]
	private TextMeshProUGUI ticketTxt;

	[Header("拍卖会金钱文本")]
	[SerializeField]
	private TextMeshProUGUI coinTxt;

	private void OnEnable()
	{
		OpenAuction();
	}

	private void Start()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Expected O, but got Unknown
		ButtonClickedEvent onClick = auctionEnterBtn.onClick;
		object obj = _003C_003Ec._003C_003E9__23_0;
		if (obj == null)
		{
			UnityAction val = delegate
			{
				SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
				if ((Object)(object)SoundManager.Instance != (Object)null)
				{
					SoundManager.Instance.EnableCoinSound = false;
				}
				AuctionDataManager.Instance?.SetAuctionOpen(isOpen: true);
				AuctionDataManager.Instance?.OpenAuctionObj();
			};
			_003C_003Ec._003C_003E9__23_0 = val;
			obj = (object)val;
		}
		((UnityEvent)onClick).AddListener((UnityAction)obj);
		if ((Object)(object)challengeBtn != (Object)null)
		{
			((UnityEvent)challengeBtn.onClick).AddListener(new UnityAction(OnChallengeBtnClicked));
		}
		((UnityEvent)selectBackBtn.onClick).AddListener((UnityAction)delegate
		{
			SelectOpenPanel(1);
		});
		((UnityEvent)playBtn.onClick).AddListener((UnityAction)delegate
		{
			if (GetTotalBattleCount() <= 0)
			{
				BroadcastManager.ShowPopUpTip("未配置帮手，请先配置帮手队伍");
			}
			else
			{
				DisplayChallengeMain();
			}
		});
		if ((Object)(object)zuduiBtn != (Object)null)
		{
			((UnityEvent)zuduiBtn.onClick).AddListener(new UnityAction(OnZuDuiBtnClicked));
		}
	}

	public void OpenAuction()
	{
		auctionEnter.SetActive(true);
		RefreshAuctionEnter();
		try
		{
			SingletonMono<HttpManager>.Instance.TrackSwitchUI(null, "panel_ auction");
		}
		catch (Exception)
		{
			throw;
		}
	}

	public void RefreshAuctionEnter()
	{
		int num = AuctionDataManager.Instance?.GetUnlockAuctionId() ?? 0;
		auction auctionConfig = ConfigManager.Instance.GetAuctionConfig(num);
		if (num <= 1)
		{
			num = 1;
		}
		SingletonMono<ResManager>.Instance.LoadSpriteAsync($"Assets/GameRes/icon/auction_title/challenge_pic_title_{num}.png", delegate(Sprite sprite)
		{
			auctionLeveImg.sprite = sprite;
		}, isAtlas: false);
		((TMP_Text)auctionrewardTxt).text = $"{auctionConfig.ItemValueMin}~{auctionConfig.ItemValueMax}";
		((TMP_Text)ticketTxt).text = $"{AuctionDataManager.Instance?.GetTicketCount() ?? 0}";
		((TMP_Text)coinTxt).text = NumberFormatter.FormatNumber(AuctionDataManager.Instance?.GetAuctionCoin() ?? 0.0) ?? "";
	}

	public void OpenChallenge()
	{
		((Component)this).gameObject.SetActive(true);
		RefreshLevelList();
		RefreshHelperList();
	}

	private void InitHelperInfo()
	{
		List<helperRole> allHelperRole = ConfigManager.Instance.GetAllHelperRole();
		List<HelperInfo> helperDataList = GameDataManager.Instance?.GetAllHeperInfo();
		int mainLevel = GameDataManager.Instance?.GetCurrentMainMissionLevel() ?? 0;
		foreach (helperRole config in allHelperRole)
		{
			if (config == null)
			{
				continue;
			}
			SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/NewUI/Challenge/prefabs/helperSelectInfo.prefab", delegate(GameObject prefab)
			{
				if ((Object)(object)prefab != (Object)null)
				{
					ChallengeHelperInfo component = Object.Instantiate<GameObject>(prefab, helperCont).GetComponent<ChallengeHelperInfo>();
					if ((Object)(object)component != (Object)null)
					{
						HelperInfo data = null;
						if (helperDataList != null)
						{
							data = helperDataList.Find((HelperInfo h) => h.helperId == config.Id);
						}
						component.Initialize(config, data, mainLevel);
						component.OnBattleCountChanged = OnHelperBattleCountChanged;
						component.OnCheckCanAdd = () => GetTotalBattleCount() < GetMaxHelperNum();
						_helperItems.Add(component);
					}
				}
			});
		}
	}

	public bool IsActive()
	{
		return ((Component)this).gameObject.activeSelf;
	}

	public void RefreshLevelList()
	{
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			GameDataManager.Instance.GetCurrentMainMissionLevel();
		}
		int challengeId = 0;
		if ((Object)(object)GameDataManager.Instance != (Object)null)
		{
			challengeId = GameDataManager.Instance.GetChallengeLevel();
		}
		UpdateChallengeDetail(challengeId);
	}

	private void UpdateChallengeDetail(int challengeId)
	{
		if ((Object)(object)ConfigManager.Instance == (Object)null)
		{
			return;
		}
		_currentChallengeConfig = ConfigManager.Instance.GetChallengeCofig(challengeId);
		if (_currentChallengeConfig == null)
		{
			Debug.LogWarning((object)$"[ChallengePanel] 未找到挑战关卡配置, id={challengeId}");
			return;
		}
		if ((Object)(object)title != (Object)null)
		{
			((TMP_Text)title).text = $"{GameDataManager.Instance.GetChallengeLevel()}";
		}
		if ((Object)(object)rewardIcon != (Object)null)
		{
			string rewardIconPath = GetRewardIconPath(_currentChallengeConfig.RewardType);
			if (!string.IsNullOrEmpty(rewardIconPath))
			{
				SingletonMono<ResManager>.Instance.LoadSpriteAsync(rewardIconPath, delegate(Sprite sprite)
				{
					if ((Object)(object)sprite != (Object)null)
					{
						rewardIcon.sprite = sprite;
						((Graphic)rewardIcon).SetNativeSize();
					}
				}, isAtlas: false);
			}
			if ((Object)(object)rewardTxt != (Object)null)
			{
				if (_currentChallengeConfig.RewardType == 1)
				{
					((TMP_Text)rewardTxt).text = NumberFormatter.FormatNumber(SingletonMono<UpgradeManager>.Instance.GetAverageGoldPerMinuteFromLast30Seconds() * _currentChallengeConfig.RewardNum);
				}
				else
				{
					((TMP_Text)rewardTxt).text = _currentChallengeConfig.RewardNum.ToString();
				}
			}
		}
		if ((Object)(object)needCoin != (Object)null)
		{
			((TMP_Text)needCoin).text = NumberFormatter.FormatNumber(_currentChallengeConfig.CostMoney);
		}
	}

	private void OnChallengeBtnClicked()
	{
		SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
		if (_currentChallengeConfig == null)
		{
			Debug.LogWarning((object)"[ChallengePanel] 当前没有可用的挑战关卡配置");
			return;
		}
		double totalGold = SingletonMono<UpgradeManager>.Instance.TotalGold;
		double costMoney = _currentChallengeConfig.CostMoney;
		if (totalGold >= costMoney)
		{
			LevelGrid.SetActive(false);
			helperSelect.SetActive(true);
			RefreshHelperList();
			RefreshPlayCountUI();
		}
		else
		{
			BroadcastManager.ShowPopUpTip("金钱不足，不能开启挑战");
		}
	}

	public void RefreshHelperList()
	{
		if ((Object)(object)helperCont == (Object)null)
		{
			Debug.LogWarning((object)"[ChallengePanel] helperCont 或 helperItemPrefab 未赋值");
		}
		else
		{
			if ((Object)(object)ConfigManager.Instance == (Object)null)
			{
				return;
			}
			List<helperRole> helperConfigs = ConfigManager.Instance.GetAllHelperRole();
			List<HelperInfo> list = GameDataManager.Instance?.GetAllHeperInfo();
			int mainMissionLevel = GameDataManager.Instance?.GetCurrentMainMissionLevel() ?? 0;
			int i;
			for (i = 0; i < helperConfigs.Count; i++)
			{
				if (_helperItems.Count <= i)
				{
					continue;
				}
				HelperInfo data = null;
				if (list != null)
				{
					data = list.Find((HelperInfo h) => h.helperId == helperConfigs[i].Id);
				}
				_helperItems[i].Refresh(helperConfigs[i], data, mainMissionLevel);
			}
		}
	}

	private string GetRewardIconPath(int rewardType)
	{
		if (rewardType == 1 || rewardType == 2)
		{
			return Constants.GetMoneyPath(rewardType);
		}
		return Constants.GetSuipianIconPath(rewardType);
	}

	private void OnHelperBattleCountChanged(int helperId, int count)
	{
		_battleHelperCounts[helperId] = count;
		RefreshPlayCountUI();
	}

	private int GetTotalBattleCount()
	{
		int num = 0;
		foreach (KeyValuePair<int, int> battleHelperCount in _battleHelperCounts)
		{
			num += battleHelperCount.Value;
		}
		return num;
	}

	private int GetMaxHelperNum()
	{
		return _currentChallengeConfig?.HelperNum ?? 0;
	}

	private void RefreshPlayCountUI()
	{
		if ((Object)(object)playCount != (Object)null)
		{
			int totalBattleCount = GetTotalBattleCount();
			int maxHelperNum = GetMaxHelperNum();
			((TMP_Text)playCount).text = $"{totalBattleCount}/{maxHelperNum}";
		}
	}

	public Dictionary<int, int> GetBattleHelperCounts()
	{
		return new Dictionary<int, int>(_battleHelperCounts);
	}

	public void ResetAllBattleCounts()
	{
		_battleHelperCounts.Clear();
		foreach (ChallengeHelperInfo helperItem in _helperItems)
		{
			if ((Object)(object)helperItem != (Object)null)
			{
				helperItem.SetBattleCount(0);
			}
		}
		RefreshPlayCountUI();
	}

	private void OnZuDuiBtnClicked()
	{
		SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
		int maxHelperNum = GetMaxHelperNum();
		if (maxHelperNum <= 0)
		{
			return;
		}
		_battleHelperCounts.Clear();
		foreach (ChallengeHelperInfo helperItem in _helperItems)
		{
			if ((Object)(object)helperItem != (Object)null)
			{
				helperItem.SetBattleCount(0);
			}
		}
		List<ChallengeHelperInfo> list = new List<ChallengeHelperInfo>();
		foreach (ChallengeHelperInfo helperItem2 in _helperItems)
		{
			if ((Object)(object)helperItem2 != (Object)null && helperItem2.IsUnlocked && helperItem2.MaxBattleCount > 0)
			{
				list.Add(helperItem2);
			}
		}
		list.Sort((ChallengeHelperInfo a, ChallengeHelperInfo b) => b.HelperId.CompareTo(a.HelperId));
		int num = maxHelperNum;
		foreach (ChallengeHelperInfo item in list)
		{
			if (num <= 0)
			{
				break;
			}
			int num2 = Mathf.Min(item.MaxBattleCount, num);
			item.SetBattleCount(num2);
			_battleHelperCounts[item.HelperId] = num2;
			num -= num2;
		}
		RefreshPlayCountUI();
	}

	private void OnDestroy()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		if ((Object)(object)challengeBtn != (Object)null)
		{
			((UnityEvent)challengeBtn.onClick).RemoveListener(new UnityAction(OnChallengeBtnClicked));
		}
		if ((Object)(object)zuduiBtn != (Object)null)
		{
			((UnityEvent)zuduiBtn.onClick).RemoveListener(new UnityAction(OnZuDuiBtnClicked));
		}
	}

	private void DisplayChallengeMain()
	{
		MonoBehaviour activeMainSceneBehaviour = UIManager.Instance.ActiveMainSceneBehaviour;
		if ((Object)(object)activeMainSceneBehaviour != (Object)null)
		{
			((Component)activeMainSceneBehaviour).gameObject.SetActive(false);
		}
		if ((Object)(object)challengeScene != (Object)null)
		{
			challengeScene.SetActive(true);
			ChallengeMain component = challengeScene.GetComponent<ChallengeMain>();
			if ((Object)(object)component != (Object)null)
			{
				component.ChallengeInit();
			}
		}
		else
		{
			string assetPath = "Assets/GameRes/NewUI/Challenge/prefabs/ChallengeMain.prefab";
			if (SingletonMono<ResManager>.Instance.IsAssetLoaded(assetPath) == 1)
			{
				return;
			}
			SingletonMono<ResManager>.Instance.LoadPrefabAsync(assetPath, delegate(GameObject prefab)
			{
				if ((Object)(object)prefab != (Object)null)
				{
					Transform layerTransform = UIManager.Instance.GetLayerTransform(UILayer.Popup);
					challengeScene = Object.Instantiate<GameObject>(prefab, layerTransform);
					challengeScene.GetComponent<ChallengeMain>().ChallengeInit();
				}
			});
		}
		double costMoney = _currentChallengeConfig.CostMoney;
		SingletonMono<UpgradeManager>.Instance.TrySpendGold(costMoney);
	}

	public void OffChallengeScene()
	{
		if ((Object)(object)challengeScene != (Object)null)
		{
			challengeScene.SetActive(false);
		}
		MonoBehaviour activeMainSceneBehaviour = UIManager.Instance.ActiveMainSceneBehaviour;
		if ((Object)(object)activeMainSceneBehaviour != (Object)null)
		{
			((Component)activeMainSceneBehaviour).gameObject.SetActive(true);
		}
		SelectOpenPanel(1);
	}

	public void SelectOpenPanel(int panelIndex)
	{
		switch (panelIndex)
		{
		case 1:
			LevelGrid.SetActive(true);
			helperSelect.SetActive(false);
			RefreshLevelList();
			break;
		case 2:
			LevelGrid.SetActive(false);
			helperSelect.SetActive(true);
			RefreshHelperList();
			break;
		case 3:
			AuctionDataManager.Instance?.OpenAuctionObj();
			break;
		}
	}
}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CoinSlotSupportDialog : MonoBehaviour
{
	private enum RandomEventState
	{
		None,
		WaitGamble,
		WaitAd,
		WaitLucky
	}

	private enum RandomEventVisualState
	{
		Normal,
		Success,
		Fail
	}

	public static int PendingRandomMoneyEventId;

	[SerializeField]
	private TextMeshProUGUI coinText;

	[SerializeField]
	private Button getButton;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private Button bgButton;

	[SerializeField]
	private DialogType dialogType;

	[SerializeField]
	private TextMeshProUGUI adDescText;

	[SerializeField]
	private TextMeshProUGUI bianlanText;

	[SerializeField]
	private TextMeshProUGUI investText1;

	[SerializeField]
	private TextMeshProUGUI investText2;

	[SerializeField]
	private TextMeshProUGUI investText3;

	[SerializeField]
	private TextMeshProUGUI adGetBackText;

	[SerializeField]
	private RectTransform successbg;

	[SerializeField]
	private RectTransform failbg;

	[SerializeField]
	private RectTransform normalIcon;

	[SerializeField]
	private RectTransform successicon;

	[SerializeField]
	private RectTransform failicon;

	[SerializeField]
	private Button repayButton;

	[SerializeField]
	private Button luckyButton;

	[SerializeField]
	private Image coinIcon;

	private double amount;

	protected Action onGetCoinCallback;

	protected Action onCloseCallback;

	private RandomEventState _randomEventState;

	private double _randomEventStake;

	private bool _randomEventStakeDeducted;

	private double _randomEventPendingPayout;

	private int _randomEventPendingTweenValue;

	private int _randomMoneyEventId;

	private string _investText1Template;

	private string _investText2Template;

	private string _investText3Template;

	private string _adGetBackTextTemplate;

	protected virtual void Awake()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		CacheRandomEventTextTemplates();
		if ((Object)(object)getButton != (Object)null)
		{
			((UnityEvent)getButton.onClick).AddListener(new UnityAction(OnGetButtonClicked));
		}
		if ((Object)(object)repayButton != (Object)null)
		{
			((UnityEvent)repayButton.onClick).AddListener(new UnityAction(OnRepayButtonClicked));
		}
		if ((Object)(object)luckyButton != (Object)null)
		{
			((UnityEvent)luckyButton.onClick).AddListener(new UnityAction(OnLuckyButtonClicked));
		}
		if ((Object)(object)closeButton != (Object)null)
		{
			((UnityEvent)closeButton.onClick).AddListener(new UnityAction(OnCloseButtonClicked));
		}
		if ((Object)(object)bgButton != (Object)null)
		{
			((UnityEvent)bgButton.onClick).AddListener(new UnityAction(OnCloseButtonClicked));
		}
	}

	public virtual void OnOpen(DialogType dialogType, double amount, Action onGetCoin = null, Action onClose = null)
	{
		if ((Object)(object)coinText == (Object)null)
		{
			Debug.LogError((object)"CoinDialog: coinText is not assigned!");
		}
		this.dialogType = dialogType;
		onGetCoinCallback = onGetCoin;
		onCloseCallback = onClose;
		_randomEventState = ((dialogType == DialogType.randomMoney || dialogType == DialogType.randomDiamond) ? RandomEventState.WaitGamble : RandomEventState.None);
		_randomEventStake = 0.0;
		_randomEventStakeDeducted = false;
		_randomEventPendingPayout = 0.0;
		_randomEventPendingTweenValue = 0;
		_randomMoneyEventId = 0;
		if (dialogType == DialogType.randomMoney)
		{
			_randomMoneyEventId = PendingRandomMoneyEventId;
			PendingRandomMoneyEventId = 0;
		}
		if ((Object)(object)repayButton != (Object)null)
		{
			((Component)repayButton).gameObject.SetActive(false);
			((Selectable)repayButton).interactable = true;
		}
		if ((Object)(object)luckyButton != (Object)null)
		{
			((Component)luckyButton).gameObject.SetActive(false);
			((Selectable)luckyButton).interactable = true;
		}
		if ((Object)(object)adGetBackText != (Object)null)
		{
			((Component)adGetBackText).gameObject.SetActive(false);
		}
		if ((Object)(object)getButton != (Object)null)
		{
			((Component)getButton).gameObject.SetActive(true);
			((Selectable)getButton).interactable = true;
		}
		if ((Object)(object)closeButton != (Object)null)
		{
			((Selectable)closeButton).interactable = true;
		}
		if ((Object)(object)bgButton != (Object)null)
		{
			((Selectable)bgButton).interactable = true;
		}
		if (dialogType == DialogType.randomDiamond)
		{
			amount = GetCurrentDiamondStake();
		}
		SetCoinDialogContent("", amount);
		((Component)this).gameObject.SetActive(true);
		if ((Object)(object)adDescText != (Object)null)
		{
			switch (dialogType)
			{
			case DialogType.incomeBuff:
			{
				float commonFactor = ConfigManager.Instance.GetCommonFactor(12);
				((TMP_Text)adDescText).text = $"所有帮手获取金钱<color=#e66e17>+{Mathf.RoundToInt((float)amount * 100f)}%</color>；<br>持续<color=#e66e17>{commonFactor}min</color>";
				break;
			}
			case DialogType.speedBuff:
			{
				float num = ConfigManager.Instance.GetCommonFactor(9);
				if ((GameDataManager.Instance?.GetCurrentWorldId() ?? 1) == 2)
				{
					num += TalentDataManager.Instance?.GetDiceTalentBuff(304) ?? 0f;
				}
				((TMP_Text)adDescText).text = $"所有帮手速度提高<color=#e66e17>{Mathf.RoundToInt((float)amount * 100f)}%</color>；<br>持续<color=#e66e17>{num}min</color>";
				break;
			}
			case DialogType.randomMoney:
				((TMP_Text)adDescText).text = $"点击下注，<color=#e66e17>{Mathf.RoundToInt(Constants.RandomEventWinRate * 100f)}%</color>概率返还<color=#e66e17>2倍</color>；失败可看广告返还赌注";
				break;
			case DialogType.randomDiamond:
				((TMP_Text)adDescText).text = $"点击下注，<color=#e66e17>{Mathf.RoundToInt(Constants.RandomEventWinRate * 100f)}%</color>概率返还<color=#e66e17>2倍</color>；失败可看广告返还赌注";
				break;
			}
		}
		if (dialogType == DialogType.randomMoney || dialogType == DialogType.randomDiamond)
		{
			if ((Object)(object)getButton != (Object)null)
			{
				((Selectable)getButton).interactable = true;
			}
			TextMeshProUGUI obj = bianlanText;
			if (obj != null)
			{
				((Component)obj).gameObject.SetActive(false);
			}
			ApplyInvestTextReplace(amount);
			SetInvestTextsVisible(visible: true);
			if ((Object)(object)adGetBackText != (Object)null)
			{
				((Component)adGetBackText).gameObject.SetActive(false);
			}
			ApplyRandomEventVisualState(RandomEventVisualState.Normal);
		}
		else
		{
			float supportRemainingTime = GameDataManager.Instance.GetSupportRemainingTime(dialogType);
			if (supportRemainingTime > 0f && dialogType != DialogType.gongfu && dialogType != DialogType.dashe)
			{
				((Selectable)getButton).interactable = false;
				TextMeshProUGUI obj2 = bianlanText;
				if (obj2 != null)
				{
					((Component)obj2).gameObject.SetActive(true);
				}
				if ((Object)(object)bianlanText != (Object)null)
				{
					((TMP_Text)bianlanText).text = "过" + FormatTime(Mathf.RoundToInt(supportRemainingTime)) + "后使用";
				}
			}
			else
			{
				((Selectable)getButton).interactable = true;
				TextMeshProUGUI obj3 = bianlanText;
				if (obj3 != null)
				{
					((Component)obj3).gameObject.SetActive(false);
				}
			}
			if ((Object)(object)repayButton != (Object)null)
			{
				((Component)repayButton).gameObject.SetActive(false);
			}
			if ((Object)(object)adGetBackText != (Object)null)
			{
				((Component)adGetBackText).gameObject.SetActive(false);
			}
			SetInvestTextsVisible(visible: false);
			ApplyRandomEventVisualState(RandomEventVisualState.Normal);
		}
		if ((Object)(object)coinIcon != (Object)null)
		{
			string moneyPath = Constants.GetMoneyPath(1);
			SingletonMono<ResManager>.Instance.LoadSpriteAsync(moneyPath, delegate(Sprite sprite)
			{
				coinIcon.sprite = sprite;
			}, isAtlas: false);
		}
	}

	public void SetCoinDialogContent(string title, double amount)
	{
		this.amount = amount;
		if (!((Object)(object)coinText != (Object)null))
		{
			return;
		}
		if (dialogType == DialogType.money)
		{
			double num = ConfigManager.Instance.GetCommonFactor(10);
			double val = SingletonMono<UpgradeManager>.Instance.GetAverageGoldPerMinuteFromLast30Seconds() * num;
			double val2 = 500.0;
			val = Math.Max(val, val2);
			if (TalentDataManager.Instance != null)
			{
				switch (GameDataManager.Instance?.GetCurrentWorldId() ?? 1)
				{
				case 1:
					val *= (double)TalentDataManager.Instance.GetMonopoly();
					break;
				case 2:
					val *= (double)TalentDataManager.Instance.GetDiceTalentBuff(307);
					break;
				}
			}
			((TMP_Text)coinText).text = NumberFormatter.FormatNumber(val);
		}
		else if (dialogType == DialogType.diamond)
		{
			((TMP_Text)coinText).text = NumberFormatter.FormatNumber(amount);
		}
		else if (dialogType == DialogType.talent)
		{
			((TMP_Text)coinText).text = NumberFormatter.FormatNumber(amount);
		}
		else if (dialogType == DialogType.randomMoney)
		{
			((TMP_Text)coinText).text = NumberFormatter.FormatNumber(amount);
		}
		else if (dialogType == DialogType.randomDiamond)
		{
			((TMP_Text)coinText).text = NumberFormatter.FormatNumber(amount);
		}
		else if (dialogType != DialogType.gongfu)
		{
			if (dialogType == DialogType.acutionMoney)
			{
				((TMP_Text)coinText).text = NumberFormatter.FormatNumber(amount);
				return;
			}
			_ = dialogType;
			_ = 7;
		}
	}

	protected virtual void OnGetButtonClicked()
	{
		if (dialogType == DialogType.randomMoney || dialogType == DialogType.randomDiamond)
		{
			HandleRandomEventButtonClicked();
			return;
		}
		string reason = "获取槽位";
		if (dialogType == DialogType.money)
		{
			reason = "大富翁获取金币";
		}
		if (dialogType == DialogType.incomeBuff)
		{
			reason = "赚钱大师广告点";
		}
		if (dialogType == DialogType.speedBuff)
		{
			reason = "鼓舞大师广告点";
		}
		if (dialogType == DialogType.talent)
		{
			reason = "获取天赋广告点";
		}
		if (dialogType == DialogType.diamond)
		{
			reason = "帮手系统获取钻石";
		}
		if (dialogType == DialogType.gongfu)
		{
			reason = "获取功夫大师广告点";
		}
		if (dialogType == DialogType.acutionMoney)
		{
			reason = "获取拍卖行金钱广告点";
		}
		if (dialogType == DialogType.dashe)
		{
			reason = "获取大蛇广告点";
		}
		if (dialogType == DialogType.randomMoney || dialogType == DialogType.randomDiamond)
		{
			reason = "随机投资事件广告";
		}
		if (dialogType == DialogType.acutionMoney)
		{
			reason = "拍卖紫币获得";
		}
		double income = 0.0;
		if (dialogType == DialogType.money)
		{
			float commonFactor = ConfigManager.Instance.GetCommonFactor(10);
			double averageGoldPerMinuteFromLast30Seconds = SingletonMono<UpgradeManager>.Instance.GetAverageGoldPerMinuteFromLast30Seconds();
			float num = 500f;
			income = Math.Max(averageGoldPerMinuteFromLast30Seconds * (double)commonFactor, num);
			if (TalentDataManager.Instance != null)
			{
				switch (GameDataManager.Instance?.GetCurrentWorldId() ?? 1)
				{
				case 1:
					income *= TalentDataManager.Instance.GetMonopoly();
					break;
				case 2:
					income *= TalentDataManager.Instance.GetDiceTalentBuff(307);
					break;
				}
			}
		}
		FakeVideoManager.Instance.ShowAd(delegate
		{
			if (dialogType == DialogType.money)
			{
				SingletonMono<UpgradeManager>.Instance.AddGoldInternal(income);
				SingletonMono<UpgradeManager>.Instance.AddGold(0.0);
			}
			else if (dialogType == DialogType.incomeBuff)
			{
				float commonFactor2 = ConfigManager.Instance.GetCommonFactor(12);
				SingletonMono<BuffManager>.Instance.AddBuff(BuffType.Income, (float)amount, commonFactor2 * 60f);
			}
			else if (dialogType == DialogType.speedBuff)
			{
				float commonFactor3 = ConfigManager.Instance.GetCommonFactor(9);
				SingletonMono<BuffManager>.Instance.AddBuff(BuffType.Speed, (float)amount, commonFactor3 * 60f);
			}
			else if (dialogType == DialogType.diamond)
			{
				GameDataManager.Instance.AddDiamond((long)Math.Round(amount));
			}
			else if (dialogType != DialogType.talent)
			{
				if (dialogType == DialogType.gongfu)
				{
					UIManager.Instance.ActiveMainScene?.AddGongfu();
				}
				else if (dialogType == DialogType.dashe)
				{
					UIManager.Instance.ActiveMainScene?.AddDashe();
				}
				else if (dialogType == DialogType.acutionMoney)
				{
					double num2 = AuctionDataManager.Instance?.GetAuctionCoin() ?? 0.0;
					AuctionDataManager.Instance?.SetAuctionCoin(num2 + amount);
				}
			}
			onGetCoinCallback?.Invoke();
			OnCloseButtonClicked();
		}, delegate
		{
			OnCloseButtonClicked();
			if (dialogType != DialogType.diamond)
			{
				_ = dialogType;
				_ = 4;
			}
		}, reason);
	}

	private void HandleRandomEventButtonClicked()
	{
		if (_randomEventState != RandomEventState.WaitGamble || !TryDeductRandomEventStake())
		{
			return;
		}
		if (Random.value < Constants.RandomEventWinRate)
		{
			if (dialogType == DialogType.randomMoney)
			{
				_randomEventPendingPayout = _randomEventStake * 2.0;
				_randomEventPendingTweenValue = 1;
			}
			else if (dialogType == DialogType.randomDiamond)
			{
				long num = (long)Math.Floor(_randomEventStake);
				_randomEventPendingPayout = ((num <= 4611686018427387903L) ? (num * 2) : long.MaxValue);
				_randomEventPendingTweenValue = 2;
			}
			EnterRandomEventLuckyState();
			return;
		}
		_randomEventState = RandomEventState.WaitAd;
		ApplyRandomEventVisualState(RandomEventVisualState.Fail);
		if ((Object)(object)repayButton != (Object)null)
		{
			((Component)repayButton).gameObject.SetActive(true);
		}
		if ((Object)(object)getButton != (Object)null)
		{
			((Component)getButton).gameObject.SetActive(false);
		}
		if ((Object)(object)adGetBackText != (Object)null)
		{
			((Component)adGetBackText).gameObject.SetActive(true);
			((TMP_Text)adGetBackText).text = (_adGetBackTextTemplate ?? ((TMP_Text)adGetBackText).text).Replace("xx", NumberFormatter.FormatNumber(_randomEventStake));
		}
		SetInvestTextsVisible(visible: false);
		((Selectable)getButton).interactable = false;
	}

	private void EnterRandomEventLuckyState()
	{
		_randomEventState = RandomEventState.WaitLucky;
		ApplyRandomEventVisualState(RandomEventVisualState.Success);
		if ((Object)(object)coinText != (Object)null)
		{
			((TMP_Text)coinText).text = NumberFormatter.FormatNumber(_randomEventPendingPayout);
		}
		if ((Object)(object)bianlanText != (Object)null)
		{
			((Component)bianlanText).gameObject.SetActive(true);
			((TMP_Text)bianlanText).text = "你运气真好,双倍奖励直接奉上";
		}
		if ((Object)(object)getButton != (Object)null)
		{
			((Component)getButton).gameObject.SetActive(false);
		}
		if ((Object)(object)repayButton != (Object)null)
		{
			((Component)repayButton).gameObject.SetActive(false);
		}
		if ((Object)(object)luckyButton != (Object)null)
		{
			((Component)luckyButton).gameObject.SetActive(true);
			((Selectable)luckyButton).interactable = true;
		}
		if ((Object)(object)adGetBackText != (Object)null)
		{
			((Component)adGetBackText).gameObject.SetActive(false);
		}
		SetInvestTextsVisible(visible: false);
		if ((Object)(object)closeButton != (Object)null)
		{
			((Selectable)closeButton).interactable = false;
		}
		if ((Object)(object)bgButton != (Object)null)
		{
			((Selectable)bgButton).interactable = false;
		}
	}

	private void OnLuckyButtonClicked()
	{
		if (_randomEventState == RandomEventState.WaitLucky)
		{
			_randomEventState = RandomEventState.None;
			if ((Object)(object)luckyButton != (Object)null)
			{
				((Selectable)luckyButton).interactable = false;
			}
			if (dialogType == DialogType.randomMoney)
			{
				SingletonMono<UpgradeManager>.Instance.AddGoldInternal(_randomEventPendingPayout);
				SingletonMono<UpgradeManager>.Instance.AddGold(0.0);
			}
			else if (dialogType == DialogType.randomDiamond)
			{
				GameDataManager.Instance.AddDiamond((long)Math.Floor(_randomEventPendingPayout));
			}
			if ((Object)(object)UIManager.Instance != (Object)null && _randomEventPendingTweenValue != 0)
			{
				UIManager.Instance.ShowCoinDimondTween(_randomEventPendingTweenValue);
			}
			onGetCoinCallback?.Invoke();
			OnCloseButtonClicked();
		}
	}

	private void ApplyRandomEventVisualState(RandomEventVisualState state)
	{
		if (dialogType == DialogType.randomMoney || dialogType == DialogType.randomDiamond)
		{
			if ((Object)(object)normalIcon != (Object)null)
			{
				((Component)normalIcon).gameObject.SetActive(state == RandomEventVisualState.Normal);
			}
			if ((Object)(object)successbg != (Object)null)
			{
				((Component)successbg).gameObject.SetActive(state == RandomEventVisualState.Success);
			}
			if ((Object)(object)successicon != (Object)null)
			{
				((Component)successicon).gameObject.SetActive(state == RandomEventVisualState.Success);
			}
			if ((Object)(object)failbg != (Object)null)
			{
				((Component)failbg).gameObject.SetActive(state == RandomEventVisualState.Fail);
			}
			if ((Object)(object)failicon != (Object)null)
			{
				((Component)failicon).gameObject.SetActive(state == RandomEventVisualState.Fail);
			}
		}
	}

	private void OnRepayButtonClicked()
	{
		if (_randomEventState != RandomEventState.WaitAd)
		{
			return;
		}
		string reason = ((dialogType == DialogType.randomMoney) ? "随机事件金币返还" : "随机事件钻石返还");
		if (dialogType == DialogType.randomMoney || dialogType == DialogType.randomDiamond)
		{
			reason = "随机投资事件广告";
		}
		FakeVideoManager.Instance.ShowAd(delegate
		{
			if (dialogType == DialogType.randomMoney)
			{
				SingletonMono<UpgradeManager>.Instance.AddGoldInternal(_randomEventStake);
				SingletonMono<UpgradeManager>.Instance.AddGold(0.0);
			}
			else if (dialogType == DialogType.randomDiamond)
			{
				long count = (long)Math.Floor(_randomEventStake);
				GameDataManager.Instance.AddDiamond(count);
			}
			onGetCoinCallback?.Invoke();
			OnCloseButtonClicked();
		}, delegate
		{
			OnCloseButtonClicked();
		}, reason);
	}

	private double GetCurrentDiamondStake()
	{
		if ((Object)(object)GameDataManager.Instance == (Object)null)
		{
			return 0.0;
		}
		long diamond = GameDataManager.Instance.GetDiamond();
		if (diamond <= 0)
		{
			return 0.0;
		}
		return Math.Min(60.0, Math.Floor((float)diamond * 0.6f));
	}

	private bool TryDeductRandomEventStake()
	{
		if (_randomEventStakeDeducted)
		{
			return true;
		}
		double num = 0.0;
		if (dialogType == DialogType.randomMoney)
		{
			num = amount;
		}
		else if (dialogType == DialogType.randomDiamond)
		{
			num = (amount = GetCurrentDiamondStake());
			SetCoinDialogContent("", amount);
			ApplyInvestTextReplace(amount);
		}
		if (num <= 0.0)
		{
			return false;
		}
		if (dialogType == DialogType.randomMoney)
		{
			if ((Object)(object)SingletonMono<UpgradeManager>.Instance == (Object)null || !SingletonMono<UpgradeManager>.Instance.TrySpendGold(num))
			{
				TextMeshProUGUI obj = bianlanText;
				if (obj != null)
				{
					((Component)obj).gameObject.SetActive(true);
				}
				if ((Object)(object)bianlanText != (Object)null)
				{
					((TMP_Text)bianlanText).text = "金币不足";
				}
				return false;
			}
		}
		else if (dialogType == DialogType.randomDiamond)
		{
			long count = (long)Math.Floor(num);
			if ((Object)(object)GameDataManager.Instance == (Object)null || !GameDataManager.Instance.RemoveDiamond(count))
			{
				TextMeshProUGUI obj2 = bianlanText;
				if (obj2 != null)
				{
					((Component)obj2).gameObject.SetActive(true);
				}
				if ((Object)(object)bianlanText != (Object)null)
				{
					((TMP_Text)bianlanText).text = "钻石不足";
				}
				return false;
			}
		}
		_randomEventStake = num;
		_randomEventStakeDeducted = true;
		return true;
	}

	private void CacheRandomEventTextTemplates()
	{
		_investText1Template = (((Object)(object)investText1 != (Object)null) ? ((TMP_Text)investText1).text : null);
		_investText2Template = (((Object)(object)investText2 != (Object)null) ? ((TMP_Text)investText2).text : null);
		_investText3Template = (((Object)(object)investText3 != (Object)null) ? ((TMP_Text)investText3).text : null);
		_adGetBackTextTemplate = (((Object)(object)adGetBackText != (Object)null) ? ((TMP_Text)adGetBackText).text : null);
	}

	private void ApplyInvestTextReplace(double stake)
	{
		string newValue = NumberFormatter.FormatNumber(stake);
		if (dialogType == DialogType.randomDiamond)
		{
			if ((Object)(object)investText1 != (Object)null)
			{
				((TMP_Text)investText1).text = (_investText1Template ?? ((TMP_Text)investText1).text).Replace("xx", newValue);
			}
		}
		else if (_randomMoneyEventId == 38)
		{
			if ((Object)(object)investText2 != (Object)null)
			{
				((TMP_Text)investText2).text = (_investText2Template ?? ((TMP_Text)investText2).text).Replace("xx", newValue);
			}
		}
		else if (_randomMoneyEventId == 39)
		{
			if ((Object)(object)investText3 != (Object)null)
			{
				((TMP_Text)investText3).text = (_investText3Template ?? ((TMP_Text)investText3).text).Replace("xx", newValue);
			}
		}
		else if ((Object)(object)investText1 != (Object)null)
		{
			((TMP_Text)investText1).text = (_investText1Template ?? ((TMP_Text)investText1).text).Replace("xx", newValue);
		}
	}

	private void SetInvestTextsVisible(bool visible)
	{
		if (dialogType == DialogType.randomDiamond)
		{
			if ((Object)(object)investText1 != (Object)null)
			{
				((Component)investText1).gameObject.SetActive(visible);
			}
			if ((Object)(object)investText2 != (Object)null)
			{
				((Component)investText2).gameObject.SetActive(false);
			}
			if ((Object)(object)investText3 != (Object)null)
			{
				((Component)investText3).gameObject.SetActive(false);
			}
		}
		else if (_randomMoneyEventId == 38)
		{
			if ((Object)(object)investText1 != (Object)null)
			{
				((Component)investText1).gameObject.SetActive(false);
			}
			if ((Object)(object)investText2 != (Object)null)
			{
				((Component)investText2).gameObject.SetActive(visible);
			}
			if ((Object)(object)investText3 != (Object)null)
			{
				((Component)investText3).gameObject.SetActive(false);
			}
		}
		else if (_randomMoneyEventId == 39)
		{
			if ((Object)(object)investText1 != (Object)null)
			{
				((Component)investText1).gameObject.SetActive(false);
			}
			if ((Object)(object)investText2 != (Object)null)
			{
				((Component)investText2).gameObject.SetActive(false);
			}
			if ((Object)(object)investText3 != (Object)null)
			{
				((Component)investText3).gameObject.SetActive(visible);
			}
		}
		else
		{
			if ((Object)(object)investText1 != (Object)null)
			{
				((Component)investText1).gameObject.SetActive(visible);
			}
			if ((Object)(object)investText2 != (Object)null)
			{
				((Component)investText2).gameObject.SetActive(false);
			}
			if ((Object)(object)investText3 != (Object)null)
			{
				((Component)investText3).gameObject.SetActive(false);
			}
		}
	}

	protected virtual void OnCloseButtonClicked()
	{
		SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
		((Component)this).gameObject.SetActive(false);
		onCloseCallback?.Invoke();
	}

	private void Update()
	{
		if (dialogType != DialogType.incomeBuff && dialogType != DialogType.speedBuff && dialogType != DialogType.money && dialogType != DialogType.diamond && dialogType != DialogType.acutionMoney)
		{
			return;
		}
		float supportRemainingTime = GameDataManager.Instance.GetSupportRemainingTime(dialogType);
		if (supportRemainingTime > 0f)
		{
			((Selectable)getButton).interactable = false;
			TextMeshProUGUI obj = bianlanText;
			if (obj != null)
			{
				((Component)obj).gameObject.SetActive(true);
			}
			if ((Object)(object)bianlanText != (Object)null)
			{
				((TMP_Text)bianlanText).text = "过" + FormatTime(Mathf.RoundToInt(supportRemainingTime)) + "后使用";
			}
		}
		else
		{
			((Selectable)getButton).interactable = true;
			TextMeshProUGUI obj2 = bianlanText;
			if (obj2 != null)
			{
				((Component)obj2).gameObject.SetActive(false);
			}
		}
	}

	protected virtual void OnDestroy()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected O, but got Unknown
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Expected O, but got Unknown
		if ((Object)(object)getButton != (Object)null)
		{
			((UnityEvent)getButton.onClick).RemoveListener(new UnityAction(OnGetButtonClicked));
		}
		if ((Object)(object)repayButton != (Object)null)
		{
			((UnityEvent)repayButton.onClick).RemoveListener(new UnityAction(OnRepayButtonClicked));
		}
		if ((Object)(object)luckyButton != (Object)null)
		{
			((UnityEvent)luckyButton.onClick).RemoveListener(new UnityAction(OnLuckyButtonClicked));
		}
		if ((Object)(object)closeButton != (Object)null)
		{
			((UnityEvent)closeButton.onClick).RemoveListener(new UnityAction(OnCloseButtonClicked));
		}
		if ((Object)(object)bgButton != (Object)null)
		{
			((UnityEvent)bgButton.onClick).RemoveListener(new UnityAction(OnCloseButtonClicked));
		}
	}

	private string FormatTime(int totalSeconds)
	{
		int num = totalSeconds / 3600;
		int num2 = totalSeconds % 3600 / 60;
		int num3 = totalSeconds % 60;
		if (num > 0)
		{
			return $"{num:D2}:{num2:D2}:{num3:D2}";
		}
		return $"{num2:D2}:{num3:D2}";
	}
}

using System.Collections.Generic;
using Spine;
using Spine.Unity;
using TMPro;
using UI.Component;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using cfg;

namespace Module.Shop;

public class ShopTurntablePanel : MonoBehaviour
{
	[SerializeField]
	private RectTransform wheelTransform;

	[SerializeField]
	private List<ShopRewardItemView> wheelSlots = new List<ShopRewardItemView>();

	[SerializeField]
	private Button spinAdButton;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private Button button201;

	[SerializeField]
	private Button button202;

	[SerializeField]
	private Button button203;

	[SerializeField]
	private Button button301;

	[SerializeField]
	private Image freeSpinIcon;

	[SerializeField]
	private Image adSpinIcon;

	[SerializeField]
	private Image progressSlider;

	[SerializeField]
	private TextMeshProUGUI progressText;

	private float spinDuration = 4.5f;

	[SerializeField]
	private int spinExtraRounds = 6;

	[SerializeField]
	private AnimationCurve spinCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	private bool spinClockwise = true;

	[SerializeField]
	private float spinAngleOffset;

	[SerializeField]
	private int progressMax = 5;

	private readonly Dictionary<int, shop> _shopById = new Dictionary<int, shop>();

	private readonly List<shop> _wheelConfigs = new List<shop>();

	private bool _isSpinning;

	private float _spinElapsed;

	private float _spinStartAngle;

	private float _spinEndAngle;

	private shop _pendingReward;

	private float _vMax;

	private float _totalSpinDistance;

	private float _overshoot;

	private const int WheelDailySpinCounterId = 990001;

	private const int WheelDailyProgressCounterId = 990002;

	[SerializeField]
	private ChestShakeReminder chestShakeReminder;

	[Header("钻石倒计时")]
	[SerializeField]
	private CountdownTimer diamondTimer;

	[SerializeField]
	private TextMeshProUGUI diamondButtonText;

	[Header("转盘广告倒计时")]
	[SerializeField]
	private CountdownTimer spinAdTimer;

	[SerializeField]
	private TextMeshProUGUI spinAdButtonText;

	[SerializeField]
	private TextMeshProUGUI adDiamondText;

	[SerializeField]
	private TextMeshProUGUI adDiamondText2;

	[SerializeField]
	private TextMeshProUGUI adDiamondText3;

	[SerializeField]
	private Button info50;

	[SerializeField]
	private Button info100;

	[SerializeField]
	private RectTransform infoGroup;

	[SerializeField]
	private TextMeshProUGUI titleTmp1;

	[SerializeField]
	private TextMeshProUGUI titleTmp2;

	[SerializeField]
	private TextMeshProUGUI normalTmp;

	[SerializeField]
	private TextMeshProUGUI seniorTmp;

	[SerializeField]
	private SkeletonGraphic skeletonGraphic_0;

	[SerializeField]
	private SkeletonGraphic skeletonGraphic_1;

	[SerializeField]
	private SkeletonGraphic skeletonGraphic_2;

	private TrackEntryDelegate _onSpinAnimCompleteHandler;

	private int times;

	private void Awake()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected O, but got Unknown
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Expected O, but got Unknown
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Expected O, but got Unknown
		if ((Object)(object)spinAdButton != (Object)null)
		{
			((UnityEvent)spinAdButton.onClick).AddListener(new UnityAction(OnSpinAdClicked));
		}
		if ((Object)(object)button201 != (Object)null)
		{
			((UnityEvent)button201.onClick).AddListener((UnityAction)delegate
			{
				OnShopButtonClicked(201);
			});
		}
		if ((Object)(object)button202 != (Object)null)
		{
			((UnityEvent)button202.onClick).AddListener((UnityAction)delegate
			{
				OnShopButtonClicked(202);
			});
		}
		if ((Object)(object)button203 != (Object)null)
		{
			((UnityEvent)button203.onClick).AddListener((UnityAction)delegate
			{
				OnShopButtonClicked(203);
			});
		}
		if ((Object)(object)button301 != (Object)null)
		{
			((UnityEvent)button301.onClick).AddListener((UnityAction)delegate
			{
				OnShopButtonClicked(301);
			});
		}
		if ((Object)(object)info50 != (Object)null)
		{
			((UnityEvent)info50.onClick).AddListener((UnityAction)delegate
			{
				OnInfoClicked(50);
			});
		}
		if ((Object)(object)info100 != (Object)null)
		{
			((UnityEvent)info100.onClick).AddListener((UnityAction)delegate
			{
				OnInfoClicked(100);
			});
		}
		if ((Object)(object)closeButton != (Object)null)
		{
			((UnityEvent)closeButton.onClick).AddListener(new UnityAction(OnInfoClose));
		}
		((Component)closeButton).gameObject.SetActive(false);
		RectTransform obj = infoGroup;
		if (obj != null)
		{
			((Component)obj).gameObject.SetActive(false);
		}
		SkeletonGraphic obj2 = skeletonGraphic_1;
		if (obj2 != null)
		{
			((Component)obj2).gameObject.SetActive(false);
		}
		if ((Object)(object)spinAdTimer != (Object)null)
		{
			spinAdTimer.OnFinished += OnSpinAdCountdownFinished;
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)spinAdTimer != (Object)null)
		{
			spinAdTimer.OnFinished -= OnSpinAdCountdownFinished;
		}
	}

	private void OnSpinAdCountdownFinished()
	{
		SetInteractable(enabled: true);
	}

	private void OnEnable()
	{
		OnOpen();
	}

	public void OnOpen()
	{
		RefreshConfigs();
		RefreshUI();
	}

	public void ConfigureSpin(float duration, int extraRounds, AnimationCurve curve, bool clockwise, float angleOffset)
	{
		spinDuration = duration;
		spinExtraRounds = extraRounds;
		spinCurve = curve;
		spinClockwise = clockwise;
		spinAngleOffset = angleOffset;
	}

	private void Update()
	{
		if (_isSpinning)
		{
			TickSpin(Time.deltaTime);
		}
	}

	public void Close()
	{
		((Component)this).gameObject.SetActive(false);
	}

	private void RefreshConfigs()
	{
		_shopById.Clear();
		_wheelConfigs.Clear();
		if ((Object)(object)ConfigManager.Instance == (Object)null)
		{
			return;
		}
		List<shop> allShop = ConfigManager.Instance.GetAllShop();
		if (allShop == null)
		{
			return;
		}
		foreach (shop item in allShop)
		{
			if (item != null)
			{
				_shopById[item.Id] = item;
				if (item.Id >= 101 && item.Id <= 110)
				{
					_wheelConfigs.Add(item);
				}
			}
		}
		_wheelConfigs.Sort((shop a, shop b) => a.Id.CompareTo(b.Id));
	}

	private void RefreshUI()
	{
		for (int i = 0; i < wheelSlots.Count; i++)
		{
			if (!((Object)(object)wheelSlots[i] == (Object)null) && i < _wheelConfigs.Count)
			{
				wheelSlots[i].SetConfig(_wheelConfigs[i]);
			}
		}
		shop shopConfig = ConfigManager.Instance.GetShopConfig(201);
		if (shopConfig != null)
		{
			((TMP_Text)adDiamondText).text = shopConfig.Count[0].ToString();
		}
		shopConfig = ConfigManager.Instance.GetShopConfig(202);
		if (shopConfig != null)
		{
			((TMP_Text)adDiamondText2).text = shopConfig.Cost.ToString();
			if ((Object)(object)normalTmp != (Object)null)
			{
				((TMP_Text)normalTmp).text = FormatBoxRewardDescription(shopConfig);
			}
		}
		shopConfig = ConfigManager.Instance.GetShopConfig(203);
		if (shopConfig != null)
		{
			((TMP_Text)adDiamondText3).text = shopConfig.Cost.ToString();
			if ((Object)(object)seniorTmp != (Object)null)
			{
				((TMP_Text)seniorTmp).text = FormatBoxRewardDescription(shopConfig);
			}
		}
		float supportRemainingTime = GameDataManager.Instance.GetSupportRemainingTime(DialogType.turnDiamond);
		if (supportRemainingTime > 0f)
		{
			diamondTimer.StartCountdown(supportRemainingTime);
		}
		else
		{
			diamondTimer.Stop();
		}
		float supportRemainingTime2 = GameDataManager.Instance.GetSupportRemainingTime(DialogType.spinAd);
		if (supportRemainingTime2 > 0f)
		{
			spinAdTimer?.StartCountdown(supportRemainingTime2);
		}
		else
		{
			spinAdTimer?.Stop();
		}
		RefreshProgressUI();
		RefreshSpinButton();
	}

	private void RefreshSpinButton()
	{
		bool flag = ShopDailyLimit.GetCount(990001) == 0;
		if ((Object)(object)freeSpinIcon != (Object)null)
		{
			((Component)freeSpinIcon).gameObject.SetActive(flag);
		}
		if ((Object)(object)adSpinIcon != (Object)null)
		{
			((Component)adSpinIcon).gameObject.SetActive(!flag);
		}
	}

	private void RefreshProgressUI()
	{
		int progress = GetProgress();
		if ((Object)(object)progressSlider != (Object)null)
		{
			progressSlider.fillAmount = Mathf.Clamp01((float)progress / (float)progressMax);
		}
		if ((Object)(object)progressText != (Object)null)
		{
			((TMP_Text)progressText).text = $"{progress}/{progressMax}";
		}
		if ((Object)(object)button301 != (Object)null)
		{
			((Selectable)button301).interactable = progress >= progressMax;
			if (progress >= progressMax)
			{
				chestShakeReminder?.StartShake();
			}
			else
			{
				chestShakeReminder?.StopShake();
			}
		}
	}

	private int GetWheelDailyLimit()
	{
		if (_wheelConfigs.Count == 0)
		{
			return 0;
		}
		return _wheelConfigs[0].TodayLimit;
	}

	private void OnSpinAdClicked()
	{
		if (_isSpinning || GameDataManager.Instance.GetSupportRemainingTime(DialogType.spinAd) > 0f)
		{
			return;
		}
		int limit = GetWheelDailyLimit();
		if (!ShopDailyLimit.CanConsume(990001, limit))
		{
			BroadcastManager.ShowPopUpTip("今日次数已用完");
			return;
		}
		if (ShopDailyLimit.GetCount(990001) == 0 || (Object)(object)FakeVideoManager.Instance == (Object)null)
		{
			if (!ShopDailyLimit.TryConsume(990001, limit))
			{
				BroadcastManager.ShowPopUpTip("今日次数已用完");
				return;
			}
			RefreshSpinButton();
			StartSpin();
			return;
		}
		FakeVideoManager.Instance.ShowAd(delegate
		{
			if (!ShopDailyLimit.TryConsume(990001, limit))
			{
				BroadcastManager.ShowPopUpTip("今日次数已用完");
			}
			else
			{
				RefreshSpinButton();
				float seconds = (float)(int)ConfigManager.Instance.GetCommonFactor(35) * 60f;
				GameDataManager.Instance.RecordSupport(DialogType.spinAd);
				StartSpinAdCountdown(seconds);
				StartSpin();
				if (ShopDailyLimit.CanConsume(990001, limit))
				{
					RefreshUI();
				}
			}
		}, delegate
		{
			BroadcastManager.ShowPopUpTip("广告观看未完成");
		}, "大转盘广告");
	}

	private void StartSpin()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (_wheelConfigs.Count == 0 || (Object)(object)wheelTransform == (Object)null)
		{
			return;
		}
		_pendingReward = PickWheelReward();
		if (_pendingReward == null)
		{
			return;
		}
		int wheelIndex = GetWheelIndex(_pendingReward);
		float targetAngleForIndex = GetTargetAngleForIndex(wheelIndex);
		_spinElapsed = 0f;
		_spinStartAngle = ((Transform)wheelTransform).localEulerAngles.z;
		float num = targetAngleForIndex - _spinStartAngle;
		num %= 360f;
		if (spinClockwise)
		{
			if (num > 0f)
			{
				num -= 360f;
			}
			num = 0f - num;
		}
		else if (num < 0f)
		{
			num += 360f;
		}
		int num2 = Mathf.RoundToInt((4248f - num) / 360f);
		if (num2 < 1)
		{
			num2 = 1;
		}
		_totalSpinDistance = num + (float)num2 * 360f;
		_overshoot = Random.Range(5f, 8f);
		_vMax = (_totalSpinDistance + _overshoot) / 2.95f;
		_spinEndAngle = _spinStartAngle + (spinClockwise ? (0f - _totalSpinDistance) : _totalSpinDistance);
		_isSpinning = true;
		SetInteractable(enabled: false);
		SkeletonGraphic obj = skeletonGraphic_2;
		if (obj != null)
		{
			obj.AnimationState.SetAnimation(0, "rolling", true);
		}
		SkeletonGraphic obj2 = skeletonGraphic_2;
		if (obj2 != null)
		{
			((Component)obj2).gameObject.SetActive(true);
		}
		SkeletonGraphic obj3 = skeletonGraphic_0;
		if (obj3 != null)
		{
			obj3.AnimationState.SetAnimation(0, "rolling", true);
		}
		SkeletonGraphic obj4 = skeletonGraphic_0;
		if (obj4 != null)
		{
			((Component)obj4).gameObject.SetActive(true);
		}
	}

	private void TickSpin(float deltaTime)
	{
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		_spinElapsed += deltaTime;
		float spinElapsed = _spinElapsed;
		float num = 0f;
		float num2 = 0.8f;
		float num3 = 2.5f;
		float num4 = 4.2f;
		float num5 = 4.5f;
		float num6 = 0.4f * _vMax;
		float num7 = num6 + 1.7f * _vMax;
		if (spinElapsed <= num2)
		{
			num = 0.5f * (_vMax / num2) * spinElapsed * spinElapsed;
		}
		else if (spinElapsed <= num3)
		{
			num = num6 + _vMax * (spinElapsed - num2);
		}
		else if (spinElapsed <= num4)
		{
			float num8 = spinElapsed - num3;
			float num9 = num4 - num3;
			num = num7 + _vMax * num8 - 0.5f * (_vMax / num9) * num8 * num8;
		}
		else
		{
			if (!(spinElapsed <= num5))
			{
				num = _totalSpinDistance;
				_isSpinning = false;
				((Transform)wheelTransform).localEulerAngles = new Vector3(0f, 0f, _spinEndAngle);
				SetInteractable(enabled: true);
				OnSpinAnimationFinish();
				return;
			}
			float num10 = spinElapsed - num4;
			float num11 = num5 - num4;
			float num12 = num10 / num11;
			float num13 = Mathf.SmoothStep(1f, 0f, num12);
			num = _totalSpinDistance + _overshoot * num13;
		}
		float num14 = _spinStartAngle + (spinClockwise ? (0f - num) : num);
		((Transform)wheelTransform).localEulerAngles = new Vector3(0f, 0f, num14);
	}

	private void OnSpinAnimationFinish()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		times = 0;
		SkeletonGraphic obj = skeletonGraphic_1;
		if (obj != null)
		{
			obj.AnimationState.SetAnimation(0, "effect", true);
		}
		if (_onSpinAnimCompleteHandler == null)
		{
			_onSpinAnimCompleteHandler = (TrackEntryDelegate)delegate
			{
				times++;
				if (times >= 3)
				{
					SkeletonGraphic obj3 = skeletonGraphic_1;
					if (obj3 != null)
					{
						((Component)obj3).gameObject.SetActive(false);
					}
					OnSpinFinished();
				}
			};
		}
		skeletonGraphic_1.AnimationState.Complete += _onSpinAnimCompleteHandler;
		SkeletonGraphic obj2 = skeletonGraphic_1;
		if (obj2 != null)
		{
			((Component)obj2).gameObject.SetActive(true);
		}
		if ((Object)(object)skeletonGraphic_2 != (Object)null)
		{
			skeletonGraphic_2.AnimationState.SetAnimation(0, "idle", true);
		}
		if ((Object)(object)skeletonGraphic_0 != (Object)null)
		{
			skeletonGraphic_0.AnimationState.SetAnimation(0, "idle", true);
		}
	}

	private void OnSpinFinished()
	{
		if (_pendingReward == null)
		{
			return;
		}
		IncrementProgress();
		List<double> list = ShopRewardCalculator.BuildRewardList(_pendingReward);
		if ((GameDataManager.Instance?.GetCurrentWorldId() ?? 1) == 2)
		{
			double num = TalentDataManager.Instance?.GetDiceTalentBuff(110) ?? 0f;
			if (num > 0.0)
			{
				num = SingletonMono<UpgradeManager>.Instance.GetAverageGoldPerMinuteFromLast30Seconds() * num;
				list.Add(1.0);
				list.Add(num);
			}
		}
		UIManager.Instance.ShowRewardBoxUI(list, isAddItem: true, isSuipian: true, "大转盘双倍奖励");
		_pendingReward = null;
	}

	private void OnShopButtonClicked(int id)
	{
		if (_isSpinning || !_shopById.TryGetValue(id, out var cfg))
		{
			return;
		}
		if (id == 201)
		{
			if (cfg.TodayLimit > 0 && !ShopDailyLimit.CanConsume(id, cfg.TodayLimit))
			{
				BroadcastManager.ShowPopUpTip("今日次数已用完");
				return;
			}
			FakeVideoManager.Instance.ShowAd(delegate
			{
				if (cfg.TodayLimit > 0 && !ShopDailyLimit.TryConsume(id, cfg.TodayLimit))
				{
					BroadcastManager.ShowPopUpTip("今日次数已用完");
				}
				else
				{
					int num = (int)ConfigManager.Instance.GetCommonFactor(36);
					UIManager.Instance.ShowRewardBoxUI(ShopRewardCalculator.BuildRewardList(cfg), isAddItem: true, isSuipian: true, "商城获取钻石双倍");
					float seconds = (float)num * 60f;
					GameDataManager.Instance.RecordSupport(DialogType.turnDiamond);
					StartDiamondCountdown(seconds);
				}
			}, delegate
			{
				BroadcastManager.ShowPopUpTip("广告观看未完成");
			}, "商城获取钻石");
		}
		else if (id == 202 || id == 203)
		{
			if (cfg.TodayLimit > 0 && !ShopDailyLimit.CanConsume(id, cfg.TodayLimit))
			{
				BroadcastManager.ShowPopUpTip("今日次数已用完");
			}
			else if (cfg.Cost > 0 && (!((Object)(object)GameDataManager.Instance != (Object)null) || !GameDataManager.Instance.RemoveDiamond(cfg.Cost)))
			{
				BroadcastManager.ShowPopUpTip("钻石不足");
			}
			else if (cfg.TodayLimit > 0 && !ShopDailyLimit.TryConsume(id, cfg.TodayLimit))
			{
				BroadcastManager.ShowPopUpTip("今日次数已用完");
			}
			else
			{
				UIManager.Instance.ShowRewardBoxUI(ShopRewardCalculator.BuildRewardList(cfg), isAddItem: true, isSuipian: true, $"购买宝箱{id}双倍奖励", noDouble: true);
			}
		}
		else if (id == 301)
		{
			int progress = GetProgress();
			if (ShopDailyLimit.IsUnlimited || progress >= progressMax)
			{
				SetProgress(0);
				RefreshProgressUI();
				UIManager.Instance.ShowRewardBoxUI(ShopRewardCalculator.BuildRewardList(cfg), isAddItem: true, isSuipian: true, "连抽5次双倍奖励");
			}
		}
	}

	private shop PickWheelReward()
	{
		double num = 0.0;
		for (int i = 0; i < _wheelConfigs.Count; i++)
		{
			num += (double)Mathf.Max(0f, _wheelConfigs[i].Rate);
		}
		if (num <= 0.0)
		{
			return _wheelConfigs[Random.Range(0, _wheelConfigs.Count)];
		}
		double num2 = (double)Random.value * num;
		double num3 = 0.0;
		for (int j = 0; j < _wheelConfigs.Count; j++)
		{
			num3 += (double)Mathf.Max(0f, _wheelConfigs[j].Rate);
			if (num2 <= num3)
			{
				return _wheelConfigs[j];
			}
		}
		return _wheelConfigs[_wheelConfigs.Count - 1];
	}

	private int GetWheelIndex(shop cfg)
	{
		for (int i = 0; i < _wheelConfigs.Count; i++)
		{
			if (_wheelConfigs[i].Id == cfg.Id)
			{
				return i;
			}
		}
		return 0;
	}

	private float GetTargetAngleForIndex(int index)
	{
		int num = Mathf.Max(1, _wheelConfigs.Count);
		float num2 = 360f / (float)num;
		return (float)index * num2 + spinAngleOffset;
	}

	private void SetInteractable(bool enabled)
	{
		if ((Object)(object)spinAdButton != (Object)null)
		{
			bool flag = GameDataManager.Instance.GetSupportRemainingTime(DialogType.spinAd) > 0f;
			int wheelDailyLimit = GetWheelDailyLimit();
			bool flag2 = ShopDailyLimit.CanConsume(990001, wheelDailyLimit);
			((Selectable)spinAdButton).interactable = enabled && (!flag || !flag2);
		}
		if ((Object)(object)button201 != (Object)null)
		{
			((Selectable)button201).interactable = enabled;
		}
		if ((Object)(object)button202 != (Object)null)
		{
			((Selectable)button202).interactable = enabled;
		}
		if ((Object)(object)button203 != (Object)null)
		{
			((Selectable)button203).interactable = enabled;
		}
		RefreshProgressUI();
	}

	private int GetProgress()
	{
		return ShopDailyLimit.GetCount(990002);
	}

	private void SetProgress(int progress)
	{
		int progress2 = GetProgress();
		if (progress - progress2 != 0)
		{
			ShopDailyLimit.SetCount(990002, progress);
		}
	}

	private void IncrementProgress()
	{
		int num = GetProgress() + 1;
		if (num > progressMax)
		{
			num = progressMax;
		}
		SetProgress(num);
		RefreshProgressUI();
	}

	public void StartDiamondCountdown(float seconds)
	{
		diamondTimer?.StartCountdown(seconds);
	}

	public void StartSpinAdCountdown(float seconds)
	{
		spinAdTimer?.StartCountdown(seconds);
	}

	private void OnInfoClicked(int id)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		((Component)infoGroup).gameObject.SetActive(true);
		switch (id)
		{
		case 50:
			infoGroup.anchoredPosition = new Vector2(-699f, infoGroup.anchoredPosition.y);
			if ((Object)(object)titleTmp1 != (Object)null)
			{
				((Component)titleTmp1).gameObject.SetActive(true);
			}
			if ((Object)(object)normalTmp != (Object)null)
			{
				((Component)normalTmp).gameObject.SetActive(true);
			}
			if ((Object)(object)titleTmp2 != (Object)null)
			{
				((Component)titleTmp2).gameObject.SetActive(false);
			}
			if ((Object)(object)seniorTmp != (Object)null)
			{
				((Component)seniorTmp).gameObject.SetActive(false);
			}
			break;
		case 100:
			infoGroup.anchoredPosition = new Vector2(-216f, infoGroup.anchoredPosition.y);
			if ((Object)(object)titleTmp1 != (Object)null)
			{
				((Component)titleTmp1).gameObject.SetActive(false);
			}
			if ((Object)(object)normalTmp != (Object)null)
			{
				((Component)normalTmp).gameObject.SetActive(false);
			}
			if ((Object)(object)titleTmp2 != (Object)null)
			{
				((Component)titleTmp2).gameObject.SetActive(true);
			}
			if ((Object)(object)seniorTmp != (Object)null)
			{
				((Component)seniorTmp).gameObject.SetActive(true);
			}
			break;
		}
		((Component)closeButton).transform.SetAsLastSibling();
		((Component)infoGroup).transform.SetAsLastSibling();
		((Component)closeButton).gameObject.SetActive(true);
	}

	private void OnInfoClose()
	{
		((Component)infoGroup).gameObject.SetActive(false);
		((Component)closeButton).gameObject.SetActive(false);
		if ((Object)(object)titleTmp1 != (Object)null)
		{
			((Component)titleTmp1).gameObject.SetActive(false);
		}
		if ((Object)(object)titleTmp2 != (Object)null)
		{
			((Component)titleTmp2).gameObject.SetActive(false);
		}
		if ((Object)(object)normalTmp != (Object)null)
		{
			((Component)normalTmp).gameObject.SetActive(false);
		}
		if ((Object)(object)seniorTmp != (Object)null)
		{
			((Component)seniorTmp).gameObject.SetActive(false);
		}
	}

	private string FormatBoxRewardDescription(shop cfg)
	{
		if (cfg == null || cfg.RewardType == null || cfg.Count == null)
		{
			return "";
		}
		List<string> list = new List<string>();
		for (int i = 0; i < cfg.RewardType.Count && i < cfg.Count.Count; i++)
		{
			int num = cfg.RewardType[i];
			int num2 = cfg.Count[i];
			string text = "";
			switch (num)
			{
			case 3:
				text = $"{num2}个N级碎片";
				break;
			case 4:
				text = $"<color=#e4771e>{num2}个R级碎片</color>";
				break;
			case 5:
				text = $"<color=#9227dd>{num2}个SR碎片</color>";
				break;
			}
			if (!string.IsNullOrEmpty(text))
			{
				list.Add(text);
			}
		}
		return "包含：" + string.Join("和", list);
	}
}

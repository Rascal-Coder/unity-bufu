using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Module.Mission;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using cfg;

namespace Module.UI;

public class GoalReachPanel : MonoBehaviour
{
	[Header("UI 组件引用")]
	[SerializeField]
	private TextMeshProUGUI goalTmp;

	[SerializeField]
	private TextMeshProUGUI countTmp;

	[SerializeField]
	private Image itemIcon;

	[SerializeField]
	private ParticleSystem coinRainEffect;

	[SerializeField]
	private TextMeshProUGUI currentLevel;

	[SerializeField]
	private TextMeshProUGUI nextLevel;

	[Header("称号显示")]
	[SerializeField]
	private Image honor;

	[SerializeField]
	private TextMeshProUGUI honorTxt;

	[Header("领取按钮")]
	[SerializeField]
	private Button confirmBtn;

	[SerializeField]
	private Button adBtn;

	[Header("报纸拍下效果参数")]
	[SerializeField]
	private float fallDuration = 0.5f;

	[SerializeField]
	private Ease fallEase = (Ease)27;

	[SerializeField]
	private float startScale = 1.3f;

	[SerializeField]
	private float startRotation = 15f;

	[SerializeField]
	private float startYOffset = 1200f;

	[SerializeField]
	private float exitDuration = 0.4f;

	[SerializeField]
	private Ease exitEase = (Ease)26;

	[SerializeField]
	private float exitScale = 0.3f;

	[SerializeField]
	private float exitRotation = -10f;

	private Queue<mainMission> _pendingMissions = new Queue<mainMission>();

	private bool _isShowing;

	private bool _isSlidIn;

	private RectTransform _rect;

	private Vector2 _hiddenPos;

	private Vector2 _visiblePos;

	private Action onClose;

	private mainMission _currentMission;

	private bool _isRewardClaimed;

	private int suipianId;

	private void Awake()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		_rect = ((Component)this).GetComponent<RectTransform>();
		if ((Object)(object)_rect == (Object)null)
		{
			Debug.LogError((object)"[GoalReachPanel] RectTransform is null!");
			return;
		}
		Button obj = confirmBtn;
		if (obj != null)
		{
			((UnityEvent)obj.onClick).AddListener(new UnityAction(OnConfirmClicked));
		}
		Button obj2 = adBtn;
		if (obj2 != null)
		{
			((UnityEvent)obj2.onClick).AddListener(new UnityAction(OnAdClicked));
		}
		((Component)this).gameObject.SetActive(false);
	}

	private void Start()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		float num = Screen.width;
		_ = _rect.sizeDelta;
		_hiddenPos = new Vector2(num, _rect.anchoredPosition.y);
		_visiblePos = Vector2.zero;
		_rect.anchoredPosition = _hiddenPos;
	}

	private void OnDestroy()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		Button obj = confirmBtn;
		if (obj != null)
		{
			((UnityEvent)obj.onClick).RemoveListener(new UnityAction(OnConfirmClicked));
		}
		Button obj2 = adBtn;
		if (obj2 != null)
		{
			((UnityEvent)obj2.onClick).RemoveListener(new UnityAction(OnAdClicked));
		}
	}

	private void OnRewardsClaimed(List<mainMission> missions)
	{
		if (missions == null || missions.Count == 0)
		{
			return;
		}
		foreach (mainMission mission in missions)
		{
			_pendingMissions.Enqueue(mission);
		}
		if (!_isShowing)
		{
			ShowNextMission();
		}
	}

	private void ShowNextMission()
	{
		if (_pendingMissions.Count == 0)
		{
			_isShowing = false;
			MissionManager.Instance?.ClearPendingRewards();
			return;
		}
		_isShowing = true;
		mainMission mission = (_currentMission = _pendingMissions.Dequeue());
		_isRewardClaimed = false;
		RefreshContent(mission);
		((Component)this).gameObject.SetActive(true);
		SlideIn();
	}

	private void RefreshContent(mainMission mission)
	{
		if (mission == null)
		{
			return;
		}
		string text = NumberFormatter.FormatNumber(mission.MainMissionNum);
		if ((Object)(object)goalTmp != (Object)null)
		{
			((TMP_Text)goalTmp).text = "恭喜达成<color=#f8642a>" + text + "</color>小目标！";
		}
		if ((Object)(object)countTmp != (Object)null)
		{
			((TMP_Text)countTmp).text = NumberFormatter.FormatNumber(mission.RewardNum);
		}
		if ((Object)(object)ConfigManager.Instance != (Object)null)
		{
			suipianId = ConfigManager.Instance.TransferSuipianRealId(mission.RewardType);
		}
		int num = (((Object)(object)MissionManager.Instance != (Object)null) ? MissionManager.Instance.GetMissionLevel(mission) : 0);
		((TMP_Text)currentLevel).text = $"{num}";
		((TMP_Text)nextLevel).text = $"{num + 1}";
		string text2 = "";
		if (mission.RewardType == 1 || mission.RewardType == 2 || mission.RewardType == 6)
		{
			text2 = GetSpritePath(mission.RewardType);
			SingletonMono<ResManager>.Instance.LoadSpriteAsync(text2, delegate(Sprite sprite)
			{
				if ((Object)(object)sprite != (Object)null)
				{
					itemIcon.sprite = sprite;
					((Graphic)itemIcon).SetNativeSize();
				}
			}, isAtlas: false);
		}
		else
		{
			text2 = GetSpritePath(suipianId);
			SingletonMono<ResManager>.Instance.LoadSpriteAsync(text2, delegate(Sprite sprite)
			{
				if ((Object)(object)sprite != (Object)null)
				{
					itemIcon.sprite = sprite;
					((Graphic)itemIcon).SetNativeSize();
				}
			});
		}
		List<mainMission> allMissions = MissionManager.Instance.GetAllMissions();
		int num2 = Mathf.Clamp(num + 1, 1, allMissions.Count);
		for (int num3 = 0; num3 < allMissions.Count; num3++)
		{
			if (num3 + 1 != num2)
			{
				continue;
			}
			if ((Object)(object)honorTxt != (Object)null)
			{
				((TMP_Text)honorTxt).text = allMissions[num3].TitleName;
			}
			int picId = allMissions[num3].PicId;
			if (!((Object)(object)honor != (Object)null))
			{
				break;
			}
			SingletonMono<ResManager>.Instance.LoadSpriteAsync($"Assets/GameRes/honor/{picId}.png", delegate(Sprite sprite)
			{
				if ((Object)(object)sprite != (Object)null && (Object)(object)honor != (Object)null)
				{
					honor.sprite = sprite;
				}
			}, isAtlas: false);
			break;
		}
	}

	private void SlideIn()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Expected O, but got Unknown
		_isSlidIn = false;
		_rect.anchoredPosition = _visiblePos + new Vector2(0f, startYOffset);
		((Transform)_rect).localScale = Vector3.one * startScale;
		((Transform)_rect).localRotation = Quaternion.Euler(0f, 0f, startRotation);
		TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(_rect.DOAnchorPos(_visiblePos, fallDuration), fallEase);
		TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale((Transform)(object)_rect, Vector3.one, fallDuration), fallEase);
		TweenSettingsExtensions.OnComplete<TweenerCore<Quaternion, Vector3, QuaternionOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Quaternion, Vector3, QuaternionOptions>>(ShortcutExtensions.DOLocalRotate((Transform)(object)_rect, Vector3.zero, fallDuration, (RotateMode)0), fallEase), (TweenCallback)delegate
		{
			_isSlidIn = true;
			PlayCoinRainEffect();
		});
	}

	private void SlideOut()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		_isSlidIn = false;
		StopCoinRainEffect();
		TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale((Transform)(object)_rect, Vector3.one * exitScale, exitDuration), exitEase);
		TweenSettingsExtensions.SetEase<TweenerCore<Quaternion, Vector3, QuaternionOptions>>(ShortcutExtensions.DOLocalRotate((Transform)(object)_rect, Vector3.forward * exitRotation, exitDuration, (RotateMode)0), exitEase);
		TweenSettingsExtensions.OnComplete<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(_rect.DOAnchorPos(_visiblePos + new Vector2(0f, 0f - startYOffset), exitDuration), exitEase), (TweenCallback)delegate
		{
			((Component)this).gameObject.SetActive(false);
			onClose?.Invoke();
			ShowNextMission();
		});
	}

	private void PlayCoinRainEffect()
	{
		if (!((Object)(object)coinRainEffect == (Object)null))
		{
			((Component)coinRainEffect).gameObject.SetActive(true);
			coinRainEffect.Play();
		}
	}

	private void StopCoinRainEffect()
	{
		if (!((Object)(object)coinRainEffect == (Object)null))
		{
			coinRainEffect.Stop();
			((Component)coinRainEffect).gameObject.SetActive(false);
		}
	}

	public void OnOpen(mainMission[] missions, Action _onClose = null)
	{
		onClose = _onClose;
		foreach (mainMission item in missions)
		{
			_pendingMissions.Enqueue(item);
		}
		if (!_isShowing)
		{
			ShowNextMission();
		}
	}

	private void OnConfirmClicked()
	{
		if (_currentMission != null && !_isRewardClaimed && _isSlidIn)
		{
			_isRewardClaimed = true;
			ClaimCurrentReward(1);
			SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
			SlideOut();
		}
	}

	private void OnAdClicked()
	{
		if (_currentMission != null && !_isRewardClaimed && _isSlidIn)
		{
			FakeVideoManager.Instance.ShowAd(delegate
			{
				_isRewardClaimed = true;
				ClaimCurrentReward(2);
				SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
				SlideOut();
			}, delegate
			{
			}, "主线任务奖励双倍");
		}
	}

	private void ClaimCurrentReward(int multiplier)
	{
		if (_currentMission != null)
		{
			double num = _currentMission.RewardNum * (double)multiplier;
			if (_currentMission.RewardType == 1)
			{
				SingletonMono<UpgradeManager>.Instance.AddGoldInternal(num);
			}
			else if (_currentMission.RewardType == 2)
			{
				GameDataManager.Instance.AddDiamond((int)num);
			}
			else if (_currentMission.RewardType == 3 || _currentMission.RewardType == 4 || _currentMission.RewardType == 5)
			{
				GameDataManager.Instance.AddItem(suipianId, (long)num);
			}
			else if (_currentMission.RewardType == 6)
			{
				GameDataManager.Instance.ChangeTalentPoint(isAdd: true, (int)num);
			}
			else if (_currentMission.RewardType >= 101)
			{
				GameDataManager.Instance.AddItem(suipianId, (long)num);
			}
		}
	}

	public void CheckAndShowPendingRewards()
	{
		if ((Object)(object)MissionManager.Instance == (Object)null)
		{
			return;
		}
		List<mainMission> pendingRewards = MissionManager.Instance.GetPendingRewards();
		if (pendingRewards == null || pendingRewards.Count == 0)
		{
			return;
		}
		if (_isShowing)
		{
			foreach (mainMission item in pendingRewards)
			{
				_pendingMissions.Enqueue(item);
			}
			return;
		}
		foreach (mainMission item2 in pendingRewards)
		{
			_pendingMissions.Enqueue(item2);
		}
		ShowNextMission();
	}

	public string GetSpritePath(int itemId)
	{
		switch (itemId)
		{
		case 1:
			return "Assets/GameRes/NewUI/Common/images1/ty_icon_coin01.png";
		case 2:
			return "Assets/GameRes/NewUI/Common/images1/ty_icon_diamond01.png";
		case 6:
			return "Assets/GameRes/NewUI/Common/images1/ty_icon_talentPoint.png";
		case 3:
		case 4:
		case 5:
		case 101:
		case 102:
		case 103:
		case 104:
		case 105:
		case 106:
		case 107:
		case 108:
			return $"Assets/GameRes/icon/suipian/{itemId}.png";
		default:
			return "Assets/GameRes/NewUI/Common/images1/ty_icon_coin01.png";
		}
	}
}

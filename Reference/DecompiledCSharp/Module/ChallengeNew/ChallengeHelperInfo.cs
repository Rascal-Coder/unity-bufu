using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using cfg;

namespace Module.ChallengeNew;

public class ChallengeHelperInfo : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI helperNameText;

	[SerializeField]
	private TextMeshProUGUI levelText;

	[SerializeField]
	private Image helperIcon;

	[SerializeField]
	private GameObject lockMask;

	[SerializeField]
	private GameObject xianshi;

	[SerializeField]
	private GameObject selectedMark;

	[Header("出战数量")]
	[SerializeField]
	private TextMeshProUGUI sumTxt;

	[SerializeField]
	private Button addBtn;

	[SerializeField]
	private Button reduceBtn;

	[SerializeField]
	private Image kuang;

	[SerializeField]
	private Image kuangBg;

	[SerializeField]
	private Image power;

	private int _helperId;

	private bool _isUnlocked;

	private int _battleCount;

	private int _maxBattleCount;

	public Action<int, int> OnBattleCountChanged;

	public Func<bool> OnCheckCanAdd;

	public int HelperId => _helperId;

	public bool IsUnlocked => _isUnlocked;

	public int BattleCount => _battleCount;

	public int MaxBattleCount => _maxBattleCount;

	private void Awake()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		if ((Object)(object)addBtn != (Object)null)
		{
			((UnityEvent)addBtn.onClick).AddListener(new UnityAction(OnAddBtnClicked));
		}
		if ((Object)(object)reduceBtn != (Object)null)
		{
			((UnityEvent)reduceBtn.onClick).AddListener(new UnityAction(OnReduceBtnClicked));
		}
	}

	public void Initialize(helperRole config, HelperInfo data, int mainMissionLevel)
	{
		if (config == null)
		{
			return;
		}
		_helperId = config.Id;
		_battleCount = 0;
		_maxBattleCount = data?.maxCount ?? 0;
		_isUnlocked = config.HelperUnlockCondition == -1 || mainMissionLevel >= config.HelperUnlockCondition;
		if (data != null)
		{
			if (data.isAdUnlock == 1)
			{
				_isUnlocked = true;
			}
		}
		else
		{
			_isUnlocked = false;
		}
		if ((Object)(object)helperNameText != (Object)null)
		{
			((TMP_Text)helperNameText).text = config.HelperName;
		}
		int num = data?.helperLevel ?? 1;
		if ((Object)(object)levelText != (Object)null)
		{
			((TMP_Text)levelText).text = $"Lv.{num}";
		}
		if ((Object)(object)lockMask != (Object)null)
		{
			lockMask.SetActive(!_isUnlocked);
		}
		if ((Object)(object)xianshi != (Object)null)
		{
			xianshi.SetActive(_isUnlocked);
		}
		if ((Object)(object)selectedMark != (Object)null)
		{
			selectedMark.SetActive(false);
		}
		if ((Object)(object)helperIcon != (Object)null)
		{
			string helperIconPath = Constants.GetHelperIconPath(config.Id);
			SingletonMono<ResManager>.Instance.LoadSpriteAsync(helperIconPath, delegate(Sprite sprite)
			{
				//IL_0083: Unknown result type (might be due to invalid IL or missing references)
				if ((Object)(object)sprite != (Object)null && (Object)(object)helperIcon != (Object)null)
				{
					helperIcon.sprite = sprite;
					((Component)helperIcon).gameObject.SetActive(true);
					((Graphic)helperIcon).SetNativeSize();
					if (config.Id == 6)
					{
						Transform transform = ((Component)helperIcon).transform;
						((RectTransform)((transform is RectTransform) ? transform : null)).anchoredPosition = new Vector2(0f, -80f);
					}
				}
			});
		}
		RefreshBattleCountUI();
		RefreshButtonState();
	}

	public void SetBattleCount(int count)
	{
		_battleCount = Mathf.Clamp(count, 0, _maxBattleCount);
		RefreshBattleCountUI();
		RefreshButtonState();
		OnBattleCountChanged?.Invoke(_helperId, _battleCount);
	}

	public void SetMaxBattleCount(int maxCount)
	{
		_maxBattleCount = maxCount;
		if (_battleCount > _maxBattleCount)
		{
			_battleCount = _maxBattleCount;
			OnBattleCountChanged?.Invoke(_helperId, _battleCount);
		}
		RefreshBattleCountUI();
		RefreshButtonState();
	}

	public void SetSelected(bool selected)
	{
		if ((Object)(object)selectedMark != (Object)null)
		{
			selectedMark.SetActive(selected);
		}
	}

	public void Refresh(helperRole config, HelperInfo data, int mainMissionLevel)
	{
		if (config == null)
		{
			return;
		}
		_isUnlocked = config.HelperUnlockCondition == -1 || mainMissionLevel >= config.HelperUnlockCondition;
		if (data != null)
		{
			if (data.isAdUnlock == 1)
			{
				_isUnlocked = true;
			}
		}
		else
		{
			_isUnlocked = false;
		}
		int rare = config.Rare;
		SingletonMono<ResManager>.Instance.LoadSpriteAsync(GetKuangPath(rare), delegate(Sprite sprite)
		{
			kuang.sprite = sprite;
			((Graphic)kuang).SetNativeSize();
		});
		SingletonMono<ResManager>.Instance.LoadSpriteAsync(GetKuangBgPath(rare), delegate(Sprite sprite)
		{
			kuangBg.sprite = sprite;
			((Graphic)kuangBg).SetNativeSize();
		});
		SingletonMono<ResManager>.Instance.LoadSpriteAsync(Constants.GetNSR(rare), delegate(Sprite sprite)
		{
			power.sprite = sprite;
			((Graphic)power).SetNativeSize();
		}, isAtlas: false);
		int num = data?.helperLevel ?? 1;
		if ((Object)(object)levelText != (Object)null)
		{
			((TMP_Text)levelText).text = $"Lv.{num}";
		}
		if ((Object)(object)lockMask != (Object)null)
		{
			lockMask.SetActive(!_isUnlocked);
		}
		if ((Object)(object)xianshi != (Object)null)
		{
			xianshi.SetActive(_isUnlocked);
		}
		_maxBattleCount = data?.maxCount ?? 0;
		if (_battleCount > _maxBattleCount)
		{
			_battleCount = _maxBattleCount;
			OnBattleCountChanged?.Invoke(_helperId, _battleCount);
		}
		RefreshBattleCountUI();
		RefreshButtonState();
	}

	private void OnAddBtnClicked()
	{
		SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
		if (_battleCount < _maxBattleCount && (OnCheckCanAdd == null || OnCheckCanAdd()))
		{
			_battleCount++;
			RefreshBattleCountUI();
			RefreshButtonState();
			OnBattleCountChanged?.Invoke(_helperId, _battleCount);
		}
	}

	private void OnReduceBtnClicked()
	{
		SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
		if (_battleCount > 0)
		{
			_battleCount--;
			RefreshBattleCountUI();
			RefreshButtonState();
			OnBattleCountChanged?.Invoke(_helperId, _battleCount);
		}
	}

	private void RefreshBattleCountUI()
	{
		if ((Object)(object)sumTxt != (Object)null)
		{
			((TMP_Text)sumTxt).text = $"{_battleCount}/{_maxBattleCount}";
		}
	}

	private void RefreshButtonState()
	{
		bool flag = _isUnlocked && _maxBattleCount > 0;
		if ((Object)(object)addBtn != (Object)null)
		{
			bool flag2 = flag && _battleCount < _maxBattleCount;
			if (flag2 && OnCheckCanAdd != null)
			{
				flag2 = OnCheckCanAdd();
			}
			((Selectable)addBtn).interactable = flag2;
		}
		if ((Object)(object)reduceBtn != (Object)null)
		{
			((Selectable)reduceBtn).interactable = flag && _battleCount > 0;
		}
	}

	private void OnDestroy()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		if ((Object)(object)addBtn != (Object)null)
		{
			((UnityEvent)addBtn.onClick).RemoveListener(new UnityAction(OnAddBtnClicked));
		}
		if ((Object)(object)reduceBtn != (Object)null)
		{
			((UnityEvent)reduceBtn.onClick).RemoveListener(new UnityAction(OnReduceBtnClicked));
		}
	}

	public string GetKuangPath(int index)
	{
		return index switch
		{
			1 => "Assets/GameRes/NewUI/Challenge/images/challenge_pic_avatarN01.png", 
			2 => "Assets/GameRes/NewUI/Challenge/images/challenge_pic_avatarR01.png", 
			3 => "Assets/GameRes/NewUI/Challenge/images/challenge_pic_avatarSR01.png", 
			_ => "Assets/GameRes/NewUI/Challenge/images/challenge_pic_avatarN01.png", 
		};
	}

	public string GetKuangBgPath(int index)
	{
		return index switch
		{
			1 => "Assets/GameRes/NewUI/Challenge/images/challenge_pic_avatarN02.png", 
			2 => "Assets/GameRes/NewUI/Challenge/images/challenge_pic_avatarR02.png", 
			3 => "Assets/GameRes/NewUI/Challenge/images/challenge_pic_avatarSR02.png", 
			_ => "Assets/GameRes/NewUI/Challenge/images/challenge_pic_avatarN02.png", 
		};
	}
}

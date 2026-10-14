using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ChallengeUpgradeItem : MonoBehaviour
{
	[Header("UI组件")]
	[SerializeField]
	private Image itemIcon;

	[SerializeField]
	private TextMeshProUGUI nameTmp;

	[SerializeField]
	private TextMeshProUGUI upgradeMoneyTmp;

	[SerializeField]
	private Image canUpgrade;

	[SerializeField]
	private TextMeshProUGUI lvTmp;

	[SerializeField]
	private Button btn;

	[SerializeField]
	private SkeletonGraphic button_upgrade1;

	private ChallengeUpgradeType _upgradeType;

	private ChallengeMain _challengeMain;

	private int _currentLevel;

	private static readonly string[] TypeNames = new string[4] { "银币数量", "银币价值", "金币数量", "金币价值" };

	public void Initialize(ChallengeUpgradeType type, ChallengeMain main)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		_upgradeType = type;
		_challengeMain = main;
		_currentLevel = (((Object)(object)main != (Object)null) ? main.GetUpgradeLevel(type) : 0);
		if ((Object)(object)btn != (Object)null)
		{
			((UnityEventBase)btn.onClick).RemoveAllListeners();
			((UnityEvent)btn.onClick).AddListener(new UnityAction(OnClick));
		}
		RefreshUI();
	}

	private void OnClick()
	{
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected O, but got Unknown
		if ((Object)(object)_challengeMain == (Object)null)
		{
			return;
		}
		double currentCost = GetCurrentCost();
		if (_challengeMain.GetCollectCoin() >= currentCost)
		{
			_challengeMain.SpendCollectCoin(currentCost);
			_currentLevel++;
			_challengeMain.SetUpgradeLevel(_upgradeType, _currentLevel);
			if (_upgradeType == ChallengeUpgradeType.SilverCount)
			{
				_challengeMain.SpawnCoin(CoinType.Silver);
			}
			else if (_upgradeType == ChallengeUpgradeType.GoldCount)
			{
				_challengeMain.SpawnCoin(CoinType.Gold);
			}
			_challengeMain.RefreshUpgradeItems();
			SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
			if (button_upgrade1.AnimationState != null)
			{
				TrackEntryDelegate completeHandler = null;
				completeHandler = (TrackEntryDelegate)delegate
				{
					button_upgrade1.AnimationState.Complete -= completeHandler;
					((Component)button_upgrade1).gameObject.SetActive(false);
				};
				((Component)button_upgrade1).gameObject.SetActive(true);
				button_upgrade1.AnimationState.SetAnimation(0, "upgrade", false);
				button_upgrade1.AnimationState.Complete += completeHandler;
			}
		}
		else
		{
			SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/cantbuy.mp3");
		}
	}

	public void RefreshUI()
	{
		if ((Object)(object)_challengeMain != (Object)null)
		{
			_currentLevel = _challengeMain.GetUpgradeLevel(_upgradeType);
		}
		if ((Object)(object)nameTmp != (Object)null)
		{
			((TMP_Text)nameTmp).text = TypeNames[(int)_upgradeType];
		}
		if ((Object)(object)lvTmp != (Object)null)
		{
			((TMP_Text)lvTmp).text = $"Lv.{_currentLevel}";
		}
		double currentCost = GetCurrentCost();
		if ((Object)(object)upgradeMoneyTmp != (Object)null)
		{
			((TMP_Text)upgradeMoneyTmp).text = NumberFormatter.FormatNumber(currentCost);
		}
		if ((Object)(object)canUpgrade != (Object)null)
		{
			bool flag = (Object)(object)_challengeMain != (Object)null && currentCost > 0.0 && _challengeMain.GetCollectCoin() >= currentCost;
			((Component)canUpgrade).gameObject.SetActive(flag);
			((Selectable)btn).interactable = flag;
		}
		string iconPath = GetIconPath();
		if (string.IsNullOrEmpty(iconPath))
		{
			return;
		}
		SingletonMono<ResManager>.Instance.LoadSpriteAsync(iconPath, delegate(Sprite sprite)
		{
			if ((Object)(object)itemIcon != (Object)null && (Object)(object)sprite != (Object)null)
			{
				itemIcon.sprite = sprite;
				((Component)itemIcon).gameObject.SetActive(true);
			}
		}, isAtlas: false);
	}

	private double GetCurrentCost()
	{
		if ((Object)(object)_challengeMain != (Object)null)
		{
			return _challengeMain.GetUpgradeCost(_upgradeType, _currentLevel);
		}
		return 0.0;
	}

	private string GetIconPath()
	{
		return _upgradeType switch
		{
			ChallengeUpgradeType.SilverCount => Constants.GetUpgradeItemIcon(3), 
			ChallengeUpgradeType.SilverValue => Constants.GetUpgradeItemIcon(2, "itemUpgrade2"), 
			ChallengeUpgradeType.GoldCount => Constants.GetUpgradeItemIcon(4), 
			ChallengeUpgradeType.GoldValue => Constants.GetUpgradeItemIcon(3, "itemUpgrade2"), 
			_ => Constants.GetUpgradeItemIcon(3), 
		};
	}
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Module.UI;

public class RewardBoxUI : MonoBehaviour
{
	[Header("奖励容器")]
	[SerializeField]
	private Button GetButton;

	[SerializeField]
	private Button doubleButton;

	[SerializeField]
	private RewardBoxItem item1;

	[SerializeField]
	private RectTransform con;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	public List<Sprite> icons;

	private bool _isSuipian;

	private string _adReason;

	private List<double> _rewardItemList;

	private bool _isAddItem;

	private void Awake()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		if ((Object)(object)GetButton != (Object)null)
		{
			((UnityEvent)GetButton.onClick).AddListener(new UnityAction(CloseRewardBox));
		}
		if ((Object)(object)doubleButton != (Object)null)
		{
			((UnityEvent)doubleButton.onClick).AddListener(new UnityAction(DoubleReward));
		}
		if ((Object)(object)closeButton != (Object)null)
		{
			((UnityEvent)closeButton.onClick).AddListener(new UnityAction(CloseRewardBox));
		}
		((Component)item1).gameObject.SetActive(false);
	}

	private void ShowSuipian()
	{
		if (_rewardItemList == null || !_isAddItem || !_isSuipian)
		{
			return;
		}
		List<int> list = new List<int>();
		for (int i = 0; i < _rewardItemList.Count; i += 2)
		{
			int num = (int)_rewardItemList[i];
			int num2 = 0;
			if (num >= 1 && num <= 5)
			{
				num2 = num;
			}
			else
			{
				switch (num)
				{
				case 101:
					num2 = 3;
					break;
				case 102:
				case 103:
				case 104:
					num2 = 4;
					break;
				default:
					if (num >= 105 && num <= 109)
					{
						num2 = 5;
					}
					break;
				}
			}
			if (num2 > 0 && num2 <= 5)
			{
				list.Add(num2);
			}
		}
		if (list.Count > 0)
		{
			UIManager.Instance.ShowCoinDimondTween(list);
		}
	}

	private void CloseRewardBox()
	{
		if (_isAddItem)
		{
			for (int i = 0; i < _rewardItemList.Count; i += 2)
			{
				if (i + 1 < _rewardItemList.Count)
				{
					GrantReward(_rewardItemList[i], _rewardItemList[i + 1], 1);
				}
			}
		}
		ShowSuipian();
		((Component)this).gameObject.SetActive(false);
	}

	private void DoubleReward()
	{
		SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
		if (!_isAddItem)
		{
			((Component)this).gameObject.SetActive(false);
			return;
		}
		if (string.IsNullOrEmpty(_adReason))
		{
			_adReason = "领奖双倍奖励";
		}
		FakeVideoManager.Instance.ShowAd(delegate
		{
			for (int i = 0; i < _rewardItemList.Count; i += 2)
			{
				if (i + 1 < _rewardItemList.Count)
				{
					GrantReward(_rewardItemList[i], _rewardItemList[i + 1], 2);
				}
			}
			((Component)this).gameObject.SetActive(false);
			ShowSuipian();
		}, delegate
		{
			CloseRewardBox();
			((Component)this).gameObject.SetActive(false);
		}, _adReason);
	}

	public void ShowRewards(List<double> rewardItemList, bool isAddItem = false, bool isSuipian = true, string adReason = null, bool noDouble = false)
	{
		((Component)doubleButton).gameObject.SetActive(!noDouble);
		_adReason = adReason;
		ClearRewards();
		((Component)this).gameObject.SetActive(true);
		if (rewardItemList == null)
		{
			return;
		}
		_isSuipian = isSuipian;
		_rewardItemList = rewardItemList;
		_isAddItem = isAddItem;
		for (int i = 0; i < rewardItemList.Count; i += 2)
		{
			if (i + 1 < rewardItemList.Count)
			{
				double itemId = rewardItemList[i];
				double count = rewardItemList[i + 1];
				CreateRewardItem(itemId, count);
			}
		}
	}

	private void CreateRewardItem(double itemId, double count)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		((Component)item1).gameObject.SetActive(true);
		RewardBoxItem rewardBoxItem = Object.Instantiate<RewardBoxItem>(item1, (Transform)(object)con);
		rewardBoxItem.SetData((int)itemId, count);
		Transform transform = ((Component)rewardBoxItem).transform;
		((RectTransform)((transform is RectTransform) ? transform : null)).anchoredPosition = new Vector2(0f, 0f);
		((Component)item1).gameObject.SetActive(false);
	}

	private void ClearRewards()
	{
		if ((Object)(object)con == (Object)null)
		{
			return;
		}
		for (int num = ((Transform)con).childCount - 1; num >= 0; num--)
		{
			if ((Object)(object)((Transform)con).GetChild(num) != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)((Transform)con).GetChild(num)).gameObject);
			}
		}
	}

	private void GrantReward(double itemId, double count, int multiplier)
	{
		if (!(count <= 0.0))
		{
			int num = (int)itemId;
			double num2 = count * (double)multiplier;
			switch (num)
			{
			case 1:
				SingletonMono<UpgradeManager>.Instance.AddGoldInternal(num2);
				return;
			case 2:
				GameDataManager.Instance.AddDiamond((long)num2);
				return;
			case 3:
				GrantRandomHelperShards(101, 102, (int)num2);
				return;
			case 4:
				GrantRandomHelperShards(102, 105, (int)num2);
				return;
			case 5:
				GrantRandomHelperShards(104, 109, (int)num2);
				return;
			case 6:
				GameDataManager.Instance.ChangeTalentPoint(isAdd: true, (int)num2);
				break;
			}
			GameDataManager.Instance.AddItem(num, (long)num2);
		}
	}

	private void GrantRandomHelperShards(int minInclusive, int maxExclusive, int count)
	{
		if (count > 0)
		{
			for (int i = 0; i < count; i++)
			{
				int itemId = Random.Range(minInclusive, maxExclusive);
				GameDataManager.Instance.AddItem(itemId, 1L);
			}
		}
	}
}

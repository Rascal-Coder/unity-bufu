using TMPro;
using UnityEngine;
using UnityEngine.UI;
using cfg;

namespace Module.Shop;

public class ShopRewardItemView : MonoBehaviour
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private Image colorbg;

	[SerializeField]
	private RectTransform valueCon;

	[SerializeField]
	private TextMeshProUGUI valueText;

	[SerializeField]
	private Image[] qualityImages;

	private shop _config;

	public void SetConfig(shop config)
	{
		_config = config;
		Refresh();
	}

	public void Refresh()
	{
		if (_config == null || _config.RewardType == null || _config.RewardType.Count == 0)
		{
			return;
		}
		int rewardType = _config.RewardType[0];
		if (rewardType == 5)
		{
			((Component)colorbg).gameObject.SetActive(true);
		}
		else
		{
			((Component)colorbg).gameObject.SetActive(false);
		}
		int num = ((_config.Count != null && _config.Count.Count > 0) ? _config.Count[0] : 0);
		if ((Object)(object)valueText != (Object)null)
		{
			if (rewardType == 1)
			{
				double num2 = 0.0;
				if ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null)
				{
					num2 = SingletonMono<UpgradeManager>.Instance.GetAverageGoldPerMinuteFromLast30Seconds() * (double)num;
					ShopRewardCalculator.AddGoldReward(_config, num2);
				}
				((TMP_Text)valueText).text = NumberFormatter.FormatNumber(num2);
			}
			else
			{
				((TMP_Text)valueText).text = NumberFormatter.FormatNumber(num);
			}
		}
		if (qualityImages != null)
		{
			for (int i = 0; i < qualityImages.Length; i++)
			{
				((Component)qualityImages[i]).gameObject.SetActive(false);
			}
		}
		if (rewardType >= 3 && rewardType <= 5)
		{
			((Component)qualityImages[rewardType - 3]).gameObject.SetActive(true);
			((Component)valueCon).gameObject.SetActive(false);
		}
		if (!((Object)(object)icon != (Object)null))
		{
			return;
		}
		string iconPath = GetIconPath(rewardType);
		if (string.IsNullOrEmpty(iconPath))
		{
			return;
		}
		SingletonMono<ResManager>.Instance.LoadSpriteAsync(iconPath, delegate(Sprite sprite)
		{
			if ((Object)(object)sprite != (Object)null)
			{
				icon.sprite = sprite;
				if (rewardType == 6)
				{
					((Graphic)icon).SetNativeSize();
				}
			}
		}, isAtlas: false);
	}

	private string GetIconPath(int rewardType)
	{
		if (_config.Id >= 101 && _config.Id <= 106)
		{
			if (rewardType == 1 || rewardType == 2)
			{
				return Constants.GetShopIconPath(_config.Id);
			}
			return Constants.GetSuipianIconPath(rewardType);
		}
		return Constants.GetSuipianIconPath(rewardType);
	}
}

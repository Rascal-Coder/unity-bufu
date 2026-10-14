using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardBoxItem : MonoBehaviour
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private TextMeshProUGUI countText;

	public void SetData(int itemId, double count)
	{
		if ((Object)(object)countText != (Object)null)
		{
			((TMP_Text)countText).text = NumberFormatter.FormatNumber(count);
		}
		if (!((Object)(object)icon != (Object)null))
		{
			return;
		}
		((Component)icon).gameObject.SetActive(false);
		Debug.Log((object)GetIconPath(itemId));
		SingletonMono<ResManager>.Instance.LoadSpriteAsync(GetIconPath(itemId), delegate(Sprite sprite)
		{
			if (!((Object)(object)this == (Object)null) && !((Object)(object)icon == (Object)null))
			{
				icon.sprite = sprite;
				((Component)icon).gameObject.SetActive(true);
				if (itemId == 6)
				{
					((Graphic)icon).SetNativeSize();
				}
			}
		}, isAtlas: false);
	}

	private string GetIconPath(int rewardType)
	{
		if (rewardType == 1 || rewardType == 2)
		{
			return Constants.GetMoneyPath(rewardType);
		}
		return Constants.GetSuipianIconPath(rewardType);
	}
}

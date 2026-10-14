using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Module.OfflineIncome;

public class OfflineRewardItem : MonoBehaviour
{
	[Header("奖励项组件底图")]
	[SerializeField]
	private Image rewardBg;

	[Header("奖励项组件")]
	[SerializeField]
	private Image rewardIcon;

	[SerializeField]
	private TextMeshProUGUI rewardCount;

	private void Awake()
	{
		((Component)rewardIcon).gameObject.SetActive(false);
	}

	public void SetData(int itemID, long count)
	{
		if ((Object)(object)rewardIcon != (Object)null)
		{
			switch (itemID)
			{
			case 0:
				SingletonMono<ResManager>.Instance.LoadSpriteAsync("Assets/GameRes/UI/common/images/ty_icon_suipianlv01.png", delegate(Sprite sprite)
				{
					SetIcon(sprite);
				});
				break;
			case 1:
				SingletonMono<ResManager>.Instance.LoadSpriteAsync("Assets/GameRes/UI/common/images/ty_icon_suipianhuang01.png", delegate(Sprite sprite)
				{
					SetIcon(sprite);
				});
				break;
			case 2:
				SingletonMono<ResManager>.Instance.LoadSpriteAsync("Assets/GameRes/UI/common/images/ty_icon_suipianzi01.png", delegate(Sprite sprite)
				{
					SetIcon(sprite);
				});
				break;
			case 1001:
				SingletonMono<ResManager>.Instance.LoadSpriteAsync("Assets/GameRes/UI/common/images/ty_icon_diamond01.png", delegate(Sprite sprite)
				{
					SetIcon(sprite);
				});
				break;
			default:
				SingletonMono<ResManager>.Instance.LoadSpriteAsync($"Assets/GameRes/icon/suipian/{itemID}.png", delegate(Sprite sprite)
				{
					SetIcon(sprite);
				});
				break;
			}
		}
		if ((Object)(object)rewardCount != (Object)null)
		{
			((TMP_Text)rewardCount).text = count.ToString();
		}
	}

	public void SetIcon(Sprite icon)
	{
		if ((Object)(object)rewardIcon != (Object)null)
		{
			rewardIcon.sprite = icon;
			((Component)rewardIcon).gameObject.SetActive(true);
		}
	}

	public void SetCount(int count)
	{
		if ((Object)(object)rewardCount != (Object)null)
		{
			((TMP_Text)rewardCount).text = count.ToString();
		}
	}
}

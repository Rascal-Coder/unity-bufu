using TMPro;
using UnityEngine;
using UnityEngine.UI;
using cfg;

public class AuctionSelectItem : MonoBehaviour
{
	[Header("自身图标")]
	[SerializeField]
	private Image icon;

	[Header("关卡未解锁提示")]
	[SerializeField]
	private TextMeshProUGUI tip;

	[Header("关卡未解锁图标")]
	[SerializeField]
	private Image suo;

	private AuctionGrid parentGrid;

	private int id;

	public void Init(int id, AuctionGrid grid)
	{
		this.id = id;
		SingletonMono<ResManager>.Instance.LoadSpriteAsync($"Assets/GameRes/icon/auction_title/challenge_pic_island_0{id}_scale2.png", delegate(Sprite sprite)
		{
			icon.sprite = sprite;
		}, isAtlas: false);
		auction auctionConfig = ConfigManager.Instance.GetAuctionConfig(id);
		((TMP_Text)tip).text = $"主线等级达到{auctionConfig.ConditionLevel}级解锁";
		parentGrid = grid;
		Refresh();
	}

	public void Refresh()
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		AuctionDataManager instance = AuctionDataManager.Instance;
		if (instance != null && instance.GetIsCanAuction(id))
		{
			((Component)suo).gameObject.SetActive(false);
			((Component)tip).gameObject.SetActive(false);
			if ((Object)(object)icon != (Object)null)
			{
				((Graphic)icon).color = Color.white;
			}
		}
		else
		{
			((Component)suo).gameObject.SetActive(true);
			((Component)tip).gameObject.SetActive(true);
			if ((Object)(object)icon != (Object)null)
			{
				((Graphic)icon).color = new Color(0.51f, 0.51f, 0.51f);
			}
		}
	}
}

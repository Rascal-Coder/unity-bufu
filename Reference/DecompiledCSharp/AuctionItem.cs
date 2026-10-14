using TMPro;
using UnityEngine;
using UnityEngine.UI;
using cfg;

public class AuctionItem : MonoBehaviour
{
	[Header("图标icon")]
	[SerializeField]
	private Image icon;

	[Header("物品名字")]
	[SerializeField]
	private TextMeshProUGUI nameTxt;

	[Header("物品价值")]
	[SerializeField]
	private TextMeshProUGUI priceTxt;

	private int itemType;

	private string itemName;

	private float price;

	public void Init(int type, float price)
	{
		itemType = type;
		int num = AuctionDataManager.Instance?.GetItemIconId(itemType) ?? 1001;
		auctionItem auctionItems = ConfigManager.Instance.GetAuctionItems(num);
		if (auctionItems != null)
		{
			itemName = auctionItems.ItemName;
			SingletonMono<ResManager>.Instance.LoadSpriteAsync($"Assets/GameRes/icon/auction/{num}.png", delegate(Sprite sprite)
			{
				icon.sprite = sprite;
			}, isAtlas: false);
		}
		this.price = price;
		HideUI();
	}

	public void HideUI()
	{
		((Component)nameTxt).gameObject.SetActive(false);
		((Component)priceTxt).gameObject.SetActive(false);
	}

	public void DisplayUI()
	{
		((TMP_Text)nameTxt).text = itemName;
		((TMP_Text)priceTxt).text = NumberFormatter.FormatNumber(price);
		((Component)nameTxt).gameObject.SetActive(true);
		((Component)priceTxt).gameObject.SetActive(true);
	}

	public float GetPrice()
	{
		return price;
	}
}

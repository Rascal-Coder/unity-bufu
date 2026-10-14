using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EvolvePanel : MonoBehaviour
{
	private bool evolveConfirmed;

	[Header("进化按键")]
	[SerializeField]
	private Button evolveBtn;

	[Header("返回按键")]
	[SerializeField]
	private Button backBtn;

	[Header("标题image")]
	[SerializeField]
	private RectTransform titleImage;

	[Header("标题文本")]
	[SerializeField]
	private TextMeshProUGUI titleTxt;

	[Header("价值1文本")]
	[SerializeField]
	private TextMeshProUGUI priceTxt1;

	[Header("价值2文本")]
	[SerializeField]
	private TextMeshProUGUI priceTxt2;

	[Header("进化前的硬币/骰子image显示")]
	[SerializeField]
	private Image objImage1;

	[Header("进化后的硬币/骰子image显示")]
	[SerializeField]
	private Image objImage2;

	[Header("进化内容文本介绍")]
	[SerializeField]
	private TextMeshProUGUI decTxt;

	[Header("进化内容文本2介绍")]
	[SerializeField]
	private TextMeshProUGUI decTxt2;

	public event Action OnEvolveConfirmed;

	private void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		((UnityEvent)backBtn.onClick).AddListener((UnityAction)delegate
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		});
		((UnityEvent)evolveBtn.onClick).AddListener(new UnityAction(OnEvolveBtnClick));
	}

	private void OnEvolveBtnClick()
	{
		if (!evolveConfirmed)
		{
			evolveConfirmed = true;
			if ((Object)(object)evolveBtn != (Object)null)
			{
				((Selectable)evolveBtn).interactable = false;
			}
			if ((Object)(object)backBtn != (Object)null)
			{
				((Selectable)backBtn).interactable = false;
			}
			this.OnEvolveConfirmed?.Invoke();
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	public void Init(int itemId, int objType, int type)
	{
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		string arg = ((objType == 1) ? "coin" : "dice");
		switch (itemId)
		{
		case 2:
			((TMP_Text)titleTxt).text = "铜币进化";
			break;
		case 3:
			((TMP_Text)titleTxt).text = "银币进化";
			break;
		case 4:
			((TMP_Text)titleTxt).text = "金币进化";
			break;
		case 5:
			((TMP_Text)titleTxt).text = "紫金币进化";
			break;
		case 102:
			((TMP_Text)titleTxt).text = "铜骰子进化";
			break;
		case 103:
			((TMP_Text)titleTxt).text = "银骰子进化";
			break;
		case 104:
			((TMP_Text)titleTxt).text = "金骰子进化";
			break;
		case 105:
			((TMP_Text)titleTxt).text = "紫金骰子进化";
			break;
		}
		string text = ((TMP_Text)titleTxt).text;
		if (text.Length == 4)
		{
			titleImage.sizeDelta = new Vector2(500f, 166f);
		}
		else if (text.Length == 5)
		{
			titleImage.sizeDelta = new Vector2(580f, 166f);
		}
		else if (text.Length == 6)
		{
			titleImage.sizeDelta = new Vector2(660f, 166f);
		}
		UpgradeManager instance = SingletonMono<UpgradeManager>.Instance;
		int num = instance?.GetEvolutionTier(itemId) ?? 0;
		int num2 = instance?.GetEvolutionTargetTier(itemId) ?? (num + 1);
		int num3 = num + 1;
		int num4 = num2 + 1;
		string assetPath = $"Assets/GameRes/NewUI/Evolve/images/{arg}_{type}_{num3}.png";
		SingletonMono<ResManager>.Instance.LoadSpriteAsync(assetPath, delegate(Sprite sprite)
		{
			if ((Object)(object)objImage1 != (Object)null && (Object)(object)sprite != (Object)null)
			{
				objImage1.sprite = sprite;
			}
		}, isAtlas: false);
		string assetPath2 = $"Assets/GameRes/NewUI/Evolve/images/{arg}_{type}_{num4}.png";
		SingletonMono<ResManager>.Instance.LoadSpriteAsync(assetPath2, delegate(Sprite sprite)
		{
			if ((Object)(object)objImage2 != (Object)null && (Object)(object)sprite != (Object)null)
			{
				objImage2.sprite = sprite;
			}
		}, isAtlas: false);
		double num5 = instance?.GetEvolutionIncomeMultiplierForTier(itemId, num) ?? 1.0;
		double num6 = instance?.GetEvolutionIncomeMultiplierForTier(itemId, num2) ?? num5;
		double num7 = ((num5 > 0.0) ? (num6 / num5) : 1.0);
		if ((Object)(object)priceTxt1 != (Object)null)
		{
			((TMP_Text)priceTxt1).text = num5.ToString();
		}
		if ((Object)(object)priceTxt2 != (Object)null)
		{
			((TMP_Text)priceTxt2).text = num6.ToString();
		}
		string obj = (new string[4] { "铜", "银", "金", "紫" })[Mathf.Clamp(type - 1, 0, 3)];
		string text2 = ((objType == 1) ? "币" : "骰子");
		string text3 = obj + text2;
		double num8 = num7;
		if ((Object)(object)decTxt != (Object)null)
		{
			((TMP_Text)decTxt).text = $"进化后,所有{text3}将合成为1个更强的{text3},新{text3}收益提升{num8}倍";
		}
		if ((Object)(object)decTxt2 != (Object)null)
		{
			((TMP_Text)decTxt2).text = $"{text3}收益*<color=#1a9211>{num8}</color>倍";
		}
	}

	private void OnDestroy()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		if ((Object)(object)evolveBtn != (Object)null)
		{
			((UnityEvent)evolveBtn.onClick).RemoveListener(new UnityAction(OnEvolveBtnClick));
		}
		if ((Object)(object)backBtn != (Object)null)
		{
			((UnityEventBase)backBtn.onClick).RemoveAllListeners();
		}
	}
}

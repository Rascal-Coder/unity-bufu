using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AuctionExchange : MonoBehaviour
{
	[Header("拍卖场金钱显示")]
	[SerializeField]
	private TextMeshProUGUI coinTxt;

	[Header("兑换金钱投入显示")]
	[SerializeField]
	private TextMeshProUGUI coinCostTxt;

	[Header("兑换金钱按钮")]
	[SerializeField]
	private Button exchangebtn1;

	[Header("获得主线金钱显示")]
	[SerializeField]
	private TextMeshProUGUI GetCoinTxt;

	[Header("兑换钻石投入显示")]
	[SerializeField]
	private TextMeshProUGUI diamondCostTxt;

	[Header("兑换钻石按钮")]
	[SerializeField]
	private Button exchangebtn2;

	[Header("获得主线钻石显示")]
	[SerializeField]
	private TextMeshProUGUI GetDiamondTxt;

	[Header("主线金钱显示")]
	[SerializeField]
	private TextMeshProUGUI missionCoinNumTmp;

	[Header("主线钻石显示")]
	[SerializeField]
	private TextMeshProUGUI diamondNumTmp;

	[Header("mask遮罩")]
	[SerializeField]
	private GameObject mask;

	[Header("钻石兑换条件提示")]
	[SerializeField]
	private TextMeshProUGUI duihuanTip;

	[Header("关闭兑换界面")]
	[SerializeField]
	private Button backBtn;

	private void Start()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		Button obj = exchangebtn1;
		if (obj != null)
		{
			((UnityEvent)obj.onClick).AddListener((UnityAction)delegate
			{
				SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
				OnExchangeCoin();
			});
		}
		Button obj2 = exchangebtn2;
		if (obj2 != null)
		{
			((UnityEvent)obj2.onClick).AddListener((UnityAction)delegate
			{
				SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
				OnExchangeDiamond();
			});
		}
		Button obj3 = backBtn;
		if (obj3 != null)
		{
			((UnityEvent)obj3.onClick).AddListener((UnityAction)delegate
			{
				SoundManager.Instance?.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
				((Component)this).gameObject.SetActive(false);
			});
		}
		RefreshPanel();
	}

	private void OnEnable()
	{
		RefreshPanel();
	}

	public void RefreshPanel()
	{
		double value = AuctionDataManager.Instance?.GetAuctionCoin() ?? 0.0;
		if ((Object)(object)coinTxt != (Object)null)
		{
			((TMP_Text)coinTxt).text = NumberFormatter.FormatNumber(value);
		}
		int num = GameDataManager.Instance?.GetGameData()?.auctionCoinExchangeCount ?? 1;
		int num2 = GameDataManager.Instance?.GetGameData()?.auctionDiamondExchangeCount ?? 1;
		float num3 = ConfigManager.Instance?.GetCommonFactor(50) ?? 0f;
		float num4 = ConfigManager.Instance?.GetCommonFactor(54) ?? 0f;
		float num5 = ConfigManager.Instance?.GetCommonFactor(51) ?? 0f;
		float num6 = ConfigManager.Instance?.GetCommonFactor(55) ?? 0f;
		double value2 = (double)num4 * Math.Pow(1f + num3, num - 1);
		double value3 = (double)num6 * Math.Pow(1f + num5, num2 - 1);
		if ((Object)(object)coinCostTxt != (Object)null)
		{
			((TMP_Text)coinCostTxt).text = NumberFormatter.FormatNumber(value2);
		}
		if ((Object)(object)diamondCostTxt != (Object)null)
		{
			((TMP_Text)diamondCostTxt).text = NumberFormatter.FormatNumber(value3);
		}
		float num7 = ConfigManager.Instance?.GetCommonFactor(56) ?? 0f;
		double num8 = SingletonMono<UpgradeManager>.Instance?.GetAverageGoldPerMinuteFromLast30Seconds() ?? 0.0;
		double value4 = (double)num7 * num8;
		if ((Object)(object)GetCoinTxt != (Object)null)
		{
			((TMP_Text)GetCoinTxt).text = NumberFormatter.FormatNumber(value4);
		}
		double value5 = ConfigManager.Instance?.GetCommonFactor(57) ?? 0f;
		if ((Object)(object)GetDiamondTxt != (Object)null)
		{
			((TMP_Text)GetDiamondTxt).text = NumberFormatter.FormatNumber(value5);
		}
		float num9 = ConfigManager.Instance?.GetCommonFactor(49) ?? 0f;
		bool flag = (float)(GameDataManager.Instance?.GetCurrentMainMissionLevel() ?? 0) >= num9;
		if ((Object)(object)exchangebtn2 != (Object)null)
		{
			((Component)exchangebtn2).gameObject.SetActive(flag);
		}
		if ((Object)(object)duihuanTip != (Object)null)
		{
			((Component)duihuanTip).gameObject.SetActive(!flag);
			((TMP_Text)duihuanTip).text = $"主线等级需达到{num9}级";
		}
		if ((Object)(object)mask != (Object)null)
		{
			mask.gameObject.SetActive(!flag);
		}
		if ((Object)(object)missionCoinNumTmp != (Object)null)
		{
			((TMP_Text)missionCoinNumTmp).text = NumberFormatter.FormatNumber(SingletonMono<UpgradeManager>.Instance.TotalGold);
		}
		if ((Object)(object)diamondNumTmp != (Object)null)
		{
			((TMP_Text)diamondNumTmp).text = NumberFormatter.FormatNumber(GameDataManager.Instance?.GetDiamond() ?? 0);
		}
	}

	private void OnExchangeCoin()
	{
		int num = GameDataManager.Instance?.GetGameData()?.auctionCoinExchangeCount ?? 1;
		float num2 = ConfigManager.Instance?.GetCommonFactor(50) ?? 0f;
		double num3 = (double)(ConfigManager.Instance?.GetCommonFactor(54) ?? 0f) * Math.Pow(1f + num2, num - 1);
		double num4 = AuctionDataManager.Instance?.GetAuctionCoin() ?? 0.0;
		if (num4 < num3)
		{
			BroadcastManager.ShowPopUpTip("拍卖金币不足");
			return;
		}
		if (num3 <= 0.0)
		{
			BroadcastManager.ShowPopUpTip("兑换数量无效");
			return;
		}
		float num5 = ConfigManager.Instance?.GetCommonFactor(56) ?? 0f;
		double num6 = SingletonMono<UpgradeManager>.Instance?.GetAverageGoldPerMinuteFromLast30Seconds() ?? 0.0;
		double a = (double)num5 * num6;
		a = Math.Ceiling(a);
		AuctionDataManager.Instance?.SetAuctionCoin(num4 - num3);
		SingletonMono<UpgradeManager>.Instance?.AddGoldInternal(a);
		GameData gameData = GameDataManager.Instance?.GetGameData();
		if (gameData != null)
		{
			gameData.auctionCoinExchangeCount++;
			GameDataManager.Instance?.SaveGameDataOnly();
		}
		RefreshPanel();
		AuctionDataManager.Instance?.auctionSelect.RefreshPanel();
		BroadcastManager.ShowPopUpTip("兑换完成");
	}

	private void OnExchangeDiamond()
	{
		float num = ConfigManager.Instance?.GetCommonFactor(49) ?? 0f;
		if ((float)(GameDataManager.Instance?.GetCurrentMainMissionLevel() ?? 0) < num)
		{
			BroadcastManager.ShowPopUpTip($"成长等级需达到{num}才能兑换钻石");
			return;
		}
		int num2 = GameDataManager.Instance?.GetGameData()?.auctionDiamondExchangeCount ?? 1;
		float num3 = ConfigManager.Instance?.GetCommonFactor(51) ?? 0f;
		double num4 = (double)(ConfigManager.Instance?.GetCommonFactor(55) ?? 0f) * Math.Pow(1f + num3, num2 - 1);
		double num5 = AuctionDataManager.Instance?.GetAuctionCoin() ?? 0.0;
		if (num5 < num4)
		{
			BroadcastManager.ShowPopUpTip("拍卖金币不足");
			return;
		}
		if (num4 <= 0.0)
		{
			BroadcastManager.ShowPopUpTip("兑换数量无效");
			return;
		}
		double a = ConfigManager.Instance?.GetCommonFactor(57) ?? 0f;
		a = Math.Ceiling(a);
		AuctionDataManager.Instance?.SetAuctionCoin(num5 - num4);
		GameDataManager.Instance?.AddDiamond((long)a);
		GameData gameData = GameDataManager.Instance?.GetGameData();
		if (gameData != null)
		{
			gameData.auctionDiamondExchangeCount++;
			GameDataManager.Instance?.SaveGameDataOnly();
		}
		RefreshPanel();
		AuctionDataManager.Instance?.auctionSelect.RefreshPanel();
		BroadcastManager.ShowPopUpTip("兑换完成");
	}

	private void OnDestroy()
	{
		Button obj = exchangebtn1;
		if (obj != null)
		{
			((UnityEventBase)obj.onClick).RemoveAllListeners();
		}
		Button obj2 = exchangebtn2;
		if (obj2 != null)
		{
			((UnityEventBase)obj2.onClick).RemoveAllListeners();
		}
	}
}

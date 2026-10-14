using System;
using P8SDKTT;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Module.UI;

public class SubscribeUI : MonoBehaviour
{
	[Header("跳转按钮")]
	[SerializeField]
	private Button jumpButton;

	[Header("获取按钮")]
	[SerializeField]
	private Button getButton;

	[Header("关闭按钮")]
	[SerializeField]
	private Button closeBtn;

	[Header("关闭按钮")]
	[SerializeField]
	private Button backgroundBtn;

	[SerializeField]
	private TextMeshProUGUI incomeText;

	[SerializeField]
	private TextMeshProUGUI incomeText2;

	[Header("马甲包特殊显示")]
	[SerializeField]
	private GameObject[] normalDisplays;

	[SerializeField]
	private GameObject[] specialDisplays0;

	[SerializeField]
	private GameObject[] specialDisplays1;

	private static string Subscribe_HAS_GET_KEY = "Subscribe_HAS_GET_M1B";

	private Action _onSubscribeSuccess;

	private void Awake()
	{
		AddButtonListeners();
		((TMP_Text)incomeText).text = "";
		((TMP_Text)incomeText2).text = "";
		Button obj = getButton;
		if (obj != null)
		{
			((Component)obj).gameObject.SetActive(false);
		}
	}

	private void Start()
	{
		UpdateSpecialDisplays();
	}

	private void UpdateSpecialDisplays()
	{
		string obj = (((Object)(object)FakeVideoManager.Instance != (Object)null) ? FakeVideoManager.Instance.appId : "");
		bool flag = obj == "biligame7d76926da6f5d9dc";
		bool flag2 = obj == "biligameccdab369d33cff71";
		bool active = !flag && !flag2;
		GameObject[] array;
		if (normalDisplays != null)
		{
			array = normalDisplays;
			foreach (GameObject val in array)
			{
				if ((Object)(object)val != (Object)null)
				{
					val.SetActive(active);
				}
			}
		}
		if (specialDisplays0 != null)
		{
			array = specialDisplays0;
			foreach (GameObject val2 in array)
			{
				if ((Object)(object)val2 != (Object)null)
				{
					val2.SetActive(flag);
				}
			}
		}
		if (specialDisplays1 == null)
		{
			return;
		}
		array = specialDisplays1;
		foreach (GameObject val3 in array)
		{
			if ((Object)(object)val3 != (Object)null)
			{
				val3.SetActive(flag2);
			}
		}
	}

	private void AddButtonListeners()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		if ((Object)(object)jumpButton != (Object)null)
		{
			((UnityEvent)jumpButton.onClick).AddListener(new UnityAction(OnJumpButtonClicked));
		}
		if ((Object)(object)getButton != (Object)null)
		{
			((UnityEvent)getButton.onClick).AddListener(new UnityAction(OnGetButtonClicked));
		}
		if ((Object)(object)closeBtn != (Object)null)
		{
			((UnityEvent)closeBtn.onClick).AddListener(new UnityAction(OnBackgroundButtonClicked));
		}
		if ((Object)(object)backgroundBtn != (Object)null)
		{
			((UnityEvent)backgroundBtn.onClick).AddListener(new UnityAction(OnBackgroundButtonClicked));
		}
	}

	private void OnJumpButtonClicked()
	{
		Debug.Log((object)"[subscribeUI] 跳转按钮被点击");
		TTFeedDirectPlayManager.Instance.ZhuDongRequestFeedSubscribe(delegate
		{
			P8SDK.Log("订阅成功");
			((Component)jumpButton).gameObject.SetActive(false);
			Button obj = getButton;
			if (obj != null)
			{
				((Component)obj).gameObject.SetActive(true);
			}
			_onSubscribeSuccess?.Invoke();
		}, delegate
		{
			P8SDK.Log("订阅失败");
		});
	}

	private void OnGetButtonClicked()
	{
		Debug.Log((object)"[subscribeUI] 获取按钮被点击");
		CloseSubscribeUI();
		double val = ConfigManager.Instance.GetCommonFactor(22);
		double averageGoldPerMinuteFromLast30Seconds = SingletonMono<UpgradeManager>.Instance.GetAverageGoldPerMinuteFromLast30Seconds();
		double amount = Math.Max(val, averageGoldPerMinuteFromLast30Seconds * 5.0);
		SingletonMono<UpgradeManager>.Instance.AddGoldInternal(amount);
		GameDataManager.Instance.AddDiamond(5L);
		PlayerPrefs.SetInt(Subscribe_HAS_GET_KEY, 2);
		PlayerPrefs.Save();
	}

	private void OnBackgroundButtonClicked()
	{
		Debug.Log((object)"[subscribeUI] 背景按钮被点击，关闭UI");
		CloseSubscribeUI();
	}

	public void OpenSubscribeUI(Action onSubscribeSuccess = null)
	{
		((Component)this).gameObject.SetActive(true);
		_onSubscribeSuccess = onSubscribeSuccess;
		switch (PlayerPrefs.GetInt(Subscribe_HAS_GET_KEY, 0))
		{
		case 0:
		{
			Button obj5 = jumpButton;
			if (obj5 != null)
			{
				((Component)obj5).gameObject.SetActive(true);
			}
			Button obj6 = getButton;
			if (obj6 != null)
			{
				((Component)obj6).gameObject.SetActive(false);
			}
			break;
		}
		case 1:
		{
			Button obj3 = jumpButton;
			if (obj3 != null)
			{
				((Component)obj3).gameObject.SetActive(false);
			}
			Button obj4 = getButton;
			if (obj4 != null)
			{
				((Component)obj4).gameObject.SetActive(true);
			}
			break;
		}
		default:
		{
			Button obj = jumpButton;
			if (obj != null)
			{
				((Component)obj).gameObject.SetActive(false);
			}
			Button obj2 = getButton;
			if (obj2 != null)
			{
				((Component)obj2).gameObject.SetActive(false);
			}
			break;
		}
		}
		double val = ConfigManager.Instance.GetCommonFactor(22);
		double averageGoldPerMinuteFromLast30Seconds = SingletonMono<UpgradeManager>.Instance.GetAverageGoldPerMinuteFromLast30Seconds();
		double value = Math.Max(val, averageGoldPerMinuteFromLast30Seconds * 5.0);
		((TMP_Text)incomeText).text = NumberFormatter.FormatNumber(value);
		((TMP_Text)incomeText2).text = "5";
		UpdateSpecialDisplays();
	}

	public void CloseSubscribeUI()
	{
		((Component)this).gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		if ((Object)(object)jumpButton != (Object)null)
		{
			((UnityEvent)jumpButton.onClick).RemoveListener(new UnityAction(OnJumpButtonClicked));
		}
		if ((Object)(object)closeBtn != (Object)null)
		{
			((UnityEvent)closeBtn.onClick).RemoveListener(new UnityAction(OnBackgroundButtonClicked));
		}
		if ((Object)(object)backgroundBtn != (Object)null)
		{
			((UnityEvent)backgroundBtn.onClick).RemoveListener(new UnityAction(OnBackgroundButtonClicked));
		}
	}
}

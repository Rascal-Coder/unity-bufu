using System;
using System.Collections.Generic;
using P8SDKTT;
using TMPro;
using TTSDK;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Module.UI;

public class AddDeskUI : MonoBehaviour
{
	[Header("跳转按钮")]
	[SerializeField]
	private Button jumpButton;

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

	private static string ADD_DESK_HAS_GET_KEY = "ADD_DESK_HAS_GET";

	private void Awake()
	{
		AddButtonListeners();
		((TMP_Text)incomeText).text = "";
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
		Debug.Log((object)"[AddDeskUI] 跳转按钮被点击");
		CloseDesk();
		P8SDK.AddShortcut();
	}

	private void OnGetButtonClicked()
	{
		Debug.Log((object)"[AddDeskUI] 获取按钮被点击");
		if (PlayerPrefs.HasKey(ADD_DESK_HAS_GET_KEY))
		{
			Debug.Log((object)"已获取过奖励");
			return;
		}
		Debug.Log((object)"获取奖励");
		PlayerPrefs.SetInt(ADD_DESK_HAS_GET_KEY, 1);
		PlayerPrefs.Save();
		int num = Mathf.CeilToInt(ConfigManager.Instance.GetCommonFactor(22));
		double averageGoldPerMinuteFromLast30Seconds = SingletonMono<UpgradeManager>.Instance.GetAverageGoldPerMinuteFromLast30Seconds();
		double item = Math.Max(num, averageGoldPerMinuteFromLast30Seconds * 5.0);
		UIManager.Instance.ShowRewardBoxUI(new List<double> { 1.0, item }, isAddItem: true);
		CloseDesk();
	}

	private void OnBackgroundButtonClicked()
	{
		Debug.Log((object)"[AddDeskUI] 背景按钮被点击，关闭UI");
		CloseDesk();
	}

	public void OpenDesk()
	{
		((Component)this).gameObject.SetActive(true);
		Button obj = jumpButton;
		if (obj != null)
		{
			((Component)obj).gameObject.SetActive(true);
		}
		Button obj2 = getButton;
		if (obj2 != null)
		{
			((Component)obj2).gameObject.SetActive(false);
		}
		if ((Object)(object)jumpButton != (Object)null)
		{
			((Selectable)jumpButton).interactable = true;
		}
		if ((Object)(object)getButton != (Object)null)
		{
			((Selectable)getButton).interactable = true;
		}
		TT.CheckShortcut((Action<bool>)delegate(bool isExist)
		{
			if (isExist)
			{
				Debug.Log((object)"已存在快捷方式");
				Button obj3 = getButton;
				if (obj3 != null)
				{
					((Component)obj3).gameObject.SetActive(true);
				}
				Button obj4 = jumpButton;
				if (obj4 != null)
				{
					((Component)obj4).gameObject.SetActive(false);
				}
				int num = Mathf.CeilToInt(ConfigManager.Instance.GetCommonFactor(22));
				double averageGoldPerMinuteFromLast30Seconds = SingletonMono<UpgradeManager>.Instance.GetAverageGoldPerMinuteFromLast30Seconds();
				double value = Math.Max(num, averageGoldPerMinuteFromLast30Seconds * 5.0);
				((TMP_Text)incomeText).text = NumberFormatter.FormatNumber(value);
			}
			else
			{
				Debug.Log((object)"不存在快捷方式");
				Button obj5 = getButton;
				if (obj5 != null)
				{
					((Component)obj5).gameObject.SetActive(false);
				}
				Button obj6 = jumpButton;
				if (obj6 != null)
				{
					((Component)obj6).gameObject.SetActive(true);
				}
			}
		});
	}

	public void CloseDesk()
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
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
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
		if ((Object)(object)getButton != (Object)null)
		{
			((UnityEvent)getButton.onClick).RemoveListener(new UnityAction(OnGetButtonClicked));
		}
	}
}

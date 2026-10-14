using System;
using P8SDKTT;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Module.UI;

public class SideBarUI : MonoBehaviour
{
	[Header("跳转按钮")]
	[SerializeField]
	private Button jumpButton;

	[Header("关闭按钮")]
	[SerializeField]
	private Button closeBtn;

	[Header("关闭按钮")]
	[SerializeField]
	private Button backgroundBtn;

	[SerializeField]
	private TextMeshProUGUI incomeText;

	[Header("马甲包特殊显示")]
	[SerializeField]
	private GameObject[] normalDisplays;

	[SerializeField]
	private GameObject[] specialDisplays0;

	[SerializeField]
	private GameObject[] specialDisplays1;

	private void Awake()
	{
		AddButtonListeners();
		((TMP_Text)incomeText).text = "";
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
		if ((Object)(object)jumpButton != (Object)null)
		{
			((UnityEvent)jumpButton.onClick).AddListener(new UnityAction(OnJumpButtonClicked));
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
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)"[SideBarUI] 跳转按钮被点击");
		CloseSideBar();
		P8SDK.NavigateToScene(new CallbackOption
		{
			success = delegate
			{
				P8SDK.Log("跳转侧边栏成功");
			},
			fail = delegate
			{
				P8SDK.Log("跳转侧边栏失败");
			}
		});
	}

	private void OnBackgroundButtonClicked()
	{
		Debug.Log((object)"[SideBarUI] 背景按钮被点击，关闭UI");
		CloseSideBar();
	}

	public void OpenSideBar()
	{
		((Component)this).gameObject.SetActive(true);
		if ((Object)(object)jumpButton != (Object)null)
		{
			((Selectable)jumpButton).interactable = true;
		}
		double val = ConfigManager.Instance.GetCommonFactor(23);
		double averageGoldPerMinuteFromLast30Seconds = SingletonMono<UpgradeManager>.Instance.GetAverageGoldPerMinuteFromLast30Seconds();
		double value = Math.Max(val, averageGoldPerMinuteFromLast30Seconds * 5.0);
		((TMP_Text)incomeText).text = NumberFormatter.FormatNumber(value);
		UpdateSpecialDisplays();
	}

	public void CloseSideBar()
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

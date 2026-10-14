using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UpgradeItemPanel : MonoBehaviour
{
	[Header("背景图片")]
	[SerializeField]
	private Image[] worldbackImage;

	[Header("按钮图片")]
	[SerializeField]
	private Image button1Image;

	[SerializeField]
	private Image button2Image;

	[SerializeField]
	private Image button3Image;

	[SerializeField]
	private Image[] worldbutton1Image;

	[Header("按钮")]
	[SerializeField]
	private Button button1;

	[SerializeField]
	private Button button2;

	[SerializeField]
	private Button button3;

	[SerializeField]
	private GameObject[] worldbutton1;

	[Header("面板")]
	[SerializeField]
	private GameObject panel1;

	[SerializeField]
	private GameObject panel2;

	[SerializeField]
	private GameObject panel3;

	[SerializeField]
	private RectTransform lockImg;

	[SerializeField]
	private RectTransform yijiesuoImg;

	[SerializeField]
	private RectTransform lockImg3;

	[SerializeField]
	private RectTransform yijiesuoImg3;

	[Header("引导动画")]
	[SerializeField]
	private RectTransform guideFinger;

	private const string BUTTON_CLICKED_KEY_PREFIX = "button_clicked_";

	private bool _hasPanel3Config = true;

	public bool isPanel1Active
	{
		get
		{
			if ((Object)(object)panel1 != (Object)null)
			{
				return panel1.activeSelf;
			}
			return false;
		}
	}

	public bool isPanel2Active
	{
		get
		{
			if ((Object)(object)panel2 != (Object)null)
			{
				return panel2.activeSelf;
			}
			return false;
		}
	}

	public bool isPanel3Active
	{
		get
		{
			if ((Object)(object)panel3 != (Object)null)
			{
				return panel3.activeSelf;
			}
			return false;
		}
	}

	public event Action<int> OnRefreshUpgradeItemsForPanel;

	private void Awake()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		if ((Object)(object)button1 != (Object)null)
		{
			((UnityEvent)button1.onClick).AddListener(new UnityAction(OnButton1Click));
		}
		if ((Object)(object)button2 != (Object)null)
		{
			((UnityEvent)button2.onClick).AddListener(new UnityAction(OnButton2Click));
		}
		if ((Object)(object)button3 != (Object)null)
		{
			((UnityEvent)button3.onClick).AddListener(new UnityAction(OnButton3Click));
		}
	}

	public void SetButtonInteractable(int index, bool isInteractable)
	{
		switch (index)
		{
		case 1:
			if ((Object)(object)button1 != (Object)null)
			{
				((Selectable)button1).interactable = isInteractable;
			}
			break;
		case 2:
			if ((Object)(object)button2 != (Object)null)
			{
				((Behaviour)button2).enabled = true;
			}
			if ((Object)(object)lockImg != (Object)null)
			{
				((Component)lockImg).gameObject.SetActive(!isInteractable);
			}
			if ((Object)(object)yijiesuoImg != (Object)null)
			{
				((Component)yijiesuoImg).gameObject.SetActive(isInteractable && !IsButtonClicked(0));
			}
			break;
		case 3:
		{
			bool flag = _hasPanel3Config && isInteractable;
			if ((Object)(object)button3 != (Object)null)
			{
				((Behaviour)button3).enabled = _hasPanel3Config;
			}
			if ((Object)(object)lockImg3 != (Object)null)
			{
				((Component)lockImg3).gameObject.SetActive(_hasPanel3Config && !isInteractable);
			}
			if ((Object)(object)yijiesuoImg3 != (Object)null)
			{
				((Component)yijiesuoImg3).gameObject.SetActive(flag && !IsButtonClicked(9));
			}
			break;
		}
		}
	}

	private void OnEnable()
	{
		RefreshForCurrentWorld();
		ShowPanel1();
	}

	public void RefreshForCurrentWorld()
	{
		_hasPanel3Config = true;
		if (ConfigManager.HasInstance() && (Object)(object)GameDataManager.Instance != (Object)null)
		{
			int currentWorldId = GameDataManager.Instance.GetCurrentWorldId();
			_hasPanel3Config = ConfigManager.Instance.GetAllUpgrade3ByWorld(currentWorldId).Count > 0;
		}
		if ((Object)(object)button3 != (Object)null)
		{
			((Component)button3).gameObject.SetActive(_hasPanel3Config);
		}
		if (!_hasPanel3Config)
		{
			RectTransform obj = lockImg3;
			if (obj != null)
			{
				((Component)obj).gameObject.SetActive(false);
			}
			RectTransform obj2 = yijiesuoImg3;
			if (obj2 != null)
			{
				((Component)obj2).gameObject.SetActive(false);
			}
			Image obj3 = button3Image;
			if (obj3 != null)
			{
				((Component)obj3).gameObject.SetActive(false);
			}
			if (isPanel3Active)
			{
				ShowPanel1();
			}
		}
		UpdateWorldButtonDisplay();
	}

	private void UpdateWorldButtonDisplay()
	{
		int num = ((!((Object)(object)GameDataManager.Instance != (Object)null)) ? 1 : GameDataManager.Instance.GetCurrentWorldId());
		if (worldbackImage != null && worldbackImage.Length != 0)
		{
			int num2 = Mathf.Clamp(num - 1, 0, worldbackImage.Length - 1);
			for (int i = 0; i < worldbackImage.Length; i++)
			{
				if ((Object)(object)worldbackImage[i] != (Object)null)
				{
					((Component)worldbackImage[i]).gameObject.SetActive(i == num2);
				}
			}
		}
		if (worldbutton1 != null && worldbutton1.Length != 0)
		{
			int num3 = Mathf.Clamp(num - 1, 0, worldbutton1.Length - 1);
			for (int j = 0; j < worldbutton1.Length; j++)
			{
				if ((Object)(object)worldbutton1[j] != (Object)null)
				{
					worldbutton1[j].SetActive(j == num3);
				}
			}
		}
		bool flag = isPanel1Active;
		bool flag2 = false;
		if (worldbutton1Image != null && worldbutton1Image.Length != 0)
		{
			int num4 = Mathf.Clamp(num - 1, 0, worldbutton1Image.Length - 1);
			for (int k = 0; k < worldbutton1Image.Length; k++)
			{
				if ((Object)(object)worldbutton1Image[k] != (Object)null)
				{
					((Component)worldbutton1Image[k]).gameObject.SetActive(flag && k == num4);
					if ((Object)(object)button1Image != (Object)null && (Object)(object)worldbutton1Image[k] == (Object)(object)button1Image)
					{
						flag2 = true;
					}
				}
			}
		}
		if ((Object)(object)button1Image != (Object)null && !flag2)
		{
			int num5 = ((worldbutton1Image != null && worldbutton1Image.Length != 0) ? Mathf.Clamp(num - 1, 0, worldbutton1Image.Length - 1) : 0);
			((Component)button1Image).gameObject.SetActive(flag && num5 == 0);
		}
	}

	private bool IsButtonClicked(int buttonIndex)
	{
		return PlayerPrefs.GetInt("button_clicked_" + buttonIndex, 0) == 1;
	}

	public void CheckUnLock()
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		RefreshForCurrentWorld();
		int currentMainMissionLevel = GameDataManager.Instance.GetCurrentMainMissionLevel();
		int num = (int)ConfigManager.Instance.GetCommonFactor(18);
		if (currentMainMissionLevel > num && !IsButtonClicked(0) && (Object)(object)guideFinger != (Object)null && (Object)(object)lockImg != (Object)null)
		{
			((Component)guideFinger).gameObject.SetActive(true);
			((Component)guideFinger).transform.position = ((Component)lockImg).transform.position;
		}
		int num2 = (int)ConfigManager.Instance.GetCommonFactor(80);
		if (currentMainMissionLevel > num2 && _hasPanel3Config && !IsButtonClicked(9) && (Object)(object)yijiesuoImg3 != (Object)null)
		{
			((Component)yijiesuoImg3).gameObject.SetActive(true);
		}
	}

	public GameObject GetPanelByIndex(int index)
	{
		return (GameObject)(index switch
		{
			1 => panel1, 
			2 => panel2, 
			3 => panel3, 
			_ => null, 
		});
	}

	private void Start()
	{
		ShowPanel1();
	}

	private void OnButton1Click()
	{
		ShowPanel1();
	}

	private void OnButton2Click()
	{
		if (ConfigManager.HasInstance() && (Object)(object)GameDataManager.Instance != (Object)null)
		{
			int currentMainMissionLevel = GameDataManager.Instance.GetCurrentMainMissionLevel();
			int num = (int)ConfigManager.Instance.GetCommonFactor(18);
			if (currentMainMissionLevel <= num)
			{
				BroadcastManager.ShowPopUpTip($"完成成长等级{num}后解锁");
				return;
			}
		}
		ShowPanel2();
	}

	private bool IsPanel3Unlocked()
	{
		if (!_hasPanel3Config)
		{
			return false;
		}
		if (ConfigManager.HasInstance() && (Object)(object)GameDataManager.Instance != (Object)null)
		{
			int currentMainMissionLevel = GameDataManager.Instance.GetCurrentMainMissionLevel();
			int num = (int)ConfigManager.Instance.GetCommonFactor(80);
			return currentMainMissionLevel > num;
		}
		return false;
	}

	private void OnButton3Click()
	{
		if (!IsPanel3Unlocked())
		{
			int num = (int)ConfigManager.Instance.GetCommonFactor(80);
			BroadcastManager.ShowPopUpTip($"完成成长等级{num}后解锁");
		}
		else
		{
			ShowPanel3();
		}
	}

	private void ShowPanel1()
	{
		if ((Object)(object)panel1 != (Object)null)
		{
			panel1.SetActive(true);
			this.OnRefreshUpgradeItemsForPanel?.Invoke(1);
		}
		if ((Object)(object)panel2 != (Object)null)
		{
			panel2.SetActive(false);
		}
		if ((Object)(object)panel3 != (Object)null)
		{
			panel3.SetActive(false);
		}
		if ((Object)(object)button2Image != (Object)null)
		{
			((Component)button2Image).gameObject.SetActive(false);
		}
		if ((Object)(object)button3Image != (Object)null)
		{
			((Component)button3Image).gameObject.SetActive(false);
		}
		UpdateWorldButtonDisplay();
	}

	private void ShowPanel2()
	{
		if ((Object)(object)panel1 != (Object)null)
		{
			panel1.SetActive(false);
		}
		if ((Object)(object)panel2 != (Object)null)
		{
			panel2.SetActive(true);
			this.OnRefreshUpgradeItemsForPanel?.Invoke(2);
		}
		if ((Object)(object)panel3 != (Object)null)
		{
			panel3.SetActive(false);
		}
		if ((Object)(object)button2Image != (Object)null)
		{
			((Component)button2Image).gameObject.SetActive(true);
		}
		if ((Object)(object)button3Image != (Object)null)
		{
			((Component)button3Image).gameObject.SetActive(false);
		}
		UpdateWorldButtonDisplay();
		if (!IsButtonClicked(0))
		{
			RectTransform obj = guideFinger;
			if (obj != null)
			{
				((Component)obj).gameObject.SetActive(false);
			}
			RectTransform obj2 = yijiesuoImg;
			if (obj2 != null)
			{
				((Component)obj2).gameObject.SetActive(false);
			}
			PlayerPrefs.SetInt("button_clicked_" + 0, 1);
			PlayerPrefs.Save();
		}
	}

	private void ShowPanel3()
	{
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		RefreshForCurrentWorld();
		if (!_hasPanel3Config)
		{
			ShowPanel1();
			return;
		}
		if ((Object)(object)panel1 != (Object)null)
		{
			panel1.SetActive(false);
		}
		if ((Object)(object)panel2 != (Object)null)
		{
			panel2.SetActive(false);
		}
		if ((Object)(object)panel3 != (Object)null)
		{
			panel3.SetActive(true);
			this.OnRefreshUpgradeItemsForPanel?.Invoke(3);
		}
		if ((Object)(object)button2Image != (Object)null)
		{
			((Component)button2Image).gameObject.SetActive(false);
		}
		if ((Object)(object)button3Image != (Object)null)
		{
			((Component)button3Image).gameObject.SetActive(true);
		}
		UpdateWorldButtonDisplay();
		if (!IsButtonClicked(9))
		{
			if ((Object)(object)guideFinger != (Object)null && (Object)(object)lockImg3 != (Object)null && ((Component)guideFinger).transform.position == ((Component)lockImg3).transform.position)
			{
				((Component)guideFinger).gameObject.SetActive(false);
			}
			RectTransform obj = yijiesuoImg3;
			if (obj != null)
			{
				((Component)obj).gameObject.SetActive(false);
			}
			PlayerPrefs.SetInt("button_clicked_" + 9, 1);
			PlayerPrefs.Save();
		}
	}

	private void OnDestroy()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		if ((Object)(object)button1 != (Object)null)
		{
			((UnityEvent)button1.onClick).RemoveListener(new UnityAction(OnButton1Click));
		}
		if ((Object)(object)button2 != (Object)null)
		{
			((UnityEvent)button2.onClick).RemoveListener(new UnityAction(OnButton2Click));
		}
		if ((Object)(object)button3 != (Object)null)
		{
			((UnityEvent)button3.onClick).RemoveListener(new UnityAction(OnButton3Click));
		}
	}
}

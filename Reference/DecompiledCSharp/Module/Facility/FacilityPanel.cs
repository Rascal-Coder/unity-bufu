using UI.Component;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Module.Facility;

public class FacilityPanel : MonoBehaviour
{
	[Header("底图")]
	[SerializeField]
	private Image[] bgImages;

	[SerializeField]
	private Image[] bgTileImages;

	[Header("Tab按钮")]
	[SerializeField]
	private Button helperTabBtn;

	[SerializeField]
	private Button deskTabBtn;

	[Header("Tab选中状态")]
	[SerializeField]
	private GameObject helperTabSelected;

	[SerializeField]
	private GameObject deskTabSelected;

	[Header("内容面板")]
	[SerializeField]
	private HelperPanel helperPanel;

	[SerializeField]
	private DeskPanel deskPanel;

	[SerializeField]
	private Button getDiamondBtn;

	[SerializeField]
	private CountdownTimer diamondTimer;

	[Header("选项卡红点")]
	[SerializeField]
	private GameObject helperTabRedDot;

	[SerializeField]
	private GameObject deskTabRedDot;

	[Header("BG2 Scroll")]
	private Image _bg2;

	[SerializeField]
	private Vector2 _bg2ScrollSpeed = new Vector2(20f, 20f);

	private RectTransform _bg2Rect;

	private Vector2 _bg2StartAnchoredPos;

	private float _bg2TileWidth;

	private float _bg2TileHeight;

	private bool _bg2Inited;

	private Sprite _bg2CachedSprite;

	private float _bg2CachedPixelsPerUnit;

	private void Awake()
	{
		InitUI();
	}

	private void InitUI()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		((UnityEvent)helperTabBtn.onClick).AddListener(new UnityAction(OnHelperTabClicked));
		((UnityEvent)deskTabBtn.onClick).AddListener(new UnityAction(OnDeskTabClicked));
		((UnityEvent)getDiamondBtn.onClick).AddListener(new UnityAction(OnGetDiamondBtnClicked));
		ShowHelperPanel();
	}

	private void OnEnable()
	{
		float supportRemainingTime = GameDataManager.Instance.GetSupportRemainingTime(DialogType.money);
		if (supportRemainingTime > 0f)
		{
			CountdownTimer countdownTimer = diamondTimer;
			if (countdownTimer != null)
			{
				((Component)countdownTimer).gameObject.SetActive(true);
			}
			diamondTimer?.StartCountdown(supportRemainingTime);
		}
		FacilityRedDot.CheckAllRedDots();
	}

	public void RefreshTabRedDots(bool isHelperRed, bool isDeskRed)
	{
		if ((Object)(object)helperTabRedDot != (Object)null)
		{
			helperTabRedDot.SetActive(isHelperRed);
		}
		if ((Object)(object)deskTabRedDot != (Object)null)
		{
			deskTabRedDot.SetActive(isDeskRed);
		}
	}

	public void OnOpen()
	{
		((Component)this).gameObject.SetActive(true);
		RefreshForCurrentWorld(forceHelperTab: true);
	}

	public void RefreshForCurrentWorld(bool forceHelperTab = false)
	{
		ShowHelperPanel();
		if ((Object)(object)helperPanel != (Object)null)
		{
			helperPanel.Initialize();
		}
		if ((Object)(object)deskPanel != (Object)null)
		{
			deskPanel.Initialize();
		}
		RefreshBgImages();
		FacilityRedDot.CheckAllRedDots();
	}

	private void RefreshBgImages()
	{
		int num = ((!((Object)(object)GameDataManager.Instance != (Object)null)) ? 1 : GameDataManager.Instance.GetCurrentWorldId());
		if (bgImages != null)
		{
			for (int i = 0; i < bgImages.Length; i++)
			{
				if ((Object)(object)bgImages[i] != (Object)null)
				{
					((Component)bgImages[i]).gameObject.SetActive(i == num - 1);
				}
			}
		}
		if (bgTileImages == null)
		{
			return;
		}
		for (int j = 0; j < bgTileImages.Length; j++)
		{
			if ((Object)(object)bgTileImages[j] != (Object)null)
			{
				((Component)bgTileImages[j]).gameObject.SetActive(j == num - 1);
				if (j == num - 1)
				{
					_bg2 = bgTileImages[j];
				}
			}
		}
	}

	public void OnClose()
	{
		((Component)this).gameObject.SetActive(false);
	}

	private void OnHelperTabClicked()
	{
		SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
		ShowHelperPanel();
	}

	private void OnDeskTabClicked()
	{
		SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
		ShowDeskPanel();
	}

	private void ShowHelperPanel()
	{
		helperTabSelected.gameObject.SetActive(true);
		deskTabSelected.gameObject.SetActive(false);
		((Component)helperPanel).gameObject.SetActive(true);
		((Component)deskPanel).gameObject.SetActive(false);
	}

	private void ShowDeskPanel()
	{
		helperTabSelected.gameObject.SetActive(false);
		deskTabSelected.gameObject.SetActive(true);
		((Component)helperPanel).gameObject.SetActive(false);
		((Component)deskPanel).gameObject.SetActive(true);
	}

	private void OnGetDiamondBtnClicked()
	{
		SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
		int num = Mathf.RoundToInt(ConfigManager.Instance.GetCommonFactor(25));
		UIManager.Instance.ShowSupportUI(DialogType.diamond, num, delegate
		{
			float seconds = ConfigManager.Instance.GetCommonFactor(32) * 60f;
			CountdownTimer countdownTimer = diamondTimer;
			if (countdownTimer != null)
			{
				((Component)countdownTimer).gameObject.SetActive(true);
			}
			diamondTimer?.StartCountdown(seconds);
			GameDataManager.Instance.RecordSupport(DialogType.diamond);
		});
	}

	private void OnDestroy()
	{
		((UnityEventBase)helperTabBtn.onClick).RemoveAllListeners();
		((UnityEventBase)deskTabBtn.onClick).RemoveAllListeners();
	}

	private void Update()
	{
		TickBg2Scroll();
	}

	private void InitBg2Scroll()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_bg2 == (Object)null)
		{
			return;
		}
		ref RectTransform bg2Rect = ref _bg2Rect;
		Transform transform = ((Component)_bg2).transform;
		bg2Rect = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		if ((Object)(object)_bg2Rect == (Object)null)
		{
			return;
		}
		_bg2StartAnchoredPos = _bg2Rect.anchoredPosition;
		_bg2TileWidth = 0f;
		_bg2TileHeight = 0f;
		_bg2CachedSprite = _bg2.sprite;
		_bg2CachedPixelsPerUnit = _bg2.pixelsPerUnit;
		Sprite sprite = _bg2.sprite;
		float num = _bg2.pixelsPerUnit;
		if (num <= 0.01f)
		{
			num = 100f;
		}
		Rect rect;
		if ((Object)(object)sprite != (Object)null)
		{
			rect = sprite.rect;
			_bg2TileWidth = ((Rect)(ref rect)).width / num;
			rect = sprite.rect;
			_bg2TileHeight = ((Rect)(ref rect)).height / num;
		}
		if (_bg2TileWidth <= 0.01f)
		{
			_bg2TileWidth = 0f;
		}
		if (_bg2TileHeight <= 0.01f)
		{
			_bg2TileHeight = 0f;
		}
		Transform parent = ((Transform)_bg2Rect).parent;
		RectTransform val = (RectTransform)(object)((parent is RectTransform) ? parent : null);
		if ((Object)(object)val != (Object)null && _bg2TileWidth > 0f && _bg2TileHeight > 0f)
		{
			rect = val.rect;
			float num2 = ((Rect)(ref rect)).width + _bg2TileWidth;
			rect = val.rect;
			float num3 = ((Rect)(ref rect)).height + _bg2TileHeight;
			rect = _bg2Rect.rect;
			if (((Rect)(ref rect)).width < num2)
			{
				_bg2Rect.SetSizeWithCurrentAnchors((Axis)0, num2);
			}
			rect = _bg2Rect.rect;
			if (((Rect)(ref rect)).height < num3)
			{
				_bg2Rect.SetSizeWithCurrentAnchors((Axis)1, num3);
			}
		}
		_bg2Inited = true;
	}

	private void TickBg2Scroll()
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		if (!((Behaviour)this).isActiveAndEnabled || (Object)(object)_bg2 == (Object)null)
		{
			return;
		}
		if (!_bg2Inited || (Object)(object)_bg2Rect == (Object)null || (Object)(object)_bg2CachedSprite != (Object)(object)_bg2.sprite || Mathf.Abs(_bg2CachedPixelsPerUnit - _bg2.pixelsPerUnit) > 0.001f)
		{
			InitBg2Scroll();
		}
		if (_bg2Inited && !((Object)(object)_bg2Rect == (Object)null))
		{
			Vector2 anchoredPosition = _bg2Rect.anchoredPosition;
			anchoredPosition += _bg2ScrollSpeed * Time.deltaTime;
			if (_bg2TileWidth > 0f)
			{
				float num = anchoredPosition.x - _bg2StartAnchoredPos.x;
				num = Mathf.Repeat(num, _bg2TileWidth);
				anchoredPosition.x = _bg2StartAnchoredPos.x + num;
			}
			if (_bg2TileHeight > 0f)
			{
				float num2 = anchoredPosition.y - _bg2StartAnchoredPos.y;
				num2 = Mathf.Repeat(num2, _bg2TileHeight);
				anchoredPosition.y = _bg2StartAnchoredPos.y + num2;
			}
			_bg2Rect.anchoredPosition = anchoredPosition;
		}
	}
}

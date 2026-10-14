using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

public class BroadcastManager : MonoBehaviour
{
	private static BroadcastManager _instance;

	private Transform _popupLayerTransform;

	private GameObject _textTemplate;

	[SerializeField]
	private float _moveSpeed = 150f;

	private Queue<TextMeshProUGUI> _activeBroadcasts = new Queue<TextMeshProUGUI>();

	private GameObject _popupTipPrefab;

	[SerializeField]
	private float _defaultShowDuration = 2f;

	[SerializeField]
	private float _fadeInDuration = 0.3f;

	[SerializeField]
	private float _fadeOutDuration = 0.5f;

	private const string TIP_TEXT_COMPONENT_NAME = "text";

	private const string TIP_BACKGROUND_COMPONENT_NAME = "bg";

	public static BroadcastManager Instance
	{
		get
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Expected O, but got Unknown
			if ((Object)(object)_instance == (Object)null)
			{
				GameObject val = new GameObject("BroadcastManager");
				_instance = val.AddComponent<BroadcastManager>();
				Object.DontDestroyOnLoad((Object)val);
				_instance.Initialize();
			}
			return _instance;
		}
	}

	private void Initialize()
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)UIManager.Instance != (Object)null)
		{
			_popupLayerTransform = UIManager.Instance.GetLayerTransform(UILayer.Popup);
		}
		if ((Object)(object)_popupLayerTransform == (Object)null)
		{
			Debug.LogError((object)"无法获取Popup层，请确保UIManager已初始化！");
			GameObject val = new GameObject("BroadcastContainer");
			_popupLayerTransform = val.transform;
			Object.DontDestroyOnLoad((Object)(object)val);
			Canvas obj = val.AddComponent<Canvas>();
			obj.renderMode = (RenderMode)0;
			obj.overrideSorting = true;
			obj.sortingOrder = 30;
			CanvasScaler obj2 = val.AddComponent<CanvasScaler>();
			obj2.uiScaleMode = (ScaleMode)1;
			obj2.referenceResolution = new Vector2(1920f, 1080f);
		}
		_textTemplate = new GameObject("BroadcastTextTemplate");
		_textTemplate.SetActive(false);
		_textTemplate.transform.SetParent(_popupLayerTransform);
		TextMeshProUGUI val2 = _textTemplate.AddComponent<TextMeshProUGUI>();
		((TMP_Text)val2).fontSize = 80f;
		((Graphic)val2).color = Color.yellow;
		((TMP_Text)val2).alignment = (TextAlignmentOptions)4097;
		((TMP_Text)val2).enableAutoSizing = false;
		((TMP_Text)val2).font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
		if ((Object)(object)((TMP_Text)val2).font != (Object)null && (Object)(object)((TMP_Asset)((TMP_Text)val2).font).material != (Object)null)
		{
			((TMP_Text)val2).fontSharedMaterial = ((TMP_Asset)((TMP_Text)val2).font).material;
		}
		RectTransform component = ((Component)val2).GetComponent<RectTransform>();
		component.sizeDelta = new Vector2(500f, 100f);
		component.anchorMin = new Vector2(0f, 0.5f);
		component.anchorMax = new Vector2(0f, 0.5f);
		component.pivot = new Vector2(0f, 0.5f);
		LoadTipPrefab();
	}

	private IEnumerator LoadTipPrefab()
	{
		AssetHandle handle = SingletonMono<ResManager>.Instance.LoadAssetAsyncHandle<GameObject>("Assets/GameRes/NewUI/Common/prefabs/tip.prefab");
		yield return handle;
		if ((int)((HandleBase)handle).Status == 2)
		{
			ref GameObject popupTipPrefab = ref _popupTipPrefab;
			Object assetObject = handle.AssetObject;
			popupTipPrefab = (GameObject)(object)((assetObject is GameObject) ? assetObject : null);
		}
		if ((Object)(object)_popupTipPrefab == (Object)null)
		{
			Debug.LogWarning((object)"无法找到提示预制体，将创建默认的预制体");
			CreateDefaultTipPrefab();
		}
		else
		{
			ValidateTipPrefab();
		}
	}

	private void CreateDefaultTipPrefab()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		_popupTipPrefab = new GameObject("tip");
		_popupTipPrefab.SetActive(false);
		GameObject val = new GameObject("bg");
		val.transform.SetParent(_popupTipPrefab.transform);
		((Graphic)val.AddComponent<Image>()).color = new Color(0f, 0f, 0f, 0.7f);
		RectTransform component = val.GetComponent<RectTransform>();
		component.anchorMin = Vector2.zero;
		component.anchorMax = Vector2.one;
		component.offsetMin = Vector2.zero;
		component.offsetMax = Vector2.zero;
		component.sizeDelta = new Vector2(400f, 100f);
		val.GetComponent<CanvasRenderer>();
		GameObject val2 = new GameObject("text");
		val2.transform.SetParent(val.transform);
		TextMeshProUGUI val3 = val2.AddComponent<TextMeshProUGUI>();
		((TMP_Text)val3).fontSize = 60f;
		((Graphic)val3).color = Color.yellow;
		((TMP_Text)val3).alignment = (TextAlignmentOptions)514;
		((TMP_Text)val3).font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
		if ((Object)(object)((TMP_Text)val3).font != (Object)null && (Object)(object)((TMP_Asset)((TMP_Text)val3).font).material != (Object)null)
		{
			((TMP_Text)val3).fontSharedMaterial = ((TMP_Asset)((TMP_Text)val3).font).material;
		}
		RectTransform component2 = ((Component)val3).GetComponent<RectTransform>();
		component2.anchorMin = Vector2.zero;
		component2.anchorMax = Vector2.one;
		component2.offsetMin = new Vector2(20f, 20f);
		component2.offsetMax = new Vector2(-20f, -20f);
	}

	private void ValidateTipPrefab()
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		Transform val = _popupTipPrefab.transform.Find("text");
		Transform val2 = _popupTipPrefab.transform.Find("bg");
		if ((Object)(object)val == (Object)null)
		{
			Debug.LogWarning((object)"提示预制体中找不到名为'text'的文本对象，将创建默认文本组件");
			CreateDefaultTipText();
		}
		else if ((Object)(object)((Component)val).GetComponent<TextMeshProUGUI>() == (Object)null)
		{
			Debug.LogWarning((object)"提示预制体中的'text'对象没有TextMeshProUGUI组件，将添加该组件");
			TextMeshProUGUI val3 = ((Component)val).gameObject.AddComponent<TextMeshProUGUI>();
			((TMP_Text)val3).fontSize = 60f;
			((Graphic)val3).color = Color.yellow;
			((TMP_Text)val3).alignment = (TextAlignmentOptions)514;
			((TMP_Text)val3).font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
			if ((Object)(object)((TMP_Text)val3).font != (Object)null && (Object)(object)((TMP_Asset)((TMP_Text)val3).font).material != (Object)null)
			{
				((TMP_Text)val3).fontSharedMaterial = ((TMP_Asset)((TMP_Text)val3).font).material;
			}
		}
		if ((Object)(object)val2 == (Object)null)
		{
			Debug.LogWarning((object)"提示预制体中找不到名为'bg'的背景对象，将创建默认背景组件");
			CreateDefaultTipBackground();
		}
	}

	private void CreateDefaultTipText()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject("text");
		val.transform.SetParent(_popupTipPrefab.transform);
		TextMeshProUGUI val2 = val.AddComponent<TextMeshProUGUI>();
		((TMP_Text)val2).fontSize = 60f;
		((Graphic)val2).color = Color.yellow;
		((TMP_Text)val2).alignment = (TextAlignmentOptions)514;
		((TMP_Text)val2).font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
		if ((Object)(object)((TMP_Text)val2).font != (Object)null && (Object)(object)((TMP_Asset)((TMP_Text)val2).font).material != (Object)null)
		{
			((TMP_Text)val2).fontSharedMaterial = ((TMP_Asset)((TMP_Text)val2).font).material;
		}
		RectTransform component = ((Component)val2).GetComponent<RectTransform>();
		component.anchorMin = Vector2.zero;
		component.anchorMax = Vector2.one;
		component.offsetMin = new Vector2(20f, 20f);
		component.offsetMax = new Vector2(-20f, -20f);
	}

	private void CreateDefaultTipBackground()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject("bg");
		val.transform.SetParent(_popupTipPrefab.transform);
		((Graphic)val.AddComponent<Image>()).color = new Color(0f, 0f, 0f, 0.7f);
		RectTransform component = val.GetComponent<RectTransform>();
		component.anchorMin = Vector2.zero;
		component.anchorMax = Vector2.one;
		component.offsetMin = Vector2.zero;
		component.offsetMax = Vector2.zero;
	}

	public static void ShowBroadcast(string message)
	{
		((MonoBehaviour)Instance).StartCoroutine(Instance.DisplayBroadcast(message));
	}

	public static void ShowPopUpTip(string message)
	{
		Instance.DisplayPopUpTipWithDoTween(message, TipPrefabType.Type1, Instance._defaultShowDuration);
	}

	public static void ShowPopUpTip(string message, TipPrefabType prefabType)
	{
		Instance.DisplayPopUpTipWithDoTween(message, prefabType, Instance._defaultShowDuration);
	}

	public static void ShowPopUpTip(string message, TipPrefabType prefabType, float showDuration)
	{
		Instance.DisplayPopUpTipWithDoTween(message, prefabType, showDuration);
	}

	private IEnumerator DisplayBroadcast(string message)
	{
		if ((Object)(object)_popupLayerTransform == (Object)null)
		{
			Initialize();
		}
		GameObject textObj = Object.Instantiate<GameObject>(_textTemplate, _popupLayerTransform);
		((Object)textObj).name = "BroadcastText";
		textObj.SetActive(true);
		TextMeshProUGUI component = textObj.GetComponent<TextMeshProUGUI>();
		((TMP_Text)component).text = message;
		RectTransform textRect = ((Component)component).GetComponent<RectTransform>();
		((TMP_Text)component).ForceMeshUpdate(false, false);
		float preferredWidth = ((TMP_Text)component).preferredWidth;
		textRect.sizeDelta = new Vector2(preferredWidth, textRect.sizeDelta.y);
		float num = Screen.width;
		textRect.anchoredPosition = new Vector2(num, 600f);
		_activeBroadcasts.Enqueue(component);
		float endX = (0f - preferredWidth) * 2f;
		while (textRect.anchoredPosition.x > endX)
		{
			float num2 = _moveSpeed * Time.deltaTime;
			textRect.anchoredPosition -= new Vector2(num2, 0f);
			yield return null;
		}
		_activeBroadcasts.Dequeue();
		Object.Destroy((Object)(object)textObj);
	}

	private void DisplayPopUpTipWithDoTween(string message, TipPrefabType prefabType, float showDuration)
	{
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Expected O, but got Unknown
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Expected O, but got Unknown
		if ((Object)(object)_popupLayerTransform == (Object)null)
		{
			Initialize();
		}
		GameObject popupTipPrefab = _popupTipPrefab;
		if ((Object)(object)popupTipPrefab == (Object)null)
		{
			((MonoBehaviour)this).StartCoroutine(LoadTipPrefabAndShow(message, prefabType, showDuration));
			return;
		}
		GameObject tipObj = Object.Instantiate<GameObject>(popupTipPrefab, _popupLayerTransform);
		tipObj.SetActive(true);
		CanvasGroup val = tipObj.GetComponent<CanvasGroup>();
		if ((Object)(object)val == (Object)null)
		{
			val = tipObj.AddComponent<CanvasGroup>();
		}
		Transform obj = tipObj.transform.Find("text");
		TextMeshProUGUI val2 = ((obj != null) ? ((Component)obj).GetComponent<TextMeshProUGUI>() : null);
		if ((Object)(object)val2 != (Object)null)
		{
			((TMP_Text)val2).text = message;
			((Graphic)val2).color = Color.yellow;
			((TMP_Text)val2).ForceMeshUpdate(false, false);
			Transform obj2 = tipObj.transform.Find("bg");
			if (obj2 != null)
			{
				((Component)obj2).GetComponent<Image>();
			}
			RectTransform component = tipObj.GetComponent<RectTransform>();
			component.anchorMin = new Vector2(0.5f, 0.5f);
			component.anchorMax = new Vector2(0.5f, 0.5f);
			component.pivot = new Vector2(0.5f, 0.5f);
			component.anchoredPosition = new Vector2(0f, 600f);
			float num = ((TMP_Text)val2).preferredWidth + 40f;
			float num2 = ((TMP_Text)val2).preferredHeight + 40f;
			component.sizeDelta = new Vector2(Mathf.Max(num, 200f), Mathf.Max(num2, 80f));
			val.alpha = 0f;
			Sequence sequence = DOTween.Sequence();
			TweenSettingsExtensions.Append(sequence, (Tween)(object)TweenSettingsExtensions.SetUpdate<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(val.DOFade(1f, _fadeInDuration), (Ease)6), true));
			TweenSettingsExtensions.AppendInterval(sequence, showDuration);
			TweenSettingsExtensions.Append(sequence, (Tween)(object)TweenSettingsExtensions.SetUpdate<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(val.DOFade(0f, _fadeOutDuration), (Ease)5), true));
			TweenSettingsExtensions.Join(sequence, (Tween)(object)TweenSettingsExtensions.SetUpdate<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(component.DOAnchorPosY(component.anchoredPosition.y + 100f, _fadeOutDuration), (Ease)6), true));
			TweenSettingsExtensions.OnComplete<Sequence>(sequence, (TweenCallback)delegate
			{
				if ((Object)(object)tipObj != (Object)null)
				{
					Object.Destroy((Object)(object)tipObj);
				}
			});
			TweenExtensions.Play<Sequence>(sequence);
			TweenSettingsExtensions.OnKill<Sequence>(sequence, (TweenCallback)delegate
			{
				sequence = null;
			});
		}
		else
		{
			Debug.LogError((object)"无法在提示对象中找到TextMeshProUGUI组件");
			Object.Destroy((Object)(object)tipObj);
		}
	}

	private IEnumerator LoadTipPrefabAndShow(string message, TipPrefabType prefabType, float showDuration)
	{
		yield return LoadTipPrefab();
		DisplayPopUpTipWithDoTween(message, prefabType, showDuration);
	}

	public static void SetMoveSpeed(float speed)
	{
		Instance._moveSpeed = speed;
	}

	public static int GetActiveBroadcastCount()
	{
		return Instance._activeBroadcasts.Count;
	}

	public static void ClearAllBroadcasts()
	{
		((MonoBehaviour)Instance).StopAllCoroutines();
		while (Instance._activeBroadcasts.Count > 0)
		{
			TextMeshProUGUI val = Instance._activeBroadcasts.Dequeue();
			if ((Object)(object)val != (Object)null && (Object)(object)((Component)val).gameObject != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)val).gameObject);
			}
		}
	}

	private void Awake()
	{
		if ((Object)(object)_instance != (Object)null && (Object)(object)_instance != (Object)(object)this)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		_instance = this;
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
	}
}

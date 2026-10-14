using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FakeAdUI : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI countdownText;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private Image backgroundImage;

	[SerializeField]
	private TextMeshProUGUI adContentText;

	private float videoLength;

	private float currentTime;

	private Action onAdFinished;

	private Action onCloseClicked;

	private bool isCountdownCompleted;

	private void Awake()
	{
		InitializeUIComponents();
	}

	private void InitializeUIComponents()
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected O, but got Unknown
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Expected O, but got Unknown
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d0: Expected O, but got Unknown
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Expected O, but got Unknown
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_0350: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Unknown result type (might be due to invalid IL or missing references)
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0385: Unknown result type (might be due to invalid IL or missing references)
		//IL_0399: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)backgroundImage == (Object)null)
		{
			GameObject val = new GameObject("Background");
			val.transform.SetParent(((Component)this).transform, false);
			backgroundImage = val.AddComponent<Image>();
			((Graphic)backgroundImage).color = Color.black;
			RectTransform component = ((Component)backgroundImage).GetComponent<RectTransform>();
			component.anchorMin = Vector2.zero;
			component.anchorMax = Vector2.one;
			component.offsetMin = Vector2.zero;
			component.offsetMax = Vector2.zero;
		}
		else
		{
			((Graphic)backgroundImage).color = Color.black;
		}
		if ((Object)(object)countdownText == (Object)null)
		{
			GameObject val2 = new GameObject("CountdownText");
			val2.transform.SetParent(((Component)this).transform, false);
			countdownText = val2.AddComponent<TextMeshProUGUI>();
			((Graphic)countdownText).color = Color.white;
			((TMP_Text)countdownText).fontSize = 48f;
			((TMP_Text)countdownText).alignment = (TextAlignmentOptions)514;
			((TMP_Text)countdownText).text = "15:00";
			RectTransform component2 = ((Component)countdownText).GetComponent<RectTransform>();
			component2.anchorMin = new Vector2(0.5f, 0.5f);
			component2.anchorMax = new Vector2(0.5f, 0.5f);
			component2.pivot = new Vector2(0.5f, 0.5f);
			component2.anchoredPosition = Vector2.zero;
			component2.sizeDelta = new Vector2(200f, 100f);
		}
		if ((Object)(object)closeButton == (Object)null)
		{
			GameObject val3 = new GameObject("CloseButton");
			val3.transform.SetParent(((Component)this).transform, false);
			closeButton = val3.AddComponent<Button>();
			((Graphic)val3.AddComponent<Image>()).color = new Color(1f, 0.3f, 0.3f, 1f);
			GameObject val4 = new GameObject("ButtonText");
			val4.transform.SetParent(val3.transform, false);
			TextMeshProUGUI obj = val4.AddComponent<TextMeshProUGUI>();
			((Graphic)obj).color = Color.white;
			((TMP_Text)obj).fontSize = 24f;
			((TMP_Text)obj).alignment = (TextAlignmentOptions)514;
			((TMP_Text)obj).text = "关闭广告";
			RectTransform component3 = ((Component)closeButton).GetComponent<RectTransform>();
			component3.anchorMin = new Vector2(1f, 1f);
			component3.anchorMax = new Vector2(1f, 1f);
			component3.pivot = new Vector2(1f, 1f);
			component3.anchoredPosition = new Vector2(-50f, -50f);
			component3.sizeDelta = new Vector2(120f, 50f);
			RectTransform component4 = ((Component)obj).GetComponent<RectTransform>();
			component4.anchorMin = Vector2.zero;
			component4.anchorMax = Vector2.one;
			component4.offsetMin = Vector2.zero;
			component4.offsetMax = Vector2.zero;
		}
		if ((Object)(object)adContentText == (Object)null)
		{
			GameObject val5 = new GameObject("AdContentText");
			val5.transform.SetParent(((Component)this).transform, false);
			adContentText = val5.AddComponent<TextMeshProUGUI>();
			((Graphic)adContentText).color = Color.white;
			((TMP_Text)adContentText).fontSize = 80f;
			((TMP_Text)adContentText).alignment = (TextAlignmentOptions)514;
			((TMP_Text)adContentText).text = "观看广告获得奖励\n倒计时结束后可关闭广告";
			RectTransform component5 = ((Component)adContentText).GetComponent<RectTransform>();
			component5.anchorMin = new Vector2(0.5f, 0.6f);
			component5.anchorMax = new Vector2(0.5f, 0.6f);
			component5.pivot = new Vector2(0.5f, 0.5f);
			component5.anchoredPosition = Vector2.zero;
			component5.sizeDelta = new Vector2(1000f, 500f);
		}
		((Component)closeButton).gameObject.SetActive(false);
		((UnityEvent)closeButton.onClick).AddListener(new UnityAction(OnCloseButtonClicked));
	}

	public void Initialize(float length, Action onFinished, Action onClose)
	{
		videoLength = length;
		currentTime = videoLength;
		onAdFinished = onFinished;
		onCloseClicked = onClose;
		isCountdownCompleted = false;
		UpdateCountdownText();
		((MonoBehaviour)this).StartCoroutine(CountdownCoroutine());
	}

	private IEnumerator CountdownCoroutine()
	{
		while (currentTime > 0f)
		{
			currentTime -= Time.unscaledDeltaTime;
			if (currentTime < 0f)
			{
				currentTime = 0f;
			}
			UpdateCountdownText();
			yield return null;
		}
		isCountdownCompleted = true;
		((Component)closeButton).gameObject.SetActive(true);
		((TMP_Text)countdownText).text = "0:00";
		((TMP_Text)adContentText).text = "广告播放完成！\n点击关闭按钮获取奖励";
	}

	private void UpdateCountdownText()
	{
		if ((Object)(object)countdownText != (Object)null)
		{
			int num = Mathf.FloorToInt(currentTime / 60f);
			int num2 = Mathf.FloorToInt(currentTime % 60f);
			((TMP_Text)countdownText).text = $"{num}:{num2:D2}";
		}
	}

	private void OnCloseButtonClicked()
	{
		SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
		if (isCountdownCompleted && onAdFinished != null)
		{
			onAdFinished();
		}
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	private void OnDestroy()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		if ((Object)(object)closeButton != (Object)null)
		{
			((UnityEvent)closeButton.onClick).RemoveListener(new UnityAction(OnCloseButtonClicked));
		}
	}
}

using System;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Module.Loading;

public class LoadingUI : MonoBehaviour
{
	[Header("UI组件")]
	[SerializeField]
	private Image backgroundImage;

	[SerializeField]
	private Image progressBarImage;

	[SerializeField]
	private RectTransform progressEffect;

	[SerializeField]
	private TextMeshProUGUI progressText;

	[SerializeField]
	private CanvasGroup canvasGroup;

	private float _progressBarWidth;

	private void Awake()
	{
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		backgroundImage = ((Component)((Component)this).transform.Find("bg")).GetComponent<Image>();
		progressBarImage = ((Component)((Component)this).transform.Find("progress/progressbg/bar")).GetComponent<Image>();
		progressEffect = ((Component)((Component)this).transform.Find("progress/progressbg/cat")).GetComponent<RectTransform>();
		progressText = ((Component)((Component)this).transform.Find("progress/progressbg/Text")).GetComponent<TextMeshProUGUI>();
		if ((Object)(object)progressBarImage != (Object)null)
		{
			progressBarImage.type = (Type)3;
			progressBarImage.fillMethod = (FillMethod)0;
			progressBarImage.fillAmount = 0f;
		}
		if ((Object)(object)progressBarImage != (Object)null && (Object)(object)progressEffect != (Object)null)
		{
			Rect rect = ((Graphic)progressBarImage).rectTransform.rect;
			_progressBarWidth = ((Rect)(ref rect)).width;
			SetEffectPosition(0f);
		}
		if ((Object)(object)progressText != (Object)null)
		{
			((TMP_Text)progressText).text = "0%";
		}
	}

	public void Set9999()
	{
		Canvas component = ((Component)this).GetComponent<Canvas>();
		component.overrideSorting = true;
		component.sortingOrder = 99;
	}

	public void SetProgress(float progress)
	{
		progress = Mathf.Clamp01(progress);
		if ((Object)(object)progressBarImage != (Object)null)
		{
			TweenSettingsExtensions.SetUpdate<TweenerCore<float, float, FloatOptions>>(progressBarImage.DOFillAmount(progress, 0.1f), true);
		}
		SetEffectPosition(progress);
		if ((Object)(object)progressText != (Object)null)
		{
			int num = Mathf.RoundToInt(progress * 100f);
			((TMP_Text)progressText).text = num + "%";
		}
	}

	public void ResetLoading()
	{
		if ((Object)(object)progressBarImage != (Object)null)
		{
			progressBarImage.fillAmount = 0f;
		}
		if ((Object)(object)progressEffect != (Object)null)
		{
			SetEffectPosition(0f);
		}
		if ((Object)(object)progressText != (Object)null)
		{
			((TMP_Text)progressText).text = "0%";
		}
	}

	private void SetEffectPosition(float progress)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)progressEffect != (Object)null)
		{
			float num = _progressBarWidth * progress - _progressBarWidth * 0.5f;
			Vector3 localPosition = ((Transform)progressEffect).localPosition;
			localPosition.x = num;
			TweenSettingsExtensions.SetUpdate<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMoveX((Transform)(object)progressEffect, num, 0.1f, false), true);
		}
	}

	private void CheckCanvas()
	{
		if ((Object)(object)canvasGroup == (Object)null)
		{
			canvasGroup = ((Component)this).gameObject.AddComponent<CanvasGroup>();
			((Component)this).gameObject.AddComponent<GraphicRaycaster>();
		}
		canvasGroup.alpha = 1f;
		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true;
		((Component)this).gameObject.SetActive(true);
	}

	public void FadeOut(float duration = 1f, Action onComplete = null)
	{
		CheckCanvas();
		((MonoBehaviour)this).StartCoroutine(FadeOutCoroutine(duration, onComplete));
	}

	private IEnumerator FadeOutCoroutine(float duration, Action onComplete)
	{
		float startTime = Time.unscaledTime;
		float startAlpha = canvasGroup.alpha;
		float targetAlpha = 0f;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
		while (Time.unscaledTime - startTime < duration)
		{
			float t = (Time.unscaledTime - startTime) / duration;
			canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, EaseInOutQuart(t));
			yield return (object)new WaitForEndOfFrame();
		}
		canvasGroup.alpha = targetAlpha;
		((Component)this).gameObject.SetActive(false);
		canvasGroup.alpha = 1f;
		Debug.Log((object)"UI FadeOut Complete");
		onComplete?.Invoke();
	}

	public void FadeIn(float duration = 1f, Action onComplete = null)
	{
		CheckCanvas();
		((MonoBehaviour)this).StartCoroutine(FadeInCoroutine(duration, onComplete));
	}

	private IEnumerator FadeInCoroutine(float duration, Action onComplete)
	{
		float startTime = Time.unscaledTime;
		float startAlpha = canvasGroup.alpha;
		float targetAlpha = 1f;
		while (Time.unscaledTime - startTime < duration)
		{
			float t = (Time.unscaledTime - startTime) / duration;
			canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, EaseInOutQuart(t));
			yield return (object)new WaitForEndOfFrame();
		}
		canvasGroup.alpha = targetAlpha;
		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true;
		onComplete?.Invoke();
	}

	private float EaseInOutQuart(float t)
	{
		if (!((double)t < 0.5))
		{
			return 1f - 8f * Mathf.Pow(t -= 1f, 4f);
		}
		return 8f * t * t * t * t;
	}
}

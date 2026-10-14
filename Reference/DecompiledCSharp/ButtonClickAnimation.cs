using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonClickAnimation : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerExitHandler
{
	[Header("缩放设置")]
	[SerializeField]
	private float pressScale = 1.2f;

	[SerializeField]
	private float bounceScale = 1.1f;

	[SerializeField]
	private float pressDuration = 0.1f;

	[SerializeField]
	private float bounceDuration = 0.15f;

	[Header("呼吸效果设置")]
	[SerializeField]
	private bool enableBreathing = true;

	[SerializeField]
	private float breathingMinScale = 1.15f;

	[SerializeField]
	private float breathingMaxScale = 1.2f;

	[SerializeField]
	private float breathingSpeed = 1f;

	private RectTransform rectTransform;

	private Vector3 originalScale;

	private Tween pressTween;

	private Tween breathingTween;

	private Tween bounceTween;

	private bool isPressed;

	private void Awake()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		rectTransform = ((Component)this).GetComponent<RectTransform>();
		originalScale = ((Transform)rectTransform).localScale;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		if (isPressed)
		{
			return;
		}
		isPressed = true;
		if (bounceTween != null)
		{
			TweenExtensions.Kill(bounceTween, false);
			bounceTween = null;
		}
		pressTween = (Tween)(object)TweenSettingsExtensions.SetUpdate<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale((Transform)(object)rectTransform, originalScale * pressScale, pressDuration), (Ease)6), true);
		TweenSettingsExtensions.OnComplete<Tween>(pressTween, (TweenCallback)delegate
		{
			if (enableBreathing && isPressed)
			{
				PlayBreathingAnimation();
			}
		});
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (isPressed)
		{
			isPressed = false;
			if (pressTween != null)
			{
				TweenExtensions.Kill(pressTween, false);
				pressTween = null;
			}
			if (breathingTween != null)
			{
				TweenExtensions.Kill(breathingTween, false);
				breathingTween = null;
			}
			PlayBounceAnimation();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (isPressed)
		{
			isPressed = false;
			if (pressTween != null)
			{
				TweenExtensions.Kill(pressTween, false);
				pressTween = null;
			}
			if (breathingTween != null)
			{
				TweenExtensions.Kill(breathingTween, false);
				breathingTween = null;
			}
			PlayBounceAnimation();
		}
	}

	private void PlayBreathingAnimation()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if (breathingTween != null)
		{
			TweenExtensions.Kill(breathingTween, false);
		}
		Sequence val = DOTween.Sequence();
		TweenSettingsExtensions.Append(val, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale((Transform)(object)rectTransform, originalScale * breathingMaxScale, breathingSpeed * 0.5f), (Ease)6));
		TweenSettingsExtensions.Append(val, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale((Transform)(object)rectTransform, originalScale * breathingMinScale, breathingSpeed * 0.5f), (Ease)5));
		TweenSettingsExtensions.SetLoops<Sequence>(val, -1, (LoopType)1);
		TweenSettingsExtensions.SetUpdate<Sequence>(val, true);
		breathingTween = (Tween)(object)val;
	}

	private void PlayBounceAnimation()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (bounceTween != null)
		{
			TweenExtensions.Kill(bounceTween, false);
		}
		Sequence obj = DOTween.Sequence();
		TweenSettingsExtensions.Append(obj, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale((Transform)(object)rectTransform, originalScale * bounceScale, bounceDuration * 0.5f), (Ease)6));
		TweenSettingsExtensions.Append(obj, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale((Transform)(object)rectTransform, originalScale, bounceDuration * 0.5f), (Ease)6));
		TweenSettingsExtensions.SetUpdate<Sequence>(obj, true);
	}

	private void OnDisable()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		isPressed = false;
		if (pressTween != null)
		{
			TweenExtensions.Kill(pressTween, false);
			pressTween = null;
		}
		if (breathingTween != null)
		{
			TweenExtensions.Kill(breathingTween, false);
			breathingTween = null;
		}
		if (bounceTween != null)
		{
			TweenExtensions.Kill(bounceTween, false);
			bounceTween = null;
		}
		((Transform)rectTransform).localScale = originalScale;
	}
}

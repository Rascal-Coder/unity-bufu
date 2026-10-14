using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIAnimatorElite : MonoBehaviour
{
	[Header("0. Group Control (群组控制防冲突)")]
	[Tooltip("如果是挂在父物体统一控制逐个播放，请【取消勾选】此项！")]
	public bool autoPlayOnEnable = true;

	[Tooltip("全局进场延迟（单机运行时有效）")]
	public float enterDelay;

	[Header("插槽：拖入你的动效预设文件 (.asset)")]
	public UIAnimPreset enterPreset;

	public UIAnimPreset exitPreset;

	private RectTransform rect;

	private CanvasGroup canvasGroup;

	private Vector2 originPos;

	private Vector3 originScale;

	private Vector3 originRot;

	private void Awake()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		rect = ((Component)this).GetComponent<RectTransform>();
		originPos = rect.anchoredPosition;
		originScale = ((Component)this).transform.localScale;
		originRot = ((Transform)rect).localEulerAngles;
		canvasGroup = ((Component)this).GetComponent<CanvasGroup>();
	}

	private void EnsureCanvasGroup(UIAnimPreset preset)
	{
		if ((Object)(object)preset != (Object)null && preset.enableFade)
		{
			if ((Object)(object)canvasGroup == (Object)null)
			{
				canvasGroup = ((Component)this).GetComponent<CanvasGroup>();
			}
			if ((Object)(object)canvasGroup == (Object)null)
			{
				canvasGroup = ((Component)this).gameObject.AddComponent<CanvasGroup>();
			}
		}
	}

	private void OnEnable()
	{
		if (autoPlayOnEnable)
		{
			PlayEnter(0f);
		}
	}

	public void PlayEnter(float customDelay)
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rect == (Object)null)
		{
			Awake();
		}
		if (!((Object)(object)rect == (Object)null) && !((Object)(object)enterPreset == (Object)null))
		{
			EnsureCanvasGroup(enterPreset);
			ShortcutExtensions.DOKill((Component)(object)((Component)this).transform, true);
			if ((Object)(object)canvasGroup != (Object)null)
			{
				ShortcutExtensions.DOKill((Component)(object)canvasGroup, false);
			}
			Sequence val = DOTween.Sequence();
			TweenSettingsExtensions.SetDelay<Sequence>(val, enterDelay + customDelay);
			if (enterPreset.enableMove)
			{
				rect.anchoredPosition = originPos + enterPreset.moveOffset;
			}
			if (enterPreset.enableFade && (Object)(object)canvasGroup != (Object)null)
			{
				canvasGroup.alpha = enterPreset.startAlpha;
			}
			if (enterPreset.enableScale || enterPreset.enableJellyBounce)
			{
				((Component)this).transform.localScale = enterPreset.startScale;
			}
			if (enterPreset.enableRotation)
			{
				((Transform)rect).localEulerAngles = enterPreset.startRotation;
			}
			if (enterPreset.enableMove)
			{
				TweenSettingsExtensions.Insert(val, 0f, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(rect.DOAnchorPos(originPos, enterPreset.duration), enterPreset.easeCurve));
			}
			if (enterPreset.enableFade && (Object)(object)canvasGroup != (Object)null)
			{
				TweenSettingsExtensions.Insert(val, 0f, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(canvasGroup.DOFade(1f, enterPreset.duration), (Ease)1));
			}
			if (enterPreset.enableScale && !enterPreset.enableJellyBounce)
			{
				TweenSettingsExtensions.Insert(val, 0f, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(((Component)this).transform, originScale, enterPreset.duration), enterPreset.easeCurve));
			}
			else if (enterPreset.enableJellyBounce)
			{
				TweenSettingsExtensions.Insert(val, 0f, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(((Component)this).transform, originScale, enterPreset.jellyDelay), (Ease)6));
				TweenSettingsExtensions.Insert(val, enterPreset.jellyDelay, (Tween)(object)ShortcutExtensions.DOPunchScale(((Component)this).transform, enterPreset.punchStrength, enterPreset.bounceDuration, enterPreset.vibrato, enterPreset.elasticity));
			}
			if (enterPreset.enableRotation)
			{
				TweenSettingsExtensions.Insert(val, enterPreset.rotationDelay, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Quaternion, Vector3, QuaternionOptions>>(ShortcutExtensions.DOLocalRotate((Transform)(object)rect, originRot, enterPreset.duration, (RotateMode)1), enterPreset.easeCurve));
			}
		}
	}

	public void PlayExit(float customDelay)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Expected O, but got Unknown
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)exitPreset == (Object)null)
		{
			((Component)this).gameObject.SetActive(false);
			return;
		}
		EnsureCanvasGroup(exitPreset);
		ShortcutExtensions.DOKill((Component)(object)((Component)this).transform, true);
		if ((Object)(object)canvasGroup != (Object)null)
		{
			ShortcutExtensions.DOKill((Component)(object)canvasGroup, false);
		}
		Sequence val = DOTween.Sequence();
		TweenSettingsExtensions.SetDelay<Sequence>(val, customDelay);
		if (exitPreset.enableMove)
		{
			TweenSettingsExtensions.Insert(val, 0f, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(rect.DOAnchorPos(originPos + exitPreset.moveOffset, exitPreset.duration), exitPreset.easeCurve));
		}
		if (exitPreset.enableFade && (Object)(object)canvasGroup != (Object)null)
		{
			TweenSettingsExtensions.Insert(val, 0f, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(canvasGroup.DOFade(exitPreset.startAlpha, exitPreset.duration), (Ease)1));
		}
		if (exitPreset.enableScale || exitPreset.enableJellyBounce)
		{
			TweenSettingsExtensions.Insert(val, 0f, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(((Component)this).transform, exitPreset.startScale, exitPreset.duration), exitPreset.easeCurve));
		}
		if (exitPreset.enableRotation)
		{
			TweenSettingsExtensions.Insert(val, exitPreset.rotationDelay, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Quaternion, Vector3, QuaternionOptions>>(ShortcutExtensions.DOLocalRotate((Transform)(object)rect, exitPreset.startRotation, exitPreset.duration, (RotateMode)0), exitPreset.easeCurve));
		}
		TweenSettingsExtensions.OnComplete<Sequence>(val, (TweenCallback)delegate
		{
			((Component)this).gameObject.SetActive(false);
		});
	}
}

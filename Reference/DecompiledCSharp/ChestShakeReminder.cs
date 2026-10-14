using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class ChestShakeReminder : MonoBehaviour
{
	[Header("晃动参数设置")]
	[Tooltip("单次晃动的角度")]
	[SerializeField]
	private float shakeAngle = 15f;

	[Tooltip("单次晃动的时间（越小晃得越快）")]
	[SerializeField]
	private float duration = 0.15f;

	private RectTransform rectTransform;

	private Tween shakeTween;

	private bool isShaking;

	private void Awake()
	{
		rectTransform = ((Component)this).gameObject.GetOrAddComponent<RectTransform>();
	}

	private void Start()
	{
		rectTransform = ((Component)this).gameObject.GetOrAddComponent<RectTransform>();
	}

	public void StartShake()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (!isShaking)
		{
			isShaking = true;
			rectTransform = ((Component)this).gameObject.GetOrAddComponent<RectTransform>();
			((Transform)rectTransform).localRotation = Quaternion.Euler(0f, 0f, shakeAngle);
			shakeTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Quaternion, Vector3, QuaternionOptions>>(TweenSettingsExtensions.SetLoops<TweenerCore<Quaternion, Vector3, QuaternionOptions>>(ShortcutExtensions.DOLocalRotate((Transform)(object)rectTransform, new Vector3(0f, 0f, 0f - shakeAngle), duration, (RotateMode)0), -1, (LoopType)1), (Ease)1);
		}
	}

	public void StopShake()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (isShaking)
		{
			isShaking = false;
			if (shakeTween != null && TweenExtensions.IsActive(shakeTween))
			{
				TweenExtensions.Kill(shakeTween, false);
			}
			((Transform)rectTransform).localRotation = Quaternion.identity;
		}
	}

	private void OnDestroy()
	{
		if (shakeTween != null && TweenExtensions.IsActive(shakeTween))
		{
			TweenExtensions.Kill(shakeTween, false);
		}
	}
}

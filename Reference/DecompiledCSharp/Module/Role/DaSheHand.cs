using System;
using Module.UI;
using UnityEngine;

namespace Module.Role;

public class DaSheHand : MonoBehaviour
{
	private RectTransform _rectTransform;

	private UISpriteAnimationSequence _animSequence;

	private Vector2 _targetSize;

	private Action _onComplete;

	private bool _isAnimating;

	private bool _useAnimation;

	private Vector2 _baseSize = Vector2.zero;

	public void Init(Vector2 startPos, Vector2 targetPos, Action onComplete)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		InitInternal(startPos, targetPos, onComplete, 1f);
	}

	public void InitVat(Vector2 startPos, Vector2 targetPos, float distanceScale, Action onComplete)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		InitInternal(startPos, targetPos, onComplete, Mathf.Max(0.0001f, distanceScale));
	}

	private void InitInternal(Vector2 startPos, Vector2 targetPos, Action onComplete, float renderScale)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_rectTransform == (Object)null)
		{
			_rectTransform = ((Component)this).GetComponent<RectTransform>();
		}
		if ((Object)(object)_animSequence == (Object)null)
		{
			_animSequence = ((Component)this).GetComponent<UISpriteAnimationSequence>();
		}
		if (_baseSize == Vector2.zero)
		{
			_baseSize = _rectTransform.sizeDelta;
		}
		_rectTransform.pivot = new Vector2(0f, 0.5f);
		((Transform)_rectTransform).position = Vector2.op_Implicit(startPos);
		Vector2 val = targetPos - startPos;
		float magnitude = ((Vector2)(ref val)).magnitude;
		float num = Mathf.Atan2(val.y, val.x) * 57.29578f;
		((Transform)_rectTransform).rotation = Quaternion.Euler(0f, 0f, num);
		float num2 = ((Random.value > 0.5f) ? (-1f) : 1f);
		((Transform)_rectTransform).localScale = new Vector3(renderScale, renderScale * num2, renderScale);
		_onComplete = onComplete;
		_isAnimating = true;
		float num3 = magnitude / renderScale;
		float y = _baseSize.y;
		if ((Object)(object)_animSequence != (Object)null)
		{
			_useAnimation = true;
			_rectTransform.sizeDelta = new Vector2(num3, y);
			_animSequence.IsLooping = false;
			_animSequence.OnAnimationComplete = null;
			UISpriteAnimationSequence animSequence = _animSequence;
			animSequence.OnAnimationComplete = (Action)Delegate.Combine(animSequence.OnAnimationComplete, new Action(OnAnimComplete));
			_animSequence.Restart();
			_animSequence.Play();
		}
		else
		{
			_useAnimation = false;
			_rectTransform.sizeDelta = new Vector2(0f, y);
			_targetSize = new Vector2(num3, y);
		}
	}

	private void Update()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if (_isAnimating && !_useAnimation)
		{
			_rectTransform.sizeDelta = Vector2.Lerp(_rectTransform.sizeDelta, _targetSize, Time.deltaTime * 10f);
			if (_targetSize.x - _rectTransform.sizeDelta.x < 1f)
			{
				_rectTransform.sizeDelta = _targetSize;
				_isAnimating = false;
				_onComplete?.Invoke();
			}
		}
	}

	private void OnAnimComplete()
	{
		if (_isAnimating && _useAnimation)
		{
			_isAnimating = false;
			_onComplete?.Invoke();
		}
	}
}

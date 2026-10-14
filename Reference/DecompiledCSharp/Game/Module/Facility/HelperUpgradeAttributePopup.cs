using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Module.Facility;

public class HelperUpgradeAttributePopup : MonoBehaviour
{
	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private TextMeshProUGUI attributeNameText;

	[SerializeField]
	private TextMeshProUGUI originalValueText;

	[SerializeField]
	private TextMeshProUGUI upgradeValueText;

	[SerializeField]
	private CanvasGroup canvasGroup;

	private Sequence _showSequence;

	private Vector3 _originalLocalPos;

	private bool _isOriginalPosSaved;

	private void Awake()
	{
		if ((Object)(object)canvasGroup == (Object)null)
		{
			canvasGroup = ((Component)this).GetComponent<CanvasGroup>();
			if ((Object)(object)canvasGroup == (Object)null)
			{
				canvasGroup = ((Component)this).gameObject.AddComponent<CanvasGroup>();
			}
		}
		canvasGroup.alpha = 0f;
	}

	private void OnDestroy()
	{
		if (_showSequence != null)
		{
			TweenExtensions.Kill((Tween)(object)_showSequence, false);
			_showSequence = null;
		}
	}

	public void SetIcon(Sprite icon)
	{
		if ((Object)(object)icon != (Object)null && (Object)(object)iconImage != (Object)null)
		{
			iconImage.sprite = icon;
		}
	}

	public void SetAttributeValues(string originalValue, string property, string upgradeValue, bool showUpgradeValue = true, bool addPlusPrefix = true)
	{
		if ((Object)(object)attributeNameText != (Object)null)
		{
			((TMP_Text)attributeNameText).text = property;
		}
		if ((Object)(object)originalValueText != (Object)null)
		{
			((TMP_Text)originalValueText).text = originalValue;
			((Component)originalValueText).gameObject.SetActive(!string.IsNullOrEmpty(originalValue));
		}
		if ((Object)(object)upgradeValueText != (Object)null)
		{
			((Component)upgradeValueText).gameObject.SetActive(showUpgradeValue);
			if (showUpgradeValue)
			{
				((TMP_Text)upgradeValueText).text = (addPlusPrefix ? ("+" + upgradeValue) : upgradeValue);
			}
		}
	}

	public void ShowWithAnimation(float delay = 0f)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Expected O, but got Unknown
		if (_showSequence != null)
		{
			TweenExtensions.Kill((Tween)(object)_showSequence, false);
		}
		if (!_isOriginalPosSaved)
		{
			_originalLocalPos = ((Component)this).transform.localPosition;
			_isOriginalPosSaved = true;
		}
		Vector3 initialPosition = _originalLocalPos;
		((Component)this).transform.localPosition = initialPosition;
		((Component)this).transform.localScale = new Vector3(0.5f, 0.5f, 1f);
		canvasGroup.alpha = 0f;
		((Component)this).gameObject.SetActive(true);
		_showSequence = DOTween.Sequence();
		if (delay > 0f)
		{
			TweenSettingsExtensions.AppendInterval(_showSequence, delay);
		}
		Sequence val = DOTween.Sequence();
		TweenSettingsExtensions.Append(val, (Tween)(object)ShortcutExtensions.DOScale(((Component)this).transform, new Vector3(1.1f, 1.1f, 1f), 0.2f));
		TweenSettingsExtensions.Append(val, (Tween)(object)ShortcutExtensions.DOScale(((Component)this).transform, Vector3.one, 0.1f));
		TweenSettingsExtensions.Append(_showSequence, (Tween)(object)canvasGroup.DOFade(1f, 0.3f));
		TweenSettingsExtensions.Join(_showSequence, (Tween)(object)val);
		TweenSettingsExtensions.AppendInterval(_showSequence, 0.8f);
		TweenSettingsExtensions.Append(_showSequence, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMoveY(((Component)this).transform, initialPosition.y + 30f, 0.8f, false), (Ease)6));
		TweenSettingsExtensions.Join(_showSequence, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(canvasGroup.DOFade(0f, 0.8f), (Ease)6));
		TweenSettingsExtensions.OnComplete<Sequence>(_showSequence, (TweenCallback)delegate
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			((Component)this).gameObject.SetActive(false);
			((Component)this).transform.localPosition = initialPosition;
			((Component)this).transform.localScale = Vector3.one;
			Object.Destroy((Object)(object)((Component)this).gameObject);
		});
	}

	public void HideImmediately()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (_showSequence != null)
		{
			TweenExtensions.Kill((Tween)(object)_showSequence, false);
			_showSequence = null;
		}
		((Component)this).gameObject.SetActive(false);
		canvasGroup.alpha = 0f;
		if (_isOriginalPosSaved)
		{
			((Component)this).transform.localPosition = _originalLocalPos;
		}
		((Component)this).transform.localScale = Vector3.one;
	}
}

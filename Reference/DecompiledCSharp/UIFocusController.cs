using System.Collections;
using UnityEngine;

public class UIFocusController : MonoBehaviour
{
	[Header("焦点配置")]
	[SerializeField]
	private float focusScale = 1.1f;

	[SerializeField]
	private float animationDuration = 0.3f;

	[SerializeField]
	private bool returnToOriginal = true;

	[Header("当前状态")]
	private RectTransform targetPanel;

	private RectTransform targetButton;

	private Vector3 originalScale;

	private Vector3 originalButtonPos;

	private Vector3 originalPanelPos;

	private int originalSiblingIndex;

	public void Focus(RectTransform button, RectTransform panel)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)targetPanel != (Object)null && returnToOriginal)
		{
			ResetFocus();
		}
		targetButton = button;
		targetPanel = panel;
		originalScale = ((Transform)panel).localScale;
		originalButtonPos = ((Transform)button).localPosition;
		originalPanelPos = Vector2.op_Implicit(panel.anchoredPosition);
		originalSiblingIndex = ((Transform)panel).GetSiblingIndex();
		((Transform)panel).SetAsLastSibling();
		((MonoBehaviour)this).StartCoroutine(AnimateToFocus(button, panel));
	}

	public void ResetFocus()
	{
		if (!((Object)(object)targetPanel == (Object)null) && !((Object)(object)targetButton == (Object)null))
		{
			((MonoBehaviour)this).StartCoroutine(AnimateToOriginal());
		}
	}

	private IEnumerator AnimateToFocus(RectTransform button, RectTransform panel)
	{
		float elapsedTime = 0f;
		Vector3 val = new Vector3((float)Screen.width * 0.5f, (float)Screen.height * 0.5f, 0f);
		Rect rect = panel.rect;
		Vector3 val2 = ((Transform)panel).TransformPoint(Vector2.op_Implicit(((Rect)(ref rect)).center));
		Vector3 position = ((Transform)button).position;
		Vector3 val3 = val;
		Vector3 buttonTargetLocalPos = ((Transform)button).parent.InverseTransformPoint(val3);
		Vector3 val4 = val3 - position;
		Vector3 val5 = val2 - val4;
		Vector3 panelTargetAnchoredPos = ((Transform)panel).parent.InverseTransformPoint(val5);
		while (elapsedTime < animationDuration)
		{
			float t = elapsedTime / animationDuration;
			t = EaseOutCubic(t);
			((Transform)button).localPosition = Vector3.Lerp(originalButtonPos, buttonTargetLocalPos, t);
			panel.anchoredPosition = Vector2.op_Implicit(Vector3.Lerp(originalPanelPos, panelTargetAnchoredPos, t));
			((Transform)panel).localScale = Vector3.Lerp(originalScale, originalScale * focusScale, t);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		((Transform)button).localPosition = buttonTargetLocalPos;
		panel.anchoredPosition = Vector2.op_Implicit(panelTargetAnchoredPos);
		((Transform)panel).localScale = originalScale * focusScale;
	}

	private IEnumerator AnimateToOriginal()
	{
		float elapsedTime = 0f;
		Vector3 currentButtonPos = ((Transform)targetButton).localPosition;
		Vector3 currentPanelPos = Vector2.op_Implicit(targetPanel.anchoredPosition);
		Vector3 currentScale = ((Transform)targetPanel).localScale;
		while (elapsedTime < animationDuration)
		{
			float t = elapsedTime / animationDuration;
			t = EaseInOutCubic(t);
			((Transform)targetButton).localPosition = Vector3.Lerp(currentButtonPos, originalButtonPos, t);
			targetPanel.anchoredPosition = Vector2.op_Implicit(Vector3.Lerp(currentPanelPos, originalPanelPos, t));
			((Transform)targetPanel).localScale = Vector3.Lerp(currentScale, originalScale, t);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		((Transform)targetButton).localPosition = originalButtonPos;
		targetPanel.anchoredPosition = Vector2.op_Implicit(originalPanelPos);
		((Transform)targetPanel).localScale = originalScale;
		((Transform)targetPanel).SetSiblingIndex(originalSiblingIndex);
		targetPanel = null;
		targetButton = null;
	}

	private float EaseOutCubic(float t)
	{
		return 1f - Mathf.Pow(1f - t, 3f);
	}

	private float EaseInOutCubic(float t)
	{
		if (!(t < 0.5f))
		{
			return 1f - Mathf.Pow(-2f * t + 2f, 3f) * 0.5f;
		}
		return 4f * t * t * t;
	}
}

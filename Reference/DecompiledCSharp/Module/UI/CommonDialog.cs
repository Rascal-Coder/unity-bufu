using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Module.UI;

public class CommonDialog : MonoBehaviour
{
	[Header("组件引用")]
	[Tooltip("对话框背景图片")]
	[SerializeField]
	private Image backgroundImage;

	[SerializeField]
	private Button bgButton;

	[Tooltip("文本显示组件")]
	[SerializeField]
	private TextMeshProUGUI dialogText;

	[Tooltip("文本父对象（用于调整位置）")]
	[SerializeField]
	private RectTransform textParent;

	[Header("按钮配置")]
	[SerializeField]
	private Button confirmButton;

	[SerializeField]
	private Button cancelButton;

	private Action _onConfirm;

	private Action _onCancel;

	public Button GetBgButton()
	{
		return bgButton;
	}

	private void Awake()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected O, but got Unknown
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Expected O, but got Unknown
		if ((Object)(object)backgroundImage == (Object)null)
		{
			backgroundImage = ((Component)this).GetComponent<Image>();
		}
		if ((Object)(object)textParent == (Object)null)
		{
			TextMeshProUGUI obj = dialogText;
			textParent = ((obj != null) ? ((TMP_Text)obj).rectTransform : null);
		}
		if ((Object)(object)confirmButton != (Object)null)
		{
			((UnityEvent)confirmButton.onClick).AddListener(new UnityAction(OnConfirmClick));
		}
		if ((Object)(object)cancelButton != (Object)null)
		{
			((UnityEvent)cancelButton.onClick).AddListener(new UnityAction(OnCancelClick));
		}
		if ((Object)(object)bgButton != (Object)null)
		{
			((UnityEvent)bgButton.onClick).AddListener(new UnityAction(OnCancelClick));
		}
	}

	public void Show(string text, Action onConfirm, Action onCancel)
	{
		if ((Object)(object)dialogText != (Object)null && !string.IsNullOrEmpty(text))
		{
			((TMP_Text)dialogText).text = text;
		}
		_onConfirm = onConfirm;
		_onCancel = onCancel;
		if ((Object)(object)cancelButton != (Object)null)
		{
			((Component)cancelButton).gameObject.SetActive(_onCancel != null);
		}
		((Component)this).gameObject.SetActive(true);
	}

	public void Hide()
	{
		((Component)this).gameObject.SetActive(false);
	}

	private void OnConfirmClick()
	{
		_onConfirm?.Invoke();
		Hide();
	}

	private void OnCancelClick()
	{
		_onCancel?.Invoke();
		Hide();
	}

	private void OnDestroy()
	{
		if ((Object)(object)confirmButton != (Object)null)
		{
			((UnityEventBase)confirmButton.onClick).RemoveAllListeners();
		}
		if ((Object)(object)cancelButton != (Object)null)
		{
			((UnityEventBase)cancelButton.onClick).RemoveAllListeners();
		}
		if ((Object)(object)bgButton != (Object)null)
		{
			((UnityEventBase)bgButton.onClick).RemoveAllListeners();
		}
	}
}

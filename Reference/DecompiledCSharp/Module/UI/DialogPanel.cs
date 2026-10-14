using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Module.UI;

public class DialogPanel : MonoBehaviour
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

	[Header("位置配置")]
	[Tooltip("顶部位置")]
	[SerializeField]
	private Vector2 topPosition = new Vector2(0f, 200f);

	[Tooltip("中间位置")]
	[SerializeField]
	private Vector2 centerPosition = new Vector2(0f, 0f);

	[Tooltip("底部位置")]
	[SerializeField]
	private Vector2 bottomPosition = new Vector2(0f, -200f);

	public Button GetBgButton()
	{
		return bgButton;
	}

	private void Awake()
	{
		if ((Object)(object)backgroundImage == (Object)null)
		{
			backgroundImage = ((Component)this).GetComponent<Image>();
		}
		if ((Object)(object)textParent == (Object)null)
		{
			TextMeshProUGUI obj = dialogText;
			textParent = ((obj != null) ? ((TMP_Text)obj).rectTransform : null);
		}
	}

	public void SetText(string text)
	{
		if ((Object)(object)dialogText != (Object)null)
		{
			((TMP_Text)dialogText).text = text;
		}
	}

	public void SetTextColor(Color color)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)dialogText != (Object)null)
		{
			((Graphic)dialogText).color = color;
		}
	}

	public void SetTextFontSize(int fontSize)
	{
		if ((Object)(object)dialogText != (Object)null)
		{
			((TMP_Text)dialogText).fontSize = fontSize;
		}
	}

	public void SetBackgroundAlpha(float alpha)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)backgroundImage != (Object)null)
		{
			Color color = ((Graphic)backgroundImage).color;
			color.a = Mathf.Clamp01(alpha);
			((Graphic)backgroundImage).color = color;
		}
	}

	public void SetDialogPosition(DialogPosition position)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)textParent != (Object)null)
		{
			switch (position)
			{
			case DialogPosition.Top:
				textParent.anchoredPosition = topPosition;
				break;
			case DialogPosition.Center:
				textParent.anchoredPosition = centerPosition;
				break;
			case DialogPosition.Bottom:
				textParent.anchoredPosition = bottomPosition;
				break;
			}
		}
	}

	public void SetDialogPosition(Vector2 customPosition)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)textParent != (Object)null)
		{
			textParent.anchoredPosition = customPosition;
		}
	}

	public void Show()
	{
		((Component)this).gameObject.SetActive(true);
	}

	public void Hide()
	{
		((Component)this).gameObject.SetActive(false);
	}

	public void ToggleVisibility(bool isVisible)
	{
		((Component)this).gameObject.SetActive(isVisible);
	}
}

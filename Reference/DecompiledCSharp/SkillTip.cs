using TMPro;
using UnityEngine;

public class SkillTip : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI tipText;

	private RectTransform _rectTransform;

	public RectTransform rectTransform
	{
		get
		{
			if ((Object)(object)_rectTransform == (Object)null)
			{
				_rectTransform = ((Component)this).GetComponent<RectTransform>();
			}
			return _rectTransform;
		}
	}

	public void SetText(string text)
	{
		if ((Object)(object)tipText != (Object)null)
		{
			((TMP_Text)tipText).text = text;
		}
	}

	public string GetText()
	{
		if (!((Object)(object)tipText != (Object)null))
		{
			return string.Empty;
		}
		return ((TMP_Text)tipText).text;
	}
}

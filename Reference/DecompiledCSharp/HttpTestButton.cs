using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class HttpTestButton : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private TextMeshProUGUI text;

	private Action _onClick;

	public string ButtonText
	{
		get
		{
			if (!((Object)(object)text != (Object)null))
			{
				return "";
			}
			return ((TMP_Text)text).text;
		}
		set
		{
			if ((Object)(object)text != (Object)null)
			{
				((TMP_Text)text).text = value;
			}
		}
	}

	public void Initialize(string displayText, Action onClick)
	{
		ButtonText = displayText;
		_onClick = onClick;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		_onClick?.Invoke();
	}
}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Module.Facility;

public class SkillTips : MonoBehaviour
{
	[Header("UI 元素")]
	[SerializeField]
	private Image skillIcon;

	[SerializeField]
	private Image skillLock;

	[SerializeField]
	private TextMeshProUGUI skillDes;

	[SerializeField]
	private TextMeshProUGUI skillName;

	[SerializeField]
	private TextMeshProUGUI skillCondition;

	[SerializeField]
	private Button bgButton;

	private Action _onCloseCallback;

	public void Initialize(bool isUnlocked, int unlockLevel, string name, string desc, int helperId, int skillLevel, Action onCloseCallback = null)
	{
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Expected O, but got Unknown
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		_onCloseCallback = onCloseCallback;
		if ((Object)(object)skillName != (Object)null && !string.IsNullOrEmpty(name))
		{
			((TMP_Text)skillName).text = name;
		}
		if ((Object)(object)skillDes != (Object)null)
		{
			((TMP_Text)skillDes).text = desc;
		}
		if ((Object)(object)skillCondition != (Object)null)
		{
			((TMP_Text)skillCondition).text = $"Lv.{unlockLevel}解锁";
		}
		if ((Object)(object)skillLock != (Object)null)
		{
			((Component)skillLock).gameObject.SetActive(!isUnlocked);
		}
		if ((Object)(object)skillIcon != (Object)null)
		{
			((Component)skillIcon).gameObject.SetActive(false);
			SingletonMono<ResManager>.Instance.LoadSpriteAsync(Constants.GetHelperSkillIconPath(helperId, skillLevel), delegate(Sprite sprite)
			{
				if ((Object)(object)sprite != (Object)null && (Object)(object)skillIcon != (Object)null)
				{
					skillIcon.sprite = sprite;
					((Component)skillIcon).gameObject.SetActive(true);
				}
			});
		}
		if ((Object)(object)bgButton != (Object)null)
		{
			RectTransform component = ((Component)bgButton).GetComponent<RectTransform>();
			if ((Object)(object)component != (Object)null)
			{
				component.anchorMin = Vector2.zero;
				component.anchorMax = Vector2.one;
				component.anchoredPosition = Vector2.zero;
				component.sizeDelta = Vector2.zero;
			}
			((UnityEventBase)bgButton.onClick).RemoveAllListeners();
			((UnityEvent)bgButton.onClick).AddListener((UnityAction)delegate
			{
				Object.Destroy((Object)(object)((Component)this).gameObject);
			});
		}
	}

	private void OnDestroy()
	{
		_onCloseCallback?.Invoke();
	}
}

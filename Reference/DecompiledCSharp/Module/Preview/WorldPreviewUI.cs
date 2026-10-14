using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Module.Preview;

public class WorldPreviewUI : MonoBehaviour
{
	[Header("Buttons")]
	[SerializeField]
	private Button bgButton;

	[SerializeField]
	private Button closeButton;

	[Header("Text")]
	[SerializeField]
	private TextMeshProUGUI descText;

	private void Awake()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		if ((Object)(object)bgButton != (Object)null)
		{
			((UnityEvent)bgButton.onClick).AddListener(new UnityAction(CloseUI));
		}
		if ((Object)(object)closeButton != (Object)null)
		{
			((UnityEvent)closeButton.onClick).AddListener(new UnityAction(CloseUI));
		}
	}

	private void CloseUI()
	{
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	public void OnOpen()
	{
		if ((Object)(object)descText != (Object)null)
		{
			int num = (int)ConfigManager.Instance.GetCommonFactor(59);
			((TMP_Text)descText).text = $"完成成长等级<color=#96ff00>{num}</color>级即可解锁世界";
		}
	}
}

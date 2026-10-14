using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Module.UI;

public class PauseUI : MonoBehaviour
{
	[Header("UI Elements")]
	[SerializeField]
	private Button continueButton;

	[SerializeField]
	private Button returnMenuButton;

	private void Awake()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		if ((Object)(object)continueButton != (Object)null)
		{
			((UnityEvent)continueButton.onClick).AddListener((UnityAction)delegate
			{
				SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
				Time.timeScale = 1f;
				Hide();
			});
		}
		if ((Object)(object)returnMenuButton != (Object)null)
		{
			((UnityEvent)returnMenuButton.onClick).AddListener((UnityAction)delegate
			{
				SoundManager.Instance.PlaySound("Assets/GameRes/sound/effect/btnclick.mp3");
				Time.timeScale = 1f;
				Hide();
			});
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
}

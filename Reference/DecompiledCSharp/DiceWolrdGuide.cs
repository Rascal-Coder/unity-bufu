using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DiceWolrdGuide : MonoBehaviour
{
	[Header("mask1")]
	[SerializeField]
	private GameObject mask1;

	[Header("bg1")]
	[SerializeField]
	private GameObject bg1;

	[Header("引导进入世界面板按键")]
	[SerializeField]
	private Button enterWorldBtn;

	[Header("引导物体")]
	[SerializeField]
	private GameObject guideEnterDiceWorld;

	[Header("引导打开确定面板按键")]
	[SerializeField]
	private Button enterDiceWorldBtn;

	[Header("引导物体")]
	[SerializeField]
	private GameObject guideFinger;

	[Header("切换世界确定框物体")]
	[SerializeField]
	private GameObject guideChangeWorld;

	[Header("引导进入骰子世界按键")]
	[SerializeField]
	private Button changeworldBtn;

	[Header("世界按键的假图标，用于高亮显示")]
	[SerializeField]
	private RectTransform tubiao;

	private void Start()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Expected O, but got Unknown
		guideEnterDiceWorld.SetActive(false);
		if ((Object)(object)enterWorldBtn != (Object)null)
		{
			Vector3 val = UpgradePanel.Instance?.GetButtonPosition(6) ?? Vector3.zero;
			Vector3 val2 = Camera.main.WorldToScreenPoint(val);
			Vector2 zero = Vector2.zero;
			Transform transform = ((Component)this).transform;
			RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)(object)((transform is RectTransform) ? transform : null), Vector2.op_Implicit(val2), Camera.main, ref zero);
			Transform transform2 = ((Component)enterWorldBtn).transform;
			((RectTransform)((transform2 is RectTransform) ? transform2 : null)).anchoredPosition = zero;
			tubiao.anchoredPosition = zero;
			((UnityEvent)enterWorldBtn.onClick).AddListener((UnityAction)delegate
			{
				mask1.SetActive(false);
				bg1.SetActive(false);
				((Component)enterWorldBtn).gameObject.SetActive(false);
				UpgradePanel.Instance?.OnButtonClicked(6);
				PlayerPrefs.SetInt("HasShownWorldSelectionGuide", 0);
				PlayerPrefs.Save();
				UIManager.Instance?.ShowWorldUI();
				GuideEnterDiceWorld();
			});
		}
	}

	public void GuideEnterDiceWorld()
	{
		if ((Object)(object)guideEnterDiceWorld != (Object)null)
		{
			guideEnterDiceWorld.SetActive(true);
		}
		if ((Object)(object)enterDiceWorldBtn != (Object)null)
		{
			((Component)enterDiceWorldBtn).gameObject.SetActive(false);
		}
		if ((Object)(object)guideFinger != (Object)null)
		{
			guideFinger.SetActive(false);
		}
		if ((Object)(object)guideChangeWorld != (Object)null)
		{
			guideChangeWorld.SetActive(false);
		}
		if ((Object)(object)guideEnterDiceWorld != (Object)null)
		{
			guideEnterDiceWorld.SetActive(true);
		}
		((MonoBehaviour)this).StartCoroutine(WaitDisplay(2.5f, delegate
		{
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Expected O, but got Unknown
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Expected O, but got Unknown
			if ((Object)(object)guideEnterDiceWorld != (Object)null)
			{
				if ((Object)(object)guideFinger != (Object)null)
				{
					guideFinger.SetActive(true);
				}
				if ((Object)(object)enterDiceWorldBtn != (Object)null)
				{
					((Component)enterDiceWorldBtn).gameObject.SetActive(true);
				}
				if ((Object)(object)enterDiceWorldBtn != (Object)null)
				{
					((UnityEvent)enterDiceWorldBtn.onClick).AddListener((UnityAction)delegate
					{
						if ((Object)(object)guideChangeWorld != (Object)null)
						{
							guideChangeWorld.SetActive(true);
						}
					});
				}
				if ((Object)(object)changeworldBtn != (Object)null)
				{
					((UnityEvent)changeworldBtn.onClick).AddListener((UnityAction)delegate
					{
						guideEnterDiceWorld.SetActive(false);
						((Component)this).gameObject.SetActive(false);
						if (!GameDataManager.Instance.TrySwitchWorld(2, out var error))
						{
							BroadcastManager.ShowPopUpTip(error);
						}
						else
						{
							PlayerPrefs.SetInt("IsUnlockDiceWorld", 1);
							if (GameDataManager.Instance.IsWorldUnlocked(2))
							{
								UIManager.Instance?.HideWorldUI();
								if ((Object)(object)UpgradePanel.Instance != (Object)null)
								{
									UpgradePanel.Instance.OnButtonClicked(1);
								}
							}
						}
					});
				}
			}
		}));
	}

	private IEnumerator WaitDisplay(float time, Action action)
	{
		yield return (object)new WaitForSeconds(time);
		action();
	}
}

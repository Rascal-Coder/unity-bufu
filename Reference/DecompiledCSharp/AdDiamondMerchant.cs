using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AdDiamondMerchant : MonoBehaviour
{
	[Header("mask遮罩物体")]
	[SerializeField]
	private GameObject mask;

	[Header("超级666物体")]
	[SerializeField]
	private GameObject supper;

	[Header("钻石商人物体")]
	[SerializeField]
	private GameObject zuanshi;

	[Header("超级666物体按键")]
	[SerializeField]
	private Button supperBtn;

	[Header("钻石商人物体按键")]
	[SerializeField]
	private Button zuanshiBtn;

	private void Start()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		DownItem item = UIManager.Instance.GetDownItem();
		((UnityEvent)supperBtn.onClick).AddListener((UnityAction)delegate
		{
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Expected O, but got Unknown
			mask.SetActive(false);
			if ((Object)(object)item != (Object)null)
			{
				ShortcutExtensions.DOScale(supper.transform, new Vector3(0.1f, 0.1f, 0.1f), 1f);
				Transform transform = supper.transform;
				TweenSettingsExtensions.OnComplete<TweenerCore<Vector2, Vector2, VectorOptions>>(((RectTransform)(object)((transform is RectTransform) ? transform : null)).DOAnchorPos(item.GetSupperUIPos() + new Vector2(-50f, 0f), 1.5f), (TweenCallback)delegate
				{
					Object.Destroy((Object)(object)((Component)this).gameObject);
				});
			}
		});
		((UnityEvent)zuanshiBtn.onClick).AddListener((UnityAction)delegate
		{
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Expected O, but got Unknown
			mask.SetActive(false);
			if ((Object)(object)item != (Object)null)
			{
				ShortcutExtensions.DOScale(zuanshi.transform, new Vector3(0.1f, 0.1f, 0.1f), 1f);
				Transform transform = zuanshi.transform;
				TweenSettingsExtensions.OnComplete<TweenerCore<Vector2, Vector2, VectorOptions>>(((RectTransform)(object)((transform is RectTransform) ? transform : null)).DOAnchorPos(item.GetZuanShiUIPos() + new Vector2(-50f, 100f), 1.5f), (TweenCallback)delegate
				{
					Object.Destroy((Object)(object)((Component)this).gameObject);
				});
			}
		});
	}

	public void SetObjActive(bool isSupper)
	{
		supper.SetActive(false);
		zuanshi.SetActive(false);
		if (isSupper)
		{
			supper.SetActive(true);
		}
		else
		{
			zuanshi.SetActive(true);
		}
	}
}

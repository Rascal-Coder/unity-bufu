using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Spine;
using Spine.Unity;
using UnityEngine;

public class RoleAD : MonoBehaviour
{
	[Header("广告人物创建位置")]
	public Vector2 createPosition;

	[Header("持续时间配置表的ID")]
	public int commonId;

	[Header("广告人物持续时间")]
	private float duration;

	[Header("加油提示框物体")]
	public GameObject jiayou;

	private SkeletonGraphic skeletonGraphic;

	private void Start()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)this).transform;
		((RectTransform)((transform is RectTransform) ? transform : null)).anchoredPosition = createPosition;
		duration = ConfigManager.Instance.GetCommonFactor(commonId) * 60f;
		if (duration <= 0f)
		{
			duration = 5f;
		}
		skeletonGraphic = ((Component)this).GetComponent<SkeletonGraphic>();
		skeletonGraphic.AnimationState.SetAnimation(0, "idle_front", true);
		skeletonGraphic.timeScale = 0.5f;
		SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/Role/helperAdd.prefab", delegate(GameObject prefab)
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Expected O, but got Unknown
			if ((Object)(object)Object.FindObjectOfType<MainScene>() != (Object)null)
			{
				GameObject creatParticle = Object.Instantiate<GameObject>(prefab, ((Component)this).transform);
				creatParticle.transform.localPosition = Vector3.zero;
				SkeletonGraphic component = ((Component)creatParticle.transform).GetComponent<SkeletonGraphic>();
				if (component.AnimationState != null)
				{
					component.AnimationState.Complete += (TrackEntryDelegate)delegate
					{
						Object.Destroy((Object)(object)creatParticle);
					};
				}
			}
		});
		MainScene mainScene = Object.FindObjectOfType<MainScene>();
		if ((Object)(object)mainScene != (Object)null)
		{
			Transform rolePool = mainScene.GetRolePool();
			((Component)this).transform.SetParent(rolePool);
		}
		TipAnimation();
	}

	private void Update()
	{
		if (duration > 0f)
		{
			duration -= Time.deltaTime;
		}
		else if (duration <= 0f)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	private void TipAnimation()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		jiayou.transform.position = ((Component)this).transform.position + new Vector3(0f, 140f, 0f);
		Rigidbody2D componentInChildren = jiayou.GetComponentInChildren<Rigidbody2D>();
		TweenSettingsExtensions.SetLoops<TweenerCore<Vector2, Vector2, VectorOptions>>(componentInChildren.DOMoveY(((Component)componentInChildren).transform.position.y + 15f, 0.4f), -1, (LoopType)1);
	}
}

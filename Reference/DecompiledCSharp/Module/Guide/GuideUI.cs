using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace Module.Guide;

public class GuideUI : MonoBehaviour
{
	private static GuideUI _instance;

	[Header("组件引用")]
	[SerializeField]
	private Image maskImage;

	[SerializeField]
	private SkeletonGraphic fingerAnimation;

	private RectTransform _fingerRect;

	private Canvas _canvas;

	private RectTransform targetRectTransform;

	private GameObject clickBlockerObj;

	[SerializeField]
	private GuideEventForward eventForward;

	public static GuideUI Instance
	{
		get
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Expected O, but got Unknown
			if ((Object)(object)_instance == (Object)null)
			{
				_instance = Object.FindObjectOfType<GuideUI>();
				if ((Object)(object)_instance == (Object)null)
				{
					GameObject val = new GameObject("GuideUI");
					_instance = val.AddComponent<GuideUI>();
					_instance.Initialize();
					Object.DontDestroyOnLoad((Object)val);
				}
			}
			return _instance;
		}
	}

	private void Awake()
	{
		if ((Object)(object)_instance != (Object)null && (Object)(object)_instance != (Object)(object)this)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		_instance = this;
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		Initialize();
	}

	private void Initialize()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected O, but got Unknown
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		_canvas = ((Component)this).GetComponent<Canvas>();
		if ((Object)(object)_canvas == (Object)null)
		{
			_canvas = ((Component)this).gameObject.AddComponent<Canvas>();
			_canvas.renderMode = (RenderMode)0;
			_canvas.sortingOrder = 1000;
		}
		if ((Object)(object)maskImage == (Object)null)
		{
			GameObject val = new GameObject("MaskImage");
			val.transform.SetParent(((Component)this).transform);
			maskImage = val.AddComponent<Image>();
			((Graphic)maskImage).color = new Color(0f, 0f, 0f, 0.7f);
			RectTransform component = val.GetComponent<RectTransform>();
			component.anchorMin = Vector2.zero;
			component.anchorMax = Vector2.one;
			component.sizeDelta = Vector2.zero;
		}
		if ((Object)(object)fingerAnimation == (Object)null)
		{
			GameObject val2 = new GameObject("FingerAnimation");
			val2.transform.SetParent(((Component)this).transform);
			fingerAnimation = val2.AddComponent<SkeletonGraphic>();
		}
		_fingerRect = ((Component)fingerAnimation).GetComponent<RectTransform>();
		if ((Object)(object)_fingerRect == (Object)null)
		{
			_fingerRect = ((Component)fingerAnimation).gameObject.AddComponent<RectTransform>();
		}
		_fingerRect.anchorMin = new Vector2(0.5f, 0.5f);
		_fingerRect.anchorMax = new Vector2(0.5f, 0.5f);
		_fingerRect.pivot = new Vector2(0.5f, 0.5f);
		((Component)this).gameObject.SetActive(false);
	}

	public void Show(RectTransform targetTransform, Vector2? offset = null)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).gameObject.SetActive(true);
		((Behaviour)maskImage).enabled = true;
		Vector2 val = Vector2.op_Implicit(((Transform)targetTransform).position);
		if (offset.HasValue)
		{
			val += offset.Value;
		}
		((Transform)_fingerRect).position = Vector2.op_Implicit(val);
		if ((Object)(object)fingerAnimation != (Object)null)
		{
			((Component)fingerAnimation).gameObject.SetActive(true);
			fingerAnimation.AnimationState.SetAnimation(0, "click", true);
		}
	}

	public void Show(Vector2 screenPosition)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).gameObject.SetActive(true);
		((Behaviour)maskImage).enabled = true;
		((Transform)_fingerRect).position = Vector2.op_Implicit(screenPosition);
		if ((Object)(object)fingerAnimation != (Object)null)
		{
			((Component)fingerAnimation).gameObject.SetActive(true);
			fingerAnimation.AnimationState.SetAnimation(0, "click", true);
		}
	}

	public void Hide()
	{
		((Component)this).gameObject.SetActive(false);
		if ((Object)(object)fingerAnimation != (Object)null)
		{
			((Component)fingerAnimation).gameObject.SetActive(false);
		}
	}

	public bool IsVisible()
	{
		return ((Component)this).gameObject.activeSelf;
	}

	public void SetMaskAlpha(float alpha)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)maskImage != (Object)null)
		{
			Color color = ((Graphic)maskImage).color;
			color.a = alpha;
			((Graphic)maskImage).color = color;
		}
	}

	public void SetFingerAnimation(string animationName, bool loop = true)
	{
		if ((Object)(object)fingerAnimation != (Object)null && !string.IsNullOrEmpty(animationName))
		{
			fingerAnimation.AnimationState.SetAnimation(0, animationName, loop);
		}
	}

	public unsafe void SetHighlightArea(Transform targetObj)
	{
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)targetObj == (Object)null || (Object)(object)maskImage == (Object)null || (Object)(object)((Graphic)maskImage).material == (Object)null)
		{
			return;
		}
		RectTransform component = ((Component)targetObj).GetComponent<RectTransform>();
		Vector3 val2;
		Vector2 val5 = default(Vector2);
		if ((Object)(object)component != (Object)null)
		{
			targetRectTransform = component;
			Canvas componentInParent = ((Component)targetObj).GetComponentInParent<Canvas>();
			if ((Object)(object)componentInParent == (Object)null)
			{
				return;
			}
			Camera obj = (((int)componentInParent.renderMode == 0) ? null : Camera.main);
			Vector3[] array = (Vector3[])(object)new Vector3[4];
			component.GetWorldCorners(array);
			Vector3 val = (array[0] + array[2]) / 2f;
			val2 = Vector2.op_Implicit(RectTransformUtility.WorldToScreenPoint(obj, val));
			Vector3 val3 = Vector2.op_Implicit(RectTransformUtility.WorldToScreenPoint(obj, array[0]));
			Vector3 val4 = Vector2.op_Implicit(RectTransformUtility.WorldToScreenPoint(obj, array[2]));
			((Vector2)(ref val5))._002Ector(Mathf.Abs(val4.x - val3.x), Mathf.Abs(val4.y - val3.y));
		}
		else
		{
			Renderer component2 = ((Component)targetObj).GetComponent<Renderer>();
			if ((Object)(object)component2 != (Object)null)
			{
				Bounds bounds = component2.bounds;
				Vector3 center = ((Bounds)(ref bounds)).center;
				val2 = Camera.main.WorldToScreenPoint(center);
				Vector3 extents = ((Bounds)(ref bounds)).extents;
				Vector3 val6 = center - extents;
				Vector3 val7 = center + extents;
				Vector3 val8 = Camera.main.WorldToScreenPoint(val6);
				Vector3 val9 = Camera.main.WorldToScreenPoint(val7);
				((Vector2)(ref val5))._002Ector(Mathf.Abs(val9.x - val8.x), Mathf.Abs(val9.y - val8.y));
			}
			else
			{
				Vector3 position = ((Component)targetObj).transform.position;
				val2 = Camera.main.WorldToScreenPoint(position);
				((Vector2)(ref val5))._002Ector(100f, 100f);
			}
		}
		float num = val2.x / (float)Screen.width;
		float num2 = val2.y / (float)Screen.height;
		Vector2 val10 = default(Vector2);
		((Vector2)(ref val10))._002Ector(num, num2);
		float num3 = Mathf.Max(val5.x, val5.y);
		Vector2 val11 = new Vector2(num3 / (float)Screen.width, num3 / (float)Screen.height) * 1.2f;
		((Graphic)maskImage).material.SetVector("_HighlightPos", Vector4.op_Implicit(val10));
		((Graphic)maskImage).material.SetVector("_HighlightSize", Vector4.op_Implicit(val11));
		if ((Object)(object)clickBlockerObj != (Object)null)
		{
			RectTransform component3 = clickBlockerObj.GetComponent<RectTransform>();
			if ((Object)(object)component3 != (Object)null)
			{
				component3.anchorMin = Vector2.zero;
				component3.anchorMax = Vector2.one;
				component3.sizeDelta = Vector2.zero;
				component3.anchoredPosition = Vector2.zero;
			}
		}
		eventForward.SetTargetPos(Vector2.op_Implicit(val2));
		string[] obj2 = new string[6] { "设置高亮区域位置: ", null, null, null, null, null };
		Vector2 val12 = val10;
		obj2[1] = ((object)(*(Vector2*)(&val12))/*cast due to .constrained prefix*/).ToString();
		obj2[2] = "，大小: ";
		val12 = val11;
		obj2[3] = ((object)(*(Vector2*)(&val12))/*cast due to .constrained prefix*/).ToString();
		obj2[4] = "，屏幕位置: ";
		Vector3 val13 = val2;
		obj2[5] = ((object)(*(Vector3*)(&val13))/*cast due to .constrained prefix*/).ToString();
		Debug.Log((object)string.Concat(obj2));
	}
}

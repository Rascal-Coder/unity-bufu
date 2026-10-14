using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIHelp
{
	private static bool IsPointerOverUI()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)EventSystem.current == (Object)null)
		{
			return false;
		}
		if (Input.touchCount > 0)
		{
			for (int i = 0; i < Input.touchCount; i++)
			{
				EventSystem current = EventSystem.current;
				Touch touch = Input.GetTouch(i);
				if (current.IsPointerOverGameObject(((Touch)(ref touch)).fingerId))
				{
					return true;
				}
			}
		}
		if (EventSystem.current.IsPointerOverGameObject())
		{
			return true;
		}
		PointerEventData val = new PointerEventData(EventSystem.current)
		{
			position = Vector2.op_Implicit(Input.mousePosition)
		};
		List<RaycastResult> list = new List<RaycastResult>();
		EventSystem.current.RaycastAll(val, list);
		return list.Count > 0;
	}

	public static Component GetComponentChildByName(Transform parent, string name)
	{
		Component[] componentsInChildren = ((Component)parent).GetComponentsInChildren(typeof(Transform));
		foreach (Component val in componentsInChildren)
		{
			if (((Object)val).name.Equals(name))
			{
				return val;
			}
		}
		return null;
	}

	public static Transform[] GetChildTransforms(Transform parent, string[] names)
	{
		int num = names.Length;
		Transform[] array = (Transform[])(object)new Transform[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = parent.Find(names[i]);
		}
		return array;
	}

	public static GameObject[] GetChildGameObjects(Transform parent, string[] names)
	{
		int num = names.Length;
		GameObject[] array = (GameObject[])(object)new GameObject[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = ((Component)parent.Find(names[i])).gameObject;
		}
		return array;
	}

	public static void GetPosition(Transform transform, out float x, out float y, out float z)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		x = 0f;
		y = 0f;
		z = 0f;
		if ((Object)(object)transform != (Object)null)
		{
			x = transform.position.x;
			y = transform.position.y;
			z = transform.position.z;
		}
	}

	public static void SetPosition(Transform transform, float x, float y, float z = 0f)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)transform != (Object)null)
		{
			transform.position = new Vector3(x, y, z);
		}
	}

	public static void SetPositionX(Transform transform, float x)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)transform != (Object)null)
		{
			transform.position = new Vector3(x, transform.position.y, transform.position.z);
		}
	}

	public static void SetPositionY(Transform transform, float y)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)transform != (Object)null)
		{
			transform.position = new Vector3(transform.position.x, y, transform.position.z);
		}
	}

	public static void SetPositionZ(Transform transform, float z)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)transform != (Object)null)
		{
			transform.position = new Vector3(transform.position.x, transform.position.y, z);
		}
	}

	public static float GetPositionX(Transform transform)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)transform != (Object)null)
		{
			return transform.position.x;
		}
		return 0f;
	}

	public static float GetPositionY(Transform transform)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)transform != (Object)null)
		{
			return transform.position.y;
		}
		return 0f;
	}

	public static float GetPositionZ(Transform transform)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)transform != (Object)null)
		{
			return transform.position.z;
		}
		return 0f;
	}

	public static void SetLocalPosition(Transform transform, float x, float y, float z)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)transform != (Object)null)
		{
			transform.localPosition = new Vector3(x, y, z);
		}
	}

	public static void SetLocalPositionXY(Transform transform, float x, float y)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)transform != (Object)null)
		{
			transform.localPosition = new Vector3(x, y, transform.localPosition.z);
		}
	}

	public static void SetLocalPositionX(Transform transform, float x)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)transform != (Object)null)
		{
			transform.localPosition = new Vector3(x, transform.localPosition.y, transform.localPosition.z);
		}
	}

	public static void SetLocalPositionY(Transform transform, float y)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)transform != (Object)null)
		{
			transform.localPosition = new Vector3(transform.localPosition.x, y, transform.localPosition.z);
		}
	}

	public static void SetLocalPositionZ(Transform transform, float z)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)transform != (Object)null)
		{
			transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, z);
		}
	}

	public static void GetLocalPosition(Transform transform, out float x, out float y, out float z)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		x = 0f;
		y = 0f;
		z = 0f;
		if ((Object)(object)transform != (Object)null)
		{
			x = transform.localPosition.x;
			y = transform.localPosition.y;
			z = transform.localPosition.z;
		}
	}

	public static float GetLocalPositionX(Transform transform)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)transform != (Object)null)
		{
			return transform.localPosition.x;
		}
		return 0f;
	}

	public static float GetLocalPositionY(Transform transform)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)transform != (Object)null)
		{
			return transform.localPosition.y;
		}
		return 0f;
	}

	public static float GetLocalPositionZ(Transform transform)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)transform != (Object)null)
		{
			return transform.localPosition.z;
		}
		return 0f;
	}

	public static void SetAnchoredPosition(Transform transform, float x, float y)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)((transform is RectTransform) ? transform : null) != (Object)null)
		{
			((RectTransform)((transform is RectTransform) ? transform : null)).anchoredPosition = new Vector2(x, y);
		}
	}

	public static void SetAnchoredPosition(RectTransform transform, float x, float y)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)transform != (Object)null)
		{
			transform.anchoredPosition = new Vector2(x, y);
		}
	}

	public static void GetAnchoredPosition(RectTransform transform, out float x, out float y)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		x = 0f;
		y = 0f;
		if ((Object)(object)transform != (Object)null)
		{
			x = transform.anchoredPosition.x;
			y = transform.anchoredPosition.y;
		}
	}

	public static void SetLocalScale(Transform transform, float x, float y, float z = 1f)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)transform != (Object)null)
		{
			transform.localScale = new Vector3(x, y, z);
		}
	}

	public static void GetScale(Transform transform, out float x, out float y, out float z)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		x = 0f;
		y = 0f;
		z = 0f;
		if ((Object)(object)transform != (Object)null)
		{
			x = transform.lossyScale.x;
			y = transform.lossyScale.y;
			z = transform.lossyScale.z;
		}
	}

	public static void GetLocalScale(Transform transform, out float x, out float y, out float z)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		x = 0f;
		y = 0f;
		z = 0f;
		if ((Object)(object)transform != (Object)null)
		{
			x = transform.localScale.x;
			y = transform.localScale.y;
			z = transform.localScale.z;
		}
	}

	public static void SetLocalRotation(Transform transform, float x, float y, float z)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)transform != (Object)null)
		{
			transform.localRotation = Quaternion.Euler(new Vector3(x, y, z));
		}
	}

	public static void GetLocalRotation(Transform transform, out float x, out float y, out float z)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		x = 0f;
		y = 0f;
		z = 0f;
		if ((Object)(object)transform != (Object)null)
		{
			x = transform.eulerAngles.x;
			y = transform.eulerAngles.y;
			z = transform.eulerAngles.z;
		}
	}

	public static void GetRotation(Transform transform, out float x, out float y, out float z)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		x = 0f;
		y = 0f;
		z = 0f;
		if ((Object)(object)transform != (Object)null)
		{
			x = transform.rotation.x;
			y = transform.rotation.y;
			z = transform.rotation.z;
		}
	}

	public static void SetRotate(Transform transform, Vector3 rotate)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)transform != (Object)null)
		{
			transform.Rotate(rotate);
		}
	}

	public static void SetRotate(Transform transform, float x, float y, float z = 0f)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)transform != (Object)null)
		{
			SetRotate(transform, new Vector3(x, y, z));
		}
	}

	public static void GetRenderBoundsSize(SkinnedMeshRenderer render, out float x, out float y, out float z)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		x = 0f;
		y = 0f;
		z = 0f;
		if ((Object)(object)render != (Object)null)
		{
			Bounds bounds = ((Renderer)render).bounds;
			x = ((Bounds)(ref bounds)).size.x;
			bounds = ((Renderer)render).bounds;
			y = ((Bounds)(ref bounds)).size.y;
			bounds = ((Renderer)render).bounds;
			z = ((Bounds)(ref bounds)).size.z;
		}
	}

	public static void GetObjectSize(Transform transform, out float x, out float y, out float z)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 zero = Vector3.zero;
		x = zero.x;
		y = zero.y;
		z = zero.z;
		Mesh mesh = ((Component)transform).GetComponent<MeshFilter>().mesh;
		if (!((Object)(object)mesh == (Object)null))
		{
			Bounds bounds = mesh.bounds;
			Vector3 size = ((Bounds)(ref bounds)).size;
			Vector3 lossyScale = transform.lossyScale;
			((Vector3)(ref zero))._002Ector(size.x * lossyScale.x, size.y * lossyScale.y, size.z * lossyScale.z);
			x = zero.x;
			y = zero.y;
			z = zero.z;
		}
	}

	public static void SetSkinnedColor(SkinnedMeshRenderer render)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)render != (Object)null)
		{
			((Renderer)render).material.color = Color.red;
		}
	}

	public static void SetSkinnedColor(Transform transform)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		((Renderer)((Component)transform).GetComponent<MeshRenderer>()).material.color = Color.red;
	}

	public static void SetSizeDelta(RectTransform transform, float x, float y)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)transform != (Object)null)
		{
			transform.sizeDelta = new Vector2(x, y);
		}
	}

	public static void SetSizeDeltaX(RectTransform transform, float x)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)transform != (Object)null)
		{
			transform.sizeDelta = new Vector2(x, transform.sizeDelta.y);
		}
	}

	public static void SetSizeDeltaY(RectTransform transform, float y)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)transform != (Object)null)
		{
			transform.sizeDelta = new Vector2(transform.sizeDelta.x, y);
		}
	}

	public static float GetSizeDeltaX(RectTransform transform)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)transform != (Object)null)
		{
			return transform.sizeDelta.x;
		}
		return 0f;
	}

	public static float GetSizeDeltaY(RectTransform transform)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)transform != (Object)null)
		{
			return transform.sizeDelta.y;
		}
		return 0f;
	}

	public static void SetChildLayer(GameObject obj, int layer)
	{
		if (!((Object)(object)obj == (Object)null))
		{
			SetChildLayer(obj.transform, layer);
		}
	}

	public static void SetChildLayer(Transform transform, int layer)
	{
		if ((Object)(object)transform == (Object)null)
		{
			return;
		}
		((Component)transform).gameObject.layer = layer;
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			((Component)child).gameObject.layer = layer;
			if (child.childCount > 0)
			{
				SetChildLayer(child, layer);
			}
		}
	}

	public static void SetParticleSpeed(GameObject obj, float _speed)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)obj == (Object)null))
		{
			Animator component = obj.GetComponent<Animator>();
			if ((Object)(object)component != (Object)null)
			{
				component.speed = _speed;
			}
			ParticleSystem[] componentsInChildren = obj.GetComponentsInChildren<ParticleSystem>(true);
			int num = componentsInChildren.Length;
			for (int i = 0; i < num; i++)
			{
				MainModule main = componentsInChildren[i].main;
				((MainModule)(ref main)).simulationSpeed = _speed;
			}
			ParticleSystem component2 = ((Component)obj.transform).GetComponent<ParticleSystem>();
			if ((Object)(object)component2 != (Object)null)
			{
				MainModule main2 = component2.main;
				((MainModule)(ref main2)).simulationSpeed = _speed;
			}
		}
	}

	public static int GetParticlePlayState(GameObject obj)
	{
		if ((Object)(object)obj == (Object)null)
		{
			return 0;
		}
		ParticleSystem[] componentsInChildren = obj.GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (!componentsInChildren[i].isStopped)
			{
				return 1;
			}
		}
		return 0;
	}

	public static void SetParticleLoop(GameObject obj, bool loop)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)obj == (Object)null))
		{
			ParticleSystem[] componentsInChildren = obj.GetComponentsInChildren<ParticleSystem>(true);
			int num = componentsInChildren.Length;
			for (int i = 0; i < num; i++)
			{
				MainModule main = componentsInChildren[i].main;
				((MainModule)(ref main)).loop = loop;
			}
			ParticleSystem component = ((Component)obj.transform).GetComponent<ParticleSystem>();
			if ((Object)(object)component != (Object)null)
			{
				MainModule main2 = component.main;
				((MainModule)(ref main2)).loop = loop;
			}
		}
	}

	public static float GetParticleSystemLength(GameObject obj)
	{
		return GetParticleSystemLength(obj.transform);
	}

	public static float GetParticleSystemLength(Transform transform)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		ParticleSystem[] componentsInChildren = ((Component)transform).GetComponentsInChildren<ParticleSystem>();
		float num = 0f;
		ParticleSystem[] array = componentsInChildren;
		foreach (ParticleSystem val in array)
		{
			float num2 = num;
			MainModule main = val.main;
			num = Mathf.Max(num2, ((MainModule)(ref main)).duration);
		}
		return num;
	}

	public static void PlayParticle(GameObject obj, bool flag)
	{
		Animation[] componentsInChildren = obj.GetComponentsInChildren<Animation>();
		foreach (Animation val in componentsInChildren)
		{
			if (flag)
			{
				val.Play();
			}
			else
			{
				val.Stop();
			}
		}
		ParticleSystem[] componentsInChildren2 = obj.GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem val2 in componentsInChildren2)
		{
			if (flag)
			{
				val2.Play();
			}
			else
			{
				val2.Pause();
			}
		}
	}

	public static void SetColor(Text text, float r, float g, float b, float a = 255f)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)text).color = new Color(r / 255f, g / 255f, b / 255f, a / 255f);
	}

	public static void SetColor(Image image, float r, float g, float b, float a = 255f)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)image).color = new Color(r / 255f, g / 255f, b / 255f, a / 255f);
	}

	public static void SetColor(Material mat, float r, float g, float b, float a = 255f)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		mat.color = new Color(r / 255f, g / 255f, b / 255f, a / 255f);
	}

	public static void SetOutLineColor(Outline outline, float r, float g, float b, float a = 255f)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		((Shadow)outline).effectColor = new Color(r / 255f, g / 255f, b / 255f, a / 255f);
	}

	public static float GetAlpha(Text text)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return ((Graphic)text).color.a;
	}

	public static float GetAlpha(Image image)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return ((Graphic)image).color.a;
	}

	public static float GetAlpha(Outline outline)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return ((Shadow)outline).effectColor.a;
	}

	public static float GetAlpha(Material ma)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return ma.color.a;
	}

	public static float GetAlpha(CanvasGroup canvasGroup)
	{
		return canvasGroup.alpha;
	}

	public static void SetAlpha(SpriteRenderer render, float a)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		render.color = new Color(render.color.r, render.color.g, render.color.b, a);
	}

	public static void SetAlpha(Text text, float a)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)text).color = new Color(((Graphic)text).color.r, ((Graphic)text).color.g, ((Graphic)text).color.b, a);
	}

	public static void SetAlpha(Image image, float a)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)image).color = new Color(((Graphic)image).color.r, ((Graphic)image).color.g, ((Graphic)image).color.b, a);
	}

	public static void SetAlpha(Outline outline, float a)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		((Shadow)outline).effectColor = new Color(((Shadow)outline).effectColor.r, ((Shadow)outline).effectColor.g, ((Shadow)outline).effectColor.b, a);
	}

	public static void SetAlpha(Material ma, float a)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		ma.color = new Color(ma.color.r, ma.color.g, ma.color.b, a);
	}

	public static void SetAlpha(CanvasGroup canvasGroup, float a)
	{
		canvasGroup.alpha = a;
	}

	public static float GetClipLength(Animator animator, string clip)
	{
		if ((Object)null == (Object)(object)animator || string.IsNullOrEmpty(clip) || (Object)null == (Object)(object)animator.runtimeAnimatorController)
		{
			return 0f;
		}
		RuntimeAnimatorController runtimeAnimatorController = animator.runtimeAnimatorController;
		AnimationClip[] animationClips = runtimeAnimatorController.animationClips;
		if (animationClips == null || animationClips.Length == 0)
		{
			return 0f;
		}
		for (int i = 0; i < animationClips.Length; i++)
		{
			AnimationClip val = runtimeAnimatorController.animationClips[i];
			if ((Object)null != (Object)(object)val && ((Object)val).name == clip)
			{
				return val.length;
			}
		}
		return 0f;
	}

	public static void HtmlColorStringToColor(string colorStr, out float r, out float g, out float b, out float a)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		Color val = default(Color);
		ColorUtility.TryParseHtmlString(colorStr, ref val);
		r = val.r * 255f;
		g = val.g * 255f;
		b = val.b * 255f;
		a = val.a * 255f;
	}

	public static string ColorToHtmlColorString(Color color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return ColorUtility.ToHtmlStringRGB(color);
	}

	public static string ColorToHtmlColorString(float r, float g, float b, float a)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		return ColorToHtmlColorString(new Color(r, g, b, a));
	}

	public static void SetHorizontalLayoutGroupSpacing(Transform transform, float f)
	{
		HorizontalLayoutGroup component = ((Component)transform).GetComponent<HorizontalLayoutGroup>();
		if ((Object)(object)component != (Object)null)
		{
			((HorizontalOrVerticalLayoutGroup)component).spacing = f;
		}
	}

	public static void ClearTranformChildEditor(Transform tr, GameObject excepts = null)
	{
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < tr.childCount; i++)
		{
			if ((Object)(object)((Component)tr.GetChild(i)).gameObject != (Object)(object)excepts)
			{
				list.Add(((Component)tr.GetChild(i)).gameObject);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			Object.DestroyImmediate((Object)(object)list[j].gameObject);
		}
	}

	public static void SetSortingOrder(Transform transform, int order)
	{
		Renderer[] componentsInChildren = ((Component)transform).GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].sortingOrder = order;
		}
		Renderer component = ((Component)transform).GetComponent<Renderer>();
		if ((Object)(object)component != (Object)null)
		{
			component.sortingOrder = order;
		}
	}

	public static Vector2 AnchoredLeftBottomToCenter(RectTransform rect)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		Vector2 anchoredPosition = rect.anchoredPosition;
		Rect rect2 = rect.rect;
		float num = ((Rect)(ref rect2)).width * 0.5f;
		rect2 = rect.rect;
		return anchoredPosition + new Vector2(num, ((Rect)(ref rect2)).height * 0.5f);
	}
}

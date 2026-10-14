using UnityEngine;
using UnityEngine.UI;

public static class ExtendedMono
{
	public static Transform GetChildByIndex(this Transform tr, int index)
	{
		if (index >= 100)
		{
			int num = Mathf.FloorToInt((float)index / 100f);
			int num2 = index % 100;
			if (tr.childCount > num)
			{
				Transform child = tr.GetChild(num);
				if ((Object)(object)child != (Object)null)
				{
					return child.GetChild(num2);
				}
			}
		}
		if (tr.childCount > index)
		{
			return tr.GetChild(index);
		}
		return null;
	}

	public static T GetOrAddComponent<T>(this Transform go) where T : Component
	{
		T val = ((Component)go).GetComponent<T>();
		if ((Object)(object)val == (Object)null)
		{
			val = ((Component)go).gameObject.AddComponent<T>();
		}
		return val;
	}

	public static T GetOrAddComponent<T>(this GameObject go) where T : Component
	{
		T val = go.GetComponent<T>();
		if ((Object)(object)val == (Object)null)
		{
			val = go.AddComponent<T>();
		}
		return val;
	}

	public static T Get<T>(this GameObject go, string childName) where T : MonoBehaviour
	{
		if (string.IsNullOrEmpty(childName))
		{
			return go.GetComponent<T>();
		}
		return ((Component)go.transform.Find(childName)).gameObject.GetComponent<T>();
	}

	public static Button GetButton(this GameObject go, string childName)
	{
		if (string.IsNullOrEmpty(childName))
		{
			return go.GetComponent<Button>();
		}
		return ((Component)go.transform.Find(childName)).gameObject.GetComponent<Button>();
	}

	public static Text GetText(this GameObject go, string childName)
	{
		if (string.IsNullOrEmpty(childName))
		{
			return go.GetComponent<Text>();
		}
		return ((Component)go.transform.Find(childName)).gameObject.GetComponent<Text>();
	}

	public static Toggle GetToggle(this GameObject go, string childName)
	{
		if (string.IsNullOrEmpty(childName))
		{
			return go.GetComponent<Toggle>();
		}
		return ((Component)go.transform.Find(childName)).gameObject.GetComponent<Toggle>();
	}

	public static ToggleGroup GetToggleGroup(this GameObject go, string childName)
	{
		if (string.IsNullOrEmpty(childName))
		{
			return go.GetComponent<ToggleGroup>();
		}
		return ((Component)go.transform.Find(childName)).gameObject.GetComponent<ToggleGroup>();
	}

	public static InputField GetInputField(this GameObject go, string childName)
	{
		if (string.IsNullOrEmpty(childName))
		{
			return go.GetComponent<InputField>();
		}
		return ((Component)go.transform.Find(childName)).gameObject.GetComponent<InputField>();
	}

	public static Slider GetSlider(this GameObject go, string childName)
	{
		if (string.IsNullOrEmpty(childName))
		{
			return go.GetComponent<Slider>();
		}
		return ((Component)go.transform.Find(childName)).gameObject.GetComponent<Slider>();
	}

	public static Scrollbar GetScrollbar(this GameObject go, string childName)
	{
		if (string.IsNullOrEmpty(childName))
		{
			return go.GetComponent<Scrollbar>();
		}
		return ((Component)go.transform.Find(childName)).gameObject.GetComponent<Scrollbar>();
	}

	public static Image GetImage(this GameObject go, string childName)
	{
		if (string.IsNullOrEmpty(childName))
		{
			return go.GetComponent<Image>();
		}
		return ((Component)go.transform.Find(childName)).gameObject.GetComponent<Image>();
	}

	public static RawImage GetRawImage(this GameObject go, string childName)
	{
		if (string.IsNullOrEmpty(childName))
		{
			return go.GetComponent<RawImage>();
		}
		return ((Component)go.transform.Find(childName)).gameObject.GetComponent<RawImage>();
	}

	public static Sprite GetSprite(this GameObject go, string childName)
	{
		if (string.IsNullOrEmpty(childName))
		{
			return go.GetComponent<Sprite>();
		}
		return ((Component)go.transform.Find(childName)).gameObject.GetComponent<Sprite>();
	}

	public static ScrollRect GetScrollRect(this GameObject go, string childName)
	{
		if (string.IsNullOrEmpty(childName))
		{
			return go.GetComponent<ScrollRect>();
		}
		return ((Component)go.transform.Find(childName)).gameObject.GetComponent<ScrollRect>();
	}

	public static Dropdown GetDropDown(this GameObject go, string childName)
	{
		if (string.IsNullOrEmpty(childName))
		{
			return go.GetComponent<Dropdown>();
		}
		return ((Component)go.transform.Find(childName)).gameObject.GetComponent<Dropdown>();
	}

	public static RectTransform GetRectTransform(this GameObject go, string childName)
	{
		if (string.IsNullOrEmpty(childName))
		{
			return go.GetComponent<RectTransform>();
		}
		return ((Component)go.transform.Find(childName)).gameObject.GetComponent<RectTransform>();
	}

	public static Camera GetCamera(this GameObject go, string childName)
	{
		if (string.IsNullOrEmpty(childName))
		{
			return go.GetComponent<Camera>();
		}
		return ((Component)go.transform.Find(childName)).gameObject.GetComponent<Camera>();
	}

	public static CanvasGroup GetCanvasGroup(this GameObject go, string childName)
	{
		if (string.IsNullOrEmpty(childName))
		{
			return go.GetComponent<CanvasGroup>();
		}
		return ((Component)go.transform.Find(childName)).gameObject.GetComponent<CanvasGroup>();
	}

	public static AudioSource GetAudioSource(this GameObject go, string childName)
	{
		if (string.IsNullOrEmpty(childName))
		{
			return go.GetComponent<AudioSource>();
		}
		return ((Component)go.transform.Find(childName)).gameObject.GetComponent<AudioSource>();
	}

	public static Animator GetAnimator(this GameObject go, string childName)
	{
		if (string.IsNullOrEmpty(childName))
		{
			return go.GetComponent<Animator>();
		}
		return ((Component)go.transform.Find(childName)).gameObject.GetComponent<Animator>();
	}

	public static BoxCollider2D GetBoxCollider2D(this GameObject go, string childName)
	{
		if (string.IsNullOrEmpty(childName))
		{
			return go.GetComponent<BoxCollider2D>();
		}
		return ((Component)go.transform.Find(childName)).gameObject.GetComponent<BoxCollider2D>();
	}

	public static PolygonCollider2D GetPolygonCollider2D(this GameObject go, string childName)
	{
		if (string.IsNullOrEmpty(childName))
		{
			return go.GetComponent<PolygonCollider2D>();
		}
		return ((Component)go.transform.Find(childName)).gameObject.GetComponent<PolygonCollider2D>();
	}

	public static CircleCollider2D GetCirCollider2D(this GameObject go, string childName)
	{
		if (string.IsNullOrEmpty(childName))
		{
			return go.GetComponent<CircleCollider2D>();
		}
		return ((Component)go.transform.Find(childName)).gameObject.GetComponent<CircleCollider2D>();
	}

	public static SpriteRenderer GetSpriteRender(this GameObject go, string childName)
	{
		if (string.IsNullOrEmpty(childName))
		{
			return go.GetComponent<SpriteRenderer>();
		}
		return ((Component)go.transform.Find(childName)).gameObject.GetComponent<SpriteRenderer>();
	}

	public static TrailRenderer GetTrailRenderer(this GameObject go, string childName)
	{
		if (string.IsNullOrEmpty(childName))
		{
			return go.GetComponent<TrailRenderer>();
		}
		return ((Component)go.transform.Find(childName)).gameObject.GetComponent<TrailRenderer>();
	}

	public static void SetPositionX(this Transform tr, float x)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = tr.position;
		position.x = x;
		tr.position = position;
	}

	public static void SetPositionY(this Transform tr, float y)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = tr.position;
		position.y = y;
		tr.position = position;
	}

	public static void SetPositionZ(this Transform tr, float z)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = tr.position;
		position.z = z;
		tr.position = position;
	}

	public static void SetLocalPositionX(this Transform tr, float x)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = tr.localPosition;
		localPosition.x = x;
		tr.localPosition = localPosition;
	}

	public static void SetLocalPositionY(this Transform tr, float y)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = tr.localPosition;
		localPosition.y = y;
		tr.localPosition = localPosition;
	}

	public static void SetLocalPositionZ(this Transform tr, float z)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = tr.localPosition;
		localPosition.z = z;
		tr.localPosition = localPosition;
	}

	public static void SetLocalPositionXY(this Transform tr, float x, float y)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = tr.localPosition;
		localPosition.x = x;
		localPosition.y = y;
		tr.localPosition = localPosition;
	}

	public static void SetLocalScaleX(this Transform tr, float x)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localScale = tr.localScale;
		localScale.x = x;
		tr.localScale = localScale;
	}

	public static void SetLocalScaleY(this Transform tr, float y)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localScale = tr.localScale;
		localScale.y = y;
		tr.localScale = localScale;
	}

	public static void SetLocalScaleXY(this Transform tr, float x, float y)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localScale = tr.localScale;
		localScale.x = x;
		localScale.y = y;
		tr.localScale = localScale;
	}

	public static void SetAnchor(this RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)rectTransform == (Object)null))
		{
			Vector3 position = ((Transform)rectTransform).position;
			rectTransform.anchorMin = anchorMin;
			rectTransform.anchorMax = anchorMax;
			((Transform)rectTransform).position = position;
		}
	}

	public static void SetAnchor(this RectTransform rectTransform, Vector2 anchor)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		rectTransform.SetAnchor(anchor, anchor);
	}

	public static void SetAnchoredPositionX(this RectTransform rectTransform, float x)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Vector2 anchoredPosition = rectTransform.anchoredPosition;
		anchoredPosition.x = x;
		rectTransform.anchoredPosition = anchoredPosition;
	}

	public static void SetAnchoredPositionY(this RectTransform rectTransform, float y)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Vector2 anchoredPosition = rectTransform.anchoredPosition;
		anchoredPosition.y = y;
		rectTransform.anchoredPosition = anchoredPosition;
	}

	public static void SetAnchoredPositionXY(this RectTransform rectTransform, float x, float y)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Vector2 anchoredPosition = rectTransform.anchoredPosition;
		anchoredPosition.x = x;
		anchoredPosition.y = y;
		rectTransform.anchoredPosition = anchoredPosition;
	}

	public static void SetColor(this Image image, Color color)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)image != (Object)null)
		{
			((Graphic)image).color = color;
		}
	}

	public static void SetAlpha(this Image image, float alpha)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)image != (Object)null)
		{
			Color color = ((Graphic)image).color;
			color.a = Mathf.Clamp01(alpha);
			((Graphic)image).color = color;
		}
	}

	public static float GetAlpha(this Image image)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)image != (Object)null)
		{
			return ((Graphic)image).color.a;
		}
		return 0f;
	}

	public static void SetSprite(this Image image, Sprite sprite)
	{
		if ((Object)(object)image != (Object)null)
		{
			image.sprite = sprite;
		}
	}

	public static void SetVisible(this Image image, bool visible)
	{
		if ((Object)(object)image != (Object)null)
		{
			((Behaviour)image).enabled = visible;
		}
	}

	public static void ToggleVisible(this Image image)
	{
		if ((Object)(object)image != (Object)null)
		{
			((Behaviour)image).enabled = !((Behaviour)image).enabled;
		}
	}

	public static void SetColor(this RawImage rawImage, Color color)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rawImage != (Object)null)
		{
			((Graphic)rawImage).color = color;
		}
	}

	public static void SetAlpha(this RawImage rawImage, float alpha)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rawImage != (Object)null)
		{
			Color color = ((Graphic)rawImage).color;
			color.a = Mathf.Clamp01(alpha);
			((Graphic)rawImage).color = color;
		}
	}

	public static float GetAlpha(this RawImage rawImage)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rawImage != (Object)null)
		{
			return ((Graphic)rawImage).color.a;
		}
		return 0f;
	}

	public static void SetTexture(this RawImage rawImage, Texture texture)
	{
		if ((Object)(object)rawImage != (Object)null)
		{
			rawImage.texture = texture;
		}
	}

	public static void SetVisible(this RawImage rawImage, bool visible)
	{
		if ((Object)(object)rawImage != (Object)null)
		{
			((Behaviour)rawImage).enabled = visible;
		}
	}

	public static void ToggleVisible(this RawImage rawImage)
	{
		if ((Object)(object)rawImage != (Object)null)
		{
			((Behaviour)rawImage).enabled = !((Behaviour)rawImage).enabled;
		}
	}
}

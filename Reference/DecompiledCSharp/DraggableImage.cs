using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableImage : MonoBehaviour, IDragHandler, IEventSystemHandler, IBeginDragHandler, IEndDragHandler
{
	[Header("拖动设置")]
	[SerializeField]
	private bool restrictToScreen = true;

	[SerializeField]
	private bool useSpringEffect;

	[SerializeField]
	private float springDamping = 5f;

	[SerializeField]
	private float springStiffness = 100f;

	[Header("位置限制范围")]
	[SerializeField]
	private float minX = -200f;

	[SerializeField]
	private float maxX = 200f;

	[SerializeField]
	private float minY = -200f;

	[SerializeField]
	private float maxY = 200f;

	[Header("放下缓动效果")]
	[SerializeField]
	private bool useDragReleaseEasing = true;

	[SerializeField]
	private float easingDuration = 0.3f;

	[SerializeField]
	private AnimationCurve easingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	private RectTransform rectTransform;

	private Vector2 originalPosition;

	private bool isDragging;

	private Vector2 velocity = Vector2.zero;

	private CanvasGroup canvasGroup;

	private Vector2 lastDragPosition;

	private Vector2 dragReleaseVelocity = Vector2.zero;

	private bool isEasing;

	private void Awake()
	{
		Initialize();
	}

	private void Initialize()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		rectTransform = ((Component)this).GetComponent<RectTransform>();
		if ((Object)(object)rectTransform == (Object)null)
		{
			Debug.LogError((object)"DraggableImage需要RectTransform组件！");
			((Behaviour)this).enabled = false;
			return;
		}
		originalPosition = rectTransform.anchoredPosition;
		lastDragPosition = rectTransform.anchoredPosition;
		canvasGroup = ((Component)this).GetComponent<CanvasGroup>();
		if ((Object)(object)canvasGroup == (Object)null)
		{
			canvasGroup = ((Component)this).gameObject.AddComponent<CanvasGroup>();
		}
	}

	private void Update()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (isDragging)
		{
			Vector2 anchoredPosition = rectTransform.anchoredPosition;
			dragReleaseVelocity = (anchoredPosition - lastDragPosition) / Time.deltaTime;
			lastDragPosition = anchoredPosition;
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		isDragging = true;
		isEasing = false;
		if ((Object)(object)canvasGroup != (Object)null)
		{
			canvasGroup.blocksRaycasts = false;
		}
		dragReleaseVelocity = Vector2.zero;
		lastDragPosition = rectTransform.anchoredPosition;
	}

	public void OnDrag(PointerEventData eventData)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (isDragging && !((Object)(object)rectTransform == (Object)null) && !isEasing)
		{
			RectTransform obj = rectTransform;
			obj.anchoredPosition += eventData.delta;
			if (restrictToScreen)
			{
				ClampToScreen();
			}
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		isDragging = false;
		if ((Object)(object)canvasGroup != (Object)null)
		{
			canvasGroup.alpha = 1f;
			canvasGroup.blocksRaycasts = true;
		}
		if (useDragReleaseEasing && !useSpringEffect)
		{
			((MonoBehaviour)this).StartCoroutine(EaseToRest());
		}
	}

	private void ClampToScreen()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)rectTransform == (Object)null))
		{
			Vector2 anchoredPosition = rectTransform.anchoredPosition;
			float num = Mathf.Clamp(anchoredPosition.x, minX, maxX);
			float num2 = Mathf.Clamp(anchoredPosition.y, minY, maxY);
			rectTransform.anchoredPosition = new Vector2(num, num2);
		}
	}

	private IEnumerator EaseToRest()
	{
		isEasing = true;
		float elapsedTime = 0f;
		Vector2 startPosition = rectTransform.anchoredPosition;
		Vector2 finalPosition = rectTransform.anchoredPosition;
		finalPosition += dragReleaseVelocity * easingDuration * 0.5f;
		if (restrictToScreen)
		{
			finalPosition.x = Mathf.Clamp(finalPosition.x, minX, maxX);
			finalPosition.y = Mathf.Clamp(finalPosition.y, minY, maxY);
		}
		while (elapsedTime < easingDuration)
		{
			elapsedTime += Time.deltaTime;
			float num = Mathf.Clamp01(elapsedTime / easingDuration);
			float num2 = easingCurve.Evaluate(num);
			rectTransform.anchoredPosition = Vector2.Lerp(startPosition, finalPosition, num2);
			if (restrictToScreen)
			{
				ClampToScreen();
			}
			yield return null;
		}
		rectTransform.anchoredPosition = finalPosition;
		if (restrictToScreen)
		{
			ClampToScreen();
		}
		isEasing = false;
		dragReleaseVelocity = Vector2.zero;
	}

	public void ResetPosition()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rectTransform == (Object)null)
		{
			return;
		}
		isEasing = false;
		((MonoBehaviour)this).StopAllCoroutines();
		if (useSpringEffect)
		{
			((MonoBehaviour)this).StartCoroutine(SpringToPosition(originalPosition));
			return;
		}
		rectTransform.anchoredPosition = originalPosition;
		if (restrictToScreen)
		{
			ClampToScreen();
		}
	}

	private IEnumerator SpringToPosition(Vector2 targetPosition)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		while (Vector2.Distance(rectTransform.anchoredPosition, targetPosition) > 0.1f)
		{
			Vector2 val = (targetPosition - rectTransform.anchoredPosition) * springStiffness - velocity * springDamping;
			velocity += val * Time.deltaTime;
			RectTransform obj = rectTransform;
			obj.anchoredPosition += velocity * Time.deltaTime;
			if (restrictToScreen)
			{
				ClampToScreen();
			}
			yield return null;
		}
		rectTransform.anchoredPosition = targetPosition;
		velocity = Vector2.zero;
		if (restrictToScreen)
		{
			ClampToScreen();
		}
	}

	public void SetRestrictToScreen(bool restrict)
	{
		restrictToScreen = restrict;
	}

	public void SetPosition(Vector2 position)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)rectTransform == (Object)null))
		{
			rectTransform.anchoredPosition = position;
			if (restrictToScreen)
			{
				ClampToScreen();
			}
		}
	}

	public void SetBoundary(float newMinX, float newMaxX, float newMinY, float newMaxY)
	{
		minX = newMinX;
		maxX = newMaxX;
		minY = newMinY;
		maxY = newMaxY;
		if (restrictToScreen)
		{
			ClampToScreen();
		}
	}
}

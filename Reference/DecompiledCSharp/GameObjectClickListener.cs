using UnityEngine;
using UnityEngine.EventSystems;

public class GameObjectClickListener : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IDragHandler
{
	public delegate void VoidDelegate(GameObject go, float x, float y);

	public VoidDelegate onClick;

	private bool isDragging;

	private Vector2 startPosition;

	private const float dragThreshold = 5f;

	public void OnPointerDown(PointerEventData eventData)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		isDragging = false;
		startPosition = eventData.position;
	}

	public void OnDrag(PointerEventData eventData)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		if (Vector2.Distance(startPosition, eventData.position) > 5f)
		{
			isDragging = true;
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (!isDragging && onClick != null)
		{
			onClick(((Component)this).gameObject, eventData.position.x, eventData.position.y);
		}
		isDragging = false;
	}

	public void OutClick()
	{
		if (!isDragging && onClick != null)
		{
			onClick(((Component)this).gameObject, 0f, 0f);
		}
		isDragging = false;
	}

	public void Call()
	{
		if (onClick != null)
		{
			onClick(((Component)this).gameObject, 0f, 0f);
		}
	}

	public static GameObjectClickListener Get(GameObject go)
	{
		GameObjectClickListener gameObjectClickListener = go.GetComponent<GameObjectClickListener>();
		if ((Object)(object)gameObjectClickListener == (Object)null)
		{
			gameObjectClickListener = go.AddComponent<GameObjectClickListener>();
		}
		return gameObjectClickListener;
	}

	public static void Remove(GameObject go)
	{
		GameObjectClickListener component = go.GetComponent<GameObjectClickListener>();
		if ((Object)(object)component != (Object)null)
		{
			component.onClick = null;
		}
	}
}

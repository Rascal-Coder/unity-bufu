using UnityEngine;
using UnityEngine.EventSystems;

public class GuideEventForward : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler
{
	[Header("目标按钮（拖入需要转发事件的按钮）")]
	public GuideTarget guideTarget;

	[Header("是否开启事件转发（引导结束后关闭）")]
	public bool isForward = true;

	[Header("判断模式：true=精确矩形判断，false=圆形区域判断")]
	public bool usePreciseRectCheck = true;

	private RectTransform _targetRt;

	private Camera _uiCamera;

	private Vector2 _position;

	private void Awake()
	{
		if ((Object)(object)guideTarget != (Object)null)
		{
			_targetRt = ((Component)guideTarget).GetComponent<RectTransform>();
		}
	}

	public void SetTarget(GuideTarget guideTarget)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Invalid comparison between Unknown and I4
		this.guideTarget = guideTarget;
		_targetRt = ((Component)this.guideTarget).GetComponent<RectTransform>();
		Canvas componentInParent = ((Component)this.guideTarget).GetComponentInParent<Canvas>();
		if ((Object)(object)componentInParent != (Object)null && (int)componentInParent.renderMode == 1)
		{
			_uiCamera = componentInParent.worldCamera;
		}
		Debug.Log((object)$"SetTarget: targetBtn={((Object)this.guideTarget).name}, _targetRt={(Object)(object)_targetRt != (Object)null}, _uiCamera={(Object)(object)_uiCamera != (Object)null}");
	}

	public void SetTargetPos(Vector2 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		_position = position;
	}

	private bool IsInTargetArea(Vector2 screenPos)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (!isForward || (Object)(object)guideTarget == (Object)null)
		{
			return false;
		}
		if (usePreciseRectCheck)
		{
			if ((Object)(object)_targetRt != (Object)null)
			{
				bool flag = RectTransformUtility.RectangleContainsScreenPoint(_targetRt, screenPos, _uiCamera);
				Debug.Log((object)$"Precise rect check result: {flag}");
				return flag;
			}
			Debug.LogWarning((object)"_targetRt is null, falling back to circle check");
			return Vector2.Distance(screenPos, _position) <= 200f;
		}
		return Vector2.Distance(screenPos, _position) <= 200f;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)$"OnPointerClick：{eventData.position}");
		if (IsInTargetArea(eventData.position) && (Object)(object)guideTarget != (Object)null)
		{
			Debug.Log((object)("点击抠孔区域，转发事件给目标按钮：" + ((Object)guideTarget).name));
			ExecuteEvents.Execute<IPointerClickHandler>(((Component)guideTarget).gameObject, (BaseEventData)(object)eventData, ExecuteEvents.pointerClickHandler);
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)$"OnPointerDown：{eventData.position}");
		if (IsInTargetArea(eventData.position))
		{
			ExecuteEvents.Execute<IPointerDownHandler>(((Component)guideTarget).gameObject, (BaseEventData)(object)eventData, ExecuteEvents.pointerDownHandler);
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)$"OnPointerUp：{eventData.position}");
		if (IsInTargetArea(eventData.position))
		{
			ExecuteEvents.Execute<IPointerUpHandler>(((Component)guideTarget).gameObject, (BaseEventData)(object)eventData, ExecuteEvents.pointerUpHandler);
		}
	}

	public void StopForward()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		guideTarget = null;
		_targetRt = null;
		_position = Vector2.zero;
		_uiCamera = null;
	}
}

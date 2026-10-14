using System;
using UnityEngine;
using UnityEngine.EventSystems;

internal sealed class WorldSelectionScrollDragRelay : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IEndDragHandler
{
	private Action _beginDrag;

	private Action _endDrag;

	public void Initialize(Action beginDrag, Action endDrag)
	{
		_beginDrag = beginDrag;
		_endDrag = endDrag;
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		_beginDrag?.Invoke();
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		_endDrag?.Invoke();
	}
}

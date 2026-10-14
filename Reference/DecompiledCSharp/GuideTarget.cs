using System;
using UnityEngine;

public class GuideTarget : MonoBehaviour
{
	[Header("引导系统查找用的Key，同一个按钮可以在多个步骤里复用")]
	public string Key;

	[Header("暂时不用")]
	[SerializeField]
	private string param;

	private const float CHECK_INTERVAL = 0.5f;

	private float _checkTimer;

	private Vector3 _lastPosition;

	public Action<Transform> OnPositionChanged;

	private void OnEnable()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		_lastPosition = ((Component)this).transform.position;
		_checkTimer = 0f;
	}

	private void Update()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		_checkTimer += Time.deltaTime;
		if (_checkTimer >= 0.5f)
		{
			if (((Component)this).transform.position != _lastPosition)
			{
				_lastPosition = ((Component)this).transform.position;
				OnPositionChanged?.Invoke(((Component)this).transform);
			}
			_checkTimer = 0f;
		}
	}

	private void OnDisable()
	{
		OnPositionChanged = null;
	}
}

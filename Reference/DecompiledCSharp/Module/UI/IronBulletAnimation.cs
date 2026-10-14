using System;
using System.Collections.Generic;
using Module.Role;
using UnityEngine;

namespace Module.UI;

public class IronBulletAnimation : MonoBehaviour
{
	private enum State
	{
		FlyingUp,
		Repositioning,
		FlyingDown,
		Exploding,
		Finished
	}

	private State _state;

	private Vector2 _startPos;

	private Vector2 _targetPos;

	private float _speed = 2500f;

	private float _explosionRadius = 250f;

	private RectTransform _rectTransform;

	private Action<CoinBase> _onHitCoin;

	private Action<Vector2> _onExplode;

	private void Awake()
	{
		_rectTransform = ((Component)this).GetComponent<RectTransform>();
	}

	public void Init(Vector2 startPos, Vector2 targetPos, Action<CoinBase> onHitCoin, Action<Vector2> onExplode = null)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		_startPos = startPos;
		_targetPos = targetPos;
		_onHitCoin = onHitCoin;
		_onExplode = onExplode;
		((Component)this).transform.position = Vector2.op_Implicit(_startPos);
		_state = State.FlyingUp;
		UpdateRotation(Vector2.up);
		((Component)this).gameObject.SetActive(true);
	}

	private void Update()
	{
		if (_state != State.Finished)
		{
			switch (_state)
			{
			case State.FlyingUp:
				FlyUp();
				break;
			case State.Repositioning:
				Reposition();
				break;
			case State.FlyingDown:
				FlyDown();
				break;
			case State.Exploding:
				Explode();
				break;
			}
		}
	}

	private void FlyUp()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.Translate(Vector2.op_Implicit(Vector2.up * _speed * Time.deltaTime), (Space)0);
		float y = ((Component)this).transform.position.y;
		Rect rect = ((Component)UIManager.Instance.GetCanvas()).GetComponent<RectTransform>().rect;
		if (y > ((Rect)(ref rect)).height / 2f + 100f)
		{
			_state = State.Repositioning;
		}
	}

	private void Reposition()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = ((Component)UIManager.Instance.GetCanvas()).GetComponent<RectTransform>().rect;
		float num = ((Rect)(ref rect)).height / 2f + 100f;
		((Component)this).transform.position = new Vector3(_targetPos.x, num, 0f);
		UpdateRotation(Vector2.down);
		_state = State.FlyingDown;
	}

	private void FlyDown()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = Vector2.op_Implicit(Vector2.MoveTowards(Vector2.op_Implicit(((Component)this).transform.position), _targetPos, _speed * Time.deltaTime));
		if (Vector2.Distance(Vector2.op_Implicit(((Component)this).transform.position), _targetPos) < 20f)
		{
			_state = State.Exploding;
		}
	}

	private void Explode()
	{
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		MainScene mainScene = UIManager.Instance.MainScene;
		if ((Object)(object)mainScene != (Object)null)
		{
			List<CoinBase> allCoins = mainScene.GetAllCoins();
			if (allCoins != null)
			{
				foreach (CoinBase item in allCoins)
				{
					if ((Object)(object)item != (Object)null && item.State == CoinState.Idle && Vector2.Distance(Vector2.op_Implicit(((Component)this).transform.position), Vector2.op_Implicit(((Component)item).transform.position)) <= _explosionRadius)
					{
						_onHitCoin?.Invoke(item);
					}
				}
			}
		}
		_onExplode?.Invoke(Vector2.op_Implicit(((Component)this).transform.position));
		_state = State.Finished;
		((Component)this).gameObject.SetActive(false);
	}

	private void UpdateRotation(Vector2 direction)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (direction != Vector2.zero)
		{
			float num = Mathf.Atan2(direction.y, direction.x) * 57.29578f - 90f;
			((Component)this).transform.rotation = Quaternion.Euler(0f, 0f, num);
		}
	}
}

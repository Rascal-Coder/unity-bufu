using System;
using System.Collections.Generic;
using Module.Role;
using UnityEngine;

namespace Module.UI;

public class ZuqiuAnimation : MonoBehaviour
{
	private float _speed;

	private Vector2 _velocity;

	private int _maxHitCount;

	private int _currentHitCount;

	private Action<CoinBase> _onHitCoin;

	private Action<IRoleThrowableVat> _onHitVatCoin;

	private HashSet<CoinBase> _hitCoins = new HashSet<CoinBase>();

	private float _hitDistance = 60f;

	private Vector2 _boundaryMin;

	private Vector2 _boundaryMax;

	private RectTransform _rectTransform;

	private ChallengeMain _challengeMain;

	private MainSceneVat _vatScene;

	private float _vatDistanceScale = 1f;

	private bool _useVatCoins;

	private readonly List<IRoleThrowableVat> _vatQueryScratch = new List<IRoleThrowableVat>();

	private readonly HashSet<IRoleThrowableVat> _hitVatCoins = new HashSet<IRoleThrowableVat>();

	public void SetChallengeMain(ChallengeMain challengeMain)
	{
		_challengeMain = challengeMain;
		_vatScene = null;
		_useVatCoins = false;
	}

	private void Awake()
	{
		_rectTransform = ((Component)this).GetComponent<RectTransform>();
	}

	public void Init(Vector2 startPos, Vector2 direction, float speed, int maxHitCount, Vector2 boundaryMin, Vector2 boundaryMax, Action<CoinBase> onHit)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_rectTransform == (Object)null)
		{
			_rectTransform = ((Component)this).GetComponent<RectTransform>();
		}
		((Transform)_rectTransform).position = Vector2.op_Implicit(startPos);
		_speed = speed;
		_velocity = ((Vector2)(ref direction)).normalized * speed;
		_maxHitCount = ((maxHitCount > 0) ? maxHitCount : 999);
		_currentHitCount = 0;
		_boundaryMin = boundaryMin;
		_boundaryMax = boundaryMax;
		_onHitCoin = onHit;
		_onHitVatCoin = null;
		_useVatCoins = false;
		_hitCoins.Clear();
		_hitVatCoins.Clear();
		UpdateRotation();
	}

	public void InitVat(Vector2 startPos, Vector2 direction, float speed, int maxHitCount, Vector2 boundaryMin, Vector2 boundaryMax, Action<IRoleThrowableVat> onHit, MainSceneVat vatScene, float distanceScale)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_rectTransform == (Object)null)
		{
			_rectTransform = ((Component)this).GetComponent<RectTransform>();
		}
		((Transform)_rectTransform).position = Vector2.op_Implicit(startPos);
		_speed = speed;
		_velocity = ((Vector2)(ref direction)).normalized * speed;
		_maxHitCount = ((maxHitCount > 0) ? maxHitCount : 999);
		_currentHitCount = 0;
		_boundaryMin = boundaryMin;
		_boundaryMax = boundaryMax;
		_onHitCoin = null;
		_onHitVatCoin = onHit;
		_challengeMain = null;
		_vatScene = vatScene;
		_vatDistanceScale = Mathf.Max(0.0001f, distanceScale);
		_useVatCoins = true;
		_hitCoins.Clear();
		_hitVatCoins.Clear();
		UpdateRotation();
	}

	private void UpdateRotation()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (_velocity != Vector2.zero)
		{
			float num = Mathf.Atan2(_velocity.y, _velocity.x) * 57.29578f;
			((Component)this).transform.rotation = Quaternion.Euler(0f, 0f, num);
		}
	}

	private void Update()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		if (!(_velocity == Vector2.zero))
		{
			Vector2 val = Vector2.op_Implicit(((Transform)_rectTransform).position) + _velocity * Time.deltaTime;
			float num = (_useVatCoins ? ScaleVatDistance(30f) : 30f);
			float num2 = _boundaryMin.x - num;
			float num3 = _boundaryMax.x + num;
			float num4 = _boundaryMin.y - num;
			float num5 = _boundaryMax.y + num;
			if (val.x < num2 || val.x > num3 || val.y < num4 || val.y > num5)
			{
				((Component)this).gameObject.SetActive(false);
				return;
			}
			((Transform)_rectTransform).position = Vector2.op_Implicit(val);
			DetectCoins();
		}
	}

	private void DetectCoins()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		if (_currentHitCount >= _maxHitCount)
		{
			return;
		}
		if (_useVatCoins)
		{
			DetectVatCoins();
			return;
		}
		List<CoinBase> allCoins;
		if ((Object)(object)_challengeMain != (Object)null)
		{
			allCoins = _challengeMain.GetAllCoins();
		}
		else
		{
			MainScene mainScene = UIManager.Instance.MainScene;
			if ((Object)(object)mainScene == (Object)null)
			{
				return;
			}
			allCoins = mainScene.GetAllCoins();
		}
		if (allCoins == null)
		{
			return;
		}
		Vector3 val = ((((Component)this).transform.childCount > 0) ? ((Component)this).transform.GetChild(0).position : ((Component)this).transform.position);
		for (int i = 0; i < allCoins.Count; i++)
		{
			CoinBase coinBase = allCoins[i];
			if ((Object)(object)coinBase != (Object)null && coinBase.State == CoinState.Idle && !_hitCoins.Contains(coinBase) && ((Object)(object)_challengeMain != (Object)null || ((Object)(object)SingletonMono<UpgradeManager>.Instance != (Object)null && SingletonMono<UpgradeManager>.Instance.CanRoleFlipCoin(coinBase.coinType))) && Vector2.Distance(Vector2.op_Implicit(val), Vector2.op_Implicit(((Component)coinBase).transform.position)) <= _hitDistance)
			{
				_hitCoins.Add(coinBase);
				_onHitCoin?.Invoke(coinBase);
				_currentHitCount++;
				if (_currentHitCount >= _maxHitCount)
				{
					((Component)this).gameObject.SetActive(false);
					break;
				}
			}
		}
	}

	private void DetectVatCoins()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_vatScene == (Object)null)
		{
			return;
		}
		Vector3 val = ((((Component)this).transform.childCount > 0) ? ((Component)this).transform.GetChild(0).position : ((Component)this).transform.position);
		float radius = ScaleVatDistance(_hitDistance);
		_vatQueryScratch.Clear();
		_vatScene.GetIdleThrowablesInRadius(Vector2.op_Implicit(val), radius, _vatQueryScratch);
		for (int i = 0; i < _vatQueryScratch.Count; i++)
		{
			IRoleThrowableVat roleThrowableVat = _vatQueryScratch[i];
			if (roleThrowableVat == null || _hitVatCoins.Contains(roleThrowableVat))
			{
				continue;
			}
			bool flag = true;
			if (roleThrowableVat is CoinVat coinVat)
			{
				flag = CanFlipVatCoin(coinVat.coinType);
			}
			if (flag)
			{
				_hitVatCoins.Add(roleThrowableVat);
				_onHitVatCoin?.Invoke(roleThrowableVat);
				_currentHitCount++;
				if (_currentHitCount >= _maxHitCount)
				{
					((Component)this).gameObject.SetActive(false);
					break;
				}
			}
		}
	}

	private bool CanFlipVatCoin(CoinType coinType)
	{
		if (!((Object)(object)SingletonMono<UpgradeManager>.Instance == (Object)null))
		{
			return SingletonMono<UpgradeManager>.Instance.CanRoleFlipCoin(coinType);
		}
		return true;
	}

	private float ScaleVatDistance(float value)
	{
		return value * _vatDistanceScale;
	}
}

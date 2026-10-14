using System;
using System.Collections.Generic;
using Module.UI;
using UnityEngine;

namespace Module.SceneUI;

public class BulletUI : MonoBehaviour
{
	private float _speed = 600f;

	private float _maxDistance = 3000f;

	private float _damage = 10f;

	private float _hitRadius = 30f;

	private LayerMask _hitLayerMask = LayerMask.op_Implicit(-1);

	private string _targetTag = "Monster";

	private Vector2 _direction;

	private Vector3 _startPos;

	private Vector3 _lastWorldPos;

	private CombatEntityUI _owner;

	private bool _initialized;

	private bool _isCritical;

	private bool _explodeOnHit = true;

	private bool _hasExploded;

	private float _baseDamage;

	private float _critMultiplier = 1f;

	private float _extraCritRate;

	private bool _hasFlame;

	private bool _hasFreeze;

	private bool _hasStink;

	private bool _hasStun;

	private bool _hasHitBack;

	private float _freezeSlowRatio = 0.2f;

	private float _freezeDuration = 3f;

	private float _stinkTotalDamageRatio = 0.75f;

	private float _stinkDuration = 5f;

	private float _stinkInterval = 1f;

	private bool _canBounce;

	private int _bounceCount;

	private int _maxBounceCount;

	private readonly List<int> _passedFoodIds = new List<int>(4);

	private readonly Dictionary<CombatEntityUI, float> _hitTargets = new Dictionary<CombatEntityUI, float>();

	private const float HIT_COOLDOWN = 0.5f;

	private GameObject _flameEffect;

	private GameObject _frozenEffect;

	private GameObject _poisonEffect;

	private bool isDestroy;

	public CombatEntityUI owner => _owner;

	public bool HasFlame => _hasFlame;

	public bool HasFreeze => _hasFreeze;

	public bool HasStun => _hasStun;

	public bool HasStink => _hasStink;

	public bool CanBounce => _canBounce;

	public int BounceCount => _bounceCount;

	public int MaxBounceCount => _maxBounceCount;

	public event Action<BulletUI, CombatEntityUI> OnBulletHit;

	public event Action<BulletUI> OnBulletExplode;

	public void Initialize(Vector2 startPos, Vector2 dir, CombatEntityUI owner = null)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		_owner = owner;
		_direction = ((((Vector2)(ref dir)).sqrMagnitude <= 0.0001f) ? Vector2.right : ((Vector2)(ref dir)).normalized);
		_startPos = Vector2.op_Implicit(startPos);
		_initialized = true;
		WarpWorldPosition(Vector2.op_Implicit(startPos));
		_lastWorldPos = GetWorldPosition();
		UpdateRotation();
	}

	public void SetDirection(Vector2 newDir)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		_direction = ((((Vector2)(ref newDir)).sqrMagnitude <= 0.0001f) ? Vector2.right : ((Vector2)(ref newDir)).normalized);
		UpdateRotation();
	}

	public void SetSpeed(float value)
	{
		_speed = value;
	}

	public void SetMaxDistance(float value)
	{
		_maxDistance = value;
	}

	public void SetDamage(float value)
	{
		_damage = value;
	}

	public void SetBaseDamage(float value)
	{
		_baseDamage = value;
	}

	public void SetCritMultiplier(float value)
	{
		_critMultiplier = Mathf.Max(1f, value);
	}

	public void AddExtraCritRate(float value)
	{
		_extraCritRate = Mathf.Clamp01(_extraCritRate + value);
	}

	public void SetFlame(bool value)
	{
		_hasFlame = value;
		if (_hasFlame)
		{
			PlayFlameAnimation();
		}
	}

	public void SetFreeze(bool value)
	{
		_hasFreeze = value;
		if (_hasFreeze)
		{
			PlayFrozenAnimation();
		}
	}

	public void SetStun(bool value)
	{
		_hasStun = value;
	}

	public void ConfigureFreezeEffect(float slowRatio, float duration)
	{
		_freezeSlowRatio = Mathf.Clamp01(slowRatio);
		_freezeDuration = Mathf.Max(0.1f, duration);
	}

	public void SetStink(bool value)
	{
		_hasStink = value;
		if (_hasStink)
		{
			PlayPoisonAnimation();
		}
	}

	public void SetHitBack(bool value)
	{
		_hasHitBack = value;
	}

	public void ConfigureStinkEffect(float damageRatio, float duration, float interval)
	{
		_stinkTotalDamageRatio = Mathf.Max(0f, damageRatio);
		_stinkDuration = Mathf.Max(0.1f, duration);
		_stinkInterval = Mathf.Max(0.1f, interval);
	}

	public void SetBounce(bool canBounce, int maxBounceCount = 3)
	{
		_canBounce = canBounce;
		_maxBounceCount = maxBounceCount;
		_bounceCount = 0;
	}

	public void SetTargetTag(string value)
	{
		_targetTag = value;
	}

	public void SetCritical(bool isCritical)
	{
		_isCritical = isCritical;
	}

	public void SetExplodeOnHit(bool value)
	{
		_explodeOnHit = value;
	}

	private void Update()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		if (_initialized)
		{
			CleanupHitTargets();
			float num = _speed * Time.deltaTime;
			Vector3 val = Vector2.op_Implicit(_direction) * num;
			Vector3 lastWorldPos = _lastWorldPos;
			Vector3 val2 = lastWorldPos + val;
			WarpWorldPosition(val2);
			_lastWorldPos = val2;
			if (Vector2.Distance(Vector2.op_Implicit(_startPos), Vector2.op_Implicit(GetWorldPosition())) >= _maxDistance)
			{
				Explode();
				return;
			}
			TryPassThroughFoods();
			TryHitObstacle();
			TryHit(lastWorldPos, val2);
		}
	}

	private void CleanupHitTargets()
	{
		float time = Time.time;
		List<CombatEntityUI> list = new List<CombatEntityUI>();
		foreach (KeyValuePair<CombatEntityUI, float> hitTarget in _hitTargets)
		{
			if (time - hitTarget.Value >= 0.5f)
			{
				list.Add(hitTarget.Key);
			}
		}
		foreach (CombatEntityUI item in list)
		{
			_hitTargets.Remove(item);
		}
	}

	private bool IsTargetInCooldown(CombatEntityUI target)
	{
		if ((Object)(object)target == (Object)null)
		{
			return false;
		}
		float time = Time.time;
		if (_hitTargets.TryGetValue(target, out var value))
		{
			return time - value < 0.5f;
		}
		return false;
	}

	private void TryPassThroughFoods()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Collider2D[] array = Physics2D.OverlapCircleAll(Vector2.op_Implicit(GetWorldPosition()), _hitRadius + 4f, LayerMask.op_Implicit(_hitLayerMask));
		if (array != null && array.Length != 0)
		{
			for (int i = 0; i < array.Length; i++)
			{
				_ = (Object)(object)array[i] == (Object)null;
			}
		}
	}

	private void TryHitObstacle()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		Collider2D[] array = Physics2D.OverlapCircleAll(Vector2.op_Implicit(GetWorldPosition()), _hitRadius, LayerMask.op_Implicit(_hitLayerMask));
		if (array == null || array.Length == 0)
		{
			return;
		}
		foreach (Collider2D val in array)
		{
			if (!((Object)(object)val == (Object)null) && ((Component)val).CompareTag("Obstacle"))
			{
				if (_canBounce && _bounceCount < _maxBounceCount && TryBounceOffObstacle(val))
				{
					_bounceCount++;
				}
				else
				{
					Explode();
				}
				break;
			}
		}
	}

	private bool TryBounceOffObstacle(Collider2D obstacle)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)obstacle == (Object)null)
		{
			return false;
		}
		Vector2 val = Vector2.op_Implicit(GetWorldPosition());
		Vector2 direction = _direction;
		Vector2 val2 = obstacle.ClosestPoint(val);
		Vector2 val3 = val - val2;
		Vector2 normalized = ((Vector2)(ref val3)).normalized;
		if (((Vector2)(ref normalized)).sqrMagnitude < 0.0001f)
		{
			return false;
		}
		Vector2 val4 = Vector2.Reflect(direction, normalized);
		if (Vector2.Dot(val4, direction) > -0.9f)
		{
			return false;
		}
		SetDirection(val4);
		WarpWorldPosition(Vector2.op_Implicit(val2 + normalized * (_hitRadius + 0.1f)));
		return true;
	}

	private void TryHit(Vector3 prevPos, Vector3 nextPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Vector2.op_Implicit(prevPos);
		Vector2 val2 = Vector2.op_Implicit(nextPos);
		Vector2 val3 = val2 - val;
		float magnitude = ((Vector2)(ref val3)).magnitude;
		RaycastHit2D val4 = default(RaycastHit2D);
		CombatEntityUI combatEntityUI = null;
		bool flag = false;
		if (magnitude > 0.0001f)
		{
			Vector2 val5 = val3 / magnitude;
			RaycastHit2D[] array = Physics2D.CircleCastAll(val, _hitRadius, val5, magnitude, LayerMask.op_Implicit(_hitLayerMask));
			if (array != null && array.Length != 0)
			{
				for (int i = 0; i < array.Length; i++)
				{
					Collider2D collider = ((RaycastHit2D)(ref array[i])).collider;
					if (!((Object)(object)collider == (Object)null) && (string.IsNullOrEmpty(_targetTag) || ((Component)collider).CompareTag(_targetTag)))
					{
						CombatEntityUI componentInParent = ((Component)collider).GetComponentInParent<CombatEntityUI>();
						if (!((Object)(object)componentInParent == (Object)null) && !((Object)(object)componentInParent == (Object)(object)_owner) && (!flag || ((RaycastHit2D)(ref array[i])).distance < ((RaycastHit2D)(ref val4)).distance))
						{
							val4 = array[i];
							combatEntityUI = componentInParent;
							flag = true;
						}
					}
				}
			}
		}
		if (!flag)
		{
			Collider2D[] array2 = Physics2D.OverlapCircleAll(val2, _hitRadius, LayerMask.op_Implicit(_hitLayerMask));
			if (array2 != null && array2.Length != 0)
			{
				foreach (Collider2D val6 in array2)
				{
					if (!((Object)(object)val6 == (Object)null) && (string.IsNullOrEmpty(_targetTag) || ((Component)val6).CompareTag(_targetTag)))
					{
						CombatEntityUI componentInParent2 = ((Component)val6).GetComponentInParent<CombatEntityUI>();
						if (!((Object)(object)componentInParent2 == (Object)null) && !((Object)(object)componentInParent2 == (Object)(object)_owner))
						{
							combatEntityUI = componentInParent2;
							flag = true;
							break;
						}
					}
				}
			}
		}
		if (flag && (Object)(object)combatEntityUI != (Object)null && !IsTargetInCooldown(combatEntityUI))
		{
			HitTarget(combatEntityUI);
			_hitTargets[combatEntityUI] = Time.time;
		}
	}

	public void AddHitTarget(CombatEntityUI target)
	{
		if (!((Object)(object)target == (Object)null))
		{
			_hitTargets.Add(target, Time.time);
		}
	}

	private void HitTarget(CombatEntityUI target)
	{
		_ = target.IsAlive;
		ApplyHitEffects(target);
		if (!_hasExploded && _explodeOnHit)
		{
			Explode();
		}
		this.OnBulletHit?.Invoke(this, target);
	}

	private void CalcBuff()
	{
		_ = (Object)(object)owner != (Object)null;
	}

	private void TryApplyExtraCrit()
	{
		if (!_isCritical && !(_extraCritRate <= 0f) && !(Random.value >= _extraCritRate))
		{
			_isCritical = true;
			if (_baseDamage > 0f)
			{
				_damage = _baseDamage * _critMultiplier;
			}
		}
	}

	private void ApplyHitEffects(CombatEntityUI target)
	{
	}

	public void Explode()
	{
		if (!_hasExploded)
		{
			_hasExploded = true;
			PlayerExplodeEffect();
			this.OnBulletExplode?.Invoke(this);
			Object.Destroy((Object)(object)((Component)this).gameObject);
			Object.Destroy((Object)(object)_flameEffect);
			Object.Destroy((Object)(object)_frozenEffect);
			Object.Destroy((Object)(object)_poisonEffect);
		}
	}

	private void PlayerExplodeEffect()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)((Component)this).transform == (Object)null)
		{
			return;
		}
		Transform p = ((Component)this).transform.parent;
		Vector2 pos = default(Vector2);
		ref Vector2 reference = ref pos;
		Transform transform = ((Component)this).transform;
		reference = ((RectTransform)((transform is RectTransform) ? transform : null)).anchoredPosition;
		SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/NewBullet/monster_blood.prefab", delegate(GameObject prefab)
		{
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			GameObject bloodEffect = Object.Instantiate<GameObject>(prefab, p);
			bloodEffect.transform.SetAsLastSibling();
			Transform transform2 = bloodEffect.transform;
			((RectTransform)((transform2 is RectTransform) ? transform2 : null)).anchoredPosition = pos;
			bloodEffect.transform.localScale = Vector3.one * 1f;
			UISpriteAnimationSequence component = bloodEffect.GetComponent<UISpriteAnimationSequence>();
			if ((Object)(object)component != (Object)null)
			{
				component.Play();
				component.OnAnimationComplete = (Action)Delegate.Combine(component.OnAnimationComplete, (Action)delegate
				{
					Object.Destroy((Object)(object)bloodEffect);
				});
			}
		});
	}

	private void UpdateRotation()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Atan2(_direction.y, _direction.x) * 57.29578f;
		((Component)this).transform.rotation = Quaternion.Euler(0f, 0f, num);
	}

	public float GetDamage()
	{
		return _damage;
	}

	public bool IsCritical()
	{
		return _isCritical;
	}

	public Vector3 GetWorldPosition()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)this).transform;
		RectTransform val = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		if ((Object)(object)val != (Object)null)
		{
			return ((Transform)val).position;
		}
		return ((Component)this).transform.position;
	}

	private void WarpWorldPosition(Vector3 worldPos)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)this).transform;
		RectTransform val = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		if ((Object)(object)val != (Object)null)
		{
			((Transform)val).position = worldPos;
		}
		else
		{
			((Component)this).transform.position = worldPos;
		}
	}

	private void PlayFlameAnimation()
	{
		SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/NewBullet/fire.prefab", delegate(GameObject prefab)
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			if (!isDestroy)
			{
				_flameEffect = Object.Instantiate<GameObject>(prefab, ((Component)this).transform);
				Transform transform = _flameEffect.transform;
				((RectTransform)((transform is RectTransform) ? transform : null)).anchoredPosition = Vector2.zero;
				_flameEffect.transform.SetAsLastSibling();
				_flameEffect.transform.localScale = Vector3.one * 2.5f;
				_flameEffect.transform.Rotate(0f, 0f, -90f);
				UISpriteAnimationSequence component = _flameEffect.GetComponent<UISpriteAnimationSequence>();
				if ((Object)(object)component != (Object)null)
				{
					component.Play();
				}
			}
		});
	}

	private void PlayFrozenAnimation()
	{
		SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/NewBullet/frozen.prefab", delegate(GameObject prefab)
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			if (!isDestroy)
			{
				_frozenEffect = Object.Instantiate<GameObject>(prefab, ((Component)this).transform);
				Transform transform = _frozenEffect.transform;
				((RectTransform)((transform is RectTransform) ? transform : null)).anchoredPosition = Vector2.zero;
				_frozenEffect.transform.SetAsLastSibling();
				_frozenEffect.transform.localScale = Vector3.one * 2.5f;
				_frozenEffect.transform.Rotate(0f, 0f, -90f);
				UISpriteAnimationSequence component = _frozenEffect.GetComponent<UISpriteAnimationSequence>();
				if ((Object)(object)component != (Object)null)
				{
					component.Play();
				}
			}
		});
	}

	private void PlayPoisonAnimation()
	{
		SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/NewBullet/poison.prefab", delegate(GameObject prefab)
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			if (!isDestroy)
			{
				_poisonEffect = Object.Instantiate<GameObject>(prefab, ((Component)this).transform);
				Transform transform = _poisonEffect.transform;
				((RectTransform)((transform is RectTransform) ? transform : null)).anchoredPosition = Vector2.zero;
				_poisonEffect.transform.SetAsLastSibling();
				_poisonEffect.transform.localScale = Vector3.one * 2.5f;
				_poisonEffect.transform.Rotate(0f, 0f, -90f);
				UISpriteAnimationSequence component = _poisonEffect.GetComponent<UISpriteAnimationSequence>();
				if ((Object)(object)component != (Object)null)
				{
					component.Play();
				}
			}
		});
	}

	private void OnDestroy()
	{
		isDestroy = true;
	}
}

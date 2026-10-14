using UnityEngine;

namespace Module.SceneUI;

public class CombatEntityUI : MovableEntityUI
{
	protected bool _isKnockedBack;

	protected Vector3 _knockbackDirection;

	protected float _knockbackDistance;

	protected float _knockbackProgress;

	protected float _knockbackDuration = 0.1f;

	protected Vector3 _knockbackStartPos;

	protected Vector3 _knockbackTargetPos;

	public virtual float attackPower { get; set; }

	public virtual float defense { get; set; }

	public virtual float attackRange { get; set; }

	public virtual float searchRange { get; set; }

	public virtual float attackSpeed { get; set; }

	public virtual float damageRate { get; set; }

	public virtual float defenceRate { get; set; }

	public virtual float dogeRate { get; set; }

	public virtual float knockbackImpact { get; set; }

	public virtual float playerAtkKnockbackDistance { get; set; }

	public virtual float hittedSlower { get; set; }

	public virtual float dropMiner { get; set; }

	public override bool isKnockedBack
	{
		get
		{
			return _isKnockedBack;
		}
		set
		{
			_isKnockedBack = value;
		}
	}

	public virtual bool IsAlive => health > 0f;

	protected override void Start()
	{
		base.Start();
	}

	protected override void Update()
	{
		base.Update();
		UpdateKnockback();
	}

	public override float TakeDamage(float damage, bool isCritical = false)
	{
		base.TakeDamage(damage, isCritical);
		health -= damage;
		if (health <= 0f)
		{
			Die();
		}
		return 0f;
	}

	public virtual void Attack(CombatEntityUI target)
	{
		if ((Object)(object)target != (Object)null && target.IsAlive)
		{
			target.TakeDamage(attackPower);
		}
	}

	public virtual void GetKnockedBack(Vector3 sourcePos, float knockbackDistance = 1f)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		if (!_isKnockedBack)
		{
			Vector3 val = sourcePos - ((Component)this).transform.position;
			_knockbackDirection = ((Vector3)(ref val)).normalized;
			_knockbackDuration = ((knockbackDistance < 0.01f) ? 0.01f : 0.1f);
			_knockbackDistance = knockbackDistance;
			_knockbackProgress = 0f;
			_isKnockedBack = true;
			ref Vector3 knockbackStartPos = ref _knockbackStartPos;
			Transform transform = ((Component)this).transform;
			knockbackStartPos = (((Object)(object)((transform is RectTransform) ? transform : null) != (Object)null) ? ((RectTransform)/*isinst with value type is only supported in some contexts*/).anchoredPosition3D : ((Component)this).transform.position);
			_knockbackTargetPos = _knockbackStartPos - _knockbackDirection * _knockbackDistance;
		}
	}

	protected virtual void UpdateKnockback()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (_isKnockedBack)
		{
			_knockbackProgress += Time.deltaTime / _knockbackDuration;
			if (_knockbackProgress >= 1f)
			{
				WarpPosition(_knockbackTargetPos);
				_isKnockedBack = false;
			}
			else
			{
				Vector3 position = Vector3.Lerp(_knockbackStartPos, _knockbackTargetPos, _knockbackProgress);
				WarpPosition(position);
			}
		}
	}
}

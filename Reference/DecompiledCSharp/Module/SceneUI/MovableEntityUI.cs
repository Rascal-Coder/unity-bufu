using UnityEngine;

namespace Module.SceneUI;

public class MovableEntityUI : EntityUI
{
	private bool _isAttacking;

	public virtual float moveSpeed { get; set; }

	public virtual bool isKnockedBack { get; set; }

	public virtual bool isAttacking
	{
		get
		{
			return _isAttacking;
		}
		set
		{
			_isAttacking = value;
		}
	}

	public virtual void SetChaseTarget(Vector2 targetPosition)
	{
	}
}

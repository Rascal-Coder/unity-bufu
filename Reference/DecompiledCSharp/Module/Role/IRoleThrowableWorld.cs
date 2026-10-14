using System.Collections.Generic;
using UnityEngine;

namespace Module.Role;

public interface IRoleThrowableWorld
{
	IRoleThrowableVat FindThrowableForRole(Vector2 rolePosition, int roleType);

	Vector2 GetThrowableLandingPosition(Vector2 startPosition);

	float GetRoleThrowCooldown(int roleType);

	void GetIdleThrowablesInRadius(Vector2 center, float radius, List<IRoleThrowableVat> results);
}

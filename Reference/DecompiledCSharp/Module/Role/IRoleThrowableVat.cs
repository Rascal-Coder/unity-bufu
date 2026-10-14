using UnityEngine;

namespace Module.Role;

public interface IRoleThrowableVat
{
	Transform VatTransform { get; }

	bool IsIdleForRole { get; }

	float RoleThrowDuration { get; }

	bool TryThrowByRole(Vector2 startPosition, Vector2 targetPosition, int roleType);
}

using UnityEngine;

namespace Utility;

public class SpawnUtility
{
	private static Vector2 GetRandomLocalPositionInSprite(SpriteRenderer spriteRenderer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		Bounds bounds = ((Renderer)spriteRenderer).bounds;
		float num = Random.Range(((Bounds)(ref bounds)).min.x, ((Bounds)(ref bounds)).max.x);
		float num2 = Random.Range(((Bounds)(ref bounds)).min.y, ((Bounds)(ref bounds)).max.y);
		return Vector2.op_Implicit(new Vector3(num, num2, 0f));
	}
}

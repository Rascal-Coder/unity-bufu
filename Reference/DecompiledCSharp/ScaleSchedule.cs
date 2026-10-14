using UnityEngine;

public class ScaleSchedule : ScheduleBase
{
	private Transform transform;

	private Vector3 startScale;

	private Vector3 targetScale;

	private float duration;

	private float elapsedTime;

	public ScaleSchedule(Transform transform, Vector3 targetScale, float duration)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		this.transform = transform;
		startScale = transform.localScale;
		this.targetScale = targetScale;
		this.duration = duration;
	}

	public override bool Update(float deltaTime)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)transform == (Object)null)
		{
			return true;
		}
		elapsedTime += deltaTime;
		float num = Mathf.Clamp01(elapsedTime / duration);
		transform.localScale = Vector3.Lerp(startScale, targetScale, num);
		return elapsedTime >= duration;
	}
}

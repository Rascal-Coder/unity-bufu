using UnityEngine;

public class ColorChangeSchedule : ScheduleBase
{
	private Renderer targetRenderer;

	private Color startColor;

	private Color targetColor;

	private float duration;

	private float elapsedTime;

	private int materialIndex;

	public ColorChangeSchedule(Renderer renderer, Color targetColor, float duration, int materialIndex = 0)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		targetRenderer = renderer;
		this.targetColor = targetColor;
		this.duration = ((duration > 0f) ? duration : 0.1f);
		this.materialIndex = materialIndex;
		if ((Object)(object)renderer != (Object)null && renderer.materials.Length > materialIndex)
		{
			startColor = renderer.materials[materialIndex].color;
		}
	}

	public override bool Update(float deltaTime)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)targetRenderer == (Object)null || !targetRenderer.isVisible)
		{
			return true;
		}
		elapsedTime += deltaTime;
		float num = Mathf.Clamp01(elapsedTime / duration);
		Color color = Color.Lerp(startColor, targetColor, num);
		if (targetRenderer.materials.Length > materialIndex)
		{
			targetRenderer.materials[materialIndex].color = color;
		}
		return elapsedTime >= duration;
	}

	public override void OnComplete()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)targetRenderer != (Object)null && targetRenderer.materials.Length > materialIndex)
		{
			targetRenderer.materials[materialIndex].color = targetColor;
		}
		base.OnComplete();
	}
}

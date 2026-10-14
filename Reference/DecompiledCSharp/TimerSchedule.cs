using System;
using UnityEngine;

public class TimerSchedule : ScheduleBase
{
	private float elapsedTime;

	private readonly float duration;

	private readonly Action onTimerComplete;

	private readonly Action<float> onTimerUpdate;

	public float Progress => Mathf.Clamp01(elapsedTime / duration);

	public float RemainingTime => Mathf.Max(0f, duration - elapsedTime);

	public TimerSchedule(float duration, Action onComplete = null, Action<float> onUpdate = null)
	{
		this.duration = ((duration > 0f) ? duration : 0.1f);
		onTimerComplete = onComplete;
		onTimerUpdate = onUpdate;
	}

	public override bool Update(float deltaTime)
	{
		elapsedTime += deltaTime;
		float obj = Mathf.Clamp01(elapsedTime / duration);
		onTimerUpdate?.Invoke(obj);
		if (elapsedTime >= duration)
		{
			onTimerComplete?.Invoke();
			return true;
		}
		return false;
	}
}

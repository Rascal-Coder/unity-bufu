using System;

public abstract class ScheduleBase : ISchedule
{
	public Action OnScheduleComplete;

	public Action OnScheduleCancel;

	public Action OnScheduleStart;

	public abstract bool Update(float deltaTime);

	public virtual void OnStart()
	{
		OnScheduleStart?.Invoke();
	}

	public virtual void OnComplete()
	{
		OnScheduleComplete?.Invoke();
		Cleanup();
	}

	public virtual void OnCancel()
	{
		OnScheduleCancel?.Invoke();
		Cleanup();
	}

	protected virtual void Cleanup()
	{
		OnScheduleComplete = null;
		OnScheduleCancel = null;
		OnScheduleStart = null;
	}
}

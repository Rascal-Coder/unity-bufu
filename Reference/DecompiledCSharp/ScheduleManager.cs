using System.Collections.Generic;

public class ScheduleManager
{
	private readonly List<ISchedule> schedules = new List<ISchedule>();

	private readonly List<ISchedule> schedulesToAdd = new List<ISchedule>();

	private bool isUpdating;

	public int ScheduleCount => schedules.Count + schedulesToAdd.Count;

	public void AddSchedule(ISchedule schedule)
	{
		if (schedule != null)
		{
			if (isUpdating)
			{
				schedulesToAdd.Add(schedule);
				return;
			}
			schedules.Add(schedule);
			schedule.OnStart();
		}
	}

	public void RemoveSchedule(ISchedule schedule)
	{
		if (schedule != null)
		{
			if (schedules.Contains(schedule))
			{
				schedules.Remove(schedule);
				schedule.OnCancel();
			}
			if (schedulesToAdd.Contains(schedule))
			{
				schedulesToAdd.Remove(schedule);
			}
		}
	}

	public void ClearAllSchedules()
	{
		foreach (ISchedule schedule in schedules)
		{
			schedule.OnCancel();
		}
		schedules.Clear();
		schedulesToAdd.Clear();
	}

	public void Update(float deltaTime)
	{
		if (schedulesToAdd.Count > 0)
		{
			foreach (ISchedule item in schedulesToAdd)
			{
				schedules.Add(item);
				item.OnStart();
			}
			schedulesToAdd.Clear();
		}
		isUpdating = true;
		for (int num = schedules.Count - 1; num >= 0; num--)
		{
			ISchedule schedule = schedules[num];
			if (schedule.Update(deltaTime))
			{
				schedules.RemoveAt(num);
				schedule.OnComplete();
			}
		}
		isUpdating = false;
	}

	public bool HasScheduleOfType<T>() where T : ISchedule
	{
		foreach (ISchedule schedule in schedules)
		{
			if (schedule is T)
			{
				return true;
			}
		}
		return false;
	}
}

public interface ISchedule
{
	bool Update(float deltaTime);

	void OnStart();

	void OnComplete();

	void OnCancel();
}

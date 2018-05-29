namespace UnityActivityManager
{
	public interface IActivity<TActivityArgs>
	{
		void Init(TActivityArgs levelName);
	}

	public interface IActivity
	{
		void Init();
	}
}
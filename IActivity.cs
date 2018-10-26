namespace UnityActivityManager
{
	public interface IActivity<TInitArgs>
	{
		void Init(TInitArgs initArgs);
	}

	public interface IActivity
	{
		void Init();
	}
}
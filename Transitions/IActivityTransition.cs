namespace UnityActivityManager.Transitions
{
	public interface IActivityTransition
	{
		void Start(IActivityTransitionController controller);
	}
}
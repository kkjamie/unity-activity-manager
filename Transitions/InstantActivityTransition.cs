namespace UnityActivityManager.Transitions
{
	public class InstantActivityTransition : IActivityTransition
	{
		public void Start(IActivityTransitionController controller)
		{
			controller.EndCurrentActivity();
			controller.StartNewActivity();
		}
	}
}
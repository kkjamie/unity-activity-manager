namespace UnityActivityManager.Transitions
{
	public class LoadingTransition : IActivityTransition
	{
		public void Start(IActivityTransitionController controller)
		{
			controller.SendMessage<ITransitionStartedHandler>(h => h.HandleTransitionStarted());

			controller.EndCurrentActivity();
			controller.StartNewActivity();

			controller.SendMessage<ILoadActivityHandler>(a =>
			{
				a.LoadActivity(() =>
				{
					controller.SendMessage<ITransitionCompleteHandler>(h => h.HandleTransitionComplete());
				});
			});
		}
	}
}
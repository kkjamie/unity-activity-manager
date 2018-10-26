using System;

namespace UnityActivityManager
{
	public interface IActivityTransitionController
	{
		void EndCurrentActivity();
		void StartNewActivity();
		void SendMessage<T>(Action<T> execute);
		void NotifyTransitionComplete();
	}
}
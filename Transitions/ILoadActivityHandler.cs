using System;

namespace UnityActivityManager.Transitions
{
	public interface ILoadActivityHandler
	{
		void LoadActivity(Action onComplete);
	}
}
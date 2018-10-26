using System;

namespace UnityActivityManager
{
	public interface IActivityTransitionController
	{
		/// <summary>
		/// Ends the current Activity.
		/// This will result in the current activity's game object being destroyed.
		/// </summary>
		void EndCurrentActivity();

		/// <summary>
		/// Starts the new activity.
		/// This will result in the creation of a new game object and the new activity component being added
		/// </summary>
		void StartNewActivity();

		/// <summary>
		/// Sends a message to any component attached to the current activity.
		/// Usually this is done by declaring receiver interfaces.
		/// </summary>
		/// <param name="execute">The function that should be called on the target component</param>
		/// <typeparam name="T">The type of the component that will be retreived and used to send the message to</typeparam>
		void SendMessage<T>(Action<T> execute);

		/// <summary>
		/// Used to notify the system that the transition has completed. This needs to be called in every transition.
		/// </summary>
		void NotifyTransitionComplete();
	}
}
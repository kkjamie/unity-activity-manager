using System;
using UnityEngine;

namespace UnityActivityManager
{
	public class ActivityManager : MonoBehaviour
	{
		public static ActivityManager Instance
		{
			get
			{
				if (instance == null) throw new Exception("ActivityManager is null, did you add it to the scene?");
				return instance;
			}
		}
		private static ActivityManager instance;

		private bool transitionInProgress = false;
		private IActivity currentActivity;

		private void Awake()
		{
			if (instance != null) throw new Exception("You cannot have 2 ActivityManagers in your game!");

			instance = this;
			DontDestroyOnLoad(gameObject);
		}

		/// <summary>
		/// Begins transitioning into a new activity
		/// </summary>
		/// <typeparam name="TActivity">The target activity</typeparam>
		public void SwitchActivity<TActivity>()
			where TActivity : Component, IActivity
		{
			SwitchActivityInternal<TActivity>(a => a.Enter());
		}

		/// <summary>
		/// Switches to a new acitivity.
		/// </summary>
		/// <param name="initArgs">The initialization argument</param>
		/// <typeparam name="TActivity">The target activity</typeparam>
		/// <typeparam name="TInitArgs">The type of the initialization argument</typeparam>
		public void SwitchActivity<TActivity, TInitArgs>(TInitArgs initArgs)
			where TActivity : Component, IActivity<TInitArgs>
		{
			SwitchActivityInternal<TActivity>(a => a.Enter(initArgs));
		}

		private void SwitchActivityInternal<TActivity>(Action<TActivity> initActivity)
			where TActivity : Component, IActivity
		{
			if (transitionInProgress)
			{
				throw new Exception("Cannot SwitchActivity, there is an activity transition already in progress.");
			}

			Action<Action> endActivity = onComplete =>
			{
				currentActivity.Exit(() => {
					Destroy(currentActivity.gameObject);
					onComplete();
				});
			};

			Action startActivity = () =>
			{
				var newActivity = new GameObject(typeof(TActivity).Name).AddComponent<TActivity>();
				newActivity.transform.SetParent(transform);
				currentActivity = newActivity;
				initActivity(newActivity);
				transitionInProgress = false;
			};

			transitionInProgress = true;

			endActivity(startActivity);
		}
	}
}
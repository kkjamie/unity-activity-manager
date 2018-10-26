using System;
using UnityActivityManager.Transitions;
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
		private GameObject currentActivity;
		private TransitionController transitionController;

		private void Awake()
		{
			if (instance != null) throw new Exception("You cannot have 2 ActivityManagers in your game!");

			instance = this;
			transitionController = new TransitionController(this);
			DontDestroyOnLoad(gameObject);
		}

		/// <summary>
		/// Switches to a new acitivity, with optional transition.
		/// If no transition is specified then InstantActivityTransition will be used.
		/// This function will result in the target activity's Init() function being called
		/// </summary>
		/// <param name="transition">The transition to use</param>
		/// <typeparam name="TActivity">The target activity</typeparam>
		public void SwitchActivity<TActivity>(IActivityTransition transition = null)
			where TActivity : Component, IActivity
		{
			SwitchActivityInternal<TActivity>(a => a.Init(), transition);
		}

		/// <summary>
		/// Switches to a new acitivity, initialization argument and optional transition.
		/// The initialization argument is strongly typed.
		/// If no transition is specified then InstantActivityTransition will be used.
		/// This function will result in the target activity's Init(TArgs initArgs) function being called with the args passed in here
		/// </summary>
		/// <param name="initArgs">The initialization argument</param>
		/// <param name="transition">The transition to use</param>
		/// <typeparam name="TActivity">The target activity</typeparam>
		/// <typeparam name="TInitArgs">The type of the initialization argument</typeparam>
		public void SwitchActivity<TActivity, TInitArgs>(TInitArgs initArgs, IActivityTransition transition = null)
			where TActivity : Component, IActivity<TInitArgs>
		{
			SwitchActivityInternal<TActivity>(a => a.Init(initArgs), transition);
		}

		private void SwitchActivityInternal<TActivity>(Action<TActivity> initActivity, IActivityTransition transition)
			where TActivity : Component
		{
			if (transitionInProgress)
			{
				throw new Exception("Cannot SwitchActivity, there is an activity transition already in progress.");
			}

			Action startActivity = () =>
			{
				var newActivity = new GameObject(typeof(TActivity).Name).AddComponent<TActivity>();
				newActivity.transform.SetParent(transform);
				currentActivity = newActivity.gameObject;
				initActivity(newActivity);
			};

			if (transition == null)
			{
				transition = new InstantActivityTransition();
			}

			transitionInProgress = true;
			transitionController.StartActivityFunc = startActivity;
			transitionController.OnComplete = () => transitionInProgress = false;
			transition.Start(transitionController);
		}

		private void EndCurrentActivity()
		{
			var oldActivity = currentActivity;
			if (oldActivity != null)
			{
				Destroy(oldActivity);
			}
		}

		private class TransitionController : IActivityTransitionController
		{
			private readonly ActivityManager activityManager;

			public Action StartActivityFunc { private get; set; }
			public Action OnComplete { private get; set; }

			public TransitionController(ActivityManager activityManager)
			{
				this.activityManager = activityManager;
			}

			void IActivityTransitionController.EndCurrentActivity()
			{
				activityManager.EndCurrentActivity();
			}

			void IActivityTransitionController.StartNewActivity()
			{
				StartActivityFunc();
			}

			void IActivityTransitionController.SendMessage<TComponentType>(Action<TComponentType> executeMessage)
			{
				var obj = activityManager.currentActivity.GetComponent<TComponentType>();
				if (obj != null)
				{
					executeMessage(obj);
				}
			}

			public void NotifyTransitionComplete()
			{
				if (OnComplete != null) OnComplete();
			}
		}
	}
}
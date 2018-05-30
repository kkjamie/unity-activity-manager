﻿using System;
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

		private GameObject currentActivity;
		private TransitionController transitionController;

		private void Awake()
		{
			if (instance != null) throw new Exception("You cannot have 2 ActivityManagers in your game!");

			instance = this;
			transitionController = new TransitionController(this);
			DontDestroyOnLoad(gameObject);
		}

		public void SwitchActivity<TActivity>(IActivityTransition transition = null)
			where TActivity : Component, IActivity
		{
			SwitchActivityInternal<TActivity>(a => a.Init(), transition);
		}

		public void SwitchActivity<TActivity, TArgs>(TArgs args, IActivityTransition transition = null)
			where TActivity : Component, IActivity<TArgs>
		{
			SwitchActivityInternal<TActivity>(a => a.Init(args), transition);
		}

		private void SwitchActivityInternal<TActivity>(Action<TActivity> initActivity, IActivityTransition transition)
			where TActivity : Component
		{
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

			transitionController.StartActivityFunc = startActivity;
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

			public TransitionController(ActivityManager activityManager)
			{
				this.activityManager = activityManager;
			}

			public Action StartActivityFunc { get; set; }

			void IActivityTransitionController.EndCurrentActivity()
			{
				activityManager.EndCurrentActivity();
			}

			void IActivityTransitionController.StartNewActivity()
			{
				StartActivityFunc();
			}

			void IActivityTransitionController.SendMessage<TSomeType>(Action<TSomeType> executeMessage)
			{
				var obj = activityManager.currentActivity.GetComponent<TSomeType>();
				if (obj != null)
				{
					executeMessage(obj);
				}
			}
		}
	}
}
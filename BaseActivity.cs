using System;
using UnityEngine;

namespace UnityActivityManager
{
	public abstract class BaseActivity : MonoBehaviour, IActivity
	{
		public virtual void Enter()
		{
		}

		public virtual void Exit(Action onComplete)
		{
			onComplete();
		}
	}
}
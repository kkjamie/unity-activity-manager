using System;
using UnityEngine;

namespace UnityActivityManager
{

	public interface IActivity
	{
		GameObject gameObject { get; }
		void Exit(Action onComplete);
		void Enter();
	}

	public interface IActivity<TInitArgs> : IActivity
	{
		void Enter(TInitArgs initArgs);
	}
}
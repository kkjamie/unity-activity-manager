# unity-activity-manager

## What is it?
Although nothing to do with android's ActivityManager, it solves a similar problem. It helps to manager and control the flow through your game through the various "game-states" or in our case "activities". It supports transitions between them and passing data around in a type safe way. It's really lightweight and pretty flexible. Other systems tend to use `object` as their arg type and require run-time type checking. This offers compile time type safety.

## How to use it.
It's a singleton, or at least a unity style mono-behaviour singleton, you place it in the scene and it will throw if you try to create another. Place it in your startup scene (empty game object and add component `ActivityManager` to it) and then switch between the activities you implement. Activities are components that implement one of the `IActivity` interaces.  When you switch to an activity it will be add to a child object of the activity manager.

### API Basics
There are 2 functions used to switch between activities

First of all

**`void SwitchActivity<TActivity>(IActivityTransition transition = null)`**

It takes a single optional argument of type `IActivityTransition` which is used to control the transition. (more on that later). If we omit this argument the `ActivityManager` uses a built-in transition called `InstantActivityTransition`.

Example:
```c#
ActivityManager.Instance.SwitchActivity<MenuActivity>();
```

Once the has been created it will be initialized using the `IActivity` interace's `void Init()` function.

If you want to pass some data in that function we can instead implement `IActivity<TArgs>` and use the following switch function

**`void SwitchActivity<TActivity, TArgs>(TArgs args, IActivityTransition transition = null)`**

It works in the same way but instead the interface's `void Init(TArgs args)` function will be called with the args you passed in.

#### Multiple Init functions with multiple implementations

There maybe multiple ways that an activity can be entered from with different data types. That's easy. Just implement the interface using several type parameters

```c#
public class GameActivity : MonoBehaviour, 
	IActivity,
	IActivity<string>,
	IActivity<StartGameArgs>
{
	// start level 1
	public void Init(){}

	// start specified levelname
	public void Init(string levelName){}

	// start using StartGameArgs
	public void Init(StartGameArgs args){}
}
```
and depending on how you call `SwitchActivity` it will call the correct function

### Transitions

#### Basics
Both `Switch` functions take an option transition argument. As mentioned before If one is not passed in, the system will use the built in transition `InstantActivityTransition`. As expected this is just an instant transition. It does the following

1. Ends the current activity. (this is done by destroying the game object)
2. Starts the new activity. (This creates a new child object and adds the new activity and calls the `Init` function, with the specified args)

#### Built-in Transitions
We have provided a few transitions as a starting point:

- `InstantAcctivityTransition` - switches instantly
- `LoadingTransition` - switches and tells the new transition to load asyncronously, providing a callback for completion. Also contains messages for start/completion of transitions.

#### Implementing your own transitions
Implementing your own transitions is easy. Just implement the `IActivityTransition` interface. All this interface requires is a function with the signature `void Start(IActivityTransitionController controller)`. The controller passed into your start function is all you need to control the transition. It has 3 functions on it:

**`void EndCurrentActivity`**

Used to end the current activity, destroying the object in the process. This must be called before `StartNewActivity` and can only be called once.

**`void StartNewActivity`**

Used to start the new activity. It creates the object and initializes it using the correct init function depending on the interface used. This cannot be called before `EndCurrentActivity` and can only be called once.

**`void SendMessage<T>(Action<T> execute);`**

Used to send messages to the currency activity. If called before `EndCurrentActivity` it will send messages to the old activity we are transitioning out of. If called after `StartNewActivity` it will send messages to the new activity. T is a component or interface from the activity in question. If the component or interface can be found on the current activity, the `execute` parameter function will be called.

Example:
```c#
// if the activity implements the ITransitionStartedHandler interface they will be notified that we started the transition.
controller.SendMessage<ITransitionStartedHandler>(t => t.HandleTransitionStarted());
```

With these tools it's easy to create any type of transition we need by using messages to communicate with the Activities and change their state so the transition is smooth. We can request the music to fade out or the UI to animate in/out or anything we need, but not make it a hard requirement.

*For reference, it's recommended to look at the built in transitions.*

##### Built in messaging interfaces

We have provided a few interfaces for the most common messaging use cases.

- `ITransitionStartedHandler` - used to notify an activity that the transition has started
- `ITransitionCompleteHandler` - used to notify an activity that the transition has completed
- `ILoadActivityHandler` - used to tell an activity to start loading, and when complete, call the callback so we can continue our transition.

#### Common transition use cases
##### Fading to black

Here we have 2 imaginary functions `FadeIn` and `FadeOut` that would actually perform the animation. In your own implementation you can decide how to acheive this.
```c#
public void Start(IActivityTransitionController controller)
{
	controller.SendMessage<ITransitionStartedHandled>(h => h.HandleTransitionStarted());

	FadeIn(() =>
	{
		controller.EndCurrentActivity();
		controller.StartNewActivity();
		FadeOut(() =>
		{
			controller.SendMessage<ITransitionCompleteHandler>(h => h.HandleTransitionComplete());
		});
	});
}
```
`SendMessage` is used here with `ITransitionStartedHandler` and `ITransitionCompleteHandler` in order to notify the activities that these things are happening. The result is

1. StartTransition and notify old activity that we have started to transition. The old activity can now prepare for the activity to end, we can stop accepting input, fade out the sound etc...
2. Fade to black
3. End activity
4. Start/Init the new activity
5. Fade in from black
6. Inform the new acitvity, that transition has complete. This gives the new activity 2 points in time to start doing things. The first is when it's initialized, it can prepare what it needs and set things up. Finally after the fade is complete it can then begin accepting input, and start simulating etc...

##### Loading
See `LoadingTransition` to view the full source

```c#
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
```

1. Start transition.
2. Inform old activity that we started the transition
3. End the old activity
4. Start thenew activity
5. Tell the new activity to start loading
6. After loading is complete we get our callback, we then inform the new activity that the transition has completed.
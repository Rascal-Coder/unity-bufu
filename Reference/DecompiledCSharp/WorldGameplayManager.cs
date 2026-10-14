using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class WorldGameplayManager
{
	private readonly Dictionary<int, WorldGameplayControllerFactory> _factories = new Dictionary<int, WorldGameplayControllerFactory>();

	private readonly Dictionary<int, IWorldGameplayController> _controllers = new Dictionary<int, IWorldGameplayController>();

	private int _switchVersion;

	public IWorldGameplayController ActiveController { get; private set; }

	public void RegisterFactory(int gameplayType, WorldGameplayControllerFactory factory)
	{
		if (gameplayType <= 0)
		{
			throw new ArgumentOutOfRangeException("gameplayType");
		}
		_factories[gameplayType] = factory ?? throw new ArgumentNullException("factory");
	}

	public void SwitchWorld(WorldGameplayContext context, Action<IWorldGameplayController> completed = null)
	{
		if (context == null)
		{
			Debug.LogError((object)"[WorldGameplayManager] Cannot switch without a world context.");
			completed?.Invoke(null);
			return;
		}
		int requestVersion = ++_switchVersion;
		if (ActiveController != null && ActiveController.CanHandle(context))
		{
			ActiveController.ReloadWorld(context);
			completed?.Invoke(ActiveController);
			return;
		}
		ActiveController?.ExitWorld();
		ActiveController = null;
		if (_controllers.TryGetValue(context.GameplayType, out var value) && value != null && value.CanHandle(context))
		{
			Activate(value, context, requestVersion, completed, reloadCachedController: true);
			return;
		}
		if (!_factories.TryGetValue(context.GameplayType, out var value2))
		{
			Debug.LogError((object)$"[WorldGameplayManager] No controller factory for gameplay type {context.GameplayType}.");
			completed?.Invoke(null);
			return;
		}
		value2(context, delegate(IWorldGameplayController controller)
		{
			if (requestVersion != _switchVersion)
			{
				if (controller != null && controller != ActiveController)
				{
					controller.ExitWorld();
				}
			}
			else if (controller == null || controller.GameplayType != context.GameplayType)
			{
				Debug.LogError((object)$"[WorldGameplayManager] Factory returned an invalid controller for gameplay type {context.GameplayType}.");
				completed?.Invoke(null);
			}
			else
			{
				_controllers[context.GameplayType] = controller;
				Activate(controller, context, requestVersion, completed);
			}
		});
	}

	public void Shutdown()
	{
		_switchVersion++;
		ActiveController?.ExitWorld();
		ActiveController = null;
		_controllers.Clear();
		_factories.Clear();
	}

	private void Activate(IWorldGameplayController controller, WorldGameplayContext context, int requestVersion, Action<IWorldGameplayController> completed, bool reloadCachedController = false)
	{
		if (requestVersion == _switchVersion)
		{
			ActiveController = controller;
			if (reloadCachedController)
			{
				controller.ReloadWorld(context);
			}
			else
			{
				controller.EnterWorld(context);
			}
			completed?.Invoke(controller);
		}
	}
}

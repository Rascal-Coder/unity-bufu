using UnityEngine;

public interface IWorldGameplayController
{
	int GameplayType { get; }

	int WorldId { get; }

	MonoBehaviour ViewBehaviour { get; }

	IMainSceneView LegacyMainSceneView { get; }

	bool CanHandle(WorldGameplayContext context);

	void EnterWorld(WorldGameplayContext context);

	void ExitWorld();

	void ReloadWorld(WorldGameplayContext context);
}

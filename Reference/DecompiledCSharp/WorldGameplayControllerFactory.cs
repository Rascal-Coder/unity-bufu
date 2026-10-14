using System;

public delegate void WorldGameplayControllerFactory(WorldGameplayContext context, Action<IWorldGameplayController> completed);

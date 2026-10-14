using UnityEngine;

public sealed class CoinWorldController : MonoBehaviour, IWorldGameplayController
{
	private MonoBehaviour _viewBehaviour;

	private IMainSceneView _sceneView;

	private string _sceneKey = string.Empty;

	private bool _hasEntered;

	public int GameplayType => 1;

	public int WorldId { get; private set; }

	public MonoBehaviour ViewBehaviour => _viewBehaviour;

	public IMainSceneView LegacyMainSceneView => _sceneView;

	public void BindView(MonoBehaviour viewBehaviour, string sceneKey)
	{
		if (!(viewBehaviour is IMainSceneView sceneView))
		{
			Debug.LogError((object)"[CoinWorldController] The bound view must implement IMainSceneView.");
			return;
		}
		string text = sceneKey ?? string.Empty;
		bool num = (Object)(object)_viewBehaviour != (Object)(object)viewBehaviour || _sceneKey != text;
		_viewBehaviour = viewBehaviour;
		_sceneView = sceneView;
		_sceneKey = text;
		if (num)
		{
			_hasEntered = false;
		}
	}

	public bool CanHandle(WorldGameplayContext context)
	{
		if (context != null && context.GameplayType == GameplayType && (Object)(object)_viewBehaviour != (Object)null && _sceneView != null)
		{
			return _sceneKey == GetSceneKey(context);
		}
		return false;
	}

	public void EnterWorld(WorldGameplayContext context)
	{
		if (CanHandle(context))
		{
			WorldId = context.WorldId;
			((Component)_viewBehaviour).gameObject.SetActive(true);
			bool flag = _hasEntered;
			if (_viewBehaviour is MainSceneVat mainSceneVat)
			{
				mainSceneVat.SetDiceWorldController(null);
				mainSceneVat.SetVatWorldVisible(visible: true);
				flag = mainSceneVat.IsInitialized;
			}
			if (flag)
			{
				ReloadView();
			}
			_hasEntered = true;
		}
	}

	public void ExitWorld()
	{
		_sceneView?.ClearRuntimeContent();
		if (_viewBehaviour is MainSceneVat mainSceneVat)
		{
			mainSceneVat.SetVatWorldVisible(visible: false);
		}
		else
		{
			MonoBehaviour viewBehaviour = _viewBehaviour;
			if (viewBehaviour != null)
			{
				((Component)viewBehaviour).gameObject.SetActive(false);
			}
		}
		WorldId = 0;
	}

	public void ReloadWorld(WorldGameplayContext context)
	{
		if (!CanHandle(context))
		{
			return;
		}
		WorldId = context.WorldId;
		((Component)_viewBehaviour).gameObject.SetActive(true);
		if (_viewBehaviour is MainSceneVat mainSceneVat)
		{
			mainSceneVat.SetDiceWorldController(null);
			mainSceneVat.SetVatWorldVisible(visible: true);
			if (!mainSceneVat.IsInitialized)
			{
				return;
			}
		}
		ReloadView();
		_hasEntered = true;
	}

	public static string GetSceneKey(WorldGameplayContext context)
	{
		return context?.Config?.SceneAssetPath ?? string.Empty;
	}

	private void ReloadView()
	{
		_sceneView?.ClearRuntimeContent();
		_sceneView?.InitializeCoins();
		_sceneView?.InitializeRoles();
	}
}

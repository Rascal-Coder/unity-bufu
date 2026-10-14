using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Module.Facility;
using Module.Invite;
using Module.Preview;
using Module.Rank;
using Module.Shop;
using Module.UI;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using cfg;

public class UIManager : MonoBehaviour
{
	private const string CutsceneTransitionAssetPath = "Assets/GameRes/Role/cutscene.prefab";

	[SerializeField]
	private float cutscenePauseDelaySeconds = 1.5f;

	private static UIManager _instance;

	private Canvas _rootCanvas;

	private Dictionary<UILayer, Transform> _layerNodes = new Dictionary<UILayer, Transform>();

	private Dictionary<string, MonoBehaviour> _openedUI = new Dictionary<string, MonoBehaviour>();

	private Dictionary<string, bool> _loadingDic = new Dictionary<string, bool>();

	private PauseUI _pauseUI;

	private RewardBoxUI rewardBoxUI;

	private UpgradePanel _upgradePanel;

	private ShopTurntablePanel _shopTurntablePanel;

	private const string DefaultShopTurntableAssetPath = "Assets/GameRes/NewUI/Shop/prefabs/shop.prefab";

	private TopPanel _topPanel;

	[SerializeField]
	private bool useVatMainScene = true;

	private const string MainSceneAssetPath = "Assets/GameRes/NewUI/Common/prefabs/MainScene.prefab";

	private const string MainSceneVatAssetPath = "Assets/GameRes/NewUI/Common/prefabs/MainSceneVat.prefab";

	private MainScene mainScene;

	private MainSceneVat mainSceneVat;

	private WorldSelectionPanel worldSelectionPanel;

	private CoinWorldController coinWorldController;

	private DiceWorldController diceWorldController;

	private WorldGameplayManager worldGameplayManager;

	private string loadedMainSceneVatAssetPath;

	private bool worldChangeSubscribed;

	private bool suppressNextWorldChangedRefresh;

	private Dictionary<DialogType, CoinSlotSupportDialog> supportUIs = new Dictionary<DialogType, CoinSlotSupportDialog>();

	private DownItem downItemPanel;

	private FacilityPanel facilityPanel;

	private UseDeskUI useDeskUI;

	private Action<int> _onUseChanged;

	private Action<bool> _onBuyClicked;

	private UseHelperUI useHelperUI;

	private Action<int> _onUpgradeChanged;

	private Action<bool> _onLimitChanged;

	private RankUI rankUI;

	private InviteUI inviteUI;

	private DialogPanel dialogPanel;

	private GoalReachPanel goalReachPanel;

	private CommonDialog commonDialog;

	private CommonDialog gameTargetDialog;

	private ShardNotEnoughDialog shardNotEnoughDialog;

	private GameObject checkInPanel;

	public static UIManager Instance
	{
		get
		{
			if ((Object)(object)_instance == (Object)null)
			{
				Debug.LogError((object)"UIManager instance is not initialized!");
			}
			return _instance;
		}
	}

	public UpgradePanel upgradePanel => _upgradePanel;

	public ShopTurntablePanel shopTurntablePanel => _shopTurntablePanel;

	public TopPanel topPanel => _topPanel;

	public MainScene MainScene => mainScene;

	public MainSceneVat MainSceneVat => mainSceneVat;

	public IMainSceneView ActiveMainScene => worldGameplayManager?.ActiveController?.LegacyMainSceneView;

	public MonoBehaviour ActiveMainSceneBehaviour => worldGameplayManager?.ActiveController?.ViewBehaviour;

	public bool UseVatMainScene
	{
		get
		{
			return useVatMainScene;
		}
		set
		{
			useVatMainScene = value;
		}
	}

	public bool IsVatMainSceneActive
	{
		get
		{
			if ((Object)(object)mainSceneVat != (Object)null)
			{
				return (Object)(object)worldGameplayManager?.ActiveController?.ViewBehaviour == (Object)(object)mainSceneVat;
			}
			return false;
		}
	}

	public static bool HasInstance()
	{
		return (Object)(object)_instance != (Object)null;
	}

	private void Awake()
	{
		if ((Object)(object)_instance != (Object)null && (Object)(object)_instance != (Object)(object)this)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		_instance = this;
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		InitializeLayers();
	}

	private void InitializeLayers()
	{
		GameObject val = GameObject.Find("Canvas");
		if ((Object)(object)val == (Object)null)
		{
			Debug.LogError((object)"场景没有Canvas");
			return;
		}
		_rootCanvas = val.GetComponent<Canvas>();
		foreach (UILayer value in Enum.GetValues(typeof(UILayer)))
		{
			string text = value.ToString();
			Transform val2 = val.transform.Find(text);
			if ((Object)(object)val2 != (Object)null)
			{
				_layerNodes[value] = val2;
				if (value != UILayer.System)
				{
					continue;
				}
				try
				{
					Canvas val3 = ((Component)val2).GetComponent<Canvas>();
					if ((Object)(object)val3 == (Object)null)
					{
						val3 = ((Component)val2).gameObject.AddComponent<Canvas>();
					}
					val3.overrideSorting = true;
					val3.sortingOrder = 100;
					if ((Object)(object)((Component)val2).GetComponent<GraphicRaycaster>() == (Object)null)
					{
						((Component)val2).gameObject.AddComponent<GraphicRaycaster>();
					}
				}
				catch (Exception ex)
				{
					Debug.LogWarning((object)("初始化 UILayer.System Canvas 时发生异常（已安全跳过）: " + ex.Message));
				}
			}
			else
			{
				Debug.LogWarning((object)("在Canvas下未找到名为 " + text + " 的UI层级节点！"));
			}
		}
	}

	public void OpenUI<T>(string uiName, UILayer layer = UILayer.Main, Action<T> onOpen = null, object data = null)
	{
	}

	public string GetUIPathByName(string uiName)
	{
		return uiName ?? "";
	}

	public void CloseUI(string uiName)
	{
	}

	public void HideUI(string uiName)
	{
		if (_openedUI.ContainsKey(uiName))
		{
			((Component)_openedUI[uiName]).gameObject.SetActive(false);
		}
		else
		{
			Debug.LogWarning((object)("Cannot hide UI " + uiName + ": not found!"));
		}
	}

	public void ShowUI(string uiName)
	{
		if (_openedUI.ContainsKey(uiName))
		{
			((Component)_openedUI[uiName]).gameObject.SetActive(true);
		}
		else
		{
			Debug.LogWarning((object)("Cannot show UI " + uiName + ": not found!"));
		}
	}

	public bool IsUIOpened(string uiName)
	{
		return _openedUI.ContainsKey(uiName);
	}

	public List<string> GetAllOpenedUINames()
	{
		return new List<string>(_openedUI.Keys);
	}

	public Transform GetLayerTransform(UILayer layer)
	{
		if (_layerNodes.ContainsKey(layer))
		{
			return _layerNodes[layer];
		}
		return null;
	}

	public void SetUIInLayer(Transform uiBase, UILayer layer)
	{
		if (_layerNodes.ContainsKey(layer))
		{
			((Component)uiBase).transform.SetParent(_layerNodes[layer], false);
		}
	}

	public Canvas GetCanvas()
	{
		return _rootCanvas;
	}

	public void ShowPauseUI()
	{
		if ((Object)(object)_pauseUI == (Object)null)
		{
			LoadPauseUI(delegate
			{
				_pauseUI.Show();
			});
		}
		else
		{
			_pauseUI.Show();
		}
	}

	public void HidePauseUI()
	{
		if ((Object)(object)_pauseUI != (Object)null)
		{
			_pauseUI.Hide();
		}
	}

	private void LoadPauseUI(Action onLoaded)
	{
		string assetPath = "Assets/GameRes/NewUI/Common/prefabs/stopPanel.prefab";
		if (SingletonMono<ResManager>.Instance.IsAssetLoaded(assetPath) == 1)
		{
			onLoaded?.Invoke();
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(assetPath, delegate(GameObject prefab)
		{
			if ((Object)(object)prefab != (Object)null)
			{
				Transform layerTransform = Instance.GetLayerTransform(UILayer.Popup);
				_pauseUI = Object.Instantiate<GameObject>(prefab, layerTransform).GetComponent<PauseUI>();
				onLoaded?.Invoke();
			}
		});
	}

	public void ShowRewardBoxUI(List<double> rewardList, bool isAddItem = false, bool isSuipian = true, string adReason = null, bool noDouble = false)
	{
		if ((Object)(object)rewardBoxUI == (Object)null)
		{
			LoadRewardBoxUI(rewardList, isAddItem, isSuipian, adReason, noDouble);
		}
		else
		{
			rewardBoxUI.ShowRewards(rewardList, isAddItem, isSuipian, adReason, noDouble);
		}
	}

	private void LoadRewardBoxUI(List<double> rewardList, bool isAddItem = false, bool isSuipian = true, string adReason = null, bool noDouble = false)
	{
		string assetPath = "Assets/GameRes/NewUI/Popup/prefabs/congratulationPanel.prefab";
		if (SingletonMono<ResManager>.Instance.IsAssetLoaded(assetPath) == 1)
		{
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(assetPath, delegate(GameObject prefab)
		{
			if ((Object)(object)prefab != (Object)null)
			{
				Transform layerTransform = Instance.GetLayerTransform(UILayer.Popup);
				rewardBoxUI = Object.Instantiate<GameObject>(prefab, layerTransform).GetComponent<RewardBoxUI>();
				ShowRewardBoxUI(rewardList, isAddItem, isSuipian, adReason, noDouble);
			}
		});
	}

	public void ShowUpgradeUI(Action onLoaded = null)
	{
		if ((Object)(object)_upgradePanel == (Object)null)
		{
			LoadUpgradePanel(onLoaded);
			return;
		}
		_upgradePanel.OnOpen();
		onLoaded?.Invoke();
	}

	private void LoadUpgradePanel(Action onLoaded = null)
	{
		string assetPath = "Assets/GameRes/NewUI/Common/prefabs/upgradePanel.prefab";
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(assetPath, delegate(GameObject prefab)
		{
			if ((Object)(object)prefab != (Object)null)
			{
				Transform layerTransform = Instance.GetLayerTransform(UILayer.Main);
				_upgradePanel = Object.Instantiate<GameObject>(prefab, layerTransform).GetComponent<UpgradePanel>();
				ShowUpgradeUI(onLoaded);
			}
		});
	}

	public void ShowShopTurntableUI(Action onLoaded = null)
	{
		ShowShopTurntableUI("Assets/GameRes/NewUI/Shop/prefabs/shop.prefab", onLoaded);
	}

	public void HideShopTurntableUI()
	{
		if ((Object)(object)_shopTurntablePanel != (Object)null)
		{
			_shopTurntablePanel.Close();
		}
	}

	public void ShowShopTurntableUI(string assetPath, Action onLoaded = null)
	{
		if ((Object)(object)_shopTurntablePanel == (Object)null)
		{
			LoadShopTurntableUI(assetPath, onLoaded);
			return;
		}
		((Component)_shopTurntablePanel).gameObject.SetActive(true);
		_shopTurntablePanel.OnOpen();
		onLoaded?.Invoke();
	}

	private void LoadShopTurntableUI(string assetPath, Action onLoaded = null)
	{
		if (string.IsNullOrEmpty(assetPath) || SingletonMono<ResManager>.Instance.IsAssetLoaded(assetPath) == 1)
		{
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(assetPath, delegate(GameObject prefab)
		{
			if ((Object)(object)prefab != (Object)null)
			{
				Transform layerTransform = Instance.GetLayerTransform(UILayer.Main);
				_shopTurntablePanel = Object.Instantiate<GameObject>(prefab, layerTransform).GetComponent<ShopTurntablePanel>();
				((Component)_shopTurntablePanel).transform.SetAsFirstSibling();
				ShowShopTurntableUI(assetPath, onLoaded);
			}
		});
	}

	public void ShowTopBtn(bool show = true)
	{
		_topPanel?.ShowTopBtn(show);
	}

	public void ShowTopUI(Action onLoaded = null)
	{
		if ((Object)(object)_topPanel == (Object)null)
		{
			LoadTopPanel(onLoaded);
			return;
		}
		_topPanel.OnShow();
		onLoaded?.Invoke();
	}

	public void HideTopUI()
	{
		if ((Object)(object)_topPanel != (Object)null)
		{
			_topPanel.OnHide();
		}
	}

	private void LoadTopPanel(Action onLoaded = null)
	{
		string assetPath = "Assets/GameRes/NewUI/Common/prefabs/topPanel.prefab";
		if (SingletonMono<ResManager>.Instance.IsAssetLoaded(assetPath) == 1)
		{
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(assetPath, delegate(GameObject prefab)
		{
			if ((Object)(object)prefab != (Object)null)
			{
				Transform layerTransform = Instance.GetLayerTransform(UILayer.Main);
				_topPanel = Object.Instantiate<GameObject>(prefab, layerTransform).GetComponent<TopPanel>();
				ShowTopUI(onLoaded);
			}
		});
	}

	private void Start()
	{
		EnsureWorldGameplayManager();
		SubscribeWorldChanges();
	}

	private void OnDestroy()
	{
		if (worldChangeSubscribed && (Object)(object)GameDataManager.Instance != (Object)null)
		{
			GameDataManager.Instance.OnWorldChanged -= HandleWorldChanged;
		}
		worldGameplayManager?.Shutdown();
	}

	private void SubscribeWorldChanges()
	{
		if (!worldChangeSubscribed && !((Object)(object)GameDataManager.Instance == (Object)null))
		{
			GameDataManager.Instance.OnWorldChanged += HandleWorldChanged;
			worldChangeSubscribed = true;
		}
	}

	private void HandleWorldChanged(int previousWorldId, int currentWorldId)
	{
		if (suppressNextWorldChangedRefresh)
		{
			suppressNextWorldChangedRefresh = false;
			return;
		}
		PlayCutsceneTransition(delegate(Action onCovered)
		{
			RefreshActiveWorld(onCovered);
		});
	}

	public void SuppressNextWorldChangedRefresh()
	{
		suppressNextWorldChangedRefresh = true;
	}

	public void PlayCutsceneTransition(Action onComplete = null)
	{
		PlayCutsceneTransition(null, onComplete);
	}

	public void PlayCutsceneTransition(Action<Action> onCovered, Action onComplete = null)
	{
		bool callbackInvoked = false;
		bool coverCallbackStarted = false;
		bool coverCallbackFinished = onCovered == null;
		bool animationFinished = false;
		GameObject transition = null;
		SkeletonGraphic transitionSkeleton = null;
		float previousAnimationTimeScale = 1f;
		bool animationPausedForSwitch = false;
		TrackEntryDelegate val2 = default(TrackEntryDelegate);
		TrackEntryDelegate val5 = default(TrackEntryDelegate);
		SingletonMono<ResManager>.Instance.LoadPrefabAsync("Assets/GameRes/Role/cutscene.prefab", delegate(GameObject prefab)
		{
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0163: Unknown result type (might be due to invalid IL or missing references)
			//IL_0165: Expected O, but got Unknown
			//IL_016a: Expected O, but got Unknown
			//IL_018d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0192: Unknown result type (might be due to invalid IL or missing references)
			//IL_0194: Expected O, but got Unknown
			//IL_0199: Expected O, but got Unknown
			Transform val = ActiveMainScene?.GetParticlePool();
			if ((Object)(object)prefab == (Object)null || (Object)(object)val == (Object)null)
			{
				Debug.LogWarning((object)"[UIManager] Unable to play cutscene transition: prefab or transition parent is missing.");
				InvokeCoverCallback();
				animationFinished = true;
				TryDestroyTransition();
			}
			else
			{
				transition = Object.Instantiate<GameObject>(prefab, val);
				transition.transform.localPosition = Vector3.zero;
				transition.transform.SetAsLastSibling();
				transitionSkeleton = transition.GetComponent<SkeletonGraphic>();
				if ((Object)(object)transitionSkeleton == (Object)null || transitionSkeleton.AnimationState == null)
				{
					Debug.LogWarning((object)"[UIManager] Cutscene transition has no initialized SkeletonGraphic.");
					InvokeCoverCallback();
					animationFinished = true;
					TryDestroyTransition();
				}
				else
				{
					transitionSkeleton.AnimationState.TimeScale = 1f;
					transitionSkeleton.AnimationState.SetAnimation(0, "guochang", false);
					if (onCovered != null)
					{
						((MonoBehaviour)this).StartCoroutine(PauseCutsceneAndInvokeCoverCallbackAfterDelay(transitionSkeleton, delegate(float timeScale)
						{
							previousAnimationTimeScale = timeScale;
							animationPausedForSwitch = true;
						}, InvokeCoverCallback));
					}
					AnimationState animationState = transitionSkeleton.AnimationState;
					TrackEntryDelegate obj = val2;
					if (obj == null)
					{
						TrackEntryDelegate val3 = delegate
						{
							animationFinished = true;
							TryDestroyTransition();
						};
						TrackEntryDelegate val4 = val3;
						val2 = val3;
						obj = val4;
					}
					animationState.Complete += obj;
					AnimationState animationState2 = transitionSkeleton.AnimationState;
					TrackEntryDelegate obj2 = val5;
					if (obj2 == null)
					{
						TrackEntryDelegate val6 = delegate
						{
							animationFinished = true;
							TryDestroyTransition();
						};
						TrackEntryDelegate val4 = val6;
						val5 = val6;
						obj2 = val4;
					}
					animationState2.End += obj2;
				}
			}
		});
		void Complete()
		{
			if (!callbackInvoked)
			{
				callbackInvoked = true;
				onComplete?.Invoke();
			}
		}
		void InvokeCoverCallback()
		{
			bool doneInvoked;
			if (!coverCallbackStarted)
			{
				coverCallbackStarted = true;
				if (onCovered != null)
				{
					doneInvoked = false;
					try
					{
						onCovered(CoveredWorkDone);
						return;
					}
					catch (Exception ex)
					{
						Debug.LogError((object)("[UIManager] Cutscene covered callback failed: " + ex.Message + "\n" + ex.StackTrace));
						CoveredWorkDone();
						return;
					}
				}
				coverCallbackFinished = true;
				TryDestroyTransition();
			}
			void CoveredWorkDone()
			{
				if (!doneInvoked)
				{
					doneInvoked = true;
					if (animationPausedForSwitch && (Object)(object)transitionSkeleton != (Object)null && transitionSkeleton.AnimationState != null)
					{
						transitionSkeleton.AnimationState.TimeScale = previousAnimationTimeScale;
						animationPausedForSwitch = false;
					}
					coverCallbackFinished = true;
					((MonoBehaviour)this).StartCoroutine(SafetyFinishCutsceneAnimation(1.2f, delegate
					{
						if (!animationFinished)
						{
							Debug.LogWarning((object)"[UIManager] Cutscene animation complete event timed out, completing transition via safety fallback.");
							animationFinished = true;
							TryDestroyTransition();
						}
					}));
					TryDestroyTransition();
				}
			}
		}
		void TryDestroyTransition()
		{
			if (animationFinished && coverCallbackFinished)
			{
				if ((Object)(object)transition != (Object)null)
				{
					Object.Destroy((Object)(object)transition);
				}
				if ((Object)(object)UpgradePanel.Instance != (Object)null)
				{
					UpgradePanel.Instance.RefreshForWorldChange();
				}
				Complete();
			}
		}
	}

	private IEnumerator SafetyFinishCutsceneAnimation(float delaySeconds, Action onTimeout)
	{
		yield return (object)new WaitForSecondsRealtime(delaySeconds);
		onTimeout?.Invoke();
	}

	private IEnumerator PauseCutsceneAndInvokeCoverCallbackAfterDelay(SkeletonGraphic skeleton, Action<float> onPaused, Action callback)
	{
		if (cutscenePauseDelaySeconds > 0f)
		{
			yield return (object)new WaitForSecondsRealtime(cutscenePauseDelaySeconds);
		}
		if ((Object)(object)skeleton != (Object)null && skeleton.AnimationState != null)
		{
			float timeScale = skeleton.AnimationState.TimeScale;
			skeleton.AnimationState.TimeScale = 0f;
			onPaused?.Invoke(timeScale);
		}
		callback?.Invoke();
	}

	public WorldSelectionPanel GetWorldSelectionPanel()
	{
		return worldSelectionPanel;
	}

	public void ShowWorldUI(Action onLoaded = null)
	{
		if (!CanOpenWorldFeature())
		{
			int num = (((Object)(object)GameDataManager.Instance != (Object)null) ? GameDataManager.Instance.GetWorldEntryUnlockLevel() : 10);
			BroadcastManager.ShowPopUpTip($"主线达到{num}级解锁世界功能");
		}
		else if ((Object)(object)worldSelectionPanel == (Object)null)
		{
			LoadWorldSelectionPanel(onLoaded);
		}
		else
		{
			worldSelectionPanel.Open();
			onLoaded?.Invoke();
		}
	}

	private void LoadWorldSelectionPanel(Action onLoaded = null)
	{
		string assetPath = "Assets/GameRes/NewUI/world/prefabs/worldSelectionPanel.prefab";
		if (SingletonMono<ResManager>.Instance.IsAssetLoaded(assetPath) == 1)
		{
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(assetPath, delegate(GameObject prefab)
		{
			if ((Object)(object)prefab != (Object)null)
			{
				Transform layerTransform = Instance.GetLayerTransform(UILayer.Main);
				worldSelectionPanel = Object.Instantiate<GameObject>(prefab, layerTransform).GetComponent<WorldSelectionPanel>();
				((Component)worldSelectionPanel).transform.SetAsFirstSibling();
				ShowWorldUI(onLoaded);
			}
		});
	}

	public void HideWorldUI()
	{
		if ((Object)(object)worldSelectionPanel != (Object)null)
		{
			worldSelectionPanel.Close();
		}
	}

	public void RefreshActiveWorld(Action onActivated = null)
	{
		if (!ConfigManager.HasInstance() || (Object)(object)GameDataManager.Instance == (Object)null)
		{
			onActivated?.Invoke();
			return;
		}
		world worldConfig = ConfigManager.Instance.GetWorldConfig(GameDataManager.Instance.GetCurrentWorldId());
		if (worldConfig == null)
		{
			Debug.LogError((object)$"[UIManager] Missing world config for world {GameDataManager.Instance.GetCurrentWorldId()}.");
			onActivated?.Invoke();
			return;
		}
		EnsureWorldGameplayManager();
		WorldGameplayContext context = new WorldGameplayContext(worldConfig, GameDataManager.Instance.GetCurrentWorldData());
		worldGameplayManager.SwitchWorld(context, delegate(IWorldGameplayController controller)
		{
			if (controller == null)
			{
				Debug.LogError((object)$"[UIManager] Failed to activate gameplay for world {context.WorldId}.");
			}
			_upgradePanel?.OnOpen();
			if ((Object)(object)facilityPanel != (Object)null && ((Component)facilityPanel).gameObject.activeInHierarchy)
			{
				facilityPanel.RefreshForCurrentWorld();
			}
			Transform layerTransform = GetLayerTransform(UILayer.Main);
			if ((Object)(object)layerTransform != (Object)null)
			{
				((Component)layerTransform).gameObject.SetActive(true);
			}
			Transform layerTransform2 = GetLayerTransform(UILayer.Popup);
			if ((Object)(object)layerTransform2 != (Object)null)
			{
				((Component)layerTransform2).gameObject.SetActive(true);
			}
			if ((Object)(object)_upgradePanel != (Object)null)
			{
				((Component)_upgradePanel).gameObject.SetActive(true);
			}
			ShowTopUI(delegate
			{
				ShowTopBtn();
				ShowDownItem();
			});
			onActivated?.Invoke();
		});
	}

	private bool CanOpenWorldFeature()
	{
		if (!ConfigManager.HasInstance() || (Object)(object)GameDataManager.Instance == (Object)null)
		{
			return false;
		}
		return GameDataManager.Instance.IsWorldEntryUnlocked();
	}

	private void EnsureDiceWorldController()
	{
		if (!((Object)(object)diceWorldController != (Object)null))
		{
			diceWorldController = ((Component)this).gameObject.GetOrAddComponent<DiceWorldController>();
		}
	}

	private void EnsureWorldGameplayManager()
	{
		if (worldGameplayManager == null)
		{
			worldGameplayManager = new WorldGameplayManager();
			worldGameplayManager.RegisterFactory(1, CreateCoinWorldController);
			worldGameplayManager.RegisterFactory(2, CreateDiceWorldController);
		}
	}

	private void CreateDiceWorldController(WorldGameplayContext context, Action<IWorldGameplayController> completed)
	{
		LoadMainSceneVat("Assets/GameRes/NewUI/Common/prefabs/MainSceneVat.prefab", delegate
		{
			if ((Object)(object)mainSceneVat == (Object)null)
			{
				Debug.LogError((object)"[UIManager] Dice world requires MainSceneVat, but it failed to load.");
				completed?.Invoke(null);
			}
			else
			{
				EnsureDiceWorldController();
				diceWorldController.BindView(mainSceneVat);
				completed?.Invoke(diceWorldController);
			}
		});
	}

	private void CreateCoinWorldController(WorldGameplayContext context, Action<IWorldGameplayController> completed)
	{
		string sceneKey = CoinWorldController.GetSceneKey(context);
		if (!useVatMainScene)
		{
			LoadMainScene(delegate
			{
				BindCoinWorldController((MonoBehaviour)(object)mainScene, sceneKey, completed);
			});
			return;
		}
		string assetPath = ((!string.IsNullOrEmpty(context.Config.SceneAssetPath)) ? context.Config.SceneAssetPath : "Assets/GameRes/NewUI/Common/prefabs/MainSceneVat.prefab");
		LoadMainSceneVat(assetPath, delegate
		{
			if ((Object)(object)mainSceneVat != (Object)null)
			{
				BindCoinWorldController((MonoBehaviour)(object)mainSceneVat, sceneKey, completed);
			}
			else
			{
				BindCoinWorldController((MonoBehaviour)(object)mainScene, sceneKey, completed);
			}
		});
	}

	private void BindCoinWorldController(MonoBehaviour sceneView, string sceneKey, Action<IWorldGameplayController> completed)
	{
		if ((Object)(object)sceneView == (Object)null)
		{
			completed?.Invoke(null);
			return;
		}
		if ((Object)(object)coinWorldController == (Object)null)
		{
			coinWorldController = ((Component)this).gameObject.GetOrAddComponent<CoinWorldController>();
		}
		coinWorldController.BindView(sceneView, sceneKey);
		completed?.Invoke(coinWorldController);
	}

	public void ShowMainScene(Action onLoaded = null)
	{
		RefreshActiveWorld(onLoaded);
	}

	private void LoadMainScene(Action onLoaded = null)
	{
		if ((Object)(object)mainScene != (Object)null)
		{
			onLoaded?.Invoke();
			return;
		}
		string assetPath = "Assets/GameRes/NewUI/Common/prefabs/MainScene.prefab";
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(assetPath, delegate(GameObject prefab)
		{
			if ((Object)(object)prefab != (Object)null)
			{
				Transform layerTransform = Instance.GetLayerTransform(UILayer.Background);
				mainScene = Object.Instantiate<GameObject>(prefab, layerTransform).GetComponent<MainScene>();
				onLoaded?.Invoke();
			}
			else
			{
				Debug.LogWarning((object)"[UIManager] MainScene prefab is missing.");
				onLoaded?.Invoke();
			}
		});
	}

	private void LoadMainSceneVat(string assetPath, Action onLoaded = null)
	{
		if ((Object)(object)mainSceneVat != (Object)null && loadedMainSceneVatAssetPath == assetPath)
		{
			onLoaded?.Invoke();
			return;
		}
		if ((Object)(object)mainSceneVat != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)mainSceneVat).gameObject);
			mainSceneVat = null;
			loadedMainSceneVatAssetPath = null;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(assetPath, delegate(GameObject prefab)
		{
			if ((Object)(object)prefab != (Object)null)
			{
				Transform layerTransform = Instance.GetLayerTransform(UILayer.Background);
				GameObject val = Object.Instantiate<GameObject>(prefab, layerTransform);
				mainSceneVat = val.GetComponent<MainSceneVat>();
				if ((Object)(object)mainSceneVat == (Object)null)
				{
					mainSceneVat = val.GetOrAddComponent<MainSceneVat>();
					Debug.LogWarning((object)"[UIManager] MainSceneVat prefab has no MainSceneVat component. Added it at runtime.");
				}
				loadedMainSceneVatAssetPath = assetPath;
				onLoaded?.Invoke();
			}
			else
			{
				Debug.LogWarning((object)"[UIManager] MainSceneVat prefab missing, fallback to MainScene.");
				useVatMainScene = false;
				LoadMainScene(onLoaded);
			}
		});
	}

	public void ShowSupportUI(DialogType dialogType, double amount, Action onComplete = null, Action onClose = null)
	{
		if (!supportUIs.ContainsKey(dialogType))
		{
			LoadSupportUI(dialogType, amount, onComplete, onClose);
		}
		else
		{
			supportUIs[dialogType].OnOpen(dialogType, amount, onComplete, onClose);
		}
	}

	private void LoadSupportUI(DialogType dialogType, double amount, Action onComplete = null, Action onClose = null)
	{
		string assetPath = "Assets/GameRes/NewUI/Ad/perfabs/AdGetMoney.prefab";
		if (dialogType == DialogType.incomeBuff)
		{
			assetPath = "Assets/GameRes/NewUI/Ad/perfabs/AdIncome.prefab";
		}
		if (dialogType == DialogType.speedBuff)
		{
			assetPath = "Assets/GameRes/NewUI/Ad/perfabs/AdSpeed.prefab";
		}
		if (dialogType == DialogType.diamond)
		{
			assetPath = "Assets/GameRes/NewUI/Ad/perfabs/AdGetDiamond.prefab";
		}
		if (dialogType == DialogType.gongfu)
		{
			assetPath = "Assets/GameRes/NewUI/Ad/perfabs/AdKongfu.prefab";
		}
		if (dialogType == DialogType.dashe)
		{
			assetPath = "Assets/GameRes/NewUI/Ad/perfabs/AdDashe.prefab";
		}
		if (dialogType == DialogType.randomMoney)
		{
			assetPath = "Assets/GameRes/NewUI/Ad/perfabs/AdRandomMoney.prefab";
		}
		if (dialogType == DialogType.randomDiamond)
		{
			assetPath = "Assets/GameRes/NewUI/Ad/perfabs/AdRandomDiamond.prefab";
		}
		if (dialogType == DialogType.acutionMoney)
		{
			assetPath = "Assets/GameRes/NewUI/Ad/perfabs/AdGetAuctiontMoney.prefab";
		}
		if (SingletonMono<ResManager>.Instance.IsAssetLoaded(assetPath) == 1)
		{
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(assetPath, delegate(GameObject prefab)
		{
			if ((Object)(object)prefab != (Object)null)
			{
				supportUIs[dialogType] = Object.Instantiate<GameObject>(prefab, GetLayerTransform(UILayer.Popup)).GetComponent<CoinSlotSupportDialog>();
				ShowSupportUI(dialogType, amount, onComplete, onClose);
			}
		});
	}

	public void ShowDownItem()
	{
		if ((Object)(object)downItemPanel == (Object)null)
		{
			LoadDownItem();
		}
		else
		{
			((Component)downItemPanel).gameObject.SetActive(true);
		}
	}

	public void HideDownItem()
	{
		if ((Object)(object)downItemPanel != (Object)null)
		{
			((Component)downItemPanel).gameObject.SetActive(false);
		}
	}

	private void LoadDownItem()
	{
		string assetPath = "Assets/GameRes/NewUI/Common/prefabs/DownPanel.prefab";
		if (SingletonMono<ResManager>.Instance.IsAssetLoaded(assetPath) == 1)
		{
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(assetPath, delegate(GameObject prefab)
		{
			if ((Object)(object)prefab != (Object)null)
			{
				Transform layerTransform = Instance.GetLayerTransform(UILayer.Popup);
				downItemPanel = Object.Instantiate<GameObject>(prefab, layerTransform).GetComponent<DownItem>();
				WorldCupDataManager.Instance.IsOpenTopRedPoint();
				ShowDownItem();
			}
		});
	}

	public DownItem GetDownItem()
	{
		if ((Object)(object)downItemPanel != (Object)null)
		{
			return downItemPanel;
		}
		return null;
	}

	public FacilityPanel GetFacilityPanel()
	{
		return facilityPanel;
	}

	public void ShowFacilityUI(Action onLoaded = null)
	{
		if ((Object)(object)facilityPanel == (Object)null)
		{
			LoadFacilityPanel(onLoaded);
			return;
		}
		facilityPanel.OnOpen();
		onLoaded?.Invoke();
	}

	private void LoadFacilityPanel(Action onLoaded = null)
	{
		string assetPath = "Assets/GameRes/NewUI/Facility/prefabs/facility.prefab";
		if (SingletonMono<ResManager>.Instance.IsAssetLoaded(assetPath) == 1)
		{
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(assetPath, delegate(GameObject prefab)
		{
			if ((Object)(object)prefab != (Object)null)
			{
				Transform layerTransform = Instance.GetLayerTransform(UILayer.Main);
				facilityPanel = Object.Instantiate<GameObject>(prefab, layerTransform).GetComponent<FacilityPanel>();
				((Component)facilityPanel).transform.SetAsFirstSibling();
				ShowFacilityUI(onLoaded);
			}
		});
	}

	public void HideFacilityUI()
	{
		if ((Object)(object)facilityPanel != (Object)null)
		{
			facilityPanel.OnClose();
		}
	}

	public void ShowUseDeskUI(desk deskConfig, Action<int> onUseChanged, Action<bool> onBuyClicked)
	{
		if ((Object)(object)useDeskUI == (Object)null)
		{
			LoadUseDeskUI(deskConfig, onUseChanged, onBuyClicked);
			return;
		}
		UseDeskUI obj = useDeskUI;
		obj.onUseChanged = (Action<int>)Delegate.Remove(obj.onUseChanged, _onUseChanged);
		UseDeskUI obj2 = useDeskUI;
		obj2.onBuyClicked = (Action<bool>)Delegate.Remove(obj2.onBuyClicked, _onBuyClicked);
		UseDeskUI obj3 = useDeskUI;
		obj3.onUseChanged = (Action<int>)Delegate.Combine(obj3.onUseChanged, onUseChanged);
		UseDeskUI obj4 = useDeskUI;
		obj4.onBuyClicked = (Action<bool>)Delegate.Combine(obj4.onBuyClicked, onBuyClicked);
		useDeskUI.Initialize(deskConfig);
		((Component)useDeskUI).gameObject.SetActive(true);
		_onUseChanged = onUseChanged;
		_onBuyClicked = onBuyClicked;
	}

	private void LoadUseDeskUI(desk deskConfig, Action<int> onUseChanged, Action<bool> onBuyClicked)
	{
		string assetPath = "Assets/GameRes/NewUI/Facility/prefabs/deskUpgrade.prefab";
		if (SingletonMono<ResManager>.Instance.IsAssetLoaded(assetPath) == 1)
		{
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(assetPath, delegate(GameObject prefab)
		{
			if ((Object)(object)prefab != (Object)null)
			{
				Transform layerTransform = Instance.GetLayerTransform(UILayer.Main);
				useDeskUI = Object.Instantiate<GameObject>(prefab, layerTransform).GetComponent<UseDeskUI>();
				ShowUseDeskUI(deskConfig, onUseChanged, onBuyClicked);
			}
		});
	}

	public void HideUseDeskUI()
	{
		if ((Object)(object)useDeskUI != (Object)null)
		{
			((Component)useDeskUI).gameObject.SetActive(false);
		}
	}

	public void ShowUseHelperUI(helperRole helperConfig, Action<int> onUpgradeChanged, Action<bool> onLimitChanged)
	{
		if ((Object)(object)useHelperUI == (Object)null)
		{
			LoadUseHelperUI(helperConfig, onUpgradeChanged, onLimitChanged);
			return;
		}
		UseHelperUI obj = useHelperUI;
		obj.onUpgradeChanged = (Action<int>)Delegate.Remove(obj.onUpgradeChanged, _onUpgradeChanged);
		UseHelperUI obj2 = useHelperUI;
		obj2.onLimitChanged = (Action<bool>)Delegate.Remove(obj2.onLimitChanged, _onLimitChanged);
		UseHelperUI obj3 = useHelperUI;
		obj3.onUpgradeChanged = (Action<int>)Delegate.Combine(obj3.onUpgradeChanged, onUpgradeChanged);
		UseHelperUI obj4 = useHelperUI;
		obj4.onLimitChanged = (Action<bool>)Delegate.Combine(obj4.onLimitChanged, onLimitChanged);
		useHelperUI.Initialize(helperConfig);
		((Component)useHelperUI).gameObject.SetActive(true);
		_onUpgradeChanged = onUpgradeChanged;
		_onLimitChanged = onLimitChanged;
	}

	private void LoadUseHelperUI(helperRole helperConfig, Action<int> onUpgradeChanged, Action<bool> onLimitChanged)
	{
		string assetPath = "Assets/GameRes/NewUI/Facility/prefabs/helperUpgrade.prefab";
		if (SingletonMono<ResManager>.Instance.IsAssetLoaded(assetPath) == 1)
		{
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(assetPath, delegate(GameObject prefab)
		{
			if ((Object)(object)prefab != (Object)null)
			{
				Transform layerTransform = Instance.GetLayerTransform(UILayer.Main);
				useHelperUI = Object.Instantiate<GameObject>(prefab, layerTransform).GetComponent<UseHelperUI>();
				ShowUseHelperUI(helperConfig, onUpgradeChanged, onLimitChanged);
			}
		});
	}

	public void HideUseHelperUI()
	{
		if ((Object)(object)useHelperUI != (Object)null)
		{
			((Component)useHelperUI).gameObject.SetActive(false);
		}
	}

	public void ShowRankUI()
	{
		if ((Object)(object)rankUI == (Object)null)
		{
			LoadRankUI();
		}
		else
		{
			rankUI.Open();
		}
	}

	private void LoadRankUI()
	{
		string assetPath = "Assets/GameRes/NewUI/Rank/prefabs/RankUI.prefab";
		if (SingletonMono<ResManager>.Instance.IsAssetLoaded(assetPath) == 1)
		{
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(assetPath, delegate(GameObject prefab)
		{
			if ((Object)(object)prefab != (Object)null)
			{
				Transform layerTransform = Instance.GetLayerTransform(UILayer.Popup);
				rankUI = Object.Instantiate<GameObject>(prefab, layerTransform).GetComponent<RankUI>();
				ShowRankUI();
			}
		});
	}

	public void HideRankUI()
	{
		if ((Object)(object)rankUI != (Object)null)
		{
			rankUI.Close();
		}
	}

	public void ShowInviteUI()
	{
		if ((Object)(object)inviteUI == (Object)null)
		{
			LoadInviteUI();
		}
		else
		{
			inviteUI.Open();
		}
	}

	private void LoadInviteUI()
	{
		string assetPath = "Assets/GameRes/NewUI/Ad/perfabs/AdInvite.prefab";
		if (SingletonMono<ResManager>.Instance.IsAssetLoaded(assetPath) == 1)
		{
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(assetPath, delegate(GameObject prefab)
		{
			if ((Object)(object)prefab != (Object)null)
			{
				Transform layerTransform = Instance.GetLayerTransform(UILayer.Popup);
				inviteUI = Object.Instantiate<GameObject>(prefab, layerTransform).GetComponent<InviteUI>();
				ShowInviteUI();
			}
		});
	}

	public void HideInviteUI()
	{
		if ((Object)(object)inviteUI != (Object)null)
		{
			inviteUI.Close();
		}
	}

	public void ShowDialogPanel(string dialogText, DialogPosition position = DialogPosition.Center, float alpha = 1f, Action onClick = null)
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Expected O, but got Unknown
		if ((Object)(object)dialogPanel == (Object)null)
		{
			LoadDialogPanel(dialogText, position, alpha, onClick);
			return;
		}
		dialogPanel.SetText(dialogText);
		dialogPanel.SetDialogPosition(position);
		dialogPanel.SetBackgroundAlpha(alpha);
		dialogPanel.Show();
		if (onClick != null)
		{
			Button bgButton = dialogPanel.GetBgButton();
			if ((Object)(object)bgButton != (Object)null)
			{
				((Graphic)((Selectable)bgButton).image).raycastTarget = true;
				((UnityEventBase)bgButton.onClick).RemoveAllListeners();
				((UnityEvent)bgButton.onClick).AddListener((UnityAction)delegate
				{
					onClick();
				});
			}
			else
			{
				((Graphic)((Selectable)bgButton).image).raycastTarget = false;
			}
		}
		else
		{
			Button bgButton2 = dialogPanel.GetBgButton();
			if ((Object)(object)bgButton2 != (Object)null)
			{
				((Graphic)((Selectable)bgButton2).image).raycastTarget = false;
			}
		}
	}

	private void LoadDialogPanel(string dialogText, DialogPosition position, float alpha = 1f, Action onClick = null)
	{
		string assetPath = "Assets/GameRes/NewUI/Common/prefabs/dialogPanel.prefab";
		if (SingletonMono<ResManager>.Instance.IsAssetLoaded(assetPath) == 1)
		{
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(assetPath, delegate(GameObject prefab)
		{
			if ((Object)(object)prefab != (Object)null)
			{
				Transform layerTransform = Instance.GetLayerTransform(UILayer.Popup);
				dialogPanel = Object.Instantiate<GameObject>(prefab, layerTransform).GetComponent<DialogPanel>();
				ShowDialogPanel(dialogText, position, alpha, onClick);
			}
		});
	}

	public void HideDialogPanel()
	{
		if ((Object)(object)dialogPanel != (Object)null)
		{
			dialogPanel.Hide();
			Button bgButton = dialogPanel.GetBgButton();
			if ((Object)(object)bgButton != (Object)null)
			{
				((UnityEventBase)bgButton.onClick).RemoveAllListeners();
			}
		}
	}

	public void ShowGoalReachPanel(mainMission[] missions, Action onClose = null)
	{
		if ((Object)(object)goalReachPanel == (Object)null)
		{
			LoadGoalReachPanel(missions, onClose);
		}
		else
		{
			goalReachPanel.OnOpen(missions, onClose);
		}
	}

	private void LoadGoalReachPanel(mainMission[] missions, Action onClose = null)
	{
		string assetPath = "Assets/GameRes/NewUI/Popup/prefabs/goalReachPanel.prefab";
		if (SingletonMono<ResManager>.Instance.IsAssetLoaded(assetPath) == 1)
		{
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(assetPath, delegate(GameObject prefab)
		{
			if ((Object)(object)prefab != (Object)null)
			{
				Transform layerTransform = Instance.GetLayerTransform(UILayer.Popup);
				goalReachPanel = Object.Instantiate<GameObject>(prefab, layerTransform).GetComponent<GoalReachPanel>();
				ShowGoalReachPanel(missions, onClose);
			}
		});
	}

	public void ShowCommonDialog(string dialogText, Action onConfirm = null, Action onCancel = null, UILayer layer = UILayer.Popup)
	{
		if ((Object)(object)commonDialog == (Object)null)
		{
			LoadCommonDialog(dialogText, onConfirm, onCancel, layer);
		}
		else
		{
			commonDialog.Show(dialogText, onConfirm, onCancel);
		}
	}

	private void LoadCommonDialog(string dialogText, Action onConfirm = null, Action onCancel = null, UILayer layer = UILayer.Popup)
	{
		string assetPath = "Assets/GameRes/NewUI/Common/prefabs/commonDialog.prefab";
		if (SingletonMono<ResManager>.Instance.IsAssetLoaded(assetPath) == 1)
		{
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(assetPath, delegate(GameObject prefab)
		{
			if ((Object)(object)prefab != (Object)null)
			{
				Transform layerTransform = Instance.GetLayerTransform(layer);
				commonDialog = Object.Instantiate<GameObject>(prefab, layerTransform).GetComponent<CommonDialog>();
				ShowCommonDialog(dialogText, onConfirm, onCancel);
			}
		});
	}

	public void ShowLv8DicePreview()
	{
		string assetPath = "Assets/GameRes/NewUI/Popup/prefabs/lv8Dice.prefab";
		if (SingletonMono<ResManager>.Instance.IsAssetLoaded(assetPath) == 1)
		{
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(assetPath, delegate(GameObject prefab)
		{
			if ((Object)(object)prefab != (Object)null)
			{
				Transform layerTransform = Instance.GetLayerTransform(UILayer.Popup);
				WorldPreviewUI component = Object.Instantiate<GameObject>(prefab, layerTransform).GetComponent<WorldPreviewUI>();
				if ((Object)(object)component != (Object)null)
				{
					component.OnOpen();
				}
			}
		});
	}

	public void HideCommonDialog()
	{
		if ((Object)(object)commonDialog != (Object)null)
		{
			commonDialog.Hide();
		}
	}

	public void ShowGameTargetDialog(string dialogText, Action onConfirm = null, Transform parent = null)
	{
		if ((Object)(object)gameTargetDialog == (Object)null)
		{
			LoadGameTargetDialog(dialogText, onConfirm, parent);
		}
		else
		{
			gameTargetDialog.Show(dialogText, onConfirm, null);
		}
	}

	private void LoadGameTargetDialog(string dialogText, Action onConfirm = null, Transform parent = null)
	{
		string assetPath = "Assets/GameRes/NewUI/video/gameTargetPanel.prefab";
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(assetPath, delegate(GameObject prefab)
		{
			if ((Object)(object)prefab != (Object)null)
			{
				Transform val = (((Object)(object)parent != (Object)null) ? parent : Instance.GetLayerTransform(UILayer.Popup));
				gameTargetDialog = Object.Instantiate<GameObject>(prefab, val).GetComponent<CommonDialog>();
				ShowGameTargetDialog(dialogText, onConfirm, parent);
			}
		});
	}

	public void HideGameTargetDialog()
	{
		if ((Object)(object)gameTargetDialog != (Object)null)
		{
			gameTargetDialog.Hide();
		}
	}

	public void ShowShardNotEnoughDialog(string dialogText, Action onConfirm = null)
	{
		if ((Object)(object)shardNotEnoughDialog == (Object)null)
		{
			LoadShardNotEnoughDialog(dialogText, onConfirm);
		}
		else
		{
			shardNotEnoughDialog.Show(dialogText, onConfirm);
		}
	}

	private void LoadShardNotEnoughDialog(string dialogText, Action onConfirm = null)
	{
		string assetPath = "Assets/GameRes/NewUI/Facility/prefabs/notEnough.prefab";
		if (SingletonMono<ResManager>.Instance.IsAssetLoaded(assetPath) == 1)
		{
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(assetPath, delegate(GameObject prefab)
		{
			if ((Object)(object)prefab != (Object)null)
			{
				Transform layerTransform = Instance.GetLayerTransform(UILayer.Popup);
				shardNotEnoughDialog = Object.Instantiate<GameObject>(prefab, layerTransform).GetComponent<ShardNotEnoughDialog>();
				ShowShardNotEnoughDialog(dialogText, onConfirm);
			}
		});
	}

	public void HideShardNotEnoughDialog()
	{
		if ((Object)(object)shardNotEnoughDialog != (Object)null)
		{
			shardNotEnoughDialog.Hide();
		}
	}

	public void ShowCheckInPanel(bool isActive, Vector2 displayPoint)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)checkInPanel == (Object)null)
		{
			LoadCheckInUI(isActive, displayPoint);
			return;
		}
		checkInPanel.SetActive(isActive);
		Transform transform = checkInPanel.transform;
		((RectTransform)((transform is RectTransform) ? transform : null)).anchoredPosition = displayPoint;
		if ((Object)(object)DailyCheckIn.Instance != (Object)null)
		{
			DailyCheckIn.Instance.RefreshUI();
		}
	}

	public void LoadCheckInUI(bool isActive, Vector2 displayPoint)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		string assetPath = "Assets/GameRes/NewUI/CheckIn/prefabs/CheckIn.prefab";
		if (SingletonMono<ResManager>.Instance.IsAssetLoaded(assetPath) == 1)
		{
			return;
		}
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(assetPath, delegate(GameObject go)
		{
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)go == (Object)null))
			{
				Transform layerTransform = Instance.GetLayerTransform(UILayer.Popup);
				checkInPanel = Object.Instantiate<GameObject>(go, layerTransform);
				Transform transform = checkInPanel.transform;
				((RectTransform)((transform is RectTransform) ? transform : null)).anchoredPosition = displayPoint;
				DailyCheckIn component = checkInPanel.GetComponent<DailyCheckIn>();
				if ((Object)(object)component != (Object)null)
				{
					component.ManualInitialize();
				}
				ShowCheckInPanel(isActive, displayPoint);
			}
		});
	}

	public void ShowCoinDimondTween(int value)
	{
		ShowCoinDimondTween(new List<int> { value });
	}

	public void ShowCoinDimondTween(List<int> values)
	{
		if ((Object)(object)topPanel == (Object)null || values == null || values.Count == 0)
		{
			return;
		}
		string text = "";
		text = ((values.Count <= 1) ? $"Assets/GameRes/NewUI/fly/prefabs/{values[0]}.prefab" : "Assets/GameRes/NewUI/fly/prefabs/all.prefab");
		SingletonMono<ResManager>.Instance.LoadPrefabAsync(text, delegate(GameObject gameobj)
		{
			//IL_020b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0212: Unknown result type (might be due to invalid IL or missing references)
			//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_025b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0260: Unknown result type (might be due to invalid IL or missing references)
			//IL_0264: Unknown result type (might be due to invalid IL or missing references)
			//IL_026b: Unknown result type (might be due to invalid IL or missing references)
			//IL_029c: Unknown result type (might be due to invalid IL or missing references)
			//IL_029e: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0385: Unknown result type (might be due to invalid IL or missing references)
			//IL_038f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0394: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_043c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0441: Unknown result type (might be due to invalid IL or missing references)
			//IL_0443: Unknown result type (might be due to invalid IL or missing references)
			//IL_0462: Unknown result type (might be due to invalid IL or missing references)
			//IL_047f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0489: Expected O, but got Unknown
			//IL_03d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d8: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)gameobj == (Object)null))
			{
				List<int> list = new List<int>();
				List<GameObject> list2 = new List<GameObject>();
				foreach (int value in values)
				{
					int num = ((value >= 3) ? 8 : 15);
					int num2 = ((value >= 3) ? 8 : 15);
					int num3 = Random.Range(num, num2 + 1);
					GameObject val = null;
					if (values.Count > 1)
					{
						int num4 = value - 1;
						if (num4 >= 0 && num4 < gameobj.transform.childCount)
						{
							val = ((Component)gameobj.transform.GetChild(num4)).gameObject;
						}
						else
						{
							Transform val2 = gameobj.transform.Find(value.ToString());
							val = ((!((Object)(object)val2 != (Object)null)) ? gameobj : ((Component)val2).gameObject);
						}
					}
					else
					{
						val = gameobj;
					}
					for (int i = 0; i < num3; i++)
					{
						list.Add(value);
						list2.Add(val);
					}
				}
				int count = list.Count;
				bool flag = false;
				foreach (int value2 in values)
				{
					if (value2 >= 3)
					{
						flag = true;
						break;
					}
				}
				float num5 = (flag ? 180f : 240f);
				float num6 = (flag ? 60f : 80f);
				if (values.Count > 1)
				{
					num5 = (flag ? 220f : 280f);
					num6 = (flag ? 40f : 50f);
				}
				List<Vector2> list3 = new List<Vector2>();
				int num7 = count * 50;
				int num8 = 0;
				while (list3.Count < count && num8 < num7)
				{
					num8++;
					Vector2 val3 = Random.insideUnitCircle * num5;
					bool flag2 = true;
					for (int j = 0; j < list3.Count; j++)
					{
						if (Vector2.Distance(list3[j], val3) < num6)
						{
							flag2 = false;
							break;
						}
					}
					if (flag2)
					{
						list3.Add(val3);
					}
				}
				while (list3.Count < count)
				{
					list3.Add(Random.insideUnitCircle * num5);
				}
				Transform layerTransform = Instance.GetLayerTransform(UILayer.Popup);
				Vector3 val6 = default(Vector3);
				Vector3 val7 = default(Vector3);
				for (int k = 0; k < count; k++)
				{
					int num9 = list[k];
					GameObject val4 = list2[k];
					Vector2 val5 = list3[k];
					((Vector3)(ref val6))._002Ector(val5.x, val5.y, 0f);
					GameObject flyItem = Object.Instantiate<GameObject>(val4, layerTransform);
					RectTransform orAddComponent = flyItem.GetOrAddComponent<RectTransform>();
					orAddComponent.anchoredPosition = Vector2.op_Implicit(val6);
					orAddComponent.SetAnchor(new Vector2(0f, 1f), new Vector2(0f, 1f));
					float num10 = ((num9 >= 3) ? Random.Range(0.25f, 0.45f) : Random.Range(0.3f, 0.55f));
					float num11 = ((num9 >= 3) ? Random.Range(0.4f, 0.8f) : Random.Range(0.5f, 0.8f));
					float num12 = Random.Range(0.18f, 0.3f);
					float num13 = Random.Range(0f, 0.2f);
					float num14 = Random.Range(0.35f, 0.55f);
					float num15 = Random.Range(0.55f, 0.85f);
					float num16 = Random.Range(0f, 0.4f);
					flyItem.transform.localScale = new Vector3(num10, num10, 1f);
					Vector3 zero = Vector3.zero;
					zero = ((num9 != 1 && num9 != 3) ? ((TMP_Text)topPanel.diamondNumTmp).transform.position : ((TMP_Text)topPanel.goldNumTmp).transform.position);
					((Vector3)(ref val7))._002Ector(Random.Range(-60f, 60f), Random.Range(80f, 140f), 0f);
					TweenSettingsExtensions.OnComplete<Sequence>(TweenSettingsExtensions.Append(TweenSettingsExtensions.Append(TweenSettingsExtensions.AppendInterval(TweenSettingsExtensions.Append(TweenSettingsExtensions.SetDelay<Sequence>(DOTween.Sequence(), num16), (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale((Transform)(object)orAddComponent, num11, num12), (Ease)27)), num13), (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOMove(((Component)orAddComponent).transform, ((Component)orAddComponent).transform.position + val7, num14, false), (Ease)3)), (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOMove(((Component)orAddComponent).transform, zero, num15, false), (Ease)5)), (TweenCallback)delegate
					{
						Object.Destroy((Object)(object)flyItem);
					});
				}
			}
		});
	}
}
